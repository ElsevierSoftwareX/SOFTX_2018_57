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
Imports ScientificInterface.Ecosim
Imports ScientificInterfaceShared.Controls.Map
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecospace

    Partial Class frmMPAOptimizations
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMPAOptimizations))
            Me.m_btnRun = New System.Windows.Forms.Button()
            Me.m_btnStop = New System.Windows.Forms.Button()
            Me.m_tsMap = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbMPA = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmClearMPA = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmSetAllMPA = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsbSeed = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmClearSeed = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmSetAllSeed = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsbEditLayers = New System.Windows.Forms.ToolStripButton()
            Me.m_cmbMPA = New System.Windows.Forms.ComboBox()
            Me.m_rbRandom = New System.Windows.Forms.RadioButton()
            Me.m_rbEcoseed = New System.Windows.Forms.RadioButton()
            Me.m_lbMPA = New System.Windows.Forms.Label()
            Me.m_lblEndYear = New System.Windows.Forms.Label()
            Me.m_nudIterations = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudStep = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudEndYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblStartYear = New System.Windows.Forms.Label()
            Me.m_nudStartYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_tcResults = New System.Windows.Forms.TabControl()
            Me.m_tpProgress = New System.Windows.Forms.TabPage()
            Me.m_graphProgress = New ZedGraph.ZedGraphControl()
            Me.m_gridProgress = New ScientificInterface.gridMPAOptimizations()
            Me.m_tpResults = New System.Windows.Forms.TabPage()
            Me.m_cmbAreaClosed = New System.Windows.Forms.ComboBox()
            Me.m_btnSave = New System.Windows.Forms.Button()
            Me.m_lblAreaClosed = New System.Windows.Forms.Label()
            Me.m_lblBestPercentile = New System.Windows.Forms.Label()
            Me.m_graphResults = New ZedGraph.ZedGraphControl()
            Me.m_btnResetMPAs = New System.Windows.Forms.Button()
            Me.m_btnConvertToMpa = New System.Windows.Forms.Button()
            Me.m_nudBestPercentile = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_gridResults = New ScientificInterface.gridMPAOptimizations()
            Me.m_hdrOutput = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblSearchType = New System.Windows.Forms.Label()
            Me.m_tlbParameters = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lblMinArea = New System.Windows.Forms.Label()
            Me.m_lblMaxArea = New System.Windows.Forms.Label()
            Me.m_nudMinArea = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudMaxArea = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblStep = New System.Windows.Forms.Label()
            Me.m_nudBaseYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblBaseYear = New System.Windows.Forms.Label()
            Me.m_lblIterations = New System.Windows.Forms.Label()
            Me.m_lblDiscRate = New System.Windows.Forms.Label()
            Me.m_nudDiscRate = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudGenDiscRate = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblGenDiscRate = New System.Windows.Forms.Label()
            Me.m_cbAutoSave = New System.Windows.Forms.CheckBox()
            Me.m_tcConfiguration = New System.Windows.Forms.TabControl()
            Me.m_tabParameters = New System.Windows.Forms.TabPage()
            Me.m_tlpObjectives = New System.Windows.Forms.TableLayoutPanel()
            Me.m_gridObjectives = New ScientificInterface.Ecosim.gridSearchObjectivesWeight()
            Me.m_gridFleet = New ScientificInterface.Ecosim.gridSearchObjectivesFleet()
            Me.m_gridGroup = New ScientificInterface.Ecosim.gridSearchObjectivesGroup()
            Me.m_tabMap = New System.Windows.Forms.TabPage()
            Me.m_tlpMap = New System.Windows.Forms.TableLayoutPanel()
            Me.m_scMap = New System.Windows.Forms.SplitContainer()
            Me.m_ucZoomBar = New ScientificInterfaceShared.Controls.Map.ucMapZoomToolbar()
            Me.m_ucZoom = New ScientificInterfaceShared.Controls.Map.ucMapZoom()
            Me.m_tlbLayers = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plLayers = New System.Windows.Forms.Panel()
            Me.m_hdrLayers = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tabSponsors = New System.Windows.Forms.TabPage()
            Me.m_tlpSponsors = New System.Windows.Forms.TableLayoutPanel()
            Me.m_pbLenfest = New System.Windows.Forms.PictureBox()
            Me.m_pbDuke = New System.Windows.Forms.PictureBox()
            Me.m_scContent = New System.Windows.Forms.SplitContainer()
            Me.m_tsMap.SuspendLayout()
            CType(Me.m_nudIterations, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudStep, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudEndYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudStartYear, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tcResults.SuspendLayout()
            Me.m_tpProgress.SuspendLayout()
            Me.m_tpResults.SuspendLayout()
            CType(Me.m_nudBestPercentile, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tlbParameters.SuspendLayout()
            CType(Me.m_nudMinArea, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudMaxArea, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudBaseYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudDiscRate, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudGenDiscRate, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tcConfiguration.SuspendLayout()
            Me.m_tabParameters.SuspendLayout()
            Me.m_tlpObjectives.SuspendLayout()
            Me.m_tabMap.SuspendLayout()
            Me.m_tlpMap.SuspendLayout()
            CType(Me.m_scMap, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMap.Panel1.SuspendLayout()
            Me.m_scMap.Panel2.SuspendLayout()
            Me.m_scMap.SuspendLayout()
            Me.m_tlbLayers.SuspendLayout()
            Me.m_tabSponsors.SuspendLayout()
            Me.m_tlpSponsors.SuspendLayout()
            CType(Me.m_pbLenfest, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbDuke, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_scContent, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scContent.Panel1.SuspendLayout()
            Me.m_scContent.Panel2.SuspendLayout()
            Me.m_scContent.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_btnRun
            '
            resources.ApplyResources(Me.m_btnRun, "m_btnRun")
            Me.m_btnRun.Name = "m_btnRun"
            Me.m_btnRun.UseVisualStyleBackColor = True
            '
            'm_btnStop
            '
            resources.ApplyResources(Me.m_btnStop, "m_btnStop")
            Me.m_btnStop.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnStop.Name = "m_btnStop"
            Me.m_btnStop.UseVisualStyleBackColor = True
            '
            'm_tsMap
            '
            Me.m_tsMap.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMap.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbMPA, Me.m_tsbSeed, Me.m_tsbEditLayers})
            resources.ApplyResources(Me.m_tsMap, "m_tsMap")
            Me.m_tsMap.Name = "m_tsMap"
            Me.m_tsMap.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbMPA
            '
            Me.m_tsbMPA.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmClearMPA, Me.m_tsmSetAllMPA})
            resources.ApplyResources(Me.m_tsbMPA, "m_tsbMPA")
            Me.m_tsbMPA.Name = "m_tsbMPA"
            '
            'm_tsmClearMPA
            '
            Me.m_tsmClearMPA.Name = "m_tsmClearMPA"
            resources.ApplyResources(Me.m_tsmClearMPA, "m_tsmClearMPA")
            '
            'm_tsmSetAllMPA
            '
            Me.m_tsmSetAllMPA.Name = "m_tsmSetAllMPA"
            resources.ApplyResources(Me.m_tsmSetAllMPA, "m_tsmSetAllMPA")
            '
            'm_tsbSeed
            '
            Me.m_tsbSeed.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmClearSeed, Me.m_tsmSetAllSeed})
            resources.ApplyResources(Me.m_tsbSeed, "m_tsbSeed")
            Me.m_tsbSeed.Name = "m_tsbSeed"
            '
            'm_tsmClearSeed
            '
            Me.m_tsmClearSeed.Name = "m_tsmClearSeed"
            resources.ApplyResources(Me.m_tsmClearSeed, "m_tsmClearSeed")
            '
            'm_tsmSetAllSeed
            '
            Me.m_tsmSetAllSeed.Name = "m_tsmSetAllSeed"
            resources.ApplyResources(Me.m_tsmSetAllSeed, "m_tsmSetAllSeed")
            '
            'm_tsbEditLayers
            '
            resources.ApplyResources(Me.m_tsbEditLayers, "m_tsbEditLayers")
            Me.m_tsbEditLayers.Name = "m_tsbEditLayers"
            '
            'm_cmbMPA
            '
            resources.ApplyResources(Me.m_cmbMPA, "m_cmbMPA")
            Me.m_cmbMPA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbMPA.FormattingEnabled = True
            Me.m_cmbMPA.Name = "m_cmbMPA"
            '
            'm_rbRandom
            '
            resources.ApplyResources(Me.m_rbRandom, "m_rbRandom")
            Me.m_rbRandom.Name = "m_rbRandom"
            Me.m_rbRandom.UseVisualStyleBackColor = True
            '
            'm_rbEcoseed
            '
            resources.ApplyResources(Me.m_rbEcoseed, "m_rbEcoseed")
            Me.m_rbEcoseed.Name = "m_rbEcoseed"
            Me.m_rbEcoseed.UseVisualStyleBackColor = True
            '
            'm_lbMPA
            '
            resources.ApplyResources(Me.m_lbMPA, "m_lbMPA")
            Me.m_lbMPA.Name = "m_lbMPA"
            '
            'm_lblEndYear
            '
            resources.ApplyResources(Me.m_lblEndYear, "m_lblEndYear")
            Me.m_lblEndYear.Name = "m_lblEndYear"
            '
            'm_nudIterations
            '
            resources.ApplyResources(Me.m_nudIterations, "m_nudIterations")
            Me.m_nudIterations.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudIterations.Name = "m_nudIterations"
            '
            'm_nudStep
            '
            resources.ApplyResources(Me.m_nudStep, "m_nudStep")
            Me.m_nudStep.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudStep.Name = "m_nudStep"
            Me.m_nudStep.Value = New Decimal(New Integer() {10, 0, 0, 0})
            '
            'm_nudEndYear
            '
            resources.ApplyResources(Me.m_nudEndYear, "m_nudEndYear")
            Me.m_nudEndYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudEndYear.Name = "m_nudEndYear"
            '
            'm_lblStartYear
            '
            resources.ApplyResources(Me.m_lblStartYear, "m_lblStartYear")
            Me.m_lblStartYear.Name = "m_lblStartYear"
            '
            'm_nudStartYear
            '
            resources.ApplyResources(Me.m_nudStartYear, "m_nudStartYear")
            Me.m_nudStartYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudStartYear.Name = "m_nudStartYear"
            '
            'm_tcResults
            '
            resources.ApplyResources(Me.m_tcResults, "m_tcResults")
            Me.m_tcResults.Controls.Add(Me.m_tpProgress)
            Me.m_tcResults.Controls.Add(Me.m_tpResults)
            Me.m_tcResults.Name = "m_tcResults"
            Me.m_tcResults.SelectedIndex = 0
            '
            'm_tpProgress
            '
            Me.m_tpProgress.Controls.Add(Me.m_graphProgress)
            Me.m_tpProgress.Controls.Add(Me.m_gridProgress)
            resources.ApplyResources(Me.m_tpProgress, "m_tpProgress")
            Me.m_tpProgress.Name = "m_tpProgress"
            Me.m_tpProgress.UseVisualStyleBackColor = True
            '
            'm_graphProgress
            '
            resources.ApplyResources(Me.m_graphProgress, "m_graphProgress")
            Me.m_graphProgress.IsAutoScrollRange = True
            Me.m_graphProgress.Name = "m_graphProgress"
            Me.m_graphProgress.ScrollGrace = 0R
            Me.m_graphProgress.ScrollMaxX = 0R
            Me.m_graphProgress.ScrollMaxY = 0R
            Me.m_graphProgress.ScrollMaxY2 = 0R
            Me.m_graphProgress.ScrollMinX = 0R
            Me.m_graphProgress.ScrollMinY = 0R
            Me.m_graphProgress.ScrollMinY2 = 0R
            '
            'm_gridProgress
            '
            Me.m_gridProgress.AllowBlockSelect = True
            resources.ApplyResources(Me.m_gridProgress, "m_gridProgress")
            Me.m_gridProgress.AutoSizeMinHeight = 10
            Me.m_gridProgress.AutoSizeMinWidth = 10
            Me.m_gridProgress.AutoStretchColumnsToFitWidth = True
            Me.m_gridProgress.AutoStretchRowsToFitHeight = False
            Me.m_gridProgress.BackColor = System.Drawing.Color.White
            Me.m_gridProgress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridProgress.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridProgress.CustomSort = False
            Me.m_gridProgress.DataName = "grid content"
            Me.m_gridProgress.FixedColumnWidths = False
            Me.m_gridProgress.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridProgress.GridToolTipActive = True
            Me.m_gridProgress.IsLayoutSuspended = False
            Me.m_gridProgress.IsOutputGrid = True
            Me.m_gridProgress.Name = "m_gridProgress"
            Me.m_gridProgress.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridProgress.UIContext = Nothing
            '
            'm_tpResults
            '
            Me.m_tpResults.Controls.Add(Me.m_cmbAreaClosed)
            Me.m_tpResults.Controls.Add(Me.m_btnSave)
            Me.m_tpResults.Controls.Add(Me.m_lblAreaClosed)
            Me.m_tpResults.Controls.Add(Me.m_lblBestPercentile)
            Me.m_tpResults.Controls.Add(Me.m_graphResults)
            Me.m_tpResults.Controls.Add(Me.m_btnResetMPAs)
            Me.m_tpResults.Controls.Add(Me.m_btnConvertToMpa)
            Me.m_tpResults.Controls.Add(Me.m_nudBestPercentile)
            Me.m_tpResults.Controls.Add(Me.m_gridResults)
            resources.ApplyResources(Me.m_tpResults, "m_tpResults")
            Me.m_tpResults.Name = "m_tpResults"
            Me.m_tpResults.UseVisualStyleBackColor = True
            '
            'm_cmbAreaClosed
            '
            Me.m_cmbAreaClosed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbAreaClosed.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbAreaClosed, "m_cmbAreaClosed")
            Me.m_cmbAreaClosed.Name = "m_cmbAreaClosed"
            '
            'm_btnSave
            '
            resources.ApplyResources(Me.m_btnSave, "m_btnSave")
            Me.m_btnSave.Name = "m_btnSave"
            Me.m_btnSave.UseVisualStyleBackColor = True
            '
            'm_lblAreaClosed
            '
            resources.ApplyResources(Me.m_lblAreaClosed, "m_lblAreaClosed")
            Me.m_lblAreaClosed.Name = "m_lblAreaClosed"
            '
            'm_lblBestPercentile
            '
            resources.ApplyResources(Me.m_lblBestPercentile, "m_lblBestPercentile")
            Me.m_lblBestPercentile.Name = "m_lblBestPercentile"
            '
            'm_graphResults
            '
            resources.ApplyResources(Me.m_graphResults, "m_graphResults")
            Me.m_graphResults.IsAutoScrollRange = True
            Me.m_graphResults.Name = "m_graphResults"
            Me.m_graphResults.ScrollGrace = 0R
            Me.m_graphResults.ScrollMaxX = 0R
            Me.m_graphResults.ScrollMaxY = 0R
            Me.m_graphResults.ScrollMaxY2 = 0R
            Me.m_graphResults.ScrollMinX = 0R
            Me.m_graphResults.ScrollMinY = 0R
            Me.m_graphResults.ScrollMinY2 = 0R
            '
            'm_btnResetMPAs
            '
            resources.ApplyResources(Me.m_btnResetMPAs, "m_btnResetMPAs")
            Me.m_btnResetMPAs.Name = "m_btnResetMPAs"
            Me.m_btnResetMPAs.UseVisualStyleBackColor = True
            '
            'm_btnConvertToMpa
            '
            resources.ApplyResources(Me.m_btnConvertToMpa, "m_btnConvertToMpa")
            Me.m_btnConvertToMpa.Name = "m_btnConvertToMpa"
            Me.m_btnConvertToMpa.UseVisualStyleBackColor = True
            '
            'm_nudBestPercentile
            '
            Me.m_nudBestPercentile.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudBestPercentile, "m_nudBestPercentile")
            Me.m_nudBestPercentile.Name = "m_nudBestPercentile"
            Me.m_nudBestPercentile.Value = New Decimal(New Integer() {10, 0, 0, 0})
            '
            'm_gridResults
            '
            Me.m_gridResults.AllowBlockSelect = True
            resources.ApplyResources(Me.m_gridResults, "m_gridResults")
            Me.m_gridResults.AutoSizeMinHeight = 10
            Me.m_gridResults.AutoSizeMinWidth = 10
            Me.m_gridResults.AutoStretchColumnsToFitWidth = True
            Me.m_gridResults.AutoStretchRowsToFitHeight = False
            Me.m_gridResults.BackColor = System.Drawing.Color.White
            Me.m_gridResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridResults.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridResults.CustomSort = False
            Me.m_gridResults.DataName = "grid content"
            Me.m_gridResults.FixedColumnWidths = False
            Me.m_gridResults.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridResults.GridToolTipActive = True
            Me.m_gridResults.IsLayoutSuspended = False
            Me.m_gridResults.IsOutputGrid = True
            Me.m_gridResults.Name = "m_gridResults"
            Me.m_gridResults.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridResults.UIContext = Nothing
            '
            'm_hdrOutput
            '
            Me.m_hdrOutput.CanCollapseParent = False
            Me.m_hdrOutput.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrOutput, "m_hdrOutput")
            Me.m_hdrOutput.IsCollapsed = False
            Me.m_hdrOutput.Name = "m_hdrOutput"
            '
            'm_lblSearchType
            '
            resources.ApplyResources(Me.m_lblSearchType, "m_lblSearchType")
            Me.m_lblSearchType.Name = "m_lblSearchType"
            '
            'm_tlbParameters
            '
            resources.ApplyResources(Me.m_tlbParameters, "m_tlbParameters")
            Me.m_tlbParameters.Controls.Add(Me.m_lblStartYear, 0, 0)
            Me.m_tlbParameters.Controls.Add(Me.m_lblEndYear, 0, 1)
            Me.m_tlbParameters.Controls.Add(Me.m_nudStartYear, 1, 0)
            Me.m_tlbParameters.Controls.Add(Me.m_nudEndYear, 1, 1)
            Me.m_tlbParameters.Controls.Add(Me.m_lblMinArea, 3, 0)
            Me.m_tlbParameters.Controls.Add(Me.m_lblMaxArea, 3, 1)
            Me.m_tlbParameters.Controls.Add(Me.m_nudMinArea, 4, 0)
            Me.m_tlbParameters.Controls.Add(Me.m_nudMaxArea, 4, 1)
            Me.m_tlbParameters.Controls.Add(Me.m_lblStep, 3, 2)
            Me.m_tlbParameters.Controls.Add(Me.m_nudStep, 4, 2)
            Me.m_tlbParameters.Controls.Add(Me.m_nudBaseYear, 1, 2)
            Me.m_tlbParameters.Controls.Add(Me.m_lblBaseYear, 0, 2)
            Me.m_tlbParameters.Controls.Add(Me.m_lbMPA, 0, 3)
            Me.m_tlbParameters.Controls.Add(Me.m_cmbMPA, 1, 3)
            Me.m_tlbParameters.Controls.Add(Me.m_lblIterations, 3, 3)
            Me.m_tlbParameters.Controls.Add(Me.m_nudIterations, 4, 3)
            Me.m_tlbParameters.Controls.Add(Me.m_lblDiscRate, 6, 0)
            Me.m_tlbParameters.Controls.Add(Me.m_nudDiscRate, 7, 0)
            Me.m_tlbParameters.Controls.Add(Me.m_nudGenDiscRate, 7, 1)
            Me.m_tlbParameters.Controls.Add(Me.m_lblGenDiscRate, 6, 1)
            Me.m_tlbParameters.Controls.Add(Me.m_cbAutoSave, 6, 2)
            Me.m_tlbParameters.Name = "m_tlbParameters"
            '
            'm_lblMinArea
            '
            resources.ApplyResources(Me.m_lblMinArea, "m_lblMinArea")
            Me.m_lblMinArea.Name = "m_lblMinArea"
            '
            'm_lblMaxArea
            '
            resources.ApplyResources(Me.m_lblMaxArea, "m_lblMaxArea")
            Me.m_lblMaxArea.Name = "m_lblMaxArea"
            '
            'm_nudMinArea
            '
            resources.ApplyResources(Me.m_nudMinArea, "m_nudMinArea")
            Me.m_nudMinArea.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudMinArea.Name = "m_nudMinArea"
            '
            'm_nudMaxArea
            '
            resources.ApplyResources(Me.m_nudMaxArea, "m_nudMaxArea")
            Me.m_nudMaxArea.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudMaxArea.Name = "m_nudMaxArea"
            '
            'm_lblStep
            '
            resources.ApplyResources(Me.m_lblStep, "m_lblStep")
            Me.m_lblStep.Name = "m_lblStep"
            '
            'm_nudBaseYear
            '
            resources.ApplyResources(Me.m_nudBaseYear, "m_nudBaseYear")
            Me.m_nudBaseYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudBaseYear.Name = "m_nudBaseYear"
            '
            'm_lblBaseYear
            '
            resources.ApplyResources(Me.m_lblBaseYear, "m_lblBaseYear")
            Me.m_lblBaseYear.Name = "m_lblBaseYear"
            '
            'm_lblIterations
            '
            resources.ApplyResources(Me.m_lblIterations, "m_lblIterations")
            Me.m_lblIterations.Name = "m_lblIterations"
            '
            'm_lblDiscRate
            '
            resources.ApplyResources(Me.m_lblDiscRate, "m_lblDiscRate")
            Me.m_lblDiscRate.Name = "m_lblDiscRate"
            '
            'm_nudDiscRate
            '
            resources.ApplyResources(Me.m_nudDiscRate, "m_nudDiscRate")
            Me.m_nudDiscRate.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudDiscRate.Name = "m_nudDiscRate"
            '
            'm_nudGenDiscRate
            '
            resources.ApplyResources(Me.m_nudGenDiscRate, "m_nudGenDiscRate")
            Me.m_nudGenDiscRate.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudGenDiscRate.Name = "m_nudGenDiscRate"
            '
            'm_lblGenDiscRate
            '
            resources.ApplyResources(Me.m_lblGenDiscRate, "m_lblGenDiscRate")
            Me.m_lblGenDiscRate.Name = "m_lblGenDiscRate"
            '
            'm_cbAutoSave
            '
            resources.ApplyResources(Me.m_cbAutoSave, "m_cbAutoSave")
            Me.m_cbAutoSave.Name = "m_cbAutoSave"
            Me.m_cbAutoSave.UseVisualStyleBackColor = True
            '
            'm_tcConfiguration
            '
            Me.m_tcConfiguration.Controls.Add(Me.m_tabParameters)
            Me.m_tcConfiguration.Controls.Add(Me.m_tabMap)
            Me.m_tcConfiguration.Controls.Add(Me.m_tabSponsors)
            resources.ApplyResources(Me.m_tcConfiguration, "m_tcConfiguration")
            Me.m_tcConfiguration.Multiline = True
            Me.m_tcConfiguration.Name = "m_tcConfiguration"
            Me.m_tcConfiguration.SelectedIndex = 0
            '
            'm_tabParameters
            '
            Me.m_tabParameters.Controls.Add(Me.m_tlbParameters)
            Me.m_tabParameters.Controls.Add(Me.m_tlpObjectives)
            resources.ApplyResources(Me.m_tabParameters, "m_tabParameters")
            Me.m_tabParameters.Name = "m_tabParameters"
            Me.m_tabParameters.UseVisualStyleBackColor = True
            '
            'm_tlpObjectives
            '
            resources.ApplyResources(Me.m_tlpObjectives, "m_tlpObjectives")
            Me.m_tlpObjectives.Controls.Add(Me.m_gridObjectives, 0, 0)
            Me.m_tlpObjectives.Controls.Add(Me.m_gridFleet, 2, 0)
            Me.m_tlpObjectives.Controls.Add(Me.m_gridGroup, 4, 0)
            Me.m_tlpObjectives.Name = "m_tlpObjectives"
            '
            'm_gridObjectives
            '
            Me.m_gridObjectives.AllowBlockSelect = True
            Me.m_gridObjectives.AutoSizeMinHeight = 10
            Me.m_gridObjectives.AutoSizeMinWidth = 10
            Me.m_gridObjectives.AutoStretchColumnsToFitWidth = True
            Me.m_gridObjectives.AutoStretchRowsToFitHeight = False
            Me.m_gridObjectives.BackColor = System.Drawing.Color.White
            Me.m_gridObjectives.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridObjectives.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridObjectives.CustomSort = False
            Me.m_gridObjectives.DataName = "grid content"
            resources.ApplyResources(Me.m_gridObjectives, "m_gridObjectives")
            Me.m_gridObjectives.FixedColumnWidths = False
            Me.m_gridObjectives.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridObjectives.GridToolTipActive = True
            Me.m_gridObjectives.IsLayoutSuspended = False
            Me.m_gridObjectives.IsOutputGrid = True
            Me.m_gridObjectives.Manager = Nothing
            Me.m_gridObjectives.Name = "m_gridObjectives"
            Me.m_gridObjectives.ShowMaxPortUtil = False
            Me.m_gridObjectives.ShowMPAOptParams = False
            Me.m_gridObjectives.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridObjectives.UIContext = Nothing
            '
            'm_gridFleet
            '
            Me.m_gridFleet.AllowBlockSelect = True
            Me.m_gridFleet.AutoSizeMinHeight = 10
            Me.m_gridFleet.AutoSizeMinWidth = 10
            Me.m_gridFleet.AutoStretchColumnsToFitWidth = True
            Me.m_gridFleet.AutoStretchRowsToFitHeight = False
            Me.m_gridFleet.BackColor = System.Drawing.Color.White
            Me.m_gridFleet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridFleet.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridFleet.CustomSort = False
            Me.m_gridFleet.DataName = "grid content"
            resources.ApplyResources(Me.m_gridFleet, "m_gridFleet")
            Me.m_gridFleet.FixedColumnWidths = False
            Me.m_gridFleet.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridFleet.GridToolTipActive = True
            Me.m_gridFleet.IsLayoutSuspended = False
            Me.m_gridFleet.IsMaximizeByFleetValue = False
            Me.m_gridFleet.IsOutputGrid = True
            Me.m_gridFleet.Manager = Nothing
            Me.m_gridFleet.Name = "m_gridFleet"
            Me.m_gridFleet.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridFleet.UIContext = Nothing
            '
            'm_gridGroup
            '
            Me.m_gridGroup.AllowBlockSelect = True
            Me.m_gridGroup.AutoSizeMinHeight = 10
            Me.m_gridGroup.AutoSizeMinWidth = 10
            Me.m_gridGroup.AutoStretchColumnsToFitWidth = True
            Me.m_gridGroup.AutoStretchRowsToFitHeight = False
            Me.m_gridGroup.BackColor = System.Drawing.Color.White
            Me.m_gridGroup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridGroup.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridGroup.CustomSort = False
            Me.m_gridGroup.DataName = "grid content"
            resources.ApplyResources(Me.m_gridGroup, "m_gridGroup")
            Me.m_gridGroup.FixedColumnWidths = False
            Me.m_gridGroup.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridGroup.GridToolTipActive = True
            Me.m_gridGroup.IsLayoutSuspended = False
            Me.m_gridGroup.IsOutputGrid = True
            Me.m_gridGroup.Manager = Nothing
            Me.m_gridGroup.Name = "m_gridGroup"
            Me.m_gridGroup.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridGroup.UIContext = Nothing
            '
            'm_tabMap
            '
            Me.m_tabMap.Controls.Add(Me.m_tlpMap)
            resources.ApplyResources(Me.m_tabMap, "m_tabMap")
            Me.m_tabMap.Name = "m_tabMap"
            Me.m_tabMap.UseVisualStyleBackColor = True
            '
            'm_tlpMap
            '
            resources.ApplyResources(Me.m_tlpMap, "m_tlpMap")
            Me.m_tlpMap.Controls.Add(Me.m_tsMap, -1, 0)
            Me.m_tlpMap.Controls.Add(Me.m_scMap, 0, 1)
            Me.m_tlpMap.Name = "m_tlpMap"
            '
            'm_scMap
            '
            Me.m_scMap.BackColor = System.Drawing.SystemColors.Control
            resources.ApplyResources(Me.m_scMap, "m_scMap")
            Me.m_scMap.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
            Me.m_scMap.Name = "m_scMap"
            '
            'm_scMap.Panel1
            '
            Me.m_scMap.Panel1.Controls.Add(Me.m_ucZoomBar)
            Me.m_scMap.Panel1.Controls.Add(Me.m_ucZoom)
            '
            'm_scMap.Panel2
            '
            Me.m_scMap.Panel2.Controls.Add(Me.m_tlbLayers)
            '
            'm_ucZoomBar
            '
            resources.ApplyResources(Me.m_ucZoomBar, "m_ucZoomBar")
            Me.m_ucZoomBar.Name = "m_ucZoomBar"
            Me.m_ucZoomBar.UIContext = Nothing
            '
            'm_ucZoom
            '
            resources.ApplyResources(Me.m_ucZoom, "m_ucZoom")
            Me.m_ucZoom.Name = "m_ucZoom"
            Me.m_ucZoom.UIContext = Nothing
            '
            'm_tlbLayers
            '
            resources.ApplyResources(Me.m_tlbLayers, "m_tlbLayers")
            Me.m_tlbLayers.Controls.Add(Me.m_plLayers, 0, 1)
            Me.m_tlbLayers.Controls.Add(Me.m_hdrLayers, 0, 0)
            Me.m_tlbLayers.Name = "m_tlbLayers"
            '
            'm_plLayers
            '
            Me.m_plLayers.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_plLayers, "m_plLayers")
            Me.m_plLayers.Name = "m_plLayers"
            '
            'm_hdrLayers
            '
            Me.m_hdrLayers.CanCollapseParent = False
            Me.m_hdrLayers.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrLayers, "m_hdrLayers")
            Me.m_hdrLayers.IsCollapsed = False
            Me.m_hdrLayers.Name = "m_hdrLayers"
            '
            'm_tabSponsors
            '
            Me.m_tabSponsors.Controls.Add(Me.m_tlpSponsors)
            resources.ApplyResources(Me.m_tabSponsors, "m_tabSponsors")
            Me.m_tabSponsors.Name = "m_tabSponsors"
            Me.m_tabSponsors.UseVisualStyleBackColor = True
            '
            'm_tlpSponsors
            '
            resources.ApplyResources(Me.m_tlpSponsors, "m_tlpSponsors")
            Me.m_tlpSponsors.Controls.Add(Me.m_pbLenfest, 1, 1)
            Me.m_tlpSponsors.Controls.Add(Me.m_pbDuke, 3, 1)
            Me.m_tlpSponsors.Name = "m_tlpSponsors"
            '
            'm_pbLenfest
            '
            Me.m_pbLenfest.BackColor = System.Drawing.Color.White
            Me.m_pbLenfest.BackgroundImage = Global.ScientificInterface.My.Resources.Resources.logo_LENFEST
            resources.ApplyResources(Me.m_pbLenfest, "m_pbLenfest")
            Me.m_pbLenfest.Cursor = System.Windows.Forms.Cursors.Hand
            Me.m_pbLenfest.Name = "m_pbLenfest"
            Me.m_pbLenfest.TabStop = False
            '
            'm_pbDuke
            '
            Me.m_pbDuke.BackColor = System.Drawing.Color.White
            Me.m_pbDuke.BackgroundImage = Global.ScientificInterface.My.Resources.Resources.logo_mgel
            resources.ApplyResources(Me.m_pbDuke, "m_pbDuke")
            Me.m_pbDuke.Cursor = System.Windows.Forms.Cursors.Hand
            Me.m_pbDuke.Name = "m_pbDuke"
            Me.m_pbDuke.TabStop = False
            '
            'm_scContent
            '
            resources.ApplyResources(Me.m_scContent, "m_scContent")
            Me.m_scContent.Name = "m_scContent"
            '
            'm_scContent.Panel1
            '
            Me.m_scContent.Panel1.Controls.Add(Me.m_tcConfiguration)
            '
            'm_scContent.Panel2
            '
            Me.m_scContent.Panel2.Controls.Add(Me.m_hdrOutput)
            Me.m_scContent.Panel2.Controls.Add(Me.m_tcResults)
            '
            'frmMPAOptimizations
            '
            Me.AcceptButton = Me.m_btnRun
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnStop
            Me.Controls.Add(Me.m_btnRun)
            Me.Controls.Add(Me.m_btnStop)
            Me.Controls.Add(Me.m_scContent)
            Me.Controls.Add(Me.m_rbRandom)
            Me.Controls.Add(Me.m_lblSearchType)
            Me.Controls.Add(Me.m_rbEcoseed)
            Me.CoreExecutionState = EwEUtils.Core.eCoreExecutionState.EcospaceLoaded
            Me.DoubleBuffered = True
            Me.Name = "frmMPAOptimizations"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.m_tsMap.ResumeLayout(False)
            Me.m_tsMap.PerformLayout()
            CType(Me.m_nudIterations, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudStep, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudEndYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudStartYear, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tcResults.ResumeLayout(False)
            Me.m_tpProgress.ResumeLayout(False)
            Me.m_tpResults.ResumeLayout(False)
            Me.m_tpResults.PerformLayout()
            CType(Me.m_nudBestPercentile, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tlbParameters.ResumeLayout(False)
            Me.m_tlbParameters.PerformLayout()
            CType(Me.m_nudMinArea, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudMaxArea, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudBaseYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudDiscRate, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudGenDiscRate, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tcConfiguration.ResumeLayout(False)
            Me.m_tabParameters.ResumeLayout(False)
            Me.m_tlpObjectives.ResumeLayout(False)
            Me.m_tabMap.ResumeLayout(False)
            Me.m_tlpMap.ResumeLayout(False)
            Me.m_tlpMap.PerformLayout()
            Me.m_scMap.Panel1.ResumeLayout(False)
            Me.m_scMap.Panel1.PerformLayout()
            Me.m_scMap.Panel2.ResumeLayout(False)
            CType(Me.m_scMap, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMap.ResumeLayout(False)
            Me.m_tlbLayers.ResumeLayout(False)
            Me.m_tabSponsors.ResumeLayout(False)
            Me.m_tlpSponsors.ResumeLayout(False)
            CType(Me.m_pbLenfest, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbDuke, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scContent.Panel1.ResumeLayout(False)
            Me.m_scContent.Panel2.ResumeLayout(False)
            CType(Me.m_scContent, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scContent.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tlbLayers As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_hdrLayers As cEwEHeaderLabel
        Private WithEvents m_tsMap As cEwEToolstrip
        Private WithEvents m_tsbMPA As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmClearMPA As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmSetAllMPA As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsbSeed As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmClearSeed As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmSetAllSeed As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tcResults As System.Windows.Forms.TabControl
        Private WithEvents m_tpProgress As System.Windows.Forms.TabPage
        Private WithEvents m_rbRandom As System.Windows.Forms.RadioButton
        Private WithEvents m_rbEcoseed As System.Windows.Forms.RadioButton
        Private WithEvents m_cmbMPA As System.Windows.Forms.ComboBox
        Private WithEvents m_tsbEditLayers As System.Windows.Forms.ToolStripButton
        Private WithEvents m_gridProgress As ScientificInterface.gridMPAOptimizations
        Private WithEvents m_tpResults As System.Windows.Forms.TabPage
        Private WithEvents m_btnConvertToMpa As System.Windows.Forms.Button
        Private WithEvents m_btnResetMPAs As System.Windows.Forms.Button
        Private WithEvents m_lblBestPercentile As System.Windows.Forms.Label
        Private WithEvents m_graphResults As ZedGraph.ZedGraphControl
        Private WithEvents m_gridResults As ScientificInterface.gridMPAOptimizations
        Private WithEvents m_lblSearchType As System.Windows.Forms.Label
        Private WithEvents m_lblEndYear As System.Windows.Forms.Label
        Private WithEvents m_lblStartYear As System.Windows.Forms.Label
        Private WithEvents m_tlbParameters As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_lbMPA As System.Windows.Forms.Label
        Private WithEvents m_scMap As System.Windows.Forms.SplitContainer
        Private WithEvents m_ucZoom As ucMapZoom
        Private WithEvents m_plLayers As System.Windows.Forms.Panel
        Private WithEvents m_btnRun As System.Windows.Forms.Button
        Private WithEvents m_btnStop As System.Windows.Forms.Button
        Private WithEvents m_graphProgress As ZedGraph.ZedGraphControl
        Private WithEvents m_hdrOutput As cEwEHeaderLabel
        Private WithEvents m_tabParameters As System.Windows.Forms.TabPage
        Private WithEvents m_tabMap As System.Windows.Forms.TabPage
        Private WithEvents m_tcConfiguration As System.Windows.Forms.TabControl
        Private WithEvents m_gridGroup As gridSearchObjectivesGroup
        Private WithEvents m_lblMinArea As System.Windows.Forms.Label
        Private WithEvents m_lblMaxArea As System.Windows.Forms.Label
        Private WithEvents m_lblStep As System.Windows.Forms.Label
        Private WithEvents m_lblIterations As System.Windows.Forms.Label
        Private WithEvents m_tlpObjectives As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_gridFleet As gridSearchObjectivesFleet
        Private WithEvents m_gridObjectives As gridSearchObjectivesWeight
        Private WithEvents m_btnSave As System.Windows.Forms.Button
        Private WithEvents m_lblAreaClosed As System.Windows.Forms.Label
        Private WithEvents m_lblBaseYear As System.Windows.Forms.Label
        Private WithEvents m_cmbAreaClosed As System.Windows.Forms.ComboBox
        Private WithEvents m_scContent As System.Windows.Forms.SplitContainer
        Private WithEvents m_lblDiscRate As System.Windows.Forms.Label
        Private WithEvents m_lblGenDiscRate As System.Windows.Forms.Label
        Private WithEvents m_tlpMap As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_ucZoomBar As ScientificInterfaceShared.Controls.Map.ucMapZoomToolbar
        Private WithEvents m_nudStartYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudEndYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudStep As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudIterations As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudBestPercentile As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMinArea As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMaxArea As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudBaseYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudDiscRate As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudGenDiscRate As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_cbAutoSave As System.Windows.Forms.CheckBox
        Private WithEvents m_tabSponsors As System.Windows.Forms.TabPage
        Private WithEvents m_tlpSponsors As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_pbLenfest As System.Windows.Forms.PictureBox
        Private WithEvents m_pbDuke As System.Windows.Forms.PictureBox
    End Class
End Namespace
