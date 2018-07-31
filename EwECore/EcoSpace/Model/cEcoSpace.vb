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

Imports System.Math
Imports System.Threading
Imports EwECore.SpatialData
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Utilities

''' <summary>
''' Definition of Time Step Delegate used for notification of an Ecospace time step
''' </summary>
Public Delegate Sub EcoSpaceTimeStepDelegate(ByVal iTime As Integer)

Public Delegate Sub EcoSpaceRunCompletedDelegate(ByVal Succeeded As Boolean)

Public Class cEcoSpace
    Inherits cThreadWaitBase

#Region "Helper Class Arguments for PredictEffortDistributionThreaded(cEffortDistArgs)"

    Private Shared m_ThreadIncrementCount As Integer
    Private Const THREAD_TIMEOUT As Integer = 5 * 60 * 1000 '5 minutes

    ''' <summary>
    ''' Arguments for PredictEffortDistributionThreaded()
    ''' </summary>
    ''' <remarks></remarks>
    Private Class cThreadedCallArgs
        Public WaitHandle As AutoResetEvent
        Public iFirst As Integer
        Public iLast As Integer
        Public iCumMonth As Integer
        Public iMonth As Integer

        Public Sub New(ByRef theWaitHandle As AutoResetEvent, ByVal iFirstIndex As Integer, ByVal iLastIndex As Integer, ByVal iMonthOfyear As Integer, ByVal iCumMonthIndex As Integer)
            WaitHandle = theWaitHandle
            iFirst = iFirstIndex
            iLast = iLastIndex
            iCumMonth = iCumMonthIndex
            iMonth = iMonthOfyear
        End Sub

        Public Sub New(ByRef theWaitHandle As AutoResetEvent, ByVal iFirstIndex As Integer, ByVal iLastIndex As Integer)
            WaitHandle = theWaitHandle
            iFirst = iFirstIndex
            iLast = iLastIndex
            iCumMonth = cCore.NULL_VALUE
            iMonth = cCore.NULL_VALUE
        End Sub

    End Class

#End Region

#Region "Solver threads"

    Public Delegate Sub SolverErrorDelegate(ByVal ThreadID As Integer, ByVal msg As String)
    Public EcoFunctions As cEcoFunctions

    Private m_TLlockOb As Object
    Private m_bsolverError As Boolean
    Private m_solverErrorMsg As String
    Private m_solverErrorID As Integer

#End Region

#Region "Private data"

    Private Const MIN_HABCAP As Single = 0.000001F
    Private Const TWO_PI As Double = Math.PI * 2.0#
    Private Const DEG2RAD As Double = TWO_PI / 360.0# 'for converting degrees to radians for functions

    Public Const MIN_MIG_PROB As Single = 0.0000000001

    ''' <summary>To call the plugins</summary>
    Private m_pluginManager As cPluginManager

    Private m_gridSolvers As List(Of cGridSolver)
    Private m_spaceSolvers As List(Of cSpaceSolver)
    Private m_IBMSolvers As List(Of cIBMSolver)

    Private m_TimestepDelegate As EcoSpaceTimeStepDelegate

    Private m_OnRunCompletedDelegate As EcoSpaceRunCompletedDelegate

    Private m_SyncObj As System.Threading.SynchronizationContext

    Private m_SpaceThread As Threading.Thread

    Private m_PauseSignal As System.Threading.ManualResetEvent

    ''' <summary>
    ''' Message Publisher for Ecospace messages
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Private m_publisher As New cMessagePublisher

    Private m_AdvectionManager As Ecospace.Advection.cAdvectionManager

    Private m_TimeSeriesManager As EcospaceTimeSeries.cEcospaceTimeSeriesManager

    Private m_EPdata As cEcopathDataStructures
    Private m_SimData As cEcosimDatastructures
    Private m_Data As cEcospaceDataStructures
    Private m_Stanza As cStanzaDatastructures
    Private m_Ecosim As Ecosim.cEcoSimModel
    Private m_search As cSearchDatastructures
    Private m_tracerData As cContaminantTracerDataStructures
    Private m_OptMPA As IMPASearchModel
    Private m_SpatialData As cSpatialDataStructures

    ''jb 16-June-2016 Remove the cEcospaceTimeSeriesDataStructures when implementing Ecosim biomass forcing in EcoSpace
    'use use the existing time series data structures
    'Private m_refdata As cEcospaceTimeSeriesDataStructures
    Private m_refdata As cTimeSeriesDataStructures

    Public m_StopRun As Boolean

    'new multiStanza stuff
    Private TotLoss() As Single
    Private TotEatenBy() As Single
    Private TotBiom() As Single
    Private TotPred() As Single
    Private TotIFDweight() As Single
    Private Blocal() As Single
    Private Tbiom As Single, Tpred As Single, Wcell As Single

    'habitat (preference function is habgrad, which has max value of habbest, 90% drop in movment if slope=-2 given movescale=1
    'habbest could be a constant
    Private HabBest As Integer
    'size of the window (number of cells) to compute the habitat gradients over see SetHabGrad()
    'iWindow could be a constant
    Private iWindow As Integer

    'Private HabGrad(,,) As Single
    ' Private NcellsHab() As Integer

    'the analog of habgrad, but for migration, and has a monthly component
    Private MigGrad(,)(,) As Single

    Private RelMoveFit(,) As Single 'populated in SetKmove()
    Private PzoTOmove() As Single 'populated in SetKmove()
    Private Kmovefit() As Single 'populated in SetKmove()
    Private RelFitness(,,) As Single

    Friend FtimeCell(,,) As Single 'feeding time???
    Private HdenCell(,,) As Single

    ''' <summary>Sum of Biomass for all the cells in the current time step </summary>
    Private Btime() As Single

    ''' <summary>Sum of cCell (contaminant)for all the cells in the current time step </summary>
    Private ConTotal() As Single
    Private MinChange As Single

    Private MigPowi(,) As Single
    Private MigPowj(,) As Single
    Private PrefRowP(,) As Single, PrefColP(,) As Single

    'these are now in the data structure
    'Private Vspace() As Single 'vulnerabilities set to same values as ecosim during initialization
    'Private Aspace() As Single 'search rate set to same values as ecosim during initialization
    Private PbSpace() As Single ' P/B from Ecopath

    Private der() As Single

    Private Basebiomass() As Single

    Private Flowin() As Single
    Private FlowoutRate() As Single

    'A() searchrate modifer one if in prefered habitate < 1 otherwise used in derivRed() to calculate effective search rate
    'repopulated for each time step each cell
    Private EatEff() As Single
    '   'V() modifier used in the same way as EatEff() to modfy effective vulnerability in derivtRed()
    Private VulPred() As Single

    Private Tstanza() As Single
    ' Private conSplit() As Single ' pred()/NstanzaBase()
    Private NstanzaBase() As Single
    Private RecSplit() As Single
    Private PconSplit() As Single

    ''' <remarks>
    ''' EwE5 loss() is global and the same variable is used for both Ecosim and Ecospace
    ''' EwE6 loss for Ecosim in declared in cEcosimDataStructres so that it can be used to initialize Wchange() in Ecospace
    ''' loss for Ecospace is private and computed in DerivtRed()
    ''' So if you need the loss from Derivt() use cEcoSimDataStructres.loss() not cEcoSpace.loss()!!!!!!
    ''' </remarks>
    Private loss() As Single


    Private pbb() As Single

    'movement parameters use for SolveGrid()
    'computed in SetMovementParameters() 
    Private Bcw(,,) As Single
    Private C(,,) As Single
    Private d(,,) As Single
    Private e(,,) As Single

    Private AMm(,,) As Single
    Private F(,,) As Single

    Private BEQlast(,,) As Single 'equilibrium biomass at the last timestep

    Private TimeStep2 As Single

    ' Dim Tn As Integer ' summary array index

    'jb Movement parameter with no migration?????
    'Set in SetMovementParameters() to the same values as counterparts BcwNomig() = Bcw()
    Private BcwNomig(,,) As Single
    Private CNomig(,,) As Single
    Private dNomig(,,) As Single
    Private Enomig(,,) As Single


    ''' <summary>
    ''' Converts an iGroup into a cumulative stanza index Nvarsplit
    ''' </summary>
    ''' <remarks>Populated in initSpatialEqulibrium().  
    ''' Use to access stanza varaibles that are stored after the groups in biologial indexes of spatial matrixes.
    ''' </remarks>
    Private IecoCode() As Integer

    ''' <summary>
    ''' Total number of time step
    ''' </summary>
    ''' <remarks>set in redimForRun()</remarks>
    Private nEcospaceTimeSteps As Integer

    ''' <summary>
    ''' This is the index to the imonth for data arrayed by month i.e. zscale()
    ''' </summary>
    ''' <remarks>If the user has set the Ecospace time step to some value other than monthly this index will point to the first month of the time step.
    ''' For example timestep = 0.5 first loop its = 1 second loop its = 7</remarks>
    Private its As Integer

    ''' <summary>
    ''' Cumulative itime step array index at the current user selected time step.
    ''' </summary>
    Private itt As Integer

    Private HabAreaUsed() As Single

    Private totalIterThread() As Integer 'total number of solvegrid iterations for each thread

    'grid solver for contaminant the tracer
    Private grdslvConSim As cGridSolver
    Private m_ConBypassIntegrated() As Boolean

    Private nMigratory As Integer 'total number of migratory variables that are solved for
    Private migratoryIndex() As Integer 'the ip index of the migratory species
    Private nGroupsInThread() As Integer 'number of groups solved in gridsolver by each thread
    Private threadGroups(,) As Integer 'the ip indices solved by each thread

    Private threadGroupsConSim(,) As Integer

    Private m_rand As Random

    Private CopySyncLock As New Object

    Private Shared FleetSyncLock As New Object
    Private Shared FleetCounter As Integer
    Private Shared nFleets As Integer

    Private bEffortAdjusted As Boolean

    ''' <summary>Number of timesteps in the total Spin-Up period</summary>   
    Private nSpinUp As Integer

    ''' <summary>Number of timesteps in one year used by Spin-Up.</summary>   
    Private nSpinUpYear As Integer

    ''' <summary>Cumulative Spin-Up counter</summary>   
    Private iSpinUp As Integer

    ''' <summary>Yearly Spin-Up counter</summary>   
    Private iSpinUpYear As Integer

    '''' <summary>Are we in a Spin-Up period </summary>   
    'Private bInSpinUp As Boolean

    ''' <summary>Does the Spin-Up base biomass need initialization</summary>
    Private bInitSpinUpBase As Boolean

#End Region

#Region "Construction Destruction"

    Public Sub New()

        Me.m_SyncObj = System.Threading.SynchronizationContext.Current
        'if there is no current context then create a new one on this thread. 
        'this happens if no interface has been created yet(I think...)
        If (Me.m_SyncObj Is Nothing) Then Me.m_SyncObj = New System.Threading.SynchronizationContext()

        Me.m_PauseSignal = New System.Threading.ManualResetEvent(True)
        Me.m_rand = New Random

        Me.m_TLlockOb = New Object

    End Sub


    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub


#End Region

#Region "Variables from FindSpatialEqulibrium()"

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Variables that where local to FindSpatialEqulibrium() in EwE5
    'moved to the level of the class so that FindSpatialEqulibrium() could be split up into smaller pieces
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    Private Cper(,,) As Single

    ''' <summary>
    ''' Converts a cumulative stanza index(Nvarsplit) into an iGroup index
    ''' </summary>
    ''' <remarks>Computed in initSpatialEquilibrium(). 
    ''' This is the opposite of IecoCode().
    ''' </remarks>
    Private Ecode() As Integer

    ''' <summary>
    ''' 1/StartBiomass of oldest stanza for this split
    ''' </summary>
    ''' <remarks>RelRepStanza(Nsplit)</remarks>
    Dim RelRepStanza() As Single

    ''' <summary>
    ''' Index of the first element after the end of the groups
    ''' </summary>
    ''' <remarks>This is used for the split group indexes that are stored after the end groups for arrays that are dimensioned by nTotVars</remarks>
    Dim nvar2 As Integer

#End Region

#Region "Public Properties"

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Exposes the MessagePublisher instance so that the core can add message handlers
    ''' </summary>
    ''' <remarks>
    '''  All messages from EcoSpace to the core are passed via this MessagePublisher. 
    '''  The core adds a message handler during Ecospace initialization <see cref="cCore.InitEcoSpace"> InitEcospace()</see> 
    ''' </remarks>
    ''' ----------------------------------------------------------------------- 
    Public ReadOnly Property Messages() As cMessagePublisher
        Get
            Return m_publisher
        End Get
    End Property

    Public Property PluginManager() As cPluginManager
        Get
            Return Me.m_pluginManager
        End Get
        Set(ByVal pm As cPluginManager)
            Me.m_pluginManager = pm
        End Set
    End Property

    ''' <summary>
    ''' Ecopath data used for initial state
    ''' </summary>
    Public Property EcoPathData() As cEcopathDataStructures
        Get
            Return m_EPdata
        End Get
        Set(ByVal value As cEcopathDataStructures)
            m_EPdata = value
        End Set
    End Property


    ''' <summary>
    ''' Ecosim data used for initial state
    ''' </summary>
    Public Property EcoSimData() As cEcosimDatastructures
        Get
            Return m_SimData
        End Get
        Set(ByVal value As cEcosimDatastructures)
            m_SimData = value
        End Set
    End Property

    Public Property EcoSpaceData() As cEcospaceDataStructures
        Get
            Return m_Data
        End Get
        Set(ByVal value As cEcospaceDataStructures)
            m_Data = value
        End Set
    End Property

    Public Property StanzaData() As cStanzaDatastructures
        Get
            Return m_Stanza
        End Get
        Set(ByVal value As cStanzaDatastructures)
            m_Stanza = value
        End Set
    End Property

    Public Property EcoSim() As Ecosim.cEcoSimModel
        Get
            Return m_Ecosim
        End Get
        Set(ByVal value As Ecosim.cEcoSimModel)
            m_Ecosim = value
        End Set
    End Property

    Public Property ContaiminantTracerData() As cContaminantTracerDataStructures
        Get
            Return m_tracerData
        End Get
        Set(ByVal value As cContaminantTracerDataStructures)
            m_tracerData = value
        End Set
    End Property

    'jb 16-June-2016 Remove the cEcospaceTimeSeriesDataStructures when implementing Ecosim biomass forcing in EcoSpace
    'Public Property EcospaceTimeSeriesData() As cEcospaceTimeSeriesDataStructures
    '    Get
    '        Return m_refdata
    '    End Get
    '    Set(ByVal newValue As cEcospaceTimeSeriesDataStructures)
    '        m_refdata = newValue
    '    End Set
    'End Property

    Public Property TimeSeriesData() As cTimeSeriesDataStructures
        Get
            Return m_refdata
        End Get
        Set(ByVal newValue As cTimeSeriesDataStructures)
            m_refdata = newValue
        End Set
    End Property

    Public Property TimeStepDelegate() As EcoSpaceTimeStepDelegate
        Get
            Return Me.m_TimestepDelegate
        End Get
        Set(ByVal value As EcoSpaceTimeStepDelegate)
            m_TimestepDelegate = value
        End Set
    End Property

    Public Property SpatialData As cSpatialDataStructures
        Get
            Return Me.m_SpatialData
        End Get
        Set(ByVal value As cSpatialDataStructures)
            Me.m_SpatialData = value
        End Set
    End Property

    Public Property AdvectionManager As Ecospace.Advection.cAdvectionManager
        Get
            Return Me.m_AdvectionManager
        End Get
        Set(ByVal value As Ecospace.Advection.cAdvectionManager)
            Me.m_AdvectionManager = value
        End Set
    End Property

    'Public Property StopRun() As Boolean
    '    Get
    '        Return m_StopRun
    '    End Get
    '    Set(ByVal value As Boolean)
    '        m_StopRun = value
    '        'if EcoSpace is Paused then let run so it can Stop
    '        If Me.Paused Then
    '            Me.Paused = False
    '        End If
    '    End Set
    'End Property

    Public Property SearchData() As cSearchDatastructures
        Get
            Return m_search
        End Get
        Set(ByVal value As cSearchDatastructures)
            m_search = value
        End Set
    End Property

    Public Property MPAOptimization() As IMPASearchModel
        Get
            Return m_OptMPA
        End Get
        Set(ByVal value As IMPASearchModel)
            m_OptMPA = value
        End Set
    End Property


    Public WriteOnly Property RunCompletedDelegate() As EcoSpaceRunCompletedDelegate
        Set(ByVal value As EcoSpaceRunCompletedDelegate)
            Me.m_OnRunCompletedDelegate = value
        End Set
    End Property


    Public Property TimeSeriesManager() As EcospaceTimeSeries.cEcospaceTimeSeriesManager
        Set(value As EcospaceTimeSeries.cEcospaceTimeSeriesManager)
            Me.m_TimeSeriesManager = value
        End Set
        Get
            Return Me.m_TimeSeriesManager
        End Get
    End Property




#End Region

#Region "Initialization"

    ''' <summary>
    ''' Initialize base varaibles with the default values
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Was MPAMoveLoadFormTasks() in EwE5. Most of this was moved to cEcoSpaceDataStructures.SetDefaults() </remarks>
    Public Function InitToDefaults() As Boolean
        Dim i As Integer

        Try

            Debug.Assert(m_Data IsNot Nothing, Me.ToString & ".Init() Data not initialized.")
            Debug.Assert(m_EPdata IsNot Nothing, Me.ToString & ".Init() Data not initialized.")
            Debug.Assert(m_SimData IsNot Nothing, Me.ToString & ".Init() Data not initialized.")
            Debug.Assert(m_Ecosim IsNot Nothing, Me.ToString & ".Init() Data not initialized.")
            Debug.Assert(m_Stanza IsNot Nothing, Me.ToString & ".Init() Data not initialized.")

            'set parameters used to define habitat gradient functions and strength of response to gradients toward desired
            'habitat (preference function is habgrad, which has max value of habbest, 90% drop in movment if slope=-2 given movescale=1
            HabBest = 10
            iWindow = 5

            'was in FindSpatialEquilibrium()
            MinChange = 0.3

            m_Data.W = 1.2
            m_Data.Tol = 0.0001
            m_Data.maxIter = 40

            m_Stanza.NPacketsMultiplier = 0.5
            'm_Data.NewMultiStanza = True
            m_Data.UseIBM = True
            m_Data.TimeStep = CSng(1 / 12) 'one month

            'this should be available to users in interface, higher values typically cause
            'instability in spatial allocation (IFD) model for multistanza biomass distributions
            m_Data.IFDPower = 0.5 'this isn't actually used anymore

            'nvartot
            ReDim IecoCode(m_Data.NGroups)

            'compute the IecoCode() index
            'this index pointer is unique to Ecospace
            'this will need to be re-computed if the number of groups or stanzas change
            Dim ir As Integer, igrp As Integer
            For i = 1 To m_Stanza.Nsplit
                For j As Integer = 1 To m_Stanza.Nstanza(i)
                    ir = ir + 1
                    igrp = m_Stanza.EcopathCode(i, j)
                    IecoCode(igrp) = ir
                Next
            Next

            Me.UpdateDepthMap()
            '   Me.m_SpaceCatchSemaphor = New System.Threading.Semaphore(1, 1, "EcoSpaceMontlyCatch")

            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Init() Error: " & ex.Message)
            Return False
        End Try

    End Function

    Public Sub Load()
        Me.UpdateDepthMap()
    End Sub

#End Region

#Region "Public methods"

    ''' <summary>
    ''' Run Ecospace synchronously. cEcoSpace.Run() will not return until the Ecospace run has completed.
    ''' </summary>
    ''' <returns>True if Ecospace ran successfully. False otherwise.</returns>
    ''' <remarks>
    ''' cEcoSpace.Run() runs Ecospace on the same thread as the calling routine. 
    ''' It is used when you need run Ecospace in some kind of a looping structure 
    ''' and gather the results at the end of each run. 
    ''' <see cref="EwECore.cMPARandomSearch.runSearch">MPA Optimization </see> 
    ''' </remarks>
    Public Function Run() As Boolean
        Dim bsuccess As Boolean = True
        Try

            cEcoSpace.nFleets = Me.m_Data.nFleets


            'redim all 
            If redimForRun() Then

                'Init the Spatial Temporal data
                Me.InitSpatialTemporalRun()
                'Initialized EcoSpace
                Me.initSpatialEquilibrium()
                'Run Ecospace
                Me.FindSpatialEquilibrium()

                'Cleanup the Spatial Temporal data
                Me.EndSpatialTemporalRun()

            Else
                bsuccess = False
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            m_publisher.AddMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_GENERIC_ERROR, ex.Message), _
                                                eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Critical, eDataTypes.NotSet))
            bsuccess = False
        End Try

        Try
            m_publisher.sendAllMessages()
            m_SyncObj.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireRunCompleted), bsuccess)
        Catch ex As Exception
            Debug.Assert(False, "Exception calling Ecosim.OnRunCompleted() Exception: " & ex.Message)
        End Try

        Return bsuccess

    End Function


    ''' <summary>
    ''' Run EcoSpace on a seperate thread from the calling routine. This will return before the EcoSpace run has completed.
    ''' </summary>
    ''' <returns>True if the EcoSpace run was successfully started, this does not mean the run has successfully completed. False otherwise.</returns>
    ''' <remarks>
    ''' Starts EcoSpace running on a seperate thread and returns immediately. 
    ''' Once the EcoSpace run has completed the <see cref="RunCompletedDelegate">RunCompletedDelegate</see> 
    ''' will be called with the success or failure of the run.   
    ''' Used by the interface so running EcoSpace does not block the interface.
    '''</remarks>
    Public Function RunThreaded() As Boolean
        Dim started As Boolean = False
        Try

            'Has the SpaceThread already been created
            If Me.m_SpaceThread IsNot Nothing Then
                'Yes Is it running
                If Me.m_SpaceThread.ThreadState = ThreadState.Running Then
                    'Yes Ecospace is already running so boot out of here
                    Me.m_publisher.SendMessage(New cMessage(My.Resources.CoreMessages.ECOSPACE_RUNNING, eMessageType.ErrorEncountered, _
                            eCoreComponentType.EcoSpace, eMessageImportance.Critical, eDataTypes.NotSet))
                    Return False
                End If

                'Space is not running 
                'But the Thread has already been created
                'so clear it out of memory
                Me.m_SpaceThread = Nothing

            End If

            'Not running Create a new thread object
            Me.m_SpaceThread = New Thread(AddressOf Me.RunSpace)

            'run EcoSpace on the new thread
            Me.m_SpaceThread.Start()
            started = True

        Catch ex As Exception
            started = False
            Debug.Assert(False, ex.Message)
            m_publisher.AddMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_GENERIC_ERROR, ex.Message), _
                                               eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Critical, eDataTypes.NotSet))
        End Try

        m_publisher.sendAllMessages()

        'Failed to start Ecospace
        'make sure the onRunCompleted delegate is fire so the core can clean up
        If Not started Then
            Me.fireRunCompleted(False)
        End If

        Return started

    End Function

    ''' <summary>
    ''' Provides a wrapper around run so the Run() function signature matches the ThreadStart delegate signature
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub RunSpace()

        Me.SetWait()
        Me.Run()
        Me.ReleaseWait()

    End Sub


    Private Sub fireRunCompleted(ByVal ob As Object)
        Try
            If Me.m_OnRunCompletedDelegate IsNot Nothing Then
                Me.m_OnRunCompletedDelegate.Invoke(ob)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Property isPaused() As Boolean
        Get
            'this is confusing
            'WaitOne(0) will wait for Zero time and return True or False
            'True if the signal state is "Signaled", the wait handle is not in use, in our case it is NOT Paused
            'False if it TimedOut, had to wait for zero milliseconds, the signal state is "Non-Signaled", the handle is in use
            Return Not Me.m_PauseSignal.WaitOne(0)
        End Get

        Set(ByVal value As Boolean)

            If value = True Then
                'Set the signal state to "Non-Signaled" which causes calls to Wait to Block
                Me.m_PauseSignal.Reset()
            Else
                'Sets the signal state to "Signaled" which means it is not in use, it's ready to be used... 
                Me.m_PauseSignal.Set()
            End If

        End Set

    End Property


    Public Overrides Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean
        Dim result As Boolean = True
        Try
            Me.m_StopRun = True
            If Me.isPaused Then
                Me.isPaused = False
            End If
            result = Me.Wait(WaitTimeInMillSec)
        Catch ex As Exception
            result = False
        End Try

        Return result

    End Function

    Public Sub Clear()

        Try
            If Me.m_spaceSolvers IsNot Nothing Then
                For Each solver As cSpaceSolver In Me.m_spaceSolvers
                    solver.Clear()
                Next
                Me.m_spaceSolvers.Clear()
                Me.m_spaceSolvers = Nothing
            End If

            If Me.m_gridSolvers IsNot Nothing Then
                Me.m_gridSolvers.Clear()
                Me.m_gridSolvers = Nothing
            End If

            MigPowi = Nothing '(m_Data.NGroups, m_Data.InRow + 1)
            MigPowj = Nothing '(m_Data.NGroups, m_Data.InCol + 1)
            PrefRowP = Nothing '(m_Data.NGroups, 12)
            PrefColP = Nothing '(m_Data.NGroups, 12)
            Cper = Nothing '(m_Data.InRow + 1, m_Data.InCol + 1, m_Data.NGroups)
            Ecode = Nothing '(m_Data.Nvarsplit)
            F = Nothing '
            AMm = Nothing
            BcwNomig = Nothing
            AMm = Nothing '(,,) As Single
            F = Nothing '(,,) As Single
            BEQlast = Nothing '(,,) As Single 'equilibrium biomass at the last timestep
            CNomig = Nothing '(,,) As Single
            dNomig = Nothing '(,,) As Single
            Enomig = Nothing '(,,) As Single
            FtimeCell = Nothing
            HdenCell = Nothing
            HabAreaUsed = Nothing
            RelFitness = Nothing
            Bcw = Nothing '(,,) As Single
            C = Nothing '(,,) As Single
            d = Nothing '(,,) As Single
            e = Nothing '(,,) As Single

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

#End Region

#Region " Private modeling code "

    'SET Dumpcb=0 TO STOP DUMPING ALL C/B VALUES TO CSV FILE AND/OR RESET FILE PATHWAY FOR SAVING

#Const Dumpcb = 0

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' This routine attempts to seek spatial equilibrium in ecosim biomasses, given mpa pattern
    ''' and start density map based on no movement
    '''</remarks>
    Private Sub FindSpatialEquilibrium()
        'this routine attempts to seek spatial equilibrium in ecosim biomasses, given mpa pattern
        'and start density map based on no movement
        Dim i As Integer
        Dim j As Integer
        Dim ip As Integer
        Dim irgn As Integer
        Dim RelFopt() As Single
        Dim Fgear() As Single
        Dim FtimeTotal(m_Data.NGroups) As Single
        Dim ExtraTime As Integer = m_search.ExtraYearsForSearch

        'timers
        Dim stpwchTotRunTime As New Stopwatch
        Dim stpwchSolver As New Stopwatch
        Dim stpwchGrid As New Stopwatch
        Dim stpwchEffort As New Stopwatch

        Try

#If Dumpcb Then
            Dim CoutFile As String = ”spaceconc.csv”
            Dim CoutWriter As New System.IO.StreamWriter(CoutFile, False)
            Dim CoutVals() As String
            ReDim CoutVals(3 + m_Data.NGroups)
#End If
            ReDim Fgear(m_EPdata.NumFleet)
            ReDim RelFopt(1)
            'stanza counters
            'nvar2 is an index that counts from the end of the groups up to cEcoSpaceDataStructures.nvartot = nGroups + NSplit(nvartot = [total number of groups] + [sum of all split groups])
            'it is used for stanza data that is stored after groups (any variable that is dimed by nvartot)
            nvar2 = m_Data.NGroups

            Dim iTotalCells As Integer = m_Data.InCol * m_Data.InRow

            'Initialize IBM 
            Me.InitIBM()

            If m_search.bInSearch Then
                m_search.initForRun(Me.m_EPdata, Me.m_SimData)
                m_search.setBaseYearEffort(Me.m_SimData)
            End If

            Dim StartTime As Single = 0
            If m_OptMPA IsNot Nothing Then
                If m_OptMPA.isRunning Then
                    StartTime = m_OptMPA.EcospaceStartTime
                End If
            End If

            'Run initialization has completed Call the plugin point
            If (Me.PluginManager IsNot Nothing) Then Me.PluginManager.EcospaceInitRunCompleted(Me.m_Data)
            stpwchTotRunTime.Start()

            'Zero the cummulative time step counter
            itt = 0

            'Set the Ecosim Forced Biomass for the first timestep
            'This ensures that the biomass matches the Ecosim time series values at the start of the simulation
            'inside the time loop Me.ForceBiomassWithEcosimTimeSeries(its) will be called at the end of the time step
            'to set biomass to Ecosim time series values for the output and next time step
            m_Data.TimeNow = 0
            m_Data.YearNow = 1
            Me.ForceBiomassWithEcosimTimeSeries(1)
            ' hack to dump ecotracer C/B to csv file 



            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'START OF TIME LOOP
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            For m_Data.TimeNow = StartTime To m_Data.TotalTime Step m_Data.TimeStep

                'One Off hack to pause the run for the Water Institute's 2017 Gulf Coast Model
                'Me.HACKAutoPause(its)

                Me.m_PauseSignal.WaitOne()

                'Ecospace has been stopped
                If Me.m_StopRun Then Exit For

                'Set itt(cumulative timestep counter) and its(monthly index counter)
                'setTimeStepCounters() also deals with the SpinUp period
                Me.setTimeStepCounters(itt, its)

                Me.m_SimData.setRelQToT(its, False)

                If itt > nEcospaceTimeSteps Then
                    'We have exceeded the number of time step bump out of the time loop.
                    'This quarantees we don't come up one time step long due to rounding issues with m_Data.TimeStep
                    itt = nEcospaceTimeSteps
                    Exit For
                End If

                'Set the isFished(fleet,row,col) array
                Me.setIsFished()

                Dim SPSt As Double = stpwchTotRunTime.Elapsed.TotalSeconds
                'Read any Spatial Temporal data into memory for this timestep
                Me.SetSpatialTempData(itt)
                ' System.Console.WriteLine("SetSpatialTempData() run time(sec), " + (stpwchTotRunTime.Elapsed.TotalSeconds - SPSt).ToString)

                'do external processing at the start of the time step i.e. Call Plugins or sub models
                Me.BeginTimeStep(Fgear, its, m_Data.MonthNow, m_Data.YearNow, Btime, RelFopt, m_Data.TimeNow)

                If Me.m_Data.isCapacityChanged Then
                    'Dim hcSt As Double = stpwchTotRunTime.Elapsed.TotalSeconds
                    'set the Capacity maps if any of the inputs have changed
                    Me.SetHabCap()
                    'System.Console.WriteLine("SetHabCap() run time(sec), " + (stpwchTotRunTime.Elapsed.TotalSeconds - hcSt).ToString)
                End If

                'Tell Ecoseed that we are at the start of a timestep
                Me.EcoseedBeginTimeStep(m_Data.MonthNow, m_Data.YearNow, Btime)

                If m_search.bInSearch Then
                    For i = 1 To m_EPdata.NumFleet
                        If m_search.FblockCode(i, m_Data.YearNow) > 0 Then
                            m_SimData.FishRateGear(i, its) = Fgear(i)
                        End If
                        m_SimData.FishRateGear(i, 0) = Fgear(i) 'm_Data.FishRateGear(i, itime)
                    Next
                End If

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'jb 10-Jun-2016 Replace the original Advection Model 
                ''********************Martell******************
                ''This is for monthy current vectors.
                'If m_Data.CurrentForce Then
                '    For i = 0 To m_Data.InRow + 1
                '        For j = 0 To m_Data.InCol + 1
                '            m_Data.Xvel(i, j) = m_Data.Xv(i, j, m_Data.MonthNow)
                '            m_Data.Yvel(i, j) = m_Data.Yv(i, j, m_Data.MonthNow)
                '        Next j
                '    Next i

                '    'ToDo_jb FindSpatial....... velmaker
                '    'Calculate Upwelling indicies
                '    CalcAdvection(m_Data.MonthNow)
                '    SetMovementParameters()
                'End If
                'xxxxxxxxxxxxxxxxxxxxxxx
                'New Advection model

                If Me.m_Data.isAdvectionActive Then
                    'Update the monthly X and Y velocity vectors
                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1
                            m_Data.Xvel(i, j) = m_Data.MonthlyXvel(m_Data.MonthNow)(i, j)
                            m_Data.Yvel(i, j) = m_Data.MonthlyYvel(m_Data.MonthNow)(i, j)
                            m_Data.UpVel(i, j) = m_Data.MonthlyUpWell(m_Data.MonthNow)(i, j)
                        Next j
                    Next i
                    'set the movement patterns based on velocity vectors for this month set above
                    SetMovementParameters()
                End If


                'Set b(),c(),d() and e() cell movement parameters based on the migration movement gradient MigGrad()
                VaryMigMovementParameters(m_Data.MonthNow)

                'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                'HACK ALERT
                'Using our Harry Potter powers to magically move migrating biomass into the new area
                'this totally messes up the trophic interaction 
                'TeleportMigrationBiomass(m_Data.MonthNow)
                'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                'set tval() (time step forcing value) to the value for this time step for each forcing shape
                'Time forcing function are disable in EcoSpace via ApplyAVmodifiers() "UseTime" flag
                'If ApplyAVmodifiers() is called with the UseTime = True then the time forcing function will be used
                For i = 0 To m_SimData.NumForcingShapes
                    m_SimData.tval(i) = m_SimData.zscale(its, i)
                Next

                'ToDo_jb EggProdShapeSplit() make sure this is correct
                'set current relative reproductive rates for stanzas groups
                For i = 1 To m_Stanza.Nsplit
                    If m_Stanza.EggProdShapeSplit(i) > 0 Then
                        'Debug.Assert(m_SimData.tval(m_Stanza.EggProdShapeSplit(i)) = 0)
                        RelRepStanza(i) = m_SimData.tval(m_Stanza.EggProdShapeSplit(i)) * m_Stanza.RscaleSplit(i) / m_SimData.StartBiomass(m_Stanza.EcopathCode(i, m_Stanza.Nstanza(i)))
                    End If
                Next



                If m_Data.PredictEffort Then

                    'Sets proportion of discards landed and discarded 
                    'With the Ecosim Discards Forcing time series
                    Me.setForcedDiscards(its, m_Data.YearNow)

                    If its >= 3 And Not bEffortAdjusted Then Me.AdjustTotalEffort()
                    stpwchEffort.Start()
                    If Me.m_Data.bUseEffortDistThreshold Then
                        'Run Effort Distribtion on cells with sailing cost < EffortDistThreshold
                        'this version also shares the load between threads
                        Me.runEffortDistributionLoadShared(m_Data.MonthNow, its)
                    Else
                        'Run Effort Distribtion on all map cells
                        Me.runEffortDistributionNoLoadShare(m_Data.MonthNow, its)
                    End If
                    stpwchEffort.Stop()
                End If

                If m_pluginManager IsNot Nothing Then m_pluginManager.EcospacePostFishingEffortModTimestep(m_Data, itt)

                ReDim Btime(m_Data.NGroups) 'this clears out btime
                ReDim ConTotal(m_Data.NGroups)

#If False Then
                Me.m_Data.debugTestDiscardsMaps()
#End If
                Array.Clear(Me.m_Data.CatchMap, 0, Me.m_Data.CatchMap.Length)
                Array.Clear(Me.m_Data.CatchFleetMap, 0, Me.m_Data.CatchFleetMap.Length)
                Array.Clear(Me.m_Data.Landings, 0, Me.m_Data.Landings.Length)
                Array.Clear(Me.m_Data.DiscardsMap, 0, Me.m_Data.DiscardsMap.Length)

                If Me.m_tracerData.EcoSpaceConSimOn Then
                    'drive contaminant concentration with external data
                    Me.m_TimeSeriesManager.ForceContaminantConcentrations(its)
                End If
                '*************
                'UPDATE SOLVERS WITH NON REFERENCED TIMESTEP DATA (itt, etc)
                '*************
                UpdateSpaceSolverThreads(m_Data.YearNow)
                stpwchSolver.Start()
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Run the biomass calculation for each spatial cell at this time step
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'System.Console.Write("T = " + itt.ToString + ", ")
                runSpaceSolverThreads()
                stpwchSolver.Stop()

                'now solve the spatial grid
                stpwchGrid.Start()
                runGridSolverThreads()
                stpwchGrid.Stop()

                'Debugging dump grid CPU times to the console
                'dumpGridRunTimes()

                'Debugging discards map dumps discards to console window
                'Me.m_Data.debugTestDiscardsMaps()

                'make sure none of the biomass cells are zero
                For ip = 1 To m_Data.nvartot
                    For i = 1 To m_Data.InRow
                        For j = 1 To m_Data.InCol
                            If Single.IsNaN(Me.m_Data.Bcell(i, j, ip)) Then
                                m_Data.Bcell(i, j, ip) = 1.0E-30
                            End If
                            If m_Data.Bcell(i, j, ip) < 1.0E-30 Then m_Data.Bcell(i, j, ip) = 1.0E-30
                        Next j
                    Next i
                Next 'For ip = 1 To m_Data.nvartot


                'For ip = 1 To m_Data.nvartot
                '    '********following will bypass unneeded solvegrid calls when NewMultiStanza=true
                '    If m_Data.ByPassIntegrate(ip) = False Then
                '        SolveGrid(ip, AMm, F, m_Data.Bcell, m_Data.Inrow, m_Data.InCol, Tol, jord, m_Data.W)
                '        SolveGridRow(ip, AMm, F, m_Data.Bcell, m_Data.Inrow, m_Data.InCol, Tol, jord, m_Data.W)
                '        For i = 1 To m_Data.Inrow : For j = 1 To m_Data.InCol
                '                If m_Data.Bcell(i, j, ip) < 1.0E-30 Then m_Data.Bcell(i, j, ip) = 1.0E-30
                '            Next : Next
                '    End If
                'Next

                Dim tbio() As Single

                'update total age structure over space for multistanza groups if new method is used
                If m_Data.NewMultiStanza Then
                    SpaceSplitUpdate()  'update overall population age structure using total loss, consumption added over grid cells
                    'then distribute updated biomasses over the spatial grid
                    'The following code (for isp...next isp) is real Rambo shit, really should be put
                    'in its own subroutine called 'DistributeMultiStanzaBiomass' so we
                    'can improve it later with more complex spatial redistribution
                    'rules eg running an IBM to predict movement among cells
                    Dim ieco As Integer


                    ReDim tbio(Me.m_Data.NGroups)
                    For isp As Integer = 1 To m_Stanza.Nsplit
                        For ist As Integer = 1 To m_Stanza.Nstanza(isp)
                            ieco = m_Stanza.EcopathCode(isp, ist)
                            '***WARNING**** FOLLOWING CALCULATION WILL FAIL IF ADJUSTSPACEPARS HAS NOT BEEN CALLED
                            'SINCE CALCTOTAREA WILL NOT HAVE BEEN CALLED AND NEITHER THABAREA OR HABAREAUSED WILL HAVE BEEN
                            'SET
                            Tbiom = m_Data.ThabArea * Blocal(ieco)  'B has been updated in spacesplitupdate at this point
                            Tpred = m_Data.ThabArea * m_SimData.pred(ieco)  'pred has been updated by call to splitsetpred in spacesplitupdate

                            'Tbiom = Me.m_Data.TotHabCap(ieco) * Blocal(ieco)  'B has been updated in spacesplitupdate at this point
                            'Tpred = Me.m_Data.TotHabCap(ieco) * m_SimData.pred(ieco)  'pred has been updated by call to splitsetpred in spacesplitupdate
                            For i = 1 To m_Data.InRow
                                For j = 1 To m_Data.InCol
                                    If m_Data.Depth(i, j) > 0 Then
                                        Wcell = m_Data.IFDweight(i, j, ieco) / TotIFDweight(ieco)
                                        m_Data.Bcell(i, j, ieco) = Tbiom * Wcell
                                        tbio(ieco) = tbio(ieco) + m_Data.Bcell(i, j, ieco)
                                        m_Data.PredCell(i, j, ieco) = Tpred * Wcell
                                    End If
                                Next j
                            Next i
                        Next ist
                    Next isp

                ElseIf m_Data.UseIBM Then
                    'IBM model
                    runIBMSolverThreads()

                End If 'end of section to overwrite PDE biomasses with multistanza distributed biomasses if newmultistanza=true

                'sum biomass after Multistanza updates
                Array.Clear(Btime, 0, Btime.Length)

                'Set any biomass forced by the Spatial Temporal Biomass forcing layer
                'back to the forced values
                Me.RestoreForcedBiomass()

                'Force Biomass with Ecosim forcing time series
                Me.ForceBiomassWithEcosimTimeSeries(its)

                For ip = 0 To m_Data.NGroups
                    For i = 1 To m_Data.InRow
                        For j = 1 To m_Data.InCol
                            'jb 12-July-2013 Added Depth check and removed width multiplier
                            'This fixes a bug in the Ecospace Results grid were biomass was not matching Ecopath base with large spatial models
                            If Me.m_Data.Depth(i, j) > 0 Then
                                Btime(ip) += m_Data.Bcell(i, j, ip)
                                'By Region
                                irgn = Me.m_Data.Region(i, j)
                                If (irgn > Me.m_Data.nRegions) Then irgn = 0
                                Me.m_Data.ResultsRegionGroup(irgn, ip, itt) += Me.m_Data.Bcell(i, j, ip)
                            End If
                        Next j
                    Next i
                    'Debug.Assert(ip <> 67)
                    'Average across all the cells
                    Btime(ip) /= m_Data.nWaterCells
                    If Btime(ip) = 0 Then Btime(ip) = 0.0000000001

                    For irgn = 0 To Me.m_Data.nRegions
                        Me.m_Data.ResultsRegionGroup(irgn, ip, itt) /= Me.m_Data.nCellsInRegion(irgn)
                        Me.m_Data.ResultsRegionGroupYear(irgn, ip, Me.m_Data.YearNow) += Me.m_Data.ResultsRegionGroup(irgn, ip, itt)
                        If ((itt Mod Me.m_Data.nTimeStepsPerYear) = 0) Then
                            Me.m_Data.ResultsRegionGroupYear(irgn, ip, Me.m_Data.YearNow) /= Me.m_Data.nTimeStepsPerYear
                        End If
                    Next irgn

                Next ip

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'contaminant tracing
                If m_tracerData.EcoSpaceConSimOn Then
                    Dim itc As Integer, ntc As Integer
                    Dim Derivcon(,,) As Single, Derivcon2(,,) As Single
                    Dim totderivcon As Single
                    ReDim Derivcon(m_Data.InRow, m_Data.InCol, m_Data.NGroups)
                    ReDim Derivcon2(m_Data.InRow, m_Data.InCol, m_Data.NGroups)

                    ntc = Me.estimateMaxTimestep()

                    Me.runContaminantTracerExplicit1(Derivcon, Derivcon2, ntc)

                    For itc = 2 To ntc
                        Me.runSpaceCSolverThreads(CSng(ntc))

                        'Me.runContaminantTracerSolveGrid()
                        Me.runContaminantTracerExplicit1(Derivcon, Derivcon2, ntc)

                        totderivcon = 0
                        For ip = 0 To m_Data.NGroups
                            For i = 1 To m_Data.InRow
                                For j = 1 To m_Data.InCol
                                    totderivcon = totderivcon + Derivcon(i, j, ip)
                                Next
                            Next
                        Next

                    Next
                    Me.summarizeContaminantTracer()
                    itc = ntc
#If Dumbcb Then
                        CoutVals(0) = CStr(itt) & ","
                        For i = 1 To m_Data.InRow
                            For j = 1 To m_Data.InCol
                                CoutVals(1) = CStr(i) & ”,”
                                CoutVals(2) = CStr(j) & ”,”
                                CoutVals(3) = m_Data.Ccell(i, j, 0) & ”,”
                                For ip = 1 To m_Data.NGroups
                                    CoutVals(3 + ip) = CStr(m_Data.Ccell(i, j, ip) / m_Data.Bcell(i, j, ip)) & ”,”
                                Next
                                For ii As Integer = 0 To 3 + m_Data.NGroups
                                    CoutWriter.Write(CoutVals(ii))
                                Next
                                CoutWriter.WriteLine()
                            Next
                        Next
#End If
                End If 'm_tracerData.EcoSpaceConSimOn 
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                'Fit to loaded time series data
                Me.AccumulateFitStats(itt, Me.m_Data.Bcell)

                If itt = 1 Then
                    Me.setBaseValues(itt)
                End If

                'Update the Biomass results with the spatially averaged values
                Me.updateBiomassResults(itt)

                Me.calcValue(itt, m_Data.YearNow)

                If m_search.bInSearch And m_Data.YearNow = m_search.BaseYear And m_Data.MonthNow = 12 Then
                    m_search.calcBaseYearCost(m_Data.YearNow, m_Data.nWaterCells)
                End If

                'GC.Collect()
                'post notification that a time step has been completed
                marshallOnTimeStep(itt)

                If m_pluginManager IsNot Nothing Then m_pluginManager.EcospaceEndTimeStep(m_Data, itt)

                'System.Console.WriteLine("FindSpatialEquilibrium() SpaceSolver run time(min.) = " & stpwchSolver.Elapsed.TotalMinutes.ToString)
                'System.Console.WriteLine("FindSpatialEquilibrium() GridSolver run time(min.) = " & stpwchGrid.Elapsed.TotalMinutes.ToString)
                'System.Console.WriteLine("FindSpatialEquilibrium() PredictEffortDistribution run time(min.) = " & stpwchEffort.Elapsed.TotalMinutes.ToString)
                'System.Console.WriteLine("FindSpatialEquilibrium() Total run time(min.) = " & stpwchTotRunTime.Elapsed.TotalMinutes.ToString)

            Next m_Data.TimeNow
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'END OF TIME LOOP
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxx

#If Dumpcb Then
            CoutWriter.Flush()
            CoutWriter.Close()
            CoutWriter.Dispose()
#End If
            ' Me.m_Data.AverageSpatialResults()
            Me.m_Data.SummarizeResultsByFleet(itt, Me.m_EPdata.cost, Me.m_search.Jobs)

            If m_search.bInSearch Then
                Dim runTime As Integer = CInt(itt * m_Data.TimeStep)
                Dim RuntimePB As Integer = runTime
                If m_search.BaseYear > m_OptMPA.EcospaceStartTime Then RuntimePB = m_OptMPA.MPAOptData.EcoSpaceEndYear - m_search.BaseYear
                m_search.EcoSpaceSummarizeIndicators(Fgear, runTime, RuntimePB, m_Data.nWaterCells)
            End If

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'jb Stats now done by EcospaceTimeSeriesManager()
            'Dim SpaceSS As Single
            ''If SpDatYear > 0 Then 'there is time series data so calculate SS SpSS
            'SpaceSS = CalculateSpaceSS()
            ''End If

            'm_Ecosim.PlotDataInfo(False, m_Data.SS)
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            Me.m_TimeSeriesManager.RunCompleted()

            Dim totalIter As Single
            For i = 1 To m_Data.nGridSolverThreads
                totalIter = totalIter + totalIterThread(i)
            Next

            'Always turn OFF the TrophicLevel calculations
            'so it does not create unnecessary overhead
            Me.m_Data.bCalTrophicLevel = False

            stpwchTotRunTime.Stop()
            Dim totRunTime As Double = stpwchTotRunTime.Elapsed.TotalMinutes
            Dim SpaceRunTime As Double = stpwchSolver.Elapsed.TotalMinutes
            Dim GridRunTime As Double = stpwchGrid.Elapsed.TotalMinutes
            Dim EffortRunTime As Double = stpwchEffort.Elapsed.TotalMinutes

            System.Console.WriteLine("---------------FindSpatialEquilibrium() Timing-------------")
            System.Console.WriteLine(" Number of Time Steps, " & itt.ToString)
            System.Console.WriteLine(" Total run time(min.), " & totRunTime.ToString)
            System.Console.WriteLine(" Average per Timestep(min.), " & (totRunTime / itt).ToString)
            System.Console.WriteLine(" Trophic time(min.), " & SpaceRunTime.ToString & ",(%)," & (SpaceRunTime / totRunTime * 100).ToString)
            System.Console.WriteLine(" GridSolver time(min.), " & GridRunTime.ToString & ",(%)," & (GridRunTime / totRunTime * 100).ToString)
            System.Console.WriteLine(" Effort dist. time(min.), " & EffortRunTime.ToString & ",(%)," & (EffortRunTime / totRunTime * 100).ToString)
            System.Console.WriteLine("-----------------------------------------------------------")

        Catch ex As Exception
            cLog.Write(ex, "cEcospace::FindSpatialEquilibrium")
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException("FindSpatialEquilibrium() Error: " & ex.Message, ex)
        End Try

    End Sub

    ''' <summary>
    ''' One Off to pause the model at a fixed timestep to allow the user to grab maps, plots...
    ''' </summary>
    ''' <param name="month"></param>
    ''' <remarks></remarks>
    Private Sub HACKAutoPause(month As Integer)
        Try
            Debug.Assert(False, "Warning Auto Pause has been called. Are you sure you want to do this.")
            Dim CalendarYear As Integer = Me.m_Data.YearNow - 1 + Me.m_EPdata.FirstYear
            'MonthNow has not been update yet so it is one month behind the actually month
            If (CalendarYear = 2029 Or CalendarYear = 2059) And Me.m_Data.MonthNow = 10 Then
                Dim msg As New Text.StringBuilder()
                msg.AppendLine("Auto paused Year = " & CalendarYear.ToString & " Month = " & (Me.m_Data.MonthNow + 1).ToString)
                msg.AppendLine("To restart the run click the Pause or Resume button once.")
                Me.isPaused = True
                Microsoft.VisualBasic.MsgBox(msg.ToString, Microsoft.VisualBasic.MsgBoxStyle.Information, "HACK WARNING")
            End If
        Catch ex As Exception
            'Ehhh that's the breaks ehhh
        End Try
    End Sub


    ''' <summary>
    ''' Set time step index counters. This includes setting the counters for the Spin-Up period.
    ''' </summary>
    ''' <param name="iCumTimeStep">Cumulative time step counter.</param>
    ''' <param name="iDataTimeStep">Time step counter use to access data arrayed on cumulative month.</param>
    ''' <remarks>This also sets YearNow and MonthNow yearly and monthly indexes.</remarks>
    Private Sub setTimeStepCounters(ByRef iCumTimeStep As Integer, ByRef iDataTimeStep As Integer)

        'The cumulative timestep counter
        iCumTimeStep += 1

        'Increment the SpinUp counters
        Me.iSpinUp += 1
        Me.iSpinUpYear += 1

        'If in a Spin Up Period then...
        If Me.m_Data.bInSpinUp Then

            If Me.iSpinUp <= Me.nSpinUp Then
                'Still in a Spin-Up period

                'Recycle through the first year
                If Me.iSpinUpYear > Me.nSpinUpYear Then
                    'Set counters and time back to the start
                    Me.iSpinUpYear = 1
                    iCumTimeStep = 1
                    m_Data.TimeNow = 0
                End If

            Else 'Me.iSpinUp <= Me.nSpinUp T
                'Spin-Up period ended
                Me.m_Data.bInSpinUp = False

                'Re-start from the first time step
                iCumTimeStep = 1
                m_Data.TimeNow = 0

                'Clear out the results that were gathered during the Spin-Up
                Me.m_Data.redimTimeStepResults(nEcospaceTimeSteps)

            End If 'Me.iSpinUp <= Me.nSpinUp T

        End If 'm_Data.bInSpinUp 

        'The cumulative MONTHLY counter used for data arrayed by month i.e. zscale()
        If Me.m_Data.nTimeStepsPerYear <> cCore.N_MONTHS Then
            iDataTimeStep = Math.Truncate((m_Data.TimeNow + 0.0000000001) * 12) + 1
        Else
            iDataTimeStep = iCumTimeStep
        End If

        'make sure the data array index (its in the main loop) do not get larger than the data they reference
        If iDataTimeStep > m_SimData.ForcePoints Then iDataTimeStep = m_SimData.ForcePoints 'HACK  bump back the index
        If iDataTimeStep > m_SimData.NTimes Then iDataTimeStep = m_SimData.NTimes

        'MonthNow will be truncated to monthly(decimal) part of TimeNow
        m_Data.MonthNow = Math.Truncate(1.0F + (m_Data.TimeNow - Math.Truncate(m_Data.TimeNow)) * 12.0F)
        'YearNow will be truncated to the integer part of timenow
        m_Data.YearNow = 1 + Math.Truncate(m_Data.TimeNow)
        If m_Data.YearNow > Math.Truncate(Me.m_Data.TotalTime) Then m_Data.YearNow = Math.Truncate(Me.m_Data.TotalTime)

        'xxxxxxxxxxxxxxxxxxxxxxxxx
        'For debugging
        'The time loop counter (iCumTimeStep) and the data time step counter (iDataTimeStep)
        'should be the same when nTimeStepsPerYear = 12
        'Not otherwise
        'Debug.Assert(iDataTimeStep = iCumTimeStep)
        'xxxxxxxxxxxxxxxxxxxxxxxxx

    End Sub

    ''' <summary>
    ''' Call Ecosim.AccumulateDataInfo() to gather the fit to time series stats once a year at the end of the sixth month
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub AccumulateFitStats(iTimeStep As Integer, Biomass(,,) As Single)

        If m_TimeSeriesManager.ContainsData(eVarNameFlags.EcospaceMapBiomass) Then
            Me.m_TimeSeriesManager.CalculateStats(iTimeStep, Biomass)
        End If

        Return

    End Sub


    Private Sub BeginTimeStep(ByRef Fgear() As Single, ByVal its As Integer, ByVal imonth As Integer, ByRef iYear As Integer, ByRef BiomassCellAvg() As Single, ByVal relfopt() As Single, ByVal TimeStep As Single)
        Try
            Dim nYears As Integer = CInt(m_Data.TotalTime)
            'clear out catch and landings data at the start of each timestep
            'Array.Clear(Me.m_Data.CatchMap, 0, Me.m_Data.CatchMap.Length)
            'Array.Clear(Me.m_Data.Landings, 0, Me.m_Data.Landings.Length)
            'Array.Clear(Me.m_Data.DiscardsMap, 0, Me.m_Data.DiscardsMap.Length)

            If m_pluginManager IsNot Nothing Then m_pluginManager.EcospaceBeginTimeStep(m_Data, itt)

            If imonth = 1 Then
                'if we are in the first month then this is a new year
                If m_search.bInSearch Then
                    'YearTimeStepEcoSpace() will compute DF, Fgear(), NetCost(), and FishYear() for this year step

                    m_search.YearTimeStepEcoSpace(BiomassCellAvg, Fgear, iYear, m_Data.nWaterCells, relfopt)
                    m_search.calcNetCost(Fgear, iYear)
                    m_search.calcYearlySummaryValues(BiomassCellAvg)

                End If

                'tell all the space solver threads that a new year has started
                InitSolversForYear(iYear)

            End If

        Catch ex As Exception
            Debug.Assert(False, ex.StackTrace)
            cLog.Write(ex, "cEcospace::BeginTimeStep")
            Throw New ApplicationException("EcoSpace.BeginTimeStep() error: " & ex.Message, ex)
        End Try

    End Sub

    Private Sub SetSpatialTempData(ByVal iTimeStepCounter As Integer)
        Try

            ' Apply Ecospace datasources
            ' * This will need to become much more sophisticated
            If (Me.m_SpatialData IsNot Nothing) Then
                For Each src As cSpatialDataAdapter In Me.m_SpatialData.DataAdapters
                    If (src IsNot Nothing) Then
                        Try
                            src.Populate(iTimeStepCounter, cCore.NULL_VALUE)
                        Catch ex As Exception
                            cLog.Write(ex, "cEcospace.SetSpatialTempData " & src.Name & "(" & src.Index & ")")
                        End Try
                    End If
                Next
            End If

        Catch ex As Exception
            '  Debug.Assert(False, ex.StackTrace)
            cLog.Write(ex, "cEcospace.SetSpatialTempData()")
            Me.Messages.AddMessage(New cMessage("Ecospace Failed to read external data.", eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Critical))
        End Try
    End Sub


    Private Sub RestoreForcedBiomass()

        Try

            If (Me.m_SpatialData IsNot Nothing) Then
                For Each src As cSpatialDataAdapter In Me.m_SpatialData.DataAdapters
                    If (src IsNot Nothing) Then
                        Try
                            src.RestoreForcing(Me.m_Data)
                        Catch ex As Exception
                            cLog.Write(ex, "cEcospace.SetSpatialTempData " & src.Name & "(" & src.Index & ")")
                        End Try
                    End If
                Next
            End If

        Catch ex As Exception
            '  Debug.Assert(False, ex.StackTrace)
            cLog.Write(ex, "cEcospace.SetSpatialTempData()")
            Me.Messages.AddMessage(New cMessage("Ecospace Failed to read external data.", eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Critical))
        End Try


        'Dim i As Integer

        'Try

        '    For Each pair As cForcingMapIndexPair In Me.m_Data.ForcingMaps
        '        i += 1
        '        If pair IsNot Nothing Then
        '            'Only restore the biomass if this timestep was forced
        '            'If not leave the predicted biomass in place for the next timestep
        '            If pair.isTimeStepForced Then
        '                Try
        '                    For ir As Integer = 1 To Me.m_Data.InRow
        '                        For ic As Integer = 1 To Me.m_Data.InCol
        '                            If Me.m_Data.Depth(ir, ic) > 0 Then
        '                                Me.m_Data.Bcell(ir, ic, pair.iLayerIndex) = pair.data(ir, ic)
        '                            End If
        '                        Next ic
        '                    Next ir

        '                Catch ex As Exception
        '                    Debug.Assert(False, "Oppss... " + ex.Message)
        '                    cLog.Write(ex, "Failed to restore forced biomass for group " + i.ToString)
        '                End Try
        '            End If

        '            'Re-set the isTimeStepForced Flag
        '            'this will be set to True next time the adapter loads data for a timestep
        '            pair.isTimeStepForced = False

        '        End If 'If pair IsNot Nothing Then
        '    Next pair

        'Catch ex As Exception

        'End Try


    End Sub

    ''' <summary>
    ''' VC stole this from Ecosim 
    ''' Force Ecospace Biomass with Ecosim time series forced biomass 
    ''' </summary>
    Private Sub ForceBiomassWithEcosimTimeSeries(iTime As Integer)

        'System.Console.WriteLine("WARNING: Ecospace biomass forced by Ecosim biomass forcing!")

        Try

            If Not Me.m_Data.UseEcosimBiomassForcing Then
                'User has turned OFF the Ecosim Biomass forcing
                Exit Sub
            End If

            Dim iForcingIndex As Integer = Me.m_refdata.toForcingTimeStep(iTime, m_Data.YearNow)

            Dim SumB As Single

            For ip As Integer = 1 To m_Data.NGroups

                'Now we can force the biomass:
                'loop over the Ecosim time series and find the group with forced biomass for this time step
                'AAAAH Group 1 is forced (as an example, so let's force the biomass of this group
                'If ip = 1 Then
                If Me.m_Data.IsEcosimBioForcingGroup(ip) Then

                    'jb Only if there is valid data for this timestep
                    If Me.EcoSim.TimeSeriesData.PoolForceBB(ip, iForcingIndex) > 0 Then
                        SumB = 0

                        Dim WaterCells As Integer = 0
                        'this group has forced biomass in Ecosim timeseries
                        For i As Integer = 1 To m_Data.InRow
                            For j As Integer = 1 To m_Data.InCol
                                'jb 12-July-2013 Added Depth check and removed width multiplier
                                'This fixes a bug in the Ecospace Results grid were biomass was not matching Ecopath base with large spatial models

                                'VC 10-June-2016 The calculation below actually should scale based on area of cell, so cell width
                                'we should revisit and fix the bug that Joe talkes about above
                                'JB the biomass values are in kg/k I think that means the size of the cell is irrelevant
                                '****************** SEE ABOVE  ************************
                                If Me.m_Data.Depth(i, j) > 0 Then
                                    SumB += m_Data.Bcell(i, j, ip)
                                    WaterCells += 1
                                End If
                            Next j
                        Next i

                        Dim BForced As Single = 0
                        Dim sumForcedB As Single = 0 'for debugging
                        Dim BMeanScalar As Single = WaterCells / SumB
                        'fb = 0.08 * WaterCells ' * (1 - m_Data.TimeNow / m_Data.TotalTime) 
                        'jb get the forced biomass value
                        BForced = Me.EcoSim.TimeSeriesData.PoolForceBB(ip, iForcingIndex)

                        For i As Integer = 1 To m_Data.InRow
                            For j As Integer = 1 To m_Data.InCol
                                'Villy
                                'If Me.m_Data.Depth(i, j) > 0 And SumB > 0 Then
                                '    m_Data.Bcell(i, j, ip) = m_Data.Bcell(i, j, ip) * fb / SumB
                                'End If

                                'jb
                                If Me.m_Data.Depth(i, j) > 0 Then
                                    'jb version 29-June-2016
                                    ' m_Data.Bcell(i, j, ip) = (WaterCells / SumB) * fb * m_Data.Bcell(i, j, ip)
                                    m_Data.Bcell(i, j, ip) = BMeanScalar * BForced * m_Data.Bcell(i, j, ip)
                                    sumForcedB += m_Data.Bcell(i, j, ip)
                                End If
                            Next j
                        Next i

                        ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        ''Debugging the mean ecospace biomass should match the forcing value
                        'System.Console.WriteLine("Ecospace Forced B grp=" + ip.ToString + " BForced/BMean=" + (BForced / (sumForcedB / WaterCells)).ToString)
                        'Debug.Assert(Math.Round(BForced / (sumForcedB / WaterCells), 3) = 1.0, "ForceBiomassWithEcosimTimeSeries(...) did not set forced biomass correctly")
                        ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    End If 'Me.EcoSim.TimeSeriesData.PoolForceBB(ip, iForcingIndex)
                End If ' Me.m_Data.IsEcosimBioForcing(ip)
            Next ip

        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' Update the Depth(,) map used by Ecospace with the Excluded layer
    ''' </summary>
    Friend Sub UpdateDepthMap()
        For i As Integer = 1 To m_Data.InRow
            For j As Integer = 1 To m_Data.InCol
                If Me.m_Data.Excluded(i, j) Or (Me.m_Data.DepthInput(i, j) = cCore.NULL_VALUE) Then
                    Me.m_Data.Depth(i, j) = cCore.NULL_VALUE
                Else
                    Me.m_Data.Depth(i, j) = Math.Max(0, Me.m_Data.DepthInput(i, j))
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Exclude cells at and past a specified depth.
    ''' </summary>
    ''' <param name="iDepth">The minimum depth to exclude.</param>
    Friend Sub SetExcludedDepth(iDepth As Integer)
        For i As Integer = 1 To m_Data.InRow
            For j As Integer = 1 To m_Data.InCol
                Me.m_Data.Excluded(i, j) = (Me.m_Data.DepthInput(i, j) >= iDepth)
            Next
        Next
        Me.UpdateDepthMap()
    End Sub

    ''' <summary>
    ''' Reset the map of excluded cells to include the entire map into computations.
    ''' </summary>
    Friend Sub ClearExcludedCells()
        For i As Integer = 1 To m_Data.InRow
            For j As Integer = 1 To m_Data.InCol
                Me.m_Data.Excluded(i, j) = False
            Next
        Next
        Me.UpdateDepthMap()
    End Sub

    ''' <summary>
    ''' Inverts the map of excluded cells.
    ''' </summary>
    Friend Sub InvertExcludedCells()
        For i As Integer = 1 To m_Data.InRow
            For j As Integer = 1 To m_Data.InCol
                Me.m_Data.Excluded(i, j) = Not Me.m_Data.Excluded(i, j)
            Next
        Next
        Me.UpdateDepthMap()
    End Sub

    Private Sub EcoseedBeginTimeStep(ByVal imonth As Integer, ByRef iYear As Integer, ByRef BiomassCellAvg() As Single)

        If m_OptMPA IsNot Nothing Then

            If m_OptMPA.isRunning Then
                'if we are in the first month then this is a new year
                If imonth = 1 Then
                    'Call Ecoseed at the start of each year
                    'On the first call EcoSeed will
                    'set iYear to the user defined Start year 
                    'populate BiomassCellAvg() with biomass values for the start year calculated by Ecospace during Ecoseed initialization
                    'If iYear = Ecoseed end year then it will set Ecospace.StopRun to true 
                    'this will cause the time loop in Ecospace to exit
                    m_OptMPA.YearTimeStep(iYear, Btime)
                End If
            End If

        End If

    End Sub


    Private Sub runIBMSolverThreads()
        Dim solver As cIBMSolver
        Dim iFstGrp As Integer
        Dim iLstgrp As Integer
        Dim iFirstPacket As Integer
        Dim iLastPacket As Integer

        iFstGrp = 1
        iLstgrp = 0
        iFirstPacket = 1
        iLastPacket = 0

        Dim solvCtr As Integer = 1

        For Each solver In m_IBMSolvers
            ReDim solver.BcellThread(m_Data.InRow, m_Data.InCol, m_Data.nvartot)
            ReDim solver.PredCellThread(m_Data.InRow, m_Data.InCol, m_Data.nvartot)
        Next
        ReDim m_Stanza.EggCell(m_Data.InRow, m_Data.InCol, m_Stanza.Nsplit)

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'jb 18-Nov-2011 Changed calling order of GrowSurvivePackets() and MovePackets()
        're Carls email
        'Yes, assuming that the IBM updates are done after all the solvegrid calls (as in ewe5)
        ' then growsurvivepackets must be called before movepackets.  
        'Otherwise the growth-survival calculations for each packet will be based on the cell position after the move, 
        'rather than the cell position used to predict food consumption and mortality rates from derivt in the solvegrid loop.
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx 

        Try

            'this loop should only excecute once
            Do While iLstgrp < m_Stanza.Nsplit
                'loop through each solver object, make sure it's okay to run, and run it
                'each thread will do several groups at a time
                For Each solver In m_IBMSolvers

                    If solver.isOkToRun Then

                        iLstgrp = iFstGrp + m_Data.nIBMGroupsPerThread - 1
                        If iLstgrp > m_Stanza.Nsplit Then iLstgrp = m_Stanza.Nsplit

                        solver.FirstLastGroups(iFstGrp, iLstgrp)
                        solver.SignalState.Reset()

                        solver.isOkToRun = False
                        ThreadPool.QueueUserWorkItem(AddressOf solver.runGrowSurvivePackets)

                        iFstGrp += m_Data.nIBMGroupsPerThread
                    Else
                        'System.Console.WriteLine("Solver thread blocked ID:" & solver.ThreadID & " Group:" & solver.iFirstIndex & " time:" & m_Data.TimeNow)
                    End If

                    If iLstgrp >= m_Stanza.Nsplit Then
                        Exit For
                    End If
                Next solver
            Loop

            ' wait for all the threads to finish before starting the next time step
            For Each solver In m_IBMSolvers
                solver.SignalState.WaitOne()
            Next

            'this loop should only excecute once
            Do While iLastPacket < m_Stanza.Npackets
                'loop through each solver object, make sure it's okay to run, and run it
                'each thread will do several groups at a time

                For Each solver In m_IBMSolvers

                    If solver.isOkToRun Then

                        iLastPacket = iFirstPacket + m_Data.nIBMPacketsPerThread - 1
                        If iLastPacket > m_Stanza.Npackets Then iLastPacket = m_Stanza.Npackets

                        'solver.FirstLastGroups(iFstGrp, iLstgrp)
                        solver.iFirstPacket = iFirstPacket
                        solver.iLastPacket = iLastPacket
                        solver.SignalState.Reset()

                        solver.isOkToRun = False
                        ThreadPool.QueueUserWorkItem(AddressOf solver.runMovePackets)

                        iFirstPacket += m_Data.nIBMPacketsPerThread
                    Else
                        'System.Console.WriteLine("Solver thread blocked ID:" & solver.ThreadID & " Group:" & solver.iFirstIndex & " time:" & m_Data.TimeNow)
                    End If

                    If iLastPacket >= m_Stanza.Npackets Then
                        Exit For
                    End If
                Next solver
            Loop

            ' wait for all the threads to finish before starting the next time step
            For Each solver In m_IBMSolvers
                solver.SignalState.WaitOne()
            Next

            Dim ieco As Integer
            For isp As Integer = 1 To m_Stanza.Nsplit
                For ist As Integer = 1 To m_Stanza.Nstanza(isp)
                    ieco = m_Stanza.EcopathCode(isp, ist)
                    For i As Integer = 1 To m_Data.InRow
                        For j As Integer = 1 To m_Data.InCol
                            m_Data.Bcell(i, j, ieco) = 0
                            m_Data.PredCell(i, j, ieco) = 0
                        Next j
                    Next i
                Next ist
            Next isp

            For Each solver In m_IBMSolvers
                For isp As Integer = 1 To m_Stanza.Nsplit
                    For ist As Integer = 1 To m_Stanza.Nstanza(isp)
                        ieco = m_Stanza.EcopathCode(isp, ist)
                        For i As Integer = 1 To m_Data.InRow
                            For j As Integer = 1 To m_Data.InCol
                                m_Data.Bcell(i, j, ieco) = m_Data.Bcell(i, j, ieco) + solver.BcellThread(i, j, ieco)
                                m_Data.PredCell(i, j, ieco) = m_Data.PredCell(i, j, ieco) + solver.PredCellThread(i, j, ieco)
                            Next j
                        Next i
                    Next ist
                Next isp
            Next solver

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException("Error in runIBMSolverThreads()", ex)
        End Try


    End Sub

    Private Sub runGridSolverThreads()
        Dim solver As cGridSolver
        Dim iFstGrp As Integer
        Dim iLstgrp As Integer
        Dim nRunning As Integer

        iFstGrp = 1
        iLstgrp = 0

        'debugDumpAverageB(44, "Before Grid")
        Try

            'Create one WaitHandle and pass it to all the threads
            'Once the cSpaceSolver.ThreadIncrementer hits zero the thread(cSpaceSolver) object will call ManualResetEvent.Set()  
            'this allows any waiting threads to continue
            Dim WaitOb As ManualResetEvent = New ManualResetEvent(False)

            Dim stpTotRun As Stopwatch = Stopwatch.StartNew

            'set the shared thread increment counter to the number of threads
            cGridSolver.ThreadIncrementer = m_gridSolvers.Count

            For Each solver In m_gridSolvers
                nRunning += 1
                'same wait object for all the threads
                'Once the thread counter has been Decrement to Zero
                'the Wait object will be released by the solver
                solver.WaitHandle = WaitOb

                solver.FirstLastGroups(1, nGroupsInThread(solver.ThreadID))

                'Dim worker As Thread = New Thread(AddressOf solver.Solve)
                'worker.Start()
                ThreadPool.QueueUserWorkItem(AddressOf solver.Solve)

            Next solver

            'The WaitObject will be signaled once the threads have counted down the ThreadIncrementer to zero
            If Not WaitOb.WaitOne() Then
                Debug.Assert(False, "runGridSolverThreads() Timed out!")
                cLog.Write(Me.ToString & ".runSpaceSolverThreads() Timed out.")
            End If

            stpTotRun.Stop()
            'debugDumpAverageB(44, "After Grid")
            'System.Console.WriteLine("Grid wall run time (sec), " & stpTotRun.Elapsed.TotalSeconds.ToString)

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException("Error in runSolverThreads()", ex)
        End Try

    End Sub

    Private Sub debugDumpAverageB(GrpIndex As Integer, msg As String)
        Dim sumb As Double
        Dim n As Integer

        Try

            For ir As Integer = 1 To Me.m_Data.InRow
                For ic As Integer = 1 To Me.m_Data.InCol
                    If Me.m_Data.Depth(ir, ic) > 0 Then
                        sumb += Me.m_Data.Bcell(ir, ic, GrpIndex)
                        n += 1
                    End If
                Next
            Next

            System.Console.WriteLine(msg + ", " + GrpIndex.ToString + ", " + (sumb / n).ToString)

        Catch ex As Exception

        End Try


    End Sub
    Private Sub runSpaceSolverThreads()
        Dim solver As cSpaceSolver
        Dim iFrstCell As Integer
        Dim iLstCell As Integer
        Dim etRunTime As Double

        Array.Clear(Btime, 0, Btime.Length)
        Array.Clear(TotLoss, 0, TotLoss.Length)
        Array.Clear(TotEatenBy, 0, TotEatenBy.Length)
        Array.Clear(TotBiom, 0, TotBiom.Length)
        Array.Clear(TotPred, 0, TotPred.Length)
        Array.Clear(TotIFDweight, 0, TotIFDweight.Length)

        iFrstCell = 1
        iLstCell = 0

        ' Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId
        ' Console.WriteLine("Ecospace ThreadID = " & thrdID.ToString)

        'Create one WaitHandle and pass it to all the threads
        'Once the cSpaceSolver.ThreadIncrementer hits zero the thread(cSpaceSolver) object will call ManualResetEvent.Set()  
        'this allows any waiting threads to continue
        Dim WaitOb As ManualResetEvent = New ManualResetEvent(False)
        Dim stpTotRun As Stopwatch = Stopwatch.StartNew

        Try

            'set the shared thread increment counter to the number of threads
            cSpaceSolver.ThreadIncrementer = m_spaceSolvers.Count

            'Number of cells to compute for the current thread
            Dim nCells As Integer
            'Total number of cells that have been computed
            Dim nCellsCompleted As Integer
            'Loop counter
            Dim iSolve As Integer

            'loop through each solver, create a thread for it, and run it
            For Each solver In m_spaceSolvers
                iSolve += 1
                'Compute the work load for each thread on the fly
                'this prevents any rounding weirdness that could cause cSpaceSolver.ThreadIncrementer to not hit Zero
                'Causing a deadlock on WaitOb.WaitOne()
                nCells = Me.computeThreadLoad(m_Data.iTotalWaterCells, nCellsCompleted, m_spaceSolvers.Count, iSolve)

                nCellsCompleted += nCells

                iLstCell = iFrstCell + nCells - 1
                If iLstCell > m_Data.iTotalWaterCells Then
                    iLstCell = m_Data.iTotalWaterCells 'iTotalCells Then iLstCell = iTotalCells
                End If

                solver.FirstLastCells(iFrstCell, iLstCell)
                'same wait object for all the threads
                'Once the thread counter has been Decrement to Zero
                'the Wait object will be released by the solver
                solver.WaitHandle = WaitOb

                ThreadPool.QueueUserWorkItem(AddressOf solver.Solve)

                iFrstCell += nCells

            Next solver

            Debug.Assert((iSolve = Me.m_Data.nSpaceSolverThreads) And (m_spaceSolvers.Count = Me.m_Data.nSpaceSolverThreads), "Ecospace.runSpaceSolverThreads() Thread counters are incorrect.")
            Debug.Assert(nCellsCompleted = m_Data.iTotalWaterCells, "Ecospace.runSpaceSolverThreads() may have computed the wrong number of cells. You need to check this.")
            ' System.Console.WriteLine("Solver init threads time, " & stpTotRun.Elapsed.TotalSeconds.ToString)

            'Not implemented on MONO
            ' Me.dumpRunningThreadInfo("Before")

            'The WaitObject will be signaled once the threads have counted down the ThreadIncrementer to zero
            If Not WaitOb.WaitOne() Then
                Debug.Assert(False, "Timed out!")
                cLog.Write(Me.ToString & ".runSpaceSolverThreads() Timed out.")
            End If

            etRunTime = stpTotRun.Elapsed.TotalSeconds

            Me.UpdateThreadedResults()

            stpTotRun.Stop()
            'System.Console.WriteLine("Solver compute time (sec), " & etRunTime.ToString)
            'System.Console.WriteLine("Solver total wall run time (sec), " & stpTotRun.Elapsed.TotalSeconds.ToString)
            'System.Console.WriteLine("Solver CPU time (sec), " & cpuTime.ToString)
            'System.Console.WriteLine("Solver Catch CPU time (sec), " & cpuTimeCatch.ToString)

            'Me.dumpCellComputeTimes()

            WaitOb.Dispose()

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException("Error in runSpaceSolverThreads() " & ex.Message, ex)
        End Try

        ' GC.Collect()

    End Sub

    Private Sub runSpaceCSolverThreads(ByVal timefactor As Single)
        Dim solver As cSpaceSolver
        Dim iFrstCell As Integer
        Dim iLstCell As Integer
        Dim etRunTime As Double

        iFrstCell = 1
        iLstCell = 0

        ' Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId
        ' Console.WriteLine("Ecospace ThreadID = " & thrdID.ToString)

        'Create one WaitHandle and pass it to all the threads
        'Once the cSpaceSolver.ThreadIncrementer hits zero the thread(cSpaceSolver) object will call ManualResetEvent.Set()  
        'this allows any waiting threads to continue
        Dim WaitOb As ManualResetEvent = New ManualResetEvent(False)
        Dim stpTotRun As Stopwatch = Stopwatch.StartNew

        Try

            'set the shared thread increment counter to the number of threads
            cSpaceSolver.ThreadIncrementer = m_spaceSolvers.Count

            'Number of cells to compute for the current thread
            Dim nCells As Integer
            'Total number of cells that have been computed
            Dim nCellsCompleted As Integer
            'Loop counter
            Dim iSolve As Integer

            'loop through each solver, create a thread for it, and run it
            For Each solver In m_spaceSolvers
                iSolve += 1
                'Compute the work load for each thread on the fly
                'this prevents any rounding weirdness that could cause cSpaceSolver.ThreadIncrementer to not hit Zero
                'Causing a deadlock on WaitOb.WaitOne()
                nCells = Me.computeThreadLoad(m_Data.iTotalWaterCells, nCellsCompleted, m_spaceSolvers.Count, iSolve)

                nCellsCompleted += nCells

                iLstCell = iFrstCell + nCells - 1
                If iLstCell > m_Data.iTotalWaterCells Then
                    iLstCell = m_Data.iTotalWaterCells 'iTotalCells Then iLstCell = iTotalCells
                End If

                solver.FirstLastCells(iFrstCell, iLstCell)
                solver.SetTimeStepC(m_Data.TimeStep / timefactor)
                'same wait object for all the threads
                'Once the thread counter has been Decrement to Zero
                'the Wait object will be released by the solver
                solver.WaitHandle = WaitOb

                ThreadPool.QueueUserWorkItem(AddressOf solver.SolveC)

                iFrstCell += nCells

            Next solver

            Debug.Assert((iSolve = Me.m_Data.nSpaceSolverThreads) And (m_spaceSolvers.Count = Me.m_Data.nSpaceSolverThreads), "Ecospace.runSpaceSolverThreads() Thread counters are incorrect.")
            Debug.Assert(nCellsCompleted = m_Data.iTotalWaterCells, "Ecospace.runSpaceSolverThreads() may have computed the wrong number of cells. You need to check this.")
            ' System.Console.WriteLine("Solver init threads time, " & stpTotRun.Elapsed.TotalSeconds.ToString)

            'Not implemented on MONO
            ' Me.dumpRunningThreadInfo("Before")

            'The WaitObject will be signaled once the threads have counted down the ThreadIncrementer to zero
            If Not WaitOb.WaitOne() Then
                Debug.Assert(False, "Timed out!")
                cLog.Write(Me.ToString & ".runSpaceSolverThreads() Timed out.")
            End If

            etRunTime = stpTotRun.Elapsed.TotalSeconds

            Me.UpdateThreadedResults()

            stpTotRun.Stop()
            'System.Console.WriteLine("Solver compute time (sec), " & etRunTime.ToString)
            'System.Console.WriteLine("Solver total wall run time (sec), " & stpTotRun.Elapsed.TotalSeconds.ToString)
            'System.Console.WriteLine("Solver CPU time (sec), " & cpuTime.ToString)
            'System.Console.WriteLine("Solver Catch CPU time (sec), " & cpuTimeCatch.ToString)

            'Me.dumpCellComputeTimes()

            WaitOb.Dispose()

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException("Error in runSpaceSolverThreads() " & ex.Message, ex)
        End Try

        ' GC.Collect()

    End Sub

    Private Sub runContaminantTracerSolveGrid()
        Dim i As Integer
        Dim j As Integer
        Dim iRgn As Integer
        Dim ip As Integer
        Dim Wtr As Double

        Me.grdslvConSim.FirstLastGroups(0, m_EPdata.NumGroups)
        'the grid solver has already been initialized with a reference to the contaminant tracing data
        'Me.grdslvConSim.Solve(Nothing)
        Me.grdslvConSim.Solve(Nothing)

        'ReDim m_tracerData.ConcMax(m_EPdata.NumGroups)

        'For i = 1 To m_Data.InRow
        '    For j = 1 To m_Data.InCol

        '        iRgn = m_Data.Region(i, j)
        '        If iRgn > m_Data.nRegions Then iRgn = 0

        '        For ip = 0 To m_Data.NGroups
        '            'If SpaceTime = False Then Wtr = Exp(AMmTr(i, j, ip) * TimeStep) Else Wtr = 0
        '            Wtr = Math.Exp(m_Data.AMmTr(i, j, ip) * m_Data.TimeStep)
        '            'ww i don't think this line should be here
        '            'm_Data.Ccell(i, j, ip) = Wtr * m_Data.Clast(i, j, ip) + (1 - Wtr) * m_Data.Ccell(i, j, ip)
        '            m_Data.Clast(i, j, ip) = m_Data.Ccell(i, j, ip)

        '            If m_Data.Ccell(i, j, ip) > m_tracerData.ConcMax(ip) Then m_tracerData.ConcMax(ip) = m_Data.Ccell(i, j, ip)

        '            m_tracerData.TracerConcByRegion(iRgn, ip, itt) = m_tracerData.TracerConcByRegion(iRgn, ip, itt) + m_Data.Ccell(i, j, ip)
        '            m_tracerData.TracerCBRegion(iRgn, ip, itt) = m_tracerData.TracerCBRegion(iRgn, ip, itt) + m_Data.Ccell(i, j, ip) / m_Data.Bcell(i, j, ip)

        '        Next ip

        '    Next j
        'Next i

        ''average contamintant results by region
        'For iRgn = 0 To m_Data.nRegions
        '    Dim nInRgn As Integer = m_Data.nCellsInRegion(iRgn)
        '    If nInRgn = 0 Then nInRgn = 1 'there can be regions with zero cells(no area) this avoids a /0 

        '    For igrp As Integer = 0 To m_Data.NGroups
        '        m_tracerData.TracerConcByRegion(iRgn, igrp, itt) = m_tracerData.TracerConcByRegion(iRgn, igrp, itt) / nInRgn
        '        m_tracerData.TracerCBRegion(iRgn, igrp, itt) = m_tracerData.TracerCBRegion(iRgn, igrp, itt) / nInRgn
        '    Next igrp
        'Next iRgn


    End Sub

    Private Sub summarizeContaminantTracer()
        Dim i As Integer
        Dim j As Integer
        Dim iRgn As Integer
        Dim ip As Integer
        Dim Wtr As Double

        'Me.grdslvConSim.FirstLastGroups(0, m_EPdata.NumGroups)
        'the grid solver has already been initialized with a reference to the contaminant tracing data
        'Me.grdslvConSim.Solve(Nothing)
        'Me.grdslvConSim.Solve(Nothing)

        ReDim m_tracerData.ConcMax(m_EPdata.NumGroups)

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol

                iRgn = m_Data.Region(i, j)
                If iRgn > m_Data.nRegions Then iRgn = 0

                For ip = 0 To m_Data.NGroups
                    'If SpaceTime = False Then Wtr = Exp(AMmTr(i, j, ip) * TimeStep) Else Wtr = 0
                    Wtr = Math.Exp(m_Data.AMmTr(i, j, ip) * m_Data.TimeStep)
                    'ww i don't think this line should be here
                    'm_Data.Ccell(i, j, ip) = Wtr * m_Data.Clast(i, j, ip) + (1 - Wtr) * m_Data.Ccell(i, j, ip)
                    m_Data.Clast(i, j, ip) = m_Data.Ccell(i, j, ip)

                    If m_Data.Ccell(i, j, ip) > m_tracerData.ConcMax(ip) Then m_tracerData.ConcMax(ip) = m_Data.Ccell(i, j, ip)

                    m_tracerData.TracerConcByRegion(iRgn, ip, itt) = m_tracerData.TracerConcByRegion(iRgn, ip, itt) + m_Data.Ccell(i, j, ip)
                    m_tracerData.TracerCBRegion(iRgn, ip, itt) = m_tracerData.TracerCBRegion(iRgn, ip, itt) + m_Data.Ccell(i, j, ip) / m_Data.Bcell(i, j, ip)

                Next ip

            Next j
        Next i

        ' Me.m_Data.debugDumpContaminantMap(0)
        'System.Console.WriteLine("Sum Region 0 " + m_tracerData.TracerConcByRegion(0, 0, itt).ToString)

        'average contamintant results by region
        For iRgn = 0 To m_Data.nRegions
            Dim nInRgn As Integer = m_Data.nCellsInRegion(iRgn)
            If nInRgn = 0 Then nInRgn = 1 'there can be regions with zero cells(no area) this avoids a /0 

            For igrp As Integer = 0 To m_Data.NGroups
                m_tracerData.TracerConcByRegion(iRgn, igrp, itt) = m_tracerData.TracerConcByRegion(iRgn, igrp, itt) / nInRgn
                m_tracerData.TracerCBRegion(iRgn, igrp, itt) = m_tracerData.TracerCBRegion(iRgn, igrp, itt) / nInRgn
            Next igrp
        Next iRgn

    End Sub


    Private Sub runContaminantTracerExplicit1(ByRef Derivcon As Single(,,), ByRef Derivcon2 As Single(,,), ByVal ntc As Integer)
        Dim i As Integer, j As Integer, iGrp As Integer
        Dim Tst As Single

        'set smaller timestep previously calculated in estimateMaxTimeStep
        Tst = m_Data.TimeStep / CSng(ntc)

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    For iGrp = 0 To m_Data.NGroups
                        Derivcon(i, j, iGrp) = m_Data.Ftr(i, j, iGrp) +
                            Bcw(i, j, iGrp) * m_Data.Ccell(i - 1, j, iGrp) +
                            C(i, j, iGrp) * m_Data.Ccell(i + 1, j, iGrp) +
                            d(i, j - 1, iGrp) * m_Data.Ccell(i, j - 1, iGrp) +
                            e(i, j + 1, iGrp) * m_Data.Ccell(i, j + 1, iGrp) +
                            m_Data.AMmTr(i, j, iGrp) * m_Data.Ccell(i, j, iGrp)
                        'm_Data.Ccell(i, j, iGrp) = m_Data.Ccell(i, j, iGrp) + Derivcon(i, j, iGrp) * Tst
                        Derivcon2(i, j, iGrp) = Derivcon(i, j, iGrp)
                    Next
                End If
            Next
        Next

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    For iGrp = 0 To m_Data.NGroups
                        m_Data.Ccell(i, j, iGrp) = m_Data.Ccell(i, j, iGrp) + Derivcon(i, j, iGrp) * Tst
                        'Derivcon2(i, j, iGrp) = Derivcon(i, j, iGrp)
                    Next
                End If
            Next
        Next

    End Sub


    Private Sub runContaminantTracerExplicit1_NoOffset(ByRef Derivcon As Single(,,), ByRef Derivcon2 As Single(,,), ByVal ntc As Integer)
        'jb 6-Dec-2016 OK This fixes the issue with the velocity vectors being off set by one cell 
        'But the model goes unstable after a long run... So not so good ehhh
        Dim i As Integer, j As Integer, iGrp As Integer
        Dim Tst As Single

        'set smaller timestep previously calculated in estimateMaxTimeStep
        Tst = m_Data.TimeStep / CSng(ntc)

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    For iGrp = 0 To m_Data.NGroups

                        'If iGrp = 41 And i = 3 And j - 1 = 3 Then
                        '    Debug.Assert(False)
                        'End If
                        Derivcon(i, j, iGrp) = m_Data.Ftr(i, j, iGrp) +
                            Bcw(i - 1, j, iGrp) * m_Data.Ccell(i - 1, j, iGrp) +
                            C(i + 1, j, iGrp) * m_Data.Ccell(i + 1, j, iGrp) +
                            d(i, j - 1, iGrp) * m_Data.Ccell(i, j - 1, iGrp) +
                            e(i, j + 1, iGrp) * m_Data.Ccell(i, j + 1, iGrp) +
                            m_Data.AMmTr(i, j, iGrp) * m_Data.Ccell(i, j, iGrp)
                        'm_Data.Ccell(i, j, iGrp) = m_Data.Ccell(i, j, iGrp) + Derivcon(i, j, iGrp) * Tst
                        Derivcon2(i, j, iGrp) = Derivcon(i, j, iGrp)
                    Next
                End If
            Next
        Next

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    For iGrp = 0 To m_Data.NGroups
                        m_Data.Ccell(i, j, iGrp) = m_Data.Ccell(i, j, iGrp) + Derivcon(i, j, iGrp) * Tst
                        'Derivcon2(i, j, iGrp) = Derivcon(i, j, iGrp)
                    Next
                End If
            Next
        Next

    End Sub

    Private Sub runContaminantTracerExplicit2(ByRef Derivcon As Single(,,), ByRef Derivcon2 As Single(,,), ByVal ntc As Integer)
        Dim i As Integer, j As Integer, iGrp As Integer
        Dim Tst As Single

        'set smaller timestep previously calculated in estimateMaxTimeStep
        Tst = m_Data.TimeStep / CSng(ntc)

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    For iGrp = 0 To m_Data.NGroups
                        Derivcon(i, j, iGrp) = m_Data.Ftr(i, j, iGrp) +
                            Bcw(i, j, iGrp) * m_Data.Ccell(i - 1, j, iGrp) +
                            C(i, j, iGrp) * m_Data.Ccell(i + 1, j, iGrp) +
                            d(i, j - 1, iGrp) * m_Data.Ccell(i, j - 1, iGrp) +
                            e(i, j + 1, iGrp) * m_Data.Ccell(i, j + 1, iGrp) +
                            m_Data.AMmTr(i, j, iGrp) * m_Data.Ccell(i, j, iGrp)
                        'm_Data.Ccell(i, j, iGrp) = m_Data.Ccell(i, j, iGrp) + 0.5 * (3.0 * Derivcon(i, j, iGrp) - Derivcon2(i, j, iGrp)) * Tst
                        Derivcon2(i, j, iGrp) = Derivcon(i, j, iGrp)
                    Next
                End If
            Next
        Next

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    For iGrp = 0 To m_Data.NGroups
                        m_Data.Ccell(i, j, iGrp) = m_Data.Ccell(i, j, iGrp) + 0.5 * (3.0 * Derivcon(i, j, iGrp) - Derivcon2(i, j, iGrp)) * Tst
                        'Derivcon2(i, j, iGrp) = Derivcon(i, j, iGrp)
                    Next
                End If
            Next
        Next
    End Sub

    Private Sub dumpGridRunTimes()
        Dim totCPU As Single
        For Each grid As cGridSolver In Me.m_gridSolvers
            totCPU += grid.CPUTime
            System.Console.WriteLine("Grid ID = " & grid.ThreadID.ToString & ", N Iters = " & grid.iterThread.ToString & ", CPU Time(sec)= " & grid.CPUTime.ToString & ", N = " & grid.nGroupsComputed.ToString)
        Next
        System.Console.WriteLine("Grid CPU time(sec)," & totCPU.ToString)
    End Sub

    Private Sub dumpRunningThreadInfo(msg As String)
        'WARNING ProcessThread's not implemented on MONO...
        'so this won't dump out anything
        System.Console.WriteLine("---------------" & msg & "---------------------")
        System.Console.WriteLine(" Number of threads = " & Process.GetCurrentProcess.Threads.Count.ToString)
        dumpRunningThreadInfo(bRunningOnly:=True)
        dumpRunningThreadInfo(bRunningOnly:=False)

    End Sub

    Private Sub dumpRunningThreadInfo(bRunningOnly As Boolean)
        'WARNING ProcessThread's not implemented on MONO...
        For Each Thread As ProcessThread In Process.GetCurrentProcess.Threads
            If (bRunningOnly = (Thread.ThreadState = Diagnostics.ThreadState.Running)) Then
                If Thread.ThreadState = Diagnostics.ThreadState.Wait Then
                    System.Console.WriteLine(" State = " & Thread.ThreadState.ToString & " Reason = " & Thread.WaitReason.ToString)
                Else
                    System.Console.WriteLine(" State = " & Thread.ThreadState.ToString)
                End If

                System.Console.WriteLine(" Thread ID = " & Thread.Id.ToString)
                System.Console.WriteLine(" Priority = " & Thread.CurrentPriority.ToString)
                System.Console.WriteLine(" Start time = " & Thread.StartTime.ToLongTimeString)
                System.Console.WriteLine(" Total time(sec) = " & Thread.TotalProcessorTime.TotalSeconds.ToString)
                System.Console.WriteLine(" CPU time(sec) = " & Thread.UserProcessorTime.TotalSeconds.ToString)
                System.Console.WriteLine()
            End If
        Next

    End Sub

    Private Sub dumpCellComputeTimes()
        System.Console.WriteLine("Cell compute times")
        For Each solver As cSpaceSolver In Me.m_spaceSolvers

            '  System.Console.Write("Cells " & solver.iFrstCell.ToString & " - " & solver.iLstCell.ToString & ", Run time(sec) " & solver.SolveCPUTimeSec.ToString)
            For Each t As Double In solver.lstCellCompTimes
                System.Console.Write("," & t.ToString)
            Next
            System.Console.WriteLine()
        Next solver
    End Sub


    Public Function initSpatialEquilibrium() As Boolean
        Dim ip As Integer, i As Integer, j As Integer
        Dim ig As Integer
        Dim isp As Integer, ist As Integer

        Try

            'Is this model coupled to an external model
            If Me.m_EPdata.isEcospaceModelCoupled Then
                'redim MPred at the start of each run because we have no way of knowing when EcoSimDataStructures.inlinks has changed
                'inlinks is the number of prey/pred linkages
                Me.m_Data.allocate(Me.m_Data.MPred, Me.m_Data.InRow + 1, Me.m_Data.InCol + 1, Me.m_SimData.inlinks)
                cLog.Write("Ecospace allocated MPred data for model interoperability", eVerboseLevel.Detailed)
            End If

            ReDim totalIterThread(m_Data.nGridSolverThreads + 1)

            m_bsolverError = False
            bEffortAdjusted = False
            m_StopRun = False
            nvar2 = m_Data.NGroups

            If Me.m_Data.TotalTime > Me.m_SimData.NumYears Then
                'Ecospace uses Ecosim.FishRateGear(fleet,EcosimYears) for fishing effort
                'so the Space run length must be less then Ecosim run length
                Me.m_Data.TotalTime = Me.m_SimData.NumYears
            End If

            Me.m_SimData.setRelQToT(1, False)

            '*******************
            'readAdvectFile()
            '*****************

            'jb Moved default values to InitToDefaults
            ' m_Data.NewMultiStanza = True
            '  m_Data.UseIBM = True
            'm_Data.TimeStep = 1 / 12

            If m_Data.NewMultiStanza Then
                '    m_Data.IFDPower = 0.5 'this should be available to users in interface, higher values typically cause
                'instability in spatial allocation (IFD) model for multistanza biomass distributions

                ReDim Blocal(m_Data.NGroups)
            End If

            Dim Wchange() As Single
            ReDim Wchange(m_Data.nvartot)

            ReDim Ecode(m_Data.Nvarsplit)

            'Update the Depth map based on the Exclusion layer
            Me.UpdateDepthMap()

            If m_tracerData.EcoSpaceConSimOn Then
                m_Data.RedimConSimVars()
                m_tracerData.redimForEcospaceRun(m_Data.nRegions, m_Data.NGroups, m_Data.nTimeSteps)
            End If

            m_SimData.FirstTime = True

            'In EwE5 this was part of InitialState changed here
            'compute the IecoCode() index
            'this index pointer is unique to Ecospace
            ReDim IecoCode(m_Data.NGroups)
            Dim ir As Integer, igrp As Integer
            For i = 1 To m_Stanza.Nsplit
                For j = 1 To m_Stanza.Nstanza(i)
                    ir = ir + 1
                    igrp = m_Stanza.EcopathCode(i, j)
                    IecoCode(igrp) = ir
                Next
            Next

            Me.NormalizeMigrationMaps()

            'populates Kmovefit() and PzoTOmove()
            SetKmove() 'test set for movement in relation to fitness 

            SetBoundaryDepths()

            'check to see if user wants to have some groups advected/migratory
            ReDim MigPowi(m_Data.NGroups, m_Data.InRow + 1), MigPowj(m_Data.NGroups, m_Data.InCol + 1)
            ReDim PrefRowP(m_Data.NGroups, 12), PrefColP(m_Data.NGroups, 12)
            For ip = 1 To m_Data.NGroups

                'jb comment out for now
                'ToDo_jb  initSpatialEquilibrium() FeedBackMessage
                'If M - Data.RelMoveBad(ip) = 1 And Mvel(ip) > 1 And IsAdvected(ip) = 0 Then
                '    If MsgBox("Is group " + Specie$(ip) + " advected?", vbYesNo) = vbYes Then IsAdvected(ip) = 1
                'End If

                If m_Data.IsMigratory(ip) Then
                    For i = 1 To m_Data.InRow : MigPowi(ip, i) = i ^ m_Data.MigConcRow(ip) : Next
                    For j = 1 To m_Data.InCol : MigPowj(ip, j) = j ^ m_Data.MigConcCol(ip) : Next
                    For i = 1 To 12
                        PrefRowP(ip, i) = m_Data.PrefRow(ip, i) ^ m_Data.MigConcRow(ip)
                        PrefColP(ip, i) = m_Data.Prefcol(ip, i) ^ m_Data.MigConcCol(ip)
                    Next
                End If

            Next

            'jb 12-May-2010 do a full initialization of Ecosim. This should have been handled by the framework...but sometimes it gets dropped
            Me.m_Ecosim.Init(True)

            'Tell all the capacity map variables that they need to be recomputed
            Me.m_Data.isCapacityChanged = True
            Me.m_Data.setHabCapGroupIsChanged(True)
            'Now reset capacity
            Me.m_Data.hasCapInitialized = False
            SetHabCap()
            Me.m_Data.hasCapInitialized = True
            'Debug.Assert(False, "hasCapInitialized = False for debugging")
            'Me.m_Data.hasCapInitialized = False

            'first set density map for all pools to no movement equilibrium
            SetBiomassesEcospace()
            Me.m_Data.PPScale = ScaleRelativePrimaryProductivityToEcopathLevel()

            'PopulateFleetCells must come before ScaleSailingCost 
            Me.m_Data.PopulateFleetCells()
            Me.ScaleSailingCost()

            'calculate exponential weights for time step updating
            m_Ecosim.Derivt(0, m_SimData.StartBiomass, der, 1)
            For ip = 1 To m_Data.NGroups
                '****Following line corrects bug where Mrate was set later in the routine
                'CJW modified Mrate calculation next line 2/2003 for migratory species
                If m_Data.IsMigratory(ip) = False Then
                    m_Data.Mrate(ip) = m_Data.Mvel(ip) / (3.14159 * m_Data.CellLength)
                Else

                    m_Data.Mrate(ip) = m_Data.Mvel(ip) / Math.Sqrt(m_Data.CellLength)
                    'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    'HACK for debugging migration movement. Set movement to base rates
                    'Debug.Assert(False, "MRate() for migrating groups not set correctly.")
                    'm_Data.Mrate(ip) = m_Data.Mvel(ip) / (3.14159 * m_Data.CellLength)
                    'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                End If

            Next ip

            For ig = 1 To m_Data.nFleets
                m_SimData.FishRateGear(ig, 0) = 1
            Next ig

            If m_Data.IsFishRateSet = False Then
                For ig = 1 To m_Data.nFleets
                    'For i = 0 To TotalTime * 12
                    For i = 0 To m_SimData.NTimes
                        m_SimData.FishRateGear(ig, i) = 1
                    Next
                Next
                'ToDo_jb IsFishRateSet in EwE5 see when this gets reset to false
                m_Data.IsFishRateSet = True
            End If

            If m_Data.PredictEffort Then SetEffortParameters(True)

            If m_tracerData.EcoSpaceConSimOn Then
                'If m_ConTracer Is Nothing Then
                '    m_ConTracer = New cContaminantTracer
                'End If
                'm_ConTracer.Init(m_tracerData, m_EPdata, m_ESData, m_Stanza)
                'm_ConTracer.CInitialize()
                Basebiomass(0) = 1
                m_Data.IsAdvected(0) = True
                'VC with CJW 20151124: first detritus group has to be advected for ecotracter environment to be moved around
                m_Data.IsAdvected(Me.EcoPathData.NumLiving + 1) = True

                'Temporary for debuging advected contaminants
                'Hardcode some Velocity vectors
                'Me.m_Data.debugSetAdvectionVectors()

            End If

            Dim btot(ip) As Single

            For iflt As Integer = 1 To Me.m_Data.nFleets
                For i = 0 To m_Data.InRow + 1
                    For j = 0 To m_Data.InCol + 1
                        If m_Data.Sail(iflt)(i, j) = 0 Then m_Data.Sail(iflt)(i, j) = 0.000001
                    Next
                Next
            Next iflt

            Dim sumb(Me.m_Data.NGroups) As Single
            For ip = 0 To m_Data.NGroups
                Btime(ip) = 0
                For i = 0 To m_Data.InRow + 1
                    For j = 0 To m_Data.InCol + 1

                        'If m_Data.MPA(i, j) > m_Data.MPAno Then m_Data.MPA(i, j) = 0

                        If m_Data.IsAdvected(ip) Then
                            m_Data.Bcell(i, j, ip) = (Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ip)) * Me.m_Data.HabCap(ip)(i, j) * m_SimData.StartBiomass(ip)
                        Else
                            m_Data.Bcell(i, j, ip) = 0
                        End If

                        If m_Data.Depth(i, j) > 0 Then

                            m_Data.Bcell(i, j, ip) = (Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ip)) * Me.m_Data.HabCap(ip)(i, j) * m_SimData.StartBiomass(ip)
                            'sumb(ip) += m_Data.Bcell(i, j, ip)

                            If m_Data.IsMigratory(ip) Then
                                If Me.m_Data.MigMaps(ip, 1)(i, j) > MIN_MIG_PROB Then
                                    m_Data.Bcell(i, j, ip) = (Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ip)) * Me.m_Data.HabCap(ip)(i, j) * m_SimData.StartBiomass(ip)
                                Else
                                    m_Data.Bcell(i, j, ip) = 0
                                End If
                                If i = 0 Or i = m_Data.InRow + 1 Or j = 0 Or j = m_Data.InCol + 1 Then m_Data.Bcell(i, j, ip) = 0
                            End If
                        Else
                            'Depth(i,j) <= 0
                            AMm(i, j, ip) = -1.0 'E+30
                        End If ' If m_Data.Depth(i, j) > 0 Then

                        If ip = 0 Then m_Data.Bcell(i, j, ip) = 1

                        m_Data.Blast(i, j, ip) = m_Data.Bcell(i, j, ip)
                        If i > 0 And j > 0 And i <= m_Data.InRow And j <= m_Data.InCol Then Btime(ip) = Btime(ip) + m_Data.Bcell(i, j, ip)

                        If m_tracerData.EcoSpaceConSimOn And ip <= m_Data.NGroups Then
                            'Debug.Assert(False, "EcoSpace Contaminant Tracer not Initialized properly.")
                            'jb in EwE5 Ccell() is initialized using ConcTr()
                            'in EwE5 CInitialize() was called right before this setting ConcTr() to Czero()
                            m_Data.Ccell(i, j, ip) = m_Data.HabCap(ip)(i, j) * Me.m_tracerData.Czero(ip)
                            m_Data.Clast(i, j, ip) = m_Data.Ccell(i, j, ip)
                        End If

                        Cper(i, j, ip) = m_SimData.Cbase(ip)
                        FtimeCell(i, j, ip) = 1
                        HdenCell(i, j, ip) = m_SimData.Hden(ip)
                        btot(ip) += m_Data.Bcell(i, j, ip)
                    Next j
                Next i
                Btime(ip) = Btime(ip) / m_Data.nWaterCells
                'For Debugging
                If ip > 0 Then
                    ' System.Console.WriteLine(Me.m_EPdata.GroupName(ip) + " BSpace/BPath = " + (Btime(ip) / Me.m_EPdata.B(ip)).ToString)
                End If
            Next ip

            Dim isc As Integer, ieco As Integer
            isc = 0
            For isp = 1 To m_Stanza.Nsplit
                For ist = 1 To m_Stanza.Nstanza(isp)
                    isc = isc + 1

                    ieco = m_Stanza.EcopathCode(isp, ist)
                    If m_Data.NewMultiStanza Or m_Data.UseIBM Then
                        'these flags turn off implicit integration for multistanza biomasses when newmultistanza=true
                        m_Data.ByPassIntegrate(ieco) = True
                        If m_Data.UseIBM Then m_Data.ByPassIntegrate(nvar2 + isc) = True
                    End If

                    Ecode(isc) = ieco
                    If m_Data.IsMigratory(ieco) = True Then m_Data.IsMigratory(nvar2 + isc) = True
                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1
                            If m_Data.Depth(i, j) > 0 Then

                                If Not m_Data.IsMigratory(ieco) Then
                                    m_Data.Bcell(i, j, nvar2 + isc) = NstanzaBase(isc) * Me.m_Data.HabCap(ieco)(i, j) * Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ieco)
                                    If m_Data.NewMultiStanza Then
                                        m_Data.PredCell(i, j, ieco) = m_SimData.pred(ieco) * Me.m_Data.HabCap(ieco)(i, j) * Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ieco)
                                    End If
                                Else
                                    'migrating(group)
                                    'only populte cells that are in the migration area for the first month
                                    If m_Data.MigMaps(ieco, 1)(i, j) > MIN_MIG_PROB Then
                                        m_Data.Bcell(i, j, nvar2 + isc) = NstanzaBase(isc) * Me.m_Data.HabCap(ieco)(i, j) * Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ieco)
                                        If m_Data.NewMultiStanza Then
                                            m_Data.PredCell(i, j, ieco) = m_SimData.pred(ieco) * Me.m_Data.HabCap(ieco)(i, j) * Me.m_Data.nWaterCells / Me.m_Data.TotHabCap(ieco)
                                        End If
                                    End If

                                End If

                            Else 'm_Data.Depth(i, j) > 0
                                'Land
                                m_Data.Bcell(i, j, nvar2 + isc) = 1.0E-20
                            End If 'm_Data.Depth(i, j) > 0
                            m_Data.Blast(i, j, nvar2 + isc) = m_Data.Bcell(i, j, nvar2 + isc)
                        Next j
                    Next i
                Next ist
            Next isp

            'set dispersal rate arrays for solvegrid
            SetMovementParameters()

            'Need to call this to initialize DepthY and DepthX arrays.  
            If m_Data.CurrentForce Then SetXYBoundaryDepths()

            'set some solvegrid solution parameters
            Dim iter As Double
            Dim TimeStep2 As Single

            Dim ihalf As Integer = Math.Truncate(m_Data.InCol / 2)
            j = 0
            For i = ihalf To 1 Step -1
                j = j + 1
                Me.m_Data.jord(i) = j
            Next
            For i = ihalf + 1 To m_Data.InCol
                j = j + 1
                Me.m_Data.jord(i) = j
            Next

            iter = 0
            TimeStep2 = m_Data.TimeStep '/ 2

            ReDim RelRepStanza(m_Stanza.Nsplit)
            For i = 1 To m_Stanza.Nsplit
                RelRepStanza(i) = 1 / m_SimData.StartBiomass(m_Stanza.EcopathCode(i, m_Stanza.Nstanza(i)))
            Next i

            Dim waterCtr As Integer = 0
            Dim foundRow As Boolean
            ReDim m_Data.iWaterCellIndex(m_Data.InCol * m_Data.InRow)
            ReDim m_Data.jWaterCellIndex(m_Data.InCol * m_Data.InRow)
            ReDim m_Data.iStartRow(m_Data.InCol)
            ReDim m_Data.iEndRow(m_Data.InCol)
            ReDim m_Data.jStartCol(m_Data.InRow)
            ReDim m_Data.jEndCol(m_Data.InRow)


            'this finds the start and end rows and columns so that solvegrid doesn't go through every one
            For j = 1 To m_Data.InCol
                foundRow = False
                m_Data.iStartRow(j) = m_Data.InRow + 1
                m_Data.iEndRow(j) = 0
                For i = 1 To m_Data.InRow
                    If m_Data.Depth(i, j) > 0 Then
                        waterCtr = waterCtr + 1
                        m_Data.iWaterCellIndex(waterCtr) = i
                        m_Data.jWaterCellIndex(waterCtr) = j
                        If m_Data.iStartRow(j) = m_Data.InRow + 1 Then
                            m_Data.iStartRow(j) = i
                            foundRow = True
                        End If
                        m_Data.iEndRow(j) = i
                    End If
                Next
                'm_Data.iStartRow(j) = 1
                'm_Data.iEndRow(j) = m_Data.Inrow
            Next
            m_Data.iTotalWaterCells = waterCtr

            For i = 1 To m_Data.InRow
                m_Data.jStartCol(i) = m_Data.InCol + 1
                m_Data.jEndCol(i) = 0
                For j = 1 To m_Data.InCol
                    If m_Data.Depth(i, j) > 0 Then
                        If m_Data.jStartCol(i) = m_Data.InCol + 1 Then
                            m_Data.jStartCol(i) = j
                        End If
                        m_Data.jEndCol(i) = j
                    End If
                Next
            Next

            'jb move to RedimForRun
            ' ReDim BEQlast(m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)

            '**** this functionality has been moved below **** 
            'this finds which groups are being integrated, so they can be adeq
            'ReDim m_Data.integratedGroups(m_Data.nvartot)
            'Dim integrateIndex As Integer = 0
            'For i = 1 To m_Data.nvartot
            '    If m_Data.ByPassIntegrate(i) = False Then
            '        integrateIndex = integrateIndex + 1
            '        m_Data.integratedGroups(integrateIndex) = i
            '    End If
            'Next
            'm_Data.totalIntegratedGroups = integrateIndex

            'ww set up thread alocation for gridsolver, since migratory takes much longer
            nMigratory = 0
            ReDim migratoryIndex(m_Data.nvartot)
            For i = 1 To m_Data.nvartot
                'find all the migratory species
                If m_Data.IsMigratory(i) And m_Data.ByPassIntegrate(i) = False Then
                    nMigratory += 1
                    migratoryIndex(nMigratory) = i
                End If
            Next

            If Me.m_Data.nEffortDistThreads > Me.m_Data.nFleets Then
                Me.m_Data.nEffortDistThreads = Me.m_Data.nFleets
                System.Console.WriteLine(Me.ToString & " Initializing threads. WARNING number of effort distribution threads limited to number of fleets.")
            End If

            If Me.m_Data.nGridSolverThreads > Me.m_Data.NGroups Then
                Me.m_Data.nGridSolverThreads = Me.m_Data.NGroups
                System.Console.WriteLine(Me.ToString & " Initializing threads. WARNING number of grid threads limited to number of groups.")
            End If

            If Me.m_Data.nSpaceSolverThreads > Me.m_Data.iTotalWaterCells Then
                Me.m_Data.nSpaceSolverThreads = Me.m_Data.iTotalWaterCells
                System.Console.WriteLine(Me.ToString & " Initializing threads. WARNING number of group threads limited to number of water cells.")
            End If

            Dim thread As Integer
            ReDim nGroupsInThread(m_Data.nGridSolverThreads)
            ReDim threadGroups(m_Data.nGridSolverThreads, m_Data.nvartot)
            For i = 1 To nMigratory
                'allocate the migratory species to threads
                thread = (i - 1) Mod m_Data.nGridSolverThreads + 1
                nGroupsInThread(thread) += 1
                threadGroups(thread, nGroupsInThread(thread)) = migratoryIndex(i)
            Next

            Dim nNonMigThreads As Integer = (m_Data.nGridSolverThreads - nMigratory Mod m_Data.nGridSolverThreads)
            Dim numNonMig As Integer
            For i = 1 To m_Data.nvartot
                'assign the nonmigratory integrated variables to the least used threads
                If m_Data.IsMigratory(i) = False And m_Data.ByPassIntegrate(i) = False Then
                    numNonMig += 1
                    thread = m_Data.nGridSolverThreads - (numNonMig - 1) Mod nNonMigThreads
                    nGroupsInThread(thread) += 1
                    threadGroups(thread, nGroupsInThread(thread)) = i
                End If
            Next

            InitGridSolverThreads() 'init the solver objects one for each thread
            InitSpaceSolverThreads()
            If m_Data.UseIBM Then InitIBMSolverThreads()

            SetMigGrad()

            If m_tracerData.EcoSpaceConSimOn Then
                'initialize the contaminant tracing
                Try
                    'contaminant tracer grid solver runs on one thread
                    'process all groups on the single thread
                    ReDim Me.threadGroupsConSim(1, m_Data.NGroups)
                    For igrp = 1 To m_Data.NGroups
                        threadGroupsConSim(1, igrp) = igrp
                    Next

                    'bypass integrated for contaminants should be false for all groups
                    ReDim m_ConBypassIntegrated(m_Data.NGroups)

                    If grdslvConSim Is Nothing Then
                        'grid solver object for the contaminant tracer
                        grdslvConSim = New cGridSolver(1)
                    End If

                    'init the grid solver object
                    grdslvConSim.Init(m_Data.AMmTr, m_Data.Ftr, m_Data.Ccell, m_Data.InRow, m_Data.InCol, m_Data.Tol, Me.m_Data.jord, m_Data.W, Bcw, C, d, e,
                                       m_Data.Depth, m_ConBypassIntegrated, m_Data.iStartRow, m_Data.iEndRow, m_Data.TimeStep, m_Data.maxIter, m_Data.jStartCol,
                                       m_Data.jEndCol, m_Data.IsMigratory, threadGroupsConSim, m_Data.UseExact)

                Catch ex As Exception
                    'something went very wrong with the initialization
                    m_tracerData.EcoSpaceConSimOn = False
                    Debug.Assert(False, ex.StackTrace)
                    cLog.Write(ex)
                End Try

            End If

            'initialize contaminant tracer data
            If m_tracerData.EcoSpaceConSimOn Then
                'temporary ww
                'm_Data.Ccell(6, 6, 0) = 300
                'm_Data.Clast(6, 6, 0) = 300
                'end temp
                If m_tracerData.Czero(0) > 0 Then 'there is an initial environmental concentration, distribute over map
                    Dim TinP As Single, Tcell As Single
                    For i = 1 To m_Data.InRow
                            For j = 1 To m_Data.InCol
                                If m_Data.Depth(i, j) > 0 Then
                                    Tcell = Tcell + 1
                                    TinP = TinP + m_Data.RelCin(i, j)
                                End If
                            Next
                        Next
                    If TinP > 0 Then 'there is contaminant input at at least one cell
                        For i = 1 To m_Data.InRow
                            For j = 1 To m_Data.InCol
                                If m_Data.Depth(i, j) > 0 Then
                                    m_Data.Ccell(i, j, 0) = m_tracerData.Czero(0) * Tcell / TinP * m_Data.RelCin(i, j)
                                    'm_Data.Ccell(i, j, 0) = m_tracerData.Czero(0) * m_Data.RelCin(i, j)
                                    m_Data.Clast(i, j, 0) = m_Data.Ccell(i, j, 0)
                                End If
                            Next
                        Next
                    End If
                End If
            End If

            m_Ecosim.InitializeDataInfo()
            m_Data.nIBMGroupsPerThread = (m_Stanza.Nsplit + m_Data.nGridSolverThreads - 1) \ m_Data.nGridSolverThreads

            'Spin-up Initialization

            'Does the current run use a Spin-Up period
            Me.m_Data.bInSpinUp = Me.m_Data.UseSpinUp

            'Total number of time steps in the Spin-Up 
            Me.nSpinUp = CInt(Me.m_Data.SpinUpYears * (1 / Me.m_Data.TimeStep))
            'Number of time steps in one year. 
            'Use to cycle through the first year for the Spin-Up period
            Me.nSpinUpYear = CInt(1 * (1 / Me.m_Data.TimeStep))
            'Clear the counters
            Me.iSpinUp = 0
            Me.iSpinUpYear = 0
            'Spin-Up biomass base has not been initialized yet
            Me.bInitSpinUpBase = True

            Me.m_TimeSeriesManager.InitForRun()

            ' Me.m_Data.bENA = True
            If Me.m_Data.bENA Then
                If Me.m_Data.dctENACells IsNot Nothing Then Me.m_Data.dctENACells.Clear()
                Me.m_Data.dctENACells = New Dictionary(Of String, cENACellData)
                For irow As Integer = 1 To Me.m_Data.InRow
                    For icol As Integer = 1 To Me.m_Data.InCol
                        If Me.m_Data.Depth(irow, icol) > 0 Then
                            Dim ENACell As New cENACellData(irow, icol)
                            ENACell.Init(Me.m_Data, Me.m_EPdata)
                            Me.m_Data.dctENACells.Add(ENACell.Key, ENACell)
                        End If
                    Next
                Next
            End If

            'Advection 
            'Clear out the time step Advection and Upwelling vectors
            'It the Advection model been run these may be populated 
            Array.Clear(Me.m_Data.UpVel, 0, Me.m_Data.UpVel.Length)
            Array.Clear(Me.m_Data.Xvel, 0, Me.m_Data.UpVel.Length)
            Array.Clear(Me.m_Data.Yvel, 0, Me.m_Data.Yvel.Length)

            'For debugging Effort Zones code
            'sets up some zones with modified effort
            'Me.m_Data.DebugTestEffortZones()

            Return True

        Catch ex As Exception
            Debug.Assert(False, ex.StackTrace)
            Throw New Exception("Exception initializing Ecospace. Ecospace cannot be run. " & ex.Message, ex)
        End Try

    End Function

    Private Sub InitSpatialTemporalRun()

        Try
            ' Preserve base RelPP, either loaded or sketched
            Me.m_Data.setBaseRelPP()

            If (Me.m_SpatialData IsNot Nothing) Then
                For Each src As cSpatialDataAdapter In Me.m_SpatialData.DataAdapters
                    If (src IsNot Nothing) Then
                        Try
                            src.InitRun()
                        Catch ex As Exception
                            cLog.Write(ex, "cEcospace::Run.InitAdapters " & src.Name & "(" & src.Index & ")")
                        End Try
                    End If
                Next
            End If

        Catch ex As Exception
            Throw New Exception("Exception initializing Spatial Temporal data. Ecospace cannot be run. " + ex.Message, ex)
        End Try

    End Sub

    Private Sub EndSpatialTemporalRun()

        If (Me.m_SpatialData IsNot Nothing) Then
            For Each src As cSpatialDataAdapter In Me.m_SpatialData.DataAdapters
                If (src IsNot Nothing) Then
                    Try
                        src.EndRun()
                    Catch ex As Exception
                        cLog.Write(ex, "cEcospace::Run.CleanAdapters " & src.Name & "(" & src.Index & ")")
                    End Try
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' Redim or Clear all variables that are needed for an Ecospace run
    ''' </summary>
    ''' <remarks>In EwE5 this was handled inside FindSpatialEquilibrium. 
    ''' These are variables that will be populated by the Ecospace initialization initSpatialEquilibrium().
    ''' This should not contain any data that was populated by the database.
    ''' </remarks>
    Friend Function redimForRun() As Boolean
        ' EwE5
        'ReDim ebb(NumGroups + 3 * npairs + Nvarsplit) As Single 'abmpa
        'ReDim BB(NumGroups + 3 * npairs + Nvarsplit) As Single

        Dim success As Boolean = True
        Dim message As cMessage
        Try

            'redim new stanza stuff
            'GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)
            'System.Console.WriteLine(GC.CollectionCount(2).ToString)

            Me.m_Data.allocate(m_Data.EffortSpace, m_Data.nFleets, m_Data.InRow, m_Data.InCol)

            Me.m_Data.allocate(Cper, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.NGroups)
            Me.m_Data.allocate(RelFitness, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.NGroups)
            Me.m_Data.allocate(F, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(AMm, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(BcwNomig, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(CNomig, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(dNomig, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(Enomig, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)

            Me.m_Data.allocate(Bcw, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(C, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(d, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(e, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)

            Me.m_Data.allocate(BEQlast, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(m_Data.PredCell, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.NGroups)
            Me.m_Data.allocate(m_Data.IFDweight, m_Data.InRow, m_Data.InCol, m_Data.NGroups)

            Me.m_Data.allocate(m_Data.Blast, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
            Me.m_Data.allocate(FtimeCell, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.NGroups)
            Me.m_Data.allocate(HdenCell, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.NGroups)
            Me.m_Data.allocate(RelMoveFit, m_Data.InRow + 1, m_Data.InCol + 1)

            Me.m_Data.allocate(m_Data.Ftot, m_Data.NGroups, m_Data.InRow, m_Data.InCol)

            Me.m_Data.allocate(m_Data.Landings, m_Data.NGroups, m_Data.nFleets)

            If Me.m_EPdata.isEcospaceModelCoupled Then Me.m_Data.allocate(m_Data.GroupDetritus, m_Data.InRow, m_Data.InCol, m_Data.NGroups)

            ReDim Me.m_Data.TotEffort(m_Data.nFleets)

            ReDim Btime(m_Data.NGroups)
            ReDim TotLoss(m_Data.NGroups)
            ReDim TotEatenBy(m_Data.NGroups)
            ReDim TotBiom(m_Data.NGroups)
            ReDim TotPred(m_Data.NGroups)
            ReDim TotIFDweight(m_Data.NGroups)

            ReDim m_Data.ByPassIntegrate(m_Data.nvartot)
            ReDim m_Data.BBase(m_Data.NGroups)
            ReDim m_Data.SpinUpBBase(m_Data.NGroups)

            ReDim m_Data.BaseFishMort(m_Data.NGroups)
            ReDim m_Data.BaseConsump(m_Data.NGroups)
            ReDim m_Data.BaseCatch(m_Data.NGroups)
            ReDim m_Data.BasePredMort(m_Data.NGroups)

            ReDim Basebiomass(m_Data.nvartot)
            ReDim der(m_Data.NGroups)
            ReDim loss(m_Data.NGroups)
            ReDim pbb(m_Data.NGroups)

            ReDim EatEff(m_Data.nvartot)
            ReDim VulPred(m_Data.nvartot)

            ReDim Flowin(m_Data.nvartot)
            ReDim FlowoutRate(m_Data.nvartot)

            ReDim RecSplit(m_Data.Nvarsplit)
            ReDim PconSplit(m_Data.Nvarsplit)
            ReDim Tstanza(m_Data.Nvarsplit)
            ReDim NstanzaBase(m_Data.Nvarsplit)

            ReDim m_Data.isGroupHabCapChanged(m_Data.NGroups)

            nEcospaceTimeSteps = CInt(m_Data.TotalTime * (1.0 / m_Data.TimeStep))
            success = success And m_Data.redimTimeStepResults(nEcospaceTimeSteps)

        Catch ex As Exception
            message = New cMessage(ex.Message, _
                                   eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Critical)
        End Try

        If message IsNot Nothing Then
            Me.Messages.AddMessage(message)
            success = False
        End If

        Return success

    End Function


    Sub SetKmove()
        Dim i As Integer


        ReDim PzoTOmove(m_Data.NGroups)
        ReDim Kmovefit(m_Data.NGroups)

        For i = 1 To m_Data.NGroups
            PzoTOmove(i) = m_Data.FitnessResp
            If m_EPdata.PB(i) > 0 Then Kmovefit(i) = 2.197225 / (PzoTOmove(i) * m_EPdata.PB(i))
        Next

    End Sub


    Sub SetBoundaryDepths()
        'set cells around system boundary to depth 1 so as to allow flow across them and proper
        'tests for critters that are advected
        Dim i As Integer
        Dim j As Integer

        For j = 0 To m_Data.InCol + 1
            m_Data.Depth(0, j) = 1
            m_Data.Depth(m_Data.InRow + 1, j) = 1
        Next

        For i = 0 To m_Data.InRow + 1
            m_Data.Depth(i, 0) = 1
            m_Data.Depth(i, m_Data.InCol + 1) = 1
        Next

    End Sub

    Private Sub SetBiomassesEcospace()
        Dim i As Integer, j As Integer, ii As Integer ' , IterMax As Integer
        ReDim m_Data.Vspace(m_SimData.inlinks), m_Data.Aspace(m_SimData.inlinks), PbSpace(m_Data.NGroups)

        'calculate pbbiomass parameter from pbbase and pbm
        m_Ecosim.Set_pbm_pbbiomass()
        'get initial derivative to define runge-kutta time step deltat
        m_Ecosim.SetFishTimetoFish1()
        '****ADDED BY CJW SEPT 2001
        m_Ecosim.InitialState()
        m_Ecosim.SetTimeSteps()
        m_Ecosim.CalcStartEatenOfBy()

        m_Ecosim.SetBBtoStartBiomass(m_Data.NGroups)

        m_Ecosim.Derivt(0, m_SimData.StartBiomass, der, 1)

        'set up initial biomass density for fished areas
        For i = 1 To m_Data.NGroups
            m_SimData.FishTime(i) = m_SimData.Fish1(i)
            Basebiomass(i) = m_SimData.StartBiomass(i)
            PbSpace(i) = m_SimData.pbbiomass(i)
        Next

        'For ii = 1 To m_ESData.inlinks
        '    i = m_ESData.ilink(ii)
        '    j = m_ESData.jlink(ii)
        '    m_Data.Aspace(ii) = m_Ecosim.A(i, j)
        '    m_Data.Vspace(ii) = m_ESData.vulrate(i, j)
        'Next

        For ii = 1 To m_SimData.inlinks
            i = m_SimData.ilink(ii) : j = m_SimData.jlink(ii) ' : ia = ArenaLink(ii)
            m_Data.Aspace(ii) = m_SimData.Alink(ii)
        Next
        For ia As Integer = 1 To m_SimData.Narena
            m_Data.Vspace(ia) = m_SimData.VulArena(ia)
        Next

        'calculate correction factors for numbers dynamics going back from delay difference
        'to continuous case
        'If m_Data.AdjustSpace = True Then AdjustSpacePars()
        AdjustSpaceParsNew()

        If m_Stanza.Nsplit > 0 Then
            For i = 1 To m_Data.NGroups
                EatEff(i) = 1
                VulPred(i) = 1
            Next

            'jb EwE5 called  derivtRed with BB() this is the biomass at the current time step defined in Ecosim
            'I have changed it to call derivtRed with StartBiomass() which sould have the same effect and keep Ecosim.BB() out of this code
            derivtRed(m_SimData.StartBiomass, Flowin, FlowoutRate, EatEff, VulPred, 1)

        End If

        Dim isp As Integer, ist As Integer, St As Single, Sn As Single, ieco As Integer
        i = 0
        For isp = 1 To m_Stanza.Nsplit
            St = 1
            For ist = 1 To m_Stanza.Nstanza(isp)
                ieco = m_Stanza.EcopathCode(isp, ist)
                i = i + 1
                Tstanza(i) = (m_Stanza.Age2(isp, ist) - m_Stanza.Age1(isp, ist)) / 12.0#
                Sn = St * Math.Exp(-Tstanza(i) * FlowoutRate(ieco))

                If ist < m_Stanza.Nstanza(isp) Then
                    RecSplit(i) = St - Sn
                Else
                    RecSplit(i) = St
                End If

                St = Sn
                NstanzaBase(i) = RecSplit(i) / FlowoutRate(ieco)
                PconSplit(i) = m_SimData.pred(ieco) / NstanzaBase(i)
            Next
        Next
    End Sub

    Public Sub CalcHabitatArea()
        Dim i As Integer, j As Integer

        ReDim m_Data.HabArea(m_Data.NoHabitats)
        ReDim m_Data.HabAreaProportion(m_Data.NoHabitats)
        m_Data.ThabArea = 0

        If m_Data.NoHabitats = 0 Then Return

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then

                    'm_data.ThabArea total usable area of the map
                    m_Data.ThabArea = m_Data.ThabArea + 1
                    For ihab As Integer = 1 To Me.m_Data.NoHabitats
                        Me.m_Data.HabArea(ihab) += m_Data.PHabType(ihab)(i, j)
                    Next ihab
                End If 'm_Data.Depth(i, j) > 0
            Next j
        Next i

        If m_Data.ThabArea = 0 Then Exit Sub
        For i = 1 To m_Data.NoHabitats
            m_Data.HabAreaProportion(i) = m_Data.HabArea(i) / m_Data.ThabArea
        Next i

        m_Data.HabAreaProportion(0) = 1
        '  m_Data.m_data.ThabArea = ThabArea

    End Sub



    ''' <summary>
    ''' Local version of derivtRed() used by Ecospace during initialization ONLY 
    ''' </summary>
    ''' <param name="Biomass">Input Biomass of all the groups</param>
    ''' <param name="Flowin">Ouput Flow of biomass into a cell  </param>
    ''' <param name="FlowoutRate">Output Rate of biomass flow out of a cell</param>
    ''' <param name="EatEff">Input consumption efficiency 1 by default</param>
    ''' <param name="VulPred">Input predation vulnerability 1 by default</param>
    ''' <param name="RelProd">Input relative primary production 1 by default</param>
    ''' <remarks>
    ''' WARNING This version of derivtRed() is used only during initialization NOT during main time loop. <see cref="cSpaceSolver" >cSpaceSolver.derivtRed()</see> for time loop derivtRed().
    ''' </remarks>
    Sub derivtRed(ByVal Biomass() As Single, ByRef Flowin() As Single, ByRef FlowoutRate() As Single, ByVal EatEff() As Single, ByRef VulPred() As Single, ByVal RelProd As Single)
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'WARNING() not used for biomass calculation during the main time loop
        'This is used for initialization only 
        'Look at cSpaceSolver.derivtRed() for biomass calculation during the time loop
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'reduced derivatives for MPA equilibration procedure
        Dim i As Integer, j As Integer, ii As Integer
        Dim eat As Single, Pmult As Single
        'Dim Vprey As Single
        'Dim Shown As Boolean
        Dim SimGEt As Single
        Dim Dwe As Single
        Dim Bprey As Single

        'Detritus by group is ignored by this version of deritRed(). Each thread has its own version that it uses.
        'So we can declare it localy and never us it to update the detritus map
        Dim GrpDet() As Single
        Dim Mo As Single
        ReDim GrpDet(m_Data.NGroups)

        Dim aeff() As Single, Veff() As Single
        ReDim aeff(m_SimData.inlinks), Veff(m_SimData.inlinks)

        Dim Hdent() As Single
        ReDim Hdent(m_Data.NGroups)

        'EwE5 ToDetritus() is declared at a global level
        'in EcoSpace this is the only place it is used so its scope is local to EcoSpace
        Dim ToDetritus() As Single
        ReDim ToDetritus(m_Data.NGroups)

        If m_SimData.BioMedData.MedIsUsed(0) Then m_Ecosim.SetMedFunctions(Biomass)

        m_Ecosim.setpred(Biomass)
        ReDim m_SimData.Eatenof(m_Data.NGroups)
        ReDim m_SimData.Eatenby(m_Data.NGroups)

        Dwe = 0.5

        'set ecosim nutrients
        m_SimData.NutBiom = 0
        For i = 1 To m_Data.NGroups
            m_SimData.NutBiom += Biomass(i)
        Next
        m_SimData.NutFree = m_SimData.NutTot * RelProd - m_SimData.NutBiom
        If m_SimData.NutFree < m_SimData.NutMin Then m_SimData.NutFree = m_SimData.NutMin

        'If m_SimData.IndicesOn Then
        '    ReDim m_SimData.Consumpt(m_Data.NGroups, m_Data.NGroups)
        'End If

        For j = m_Data.nLiving + 1 To m_Data.NGroups
            ToDetritus(j - m_Data.nLiving) = 0
            'jb DetPassedOn() is not used anywhere
            ' DetPassedOn(j) = 0
        Next j

        m_Ecosim.SetRelaSwitch(Biomass)

        'get first estimate of denominators of predation rate disc equations
        Dim ia As Integer, Vbiom() As Single, Vdenom() As Single
        'this requires first estimates of vulnerable biomasses Vbiom by foraging arena
        ReDim Vbiom(m_SimData.Narena), Vdenom(m_SimData.Narena)
        For ii = 1 To m_SimData.inlinks
            i = m_SimData.ilink(ii) : j = m_SimData.jlink(ii) : ia = m_SimData.ArenaLink(ii)
            'jb EatEff() and VulPred() ignored here because this is only used for initialization and both values are 1
            aeff(ii) = m_SimData.Alink(ii) * m_SimData.Ftime(j) * m_SimData.RelaSwitch(ii)
            Veff(ia) = m_SimData.VulArena(ia) * m_SimData.Ftime(i)
            m_Ecosim.ApplyAVmodifiers(its, aeff(ii), Veff(ia), Mo, i, m_SimData.Jarena(ia), False)  '?not sure this will work right with multiple preds in arenas
            Vdenom(ia) = Vdenom(ia) + aeff(ii) * m_SimData.pred(j) / m_SimData.Hden(j)
        Next

        'then calculate first estimate using initial Hden estimates of vulnerable biomass in each arena
        For ia = 1 To m_SimData.Narena
            i = m_SimData.Iarena(ia)
            If m_SimData.BoutFeeding Then
                If Vdenom(ia) > 0 Then
                    Vbiom(ia) = Veff(ia) * Biomass(i) * (1 - Math.Exp(-Vdenom(ia))) / Vdenom(ia)
                Else
                    Vbiom(ia) = Veff(ia) * Biomass(i)
                End If
            Else
                Vbiom(ia) = Veff(ia) * Biomass(i) / (m_SimData.VulArena(ia) + Veff(ia) + Vdenom(ia))
            End If
        Next

        'then update hden estimates based on new vulnerable biomass estimates
        For ii = 1 To m_SimData.inlinks
            j = m_SimData.jlink(ii)
            ia = m_SimData.ArenaLink(ii)
            Hdent(j) = Hdent(j) + aeff(ii) * Vbiom(ia)
        Next

        For j = 1 To m_Data.NGroups
            m_SimData.Hden(j) = (1 - Dwe) * (1 + m_SimData.Htime(j) * Hdent(j)) + Dwe * m_SimData.Hden(j)
        Next

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'then update vulnerable biomass estimates using new Hden estimates (THIS MAY NOT BE NECESSARY?)
        ReDim Vbiom(m_SimData.Narena), Vdenom(m_SimData.Narena)
        For ii = 1 To m_SimData.inlinks
            i = m_SimData.ilink(ii) : j = m_SimData.jlink(ii) : ia = m_SimData.ArenaLink(ii)
            aeff(ii) = aeff(ii) * m_SimData.Ftime(j) * m_SimData.RelaSwitch(ii)
            'see ecosim derivt
            'aeff(ii) = m_ESData.Alink(ii) * m_ESData.Ftime(j) * m_ESData.RelaSwitch(ii)
            Vdenom(ia) = Vdenom(ia) + aeff(ii) * m_SimData.pred(j) / m_SimData.Hden(j)
        Next
        For ia = 1 To m_SimData.Narena
            i = m_SimData.Iarena(ia)
            If m_SimData.BoutFeeding Then
                If Vdenom(ia) > 0 Then
                    Vbiom(ia) = Veff(ia) * Biomass(i) * (1.0F - Math.Exp(-Vdenom(ia))) / Vdenom(ia)
                Else
                    Vbiom(ia) = Veff(ia) * Biomass(i)
                End If
            Else
                Vbiom(ia) = Veff(ia) * Biomass(i) / (m_SimData.VulArena(ia) + Veff(ia) + Vdenom(ia))
            End If
        Next
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'then predict consumption flows and cumulative consumptions using the new Vbiom estimates
        For ii = 1 To m_SimData.inlinks
            i = m_SimData.ilink(ii) : j = m_SimData.jlink(ii) : ia = m_SimData.ArenaLink(ii)
            If m_SimData.TrophicOff Then Bprey = m_SimData.StartBiomass(i) Else Bprey = Biomass(i)

            'prey
            ' For j = 1 To N  'VC ignore detritus; CJW had NumGroups 'predator
            '    aeff = A(i, j) * tval(SeasonType(i, j)) * Ftime(j)
            '    Veff = vulrate(i, j) * Ftime(i) * MedVal(MF(i, j))
            Select Case m_SimData.FlowType(i, j) 'prey always first
                Case 1 'donor controlled flow
                    eat = aeff(ii) * Bprey
                Case 3 'limited total flow
                    'MsgBox ("invalid flow control type setting; edit your mdb")
                    eat = aeff(ii) * Bprey * m_SimData.pred(j) / (1 + aeff(ii) * m_SimData.pred(j) * Bprey / m_SimData.maxflow(i, j))
                Case 2 'prey limited flow
                    'Vprey = Veff(ii) * Bprey / (vulrate(i, j) + Veff(ii) + aeff(ii) * pred(j) / Hden(j))
                    eat = aeff(ii) * Vbiom(ia) * m_SimData.pred(j) / m_SimData.Hden(j)
                Case Else
                    eat = 0
            End Select
            m_SimData.Eatenof(i) = m_SimData.Eatenof(i) + eat
            m_SimData.Eatenby(j) = m_SimData.Eatenby(j) + eat
            'If m_SimData.IndicesOn Then m_SimData.Consumpt(i, j) = m_SimData.Consumpt(i, j) + eat

            'If frmSim1.IndicesOn Then Consumption(i, j) = Consumption(i, j) + eat
            'ToDetritus = ToDetritus + GS(j) * eat       'DF should be considered

            'jb 
            'If m_ESData.ConSimOn = True Then
            '    If Biomass(i) > 0 Then m_ESData.ConKtrophic(ii) = eat / Biomass(i) Else m_ESData.ConKtrophic(ii) = 0
            'End If

        Next

        'Make the detritus calculations here:
        m_Ecosim.SimDetritusMT(its, Biomass, m_SimData.FishRateGear, m_SimData.Eatenby, m_SimData.Eatenof, ToDetritus, GrpDet)

        For i = 1 To m_Data.NGroups

            m_SimData.Eatenby(i) = m_SimData.Eatenby(i) + m_SimData.QBoutside(i) * Biomass(i)

            If i <= m_Data.nLiving Then      'Living group
                Pmult = 1.0#
                m_Ecosim.ApplyAVmodifiers(its, Pmult, Veff(1), Mo, i, i, False)

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Changed 3-Mar-2017
                'Carl Walters email "fixing nutrient effects on primary production in ecosim, and bug in modifying producers with forcing functions and mediation functions"
                'There is a bad setup in derivt that couples nutrient response effects to the biomass shading effects; these need to vary independently. 
                '1)      There is a line that calculates pbb(i):
                'pbb(i) = m_Data.PBmaxs(i) * m_Data.NutFree / (m_Data.NutFree + m_Data.NutFreeBase(i)) * Pmult * m_Data.pbm(i) / (1 + Biomass(i) * m_Data.pbbiomass(i))
                'change the term m_Data.PBmaxs(i) * m_Data.NutFree / (m_Data.NutFree + m_Data.NutFreeBase(i)) in this line to just
                '2.0* m_Data.NutFree / (m_Data.NutFree + m_Data.NutFreeBase(i))
                '(this allows primary production rate to as much as double as nutrient concentrations increase)
                '2)      This necessitates a change in the calculation of NutFreeBase(i) in InitialState:

                'pbb(i) = Pmult * EatEff(i) * m_SimData.PBmaxs(i) * m_SimData.NutFree / (m_SimData.NutFree + m_SimData.NutFreeBase(i)) * m_SimData.pbm(i) / (1 + Biomass(i) * PbSpace(i))
                'pbb becomes pbmaxs= pb times a max increase factor = pbm for consumers
                pbb(i) = 2 * EatEff(i) * m_SimData.NutFree / (m_SimData.NutFree + m_SimData.NutFreeBase(i)) * Pmult * m_SimData.pbm(i) / (1 + Biomass(i) * PbSpace(i))

                loss(i) = m_SimData.Eatenof(i) + (m_SimData.mo(i) * (1 - m_SimData.MoPred(i) + m_SimData.MoPred(i) * m_SimData.Ftime(i)) + m_EPdata.Emig(i) + m_SimData.FishTime(i)) * Biomass(i)
                'deriv(i) = Immig(i) + Biomass(i) * pbb(i) + simGE(i) * Eatenby(i) - loss(i)
                'biomeq(i) = (Immig(i) + simGE(i) * Eatenby(i) + pbb(i) * Biomass(i)) / (loss(i) / Biomass(i))

                If m_SimData.UseVarPQ And m_EPdata.vbK(i) > 0 Then
                    SimGEt = m_SimData.AssimEff(i) * loss(i) / Biomass(i) / (loss(i) / Biomass(i) + 3 * m_EPdata.vbK(i))
                Else
                    SimGEt = m_SimData.SimGE(i)
                End If

                Flowin(i) = m_EPdata.Immig(i) + SimGEt * m_SimData.Eatenby(i) + pbb(i) * Biomass(i)

                If Biomass(i) > 1.0E-20 Then
                    FlowoutRate(i) = loss(i) / Biomass(i)
                Else
                    FlowoutRate(i) = 100
                End If
                'If Abs(Flowin(i) - loss(i)) > 0.1 * loss(i) Then Stop
            Else                'Detritus group
                loss(i) = m_SimData.Eatenof(i) + m_EPdata.Emig(i) + m_SimData.DetritusOut(i) * Biomass(i)
                'deriv(i) = Immig(i) + ToDetritus(i - n) - loss(i)
                If loss(i) <> 0 And Biomass(i) > 0 Then
                    'biomeq(i) = (Immig(i) + ToDetritus(i - n)) / (loss(i) / Biomass(i))
                    Flowin(i) = (m_EPdata.Immig(i) + ToDetritus(i - m_Data.nLiving))
                    FlowoutRate(i) = loss(i) / Biomass(i)
                Else
                    Flowin(i) = 1.0E-20
                    'VC160398 below FlowoutRate(i) was set to 100 before
                    If Biomass(i) > 0 Then
                        FlowoutRate(i) = Flowin(i) / Biomass(i)
                    Else
                        FlowoutRate(i) = 0.0000000001
                    End If
                End If
            End If
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a scaling factor to ensure that the total primary productivity 
    ''' is the same in Ecospace and Ecopath.
    ''' </summary>
    ''' <returns>
    ''' The average PP value for all cells in the RelPP map.
    ''' Calculated from the base value of RelPP(row,col).
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function ScaleRelativePrimaryProductivityToEcopathLevel() As Double
        Dim totPP As Double
        Dim i As Integer
        Dim j As Integer

        ' Restore RelPP
        Me.m_Data.restoreBaseRelPP()

        'Make sure we know the number of water cells
        ' If Me.m_Data.nWaterCells <= 0 Then
        Me.m_Data.setNWaterCells()
        ' End If

        'This function is used to scale the relative primary productivity _
        'so that the total primary productivity is the same in Ecospace and Ecopath
        For i = 1 To Me.m_Data.InRow
            For j = 1 To Me.m_Data.InCol
                If Me.m_Data.Depth(i, j) > 0 Then 'Water
                    totPP = totPP + Me.m_Data.RelPP(i, j)
                End If
            Next
        Next

        If m_Data.nWaterCells > 0 And totPP > 0 Then
            Return totPP / m_Data.nWaterCells
        Else
            If (m_Data.nWaterCells = 0) Then
                Me.Messages.SendMessage(New cMessage(My.Resources.CoreMessages.MAP_INVALID_NOWATERCELLS, eMessageType.DataValidation, eCoreComponentType.EcoSpace, eMessageImportance.Warning))
            End If
            If (totPP = 0) Then
                Me.Messages.SendMessage(New cMessage(My.Resources.CoreMessages.MAP_INVALID_NOPP, eMessageType.DataValidation, eCoreComponentType.EcoSpace, eMessageImportance.Warning))
            End If
            Return 1
        End If

    End Function



    ''' <summary>
    ''' Scaling Sailing cost 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ScaleSailingCost()

        If Me.m_Data.bUseEffortDistThreshold Then
            Me.ScaleSailingByCells()
        Else
            Me.ScaleSailingToUnity()
        End If

    End Sub

    ''' <summary>
    ''' This function is used to scale the sailing cost so that _
    ''' it is the same in Ecospace and Ecopath
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ScaleSailingToUnity()
        Dim Factor As Single
        Dim i As Integer
        Dim j As Integer
        Dim Count As Long
        Dim GearNo As Integer

        Debug.Assert(Me.m_Data.bUseEffortDistThreshold = False, Me.ToString + ".ScaleSailingToUnity() Called with incorrect bDistEffortByCell.")

        ReDim m_Data.SailScale(m_Data.nFleets)
        m_Data.SailScale(0) = 1

        For GearNo = 1 To m_Data.nFleets
            'jb 3-May-2011 Clear Count and Factor for each fleet
            'This was not happening and SailScale() 
            'would contain the average off all fleets with a lower fleet index
            Count = 0
            Factor = 0
            For i = 1 To m_Data.InRow
                For j = 1 To m_Data.InCol
                    If m_Data.Depth(i, j) > 0 Then 'Water
                        Factor = Factor + m_Data.Sail(GearNo)(i, j)
                        Count = Count + 1
                    End If
                Next
            Next

            If Count > 0 And Factor > 0 Then
                m_Data.SailScale(GearNo) = Factor / Count
            Else
                m_Data.SailScale(GearNo) = 1
            End If

        Next GearNo

    End Sub

    Private Sub ScaleSailingByCells()
        Dim sumCost As Single
        Dim Count As Long
        Dim iFlt As Integer

        Debug.Assert(Me.m_Data.bUseEffortDistThreshold, Me.ToString + ".ScaleSailingByCells() Called with incorrect bDistEffortByCell.")

        ReDim m_Data.SailScale(m_Data.nFleets)
        m_Data.SailScale(0) = 1

        For iFlt = 1 To Me.m_Data.nFleets
            ' For GearNo = 1 To m_Data.nFleets
            'jb 3-May-2011 Clear Count and Factor for each fleet
            'This was not happening and SailScale() 
            'would contain the average off all fleets with a lower fleet index
            Count = 0
            sumCost = 0

            For Each cell As cRowCol In Me.m_Data.FleetSailCells(iFlt)
                sumCost += m_Data.Sail(iFlt)(cell.Row, cell.Col)
                Count = Count + 1
            Next cell

            If Count > 0 And sumCost > 0 Then
                m_Data.SailScale(iFlt) = sumCost / Count
            Else
                m_Data.SailScale(iFlt) = 1
            End If


        Next iFlt

    End Sub


    Private Sub SetEffortParameters(ByVal ResetTotEffort As Boolean)
        'this predicts total effort by gear type over model cells
        'accounting for habitat type restriction of each gear (gearhab(geartype,habitat))
        Dim i As Integer, j As Integer, ig As Integer
        Dim PFished As Single

        For ig = 1 To m_Data.nFleets
            m_Data.TotEffort(ig) = 0
            For i = 1 To m_Data.InRow
                For j = 1 To m_Data.InCol
                    'below changed following CJW's email of 20 Jan 98:
                    'I found one bad error in ecospace: subroutine that calculates total
                    'effort (seteffortparameters) has wrong conditions for summing total
                    'effort (should be over all cells where depth>0, remove other conditions),
                    'causing ecospace to reduce effort whenever MPA cells added (should just
                    'redistribute ecopath total, not reduce it at same time).

                    If m_Data.Depth(i, j) > 0 Then
                        'sum of habitat type in a cell fished by this gear
                        PFished = 0

                        'Is this Fleet habitat restricted
                        If m_Data.GearHab(ig, 0) = False Then
                            'Yes habitat restricted so sum the proportion of habitat types in this cell
                            For ihab As Integer = 1 To Me.m_Data.NoHabitats
                                If m_Data.GearHab(ig, ihab) Then
                                    PFished += Me.m_Data.PHabType(ihab)(i, j)
                                End If
                            Next
                        Else
                            'No this Fleet has no area/habitat restrictions
                            'So it fishes all cells 100%
                            PFished = 1
                        End If

                        'Debug.Assert(PFished <= 1.0, "Proportion of habitat in a cell not set correctly. It should sum to one for all habitat types.")

                        'set the Proportion of area fished by this fleet for all the habitats in the cell
                        Me.m_Data.PAreaFished(ig)(i, j) = PFished
                        'constrain percentage of area fished to 1.0
                        If Me.m_Data.PAreaFished(ig)(i, j) > 1.0 Then Me.m_Data.PAreaFished(ig)(i, j) = 1.0

                        'sum the weighted total effort
                        If ResetTotEffort Then

                            If Not Me.m_Data.bUseEffortDistThreshold Then
                                'Fishing is only restricted by the Habitat types
                                m_Data.TotEffort(ig) += Me.m_Data.PAreaFished(ig)(i, j)

                            Else ' Me.m_Data.bUseEffortDistThreshold  = True
                                'Fishing is also restricted by sailing cost < effort distribution threshold
                                If Me.m_Data.Sail(ig)(i, j) < Me.m_Data.EffortDistThreshold Then
                                    m_Data.TotEffort(ig) += Me.m_Data.PAreaFished(ig)(i, j)
                                Else
                                    'Sailing cost > effort distribution threshold
                                    'So this fleet is not fishing in this cell
                                    Me.m_Data.PAreaFished(ig)(i, j) = 0
                                End If 'Me.m_Data.Sail(ig, i, j) < Me.m_Data.EffortDistThreshold

                            End If 'Me.m_Data.bUseEffortDistThreshold
                        End If 'ResetTotEffort
                    End If 'm_Data.Depth(i, j) > 0

                Next j 'map cols
            Next i 'map rows

        Next ig ' fleets

    End Sub

    Sub SetMovementParameters()
        'sets solvegrid movement arrays based on depth map
        Dim i As Integer, j As Integer, ip As Integer, AdScale As Single ', iad As Integer, iju As Integer
        Dim isp As Integer, ist As Integer, nvar2 As Integer, ir As Integer, ieco As Integer

        Me.m_Data.allocate(Bcw, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
        Me.m_Data.allocate(C, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
        'd movement to right
        Me.m_Data.allocate(d, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
        'e movement to left
        Me.m_Data.allocate(e, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)

        'Advection vectors Xvel(,) are in cm/sec convert to km/year, same units as the mrate()
        '[km/year] / [cell length]
        AdScale = 315.36 / m_Data.CellLength

        'set depth for the boundary cells to be equal to the depth just inside the model
        m_Data.Width(0) = m_Data.Width(1)
        'm_Data.Width(m_Data.InRow + 1) = m_Data.Width(m_Data.InRow)
        For i = 1 To m_Data.InRow
            m_Data.Depth(i, 0) = m_Data.Depth(i, 1)
            m_Data.Depth(i, m_Data.InCol + 1) = m_Data.Depth(i, m_Data.InCol)
            If m_Data.Depth(i, 0) > 0 Then
                m_Data.Xvel(i, 0) = m_Data.Xvel(i, 1)
                m_Data.Yvel(i, 0) = m_Data.Yvel(i, 1)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(i, 0) = m_Data.HabCap(ip)(i, 1)
                Next
            End If
            If m_Data.Depth(i, m_Data.InCol + 1) > 0 Then
                m_Data.Xvel(i, m_Data.InCol + 1) = m_Data.Xvel(i, m_Data.InCol)
                m_Data.Yvel(i, m_Data.InCol + 1) = m_Data.Yvel(i, m_Data.InCol)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(i, m_Data.InCol + 1) = m_Data.HabCap(ip)(i, m_Data.InCol)
                Next
            End If
        Next
        For j = 1 To m_Data.InCol
            m_Data.Depth(0, j) = m_Data.Depth(1, j)
            m_Data.Depth(m_Data.InRow + 1, j) = m_Data.Depth(m_Data.InRow, j)
            If m_Data.Depth(0, j) > 0 Then
                m_Data.Xvel(0, j) = m_Data.Xvel(1, j)
                m_Data.Yvel(0, j) = m_Data.Yvel(1, j)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(0, j) = m_Data.HabCap(ip)(1, j)
                Next
            End If
            If m_Data.Depth(m_Data.InRow + 1, j) > 0 Then
                m_Data.Xvel(m_Data.InRow + 1, j) = m_Data.Xvel(m_Data.InRow, j)
                m_Data.Yvel(m_Data.InRow + 1, j) = m_Data.Yvel(m_Data.InRow, j)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(m_Data.InRow + 1, j) = m_Data.HabCap(ip)(m_Data.InRow, j)
                Next
            End If
        Next

        For i = 0 To m_Data.InRow
            For j = 0 To m_Data.InCol
                'check depth on right face of this cell
                If m_Data.Depth(i, j) > 0 Then
                    If m_Data.Depth(i, j + 1) > 0 Then

                        For ip = 1 To m_Data.NGroups
                            If j > 0 And j < m_Data.InCol Then

                                If m_Data.HabCap(ip)(i, j + 1) = m_Data.HabCap(ip)(i, j) Then
                                    d(i, j, ip) = m_Data.Mrate(ip)
                                    e(i, j + 1, ip) = m_Data.Mrate(ip)
                                ElseIf m_Data.HabCap(ip)(i, j + 1) > m_Data.HabCap(ip)(i, j) Then
                                    d(i, j, ip) = m_Data.Mrate(ip)
                                    e(i, j + 1, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i, j) / m_Data.HabCap(ip)(i, j + 1)
                                Else
                                    d(i, j, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i, j + 1) / m_Data.HabCap(ip)(i, j)
                                    e(i, j + 1, ip) = m_Data.Mrate(ip)
                                End If

                                'e(i, j + 1, ip) = m_Data.Mrate(ip) * RelMove(ip, i, j + 1) * RelHabMove(i, j + 1, i, j, Me.HabGrad, m_Data.MoveScale, ip)
                                'd(i, j, ip) = m_Data.Mrate(ip) * RelMove(ip, i, j) * RelHabMove(i, j, i, j + 1, Me.HabGrad, m_Data.MoveScale, ip)
                                If m_Data.IsAdvected(ip) Then
                                    If m_Data.Xvel(i, j) > 0 Then
                                        d(i, j, ip) = d(i, j, ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                    Else
                                        e(i, j + 1, ip) = e(i, j + 1, ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                    End If

                                End If
                            Else
                                If m_Data.IsAdvected(ip) Then
                                    If m_Data.Xvel(i, j) > 0 Then
                                        e(i, j + 1, ip) = m_Data.Mrate(ip) 'into j from right
                                        d(i, j, ip) = m_Data.Mrate(ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                    Else
                                        e(i, j + 1, ip) = m_Data.Mrate(ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                        d(i, j, ip) = m_Data.Mrate(ip) 'from j to the right

                                    End If
                                Else
                                    e(i, j + 1, ip) = 0
                                    d(i, j, ip) = 0
                                End If
                            End If
                            Enomig(i, j + 1, ip) = e(i, j + 1, ip)
                            dNomig(i, j, ip) = d(i, j, ip)
                        Next ip

                        'EwE5
                        ' nvar2 = nvar + 2 * npairs
                        nvar2 = m_Data.NGroups
                        ir = 0
                        For isp = 1 To m_Stanza.Nsplit
                            For ist = 1 To m_Stanza.Nstanza(isp)
                                ieco = m_Stanza.EcopathCode(isp, ist)
                                ir = ir + 1
                                e(i, j + 1, nvar2 + ir) = e(i, j + 1, ieco)
                                d(i, j, nvar2 + ir) = d(i, j, ieco)
                                Enomig(i, j + 1, nvar2 + ir) = e(i, j + 1, ieco)
                                dNomig(i, j, nvar2 + ir) = d(i, j, ieco)
                            Next
                        Next
                    End If
                    'then check depths on bottom face of this cell
                    If m_Data.Depth(i + 1, j) > 0 Then
                        For ip = 1 To m_Data.NGroups
                            If i > 0 And i < m_Data.InRow Then

                                If m_Data.HabCap(ip)(i + 1, j) = m_Data.HabCap(ip)(i, j) Then
                                    Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) '* RelMove(ip, i, j) * RelHabMove(i, j, i + 1, j, HabGrad, m_Data.MoveScale, ip)
                                    C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i)
                                ElseIf m_Data.HabCap(ip)(i + 1, j) > m_Data.HabCap(ip)(i, j) Then
                                    Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) '* RelMove(ip, i, j) * RelHabMove(i, j, i + 1, j, HabGrad, m_Data.MoveScale, ip)
                                    C(i, j, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i, j) / m_Data.HabCap(ip)(i + 1, j) * m_Data.Width(i) 'RelMove(ip, i + 1, j) * RelHabMove(i + 1, j, i, j, HabGrad, m_Data.MoveScale, ip)
                                Else
                                    Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i + 1, j) / m_Data.HabCap(ip)(i, j) * m_Data.Width(i)
                                    C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) '* RelMove(ip, i + 1, j) * RelHabMove(i + 1, j, i, j, HabGrad, m_Data.MoveScale, ip)
                                End If
                                'C(i, j, ip) = m_Data.Mrate(ip) * RelMove(ip, i + 1, j) * RelHabMove(i + 1, j, i, j, HabGrad, m_Data.MoveScale, ip)
                                'Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * RelMove(ip, i, j) * RelHabMove(i, j, i + 1, j, HabGrad, m_Data.MoveScale, ip)
                                If m_Data.IsAdvected(ip) Then
                                    'jb 1-Dec-2016 Include cell width scaler in Y velocity movements
                                    If m_Data.Yvel(i, j) > 0 Then
                                        Bcw(i + 1, j, ip) = Bcw(i + 1, j, ip) + m_Data.Yvel(i, j) * AdScale * m_Data.Width(i)
                                    Else
                                        C(i, j, ip) = C(i, j, ip) - m_Data.Yvel(i, j) * AdScale * m_Data.Width(i)
                                    End If

                                End If
                            Else
                                If m_Data.IsAdvected(ip) Then
                                    If m_Data.Yvel(i, j) > 0 Then
                                        C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) 'from row i+1 to i
                                        Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) + m_Data.Yvel(i, j) * AdScale ' + AdvectSouth 'from i to i+1
                                    Else
                                        C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) - m_Data.Yvel(i, j) * AdScale 'from row i+1 to i
                                        Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i)
                                    End If
                                Else
                                    C(i, j, ip) = 0
                                    Bcw(i + 1, j, ip) = 0
                                End If
                            End If
                            CNomig(i, j, ip) = C(i, j, ip)
                            BcwNomig(i + 1, j, ip) = Bcw(i + 1, j, ip)
                        Next

                        'EwE5
                        ' nvar2 = nvar + 2 * npairs
                        nvar2 = m_Data.NGroups
                        ir = 0
                        For isp = 1 To m_Stanza.Nsplit
                            For ist = 1 To m_Stanza.Nstanza(isp)
                                ieco = m_Stanza.EcopathCode(isp, ist)
                                ir = ir + 1
                                Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ieco)
                                C(i, j, nvar2 + ir) = C(i, j, ieco)
                                BcwNomig(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ieco)
                                CNomig(i, j, nvar2 + ir) = C(i, j, ieco)
                            Next
                        Next
                    End If
                End If

            Next j
        Next i

        'Me.debugDumpFlowRates(Bcw, 24, "SetMovementParameters b")
        'Me.debugDumpFlowRates(C, 24, "SetMovementParameters c")
        'Me.debugDumpFlowRates(d, 24, "SetMovementParameters d")
        'Me.debugDumpFlowRates(e, 24, "SetMovementParameters e")

        If m_tracerData.EcoSpaceConSimOn Then
            'set movement rates for physical contaminant concentration to
            'rates for first detritus pool
            For i = 0 To m_Data.InRow + 1
                For j = 0 To m_Data.InCol + 1
                    Bcw(i, j, 0) = Bcw(i, j, m_EPdata.NumLiving + 1)
                    C(i, j, 0) = C(i, j, m_EPdata.NumLiving + 1)
                    d(i, j, 0) = d(i, j, m_EPdata.NumLiving + 1)
                    e(i, j, 0) = e(i, j, m_EPdata.NumLiving + 1)
                    BcwNomig(i, j, 0) = Bcw(i, j, m_EPdata.NumLiving + 1)
                    CNomig(i, j, 0) = C(i, j, m_EPdata.NumLiving + 1)
                    dNomig(i, j, 0) = d(i, j, m_EPdata.NumLiving + 1)
                    Enomig(i, j, 0) = e(i, j, m_EPdata.NumLiving + 1)
                Next
            Next
        End If
    End Sub


    Sub SetMovementParameters_NoOffset()
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'jb 6-Dec-2016 OK This fixes the issue with the velocity vectors being off set by one cell 
        'But the model goes unstable after a long run... So not so good ehhh
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'sets solvegrid movement arrays based on depth map
        Dim i As Integer, j As Integer, ip As Integer, AdScale As Single ', iad As Integer, iju As Integer
        Dim isp As Integer, ist As Integer, nvar2 As Integer, ir As Integer, ieco As Integer

        Me.m_Data.allocate(Bcw, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
        Me.m_Data.allocate(C, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
        'd movement to right
        Me.m_Data.allocate(d, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)
        'e movement to left
        Me.m_Data.allocate(e, m_Data.InRow + 1, m_Data.InCol + 1, m_Data.nvartot)

        'Advection vectors Xvel(,) are in cm/sec convert to km/year, same units as the mrate()
        '[km/year] / [cell length]
        AdScale = 315.36 / m_Data.CellLength

        'set depth for the boundary cells to be equal to the depth just inside the model
        m_Data.Width(0) = m_Data.Width(1)
        'm_Data.Width(m_Data.InRow + 1) = m_Data.Width(m_Data.InRow)
        For i = 1 To m_Data.InRow
            m_Data.Depth(i, 0) = m_Data.Depth(i, 1)
            m_Data.Depth(i, m_Data.InCol + 1) = m_Data.Depth(i, m_Data.InCol)
            If m_Data.Depth(i, 0) > 0 Then
                m_Data.Xvel(i, 0) = m_Data.Xvel(i, 1)
                m_Data.Yvel(i, 0) = m_Data.Yvel(i, 1)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(i, 0) = m_Data.HabCap(ip)(i, 1)
                Next
            End If
            If m_Data.Depth(i, m_Data.InCol + 1) > 0 Then
                m_Data.Xvel(i, m_Data.InCol + 1) = m_Data.Xvel(i, m_Data.InCol)
                m_Data.Yvel(i, m_Data.InCol + 1) = m_Data.Yvel(i, m_Data.InCol)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(i, m_Data.InCol + 1) = m_Data.HabCap(ip)(i, m_Data.InCol)
                Next
            End If
        Next
        For j = 1 To m_Data.InCol
            m_Data.Depth(0, j) = m_Data.Depth(1, j)
            m_Data.Depth(m_Data.InRow + 1, j) = m_Data.Depth(m_Data.InRow, j)
            If m_Data.Depth(0, j) > 0 Then
                m_Data.Xvel(0, j) = m_Data.Xvel(1, j)
                m_Data.Yvel(0, j) = m_Data.Yvel(1, j)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(0, j) = m_Data.HabCap(ip)(1, j)
                Next
            End If
            If m_Data.Depth(m_Data.InRow + 1, j) > 0 Then
                m_Data.Xvel(m_Data.InRow + 1, j) = m_Data.Xvel(m_Data.InRow, j)
                m_Data.Yvel(m_Data.InRow + 1, j) = m_Data.Yvel(m_Data.InRow, j)
                For ip = 1 To m_Data.NGroups
                    m_Data.HabCap(ip)(m_Data.InRow + 1, j) = m_Data.HabCap(ip)(m_Data.InRow, j)
                Next
            End If
        Next

        For i = 0 To m_Data.InRow
            For j = 0 To m_Data.InCol
                'is this cell water
                If m_Data.Depth(i, j) > 0 Then
                    'Yes we are in a water cell
                    'check depth on right face of this cell
                    If m_Data.Depth(i, j + 1) > 0 Then

                        For ip = 1 To m_Data.NGroups
                            If j > 0 And j < m_Data.InCol + 1 Then

                                If m_Data.HabCap(ip)(i, j + 1) = m_Data.HabCap(ip)(i, j) Then
                                    d(i, j, ip) = m_Data.Mrate(ip)
                                    e(i, j + 1, ip) = m_Data.Mrate(ip)
                                ElseIf m_Data.HabCap(ip)(i, j + 1) > m_Data.HabCap(ip)(i, j) Then
                                    d(i, j, ip) = m_Data.Mrate(ip)
                                    e(i, j + 1, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i, j) / m_Data.HabCap(ip)(i, j + 1)
                                Else
                                    d(i, j, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i, j + 1) / m_Data.HabCap(ip)(i, j)
                                    e(i, j + 1, ip) = m_Data.Mrate(ip)
                                End If

                                'e(i, j + 1, ip) = m_Data.Mrate(ip) * RelMove(ip, i, j + 1) * RelHabMove(i, j + 1, i, j, Me.HabGrad, m_Data.MoveScale, ip)
                                'd(i, j, ip) = m_Data.Mrate(ip) * RelMove(ip, i, j) * RelHabMove(i, j, i, j + 1, Me.HabGrad, m_Data.MoveScale, ip)
                                If m_Data.IsAdvected(ip) Then
                                    If m_Data.Xvel(i, j) > 0 Then
                                        d(i, j, ip) = d(i, j, ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                        ' d(i, j, ip) = d(i, j, ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                    Else
                                        ' e(i, j + 1, ip) = e(i, j + 1, ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                        e(i, j, ip) = e(i, j, ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                    End If

                                End If
                            Else
                                'Outside the grid bounds Col=0 or n+1
                                If m_Data.IsAdvected(ip) Then
                                    If m_Data.Xvel(i, j) > 0 Then
                                        e(i, j + 1, ip) = m_Data.Mrate(ip) 'into j from right
                                        d(i, j, ip) = m_Data.Mrate(ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                    Else
                                        e(i, j + 1, ip) = m_Data.Mrate(ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                        d(i, j, ip) = m_Data.Mrate(ip) 'from j to the right

                                    End If
                                Else
                                    e(i, j + 1, ip) = 0
                                    d(i, j, ip) = 0
                                End If
                            End If
                            Enomig(i, j + 1, ip) = e(i, j + 1, ip)
                            dNomig(i, j, ip) = d(i, j, ip)
                        Next ip

                        'EwE5
                        ' nvar2 = nvar + 2 * npairs
                        nvar2 = m_Data.NGroups
                        ir = 0
                        For isp = 1 To m_Stanza.Nsplit
                            For ist = 1 To m_Stanza.Nstanza(isp)
                                ieco = m_Stanza.EcopathCode(isp, ist)
                                ir = ir + 1
                                e(i, j + 1, nvar2 + ir) = e(i, j + 1, ieco)
                                d(i, j, nvar2 + ir) = d(i, j, ieco)
                                Enomig(i, j + 1, nvar2 + ir) = e(i, j + 1, ieco)
                                dNomig(i, j, nvar2 + ir) = d(i, j, ieco)
                            Next
                        Next
                    End If 'm_Data.Depth(i, j + 1) > 0
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'end of col
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    'xxxxxxxxxxxxxxxxxxxxxxxxxx
                    'Start or rows
                    'xxxxxxxxxxxxxxxxxxxxxx
                    'then check depths on bottom face of this cell
                    If m_Data.Depth(i + 1, j) > 0 Then
                        For ip = 1 To m_Data.NGroups
                            If i > 0 And i < m_Data.InRow + 1 Then

                                If m_Data.HabCap(ip)(i + 1, j) = m_Data.HabCap(ip)(i, j) Then
                                    Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) '* RelMove(ip, i, j) * RelHabMove(i, j, i + 1, j, HabGrad, m_Data.MoveScale, ip)
                                    C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i)
                                ElseIf m_Data.HabCap(ip)(i + 1, j) > m_Data.HabCap(ip)(i, j) Then
                                    Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) '* RelMove(ip, i, j) * RelHabMove(i, j, i + 1, j, HabGrad, m_Data.MoveScale, ip)
                                    C(i, j, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i, j) / m_Data.HabCap(ip)(i + 1, j) * m_Data.Width(i) 'RelMove(ip, i + 1, j) * RelHabMove(i + 1, j, i, j, HabGrad, m_Data.MoveScale, ip)
                                Else
                                    Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.HabCap(ip)(i + 1, j) / m_Data.HabCap(ip)(i, j) * m_Data.Width(i)
                                    C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) '* RelMove(ip, i + 1, j) * RelHabMove(i + 1, j, i, j, HabGrad, m_Data.MoveScale, ip)
                                End If
                                'C(i, j, ip) = m_Data.Mrate(ip) * RelMove(ip, i + 1, j) * RelHabMove(i + 1, j, i, j, HabGrad, m_Data.MoveScale, ip)
                                'Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * RelMove(ip, i, j) * RelHabMove(i, j, i + 1, j, HabGrad, m_Data.MoveScale, ip)
                                If m_Data.IsAdvected(ip) Then
                                    'Debug.Assert(i <> 4)
                                    'jb 1-Dec-2016 Include cell width scaler in Y velocity movements
                                    If m_Data.Yvel(i, j) > 0 Then
                                        ' Bcw(i + 1, j, ip) = Bcw(i + 1, j, ip) + m_Data.Yvel(i, j) * AdScale * m_Data.Width(i)
                                        Bcw(i, j, ip) = Bcw(i, j, ip) + m_Data.Yvel(i, j) * AdScale * m_Data.Width(i)
                                    Else
                                        C(i, j, ip) = C(i, j, ip) - m_Data.Yvel(i, j) * AdScale * m_Data.Width(i)
                                    End If

                                End If
                            Else
                                If m_Data.IsAdvected(ip) Then

                                    If m_Data.Yvel(i, j) > 0 Then
                                        C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) 'from row i+1 to i
                                        Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) + m_Data.Yvel(i, j) * AdScale ' + AdvectSouth 'from i to i+1
                                    Else
                                        C(i, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i) - m_Data.Yvel(i, j) * AdScale 'from row i+1 to i
                                        Bcw(i + 1, j, ip) = m_Data.Mrate(ip) * m_Data.Width(i)
                                    End If
                                Else
                                    C(i, j, ip) = 0
                                    Bcw(i + 1, j, ip) = 0
                                End If
                            End If
                            CNomig(i, j, ip) = C(i, j, ip)
                            BcwNomig(i + 1, j, ip) = Bcw(i + 1, j, ip)
                        Next

                        'EwE5
                        ' nvar2 = nvar + 2 * npairs
                        nvar2 = m_Data.NGroups
                        ir = 0
                        For isp = 1 To m_Stanza.Nsplit
                            For ist = 1 To m_Stanza.Nstanza(isp)
                                ieco = m_Stanza.EcopathCode(isp, ist)
                                ir = ir + 1
                                Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ieco)
                                C(i, j, nvar2 + ir) = C(i, j, ieco)
                                BcwNomig(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ieco)
                                CNomig(i, j, nvar2 + ir) = C(i, j, ieco)
                            Next
                        Next
                    End If
                End If

            Next j
        Next i

        'Me.debugDumpFlowRates(Bcw, Me.m_Data.nLiving + 1, "SetMovementParameters b")
        'Me.debugDumpFlowRates(C, Me.m_Data.nLiving + 1, "SetMovementParameters c")
        'Me.debugDumpFlowRates(d, Me.m_Data.nLiving + 1, "SetMovementParameters d")
        'Me.debugDumpFlowRates(e, Me.m_Data.nLiving + 1, "SetMovementParameters e")

        If m_tracerData.EcoSpaceConSimOn Then
            'set movement rates for physical contaminant concentration to
            'rates for first detritus pool
            For i = 0 To m_Data.InRow + 1
                For j = 0 To m_Data.InCol + 1
                    Bcw(i, j, 0) = Bcw(i, j, m_EPdata.NumLiving + 1)
                    C(i, j, 0) = C(i, j, m_EPdata.NumLiving + 1)
                    d(i, j, 0) = d(i, j, m_EPdata.NumLiving + 1)
                    e(i, j, 0) = e(i, j, m_EPdata.NumLiving + 1)
                    BcwNomig(i, j, 0) = Bcw(i, j, m_EPdata.NumLiving + 1)
                    CNomig(i, j, 0) = C(i, j, m_EPdata.NumLiving + 1)
                    dNomig(i, j, 0) = d(i, j, m_EPdata.NumLiving + 1)
                    Enomig(i, j, 0) = e(i, j, m_EPdata.NumLiving + 1)
                Next
            Next
        End If
    End Sub

    Private Sub debugDumpFlowRates(flowArray(,,) As Single, iGrp As Integer, Optional msg As String = " ")
        Dim tempstr As String
        Debug.Print(msg)
        Debug.Print("Flow for " + iGrp.ToString)
        For i As Integer = 1 To m_Data.InRow '+ 1
            For j As Integer = 1 To m_Data.InCol ' + 1
                tempstr = tempstr + Math.Round(flowArray(i, j, iGrp), 10).ToString.PadRight(20)
            Next
            Debug.Print(tempstr)
            tempstr = ""
        Next
    End Sub


    Private Function getMoveRate(igrp As Integer, imonth As Integer, irow As Integer, iCol As Integer) As Single
        If Not Me.m_Data.IsMigratory(igrp) Then
            Return Me.m_Data.Mrate(igrp)
        Else
            If Me.m_Data.MigMaps(igrp, imonth)(irow, iCol) > MIN_MIG_PROB Then
                Return m_Data.Mvel(igrp) / (3.14159 * m_Data.CellLength)
            Else
                Return Me.m_Data.Mrate(igrp)
            End If
        End If
    End Function


    ''' <summary>
    ''' Returns the relative movement from bad habitat multiplier based on preferred habitat and percentage of habitat type in the cell.
    ''' <see cref="cEcospaceDataStructures.PrefHab">Preferred habitat</see> 
    ''' <see cref="cEcospaceDataStructures.PHabType">Percentage of habitat type in a cell</see>
    ''' </summary>
    ''' <param name="ip">Group index</param>
    ''' <param name="i">Map Row</param>
    ''' <param name="j">Map Col</param>
    ''' <returns>
    ''' One if this cell contains any preferred habitat for this group.
    ''' Otherwise returns Relative movement from bad habitat from the interface.
    ''' </returns>
    ''' <remarks>
    ''' This is only used for the migration movement <see cref="cEcoSpace.VaryMigMovementParameters"> Migration movement</see>. 
    ''' Movement/difussion rates across cells are set in <see cref="cEcoSpace.SetMovementParameters">SetMovementParameters</see> 
    ''' based on habitat capacity <see cref="cEcospaceDataStructures.HabCap">HabCap</see>.
    ''' </remarks>
    Function RelMove(ByVal ip As Integer, ByVal i As Integer, ByVal j As Integer) As Single
        'calculates relative movemement rate out of cell i,j for pool/species ip, as function of
        'habitat state in cell i,j

        If m_Data.PrefHab(ip, 0) > 0 Then
            'this group uses all habitats
            Return 1.0F
        End If

        'If there is any preferred habitat in this cell then don't move out
        For ihab As Integer = 1 To Me.m_Data.NoHabitats
            If m_Data.PrefHab(ip, ihab) > 0 And m_Data.PHabType(ihab)(i, j) > 0 Then
                'There is some preferred habitat in this cell 
                'so return one
                Return 1.0F
            End If
        Next

        'No preferred habitat in this cell 
        Return m_Data.RelMoveBad(ip)

        'jb before PHabType() percentage of each habitat type in a cell
        'If (m_Data.PrefHab(ip, m_Data.HabType(i, j)) > 0) Or (m_Data.PrefHab(ip, 0) > 0) Then
        '    RelMove = 1
        'Else
        '    RelMove = m_Data.RelMoveBad(ip)
        'End If

    End Function


    Function RelHabMove(ByVal i1 As Integer, ByVal j1 As Integer, ByVal i2 As Integer, ByVal j2 As Integer, ByVal G(,,) As Single, ByVal gk As Single, ByVal ihab As Integer) As Single
        'sets relative movement rate using slope of g() function between origin (i1,j1) and destination (i2,j2) cells
        'function is 1 when slope ss is zero
        Dim Ss As Single
        Ss = G(i2, j2, ihab) - G(i1, j1, ihab)
        Select Case Ss
            Case 0
                RelHabMove = 1
            Case Is > 0
                RelHabMove = 2 / (1 + Math.Exp(-gk * Ss))
            Case Is < 0
                RelHabMove = 0.01
            Case Else
                Stop
        End Select
    End Function


    Sub SolveGrid(ByVal ip As Integer, ByVal Aloc(,,) As Single, ByVal Floc(,,) As Single, ByVal X(,,) As Single, ByVal M As Integer, ByVal NomCols As Integer, ByVal Tol As Single, ByVal jord() As Integer, ByVal W As Single)
        'this routine solves for equilibrium field of concentrations x over a grid
        ' x(i,j) is equilibrium concentration of x in grid cell i,j
        'am(i,j) is total loss rate of x from cell i,j...NB:am(i,j)<0 !!!!!!
        'b(i,j) is loss rate from element i-1 to i in column j of grid
        'c(i,j) is loss rate from element i+1 to i in column j of grid
        'd(i,j) is loss rate from element j to element j+1 in row i of grid
        'e(i,j) is loss rate from element j to element j-1 in row i of grid
        'f(i,j) is forcing input to element i,j from sources outside the grid
        'm is number of rows (i) in grid
        'NomCols is number of columns (j) in grid
        'tol is tolerance limit for change in iterative solution
        'jord(k) is which column j to do as k=1, k=2,...,k=n (iteration order)
        'w is SOR overrelaxation parameter-found 1.25 to be good for typical problems
        Dim iter As Integer, j As Integer, i As Integer, jj As Integer, ic As Integer

        Dim alfa(,) As Single
        Dim gam(,) As Single
        Dim rhs(,) As Single
        Dim G() As Single
        Dim Xold(,) As Single
        ReDim alfa(M + 1, NomCols + 1)
        ReDim gam(M + 1, NomCols + 1)
        ReDim rhs(M + 1, NomCols + 1)
        ReDim G(M + 1)
        ReDim Xold(M + 1, NomCols + 1)

        'first compute LU decomposition elements for each column j
        'If StopRun = 1 Then Exit Sub
        For j = 1 To NomCols
            Xold(1, j) = X(1, j, ip)
            alfa(1, j) = Aloc(1, j, ip)
            gam(1, j) = C(1, j, ip) / alfa(1, j)
            For i = 2 To M
                Xold(i, j) = X(i, j, ip)
                alfa(i, j) = Aloc(i, j, ip) - Bcw(i, j, ip) * gam(i - 1, j)
                gam(i, j) = C(i, j, ip) / alfa(i, j)
            Next
        Next
        'now begin block Gauss-Seidel/SOR iteration over columns of grid
        'at each iteration, solve explicitly for values in each column given
        'current estimates of "forcing" input from other columns based on their
        'current estimates
        iter = 0
iterate:
        For jj = 1 To NomCols

            j = jord(jj)
            For i = 1 To M
                rhs(i, j) = -Floc(i, j, ip) - d(i, j - 1, ip) * X(i, j - 1, ip) - e(i, j + 1, ip) * X(i, j + 1, ip)
            Next
            rhs(1, j) = rhs(1, j) - Bcw(1, j, ip) * X(0, j, ip)
            rhs(M, j) = rhs(M, j) - C(M, j, ip) * X(M + 1, j, ip)
            'now solve for x(i,j) over i using these forcing inputs to one dimensional
            'tridiagonal solver
            G(1) = rhs(1, j) / alfa(1, j)
            'IF iflag > 0 THEN FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT: STOP
            For i = 2 To M
                G(i) = (rhs(i, j) - Bcw(i, j, ip) * G(i - 1)) / alfa(i, j)
            Next
            X(M, j, ip) = G(M)
            For i = M - 1 To 1 Step -1
                X(i, j, ip) = G(i) - gam(i, j) * X(i + 1, j, ip)
            Next
            'IF iflag > 0 THEN
            '        FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT
            '        PRINT FRE(-1), FRE(-2)
            '        : STOP
            'END IF
            For i = 1 To M
                X(i, j, ip) = (1 - W) * Xold(i, j) + W * X(i, j, ip)
            Next
        Next

        ic = 0
        For i = 1 To M
            For j = 1 To NomCols
                If m_Data.Depth(i, j) > 0 Then

                    If Math.Abs(X(i, j, ip) - Xold(i, j)) > Tol Then
                        ic = ic + 1
                    End If
                    Xold(i, j) = X(i, j, ip)
                    If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                        Xold(i, j) = 0
                    End If

                End If
            Next
        Next
        'LOCATE 1, 1: Print "SOR it="; iter;: LOCATE 2, 1: Print "    nc="; ic;
        ' Label12.Caption = iter : Label13.Caption = ic 'DoEvents
        iter = iter + 1
        If ic > 0 And iter < 20 Then GoTo iterate
        'CLS
        'LOCATE 1, 1
        'FOR i = 1 TO 20: PRINT USING "## "; i; : FOR j = 1 TO nomcols: PRINT USING " .##"; x(i, j); : NEXT: PRINT : NEXT
        'WHILE INKEY$ = "": WEND
exitline:
        Erase alfa, gam, rhs, G, Xold

    End Sub

    Sub SolveGridRow(ByVal ip As Integer, ByVal Aloc(,,) As Single, ByVal Floc(,,) As Single, ByVal X(,,) As Single, ByVal M As Integer, ByVal NomCols As Integer, ByVal Tol As Single, ByVal jord() As Integer, ByVal W As Single)
        'this routine solves for equilibrium field of concentrations x over a grid
        ' x(i,j) is equilibrium concentration of x in grid cell i,j
        'am(i,j) is total loss rate of x from cell i,j...NB:am(i,j)<0 !!!!!!
        'b(i,j) is loss rate from element i-1 to i in column j of grid
        'c(i,j) is loss rate from element i+1 to i in column j of grid
        'd(i,j) is loss rate from element j to element j+1 in row i of grid
        'e(i,j) is loss rate from element j to element j-1 in row i of grid
        'f(i,j) is forcing input to element i,j from sources outside the grid
        'm is number of rows (i) in grid
        'NomCols is number of columns (j) in grid
        'tol is tolerance limit for change in iterative solution
        'jord(k) is which column j to do as k=1, k=2,...,k=n (iteration order)
        'w is SOR overrelaxation parameter-found 1.25 to be good for typical problems
        Dim iter As Integer, j As Integer, i As Integer, ic As Integer ', ii As Integer

        Dim alfa(,) As Single
        Dim gam(,) As Single
        Dim rhs(,) As Single
        Dim G() As Single
        Dim Xold(,) As Single
        ReDim alfa(M + 1, NomCols + 1)
        ReDim gam(M + 1, NomCols + 1)
        ReDim rhs(M + 1, NomCols + 1)
        ReDim G(NomCols + 1)
        ReDim Xold(M + 1, NomCols + 1)

        'first compute LU decomposition elements for each column j
        'If StopRun = 1 Then Exit Sub

        For i = 1 To M
            Xold(i, 1) = X(i, 1, ip)
            alfa(i, 1) = Aloc(i, 1, ip) : gam(i, 1) = e(i, 2, ip) / alfa(i, 1)
            For j = 2 To NomCols
                Xold(i, j) = X(i, j, ip)
                alfa(i, j) = Aloc(i, j, ip) - d(i, j - 1, ip) * gam(i, j - 1)
                gam(i, j) = e(i, j + 1, ip) / alfa(i, j)
            Next
        Next
        'now begin block Gauss-Seidel/SOR iteration over columns of grid
        'at each iteration, solve explicitly for values in each column given
        'current estimates of "forcing" input from other columns based on their
        'current estimates
        iter = 0
iterate:
        For i = 1 To M
            ' If StopRun = 1 Then Exit Sub
            'j = jord(jj)
            For j = 1 To NomCols
                rhs(i, j) = -Floc(i, j, ip) - Bcw(i, j, ip) * X(i - 1, j, ip) - C(i, j, ip) * X(i + 1, j, ip)
            Next
            rhs(i, 1) = rhs(i, 1) - d(i, 0, ip) * X(i, 0, ip)
            rhs(i, NomCols) = rhs(i, NomCols) - e(i, NomCols + 1, ip) * X(i, NomCols + 1, ip)
            'now solve for x(i,j) over i using these forcing inputs to one dimensional
            'tridiagonal solver
            G(1) = rhs(i, 1) / alfa(i, 1)
            'IF iflag > 0 THEN FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT: STOP
            For j = 2 To NomCols
                G(j) = (rhs(i, j) - d(i, j - 1, ip) * G(j - 1)) / alfa(i, j)
            Next
            X(i, NomCols, ip) = G(NomCols)
            For j = NomCols - 1 To 1 Step -1
                X(i, j, ip) = G(j) - gam(i, j) * X(i, j + 1, ip)
            Next
            'IF iflag > 0 THEN
            '        FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT
            '        PRINT FRE(-1), FRE(-2)
            '        : STOP
            'END IF
            For j = 1 To NomCols
                X(i, j, ip) = (1 - W) * Xold(i, j) + W * X(i, j, ip)
            Next
        Next

        ic = 0
        For i = 1 To M
            For j = 1 To NomCols
                If m_Data.Depth(i, j) > 0 Then

                    If Math.Abs(X(i, j, ip) - Xold(i, j)) > Tol Then
                        ic = ic + 1
                    End If
                    Xold(i, j) = X(i, j, ip)
                    If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                        Xold(i, j) = 0 ': Stop
                    End If

                End If
            Next j
        Next i
        'LOCATE 1, 1: Print "SOR it="; iter;: LOCATE 2, 1: Print "    nc="; ic;
        ' Label12.Caption = iter: Label13.Caption = ic: 'DoEvents
        iter = iter + 1
        If ic > 0 And iter < 20 Then GoTo iterate
        'CLS
        'LOCATE 1, 1
        'FOR i = 1 TO 20: PRINT USING "## "; i; : FOR j = 1 TO nomcols: PRINT USING " .##"; x(i, j); : NEXT: PRINT : NEXT
        'WHILE INKEY$ = "": WEND
exitline:
        Erase alfa, gam, rhs, G, Xold

    End Sub

    ''' <summary>
    ''' Evaluate the <see cref="cEcospaceDataStructures.IsFished">fishing access map</see> for the current month.
    ''' </summary>
    Sub setIsFished()

        'System.Console.WriteLine("----------------MPA Fished------------------------")

        ' For all cells
        For i As Integer = 1 To Me.m_Data.InRow
            For j As Integer = 1 To Me.m_Data.InCol

                ' For all fleets
                For ig As Integer = 1 To Me.m_Data.nFleets
                    Dim bFished As Boolean = False
                    ' Is this is a fished water cell?
                    If (Me.m_Data.Depth(i, j) > 0) And (Me.m_Data.PAreaFished(ig)(i, j) > 0 Or Me.m_Data.GearHab(ig, 0)) Then
                        '#Yes: Cell is potentialy fished 
                        ' If this cell is closed to fishing for this fleet and month by any of the MPAs, then do not allow fishing here 
                        bFished = True
                        For iMPA As Integer = 1 To Me.m_Data.MPAno
                            ' Is this MPA active in this cell?
                            '   - Me.m_Data.MPAfishery(ig, iMPA) = True if this fleet can fish in the MPA
                            '   - Me.m_Data.MPAmonth(Me.m_Data.MonthNow, iCellMPA) = True if the MPA is open for fishing (e.g., is NOT enforced) in this month
                            If (Me.m_Data.MPA(iMPA)(i, j)) And (Not Me.m_Data.MPAfishery(ig, iMPA)) And (Not Me.m_Data.MPAmonth(Me.m_Data.MonthNow, iMPA)) Then
                                ' #Yes: This MPA prohibits this fleet from fishing in this cell for this month
                                bFished = False
                                Exit For
                            End If
                        Next

                        If bFished Then
                            'If it is still fished check it against the EffortDistThreshold
                            'Include the fishing effort threshold 
                            If Me.m_Data.bUseEffortDistThreshold And Me.m_Data.Sail(ig)(i, j) >= Me.m_Data.EffortDistThreshold Then
                                bFished = False
                            End If
                            'System.Console.WriteLine("Fished fleet=" + ig.ToString + " row=" + i.ToString + " col=" + j.ToString)
                        End If

                    End If
                    Me.m_Data.IsFished(ig, i, j) = bFished
                Next ig
            Next j
        Next i

    End Sub

    Sub PredictEffortDistributionThreaded(ByVal obParam As Object)
        Dim i As Integer, j As Integer, TotAttract As Single
        Dim Valt As Single, isp As Integer
        Dim EffortCost As Single
        Dim SailCost As Single
        Dim TotE As Single
        Dim Attract(,) As Single
        Dim arguments As cThreadedCallArgs
        'Dim stpwtch As Stopwatch

        Dim TotAttractZone(Me.m_Data.nEffZones) As Single
        Dim TotEffortZone(Me.m_Data.nEffZones) As Single

        Try

            arguments = DirectCast(obParam, cThreadedCallArgs)
            'Make sure the number of fleets is in bounds
            'This could happen because of rounding error in the number of fleets per thread
            If arguments.iFirst <= Me.m_Data.nFleets Then

                'Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId
                'Console.WriteLine("Effort Distribution , ThreadID = " & thrdID.ToString & ", Start T = " & DateTime.Now.ToLongTimeString)
                'Console.WriteLine("  N Fleets = " & (arguments.iLast - arguments.iFirst + 1).ToString)
                'stpwtch = Stopwatch.StartNew

                ReDim Attract(m_Data.InRow, m_Data.InCol)

                For iFlt As Integer = arguments.iFirst To arguments.iLast
                    'check the bounds
                    If (iFlt < 1) Or (iFlt > Me.m_Data.nFleets) Then Exit For
                    'System.Console.WriteLine("  Fleet " & iFlt.ToString)

                    TotE = m_Data.TotEffort(iFlt) * m_Data.SEmult(iFlt)

                    'set the total effort by zone
                    For iZone As Integer = 0 To Me.m_Data.nEffZones
                        TotEffortZone(iZone) = TotE * Me.m_Data.PropEffortFleetZone(iFlt, iZone)
                        TotAttractZone(iZone) = 1.0E-30
                    Next iZone

                    'jb Attract() gets cleared out for each fleet
                    Array.Clear(Attract, 0, Attract.Length)
                    TotAttract = 0.0000000001

                    'Introduce a factor which balances fixed and sailingcost: (up to 02Jan02 the next if then was in the loop over spatial cells below, no need for this)
                    If m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail) = 0 Then
                        EffortCost = 0
                        SailCost = 1
                    Else
                        EffortCost = m_EPdata.cost(iFlt, eCostIndex.CUPE) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
                        SailCost = m_EPdata.cost(iFlt, eCostIndex.Sail) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
                    End If

                    For i = 1 To m_Data.InRow
                        For j = 1 To m_Data.InCol
                            'isFished is evaluated each timestep
                            If Me.m_Data.IsFished(iFlt, i, j) Then
                                'Water and (Not closed by MPA) and (Fished by this gear)
                                'mpamonth(Month, MPAType) is false if closed, True if open.
                                Valt = 0
                                For isp = 1 To m_Data.NGroups
                                    Valt = Valt + m_EPdata.Market(iFlt, isp) * m_Data.Bcell(i, j, isp) * m_SimData.relQ(iFlt, isp) * m_SimData.PropLandedTime(iFlt, isp)
                                Next

                                'VC Sail() above: to avoid dividing with zero
                                Valt = (Valt ^ m_Data.EffPower(iFlt)) / (EffortCost + SailCost * m_Data.Sail(iFlt)(i, j) / m_Data.SailScale(iFlt))
                                'jb 9-May-2014 change re Carls email
                                'What this represents is attractiveness equal to exp(effpower*(I/C-1)), where I/C is profitability, -1 is subtracted to scale to exp(0)=1 for I/C=1.  
                                'Effpower represents (as before) an effort concentration factor, low values implying less variation in valt with changes in I/C.
                                'If you run this, should be nearly 2x faster than old code, and will concentrate effort a bit more in best fishing areas.  
                                'I can also modify it further to force the attract’s to result in any observed effort map that we might enter, 
                                'essentially by replacing the cost C with a simpler empirical cost scaler.
                                ' Valt = Exp((Valt / (EffortCost + SailCost * m_Data.Sail(iFlt, i, j) / m_Data.SailScale(iFlt)) - 1.0) * m_Data.EffPower(iFlt))


                                Attract(i, j) = Valt * Me.m_Data.PAreaFished(iFlt)(i, j) 'may want to modify this by dividing by a site cost factor for cell i,j
                                'sum of attractivness by zone
                                TotAttractZone(Me.m_Data.EffZones(i, j)) += Attract(i, j)

                            End If 'Me.m_Data.IsFished(iFlt, i, j)
                        Next j
                    Next i

                    Dim sumEff As Single = 0, nEf As Integer = 0
                    For i = 1 To m_Data.InRow
                        For j = 1 To m_Data.InCol
                            'VC19Aug98: Fishing in water, not in MPA unless the MPA is fished, and only if this gear operate in this habitat or in all habitats
                            If Me.m_Data.IsFished(iFlt, i, j) Then

                                'Effort distribution scaled by Effort Zone
                                '3-Feb-2014 Villy changed this to use Me.m_Data.EffZones(i, j) which is the index of the zone not the effort in the zone???
                                'm_Data.EffortSpace(iFlt, i, j) = m_SimData.FishRateGear(iFlt, arguments.iCumMonth) * Me.m_Data.EffZones(i, j) * Attract(i, j) / TotAttractZone(Me.m_Data.EffZones(i, j))
                                m_Data.EffortSpace(iFlt, i, j) = m_SimData.FishRateGear(iFlt, arguments.iCumMonth) * TotEffortZone(Me.m_Data.EffZones(i, j)) * Attract(i, j) / TotAttractZone(Me.m_Data.EffZones(i, j))

                                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                'jb 19-July-2012 moved summing of fishing mortality out of the distribution threads
                                'this stops the threading bug caused when different threads try to sum F at the same time resulting in different F (Ftot(,,,))
                                '        For isp = 1 To m_Data.NGroups
                                '            'Fishing Mort
                                '            m_Data.Ftot(isp, i, j) = m_Data.Ftot(isp, i, j) + m_Data.EffortSpace(iFlt, i, j) * m_SimData.relQ(iFlt, isp) / Me.m_Data.PAreaFished(i, j, iFlt)
                                '        Next isp
                                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                                ''For debugging
                                'sumEff += m_Data.EffortSpace(iFlt, i, j)
                                'nEf += 1
                            End If
                        Next j
                    Next i

                    ''xxxxxxxxxxxxxxxxxxxxxxxxxx
                    ''For debugging
                    'If iFlt = 3 Then
                    '    sumEff /= nEf
                    '    System.Console.WriteLine("Effort Distribution Fleet = " + iFlt.ToString _
                    '                                + ", sim effort = " + m_SimData.FishRateGear(iFlt, arguments.iCumMonth).ToString _
                    '                                + ", avg effort = " + sumEff.ToString _
                    '                                + ", error = " + (sumEff / m_SimData.FishRateGear(iFlt, arguments.iCumMonth)).ToString)
                    'End If
                    ''xxxxxxxxxxxxxxxxxxxxxxxxxxxx

                Next iFlt

            Else ' If arguments.iFirst <= Me.m_Data.nFleets Then
                'First Fleet Index > Number of Fleets
                'We still need to Decrement the Interlock counter
                System.Console.WriteLine("Effort Dist No fleets to process = " & cEcoSpace.m_ThreadIncrementCount.ToString)
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        If Interlocked.Decrement(cEcoSpace.m_ThreadIncrementCount) = 0 Then
            arguments.WaitHandle.Set()
        End If

        'System.Console.WriteLine("Effort Dist Increment Lock = " & cEcoSpace.m_ThreadIncrementCount.ToString)

    End Sub

    '    Sub InitSpaceCostThreaded(ByVal obParam As Object)
    '        'this needs to be called before the first call 
    '        ' to PredictEffortDistributionThreaded, but after initial estimates of 
    '        ' Bcell(...) have been calculated using habitat info, etc.
    '        'there needs to have been an array called SpaceCostCell(i,j,iflt) dimensioned
    '        'with number of map rows, number of map columns(m_Data.InRow, m_Data.InCol), and 
    '        ' number of fleets '(Me.m_Data.nFleets) that needs to be available within
    '        'PredictEffortDistributionThreaded whenever it is called
    '        'note many lines from PredictEffortDistributionThreaded have just been commented out for
    '        'this routine
    '        Dim i As Integer, j As Integer, TotAttract As Single
    '        Dim Valt As Single, isp As Integer
    '        Dim EffortCost As Single
    '        Dim SailCost As Single
    '        Dim TotE As Single
    '        '    Dim Attract(,) As Single
    '        Dim arguments As cThreadedCallArgs

    '        Dim stpwtch As Stopwatch

    '        Try

    '            arguments = DirectCast(obParam, cThreadedCallArgs)
    '            'Make sure the number of fleets is in bounds
    '            'This could happen because of rounding error in the number of fleets per thread
    '            If arguments.iFirst <= Me.m_Data.nFleets Then

    '                'Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId

    '                'Console.WriteLine("Effort Distribution , ThreadID = " & thrdID.ToString & ", Start T = " & DateTime.Now.ToLongTimeString)
    '                'Console.WriteLine("  N Fleets = " & (arguments.iLast - arguments.iFirst + 1).ToString)
    '                stpwtch = Stopwatch.StartNew

    '                '           ReDim Attract(m_Data.InRow, m_Data.InCol)

    '                For iFlt As Integer = arguments.iFirst To arguments.iLast
    '                    'check the bounds
    '                    If (iFlt < 1) Or (iFlt > Me.m_Data.nFleets) Then Exit For
    '                    'System.Console.WriteLine("  Fleet " & iFlt.ToString)
    '                    TotE = TotEffort(iFlt) * m_Data.SEmult(iFlt)

    '                    'jb Attract() gets cleared out for each fleet
    '                    '              Array.Clear(Attract, 0, Attract.Length)
    '                    '              TotAttract = 0.0000000001

    '                    'Introduce a factor which balances fixed and sailingcost: (up to 02Jan02 the next if then was in the loop over spatial cells below, no need for this)
    '                    If m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail) = 0 Then
    '                        EffortCost = 0
    '                        SailCost = 1
    '                    Else
    '                        EffortCost = m_EPdata.cost(iFlt, eCostIndex.CUPE) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
    '                        SailCost = m_EPdata.cost(iFlt, eCostIndex.Sail) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
    '                    End If

    '                    '
    '                    For i = 1 To m_Data.InRow
    '                        For j = 1 To m_Data.InCol
    '                            'Moved to InitSpatialEquilibrium
    '                            If Me.m_Data.IsFished(iFlt, i, j) Then
    '                                'Water and (Not closed by MPA) and (Fished by this gear)
    '                                'mpamonth(Month, MPAType) is false if closed, True if open.
    '                                Valt = 0
    '                                For isp = 1 To m_Data.NGroups
    '                                    Valt = Valt + m_EPdata.Market(iFlt, isp) * m_Data.Bcell(i, j, isp) * m_SimData.relQ(iFlt, isp)
    '                                Next

    '                                'jb Move to InitSpatialEquilibrium()
    '                                ' If m_Data.Sail(iFlt, i, j) = 0 Then m_Data.Sail(iFlt, i, j) = 0.000001
    '                                ' here is the SpaceCostCell calculation
    '                                SpaceCostCell(i, j, iFlt) = (EffortCost + SailCost * m_Data.Sail(iFlt, i, j) / m_Data.SailScale(iFlt))

    '                                'IN PredicteEffortDistributionThreaded, the Valt calculation in line below should be 
    '                                'replaced by the following line:
    'Valt=exp(-m_Data.EffPower(iFlt)*(spacecostcell(i,j,iflt)/(Valt+spacecostcell(i,j,iflt)/300)-1.))
    '                                'in this calculation, the term spacecostcell(i,j,iflt)/300 is only there to prevent the
    '                                'exponential from being less than -300 in case Valt is zero;
    '                                'VC Sail() above: to avoid dividing with zero

    '                                '  this line should be replaced:           Valt = (Valt ^ m_Data.EffPower(iFlt)) / ' '(EffortCost + SailCost * m_Data.Sail(iFlt, i, j) / m_Data.SailScale(iFlt))
    '                                '                               Attract(i, j) = Valt * Me.m_Data.PAreaFished(i, j, iFlt) 'may want to modify this by dividing by a site cost factor for cell i,j
    '                                '                               TotAttract = TotAttract + Valt * 'Me.m_Data.PAreaFished(i, j, iFlt)
    '                            End If
    '                        Next
    '                    Next

    '                    For i = 1 To m_Data.InRow
    '                        For j = 1 To m_Data.InCol
    '                            'VC19Aug98: Fishing in water, not in MPA unless the MPA is fished, and only if this gear operate in this habitat or in all habitats
    '                            '                           If Me.m_Data.IsFished(iFlt, i, j) Then

    '                            'VC/080499 Above changed per CJWs advice to reflect effort change over time in Ecospace
    '                            '                               m_Data.EffortSpace(iFlt, i, j) = 'm_SimData.FishRateGear(iFlt, arguments.iCumMonth) * TotE * Attract(i, j) / 'TotAttract

    '                            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    '                            'jb 19-July-2012 moved summing of fishing mortality out of the distribution threads
    '                            'this stops the threading bug caused when different threads try to sum F at the same time resulting in different F (Ftot(,,,))
    '                            '        For isp = 1 To m_Data.NGroups
    '                            '            'Fishing Mort
    '                            '            m_Data.Ftot(isp, i, j) = m_Data.Ftot(isp, i, j) + m_Data.EffortSpace(iFlt, i, j) * m_SimData.relQ(iFlt, isp) / Me.m_Data.PAreaFished(i, j, iFlt)
    '                            '        Next isp
    '                            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    '                            End If
    '                        Next j
    '                    Next i
    '                Next iFlt

    '            Else ' If arguments.iFirst <= Me.m_Data.nFleets Then
    '            'First Fleet Index > Number of Fleets
    '            'We still need to Decrement the Interlock counter
    '            System.Console.WriteLine("Effort Dist No fleets to process = " & cEcoSpace.m_ThreadIncrementCount.ToString)
    '            End If

    '        Catch ex As Exception
    '            Debug.Assert(False, ex.Message)
    '        End Try

    '        If Interlocked.Decrement(cEcoSpace.m_ThreadIncrementCount) = 0 Then
    '            arguments.WaitHandle.Set()
    '        End If

    '        'System.Console.WriteLine("Effort Dist Increment Lock = " & cEcoSpace.m_ThreadIncrementCount.ToString)

    '    End Sub

    ''' <summary>
    ''' Threaded and Load Shared Version
    ''' This routine predicts spatial effort and fishing mortality rate
    ''' distribution by gear type; called at each iteration
    ''' step in finding biomass spatial equilibrium
    ''' model below is a gravity attraction model, distributing
    ''' total efforts TotEffort(gear) over all cells where each gear can fish
    ''' in proportion to relative profitability (catch rate x price sum) for that cell for the gear
    ''' </summary>
    ''' <remarks>
    ''' The load sharing works by spinning in a loop requesting the next available fleet to process. 
    ''' Once all the fleets have been processed the thread will exit.
    ''' </remarks>
    Sub PredictEffortDistributionThreadedLoadShared(ByVal obParam As Object)
        Dim iRow As Integer, iCol As Integer, TotAttract As Single
        Dim Valt As Single, isp As Integer
        Dim EffortCost As Single
        Dim SailCost As Single
        Dim TotE As Single
        Dim Attract(,) As Single
        Dim arguments As cThreadedCallArgs
        Dim ncells As Integer
        Dim iFlt As Integer
        'Dim stpwtch As Stopwatch

        Dim TotAttractZone(Me.m_Data.nEffZones) As Single
        Dim TotEffortZone(Me.m_Data.nEffZones) As Single

        Try

            arguments = DirectCast(obParam, cThreadedCallArgs)

            'Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId
            'Console.WriteLine("Effort Distribution , ThreadID = " & thrdID.ToString & ", Start T = " & DateTime.Now.ToLongTimeString)
            'Console.WriteLine("  N Fleets = " & (arguments.iLast - arguments.iFirst + 1).ToString)
            'stpwtch = Stopwatch.StartNew

            ReDim Attract(m_Data.InRow, m_Data.InCol)

            Do While cEcoSpace.getNextFleet(iFlt)
                'System.Console.WriteLine("Effort Distribution Fleet " + iFlt.ToString)

                'check the bounds
                Debug.Assert(iFlt > 0 And iFlt <= Me.m_Data.nFleets, "cEcoSpace.getNextFleet(fleetIndex) Returned an invalid fleet index.")
                If (iFlt < 1) Or (iFlt > Me.m_Data.nFleets) Then Exit Do

                TotE = m_Data.TotEffort(iFlt) * m_Data.SEmult(iFlt)
                'set the total effort by zone
                For iZone As Integer = 0 To Me.m_Data.nEffZones
                    TotEffortZone(iZone) = TotE * Me.m_Data.PropEffortFleetZone(iFlt, iZone)
                    TotAttractZone(iZone) = 1.0E-30
                Next iZone

                'jb Attract() gets cleared out for each fleet
                Array.Clear(Attract, 0, Attract.Length)
                TotAttract = 0.0000000001

                'Introduce a factor which balances fixed and sailingcost: (up to 02Jan02 the next if then was in the loop over spatial cells below, no need for this)
                If m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail) = 0 Then
                    EffortCost = 0
                    SailCost = 1
                Else
                    EffortCost = m_EPdata.cost(iFlt, eCostIndex.CUPE) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
                    SailCost = m_EPdata.cost(iFlt, eCostIndex.Sail) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
                End If

                'Now loop over the fished cells and compute the attraction of each fished cell
                For Each rowcol As cRowCol In Me.m_Data.FleetSailCells(iFlt)
                    ncells += 1
                    iRow = rowcol.Row
                    iCol = rowcol.Col

                    'Debug.Assert(iFlt <> 14 And Not Me.m_Data.EffZones(iRow, iCol) = 0)

                    'IsFished() is set every timestep to account for monthly MPA Closures
                    If Me.m_Data.IsFished(iFlt, iRow, iCol) Then
                        'Water and (Not closed by MPA) and (Fished by this gear)
                        'mpamonth(Month, MPAType) is false if closed, True if open.
                        Valt = 0
                        For isp = 1 To m_Data.NGroups
                            'discards will have a value of zero so they will not be included in the total value
                            Valt = Valt + m_EPdata.Market(iFlt, isp) * m_Data.Bcell(iRow, iCol, isp) * m_SimData.relQ(iFlt, isp) * m_SimData.PropLandedTime(iFlt, isp)
                        Next
                        Valt = (Valt ^ m_Data.EffPower(iFlt)) / (EffortCost + SailCost * m_Data.Sail(iFlt)(iRow, iCol) / m_Data.SailScale(iFlt))

                        'What this represents is attractiveness equal to exp(effpower*(I/C-1)), where I/C is profitability, -1 is subtracted to scale to exp(0)=1 for I/C=1.  
                        'Effpower represents (as before) an effort concentration factor, low values implying less variation in valt with changes in I/C.
                        'If you run this, should be nearly 2x faster than old code, and will concentrate effort a bit more in best fishing areas.  
                        'I can also modify it further to force the attract’s to result in any observed effort map that we might enter, essentially by replacing the cost C with a simpler empirical cost scaler.
                        'Valt = Exp((Valt / (EffortCost + SailCost * m_Data.Sail(iFlt, iRow, iCol) / m_Data.SailScale(iFlt)) - 1.0) * m_Data.EffPower(iFlt))
                        Attract(iRow, iCol) = Valt * Me.m_Data.PAreaFished(iFlt)(iRow, iCol)  'may want to modify this by dividing by a site cost factor for cell i,j
                        'TotAttract += Attract(iRow, iCol)
                        'Total attractiveness by zone
                        TotAttractZone(Me.m_Data.EffZones(iRow, iCol)) += Attract(iRow, iCol)

                    End If
                Next

                'Again loop over the cells and computed the weighted effort base on the Ecosim Timeseries effort and the weighted attractiveness of the cell
                For Each rowcol As cRowCol In Me.m_Data.FleetSailCells(iFlt)
                    ncells += 1
                    iRow = rowcol.Row
                    iCol = rowcol.Col

                    'IsFished() is set every timestep to account for monthly MPA Closures
                    If Me.m_Data.IsFished(iFlt, iRow, iCol) Then

                        'Effort distribution scaled by Effort Zone
                        m_Data.EffortSpace(iFlt, iRow, iCol) = m_SimData.FishRateGear(iFlt, arguments.iCumMonth) * TotEffortZone(Me.m_Data.EffZones(iRow, iCol)) * Attract(iRow, iCol) / TotAttractZone(Me.m_Data.EffZones(iRow, iCol))
                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        'jb 19-July-2012 moved summing of fishing mortality out of the distribution threads
                        'this stops the threading bug caused when different threads try to sum F at the same time resulting in different F (Ftot(,,,))
                        '        For isp = 1 To m_Data.NGroups
                        '            'Fishing Mort
                        '            m_Data.Ftot(isp, i, j) = m_Data.Ftot(isp, i, j) + m_Data.EffortSpace(iFlt, i, j) * m_SimData.relQ(iFlt, isp) / Me.m_Data.PAreaFished(i, j, iFlt)
                        '        Next isp
                        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    End If
                Next rowcol

            Loop

            'stpwtch.Stop()
            'Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId
            'Console.WriteLine("Effort Distribution Loadshared , ThreadID = " + thrdID.ToString & ",  RunTime(sec) = " + stpwtch.Elapsed.TotalSeconds.ToString + ", N Cells = " + ncells.ToString)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        If Interlocked.Decrement(cEcoSpace.m_ThreadIncrementCount) = 0 Then
            arguments.WaitHandle.Set()
        End If

        'System.Console.WriteLine("Effort Dist Increment Lock = " & cEcoSpace.m_ThreadIncrementCount.ToString)

    End Sub


    ''' <summary>
    ''' Run the PredictEffortDistribution on multiple threads
    ''' </summary>
    ''' <param name="iMonth"></param>
    ''' <param name="iCumMonth"></param>
    ''' <remarks></remarks>
    Private Sub runEffortDistributionNoLoadShare(ByVal iMonth As Integer, ByVal iCumMonth As Integer)
        Dim stpwTotRunTime As Stopwatch
        ' Dim stpwF As Stopwatch
        Dim nThrds As Integer
        Dim nFltsPerThread As Integer
        Dim iFirstFleet As Integer = 1
        Dim iLastFleet As Integer
        Dim nCompFleets As Integer

        Debug.Assert(Me.m_Data.bUseEffortDistThreshold = False, Me.ToString + ".runEffortDistributionNoLoadShare() Called with bUseEffortDistThreshold = True.")

        'GC.Collect()

        Array.Clear(m_Data.Ftot, 0, m_Data.Ftot.Length)
        Array.Clear(m_Data.EffortSpace, 0, m_Data.EffortSpace.Length)

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'First run the Effort Distrubution threads by Fleet
        'This Computes the Effort by Fleet into EffortSpace(iFlt, iRow, iCol)
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        nThrds = Me.m_Data.nEffortDistThreads
        'Constrain the number of fleet threads to the number of fleets
        If nThrds > Me.m_Data.nFleets Then nThrds = Me.m_Data.nFleets
        'Set the thread increment counter
        cEcoSpace.m_ThreadIncrementCount = nThrds

        Dim waitOb As WaitHandle = New AutoResetEvent(False)

        stpwTotRunTime = Stopwatch.StartNew
        For ithrd As Integer = 1 To nThrds

            nFltsPerThread = Me.computeThreadLoad(Me.m_Data.nFleets, nCompFleets, nThrds, ithrd)
            nCompFleets += nFltsPerThread

            iLastFleet = iFirstFleet + nFltsPerThread - 1
            If iLastFleet > m_Data.nFleets Then iLastFleet = m_Data.nFleets

            'Distribute fishing effort across the map for the fleet indexes iFirstFleet to ilastfleet
            'ThreadPool.QueueUserWorkItem(AddressOf Me.PredictEffortDistributionThreaded, New cThreadedCallArgs(waitOb, iFirstFleet, iLastFleet, iMonth, iCumMonth))
            ThreadPool.QueueUserWorkItem(AddressOf Me.PredictEffortDistributionThreaded, New cThreadedCallArgs(waitOb, iFirstFleet, iLastFleet, iMonth, iCumMonth))
            iFirstFleet += nFltsPerThread
        Next ithrd

        Debug.Assert(nCompFleets = m_Data.nFleets)

        'waitOb will AutoReset so the next call to WaitOne will block until Set() is called
        If Not waitOb.WaitOne() Then
            System.Console.WriteLine("EffortDistribution PredictEffortDistributionThreaded() Timed Out WTF!")
            'Ok something has to happen here
            'Maybe pitch an error
            cLog.Write(Me.ToString & ".runPredictEffortDistributionThreads() PredictEffortDistributionThreaded timed out.")
        End If

        waitOb.Dispose()
        waitOb = Nothing

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Next update the Fishing Mortality base on the effort computed above
        'Popultes Ftot(igrp, irow, jcol) base on distributed effort in  EffortSpace(iFlt, iRow, iCol)
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Figure out the number of threads
        Dim nMortThrds As Integer = Me.m_Data.nEffortDistThreads
        If nMortThrds > m_Data.iTotalWaterCells Then nMortThrds = m_Data.iTotalWaterCells
        cEcoSpace.m_ThreadIncrementCount = nMortThrds

        waitOb = New AutoResetEvent(False)
        Dim iFirstCell As Integer = 1
        Dim iLastcell As Integer

        'Number of cells to compute for the current thread
        'Computed on the fly in the loop
        Dim nCells As Integer

        'Total number of cells that have been computed
        Dim nCellCompleted As Integer

        For iThrd As Integer = 1 To nMortThrds
            'Compute the work load for each thread on the fly
            'this prevents any rounding weirdness that could cause m_ThreadIncrementCount to not hit Zero
            'Causing a deadlock on WaitOb.WaitOne()
            nCells = Me.computeThreadLoad(m_Data.iTotalWaterCells, nCellCompleted, nMortThrds, iThrd)
            nCellCompleted += nCells

            iLastcell = iFirstCell + nCells - 1
            If iLastcell > m_Data.iTotalWaterCells Then iLastcell = m_Data.iTotalWaterCells

            ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf Me.setFishMortFromEffort), New cThreadedCallArgs(waitOb, iFirstCell, iLastcell))

            iFirstCell += nCells

        Next

        Debug.Assert(nCellCompleted = m_Data.iTotalWaterCells)

        'Wait for the setFishMortFromEffort() threads to complete
        If Not waitOb.WaitOne() Then
            System.Console.WriteLine("EffortDistribution setFishMortFromEffort() Timed Out WTF!")
            'Ok something has to happen here
            'Maybe pitch an error
            cLog.Write(Me.ToString & ".runPredictEffortDistributionThreads() setFishMortFromEffort timed out.")
        End If
        waitOb.Dispose()
        waitOb = Nothing

        stpwTotRunTime.Stop()
        'System.Console.WriteLine("EffortDistribution wall time (sec), " & stpwTotRunTime.Elapsed.TotalSeconds.ToString)

        'GC.Collect()

    End Sub

    ''' <summary>
    ''' A load sharing version of Threaded Effort Distrubtion threads
    ''' </summary>
    ''' <param name="iMonth"></param>
    ''' <param name="iCumMonth"></param>
    ''' <remarks>
    ''' In this version the effort distribution threads will request the next available fleet for effort distribution. 
    ''' Once all the fleets have been completed all the threads will exit.  
    ''' </remarks>
    Private Sub runEffortDistributionLoadShared(ByVal iMonth As Integer, ByVal iCumMonth As Integer)
        Dim stpwTotRunTime As Stopwatch
        ' Dim stpwF As Stopwatch
        Dim nThrds As Integer
        Dim distET As Double

        Debug.Assert(Me.m_Data.bUseEffortDistThreshold, Me.ToString + ".runEffortDistributionNoLoadShare() Called with bUseEffortDistThreshold = True.")

        'GC.Collect()
        Array.Clear(m_Data.Ftot, 0, m_Data.Ftot.Length)
        Array.Clear(m_Data.EffortSpace, 0, m_Data.EffortSpace.Length)


        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'First run the Effort Distrubution threads by Fleet
        'This Computes the Effort by Fleet into EffortSpace(iFlt, iRow, iCol)
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        nThrds = Me.m_Data.nEffortDistThreads
        'Constrain the number of fleet threads to the number of fleets
        If nThrds > Me.m_Data.nFleets Then nThrds = Me.m_Data.nFleets
        'Set the thread increment counter
        cEcoSpace.m_ThreadIncrementCount = nThrds

        'Set the shared fleet counter this is used by GetNextFleet(ifleet) to get the next fleet index to compute
        cEcoSpace.FleetCounter = 0

        Dim waitOb As WaitHandle = New AutoResetEvent(False)

        stpwTotRunTime = Stopwatch.StartNew
        'Fire up all the effort distribution threads
        'Each thread will spin in a loop reguesting fleets
        'Once all the fleets have been computed waitOb.Set() will be called releasing the waithandle
        For ithrd As Integer = 1 To nThrds
            ThreadPool.QueueUserWorkItem(AddressOf Me.PredictEffortDistributionThreadedLoadShared, New cThreadedCallArgs(waitOb, cCore.NULL_VALUE, cCore.NULL_VALUE, iMonth, iCumMonth))
        Next ithrd

        'The Effort Distribtion threads will count down cEcoSpace.m_ThreadIncrementCount
        'Once one of the threads reaches zero it will call WaitOb.Set() releasing the wait handle
        If Not waitOb.WaitOne() Then
            System.Console.WriteLine("EffortDistribution PredictEffortDistributionThreaded() Timed Out WTF!")
            'Ok something has to happen here
            'Maybe pitch an error
            cLog.Write(Me.ToString & ".runPredictEffortDistributionThreads() PredictEffortDistributionThreaded timed out.")
        End If

        distET = stpwTotRunTime.Elapsed.TotalSeconds

        waitOb.Dispose()
        waitOb = Nothing

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Next update the Fishing Mortality base on the effort computed above
        'Populates Ftot(igrp, irow, jcol) base on distributed effort in  EffortSpace(iFlt, iRow, iCol)
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Figure out the number of threads
        Dim nMortThrds As Integer = Me.m_Data.nEffortDistThreads
        If nMortThrds > m_Data.iTotalWaterCells Then nMortThrds = m_Data.iTotalWaterCells
        cEcoSpace.m_ThreadIncrementCount = nMortThrds

        waitOb = New AutoResetEvent(False)
        Dim iFirstCell As Integer = 1
        Dim iLastcell As Integer

        'Number of cells to compute for the current thread
        'Computed on the fly in the loop
        Dim nCells As Integer

        'Total number of cells that have been computed
        Dim nCellCompleted As Integer

        For iThrd As Integer = 1 To nMortThrds
            'Compute the work load for each thread on the fly
            'this prevents any rounding weirdness that could cause m_ThreadIncrementCount to not hit Zero
            'Causing a deadlock on WaitOb.WaitOne()
            nCells = Me.computeThreadLoad(m_Data.iTotalWaterCells, nCellCompleted, nMortThrds, iThrd)
            nCellCompleted += nCells

            iLastcell = iFirstCell + nCells - 1
            If iLastcell > m_Data.iTotalWaterCells Then iLastcell = m_Data.iTotalWaterCells

            ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf Me.setFishMortFromEffort), New cThreadedCallArgs(waitOb, iFirstCell, iLastcell))

            iFirstCell += nCells

        Next

        Debug.Assert(nCellCompleted = m_Data.iTotalWaterCells)

        ' System.Console.WriteLine("EffortDistribution Waiting for setFishMortFromEffort()")
        If Not waitOb.WaitOne() Then
            System.Console.WriteLine("EffortDistribution setFishMortFromEffort() Timed Out WTF!")
            'Ok something has to happen here
            'Maybe pitch an error
            cLog.Write(Me.ToString & ".runPredictEffortDistributionThreads() setFishMortFromEffort timed out.")
        End If
        waitOb.Dispose()
        waitOb = Nothing

        stpwTotRunTime.Stop()
        ' System.Console.WriteLine("EffortDistribution Total run time (sec), " & stpwTotRunTime.Elapsed.TotalSeconds.ToString)

        'GC.Collect()

    End Sub


    Private Shared Function getNextFleet(ByRef FleetIndex As Integer) As Boolean
        SyncLock cEcoSpace.FleetSyncLock
            'cEcoSpace.FleetCounter  Must be set to zero before this is used
            cEcoSpace.FleetCounter += 1
            If cEcoSpace.FleetCounter <= cEcoSpace.nFleets Then
                FleetIndex = cEcoSpace.FleetCounter
                Return True
            End If
            FleetIndex = cCore.NULL_VALUE
            'No more fleets left to process
            Return False
        End SyncLock
    End Function

    'Private Function getPredictEffortFunction(ByVal bByCell As Boolean) As WaitCallback
    '    If bByCell Then
    '        Return New WaitCallback(AddressOf Me.PredictEffortDistribution_CellList)
    '        'Return New WaitCallback(AddressOf Me.PredictEffortDistribution_CellList_LoadShared)
    '    Else
    '        Return New WaitCallback(AddressOf Me.PredictEffortDistributionThreaded)
    '    End If
    'End Function


    Private Function computeThreadLoad(TotalWork As Integer, WorkCompleted As Integer, TotalThreads As Integer, CurrentThread As Integer) As Integer
        Return CInt(TotalWork - WorkCompleted) / (TotalThreads - (CurrentThread - 1))
    End Function

    Private Sub setFishMortFromEffort(ByVal obParam As Object)
        Dim irow As Integer
        Dim jcol As Integer
        Dim args As cThreadedCallArgs
        Try

            Debug.Assert(TypeOf obParam Is cThreadedCallArgs, "Ecospace.setFishMortFromEffort() parameter is not the correct type.")
            args = DirectCast(obParam, cThreadedCallArgs)

            For icell As Integer = args.iFirst To args.iLast
                irow = m_Data.iWaterCellIndex(icell)
                jcol = m_Data.jWaterCellIndex(icell)

                For iflt As Integer = 1 To Me.m_Data.nFleets

                    'VC19Aug98: Fishing in water, not in MPA unless the MPA is fished, and only if this gear operate in this habitat or in all habitats
                    If Me.m_Data.IsFished(iflt, irow, jcol) Then
                        For igrp As Integer = 1 To m_Data.NGroups
                            'Fishing Mort Rate in a cell by group
                            Dim f As Single = m_SimData.relQ(iflt, igrp) * (m_SimData.PropLandedTime(iflt, igrp) + m_SimData.Propdiscardtime(iflt, igrp))
                            m_Data.Ftot(igrp, irow, jcol) += m_Data.EffortSpace(iflt, irow, jcol) * f / Me.m_Data.PAreaFished(iflt)(irow, jcol)

                        Next igrp
                    End If ' m_Data.Depth(i, j) > 0

                Next iflt
            Next icell

        Catch ex As Exception
            cLog.Write(ex, " Error computing Fishing Mortality")
            System.Console.WriteLine("Ecospace.setFishMortFromEffort() Exception: " & ex.Message)
        End Try

        If Interlocked.Decrement(cEcoSpace.m_ThreadIncrementCount) = 0 Then
            args.WaitHandle.Set()
        End If

    End Sub



    ''' <summary>
    ''' This is a modified version of PredictEffortDistribution, to be called only once at around simulation
    ''' month 2 or 3; it resets totaleffort(gear) so as to avoid overfishing (relative to ecopath base) on concentrated species
    ''' modifications to PredictEffortDistribution are ahown as '***
    ''' </summary>
    ''' <remarks></remarks>
    Sub AdjustTotalEffort()

        Dim ig As Integer, i As Integer, j As Integer, TotAttract As Single
        Dim Valt As Single, isp As Integer
        Dim Effort() As Single
        Dim EffortCost As Single
        Dim SailCost As Single
        Dim CatGear As Single, CatLoc(,) As Single, WtCat As Single
        Dim Attract(,) As Single

        'Use Ecopath effort
        If Not Me.m_Data.PredictEffort Then Return
        'Don't adjust total effort when using EffortDistThreshold
        'EffortDistThreshold restricts the fishing to small number of cells
        If Me.m_Data.bUseEffortDistThreshold Then Return

        ReDim Effort(m_Data.nFleets)

        For ig = 1 To m_Data.nFleets
            'jb Attract() gets cleared out for each fleet
            ReDim Attract(m_Data.InRow, m_Data.InCol)
            TotAttract = 0.0000000001

            'Introduce a factor which balances fixed and sailingcost: (up to 02Jan02 the next if then was in the loop over spatial cells below, no need for this)
            If m_EPdata.cost(ig, eCostIndex.CUPE) + m_EPdata.cost(ig, eCostIndex.Sail) = 0 Then
                EffortCost = 0
                SailCost = 1
            Else
                EffortCost = m_EPdata.cost(ig, eCostIndex.CUPE) / (m_EPdata.cost(ig, eCostIndex.Fixed) + m_EPdata.cost(ig, eCostIndex.CUPE) + m_EPdata.cost(ig, eCostIndex.Sail))
                SailCost = m_EPdata.cost(ig, eCostIndex.Sail) / (m_EPdata.cost(ig, eCostIndex.Fixed) + m_EPdata.cost(ig, eCostIndex.CUPE) + m_EPdata.cost(ig, eCostIndex.Sail))
            End If

            CatGear = 0 '*****ecopath base total catch for this gear
            For isp = 1 To m_Data.NGroups  '***
                CatGear = CatGear + m_EPdata.Landing(ig, isp) + m_EPdata.Discard(ig, isp) '****
            Next  '***

            ReDim CatLoc(m_Data.InRow, m_Data.InCol) '****

            For i = 1 To m_Data.InRow
                For j = 1 To m_Data.InCol
                    If m_Data.Depth(i, j) > 0 And (m_Data.GearHab(ig, 0) Or (Me.m_Data.PAreaFished(ig)(i, j) > 0)) Then
                        'This cell is water and it is fished by this gear
                        Valt = 0
                        CatLoc(i, j) = 0
                        For isp = 1 To m_Data.NGroups
                            'Catch
                            CatLoc(i, j) = CatLoc(i, j) + m_Data.Bcell(i, j, isp) * m_SimData.relQ(ig, isp) '****
                            'Value of catch
                            Valt = Valt + m_EPdata.Market(ig, isp) * m_Data.Bcell(i, j, isp) * m_SimData.relQ(ig, isp)
                        Next

                        If m_Data.Sail(ig)(i, j) = 0 Then m_Data.Sail(ig)(i, j) = 0.000001
                        'VC Sail() above: to avoid dividing with zero
                        Valt = (Valt ^ m_Data.EffPower(ig)) / (EffortCost + SailCost * m_Data.Sail(ig)(i, j) / m_Data.SailScale(ig))
                        Attract(i, j) = Valt
                        TotAttract = TotAttract + Valt
                    End If
                Next j
            Next i

            WtCat = 0.0000000001 '****
            For i = 1 To m_Data.InRow
                For j = 1 To m_Data.InCol
                    If m_Data.Depth(i, j) > 0 And (m_Data.GearHab(ig, 0) Or (Me.m_Data.PAreaFished(ig)(i, j) > 0)) Then
                        'This cell is water and it is fished by this gear
                        WtCat = WtCat + Attract(i, j) / TotAttract * CatLoc(i, j) '***
                    End If
                Next j
            Next i

            '*** finally reset total effort using number of water cells, Ecopath base catch, and WtCat summed catch/effort x attraction weight
            '***note ThabArea below is total number of cells with depth>0 (water cells)
            m_Data.TotEffort(ig) = m_Data.ThabArea * CatGear / WtCat  '***

        Next ig

        Me.bEffortAdjusted = True

    End Sub


    Private Sub setForcedDiscards(ByVal iModelTimeStep As Integer, iYear As Integer)
        Dim bForced As Boolean = False
        Dim bFChanged As Boolean = False
        Dim totCatch As Single

        If Not Me.m_Data.UseEcosimDiscardForcing Then
            Return
        End If

        Dim iForcedTime As Integer = Me.m_refdata.toForcingTimeStep(iModelTimeStep, iYear)

        For igrp As Integer = 1 To Me.m_Data.NGroups
            For iflt As Integer = 1 To Me.m_Data.nFleets

                If Me.m_refdata.PoolForceDiscardMort(iflt, igrp, iForcedTime) >= 0.0 Then
                    'Discard Mortality has changed
                    'Save the discard mortality rate for this timestep
                    Me.m_SimData.PropDiscardMortTime(iflt, igrp) = Me.m_refdata.PoolForceDiscardMort(iflt, igrp, iForcedTime)
                    'Propdiscardtime() does NOT include discards that survived
                    Me.m_SimData.Propdiscardtime(iflt, igrp) = (1 - Me.m_SimData.PropLandedTime(iflt, igrp)) * Me.m_SimData.PropDiscardMortTime(iflt, igrp)

                    bFChanged = True
                    bForced = True
                End If

                If Me.m_refdata.PoolForceDiscardProp(iflt, igrp, iForcedTime) >= 0.0 Then
                    'Propdiscardtime does not include discards that survived
                    Me.m_SimData.Propdiscardtime(iflt, igrp) = Me.m_refdata.PoolForceDiscardProp(iflt, igrp, iForcedTime) * Me.m_SimData.PropDiscardMortTime(iflt, igrp)
                    Me.m_SimData.PropLandedTime(iflt, igrp) = 1 - Me.m_refdata.PoolForceDiscardProp(iflt, igrp, iForcedTime)

                    bForced = True
                    bFChanged = True
                End If

                If bFChanged Then
                    Debug.Assert((Me.m_SimData.PropLandedTime(iflt, igrp) + Me.m_SimData.Propdiscardtime(iflt, igrp)) <= 1.0, "Opps cEcosimModel.setForcedDiscards() may have calculated an incorrect PropLandedTime() or Propdiscardtime()")
                    'FishMGear() only contains catch that incure mortality
                    'Changing the discard mortality rate changes F
                    'Changing the proportion of landings and discards changes F if discard mort rate is not 1
                    'Calulate the new F from base values 
                    totCatch = (m_EPdata.Landing(iflt, igrp) + m_EPdata.Discard(iflt, igrp)) * (Me.m_SimData.PropLandedTime(iflt, igrp) + Me.m_SimData.Propdiscardtime(iflt, igrp))
                    m_SimData.FishMGear(iflt, igrp) = totCatch / m_EPdata.B(igrp)

                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'for debugging
                    'FishMGear should equal relQ * [proportion of catch mort] 
                    'debugTestRelQFishMGear will test this assumption
                    'FishMGear() = relQ() * (PropLandedTime() + Propdiscardtime())
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    bFChanged = False
                End If 'bFChanged

            Next iflt
        Next igrp

        'If bForced Then
        '    Me.SetFtimeFromGear(m_Data.StartBiomass, iModelTimeStep, QYear, False)
        '    Debugging check that FishMGear And relQ are still in sync
        '    Me.debugTestRelQFishMGear()
        'End If

    End Sub


    ''' <summary>
    ''' solvetime() is not called at this time. It has been left in for reference
    ''' </summary>
    ''' <param name="ip"></param>
    ''' <param name="Aloc"></param>
    ''' <param name="Floc"></param>
    ''' <param name="X"></param>
    ''' <param name="M"></param>
    ''' <param name="NomCols"></param>
    ''' <param name="Tol"></param>
    ''' <param name="jord"></param>
    ''' <param name="Dt"></param>
    ''' <remarks></remarks>
    Sub solvetime(ByVal ip As Integer, ByVal Aloc(,,) As Single, ByVal Floc(,,) As Single, ByVal X(,,) As Single, ByVal M As Integer, ByVal NomCols As Integer, ByVal Tol As Single, ByVal jord() As Integer, ByVal Dt As Single)
        Dim i As Integer, j As Integer, Xold(,) As Single
        ReDim Xold(m_Data.InRow + 1, m_Data.InCol + 1)
        For i = 0 To M + 1
            For j = 0 To NomCols + 1
                Xold(i, j) = X(i, j, ip)
            Next
        Next
        For i = 1 To M
            For j = 1 To NomCols
                X(i, j, ip) = (1 / (1 - Aloc(i, j, ip) * Dt)) * (Xold(i, j) + Dt * (Floc(i, j, ip) + Bcw(i, j, ip) * Xold(i - 1, j) + C(i, j, ip) * Xold(i + 1, j) + d(i, j - 1, ip) * Xold(i, j - 1) + e(i, j + 1, ip) * Xold(i, j + 1)))
            Next
        Next
    End Sub



    Private Sub SetMigGrad()
        'set habitat quality gradient maps for all habitat types, for use in biased movement assessments
        Dim i As Integer, j As Integer, ii As Integer, jj As Integer, iMigGrp As Integer
        Dim i1 As Integer, i2 As Integer, j1 As Integer, j2 As Integer, Sweep As Integer, imonth As Integer
        Dim nsweep As Integer
        Dim smallestDist As Single
        Dim pathFound As Integer

        Dim nMig As Integer
        Dim migIndex() As Integer
        ReDim migIndex(m_Data.NGroups)
        Dim diagAdjust As Single
        Dim minHabCap As Single = 0.001

        'Me.m_Data.debugSetMigMapsFromPrefRowCol()

        Try
            For i = 1 To m_Data.NGroups
                If m_Data.IsMigratory(i) Then
                    nMig = nMig + 1
                    migIndex(nMig) = i
                End If
            Next
            ReDim MigGrad(nMig, 12) ' m_Data.InRow + 1, m_Data.InCol + 1, nMig, 12)

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Initialize the MigGrad(row,col,group,month) migration gradient matrix with
            '0 for cells inside a migration area
            '1000 for cells outside migration area
            '2000 for cells in low capacity habitat or land
            For iMigGrp = 1 To nMig
                For imonth = 1 To 12

                    ' Debug.Assert(imonth <> 6)
                    Dim grad(m_Data.InRow + 1, m_Data.InCol + 1) As Single

                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1
                            grad(i, j) = 1000

                            If m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > MIN_MIG_PROB Then
                                grad(i, j) = 0
                            End If

                            If m_Data.Depth(i, j) = 0 Or m_Data.HabCap(migIndex(iMigGrp))(i, j) < minHabCap Then
                                grad(i, j) = 2000
                            End If

                        Next j
                    Next i
                    MigGrad(iMigGrp, imonth) = grad
                Next imonth
            Next iMigGrp
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


            If m_Data.InRow > m_Data.InCol Then nsweep = m_Data.InRow Else nsweep = m_Data.InCol
            nsweep = nsweep * 2
            iWindow = 1
            For iMigGrp = 1 To nMig
                For imonth = 1 To 12
                    For Sweep = 1 To nsweep
                        For i = 0 To m_Data.InRow + 1
                            For j = 0 To m_Data.InCol + 1

                                If MigGrad(iMigGrp, imonth)(i, j) > 0 Then
                                    smallestDist = 2000
                                    diagAdjust = 0
                                    'smallesti = -1
                                    'smallestJ = -1
                                    pathFound = False
                                    i1 = i - iWindow : If i1 < 0 Then i1 = 0
                                    i2 = i + iWindow : If i2 > m_Data.InRow + 1 Then i2 = m_Data.InRow + 1
                                    j1 = j - iWindow : If j1 < 0 Then j1 = 0
                                    j2 = j + iWindow : If j2 > m_Data.InCol + 1 Then j2 = m_Data.InCol + 1
                                    For ii = i1 To i2 : For jj = j1 To j2
                                            If ii = i Or jj = j Then
                                                diagAdjust = 0
                                            Else
                                                diagAdjust = 0.4142 'sqrt(2)-1
                                            End If

                                            If MigGrad(iMigGrp, imonth)(ii, jj) + diagAdjust < smallestDist And ((m_Data.Depth(i, j) > 0 And m_Data.HabCap(migIndex(iMigGrp))(i, j) > minHabCap) Or i = 0 Or i = m_Data.InRow + 1 Or j = 0 Or j = m_Data.InCol + 1) Then
                                                smallestDist = MigGrad(iMigGrp, imonth)(ii, jj) + diagAdjust
                                                ' Debug.Assert(Not (ii = 3 And jj > 1 And imonth = 6))
                                                pathFound = True
                                            End If
                                        Next
                                    Next

                                    If pathFound Then
                                        MigGrad(iMigGrp, imonth)(i, j) = smallestDist + 1
                                        'Debug.Assert(Not ((MigGrad(i, j, iMigGrp, imonth) > 0) And (m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > MIN_MIG_PROB)))
                                    End If

                                End If

                            Next j
                        Next i
                    Next Sweep 'Sweep = 1 To nsweep
                Next imonth 'imonth = 1 To 12
            Next iMigGrp 'iMigGrp = 1 To nMig

            ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
            ''DEBUGGING()
            ''Dump the migration maps to the debug/immediate window
            'Dim tempstr As String
            'For iMigGrp = 1 To nMig 'm_Data.NGroups
            '    For imonth = 1 To 12
            '        Debug.Print("")
            '        Debug.Print(Me.m_EPdata.GroupName(migIndex(iMigGrp)) + ", imonth = " + imonth.ToString)
            '        For i = 0 To m_Data.InRow + 1
            '            For j = 0 To m_Data.InCol + 1
            '                tempstr = tempstr + Math.Round(MigGrad(i, j, iMigGrp, imonth), 3).ToString.PadRight(10)

            '            Next j
            '            Debug.Print(tempstr)
            '            tempstr = ""
            '        Next i
            '    Next imonth
            'Next iMigGrp
            ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub


    Private Sub NormalizeMigrationMaps()
        Dim imonth As Integer
        Dim iMigGrp As Integer
        Dim nMig As Integer
        Dim MaxMigProb As Single
        Dim migIndex() As Integer
        ReDim migIndex(m_Data.NGroups)

        For i As Integer = 1 To m_Data.NGroups
            If m_Data.IsMigratory(i) Then
                nMig = nMig + 1
                migIndex(nMig) = i
            End If
        Next

        For iMigGrp = 1 To nMig
            For imonth = 1 To 12
                MaxMigProb = 0
                For i As Integer = 0 To m_Data.InRow + 1
                    For j As Integer = 0 To m_Data.InCol + 1

                        If m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > MIN_MIG_PROB Then
                            MaxMigProb = Math.Max(m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j), MaxMigProb)
                        End If

                    Next j
                Next i

                For i As Integer = 0 To m_Data.InRow + 1
                    For j As Integer = 0 To m_Data.InCol + 1

                        If m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > MIN_MIG_PROB Then
                            m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) /= (MaxMigProb + 1.0E-20)
                        End If

                    Next j
                Next i

            Next imonth
        Next iMigGrp

    End Sub





    'Sub VaryMigMovementParameters_HighMRate(ByVal imonth As Integer)
    '    '20-Jan-2016 Altered to base the migration movement on an area rather than a single point
    '    'the original code that set the movement based on a single cell is in VaryMigMovementParameters_SinglePoint()
    '    Dim ip As Integer
    '    Dim i As Integer, j As Integer, AdScale As Single
    '    Dim nvar2 As Integer, ir As Integer
    '    Dim Ep As Single
    '    Dim MaxCh As Single
    '    Dim FitRatio As Single
    '    AdScale = 1 '/ (2 * 3.14159 * CellLength)
    '    MaxCh = 1
    '    Dim ieco As Integer
    '    Dim imig As Integer
    '    Dim nMig As Integer
    '    Dim MigToEcopath() As Integer
    '    Dim migGradWeight As Single = 0.05

    '    ReDim MigToEcopath(m_Data.NGroups)
    '    For i = 1 To m_Data.NGroups
    '        If m_Data.IsMigratory(i) Then
    '            nMig = nMig + 1
    '            MigToEcopath(nMig) = i
    '        End If
    '    Next

    '    For imig = 1 To nMig
    '        ip = MigToEcopath(imig)
    '        ieco = IecoCode(ip)

    '        'calculate relative emigration rate from each cell as function
    '        'of fitness, scaling parameter KmoveFit(ip) set in setKmove routine
    '        For i = 0 To m_Data.InRow + 1
    '            For j = 0 To m_Data.InCol + 1
    '                If m_Data.FitRespType > 0 Then
    '                    Ep = -Kmovefit(ip) * RelFitness(i, j, ip)
    '                    If Ep < -MaxCh Then Ep = -MaxCh
    '                    If Ep > MaxCh Then Ep = MaxCh
    '                    Ep = Math.Exp(Ep)
    '                    RelMoveFit(i, j) = 2.0# * Ep / (1 + Ep)

    '                Else
    '                    RelMoveFit(i, j) = 1
    '                End If
    '            Next
    '        Next

    '        For i = 0 To m_Data.InRow
    '            For j = 0 To m_Data.InCol
    '                If m_Data.Depth(i, j) > 0 Then

    '                    'check depth on right face of this cell
    '                    If m_Data.Depth(i, j + 1) > 0 Then

    '                        If m_Data.FitRespType < 2 Then
    '                            e(i, j + 1, ip) = Enomig(i, j + 1, ip) * RelMoveFit(i, j + 1) * RelMigMove(i, j + 1, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                            d(i, j, ip) = dNomig(i, j, ip) * RelMoveFit(i, j) * RelMigMove(i, j, i, j + 1, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                        Else
    '                            FitRatio = RelMoveFit(i, j + 1) / RelMoveFit(i, j)
    '                            e(i, j + 1, ip) = Enomig(i, j + 1, ip) * FitRatio * RelMigMove(i, j + 1, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                            d(i, j, ip) = dNomig(i, j, ip) / FitRatio * RelMigMove(i, j, i, j + 1, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                        End If

    '                        If j = 0 Or j = m_Data.InCol Then
    '                            e(i, j + 1, ip) = 0
    '                            d(i, j, ip) = 0
    '                        End If

    '                        nvar2 = m_Data.NGroups
    '                        If ieco > 0 Then
    '                            ir = IecoCode(ip)
    '                            e(i, j + 1, nvar2 + ir) = e(i, j + 1, ip)
    '                            d(i, j, nvar2 + ir) = d(i, j, ip)
    '                        End If
    '                    End If ' If m_Data.Depth(i, j + 1) > 0 Then check depth on right face of this cell

    '                    'then check depths on bottom face of this cell
    '                    If m_Data.Depth(i + 1, j) > 0 Then
    '                        If m_Data.FitRespType < 2 Then
    '                            C(i, j, ip) = CNomig(i, j, ip) * RelMoveFit(i + 1, j) * RelMigMove(i + 1, j, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                            Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) * RelMoveFit(i, j) * RelMigMove(i, j, i + 1, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                        Else
    '                            FitRatio = RelMoveFit(i + 1, j) / RelMoveFit(i, j)
    '                            C(i, j, ip) = CNomig(i, j, ip) * FitRatio * RelMigMove(i + 1, j, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                            Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) / FitRatio * RelMigMove(i, j, i + 1, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
    '                        End If

    '                        If i = 0 Or i = m_Data.InRow Then
    '                            C(i, j, ip) = 0
    '                            Bcw(i + 1, j, ip) = 0
    '                        End If

    '                        If ieco > 0 Then
    '                            ir = ieco
    '                            Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ip)
    '                            C(i, j, nvar2 + ir) = C(i, j, ip)
    '                        End If

    '                    End If 'If m_Data.Depth(i + 1, j) > 0 Then then check depths on bottom face of this cell

    '                End If 'If m_Data.Depth(i, j) > 0 Then

    '            Next j
    '        Next i
    '    Next imig
    'End Sub


    Sub VaryMigMovementParameters(ByVal imonth As Integer)
        '20-Jan-2016 Altered to base the migration movement on an area rather than a single point
        'the original code that set the movement based on a single cell is in VaryMigMovementParameters_SinglePoint()
        Dim ip As Integer
        Dim i As Integer, j As Integer, AdScale As Single
        Dim nvar2 As Integer, ir As Integer
        Dim Ep As Single
        Dim MaxCh As Single
        Dim FitRatio As Single
        AdScale = 1 '/ (2 * 3.14159 * CellLength)
        MaxCh = 1
        Dim ieco As Integer
        Dim imig As Integer
        Dim nMig As Integer
        Dim MigToEcopath() As Integer

        ReDim MigToEcopath(m_Data.NGroups)
        For i = 1 To m_Data.NGroups
            If m_Data.IsMigratory(i) Then
                nMig = nMig + 1
                MigToEcopath(nMig) = i
            End If
        Next

        For imig = 1 To nMig
            ip = MigToEcopath(imig)
            ieco = IecoCode(ip)

            'calculate relative emigration rate from each cell as function
            'of fitness, scaling parameter KmoveFit(ip) set in setKmove routine
            For i = 0 To m_Data.InRow + 1
                For j = 0 To m_Data.InCol + 1
                    If m_Data.FitRespType > 0 Then
                        Ep = -Kmovefit(ip) * RelFitness(i, j, ip)
                        If Ep < -MaxCh Then Ep = -MaxCh
                        If Ep > MaxCh Then Ep = MaxCh
                        Ep = Math.Exp(Ep)
                        RelMoveFit(i, j) = 2.0# * Ep / (1 + Ep)

                    Else
                        RelMoveFit(i, j) = 1
                    End If
                Next
            Next

            For i = 0 To m_Data.InRow
                For j = 0 To m_Data.InCol
                    If m_Data.Depth(i, j) > 0 Then

                        ' Debug.Assert(Not (ip = 23 And i > 0 And j > 0))

                        'check depth on right face of this cell
                        'can there be movement to or from the cell to the right for this cell
                        If m_Data.Depth(i, j + 1) > 0 Then

                            If m_Data.FitRespType < 2 Then
                                'e() is the movement to the left 
                                'set the movement from the cell to the left into this cell
                                e(i, j + 1, ip) = getMigMoveRate(Enomig, ip, i, j + 1, i, j, imonth) * RelMoveFit(i, j + 1) * RelMigMove(i, j + 1, i, j, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)
                                'Movement from this cell into the cell to the right
                                d(i, j, ip) = getMigMoveRate(dNomig, ip, i, j, i, j + 1, imonth) * RelMoveFit(i, j) * RelMigMove(i, j, i, j + 1, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)

                            Else
                                FitRatio = RelMoveFit(i, j + 1) / RelMoveFit(i, j)
                                e(i, j + 1, ip) = getMigMoveRate(Enomig, ip, i, j + 1, i, j, imonth) * FitRatio * RelMigMove(i, j, i, j + 1, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)
                                d(i, j, ip) = getMigMoveRate(dNomig, ip, i, j, i, j + 1, imonth) / FitRatio * RelMigMove(i, j + 1, i, j, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)

                            End If

                            If j = 0 Or j = m_Data.InCol Then
                                e(i, j + 1, ip) = 0
                                d(i, j, ip) = 0
                            End If

                            nvar2 = m_Data.NGroups
                            If ieco > 0 Then
                                ir = IecoCode(ip)
                                e(i, j + 1, nvar2 + ir) = e(i, j + 1, ip)
                                d(i, j, nvar2 + ir) = d(i, j, ip)
                            End If
                        End If ' If m_Data.Depth(i, j + 1) > 0 Then check depth on right face of this cell

                        'then check depths on bottom face of this cell
                        If m_Data.Depth(i + 1, j) > 0 Then
                            If m_Data.FitRespType < 2 Then
                                C(i, j, ip) = getMigMoveRate(CNomig, ip, i, j, i + 1, j, imonth) * RelMoveFit(i + 1, j) * RelMigMove(i + 1, j, i, j, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)
                                Bcw(i + 1, j, ip) = getMigMoveRate(BcwNomig, ip, i + 1, j, i, j, imonth) * RelMoveFit(i, j) * RelMigMove(i, j, i + 1, j, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)

                            Else
                                FitRatio = RelMoveFit(i + 1, j) / RelMoveFit(i, j)
                                C(i, j, ip) = getMigMoveRate(CNomig, ip, i, j, i + 1, j, imonth) * FitRatio * RelMigMove(i + 1, j, i, j, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)
                                Bcw(i + 1, j, ip) = getMigMoveRate(BcwNomig, ip, i + 1, j, i, j, imonth) / FitRatio * RelMigMove(i, j, i + 1, j, MigGrad(imig, imonth), m_Data.MoveScale, imig, imonth, ip)

                            End If

                            If i = 0 Or i = m_Data.InRow Then
                                C(i, j, ip) = 0
                                Bcw(i + 1, j, ip) = 0
                            End If

                            If ieco > 0 Then
                                ir = ieco
                                Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ip)
                                C(i, j, nvar2 + ir) = C(i, j, ip)
                            End If

                        End If 'If m_Data.Depth(i + 1, j) > 0 Then then check depths on bottom face of this cell

                    End If 'If m_Data.Depth(i, j) > 0 Then

                Next j
            Next i
        Next imig

        'Me.debugDumpFlowRates(C, 23, "VaryMigMovementParameters c " + imonth.ToString)
        'Me.debugDumpFlowRates(Bcw, 23, "VaryMigMovementParameters b " + imonth.ToString)

    End Sub

    ''' <summary>
    ''' Calculate the directional flow rate (Bcw,c,d,e) dependant on migratory movement. 
    ''' Base dispersal if inside a migratory area, rapid dispersal if outside migratory area.
    ''' </summary>
    ''' <param name="BaseFlow">Directional flow rate for a cell. Bcw, c, d or e arrays</param>
    ''' <param name="iGroup"></param>
    ''' <param name="iMonth"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getMigMoveRate(BaseFlow(,,) As Single, iGroup As Integer, iRowFrom As Integer, iColFrom As Integer, iRowTo As Integer, iColTo As Integer, iMonth As Integer) As Single

        Debug.Assert(Me.m_Data.IsMigratory(iGroup), "Really... you should only call getMigMoveRate() for a migratory group.")

        'Flow rates based on migration caused the biomass outside of the migration areas to clump on the boundaries
        'Just return the base directional flow rate for now
        Return BaseFlow(iRowFrom, iColFrom, iGroup)

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'jb This caused the biomass to clump up on the boundaries of the migration area
        'Left in place incase we want to implement this at a later date

        'Dim MigProbFrom As Single = Me.m_Data.MigMaps(iGroup, iMonth)(iRowFrom, iColFrom)
        'Dim MigProbTo As Single = Me.m_Data.MigMaps(iGroup, iMonth)(iRowTo, iColTo)
        'If MigProbFrom > MIN_MIG_PROB Or MigProbTo > MIN_MIG_PROB Then
        '    ''BaseFlow() contains the [habitat gradient flows] * [mRate]
        '    ''Get the flow due to habitat gradient only
        '    Dim habGrad As Single = BaseFlow(iRowFrom, iColFrom, iGroup) / Me.m_Data.Mrate(iGroup)
        '    ''return the direction flow with base dispersal rate
        '    Return habGrad * m_Data.Mvel(iGroup) / (3.14159 * m_Data.CellLength)

        'Else
        '    'Not a migrating cell
        '    'return the base flow rate calculated with a higher mRate() for migration 
        '    Return BaseFlow(iRowFrom, iColFrom, iGroup)
        'End If

    End Function


    Private Sub TeleportMigrationBiomass(iMonth As Integer)

        System.Console.WriteLine("WOW FUCK Migratory biomass has been magically transported to the new migrating area! YEAH REALLY THIS IS FUCKED UP...")

        Dim imig As Integer
        Dim nMig As Integer
        Dim MigToEcopath() As Integer
        Dim sumB() As Single
        Dim SumMigProb() As Single

        ReDim MigToEcopath(m_Data.NGroups)
        For i As Integer = 1 To m_Data.NGroups
            If m_Data.IsMigratory(i) Then
                nMig = nMig + 1
                MigToEcopath(nMig) = i
            End If
        Next

        sumB = New Single(nMig) {}
        SumMigProb = New Single(nMig) {}
        'sum the biomass for all migratory groups
        'sum the probabilities of presents from the MigMaps
        For imig = 1 To nMig
            For irow As Integer = 1 To m_Data.InRow
                For icol As Integer = 1 To m_Data.InCol
                    If m_Data.Depth(irow, icol) > 0 Then
                        sumB(imig) += m_Data.Bcell(irow, icol, MigToEcopath(imig))

                        If m_Data.MigMaps(MigToEcopath(imig), iMonth)(irow, icol) > MIN_MIG_PROB Then
                            SumMigProb(imig) += m_Data.MigMaps(MigToEcopath(imig), iMonth)(irow, icol)
                        End If
                    End If
                Next icol
            Next irow
        Next imig

        Dim tempBSum As Single

        'now spread the biomass into the new migratory cells
        'based on the MigMaps() probability of presents
        For imig = 1 To nMig
            tempBSum = 0
            For irow As Integer = 1 To m_Data.InRow
                For icol As Integer = 1 To m_Data.InCol
                    If m_Data.Depth(irow, icol) > 0 Then
                        m_Data.Bcell(irow, icol, MigToEcopath(imig)) = 0 ' 1.0E-20
                        If m_Data.MigMaps(MigToEcopath(imig), iMonth)(irow, icol) > MIN_MIG_PROB Then
                            m_Data.Bcell(irow, icol, MigToEcopath(imig)) = sumB(imig) * (m_Data.MigMaps(MigToEcopath(imig), iMonth)(irow, icol) / SumMigProb(imig))
                            tempBSum += m_Data.Bcell(irow, icol, MigToEcopath(imig))
                        End If
                    End If
                Next icol
            Next irow

            'Debug.Assert(tempBSum = sumB(imig))
        Next imig


    End Sub





    Function RelMigMove(ByVal iRowFrom As Integer, ByVal iColFrom As Integer, ByVal iRowTo As Integer, ByVal iColTo As Integer, ByVal G(,) As Single, ByVal gk As Single, ByVal iMigGrp As Integer, ByVal imonth As Integer, ByVal ip As Integer) As Single
        'sets relative movement rate using slope of g() function between origin (i1,j1) and destination (i2,j2) cells
        'function is 1 when slope ss is zero

        Dim Ss As Single
        Dim multDir As Single
        Dim numDir As Single

        Dim RelMovement As Single = 1
        Dim BarrierAvoid As Single = m_Data.barrierAvoidanceWeight(ip)
        If BarrierAvoid <= 0 Then BarrierAvoid = 0.0000000001

        'xxxxxxxxxxxxxxxxxxxxxxxxxxx
        'WARNING for debugging
        'Set gk to some other value for testing
        'gk = 0.001
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        Try
            'If both cells are in the same migration area then use the internal migMaps movement probabilities
            If Me.m_Data.MigMaps(ip, imonth)(iRowTo, iColTo) > MIN_MIG_PROB And Me.m_Data.MigMaps(ip, imonth)(iRowFrom, iColFrom) > MIN_MIG_PROB Then
                'Use the gradient to decide the direction of flow
                Dim grad As Single = Me.m_Data.MigMaps(ip, imonth)(iRowTo, iColTo) / Me.m_Data.MigMaps(ip, imonth)(iRowFrom, iColFrom)
                If grad = 1.0 Then
                    Return 1.0
                ElseIf grad > 1 Then
                    Return 1 + m_Data.InMigAreaMovement(ip)
                Else
                    Return 1 - m_Data.InMigAreaMovement(ip)
                End If

            End If

            multDir = 1
            If iRowFrom > 0 And iColFrom > 0 And iRowFrom <= m_Data.InRow And iColFrom <= m_Data.InCol Then
                'Calculate the number of cells to migrated into
                If (G(iRowFrom + 1, iColFrom) - G(iRowFrom, iColFrom)) < 0 Then numDir += 1.0
                If (G(iRowFrom - 1, iColFrom) - G(iRowFrom, iColFrom)) < 0 Then numDir += 1.0
                If (G(iRowFrom, iColFrom - 1) - G(iRowFrom, iColFrom)) < 0 Then numDir += 1.0
                If (G(iRowFrom, iColFrom + 1) - G(iRowFrom, iColFrom)) < 0 Then numDir += 1.0
                'numDir will = zero if this cell is in preferred migratory area
                If numDir <> 0.0F Then
                    'Multiple direction weighting
                    'Each cell gets an equal weight
                    multDir = 1 / numDir
                Else
                    multDir = 1
                End If

            End If

            'HACK Warning
            'multDir = 1

            Ss = G(iRowTo, iColTo) - G(iRowFrom, iColFrom)
            Select Case Ss
                'Case 0
                'RelMigMove = 1
                Case Is < 0
                    RelMovement = 1 + BarrierAvoid * multDir * G(iRowFrom, iColFrom) / (0.5 * gk + G(iRowFrom, iColFrom)) '2 / (2 - Math.Exp(-G(i1, j1, ihab, imonth)))
                Case Is > 0
                    RelMovement = 1 - BarrierAvoid * multDir * G(iRowFrom, iColFrom) / (0.5 * gk + G(iRowFrom, iColFrom))
                Case Else
                    RelMovement = 1 'Stop
            End Select

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Return RelMovement

    End Function

    Sub AdjustSpaceParsNew()
        'set ecospace basebiomass using proportions of usable habitat for each pool, and adjust
        'vulnerability, search parameters to mean biomasses in habitats used
        Dim i As Integer, j As Integer, ii As Integer, ia As Integer
        Dim Qarena() As Single, VulBiom() As Single

        'just set v and a to base values, do not change basebiomass unless adjustspace=true
        'get habitat areas
        ReDim m_Data.BRatio(m_Data.NGroups)

        CalcHabitatArea()
        'calculate habitat area used by each biomass type
        ReDim HabAreaUsed(m_Data.NGroups)

        For i = 1 To m_Data.NGroups

            If Me.m_Data.TotHabCap(i) > 0 Then
                Basebiomass(i) = m_Data.ThabArea * m_SimData.StartBiomass(i) / Me.m_Data.TotHabCap(i) ' HabAreaUsed(i)
                m_Data.BRatio(i) = m_Data.ThabArea / Me.m_Data.TotHabCap(i) 'HabAreaUsed(i)
                If m_Data.AdjustSpace = False Then m_Data.BRatio(i) = 1
            Else
                Basebiomass(i) = m_SimData.StartBiomass(i) 'don't really need this; set before routine called
                m_Data.BRatio(i) = 1
            End If
        Next

        'adjust vulnerability and search parameters for these basebiomass values in preferred habitats
        ReDim m_Data.Vspace(m_SimData.Narena), m_Data.Aspace(m_SimData.inlinks)

        'find total consumptions of prey type for each arena, added over predators
        ReDim Qarena(m_SimData.Narena), VulBiom(m_SimData.Narena)
        For ii = 1 To m_SimData.inlinks
            i = m_SimData.ilink(ii)
            j = m_SimData.jlink(ii)
            ia = m_SimData.ArenaLink(ii)
            Qarena(ia) = Qarena(ia) + m_SimData.Qlink(ii) * m_Data.BRatio(j)
        Next
        'then set initial vulnerable biomasses (V) by arena
        For ii = 1 To m_SimData.Narena
            i = m_SimData.Iarena(ii)
            j = m_SimData.Jarena(ii)
            If m_SimData.VulMult(i, j) > 10000000000.0# Then m_SimData.VulMult(i, j) = 10000000000.0#
            m_Data.Vspace(ii) = (m_SimData.VulMult(i, j) + 0.0000000001) * Qarena(ii) / (m_SimData.StartBiomass(i) * m_Data.BRatio(i))
            If m_Data.Vspace(ii) = 0 Then m_Data.Vspace(ii) = 1
            ' CJW 12jun11: Fix possible cause of the jumps in biomass of spatially restricted groups.
            '              This correction is aimed at preventing underestimation of predation mortality rates 
            '              on spatially restricted groups by predators that are widely distributed. When Vspace
            '              is too low, one effect will be to cause the iterative procedure that I sent this 
            '              morning to diverge, giving larger Atemp(ii) values on each iteration without limit.
            '              What this divergence means is that Vspace is set too low to predict the “observed” 
            '              ecopath base consumption rates, when added up over the ecospace grid.
            'Debug.Assert(m_Data.Vspace(ii) > m_SimData.VulArena(ii))
            If m_Data.Vspace(ii) < m_SimData.VulArena(ii) Then m_Data.Vspace(ii) = m_SimData.VulArena(ii)

            If m_SimData.BoutFeeding Then
                VulBiom(ii) = -Qarena(ii) / Math.Log(1 - 1 / (m_SimData.VulMult(i, j) + 0.0000000001))
            Else
                VulBiom(ii) = (m_SimData.VulMult(i, j) + 0.0000000001 - 1.0#) * Qarena(ii) / (2 * m_Data.Vspace(ii))
            End If
            If VulBiom(ii) = 0 Then VulBiom(ii) = 1

            'note above calculation will give wrong result if vulmult(i,j)=1, i.e. vulmult must be strictly
            'greater than 1.0
            'set nonzero value for vularena to avoid divides by zero if no feeding in it
        Next
        'then set predator search rates (a) by trophic link
        Dim Dzero As Single
        For ii = 1 To m_SimData.inlinks
            ia = m_SimData.ArenaLink(ii)
            j = m_SimData.jlink(ii)
            If VulBiom(ia) > 0 Then
                Dzero = m_SimData.CmCo(j) / (m_SimData.CmCo(j) - 1)
                m_Data.Aspace(ii) = Dzero * m_SimData.Qlink(ii) / (VulBiom(ia) * m_SimData.pred(j))
            Else
                m_Data.Aspace(ii) = 0
            End If
        Next

        'adjust pbbiomass for primary producers
        For i = 1 To m_Data.NGroups
            If m_SimData.pbm(i) > 0 Then 'primary producer
                ' PbSpace(i) = m_SimData.pbbiomass(i) * m_SimData.StartBiomass(i) / Basebiomass(i)
                PbSpace(i) = m_SimData.pbbiomass(i) / m_Data.BRatio(i)
            End If
        Next

        ' CJW 12Jun2011: This might fix that jumping problem with multistanza groups
        Dim ir As Integer, ic As Integer, Pbar(m_Data.NGroups) As Single, Atemp(m_SimData.inlinks) As Single, Vtemp(m_SimData.Narena) As Single, MeanBP(m_SimData.inlinks) As Single
        Dim iter As Integer, Vden(m_SimData.Narena) As Single 'calculate mean Pred for vratio (Vtemp) estimation
        For i = 1 To m_Data.NGroups
            Pbar(i) = m_SimData.pred(i) * m_Data.ThabArea / m_Data.TotHabCap(i)
        Next

        'calculate mean BP product over cells for each link
        For ii = 1 To m_SimData.inlinks
            i = m_SimData.ilink(ii)
            j = m_SimData.jlink(ii)
            Atemp(ii) = m_Data.Aspace(ii) * (m_SimData.CmCo(j) - 1) / m_SimData.CmCo(j)  'remove handling time adjustment to start iteration 
            MeanBP(ii) = 0
            For ir = 1 To m_Data.InRow
                For ic = 1 To m_Data.InCol
                    MeanBP(ii) = MeanBP(ii) + m_Data.HabCap(i)(ir, ic) * m_Data.HabCap(j)(ir, ic)
                Next
            Next
            MeanBP(ii) = (MeanBP(ii) * m_SimData.StartBiomass(i) * m_SimData.pred(j) / (m_Data.TotHabCap(i) * m_Data.TotHabCap(j))) * m_Data.ThabArea
        Next ii

        Dim Anew As Single, TotCh As Single
        'iterate to improve atemp estimates
        For iter = 1 To 5 'spreadsheet tests indicate this should be enough iterations
            'calculate Vtemp ratio of vulnerable to total prey biomass for this iter, recognizing ratio is independent of ir,ic because P's divided by local caps
            For ia = 1 To m_SimData.Narena
                Vden(ia) = 2 * m_Data.Vspace(ia)
            Next
            For ii = 1 To m_SimData.inlinks
                j = m_SimData.Jarena(ii)
                i = m_SimData.Iarena(ii)
                ia = m_SimData.ArenaNo(i, j)
                Vden(ia) = Vden(ia) + Atemp(ii) * Pbar(j)
            Next
            For ia = 1 To m_SimData.Narena
                Vtemp(ia) = m_Data.Vspace(ia) / Vden(ia)
            Next
            TotCh = 0
            'now recalculate the a's by link
            For ii = 1 To m_SimData.inlinks

                i = m_SimData.ilink(ii)
                j = m_SimData.jlink(ii)
                ia = m_SimData.ArenaNo(i, j)

                Anew = m_SimData.Qlink(ii) / (Vtemp(ia) * MeanBP(ii))
                TotCh = TotCh + Math.Abs(Anew - Atemp(ii))
                Atemp(ii) = Anew
            Next
            If TotCh < 0.01 * m_SimData.inlinks Then Exit For
        Next iter

        'finally correct the a's for handling time effects
        For ii = 1 To m_SimData.inlinks
            j = m_SimData.jlink(ii)
            m_Data.Aspace(ii) = Atemp(ii) * m_SimData.CmCo(j) / (m_SimData.CmCo(j) - 1)
        Next

        '#End If

    End Sub

    Private Function estimateMaxTimestep()
        Dim ntc As Integer
        Dim i As Integer, j As Integer, iGrp As Integer
        Dim Ceq As Single, Cin As Single, Cout As Single
        Dim Terr As Single, Ttemp As Single, maxT As Single

        maxT = m_Data.TimeStep

        For i = 1 To m_Data.InRow
            For j = 1 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                End If
                For iGrp = 1 To m_Data.NGroups
                    Cin = m_Data.Ftr(i, j, iGrp) '+ inflows from surrounding cells
                    Cin = Cin + Bcw(i, j, iGrp) * m_Data.Ccell(i - 1, j, iGrp)
                    Cin = Cin + C(i, j, iGrp) * m_Data.Ccell(i + 1, j, iGrp)
                    Cin = Cin + d(i, j - 1, iGrp) * m_Data.Ccell(i, j - 1, iGrp)
                    Cin = Cin + e(i, j + 1, iGrp) * m_Data.Ccell(i, j + 1, iGrp)
                    Cout = -m_Data.AMmTr(i, j, iGrp)

                    If Cout <> 0 Then
                        Ceq = Cin / Cout
                        'calculate distance to equilibrium (%)
                        Terr = CSng(2.0 * Math.Abs(Ceq - m_Data.Ccell(i, j, iGrp)) / (Ceq + m_Data.Ccell(i, j, iGrp) + 1.0E-30))
                        If Terr < 0.5 Then
                            Terr = 0.5
                        End If
                        'minimum timestep is 0.01 times 1/closs (which is essentially the time to equilibrium at the current derivative value)
                        'the timestep scales from (0.01 to 0.1) times 1/closs as ConcTr approaches Ceq
                        Ttemp = CSng(0.1 / Terr / Cout)
                        If Ttemp < maxT Then
                            maxT = Ttemp
                        End If
                    End If

                Next
            Next
        Next

        ntc = Math.Ceiling(m_Data.TimeStep / maxT)
        'jb 1-Dec-2016 Hack to cap the number of ecotrace time steps!
        'Default to 1000
        Return Math.Min(ntc, Me.m_tracerData.MaxTimeSteps)

    End Function

#End Region

#Region "Data summary"

    ' ''' <summary>
    ' ''' Accumulate the fisheries data (catch) for a single group for this map cell. 
    ' ''' This is called before DerivtRed(), in the time step, so it is the condition at the start of the time step.
    ' ''' </summary>
    ' ''' <param name="Biomass">Biomass for all the groups at this time step</param>
    ' ''' <param name="iRow">Map row</param>
    ' ''' <param name="iCol">Map col</param>
    ' ''' <remarks></remarks>
    'Public Sub accumCatchData(ByVal iCumTime As Integer, ByVal iYear As Integer, ByVal Biomass() As Single, ByVal FMortByGroup() As Single, ByVal iRow As Integer, ByVal iCol As Integer)
    '    Dim cellCatch As Single, iFlt As Integer, igrp As Integer
    '    'Dim Landings(,) As Single

    '    'ReDim Landings(Me.m_EPdata.NumGroups, Me.m_EPdata.NumFleet)

    '    'Only one thread can use this code at a time
    '    'block all others
    '    Me.m_SpaceCatchSemaphor.WaitOne()

    '    Try

    '        For iFlt = 1 To m_Data.nFleets
    '            'Effort
    '            m_Data.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iFlt, iCumTime) += m_Data.EffortSpace(iFlt, iRow, iCol)
    '            'SailingEffort: at this point SailingEffort is  sum of [fishing effort] * [effort of fishing each cell (Sail(iFlt, iRow, iCol))] /  SailScale(ifleet)
    '            'Effort of fishing all the cells
    '            m_Data.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iFlt, iCumTime) += (m_Data.EffortSpace(iFlt, iRow, iCol) * m_Data.Sail(iFlt, iRow, iCol) / m_Data.SailScale(iFlt))

    '            'sum values into All Fleets 0 index 
    '            m_Data.ResultsByFleet(eSpaceResultsFleets.FishingEffort, 0, iCumTime) += m_Data.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iFlt, iCumTime)
    '            m_Data.ResultsByFleet(eSpaceResultsFleets.SailingEffort, 0, iCumTime) += m_Data.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iFlt, iCumTime)

    '            ''To get the original effort the effortspace is divided by the fishrategear for the month
    '            'If m_ESData.FishRateGear(iFlt, iCumTime) > 0 Then
    '            '    m_Data.SumCostInit(iSumIndex, iFlt) = m_Data.SumCostInit(iSumIndex, iFlt) + m_Data.EffortSpace(iFlt, iRow, iCol) / m_ESData.FishRateGear(iFlt, iCumTime) * m_Data.Sail(iFlt, iRow, iCol) / m_Data.SailScale(iFlt)
    '            'End If

    '        Next

    '        For igrp = 1 To Me.m_Data.NGroups

    '            If m_EPdata.fCatch(igrp) > 0 Then
    '                'jb 29-Jan-12 in the multithreaded version FishTime was not updated to the F for this cell
    '                'use fishing mortality rate passed in instead 
    '                'Dim bCatch As Single = Biomass(igrp) * m_SimData.FishTime(igrp) * m_Data.Width(iRow)
    '                Dim bCatch As Single = Biomass(igrp) * FMortByGroup(igrp) * m_Data.Width(iRow)
    '                m_Data.ResultsByGroup(eSpaceResultsGroups.CatchBio, igrp, iCumTime) = m_Data.ResultsByGroup(eSpaceResultsGroups.CatchBio, igrp, iCumTime) + bCatch
    '                m_Data.CatchMap(iRow, iCol, igrp) += bCatch
    '                'Next value of catch, depends on what gear was used:
    '                For iFlt = 1 To m_EPdata.NumFleet
    '                    If m_EPdata.Landing(iFlt, igrp) + m_EPdata.Discard(iFlt, igrp) > 0 Then
    '                        'First get catch
    '                        cellCatch = Biomass(igrp) * m_Data.EffortSpace(iFlt, iRow, iCol) * m_SimData.relQ(iFlt, igrp) * m_Data.Width(iRow)

    '                        'Sum the total catch by gear
    '                        m_Data.ResultsByFleet(eSpaceResultsFleets.CatchBio, iFlt, iCumTime) += cellCatch
    '                        'sum all fleets
    '                        m_Data.ResultsByFleet(eSpaceResultsFleets.CatchBio, 0, iCumTime) += cellCatch

    '                        m_Data.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, iFlt, igrp, iCumTime) += cellCatch
    '                        'sum all fleets into the zero fleet index
    '                        m_Data.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, 0, igrp, iCumTime) += cellCatch

    '                        'Next line is for adding up catch by region etc
    '                        If m_Data.nRegions > 0 Then
    '                            m_Data.ResultsCatchRegionGearGroup(m_Data.Region(iRow, iCol), iFlt, igrp, iCumTime) += cellCatch
    '                        End If

    '                        m_Data.Landings(igrp, iFlt) += cellCatch * Me.m_EPdata.PropLanded(iFlt, igrp)
    '                    End If
    '                Next iFlt
    '            End If 'If m_EPdata.fCatch(igrp) > 0 Then
    '        Next igrp


    '    Catch ex As Exception
    '        cLog.Write(ex)
    '    End Try

    '    Me.m_SpaceCatchSemaphor.Release()



    '    '                        '060109VC: Adding Time Series Reference Data to Ecospace
    '    '                        'Will only save data for once per year, (at half year)
    '    '                        'save at iYear = int(timenow)
    '    '                        'ReDim SpaceBiomassByRegion(totalTime, m_data.nGroups, NoRegions)
    '    '                        'ReDim SpaceCatchByRegion(totalTime, m_data.nGroups, NoRegions)
    '    '                        If StoreTimeSeriesData Then
    '    '                            SpaceBiomassByRegion(iYear, iGrp, 0) = SpaceBiomassByRegion(iYear, iGrp, 0) + Biomass(iGrp)
    '    '                            SpaceBiomassByRegion(iYear, iGrp, Region(iRow, iCol)) = SpaceBiomassByRegion(iYear, iGrp, Region(iRow, iCol)) + Biomass(iGrp)
    '    '                            SpaceBiomassByRegionCount(iYear, iGrp, 0) = SpaceBiomassByRegionCount(iYear, iGrp, 0) + 1
    '    '                            SpaceBiomassByRegionCount(iYear, iGrp, Region(iRow, iCol)) = SpaceBiomassByRegionCount(iYear, iGrp, Region(iRow, iCol)) + 1
    '    '                    If Catch(iGrp) > 0 Then
    '    '                                SpaceCatchByRegion(iYear, iGrp, 0) = SpaceCatchByRegion(iYear, iGrp, 0) + Biomass(iGrp) * FishTime(iGrp)
    '    '                                SpaceCatchByRegion(iYear, iGrp, Region(iRow, iCol)) = SpaceCatchByRegion(iYear, iGrp, Region(iRow, iCol)) + Biomass(iGrp) * FishTime(iGrp)
    '    '                                SpaceCatchByRegionCount(iYear, iGrp, 0) = SpaceCatchByRegionCount(iYear, iGrp, 0) + 1
    '    '                                SpaceCatchByRegionCount(iYear, iGrp, Region(iRow, iCol)) = SpaceCatchByRegionCount(iYear, iGrp, Region(iRow, iCol)) + 1
    '    '                            End If
    '    '                            If iGrp = 1 Then
    '    '                                ForiFlt= 1 To NumGear
    '    '                                    SpaceEffortByRegionFleet(iYear, ig, 0) = SpaceEffortByRegionFleet(iYear, ig, 0) + EffortSpace(ig, iRow, iCol)
    '    '                                    SpaceEffortByRegionFleet(iYear, ig, Region(iRow, iCol)) = SpaceEffortByRegionFleet(iYear, ig, Region(iRow, iCol)) + EffortSpace(ig, iRow, iCol)
    '    '                                    SpaceEffortByRegionFleetCount(iYear, ig, 0) = SpaceEffortByRegionFleetCount(iYear, ig, 0) + 1
    '    '                                    SpaceEffortByRegionFleetCount(iYear, ig, Region(iRow, iCol)) = SpaceEffortByRegionFleetCount(iYear, ig, Region(iRow, iCol)) + 1
    '    '                                Next
    '    '                            End If
    '    '                        End If

    '    '                        'abmpa: use this routine for ecoseed abmpa
    '    '                        If En1 >= 0 And chkMPA.value = Checked Then
    '    '                            If Shadow(iGrp) > 0 Then Ecoseed.CalcBioValSeed(iRow, iCol, ebb(), iGrp, En1)
    '    '                    If Catch(iGrp) > 0 Then Ecoseed.CalcGearValSeed iRow, iCol, ebb(), iGrp, En1
    '    '                        End If

    'End Sub


    Private Sub calcValue(ByVal iCumTime As Integer, ByVal iYear As Integer)
        Dim igrp As Integer
        Dim iflt As Integer
        Dim AvgLandings(,) As Single
        ReDim AvgLandings(Me.m_Data.NGroups, Me.m_Data.nFleets)

        'Ecospace Landings(group,fleet) are the sum of landing across all cells for a timestep.
        'SetPriceMedFunctions(landings) sets the PES multiplier based on Ecopath landings which are t/km2/Year
        'The average will be the landing in Ecopath units 
        For igrp = 1 To Me.m_EPdata.NumGroups
            For iflt = 0 To Me.m_EPdata.NumFleet
                AvgLandings(igrp, iflt) = Me.m_Data.Landings(igrp, iflt) / Me.m_Data.nWaterCells
            Next
        Next

        'set the price elasticity values for the average landings
        Me.m_SimData.PriceMedData.SetPriceMedFunctions(AvgLandings)

        'Value of landings
        Dim ValLandings As Single
        For igrp = 1 To Me.m_EPdata.NumGroups

            For iflt = 0 To Me.m_EPdata.NumFleet

                If Me.m_Data.Landings(igrp, iflt) > 0.0 Then
                    'Value = Landings * [market value] * [price elasticity multiplier]
                    'Value is in the same units as Ecopath Off-vessel price value/km2/year. So the landings need to be the Ecospace average.
                    ValLandings = AvgLandings(igrp, iflt) * Me.m_EPdata.Market(iflt, igrp) * Me.m_SimData.PriceMedData.getPESMult(igrp, iflt)

                    'Add to group and to gear sums
                    m_Data.ResultsByFleetGroup(eSpaceResultsFleetsGroups.Value, iflt, igrp, iCumTime) += ValLandings
                    'sum of all fleets into zero index
                    m_Data.ResultsByFleetGroup(eSpaceResultsFleetsGroups.Value, 0, igrp, iCumTime) += ValLandings

                    m_Data.ResultsByFleet(eSpaceResultsFleets.Value, iflt, iCumTime) += ValLandings
                    m_Data.ResultsByFleet(eSpaceResultsFleets.Value, 0, iCumTime) += ValLandings

                    m_Data.ResultsByGroup(eSpaceResultsGroups.Value, igrp, itt) += ValLandings

                    'Landings and Value for searches
                    If Me.m_search.bInSearch Then
                        'Search CatchYear() is the sum of monthly catch over one year 
                        'Landings(igrp, iFlt) is the yearly Ecopath landings so convert it to monthly for Search CatchYear()
                        Me.m_search.CatchYear(iflt, igrp) += Me.m_Data.Landings(igrp, iflt) * Me.m_Data.TimeStep
                        If iYear > Me.m_search.BaseYear Then
                            'Search value = Landings * [market value] * [price elasticity multiplier] * [discount factor] * [monthly conversion]
                            Me.m_search.ValCatch(iflt, igrp) += ValLandings * Me.m_search.DF * Me.m_Data.TimeStep
                        End If
                    End If

                End If '  If Landings(igrp, iFlt) > 0.0 Then

            Next
        Next

    End Sub


    Private Sub UpdateThreadedResults()
        Dim solver As cSpaceSolver
        Dim ieco As Integer

        'Don't save the results if in a Spin-Up period
        'If Me.m_Data.bInSpinUp Then Return

        Try

            'Gather data from across all threads
            For Each solver In m_spaceSolvers

                ''Console.WriteLine("SpaceSolver.Solve() ID " & solver.ThreadID.ToString & " CPU time(sec), " & solver.RunTimeSeconds.ToString)
                'cpuTime += solver.RunTimeSeconds
                'cpuTimeCatch += solver.CatchCPUTimeSec

                For igrp As Integer = 1 To m_Data.NGroups
                    m_Data.ResultsByGroup(eSpaceResultsGroups.CatchBio, igrp, itt) += solver.ResultsByGroup(eSpaceResultsGroups.CatchBio, igrp)

                    m_Data.ResultsByGroup(eSpaceResultsGroups.FishingMort, igrp, itt) += solver.ResultsByGroup(eSpaceResultsGroups.FishingMort, igrp)
                    m_Data.ResultsByGroup(eSpaceResultsGroups.ConsumpRate, igrp, itt) += solver.ResultsByGroup(eSpaceResultsGroups.ConsumpRate, igrp)
                    m_Data.ResultsByGroup(eSpaceResultsGroups.PredMortRate, igrp, itt) += solver.ResultsByGroup(eSpaceResultsGroups.PredMortRate, igrp)

                    m_Data.ResultsByGroup(eSpaceResultsGroups.TotalLoss, igrp, itt) += solver.ResultsByGroup(eSpaceResultsGroups.TotalLoss, igrp)

                Next igrp

                For iflt As Integer = 0 To Me.m_Data.nFleets
                    m_Data.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iflt, itt) += solver.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iflt)
                    m_Data.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iflt, itt) += solver.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iflt)
                    m_Data.ResultsByFleet(eSpaceResultsFleets.CatchBio, iflt, itt) += solver.ResultsByFleet(eSpaceResultsFleets.CatchBio, iflt)

                    For igrp As Integer = 1 To m_Data.NGroups
                        m_Data.Landings(igrp, iflt) += solver.Landings(igrp, iflt)
                        m_Data.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, iflt, igrp, itt) += solver.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, iflt, igrp)

                        For irgn As Integer = 0 To m_Data.nRegions
                            m_Data.ResultsCatchRegionGearGroup(irgn, iflt, igrp, itt) += solver.ResultsCatchRegionGearGroup(irgn, iflt, igrp)
                            m_Data.ResultsCatchRegionGearGroupYear(irgn, iflt, igrp, m_Data.YearNow) += solver.ResultsCatchRegionGearGroup(irgn, iflt, igrp)
                        Next irgn

                    Next igrp
                Next iflt

                If m_Data.NewMultiStanza Then
                    For isp As Integer = 1 To m_Stanza.Nsplit
                        For ist As Integer = 1 To m_Stanza.Nstanza(isp)
                            ieco = m_Stanza.EcopathCode(isp, ist)

                            'accumulate information needed to predict mean stanza loss, feeding, IFD weights from derivtred outputs
                            'these arrays are used in the new SpaceSplitUpdate subroutine for predicting mortality
                            'rate and growth rate averages over space by age in that update routine
                            'IFDweight is used to predict proportion of biomass of ieco stanza that will be on cell i,j
                            TotLoss(ieco) = TotLoss(ieco) + solver.TotLossThread(ieco)
                            TotEatenBy(ieco) = TotEatenBy(ieco) + solver.TotEatenByThread(ieco)
                            TotBiom(ieco) = TotBiom(ieco) + solver.TotBiomThread(ieco)
                            TotPred(ieco) = TotPred(ieco) + solver.TotPredThread(ieco)
                            TotIFDweight(ieco) = TotIFDweight(ieco) + solver.TotIFDweightThread(ieco)

                        Next
                    Next
                End If
            Next solver

            'Average values across all the map cells
            For igrp As Integer = 1 To m_Data.NGroups
                m_Data.ResultsByGroup(eSpaceResultsGroups.CatchBio, igrp, itt) /= m_Data.nWaterCells
                m_Data.ResultsByGroup(eSpaceResultsGroups.FishingMort, igrp, itt) /= m_Data.nWaterCells
                m_Data.ResultsByGroup(eSpaceResultsGroups.ConsumpRate, igrp, itt) /= m_Data.nWaterCells
                m_Data.ResultsByGroup(eSpaceResultsGroups.PredMortRate, igrp, itt) /= m_Data.nWaterCells
                'Don't include TotalLoss in the averaging. It's the total loss across the model area, NOT KM2
                'm_Data.ResultsByGroup(eSpaceResultsGroups.TotalLoss, igrp, itt) /= m_Data.nWaterCells

                'HACK
                'Lovley little hack for computing fit to Ecosim time series data SS
                'Ecosim.AccumulateDataInfo(...) Uses Ecosim.FishTime() to calculate catch for the fitting stats
                'So set it to the average F 
                'This shouldn't cause problems because when Ecosim it run FishTime() is calculated on the fly for each time step
                'this value will be over written
                Me.m_SimData.FishTime(igrp) = m_Data.ResultsByGroup(eSpaceResultsGroups.FishingMort, igrp, itt)
            Next igrp

            For iflt As Integer = 0 To Me.m_Data.nFleets
                m_Data.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iflt, itt) /= m_Data.nWaterCells
                m_Data.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iflt, itt) /= m_Data.nWaterCells
                m_Data.ResultsByFleet(eSpaceResultsFleets.CatchBio, iflt, itt) /= m_Data.nWaterCells

                For igrp As Integer = 1 To m_Data.NGroups
                    m_Data.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, iflt, igrp, itt) /= m_Data.nWaterCells

                    Dim nInReg As Integer
                    For irgn As Integer = 0 To m_Data.nRegions
                        nInReg = m_Data.nCellsInRegion(irgn)
                        If nInReg = 0 Then nInReg = 1
                        m_Data.ResultsCatchRegionGearGroup(irgn, iflt, igrp, itt) /= nInReg
                        If ((itt Mod Me.m_Data.NumStep) = 0) Then
                            m_Data.ResultsCatchRegionGearGroupYear(irgn, iflt, igrp, Me.m_Data.YearNow) /= Me.m_Data.NumStep
                        End If
                    Next irgn

                Next igrp
            Next iflt

        Catch ex As Exception
            cLog.Write(ex, "EcoSpace.UpdateEcospaceResults()")
            Debug.Assert(False, "Exception in EcoSpace.UpdateEcospaceResults() " + ex.Message)
        End Try

    End Sub



    ''' <summary>
    ''' Keep the Biomass results for this time step after all the calculation have been done. Trophic (deritRed), Spatial distribution (solve grid) and Multi-stanza biomasses updated.
    ''' </summary>
    ''' <param name="iTimeStep">Current cumulative time step.</param>
    ''' <remarks>Biomass is the average across all the water cells</remarks>
    Private Sub updateBiomassResults(ByVal iTimeStep As Integer)
        Dim igrp As Integer
        Try

            'Don't gather results when in a spin-up period
            ' If Me.m_Data.bInSpinUp Then Return

            For igrp = 1 To m_Data.NGroups

                'biomass
                m_Data.ResultsByGroup(eSpaceResultsGroups.Biomass, igrp, iTimeStep) = Btime(igrp)
                'relative biomass
                m_Data.ResultsByGroup(eSpaceResultsGroups.RelativeBiomass, igrp, iTimeStep) = Btime(igrp) / m_Data.BBase(igrp) ' Me.m_EPdata.B(igrp) ' to use Ecopath base '

            Next igrp

        Catch ex As Exception
            Debug.Assert(False)
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub fireOnTimeStep(ByVal iTime As Integer)

        Try
            If Me.m_TimestepDelegate IsNot Nothing Then
                m_TimestepDelegate(iTime)
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub


    Private Sub marshallOnTimeStep(ByVal iTime As Integer)
        Try
            'No timesteps if in the Spinup period
            If Not Me.m_Data.bInSpinUp Then
                m_SyncObj.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireOnTimeStep), iTime)
            End If
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub


#End Region

#Region "Map Velocities"

    Sub SetXYBoundaryDepths()
        'calculates flow depths at cell faces:depthX is bottom face, depthY is right face of each cell
        Dim i As Integer, j As Integer

        ReDim m_Data.DepthX(m_Data.InRow, m_Data.InCol)
        ReDim m_Data.DepthY(m_Data.InRow, m_Data.InCol)

        For i = 0 To m_Data.InRow
            For j = 0 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    If m_Data.Depth(i + 1, j) > 0 Then
                        If m_Data.DepthA(i + 1, j) > m_Data.DepthA(i, j) Then
                            m_Data.DepthY(i, j) = m_Data.DepthA(i, j)
                        Else
                            m_Data.DepthY(i, j) = m_Data.DepthA(i + 1, j)
                        End If
                    End If 'If m_Data.Depth(i + 1, j) > 0 Then

                    If m_Data.Depth(i, j + 1) > 0 Then
                        If m_Data.DepthA(i, j + 1) > m_Data.DepthA(i, j) Then
                            m_Data.DepthX(i, j) = m_Data.DepthA(i, j)
                        Else
                            m_Data.DepthX(i, j) = m_Data.DepthA(i, j + 1)
                        End If
                    End If ' If m_Data.Depth(i, j + 1) > 0 Then

                End If ' If m_Data.Depth(i, j) > 0 Then
            Next j
        Next i

        ReDim m_Data.Xvel(m_Data.InRow + 1, m_Data.InCol + 1)
        ReDim m_Data.Yvel(m_Data.InRow + 1, m_Data.InCol + 1)
        For i = 0 To m_Data.InRow + 1
            For j = 0 To m_Data.InCol + 1
                If m_Data.Depth(i, j) > 0 Then
                    m_Data.Xvel(i, j) = m_Data.Xvloc(i, j)
                    m_Data.Yvel(i, j) = m_Data.Yvloc(i, j)
                End If
            Next j
        Next i

    End Sub



    Private Sub readAdvectFile()
        ''Read in Advection Field data.  SM, Jan 7, 2003
        ''Used for reading in Advection field data.
        'Dim i As Integer, j As Integer
        'Dim F$
        ''F$ = DispCommonDlg(7, frmMdiEcopath4.dlgFileAccess, F$)
        'If DispCommonDlg(7, frmMdiEcopath4.dlgFileAccess, F$) Then
        '    'Stop
        '    m_Data.CurrentForce = True
        '    'velmaker.ReadVelFields F$
        '    NewReadVelFields(F$)
        'End If
        Try
            Dim d As New System.Windows.Forms.OpenFileDialog
            Dim sr As System.IO.TextReader

            If d.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                sr = New System.IO.StreamReader(d.FileName)

                Dim i As Integer, j As Integer, InrowRead As Integer, IncolRead As Integer
                Dim Xvl As Single, Yvl As Single, Xvv As Single, Yvv As Single, Vxp As Single, Vyp As Single, Upv As Single, Dep As Single

                InrowRead = cFileUtils.ReadNumber(sr)
                IncolRead = cFileUtils.ReadNumber(sr)

                If InrowRead <> m_Data.InRow Or IncolRead <> m_Data.InCol Then
                    'vbYesNo is not Mon compatible
                    'If MsgBox("Number of rows and columns in this advection file are not the same as your current map; try to read anyway?", vbYesNo) = vbNo Then Exit Sub
                End If

                Vxp = cFileUtils.ReadNumber(sr)
                Vyp = cFileUtils.ReadNumber(sr)

                For i = 0 To InrowRead + 1
                    For j = 0 To IncolRead + 1
                        Xvl = cFileUtils.ReadNumber(sr)
                        Yvl = cFileUtils.ReadNumber(sr)
                        Xvv = cFileUtils.ReadNumber(sr)
                        Yvv = cFileUtils.ReadNumber(sr)
                        Upv = cFileUtils.ReadNumber(sr)
                        Dep = cFileUtils.ReadNumber(sr)
                        If i <= m_Data.InRow + 1 And j <= m_Data.InCol + 1 Then
                            m_Data.Xvloc(i, j) = Xvl
                            m_Data.Yvloc(i, j) = Yvl
                            m_Data.Xvel(i, j) = Xvv
                            m_Data.Yvel(i, j) = Yvv
                            m_Data.UpVel(i, j) = Upv
                            m_Data.DepthA(i, j) = Dep
                        End If
                    Next
                Next

            End If
        Catch ex As Exception
            Debug.Assert(False, "Reading advection file failed - " + ex.Message)
        End Try

    End Sub

#End Region

#Region "Multi Threading stuff"
    ' this creates a solver object for each thread and initialises them with ecospace data
    Private Function InitGridSolverThreads() As Boolean
        Dim solver As cGridSolver

        Try
            If m_gridSolvers Is Nothing Then
                m_gridSolvers = New List(Of cGridSolver)
            Else
                m_gridSolvers.Clear()
            End If

            For i As Integer = 1 To m_Data.nGridSolverThreads
                If nGroupsInThread(i) > 0 Then
                    solver = New cGridSolver(i)
                    'solver.bUseLocalMemory = True 'Me.m_Data.bUseLocalMemory

                    solver.Init(AMm, F, m_Data.Bcell, m_Data.InRow, m_Data.InCol, m_Data.Tol, m_Data.jord, m_Data.W, Bcw, C, d, e, m_Data.Depth, _
                                m_Data.ByPassIntegrate, m_Data.iStartRow, m_Data.iEndRow, m_Data.TimeStep, m_Data.maxIter, m_Data.jStartCol, m_Data.jEndCol, _
                                m_Data.IsMigratory, threadGroups, m_Data.UseExact)
                    m_gridSolvers.Add(solver)
                End If
            Next i

            Return True

        Catch ex As Exception

            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException(Me.ToString & ".InitGridSolverThreads() Error:  " & ex.Message, ex)

        End Try


    End Function

    ''' <summary>
    ''' Creates a spacesolver object for each thread, and initialises them with references to ecospace variables
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function InitSpaceSolverThreads() As Boolean
        Dim solver As cSpaceSolver

        Try
            If m_spaceSolvers Is Nothing Then
                m_spaceSolvers = New List(Of cSpaceSolver)
            Else
                m_spaceSolvers.Clear()
            End If

            For i As Integer = 1 To m_Data.nSpaceSolverThreads
                solver = New cSpaceSolver(i)

                'set reference variables
                solver.m_EcospaceModel = Me
                solver.m_Data = m_Data
                solver.m_SimData = m_SimData
                solver.m_PathData = m_EPdata
                solver.m_Stanza = m_Stanza
                solver.m_Ecosim = m_Ecosim

                ' solver.syncCopyLock = Me.CopySyncLock
                'solver.bUseLocalMemory = Me.m_Data.bUseLocalMemory

                'copy tracer data into each thread
                'this way each thread gets its own copy of the data that has been initialized by the database
                m_tracerData.CopyTo(solver.m_TracerData)

                solver.Search = m_search
                solver.Bcw = Bcw
                solver.C = C
                solver.d = d
                solver.e = e
                solver.BEQLast = BEQlast
                solver.Btime = Btime
                solver.F = F
                solver.AMm = AMm
                solver.Ecode = Ecode
                solver.HdenCell = HdenCell
                solver.RelFitness = RelFitness
                solver.FtimeCell = FtimeCell
                solver.Cper = Cper
                solver.PconSplit = PconSplit
                solver.RelRepStanza = RelRepStanza
                solver.Tstanza = Tstanza
                solver.PbSpace = PbSpace

                'needs to be set from ecospace, but not references
                ' solver.Tn = Tn
                solver.nvar2 = nvar2
                solver.itt = itt 'itimestep index to data stored by month
                solver.PPScale = Me.m_Data.PPScale
                solver.TimeStep2 = m_Data.TimeStep / 2
                solver.MinChange = MinChange

                'solver.bUseLocalMemory = Me.m_Data.bUseLocalMemory
                'EcoFunction and Lock object used to calculate trophic level 
                'in calTrophicLevel()
                solver.EcoFunctions = Me.EcoFunctions
                solver.TLlockOb = Me.m_TLlockOb
                solver.Init()

                'solver.EcospaceErrorHandler = AddressOf Me.SolverErrorHandler

                m_spaceSolvers.Add(solver)
            Next i

            Return True

        Catch ex As Exception

            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException(Me.ToString & ".InitSpaceSolverThreads() Error:  " & ex.Message, ex)

        End Try


    End Function


    Private Sub InitSolversForYear(ByVal iYear As Integer)
        Try
            For Each solver As cSpaceSolver In Me.m_spaceSolvers
                solver.YearTimeStep(iYear)
                'discount factor was computed in the main time loop
            Next
        Catch ex As Exception
            cLog.Write(ex)
            Throw New ApplicationException(Me.ToString & ".InitForYear() Error:  " & ex.Message, ex)
        End Try

    End Sub

    ''' <summary>
    ''' Creates a spacesolver object for each thread, and initialises them with references to ecospace variables
    ''' </summary>
    ''' <returns>True if successful.</returns>
    Private Function InitIBMSolverThreads() As Boolean
        Dim solver As cIBMSolver

        Try
            If m_IBMSolvers Is Nothing Then
                m_IBMSolvers = New List(Of cIBMSolver)
            Else
                m_IBMSolvers.Clear()
            End If

            For i As Integer = 1 To m_Data.nGridSolverThreads
                solver = New cIBMSolver(i)

                'set reference variables
                solver.m_EcospaceModel = Me
                solver.m_Data = m_Data
                solver.m_ESData = m_SimData
                solver.m_Stanza = m_Stanza
                solver.m_Ecosim = m_Ecosim
                solver.Bcw = Bcw
                solver.C = C
                solver.d = d
                solver.e = e
                solver.Cper = Cper

                solver.Init()

                solver.EcospaceErrorHandler = AddressOf Me.SolverErrorHandler

                m_IBMSolvers.Add(solver)
            Next i

            Return True

        Catch ex As Exception

            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException(Me.ToString & ".InitIBMSolverThreads() Error:  " & ex.Message, ex)

        End Try


    End Function


    Private Sub SolverErrorHandler(ByVal ThreadID As Integer, ByVal msg As String)
        m_solverErrorMsg = msg
        m_solverErrorID = ThreadID
        Me.m_bsolverError = True
        System.Console.WriteLine("Error in Ecospace solver thread ID " & ThreadID.ToString & " " & msg)
    End Sub

    ''' <summary>
    ''' This iterates over the list of space solvers, and initializes them with variables calculated by Ecospace, at each time step
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function UpdateSpaceSolverThreads(ByVal Year As Integer) As Boolean

        Dim solver As cSpaceSolver
        Try

            For Each solver In m_spaceSolvers
                solver.nvar2 = nvar2
                solver.itt = itt 'itimestep index to data stored by month
                solver.PPScale = Me.m_Data.PPScale
                solver.TimeStep2 = m_Data.TimeStep / 2
                solver.MinChange = MinChange
                solver.Btime = Btime
                solver.iYear = Year
            Next

            Return True

        Catch ex As Exception

            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException(Me.ToString & ".UpdateGridSolverThreads() Error:  " & ex.Message, ex)

        End Try


    End Function

    Private Function Get2DArray(ByVal startIndex As Integer, ByVal endIndex As Integer, ByRef X As Single(,), ByVal nRows As Integer, ByVal nCols As Integer) As Single(,)
        Dim iStart As Integer
        Dim jStart As Integer
        Dim iEnd As Integer
        Dim jNew As Integer
        Dim iNew As Integer
        Dim iOld As Integer
        Dim jOld As Integer
        Dim newX(,) As Single

        iStart = (startIndex - 1) \ nCols + 1
        jStart = (startIndex - 1) Mod nCols + 1
        iEnd = (endIndex - 1) \ nCols + 1

        ReDim newX(iEnd - iStart + 1, nCols)

        For k As Integer = 1 To endIndex - startIndex + 1

            jNew = (k + jStart - 2) Mod nCols + 1
            iNew = (k + jStart - 2) \ nCols + 1
            iOld = (k + jStart - 2) \ nCols + iStart
            jOld = jNew

            newX(iNew, jNew) = X(iOld, jOld)

        Next

        Return newX

    End Function

    Private Function Get3DArray(ByVal startIndex As Integer, ByVal endIndex As Integer, ByRef X As Single(,,), ByVal nRows As Integer, ByVal nCols As Integer, ByVal nGroups As Integer) As Single(,,)
        Dim iStart As Integer
        Dim jStart As Integer
        Dim iEnd As Integer
        Dim jNew As Integer
        Dim iNew As Integer
        Dim iOld As Integer
        Dim jOld As Integer
        Dim newX(,,) As Single

        iStart = (startIndex - 1) \ nCols + 1
        jStart = (startIndex - 1) Mod nCols + 1
        iEnd = (endIndex - 1) \ nCols + 1

        ReDim newX(iEnd - iStart + 1, nCols, nGroups)

        For k As Integer = 1 To endIndex - startIndex + 1

            jNew = (k + jStart - 2) Mod nCols + 1
            iNew = (k + jStart - 2) \ nCols + 1
            iOld = (k + jStart - 2) \ nCols + iStart
            jOld = jNew

            For ip As Integer = 1 To nGroups
                newX(iNew, jNew, ip) = X(iOld, jOld, ip)
            Next
        Next

        Return newX

    End Function
    Private Function Get2DArray(ByVal startIndex As Integer, ByVal endIndex As Integer, ByRef X As Integer(,), ByVal nRows As Integer, ByVal nCols As Integer) As Integer(,)
        Dim iStart As Integer
        Dim jStart As Integer
        Dim iEnd As Integer
        Dim jNew As Integer
        Dim iNew As Integer
        Dim iOld As Integer
        Dim jOld As Integer
        Dim newX(,) As Integer

        iStart = (startIndex - 1) \ nCols + 1
        jStart = (startIndex - 1) Mod nCols + 1
        iEnd = (endIndex - 1) \ nCols + 1

        ReDim newX(iEnd - iStart + 1, nCols)

        For k As Integer = 1 To endIndex - startIndex + 1

            jNew = (k + jStart - 2) Mod nCols + 1
            iNew = (k + jStart - 2) \ nCols + 1
            iOld = (k + jStart - 2) \ nCols + iStart
            jOld = jNew

            newX(iNew, jNew) = X(iOld, jOld)

        Next

        Return newX

    End Function

    Private Function Get3DArray(ByVal startIndex As Integer, ByVal endIndex As Integer, ByRef X As Integer(,,), ByVal nRows As Integer, ByVal nCols As Integer, ByVal nGroups As Integer) As Integer(,,)
        Dim iStart As Integer
        Dim jStart As Integer
        Dim iEnd As Integer
        Dim jNew As Integer
        Dim iNew As Integer
        Dim iOld As Integer
        Dim jOld As Integer
        Dim newX(,,) As Integer

        iStart = (startIndex - 1) \ nCols + 1
        jStart = (startIndex - 1) Mod nCols + 1
        iEnd = (endIndex - 1) \ nCols + 1

        ReDim newX(iEnd - iStart + 1, nCols, nGroups)

        For k As Integer = 1 To endIndex - startIndex + 1

            jNew = (k + jStart - 2) Mod nCols + 1
            iNew = (k + jStart - 2) \ nCols + 1
            iOld = (k + jStart - 2) \ nCols + iStart
            jOld = jNew

            For ip As Integer = 1 To nGroups
                newX(iNew, jNew, ip) = X(iOld, jOld, ip)
            Next
        Next

        Return newX

    End Function


#End Region

#Region "Summary stats"

    Public Function CalculateSpaceSS() As Single
        '        'accumulates statistical information for comparing model to data
        '        'for simulation years 0=first simulation year)
        '        'assumes first simulation year is first calendar year in data csv file
        'On Local Error GoTo exitSub
        'Dim i As Long, j As Long, iDyear As Integer, Zstat As Single
        Dim Erpred() As Single
        Dim Ss As Single
        'Dim Cnt As Long
        'Dim bCountStat As Boolean

        Dim SpNObs() As Integer
        Dim SpSumZ() As Single
        Dim SpSumZ2() As Single


        ReDim Erpred(m_refdata.NdatType * m_refdata.nDatPoints)
        '  ReDim ErTrace(m_refdata.NdatType * m_refdata.NdatYear)
        'ReDim SpTraceObs(SpDat)
        'ReDim SpTraceZ(SpDat)
        'ReDim SpTraceZ2(SpDat)

        ReDim SpNObs(m_refdata.NdatType)
        ReDim SpSumZ(m_refdata.NdatType)
        ReDim SpSumZ2(m_refdata.NdatType)

        'ReDim SpeDatq(SpDat)
        'Dim m_refdata.Iobs As Long
        ''SS Calculations for Ecospace
        ''Timeseries values are stored for each year in variables below; 0 is used for the first year:
        ''ReDim SpaceBiomassByRegion(totalTime, NumGroups, NoRegions)
        ''ReDim SpaceCatchByRegion(totalTime, NumGroups, NoRegions)
        'ReDim ObsPred(2, 0)
        'ReDim ObsName(0)
        'ReDim ObsUse(0)
        'm_refdata.Iobs = 0
        ''SpTraceObs = 0
        'Accumulate z statistics for observations:
        'For j = 1 To m_refdata.NdatType  'This time series is for region: m_refdata.spregion(j)
        '    For iDyear = 1 To m_Data.TotalTime '- 1
        '        If m_refdata.DatVal(iDyear, j) > 0 And _
        '            (m_refdata.DatType(j) = eTimeSeriesType.BiomassAbs Or m_refdata.DatType(j) = eTimeSeriesType.BiomassRel Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.Catches Or m_refdata.DatType(j) = eTimeSeriesType.CatchesForcing Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.FishingEffort) Then

        '            bCountStat = False

        '            'SWt(m_refdata.Iobs) = SpWt(j)
        '            Select Case m_refdata.DatType(j)
        '                Case eTimeSeriesType.BiomassAbs, eTimeSeriesType.BiomassRel 'Abundance Data
        '                    If m_Data.SpaceBiomassByRegionCount(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)) > 0 Then
        '                        Zstat = Math.Log(m_refdata.DatVal(iDyear, j) / m_Data.SpaceBiomassByRegion(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)) * m_Data.SpaceBiomassByRegionCount(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)))          'BB(DatPool(j)))
        '                        bCountStat = True
        '                    End If

        '                Case eTimeSeriesType.FishingEffort '3  'Effort data, In Ecospace only used for comparison, not to drive effort
        '                    If m_Data.SpaceEffortByRegionFleetCount(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)) > 0 Then
        '                        Zstat = Math.Log(m_refdata.DatVal(iDyear, j) / m_Data.SpaceEffortByRegionFleet(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)) * m_Data.SpaceEffortByRegionFleetCount(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)))          'BB(DatPool(j)))
        '                        bCountStat = True
        '                    End If

        '                Case eTimeSeriesType.Catches, eTimeSeriesType.CatchesForcing '-6, 6     'Absolute Catch Data, Martell, Jan 02
        '                    If m_Data.SpaceCatchByRegion(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)) > 0 Then
        '                        Zstat = Math.Log(m_refdata.DatVal(iDyear, j) / m_Data.SpaceCatchByRegion(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)) * m_Data.SpaceCatchByRegionCount(iDyear, m_refdata.DatPool(j), m_refdata.SPRegion(j)))
        '                        bCountStat = True
        '                    End If

        '                Case Else
        '            End Select

        '            If bCountStat Then
        '                m_refdata.Iobs = m_refdata.Iobs + 1

        '                Erpred(m_refdata.Iobs) = Zstat
        '                SpNObs(j) = SpNObs(j) + 1
        '                SpSumZ(j) = SpSumZ(j) + Zstat
        '                SpSumZ2(j) = SpSumZ2(j) + Zstat * Zstat
        '            End If

        '            'ElseIf ConSimOn And m_refdata.datVal(iDyear, j) > 0 And (m_refdata.datType(j) = 8 Or m_refdata.datType(j) = 9) Then
        '            '    'This is to add reference time series for Ecotracer runs
        '            '    If ConcTr(m_refdata.datpool(j)) > 0 Then
        '            '        'NobsTime(iDyear) = NobsTime(iDyear) + 1
        '            '        'NobsTime is for testing significance, don't need a similar one for tracer, for now at least
        '            '        'Wt(m_refdata.Iobs) = WtType(j)
        '            '        If SpaceTraceByRegionCount(iDyear, m_refdata.datpool(j), m_refdata.spregion(j)) > 0 Then
        '            '            Zstat = Log(m_refdata.datVal(iDyear, j) / SpaceTraceByRegion(iDyear, m_refdata.datpool(j), m_refdata.spregion(j)) * SpaceTraceByRegionCount(iDyear, m_refdata.datpool(j), m_refdata.spregion(j)))          'BB(DatPool(j)))
        '            '            Cnt = Cnt + 1
        '            '            ReDim Preserve ObsPred(2, Cnt)
        '            '            ReDim Preserve ObsName(Cnt)
        '            '            ReDim Preserve ObsUse(Cnt)
        '            '            ObsPred(0, Cnt) = m_refdata.datVal(iDyear, j)
        '            '            ObsName(Cnt) = SpName(j)   'name of the series
        '            '            ObsUse(Cnt) = True
        '            '            ObsPred(1, Cnt) = SpaceTraceByRegion(iDyear, m_refdata.datpool(j), m_refdata.spregion(j)) / SpaceTraceByRegionCount(iDyear, m_refdata.datpool(j), m_refdata.spregion(j))
        '            '            ObsPred(2, Cnt) = m_refdata.spregion(j)
        '            '            'ObsPred(1, Cnt) = ObsPred(0, Cnt) * (1 + Rnd())
        '            '            'save the biggest values for scaling
        '            '            If ObsPred(0, Cnt) > ObsPred(0, 0) Then ObsPred(0, 0) = ObsPred(0, Cnt)
        '            '            If ObsPred(1, Cnt) > ObsPred(1, 0) Then ObsPred(1, 0) = ObsPred(1, Cnt)
        '            '        End If
        '            '        'YTraceHat(m_refdata.Iobs) = Log(ConcTr(DatPool(j)))
        '            '        ErTrace(TraceObs) = Zstat
        '            '        SpTraceObs(j) = SpTraceObs(j) + 1
        '            '        SpTraceZ(j) = SpTraceZ(j) + Zstat
        '            '        SpTraceZ2(j) = SpTraceZ2(j) + Zstat * Zstat
        '            '    End If
        '        End If
        '    Next
        'Next j

        ''        '-----------------------------------
        ''ReDim m_refdata.DatSS(SpDat) As Single
        ''ReDim m_refdata.DatQ(SpDat) As Single

        'For j = 1 To m_refdata.NdatType
        '    If SpNObs(j) > 0 Then
        '        If m_refdata.DatType(j) = eTimeSeriesType.BiomassAbs Then
        '            m_refdata.DatSS(j) = SpSumZ2(j)
        '            m_refdata.DatQ(j) = 0

        '        ElseIf m_refdata.DatType(j) = eTimeSeriesType.BiomassRel _
        '            Or m_refdata.DatType(j) = eTimeSeriesType.FishingEffort Or _
        '               m_refdata.DatType(j) = eTimeSeriesType.TotalMortality Or _
        '               m_refdata.DatType(j) = eTimeSeriesType.Catches Or _
        '               m_refdata.DatType(j) = eTimeSeriesType.CatchesForcing Or _
        '               m_refdata.DatType(j) = eTimeSeriesType.AverageWeight Then 'added mean body wieght here

        '            m_refdata.DatSS(j) = SpSumZ2(j) - SpSumZ(j) ^ 2 / SpNObs(j)
        '            m_refdata.DatQ(j) = SpSumZ(j) / SpNObs(j)
        '            ' SpeDatq(j) = Exp(m_refdata.DatQ(j))
        '        End If
        '    End If
        'Next

        'm_refdata.Iobs = 0
        'Ss = 0

        'For i = 1 To m_refdata.NdatYear
        '    iDyear = m_refdata.DatYear(i) - m_refdata.DatYear(1)
        '    For j = 1 To m_refdata.NdatType
        '        If m_refdata.DatVal(i, j) > 0 And iDyear < m_Data.TotalTime + 1 And _
        '            (m_refdata.DatType(j) = eTimeSeriesType.BiomassRel Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.BiomassAbs Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.FishingEffort Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.TotalMortality Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.Catches Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.CatchesForcing Or _
        '             m_refdata.DatType(j) = eTimeSeriesType.AverageWeight) Then

        '            m_refdata.Iobs = m_refdata.Iobs + 1
        '            Erpred(m_refdata.Iobs) = Erpred(m_refdata.Iobs) - m_refdata.DatQ(j)
        '            Ss = Ss + Erpred(m_refdata.Iobs) ^ 2 ' * Wt(m_refdata.Iobs)
        '        End If

        '    Next
        'Next


        ''        '--------------------------------------------------
        ''        'Trace SS Spatial:
        ''        TraceSS = 0
        ''        If ConSimOn Then
        ''    ReDim tDatSS(SpDat) As Single, tDatq(SpDat) As Single, teDatq(SpDat) As Single
        ''            'Dim i As Integer, j As Integer, iYear As Integer, bplot As Single

        ''            For j = 1 To SpDat
        ''                If SpTraceObs(j) > 0 Then
        ''                    If m_refdata.datType(j) = 9 Then      'absolute concentration
        ''                        tDatSS(j) = SpTraceZ2(j)
        ''                        tDatq(j) = 0
        ''                    ElseIf m_refdata.datType(j) = 8 Then  'relative concentration
        ''                        tDatSS(j) = SpTraceZ2(j) - SpTraceZ(j) ^ 2 / SpTraceObs(j)
        ''                        tDatq(j) = SpTraceZ(j) / SpTraceObs(j)
        ''                        teDatq(j) = Exp(tDatq(j))
        ''                    End If
        ''                End If
        ''            Next

        ''            TraceObs = 0
        ''            'Ss = 0

        ''            For i = 1 To SpDatYear
        ''                iDyear = SpYear(i) - SpYear(1)
        ''                For j = 1 To SpDat
        ''                    If (m_refdata.datType(j) = 8 Or m_refdata.datType(j) = 9) And Abs(m_refdata.datVal(i, j)) > 0 And iDyear < TotalTime + 1 Then
        ''                        TraceObs = TraceObs + 1
        ''                        ErTrace(TraceObs) = ErTrace(TraceObs) - tDatq(j)
        ''                        'DatDev(j, i) = ErTrace(TraceObs)
        ''                        TraceSS = TraceSS + ErTrace(TraceObs) ^ 2 '* Wt(m_refdata.Iobs) 'No weight on trace
        ''                        'YTraceHat(TraceObs) = YTraceHat(TraceObs) + Datq(j)
        ''                    End If

        ''                Next
        ''            Next
        ''        End If
        ''        '===================================================

        ''        'As a start using SS not LL
        ''        Dim LogL As Single
        ''        For j = 1 To SpDat
        ''            If m_refdata.DatSS(j) > 0 Then LogL = LogL + SpWt(j) * (SpNObs(j) - 1) * Log(m_refdata.DatSS(j))
        ''        Next
        ''        LogL = LogL / 2
        ''        If SetToLike = True Then Ss = LogL

        Return Ss

        'exitSub:
        '        CalculateSpaceSS = Ss
    End Function


    Public Sub RedimSpaceCSVvariables()
        'Ecosim name    Ecospace name

        'NdatType       SpDat
        'DatName        SpName()
        'DatPool        SpPool()
        'DatType        SpType()
        'WtType         SpWt()
        'DatVal         SpVal()
        'DatYear        SpYear()
        'PoolForceBB    SpForceBB()
        'PoolForceCatch SpForceCatch()
        'PoolForceZ     SpForceZ()
        'IsDatShown     IsSpShown()
        '               SpRegion()
        '(the next ones are dimensioned elsewhere)
        'DatSumZ()      SpSumZ()
        'DatSumZ2()     SpSumZ2()
        'DatNobs()      SpNobs()
        'DatSS()        SpSS()
        'DatTraceObs()  SpTraceObc()
        'DatTraceZ()    SpTraceZ()
        'DatTraceZ2()   SpTraceZ2()
        'Datq()         SpDatq()
        'eDatq()        SpeDatq()

        ReDim m_Data.SpName(m_Data.SpDat)
        ReDim m_Data.SpPool(m_Data.SpDat)
        ReDim m_Data.SpType(m_Data.SpDat)
        ReDim m_Data.SpWt(m_Data.SpDat)
        ReDim m_Data.SpVal(m_Data.SpDatYear + 1, m_Data.SpDat)
        ReDim m_Data.SpYear(m_Data.SpDatYear)
        ReDim m_Data.SpForceBB(m_Data.NGroups, m_Data.SpDatYear)
        ReDim m_Data.SpForceCatch(m_Data.NGroups, m_Data.SpDatYear)
        ReDim m_Data.SpForceZ(m_Data.NGroups, m_Data.SpDatYear)
        ReDim m_Data.IsSpShown(m_Data.SpDat)
        ReDim m_Data.SpRegion(m_Data.SpDat)
    End Sub



#End Region

#Region " New Multistanza Stuff"

    ''' <summary>
    '''  updates numbers, weight, and biomass for multiple stanza species using information 
    '''  on average performance (eatenby, loss) over ecospace grid cells used by the species
    ''' </summary>
    ''' <remarks></remarks>
    Sub SpaceSplitUpdate()
        'updates numbers, weight, and biomass for multiple stanza species using information
        'on average performance (eatenby, loss) over ecospace grid cells used by the species
        Dim isp As Integer, ist As Integer, ieco As Integer, ia As Integer
        Dim Su As Single, Gf As Single, Nt As Single
        Dim Agemax As Integer, AgeMin As Integer, Be As Single

        For isp = 1 To m_Stanza.Nsplit
            'update numbers and body weights
            ieco = m_Stanza.EcopathCode(isp, m_Stanza.Nstanza(isp))
            If m_Ecosim.ResetPred(ieco) = False Then
                ' Debug.Assert((m_SimData.tval(m_Stanza.EggProdShapeSplit(isp)) <= 1))
                Be = 0
                For ist = 1 To m_Stanza.Nstanza(isp)
                    ieco = m_Stanza.EcopathCode(isp, ist)
                    TotLoss(ieco) = TotLoss(ieco) / TotIFDweight(ieco)
                    TotEatenBy(ieco) = TotEatenBy(ieco) / TotIFDweight(ieco)
                    TotPred(ieco) = TotPred(ieco) / TotIFDweight(ieco)
                    TotBiom(ieco) = TotBiom(ieco) / TotIFDweight(ieco)

                    Su = Math.Exp(-TotLoss(ieco) / 12.0# / TotBiom(ieco))
                    Gf = TotEatenBy(ieco) / TotPred(ieco)  '(month factor here included in splitalpha scaling setup)

                    For ia = m_Stanza.Age1(isp, ist) To m_Stanza.Age2(isp, ist)
                        m_Stanza.NageS(isp, ia) = m_Stanza.NageS(isp, ia) * Su
                        m_Stanza.WageS(isp, ia) = m_Stanza.vBM(isp) * m_Stanza.WageS(isp, ia) + Gf * m_Stanza.SplitAlpha(isp, ia)
                        If m_Stanza.FixedFecundity(isp) Then
                            Be = Be + m_Stanza.NageS(isp, ia) * m_Stanza.EggsSplit(isp, ia)
                        Else
                            If m_Stanza.WageS(isp, ia) > m_Stanza.WmatWinf(isp) Then Be = Be + m_Stanza.NageS(isp, ia) * (m_Stanza.WageS(isp, ia) - m_Stanza.WmatWinf(isp))
                        End If
                    Next
                Next
                m_Stanza.WageS(isp, m_Stanza.Age2(isp, m_Stanza.Nstanza(isp))) = (Su * m_Ecosim.AhatStanza(isp) + (1 - Su) * m_Stanza.WageS(isp, m_Stanza.Age2(isp, m_Stanza.Nstanza(isp)) - 1)) / (1 - m_Ecosim.RhatStanza(isp) * Su)
                m_Stanza.EggsStanza(isp) = Be
                'WageS(iSp, 0) = 0
                'update ages looping backward over age
                For ist = m_Stanza.Nstanza(isp) To 1 Step -1
                    Agemax = m_Stanza.Age2(isp, ist)
                    If ist > 1 Then AgeMin = m_Stanza.Age1(isp, ist) Else AgeMin = 1
                    If ist = m_Stanza.Nstanza(isp) Then
                        Nt = m_Stanza.NageS(isp, Agemax) + m_Stanza.NageS(isp, Agemax - 1)
                        If Nt = 0 Then Nt = 1.0E-30 'watch for zero numbers of older animals
                        'WageS(isp, Agemax) = (WageS(isp, Agemax) * NageS(isp, Agemax) + WageS(isp, Agemax - 1) * NageS(isp, Agemax - 1)) / Nt
                        m_Stanza.NageS(isp, Agemax) = Nt
                        Agemax = Agemax - 1
                    End If
                    For ia = Agemax To AgeMin Step -1
                        m_Stanza.NageS(isp, ia) = m_Stanza.NageS(isp, ia - 1)
                        m_Stanza.WageS(isp, ia) = m_Stanza.WageS(isp, ia - 1)
                    Next
                    ieco = m_Stanza.EcopathCode(isp, ist)
                    If ist < m_Stanza.Nstanza(isp) Then m_Ecosim.Brec(ieco) = m_Stanza.NageS(isp, m_Stanza.Age2(isp, ist) + 1) * m_Stanza.WageS(isp, m_Stanza.Age2(isp, ist) + 1)
                Next
                'finally set abundance at youngest age to recruitment rate
                ieco = m_Stanza.EcopathCode(isp, m_Stanza.Nstanza(isp)) 'code for adult biomass for sp isp
                'VILLY: note following assumes we extend pair list for egg prod and recpower to add multistanza options  at end of pair lists
                m_Ecosim.Srec(ieco) = m_EPdata.B(ieco)
                If m_Stanza.BaseEggsStanza(isp) > 0 Then

                    m_Stanza.NageS(isp, m_Stanza.Age1(isp, 1)) = m_Stanza.RscaleSplit(isp) * m_SimData.tval(m_Stanza.EggProdShapeSplit(isp)) * m_Stanza.RzeroS(isp) * m_SimData.tval(m_Stanza.HatchCode(isp))
                End If
                If m_Stanza.HatchCode(isp) = 0 Then m_Stanza.NageS(isp, m_Stanza.Age1(isp, 1)) = m_Stanza.NageS(isp, m_Stanza.Age1(isp, 1)) * (m_Stanza.EggsStanza(isp) / m_Stanza.BaseEggsStanza(isp)) ^ m_Stanza.RecPowerSplit(isp)
                m_Stanza.WageS(isp, m_Stanza.Age1(isp, 1)) = 0
            End If
        Next
        ' finally update bioamss and pred index information for all species
        m_Ecosim.SplitSetPred(Blocal)
        'this changes Blocal

    End Sub

    ''' <summary>
    ''' Wraps the initialization of Ecospace IBM 
    ''' </summary>
    ''' <remarks>Initialize IMB packet numbers, weights, and positions</remarks>
    Private Sub InitIBM()
        Try
            If Me.m_Data.UseIBM Then
                'Packet number weights and iPacket, jPacket positions
                InitPackets()
                '
                SetNearestOKcellforIBM()
            End If

            m_Data.nIBMPacketsPerThread = (m_Stanza.Npackets + m_Data.nGridSolverThreads - 1) \ m_Data.nGridSolverThreads

        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Initialize numbers, weights, and positions ipacket,jpacket for IBM representation
    ''' note must be called in findspatialequilibrium after calls to initialize ecospace
    ''' variables
    ''' </summary>
    ''' <remarks></remarks>
    Sub InitPackets()

        Dim ia As Integer, isp As Integer, ist As Integer, iaa As Integer
        Dim ip As Integer, i As Integer, j As Integer, Nused As Integer, i1 As Integer
        Dim iList() As Integer, Jlist() As Integer, ieco As Integer, isc As Integer
        ReDim iList(m_Data.ThabArea), Jlist(m_Data.ThabArea), m_Stanza.iNursery(m_Stanza.Nsplit, m_Data.ThabArea), m_Stanza.jNursery(m_Stanza.Nsplit, m_Data.ThabArea)
        ReDim m_Stanza.IBMMovesPerMonth(m_Data.NGroups)
        ReDim m_Stanza.IBMdistmove(m_Stanza.Nsplit, m_Stanza.MaxAgeSplit)
        ReDim m_Data.PredCell(m_Data.InRow, m_Data.InCol, m_Data.NGroups)
        ReDim m_Stanza.Nnursery(m_Stanza.Nsplit), m_Stanza.StanzaNo(m_Stanza.Nsplit, m_Stanza.MaxAgeSplit)
        ReDim m_Stanza.MaxAgeSpecies(m_Stanza.Nsplit), m_Stanza.AgeIndex1(m_Stanza.Nsplit)
        'ReDim Cper(m_Data.Inrow, m_Data.InCol, m_Data.NGroups)

        'set number of packets per age **** to interface?****
        m_Stanza.Npackets = m_Data.InRow * m_Data.InCol * m_Stanza.NPacketsMultiplier

        ReDim m_Stanza.Npacket(m_Stanza.Nsplit, m_Stanza.MaxAgeSplit, m_Stanza.Npackets)
        ReDim m_Stanza.Wpacket(m_Stanza.Nsplit, m_Stanza.MaxAgeSplit, m_Stanza.Npackets)
        ReDim m_Stanza.iPacket(m_Stanza.Nsplit, m_Stanza.MaxAgeSplit, m_Stanza.Npackets)
        ReDim m_Stanza.jPacket(m_Stanza.Nsplit, m_Stanza.MaxAgeSplit, m_Stanza.Npackets)

        Dim cellsPerMonth As Single, Dmove As Single

        'set up pointer array to stanza number for each fish age
        'and set up initial numbers and weights by packet and age
        'and set initial packet positions on map grid
        'note assumes calls to initialize multistanza Nages,Wages,pred(ieco) have been made already
        isc = 0
        For isp = 1 To m_Stanza.Nsplit
            ia = -1
            m_Stanza.AgeIndex1(isp) = 0
            For ist = 1 To m_Stanza.Nstanza(isp)
                ieco = m_Stanza.EcopathCode(isp, ist)
                cellsPerMonth = m_Data.Mvel(ieco) / (12 * m_Data.CellLength)
                If cellsPerMonth >= 0.5 Then
                    m_Stanza.IBMMovesPerMonth(ieco) = cellsPerMonth / 0.5 '+1
                    Dmove = 0.5
                Else
                    m_Stanza.IBMMovesPerMonth(ieco) = 1
                    Dmove = cellsPerMonth / 1
                End If
                'm_Stanza.IBMMovesPerMonth(ieco) = cellsPerMonth / 0.49 + 1
                'Dmove = cellsPerMonth / m_Stanza.IBMMovesPerMonth(ieco)

                'm_Stanza.IBMMovesPerMonth(ieco) = Int(2 * m_Data.Mvel(ieco) / (12 * m_Data.CellLength) + 1) 'allows movement of roughly 1/2 cellwidth per move

                isc = isc + 1
                'make up temporary list of suitable cells for this stanza
                Nused = 0
                For i = 1 To m_Data.InRow
                    For j = 1 To m_Data.InCol
                        m_Data.Bcell(i, j, ieco) = 0 ' NOTE call to initpackets must be after any other Bcell initialization for multistanza biomasses
                        m_Data.PredCell(i, j, ieco) = 0
                        'If (m_Data.PrefHab(ieco, m_Data.HabType(i, j)) = True Or m_Data.PrefHab(ieco, 0) = True) And m_Data.Depth(i, j) > 0 Then
                        'this could be proportional
                        If m_Data.HabCap(ieco)(i, j) > 0.1 And m_Data.Depth(i, j) > 0 Then
                            Nused = Nused + 1
                            iList(Nused) = i : Jlist(Nused) = j
                            If ist = 1 Then m_Stanza.iNursery(isp, Nused) = i : m_Stanza.jNursery(isp, Nused) = j
                        End If

                    Next j
                Next i

                If ist = 1 Then m_Stanza.Nnursery(isp) = Nused
                'then loop over ages to initialize numbers by age in stanza and distribute spatially
                For iaa = m_Stanza.Age1(isp, ist) To m_Stanza.Age2(isp, ist)
                    ia = ia + 1
                    m_Stanza.StanzaNo(isp, ia) = ist  'this table stores stanza number for fish of age ia, species isp
                    'following loop distributes total numbers at age over packets, sets initial weights
                    'note must be called after m_data.ThabArea (number of active cells) has been calculated
                    For ip = 1 To m_Stanza.Npackets
                        m_Stanza.Npacket(isp, ia, ip) = m_Stanza.NageS(isp, ia) / m_Stanza.Npackets * m_Data.ThabArea
                        m_Stanza.Wpacket(isp, ia, ip) = m_Stanza.WageS(isp, ia) + 0.0000000001
                    Next
                    For ip = 1 To m_Stanza.Npackets
                        'distribute packets uniformly over suitable cells for this stanza, using list set above
                        i1 = 1 + Me.m_rand.NextDouble() * (Nused - 1)
                        m_Stanza.iPacket(isp, ia, ip) = iList(i1) + Me.m_rand.NextDouble() ' 0.5 '
                        m_Stanza.jPacket(isp, ia, ip) = Jlist(i1) + Me.m_rand.NextDouble() '0.5 '
                        'DEBUG: reenable following two lines
                        m_Data.Bcell(iList(i1), Jlist(i1), ieco) = m_Data.Bcell(iList(i1), Jlist(i1), ieco) + m_Stanza.Npacket(isp, ia, ip) * m_Stanza.Wpacket(isp, ia, ip)
                        m_Data.PredCell(iList(i1), Jlist(i1), ieco) = m_Data.PredCell(iList(i1), Jlist(i1), ieco) + m_Stanza.Npacket(isp, ia, ip) * m_Stanza.WWa(isp, ia)
                    Next

                    'calculate distance per move for this group
                    m_Stanza.IBMdistmove(isp, ia) = Dmove 'm_Data.Mvel(ieco) / (12 * m_Data.CellLength) / m_Stanza.IBMMovesPerMonth(ieco) 'movement distance (cell widths) per movement step
                    'note dependence here is only on ieco so far; could be more closely related to age ia
                Next
            Next
            m_Stanza.MaxAgeSpecies(isp) = ia
        Next
        ReDim m_Stanza.Zcell(m_Data.InRow, m_Data.InCol, m_Data.NGroups)  'this variable used to store spatial field of total mortality rates for survival updates
        ' For i = 1 To 6: Debug.Print Bcell(1, 1, i), StartBiomass(i): Next
        'For i = 1 To 6: Debug.Print PredCell(1, 1, i), pred(i): Next
        'Stop
    End Sub

    ''' <summary>
    ''' Finds nearest suitable cell for IBM packets entering a stanza in a cell not suitable for that stanza
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' </remarks>
    Sub SetNearestOKcellforIBM()

        Dim isp As Integer, ist As Integer, ieco As Integer, i As Integer, j As Integer
        Dim i1 As Integer, i2 As Integer, j1 As Integer, j2 As Integer, ii As Integer, jj As Integer
        Dim dmin As Integer, Dist As Integer
        Dim MaxIter As Integer
        Dim iter As Integer

        MaxIter = Math.Max(Me.m_Data.InRow, Me.m_Data.InCol)
        'MaxIter = Inrow : If Incol > Inrow Then MaxIter = Incol

        ReDim Me.m_Data.ItoUse(Me.m_Stanza.Nsplit, Me.m_Stanza.MaxStanza, Me.m_Data.InRow, Me.m_Data.InCol)
        ReDim Me.m_Data.JtoUse(Me.m_Stanza.Nsplit, Me.m_Stanza.MaxStanza, Me.m_Data.InRow, Me.m_Data.InCol)

        For isp = 1 To Me.m_Stanza.Nsplit
            For ist = 2 To Me.m_Stanza.Nstanza(isp)
                ieco = Me.m_Stanza.EcopathCode(isp, ist)
                For i = 1 To Me.m_Data.InRow
                    For j = 1 To Me.m_Data.InCol
                        ' zero value will be used to indicate packet should not be moved on entry to stanza
                        Me.m_Data.ItoUse(isp, ist, i, j) = 0
                        Me.m_Data.ItoUse(isp, ist, i, j) = 0
                        'Does the packet need to be moved, only reset itouse and jtouse if so
                        If HabIsOk(ieco, i, j) = False Then
                            'Yes move the packet
                            dmin = 10000
                            i1 = i
                            i2 = i
                            j1 = j
                            j2 = j
                            iter = 0
                            Do While iter <= MaxIter
                                iter = iter + 1
                                i1 = i1 - 1 : If i1 < 1 Then i1 = 1
                                i2 = i2 + 1 : If i2 > Me.m_Data.InRow Then i2 = Me.m_Data.InRow
                                j1 = j1 - 1 : If j1 < 1 Then j1 = 1
                                j2 = j2 + 1 : If j2 > Me.m_Data.InCol Then j2 = Me.m_Data.InCol
                                For ii = i1 To i2
                                    If HabIsOk(ieco, ii, j1) Then 'check first column for row ii
                                        Dist = Abs(ii - i) + Abs(j1 - j)
                                        If Dist < dmin Then
                                            dmin = Dist
                                            Me.m_Data.ItoUse(isp, ist, i, j) = ii
                                            Me.m_Data.JtoUse(isp, ist, i, j) = j1
                                        End If
                                    End If
                                    If HabIsOk(ieco, ii, j2) Then 'check last column for row ii
                                        Dist = Abs(ii - i) + Abs(j2 - j)
                                        If Dist < dmin Then
                                            dmin = Dist
                                            Me.m_Data.ItoUse(isp, ist, i, j) = ii
                                            Me.m_Data.JtoUse(isp, ist, i, j) = j2
                                        End If

                                    End If
                                Next
                                For jj = j1 + 1 To j2 - 1
                                    If HabIsOk(ieco, i1, jj) Then 'check first row for column jj
                                        Dist = Abs(i1 - i) + Abs(jj - j)
                                        If Dist < dmin Then
                                            dmin = Dist
                                            Me.m_Data.ItoUse(isp, ist, i, j) = i1
                                            Me.m_Data.JtoUse(isp, ist, i, j) = jj
                                        End If

                                    End If
                                    If HabIsOk(ieco, i2, jj) Then 'check last row for column jj
                                        Dist = Abs(i2 - i) + Abs(jj - j)
                                        If Dist < dmin Then
                                            dmin = Dist
                                            Me.m_Data.ItoUse(isp, ist, i, j) = i2
                                            Me.m_Data.JtoUse(isp, ist, i, j) = jj
                                        End If

                                    End If
                                Next
                                'If ieco = 15 Then Stop
                                If dmin < 10000 Then Exit Do 'have found the best move
                            Loop
                        End If
                        Debug.Assert(Me.m_Data.ItoUse(isp, ist, i, j) >= 0 And Me.m_Data.ItoUse(isp, ist, i, j) <= m_Data.InRow, "SetNearestOKcellforIBM() set out of bounds cell.")
                        Debug.Assert(Me.m_Data.JtoUse(isp, ist, i, j) >= 0 And Me.m_Data.JtoUse(isp, ist, i, j) <= m_Data.InCol, "SetNearestOKcellforIBM() set out of bounds cell.")
                    Next j 'Map cols
                Next i 'Map rows
            Next ist 'Number of stanzas in this group
        Next isp ' number of multistanza groups

    End Sub


    Function HabIsOk(ByVal ieco As Integer, ByVal i As Integer, ByVal j As Integer) As Boolean
        'If Depth(i, j) > 0 And (PrefHab(ieco, HabType(i, j)) = True Or PrefHab(ieco, 0) = True) Then
        If Me.m_Data.Depth(i, j) > 0 And Me.m_Data.HabCap(ieco)(i, j) > 0.5 Then
            HabIsOk = True
        Else
            HabIsOk = False
        End If
    End Function



#End Region

#Region "Sailing Costs"

    Public Sub ClearPorts(ByVal iFleet As Integer)

        Dim iStart As Integer = iFleet
        Dim iEnd As Integer = iFleet
        Dim inRow As Integer = m_Data.InRow
        Dim inCol As Integer = m_Data.InCol
        Dim i As Integer
        Dim j As Integer

        If iStart <= 0 Then iStart = 0 : iEnd = Me.EcoSpaceData.nFleets

        For i = 1 To inRow
            For j = 1 To inCol
                For iFleet = iStart To iEnd
                    m_Data.Port(iFleet)(i, j) = False
                Next iFleet
            Next
        Next

    End Sub

    Public Sub SetAllCoastsToPorts(ByVal iFleet As Integer)

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim l As Integer
        Dim inRow As Integer = m_Data.InRow
        Dim inCol As Integer = m_Data.InCol
        Dim iStart As Integer = iFleet
        Dim iEnd As Integer = iFleet

        If iStart <= 0 Then iStart = 0 : iEnd = Me.EcoSpaceData.nFleets

        For i = 1 To inRow
            For j = 1 To inCol
                'Check if there is a neighboring cell which is in water
                If Me.EcoSpaceData.Depth(i, j) <= 0 Then    'it is a land cell
                    For k = i - 1 To i + 1 Step 2
                        For l = j - 1 To j + 1 Step 2
                            If k > 0 And k <= inRow And l > 0 And l <= inCol And Me.EcoSpaceData.Depth(k, l) > 0 Then
                                For iFleet = iStart To iEnd
                                    m_Data.Port(iFleet)(i, j) = True
                                Next iFleet
                            End If
                        Next
                    Next
                End If
            Next
        Next

    End Sub

    Public Sub CalculateCostOfSailing()

        If (Me.m_pluginManager IsNot Nothing) Then
            If Me.m_pluginManager.EcospaceCalculateCostOfSailing(Me.m_Data, Me.m_Data.Depth, Me.m_Data.Port, Me.m_Data.Sail) Then
                ' Done, overruled
                Return
            End If
        End If

        Dim iRow As Integer
        Dim iColPortX As Integer
        Dim iRowPortY As Integer
        Dim iCol As Integer
        Dim iPort As Integer
        Dim iFleet As Integer
        Dim Ports As Integer
        Dim minD(,,) As Single
        Dim Dist As Single
        Dim Lati As Single
        Dim Longi As Single
        Dim LatPort As Single
        Dim LonPort As Single
        Dim PortX() As Integer
        Dim PortY() As Integer
        Dim Disti As Single

        ' This calculation does NOT take the shape of land into account

        Ports = 0
        For iRow = 1 To m_Data.InRow
            For iCol = 1 To m_Data.InCol
                Me.m_Data.Port(0)(iRow, iCol) = False
                For iFleet = 1 To Me.m_Data.nFleets
                    If Me.m_Data.Port(iFleet)(iRow, iCol) = True Then
                        Ports += 1
                        Me.m_Data.Port(0)(iRow, iCol) = True
                        Exit For
                    End If
                Next
            Next
        Next
        ReDim PortX(Ports)
        ReDim PortY(Ports)
        Ports = 0

        For iRow = 1 To m_Data.InRow
            For iCol = 1 To m_Data.InCol
                If Me.m_Data.Port(0)(iRow, iCol) = True Then
                    Ports += 1
                    'PortX(Ports) = i
                    'PortY(Ports) = j

                    'm_Data.InRow = Y = Lat
                    'm_Data.InCol = X = Lon
                    PortX(Ports) = iCol
                    PortY(Ports) = iRow
                End If
            Next
        Next

        ReDim minD(Me.m_Data.nFleets, Me.m_Data.InRow, Me.m_Data.InCol)
        For iFleet = 0 To Me.m_Data.nFleets
            For iRow = 1 To m_Data.InRow
                For iCol = 1 To m_Data.InCol
                    minD(iFleet, iRow, iCol) = Single.MaxValue
                Next iCol
            Next iRow
        Next iFleet

        'Cell size in degress at the equator
        Dim CellSize As Single = cEcospaceDataStructures.ToCellSize(Me.m_Data.CellLength, Me.m_Data.AssumeSquareCells)

        For iPort = 1 To Ports      'go port by port
            iColPortX = PortX(iPort)
            iRowPortY = PortY(iPort)

            'Port location in degrees
            'm_Data.InRow = Y = Lat
            'm_Data.InCol = X = Lon
            LonPort = CSng(m_Data.Lon1 + (iColPortX * CellSize))
            LatPort = CSng(m_Data.Lat1 - (iRowPortY * CellSize))

            For iFleet = 0 To Me.m_Data.nFleets
                ' Is this fleet based in a this port?
                If Me.m_Data.Port(iFleet)(iRowPortY, iColPortX) Then

                    For iRow = 1 To m_Data.InRow
                        For iCol = 1 To m_Data.InCol
                            If Me.EcoSpaceData.Depth(iRow, iCol) > 0 Then 'water cell
                                Longi = CSng(m_Data.Lon1 + (iCol * CellSize))
                                Lati = CSng(m_Data.Lat1 - (iRow * CellSize))

                                Dist = CalDistance(LonPort, LatPort, Longi, Lati)
                                minD(iFleet, iRow, iCol) = Math.Min(Dist, minD(iFleet, iRow, iCol))
                            Else
                                minD(iFleet, iRow, iCol) = 0
                            End If
                        Next iCol
                    Next iRow
                End If
            Next iFleet
        Next iPort

        For iFleet = 0 To Me.m_Data.nFleets
            For iRow = 1 To m_Data.InRow
                For iCol = 1 To m_Data.InCol
                    If minD(iFleet, iRow, iCol) < Single.MaxValue Then Disti = minD(iFleet, iRow, iCol) Else Disti = 0.0!
                    Me.m_Data.Sail(iFleet)(iRow, iCol) = Disti
                Next iCol
            Next iRow
        Next iFleet

    End Sub

    Private Function CalDistance(ByVal Lon1 As Single, ByVal Lat1 As Single, ByVal Lon2 As Single, ByVal Lat2 As Single) As Single
        'On Local Error GoTo errCalDistance
        'Villy C received this sub is from Reg Watson 04 May 2001, modified to function and dropped last terms, also made types explicit
        'Calculates the distance between two map points Lon1,Lat1 and Lon2,Lat2
        'Points are measured decimal degrees
        'Returns Dist, Long Dist(X), Lat Dist(Y) in either NatMiles or km (DistType)
        'DistType 0=NatMiles, 1=km, 2=degrees
        'Provided by Laura Wing lwing@clausent.demon.co.uk to Ken White
        'Uses a spherical triangle to the pole
        '3 variations:
        '   Same Hemisphere
        '   Different Hemisphere
        '   Spans Greenwich meridian or anti-meridian

        'Expects - (Neg) for South Latitudes
        'Note: always goes the shortest way... not over pole or wrong way around the world
        'Dist does not have a sign but XDist and YDist do

        Dim Dist As Double

        If (Me.m_Data.AssumeSquareCells) Then
            Dim dx As Double = Lon2 - Lon1
            Dim dy As Double = Lat2 - Lat1
            Dist = Math.Sqrt(dx * dx + dy * dy)
        Else
            Dim CoLatA As Double
            Dim CoLatB As Double
            Dim DifLong As Double
            Dim PartA As Double
            Dim PartB As Double
            Dim XXD As Double
            Dim Ydist As Double

            CoLatA = 90 + Sign(Lat1) * Abs(Lat1)
            CoLatB = 90 + Sign(Lat2) * Abs(Lat2)

            DifLong = Abs(Lon1 - Lon2)

            If DifLong > 180 Then
                DifLong = 360 - DifLong
            End If

            Ydist = Lat1 - Lat2

            PartA = Cos(CoLatA * DEG2RAD) * Cos(CoLatB * DEG2RAD)
            PartB = Sin(CoLatA * DEG2RAD) * Sin(CoLatB * DEG2RAD) * Cos(DifLong * DEG2RAD)
            XXD = PartA + PartB

            If XXD = 1.0# Then XXD = 1.000001
            'There is no arccos so it is atn(-X/sqr(-X*X+1))+1.5708
            Dist = (Atan(-XXD / Sqrt(-XXD * XXD + 1.0#)) + 1.5708) / TWO_PI * 360.0#
        End If

        Return CSng(Dist)

        'This code can not be reached
        'Dist is returned before this can execute
        '        Xdist = Sqrt(Dist ^ 2 - Ydist ^ 2) * Sign(Lon1 - Lon2)
        '        Exit Function

        'errCalDistance:
        '        Xdist = -1
        '        CalDistance = 0 '-1 vc changed this from -1 to 0
        '        Exit Function

    End Function

    ''' <summary>
    ''' Set the capacity of each cell in the map based on habitat inputs Habitat Preference and Proportion of Habitat type in cell
    ''' </summary>
    ''' <remarks>Habitat capacity of a map cell = sum product of habitat preference and habitat area in cell</remarks>
    Private Sub setHabCapFromHabitat()

        Dim i As Integer, j As Integer, K As Integer

        For K = 1 To Me.m_Data.NGroups
            If (Me.m_Data.isGroupHabCapChanged(K) = True) And
               ((Me.m_Data.CapCalType(K) And eEcospaceCapacityCalType.Habitat) = eEcospaceCapacityCalType.Habitat) Then
                For i = 1 To Me.m_Data.InRow
                    For j = 1 To Me.m_Data.InCol
                        If m_Data.Depth(i, j) > 0.0 Then
                            'Does this group use specific habitats?
                            If (Me.m_Data.PrefHab(K, 0) = 0.0) Then
                                ' #Yes: determine cell capacity from habitat area * habitat usage
                                Dim cap As Single = 0
                                For ihab As Integer = 1 To Me.m_Data.NoHabitats
                                    '[capacity of cell] = sumof([habitat preference] * [percentage of habitat in cell])
                                    cap += Me.m_Data.PrefHab(K, ihab) * Me.m_Data.PHabType(ihab)(i, j)
                                Next ihab
                                ' JS 05May16: Multiply base capacity by local cell capacity
                                Me.m_Data.HabCap(K)(i, j) *= cap
                                'Debug.Assert(Not (K = 1 And Me.m_Data.HabCap(K)(i, j) <> 0))
                            Else
                                ' JS 05May16: No nothing; leave base capacity intact
                                ' 'Group uses All Habitats at 100% (PrefHab(K, 0) = 1.0)
                                ' Me.m_Data.HabCap(K)(i, j) = 1.0
                            End If 'Me.m_Data.PrefHab(K, 0) = 0.0
                        End If 'm_Data.Depth(i, j) > 0.0

                    Next j
                Next i
            End If ' Me.m_Data.bHasHabitatChanged(K)
        Next K

    End Sub



    ''' <summary>
    ''' Set capacity maps from all input sources 
    ''' Habitats, habitat preference, habitat proportion in cell, Input Capacity Maps and enviromental response functions.
    ''' </summary>
    ''' <remarks>
    ''' 1.)The Capacity input map and Habitat preferences are summed into the capacity map.
    ''' 2.)The Enviromental input maps are multiplied onto the existing capacity. This allows the Enviromental maps to reduce the capacity.
    ''' 3.)The capacity map is normalized.
    ''' 4.)Capacity gradients are set to move biomass from low to high capacity cells
    ''' </remarks>
    Public Sub SetHabCap()

        'Have ANY of the inputs changed
        If Not Me.m_Data.isCapacityChanged Then
            Return
        End If

        'Clear out the old value used to normalize the map  
        ClearHabCapGroups(Me.m_Data.isGroupHabCapChanged)

        'Sum the capacity input map into HabCap
        Me.setHabCapFromCapInputMap()

        'Capacity from Habitats
        Me.setHabCapFromHabitat()

        'Capacity from Enviromental input maps
        Me.setHabCapFromMaps()

        'now normalize the capacity map
        Me.normalizeCapacityMap()

        'Set the capacity gradients to flow from low to high capacity cells
        Me.runAjustLowHabCapsThreaded()

        'Make sure runAjustLowHabCapsThreaded() did not set HabCap()<MIN_HABCAP
        For ig As Integer = 1 To Me.m_Data.NGroups
            For ir As Integer = 1 To Me.m_Data.InRow
                For ic As Integer = 1 To Me.m_Data.InCol
                    If Me.m_Data.Depth(ir, ic) > 0 Then
                        'Debug.Assert(Me.m_Data.HabCap(ir, ic, ig) > 0, "Habcap = 0")
                        If Me.m_Data.HabCap(ig)(ir, ic) < MIN_HABCAP Then Me.m_Data.HabCap(ig)(ir, ic) = MIN_HABCAP
                    End If 'Me.m_Data.Depth(ir, ic) 
                Next ic
            Next ir
        Next ig

        Me.SetMovementParameters()

        'All the map changes have been computed
        Me.m_Data.isCapacityChanged = False
        'this will clear the individual isGroupHabCapChanged() flags
        Me.m_Data.setHabCapGroupIsChanged(False)

    End Sub


    Private Sub ClearHabCapGroups(isCapChanged() As Boolean)

        For igrp As Integer = 1 To Me.m_Data.NGroups

            If isCapChanged(igrp) Then

                'If capacity has not initialized then clear out the max and total values
                If Not Me.m_Data.hasCapInitialized Then
                    m_Data.MaxHabCap(igrp) = 0.0F
                End If

                'm_Data.TotHabCap(igrp) = 0.0F

                For irow As Integer = 1 To Me.m_Data.InRow
                    For icol As Integer = 1 To Me.m_Data.InCol

                        Me.m_Data.HabCap(igrp)(irow, icol) = 0.0F

                    Next icol
                Next irow
            End If

        Next igrp

    End Sub


    ''' <summary>
    ''' Create and run the AdjustLowHapCapsThreaded() threads
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub runAjustLowHabCapsThreaded()
        Dim iFirstGrp As Integer = 1
        Dim ilastGrp As Integer
        Dim nGrpsPerThread As Integer
        Dim nCompleted As Integer
        Dim nRun As Integer

        Dim waitOb As AutoResetEvent = New AutoResetEvent(False)
        Dim nThreads As Integer = Me.m_Data.nGridSolverThreads
        If nThreads > Me.m_Data.NGroups Then nThreads = Me.m_Data.NGroups

        cEcoSpace.m_ThreadIncrementCount = nThreads

        For ithrd As Integer = 1 To nThreads

            'Amount of work remaining / resourses remaining
            nGrpsPerThread = Me.computeThreadLoad(Me.m_Data.NGroups, nCompleted, nThreads, ithrd)

            nCompleted += nGrpsPerThread
            nRun += 1

            ilastGrp = iFirstGrp + nGrpsPerThread - 1

            'bound the group index
            If ilastGrp > Me.m_Data.NGroups Then
                ilastGrp = Me.m_Data.NGroups + 1
            End If

            'ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf Me.AdjustLowHabCapsThreaded), New cThreadedCallArgs(waitOb, iFirstGrp, ilastGrp))

            Dim arg As Object = New cThreadedCallArgs(waitOb, iFirstGrp, ilastGrp)
            Dim worker As Thread = New Thread(Sub() Me.AdjustLowHabCapsThreaded(arg))
            worker.Start()

            iFirstGrp += nGrpsPerThread

        Next ithrd

        Debug.Assert(nCompleted = Me.m_Data.NGroups)
        If Not waitOb.WaitOne(THREAD_TIMEOUT) Then
            cLog.Write(Me.ToString & ".runAjustLowHabCapsThreaded() AdjustLowHabCapsThreaded timed out.")
            Debug.Assert(False, Me.ToString & ".runAjustLowHabCapsThreaded() AdjustLowHabCapsThreaded timed out.")
        End If


    End Sub


    ''' <summary>
    ''' Multithreaded version
    ''' Adjust habcap’s so as to cause oriented movement toward nearest good cells. It should be much, much faster than 
    ''' old habgrad, uses dynamic programming to find minimum distances from bad cells to good ones and then exponential 
    ''' decrease in habcap with those distances; I have used the same algorithm to find things like minimum distances to 
    ''' fishing ports, nice because it even works for moving creatures around island barriers and such.
    ''' </summary>
    Private Sub AdjustLowHabCapsThreaded(ByVal ArgsOb As Object)

        'dynamic programming algorithm to set gradient in low habcap cells (habcap<=habcapmin) so as to orient movement ‘toward cells with habcap>habcapmin
        Dim i As Integer, j As Integer, k As Integer, d As Integer, Dmin As Integer, DistMin(,) As Integer, Maxiter As Integer
        Dim HabCapMin As Single, MaxDist As Integer, iter As Integer, DistFac As Single, NumBad As Integer
        Dim grpArgs As cThreadedCallArgs

        Try

            grpArgs = DirectCast(ArgsOb, cThreadedCallArgs)

            Debug.Assert(grpArgs.iFirst > 0)

            'bounds checking 
            If grpArgs.iFirst < 1 Then grpArgs.iFirst = 1
            If grpArgs.iLast > Me.m_Data.NGroups Then grpArgs.iLast = Me.m_Data.NGroups

            ReDim DistMin(Me.m_Data.InRow + 1, Me.m_Data.InCol + 1)

            HabCapMin = 0.01 'minimum allowable value of habcap before adjustment for distance to cell with habcap>habcapmin
            DistFac = 0.4 'exponential decrease in habcap per cell width distance from cell with habcap>habcapmin
            MaxDist = Math.Max(Me.m_Data.InRow, Me.m_Data.InCol)
            'Maxiter = MaxDist / 2 : If Maxiter = 0 Then Maxiter = 1

            For k = grpArgs.iFirst To grpArgs.iLast 'Me.m_Data.nvartot
                'initialize distmin for all cells for group k

                If Me.m_Data.isGroupHabCapChanged(k) Then

                    'How many cells can a fish move in a lifetime? We take it to be longevity * dispersal as a distance in km. 
                    'Divide this with the average cell size. For this we could use length or width or ? 
                    'We chose now to use half the cell length as a compromise, rather than cell width, as it up north would mean that
                    'groups could move perhaps down to equator. 
                    'this is really important with the big global half degree model, where it now (Jan 2012) was iterating 360 times
                    'over the 350 x 720 cell maps.
                    Dim MaxNoOfCellsToMoveInALifetime As Integer = CInt(EcoSpaceData.Mvel(k) / EcoPathData.PB(k) / (EcoSpaceData.CellLength / 2))
                    '                                           = Dispersal           * Longevity          /half the cell length
                    Maxiter = Min(MaxNoOfCellsToMoveInALifetime, MaxDist)
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'jb 24-Feb-2016
                    'Allow Maxitter to be zero. 
                    'This means there will be no dispersal across areas of low habitat for groups with very low dispersal rates
                    'This came to light because of a model with fish farming
                    'that seemed to "leak" biomass toward the closest edge
                    'If Maxiter = 0 Then Maxiter = 1
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    'Longevity for this species:
                    'Dim Longevity As Single = 1 / Me.EcoPathData.PB(k)
                    'Dim Dispersal As Single = Me.EcoSpaceData.Mvel(k)

                    NumBad = 0
                    For i = 0 To Me.m_Data.InRow + 1
                        For j = 0 To Me.m_Data.InCol + 1
                            If Me.m_Data.Depth(i, j) > 0 Then
                                If Me.m_Data.HabCap(k)(i, j) <= HabCapMin Then
                                    Me.m_Data.HabCap(k)(i, j) = HabCapMin
                                    DistMin(i, j) = MaxDist
                                    NumBad = NumBad + 1
                                Else
                                    DistMin(i, j) = 0
                                End If
                            End If
                        Next j
                    Next i

                    'then do dynamic program iteratation to reset distmin for each cell to minimum distance to cell with habcap>habcapmin
                    'skip iteration if numbad=0
                    If NumBad > 0 Then
                        For iter = 1 To Maxiter
                            For i = 1 To Me.m_Data.InRow
                                For j = 1 To Me.m_Data.InCol
                                    If Me.m_Data.Depth(i, j) > 0 And Me.m_Data.HabCap(k)(i, j) <= HabCapMin Then
                                        'check the four faces of this cell to find min distance from it toward good cell
                                        Dmin = MaxDist

                                        If Me.m_Data.Depth(i - 1, j) > 0 Then
                                            d = DistMin(i - 1, j) + 1
                                            If d < Dmin Then Dmin = d
                                        End If

                                        If Me.m_Data.Depth(i + 1, j) > 0 Then
                                            d = DistMin(i + 1, j) + 1
                                            If d < Dmin Then Dmin = d
                                        End If

                                        If Me.m_Data.Depth(i, j - 1) > 0 Then
                                            d = DistMin(i, j - 1) + 1
                                            If d < Dmin Then Dmin = d
                                        End If

                                        If Me.m_Data.Depth(i, j + 1) > 0 Then
                                            d = DistMin(i, j + 1) + 1
                                            If d < Dmin Then Dmin = d
                                        End If
                                        DistMin(i, j) = Dmin
                                    End If
                                Next j
                            Next i
                        Next iter

                        'have now set distmin for each bad cell to minimum travel distance from that cell to a cell with habcap>habcapmin
                        'apply exponential decrease to habcap based on the minimum travel distance
                        For i = 1 To Me.m_Data.InRow
                            For j = 1 To Me.m_Data.InCol
                                If Me.m_Data.Depth(i, j) > 0 And Me.m_Data.HabCap(k)(i, j) <= HabCapMin Then
                                    Me.m_Data.HabCap(k)(i, j) = HabCapMin * Exp(-DistFac * DistMin(i, j))
                                    If Me.m_Data.HabCap(k)(i, j) < MIN_HABCAP Then Me.m_Data.HabCap(k)(i, j) = MIN_HABCAP
                                End If
                            Next j
                        Next i

                    End If 'end of if when numbad=0 and iteration+adjustment can be skipped
                End If ' Me.m_Data.isGroupHabCapChanged(k)
            Next k

        Catch ex As Exception
            System.Console.WriteLine("Ecospace.AdjustLowHabCapsThreaded Exception: " & ex.Message)
        End Try

        Try

            If Interlocked.Decrement(cEcoSpace.m_ThreadIncrementCount) = 0 Then
                grpArgs.WaitHandle.Set()
            End If

        Catch ex As Exception
            System.Console.WriteLine("Ecospace.AdjustLowHabCapsThreaded Exception: " & ex.Message)
        End Try


    End Sub

    ''' <summary>
    ''' Set Habitat Capacity map from the user input HabCap map
    ''' </summary>
    Private Function setHabCapFromCapInputMap() As Boolean

        For igrp As Integer = 1 To Me.m_Data.NGroups
            ' Have the Habitat Capacity input maps changed?
            If Me.m_Data.isGroupHabCapChanged(igrp) Then
                ' #Yes, the map has changed

                ' Check if this group has valid capacity input. 
                Dim bUseHapCapInput As Boolean = False

                ' If no input bUseHapCapInput will be false, and homogenous map input of 1 will be assumed
                Dim irow As Integer = 1
                While (bUseHapCapInput = False) And (irow <= Me.m_Data.InRow)
                    Dim icol As Integer = 1
                    While (bUseHapCapInput = False) And (icol <= Me.m_Data.InCol)
                        If Me.m_Data.Depth(irow, icol) > 0 Then
                            If (Me.m_Data.HabCapInput(igrp)(irow, icol) > 0) Then
                                bUseHapCapInput = True
                            End If
                        End If
                        icol += 1
                    End While
                    irow += 1
                End While

                'Get the baseline capacity values from the user input capacity map 
                'This is done first so the values are just copied in
                'All others capacity drivers act as a multiplier on this baseline 
                For irow = 1 To Me.m_Data.InRow
                    For icol As Integer = 1 To Me.m_Data.InCol
                        If Me.m_Data.Depth(irow, icol) > 0 Then
                            If Not bUseHapCapInput Then
                                Me.m_Data.HabCap(igrp)(irow, icol) = 1
                            Else
                                Me.m_Data.HabCap(igrp)(irow, icol) = Me.m_Data.HabCapInput(igrp)(irow, icol)
                            End If
                        End If
                    Next icol
                Next irow
            End If
        Next igrp

        Return True

    End Function


    Public Function GetHabCapsLessThen(ByVal LowerLimits() As Single) As List(Of Integer)

        Me.UpdateDepthMap()

        Me.NormalizeMigrationMaps()

        'make sure the habitat capacity has been set
        Me.m_Data.isCapacityChanged = True
        Me.m_Data.setHabCapGroupIsChanged(True)
        Me.m_Data.hasCapInitialized = False
        Me.SetHabCap()
        Me.m_Data.hasCapInitialized = True

        'Makes sure the number of water cells has been set
        If Me.m_Data.nWaterCells <= 0 Then
            Me.m_Data.setNWaterCells()
        End If

        'build a list of groups that have a max capacity of less than the lower limit
        Dim failedIndexes As New List(Of Integer)
        For igrp As Integer = 1 To Me.m_Data.NGroups
            Dim CapPerCell As Single = Me.m_Data.TotHabCap(igrp) / Me.m_Data.nWaterCells * 100
            If CapPerCell < LowerLimits(igrp) Or Single.IsNaN(CapPerCell) Then
                failedIndexes.Add(igrp)
            End If
        Next

        Return failedIndexes

    End Function

    ''' <summary>
    ''' Update the capacity map based on the datatype
    ''' </summary>
    ''' <param name="MapDataType"></param>
    ''' <returns>True if the capacity map was changed. False otherwise.</returns>
    ''' <remarks></remarks>
    Public Function UpdateMaps(ByVal MapDataType As eDataTypes) As Boolean
        Try

            If MapDataType = eDataTypes.EcospaceLayerHabitat Or _
                MapDataType = eDataTypes.EcospaceHabitat Or _
                MapDataType = eDataTypes.EcospaceLayerDepth Or _
                MapDataType = eDataTypes.EcospaceLayerHabitatCapacityInput Or _
                MapDataType = eDataTypes.EcospaceLayerHabitatCapacity Or _
                MapDataType = eDataTypes.EcospaceLayerRelPP Or _
                MapDataType = eDataTypes.CapacityMediation Or _
                MapDataType = eDataTypes.EcospaceMapResponse Or _
                MapDataType = eDataTypes.EcospaceModelParameter Or _
                MapDataType = eDataTypes.EcospaceGroup Or
                MapDataType = eDataTypes.EcospaceLayerDriver Or _
                MapDataType = eDataTypes.EcospaceLayerExclusion Then

                'This will force the capacity model to update
                Me.m_Data.isCapacityChanged = True
                Me.m_Data.setHabCapGroupIsChanged(True)

                Return True

            End If

        Catch ex As Exception
            Me.m_publisher.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_HABCAP_COMPUTE_ERROR, ex.Message), _
                                                    eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Warning))
        End Try

        Return False

    End Function


    Private Sub normalizeCapacityMap()
        Dim iGrp As Integer, ir As Integer, ic As Integer

        'now normalize the capacity map
        For iGrp = 1 To Me.m_Data.NGroups

            If Me.m_Data.isGroupHabCapChanged(iGrp) Then

                m_Data.TotHabCap(iGrp) = 0.0F

                'Capacity Model has a one time initialization of the max capacity used for normalization
                If Not Me.m_Data.hasCapInitialized Then
                    For ir = 1 To Me.m_Data.InRow
                        For ic = 1 To Me.m_Data.InCol
                            If Me.m_Data.Depth(ir, ic) > 0 Then
                                If Not m_Data.IsMigratory(iGrp) Then
                                    m_Data.MaxHabCap(iGrp) = Math.Max(Me.m_Data.HabCap(iGrp)(ir, ic), m_Data.MaxHabCap(iGrp))
                                Else
                                    For imon As Integer = 1 To 12
                                        'is this cell part of the migration pattern for any month
                                        If Me.m_Data.MigMaps(iGrp, imon)(ir, ic) > MIN_MIG_PROB Then
                                            'Yes sum up the capacity
                                            m_Data.MaxHabCap(iGrp) = Math.Max(Me.m_Data.HabCap(iGrp)(ir, ic), m_Data.MaxHabCap(iGrp))
                                        End If
                                    Next ' For imon As Integer = 1 To 12
                                End If 'Not m_Data.IsMigratory(iGrp)
                            End If 'Me.m_Data.Depth(ir, ic) > 0
                        Next ic
                    Next ir
                End If 'Not Me.m_Data.hasCapInitialized 

                'Normalize and get the total cap by group
                For ir = 1 To Me.m_Data.InRow
                    For ic = 1 To Me.m_Data.InCol
                        If Me.m_Data.Depth(ir, ic) > 0 Then
                            'normalized capacity
                            Me.m_Data.HabCap(iGrp)(ir, ic) = Me.m_Data.HabCap(iGrp)(ir, ic) / Me.m_Data.MaxHabCap(iGrp)
                            'Greater than min Capacity
                            If Me.m_Data.HabCap(iGrp)(ir, ic) < MIN_HABCAP Then Me.m_Data.HabCap(iGrp)(ir, ic) = MIN_HABCAP '0.000001F

                            'Populate TotHabCap with sum of capacity
                            'Used to spatially  distribute the initial state biomass in InitSpatialEquilibrium
                            If Not Me.m_Data.IsMigratory(iGrp) Then
                                'Non migrating group
                                Me.m_Data.TotHabCap(iGrp) += Me.m_Data.HabCap(iGrp)(ir, ic)
                            Else
                                'Migrating Group 
                                'Annual average capacity for the 12 month migration pattern
                                For imon As Integer = 1 To 12
                                    'is this cell part of the migration pattern for any month
                                    If Me.m_Data.MigMaps(iGrp, imon)(ir, ic) > MIN_MIG_PROB Then
                                        'Yes caculate the average monthly total capacity
                                        Me.m_Data.TotHabCap(iGrp) += Me.m_Data.HabCap(iGrp)(ir, ic) / 12
                                    End If
                                Next

                                ''Alternatively
                                ''Total Capacity for the first month only
                                ''This makes the spatialy average biomass equal to the Ecopath biomass
                                ''but can create even bigger biomass concentration issues 
                                ''setting V and A
                                'If Me.m_Data.MigMaps(iGrp, 1)(ir, ic) > MIN_MIG_PROB Then
                                '    'Yes caculate the average monthly total capacity
                                '    Me.m_Data.TotHabCap(iGrp) += Me.m_Data.HabCap(iGrp)(ir, ic)
                                'End If

                            End If
                        End If 'Me.m_Data.Depth(ir, ic) > 0 
                    Next ic
                Next ir

                'set habcaps for cells across grid boundaries
                Dim bMultiStanza As Boolean = False
                For ist As Integer = 1 To m_Stanza.Nsplit

                    For ii As Integer = 1 To m_Stanza.Nstanza(ist)
                        If iGrp = m_Stanza.EcopathCode(ist, ii) Then
                            bMultiStanza = True 'stanzas are indexed from zero
                            Exit For
                        End If
                    Next ii
                    If bMultiStanza = True Then Exit For
                Next ist

                If Not bMultiStanza Then
                    For ic = 0 To Me.m_Data.InCol + 1
                        Me.m_Data.HabCap(iGrp)(0, ic) = Me.m_Data.HabCap(iGrp)(1, ic)

                        Me.m_Data.HabCap(iGrp)(Me.m_Data.InRow + 1, ic) = Me.m_Data.HabCap(iGrp)(Me.m_Data.InRow, ic)
                    Next ic

                    For ir = 0 To Me.m_Data.InRow + 1
                        Me.m_Data.HabCap(iGrp)(ir, 0) = Me.m_Data.HabCap(iGrp)(ir, 1)
                        Me.m_Data.HabCap(iGrp)(ir, Me.m_Data.InCol + 1) = Me.m_Data.HabCap(iGrp)(ir, Me.m_Data.InCol)
                    Next ir
                End If
            End If ' Me.m_Data.isGroupHabCapChanged(iGrp)
        Next iGrp
    End Sub

    ''' <summary>
    ''' Normalize the proportion of habitat type in a cell that exceed 100% PHabType(ir,ic,ihab).
    ''' </summary>
    ''' <remarks>Sum of all habitat types in a cell must be 100% or less.</remarks>
    Public Sub normalizePropHabType()
        Dim TProp As Single
        Dim ihab As Integer
        Dim bChanged As Boolean

        Try

            For ir As Integer = 1 To Me.m_Data.InRow
                For ic As Integer = 1 To Me.m_Data.InCol
                    TProp = 0
                    For ihab = 1 To Me.m_Data.NoHabitats
                        TProp += Me.m_Data.PHabType(ihab)(ir, ic)
                    Next

                    'Does the total proportion of habitats exceed 100%
                    'Less than 100% is ok
                    If TProp > 1 Then
                        'Yes normalize this cell 
                        For ihab = 1 To Me.m_Data.NoHabitats
                            Me.m_Data.PHabType(ihab)(ir, ic) = Me.m_Data.PHabType(ihab)(ir, ic) / TProp
                        Next
                        bChanged = True
                    End If

                Next ic
            Next ir

            'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            'TEMP HACK
            'For update bug that set PHabType as the Habitat Index instead of Propotion of Habitat Type
            ''Debug.Assert(False, "WARNING: EcoSpace.normalizePropHabType() has changed all valid habitats to 1.0!")
            'For ir As Integer = 1 To Me.m_Data.InRow
            '    For ic As Integer = 1 To Me.m_Data.InCol

            '        For ihab = 1 To Me.m_Data.NoHabitats
            '            If Me.m_Data.PHabType(ir, ic, ihab) > 1.0 Then
            '                Me.m_Data.PHabType(ir, ic, ihab) = 1.0
            '                bChanged = True
            '            End If

            '        Next ihab
            '    Next ic
            'Next ir
            'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


            If bChanged Then
                'maybe send a message
                System.Console.WriteLine("Habitat proportions normalized.")
            End If

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Friend Function getMissingMigrationMaps(ByRef MigrationMapsSet() As Boolean) As Integer
        Dim nMissing As Integer
        Dim bFoundMonth As Boolean

        MigrationMapsSet = New Boolean(Me.m_Data.NGroups) {}

        For igrp As Integer = 1 To Me.m_Data.NGroups
            MigrationMapsSet(igrp) = True
            If Me.m_Data.IsMigratory(igrp) Then

                MigrationMapsSet(igrp) = True

                For imon As Integer = 1 To 12
                    bFoundMonth = False
                    For ir As Integer = 1 To Me.m_Data.InRow
                        For ic As Integer = 1 To Me.m_Data.InCol
                            If Me.m_Data.Depth(ir, ic) > 0 Then

                                'is this cell part of the migration pattern for any month
                                'then True for this group
                                If Me.m_Data.MigMaps(igrp, imon)(ir, ic) > MIN_MIG_PROB Then
                                    bFoundMonth = True
                                    Exit For
                                End If

                            End If 'Me.m_Data.Depth(ir, ic) > 0 
                        Next ic
                        If bFoundMonth Then Exit For
                    Next ir

                    MigrationMapsSet(igrp) = MigrationMapsSet(igrp) And bFoundMonth
                    If Not MigrationMapsSet(igrp) Then nMissing += 1
                    'This group is already missing a map for this month
                    'No need to do the other months 
                    If Not MigrationMapsSet(igrp) Then Exit For

                Next imon

            End If 'If Me.m_Data.IsMigratory(igrp) Then

        Next igrp

        Return nMissing

    End Function


    ''' <summary>
    ''' Set Capacity based on enviromental response functions
    ''' </summary>
    ''' <returns>True if enviromental response functions were used to set habitat capacity. False otherwise.</returns>
    ''' <remarks>
    ''' Enviromental response functions are multiplied onto the existing capacity. 
    ''' This allows the response function to reduce the capacity.
    ''' </remarks>
    Private Function setHabCapFromMaps() As Boolean
        Dim irow As Integer, icol As Integer, igrp As Integer, bReturn As Boolean
        'Dim orgCap As Single
        If (Me.m_Data.CapMaps Is Nothing) Then Return False

        'System.Console.WriteLine("Sethabcap")
        For Each map As IEnviroInputData In Me.m_Data.CapMaps

            'System.Console.WriteLine(map.Layer.Name + ", " + map.isLayerActive.ToString)

            'Is this layer active
            If map.IsDriverActive Then
                'System.Console.Write("Active Layer = " + map.Layer.Name + ",")
                For igrp = 1 To Me.m_Data.NGroups
                    'Has the habitat for this group changed
                    If (Me.m_Data.isGroupHabCapChanged(igrp) = True) And
                      ((Me.m_Data.CapCalType(igrp) And eEcospaceCapacityCalType.EnvResponses) = eEcospaceCapacityCalType.EnvResponses) Then
                        'Does this group contain a response function for this map
                        If map.ResponseIndexForGroup(igrp) > 0 Then
                            'System.Console.Write(igrp.ToString + ",")
                            'Yep Layer is Active
                            'Habitat for this group has changed
                            'There is a response function
                            For irow = 1 To Me.m_Data.InRow
                                For icol = 1 To Me.m_Data.InCol
                                    If Me.m_Data.Depth(irow, icol) > 0 Then
                                        'For debugging
                                        'dumpCapacity(map, igrp, irow, icol)
                                        Me.m_Data.HabCap(igrp)(irow, icol) *= map.ResponseFunction(igrp, irow, icol)

                                    End If
                                Next icol
                            Next irow
                        End If ' map.ResponseIndexForGroup(igrp) > 0
                    End If ' Me.m_Data.isGroupHabCapChanged(igrp)
                Next igrp
                'System.Console.WriteLine()
            End If ' map.isLayerActive

        Next map

        bReturn = True

        Return bReturn

    End Function

    Private Sub dumpCapacity(map As cEnviroInputMap, igrp As Integer, row As Integer, col As Integer)
        If map.Layer.DataType = eDataTypes.EcospaceLayerDriver Then
            If igrp = 27 Then
                If row = 34 And col = 174 Then
                    Dim cellValue As Single = map.Layer.Cell(row, col)
                    Dim response As Single = map.ResponseFunction(igrp, row, col)
                    Dim cap As Single = Me.m_Data.HabCap(igrp)(row, col)

                    'System.Console.WriteLine("SST," + cellValue.ToString + ",Response," + response.ToString + ",Cap," + cap.ToString + ",NewCap," + (cap * response).ToString)

                End If
            End If
        End If
    End Sub

    Private Sub SmoothCap(ByVal k As Integer)

        Dim cnew(,) As Single, i As Integer, j As Integer
        Dim t As Single
        Dim n As Integer
        ReDim cnew(Me.m_Data.InRow, Me.m_Data.InCol)

        For i = 1 To Me.m_Data.InRow
            For j = 1 To Me.m_Data.InCol
                t = 0
                n = 0
                For ii As Integer = i - 1 To i + 1
                    For jj As Integer = j - 1 To j + 1
                        If Not (ii = 0 Or jj = 0 Or ii = Me.m_Data.InRow + 1 Or jj = Me.m_Data.InCol + 1) Then
                            t += Me.m_Data.HabCap(k)(ii, jj)
                            n += 1
                        End If
                    Next jj
                Next ii
                cnew(i, j) = t / n
            Next j
        Next i

        For i = 1 To Me.m_Data.InRow
            For j = 1 To Me.m_Data.InCol
                Me.m_Data.HabCap(k)(i, j) = cnew(i, j)
            Next
        Next

    End Sub

    Public Sub setBaseValues(iTime As Integer)
        Dim igrp As Integer

        If iTime <> 1 Then
            'Only set base values if in the first timestep
            'If in a Spin-Up iTime is held at 1
            Return
        End If

        'Spin-Up Flags
        'Me.m_Data.UseSpinUp 
        'Using a Spin-Up period
        'Public Boolean used by the interface

        'Me.m_Data.bInSpinUp 
        'Are we currently in a Spin-Up period 
        'Can be used by an interface to tell if the current time step is in the Spin-Up period

        'Me.bInitSpinUpBase
        'Used to tell if the Spin-Up base has been ititialized yet
        'Private use only by Ecospace to maintain the initialization state of the base values

        'xxxxxxxxxxxxxxx NOT USING SPIN-UP xxxxxxxxxxxxxxx
        'Set the base values

        If Not Me.m_Data.UseSpinUp Then
            'xxxxx NOT Using the Spin-Up xxxxxx'
            For igrp = 1 To Me.m_Data.NGroups
                Me.m_Data.BBase(igrp) = Btime(igrp) ' Me.m_EPdata.B(igrp) '

                'Base values from Ecosim and EcoPath
                Me.m_Data.BaseFishMort(igrp) = Me.m_SimData.Fish1(igrp)
                Me.m_Data.BaseConsump(igrp) = Me.m_EPdata.QB(igrp) ' (Me.m_SimData.Eatenby(igrp) / Me.m_SimData.StartBiomass(igrp))
                Me.m_Data.BasePredMort(igrp) = (Me.m_SimData.Eatenof(igrp) / Me.m_SimData.StartBiomass(igrp))
                Me.m_Data.BaseCatch(igrp) = Me.m_EPdata.fCatch(igrp)
            Next igrp
        End If

        'xxxxxxxxxxxxxxx USE SPIN-UP xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'In a Spin-Up Period and the base values have NOT been initialized
        If Me.m_Data.bInSpinUp And Me.bInitSpinUpBase Then
            'ONLY Do this once for the first time step

            'In SpinUp period always keep the SpinUpBase for the Spin-Up Stats
            For igrp = 1 To Me.m_Data.NGroups
                Me.m_Data.SpinUpBBase(igrp) = Btime(igrp)
            Next igrp

            If Me.m_Data.UseSpinUpBase Then
                'User want to plot the values relative to the beginning of the Spin-Up period
                For igrp = 1 To Me.m_Data.NGroups
                    Me.m_Data.BBase(igrp) = Btime(igrp)

                    'Base values from Ecosim and EcoPath
                    Me.m_Data.BaseFishMort(igrp) = Me.m_SimData.Fish1(igrp)
                    Me.m_Data.BaseConsump(igrp) = (Me.m_SimData.Eatenby(igrp) / Me.m_SimData.StartBiomass(igrp))
                    Me.m_Data.BasePredMort(igrp) = (Me.m_SimData.Eatenof(igrp) / Me.m_SimData.StartBiomass(igrp))
                    Me.m_Data.BaseCatch(igrp) = Me.m_EPdata.fCatch(igrp)

                Next igrp
            End If 'Me.m_Data.UseSpinUpBase

            'SpinUp base has been initialized
            'we don't want to do this again
            Me.bInitSpinUpBase = False

        End If 'Me.m_Data.UseSpinUp And Me.m_Data.bInitSpinUpBase


        'xxxxxxxxxxxxxxx USING SPIN-UP xxxxxxxxxxxxxxxxxxxxxx'
        If Me.m_Data.UseSpinUp And Not Me.m_Data.UseSpinUpBase Then
            'NOT using the Spin-Up base values
            'Base values from the first timestep after the Spin-Up

            'HACK These values will get set at each step of the Spin-Up 
            'Not just the first timestep of the run
            For igrp = 1 To Me.m_Data.NGroups
                Me.m_Data.BBase(igrp) = Btime(igrp)

                'Use the first timestep as the base
                Me.m_Data.BaseFishMort(igrp) = Me.m_Data.ResultsByGroup(eSpaceResultsGroups.FishingMort, igrp, 1)
                Me.m_Data.BaseConsump(igrp) = Me.m_Data.ResultsByGroup(eSpaceResultsGroups.ConsumpRate, igrp, 1)
                Me.m_Data.BasePredMort(igrp) = Me.m_Data.ResultsByGroup(eSpaceResultsGroups.PredMortRate, igrp, 1)
                Me.m_Data.BaseCatch(igrp) = Me.m_Data.ResultsByGroup(eSpaceResultsGroups.CatchBio, igrp, 1)

            Next igrp
        End If 'Me.m_Data.UseSpinUp And Not Me.m_Data.UseSpinUpBase

        For igrp = 1 To Me.m_Data.NGroups
            If Me.m_Data.BBase(igrp) = 0.0 Then Me.m_Data.BBase(igrp) = 1.0E-20
            If Me.m_Data.BaseFishMort(igrp) = 0.0 Then Me.m_Data.BaseFishMort(igrp) = 1.0E-20
            If Me.m_Data.BaseConsump(igrp) = 0 Then Me.m_Data.BaseConsump(igrp) = 1.0E-20
            If Me.m_Data.BasePredMort(igrp) = 0 Then Me.m_Data.BasePredMort(igrp) = 1.0E-20
            If Me.m_Data.BaseCatch(igrp) = 0 Then Me.m_Data.BaseCatch(igrp) = 1.0E-20
        Next igrp

    End Sub

#End Region

#Region "Depreciated Code"

#If 0 Then
    ''' <summary>
    ''' Threaded Version
    ''' This routine predicts spatial effort and fishing mortality rate
    ''' distribution by gear type; called at each iteration
    ''' step in finding biomass spatial equilibrium
    ''' model below is a gravity attraction model, distributing
    ''' total efforts TotEffort(gear) over all cells where each gear can fish
    ''' in proportion to relative profitability (catch rate x price sum) for that cell for the gear
    ''' </summary>
    ''' <remarks></remarks>
    Sub PredictEffortDistributionThreaded_NoZones(ByVal obParam As Object)
        Dim i As Integer, j As Integer, TotAttract As Single
        Dim Valt As Single, isp As Integer
        Dim EffortCost As Single
        Dim SailCost As Single
        Dim TotE As Single
        Dim Attract(,) As Single
        Dim arguments As cThreadedCallArgs
        Dim nEffCells As Integer, sumEff As Single

        Dim stpwtch As Stopwatch

        Try

            arguments = DirectCast(obParam, cThreadedCallArgs)
            'Make sure the number of fleets is in bounds
            'This could happen because of rounding error in the number of fleets per thread
            If arguments.iFirst <= Me.m_Data.nFleets Then

                'Dim thrdID As Integer = Threading.Thread.CurrentThread.ManagedThreadId

                'Console.WriteLine("Effort Distribution , ThreadID = " & thrdID.ToString & ", Start T = " & DateTime.Now.ToLongTimeString)
                'Console.WriteLine("  N Fleets = " & (arguments.iLast - arguments.iFirst + 1).ToString)
                stpwtch = Stopwatch.StartNew

                ReDim Attract(m_Data.InRow, m_Data.InCol)

                For iFlt As Integer = arguments.iFirst To arguments.iLast
                    'check the bounds
                    If (iFlt < 1) Or (iFlt > Me.m_Data.nFleets) Then Exit For
                    'System.Console.WriteLine("  Fleet " & iFlt.ToString)

                    For iArea As Integer = 1 To Me.m_Data.nEffZones
                        TotE = TotEffort(iFlt) * m_Data.SEmult(iFlt) * Me.m_Data.PropEffortFleetZone(iFlt, iArea)

                        'jb Attract() gets cleared out for each fleet
                        Array.Clear(Attract, 0, Attract.Length)
                        TotAttract = 0.0000000001

                        'Introduce a factor which balances fixed and sailingcost: (up to 02Jan02 the next if then was in the loop over spatial cells below, no need for this)
                        If m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail) = 0 Then
                            EffortCost = 0
                            SailCost = 1
                        Else
                            EffortCost = m_EPdata.cost(iFlt, eCostIndex.CUPE) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
                            SailCost = m_EPdata.cost(iFlt, eCostIndex.Sail) / (m_EPdata.cost(iFlt, eCostIndex.Fixed) + m_EPdata.cost(iFlt, eCostIndex.CUPE) + m_EPdata.cost(iFlt, eCostIndex.Sail))
                        End If

                        '
                        For i = 1 To m_Data.InRow
                            For j = 1 To m_Data.InCol
                                'Moved to InitSpatialEquilibrium
                                If Me.m_Data.IsFished(iFlt, i, j) And iArea = Me.m_Data.EffZones(i, j) Then
                                    'Water and (Not closed by MPA) and (Fished by this gear)
                                    'mpamonth(Month, MPAType) is false if closed, True if open.
                                    Valt = 0
                                    For isp = 1 To m_Data.NGroups
                                        Valt = Valt + m_EPdata.Market(iFlt, isp) * m_Data.Bcell(i, j, isp) * m_SimData.relQ(iFlt, isp)
                                    Next
                                    'Debug.Assert(Not Single.IsNaN(Valt))
                                    'jb Move to InitSpatialEquilibrium()
                                    ' If m_Data.Sail(iFlt, i, j) = 0 Then m_Data.Sail(iFlt, i, j) = 0.000001

                                    'VC Sail() above: to avoid dividing with zero
                                    Valt = (Valt ^ m_Data.EffPower(iFlt)) / (EffortCost + SailCost * m_Data.Sail(iFlt, i, j) / m_Data.SailScale(iFlt))
                                    Attract(i, j) = Valt * Me.m_Data.PAreaFished(i, j, iFlt) 'may want to modify this by dividing by a site cost factor for cell i,j
                                    TotAttract += Attract(i, j) ' TotAttract + Valt * Me.m_Data.PAreaFished(i, j, iFlt)
                                End If 'Me.m_Data.IsFished(iFlt, i, j)
                            Next j
                        Next i

                        sumEff = 0
                        nEffCells = 0
                        For i = 1 To m_Data.InRow
                            For j = 1 To m_Data.InCol
                                'VC19Aug98: Fishing in water, not in MPA unless the MPA is fished, and only if this gear operate in this habitat or in all habitats
                                If Me.m_Data.IsFished(iFlt, i, j) And iArea = Me.m_Data.EffZones(i, j) Then

                                    'VC/080499 Above changed per CJWs advice to reflect effort change over time in Ecospace
                                    m_Data.EffortSpace(iFlt, i, j) = m_SimData.FishRateGear(iFlt, arguments.iCumMonth) * TotE * Attract(i, j) / TotAttract * Me.m_Data.PropEffortFleetZone(iFlt, iArea)  'propfleeteffort in this lme
                                    sumEff += m_Data.EffortSpace(iFlt, i, j)
                                    nEffCells += 1 ' Me.m_Data.PAreaFished(i, j, iFlt)

                                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                    'jb 19-July-2012 moved summing of fishing mortality out of the distribution threads
                                    'this stops the threading bug caused when different threads try to sum F at the same time resulting in different F (Ftot(,,,))
                                    '        For isp = 1 To m_Data.NGroups
                                    '            'Fishing Mort
                                    '            m_Data.Ftot(isp, i, j) = m_Data.Ftot(isp, i, j) + m_Data.EffortSpace(iFlt, i, j) * m_SimData.relQ(iFlt, isp) / Me.m_Data.PAreaFished(i, j, iFlt)
                                    '        Next isp
                                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                                End If
                            Next j
                        Next i

                        If nEffCells = 0 Then nEffCells = 1
                        Console.WriteLine("Fleet," + iFlt.ToString + ",avg effort," + (sumEff / nEffCells).ToString + ",Sim Effort," + m_SimData.FishRateGear(iFlt, arguments.iCumMonth).ToString)

                    Next iArea

                Next iFlt

            Else ' If arguments.iFirst <= Me.m_Data.nFleets Then
                'First Fleet Index > Number of Fleets
                'We still need to Decrement the Interlock counter
                System.Console.WriteLine("Effort Dist No fleets to process = " & cEcoSpace.m_ThreadIncrementCount.ToString)
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        If Interlocked.Decrement(cEcoSpace.m_ThreadIncrementCount) = 0 Then
            arguments.WaitHandle.Set()
        End If

        'System.Console.WriteLine("Effort Dist Increment Lock = " & cEcoSpace.m_ThreadIncrementCount.ToString)

    End Sub

    Private Sub SetVtot(ByVal XvTot(,) As Single, ByVal YvTot(,) As Single, ByVal Corio As Single, ByVal Hstress As Single)
        'sets total pressure in x and y directions for all cells
        Dim i As Integer, j As Integer
        For i = 0 To Me.m_Data.InRow + 1
            For j = 0 To Me.m_Data.InCol + 1
                If Me.m_Data.Depth(i, j) > 0 Then
                    XvTot(i, j) = Me.m_Data.Xvloc(i, j)
                    YvTot(i, j) = Me.m_Data.Yvloc(i, j)
                End If
            Next
        Next
        'add force components due to horizontal shear along box sides
        For i = 1 To Me.m_Data.InRow
            For j = 1 To Me.m_Data.InCol
                If Me.m_Data.Depth(i, j) > 0 Then
                    XvTot(i, j) = XvTot(i, j) - Corio * Me.m_Data.Yvel(i, j) + Hstress * (Me.m_Data.Xvel(i - 1, j) + Me.m_Data.Xvel(i + 1, j) - 2.0# * Me.m_Data.Xvel(i, j))
                    YvTot(i, j) = YvTot(i, j) + Corio * Me.m_Data.Xvel(i, j) + Hstress * (Me.m_Data.Yvel(i, j - 1) + Me.m_Data.Yvel(i, j + 1) - 2.0# * Me.m_Data.Yvel(i, j))
                End If
            Next
        Next
    End Sub

    Private Sub SetVelocities(ByRef vel(,) As Single, _
                              ByVal SorWv As Single, ByVal Grav As Single, ByVal UpWell As Single, _
                              ByVal XvToT(,) As Single, ByVal YvTot(,) As Single)
        Dim i As Integer
        Dim j As Integer
        For i = 0 To Me.m_Data.InRow
            For j = 0 To Me.m_Data.InCol
                If Me.m_Data.Depth(i, j) > 0 Then
                    If Me.m_Data.Depth(i, j + 1) > 0 Then Me.m_Data.Xvel(i, j) = (1 - SorWv) * Me.m_Data.Xvel(i, j) + SorWv * Me.m_Data.DepthX(i, j) * (XvToT(i, j) + Grav * (vel(i, j) - vel(i, j + 1))) Else Me.m_Data.Xvel(i, j) = 0
                    If Me.m_Data.Depth(i + 1, j) > 0 Then Me.m_Data.Yvel(i, j) = (1 - SorWv) * Me.m_Data.Yvel(i, j) + SorWv * Me.m_Data.DepthY(i, j) * (YvTot(i, j) + Grav * (vel(i, j) - vel(i + 1, j))) Else Me.m_Data.Yvel(i, j) = 0
                    Me.m_Data.UpVel(i, j) = -UpWell * Me.m_Data.DepthA(i, j) * vel(i, j)
                Else
                    Me.m_Data.Xvel(i, j) = 0
                    Me.m_Data.Yvel(i, j) = 0
                End If
            Next
        Next
    End Sub


     Sub VaryMigMovementParameters(ByVal imonth As Integer)
        'sets solvegrid movement arrays based on depth map
        Dim i As Integer, j As Integer, ip As Integer, AdScale As Single ', iad As Integer, iju As Integer
        Dim isp As Integer, ist As Integer, nvar2 As Integer, ir As Integer, ieco As Integer
        Dim imig As Integer
        Dim nMig As Integer
        Dim migIndex() As Integer
        'Dim distortNS As Single
        'Dim distortEW As Single
        Dim distort As Single
        Try

            ReDim migIndex(m_Data.NGroups)
            For i = 1 To m_Data.NGroups
                If m_Data.IsMigratory(i) Then
                    nMig = nMig + 1
                    migIndex(nMig) = i
                End If
            Next

            AdScale = 1 / m_Data.CellLength '/ (2 * 3.14159 * CellLength)
            For i = 0 To m_Data.InRow
                For j = 0 To m_Data.InCol
                    'check depth on right face of this cell
                    If m_Data.Depth(i, j) > 0 Then
                        If m_Data.Depth(i, j + 1) > 0 Then
                            For imig = 1 To nMig
                                ip = migIndex(imig)
                                If MigPowj(ip, j) > 0 Then
                                    distort = 1 * MigPowj(ip, j) / (PrefColP(ip, imonth) + MigPowj(ip, j))
                                Else
                                    distort = 0.5
                                End If
                                If j > 0 And j < m_Data.InCol Then
                                    e(i, j + 1, ip) = Enomig(i, j + 1, ip) * RelMove(ip, i, j + 1) * RelMigMove(i, j + 1, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip) * distort
                                    d(i, j, ip) = dNomig(i, j, ip) * RelMove(ip, i, j) * RelMigMove(i, j, i, j + 1, MigGrad, m_Data.MoveScale, imig, imonth, ip) * (1 - distort)
                                    If m_Data.IsAdvected(ip) Then
                                        If m_Data.Xvel(i, j) > 0 Then
                                            d(i, j, ip) = d(i, j, ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                        Else
                                            e(i, j + 1, ip) = e(i, j + 1, ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                        End If

                                    End If
                                Else
                                    If m_Data.IsAdvected(ip) Then
                                        If m_Data.Xvel(i, j) > 0 Then
                                            e(i, j + 1, ip) = m_Data.Mrate(ip) 'into j from right
                                            d(i, j, ip) = m_Data.Mrate(ip) + m_Data.Xvel(i, j) * AdScale 'from j to the right
                                        Else
                                            e(i, j + 1, ip) = m_Data.Mrate(ip) - m_Data.Xvel(i, j) * AdScale 'into j from right
                                            d(i, j, ip) = m_Data.Mrate(ip) 'from j to the right

                                        End If
                                    Else
                                        e(i, j + 1, ip) = 0
                                        d(i, j, ip) = 0
                                    End If
                                End If
                                'Enomig(i, j + 1, ip) = e(i, j + 1, ip)
                                'dNomig(i, j, ip) = d(i, j, ip)
                            Next

                            nvar2 = m_Data.NGroups
                            ir = 0
                            For isp = 1 To m_Stanza.Nsplit
                                For ist = 1 To m_Stanza.Nstanza(isp)
                                    ieco = m_Stanza.EcopathCode(isp, ist)
                                    ir = ir + 1
                                    e(i, j + 1, nvar2 + ir) = e(i, j + 1, ieco)
                                    d(i, j, nvar2 + ir) = d(i, j, ieco)
                                    'Enomig(i, j + 1, nvar2 + ir) = e(i, j + 1, ieco)
                                    'dNomig(i, j, nvar2 + ir) = d(i, j, ieco)
                                Next
                            Next
                        End If
                        'then check depths on bottom face of this cell
                        If m_Data.Depth(i + 1, j) > 0 Then
                            For imig = 1 To nMig
                                If i > 0 And i < m_Data.InRow Then
                                    ip = migIndex(imig)
                                    C(i, j, ip) = CNomig(i, j, ip) * RelMove(ip, i + 1, j) * RelMigMove(i + 1, j, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip) * distort
                                    Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) * RelMove(ip, i, j) * RelMigMove(i, j, i + 1, j, MigGrad, m_Data.MoveScale, imig, imonth, ip) * (1 - distort)
                                    If m_Data.IsAdvected(ip) Then
                                        If m_Data.Yvel(i, j) > 0 Then
                                            Bcw(i + 1, j, ip) = Bcw(i + 1, j, ip) + m_Data.Yvel(i, j) * AdScale 'from j to the right
                                        Else
                                            C(i, j, ip) = C(i, j, ip) - m_Data.Yvel(i, j) * AdScale   'into j from right
                                        End If

                                    End If
                                Else
                                    If m_Data.IsAdvected(ip) Then
                                        If m_Data.Yvel(i, j) > 0 Then
                                            C(i, j, ip) = m_Data.Mrate(ip) 'from row i+1 to i
                                            Bcw(i + 1, j, ip) = m_Data.Mrate(ip) + m_Data.Yvel(i, j) * AdScale ' + AdvectSouth 'from i to i+1
                                        Else
                                            C(i, j, ip) = m_Data.Mrate(ip) - m_Data.Yvel(i, j) * AdScale 'from row i+1 to i
                                            Bcw(i + 1, j, ip) = m_Data.Mrate(ip)
                                        End If
                                    Else
                                        C(i, j, ip) = 0
                                        Bcw(i + 1, j, ip) = 0
                                    End If
                                End If
                                'CNomig(i, j, ip) = C(i, j, ip)
                                'BcwNomig(i + 1, j, ip) = Bcw(i + 1, j, ip)
                            Next

                            nvar2 = m_Data.NGroups
                            ir = 0
                            For isp = 1 To m_Stanza.Nsplit
                                For ist = 1 To m_Stanza.Nstanza(isp)
                                    ieco = m_Stanza.EcopathCode(isp, ist)
                                    ir = ir + 1
                                    Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ieco)
                                    C(i, j, nvar2 + ir) = C(i, j, ieco)
                                    BcwNomig(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ieco)
                                    CNomig(i, j, nvar2 + ir) = C(i, j, ieco)
                                Next
                            Next
                        End If
                    End If

                Next j
            Next i

            If m_tracerData.EcoSpaceConSimOn Then
                'set movement rates for physical contaminant concentration to
                'rates for first detritus pool
                For i = 0 To m_Data.InRow + 1
                    For j = 0 To m_Data.InCol + 1
                        Bcw(i, j, 0) = Bcw(i, j, m_EPdata.NumLiving + 1)
                        C(i, j, 0) = C(i, j, m_EPdata.NumLiving + 1)
                        d(i, j, 0) = d(i, j, m_EPdata.NumLiving + 1)
                        e(i, j, 0) = e(i, j, m_EPdata.NumLiving + 1)
                        BcwNomig(i, j, 0) = Bcw(i, j, m_EPdata.NumLiving + 1)
                        CNomig(i, j, 0) = C(i, j, m_EPdata.NumLiving + 1)
                        dNomig(i, j, 0) = d(i, j, m_EPdata.NumLiving + 1)
                        Enomig(i, j, 0) = e(i, j, m_EPdata.NumLiving + 1)
                    Next
                Next
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub


    Sub VaryMovementParameters(ByVal imonth As Integer, ByVal ip As Integer, ByVal ieco As Integer)
        'EwE5 definition IsIad and IsIju indexes remove these are iAdult and iJuvenial indexes for the split pool code
        'Sub VaryMovementParameters(ByVal imonth As Integer, ByVal ip As Integer, ByVal IsIad As Integer, ByVal IsIju As Integer, ByVal ieco As Integer)

        'sets solvegrid movement arrays based on depth map
        Dim i As Integer, j As Integer, AdScale As Single
        Dim nvar2 As Integer, ir As Integer, Distort As Single
        Dim Ep As Single
        Dim MaxCh As Single
        Dim FitRatio As Single
        AdScale = 1 '/ (2 * 3.14159 * CellLength)
        MaxCh = 1

        'calculate relative emigration rate from each cell as function
        'of fitness, scaling parameter KmoveFit(ip) set in setKmove routine
        For i = 0 To m_Data.InRow + 1
            For j = 0 To m_Data.InCol + 1
                If m_Data.FitRespType > 0 Then
                    Ep = -Kmovefit(ip) * RelFitness(i, j, ip)
                    If Ep < -MaxCh Then Ep = -MaxCh
                    If Ep > MaxCh Then Ep = MaxCh
                    Ep = Math.Exp(Ep)
                    RelMoveFit(i, j) = 2.0# * Ep / (1 + Ep)
                    '        If ip = 18 And imonth > 1 And i > 30 And i <  m_data.inrow + 1 And j > 1 And j < 5 Then Stop
                Else
                    RelMoveFit(i, j) = 1
                End If
            Next
        Next

        For i = 0 To m_Data.InRow
            For j = 0 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then

                    'check depth on right face of this cell
                    If m_Data.Depth(i, j + 1) > 0 Then

                        If MigPowj(ip, j) > 0 Then
                            Distort = 2 * MigPowj(ip, j) / (PrefColP(ip, imonth) + MigPowj(ip, j))
                        Else
                            Distort = 1
                        End If

                        If m_Data.FitRespType < 2 Then
                            e(i, j + 1, ip) = Enomig(i, j + 1, ip) * RelMoveFit(i, j + 1) * (Distort)
                            d(i, j, ip) = dNomig(i, j, ip) * RelMoveFit(i, j) * (2 - Distort)
                        Else
                            FitRatio = RelMoveFit(i, j + 1) / RelMoveFit(i, j)
                            e(i, j + 1, ip) = Enomig(i, j + 1, ip) * FitRatio * (Distort)
                            d(i, j, ip) = dNomig(i, j, ip) / FitRatio * (2 - Distort)
                        End If

                        If j = 0 Or j = m_Data.InCol Then
                            e(i, j + 1, ip) = 0
                            d(i, j, ip) = 0
                        End If

                        'jb split pool code removed
                        'If IsIad > 0 Then
                        '    e(i, j + 1, nvar + IsIad) = e(i, j + 1, ip)
                        '    d(i, j, nvar + IsIad) = d(i, j, ip)
                        'End If
                        'If IsIju > 0 Then
                        '    e(i, j + 1, nvar + npairs + IsIju) = e(i, j + 1, ip)
                        '    d(i, j, nvar + npairs + IsIju) = d(i, j, ip)
                        'End If

                        nvar2 = m_Data.NGroups
                        If ieco > 0 Then
                            ir = IecoCode(ip)
                            e(i, j + 1, nvar2 + ir) = e(i, j + 1, ip)
                            d(i, j, nvar2 + ir) = d(i, j, ip)
                            'Enomig(i, j + 1, nvar2 + ir) = E(i, j + 1, ieco)
                            'dNomig(i, j, nvar2 + ir) = d(i, j, ieco)
                        End If
                    End If ' If m_Data.Depth(i, j + 1) > 0 Then check depth on right face of this cell

                    'then check depths on bottom face of this cell
                    If m_Data.Depth(i + 1, j) > 0 Then
                        If MigPowi(ip, i) > 0 Then
                            Distort = 2 * MigPowi(ip, i) / (PrefRowP(ip, imonth) + MigPowi(ip, i))
                        Else
                            Distort = 1
                        End If

                        If m_Data.FitRespType < 2 Then
                            C(i, j, ip) = CNomig(i, j, ip) * Distort * RelMoveFit(i + 1, j)
                            Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) * RelMoveFit(i, j) * (2 - Distort)
                        Else
                            FitRatio = RelMoveFit(i + 1, j) / RelMoveFit(i, j)
                            C(i, j, ip) = CNomig(i, j, ip) * Distort * FitRatio
                            Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) / FitRatio * (2 - Distort)
                        End If

                        If i = 0 Or i = m_Data.InRow Then
                            C(i, j, ip) = 0
                            Bcw(i + 1, j, ip) = 0
                        End If

                        ''jb split pool code removed
                        'If IsIad > 0 Then
                        '    Bcw(i + 1, j, nvar + IsIad) = Bcw(i + 1, j, ip)
                        '    C(i, j, nvar + IsIad) = C(i, j, ip)
                        'End If
                        'If IsIju > 0 Then
                        '    Bcw(i + 1, j, nvar + npairs + IsIju) = Bcw(i + 1, j, ip)
                        '    C(i, j, nvar + npairs + IsIju) = C(i, j, ip)
                        'End If

                        If ieco > 0 Then
                            ir = ieco
                            Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ip)
                            C(i, j, nvar2 + ir) = C(i, j, ip)
                        End If
                    End If 'If m_Data.Depth(i + 1, j) > 0 Then then check depths on bottom face of this cell

                End If 'If m_Data.Depth(i, j) > 0 Then

            Next j
        Next i

    End Sub


    Sub VaryMigMovementParameters_SinglePoint(ByVal imonth As Integer)
        '20-Jan-2016 Original code to set migration movement based on a single point
        'Update to set migration movement based on an area

        Dim ip As Integer
        'sets solvegrid movement arrays based on depth map
        Dim i As Integer, j As Integer, AdScale As Single
        Dim nvar2 As Integer, ir As Integer, Distort As Single
        Dim Ep As Single
        Dim MaxCh As Single
        Dim FitRatio As Single
        AdScale = 1 '/ (2 * 3.14159 * CellLength)
        MaxCh = 1
        Dim ieco As Integer
        Dim imig As Integer
        Dim nMig As Integer
        Dim migIndex() As Integer
        Dim migGradWeight As Single = 0.05

        ReDim migIndex(m_Data.NGroups)
        For i = 1 To m_Data.NGroups
            If m_Data.IsMigratory(i) Then
                nMig = nMig + 1
                migIndex(nMig) = i
            End If
        Next

        For imig = 1 To nMig
            ip = migIndex(imig)
            ieco = IecoCode(ip)
            'calculate relative emigration rate from each cell as function
            'of fitness, scaling parameter KmoveFit(ip) set in setKmove routine
            For i = 0 To m_Data.InRow + 1
                For j = 0 To m_Data.InCol + 1
                    If m_Data.FitRespType > 0 Then
                        Ep = -Kmovefit(ip) * RelFitness(i, j, ip)
                        If Ep < -MaxCh Then Ep = -MaxCh
                        If Ep > MaxCh Then Ep = MaxCh
                        Ep = Math.Exp(Ep)
                        RelMoveFit(i, j) = 2.0# * Ep / (1 + Ep)
                    Else
                        RelMoveFit(i, j) = 1
                    End If
                Next
            Next

            For i = 0 To m_Data.InRow
                For j = 0 To m_Data.InCol
                    If m_Data.Depth(i, j) > 0 Then

                        'check depth on right face of this cell
                        If m_Data.Depth(i, j + 1) > 0 Then

                            If MigPowj(ip, j) > 0 Then
                                Distort = 2 * MigPowj(ip, j) / (PrefColP(ip, imonth) + MigPowj(ip, j))
                            Else
                                Distort = 1
                            End If
                            'Distort = 1 + Distort / 4

                            If m_Data.FitRespType < 2 Then
                                e(i, j + 1, ip) = Enomig(i, j + 1, ip) * RelMoveFit(i, j + 1) * RelMigMove(i, j + 1, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip) * (Distort) '* (MigGrad(i, j, imig, imonth) + 1) / m_Data.InCol
                                d(i, j, ip) = dNomig(i, j, ip) * RelMoveFit(i, j) * RelMigMove(i, j, i, j + 1, MigGrad, m_Data.MoveScale, imig, imonth, ip) * (2 - Distort) '* (MigGrad(i, j, imig, imonth) + 1) / m_Data.InCol
                            Else
                                FitRatio = RelMoveFit(i, j + 1) / RelMoveFit(i, j)
                                e(i, j + 1, ip) = Enomig(i, j + 1, ip) * FitRatio * (Distort) * RelMigMove(i, j + 1, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
                                d(i, j, ip) = dNomig(i, j, ip) / FitRatio * (2 - Distort) * RelMigMove(i, j, i, j + 1, MigGrad, m_Data.MoveScale, imig, imonth, ip)
                            End If

                            If j = 0 Or j = m_Data.InCol Then
                                e(i, j + 1, ip) = 0
                                d(i, j, ip) = 0
                            End If

                            nvar2 = m_Data.NGroups
                            If ieco > 0 Then
                                ir = IecoCode(ip)
                                e(i, j + 1, nvar2 + ir) = e(i, j + 1, ip)
                                d(i, j, nvar2 + ir) = d(i, j, ip)
                            End If
                        End If ' If m_Data.Depth(i, j + 1) > 0 Then check depth on right face of this cell

                        'then check depths on bottom face of this cell
                        If m_Data.Depth(i + 1, j) > 0 Then
                            If MigPowi(ip, i) > 0 Then
                                Distort = 2 * MigPowi(ip, i) / (PrefRowP(ip, imonth) + MigPowi(ip, i))
                            Else
                                Distort = 1
                            End If
                            'Distort = 1 + Distort / 2

                            If m_Data.FitRespType < 2 Then
                                C(i, j, ip) = CNomig(i, j, ip) * RelMoveFit(i + 1, j) * RelMigMove(i + 1, j, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip) * Distort '* (MigGrad(i, j, imig, imonth) + 1) / m_Data.Inrow
                                Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) * RelMoveFit(i, j) * RelMigMove(i, j, i + 1, j, MigGrad, m_Data.MoveScale, imig, imonth, ip) * (2 - Distort) '* (MigGrad(i, j, imig, imonth) + 1) / m_Data.Inrow
                            Else
                                FitRatio = RelMoveFit(i + 1, j) / RelMoveFit(i, j)
                                C(i, j, ip) = CNomig(i, j, ip) * Distort * FitRatio * RelMigMove(i + 1, j, i, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
                                Bcw(i + 1, j, ip) = BcwNomig(i + 1, j, ip) / FitRatio * (2 - Distort) * RelMigMove(i, j, i + 1, j, MigGrad, m_Data.MoveScale, imig, imonth, ip)
                            End If

                            If i = 0 Or i = m_Data.InRow Then
                                C(i, j, ip) = 0
                                Bcw(i + 1, j, ip) = 0
                            End If

                            If ieco > 0 Then
                                ir = ieco
                                Bcw(i + 1, j, nvar2 + ir) = Bcw(i + 1, j, ip)
                                C(i, j, nvar2 + ir) = C(i, j, ip)
                            End If

                        End If 'If m_Data.Depth(i + 1, j) > 0 Then then check depths on bottom face of this cell

                    End If 'If m_Data.Depth(i, j) > 0 Then

                Next j
            Next i
        Next imig
    End Sub


    Private Sub SetMigGrad_SinglePoint()
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        '20-Jan-2016 original code to set MigGrad(),migration movement gradients, base on a preferred row and col/ single cell
        'change to use the migration areas
        'Not used any more but left in for a reference
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'set habitat quality gradient maps for all habitat types, for use in biased movement assessments
        Dim i As Integer, j As Integer, ii As Integer, jj As Integer, iMigGrp As Integer
        Dim i1 As Integer, i2 As Integer, j1 As Integer, j2 As Integer, Sweep As Integer, imonth As Integer
        Dim nsweep As Integer
        Dim smallestDist As Single
        Dim pathFound As Integer

        Dim nMig As Integer
        Dim migIndex() As Integer
        ReDim migIndex(m_Data.NGroups)
        Dim diagAdjust As Single
        'Dim diagAdjustFinal As Single

        Try
            For i = 1 To m_Data.NGroups
                If m_Data.IsMigratory(i) Then
                    nMig = nMig + 1
                    migIndex(nMig) = i
                End If
            Next
            ReDim MigGrad(m_Data.InRow + 1, m_Data.InCol + 1, nMig, 12)

            If m_Data.InRow > m_Data.InCol Then nsweep = m_Data.InRow Else nsweep = m_Data.InCol
            nsweep = nsweep * 2
            iWindow = 1
            For iMigGrp = 1 To nMig
                For imonth = 1 To 12
                    For Sweep = 1 To nsweep
                        For i = 0 To m_Data.InRow + 1
                            For j = 0 To m_Data.InCol + 1
                                If Sweep = 1 Then
                                    MigGrad(i, j, iMigGrp, imonth) = 1000
                                ElseIf MigGrad(i, j, iMigGrp, imonth) <> 0 Then
                                    smallestDist = 2000
                                    diagAdjust = 0
                                    'smallesti = -1
                                    'smallestJ = -1
                                    pathFound = False
                                    i1 = i - iWindow : If i1 < 0 Then i1 = 0
                                    i2 = i + iWindow : If i2 > m_Data.InRow + 1 Then i2 = m_Data.InRow + 1
                                    j1 = j - iWindow : If j1 < 0 Then j1 = 0
                                    j2 = j + iWindow : If j2 > m_Data.InCol + 1 Then j2 = m_Data.InCol + 1
                                    For ii = i1 To i2 : For jj = j1 To j2
                                            If ii = i Or jj = j Then
                                                diagAdjust = 0
                                            Else
                                                diagAdjust = 0.4142 'sqrt(2)-1
                                            End If

                                            If MigGrad(ii, jj, iMigGrp, imonth) + diagAdjust < smallestDist And ((m_Data.Depth(i, j) > 0 And m_Data.HabCap(migIndex(iMigGrp))(i, j) > 0.1) Or i = 0 Or i = m_Data.InRow + 1 Or j = 0 Or j = m_Data.InCol + 1) Then
                                                smallestDist = MigGrad(ii, jj, iMigGrp, imonth) + diagAdjust
                                                pathFound = True
                                            End If
                                        Next
                                    Next
                                    If pathFound Then
                                        MigGrad(i, j, iMigGrp, imonth) = smallestDist + 1
                                    End If

                                End If 'Sweep = 1

                                If m_Data.Depth(i, j) = 0 Or m_Data.HabCap(migIndex(iMigGrp))(i, j) < 0.1 Then
                                    MigGrad(i, j, iMigGrp, imonth) = 2000
                                End If

                            Next j
                        Next i
                        If Sweep = 1 Then
                            i = Math.Max(0, Math.Min(Me.m_Data.InRow, m_Data.PrefRow(migIndex(iMigGrp), imonth)))
                            j = Math.Max(0, Math.Min(Me.m_Data.InCol, m_Data.Prefcol(migIndex(iMigGrp), imonth)))
                            MigGrad(i, j, iMigGrp, imonth) = 0
                        End If
                    Next Sweep 'Sweep = 1 To nsweep
                Next imonth 'imonth = 1 To 12
            Next iMigGrp 'iMigGrp = 1 To nMig

            Dim tempstr As String
            For iMigGrp = 1 To 1 'nMig 'm_Data.NGroups
                For imonth = 1 To 12
                    Debug.Print("")
                    Debug.Print("imonth = " + imonth.ToString)
                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1
                            If Math.Round(MigGrad(i, j, iMigGrp, imonth)) < 100 Or MigGrad(i, j, iMigGrp, imonth) = 2000 Then
                                tempstr = tempstr + " "
                                If Math.Round(MigGrad(i, j, iMigGrp, imonth)) < 10 Or MigGrad(i, j, iMigGrp, imonth) = 2000 Then
                                    tempstr = tempstr + " "
                                End If
                            End If
                            If MigGrad(i, j, iMigGrp, imonth) >= 0 And MigGrad(i, j, iMigGrp, imonth) < 2000 Then
                                tempstr = tempstr + Math.Round(MigGrad(i, j, iMigGrp, imonth)).ToString + " "
                                'ElseIf MigGrad(i, j, ihab, imonth) = 0 Then
                                '    tempstr = tempstr + "X "
                            Else
                                tempstr = tempstr + "  "
                            End If
                        Next
                        Debug.Print(tempstr)
                        tempstr = ""
                    Next
                Next
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub


    Private Sub SetMigGrad_Distance()
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        '20-Jan-2016 This was for debugging the Area based migration movements 
        'based on cell distance from centroid of area
        'Not used here
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'set habitat quality gradient maps for all habitat types, for use in biased movement assessments
        Dim i As Integer, j As Integer, ii As Integer, jj As Integer, iMigGrp As Integer, imonth As Integer
        'Dim i1 As Integer, i2 As Integer, j1 As Integer, j2 As Integer, Sweep As Integer, 
        'Dim nsweep As Integer
        'Dim smallestDist As Single
        'Dim pathFound As Integer

        Dim nMig As Integer
        Dim migIndex() As Integer
        ReDim migIndex(m_Data.NGroups)
        'Dim diagAdjust As Single

        '  Me.m_Data.debugSetMigMapsFromPrefRowCol()

        Me.m_Data.calcPrefRowColFromMigrationMap()

        Try
            For i = 1 To m_Data.NGroups
                If m_Data.IsMigratory(i) Then
                    nMig = nMig + 1
                    migIndex(nMig) = i
                End If
            Next
            ReDim MigGrad(m_Data.InRow + 1, m_Data.InCol + 1, nMig, 12)

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Initialize the MigGrad(row,col,group,month) migration gradient matrix with
            '0 for cells inside a migration area
            '1000 for cells outside migration area
            '2000 for cells in low capacity habitat or land
            For iMigGrp = 1 To nMig
                For imonth = 1 To 12

                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1

                            MigGrad(i, j, iMigGrp, imonth) = 1000

                            If m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > 0 Then
                                MigGrad(i, j, iMigGrp, imonth) = 0
                            End If

                            If m_Data.Depth(i, j) = 0 Or m_Data.HabCap(migIndex(iMigGrp))(i, j) < 0.1 Then
                                MigGrad(i, j, iMigGrp, imonth) = 2000
                            End If

                        Next j
                    Next i
                Next imonth
            Next iMigGrp
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            Dim dist As Double

            For iMigGrp = 1 To nMig
                For imonth = 1 To 12

                    Dim row As Integer = Me.m_Data.PrefRow(migIndex(iMigGrp), imonth)
                    Dim col As Integer = Me.m_Data.Prefcol(migIndex(iMigGrp), imonth)

                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1

                            If Not m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > 0 Then
                                dist = Math.Sqrt((j - col) ^ 2 + (i - row) ^ 2)
                                MigGrad(i, j, iMigGrp, imonth) = dist
                            Else
                                MigGrad(i, j, iMigGrp, imonth) = 0
                            End If

                            If m_Data.Depth(i, j) = 0 Or m_Data.HabCap(migIndex(iMigGrp))(i, j) < 0.1 Then
                                MigGrad(i, j, iMigGrp, imonth) = 2000
                            End If

                        Next j
                    Next i
                Next imonth
            Next iMigGrp



            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'DUBUGGING
            'Dump the migration maps to the debug/immediate window
            Dim tempstr As String
            For iMigGrp = 1 To 1 'nMig 'm_Data.NGroups
                For imonth = 1 To 12
                    Debug.Print("")
                    Debug.Print("imonth = " + imonth.ToString)
                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1
                            tempstr = tempstr + Math.Round(MigGrad(i, j, iMigGrp, imonth), 3).ToString.PadRight(10)

                        Next j
                        Debug.Print(tempstr)
                        tempstr = ""
                    Next i
                Next imonth
            Next iMigGrp
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub



    Private Sub SetMigGrad_Distance()
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        '20-Jan-2016 This was for debugging the Area based migration movements 
        'based on cell distance from centroid of area
        'Not used here
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'set habitat quality gradient maps for all habitat types, for use in biased movement assessments
        Dim i As Integer, j As Integer, iMigGrp As Integer, imonth As Integer
        'Dim i1 As Integer, i2 As Integer, j1 As Integer, j2 As Integer, Sweep As Integer, 
        'Dim nsweep As Integer
        'Dim smallestDist As Single
        'Dim pathFound As Integer

        Dim nMig As Integer
        Dim migIndex() As Integer
        ReDim migIndex(m_Data.NGroups)
        'Dim diagAdjust As Single

        '  Me.m_Data.debugSetMigMapsFromPrefRowCol()

        Me.m_Data.calcPrefRowColFromMigrationMap()

        Try
            For i = 1 To m_Data.NGroups
                If m_Data.IsMigratory(i) Then
                    nMig = nMig + 1
                    migIndex(nMig) = i
                End If
            Next
            ReDim MigGrad(m_Data.InRow + 1, m_Data.InCol + 1, nMig, 12)

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Initialize the MigGrad(row,col,group,month) migration gradient matrix with
            '0 for cells inside a migration area
            '1000 for cells outside migration area
            '2000 for cells in low capacity habitat or land
            For iMigGrp = 1 To nMig
                For imonth = 1 To 12

                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1

                            MigGrad(i, j, iMigGrp, imonth) = 1000

                            If m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > MIN_MIG_PROB Then
                                MigGrad(i, j, iMigGrp, imonth) = 0
                            End If

                            If m_Data.Depth(i, j) = 0 Or m_Data.HabCap(migIndex(iMigGrp))(i, j) < 0.1 Then
                                MigGrad(i, j, iMigGrp, imonth) = 2000
                            End If

                        Next j
                    Next i
                Next imonth
            Next iMigGrp
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            Dim dist As Double

            For iMigGrp = 1 To nMig
                For imonth = 1 To 12

                    Dim row As Integer = Me.m_Data.PrefRow(migIndex(iMigGrp), imonth)
                    Dim col As Integer = Me.m_Data.Prefcol(migIndex(iMigGrp), imonth)

                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1

                            If Not m_Data.MigMaps(migIndex(iMigGrp), imonth)(i, j) > MIN_MIG_PROB Then
                                dist = Math.Sqrt((j - col) ^ 2 + (i - row) ^ 2)
                                MigGrad(i, j, iMigGrp, imonth) = dist
                            Else
                                MigGrad(i, j, iMigGrp, imonth) = 0
                            End If

                            If m_Data.Depth(i, j) = 0 Or m_Data.HabCap(migIndex(iMigGrp))(i, j) < 0.1 Then
                                MigGrad(i, j, iMigGrp, imonth) = 2000
                            End If

                        Next j
                    Next i
                Next imonth
            Next iMigGrp

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'DUBUGGING
            'Dump the migration maps to the debug/immediate window
            Dim tempstr As String
            For iMigGrp = 1 To 1 'nMig 'm_Data.NGroups
                For imonth = 1 To 12
                    Debug.Print("")
                    Debug.Print("imonth = " + imonth.ToString)
                    For i = 0 To m_Data.InRow + 1
                        For j = 0 To m_Data.InCol + 1
                            tempstr = tempstr + Math.Round(MigGrad(i, j, iMigGrp, imonth), 3).ToString.PadRight(10)

                        Next j
                        Debug.Print(tempstr)
                        tempstr = ""
                    Next i
                Next imonth
            Next iMigGrp
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub


    
    ''' <summary>
    ''' Calculate advection for a given month.
    ''' </summary>
    ''' <param name="iMonth">Month [1, 12] to calculate advection for.</param>
    Public Sub CalcAdvection(ByVal iMonth As Integer)

        Debug.Assert(iMonth > 0)

        Try
            For i As Integer = 0 To Me.m_Data.InRow
                For j As Integer = 0 To Me.m_Data.InCol
                    Me.m_Data.Xvloc(i, j) = Me.m_Data.Xv(i, j, iMonth)
                    Me.m_Data.Yvloc(i, j) = Me.m_Data.Yv(i, j, iMonth)

                    If Me.m_Data.Depth(i, j) > 0 Then
                        If Me.m_Data.Depth(i + 1, j) > 0 Then
                            If Me.m_Data.DepthA(i + 1, j) > Me.m_Data.DepthA(i, j) Then Me.m_Data.DepthY(i, j) = Me.m_Data.DepthA(i, j) Else Me.m_Data.DepthY(i, j) = Me.m_Data.DepthA(i + 1, j)
                        End If
                        If Me.m_Data.Depth(i, j + 1) > 0 Then
                            If Me.m_Data.DepthA(i, j + 1) > Me.m_Data.DepthA(i, j) Then Me.m_Data.DepthX(i, j) = Me.m_Data.DepthA(i, j) Else Me.m_Data.DepthX(i, j) = Me.m_Data.DepthA(i, j + 1)
                        End If
                    End If
                Next
            Next
        Catch ex As Exception

        End Try

        Try
            For i As Integer = 0 To Me.m_Data.InRow + 1
                For j As Integer = 0 To Me.m_Data.InCol + 1
                    If Me.m_Data.Depth(i, j) > 0 Then
                        Me.m_Data.Xvel(i, j) = Me.m_Data.Xvloc(i, j)
                        Me.m_Data.Yvel(i, j) = Me.m_Data.Yvloc(i, j)
                    End If
                Next
            Next
        Catch ex As Exception

        End Try

        Me.SM_MapApparentUpwell()
    End Sub

    
    ''' <summary>
    ''' Sets apparent upwelling/downwelling rates based only on flow forcing field 
    ''' sketched by model user
    ''' </summary>
    Private Sub SM_MapApparentUpwell()

        Dim Fl As Single
        Dim i As Integer
        Dim j As Integer

        For i = 0 To m_Data.InRow : For j = 0 To m_Data.InCol : m_Data.flow(i, j) = 0 : Next : Next

        ' JS: Moved to UI
        ' , UpMax As Single, UpLoc As Single, Cl2 As Single
        'Cl2 = 0.01 / m_Data.CellLength ' ^ 2
        For i = 0 To m_Data.InRow
            For j = 0 To m_Data.InCol
                If m_Data.Depth(i, j) > 0 Then
                    If m_Data.Depth(i + 1, j) > 0 Then
                        Fl = m_Data.Yvloc(i, j) * m_Data.DepthY(i, j)
                        'Yvel(i, j) = Yvloc(i, j) '?????????????????????
                        m_Data.flow(i, j) -= Fl
                        m_Data.flow(i + 1, j) = m_Data.flow(i + 1, j) + Fl
                    End If
                    If m_Data.Depth(i, j + 1) > 0 Then
                        Fl = m_Data.Xvloc(i, j) * m_Data.DepthX(i, j)
                        'Xvel(i, j) = Xvloc(i, j) '??????????????????????????
                        m_Data.flow(i, j) = m_Data.flow(i, j) - Fl
                        m_Data.flow(i, j + 1) = m_Data.flow(i, j + 1) + Fl
                    End If
                End If
            Next
        Next

        ' JS: Moved to UI
        'UpMax = 0
        'For i = 1 To m_Data.InRow
        '    For j = 1 To m_Data.InCol
        '        If m_Data.Depth(i, j) > 0 Then
        '            If Math.Abs(m_Data.flow(i, j)) > UpMax Then UpMax = Math.Abs(m_Data.flow(i, j))
        '        End If
        '    Next
        'Next
        'UpMax = UpMax * Cl2
        ''  Up.Cls()
        'For i = 1 To m_Data.InRow
        '    For j = 1 To m_Data.InCol
        '        If m_Data.Depth(i, j) > 0 Then
        '            UpLoc = -m_Data.flow(i, j) * Cl2
        '            m_Data.UpVel(i, j) = UpLoc  'Added for this model  SM.
        '            'Up.Circle (j + 0.5, i + 0.5 - UpLoc / UpMax), 0.1
        '            'Up.Line (j + 0.5, i + 0.5)-Step(0, -UpLoc / UpMax)
        '        End If
        '    Next
        'Next
        ''UpCap.Caption = "Upwelling velocities, max=" + Format$(UpMax / CellLength, "###.##") + "km/yr"
    End Sub

#End If
#End Region

End Class