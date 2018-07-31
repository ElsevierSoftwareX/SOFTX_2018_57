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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Controls.EwEGrid

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Main form for the plug-in
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class frmUI

#Region " Private vars "

    Private m_data As cData = Nothing
    Private m_bReady As Boolean = False
    Private m_fmtBAType As New cBACalcTypeFormatter()
    Private m_fmtDatasourceType As New cDatasourceTypeFormatter()
    Private m_qeh As cQuickEditHandler = Nothing

#End Region ' Private vars

#Region " Construction "

    Public Sub New(ByVal data As cData)
        MyBase.New()
        Me.InitializeComponent()
        Me.m_data = data

    End Sub

#End Region ' Construction

#Region " Overrides "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IUIElement.UIContext"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Property UIContext() As cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As cUIContext)
            MyBase.UIContext = value
            Me.m_grid.UIContext = value
        End Set
    End Property

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        ' Initialize UI from data
        Try

            Me.m_cmbBACalcType.Items.Clear()
            For Each bac As cEcopathModelFromEcosim.eBACalcTypes In [Enum].GetValues(GetType(cEcopathModelFromEcosim.eBACalcTypes))
                Me.m_cmbBACalcType.Items.Add(bac)
            Next

            Me.m_cmbFormat.Items.Clear()
            Me.m_cmbFormat.Items.Add(eDataSourceTypes.Access2003)
            Me.m_cmbFormat.Items.Add(eDataSourceTypes.Access2007)

            Me.m_nudNumYears.Minimum = 1
            Me.m_nudNumYears.Maximum = Core.nEcosimYears
            Me.m_nudNumYears.Value = Math.Max(1, Math.Min(Me.m_data.NumYears, Me.m_nudNumYears.Maximum))

            Me.m_nudTimeStep.Minimum = 1
            Me.m_nudTimeStep.Maximum = CInt(Me.Core.nEcosimTimeSteps / Me.Core.nEcosimYears)
            Me.m_nudTimeStep.Value = Math.Max(1, Math.Min(Me.m_data.OutputTimeStep, CInt(Me.Core.nEcosimTimeSteps / Me.Core.nEcosimYears)))

            Me.m_tbxWeightPower.Text = Me.m_data.WPower.ToString
            Me.m_cmbBACalcType.SelectedItem = Me.m_data.BACalcMode
            Me.m_cmbFormat.SelectedItem = Me.m_data.OutputFormat

            Me.m_qeh = New cQuickEditHandler()
            Me.m_qeh.Attach(Me.m_grid, Me.UIContext, Me.m_ts)

            ' UI initialized, release form for normal operation
            Me.m_grid.Data = Me.m_data

        Catch ex As Exception
            ' This really should not happen
            Debug.Assert(False, ex.Message)
        End Try

        Me.m_bReady = True

        ' Initialize UI state
        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSim, eCoreComponentType.TimeSeries}
        Me.UpdateControls()

        AddHandler My.Settings.PropertyChanged, AddressOf OnSettingsChanged

        ' Populate form async
        Me.BeginInvoke(New MethodInvoker(AddressOf Me.m_grid.RefreshContent))

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        Me.m_qeh.Detach()
        Me.m_qeh = Nothing

        RemoveHandler My.Settings.PropertyChanged, AddressOf OnSettingsChanged

        Me.CoreComponents = Nothing
        MyBase.OnFormClosed(e)

    End Sub

    Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
        MyBase.OnCoreMessage(msg)

        If (msg.Source = EwEUtils.Core.eCoreComponentType.EcoSim) And _
           (msg.Type = eMessageType.EcosimNYearsChanged Or msg.Type = eMessageType.DataAddedOrRemoved) Then
            Me.UpdateEcosimRunTime()
        End If

        'If (msg.Source = EwEUtils.Core.eCoreComponentType.TimeSeries) And _
        '   (msg.Type = eMessageType.DataAddedOrRemoved) Then
        '    Me.UpdateEcosimRunTime()
        'End If

    End Sub

#End Region ' Overrides

#Region " Control events "

    Private Sub OnGenerateEnableChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbEnable.CheckedChanged

        If (Not Me.m_bReady) Then Return
        Try
            Me.Apply()
        Catch ex As Exception
            cLog.Write(ex, "frmUI::OnGenerateEnableChanged")
        End Try

    End Sub

    'Private Sub OnBrowseOutputPath(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    '    Handles m_btnChoose.Click

    '    If (Not Me.m_bReady) Then Return
    '    Try
    '        Dim cmd As cDirectoryOpenCommand = DirectCast(Me.CommandHandler.GetCommand(cDirectoryOpenCommand.COMMAND_NAME), cDirectoryOpenCommand)
    '        cmd.Invoke(Me.m_data.CustomOutputPath, My.Resources.PROMPT_OUTPUT_FOLDER)
    '        If (cmd.Result = DialogResult.OK) Then
    '            Me.m_tbxOutputPath.Text = cmd.Directory
    '            Me.Apply()
    '        End If
    '    Catch ex As Exception
    '        cLog.Write(ex, "frmUI::OnBrowseOutputPath")
    '    End Try

    'End Sub

    Private Sub OnOutputPathTextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tbxOutputPath.TextChanged

        If (Not Me.m_bReady) Then Return
        Try
            Me.Apply()
        Catch ex As Exception
            cLog.Write(ex, "frmUI::OnOutputPathTextChanged")
        End Try

    End Sub

    Private Sub OnComboSelectionChanged(sender As Object, e As System.EventArgs) _
        Handles m_cmbBACalcType.SelectedIndexChanged, m_cmbFormat.SelectedIndexChanged

        If (Not Me.m_bReady) Then Return
        Try
            Me.Apply()
        Catch ex As Exception
            cLog.Write(ex, "frmUI::OnComboSelectionChanged")
        End Try

    End Sub

    Private Sub OnInputChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_tbxWeightPower.Validated, m_nudNumYears.Validated, m_nudTimeStep.Validated,
                m_tbxWeightPower.LostFocus, m_nudNumYears.LostFocus, m_nudTimeStep.LostFocus

        If (Not Me.m_bReady) Then Return
        Try
            Me.Apply()
        Catch ex As Exception
            cLog.Write(ex, "frmUI::OnWeightPowerChanged")
        End Try

    End Sub

    Private Sub OnFormatBACalcType(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbBACalcType.Format

        Try
            e.Value = Me.m_fmtBAType.GetDescriptor(DirectCast(e.ListItem, cEcopathModelFromEcosim.eBACalcTypes))
        Catch ex As Exception
            cLog.Write(ex, "frmUI::OnFormatBACalcType")
        End Try

    End Sub

    Private Sub OnFormatDatabaseType(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbFormat.Format

        Try
            e.Value = Me.m_fmtDatasourceType.GetDescriptor(DirectCast(e.ListItem, eDataSourceTypes))
        Catch ex As Exception
            cLog.Write(ex, "frmUI::OnFormatDatabaseType")
        End Try

    End Sub

    Private Sub OnSelectAllTimeSteps(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnAll.Click
        Try
            For i As Integer = 1 To Me.m_data.NumYears
                Me.m_data.CreateModel(i) = True
            Next
            Me.m_grid.RefreshContent()
        Catch ex As Exception
            cLog.Write(ex, "EwEModelFromEcosim.frmUI:SelectAll")
        End Try
    End Sub

    Private Sub OnClearAllTimeSteps(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnNone.Click
        Try
            For i As Integer = 1 To Me.m_data.NumYears
                Me.m_data.CreateModel(i) = False
            Next
            Me.m_grid.RefreshContent()
        Catch ex As Exception
            cLog.Write(ex, "EwEModelFromEcosim.frmUI:SelectNone")
        End Try
    End Sub

#End Region ' Control events

#Region " Settings events "

    Private Sub OnSettingsChanged(sender As Object, args As EventArgs)
        Me.UpdateControls()
    End Sub

#End Region ' Settings events

#Region " Internals "

    ''' <summary>
    ''' Write UI control contents to the engine.
    ''' </summary>
    Private Sub Apply()

        ' Only allow data updates when the plug-in is fully initialized
        If (Not Me.m_bReady) Then Return

        Me.m_data.Enabled = Me.m_cbEnable.Checked
        Me.m_data.NumYears = Me.Core.nEcosimYears
        'Me.m_data.CustomOutputPath = Me.m_tbxOutputPath.Text
        Me.m_data.OutputFormat = DirectCast(Me.m_cmbFormat.SelectedItem, eDataSourceTypes)
        Me.m_data.BACalcMode = DirectCast(Me.m_cmbBACalcType.SelectedIndex, cEcopathModelFromEcosim.eBACalcTypes)
        Me.m_data.BAAverageYears = CInt(Me.m_nudNumYears.Value)
        Me.m_data.OutputTimeStep = CInt(Me.m_nudTimeStep.Value)

        Dim w As Single
        If Single.TryParse(Me.m_tbxWeightPower.Text, w) Then
            Me.m_data.WPower = w
        End If

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub UpdateControls()

        Dim bReqWPower As Boolean = False
        Dim bReqNYears As Boolean = False

        Select Case Me.m_data.BACalcMode
            Case cEcopathModelFromEcosim.eBACalcTypes.FromEcosimYearsAverage
                bReqNYears = True
            Case cEcopathModelFromEcosim.eBACalcTypes.FromEcosimYearsWeightedAverage
                bReqWPower = True
                bReqNYears = True
        End Select

        Me.m_cbEnable.Checked = Me.m_data.Enabled
        Me.m_tbxOutputPath.Text = Me.m_data.OutputPath
        Me.m_tbxOutputPath.Enabled = Me.m_data.Enabled
        Me.m_btnChoose.Enabled = Me.m_data.Enabled
        Me.m_cmbFormat.Enabled = Me.m_data.Enabled
        Me.m_nudTimeStep.Enabled = Me.m_data.Enabled
        Me.m_cmbBACalcType.Enabled = Me.m_data.Enabled
        Me.m_nudNumYears.Enabled = Me.m_data.Enabled And bReqNYears
        Me.m_tbxWeightPower.Enabled = Me.m_data.Enabled And bReqWPower
        Me.m_grid.Enabled = Me.m_data.Enabled

        Me.m_nudTimeStep.Maximum = CInt(Me.Core.nEcosimTimeSteps / Me.Core.nEcosimYears)
        Me.m_nudNumYears.Maximum = Me.Core.nEcosimYears

    End Sub

    Protected Sub UpdateEcosimRunTime()

        ' This is a bit blunt, just to intercept the Ecosim changes..
        Me.Apply()
        ' But then again, this is just as blunt - Shazaam!
        Me.m_grid.RefreshContent()

    End Sub

#End Region ' Internals

End Class