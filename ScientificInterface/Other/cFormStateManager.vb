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
Imports System.Xml
Imports EwECore
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Forms
Imports WeifenLuo.WinFormsUI
Imports ScientificInterfaceShared
Imports WeifenLuo.WinFormsUI.Docking
Imports EwEUtils.Core

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Helper class; maintains form enabled / availability states in the AppLauncher.
''' </summary>
''' -----------------------------------------------------------------------
Friend Class cEwEFormStateManager
    Implements IDisposable

#Region " Privates "

    ''' <summary>Core state monitor that is being observed.</summary>
    Private m_csm As cCoreStateMonitor = Nothing
    ''' <summary>Dock panel containing the forms to maintain.</summary>
    Private m_dp As DockPanel = Nothing
    ''' <summary>Core controller to work with.</summary>
    Private m_cc As cCoreController = Nothing

#End Region ' Privates

#Region " Construction "

    Public Sub New(ByVal csm As cCoreStateMonitor,
                   ByVal cc As cCoreController,
                   ByVal dp As DockPanel)
        Me.m_dp = dp
        Me.m_cc = cc
        Me.m_csm = csm

        AddHandler Me.m_csm.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged
    End Sub

    Public Sub Dispose() _
        Implements IDisposable.Dispose

        If (Me.m_csm IsNot Nothing) Then
            RemoveHandler Me.m_csm.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged
            Me.m_dp = Nothing
            Me.m_cc = Nothing
            Me.m_csm = Nothing
        End If
        GC.SuppressFinalize(Me)

    End Sub

#End Region ' Construction

#Region " Events "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; responds to core execution state changes.
    ''' </summary>
    ''' <param name="csm">Core state monitor that threw the event.</param>
    ''' -------------------------------------------------------------------
    Private Sub OnCoreExecutionStateChanged(ByVal csm As cCoreStateMonitor)
        Me.UpdateFormStates()
    End Sub

#End Region ' Events

#Region " Internals "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns a list of all currently opened 
    ''' <see cref="frmEwE">frmEwE-derived forms</see>.
    ''' </summary>
    ''' <returns></returns>
    ''' -------------------------------------------------------------------
    Private Function GetEwEForms() As List(Of frmEwE)

        Dim l As New List(Of frmEwE)
        Dim f As frmEwE = Nothing

        ' Assess all docked windows
        If (Me.m_dp IsNot Nothing) Then
            For Each idc As IDockContent In Me.m_dp.Documents
                If (TypeOf idc Is frmEwE) Then
                    f = DirectCast(idc, frmEwE)
                    l.Add(f)
                End If
            Next
        End If

        ' Assess all floating windows
        For Each fw As FloatWindow In Me.m_dp.FloatWindows
            For iPane As Integer = 0 To fw.VisibleNestedPanes.Count - 1
                For Each idc As IDockContent In fw.VisibleNestedPanes(iPane).Contents
                    If (TypeOf idc Is frmEwE) Then
                        f = DirectCast(idc, frmEwE)
                        If (l.IndexOf(f) = -1) Then
                            l.Add(f)
                        End If
                    End If
                Next
            Next
        Next

        Return l

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Manage form states in response to the core execution state.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub UpdateFormStates()

        Dim stateForm As eCoreExecutionState = eCoreExecutionState.Idle
        Dim bMustCloseForm As Boolean = False
        Dim bMustDisableForm As Boolean = False

        ' JS 09Jan11: The application should NOT attempt to update the core state
        '             if the application is amidst responding to a core state change.
        '             Thus, while executing the following loop, the core controller
        '             should be disabled.
        Me.m_cc.Enabled = False

        Try

            For Each f As frmEwE In Me.GetEwEForms()

                ' Get form state
                stateForm = f.CoreExecutionState

                ' Check if form should be closed
                ' A form should be closed if its outputs are invalidated or then the data 
                ' used to populate the form are no longer available.
                If frmEwE.IsOutputForm(stateForm) Then
                    Select Case stateForm
                        Case eCoreExecutionState.EcopathCompleted
                            bMustCloseForm = Not Me.m_csm.HasEcopathRan
                        Case eCoreExecutionState.EcosimCompleted
                            bMustCloseForm = Not Me.m_csm.HasEcosimRan
                        Case eCoreExecutionState.EcospaceCompleted
                            bMustCloseForm = Not Me.m_csm.HasEcospaceRan
                    End Select
                Else
                    bMustCloseForm = Me.m_csm.IsExecutionStateSuperceded(stateForm) = False

                    ' Check if form should be disabled
                    ' A form should be disabled if it is an input form; path, sim or space are running,
                    ' and the form is not used to start runs.
                    bMustDisableForm = (Me.m_csm.IsBusy) And (Not f.IsRunForm()) And (Not Me.m_csm.IsPaused())
                End If

                If bMustCloseForm Then
                    If Not f.IsDisposed Then
                        Try
                            ' #Yes: Close the form
                            f.Close()
                        Catch ex As Exception
                            cLog.Write(ex, "cEwEFormStateHelper.UpdateFormState(" & f.Name & ")")
                        End Try
                    End If
                Else
                    ' #No: update enabled state
                    f.Enabled = (bMustDisableForm = False)
                End If

            Next f

        Catch ex As Exception
            ' Whoah!
            cLog.Write("cEwEFormStateHelper: " & ex.Message)
        End Try

        Me.m_cc.Enabled = True

    End Sub

#End Region ' Internals

End Class
