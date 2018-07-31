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
Imports WeifenLuo.WinFormsUI.Docking

Partial Class frmEcotroph
    Inherits frmEwE

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
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

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcotroph))
        Me.diagnosis_page = New System.Windows.Forms.TabPage()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.list_group_diag = New System.Windows.Forms.ListBox()
        Me.All_group = New System.Windows.Forms.CheckBox()
        Me.List_fleet1 = New System.Windows.Forms.ListBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Ponto = New System.Windows.Forms.MaskedTextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Kfeed = New System.Windows.Forms.MaskedTextBox()
        Me.Forag = New System.Windows.Forms.CheckBox()
        Me.same_mf = New System.Windows.Forms.CheckBox()
        Me.b_input_check = New System.Windows.Forms.CheckBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.formd = New System.Windows.Forms.MaskedTextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.beta = New System.Windows.Forms.MaskedTextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TopD = New System.Windows.Forms.MaskedTextBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.log_scale_diagnose = New System.Windows.Forms.CheckBox()
        Me.PictureBox5 = New System.Windows.Forms.PictureBox()
        Me.getgraph_diag = New System.Windows.Forms.CheckBox()
        Me.reset_param_diag = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.panel_result_diag = New System.Windows.Forms.TabControl()
        Me.ET_Main_diagnose = New System.Windows.Forms.TabPage()
        Me.grille_ET_main_diagnose = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_B = New System.Windows.Forms.TabPage()
        Me.ET_M_D_B = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_B_acc = New System.Windows.Forms.TabPage()
        Me.ET_M_D_B_acc = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_FL_P = New System.Windows.Forms.TabPage()
        Me.ET_M_D_FL_P = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_FL_P_acc = New System.Windows.Forms.TabPage()
        Me.ET_M_D_FL_P_acc = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_Kin = New System.Windows.Forms.TabPage()
        Me.ET_M_D_Kin = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_Kin_acc = New System.Windows.Forms.TabPage()
        Me.ET_M_D_Kin_acc = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_Fish_Mort = New System.Windows.Forms.TabPage()
        Me.ET_M_D_F = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_Fish_Mort_acc = New System.Windows.Forms.TabPage()
        Me.ET_M_D_F_acc = New System.Windows.Forms.DataGridView()
        Me.ET_Main_diagnose_Y = New System.Windows.Forms.TabPage()
        Me.ET_M_D_Y = New System.Windows.Forms.DataGridView()
        Me.ET_EMSY = New System.Windows.Forms.TabPage()
        Me.ET_M_EMSY = New System.Windows.Forms.DataGridView()
        Me.result_pdf_et_diag = New System.Windows.Forms.WebBrowser()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.PictureBox4 = New System.Windows.Forms.PictureBox()
        Me.Log_scale = New System.Windows.Forms.CheckBox()
        Me.getgraphs = New System.Windows.Forms.CheckBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.result_pdf = New System.Windows.Forms.WebBrowser()
        Me.panel_result = New System.Windows.Forms.TabControl()
        Me.TabPage4 = New System.Windows.Forms.TabPage()
        Me.grille_ET_main = New System.Windows.Forms.DataGridView()
        Me.TabPage5 = New System.Windows.Forms.TabPage()
        Me.grille_biomass = New System.Windows.Forms.DataGridView()
        Me.TabPage6 = New System.Windows.Forms.TabPage()
        Me.grille_biomass_acc = New System.Windows.Forms.DataGridView()
        Me.TabPage7 = New System.Windows.Forms.TabPage()
        Me.grille_flow_p = New System.Windows.Forms.DataGridView()
        Me.TabPage8 = New System.Windows.Forms.TabPage()
        Me.grille_flow_p_acc = New System.Windows.Forms.DataGridView()
        Me.Y = New System.Windows.Forms.TabPage()
        Me.grille_y = New System.Windows.Forms.DataGridView()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.ecotroph_version = New System.Windows.Forms.TextBox()
        Me.smooth_pdf = New System.Windows.Forms.WebBrowser()
        Me.smooth_graph = New System.Windows.Forms.CheckBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.decalage = New System.Windows.Forms.MaskedTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.smooth_param = New System.Windows.Forms.MaskedTextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.parameters_cst = New System.Windows.Forms.GroupBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.smooth_param_1 = New System.Windows.Forms.MaskedTextBox()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.Reset_smooth = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.type_smooth3 = New System.Windows.Forms.RadioButton()
        Me.type_smooth1 = New System.Windows.Forms.RadioButton()
        Me.type_smooth2 = New System.Windows.Forms.RadioButton()
        Me.datasmooth = New System.Windows.Forms.DataGridView()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.modeldescription = New System.Windows.Forms.TextBox()
        Me.Modelname = New System.Windows.Forms.TextBox()
        Me.commentaires = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Save_ETdata = New System.Windows.Forms.Button()
        Me.Load_from_ecopath = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.ETgridinput = New System.Windows.Forms.DataGridView()
        Me.Group_name = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TTL = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Biomass = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Production = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.accessibilty = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.OI = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.panel_webservi = New System.Windows.Forms.Panel()
        Me.site_eco = New System.Windows.Forms.WebBrowser()
        Me.Button8 = New System.Windows.Forms.Button()
        Me.models_list = New System.Windows.Forms.ListBox()
        Me.inputdata = New System.Windows.Forms.TabControl()
        Me.diagnosis_page.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        CType(Me.PictureBox5, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panel_result_diag.SuspendLayout()
        Me.ET_Main_diagnose.SuspendLayout()
        CType(Me.grille_ET_main_diagnose, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_B.SuspendLayout()
        CType(Me.ET_M_D_B, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_B_acc.SuspendLayout()
        CType(Me.ET_M_D_B_acc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_FL_P.SuspendLayout()
        CType(Me.ET_M_D_FL_P, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_FL_P_acc.SuspendLayout()
        CType(Me.ET_M_D_FL_P_acc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_Kin.SuspendLayout()
        CType(Me.ET_M_D_Kin, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_Kin_acc.SuspendLayout()
        CType(Me.ET_M_D_Kin_acc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_Fish_Mort.SuspendLayout()
        CType(Me.ET_M_D_F, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_Fish_Mort_acc.SuspendLayout()
        CType(Me.ET_M_D_F_acc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_Main_diagnose_Y.SuspendLayout()
        CType(Me.ET_M_D_Y, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ET_EMSY.SuspendLayout()
        CType(Me.ET_M_EMSY, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage3.SuspendLayout()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panel_result.SuspendLayout()
        Me.TabPage4.SuspendLayout()
        CType(Me.grille_ET_main, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage5.SuspendLayout()
        CType(Me.grille_biomass, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage6.SuspendLayout()
        CType(Me.grille_biomass_acc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage7.SuspendLayout()
        CType(Me.grille_flow_p, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage8.SuspendLayout()
        CType(Me.grille_flow_p_acc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Y.SuspendLayout()
        CType(Me.grille_y, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage2.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.parameters_cst.SuspendLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.datasmooth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage1.SuspendLayout()
        CType(Me.ETgridinput, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panel_webservi.SuspendLayout()
        Me.inputdata.SuspendLayout()
        Me.SuspendLayout()
        '
        'diagnosis_page
        '
        Me.diagnosis_page.AutoScroll = True
        Me.diagnosis_page.Controls.Add(Me.GroupBox3)
        Me.diagnosis_page.Controls.Add(Me.GroupBox5)
        Me.diagnosis_page.Controls.Add(Me.panel_result_diag)
        Me.diagnosis_page.Controls.Add(Me.result_pdf_et_diag)
        Me.diagnosis_page.Location = New System.Drawing.Point(4, 22)
        Me.diagnosis_page.Name = "diagnosis_page"
        Me.diagnosis_page.Padding = New System.Windows.Forms.Padding(3)
        Me.diagnosis_page.Size = New System.Drawing.Size(894, 741)
        Me.diagnosis_page.TabIndex = 3
        Me.diagnosis_page.Text = "ET diagnosis"
        Me.diagnosis_page.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox3.BackColor = System.Drawing.Color.Transparent
        Me.GroupBox3.Controls.Add(Me.Label9)
        Me.GroupBox3.Controls.Add(Me.Label20)
        Me.GroupBox3.Controls.Add(Me.list_group_diag)
        Me.GroupBox3.Controls.Add(Me.All_group)
        Me.GroupBox3.Controls.Add(Me.List_fleet1)
        Me.GroupBox3.Controls.Add(Me.Label11)
        Me.GroupBox3.Controls.Add(Me.Ponto)
        Me.GroupBox3.Controls.Add(Me.Label10)
        Me.GroupBox3.Controls.Add(Me.Kfeed)
        Me.GroupBox3.Controls.Add(Me.Forag)
        Me.GroupBox3.Controls.Add(Me.same_mf)
        Me.GroupBox3.Controls.Add(Me.b_input_check)
        Me.GroupBox3.Controls.Add(Me.Label8)
        Me.GroupBox3.Controls.Add(Me.formd)
        Me.GroupBox3.Controls.Add(Me.Label7)
        Me.GroupBox3.Controls.Add(Me.beta)
        Me.GroupBox3.Controls.Add(Me.Label5)
        Me.GroupBox3.Controls.Add(Me.TopD)
        Me.GroupBox3.Location = New System.Drawing.Point(4, 45)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(884, 155)
        Me.GroupBox3.TabIndex = 11
        Me.GroupBox3.TabStop = False
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(319, 36)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(127, 13)
        Me.Label9.TabIndex = 28
        Me.Label9.Text = "Applied to selected group"
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(539, 36)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(174, 13)
        Me.Label20.TabIndex = 27
        Me.Label20.Text = "Fleet(s) of interest for isopleth graph"
        '
        'list_group_diag
        '
        Me.list_group_diag.Enabled = False
        Me.list_group_diag.FormattingEnabled = True
        Me.list_group_diag.IntegralHeight = False
        Me.list_group_diag.Location = New System.Drawing.Point(322, 53)
        Me.list_group_diag.Name = "list_group_diag"
        Me.list_group_diag.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple
        Me.list_group_diag.Size = New System.Drawing.Size(195, 96)
        Me.list_group_diag.TabIndex = 26
        '
        'All_group
        '
        Me.All_group.AutoSize = True
        Me.All_group.BackColor = System.Drawing.Color.Transparent
        Me.All_group.Checked = True
        Me.All_group.CheckState = System.Windows.Forms.CheckState.Checked
        Me.All_group.Location = New System.Drawing.Point(322, 19)
        Me.All_group.Margin = New System.Windows.Forms.Padding(0)
        Me.All_group.Name = "All_group"
        Me.All_group.Size = New System.Drawing.Size(116, 17)
        Me.All_group.TabIndex = 25
        Me.All_group.Text = "Applied to all group"
        Me.All_group.UseVisualStyleBackColor = False
        '
        'List_fleet1
        '
        Me.List_fleet1.FormattingEnabled = True
        Me.List_fleet1.IntegralHeight = False
        Me.List_fleet1.Location = New System.Drawing.Point(542, 54)
        Me.List_fleet1.Name = "List_fleet1"
        Me.List_fleet1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple
        Me.List_fleet1.Size = New System.Drawing.Size(185, 95)
        Me.List_fleet1.TabIndex = 22
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(219, 114)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(35, 13)
        Me.Label11.TabIndex = 19
        Me.Label11.Text = "Ponto"
        Me.Label11.Visible = False
        '
        'Ponto
        '
        Me.Ponto.Enabled = False
        Me.Ponto.Location = New System.Drawing.Point(254, 111)
        Me.Ponto.Mask = "0.00"
        Me.Ponto.Name = "Ponto"
        Me.Ponto.Size = New System.Drawing.Size(42, 20)
        Me.Ponto.TabIndex = 18
        Me.Ponto.Text = "03"
        Me.Ponto.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.Ponto.Visible = False
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(129, 114)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(35, 13)
        Me.Label10.TabIndex = 17
        Me.Label10.Text = "Kfeed"
        Me.Label10.Visible = False
        '
        'Kfeed
        '
        Me.Kfeed.Enabled = False
        Me.Kfeed.Location = New System.Drawing.Point(166, 111)
        Me.Kfeed.Mask = "00"
        Me.Kfeed.Name = "Kfeed"
        Me.Kfeed.Size = New System.Drawing.Size(46, 20)
        Me.Kfeed.TabIndex = 16
        Me.Kfeed.Text = "05"
        Me.Kfeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.Kfeed.Visible = False
        '
        'Forag
        '
        Me.Forag.AutoSize = True
        Me.Forag.Location = New System.Drawing.Point(144, 88)
        Me.Forag.Name = "Forag"
        Me.Forag.Size = New System.Drawing.Size(98, 17)
        Me.Forag.TabIndex = 15
        Me.Forag.Text = "Foraging Arena"
        Me.Forag.UseVisualStyleBackColor = True
        Me.Forag.Visible = False
        '
        'same_mf
        '
        Me.same_mf.AutoSize = True
        Me.same_mf.Checked = True
        Me.same_mf.CheckState = System.Windows.Forms.CheckState.Checked
        Me.same_mf.Location = New System.Drawing.Point(543, 19)
        Me.same_mf.Name = "same_mf"
        Me.same_mf.Size = New System.Drawing.Size(127, 17)
        Me.same_mf.TabIndex = 14
        Me.same_mf.Text = "Same.mE for all fleets"
        Me.same_mf.UseVisualStyleBackColor = True
        '
        'b_input_check
        '
        Me.b_input_check.AutoSize = True
        Me.b_input_check.Location = New System.Drawing.Point(27, 88)
        Me.b_input_check.Name = "b_input_check"
        Me.b_input_check.Size = New System.Drawing.Size(59, 17)
        Me.b_input_check.TabIndex = 13
        Me.b_input_check.Text = "B.input"
        Me.b_input_check.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(156, 41)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(47, 13)
        Me.Label8.TabIndex = 9
        Me.Label8.Text = "FormD  :"
        '
        'formd
        '
        Me.formd.Location = New System.Drawing.Point(204, 38)
        Me.formd.Mask = "0.##"
        Me.formd.Name = "formd"
        Me.formd.Size = New System.Drawing.Size(38, 20)
        Me.formd.TabIndex = 8
        Me.formd.Text = "05"
        Me.formd.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(25, 114)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(29, 13)
        Me.Label7.TabIndex = 7
        Me.Label7.Text = "Beta"
        '
        'beta
        '
        Me.beta.Enabled = False
        Me.beta.Location = New System.Drawing.Point(60, 111)
        Me.beta.Mask = "0.##"
        Me.beta.Name = "beta"
        Me.beta.Size = New System.Drawing.Size(43, 20)
        Me.beta.TabIndex = 6
        Me.beta.Text = "01"
        Me.beta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(49, 41)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(64, 13)
        Me.Label5.TabIndex = 5
        Me.Label5.Text = "Top down  :"
        '
        'TopD
        '
        Me.TopD.Location = New System.Drawing.Point(114, 38)
        Me.TopD.Mask = "0.##"
        Me.TopD.Name = "TopD"
        Me.TopD.Size = New System.Drawing.Size(34, 20)
        Me.TopD.TabIndex = 4
        Me.TopD.Text = "04"
        Me.TopD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'GroupBox5
        '
        Me.GroupBox5.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox5.Controls.Add(Me.log_scale_diagnose)
        Me.GroupBox5.Controls.Add(Me.PictureBox5)
        Me.GroupBox5.Controls.Add(Me.getgraph_diag)
        Me.GroupBox5.Controls.Add(Me.reset_param_diag)
        Me.GroupBox5.Controls.Add(Me.Button4)
        Me.GroupBox5.Location = New System.Drawing.Point(4, 3)
        Me.GroupBox5.Margin = New System.Windows.Forms.Padding(0)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Padding = New System.Windows.Forms.Padding(0)
        Me.GroupBox5.Size = New System.Drawing.Size(885, 41)
        Me.GroupBox5.TabIndex = 14
        Me.GroupBox5.TabStop = False
        '
        'log_scale_diagnose
        '
        Me.log_scale_diagnose.AutoSize = True
        Me.log_scale_diagnose.Location = New System.Drawing.Point(314, 15)
        Me.log_scale_diagnose.Name = "log_scale_diagnose"
        Me.log_scale_diagnose.Size = New System.Drawing.Size(124, 17)
        Me.log_scale_diagnose.TabIndex = 17
        Me.log_scale_diagnose.Text = "Log scale for Graphs"
        Me.log_scale_diagnose.UseVisualStyleBackColor = True
        '
        'PictureBox5
        '
        Me.PictureBox5.Image = CType(resources.GetObject("PictureBox5.Image"), System.Drawing.Image)
        Me.PictureBox5.Location = New System.Drawing.Point(261, 13)
        Me.PictureBox5.Name = "PictureBox5"
        Me.PictureBox5.Size = New System.Drawing.Size(23, 27)
        Me.PictureBox5.TabIndex = 16
        Me.PictureBox5.TabStop = False
        '
        'getgraph_diag
        '
        Me.getgraph_diag.AutoSize = True
        Me.getgraph_diag.Location = New System.Drawing.Point(454, 16)
        Me.getgraph_diag.Name = "getgraph_diag"
        Me.getgraph_diag.Size = New System.Drawing.Size(84, 17)
        Me.getgraph_diag.TabIndex = 14
        Me.getgraph_diag.Text = "View graphs"
        Me.getgraph_diag.UseVisualStyleBackColor = True
        '
        'reset_param_diag
        '
        Me.reset_param_diag.Cursor = System.Windows.Forms.Cursors.Default
        Me.reset_param_diag.Location = New System.Drawing.Point(135, 13)
        Me.reset_param_diag.Name = "reset_param_diag"
        Me.reset_param_diag.Size = New System.Drawing.Size(120, 23)
        Me.reset_param_diag.TabIndex = 12
        Me.reset_param_diag.Text = "Reset parameters"
        Me.reset_param_diag.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Enabled = False
        Me.Button4.Location = New System.Drawing.Point(9, 13)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(120, 23)
        Me.Button4.TabIndex = 8
        Me.Button4.Text = "Launch ET diagnosis"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'panel_result_diag
        '
        Me.panel_result_diag.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_B)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_B_acc)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_FL_P)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_FL_P_acc)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_Kin)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_Kin_acc)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_Fish_Mort)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_Fish_Mort_acc)
        Me.panel_result_diag.Controls.Add(Me.ET_Main_diagnose_Y)
        Me.panel_result_diag.Controls.Add(Me.ET_EMSY)
        Me.panel_result_diag.Location = New System.Drawing.Point(4, 206)
        Me.panel_result_diag.Name = "panel_result_diag"
        Me.panel_result_diag.SelectedIndex = 0
        Me.panel_result_diag.Size = New System.Drawing.Size(890, 535)
        Me.panel_result_diag.TabIndex = 10
        '
        'ET_Main_diagnose
        '
        Me.ET_Main_diagnose.Controls.Add(Me.grille_ET_main_diagnose)
        Me.ET_Main_diagnose.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose.Name = "ET_Main_diagnose"
        Me.ET_Main_diagnose.Padding = New System.Windows.Forms.Padding(3)
        Me.ET_Main_diagnose.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose.TabIndex = 0
        Me.ET_Main_diagnose.Text = "ET_Main_diagnose"
        Me.ET_Main_diagnose.UseVisualStyleBackColor = True
        '
        'grille_ET_main_diagnose
        '
        Me.grille_ET_main_diagnose.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_ET_main_diagnose.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_ET_main_diagnose.Location = New System.Drawing.Point(3, 3)
        Me.grille_ET_main_diagnose.Name = "grille_ET_main_diagnose"
        Me.grille_ET_main_diagnose.Size = New System.Drawing.Size(876, 503)
        Me.grille_ET_main_diagnose.TabIndex = 0
        '
        'ET_Main_diagnose_B
        '
        Me.ET_Main_diagnose_B.Controls.Add(Me.ET_M_D_B)
        Me.ET_Main_diagnose_B.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_B.Name = "ET_Main_diagnose_B"
        Me.ET_Main_diagnose_B.Padding = New System.Windows.Forms.Padding(3)
        Me.ET_Main_diagnose_B.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_B.TabIndex = 1
        Me.ET_Main_diagnose_B.Text = "B"
        Me.ET_Main_diagnose_B.UseVisualStyleBackColor = True
        '
        'ET_M_D_B
        '
        Me.ET_M_D_B.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_B.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_B.Location = New System.Drawing.Point(3, 3)
        Me.ET_M_D_B.Name = "ET_M_D_B"
        Me.ET_M_D_B.Size = New System.Drawing.Size(876, 503)
        Me.ET_M_D_B.TabIndex = 0
        '
        'ET_Main_diagnose_B_acc
        '
        Me.ET_Main_diagnose_B_acc.Controls.Add(Me.ET_M_D_B_acc)
        Me.ET_Main_diagnose_B_acc.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_B_acc.Name = "ET_Main_diagnose_B_acc"
        Me.ET_Main_diagnose_B_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_B_acc.TabIndex = 2
        Me.ET_Main_diagnose_B_acc.Text = "B_acc"
        Me.ET_Main_diagnose_B_acc.UseVisualStyleBackColor = True
        '
        'ET_M_D_B_acc
        '
        Me.ET_M_D_B_acc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_B_acc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_B_acc.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_B_acc.Name = "ET_M_D_B_acc"
        Me.ET_M_D_B_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_B_acc.TabIndex = 1
        '
        'ET_Main_diagnose_FL_P
        '
        Me.ET_Main_diagnose_FL_P.Controls.Add(Me.ET_M_D_FL_P)
        Me.ET_Main_diagnose_FL_P.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_FL_P.Name = "ET_Main_diagnose_FL_P"
        Me.ET_Main_diagnose_FL_P.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_FL_P.TabIndex = 3
        Me.ET_Main_diagnose_FL_P.Text = "P"
        Me.ET_Main_diagnose_FL_P.UseVisualStyleBackColor = True
        '
        'ET_M_D_FL_P
        '
        Me.ET_M_D_FL_P.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_FL_P.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_FL_P.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_FL_P.Name = "ET_M_D_FL_P"
        Me.ET_M_D_FL_P.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_FL_P.TabIndex = 1
        '
        'ET_Main_diagnose_FL_P_acc
        '
        Me.ET_Main_diagnose_FL_P_acc.Controls.Add(Me.ET_M_D_FL_P_acc)
        Me.ET_Main_diagnose_FL_P_acc.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_FL_P_acc.Name = "ET_Main_diagnose_FL_P_acc"
        Me.ET_Main_diagnose_FL_P_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_FL_P_acc.TabIndex = 4
        Me.ET_Main_diagnose_FL_P_acc.Text = "P_acc"
        Me.ET_Main_diagnose_FL_P_acc.UseVisualStyleBackColor = True
        '
        'ET_M_D_FL_P_acc
        '
        Me.ET_M_D_FL_P_acc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_FL_P_acc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_FL_P_acc.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_FL_P_acc.Name = "ET_M_D_FL_P_acc"
        Me.ET_M_D_FL_P_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_FL_P_acc.TabIndex = 1
        '
        'ET_Main_diagnose_Kin
        '
        Me.ET_Main_diagnose_Kin.Controls.Add(Me.ET_M_D_Kin)
        Me.ET_Main_diagnose_Kin.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_Kin.Name = "ET_Main_diagnose_Kin"
        Me.ET_Main_diagnose_Kin.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_Kin.TabIndex = 5
        Me.ET_Main_diagnose_Kin.Text = "Kin"
        Me.ET_Main_diagnose_Kin.UseVisualStyleBackColor = True
        '
        'ET_M_D_Kin
        '
        Me.ET_M_D_Kin.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_Kin.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_Kin.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_Kin.Name = "ET_M_D_Kin"
        Me.ET_M_D_Kin.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_Kin.TabIndex = 1
        '
        'ET_Main_diagnose_Kin_acc
        '
        Me.ET_Main_diagnose_Kin_acc.Controls.Add(Me.ET_M_D_Kin_acc)
        Me.ET_Main_diagnose_Kin_acc.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_Kin_acc.Name = "ET_Main_diagnose_Kin_acc"
        Me.ET_Main_diagnose_Kin_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_Kin_acc.TabIndex = 6
        Me.ET_Main_diagnose_Kin_acc.Text = "Kin_acc"
        Me.ET_Main_diagnose_Kin_acc.UseVisualStyleBackColor = True
        '
        'ET_M_D_Kin_acc
        '
        Me.ET_M_D_Kin_acc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_Kin_acc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_Kin_acc.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_Kin_acc.Name = "ET_M_D_Kin_acc"
        Me.ET_M_D_Kin_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_Kin_acc.TabIndex = 1
        '
        'ET_Main_diagnose_Fish_Mort
        '
        Me.ET_Main_diagnose_Fish_Mort.Controls.Add(Me.ET_M_D_F)
        Me.ET_Main_diagnose_Fish_Mort.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_Fish_Mort.Name = "ET_Main_diagnose_Fish_Mort"
        Me.ET_Main_diagnose_Fish_Mort.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_Fish_Mort.TabIndex = 7
        Me.ET_Main_diagnose_Fish_Mort.Text = "Fish_Mort"
        Me.ET_Main_diagnose_Fish_Mort.UseVisualStyleBackColor = True
        '
        'ET_M_D_F
        '
        Me.ET_M_D_F.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_F.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_F.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_F.Name = "ET_M_D_F"
        Me.ET_M_D_F.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_F.TabIndex = 1
        '
        'ET_Main_diagnose_Fish_Mort_acc
        '
        Me.ET_Main_diagnose_Fish_Mort_acc.Controls.Add(Me.ET_M_D_F_acc)
        Me.ET_Main_diagnose_Fish_Mort_acc.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_Fish_Mort_acc.Name = "ET_Main_diagnose_Fish_Mort_acc"
        Me.ET_Main_diagnose_Fish_Mort_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_Fish_Mort_acc.TabIndex = 8
        Me.ET_Main_diagnose_Fish_Mort_acc.Text = "Fish_Mort_acc"
        Me.ET_Main_diagnose_Fish_Mort_acc.UseVisualStyleBackColor = True
        '
        'ET_M_D_F_acc
        '
        Me.ET_M_D_F_acc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_F_acc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_F_acc.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_F_acc.Name = "ET_M_D_F_acc"
        Me.ET_M_D_F_acc.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_F_acc.TabIndex = 1
        '
        'ET_Main_diagnose_Y
        '
        Me.ET_Main_diagnose_Y.Controls.Add(Me.ET_M_D_Y)
        Me.ET_Main_diagnose_Y.Location = New System.Drawing.Point(4, 22)
        Me.ET_Main_diagnose_Y.Name = "ET_Main_diagnose_Y"
        Me.ET_Main_diagnose_Y.Size = New System.Drawing.Size(882, 509)
        Me.ET_Main_diagnose_Y.TabIndex = 9
        Me.ET_Main_diagnose_Y.Text = "Y"
        Me.ET_Main_diagnose_Y.UseVisualStyleBackColor = True
        '
        'ET_M_D_Y
        '
        Me.ET_M_D_Y.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_D_Y.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_D_Y.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_D_Y.Name = "ET_M_D_Y"
        Me.ET_M_D_Y.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_D_Y.TabIndex = 1
        '
        'ET_EMSY
        '
        Me.ET_EMSY.Controls.Add(Me.ET_M_EMSY)
        Me.ET_EMSY.Location = New System.Drawing.Point(4, 22)
        Me.ET_EMSY.Name = "ET_EMSY"
        Me.ET_EMSY.Size = New System.Drawing.Size(882, 509)
        Me.ET_EMSY.TabIndex = 10
        Me.ET_EMSY.Text = "EMSY"
        Me.ET_EMSY.UseVisualStyleBackColor = True
        '
        'ET_M_EMSY
        '
        Me.ET_M_EMSY.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ET_M_EMSY.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ET_M_EMSY.Location = New System.Drawing.Point(0, 0)
        Me.ET_M_EMSY.Name = "ET_M_EMSY"
        Me.ET_M_EMSY.Size = New System.Drawing.Size(882, 509)
        Me.ET_M_EMSY.TabIndex = 2
        '
        'result_pdf_et_diag
        '
        Me.result_pdf_et_diag.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.result_pdf_et_diag.Location = New System.Drawing.Point(3, 206)
        Me.result_pdf_et_diag.MinimumSize = New System.Drawing.Size(20, 20)
        Me.result_pdf_et_diag.Name = "result_pdf_et_diag"
        Me.result_pdf_et_diag.Size = New System.Drawing.Size(895, 527)
        Me.result_pdf_et_diag.TabIndex = 16
        Me.result_pdf_et_diag.Url = New System.Uri("about:blank", System.UriKind.Absolute)
        Me.result_pdf_et_diag.Visible = False
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.PictureBox4)
        Me.TabPage3.Controls.Add(Me.Log_scale)
        Me.TabPage3.Controls.Add(Me.getgraphs)
        Me.TabPage3.Controls.Add(Me.Button3)
        Me.TabPage3.Controls.Add(Me.result_pdf)
        Me.TabPage3.Controls.Add(Me.panel_result)
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Size = New System.Drawing.Size(894, 741)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "ET transpose"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'PictureBox4
        '
        Me.PictureBox4.Image = CType(resources.GetObject("PictureBox4.Image"), System.Drawing.Image)
        Me.PictureBox4.Location = New System.Drawing.Point(259, 7)
        Me.PictureBox4.Name = "PictureBox4"
        Me.PictureBox4.Size = New System.Drawing.Size(23, 19)
        Me.PictureBox4.TabIndex = 13
        Me.PictureBox4.TabStop = False
        '
        'Log_scale
        '
        Me.Log_scale.AutoSize = True
        Me.Log_scale.Location = New System.Drawing.Point(129, 7)
        Me.Log_scale.Name = "Log_scale"
        Me.Log_scale.Size = New System.Drawing.Size(124, 17)
        Me.Log_scale.TabIndex = 7
        Me.Log_scale.Text = "Log scale for Graphs"
        Me.Log_scale.UseVisualStyleBackColor = True
        '
        'getgraphs
        '
        Me.getgraphs.AutoSize = True
        Me.getgraphs.Location = New System.Drawing.Point(301, 7)
        Me.getgraphs.Name = "getgraphs"
        Me.getgraphs.Size = New System.Drawing.Size(84, 17)
        Me.getgraphs.TabIndex = 6
        Me.getgraphs.Text = "View graphs"
        Me.getgraphs.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.Enabled = False
        Me.Button3.Location = New System.Drawing.Point(3, 3)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(120, 23)
        Me.Button3.TabIndex = 1
        Me.Button3.Text = "Launch ET transpose"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'result_pdf
        '
        Me.result_pdf.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.result_pdf.Location = New System.Drawing.Point(3, 32)
        Me.result_pdf.MinimumSize = New System.Drawing.Size(20, 20)
        Me.result_pdf.Name = "result_pdf"
        Me.result_pdf.Size = New System.Drawing.Size(887, 701)
        Me.result_pdf.TabIndex = 6
        Me.result_pdf.Url = New System.Uri("about:blank", System.UriKind.Absolute)
        Me.result_pdf.Visible = False
        '
        'panel_result
        '
        Me.panel_result.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.panel_result.Controls.Add(Me.TabPage4)
        Me.panel_result.Controls.Add(Me.TabPage5)
        Me.panel_result.Controls.Add(Me.TabPage6)
        Me.panel_result.Controls.Add(Me.TabPage7)
        Me.panel_result.Controls.Add(Me.TabPage8)
        Me.panel_result.Controls.Add(Me.Y)
        Me.panel_result.Location = New System.Drawing.Point(3, 32)
        Me.panel_result.Name = "panel_result"
        Me.panel_result.SelectedIndex = 0
        Me.panel_result.Size = New System.Drawing.Size(891, 701)
        Me.panel_result.TabIndex = 0
        '
        'TabPage4
        '
        Me.TabPage4.Controls.Add(Me.grille_ET_main)
        Me.TabPage4.Location = New System.Drawing.Point(4, 22)
        Me.TabPage4.Name = "TabPage4"
        Me.TabPage4.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage4.Size = New System.Drawing.Size(883, 675)
        Me.TabPage4.TabIndex = 0
        Me.TabPage4.Text = "ET_main"
        Me.TabPage4.UseVisualStyleBackColor = True
        '
        'grille_ET_main
        '
        Me.grille_ET_main.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_ET_main.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_ET_main.Location = New System.Drawing.Point(3, 3)
        Me.grille_ET_main.Name = "grille_ET_main"
        Me.grille_ET_main.Size = New System.Drawing.Size(877, 669)
        Me.grille_ET_main.TabIndex = 0
        '
        'TabPage5
        '
        Me.TabPage5.Controls.Add(Me.grille_biomass)
        Me.TabPage5.Location = New System.Drawing.Point(4, 22)
        Me.TabPage5.Name = "TabPage5"
        Me.TabPage5.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage5.Size = New System.Drawing.Size(883, 675)
        Me.TabPage5.TabIndex = 1
        Me.TabPage5.Text = "Biomass"
        Me.TabPage5.UseVisualStyleBackColor = True
        '
        'grille_biomass
        '
        Me.grille_biomass.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_biomass.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_biomass.Location = New System.Drawing.Point(3, 3)
        Me.grille_biomass.Name = "grille_biomass"
        Me.grille_biomass.Size = New System.Drawing.Size(877, 669)
        Me.grille_biomass.TabIndex = 0
        '
        'TabPage6
        '
        Me.TabPage6.Controls.Add(Me.grille_biomass_acc)
        Me.TabPage6.Location = New System.Drawing.Point(4, 22)
        Me.TabPage6.Name = "TabPage6"
        Me.TabPage6.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage6.Size = New System.Drawing.Size(883, 675)
        Me.TabPage6.TabIndex = 2
        Me.TabPage6.Text = "Accessible Biomass"
        Me.TabPage6.UseVisualStyleBackColor = True
        '
        'grille_biomass_acc
        '
        Me.grille_biomass_acc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_biomass_acc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_biomass_acc.Location = New System.Drawing.Point(3, 3)
        Me.grille_biomass_acc.Name = "grille_biomass_acc"
        Me.grille_biomass_acc.Size = New System.Drawing.Size(877, 669)
        Me.grille_biomass_acc.TabIndex = 0
        '
        'TabPage7
        '
        Me.TabPage7.Controls.Add(Me.grille_flow_p)
        Me.TabPage7.Location = New System.Drawing.Point(4, 22)
        Me.TabPage7.Name = "TabPage7"
        Me.TabPage7.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage7.Size = New System.Drawing.Size(883, 675)
        Me.TabPage7.TabIndex = 3
        Me.TabPage7.Text = "Production"
        Me.TabPage7.UseVisualStyleBackColor = True
        '
        'grille_flow_p
        '
        Me.grille_flow_p.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_flow_p.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_flow_p.Location = New System.Drawing.Point(3, 3)
        Me.grille_flow_p.Name = "grille_flow_p"
        Me.grille_flow_p.Size = New System.Drawing.Size(877, 669)
        Me.grille_flow_p.TabIndex = 0
        '
        'TabPage8
        '
        Me.TabPage8.Controls.Add(Me.grille_flow_p_acc)
        Me.TabPage8.Location = New System.Drawing.Point(4, 22)
        Me.TabPage8.Name = "TabPage8"
        Me.TabPage8.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage8.Size = New System.Drawing.Size(883, 675)
        Me.TabPage8.TabIndex = 4
        Me.TabPage8.Text = "Production_acc"
        Me.TabPage8.UseVisualStyleBackColor = True
        '
        'grille_flow_p_acc
        '
        Me.grille_flow_p_acc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_flow_p_acc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_flow_p_acc.Location = New System.Drawing.Point(3, 3)
        Me.grille_flow_p_acc.Name = "grille_flow_p_acc"
        Me.grille_flow_p_acc.Size = New System.Drawing.Size(877, 669)
        Me.grille_flow_p_acc.TabIndex = 0
        '
        'Y
        '
        Me.Y.Controls.Add(Me.grille_y)
        Me.Y.Location = New System.Drawing.Point(4, 22)
        Me.Y.Name = "Y"
        Me.Y.Padding = New System.Windows.Forms.Padding(3)
        Me.Y.Size = New System.Drawing.Size(883, 675)
        Me.Y.TabIndex = 5
        Me.Y.Text = "Y"
        Me.Y.UseVisualStyleBackColor = True
        '
        'grille_y
        '
        Me.grille_y.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grille_y.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grille_y.Location = New System.Drawing.Point(3, 3)
        Me.grille_y.Name = "grille_y"
        Me.grille_y.Size = New System.Drawing.Size(877, 669)
        Me.grille_y.TabIndex = 0
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.Label12)
        Me.TabPage2.Controls.Add(Me.ecotroph_version)
        Me.TabPage2.Controls.Add(Me.smooth_pdf)
        Me.TabPage2.Controls.Add(Me.smooth_graph)
        Me.TabPage2.Controls.Add(Me.GroupBox2)
        Me.TabPage2.Controls.Add(Me.GroupBox1)
        Me.TabPage2.Controls.Add(Me.type_smooth2)
        Me.TabPage2.Controls.Add(Me.datasmooth)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(894, 741)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Smooth parameters"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(6, 162)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(202, 13)
        Me.Label12.TabIndex = 10
        Me.Label12.Text = "Version of R EcoTroph Package installed"
        '
        'ecotroph_version
        '
        Me.ecotroph_version.Location = New System.Drawing.Point(214, 159)
        Me.ecotroph_version.Name = "ecotroph_version"
        Me.ecotroph_version.Size = New System.Drawing.Size(66, 20)
        Me.ecotroph_version.TabIndex = 9
        '
        'smooth_pdf
        '
        Me.smooth_pdf.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.smooth_pdf.Location = New System.Drawing.Point(6, 181)
        Me.smooth_pdf.MinimumSize = New System.Drawing.Size(20, 20)
        Me.smooth_pdf.Name = "smooth_pdf"
        Me.smooth_pdf.Size = New System.Drawing.Size(885, 552)
        Me.smooth_pdf.TabIndex = 8
        Me.smooth_pdf.Url = New System.Uri("about:blank", System.UriKind.Absolute)
        Me.smooth_pdf.Visible = False
        '
        'smooth_graph
        '
        Me.smooth_graph.AutoSize = True
        Me.smooth_graph.Location = New System.Drawing.Point(508, 158)
        Me.smooth_graph.Name = "smooth_graph"
        Me.smooth_graph.Size = New System.Drawing.Size(84, 17)
        Me.smooth_graph.TabIndex = 7
        Me.smooth_graph.Text = "View graphs"
        Me.smooth_graph.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.decalage)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.smooth_param)
        Me.GroupBox2.Location = New System.Drawing.Point(610, 13)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(288, 102)
        Me.GroupBox2.TabIndex = 6
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "parameters"
        Me.GroupBox2.Visible = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(31, 67)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(109, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Decalage parameter :"
        '
        'decalage
        '
        Me.decalage.Location = New System.Drawing.Point(179, 67)
        Me.decalage.Mask = "0.##"
        Me.decalage.Name = "decalage"
        Me.decalage.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.decalage.Size = New System.Drawing.Size(100, 20)
        Me.decalage.TabIndex = 7
        Me.decalage.Text = "095"
        Me.decalage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(31, 25)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(99, 13)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Smooth parameter :"
        '
        'smooth_param
        '
        Me.smooth_param.Location = New System.Drawing.Point(179, 23)
        Me.smooth_param.Mask = "0.##"
        Me.smooth_param.Name = "smooth_param"
        Me.smooth_param.Size = New System.Drawing.Size(100, 20)
        Me.smooth_param.TabIndex = 4
        Me.smooth_param.Text = "007"
        Me.smooth_param.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.Transparent
        Me.GroupBox1.Controls.Add(Me.parameters_cst)
        Me.GroupBox1.Controls.Add(Me.PictureBox3)
        Me.GroupBox1.Controls.Add(Me.Reset_smooth)
        Me.GroupBox1.Controls.Add(Me.Button2)
        Me.GroupBox1.Controls.Add(Me.type_smooth3)
        Me.GroupBox1.Controls.Add(Me.type_smooth1)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(586, 150)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Smooth type"
        '
        'parameters_cst
        '
        Me.parameters_cst.Controls.Add(Me.Label6)
        Me.parameters_cst.Controls.Add(Me.smooth_param_1)
        Me.parameters_cst.Location = New System.Drawing.Point(282, 10)
        Me.parameters_cst.Name = "parameters_cst"
        Me.parameters_cst.Size = New System.Drawing.Size(288, 102)
        Me.parameters_cst.TabIndex = 9
        Me.parameters_cst.TabStop = False
        Me.parameters_cst.Text = "Parameters"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(31, 25)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(99, 13)
        Me.Label6.TabIndex = 5
        Me.Label6.Text = "Smooth parameter :"
        '
        'smooth_param_1
        '
        Me.smooth_param_1.Location = New System.Drawing.Point(179, 23)
        Me.smooth_param_1.Mask = "0.##"
        Me.smooth_param_1.Name = "smooth_param_1"
        Me.smooth_param_1.Size = New System.Drawing.Size(100, 20)
        Me.smooth_param_1.TabIndex = 4
        Me.smooth_param_1.Text = "012"
        Me.smooth_param_1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'PictureBox3
        '
        Me.PictureBox3.Image = CType(resources.GetObject("PictureBox3.Image"), System.Drawing.Image)
        Me.PictureBox3.Location = New System.Drawing.Point(196, 19)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(23, 27)
        Me.PictureBox3.TabIndex = 12
        Me.PictureBox3.TabStop = False
        '
        'Reset_smooth
        '
        Me.Reset_smooth.Cursor = System.Windows.Forms.Cursors.Default
        Me.Reset_smooth.Location = New System.Drawing.Point(420, 116)
        Me.Reset_smooth.Name = "Reset_smooth"
        Me.Reset_smooth.Size = New System.Drawing.Size(150, 23)
        Me.Reset_smooth.TabIndex = 10
        Me.Reset_smooth.Text = "Reset parameters"
        Me.Reset_smooth.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Enabled = False
        Me.Button2.Location = New System.Drawing.Point(18, 89)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(223, 23)
        Me.Button2.TabIndex = 5
        Me.Button2.Text = "Create smooth"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'type_smooth3
        '
        Me.type_smooth3.AutoSize = True
        Me.type_smooth3.Location = New System.Drawing.Point(22, 43)
        Me.type_smooth3.Name = "type_smooth3"
        Me.type_smooth3.Size = New System.Drawing.Size(118, 17)
        Me.type_smooth3.TabIndex = 2
        Me.type_smooth3.Text = "Lognorm Sigma =OI"
        Me.type_smooth3.UseVisualStyleBackColor = True
        '
        'type_smooth1
        '
        Me.type_smooth1.AutoSize = True
        Me.type_smooth1.Checked = True
        Me.type_smooth1.Location = New System.Drawing.Point(22, 20)
        Me.type_smooth1.Name = "type_smooth1"
        Me.type_smooth1.Size = New System.Drawing.Size(139, 17)
        Me.type_smooth1.TabIndex = 0
        Me.type_smooth1.TabStop = True
        Me.type_smooth1.Text = "Constant lognorm Sigma"
        Me.type_smooth1.UseVisualStyleBackColor = True
        '
        'type_smooth2
        '
        Me.type_smooth2.AutoSize = True
        Me.type_smooth2.Location = New System.Drawing.Point(610, 122)
        Me.type_smooth2.Name = "type_smooth2"
        Me.type_smooth2.Size = New System.Drawing.Size(174, 17)
        Me.type_smooth2.TabIndex = 1
        Me.type_smooth2.Text = "Function defined lognorm sigma"
        Me.type_smooth2.UseVisualStyleBackColor = True
        Me.type_smooth2.Visible = False
        '
        'datasmooth
        '
        Me.datasmooth.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.datasmooth.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.datasmooth.Location = New System.Drawing.Point(8, 185)
        Me.datasmooth.Name = "datasmooth"
        Me.datasmooth.Size = New System.Drawing.Size(880, 548)
        Me.datasmooth.TabIndex = 1
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.Button7)
        Me.TabPage1.Controls.Add(Me.Label13)
        Me.TabPage1.Controls.Add(Me.Label3)
        Me.TabPage1.Controls.Add(Me.modeldescription)
        Me.TabPage1.Controls.Add(Me.Modelname)
        Me.TabPage1.Controls.Add(Me.commentaires)
        Me.TabPage1.Controls.Add(Me.Label2)
        Me.TabPage1.Controls.Add(Me.Save_ETdata)
        Me.TabPage1.Controls.Add(Me.Load_from_ecopath)
        Me.TabPage1.Controls.Add(Me.Button1)
        Me.TabPage1.Controls.Add(Me.ETgridinput)
        Me.TabPage1.Controls.Add(Me.panel_webservi)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(894, 741)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Input data"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'Button7
        '
        Me.Button7.Location = New System.Drawing.Point(8, 66)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(168, 23)
        Me.Button7.TabIndex = 2
        Me.Button7.Text = "Load data from Webservices"
        Me.Button7.UseVisualStyleBackColor = True
        Me.Button7.Visible = False
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(199, 39)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(63, 13)
        Me.Label13.TabIndex = 5
        Me.Label13.Text = "Description:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(199, 13)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(68, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "Model name:"
        '
        'modeldescription
        '
        Me.modeldescription.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.modeldescription.Location = New System.Drawing.Point(273, 36)
        Me.modeldescription.Multiline = True
        Me.modeldescription.Name = "modeldescription"
        Me.modeldescription.Size = New System.Drawing.Size(612, 36)
        Me.modeldescription.TabIndex = 6
        '
        'Modelname
        '
        Me.Modelname.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Modelname.Location = New System.Drawing.Point(273, 10)
        Me.Modelname.Name = "Modelname"
        Me.Modelname.Size = New System.Drawing.Size(612, 20)
        Me.Modelname.TabIndex = 4
        '
        'commentaires
        '
        Me.commentaires.AcceptsReturn = True
        Me.commentaires.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.commentaires.Location = New System.Drawing.Point(273, 78)
        Me.commentaires.Multiline = True
        Me.commentaires.Name = "commentaires"
        Me.commentaires.Size = New System.Drawing.Size(612, 78)
        Me.commentaires.TabIndex = 8
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(199, 81)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(59, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Comments:"
        '
        'Save_ETdata
        '
        Me.Save_ETdata.Location = New System.Drawing.Point(8, 133)
        Me.Save_ETdata.Name = "Save_ETdata"
        Me.Save_ETdata.Size = New System.Drawing.Size(168, 23)
        Me.Save_ETdata.TabIndex = 9
        Me.Save_ETdata.Text = "Save input data..."
        Me.Save_ETdata.UseVisualStyleBackColor = True
        '
        'Load_from_ecopath
        '
        Me.Load_from_ecopath.Location = New System.Drawing.Point(8, 8)
        Me.Load_from_ecopath.Name = "Load_from_ecopath"
        Me.Load_from_ecopath.Size = New System.Drawing.Size(168, 23)
        Me.Load_from_ecopath.TabIndex = 0
        Me.Load_from_ecopath.Text = "Load data from Ecopath"
        Me.Load_from_ecopath.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(8, 37)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(168, 23)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "Load data from file"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'ETgridinput
        '
        Me.ETgridinput.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ETgridinput.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
        Me.ETgridinput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.ETgridinput.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Group_name, Me.TTL, Me.Biomass, Me.Production, Me.accessibilty, Me.OI})
        Me.ETgridinput.Location = New System.Drawing.Point(8, 162)
        Me.ETgridinput.Name = "ETgridinput"
        Me.ETgridinput.Size = New System.Drawing.Size(878, 571)
        Me.ETgridinput.TabIndex = 10
        '
        'Group_name
        '
        Me.Group_name.HeaderText = "Group name"
        Me.Group_name.Name = "Group_name"
        '
        'TTL
        '
        Me.TTL.HeaderText = "Trophic Level"
        Me.TTL.Name = "TTL"
        '
        'Biomass
        '
        Me.Biomass.HeaderText = "Biomass"
        Me.Biomass.Name = "Biomass"
        '
        'Production
        '
        Me.Production.HeaderText = "P/B"
        Me.Production.Name = "Production"
        '
        'accessibilty
        '
        Me.accessibilty.HeaderText = "accessibilty"
        Me.accessibilty.Name = "accessibilty"
        '
        'OI
        '
        Me.OI.HeaderText = "Omnivory index"
        Me.OI.Name = "OI"
        '
        'panel_webservi
        '
        Me.panel_webservi.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.panel_webservi.BackColor = System.Drawing.Color.DimGray
        Me.panel_webservi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panel_webservi.Controls.Add(Me.site_eco)
        Me.panel_webservi.Controls.Add(Me.Button8)
        Me.panel_webservi.Controls.Add(Me.models_list)
        Me.panel_webservi.Location = New System.Drawing.Point(7, 162)
        Me.panel_webservi.Name = "panel_webservi"
        Me.panel_webservi.Size = New System.Drawing.Size(878, 571)
        Me.panel_webservi.TabIndex = 13
        Me.panel_webservi.Visible = False
        '
        'site_eco
        '
        Me.site_eco.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.site_eco.Location = New System.Drawing.Point(194, 12)
        Me.site_eco.MinimumSize = New System.Drawing.Size(20, 20)
        Me.site_eco.Name = "site_eco"
        Me.site_eco.Size = New System.Drawing.Size(670, 540)
        Me.site_eco.TabIndex = 1
        '
        'Button8
        '
        Me.Button8.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Button8.Location = New System.Drawing.Point(13, 529)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(168, 23)
        Me.Button8.TabIndex = 2
        Me.Button8.Text = "Close Models List Selection"
        Me.Button8.UseVisualStyleBackColor = True
        '
        'models_list
        '
        Me.models_list.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.models_list.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.models_list.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.models_list.ForeColor = System.Drawing.Color.Black
        Me.models_list.FormattingEnabled = True
        Me.models_list.IntegralHeight = False
        Me.models_list.Location = New System.Drawing.Point(13, 11)
        Me.models_list.Name = "models_list"
        Me.models_list.Size = New System.Drawing.Size(168, 512)
        Me.models_list.TabIndex = 0
        '
        'inputdata
        '
        Me.inputdata.Controls.Add(Me.TabPage1)
        Me.inputdata.Controls.Add(Me.TabPage2)
        Me.inputdata.Controls.Add(Me.TabPage3)
        Me.inputdata.Controls.Add(Me.diagnosis_page)
        Me.inputdata.Dock = System.Windows.Forms.DockStyle.Fill
        Me.inputdata.Location = New System.Drawing.Point(0, 0)
        Me.inputdata.Name = "inputdata"
        Me.inputdata.SelectedIndex = 0
        Me.inputdata.Size = New System.Drawing.Size(902, 767)
        Me.inputdata.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight
        Me.inputdata.TabIndex = 0
        '
        'frmEcotroph
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(902, 767)
        Me.Controls.Add(Me.inputdata)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmEcotroph"
        Me.TabText = ""
        Me.Text = "EcoTroph plugin"
        Me.diagnosis_page.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        CType(Me.PictureBox5, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panel_result_diag.ResumeLayout(False)
        Me.ET_Main_diagnose.ResumeLayout(False)
        CType(Me.grille_ET_main_diagnose, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_B.ResumeLayout(False)
        CType(Me.ET_M_D_B, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_B_acc.ResumeLayout(False)
        CType(Me.ET_M_D_B_acc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_FL_P.ResumeLayout(False)
        CType(Me.ET_M_D_FL_P, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_FL_P_acc.ResumeLayout(False)
        CType(Me.ET_M_D_FL_P_acc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_Kin.ResumeLayout(False)
        CType(Me.ET_M_D_Kin, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_Kin_acc.ResumeLayout(False)
        CType(Me.ET_M_D_Kin_acc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_Fish_Mort.ResumeLayout(False)
        CType(Me.ET_M_D_F, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_Fish_Mort_acc.ResumeLayout(False)
        CType(Me.ET_M_D_F_acc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_Main_diagnose_Y.ResumeLayout(False)
        CType(Me.ET_M_D_Y, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ET_EMSY.ResumeLayout(False)
        CType(Me.ET_M_EMSY, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage3.PerformLayout()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panel_result.ResumeLayout(False)
        Me.TabPage4.ResumeLayout(False)
        CType(Me.grille_ET_main, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage5.ResumeLayout(False)
        CType(Me.grille_biomass, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage6.ResumeLayout(False)
        CType(Me.grille_biomass_acc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage7.ResumeLayout(False)
        CType(Me.grille_flow_p, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage8.ResumeLayout(False)
        CType(Me.grille_flow_p_acc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Y.ResumeLayout(False)
        CType(Me.grille_y, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.parameters_cst.ResumeLayout(False)
        Me.parameters_cst.PerformLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.datasmooth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        CType(Me.ETgridinput, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panel_webservi.ResumeLayout(False)
        Me.inputdata.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents diagnosis_page As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents list_group_diag As System.Windows.Forms.ListBox
    Friend WithEvents All_group As System.Windows.Forms.CheckBox
    Friend WithEvents List_fleet1 As System.Windows.Forms.ListBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Ponto As System.Windows.Forms.MaskedTextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Kfeed As System.Windows.Forms.MaskedTextBox
    Friend WithEvents Forag As System.Windows.Forms.CheckBox
    Friend WithEvents same_mf As System.Windows.Forms.CheckBox
    Friend WithEvents b_input_check As System.Windows.Forms.CheckBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents formd As System.Windows.Forms.MaskedTextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents beta As System.Windows.Forms.MaskedTextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents TopD As System.Windows.Forms.MaskedTextBox
 
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents log_scale_diagnose As System.Windows.Forms.CheckBox
    Friend WithEvents PictureBox5 As System.Windows.Forms.PictureBox
    Friend WithEvents getgraph_diag As System.Windows.Forms.CheckBox
    Friend WithEvents reset_param_diag As System.Windows.Forms.Button
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents panel_result_diag As System.Windows.Forms.TabControl
    Friend WithEvents ET_Main_diagnose As System.Windows.Forms.TabPage
    Friend WithEvents grille_ET_main_diagnose As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_B As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_B As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_B_acc As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_B_acc As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_FL_P As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_FL_P As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_FL_P_acc As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_FL_P_acc As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_Kin As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_Kin As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_Kin_acc As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_Kin_acc As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_Fish_Mort As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_F As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_Fish_Mort_acc As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_F_acc As System.Windows.Forms.DataGridView
    Friend WithEvents ET_Main_diagnose_Y As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_D_Y As System.Windows.Forms.DataGridView
    Friend WithEvents ET_EMSY As System.Windows.Forms.TabPage
    Friend WithEvents ET_M_EMSY As System.Windows.Forms.DataGridView
    Friend WithEvents result_pdf_et_diag As System.Windows.Forms.WebBrowser
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents PictureBox4 As System.Windows.Forms.PictureBox
    Friend WithEvents Log_scale As System.Windows.Forms.CheckBox
    Friend WithEvents getgraphs As System.Windows.Forms.CheckBox
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents result_pdf As System.Windows.Forms.WebBrowser
    Friend WithEvents panel_result As System.Windows.Forms.TabControl
    Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
    Friend WithEvents grille_ET_main As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage5 As System.Windows.Forms.TabPage
    Friend WithEvents grille_biomass As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage6 As System.Windows.Forms.TabPage
    Friend WithEvents grille_biomass_acc As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage7 As System.Windows.Forms.TabPage
    Friend WithEvents grille_flow_p As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage8 As System.Windows.Forms.TabPage
    Friend WithEvents grille_flow_p_acc As System.Windows.Forms.DataGridView
    Friend WithEvents Y As System.Windows.Forms.TabPage
    Friend WithEvents grille_y As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents ecotroph_version As System.Windows.Forms.TextBox
    Friend WithEvents smooth_pdf As System.Windows.Forms.WebBrowser
    Friend WithEvents smooth_graph As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents decalage As System.Windows.Forms.MaskedTextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents smooth_param As System.Windows.Forms.MaskedTextBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents parameters_cst As System.Windows.Forms.GroupBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents smooth_param_1 As System.Windows.Forms.MaskedTextBox
    Friend WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Friend WithEvents Reset_smooth As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents type_smooth3 As System.Windows.Forms.RadioButton
    Friend WithEvents type_smooth1 As System.Windows.Forms.RadioButton
    Friend WithEvents type_smooth2 As System.Windows.Forms.RadioButton
    Friend WithEvents datasmooth As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents modeldescription As System.Windows.Forms.TextBox
    Friend WithEvents Modelname As System.Windows.Forms.TextBox
    Friend WithEvents commentaires As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Save_ETdata As System.Windows.Forms.Button
    Friend WithEvents Load_from_ecopath As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents ETgridinput As System.Windows.Forms.DataGridView
    Friend WithEvents panel_webservi As System.Windows.Forms.Panel
    Friend WithEvents site_eco As System.Windows.Forms.WebBrowser
    Friend WithEvents Button8 As System.Windows.Forms.Button
    Friend WithEvents models_list As System.Windows.Forms.ListBox
    Friend WithEvents inputdata As System.Windows.Forms.TabControl
    Friend WithEvents Group_name As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents TTL As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Biomass As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Production As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents accessibilty As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents OI As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Button7 As System.Windows.Forms.Button
End Class
