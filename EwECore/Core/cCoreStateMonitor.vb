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

Imports EwECore.DataSources
Imports EwEUtils.Core
Imports System.ComponentModel
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Monitor that distributes Core execution state change events and Core data
''' state change events.
''' </summary>
''' <remarks>
''' <para>The following class tracks core execution state changes:</para>
''' <code>
''' Class StateTracker
''' 
'''     Public Sub New(ByRef sm as cCoreStateMonitor)
'''         ' Hook up to core state monitor
'''        AddHandler sm.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChange
'''     End Sub
''' 
'''     Private Sub OnCoreExecutionStateChange(ByVal core As cCore, ByVal iState As eCoreExecutionState)
'''        ' Handle core state changes
'''        Console.WriteLine("State tracker: core {0} state has changed to {1}", core, iState)
'''     End Sub
'''
''' End Class
''' </code>
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cCoreStateMonitor

    'ToDo 27-Aug-09 cCoreStateMonitor events. Broadcasting of ALL events needs to be protected from having the handler removed while the event is being sent.
    'This is done in a hack way (via a temp handler array) in CoreExecutionStateEvent this should change to not managing the handlers ourselves 
    'or using a blocking object during broadcasting, adding and removal of events

    ' ToDO 12-Nov-15: add means to query if searches are running or have been completed: IsMonteCarloRunning, HasMonteCarloRan, etc

#Region " Private members "

    ''' <summary>Reference to the monitored core.</summary>
    Private m_core As cCore = Nothing
    ''' <summary>Sync object for delivering events.</summary>
    Private m_sync As ISynchronizeInvoke = Nothing

    ''' <summary>Core execution state flag.</summary>
    Private m_iExecutionState As eCoreExecutionState = eCoreExecutionState.Idle
    ''' <summary>Ecopath execution state flag.</summary>
    Private m_iEcopathState As eCoreExecutionState = eCoreExecutionState.Idle
    ''' <summary>Ecosim execution state flag.</summary>
    Private m_iEcosimState As eCoreExecutionState = eCoreExecutionState.Idle
    ''' <summary>Ecospace execution state flag.</summary>
    Private m_iEcospaceState As eCoreExecutionState = eCoreExecutionState.Idle
    ''' <summary>Ecotracer execution state flag.</summary>
    Private m_iEcotracerState As eCoreExecutionState = eCoreExecutionState.Idle
    ''' <summary>Ecotracer execution result state flag.</summary>
    Private m_iEcotracerResultState As eEcotracerRunState = eEcotracerRunState.None

    ''' <summary>Flag stating the current search mode.</summary>
    Private m_searchmode As eSearchModes = eSearchModes.NotInSearch
    ''' <summary>Flag stating that a batch lock is active.</summary>
    Private m_bIsBatchLocked As Boolean = False

    ''' <summary>Flag indicating whether the datasource contains unsaved changes that do not affect the running model and its scenarios.</summary>
    Private m_bDatasourceModified As Boolean = False
    ''' <summary>Flag indicating whether the ecopath model data contains unsaved changes.</summary>
    Private m_bEcopathModified As Boolean = False
    ''' <summary>Flag indicating whether the ecosim scenario data contains unsaved changes.</summary>
    Private m_bEcosimModified As Boolean = False
    ''' <summary>Flag indicating whether the ecospace scenario data contains unsaved changes.</summary>
    Private m_bEcospaceModified As Boolean = False
    ''' <summary>Flag indicating whether the ecotracer scenario data contains unsaved changes.</summary>
    Private m_bEcotracerModified As Boolean = False
    ''' <summary>Flag indicating whether plugin data contains unsaved changes.</summary>
    Private m_bPluginModified As Boolean = False

#End Region ' Private members

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of this class.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">Core</see> that is monitored.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByRef core As cCore)
        Debug.Assert(core IsNot Nothing)
        Me.m_core = core
    End Sub

#Region " CoreExecutionState Delegates and Events "

    ''' -----------------------------------------------------------------------
    ''' <summary>Delegate, invoked to broadcast a core execution state change event.</summary>
    ''' <param name="statemonitor">A reference to the EwE <see cref="cCore">Core</see> which
    ''' execution state changed.</param>
    ''' -----------------------------------------------------------------------
    Public Delegate Sub CoreExecutionStateDelegate(ByVal statemonitor As cCoreStateMonitor)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The core execution state change event.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Custom Event CoreExecutionStateEvent As CoreExecutionStateDelegate
        AddHandler(ByVal handler As CoreExecutionStateDelegate)
            Me.m_executionStateHandlers.Add(handler)
            Try
                If m_sync IsNot Nothing Then
                    Me.m_sync.Invoke(handler, New Object() {Me})
                Else
                    handler.Invoke(Me)
                End If
            Catch ex As Exception

            End Try
        End AddHandler

        RemoveHandler(ByVal handler As CoreExecutionStateDelegate)
            Me.m_executionStateHandlers.Remove(handler)
        End RemoveHandler

        RaiseEvent(ByVal statemonitor As cCoreStateMonitor)
            '27-Aug-09 Make a temp array of handlers and use that to broadcast the events
            'handlers can be removed in response to an event this violates the m_executionStateHandlers collection
            'A better way to handle this would be to block the adding and removing of handlers while events are being broadcast (Semaphore)
            'this would stop any Exceptions from being thrown
            Dim handlers() As CoreExecutionStateDelegate = Me.m_executionStateHandlers.ToArray()
            For Each h As CoreExecutionStateDelegate In handlers
                Try

                    If h IsNot Nothing Then
                        If m_sync IsNot Nothing Then
                            Me.m_sync.Invoke(h, New Object() {Me})
                        Else
                            h.Invoke(Me)
                        End If
                    End If
                Catch ex As Exception
                    System.Console.WriteLine(Me.ToString & ".CoreExecutionStateEvent() Exception: " & ex.Message)
                End Try
            Next
        End RaiseEvent
    End Event

    ''' <summary>List of all subscribed core execution state event listeners.</summary>
    Private m_executionStateHandlers As New List(Of CoreExecutionStateDelegate)

#End Region ' CoreExcutionState Delegates and Events

#Region " CoreDataState Delegate and Event "

    ''' -----------------------------------------------------------------------
    ''' <summary>Delegate, invoked to broadcast a core data state change event.</summary>
    ''' <param name="coreStateMonitor">THe monitor sending the event.</param>
    ''' -----------------------------------------------------------------------
    Public Delegate Sub CoreDataStateDelegate(ByVal coreStateMonitor As cCoreStateMonitor)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The core data state change event.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Custom Event CoreDataStateEvent As CoreDataStateDelegate
        AddHandler(ByVal handler As CoreDataStateDelegate)
            Me.m_dataStateHandlers.Add(handler)
            If m_sync IsNot Nothing Then
                Me.m_sync.Invoke(handler, New Object() {Me})
            Else
                handler.Invoke(Me)
            End If
        End AddHandler

        RemoveHandler(ByVal handler As CoreDataStateDelegate)
            Me.m_dataStateHandlers.Remove(handler)
        End RemoveHandler

        RaiseEvent(ByVal coreStateMonitor As cCoreStateMonitor)
            For Each h As CoreDataStateDelegate In Me.m_dataStateHandlers
                If m_sync IsNot Nothing Then
                    Me.m_sync.Invoke(h, New Object() {Me})
                Else
                    h.Invoke(Me)
                End If
            Next
        End RaiseEvent
    End Event

    ''' <summary>List of all subscribed core data state event listeners.</summary>
    Private m_dataStateHandlers As New List(Of CoreDataStateDelegate)

#End Region ' CoreExcutionState Delegate and Event

#Region " Private helpers "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Calculates and updates the Core execution state. A 
    ''' <see cref="CoreExecutionStateEvent">CoreExecutionStateEvent</see> is
    ''' broadcasted when the Core execution state changes.
    ''' </summary>
    ''' <param name="tsForceUpdate">Flag stating whether an update should be sent:
    ''' true    - force, 
    ''' default - send update when changed, 
    ''' false   - do NOT send update
    ''' </param>
    ''' -----------------------------------------------------------------------
    Private Sub CalcExecutionState(ByVal iEcopathState As eCoreExecutionState,
            ByVal iEcosimState As eCoreExecutionState,
            ByVal iEcospaceState As eCoreExecutionState,
            ByVal iEcotracerState As eCoreExecutionState,
            Optional ByVal tsForceUpdate As TriState = TriState.UseDefault)

        Dim iState As eCoreExecutionState = eCoreExecutionState.Idle
        Dim bEcopathStateChange As Boolean = False
        Dim bEcosimStateChange As Boolean = False
        Dim bEcospaceStateChange As Boolean = False
        Dim bEcotracerStateChange As Boolean = False

        bEcopathStateChange = (iEcopathState <> Me.m_iEcopathState)
        bEcosimStateChange = (iEcosimState <> Me.m_iEcosimState)
        bEcospaceStateChange = (iEcospaceState <> Me.m_iEcospaceState)
        bEcotracerStateChange = (iEcotracerState <> Me.m_iEcotracerState)

        ' No state changes?
        If (Not bEcopathStateChange And Not bEcosimStateChange And Not bEcospaceStateChange And Not bEcotracerStateChange) And
           (tsForceUpdate <> TriState.True) Then Return

        ' Accept ecopath state
        iState = iEcopathState
        ' Has ecopath model ran?
        If iState = eCoreExecutionState.EcopathCompleted Then
            ' #Yes: is an ecosim scenario loaded?
            If iEcosimState <> eCoreExecutionState.Idle Then
                ' #Yes: accept ecosim state
                iState = iEcosimState
                ' Is an ecosim model loaded?
                If iState >= eCoreExecutionState.EcosimLoaded Then
                    ' #Yes: is an ecospace model loaded?
                    If iEcospaceState <> eCoreExecutionState.Idle Then
                        ' #Yes: accept ecospace state
                        iState = iEcospaceState
                    End If
                End If
            End If
        End If

        ' Update local execution sub-state flags
        Me.m_iEcopathState = iEcopathState
        Me.m_iEcosimState = iEcosimState
        Me.m_iEcospaceState = iEcospaceState
        Me.m_iEcotracerState = iEcotracerState

        ' Update core execution state flag
        Me.m_iExecutionState = iState

        ' No need to suppress update?
        If (tsForceUpdate <> TriState.False) Then
            ' #Yes: Broadcast event
            RaiseEvent CoreExecutionStateEvent(Me)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Calculates and updates the Core data state. A 
    ''' <see cref="CoreDataStateEvent">CoreDataStateEvent</see> is
    ''' broadcasted when the data state of either Ecopath or Ecosim changes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateDataState(ByVal bDatasourceModified As Boolean,
            ByVal bEcopathModified As Boolean,
            ByVal bEcosimModified As Boolean,
            ByVal bEcospaceModified As Boolean,
            ByVal bEcotracerModified As Boolean,
            ByVal bPluginModified As Boolean,
            Optional ByVal tsSendUpdate As TriState = TriState.UseDefault)

        Dim bChange As Boolean = (bDatasourceModified <> Me.m_bDatasourceModified) Or
           (bEcopathModified <> Me.m_bEcopathModified) Or
           (bEcosimModified <> Me.m_bEcosimModified) Or
           (bEcospaceModified <> Me.m_bEcospaceModified) Or
           (bEcotracerModified <> Me.m_bEcotracerModified) Or
           (bPluginModified <> Me.m_bPluginModified)

        ' Update flags
        Me.m_bDatasourceModified = bDatasourceModified
        Me.m_bEcopathModified = bEcopathModified
        Me.m_bEcosimModified = bEcosimModified
        Me.m_bEcospaceModified = bEcospaceModified
        Me.m_bEcotracerModified = bEcotracerModified
        Me.m_bPluginModified = bPluginModified

        ' Broadcast data state event
        If tsSendUpdate = TriState.False Then Return
        If tsSendUpdate = TriState.UseDefault And Not bChange Then Return

        ' Broadcast changes
        RaiseEvent CoreDataStateEvent(Me)

    End Sub

#End Region ' Private helpers

#Region " Config "

    Public Property SyncObject() As ISynchronizeInvoke
        Get
            Return Me.m_sync
        End Get
        Set(ByVal value As ISynchronizeInvoke)
            Me.m_sync = value
        End Set
    End Property

#End Region ' Config

#Region " State configuration "

#Region " Data "

    Friend Sub UpdateDataState(ByVal ds As IEwEDataSource,
                               Optional ByVal tsSendUpdate As EwEUtils.Core.TriState = TriState.UseDefault)

        Dim bDatasourceModified As Boolean = False
        Dim bEcopathModified As Boolean = False
        Dim bEcosimModified As Boolean = False
        Dim bEcospaceModified As Boolean = False
        Dim bEcotracerModified As Boolean = False
        Dim bPluginModified As Boolean = False

        If (ds IsNot Nothing) Then
            bDatasourceModified = ds.IsModified()
            If (TypeOf ds Is IEcopathDataSource) Then bEcopathModified = DirectCast(ds, IEcopathDataSource).IsEcopathModified()
            If (TypeOf ds Is IEcosimDatasource) Then bEcosimModified = DirectCast(ds, IEcosimDatasource).IsEcosimModified()
            If (TypeOf ds Is IEcospaceDatasource) Then bEcospaceModified = DirectCast(ds, IEcospaceDatasource).IsEcospaceModified()
            If (TypeOf ds Is IEcotracerDatasource) Then bEcotracerModified = DirectCast(ds, IEcotracerDatasource).IsEcotracerModified()
        End If

        If (Me.m_core.PluginManager IsNot Nothing) Then
            bPluginModified = Me.m_core.PluginManager.IsDatabaseModified
        End If

        Me.UpdateDataState(bDatasourceModified, bEcopathModified,
                           bEcosimModified, bEcospaceModified,
                           bEcotracerModified, bPluginModified,
                           tsSendUpdate)
    End Sub

#End Region ' Data

#Region " Execution "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="cc">The core component that changed.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub UpdateExecutionState(ByVal cc As eCoreComponentType,
                                    Optional ByVal tsSendUpdate As TriState = TriState.UseDefault)

        Dim pm As EwEPlugin.cPluginManager = Me.m_core.PluginManager
        Dim bHasEcopathRan As Boolean = Me.HasEcopathRan
        Dim bHasEcosimRan As Boolean = Me.HasEcosimRan
        Dim bHasEcospaceRan As Boolean = Me.HasEcospaceRan
        Dim bHandled As Boolean = False

        Select Case cc
            Case eCoreComponentType.Core,
                 eCoreComponentType.DataSource,
                 eCoreComponentType.External,
                 eCoreComponentType.NotSet,
                 eCoreComponentType.SearchObjective
                ' NOP

            Case eCoreComponentType.EcoPath
                SetEcopathLoaded(Me.HasEcopathLoaded(), tsSendUpdate)
                bHandled = True

            Case eCoreComponentType.EcoSim,
                 eCoreComponentType.EcoSimFitToTimeSeries,
                 eCoreComponentType.EcoSimMonteCarlo,
                 eCoreComponentType.FishingPolicySearch,
                 eCoreComponentType.ShapesManager,
                 eCoreComponentType.TimeSeries
                SetEcoSimLoaded(Me.HasEcosimLoaded(), tsSendUpdate, False)
                bHandled = True

            Case eCoreComponentType.EcoSpace,
                 eCoreComponentType.MPAOptimization
                Me.SetEcospaceLoaded(Me.HasEcospaceLoaded(), tsSendUpdate, False)
                bHandled = True

            Case eCoreComponentType.Ecotracer
                Me.SetEcotracerLoaded(Me.HasEcotracerLoaded(), tsSendUpdate, False)
                bHandled = True

        End Select

        ' Not handled but notification requested?
        If tsSendUpdate = TriState.True And bHandled = False Then
            ' #Yes: force notification
            Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, Me.m_iEcospaceState, Me.m_iEcotracerState, tsSendUpdate)
        End If

        If (tsSendUpdate = TriState.False) Or (pm Is Nothing) Then Return
        If (bHasEcospaceRan <> Me.HasEcospaceRan) Then pm.EcospaceRunInvalidated()
        If (bHasEcosimRan <> Me.HasEcosimRan) Then pm.EcosimRunInvalidated()
        If (bHasEcopathRan <> Me.HasEcopathRan) Then pm.EcopathRunInvalidated()

    End Sub

#Region " Ecopath "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecopath model is loaded
    ''' or unloaded.
    ''' </summary>
    ''' <param name="bHasModel">Flag indicating whether an Ecopath model is
    ''' loaded (True) or unloaded (False).</param>
    ''' <param name="tsForceUpdate">Flag stating whether an update should be sent:
    ''' true    - force, 
    ''' default - send update when changed, 
    ''' false   - do NOT send update
    ''' </param>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcopathLoaded(ByVal bHasModel As Boolean,
                                Optional ByVal tsForceUpdate As TriState = TriState.UseDefault)
        ' Update execution state
        If bHasModel Then
            ' Switch to ecopath loaded. All other model states must be reset to either idle or loaded
            Me.CalcExecutionState(eCoreExecutionState.EcopathLoaded,
                DirectCast(Math.Min(Me.m_iEcosimState, eCoreExecutionState.EcosimLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState),
                tsForceUpdate)
        Else
            Me.CalcExecutionState(eCoreExecutionState.Idle, eCoreExecutionState.Idle, eCoreExecutionState.Idle, eCoreExecutionState.Idle, tsForceUpdate)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecopath model is initialized.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcopathInitialized()
        ' Check for invalid state transitions
        If (Me.m_iEcopathState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(eCoreExecutionState.EcopathInitialized,
            DirectCast(Math.Min(Me.m_iEcosimState, eCoreExecutionState.EcosimLoaded), eCoreExecutionState),
            DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
            DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecopath model is started.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcopathRun()
        ' Check for invalid state transitions
        If (Me.m_iEcopathState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(eCoreExecutionState.EcopathRunning,
                DirectCast(Math.Min(Me.m_iEcosimState, eCoreExecutionState.EcosimLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecopath model has
    ''' completed its parameter estimation.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcopathCompleted()
        ' Check for invalid state transitions
        If (Me.m_iEcopathState <> eCoreExecutionState.EcopathRunning) Then Return
        ' Update execution state
        Me.CalcExecutionState(eCoreExecutionState.EcopathCompleted,
                DirectCast(Math.Min(Me.m_iEcosimState, eCoreExecutionState.EcosimLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecopath model is started.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetPSDCompleted()
        ' Check for invalid state transitions
        If (Me.m_iEcopathState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(eCoreExecutionState.PSDCompleted,
                DirectCast(Math.Min(Me.m_iEcosimState, eCoreExecutionState.EcosimLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

#End Region ' Ecopath

#Region " Ecosim "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecosim scenario is loaded
    ''' or unloaded.
    ''' </summary>
    ''' <param name="bHasScenario">Flag indicating whether an Ecosim scenario is
    ''' loaded (True) or unloaded (False).</param>
    ''' <param name="tsForceUpdate">Flag stating whether an update should be sent:
    ''' true    - force, 
    ''' default - send update when changed, 
    ''' false   - do NOT send update
    ''' </param>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcoSimLoaded(ByVal bHasScenario As Boolean,
                               Optional ByVal tsForceUpdate As TriState = TriState.UseDefault,
                               Optional ByVal bResetDataState As Boolean = True)
        ' Update execution state
        If bHasScenario Then
            ' Switch to ecosim loaded state. Space and Tracer states must be reset to either idle or loaded
            Me.CalcExecutionState(Me.m_iEcopathState, eCoreExecutionState.EcosimLoaded,
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState),
                tsForceUpdate)
        Else
            Me.CalcExecutionState(Me.m_iEcopathState, eCoreExecutionState.Idle, eCoreExecutionState.Idle, eCoreExecutionState.Idle, tsForceUpdate)
        End If

        If bResetDataState Then
            ' Clear scenario changed flags
            Me.UpdateDataState(Me.m_bDatasourceModified, Me.m_bEcopathModified, False, False, Me.m_bEcotracerModified, Me.m_bPluginModified)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecosim scenario is initialized.
    ''' or unloaded.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcoSimInitialized()
        ' Check for invalid state transitions
        If (Me.m_iEcosimState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(Me.m_iEcopathState, eCoreExecutionState.EcosimInitialized,
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecosim run is started.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcosimRun()
        ' Check for invalid state transitions
        If (Me.m_iEcosimState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(Me.m_iEcopathState, eCoreExecutionState.EcosimRunning,
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecosim scenario has 
    ''' completed its timesteps.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcosimCompleted(ByVal bEcotracerOn As Boolean)
        ' Check for invalid state transitions
        If (Me.m_iEcosimState <> eCoreExecutionState.EcosimRunning) Then Return

        Me.m_bRequiresEcosimFullInit = False
        Me.m_iEcotracerResultState = If(bEcotracerOn, eEcotracerRunState.Ecosim, eEcotracerRunState.None)

        ' Update execution state
        Me.CalcExecutionState(Me.m_iEcopathState, eCoreExecutionState.EcosimCompleted,
                DirectCast(Math.Min(Me.m_iEcospaceState, eCoreExecutionState.EcospaceLoaded), eCoreExecutionState),
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

#End Region ' Ecosim

#Region " Ecospace "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecospace scenario is loaded
    ''' or unloaded.
    ''' </summary>
    ''' <param name="bHasScenario">Flag indicating whether an Ecospace scenario is
    ''' loaded (True) or unloaded (False).</param>
    ''' <param name="tsForceUpdate">Flag stating whether an update should be sent:
    ''' true    - force, 
    ''' default - send update when changed, 
    ''' false   - do NOT send update
    ''' </param>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcospaceLoaded(ByVal bHasScenario As Boolean,
                                 Optional ByVal tsForceUpdate As TriState = TriState.UseDefault,
                                Optional ByVal bResetDataState As Boolean = True)
        ' Update execution state
        If bHasScenario Then
            ' Switch to ecospace loaded state. Tracer state must be reset to either idle or loaded
            Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, eCoreExecutionState.EcospaceLoaded,
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState),
                tsForceUpdate)
        Else
            Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, eCoreExecutionState.Idle, Me.m_iEcotracerState, tsForceUpdate)
        End If

        If bResetDataState Then
            ' Clear scenario changed flags
            Me.UpdateDataState(Me.m_bDatasourceModified, Me.m_bEcopathModified, Me.m_bEcosimModified, False, Me.m_bEcotracerModified, Me.m_bPluginModified)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecospace scenario is initialized.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcospaceInitialized()
        ' Check for invalid state transitions
        If (Me.m_iEcosimState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, eCoreExecutionState.EcospaceInitialized,
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecospace scenario is started.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcospaceRun()
        ' Check for invalid state transitions
        If (Me.m_iEcosimState = eCoreExecutionState.Idle) Then Return
        ' Update execution state
        Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, eCoreExecutionState.EcospaceRunning,
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecosim scenario has 
    ''' completed its timesteps.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcospaceCompleted(ByVal bEcotracerOn As Boolean)
        ' Check for invalid state transitions
        If (Me.m_iEcospaceState <> eCoreExecutionState.EcospaceRunning) Then Return
        Me.m_iEcotracerResultState = If(bEcotracerOn, eEcotracerRunState.Ecospace, eEcotracerRunState.None)

        ' Update execution state
        Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, eCoreExecutionState.EcospaceCompleted,
                DirectCast(Math.Min(Me.m_iEcotracerState, eCoreExecutionState.EcotracerLoaded), eCoreExecutionState))
    End Sub

#End Region ' Ecospace

#Region " Ecotracer "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' State change entry point; to be called when an Ecotracer scenario is loaded
    ''' or unloaded.
    ''' </summary>
    ''' <param name="bHasScenario">Flag indicating whether an Ecotracer scenario is
    ''' loaded (True) or unloaded (False).</param>
    ''' <param name="tsForceUpdate">Flag stating whether an update should be sent:
    ''' true    - force, 
    ''' default - send update when changed, 
    ''' false   - do NOT send update
    ''' </param>
    ''' -----------------------------------------------------------------------
    Friend Sub SetEcotracerLoaded(ByVal bHasScenario As Boolean,
                                  Optional ByVal tsForceUpdate As TriState = TriState.UseDefault,
                                  Optional ByVal bResetDataState As Boolean = True)
        ' Update execution state
        If bHasScenario Then
            Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, Me.m_iEcospaceState, eCoreExecutionState.EcotracerLoaded, tsForceUpdate)
        Else
            Me.CalcExecutionState(Me.m_iEcopathState, Me.m_iEcosimState, Me.m_iEcospaceState, eCoreExecutionState.Idle, tsForceUpdate)
        End If

        If bResetDataState Then
            ' Clear scenario changed flags
            Me.UpdateDataState(Me.m_bDatasourceModified, Me.m_bEcopathModified, Me.m_bEcosimModified, Me.m_bEcospaceModified, False, Me.m_bPluginModified)
        End If

    End Sub

#End Region ' Ecotracer

#Region " Busy state "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the searching state of the core state monitor.
    ''' </summary>
    ''' <param name="searchmode"><see cref="eSearchModes">Search mode state flag</see>.</param>
    ''' <remarks>Use this with care!!</remarks>
    ''' -----------------------------------------------------------------------
    Public Sub SetIsSearching(ByVal searchmode As eSearchModes)
        If (Me.m_searchmode <> searchmode) Then
            Me.m_searchmode = searchmode
            RaiseEvent CoreExecutionStateEvent(Me)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the batch lock state of the core state monitor.
    ''' </summary>
    ''' <param name="bIsBatchLocked">State flag to set</param>
    ''' -----------------------------------------------------------------------
    Friend Sub SetIsBatchLocked(ByVal bIsBatchLocked As Boolean)
        Me.m_bIsBatchLocked = bIsBatchLocked
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This only fires off a core execution state event to allow depending 
    ''' elements to check the <see cref="IsPaused()"/> state.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub SetIsPaused()
        RaiseEvent CoreExecutionStateEvent(Me)
    End Sub

#End Region ' Busy state

#End Region ' Ececution

#End Region ' State configuration

#Region " State diagnostics "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether there are ANY unsaved changes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsModified() As Boolean
        ' OMG
        Return (Me.IsDatasourceModified Or Me.IsEcopathModified Or Me.IsEcosimModified Or Me.IsEcospaceModified Or Me.IsEcotracerModified Or Me.IsPluginModified)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the core is busy. Either a model is running, a search
    ''' is in progress, or a batch operation is in progress.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsBusy() As Boolean
        Return (Me.IsEcopathRunning Or Me.IsEcosimRunning Or Me.IsEcospaceRunning Or Me.IsSearching Or Me.IsBatchLocked)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the core is paused while <see cref="IsBusy()"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsPaused() As Boolean
        Return Me.IsBusy And Me.m_core.EcospacePaused
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecopath model has been loaded.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcopathLoaded() As Boolean
        Return Me.m_iEcopathState <> eCoreExecutionState.Idle
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecopath model has been initialized.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcopathInitialized() As Boolean
        Return Me.m_iEcopathState >= eCoreExecutionState.EcopathInitialized
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecopath model is running.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsEcopathRunning() As Boolean
        Return Me.m_iEcopathState = eCoreExecutionState.EcopathRunning
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecopath model has completed succesfully.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcopathRan() As Boolean
        Return Me.m_iEcopathState = eCoreExecutionState.EcopathCompleted
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the Ecopath PSD model has completed succesfully.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasPSDRan() As Boolean
        Return Me.m_iEcopathState = eCoreExecutionState.PSDCompleted
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecosim scenario has been loaded.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcosimLoaded() As Boolean
        Return Me.m_iEcosimState <> eCoreExecutionState.Idle
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecosim scenario has been initialized.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcosimInitialized() As Boolean
        Return Me.m_iEcosimState >= eCoreExecutionState.EcosimInitialized
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecosim scenario is running.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsEcosimRunning() As Boolean
        Return Me.m_iEcosimState = eCoreExecutionState.EcosimRunning
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecosim scenario has completed its timesteps succesfully.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcosimRan() As Boolean
        Return Me.m_iEcosimState = eCoreExecutionState.EcosimCompleted
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether Ecotracer results have been computed for the last 
    ''' <see cref="HasEcosimRan()">Ecosim run</see> .
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcotracerRanForEcosim() As Boolean
        Return Me.HasEcosimRan And Me.m_iEcotracerResultState = eEcotracerRunState.Ecosim
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecospace scenario has been loaded.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcospaceLoaded() As Boolean
        Return Me.m_iEcospaceState <> eCoreExecutionState.Idle
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecospace scenario has been initialized.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcospaceInitialized() As Boolean
        Return Me.m_iEcospaceState >= eCoreExecutionState.EcospaceInitialized
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecospace scenario is running.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsEcospaceRunning() As Boolean
        Return Me.m_iEcospaceState = eCoreExecutionState.EcospaceRunning
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecospace scenario has completed its timesteps succesfully.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcospaceRan() As Boolean
        Return Me.m_iEcospaceState = eCoreExecutionState.EcospaceCompleted
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether Ecotracer results have been computed for the last 
    ''' <see cref="HasEcospaceRan()">Ecospace run</see> .
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcotracerRanForEcospace() As Boolean
        Return Me.HasEcospaceRan And Me.m_iEcotracerResultState = eEcotracerRunState.Ecospace
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the core is searching. To find what search is running
    ''' refer to <see cref="SearchMode"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsSearching() As Boolean
        Return (Me.m_searchmode <> eSearchModes.NotInSearch)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the current <see cref="eSearchModes"/>.
    ''' </summary>
    ''' <returns>The current <see cref="eSearchModes"/>.</returns>
    ''' -----------------------------------------------------------------------
    Public Function SearchMode() As eSearchModes
        Return Me.m_searchmode
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a batch lock is active on the core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsBatchLocked() As Boolean
        Return Me.m_bIsBatchLocked
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the current core execution state equals or exceeds
    ''' a given state.
    ''' </summary>
    ''' <param name="iState">The <see cref="eCoreExecutionState">core execution state</see> to check.</param>
    ''' <remarks>
    ''' <para>The core execution states system is described by a sequence of 
    ''' states that supercede earlier states.</para>
    ''' <para>For instance, the eCoreExecutionState EcosimReady can only 
    ''' occur when an underlying Ecopath model has been loaded and when
    ''' the Ecopath model as completed a successful run: the state
    ''' <see cref="eCoreExecutionState.EcosimLoaded">EcosimLoaded</see> thus supercedes
    ''' <see cref="eCoreExecutionState.EcopathLoaded">EcopathLoaded</see> and
    ''' <see cref="eCoreExecutionState.EcopathCompleted">EcopathCompleted</see>.</para>
    ''' <para>Please be careful when interpreting results from this method; do
    ''' not confuse superceding with implying! In the aforementioned example, 
    ''' the EcosimReady state also supercedes the state that descibes that an 
    ''' Ecopath model has been modified by the user, and the state that describes
    ''' that an Ecopath model is not loaded yet.</para>
    ''' <para>In some case, assuming that superceded states are also current 
    ''' states may lead to serious nonsense.</para>
    ''' </remarks>
    ''' <note_to_self>
    ''' Wow, that's a lot of talking for a one-line function implementation...
    ''' </note_to_self>
    ''' -----------------------------------------------------------------------
    Public Function IsExecutionStateSuperceded(ByVal iState As eCoreExecutionState) As Boolean
        ' Exception for Ecotracer load state since it does not fit the incremental state tree well;
        ' If Ecospace is loaded, the ecotracer loaded state is assumed true, ugh...
        If iState = eCoreExecutionState.EcotracerLoaded Then Return Me.HasEcotracerLoaded()

        Return (iState <= Me.m_iExecutionState)
    End Function

    Public Function CanEcopathLoad() As Boolean
        Return True
    End Function

    Public Function CanEcosimLoad() As Boolean
        Return Me.HasEcopathRan()
    End Function

    Public Function CanEcospaceLoad() As Boolean
        Return Me.HasEcosimLoaded()
    End Function

    Public Function CanEcotracerLoad() As Boolean
        Return Me.HasEcopathLoaded()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an Ecotracer scenario has been loaded.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasEcotracerLoaded() As Boolean
        Return Me.m_iEcotracerState <> eCoreExecutionState.Idle
    End Function

#End Region ' State diagnostics

#Region " State variable access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the current EwE <see cref="cCore">Core</see> Execution state.
    ''' </summary>
    ''' <returns>A <see cref="eCoreExecutionState">eCoreExecutionState</see>
    ''' value indicating the Core execution state.</returns>
    ''' -----------------------------------------------------------------------
    Public Function CoreExecutionState() As eCoreExecutionState
        Return Me.m_iExecutionState
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the Datasource contains changes that have not 
    ''' been saved, which will not influence the running model and its scenarios.
    ''' </summary>
    ''' <returns>True if there are unsaved changes, False otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsDatasourceModified() As Boolean
        Return Me.m_bDatasourceModified
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the Ecopath model data contains changes that have not 
    ''' been saved.</summary>
    ''' <returns>True if there are unsaved changes, False otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsEcopathModified() As Boolean
        Return Me.m_bEcopathModified
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the Ecosim scenario data contains changes that have not 
    ''' been saved.</summary>
    ''' <returns>True if there are unsaved changes, False otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsEcosimModified() As Boolean
        Return Me.m_bEcosimModified
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the Ecospace scenario data contains changes that have not 
    ''' been saved.</summary>
    ''' <returns>True if there are unsaved changes, False otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsEcospaceModified() As Boolean
        Return Me.m_bEcospaceModified
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the Ecotracer scenario data contains changes that have not 
    ''' been saved.</summary>
    ''' <returns>True if there are unsaved changes, False otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsEcotracerModified() As Boolean
        Return Me.m_bEcotracerModified
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the plug-ins contains data changes that have not 
    ''' been saved.</summary>
    ''' <returns>True if there are unsaved changes, False otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsPluginModified() As Boolean
        Return Me.m_bPluginModified
    End Function

#End Region ' ..for if you don't like events

#Region " Registry of modifications "

    Private m_bRequiresEcosimFullInit As Boolean = False

    Friend Sub RegisterModification(ByVal component As eCoreComponentType)
        If component = eCoreComponentType.EcoPath Then Me.m_bRequiresEcosimFullInit = True
    End Sub

    Friend Function RequiresEcosimFullInit() As Boolean
        Return Me.m_bRequiresEcosimFullInit
    End Function

#End Region ' Ministry of silly walks

End Class
