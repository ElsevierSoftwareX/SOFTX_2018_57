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

Imports ScientificInterfaceShared.Forms
Imports ZedGraph

Namespace Ecosim

    Partial Class frmMCRun
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMCRun))
            Me.m_lblNumTrials = New System.Windows.Forms.Label()
            Me.m_btnRunTrials = New System.Windows.Forms.Button()
            Me.m_btnStop = New System.Windows.Forms.Button()
            Me.m_tcMain = New ScientificInterfaceShared.Controls.ucTabControlEx()
            Me.m_tbpSettings = New System.Windows.Forms.TabPage()
            Me.m_lblEnabledVariables = New System.Windows.Forms.Label()
            Me.m_clbEnabledVariables = New ScientificInterfaceShared.Controls.cFlickerFreeCheckedListBox()
            Me.m_cbRetainEstimates = New System.Windows.Forms.CheckBox()
            Me.m_btDefaultTol = New System.Windows.Forms.Button()
            Me.m_cbSRA = New System.Windows.Forms.CheckBox()
            Me.m_cbRetainCurPattern = New System.Windows.Forms.CheckBox()
            Me.m_lblFMratio = New System.Windows.Forms.Label()
            Me.m_lblEEtol = New System.Windows.Forms.Label()
            Me.m_tbxFMratio = New System.Windows.Forms.TextBox()
            Me.m_tbxEETol = New System.Windows.Forms.TextBox()
            Me.m_tbpB = New System.Windows.Forms.TabPage()
            Me.m_gridB = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsB = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnLoadPedB = New System.Windows.Forms.ToolStripButton()
            Me.m_tbpPB = New System.Windows.Forms.TabPage()
            Me.m_gridPB = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsPB = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnLoadPedPB = New System.Windows.Forms.ToolStripButton()
            Me.m_tbpQB = New System.Windows.Forms.TabPage()
            Me.m_gridQB = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsQB = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnLoadPedQB = New System.Windows.Forms.ToolStripButton()
            Me.m_tbpEE = New System.Windows.Forms.TabPage()
            Me.m_gridEE = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsEE = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tbpBA = New System.Windows.Forms.TabPage()
            Me.m_gridBA = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsBA = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tbpBABi = New System.Windows.Forms.TabPage()
            Me.m_gridBaBi = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsBaBi = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tbpDiets = New System.Windows.Forms.TabPage()
            Me.m_gridDiets = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsDiets = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tslblMethodDC = New System.Windows.Forms.ToolStripLabel()
            Me.m_tscmbMethodDC = New System.Windows.Forms.ToolStripComboBox()
            Me.m_tssepDC = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsbnLoadPedDC = New System.Windows.Forms.ToolStripButton()
            Me.m_tbpLandings = New System.Windows.Forms.TabPage()
            Me.m_gridLandings = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsLandings = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnLoadPedLandings = New System.Windows.Forms.ToolStripButton()
            Me.m_tbpDiscards = New System.Windows.Forms.TabPage()
            Me.m_gridDiscards = New ScientificInterface.Ecosim.gridMCRunInput()
            Me.m_tsDiscards = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnLoadPedDiscards = New System.Windows.Forms.ToolStripButton()
            Me.m_tbpBPlot = New System.Windows.Forms.TabPage()
            Me.m_spPlot = New System.Windows.Forms.SplitContainer()
            Me.m_graph = New ZedGraph.ZedGraphControl()
            Me.m_tsPlot = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnUpdatePlot = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnShowBestOnly = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnShowGroups = New System.Windows.Forms.ToolStripButton()
            Me.m_lbGroups = New ScientificInterfaceShared.Controls.cGroupListBox()
            Me.m_lblGroups = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tbpBestTrial = New System.Windows.Forms.TabPage()
            Me.m_gridBestFit = New ScientificInterface.Ecosim.gridMCRunOutput()
            Me.m_cbSave = New System.Windows.Forms.CheckBox()
            Me.m_lblTrial = New System.Windows.Forms.Label()
            Me.m_lblERun = New System.Windows.Forms.Label()
            Me.m_lblSScurr = New System.Windows.Forms.Label()
            Me.m_lblSSbest = New System.Windows.Forms.Label()
            Me.m_btnApply = New System.Windows.Forms.Button()
            Me.m_nudNumTrials = New System.Windows.Forms.NumericUpDown()
            Me.m_btnTS = New System.Windows.Forms.Button()
            Me.m_lblERunValue = New System.Windows.Forms.Label()
            Me.m_lblSSbestValue = New System.Windows.Forms.Label()
            Me.m_lblSScurrValue = New System.Windows.Forms.Label()
            Me.m_lblSSorgValue = New System.Windows.Forms.Label()
            Me.m_lblTrialValue = New System.Windows.Forms.Label()
            Me.m_lbSSOrg = New System.Windows.Forms.Label()
            Me.m_hdrInputOpt = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrOutputParam = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpOutputs = New System.Windows.Forms.TableLayoutPanel()
            Me.m_cmbSaveFormat = New System.Windows.Forms.ComboBox()
            Me.m_tcMain.SuspendLayout()
            Me.m_tbpSettings.SuspendLayout()
            Me.m_tbpB.SuspendLayout()
            Me.m_tsB.SuspendLayout()
            Me.m_tbpPB.SuspendLayout()
            Me.m_tsPB.SuspendLayout()
            Me.m_tbpQB.SuspendLayout()
            Me.m_tsQB.SuspendLayout()
            Me.m_tbpEE.SuspendLayout()
            Me.m_tbpBA.SuspendLayout()
            Me.m_tbpBABi.SuspendLayout()
            Me.m_tbpDiets.SuspendLayout()
            Me.m_tsDiets.SuspendLayout()
            Me.m_tbpLandings.SuspendLayout()
            Me.m_tsLandings.SuspendLayout()
            Me.m_tbpDiscards.SuspendLayout()
            Me.m_tsDiscards.SuspendLayout()
            Me.m_tbpBPlot.SuspendLayout()
            CType(Me.m_spPlot, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_spPlot.Panel1.SuspendLayout()
            Me.m_spPlot.Panel2.SuspendLayout()
            Me.m_spPlot.SuspendLayout()
            Me.m_tsPlot.SuspendLayout()
            Me.m_tbpBestTrial.SuspendLayout()
            CType(Me.m_nudNumTrials, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tlpOutputs.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_lblNumTrials
            '
            resources.ApplyResources(Me.m_lblNumTrials, "m_lblNumTrials")
            Me.m_lblNumTrials.Name = "m_lblNumTrials"
            '
            'm_btnRunTrials
            '
            resources.ApplyResources(Me.m_btnRunTrials, "m_btnRunTrials")
            Me.m_btnRunTrials.Name = "m_btnRunTrials"
            Me.m_btnRunTrials.UseVisualStyleBackColor = True
            '
            'm_btnStop
            '
            resources.ApplyResources(Me.m_btnStop, "m_btnStop")
            Me.m_btnStop.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnStop.Name = "m_btnStop"
            Me.m_btnStop.UseVisualStyleBackColor = True
            '
            'm_tcMain
            '
            resources.ApplyResources(Me.m_tcMain, "m_tcMain")
            Me.m_tcMain.Controls.Add(Me.m_tbpSettings)
            Me.m_tcMain.Controls.Add(Me.m_tbpB)
            Me.m_tcMain.Controls.Add(Me.m_tbpPB)
            Me.m_tcMain.Controls.Add(Me.m_tbpQB)
            Me.m_tcMain.Controls.Add(Me.m_tbpEE)
            Me.m_tcMain.Controls.Add(Me.m_tbpBA)
            Me.m_tcMain.Controls.Add(Me.m_tbpBABi)
            Me.m_tcMain.Controls.Add(Me.m_tbpDiets)
            Me.m_tcMain.Controls.Add(Me.m_tbpLandings)
            Me.m_tcMain.Controls.Add(Me.m_tbpDiscards)
            Me.m_tcMain.Controls.Add(Me.m_tbpBPlot)
            Me.m_tcMain.Controls.Add(Me.m_tbpBestTrial)
            Me.m_tcMain.Name = "m_tcMain"
            Me.m_tcMain.SelectedIndex = 0
            Me.m_tcMain.TextOrientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_tbpSettings
            '
            resources.ApplyResources(Me.m_tbpSettings, "m_tbpSettings")
            Me.m_tbpSettings.Controls.Add(Me.m_lblEnabledVariables)
            Me.m_tbpSettings.Controls.Add(Me.m_clbEnabledVariables)
            Me.m_tbpSettings.Controls.Add(Me.m_cbRetainEstimates)
            Me.m_tbpSettings.Controls.Add(Me.m_btDefaultTol)
            Me.m_tbpSettings.Controls.Add(Me.m_cbSRA)
            Me.m_tbpSettings.Controls.Add(Me.m_cbRetainCurPattern)
            Me.m_tbpSettings.Controls.Add(Me.m_lblFMratio)
            Me.m_tbpSettings.Controls.Add(Me.m_lblEEtol)
            Me.m_tbpSettings.Controls.Add(Me.m_tbxFMratio)
            Me.m_tbpSettings.Controls.Add(Me.m_tbxEETol)
            Me.m_tbpSettings.Name = "m_tbpSettings"
            Me.m_tbpSettings.UseVisualStyleBackColor = True
            '
            'm_lblEnabledVariables
            '
            resources.ApplyResources(Me.m_lblEnabledVariables, "m_lblEnabledVariables")
            Me.m_lblEnabledVariables.Name = "m_lblEnabledVariables"
            '
            'm_clbEnabledVariables
            '
            resources.ApplyResources(Me.m_clbEnabledVariables, "m_clbEnabledVariables")
            Me.m_clbEnabledVariables.CheckOnClick = True
            Me.m_clbEnabledVariables.FormattingEnabled = True
            Me.m_clbEnabledVariables.Name = "m_clbEnabledVariables"
            Me.m_clbEnabledVariables.Sorted = True
            '
            'm_cbRetainEstimates
            '
            resources.ApplyResources(Me.m_cbRetainEstimates, "m_cbRetainEstimates")
            Me.m_cbRetainEstimates.Name = "m_cbRetainEstimates"
            Me.m_cbRetainEstimates.UseVisualStyleBackColor = True
            '
            'm_btDefaultTol
            '
            resources.ApplyResources(Me.m_btDefaultTol, "m_btDefaultTol")
            Me.m_btDefaultTol.Name = "m_btDefaultTol"
            Me.m_btDefaultTol.UseVisualStyleBackColor = True
            '
            'm_cbSRA
            '
            resources.ApplyResources(Me.m_cbSRA, "m_cbSRA")
            Me.m_cbSRA.Name = "m_cbSRA"
            Me.m_cbSRA.UseVisualStyleBackColor = True
            '
            'm_cbRetainCurPattern
            '
            resources.ApplyResources(Me.m_cbRetainCurPattern, "m_cbRetainCurPattern")
            Me.m_cbRetainCurPattern.Name = "m_cbRetainCurPattern"
            Me.m_cbRetainCurPattern.UseVisualStyleBackColor = True
            '
            'm_lblFMratio
            '
            resources.ApplyResources(Me.m_lblFMratio, "m_lblFMratio")
            Me.m_lblFMratio.Name = "m_lblFMratio"
            '
            'm_lblEEtol
            '
            resources.ApplyResources(Me.m_lblEEtol, "m_lblEEtol")
            Me.m_lblEEtol.Name = "m_lblEEtol"
            '
            'm_tbxFMratio
            '
            resources.ApplyResources(Me.m_tbxFMratio, "m_tbxFMratio")
            Me.m_tbxFMratio.Name = "m_tbxFMratio"
            '
            'm_tbxEETol
            '
            resources.ApplyResources(Me.m_tbxEETol, "m_tbxEETol")
            Me.m_tbxEETol.Name = "m_tbxEETol"
            '
            'm_tbpB
            '
            Me.m_tbpB.Controls.Add(Me.m_gridB)
            Me.m_tbpB.Controls.Add(Me.m_tsB)
            resources.ApplyResources(Me.m_tbpB, "m_tbpB")
            Me.m_tbpB.Name = "m_tbpB"
            Me.m_tbpB.UseVisualStyleBackColor = True
            '
            'm_gridB
            '
            Me.m_gridB.AllowBlockSelect = True
            Me.m_gridB.AutoSizeMinHeight = 10
            Me.m_gridB.AutoSizeMinWidth = 10
            Me.m_gridB.AutoStretchColumnsToFitWidth = False
            Me.m_gridB.AutoStretchRowsToFitHeight = False
            Me.m_gridB.BackColor = System.Drawing.Color.White
            Me.m_gridB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridB.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridB.CustomSort = False
            Me.m_gridB.DataName = "MC_B"
            Me.m_gridB.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.B
            resources.ApplyResources(Me.m_gridB, "m_gridB")
            Me.m_gridB.FixedColumnWidths = False
            Me.m_gridB.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridB.GridToolTipActive = True
            Me.m_gridB.IsLayoutSuspended = False
            Me.m_gridB.IsOutputGrid = True
            Me.m_gridB.Name = "m_gridB"
            Me.m_gridB.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridB.UIContext = Nothing
            '
            'm_tsB
            '
            Me.m_tsB.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsB.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnLoadPedB})
            resources.ApplyResources(Me.m_tsB, "m_tsB")
            Me.m_tsB.Name = "m_tsB"
            Me.m_tsB.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnLoadPedB
            '
            resources.ApplyResources(Me.m_tsbnLoadPedB, "m_tsbnLoadPedB")
            Me.m_tsbnLoadPedB.Name = "m_tsbnLoadPedB"
            '
            'm_tbpPB
            '
            Me.m_tbpPB.Controls.Add(Me.m_gridPB)
            Me.m_tbpPB.Controls.Add(Me.m_tsPB)
            resources.ApplyResources(Me.m_tbpPB, "m_tbpPB")
            Me.m_tbpPB.Name = "m_tbpPB"
            Me.m_tbpPB.UseVisualStyleBackColor = True
            '
            'm_gridPB
            '
            Me.m_gridPB.AllowBlockSelect = True
            Me.m_gridPB.AutoSizeMinHeight = 10
            Me.m_gridPB.AutoSizeMinWidth = 10
            Me.m_gridPB.AutoStretchColumnsToFitWidth = False
            Me.m_gridPB.AutoStretchRowsToFitHeight = False
            Me.m_gridPB.BackColor = System.Drawing.Color.White
            Me.m_gridPB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridPB.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridPB.CustomSort = False
            Me.m_gridPB.DataName = "MC_PB"
            Me.m_gridPB.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.PB
            resources.ApplyResources(Me.m_gridPB, "m_gridPB")
            Me.m_gridPB.FixedColumnWidths = False
            Me.m_gridPB.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridPB.GridToolTipActive = True
            Me.m_gridPB.IsLayoutSuspended = False
            Me.m_gridPB.IsOutputGrid = True
            Me.m_gridPB.Name = "m_gridPB"
            Me.m_gridPB.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridPB.UIContext = Nothing
            '
            'm_tsPB
            '
            Me.m_tsPB.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsPB.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnLoadPedPB})
            resources.ApplyResources(Me.m_tsPB, "m_tsPB")
            Me.m_tsPB.Name = "m_tsPB"
            Me.m_tsPB.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnLoadPedPB
            '
            resources.ApplyResources(Me.m_tsbnLoadPedPB, "m_tsbnLoadPedPB")
            Me.m_tsbnLoadPedPB.Name = "m_tsbnLoadPedPB"
            '
            'm_tbpQB
            '
            Me.m_tbpQB.Controls.Add(Me.m_gridQB)
            Me.m_tbpQB.Controls.Add(Me.m_tsQB)
            resources.ApplyResources(Me.m_tbpQB, "m_tbpQB")
            Me.m_tbpQB.Name = "m_tbpQB"
            Me.m_tbpQB.UseVisualStyleBackColor = True
            '
            'm_gridQB
            '
            Me.m_gridQB.AllowBlockSelect = True
            Me.m_gridQB.AutoSizeMinHeight = 10
            Me.m_gridQB.AutoSizeMinWidth = 10
            Me.m_gridQB.AutoStretchColumnsToFitWidth = False
            Me.m_gridQB.AutoStretchRowsToFitHeight = False
            Me.m_gridQB.BackColor = System.Drawing.Color.White
            Me.m_gridQB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridQB.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridQB.CustomSort = False
            Me.m_gridQB.DataName = "MC_QB"
            Me.m_gridQB.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.QB
            resources.ApplyResources(Me.m_gridQB, "m_gridQB")
            Me.m_gridQB.FixedColumnWidths = False
            Me.m_gridQB.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridQB.GridToolTipActive = True
            Me.m_gridQB.IsLayoutSuspended = False
            Me.m_gridQB.IsOutputGrid = True
            Me.m_gridQB.Name = "m_gridQB"
            Me.m_gridQB.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridQB.UIContext = Nothing
            '
            'm_tsQB
            '
            Me.m_tsQB.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsQB.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnLoadPedQB})
            resources.ApplyResources(Me.m_tsQB, "m_tsQB")
            Me.m_tsQB.Name = "m_tsQB"
            Me.m_tsQB.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnLoadPedQB
            '
            resources.ApplyResources(Me.m_tsbnLoadPedQB, "m_tsbnLoadPedQB")
            Me.m_tsbnLoadPedQB.Name = "m_tsbnLoadPedQB"
            '
            'm_tbpEE
            '
            Me.m_tbpEE.Controls.Add(Me.m_gridEE)
            Me.m_tbpEE.Controls.Add(Me.m_tsEE)
            resources.ApplyResources(Me.m_tbpEE, "m_tbpEE")
            Me.m_tbpEE.Name = "m_tbpEE"
            Me.m_tbpEE.UseVisualStyleBackColor = True
            '
            'm_gridEE
            '
            Me.m_gridEE.AllowBlockSelect = True
            Me.m_gridEE.AutoSizeMinHeight = 10
            Me.m_gridEE.AutoSizeMinWidth = 10
            Me.m_gridEE.AutoStretchColumnsToFitWidth = False
            Me.m_gridEE.AutoStretchRowsToFitHeight = False
            Me.m_gridEE.BackColor = System.Drawing.Color.White
            Me.m_gridEE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridEE.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridEE.CustomSort = False
            Me.m_gridEE.DataName = "MC_EE"
            Me.m_gridEE.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.EE
            resources.ApplyResources(Me.m_gridEE, "m_gridEE")
            Me.m_gridEE.FixedColumnWidths = False
            Me.m_gridEE.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridEE.GridToolTipActive = True
            Me.m_gridEE.IsLayoutSuspended = False
            Me.m_gridEE.IsOutputGrid = True
            Me.m_gridEE.Name = "m_gridEE"
            Me.m_gridEE.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridEE.UIContext = Nothing
            '
            'm_tsEE
            '
            Me.m_tsEE.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            resources.ApplyResources(Me.m_tsEE, "m_tsEE")
            Me.m_tsEE.Name = "m_tsEE"
            Me.m_tsEE.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tbpBA
            '
            Me.m_tbpBA.Controls.Add(Me.m_gridBA)
            Me.m_tbpBA.Controls.Add(Me.m_tsBA)
            resources.ApplyResources(Me.m_tbpBA, "m_tbpBA")
            Me.m_tbpBA.Name = "m_tbpBA"
            Me.m_tbpBA.UseVisualStyleBackColor = True
            '
            'm_gridBA
            '
            Me.m_gridBA.AllowBlockSelect = True
            Me.m_gridBA.AutoSizeMinHeight = 10
            Me.m_gridBA.AutoSizeMinWidth = 10
            Me.m_gridBA.AutoStretchColumnsToFitWidth = False
            Me.m_gridBA.AutoStretchRowsToFitHeight = False
            Me.m_gridBA.BackColor = System.Drawing.Color.White
            Me.m_gridBA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridBA.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridBA.CustomSort = False
            Me.m_gridBA.DataName = "MC_BA"
            Me.m_gridBA.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.BA
            resources.ApplyResources(Me.m_gridBA, "m_gridBA")
            Me.m_gridBA.FixedColumnWidths = False
            Me.m_gridBA.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridBA.GridToolTipActive = True
            Me.m_gridBA.IsLayoutSuspended = False
            Me.m_gridBA.IsOutputGrid = True
            Me.m_gridBA.Name = "m_gridBA"
            Me.m_gridBA.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridBA.UIContext = Nothing
            '
            'm_tsBA
            '
            Me.m_tsBA.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            resources.ApplyResources(Me.m_tsBA, "m_tsBA")
            Me.m_tsBA.Name = "m_tsBA"
            Me.m_tsBA.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tbpBABi
            '
            Me.m_tbpBABi.Controls.Add(Me.m_gridBaBi)
            Me.m_tbpBABi.Controls.Add(Me.m_tsBaBi)
            resources.ApplyResources(Me.m_tbpBABi, "m_tbpBABi")
            Me.m_tbpBABi.Name = "m_tbpBABi"
            Me.m_tbpBABi.UseVisualStyleBackColor = True
            '
            'm_gridBaBi
            '
            Me.m_gridBaBi.AllowBlockSelect = True
            Me.m_gridBaBi.AutoSizeMinHeight = 10
            Me.m_gridBaBi.AutoSizeMinWidth = 10
            Me.m_gridBaBi.AutoStretchColumnsToFitWidth = False
            Me.m_gridBaBi.AutoStretchRowsToFitHeight = False
            Me.m_gridBaBi.BackColor = System.Drawing.Color.White
            Me.m_gridBaBi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridBaBi.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridBaBi.CustomSort = False
            Me.m_gridBaBi.DataName = "MC_BA_rate"
            Me.m_gridBaBi.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.BaBi
            resources.ApplyResources(Me.m_gridBaBi, "m_gridBaBi")
            Me.m_gridBaBi.FixedColumnWidths = False
            Me.m_gridBaBi.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridBaBi.GridToolTipActive = True
            Me.m_gridBaBi.IsLayoutSuspended = False
            Me.m_gridBaBi.IsOutputGrid = True
            Me.m_gridBaBi.Name = "m_gridBaBi"
            Me.m_gridBaBi.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridBaBi.UIContext = Nothing
            '
            'm_tsBaBi
            '
            Me.m_tsBaBi.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            resources.ApplyResources(Me.m_tsBaBi, "m_tsBaBi")
            Me.m_tsBaBi.Name = "m_tsBaBi"
            Me.m_tsBaBi.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tbpDiets
            '
            Me.m_tbpDiets.Controls.Add(Me.m_gridDiets)
            Me.m_tbpDiets.Controls.Add(Me.m_tsDiets)
            resources.ApplyResources(Me.m_tbpDiets, "m_tbpDiets")
            Me.m_tbpDiets.Name = "m_tbpDiets"
            Me.m_tbpDiets.UseVisualStyleBackColor = True
            '
            'm_gridDiets
            '
            Me.m_gridDiets.AllowBlockSelect = True
            Me.m_gridDiets.AutoSizeMinHeight = 10
            Me.m_gridDiets.AutoSizeMinWidth = 10
            Me.m_gridDiets.AutoStretchColumnsToFitWidth = False
            Me.m_gridDiets.AutoStretchRowsToFitHeight = False
            Me.m_gridDiets.BackColor = System.Drawing.Color.White
            Me.m_gridDiets.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridDiets.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridDiets.CustomSort = False
            Me.m_gridDiets.DataName = "MC_Diets"
            Me.m_gridDiets.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.Diets
            resources.ApplyResources(Me.m_gridDiets, "m_gridDiets")
            Me.m_gridDiets.FixedColumnWidths = False
            Me.m_gridDiets.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridDiets.GridToolTipActive = True
            Me.m_gridDiets.IsLayoutSuspended = False
            Me.m_gridDiets.IsOutputGrid = True
            Me.m_gridDiets.Name = "m_gridDiets"
            Me.m_gridDiets.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridDiets.UIContext = Nothing
            '
            'm_tsDiets
            '
            Me.m_tsDiets.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsDiets.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslblMethodDC, Me.m_tscmbMethodDC, Me.m_tssepDC, Me.m_tsbnLoadPedDC})
            resources.ApplyResources(Me.m_tsDiets, "m_tsDiets")
            Me.m_tsDiets.Name = "m_tsDiets"
            Me.m_tsDiets.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tslblMethodDC
            '
            Me.m_tslblMethodDC.Name = "m_tslblMethodDC"
            resources.ApplyResources(Me.m_tslblMethodDC, "m_tslblMethodDC")
            '
            'm_tscmbMethodDC
            '
            Me.m_tscmbMethodDC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_tscmbMethodDC.Name = "m_tscmbMethodDC"
            resources.ApplyResources(Me.m_tscmbMethodDC, "m_tscmbMethodDC")
            '
            'm_tssepDC
            '
            Me.m_tssepDC.Name = "m_tssepDC"
            resources.ApplyResources(Me.m_tssepDC, "m_tssepDC")
            '
            'm_tsbnLoadPedDC
            '
            resources.ApplyResources(Me.m_tsbnLoadPedDC, "m_tsbnLoadPedDC")
            Me.m_tsbnLoadPedDC.Name = "m_tsbnLoadPedDC"
            '
            'm_tbpLandings
            '
            Me.m_tbpLandings.Controls.Add(Me.m_gridLandings)
            Me.m_tbpLandings.Controls.Add(Me.m_tsLandings)
            resources.ApplyResources(Me.m_tbpLandings, "m_tbpLandings")
            Me.m_tbpLandings.Name = "m_tbpLandings"
            Me.m_tbpLandings.UseVisualStyleBackColor = True
            '
            'm_gridLandings
            '
            Me.m_gridLandings.AllowBlockSelect = True
            Me.m_gridLandings.AutoSizeMinHeight = 10
            Me.m_gridLandings.AutoSizeMinWidth = 10
            Me.m_gridLandings.AutoStretchColumnsToFitWidth = False
            Me.m_gridLandings.AutoStretchRowsToFitHeight = False
            Me.m_gridLandings.BackColor = System.Drawing.Color.White
            Me.m_gridLandings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridLandings.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridLandings.CustomSort = False
            Me.m_gridLandings.DataName = "MC_Landings"
            Me.m_gridLandings.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.Landings
            resources.ApplyResources(Me.m_gridLandings, "m_gridLandings")
            Me.m_gridLandings.FixedColumnWidths = False
            Me.m_gridLandings.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridLandings.GridToolTipActive = True
            Me.m_gridLandings.IsLayoutSuspended = False
            Me.m_gridLandings.IsOutputGrid = True
            Me.m_gridLandings.Name = "m_gridLandings"
            Me.m_gridLandings.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridLandings.UIContext = Nothing
            '
            'm_tsLandings
            '
            Me.m_tsLandings.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsLandings.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnLoadPedLandings})
            resources.ApplyResources(Me.m_tsLandings, "m_tsLandings")
            Me.m_tsLandings.Name = "m_tsLandings"
            Me.m_tsLandings.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnLoadPedLandings
            '
            resources.ApplyResources(Me.m_tsbnLoadPedLandings, "m_tsbnLoadPedLandings")
            Me.m_tsbnLoadPedLandings.Name = "m_tsbnLoadPedLandings"
            '
            'm_tbpDiscards
            '
            Me.m_tbpDiscards.Controls.Add(Me.m_gridDiscards)
            Me.m_tbpDiscards.Controls.Add(Me.m_tsDiscards)
            resources.ApplyResources(Me.m_tbpDiscards, "m_tbpDiscards")
            Me.m_tbpDiscards.Name = "m_tbpDiscards"
            Me.m_tbpDiscards.UseVisualStyleBackColor = True
            '
            'm_gridDiscards
            '
            Me.m_gridDiscards.AllowBlockSelect = True
            Me.m_gridDiscards.AutoSizeMinHeight = 10
            Me.m_gridDiscards.AutoSizeMinWidth = 10
            Me.m_gridDiscards.AutoStretchColumnsToFitWidth = False
            Me.m_gridDiscards.AutoStretchRowsToFitHeight = False
            Me.m_gridDiscards.BackColor = System.Drawing.Color.White
            Me.m_gridDiscards.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridDiscards.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridDiscards.CustomSort = False
            Me.m_gridDiscards.DataName = "MC_Discards"
            Me.m_gridDiscards.DisplayInputValue = ScientificInterfaceShared.Definitions.eMCRunDisplayInputValueTypes.Discards
            resources.ApplyResources(Me.m_gridDiscards, "m_gridDiscards")
            Me.m_gridDiscards.FixedColumnWidths = False
            Me.m_gridDiscards.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridDiscards.GridToolTipActive = True
            Me.m_gridDiscards.IsLayoutSuspended = False
            Me.m_gridDiscards.IsOutputGrid = True
            Me.m_gridDiscards.Name = "m_gridDiscards"
            Me.m_gridDiscards.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridDiscards.UIContext = Nothing
            '
            'm_tsDiscards
            '
            Me.m_tsDiscards.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsDiscards.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnLoadPedDiscards})
            resources.ApplyResources(Me.m_tsDiscards, "m_tsDiscards")
            Me.m_tsDiscards.Name = "m_tsDiscards"
            Me.m_tsDiscards.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnLoadPedDiscards
            '
            resources.ApplyResources(Me.m_tsbnLoadPedDiscards, "m_tsbnLoadPedDiscards")
            Me.m_tsbnLoadPedDiscards.Name = "m_tsbnLoadPedDiscards"
            '
            'm_tbpBPlot
            '
            Me.m_tbpBPlot.BackColor = System.Drawing.Color.Transparent
            Me.m_tbpBPlot.Controls.Add(Me.m_spPlot)
            resources.ApplyResources(Me.m_tbpBPlot, "m_tbpBPlot")
            Me.m_tbpBPlot.Name = "m_tbpBPlot"
            Me.m_tbpBPlot.UseVisualStyleBackColor = True
            '
            'm_spPlot
            '
            Me.m_spPlot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            resources.ApplyResources(Me.m_spPlot, "m_spPlot")
            Me.m_spPlot.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
            Me.m_spPlot.Name = "m_spPlot"
            '
            'm_spPlot.Panel1
            '
            Me.m_spPlot.Panel1.Controls.Add(Me.m_graph)
            Me.m_spPlot.Panel1.Controls.Add(Me.m_tsPlot)
            '
            'm_spPlot.Panel2
            '
            Me.m_spPlot.Panel2.Controls.Add(Me.m_lbGroups)
            Me.m_spPlot.Panel2.Controls.Add(Me.m_lblGroups)
            '
            'm_graph
            '
            resources.ApplyResources(Me.m_graph, "m_graph")
            Me.m_graph.Name = "m_graph"
            Me.m_graph.ScrollGrace = 0R
            Me.m_graph.ScrollMaxX = 0R
            Me.m_graph.ScrollMaxY = 0R
            Me.m_graph.ScrollMaxY2 = 0R
            Me.m_graph.ScrollMinX = 0R
            Me.m_graph.ScrollMinY = 0R
            Me.m_graph.ScrollMinY2 = 0R
            '
            'm_tsPlot
            '
            Me.m_tsPlot.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsPlot.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnUpdatePlot, Me.m_tsbnShowBestOnly, Me.m_tsbnShowGroups})
            resources.ApplyResources(Me.m_tsPlot, "m_tsPlot")
            Me.m_tsPlot.Name = "m_tsPlot"
            Me.m_tsPlot.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnUpdatePlot
            '
            Me.m_tsbnUpdatePlot.Checked = True
            Me.m_tsbnUpdatePlot.CheckOnClick = True
            Me.m_tsbnUpdatePlot.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_tsbnUpdatePlot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnUpdatePlot, "m_tsbnUpdatePlot")
            Me.m_tsbnUpdatePlot.Name = "m_tsbnUpdatePlot"
            '
            'm_tsbnShowBestOnly
            '
            Me.m_tsbnShowBestOnly.CheckOnClick = True
            Me.m_tsbnShowBestOnly.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnShowBestOnly, "m_tsbnShowBestOnly")
            Me.m_tsbnShowBestOnly.Name = "m_tsbnShowBestOnly"
            '
            'm_tsbnShowGroups
            '
            Me.m_tsbnShowGroups.CheckOnClick = True
            resources.ApplyResources(Me.m_tsbnShowGroups, "m_tsbnShowGroups")
            Me.m_tsbnShowGroups.Name = "m_tsbnShowGroups"
            '
            'm_lbGroups
            '
            Me.m_lbGroups.AllGroupsItemColor = System.Drawing.Color.Transparent
            Me.m_lbGroups.AllGroupsItemText = "(All)"
            resources.ApplyResources(Me.m_lbGroups, "m_lbGroups")
            Me.m_lbGroups.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.m_lbGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_lbGroups.FormattingEnabled = True
            Me.m_lbGroups.GroupListTracking = ScientificInterfaceShared.Controls.cGroupListBox.eGroupTrackingType.LivingGroups
            Me.m_lbGroups.IsAllGroupsItemSelected = False
            Me.m_lbGroups.Name = "m_lbGroups"
            Me.m_lbGroups.SelectedGroup = Nothing
            Me.m_lbGroups.SelectedGroupIndex = -1
            Me.m_lbGroups.SortThreshold = -9999.0!
            Me.m_lbGroups.SortType = ScientificInterfaceShared.Controls.cGroupListBox.eSortType.ValueAsc
            '
            'm_lblGroups
            '
            resources.ApplyResources(Me.m_lblGroups, "m_lblGroups")
            Me.m_lblGroups.CanCollapseParent = False
            Me.m_lblGroups.CollapsedParentHeight = 0
            Me.m_lblGroups.IsCollapsed = False
            Me.m_lblGroups.Name = "m_lblGroups"
            '
            'm_tbpBestTrial
            '
            Me.m_tbpBestTrial.Controls.Add(Me.m_gridBestFit)
            resources.ApplyResources(Me.m_tbpBestTrial, "m_tbpBestTrial")
            Me.m_tbpBestTrial.Name = "m_tbpBestTrial"
            Me.m_tbpBestTrial.UseVisualStyleBackColor = True
            '
            'm_gridBestFit
            '
            Me.m_gridBestFit.AllowBlockSelect = True
            Me.m_gridBestFit.AutoSizeMinHeight = 10
            Me.m_gridBestFit.AutoSizeMinWidth = 10
            Me.m_gridBestFit.AutoStretchColumnsToFitWidth = False
            Me.m_gridBestFit.AutoStretchRowsToFitHeight = False
            Me.m_gridBestFit.BackColor = System.Drawing.Color.White
            Me.m_gridBestFit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridBestFit.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridBestFit.CustomSort = False
            Me.m_gridBestFit.DataName = "MC_best_fit"
            resources.ApplyResources(Me.m_gridBestFit, "m_gridBestFit")
            Me.m_gridBestFit.FixedColumnWidths = True
            Me.m_gridBestFit.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridBestFit.GridToolTipActive = True
            Me.m_gridBestFit.IsLayoutSuspended = False
            Me.m_gridBestFit.IsOutputGrid = True
            Me.m_gridBestFit.Name = "m_gridBestFit"
            Me.m_gridBestFit.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridBestFit.UIContext = Nothing
            '
            'm_cbSave
            '
            resources.ApplyResources(Me.m_cbSave, "m_cbSave")
            Me.m_cbSave.Name = "m_cbSave"
            Me.m_cbSave.UseVisualStyleBackColor = True
            '
            'm_lblTrial
            '
            resources.ApplyResources(Me.m_lblTrial, "m_lblTrial")
            Me.m_lblTrial.Name = "m_lblTrial"
            '
            'm_lblERun
            '
            resources.ApplyResources(Me.m_lblERun, "m_lblERun")
            Me.m_lblERun.Name = "m_lblERun"
            '
            'm_lblSScurr
            '
            resources.ApplyResources(Me.m_lblSScurr, "m_lblSScurr")
            Me.m_lblSScurr.Name = "m_lblSScurr"
            '
            'm_lblSSbest
            '
            resources.ApplyResources(Me.m_lblSSbest, "m_lblSSbest")
            Me.m_lblSSbest.Name = "m_lblSSbest"
            '
            'm_btnApply
            '
            resources.ApplyResources(Me.m_btnApply, "m_btnApply")
            Me.m_btnApply.Name = "m_btnApply"
            Me.m_btnApply.UseVisualStyleBackColor = True
            '
            'm_nudNumTrials
            '
            resources.ApplyResources(Me.m_nudNumTrials, "m_nudNumTrials")
            Me.m_nudNumTrials.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
            Me.m_nudNumTrials.Name = "m_nudNumTrials"
            '
            'm_btnTS
            '
            resources.ApplyResources(Me.m_btnTS, "m_btnTS")
            Me.m_btnTS.Name = "m_btnTS"
            Me.m_btnTS.UseVisualStyleBackColor = True
            '
            'm_lblERunValue
            '
            resources.ApplyResources(Me.m_lblERunValue, "m_lblERunValue")
            Me.m_lblERunValue.Name = "m_lblERunValue"
            '
            'm_lblSSbestValue
            '
            resources.ApplyResources(Me.m_lblSSbestValue, "m_lblSSbestValue")
            Me.m_lblSSbestValue.Name = "m_lblSSbestValue"
            '
            'm_lblSScurrValue
            '
            resources.ApplyResources(Me.m_lblSScurrValue, "m_lblSScurrValue")
            Me.m_lblSScurrValue.Name = "m_lblSScurrValue"
            '
            'm_lblSSorgValue
            '
            resources.ApplyResources(Me.m_lblSSorgValue, "m_lblSSorgValue")
            Me.m_lblSSorgValue.Name = "m_lblSSorgValue"
            '
            'm_lblTrialValue
            '
            resources.ApplyResources(Me.m_lblTrialValue, "m_lblTrialValue")
            Me.m_lblTrialValue.Name = "m_lblTrialValue"
            '
            'm_lbSSOrg
            '
            resources.ApplyResources(Me.m_lbSSOrg, "m_lbSSOrg")
            Me.m_lbSSOrg.Name = "m_lbSSOrg"
            '
            'm_hdrInputOpt
            '
            Me.m_hdrInputOpt.CanCollapseParent = False
            Me.m_hdrInputOpt.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrInputOpt, "m_hdrInputOpt")
            Me.m_hdrInputOpt.IsCollapsed = False
            Me.m_hdrInputOpt.Name = "m_hdrInputOpt"
            '
            'm_hdrOutputParam
            '
            resources.ApplyResources(Me.m_hdrOutputParam, "m_hdrOutputParam")
            Me.m_hdrOutputParam.CanCollapseParent = False
            Me.m_hdrOutputParam.CollapsedParentHeight = 0
            Me.m_hdrOutputParam.IsCollapsed = False
            Me.m_hdrOutputParam.Name = "m_hdrOutputParam"
            '
            'm_tlpOutputs
            '
            resources.ApplyResources(Me.m_tlpOutputs, "m_tlpOutputs")
            Me.m_tlpOutputs.Controls.Add(Me.m_lblTrial, 0, 0)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblERun, 0, 1)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblTrialValue, 1, 0)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblERunValue, 1, 1)
            Me.m_tlpOutputs.Controls.Add(Me.m_lbSSOrg, 3, 0)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblSScurrValue, 4, 1)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblSScurr, 3, 1)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblSSorgValue, 4, 0)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblSSbest, 6, 0)
            Me.m_tlpOutputs.Controls.Add(Me.m_lblSSbestValue, 7, 0)
            Me.m_tlpOutputs.Name = "m_tlpOutputs"
            '
            'm_cmbSaveFormat
            '
            Me.m_cmbSaveFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbSaveFormat.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbSaveFormat, "m_cmbSaveFormat")
            Me.m_cmbSaveFormat.Name = "m_cmbSaveFormat"
            Me.m_cmbSaveFormat.Sorted = True
            '
            'frmMCRun
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnStop
            Me.Controls.Add(Me.m_cmbSaveFormat)
            Me.Controls.Add(Me.m_cbSave)
            Me.Controls.Add(Me.m_tlpOutputs)
            Me.Controls.Add(Me.m_hdrOutputParam)
            Me.Controls.Add(Me.m_hdrInputOpt)
            Me.Controls.Add(Me.m_nudNumTrials)
            Me.Controls.Add(Me.m_btnTS)
            Me.Controls.Add(Me.m_lblNumTrials)
            Me.Controls.Add(Me.m_tcMain)
            Me.Controls.Add(Me.m_btnApply)
            Me.Controls.Add(Me.m_btnStop)
            Me.Controls.Add(Me.m_btnRunTrials)
            Me.CoreExecutionState = EwEUtils.Core.eCoreExecutionState.EcosimLoaded
            Me.Name = "frmMCRun"
            Me.TabText = "Monte Carlo simulations"
            Me.m_tcMain.ResumeLayout(False)
            Me.m_tbpSettings.ResumeLayout(False)
            Me.m_tbpSettings.PerformLayout()
            Me.m_tbpB.ResumeLayout(False)
            Me.m_tbpB.PerformLayout()
            Me.m_tsB.ResumeLayout(False)
            Me.m_tsB.PerformLayout()
            Me.m_tbpPB.ResumeLayout(False)
            Me.m_tbpPB.PerformLayout()
            Me.m_tsPB.ResumeLayout(False)
            Me.m_tsPB.PerformLayout()
            Me.m_tbpQB.ResumeLayout(False)
            Me.m_tbpQB.PerformLayout()
            Me.m_tsQB.ResumeLayout(False)
            Me.m_tsQB.PerformLayout()
            Me.m_tbpEE.ResumeLayout(False)
            Me.m_tbpEE.PerformLayout()
            Me.m_tbpBA.ResumeLayout(False)
            Me.m_tbpBA.PerformLayout()
            Me.m_tbpBABi.ResumeLayout(False)
            Me.m_tbpBABi.PerformLayout()
            Me.m_tbpDiets.ResumeLayout(False)
            Me.m_tbpDiets.PerformLayout()
            Me.m_tsDiets.ResumeLayout(False)
            Me.m_tsDiets.PerformLayout()
            Me.m_tbpLandings.ResumeLayout(False)
            Me.m_tbpLandings.PerformLayout()
            Me.m_tsLandings.ResumeLayout(False)
            Me.m_tsLandings.PerformLayout()
            Me.m_tbpDiscards.ResumeLayout(False)
            Me.m_tbpDiscards.PerformLayout()
            Me.m_tsDiscards.ResumeLayout(False)
            Me.m_tsDiscards.PerformLayout()
            Me.m_tbpBPlot.ResumeLayout(False)
            Me.m_spPlot.Panel1.ResumeLayout(False)
            Me.m_spPlot.Panel1.PerformLayout()
            Me.m_spPlot.Panel2.ResumeLayout(False)
            CType(Me.m_spPlot, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_spPlot.ResumeLayout(False)
            Me.m_tsPlot.ResumeLayout(False)
            Me.m_tsPlot.PerformLayout()
            Me.m_tbpBestTrial.ResumeLayout(False)
            CType(Me.m_nudNumTrials, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tlpOutputs.ResumeLayout(False)
            Me.m_tlpOutputs.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lblNumTrials As System.Windows.Forms.Label
        Private WithEvents m_btnRunTrials As System.Windows.Forms.Button
        Private WithEvents m_btnStop As System.Windows.Forms.Button
        Private WithEvents m_btnApply As System.Windows.Forms.Button
        Private WithEvents m_btnTS As System.Windows.Forms.Button
        Private WithEvents m_nudNumTrials As System.Windows.Forms.NumericUpDown
        Private WithEvents m_hdrInputOpt As cEwEHeaderLabel
        Private WithEvents m_hdrOutputParam As cEwEHeaderLabel
        Private WithEvents m_tcMain As ucTabControlEx
        Private WithEvents m_lblERunValue As System.Windows.Forms.Label
        Private WithEvents m_lblSSbestValue As System.Windows.Forms.Label
        Private WithEvents m_lblSScurrValue As System.Windows.Forms.Label
        Private WithEvents m_lblSSorgValue As System.Windows.Forms.Label
        Private WithEvents m_lblTrialValue As System.Windows.Forms.Label
        Private WithEvents m_lblTrial As System.Windows.Forms.Label
        Private WithEvents m_lblERun As System.Windows.Forms.Label
        Private WithEvents m_lblSScurr As System.Windows.Forms.Label
        Private WithEvents m_lblSSbest As System.Windows.Forms.Label
        Private WithEvents m_lbSSOrg As System.Windows.Forms.Label
        Private WithEvents m_tlpOutputs As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_gridBestFit As ScientificInterface.Ecosim.gridMCRunOutput
        Private WithEvents m_tbpBPlot As System.Windows.Forms.TabPage
        Private WithEvents m_tbpBestTrial As System.Windows.Forms.TabPage
        Private WithEvents m_tbpBA As System.Windows.Forms.TabPage
        Private WithEvents m_tbpEE As System.Windows.Forms.TabPage
        Private WithEvents m_tbpPB As System.Windows.Forms.TabPage
        Private WithEvents m_tbpB As System.Windows.Forms.TabPage
        Private WithEvents m_spPlot As System.Windows.Forms.SplitContainer
        Private WithEvents m_lbGroups As ScientificInterfaceShared.Controls.cGroupListBox
        Private WithEvents m_lblGroups As cEwEHeaderLabel
        Private WithEvents m_tbpQB As System.Windows.Forms.TabPage
        Private WithEvents m_btDefaultTol As System.Windows.Forms.Button
        Private WithEvents m_tbpSettings As System.Windows.Forms.TabPage
        Private WithEvents m_cbSave As System.Windows.Forms.CheckBox
        Private WithEvents m_cbRetainEstimates As System.Windows.Forms.CheckBox
        Private WithEvents m_cbRetainCurPattern As System.Windows.Forms.CheckBox
        Private WithEvents m_gridB As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_tsB As cEwEToolstrip
        Private WithEvents m_tsbnLoadPedB As System.Windows.Forms.ToolStripButton
        Private WithEvents m_gridPB As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_tsPB As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnLoadPedPB As System.Windows.Forms.ToolStripButton
        Private WithEvents m_gridQB As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_tsQB As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnLoadPedQB As System.Windows.Forms.ToolStripButton
        Private WithEvents m_gridEE As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_tsEE As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_gridBA As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_tsBA As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_cbSRA As System.Windows.Forms.CheckBox
        Private WithEvents m_lblFMratio As System.Windows.Forms.Label
        Private WithEvents m_lblEEtol As System.Windows.Forms.Label
        Private WithEvents m_tbxEETol As System.Windows.Forms.TextBox
        Private WithEvents m_tbxFMratio As System.Windows.Forms.TextBox
        Private WithEvents m_tsPlot As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnShowBestOnly As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnShowGroups As System.Windows.Forms.ToolStripButton
        Private WithEvents m_graph As ZedGraph.ZedGraphControl
        Private WithEvents m_tbpLandings As System.Windows.Forms.TabPage
        Private WithEvents m_tsLandings As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnLoadPedLandings As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tbpDiscards As System.Windows.Forms.TabPage
        Private WithEvents m_tsDiscards As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnLoadPedDiscards As System.Windows.Forms.ToolStripButton
        Private WithEvents m_gridLandings As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_gridDiscards As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_tbpDiets As System.Windows.Forms.TabPage
        Private WithEvents m_tsDiets As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnLoadPedDC As System.Windows.Forms.ToolStripButton
        Private WithEvents m_gridDiets As ScientificInterface.Ecosim.gridMCRunInput
        Private WithEvents m_clbEnabledVariables As cFlickerFreeCheckedListBox
        Private WithEvents m_lblEnabledVariables As System.Windows.Forms.Label
        Private WithEvents m_cmbSaveFormat As ComboBox
        Private WithEvents m_tbpBABi As TabPage
        Private WithEvents m_gridBaBi As gridMCRunInput
        Private WithEvents m_tsBaBi As cEwEToolstrip
        Private WithEvents m_tsbnUpdatePlot As ToolStripButton
        Private WithEvents m_tslblMethodDC As ToolStripLabel
        Private WithEvents m_tscmbMethodDC As ToolStripComboBox
        Private WithEvents m_tssepDC As ToolStripSeparator
    End Class

End Namespace

