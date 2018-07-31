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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.m_tvIndicators = New ScientificInterfaceShared.Controls.cThemedTreeView()
        Me.m_tcOutput = New System.Windows.Forms.TabControl()
        Me.m_tpSettings = New System.Windows.Forms.TabPage()
        Me.m_lblCredits = New System.Windows.Forms.Label()
        Me.m_plCredits = New System.Windows.Forms.Panel()
        Me.m_tlpCredits = New System.Windows.Forms.TableLayoutPanel()
        Me.m_pbIRD = New System.Windows.Forms.PictureBox()
        Me.m_pbCSIC = New System.Windows.Forms.PictureBox()
        Me.m_pbEII = New System.Windows.Forms.PictureBox()
        Me.m_btnChangeDefault = New System.Windows.Forms.Button()
        Me.m_btnChooseFolder = New System.Windows.Forms.Button()
        Me.m_tbxDefaultLocation = New System.Windows.Forms.TextBox()
        Me.m_tbxOutputFolder = New System.Windows.Forms.TextBox()
        Me.m_lblSaveTo = New System.Windows.Forms.Label()
        Me.m_rbCustom = New System.Windows.Forms.RadioButton()
        Me.m_rbDefault = New System.Windows.Forms.RadioButton()
        Me.m_cbAutoSaveCSV = New System.Windows.Forms.CheckBox()
        Me.m_cbRunWithMC = New System.Windows.Forms.CheckBox()
        Me.m_cbRunWithEcospace = New System.Windows.Forms.CheckBox()
        Me.m_cbRunWithEcosim = New System.Windows.Forms.CheckBox()
        Me.m_cbPlotAtEnd = New System.Windows.Forms.CheckBox()
        Me.m_cbRunWithEcopath = New System.Windows.Forms.CheckBox()
        Me.m_hdrExport = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrExecution = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tpEcopath = New System.Windows.Forms.TabPage()
        Me.m_grid = New EwEEcologicalIndicatorsPlugin.gridEcopath()
        Me.m_tpEcosim = New System.Windows.Forms.TabPage()
        Me.m_graphSim = New ZedGraph.ZedGraphControl()
        Me.m_tpEcospace = New System.Windows.Forms.TabPage()
        Me.m_legend = New ScientificInterfaceShared.Controls.ucLegendBar()
        Me.m_pbEcospaceMap = New System.Windows.Forms.PictureBox()
        Me.m_tpMCpath = New System.Windows.Forms.TabPage()
        Me.m_graphMCpath = New ZedGraph.ZedGraphControl()
        Me.m_tpMCsim = New System.Windows.Forms.TabPage()
        Me.m_graphMCsim = New ZedGraph.ZedGraphControl()
        Me.m_btnSaveToCSV = New System.Windows.Forms.Button()
        Me.m_pbStatus = New System.Windows.Forms.PictureBox()
        Me.m_llStatus = New ScientificInterfaceShared.Controls.ucLinkLabel()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.m_tcOutput.SuspendLayout()
        Me.m_tpSettings.SuspendLayout()
        Me.m_plCredits.SuspendLayout()
        Me.m_tlpCredits.SuspendLayout()
        CType(Me.m_pbIRD, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbCSIC, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbEII, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpEcopath.SuspendLayout()
        Me.m_tpEcosim.SuspendLayout()
        Me.m_tpEcospace.SuspendLayout()
        CType(Me.m_pbEcospaceMap, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpMCpath.SuspendLayout()
        Me.m_tpMCsim.SuspendLayout()
        CType(Me.m_pbStatus, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.m_tvIndicators)
        resources.ApplyResources(Me.SplitContainer1.Panel1, "SplitContainer1.Panel1")
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.m_tcOutput)
        '
        'm_tvIndicators
        '
        resources.ApplyResources(Me.m_tvIndicators, "m_tvIndicators")
        Me.m_tvIndicators.FullRowSelect = True
        Me.m_tvIndicators.HideSelection = False
        Me.m_tvIndicators.HotTracking = True
        Me.m_tvIndicators.Name = "m_tvIndicators"
        Me.m_tvIndicators.ShowLines = False
        '
        'm_tcOutput
        '
        Me.m_tcOutput.Controls.Add(Me.m_tpSettings)
        Me.m_tcOutput.Controls.Add(Me.m_tpEcopath)
        Me.m_tcOutput.Controls.Add(Me.m_tpEcosim)
        Me.m_tcOutput.Controls.Add(Me.m_tpEcospace)
        Me.m_tcOutput.Controls.Add(Me.m_tpMCpath)
        Me.m_tcOutput.Controls.Add(Me.m_tpMCsim)
        resources.ApplyResources(Me.m_tcOutput, "m_tcOutput")
        Me.m_tcOutput.Name = "m_tcOutput"
        Me.m_tcOutput.SelectedIndex = 0
        '
        'm_tpSettings
        '
        Me.m_tpSettings.Controls.Add(Me.m_lblCredits)
        Me.m_tpSettings.Controls.Add(Me.m_plCredits)
        Me.m_tpSettings.Controls.Add(Me.m_btnChangeDefault)
        Me.m_tpSettings.Controls.Add(Me.m_btnChooseFolder)
        Me.m_tpSettings.Controls.Add(Me.m_tbxDefaultLocation)
        Me.m_tpSettings.Controls.Add(Me.m_tbxOutputFolder)
        Me.m_tpSettings.Controls.Add(Me.m_lblSaveTo)
        Me.m_tpSettings.Controls.Add(Me.m_rbCustom)
        Me.m_tpSettings.Controls.Add(Me.m_rbDefault)
        Me.m_tpSettings.Controls.Add(Me.m_cbAutoSaveCSV)
        Me.m_tpSettings.Controls.Add(Me.m_cbRunWithMC)
        Me.m_tpSettings.Controls.Add(Me.m_cbRunWithEcospace)
        Me.m_tpSettings.Controls.Add(Me.m_cbRunWithEcosim)
        Me.m_tpSettings.Controls.Add(Me.m_cbPlotAtEnd)
        Me.m_tpSettings.Controls.Add(Me.m_cbRunWithEcopath)
        Me.m_tpSettings.Controls.Add(Me.m_hdrExport)
        Me.m_tpSettings.Controls.Add(Me.m_hdrExecution)
        resources.ApplyResources(Me.m_tpSettings, "m_tpSettings")
        Me.m_tpSettings.Name = "m_tpSettings"
        '
        'm_lblCredits
        '
        resources.ApplyResources(Me.m_lblCredits, "m_lblCredits")
        Me.m_lblCredits.Name = "m_lblCredits"
        '
        'm_plCredits
        '
        resources.ApplyResources(Me.m_plCredits, "m_plCredits")
        Me.m_plCredits.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_plCredits.Controls.Add(Me.m_tlpCredits)
        Me.m_plCredits.Name = "m_plCredits"
        '
        'm_tlpCredits
        '
        Me.m_tlpCredits.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.m_tlpCredits, "m_tlpCredits")
        Me.m_tlpCredits.Controls.Add(Me.m_pbIRD, 3, 1)
        Me.m_tlpCredits.Controls.Add(Me.m_pbEII, 5, 1)
        Me.m_tlpCredits.Controls.Add(Me.m_pbCSIC, 1, 1)
        Me.m_tlpCredits.Name = "m_tlpCredits"
        '
        'm_pbIRD
        '
        Me.m_pbIRD.BackgroundImage = Global.EwEEcologicalIndicatorsPlugin.My.Resources.Resources.logo_ird
        resources.ApplyResources(Me.m_pbIRD, "m_pbIRD")
        Me.m_pbIRD.Cursor = System.Windows.Forms.Cursors.Hand
        Me.m_pbIRD.Name = "m_pbIRD"
        Me.m_pbIRD.TabStop = False
        '
        'm_pbCSIC
        '
        Me.m_pbCSIC.BackgroundImage = Global.EwEEcologicalIndicatorsPlugin.My.Resources.Resources.csic
        resources.ApplyResources(Me.m_pbCSIC, "m_pbCSIC")
        Me.m_pbCSIC.Cursor = System.Windows.Forms.Cursors.Hand
        Me.m_pbCSIC.Name = "m_pbCSIC"
        Me.m_pbCSIC.TabStop = False
        '
        'm_pbEII
        '
        Me.m_pbEII.BackgroundImage = Global.EwEEcologicalIndicatorsPlugin.My.Resources.Resources.eii
        resources.ApplyResources(Me.m_pbEII, "m_pbEII")
        Me.m_pbEII.Cursor = System.Windows.Forms.Cursors.Hand
        Me.m_pbEII.Name = "m_pbEII"
        Me.m_pbEII.TabStop = False
        '
        'm_btnChangeDefault
        '
        resources.ApplyResources(Me.m_btnChangeDefault, "m_btnChangeDefault")
        Me.m_btnChangeDefault.Name = "m_btnChangeDefault"
        Me.m_btnChangeDefault.UseVisualStyleBackColor = True
        '
        'm_btnChooseFolder
        '
        resources.ApplyResources(Me.m_btnChooseFolder, "m_btnChooseFolder")
        Me.m_btnChooseFolder.Name = "m_btnChooseFolder"
        Me.m_btnChooseFolder.UseVisualStyleBackColor = True
        '
        'm_tbxDefaultLocation
        '
        resources.ApplyResources(Me.m_tbxDefaultLocation, "m_tbxDefaultLocation")
        Me.m_tbxDefaultLocation.Name = "m_tbxDefaultLocation"
        Me.m_tbxDefaultLocation.ReadOnly = True
        '
        'm_tbxOutputFolder
        '
        resources.ApplyResources(Me.m_tbxOutputFolder, "m_tbxOutputFolder")
        Me.m_tbxOutputFolder.Name = "m_tbxOutputFolder"
        '
        'm_lblSaveTo
        '
        resources.ApplyResources(Me.m_lblSaveTo, "m_lblSaveTo")
        Me.m_lblSaveTo.Name = "m_lblSaveTo"
        '
        'm_rbCustom
        '
        resources.ApplyResources(Me.m_rbCustom, "m_rbCustom")
        Me.m_rbCustom.Name = "m_rbCustom"
        Me.m_rbCustom.TabStop = True
        Me.m_rbCustom.UseVisualStyleBackColor = True
        '
        'm_rbDefault
        '
        resources.ApplyResources(Me.m_rbDefault, "m_rbDefault")
        Me.m_rbDefault.Name = "m_rbDefault"
        Me.m_rbDefault.TabStop = True
        Me.m_rbDefault.UseVisualStyleBackColor = True
        '
        'm_cbAutoSaveCSV
        '
        resources.ApplyResources(Me.m_cbAutoSaveCSV, "m_cbAutoSaveCSV")
        Me.m_cbAutoSaveCSV.Name = "m_cbAutoSaveCSV"
        Me.m_cbAutoSaveCSV.UseVisualStyleBackColor = True
        '
        'm_cbRunWithMC
        '
        resources.ApplyResources(Me.m_cbRunWithMC, "m_cbRunWithMC")
        Me.m_cbRunWithMC.Name = "m_cbRunWithMC"
        Me.m_cbRunWithMC.UseVisualStyleBackColor = True
        '
        'm_cbRunWithEcospace
        '
        resources.ApplyResources(Me.m_cbRunWithEcospace, "m_cbRunWithEcospace")
        Me.m_cbRunWithEcospace.Name = "m_cbRunWithEcospace"
        Me.m_cbRunWithEcospace.UseVisualStyleBackColor = True
        '
        'm_cbRunWithEcosim
        '
        resources.ApplyResources(Me.m_cbRunWithEcosim, "m_cbRunWithEcosim")
        Me.m_cbRunWithEcosim.Name = "m_cbRunWithEcosim"
        Me.m_cbRunWithEcosim.UseVisualStyleBackColor = True
        '
        'm_cbPlotAtEnd
        '
        resources.ApplyResources(Me.m_cbPlotAtEnd, "m_cbPlotAtEnd")
        Me.m_cbPlotAtEnd.Name = "m_cbPlotAtEnd"
        Me.m_cbPlotAtEnd.UseVisualStyleBackColor = True
        '
        'm_cbRunWithEcopath
        '
        resources.ApplyResources(Me.m_cbRunWithEcopath, "m_cbRunWithEcopath")
        Me.m_cbRunWithEcopath.Name = "m_cbRunWithEcopath"
        Me.m_cbRunWithEcopath.UseVisualStyleBackColor = True
        '
        'm_hdrExport
        '
        resources.ApplyResources(Me.m_hdrExport, "m_hdrExport")
        Me.m_hdrExport.CanCollapseParent = False
        Me.m_hdrExport.CollapsedParentHeight = 0
        Me.m_hdrExport.IsCollapsed = False
        Me.m_hdrExport.Name = "m_hdrExport"
        '
        'm_hdrExecution
        '
        resources.ApplyResources(Me.m_hdrExecution, "m_hdrExecution")
        Me.m_hdrExecution.CanCollapseParent = False
        Me.m_hdrExecution.CollapsedParentHeight = 0
        Me.m_hdrExecution.IsCollapsed = False
        Me.m_hdrExecution.Name = "m_hdrExecution"
        '
        'm_tpEcopath
        '
        Me.m_tpEcopath.BackColor = System.Drawing.SystemColors.Control
        Me.m_tpEcopath.Controls.Add(Me.m_grid)
        resources.ApplyResources(Me.m_tpEcopath, "m_tpEcopath")
        Me.m_tpEcopath.Name = "m_tpEcopath"
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        resources.ApplyResources(Me.m_grid, "m_grid")
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
        Me.m_grid.UIContext = Nothing
        '
        'm_tpEcosim
        '
        Me.m_tpEcosim.BackColor = System.Drawing.SystemColors.Control
        Me.m_tpEcosim.Controls.Add(Me.m_graphSim)
        resources.ApplyResources(Me.m_tpEcosim, "m_tpEcosim")
        Me.m_tpEcosim.Name = "m_tpEcosim"
        '
        'm_graphSim
        '
        resources.ApplyResources(Me.m_graphSim, "m_graphSim")
        Me.m_graphSim.Name = "m_graphSim"
        Me.m_graphSim.ScrollGrace = 0R
        Me.m_graphSim.ScrollMaxX = 0R
        Me.m_graphSim.ScrollMaxY = 0R
        Me.m_graphSim.ScrollMaxY2 = 0R
        Me.m_graphSim.ScrollMinX = 0R
        Me.m_graphSim.ScrollMinY = 0R
        Me.m_graphSim.ScrollMinY2 = 0R
        '
        'm_tpEcospace
        '
        Me.m_tpEcospace.BackColor = System.Drawing.SystemColors.Control
        Me.m_tpEcospace.Controls.Add(Me.m_legend)
        Me.m_tpEcospace.Controls.Add(Me.m_pbEcospaceMap)
        resources.ApplyResources(Me.m_tpEcospace, "m_tpEcospace")
        Me.m_tpEcospace.Name = "m_tpEcospace"
        '
        'm_legend
        '
        resources.ApplyResources(Me.m_legend, "m_legend")
        Me.m_legend.BarWidthPercentage = 80
        Me.m_legend.ColorHigh = System.Drawing.Color.DarkGreen
        Me.m_legend.ColorLow = System.Drawing.Color.Red
        Me.m_legend.Colors = CType(resources.GetObject("m_legend.Colors"), System.Collections.Generic.List(Of System.Drawing.Color))
        Me.m_legend.LabelHigh = "High"
        Me.m_legend.LabelLow = "Low"
        Me.m_legend.Name = "m_legend"
        Me.m_legend.UIContext = Nothing
        '
        'm_pbEcospaceMap
        '
        resources.ApplyResources(Me.m_pbEcospaceMap, "m_pbEcospaceMap")
        Me.m_pbEcospaceMap.Name = "m_pbEcospaceMap"
        Me.m_pbEcospaceMap.TabStop = False
        '
        'm_tpMCpath
        '
        Me.m_tpMCpath.Controls.Add(Me.m_graphMCpath)
        resources.ApplyResources(Me.m_tpMCpath, "m_tpMCpath")
        Me.m_tpMCpath.Name = "m_tpMCpath"
        Me.m_tpMCpath.UseVisualStyleBackColor = True
        '
        'm_graphMCpath
        '
        resources.ApplyResources(Me.m_graphMCpath, "m_graphMCpath")
        Me.m_graphMCpath.Name = "m_graphMCpath"
        Me.m_graphMCpath.ScrollGrace = 0R
        Me.m_graphMCpath.ScrollMaxX = 0R
        Me.m_graphMCpath.ScrollMaxY = 0R
        Me.m_graphMCpath.ScrollMaxY2 = 0R
        Me.m_graphMCpath.ScrollMinX = 0R
        Me.m_graphMCpath.ScrollMinY = 0R
        Me.m_graphMCpath.ScrollMinY2 = 0R
        '
        'm_tpMCsim
        '
        Me.m_tpMCsim.Controls.Add(Me.m_graphMCsim)
        resources.ApplyResources(Me.m_tpMCsim, "m_tpMCsim")
        Me.m_tpMCsim.Name = "m_tpMCsim"
        Me.m_tpMCsim.UseVisualStyleBackColor = True
        '
        'm_graphMCsim
        '
        resources.ApplyResources(Me.m_graphMCsim, "m_graphMCsim")
        Me.m_graphMCsim.Name = "m_graphMCsim"
        Me.m_graphMCsim.ScrollGrace = 0R
        Me.m_graphMCsim.ScrollMaxX = 0R
        Me.m_graphMCsim.ScrollMaxY = 0R
        Me.m_graphMCsim.ScrollMaxY2 = 0R
        Me.m_graphMCsim.ScrollMinX = 0R
        Me.m_graphMCsim.ScrollMinY = 0R
        Me.m_graphMCsim.ScrollMinY2 = 0R
        '
        'm_btnSaveToCSV
        '
        resources.ApplyResources(Me.m_btnSaveToCSV, "m_btnSaveToCSV")
        Me.m_btnSaveToCSV.Name = "m_btnSaveToCSV"
        Me.m_btnSaveToCSV.UseVisualStyleBackColor = True
        '
        'm_pbStatus
        '
        resources.ApplyResources(Me.m_pbStatus, "m_pbStatus")
        Me.m_pbStatus.Name = "m_pbStatus"
        Me.m_pbStatus.TabStop = False
        '
        'm_llStatus
        '
        resources.ApplyResources(Me.m_llStatus, "m_llStatus")
        Me.m_llStatus.Name = "m_llStatus"
        Me.m_llStatus.TabStop = True
        Me.m_llStatus.UIContext = Nothing
        Me.m_llStatus.UseCompatibleTextRendering = True
        '
        'frmMain
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_llStatus)
        Me.Controls.Add(Me.m_pbStatus)
        Me.Controls.Add(Me.m_btnSaveToCSV)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmMain"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.m_tcOutput.ResumeLayout(False)
        Me.m_tpSettings.ResumeLayout(False)
        Me.m_tpSettings.PerformLayout()
        Me.m_plCredits.ResumeLayout(False)
        Me.m_tlpCredits.ResumeLayout(False)
        CType(Me.m_pbIRD, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbCSIC, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbEII, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpEcopath.ResumeLayout(False)
        Me.m_tpEcopath.PerformLayout()
        Me.m_tpEcosim.ResumeLayout(False)
        Me.m_tpEcospace.ResumeLayout(False)
        CType(Me.m_pbEcospaceMap, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpMCpath.ResumeLayout(False)
        Me.m_tpMCsim.ResumeLayout(False)
        CType(Me.m_pbStatus, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_tvIndicators As ScientificInterfaceShared.Controls.cThemedTreeView
    Private WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Private WithEvents m_tcOutput As System.Windows.Forms.TabControl
    Private WithEvents m_tpEcopath As System.Windows.Forms.TabPage
    Private WithEvents m_tpEcosim As System.Windows.Forms.TabPage
    Private WithEvents m_grid As gridEcopath
    Private WithEvents m_btnSaveToCSV As System.Windows.Forms.Button
    Private WithEvents m_tpSettings As System.Windows.Forms.TabPage
    Private WithEvents m_btnChooseFolder As System.Windows.Forms.Button
    Private WithEvents m_tbxDefaultLocation As System.Windows.Forms.TextBox
    Private WithEvents m_tbxOutputFolder As System.Windows.Forms.TextBox
    Private WithEvents m_lblSaveTo As System.Windows.Forms.Label
    Private WithEvents m_rbCustom As System.Windows.Forms.RadioButton
    Private WithEvents m_rbDefault As System.Windows.Forms.RadioButton
    Private WithEvents m_cbAutoSaveCSV As System.Windows.Forms.CheckBox
    Private WithEvents m_cbRunWithEcospace As System.Windows.Forms.CheckBox
    Private WithEvents m_cbRunWithEcosim As System.Windows.Forms.CheckBox
    Private WithEvents m_cbRunWithEcopath As System.Windows.Forms.CheckBox
    Private WithEvents m_hdrExport As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrExecution As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tlpCredits As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_pbCSIC As System.Windows.Forms.PictureBox
    Private WithEvents m_pbIRD As System.Windows.Forms.PictureBox
    Private WithEvents m_plCredits As System.Windows.Forms.Panel
    Private WithEvents m_lblCredits As System.Windows.Forms.Label
    Private WithEvents m_cbRunWithMC As System.Windows.Forms.CheckBox
    Private WithEvents m_tpMCsim As System.Windows.Forms.TabPage
    Private WithEvents m_graphSim As ZedGraph.ZedGraphControl
    Private WithEvents m_graphMCsim As ZedGraph.ZedGraphControl
    Private WithEvents m_pbStatus As System.Windows.Forms.PictureBox
    Private WithEvents m_llStatus As ScientificInterfaceShared.Controls.ucLinkLabel
    Private WithEvents m_btnChangeDefault As System.Windows.Forms.Button
    Private WithEvents m_tpEcospace As System.Windows.Forms.TabPage
    Private WithEvents m_pbEcospaceMap As System.Windows.Forms.PictureBox
    Private WithEvents m_legend As ScientificInterfaceShared.Controls.ucLegendBar
    Private WithEvents m_pbEII As System.Windows.Forms.PictureBox
    Private WithEvents m_tpMCpath As System.Windows.Forms.TabPage
    Private WithEvents m_graphMCpath As ZedGraph.ZedGraphControl
    Private WithEvents m_cbPlotAtEnd As System.Windows.Forms.CheckBox
End Class
