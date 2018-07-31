' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwEUtils.Core
Imports EwEPlugin
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecopath

    ''' <summary>
    ''' State of the last call to <see cref="cEcopathmodel.Run">cEcopathmodel.Run</see>
    ''' </summary>
    ''' <remarks>This passes out the internal state of an Ecopath run.</remarks>
    Public Enum eEcopathRunState
        ''' <summary>Model has not been run yet.</summary>
        NotRun
        ''' <summary>Model ran with a balanced model. EE in bounds. </summary>
        ValidEE
        ''' <summary>Failed EE out of bounds.</summary>
        InValidEE
        ''' <summary>Failed to find all the parameter. </summary>
        MissingParameter
        ''' <summary>Failed to run because diet matrix does not sum to one. </summary>
        InValidDietMatrix
        ''' <summary>Failed due to invalid initialization of data.  </summary>
        InValidInitialization
        ''' <summary>Failed to run because an error was encountered. </summary>
        [Error]
    End Enum

    ''' <summary>
    ''' Class that Encapsulates the EcoPath Model
    ''' </summary>
    Public Class cEcoPathModel

        Public RunState As eEcopathRunState = eEcopathRunState.NotRun

        ''' <summary>Enumerator indicating which variables to estimate.</summary>
        Private Enum eEstimateTypes As Byte
            ''' <summary>Default value for the <see cref="EstimateWhat">estimation flag</see>, 
            ''' it seems that type EE and 0 are used interchangably.</summary>
            NotSet = 0
            ''' <summary>Estimate EE.</summary>
            EE = NotSet
            ''' <summary>Estimate Biomass Accumulation.</summary>
            BA
            ''' <summary>Estimate migration.</summary>
            Migration
        End Enum

        Private m_Data As cEcopathDataStructures
        Private m_pluginManager As cPluginManager = Nothing
        Private m_bSuppressMsgs As Boolean
        Private m_eEstimType As eEstimateParameterFor

        Private DCNoCyc(,) As Single
        Private CycDC(,) As Single
        Private NumPath() As Long
        Private SumCycDC() As Single
        Private Cons() As Single
        Friend missing(,) As Boolean
        Friend missingDiets(,) As Boolean
        ' Private CheckedMissing As Boolean
        Private EstimateWhat() As eEstimateTypes

        Private m_messages As New List(Of cVariableStatus)

        Private m_coreNotifier As cCore.CoreMessageDelegate
        Private m_msgPub As New cMessagePublisher

        'status flag for the estimation results
        Private m_EstimStatus As eStatusFlags

        Private m_Ecofunctions As cEcoFunctions

        Private InParameterEstimation As Integer 'currently in the parameter estimation loop 0 = false 1 = true
        Private Exit_Sub_Missing_Par As Integer 'exit parameter estimation sub (ParamEstimate(...)) because number of missing parameter > 2 

        Private H() As Double
        Private Y(,) As Double
        Private P() As Double
        Private Q() As Double
        'NoBQB() Acts as a flag for B and QB 
        '1 means B(i) is missing
        '10 means QB(i) is missing
        '11 means both are missing
        Private NoBQB() As Integer
        Private AUL(,) As Single
        Private bDietsModified As Boolean

        Friend m_stanza As cStanzaDatastructures
        Friend m_psd As cPSDDatastructures

        Public Sub New(ByVal EcoFunctions As cEcoFunctions)
            m_eEstimType = eEstimateParameterFor.ParameterEstimation
            m_Ecofunctions = EcoFunctions
            RunState = eEcopathRunState.NotRun
        End Sub

        Public Sub Clear()
            Me.NoBQB = Nothing '(m_Data.NumGroups)
            Me.H = Nothing '(m_Data.NumGroups + 3)
            Me.Y = Nothing '(m_Data.NumGroups, m_Data.NumGroups)
            Me.P = Nothing '(m_Data.NumGroups)
            Me.Q = Nothing '(m_Data.NumGroups + 10)
            Me.AUL = Nothing '(m_Data.NumGroups + 3, m_Data.NumGroups + 3)
        End Sub
        ''' <summary>
        ''' Results of the Parameter Estimation 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Parameter Estimation results are done as a Property instead of the return value so that a plugin can do the estimation  </remarks>
        Public ReadOnly Property EstimationStatus() As eStatusFlags
            Get
                Return m_EstimStatus
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Exposes the MessagePublisher instance so that the core can add message handlers
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Messages() As cMessagePublisher
            Get
                Return m_msgPub
            End Get
        End Property


        ''' <summary>
        ''' Do not send any messages
        ''' </summary>
        ''' <value>True to prevent Ecopath from sending messages, false to allow Ecopath to send messages (which is the default behaviour).</value>
        ''' <returns></returns>
        ''' <remarks>This allows Ecopath to run in a 'Silent' mode</remarks>
        Public Property suppressMessages() As Boolean
            Get
                Return m_bSuppressMsgs
            End Get
            Set(ByVal value As Boolean)
                m_bSuppressMsgs = value
            End Set
        End Property


        Public Property ParameterEstimationType() As eEstimateParameterFor
            Get
                Return m_eEstimType
            End Get
            Set(ByVal value As eEstimateParameterFor)
                m_eEstimType = value
            End Set
        End Property



        ''' <summary>
        ''' Tell the core to send this message
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <remarks>Wraps the delegate instance that is use to notify the core of a message</remarks>
        Private Sub NotifyCore(ByVal msg As cMessage)

            If msg Is Nothing Then Return

            Try
                'messages can be turned off be a user
                'to speed up the running of the model as in the case of the Monte Carlo which run the model multiple times
                'this puts the model into a 'silent' mode
                If Not m_bSuppressMsgs Then
                    m_msgPub.SendMessage(msg)
                End If
            Catch ex As Exception
                cLog.Write("cEcoPathModel.NotifyCore(...) Failed to post message " & msg.ToString())
            End Try

        End Sub


        ''' <summary>
        ''' Get or Set the cEcoPathDatastructures object
        ''' </summary>
        ''' <value></value>
        ''' <remarks>
        ''' This is the wrapper that contains all the underlying data for EcoPath
        ''' </remarks>
        Public Property EcopathData() As cEcopathDataStructures

            Get
                EcopathData = m_Data
            End Get

            Set(ByVal NewParameters As cEcopathDataStructures)
                m_Data = NewParameters
            End Set

        End Property

        ''' <summary>
        ''' Estimate the unknown parameters in EcoPath
        ''' </summary>
        ''' <returns>
        ''' True if successfull 
        ''' False if something went wrong
        ''' </returns>
        ''' <remarks>
        ''' EcoPath must be initialized before this can be called
        ''' see cModelInterface.InitEcoPath(Datasource_filename) for EcoPath initialization.
        ''' Check the EstimationStatus (eStatusFlag) for failure code 
        ''' </remarks>
        Public Function Run() As Boolean
            Dim iParamsEstimated As Integer = eStatusFlags.ErrorEncountered
            Dim msg As cMessage = Nothing

            m_EstimStatus = eStatusFlags.Null
            Me.RunState = eEcopathRunState.NotRun

            'clear out any existing error messages
            m_messages.Clear()

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Paraniod double checking for the release version
            'Is there a valid Ecopath data object. There is no messages for this as it should not happen in the release version. Just write to the log???????
            If m_Data Is Nothing Then
                cLog.Write(Me.ToString + ".Run() EcoPathModel.m_Data is Nothing, Ecopath has not been initialized correctly. Ecopath could not be run.")
                Debug.Assert(Not m_Data Is Nothing, Me.ToString + ".Run() m_data in Nothing. Ecopath could not be run.")
                Me.RunState = eEcopathRunState.InValidInitialization
                Return False
            End If
            'have the parameters been initialized
            If Not m_Data.bInitialized Then
                cLog.Write(Me.ToString + ".Run() EcoPathModel.m_Data has been created but not Initialized. Ecopath could not be run.")
                Debug.Assert(m_Data.bInitialized, Me.ToString + ".Run() EcoPathModel.m_Data has been created but not Initialized..")
                Me.RunState = eEcopathRunState.InValidInitialization
                Return False
            End If
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            'ToDo_jb Ecopath.EstimateParameter this dimensioning should get moved to a separate routine
            ReDim DCNoCyc(m_Data.NumGroups + 1, m_Data.NumGroups + 1)
            ReDim CycDC(m_Data.NumGroups + 1, m_Data.NumGroups + 1)
            ReDim NumPath(m_Data.NumLiving)
            ReDim SumCycDC(m_Data.NumGroups)
            ReDim Cons(m_Data.NumLiving)

            'Dump parameter and dietmatrix to file for debugging
            'Me.dumpInputPars()
            'Me.dumpDietMatrix()

            Me.setEstimateWhat()

            Me.FindMissing()

            Try

                'check that all diet composition DC() sum to 1
                'False flag do NOT to ask user what to do
                If Not checkDietsSumToOne(False) Then
                    Me.RunState = eEcopathRunState.InValidDietMatrix
                    Return False
                End If

                If m_eEstimType = eEstimateParameterFor.ParameterEstimation Then

                    CheckDetritusFate()
                    checkForMissingDetritusBiomass()

                    CheckForImportOnlyGroups()
                    CheckDetritusFateTooBig()
                    CheckDiscardFateZero()
                    CheckQB()

                    CalcNewExportCatch(0)
                    Catch_calculations()
                End If


                Dim bPluginFailed As Boolean = True
                'Ask the plugin manager to try and do the mass balance 
                'if it fails then run the default mass balance 
                If Me.m_pluginManager IsNot Nothing Then
                    If Me.m_pluginManager.MassBalance(m_Data, m_eEstimType, iParamsEstimated) Then
                        m_EstimStatus = DirectCast(iParamsEstimated, eStatusFlags)
                        bPluginFailed = False
                    End If
                End If

                If bPluginFailed Then
                    Me.EstimateParameters(m_eEstimType, m_EstimStatus)
                End If

                If m_EstimStatus = eStatusFlags.OK Then
                    If m_eEstimType = eEstimateParameterFor.ParameterEstimation Then
                        'this code does not run for the sensitivty estimation

                        'parameters successfully estimated
                        CalcTotalPrimProd()
                        CheckIfEstimatesAreZero()
                        EstimEEAgain()
                        EstimateTrophicLevels(m_Data.NumGroups, m_Data.NumLiving, m_Data.PP, m_Data.DC, m_Data.TTLX)
                        DetritusCalculations()
                        Omniv(m_Data.DC, m_Data.TTLX, m_Data.BQB, m_Data.NumGroups)
                        CalcNichePiankaPred()
                        CalcNichePiankaPrey()
                        Chesson()

                        m_Data.onPostEcopathRun(Me.m_Ecofunctions)

                        CheckIfEEsAreOK(bSendMessage:=True)

                    Else

                        EstimEEAgain()

                    End If

                Else

                    'Failed to estimate parameters
                    'post a message if missing parameters
                    If m_EstimStatus = eStatusFlags.MissingParameter Then

                        MissingParameterMessage()
                        Me.RunState = eEcopathRunState.MissingParameter

                    Else 'If ParamsEstimated  = eStatusFlags.ErrorEncountered Then
                        Me.RunState = eEcopathRunState.Error
                        If m_EstimStatus <> eStatusFlags.ErrorEncountered Then
                            System.Console.WriteLine("WARNING: cEcopathModel.Run() may have set EstimationStatus to the wrong value.")
                        End If
                        'WARNING: This assumes that any m_EstimStatus other than eStatusFlags.OK or eStatusFlags.MissingParameter is an Error
                        'So if you mess with m_EstimStatus make sure you have it right
                        msg = New cMessage(My.Resources.CoreMessages.ECOPATH_RUN_ERROR, _
                                            eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Critical, eDataTypes.NotSet)
                        NotifyCore(msg)
                        cLog.Write("cEcopathModel.Run() failed to estimate parameters because of an error.")

                    End If

                    Return False

                End If ' If parmest_returncode = eParmEstimateCodes.Success Then

                '  mParamEstimator.InParameterEstimation = 0

            Catch ex As Exception

                Debug.Assert(False, ex.Message)
                Me.RunState = eEcopathRunState.Error
                msg = New cMessage(My.Resources.CoreMessages.ECOPATH_RUN_ERROR, _
                                    eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Critical, eDataTypes.NotSet)
                NotifyCore(msg)

                cLog.Write(Me.ToString + ".Run() Error during parameter estimation: " & ex.Message)
                Return False

            End Try

            'Finally did the model balance
            Me.RunState = eEcopathRunState.InValidEE
            If Me.CheckIfEEsAreOK(False) Then
                Me.RunState = eEcopathRunState.ValidEE
            End If

            Return True

        End Function

        ''' <summary>
        ''' Set GS Unassimilated Consumption based on the Model Currency Units <see cref="cEcopathDataStructures.ModelUnitCurrency"/>
        ''' </summary>
        ''' <remarks>
        ''' If Currency is Nutrient then GS has a default of (1 - Me.GE(igrp))
        ''' Otherwise it is defined by the user with a default of 0.2.
        ''' </remarks>
        Public Sub onModelUnitCurrencyChanged()

            Try

                If Me.m_Data.areUnitCurrencyNutrients Then
                    'NUTRIENT units set GS to default

                    'GE is not usually supplied by the user so make sure it has some values
                    Me.EstimateGE()

                    'Model Currency Unit are Nutrients 
                    'Set default for GS = (1 - production / consumption) 
                    'In this case GS is not editable
                    For igrp As Integer = 1 To Me.m_Data.NumLiving
                        Me.m_Data.GS(igrp) = CSng(if(Me.m_Data.GE(igrp) > 0, (1 - Me.m_Data.GE(igrp)), -99))
                        If Me.m_Data.PP(igrp) = 1 Then
                            Me.m_Data.GS(igrp) = 0
                        End If
                    Next igrp

                Else
                    'ENERGY Currency Units
                    'Restore GS to its original values 
                    'these could be default or values from the database
                    Array.Copy(Me.m_Data.GSEng, Me.m_Data.GS, Me.m_Data.GS.Length)

                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub

        ''' <summary>
        ''' Set the EstimateWhat(iGroup) in response to user input
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub setEstimateWhat()
            Dim strMsg As String = ""

            ReDim EstimateWhat(m_Data.NumGroups)

            EstimateGE()

            For igrp As Integer = 1 To m_Data.NumGroups
                If m_Data.B(igrp) > 0 And m_Data.PB(igrp) >= 0 And m_Data.EE(igrp) >= 0 Then
                    If m_Data.PP(igrp) = 1 Or m_Data.QB(igrp) >= 0 Then
                        If EstimateWhat(igrp) = eEstimateTypes.EE Then

                            If m_Data.PP(igrp) < 1 Then
                                strMsg = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PROMPT_ESTIMATE_BA_FOR_B_PB_QB_EE, m_Data.GroupName(igrp))
                            Else
                                strMsg = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PROMPT_ESTIMATE_BA_FOR_B_PB_EE, m_Data.GroupName(igrp))
                            End If

                            Dim fbMsg As New cFeedbackMessage(strMsg, eCoreComponentType.EcoPath, eMessageType.Estimate_BA, eMessageImportance.Information, eMessageReplyStyle.YES_NO, eDataTypes.EcoPathGroupInput)
                            fbMsg.Suppressable = True
                            NotifyCore(fbMsg)

                            If fbMsg.Reply = eMessageReply.YES Then
                                EstimateWhat(igrp) = eEstimateTypes.BA
                            ElseIf fbMsg.Reply = eMessageReply.NO Then

                                strMsg = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PROMPT_ESTIMATE_NETMIGRATION, m_Data.GroupName(igrp))
                                fbMsg.Type = eMessageType.Estimate_Net_Migration
                                fbMsg.Message = strMsg
                                NotifyCore(fbMsg)

                                If fbMsg.Reply = eMessageReply.YES Then
                                    EstimateWhat(igrp) = eEstimateTypes.Migration
                                End If

                            End If 'If fbMsg.Reply = eReply.YES Then

                        End If 'If EstimateWhat(igrp) = 0 Then
                    End If ' If m_Data.PP(igrp) = 1 Or m_Data.QB(igrp) >= 0 Then
                End If ' If m_Data.B(igrp) > 0 And m_Data.PB(igrp) >= 0 And m_Data.EE(igrp) >= 0 Then

            Next igrp

        End Sub


        ''' <summary>
        ''' Set all QB values that are CORE_NULL to Zero
        ''' </summary>
        ''' <remarks>QB is used by Ecosim which assumes that null values are zero</remarks>
        Private Sub CheckQB()
            Dim i As Integer
            For i = 1 To m_Data.NumLiving
                If m_Data.QB(i) < 0 And m_Data.PP(i) = 1 Then m_Data.QB(i) = 0
            Next i
        End Sub

        Private Sub EstimEEAgain()             ' Recalculate EE - Ecotrophic Efficiency
            Dim i As Integer
            Dim j As Integer
            Dim Sum As Single
            Dim MM2 As Single
            Dim msg As New cMessage(My.Resources.CoreMessages.ECOPATH_ESTIMATED, _
                                    eMessageType.DataModified, eCoreComponentType.EcoPath, _
                                    eMessageImportance.Information)
            Dim vs As cVariableStatus = Nothing

            For i = 1 To m_Data.NumLiving
                Sum = CSng(if(m_Data.BaBi(i) <> 0 And m_Data.BA(i) = 0, m_Data.BaBi(i), 0))
                Sum = Sum + CSng(if(m_Data.Emig(i) > 0 And m_Data.Emigration(i) = 0, m_Data.Emig(i), 0))
                Sum = CSng(Sum * m_Data.B(i))

                MM2 = 0
                For j = 1 To m_Data.NumLiving
                    If m_Data.QB(j) > 0 Then
                        MM2 = MM2 + m_Data.B(j) * m_Data.QB(j) * m_Data.DC(j, i)
                    End If
                Next j

                'ToDo_jb EstimEEAgain EstimateWhat(i) Is never getting set to anything I need to check this with the EwE5 code
                Select Case EstimateWhat(i)

                    Case eEstimateTypes.EE, _
                         eEstimateTypes.NotSet

                        If m_Data.B(i) > 0 And m_Data.PB(i) > 0 Then
                            '031220VC: modified to incorporate that BioAcc and emigration can be rates
                            If m_Data.StanzaGroup(i) = False Then
                                m_Data.EE(i) = (m_Data.fCatch(i) + Sum + m_Data.BA(i) + m_Data.Emigration(i) - m_Data.Immig(i) + MM2) / (m_Data.B(i) * m_Data.PB(i))
                            Else
                                m_Data.EE(i) = (m_Data.fCatch(i) + Sum + m_Data.Emigration(i) - m_Data.Immig(i) + MM2) / (m_Data.B(i) * m_Data.PB(i))
                            End If
                        End If

                    Case eEstimateTypes.BA    ' Estimate BA

                        Dim sBA As Single = m_Data.B(i) * m_Data.PB(i) * m_Data.EE(i) - m_Data.fCatch(i) - Sum - m_Data.Emigration(i) + m_Data.Immig(i) - MM2
                        ' Is a change?
                        If (sBA <> Me.m_Data.BA(i)) Then
                            ' #Yes: update BA
                            m_Data.BA(i) = sBA
                            ' Send out notification
                            msg.AddVariable(New cVariableStatus(eStatusFlags.CoreHighlight,
                                                cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_ESTIMATED_BA, Me.m_Data.GroupName(i)),
                                                eVarNameFlags.BioAccumOutput, eDataTypes.EcoPathGroupOutput, eCoreComponentType.EcoPath, i))
                        End If

                    Case eEstimateTypes.Migration

                        Sum = CSng(if(m_Data.BaBi(i) <> 0 And m_Data.BA(i) = 0, m_Data.B(i) * m_Data.BaBi(i), 0))
                        Sum = CSng(m_Data.B(i) * m_Data.PB(i) * m_Data.EE(i) - Sum - m_Data.BA(i) - m_Data.fCatch(i) - MM2)
                        If Sum < 0 Then
                            If (m_Data.Immig(i) <> -Sum) Then
                                m_Data.Immig(i) = -Sum
                                msg.AddVariable(New cVariableStatus(eStatusFlags.CoreHighlight, _
                                                    cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_ESTIMATED_IMMIGRATION, Me.m_Data.GroupName(i)), _
                                                    eVarNameFlags.Immig, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                            End If
                        Else
                            If (m_Data.Emigration(i) <> Sum) Then
                                m_Data.Emigration(i) = Sum
                                msg.AddVariable(New cVariableStatus(eStatusFlags.CoreHighlight, _
                                                    cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_ESTIMATED_EMIGRATION, Me.m_Data.GroupName(i)), _
                                                    eVarNameFlags.Emig, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                            End If
                        End If

                End Select
            Next i

            ' Were any variables changed?
            If (msg.Variables.Count > 0) Then
                ' Send
                Me.NotifyCore(msg)
            End If

        End Sub

        Friend Sub DetritusCalculations()

            ' Me.m_Data.setGSForModelCurrency()

            CalcGS_Det_FlowToDet()            'det ij is flow of det from i to j
            Array.Clear(m_Data.DetEaten, 0, m_Data.NumGroups + 1)
            CalcDetEaten()
            CalcFateOfDetritus()
            CalcBAofDetritus()                'BA= Surplus * DF
            CalcEEforDetritus()               'EE=(DetEaten+DetPassedOn)/INputToDet
            CalcExportOfDetritus()            'EX=INputToDet-DetEaten-BA-DetPassedON
            CalcDCofDetritus()

        End Sub

        Private Sub CalcGS_Det_FlowToDet()
            Dim i As Integer, j As Integer
            ' Dim gs As Single

            For i = 0 To m_Data.NumGroups + m_Data.NumFleet
                For j = 1 To m_Data.NumGroups + m_Data.NumFleet
                    m_Data.det(i, j) = 0
                Next j
            Next i

            For i = 1 To m_Data.NumLiving

                m_Data.FlowToDet(i) = 0
                'xxxxxxxxxxxxxxxxxx
                'jb 5-June-2012 Bug fix if the Unit Currency got switched GS was mangled this caused issues when GS was used by other routines
                'Use a local variable for GS
                'gs = m_Data.GS(i)
                'If m_Data.areUnitCurrencyNutrients() Then
                '    'EwE5 Globaly changed GS if Unit Currency was nutrient 
                '    'this causes a bug if the units are switched back
                '    gs = CSng(if(m_Data.GE(i) > 0, (1 - m_Data.GE(i)), -99))
                'End If
                'If m_Data.currUnitIndex = eUnitCurrencyType.Nitrogen Or m_Data.currUnitIndex = eUnitCurrencyType.Phosporous Or m_Data.currUnitIndex = eUnitCurrencyType.CustomNutrient Then
                '    m_Data.GS(i) = CSng(if(m_Data.GE(i) > 0, (1 - m_Data.GE(i)), -99))
                'Else
                '    'modified 053196 eli.
                '    If m_Data.GS(i) > 1 Then m_Data.GS(i) = m_Data.GS(i) / 100
                'End If
                'xxxxxxxxxxxxxxxxxxxxxx

                For j = m_Data.NumLiving + 1 To m_Data.NumGroups
                    m_Data.det(i, j) = m_Data.B(i) * m_Data.PB(i) * (1 - m_Data.EE(i)) * m_Data.DF(i, j - m_Data.NumLiving)
                    'Cont. from dying i-organisms to detritus j

                    m_Data.det(i, j) = m_Data.det(i, j) + m_Data.B(i) * m_Data.QB(i) * m_Data.GS(i) * m_Data.DF(i, j - m_Data.NumLiving)
                    'Cont. from egestion of i to detritus j.

                    m_Data.det(0, j) = m_Data.det(0, j) + m_Data.det(i, j)
                    'Total flow into detritus group j

                    ' Here sum all flows from living groups to each detritus group
                    m_Data.FlowToDet(i) = CSng(m_Data.FlowToDet(i) + m_Data.det(i, j))
                Next j
            Next i      'end for groups

            'Next for fleets
            If m_Data.NumFleet > 0 Then
                For i = 1 To m_Data.NumFleet
                    For j = m_Data.NumLiving + 1 To m_Data.NumGroups
                        m_Data.det(i + m_Data.NumGroups, j) = m_Data.Discard(i, 0) * m_Data.DiscardFate(i, j - m_Data.NumLiving)
                        m_Data.det(0, j) = m_Data.det(0, j) + m_Data.det(i + m_Data.NumGroups, j)
                        m_Data.FlowToDet(m_Data.NumGroups + i) = CSng(m_Data.FlowToDet(m_Data.NumGroups + i) + m_Data.det(i + m_Data.NumGroups, j))
                    Next
                Next
            End If
        End Sub

        Private Sub CalcDetEaten()
            Dim i As Integer, j As Integer

            For i = 1 To m_Data.NumGroups
                For j = m_Data.NumLiving + 1 To m_Data.NumGroups            'Detritus boxes
                    If m_Data.QB(i) > 0 Then
                        m_Data.DetEaten(j) = CSng(m_Data.DetEaten(j) + m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, j))
                    End If
                Next j
            Next i

        End Sub

        Private Sub CalcFateOfDetritus()
            'calculate fate of detritus
            'First sum flow to detritus from import and flow from living groups
            Dim i As Integer, j As Integer, Surplus As Single

            For i = m_Data.NumLiving + 1 To m_Data.NumGroups
                m_Data.InputToDet(i) = CSng(m_Data.DtImp(i) + m_Data.det(0, i))
            Next i

            For i = m_Data.NumLiving + 1 To m_Data.NumGroups
                m_Data.DetPassedOn(i) = 0
                'DetEaten(i) is amount eaten of the group by all consumers
                Surplus = m_Data.InputToDet(i) - m_Data.DetEaten(i) - m_Data.Resp(i)
                If Surplus > 0 Then    'Where do we send the surplus detr. to?
                    For j = m_Data.NumLiving + 1 To m_Data.NumGroups 'recipient
                        If i <> j Then
                            m_Data.InputToDet(j) = m_Data.InputToDet(j) + Surplus * m_Data.DF(i, j - m_Data.NumLiving)
                            m_Data.DetPassedOn(i) = m_Data.DetPassedOn(i) + Surplus * m_Data.DF(i, j - m_Data.NumLiving)
                            m_Data.det(i, j) = Surplus * m_Data.DF(i, j - m_Data.NumLiving)   'Detritus sent from j to i
                        End If
                    Next j
                End If   'Surplus > 0
                m_Data.FlowToDet(i) = m_Data.DetPassedOn(i)
                m_Data.DetPassedProp(i) = CSng(m_Data.DetPassedOn(i) / (m_Data.B(i) + 1.0E-20))
            Next i
        End Sub

        Private Sub CalcBAofDetritus()
            'Calculate fate of detritus as BA biomass accumulation of detritus 
            'for groups that have detritus fate as themselves
            '27 May 2002: VC subtracted Catch from the surplus as Simone had a model where there is a 'catch' of discard,
            'which is subsequently discarded and sent to another detritus group.
            Dim i As Integer, Surplus As Single

            'Debug.Assert(False, "WARNING: Ecopath.CalcBAofDetritus() has been called.")

            For i = m_Data.NumLiving + 1 To m_Data.NumGroups

                'InputToDet(i) is the sum of all detritus flows NOT including immigration, emigration, import or biomass accumulation. 
                'See CalcGS_Det_FlowToDet() and CalcFateOfDetritus()
                'DetEaten(i) is amount eaten of the group by all consumers
                Surplus = m_Data.InputToDet(i) - m_Data.DetEaten(i) - m_Data.fCatch(i)
                'jb Add detritus fate from this group to BA (biomass accumulation)
                'js add to computed BA, not BA input
                ' m_Data.BA(i) = Surplus * m_Data.DF(i, i - m_Data.NumLiving)
                m_Data.BA(i) += Surplus * m_Data.DF(i, i - m_Data.NumLiving)
            Next i

        End Sub

        Private Sub CalcEEforDetritus()

            Dim msg As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing
            Dim str As String = ""

            'Now calculate the EE for each of the detritus groups
            For i As Integer = m_Data.NumLiving + 1 To m_Data.NumGroups
                If m_Data.InputToDet(i) > 0 Then

                    'EE(i) = (DetEaten(i) + DetPassedOn(i)) / InputToDet(i)
                    If m_Data.InputToDet(i) <> m_Data.Resp(i) Then
                        m_Data.EE(i) = m_Data.DetEaten(i) / (m_Data.InputToDet(i) - m_Data.Resp(i))
                    End If

                    If m_Data.Resp(i) >= m_Data.InputToDet(i) Then
                        If msg Is Nothing Then
                            msg = New cMessage(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_RESPLARGERTHANDETIMP, _
                                               eMessageType.RespirationExceeedsDetritus, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                            msg.Suppressable = True
                        End If

                        str = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_RESPLARGERTHANDETIMP_GROUP, Me.m_Data.GroupName(i))
                        vs = New cVariableStatus(eStatusFlags.ErrorEncountered, str, eVarNameFlags.Name, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                        msg.AddVariable(vs)
                    End If

                End If
            Next i

            If (vs IsNot Nothing) Then
                Me.NotifyCore(msg)
            End If
        End Sub

        Private Sub CalcExportOfDetritus()

            'Find export of detritus
            Dim i As Integer, j As Integer, SumDF As Single

            m_Data.Dt = 0          'Total flow to detritus
            For i = m_Data.NumLiving + 1 To m_Data.NumGroups
                SumDF = 0
                For j = m_Data.NumLiving + 1 To m_Data.NumGroups
                    SumDF = SumDF + m_Data.DF(i, j - m_Data.NumLiving)
                Next j
                m_Data.Dt = m_Data.Dt + m_Data.InputToDet(i)
                If SumDF < 1 Then
                    m_Data.Ex(i) = CSng(m_Data.InputToDet(i) - m_Data.DetEaten(i) - m_Data.BA(i) - m_Data.DetPassedOn(i) - m_Data.Resp(i))
                Else
                    m_Data.Ex(i) = 0.0
                End If
            Next i

        End Sub

        Private Sub CalcDCofDetritus()
            Dim i As Integer, j As Integer

            For i = m_Data.NumLiving + 1 To m_Data.NumGroups                    'Diet comp of detr.box(es)
                ' InputToDet(i) gives all flow to detritus, only if positive
                If m_Data.InputToDet(i) > 0 Then
                    'First for the GROUPS
                    For j = 1 To m_Data.NumGroups
                        m_Data.DC(i, j) = CSng(m_Data.det(j, i) / m_Data.InputToDet(i))
                    Next j
                    'Then for IMPORT = DtImp
                    m_Data.DC(i, 0) = m_Data.DtImp(i) / m_Data.InputToDet(i)
                    'Then for FISHERY   VCJan97
                    If m_Data.NumFleet > 0 Then
                        For j = 1 To m_Data.NumFleet
                            m_Data.DCDet(i - m_Data.NumLiving, j) = CSng(m_Data.det(m_Data.NumGroups + j, i) / m_Data.InputToDet(i))
                        Next
                    End If
                End If
            Next i
        End Sub

        '--------------------------------------------------------------------------
        'CheckIfEEsAreOK
        '
        'Inputs:
        '   EE - (global) calculated ecotrophic efficiency array
        '   NumGroups - (global) number of species groups in the model
        '
        'Outputs:
        '   current database updated with input diets set to DC
        '
        'Description:
        'Check whether model is balanced (EE<1) and tell user.
        'Optionallty invoke auto mass balance feature, if user requests.
        '
        'History:
        '   May 2002    P Kavanagh      Modified to include dialog to invoke auto
        '                               mass balance facility
        '                               Also reduce EElimit from 1.005 to 1
        '                               Get rid of DontDisplay flag (unused)
        '--------------------------------------------------------------------------

        Friend Function CheckIfEEsAreOK(bSendMessage As Boolean) As Boolean
            Dim i As Integer
            Dim EEMax As Single
            Dim msg As cMessage
            Dim breturn As Boolean = True

            EEMax = 1

            For i = 1 To m_Data.NumGroups
                'only test for EE > 1
                If m_Data.EE(i) > EEMax Then
                    breturn = False
                    If bSendMessage Then
                        If msg Is Nothing Then
                            msg = New cMessage(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_EE_GENERIC, _
                                                eMessageType.EE, eCoreComponentType.EcoPath, eMessageImportance.Warning, eDataTypes.EcoPathGroupOutput)
                            msg.Suppressable = True
                        End If
                        msg.AddVariable(New cVariableStatus(eStatusFlags.InvalidModelResult, _
                                    cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_EE, Me.m_Data.GroupName(i), m_Data.EE(i)), _
                                    eVarNameFlags.EEOutput, eDataTypes.EcoPathGroupOutput, eCoreComponentType.EcoPath, i))
                    End If
                End If
            Next

            If Not msg Is Nothing Then
                NotifyCore(msg)
            End If

            Return breturn

        End Function



        Private Sub CalcTotalPrimProd()
            Dim i As Integer

            m_Data.PProd = 0  ' Calculated primary production
            For i = 1 To m_Data.NumLiving
                If m_Data.PP(i) > 0 Then m_Data.PProd = CSng(m_Data.PProd + m_Data.PB(i) * m_Data.B(i) * m_Data.PP(i))
            Next i

        End Sub

        Private Sub CheckIfEstimatesAreZero()
            Dim msgPB0 As cMessage = Nothing
            Dim msgQB0 As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing

            For i As Integer = 1 To m_Data.NumLiving
                If m_Data.PP(i) < 1 And (m_Data.PB(i) = 0 Or m_Data.QB(i) = 0) Then
                    If m_Data.PB(i) = 0 Then

                        ' Msg for PB0 not created yet?
                        If msgPB0 Is Nothing Then
                            ' #Not existing, create it
                            msgPB0 = New cMessage(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_PB0_GENERIC, _
                                     eMessageType.InvalidModel_PB0_Generic, eCoreComponentType.EcoPath, eMessageImportance.Warning, _
                                     eDataTypes.EcoPathGroupInput)
                            msgPB0.Suppressable = True
                        End If
                        ' Create variable information for this messages
                        vs = New cVariableStatus(eStatusFlags.InvalidModelResult, _
                                cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_PB0, Me.m_Data.GroupName(i)), _
                                eVarNameFlags.PBInput, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                        ' Add variable info
                        msgPB0.Variables.Add(vs)

                    ElseIf m_Data.QB(i) = 0 Then

                        If msgQB0 Is Nothing Then
                            ' #Not existing, create it
                            msgQB0 = New cMessage(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_QB0_GENERIC, _
                                     eMessageType.InvalidModel_QB0_Generic, eCoreComponentType.EcoPath, eMessageImportance.Warning, _
                                     eDataTypes.EcoPathGroupInput)
                            msgQB0.Suppressable = True
                        End If
                        ' Create variable information for this messages
                        vs = New cVariableStatus(eStatusFlags.InvalidModelResult, _
                                cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_QB0_GENERIC, Me.m_Data.GroupName(i)), _
                                eVarNameFlags.QBInput, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                        ' Add variable info
                        msgQB0.Variables.Add(vs)
                    End If
                End If
            Next

            ' Send messages, if any
            If msgPB0 IsNot Nothing Then Me.m_msgPub.SendMessage(msgPB0)
            If msgQB0 IsNot Nothing Then Me.m_msgPub.SendMessage(msgQB0)
        End Sub

        Private Sub Omniv(ByVal DC(,) As Single, ByVal TTLX() As Single, ByVal BQB() As Single, ByVal NumGroups As Integer)
            Dim i As Integer, S1 As Single, j As Integer

            For i = 1 To NumGroups
                S1 = 0
                BQB(i) = 0
                If TTLX(i) <> 0 Then
                    For j = 1 To NumGroups
                        S1 = S1 + TTLX(j) * DC(i, j)
                    Next j
                    BQB(i) = S1
                End If
                ' Now BQQ(i%) is the average trophic level of the preys (j%) of i%
            Next i

            For i = 1 To NumGroups
                If TTLX(i) <> 0 Then
                    S1 = 0
                    For j = 1 To NumGroups
                        S1 = S1 + CSng(((TTLX(j) - BQB(i)) ^ 2) * DC(i, j))
                    Next j
                    BQB(i) = S1
                End If
            Next i

        End Sub

        Private Sub CalcNewExportCatch(ByVal OneOnly As Integer)
            Dim Group As Integer

            If OneOnly = 0 Then     'Do them all
                For Group = 1 To m_Data.NumGroups 'Step 1
                    UpdateExportCatch(Group)
                Next
            Else        'Really one only
                If OneOnly > 0 And OneOnly <= m_Data.NumGroups Then UpdateExportCatch(OneOnly)
            End If

        End Sub

        Private Sub UpdateExportCatch(ByVal Group As Integer)
            Dim GearCount As Integer
            Dim sumValue As Single
            Dim Sum As Single

            sumValue = 0
            For GearCount = 1 To m_Data.NumFleet ' Step 1
                sumValue = sumValue + CSng((m_Data.Landing(GearCount, Group) + m_Data.Discard(GearCount, Group)))
            Next

            m_Data.fCatch(Group) = sumValue
            m_Data.Ex(Group) = m_Data.fCatch(Group)

            Sum = CSng(if(m_Data.Emig(Group) > 0 And m_Data.Emigration(Group) = 0 And m_Data.B(Group) > 0, m_Data.Emig(Group) * m_Data.B(Group), 0))

            If Group <= m_Data.NumLiving Then
                m_Data.Ex(Group) = m_Data.Ex(Group) - m_Data.Immig(Group) + m_Data.Emigration(Group) + Sum
            End If

        End Sub

        Private Sub Catch_calculations()
            Dim tcost As Single
            Dim value As Single
            Dim i As Integer
            Dim j As Integer
            Dim K As Integer

            Dim msg As cMessage

            m_Data.Landing(0, 0) = 0
            m_Data.Discard(0, 0) = 0
            For j = 1 To m_Data.NumGroups
                m_Data.Landing(0, j) = 0
                m_Data.Discard(0, j) = 0
                m_Data.fCatch(j) = 0
            Next
            For i = 1 To m_Data.NumFleet
                m_Data.Landing(i, 0) = 0
                m_Data.Discard(i, 0) = 0
                For j = 1 To m_Data.NumGroups
                    'mData.fcatch by NumGear, group and total

                    'jb Only include discards that suffer mortality
                    m_Data.fCatch(j) = CSng(m_Data.fCatch(j) + m_Data.Landing(i, j) + (m_Data.Discard(i, j) * m_Data.PropDiscardMort(i, j)))       'by group

                    'mData.Discards by gear, group, and total
                    m_Data.Landing(i, 0) = m_Data.Landing(i, 0) + m_Data.Landing(i, j) 'sum of Landing by gear
                    m_Data.Landing(0, j) = m_Data.Landing(0, j) + m_Data.Landing(i, j) 'sum Landing by group
                    m_Data.Landing(0, 0) = m_Data.Landing(0, 0) + m_Data.Landing(i, j) 'sum all landing by group and gear

                    'Include all discards even those the survive
                    m_Data.Discard(i, 0) = m_Data.Discard(i, 0) + m_Data.Discard(i, j)
                    m_Data.Discard(0, j) = m_Data.Discard(0, j) + m_Data.Discard(i, j)
                    m_Data.Discard(0, 0) = m_Data.Discard(0, 0) + m_Data.Discard(i, j)

                    If (m_Data.Discard(i, j) = 0) Or (m_Data.Landing(i, j) = 0) Then
                        If msg Is Nothing Then
                            msg = New cMessage(My.Resources.CoreMessages.ECOPATH_MISSINGPARAM_CATCH_GENERIC, _
                                    eMessageType.NoCatchForFleet, eCoreComponentType.EcoPath, _
                                    eMessageImportance.Warning, eDataTypes.FleetInput)
                            msg.Suppressable = True
                        End If

                        If m_Data.Landing(i, j) = 0 Then
                            ' Inform core that the sum of landing and discards is missing
                            msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, _
                                    cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_MISSINGPARAM_LANDING, m_Data.FleetName(i), m_Data.GroupName(j)), _
                                    eVarNameFlags.Landings, eDataTypes.FleetInput, eCoreComponentType.EcoPath, i, j))
                        End If
                        If m_Data.Discard(i, j) = 0 Then
                            msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, _
                                    cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_MISSINGPARAM_DISCARD, m_Data.FleetName(i), m_Data.GroupName(j)), _
                                    eVarNameFlags.Discards, eDataTypes.FleetInput, eCoreComponentType.EcoPath, i, j))
                        End If
                    End If

                Next

                'if this group has no catch then tell the user
                'Gather the data for a message
                If (m_Data.Landing(i, 0) + m_Data.Discard(i, 0)) > 0 Then
                    'this has not been tested so stop and see if it works the first time in here
                    ' Debug.Assert(False)
                    msg = Nothing
                End If
            Next

            If Not msg Is Nothing Then
                NotifyCore(msg)
            End If


            'Also calculate the average market value by group  -- average value
            For j = 1 To m_Data.NumGroups
                m_Data.Market(0, j) = 0
                value = 0
                For i = 1 To m_Data.NumFleet

                    If m_Data.Landing(i, j) > 0 Then value = CSng(value + m_Data.Landing(i, j) * m_Data.Market(i, j))
                    m_Data.PropLanded(i, j) = 0
                    m_Data.PropDiscard(i, j) = 0
                    If m_Data.Landing(i, j) + m_Data.Discard(i, j) > 0 Then
                        m_Data.PropLanded(i, j) = CSng(m_Data.Landing(i, j) / (m_Data.Landing(i, j) + m_Data.Discard(i, j)))
                        'PropDiscard() includes discards that survived 
                        m_Data.PropDiscard(i, j) = CSng(m_Data.Discard(i, j) / (m_Data.Landing(i, j) + m_Data.Discard(i, j)))
                    End If

                Next i
                If value > 0 And m_Data.Landing(0, j) > 0 Then m_Data.Market(0, j) = CSng(value / m_Data.Landing(0, j))
                'Calculate proportion mData.Discarded by group
            Next

            'Estimate the value and cost PLUS PROFIT for the fisheries:
            '   Dim ttt As Single
            For i = 1 To m_Data.NumFleet
                value = 0
                For j = 1 To m_Data.NumLiving
                    If m_Data.Landing(i, j) > 0 Then value = CSng(value + m_Data.Landing(i, j) * m_Data.Market(i, j))
                Next
                '       ttt = ttt + value
                'Now knows the value; the costs are known as %. The profit is calculated from:
                'Fixed plus variable cost is summed to give total cost for this gear:
                tcost = m_Data.CostPct(i, eCostIndex.Fixed) + m_Data.CostPct(i, eCostIndex.CUPE) + m_Data.CostPct(i, eCostIndex.Sail) 'this will sum e.g. to 90 = 90% of value, hence:
                'If tcost > 0 Then
                For K = 1 To 3
                    m_Data.cost(i, K) = value * (m_Data.CostPct(i, K) / 100)
                Next
                m_Data.cost(i, 0) = value * (100 - tcost) / 100 'This is the profit
                m_Data.CostPct(i, eCostIndex.Profit) = 100 - tcost
                'End If
            Next

            Dim Code As Integer
            Dim Group As Integer

            'mData.fcatch codes totals are needed for scaling later
            For Group = 1 To m_Data.NumGroups
                For Code = 1 To m_Data.NumCatchCodes
                    m_Data.CatchCode(0, Group) = m_Data.CatchCode(0, Group) + m_Data.CatchCode(Code, Group)
                Next
            Next

            'ttt = 0: For i = 1 To NumGear: For j = 1 To NumLiving: ttt = ttt + mData.landing(i, j) * Market(i, j): Next: Next
        End Sub

        Private Function MissingParameterMessage() As Boolean
            Dim i As Integer
            Dim isMissing As Boolean

            For i = 1 To m_Data.NumLiving
                If m_Data.B(i) <= 0 Or m_Data.PB(i) < 0 Or m_Data.QB(i) < 0 Or m_Data.EE(i) < 0 Or m_Data.BA(i) < 0 Then
                    isMissing = True
                    Exit For
                End If
            Next i

            If isMissing Then

                Dim msg As New cMessage(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_MISSINGGENERIC, _
                                        eMessageType.TooManyMissingParameters, eCoreComponentType.EcoPath, _
                                        eMessageImportance.Warning, eDataTypes.EcoPathGroupInput)
                msg.Suppressable = False

                For i = 1 To m_Data.NumLiving
                    If m_Data.B(i) <= 0 Then msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_B, i), eVarNameFlags.Biomass, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                    If m_Data.PB(i) < 0 Then msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_PB, i), eVarNameFlags.PBInput, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                    If m_Data.QB(i) < 0 Then msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_QB, i), eVarNameFlags.QBInput, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                    If m_Data.EE(i) < 0 Then
                        msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_EE, i), eVarNameFlags.EEInput, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                        If m_Data.BA(i) < 0 Then
                            msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_EE_BA, i), eVarNameFlags.BioAccumInput, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i))
                        End If
                    End If
                Next

                'send the message to the core
                'the core will forward it out to an interface
                NotifyCore(msg)

            End If

            Return True

        End Function

        Private Function FindMissing() As Boolean
            Dim i As Integer

            'jb clear out missing array and recompute it in FindMissing() this does not really need to happen every run
            'only if something changes
            ReDim missing(m_Data.NumGroups, 4)
            'ReDim missingDiets(m_Data.NumGroups, m_Data.NumGroups)

            'jb in Ewe this also included a test for Biomass/Area bh() for detritus groups 
            'the test is only performed once then missing values are let through??????
            'that test has been moved to cCore.checkBiomassForDetritus()

            For i = 1 To m_Data.NumLiving
                If m_Data.QB(i) < 0 And m_Data.PP(i) = 1 Then m_Data.QB(i) = 0
                missing(i, 1) = (m_Data.BH(i) <= 0)
                missing(i, 2) = (m_Data.PB(i) < 0 And m_Data.GE(i) < 0)
                missing(i, 3) = (m_Data.QB(i) < 0 And m_Data.GE(i) < 0)

                'If i = 1 And m_Data.EE(i) > 0 Then
                '    System.Console.WriteLine("EE ")
                'End If

                missing(i, 4) = (m_Data.EE(i) < 0)
                'For j As Integer = 1 To m_Data.NumLiving
                '    missingDiets(i, j) = m_Data.DC(i, j) < 0
                'Next j
            Next i
        End Function

        Private Sub CalcNichePiankaPred()
            Dim i As Integer, j As Integer, K As Integer
            Dim SumP2(m_Data.NumGroups) As Single
            Dim SumHost(m_Data.NumGroups) As Single

            '*** Pianka predator niche overlap - start
            For i = 1 To m_Data.NumLiving
                SumHost(i) = 0
                For j = 1 To m_Data.NumLiving
                    If m_Data.B(j) > 0 And m_Data.QB(j) > 0 And m_Data.DC(j, i) > 0 Then
                        m_Data.Host(i, j) = m_Data.B(j) * m_Data.QB(j) * m_Data.DC(j, i)
                        SumHost(i) = SumHost(i) + m_Data.Host(i, j)
                    Else
                        m_Data.Host(i, j) = 0
                    End If
                Next j
            Next i       'Host(ij) is amount eaten of group i by predator j
            'Here calculated not regarding detritus as a pred.

            For i = 1 To m_Data.NumLiving
                For j = 1 To m_Data.NumLiving
                    m_Data.Hlap(i, j) = 0
                    If SumHost(i) > 0 Then m_Data.Host(i, j) = m_Data.Host(i, j) / SumHost(i)
                Next j
            Next i

            For i = 1 To m_Data.NumLiving
                SumP2(i) = 0
                For j = 1 To m_Data.NumLiving
                    SumP2(i) = CSng(SumP2(i) + m_Data.Host(i, j) ^ 2)
                    For K = 1 To m_Data.NumGroups
                        m_Data.Hlap(i, j) = m_Data.Hlap(i, j) + m_Data.Host(i, K) * m_Data.Host(j, K)
                    Next K
                Next j
            Next i

            For i = 1 To m_Data.NumLiving
                For j = 1 To m_Data.NumLiving
                    If SumP2(i) > 0 And SumP2(j) > 0 Then m_Data.Hlap(i, j) = m_Data.Hlap(i, j) / (SumP2(i) + SumP2(j)) * 2
                Next j
            Next i
        End Sub

        Private Sub CalcNichePiankaPrey()
            Dim i As Integer, j As Integer, K As Integer
            Dim SumP2() As Single
            Dim SumHost() As Single

            ReDim SumP2(m_Data.NumGroups)
            ReDim SumHost(m_Data.NumGroups)

            'estimates the results
            For i = 1 To m_Data.NumGroups
                For j = 1 To m_Data.NumGroups
                    m_Data.Plap(i, j) = 0
                    If m_Data.DC(i, j) > 0 Then SumP2(i) = CSng(SumP2(i) + m_Data.DC(i, j) ^ 2)
                    For K = 1 To m_Data.NumGroups
                        m_Data.Plap(i, j) = m_Data.Plap(i, j) + m_Data.DC(i, K) * m_Data.DC(j, K)
                    Next K
                Next j
            Next i

            For i = 1 To m_Data.NumGroups
                For j = 1 To m_Data.NumGroups
                    If SumP2(i) > 0 And SumP2(j) > 0 Then m_Data.Plap(i, j) = m_Data.Plap(i, j) / (SumP2(i) + SumP2(j)) * 2
                Next j
            Next i
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>This method is borrowed from EwE5 EcoRanger since it is required for
        ''' calculating Ecopath outputs.</remarks>
        Private Sub Chesson()
            Dim LivingBio As Single
            Dim MaxBio As Single

            MaxBio = 0
            LivingBio = 0
            m_Data.SumBio = 0

            For i As Integer = 1 To m_Data.NumLiving
                If m_Data.B(i) > MaxBio Then MaxBio = m_Data.B(i)
                LivingBio = LivingBio + m_Data.B(i)
            Next i
            m_Data.SumBio = LivingBio

            'Will assume that if there is no biomass for a detritus box
            'then the biomass will correspond to the max living biomass
            'divided by the number of detritus boxes. Thus if all detritus
            'biomasses are lacking the total detritus biomass = max living biom.
            For i As Integer = m_Data.NumLiving + 1 To m_Data.NumGroups
                'If m_Data.B(i) < 0 Then
                '    m_Data.B(i) = MaxBio / (m_Data.NumGroups - m_Data.NumLiving)
                'End If

                'jb changed this to not change B() for detritus if no biomass was specified by the user
                'Changing B() for detritus messes up Ecosim
                If m_Data.B(i) < 0 Then
                    m_Data.SumBio = m_Data.SumBio + MaxBio / (m_Data.NumGroups - m_Data.NumLiving)
                Else
                    m_Data.SumBio = m_Data.SumBio + m_Data.B(i)
                End If

            Next i

            For i As Integer = 1 To m_Data.NumGroups               'CALCULATION OF PREFERENCE INDEX
                m_Data.SumR(i) = 0
                For j As Integer = 1 To m_Data.NumGroups           'FOLLOWING CHESSON (1983)
                    m_Data.Alpha(i, j) = 0
                    If m_Data.B(j) > 0 Then
                        m_Data.Alpha(i, j) = m_Data.DC(i, j) / (m_Data.B(j) / m_Data.SumBio)
                    End If
                    m_Data.SumR(i) = m_Data.SumR(i) + m_Data.Alpha(i, j)
                Next j
            Next i

            For i As Integer = 1 To m_Data.NumGroups
                For j As Integer = 1 To m_Data.NumGroups
                    If m_Data.SumR(i) > 0 Then
                        m_Data.Alpha(i, j) = m_Data.Alpha(i, j) / m_Data.SumR(i)
                    End If
                Next j               'THIS ALPHA IS THE SAME AS CHESSONS ALPHA
            Next i

            For i As Integer = 1 To m_Data.NumGroups
                If m_Data.QB(i) > 0 Then
                    For j As Integer = 1 To m_Data.NumGroups
                        m_Data.Alpha(i, j) = (m_Data.NumGroups * m_Data.Alpha(i, j) - 1) / ((m_Data.NumGroups - 2) * m_Data.Alpha(i, j) + 1)
                    Next j
                End If                     'THIS ALPHA EQUALS CHESSONS EPSILON
            Next i
        End Sub

        ''' <summary>
        ''' Warn the user if detritus has no biomass
        ''' </summary>
        ''' <remarks>In EwE5 this was part of FindMissing()</remarks>
        Private Sub checkForMissingDetritusBiomass()
            Dim msg As cMessage = Nothing

            For i As Integer = m_Data.NumLiving + 1 To m_Data.NumGroups
                If m_Data.BHinput(i) < 0 And msg Is Nothing Then
                    msg = New cMessage(My.Resources.CoreMessages.ECOPATH_PROMPT_ENTER_B_BEFORE_PROCEEDING, _
                                eMessageType.InvalidModel_B_Detritus, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                    msg.Suppressable = True
                End If
            Next

            If msg IsNot Nothing Then NotifyCore(msg)

        End Sub

        Private Sub CheckForImportOnlyGroups()

            Dim nFound As Integer = 0
            Dim bImportOnly(m_Data.NumLiving) As Boolean
            Dim msg As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing

            For iGroup As Integer = 1 To m_Data.NumLiving
                If (m_Data.DC(iGroup, 0) > 0.9999999) And (m_Data.PP(iGroup) < 1) Then
                    bImportOnly(iGroup) = True
                    nFound += 1
                Else
                    bImportOnly(iGroup) = False
                End If
            Next

            If (nFound > 0) Then
                msg = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_DIETIMPORTONLY, nFound), _
                                   eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                msg.Suppressable = True

                For iGroup As Integer = 1 To m_Data.NumLiving
                    If bImportOnly(iGroup) Then msg.AddVariable(New cVariableStatus(eStatusFlags.MissingParameter, _
                        cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_DIETIMPORTONLY_GROUP, Me.m_Data.GroupName(iGroup)), _
                        eVarNameFlags.Name, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, iGroup))
                Next
                Me.NotifyCore(msg)
            End If

        End Sub

        Private Sub CheckDetritusFate()
            Dim i As Integer
            Dim j As Integer
            Dim Dummy As Integer
            Dim msg As String = ""
            Dim PFlag As Boolean
            Dim AllOne As Boolean
            Dim SumDF As Single

            Dummy = 0
            AllOne = True
            For j = 1 To m_Data.NumLiving 'row/groups
                PFlag = False
                SumDF = 0
                For i = 1 To m_Data.NumDetrit
                    If m_Data.DF(j, i) > 0 Then PFlag = True
                    SumDF = SumDF + m_Data.DF(j, i)
                Next
                If Not PFlag Then Dummy = Dummy + 1
                If SumDF < 0.99 Then AllOne = False
            Next

            'If Not AllOne Then MsgBox "Detritus fate for one or more living groups sum to less than 1. ", vbInformation + vbOKOnly, "Detritus fate sum to less than 1"
            If Dummy > 5 Then
                'ToDo_jb CheckDetritusFate message

                'msg = "Detritus fate is 0 for " & CStr(Dummy) & " living groups."
                'msg = msg & "This means that detritus from these groups will be exported from the system. "
                'msg = msg & Chr$(13) & Chr$(13) & "Force to 1 to send all detritus to the last detritus group?"
                'If MsgBox(msg, 36) = 6 Then
                '    For j = 1 To NumLiving Step 1 'row/groups
                '        If DF(j, NumDetrit) = 0 Then DF(j, NumDetrit) = 1
                '    Next
                '    DF(NumGroups, NumDetrit) = 0
                '    'Update the database:
                '    If ImportedFlag = False Then SaveDetritusFate()
                'End If
            End If
            'set DF(NumGroups) to 0  to avoid biomass accumulation which screws up EcoSim
            'jb 30-Sept-2013 allow BA for Detritus to make it to Ecosim
            'this would only have set the last detritus group to zero...
            'm_Data.DF(m_Data.NumGroups, m_Data.NumDetrit) = 0
        End Sub

        Private Sub CheckDetritusFateTooBig()

            Dim nFound As Integer = 0
            Dim DFtooBig(m_Data.NumGroups) As Boolean
            Dim SumDF As Single = 0.0!
            Dim str As String = ""
            Dim msg As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing

            For i As Integer = 1 To m_Data.NumGroups
                SumDF = 0.0!
                DFtooBig(i) = False
                For j As Integer = 1 To m_Data.NumDetrit
                    SumDF += m_Data.DF(i, j)
                Next
                ' JS 25Feb10: added grace range to this test
                If Math.Round(SumDF, 4) > 1.0! Then
                    DFtooBig(i) = True
                    nFound += 1
                End If
            Next

            If nFound > 0 Then
                msg = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_DFLARGERTHANONE, nFound), _
                                   eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                msg.Suppressable = True

                For i As Integer = 1 To m_Data.NumGroups
                    If DFtooBig(i) Then
                        str = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_DFLARGERTHANONE_GROUP, Me.m_Data.GroupName(i))
                        vs = New cVariableStatus(eStatusFlags.ErrorEncountered, str, eVarNameFlags.DetritusFate, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                        msg.AddVariable(vs)
                    End If
                Next
                Me.NotifyCore(msg)
            End If

        End Sub

        Public Function IsFished(igrp As Integer) As Boolean
            Return IsLanded(igrp) Or IsDiscarded(igrp)
        End Function

        Public Function IsLanded(igrp As Integer) As Boolean
            ' Unfortunately the 0 item test is only valid if Ecopath has ran
            ' Return (Me.m_Data.Landing(0, igrp) > 0)
            For iFleet As Integer = 1 To Me.m_Data.NumFleet
                If (Me.m_Data.Landing(iFleet, igrp) > 0) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function IsDiscarded(igrp As Integer) As Boolean
            ' Unfortunately the 0 item test is only valid if Ecopath has ran
            ' Return (Me.m_Data.Discard(0, igrp) > 0)
            For iFleet As Integer = 1 To Me.m_Data.NumFleet
                If (Me.m_Data.Discard(iFleet, igrp) > 0) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub CheckDiscardFateZero()
            Dim nFound As Integer
            Dim bNoDiscardFate(m_Data.NumFleet) As Boolean
            Dim bHasDiscards(m_Data.NumFleet) As Boolean
            Dim msg As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing
            Dim str As String = ""
            Dim ig As Integer
            Dim SumDF As Single

            nFound = 0
            For i As Integer = 1 To m_Data.NumGroups
                For j As Integer = 1 To m_Data.NumFleet
                    If m_Data.Discard(j, i) > 0 Then bHasDiscards(j) = True
                Next
            Next

            ig = 0
            For i As Integer = 1 To m_Data.NumFleet
                If bHasDiscards(i) Then
                    SumDF = 0
                    ig += 1
                    For j As Integer = 1 To m_Data.NumDetrit
                        SumDF = SumDF + m_Data.DiscardFate(i, j)
                    Next
                    If SumDF = 0 Then bNoDiscardFate(i) = True : nFound += 1
                End If
            Next

            If nFound = ig And m_Data.NumDetrit = 1 Then
                'If there is only one detritus group, and if all groups with discard lacks detritus fate, then use a default
                For i As Integer = 1 To m_Data.NumFleet
                    m_Data.DiscardFate(i, 1) = 1
                Next
            ElseIf nFound > 0 Then

                str = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_MISSINGDISCARDFATE, nFound)
                msg = New cMessage(str, eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                msg.Suppressable = True

                For i As Integer = 1 To m_Data.NumFleet
                    If bNoDiscardFate(i) Then
                        str = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_MISSINGDISCARDFATE_FLEET, m_Data.FleetName(i))
                        vs = New cVariableStatus(eStatusFlags.MissingParameter, str, eVarNameFlags.DiscardFate, eDataTypes.FleetInput, eCoreComponentType.EcoPath, i)
                        msg.AddVariable(vs)
                    End If
                Next

                Me.NotifyCore(msg)

            End If

        End Sub

        Public Property PluginManager() As cPluginManager
            Get
                Return Me.m_pluginManager
            End Get
            Set(ByVal pm As cPluginManager)
                Me.m_pluginManager = pm
            End Set
        End Property



#Region "Estimate Parameters"

        Private Function EstimateParameters(ByVal EstimateFor As eEstimateParameterFor, ByRef Result As eStatusFlags) As Boolean

            Dim ji As Integer
            'Dim From As Integer
            Dim LoopC As Integer, Pass As Integer, SecLoop As Integer
            Dim noMissing As Integer
            Dim ExitSen As Boolean
            Dim CyclesDone As Boolean
            Dim msg As cMessage = Nothing
            Dim TimesTried As Integer

            RedimVariables()
            'Programmer: Villy Christensen
            'This is the main module for parametrization, i.e. estimation of 'missing parameter'
            'for securing mass balance
            Try

                Exit_Sub_Missing_Par = 1

Start:
                LoopC = 0

LoopCalc:
                LoopC = LoopC + 1
                'exit strategies if the loop has executed to many times
                If LoopC > m_Data.NumGroups + 2 Then

                    If EstimateFor = eEstimateParameterFor.ParameterEstimation Then
                        'FROM ParamEstimate1
                        If LoopC > m_Data.NumGroups + 2 Then
                            If CyclesDone = False Then
                                FindCyclesWhenEstimatingBiomass()
                                CyclesDone = True
                                GoTo Start
                            ElseIf CheckPredatorPreyTrophicLevels() Then
                                'Check if there are crazy values with high consumption of prey with higher TL:
                                GoTo Start
                            ElseIf DoIterationsToEstimateB() <= 3 Then 'Try to do iterations
                                GoTo Start
                            Else
                                Exit_Sub_Missing_Par = 0
                                InParameterEstimation = 0
                                Result = eStatusFlags.MissingParameter
                                Return False
                            End If
                        End If

                    ElseIf EstimateFor = eEstimateParameterFor.Sensitivity Then
                        Exit_Sub_Missing_Par = 0
                        InParameterEstimation = 0
                        Result = eStatusFlags.MissingParameter

                        Return False

                        'From SensitivLoop
                        'If LoopC > m_data.NumGroups + 2 Then
                        '    MsgBox("Too many loops. Quitting Parameter Estimation.")
                        '    Exit_Sub_Missing_Par = 0
                        '    InParameterEstimation = 0
                        '    Return False
                        'End If
                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


                    End If 'If EstimateFor = eEstimateParameterFor.ParameterEstimation Then
                End If ' If LoopC > m_data.NumGroups + 2 Then

                EstimateGE()

                If (CountNoOfMissing(m_Data.mis, noMissing, EstimateFor) = False) Then
                    InParameterEstimation = 0
                    'jb 
                    cLog.Write("Too many missing parameters. Parameter Estimation not completed successfully.")
                    Result = eStatusFlags.MissingParameter
                    Return False
                End If
                '040112VC: In case B is missing, BABi is entered, then estimate BA(i) again
                For ji = 1 To m_Data.NumLiving
                    If m_Data.BA(ji) = 0 Then
                        m_Data.BA(ji) = CSng(if(m_Data.BaBi(ji) <> 0 And m_Data.B(ji) > 0, m_Data.BaBi(ji) * m_Data.B(ji), m_Data.BA(ji)))
                        'jb was m_data.BA(ji) = if(m_data.BaBi(ji) <> 0 And m_data.B(ji) > 0, m_data.BaBi(ji) * m_data.B(ji), m_data.BAi(ji))
                    End If

                Next

                If noMissing > 0 Then              ' else No GIM

                    EstimatePB(Pass)
                    If Pass = 1 Then
                        GoTo LoopCalc ' NOW PB IS KNOWN
                    End If

                    EstimateEE(Pass)
                    If Pass = 1 Then
                        GoTo LoopCalc ' Now EE is known
                    End If

                    EstimateB(Pass, EstimateFor, ExitSen)

                    If EstimateFor = eEstimateParameterFor.ParameterEstimation And (Exit_Sub_Missing_Par = 0 Or SecLoop = 3) Then

                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        'jb
                        'Exit stratagy ParamEstimate1 
                        If Exit_Sub_Missing_Par = 0 Or SecLoop% = 3 Then
                            If CyclesDone = False Then
                                'FindCyclesWhenEstimatingBiomass(Cons())
                                FindCyclesWhenEstimatingBiomass()
                                If bDietsModified Then checkDietsSumToOne(True)
                                CyclesDone = True
                                GoTo Start
                            ElseIf CheckPredatorPreyTrophicLevels() Then
                                'Check if there are crazy values with high consumption of prey with higher TL:
                                GoTo Start
                            ElseIf DoIterationsToEstimateB() <= 3 And TimesTried <= 10 Then 'Try to do iterations
                                TimesTried = TimesTried + 1
                                GoTo Start
                            Else
                                Exit_Sub_Missing_Par = 0
                                InParameterEstimation = 0
                                Result = eStatusFlags.MissingParameter
                                Return False
                            End If
                        End If
                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    End If

                    'in the original code  EstimateB(Pass, from) could set "From" to -1
                    'which would cause the loop to exit
                    'This has been changed to set a boolean flag 'ExitSen'
                    ''ExitSen' should only get set when called with EstimateFor =  eEstimateParameterFor.Sensitivity
                    'see EstimateB()
                    'If From = -1 Then
                    If ExitSen Then
                        'GoTo NextSensL ' Couldn't est parameters
                        '    Debug.Assert(False)
                        Result = eStatusFlags.ErrorEncountered
                        Return False
                    End If

                    If Pass = 1 Then
                        GoTo Start ' Now B is known
                    End If

                    EstimateQBorB_1(Pass)
                    If Pass = 1 Then
                        GoTo Start
                    End If

                    EstimateQBorB_2(Pass)
                    If Pass = 1 Then
                        GoTo Start
                    End If

                    If EstimateFor = eEstimateParameterFor.Sensitivity And SecLoop = 3 Then
                        'in the Sensitivity loop Exit 
                        Result = eStatusFlags.OK
                        Return False

                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        'Exit strategy from SensitivLoop
                        'If SecLoop = 3 Then
                        '    'If Not ShownAlready Then
                        '    '    MsgBox "Cannot estimate all parameters." + Chr$(13) + " Results may be unreliable." + Chr$(13) + "Please check your data"
                        '    '    ShownAlready = -1
                        '    'End If
                        '    'jb this is the equivalent to exit sub
                        '    GoTo NextSensL
                        '    'Insufficient Data
                        '    'jb
                        '    'Exit Sub
                        'End If
                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    End If

                    'Enter Generalized Inverse Method
                    Pass = 0
                    For ji = 1 To m_Data.NumLiving                  'GIM
                        If m_Data.mis(ji) > 0 Then           'IF EQ2=0 THEN B,PB,QB,EE known
                            'If Mis(ji) > 1 Then           'IF EQ2=0 THEN B,PB,QB,EE known
                            GIM(Result)

                            'GIM has thrown an error
                            If Result = eStatusFlags.ErrorEncountered Then
                                InParameterEstimation = 0
                                Result = eStatusFlags.ErrorEncountered
                                Return False
                            End If

                            'Failed to estimate parameters
                            If Exit_Sub_Missing_Par = 0 Then
                                InParameterEstimation = 0
                                Result = eStatusFlags.MissingParameter
                                Return False
                            End If

                            Pass = 1
                            ji = m_Data.NumLiving + 1
                            SecLoop = SecLoop + 1
                        End If       'B(2)  B(3)  EE(1)
                    Next ji

                    If Pass = 1 Then
                        GoTo LoopCalc
                    End If

                End If           'If NoMissing>0   This was earlier called NOGIM

                'From EwE5 SensitivLoop 
                'EwE6 does not do EcoPath Sensitivity 
                'If iterate > 1 Then CalcDeviation()
                'If iterate > 1 Then Progress()
                'NextSensL:
            Catch ex As Exception
                cLog.Write(ex)
                'Debug.Assert(False, "Error in EstimateParameters() " & ex.Message)
                'Message will be handled by the calling routine
                Result = eStatusFlags.ErrorEncountered
                Return False
            End Try

            Result = eStatusFlags.OK
            Debug.Assert(Result <> eStatusFlags.Null)
            Return True
        End Function


        Sub FindCyclesWhenEstimatingBiomass()
            ' Sub FindCyclesWhenEstimatingBiomass(ByVal Cons() As Single)'EwE5
            'CycDC [previously called CD] contains the proportion of the diet that is the minimum
            'amount in a cycle and should be removed to break the cycle.
            'This amount is subtracted from all flows in the cycle that contains only groups without biomasses.
            Dim APred, Comp, pred, prey As Integer
            'Dim Answer As Object
            Dim Cnt As Long
            'Dim arrow, bib
            Dim K As Integer
            Dim Diff As Single
            Dim PredTL As Single
            Dim PreyTL As Single

            Dim Pivot As Integer
            Dim path() As Integer
            Dim lastComp() As Integer
            Dim i As Integer
            Dim Level As Integer

            EstimateTrophicLevels(m_Data.NumGroups, m_Data.NumLiving, m_Data.PP, m_Data.DC, m_Data.TTLX)

            'AbortRun = False
            'DoWhat = "PPR"
            'frmWait.Caption = "Cycles may prevent estimation of biomasses; will break cycles by modifying diets"
            'frmWait.Frame1.Visible = False
            'frmWait.PBar.max = 1000 'm_data.Numliving
            'frmWait.ZOrder()
            'frmWait.Show() '0
            'frmWait.Refresh()
            Cnt = 0
            '   Array.Clear(Cons, 0, Cons.Length)

            ReDim path(2 * m_Data.NumGroups + 2)
            ReDim lastComp(2 * m_Data.NumGroups + 2)

            For Pivot = 1 To m_Data.NumLiving
                If m_Data.B(Pivot) > 0 Then GoTo NextPivot
                Array.Clear(path, 0, path.Length)
                '  Init1DimInteg(0, 2 * m_data.NumGroups + 2, Path())
                '  Assign1DimInteg(1, 2 * m_data.NumGroups + 1, Pivot, lastComp)
                For i = 1 To lastComp.Length - 1 : lastComp(i) = Pivot : Next

                path(Pivot - 1) = Pivot           ' Path's limits are Pivot -1 to Level 
                '*** FOR Level  = Pivot  TO m_data.Numliving
                For Level = Pivot To 2 * m_Data.NumLiving
                    If path(Level - 1) > 0 Then
                        pred = path(Level - 1)
                    Else
                        pred = Pivot
                    End If
                    For Comp = lastComp(Level) To m_Data.NumLiving
                        'only for groups that do not have biomass
                        If m_Data.B(Comp) <= 0 And m_Data.DC(pred, Comp) > 0 Then
                            prey = Comp
                            path(Level) = 0
                            CheckPath(path, Pivot, prey, Level)
                            If prey = 0 And Comp <> Pivot Then GoTo NextComp 'In Path already
                            If Pivot = Comp Then
                                path(Level) = Comp
                                '------------------------------------
                                'Next is a version of CyclePrint:
                                'CyclePrint CycDC(), Cons()
                                'arrow  = 1
                                'aa  = 0
                                'For k  = Pivot  - 1 To Level 
                                '    If Path(k ) > 0 Then
                                '        bib  = aa 
                                '        aa  = Path(k )
                                '        If arrow  = 0 Then
                                '            'mess$ = mess$ & "<---"
                                '            If CycDC(bib , aa ) < 0 Then
                                '                Cons(bib ) = 0
                                '            Else
                                '                Cons(bib ) = -DC(bib , aa )
                                '            End If
                                '        End If
                                '        'mess$ = mess$ & Cstr$(Path(k ))
                                '        arrow  = 0
                                '    End If
                                'Next k 
                                'MsgBox mess$
                                'mess$ = ""
                                'end of CyclePrint
                                '-------------------------------------
                                Cnt = Cnt + 1
                                'frmWait.Label1(0).Caption = "No of cycles: " + CStr(Cnt)
                                'UpdateWait()
                                'If AbortRun Then Exit Sub 'Have identified a cycle

                                'FindMinConsump Cons(), MinCons
                                'Find the link with the highest difference in TL
                                Diff = 100
                                For K = Pivot To m_Data.NumLiving
                                    If path(K) > 0 Then
                                        PreyTL = m_Data.TTLX(path(K))
                                        PredTL = m_Data.TTLX(path(K - 1))
                                        If PredTL - PreyTL < Diff Then
                                            Diff = PredTL - PreyTL
                                            prey = path(K)
                                            pred = path(K - 1)
                                        End If
                                        'If Cons(Path(k)) <= 0 And QB(Path(k)) > 0 Then
                                        '    If Cons(Path(k)) > MinCons Then MinCons = Cons(Path(k))
                                        'End If
                                    Else
                                        Exit For
                                    End If
                                Next K

                                ' JS 09may09: disabled diet=0 fix per VC email:
                                '      "The diets should not be set to 0 automatically. This was a fix for 
                                '       how to handle some specific problems (don’t remember the details). 
                                '       Probably best to remark out this cannibalism fix for now"

                                'If pred <> prey Then    'no need to break cannibalism cycles
                                '    'Debug.Print pred, prey, DC(pred, prey)
                                '    m_Data.DC(pred, prey) = 0
                                '    bDietsModified = True
                                'End If
                                path(Level) = 0
                            Else
                                path(Level) = Comp                      'Include group in Path
                                path(Level + 1) = 0
                                lastComp(Level) = Comp
                                lastComp(Level + 1) = Pivot
                                APred = 1
                                Exit For              'exit Comp  for loop when path found
                                'and continue to next Level 
                            End If
                        End If
                        APred = 0          'if program doesn't use EXIT FOR it will reset APred 
NextComp:

                        '   DoEvents()
                    Next Comp
                    '       If AbortRun Then Exit Sub 'Have identified a cycle
                    If APred = 0 Then                   'Start backtracking
                        'For Answer = 1 To Level: Debug.Print Format(Path(Answer), "  ##");: Next
                        'Debug.Print
                        If Level > Pivot Then lastComp(Level - 1) = path(Level - 1) + 1
                        path(Level) = 0
                        Level = Level - 2
                        If Level = Pivot - 2 Then Exit For 'Exit the Level for next and try new pivot
                    End If
                    '      frmWait.Label1(0).Caption = "Cycles: " + CStr(Cnt) + ", Pivot: " + CStr(Pivot) + ", Level: " + CStr(Level)
                Next Level
NextPivot:
            Next Pivot
            'Unload(frmWait)
            'frmWait = Nothing
        End Sub


        Sub CheckPath(ByRef path() As Integer, ByVal Pivot As Integer, ByRef prey As Integer, ByVal level As Integer)
            Dim K As Integer

            For K = Pivot - 1 To level + 1
                If prey = path(K) Then prey = 0 : Exit For
            Next K

        End Sub


        Private Sub RedimVariables()

            'ReDim mis(m_data.NumGroups)

            Erase NoBQB
            Erase H
            Erase Y
            Erase P
            Erase Q
            Erase AUL

            ReDim NoBQB(m_Data.NumGroups)
            ReDim H(m_Data.NumGroups + 3)
            ReDim Y(m_Data.NumGroups, m_Data.NumGroups)
            ReDim P(m_Data.NumGroups)
            ReDim Q(m_Data.NumGroups + 10)
            ReDim AUL(m_Data.NumGroups + 3, m_Data.NumGroups + 3)

        End Sub

        Private Sub CountMissingB_Ex()
            Dim doub1 As Integer, doub2 As Integer, i As Integer

            doub1 = 0
            doub2 = 0
            For i = 1 To m_Data.NumLiving
                If m_Data.B(i) <= 0 Then doub1 = doub1 + 1
                If m_Data.Ex(i) = 0 Then doub2 = doub2 + 1
            Next i
            If doub1 = m_Data.NumLiving And doub2 > m_Data.NumLiving - 1 Then
                'up to Mar. 94 it was Doub2 > NumGroups - 4 but no need for this ***
                'MsgBox("No biomasses -- Edit data ")
                'End
            End If

        End Sub

        Private Sub EstimateGE()
            Dim i As Integer

            For i = 1 To m_Data.NumLiving
                If m_Data.PB(i) < 0 And m_Data.QB(i) > 0 And m_Data.GE(i) > 0 Then
                    m_Data.PB(i) = m_Data.GE(i) * m_Data.QB(i)
                End If

                If m_Data.QB(i) < 0 And m_Data.PB(i) > 0 And m_Data.GE(i) > 0 Then
                    m_Data.QB(i) = m_Data.PB(i) / m_Data.GE(i)
                End If

                If m_Data.QB(i) > 0 And m_Data.PB(i) >= 0 Then
                    m_Data.GE(i) = m_Data.PB(i) / m_Data.QB(i)
                End If
            Next i

        End Sub


        Private Function CountNoOfMissing(ByRef Mis() As Integer, ByRef nNoMissing As Integer, ByVal From As eEstimateParameterFor) As Boolean
            'Private Sub CountNoOfMissing(ByRef Mis() As Integer, ByRef NoMissing As Integer, ByVal From As String, ByVal chk As Integer)

            'count the number of missing parameters for each group and store the values in the argument Mis()
            'this will have to change because the basic estimator and the Sensitivity loop count the number of missing parameters differently
            Dim iMissingForGroup As Integer
            Dim i As Integer
            Static done As Boolean

            nNoMissing = 0

            For i = 1 To m_Data.NumLiving
                iMissingForGroup = 0
                If m_Data.B(i) <= 0 Then iMissingForGroup += 1
                If m_Data.PB(i) < 0 Then iMissingForGroup += 1
                If m_Data.EE(i) < 0 Then iMissingForGroup += 1

                ' If Miss >= 2 And From = "ParameterEstimate" Then
                If iMissingForGroup >= 2 And From = eEstimateParameterFor.ParameterEstimation Then
                    MsgManyMissingPar(i)
                    Exit_Sub_Missing_Par = 0
                    cLog.Write("'CountNoOfMissing(...)' Group " & i & " missing " & iMissingForGroup.ToString & " parameter(s).")
                    Return False
                End If

                If m_Data.QB(i) < 0 And m_Data.PP(i) < 1 Then
                    iMissingForGroup = iMissingForGroup + 1
                End If

                '   If Miss >= 2 And From = "SensitivLoop" Then
                If iMissingForGroup >= 2 And From = eEstimateParameterFor.Sensitivity Then
                    ' From Sensitivity routine
                    ' chk = 1
                    If done = False Then
                        'jb todo
                        'MsgManyMissingSens(i)
                        done = True
                    End If

                    Exit_Sub_Missing_Par = 0
                    Return False
                End If

                Mis(i) = iMissingForGroup
                nNoMissing += iMissingForGroup
            Next i
            Return True

        End Function


        Private Sub EstimatePB(ByRef Pass As Integer)
            Dim MM2 As Double
            Dim i As Integer, j As Integer
            Dim Sum As Single

            For j = 1 To m_Data.NumLiving
                Pass = 0                       'Estimate PB from other parameters
                If m_Data.PB(j) < 0 And m_Data.B(j) > 0 And m_Data.EE(j) >= 0 Then    '1490
                    MM2 = 0
                    For i = 1 To m_Data.NumLiving
                        If m_Data.DC(i, j) > 0 Then                         '1470
                            If m_Data.B(i) <= 0 Or m_Data.QB(i) < 0 Then Exit For '1490
                            MM2 = MM2 + m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, j)   'M2 is amount eaten of
                        End If                                          'group J by predators I.

                        If i = m_Data.NumLiving Then
                            If (m_Data.B(j) * m_Data.EE(j)) <> 0 Then
                                '031220VC: Either BABi or BA is zero; Either Emigration or Emig is zero
                                Sum = CSng(if(m_Data.BaBi(j) <> 0 And m_Data.BA(j) = 0, m_Data.BaBi(j), 0))
                                Sum = Sum + CSng(if(m_Data.Emig(j) > 0 And m_Data.Emigration(j) = 0, m_Data.Emig(j), 0))
                                Sum = Sum * m_Data.B(j)
                                m_Data.PB(j) = CSng((MM2 + Sum + m_Data.BA(j) + m_Data.Emigration(j) - m_Data.Immig(j) + m_Data.fCatch(j)) / (m_Data.B(j) * m_Data.EE(j)))
                                'Added mig above 15022000 per discussion with Kerim / Villy
                            End If

                            If m_Data.PB(j) > 0 Then
                                Pass = 1
                                Exit Sub
                            Else
                                m_Data.PB(j) = -9                                    'Calc production
                            End If
                        End If
                    Next i
                End If
            Next j
        End Sub

        Private Sub EstimateEE(ByRef Pass As Integer)
            Dim i As Integer
            Dim j As Integer
            Dim MM2 As Double
            Dim Sum As Single
            'Estimate EE from other parameters
            For j = 1 To m_Data.NumLiving
                Pass = 0
                If m_Data.EE(j) < 0 And m_Data.B(j) > 0 And m_Data.PB(j) > 0 Then
                    MM2 = 0
                    For i = 1 To m_Data.NumLiving
                        If m_Data.DC(i, j) > 0 And m_Data.PP(i) < 1 Then

                            If m_Data.B(i) <= 0 Or m_Data.QB(i) < 0 Then
                                GoTo nextJ 'Exit For
                            End If

                            MM2 = MM2 + m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, j)     'M2 is amount eaten of
                        End If                                         'group j by predators i
                        '031220VC Now has Emigi and BABi as rates, won't have values if Emigration and BA have.
                        '031220VC: Either BABi or BA is zero; Either Emigration or Emig is zero
                    Next i
                    Sum = CSng(if(m_Data.BaBi(j) <> 0 And m_Data.BA(j) = 0, m_Data.BaBi(j), 0))
                    Sum = Sum + CSng(if(m_Data.Emig(j) > 0 And m_Data.Emigration(j) = 0, m_Data.Emig(j), 0))
                    Sum = Sum * m_Data.B(j)

                    If m_Data.B(j) * m_Data.PB(j) > 0 Then
                        If m_Data.StanzaGroup(j) = False Then
                            m_Data.EE(j) = CSng(MM2 + Sum + m_Data.Emigration(j) - m_Data.Immig(j) + m_Data.BA(j) + m_Data.fCatch(j)) / (m_Data.B(j) * m_Data.PB(j))
                        Else
                            m_Data.EE(j) = CSng(MM2 + Sum + m_Data.Emigration(j) - m_Data.Immig(j) + m_Data.fCatch(j)) / (m_Data.B(j) * m_Data.PB(j))
                        End If
                    End If

                    If m_Data.EE(j) >= 0 Then
                        Pass = 1
                        Exit Sub
                    Else
                        m_Data.EE(j) = -91
                    End If
                End If 'If m_data.EE(j) < 0 And m_data.B(j) > 0 And m_data.PB(j) > 0 Then
nextJ:
            Next j
        End Sub


        Private Sub EstimateB(ByRef Pass As Integer, ByVal EstimateFor As eEstimateParameterFor, ByRef SenExit As Boolean)
            Dim i As Integer
            Dim j As Integer
            Dim Miss As Integer
            Dim PartM2 As Double
            Dim Only As Double
            Dim CancelPressed As Boolean = False
            Dim Sum As Single
            Dim msg As cFeedbackMessage = Nothing
            Dim strMessage As String = ""

            For j = 1 To m_Data.NumLiving
                Pass = 0 'Estimate B
                If m_Data.PB(j) > 0 And m_Data.EE(j) > 0 And m_Data.B(j) <= 0 And m_Data.mis(j) = 1 Then
                    'If Mis(j) = 0 Or Mis(j) > 1 Then goto NextJ '1680
                    Miss = 0 : PartM2 = 0
                    For i = 1 To m_Data.NumLiving
                        If m_Data.DC(i, j) > 0 Then
                            If m_Data.B(i) <= 0 Or m_Data.QB(i) < 0 Then
                                If i <> j Then GoTo nextJ '1680
                            Else
                                PartM2 = PartM2 + m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, j)
                            End If
                        End If
                    Next i
                    If m_Data.QB(j) < 0 And m_Data.PP(j) < 0 Then GoTo nextJ '1680
                    '031220VC: Either BABi or BA is zero; Either Emigration or Emig is zero
                    Sum = CSng(if(m_Data.BaBi(j) <> 0 And m_Data.BA(j) = 0, m_Data.BaBi(j), 0))
                    Sum = Sum + CSng(if(m_Data.Emig(j) > 0 And m_Data.Emigration(j) = 0, m_Data.Emig(j), 0))
                    Only = m_Data.PB(j) * m_Data.EE(j) - m_Data.QB(j) * m_Data.DC(j, j) - Sum
                    ' There may be too much cannibalism when e.g. the biomass
                    ' of a group is changed and the EE is kept constant in the
                    ' sens analysis. The results are not valid when this happens
                    ' and they are not presented, but still the sensitivity routine
                    ' is allowed to continue.
                    If Only < 0 Then
                        'If From = 1 Then             ' From Parameter estimation
                        If EstimateFor = eEstimateParameterFor.ParameterEstimation Then
                            If Abort4(j) Then 'has changed data if this is true
                                'repeat estimation
                                Pass = 1    'Will make it start estimation again from scratch
                            Else
                                Exit_Sub_Missing_Par = 0
                            End If
                            Exit Sub
                            'jb changed from
                            ' ElseIf From = 2 Then         ' From Sensitivity routine
                        ElseIf EstimateFor = eEstimateParameterFor.Sensitivity Then
                            'the calling model is the Sensitivity routine 
                            'this will cause it to exit the parameter estimation
                            cLog.Write("Ecopath.EstimateB() parameter estimation error. There maybe to much cannibalism.")
                            SenExit = True
                        End If
                    End If
                    If Only = 0 Then GoTo nextJ '1680
                    If PartM2 < 0 Then m_Data.B(j) = -99 : GoTo nextJ '1680
                    ' Up to Mar 94 it was PartM2 <= 0 but this failed
                    ' to catch cases with toppredators with unknown B.
                    m_Data.B(j) = CSng((m_Data.fCatch(j) + m_Data.BA(j) + m_Data.Emigration(j) - m_Data.Immig(j) + PartM2) / Only)
                    'Added mig above 15022000 per discussion with Kerim / Villy
                    If m_Data.B(j) > 0 Then
                        Pass = 1
                        Exit Sub
                    ElseIf Not CancelPressed Then

                        If m_Data.B(j) = 0 Then
                            ' Prepare message text
                            strMessage = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_B0_FISHERY, j, m_Data.GroupName(j))
                            ' Prepare message
                            msg = New cFeedbackMessage(strMessage, eCoreComponentType.EcoPath, eMessageType.Any, eMessageImportance.Maintenance)
                            msg.Suppressable = True
                            ' Send off
                            NotifyCore(msg)
                            ' Catch result
                            CancelPressed = (msg.Reply = 0)
                        End If

                        If m_Data.B(j) < 0 Then
                            If Only < 0 Then
                                ' Prepare message text
                                strMessage = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_PRODxEE, j, m_Data.GroupName(j), Only.ToString("0.000"))
                                ' Prepare message
                                msg = New cFeedbackMessage(strMessage, eCoreComponentType.EcoPath, eMessageType.Any, eMessageImportance.Maintenance)
                                msg.Suppressable = True
                                ' Send off
                                NotifyCore(msg)
                                ' Catch result
                                CancelPressed = (msg.Reply = 0)
                            Else
                                ' Prepare message text
                                strMessage = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_B_FISHERIY, j, m_Data.GroupName(j), m_Data.B(j).ToString("0.000"))
                                ' Prepare message
                                msg = New cFeedbackMessage(strMessage, eCoreComponentType.EcoPath, eMessageType.Any, eMessageImportance.Maintenance)
                                msg.Suppressable = True
                                ' Send off
                                NotifyCore(msg)
                                ' Catch result
                                CancelPressed = (msg.Reply = 0)
                            End If
                        End If
                        If CancelPressed = True Then Exit Sub
                        m_Data.B(j) = -9
                    End If
                End If
nextJ:
            Next j
        End Sub

        Private Function Abort4(ByVal j As Integer) As Boolean

            Dim bSucces As Boolean = False
            Dim str As String
            Dim Answer As Single
            Dim fmsg As cFeedbackMessage = Nothing
            Dim msg As cMessage = Nothing

            str = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PREDMORT_CANN, Me.m_Data.GroupName(j))
            fmsg = New cFeedbackMessage(str, _
                                       eCoreComponentType.EcoPath, eMessageType.Any, _
                                       eMessageImportance.Maintenance, eMessageReplyStyle.YES_NO_CANCEL)
            fmsg.Suppressable = True
            Me.NotifyCore(fmsg)

            Select Case fmsg.Reply
                Case eMessageReply.YES
                    Me.m_Data.DietsModified = True
                    For j = 1 To Me.m_Data.NumLiving
                        If Me.m_Data.EE(j) > 0 Then
                            Answer = Me.m_Data.PB(j) * Me.m_Data.EE(j) - Me.m_Data.QB(j) * Me.m_Data.DC(j, j)
                            If Answer < 0 Then  'cannibalism exceeds utilized production
                                Me.m_Data.DC(j, j) = Me.m_Data.PB(j) * Me.m_Data.EE(j) / Me.m_Data.QB(j) / 5
                            End If
                        End If
                    Next
                    'Now make the diets sum to 1 again:
                    bSucces = checkDietsSumToOne(True)

                Case Else
                    msg = New cMessage(My.Resources.CoreMessages.GENERIC_ABORTING_EDIT_DATA, eMessageType.Any, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                    Me.NotifyCore(msg)
                    bSucces = False

            End Select

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bQuiet">Flag stating whether the user should be asked
        ''' what to do if diets do not sum to one. If true, this method executes
        ''' quietly and no questions will be asked. If False, the user will be
        ''' prompted.</param>
        ''' <returns>True if diets do sum to one.</returns>
        ''' -------------------------------------------------------------------
        Private Function checkDietsSumToOne(ByVal bQuiet As Boolean) As Boolean

            Dim iPred As Integer
            Dim iPrey As Integer
            Dim sSum As Single
            Dim sTolerance As Single
            Dim msgMissing As cMessage = Nothing
            Dim msgSumToOne As cFeedbackMessage = Nothing
            Dim msgMaintenance As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing
            Dim reply As eMessageReply = eMessageReply.YES
            Dim bMissing As Boolean = False
            Dim bSumToOne As Boolean = True

            sTolerance = 0.001

            ' Check all diets
            For iPred = 1 To m_Data.NumLiving
                ' Is consumer?
                If m_Data.PP(iPred) < 1 Then
                    ' #Yes: determine diet sum
                    sSum = 0
                    For iPrey = 0 To m_Data.NumGroups
                        sSum += m_Data.DC(iPred, iPrey)
                    Next

                    ' JS 4Nov14 - EwE30 course feedback: should warn when a predator has no diet!
                    If (sSum <= 0) Then

                        bMissing = True
                        bSumToOne = False

                        If (msgMissing Is Nothing) Then
                            msgMissing = New cMessage(
                                My.Resources.CoreMessages.DIETCOMP_PROMPT_MISSING, _
                                eMessageType.DataValidation, eCoreComponentType.EcoPath, eMessageImportance.Critical)
                        End If

                        ' Attach variable status
                        vs = New cVariableStatus(eStatusFlags.MissingParameter, _
                                cStringUtils.Localize(My.Resources.CoreMessages.DIETCOMP_MISSING, Me.m_Data.GroupName(iPred)), _
                                eVarNameFlags.DietComp, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, iPred)
                        msgMissing.AddVariable(vs)

                    ElseIf (Math.Abs(sSum - 1) > sTolerance) Then
                        ' #Yes: does not sum to one!
                        bSumToOne = False

                        ' No message ready?
                        If (msgSumToOne Is Nothing) Then

                            ' #Yes: prepare message
                            msgSumToOne = New cFeedbackMessage( _
                                    My.Resources.CoreMessages.DIETCOMP_PROMPT_SUMTOONE, _
                                    eCoreComponentType.EcoPath, eMessageType.DietComp, eMessageImportance.Warning, _
                                    eMessageReplyStyle.YES_NO)
                            msgMaintenance = New cMessage("Ecopath diets have changed", eMessageType.DataModified, eCoreComponentType.EcoPath, eMessageImportance.Maintenance)
                        End If

                        ' Attach variable status
                        vs = New cVariableStatus(eStatusFlags.MissingParameter, _
                                cStringUtils.Localize(My.Resources.CoreMessages.DIETCOMP_SUMTOONE_PRED, Me.m_Data.GroupName(iPred)), _
                                eVarNameFlags.DietComp, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, iPred)

                        msgSumToOne.AddVariable(vs)
                        msgMaintenance.AddVariable(vs)

                    End If
                End If
            Next

            ' Found any missing diets?
            If (bMissing = True) Then
                Me.NotifyCore(msgMissing)
            Else
                ' Found any diets that did not sum to 1?
                If (bSumToOne = False) Then
                    ' #Yes: has message to send?
                    If (msgSumToOne IsNot Nothing) Then
                        ' #Yes: send message and grab reply
                        Me.NotifyCore(msgSumToOne)
                        reply = msgSumToOne.Reply
                    End If
                End If

                ' Any diets that did not sum, and needing to fix?
                If (bSumToOne = False) And (reply = eMessageReply.YES) Then
                    ' #Yes: make diets sum to one
                    For iPred = 1 To m_Data.NumLiving Step 1
                        If m_Data.PP(iPred) < 1 Then
                            sSum = 0
                            For iPrey = 0 To m_Data.NumGroups Step 1
                                sSum = sSum + m_Data.DC(iPred, iPrey)
                            Next
                            If sSum <> 0 And Math.Abs(sSum - 1) > sTolerance Then
                                For iPrey = 0 To m_Data.NumGroups Step 1
                                    m_Data.DC(iPred, iPrey) = m_Data.DC(iPred, iPrey) / sSum
                                Next
                                m_Data.DietsModified = True
                            End If
                        End If
                    Next
                    bSumToOne = True

                    If m_Data.DietsModified Then
                        ' Notify the core that data has changed
                        Me.NotifyCore(msgMaintenance)
                    End If
                End If
            End If

            Return bSumToOne

        End Function




        Private Sub EstimateQBorB_1(ByVal Pass As Integer)
            'The following is a routine made by VC in March 1994 to estimate
            'QB Or B independent of the Generalized Inverse. It works in cases
            'where for a given predator j the PB, B, EE are known for all
            'prey, and where all predation on these prey apart from that
            'caused by predator j is known

            Dim i As Integer, j As Integer, K As Integer
            Dim SumQ As Single, SumMi As Single
            Dim Sum As Single

            For j = 1 To m_Data.NumLiving
                SumQ = 0
                Pass = 0                      'Estimate QB or B
                If (m_Data.QB(j) > 0 And m_Data.B(j) <= 0) Or (m_Data.QB(j) < 0 And m_Data.B(j) > 0) Then
                    ' If QB(j) * B(j) < 0 Then
                    ' If both are known or both are unknown it won't enter
                    For i = 1 To m_Data.NumLiving
                        If m_Data.DC(j, i) > 0 And SumQ >= 0 Then
                            If m_Data.B(i) > 0 And m_Data.PB(i) > 0 And m_Data.EE(i) >= 0 Then
                                '031220VC:
                                Sum = CSng(if(m_Data.BaBi(i) <> 0 And m_Data.BA(i) = 0, m_Data.BaBi(i), 0))
                                Sum = Sum + CSng(if(m_Data.Emig(i) > 0 And m_Data.Emigration(i) = 0, m_Data.Emig(i), 0))
                                Sum = Sum * m_Data.B(i)
                                SumMi = m_Data.BA(i) + Sum + m_Data.Emigration(i) + m_Data.Immig(i) + m_Data.fCatch(i) + (1 - m_Data.EE(i)) * m_Data.PB(i) * m_Data.B(i)
                                'Added mig above 15022000 per discussion with Kerim / Villy
                                'SumMi is used to add up all mortalities of i. If the only
                                ' lacking mortality is due to j then QB(j) or B(j) can
                                ' be estimated. The first term (above) sums export and M0.
                                For K = 1 To m_Data.NumLiving
                                    If K <> j Then
                                        If m_Data.DC(K, i) > 0 Then
                                            'This is a predator on i
                                            If m_Data.QB(K) > 0 And m_Data.B(K) > 0 Then
                                                SumMi = SumMi + m_Data.QB(K) * m_Data.B(K) * m_Data.DC(K, i)
                                                'This terms gives how much k eats of i
                                            Else
                                                SumMi = -9
                                                SumQ = -9
                                                Exit For    'for k
                                            End If
                                        End If    'End DC(k,i) > 0
                                    End If       'End k <> j
                                Next K
                                If SumMi > 0 Then SumQ = SumQ + m_Data.PB(i) * m_Data.B(i) - SumMi
                            Else
                                SumQ = -9
                                Exit For    'for i
                            End If
                        End If
                    Next i
                End If
                If SumQ > 0.0001 Then '0 Then
                    If m_Data.QB(j) < 0 And m_Data.B(j) > 0 And SumQ > 0.0001 Then
                        m_Data.QB(j) = SumQ / m_Data.B(j)
                        Pass = 1
                    ElseIf m_Data.B(j) <= 0 And m_Data.QB(j) > 0 And SumQ > 0.0001 Then
                        m_Data.B(j) = SumQ / m_Data.QB(j)
                        Pass = 1
                    End If
                    SumQ = -9
                End If
            Next j
        End Sub

        Private Sub GIM(ByRef Result As eStatusFlags)
            Dim i As Integer
            Dim j As Integer
            Dim Estim As Integer
            Dim Pass As Integer
            Dim Total As Integer
            Dim NBQB As Integer
            'Dim jj As Integer
            Dim Kount As Integer
            Dim kountj As Integer
            Dim NN As Integer
            Dim MM As Integer
            'Dim kc As Integer
            'Dim kq As Integer
            'Dim BQBDC As Double
            'Dim PartM2 As Double
            Dim Sum As Single
            NN = 0
            MM = 0

            'added 053196 eli.
            ReDim AUL(m_Data.NumGroups + 10, m_Data.NumGroups + 10)
            ReDim Q(m_Data.NumGroups + 10)
            Dim LHS(m_Data.NumGroups, m_Data.NumGroups) As Single

            Try


                '             Count number of unknown B's and QB's
                '             ------------------------------------
                'jb
                'set the NoBQB() flag
                '1 means B is missing
                '10 means QB and PP are missing 
                '11 means B QB and PP are all missing
                For i = 1 To m_Data.NumLiving

                    NoBQB(i) = 0
                    If m_Data.B(i) <= 0 Then
                        NoBQB(i) = 1
                    End If

                    '040112VC Added the check for pproducers below, seems necessary when calling from frmBvary
                    If m_Data.QB(i) < 0 And m_Data.PP(i) < 1 Then
                        NoBQB(i) = NoBQB(i) + 10
                    End If

                Next i

                'now count the the number of missing B QB 
                'and total missing parameters
                Total = 0 'total number of missing parameter
                NBQB = 0 'total missing  B QB 
                For i = 1 To m_Data.NumLiving
                    If NoBQB(i) = 11 Then NBQB = NBQB + 1
                    If NoBQB(i) > 0 Then Total = Total + 1
                Next i

                Pass = 0
                If NBQB >= 1 Then
                    'compute B()  and QB() 
                    SolvenoBnoQB(Pass, NBQB)
                    If Pass = 1 Then
                        'all done
                        Result = eStatusFlags.OK
                        Exit Sub
                    End If
                End If

                If NBQB > 0 Then

                    Dim msg As New cMessage(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_INSUFFICIENTDATA, _
                        eMessageType.MassBalance_InsufficientData, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                    msg.Suppressable = True

                    NotifyCore(msg)
                    Exit_Sub_Missing_Par = 0
                    Result = eStatusFlags.MissingParameter
                    Exit Sub
                End If

                For i = 1 To m_Data.NumLiving
                    If m_Data.PB(i) >= 0 And m_Data.EE(i) >= 0 Then
                        'jb fixed bug 891
                        Q(i) = m_Data.fCatch(i) + m_Data.BA(i) + m_Data.Emigration(i) - m_Data.Immig(i)
                        'Q(i) = m_Data.fCatch(i) + m_Data.BA(i) + m_Data.Emigration(j) - m_Data.Immig(j)
                        'vc980303 This was including detritus, don't know why For j = 1 To NumGroups
                        For j = 1 To m_Data.NumLiving
                            AUL(i, j) = -9999
                            If NoBQB(j) = 11 Then
                                Dim strMsg As String = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_INVALIDMODEL_MISSING_B_QB, m_Data.GroupName(j))
                                Dim msg As New cMessage(strMsg, eMessageType.Any, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                                NotifyCore(msg)
                                Exit_Sub_Missing_Par = 0
                                Result = eStatusFlags.MissingParameter
                                Exit Sub
                            End If
                            If NoBQB(j) = 10 And i = j Then Q(i) = Q(i) - m_Data.B(i) * m_Data.PB(i) * m_Data.EE(i)
                            If NoBQB(j) = 1 And i = j Then AUL(i, j) = (m_Data.PB(i) * m_Data.EE(i) - m_Data.QB(j) * m_Data.DC(j, i))
                            ' No B:
                            If NoBQB(j) = 1 And i <> j Then AUL(i, j) = -m_Data.QB(j) * m_Data.DC(j, i)
                            'No QB
                            '031220VC Emigi and BABi now included as rates, will be zero if there are flows (Emigration and BA)
                            Sum = CSng(if(m_Data.BaBi(j) <> 0 And m_Data.BA(j) = 0, m_Data.BaBi(j), 0))
                            Sum = Sum + CSng(if(m_Data.Emig(j) > 0 And m_Data.Emigration(j) = 0, m_Data.Emig(j), 0))
                            Sum = Sum * m_Data.B(j)
                            If NoBQB(j) = 10 Then AUL(i, j) = -m_Data.B(j) * m_Data.DC(j, i)
                            If NoBQB(j) = 0 And i <> j Then Q(i) = Q(i) + (m_Data.B(j) * m_Data.QB(j) * m_Data.DC(j, i)) + Sum
                            If NoBQB(j) = 0 And i = j Then Q(i) = Q(i) - m_Data.B(j) * (m_Data.EE(i) * m_Data.PB(j) - m_Data.QB(j) * m_Data.DC(j, i)) + Sum
                        Next j
                    End If
                Next i

                'GoSub 7440     'Goto generalized inverse routine
                '             Generalized inverse method
                '             --------------------------
                Kount = 0
                For i = 1 To m_Data.NumLiving 'N1 modified 053196 eli.
                    Total = 0
                    For j = 1 To m_Data.NumGroups
                        If AUL(i, j) <> -9999 And AUL(i, j) <> 0 Then
                            Total = 1
                            j = m_Data.NumGroups + 1
                        End If
                    Next j

                    If m_Data.fCatch(i) >= 0 And m_Data.PB(i) >= 0 And Total = 1 Then    'GoTo 7620 'OR TD(i) < 0
                        Kount = Kount + 1
                        H(Kount) = Q(i)
                        kountj = 0
                        For j = 1 To m_Data.NumGroups
                            If AUL(i, j) <> -9999 Then    'GoTo 7610         'EXCL PRIMARY PROD. & DETRITUS
                                kountj = kountj + 1
                                LHS(Kount, kountj) = AUL(i, j)
                            End If
                        Next j
                    End If
                Next i

                NN = Kount : MM = kountj
                If NN < MM Then Me.ManyUnknown(m_Data.NumLiving, NN, MM, NoBQB)
                If NN <> 0 And MM <> 0 Then
                    Geninv(NN, MM, LHS)
                    Estim = 1
                    Result = eStatusFlags.OK
                Else
                    Result = eStatusFlags.MissingParameter
                    Estim = 0
                    For j = 1 To m_Data.NumGroups
                        P(j) = 0
                    Next j
                End If



                '             If parameters have been estimated
                '             ---------------------------------
                If Estim = 1 Then
                    Kount = 0
                    For i = 1 To m_Data.NumLiving                     '*** Changed 19 jan 94
                        If m_Data.PB(i) >= 0 And NoBQB(i) > 0 Then
                            Kount = Kount + 1
                            If NoBQB(i) = 1 Then m_Data.B(i) = CSng(P(Kount))
                            If NoBQB(i) = 10 Then m_Data.QB(i) = CSng(P(Kount))
                        End If
                    Next i
                End If

            Catch ex As Exception
                Result = eStatusFlags.ErrorEncountered
                cLog.Write(ex, "Ecpopath.GIM() Exception. ")
            End Try

        End Sub


        Private Sub SolvenoBnoQB(ByRef pass As Integer, ByRef NBQB As Integer)
            'Solve equation if B(i) and QB(i) are both unknown

            Dim kc As Integer, kq As Integer
            Dim i As Integer
            Dim sum As Single
            Dim BQBDC As Single
            Dim PartM2 As Single
            Dim msg As cMessage = Nothing

            'jb
            'find the first group that is missing both B QB and PP 
            'this is flaged by the NoBQB(i) = 11 see GIM()
            'kg will be this group
            For i = 1 To m_Data.NumLiving
                If NoBQB(i) = 11 Then
                    kq = i
                    'jb todo  i = m_data.NumLiving is not doing anything
                    'i is reset in the next loop
                    ' i = m_data.NumLiving
                    Exit For
                End If
            Next i

            'jb
            'now find the pray that is not missing any parameters for group kg
            For i = 1 To m_Data.NumLiving
                If m_Data.mis(i) = 0 And m_Data.DC(kq, i) > 0 Then kc = i
            Next i

            If kc = 0 Or m_Data.PB(kq) <= 0 Or m_Data.EE(kq) <= 0 Then
                'jb changed to return to calling code
                'as was the intent of the original code
                Exit Sub
                'GoTo returnfrom_SolveBnoQB
                'Return 'GOTO ABORT
            End If

            '031220VC: rates or flows,
            sum = CSng(if(m_Data.BaBi(kc) <> 0 And m_Data.BA(kc) = 0, m_Data.BaBi(kc), 0))
            sum = sum + CSng(if(m_Data.Emig(kc) > 0 And m_Data.Emigration(kc) = 0, m_Data.Emig(kc), 0))
            sum = sum * m_Data.B(kc)

            BQBDC = m_Data.B(kc) * m_Data.PB(kc) * m_Data.EE(kc) - m_Data.fCatch(kc) - m_Data.BA(kc) - m_Data.Emigration(kc) + m_Data.Immig(kc) - sum
            'Added mig above 15022000 per discussion with Kerim / Villy
            PartM2 = 0
            For i = 1 To m_Data.NumLiving
                If i <> kq Then
                    If (m_Data.DC(i, kq) > 0 Or m_Data.DC(i, kc) > 0) Then   'GoTo 6830

                        If m_Data.B(i) <= 0 Or m_Data.QB(i) < 0 Then
                            i = m_Data.NumLiving
                            'jb changed to return to calling code
                            'as was the intent of the original code
                            Exit Sub
                            'GoTo returnfrom_SolveBnoQB
                            'Return 'GoTo 6840
                        End If

                        BQBDC = BQBDC - m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, kc)
                        PartM2 = PartM2 + m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, kq)
                    End If
                End If
            Next i

            If BQBDC < 0 Then

                Debug.Assert(False, "SolvenoBnoQB")

                'msg = New cMessage(My.Resources.INFORMATION_MISSING_PARAMETERS, eMessageType.Any, eCoreComponentType.EcoPath, eMessageImportance.Information)
                'notifyCore(msg)

                Exit_Sub_Missing_Par = 0
                Exit Sub
            End If

            '031220VC, either use BaBi or BA and either Emigi or Emigration
            If (m_Data.DC(kq, kc)) > 0 And (m_Data.PB(kq) * m_Data.EE(kq)) > 0 Then
                sum = CSng(if(m_Data.BaBi(kq) <> 0 And m_Data.BA(kq) = 0, m_Data.BaBi(kq), 0))
                sum = sum + CSng(if(m_Data.Emig(kq) > 0 And m_Data.Emigration(kq) = 0, m_Data.Emig(kq), 0))
                'Sum is the combined migration and biomass.acc instantaneous mortality rate, will only be non-zero if these are entered
                'B.PB.EE = Pred + NM.B + BAB.B + Catch, hence B = (Pred + Catch)/(PB.EE-NM-BAB)
                m_Data.B(kq) = (PartM2 + m_Data.BA(kq) + m_Data.Emigration(kq) - m_Data.Immig(kq) + m_Data.fCatch(kq) + m_Data.DC(kq, kq) * BQBDC / m_Data.DC(kq, kc)) / (m_Data.PB(kq) * m_Data.EE(kq) - sum)
            End If
            'Added mig above 15022000 per discussion with Kerim / Villy

            If m_Data.B(kq) > 0 And m_Data.DC(kq, kc) > 0 Then
                m_Data.QB(kq) = BQBDC / (m_Data.B(kq) * m_Data.DC(kq, kc))
            Else
                m_Data.B(kq) = -9
            End If

            If m_Data.B(kq) > 0 And m_Data.QB(kq) >= 0 Then
                NoBQB(kq) = 0
                pass = 1
                NBQB = NBQB - 1
            End If

        End Sub

        Private Sub Geninv(ByVal NN As Integer, ByVal MM As Integer, ByVal LHS(,) As Single)
            Dim t As Single, i As Integer, j As Integer, L As Integer, lhsi As Integer, K As Single, d As Single
            'jb are these local?????????
            'I hope so because they are now
            Dim Z(,) As Single, W(,) As Single, Y(,) As Single

            ReDim Z(m_Data.NumGroups, m_Data.NumGroups)
            ReDim W(m_Data.NumGroups, m_Data.NumGroups)
            ReDim Y(m_Data.NumGroups, m_Data.NumGroups)
            'StrLong$ = " Geninv routine" & Chr$(13) & Chr$(13)
            ' LOCATE 16, 12: Print "              2*N + trace =";

            'StrLong$ = StrLong$ & "Calculating parameters using generalized inverse method" & Chr$(13)
            'StrLong$ = StrLong$ & "Optimizing until trace of matrix is near integer"
            ' NN is the number of equations
            ' MM is the number of unknowns
            For lhsi = 1 To NN
                For j = 1 To MM
                    W(j, lhsi) = LHS(lhsi, j)                'W is transpose of LHS
                    '    LPRINT USING " ####.####"; lhs(lhsi, j);
                Next j                            'RightHandSide of equation
                ' LPRINT H(lhsi)
            Next lhsi

            K = 0
            For i = 1 To NN
                For j = 1 To NN
                    Z(i, j) = 0
                    For L = 1 To MM
                        Z(i, j) = Z(i, j) + LHS(i, L) * W(L, j)
                    Next L
                    K = K + Math.Abs(Z(i, j))
                Next j
            Next i

            K = 1 / K
            d = 0.00001                                       'small constant
            For i = 1 To MM
                For j = 1 To NN
                    Y(i, j) = K * W(i, j)                  'first approximation to inverse
                Next j
            Next i

ONE:

            For i = 1 To NN
                For j = 1 To NN
                    Z(i, j) = 0
                    For L = 1 To MM
                        Z(i, j) = Z(i, j) + LHS(i, L) * Y(L, j)
                    Next L
                Next j
            Next i

            t = 0                                    'Trace=T
            For i = 1 To NN
                Z(i, i) = Z(i, i) - 2
                t = t + Z(i, i)
            Next i
            'LOCATE 16, 40: Print 2 * NN + T; Spc(10);

            'If g_in_Ranger = 0 And g_in_senseloop = 0 Then
            '    'frmGIM.Label1 = Format(2 * NN + T, " ###.###### ")
            '    'frmGIM.Refresh
            'End If

            For i = 1 To MM
                For j = 1 To NN
                    W(i, j) = 0
                    For L = 1 To NN
                        W(i, j) = W(i, j) + Y(i, L) * Z(L, j)
                    Next L
                Next j
            Next i
            For i = 1 To MM
                For j = 1 To NN
                    Y(i, j) = -W(i, j)
                Next j
            Next i
            If Math.Abs(t - CInt(t) - 1) >= d And Math.Abs(t - CInt(t)) >= d Then GoTo ONE
            'Repeat until T is an integer

            'P is the solutions to the equations
            'Y is the generalized inverse
            For i = 1 To MM
                P(i) = 0
                For j = 1 To NN
                    P(i) = P(i) + Y(i, j) * H(j)
                    'If Y(i, j) < 0 Then
                    '
                    'End If
                Next j
            Next i
            Erase Z, W   '- test for compilation
        End Sub

        Private Sub EstimateQBorB_2(ByVal Pass As Integer)
            'The following is a routine made by VC in March 1994 to estimate
            'QB Or B independent of the Generalized Inverse. It works in cases
            'where for a given prey j the PB, B, EE is known and where
            'the only unknown predation is due to one predator j
            'whose B or QB is unknown.

            Dim i As Integer, j As Integer
            Dim Cnt As Integer
            Dim LeftProd As Single
            Dim MisQ As Integer
            Dim Sum As Single
            For j = 1 To m_Data.NumLiving                'j is the prey
                Pass = 0                       'Estimate QB or B
                Cnt = 0
                If m_Data.B(j) > 0 And m_Data.PB(j) > 0 And m_Data.EE(j) > 0 Then
                    '031220VC, emig and ba as rates
                    Sum = CSng(if(m_Data.BaBi(j) <> 0 And m_Data.BA(j) = 0, m_Data.BaBi(j), 0))
                    Sum = Sum + CSng(if(m_Data.Emig(j) > 0 And m_Data.Emigration(j) = 0, m_Data.Emig(j), 0))
                    Sum = Sum * m_Data.B(j)

                    LeftProd = m_Data.B(j) * m_Data.PB(j) * m_Data.EE(j) - m_Data.fCatch(j) - Sum - m_Data.BA(j) - m_Data.Emigration(j) + m_Data.Immig(j)
                    'Added mig above 15022000 per discussion with Kerim / Villy
                    For i = 1 To m_Data.NumLiving             'i is the predator without Q
                        If m_Data.DC(i, j) > 0 Then
                            If m_Data.B(i) <= 0 Or m_Data.QB(i) < 0 Then
                                Cnt = Cnt + 1
                                MisQ = i
                            Else
                                LeftProd = LeftProd - m_Data.B(i) * m_Data.QB(i) * m_Data.DC(i, j)
                            End If
                        End If
                    Next i
                End If
                If Cnt = 1 Then
                    If m_Data.QB(MisQ) < 0 And m_Data.B(MisQ) > 0 And LeftProd > 0.0001 Then
                        m_Data.QB(MisQ) = LeftProd / m_Data.B(MisQ) / m_Data.DC(MisQ, j)
                        Pass = 1
                    ElseIf m_Data.B(MisQ) < 0 And m_Data.QB(MisQ) > 0 And LeftProd > 0.0001 Then
                        m_Data.B(MisQ) = LeftProd / m_Data.QB(MisQ) / m_Data.DC(MisQ, j)
                        Pass = 1
                    End If
                End If
            Next j
        End Sub

        Private Sub ManyUnknown(ByVal NumLiving As Integer, ByVal NN As Integer, ByVal MM As Integer, ByVal NoBQB() As Integer)

            Dim msg As cMessage = Nothing
            Dim vs As cVariableStatus = Nothing
            Dim varname As eVarNameFlags = eVarNameFlags.NotSet
            Dim strMsg As String = ""
            Dim i As Integer

            If InParameterEstimation = 0 Then
                Exit Sub
            End If

            ' Create error message
            strMsg = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_NOTUNIQUE, MM, NN)
            msg = New cMessage(strMsg, eMessageType.TooManyMissingParameters, eCoreComponentType.EcoPath, eMessageImportance.Warning)
            msg.Suppressable = False

            ' Add error details
            For i = 1 To NumLiving
                ' Reset varname
                varname = eVarNameFlags.NotSet
                ' Determine error detail
                If NoBQB(i) = 1 Then
                    strMsg = cStringUtils.Localize(My.Resources.CoreMessages.B_MISSING_GROUP, Me.m_Data.GroupName(i))
                    varname = eVarNameFlags.BiomassAreaInput
                ElseIf NoBQB(i) = 10 Then
                    strMsg = cStringUtils.Localize(My.Resources.CoreMessages.QB_MISSING_GROUP, Me.m_Data.GroupName(i))
                    varname = eVarNameFlags.QBInput
                End If
                ' Has error detail?
                If (varname <> eVarNameFlags.NotSet) Then
                    ' #Yes: append to message
                    vs = New cVariableStatus(eStatusFlags.MissingParameter, strMsg, varname, _
                                             eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                    msg.AddVariable(vs)
                End If
            Next i

            ' Send error
            Me.NotifyCore(msg)

        End Sub

        Private Sub MsgManyMissingPar(ByVal i As Integer)
            Dim strMsg As String
            Dim msg As cMessage = Nothing

            Try
                strMsg = cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_PARAMESTIMATION_FAILED_MANYMISSING, _
                                       Me.m_Data.GroupName(i), Environment.NewLine)
                msg = New cMessage(strMsg, eMessageType.TooManyMissingParameters, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                msg.Suppressable = False
                NotifyCore(msg)

            Catch ex As Exception
                cLog.Write("Error in MsgManyMissingPar(). Error: " + ex.Message())
                Debug.Assert(False)
            End Try
        End Sub


        Private Sub EstimateTrophicLevels(ByVal iNumGroups As Integer, ByVal iNumLiving As Integer, _
                                          ByVal PP() As Single, ByVal Diet(,) As Single, ByVal TLreturn() As Single)

            Me.m_Ecofunctions.EstimateTrophicLevels(iNumGroups, iNumLiving, PP, Diet, TLreturn)

        End Sub

        Private Function CheckPredatorPreyTrophicLevels() As Boolean
            Dim i As Integer
            Dim j As Integer
            Dim dcsum As Single
            Dim RetVal As eMessageReply = eMessageReply.CANCEL
            Static done As Boolean
            Static DoneAlready As Boolean

            'JB WARNING
            'THIS HAS NEVER BEEN EXPLICITY TESTED!!!!!!!!!!!!!!!!!!
            'It was just copied from EWE5 this could be dangerous

            If DoneAlready = False Then
                EstimateTrophicLevels(m_Data.NumGroups, m_Data.NumLiving, m_Data.PP, m_Data.DC, m_Data.TTLX)
                'Now check if any group gets more than 15% of its consumption from a group that has >= m_data.ttlx as itself:
                For i = 1 To m_Data.NumLiving  'Consumers only
                    dcsum = 0
                    For j = 1 To m_Data.NumLiving 'only living groups can have a higher TL
                        If (m_Data.DC(i, j) > 0.0!) Then 'i eats j
                            If m_Data.TTLX(i) <= m_Data.TTLX(j) Then 'prey has higher TL
                                dcsum = dcsum + m_Data.DC(i, j)
                            End If
                        End If
                    Next
                    If dcsum > 0.151 Then  'there are some culprits with high consumption of
                        If done = False Then

                            ' Prepare message
                            Dim strMsg As String = cStringUtils.Localize(My.Resources.CoreMessages.DIETCOMP_PROMPT_CORRECTTO15PERC, m_Data.GroupName(i), CInt(dcsum * 100))
                            Dim msg As New cFeedbackMessage(strMsg, eCoreComponentType.EcoPath, eMessageType.DietComp_CorrectTo15Perc, eMessageImportance.Critical, eMessageReplyStyle.YES_NO_CANCEL)
                            msg.Suppressable = True

                            ' Send message
                            NotifyCore(msg)
                            RetVal = msg.Reply
                            If RetVal = eMessageReply.CANCEL Then done = True

                        Else
                            RetVal = eMessageReply.NO
                        End If
                        If RetVal = eMessageReply.YES Then
                            bDietsModified = True
                            DoneAlready = True
                            For j = 1 To m_Data.NumGroups
                                If m_Data.DC(i, j) > 0 Then
                                    'Debug.Print i, j, m_data.ttlx(i), m_data.ttlx(j), m_data.dc(i, j),
                                    If m_Data.TTLX(i) <= m_Data.TTLX(j) Then
                                        m_Data.DC(i, j) = CSng(m_Data.DC(i, j) * 0.15 / dcsum)
                                    Else
                                        m_Data.DC(i, j) = CSng(m_Data.DC(i, j) * 0.85 / (1 - dcsum))
                                    End If
                                    'Debug.Print m_data.dc(i, j)
                                End If
                            Next
                            m_Data.DC(i, 0) = CSng(m_Data.DC(i, 0) * 0.85 / (1 - dcsum))
                        End If
                    End If
                Next
                If DoneAlready = True Then
                    checkDietsSumToOne(True)
                Else
                    DoneAlready = True  'this has to be done, so it won't repeat this circus
                End If
            End If
        End Function


        Private Function DoIterationsToEstimateB() As Integer
            'VC programmed this function on 12 March 2002 to be able to estimate B for groups that are cannibalistic
            Dim i As Integer
            Dim j As Integer
            Dim Cnt As Long
            Dim GuessedBiomass() As Boolean
            Dim BIter() As Double   'B iterated
            Dim NewSum As Single
            Dim OldSum As Single
            Dim Only As Double
            Dim PartM2 As Double
            Dim NewB As Double
            Dim MaxBio As Double
            Dim Sum As Single
            'On Local Error GoTo exitSub
            'Dim NegativeB() As Boolean
            ReDim BIter(m_Data.NumLiving)
            'ReDim NegativeB(m_data.numliving)
            ReDim GuessedBiomass(m_Data.NumLiving)
            'This is based on EstimateB

            Try

                For j = 1 To m_Data.NumLiving 'If we only lack the Biomass then let's try to guess it
                    If m_Data.B(j) <= 0 And m_Data.PB(j) > 0 And m_Data.QB(j) > 0 And m_Data.EE(j) > 0 Then
                        GuessedBiomass(j) = True
                    Else
                        If m_Data.B(j) > MaxBio Then MaxBio = m_Data.B(j)
                    End If
                Next
                MaxBio = CDbl(if(MaxBio > 0, CSng(10 * MaxBio), 100))
                DoIterationsToEstimateB = 0
                NewSum = 0
                OldSum = -1
                Cnt = 0
                Do While Math.Abs(NewSum - OldSum) > 0.000001 And Cnt < 1000
                    OldSum = NewSum
                    NewSum = 0
                    Cnt = Cnt + 1
                    For j = 1 To m_Data.NumLiving
                        'If j = 2 Then Stop
                        If GuessedBiomass(j) Then   'Only do something if the biomass has been guessed
                            PartM2 = 0  'partM2 because cannibalism is excluded from calc.
                            For i = 1 To m_Data.NumLiving
                                If m_Data.DC(i, j) > 0 And i <> j Then 'this group, i, is a consumer,
                                    If m_Data.QB(i) < 0 Then  'we don't know the qb of this consumer so it won't work
                                        GoTo nextJ
                                    ElseIf GuessedBiomass(i) Then
                                        PartM2 = PartM2 + m_Data.QB(i) * m_Data.DC(i, j) * BIter(i)
                                    Else
                                        PartM2 = PartM2 + m_Data.QB(i) * m_Data.DC(i, j) * m_Data.B(i)
                                    End If
                                End If
                            Next i
                            '031220VC: modified to incorporate that BioAcc and emigration can be rates
                            Sum = CSng(if(m_Data.BaBi(j) <> 0 And m_Data.BA(j) = 0, m_Data.BaBi(j), 0))
                            Sum = Sum + CSng(if(m_Data.Emig(j) > 0 And m_Data.Emigration(j) = 0, m_Data.Emig(j), 0))
                            Sum = Sum * m_Data.B(j)
                            Only = m_Data.PB(j) * m_Data.EE(j) - m_Data.QB(j) * m_Data.DC(j, j) - Sum
                            If Only > 0 Then
                                NewB = (m_Data.fCatch(j) + m_Data.BA(j) + m_Data.Emigration(j) - m_Data.Immig(j) + PartM2) / Only
                                If NewB > MaxBio Then NewB = MaxBio
                                If NewB > 0 And Cnt > 4 And Math.Abs(NewB - BIter(j)) < 10 ^ -6 * BIter(j) And BIter(j) > 10 ^ -7 Then
                                    'get the biomasses that are OK
                                    GuessedBiomass(j) = False
                                    DoIterationsToEstimateB = DoIterationsToEstimateB + 1
                                    m_Data.B(j) = CSng(NewB)
                                End If
                                If NewB = 0 Then Return 0
                                BIter(j) = NewB
                                'If NewB > 100 Then Stop
                                'Debug.Print j; BIter(j);
                                If BIter(j) = 0 Then BIter(j) = 10 ^ -8
                            Else    'Only is negative = bad EE or too high cannibalism.
                                'If NegativeB(j) = False Then
                                '    MsgBox "Cannibalism exceeds production (P/B * EE > QB * DCii) for group " + CStr(j) + ", " + Specie(j)
                                '    NegativeB(j) = True
                                'End If
                                BIter(j) = 0
                            End If
                            If BIter(j) > 0 Then
                                NewSum = CSng(NewSum + BIter(j))
                            End If
                        End If
nextJ:
                    Next j
                    'Debug.Print
                Loop
                'DoIterationsToEstimateB = True
                'So transfer the values we have obtained through iteration
                For i = 1 To m_Data.NumLiving
                    If BIter(i) > 10 ^ -7 Then m_Data.B(i) = CSng(BIter(i))
                Next
            Catch ex As Exception
                Return 0
            End Try

        End Function

#End Region

        Private Sub dumpDietMatrix()

            Dim strm As New System.IO.StreamWriter("EcopathDietMatrix.csv", True)
            strm.WriteLine("iter")

            strm.WriteLine("-----------------Start Diet Matrix-----------------------")
            For iprey As Integer = 1 To Me.m_Data.NumGroups
                For ipred As Integer = 1 To Me.m_Data.NumLiving
                    strm.Write(Me.m_Data.DC(ipred, iprey).ToString() + ",")
                Next
                strm.WriteLine()
            Next
            strm.WriteLine("-----------------End Diet Matrix------------------------")
            strm.Close()

        End Sub


        Private Sub dumpInputPars()
            Try
                Dim strm As New System.IO.StreamWriter("EcopathPars.csv", True)
                strm.WriteLine("iter")
                For igrp As Integer = 1 To Me.m_Data.NumGroups
                    strm.WriteLine(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.m_Data.GroupName(igrp)) + "," + Me.m_Data.B(igrp).ToString + "," + Me.m_Data.PB(igrp).ToString + "," + Me.m_Data.QB(igrp).ToString + "," + Me.m_Data.EE(igrp).ToString)
                Next

                strm.Close()
            Catch ex As Exception

            End Try
        End Sub

    End Class

End Namespace
