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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucParameters
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.m_tlpSponsors = New System.Windows.Forms.TableLayoutPanel()
        Me.m_pbLenfest = New System.Windows.Forms.PictureBox()
        Me.m_pbSAUP = New System.Windows.Forms.PictureBox()
        Me.m_pbEcostProject = New System.Windows.Forms.PictureBox()
        Me.m_lblSponsors = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblBaseYear = New System.Windows.Forms.Label()
        Me.m_nudBaseYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_hdrEcosimSettings = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrExecution = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_chkRunWithEcopath = New System.Windows.Forms.CheckBox()
        Me.m_chkRunWithEcosim = New System.Windows.Forms.CheckBox()
        Me.m_chkRunWithSearches = New System.Windows.Forms.CheckBox()
        Me.m_hdrEQ = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_clbFleets = New System.Windows.Forms.CheckedListBox()
        Me.m_lblFleets = New System.Windows.Forms.Label()
        Me.m_lblEffortMin = New System.Windows.Forms.Label()
        Me.m_lblEffortMax = New System.Windows.Forms.Label()
        Me.m_lbEffortIncr = New System.Windows.Forms.Label()
        Me.m_nudEffortMin = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudEffortMax = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudEffortIncr = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_hdrUI = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_cbConfirmDelete = New System.Windows.Forms.CheckBox()
        Me.m_rbAggNone = New System.Windows.Forms.RadioButton()
        Me.m_rbAggFleet = New System.Windows.Forms.RadioButton()
        Me.m_rbAggGroup = New System.Windows.Forms.RadioButton()
        Me.m_lblAgg = New System.Windows.Forms.Label()
        Me.m_cbAutoSave = New System.Windows.Forms.CheckBox()
        Me.m_tlpSponsors.SuspendLayout()
        CType(Me.m_pbLenfest, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbSAUP, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbEcostProject, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudBaseYear, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudEffortMin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudEffortMax, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudEffortIncr, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_tlpSponsors
        '
        Me.m_tlpSponsors.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tlpSponsors.BackColor = System.Drawing.Color.White
        Me.m_tlpSponsors.ColumnCount = 3
        Me.m_tlpSponsors.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.m_tlpSponsors.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.m_tlpSponsors.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.m_tlpSponsors.Controls.Add(Me.m_pbLenfest, 0, 0)
        Me.m_tlpSponsors.Controls.Add(Me.m_pbSAUP, 1, 0)
        Me.m_tlpSponsors.Controls.Add(Me.m_pbEcostProject, 2, 0)
        Me.m_tlpSponsors.Location = New System.Drawing.Point(3, 397)
        Me.m_tlpSponsors.Margin = New System.Windows.Forms.Padding(0)
        Me.m_tlpSponsors.Name = "m_tlpSponsors"
        Me.m_tlpSponsors.RowCount = 1
        Me.m_tlpSponsors.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpSponsors.Size = New System.Drawing.Size(697, 76)
        Me.m_tlpSponsors.TabIndex = 24
        '
        'm_pbLenfest
        '
        Me.m_pbLenfest.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_pbLenfest.Image = Global.EwEValueChainPlugin.My.Resources.Resources.Lenfest_Logo_50px
        Me.m_pbLenfest.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_pbLenfest.Location = New System.Drawing.Point(3, 3)
        Me.m_pbLenfest.Name = "m_pbLenfest"
        Me.m_pbLenfest.Size = New System.Drawing.Size(226, 70)
        Me.m_pbLenfest.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.m_pbLenfest.TabIndex = 0
        Me.m_pbLenfest.TabStop = False
        '
        'm_pbSAUP
        '
        Me.m_pbSAUP.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_pbSAUP.Image = Global.EwEValueChainPlugin.My.Resources.Resources.sautxt_50px
        Me.m_pbSAUP.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_pbSAUP.Location = New System.Drawing.Point(235, 3)
        Me.m_pbSAUP.Name = "m_pbSAUP"
        Me.m_pbSAUP.Size = New System.Drawing.Size(226, 70)
        Me.m_pbSAUP.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.m_pbSAUP.TabIndex = 1
        Me.m_pbSAUP.TabStop = False
        '
        'm_pbEcostProject
        '
        Me.m_pbEcostProject.BackgroundImage = Global.EwEValueChainPlugin.My.Resources.Resources.ecost_256x256
        Me.m_pbEcostProject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.m_pbEcostProject.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_pbEcostProject.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_pbEcostProject.Location = New System.Drawing.Point(467, 3)
        Me.m_pbEcostProject.Name = "m_pbEcostProject"
        Me.m_pbEcostProject.Size = New System.Drawing.Size(227, 70)
        Me.m_pbEcostProject.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.m_pbEcostProject.TabIndex = 2
        Me.m_pbEcostProject.TabStop = False
        '
        'm_lblSponsors
        '
        Me.m_lblSponsors.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_lblSponsors.CanCollapseParent = False
        Me.m_lblSponsors.CollapsedParentHeight = 0
        Me.m_lblSponsors.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblSponsors.IsCollapsed = False
        Me.m_lblSponsors.Location = New System.Drawing.Point(0, 374)
        Me.m_lblSponsors.Name = "m_lblSponsors"
        Me.m_lblSponsors.Size = New System.Drawing.Size(703, 18)
        Me.m_lblSponsors.TabIndex = 23
        Me.m_lblSponsors.Text = "Sponsors"
        Me.m_lblSponsors.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_lblBaseYear
        '
        Me.m_lblBaseYear.AutoSize = True
        Me.m_lblBaseYear.Location = New System.Drawing.Point(3, 169)
        Me.m_lblBaseYear.Name = "m_lblBaseYear"
        Me.m_lblBaseYear.Size = New System.Drawing.Size(57, 13)
        Me.m_lblBaseYear.TabIndex = 10
        Me.m_lblBaseYear.Text = "&Base year:"
        '
        'm_nudBaseYear
        '
        Me.m_nudBaseYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudBaseYear.Location = New System.Drawing.Point(93, 167)
        Me.m_nudBaseYear.Name = "m_nudBaseYear"
        Me.m_nudBaseYear.Size = New System.Drawing.Size(106, 20)
        Me.m_nudBaseYear.TabIndex = 11
        '
        'm_hdrEcosimSettings
        '
        Me.m_hdrEcosimSettings.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrEcosimSettings.CanCollapseParent = False
        Me.m_hdrEcosimSettings.CollapsedParentHeight = 0
        Me.m_hdrEcosimSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_hdrEcosimSettings.IsCollapsed = False
        Me.m_hdrEcosimSettings.Location = New System.Drawing.Point(0, 140)
        Me.m_hdrEcosimSettings.Name = "m_hdrEcosimSettings"
        Me.m_hdrEcosimSettings.Size = New System.Drawing.Size(703, 18)
        Me.m_hdrEcosimSettings.TabIndex = 9
        Me.m_hdrEcosimSettings.Text = "Ecosim-dependent settings"
        Me.m_hdrEcosimSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_hdrExecution
        '
        Me.m_hdrExecution.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrExecution.CanCollapseParent = False
        Me.m_hdrExecution.CollapsedParentHeight = 0
        Me.m_hdrExecution.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_hdrExecution.IsCollapsed = False
        Me.m_hdrExecution.Location = New System.Drawing.Point(0, 0)
        Me.m_hdrExecution.Name = "m_hdrExecution"
        Me.m_hdrExecution.Size = New System.Drawing.Size(703, 18)
        Me.m_hdrExecution.TabIndex = 0
        Me.m_hdrExecution.Text = "Execution"
        Me.m_hdrExecution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_chkRunWithEcopath
        '
        Me.m_chkRunWithEcopath.AutoSize = True
        Me.m_chkRunWithEcopath.Location = New System.Drawing.Point(6, 24)
        Me.m_chkRunWithEcopath.Name = "m_chkRunWithEcopath"
        Me.m_chkRunWithEcopath.Size = New System.Drawing.Size(111, 17)
        Me.m_chkRunWithEcopath.TabIndex = 1
        Me.m_chkRunWithEcopath.Text = "Run with Eco&path"
        Me.m_chkRunWithEcopath.UseVisualStyleBackColor = True
        '
        'm_chkRunWithEcosim
        '
        Me.m_chkRunWithEcosim.AutoSize = True
        Me.m_chkRunWithEcosim.Location = New System.Drawing.Point(6, 47)
        Me.m_chkRunWithEcosim.Name = "m_chkRunWithEcosim"
        Me.m_chkRunWithEcosim.Size = New System.Drawing.Size(105, 17)
        Me.m_chkRunWithEcosim.TabIndex = 2
        Me.m_chkRunWithEcosim.Text = "Run with Eco&sim"
        Me.m_chkRunWithEcosim.UseVisualStyleBackColor = True
        '
        'm_chkRunWithSearches
        '
        Me.m_chkRunWithSearches.AutoSize = True
        Me.m_chkRunWithSearches.Location = New System.Drawing.Point(6, 70)
        Me.m_chkRunWithSearches.Name = "m_chkRunWithSearches"
        Me.m_chkRunWithSearches.Size = New System.Drawing.Size(114, 17)
        Me.m_chkRunWithSearches.TabIndex = 3
        Me.m_chkRunWithSearches.Text = "Run with &searches"
        Me.m_chkRunWithSearches.UseVisualStyleBackColor = True
        '
        'm_hdrEQ
        '
        Me.m_hdrEQ.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrEQ.CanCollapseParent = False
        Me.m_hdrEQ.CollapsedParentHeight = 0
        Me.m_hdrEQ.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_hdrEQ.IsCollapsed = False
        Me.m_hdrEQ.Location = New System.Drawing.Point(0, 199)
        Me.m_hdrEQ.Name = "m_hdrEQ"
        Me.m_hdrEQ.Size = New System.Drawing.Size(703, 18)
        Me.m_hdrEQ.TabIndex = 12
        Me.m_hdrEQ.Text = "Equilibrium search"
        Me.m_hdrEQ.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_clbFleets
        '
        Me.m_clbFleets.CheckOnClick = True
        Me.m_clbFleets.FormattingEnabled = True
        Me.m_clbFleets.IntegralHeight = False
        Me.m_clbFleets.Location = New System.Drawing.Point(356, 225)
        Me.m_clbFleets.Name = "m_clbFleets"
        Me.m_clbFleets.Size = New System.Drawing.Size(137, 72)
        Me.m_clbFleets.TabIndex = 20
        '
        'm_lblFleets
        '
        Me.m_lblFleets.AutoSize = True
        Me.m_lblFleets.Location = New System.Drawing.Point(235, 227)
        Me.m_lblFleets.Name = "m_lblFleets"
        Me.m_lblFleets.Size = New System.Drawing.Size(115, 13)
        Me.m_lblFleets.TabIndex = 19
        Me.m_lblFleets.Text = "&Fleets to vary effort for:"
        '
        'm_lblEffortMin
        '
        Me.m_lblEffortMin.AutoSize = True
        Me.m_lblEffortMin.Location = New System.Drawing.Point(3, 227)
        Me.m_lblEffortMin.Name = "m_lblEffortMin"
        Me.m_lblEffortMin.Size = New System.Drawing.Size(78, 13)
        Me.m_lblEffortMin.TabIndex = 13
        Me.m_lblEffortMin.Text = "M&inimum effort:"
        '
        'm_lblEffortMax
        '
        Me.m_lblEffortMax.AutoSize = True
        Me.m_lblEffortMax.Location = New System.Drawing.Point(3, 253)
        Me.m_lblEffortMax.Name = "m_lblEffortMax"
        Me.m_lblEffortMax.Size = New System.Drawing.Size(78, 13)
        Me.m_lblEffortMax.TabIndex = 15
        Me.m_lblEffortMax.Text = "M&aximum effort"
        '
        'm_lbEffortIncr
        '
        Me.m_lbEffortIncr.AutoSize = True
        Me.m_lbEffortIncr.Location = New System.Drawing.Point(3, 279)
        Me.m_lbEffortIncr.Name = "m_lbEffortIncr"
        Me.m_lbEffortIncr.Size = New System.Drawing.Size(84, 13)
        Me.m_lbEffortIncr.TabIndex = 17
        Me.m_lbEffortIncr.Text = "Effort i&ncrement:"
        '
        'm_nudEffortMin
        '
        Me.m_nudEffortMin.DecimalPlaces = 2
        Me.m_nudEffortMin.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudEffortMin.Location = New System.Drawing.Point(93, 225)
        Me.m_nudEffortMin.Name = "m_nudEffortMin"
        Me.m_nudEffortMin.Size = New System.Drawing.Size(106, 20)
        Me.m_nudEffortMin.TabIndex = 14
        '
        'm_nudEffortMax
        '
        Me.m_nudEffortMax.DecimalPlaces = 2
        Me.m_nudEffortMax.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudEffortMax.Location = New System.Drawing.Point(93, 251)
        Me.m_nudEffortMax.Name = "m_nudEffortMax"
        Me.m_nudEffortMax.Size = New System.Drawing.Size(106, 20)
        Me.m_nudEffortMax.TabIndex = 16
        '
        'm_nudEffortIncr
        '
        Me.m_nudEffortIncr.DecimalPlaces = 2
        Me.m_nudEffortIncr.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudEffortIncr.Location = New System.Drawing.Point(93, 277)
        Me.m_nudEffortIncr.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.m_nudEffortIncr.Name = "m_nudEffortIncr"
        Me.m_nudEffortIncr.Size = New System.Drawing.Size(106, 20)
        Me.m_nudEffortIncr.TabIndex = 18
        Me.m_nudEffortIncr.Value = New Decimal(New Integer() {25, 0, 0, 131072})
        '
        'm_hdrUI
        '
        Me.m_hdrUI.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrUI.CanCollapseParent = False
        Me.m_hdrUI.CollapsedParentHeight = 0
        Me.m_hdrUI.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_hdrUI.IsCollapsed = False
        Me.m_hdrUI.Location = New System.Drawing.Point(0, 311)
        Me.m_hdrUI.Name = "m_hdrUI"
        Me.m_hdrUI.Size = New System.Drawing.Size(703, 18)
        Me.m_hdrUI.TabIndex = 21
        Me.m_hdrUI.Text = "User interface"
        Me.m_hdrUI.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_cbConfirmDelete
        '
        Me.m_cbConfirmDelete.AutoSize = True
        Me.m_cbConfirmDelete.Location = New System.Drawing.Point(6, 336)
        Me.m_cbConfirmDelete.Name = "m_cbConfirmDelete"
        Me.m_cbConfirmDelete.Size = New System.Drawing.Size(181, 17)
        Me.m_cbConfirmDelete.TabIndex = 22
        Me.m_cbConfirmDelete.Text = "&Prompt for confirmation on delete"
        Me.m_cbConfirmDelete.UseVisualStyleBackColor = True
        '
        'm_rbAggNone
        '
        Me.m_rbAggNone.AutoSize = True
        Me.m_rbAggNone.Location = New System.Drawing.Point(356, 23)
        Me.m_rbAggNone.Name = "m_rbAggNone"
        Me.m_rbAggNone.Size = New System.Drawing.Size(133, 17)
        Me.m_rbAggNone.TabIndex = 5
        Me.m_rbAggNone.TabStop = True
        Me.m_rbAggNone.Text = "&Unaggregated (fastest)"
        Me.m_rbAggNone.UseVisualStyleBackColor = True
        '
        'm_rbAggFleet
        '
        Me.m_rbAggFleet.AutoSize = True
        Me.m_rbAggFleet.Location = New System.Drawing.Point(356, 46)
        Me.m_rbAggFleet.Name = "m_rbAggFleet"
        Me.m_rbAggFleet.Size = New System.Drawing.Size(97, 17)
        Me.m_rbAggFleet.TabIndex = 6
        Me.m_rbAggFleet.TabStop = True
        Me.m_rbAggFleet.Text = "Results by &fleet"
        Me.m_rbAggFleet.UseVisualStyleBackColor = True
        '
        'm_rbAggGroup
        '
        Me.m_rbAggGroup.AutoSize = True
        Me.m_rbAggGroup.Location = New System.Drawing.Point(356, 69)
        Me.m_rbAggGroup.Name = "m_rbAggGroup"
        Me.m_rbAggGroup.Size = New System.Drawing.Size(104, 17)
        Me.m_rbAggGroup.TabIndex = 7
        Me.m_rbAggGroup.TabStop = True
        Me.m_rbAggGroup.Text = "Results by &group"
        Me.m_rbAggGroup.UseVisualStyleBackColor = True
        '
        'm_lblAgg
        '
        Me.m_lblAgg.AutoSize = True
        Me.m_lblAgg.Location = New System.Drawing.Point(235, 25)
        Me.m_lblAgg.Name = "m_lblAgg"
        Me.m_lblAgg.Size = New System.Drawing.Size(92, 13)
        Me.m_lblAgg.TabIndex = 4
        Me.m_lblAgg.Text = "&Data aggregation:"
        '
        'm_cbAutoSave
        '
        Me.m_cbAutoSave.AutoSize = True
        Me.m_cbAutoSave.Location = New System.Drawing.Point(6, 104)
        Me.m_cbAutoSave.Name = "m_cbAutoSave"
        Me.m_cbAutoSave.Size = New System.Drawing.Size(199, 17)
        Me.m_cbAutoSave.TabIndex = 8
        Me.m_cbAutoSave.Text = "&Automatically save results to CSV file"
        Me.m_cbAutoSave.UseVisualStyleBackColor = True
        '
        'ucParameters
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.m_rbAggGroup)
        Me.Controls.Add(Me.m_rbAggFleet)
        Me.Controls.Add(Me.m_rbAggNone)
        Me.Controls.Add(Me.m_cbConfirmDelete)
        Me.Controls.Add(Me.m_nudEffortIncr)
        Me.Controls.Add(Me.m_nudEffortMax)
        Me.Controls.Add(Me.m_nudEffortMin)
        Me.Controls.Add(Me.m_lbEffortIncr)
        Me.Controls.Add(Me.m_lblEffortMax)
        Me.Controls.Add(Me.m_lblEffortMin)
        Me.Controls.Add(Me.m_lblAgg)
        Me.Controls.Add(Me.m_lblFleets)
        Me.Controls.Add(Me.m_clbFleets)
        Me.Controls.Add(Me.m_cbAutoSave)
        Me.Controls.Add(Me.m_chkRunWithSearches)
        Me.Controls.Add(Me.m_chkRunWithEcosim)
        Me.Controls.Add(Me.m_chkRunWithEcopath)
        Me.Controls.Add(Me.m_tlpSponsors)
        Me.Controls.Add(Me.m_nudBaseYear)
        Me.Controls.Add(Me.m_lblBaseYear)
        Me.Controls.Add(Me.m_hdrUI)
        Me.Controls.Add(Me.m_hdrEQ)
        Me.Controls.Add(Me.m_hdrExecution)
        Me.Controls.Add(Me.m_hdrEcosimSettings)
        Me.Controls.Add(Me.m_lblSponsors)
        Me.MinimumSize = New System.Drawing.Size(400, 400)
        Me.Name = "ucParameters"
        Me.Size = New System.Drawing.Size(703, 476)
        Me.m_tlpSponsors.ResumeLayout(False)
        CType(Me.m_pbLenfest, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbSAUP, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbEcostProject, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudBaseYear, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudEffortMin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudEffortMax, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudEffortIncr, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_pbSAUP As System.Windows.Forms.PictureBox
    Private WithEvents m_pbEcostProject As System.Windows.Forms.PictureBox
    Private WithEvents m_lblBaseYear As System.Windows.Forms.Label
    Private WithEvents m_hdrEcosimSettings As cEwEHeaderLabel
    Private WithEvents m_hdrExecution As cEwEHeaderLabel
    Private WithEvents m_chkRunWithEcopath As System.Windows.Forms.CheckBox
    Private WithEvents m_chkRunWithEcosim As System.Windows.Forms.CheckBox
    Private WithEvents m_chkRunWithSearches As System.Windows.Forms.CheckBox
    Private WithEvents m_tlpSponsors As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_lblSponsors As cEwEHeaderLabel
    Private WithEvents m_hdrEQ As cEwEHeaderLabel
    Private WithEvents m_clbFleets As System.Windows.Forms.CheckedListBox
    Private WithEvents m_lblFleets As System.Windows.Forms.Label
    Private WithEvents m_lblEffortMin As System.Windows.Forms.Label
    Private WithEvents m_lbEffortIncr As System.Windows.Forms.Label
    Private WithEvents m_lblEffortMax As System.Windows.Forms.Label
    Private WithEvents m_hdrUI As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_cbConfirmDelete As System.Windows.Forms.CheckBox
    Private WithEvents m_nudBaseYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudEffortMin As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudEffortMax As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudEffortIncr As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_rbAggNone As System.Windows.Forms.RadioButton
    Private WithEvents m_rbAggFleet As System.Windows.Forms.RadioButton
    Private WithEvents m_rbAggGroup As System.Windows.Forms.RadioButton
    Private WithEvents m_lblAgg As System.Windows.Forms.Label
    Private WithEvents m_cbAutoSave As System.Windows.Forms.CheckBox
    Private WithEvents m_pbLenfest As System.Windows.Forms.PictureBox

End Class
