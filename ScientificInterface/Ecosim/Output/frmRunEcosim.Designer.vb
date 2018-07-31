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

Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Forms
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecosim

    Partial Class frmRunEcosim
        Inherits frmEwE

        'UserControl overrides dispose to clean up the component list.
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
            Dim sep1 As System.Windows.Forms.ToolStripSeparator
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmRunEcosim))
            Dim sep2 As System.Windows.Forms.ToolStripSeparator
            Dim sep4 As System.Windows.Forms.ToolStripSeparator
            Dim sep11 As System.Windows.Forms.ToolStripSeparator
            Dim sep5 As System.Windows.Forms.ToolStripSeparator
            Dim sep3 As System.Windows.Forms.ToolStripSeparator
            Me.m_btnRun = New System.Windows.Forms.Button()
            Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnFleet = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnGroup = New System.Windows.Forms.ToolStripButton()
            Me.m_tscbTarget = New System.Windows.Forms.ToolStripComboBox()
            Me.m_tsbnSetTo0 = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnSetToValue = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnResetFs = New System.Windows.Forms.ToolStripButton()
            Me.m_scPlots = New System.Windows.Forms.SplitContainer()
            Me.m_graph = New ZedGraph.ZedGraphControl()
            Me.m_sketchPad = New ScientificInterfaceShared.Controls.ucForcingSketchPad()
            Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbtnShowHideGroups = New System.Windows.Forms.ToolStripButton()
            Me.m_tslblSSValue = New System.Windows.Forms.ToolStripLabel()
            Me.m_tsblbSS = New System.Windows.Forms.ToolStripLabel()
            Me.m_tsddGraphOptions = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmiAutoscale = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiCustomScaleLabel = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiMax = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tstbMax = New System.Windows.Forms.ToolStripTextBox()
            Me.m_tsmiMin = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tstbMin = New System.Windows.Forms.ToolStripTextBox()
            Me.m_tsmiShowEffortAndMortalities = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiShowAnnualOutput = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiSort = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tslChangeAmount = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tstbChangeAmount = New System.Windows.Forms.ToolStripTextBox()
            Me.m_tsdrpdnbtnContent = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmiBiomassAbs = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiBiomassRel = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator6 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiCatchAbs = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiCatchRel = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator7 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiValueAbs = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiValueRel = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsbnShowMultipleRuns = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnExplore = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnSaveOutput = New System.Windows.Forms.ToolStripButton()
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lbGroups = New ScientificInterfaceShared.Controls.cGroupListBox()
            Me.m_hdrGroups = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lbRuns = New System.Windows.Forms.ListBox()
            Me.m_hdrRuns = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnStop = New System.Windows.Forms.Button()
            sep1 = New System.Windows.Forms.ToolStripSeparator()
            sep2 = New System.Windows.Forms.ToolStripSeparator()
            sep4 = New System.Windows.Forms.ToolStripSeparator()
            sep11 = New System.Windows.Forms.ToolStripSeparator()
            sep5 = New System.Windows.Forms.ToolStripSeparator()
            sep3 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsMain.SuspendLayout()
            CType(Me.m_scPlots, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scPlots.Panel1.SuspendLayout()
            Me.m_scPlots.Panel2.SuspendLayout()
            Me.m_scPlots.SuspendLayout()
            Me.m_ts.SuspendLayout()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.m_tlpControls.SuspendLayout()
            Me.SuspendLayout()
            '
            'sep1
            '
            sep1.Name = "sep1"
            resources.ApplyResources(sep1, "sep1")
            '
            'sep2
            '
            sep2.Name = "sep2"
            resources.ApplyResources(sep2, "sep2")
            '
            'sep4
            '
            sep4.Name = "sep4"
            resources.ApplyResources(sep4, "sep4")
            '
            'sep11
            '
            sep11.Name = "sep11"
            resources.ApplyResources(sep11, "sep11")
            '
            'sep5
            '
            sep5.Name = "sep5"
            resources.ApplyResources(sep5, "sep5")
            '
            'sep3
            '
            sep3.Name = "sep3"
            resources.ApplyResources(sep3, "sep3")
            '
            'm_btnRun
            '
            resources.ApplyResources(Me.m_btnRun, "m_btnRun")
            Me.m_btnRun.Name = "m_btnRun"
            Me.m_btnRun.UseVisualStyleBackColor = True
            '
            'm_tsMain
            '
            Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnFleet, Me.m_tsbnGroup, Me.m_tscbTarget, sep11, Me.m_tsbnSetTo0, Me.m_tsbnSetToValue, Me.m_tsbnResetFs})
            resources.ApplyResources(Me.m_tsMain, "m_tsMain")
            Me.m_tsMain.Name = "m_tsMain"
            Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnFleet
            '
            Me.m_tsbnFleet.Checked = True
            Me.m_tsbnFleet.CheckOnClick = True
            Me.m_tsbnFleet.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_tsbnFleet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsbnFleet, "m_tsbnFleet")
            Me.m_tsbnFleet.Name = "m_tsbnFleet"
            '
            'm_tsbnGroup
            '
            Me.m_tsbnGroup.CheckOnClick = True
            Me.m_tsbnGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsbnGroup, "m_tsbnGroup")
            Me.m_tsbnGroup.Name = "m_tsbnGroup"
            '
            'm_tscbTarget
            '
            Me.m_tscbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            resources.ApplyResources(Me.m_tscbTarget, "m_tscbTarget")
            Me.m_tscbTarget.Name = "m_tscbTarget"
            '
            'm_tsbnSetTo0
            '
            Me.m_tsbnSetTo0.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnSetTo0, "m_tsbnSetTo0")
            Me.m_tsbnSetTo0.Name = "m_tsbnSetTo0"
            '
            'm_tsbnSetToValue
            '
            Me.m_tsbnSetToValue.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnSetToValue, "m_tsbnSetToValue")
            Me.m_tsbnSetToValue.Name = "m_tsbnSetToValue"
            '
            'm_tsbnResetFs
            '
            Me.m_tsbnResetFs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnResetFs, "m_tsbnResetFs")
            Me.m_tsbnResetFs.Name = "m_tsbnResetFs"
            '
            'm_scPlots
            '
            resources.ApplyResources(Me.m_scPlots, "m_scPlots")
            Me.m_scPlots.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
            Me.m_scPlots.Name = "m_scPlots"
            '
            'm_scPlots.Panel1
            '
            Me.m_scPlots.Panel1.Controls.Add(Me.m_graph)
            '
            'm_scPlots.Panel2
            '
            Me.m_scPlots.Panel2.Controls.Add(Me.m_sketchPad)
            Me.m_scPlots.Panel2.Controls.Add(Me.m_tsMain)
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
            'm_sketchPad
            '
            Me.m_sketchPad.AllowDragXMark = False
            resources.ApplyResources(Me.m_sketchPad, "m_sketchPad")
            Me.m_sketchPad.AxisTickMarkDisplayMode = ScientificInterfaceShared.Definitions.eAxisTickmarkDisplayModeTypes.Absolute
            Me.m_sketchPad.BackColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(250, Byte), Integer))
            Me.m_sketchPad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_sketchPad.CanEditPoints = True
            Me.m_sketchPad.Cursor = System.Windows.Forms.Cursors.Hand
            Me.m_sketchPad.DisplayAxis = True
            Me.m_sketchPad.Editable = True
            Me.m_sketchPad.Handler = Nothing
            Me.m_sketchPad.IsSeasonal = False
            Me.m_sketchPad.Name = "m_sketchPad"
            Me.m_sketchPad.NumDataPoints = 0
            Me.m_sketchPad.Shape = Nothing
            Me.m_sketchPad.ShapeColor = System.Drawing.Color.AliceBlue
            Me.m_sketchPad.ShowValueTooltip = True
            Me.m_sketchPad.ShowXMark = False
            Me.m_sketchPad.ShowYMark = False
            Me.m_sketchPad.SketchDrawMode = ScientificInterfaceShared.Definitions.eSketchDrawModeTypes.Fill
            Me.m_sketchPad.UIContext = Nothing
            Me.m_sketchPad.XAxisMaxValue = -9999
            Me.m_sketchPad.XMarkValue = -9999.0!
            Me.m_sketchPad.YAxisAutoScaleMode = ScientificInterfaceShared.Definitions.eAxisAutoScaleModeTypes.[Auto]
            Me.m_sketchPad.YAxisMaxValue = 0!
            Me.m_sketchPad.YAxisMinValue = 1.0!
            Me.m_sketchPad.YMarkLabel = ""
            Me.m_sketchPad.YMarkValue = -9999.0!
            '
            'm_ts
            '
            Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbtnShowHideGroups, Me.m_tslblSSValue, Me.m_tsblbSS, Me.m_tsddGraphOptions, sep1, Me.m_tsdrpdnbtnContent, Me.m_tsbnShowMultipleRuns, sep2, Me.m_tsbnExplore, sep3, Me.m_tsbnSaveOutput})
            resources.ApplyResources(Me.m_ts, "m_ts")
            Me.m_ts.Name = "m_ts"
            Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbtnShowHideGroups
            '
            resources.ApplyResources(Me.m_tsbtnShowHideGroups, "m_tsbtnShowHideGroups")
            Me.m_tsbtnShowHideGroups.Name = "m_tsbtnShowHideGroups"
            '
            'm_tslblSSValue
            '
            Me.m_tslblSSValue.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.m_tslblSSValue.Name = "m_tslblSSValue"
            resources.ApplyResources(Me.m_tslblSSValue, "m_tslblSSValue")
            '
            'm_tsblbSS
            '
            Me.m_tsblbSS.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.m_tsblbSS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            Me.m_tsblbSS.Name = "m_tsblbSS"
            resources.ApplyResources(Me.m_tsblbSS, "m_tsblbSS")
            '
            'm_tsddGraphOptions
            '
            Me.m_tsddGraphOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            Me.m_tsddGraphOptions.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiAutoscale, Me.m_tsmiCustomScaleLabel, Me.m_tsmiMax, Me.m_tstbMax, Me.m_tsmiMin, Me.m_tstbMin, sep4, Me.m_tsmiShowEffortAndMortalities, Me.m_tsmiShowAnnualOutput, sep5, Me.m_tsmiSort, Me.m_tslChangeAmount, Me.m_tstbChangeAmount})
            resources.ApplyResources(Me.m_tsddGraphOptions, "m_tsddGraphOptions")
            Me.m_tsddGraphOptions.Name = "m_tsddGraphOptions"
            '
            'm_tsmiAutoscale
            '
            Me.m_tsmiAutoscale.Checked = True
            Me.m_tsmiAutoscale.CheckOnClick = True
            Me.m_tsmiAutoscale.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_tsmiAutoscale.Name = "m_tsmiAutoscale"
            resources.ApplyResources(Me.m_tsmiAutoscale, "m_tsmiAutoscale")
            '
            'm_tsmiCustomScaleLabel
            '
            Me.m_tsmiCustomScaleLabel.CheckOnClick = True
            Me.m_tsmiCustomScaleLabel.Name = "m_tsmiCustomScaleLabel"
            resources.ApplyResources(Me.m_tsmiCustomScaleLabel, "m_tsmiCustomScaleLabel")
            '
            'm_tsmiMax
            '
            Me.m_tsmiMax.Margin = New System.Windows.Forms.Padding(15, 0, 0, 0)
            Me.m_tsmiMax.Name = "m_tsmiMax"
            resources.ApplyResources(Me.m_tsmiMax, "m_tsmiMax")
            '
            'm_tstbMax
            '
            Me.m_tstbMax.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_tstbMax.Margin = New System.Windows.Forms.Padding(50, -21, 1, 1)
            resources.ApplyResources(Me.m_tstbMax, "m_tstbMax")
            Me.m_tstbMax.Name = "m_tstbMax"
            '
            'm_tsmiMin
            '
            Me.m_tsmiMin.Margin = New System.Windows.Forms.Padding(15, 0, 0, 0)
            Me.m_tsmiMin.Name = "m_tsmiMin"
            resources.ApplyResources(Me.m_tsmiMin, "m_tsmiMin")
            '
            'm_tstbMin
            '
            Me.m_tstbMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_tstbMin.Margin = New System.Windows.Forms.Padding(50, -21, 1, 1)
            resources.ApplyResources(Me.m_tstbMin, "m_tstbMin")
            Me.m_tstbMin.Name = "m_tstbMin"
            '
            'm_tsmiShowEffortAndMortalities
            '
            Me.m_tsmiShowEffortAndMortalities.CheckOnClick = True
            Me.m_tsmiShowEffortAndMortalities.Name = "m_tsmiShowEffortAndMortalities"
            resources.ApplyResources(Me.m_tsmiShowEffortAndMortalities, "m_tsmiShowEffortAndMortalities")
            '
            'm_tsmiShowAnnualOutput
            '
            Me.m_tsmiShowAnnualOutput.CheckOnClick = True
            Me.m_tsmiShowAnnualOutput.Name = "m_tsmiShowAnnualOutput"
            resources.ApplyResources(Me.m_tsmiShowAnnualOutput, "m_tsmiShowAnnualOutput")
            '
            'm_tsmiSort
            '
            Me.m_tsmiSort.Name = "m_tsmiSort"
            resources.ApplyResources(Me.m_tsmiSort, "m_tsmiSort")
            '
            'm_tslChangeAmount
            '
            Me.m_tslChangeAmount.Margin = New System.Windows.Forms.Padding(15, 0, 0, 0)
            Me.m_tslChangeAmount.Name = "m_tslChangeAmount"
            resources.ApplyResources(Me.m_tslChangeAmount, "m_tslChangeAmount")
            '
            'm_tstbChangeAmount
            '
            Me.m_tstbChangeAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_tstbChangeAmount.Margin = New System.Windows.Forms.Padding(110, -21, 1, 1)
            resources.ApplyResources(Me.m_tstbChangeAmount, "m_tstbChangeAmount")
            Me.m_tstbChangeAmount.Name = "m_tstbChangeAmount"
            '
            'm_tsdrpdnbtnContent
            '
            Me.m_tsdrpdnbtnContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            Me.m_tsdrpdnbtnContent.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiBiomassAbs, Me.m_tsmiBiomassRel, Me.ToolStripSeparator6, Me.m_tsmiCatchAbs, Me.m_tsmiCatchRel, Me.ToolStripSeparator7, Me.m_tsmiValueAbs, Me.m_tsmiValueRel})
            resources.ApplyResources(Me.m_tsdrpdnbtnContent, "m_tsdrpdnbtnContent")
            Me.m_tsdrpdnbtnContent.Name = "m_tsdrpdnbtnContent"
            '
            'm_tsmiBiomassAbs
            '
            Me.m_tsmiBiomassAbs.Checked = True
            Me.m_tsmiBiomassAbs.CheckOnClick = True
            Me.m_tsmiBiomassAbs.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_tsmiBiomassAbs.Name = "m_tsmiBiomassAbs"
            resources.ApplyResources(Me.m_tsmiBiomassAbs, "m_tsmiBiomassAbs")
            '
            'm_tsmiBiomassRel
            '
            Me.m_tsmiBiomassRel.Name = "m_tsmiBiomassRel"
            resources.ApplyResources(Me.m_tsmiBiomassRel, "m_tsmiBiomassRel")
            '
            'ToolStripSeparator6
            '
            Me.ToolStripSeparator6.Name = "ToolStripSeparator6"
            resources.ApplyResources(Me.ToolStripSeparator6, "ToolStripSeparator6")
            '
            'm_tsmiCatchAbs
            '
            Me.m_tsmiCatchAbs.CheckOnClick = True
            Me.m_tsmiCatchAbs.Name = "m_tsmiCatchAbs"
            resources.ApplyResources(Me.m_tsmiCatchAbs, "m_tsmiCatchAbs")
            '
            'm_tsmiCatchRel
            '
            Me.m_tsmiCatchRel.Name = "m_tsmiCatchRel"
            resources.ApplyResources(Me.m_tsmiCatchRel, "m_tsmiCatchRel")
            '
            'ToolStripSeparator7
            '
            Me.ToolStripSeparator7.Name = "ToolStripSeparator7"
            resources.ApplyResources(Me.ToolStripSeparator7, "ToolStripSeparator7")
            '
            'm_tsmiValueAbs
            '
            Me.m_tsmiValueAbs.Name = "m_tsmiValueAbs"
            resources.ApplyResources(Me.m_tsmiValueAbs, "m_tsmiValueAbs")
            '
            'm_tsmiValueRel
            '
            Me.m_tsmiValueRel.Name = "m_tsmiValueRel"
            resources.ApplyResources(Me.m_tsmiValueRel, "m_tsmiValueRel")
            '
            'm_tsbnShowMultipleRuns
            '
            Me.m_tsbnShowMultipleRuns.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnShowMultipleRuns, "m_tsbnShowMultipleRuns")
            Me.m_tsbnShowMultipleRuns.Name = "m_tsbnShowMultipleRuns"
            '
            'm_tsbnExplore
            '
            resources.ApplyResources(Me.m_tsbnExplore, "m_tsbnExplore")
            Me.m_tsbnExplore.Name = "m_tsbnExplore"
            '
            'm_tsbnSaveOutput
            '
            Me.m_tsbnSaveOutput.CheckOnClick = True
            Me.m_tsbnSaveOutput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnSaveOutput, "m_tsbnSaveOutput")
            Me.m_tsbnSaveOutput.Name = "m_tsbnSaveOutput"
            '
            'm_scMain
            '
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_scPlots)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_tlpControls)
            '
            'm_tlpControls
            '
            resources.ApplyResources(Me.m_tlpControls, "m_tlpControls")
            Me.m_tlpControls.Controls.Add(Me.m_lbGroups, 0, 3)
            Me.m_tlpControls.Controls.Add(Me.m_btnRun, 0, 4)
            Me.m_tlpControls.Controls.Add(Me.m_hdrGroups, 0, 2)
            Me.m_tlpControls.Controls.Add(Me.m_lbRuns, 0, 1)
            Me.m_tlpControls.Controls.Add(Me.m_hdrRuns, 0, 0)
            Me.m_tlpControls.Controls.Add(Me.m_btnStop, 0, 5)
            Me.m_tlpControls.Name = "m_tlpControls"
            '
            'm_lbGroups
            '
            Me.m_lbGroups.AllGroupsItemColor = System.Drawing.Color.Transparent
            Me.m_lbGroups.AllGroupsItemText = "(All)"
            resources.ApplyResources(Me.m_lbGroups, "m_lbGroups")
            Me.m_lbGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_lbGroups.FormattingEnabled = True
            Me.m_lbGroups.GroupDisplayStyle = ScientificInterfaceShared.Controls.cGroupListBox.eGroupDisplayStyleTypes.DisplayAsHidden
            Me.m_lbGroups.IsAllGroupsItemSelected = False
            Me.m_lbGroups.Name = "m_lbGroups"
            Me.m_lbGroups.SelectedGroup = Nothing
            Me.m_lbGroups.SelectedGroupIndex = -1
            Me.m_lbGroups.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
            Me.m_lbGroups.SortThreshold = -9999.0!
            '
            'm_hdrGroups
            '
            resources.ApplyResources(Me.m_hdrGroups, "m_hdrGroups")
            Me.m_hdrGroups.CanCollapseParent = False
            Me.m_hdrGroups.CollapsedParentHeight = 0
            Me.m_hdrGroups.IsCollapsed = False
            Me.m_hdrGroups.Name = "m_hdrGroups"
            '
            'm_lbRuns
            '
            resources.ApplyResources(Me.m_lbRuns, "m_lbRuns")
            Me.m_lbRuns.BackColor = System.Drawing.SystemColors.Window
            Me.m_lbRuns.FormattingEnabled = True
            Me.m_lbRuns.Name = "m_lbRuns"
            Me.m_lbRuns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
            '
            'm_hdrRuns
            '
            resources.ApplyResources(Me.m_hdrRuns, "m_hdrRuns")
            Me.m_hdrRuns.CanCollapseParent = False
            Me.m_hdrRuns.CollapsedParentHeight = 0
            Me.m_hdrRuns.IsCollapsed = False
            Me.m_hdrRuns.Name = "m_hdrRuns"
            '
            'm_btnStop
            '
            Me.m_btnStop.DialogResult = System.Windows.Forms.DialogResult.Cancel
            resources.ApplyResources(Me.m_btnStop, "m_btnStop")
            Me.m_btnStop.Name = "m_btnStop"
            Me.m_btnStop.UseVisualStyleBackColor = True
            '
            'frmRunEcosim
            '
            Me.AcceptButton = Me.m_btnRun
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnStop
            Me.Controls.Add(Me.m_scMain)
            Me.Controls.Add(Me.m_ts)
            Me.Name = "frmRunEcosim"
            Me.TabText = ""
            Me.m_tsMain.ResumeLayout(False)
            Me.m_tsMain.PerformLayout()
            Me.m_scPlots.Panel1.ResumeLayout(False)
            Me.m_scPlots.Panel2.ResumeLayout(False)
            Me.m_scPlots.Panel2.PerformLayout()
            CType(Me.m_scPlots, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scPlots.ResumeLayout(False)
            Me.m_ts.ResumeLayout(False)
            Me.m_ts.PerformLayout()
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.m_tlpControls.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_btnRun As System.Windows.Forms.Button
        Private WithEvents m_tsMain As cEwEToolstrip
        Private WithEvents m_tscbTarget As System.Windows.Forms.ToolStripComboBox
        Private WithEvents m_tsbnResetFs As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnSetTo0 As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnSetToValue As System.Windows.Forms.ToolStripButton
        Private WithEvents m_scPlots As System.Windows.Forms.SplitContainer
        Private WithEvents m_ts As cEwEToolstrip
        Private WithEvents m_tsbtnShowHideGroups As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tslblSSValue As System.Windows.Forms.ToolStripLabel
        Private WithEvents m_tsblbSS As System.Windows.Forms.ToolStripLabel
        Private WithEvents m_tsddGraphOptions As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmiAutoscale As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiCustomScaleLabel As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiMax As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tstbMax As System.Windows.Forms.ToolStripTextBox
        Private WithEvents m_tsmiMin As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tstbMin As System.Windows.Forms.ToolStripTextBox
        Private WithEvents m_tsmiShowAnnualOutput As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_graph As ZedGraph.ZedGraphControl
        Private WithEvents m_lbRuns As System.Windows.Forms.ListBox
        Private WithEvents m_lbGroups As ScientificInterfaceShared.Controls.cGroupListBox
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_hdrGroups As cEwEHeaderLabel
        Private WithEvents m_hdrRuns As cEwEHeaderLabel
        Private WithEvents m_tsdrpdnbtnContent As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmiBiomassAbs As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiCatchAbs As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiBiomassRel As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents ToolStripSeparator6 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents ToolStripSeparator7 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tsmiCatchRel As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiValueAbs As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiValueRel As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsbnExplore As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsmiSort As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tstbChangeAmount As System.Windows.Forms.ToolStripTextBox
        Private WithEvents m_tslChangeAmount As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsbnShowMultipleRuns As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnFleet As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnGroup As System.Windows.Forms.ToolStripButton
        Private WithEvents m_sketchPad As ScientificInterfaceShared.Controls.ucForcingSketchPad
        Private WithEvents m_tsbnSaveOutput As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tlpControls As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_btnStop As System.Windows.Forms.Button
        Private WithEvents m_tsmiShowEffortAndMortalities As ToolStripMenuItem
    End Class
End Namespace

