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

Option Strict On

Imports System.Threading
Imports EwECore.SearchObjectives
Imports EwEUtils.Core
Imports EwEUtils.Utilities

Namespace MSE

    Public Enum eMSEStatNames
        Min
        Max
        Values
        CV
        Std
        Mean
        nBins
        BinWidth
        PercentageHistogram
        MeanRun
        AboveLimit
        BelowLimit
        nIterations
    End Enum

    ''' <summary>
    ''' Manager class for the MSE (Closed loop simulator in EwE5)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cMSEManager
        Inherits cThreadWaitBase
        Implements ICoreInterface
        Implements ISearchObjective


#Region "Private data"

        Private m_core As cCore
        Private m_MSE As cMSE
        Private m_MSEdata As cMSEDataStructures
        Private m_search As cSearchDatastructures
        Private m_searchObjective As cSearchObjective

        Private m_Batch As MSEBatchManager.cMSEBatchManager

        Private m_MSECallback As MSEProgressDelegate
        Private m_MSYCallback As MSYProgressDelegate
        Private m_SyncOb As System.Threading.SynchronizationContext
        Private m_bConnected As Boolean

        Private m_lstGroupInputs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEGroupInput, 1)
        Private m_lstEcopathFleetInputs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEFleetInput, 1)
        Private m_lstGroupOutputs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEGroupOutputs, 1)
        Private m_lstFleetOutputs As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEFleetOutputs, 1)

        Private m_lstBiomassStats As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEBiomassStats, 1)
        Private m_lstGroupCatchStats As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSECatchByGroupStats, 1)
        Private m_lstEffortStats As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEEffortStats, 1)
        Private m_lstFleetStats As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEEffortStats, 1)

        Private m_lstBioEstStats As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEBiomassStats, 1)

        Private m_lstFStats As New cCoreInputOutputList(Of cCoreInputOutputBase)(eDataTypes.MSEFStats, 1)

        Private m_TotFleetValue As cMSEStats

        Private m_output As cMSEOutput
        Private m_parameters As cMSEParameters

        Private m_thrdRunMSE As Thread
        Private m_thrdRunMSY As Thread

        Private m_VarToStat As New Dictionary(Of eVarNameFlags, eMSEStatNames)

#End Region

#Region "Connection and Disconnection"

        Public Sub Connect(ByRef OnMSE As MSEProgressDelegate, ByRef OnMSY As MSYProgressDelegate)

            Me.m_MSECallback = OnMSE
            Me.m_MSYCallback = OnMSY
            'MSE does not listen to the Ecosim timesteps
            Me.m_core.m_EcoSim.TimeStepDelegate = Nothing
            Me.m_bConnected = True

        End Sub


        Public Sub Disconnect()

            'm_MSECallback = Nothing
            'm_MSYCallback = Nothing
            m_bConnected = False

        End Sub

#End Region

#Region "Public Properties and Methods"

        Public Sub UpdateAssesmentVars()

            ' Me.m_MSE.InitAssessment()

        End Sub

        Public ReadOnly Property EcopathFleetInputs(ByVal iFleet As Integer) As cMSEFleetInput
            Get
                Return DirectCast(Me.m_lstEcopathFleetInputs(iFleet), cMSEFleetInput)
            End Get
        End Property

        Public ReadOnly Property FleetOutputs(ByVal iFleet As Integer) As cMSEFleetOutput
            Get
                Return DirectCast(Me.m_lstFleetOutputs(iFleet), cMSEFleetOutput)
            End Get
        End Property

        Public ReadOnly Property FleetOutputs() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstFleetOutputs
            End Get
        End Property

        Public ReadOnly Property GroupInputs(ByVal iGroup As Integer) As cMSEGroupInput
            Get
                Return DirectCast(Me.m_lstGroupInputs(iGroup), cMSEGroupInput)
            End Get
        End Property

        Friend ReadOnly Property GroupInputs() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstGroupInputs
            End Get
        End Property

        Public ReadOnly Property NumGroups() As Integer
            Get
                Return Me.m_lstGroupInputs.Count
            End Get
        End Property

        Friend ReadOnly Property EcopathFleetInputs() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstEcopathFleetInputs
            End Get
        End Property

        Public ReadOnly Property NumFleets() As Integer
            Get
                Return Me.m_lstEcopathFleetInputs.Count
            End Get
        End Property

        Public Function Output() As cMSEOutput
            Return Me.m_output
        End Function

        Public ReadOnly Property GroupOutputs() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstGroupOutputs
            End Get
        End Property


        Public ReadOnly Property GroupOutputs(ByVal iGroupIndex As Integer) As cMSEGroupOutput
            Get
                Return DirectCast(Me.m_lstGroupOutputs(iGroupIndex), cMSEGroupOutput)
            End Get
        End Property


        Public ReadOnly Property BiomassStats() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstBiomassStats
            End Get
        End Property



        Public ReadOnly Property BioEstimatesStats() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstBioEstStats
            End Get
        End Property

        Public ReadOnly Property GroupCatchStats() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstGroupCatchStats
            End Get
        End Property

        Public ReadOnly Property EffortStats() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstEffortStats
            End Get
        End Property

        Public ReadOnly Property EffortStats(ByVal iGroupIndex As Integer) As cMSEStats
            Get
                Return DirectCast(Me.m_lstEffortStats(iGroupIndex), cMSEStats)
            End Get
        End Property

        Public ReadOnly Property FCompareStats() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstFStats
            End Get
        End Property


        Public ReadOnly Property FleetStats() As cCoreInputOutputList(Of cCoreInputOutputBase)
            Get
                Return Me.m_lstFleetStats
            End Get
        End Property

        Public ReadOnly Property FleetStats(ByVal iGroupIndex As Integer) As cMSEStats
            Get
                Return DirectCast(Me.m_lstFleetStats(iGroupIndex), cMSEStats)
            End Get
        End Property

        'm_lstFleetStats

        Public ReadOnly Property GroupCatchStats(ByVal iGroupIndex As Integer) As cMSEStats
            Get
                Return DirectCast(Me.m_lstGroupCatchStats(iGroupIndex), cMSEStats)
            End Get
        End Property

        Public ReadOnly Property BiomassStats(ByVal iGroupIndex As Integer) As cMSEStats
            Get
                Return DirectCast(Me.m_lstBiomassStats(iGroupIndex), cMSEStats)
            End Get
        End Property


        Public ReadOnly Property BioEstimatesStats(ByVal iGroupIndex As Integer) As cMSEStats
            Get
                Return DirectCast(Me.m_lstBioEstStats(iGroupIndex), cMSEStats) 'Return Me.m_lstBioEstStats
            End Get
        End Property

        Public ReadOnly Property FCompare(ByVal iGroupIndex As Integer) As cMSEStats
            Get
                Return DirectCast(Me.m_lstFStats(iGroupIndex), cMSEStats) 'Return Me.m_lstBioEstStats
            End Get
        End Property


        Public ReadOnly Property ModelParameters() As cMSEParameters
            Get
                Return Me.m_parameters
            End Get
        End Property

        Public ReadOnly Property TotalFleetValueStats() As cMSEStats
            Get
                Return Me.m_TotFleetValue
            End Get
        End Property

        Public ReadOnly Property isConnected() As Boolean
            Get
                Return Me.m_bConnected
            End Get
        End Property

        Public ReadOnly Property MSEBatchManager As MSEBatchManager.cMSEBatchManager
            Get
                Return Me.m_MSE.BatchManager
            End Get
        End Property

#End Region

#Region "Construction Initialization and Running of the model"

        Public Sub New(ByVal theCore As cCore, ByVal data As cMSEDataStructures)
            Me.m_output = New cMSEOutput(theCore)
            Me.m_parameters = New cMSEParameters(theCore)
            Me.m_MSE = New cMSE(theCore)
            Me.m_MSEdata = data

            Me.m_Batch = New MSEBatchManager.cMSEBatchManager()

            'clear out the old data
            Me.m_VarToStat.Clear()
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassHistogram, eMSEStatNames.PercentageHistogram)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassMeanValues, eMSEStatNames.MeanRun)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassMin, eMSEStatNames.Min)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassMax, eMSEStatNames.Max)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassCV, eMSEStatNames.CV)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassBins, eMSEStatNames.nBins)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassBinWidths, eMSEStatNames.BinWidth)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassValues, eMSEStatNames.Values)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassAboveLimit, eMSEStatNames.AboveLimit)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBiomassBelowLimit, eMSEStatNames.BelowLimit)

            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstHistogram, eMSEStatNames.PercentageHistogram)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstMeanValues, eMSEStatNames.MeanRun)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstMin, eMSEStatNames.Min)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstMax, eMSEStatNames.Max)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstCV, eMSEStatNames.CV)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstBins, eMSEStatNames.nBins)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstBinWidths, eMSEStatNames.BinWidth)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstValues, eMSEStatNames.Values)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstAboveLimit, eMSEStatNames.AboveLimit)
            Me.m_VarToStat.Add(eVarNameFlags.MSEBioEstBelowLimit, eMSEStatNames.BelowLimit)

            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueHistogram, eMSEStatNames.PercentageHistogram)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueMeanValues, eMSEStatNames.MeanRun)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueMin, eMSEStatNames.Min)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueMax, eMSEStatNames.Max)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueCV, eMSEStatNames.CV)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueBins, eMSEStatNames.nBins)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueBinWidths, eMSEStatNames.BinWidth)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueValues, eMSEStatNames.Values)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueAboveLimit, eMSEStatNames.AboveLimit)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFleetValueBelowLimit, eMSEStatNames.BelowLimit)

            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortHistogram, eMSEStatNames.PercentageHistogram)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortMeanValues, eMSEStatNames.MeanRun)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortMin, eMSEStatNames.Min)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortMax, eMSEStatNames.Max)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortCV, eMSEStatNames.CV)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortBins, eMSEStatNames.nBins)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortBinWidths, eMSEStatNames.BinWidth)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortValues, eMSEStatNames.Values)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortAboveLimit, eMSEStatNames.AboveLimit)
            Me.m_VarToStat.Add(eVarNameFlags.MSEEffortBelowLimit, eMSEStatNames.BelowLimit)

            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatHistogram, eMSEStatNames.PercentageHistogram)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatMeanValues, eMSEStatNames.MeanRun)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatMin, eMSEStatNames.Min)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatMax, eMSEStatNames.Max)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatCV, eMSEStatNames.CV)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatBins, eMSEStatNames.nBins)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatBinWidths, eMSEStatNames.BinWidth)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatValues, eMSEStatNames.Values)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatAboveLimit, eMSEStatNames.AboveLimit)
            Me.m_VarToStat.Add(eVarNameFlags.MSEFStatBelowLimit, eMSEStatNames.BelowLimit)

            Me.m_VarToStat.Add(eVarNameFlags.MSENTrials, eMSEStatNames.nIterations)

        End Sub

        Public Function Run() As Boolean

            Try

                If Me.IsRunning Then
                    Me.m_core.Messages.SendMessage(New cMessage("A Management Strategy Evaluation is already running. Only one evaluation can be run at a time.", _
                                                                eMessageType.ErrorEncountered, eCoreComponentType.MSE, eMessageImportance.Critical, eDataTypes.MSEManager))
                    Return False
                End If

                Me.m_MSE.Connect(AddressOf Me.OnMSECallBack, AddressOf Me.OnMSYCallBack)

                Try
                    Me.m_core.PluginManager.MSERunStarted()
                Catch ex As Exception
                    System.Console.WriteLine(Me.ToString & ".ProcessCallBack() Exception thrown from PluginManager. " & ex.Message)
                End Try


                m_thrdRunMSE = New Thread(AddressOf m_MSE.Run)
                m_thrdRunMSE.Name = "MSE"

                'set the wait object to block all calling threads
                'this will set isRunning to True
                Me.SetWait()

                '  m_thrdRunMSE.Priority = ThreadPriority.Lowest
                m_thrdRunMSE.Start()

            Catch ex As Exception
                cLog.Write(ex)
                Me.ReleaseWait()
                Return False
            End Try

            Return True

        End Function

        Public Function ValidateRun() As Boolean
            Dim bOK As Boolean = True

            For iTimeSeries As Integer = 1 To m_core.nTimeSeries
                If m_core.EcosimTimeSeries(iTimeSeries).TimeSeriesType = eTimeSeriesType.DiscardProportion Or m_core.EcosimTimeSeries(iTimeSeries).TimeSeriesType = eTimeSeriesType.DiscardMortality Then
                    If m_core.EcosimTimeSeries(iTimeSeries).Enabled Then
                        Dim sTypeTimeSeries2Check As String = m_core.EcosimTimeSeries(iTimeSeries).TimeSeriesType.ToString
                        Dim fbMess As New cFeedbackMessage(String.Format(My.Resources.CoreMessages.MSE_DISCARD_TIMESERIES_WARNING, sTypeTimeSeries2Check),
                                                   eCoreComponentType.MSE, eMessageType.DataValidation,
                                                   eMessageImportance.Warning, eMessageReplyStyle.YES_NO)
                        Me.m_core.Messages.SendMessage(fbMess)
                        If fbMess.Reply = eMessageReply.NO Then
                            Return False
                        End If
                    End If
                End If
            Next

            If Me.ModelParameters.UseLPSolution Then
                'When running the LP solution
                'there is no need validate the Fleet Controls
                'All Fleets are optimized when using the LP
                Return True
            End If

            'If using Quota regulations and any type of effort
            'make sure there is at least one type of Control has been set
            If Me.ModelParameters.RegulatoryMode = eMSERegulationMode.UseRegulations Then
                'xxxxxxxxxxxxxxxxxxxxxxx
                'check the Quota type
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxx
                Dim bNoQuotaSet As Boolean = True
                For iFlt As Integer = 1 To Me.m_core.nFleets
                    If Me.EcopathFleetInputs(iFlt).QuotaType <> eQuotaTypes.NoControls Then
                        bNoQuotaSet = False
                        Exit For
                    End If
                Next

                If bNoQuotaSet Then
                    'no control type has been set for any fleet(s)
                    'ask the user what to do
                    Dim response As eMessageReply
                    Dim fbMess As New cFeedbackMessage(My.Resources.CoreMessages.MSE_VALIDATION_QUOTAS,
                                                       eCoreComponentType.MSE, eMessageType.DataValidation,
                                                       eMessageImportance.Warning, eMessageReplyStyle.YES_NO)
                    Me.m_core.Messages.SendMessage(fbMess)
                    response = fbMess.Reply

                    If response = eMessageReply.YES Then
                        Return False
                    End If
                End If

            End If

            '14-May-2010 jb no need for this either
            '
            ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ''Check for fixed escapement
            ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Dim fixedGroups As String
            'Dim fixed As Single
            'For igrp As Integer = 1 To Me.m_core.nGroups
            '    fixed = Me.m_core.MSEManager.GroupInputs(igrp).FixedEscapement + Me.m_core.MSEManager.GroupInputs(igrp).FixedF
            '    If fixed > 0 Then
            '        'Fixed escapement has been set for this group

            '        'check the Quota options for this group
            '        For iFlt As Integer = 1 To Me.m_core.nFleets
            '            If Me.m_core.EcopathFleetInputs(iFlt).Landings(igrp) > 0 Then
            '                If Me.EcopathFleetInputs(iFlt).QuotaType <> eQuotaTypes.NotUsed Then
            '                    'this group has both Fixed Escapement and Quota option set
            '                    'Only Fixed Escapement will be used

            '                    fixedGroups = fixedGroups & "'" & Me.m_core.MSEManager.GroupInputs(igrp).Name & "', "

            '                End If
            '            End If
            '        Next

            '    End If
            'Next

            'If Not String.IsNullOrEmpty(fixedGroups) Then

            '    fixedGroups = fixedGroups.Remove(fixedGroups.Length - 2)

            '    Me.m_core.Messages.AddMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MSE_VALIDATION_FIXEDESCAPEMENT, fixedGroups), _
            '            eMessageType.DataValidation, eCoreComponentType.MSE, eMessageImportance.Warning))
            'End If



            ' End If ' If Me.m_MSE.ModelParameters.EffortMode = eMSEEffortMode.PredictUseQuota Or _

            'jb 15-April-2010 Don't need to do this anymore Effort is set by MSE if EffortSource = NoCap
            ''If Ecosim Effort and Quota options are set
            ''Make sure the Effort is set high so the Quota regulation will limit effort
            ''This is not absolutely necessary
            'If Me.ModelParameters.RegulatoryMode = eMSERegulationMode.UseRegulations Then
            '    Dim fleets As String
            '    'if we are regulating catch and using the Ecosim effort
            '    'make sure the effort is high... what's high... that's a good question
            '    For iflt As Integer = 0 To Me.m_core.nFleets - 1
            '        Dim effShp As cForcingFunction
            '        effShp = Me.m_core.FishingEffortShapeManager.Item(iflt)
            '        ' JS 02Mar10: Only consider fleets with quota options set
            '        If (effShp.Mean < 10) And (Me.m_MSEdata.QuotaType(iflt) <> eQuotaTypes.NotUsed) Then
            '            fleets = fleets & "'" & effShp.Name & "', "
            '        End If
            '    Next

            '    If Not String.IsNullOrEmpty(fleets) Then

            '        'strip off the last ', '
            '        fleets = fleets.Remove(fleets.Length - 2)
            '        Me.m_core.Messages.AddMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MSE_VALIDATION_EFFORT, fleets), _
            '                                    eMessageType.DataValidation, eCoreComponentType.MSE, eMessageImportance.Warning))
            '    End If


            'End If 'If Me.m_MSE.ModelParameters.EffortMode = eMSEEffortMode.TrackUseQuota Then

            Me.m_core.Messages.sendAllMessages()

            Return bOK

        End Function

        Public Sub RunMSYSearch(ByVal byFleet As Boolean)

            Dim orgSearchMode As eSearchModes = Me.m_search.SearchMode
            Me.m_search.SearchMode = eSearchModes.NotInSearch

            If byFleet Then
                Me.m_MSE.RunMSYSearch()
                Me.m_MSE.RunBoEstimation()
            Else 'F by group
                Me.m_MSE.RunMSYSearchUsingFishingMortalityInsteadOfEffort()
            End If

            'the MSY search set BBase, Blim and Fopt these are in the Ecosim group inputs 
            'Load the core values into the interface objects
            Me.m_core.LoadEcosimGroups()
            Me.Load()
            'tell the interface that data has changed
            Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.EcoSim, eMessageImportance.Maintenance, eDataTypes.EcoSimGroupInput))
            'reference levels where set to Blim and Bbase
            Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSEGroupInput))
            Me.m_core.Messages.sendAllMessages()

            Me.m_search.SearchMode = orgSearchMode

            If Me.m_core.PluginManager IsNot Nothing Then
                Me.m_core.PluginManager.MSYRunCompleted()
            End If

        End Sub

        Friend Function Init(ByRef theCore As cCore) As Boolean Implements ISearchObjective.Init

            m_core = theCore
            m_searchObjective = m_core.SearchObjective
            m_search = theCore.m_SearchData

            Me.m_SyncOb = System.Threading.SynchronizationContext.Current
            'if there is no current context then create a new one on this thread. I'm not sure why this can happen but it was in all the samples...
            If (Me.m_SyncOb Is Nothing) Then Me.m_SyncOb = New System.Threading.SynchronizationContext()

            'cMSEDataStructures are not part of the core!!!!!
            'Only the MSEManager and model know about them 
            'this may have to change when the input/output object are created
            Me.m_MSEdata.Init(theCore)

            Me.m_MSE.Init(m_MSEdata, m_core.m_EcoSim, m_core.m_SearchData, m_core.m_EcoPathData, m_core.m_TSData, Me.m_core.PluginManager)

            'Initialize the Batch manager
            Me.m_Batch.Init(Me.m_core, Me.m_MSE)

            'set CV to the first timestep
            Me.m_MSE.setTime(1, 1)

            'connect the MSE model to the manager
            'jb move to connect at start of run 
            'to prevent loss of connection when the mse is run be the batch manager
            'Me.m_MSE.Connect(AddressOf Me.OnMSECallBack, AddressOf Me.OnMSYCallBack)

            'set the MSE model in Ecosim
            'Ecosim calls MSE.AssessFs() if the Search is turned On
            Me.m_core.m_EcoSim.InitMSE(m_MSE)

            Me.m_TotFleetValue = New cMSEStats(Me.m_core, Me.m_MSEdata.ValueFleetStats, eDataTypes.MSEValueTotalStats, Me.m_VarToStat, -9999, 1)

            'build the Input and Output objects
            Me.m_lstGroupInputs.Clear()
            For igrp As Integer = 1 To m_core.nLivingGroups
                Me.m_lstGroupInputs.Add(New cMSEGroupInput(m_core, m_core.m_EcoPathData.GroupDBID(igrp)))
            Next

            Me.m_lstEcopathFleetInputs.Clear()
            Me.m_lstFleetOutputs.Clear()
            Me.m_lstEffortStats.Clear()
            Me.m_lstFleetStats.Clear()
            For iflt As Integer = 1 To m_core.nFleets
                Me.m_lstEcopathFleetInputs.Add(New cMSEFleetInput(m_core, m_core.m_EcoPathData.FleetDBID(iflt)))
                Me.m_lstFleetOutputs.Add(New cMSEFleetOutput(Me.m_core, Me.m_MSEdata, Me.m_core.m_EcoPathData.FleetDBID(iflt), iflt))
                Me.m_lstEffortStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.EffortStats, eDataTypes.MSEEffortStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.FleetDBID(iflt), iflt))
                Me.m_lstFleetStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.CatchFleetStats, eDataTypes.MSECatchByFleetStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.FleetDBID(iflt), iflt))
            Next

            Me.m_lstGroupOutputs.Clear()
            Me.m_lstBiomassStats.Clear()
            Me.m_lstGroupCatchStats.Clear()
            Me.m_lstBioEstStats.Clear()
            Me.m_lstFStats.Clear()

            For igrp As Integer = 1 To m_core.nLivingGroups
                'BioEst
                Me.m_lstBioEstStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.BioEstStats, eDataTypes.MSEBioEstStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.GroupDBID(igrp), igrp))

                Me.m_lstGroupOutputs.Add(New cMSEGroupOutput(Me.m_core, Me.m_MSEdata, Me.m_core.m_EcoPathData.GroupDBID(igrp), igrp))
                Me.m_lstBiomassStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.BioStats, eDataTypes.MSEBiomassStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.GroupDBID(igrp), igrp))
                Me.m_lstGroupCatchStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.CatchGroupStats, eDataTypes.MSECatchByGroupStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.GroupDBID(igrp), igrp))

                Me.m_lstFStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.FLPDualValue, eDataTypes.MSEFStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.GroupDBID(igrp), igrp))
                ' Me.m_lstBiomassStats.Add(New cMSEStats(Me.m_core, Me.m_MSEdata.FActualStats, eDataTypes.MSEBiomassStats, Me.m_VarToStat, Me.m_core.m_EcoPathData.GroupDBID(igrp), igrp))
            Next

            If Me.m_core.PluginManager IsNot Nothing Then
                Me.m_core.PluginManager.MSEInitialized(Me.m_MSE, Me.m_MSEdata, Me.m_core.m_EcoSimData)
                Me.m_core.PluginManager.MSYInitialized(Me.m_MSEdata, Me.m_core.m_EcoSimData)
            End If

        End Function

        Friend Function Load() As Boolean Implements ISearchObjective.Load

            Try

                'Load the Input Output objects
                Me.loadInputs()

                'Load the MSE Batch Manager
                Me.loadBatch()

            Catch ex As Exception

            End Try

        End Function

        Private Function loadInputs() As Boolean

            Dim coreData As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim iGroup As Integer, iFleet As Integer

            Try

                'Group inputs
                For Each mseGrp As cMSEGroupInput In Me.m_lstGroupInputs
                    mseGrp.AllowValidation = False

                    mseGrp.Resize()
                    'convert the Database ID into a group index
                    iGroup = Array.IndexOf(coreData.GroupDBID, mseGrp.DBID)

                    mseGrp.Index = iGroup
                    mseGrp.Name = Me.m_core.m_EcoPathData.GroupName(iGroup)

                    mseGrp.UpperRisk = Me.m_MSEdata.BioRiskValue(iGroup, 1)
                    mseGrp.LowerRisk = Me.m_MSEdata.BioRiskValue(iGroup, 0)
                    mseGrp.FixedEscapement = Me.m_MSEdata.FixedEscapement(iGroup)

                    mseGrp.BiomassRefLower = Me.m_MSEdata.BioBounds(iGroup).Lower
                    mseGrp.BiomassRefUpper = Me.m_MSEdata.BioBounds(iGroup).Upper

                    mseGrp.BiomassEstRefLower = Me.m_MSEdata.BioEstBounds(iGroup).Lower
                    mseGrp.BiomassEstRefUpper = Me.m_MSEdata.BioEstBounds(iGroup).Upper

                    mseGrp.CatchRefLower = Me.m_MSEdata.CatchGroupBounds(iGroup).Lower
                    mseGrp.CatchRefUpper = Me.m_MSEdata.CatchGroupBounds(iGroup).Upper

                    mseGrp.RHalfB0Ratio = Me.m_MSEdata.RHalfB0Ratio(iGroup)
                    mseGrp.ForcastGain = Me.m_MSEdata.RstockRatio(iGroup)

                    mseGrp.FixedF = Me.m_MSEdata.FixedF(iGroup)
                    mseGrp.TAC = Me.m_MSEdata.TAC(iGroup)
                    mseGrp.RecruitmentCV = Me.m_MSEdata.cvRec(iGroup)

                    mseGrp.BLim = Me.m_MSEdata.Blim(iGroup)
                    mseGrp.BBase = Me.m_MSEdata.Bbase(iGroup)
                    mseGrp.FOpt = Me.m_MSEdata.Fopt(iGroup)
                    mseGrp.Fmin = Me.m_MSEdata.Fmin(iGroup)

                    For it As Integer = 1 To Me.m_MSEdata.nYears
                        mseGrp.BiomassCV(it) = Me.m_MSEdata.CVBiomT(iGroup, it)
                    Next

                    mseGrp.ResetStatusFlags()
                    mseGrp.AllowValidation = True
                Next

                'Group outputs just the index outputs will be populated in LoadOutputs() at each iteration
                For Each mseOutput As cMSEGroupOutput In Me.m_lstGroupOutputs
                    mseOutput.AllowValidation = False 'no validation of outputs
                    mseOutput.Resize()
                    mseOutput.Index = Array.IndexOf(coreData.GroupDBID, mseOutput.DBID)
                    mseOutput.Name = Me.m_core.m_EcoPathData.GroupName(mseOutput.Index)
                Next

                'Stat objects 
                For Each stat As cMSEStats In Me.m_lstBiomassStats
                    stat.AllowValidation = False 'no validation of outputs
                    stat.Index = Array.IndexOf(coreData.GroupDBID, stat.DBID)
                    stat.Name = Me.m_core.m_EcoPathData.GroupName(stat.Index)
                Next

                For Each stat As cMSEStats In Me.m_lstBioEstStats
                    stat.AllowValidation = False 'no validation of outputs
                    stat.Index = Array.IndexOf(coreData.GroupDBID, stat.DBID)
                    stat.Name = Me.m_core.m_EcoPathData.GroupName(stat.Index)
                Next

                For Each stat As cMSEStats In Me.m_lstGroupCatchStats
                    stat.AllowValidation = False 'no validation of outputs
                    stat.Index = Array.IndexOf(coreData.GroupDBID, stat.DBID)
                    stat.Name = Me.m_core.m_EcoPathData.GroupName(stat.Index)
                Next

                For Each stat As cMSEStats In Me.m_lstFleetStats
                    stat.AllowValidation = False 'no validation of outputs
                    stat.Index = Array.IndexOf(coreData.FleetDBID, stat.DBID)
                    stat.Name = Me.m_core.m_EcoPathData.FleetName(stat.Index)
                Next

                For Each stat As cMSEStats In Me.m_lstEffortStats
                    stat.AllowValidation = False 'no validation of outputs
                    stat.Index = Array.IndexOf(coreData.FleetDBID, stat.DBID)
                    stat.Name = Me.m_core.m_EcoPathData.FleetName(stat.Index)
                Next

                For Each stat As cMSEStats In Me.m_lstFStats
                    stat.AllowValidation = False 'no validation of outputs
                    stat.Index = Array.IndexOf(coreData.GroupDBID, stat.DBID)
                    stat.Name = Me.m_core.m_EcoPathData.GroupName(stat.Index)
                Next


                'fleets
                For Each mseFlt As cMSEFleetInput In Me.m_lstEcopathFleetInputs
                    mseFlt.AllowValidation = False
                    mseFlt.Resize()
                    'convert the Database ID into a fleet index
                    iFleet = Array.IndexOf(coreData.FleetDBID, mseFlt.DBID)

                    mseFlt.Index = iFleet
                    mseFlt.Name = Me.m_core.m_EcoPathData.FleetName(iFleet)
                    mseFlt.QIncrease = Me.m_MSEdata.Qgrow(iFleet)
                    mseFlt.MSYEvaluateFleet = Me.m_MSEdata.MSYEvaluateFleet(iFleet)

                    mseFlt.CatchRefLower = Me.m_MSEdata.CatchFleetBounds(iFleet).Lower
                    mseFlt.CatchRefUpper = Me.m_MSEdata.CatchFleetBounds(iFleet).Upper

                    mseFlt.EffortRefLower = Me.m_MSEdata.EffortFleetBounds(iFleet).Lower
                    mseFlt.EffortRefUpper = Me.m_MSEdata.EffortFleetBounds(iFleet).Upper

                    For igrp As Integer = 1 To m_core.nLivingGroups
                        mseFlt.FleetWeight(igrp) = Me.m_MSEdata.Fweight(iFleet, igrp)
                    Next

                    For it As Integer = 1 To Me.m_MSEdata.nYears
                        mseFlt.FleetCV(it) = Me.m_MSEdata.CVFT(iFleet, it)
                    Next

                    mseFlt.MaxEffort = Me.m_MSEdata.MaxEffort(iFleet)
                    mseFlt.QuotaType = Me.m_MSEdata.QuotaType(iFleet)

                    mseFlt.LowerLPEffortBound = Me.m_MSEdata.LowLPEffort(iFleet)
                    mseFlt.UpperLPEffortBound = Me.m_MSEdata.UpperLPEffort(iFleet)

                    For iGroup = 1 To m_core.nGroups
                        mseFlt.QuotaShare(iGroup) = m_MSEdata.Quotashare(iFleet, iGroup)
                    Next

                    mseFlt.ResetStatusFlags()
                    mseFlt.AllowValidation = True
                Next

                m_parameters.AllowValidation = False
                m_parameters.AssessmentMethod = Me.m_MSEdata.AssessMethod
                m_parameters.AssessPower = Me.m_MSEdata.AssessPower

                'Use the first array element as the interface value
                'Copied from EwE5
                Try
                    'Ok both ForcastGain and KalmanGain are arrays in the code
                    'but both set as a single value from the interface
                    ' m_parameters.ForcastGain = Me.m_MSEdata.GstockPred(1)
                    '  m_parameters.KalmanGain = Me.m_MSEdata.KalmanGain(1)
                Catch ex As Exception

                End Try

                m_parameters.UseEconomicPlugin = Me.m_search.MSEUseEconomicPlugin
                m_parameters.NTrials = Me.m_MSEdata.NTrials
                m_parameters.RegulatoryMode = Me.m_MSEdata.RegulationMode
                m_parameters.EffortSource = Me.m_MSEdata.EffortSource

                m_parameters.MSYStartTimeIndex = Me.m_MSEdata.MSYStartTimeIndex
                m_parameters.MSYRunSilent = Me.m_MSEdata.MSYRunSilent
                m_parameters.MSYEvaluateValue = Me.m_MSEdata.MSYEvaluateValue
                m_parameters.MSEStartYear = Me.m_MSEdata.StartYear

                m_parameters.MSEResultsEndYear = Me.m_MSEdata.ResultsEndYear

                m_parameters.MaxEffort = Me.m_MSEdata.MSEMaxEffort
                m_parameters.UseLPSolution = Me.m_MSEdata.UseLPSolution

                m_parameters.ResetStatusFlags()

                m_parameters.AllowValidation = True

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".Load() Error: " & ex.Message, ex)
            End Try

            Return True

        End Function


        Private Function loadBatch() As Boolean
            Dim breturn As Boolean
            Try
                Me.m_Batch.LoadScenario()

                'WARNING: this will overwrite any data loaded by the database
                'Right now it's used for debugging to load data into the objects
                'Me.m_Batch.setDefaults()

                breturn = True
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & " Exception: " & ex.Message)
            End Try

            Return breturn
        End Function

        Friend Sub Clear() Implements ISearchObjective.Clear
            Try

                Me.m_lstBioEstStats.Clear()
                Me.m_lstBiomassStats.Clear()
                Me.m_lstEffortStats.Clear()
                Me.m_lstEcopathFleetInputs.Clear()
                Me.m_lstFleetOutputs.Clear()
                Me.m_lstFleetStats.Clear()
                Me.m_lstGroupCatchStats.Clear()
                Me.m_lstGroupInputs.Clear()
                Me.m_lstGroupOutputs.Clear()

                Me.m_MSEdata.Clear()
                Me.m_MSE.Disconnect(AddressOf Me.OnMSECallBack, AddressOf Me.OnMSYCallBack)
                Me.m_MSE.Clear()

                Me.m_Batch.Clear()

                Me.m_output.Clear()
                Me.m_parameters.Clear()

                If Me.m_TotFleetValue IsNot Nothing Then
                    'm_TotFleetValue gets created during Init() which gets called when an Ecosim scenario is loaded
                    'Clear can be called by the Core BEFORE Init().
                    'When the core is initialized before an Ecosim scenario is loaded
                    Me.m_TotFleetValue.Clear()
                End If

                ' JS: 03Jan01: do not destroy objects only created in the constructor. Clear() is not a Destructor!
                'Me.m_output = Nothing
                'Me.m_parameters = Nothing
                'Me.m_MSE = Nothing
                'Me.m_Batch = Nothing


            Catch ex As Exception
                cLog.Write(ex)
            End Try
        End Sub

        ''' <summary>
        ''' Update the underlying core data with edits from the interface
        ''' </summary>
        ''' <remarks>This is called by the core when a variable passes validation via cCore.OnValidated()</remarks>
        Public Function Update(ByVal DataType As eDataTypes) As Boolean Implements ISearchObjective.Update

            Try
                Select Case DataType

                    Case eDataTypes.MSEGroupInput

                        For Each mseGrp As cMSEGroupInput In Me.m_lstGroupInputs

                            Dim iGroup As Integer = mseGrp.Index

                            ' Me.m_MSEdata.CVbiomEst(iGroup) = mseGrp.BiomassCV
                            Me.m_MSEdata.FixedEscapement(iGroup) = mseGrp.FixedEscapement

                            Me.m_MSEdata.BioRiskValue(iGroup, 1) = mseGrp.UpperRisk
                            Me.m_MSEdata.BioRiskValue(iGroup, 0) = mseGrp.LowerRisk

                            Me.m_MSEdata.BioBounds(iGroup).Lower = mseGrp.BiomassRefLower
                            Me.m_MSEdata.BioBounds(iGroup).Upper = mseGrp.BiomassRefUpper

                            Me.m_MSEdata.CatchGroupBounds(iGroup).Lower = mseGrp.CatchRefLower
                            Me.m_MSEdata.CatchGroupBounds(iGroup).Upper = mseGrp.CatchRefUpper

                            Me.m_MSEdata.RHalfB0Ratio(iGroup) = mseGrp.RHalfB0Ratio
                            Me.m_MSEdata.RstockRatio(iGroup) = mseGrp.ForcastGain
                            Me.m_MSEdata.cvRec(iGroup) = mseGrp.RecruitmentCV
                            Me.m_MSEdata.FixedF(iGroup) = mseGrp.FixedF
                            Me.m_MSEdata.TAC(iGroup) = mseGrp.TAC

                            Me.m_MSEdata.Blim(iGroup) = mseGrp.BLim
                            Me.m_MSEdata.Bbase(iGroup) = mseGrp.BBase
                            Me.m_MSEdata.Fopt(iGroup) = mseGrp.FOpt
                            Me.m_MSEdata.Fmin(iGroup) = mseGrp.Fmin
                            Me.m_MSEdata.UseLPSolution = m_parameters.UseLPSolution

                            For it As Integer = 1 To Me.m_MSEdata.nYears
                                Me.m_MSEdata.CVBiomT(iGroup, it) = mseGrp.BiomassCV(it)
                            Next

                        Next

                    Case eDataTypes.MSEFleetInput

                        For Each mseFlt As cMSEFleetInput In Me.m_lstEcopathFleetInputs

                            Dim iFleet As Integer = mseFlt.Index

                            Me.m_MSEdata.Qgrow(iFleet) = mseFlt.QIncrease

                            Me.m_MSEdata.CatchFleetBounds(iFleet).Lower = mseFlt.CatchRefLower
                            Me.m_MSEdata.CatchFleetBounds(iFleet).Upper = mseFlt.CatchRefUpper

                            Me.m_MSEdata.EffortFleetBounds(iFleet).Lower = mseFlt.EffortRefLower
                            Me.m_MSEdata.EffortFleetBounds(iFleet).Upper = mseFlt.EffortRefUpper
                            Me.m_MSEdata.MSYEvaluateFleet(iFleet) = mseFlt.MSYEvaluateFleet

                            For igrp As Integer = 1 To m_core.nLivingGroups
                                Me.m_MSEdata.Fweight(iFleet, igrp) = mseFlt.FleetWeight(igrp)
                            Next igrp

                            For it As Integer = 1 To Me.m_MSEdata.nYears
                                Me.m_MSEdata.CVFT(iFleet, it) = mseFlt.FleetCV(it)
                            Next

                            Me.m_MSEdata.MaxEffort(iFleet) = mseFlt.MaxEffort
                            Me.m_MSEdata.QuotaType(iFleet) = mseFlt.QuotaType

                            Me.m_MSEdata.LowLPEffort(iFleet) = mseFlt.LowerLPEffortBound
                            Me.m_MSEdata.UpperLPEffort(iFleet) = mseFlt.UpperLPEffortBound

                            For iGroup As Integer = 1 To m_core.nGroups
                                m_MSEdata.Quotashare(iFleet, iGroup) = mseFlt.QuotaShare(iGroup)
                            Next

                        Next mseFlt

                    Case eDataTypes.MSEParameters

                        'For igrp As Integer = 1 To m_core.nLivingGroups
                        '    'KalmanGain and ForcastGain are set as a single value in the interface
                        '    'this is applied to all the groups for the code
                        '    Me.m_MSEdata.KalmanGain(igrp) = Me.m_parameters.KalmanGain
                        '    'Me.m_MSEdata.GstockPred(igrp) = Me.m_parameters.ForcastGain
                        '    'Me.m_MSEdata.RstockRatio(igrp) = (1 - Me.m_MSEdata.GstockPred(igrp)) * m_core.StartBiomass(igrp)
                        'Next igrp

                        Me.m_MSEdata.AssessMethod = Me.m_parameters.AssessmentMethod()
                        Me.m_MSEdata.AssessPower = Me.m_parameters.AssessPower()
                        Me.m_MSEdata.NTrials = Me.m_parameters.NTrials()
                        Me.m_MSEdata.RegulationMode = Me.m_parameters.RegulatoryMode
                        Me.m_MSEdata.EffortSource = Me.m_parameters.EffortSource
                        Me.m_MSEdata.StartYear = Me.m_parameters.MSEStartYear
                        Me.m_MSEdata.MSYStartTimeIndex = Me.m_parameters.MSYStartTimeIndex
                        Me.m_MSEdata.ResultsStartYear = Me.m_parameters.MSEResultsStartYear
                        Me.m_MSEdata.MSYEvaluateValue = Me.m_parameters.MSYEvaluateValue
                        Me.m_MSEdata.MSYRunSilent = Me.m_parameters.MSYRunSilent

                        Me.m_MSEdata.MSEMaxEffort = Me.m_parameters.MaxEffort

                        Me.m_search.MSEUseEconomicPlugin = Me.m_parameters.UseEconomicPlugin

                        Me.m_MSEdata.UseLPSolution = Me.m_parameters.UseLPSolution

                End Select

                System.Console.WriteLine(Me.ToString & ".Update(" & DataType.ToString & ")")

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Function

        Public Sub FleetTradeoffs()

            Me.m_MSE.RunFleetTradeoffs()

        End Sub

        ''' <summary>
        ''' Sum quota shares to one.
        ''' </summary>
        Public Sub SumQuotaShareToOne()

            Dim QuotaShareTot As Single
            Dim igrp As Integer
            Dim iflt As Integer

            For igrp = 1 To Me.m_core.nGroups
                QuotaShareTot = 0
                For iflt = 1 To Me.m_core.nFleets
                    QuotaShareTot += Me.m_MSEdata.Quotashare(iflt, igrp)
                Next

                If (QuotaShareTot > 0) And (QuotaShareTot <> 1.0!) Then
                    For iflt = 1 To Me.m_core.nFleets
                        Me.m_MSEdata.Quotashare(iflt, igrp) /= QuotaShareTot
                    Next
                End If
            Next igrp
            Me.Load()

        End Sub

        ''' <summary>
        ''' Reset quota shares to defaults.
        ''' </summary>
        Public Sub SetDefaultQuotaShare()

            Me.m_MSEdata.setDefaultQuotaShare()
            Me.Load()

        End Sub


        ''' <summary>
        ''' Reset quota shares to defaults.
        ''' </summary>
        Public Sub SetDefaultTFM()

            Me.m_MSEdata.setDefaultTFM()
            Me.Load()

        End Sub

        Public Sub SetDefaultRecruitment()

            Me.m_MSEdata.setDefaultRecruitment()
            Me.Load()

        End Sub

        ''' <summary>
        ''' Reset group references levels to default fishing mortalities.
        ''' </summary>
        Public Sub SetDefaultGroupRefLevels()
            Dim refLevelPercent As Single = 0.5
            For iGroup As Integer = 1 To Me.m_MSEdata.NGroups
                'Me.m_MSEdata.BioBounds(iGroup).Lower = Me.m_MSEdata.Blim(iGroup)
                'Me.m_MSEdata.BioBounds(iGroup).Upper = Me.m_MSEdata.Bbase(iGroup)
                'Set default to percentage of Ecopath base
                Dim b As Single = Me.m_core.m_EcoPathData.B(iGroup)
                Me.m_MSEdata.BioBounds(iGroup).Lower = b * refLevelPercent
                Me.m_MSEdata.BioBounds(iGroup).Upper = b / refLevelPercent
            Next iGroup
            Me.Load()

        End Sub

#End Region

#Region "Model communication callback delegates for model and interface"


        ''' <summary>
        ''' Callback handler called by the MSE model
        ''' </summary>
        ''' <param name="CallBackType"></param>
        ''' <remarks></remarks>
        Private Sub OnMSECallBack(ByVal CallBackType As eMSERunStates)
            'this is called on the MSE worker thread
            'so even if the main thread has called Wait() and is blocking this code will be processed and the MSE can continue 

            Try

                'do any processing based on the type of callback even if there is no interface connected
                Me.ProcessCallBack(CallBackType)

                'At this time it is possible to run the manager without it being connected to an interface
                'This is so it can be run as a TOOL or as part of a Plugin process without calling an interface
                If m_bConnected Then

                    'make sure something didn't screwup
                    Debug.Assert(m_SyncOb IsNot Nothing And m_MSECallback IsNot Nothing, Me.ToString & ".OnMSECallBack() not connected properly.")

                    'Connected so call the interface
                    'm_SyncOb.Post(New System.Threading.SendOrPostCallback(AddressOf fireCallBack), CallBackType)
                    m_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf fireCallBack), CallBackType)

                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub


        Private Sub fireCallBack(ByVal obj As Object)
            Try
                'Debug.Assert(m_SyncOb IsNot Nothing And m_MSECallback IsNot Nothing, Me.ToString & ".OnMSECallBack() not connected properly.")
                If m_MSECallback IsNot Nothing Then
                    Dim cbType As eMSERunStates = DirectCast(obj, eMSERunStates)
                    m_MSECallback.Invoke(cbType)
                End If
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & " Error sending message to interface.")
            End Try
        End Sub


        Private Sub ProcessCallBack(ByVal CallBackType As eMSERunStates)

            ' System.Console.WriteLine(Me.ToString & " Callback type = " & CallBackType.ToString)

            Select Case CallBackType

                Case eMSERunStates.IterationCompleted

                    'populate output objects for this iteration
                    Me.LoadOutputs()

                Case eMSERunStates.IterationStarted

                Case eMSERunStates.Started

                Case eMSERunStates.RunCompleted

                    Me.LoadOutputs()
                    'the thread has completed its task
                    'clear the signal state of the thread this will release any threads that called Wait()
                    Me.ReleaseWait()

                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.MSERunCompleted, eCoreComponentType.MSE, eMessageImportance.Maintenance))

                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSEOutput))
                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSEGroupOutputs))

                    'stat objects
                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSEBiomassStats))
                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSECatchByFleetStats))
                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSECatchByGroupStats))
                    Me.m_core.Messages.AddMessage(New cMessage("", eMessageType.DataModified, eCoreComponentType.MSE, eMessageImportance.Maintenance, eDataTypes.MSEEffortStats))

                    Try
                        Me.m_core.PluginManager.MSERunCompleted()
                    Catch ex As Exception
                        System.Console.WriteLine(Me.ToString & ".ProcessCallBack() Exception thrown from PluginManager. " & ex.Message)
                    End Try

                    If Me.m_MSEdata.lstNonOptSolutions.Count > 0 Then
                        'LP Solver failed to find optimal solution for 1 or more timesteps
                        Dim msg As String = cStringUtils.Localize(My.Resources.CoreMessages.MSE_LPSOLVER_NONOPTIMAL, Me.m_MSEdata.lstNonOptSolutions.Count)
                        Me.m_core.Messages.AddMessage(New cMessage(msg, eMessageType.ErrorEncountered, eCoreComponentType.MSE, eMessageImportance.Warning))
                    End If

                    Me.m_core.Messages.sendAllMessages()

                    Me.m_MSE.Disconnect(AddressOf Me.OnMSECallBack, AddressOf Me.OnMSYCallBack)

            End Select

            Try
                'jb 20-Oct-09 the core message publisher now marshalls all messages to the handlers thread
                'no need to do it here
                'Me.m_core.Messages.sendAllMessages()

                ''make sure something didn't screwup
                'Debug.Assert(m_SyncOb IsNot Nothing, Me.ToString & ".OnMSECallBack() not connected properly.")
                ''Marshall the call the cCore.Messages.sendAllMessages() onto the cores thread
                'm_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireSendMessages), Nothing)
            Catch ex As Exception

            End Try

        End Sub


        ''' <summary>
        ''' Callback handler called by the MSY search 
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub OnMSYCallBack(ByVal MYSProgress As cMSYProgressArgs)
            'this is called on the MSE worker thread
            'so even if the main thread has called Wait() and is blocking this code will be processed and the MSE can continue 

            Try

                'At this time it is possible to run the manager without it being connected to an interface
                'This is so it can be run as a TOOL or as part of a Plugin process without calling an interface
                If m_bConnected Then

                    'make sure something didn't screwup
                    Debug.Assert(m_SyncOb IsNot Nothing And Me.m_MSYCallback IsNot Nothing, Me.ToString & ".OnMSYCallBack() not connected properly.")
                    m_SyncOb.Post(New System.Threading.SendOrPostCallback(AddressOf fireMSYCallBack), MYSProgress)

                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub


        Private Sub fireMSYCallBack(ByVal obj As Object)
            Try
                Debug.Assert(m_SyncOb IsNot Nothing And m_MSYCallback IsNot Nothing, Me.ToString & ".OnMSECallBack() not connected properly.")
                m_MSYCallback(DirectCast(obj, cMSYProgressArgs))
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & " Error sending message to interface.")
            End Try
        End Sub

        Private Sub dumpOutput()
            Console.WriteLine("----------MSE output-------------")

            Console.WriteLine("Mean Economic = " & Me.m_output.MeanEconomicValue.ToString)
            Console.WriteLine("Mean Ecologial = " & Me.m_output.MeanEcologicalValue.ToString)
            Console.WriteLine("Mean Employ = " & Me.m_output.MeanEmployValue.ToString)
            Console.WriteLine("Biomass risk")

            For Each grp As cMSEGroupOutput In Me.GroupOutputs
                Dim igrp As Integer = grp.Index
                Console.WriteLine("grp = " & igrp.ToString & ", lower = " & grp.LowerRiskPercent.ToString & ", upper = " & grp.UpperRiskPercent.ToString & ", ")
            Next
            Console.WriteLine("---------Done-------------")

        End Sub

        ''' <summary>
        ''' Load results of trial(s) into output object
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub LoadOutputs()

            'Results of the trials are outputs only and are not validated AllowValidation = False
            'the Status is hardwired in .ResetStatusFlags()
            'If a Validation object is used it must be made thread safe as this is run on the MSE worker thread

            Me.m_output.TrialNumber = Me.m_MSEdata.CurrentIteration

            Me.m_output.WeightedMeanTotalValue = Me.m_MSEdata.sumWeightedValues / Me.m_MSEdata.CurrentIteration
            Me.m_output.BestTotalValue = Me.m_MSEdata.BestTotalValue

            Me.m_output.MeanEconomicValue = Me.m_MSEdata.SumTotVal / Me.m_MSEdata.CurrentIteration
            Me.m_output.MeanEcologicalValue = Me.m_MSEdata.sumEcoVal / Me.m_MSEdata.CurrentIteration
            Me.m_output.MeanEmployValue = Me.m_MSEdata.sumEmployVal / Me.m_MSEdata.CurrentIteration
            Me.m_output.MeanMandatedValue = Me.m_MSEdata.sumManVal / Me.m_MSEdata.CurrentIteration

            Me.m_output.EconomicValue = Me.m_MSEdata.BaseTotalVal
            Me.m_output.EcologicalValue = Me.m_MSEdata.BaseEcoVal
            Me.m_output.EmployValue = Me.m_MSEdata.BaseEmployVal
            Me.m_output.MandatedValue = Me.m_MSEdata.BaseManValue

            Dim nt As Integer = m_core.GetCoreCounter(eCoreCounterTypes.nEcosimTimeSteps)
            For Each grp As cMSEGroupOutput In Me.m_lstGroupOutputs
                Dim igrp As Integer = grp.Index
                grp.Init()
                ' grp.LowerRiskPercent = Me.m_MSEdata.BioSum.PercentageBelow(igrp, Me.m_MSEdata.BioBounds(igrp).Lower)
                ' grp.UpperRiskPercent = Me.m_MSEdata.BioSum.PrecentageAbove(igrp, Me.m_MSEdata.BioBounds(igrp).Upper)
            Next grp

            For Each flt As cMSEFleetOutput In Me.m_lstFleetOutputs
                flt.Init()
            Next

        End Sub

#End Region

#Region "ICoreInterface"

        Public ReadOnly Property DataType() As eDataTypes Implements ICoreInterface.DataType
            Get
                Return eDataTypes.MSEManager
            End Get
        End Property

        Public ReadOnly Property CoreComponent() As eCoreComponentType Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.EcoSpace
            End Get
        End Property

        Public Property DBID() As Integer Implements ICoreInterface.DBID
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(ByVal value As Integer)

            End Set
        End Property

        Public Function GetID() As String Implements ICoreInterface.GetID
            Return cCore.NULL_VALUE.ToString
        End Function

        Public Property Index() As Integer Implements ICoreInterface.Index
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(ByVal value As Integer)

            End Set
        End Property

        Public Property Name() As String Implements ICoreInterface.Name
            Get
                Return Me.ToString
            End Get
            Set(ByVal value As String)

            End Set
        End Property

#End Region

#Region "ISearchObjective"

        Public ReadOnly Property FleetObjectives(ByVal iFleet As Integer) As cSearchObjectiveFleetInput Implements ISearchObjective.FleetObjectives
            Get
                Return Me.m_searchObjective.FleetObjectives(iFleet)
            End Get
        End Property

        Public ReadOnly Property GroupObjectives(ByVal iGroup As Integer) As cSearchObjectiveGroupInput Implements ISearchObjective.GroupObjectives
            Get
                Return Me.m_searchObjective.GroupObjectives(iGroup)
            End Get
        End Property

        Public ReadOnly Property ValueWeights() As cSearchObjectiveWeights Implements ISearchObjective.ValueWeights
            Get
                Return Me.m_searchObjective.ValueWeights
            End Get
        End Property

        Public ReadOnly Property ObjectiveParameters() As SearchObjectives.cSearchObjectiveParameters Implements SearchObjectives.ISearchObjective.ObjectiveParameters
            Get
                Return Me.m_searchObjective.ObjectiveParameters
            End Get
        End Property

#End Region

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub

        Public Overrides Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean ' Implements SearchObjectives.ISearchObjective.StopRun
            Dim result As Boolean = True

            If (Me.m_core Is Nothing) Then Return True

            Try

                If WaitTimeInMillSec <> 0 Then
                    Me.m_core.StopEcoSim()
                End If

                Me.m_MSEdata.StopRun = True
                result = Me.Wait(WaitTimeInMillSec)
            Catch ex As Exception
                result = False
            End Try
            Return result
        End Function
    End Class

End Namespace





