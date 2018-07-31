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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterface.Ecosim
Imports ScientificInterface.Ecospace

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class that tries to make sure the interface correctly loads a particular
''' scenario.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cCoreController

#Region " Private vars "

    Private m_ui As frmEwE6 = Nothing

    ''' <summary>Core state monitor to query.</summary>
    Private m_monitor As cCoreStateMonitor = Nothing
    ''' <summary>Manager to use for bringing the core up to date.</summary>
    Private m_manager As cCoreStateManager = Nothing
    ''' <summary>Flag stating whether the core controller is enabled.</summary>
    Private m_bEnabled As Boolean = True

#End Region ' Private vars

#Region " Public access "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of a EwECoreController.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Sub New(ByVal monitor As cCoreStateMonitor, ByVal manager As cCoreStateManager, ui As frmEwE6)
        Me.m_manager = manager
        Me.m_monitor = monitor
        Me.m_ui = ui
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Check whether the EwE Core running state matches a given state, and if
    ''' this is not the case, try to bring the Core up to par.
    ''' </summary>
    ''' <param name="iState">The <see cref="eCoreExecutionState">Core execution state</see>
    ''' to check.</param>
    ''' <param name="bForceState">Tells this method to force loading the state, 
    ''' regardless what state the EwE6 core is at. Handle this parameter with 
    ''' care because recklessly overriding core states may have unpredictable 
    ''' results.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function LoadState(ByVal iState As eCoreExecutionState,
            Optional ByVal bForceState As Boolean = False) As Boolean

        Dim bSucces As Boolean = True

        If (Not Me.Enabled) Then Return bSucces

        ' State already superceded or active?
        If (Me.m_monitor.IsExecutionStateSuperceded(iState)) And (bForceState = False) Then
            Return bSucces
        End If

        ' Fixed #1433
        If (Me.m_monitor.IsBusy) Then Return False

        Select Case iState

            Case eCoreExecutionState.EcopathLoaded,
                 eCoreExecutionState.EcopathInitialized
                bSucces = TryLoadEcopathModel()

            Case eCoreExecutionState.EcopathCompleted
                bSucces = TryCompleteEcopath()

            Case eCoreExecutionState.EcosimLoaded
                bSucces = TryLoadEcosimScenario()

            Case eCoreExecutionState.EcosimInitialized
                bSucces = TryInitializeEcosim()

            Case eCoreExecutionState.EcosimCompleted
                bSucces = TryCompleteEcosim()

            Case eCoreExecutionState.EcotracerLoaded
                bSucces = TryLoadEcotracerScenario()

            Case eCoreExecutionState.EcospaceLoaded,
                 eCoreExecutionState.EcospaceInitialized
                bSucces = TryLoadEcospaceScenario()

            Case Else
                bSucces = False

        End Select

        Return bSucces

    End Function

    Public Sub LoadEcosimScenario()
        ' Force this core state
        LoadState(eCoreExecutionState.EcosimLoaded, True)
    End Sub

    Public Sub LoadEcospaceScenario()
        ' Force this core state
        LoadState(eCoreExecutionState.EcospaceLoaded, True)
    End Sub

    Public Sub LoadEcotracerScenario()
        ' Force this core state
        LoadState(eCoreExecutionState.EcotracerLoaded, True)
    End Sub

    ''' <summary>
    ''' Get/set the enabled state of the core controller.
    ''' </summary>
    ''' <remarks>
    ''' It may be necessary to temporarily disable the core controller
    ''' while performing batch operations on the UI. Please use this 
    ''' option with great care; leaving the core controller disabled
    ''' will cripple the Scientific Interface.
    ''' </remarks>
    Public Property Enabled() As Boolean
        Get
            Return Me.m_bEnabled
        End Get
        Set(ByVal value As Boolean)
            Me.m_bEnabled = value
        End Set
    End Property

#End Region ' Public access

#Region " Private members "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to load an Ecopath model.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' ---------------------------------------------------------------------------
    Private Function TryLoadEcopathModel() As Boolean
        Return Me.m_monitor.HasEcopathLoaded
    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Attempt get Ecopath to produce outputs.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' ---------------------------------------------------------------------------
    Private Function TryCompleteEcopath() As Boolean

        ' Is ecopath model loaded?
        If LoadState(eCoreExecutionState.EcopathLoaded) Then
            ' #Yes: get ecopath up to par
            Return Me.m_manager.LoadState(eCoreExecutionState.EcopathCompleted)
        End If

        Return False
    End Function

    Private Function TryInitializeEcosim() As Boolean

        ' Is Ecosim scenario loaded?
        If LoadState(eCoreExecutionState.EcosimLoaded) Then
            Return Me.m_manager.LoadState(eCoreExecutionState.EcosimInitialized)
        End If

        Return False

    End Function

    Private Function TryCompleteEcosim() As Boolean

        ' Is Ecosim scenario loaded?
        If LoadState(eCoreExecutionState.EcosimLoaded) Then
            Return Me.m_manager.LoadState(eCoreExecutionState.EcosimCompleted)
        End If

        Return False

    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to load an Ecosim scenario.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' ---------------------------------------------------------------------------
    Private Function TryLoadEcosimScenario() As Boolean

        Dim bSuccess As Boolean = False

        If Me.LoadState(eCoreExecutionState.EcopathCompleted) Then
            ' Let AppLauncher perform the load as it sees fit
            bSuccess = Me.m_ui.LoadEcosimScenario()
        End If

        Return bSuccess
    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to load an Ecospace scenario.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' ---------------------------------------------------------------------------
    Private Function TryLoadEcospaceScenario() As Boolean

        Dim bSuccess As Boolean = False

        ' JS 07mar07: Ecosim model needs to be loaded, not run, for an ecospace model to load.
        ' JS 07may18: (11 years on) Ecosim should run, otherwise the core will misinform the world
        '             This fixes issue #1572
        If LoadState(eCoreExecutionState.EcosimCompleted) Then
            ' Let AppLauncher perform the load as it sees fit
            bSuccess = Me.m_ui.LoadEcospaceScenario()
        End If

        Return bSuccess
    End Function


    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to load an Ecotracer scenario.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' ---------------------------------------------------------------------------
    Private Function TryLoadEcotracerScenario() As Boolean

        Dim bSuccess As Boolean = False

        ' JS 07mar07: Ecosim model needs to be loaded, not run, for an ecotracer model to load.
        ' JS 07may18: (11 years on) Ecosim should run, otherwise the core will misinform the world
        '             This fixes issue #1572
        If Me.LoadState(eCoreExecutionState.EcosimCompleted) Then
            ' Let AppLauncher perform the load as it sees fit
            bSuccess = Me.m_ui.LoadEcotracerScenario()
        End If

        Return bSuccess
    End Function

#End Region ' Private members 

End Class
