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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Option Explicit On
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports LumenWorks.Framework.IO.Csv
Imports ScientificInterfaceShared.Controls
Imports Troschuetz.Random
Imports EwEMSEPlugin.HCR_GroupNS

#End Region ' Imports

Public Class cMSE

#Region "Internal definitions"

    Private Enum eCatchTypes
        Landings
        DiscardSurvivals
        DiscardMortalities
    End Enum
#End Region

#Region " Internal vars "

    Private m_core As cCore = Nothing
    Private m_strategies As Strategies = Nothing
    Private m_survivability As cSurvivability = Nothing
    Private m_regulations As cRegulations = Nothing
    Private m_monitor As cMSEStateMonitor = Nothing
    Private m_effortlimits As cEffortLimits = Nothing
    Private m_diets As cDiets = Nothing
    Private m_implementation_error As Single
    Private m_currentModelID As Integer
    Public m_currentTimeStep As Integer
    Private m_FleetImpError As Single()

    'Private m_currentStrategy As Strategy = Nothing

    'Zero based index of the current strategy
    Private m_iCurStategy As Integer = cCore.NULL_VALUE

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'For the Stock Assessment Model
    Private m_StockAssessment As cStockAssessmentModel

    'For now use the MSE data from the Core get CV's for distributions
    'Once we have an interface we can replace this
    Private m_CoreMSEData As MSE.cMSEDataStructures
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    Private m_ecosim As EwECore.Ecosim.cEcoSimModel
    Private _simdata As cEcosimDatastructures
    Private _pathdata As cEcopathDataStructures
    Private m_ecopath As Ecopath.cEcoPathModel
    Private _EcosimTimeStepDelegate As EwECore.Ecosim.EcoSimTimeStepDelegate
    Private StrategyIndex As Integer
    Public OriginalNTimesteps As Integer
    Private MaxEffortThisYear() As Single
    Private m_fleet_max_effort As List(Of cFleetMaxEffort)

    Private TargConsQuota(,) As Single 'Stores the target and conservation quota's for each species
    Private nSuccessfullyProjectedModels As Integer

    Private TechnologyCreep() As Single 'an array where each element represents the percentage with which each fleet increases its catching efficiency each year
    Private m_plugin As cMSEPluginPoint = Nothing

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'All basic Ecopath and Ecosim parameters X(a,b) a = iteration b = the functional group
    Private B(,) As Double
    Private PB(,) As Double
    Private QB(,) As Double
    Private EE(,) As Double
    Private BA(,) As Double
    Private DenDepCatchability(,) As Double
    Private FeedingTimeAdjustRate(,) As Double
    Private MaxRelFeedingTime(,) As Double
    Private OtherMortFeedingTime(,) As Double
    Private PredEffectFeedingTime(,) As Double
    Private QBMaxxQBio(,) As Double
    Private SwitchingPower(,) As Double
    Private Vulnerabilities(,,) As Double
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    Private FleetsThatFishHCRGrp As List(Of Integer) = New List(Of Integer)
    Public Property m_quotashares As cQuotaShares
    Public Property m_biomasslimits As cBiomassLimits
    'Private m_Survivability As cSurvivability

    Private BTemp() As Double
    Private PBTemp() As Double
    Private QBTemp() As Double
    Private EETemp() As Double
    Private BATemp() As Double
    Private DenDepCatchabilityTemp() As Double
    Private FeedingTimeAdjustRateTemp() As Double
    Private MaxRelFeedingTimeTemp() As Double
    Private OtherMortFeedingTimeTemp() As Double
    Private PredEffectFeedingTimeTemp() As Double
    Private QBMaxxQBioTemp() As Double
    Private SwitchingPowerTemp() As Double
    Private VulnerabilitiesTemp(,) As Double
    Private DietMatrixTemp(,) As Double
    Private DietImpTemp() As Double
    Private ChangeEffortFlag As Boolean = False
    Private m_rand As New Random()

    Private m_StopRun As Boolean
    Private m_runstate As eRunStates = eRunStates.Idle

    Private HCR_F_Table As DataTable
    Private HCR_Quota_Table As DataTable
    Private TargetFs(,) As Single
    Private ConservationFs(,) As Single
    Private TargetQuotas(,,) As Single
    Private ConservationQuotas(,,) As Single

    'Private Consumption(,,) As Single

    Private Realised_F_Table As DataTable
    Private m_RealisedFs(,) As Single
    Private Realised_Landed_F_Table As DataTable
    Private m_RealisedLandedFs(,) As Single
    Private Realised_Discard_F_Table As DataTable
    Private m_RealisedDiscardFs(,) As Single

    Private HighestValueSpeciesTable As DataTable
    Private m_HighestValueSpecies(,) As String

    Private ChokeGroupTable As DataTable
    Private m_ChokeGroup(,) As String

    Private CatchTrajTable As DataTable

    Const MIN_DIET_PROP As Single = 0.000001

    ''' <summary>
    ''' Landings and discards by Fleet, Group and eCatchTypes
    ''' </summary>
    Private LandingsDiscards(,,) As Single
    Private m_LandingsDiscardsThroughoutProjection(,,,) As Single

    Private m_ConsumptionThroughoutProjection(,,) As Single

    ' Private m_PredationThroughoutProjection(,,,) As Single

    ' ''' <summary>
    ' ''' Total base catch rates, including discards that survived.
    ' ''' </summary>
    ' ''' <remarks>This is not just mortality it includes catch that survived. </remarks>
    'Private BaseCatchRate(,) As Single

    Private m_relQModifier(,) As Single
    Private m_bQSet As Boolean

    Private m_PassedChangeInFAtBeginProjTest As Boolean

#Region "Threading variables"
    'Private m_WaitObject As System.Threading.ManualResetEvent

    Private m_SyncObj As System.Threading.SynchronizationContext

#End Region

    Public Enum DistributionType As Integer
        NotSet = 0
        Uniform
        Triangular
    End Enum

    Public Enum RegulateFleet As Integer
        Discards = 0
        NoDiscards_NonSelective
        NoDiscards_Selective
    End Enum

    Private Enum HCRType As Integer
        Target = 0
        Conservation = 1
    End Enum

    Private m_mhSettings As cMessageHandler = Nothing
    Private m_mhEcosim As cMessageHandler = Nothing
    Private m_iNumModelsAvailable As Integer = cCore.NULL_VALUE
    Private m_iNumStrategiesAvailable As Integer = cCore.NULL_VALUE
    Private m_tsInputDataCompatibility As TriState = TriState.UseDefault
    Private m_tsRunDataCompatibility As TriState = TriState.UseDefault
    Private TrajectoryCsv As StreamWriter
    Private Trajectory2Csv As List(Of StreamWriter)             'Trajectories2 is similar to trajectories apart from it each file contains only 1 group
    Private swFleetEffort As StreamWriter

#End Region ' Internal vars

#Region " Public Properties "

    Public Enum eRunStates As Byte
        Idle = 0
        RunningModels
        RunningMSE
    End Enum

    Public ReadOnly Property CurrentModelID As Integer
        Get
            Return m_currentModelID
        End Get
    End Property

    Public ReadOnly Property RealisedFs(iGroup As Integer, iTime As Integer) As Single
        Get
            Return m_RealisedFs(iGroup - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property RealisedLandedFs(iGroup As Integer, iTime As Integer) As Single
        Get
            Return m_RealisedLandedFs(iGroup - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property RealisedDiscardFs(iGroup As Integer, iTime As Integer) As Single
        Get
            Return m_RealisedDiscardFs(iGroup - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property ConsumptionThroughoutProjection(iPred As Integer, iPrey As Integer, iTime As Integer) As Single
        Get
            Return Me.m_ConsumptionThroughoutProjection(iTime, iPred, iPrey)
        End Get
    End Property

    Public ReadOnly Property LandingsThroughoutProjection(iGroup As Integer, iFleet As Integer, iTime As Integer) As Single
        Get
            Return Me.m_LandingsDiscardsThroughoutProjection(iTime, iFleet, iGroup, eCatchTypes.Landings)
        End Get
    End Property

    Public ReadOnly Property CatchesThroughoutProjection(iGroup As Integer, iFleet As Integer, iTime As Integer) As Single
        Get
            Return Me.m_LandingsDiscardsThroughoutProjection(iTime, iFleet, iGroup, eCatchTypes.Landings) + _
                Me.m_LandingsDiscardsThroughoutProjection(iTime, iFleet, iGroup, eCatchTypes.DiscardMortalities) + _
                Me.m_LandingsDiscardsThroughoutProjection(iTime, iFleet, iGroup, eCatchTypes.DiscardSurvivals)
        End Get
    End Property

    Public ReadOnly Property HighestValueGroup(iFleet As Integer, iTime As Integer) As String
        Get
            Return m_HighestValueSpecies(iFleet - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property ChokeGroup(iFleet As Integer, iTime As Integer) As String
        Get
            Return m_ChokeGroup(iFleet - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property DiscardsThroughoutProjection(iGroup As Integer, iFleet As Integer, iTime As Integer) As Single
        Get
            Return Me.m_LandingsDiscardsThroughoutProjection(iTime, iFleet, iGroup, eCatchTypes.DiscardMortalities) + _
                Me.m_LandingsDiscardsThroughoutProjection(iTime, iFleet, iGroup, eCatchTypes.DiscardSurvivals)
        End Get
    End Property

    Public ReadOnly Property HCR_F_Conservation(iGroup As Integer, iTime As Integer) As Single
        Get
            Return ConservationFs(iGroup - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property HCR_F_Target(iGroup As Integer, iTime As Integer) As Single
        Get
            Return TargetFs(iGroup - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property HCR_Quota_Conservation(iGroup As Integer, iFleet As Integer, iTime As Integer) As Single
        Get
            Return ConservationQuotas(iFleet - 1, iGroup - 1, iTime - 1)
        End Get
    End Property

    Public ReadOnly Property HCR_Quota_Target(iGroup As Integer, iFleet As Integer, iTime As Integer) As Single
        Get
            Return TargetQuotas(iFleet - 1, iGroup - 1, iTime - 1)
        End Get
    End Property


    Friend ReadOnly Property currentStrategy As Strategy
        Get
            'use the current strategy index to get the current 
            Return Me.Strategies.Item(Me.m_iCurStategy)
            'Return m_currentStrategy
        End Get
    End Property



    Public Property RunState As eRunStates
        Get
            Return Me.m_runstate
        End Get
        Set(value As eRunStates)
            If (value <> Me.m_runstate) Then
                Me.m_runstate = value
                Me.fireChangeRunState()
            End If
        End Set
    End Property

    Public ReadOnly Property Core As cCore
        Get
            Return Me.m_core
        End Get
    End Property

    Public ReadOnly Property Survivability As cSurvivability
        Get
            Return Me.m_survivability
        End Get
    End Property

    Public ReadOnly Property QuotaShares As cQuotaShares
        Get
            Return Me.m_quotashares
        End Get
    End Property

    Public ReadOnly Property BiomassLimits As cBiomassLimits
        Get
            Return Me.m_biomasslimits
        End Get
    End Property

    Public ReadOnly Property Strategies As Strategies
        Get
            Return m_strategies
        End Get
    End Property

    Public ReadOnly Property EffortLimits As cEffortLimits
        Get
            Return Me.m_effortlimits
        End Get
    End Property

    Public ReadOnly Property Diets As cDiets
        Get
            Return Me.m_diets
        End Get
    End Property

    Public ReadOnly Property StockAssessment As cStockAssessmentModel
        Get
            Return Me.m_StockAssessment
        End Get
    End Property

    Public ReadOnly Property EcosimData As cEcosimDatastructures
        Get
            Return Me._simdata
        End Get
    End Property

    Public ReadOnly Property EcopathData As cEcopathDataStructures
        Get
            Return Me._pathdata
        End Get
    End Property

    Public Property CoreMSEData As MSE.cMSEDataStructures
        Get
            Return Me.m_CoreMSEData
        End Get
        Set(value As MSE.cMSEDataStructures)
            Me.m_CoreMSEData = value
        End Set
    End Property

    Public Sub StopRun()
        If (Me.RunState <> eRunStates.Idle) Then
            Me.m_StopRun = True
        End If
    End Sub

#End Region

#Region " Construction "

    Public Sub New(ByVal Monitor As cMSEStateMonitor, pluginPoint As cMSEPluginPoint)
        Me.m_monitor = Monitor
        Me.m_plugin = pluginPoint

        m_rand = New Random(CInt(Date.Now.Ticks Mod Integer.MaxValue))

        Me.m_SyncObj = System.Threading.SynchronizationContext.Current
        'if there is no current context then create a new one on this thread. 
        'this happens if no interface has been created yet(I think...)
        If (Me.m_SyncObj Is Nothing) Then Me.m_SyncObj = New System.Threading.SynchronizationContext()

        'Create the wait object with the initial state signaled, the object can be used
        'Me.m_WaitObject = New System.Threading.ManualResetEvent(True)

        Me.InvalidateConfigurationState()
    End Sub

    Public Sub onCoreInitialized(EwECore As cCore, Ecopath As Ecopath.cEcoPathModel, Ecosim As Ecosim.cEcoSimModel)

        Me.m_core = EwECore
        Me.m_ecopath = Ecopath
        Me.m_ecosim = Ecosim

        'Check whether there are any forcing catch time series and if there are then flag an error - we need to change fisforced here into a similar boolean for catch
        'For iGrp = 1 To m_core.nGroups
        '    If m_ecosim.EcosimData.FisForced(iGrp) Then Stop
        'Next

        Me.InvalidateConfigurationState()

    End Sub

#End Region ' Construction

#Region " Public methods "

    ''' <summary>
    ''' Interactively resolve MSE folder conflicts.
    ''' </summary>
    Public Function ResolveMSEPathConflicts(ByVal bInteractive As Boolean) As Boolean

        ' Forget all we know
        Me.InvalidateConfigurationState(False)

        ' Check if input structure is missing
        If Not Me.IsInputStructureAvailable() Then
            ' Ask user to create folder structure
            If (bInteractive) And Me.AskUser(String.Format(My.Resources.PROMPT_DATAPATH_MISSING, Me.DataPath), eMessageReplyStyle.YES_NO) <> eMessageReply.OK Then
                ' #User abort: abandon process
                Return False
            End If
        Else
            ' Check if input structure is compatible
            ' JS: this check is for ow ruled out by disabling the 'Review dist params' button in the UI
            If Not Me.IsInputDataCompatible() Then
                If (bInteractive) And Me.AskUser("The selected folder is not compatible with the currently loaded model. If you continue, previously saved MSE settings will be lost. Do you wish to continue, delete existing MSE settings, and start anew?",
                               eMessageReplyStyle.YES_NO, eMessageImportance.Warning, eMessageReply.NO) = eMessageReply.NO Then
                    Return False
                End If
            Else
                Return True
            End If
        End If


        ' Make sure plug-in has all dirs
        Dim strPath As String = Me.DataPath
        For Each f As cMSEUtils.eMSEPaths In [Enum].GetValues(GetType(cMSEUtils.eMSEPaths))
            cFileUtils.IsDirectoryAvailable(cMSEUtils.MSEFolder(strPath, f), True)
            'bSuccess = bSuccess And cFileUtils.IsDirectoryAvailable(cMSEUtils.MSEFolder(strPath, f), True)
        Next

        ' --- BEGIN GENERATING ALL ESSENTIAL INPUT FILES FOR A NEW MSE FOLDER ---

        Me.GenerateEmptyDistributions()
        Me.GenerateDefaultSurviveDistributions()
        Me.GenerateDefaultDiets()
        Me.GenerateEmptyBiomassLimitsCSV()
        Me.GenerateEmptyEffortLimitsCSV()
        Me.GenerateEmptyQuotaSharesCSV()

        ' .. add more

        ' --- END GENERATING ALL ESSENTIAL INPUT FILES FOR A NEW MSE FOLDER ---


        ' Re-assess state
        Me.InvalidateConfigurationState(True)

#If DEBUG Then
        ' Panic in debug mode only
        'Stop
        Debug.Assert(Me.IsInputDataCompatible(), "Cefas MSE default data generation logic is not working")
#End If

        Return Me.IsInputDataCompatible()

    End Function

    Public Sub CheckNoForcedCatchesTimeseries()

        For iTimeSeries = 1 To m_core.nTimeSeries
            If m_core.EcosimTimeSeries(iTimeSeries).TimeSeriesType = eTimeSeriesType.CatchesForcing Then
                MessageBox.Show("You are applying forcing catch time series. The MSE Plugin does not handle forcing catch timeseries.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Throw New ApplicationException(Me.ToString & ".CheckNoForcedCatchesTimeseries(). ")
            End If
        Next

    End Sub

    Public Sub LoadSampledParams()

        Me.RunState = eRunStates.RunningModels
        Me.ChangeEffortFlag = True
        cApplicationStatusNotifier.StartProgress(Me.Core, "", -1)

        Try
            Me.CheckNoForcedCatchesTimeseries()
            Me.InvalidateConfigurationState(True)
            Me.Run()
        Catch ex As Exception
            ' Whoah!
            'This shouldn't happen, really....
            cLog.Write(ex, "CefasMSE:LoadSampledParameters")
        End Try

        cApplicationStatusNotifier.EndProgress(Me.Core)
        Me.ChangeEffortFlag = False
        Me.RunState = eRunStates.Idle

    End Sub

    Public Function CreateModels() As Boolean
        Dim bsuccess As Boolean = True

        If (Me.RunState <> eRunStates.Idle) Then
            Me.InformUser("Sorry already running.", eMessageImportance.Warning)
            Return False
        End If

        Me.m_StopRun = False
        Me.InvalidateConfigurationState(True)

        Dim RunThread As Threading.Thread

        RunThread = New Threading.Thread(AddressOf Me.runCreateModelsThread)
        RunThread.Name = "CEFAS_MSE"

        RunThread.Start()

        Return bsuccess

    End Function

    Public Sub GenerateDefaultSurviveDistributions()

        Dim TSurvivability As cSurvivability = New cSurvivability(Me, m_core, _simdata, _pathdata)
        TSurvivability.Save()

    End Sub

    Public Sub GenerateSurvivabilities()

        Dim TSurvivability As cSurvivability = New cSurvivability(Me, m_core, _simdata, _pathdata)

        TSurvivability.Load()
        TSurvivability.SampleParams(Me.NModels)
        TSurvivability.SaveSampledToCSV()
        TSurvivability.Save()

        Me.Survivability.Load()
        'Me.InvalidateConfigurationState()

    End Sub

    Public Sub GenerateEmptyBiomassLimitsCSV()
        Dim TBiomassLimits As cBiomassLimits = New cBiomassLimits(Me)
        TBiomassLimits.Save()
    End Sub

    Public Sub GenerateEmptyEffortLimitsCSV()
        Dim TEffortLimits As cEffortLimits = New cEffortLimits(Me, m_core)
        TEffortLimits.Save()
    End Sub

    Public Sub GenerateEmptyQuotaSharesCSV()
        Dim TQuotaShares As cQuotaShares = New cQuotaShares(Me, Core)
        TQuotaShares.Save()
    End Sub

    Public Function GenerateDefaultDiets() As Boolean
        ' JS 20Jul14: Diet writing moved to cDiets class
        Dim d As New cDiets(Me, Me.Core)
        Return d.Save()
    End Function

    Public Function GenerateEmptyDistributions() As Boolean

        Dim distpath As New cEcopathDistributionParams(Me, Me.Core)
        Dim distsim As New cEcosimDistributionParams(Me, Me.Core)
        Return distpath.Save() And distsim.Save()

    End Function

    Public Property WriteAllResults As Boolean = False
    Public Property WriteYearlyOnly As Boolean = False

#End Region ' Public methods

#Region " Threading "

    Private Sub runCreateModelsThread()

#If DEBUG Then
        Console.WriteLine("Starting creating models at: " & DateTime.Now.ToShortTimeString)
#End If

        Me.m_StopRun = False
        Me.m_core.StateMonitor.SetIsSearching(eSearchModes.External)
        Me.m_core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.StopRun))
        Me.RunState = eRunStates.RunningModels

        cApplicationStatusNotifier.StartProgress(Me.Core, "", -1)

        Try
            Me.GenerateEcosimParameters("MaxRelFeedingTime")
            Me.GenerateEcosimParameters("FeedingTimeAdjustRate")
            Me.GenerateEcosimParameters("OtherMortFeedingTime")
            Me.GenerateEcosimParameters("PredEffectFeedingTime")
            Me.GenerateEcosimParameters("DenDepCatchability")
            Me.GenerateEcosimParameters("QBMaxxQBio")
            Me.GenerateEcosimParameters("SwitchingPower")
            Me.GenerateSurvivabilities()
            Me.CreateVulnerabilities()
            Me.GenerateEcopathParamaters()

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.EndProgress(Me.Core)

        Me.m_core.StateMonitor.SetIsSearching(eSearchModes.NotInSearch)
        Me.m_core.SetStopRunDelegate(Nothing)
        Me.RunState = eRunStates.Idle

#If DEBUG Then
        Console.WriteLine("Finished creating models at: " & DateTime.Now.ToShortTimeString)
#End If

    End Sub

    Private Sub fireChangeRunState()
        'Make sure this gets called even if there is an exception
        m_SyncObj.Send(New System.Threading.SendOrPostCallback(AddressOf Me.SendEvent), Nothing)
    End Sub

    Private Sub SendEvent(obj As Object)
        Try
            ' Have the State monitor tell the world
            Me.m_monitor.Invalidate()
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".fireChangeRunState() Exception: " & ex.Message)
        End Try
    End Sub

#End Region ' Threading

#Region " Diagnostics and state management "

    ''' <summary>
    ''' Invalidate the known state of the MSE engine. Data may have been changed, directories
    ''' may have been swapped: MSE simply does not know its state any longer and will need to
    ''' reassess this at a next opportunity.
    ''' </summary>
    ''' <param name="bReloadData"></param>
    Friend Sub InvalidateConfigurationState(Optional bReloadData As Boolean = True)

        Me.m_iNumModelsAvailable = cCore.NULL_VALUE
        Me.m_iNumStrategiesAvailable = cCore.NULL_VALUE
        Me.m_tsInputDataCompatibility = TriState.UseDefault
        Me.m_tsRunDataCompatibility = TriState.UseDefault
        Me.m_monitor.Invalidate()

        ' Test whether core is up and running
        If (Me.m_core Is Nothing) Then Return
        ' Test whether a scenario is available
        If (Me.m_core.ActiveEcosimScenarioIndex < 1) Then Return
        ' Test whether MSE has been initialized
        If (Me.m_survivability Is Nothing) Then Return

        If (Not Me.IsInputStructureAvailable()) Then Return
        If (Not Me.IsInputDataCompatible()) Then Return
        If (Not bReloadData) Then Return

        cApplicationStatusNotifier.StartProgress(Me.m_core, My.Resources.STATUS_LOADING, -1)
        Try
            ' Reload possible data
            Me.EffortLimits.Load()
            Me.QuotaShares.Load()
            Me.Strategies.Load()
            Me.Diets.Load()
            Me.Survivability.LoadSampledParamsFromCSV()
            Me.Survivability.Load()
            Me.StockAssessment.Load()
        Catch ex As Exception

        End Try
        cApplicationStatusNotifier.EndProgress(Me.m_core)

    End Sub


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the MSE plug-in has all directories that it needs in the
    ''' <see cref="DataPath"></see>.
    ''' </summary>
    ''' <returns>True if the directory structure exists in its entirety.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsInputStructureAvailable() As Boolean

        Return Me.Survivability.FileExists()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether all base data is available for building models
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function IsInputDataCompatible() As Boolean

        ' JS 21Jul14: data compatibility is assessed by individual data classes now

        If (Me.m_tsInputDataCompatibility = TriState.UseDefault) Then

            ' Hope for the best
            Dim msgError As New cMessage("", eMessageType.Any, eCoreComponentType.External, eMessageImportance.Maintenance, eDataTypes.External)
            Me.m_tsInputDataCompatibility = TriState.True

            ' Make sure plug-in has empty CSV
            If Not Me.Diets.Load(msgError) Or Not Me.Survivability.Load(msgError) Then
                Me.m_tsInputDataCompatibility = TriState.False
            End If

            If (Me.m_tsInputDataCompatibility <> TriState.False) Then
                ' Assess Ecopath distributions
                Dim dist As New cEcopathDistributionParams(Me, Me.Core)
                If Not dist.Load(msgError) Then
                    Me.m_tsInputDataCompatibility = TriState.False
                End If
            End If

            If (Me.m_tsInputDataCompatibility <> TriState.False) Then
                ' Assess Ecosim distributions
                Dim dist As New cEcosimDistributionParams(Me, Me.Core)
                If Not dist.Load(msgError) Then
                    Me.m_tsInputDataCompatibility = TriState.False
                End If
            End If

            ' Input structure not compatible?
            If (Me.m_tsInputDataCompatibility = TriState.False) Then
                ' Prepare and send message
                msgError.Message = String.Format(My.Resources.PROMPT_DATAPATH_INCOMPATIBLE, Me.DataPath)
                msgError.Importance = eMessageImportance.Critical
                Me.m_core.Messages.SendMessage(msgError)
            End If

        End If

        Return (Me.m_tsInputDataCompatibility = TriState.True)

    End Function


    Public Function IsRunDataCompatible() As Boolean

        ' Would it not be nice if these file names were represented by enums as well?

        'Dim aFilesFleet As String() = New String() {"ChangesInEffortLimits", "QuotaShares"}
        'Dim strRoot As String = cMSEUtils.MSEFolder(Me.DataPath, cMSEUtils.eMSEPaths.Fleet)
        Dim outParamFiles As String() = New String() {"b_out", "ba_out", "ee_out", "pb_out", "qb_out", "DenDepCatchability_out", _
                                                      "FeedingTimeAdjustRate_out", "MaxRelFeedingTime_out", "OtherMortFeedingTime_out", _
                                                      "PredEffectFeedingTime_out", "QBMaxxQBio_out", "SwitchingPower_out"}

        If (Me.m_tsRunDataCompatibility = TriState.UseDefault) Then

            ' Hope for the best
            Me.m_tsRunDataCompatibility = TriState.True

            ' Make sure folder has out CSV files
            If Not File.Exists(cMSEUtils.MSEFile(Me.DataPath, cMSEUtils.eMSEPaths.Fleet, "ChangesInEffortLimits.csv")) Or _
                Not File.Exists(cMSEUtils.MSEFile(Me.DataPath, cMSEUtils.eMSEPaths.Fleet, "QuotaShares.csv")) Or _
                Not File.Exists(cMSEUtils.MSEFile(Me.DataPath, cMSEUtils.eMSEPaths.ParamsOut, "Survivabilities_out.csv")) Then
                Me.m_tsRunDataCompatibility = TriState.False
            End If

            ' Instead, test whether the data classes are populated with data:
            ' - Has fishing strategies?
            If (Me.NumStrategiesAvailable = 0) Then Me.m_tsRunDataCompatibility = TriState.False
            ' - Has quota shares? etc

            ' Assess Ecopath files
            For Each strFile As String In outParamFiles
                If Not File.Exists(cMSEUtils.MSEFile(Me.DataPath, cMSEUtils.eMSEPaths.ParamsOut, strFile & ".csv")) Then
                    Me.m_tsRunDataCompatibility = TriState.False
                    Exit For
                End If
            Next strFile

        End If

        Return (Me.m_tsRunDataCompatibility = TriState.True)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of pre-generated models found in the current <see cref="DataPath"/>.
    ''' </summary>
    ''' <returns>
    ''' The number of pre-generated models found in the current <see cref="DataPath"/>.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function NumModelsAvailable() As Integer

        If (Me.m_iNumModelsAvailable = cCore.NULL_VALUE) Then

            SyncLock Me
                Me.m_iNumModelsAvailable = 0
                ' JS 07Oct13: Very simple test
                Dim strPath As String = cMSEUtils.MSEFile(Me.DataPath, cMSEUtils.eMSEPaths.ParamsOut, "b_out.csv")
                If File.Exists(strPath) Then
                    Dim reader As StreamReader = cMSEUtils.GetReader(strPath)
                    If (reader IsNot Nothing) Then
                        reader.ReadLine()
                        While Not reader.EndOfStream
                            reader.ReadLine()
                            Me.m_iNumModelsAvailable += 1
                        End While
                    End If
                End If
            End SyncLock

        End If
        Return Me.m_iNumModelsAvailable

    End Function


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of fishing strategies found in the current <see cref="DataPath"/>.
    ''' </summary>
    ''' <returns>
    ''' The number of fishing strategies found in the current <see cref="DataPath"/>.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function NumStrategiesAvailable() As Integer

        If (Me.m_iNumStrategiesAvailable = cCore.NULL_VALUE) Then

            SyncLock Me
                Me.Strategies.Load()
                Me.m_iNumStrategiesAvailable = Me.Strategies.Count
            End SyncLock

        End If
        Return Me.m_iNumStrategiesAvailable

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether there are results in the <see cref="DataPath"/>.
    ''' </summary>
    ''' <returns>True if there are results in the <see cref="DataPath"/>.</returns>
    ''' -----------------------------------------------------------------------
    Public Function HasResults() As Boolean

        Dim strPath As String = cMSEUtils.MSEFolder(Me.DataPath, cMSEUtils.eMSEPaths.Results)
        Return File.Exists(Path.Combine(strPath, "results.csv"))

    End Function

    Private m_strModelCompatibility As String = ""

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get any recent compatibility assessment.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ModelCompatibilityInfo As String
        Get
            Return Me.m_strModelCompatibility
        End Get
    End Property

#End Region ' Diagnostics and state management

#Region " File I/O and other file related 'stuff' "

    Public ReadOnly Property DataPath As String
        Get
            If Me.UseEwEPath Then
                Return Path.Combine(Me.Core.DefaultOutputPath(EwEUtils.Core.eAutosaveTypes.Ecosim), "CefasMSE")
            End If
            Return Me.CustomPath
        End Get
    End Property

    Private Function ExtractParamsCSV(ByRef param_name As String) As Double(,)

        ' JS 09Oct13: Used standard readers/writers, and made robust

        Dim Params(,) As Double = Nothing
        Dim iRecord As Integer = 0
        Dim csv As CsvReader = Nothing
        Dim nIterations As Integer = Me.NModels2Run
        Dim reader As StreamReader = cMSEUtils.GetReader(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, param_name & "_out.csv"))

        If (reader Is Nothing) Then Return Params

        csv = New CsvReader(reader, True)
        ReDim Params(nIterations - 1, csv.FieldCount - 1)
        Try
            While Not csv.EndOfStream And iRecord < nIterations
                If csv.ReadNextRecord() Then
                    For iField = 1 To csv.FieldCount()
                        Params(iRecord, iField - 1) = cStringUtils.ConvertToDouble(csv(iField - 1))
                    Next
                End If
                iRecord += 1
            End While
        Catch ex As Exception
            ' ToDo: decide what to do when CSV data is malformed
        End Try

        csv.Dispose()
        cMSEUtils.ReleaseReader(reader)

        Return Params

    End Function

    Private Function ExtractVulnerabilitiesCSV() As Double(,,)

        ' JS 09Oct13: Used standard readers/writers, and made robust

        Dim nIterations As Integer = Me.NModels2Run
        Dim csv As CsvReader
        Dim vulnerabilities(nIterations - 1, m_ecopath.EcopathData.NumGroups - 1, m_ecopath.EcopathData.NumGroups - 1) As Double

        For iIteration As Integer = 1 To nIterations
            Dim reader As StreamReader = cMSEUtils.GetReader(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, "VulnerabilityIteration" & iIteration.ToString & "_out.csv"))
            If (reader IsNot Nothing) Then
                csv = New CsvReader(reader, False)
                Try
                    While Not csv.EndOfStream
                        If csv.ReadNextRecord() Then
                            For iPrey As Integer = 1 To m_ecopath.EcopathData.NumGroups
                                vulnerabilities(iIteration - 1, CInt(csv.CurrentRecordIndex), iPrey - 1) = cStringUtils.ConvertToDouble(csv(iPrey - 1))
                            Next
                        End If
                    End While
                Catch ex As Exception
                    ' ToDo: decide what to do when CSV data is malformed
                End Try
                csv.Dispose()
                cMSEUtils.ReleaseReader(reader)
            End If
        Next

        Return vulnerabilities

    End Function

    ''' <summary>
    ''' Append an Ecopath variable to a CSV out file.
    ''' </summary>
    ''' <param name="strFile"></param>
    ''' <param name="data"></param>
    ''' <returns>True if successful.</returns>
    Private Function WriteEcopathParms(strFile As String, data As Single()) As Boolean

        Dim strPath As String = cMSEUtils.MSEFile(Me.DataPath, cMSEUtils.eMSEPaths.ParamsOut, strFile)
        Dim writer As StreamWriter = Nothing

        If Not File.Exists(strPath) Then
            writer = cMSEUtils.GetWriter(strPath)
            If (writer Is Nothing) Then Return False

            For igrp As Integer = 1 To Me.Core.nLivingGroups
                If (igrp > 1) Then writer.Write(",")
                writer.Write(cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(igrp).Name))
            Next

        Else
            writer = cMSEUtils.GetWriter(strPath, True)
            If (writer Is Nothing) Then Return False
        End If

        writer.WriteLine()
        For igrp As Integer = 1 To Me.Core.nLivingGroups
            If (igrp > 1) Then writer.Write(",")
            writer.Write(data(igrp))
        Next
        cMSEUtils.ReleaseWriter(writer)
        Return True

    End Function

    Private Function InitMonteCarloParamX(ByVal strPath As String, ByVal ParamName As eParamName) As Boolean

        Dim csvParamX As CsvReader
        Dim MonteCarlo As cMonteCarloManager = m_core.EcosimMonteCarlo
        Dim MCGroup As cMonteCarloGroup
        Dim xgrp As Integer

        ' ToDo: merge with self-load capabilities of cDistributionParameters

        Try

            'If Not CheckEcopathDistributionFilesOkay(strPath) Then Return False

            csvParamX = New CsvReader(New StreamReader(strPath), True) ' I think this is to restart the reading of the csv

            For igrp = 1 To m_core.nLivingGroups

                xgrp = 1
                If (Not csvParamX.EndOfStream) And (csvParamX.ReadNextRecord()) Then
                    'Make sure that .csv files are set up with group names in same order because xgrp is found only from the B file
                    'and then is assumed to be the same for all other files
                    While MonteCarlo.Groups(xgrp).Name <> csvParamX(1) 'And xgrp <= mCore.nLivingGroups
                        xgrp += 1
                    End While

                    MCGroup = MonteCarlo.Groups(xgrp)

                    'Setting a CV value will automatically set the Lower and Upper limits
                    'by Calling cEcosimMonteCarlo.CalculateUpperLowerLimits()
                    'If you want to manually set limits it must be done after the CV has been set

                    'CVs
                    If ParamName = eParamName.B Then
                        MCGroup.Bcv = cStringUtils.ConvertToSingle(csvParamX(2))
                        MCGroup.BLower = cStringUtils.ConvertToSingle(csvParamX(3))
                        MCGroup.BUpper = cStringUtils.ConvertToSingle(csvParamX(4))
                    End If

                    If ParamName = eParamName.PB Then
                        MCGroup.PBcv = cStringUtils.ConvertToSingle(csvParamX(2))
                        MCGroup.PBLower = cStringUtils.ConvertToSingle(csvParamX(3))
                        MCGroup.PBUpper = cStringUtils.ConvertToSingle(csvParamX(4))
                    End If

                    If ParamName = eParamName.QB Then
                        MCGroup.QBcv = cStringUtils.ConvertToSingle(csvParamX(2))
                        MCGroup.QBLower = cStringUtils.ConvertToSingle(csvParamX(3))
                        MCGroup.QBUpper = cStringUtils.ConvertToSingle(csvParamX(4))
                    End If

                    If ParamName = eParamName.EE Then
                        MCGroup.EEcv = cStringUtils.ConvertToSingle(csvParamX(2))
                        MCGroup.EELower = cStringUtils.ConvertToSingle(csvParamX(3))
                        MCGroup.EEUpper = cStringUtils.ConvertToSingle(csvParamX(4))
                    End If

                    If ParamName = eParamName.BA Then
                        MCGroup.BAcv = cStringUtils.ConvertToSingle(csvParamX(2))
                        MCGroup.BALower = cStringUtils.ConvertToSingle(csvParamX(3))
                        MCGroup.BAUpper = cStringUtils.ConvertToSingle(csvParamX(4))
                    End If
                Else
                    ' ToDo_JS: Error reading CSV content. How to respond?
                End If

            Next '========================================================================================================================================================

            'reset the connection to the csv files ready to be read from the beginning again
            csvParamX.Dispose()

            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".InitMonteCarloParameters() Exception: " & ex.Message)
        End Try

        Return False

    End Function

    Enum eParamName As Integer
        B
        PB
        QB
        EE
        BA
    End Enum

    Private Function InitMonteCarloParameters() As Boolean

        'loads the distribution parameters for the Ecopath parameters from csvs
        If Not InitMonteCarloParamX(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, "B_Dist.csv"), eParamName.B) Then Return False
        If Not InitMonteCarloParamX(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, "PB_Dist.csv"), eParamName.PB) Then Return False
        If Not InitMonteCarloParamX(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, "QB_Dist.csv"), eParamName.QB) Then Return False
        If Not InitMonteCarloParamX(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, "EE_Dist.csv"), eParamName.EE) Then Return False
        If Not InitMonteCarloParamX(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, "BA_Dist.csv"), eParamName.BA) Then Return False

        Return True

    End Function

    Private Function GenerateEcosimParameters(ByVal ParamName As String) As Boolean

        Dim strPath As String = cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, ParamName & ".csv")

        'If Not CheckEcoSimDistributionFilesOkay(strPath) Then Return False

        Dim reader As StreamReader = Nothing
        Dim csv As CsvReader = Nothing
        Dim ParameterArray(m_core.nLivingGroups - 1, 3) As Single

        ' JS 30Sep13: Use local properties
        Dim nModels As Integer = Me.NModels
        Dim eDistributionType As DistributionType
        Dim SampledParameters(nModels - 1, m_core.nLivingGroups - 1) As Double
        Dim GroupNames(m_core.nLivingGroups - 1) As String

        reader = cMSEUtils.GetReader(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DistrParams, ParamName & ".csv"))
        If (reader IsNot Nothing) Then
            csv = New CsvReader(reader, True)
            'Read all the distribution information from the .csv file and into an array ParameterArray
            Try

                While Not csv.EndOfStream
                    If csv.ReadNextRecord() Then
                        GroupNames(CInt(csv.CurrentRecordIndex)) = cMSEUtils.FromCSVField(csv("GroupName"))
                        For iField = 2 To 5
                            ParameterArray(CInt(csv.CurrentRecordIndex), iField - 2) = cStringUtils.ConvertToSingle(csv(iField))
                        Next
                    End If
                End While
            Catch ex As Exception

            End Try
            csv.Dispose()
        End If

        cMSEUtils.ReleaseReader(reader)

        'Generate an array of sample parameters
        For iGroup = 1 To m_core.nLivingGroups

            If Not ParameterArray(iGroup - 1, 1) = cCore.NULL_VALUE Then

                eDistributionType = CType(ParameterArray(iGroup - 1, 0), DistributionType)

                For iModel = 1 To nModels

                    Select Case eDistributionType
                        Case DistributionType.Uniform
                            SampledParameters(iModel - 1, iGroup - 1) = UniformSample(ParameterArray(iGroup - 1, 1), ParameterArray(iGroup - 1, 2))
                        Case DistributionType.Triangular
                            SampledParameters(iModel - 1, iGroup - 1) = TriangularSample(ParameterArray(iGroup - 1, 1), ParameterArray(iGroup - 1, 2), ParameterArray(iGroup - 1, 3))
                    End Select

                Next
            Else
                For iModel = 1 To nModels
                    SampledParameters(iModel - 1, iGroup - 1) = cCore.NULL_VALUE
                Next
            End If

        Next

        'Output the sampled parameters to a csv
        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, ParamName & "_out.csv"))
        If (writer IsNot Nothing) Then
            Try
                For igrp As Integer = 1 To m_core.nLivingGroups
                    'If igrp = m_core.nLivingGroups Then Stop
                    If (igrp > 1) Then writer.Write(",")
                    If GroupNames(igrp - 1) <> Nothing Then writer.Write(cStringUtils.ToCSVField(GroupNames(igrp - 1)))
                Next
                writer.WriteLine()

                For iIteration = 1 To nModels
                    For iGroup = 1 To m_core.nLivingGroups
                        'If iGroup = m_core.nLivingGroups Then Stop
                        If (iGroup > 1) Then writer.Write(",")
                        If GroupNames(iGroup - 1) <> Nothing Then writer.Write(cStringUtils.ToCSVField(SampledParameters(iIteration - 1, iGroup - 1)))
                    Next
                    writer.WriteLine()
                Next
            Catch ex As Exception
                ' ToDo: respond to error, somehow
            End Try
        End If
        cMSEUtils.ReleaseWriter(writer)
        Return True

    End Function

    ''' <summary>
    ''' Generate csv with vulnerabilities.
    ''' </summary>
    Private Sub CreateVulnerabilities()

        ' JS 13Oct13: Fixed path usage
        ' JS 13Oct13: Used standard CSV field reading/writing
        ' JS 30Sep13: Used persistent properties

        Dim writer As StreamWriter = Nothing
        Dim nIterations As Integer = NModels

        For iIteration = 1 To nIterations

            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, "VulnerabilityIteration" & iIteration & "_out.csv"), False)
            If (writer IsNot Nothing) Then
                'Create random values for the vulnerabilities and store in a csv
                For igrppredator As Integer = 1 To m_ecopath.EcopathData().NumLiving
                    If m_core.EcoPathGroupInputs(igrppredator).IsProducer Then
                        For igrpprey As Integer = 1 To m_ecopath.EcopathData().NumGroups
                            If (igrpprey > 1) Then writer.Write(",")
                            writer.Write(Convert.ToSingle(cCore.NULL_VALUE))
                        Next igrpprey
                    Else
                        For igrpprey As Integer = 1 To m_ecopath.EcopathData().NumGroups
                            If (igrpprey > 1) Then writer.Write(",")
                            writer.Write(Convert.ToSingle(1 + Math.Exp(9 * (CSng(Me.m_rand.NextDouble()) - 0.5))))
                        Next igrpprey
                    End If
                    writer.WriteLine()
                Next igrppredator
            Else
                ' Hmm, writer could not be created?!
            End If
            cMSEUtils.ReleaseWriter(writer)
        Next

    End Sub

    'Private Function LoadCostFunctionsCSV()
    '    Dim CostFunctionReader As CsvReader
    '    Dim CostFunctionArray(,) As String
    '    Dim CostFunctionArrayIndex As Integer = 0
    '    Dim StratFileNames() As String
    '    Dim FileName As String
    '    Dim FoundElement As Boolean

    '    'Get the names of the files in the strategies folder
    '    StratFileNames = Directory.GetFiles(DataPath & "\Strategies")

    '    'Redim the cost function array so that there are enough rows for each strategy
    '    ReDim CostFunctionArray(StratFileNames.Count - 2, 1)

    '    'Setup csvreader object
    '    CostFunctionReader = New CsvReader(New StreamReader(DataPath & "\Strategies\CostFunctionType.csv"), True)

    '    'Read the data in CostFunctionType.csv into the CostFunctionArray
    '    While Not CostFunctionReader.EndOfStream
    '        CostFunctionReader.ReadNextRecord()
    '        CostFunctionArray(CostFunctionArrayIndex, 0) = CostFunctionReader(0)
    '        CostFunctionArray(CostFunctionArrayIndex, 1) = CostFunctionReader(1)
    '        CostFunctionArrayIndex += 1
    '    End While

    '    'Check that all files in the strategies folder are represented in the CostFunctionArray
    '    For Each iFile In StratFileNames
    '        FileName = Path.GetFileName(iFile)
    '        If FileName = "CostFunctionType.csv" Then Continue For 'skip over the file that holds the costfunctions

    '        FoundElement = False
    '        For iCostFunctionElement = 0 To CostFunctionArray.GetLength(0) - 1
    '            If FileName = CostFunctionArray(iCostFunctionElement, 0) & ".csv" Then FoundElement = True
    '        Next

    '        If FoundElement = False Then
    '            Err.Raise(1000, "LoadCostFunctionsCSV", "Strategy file not listed in the CostFunctionType.csv file")
    '        End If

    '    Next

    '    Return CostFunctionArray

    'End Function

    Public Sub DeleteResults()

        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.Results, "Fleet.csv")
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.Results, "Results.csv")
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.Results, "EffortTrajectories.csv")
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.Results, "BadDynamicsTrajectories.csv")
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.CatchTrajectories)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.DiscardsTrajectories)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.HCRF_Cons)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.HCRF_Targ)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.HCRQuota_Cons)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.HCRQuota_Targ)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.LandingsTrajectories)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.RealisedF)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.RealisedLandedF)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.RealisedDiscardF)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.ValueTrajectories)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.PredationMortality)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.Biomass)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.Effort)
        Me.SafeDeleteResult(cMSEUtils.eMSEPaths.HighestValueGroup)

    End Sub

    Private Sub SafeDeleteResult(ByVal category As cMSEUtils.eMSEPaths,
                                 Optional strFile As String = "")

        Dim strPath As String = cMSEUtils.MSEFolder(Me.DataPath, category)
        Dim lstrFiles As New List(Of String)

        If (Not String.IsNullOrWhiteSpace(strFile)) Then
            lstrFiles.Add(Path.Combine(strPath, strFile))
        Else
            lstrFiles.AddRange(Directory.GetFiles(strPath))
        End If

        For Each strFile In lstrFiles
            If (File.Exists(strFile)) Then
                Try
                    File.Delete(strFile)
                Catch ex As Exception
                    ' Whoah!
                    cLog.Write(ex, eVerboseLevel.Detailed, "CEFAS.cMSE::SafeDeleteResult(" & strFile & ")")
                End Try
            End If
        Next

    End Sub

#End Region ' File I/O

#Region " MonteCarlo state save and restore "

    ''' <summary>
    ''' Save any variable that will be changed so the model can be restore to it's original state 
    ''' </summary>
    ''' <remarks>This just stores a sub set of variable as an example</remarks>
    Private Sub SaveOriginalState()
        Try
            'Have the MonteCarloManager save the values it will alter
            m_core.EcosimMonteCarlo.SaveOriginalValues()

            'Now store the variables that this app will change so they can be restored in RestoreOriginalState()

            'The makes sure Ecopath does not make a fuss, popping up message boxes, when it fails to balance a model
            Me.m_ecopath.suppressMessages = True

            'Make sure nothing is listening to Ecosim when we run it
            Me._EcosimTimeStepDelegate = Me.m_ecosim.TimeStepDelegate
            Me.m_ecosim.TimeStepDelegate = Nothing

            'Save any parameters that we are going to change 
            'This has not been implemented here but...
            'For igrp = 1 To Core.nLivingGroups
            '    MCGroup = MonteCarlo.Groups(igrp)
            '   _orgB(igrp) =  MCGroup.Bcv 
            '    'PB, QB...               
            'Next

        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' Restore the currently loaded model back to its original state so that it can be run in the interface.
    ''' </summary>
    ''' <remarks>In some cases you may want to save changes you made to the model.</remarks>
    Private Sub RestoreOriginalState()
        Try

            Dim iscenario As Integer = Me.m_core.ActiveEcosimScenarioIndex

            'Have the MonteCarloManager restore it's variables to the original state
            Me.RestoreParameters()
            Me.m_ecosim.TimeStepDelegate = Me._EcosimTimeStepDelegate

            ' No database changes left, yippee
            Me.Core.DiscardChanges()

            'Closing and reloading the Ecosim Scenario causes all time series to be unloaded
            'If this happens and the user runs the MSE again it will on longer be in the same state as the previous run
            'Core.CloseEcosimScenario()
            'Me.Core.LoadEcosimScenario(iscenario)

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub SaveOriginalParameters()

        Dim ecopathData As cEcopathDataStructures = Me.m_ecopath.EcopathData
        Dim ecosimData As cEcosimDatastructures = Me.m_ecosim.EcosimData

        ReDim BTemp(ecopathData.B.Length - 1)
        ReDim PBTemp(ecopathData.PB.Length - 1)
        ReDim QBTemp(ecopathData.QB.Length - 1)
        ReDim EETemp(ecopathData.EE.Length - 1)
        ReDim BATemp(ecopathData.BA.Length - 1)
        ReDim DenDepCatchabilityTemp(ecosimData.QmQo.Length - 1)
        ReDim FeedingTimeAdjustRateTemp(ecosimData.FtimeAdjust.Length - 1)
        ReDim MaxRelFeedingTimeTemp(ecosimData.FtimeMax.Length - 1)
        ReDim OtherMortFeedingTimeTemp(ecosimData.MoPred.Length - 1)
        ReDim PredEffectFeedingTimeTemp(ecosimData.RiskTime.Length - 1)
        ReDim QBMaxxQBioTemp(ecosimData.CmCo.Length - 1)
        ReDim SwitchingPowerTemp(ecosimData.SwitchPower.Length - 1)
        ReDim VulnerabilitiesTemp(ecosimData.VulMult.GetLength(0) - 1, ecosimData.VulMult.GetLength(1) - 1)
        ReDim DietMatrixTemp(ecopathData.DC.GetLength(0) - 1, ecopathData.DC.GetLength(1) - 1)
        ReDim DietImpTemp(m_core.nGroups - 1)

        For x = 0 To ecopathData.B.Length - 1
            BTemp(x) = ecopathData.B(x)
            PBTemp(x) = ecopathData.PB(x)
            QBTemp(x) = ecopathData.QB(x)
            EETemp(x) = ecopathData.EE(x)
            BATemp(x) = ecopathData.BA(x)
            DenDepCatchabilityTemp(x) = ecosimData.QmQo(x)
            FeedingTimeAdjustRateTemp(x) = ecosimData.FtimeAdjust(x)
            MaxRelFeedingTimeTemp(x) = ecosimData.FtimeMax(x)
            OtherMortFeedingTimeTemp(x) = ecosimData.MoPred(x)
            PredEffectFeedingTimeTemp(x) = ecosimData.RiskTime(x)
            QBMaxxQBioTemp(x) = ecosimData.CmCo(x)
            SwitchingPowerTemp(x) = ecosimData.SwitchPower(x)
        Next
        For x = 0 To ecopathData.DC.GetLength(0) - 1
            For y = 0 To ecopathData.DC.GetLength(1) - 1
                DietMatrixTemp(x, y) = ecopathData.DC(x, y)
            Next
            'DietImpTemp(x) = mCore.EcoPathGroupInputs(x + 1).ImpDiet
        Next
        For x = 1 To ecosimData.VulMult.GetLength(0) - 1
            For y = 0 To ecosimData.VulMult.GetLength(1) - 1
                VulnerabilitiesTemp(x, y) = ecosimData.VulMult(x, y)
            Next
        Next

        OriginalNTimesteps = m_ecosim.EcosimData.NTimes

    End Sub

    Private Sub RestoreParameters()

        Dim ecopathData As cEcopathDataStructures = Me.m_ecopath.EcopathData
        Dim ecosimData As cEcosimDatastructures = Me.m_ecosim.EcosimData

        For x = 0 To ecopathData.B.Length - 1
            ecopathData.B(x) = CSng(BTemp(x))
            ecopathData.PB(x) = CSng(PBTemp(x))
            ecopathData.QB(x) = CSng(QBTemp(x))
            ecopathData.EE(x) = CSng(EETemp(x))
            ecopathData.BA(x) = CSng(BATemp(x))
            ecosimData.QmQo(x) = CSng(DenDepCatchabilityTemp(x))
            ecosimData.FtimeAdjust(x) = CSng(FeedingTimeAdjustRateTemp(x))
            ecosimData.FtimeMax(x) = CSng(MaxRelFeedingTimeTemp(x))
            ecosimData.MoPred(x) = CSng(OtherMortFeedingTimeTemp(x))
            ecosimData.RiskTime(x) = CSng(PredEffectFeedingTimeTemp(x))
            ecosimData.CmCo(x) = CSng(QBMaxxQBioTemp(x))
            ecosimData.SwitchPower(x) = CSng(SwitchingPowerTemp(x))
        Next

        For x = 0 To ecopathData.DC.GetLength(0) - 1
            For y = 0 To ecopathData.DC.GetLength(1) - 1
                ecopathData.DC(x, y) = CSng(DietMatrixTemp(x, y))
            Next
        Next

        'I don't think we should do this 
        'We never changed the input/output dietmatrix
        'For x = 1 To mCore.nGroups
        '    For y = 1 To mCore.nLivingGroups
        '        mCore.EcoPathGroupInputs(x).DietComp(y) = CSng(DietMatrixTemp(x - 1, y - 1))
        '    Next
        'Next

        For x = 1 To ecosimData.VulMult.GetLength(0) - 1
            For y = 0 To ecosimData.VulMult.GetLength(1) - 1
                ecosimData.VulMult(x, y) = CSng(VulnerabilitiesTemp(x, y))
            Next
        Next

        Me.Core.DiscardChanges()

    End Sub

#End Region ' MonteCarlo state save and restore

#Region " Private modeling code "

    ''' <summary>
    ''' Resets the effort to the maximum specifed effort for the project time steps
    ''' </summary>
    ''' <remarks>The effort is used is the effort determined through regulation unless greater than this maximum effort</remarks>
    Private Sub ResetEffortToMax(StartT As Integer, EndT As Integer)
        Dim MSEMaxEffort As Single = 200

        Try

            For iflt As Integer = 1 To m_ecopath.EcopathData.NumFleet
                'Only if this fleet is regulated

                For it As Integer = StartT To EndT
                    _simdata.FishRateGear(iflt, it) = MSEMaxEffort
                Next it

            Next iflt

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub init_ResultCollectors_and_Writers(ByRef Collectors As List(Of cResultsCollector_Base), ByRef Writers As List(Of cResultsWriter_Base), ByVal msgReport As cMessage)

        Dim RealisedTotalF_Collector = New cResultsCollector_RealisedTotalFs
        Dim RealisedTotalF_Collector_Yearly = New cResultsCollector_RealisedTotalFs_Yearly
        Dim RealisedLandedF_Collector = New cResultsCollector_RealisedLandedFs
        Dim RealisedLandedF_Collector_Yearly = New cResultsCollector_RealisedLandedFs_Yearly
        Dim RealisedDiscardedF_Collector = New cResultsCollector_RealisedDiscardedFs
        Dim RealisedDiscardedF_Collector_Yearly = New cResultsCollector_RealisedDiscardedFs_Yearly
        Dim CatchesTotal_Collector = New cResultsCollector_TotalCatch
        Dim CatchesTotal_Collector_Yearly = New cResultsCollector_TotalCatch_Yearly
        Dim Landings_Collector = New cResultsCollector_Landings
        Dim Landings_Collector_Yearly = New cResultsCollector_Landings_Yearly
        Dim Discards_Collector = New cResultsCollector_Discards
        Dim Discards_Collector_Yearly = New cResultsCollector_Discards_Yearly
        Dim HCR_F_Cons_Collector = New cResultsCollector_HCR_F_Cons
        Dim HCR_F_Targ_Collector = New cResultsCollector_HCR_F_Targ
        Dim HCR_Quota_Cons_Collector = New cResultsCollector_HCR_Quota_Cons
        Dim HCR_Quota_Targ_Collector = New cResultsCollector_HCR_Quota_Targ
        Dim Value_Collector = New cResultsCollector_Value
        Dim Value_Collector_Yearly = New cResultsCollector_Value_Yearly
        Dim Biomass_Collector = New cResultsCollector_Biomass
        Dim Biomass_Collector_Yearly = New cResultsCollector_Biomass_Yearly
        Dim HighestValueGroup_Collector = New cResultsCollector_HighestValueGroup
        Dim ChokeGroup_Collector = New cResultsCollector_ChokeGroup
        Dim Effort_Collector = New cResultsCollector_Efforts
        Dim Effort_Collector_Yearly = New cResultsCollector_Efforts_Yearly
        Dim PredationMortality_Collector = New cResultsCollector_PredationMortality
        Dim PredationMortality_Collector_Yearly = New cResultsCollector_PredationMortality_Yearly
        Dim PredationMortalityPreyOnly_Collector = New cResultsCollector_PredationMortality_PreyOnly
        Dim PredationMortalityPreyOnly_Yearly_Collector = New cResultsCollector_PredationMortality_PreyOnly_Yearly

        If Not Me.WriteYearlyOnly Then
            Collectors.Add(Biomass_Collector)
            Collectors.Add(RealisedTotalF_Collector)
            Collectors.Add(RealisedLandedF_Collector)
            Collectors.Add(RealisedDiscardedF_Collector)
            Collectors.Add(CatchesTotal_Collector)
            Collectors.Add(Landings_Collector)
            Collectors.Add(Discards_Collector)
            Collectors.Add(Value_Collector)
            Collectors.Add(Effort_Collector)
            'Collectors.Add(PredationMortality_Collector)
            'Collectors.Add(PredationMortalityPreyOnly_Collector)
        End If

        Collectors.Add(Biomass_Collector_Yearly)
        Collectors.Add(RealisedTotalF_Collector_Yearly)
        Collectors.Add(RealisedLandedF_Collector_Yearly)
        Collectors.Add(RealisedDiscardedF_Collector_Yearly)
        Collectors.Add(CatchesTotal_Collector_Yearly)
        Collectors.Add(Landings_Collector_Yearly)
        Collectors.Add(Discards_Collector_Yearly)
        Collectors.Add(HCR_F_Cons_Collector)
        Collectors.Add(HCR_F_Targ_Collector)
        Collectors.Add(HCR_Quota_Cons_Collector)
        Collectors.Add(HCR_Quota_Targ_Collector)
        Collectors.Add(Value_Collector_Yearly)
        Collectors.Add(Effort_Collector_Yearly)
        Collectors.Add(HighestValueGroup_Collector)
        Collectors.Add(ChokeGroup_Collector)
        Collectors.Add(PredationMortality_Collector_Yearly)
        Collectors.Add(PredationMortalityPreyOnly_Yearly_Collector)


        For Each iCollector In Collectors
            iCollector.Initialise(Me)
        Next

        If Not Me.WriteYearlyOnly Then

            Dim Biomass_Writer = New cResultsWriter_1DArray
            Biomass_Writer.Initialise(msgReport, Me, Biomass_Collector, cMSEUtils.eMSEPaths.Biomass)
            Writers.Add(Biomass_Writer)

            Dim RealisedTotalFs_Writer = New cResultsWriter_1DArray
            RealisedTotalFs_Writer.Initialise(msgReport, Me, RealisedTotalF_Collector, cMSEUtils.eMSEPaths.RealisedF)
            Writers.Add(RealisedTotalFs_Writer)

            Dim RealisedLandedFs_Writer = New cResultsWriter_1DArray
            RealisedLandedFs_Writer.Initialise(msgReport, Me, RealisedLandedF_Collector, cMSEUtils.eMSEPaths.RealisedLandedF)
            Writers.Add(RealisedLandedFs_Writer)

            Dim RealisedDiscardedFs_Writer = New cResultsWriter_1DArray
            RealisedDiscardedFs_Writer.Initialise(msgReport, Me, RealisedDiscardedF_Collector, cMSEUtils.eMSEPaths.RealisedDiscardF)
            Writers.Add(RealisedDiscardedFs_Writer)

            Dim TotalCatch_Writer = New cResultsWriter_2DArray
            TotalCatch_Writer.Initialise(msgReport, Me, CatchesTotal_Collector, cMSEUtils.eMSEPaths.CatchTrajectories)
            Writers.Add(TotalCatch_Writer)

            Dim Landings_Writer = New cResultsWriter_2DArray
            Landings_Writer.Initialise(msgReport, Me, Landings_Collector, cMSEUtils.eMSEPaths.LandingsTrajectories)
            Writers.Add(Landings_Writer)

            Dim Discards_Writer = New cResultsWriter_2DArray
            Discards_Writer.Initialise(msgReport, Me, Discards_Collector, cMSEUtils.eMSEPaths.DiscardsTrajectories)
            Writers.Add(Discards_Writer)

            Dim Value_Writer = New cResultsWriter_2DArray
            Value_Writer.Initialise(msgReport, Me, Value_Collector, cMSEUtils.eMSEPaths.ValueTrajectories)
            Writers.Add(Value_Writer)

            Dim Effort_Writer = New cResultsWriter_1DArray
            Effort_Writer.Initialise(msgReport, Me, Effort_Collector, cMSEUtils.eMSEPaths.Effort)
            Writers.Add(Effort_Writer)

            'Dim PredationMortality_Writer = New cResultsWriter_2DArray_Group_Group
            'PredationMortality_Writer.Initialise(msgReport, Me, PredationMortality_Collector, cMSEUtils.eMSEPaths.PredationMortality)
            'Writers.Add(PredationMortality_Writer)

            'Dim PredationMortalityPreyOnly_Writer = New cResultsWriter_1DArray
            'PredationMortalityPreyOnly_Writer.Initialise(msgReport, Me, PredationMortalityPreyOnly_Collector, cMSEUtils.eMSEPaths.PredationMortalityPreyOnly)
            'Writers.Add(PredationMortalityPreyOnly_Writer)

        End If

        Dim ChokeGroup_Writer = New cResultsWriter_1DArray
        ChokeGroup_Writer.Initialise(msgReport, Me, ChokeGroup_Collector, cMSEUtils.eMSEPaths.ChokeGroup)

        Dim HighestValueGroup_Writer = New cResultsWriter_1DArray
        HighestValueGroup_Writer.Initialise(msgReport, Me, HighestValueGroup_Collector, cMSEUtils.eMSEPaths.HighestValueGroup)

        Dim RealisedTotalFs_Writer_Yearly = New cResultsWriter_1DArray
        RealisedTotalFs_Writer_Yearly.Initialise(msgReport, Me, RealisedTotalF_Collector_Yearly, cMSEUtils.eMSEPaths.RealisedF)

        Dim RealisedLandedFs_Writer_Yearly = New cResultsWriter_1DArray
        RealisedLandedFs_Writer_Yearly.Initialise(msgReport, Me, RealisedLandedF_Collector_Yearly, cMSEUtils.eMSEPaths.RealisedLandedF)

        Dim RealisedDiscardFs_Writer_Yearly = New cResultsWriter_1DArray
        RealisedDiscardFs_Writer_Yearly.Initialise(msgReport, Me, RealisedDiscardedF_Collector_Yearly, cMSEUtils.eMSEPaths.RealisedDiscardF)

        Dim TotalCatch_Writer_Yearly = New cResultsWriter_2DArray
        TotalCatch_Writer_Yearly.Initialise(msgReport, Me, CatchesTotal_Collector_Yearly, cMSEUtils.eMSEPaths.CatchTrajectories)

        Dim Landings_Writer_Yearly = New cResultsWriter_2DArray
        Landings_Writer_Yearly.Initialise(msgReport, Me, Landings_Collector_Yearly, cMSEUtils.eMSEPaths.LandingsTrajectories)

        Dim Discards_Writer_Yearly = New cResultsWriter_2DArray
        Discards_Writer_Yearly.Initialise(msgReport, Me, Discards_Collector_Yearly, cMSEUtils.eMSEPaths.DiscardsTrajectories)

        Dim HCR_Fs_Cons_Writer = New cResultsWriter_1DArray
        HCR_Fs_Cons_Writer.Initialise(msgReport, Me, HCR_F_Cons_Collector, cMSEUtils.eMSEPaths.HCRF_Cons)

        Dim HCR_Fs_Targ_Writer = New cResultsWriter_1DArray
        HCR_Fs_Targ_Writer.Initialise(msgReport, Me, HCR_F_Targ_Collector, cMSEUtils.eMSEPaths.HCRF_Targ)

        Dim HCR_Quotas_Targ_Writer = New cResultsWriter_2DArray
        HCR_Quotas_Targ_Writer.Initialise(msgReport, Me, HCR_Quota_Targ_Collector, cMSEUtils.eMSEPaths.HCRQuota_Targ)

        Dim HCR_Quotas_Cons_Writer = New cResultsWriter_2DArray
        HCR_Quotas_Cons_Writer.Initialise(msgReport, Me, HCR_Quota_Cons_Collector, cMSEUtils.eMSEPaths.HCRQuota_Cons)

        Dim Value_Writer_Yearly = New cResultsWriter_2DArray
        Value_Writer_Yearly.Initialise(msgReport, Me, Value_Collector_Yearly, cMSEUtils.eMSEPaths.ValueTrajectories)

        Dim Biomass_Writer_Yearly = New cResultsWriter_1DArray
        Biomass_Writer_Yearly.Initialise(msgReport, Me, Biomass_Collector_Yearly, cMSEUtils.eMSEPaths.Biomass)

        Dim Effort_Writer_Yearly = New cResultsWriter_1DArray
        Effort_Writer_Yearly.Initialise(msgReport, Me, Effort_Collector_Yearly, cMSEUtils.eMSEPaths.Effort)

        Dim PredationMortality_Writer_Yearly = New cResultsWriter_2DArray_Group_Group
        PredationMortality_Writer_Yearly.Initialise(msgReport, Me, PredationMortality_Collector_Yearly, cMSEUtils.eMSEPaths.PredationMortality)

        Dim PredationMortalityPreyOnly_Writer_Yearly = New cResultsWriter_1DArray
        PredationMortalityPreyOnly_Writer_Yearly.Initialise(msgReport, Me, PredationMortalityPreyOnly_Yearly_Collector, cMSEUtils.eMSEPaths.PredationMortalityPreyOnly)

        Writers.Add(Effort_Writer_Yearly)
        Writers.Add(RealisedTotalFs_Writer_Yearly)
        Writers.Add(RealisedLandedFs_Writer_Yearly)
        Writers.Add(RealisedDiscardFs_Writer_Yearly)
        Writers.Add(TotalCatch_Writer_Yearly)
        Writers.Add(Landings_Writer_Yearly)
        Writers.Add(Discards_Writer_Yearly)
        Writers.Add(HCR_Fs_Cons_Writer)
        Writers.Add(HCR_Fs_Targ_Writer)
        Writers.Add(HCR_Quotas_Cons_Writer)
        Writers.Add(HCR_Quotas_Targ_Writer)
        Writers.Add(Value_Writer_Yearly)
        Writers.Add(Biomass_Writer_Yearly)
        Writers.Add(HighestValueGroup_Writer)
        Writers.Add(ChokeGroup_Writer)
        Writers.Add(PredationMortality_Writer_Yearly)
        Writers.Add(PredationMortalityPreyOnly_Writer_Yearly)

    End Sub


    Private Sub Run()

        ' JS 20Oct13: Fixed path usage
        ' JS 20Oct13: Used standard CSV field reading/writing; all names CSV protected and values written in US-en notation
        ' JS 20Oct13: Applied standard EwE headers. Mark, please don't kill me

        Debug.Assert(Me.IsInputDataCompatible)
        Dim nModels As Integer
        Dim GoodDynamics As Boolean
        ' Dim diet_matrix As CsvReader
        Dim nResultIters As Integer
        Dim nFleetIters As Integer
        Dim nFailedModels As Integer
        Dim BiomassLimits As cBiomassLimits
        Dim ModelValid As Boolean

        Dim swGroup As StreamWriter = Nothing
        Dim swFleet As StreamWriter = Nothing
        Dim swBadDynamics As StreamWriter = Nothing
        Dim ListofResultCollectors As New List(Of cResultsCollector_Base)
        Dim ListofResultWriters As New List(Of cResultsWriter_Base)

        Dim FleetCatchTable As DataTable
        Dim ResultsTable As DataTable

        ReDim m_FleetImpError(m_core.nFleets)

        Try

            Me.StockAssessment.BeginRun()

            'Dim BiomassProjected(Me.NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1) As Double

            Dim msgReport As New cFeedbackMessage("?", eCoreComponentType.External, eMessageType.DataExport, eMessageImportance.Information, eMessageReplyStyle.OK)
            msgReport.Hyperlink = cMSEUtils.MSEFolder(Me.DataPath, cMSEUtils.eMSEPaths.Results)

            'Save the original Ecopath and Ecosim parameter values 
            'so the model can be restored at the end of the run
            Me.SaveOriginalParameters()

            m_implementation_error = 0

            'Set the TechnologyCreep(nfleets) to one for all fleets
            'No technology creep for us
            Me.initTechnologyCreep()

            'Open the "Results.csv" and "Fleet.cvs" file and write the header info
            Me.initResultFiles(msgReport, swGroup, swFleet)

            Me.initQuotaEffortArrays()

            'Read all the parameters from the <parameter name>_out.csv files into memory
            Me.readEcopathEcosimParameters()

            swBadDynamics = Me.initBadDynamicsFile(msgReport)

            init_ResultCollectors_and_Writers(ListofResultCollectors, ListofResultWriters, msgReport)

            'increase the number of years for the projection
            m_core.EcoSimModelParameters.NumberYears = CInt(OriginalNTimesteps / m_ecosim.EcosimData.NumStepsPerYear + NYearsProject)

            'Tell Ecopath not to send out messages
            Me.m_ecopath.suppressMessages = True

            'Initialise and load from CSV the biomass limits
            BiomassLimits = New cBiomassLimits(Me)
            BiomassLimits.Load()

#If DEBUG Then
            'output the headings of the csv for the F results to test whether F steps correctly
            'OutputFHeadings()
#End If

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Run The Trials 
            'load parameter values into ecopath and ecosim to be used
            nModels = Me.NModels2Run    '0 is the 1st dimension and 1' the second etc
            m_PassedChangeInFAtBeginProjTest = True
            For iModel = 1 To nModels

                m_currentModelID = iModel
                ModelValid = True

                Me.initResultsTables(FleetCatchTable, ResultsTable)
                Debug.Assert(FleetCatchTable IsNot Nothing And ResultsTable IsNot Nothing, Me.ToString + ".Run() initResultsTables() Failed to create output tables.")

                cApplicationStatusNotifier.UpdateProgress(Me.Core, String.Format(My.Resources.STATUS_RUN_PROGRESS, My.Resources.CAPTION, iModel), CSng(iModel / nModels))

                'Only run the Strategies if the parameters loaded
                If Me.updateEcopathEcosimParameters(iModel) Then
                    'Yep loaded all the parameters from file or memory

                    Try
                        'Run Ecopath with the parameters updated above
                        Dim bEcopathRan As Boolean
                        bEcopathRan = Me.m_ecopath.Run()
                        'this should not happen 
                        Debug.Assert(bEcopathRan, Me.ToString + ".Run() Ecopath failed to run from balanced parameter set.")

                        Me.initQModifier()

                        Me.m_StockAssessment.TrialNumber = iModel

                        'initialize the base fishing mortality rates to the new ecopath parameters loaded above
                        'Me.initBaseCatchRate()

                        For Each iResultCollector In ListofResultCollectors
                            iResultCollector.Init_for_iModel(iModel)
                        Next

                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        'Loop over all the strategies for this trial
                        'Iterate over a static array of strategies instead of the list
                        'because the Strategies can reload at any time
                        Me.m_iCurStategy = 0
                        Dim StratArray() As Strategy = Strategies.ToArray()
                        For istrat As Integer = 0 To StratArray.Count - 1

                            'reset the seed for the random number generator of implementation and observation errors so that 
                            'the same errors are applied for a model across all strategies
                            Me.m_StockAssessment.ResetSeed(iModel)

                            'Re-load any Ecosim forcing data for the hind cast period
                            Me.setFishForcedToBase()

                            'Set the index to the CurrentStrategy used by me.currentStrategy to retrieve the correct strategy
                            m_iCurStategy = istrat

                            'Only run strategies that have been set to run
                            If Not currentStrategy.RunThisStrategy Then Continue For

                            'Initialise Arrays for recording the F's from Targ and Cons HCR's
                            InitArraysForStrategy()

                            ResetEffortToMax(OriginalNTimesteps + 1, m_core.EcoSimModelParameters.NumberYears * m_ecosim.EcosimData.NumStepsPerYear)

                            'Get a list of all fleets that fish the groups that have HCRs
                            'Populates FleetsTheFishHCRGroup() which is used by onEcosimTimeStep() to optimize the fleets it loops over
                            Me.initFishedByHCR(Me.currentStrategy)

                            'Clear out the catch results from the last HCR
                            Me.initCatchResults()

                            Check_All_Necessary_QuotaShares_Exist()

                            If Me.RunEcosim() Then
                                'Save the Ecosim results

                                For Each iResultCollector In ListofResultCollectors
                                    iResultCollector.Populate()
                                Next

                                GoodDynamics = Me.SaveResults2RAM(iModel, Me.currentStrategy, nResultIters, nFleetIters, BiomassLimits, swBadDynamics, ResultsTable, FleetCatchTable)
                                If GoodDynamics = False And Not Me.WriteAllResults Then
                                    nFailedModels += 1
                                    Exit For
                                End If

                            Else
                                GoodDynamics = False
                                nFailedModels += 1
                                Exit For
                            End If

                            Me.clearQModifiers()

                        Next istrat
                        'End of Strategy loop
                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                        If GoodDynamics Or Me.WriteAllResults Then
                            SaveResults2CSV(iModel, FleetCatchTable, ResultsTable, swFleet, swGroup)
                        End If

                        If GoodDynamics Or Me.WriteAllResults Then
                            For Each iWriter In ListofResultWriters
                                iWriter.WriteResults()
                            Next
                        End If

                    Catch ex As Exception
                        Debug.Assert(False, Me.ToString & ".Run() Exception: " & ex.Message)
                        If ex.Message = "EwEMSEPlugin.cMSE.Check_All_Necessary_QuotaShares_Exist(). " Then Throw ex
                    End Try
                End If

                'BadDynamics:  ' This is so that if the dynamics of a parameterisation are bad that we can skip out of the loops and onto the the next Trial
                GoodDynamics = True

                Me.clearQModifiers()

            Next iModel
            'End of trials loop
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            Debug.Assert(m_PassedChangeInFAtBeginProjTest, "Somewhere the jump in F from end of hindcast to beginning of forecast is too big. See ")


            'ecosimData.NTimes is the number of months so 17 years = 204 timesteps
            m_core.EcoSimModelParameters.NumberYears = CInt(OriginalNTimesteps / m_ecosim.EcosimData.NumStepsPerYear)

            'Provide user with a message stating how many of the Trials produced reasonable dynamics
            msgReport.Message = String.Format(My.Resources.PROMPT_TRIAL_REPORT, (nModels - nFailedModels), nModels, CInt((nModels - nFailedModels) * 100 / nModels))
            Me.Core.Messages.SendMessage(msgReport)

        Catch ex As Exception
            cLog.Write(ex)
            'Warn the user
            Me.Core.Messages.SendMessage(New cMessage("CEFAS MSE Failed to run MSE trials due to exception " + ex.Message,
                                                      eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Information))
        End Try

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Cleanup even if there has been an exception

        For Each iWriter In ListofResultWriters
            iWriter.ReleaseWriters()
        Next

        cMSEUtils.ReleaseWriter(swGroup)
        cMSEUtils.ReleaseWriter(swFleet)
        cMSEUtils.ReleaseWriter(swBadDynamics)

        Me.StockAssessment.RunEnded()

        Me.RestoreOriginalState()
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    End Sub

    Private Sub Check_All_Necessary_QuotaShares_Exist()

        For iGrp = 1 To m_core.nGroups
            If currentStrategy.StrategyContainsHCRforiGrp(iGrp) Then
                For iFleet = 1 To m_core.nFleets
                    If m_core.EcopathFleetInputs(iFleet).Landings(iGrp) > 0 Then
                        If Not m_quotashares.QuotaShareExistsForGroupFleet(iGrp, iFleet) Then
                            MessageBox.Show("A quotashare, that is not specified for fleet " & m_core.EcopathFleetInputs(iFleet).Name & " group " & m_core.EcoPathGroupInputs(iGrp).Name & " is required. Please specify and then rerun", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Throw New ApplicationException(Me.ToString & ".Check_All_Necessary_QuotaShares_Exist(). ")
                        End If
                    End If
                Next
            End If
        Next

    End Sub

    Private Sub SaveResults2CSV(ByRef iModel As Integer, ByRef FleetCatchTable As DataTable, ByRef GroupResultsTable As DataTable,
                     ByRef swFleet As StreamWriter, ByRef swGroup As StreamWriter)


        'Save the FleetCatchTable to CSV
        'swFleet.WriteLine("Model,Strategy,FleetNumber,FleetName,GroupNumber,GroupName,Value")
        For iRow = 0 To FleetCatchTable.Rows.Count - 1
            Dim RowData As Data.DataRow = FleetCatchTable.Rows(iRow)
            swFleet.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("ModelID")),
                                cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")),
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("FleetNumber")),
                                cStringUtils.ToCSVField(RowData.Field(Of String)("FleetName")),
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("GroupNumber")),
                                cStringUtils.ToCSVField(RowData.Field(Of String)("GroupName")),
                                cStringUtils.FormatNumber(RowData.Field(Of Single)("CatchAtLastTimeStep")),
                                cStringUtils.FormatNumber(RowData.Field(Of Single)("DiscardSurvivals")),
                                cStringUtils.FormatNumber(RowData.Field(Of Single)("DiscardMortalities")))
        Next

        'Save the GroupResultsTable to CSV
        'swGroup.WriteLine("Model,Strategy,ID,Name,ResultName,ResultValue")
        For iRow = 0 To GroupResultsTable.Rows.Count - 1
            Dim RowData As Data.DataRow = GroupResultsTable.Rows(iRow)
            swGroup.WriteLine("{0},{1},{2},{3},{4},{5}",
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("ModelID")),
                                cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")),
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("ID")),
                                cStringUtils.ToCSVField(RowData.Field(Of String)("Name")),
                                cStringUtils.ToCSVField(RowData.Field(Of String)("ResultName")),
                                cStringUtils.FormatNumber(RowData.Field(Of Double)("ResultValue")))
        Next


    End Sub

    Private Sub InitArraysForStrategy()


        'Initialise Arrays for recording the F's from Targ and Cons HCR's
        ReDim TargetFs(m_core.nGroups - 1, NYearsProject - 1)
        ReDim ConservationFs(m_core.nGroups - 1, NYearsProject - 1)
        ReDim TargetQuotas(m_core.nFleets - 1, m_core.nGroups - 1, NYearsProject - 1)
        ReDim ConservationQuotas(m_core.nFleets - 1, m_core.nGroups - 1, NYearsProject - 1)
        For iGrp = 1 To m_core.nGroups
            For iYear = 1 To NYearsProject
                TargetFs(iGrp - 1, iYear - 1) = -9999
                ConservationFs(iGrp - 1, iYear - 1) = -9999
                For iFleet = 1 To m_core.nFleets
                    TargetQuotas(iFleet - 1, iGrp - 1, iYear - 1) = -9999
                    ConservationQuotas(iFleet - 1, iGrp - 1, iYear - 1) = -9999
                Next
            Next
        Next

        ReDim m_HighestValueSpecies(m_core.nFleets - 1, NYearsProject - 1)
        For iFleet = 1 To m_core.nFleets
            For iYear = 1 To NYearsProject
                m_HighestValueSpecies(iFleet - 1, iYear - 1) = "NA"
            Next
        Next

        ReDim m_ChokeGroup(m_core.nFleets - 1, NYearsProject * EcosimData.NumStepsPerYear - 1)
        For iFleet = 1 To m_core.nFleets
            For iTimestep = 1 To NYearsProject * EcosimData.NumStepsPerYear
                m_ChokeGroup(iFleet - 1, iTimestep - 1) = "NA"
            Next
        Next


        'Initialise arrays for recording the realised F's
        ReDim m_RealisedFs(m_core.nGroups - 1, NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1)
        For iGrp = 1 To m_core.nGroups
            For iTimeStep = 1 To NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                m_RealisedFs(iGrp - 1, iTimeStep - 1) = -9999
            Next
        Next

        'Initialise arrays for recording the realised landed F's
        ReDim m_RealisedLandedFs(m_core.nGroups - 1, NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1)
        For iGrp = 1 To m_core.nGroups
            For iTimeStep = 1 To NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                m_RealisedLandedFs(iGrp - 1, iTimeStep - 1) = -9999
            Next
        Next

        'Initialise arrays for recording the realised landed F's
        ReDim m_RealisedDiscardFs(m_core.nGroups - 1, NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1)
        For iGrp = 1 To m_core.nGroups
            For iTimeStep = 1 To NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                m_RealisedDiscardFs(iGrp - 1, iTimeStep - 1) = -9999
            Next
        Next

    End Sub


    Private Sub initResultsTables(ByRef FleetCatchTable As DataTable, ByRef ResultsTable As DataTable)

        FleetCatchTable = New DataTable
        ResultsTable = New DataTable

        FleetCatchTable.Columns.Add("ModelID", GetType(Integer))
        FleetCatchTable.Columns.Add("StrategyName", GetType(String))
        FleetCatchTable.Columns.Add("FleetNumber", GetType(Integer))
        FleetCatchTable.Columns.Add("FleetName", GetType(String))
        FleetCatchTable.Columns.Add("GroupNumber", GetType(Integer))
        FleetCatchTable.Columns.Add("GroupName", GetType(String))
        FleetCatchTable.Columns.Add("CatchAtLastTimeStep", GetType(Single))
        FleetCatchTable.Columns.Add("DiscardSurvivals", GetType(Single))
        FleetCatchTable.Columns.Add("DiscardMortalities", GetType(Single))

        ResultsTable.Columns.Add("ModelID", GetType(Integer))
        ResultsTable.Columns.Add("StrategyName", GetType(String))
        ResultsTable.Columns.Add("ID", GetType(Integer))
        ResultsTable.Columns.Add("Name", GetType(String))
        ResultsTable.Columns.Add("ResultName", GetType(String))
        ResultsTable.Columns.Add("ResultValue", GetType(Double))

    End Sub

    Private Sub initFishedByHCR(curStrategy As Strategy)
        'Clear the data from the last Strategy
        FleetsThatFishHCRGrp.Clear()

        'Get a list of all fleets that fish the groups that have HCRs
        For iFleet As Integer = 1 To m_core.nFleets
            For Each HCRGroup As HCR_Group In curStrategy
                If m_core.EcopathFleetInputs(iFleet).Landings(HCRGroup.GroupF.Index) + m_core.EcopathFleetInputs(iFleet).Discards(HCRGroup.GroupF.Index) > 0 Then
                    If Not FleetsThatFishHCRGrp.Contains(iFleet) Then
                        FleetsThatFishHCRGrp.Add(iFleet)
                    End If
                    'Exit For
                End If
            Next HCRGroup
        Next iFleet

    End Sub

    Private Sub initCatchResults()

        Dim nCatchTypes As Integer = [Enum].GetNames(GetType(eCatchTypes)).Length
        Me.LandingsDiscards = New Single(m_core.nFleets, m_core.nGroups, nCatchTypes) {}

        'Initialise arrays for recording the realised F's
        Me.m_LandingsDiscardsThroughoutProjection = New Single(NYearsProject * EcosimData.NumStepsPerYear, m_core.nFleets, m_core.nGroups, nCatchTypes) {}
        For iGrp = 1 To m_core.nGroups
            For iTimeStep = 1 To NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                For iFleet = 0 To m_core.nFleets
                    For iCatchType = 1 To nCatchTypes
                        m_LandingsDiscardsThroughoutProjection(iTimeStep, iFleet, iGrp, iCatchType) = 0
                    Next
                Next
            Next
        Next

    End Sub

    Private Sub initTechnologyCreep()

        'an array where each element represents the percentage with which each fleet increases its catching efficiency each year
        TechnologyCreep = New Single(m_core.nFleets) {}
        For iTechCreep As Integer = 1 To m_core.nFleets
            TechnologyCreep(iTechCreep) = 1
        Next

    End Sub

    Private Sub initQuotaEffortArrays()

        ReDim TargConsQuota(m_core.nGroups - 1, 1)
        ReDim MaxEffortThisYear(m_core.nFleets - 1)

    End Sub



    ''' <summary>
    ''' Read the DietMatrix file for this trial and populate the Ecopath diet matrix
    ''' </summary>
    ''' <param name="iTrial"></param>
    ''' <returns></returns>
    Private Function updateDietMatrixFromCSVFile(ByVal iTrial As Integer) As Boolean

        Dim GoodDynamics As Boolean = True
        Dim csvDietMatrix As CsvReader = Nothing
        Dim strmReader As StreamReader = cMSEUtils.GetReader(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, "DietMatrixTrial" & iTrial & ".csv"))

        If (strmReader IsNot Nothing) Then
            csvDietMatrix = New CsvReader(strmReader, False)
            If csvDietMatrix.ReadNextRecord() Then
                For iPred As Integer = 1 To m_core.nLivingGroups
                    'NOT DCInput because we don't have the support that copies this into DC
                    If cStringUtils.ConvertToSingle(csvDietMatrix(iPred - 1)) < MIN_DIET_PROP Then
                        _pathdata.DC(iPred, 0) = 0
                    Else
                        _pathdata.DC(iPred, 0) = cStringUtils.ConvertToSingle(csvDietMatrix(iPred - 1))
                    End If
                Next
            Else
                ' Unable to read predator header line! We have a problem
                GoodDynamics = False
            End If

            'Me.dumpDietMatrix()
            For iPrey As Integer = 1 To m_core.nGroups
                If (Not csvDietMatrix.EndOfStream) And (csvDietMatrix.ReadNextRecord()) Then
                    For iPred As Integer = 1 To m_core.nLivingGroups
                        'If m_ecopath.EcopathData.DC(iPred, iPrey) > 0 Then
                        'Debug.Assert(cStringUtils.ConvertToSingle(csvDietMatrix(iPred - 1)) > 0)
                        'NOT DCInput because we don't have the support that copies this into DC
                        If cStringUtils.ConvertToSingle(csvDietMatrix(iPred - 1)) < MIN_DIET_PROP Then
                            _pathdata.DC(iPred, iPrey) = 0
                        Else
                            _pathdata.DC(iPred, iPrey) = cStringUtils.ConvertToSingle(csvDietMatrix(iPred - 1))
                        End If
                    Next
                Else
                    ' Unable to read prey line! We have a problem
                    GoodDynamics = False
                End If
            Next

            Me.NormalizeDiet(_pathdata.DC)

        Else
            ' Could not read diet matrix for this trial
            GoodDynamics = False
        End If

        cMSEUtils.ReleaseReader(strmReader)
        If csvDietMatrix IsNot Nothing Then csvDietMatrix.Dispose()

        'Me.dumpDietMatrix()

        Return GoodDynamics

    End Function

    Private Sub DumpFishingEffort()

        Try
            Dim strm As New System.IO.StreamWriter("C:\Users\Mark\Desktop\GAP\Data\Results\fishingEffort.csv", True)
            strm.WriteLine("iter")

            strm.WriteLine("-----------------Start Fishing Effort Matrix-----------------------")
            For iFleet As Integer = 1 To m_core.nFleets
                strm.Write("Fleet = " & m_core.EcosimFleetInputs(iFleet).Name & ",")
                For iTimeStep As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                    strm.Write(Me._simdata.ResultsEffort(iFleet, iTimeStep).ToString() + ",")
                Next
                strm.WriteLine()
            Next
            strm.WriteLine("-----------------Start Fishing Effort Matrix------------------------")
            'strm.Close()
            cMSEUtils.ReleaseWriter(strm)
        Catch ex As Exception
            ' Aargh
        End Try

    End Sub

    ''' <summary>
    ''' Used for debugging to test the diet matrix created here against the Ecopath diet matrix 
    ''' </summary>
    ''' <remarks>
    ''' There is a equivalent method in Ecopath that dumps the diet matrix use for the current iteration out to file. 
    ''' These file can then be compared.
    ''' </remarks>
    Private Sub dumpDietMatrix()

        Try
            Dim strm As New System.IO.StreamWriter("MSEDietMatrix.csv", True)
            strm.WriteLine("iter")

            strm.WriteLine("-----------------Start Diet Matrix-----------------------")
            For iprey As Integer = 1 To m_core.nGroups
                For ipred As Integer = 1 To m_core.nLivingGroups
                    strm.Write(Me._pathdata.DCInput(ipred, iprey).ToString() + ",")
                Next
                strm.WriteLine()
            Next
            strm.WriteLine("-----------------End Diet Matrix------------------------")
            cMSEUtils.ReleaseWriter(strm)
        Catch ex As Exception
            ' Aargh
        End Try

    End Sub

    Private Function SaveResults2RAM(ByVal iModel As Integer, ByVal currentStrat As Strategy, ByRef NumberIterationsAlreadyInResults As Integer,
                                        ByVal NumberIterationsAlreadyInFleets As Integer, ByRef BiomassLimits As cBiomassLimits,
                                        ByRef swBadDynamics As StreamWriter, ByRef ResultsTable As DataTable, ByRef FleetCatchTable As DataTable) As Boolean
        ', ByRef FleetEffortTable As DataTable, _
        'ByRef TrajectoryTable As DataTable) As Boolean

        Dim GoodDynamics As Boolean = True

        Dim TempCalcedFleetEndValue As Single

        Dim BiomassProjected(NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1) As Double

        nSuccessfullyProjectedModels = 0

        'This outputs information that can be used to resolve issues with the biomass limits are exceeded
        'Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, "BadDynamicsTrajectories.csv"), True)
        Dim MaxBiomass As Single
        Dim MinBiomass As Single

        For Each iGrp In BiomassLimits.lstBiomassLimits
            MaxBiomass = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, OriginalNTimesteps + 1)
            MinBiomass = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, OriginalNTimesteps + 1)
            For iTimeStep As Integer = OriginalNTimesteps + 2 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                If Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, iTimeStep) > MaxBiomass Then
                    MaxBiomass = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, iTimeStep)
                End If
                If Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, iTimeStep) < MinBiomass Then
                    MinBiomass = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, iTimeStep)
                End If
            Next
            swBadDynamics.WriteLine()
            swBadDynamics.Write("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        cStringUtils.FormatNumber(iModel),
                        cStringUtils.ToCSVField(Me.currentStrategy.Name),
                        cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp.mGroup.Index).Name),
                        cStringUtils.FormatNumber(MaxBiomass),
                        cStringUtils.FormatNumber(iGrp.mUpperLimit),
                        cStringUtils.FormatNumber(MaxBiomass / iGrp.mUpperLimit),
                        cStringUtils.FormatNumber(MaxBiomass > iGrp.mUpperLimit),
                        cStringUtils.FormatNumber(MinBiomass),
                        cStringUtils.FormatNumber(iGrp.mLowerLimit),
                        cStringUtils.FormatNumber(MinBiomass <= iGrp.mLowerLimit))
        Next

        'Check whether the biomass for any species goes beneath or hits zero
        For iGrp As Integer = 1 To m_core.nLivingGroups
            For iTimeStepHistoric As Integer = 1 To OriginalNTimesteps
                'Console.Write(mCore.EcoSimGroupOutputs(iGrp).Biomass(iTimeStep).ToString & " ")
                If Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp, iTimeStepHistoric) <= 1.0E-20 Then
                    GoodDynamics = False
                End If
                'If mCore.EcoSimGroupOutputs(iGrp).Biomass(iTimeStep) <= 0 Then GoodDynamics = False
                'Test
                If GoodDynamics = False Then
                    Exit For
                End If
            Next
            If GoodDynamics = False Then Exit For
        Next iGrp
        'Code to test whether the efforts are reasonable
        'DumpFishingEffort()

        For Each iGrp In BiomassLimits.lstBiomassLimits
            For iTimeStep As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                'Check projection is above minimum biomass
                If Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, iTimeStep) <= iGrp.mLowerLimit Then
                    GoodDynamics = False
                    'GoodDynamics = True ' test to make it output the biomass trajectories - todo comment out
                    Exit For
                End If
                'check the projection is above the max biomass
                If Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp.mGroup.Index, iTimeStep) > iGrp.mUpperLimit Then
                    GoodDynamics = False
                    'GoodDynamics = True ' test to make it output the biomass trajectories - todo comment out
                    Exit For
                End If
            Next
            If GoodDynamics = False Then Exit For
        Next

        If GoodDynamics = False And Not Me.WriteAllResults Then
            Console.WriteLine("This set of parameters is no good")
        Else


            'Output the trajectories of the efforts
            'For iFleet As Integer = 1 To m_core.nFleets
            '    Dim EffortVals(OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1) As Single
            '    For iTime As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
            '        EffortVals(iTime - 1) = Me._simdata.ResultsEffort(iFleet, iTime)
            '    Next
            '    FleetEffortTable.Rows.Add(iModel, m_currentStrategy.Name, iFleet, m_core.FleetInputs(iFleet).Name, EffortVals)
            'Next

            For iFleet As Integer = 1 To m_core.nFleets
                For iGrp As Integer = 1 To m_core.nLivingGroups
                    Dim landings As Single = Me.LandingsDiscards(iFleet, iGrp, eCatchTypes.Landings)
                    FleetCatchTable.Rows.Add(iModel + NumberIterationsAlreadyInFleets, Me.currentStrategy.Name,
                    iFleet, m_core.EcopathFleetInputs(iFleet).Name, iGrp, m_core.EcoPathGroupInputs(iGrp).Name,
                    Me.LandingsDiscards(iFleet, iGrp, eCatchTypes.Landings),
                    Me.LandingsDiscards(iFleet, iGrp, eCatchTypes.DiscardSurvivals),
                    Me.LandingsDiscards(iFleet, iGrp, eCatchTypes.DiscardMortalities))

                    'FleetCatchTable.Rows.Add(iModel + NumberIterationsAlreadyInFleets, m_currentStrategy.Name, _
                    '                         iFleet, m_core.FleetInputs(iFleet).Name, iGrp, m_core.EcoPathGroupInputs(iGrp).Name, _
                    '                        Me._simdata.ResultsSumCatchByGroupGear(iGrp, iFleet, OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear))
                Next
            Next

            For iGrp As Integer = 1 To m_core.nLivingGroups
                'calculate what the minimum biomass was for each group
                For iTime As Integer = 1 To NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                    BiomassProjected(iTime - 1) = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp, OriginalNTimesteps + iTime)
                Next

                Dim BiomassVals(OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear - 1) As Single
                For iTime As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
                    BiomassVals(iTime - 1) = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp, iTime)
                Next
                'TrajectoryTable.Rows.Add(iModel, m_currentStrategy.Name, iGrp, m_core.EcoPathGroupInputs(iGrp).Name, BiomassVals)

                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iGrp,
                                           m_core.EcoPathGroupOutputs(iGrp).Name, "BiomassMin", BiomassProjected.Min)
                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iGrp,
                                           m_core.EcoPathGroupOutputs(iGrp).Name, "BiomassEnd", Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp, Me._simdata.NTimes)) 'confirmed that _simdata.NTimes is the last timestep

                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iGrp,
                                           m_core.EcoPathGroupOutputs(iGrp).Name, "Catch", Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, iGrp, Me._simdata.NTimes)) 'confirmed that _simdata.NTimes is the last timestep

                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iGrp,
                                          m_core.EcoPathGroupOutputs(iGrp).Name, "Landings", Me.LandingsDiscards(0, iGrp, eCatchTypes.Landings))

                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iGrp,
                                          m_core.EcoPathGroupOutputs(iGrp).Name, "DiscardMortalities", Me.LandingsDiscards(0, iGrp, eCatchTypes.DiscardMortalities))

                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iGrp,
                                          m_core.EcoPathGroupOutputs(iGrp).Name, "DiscardSurvivals", Me.LandingsDiscards(0, iGrp, eCatchTypes.DiscardSurvivals))
            Next

            'Save the market value for each fleet of what was landed
            For iFleet As Integer = 1 To m_core.nFleets
                TempCalcedFleetEndValue = 0
                For iGrp As Integer = 1 To m_core.nGroups
                    TempCalcedFleetEndValue += Me.LandingsDiscards(iFleet, iGrp, eCatchTypes.Landings) * EcopathData.Market(iFleet, iGrp)
                Next
                ResultsTable.Rows.Add(NumberIterationsAlreadyInResults + iModel, Me.currentStrategy.Name, iFleet,
                                      m_core.EcopathFleetInputs(iFleet).Name, "TotalEndValue", TempCalcedFleetEndValue)
            Next

            'Save the HCR F's to a datatable
            'HCR_F_Table.Rows.Add(m_currentStrategy.Name, TargetFs, ConservationFs)

            'Save the HCR Quotas to a datatable
            'HCR_Quota_Table.Rows.Add(m_currentStrategy.Name, TargetQuotas, ConservationQuotas)

            'Save the realised F's to a datatable
            'Realised_F_Table.Rows.Add(m_currentStrategy.Name, m_RealisedFs)

            'Save the realised Landed F's to a datatable
            'Realised_Landed_F_Table.Rows.Add(m_currentStrategy.Name, m_RealisedLandedFs)

            'Save the realised Discard F's to a datatable
            'Realised_Discard_F_Table.Rows.Add(m_currentStrategy.Name, m_RealisedDiscardFs)

            'Save the catch trajectory results to RAM
            'CatchTrajTable.Rows.Add(m_currentStrategy.Name, m_LandingsDiscardsThroughoutProjection)

            'Save what the highest value species was for each year of the last run to RAM
            'HighestValueSpeciesTable.Rows.Add(m_currentStrategy.Name, m_HighestValueSpecies)

            'Save what the choke species was for each time step of each fleet to RAM
            'ChokeGroupTable.Rows.Add(m_currentStrategy.Name, m_ChokeGroup)

        End If

        Return GoodDynamics
    End Function

    Private Function updateEcopathEcosimParameters(iModel As Integer) As Boolean

        'Update the Ecopath and Ecosim parameters from the data read into memory by Me.readEcopathEcosimParameters()
        Me.updateParametersFromMemory(iModel)
        Survivability.ConfigCoreWithSurvivabilities(iModel)

        ' Me.updateDietRandom()
        'Return Me.readDietMatrix(iTrial)
        ' Return True
        'Diet matrix parameters are stored in file by iTrial
        'Read the file and update the dietmatrix parameters
        Return Me.updateDietMatrixFromCSVFile(iModel)

    End Function

    ''' <summary>
    ''' Populte the Ecopath and Ecosim parameter with values read into memory by Me.readEcopathEcosimParameters()
    ''' </summary>
    ''' <param name="itrial"></param>
    Private Sub updateParametersFromMemory(itrial As Integer)

        For igrp = 1 To m_core.nLivingGroups
            Debug.Assert(Not B(itrial - 1, igrp - 1) = cCore.NULL_VALUE, "Oppss something is very wrong with the parameters read from file.")
            If Not B(itrial - 1, igrp - 1) = cCore.NULL_VALUE Then
                Me._pathdata.B(igrp) = CSng(B(itrial - 1, igrp - 1))
                Me._pathdata.PB(igrp) = CSng(PB(itrial - 1, igrp - 1))
                Me._pathdata.QB(igrp) = CSng(QB(itrial - 1, igrp - 1))
                Me._pathdata.EE(igrp) = CSng(EE(itrial - 1, igrp - 1))
                Me._pathdata.BA(igrp) = CSng(BA(itrial - 1, igrp - 1))
                If Not m_core.EcoPathGroupInputs(igrp).IsProducer Then
                    Me._simdata.QmQo(igrp) = CSng(DenDepCatchability(itrial - 1, igrp - 1))
                    Me._simdata.FtimeAdjust(igrp) = CSng(FeedingTimeAdjustRate(itrial - 1, igrp - 1))
                    Me._simdata.FtimeMax(igrp) = CSng(MaxRelFeedingTime(itrial - 1, igrp - 1))
                    Me._simdata.MoPred(igrp) = CSng(OtherMortFeedingTime(itrial - 1, igrp - 1))
                    Me._simdata.RiskTime(igrp) = CSng(PredEffectFeedingTime(itrial - 1, igrp - 1))
                    Me._simdata.CmCo(igrp) = CSng(QBMaxxQBio(itrial - 1, igrp - 1))
                    Me._simdata.SwitchPower(igrp) = CSng(SwitchingPower(itrial - 1, igrp - 1))
                    If m_core.EcoPathGroupInputs(igrp).IsProducer Then Stop
                End If
            End If 'Not B(iTrial - 1, igrp - 1) = cCore.NULL_VALUE 
        Next igrp


        'Vulnerabilities
        For iPrey As Integer = 1 To m_core.nGroups

            For iPred As Integer = 1 To m_core.nLivingGroups
                'For iPred As Integer = 1 To Vulnerabilities.GetLength(2)
                If Not Vulnerabilities(itrial - 1, iPred - 1, iPrey - 1) = cCore.NULL_VALUE Then
                    Me._simdata.VulMult(iPrey, iPred) = CSng(Vulnerabilities(itrial - 1, iPred - 1, iPrey - 1))
                Else
                    Me._simdata.VulMult(iPrey, iPred) = cCore.NULL_VALUE
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' Initialize the biomass and fleet results files
    ''' </summary>
    ''' <param name="msgReport"></param>
    ''' <param name="strmGroup"></param>
    ''' <param name="strmFleet"></param>
    Private Sub initResultFiles(ByVal msgReport As cMessage, ByRef strmGroup As StreamWriter, ByRef strmFleet As StreamWriter)

        'Output the final results
        strmGroup = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, "Results.csv"), False)
        If Me.m_core.SaveWithFileHeader Then strmGroup.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        strmGroup.WriteLine("Iteration,Strategy,GroupNumber,GroupName,ResultName,Value")
        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, "Results.csv"), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

        'Create the csv writer for writing out individual fleets catches of each group
        strmFleet = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, "Fleet.csv"), False)
        If Me.m_core.SaveWithFileHeader Then strmFleet.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        strmFleet.WriteLine("Iteration,Strategy,FleetNumber,FleetName,GroupNumber,GroupName,Landings,DiscardSurvivals,DiscardMortalities")
        'strmFleet.WriteLine("Iteration,Strategy,FleetNumber,FleetName,GroupNumber,GroupName,Value")
        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, "Fleet.csv"), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

        ''Create the csv writer for writing out individual fleet efforts
        'strmFleetEffort = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, "FleetEfforts.csv"), False)
        'If Me.m_core.SaveWithFileHeader Then strmFleetEffort.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        'strmFleetEffort.WriteLine("Iteration,Strategy,FleetNumber,FleetName,Effort")
        'msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, "FleetEfforts.csv"), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

    End Sub

    'Private Function initTrialTrajectoryFile(msgReport As cMessage, iModel As Integer) As StreamWriter

    '    Dim strm As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ResultsTrajectories, "Model" & iModel & ".csv"), False)
    '    If Me.m_core.SaveWithFileHeader Then strm.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
    '    strm.Write("GroupNumber,Group,Strategy")
    '    msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, "Model" & iModel & ".csv"), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

    '    For iTime As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
    '        strm.Write("," & cStringUtils.FormatNumber(iTime))
    '    Next
    '    strm.WriteLine()
    '    Return strm

    'End Function

    Private Function initBadDynamicsFile(msgReport As cMessage) As StreamWriter

        Dim strm As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, "BadDynamicsTrajectories.csv"), False)
        If Me.m_core.SaveWithFileHeader Then strm.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        strm.Write("iModel,StrategyName,GroupName,MaxB,UpperLim,MaxB/UpperLim,MaxB>UpperLim,MinB,LowerLim, MinB<=LowerLim")
        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, "BadDynamicsTrajectories.csv"), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

        Return strm

    End Function

    Private Function initTrajectoryEffortFiles(msgReport As cMessage) As StreamWriter

        Dim strm As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, "EffortTrajectories.csv"), False)
        If Me.m_core.SaveWithFileHeader Then strm.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        strm.Write("Model,Strategy,FleetNumber,FleetName")
        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, "EffortTrajectories.csv"), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

        For iTime As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
            strm.Write("," & cStringUtils.FormatNumber(iTime))
        Next
        Return strm

    End Function


    Private Sub initTrajectoryHCRFQuotaByFleetGroupFiles(ByVal msgReport As cMessage, ByVal HCRF_Targ As List(Of StreamWriter), ByVal HCRF_Cons As List(Of StreamWriter),
                                               ByVal HCR_Quota_Targ(,) As StreamWriter, ByVal HCR_Quota_Cons(,) As StreamWriter)
        Dim strFile As String

        For igrp As Integer = 1 To m_core.nGroups

            strFile = cFileUtils.ToValidFileName(m_core.EcoPathGroupInputs(igrp).Name & "_GroupNo" & igrp & ".csv", False)
            Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.HCRF_Targ, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            HCRF_Targ.Add(writer)
            If Me.m_core.SaveWithFileHeader Then HCRF_Targ(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            HCRF_Targ(igrp - 1).Write("GroupName, ModelID, StrategyName, HCRType")
            For iTime As Integer = 1 To NYearsProject
                HCRF_Targ(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            HCRF_Targ(igrp - 1).WriteLine()

            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.HCRF_Cons, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            HCRF_Cons.Add(writer)
            If Me.m_core.SaveWithFileHeader Then HCRF_Cons(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            HCRF_Cons(igrp - 1).Write("GroupName, ModelID, StrategyName, HCRType")
            For iTime As Integer = 1 To NYearsProject
                HCRF_Cons(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            HCRF_Cons(igrp - 1).WriteLine()


            'writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.HCRQuota_Targ, strFile))
            'msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            'Debug.Assert(writer IsNot Nothing)

            For iFleet = 1 To m_core.nFleets

                strFile = cFileUtils.ToValidFileName(m_core.EcopathFleetInputs(iFleet).Name & "_FleetNo" & iFleet & "_" & m_core.EcoPathGroupInputs(igrp).Name & "_GroupNo" & igrp & ".csv", False)
                writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.HCRQuota_Targ, strFile))
                msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

                Debug.Assert(writer IsNot Nothing)

                HCR_Quota_Targ(iFleet - 1, igrp - 1) = writer
                If Me.m_core.SaveWithFileHeader Then HCR_Quota_Targ(iFleet - 1, igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                HCR_Quota_Targ(iFleet - 1, igrp - 1).Write("FleetName, GroupName, ModelID, StrategyName, HCRType")
                For iTime As Integer = 1 To NYearsProject
                    HCR_Quota_Targ(iFleet - 1, igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
                Next
                HCR_Quota_Targ(iFleet - 1, igrp - 1).WriteLine()


                writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.HCRQuota_Cons, strFile))
                msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

                Debug.Assert(writer IsNot Nothing)

                HCR_Quota_Cons(iFleet - 1, igrp - 1) = writer
                If Me.m_core.SaveWithFileHeader Then HCR_Quota_Cons(iFleet - 1, igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                HCR_Quota_Cons(iFleet - 1, igrp - 1).Write("FleetName, GroupName, ModelID, StrategyName, HCRType")
                For iTime As Integer = 1 To NYearsProject
                    HCR_Quota_Cons(iFleet - 1, igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
                Next
                HCR_Quota_Cons(iFleet - 1, igrp - 1).WriteLine()

            Next

        Next

    End Sub

    Private Sub initHighestValueGroupFile(ByVal msgReport As cMessage, ByRef HighestValueGroup As StreamWriter)

        Dim strFile As String = cFileUtils.ToValidFileName("HighestValueGroups.csv", False)
        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, strFile))
        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

        Debug.Assert(writer IsNot Nothing)

        HighestValueGroup = writer

        If Me.m_core.SaveWithFileHeader Then HighestValueGroup.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim, OriginalNTimesteps \ 12 + 1))
        HighestValueGroup.Write("FleetName, ModelID, StrategyNo")

        For iYear As Integer = 1 To NYearsProject
            HighestValueGroup.Write("," & cStringUtils.FormatNumber(iYear))
        Next

        HighestValueGroup.WriteLine()

    End Sub

    Private Sub initChokeGroupFile(ByVal msgReport As cMessage, ByRef ChokeGroup As StreamWriter)

        Dim strFile As String = cFileUtils.ToValidFileName("ChokeGroups.csv", False)
        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, strFile))
        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

        Debug.Assert(writer IsNot Nothing)

        ChokeGroup = writer

        If Me.m_core.SaveWithFileHeader Then ChokeGroup.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim, OriginalNTimesteps \ 12 + 1))
        ChokeGroup.Write("FleetName, ModelID, StrategyName")

        For iTimeStep As Integer = 1 To NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
            ChokeGroup.Write("," & cStringUtils.FormatNumber(iTimeStep))
        Next

        ChokeGroup.WriteLine()

    End Sub

    Private Sub initTrajectoryFleetGroupCatchTrajectoryFiles(ByVal msgReport As cMessage, ByVal LandingsFleetGroupTrajectories(,) As StreamWriter,
                                               ByVal DiscardsFleetGroupTrajectories(,) As StreamWriter, ByVal CatchFleetGroupTrajectories(,) As StreamWriter,
                                               ByVal ValueFleetGroupTrajectories(,) As StreamWriter)
        Dim strFile As String

        For iGrp As Integer = 0 To m_core.nGroups
            For iFleet As Integer = 1 To m_core.nFleets
                If iGrp = 0 Then
                    strFile = cFileUtils.ToValidFileName("AllGroups_FleetNo" & iFleet & ".csv", False)
                Else
                    strFile = cFileUtils.ToValidFileName(m_core.EcoPathGroupInputs(iGrp).Name & "_GroupNo" & iGrp & "_FleetNo" & iFleet & ".csv", False)
                End If

                Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.LandingsTrajectories, strFile))
                msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

                Debug.Assert(writer IsNot Nothing)

                LandingsFleetGroupTrajectories(iFleet - 1, iGrp) = writer

                'Setup the HCR F Targ file for igrp
                If Me.m_core.SaveWithFileHeader Then LandingsFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                LandingsFleetGroupTrajectories(iFleet - 1, iGrp).Write("GroupName, FleetName, ModelID, StrategyName, CatchType")
                For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                    LandingsFleetGroupTrajectories(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(iTime))
                Next
                LandingsFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine()


                writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DiscardsTrajectories, strFile))
                msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

                Debug.Assert(writer IsNot Nothing)

                DiscardsFleetGroupTrajectories(iFleet - 1, iGrp) = writer

                'Setup the HCR F Targ file for igrp
                If Me.m_core.SaveWithFileHeader Then DiscardsFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                DiscardsFleetGroupTrajectories(iFleet - 1, iGrp).Write("GroupName, FleetName, ModelID, StrategyName, CatchType")
                For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                    DiscardsFleetGroupTrajectories(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(iTime))
                Next
                DiscardsFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine()


                writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.CatchTrajectories, strFile))
                msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

                Debug.Assert(writer IsNot Nothing)

                CatchFleetGroupTrajectories(iFleet - 1, iGrp) = writer

                'Setup the HCR F Targ file for igrp
                If Me.m_core.SaveWithFileHeader Then CatchFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                CatchFleetGroupTrajectories(iFleet - 1, iGrp).Write("GroupName, FleetName, ModelID, StrategyName, CatchType")
                For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                    CatchFleetGroupTrajectories(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(iTime))
                Next
                CatchFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine()



                writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ValueTrajectories, strFile))
                msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

                Debug.Assert(writer IsNot Nothing)

                ValueFleetGroupTrajectories(iFleet - 1, iGrp) = writer

                'Setup the HCR F Targ file for igrp
                If Me.m_core.SaveWithFileHeader Then ValueFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                ValueFleetGroupTrajectories(iFleet - 1, iGrp).Write("GroupName, FleetName, ModelID, StrategyName, ResultType")
                For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                    ValueFleetGroupTrajectories(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(iTime))
                Next
                ValueFleetGroupTrajectories(iFleet - 1, iGrp).WriteLine()

            Next
        Next

    End Sub




    Private Sub initTrajectoryCatchTrajectoryFiles(ByVal msgReport As cMessage, ByVal LandingsTrajectories As List(Of StreamWriter),
                                                   ByVal DiscardsTrajectories As List(Of StreamWriter), ByVal CatchTrajectories As List(Of StreamWriter))

        For igrp As Integer = 1 To m_core.nGroups

            Dim strFile As String = cFileUtils.ToValidFileName(m_core.EcoPathGroupInputs(igrp).Name & "_GroupNo" & igrp & ".csv", False)

            'Init Landings CSV file
            Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.LandingsTrajectories, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            LandingsTrajectories.Add(writer)
            If Me.m_core.SaveWithFileHeader Then LandingsTrajectories(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            LandingsTrajectories(igrp - 1).Write("GroupName, ModelID, StrategyName, CatchType")
            For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                LandingsTrajectories(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            LandingsTrajectories(igrp - 1).WriteLine()


            'Init Discard CSV file
            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.DiscardsTrajectories, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            DiscardsTrajectories.Add(writer)
            If Me.m_core.SaveWithFileHeader Then DiscardsTrajectories(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            DiscardsTrajectories(igrp - 1).Write("GroupName, ModelID, StrategyName, CatchType")
            For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                DiscardsTrajectories(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            DiscardsTrajectories(igrp - 1).WriteLine()


            'Init Catch CSV file
            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.CatchTrajectories, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            CatchTrajectories.Add(writer)
            If Me.m_core.SaveWithFileHeader Then CatchTrajectories(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            CatchTrajectories(igrp - 1).Write("GroupName, ModelID, StrategyName, CatchType")
            For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                CatchTrajectories(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            CatchTrajectories(igrp - 1).WriteLine()

        Next

    End Sub

    Private Sub initTrajectoryRealisedFFiles(ByVal msgReport As cMessage, ByVal RealisedF As List(Of StreamWriter),
                                             ByVal RealisedLandedF As List(Of StreamWriter), ByVal RealisedDiscardF As List(Of StreamWriter))
        Dim strFile As String
        Dim writer As StreamWriter

        For igrp As Integer = 1 To m_core.nGroups

            strFile = cFileUtils.ToValidFileName(m_core.EcoPathGroupInputs(igrp).Name & "_GroupNo" & igrp & ".csv", False)
            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.RealisedF, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            RealisedF.Add(writer)
            If Me.m_core.SaveWithFileHeader Then RealisedF(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            RealisedF(igrp - 1).Write("GroupName, ModelID, StrategyName, RealisedFType")
            For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                RealisedF(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            RealisedF(igrp - 1).WriteLine()


            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.RealisedLandedF, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            RealisedLandedF.Add(writer)
            If Me.m_core.SaveWithFileHeader Then RealisedLandedF(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            RealisedLandedF(igrp - 1).Write("GroupName, ModelID, StrategyName, RealisedFType")
            For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                RealisedLandedF(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            RealisedLandedF(igrp - 1).WriteLine()


            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.RealisedDiscardF, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            RealisedDiscardF.Add(writer)
            If Me.m_core.SaveWithFileHeader Then RealisedDiscardF(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            RealisedDiscardF(igrp - 1).Write("GroupName, ModelID, StrategyName, RealisedFType")
            For iTime As Integer = 1 To NYearsProject * EcosimData.NumStepsPerYear
                RealisedDiscardF(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            RealisedDiscardF(igrp - 1).WriteLine()

        Next

    End Sub

    'Private Sub initTrajectoryByGroupFiles(ByVal msgReport As cMessage, ByVal TrajectoryList As List(Of StreamWriter))

    '    For igrp As Integer = 1 To m_core.nLivingGroups

    '        Dim strFile As String = cFileUtils.ToValidFileName(m_core.EcoPathGroupInputs(igrp).Name & "_GroupNo" & igrp & ".csv", False)
    '        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ResultsTraj2, strFile))
    '        msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

    '        Debug.Assert(writer IsNot Nothing)

    '        TrajectoryList.Add(writer)
    '        If Me.m_core.SaveWithFileHeader Then TrajectoryList(igrp - 1).WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
    '        TrajectoryList(igrp - 1).Write("Model,Strategy")
    '        For iTime As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
    '            TrajectoryList(igrp - 1).Write("," & cStringUtils.FormatNumber(iTime))
    '        Next
    '        TrajectoryList(igrp - 1).WriteLine()

    '    Next

    'End Sub

    ''' <summary>
    ''' Calculate the total catch including discards that survived
    ''' </summary>
    Private Function Calc_Total_Catch_If_Quota_Didnt_Affect_It(BiomassAtT As Single, FleetIndex As Integer, GroupIndex As Integer, TimeStep As Integer) As Single
        Return Me.calcCatchRate(BiomassAtT, FleetIndex, GroupIndex, TimeStep) * BiomassAtT
    End Function

    ''' <summary>
    ''' Calculate the total catch rate at the current effort, including discards that survived and the density dependant catchability.
    ''' </summary>
    ''' <remarks>
    ''' This is not really F because it includes survivals
    ''' </remarks>
    Private Function calcCatchRate(BiomassAtT As Single, FleetIndex As Integer, GroupIndex As Integer, TimeStep As Integer) As Single
        Dim DenDep As Single = m_ecosim.EcosimData.QmQo(GroupIndex) / (1 + (m_ecosim.EcosimData.QmQo(GroupIndex) - 1) * BiomassAtT / m_ecosim.EcosimData.StartBiomass(GroupIndex))
        '[catch rate] * [effort] * [density dependant catchability]
        Return CSng(Me._simdata.relQ(FleetIndex, GroupIndex) * _simdata.FishRateGear(FleetIndex, TimeStep) * DenDep)
    End Function



    'Private Sub initBaseCatchRate()
    '    Me.BaseCatchRate = New Single(Me._pathdata.NumFleet, Me._pathdata.NumGroups) {}

    '    For igrp As Integer = 1 To Me._pathdata.NumGroups
    '        For iflt As Integer = 1 To Me._pathdata.NumFleet
    '            'Ecopath base catch rate including catch that were discarded and survived
    '            '[Total Catch] / [Ecopath Biomass]
    '            Me.BaseCatchRate(iflt, igrp) = CSng((Me._pathdata.Landing(iflt, igrp) + Me._pathdata.Discard(iflt, igrp)) / Me._pathdata.B(igrp))
    '        Next
    '    Next

    'End Sub

    ''' <summary>
    ''' Load all the parameters from the [parameter name]_out.csv files
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub readEcopathEcosimParameters()
        'Load all the parameters from the <parameter name>_out.csv files
        B = ExtractParamsCSV("B")
        PB = ExtractParamsCSV("PB")
        QB = ExtractParamsCSV("QB")
        EE = ExtractParamsCSV("EE")
        BA = ExtractParamsCSV("BA")
        DenDepCatchability = ExtractParamsCSV("DenDepCatchability")
        FeedingTimeAdjustRate = ExtractParamsCSV("FeedingTimeAdjustRate")
        MaxRelFeedingTime = ExtractParamsCSV("MaxRelFeedingTime")
        OtherMortFeedingTime = ExtractParamsCSV("OtherMortFeedingTime")
        PredEffectFeedingTime = ExtractParamsCSV("PredEffectFeedingTime")
        QBMaxxQBio = ExtractParamsCSV("QBMaxxQBio")
        SwitchingPower = ExtractParamsCSV("SwitchingPower")
        Vulnerabilities = ExtractVulnerabilitiesCSV()
    End Sub

    Private Sub GenerateEcopathParamaters()

        Me.SaveOriginalParameters()
        Try
            Me.GenerateInputStructure()
        Catch ex As Exception
            ' Kaboom!
        End Try
        ' Re-assess configuration when next needed
        Me.InvalidateConfigurationState()
        Me.RestoreOriginalState()

    End Sub

    Private Sub GenerateInputStructure()

        Dim nLiving As Integer = m_core.nLivingGroups
        Dim nGroups As Integer = m_core.nGroups
        Dim MonteCarlo As cMonteCarloManager = m_core.EcosimMonteCarlo
        Dim nTrials As Integer = Me.NModels
        Dim b(nTrials, nGroups) As Single
        Dim ba(nTrials, nLiving) As Single
        Dim pb(nTrials, nLiving) As Single
        Dim qb(nTrials, nLiving) As Single
        Dim ee(nTrials, nLiving) As Single
        Dim TimeFindingBalanced As New Stopwatch
        'Dim nPPers As Integer 'number of primary producers
        'Dim nLivingMinusPPers As Integer 'number of living groups minus primary producers
        Const PQThreshold As Double = 0.5
        Const RespirThreshold As Double = 0
        Dim isbalanced As Boolean
        Dim iNumFound As Integer = 0

        'I am just altering the tolerance so that it can run faster; this needs deleting later
        'MessageBox.Show("the default tolerance = " & MonteCarlo.EcopathEETolerance)
        MonteCarlo.EcopathEETolerance = Me.MassBalanceTol
        'MonteCarlo.EcopathEETolerance = 0.05 'comment out and uncomment above line!!! this line just a test
        'Forces the same sequence of random numbers for each run. Used only for debugging runs
        'MonteCarlo.InitRandomSequence(666)

        'cMonteCarloManager.selectNewEcopathParameters() will alter the Ecopath Input parameters
        'We need to save the original state of Ecopath so it can be restored when we are done
        Me.SaveOriginalState()

        'Calculate how many living groups that aren't primary producers
        'For i = 1 To mCore.nGroups
        '    If mCore.EcoPathGroupInputs(i).IsProducer Then nPPers += 1
        'Next i
        'nLivingMinusPPers = mCore.nLivingGroups - nPPers
        Try

            'Init some of the Monte Carlo parameters
            If Me.InitMonteCarloParameters() Then
                'Succeeded in intitializing Monte Carlo Parameters
                Dim iTrial As Integer = 1
                Dim bExpired As Boolean = False
                Dim sw As New Stopwatch()
                Dim lTimeout As Long = CLng(60 * 60 * 1000 * Me.NMaxTime)

                sw.Start()

                While (iTrial <= Me.NModels) And Not bExpired

                    'Set the Ecopath parameters using the Monte Carlo input parameters set above
                    TimeFindingBalanced.Start()
                    Dim i As Integer = 1
                    Dim bFound As Boolean = False

                    While (i <= Me.NMaxAttempts) And (Not bExpired) And (Not bFound)

                        isbalanced = True

                        'Has the user tried to stop the run
                        If Me.m_StopRun Then
                            'Yep
                            'HACK use the bExpired flag to stop the run
                            bExpired = True
                        End If

                        ' Provide occassional UI feedback
                        If (i = 1) Or ((i Mod 50) = 0) Then
                            cApplicationStatusNotifier.UpdateProgress(Me.Core, String.Format(My.Resources.STATUS_TRIAL_PROGRESS, My.Resources.CAPTION, iTrial, i), -1)
                        End If

                        'Write code here that generates a whole set of diet parameters to be used in combination with new ecopath parameters
                        'to be tested for the mass-balance criteria
                        Me.SampleDietMatrix(Me.m_diets.Interacts, Me.m_diets.InteractsImports, Me.m_diets.MeanProportions,
                                            Me.m_diets.MeanProportionsImports, Me.m_diets.DietPropMultipliers)

                        'jb 23-Sept-2014 SampleDietMatrix(...) will truncate values less than a hardwired min (1.0E-6 inside SampleDietMatrix )
                        'If that happens we need to re-normalize the diet matrix
                        Me.NormalizeDiet(Me._pathdata.DCInput)
                        'Me.dumpDietMatrix()

                        'Console.WriteLine("Iteration = " & i)
                        If MonteCarlo.SelectNewEcopathParameters(1) Then

                            For iGrp = 1 To m_core.nGroups
                                If m_core.EcoPathGroupInputs(iGrp).IsLiving Then
                                    If _pathdata.GE(iGrp) > PQThreshold Then Console.WriteLine("Group " & iGrp & " failed PQ<0.5 test")
                                    If _pathdata.Resp(iGrp) < RespirThreshold Then Console.WriteLine("Group" & iGrp & " failing the respiration>0 test")
                                    If _pathdata.GE(iGrp) > PQThreshold Or _pathdata.Resp(iGrp) < RespirThreshold Then
                                        isbalanced = False
                                    End If
                                End If
                            Next

                            If isbalanced = True Then

                                'Output the diet matrix parameters to csv
                                Dim csv_dietout As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, "DietMatrixTrial" & iTrial & ".csv"), False)
                                Try
                                    For iPrey = 0 To nGroups
                                        For iPred = 1 To m_core.nLivingGroups
                                            If iPred > 1 Then csv_dietout.Write(", ")
                                            csv_dietout.Write(cStringUtils.FormatNumber(Me.m_ecopath.EcopathData.DC(iPred, iPrey)))
                                        Next
                                        csv_dietout.WriteLine()
                                    Next
                                Catch ex As Exception
                                    ' ToDo: respond to error
                                End Try
                                cMSEUtils.ReleaseWriter(csv_dietout)

                                ' JS 30Sep13: greatly simplified :)
                                WriteEcopathParms("b_out.csv", Me.m_ecopath.EcopathData.B)
                                WriteEcopathParms("ba_out.csv", Me.m_ecopath.EcopathData.BA)
                                WriteEcopathParms("pb_out.csv", Me.m_ecopath.EcopathData.PB)
                                WriteEcopathParms("qb_out.csv", Me.m_ecopath.EcopathData.QB)
                                WriteEcopathParms("ee_out.csv", Me.m_ecopath.EcopathData.EE)
                                ''This runs Ecosim without core support
                                'If Me.RunEcosim() Then
                                '    'dumps out some Ecosim results
                                '    Me.getEcosimResults()
                                'End If 'RunEcosim

                                'Me.InformUser(String.Format(My.Resources.STATUS_FOUND_MODEL, My.Resources.CAPTION, i), eMessageImportance.Information)
                                cLog.Write(String.Format(My.Resources.STATUS_FOUND_MODEL, My.Resources.CAPTION, i, sw.Elapsed.ToString()))

                                iNumFound += 1
                                bFound = True

                            End If
                        Else
                            System.Console.WriteLine("Failed to find balanced Ecopath model")
                        End If ' MonteCarlo.selectNewEcopathParameters()

                        i += 1

                        If (sw.ElapsedMilliseconds > lTimeout) Then
                            cLog.Write(String.Format("Cefas MSE time-out expired at iteration {0}, {1}", i, sw.Elapsed.ToString()))
                            bExpired = True
                        End If

                    End While

                    'Console.WriteLine("Number of seconds to run iteration:  " & (TimeFindingBalanced.ElapsedMilliseconds / 1000).ToString)
                    TimeFindingBalanced.Reset()

                    iTrial += 1

                End While

                If bExpired And Not Me.m_StopRun Then
                    Me.InformUser("MSE expired after " & sw.Elapsed.ToString(), eMessageImportance.Information)
                End If

            End If 'Me.InitMonteCarloParameters()

            'Save the results to a .csv

        Catch ex As Exception

        End Try

        Me.RestoreOriginalState()

        ' Provide summary
        If iNumFound = 0 Then
            Me.InformUser(String.Format(My.Resources.STATUS_FINDMODELS_SUMMARY, My.Resources.CAPTION, iNumFound, nTrials), eMessageImportance.Warning)
        Else
            Me.InformUser(String.Format(My.Resources.STATUS_FINDMODELS_SUMMARY, My.Resources.CAPTION, iNumFound, nTrials), eMessageImportance.Information)
        End If

    End Sub

    Private Function getEcosimResults() As Boolean
        Try
            'Because we ran Ecosim directly from cEcosimModel.Run() instead of via the core cCore.RunEcosim()
            'the Core output objects cCore.EcoSimGroupOutputs() will not be populated
            'Instead get the Ecosim results directly from the underlying arrays
            Dim sumb() As Single
            ReDim sumb(m_core.nLivingGroups)
            For igrp As Integer = 1 To m_core.nLivingGroups
                'sum biomass over all the Ecosim timesteps
                For itime As Integer = 1 To m_core.nEcosimTimeSteps
                    'see cEcosimModel.PopulateResults() for how ResultsOverTime(var,group,time) are stored
                    sumb(igrp) += Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, itime)
                Next itime

                System.Console.WriteLine("Average Biomass for " & Me.m_ecopath.EcopathData.GroupName(igrp) & " = " & (sumb(igrp) / m_core.nEcosimTimeSteps).ToString)

            Next igrp

        Catch ex As Exception

        End Try

        Return Nothing

    End Function



    Private Function DetermineZeroEffortFleets(ByRef FTargCons(,) As Double) As Integer()
        Dim ZeroEffortFleets As New List(Of Integer)

        For iGrp = 1 To m_core.nGroups
            If FTargCons(iGrp - 1, HCRType.Conservation) = 0 Then
                For iFleet = 1 To m_core.nFleets
                    If m_core.EcopathFleetInputs(iFleet).Landings(iGrp) + m_core.EcopathFleetInputs(iFleet).Discards(iGrp) > 0 And
                        Not ZeroEffortFleets.Contains(iFleet) Then
                        ZeroEffortFleets.Add(iFleet)
                    End If
                Next
            End If
        Next
        Return ZeroEffortFleets.ToArray

    End Function

    Private Function DetermineQuotas(BiomassAtTimestep() As Single, iYearProjecting As Integer, ByVal iTimeStep As Integer) As Single(,)

        Dim TargConsQuota(m_core.nGroups - 1, 1) As Single
        Dim TempTargConsQuota As Single
        Dim FfromHCR As Single

        'Calc the maximum decreases in the biomass


        'Initialise FTargetandConservation
        For i = 1 To m_core.nGroups
            TargConsQuota(i - 1, 0) = cEffortLimits.NoHCR_F
            TargConsQuota(i - 1, 1) = cEffortLimits.NoHCR_F
        Next

        For Each iHCRGroup As HCR_Group In Me.currentStrategy
            ' Determines the F for each group
            If TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) = cEffortLimits.NoHCR_F And iHCRGroup.Targ_Or_Cons = HCRType.Target Then
                TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) = iHCRGroup.CalcF(BiomassAtTimestep, iYearProjecting, iTimeStep) * BiomassAtTimestep(iHCRGroup.GroupF.Index)
            ElseIf iHCRGroup.Targ_Or_Cons = HCRType.Conservation Then
                If TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) = cEffortLimits.NoHCR_F Then
                    FfromHCR = iHCRGroup.CalcF(BiomassAtTimestep, iYearProjecting, iTimeStep)
                    TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) = FfromHCR * BiomassAtTimestep(iHCRGroup.GroupF.Index)
                Else
                    FfromHCR = iHCRGroup.CalcF(BiomassAtTimestep, iYearProjecting, iTimeStep)
                    TempTargConsQuota = FfromHCR * BiomassAtTimestep(iHCRGroup.GroupF.Index)
                    If TempTargConsQuota < TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) Then
                        TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) = TempTargConsQuota
                    End If
                End If
            ElseIf TargConsQuota(iHCRGroup.GroupF.Index - 1, iHCRGroup.Targ_Or_Cons) <> cEffortLimits.NoHCR_F And iHCRGroup.Targ_Or_Cons = HCRType.Target Then
                Me.InformUser(String.Format(My.Resources.ERROR_HARVESTRUILE_DUPLICATE_F, iHCRGroup.GroupF.Name), eMessageImportance.Warning)
            End If
        Next

        'Output as F's to data table
        For iGrp = 1 To m_core.nGroups
            If TargConsQuota(iGrp - 1, HCRType.Target) <> cEffortLimits.NoHCR_F Then
                TargetFs(iGrp - 1, iYearProjecting - 1) = TargConsQuota(iGrp - 1, HCRType.Target) / BiomassAtTimestep(iGrp)
                For iFleet = 1 To m_core.nFleets
                    If (m_ecopath.EcopathData.Landing(iFleet, iGrp)) > 0 Then
                        TargetQuotas(iFleet - 1, iGrp - 1, iYearProjecting - 1) = TargConsQuota(iGrp - 1, HCRType.Target) * m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, True).mShare
                    End If
                Next
            End If
            If TargConsQuota(iGrp - 1, HCRType.Conservation) <> cEffortLimits.NoHCR_F Then
                ConservationFs(iGrp - 1, iYearProjecting - 1) = TargConsQuota(iGrp - 1, HCRType.Conservation) / BiomassAtTimestep(iGrp)
                For iFleet = 1 To m_core.nFleets
                    If m_ecopath.EcopathData.Landing(iFleet, iGrp) > 0 Then
                        ConservationQuotas(iFleet - 1, iGrp - 1, iYearProjecting - 1) = TargConsQuota(iGrp - 1, HCRType.Conservation) * m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, True).mShare
                    End If
                Next
            End If
        Next

        Return TargConsQuota

    End Function

    'This is just a diagnostics routine that outputs to console the Biomass for each living group at a particular iteration
    Private Sub dumpEcopathParameters(ByVal iteration As Integer)
        Dim nliving As Integer = Me.m_core.nLivingGroups
        Dim MonteCarlo As cMonteCarloManager = Me.m_core.EcosimMonteCarlo

        System.Console.WriteLine("Iteration = " & iteration.ToString)
        For igrp = 1 To nliving
            Dim mcGrp As cMonteCarloGroup = MonteCarlo.Groups(igrp)
            System.Console.Write(mcGrp.Name & " = " & mcGrp.B & " , ")
            'Other parameters...  mcGrp.PB
        Next igrp
        System.Console.WriteLine()

    End Sub

    Public Function RunEcosim() As Boolean

        Try
            'increase the number of years for the projection
            ' mCore.EcoSimModelParameters.NumberYears = CInt(OriginalNTimesteps / _ecosim.EcosimData.NumStepsPerYear + NYearsProject)

            'make sure Ecosim computes the output data
            Me.m_ecosim.EcosimData.bTimestepOutput = True

            'No timestep call back
            Me.m_ecosim.TimeStepDelegate = Nothing

            'Run on the same thread 
            'this means Me._ecosim.Run() will block until Ecosim has finished running
            Me.m_ecosim.EcosimData.bMultiThreaded = False

            'Run Ecosim without Core support 
            'This means Core Input/ouput objects will not be populate 
            'So you can not use cCore.EcoSimGroupOutputs() to retrieve the results
            Me.m_ecosim.Init(True)
            Return Me.m_ecosim.Run()

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".RunEcosim() Exception: " & ex.Message)
        End Try

        Return False

    End Function

    Private Sub NormalizeDiet(ByRef DietMatrix(,) As Single)
        Dim dietsum As Single
        Dim tol As Single = 0.001
        Dim bwarning As Boolean = False

        For iPred = 1 To Me._pathdata.NumLiving
            bwarning = False
            If Me._pathdata.PP(iPred) < 1 Then
                dietsum = 0
                For iPrey = 0 To Me._pathdata.NumGroups
                    dietsum = dietsum + DietMatrix(iPred, iPrey)
                Next
                If dietsum <> 0 And Math.Abs(dietsum - 1) > tol Then
                    bwarning = True
                    For iPrey = 0 To Me._pathdata.NumGroups
                        DietMatrix(iPred, iPrey) = DietMatrix(iPred, iPrey) / dietsum
                    Next
                    'm_Data.DietsModified = True
                End If
            End If
        Next
        If bwarning Then
            System.Console.WriteLine("WARNING MSE Normalized Diet after sampling.")
        End If

    End Sub

    Public Sub CreateRCode()
        Dim writer As StreamWriter = New StreamWriter("C:\Users\Mark\Desktop\vbcreatedRcode.txt", False)
        Dim bSuccess As Boolean = False
        Dim firstpreyfound As Boolean
        Dim tempname As String

        For iPred = 1 To m_core.nLivingGroups
            tempname = m_core.EcoPathGroupInputs(iPred).Name
            tempname = tempname.Replace(" ", "")
            writer.Write("dietmeans[[" & iPred & "]] = c(")
            firstpreyfound = False
            For iPrey = 1 To m_core.nGroups
                If m_core.EcoPathGroupInputs(iPred).DietComp(iPrey) <> 0 Then
                    If firstpreyfound = False Then
                        writer.Write(m_core.EcoPathGroupInputs(iPred).DietComp(iPrey))
                        firstpreyfound = True
                    Else
                        writer.Write(", " & m_core.EcoPathGroupInputs(iPred).DietComp(iPrey))
                    End If
                End If
            Next
            writer.WriteLine(")")

            writer.Write("preynames[[" & iPred & "]] = c(")
            firstpreyfound = False
            For iPrey = 1 To m_core.nGroups
                If m_core.EcoPathGroupInputs(iPred).DietComp(iPrey) <> 0 Then
                    If firstpreyfound = False Then
                        writer.Write("""" & m_core.EcoPathGroupInputs(iPrey).Name & """")
                        firstpreyfound = True
                    Else
                        writer.Write(", """ & m_core.EcoPathGroupInputs(iPrey).Name & """")
                    End If
                End If
            Next
            writer.WriteLine(")")
        Next

        writer.Write("prednames = c(")
        For iPred = 1 To m_core.nLivingGroups
            If iPred = 1 Then
                writer.Write("""" & m_core.EcoPathGroupInputs(iPred).Name & """")
            Else
                writer.Write(", """ & m_core.EcoPathGroupInputs(iPred).Name & """")
            End If
        Next
        writer.WriteLine(")")

        cMSEUtils.ReleaseWriter(writer)

    End Sub

    Private Sub CalCatchDiscards(ByVal BiomassAtTimestep() As Single, ByVal iTime As Integer)

        Debug.Assert(iTime > (Me.OriginalNTimesteps + (Me.NYearsProject - 1) * EcosimData.NumStepsPerYear) And iTime <= Me.OriginalNTimesteps + Me.NYearsProject * EcosimData.NumStepsPerYear,
                     "Time step for Catch and Discards averaging out of bounds")

        sumCatchDiscards(BiomassAtTimestep, iTime)

        'Average on the last time step
        If iTime = Me.OriginalNTimesteps + (NYearsProject * Me.EcosimData.NumStepsPerYear) Then
            Me.averageCatchDiscards(iTime)
        End If

    End Sub

    Private Sub sumCatchDiscards(ByVal BiomassAtTimestep() As Single, ByVal iTime As Integer)
        Dim CatchFleetGrp As Single
        Dim DenDepCatch As Single

        For igrp As Integer = 1 To Me.m_core.nGroups
            'Density dependant catchability
            DenDepCatch = m_ecosim.EcosimData.QmQo(igrp) / (1 + (m_ecosim.EcosimData.QmQo(igrp) - 1) * BiomassAtTimestep(igrp) / m_ecosim.EcosimData.StartBiomass(igrp))

            For iflt As Integer = 1 To Me.m_core.nFleets
                If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then

                    CatchFleetGrp = Me.Calc_Total_Catch_If_Quota_Didnt_Affect_It(BiomassAtTimestep(igrp), iflt, igrp, iTime)

                    'Debug.Assert((m_ecosim.EcosimData.Propdiscardtime(iflt, igrp) > 0 And _simdata.FishRateGear(iflt, iTime) > 0) = False)

                    Me.LandingsDiscards(iflt, igrp, eCatchTypes.Landings) += CatchFleetGrp * m_ecosim.EcosimData.PropLandedTime(iflt, igrp)
                    Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardMortalities) += CatchFleetGrp * m_ecosim.EcosimData.Propdiscardtime(iflt, igrp)
                    Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardSurvivals) += CatchFleetGrp * (1 - m_ecosim.EcosimData.PropLandedTime(iflt, igrp)) * (1 - m_ecopath.EcopathData.PropDiscardMort(iflt, igrp))

                    'Sum across fleets into the zero index
                    Me.LandingsDiscards(0, igrp, eCatchTypes.Landings) += CatchFleetGrp * m_ecosim.EcosimData.PropLandedTime(iflt, igrp)
                    Me.LandingsDiscards(0, igrp, eCatchTypes.DiscardMortalities) += CatchFleetGrp * m_ecosim.EcosimData.Propdiscardtime(iflt, igrp)
                    Me.LandingsDiscards(0, igrp, eCatchTypes.DiscardSurvivals) += CatchFleetGrp * (1 - m_ecosim.EcosimData.PropLandedTime(iflt, igrp)) * (1 - m_ecopath.EcopathData.PropDiscardMort(iflt, igrp))

                    ' Dim tmpCatch As Single = Me.LandingsDiscards(iflt, igrp, eCatchTypes.Landings) + Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardsMort) + Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardSurvived)
                    ' Debug.Assert(Math.Abs(CatchFleetGrp - tmpCatch) < 0.00001)

                End If 'If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then
            Next iflt
        Next igrp
    End Sub

    Private Sub averageCatchDiscards(ByVal iTime As Integer)
        Dim nLastTimeStep As Integer = Me.OriginalNTimesteps + (NYearsProject * Me.EcosimData.NumStepsPerYear)
        'If this is the last timestep average the values over the year
        If iTime = nLastTimeStep Then

            Dim n As Integer = Me.EcosimData.NumStepsPerYear
            For igrp As Integer = 1 To Me.m_core.nGroups
                For iflt As Integer = 1 To Me.m_core.nFleets
                    If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then

                        Me.LandingsDiscards(iflt, igrp, eCatchTypes.Landings) /= n
                        Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardMortalities) /= n
                        Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardSurvivals) /= n

                    End If 'If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then
                Next iflt

                Me.LandingsDiscards(0, igrp, eCatchTypes.Landings) /= n
                Me.LandingsDiscards(0, igrp, eCatchTypes.DiscardMortalities) /= n
                Me.LandingsDiscards(0, igrp, eCatchTypes.DiscardSurvivals) /= n

            Next igrp
        End If 'iTime = (NYearsProject * Me.EcosimData.NumStepsPerYear)
    End Sub

#End Region ' Private modeling code

#Region " Distributions and sampling code "

    Public Function SampleGamma(ByVal Alpha As Single, ByVal Theta As Single) As Double
        Dim n As Double = Math.Truncate(Alpha)
        Dim delta As Double = Alpha - n
        Dim xi As Double = 0
        Dim eta As Double
        Dim part1 As Double = 0
        Dim U As Double
        Dim V As Double
        Dim W As Double

        If (n > 0) Then
            For k As Integer = 1 To CInt(n)
                part1 = part1 + Math.Log(m_rand.NextDouble)
            Next
        End If

        If (delta > 0) Then
            Do
                U = m_rand.NextDouble
                V = m_rand.NextDouble
                W = m_rand.NextDouble

                If (U <= (Math.E / (Math.E + delta))) Then
                    xi = V ^ (1 / delta)
                    eta = W * xi ^ (delta - 1)
                Else
                    xi = 1 - Math.Log(V)
                    eta = W * Math.Exp(-xi)
                End If
            Loop Until eta <= (xi ^ (delta - 1) * Math.Exp(-xi))
        End If

        Return ((xi - part1) * Theta)

    End Function

    Public Function DirichletSample2(ByVal nDimensions As Integer, ByVal alpha() As Single, ByRef DietMultiplier As Double) As Single()
        Dim gamma(nDimensions - 1) As Single
        Dim dirichlet(nDimensions - 1) As Single
        Dim sumofgamma As Single
        Dim GammaGenerator As New GammaDistribution

        For i = 0 To alpha.Length() - 1
            'alpha(i) = alpha(i) * TempDietMultiplier
            alpha(i) = CSng(alpha(i) * DietMultiplier)
        Next

        For i As Integer = 0 To nDimensions - 1
            'GammaGenerator.Alpha = alpha(i)
            'GammaGenerator.Theta = 1
            'gamma(i) = CSng(GammaGenerator.NextDouble())
            gamma(i) = CSng(SampleGamma(alpha(i), 1))
        Next

        sumofgamma = gamma.Sum()
        For i = 0 To nDimensions - 1
            dirichlet(i) = gamma(i) / sumofgamma
        Next

        Return (dirichlet)

    End Function

    '    Private Function DirichletSample(ByVal nDimensions As Integer, ByVal a() As Single, ByRef DietMultiplier As Double) As Single()
    '        Dim u1, u2, b, p, x As Single
    '        Dim gamma(nDimensions - 1) As Single
    '        Dim dirichlet(nDimensions - 1) As Single
    '        Dim sumofgamma As Single

    '        For i = 0 To a.Length() - 1
    '            a(i) = CSng(a(i) * DietMultiplier)
    '        Next

    '        For i As Integer = 0 To nDimensions - 1
    'step1:
    '            u1 = CSng(Me.m_rand.NextDouble())
    '            b = CSng((Math.E + a(i)) / Math.E)
    '            p = b * u1
    '            If p >= 1 Then GoTo step3
    'step2:
    '            x = CSng(p ^ (1 / a(i)))
    '            u2 = CSng(Me.m_rand.NextDouble())
    '            If u2 > Math.Exp(-x) Then
    '                GoTo step1
    '            Else
    '                gamma(i) = x
    '                GoTo stepend
    '            End If
    'step3:
    '            x = CSng(-Math.Log((b - p) / a(i)))
    '            u2 = CSng(Me.m_rand.NextDouble())
    '            If u2 > x ^ (a(i) - 1) Then
    '                GoTo step1
    '            Else
    '                gamma(i) = x
    '            End If
    'stepend:
    '        Next

    '        sumofgamma = gamma.Sum()
    '        For i = 0 To nDimensions - 1
    '            dirichlet(i) = gamma(i) / sumofgamma
    '        Next

    '        Return (dirichlet)

    '    End Function

    Private Function UniformSample(ByVal min_par As Single, ByVal max_par As Single) As Double

        Return (min_par + Me.m_rand.NextDouble() * (max_par - min_par))

    End Function

    Private Function TriangularSample(ByVal A_par As Single, ByVal B_par As Single, ByVal C_par As Single) As Double

        Dim U As Double = Me.m_rand.NextDouble()
        If U < ((C_par - A_par) / (B_par - A_par)) Then
            Return A_par + Math.Sqrt(U * (B_par - A_par) * (C_par - A_par))
        Else
            Return B_par - Math.Sqrt((1 - U) * (B_par - A_par) * (B_par - C_par))
        End If

    End Function

    Private Sub SampleDietMatrix(ByVal Interacts(,) As Integer, ByVal InteractsImports() As Integer, ByVal MeanProportions(,) As Single, ByVal MeanProportionsImports() As Single, ByVal DietPropMultipliers() As Double)
        Dim MeanPropMod() As Single
        Dim SumInteractions(m_core.nLivingGroups - 1) As Single
        Dim TempDirichlet() As Single
        Dim DirichStopWatch As New Stopwatch
        Dim NormaliseStopWatch As New Stopwatch
        Dim EcopathStopWatch As New Stopwatch
        Dim EcopathInternalStopWatch As New Stopwatch
        Dim ecopathData As cEcopathDataStructures = Me.m_ecopath.EcopathData
        Dim iPointer As Integer = 0


        'Dim DirichletArray(mCore.nLivingGroups - 1, mCore.nLivingGroups - 1) As Single

        'Array.Clear(DirichletArray, 0, DirichletArray.GetLength(1))

        'Generate a vector 'SumInteractions' that counts how many prey each predator has
        For iPred As Integer = 0 To m_core.nLivingGroups - 1
            SumInteractions(iPred) += InteractsImports(iPred)
            For iPrey As Integer = 0 To m_core.nGroups - 1
                SumInteractions(iPred) += Interacts(iPred, iPrey)
            Next
        Next

        For iPred As Integer = 0 To m_core.nLivingGroups - 1
            'mCore.EcoPathGroupInputs(iPred + 1).DietComp(0) = 0
            If (SumInteractions(iPred) = 0) Then    'No need to do any of this unless there is at least 1 prey for this parameter
                'Set all values to zero - if running slow might want to consider how this could be skipped - possibly setting whole array to zero at start
                For iPrey = 0 To m_core.nGroups
                    ecopathData.DCInput(iPred + 1, iPrey) = 0
                Next
            Else
                ' DirichStopWatch.Start()

                ReDim MeanPropMod(CInt(SumInteractions(iPred) - 1))
                iPointer = 0
                If InteractsImports(iPred) = 1 Then
                    MeanPropMod(iPointer) = MeanProportionsImports(iPred)
                    iPointer += 1
                End If
                For iPrey = 0 To m_core.nGroups - 1
                    If Interacts(iPred, iPrey) = 1 Then
                        MeanPropMod(iPointer) = MeanProportions(iPred, iPrey)
                        iPointer += 1
                    End If
                Next iPrey

                'Samples a set of Dirichlet distributed parameters
                TempDirichlet = DirichletSample2(CInt(SumInteractions(iPred)), MeanPropMod, DietPropMultipliers(iPred))

                Dim i As Integer = 0
                Dim dProp As Single
                If InteractsImports(iPred) = 1 Then
                    dProp = TempDirichlet(i)
                    If dProp < MIN_DIET_PROP Then dProp = 0.0F
                    ecopathData.DCInput(iPred + 1, 0) = dProp
                    i += 1
                End If 'InteractsImports(iPred) = 1

                For iPrey = 1 To m_core.nGroups
                    If Interacts(iPred, iPrey - 1) = 1 Then
                        dProp = TempDirichlet(i)
                        If dProp < MIN_DIET_PROP Then dProp = 0.0F
                        ecopathData.DCInput(iPred + 1, iPrey) = dProp
                        i += 1
                    End If 'Interacts(iPred, iPrey - 1) = 1
                Next iPrey

            End If '(SumInteractions(iPred) = 0)
        Next iPred

    End Sub

#End Region ' Distributions and sampling code

#Region " EwE Events onEcosimInitialized()... "

    Public Sub onEcosimInitialized(ByVal EcosimDatastructures As cEcosimDatastructures)

        Me._simdata = EcosimDatastructures

        Me.m_regulations = New cRegulations(Me, Me.m_core)
        Me.m_quotashares = New cQuotaShares(Me, Me.m_core)
        Me.m_survivability = New cSurvivability(Me, Me.m_core, EcosimDatastructures, _pathdata)
        Me.m_strategies = New Strategies(Me, Me.m_core)
        Me.m_effortlimits = New cEffortLimits(Me, Me.m_core)
        Me.m_StockAssessment = New cStockAssessmentModel(Me)
        Me.m_diets = New cDiets(Me, Me.m_core)

    End Sub

    Public Sub onEcosimRunBeginning(ByVal EcosimDatastructures As cEcosimDatastructures)
        Try
            Me.StockAssessment.InitForRun()
        Catch ex As Exception

        End Try
    End Sub

    Public Sub onEcopathInitialized(ByVal EcopathData As cEcopathDataStructures)
        Me._pathdata = EcopathData
    End Sub

    Public Sub onEcosimBeginTimeStep(ByRef BiomassAtTimestep() As Single, ByVal iTime As Integer)
        ' JS 13Oct13: Fixed CurDir vulnerability in lpsolve
        ' JS 13Oct13: Globalized this method
        ' JS 13Oct13: Fixed path usage
        ' JS 13Oct13: Removed MsgBox

        'Must have nfleets+1 elements so for 10 fleets needs elements 0-10
        'This is because of the way code works in EwE
        Dim TargetF(m_core.nGroups) As Double
        Dim QMult(m_ecosim.EcosimData.nGroups) As Single
        Dim NumberTimeStepsIntoProjection As Integer

        m_currentTimeStep = iTime

        NumberTimeStepsIntoProjection = iTime - OriginalNTimesteps

        If ChangeEffortFlag = True And iTime > OriginalNTimesteps Then 'Flag is only set to true when the button on the form is clicked
            'this is so that its only executed when ecosim is run from mseform

            'jb QMult() is Density dependant catchability
            QMult = Me.CalcDensityDependency(BiomassAtTimestep)

            If (iTime - 1) Mod 12 = 0 Then 'if it is the first timestep of the year

                If iTime = OriginalNTimesteps + 1 Then
                    m_fleet_max_effort = New List(Of cFleetMaxEffort)
                    For iFleet = 1 To m_core.nFleets
                        m_fleet_max_effort.Add(New cFleetMaxEffort(iFleet:=iFleet,
                                                                   final_hindcast_effort:=_simdata.FishRateGear(iFleet, iTime - 1),
                                                                   max_percentage_change_in_max_effort:=m_effortlimits.Value(iFleet),
                                                                   decaying_max_effort:=m_effortlimits.decaying_max_effort))
                    Next
                Else

                End If

                For iFleet = 1 To m_ecosim.EcosimData.nGear
                    m_FleetImpError(iFleet) = StockAssessment.getImplementationError(iFleet)
                Next

                Me.setQModifiers(iTime)

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Get Biomass estimated by the stock assessment model
                Dim bioEst() As Single = Me.StockAssessment.DoAnnualStockAssessment(Me.currentStrategy, iTime, BiomassAtTimestep)

                'OK Hook the stock assessment model up to the Quota setting
                'Use the biomass estimated by the stock assessment model as the true biomass to set the Quota
                Dim NumberofYearProjecting As Integer = CInt((iTime + 11 - OriginalNTimesteps) / 12)
                TargConsQuota = DetermineQuotas(bioEst, NumberofYearProjecting, iTime)

                SetMinMaxEfforts(iTime)

            End If

            CalcEffortAndDiscards(BiomassAtTimestep, iTime, NumberTimeStepsIntoProjection, QMult)

            'Sets F's used by Ecosim for this timestep
            'given the regulated effort and proportions of landing and discards set above
            Me.SetFtimeFromGear(iTime)

#If DEBUG Then
            TestingJumpInFAtBeginProjection(iTime)
#End If

            RecordRealisedFResults(iTime, BiomassAtTimestep, NumberTimeStepsIntoProjection, QMult)

        End If

    End Sub

    Private Sub OutputFHeadings()
        'Dim FChange As Double
        Dim strmWriter As StreamWriter
        Dim strFile As String = cFileUtils.ToValidFileName("Diagnostics_F_Steps.csv", False)
        Dim fileexists As Boolean = File.Exists(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, strFile))
        strmWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, strFile), False)

        strmWriter.WriteLine("ModelNumber,Strategy,GroupName,F")


        ''output model number
        'strmWriter.Write("Model Number")
        'For iModel = 1 To NModels2Run
        '    For Each iStrategy In Strategies
        '        For iGroup = 1 To m_core.nGroups
        '            If iStrategy.StrategyContainsHCRforiGrp(iGroup) Then
        '                strmWriter.Write(", " & iModel)
        '            End If
        '        Next
        '    Next
        'Next
        'strmWriter.WriteLine()

        ''output strategy
        'strmWriter.Write("Strategy")
        'For iModel = 1 To NModels2Run
        '    For Each iStrategy In Strategies
        '        For iGroup = 1 To m_core.nGroups
        '            If iStrategy.StrategyContainsHCRforiGrp(iGroup) Then
        '                strmWriter.Write(", " & iStrategy.Name)
        '            End If
        '        Next
        '    Next
        'Next
        'strmWriter.WriteLine()

        ''output group numbers
        'strmWriter.Write("Group Number")
        'For iModel = 1 To NModels2Run
        '    For Each iStrategy In Strategies
        '        For iGroup = 1 To m_core.nGroups
        '            If iStrategy.StrategyContainsHCRforiGrp(iGroup) Then
        '                strmWriter.Write(", " & iGroup)
        '            End If
        '        Next
        '    Next
        'Next
        'strmWriter.WriteLine()

        strmWriter.Close()
        strmWriter.Dispose()
    End Sub

    Private Sub RecordRealisedFResults(ByVal iTime As Integer, ByVal BiomassAtTimeStep() As Single, ByVal NumberTimeStepsIntoProjection As Integer, ByRef QMult() As Single)

        For iGrp = 1 To m_core.nGroups
            'If we want f's for entire run including hindcast we need to not multiply by qmult for any periods with applied forcing f
            m_RealisedFs(iGrp - 1, NumberTimeStepsIntoProjection - 1) = Me._simdata.FishRateNo(iGrp, iTime) * QMult(iGrp)
            'Calculate the Realised Landed F
            m_RealisedLandedFs(iGrp - 1, NumberTimeStepsIntoProjection - 1) = Calc_RealisedLandedFs(BiomassAtTimeStep(iGrp), iGrp, iTime)
            m_RealisedDiscardFs(iGrp - 1, NumberTimeStepsIntoProjection - 1) = Calc_RealisedDiscardFs(BiomassAtTimeStep(iGrp), iGrp, iTime)
        Next

    End Sub

    Private Function FindHighestValueStock(ByVal iFleet As Integer) As Integer

        Dim vmax As Single = -9999
        Dim imax As Integer = 0
        Dim v As Single
        For iGrp = 1 To m_ecopath.EcopathData.NumGroups
            If (m_ecopath.EcopathData.Landing(iFleet, iGrp)) > 0 And TargConsQuota(iGrp - 1, HCRType.Target) >= 0 And IsTargetSpecies(iGrp, iFleet) Then
                If m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Target) >= m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Conservation) Then
                    v = CSng(m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Target) * m_ecopath.EcopathData.Market(iFleet, iGrp))
                Else
                    v = CSng(m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Conservation) * m_ecopath.EcopathData.Market(iFleet, iGrp))
                End If
                If v > vmax Then
                    vmax = v
                    imax = iGrp
                End If
            End If
        Next iGrp

        Return imax

    End Function


    Private Sub RecordHighestValueStock(ByVal iTime As Integer, ByVal iFleet As Integer, ByVal iMax As Integer)

        m_HighestValueSpecies(iFleet - 1, (iTime - OriginalNTimesteps - 1) \ 12) = m_core.EcoPathGroupOutputs(iMax).Name

    End Sub

    Private Function CalcEffort2CatchHighestValue(ByVal iFleet As Integer, ByVal iMax As Integer, ByVal QMult() As Single, ByVal BiomassAtTimestep() As Single, ByRef Emax As Single) As Single
        'ToDo jb Check that this is using FishMGear() correctly
        Emax = 0
        'Emax = CSng((m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iMax).mShare * TargConsQuota(iMax - 1, HCRType.Target) / m_ecopath.EcopathData.PropLanded(iFleet, iMax)) / (1.0E-20 + QMult(iMax) * m_ecosim.EcosimData.FishMGear(iFleet, iMax) * BiomassAtTimestep(iMax)))
        Emax = (m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iMax, False).mShare * TargConsQuota(iMax - 1, HCRType.Target) / m_ecopath.EcopathData.PropLanded(iFleet, iMax)) / (CSng(1.0E-20) + QMult(iMax) * m_ecosim.EcosimData.relQ(iFleet, iMax) * BiomassAtTimestep(iMax))
        Return Emax
    End Function

    'Private Sub ReportQuotaShareExists(ByVal iFleet As Integer, ByVal iGroup As Integer)
    '    If m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGroup).mShare = Nothing Then
    '        MessageBox.Show("A quotashare, that is not specified for fleet " & m_core.FleetInputs(iFleet).Name & " group " & m_core.EcoPathGroupInputs(iGroup).Name & " is required. Please specify and then rerun", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    '    End If
    'End Sub

    Private Function ChangeMaxEffortIfConsRequires(ByVal iMax As Integer, ByVal iFleet As Integer, Emax As Single, ByVal QMult() As Single, ByVal BiomassAtTimestep() As Single) As Single
        'ToDo jb Check that this is using FishMGear() correctly
        Dim Elim_Conservation As Single = 200
        If TargConsQuota(iMax - 1, HCRType.Conservation) <> cEffortLimits.NoHCR_F And TargConsQuota(iMax - 1, HCRType.Target) <> cEffortLimits.NoHCR_F Then
            'Elim_Conservation = CSng((m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iMax).mShare * TargConsQuota(iMax - 1, HCRType.Conservation) / m_ecopath.EcopathData.PropLanded(iFleet, iMax)) / (1.0E-20 + QMult(iMax) * m_ecosim.EcosimData.FishMGear(iFleet, iMax) * BiomassAtTimestep(iMax)))
            Elim_Conservation = (m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iMax, False).mShare * TargConsQuota(iMax - 1, HCRType.Conservation) / m_ecopath.EcopathData.PropLanded(iFleet, iMax)) / (CSng(1.0E-20) + QMult(iMax) * m_ecosim.EcosimData.relQ(iFleet, iMax) * BiomassAtTimestep(iMax))
            If Elim_Conservation < Emax Then Emax = Elim_Conservation
        End If
        Return Emax
    End Function

    Private Function SetEmaxBelowUserSpecifiedMaxEffort(ByVal Effort As Single, ByVal iFleet As Integer) As Single

        If Effort > MaxEffortThisYear(iFleet - 1) Then
            Effort = MaxEffortThisYear(iFleet - 1)
        End If

        Debug.Assert(Effort >= 0)

        Return Effort

    End Function

    Private Function SetEffortBelowHighestPermittedEffort(ByVal Effort As Single, ByVal iFleet As Integer, ByVal iTime As Integer) As Boolean

        If Effort < _simdata.FishRateGear(iFleet, iTime) Then
            _simdata.FishRateGear(iFleet, iTime) = Effort
            Return True
        Else
            Return False
        End If

    End Function

    Private Function Fleet_Catches_Group_With_Quota(ByVal iFleet As Integer, ByVal iGrp As Integer) As Boolean

        If (m_ecopath.EcopathData.Landing(iFleet, iGrp) + m_ecopath.EcopathData.Discard(iFleet, iGrp) > 0) And (TargConsQuota(iGrp - 1, HCRType.Target) <> cCore.NULL_VALUE) Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function There_Is_A_Conservation_HCR_For(ByVal iGrp As Integer) As Boolean
        If TargConsQuota(iGrp - 1, HCRType.Conservation) <> cEffortLimits.NoHCR_F Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function There_Is_A_Target_HCR_For(ByVal iGrp As Integer) As Boolean
        If TargConsQuota(iGrp - 1, HCRType.Target) <> cEffortLimits.NoHCR_F Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function Calc_Fleet_Quota_Given_Conservation_HCR_Exists(ByVal iGrp As Integer, ByVal iFleet As Integer) As Double

        Dim FleetConsQuota As Double
        Dim FleetTargQuota As Double
        Dim FleetQuota As Double

        FleetTargQuota = m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Target)
        FleetConsQuota = m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Conservation)
        If FleetTargQuota <= FleetConsQuota Then
            FleetQuota = FleetTargQuota
        Else
            FleetQuota = FleetConsQuota
        End If

        Return FleetQuota

    End Function

    Private Function Calc_Fleet_Quota_Given_Only_Target_HCR_Exists(ByVal iGrp As Integer, ByVal iFleet As Integer) As Double

        Return m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Target)

    End Function

    Private Function Theoretical_Landings_Is_Greater_Than_Quota(ByVal iGrp As Integer, ByVal iFleet As Integer, ByVal iCatch As Single, ByVal FleetQuota As Double) As Boolean
        Return iCatch * m_ecopath.EcopathData.PropLanded(iFleet, iGrp) > FleetQuota
    End Function

    Private Sub Set_Prop_Landed_plus_Prop_Discarded_and_Die_Given_Greater(ByVal iGrp As Integer, ByVal iFleet As Integer, ByVal iCatch As Single, ByVal FleetQuota As Double)
        Dim Catch2LandQuota As Single

        'fishing mortality exceeds quota
        m_ecosim.EcosimData.PropLandedTime(iFleet, iGrp) = CSng(FleetQuota / (iCatch + 1.0E-20))

        If Me.currentStrategy.Regulations.Method(iFleet) = cRegulations.eRegMethod.HighestValue Or Not IsTargetSpecies(iGrp, iFleet) Then
            'QuotaType = Strongest
            'excess catch discarded and included in the fishing mortality()
            m_ecosim.EcosimData.Propdiscardtime(iFleet, iGrp) = (1 - m_ecosim.EcosimData.PropLandedTime(iFleet, iGrp)) * m_ecopath.EcopathData.PropDiscardMort(iFleet, iGrp)
        Else
            'QuotaType = Selective 
            'excess catch is NOT included in fishing mortality all discards survive
            'm_ecosim.EcosimData.Propdiscardtime(iFleet, iGrp) = CSng(1 - (FleetQuota / (m_ecopath.EcopathData.PropLanded(iFleet, iGrp) * iCatch)) * (1 - m_ecopath.EcopathData.PropDiscard(iFleet, iGrp)) - (iCatch - FleetQuota / m_ecopath.EcopathData.PropLanded(iFleet, iGrp)) / iCatch)
            Catch2LandQuota = CSng(FleetQuota / (m_ecopath.EcopathData.PropLanded(iFleet, iGrp) + +1.0E-20))
            m_ecosim.EcosimData.Propdiscardtime(iFleet, iGrp) = CSng((1 - m_ecosim.EcosimData.PropLandedTime(iFleet, iGrp)) * (Catch2LandQuota / iCatch) * (m_ecopath.EcopathData.PropDiscardMort(iFleet, iGrp)))
        End If

    End Sub

    Private Function IsTargetSpecies(ByVal iGrp As Integer, ByVal iFleet As Integer) As Boolean

        Dim qshare As cQuotaShares.QuotaShare = m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False)

        If qshare Is Nothing Then
            Return False
        ElseIf qshare.mShare = 0 Then
            Return False
        Else
            Return True
        End If

    End Function

    Private Sub SetDiscardParameters_HighestValue_Selective(ByVal iFleet As Integer, ByVal iTime As Integer, ByVal BiomassAtTimestep() As Single)
        Dim iCatch As Single

        Dim FleetQuota As Double

        For iGrp = 1 To m_ecopath.EcopathData.NumGroups

            If Fleet_Catches_Group_With_Quota(iFleet, iGrp) Then

                'iCatch is theoretical and whether it is caught or not depends on whether it is greater than
                'the quota and the regulation Is selective

                iCatch = Calc_Total_Catch_If_Quota_Didnt_Affect_It(BiomassAtTimestep(iGrp), iFleet, iGrp, iTime)

                If There_Is_A_Conservation_HCR_For(iGrp) Then

                    FleetQuota = Calc_Fleet_Quota_Given_Conservation_HCR_Exists(iGrp, iFleet)

                ElseIf There_Is_A_Target_HCR_For(iGrp) Then

                    FleetQuota = Calc_Fleet_Quota_Given_Only_Target_HCR_Exists(iGrp, iFleet)

                End If

                If Theoretical_Landings_Is_Greater_Than_Quota(iGrp, iFleet, iCatch, FleetQuota) Then

                    Set_Prop_Landed_plus_Prop_Discarded_and_Die_Given_Greater(iGrp, iFleet, iCatch, FleetQuota)

                Else ' Theoretical Landings are less than the Quota

                    Set_Prop_Landed_Prop_Discarded_Die_To_Default(iFleet, iGrp)

                End If
            End If
        Next iGrp

    End Sub

    Private Sub SetEffortWithinPermissableRange_HighestValue_Selective(ByVal iFleet As Integer, ByVal iMax As Integer, ByVal QMult() As Single,
                                                      ByVal BiomassAtTimestep() As Single, ByVal iTime As Integer)

        Dim Effort As Single 'the calculated effort that is within the permissable range

        'Calc effort to catch highest value stock
        Effort = CalcEffort2CatchHighestValue(iFleet, iMax, QMult, BiomassAtTimestep, Effort)

        'Calculate the maximum effort given the conservations
        Effort = ChangeMaxEffortIfConsRequires(iMax, iFleet, Effort, QMult, BiomassAtTimestep)

        'Check whether the calculated effort is greater than the max increase and if it is set it to the max increase
        Effort = SetEmaxBelowUserSpecifiedMaxEffort(Effort, iFleet)

        'Limit the effort if it is greater than the max allowable
        SetEffortBelowHighestPermittedEffort(Effort, iFleet, iTime)

    End Sub

    Private Function CalcEffortFromTargAndConsQuotas_WeakestStock(ByVal iFleet As Integer, ByVal iGrp As Integer, ByVal Elim As Single, QMult() As Single, BiomassAtTimestep() As Single) As Single

        Dim Elim_Target As Single
        Dim Elim_Conservation As Single

        If TargConsQuota(iGrp - 1, HCRType.Target) <> cEffortLimits.NoHCR_F Then
            Elim_Target = (m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Target) / m_ecopath.EcopathData.PropLanded(iFleet, iGrp)) / (CSng(1.0E-20) + QMult(iGrp) * _simdata.relQ(iFleet, iGrp) * BiomassAtTimestep(iGrp))
            Elim = Elim_Target 'sets this because if both cons and targ don't exist then this is what it will be
        End If
        If TargConsQuota(iGrp - 1, HCRType.Conservation) <> cEffortLimits.NoHCR_F Then
            Elim_Conservation = (m_quotashares.ReadiFleetiGroupQuotaShare(iFleet, iGrp, False).mShare * TargConsQuota(iGrp - 1, HCRType.Conservation) / m_ecopath.EcopathData.PropLanded(iFleet, iGrp)) / (CSng(1.0E-20) + QMult(iGrp) * _simdata.relQ(iFleet, iGrp) * BiomassAtTimestep(iGrp))
            Elim = Elim_Conservation 'sets this because if both cons and targ don't exist then this is what it will be
        End If
        If TargConsQuota(iGrp - 1, HCRType.Target) <> cEffortLimits.NoHCR_F And TargConsQuota(iGrp - 1, HCRType.Conservation) <> cEffortLimits.NoHCR_F Then
            If Elim_Conservation < Elim_Target Then Elim = Elim_Conservation
            If Elim_Conservation >= Elim_Target Then Elim = Elim_Target
        End If

        Return Elim

    End Function

    Private Sub SetEffortWithinPermissableRange_WeakestStock(ByVal iFleet As Integer, ByVal iGrp As Integer, ByVal QMult() As Single,
                                                      ByVal BiomassAtTimestep() As Single, ByVal iTime As Integer, ByVal NumberTimeStepsIntoProjection As Integer)

        Dim Elim As Single 'the maximum effort that can be exerted without causing discards

        Elim = CalcEffortFromTargAndConsQuotas_WeakestStock(iFleet, iGrp, Elim, QMult, BiomassAtTimestep)

        'Check whether the calculated effort is less than the max decrease and if it is set it to the max decrease
        If TargConsQuota(iGrp - 1, HCRType.Target) <> cCore.NULL_VALUE Or TargConsQuota(iGrp - 1, HCRType.Conservation) <> cCore.NULL_VALUE Then
            Elim = SetEmaxBelowUserSpecifiedMaxEffort(Elim, iFleet)

            If SetEffortBelowHighestPermittedEffort(Elim, iFleet, iTime) = True Then
                m_ChokeGroup(iFleet - 1, NumberTimeStepsIntoProjection - 1) = m_core.EcoPathGroupOutputs(iGrp).Name
            End If

        End If

    End Sub



    Private Sub CalcEffortAndDiscards(ByVal BiomassAtTimestep() As Single, ByVal iTime As Integer, ByVal NumberTimeStepsIntoProjection As Integer, ByVal QMult() As Single)

        Dim iMax As Integer

        'if there are no fleets to optimise for skip all this
        If FleetsThatFishHCRGrp.Count > 0 Then

            For Each iFleet In FleetsThatFishHCRGrp

                Select Case Me.currentStrategy.Regulations.Method(iFleet)

                    Case cRegulations.eRegMethod.HighestValue, cRegulations.eRegMethod.SelectiveFishing

                        'find the stock with the biggest economic value that the given fleet catches
                        iMax = FindHighestValueStock(iFleet)

                        'Check whether it is the beginning of a year and if so record what the highest value species is
                        If (iTime - 1) Mod 12 = 0 Then
                            RecordHighestValueStock(iTime, iFleet, iMax)
                        End If

                        SetEffortWithinPermissableRange_HighestValue_Selective(iFleet, iMax, QMult, BiomassAtTimestep, iTime)

                        'Alters the discard parameters
                        SetDiscardParameters_HighestValue_Selective(iFleet, iTime, BiomassAtTimestep)


                    Case cRegulations.eRegMethod.WeakestStock

                        For iGrp = 1 To m_ecopath.EcopathData.NumGroups

                            If (m_ecopath.EcopathData.Landing(iFleet, iGrp) + m_ecopath.EcopathData.Discard(iFleet, iGrp)) > 0 Then

                                If IsTargetSpecies(iGrp, iFleet) Then

                                    SetEffortWithinPermissableRange_WeakestStock(iFleet, iGrp, QMult, BiomassAtTimestep, iTime, NumberTimeStepsIntoProjection)

                                    Set_Prop_Landed_Prop_Discarded_Die_To_Default(iFleet, iGrp)

                                Else

                                    Set_Prop_Landed_Prop_Discarded_Die_Given_NonTarget(iFleet, iGrp)

                                End If

                            End If
                        Next iGrp

                    Case cRegulations.eRegMethod.None

                        _simdata.FishRateGear(iFleet, iTime) = _simdata.FishRateGear(iFleet, iTime - 1)

                        For iGrp = 1 To m_ecopath.EcopathData.NumGroups

                            If IsTargetSpecies(iGrp, iFleet) Then

                                Set_Prop_Landed_Prop_Discarded_Die_To_Default(iFleet, iGrp)

                            Else

                                Set_Prop_Landed_Prop_Discarded_Die_Given_NonTarget(iFleet, iGrp)

                            End If

                        Next

                    Case cRegulations.eRegMethod.NoFishing

                        _simdata.FishRateGear(iFleet, iTime) = 0

                End Select
            Next

        End If

        For iFleet = 1 To m_core.nFleets
            If FleetsThatFishHCRGrp.IndexOf(iFleet) = -1 Then
                'This sets the effort for any fleet that does not have a HCR which affects it to the effort as it was in the previous timestep
                _simdata.FishRateGear(iFleet, iTime) = _simdata.FishRateGear(iFleet, iTime - 1)
                For iGrp = 1 To m_ecopath.EcopathData.NumGroups
                    Set_Prop_Landed_Prop_Discarded_Die_To_Default(iFleet, iGrp)
                Next
            Else
                'Make sure the fleet is regulated if we are going add error to Effort Implementation
                If Me.currentStrategy.Regulations.Method(iFleet) <> cRegulations.eRegMethod.None Then
                    'Add uncertainty to the regulated Effort 
                    'This is implementation error. The Effort actually achieved by the Fleet.
                    'The implementation error is not really the business of the stock assessment model
                    'It is part of the stock assessment for practical reasons 
                    '   CV can be included in the interface
                    '   Random number generator needs to be seeded at the same time as the stock assessment model
                    _simdata.FishRateGear(iFleet, iTime) = _simdata.FishRateGear(iFleet, iTime) * Me.m_FleetImpError(iFleet)
                Else
                    _simdata.FishRateGear(iFleet, iTime) = _simdata.FishRateGear(iFleet, iTime - 1)
                End If 'Me.m_currentStrategy.Regulations.Method(iFleet) <> cRegulations.eRegMethod.None

            End If
        Next
    End Sub

    Private Sub Set_Prop_Landed_Prop_Discarded_Die_To_Default(ByVal iFleet As Integer, ByVal iGrp As Integer)

        m_ecosim.EcosimData.PropLandedTime(iFleet, iGrp) = m_ecopath.EcopathData.PropLanded(iFleet, iGrp)
        m_ecosim.EcosimData.Propdiscardtime(iFleet, iGrp) = m_ecopath.EcopathData.PropDiscard(iFleet, iGrp) * m_ecopath.EcopathData.PropDiscardMort(iFleet, iGrp)

    End Sub

    Private Sub Set_Prop_Landed_Prop_Discarded_Die_Given_NonTarget(ByVal iFleet As Integer, ByVal iGrp As Integer)

        m_ecosim.EcosimData.PropLandedTime(iFleet, iGrp) = 0
        m_ecosim.EcosimData.Propdiscardtime(iFleet, iGrp) = m_ecopath.EcopathData.PropDiscard(iFleet, iGrp) * m_ecopath.EcopathData.PropDiscardMort(iFleet, iGrp)

    End Sub


    Private Sub SetMinMaxEfforts(iTime As Integer)
        For iFleet = 1 To m_core.nFleets

            If _simdata.FishRateGear(iFleet, iTime - 1) = 0 Then
                System.Console.WriteLine("CEFAS MSE WARNING: Effort for the last timestep of fleet " _
                                         + Me.EcopathData.FleetName(iFleet) + " = zero.")
            End If

            'If m_effortlimits.decaying_max_effort Then
            '    m_fleet_max_effort(iFleet - 1).UpdateLimit(_simdata.FishRateGear(iFleet, iTime - 1))
            '    MaxEffortThisYear(iFleet - 1) = m_fleet_max_effort(iFleet - 1).MaxEffort
            'Else
            '    MaxEffortThisYear(iFleet - 1) = _simdata.FishRateGear(iFleet, iTime - 1) * (1 + m_effortlimits.Value(iFleet))
            'End If
            m_fleet_max_effort(iFleet - 1).UpdateLimit(_simdata.FishRateGear(iFleet, iTime - 1))
            MaxEffortThisYear(iFleet - 1) = m_fleet_max_effort(iFleet - 1).MaxEffort

            If m_effortlimits.Value(iFleet) = cCore.NULL_VALUE Then MaxEffortThisYear(iFleet - 1) = 200


        Next
    End Sub


    Private Function Calc_RealisedLandedFs(BiomassAtT As Single, ByVal iGrp As Integer, ByVal t As Integer) As Single
        Dim iFleet As Integer, Ft As Single

        For iFleet = 1 To m_ecosim.EcosimData.nGear
            Ft += Me.calcCatchRate(BiomassAtT, iFleet, iGrp, t) * Me.m_ecosim.EcosimData.PropLandedTime(iFleet, iGrp)
        Next

        Return Ft

    End Function

    Private Function Calc_RealisedDiscardFs(BiomassAtT As Single, ByVal iGrp As Integer, ByVal t As Integer) As Single
        Dim iFleet As Integer, Ft As Single

        For iFleet = 1 To m_ecosim.EcosimData.nGear
            'Propdiscardtime(iFleet, iGrp) does not include discards that survived
            Ft += calcCatchRate(BiomassAtT, iFleet, iGrp, t) * Me.m_ecosim.EcosimData.Propdiscardtime(iFleet, iGrp)
        Next

        Return Ft

    End Function

    ''' <summary>
    ''' Populates FishRateNo(group,time) with fishing mortality rates for a timestep from the base catch rates, current Effort and Proportions Landed and Discarded.
    ''' FishRateNo(group,time) is then used by Ecosim to set FishTime() in SetFishTime()
    ''' </summary>
    ''' <param name="t">Timestep.</param>
    ''' <remarks>
    ''' Calculates F from the BaseCatchRate(fleet,group) which it the total catch rate including discards that survived.
    ''' The propotion of discards that survives was removed from  Propdiscardtime(fleet,group) in the regulatory code.
    ''' </remarks>
    Public Sub SetFtimeFromGear(ByVal t As Integer)
        Dim i As Integer, ig As Integer, totF As Single

        'fishing mortality at the current effort
        For i = 1 To Me._simdata.nGroups

            totF = 0
            For ig = 1 To Me._simdata.nGear
                'jb 27-June-2014  Propdiscardtime(fleet,group) does not include fish that survived discarding
                totF += Me._simdata.relQ(ig, i) * Me._simdata.FishRateGear(ig, t) * (Me._simdata.PropLandedTime(ig, i) + Me._simdata.Propdiscardtime(ig, i))
            Next

            'Save F for this time step 
            'NOT including Density Dependant Catchability.
            'Density Dependant Catchability will be be applied by Ecosim when FishTime() In SetFishTime()
            Me._simdata.FishRateNo(i, t) = totF

        Next i

    End Sub

    Private Sub setFishForcedToBase()

        Try

            'reloads time series forcing data into core arrays and resets FisForced(groups)
            'F into FishRateNo()
            Me.m_ecosim.TimeSeriesData.DoDatValCalculations()

            'resets F in FishRateNo() based on forced Catches or Effort 
            Me.m_ecosim.SetBaseFFromGear()

        Catch ex As Exception
            Debug.Assert(False, Me.ToString + ".setFishForcedToBase() Exception: " + ex.Message)
        End Try

    End Sub


    'Public Sub RecordPredation(ByRef BiomassAtTimestep() As Single, ByVal iTime As Integer)

    '    Dim iTimeIntoProjection As Integer = iTime - OriginalNTimesteps

    '    For iPred As Integer = 1 To Me.m_core.nGroups
    '        'Density dependant catchability
    '        'DenDepCatch = m_ecosim.EcosimData.QmQo(igrp) / (1 + (m_ecosim.EcosimData.QmQo(igrp) - 1) * BiomassAtTimestep(igrp) / m_ecosim.EcosimData.StartBiomass(igrp))

    '        For iPrey As Integer = 1 To Me.m_core.nFleets
    '            'If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then
    '            'CatchFleetGrp = CSng(_simdata.FishRateGear(iflt, iTime) * DenDepCatch * m_ecosim.EcosimData.FishMGear(iflt, igrp) * BiomassAtTimestep(igrp))
    '            'CatchFleetGrp = Me.Calc_Total_Catch_If_Quota_Didnt_Affect_It(BiomassAtTimestep(igrp), iflt, igrp, iTime)

    '            'Debug.Assert((m_ecosim.EcosimData.Propdiscardtime(iflt, igrp) > 0 And _simdata.FishRateGear(iflt, iTime) > 0) = False)

    '            Me.m_ConsumptionThroughoutProjection(iTimeIntoProjection, iPred, iPrey) = m_ecosim.EcosimData.PredPreyResultsOverTime(cEcosimDatastructures.eEcosimPreyPredResults.Consumption, iPrey, iPred, iTime)
    '            'Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, iPred, iPrey, eCatchTypes.DiscardMortalities) = CatchFleetGrp * m_ecosim.EcosimData.Propdiscardtime(iflt, igrp)
    '            'Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, iPred, iPrey, eCatchTypes.DiscardSurvivals) = CatchFleetGrp * (1 - m_ecosim.EcosimData.PropLandedTime(iflt, igrp)) * (1 - m_ecopath.EcopathData.PropDiscardMort(iflt, igrp))

    '            'Sum across Pred into the zero index
    '            Me.m_ConsumptionThroughoutProjection(iTimeIntoProjection, 0, iPrey) += m_ecosim.EcosimData.PredPreyResultsOverTime(cEcosimDatastructures.eEcosimPreyPredResults.Consumption, iPrey, iPred, iTime)
    '            'Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, 0, iPrey, eCatchTypes.DiscardMortalities) += CatchFleetGrp * m_ecosim.EcosimData.Propdiscardtime(iflt, igrp)
    '            'Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, 0, iPrey, eCatchTypes.DiscardSurvivals) += CatchFleetGrp * (1 - m_ecosim.EcosimData.PropLandedTime(iflt, igrp)) * (1 - m_ecopath.EcopathData.PropDiscardMort(iflt, igrp))

    '            Me.m_ConsumptionThroughoutProjection(iTimeIntoProjection, iPred, 0) += m_ecosim.EcosimData.PredPreyResultsOverTime(cEcosimDatastructures.eEcosimPreyPredResults.Consumption, iPrey, iPred, iTime)
    '            ' Dim tmpCatch As Single = Me.LandingsDiscards(iflt, igrp, eCatchTypes.Landings) + Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardsMort) + Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardSurvived)
    '            ' Debug.Assert(Math.Abs(CatchFleetGrp - tmpCatch) < 0.00001)

    '            'End If 'If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then
    '        Next iPrey
    '    Next iPred
    'End Sub

    Public Sub RecordCatches(ByRef BiomassAtTimestep() As Single, ByVal iTime As Integer)
        Dim DenDepCatch As Single
        Dim CatchFleetGrp As Single
        Dim iTimeIntoProjection As Integer = iTime - OriginalNTimesteps

        For igrp As Integer = 1 To Me.m_core.nGroups
            'Density dependant catchability
            DenDepCatch = m_ecosim.EcosimData.QmQo(igrp) / (1 + (m_ecosim.EcosimData.QmQo(igrp) - 1) * BiomassAtTimestep(igrp) / m_ecosim.EcosimData.StartBiomass(igrp))

            For iflt As Integer = 1 To Me.m_core.nFleets
                'If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then
                'CatchFleetGrp = CSng(_simdata.FishRateGear(iflt, iTime) * DenDepCatch * m_ecosim.EcosimData.FishMGear(iflt, igrp) * BiomassAtTimestep(igrp))
                CatchFleetGrp = Me.Calc_Total_Catch_If_Quota_Didnt_Affect_It(BiomassAtTimestep(igrp), iflt, igrp, iTime)

                'Debug.Assert((m_ecosim.EcosimData.Propdiscardtime(iflt, igrp) > 0 And _simdata.FishRateGear(iflt, iTime) > 0) = False)

                Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, iflt, igrp, eCatchTypes.Landings) = CatchFleetGrp * m_ecosim.EcosimData.PropLandedTime(iflt, igrp)
                Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, iflt, igrp, eCatchTypes.DiscardMortalities) = CatchFleetGrp * m_ecosim.EcosimData.Propdiscardtime(iflt, igrp)
                Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, iflt, igrp, eCatchTypes.DiscardSurvivals) = CatchFleetGrp * (1 - m_ecosim.EcosimData.PropLandedTime(iflt, igrp)) * (1 - m_ecopath.EcopathData.PropDiscardMort(iflt, igrp))

                'Sum across fleets into the zero index
                Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, 0, igrp, eCatchTypes.Landings) += CatchFleetGrp * m_ecosim.EcosimData.PropLandedTime(iflt, igrp)
                Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, 0, igrp, eCatchTypes.DiscardMortalities) += CatchFleetGrp * m_ecosim.EcosimData.Propdiscardtime(iflt, igrp)
                Me.m_LandingsDiscardsThroughoutProjection(iTimeIntoProjection, 0, igrp, eCatchTypes.DiscardSurvivals) += CatchFleetGrp * (1 - m_ecosim.EcosimData.PropLandedTime(iflt, igrp)) * (1 - m_ecopath.EcopathData.PropDiscardMort(iflt, igrp))

                ' Dim tmpCatch As Single = Me.LandingsDiscards(iflt, igrp, eCatchTypes.Landings) + Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardsMort) + Me.LandingsDiscards(iflt, igrp, eCatchTypes.DiscardSurvived)
                ' Debug.Assert(Math.Abs(CatchFleetGrp - tmpCatch) < 0.00001)

                'End If 'If (Me.EcopathData.Landing(iflt, igrp) + Me.EcopathData.Discard(iflt, igrp)) > 0 Then
            Next iflt
        Next igrp
    End Sub

    Public Sub onEcosimEndTimeStep(ByRef BiomassAtTimestep() As Single, ByVal iTime As Integer)
        'Number of time step up to the last year
        Dim nTSToLastYear As Integer = Me.OriginalNTimesteps + (Me.NYearsProject - 1) * EcosimData.NumStepsPerYear

        If ChangeEffortFlag = True And iTime > nTSToLastYear Then
            Me.CalCatchDiscards(BiomassAtTimestep, iTime)
        End If

        If ChangeEffortFlag = True And iTime > Me.OriginalNTimesteps Then
            RecordCatches(BiomassAtTimestep, iTime)
        End If

    End Sub

#End Region ' EwE Events onEcosimInitialized()...

#Region " Helper methods "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Resolve a name and index to a <see cref="cEcopathFleetInput"/> instance.
    ''' </summary>
    ''' <param name="strName">The name to resolve.</param>
    ''' <param name="iIndex">The index to resolve.</param>
    ''' <returns>A <see cref="cEcopathFleetInput"/> instance, or Nothing if
    ''' the index or name did not match any of the present fleets.</returns>
    ''' <remarks>Note that name comparison is not case sensitive.</remarks>
    ''' -----------------------------------------------------------------------
    Private Function ResolveFleet(strName As String, iIndex As Integer) As cEcopathFleetInput
        If (iIndex < 1) Or (iIndex > Me.Core.nFleets) Then Return Nothing
        Dim flt As cEcopathFleetInput = Me.Core.EcopathFleetInputs(iIndex)
        If String.Compare(flt.Name, strName, True) <> 0 Then
            Return Nothing
        End If
        Return flt
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Notify the user of an event.
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="importance"></param>
    ''' <param name="strHyperlink"></param>
    ''' -----------------------------------------------------------------------
    Friend Sub InformUser(strMessage As String, importance As eMessageImportance,
                          Optional strHyperlink As String = "",
                          Optional astrSubMessages As String() = Nothing)

        If (Me.Core Is Nothing) Then Return

        Dim msg As New cMessage(String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_INDEXED, My.Resources.CAPTION, strMessage),
                                eMessageType.Any, eCoreComponentType.External, importance)
        msg.Hyperlink = strHyperlink
        If (astrSubMessages IsNot Nothing) Then
            For Each strSubMessage As String In astrSubMessages
                msg.AddVariable(New cVariableStatus(eStatusFlags.OK, strSubMessage, eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0))
            Next
        End If
        Me.Core.Messages.SendMessage(msg)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ask the user a question.
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="style"></param>
    ''' <param name="importance"></param>
    ''' <param name="replyDefault"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Friend Function AskUser(strMessage As String,
                            style As eMessageReplyStyle,
                            Optional importance As eMessageImportance = eMessageImportance.Question,
                            Optional replyDefault As eMessageReply = eMessageReply.OK) As eMessageReply

        If (Me.Core Is Nothing) Then Return replyDefault

        Dim fmsg As New cFeedbackMessage(String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_INDEXED, My.Resources.CAPTION, strMessage),
                                         eCoreComponentType.External, eMessageType.Any, importance, style)
        fmsg.Reply = replyDefault
        Me.Core.Messages.SendMessage(fmsg)
        Return fmsg.Reply

    End Function

    Private Sub OnCoreMessage(ByRef msg As cMessage)

        Dim bRefresh As Boolean = False

        ' Refresh when Core settings have changed
        If (msg.Type = eMessageType.GlobalSettingsChanged) Then
            bRefresh = True
        End If

        ' Refresh upon ecosim scenario load
        If (msg.Type = eMessageType.DataAddedOrRemoved And msg.Source = eCoreComponentType.EcoSim) Then
            bRefresh = True
        End If

        If (bRefresh = True) Then
            Me.InvalidateConfigurationState()
        End If

    End Sub


#End Region ' Helper methods

#Region " Configurable settings "

    Public Property NModels2Run As Integer
        Get
            'Return Math.Max(1, Math.Min(My.Settings.NModels2Run, 100))
            Return My.Settings.NModels2Run
        End Get
        Set(value As Integer)
            If (value <> My.Settings.NModels2Run) Then
                My.Settings.NModels2Run = value
                My.Settings.Save()
            End If
        End Set
    End Property

    Public Property NModels As Integer
        Get
            'Return Math.Max(1, Math.Min(My.Settings.NTrials, 100))
            Return My.Settings.NTrials
        End Get
        Set(value As Integer)
            If (value <> My.Settings.NTrials) Then
                My.Settings.NTrials = value
                My.Settings.Save()
            End If
        End Set
    End Property

    Public Property NYearsProject As Integer
        Get
            Return Math.Max(1, Math.Min(My.Settings.NYearsProject, 1000))
        End Get
        Set(value As Integer)
            If (value <> My.Settings.NYearsProject) Then
                My.Settings.NYearsProject = value
                My.Settings.Save()
            End If
        End Set
    End Property

    Public Property MassBalanceTol As Single
        Get
            Return Math.Max(0.0!, Math.Min(My.Settings.MassBalanceTol, 0.1!))
        End Get
        Set(value As Single)
            If (value <> My.Settings.MassBalanceTol) Then
                My.Settings.MassBalanceTol = value
                My.Settings.Save()
            End If
        End Set
    End Property

    Public Property UseEwEPath As Boolean
        Get
            Return My.Settings.UseEwEPath
        End Get
        Set(value As Boolean)
            If (value <> My.Settings.UseEwEPath) Then
                My.Settings.UseEwEPath = value
                My.Settings.Save()
                Me.InvalidateConfigurationState(True)
            End If
        End Set
    End Property

    Public Property CustomPath As String
        Get
            Dim strPath As String = My.Settings.CustomPath
            If (String.IsNullOrWhiteSpace(strPath)) Then
                Return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            End If
            Return strPath
        End Get
        Set(value As String)
            If (value <> My.Settings.CustomPath) Then
                My.Settings.CustomPath = value
                My.Settings.Save()
                Me.InvalidateConfigurationState(True)
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the max # of trials for finding a balanced model
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property NMaxAttempts As Integer
        Get
            Return My.Settings.NMaxAttempts
        End Get
        Set(value As Integer)
            If (value <> My.Settings.NMaxAttempts) Then
                My.Settings.NMaxAttempts = value
                My.Settings.Save()
                'Me.InvalidateConfigurationState()
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the max time for finding a balanced model (in fractions of hours)
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property NMaxTime As Single
        Get
            Return My.Settings.NMaxTime
        End Get
        Set(value As Single)
            If (value <> My.Settings.NMaxTime) Then
                My.Settings.NMaxTime = value
                My.Settings.Save()
                'Me.InvalidateConfigurationState()
            End If
        End Set
    End Property

#End Region ' Configurable settings

#Region "Q Modifiers set and clear Ecopath base F's"
    ''' <summary>
    ''' Start of run initializer for Q modifiers (Ecopath base fishing mortality)
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub initQModifier()

        'FishMGear(iFleet, imax)
        Me.m_relQModifier = New Single(Me.m_core.nFleets, Me.m_core.nGroups) {}

        Me.m_bQSet = False

    End Sub


    Private Sub setQModifiers(iTime As Integer)

        'Only set the Q Modifiers once
        'At the start of the first time step of the forecast
        If Me.m_bQSet Then
            'Q Modifiers have already been set
            Return
        End If

        Debug.Assert(iTime = OriginalNTimesteps + 1, "Oppss setQModifiers(t) called at the wrong time step.")

        'Get the relQ() modifier at the previous timestep
        Me.m_relQModifier = Me.calcQModifiers(iTime - 1)

        For iflt As Integer = 1 To Me.m_core.nFleets
            For igrp As Integer = 1 To Me.m_core.nGroups
                If Me._simdata.FishMGear(iflt, igrp) > 0 And Me._simdata.FisForced(igrp) Then

                    'Modify both the Ecosim baseline F and MSE baseline F(Catch rate includes discards).
                    'Ecosim F base fishing mortality rate, this is mortality only it does not include discards that survived.
                    Me._simdata.FishMGear(iflt, igrp) *= Me.m_relQModifier(iflt, igrp)
                    'Ecosim's base catch rate, this includes discards that survived, so it's not just fishing mortality
                    'Modified here so calculation of catch and value are correct in Ecosim
                    Me._simdata.relQ(iflt, igrp) *= Me.m_relQModifier(iflt, igrp)

                End If
            Next
        Next

        Me.m_bQSet = True

        'jb 3-Jun-2016
        'Now that we have used FisForced() flag to figure out which groups are forced clear it at the start of the forecast
        'There is no forcing data for the forecast period.
        'FisForced will be reset at the start of each run in the main run loop via Me.setFishForcedToBase()
        Me._simdata.clearFishForced()

    End Sub

    Public Function calcQModifiers(it As Integer) As Single(,)
        'F Calcualted from baseline values at t
        Dim FtCalc As Double

        'fishing mortality at time  
        'F(t) = q0 * e(t) or Forced F at t
        Dim Ft As Double

        Dim relQMod(m_core.nFleets, m_core.nGroups) As Single
        Dim PropCatchFleet(m_core.nFleets, m_core.nGroups) As Double

        Dim TotPropCatchFleet As Double = 0
        Dim TotFt As Double = 0

        'iTime is the first timestep of the forecast
        'We want to use the data from the last timestep of the hindcast
        'so iTime - 1

        'Get the density dependant catchability at last time step of the hindcast
        Dim QMultAtT() As Single = Me.CalcDensityDependencyAtT(it)

        'Get proporton of catch by Fleet Group 
        For iFleet = 1 To m_core.nFleets
            For iGrp = 1 To m_core.nGroups
                PropCatchFleet(iFleet, iGrp) = Me.PropTotCatchFleet(it, iFleet, iGrp)
            Next
        Next

        For iflt As Integer = 1 To Me.m_core.nFleets
            For igrp As Integer = 1 To Me.m_core.nGroups
                If Me._simdata.relQ(iflt, igrp) > 0 And Me._simdata.FisForced(igrp) Then

                    'Proportion of the fishing mortality caused by this fleet
                    'This is not the baseline proportion but the F at this effort timestep.
                    'This accounts for changes in the effort timeseries
                    Ft = Me._simdata.FishRateNo(igrp, it) * PropCatchFleet(iflt, igrp)

                    'F at the current timestep calculated from baseline values
                    FtCalc = Me._simdata.relQ(iflt, igrp) * QMultAtT(igrp) * Me._simdata.FishRateGear(iflt, it) * (Me._simdata.PropLandedTime(iflt, igrp) + Me._simdata.Propdiscardtime(iflt, igrp))

                    'Ratio of F from time series to F computed from Effort
                    'If there is no timeseries or the F and Effort timeseries are synchronised then m_QModifier will be one
                    relQMod(iflt, igrp) = CSng(Ft / FtCalc)
                    If relQMod(iflt, igrp) = 0 Then relQMod(iflt, igrp) = 1.0

                    If (Math.Round(relQMod(iflt, igrp), 2) <> 1) Then
                        System.Console.WriteLine("Flt=" + iflt.ToString + "," + "Grp=" + igrp.ToString + "," + relQMod(iflt, igrp).ToString + ",")
                    End If

                Else
                    'Not a forced group so the rel Q modifier is one
                    'has no affect on baseline Q
                    relQMod(iflt, igrp) = 1.0
                End If
            Next
        Next

        Return relQMod

    End Function

    'Private Sub setQModifiers(iTime As Integer, ByVal QMult() As Single)
    '    'ToDo Deal with zeros in input data
    '    '   This includes zeros for F(t) where there is a baseline catch
    '    'ToDo PropTotCatchFleet() Make sure including effort in proportion of total catch is correct

    '    'ToDo Check that CalcEffortFromTargAndConsQuotas_WeakestStock(...), ChangeMaxEffortIfConsRequires(...), CalcEffort2CatchHighestValue(...)

    '    'Only set the Q Modifiers once
    '    'At the start of the first time step of the forecast
    '    If Me.m_bQSet Then
    '        'Q Modifiers have already been set
    '        Return
    '    End If

    '    Debug.Assert(iTime = OriginalNTimesteps + 1, "Oppss setQModifiers(t) called at the wrong time step.")

    '    'Effort at time
    '    'Dim Et As Double
    '    'F Calcualted from baseline values at t
    '    Dim FtCalc As Double

    '    'fishing mortality at time  
    '    'F(t) = q0 * e(t) or Forced F at t
    '    Dim Ft As Double

    '    Dim PropCatchFleet(m_core.nFleets, m_core.nGroups) As Double

    '    Dim TotPropCatchFleet As Double = 0
    '    Dim TotFt As Double = 0

    '    'iTime is the first timestep of the forecast
    '    'We want to use the data from the last timestep of the hindcast
    '    'so iTime - 1
    '    Dim it As Integer = iTime - 1

    '    For iFleet = 1 To m_core.nFleets
    '        For iGrp = 1 To m_core.nGroups
    '            PropCatchFleet(iFleet, iGrp) = Me.PropTotCatchFleet(it, iFleet, iGrp)
    '        Next
    '    Next

    '    For iflt As Integer = 1 To Me.m_core.nFleets
    '        For igrp As Integer = 1 To Me.m_core.nGroups
    '            If Me._simdata.FishMGear(iflt, igrp) > 0 And Me._simdata.FisForced(igrp) Then

    '                'Proportion of the fishing mortality caused by this fleet
    '                'This is not the baseline proportion but the F at this effort timestep.
    '                'This accounts for changes in the effort timeseries
    '                Ft = Me._simdata.FishRateNo(igrp, it) * PropCatchFleet(iflt, igrp)

    '                'F at the current timestep calculated from baseline values
    '                FtCalc = Me._simdata.relQ(iflt, igrp) * CalcQMultLastTimeStepHindcast(igrp) * Me._simdata.FishRateGear(iflt, it) * (Me._simdata.PropLandedTime(iflt, igrp) + Me._simdata.Propdiscardtime(iflt, igrp))

    '                'Ratio of F from time series to F computed from Effort
    '                'If there is no timeseries or the F and Effort timeseries are synchronised then m_QModifier will be one
    '                Me.m_QModifier(iflt, igrp) = CSng(Ft / FtCalc)
    '                If Me.m_QModifier(iflt, igrp) = 0 Then Me.m_QModifier(iflt, igrp) = 1.0

    '                If (Math.Round(Me.m_QModifier(iflt, igrp), 2) <> 1) Then
    '                    System.Console.WriteLine("Flt=" + iflt.ToString + "," + "Grp=" + igrp.ToString + "," + Me.m_QModifier(iflt, igrp).ToString + ",")
    '                End If

    '                'Modify both the Ecosim baseline F and MSE baseline F(Catch rate includes discards).
    '                'Ecosim F base fishing mortality rate, this is mortality only it does not include discards that survived.
    '                Me._simdata.FishMGear(iflt, igrp) *= Me.m_QModifier(iflt, igrp)
    '                'Ecosim's base catch rate, this includes discards that survived, so it's not just fishing mortality
    '                'Modified here so calculation of catch and value are correct in Ecosim
    '                Me._simdata.relQ(iflt, igrp) *= Me.m_QModifier(iflt, igrp)
    '            End If
    '        Next
    '    Next

    '    '#If DEBUG Then
    '    '        Dim TestTotalFWhitingLastYearHindcast As Double = 0
    '    '        Dim WhitingIndex As Integer = 16
    '    '        Dim QWhiting As Single = CalcQMultLastTimeStepHindcast(WhitingIndex)
    '    '        For iFleet = 1 To m_core.nFleets
    '    '            TestTotalFWhitingLastYearHindcast += Me._simdata.relQ(iFleet, WhitingIndex) * QWhiting * Me._simdata.FishRateGear(iFleet, it) * (Me._simdata.PropLandedTime(iFleet, WhitingIndex) + Me._simdata.Propdiscardtime(iFleet, WhitingIndex))
    '    '        Next
    '    '#End If

    '    Me.m_bQSet = True
    'End Sub

    Private Function DensityDependencyForGroup(iGrp As Integer, Biomass As Single) As Single

        Return _simdata.QmQo(iGrp) / (1 + (_simdata.QmQo(iGrp) - 1) * Biomass / _simdata.StartBiomass(iGrp))

    End Function

    Public Function CalcDensityDependency(BiomassAtTimestep() As Single) As Single()
        Dim Qmult(Me._pathdata.NumGroups) As Single

        For igrp As Integer = 1 To Me._pathdata.NumGroups
            Qmult(igrp) = DensityDependencyForGroup(igrp, BiomassAtTimestep(igrp))
        Next

        Return Qmult

    End Function

    Private Function CalcDensityDependencyAtT(iTime As Integer) As Single()
        Dim Qmult(Me._pathdata.NumGroups) As Single
        Dim BatT As Single

        For igrp As Integer = 1 To Me._pathdata.NumGroups
            BatT = Me._simdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, iTime)
            Qmult(igrp) = DensityDependencyForGroup(igrp, BatT)
        Next

        Return Qmult

    End Function

    ''' <summary>
    ''' Get the proportion of the fishing mortality caused by a fleet on a group at a time step.
    ''' </summary>
    ''' <param name="iTime"></param>
    ''' <param name="iFleet"></param>
    ''' <param name="iGroup"></param>
    ''' <returns>Proportion of fishing mortality cause by a fleet</returns>
    ''' <remarks></remarks>
    Private Function PropTotCatchFleet(iTime As Integer, iFleet As Integer, iGroup As Integer) As Single

        Dim sumCatchMortality As Single

        'Debug.Assert(iTime = OriginalNTimesteps, "Oppss PropTotCatchFleet(t) called at the wrong time step.")

        For iflt As Integer = 1 To Me.m_core.nFleets
            sumCatchMortality += Me._simdata.relQ(iflt, iGroup) * Me._simdata.FishRateGear(iflt, iTime) * (Me._simdata.PropLandedTime(iflt, iGroup) + Me._simdata.Propdiscardtime(iflt, iGroup))
        Next

        Dim catchMortalityByFleet As Single = Me._simdata.relQ(iFleet, iGroup) * Me._simdata.FishRateGear(iFleet, iTime) * (Me._simdata.PropLandedTime(iFleet, iGroup) + Me._simdata.Propdiscardtime(iFleet, iGroup))

        sumCatchMortality += CSng(1.0E-20)

        'Proportion of total catch caught by this fleet at this timestep
        Return catchMortalityByFleet / sumCatchMortality

    End Function

    Private Sub clearQModifiers()
        For iflt As Integer = 1 To Me.m_core.nFleets
            For igrp As Integer = 1 To Me.m_core.nGroups
                If Me.m_relQModifier(iflt, igrp) > 0 Then
                    'Don't need to restore the mse's internal BaseCatchRate()
                    'Because it will be initialized in Run() for each new model run
                    Me._simdata.FishMGear(iflt, igrp) /= Me.m_relQModifier(iflt, igrp)
                    Me._simdata.relQ(iflt, igrp) /= Me.m_relQModifier(iflt, igrp)
                End If
            Next
        Next

        Me.m_bQSet = False
    End Sub


#End Region

#Region "Error checking"
    Private Sub TestingJumpInFAtBeginProjection(iTime As Integer)

        'The reason we have hardwired the max_percent_change_F is because the precise value is difficult to calculate. If we were to use
        'the max change in fleet effort it would be misleading, because the max change in F is dependent on more than one fleet and 
        'each fleet catches more than one group. And so this value is a guide only and we are using it to flag results which are clearly
        'wrong. This means that this test is not 100% airtight and will be guarenteed to flag all errors, but we believe it is
        'adequate for our needs.
        'However, should the max change in efforts be specified much different to 5% and this test is still to be applied then it
        'changing this value is advisable.
        Const max_percent_change_F = 0.05


        If iTime = OriginalNTimesteps + 1 Then
            Dim FChange As Double
            Dim strmWriter As StreamWriter
            Dim strFile As String = cFileUtils.ToValidFileName("Diagnostics_TestingFishRateNoJumpBegProj.csv", False)
            Dim fileexists As Boolean = File.Exists(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, strFile))
            strmWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.Results, strFile), True)
            If fileexists = False Then
                strmWriter.WriteLine("Date & Time, ModelID, StrategyName, GroupName, FishRateNo@iTime-1, FishRateNo@iTime, %ChangeInFishRateNo(Decimal)")
            End If
            For iGrp = 1 To m_core.nGroups
                'jb round the percent change so it doesn't trip because of rounding error when the value is right on the limit
                FChange = Math.Round((Me._simdata.FishRateNo(iGrp, iTime) - EcosimData.FishRateNo(iGrp, iTime - 1)) / EcosimData.FishRateNo(iGrp, iTime - 1), 4)
                If Math.Abs(FChange) > max_percent_change_F Then
                    strmWriter.WriteLine(DateTime.Now & "," & m_currentModelID & "," & Me.currentStrategy.Name & "," & cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name) & "," & EcosimData.FishRateNo(iGrp, iTime - 1) & "," & Me._simdata.FishRateNo(iGrp, iTime) & "," & FChange)
                    m_PassedChangeInFAtBeginProjTest = False
                End If
            Next
            strmWriter.Close()
            strmWriter.Dispose()
        End If

    End Sub
#End Region

#Region " Dead Code "

#If 0 Then

    'JB 3-April-2014 
    'Code used to create, save and reload a dietmatrix file for debugging

    ''' <summary>
    ''' Reads a known dietmatrix file
    ''' </summary>
    ''' <param name="iTrial"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function readDietMatrix(ByVal iTrial As Integer) As Boolean
        Dim buff As String
        Dim data() As String
        Dim strmReader As StreamReader = cMSEUtils.GetReader(cMSEUtils.MSEFile(DataPath, cMSEUtils.eMSEPaths.ParamsOut, "DietMatrix.csv"))

        If (strmReader IsNot Nothing) Then

            For iprey As Integer = 1 To mCore.nGroups
                buff = strmReader.ReadLine
                data = buff.Split(","c)
                Debug.Assert(data.Length = mCore.nLivingGroups)
                For ipred As Integer = 1 To mCore.nLivingGroups
                    If _ecopath.EcopathData.DC(ipred, iprey) > 0 And cStringUtils.ConvertToSingle(data(ipred - 1)) > 0 Then
                        _ecopath.EcopathData.DC(ipred, iprey) = cStringUtils.ConvertToSingle(data(ipred - 1))
                    End If
                Next ipred

            Next iprey
        End If

        Me.NormalizeDiet()

        cMSEUtils.ReleaseReader(strmReader)
        Me.dumpDietMatrix()

        Return True

    End Function

    ''' <summary>
    ''' Sample the diet matrix
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub updateDietRandom()

        Dim rnd As New Random(666)
        Dim dist As New Troschuetz.Random.NormalDistribution

        For iPred = 1 To Me._pathdata.NumLiving
            If Me._pathdata.PP(iPred) < 1 Then
                For iPrey = 0 To Me._pathdata.NumGroups
                    If Me._pathdata.DC(iPred, iPrey) > 0 Then
                        dist.Mu = Me._pathdata.DC(iPred, iPrey)
                        dist.Sigma = dist.Mu * 0.1
                        Me._pathdata.DC(iPred, iPrey) = CSng(dist.NextDouble)
                    End If
                Next
            End If
        Next

        Me.NormalizeDiet()

        Me.dumpDietMatrix()

    End Sub

    'Commented out because redundant 3-9-13

    'Public Sub Create2DimParams(ByVal ParamName As String)
    '    Dim sPath As String = DataPath & "\DistributionParameters"
    '    Dim csv = New CsvReader(New StreamReader(sPath & "\" & ParamName & ".csv"), True)
    '    Dim ParameterArray(mCore.nLivingGroups * mCore.nLivingGroups, 5) As Single
    '    Dim nIterations As Integer = Convert.ToInt32(MSEForm.txtnTrials.Text)
    '    Dim SampledParameters(nIterations, mCore.nLivingGroups, mCore.nLivingGroups)
    '    Dim eDistributionType As DistributionType

    '    For iGroup = 1 To mCore.nLivingGroups * mCore.nLivingGroups
    '        csv.ReadNextRecord()
    '        For iField = 1 To 5
    '            ParameterArray(iGroup - 1, iField) = csv(iField)
    '        Next
    '    Next

    '    'Generate an array of sample parameters
    '    For iGroup = 1 To mCore.nLivingGroups
    '        For jGroup = 1 To mCore.nLivingGroups
    '            eDistributionType = ParameterArray((iGroup - 1) * mCore.nLivingGroups + jGroup - 1, 1)
    '            For iIteration = 1 To nIterations

    '                Select Case eDistributionType
    '                    Case DistributionType.Uniform
    '                        SampledParameters(iIteration - 1, iGroup, jGroup) = UniformSample(ParameterArray((iGroup - 1) * mCore.nLivingGroups + jGroup - 1, 2), ParameterArray((iGroup - 1) * mCore.nLivingGroups + jGroup - 1, 3))

    '                    Case DistributionType.Triangular
    '                        SampledParameters(iIteration - 1, iGroup, jGroup) = TriangularSample(ParameterArray((iGroup - 1) * mCore.nLivingGroups + jGroup - 1, 2), ParameterArray((iGroup - 1) * mCore.nLivingGroups + jGroup - 1, 3), ParameterArray((iGroup - 1) * mCore.nLivingGroups + jGroup - 1, 4))
    '                End Select

    '            Next
    '        Next
    '    Next

    '    For iIteration = 1 To nIterations
    '        'Output the sampled parameters to a csv
    '        sPath = DataPath & "\ParametersOut"
    '        Dim csvout As New StreamWriter(Path.Combine(sPath & "\" & ParamName & ToString(iIteration) & "out.csv"), True)

    '        For igrp As Integer = 1 To mCore.nLivingGroups
    '            csvout.Write(",""" & mCore.EcoPathGroupInputs(igrp).Name & """")
    '        Next
    '        csvout.WriteLine()

    '        For jGroup = 1 To mCore.nLivingGroups
    '            For iGroup = 1 To mCore.nLivingGroups
    '                csvout.Write("," & SampledParameters(iIteration - 1, iGroup - 1, jGroup - 1))
    '            Next
    '            csvout.WriteLine()
    '        Next

    '        csvout.Dispose()
    '    Next

    'End Sub

    'Private Sub CalculateFError(ByRef eps() As Double)
    '    Dim Fopt(mCore.nGroups - 1) As Double
    '    For iGrp As Integer = 1 To mCore.nLivingGroups
    '        Fopt(iGrp - 1) = 0
    '        For iFleet As Integer 1 to mCore.nFleets
    '            Fopt(iGrp - 1) = Fopt(iGrp - 1) + (mCore.FleetInputs(iFleet).Landings(iGrp) + mCore.FleetInputs(iFleet).Discards(iGrp)) * eps(iGrp - 1)
    '        Next
    '    Next
    'End Sub



    'Private Sub SaveResults2CSV(ByRef iModel As Integer, ByRef FleetEffortTable As DataTable, ByRef FleetCatchTable As DataTable, ByRef GroupResultsTable As DataTable, _
    '                     ByRef TrajectoryTable As DataTable, ByRef HCR_F_Tab As DataTable, ByRef HCR_Quota_Tab As DataTable, _
    '                     ByRef Realised_F_Tab As DataTable, ByRef Realised_Landed_F_Tab As DataTable, ByRef Realised_Discard_F_Tab As DataTable, _
    '                     ByRef CatchTrajTable As DataTable, ByRef HighestValueGroupTab As DataTable, ByRef ChokeGroupTab As DataTable, _
    '                     ByRef swFleetEffort As StreamWriter, ByRef swFleet As StreamWriter, ByRef swGroup As StreamWriter, _
    '                     ByRef swTrajectory As StreamWriter, _
    '                     ByRef swHCR_F_Targ As List(Of StreamWriter), ByRef swHCR_F_Cons As List(Of StreamWriter), _
    '                     ByRef swHCR_Quota_Targ(,) As StreamWriter, ByRef swHCR_Quota_Cons(,) As StreamWriter, _
    '                     ByRef swRealised_F As List(Of StreamWriter), ByRef swRealised_LandedF As List(Of StreamWriter), _
    '                     ByRef swRealised_DiscardF As List(Of StreamWriter), _
    '                     ByRef swLandingsTraj As List(Of StreamWriter), ByRef swDiscardsTraj As List(Of StreamWriter), _
    '                     ByRef swCatchTraj As List(Of StreamWriter), _
    '                     ByRef swLandingsFleetGroupTraj(,) As StreamWriter, ByRef swDiscardsFleetGroupTraj(,) As StreamWriter, _
    '                     ByRef swCatchFleetGroupTraj(,) As StreamWriter, swValueFleetGroupTraj(,) As StreamWriter, _
    '                     ByRef swHighestValueGroup As StreamWriter, ByRef swChokeGroup As StreamWriter)


     Private Sub SaveResults2CSV(ByRef iModel As Integer, ByRef FleetCatchTable As DataTable, ByRef GroupResultsTable As DataTable, _
                     ByRef swFleet As StreamWriter, ByRef swGroup As StreamWriter)


        'Save the FleetCatchTable to CSV
        'swFleet.WriteLine("Model,Strategy,FleetNumber,FleetName,GroupNumber,GroupName,Value")
        For iRow = 0 To FleetCatchTable.Rows.Count - 1
            Dim RowData As Data.DataRow = FleetCatchTable.Rows(iRow)
            swFleet.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", _
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("ModelID")), _
                                cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("FleetNumber")), _
                                cStringUtils.ToCSVField(RowData.Field(Of String)("FleetName")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("GroupNumber")), _
                                cStringUtils.ToCSVField(RowData.Field(Of String)("GroupName")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Single)("CatchAtLastTimeStep")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Single)("DiscardSurvivals")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Single)("DiscardMortalities")))
        Next

        'Save the GroupResultsTable to CSV
        'swGroup.WriteLine("Model,Strategy,ID,Name,ResultName,ResultValue")
        For iRow = 0 To GroupResultsTable.Rows.Count - 1
            Dim RowData As Data.DataRow = GroupResultsTable.Rows(iRow)
            swGroup.WriteLine("{0},{1},{2},{3},{4},{5}", _
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("ModelID")), _
                                cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Integer)("ID")), _
                                cStringUtils.ToCSVField(RowData.Field(Of String)("Name")), _
                                cStringUtils.ToCSVField(RowData.Field(Of String)("ResultName")), _
                                cStringUtils.FormatNumber(RowData.Field(Of Double)("ResultValue")))
        Next


        'Dim TempRow As DataRow
        'Dim TempArrayResultsTarget(m_core.nGroups - 1, NYearsProject - 1) As Double
        'Dim TempArrayResultsConservation(m_core.nGroups - 1, NYearsProject - 1) As Double
        'Dim TempArrayHighestValueGroup(m_core.nFleets - 1, NYearsProject - 1) As String
        'Dim TempArrayChokeGroup(m_core.nFleets - 1, NYearsProject * EcosimData.NumStepsPerYear - 1) As String
        'Dim nTypes As Integer = [Enum].GetNames(GetType(eCatchTypes)).Length
        'Dim TempArrayCatchesTraj(NYearsProject * EcosimData.NumStepsPerYear, m_core.nFleets, m_core.nGroups, nTypes) As Single
        'Dim TempArrayQuotasTraj(m_core.nFleets - 1, m_core.nGroups - 1, NYearsProject - 1) As Double
        'Dim SumLandingsAcrossAllGroups As Single
        'Dim SumDiscardsAcrossAllGroups As Single
        'Dim SumCatchAcrossAllGroups As Single
        'Dim SumValueAcrossAllGroups As Single
        'Const AllGroups As Integer = 0

        ''Save the highest value species
        'For iRow = 1 To HighestValueGroupTab.Rows.Count
        '    TempRow = HighestValueGroupTab.Rows(iRow - 1)
        '    TempArrayHighestValueGroup = TempRow.Field(Of String(,))("Group")

        '    For iFleet = 1 To m_core.nFleets

        '        swHighestValueGroup.Write("{0},{1},{2}", _
        '                                  cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                  cStringUtils.FormatNumber(iModel), _
        '                                  cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")))
        '        For iYear = 1 To NYearsProject
        '            swHighestValueGroup.Write("," & cStringUtils.ToCSVField(TempArrayHighestValueGroup(iFleet - 1, iYear - 1)))
        '        Next
        '        swHighestValueGroup.WriteLine()
        '    Next
        'Next

        ''Save the choke group
        'For iRow = 1 To ChokeGroupTab.Rows.Count
        '    TempRow = ChokeGroupTab.Rows(iRow - 1)
        '    TempArrayChokeGroup = TempRow.Field(Of String(,))("Group")

        '    For iFleet = 1 To m_core.nFleets

        '        swChokeGroup.Write("{0},{1},{2}", _
        '                           cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                           cStringUtils.FormatNumber(iModel), _
        '                           cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")))
        '        For iTimeStep = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swChokeGroup.Write("," & cStringUtils.ToCSVField(TempArrayChokeGroup(iFleet - 1, iTimeStep - 1)))
        '        Next
        '        swChokeGroup.WriteLine()
        '    Next
        'Next


        ''Save the HCR F and Quota trajectories
        'For iGrp = 1 To m_core.nGroups
        '    For iRow = 1 To HCR_F_Tab.Rows.Count
        '        TempRow = HCR_F_Tab.Rows(iRow - 1)

        '        'Output the target F's to file
        '        swHCR_F_Targ(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                   cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                   cStringUtils.FormatNumber(iModel), _
        '                                   cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                   cStringUtils.ToCSVField("Target"))
        '        TempArrayResultsTarget = TempRow.Field(Of Double(,))("Target")
        '        For iYear = 1 To NYearsProject
        '            swHCR_F_Targ(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayResultsTarget(iGrp - 1, iYear - 1)))
        '        Next
        '        swHCR_F_Targ(iGrp - 1).WriteLine()



        '        'Output the conservation F's to file
        '        swHCR_F_Cons(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                   cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                   cStringUtils.FormatNumber(iModel), _
        '                                   cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                   cStringUtils.ToCSVField("Conservation"))
        '        TempArrayResultsConservation = TempRow.Field(Of Double(,))("Conservation")
        '        For iYear = 1 To NYearsProject
        '            swHCR_F_Cons(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayResultsConservation(iGrp - 1, iYear - 1)))
        '        Next
        '        swHCR_F_Cons(iGrp - 1).WriteLine()



        '        For iFleet = 1 To m_core.nFleets

        '            TempRow = HCR_Quota_Tab.Rows(iRow - 1)

        '            'Output the target F's to file
        '            swHCR_Quota_Targ(iFleet - 1, iGrp - 1).Write("{0},{1},{2},{3},{4}", _
        '                                                         cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                         cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                                         cStringUtils.FormatNumber(iModel), _
        '                                                         cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                         cStringUtils.ToCSVField("Target"))

        '            TempArrayQuotasTraj = TempRow.Field(Of Double(,,))("Target")
        '            For iYear = 1 To NYearsProject
        '                swHCR_Quota_Targ(iFleet - 1, iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayQuotasTraj(iFleet - 1, iGrp - 1, iYear - 1)))
        '            Next
        '            swHCR_Quota_Targ(iFleet - 1, iGrp - 1).WriteLine()



        '            'Output the conservation F's to file
        '            swHCR_Quota_Cons(iFleet - 1, iGrp - 1).Write("{0},{1},{2},{3},{4}", _
        '                                                         cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                         cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                                         cStringUtils.FormatNumber(iModel), _
        '                                                         cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                         cStringUtils.ToCSVField("Conservation"))

        '            TempArrayQuotasTraj = TempRow.Field(Of Double(,,))("Conservation")
        '            For iYear = 1 To NYearsProject
        '                swHCR_Quota_Cons(iFleet - 1, iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayQuotasTraj(iFleet - 1, iGrp - 1, iYear - 1)))
        '            Next
        '            swHCR_Quota_Cons(iFleet - 1, iGrp - 1).WriteLine()

        '        Next

        '    Next
        'Next

        ''Save the Realised F trajectories
        'For iGrp = 1 To m_core.nGroups
        '    For iRow = 1 To Realised_F_Tab.Rows.Count
        '        TempRow = Realised_F_Tab.Rows(iRow - 1)

        '        'Output the realised F's to file
        '        swRealised_F(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                   cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                   cStringUtils.FormatNumber(iModel), _
        '                                   cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                   cStringUtils.ToCSVField("TotalF"))
        '        TempArrayResultsTarget = TempRow.Field(Of Double(,))("TotalF")
        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swRealised_F(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayResultsTarget(iGrp - 1, iTime - 1)))
        '        Next
        '        swRealised_F(iGrp - 1).WriteLine()

        '    Next
        'Next


        ''Save the Realised Landed F trajectories
        'For iGrp = 1 To m_core.nGroups
        '    For iRow = 1 To Realised_Landed_F_Tab.Rows.Count
        '        TempRow = Realised_Landed_F_Tab.Rows(iRow - 1)

        '        'Output the realised F's to file

        '        swRealised_LandedF(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                           cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                           cStringUtils.FormatNumber(iModel), _
        '                                           cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                           cStringUtils.ToCSVField("LandedF"))
        '        TempArrayResultsTarget = TempRow.Field(Of Double(,))("LandedF")
        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swRealised_LandedF(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayResultsTarget(iGrp - 1, iTime - 1)))
        '        Next
        '        swRealised_LandedF(iGrp - 1).WriteLine()

        '    Next
        'Next

        ''Save the Realised Discard F trajectories
        'For iGrp = 1 To m_core.nGroups
        '    For iRow = 1 To Realised_Discard_F_Tab.Rows.Count
        '        TempRow = Realised_Discard_F_Tab.Rows(iRow - 1)

        '        'Output the realised F's to file
        '        swRealised_DiscardF(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                            cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                            cStringUtils.FormatNumber(iModel), _
        '                                            cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                            cStringUtils.ToCSVField("DiscardF"))
        '        TempArrayResultsTarget = TempRow.Field(Of Double(,))("DiscardF")
        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swRealised_DiscardF(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayResultsTarget(iGrp - 1, iTime - 1)))
        '        Next
        '        swRealised_DiscardF(iGrp - 1).WriteLine()

        '    Next
        'Next



        ''Save the Catch Trajectories
        'For iGrp = 1 To m_core.nGroups
        '    For iRow = 1 To CatchTrajTable.Rows.Count
        '        TempRow = CatchTrajTable.Rows(iRow - 1)
        '        TempArrayCatchesTraj = TempRow.Field(Of Single(,,,))("Value")

        '        'Output the Landings to file
        '        swLandingsTraj(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                       cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                       cStringUtils.FormatNumber(iModel), _
        '                                       cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                       cStringUtils.ToCSVField("Landings"))
        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swLandingsTraj(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, 0, iGrp, eCatchTypes.Landings)))
        '        Next
        '        swLandingsTraj(iGrp - 1).WriteLine()



        '        'Output the discards
        '        swDiscardsTraj(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                       cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                       cStringUtils.FormatNumber(iModel), _
        '                                       cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                       cStringUtils.ToCSVField("Discards"))
        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swDiscardsTraj(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, 0, iGrp, eCatchTypes.DiscardMortalities) + _
        '                                           TempArrayCatchesTraj(iTime, 0, iGrp, eCatchTypes.DiscardSurvivals)))
        '        Next
        '        swDiscardsTraj(iGrp - 1).WriteLine()



        '        'Output the catches
        '        swCatchTraj(iGrp - 1).Write("{0},{1},{2},{3}", _
        '                                    cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                    cStringUtils.FormatNumber(iModel), _
        '                                    cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                    cStringUtils.ToCSVField("Catch"))
        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            swCatchTraj(iGrp - 1).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, 0, iGrp, eCatchTypes.DiscardMortalities) + _
        '                                           TempArrayCatchesTraj(iTime, 0, iGrp, eCatchTypes.DiscardSurvivals) + _
        '                                           TempArrayCatchesTraj(iTime, 0, iGrp, eCatchTypes.Landings)))
        '        Next
        '        swCatchTraj(iGrp - 1).WriteLine()

        '    Next
        'Next

        'For iRow = 1 To CatchTrajTable.Rows.Count
        '    TempRow = CatchTrajTable.Rows(iRow - 1)
        '    TempArrayCatchesTraj = TempRow.Field(Of Single(,,,))("Value")
        '    For iGrp = 1 To m_core.nGroups
        '        For iFleet = 1 To m_core.nFleets

        '            'Output the value of the landings
        '            swValueFleetGroupTraj(iFleet - 1, iGrp).Write("{0},{1},{2},{3}", _
        '                                                          cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                                          cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                          cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                          cStringUtils.ToCSVField("Value"))

        '            For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '                swValueFleetGroupTraj(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings) * m_core.FleetInputs(iFleet).OffVesselValue(iGrp)))
        '            Next
        '            swValueFleetGroupTraj(iFleet - 1, iGrp).WriteLine()

        '            'Output the Landings to file
        '            swLandingsFleetGroupTraj(iFleet - 1, iGrp).Write("{0},{1},{2},{3},{4}", _
        '                                                             cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                                             cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                             cStringUtils.FormatNumber(iModel), _
        '                                                             cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                             cStringUtils.ToCSVField("Landings"))

        '            For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '                swLandingsFleetGroupTraj(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings)))
        '            Next
        '            swLandingsFleetGroupTraj(iFleet - 1, iGrp).WriteLine()

        '            'Output the Landings to file
        '            swDiscardsFleetGroupTraj(iFleet - 1, iGrp).Write("{0},{1},{2},{3},{4}", _
        '                                                             cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                                             cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                             cStringUtils.FormatNumber(iModel), _
        '                                                             cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                             cStringUtils.ToCSVField("Discards"))
        '            For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '                swDiscardsFleetGroupTraj(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardSurvivals) _
        '                                                                     + TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardMortalities)))
        '            Next
        '            swDiscardsFleetGroupTraj(iFleet - 1, iGrp).WriteLine()



        '            'Output the Landings to file
        '            swCatchFleetGroupTraj(iFleet - 1, iGrp).Write("{0},{1},{2},{3},{4}", _
        '                                                          cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iGrp).Name), _
        '                                                          cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                          cStringUtils.FormatNumber(iModel), _
        '                                                          cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                          cStringUtils.ToCSVField("Catch"))
        '            For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '                swCatchFleetGroupTraj(iFleet - 1, iGrp).Write("," & cStringUtils.FormatNumber(TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardSurvivals) + _
        '                                                                  TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardMortalities) + _
        '                                                                  TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings)))
        '            Next
        '            swCatchFleetGroupTraj(iFleet - 1, iGrp).WriteLine()

        '        Next
        '    Next

        '    For iFleet = 1 To m_core.nFleets

        '        'Output the Landings of all groups to file
        '        swLandingsFleetGroupTraj(iFleet - 1, AllGroups).Write("{0},{1},{2},{3},{4}", _
        '                                                              cStringUtils.ToCSVField("All Groups"), _
        '                                                              cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                              cStringUtils.FormatNumber(iModel), _
        '                                                              cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                              cStringUtils.ToCSVField("Landings"))

        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            SumLandingsAcrossAllGroups = 0
        '            For iGrp = 1 To m_core.nGroups
        '                If TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings) <> -9999 Then
        '                    SumLandingsAcrossAllGroups += TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings)
        '                End If
        '            Next
        '            swLandingsFleetGroupTraj(iFleet - 1, AllGroups).Write("," & cStringUtils.FormatNumber(SumLandingsAcrossAllGroups))
        '        Next
        '        swLandingsFleetGroupTraj(iFleet - 1, AllGroups).WriteLine()


        '        'Output the Discards of all groups to file
        '        swDiscardsFleetGroupTraj(iFleet - 1, AllGroups).Write("{0},{1},{2},{3},{4}", _
        '                                                              cStringUtils.ToCSVField("All Groups"), _
        '                                                              cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                              cStringUtils.FormatNumber(iModel), _
        '                                                              cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                              cStringUtils.ToCSVField("Discards"))

        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            SumDiscardsAcrossAllGroups = 0
        '            For iGrp = 1 To m_core.nGroups
        '                If TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardMortalities) <> -9999 Then
        '                    SumDiscardsAcrossAllGroups += TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardMortalities) +
        '                                                    TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardSurvivals)
        '                End If
        '            Next
        '            swDiscardsFleetGroupTraj(iFleet - 1, AllGroups).Write("," & cStringUtils.FormatNumber(SumDiscardsAcrossAllGroups))
        '        Next
        '        swDiscardsFleetGroupTraj(iFleet - 1, AllGroups).WriteLine()


        '        'Output the Catch of all groups to file
        '        swCatchFleetGroupTraj(iFleet - 1, AllGroups).Write("{0},{1},{2},{3},{4}", _
        '                                                           cStringUtils.ToCSVField("All Groups"), _
        '                                                           cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                           cStringUtils.FormatNumber(iModel), _
        '                                                           cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                           cStringUtils.ToCSVField("Catch"))

        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            SumCatchAcrossAllGroups = 0
        '            For iGrp = 1 To m_core.nGroups
        '                If TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings) <> -9999 Then
        '                    SumCatchAcrossAllGroups += TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardMortalities) + _
        '                                                    TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.DiscardSurvivals) + _
        '                                                    TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings)
        '                End If
        '            Next
        '            swCatchFleetGroupTraj(iFleet - 1, AllGroups).Write("," & cStringUtils.FormatNumber(SumCatchAcrossAllGroups))
        '        Next
        '        swCatchFleetGroupTraj(iFleet - 1, AllGroups).WriteLine()

        '        'Output the value of landings of all groups to file
        '        swValueFleetGroupTraj(iFleet - 1, AllGroups).Write("{0},{1},{2},{3},{4}", _
        '                                                           cStringUtils.ToCSVField("All Groups"), _
        '                                                           cStringUtils.ToCSVField(m_core.FleetInputs(iFleet).Name), _
        '                                                           cStringUtils.FormatNumber(iModel), _
        '                                                           cStringUtils.ToCSVField(TempRow.Field(Of String)("StrategyName")), _
        '                                                           cStringUtils.ToCSVField("Value"))

        '        For iTime = 1 To NYearsProject * EcosimData.NumStepsPerYear
        '            SumValueAcrossAllGroups = 0
        '            For iGrp = 1 To m_core.nGroups
        '                SumValueAcrossAllGroups += TempArrayCatchesTraj(iTime, iFleet, iGrp, eCatchTypes.Landings) * m_core.FleetInputs(iFleet).OffVesselValue(iGrp)
        '            Next
        '            swValueFleetGroupTraj(iFleet - 1, AllGroups).Write("," & cStringUtils.FormatNumber(SumValueAcrossAllGroups))
        '        Next
        '        swValueFleetGroupTraj(iFleet - 1, AllGroups).WriteLine()

        '    Next
        'Next


        ''Save FleetEffortTable to CSV
        'For iRow = 0 To FleetEffortTable.Rows.Count - 1
        '    swFleetEffort.WriteLine()
        '    Dim RowData As Data.DataRow
        '    Dim EffortVals() As Single
        '    RowData = FleetEffortTable.Rows(iRow)
        '    EffortVals = RowData.Field(Of Single())("Effort")
        '    swFleetEffort.Write("{0},{1},{2},{3}", _
        '                        cStringUtils.FormatNumber(RowData.Field(Of Integer)("ModelID")), cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")), _
        '                        cStringUtils.FormatNumber(RowData.Field(Of Integer)("FleetNumber")), cStringUtils.ToCSVField(RowData.Field(Of String)("FleetName")))
        '    For iTimeStep As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
        '        swFleetEffort.Write("," & cStringUtils.FormatNumber(EffortVals(iTimeStep - 1)))
        '    Next
        'Next


        ''Save the trajectories by group
        'For iRow = 0 To TrajectoryTable.Rows.Count - 1
        '    Dim RowData As Data.DataRow
        '    Dim Biomass() As Single
        '    RowData = TrajectoryTable.Rows(iRow)
        '    Biomass = RowData.Field(Of Single())("Biomass")
        '    swTrajectory.Write("{0},{1},{2}", _
        '                        cStringUtils.FormatNumber(RowData.Field(Of Integer)("GroupID")), cStringUtils.ToCSVField(RowData.Field(Of String)("GroupName")), _
        '                        cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")))
        '    For iTimeStep As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
        '        swTrajectory.Write("," & cStringUtils.FormatNumber(Biomass(iTimeStep - 1)))
        '    Next
        '    swTrajectory.WriteLine()
        'Next

        'For iRow = 0 To TrajectoryTable.Rows.Count - 1
        '    Dim RowData As Data.DataRow
        '    Dim Biomass() As Single
        '    RowData = TrajectoryTable.Rows(iRow)
        '    Biomass = RowData.Field(Of Single())("Biomass")
        '    Trajectory2Csv(RowData.Field(Of Integer)("GroupID") - 1).Write(cStringUtils.FormatNumber(RowData.Field(Of Integer)("ModelID")) & "," & cStringUtils.ToCSVField(RowData.Field(Of String)("StrategyName")))
        '    For iTime As Integer = 1 To OriginalNTimesteps + NYearsProject * m_ecosim.EcosimData.NumStepsPerYear
        '        Trajectory2Csv(RowData.Field(Of Integer)("GroupID") - 1).Write("," & cStringUtils.FormatNumber(Biomass(iTime - 1)))
        '    Next
        '    Trajectory2Csv(RowData.Field(Of Integer)("GroupID") - 1).WriteLine()
        'Next


    End Sub

#End If

#End Region ' Dead Code

End Class