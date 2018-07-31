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

Namespace Ecosim

    Partial Class frmFishingPolicySearch
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
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.m_scTopBits = New System.Windows.Forms.SplitContainer()
            Me.m_scTop = New System.Windows.Forms.SplitContainer()
            Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plRunParams = New System.Windows.Forms.Panel()
            Me.m_lbGenDiscount = New System.Windows.Forms.Label()
            Me.m_txtGenDiscount = New System.Windows.Forms.TextBox()
            Me.m_chkIncludeCCosts = New System.Windows.Forms.CheckBox()
            Me.m_chkMaxPortUl = New System.Windows.Forms.CheckBox()
            Me.m_chkUsePlugin = New System.Windows.Forms.CheckBox()
            Me.m_chkPrevCE = New System.Windows.Forms.CheckBox()
            Me.m_lblDiscRate = New System.Windows.Forms.Label()
            Me.m_txtDiscountRate = New System.Windows.Forms.TextBox()
            Me.m_nudMaxEffChg = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudBaseYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblBaseYear = New System.Windows.Forms.Label()
            Me.m_cmbOptmApproach = New System.Windows.Forms.ComboBox()
            Me.m_lblMaxEffChg = New System.Windows.Forms.Label()
            Me.m_lblOptmApproach = New System.Windows.Forms.Label()
            Me.m_cmbSearchUsing = New System.Windows.Forms.ComboBox()
            Me.m_lblSearchUsing = New System.Windows.Forms.Label()
            Me.m_cmbInitUsing = New System.Windows.Forms.ComboBox()
            Me.m_lblInitUsing = New System.Windows.Forms.Label()
            Me.m_nudMaxNumEval = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudNumberOfRuns = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblMaxNumEval = New System.Windows.Forms.Label()
            Me.m_lblNumOfRuns = New System.Windows.Forms.Label()
            Me.m_hdrParms = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpRunStop = New System.Windows.Forms.TableLayoutPanel()
            Me.btnStop = New System.Windows.Forms.Button()
            Me.btnSearch = New System.Windows.Forms.Button()
            Me.m_blocks = New ScientificInterface.Ecosim.ucPolicyColorBlocks()
            Me.m_tcMain = New System.Windows.Forms.TabControl()
            Me.m_tabObjectives = New System.Windows.Forms.TabPage()
            Me.m_scObjectives = New System.Windows.Forms.SplitContainer()
            Me.m_gridObjWeights = New ScientificInterface.Ecosim.gridSearchObjectivesWeight()
            Me.m_scAarghArghAaargh = New System.Windows.Forms.SplitContainer()
            Me.m_gridObjFleet = New ScientificInterface.Ecosim.gridSearchObjectivesFleet()
            Me.m_gridObjGroup = New ScientificInterface.Ecosim.gridSearchObjectivesGroup()
            Me.m_tabResultTable = New System.Windows.Forms.TabPage()
            Me.m_scIterResult = New System.Windows.Forms.SplitContainer()
            Me.m_scIterResultMultiRun = New System.Windows.Forms.SplitContainer()
            Me.m_tpPlots = New System.Windows.Forms.TabPage()
            Me.m_graphResults = New ZedGraph.ZedGraphControl()
            CType(Me.m_scTopBits, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scTopBits.Panel1.SuspendLayout()
            Me.m_scTopBits.Panel2.SuspendLayout()
            Me.m_scTopBits.SuspendLayout()
            CType(Me.m_scTop, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scTop.Panel1.SuspendLayout()
            Me.m_scTop.Panel2.SuspendLayout()
            Me.m_scTop.SuspendLayout()
            Me.m_tlpControls.SuspendLayout()
            Me.m_plRunParams.SuspendLayout()
            CType(Me.m_nudMaxEffChg, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudBaseYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudMaxNumEval, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNumberOfRuns, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tlpRunStop.SuspendLayout()
            Me.m_tcMain.SuspendLayout()
            Me.m_tabObjectives.SuspendLayout()
            CType(Me.m_scObjectives, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scObjectives.Panel1.SuspendLayout()
            Me.m_scObjectives.Panel2.SuspendLayout()
            Me.m_scObjectives.SuspendLayout()
            CType(Me.m_scAarghArghAaargh, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scAarghArghAaargh.Panel1.SuspendLayout()
            Me.m_scAarghArghAaargh.Panel2.SuspendLayout()
            Me.m_scAarghArghAaargh.SuspendLayout()
            Me.m_tabResultTable.SuspendLayout()
            CType(Me.m_scIterResult, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scIterResult.Panel1.SuspendLayout()
            Me.m_scIterResult.SuspendLayout()
            CType(Me.m_scIterResultMultiRun, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scIterResultMultiRun.SuspendLayout()
            Me.m_tpPlots.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_scTopBits
            '
            Me.m_scTopBits.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_scTopBits.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scTopBits.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
            Me.m_scTopBits.Location = New System.Drawing.Point(0, 0)
            Me.m_scTopBits.Margin = New System.Windows.Forms.Padding(0)
            Me.m_scTopBits.Name = "m_scTopBits"
            Me.m_scTopBits.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_scTopBits.Panel1
            '
            Me.m_scTopBits.Panel1.Controls.Add(Me.m_scTop)
            Me.m_scTopBits.Panel1MinSize = 338
            '
            'm_scTopBits.Panel2
            '
            Me.m_scTopBits.Panel2.Controls.Add(Me.m_tcMain)
            Me.m_scTopBits.Size = New System.Drawing.Size(993, 687)
            Me.m_scTopBits.SplitterDistance = 386
            Me.m_scTopBits.TabIndex = 0
            '
            'm_scTop
            '
            Me.m_scTop.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scTop.Location = New System.Drawing.Point(0, 0)
            Me.m_scTop.Margin = New System.Windows.Forms.Padding(0)
            Me.m_scTop.Name = "m_scTop"
            '
            'm_scTop.Panel1
            '
            Me.m_scTop.Panel1.Controls.Add(Me.m_tlpControls)
            Me.m_scTop.Panel1MinSize = 200
            '
            'm_scTop.Panel2
            '
            Me.m_scTop.Panel2.Controls.Add(Me.m_blocks)
            Me.m_scTop.Size = New System.Drawing.Size(989, 382)
            Me.m_scTop.SplitterDistance = 250
            Me.m_scTop.TabIndex = 3
            '
            'm_tlpControls
            '
            Me.m_tlpControls.ColumnCount = 1
            Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpControls.Controls.Add(Me.m_plRunParams, 0, 0)
            Me.m_tlpControls.Controls.Add(Me.m_tlpRunStop, 0, 1)
            Me.m_tlpControls.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tlpControls.Location = New System.Drawing.Point(0, 0)
            Me.m_tlpControls.Name = "m_tlpControls"
            Me.m_tlpControls.RowCount = 2
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpControls.Size = New System.Drawing.Size(250, 382)
            Me.m_tlpControls.TabIndex = 1
            '
            'm_plRunParams
            '
            Me.m_plRunParams.AutoScroll = True
            Me.m_plRunParams.Controls.Add(Me.m_lbGenDiscount)
            Me.m_plRunParams.Controls.Add(Me.m_txtGenDiscount)
            Me.m_plRunParams.Controls.Add(Me.m_chkIncludeCCosts)
            Me.m_plRunParams.Controls.Add(Me.m_chkMaxPortUl)
            Me.m_plRunParams.Controls.Add(Me.m_chkUsePlugin)
            Me.m_plRunParams.Controls.Add(Me.m_chkPrevCE)
            Me.m_plRunParams.Controls.Add(Me.m_lblDiscRate)
            Me.m_plRunParams.Controls.Add(Me.m_txtDiscountRate)
            Me.m_plRunParams.Controls.Add(Me.m_nudMaxEffChg)
            Me.m_plRunParams.Controls.Add(Me.m_nudBaseYear)
            Me.m_plRunParams.Controls.Add(Me.m_lblBaseYear)
            Me.m_plRunParams.Controls.Add(Me.m_cmbOptmApproach)
            Me.m_plRunParams.Controls.Add(Me.m_lblMaxEffChg)
            Me.m_plRunParams.Controls.Add(Me.m_lblOptmApproach)
            Me.m_plRunParams.Controls.Add(Me.m_cmbSearchUsing)
            Me.m_plRunParams.Controls.Add(Me.m_lblSearchUsing)
            Me.m_plRunParams.Controls.Add(Me.m_cmbInitUsing)
            Me.m_plRunParams.Controls.Add(Me.m_lblInitUsing)
            Me.m_plRunParams.Controls.Add(Me.m_nudMaxNumEval)
            Me.m_plRunParams.Controls.Add(Me.m_nudNumberOfRuns)
            Me.m_plRunParams.Controls.Add(Me.m_lblMaxNumEval)
            Me.m_plRunParams.Controls.Add(Me.m_lblNumOfRuns)
            Me.m_plRunParams.Controls.Add(Me.m_hdrParms)
            Me.m_plRunParams.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_plRunParams.Location = New System.Drawing.Point(0, 0)
            Me.m_plRunParams.Margin = New System.Windows.Forms.Padding(0)
            Me.m_plRunParams.Name = "m_plRunParams"
            Me.m_plRunParams.Size = New System.Drawing.Size(250, 359)
            Me.m_plRunParams.TabIndex = 0
            '
            'm_lbGenDiscount
            '
            Me.m_lbGenDiscount.AutoSize = True
            Me.m_lbGenDiscount.Location = New System.Drawing.Point(5, 52)
            Me.m_lbGenDiscount.Name = "m_lbGenDiscount"
            Me.m_lbGenDiscount.Size = New System.Drawing.Size(79, 13)
            Me.m_lbGenDiscount.TabIndex = 22
            Me.m_lbGenDiscount.Text = "&Gen. disc. rate:"
            '
            'm_txtGenDiscount
            '
            Me.m_txtGenDiscount.Location = New System.Drawing.Point(121, 48)
            Me.m_txtGenDiscount.Name = "m_txtGenDiscount"
            Me.m_txtGenDiscount.Size = New System.Drawing.Size(60, 20)
            Me.m_txtGenDiscount.TabIndex = 23
            '
            'm_chkIncludeCCosts
            '
            Me.m_chkIncludeCCosts.AutoSize = True
            Me.m_chkIncludeCCosts.Location = New System.Drawing.Point(7, 333)
            Me.m_chkIncludeCCosts.Name = "m_chkIncludeCCosts"
            Me.m_chkIncludeCCosts.Size = New System.Drawing.Size(133, 17)
            Me.m_chkIncludeCCosts.TabIndex = 21
            Me.m_chkIncludeCCosts.Text = "Include &compete costs"
            Me.m_chkIncludeCCosts.UseVisualStyleBackColor = True
            '
            'm_chkMaxPortUl
            '
            Me.m_chkMaxPortUl.AutoSize = True
            Me.m_chkMaxPortUl.Location = New System.Drawing.Point(7, 310)
            Me.m_chkMaxPortUl.Name = "m_chkMaxPortUl"
            Me.m_chkMaxPortUl.Size = New System.Drawing.Size(135, 17)
            Me.m_chkMaxPortUl.TabIndex = 20
            Me.m_chkMaxPortUl.Text = "Maximize portfolio &utility"
            Me.m_chkMaxPortUl.UseVisualStyleBackColor = True
            '
            'm_chkUsePlugin
            '
            Me.m_chkUsePlugin.AutoSize = True
            Me.m_chkUsePlugin.Location = New System.Drawing.Point(6, 265)
            Me.m_chkUsePlugin.Name = "m_chkUsePlugin"
            Me.m_chkUsePlugin.Size = New System.Drawing.Size(152, 17)
            Me.m_chkUsePlugin.TabIndex = 18
            Me.m_chkUsePlugin.Text = "Use p&lug-in economic data"
            Me.m_chkUsePlugin.UseVisualStyleBackColor = True
            '
            'm_chkPrevCE
            '
            Me.m_chkPrevCE.AutoSize = True
            Me.m_chkPrevCE.Location = New System.Drawing.Point(7, 287)
            Me.m_chkPrevCE.Name = "m_chkPrevCE"
            Me.m_chkPrevCE.Size = New System.Drawing.Size(138, 17)
            Me.m_chkPrevCE.TabIndex = 19
            Me.m_chkPrevCE.Text = "&Prevent cost > earnings"
            Me.m_chkPrevCE.UseVisualStyleBackColor = True
            '
            'm_lblDiscRate
            '
            Me.m_lblDiscRate.AutoSize = True
            Me.m_lblDiscRate.Location = New System.Drawing.Point(5, 26)
            Me.m_lblDiscRate.Name = "m_lblDiscRate"
            Me.m_lblDiscRate.Size = New System.Drawing.Size(73, 13)
            Me.m_lblDiscRate.TabIndex = 0
            Me.m_lblDiscRate.Text = "&Discount rate:"
            '
            'm_txtDiscountRate
            '
            Me.m_txtDiscountRate.Location = New System.Drawing.Point(121, 22)
            Me.m_txtDiscountRate.Name = "m_txtDiscountRate"
            Me.m_txtDiscountRate.Size = New System.Drawing.Size(60, 20)
            Me.m_txtDiscountRate.TabIndex = 1
            '
            'm_nudMaxEffChg
            '
            Me.m_nudMaxEffChg.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudMaxEffChg.Location = New System.Drawing.Point(121, 152)
            Me.m_nudMaxEffChg.Name = "m_nudMaxEffChg"
            Me.m_nudMaxEffChg.Size = New System.Drawing.Size(60, 20)
            Me.m_nudMaxEffChg.TabIndex = 11
            '
            'm_nudBaseYear
            '
            Me.m_nudBaseYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudBaseYear.Location = New System.Drawing.Point(121, 126)
            Me.m_nudBaseYear.Maximum = New Decimal(New Integer() {9999, 0, 0, 0})
            Me.m_nudBaseYear.Name = "m_nudBaseYear"
            Me.m_nudBaseYear.Size = New System.Drawing.Size(60, 20)
            Me.m_nudBaseYear.TabIndex = 9
            '
            'm_lblBaseYear
            '
            Me.m_lblBaseYear.AutoSize = True
            Me.m_lblBaseYear.Location = New System.Drawing.Point(5, 130)
            Me.m_lblBaseYear.Name = "m_lblBaseYear"
            Me.m_lblBaseYear.Size = New System.Drawing.Size(57, 13)
            Me.m_lblBaseYear.TabIndex = 8
            Me.m_lblBaseYear.Text = "&Base year:"
            '
            'm_cmbOptmApproach
            '
            Me.m_cmbOptmApproach.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbOptmApproach.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbOptmApproach.FormattingEnabled = True
            Me.m_cmbOptmApproach.Items.AddRange(New Object() {"Maximize system objective", "Maximize by fleet values"})
            Me.m_cmbOptmApproach.Location = New System.Drawing.Point(121, 232)
            Me.m_cmbOptmApproach.Name = "m_cmbOptmApproach"
            Me.m_cmbOptmApproach.Size = New System.Drawing.Size(125, 21)
            Me.m_cmbOptmApproach.TabIndex = 17
            '
            'm_lblMaxEffChg
            '
            Me.m_lblMaxEffChg.AutoSize = True
            Me.m_lblMaxEffChg.Location = New System.Drawing.Point(5, 156)
            Me.m_lblMaxEffChg.Name = "m_lblMaxEffChg"
            Me.m_lblMaxEffChg.Size = New System.Drawing.Size(96, 13)
            Me.m_lblMaxEffChg.TabIndex = 10
            Me.m_lblMaxEffChg.Text = "Max e&ffort change:"
            '
            'm_lblOptmApproach
            '
            Me.m_lblOptmApproach.Location = New System.Drawing.Point(5, 226)
            Me.m_lblOptmApproach.Name = "m_lblOptmApproach"
            Me.m_lblOptmApproach.Size = New System.Drawing.Size(105, 32)
            Me.m_lblOptmApproach.TabIndex = 16
            Me.m_lblOptmApproach.Text = "&Optimization approach:"
            '
            'm_cmbSearchUsing
            '
            Me.m_cmbSearchUsing.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbSearchUsing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbSearchUsing.FormattingEnabled = True
            Me.m_cmbSearchUsing.Items.AddRange(New Object() {"Fletch", "DFPmin"})
            Me.m_cmbSearchUsing.Location = New System.Drawing.Point(121, 205)
            Me.m_cmbSearchUsing.Name = "m_cmbSearchUsing"
            Me.m_cmbSearchUsing.Size = New System.Drawing.Size(125, 21)
            Me.m_cmbSearchUsing.TabIndex = 15
            '
            'm_lblSearchUsing
            '
            Me.m_lblSearchUsing.AutoSize = True
            Me.m_lblSearchUsing.Location = New System.Drawing.Point(5, 209)
            Me.m_lblSearchUsing.Name = "m_lblSearchUsing"
            Me.m_lblSearchUsing.Size = New System.Drawing.Size(72, 13)
            Me.m_lblSearchUsing.TabIndex = 14
            Me.m_lblSearchUsing.Text = "&Search using:"
            '
            'm_cmbInitUsing
            '
            Me.m_cmbInitUsing.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbInitUsing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbInitUsing.FormattingEnabled = True
            Me.m_cmbInitUsing.Items.AddRange(New Object() {"Ecopath base F's", "Current F's", "Random F's"})
            Me.m_cmbInitUsing.Location = New System.Drawing.Point(121, 178)
            Me.m_cmbInitUsing.Name = "m_cmbInitUsing"
            Me.m_cmbInitUsing.Size = New System.Drawing.Size(125, 21)
            Me.m_cmbInitUsing.TabIndex = 13
            '
            'm_lblInitUsing
            '
            Me.m_lblInitUsing.AutoSize = True
            Me.m_lblInitUsing.Location = New System.Drawing.Point(5, 182)
            Me.m_lblInitUsing.Name = "m_lblInitUsing"
            Me.m_lblInitUsing.Size = New System.Drawing.Size(75, 13)
            Me.m_lblInitUsing.TabIndex = 12
            Me.m_lblInitUsing.Text = "&Initialize using:"
            '
            'm_nudMaxNumEval
            '
            Me.m_nudMaxNumEval.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudMaxNumEval.Location = New System.Drawing.Point(121, 100)
            Me.m_nudMaxNumEval.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
            Me.m_nudMaxNumEval.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudMaxNumEval.Name = "m_nudMaxNumEval"
            Me.m_nudMaxNumEval.Size = New System.Drawing.Size(60, 20)
            Me.m_nudMaxNumEval.TabIndex = 7
            Me.m_nudMaxNumEval.Value = New Decimal(New Integer() {2000, 0, 0, 0})
            '
            'm_nudNumberOfRuns
            '
            Me.m_nudNumberOfRuns.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudNumberOfRuns.Location = New System.Drawing.Point(121, 74)
            Me.m_nudNumberOfRuns.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
            Me.m_nudNumberOfRuns.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudNumberOfRuns.Name = "m_nudNumberOfRuns"
            Me.m_nudNumberOfRuns.Size = New System.Drawing.Size(60, 20)
            Me.m_nudNumberOfRuns.TabIndex = 5
            Me.m_nudNumberOfRuns.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_lblMaxNumEval
            '
            Me.m_lblMaxNumEval.AutoSize = True
            Me.m_lblMaxNumEval.Location = New System.Drawing.Point(5, 104)
            Me.m_lblMaxNumEval.Name = "m_lblMaxNumEval"
            Me.m_lblMaxNumEval.Size = New System.Drawing.Size(85, 13)
            Me.m_lblMaxNumEval.TabIndex = 6
            Me.m_lblMaxNumEval.Text = "Max no of &evals:"
            '
            'm_lblNumOfRuns
            '
            Me.m_lblNumOfRuns.AutoSize = True
            Me.m_lblNumOfRuns.Location = New System.Drawing.Point(5, 78)
            Me.m_lblNumOfRuns.Name = "m_lblNumOfRuns"
            Me.m_lblNumOfRuns.Size = New System.Drawing.Size(82, 13)
            Me.m_lblNumOfRuns.TabIndex = 4
            Me.m_lblNumOfRuns.Text = "&Number of runs:"
            '
            'm_hdrParms
            '
            Me.m_hdrParms.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrParms.CanCollapseParent = False
            Me.m_hdrParms.CollapsedParentHeight = 0
            Me.m_hdrParms.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrParms.IsCollapsed = False
            Me.m_hdrParms.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrParms.Name = "m_hdrParms"
            Me.m_hdrParms.Size = New System.Drawing.Size(250, 18)
            Me.m_hdrParms.TabIndex = 0
            Me.m_hdrParms.Text = "Parameters"
            Me.m_hdrParms.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_tlpRunStop
            '
            Me.m_tlpRunStop.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_tlpRunStop.ColumnCount = 5
            Me.m_tlpRunStop.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpRunStop.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75.0!))
            Me.m_tlpRunStop.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 3.0!))
            Me.m_tlpRunStop.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75.0!))
            Me.m_tlpRunStop.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpRunStop.Controls.Add(Me.btnStop, 3, 0)
            Me.m_tlpRunStop.Controls.Add(Me.btnSearch, 1, 0)
            Me.m_tlpRunStop.Location = New System.Drawing.Point(0, 359)
            Me.m_tlpRunStop.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tlpRunStop.Name = "m_tlpRunStop"
            Me.m_tlpRunStop.RowCount = 1
            Me.m_tlpRunStop.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpRunStop.Size = New System.Drawing.Size(250, 23)
            Me.m_tlpRunStop.TabIndex = 0
            '
            'btnStop
            '
            Me.btnStop.Location = New System.Drawing.Point(126, 0)
            Me.btnStop.Margin = New System.Windows.Forms.Padding(0)
            Me.btnStop.Name = "btnStop"
            Me.btnStop.Size = New System.Drawing.Size(75, 23)
            Me.btnStop.TabIndex = 1
            Me.btnStop.Text = "Sto&p"
            Me.btnStop.UseVisualStyleBackColor = True
            '
            'btnSearch
            '
            Me.btnSearch.Location = New System.Drawing.Point(48, 0)
            Me.btnSearch.Margin = New System.Windows.Forms.Padding(0)
            Me.btnSearch.Name = "btnSearch"
            Me.btnSearch.Size = New System.Drawing.Size(75, 23)
            Me.btnSearch.TabIndex = 0
            Me.btnSearch.Text = "&Search"
            Me.btnSearch.UseVisualStyleBackColor = True
            '
            'm_blocks
            '
            Me.m_blocks.ControlPanelVisible = True
            Me.m_blocks.CurColor = System.Drawing.Color.Empty
            Me.m_blocks.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_blocks.Location = New System.Drawing.Point(0, 0)
            Me.m_blocks.Margin = New System.Windows.Forms.Padding(0)
            Me.m_blocks.Name = "m_blocks"
            Me.m_blocks.ParmBlockCodes = Nothing
            Me.m_blocks.ShowTooltip = True
            Me.m_blocks.Size = New System.Drawing.Size(735, 382)
            Me.m_blocks.TabIndex = 3
            Me.m_blocks.UIContext = Nothing
            '
            'm_tcMain
            '
            Me.m_tcMain.Controls.Add(Me.m_tabObjectives)
            Me.m_tcMain.Controls.Add(Me.m_tabResultTable)
            Me.m_tcMain.Controls.Add(Me.m_tpPlots)
            Me.m_tcMain.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tcMain.Location = New System.Drawing.Point(0, 0)
            Me.m_tcMain.Name = "m_tcMain"
            Me.m_tcMain.SelectedIndex = 0
            Me.m_tcMain.Size = New System.Drawing.Size(989, 293)
            Me.m_tcMain.TabIndex = 0
            '
            'm_tabObjectives
            '
            Me.m_tabObjectives.Controls.Add(Me.m_scObjectives)
            Me.m_tabObjectives.Location = New System.Drawing.Point(4, 22)
            Me.m_tabObjectives.Name = "m_tabObjectives"
            Me.m_tabObjectives.Padding = New System.Windows.Forms.Padding(3)
            Me.m_tabObjectives.Size = New System.Drawing.Size(981, 267)
            Me.m_tabObjectives.TabIndex = 0
            Me.m_tabObjectives.Text = "Objectives"
            Me.m_tabObjectives.UseVisualStyleBackColor = True
            '
            'm_scObjectives
            '
            Me.m_scObjectives.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scObjectives.Location = New System.Drawing.Point(3, 3)
            Me.m_scObjectives.Name = "m_scObjectives"
            '
            'm_scObjectives.Panel1
            '
            Me.m_scObjectives.Panel1.Controls.Add(Me.m_gridObjWeights)
            '
            'm_scObjectives.Panel2
            '
            Me.m_scObjectives.Panel2.Controls.Add(Me.m_scAarghArghAaargh)
            Me.m_scObjectives.Size = New System.Drawing.Size(975, 261)
            Me.m_scObjectives.SplitterDistance = 319
            Me.m_scObjectives.TabIndex = 0
            '
            'm_gridObjWeights
            '
            Me.m_gridObjWeights.AllowBlockSelect = True
            Me.m_gridObjWeights.AutoSizeMinHeight = 10
            Me.m_gridObjWeights.AutoSizeMinWidth = 10
            Me.m_gridObjWeights.AutoStretchColumnsToFitWidth = False
            Me.m_gridObjWeights.AutoStretchRowsToFitHeight = False
            Me.m_gridObjWeights.BackColor = System.Drawing.Color.White
            Me.m_gridObjWeights.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridObjWeights.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridObjWeights.CustomSort = False
            Me.m_gridObjWeights.DataName = "grid content"
            Me.m_gridObjWeights.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_gridObjWeights.FixedColumnWidths = False
            Me.m_gridObjWeights.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridObjWeights.GridToolTipActive = True
            Me.m_gridObjWeights.IsLayoutSuspended = False
            Me.m_gridObjWeights.IsOutputGrid = True
            Me.m_gridObjWeights.Location = New System.Drawing.Point(0, 0)
            Me.m_gridObjWeights.Manager = Nothing
            Me.m_gridObjWeights.Name = "m_gridObjWeights"
            Me.m_gridObjWeights.ShowMaxPortUtil = False
            Me.m_gridObjWeights.ShowMPAOptParams = False
            Me.m_gridObjWeights.Size = New System.Drawing.Size(319, 261)
            Me.m_gridObjWeights.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridObjWeights.TabIndex = 0
            Me.m_gridObjWeights.UIContext = Nothing
            '
            'm_scAarghArghAaargh
            '
            Me.m_scAarghArghAaargh.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scAarghArghAaargh.Location = New System.Drawing.Point(0, 0)
            Me.m_scAarghArghAaargh.Name = "m_scAarghArghAaargh"
            '
            'm_scAarghArghAaargh.Panel1
            '
            Me.m_scAarghArghAaargh.Panel1.Controls.Add(Me.m_gridObjFleet)
            '
            'm_scAarghArghAaargh.Panel2
            '
            Me.m_scAarghArghAaargh.Panel2.Controls.Add(Me.m_gridObjGroup)
            Me.m_scAarghArghAaargh.Size = New System.Drawing.Size(652, 261)
            Me.m_scAarghArghAaargh.SplitterDistance = 212
            Me.m_scAarghArghAaargh.TabIndex = 0
            '
            'm_gridObjFleet
            '
            Me.m_gridObjFleet.AllowBlockSelect = True
            Me.m_gridObjFleet.AutoSizeMinHeight = 10
            Me.m_gridObjFleet.AutoSizeMinWidth = 10
            Me.m_gridObjFleet.AutoStretchColumnsToFitWidth = False
            Me.m_gridObjFleet.AutoStretchRowsToFitHeight = False
            Me.m_gridObjFleet.BackColor = System.Drawing.Color.White
            Me.m_gridObjFleet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridObjFleet.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridObjFleet.CustomSort = False
            Me.m_gridObjFleet.DataName = "grid content"
            Me.m_gridObjFleet.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_gridObjFleet.FixedColumnWidths = False
            Me.m_gridObjFleet.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridObjFleet.GridToolTipActive = True
            Me.m_gridObjFleet.IsLayoutSuspended = False
            Me.m_gridObjFleet.IsMaximizeByFleetValue = False
            Me.m_gridObjFleet.IsOutputGrid = True
            Me.m_gridObjFleet.Location = New System.Drawing.Point(0, 0)
            Me.m_gridObjFleet.Manager = Nothing
            Me.m_gridObjFleet.Name = "m_gridObjFleet"
            Me.m_gridObjFleet.Size = New System.Drawing.Size(212, 261)
            Me.m_gridObjFleet.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridObjFleet.TabIndex = 0
            Me.m_gridObjFleet.UIContext = Nothing
            '
            'm_gridObjGroup
            '
            Me.m_gridObjGroup.AllowBlockSelect = True
            Me.m_gridObjGroup.AutoSizeMinHeight = 10
            Me.m_gridObjGroup.AutoSizeMinWidth = 10
            Me.m_gridObjGroup.AutoStretchColumnsToFitWidth = False
            Me.m_gridObjGroup.AutoStretchRowsToFitHeight = False
            Me.m_gridObjGroup.BackColor = System.Drawing.Color.White
            Me.m_gridObjGroup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridObjGroup.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridObjGroup.CustomSort = False
            Me.m_gridObjGroup.DataName = "grid content"
            Me.m_gridObjGroup.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_gridObjGroup.FixedColumnWidths = False
            Me.m_gridObjGroup.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridObjGroup.GridToolTipActive = True
            Me.m_gridObjGroup.IsLayoutSuspended = False
            Me.m_gridObjGroup.IsOutputGrid = True
            Me.m_gridObjGroup.Location = New System.Drawing.Point(0, 0)
            Me.m_gridObjGroup.Manager = Nothing
            Me.m_gridObjGroup.Name = "m_gridObjGroup"
            Me.m_gridObjGroup.Size = New System.Drawing.Size(436, 261)
            Me.m_gridObjGroup.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridObjGroup.TabIndex = 0
            Me.m_gridObjGroup.UIContext = Nothing
            '
            'm_tabResultTable
            '
            Me.m_tabResultTable.Controls.Add(Me.m_scIterResult)
            Me.m_tabResultTable.Location = New System.Drawing.Point(4, 22)
            Me.m_tabResultTable.Name = "m_tabResultTable"
            Me.m_tabResultTable.Padding = New System.Windows.Forms.Padding(3)
            Me.m_tabResultTable.Size = New System.Drawing.Size(981, 267)
            Me.m_tabResultTable.TabIndex = 1
            Me.m_tabResultTable.Text = "Iteration results"
            Me.m_tabResultTable.UseVisualStyleBackColor = True
            '
            'm_scIterResult
            '
            Me.m_scIterResult.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scIterResult.Location = New System.Drawing.Point(3, 3)
            Me.m_scIterResult.Name = "m_scIterResult"
            '
            'm_scIterResult.Panel1
            '
            Me.m_scIterResult.Panel1.Controls.Add(Me.m_scIterResultMultiRun)
            Me.m_scIterResult.Size = New System.Drawing.Size(975, 261)
            Me.m_scIterResult.SplitterDistance = 541
            Me.m_scIterResult.TabIndex = 0
            '
            'm_scIterResultMultiRun
            '
            Me.m_scIterResultMultiRun.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scIterResultMultiRun.Location = New System.Drawing.Point(0, 0)
            Me.m_scIterResultMultiRun.Name = "m_scIterResultMultiRun"
            Me.m_scIterResultMultiRun.Orientation = System.Windows.Forms.Orientation.Horizontal
            Me.m_scIterResultMultiRun.Size = New System.Drawing.Size(541, 261)
            Me.m_scIterResultMultiRun.SplitterDistance = 131
            Me.m_scIterResultMultiRun.TabIndex = 0
            '
            'm_tpPlots
            '
            Me.m_tpPlots.Controls.Add(Me.m_graphResults)
            Me.m_tpPlots.Location = New System.Drawing.Point(4, 22)
            Me.m_tpPlots.Name = "m_tpPlots"
            Me.m_tpPlots.Padding = New System.Windows.Forms.Padding(3)
            Me.m_tpPlots.Size = New System.Drawing.Size(981, 267)
            Me.m_tpPlots.TabIndex = 2
            Me.m_tpPlots.Text = "Plot results"
            Me.m_tpPlots.UseVisualStyleBackColor = True
            '
            'm_graphResults
            '
            Me.m_graphResults.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_graphResults.IsAutoScrollRange = True
            Me.m_graphResults.Location = New System.Drawing.Point(3, 3)
            Me.m_graphResults.Name = "m_graphResults"
            Me.m_graphResults.ScrollGrace = 0.0R
            Me.m_graphResults.ScrollMaxX = 0.0R
            Me.m_graphResults.ScrollMaxY = 0.0R
            Me.m_graphResults.ScrollMaxY2 = 0.0R
            Me.m_graphResults.ScrollMinX = 0.0R
            Me.m_graphResults.ScrollMinY = 0.0R
            Me.m_graphResults.ScrollMinY2 = 0.0R
            Me.m_graphResults.Size = New System.Drawing.Size(975, 261)
            Me.m_graphResults.TabIndex = 8
            '
            'frmFishingPolicySearch
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.ClientSize = New System.Drawing.Size(993, 687)
            Me.Controls.Add(Me.m_scTopBits)
            Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Name = "frmFishingPolicySearch"
            Me.TabText = "Fishing policy search"
            Me.Text = "Fishing policy search"
            Me.m_scTopBits.Panel1.ResumeLayout(False)
            Me.m_scTopBits.Panel2.ResumeLayout(False)
            CType(Me.m_scTopBits, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scTopBits.ResumeLayout(False)
            Me.m_scTop.Panel1.ResumeLayout(False)
            Me.m_scTop.Panel2.ResumeLayout(False)
            CType(Me.m_scTop, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scTop.ResumeLayout(False)
            Me.m_tlpControls.ResumeLayout(False)
            Me.m_plRunParams.ResumeLayout(False)
            Me.m_plRunParams.PerformLayout()
            CType(Me.m_nudMaxEffChg, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudBaseYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudMaxNumEval, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNumberOfRuns, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tlpRunStop.ResumeLayout(False)
            Me.m_tcMain.ResumeLayout(False)
            Me.m_tabObjectives.ResumeLayout(False)
            Me.m_scObjectives.Panel1.ResumeLayout(False)
            Me.m_scObjectives.Panel2.ResumeLayout(False)
            CType(Me.m_scObjectives, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scObjectives.ResumeLayout(False)
            Me.m_scAarghArghAaargh.Panel1.ResumeLayout(False)
            Me.m_scAarghArghAaargh.Panel2.ResumeLayout(False)
            CType(Me.m_scAarghArghAaargh, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scAarghArghAaargh.ResumeLayout(False)
            Me.m_tabResultTable.ResumeLayout(False)
            Me.m_scIterResult.Panel1.ResumeLayout(False)
            CType(Me.m_scIterResult, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scIterResult.ResumeLayout(False)
            CType(Me.m_scIterResultMultiRun, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scIterResultMultiRun.ResumeLayout(False)
            Me.m_tpPlots.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_scTopBits As System.Windows.Forms.SplitContainer
        Private WithEvents m_lblSearchUsing As System.Windows.Forms.Label
        Private WithEvents btnStop As System.Windows.Forms.Button
        Private WithEvents btnSearch As System.Windows.Forms.Button
        Private WithEvents m_graphResults As ZedGraph.ZedGraphControl
        Private WithEvents m_tlpRunStop As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_lblOptmApproach As System.Windows.Forms.Label
        Private WithEvents m_lblInitUsing As System.Windows.Forms.Label
        Private WithEvents m_lblMaxEffChg As System.Windows.Forms.Label
        Private WithEvents m_lblBaseYear As System.Windows.Forms.Label
        Private WithEvents m_lblMaxNumEval As System.Windows.Forms.Label
        Private WithEvents m_lblNumOfRuns As System.Windows.Forms.Label
        Private WithEvents m_lblDiscRate As System.Windows.Forms.Label
        Private WithEvents m_txtDiscountRate As System.Windows.Forms.TextBox
        Private WithEvents m_cmbInitUsing As System.Windows.Forms.ComboBox
        Private WithEvents m_cmbSearchUsing As System.Windows.Forms.ComboBox
        Private WithEvents m_cmbOptmApproach As System.Windows.Forms.ComboBox
        Private WithEvents m_chkUsePlugin As System.Windows.Forms.CheckBox
        Private WithEvents m_chkPrevCE As System.Windows.Forms.CheckBox
        Private WithEvents m_chkMaxPortUl As System.Windows.Forms.CheckBox
        Private WithEvents m_chkIncludeCCosts As System.Windows.Forms.CheckBox
        Private WithEvents m_plRunParams As System.Windows.Forms.Panel
        Private WithEvents m_tcMain As System.Windows.Forms.TabControl
        Private WithEvents m_tabObjectives As System.Windows.Forms.TabPage
        Private WithEvents m_scObjectives As System.Windows.Forms.SplitContainer
        Private WithEvents m_scAarghArghAaargh As System.Windows.Forms.SplitContainer
        Private WithEvents m_tabResultTable As System.Windows.Forms.TabPage
        Private WithEvents m_scIterResult As System.Windows.Forms.SplitContainer
        Private WithEvents m_scIterResultMultiRun As System.Windows.Forms.SplitContainer
        Private WithEvents m_tpPlots As System.Windows.Forms.TabPage
        Private WithEvents m_gridObjWeights As gridSearchObjectivesWeight
        Private WithEvents m_gridObjFleet As gridSearchObjectivesFleet
        Private WithEvents m_gridObjGroup As gridSearchObjectivesGroup
        Private WithEvents m_hdrParms As cEwEHeaderLabel
        Private WithEvents m_scTop As System.Windows.Forms.SplitContainer
        Private WithEvents m_blocks As ScientificInterface.Ecosim.ucPolicyColorBlocks
        Private WithEvents m_lbGenDiscount As System.Windows.Forms.Label
        Private WithEvents m_txtGenDiscount As System.Windows.Forms.TextBox
        Private WithEvents m_tlpControls As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_nudNumberOfRuns As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMaxNumEval As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudBaseYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMaxEffChg As ScientificInterfaceShared.Controls.cEwENumericUpDown
    End Class
End Namespace