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
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Core
Imports System.ComponentModel

#End Region ' Imports

Public Class ucOptions

    Private m_uic As cUIContext = Nothing
    Private m_man As cNetworkManager = Nothing
    Private m_bInUpdate As Boolean = False
    Private m_cbh As cCheckboxHierarchy = Nothing

    Public Sub New(ByVal uic As cUIContext,
                   ByVal man As cNetworkManager)

        Me.m_uic = uic
        Me.m_man = man

        Me.InitializeComponent()

    End Sub

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_bInUpdate = True
        Me.m_cbUseTimeout.Checked = Me.m_man.UseAbortTimer
        Me.m_nudTimeOut.Value = CInt(Me.m_man.TimeOutMilSecs / (1000 * 60))

        Me.m_cbh = New cCheckboxHierarchy(Me.m_cbAutosaveRoot)
        Me.m_cbh.Add(Me.m_cbAutosaveEcopath, Me.m_cbAutosaveRoot)
        Me.m_cbh.Add(Me.m_cbAutosaveEcosimWoPPR, Me.m_cbAutosaveRoot)
        Me.m_cbh.Add(Me.m_cbAutosaveEcosimWithPPR, Me.m_cbAutosaveRoot)
        Me.m_cbh.ManageCheckedStates = True

        AddHandler My.Settings.PropertyChanged, AddressOf OnSettingsChanged

        Me.m_bInUpdate = False

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing And Me.components IsNot Nothing Then
                Me.m_cbh.Dispose()
                Me.components.Dispose()
                RemoveHandler My.Settings.PropertyChanged, AddressOf OnSettingsChanged
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub OnTimeOutCheckChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_cbUseTimeout.CheckedChanged

        If Me.m_bInUpdate Then Return

        Try
            Me.m_man.UseAbortTimer = m_cbUseTimeout.Checked
            My.Settings.UseAbortTimer = m_cbUseTimeout.Checked
            My.Settings.Save()
        Catch ex As Exception
            cLog.Write(ex)
        End Try
        Me.UpdateControls()

    End Sub

    Private Sub OnTimeOutChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_nudTimeOut.Validated

        If Me.m_bInUpdate Then Return

        Try
            Me.m_man.TimeOutMilSecs = CInt(Me.m_nudTimeOut.Value * 1000 * 60)
            My.Settings.AbortTimoutMins = CInt(Me.m_nudTimeOut.Value)
            My.Settings.Save()
        Catch ex As Exception
            cLog.Write(ex)
        End Try
        Me.UpdateControls()

    End Sub

    Private Sub UpdateControls()

        Me.m_bInUpdate = True

        Me.m_nudTimeOut.Enabled = Me.m_man.UseAbortTimer
        Me.m_lblTimeout.Enabled = Me.m_man.UseAbortTimer
        Me.m_lblTimeOutUnit.Enabled = Me.m_man.UseAbortTimer

        Me.m_cbAutosaveEcopath.Checked = My.Settings.AutosaveEcopath
        Me.m_cbAutosaveEcosimWoPPR.Checked = My.Settings.AutosaveEcosimWoPPR
        Me.m_cbAutosaveEcosimWithPPR.Checked = My.Settings.AutosaveEcosimWithPPR

        Me.m_bInUpdate = False

    End Sub

#Region " Event handlers "

    Private Sub OnSaveEcopathChecked(sender As Object, e As EventArgs) Handles m_cbAutosaveEcopath.CheckedChanged

        If Me.m_bInUpdate Then Return

        My.Settings.AutosaveEcopath = Me.m_cbAutosaveEcopath.Checked
        My.Settings.Save()

    End Sub

    Private Sub OnSaveEcosimWoPPRChecked(sender As Object, e As EventArgs) Handles m_cbAutosaveEcosimWoPPR.CheckedChanged

        If Me.m_bInUpdate Then Return

        My.Settings.AutosaveEcosimWoPPR = Me.m_cbAutosaveEcosimWoPPR.Checked
        If (My.Settings.AutosaveEcosimWoPPR) Then My.Settings.AutosaveEcosimWithPPR = False
        My.Settings.Save()

    End Sub

    Private Sub OnSaveEcosimWithPPRChecked(sender As Object, e As EventArgs) Handles m_cbAutosaveEcosimWithPPR.CheckedChanged

        If Me.m_bInUpdate Then Return

        My.Settings.AutosaveEcosimWithPPR = Me.m_cbAutosaveEcosimWithPPR.Checked
        If (My.Settings.AutosaveEcosimWithPPR) Then My.Settings.AutosaveEcosimWoPPR = False
        My.Settings.Save()

    End Sub

    Private Sub OnSettingsChanged(sender As Object, e As PropertyChangedEventArgs)
        Me.UpdateControls()
    End Sub

#End Region ' Event handlers

End Class
