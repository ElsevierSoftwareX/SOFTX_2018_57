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
' Copyright 1991- Ecopath International Initiative, Barcelona, Spain and
'                 Joint Reseach Centre, Ispra, Italy.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Utilities
Imports EwECore
Imports System.Reflection
Imports System.IO

#End Region ' Imports

''' <summary>
''' Main UI for the Aquamaps distribution envelope import plug-in
''' </summary>
Public Class frmImport

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_data As cImportData = Nothing
    Private m_bDragOver As Boolean = False

#End Region ' Private vars 

#Region " Construction "

    Public Sub New(uic As cUIContext)
        MyBase.New()
        Me.m_uic = uic
        Me.m_data = New cImportData()
        Me.InitializeComponent()
    End Sub

#End Region ' Construction

#Region " Form overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
        Dim cmd As cBrowserCommand = CType(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
        cmd.AddControl(Me.m_pbAquamaps, "http://aquamaps.org")
        cmd.AddControl(Me.m_pbJRC, "https://ec.europa.eu/jrc/")

        Me.UpdateControls()
        Me.CenterToParent()
    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

        Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
        Dim cmd As cCommand = cmdh.GetCommand(cBrowserCommand.COMMAND_NAME)
        cmd.RemoveControl(Me.m_pbAquamaps)
        cmd.RemoveControl(Me.m_pbJRC)

        MyBase.OnFormClosed(e)
    End Sub

#End Region ' Form overrides

#Region " Events "

    Private Sub OnFilesDropped(sender As Object, files() As String) _
        Handles m_lblDrop.OnFilesDropped
        Try
            Me.ReadFiles(files)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnSelectFile(sender As System.Object, e As System.EventArgs) _
        Handles m_lblDrop.Click
        Try
            Dim cmd As cFileOpenCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)
            cmd.AllowMultiple = True

            cmd.Title = My.Resources.CAPTION_LOAD_HSPEN
            cmd.Filters = ScientificInterfaceShared.My.Resources.FILEFILTER_CSV
            cmd.Invoke()

            If (cmd.Result = System.Windows.Forms.DialogResult.OK) Then
                Me.ReadFiles(cmd.FileNames)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnImport(sender As System.Object, e As System.EventArgs) _
        Handles m_btnImport.Click

        Try
            Dim imp As New cImporter(Me.m_data, Me.m_uic)

            Dim lstrSpecies As New List(Of String)
            For Each strSpecies As String In Me.m_clbxSpecies.CheckedItems
                lstrSpecies.Add(strSpecies)
            Next

            Dim lstrEnvelopes As New List(Of String)
            For Each strEnvelope As String In Me.m_clbxEnvelopes.CheckedItems
                lstrEnvelopes.Add(strEnvelope)
            Next

            imp.Import(lstrSpecies.ToArray, lstrEnvelopes.ToArray)
            Me.Clear()

        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

#End Region ' Events

#Region " Internals "

    Private Sub Clear()
        Me.m_data.Clear()
        Me.m_clbxSpecies.Items.Clear()
        Me.m_clbxEnvelopes.Items.Clear()
    End Sub

    Private Sub ReadFiles(files As String())

        Dim reader As New cFileReader(Me.m_uic.Core)

        Me.Clear()

        For Each strFile As String In files
            reader.ReadEnvelopeData(strFile, Me.m_data)
        Next

        For Each strSpecies As String In Me.m_data.Species
            Dim i As Integer = Me.m_clbxSpecies.Items.Add(strSpecies)
            Me.m_clbxSpecies.SetItemChecked(i, True)
        Next

        For Each strEnv As String In Me.m_data.Envelopes
            Dim i As Integer = Me.m_clbxEnvelopes.Items.Add(strEnv)
            Me.m_clbxEnvelopes.SetItemChecked(i, True)
        Next

        Me.UpdateControls()

    End Sub

    Private Sub UpdateControls()

        Me.m_btnImport.Enabled = (Me.m_data.Files.Length > 0)
        'If Me.m_bDragOver Then
        '    Me.m_lblDrop.BackColor = SystemColors.Highlight
        'Else
        '    Me.m_lblDrop.BackColor = Drawing.Color.Transparent
        'End If

    End Sub

    Private Sub VisitURL(ByVal strURL As String)
        Try
            Dim cmd As cBrowserCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            cmd.Invoke(strURL)
        Catch ex As Exception
            cLog.Write(ex, "AquamapsImporter::VisitURL(" & strURL & ")")
        End Try

    End Sub

    Private Sub OnViewExample(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) _
        Handles m_lllblExample.LinkClicked

        Dim strFile As String = cFileUtils.MakeTempFile(".csv")
        Dim msg As cMessage = Nothing

        Try
            cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True)

            Dim writer As New StreamWriter(strFile, False)
            writer.Write(My.Resources.HSPEN_example_csv)
            writer.Flush()
            writer.Close()

            msg = New cMessage(String.Format(My.Resources.STATUS_EXAMPLE_SAVE_SUCCESS, strFile), _
                               eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Information)
            msg.Hyperlink = IO.Path.GetDirectoryName(strFile)

            Me.VisitURL(strFile)

        Catch ex As Exception
            msg = New cMessage(String.Format(My.Resources.STATUS_EXAMPLE_SAVE_FAILED, strFile), _
                               eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Critical)
        End Try
        Me.m_uic.Core.Messages.SendMessage(msg)
    End Sub

#End Region ' Internals

End Class