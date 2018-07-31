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
Imports ScientificInterfaceShared
Imports SharedResources = ScientificInterfaceShared.My.Resources

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucResults
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucResults))
        Me.m_btnRunEcopath = New System.Windows.Forms.Button()
        Me.m_tsResults = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tslblData = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmbGraphData = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tssSep3 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tslItem = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmbItems = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tslUnit = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmbUnit = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tssep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsbShowFlow = New System.Windows.Forms.ToolStripButton()
        Me.m_tssep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tslbYear = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscbYear = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tsbEcopath = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbEcosim = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbEquilibrium = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsbnSave = New System.Windows.Forms.ToolStripButton()
        Me.m_btnRunEcosim = New System.Windows.Forms.Button()
        Me.m_scResults = New System.Windows.Forms.SplitContainer()
        Me.m_plFlow = New EwEValueChainPlugin.plFlow()
        Me.m_btnRunEquilibrium = New System.Windows.Forms.Button()
        Me.m_lblAgg = New System.Windows.Forms.Label()
        Me.m_cmbAgg = New System.Windows.Forms.ComboBox()
        Me.m_tsResults.SuspendLayout()
        CType(Me.m_scResults, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scResults.Panel1.SuspendLayout()
        Me.m_scResults.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_btnRunEcopath
        '
        Me.m_btnRunEcopath.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnRunEcopath.Location = New System.Drawing.Point(700, 522)
        Me.m_btnRunEcopath.Margin = New System.Windows.Forms.Padding(0)
        Me.m_btnRunEcopath.Name = "m_btnRunEcopath"
        Me.m_btnRunEcopath.Size = New System.Drawing.Size(100, 23)
        Me.m_btnRunEcopath.TabIndex = 0
        Me.m_btnRunEcopath.Text = "&Run Ecopath"
        Me.m_btnRunEcopath.UseVisualStyleBackColor = True
        '
        'm_tsResults
        '
        Me.m_tsResults.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsResults.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslblData, Me.m_tscmbGraphData, Me.m_tssSep3, Me.m_tslItem, Me.m_tscmbItems, Me.m_tslUnit, Me.m_tscmbUnit, Me.m_tssep1, Me.m_tsbShowFlow, Me.m_tssep2, Me.m_tslbYear, Me.m_tscbYear, Me.m_tsbEcopath, Me.m_tsbEcosim, Me.m_tsbEquilibrium, Me.ToolStripSeparator1, Me.m_tsbnSave})
        Me.m_tsResults.Location = New System.Drawing.Point(0, 0)
        Me.m_tsResults.Name = "m_tsResults"
        Me.m_tsResults.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.m_tsResults.Size = New System.Drawing.Size(1016, 25)
        Me.m_tsResults.TabIndex = 2
        '
        'm_tslblData
        '
        Me.m_tslblData.Name = "m_tslblData"
        Me.m_tslblData.Size = New System.Drawing.Size(34, 22)
        Me.m_tslblData.Text = "&Data:"
        '
        'm_tscmbGraphData
        '
        Me.m_tscmbGraphData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmbGraphData.Name = "m_tscmbGraphData"
        Me.m_tscmbGraphData.Size = New System.Drawing.Size(121, 25)
        Me.m_tscmbGraphData.Sorted = True
        '
        'm_tssSep3
        '
        Me.m_tssSep3.Name = "m_tssSep3"
        Me.m_tssSep3.Size = New System.Drawing.Size(6, 25)
        '
        'm_tslItem
        '
        Me.m_tslItem.Name = "m_tslItem"
        Me.m_tslItem.Size = New System.Drawing.Size(34, 22)
        Me.m_tslItem.Text = "&Item:"
        '
        'm_tscmbItems
        '
        Me.m_tscmbItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmbItems.Name = "m_tscmbItems"
        Me.m_tscmbItems.Size = New System.Drawing.Size(150, 25)
        '
        'm_tslUnit
        '
        Me.m_tslUnit.Name = "m_tslUnit"
        Me.m_tslUnit.Size = New System.Drawing.Size(32, 22)
        Me.m_tslUnit.Text = "Unit:"
        '
        'm_tscmbUnit
        '
        Me.m_tscmbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmbUnit.Name = "m_tscmbUnit"
        Me.m_tscmbUnit.Size = New System.Drawing.Size(150, 25)
        Me.m_tscmbUnit.Sorted = True
        '
        'm_tssep1
        '
        Me.m_tssep1.Name = "m_tssep1"
        Me.m_tssep1.Size = New System.Drawing.Size(6, 25)
        '
        'm_tsbShowFlow
        '
        Me.m_tsbShowFlow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsbShowFlow.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbShowFlow.Name = "m_tsbShowFlow"
        Me.m_tsbShowFlow.Size = New System.Drawing.Size(66, 22)
        Me.m_tsbShowFlow.Text = "&Show flow"
        '
        'm_tssep2
        '
        Me.m_tssep2.Name = "m_tssep2"
        Me.m_tssep2.Size = New System.Drawing.Size(6, 25)
        '
        'm_tslbYear
        '
        Me.m_tslbYear.Name = "m_tslbYear"
        Me.m_tslbYear.Size = New System.Drawing.Size(64, 22)
        Me.m_tslbYear.Text = "Show &year:"
        '
        'm_tscbYear
        '
        Me.m_tscbYear.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscbYear.Name = "m_tscbYear"
        Me.m_tscbYear.Size = New System.Drawing.Size(75, 25)
        '
        'm_tsbEcopath
        '
        Me.m_tsbEcopath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbEcopath.Image = CType(resources.GetObject("m_tsbEcopath.Image"), System.Drawing.Image)
        Me.m_tsbEcopath.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbEcopath.Name = "m_tsbEcopath"
        Me.m_tsbEcopath.Size = New System.Drawing.Size(23, 22)
        Me.m_tsbEcopath.Text = "Table"
        Me.m_tsbEcopath.ToolTipText = "Show value chain results table"
        '
        'm_tsbEcosim
        '
        Me.m_tsbEcosim.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbEcosim.Image = CType(resources.GetObject("m_tsbEcosim.Image"), System.Drawing.Image)
        Me.m_tsbEcosim.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbEcosim.Name = "m_tsbEcosim"
        Me.m_tsbEcosim.Size = New System.Drawing.Size(23, 22)
        Me.m_tsbEcosim.Text = "Graph"
        Me.m_tsbEcosim.ToolTipText = "Show value chain results graph"
        '
        'm_tsbEquilibrium
        '
        Me.m_tsbEquilibrium.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbEquilibrium.Image = CType(resources.GetObject("m_tsbEquilibrium.Image"), System.Drawing.Image)
        Me.m_tsbEquilibrium.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbEquilibrium.Name = "m_tsbEquilibrium"
        Me.m_tsbEquilibrium.Size = New System.Drawing.Size(23, 22)
        Me.m_tsbEquilibrium.Text = "Equilibrium"
        Me.m_tsbEquilibrium.ToolTipText = "Show equilibrium results graph"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'm_tsbnSave
        '
        Me.m_tsbnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbnSave.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbnSave.Name = "m_tsbnSave"
        Me.m_tsbnSave.Size = New System.Drawing.Size(23, 22)
        Me.m_tsbnSave.Text = "&Save results"
        '
        'm_btnRunEcosim
        '
        Me.m_btnRunEcosim.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnRunEcosim.Location = New System.Drawing.Point(806, 522)
        Me.m_btnRunEcosim.Margin = New System.Windows.Forms.Padding(0)
        Me.m_btnRunEcosim.Name = "m_btnRunEcosim"
        Me.m_btnRunEcosim.Size = New System.Drawing.Size(100, 23)
        Me.m_btnRunEcosim.TabIndex = 0
        Me.m_btnRunEcosim.Text = "Run &Ecosim"
        Me.m_btnRunEcosim.UseVisualStyleBackColor = True
        '
        'm_scResults
        '
        Me.m_scResults.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_scResults.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.m_scResults.Location = New System.Drawing.Point(0, 25)
        Me.m_scResults.Margin = New System.Windows.Forms.Padding(0)
        Me.m_scResults.Name = "m_scResults"
        Me.m_scResults.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'm_scResults.Panel1
        '
        Me.m_scResults.Panel1.Controls.Add(Me.m_plFlow)
        Me.m_scResults.Size = New System.Drawing.Size(1016, 490)
        Me.m_scResults.SplitterDistance = 72
        Me.m_scResults.TabIndex = 3
        '
        'm_plFlow
        '
        Me.m_plFlow.AutoScroll = True
        Me.m_plFlow.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plFlow.EditMode = EwEValueChainPlugin.plFlow.eEditMode.[ReadOnly]
        Me.m_plFlow.ItemFilter = Nothing
        Me.m_plFlow.Location = New System.Drawing.Point(0, 0)
        Me.m_plFlow.Margin = New System.Windows.Forms.Padding(0)
        Me.m_plFlow.Name = "m_plFlow"
        Me.m_plFlow.ShowGrid = False
        Me.m_plFlow.Size = New System.Drawing.Size(1012, 68)
        Me.m_plFlow.TabIndex = 0
        Me.m_plFlow.UnitFilter = Nothing
        '
        'm_btnRunEquilibrium
        '
        Me.m_btnRunEquilibrium.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnRunEquilibrium.Location = New System.Drawing.Point(912, 522)
        Me.m_btnRunEquilibrium.Margin = New System.Windows.Forms.Padding(0)
        Me.m_btnRunEquilibrium.Name = "m_btnRunEquilibrium"
        Me.m_btnRunEquilibrium.Size = New System.Drawing.Size(100, 23)
        Me.m_btnRunEquilibrium.TabIndex = 0
        Me.m_btnRunEquilibrium.Text = "Run E&quilibrium"
        Me.m_btnRunEquilibrium.UseVisualStyleBackColor = True
        '
        'm_lblAgg
        '
        Me.m_lblAgg.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_lblAgg.AutoSize = True
        Me.m_lblAgg.Location = New System.Drawing.Point(464, 527)
        Me.m_lblAgg.Name = "m_lblAgg"
        Me.m_lblAgg.Size = New System.Drawing.Size(92, 13)
        Me.m_lblAgg.TabIndex = 4
        Me.m_lblAgg.Text = "&Data aggregation:"
        '
        'm_cmbAgg
        '
        Me.m_cmbAgg.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_cmbAgg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbAgg.FormattingEnabled = True
        Me.m_cmbAgg.Location = New System.Drawing.Point(562, 524)
        Me.m_cmbAgg.Name = "m_cmbAgg"
        Me.m_cmbAgg.Size = New System.Drawing.Size(121, 21)
        Me.m_cmbAgg.TabIndex = 5
        '
        'ucResults
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.m_cmbAgg)
        Me.Controls.Add(Me.m_lblAgg)
        Me.Controls.Add(Me.m_scResults)
        Me.Controls.Add(Me.m_btnRunEquilibrium)
        Me.Controls.Add(Me.m_btnRunEcosim)
        Me.Controls.Add(Me.m_btnRunEcopath)
        Me.Controls.Add(Me.m_tsResults)
        Me.Name = "ucResults"
        Me.Size = New System.Drawing.Size(1016, 552)
        Me.m_tsResults.ResumeLayout(False)
        Me.m_tsResults.PerformLayout()
        Me.m_scResults.Panel1.ResumeLayout(False)
        CType(Me.m_scResults, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scResults.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_btnRunEcopath As System.Windows.Forms.Button
    Private WithEvents m_tsResults As cEwEToolstrip
    Private WithEvents m_tslItem As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscmbItems As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_btnRunEcosim As System.Windows.Forms.Button
    Private WithEvents m_scResults As System.Windows.Forms.SplitContainer
    Private WithEvents m_tssep1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsbShowFlow As System.Windows.Forms.ToolStripButton
    Private WithEvents m_plFlow As plFlow
    Private WithEvents m_tssep2 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsbEcopath As System.Windows.Forms.ToolStripButton
    Private WithEvents m_btnRunEquilibrium As System.Windows.Forms.Button
    Private WithEvents m_tsbEcosim As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbEquilibrium As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tscmbGraphData As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tslblData As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tssSep3 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tslUnit As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscmbUnit As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tscbYear As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_lblAgg As System.Windows.Forms.Label
    Private WithEvents m_cmbAgg As System.Windows.Forms.ComboBox
    Private WithEvents m_tslbYear As System.Windows.Forms.ToolStripLabel
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsbnSave As System.Windows.Forms.ToolStripButton

End Class
