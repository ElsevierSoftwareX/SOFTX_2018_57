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

Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms

Partial Class frmUI
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUI))
        Me.m_cbEnable = New System.Windows.Forms.CheckBox()
        Me.m_grid = New EwEModelFromEcosimPlugin.gridUI()
        Me.m_lblPath = New System.Windows.Forms.Label()
        Me.m_tbxOutputPath = New System.Windows.Forms.TextBox()
        Me.m_btnChoose = New System.Windows.Forms.Button()
        Me.m_cmbBACalcType = New System.Windows.Forms.ComboBox()
        Me.m_lblBACalcType = New System.Windows.Forms.Label()
        Me.m_lblNumYears = New System.Windows.Forms.Label()
        Me.m_nudNumYears = New System.Windows.Forms.NumericUpDown()
        Me.m_tbxWeightPower = New System.Windows.Forms.TextBox()
        Me.m_lblDWP = New System.Windows.Forms.Label()
        Me.m_tcMain = New System.Windows.Forms.TabControl()
        Me.m_tabOutput = New System.Windows.Forms.TabPage()
        Me.m_nudTimeStep = New System.Windows.Forms.NumericUpDown()
        Me.m_lblTimeStep = New System.Windows.Forms.Label()
        Me.m_cmbFormat = New System.Windows.Forms.ComboBox()
        Me.m_lblFormat = New System.Windows.Forms.Label()
        Me.m_tabBA = New System.Windows.Forms.TabPage()
        Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tslQuickSelect = New System.Windows.Forms.ToolStripLabel()
        Me.m_tsbnAll = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnNone = New System.Windows.Forms.ToolStripButton()
        CType(Me.m_nudNumYears, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tcMain.SuspendLayout()
        Me.m_tabOutput.SuspendLayout()
        CType(Me.m_nudTimeStep, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tabBA.SuspendLayout()
        Me.m_ts.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_cbEnable
        '
        resources.ApplyResources(Me.m_cbEnable, "m_cbEnable")
        Me.m_cbEnable.Name = "m_cbEnable"
        Me.m_cbEnable.UseVisualStyleBackColor = True
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        resources.ApplyResources(Me.m_grid, "m_grid")
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        Me.m_grid.AutoStretchColumnsToFitWidth = True
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.m_grid.ContextMenuStyle = SourceGrid2.ContextMenuStyle.None
        Me.m_grid.CustomSort = False
        Me.m_grid.Data = Nothing
        Me.m_grid.DataName = "EcopathModelsFromEcosim"
        Me.m_grid.FixedColumnWidths = False
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = False
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = True
        Me.m_grid.Name = "m_grid"
        Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_grid.TrackPropertySelection = False
        Me.m_grid.UIContext = Nothing
        '
        'm_lblPath
        '
        resources.ApplyResources(Me.m_lblPath, "m_lblPath")
        Me.m_lblPath.Name = "m_lblPath"
        '
        'm_tbxOutputPath
        '
        resources.ApplyResources(Me.m_tbxOutputPath, "m_tbxOutputPath")
        Me.m_tbxOutputPath.Name = "m_tbxOutputPath"
        Me.m_tbxOutputPath.ReadOnly = True
        '
        'm_btnChoose
        '
        resources.ApplyResources(Me.m_btnChoose, "m_btnChoose")
        Me.m_btnChoose.Name = "m_btnChoose"
        Me.m_btnChoose.UseVisualStyleBackColor = True
        '
        'm_cmbBACalcType
        '
        resources.ApplyResources(Me.m_cmbBACalcType, "m_cmbBACalcType")
        Me.m_cmbBACalcType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbBACalcType.FormattingEnabled = True
        Me.m_cmbBACalcType.Name = "m_cmbBACalcType"
        Me.m_cmbBACalcType.Sorted = True
        '
        'm_lblBACalcType
        '
        resources.ApplyResources(Me.m_lblBACalcType, "m_lblBACalcType")
        Me.m_lblBACalcType.Name = "m_lblBACalcType"
        '
        'm_lblNumYears
        '
        resources.ApplyResources(Me.m_lblNumYears, "m_lblNumYears")
        Me.m_lblNumYears.Name = "m_lblNumYears"
        '
        'm_nudNumYears
        '
        resources.ApplyResources(Me.m_nudNumYears, "m_nudNumYears")
        Me.m_nudNumYears.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudNumYears.Name = "m_nudNumYears"
        Me.m_nudNumYears.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_tbxWeightPower
        '
        resources.ApplyResources(Me.m_tbxWeightPower, "m_tbxWeightPower")
        Me.m_tbxWeightPower.Name = "m_tbxWeightPower"
        '
        'm_lblDWP
        '
        resources.ApplyResources(Me.m_lblDWP, "m_lblDWP")
        Me.m_lblDWP.Name = "m_lblDWP"
        '
        'm_tcMain
        '
        resources.ApplyResources(Me.m_tcMain, "m_tcMain")
        Me.m_tcMain.Controls.Add(Me.m_tabOutput)
        Me.m_tcMain.Controls.Add(Me.m_tabBA)
        Me.m_tcMain.Name = "m_tcMain"
        Me.m_tcMain.SelectedIndex = 0
        '
        'm_tabOutput
        '
        Me.m_tabOutput.Controls.Add(Me.m_nudTimeStep)
        Me.m_tabOutput.Controls.Add(Me.m_lblTimeStep)
        Me.m_tabOutput.Controls.Add(Me.m_cmbFormat)
        Me.m_tabOutput.Controls.Add(Me.m_cbEnable)
        Me.m_tabOutput.Controls.Add(Me.m_lblFormat)
        Me.m_tabOutput.Controls.Add(Me.m_lblPath)
        Me.m_tabOutput.Controls.Add(Me.m_tbxOutputPath)
        Me.m_tabOutput.Controls.Add(Me.m_btnChoose)
        resources.ApplyResources(Me.m_tabOutput, "m_tabOutput")
        Me.m_tabOutput.Name = "m_tabOutput"
        Me.m_tabOutput.UseVisualStyleBackColor = True
        '
        'm_nudTimeStep
        '
        resources.ApplyResources(Me.m_nudTimeStep, "m_nudTimeStep")
        Me.m_nudTimeStep.Maximum = New Decimal(New Integer() {12, 0, 0, 0})
        Me.m_nudTimeStep.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudTimeStep.Name = "m_nudTimeStep"
        Me.m_nudTimeStep.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_lblTimeStep
        '
        resources.ApplyResources(Me.m_lblTimeStep, "m_lblTimeStep")
        Me.m_lblTimeStep.Name = "m_lblTimeStep"
        '
        'm_cmbFormat
        '
        Me.m_cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbFormat.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbFormat, "m_cmbFormat")
        Me.m_cmbFormat.Name = "m_cmbFormat"
        '
        'm_lblFormat
        '
        resources.ApplyResources(Me.m_lblFormat, "m_lblFormat")
        Me.m_lblFormat.Name = "m_lblFormat"
        '
        'm_tabBA
        '
        Me.m_tabBA.Controls.Add(Me.m_lblBACalcType)
        Me.m_tabBA.Controls.Add(Me.m_lblDWP)
        Me.m_tabBA.Controls.Add(Me.m_cmbBACalcType)
        Me.m_tabBA.Controls.Add(Me.m_lblNumYears)
        Me.m_tabBA.Controls.Add(Me.m_tbxWeightPower)
        Me.m_tabBA.Controls.Add(Me.m_nudNumYears)
        resources.ApplyResources(Me.m_tabBA, "m_tabBA")
        Me.m_tabBA.Name = "m_tabBA"
        Me.m_tabBA.UseVisualStyleBackColor = True
        '
        'm_ts
        '
        resources.ApplyResources(Me.m_ts, "m_ts")
        Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslQuickSelect, Me.m_tsbnAll, Me.m_tsbnNone})
        Me.m_ts.Name = "m_ts"
        Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.m_ts.Stretch = True
        '
        'm_tslQuickSelect
        '
        Me.m_tslQuickSelect.Name = "m_tslQuickSelect"
        resources.ApplyResources(Me.m_tslQuickSelect, "m_tslQuickSelect")
        '
        'm_tsbnAll
        '
        Me.m_tsbnAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnAll, "m_tsbnAll")
        Me.m_tsbnAll.Name = "m_tsbnAll"
        '
        'm_tsbnNone
        '
        Me.m_tsbnNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnNone, "m_tsbnNone")
        Me.m_tsbnNone.Name = "m_tsbnNone"
        '
        'frmUI
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ControlBox = False
        Me.Controls.Add(Me.m_ts)
        Me.Controls.Add(Me.m_tcMain)
        Me.Controls.Add(Me.m_grid)
        Me.Name = "frmUI"
        Me.TabText = ""
        CType(Me.m_nudNumYears, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tcMain.ResumeLayout(False)
        Me.m_tabOutput.ResumeLayout(False)
        Me.m_tabOutput.PerformLayout()
        CType(Me.m_nudTimeStep, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tabBA.ResumeLayout(False)
        Me.m_tabBA.PerformLayout()
        Me.m_ts.ResumeLayout(False)
        Me.m_ts.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_grid As gridUI
    Private WithEvents m_cbEnable As System.Windows.Forms.CheckBox
    Private WithEvents m_lblPath As System.Windows.Forms.Label
    Private WithEvents m_btnChoose As System.Windows.Forms.Button
    Private WithEvents m_lblBACalcType As System.Windows.Forms.Label
    Private WithEvents m_cmbBACalcType As System.Windows.Forms.ComboBox
    Private WithEvents m_tbxOutputPath As System.Windows.Forms.TextBox
    Private WithEvents m_lblNumYears As System.Windows.Forms.Label
    Private WithEvents m_nudNumYears As System.Windows.Forms.NumericUpDown
    Private WithEvents m_tbxWeightPower As System.Windows.Forms.TextBox
    Private WithEvents m_lblDWP As System.Windows.Forms.Label
    Private WithEvents m_tcMain As System.Windows.Forms.TabControl
    Private WithEvents m_tabOutput As System.Windows.Forms.TabPage
    Private WithEvents m_cmbFormat As System.Windows.Forms.ComboBox
    Private WithEvents m_lblFormat As System.Windows.Forms.Label
    Private WithEvents m_tabBA As System.Windows.Forms.TabPage
    Private WithEvents m_ts As cEwEToolstrip
    Private WithEvents m_tsbnAll As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnNone As System.Windows.Forms.ToolStripButton
    Private WithEvents m_nudTimeStep As System.Windows.Forms.NumericUpDown
    Friend WithEvents m_lblTimeStep As System.Windows.Forms.Label
    Friend WithEvents m_tslQuickSelect As System.Windows.Forms.ToolStripLabel
End Class
