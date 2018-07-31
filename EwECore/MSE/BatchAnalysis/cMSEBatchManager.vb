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

Imports System.IO

Imports EwECore.MSE
Imports EwEUtils.Core
Imports EwECore.MSECommandFile
Imports EwEUtils.Utilities


Namespace MSEBatchManager
    Public Enum eMSEBatchProgress
        MSEIteration
        RunStarted
        RunCompleted
    End Enum

    Public Enum eMSEBatchRunTypes
        Any = 0
        FixedF = 1
        TAC = 2
        TFM = 3
        NotManaged = 4
    End Enum

    Public Enum eMSEBatchOuputTypes
        NotSet
        Biomass
        QB 'consumption/biomass
        FeedingTime
        FishingMortRate
        PredRate
        CatchByGroup
        Effort
    End Enum

    Public Class cMSEBatchManager
        Inherits cThreadWaitBase
        Implements ICoreInterface

        Public Delegate Sub MSEBatchMessage(ByVal strMessage As String)
        Public Delegate Sub onMSEBatchProgress(ByVal ProgressEnum As eMSEBatchProgress)

#Region "Private data"

        Private Enum eBatchRunState
            Running
            Idle
        End Enum


        Private m_core As cCore
        Private m_fileReader As cMSECommandFileReader
        Private m_MSE As cMSE
        Private m_MSEdata As cMSEDataStructures
        Private m_BatchData As cMSEBatchDataStructures

        Private m_curForceIter As Integer

        Private m_msgDelegate As MSEBatchMessage
        Private m_OutputWriter As cMSEBatchOutputWriter

        Private m_thrdRun As System.Threading.Thread
        Private m_SyncOb As System.Threading.SynchronizationContext
        Private m_runState As eBatchRunState

        Private m_parameters As cMSEBatchParameters
        Private m_lstTFMs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEBatchTFMInput, 1)
        Private m_lstFixedFs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEBatchFixedFInput, 1)
        Private m_lstTACs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEBatchTACInput, 1)
        Private m_OnProgressDelegate As onMSEBatchProgress
#End Region

#Region "Construction, Initialization and Destruction"

        Public Sub Connect(ByRef OnProgress As onMSEBatchProgress)
            Me.m_OnProgressDelegate = Nothing
            Me.m_OnProgressDelegate = OnProgress
        End Sub

        Public Sub DisConnect()
            '  Me.m_OnProgressDelegate = Nothing
        End Sub

        Public Sub New()
            Me.m_SyncOb = System.Threading.SynchronizationContext.Current
            'if there is no current context then create a new one on this thread. 
            If (Me.m_SyncOb Is Nothing) Then Me.m_SyncOb = New System.Threading.SynchronizationContext()
        End Sub

        Public Sub Init(ByVal theCore As cCore, ByVal MSE As cMSE)

            If Me.m_SyncOb Is Nothing Then
                Me.m_SyncOb = System.Threading.SynchronizationContext.Current
                'if there is no current context then create a new one on this thread. 
                If (Me.m_SyncOb Is Nothing) Then Me.m_SyncOb = New System.Threading.SynchronizationContext()
            End If

            Me.m_runState = eBatchRunState.Idle

            Me.m_core = theCore
            Me.m_MSE = MSE
            Me.m_MSEdata = MSE.Data

            Me.m_BatchData = New cMSEBatchDataStructures(Me.m_MSEdata)
            Me.m_fileReader = New cMSECommandFileReader(Me.Core, Me)

            MSE.BatchManager = Me

            'Me.m_BatchData.redimForcing(1)
            'Me.m_BatchData.redimControlTypes(1, Me.m_core.nFleets)
            Me.m_BatchData.OuputDir = Me.m_core.OutputPath

            Try
                If (Me.Core.PluginManager IsNot Nothing) Then
                    Me.Core.PluginManager.MSEBatchInitialized(Me, Me.BatchData)
                End If
            Catch ex As Exception

            End Try

        End Sub


        Public Sub Clear()
            Try

                Me.m_BatchData = Nothing
                Me.m_fileReader = Nothing

                Me.m_msgDelegate = Nothing
                Me.m_SyncOb = Nothing

            Catch ex As Exception

            End Try
        End Sub

#End Region

#Region "Public Methods"

        Public Sub Run()

            Try

                If Not Me.m_BatchData.isInit Then
                    Me.MarshallMessage("MSE Batch cannot be run. Data failed to initialize.")
                    Return
                End If

                If Me.m_runState = eBatchRunState.Running Then
                    Me.MarshallMessage("MSE Batch already running, please wait for the current run to stop before trying again.")
                    Return
                End If

                If Me.m_SyncOb IsNot Nothing Then
                    m_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireProgress), eMSEBatchProgress.RunStarted)
                End If

                Me.SetWait()

                Me.update()

                m_thrdRun = New System.Threading.Thread(AddressOf Me.RunThreaded)
                m_thrdRun.Name = "MSEBatch.Run"
                m_thrdRun.Start()

            Catch ex As Exception

            End Try

        End Sub

        Public Sub LoadScenario()
            Try

                Me.setDefaults()

                Me.m_parameters = New cMSEBatchParameters(Me.m_core, Me.m_BatchData, Me.m_BatchData.ScenarioDBID)

                Me.m_lstTFMs.Clear()
                For igrp As Integer = 1 To Me.nGroups
                    Me.m_lstTFMs.Add(New cMSEBatchTFMGroup(Me.m_core, Me.m_BatchData, Me.m_BatchData.TFMDBIDs(igrp)))
                Next

                Me.m_lstFixedFs.Clear()
                For igrp As Integer = 1 To Me.nGroups
                    Me.m_lstFixedFs.Add(New cMSEBatchFGroup(Me.m_core, Me.m_BatchData, igrp))
                Next

                Me.m_lstTACs.Clear()
                For igrp As Integer = 1 To Me.nGroups
                    Me.m_lstTACs.Add(New cMSEBatchTACGroup(Me.m_core, Me.m_BatchData, igrp))
                Next

                'Load the values into the input objects
                Me.Load()

                'Calculate Iteration values base on defaults set above
                Me.CalculateTFMIterationValues()
                Me.CalculateFIterationValues()
                Me.CalculateTACIterationValues()


            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".LoadScenario() Exception: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Load core data into Input objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Load()

            Me.Parameters.AllowValidation = False
            Me.Parameters.nTFMIteration = Me.m_BatchData.nTFM
            Me.Parameters.nFixedFIteration = Me.m_BatchData.nFixedF
            Me.Parameters.nTACIteration = Me.m_BatchData.nTAC
            Me.Parameters.IterCalcType = Me.m_BatchData.IterCalcType

            Me.Parameters.bSaveBiomass = Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.Biomass)
            Me.Parameters.bSaveCatch = Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.CatchByGroup)
            Me.Parameters.bSaveFeedingTime = Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.FeedingTime)
            Me.Parameters.bSaveFishingMort = Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.FishingMortRate)
            Me.Parameters.bSaveFeedingTime = Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.FeedingTime)
            Me.Parameters.bSaveConsumptBio = Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.QB)

            Me.Parameters.OutputDir = Me.m_BatchData.OuputDir

            For igrp As Integer = 1 To Me.nGroups

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Target Fishing Mortality
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                Dim tfm As cMSEBatchTFMGroup = Me.m_lstTFMs.Item(igrp)
                tfm.AllowValidation = False

                tfm.Index = igrp
                tfm.Name = Me.m_core.m_EcoPathData.GroupName(igrp)

                tfm.BLim = Me.m_MSEdata.Blim(igrp)
                tfm.BLimLower = Me.m_BatchData.BlimLower(igrp)
                tfm.BLimUpper = Me.m_BatchData.BlimUpper(igrp)

                tfm.BBase = Me.m_MSEdata.Bbase(igrp)
                tfm.BBaseLower = Me.m_BatchData.BBaseLower(igrp)
                tfm.BBaseUpper = Me.m_BatchData.BBaseUpper(igrp)

                tfm.FMax = Me.m_MSEdata.Fopt(igrp)
                tfm.FMaxLower = Me.m_BatchData.FOptLower(igrp)
                tfm.FMaxUpper = Me.m_BatchData.FOptUpper(igrp)

                For it As Integer = 1 To Me.m_BatchData.nTFM
                    tfm.FMaxValue(it) = Me.m_BatchData.tfmFmax(it, igrp)
                    tfm.BLimValue(it) = Me.m_BatchData.tfmBlim(it, igrp)
                    tfm.BBaseValue(it) = Me.m_BatchData.tfmBbase(it, igrp)
                Next

                tfm.isManaged = (Me.m_BatchData.GroupRunType(igrp) = eMSEBatchRunTypes.TFM)

                tfm.ResetStatusFlags()
                tfm.AllowValidation = True

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Fixed Fishing Mortality
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                Dim FixedF As cMSEBatchFGroup = Me.m_lstFixedFs.Item(igrp)
                FixedF.AllowValidation = False

                FixedF.Index = igrp
                FixedF.Name = Me.m_core.m_EcoPathData.GroupName(igrp)

                FixedF.FixedMort = Me.m_MSEdata.FixedF(igrp)
                FixedF.FLower = Me.m_BatchData.FixedFLower(igrp)
                FixedF.FUpper = Me.m_BatchData.FixedFUpper(igrp)

                For it As Integer = 1 To Me.m_BatchData.nTFM
                    FixedF.FixedFValue(it) = Me.m_BatchData.FixedF(it, igrp)
                Next

                FixedF.ResetStatusFlags()
                FixedF.AllowValidation = True

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Total Allowable Catch
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                Dim TAC As cMSEBatchTACGroup = Me.m_lstTACs.Item(igrp)
                TAC.AllowValidation = False

                TAC.Index = igrp
                TAC.Name = Me.m_core.m_EcoPathData.GroupName(igrp)

                TAC.TAC = Me.m_MSEdata.TAC(igrp)
                TAC.TACLower = Me.m_BatchData.TACLower(igrp)
                TAC.TACUpper = Me.m_BatchData.TACUpper(igrp)

                'For it As Integer = 1 To Me.m_BatchData.nTFM
                '    TAC.TACValue(it) = Me.m_BatchData.TAC(it, igrp)
                'Next

                TAC.ResetStatusFlags()
                TAC.AllowValidation = True

            Next igrp

            Me.Parameters.AllowValidation = True
            Me.m_BatchData.isInit = True

            Me.Parameters.ResetStatusFlags()

        End Sub

        ''' <summary>
        ''' Update core data with data from input objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Update(DataType As eDataTypes, VarName As eVarNameFlags)
            Try

                If VarName = eVarNameFlags.MSETFMNIteration Then
                    Me.m_BatchData.nTFM = Me.Parameters.nTFMIteration
                    Me.m_BatchData.redimTFM(Me.m_BatchData.nTFM, Me.nGroups)
                    Me.UpdateNParameterIters()
                End If

                Me.update()

                If VarName = eVarNameFlags.MSEBatchIterCalcType Then
                    'swap the Upper and Lower limits between Percentage and Values
                    Me.SwapCalcType()
                    Load()

                    'maybe this will update the interface???? bitch....
                    Me.m_core.Messages.AddMessage(New cMessage("Update MSEBatch TFM.", eMessageType.DataModified, _
                                                               eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSEBatchTFMInput))
                End If

            Catch ex As Exception

            End Try

        End Sub

        Private Sub OnMSEProgress(ByVal RunStateType As eMSERunStates)

            If RunStateType = eMSERunStates.RunCompleted Then
                System.Console.WriteLine("MSEBatch Calling Interface.")
                If Me.m_SyncOb IsNot Nothing Then
                    m_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireProgress), eMSEBatchProgress.MSEIteration)
                End If
                System.Console.WriteLine("MSEBatch Interface completed.")
            End If

        End Sub

        Private Sub fireProgress(ob As Object)
            Try
                Me.m_OnProgressDelegate.Invoke(ob)
            Catch ex As Exception

            End Try
        End Sub



        Private Sub update()
            Dim igrp As Integer
            Dim irep As Integer
            Me.m_BatchData.nTFM = Me.Parameters.nTFMIteration
            Me.m_BatchData.IterCalcType = Me.Parameters.IterCalcType

            Me.m_BatchData.OuputDir = Me.Parameters.OutputDir

            Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.Biomass) = Me.Parameters.bSaveBiomass
            Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.CatchByGroup) = Me.Parameters.bSaveCatch
            Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.FeedingTime) = Me.Parameters.bSaveFeedingTime
            Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.FishingMortRate) = Me.Parameters.bSaveFishingMort
            Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.FeedingTime) = Me.Parameters.bSaveFeedingTime
            Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.QB) = Me.Parameters.bSaveConsumptBio
            'Me.m_BatchData.isOuputSaved(eMSEBatchOuputTypes.Effort) = Me.Parameters.bsaveEffort

            Dim t As eMSEBatchOuputTypes
            For iout As Integer = 1 To Me.m_BatchData.nOuputTypes
                t = eMSEBatchOuputTypes.NotSet
                If Me.m_BatchData.isOuputSaved(iout) Then
                    t = iout
                End If
                Me.m_BatchData.OuputType(iout) = t
            Next

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'TFM
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            For igrp = 1 To Me.nGroups
                'TFM's
                Dim tfm As cMSEBatchTFMGroup = Me.m_lstTFMs.Item(igrp)
                Me.m_MSEdata.Blim(igrp) = tfm.BLim
                Me.m_BatchData.BlimLower(igrp) = tfm.BLimLower
                Me.m_BatchData.BlimUpper(igrp) = tfm.BLimUpper

                Me.m_MSEdata.Bbase(igrp) = tfm.BBase
                Me.m_BatchData.BBaseLower(igrp) = tfm.BBaseLower
                Me.m_BatchData.BBaseUpper(igrp) = tfm.BBaseUpper

                Me.m_MSEdata.Fopt(igrp) = tfm.FMax
                Me.m_BatchData.FOptLower(igrp) = tfm.FMaxLower
                Me.m_BatchData.FOptUpper(igrp) = tfm.FMaxUpper

                'isManaged The concept is to set this flag for the type of Quota TMF, F, TAC... for a group
                'However the MSE does not work this way
                'It sets the Quota for a group base on > zero values in F, TAC see cMMSEUpdateQuotas
                'Here we need some kind of a flag that tells what type to use when setting the Quota
                If tfm.isManaged Then
                    'this should be exclusive only TFM should have its isManaged Flag set to True
                    'Other Quota types should update to the new value
                    Me.m_BatchData.GroupRunType(igrp) = eMSEBatchRunTypes.TFM

                End If

                For iTFM As Integer = 1 To Me.m_BatchData.nTFM
                    Me.m_BatchData.tfmBlim(iTFM, igrp) = tfm.BLimValue(iTFM)
                    Me.m_BatchData.tfmBbase(iTFM, igrp) = tfm.BBaseValue(iTFM)
                    Me.m_BatchData.tfmFmax(iTFM, igrp) = tfm.FMaxValue(iTFM)
                Next iTFM

            Next igrp

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Fixed F
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            For igrp = 1 To Me.nGroups
                'Fixed F
                Dim FixedF As cMSEBatchFGroup = Me.m_lstFixedFs.Item(igrp)
                Me.m_MSEdata.FixedF(igrp) = FixedF.FixedMort
                Me.m_BatchData.FixedFLower(igrp) = FixedF.FLower
                Me.m_BatchData.FixedFUpper(igrp) = FixedF.FUpper


                'isManaged The concept is to set this flag for the type of Quota TMF, F, TAC... for a group
                'However the MSE does not work this way
                'It sets the Quota for a group base on > zero values in F, TAC see cMMSEUpdateQuotas
                'Here we need some kind of a flag that tells what type to use when setting the Quota
                If FixedF.isManaged Then
                    'this should be exclusive only TFM should have its isManaged Flag set to True
                    'Other Quota types should update to the new value
                    Me.m_BatchData.GroupRunType(igrp) = eMSEBatchRunTypes.FixedF

                End If

                For iFixed As Integer = 1 To Me.m_BatchData.nFixedF
                    Me.m_BatchData.tfmBlim(iFixed, igrp) = FixedF.FixedFValue(iFixed)
                Next iFixed

            Next igrp

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'TAC
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            For igrp = 1 To Me.nGroups

                Dim tac As cMSEBatchTACGroup = Me.m_lstTACs.Item(igrp)
                Me.m_MSEdata.TAC(igrp) = tac.TAC
                Me.m_BatchData.TACLower(igrp) = tac.TACLower
                Me.m_BatchData.TACUpper(igrp) = tac.TACUpper


                'isManaged The concept is to set this flag for the type of Quota TMF, F, TAC... for a group
                'However the MSE does not work this way
                'It sets the Quota for a group base on > zero values in F, TAC see cMMSEUpdateQuotas
                'Here we need some kind of a flag that tells what type to use when setting the Quota
                If tac.isManaged Then
                    'this should be exclusive only TFM should have its isManaged Flag set to True
                    'Other Quota types should update to the new value
                    Me.m_BatchData.GroupRunType(igrp) = eMSEBatchRunTypes.TAC

                End If

                For iFixed As Integer = 1 To Me.m_BatchData.nFixedF
                    Me.m_BatchData.TAC(iFixed, igrp) = tac.TACValue(iFixed)
                Next iFixed

            Next igrp

            'Control types
            Me.m_BatchData.redimControlTypes(Me.m_BatchData.nControlTypes, Me.nFleets)
            For icon As Integer = 1 To Me.m_BatchData.nControlTypes
                For iflt As Integer = 1 To Me.m_MSEdata.nFleets
                    'set the control type to what ever is currently loaded
                    Me.BatchData.ControlType(icon, iflt) = Me.m_MSEdata.QuotaType(iflt)
                Next iflt
            Next icon

            'Clear out the FixedF and TAC values if the GroupRunType is TFM
            For igrp = 1 To Me.nGroups
                If Me.m_BatchData.GroupRunType(igrp) = eMSEBatchRunTypes.TFM Then
                    'clear out all the other Quota 
                    For irep = 1 To Me.m_BatchData.nFixedF
                        Me.m_BatchData.FixedF(irep, igrp) = 0.0
                    Next irep

                    For irep = 1 To Me.m_BatchData.nTAC
                        Me.m_BatchData.TAC(irep, igrp) = 0.0
                    Next irep

                End If
            Next

        End Sub


        Private Sub calcIterationValues(iGroup As Integer, Value As Single, LowPercent As Single, UPPercent As Single, n As Integer, CalcType As eMSEBatchIterCalcTypes, ByRef values(,) As Single)

            Try

                Dim LowB As Single, UpB As Single
                Dim dx As Single

                If CalcType = eMSEBatchIterCalcTypes.Percent Then
                    LowB = Value - Value * LowPercent
                    UpB = Value + Value * UPPercent
                    dx = (UpB - LowB) / (n - 1)

                ElseIf CalcType = eMSEBatchIterCalcTypes.UpperLowerValues Then
                    LowB = LowPercent
                    UpB = UPPercent
                    dx = (UpB - LowB) / (n - 1)

                End If

                For i As Integer = 1 To n
                    values(i, iGroup) = LowB + dx * (i - 1)
                Next
            Catch ex As Exception

            End Try

        End Sub


        Public Sub CalculateTFMIterationValues()

            Try

                For igrp As Integer = 1 To Me.m_BatchData.nGroups
                    Me.calcIterationValues(igrp, Me.m_MSEdata.Fopt(igrp), Me.m_BatchData.FOptLower(igrp), Me.m_BatchData.FOptUpper(igrp), _
                                        Me.m_BatchData.nTFM, Me.m_BatchData.IterCalcType, Me.m_BatchData.tfmFmax)

                    Me.calcIterationValues(igrp, Me.m_MSEdata.Blim(igrp), Me.m_BatchData.BlimLower(igrp), Me.m_BatchData.BlimUpper(igrp), _
                                        Me.m_BatchData.nTFM, Me.m_BatchData.IterCalcType, Me.m_BatchData.tfmBlim)

                    Me.calcIterationValues(igrp, Me.m_MSEdata.Bbase(igrp), Me.m_BatchData.BBaseLower(igrp), Me.m_BatchData.BBaseUpper(igrp), _
                                        Me.m_BatchData.nTFM, Me.m_BatchData.IterCalcType, Me.m_BatchData.tfmBbase)

                Next

                Me.Load()

                Me.m_core.Messages.SendMessage(New cMessage("Values update.", eMessageType.MSEBatch_IterationDataUpdated, eCoreComponentType.MSE, _
                                                            eMessageImportance.Maintenance, eDataTypes.MSEBatchTFMInput))


            Catch ex As Exception

            End Try

        End Sub

        Public Sub CalculateFIterationValues()

            For igrp As Integer = 1 To Me.m_BatchData.nGroups
                Me.calcIterationValues(igrp, Me.m_MSEdata.FixedF(igrp), Me.m_BatchData.FixedFLower(igrp), Me.m_BatchData.FixedFUpper(igrp), _
                                    Me.m_BatchData.nFixedF, Me.m_BatchData.IterCalcType, Me.m_BatchData.FixedF)

            Next

            Me.Load()

            Me.m_core.Messages.SendMessage(New cMessage("Values update.", eMessageType.MSEBatch_IterationDataUpdated, eCoreComponentType.MSE, _
                                                        eMessageImportance.Maintenance, eDataTypes.MSEBatchFixedFInput))

        End Sub


        Public Sub CalculateTACIterationValues()


            For igrp As Integer = 1 To Me.m_BatchData.nGroups
                Me.calcIterationValues(igrp, Me.m_MSEdata.TAC(igrp), Me.m_BatchData.TACLower(igrp), Me.m_BatchData.TACUpper(igrp), _
                                    Me.m_BatchData.nTAC, Me.m_BatchData.IterCalcType, Me.m_BatchData.TAC)

            Next

            Me.Load()

            Me.m_core.Messages.SendMessage(New cMessage("Values update.", eMessageType.MSEBatch_IterationDataUpdated, eCoreComponentType.MSE, _
                                                        eMessageImportance.Maintenance, eDataTypes.MSEBatchTACInput))

        End Sub

        ''' <summary>
        ''' Sets Default values that are needed when the MSE Batch is run from the interface 
        ''' instead of the Batch Command file
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub setDefaults()

            Me.m_BatchData.redimTFM(10, Me.nGroups)
            Me.m_BatchData.redimFixedF(10, Me.nGroups)
            Me.m_BatchData.redimTAC(10, Me.nGroups)

            Me.m_BatchData.redimControlTypes(1, Me.m_core.nFleets)
            Me.m_BatchData.redimForcing(1)

            Me.m_BatchData.setDefaultLimits()

            Me.m_BatchData.nParIters = Me.m_BatchData.nTFM
            Me.m_BatchData.RunType = eMSEBatchRunTypes.Any

            Me.m_BatchData.bForcingLoaded = False

            Me.m_BatchData.OuputDir = Me.m_core.OutputPath

        End Sub


        ''' <summary>
        ''' Update the Input/Ouput objects to the number of Iteration set by the interface
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub UpdateNParameterIters()

            ' Debug.Assert(False, "UpdateNParameterIters Still needs to be implemented.")
            'For Each grp As cMSETFMGroup In Me.m_lstTFMs
            '    grp.updateN()
            'Next

        End Sub

        Private Sub SwapCalcType()

            For igrp As Integer = 1 To nGroups

                If Me.m_BatchData.IterCalcType = eMSEBatchIterCalcTypes.Percent Then
                    ToPercent(Me.m_MSEdata.Blim(igrp), Me.m_BatchData.BlimLower(igrp), Me.m_BatchData.BlimUpper(igrp))
                    ToPercent(Me.m_MSEdata.Bbase(igrp), Me.m_BatchData.BBaseLower(igrp), Me.m_BatchData.BBaseUpper(igrp))
                    ToPercent(Me.m_MSEdata.Fopt(igrp), Me.m_BatchData.FOptLower(igrp), Me.m_BatchData.FOptUpper(igrp))
                Else
                    ToValue(Me.m_MSEdata.Blim(igrp), Me.m_BatchData.BlimLower(igrp), Me.m_BatchData.BlimUpper(igrp))
                    ToValue(Me.m_MSEdata.Bbase(igrp), Me.m_BatchData.BBaseLower(igrp), Me.m_BatchData.BBaseUpper(igrp))
                    ToValue(Me.m_MSEdata.Fopt(igrp), Me.m_BatchData.FOptLower(igrp), Me.m_BatchData.FOptUpper(igrp))
                End If

            Next

            Me.m_core.Messages.SendMessage(New cMessage("Values update.", eMessageType.DataModified, eCoreComponentType.MSE, _
                                                       eMessageImportance.Maintenance, eDataTypes.MSEBatchTFMInput))

        End Sub

        Private Sub ToValue(ByVal mean As Single, ByRef Lower As Single, ByRef Upper As Single)
            Lower = mean - mean * Lower
            Upper = mean + mean * Upper
        End Sub


        Private Sub ToPercent(ByVal mean As Single, ByRef Lower As Single, ByRef Upper As Single)
            Lower = (mean - Lower) / mean
            Upper = (Upper - mean) / mean
        End Sub


        ''' <summary>
        ''' Vary the Primary Production forcing function value of the current time step
        ''' </summary>
        ''' <param name="ForcingMultTime"></param>
        ''' <remarks></remarks>
        Public Sub varyForcing(ByRef ForcingMultTime() As Single)

            If Not Me.BatchData.bForcingLoaded Then
                Return
            End If

            Dim iGrp As Integer = Me.BatchData.ForcingGroup(Me.m_curForceIter)
            Dim simData As cEcosimDatastructures = Me.m_core.m_EcoSimData

            For ifn As Integer = 1 To cMediationDataStructures.MAXFUNCTIONS
                'are there any forcing functions set
                If simData.BioMedData.FunctionNumber(iGrp, iGrp, ifn) = 0 Then Exit For

                'is this the same function as the command file loaded
                If simData.BioMedData.FunctionNumber(iGrp, iGrp, ifn) = Me.BatchData.ForcingIndexes(Me.m_curForceIter) Then
                    Dim iforce As Integer = simData.BioMedData.FunctionNumber(iGrp, iGrp, ifn)
                    'Yes vary the forcing data for this timestep
                    'System.Console.Write("tval=" & tval(iforce).ToString & ", ")
                    ForcingMultTime(iforce) = Me.m_MSE.RandNormDist(Me.BatchData.STDevForcing, ForcingMultTime(iforce))

                    'constrain the forcing value to >= zero
                    If ForcingMultTime(iforce) < 0 Then ForcingMultTime(iforce) = 0
                    'System.Console.WriteLine("varied=" & tval(iforce).ToString)

                End If
            Next ifn

        End Sub


        Public Function ReadCommandFile(ByVal CommandFileName As String) As Boolean

            If Me.m_runState = eBatchRunState.Running Then
                'message can't run
                Return False
            End If

            Me.BatchData.CommandFilename = CommandFileName

            If Me.m_fileReader.Read(CommandFileName) Then
                If Me.ValidateData() Then
                    If Me.updateDataStructures() Then
                        Me.checkRunType()
                        Me.postValidationMessage()
                        Me.BatchData.isInit = True
                        Return True
                    End If
                End If
                Return False
            End If

            Me.BatchData.isInit = False
            'message failed to read file
            Return False

        End Function

#End Region

#Region "Private Methods"

        Private Function ValidateData() As Boolean
            Dim bSuccess As Boolean
            Try

                bSuccess = Me.m_fileReader.ValidateData()
                If Not bSuccess Then
                    'Failed validation
                    Me.MarshallMessage("Command file failed validation.")
                    Me.MarshallMessage("Please fix all errors in the command file and try again.")
                    ' Me.MarshallMessage("")
                End If

            Catch ex As Exception
                Me.MarshallMessage("Error while validating command file data. " & ex.Message)
                bSuccess = False
            End Try

            Return bSuccess


        End Function


        Private Sub setForcing(ByVal iForcing As Integer)
            If Me.BatchData.bForcingLoaded Then
                Me.LoadPPForcing(Me.BatchData.ForcingIndexes(iForcing), Me.BatchData.ForcingGroup(iForcing))
            End If

        End Sub

        Private Sub setControls(ByVal iControlIndex As Integer)

            For iflt As Integer = 1 To Me.m_MSEdata.nFleets
                Me.m_MSEdata.QuotaType(iflt) = Me.BatchData.ControlType(iControlIndex, iflt)
            Next iflt

        End Sub

        ''' <summary>
        ''' Set Quota parameters e.g F, TFM... according to the RunType
        ''' </summary>
        ''' <param name="iParIter"></param>
        ''' <remarks>
        ''' If the RunType is Any then set any of the parameters to the values in the command file. 
        ''' For all other RunTypes set the other parameters to zero so they will not be used. </remarks>
        Private Sub setParameters(ByVal iParIter As Integer)
            Dim igrp As Integer
            Dim blim As Single, bbase As Single, fmin As Single, fmax As Single
            Dim f As Single, tac As Single
            Me.m_BatchData.iCurRun = iParIter

            For igrp = 1 To Me.m_MSEdata.NGroups

                Select Case Me.BatchData.RunType

                    Case eMSEBatchRunTypes.TFM
                        blim = Me.BatchData.tfmBlim(iParIter, igrp)
                        bbase = Me.BatchData.tfmBbase(iParIter, igrp)
                        fmax = Me.BatchData.tfmFmax(iParIter, igrp)
                        fmin = Me.BatchData.tfmFmin(iParIter, igrp)

                    Case eMSEBatchRunTypes.FixedF
                        f = Me.BatchData.FixedF(iParIter, igrp)

                    Case eMSEBatchRunTypes.TAC
                        tac = Me.BatchData.TAC(iParIter, igrp)

                    Case eMSEBatchRunTypes.Any
                        'If RunType is Any then set Any of the MSE parameters to values from the command file 
                        'If the parameter iteration is > number of parameters then use the last value from the file
                        Select Case Me.m_BatchData.GroupRunType(igrp)

                            Case eMSEBatchRunTypes.TFM

                                Dim itfm As Integer = Math.Min(iParIter, Me.BatchData.nTFM)
                                blim = Me.BatchData.tfmBlim(itfm, igrp)
                                bbase = Me.BatchData.tfmBbase(itfm, igrp)
                                fmax = Me.BatchData.tfmFmax(itfm, igrp)
                                fmin = Me.BatchData.tfmFmin(itfm, igrp)

                            Case eMSEBatchRunTypes.FixedF

                                Dim iFx As Integer = Math.Min(iParIter, Me.BatchData.nFixedF)
                                f = Me.BatchData.FixedF(iFx, igrp)

                            Case eMSEBatchRunTypes.TAC
                                Dim itac As Integer = Math.Min(iParIter, Me.BatchData.nTAC)
                                tac = Me.BatchData.TAC(itac, igrp)

                        End Select

                End Select

                'set the values based on the RunType selected above
                Me.m_MSEdata.Blim(igrp) = blim
                Me.m_MSEdata.Bbase(igrp) = bbase
                Me.m_MSEdata.Fopt(igrp) = fmax
                Me.m_MSEdata.Fmin(igrp) = fmin

                Me.m_MSEdata.FixedF(igrp) = f
                Me.m_MSEdata.TAC(igrp) = tac
                Me.m_MSEdata.FixedEscapement(igrp) = 0

            Next


        End Sub

        Private Sub postValidationMessage()

            Try

                Me.MarshallMessage("")
                Me.MarshallMessage("Run Type:")
                Me.MarshallMessage(cStringUtils.vbTab & Me.BatchData.RunType.ToString)

                Me.MarshallMessage("Output directory:")
                Me.MarshallMessage(cStringUtils.vbTab & Me.BatchData.OuputDir)

                Dim endYearMsg As String = "End of Ecosim run."
                If Me.MSEData.EndYear > 0 Then
                    endYearMsg = Me.MSEData.EndYear.ToString
                End If

                Me.MarshallMessage("Last control year:")
                Me.MarshallMessage(cStringUtils.vbTab & endYearMsg)

                Me.MarshallMessage("Primary production variation:")
                Me.MarshallMessage(cStringUtils.vbTab & Me.BatchData.STDevForcing.ToString)

                Me.MarshallMessage("Loaded primary production forcing:")
                For iff As Integer = 1 To Me.BatchData.nForcing
                    Me.MarshallMessage(cStringUtils.vbTab & Me.BatchData.ForcingNames(iff))
                Next

                ' Me.checkRunType()

                Me.MarshallMessage("Ready to run.")

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Private Sub checkRunType()
            Dim igrp As Integer
            Dim bFixedEsc As Boolean
            Dim bFixedF As Boolean
            Dim bTAC As Boolean

            Select Case Me.BatchData.RunType

                Case eMSEBatchRunTypes.TFM

                    For igrp = 1 To Me.MSEData.NGroups
                        If Me.m_MSEdata.FixedEscapement(igrp) <> 0 Then bFixedEsc = True
                        If Me.m_MSEdata.FixedF(igrp) <> 0 Then bFixedF = True
                        If Me.m_MSEdata.TAC(igrp) <> 0 Then bTAC = True
                    Next

                Case eMSEBatchRunTypes.TAC
                    For igrp = 1 To Me.MSEData.NGroups
                        If Me.m_MSEdata.FixedEscapement(igrp) <> 0 Then bFixedEsc = True
                        If Me.m_MSEdata.FixedF(igrp) <> 0 Then bFixedF = True
                    Next

                Case eMSEBatchRunTypes.FixedF
                    For igrp = 1 To Me.MSEData.NGroups
                        If Me.m_MSEdata.FixedEscapement(igrp) <> 0 Then bFixedEsc = True
                        If Me.m_MSEdata.TAC(igrp) <> 0 Then bTAC = True
                    Next

            End Select


            If bFixedEsc Or bFixedF Or bTAC Then
                Me.MarshallMessage("WARNING: values for")

                If bFixedEsc Then
                    Me.MarshallMessage(cStringUtils.vbTab & "Fixed Escapement")
                End If
                If bFixedF Then
                    Me.MarshallMessage(cStringUtils.vbTab & "Fixed F")
                End If
                If bTAC Then
                    Me.MarshallMessage(cStringUtils.vbTab & "Total Allowable Catch")
                End If

                Me.MarshallMessage(cStringUtils.vbTab & "Have been set in the user interface.")
                Me.MarshallMessage(cStringUtils.vbTab & "Please check these value(s) in the user interface to make sure this is correct.")

            End If


        End Sub


        Private Sub RunThreaded()
            Dim bSuccess As Boolean

            Try

                Me.m_MSE.Connect(AddressOf Me.OnMSEProgress, Nothing)

                cLog.Write("MSE batch run started.")

                Me.BatchData.StoreMSEState(Me.MSEData)

                Me.m_runState = eBatchRunState.Running
                Me.m_MSEdata.bInBatch = True
                Me.BatchData.StopRun = False

                Me.MarshallMessage("Starting batch run.")

                Me.m_OutputWriter = New cMSEBatchOutputWriter(Me.Core, Me.m_MSEdata, Me.BatchData)
                Me.m_OutputWriter.InitBatchRun()
                Me.m_OutputWriter.WriteBatchHeader()

                Dim n As Integer = Me.m_BatchData.nForcing * Me.m_BatchData.nControlTypes * Me.m_BatchData.nParIters
                Dim iter As Integer

                For iFor As Integer = 1 To Me.m_BatchData.nForcing
                    Me.m_curForceIter = iFor
                    Me.setForcing(iFor)

                    For iCon As Integer = 1 To Me.m_BatchData.nControlTypes
                        Me.setControls(iCon)

                        For iPar As Integer = 1 To Me.m_BatchData.nParIters
                            Me.setParameters(iPar)
                            Me.m_OutputWriter.WriteIterationHeader(iFor, iCon, iPar)
                            Me.m_OutputWriter.setSimCounter()

                            iter += 1
                            bSuccess = Me.m_MSE.Run()
                            If Not bSuccess Then
                                Me.MarshallMessage("   MSE Error run " & iter.ToString & " of " & n.ToString & " run stopped.")
                                Me.BatchData.StopRun = True
                                Exit For
                            End If
                            Me.MarshallMessage("   Completed " & iter.ToString & " of " & n.ToString)

                            If Me.BatchData.StopRun Then Exit For

                            GC.Collect()

                        Next iPar

                        If Me.BatchData.StopRun Then Exit For
                    Next iCon

                    If Me.BatchData.StopRun Then Exit For
                Next iFor

            Catch ex As Exception

                cLog.Write(ex)
                Me.MarshallMessage("MSE Batch run Exception: " & ex.Message)

            End Try

            Me.m_MSEdata.bInBatch = False
            Me.m_runState = eBatchRunState.Idle

            Me.BatchData.ReStoreMSEState(Me.MSEData)

            Dim msg As String = "Batch run completed."
            If Me.BatchData.StopRun Then msg = "Batch run stopped."

            Me.MarshallMessage(msg)

            Me.ReleaseWait()

            If Me.m_SyncOb IsNot Nothing Then
                m_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireProgress), eMSEBatchProgress.RunCompleted)
            End If

        End Sub

        Private Function updateDataStructures() As Boolean

            Return Me.m_fileReader.updateDataStructures()

        End Function

        Friend ReadOnly Property OutputWriter() As IMSEOutputWriter
            Get
                Return Me.m_OutputWriter
            End Get
        End Property

#End Region

#Region "Properties"

        Public ReadOnly Property Parameters As cMSEBatchParameters
            Get
                Return Me.m_parameters
            End Get
        End Property

        Public ReadOnly Property TFMGroups(GroupIndex As Integer) As cMSEBatchTFMGroup
            Get
                Return Me.m_lstTFMs.Item(GroupIndex)
            End Get
        End Property

        Public ReadOnly Property FixedFGroups(GroupIndex As Integer) As cMSEBatchFGroup
            Get
                Return Me.m_lstFixedFs.Item(GroupIndex)
            End Get
        End Property


        Public ReadOnly Property TACGroups(GroupIndex As Integer) As cMSEBatchTACGroup
            Get
                Return Me.m_lstTACs.Item(GroupIndex)
            End Get
        End Property


        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>JS: changed to public to make sure existing plug-ins keep working</remarks>
        Public ReadOnly Property BatchData() As cMSEBatchDataStructures
            Get
                Return Me.m_BatchData
            End Get
        End Property


        Friend ReadOnly Property Core() As cCore
            Get
                Return Me.m_core
            End Get
        End Property


        Friend ReadOnly Property MSEData() As cMSEDataStructures
            Get
                Return Me.m_MSEdata
            End Get
        End Property


        Friend ReadOnly Property nGroups() As Integer
            Get
                Return Me.m_MSEdata.NGroups
            End Get
        End Property


        Friend ReadOnly Property nFleets() As Integer
            Get
                Return Me.m_MSEdata.nFleets
            End Get
        End Property

        ''' <summary>
        ''' Load an existing Primary Production forcing function 
        ''' </summary>
        ''' <param name="iShapeIndex">Index of the Forcing Function shape</param>
        ''' <param name="iPPGroupIndex">Ecosim index of the Primary Production group this forcing function applies to</param>
        ''' <remarks></remarks>
        Private Sub LoadPPForcing(ByVal iShapeIndex As Integer, ByVal iPPGroupIndex As Integer)

            'no forcing data loaded from the command file
            If Not BatchData.bForcingLoaded Then Exit Sub

            'shapes are held in a list Indexed from 0
            If iShapeIndex < 1 Or iPPGroupIndex < 1 Then
                'no shape and or PredPrey index defined
                'This will happen if the user does not want to alter the PP Forcing 
                Return
            End If

            iShapeIndex -= 1
            Dim shape As cForcingFunction = Me.m_core.ForcingShapeManager.Item(iShapeIndex)
            Debug.Assert(shape IsNot Nothing, "Invalid PP forcing index.")

            Dim ppi As cMediatedInteraction = Me.m_core.MediatedInteractionManager.PredPreyInteraction(iPPGroupIndex, iPPGroupIndex)
            ppi.LockUpdates = True
            ' Clear all shapes
            For i As Integer = 1 To ppi.MaxNumShapes
                ppi.setShape(i, Nothing)
            Next
            ' Set appropriate shape
            ppi.setShape(1, shape, eForcingFunctionApplication.ProductionRate)

            'Updates the underlying Ecosim data
            ppi.LockUpdates = False


        End Sub

        Public WriteOnly Property onMessageDelegate() As MSEBatchMessage
            Set(ByVal value As MSEBatchMessage)
                Me.m_msgDelegate = value
            End Set
        End Property


        Public Sub MarshallMessage(ByVal message As String)
            Try
                Debug.Assert((Me.m_msgDelegate IsNot Nothing) And (Me.m_SyncOb IsNot Nothing), Me.ToString & ".MarshallMessage() not initialized correctly")
                If (Me.m_msgDelegate IsNot Nothing) And (Me.m_SyncOb IsNot Nothing) Then
                    'marshall the message onto the main thread
                    m_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf sendMessage), message)
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub sendMessage(ByVal obj As Object)
            Try
                Dim message As String = DirectCast(obj, String)
                Me.m_msgDelegate.Invoke(message)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & " Error sending message to interface.")
            End Try
        End Sub


        Public ReadOnly Property TFMInputs(ByVal iGroup As Integer) As cMSEBatchTFMGroup
            Get
                Return DirectCast(Me.m_lstTFMs(iGroup), cMSEBatchTFMGroup)
            End Get
        End Property

#End Region

#Region "ICoreInterface implementation"


        Public ReadOnly Property CoreComponent As EwEUtils.Core.eCoreComponentType Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.MSE '??? maybe needs its own component type
            End Get
        End Property

        Public ReadOnly Property DataType As EwEUtils.Core.eDataTypes Implements ICoreInterface.DataType
            Get
                Return eDataTypes.MSEBatchManager
            End Get
        End Property

        Public Property DBID As Integer Implements ICoreInterface.DBID
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(value As Integer)

            End Set
        End Property

        Public Function GetID() As String Implements ICoreInterface.GetID
            Return cValueID.GetDataTypeID(Me.DataType, Me.DBID)
        End Function

        Public Property Index As Integer Implements ICoreInterface.Index
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(value As Integer)

            End Set
        End Property

        Public Property Name As String Implements ICoreInterface.Name
            Get
                Return Me.ToString
            End Get
            Set(value As String)

            End Set
        End Property

#End Region

        Public Overrides Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean ' Implements SearchObjectives.ISearchObjective.StopRun
            Dim result As Boolean = True

            Try
                'Me.m_core.m_EcoSim.bStopRunning = True
                Me.m_MSEdata.StopRun = True
                Me.m_BatchData.StopRun = True

                'Do Until Me.Wait(10)
                '    System.Windows.Forms.Application.DoEvents()
                'Loop

            Catch ex As Exception
                result = False
            End Try
            Return result
        End Function

    End Class


End Namespace
