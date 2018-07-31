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
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecosim

    Partial Class frmFitToTimeSeries
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFitToTimeSeries))
            Me.m_split1 = New System.Windows.Forms.SplitContainer()
            Me.m_cbResetVs = New System.Windows.Forms.CheckBox()
            Me.m_btnTimeSeriesWeights = New System.Windows.Forms.Button()
            Me.m_splitSearch = New System.Windows.Forms.SplitContainer()
            Me.m_scGrids = New System.Windows.Forms.SplitContainer()
            Me.m_hdrFishingMortality = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_grid = New ScientificInterface.Ecosim.gridFitToTimeSeriesGroup()
            Me.m_tbxAICDataPts = New System.Windows.Forms.TextBox()
            Me.m_btnClearOutputs = New System.Windows.Forms.Button()
            Me.m_gridOutput = New ScientificInterface.Ecosim.gridFitToTimeSeriesOutput()
            Me.m_lblAICDataPts = New System.Windows.Forms.Label()
            Me.m_hdrOutput = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlbSearch = New System.Windows.Forms.TableLayoutPanel()
            Me.m_hdrIterations = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
            Me.m_btnStop = New System.Windows.Forms.Button()
            Me.m_btnSearch = New System.Windows.Forms.Button()
            Me.m_lbResults = New System.Windows.Forms.ListBox()
            Me.m_hdrSearchTypes = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_cbVulnerabilitySearch = New System.Windows.Forms.CheckBox()
            Me.m_cbAnomalySearch = New System.Windows.Forms.CheckBox()
            Me.m_tabSearchOptions = New System.Windows.Forms.TabControl()
            Me.tpVulnerabilitySearch = New System.Windows.Forms.TabPage()
            Me.m_nudVariance = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_tsVulSearchTools = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbSensOfSS2V = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbSearchGroup = New System.Windows.Forms.ToolStripButton()
            Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
            Me.m_vulnerabilityBlockCodeSelector = New ScientificInterface.Ecosim.ucParmBlockCodes()
            Me.m_vulnerabilityBlockMatrix = New ScientificInterface.Ecosim.ucVulnerabiltyBlocks()
            Me.m_lblVarianceVulnerability = New System.Windows.Forms.Label()
            Me.m_tpAnomalySearch = New System.Windows.Forms.TabPage()
            Me.m_cbShowAllData = New System.Windows.Forms.CheckBox()
            Me.m_nudVariancePrimaryProd = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudLastYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudFirstYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudSplinePts = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_splitAnomalyShape = New System.Windows.Forms.SplitContainer()
            Me.m_sketchPad = New ScientificInterface.Ecosim.ucAnomalySearchSketchPad()
            Me.m_shapeToolBox = New ScientificInterfaceShared.Controls.ucShapeToolbox()
            Me.m_hdrAppliedFF = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lbFirstYear = New System.Windows.Forms.Label()
            Me.m_lbLastYear = New System.Windows.Forms.Label()
            Me.m_lbVariancePrimaryProd = New System.Windows.Forms.Label()
            Me.m_lbSplinePoints = New System.Windows.Forms.Label()
            Me.m_hdrSearch = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            CType(Me.m_split1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_split1.Panel1.SuspendLayout()
            Me.m_split1.Panel2.SuspendLayout()
            Me.m_split1.SuspendLayout()
            CType(Me.m_splitSearch, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_splitSearch.Panel1.SuspendLayout()
            Me.m_splitSearch.Panel2.SuspendLayout()
            Me.m_splitSearch.SuspendLayout()
            CType(Me.m_scGrids, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scGrids.Panel1.SuspendLayout()
            Me.m_scGrids.Panel2.SuspendLayout()
            Me.m_scGrids.SuspendLayout()
            Me.m_tlbSearch.SuspendLayout()
            Me.TableLayoutPanel2.SuspendLayout()
            Me.m_tabSearchOptions.SuspendLayout()
            Me.tpVulnerabilitySearch.SuspendLayout()
            CType(Me.m_nudVariance, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tsVulSearchTools.SuspendLayout()
            Me.m_tpAnomalySearch.SuspendLayout()
            CType(Me.m_nudVariancePrimaryProd, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudLastYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudFirstYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSplinePts, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_splitAnomalyShape, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_splitAnomalyShape.Panel1.SuspendLayout()
            Me.m_splitAnomalyShape.Panel2.SuspendLayout()
            Me.m_splitAnomalyShape.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_split1
            '
            Me.m_split1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_split1.Location = New System.Drawing.Point(0, 0)
            Me.m_split1.Name = "m_split1"
            '
            'm_split1.Panel1
            '
            Me.m_split1.Panel1.Controls.Add(Me.m_cbResetVs)
            Me.m_split1.Panel1.Controls.Add(Me.m_btnTimeSeriesWeights)
            Me.m_split1.Panel1.Controls.Add(Me.m_splitSearch)
            Me.m_split1.Panel1.Controls.Add(Me.m_hdrSearchTypes)
            Me.m_split1.Panel1.Controls.Add(Me.m_cbVulnerabilitySearch)
            Me.m_split1.Panel1.Controls.Add(Me.m_cbAnomalySearch)
            Me.m_split1.Panel1.Margin = New System.Windows.Forms.Padding(3)
            Me.m_split1.Panel1.Padding = New System.Windows.Forms.Padding(3)
            Me.m_split1.Panel1MinSize = 249
            '
            'm_split1.Panel2
            '
            Me.m_split1.Panel2.Controls.Add(Me.m_tabSearchOptions)
            Me.m_split1.Panel2.Controls.Add(Me.m_hdrSearch)
            Me.m_split1.Panel2.Padding = New System.Windows.Forms.Padding(0, 3, 0, 0)
            Me.m_split1.Size = New System.Drawing.Size(684, 439)
            Me.m_split1.SplitterDistance = 249
            Me.m_split1.TabIndex = 0
            '
            'm_cbResetVs
            '
            Me.m_cbResetVs.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cbResetVs.AutoSize = True
            Me.m_cbResetVs.Location = New System.Drawing.Point(138, 54)
            Me.m_cbResetVs.Name = "m_cbResetVs"
            Me.m_cbResetVs.Size = New System.Drawing.Size(109, 17)
            Me.m_cbResetVs.TabIndex = 5
            Me.m_cbResetVs.Text = "&Reset V's on Run"
            Me.m_cbResetVs.UseVisualStyleBackColor = True
            '
            'm_btnTimeSeriesWeights
            '
            Me.m_btnTimeSeriesWeights.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnTimeSeriesWeights.Location = New System.Drawing.Point(138, 26)
            Me.m_btnTimeSeriesWeights.Name = "m_btnTimeSeriesWeights"
            Me.m_btnTimeSeriesWeights.Size = New System.Drawing.Size(108, 23)
            Me.m_btnTimeSeriesWeights.TabIndex = 3
            Me.m_btnTimeSeriesWeights.Text = "&Time series..."
            Me.m_btnTimeSeriesWeights.UseVisualStyleBackColor = True
            '
            'm_splitSearch
            '
            Me.m_splitSearch.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_splitSearch.Location = New System.Drawing.Point(0, 73)
            Me.m_splitSearch.Margin = New System.Windows.Forms.Padding(0)
            Me.m_splitSearch.Name = "m_splitSearch"
            Me.m_splitSearch.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_splitSearch.Panel1
            '
            Me.m_splitSearch.Panel1.Controls.Add(Me.m_scGrids)
            '
            'm_splitSearch.Panel2
            '
            Me.m_splitSearch.Panel2.Controls.Add(Me.m_tlbSearch)
            Me.m_splitSearch.Size = New System.Drawing.Size(246, 366)
            Me.m_splitSearch.SplitterDistance = 255
            Me.m_splitSearch.TabIndex = 8
            '
            'm_scGrids
            '
            Me.m_scGrids.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scGrids.Location = New System.Drawing.Point(0, 0)
            Me.m_scGrids.Name = "m_scGrids"
            Me.m_scGrids.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_scGrids.Panel1
            '
            Me.m_scGrids.Panel1.Controls.Add(Me.m_hdrFishingMortality)
            Me.m_scGrids.Panel1.Controls.Add(Me.m_grid)
            '
            'm_scGrids.Panel2
            '
            Me.m_scGrids.Panel2.Controls.Add(Me.m_tbxAICDataPts)
            Me.m_scGrids.Panel2.Controls.Add(Me.m_btnClearOutputs)
            Me.m_scGrids.Panel2.Controls.Add(Me.m_gridOutput)
            Me.m_scGrids.Panel2.Controls.Add(Me.m_lblAICDataPts)
            Me.m_scGrids.Panel2.Controls.Add(Me.m_hdrOutput)
            Me.m_scGrids.Size = New System.Drawing.Size(246, 255)
            Me.m_scGrids.SplitterDistance = 126
            Me.m_scGrids.TabIndex = 0
            '
            'm_hdrFishingMortality
            '
            Me.m_hdrFishingMortality.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrFishingMortality.CanCollapseParent = False
            Me.m_hdrFishingMortality.CollapsedParentHeight = 0
            Me.m_hdrFishingMortality.IsCollapsed = False
            Me.m_hdrFishingMortality.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrFishingMortality.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrFishingMortality.Name = "m_hdrFishingMortality"
            Me.m_hdrFishingMortality.Size = New System.Drawing.Size(246, 18)
            Me.m_hdrFishingMortality.TabIndex = 0
            Me.m_hdrFishingMortality.Text = "Max fishing mortality"
            Me.m_hdrFishingMortality.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            Me.m_grid.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = False
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Location = New System.Drawing.Point(0, 18)
            Me.m_grid.Manager = Nothing
            Me.m_grid.Margin = New System.Windows.Forms.Padding(0)
            Me.m_grid.Name = "m_grid"
            Me.m_grid.Size = New System.Drawing.Size(246, 108)
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.TabIndex = 1
            Me.m_grid.UIContext = Nothing
            '
            'm_tbxAICDataPts
            '
            Me.m_tbxAICDataPts.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.m_tbxAICDataPts.Location = New System.Drawing.Point(111, 104)
            Me.m_tbxAICDataPts.Name = "m_tbxAICDataPts"
            Me.m_tbxAICDataPts.Size = New System.Drawing.Size(48, 20)
            Me.m_tbxAICDataPts.TabIndex = 2
            '
            'm_btnClearOutputs
            '
            Me.m_btnClearOutputs.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnClearOutputs.Location = New System.Drawing.Point(171, 102)
            Me.m_btnClearOutputs.Margin = New System.Windows.Forms.Padding(0)
            Me.m_btnClearOutputs.Name = "m_btnClearOutputs"
            Me.m_btnClearOutputs.Size = New System.Drawing.Size(75, 23)
            Me.m_btnClearOutputs.TabIndex = 3
            Me.m_btnClearOutputs.Text = "&Clear"
            Me.m_btnClearOutputs.UseVisualStyleBackColor = True
            '
            'm_gridOutput
            '
            Me.m_gridOutput.AllowBlockSelect = True
            Me.m_gridOutput.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_gridOutput.AutoSizeMinHeight = 10
            Me.m_gridOutput.AutoSizeMinWidth = 10
            Me.m_gridOutput.AutoStretchColumnsToFitWidth = False
            Me.m_gridOutput.AutoStretchRowsToFitHeight = False
            Me.m_gridOutput.BackColor = System.Drawing.Color.White
            Me.m_gridOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridOutput.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridOutput.CustomSort = False
            Me.m_gridOutput.DataName = "grid content"
            Me.m_gridOutput.FixedColumnWidths = True
            Me.m_gridOutput.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridOutput.GridToolTipActive = True
            Me.m_gridOutput.IsLayoutSuspended = False
            Me.m_gridOutput.IsOutputGrid = True
            Me.m_gridOutput.Location = New System.Drawing.Point(0, 18)
            Me.m_gridOutput.Margin = New System.Windows.Forms.Padding(0)
            Me.m_gridOutput.Name = "m_gridOutput"
            Me.m_gridOutput.NumAICPoints = 0
            Me.m_gridOutput.Size = New System.Drawing.Size(246, 81)
            Me.m_gridOutput.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridOutput.TabIndex = 0
            Me.m_gridOutput.UIContext = Nothing
            '
            'm_lblAICDataPts
            '
            Me.m_lblAICDataPts.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.m_lblAICDataPts.AutoSize = True
            Me.m_lblAICDataPts.Location = New System.Drawing.Point(3, 107)
            Me.m_lblAICDataPts.Name = "m_lblAICDataPts"
            Me.m_lblAICDataPts.Size = New System.Drawing.Size(102, 13)
            Me.m_lblAICDataPts.TabIndex = 1
            Me.m_lblAICDataPts.Text = "No. &AIC data points:"
            '
            'm_hdrOutput
            '
            Me.m_hdrOutput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrOutput.CanCollapseParent = False
            Me.m_hdrOutput.CollapsedParentHeight = 0
            Me.m_hdrOutput.IsCollapsed = False
            Me.m_hdrOutput.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrOutput.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrOutput.Name = "m_hdrOutput"
            Me.m_hdrOutput.Size = New System.Drawing.Size(246, 18)
            Me.m_hdrOutput.TabIndex = 0
            Me.m_hdrOutput.Text = "Output"
            Me.m_hdrOutput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_tlbSearch
            '
            Me.m_tlbSearch.ColumnCount = 1
            Me.m_tlbSearch.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlbSearch.Controls.Add(Me.m_hdrIterations, 0, 0)
            Me.m_tlbSearch.Controls.Add(Me.TableLayoutPanel2, 0, 2)
            Me.m_tlbSearch.Controls.Add(Me.m_lbResults, 0, 1)
            Me.m_tlbSearch.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tlbSearch.Location = New System.Drawing.Point(0, 0)
            Me.m_tlbSearch.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tlbSearch.Name = "m_tlbSearch"
            Me.m_tlbSearch.RowCount = 3
            Me.m_tlbSearch.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18.0!))
            Me.m_tlbSearch.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlbSearch.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
            Me.m_tlbSearch.Size = New System.Drawing.Size(246, 107)
            Me.m_tlbSearch.TabIndex = 0
            '
            'm_hdrIterations
            '
            Me.m_hdrIterations.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrIterations.CanCollapseParent = False
            Me.m_hdrIterations.CollapsedParentHeight = 0
            Me.m_hdrIterations.IsCollapsed = False
            Me.m_hdrIterations.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrIterations.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrIterations.Name = "m_hdrIterations"
            Me.m_hdrIterations.Size = New System.Drawing.Size(246, 18)
            Me.m_hdrIterations.TabIndex = 0
            Me.m_hdrIterations.Text = "Iterations"
            Me.m_hdrIterations.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'TableLayoutPanel2
            '
            Me.TableLayoutPanel2.ColumnCount = 4
            Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75.0!))
            Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75.0!))
            Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.TableLayoutPanel2.Controls.Add(Me.m_btnStop, 1, 0)
            Me.TableLayoutPanel2.Controls.Add(Me.m_btnSearch, 2, 0)
            Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel2.Location = New System.Drawing.Point(0, 77)
            Me.TableLayoutPanel2.Margin = New System.Windows.Forms.Padding(0)
            Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
            Me.TableLayoutPanel2.RowCount = 1
            Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel2.Size = New System.Drawing.Size(246, 30)
            Me.TableLayoutPanel2.TabIndex = 2
            '
            'm_btnStop
            '
            Me.m_btnStop.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnStop.Location = New System.Drawing.Point(51, 3)
            Me.m_btnStop.Name = "m_btnStop"
            Me.m_btnStop.Size = New System.Drawing.Size(69, 24)
            Me.m_btnStop.TabIndex = 0
            Me.m_btnStop.Text = "Stop"
            Me.m_btnStop.UseVisualStyleBackColor = True
            '
            'm_btnSearch
            '
            Me.m_btnSearch.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnSearch.Location = New System.Drawing.Point(126, 3)
            Me.m_btnSearch.Name = "m_btnSearch"
            Me.m_btnSearch.Size = New System.Drawing.Size(69, 24)
            Me.m_btnSearch.TabIndex = 1
            Me.m_btnSearch.Text = "&Search"
            Me.m_btnSearch.UseVisualStyleBackColor = True
            '
            'm_lbResults
            '
            Me.m_lbResults.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_lbResults.FormattingEnabled = True
            Me.m_lbResults.IntegralHeight = False
            Me.m_lbResults.Location = New System.Drawing.Point(0, 18)
            Me.m_lbResults.Margin = New System.Windows.Forms.Padding(0)
            Me.m_lbResults.Name = "m_lbResults"
            Me.m_lbResults.Size = New System.Drawing.Size(246, 59)
            Me.m_lbResults.TabIndex = 3
            '
            'm_hdrSearchTypes
            '
            Me.m_hdrSearchTypes.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrSearchTypes.CanCollapseParent = False
            Me.m_hdrSearchTypes.CollapsedParentHeight = 0
            Me.m_hdrSearchTypes.IsCollapsed = False
            Me.m_hdrSearchTypes.Location = New System.Drawing.Point(0, 3)
            Me.m_hdrSearchTypes.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrSearchTypes.Name = "m_hdrSearchTypes"
            Me.m_hdrSearchTypes.Size = New System.Drawing.Size(246, 18)
            Me.m_hdrSearchTypes.TabIndex = 0
            Me.m_hdrSearchTypes.Text = "Search types"
            Me.m_hdrSearchTypes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_cbVulnerabilitySearch
            '
            Me.m_cbVulnerabilitySearch.AutoSize = True
            Me.m_cbVulnerabilitySearch.Checked = True
            Me.m_cbVulnerabilitySearch.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbVulnerabilitySearch.Location = New System.Drawing.Point(3, 30)
            Me.m_cbVulnerabilitySearch.Name = "m_cbVulnerabilitySearch"
            Me.m_cbVulnerabilitySearch.Size = New System.Drawing.Size(117, 17)
            Me.m_cbVulnerabilitySearch.TabIndex = 2
            Me.m_cbVulnerabilitySearch.Text = "&Vulnerability search"
            Me.m_cbVulnerabilitySearch.UseVisualStyleBackColor = True
            '
            'm_cbAnomalySearch
            '
            Me.m_cbAnomalySearch.AutoSize = True
            Me.m_cbAnomalySearch.Location = New System.Drawing.Point(3, 54)
            Me.m_cbAnomalySearch.Name = "m_cbAnomalySearch"
            Me.m_cbAnomalySearch.Size = New System.Drawing.Size(101, 17)
            Me.m_cbAnomalySearch.TabIndex = 4
            Me.m_cbAnomalySearch.Text = "&Anomaly search"
            Me.m_cbAnomalySearch.UseVisualStyleBackColor = True
            '
            'm_tabSearchOptions
            '
            Me.m_tabSearchOptions.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_tabSearchOptions.Controls.Add(Me.tpVulnerabilitySearch)
            Me.m_tabSearchOptions.Controls.Add(Me.m_tpAnomalySearch)
            Me.m_tabSearchOptions.Location = New System.Drawing.Point(0, 26)
            Me.m_tabSearchOptions.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tabSearchOptions.Name = "m_tabSearchOptions"
            Me.m_tabSearchOptions.SelectedIndex = 0
            Me.m_tabSearchOptions.Size = New System.Drawing.Size(431, 413)
            Me.m_tabSearchOptions.TabIndex = 1
            '
            'tpVulnerabilitySearch
            '
            Me.tpVulnerabilitySearch.Controls.Add(Me.m_nudVariance)
            Me.tpVulnerabilitySearch.Controls.Add(Me.m_tsVulSearchTools)
            Me.tpVulnerabilitySearch.Controls.Add(Me.m_vulnerabilityBlockCodeSelector)
            Me.tpVulnerabilitySearch.Controls.Add(Me.m_vulnerabilityBlockMatrix)
            Me.tpVulnerabilitySearch.Controls.Add(Me.m_lblVarianceVulnerability)
            Me.tpVulnerabilitySearch.Location = New System.Drawing.Point(4, 22)
            Me.tpVulnerabilitySearch.Margin = New System.Windows.Forms.Padding(0)
            Me.tpVulnerabilitySearch.Name = "tpVulnerabilitySearch"
            Me.tpVulnerabilitySearch.Padding = New System.Windows.Forms.Padding(3)
            Me.tpVulnerabilitySearch.Size = New System.Drawing.Size(423, 387)
            Me.tpVulnerabilitySearch.TabIndex = 0
            Me.tpVulnerabilitySearch.Text = "Vulnerability Search"
            Me.tpVulnerabilitySearch.UseVisualStyleBackColor = True
            '
            'm_nudVariance
            '
            Me.m_nudVariance.DecimalPlaces = 3
            Me.m_nudVariance.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudVariance.Location = New System.Drawing.Point(76, 86)
            Me.m_nudVariance.Name = "m_nudVariance"
            Me.m_nudVariance.Size = New System.Drawing.Size(74, 20)
            Me.m_nudVariance.TabIndex = 4
            Me.m_nudVariance.Value = New Decimal(New Integer() {1, 0, 0, 65536})
            '
            'm_tsVulSearchTools
            '
            Me.m_tsVulSearchTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsVulSearchTools.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbSensOfSS2V, Me.m_tsbSearchGroup, Me.ToolStripButton1})
            Me.m_tsVulSearchTools.Location = New System.Drawing.Point(3, 3)
            Me.m_tsVulSearchTools.Name = "m_tsVulSearchTools"
            Me.m_tsVulSearchTools.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            Me.m_tsVulSearchTools.Size = New System.Drawing.Size(417, 25)
            Me.m_tsVulSearchTools.TabIndex = 0
            '
            'm_tsbSensOfSS2V
            '
            Me.m_tsbSensOfSS2V.Image = CType(resources.GetObject("m_tsbSensOfSS2V.Image"), System.Drawing.Image)
            Me.m_tsbSensOfSS2V.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbSensOfSS2V.Name = "m_tsbSensOfSS2V"
            Me.m_tsbSensOfSS2V.Size = New System.Drawing.Size(133, 22)
            Me.m_tsbSensOfSS2V.Text = "Sensitivity of SS to V"
            '
            'm_tsbSearchGroup
            '
            Me.m_tsbSearchGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            Me.m_tsbSearchGroup.Image = CType(resources.GetObject("m_tsbSearchGroup.Image"), System.Drawing.Image)
            Me.m_tsbSearchGroup.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbSearchGroup.Name = "m_tsbSearchGroup"
            Me.m_tsbSearchGroup.Size = New System.Drawing.Size(171, 22)
            Me.m_tsbSearchGroup.Text = "Search groups with time series"
            '
            'ToolStripButton1
            '
            Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
            Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.ToolStripButton1.Name = "ToolStripButton1"
            Me.ToolStripButton1.Size = New System.Drawing.Size(90, 22)
            Me.ToolStripButton1.Text = "Apply again"
            Me.ToolStripButton1.Visible = False
            '
            'm_vulnerabilityBlockCodeSelector
            '
            Me.m_vulnerabilityBlockCodeSelector.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_vulnerabilityBlockCodeSelector.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_vulnerabilityBlockCodeSelector.Location = New System.Drawing.Point(0, 30)
            Me.m_vulnerabilityBlockCodeSelector.Margin = New System.Windows.Forms.Padding(0)
            Me.m_vulnerabilityBlockCodeSelector.Name = "m_vulnerabilityBlockCodeSelector"
            Me.m_vulnerabilityBlockCodeSelector.NumBlocks = 30
            Me.m_vulnerabilityBlockCodeSelector.SelectedBlock = 15
            Me.m_vulnerabilityBlockCodeSelector.Size = New System.Drawing.Size(418, 52)
            Me.m_vulnerabilityBlockCodeSelector.TabIndex = 1
            Me.m_vulnerabilityBlockCodeSelector.UIContext = Nothing
            '
            'm_vulnerabilityBlockMatrix
            '
            Me.m_vulnerabilityBlockMatrix.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_vulnerabilityBlockMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_vulnerabilityBlockMatrix.BlockColors = Nothing
            Me.m_vulnerabilityBlockMatrix.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_vulnerabilityBlockMatrix.Location = New System.Drawing.Point(-1, 112)
            Me.m_vulnerabilityBlockMatrix.Name = "m_vulnerabilityBlockMatrix"
            Me.m_vulnerabilityBlockMatrix.SelectedBlockNum = 0
            Me.m_vulnerabilityBlockMatrix.Size = New System.Drawing.Size(421, 275)
            Me.m_vulnerabilityBlockMatrix.TabIndex = 2
            Me.m_vulnerabilityBlockMatrix.TabStop = False
            Me.m_vulnerabilityBlockMatrix.UIContext = Nothing
            '
            'm_lblVarianceVulnerability
            '
            Me.m_lblVarianceVulnerability.AutoSize = True
            Me.m_lblVarianceVulnerability.Location = New System.Drawing.Point(1, 88)
            Me.m_lblVarianceVulnerability.Name = "m_lblVarianceVulnerability"
            Me.m_lblVarianceVulnerability.Size = New System.Drawing.Size(52, 13)
            Me.m_lblVarianceVulnerability.TabIndex = 2
            Me.m_lblVarianceVulnerability.Text = "Va&riance:"
            '
            'm_tpAnomalySearch
            '
            Me.m_tpAnomalySearch.Controls.Add(Me.m_cbShowAllData)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_nudVariancePrimaryProd)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_nudLastYear)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_nudFirstYear)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_nudSplinePts)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_splitAnomalyShape)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_lbFirstYear)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_lbLastYear)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_lbVariancePrimaryProd)
            Me.m_tpAnomalySearch.Controls.Add(Me.m_lbSplinePoints)
            Me.m_tpAnomalySearch.Location = New System.Drawing.Point(4, 22)
            Me.m_tpAnomalySearch.Name = "m_tpAnomalySearch"
            Me.m_tpAnomalySearch.Padding = New System.Windows.Forms.Padding(3)
            Me.m_tpAnomalySearch.Size = New System.Drawing.Size(423, 387)
            Me.m_tpAnomalySearch.TabIndex = 1
            Me.m_tpAnomalySearch.Text = "Anomaly Search"
            Me.m_tpAnomalySearch.UseVisualStyleBackColor = True
            '
            'm_cbShowAllData
            '
            Me.m_cbShowAllData.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cbShowAllData.AutoSize = True
            Me.m_cbShowAllData.Location = New System.Drawing.Point(296, 32)
            Me.m_cbShowAllData.Name = "m_cbShowAllData"
            Me.m_cbShowAllData.Size = New System.Drawing.Size(121, 17)
            Me.m_cbShowAllData.TabIndex = 9
            Me.m_cbShowAllData.Text = "Show all data points"
            Me.m_cbShowAllData.UseVisualStyleBackColor = True
            '
            'm_nudVariancePrimaryProd
            '
            Me.m_nudVariancePrimaryProd.DecimalPlaces = 3
            Me.m_nudVariancePrimaryProd.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudVariancePrimaryProd.Location = New System.Drawing.Point(222, 5)
            Me.m_nudVariancePrimaryProd.Name = "m_nudVariancePrimaryProd"
            Me.m_nudVariancePrimaryProd.Size = New System.Drawing.Size(53, 20)
            Me.m_nudVariancePrimaryProd.TabIndex = 3
            Me.m_nudVariancePrimaryProd.Value = New Decimal(New Integer() {1, 0, 0, 65536})
            '
            'm_nudLastYear
            '
            Me.m_nudLastYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudLastYear.Location = New System.Drawing.Point(63, 29)
            Me.m_nudLastYear.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
            Me.m_nudLastYear.Name = "m_nudLastYear"
            Me.m_nudLastYear.Size = New System.Drawing.Size(60, 20)
            Me.m_nudLastYear.TabIndex = 5
            Me.m_nudLastYear.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_nudFirstYear
            '
            Me.m_nudFirstYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudFirstYear.Location = New System.Drawing.Point(63, 5)
            Me.m_nudFirstYear.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
            Me.m_nudFirstYear.Name = "m_nudFirstYear"
            Me.m_nudFirstYear.Size = New System.Drawing.Size(60, 20)
            Me.m_nudFirstYear.TabIndex = 1
            '
            'm_nudSplinePts
            '
            Me.m_nudSplinePts.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSplinePts.Location = New System.Drawing.Point(223, 30)
            Me.m_nudSplinePts.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
            Me.m_nudSplinePts.Name = "m_nudSplinePts"
            Me.m_nudSplinePts.Size = New System.Drawing.Size(52, 20)
            Me.m_nudSplinePts.TabIndex = 7
            '
            'm_splitAnomalyShape
            '
            Me.m_splitAnomalyShape.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_splitAnomalyShape.Location = New System.Drawing.Point(0, 53)
            Me.m_splitAnomalyShape.Margin = New System.Windows.Forms.Padding(0)
            Me.m_splitAnomalyShape.Name = "m_splitAnomalyShape"
            Me.m_splitAnomalyShape.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_splitAnomalyShape.Panel1
            '
            Me.m_splitAnomalyShape.Panel1.Controls.Add(Me.m_sketchPad)
            '
            'm_splitAnomalyShape.Panel2
            '
            Me.m_splitAnomalyShape.Panel2.Controls.Add(Me.m_shapeToolBox)
            Me.m_splitAnomalyShape.Panel2.Controls.Add(Me.m_hdrAppliedFF)
            Me.m_splitAnomalyShape.Size = New System.Drawing.Size(423, 334)
            Me.m_splitAnomalyShape.SplitterDistance = 245
            Me.m_splitAnomalyShape.TabIndex = 8
            '
            'm_sketchPad
            '
            Me.m_sketchPad.AllowDragXMark = False
            Me.m_sketchPad.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_sketchPad.AxisTickMarkDisplayMode = ScientificInterfaceShared.Definitions.eAxisTickmarkDisplayModeTypes.Absolute
            Me.m_sketchPad.BackColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(250, Byte), Integer))
            Me.m_sketchPad.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_sketchPad.CanEditPoints = True
            Me.m_sketchPad.Cursor = System.Windows.Forms.Cursors.Hand
            Me.m_sketchPad.DisplayAxis = True
            Me.m_sketchPad.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_sketchPad.Editable = False
            Me.m_sketchPad.FirstYear = 0
            Me.m_sketchPad.Handler = Nothing
            Me.m_sketchPad.IsSeasonal = False
            Me.m_sketchPad.LastYear = 0
            Me.m_sketchPad.Location = New System.Drawing.Point(0, 0)
            Me.m_sketchPad.Margin = New System.Windows.Forms.Padding(0)
            Me.m_sketchPad.Name = "m_sketchPad"
            Me.m_sketchPad.NumDataPoints = 0
            Me.m_sketchPad.NumSplinePoints = 0
            Me.m_sketchPad.Shape = Nothing
            Me.m_sketchPad.ShapeColor = System.Drawing.Color.AliceBlue
            Me.m_sketchPad.ShowValueTooltip = True
            Me.m_sketchPad.ShowXMark = False
            Me.m_sketchPad.ShowYMark = False
            Me.m_sketchPad.Size = New System.Drawing.Size(423, 245)
            Me.m_sketchPad.SketchDrawMode = ScientificInterfaceShared.Definitions.eSketchDrawModeTypes.Fill
            Me.m_sketchPad.TabIndex = 0
            Me.m_sketchPad.UIContext = Nothing
            Me.m_sketchPad.XAxisMaxValue = -9999
            Me.m_sketchPad.XMarkValue = -9999.0!
            Me.m_sketchPad.YAxisAutoScaleMode = ScientificInterfaceShared.Definitions.eAxisAutoScaleModeTypes.[Auto]
            Me.m_sketchPad.YAxisMaxValue = 0!
            Me.m_sketchPad.YAxisMinValue = 1.0!
            Me.m_sketchPad.YMarkLabel = ""
            Me.m_sketchPad.YMarkValue = -9999.0!
            '
            'm_shapeToolBox
            '
            Me.m_shapeToolBox.AllowCheckboxes = False
            Me.m_shapeToolBox.AutoSize = True
            Me.m_shapeToolBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_shapeToolBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_shapeToolBox.Color = System.Drawing.Color.Empty
            Me.m_shapeToolBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_shapeToolBox.Handler = Nothing
            Me.m_shapeToolBox.Location = New System.Drawing.Point(0, 23)
            Me.m_shapeToolBox.Margin = New System.Windows.Forms.Padding(0)
            Me.m_shapeToolBox.MinimumSize = New System.Drawing.Size(10, 10)
            Me.m_shapeToolBox.Name = "m_shapeToolBox"
            Me.m_shapeToolBox.Selection = New EwECore.cShapeData(-1) {}
            Me.m_shapeToolBox.Size = New System.Drawing.Size(423, 62)
            Me.m_shapeToolBox.SketchDrawMode = ScientificInterfaceShared.Definitions.eSketchDrawModeTypes.Fill
            Me.m_shapeToolBox.TabIndex = 2
            Me.m_shapeToolBox.UIContext = Nothing
            Me.m_shapeToolBox.XAxisMaxValue = -9999
            Me.m_shapeToolBox.YAxisMinValue = -9999.0!
            '
            'm_hdrAppliedFF
            '
            Me.m_hdrAppliedFF.CanCollapseParent = False
            Me.m_hdrAppliedFF.CollapsedParentHeight = 0
            Me.m_hdrAppliedFF.Dock = System.Windows.Forms.DockStyle.Top
            Me.m_hdrAppliedFF.IsCollapsed = False
            Me.m_hdrAppliedFF.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrAppliedFF.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrAppliedFF.Name = "m_hdrAppliedFF"
            Me.m_hdrAppliedFF.Size = New System.Drawing.Size(423, 23)
            Me.m_hdrAppliedFF.TabIndex = 0
            Me.m_hdrAppliedFF.Text = "Applied Forcing Functions"
            Me.m_hdrAppliedFF.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_lbFirstYear
            '
            Me.m_lbFirstYear.AutoSize = True
            Me.m_lbFirstYear.Location = New System.Drawing.Point(4, 7)
            Me.m_lbFirstYear.Name = "m_lbFirstYear"
            Me.m_lbFirstYear.Size = New System.Drawing.Size(52, 13)
            Me.m_lbFirstYear.TabIndex = 0
            Me.m_lbFirstYear.Text = "First year:"
            '
            'm_lbLastYear
            '
            Me.m_lbLastYear.AutoSize = True
            Me.m_lbLastYear.Location = New System.Drawing.Point(4, 33)
            Me.m_lbLastYear.Name = "m_lbLastYear"
            Me.m_lbLastYear.Size = New System.Drawing.Size(53, 13)
            Me.m_lbLastYear.TabIndex = 4
            Me.m_lbLastYear.Text = "Last year:"
            '
            'm_lbVariancePrimaryProd
            '
            Me.m_lbVariancePrimaryProd.AutoSize = True
            Me.m_lbVariancePrimaryProd.Location = New System.Drawing.Point(148, 7)
            Me.m_lbVariancePrimaryProd.Name = "m_lbVariancePrimaryProd"
            Me.m_lbVariancePrimaryProd.Size = New System.Drawing.Size(69, 13)
            Me.m_lbVariancePrimaryProd.TabIndex = 2
            Me.m_lbVariancePrimaryProd.Text = "&PP Variance:"
            '
            'm_lbSplinePoints
            '
            Me.m_lbSplinePoints.AutoSize = True
            Me.m_lbSplinePoints.Location = New System.Drawing.Point(147, 33)
            Me.m_lbSplinePoints.Name = "m_lbSplinePoints"
            Me.m_lbSplinePoints.Size = New System.Drawing.Size(70, 13)
            Me.m_lbSplinePoints.TabIndex = 6
            Me.m_lbSplinePoints.Text = "Spline points:"
            '
            'm_hdrSearch
            '
            Me.m_hdrSearch.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrSearch.CanCollapseParent = False
            Me.m_hdrSearch.CollapsedParentHeight = 0
            Me.m_hdrSearch.IsCollapsed = False
            Me.m_hdrSearch.Location = New System.Drawing.Point(0, 3)
            Me.m_hdrSearch.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrSearch.Name = "m_hdrSearch"
            Me.m_hdrSearch.Size = New System.Drawing.Size(431, 18)
            Me.m_hdrSearch.TabIndex = 0
            Me.m_hdrSearch.Text = "Search"
            Me.m_hdrSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'frmFitToTimeSeries
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.ClientSize = New System.Drawing.Size(684, 439)
            Me.Controls.Add(Me.m_split1)
            Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.MinimumSize = New System.Drawing.Size(700, 400)
            Me.Name = "frmFitToTimeSeries"
            Me.TabText = "Fit to time series"
            Me.Text = "Fit to time series"
            Me.m_split1.Panel1.ResumeLayout(False)
            Me.m_split1.Panel1.PerformLayout()
            Me.m_split1.Panel2.ResumeLayout(False)
            CType(Me.m_split1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_split1.ResumeLayout(False)
            Me.m_splitSearch.Panel1.ResumeLayout(False)
            Me.m_splitSearch.Panel2.ResumeLayout(False)
            CType(Me.m_splitSearch, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_splitSearch.ResumeLayout(False)
            Me.m_scGrids.Panel1.ResumeLayout(False)
            Me.m_scGrids.Panel2.ResumeLayout(False)
            Me.m_scGrids.Panel2.PerformLayout()
            CType(Me.m_scGrids, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scGrids.ResumeLayout(False)
            Me.m_tlbSearch.ResumeLayout(False)
            Me.TableLayoutPanel2.ResumeLayout(False)
            Me.m_tabSearchOptions.ResumeLayout(False)
            Me.tpVulnerabilitySearch.ResumeLayout(False)
            Me.tpVulnerabilitySearch.PerformLayout()
            CType(Me.m_nudVariance, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tsVulSearchTools.ResumeLayout(False)
            Me.m_tsVulSearchTools.PerformLayout()
            Me.m_tpAnomalySearch.ResumeLayout(False)
            Me.m_tpAnomalySearch.PerformLayout()
            CType(Me.m_nudVariancePrimaryProd, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudLastYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudFirstYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSplinePts, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_splitAnomalyShape.Panel1.ResumeLayout(False)
            Me.m_splitAnomalyShape.Panel2.ResumeLayout(False)
            Me.m_splitAnomalyShape.Panel2.PerformLayout()
            CType(Me.m_splitAnomalyShape, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_splitAnomalyShape.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_split1 As System.Windows.Forms.SplitContainer
        Private WithEvents m_lbSplinePoints As System.Windows.Forms.Label
        Private WithEvents m_btnSearch As System.Windows.Forms.Button
        Private WithEvents m_lbVariancePrimaryProd As System.Windows.Forms.Label
        Private WithEvents m_lbLastYear As System.Windows.Forms.Label
        Private WithEvents m_lbFirstYear As System.Windows.Forms.Label
        Private WithEvents m_cbAnomalySearch As System.Windows.Forms.CheckBox
        Private WithEvents m_lblVarianceVulnerability As System.Windows.Forms.Label
        Private WithEvents m_cbVulnerabilitySearch As System.Windows.Forms.CheckBox
        Private WithEvents m_vulnerabilityBlockMatrix As ucVulnerabiltyBlocks
        Private WithEvents m_vulnerabilityBlockCodeSelector As ucParmBlockCodes
        Private WithEvents m_splitSearch As System.Windows.Forms.SplitContainer
        Private WithEvents m_btnStop As System.Windows.Forms.Button
        Private WithEvents m_tabSearchOptions As System.Windows.Forms.TabControl
        Private WithEvents tpVulnerabilitySearch As System.Windows.Forms.TabPage
        Private WithEvents m_tpAnomalySearch As System.Windows.Forms.TabPage
        Private WithEvents m_hdrFishingMortality As cEwEHeaderLabel
        Private WithEvents m_splitAnomalyShape As System.Windows.Forms.SplitContainer
        Private WithEvents m_sketchPad As ucAnomalySearchSketchPad
        Private WithEvents m_tlbSearch As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_hdrIterations As cEwEHeaderLabel
        Private WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_hdrSearchTypes As cEwEHeaderLabel
        Private WithEvents m_tsVulSearchTools As cEwEToolstrip
        Private WithEvents m_tsbSensOfSS2V As System.Windows.Forms.ToolStripButton
        Private WithEvents m_hdrAppliedFF As cEwEHeaderLabel
        Private WithEvents m_grid As gridFitToTimeSeriesGroup
        Private WithEvents m_btnTimeSeriesWeights As System.Windows.Forms.Button
        Private WithEvents m_hdrSearch As cEwEHeaderLabel
        Private WithEvents m_tsbSearchGroup As System.Windows.Forms.ToolStripButton
        Private WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
        Private WithEvents m_lblAICDataPts As System.Windows.Forms.Label
        Private WithEvents m_scGrids As System.Windows.Forms.SplitContainer
        Private WithEvents m_hdrOutput As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_gridOutput As ScientificInterface.Ecosim.gridFitToTimeSeriesOutput
        Friend WithEvents m_btnClearOutputs As System.Windows.Forms.Button
        Private WithEvents m_tbxAICDataPts As System.Windows.Forms.TextBox
        Private WithEvents m_cbResetVs As System.Windows.Forms.CheckBox
        Private WithEvents m_cbShowAllData As System.Windows.Forms.CheckBox
        Private WithEvents m_nudSplinePts As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudLastYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudFirstYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudVariance As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudVariancePrimaryProd As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_lbResults As System.Windows.Forms.ListBox
        Private WithEvents m_shapeToolBox As ScientificInterfaceShared.Controls.ucShapeToolbox
    End Class

End Namespace

