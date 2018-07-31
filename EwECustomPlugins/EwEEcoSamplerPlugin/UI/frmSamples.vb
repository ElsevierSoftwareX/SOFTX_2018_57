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

Imports System.Windows.Forms
Imports EwECore
Imports EwECore.Samples
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

' ToDo: make record button image blink with a timer

Public Class frmSamples

#Region " Private vars "

    Private m_plugin As EwEEcosamplerPlugin = Nothing

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New(uic As cUIContext, plugin As EwEEcosamplerPlugin)
        Me.InitializeComponent()

        Me.UIContext = uic
        Me.m_plugin = plugin
        Me.Text = My.Resources.TABTEXT
        Me.TabText = My.Resources.TABTEXT

        Me.m_grid.UIContext = uic

    End Sub

#End Region ' Construction / destruction

#Region " Form overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext IsNot Nothing) Then
            AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcopathSample}

            Dim man As cEcopathSampleManager = Me.Core.SampleManager
            Me.m_nudNumSamples.Value = man.nSamples
        End If

        Me.Icon = System.Drawing.Icon.FromHandle(My.Resources.SampleHS.GetHicon)

        ' Not supported (yet)
        Me.m_tsmiImportCefas.Visible = False

        Me.LoadSamples()

    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

        If (Me.UIContext IsNot Nothing) Then
            RemoveHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged
            Me.CoreComponents = New eCoreComponentType() {}
        End If

        Me.Icon.Dispose()
        MyBase.OnFormClosed(e)

    End Sub

    Public Overrides ReadOnly Property IsRunForm As Boolean
        Get
            Return True
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to prevent panel from closing with 'close all docs'
    ''' </summary>
    ''' <returns>Cheese!</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function PanelType() As frmEwEDockContent.ePanelType
        Return ePanelType.SystemPanel
    End Function

    Protected Overrides Sub UpdateControls()

        MyBase.UpdateControls()

        Dim man As cEcopathSampleManager = Me.Core.SampleManager
        Dim bIsRunning As Boolean = Me.Core.StateMonitor.IsBusy
        Dim bHasSelection As Boolean = (Me.SelectedRow > 0)
        Dim bHasSamples As Boolean = (man.nSamples > 0)
        Dim bIsLoaded As Boolean = bHasSelection And (man.IsLoaded(Me.SelectedSample()))

        If (Me.m_plugin IsNot Nothing) Then
            Me.m_tsbnRecord.Checked = Me.m_plugin.IsRecording
            Me.m_tsbnRecord.Image = If(Me.m_plugin.IsRecording, My.Resources.RecordingHS, My.Resources.RecordHS)
        End If

        Me.m_tsddImport.Image = SharedResources.ImportHS

        Me.m_btnLoad.Enabled = Not bIsRunning And bHasSelection
        Me.m_btnLoad.Text = If(bIsLoaded, My.Resources.LABEL_UNLOAD, My.Resources.LABEL_LOAD)
        Me.m_btnDelete.Enabled = Not bIsRunning And bHasSelection

        Me.m_nudNumSamples.Value = Math.Min(Me.m_nudNumSamples.Value, man.nSamples)
        Me.m_nudNumSamples.Maximum = man.nSamples

        Me.m_btnRun.Enabled = Not bIsRunning And bHasSamples And (CInt(Me.m_nudNumSamples.Value) > 0)

        Me.m_grid.Enabled = Not bIsRunning
        Me.m_tsMain.Enabled = Not bIsRunning

    End Sub

    Public Overrides Sub OnCoreMessage(msg As EwECore.cMessage)
        MyBase.OnCoreMessage(msg)

        Try
            If (msg.Source = eCoreComponentType.EcopathSample) Then
                Select Case msg.Type
                    Case eMessageType.DataAddedOrRemoved
                        Me.LoadSamples()
                    Case eMessageType.DataModified
                        Me.m_grid.UpdateLoadState()
                        Me.UpdateControls()
                End Select
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

#End Region ' Form overrides

#Region " Event handlers "

    Private m_bInUpdate As Boolean = False

    Private Sub OnToggleEnableRecording(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnRecord.CheckedChanged

        If (Me.m_plugin Is Nothing) Then Return

        Try
            Me.m_plugin.IsRecording = Me.m_tsbnRecord.Checked
            Me.UpdateControls()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnDeleteSample(sender As Object, e As System.EventArgs) _
        Handles m_btnDelete.Click

        If (Me.Core Is Nothing) Then Return

        Try
            Me.Core.SampleManager.Delete(Me.SelectedSamples())
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnGridSelectionChanged() _
        Handles m_grid.OnSelectionChanged

        Try
            Me.UpdateControls()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnLoadSample(sender As System.Object, e As System.EventArgs) _
        Handles m_btnLoad.Click

        If (Me.Core Is Nothing) Then Return

        Try
            Dim s As cEcopathSample = Me.m_grid.Sample(Me.m_grid.SelectedRow)
            Dim man As cEcopathSampleManager = Me.Core.SampleManager

            ' Toggle load
            If (s IsNot Nothing) And (man.IsLoaded(s)) Then s = Nothing
            Me.Core.SampleManager.Load(s, True)

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnNumSamplesChanged(sender As Object, e As System.EventArgs) _
        Handles m_nudNumSamples.ValueChanged
        Try
            Me.UpdateControls()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub OnRunBatch(sender As System.Object, e As System.EventArgs) _
        Handles m_btnRun.Click
        Try
            If Not Me.Core.SaveChanges() Then Return
            ' ToDo: globalize this
            Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.PROMPT_BATCHRUN, CInt(Me.m_nudNumSamples.Value)),
                                             eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO, defaultReply:=eMessageReply.NO)
            Core.Messages.SendMessage(fmsg)
            If (fmsg.Reply = eMessageReply.YES) Then
                Me.Core.SampleManager.Run(CInt(Me.m_nudNumSamples.Value), Me.m_cbBatchRandomize.Checked)
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub OnCoreStateChanged(ByVal cms As cCoreStateMonitor)
        Try
            Me.UpdateControls()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub OnImportFromModel(sender As System.Object, e As System.EventArgs) _
        Handles m_tsmiImportModel.Click

        ' ToDo: globalize this
        Try
            Dim man As cEcopathSampleManager = Me.Core.SampleManager
            Dim ofd As OpenFileDialog = cEwEFileDialogHelper.OpenFileDialog(My.Resources.PROMPT_IMPORT_MODEL, "", SharedResources.FILEFILTER_MODEL_OPEN)

            If (ofd.ShowDialog(Me.UIContext.FormMain) = System.Windows.Forms.DialogResult.OK) Then
                man.ImportFromModel(ofd.FileName)
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnImportCefasSamples(sender As System.Object, e As System.EventArgs) _
        Handles m_tsmiImportCefas.Click

        Me.Core.Messages.SendMessage(New cMessage("Cefas MSE sample import is not implemented yet", eMessageType.Any, eCoreComponentType.External, eMessageImportance.Information))

    End Sub

#End Region ' Event handlers

#Region " Internals "

    Private Sub LoadSamples()

        If (Me.IsDisposed) Then Return

        Try
            Me.m_grid.RefreshContent()
        Catch ex As Exception
            Debug.Assert(False)
        End Try

        Me.UpdateControls()

    End Sub

    Private Function SelectedRow() As Integer
        Return Me.m_grid.SelectedRow
    End Function

    Private Function SelectedSample() As cEcopathSample
        Return Me.m_grid.Sample(Me.m_grid.SelectedRow)
    End Function

    Private Function SelectedSamples() As cEcopathSample()
        Dim lSamples As New List(Of cEcopathSample)
        For Each i As Integer In Me.m_grid.SelectedRows
            Dim s As cEcopathSample = Me.m_grid.Sample(i)
            If (s IsNot Nothing) Then lSamples.Add(s)
        Next
        Return lSamples.ToArray()
    End Function

#End Region ' Internals

End Class