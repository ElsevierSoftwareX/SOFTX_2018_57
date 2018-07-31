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
Option Explicit On
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Controls
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' Manager for the Network Analysis
''' </summary>
''' <remarks>This object is used to coordinate running of the Network Analysis and population of the ouput. </remarks>
Public Class cNetworkManager

#Region " Private data "

    Private Enum ePathways
        NotRan = 0
        ''' <summary>TL1->Consumer </summary>
        ToConsumer = 1
        ''' <summary>TL1->Prey->Consumer </summary>
        ToConsumerViaPrey = 2
        ''' <summary>Prey->Top Predator </summary>
        FromPrey = 3
        ''' <summary>Cycles</summary>
        LinkedPathways = 4
        ''' <summary>All Cycles </summary>
        All = 14
    End Enum

    ''' <summary>
    ''' State information for the core set in CoreStateMonitor_CoreExecutionStateEvent(...)
    ''' </summary>
    ''' <remarks>
    ''' This is the state of the core as it relates to the Network Analysis. 
    ''' It is hierarchical
    ''' </remarks>
    Private Enum eRunState As Byte
        CoreNotReady
        NetworkNeedsToRun
        NetworkHasRun
        RequirePPHasRun
        ''' <summary>Ecoism has loaded a scenario. Ecosim network can initialize. </summary>
        EcosimIsLoaded
        ''' <summary>Ecosim network has been initialized. </summary>
        EcosimNetworkInitialized
    End Enum

    Private m_core As cCore = Nothing
    Private m_econetwork As cEcoNetwork = Nothing
    Private m_corestatemonitor As cCoreStateMonitor = Nothing
    Private m_epdata As cEcopathDataStructures = Nothing
    Private m_esdata As cEcosimDatastructures = Nothing
    Private m_messagesource As eCoreComponentType = eCoreComponentType.Plugin
    Private m_runstate As eRunState = eRunState.CoreNotReady
    ''' <summary>Last pathways run state</summary>
    Private m_pathwaystate As ePathways = ePathways.NotRan
    Private m_iPathwayToGroup As Integer = cCore.NULL_VALUE
    Private m_iPathwayViaGroup As Integer = cCore.NULL_VALUE
    Private m_iPathwayFromGroup As Integer = cCore.NULL_VALUE

    ''' <summary>Flag stating whether Ecosim NA should run with Ecosim.</summary>
    Private m_bUseEcosimNetwork As Boolean = False

    ''' <summary>Flag stating whether the main N/A network has ran.</summary>
    Private m_bIsMainNetworkRun As Boolean = False
    ''' <summary>Flag stating whether the Ecosin N/A network has ran.</summary>
    Private m_bIsEcosimNetworkRun As Boolean = False

    ''' <summary><see cref="cMessagePublisher">Core message publisher</see> for
    ''' sending messages through the EwE core system.</summary>
    Private m_publisher As cMessagePublisher = Nothing

#End Region ' Private data

#Region " Construction and initialization "

    Public Sub New(core As cCore)
        m_runstate = eRunState.CoreNotReady
        Me.Init(core)
    End Sub

    Private Function Init(ByRef theCore As cCore) As Boolean

        Me.m_core = theCore
        Me.m_corestatemonitor = theCore.StateMonitor
        Me.m_publisher = theCore.Messages
        Me.m_econetwork = New cEcoNetwork(Me)

        AddHandler Me.m_corestatemonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

        Me.UseAbortTimer = My.Settings.UseAbortTimer
        Me.TimeOutMilSecs = CLng(My.Settings.AbortTimoutMins * 60 * 1000)

        Return True

    End Function

    Friend Sub Clear()
        RemoveHandler Me.m_corestatemonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged
    End Sub

#End Region ' Construction and initialization

#Region " Public Methods for running models "

    Public Event OnRunStateChanged()

    Public Sub StopNetworkAnalysis()
        Me.m_econetwork.bStopNetworkAnnalysis = True
    End Sub

    Private Sub AllowStopNetworkAnalysis()
        Me.m_core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf StopNetworkAnalysis))
    End Sub

#Region " Main Network Analysis "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run the Main Network Analysis routines - if necessary
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' This populates the data for EwE5 'Trophic level decomposition', 
    ''' 'Flow and biomass', 'Mixed Trophic impact', 'Acendency' and 
    ''' 'Flow form detritus' tabs.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Function RunMainNetwork() As Boolean

        Dim bSucces As Boolean = True
        Dim abGroupsToShow(Me.Core.nGroups) As Boolean

        Debug.Assert(Me.m_econetwork IsNot Nothing)

        ' Core not ready? Abort and wait for the world to improve
        If Me.m_runstate = eRunState.CoreNotReady Then Return False

        ' Optimization
        If Me.IsMainNetworkRun = True Then Return True

        ' Forget run states
        Me.m_runstate = eRunState.NetworkNeedsToRun
        Me.m_pathwaystate = ePathways.NotRan

        If (Me.m_econetwork Is Nothing) Then
            'message of some sort
            Me.m_publisher.SendMessage(New cMessage(My.Resources.PROMPT_ERROR_INITIALIZE, _
                                                 eMessageType.ErrorEncountered, m_messagesource, eMessageImportance.Warning))
            bSucces = False
        End If

        'If (Me.MayRunSlow()) Then
        '    Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_NETWORK_COMPLEXDIET, _
        '                                     eCoreComponentType.External, eMessageType.Any, _
        '                                     eMessageImportance.Question, eReplyStyle.YES_NO)
        '    fmsg.Reply = eReply.NO
        '    fmsg.Suppressable = True
        '    Me.m_publisher.SendMessage(fmsg)
        '    If fmsg.Reply <> eReply.YES Then Return False
        'End If

        If bSucces Then
            Try
                For iGroup As Integer = 1 To Me.Core.nGroups
                    abGroupsToShow(iGroup) = True
                    ' JS: group hiding has not yet been enabled
                    'abGroupsToShow(iGroup) = sg.GroupVisible(iGroup)
                Next

                Me.m_runstate = eRunState.NetworkNeedsToRun
                Me.m_econetwork.GroupsToShow = abGroupsToShow
                Me.AllowStopNetworkAnalysis()

                'Make sure the network analysis object has the latest data computed by the core
                'This may not be necessary because m_EcoNetwork keeps a reference to the data. 
                'However, this is more robust, incase the core has created a new m_EcoPathData object.
                Me.m_econetwork.EcopathData = m_epdata
                If Me.m_econetwork.RunNetworkAnalysis() Then

                    Me.m_runstate = eRunState.NetworkHasRun
                    bSucces = True
                    Me.IsMainNetworkRun = True

                Else

                    'This may need a TimedOut State
                    Me.m_runstate = eRunState.NetworkNeedsToRun
                    bSucces = False
                    Me.IsMainNetworkRun = False

                End If

            Catch ex As Exception
                cLog.Write(ex)
                Dim msg As String = cStringUtils.UnravelException(ex)
                ' ToDo: globalize this
                m_publisher.SendMessage(New cMessage("Network analysis RunMainNetwork() error: " & msg, eMessageType.ErrorEncountered, m_messagesource, eMessageImportance.Critical))
                bSucces = False
            End Try
        Else
            ''message of some sort
            Me.m_publisher.SendMessage(New cMessage(My.Resources.PROMPT_ERROR_ECOPATH, _
                                                 eMessageType.StateNotMet, m_messagesource, eMessageImportance.Warning))
            bSucces = False
        End If

        Return bSucces

    End Function

    Public Property IsMainNetworkRun() As Boolean
        Get
            Return Me.m_bIsMainNetworkRun
        End Get
        Set(ByVal value As Boolean)
            If (value <> Me.m_bIsMainNetworkRun) Then
                Me.m_bIsMainNetworkRun = value
                RaiseEvent OnRunStateChanged()
            End If
        End Set
    End Property

#End Region ' Main Network Analysis

#Region " Required PP "

    ''' <summary>
    ''' Run the Require Primary Procuction models - if not already ran.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>This popluates data for the EwE5 tabs 'Primary prod. required'-'For harvest of all groups' and 'For consumption of all groups'</remarks>
    Public Function RunRequiredPrimaryProd() As Boolean

        Dim bSuccess As Boolean = True

        If (Me.IsRequiredPrimaryProdRun = True) Then
            Return bSuccess
        End If

        Debug.Assert(m_econetwork IsNot Nothing)

        If m_econetwork Is Nothing Then
            'message of some sort
            ' ToDo: globalize this
            Core.Messages.SendMessage(New cMessage("Network Analysis not initialized properly.", eMessageType.ErrorEncountered, m_messagesource, eMessageImportance.Critical))
            Return False
        End If

        If m_runstate <> eRunState.CoreNotReady Then
            Try
                'For Primary Prod to run the main network routines need to have run first
                'm_runstate is set by the core's statemonitor events see CoreStateMonitor_CoreExecutionStateEvent()
                If m_runstate < eRunState.NetworkHasRun Then
                    'implicitly run the network analysis if it has not been run
                    If Not Me.RunMainNetwork() Then
                        'ooopssss........
                        ' ToDo: globalize this
                        Core.Messages.SendMessage(New cMessage("Required Primary Production could not be run because of a problem in Network Analysis.", _
                                                                 eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Critical))
                        Return False
                    End If
                End If

                'Debug.Assert(m_runstate = eRunState.NetworkHasRun)

                m_econetwork.CalculateRequiredPP()

                m_runstate = eRunState.RequirePPHasRun

                bSuccess = True
                Me.IsRequiredPrimaryProdRun = True

            Catch ex As Exception
                cLog.Write(ex)
                Dim msg As String = cStringUtils.UnravelException(ex)
                ' ToDo: globalize this
                Core.Messages.SendMessage(New cMessage("Network Analysis run PPR error: " & msg, eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Critical))
                bSuccess = False
            End Try
        Else
            'message of some sort
            ' ToDo: globalize this
            Core.Messages.SendMessage(New cMessage("Required Primary Production can not be run.", eMessageType.StateNotMet, m_messagesource, eMessageImportance.Warning))
            bSuccess = False
        End If

        Return bSuccess

    End Function

    Public Property IsRequiredPrimaryProdRun() As Boolean

#End Region ' Required PP

#Region " Pathways "

    ''' <summary>
    ''' TL1-->Consumer
    ''' </summary>
    ''' <param name="iToGroup"></param>
    ''' <returns></returns>
    Public Function FindPathwaysToConsumer(ByVal iToGroup As Integer) As Boolean

        Dim nPaths As Integer, nArrows As Integer

        ' Optimization
        If (Me.m_pathwaystate = ePathways.ToConsumer) And (Me.m_iPathwayToGroup = iToGroup) Then Return True

        cApplicationStatusNotifier.StartProgress(Me.m_core, _
                                                 cStringUtils.Localize(My.Resources.STATUS_FINDING_PATHWAYS_CONSUMER, Me.GroupName(iToGroup)))
        Try
            Me.AllowStopNetworkAnalysis()
            Me.m_econetwork.FindCycles(m_epdata.DC, ePathways.ToConsumer, iToGroup, 0, nPaths, nArrows)
            Me.m_pathwaystate = ePathways.ToConsumer
            Me.m_iPathwayToGroup = iToGroup
        Catch ex As Exception
            Me.m_pathwaystate = ePathways.NotRan
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.EndProgress(Me.m_core)
        Return True

    End Function

    ''' <summary>
    ''' TL1-->Prey-->Consumer
    ''' </summary>
    ''' <param name="iToGroup"></param>
    ''' <param name="iViaGroup"></param>
    ''' <returns></returns>

    Public Function FindPathwaysToConsumerViaPrey(ByVal iToGroup As Integer, ByVal iViaGroup As Integer) As Boolean

        Dim nPaths As Integer, nArrows As Integer

        ' Optimization
        If (Me.m_pathwaystate = ePathways.ToConsumerViaPrey) And _
           (Me.m_iPathwayToGroup = iToGroup) And _
           (Me.m_iPathwayViaGroup = iViaGroup) Then Return True

        cApplicationStatusNotifier.StartProgress(Me.m_core, _
                                                 cStringUtils.Localize(My.Resources.STATUS_FINDING_PATHWAYS_CONSPREY, _
                                                               Me.GroupName(iToGroup), _
                                                               Me.GroupName(iViaGroup)))

        Try
            Me.AllowStopNetworkAnalysis()
            Me.m_econetwork.FindCycles(m_epdata.DC, ePathways.ToConsumerViaPrey, iToGroup, iViaGroup, nPaths, nArrows)
            Me.m_pathwaystate = ePathways.ToConsumerViaPrey
            Me.m_iPathwayToGroup = iToGroup
            Me.m_iPathwayViaGroup = iViaGroup
        Catch ex As Exception
            Me.m_pathwaystate = ePathways.NotRan
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.EndProgress(Me.m_core)
        Return True

    End Function

    ''' <summary>
    ''' Prey-->Top Predator
    ''' </summary>
    ''' <param name="iFromGroup"></param>
    ''' <returns></returns>
    Public Function FindPathwaysFromPrey(ByVal iFromGroup As Integer) As Boolean

        Dim nPaths As Integer, nArrows As Integer

        ' Optimization
        If (Me.m_pathwaystate = ePathways.FromPrey) And _
           (Me.m_iPathwayFromGroup = iFromGroup) Then Return True

        cApplicationStatusNotifier.StartProgress(Me.m_core, cStringUtils.Localize(My.Resources.STATUS_FINDING_PATHWAYS_PREY, Me.GroupName(iFromGroup)))

        Try
            Me.AllowStopNetworkAnalysis()
            Me.m_econetwork.FindCycles(m_epdata.DC, ePathways.FromPrey, 1, iFromGroup, nPaths, nArrows)
            Me.m_pathwaystate = ePathways.FromPrey
            Me.m_iPathwayFromGroup = iFromGroup
        Catch ex As Exception
            Me.m_pathwaystate = ePathways.NotRan
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.EndProgress(Me.m_core)
        Return True

    End Function

    ''' <summary>
    ''' Cycles(excl. detitus)
    ''' </summary>
    ''' <returns></returns>
    Public Function FindPathwaysCycles() As Boolean

        Dim nPaths As Integer, nArrows As Integer

        ' Optimization
        If (Me.m_pathwaystate = ePathways.LinkedPathways) Then Return True

        cApplicationStatusNotifier.StartProgress(Me.m_core, My.Resources.STATUS_FINDING_PATHWAYS)

        Try
            'ToDo_jb FindPathwaysCycles EwE5 calls InitCyclesList ????? I can not find this again
            Me.AllowStopNetworkAnalysis()
            Me.m_econetwork.FindCycles(m_epdata.DC, ePathways.LinkedPathways, 1, 1, nPaths, nArrows)
            Me.m_pathwaystate = ePathways.LinkedPathways
        Catch ex As Exception
            Me.m_pathwaystate = ePathways.NotRan
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.EndProgress(Me.m_core)
        Return True

    End Function

    ''' <summary>
    ''' All cycles
    ''' </summary>
    ''' <returns></returns>
    Public Function FindPathwaysCyclesAll() As Boolean

        Dim nPaths As Integer, nArrows As Integer

        ' Optimization
        If (Me.m_pathwaystate = ePathways.All) Then Return True

        cApplicationStatusNotifier.StartProgress(Me.m_core, My.Resources.STATUS_FINDING_PATHWAYS, -1)

        Try
            Me.AllowStopNetworkAnalysis()
            Me.m_econetwork.FindCycles(m_epdata.DC, ePathways.All, 1, 1, nPaths, nArrows)
            Me.m_pathwaystate = ePathways.All
        Catch ex As Exception
            Me.m_pathwaystate = ePathways.NotRan
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.EndProgress(Me.m_core)
        Return True

    End Function

#End Region ' Pathways

#Region " Network from Ecosim "

    ''' <summary>
    ''' Run Ecosim and compute the ecosim network analysis data - if not already ran.
    ''' </summary>
    Public Function RunEcosimNetwork() As Boolean

        Try

            If Not Me.m_bUseEcosimNetwork Then Return False

            If Not Core.StateMonitor.HasEcosimLoaded Then
                'No Ecosim Scenario is loaded the Ecosim network analysis can not be run
                Core.Messages.SendMessage(New cMessage(My.Resources.PROMPT_LOAD_ECOSIM,
                         eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Warning))

                Return False
            End If

            If Me.IsEcosimNetworkRun Then Return True

            cApplicationStatusNotifier.StartProgress(Me.m_core, My.Resources.STATUS_RUNNING_NETWORKANALYSIS)
            Me.IsEcosimNetworkRun = Me.Core.RunEcoSim()
            cApplicationStatusNotifier.EndProgress(Me.m_core)

        Catch ex As Exception
            cLog.Write(ex)
            Core.Messages.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.PROMPT_ERROR_ECOSIM, ex.Message),
                                                   eMessageType.ErrorEncountered,
                                                   eCoreComponentType.Plugin, eMessageImportance.Critical))
            Return False
        End Try

        Return True

    End Function

    ''' <summary>
    ''' Initialize Ecosim Network Analysis
    ''' </summary>
    ''' <returns></returns>
    Public Function InitNetworkForEcosim() As Boolean

        Try

            'don't do anything if the Ecosim Network Analysis is turned off
            If Not Me.m_bUseEcosimNetwork Then
                Return False
            End If

            'If m_runstate < eRunState.EcosimIsLoaded Then
            If Not Core.StateMonitor.HasEcosimLoaded Then
                'No Ecosim Scenario is loaded this can not be initialized
                Core.Messages.SendMessage(New cMessage(My.Resources.PROMPT_LOAD_ECOSIM, _
                         eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Information))
                Return False
            End If

            'm_runstate is set by the core's statemonitor events see CoreStateMonitor_CoreExecutionStateEvent()
            If m_runstate < eRunState.NetworkHasRun Then
                'implicitly run the network analysis if it has not been run
                If Not Me.RunMainNetwork() Then
                    'ooopssss........
                    Core.Messages.SendMessage(New cMessage(My.Resources.PROMPT_ERROR_ECOSIM_INIT, _
                                                           eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Critical))
                    Return False
                End If
            End If

            m_econetwork.EcopathData = m_epdata
            m_econetwork.EcosimData = m_esdata

            m_econetwork.InitForEcosim()
            m_runstate = eRunState.EcosimNetworkInitialized


        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".InitNetworkForEcosim " & ex.Message)
            Return False
        End Try
        Return True

    End Function

    ''' <summary>
    ''' Compute network analysis for ecosim at this time step
    ''' </summary>
    ''' <param name="BiomassAtTimestep"></param>
    ''' <param name="EcosimDatastructures"></param>
    ''' <param name="iTime"></param>
    ''' <returns></returns>
    Public Function EcosimTimeStep(ByRef BiomassAtTimestep() As Single, ByVal EcosimDatastructures As cEcosimDatastructures, ByVal iTime As Integer) As Boolean

        Dim bSucces As Boolean = True
        Try

            'don't do anything if the Ecosim Network Analysis is turned off
            If Not Me.UseEcosimNetwork Then
                Return False
            End If

            If m_runstate < eRunState.EcosimNetworkInitialized Then
                'do not try to run this if it has not been initialized
                'no messages here so that this does not slow down Ecosim
                Return False
            End If

            'do ecosim network calculation for this time step
            m_econetwork.EcosimTimestep(BiomassAtTimestep, EcosimDatastructures, iTime)
            'tell the world that a time step has been computed
            Me.UpdateProgress(My.Resources.STATUS_RUNNING_NETWORKANALYSIS, CSng(iTime / Me.m_esdata.NTimes))

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.ToString)
            bSucces = False
        End Try

        Return bSucces

    End Function

#End Region ' Network from Ecosim

#End Region ' Public Methods for running models

#Region " Public Properties "

#Region " Settings "

    ''' <summary>
    ''' Boolean flag to run Network Analysis automatically when an Ecopath run has completed.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>
    ''' WARNING this may not function correctly.
    '''  Call cNetworkManager.RunMainNetwork() explicity to run the Network Analysis after an Ecopath run.
    '''  The internal run state flag m_runstate maybe set to eRunState.CoreNotReady preventing the EcopathRunCompleted plugin point from working.
    ''' </remarks>
    Public Property RunWithEcopath() As Boolean
    Public Property RunWithEcosim() As Boolean

    ''' <summary>Use the Abort Timer to abort a run after <see cref="TimeOutMilSecs">time out in Milliseconds</see></summary>
    ''' <remarks>
    ''' False by default. The AbortTimer works in the Scientific interface but needs an interface to turn it on/off and set the TimeOutMilSecs. 
    ''' At this time this can only be used from code.
    ''' </remarks>
    Public Property UseAbortTimer As Boolean
        Get
            Return Me.m_econetwork.bUseAbortTimer
        End Get
        Set(ByVal value As Boolean)
            Me.m_econetwork.bUseAbortTimer = value
        End Set
    End Property

    ''' <summary>
    ''' Number of milliseconds to wait for the Network Analysis to complete before it times out.
    ''' </summary>
    ''' <remarks>This is only effective if <see cref="UseAbortTimer"/> = True. Default of 30 minutes</remarks> 
    Public Property TimeOutMilSecs As Long
        Get
            Return Me.m_econetwork.TimeOutMilSecs
        End Get
        Set(ByVal value As Long)
            Me.m_econetwork.TimeOutMilSecs = value
        End Set
    End Property

    Public ReadOnly Property IsTimedOut As Boolean
        Get
            Return Me.m_econetwork.IsTimedOut
        End Get
    End Property

#End Region ' Settings

#Region " Inputs "

    ''' <summary>
    ''' Ecopath data to run the analysis on
    ''' </summary>
    ''' <remarks>This is set by plugin (cEwENetworkAnalysisPlugin) each time the core fires the EcopathRunCompleted() Plugin point.</remarks>
    Public Property EcopathData() As cEcopathDataStructures
        Get
            Return m_epdata
        End Get
        Set(ByVal value As cEcopathDataStructures)
            m_epdata = value
        End Set
    End Property

    ''' <summary>
    ''' Ecopath data to run the analysis on
    ''' </summary>
    Public Property EcosimData() As cEcosimDatastructures
        Get
            Return m_esdata
        End Get
        Set(ByVal value As cEcosimDatastructures)
            m_esdata = value
        End Set
    End Property

    ''' <summary>
    ''' Get/set whether the Network Analysis plug-in should run with Ecosim.
    ''' </summary>
    ''' <remarks>
    ''' Was flag 'IndicesOn' in EwE5. If this flag is set to False, the plugin 
    ''' will not respond to the EcosimEndTimeStep() plugin point.
    ''' </remarks>
    Public Property UseEcosimNetwork() As Boolean
        Get
            Return Me.m_bUseEcosimNetwork
        End Get
        Set(ByVal value As Boolean)
            Me.m_bUseEcosimNetwork = value
        End Set
    End Property

    ''' <summary>
    ''' Get/set whether network analysis for Ecosim has ran.
    ''' </summary>
    Public Property IsEcosimNetworkRun() As Boolean
        Get
            Return m_bIsEcosimNetworkRun
        End Get
        Set(ByVal value As Boolean)
            If (value <> Me.m_bIsEcosimNetworkRun) Then
                Me.m_bIsEcosimNetworkRun = value
                RaiseEvent OnRunStateChanged()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Run the Required Primary Production routines for ecosim
    ''' </summary>
    ''' <returns>True </returns>
    ''' <remarks>This is very time consuming</remarks>
    Public Property EcosimPPROn() As Boolean
        Get
            Return Me.m_econetwork.PPRon
        End Get
        Set(ByVal value As Boolean)
            If (value <> Me.m_econetwork.PPRon) Then
                ' Update flag
                Me.m_econetwork.PPRon = value
                ' Void ecosim run
                Me.IsEcosimNetworkRun = False
            End If
        End Set
    End Property

    Public ReadOnly Property Core() As cCore
        Get
            Return Me.m_core
        End Get
    End Property

#End Region ' Inputs

#Region " Model outputs "

#Region " Counters "

    Public ReadOnly Property nTrophicLevels() As Integer
        Get
            Return Me.m_econetwork.NoTL
        End Get
    End Property

    Public ReadOnly Property nGroups() As Integer
        Get
            Return Me.Core.nGroups
        End Get
    End Property

    Public ReadOnly Property nLivingGroups() As Integer
        Get
            Return Me.Core.nLivingGroups
        End Get
    End Property

    Public ReadOnly Property nDetritusGroups() As Integer
        Get
            Return Me.Core.nDetritusGroups
        End Get
    End Property

    Public ReadOnly Property GroupName(ByVal iGroup As Integer) As String
        Get
            Try
                Return Me.m_epdata.GroupName(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return ""
            End Try
        End Get
    End Property

    Public ReadOnly Property nFleets() As Integer
        Get
            Return Me.Core.nFleets
        End Get
    End Property

    Public ReadOnly Property FleetName(ByVal iFleet As Integer) As String
        Get
            Try
                Return Me.m_epdata.FleetName(iFleet)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return ""
            End Try
        End Get
    End Property

#End Region ' Counters

#Region " Pathways "

    ''' <summary>
    ''' EwE5 Cycles and Pathways
    ''' </summary>
    ''' <remarks>PathWays will contain new data on each call to FindPathwaysxxxxxx</remarks>
    Public ReadOnly Property PathWays() As List(Of String)
        Get
            Return Me.m_econetwork.lstPathways
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Cycles and Pathways
    ''' </summary>

    Public ReadOnly Property NumArrows() As Integer
        Get
            Return Me.m_econetwork.NumberArrows
        End Get
    End Property

#End Region ' Pathways

#Region " Flows "

    ''' <summary>
    ''' EwE5 Trophic level decomposition Relative Flows
    ''' </summary>
    Public ReadOnly Property RelativeFlow(ByVal iGroup As Integer, ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.AM(iTrophicLevel, iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Absolute Flows
    ''' </summary>
    Public ReadOnly Property AbsoluteFlow(ByVal iGroup As Integer, ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.AM_Abs(iTrophicLevel, iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Sum of Absolute Flows across all the groups for a trophic level
    ''' </summary>
    Public ReadOnly Property AbsoluteFlowTotal(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.QTL(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Used in computing Transfer Effeiciency
    ''' </summary>
    Public ReadOnly Property CA(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.CA(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Used in computing Transfer Effeiciency
    ''' </summary>
    Public ReadOnly Property CatchDetritus(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.CAD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Used in computing Transfer Effeiciency
    ''' </summary>
    Public ReadOnly Property FlowFromDetritus() As Single
        Get
            Return m_econetwork.DetIndex
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Biomass by Trophic Level
    ''' </summary>
    Public ReadOnly Property BiomassByTrophicLevel(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.BbyTL(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Biomass by Group
    ''' </summary>
    Public ReadOnly Property BiomassByGroup(ByVal iGroupNum As Integer) As Single
        Get
            Return m_epdata.B(iGroupNum)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition detritus by Trophic Level
    ''' </summary>
    Public ReadOnly Property DetritusByTrophicLevel(ByVal iTrophicLevel As Integer) As Single
        Get
            Dim sMass As Single = 0
            If iTrophicLevel = 1 Then
                For i As Integer = Me.nLivingGroups + 1 To Me.nGroups
                    sMass += Me.BiomassByGroup(i)
                Next
            Else
                If Me.PPToDetritus(iTrophicLevel) > 0 Then
                    sMass = (Me.BiomassByTrophicLevel(iTrophicLevel) * Me.DetToDetritus(iTrophicLevel)) / _
                            (Me.PPToDetritus(iTrophicLevel) + Me.DetToDetritus(iTrophicLevel))
                End If
            End If
            Return sMass
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Catch by Trophic Level
    ''' </summary>
    Public ReadOnly Property CatchByTrophicLevel(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.CbyTL(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Trophic level decomposition Catch by Group
    ''' </summary>
    Public ReadOnly Property CatchByGroup(ByVal iGroupNum As Integer) As Single
        Get
            Return m_epdata.fCatch(iGroupNum)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Mixed Trophic impact
    ''' </summary>
    Public ReadOnly Property MixedTrophicImpacts(ByVal iPred As Integer, ByVal iPrey As Integer) As Single
        Get
            Return m_econetwork.MTI(iPred, iPrey)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Flow from detritus
    ''' </summary>
    Public ReadOnly Property FlowFromDetritus(ByVal iGroup As Integer) As Single
        Get
            Dim sumad As Single
            For itl As Integer = 1 To m_econetwork.NoTL
                sumad += m_econetwork.Ad(itl, iGroup)
            Next
            Return sumad
        End Get
    End Property

    Public ReadOnly Property DetTransferEfficiency(ByVal iTrophicLevel As Integer) As Single
        Get
            If Me.DetThroughtput(iTrophicLevel) > 0.001 Then
                Return (Me.CatchDetritus(iTrophicLevel) + Me.DetConsByPred(iTrophicLevel)) / Me.DetThroughtput(iTrophicLevel)
            End If
            Return 0
        End Get
    End Property

    Public ReadOnly Property PPTransferEfficiency(ByVal iTrophicLevel As Integer) As Single
        Get
            If Me.PPThroughtput(iTrophicLevel) > 0.001 Then
                Return (Me.CA(iTrophicLevel) + Me.PPConsByPred(iTrophicLevel)) / Me.PPThroughtput(iTrophicLevel)
            End If
            Return 0
        End Get
    End Property

    Public ReadOnly Property TotTransferEfficiency(ByVal iTrophicLevel As Integer) As Single
        Get
            Dim sTotThroughput As Single = (Me.DetThroughtput(iTrophicLevel) + Me.PPThroughtput(iTrophicLevel))
            If sTotThroughput > 0 Then
                Return (Me.CatchDetritus(iTrophicLevel) + _
                        Me.CA(iTrophicLevel) + _
                        Me.DetConsByPred(iTrophicLevel) + _
                        Me.PPConsByPred(iTrophicLevel)) / sTotThroughput
            End If
            Return 0
        End Get
    End Property

#End Region ' Flows

#Region " Ascendancy "

#Region "By Group"

    Public ReadOnly Property AscendancyByGroup(ByVal iGroup As Integer) As Single
        Get
            Return m_econetwork.Ac(iGroup)
        End Get
    End Property

    Public ReadOnly Property OverheadByGroup(ByVal iGroup As Integer) As Single
        Get
            Return m_econetwork.Ec(iGroup)
        End Get
    End Property

    Public ReadOnly Property CapacityByGroup(ByVal iGroup As Integer) As Single
        Get
            Return m_econetwork.CC(iGroup)
        End Get
    End Property

    Public ReadOnly Property InformationByGroup(ByVal iGroup As Integer) As Single
        Get
            If m_econetwork.TruPut > 0 Then
                Return m_econetwork.Ac(iGroup) / m_econetwork.TruPut
            Else
                Return cCore.NULL_VALUE
            End If
        End Get
    End Property

    Public ReadOnly Property ThroughputByGroup(ByVal iGroup As Integer) As Single
        Get
            Return CSng(m_econetwork.Q(iGroup))
        End Get
    End Property

    Public ReadOnly Property AscendencyTotal() As Single
        Get
            Return m_econetwork.SumAc
        End Get
    End Property

    Public ReadOnly Property OverheadTotal() As Single
        Get
            Return m_econetwork.SumEc
        End Get
    End Property

    Public ReadOnly Property CapacityTotal() As Single
        Get
            Return m_econetwork.SumCc
        End Get
    End Property

    Public ReadOnly Property ThroughputTotal() As Single
        Get
            Return m_econetwork.TruPut
        End Get
    End Property

    Public ReadOnly Property ThroughputCycledLiving() As Single
        Get
            Return m_econetwork.Tc
        End Get
    End Property

    Public ReadOnly Property ThroughputCycledPredatory() As Single
        Get
            Return m_econetwork.TCyc
        End Get
    End Property

    Public ReadOnly Property ThroughputCycledAll() As Single
        Get
            Return m_econetwork.TcD
        End Get
    End Property

    Public ReadOnly Property ThroughputExport() As Single
        Get
            Return m_econetwork.SumEx
        End Get
    End Property

    Public ReadOnly Property ThroughputResp() As Single
        Get
            Return m_econetwork.SumResp
        End Get
    End Property

    Public ReadOnly Property ThroughputExportByGroup(ByVal iGroup As Integer) As Single
        Get
            Return Me.m_epdata.Ex(iGroup)
        End Get
    End Property
#End Region

#Region "Totals"

#Region "Ascendancy Flow"

    ''' <summary>
    ''' Ascendency total flow
    ''' </summary>
    Public ReadOnly Property AscendancyInternalFlowTotal() As Single
        Get
            Return m_econetwork.Ai
        End Get
    End Property

    ''' <summary>
    ''' Ascendency percentage flow
    ''' </summary>
    Public ReadOnly Property AscendancyInternalFlowPer() As Single
        Get
            Return m_econetwork.Aip
        End Get
    End Property


    ''' <summary>
    ''' Ascendency total import
    ''' </summary>
    Public ReadOnly Property AscendancyImportTotal() As Single
        Get
            Return m_econetwork.Ao
        End Get
    End Property

    ''' <summary>
    ''' Ascendency percentage import
    ''' </summary>
    Public ReadOnly Property AscendancyImportPer() As Single
        Get
            Return m_econetwork.Aop
        End Get
    End Property


    ''' <summary>
    ''' Ascendency total export
    ''' </summary>
    Public ReadOnly Property AscendancyExportTotal() As Single
        Get
            Return m_econetwork.Ae
        End Get
    End Property

    ''' <summary>
    ''' Ascendency percentage export
    ''' </summary>
    Public ReadOnly Property AscendancyExportPer() As Single
        Get
            Return m_econetwork.Aep
        End Get
    End Property

    ''' <summary>
    ''' Ascendency total respiration
    ''' </summary>
    Public ReadOnly Property AscendancyRespTotal() As Single
        Get
            Return m_econetwork.Ar
        End Get
    End Property

    ''' <summary>
    ''' Ascendency percentage respiration
    ''' </summary>
    Public ReadOnly Property AscendancyRespPer() As Single
        Get
            Return m_econetwork.Arp
        End Get
    End Property

    ''' <summary>
    ''' Ascendency total totals
    ''' </summary>
    Public ReadOnly Property AscendancyTotalsTotal() As Single
        Get
            Return m_econetwork.Ascen
        End Get
    End Property

    ''' <summary>
    ''' Ascendency percentage totals
    ''' </summary>
    Public ReadOnly Property AscendancyTotalsPer() As Single
        Get
            Return m_econetwork.Ascp
        End Get
    End Property

#End Region

#Region "Overhead"

    ''' <summary>
    ''' Overhead flow total 
    ''' </summary>
    Public ReadOnly Property OverheadFlowTotal() As Single
        Get
            Return m_econetwork.Ei
        End Get
    End Property

    ''' <summary>
    ''' Overhead Flow percentage 
    ''' </summary>
    Public ReadOnly Property OverheadFlowPer() As Single
        Get
            Return m_econetwork.Eip
        End Get
    End Property


    ''' <summary>
    ''' Overhead total import
    ''' </summary>
    Public ReadOnly Property OverheadImportTotal() As Single
        Get
            Return m_econetwork.Eo
        End Get
    End Property

    ''' <summary>
    ''' Overhead percentage import
    ''' </summary>
    Public ReadOnly Property OverheadImportPer() As Single
        Get
            Return m_econetwork.Eop
        End Get
    End Property

    ''' <summary>
    ''' Overhead  Export total 
    ''' </summary>
    Public ReadOnly Property OverheadExportTotal() As Single
        Get
            Return m_econetwork.Eee
        End Get
    End Property

    ''' <summary>
    ''' Overhead Export percentage 
    ''' </summary>
    Public ReadOnly Property OverheadExportPer() As Single
        Get
            Return m_econetwork.Eep
        End Get
    End Property

    ''' <summary>
    ''' Overhead respiration  total 
    ''' </summary>
    Public ReadOnly Property OverheadRespTotal() As Single
        Get
            Return m_econetwork.er
        End Get
    End Property

    ''' <summary>
    ''' Overhead respiration percentage 
    ''' </summary>
    Public ReadOnly Property OverheadRespPer() As Single
        Get
            Return m_econetwork.Erp
        End Get
    End Property

    ''' <summary>
    ''' Overhead totals total 
    ''' </summary>
    Public ReadOnly Property OverheadTotalsTotal() As Single
        Get
            Return m_econetwork.Overhead
        End Get
    End Property

    ''' <summary>
    ''' Overhead totals percentage 
    ''' </summary>
    Public ReadOnly Property OverheadTotalsPer() As Single
        Get
            Return m_econetwork.Overp
        End Get
    End Property

#End Region

#Region "Capacity"

    ''' <summary>
    ''' Capacity  flow percentage 
    ''' </summary>
    Public ReadOnly Property CapacityFlowTotal() As Single
        Get
            Return m_econetwork.Ci
        End Get
    End Property

    ''' <summary>
    ''' Capacity flow percentage 
    ''' </summary>
    Public ReadOnly Property CapacityFlowPer() As Single
        Get
            Return m_econetwork.Cip
        End Get
    End Property


    ''' <summary>
    ''' Capacity total import
    ''' </summary>
    Public ReadOnly Property CapacityImportTotal() As Single
        Get
            Return m_econetwork.Co
        End Get
    End Property

    ''' <summary>
    ''' Capacity percentage import
    ''' </summary>
    Public ReadOnly Property CapacityImportPer() As Single
        Get
            Return m_econetwork.Cop
        End Get
    End Property

    ''' <summary>
    ''' Capacity export total
    ''' </summary>
    Public ReadOnly Property CapacityExportTotal() As Single
        Get
            Return m_econetwork.Ce
        End Get
    End Property

    ''' <summary>
    ''' Capacity export precentage
    ''' </summary>
    Public ReadOnly Property CapacityExportPer() As Single
        Get
            Return m_econetwork.Cep
        End Get
    End Property

    ''' <summary>
    ''' Capacity respiration total
    ''' </summary>
    Public ReadOnly Property CapacityRespTotal() As Single
        Get
            Return m_econetwork.Cr
        End Get
    End Property

    ''' <summary>
    ''' Capacity respiration precentage
    ''' </summary>
    Public ReadOnly Property CapacityRespPer() As Single
        Get
            Return m_econetwork.Crp
        End Get
    End Property

    ''' <summary>
    ''' Capacity respiration precentage
    ''' </summary>
    Public ReadOnly Property CapacityTotalsTotal() As Single
        Get
            Return m_econetwork.Capacity
        End Get
    End Property

    ''' <summary>
    ''' Capacity respiration precentage
    ''' </summary>
    Public ReadOnly Property CapacityTotalsPer() As Single
        Get
            Return m_econetwork.Capp
        End Get
    End Property

#End Region

#End Region

#End Region ' Ascendancy

#Region " Trophic Level "

    ''' <summary>
    ''' Flow and Biomass From primary prod. Import 
    ''' </summary>
    Public ReadOnly Property PPImport(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.Impo(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Cons by Pred 
    ''' </summary>
    Public ReadOnly Property PPConsByPred(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.Predat(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Export
    ''' </summary>
    Public ReadOnly Property PPExport(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.EXA(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Flow To Detritus
    ''' </summary>
    Public ReadOnly Property PPToDetritus(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.DTA(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Respiration
    ''' </summary>
    Public ReadOnly Property PPRespiration(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.RSP(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Throughtput
    ''' </summary>
    Public ReadOnly Property PPThroughtput(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.TRP(iTrophicLevel)
        End Get
    End Property

    Public ReadOnly Property PPThroughtputSum() As Single
        Get
            Dim sSum As Single = 0
            For i As Integer = 1 To Me.nTrophicLevels
                sSum += Me.PPThroughtput(i)
            Next
            Return sSum
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Import 
    ''' </summary>
    Public ReadOnly Property DetImport(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.ImpD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Cons by Pred 
    ''' </summary>
    Public ReadOnly Property DetConsByPred(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.PredatD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Export
    ''' </summary>
    Public ReadOnly Property DetExport(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.EXAD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Flow To Detritus
    ''' </summary>
    Public ReadOnly Property DetToDetritus(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.DTAD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Respiration
    ''' </summary>
    Public ReadOnly Property DetRespiration(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.RSPD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Throughtput
    ''' </summary>
    Public ReadOnly Property DetThroughtput(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.TRPD(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From detritus. Throughtput sum
    ''' </summary>
    Public ReadOnly Property DetThroughtputSum() As Single
        Get
            Dim sSum As Single = 0
            For iTrophicLevel As Integer = 1 To Me.nTrophicLevels
                sSum += Me.DetThroughtput(iTrophicLevel)
            Next
            Return sSum
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Throughtput shown
    ''' </summary>
    Public ReadOnly Property ThroughtputShow(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.TrpShow(iTrophicLevel)
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From primary prod. Throughtput shown
    ''' </summary>
    Public Property TrEm1(ByVal iTrophicLevel As Integer) As Single
        Get
            Return m_econetwork.TrEm1(iTrophicLevel)
        End Get
        Set(ByVal value As Single)
            m_econetwork.TrEm1(iTrophicLevel) = value
        End Set
    End Property

    ''' <summary>
    ''' Flow and Biomass From all combined. Extracted to break cycles
    ''' </summary>
    Public ReadOnly Property ExtractedToBreakCycles() As Single
        Get
            Return m_econetwork.AmCyc
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From all combined. Input TLII+
    ''' </summary>
    Public ReadOnly Property InputTLIIPlus() As Single
        Get
            Return m_econetwork.SumIm
        End Get
    End Property

    ''' <summary>
    ''' Flow and Biomass From all combined. Total throughput
    ''' </summary>
    Public ReadOnly Property TotalThroughput() As Single
        Get
            Return m_econetwork.TotalTrp
        End Get
    End Property

#End Region ' Trophic Level

#Region " Indicators "

    Public ReadOnly Property Electivity(ByVal iSel As Integer, ByVal iPrey As Integer, ByVal iTime As Integer) As Single
        Get
            Return Me.EcosimData.Elect(iSel, iPrey, iTime)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the L-index for a fished group.
    ''' </summary>
    ''' <param name="iGroup">The group index to retrieve the L-index for.</param>
    ''' <remarks>If <paramref name="iGroup"/> does not refer to a fished group
    ''' 0 will be returned.</remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Lindex(ByVal iGroup As Integer) As Single
        Get
            If Me.PPRCatchHarvest(iGroup) <= 0.0 Or Me.PPRCatchHarvest(iGroup) <= 0.0 Then Return 0

            Dim TE2 As Single = Me.TotTransferEfficiency(2)
            Dim TE3 As Single = Me.TotTransferEfficiency(3)
            Dim TE4 As Single = Me.TotTransferEfficiency(4)
            Dim TE As Single = CSng((TE2 * TE3 * TE4) ^ (1 / 3)) ' OK, NOT expressed in percent
            Dim PPRi As Single = Me.PPRTotPPHarvest(iGroup)  ' OK, expressed in percent

            ' Loss of Prod for fn group: -PPRi%*TE^(TLi-1) / ln(TE)
            Return CSng(-PPRi * TE ^ (Me.m_epdata.TTLX(iGroup) - 1) / Math.Log(TE))
        End Get
    End Property

    Public ReadOnly Property LindexSim(ByVal iGroup As Integer) As Single
        Get
            If Me.PPRCatchHarvest(iGroup) <= 0.0 Or Me.PPRCatchHarvest(iGroup) <= 0.0 Then Return 0

            ' TotTransferEfficiency is computed for Ecosim, *phew*
            Dim TE2 As Single = Me.TotTransferEfficiency(2)
            Dim TE3 As Single = Me.TotTransferEfficiency(3)
            Dim TE4 As Single = Me.TotTransferEfficiency(4)
            Dim TE As Single = CSng((TE2 * TE3 * TE4) ^ (1 / 3)) ' OK, NOT expressed in percent
            Dim PPRi As Single = Me.PPRTotPPHarvest(iGroup)  ' OK, expressed in percent

            ' Loss of Prod for fn group: -PPRi%*TE^(TLi-1) / ln(TE)
            Return CSng(-PPRi * TE ^ (Me.m_esdata.TLSim(iGroup) - 1) / Math.Log(TE))
        End Get
    End Property

    Public ReadOnly Property Psust(ByVal iGroup As Integer) As Single
        Get
            Return CalcPsust(Lindex(iGroup))
        End Get
    End Property

    'Public ReadOnly Property PsustSDupper(iGroup As Integer) As Single
    '    Get
    '        Return CalcPsustSDupper(Lindex(iGroup))
    '    End Get
    'End Property

    'Public ReadOnly Property PsustSDlower(iGroup As Integer) As Single
    '    Get
    '        Return CalcPsustSDlower(Lindex(iGroup))
    '    End Get
    'End Property

    ''' <summary>
    ''' Calculate P-sust (probability percentage of sustainable fishing) from an L-Index value
    ''' </summary>
    ''' <param name="LIndex">The L-Index value to calculate P-sust from.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' From Marta Coll / Simone Libralato
    ''' </remarks>
    Public Function CalcPsust(ByVal LIndex As Single) As Single
        'Return CSng(Math.Max(0, Math.Min(1, 1 - lIndex / 0.18)))
        Return CSng(-238674 * LIndex ^ 6 + 190305 * LIndex ^ 5 - 57326 * LIndex ^ 4 + 7916.6 * LIndex ^ 3 - 447.24 * LIndex ^ 2 - 1.5725 * LIndex + 0.9686)
    End Function

    ' JS: commented-out until methodology is better established by MC + SL

    ' ''' <summary>
    ' ''' Calculate P-sust upper SD from an L-Index value
    ' ''' </summary>
    ' ''' <param name="LIndex">The L-Index value to calculate P-sust SD upper from.</param>
    ' ''' <returns></returns>
    ' ''' <remarks>
    ' ''' From Marta Coll / Simone Libralato
    ' ''' </remarks>
    'Private Function CalcPsustSDupper(LIndex As Single) As Single
    '    Return CSng(1000000.0 * LIndex ^ 6 - 574602 * LIndex ^ 5 + 92861 * LIndex ^ 4 - 4778.1 * LIndex ^ 3 - 60.762 * LIndex ^ 2 - 1.31 * LIndex + 1.0066)
    'End Function

    ' ''' <summary>
    ' ''' Calculate P-sust lower SD from an L-Index value
    ' ''' </summary>
    ' ''' <param name="LIndex">The L-Index value to calculate P-sust SD lower from.</param>
    ' ''' <returns></returns>
    ' ''' <remarks>
    ' ''' From Marta Coll / Simone Libralato
    ' ''' </remarks>
    'Private Function CalcPsustSDlower(LIndex As Single) As Single
    '    Return CSng(690857 * LIndex ^ 6 - 324339 * LIndex ^ 5 + 52144 * LIndex ^ 4 - 3409.8 * LIndex ^ 3 + 160.53 * LIndex ^ 2 - 18.241 * LIndex + 1.0109)
    'End Function

    ''' <summary>
    ''' Absolute L-index over time (Ecosim)
    ''' </summary>
    Public ReadOnly Property LIndexEcosim() As Single()
        Get
            Return Me.m_econetwork.AbsoluteLIndex
        End Get
    End Property

    ''' <summary>
    ''' Relative L-index over time (Ecosim)
    ''' </summary>
    Public ReadOnly Property LIndexPlot() As Single()
        Get
            Return Me.m_econetwork.RelativeLIndex
        End Get
    End Property

    ''' <summary>
    ''' Absolute Psust over time (Ecosim)
    ''' </summary>
    Public ReadOnly Property PsustEcosim() As Single()
        Get
            Return Me.m_econetwork.AbsolutePsust
        End Get
    End Property

    ''' <summary>
    ''' Relative Psust over time (Ecosim)
    ''' </summary>
    Public ReadOnly Property PsustPlot() As Single()
        Get
            Return Me.m_econetwork.RelativePsust
        End Get
    End Property

#End Region ' Indicators

#Region " Primary Production Required "

#If 0 Then ' Unused code

    Public ReadOnly Property nCatch() As Integer
        Get
            'count the number of groups with that have fish catch
            lstCatch.Clear()
            Dim n As Integer
            For igrp As Integer = 1 To m_epdata.NumLiving
                If m_epdata.fCatch(igrp) <> 0 Then
                    n += 1
                End If
            Next igrp
            Return n

        End Get
    End Property

    ''' <summary>
    ''' List of Groups that have fish catch
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is the iGroup of groups that make up the EwE5 Primary Prod.required "For harvest of all groups" grid  </remarks>
    Public ReadOnly Property CatchGroups() As List(Of Integer)

        Get

            For igrp As Integer = 1 To m_epdata.NumLiving
                If m_epdata.fCatch(igrp) <> 0 Then
                    lstCatch.Add(igrp)
                End If
            Next igrp
            Return lstCatch
        End Get

    End Property

#End If ' Unused code

    ''' <summary>
    ''' EwE5 No.of paths
    ''' </summary>
    Public ReadOnly Property NumOfPaths(ByVal iGroup As Integer) As Integer
        Get
            If (iGroup > Me.Core.nLivingGroups) Then Return cCore.NULL_VALUE
            Return m_econetwork.NumPath(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 TL
    ''' </summary>
    Public ReadOnly Property TrophicLevel(ByVal iGroup As Integer) As Single
        Get
            Return m_epdata.TTLX(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 totalPP
    ''' </summary>
    Public ReadOnly Property TotalPrimaryProduction() As Single
        Get
            Return m_econetwork.totalPP
        End Get
    End Property

#Region "For consumption of all groups"

    ''' <summary>
    ''' EwE5 PPR(PP)
    ''' </summary>
    Public ReadOnly Property PPRRequired(ByVal iGroup As Integer) As Single
        Get
            'Return m_epdata.TTLX(iGroup)
            Return m_econetwork.SumPPRequired(1, iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR(det)
    ''' </summary>
    Public ReadOnly Property PPRRequiredDet(ByVal iGroup As Integer) As Single
        Get
            Return m_econetwork.SumDetRequired(1, iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR (= PPRRequired + PPRRequiredDet)
    ''' </summary>
    Public ReadOnly Property PPRRequiredSum(ByVal iGroup As Integer) As Single
        Get
            Return PPRRequired(iGroup) + PPRRequiredDet(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Cons
    ''' </summary>
    Public ReadOnly Property PPRCons(ByVal iGroup As Integer) As Single
        Get
            Return m_epdata.B(iGroup) * m_epdata.QB(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR/cons
    ''' </summary>
    Public ReadOnly Property PPROverCons(ByVal iGroup As Integer) As Single
        Get
            Return PPRRequiredSum(iGroup) / PPRCons(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR/TotPP(%)
    ''' </summary>
    Public ReadOnly Property PPRTotPP(ByVal iGroup As Integer) As Single
        Get
            Return CSng(100.0 * PPRRequiredSum(iGroup) / (m_econetwork.totalPP + m_econetwork.TRPD(1)))
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR/u.biom.
    ''' </summary>
    Public ReadOnly Property PPRU(ByVal iGroup As Integer) As Single
        Get
            Return PPRRequiredSum(iGroup) / (m_econetwork.totalPP + m_econetwork.TRPD(1)) / m_epdata.B(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 NumLivPath.
    ''' </summary>
    Public ReadOnly Property NumLivPath() As Single
        Get
            Return m_econetwork.NumLivPath
        End Get
    End Property

    ''' <summary>
    ''' EwE5 NumDetPath.
    ''' </summary>
    Public ReadOnly Property NumDetPath() As Single
        Get
            Return m_econetwork.NumDetPath
        End Get
    End Property

#End Region

#Region " For harvest of all groups "

    ''' <summary>
    ''' EwE5 PPR(PP)
    ''' </summary>
    Public ReadOnly Property PPRRequiredHarvest(ByVal iGroup As Integer) As Single
        Get
            Return m_econetwork.SumPPRequired(0, iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR(det)
    ''' </summary>
    Public ReadOnly Property PPRRequiredDetHarvest(ByVal iGroup As Integer) As Single
        Get
            Return m_econetwork.SumDetRequired(0, iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR (= PPRRequired + PPRRequiredDet)
    ''' </summary>
    Public ReadOnly Property PPRRequiredSumHarvest(ByVal iGroup As Integer) As Single
        Get
            Return PPRRequiredHarvest(iGroup) + PPRRequiredDetHarvest(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Catch
    ''' </summary>
    Public ReadOnly Property PPRCatchHarvest(ByVal iGroup As Integer) As Single
        Get
            Return m_epdata.fCatch(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR/catch
    ''' </summary>
    Public ReadOnly Property PPROverCatchHarvest(ByVal iGroup As Integer) As Single
        Get
            Return PPRRequiredSumHarvest(iGroup) / PPRCatchHarvest(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR/TotPP(%)
    ''' </summary>
    Public ReadOnly Property PPRTotPPHarvest(ByVal iGroup As Integer) As Single
        Get
            Return CSng(100.0 * PPRRequiredSumHarvest(iGroup) / (m_econetwork.totalPP + m_econetwork.TRPD(1)))
        End Get
    End Property

    ''' <summary>
    ''' EwE5 PPR/u.catch
    ''' </summary>
    Public ReadOnly Property PPRUHarvest(ByVal iGroup As Integer) As Single
        Get
            Return PPRRequiredSumHarvest(iGroup) / (m_econetwork.totalPP + m_econetwork.TRPD(1)) / m_epdata.fCatch(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 RaiseToPP
    ''' </summary>
    Public ReadOnly Property TotalTL() As Single
        Get
            Return m_epdata.TLcatch
        End Get
    End Property

    ''' <summary>
    ''' EwE5 RaiseToPP
    ''' </summary>
    Public ReadOnly Property TotalPPRPP() As Single
        Get
            Return m_econetwork.RaiseToPP(0)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 RaiseToDet
    ''' </summary>
    Public ReadOnly Property TotalPPRDet() As Single
        Get
            Return m_econetwork.RaiseToDet(0)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 totalCatch
    ''' </summary>
    Public ReadOnly Property TotalCatch() As Single
        Get
            Return m_econetwork.totalCatch
        End Get
    End Property

#End Region

#End Region ' Primary Production Required

#Region " Ecosim Public Properties "

    Public ReadOnly Property nEcosimTimesteps() As Integer
        Get
            Return Core.nEcosimTimeSteps
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Ecosim plot "FIB index"
    ''' </summary>
    Public ReadOnly Property FIB() As Single()
        Get
            Return Me.EcosimData.FIB
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Ecosim plot "Total catch "
    ''' </summary>
    Public ReadOnly Property RelativeSumOfCatch() As Single()
        Get
            Return Me.m_econetwork.RelativeSumOfCatch
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Ecosim plot "Kemptons Q"
    ''' </summary>
    Public ReadOnly Property RelativeDiversity() As Single()
        Get
            Return Me.m_econetwork.RelativeDiversityIndex
        End Get
    End Property

    ''' <summary>
    '''  EwE5 Ecosim plot "TL of catch "
    ''' </summary>
    Public ReadOnly Property TLCatch() As Single()
        Get
            Return Me.m_econetwork.TLCatch
        End Get
    End Property

    ''' <summary>
    '''  EwE5 Ecosim plot TL (trophic level of all groups)
    ''' </summary>
    Public ReadOnly Property TLSim(ByVal iGroup As Integer, ByVal iTime As Integer) As Single
        Get
            Return Me.m_econetwork.TLSim(iGroup, iTime)
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Ecosim plot "Catch PPR "
    ''' </summary>
    Public ReadOnly Property RelativeCatchPPR() As Single()
        Get
            Return Me.m_econetwork.RelativeCatchPPR
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Ecosim plot "Catch detritus req."
    ''' </summary>
    Public ReadOnly Property RelativeDetritusReq() As Single()
        Get
            Return Me.m_econetwork.RelativeCatchDetReq
        End Get
    End Property

    ''' <summary>
    ''' EwE5 Ecosim csv parameter "TruPut"
    ''' </summary>
    Public ReadOnly Property ThroughputEcosim() As Single()
        Get
            Return Me.m_econetwork.Throughput
        End Get
    End Property

    Public ReadOnly Property CapacityEcosim() As Single()
        Get
            Return Me.m_econetwork.CapacityEcosim
        End Get
    End Property

    Public ReadOnly Property AscendImportEcosim() As Single()
        Get
            Return Me.m_econetwork.AscendImport
        End Get
    End Property

    Public ReadOnly Property AscendFlowEcosim() As Single()
        Get
            Return Me.m_econetwork.AscendFlow
        End Get
    End Property

    Public ReadOnly Property AscendExportEcosim() As Single()
        Get
            Return Me.m_econetwork.AscendExport
        End Get
    End Property

    Public ReadOnly Property AscendRespEcosim() As Single()
        Get
            Return Me.m_econetwork.AscendResp
        End Get
    End Property

    Public ReadOnly Property OverheadImportEcosim() As Single()
        Get
            Return Me.m_econetwork.OverheadImport
        End Get
    End Property

    Public ReadOnly Property OverheadFlowEcosim() As Single()
        Get
            Return Me.m_econetwork.OverheadFlow
        End Get
    End Property

    Public ReadOnly Property OverheadExportEcosim() As Single()
        Get
            Return Me.m_econetwork.OverheadExport
        End Get
    End Property

    Public ReadOnly Property OverheadRespEcosim() As Single()
        Get
            Return Me.m_econetwork.OverheadResp
        End Get
    End Property

    Public ReadOnly Property PCIEcosim() As Single()
        Get
            Return Me.m_econetwork.PCI
        End Get
    End Property

    Public ReadOnly Property FCIEcosim() As Single()
        Get
            Return Me.m_econetwork.FCI
        End Get
    End Property

    Public ReadOnly Property PathLengthEcosim() As Single()
        Get
            Return Me.m_econetwork.PathLength
        End Get
    End Property

    Public ReadOnly Property ExportEcosim() As Single()
        Get
            Return Me.m_econetwork.Export
        End Get
    End Property

    Public ReadOnly Property RespEcosim() As Single()
        Get
            Return Me.m_econetwork.Resp
        End Get
    End Property

    Public ReadOnly Property PrimaryProdEcosim() As Single()
        Get
            Return Me.m_econetwork.PrimaryProd
        End Get
    End Property

    Public ReadOnly Property ProdEcosim() As Single()
        Get
            Return Me.m_econetwork.Prod
        End Get
    End Property

    Public ReadOnly Property BiomassEcosim() As Single()
        Get
            Return Me.m_econetwork.Biomass
        End Get
    End Property

    Public ReadOnly Property CatchEcosim() As Single()
        Get
            Return Me.m_econetwork.CatchEcosim
        End Get
    End Property

    Public ReadOnly Property PropFlowDetEcosim() As Single()
        Get
            Return Me.m_econetwork.PropFlowDet
        End Get
    End Property

    Public ReadOnly Property RaiseToPPEcosim() As Single()
        Get
            Return Me.m_econetwork.RaiseToPPEcosim
        End Get
    End Property

    Public ReadOnly Property RaiseToDetEcosim() As Single()
        Get
            Return Me.m_econetwork.RaiseToDetEcosim
        End Get
    End Property

    Public ReadOnly Property AscendTotalEcosim() As Single()
        Get
            Return Me.m_econetwork.Ascendency
        End Get
    End Property

    Public ReadOnly Property AMIEcosim() As Single()
        Get
            Return Me.m_econetwork.AMI
        End Get
    End Property

    Public ReadOnly Property EntropyEcosim() As Single()
        Get
            Return Me.m_econetwork.Entropy
        End Get
    End Property

    Public ReadOnly Property DetTransferEfficiencyWeighted() As Single()
        Get
            Return Me.m_econetwork.DetTransferEfficiencyWeighted
        End Get
    End Property

    Public ReadOnly Property PPTransferEfficiencyWeighted() As Single()
        Get
            Return Me.m_econetwork.PPTransferEfficiencyWeighted
        End Get
    End Property

    Public ReadOnly Property TotTransferEfficiencyWeighted() As Single()
        Get
            Return Me.m_econetwork.TotTransferEfficiencyWeighted
        End Get
    End Property

#End Region ' Ecosim Public Properties

#Region " Keystoneness "

    ''' <summary>
    ''' Libralato et al
    ''' </summary>
    Public ReadOnly Property KeystoneIndex1(ByVal iGroup As Integer) As Double
        Get
            If (iGroup > Me.Core.nLivingGroups) Then Return cCore.NULL_VALUE
            Return Me.m_econetwork.KeystoneIndex1(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' Power et al
    ''' </summary>
    Public ReadOnly Property KeystoneIndex2(ByVal iGroup As Integer) As Double
        Get
            If (iGroup > Me.Core.nLivingGroups) Then Return cCore.NULL_VALUE
            Return Me.m_econetwork.KeystoneIndex2(iGroup)
        End Get
    End Property

    ''' <summary>
    ''' Valls
    ''' </summary>
    Public ReadOnly Property KeystoneIndex3(ByVal iGroup As Integer) As Double
        Get
            If (iGroup > Me.Core.nLivingGroups) Then Return cCore.NULL_VALUE
            Return Me.m_econetwork.KeystoneIndex3(iGroup)
        End Get
    End Property

    Public ReadOnly Property RelativeTotalImpact(ByVal iGroup As Integer) As Double
        Get
            If (iGroup > Me.Core.nLivingGroups) Then Return cCore.NULL_VALUE
            Return Me.m_econetwork.RelTotalImpact(iGroup)
        End Get
    End Property

#End Region ' Keystoneness

#End Region ' Model outputs

#End Region ' Public Properties

#Region " Message handlers "

#Region "Methods used by Network Analysis to update the Manager about progress."

    ''' ------------------------------------------------------------------------
    ''' <summary>
    ''' Notify the world of our progress.
    ''' </summary>
    ''' ------------------------------------------------------------------------
    Friend Sub UpdateProgress(ByVal strText As String, ByVal sProgress As Single)
        Try
            cApplicationStatusNotifier.UpdateProgress(Me.m_core, strText, sProgress)
        Catch ex As Exception
        End Try
    End Sub

#End Region

    ''' <summary>
    ''' Listen to the core's state monitor to see if Ecopath and Ecosim have changed
    ''' </summary>
    Private Sub OnCoreExecutionStateChanged(ByVal csm As cCoreStateMonitor)

        Dim bStateChanged As Boolean = False

        ' Assume the worst
        ' Fixes bug 937
        m_runstate = eRunState.CoreNotReady

        'If ecopath has loaded or it has just run 
        'then the network analysis needs to be run or re-run
        If csm.IsExecutionStateSuperceded(EwEUtils.Core.eCoreExecutionState.EcopathCompleted) Then
            m_runstate = eRunState.NetworkNeedsToRun
        End If

        'An ecosim scenario has loaded 
        If csm.IsExecutionStateSuperceded(EwEUtils.Core.eCoreExecutionState.EcosimLoaded) Then
            m_runstate = eRunState.EcosimIsLoaded
        End If

        ' Invalidate results when core states dictate 
        ' Fixes bug 617
        If Not csm.HasEcopathRan Then
            bStateChanged = Me.IsMainNetworkRun
            Me.IsMainNetworkRun = False
        End If

        If Not csm.HasEcosimRan Then
            bStateChanged = bStateChanged Or Me.IsEcosimNetworkRun
            Me.IsEcosimNetworkRun = False
        End If

        Try
            If bStateChanged Then RaiseEvent OnRunStateChanged()
        Catch ex As Exception

        End Try

    End Sub

    Friend Function AskUserConfirmation(ByVal strMsg As String) As Boolean

        Dim fmsg As New cFeedbackMessage(strMsg, _
                                         eCoreComponentType.External, _
                                         eMessageType.Any, _
                                         eMessageImportance.Question, eMessageReplyStyle.YES_NO)
        fmsg.Suppressable = True
        fmsg.Reply = eMessageReply.YES

        Me.Core.Messages.SendMessage(fmsg)

        Return (fmsg.Reply = eMessageReply.YES)

    End Function

#End Region ' Message handlers

End Class
