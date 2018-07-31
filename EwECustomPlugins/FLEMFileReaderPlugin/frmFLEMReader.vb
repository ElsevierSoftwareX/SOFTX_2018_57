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
Imports System.IO
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class frmFLEMReader

    Private m_plugin As cFLEMPluginPoint
    Private m_bChanged As Boolean = False
    Private m_isInitializing As Boolean

    Public Sub New(ByVal uic As cUIContext, ByVal plugin As cFLEMPluginPoint)
        MyBase.New()
        Me.InitializeComponent()
        Me.m_plugin = plugin
        Me.UIContext = uic
    End Sub

#Region " Overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Try

            Me.m_isInitializing = True

            Me.m_tbxForceFile.Text = m_plugin.ForceFile
            Me.m_chkForcePP.Checked = m_plugin.ForcePPSalinity
            Me.m_chkForceHabCap.Checked = m_plugin.VaryHabCapWithCultch
            Me.m_chkUsePPMod.Checked = m_plugin.UsePPModifier

            Me.LoadGroups(Me.m_cmbHabCap)

            If m_plugin.HabCapModGroup <= Me.Core.nGroups Then
                m_cmbHabCap.SelectedIndex = m_plugin.HabCapModGroup - 1
            End If

            Me.m_bChanged = False
            Me.UpdateControls()

            If Me.Modal Then
                Me.CenterToScreen()
            End If

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString + ".OnLoad() Exception: " + ex.Message)
        End Try

        Me.m_isInitializing = False

    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Dim bValid As Boolean = False

        If Not String.IsNullOrWhiteSpace(Me.m_tbxForceFile.Text) Then
            bValid = File.Exists(Me.m_tbxForceFile.Text)
        End If

        ' Trick: disaply OK and Cancel when shown as dialog box
        Me.m_btnOK.Visible = Me.Modal
        Me.m_btnCancel.Visible = Me.Modal

        Me.m_btnOK.Enabled = Me.m_bChanged And bValid

    End Sub

#End Region ' Overrides

#Region " Control events "

    Private Sub OnchkForcePP_CheckedChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_chkForcePP.CheckedChanged
        Try
            'm_plugin.ForcePPSalinity = Me.m_chkForcePP.Checked
            Me.SetChanged()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnchkForceHabCap_CheckedChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_chkForceHabCap.CheckedChanged
        Try
            'm_plugin.VaryHabCapWithCultch = m_chkForceHabCap.Checked
            Me.SetChanged()
        Catch ex As Exception

        End Try
    End Sub


    Private Sub OnchkUsePPMod_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles m_chkUsePPMod.CheckedChanged
        Try
            Me.SetChanged()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btForcingFile_Click(sender As System.Object, e As System.EventArgs) _
        Handles m_btnChooseFile.Click

        Try

            Dim cmd As cFileOpenCommand = DirectCast(Me.CommandHandler.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)
            Debug.Assert(cmd IsNot Nothing)

            cmd.Invoke(m_tbxForceFile.Text, "FLEM files (*.nuo)|*.nuo|All files (*.*)|*.*", 0, "Select FLEM file to read")

            If (cmd.Result = Windows.Forms.DialogResult.OK) Then
                Me.m_tbxForceFile.Text = cmd.FileName
                Me.SetChanged()
            End If

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnGroupSelected(sender As System.Object, e As System.EventArgs) _
        Handles m_cmbHabCap.SelectedIndexChanged
        Try
            'm_plugin.HabCapModGroup = m_cmbHabCap.SelectedIndex + 1
            Me.SetChanged()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
        Handles m_btnOK.Click
        Try
            Me.Apply()
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
        Handles m_btnCancel.Click
        Try
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            Me.Close()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Control events

#Region " Internals "

    Private Sub LoadGroups(cbBox As Windows.Forms.ComboBox)

        ' Always show group with Index
        Dim fmt As New cCoreInterfaceFormatter()
        For igrp As Integer = 1 To Me.Core.nGroups
            cbBox.Items.Add(fmt.GetDescriptor(Me.Core.EcoPathGroupInputs(igrp)))
        Next

    End Sub

    Private Sub Apply()

        Try

            If Me.m_isInitializing Then
                'Don't save the changes during initialization
                Exit Sub
            End If

            'update the plugin with the values from the interface
            Me.m_plugin.ForceFile = Me.m_tbxForceFile.Text
            Me.m_plugin.HabCapModGroup = m_cmbHabCap.SelectedIndex + 1

            Me.m_plugin.VaryHabCapWithCultch = m_chkForceHabCap.Checked
            Me.m_plugin.ForcePPSalinity = Me.m_chkForcePP.Checked
            Me.m_plugin.UsePPModifier = Me.m_chkUsePPMod.Checked

            'Now save the updated values to the settings
            'VaryHabCapWithCultch is NOT saved in the Setting
            My.Settings.ForceFile = Me.m_plugin.ForceFile
            My.Settings.UsePPModifier = Me.m_plugin.UsePPModifier
            My.Settings.ForcePPSalinity = Me.m_plugin.ForcePPSalinity
            My.Settings.Save()

        Catch ex As Exception
            System.Console.WriteLine(Me.m_chkUsePPMod.ToString + ".Apply() Exception: " + ex.Message)
        End Try

    End Sub

    Private Sub SetChanged()

        If Not Me.Modal Then
            Me.Apply()
        End If

        Me.m_bChanged = True
        Me.UpdateControls()

    End Sub

#End Region ' Internals


End Class