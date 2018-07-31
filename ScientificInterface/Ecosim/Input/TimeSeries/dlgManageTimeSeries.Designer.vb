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

Partial Class dlgManageTimeSeries
    Inherits System.Windows.Forms.Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgManageTimeSeries))
        Me.m_btnOk = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_ilTabImages = New System.Windows.Forms.ImageList(Me.components)
        Me.lbSettings = New System.Windows.Forms.Label()
        Me.m_tpDelete = New System.Windows.Forms.TabPage()
        Me.m_lvDeleteDatasets = New System.Windows.Forms.ListView()
        Me.m_colDeleteDataset = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_colDeleteLoaded = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_colDeleteNumSeries = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_tpImport = New System.Windows.Forms.TabPage()
        Me.m_cmbImportInterval = New System.Windows.Forms.ComboBox()
        Me.m_lblImportInterval = New System.Windows.Forms.Label()
        Me.m_cbImportEnableOnImport = New System.Windows.Forms.CheckBox()
        Me.m_dgvImportPreview = New System.Windows.Forms.DataGridView()
        Me.m_hdrTarget = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrPreview = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrSource = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tbImportSeparator = New ScientificInterfaceShared.Controls.ucCharacterTextBox()
        Me.m_tbImportDelimiter = New ScientificInterfaceShared.Controls.ucCharacterTextBox()
        Me.m_tbImportAuthor = New System.Windows.Forms.TextBox()
        Me.m_tbImportContact = New System.Windows.Forms.TextBox()
        Me.m_tbImportDescription = New System.Windows.Forms.TextBox()
        Me.m_tbImportFileName = New System.Windows.Forms.TextBox()
        Me.m_lblImportContact = New System.Windows.Forms.Label()
        Me.m_lblImportAuthor = New System.Windows.Forms.Label()
        Me.m_lblImportDescription = New System.Windows.Forms.Label()
        Me.m_cmbImportDataset = New System.Windows.Forms.ComboBox()
        Me.m_lblImportDataset = New System.Windows.Forms.Label()
        Me.m_lblImportDecimalSeparator = New System.Windows.Forms.Label()
        Me.m_lblImportDelimiter = New System.Windows.Forms.Label()
        Me.m_rbImportSourceClipboard = New System.Windows.Forms.RadioButton()
        Me.m_btnImportBrowse = New System.Windows.Forms.Button()
        Me.m_rbImportSourceTextFile = New System.Windows.Forms.RadioButton()
        Me.m_tpWeights = New System.Windows.Forms.TabPage()
        Me.m_gridWeights = New ScientificInterface.gridWeightTS()
        Me.m_btnApplyCheckNone = New System.Windows.Forms.Button()
        Me.m_btnApplyCheckAll = New System.Windows.Forms.Button()
        Me.m_tpLoad = New System.Windows.Forms.TabPage()
        Me.m_cbLoadEnableOnLoad = New System.Windows.Forms.CheckBox()
        Me.m_lvLoadDatasets = New System.Windows.Forms.ListView()
        Me.m_colLoadDataset = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_colLoaded = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_clInterval = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_colDescription = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.m_tcMain = New System.Windows.Forms.TabControl()
        Me.m_tpDelete.SuspendLayout()
        Me.m_tpImport.SuspendLayout()
        CType(Me.m_dgvImportPreview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpWeights.SuspendLayout()
        Me.m_tpLoad.SuspendLayout()
        Me.m_tcMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_btnOk
        '
        Me.m_btnOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnOk.Location = New System.Drawing.Point(329, 612)
        Me.m_btnOk.Margin = New System.Windows.Forms.Padding(0, 3, 3, 3)
        Me.m_btnOk.Name = "m_btnOk"
        Me.m_btnOk.Size = New System.Drawing.Size(87, 23)
        Me.m_btnOk.TabIndex = 1
        Me.m_btnOk.Text = "OK"
        '
        'm_btnCancel
        '
        Me.m_btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Location = New System.Drawing.Point(422, 612)
        Me.m_btnCancel.Margin = New System.Windows.Forms.Padding(3, 3, 0, 3)
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.Size = New System.Drawing.Size(87, 23)
        Me.m_btnCancel.TabIndex = 2
        Me.m_btnCancel.Text = "Cancel"
        '
        'm_ilTabImages
        '
        Me.m_ilTabImages.ImageStream = CType(resources.GetObject("m_ilTabImages.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.m_ilTabImages.TransparentColor = System.Drawing.Color.Transparent
        Me.m_ilTabImages.Images.SetKeyName(0, "openHS.png")
        Me.m_ilTabImages.Images.SetKeyName(1, "CheckBoxHS.png")
        Me.m_ilTabImages.Images.SetKeyName(2, "NewDocumentHS.png")
        Me.m_ilTabImages.Images.SetKeyName(3, "saveHS.png")
        Me.m_ilTabImages.Images.SetKeyName(4, "DeleteHS.png")
        '
        'lbSettings
        '
        Me.lbSettings.AutoSize = True
        Me.lbSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.lbSettings.Location = New System.Drawing.Point(-41, 130)
        Me.lbSettings.Name = "lbSettings"
        Me.lbSettings.Size = New System.Drawing.Size(48, 13)
        Me.lbSettings.TabIndex = 10
        Me.lbSettings.Text = "S&ettings:"
        '
        'm_tpDelete
        '
        Me.m_tpDelete.Controls.Add(Me.m_lvDeleteDatasets)
        Me.m_tpDelete.ImageIndex = 4
        Me.m_tpDelete.Location = New System.Drawing.Point(4, 26)
        Me.m_tpDelete.Name = "m_tpDelete"
        Me.m_tpDelete.Size = New System.Drawing.Size(499, 570)
        Me.m_tpDelete.TabIndex = 4
        Me.m_tpDelete.Text = "Delete"
        Me.m_tpDelete.UseVisualStyleBackColor = True
        '
        'm_lvDeleteDatasets
        '
        Me.m_lvDeleteDatasets.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.m_colDeleteDataset, Me.m_colDeleteLoaded, Me.m_colDeleteNumSeries})
        Me.m_lvDeleteDatasets.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lvDeleteDatasets.FullRowSelect = True
        Me.m_lvDeleteDatasets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.m_lvDeleteDatasets.HideSelection = False
        Me.m_lvDeleteDatasets.LabelWrap = False
        Me.m_lvDeleteDatasets.Location = New System.Drawing.Point(0, 0)
        Me.m_lvDeleteDatasets.Name = "m_lvDeleteDatasets"
        Me.m_lvDeleteDatasets.Size = New System.Drawing.Size(499, 570)
        Me.m_lvDeleteDatasets.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.m_lvDeleteDatasets.TabIndex = 1
        Me.m_lvDeleteDatasets.UseCompatibleStateImageBehavior = False
        Me.m_lvDeleteDatasets.View = System.Windows.Forms.View.Details
        '
        'm_colDeleteDataset
        '
        Me.m_colDeleteDataset.Text = "Dataset"
        Me.m_colDeleteDataset.Width = 200
        '
        'm_colDeleteLoaded
        '
        Me.m_colDeleteLoaded.Text = "Loaded"
        Me.m_colDeleteLoaded.Width = 50
        '
        'm_colDeleteNumSeries
        '
        Me.m_colDeleteNumSeries.Text = "Number of Time Series"
        Me.m_colDeleteNumSeries.Width = 300
        '
        'm_tpImport
        '
        Me.m_tpImport.Controls.Add(Me.m_cmbImportInterval)
        Me.m_tpImport.Controls.Add(Me.m_lblImportInterval)
        Me.m_tpImport.Controls.Add(Me.m_cbImportEnableOnImport)
        Me.m_tpImport.Controls.Add(Me.m_dgvImportPreview)
        Me.m_tpImport.Controls.Add(Me.m_hdrTarget)
        Me.m_tpImport.Controls.Add(Me.m_hdrPreview)
        Me.m_tpImport.Controls.Add(Me.m_hdrSource)
        Me.m_tpImport.Controls.Add(Me.m_tbImportSeparator)
        Me.m_tpImport.Controls.Add(Me.m_tbImportDelimiter)
        Me.m_tpImport.Controls.Add(Me.m_tbImportAuthor)
        Me.m_tpImport.Controls.Add(Me.m_tbImportContact)
        Me.m_tpImport.Controls.Add(Me.m_tbImportDescription)
        Me.m_tpImport.Controls.Add(Me.m_tbImportFileName)
        Me.m_tpImport.Controls.Add(Me.m_lblImportContact)
        Me.m_tpImport.Controls.Add(Me.m_lblImportAuthor)
        Me.m_tpImport.Controls.Add(Me.m_lblImportDescription)
        Me.m_tpImport.Controls.Add(Me.m_cmbImportDataset)
        Me.m_tpImport.Controls.Add(Me.m_lblImportDataset)
        Me.m_tpImport.Controls.Add(Me.m_lblImportDecimalSeparator)
        Me.m_tpImport.Controls.Add(Me.m_lblImportDelimiter)
        Me.m_tpImport.Controls.Add(Me.m_rbImportSourceClipboard)
        Me.m_tpImport.Controls.Add(Me.m_btnImportBrowse)
        Me.m_tpImport.Controls.Add(Me.m_rbImportSourceTextFile)
        Me.m_tpImport.ImageIndex = 2
        Me.m_tpImport.Location = New System.Drawing.Point(4, 26)
        Me.m_tpImport.Name = "m_tpImport"
        Me.m_tpImport.Size = New System.Drawing.Size(499, 570)
        Me.m_tpImport.TabIndex = 3
        Me.m_tpImport.Text = "Import"
        Me.m_tpImport.UseVisualStyleBackColor = True
        '
        'm_cmbImportInterval
        '
        Me.m_cmbImportInterval.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_cmbImportInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbImportInterval.FormattingEnabled = True
        Me.m_cmbImportInterval.Location = New System.Drawing.Point(74, 94)
        Me.m_cmbImportInterval.Name = "m_cmbImportInterval"
        Me.m_cmbImportInterval.Size = New System.Drawing.Size(345, 21)
        Me.m_cmbImportInterval.TabIndex = 10
        '
        'm_lblImportInterval
        '
        Me.m_lblImportInterval.AutoSize = True
        Me.m_lblImportInterval.Location = New System.Drawing.Point(3, 97)
        Me.m_lblImportInterval.Name = "m_lblImportInterval"
        Me.m_lblImportInterval.Size = New System.Drawing.Size(45, 13)
        Me.m_lblImportInterval.TabIndex = 9
        Me.m_lblImportInterval.Text = "&Interval:"
        '
        'm_cbImportEnableOnImport
        '
        Me.m_cbImportEnableOnImport.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.m_cbImportEnableOnImport.AutoSize = True
        Me.m_cbImportEnableOnImport.Checked = True
        Me.m_cbImportEnableOnImport.CheckState = System.Windows.Forms.CheckState.Checked
        Me.m_cbImportEnableOnImport.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_cbImportEnableOnImport.Location = New System.Drawing.Point(4, 550)
        Me.m_cbImportEnableOnImport.Name = "m_cbImportEnableOnImport"
        Me.m_cbImportEnableOnImport.Size = New System.Drawing.Size(172, 17)
        Me.m_cbImportEnableOnImport.TabIndex = 23
        Me.m_cbImportEnableOnImport.Text = "&Enable Time Series after import"
        Me.m_cbImportEnableOnImport.UseVisualStyleBackColor = True
        '
        'm_dgvImportPreview
        '
        Me.m_dgvImportPreview.AllowUserToAddRows = False
        Me.m_dgvImportPreview.AllowUserToDeleteRows = False
        Me.m_dgvImportPreview.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_dgvImportPreview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.m_dgvImportPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.m_dgvImportPreview.ColumnHeadersVisible = False
        Me.m_dgvImportPreview.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.m_dgvImportPreview.Location = New System.Drawing.Point(3, 140)
        Me.m_dgvImportPreview.Margin = New System.Windows.Forms.Padding(0)
        Me.m_dgvImportPreview.Name = "m_dgvImportPreview"
        Me.m_dgvImportPreview.ReadOnly = True
        Me.m_dgvImportPreview.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
        Me.m_dgvImportPreview.Size = New System.Drawing.Size(493, 254)
        Me.m_dgvImportPreview.TabIndex = 12
        '
        'm_hdrTarget
        '
        Me.m_hdrTarget.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrTarget.CanCollapseParent = False
        Me.m_hdrTarget.CollapsedParentHeight = 0
        Me.m_hdrTarget.IsCollapsed = False
        Me.m_hdrTarget.Location = New System.Drawing.Point(1, 398)
        Me.m_hdrTarget.Name = "m_hdrTarget"
        Me.m_hdrTarget.Size = New System.Drawing.Size(499, 18)
        Me.m_hdrTarget.TabIndex = 14
        Me.m_hdrTarget.Text = "Target"
        Me.m_hdrTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_hdrPreview
        '
        Me.m_hdrPreview.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrPreview.CanCollapseParent = False
        Me.m_hdrPreview.CollapsedParentHeight = 0
        Me.m_hdrPreview.IsCollapsed = False
        Me.m_hdrPreview.Location = New System.Drawing.Point(0, 122)
        Me.m_hdrPreview.Name = "m_hdrPreview"
        Me.m_hdrPreview.Size = New System.Drawing.Size(499, 18)
        Me.m_hdrPreview.TabIndex = 11
        Me.m_hdrPreview.Text = "Preview"
        Me.m_hdrPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_hdrSource
        '
        Me.m_hdrSource.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrSource.CanCollapseParent = False
        Me.m_hdrSource.CollapsedParentHeight = 0
        Me.m_hdrSource.IsCollapsed = False
        Me.m_hdrSource.Location = New System.Drawing.Point(0, 0)
        Me.m_hdrSource.Name = "m_hdrSource"
        Me.m_hdrSource.Size = New System.Drawing.Size(499, 18)
        Me.m_hdrSource.TabIndex = 0
        Me.m_hdrSource.Text = "Source"
        Me.m_hdrSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_tbImportSeparator
        '
        Me.m_tbImportSeparator.AcceptsReturn = True
        Me.m_tbImportSeparator.AcceptsTab = True
        Me.m_tbImportSeparator.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tbImportSeparator.Character = Global.Microsoft.VisualBasic.ChrW(46)
        Me.m_tbImportSeparator.CharacterMask = ""
        Me.m_tbImportSeparator.CharCode = 46
        Me.m_tbImportSeparator.Location = New System.Drawing.Point(329, 68)
        Me.m_tbImportSeparator.MaskInclusive = False
        Me.m_tbImportSeparator.Multiline = True
        Me.m_tbImportSeparator.Name = "m_tbImportSeparator"
        Me.m_tbImportSeparator.ShortcutsEnabled = False
        Me.m_tbImportSeparator.Size = New System.Drawing.Size(90, 20)
        Me.m_tbImportSeparator.TabIndex = 8
        Me.m_tbImportSeparator.Text = ". (period)"
        '
        'm_tbImportDelimiter
        '
        Me.m_tbImportDelimiter.AcceptsReturn = True
        Me.m_tbImportDelimiter.AcceptsTab = True
        Me.m_tbImportDelimiter.Character = Global.Microsoft.VisualBasic.ChrW(44)
        Me.m_tbImportDelimiter.CharacterMask = ""
        Me.m_tbImportDelimiter.CharCode = 44
        Me.m_tbImportDelimiter.Location = New System.Drawing.Point(74, 68)
        Me.m_tbImportDelimiter.MaskInclusive = False
        Me.m_tbImportDelimiter.MaxLength = 10
        Me.m_tbImportDelimiter.Multiline = True
        Me.m_tbImportDelimiter.Name = "m_tbImportDelimiter"
        Me.m_tbImportDelimiter.ShortcutsEnabled = False
        Me.m_tbImportDelimiter.Size = New System.Drawing.Size(90, 20)
        Me.m_tbImportDelimiter.TabIndex = 6
        Me.m_tbImportDelimiter.Text = ", (comma)"
        '
        'm_tbImportAuthor
        '
        Me.m_tbImportAuthor.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tbImportAuthor.Location = New System.Drawing.Point(72, 524)
        Me.m_tbImportAuthor.Name = "m_tbImportAuthor"
        Me.m_tbImportAuthor.Size = New System.Drawing.Size(424, 20)
        Me.m_tbImportAuthor.TabIndex = 22
        '
        'm_tbImportContact
        '
        Me.m_tbImportContact.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tbImportContact.Location = New System.Drawing.Point(72, 446)
        Me.m_tbImportContact.Multiline = True
        Me.m_tbImportContact.Name = "m_tbImportContact"
        Me.m_tbImportContact.Size = New System.Drawing.Size(424, 46)
        Me.m_tbImportContact.TabIndex = 18
        '
        'm_tbImportDescription
        '
        Me.m_tbImportDescription.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tbImportDescription.Location = New System.Drawing.Point(72, 498)
        Me.m_tbImportDescription.Name = "m_tbImportDescription"
        Me.m_tbImportDescription.Size = New System.Drawing.Size(424, 20)
        Me.m_tbImportDescription.TabIndex = 20
        '
        'm_tbImportFileName
        '
        Me.m_tbImportFileName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tbImportFileName.Location = New System.Drawing.Point(75, 21)
        Me.m_tbImportFileName.Name = "m_tbImportFileName"
        Me.m_tbImportFileName.Size = New System.Drawing.Size(344, 20)
        Me.m_tbImportFileName.TabIndex = 2
        '
        'm_lblImportContact
        '
        Me.m_lblImportContact.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.m_lblImportContact.AutoSize = True
        Me.m_lblImportContact.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblImportContact.Location = New System.Drawing.Point(3, 527)
        Me.m_lblImportContact.Name = "m_lblImportContact"
        Me.m_lblImportContact.Size = New System.Drawing.Size(47, 13)
        Me.m_lblImportContact.TabIndex = 21
        Me.m_lblImportContact.Text = "Con&tact:"
        '
        'm_lblImportAuthor
        '
        Me.m_lblImportAuthor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.m_lblImportAuthor.AutoSize = True
        Me.m_lblImportAuthor.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblImportAuthor.Location = New System.Drawing.Point(3, 501)
        Me.m_lblImportAuthor.Name = "m_lblImportAuthor"
        Me.m_lblImportAuthor.Size = New System.Drawing.Size(41, 13)
        Me.m_lblImportAuthor.TabIndex = 19
        Me.m_lblImportAuthor.Text = "A&uthor:"
        '
        'm_lblImportDescription
        '
        Me.m_lblImportDescription.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.m_lblImportDescription.AutoSize = True
        Me.m_lblImportDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblImportDescription.Location = New System.Drawing.Point(3, 449)
        Me.m_lblImportDescription.Name = "m_lblImportDescription"
        Me.m_lblImportDescription.Size = New System.Drawing.Size(63, 13)
        Me.m_lblImportDescription.TabIndex = 17
        Me.m_lblImportDescription.Text = "Desc&ription:"
        '
        'm_cmbImportDataset
        '
        Me.m_cmbImportDataset.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_cmbImportDataset.FormattingEnabled = True
        Me.m_cmbImportDataset.Location = New System.Drawing.Point(72, 419)
        Me.m_cmbImportDataset.Name = "m_cmbImportDataset"
        Me.m_cmbImportDataset.Size = New System.Drawing.Size(424, 21)
        Me.m_cmbImportDataset.TabIndex = 16
        '
        'm_lblImportDataset
        '
        Me.m_lblImportDataset.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.m_lblImportDataset.AutoSize = True
        Me.m_lblImportDataset.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblImportDataset.Location = New System.Drawing.Point(3, 422)
        Me.m_lblImportDataset.Name = "m_lblImportDataset"
        Me.m_lblImportDataset.Size = New System.Drawing.Size(47, 13)
        Me.m_lblImportDataset.TabIndex = 15
        Me.m_lblImportDataset.Text = "D&ataset:"
        '
        'm_lblImportDecimalSeparator
        '
        Me.m_lblImportDecimalSeparator.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_lblImportDecimalSeparator.AutoSize = True
        Me.m_lblImportDecimalSeparator.Location = New System.Drawing.Point(228, 71)
        Me.m_lblImportDecimalSeparator.Name = "m_lblImportDecimalSeparator"
        Me.m_lblImportDecimalSeparator.Size = New System.Drawing.Size(95, 13)
        Me.m_lblImportDecimalSeparator.TabIndex = 7
        Me.m_lblImportDecimalSeparator.Text = "D&ecimal separator:"
        '
        'm_lblImportDelimiter
        '
        Me.m_lblImportDelimiter.AutoSize = True
        Me.m_lblImportDelimiter.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblImportDelimiter.Location = New System.Drawing.Point(3, 71)
        Me.m_lblImportDelimiter.Name = "m_lblImportDelimiter"
        Me.m_lblImportDelimiter.Size = New System.Drawing.Size(50, 13)
        Me.m_lblImportDelimiter.TabIndex = 5
        Me.m_lblImportDelimiter.Text = "&Delimiter:"
        '
        'm_rbImportSourceClipboard
        '
        Me.m_rbImportSourceClipboard.AutoSize = True
        Me.m_rbImportSourceClipboard.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_rbImportSourceClipboard.Location = New System.Drawing.Point(6, 45)
        Me.m_rbImportSourceClipboard.Name = "m_rbImportSourceClipboard"
        Me.m_rbImportSourceClipboard.Size = New System.Drawing.Size(69, 17)
        Me.m_rbImportSourceClipboard.TabIndex = 4
        Me.m_rbImportSourceClipboard.Text = "&Clipboard"
        Me.m_rbImportSourceClipboard.UseVisualStyleBackColor = True
        '
        'm_btnImportBrowse
        '
        Me.m_btnImportBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnImportBrowse.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_btnImportBrowse.Location = New System.Drawing.Point(425, 19)
        Me.m_btnImportBrowse.Name = "m_btnImportBrowse"
        Me.m_btnImportBrowse.Size = New System.Drawing.Size(75, 23)
        Me.m_btnImportBrowse.TabIndex = 3
        Me.m_btnImportBrowse.Text = "&Browse..."
        Me.m_btnImportBrowse.UseVisualStyleBackColor = True
        '
        'm_rbImportSourceTextFile
        '
        Me.m_rbImportSourceTextFile.AutoSize = True
        Me.m_rbImportSourceTextFile.Checked = True
        Me.m_rbImportSourceTextFile.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_rbImportSourceTextFile.Location = New System.Drawing.Point(6, 22)
        Me.m_rbImportSourceTextFile.Name = "m_rbImportSourceTextFile"
        Me.m_rbImportSourceTextFile.Size = New System.Drawing.Size(65, 17)
        Me.m_rbImportSourceTextFile.TabIndex = 1
        Me.m_rbImportSourceTextFile.TabStop = True
        Me.m_rbImportSourceTextFile.Text = "Text &file:"
        Me.m_rbImportSourceTextFile.UseVisualStyleBackColor = True
        '
        'm_tpWeights
        '
        Me.m_tpWeights.Controls.Add(Me.m_gridWeights)
        Me.m_tpWeights.Controls.Add(Me.m_btnApplyCheckNone)
        Me.m_tpWeights.Controls.Add(Me.m_btnApplyCheckAll)
        Me.m_tpWeights.ImageIndex = 1
        Me.m_tpWeights.Location = New System.Drawing.Point(4, 26)
        Me.m_tpWeights.Name = "m_tpWeights"
        Me.m_tpWeights.Padding = New System.Windows.Forms.Padding(0, 3, 0, 3)
        Me.m_tpWeights.Size = New System.Drawing.Size(499, 570)
        Me.m_tpWeights.TabIndex = 1
        Me.m_tpWeights.Text = "Weights"
        Me.m_tpWeights.UseVisualStyleBackColor = True
        '
        'm_gridWeights
        '
        Me.m_gridWeights.AllowBlockSelect = True
        Me.m_gridWeights.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_gridWeights.AutoSizeMinHeight = 10
        Me.m_gridWeights.AutoSizeMinWidth = 10
        Me.m_gridWeights.AutoStretchColumnsToFitWidth = True
        Me.m_gridWeights.AutoStretchRowsToFitHeight = False
        Me.m_gridWeights.BackColor = System.Drawing.Color.White
        Me.m_gridWeights.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_gridWeights.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_gridWeights.CustomSort = False
        Me.m_gridWeights.DataName = "grid content"
        Me.m_gridWeights.FixedColumnWidths = False
        Me.m_gridWeights.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_gridWeights.GridToolTipActive = True
        Me.m_gridWeights.IsLayoutSuspended = False
        Me.m_gridWeights.IsOutputGrid = True
        Me.m_gridWeights.Location = New System.Drawing.Point(3, 3)
        Me.m_gridWeights.Name = "m_gridWeights"
        Me.m_gridWeights.Size = New System.Drawing.Size(420, 578)
        Me.m_gridWeights.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_gridWeights.TabIndex = 0
        Me.m_gridWeights.UIContext = Nothing
        '
        'm_btnApplyCheckNone
        '
        Me.m_btnApplyCheckNone.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnApplyCheckNone.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_btnApplyCheckNone.Location = New System.Drawing.Point(429, 32)
        Me.m_btnApplyCheckNone.Name = "m_btnApplyCheckNone"
        Me.m_btnApplyCheckNone.Size = New System.Drawing.Size(67, 23)
        Me.m_btnApplyCheckNone.TabIndex = 2
        Me.m_btnApplyCheckNone.Text = "None"
        Me.m_btnApplyCheckNone.UseVisualStyleBackColor = True
        '
        'm_btnApplyCheckAll
        '
        Me.m_btnApplyCheckAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnApplyCheckAll.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_btnApplyCheckAll.Location = New System.Drawing.Point(429, 3)
        Me.m_btnApplyCheckAll.Name = "m_btnApplyCheckAll"
        Me.m_btnApplyCheckAll.Size = New System.Drawing.Size(67, 23)
        Me.m_btnApplyCheckAll.TabIndex = 1
        Me.m_btnApplyCheckAll.Text = "All"
        Me.m_btnApplyCheckAll.UseVisualStyleBackColor = True
        '
        'm_tpLoad
        '
        Me.m_tpLoad.Controls.Add(Me.m_cbLoadEnableOnLoad)
        Me.m_tpLoad.Controls.Add(Me.m_lvLoadDatasets)
        Me.m_tpLoad.ImageIndex = 0
        Me.m_tpLoad.Location = New System.Drawing.Point(4, 26)
        Me.m_tpLoad.Name = "m_tpLoad"
        Me.m_tpLoad.Padding = New System.Windows.Forms.Padding(3)
        Me.m_tpLoad.Size = New System.Drawing.Size(499, 570)
        Me.m_tpLoad.TabIndex = 0
        Me.m_tpLoad.Text = "Load"
        Me.m_tpLoad.UseVisualStyleBackColor = True
        '
        'm_cbLoadEnableOnLoad
        '
        Me.m_cbLoadEnableOnLoad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.m_cbLoadEnableOnLoad.AutoSize = True
        Me.m_cbLoadEnableOnLoad.Checked = True
        Me.m_cbLoadEnableOnLoad.CheckState = System.Windows.Forms.CheckState.Checked
        Me.m_cbLoadEnableOnLoad.Location = New System.Drawing.Point(3, 547)
        Me.m_cbLoadEnableOnLoad.Name = "m_cbLoadEnableOnLoad"
        Me.m_cbLoadEnableOnLoad.Size = New System.Drawing.Size(181, 17)
        Me.m_cbLoadEnableOnLoad.TabIndex = 1
        Me.m_cbLoadEnableOnLoad.Text = "&Enable Time Series when loaded"
        Me.m_cbLoadEnableOnLoad.UseVisualStyleBackColor = True
        '
        'm_lvLoadDatasets
        '
        Me.m_lvLoadDatasets.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_lvLoadDatasets.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.m_colLoadDataset, Me.m_colLoaded, Me.m_clInterval, Me.m_colDescription})
        Me.m_lvLoadDatasets.FullRowSelect = True
        Me.m_lvLoadDatasets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.m_lvLoadDatasets.HideSelection = False
        Me.m_lvLoadDatasets.LabelWrap = False
        Me.m_lvLoadDatasets.Location = New System.Drawing.Point(0, 0)
        Me.m_lvLoadDatasets.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lvLoadDatasets.MultiSelect = False
        Me.m_lvLoadDatasets.Name = "m_lvLoadDatasets"
        Me.m_lvLoadDatasets.Size = New System.Drawing.Size(499, 544)
        Me.m_lvLoadDatasets.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.m_lvLoadDatasets.TabIndex = 0
        Me.m_lvLoadDatasets.UseCompatibleStateImageBehavior = False
        Me.m_lvLoadDatasets.View = System.Windows.Forms.View.Details
        '
        'm_colLoadDataset
        '
        Me.m_colLoadDataset.Text = "Dataset"
        Me.m_colLoadDataset.Width = 200
        '
        'm_colLoaded
        '
        Me.m_colLoaded.Text = "Loaded"
        Me.m_colLoaded.Width = 50
        '
        'm_clInterval
        '
        Me.m_clInterval.Text = "Interval"
        Me.m_clInterval.Width = 88
        '
        'm_colDescription
        '
        Me.m_colDescription.Text = "Number of Time Series"
        Me.m_colDescription.Width = 300
        '
        'm_tcMain
        '
        Me.m_tcMain.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_tcMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons
        Me.m_tcMain.Controls.Add(Me.m_tpLoad)
        Me.m_tcMain.Controls.Add(Me.m_tpWeights)
        Me.m_tcMain.Controls.Add(Me.m_tpImport)
        Me.m_tcMain.Controls.Add(Me.m_tpDelete)
        Me.m_tcMain.ImageList = Me.m_ilTabImages
        Me.m_tcMain.Location = New System.Drawing.Point(9, 9)
        Me.m_tcMain.Margin = New System.Windows.Forms.Padding(0)
        Me.m_tcMain.Name = "m_tcMain"
        Me.m_tcMain.SelectedIndex = 0
        Me.m_tcMain.Size = New System.Drawing.Size(507, 600)
        Me.m_tcMain.TabIndex = 0
        '
        'dlgManageTimeSeries
        '
        Me.AcceptButton = Me.m_btnOk
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.CancelButton = Me.m_btnCancel
        Me.ClientSize = New System.Drawing.Size(525, 650)
        Me.ControlBox = False
        Me.Controls.Add(Me.m_btnOk)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_tcMain)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(533, 500)
        Me.Name = "dlgManageTimeSeries"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Manage Time Series"
        Me.m_tpDelete.ResumeLayout(False)
        Me.m_tpImport.ResumeLayout(False)
        Me.m_tpImport.PerformLayout()
        CType(Me.m_dgvImportPreview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpWeights.ResumeLayout(False)
        Me.m_tpLoad.ResumeLayout(False)
        Me.m_tpLoad.PerformLayout()
        Me.m_tcMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_btnOk As System.Windows.Forms.Button
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_ilTabImages As System.Windows.Forms.ImageList
    Private WithEvents lbSettings As System.Windows.Forms.Label
    Private WithEvents m_tpDelete As System.Windows.Forms.TabPage
    Private WithEvents m_lvDeleteDatasets As System.Windows.Forms.ListView
    Private WithEvents m_colDeleteDataset As System.Windows.Forms.ColumnHeader
    Private WithEvents m_colDeleteLoaded As System.Windows.Forms.ColumnHeader
    Private WithEvents m_colDeleteNumSeries As System.Windows.Forms.ColumnHeader
    Private WithEvents m_tpImport As System.Windows.Forms.TabPage
    Private WithEvents m_tbImportSeparator As ScientificInterfaceShared.Controls.ucCharacterTextBox
    Private WithEvents m_tbImportDelimiter As ScientificInterfaceShared.Controls.ucCharacterTextBox
    Private WithEvents m_tbImportAuthor As System.Windows.Forms.TextBox
    Private WithEvents m_tbImportContact As System.Windows.Forms.TextBox
    Private WithEvents m_tbImportDescription As System.Windows.Forms.TextBox
    Private WithEvents m_tbImportFileName As System.Windows.Forms.TextBox
    Private WithEvents m_cbImportEnableOnImport As System.Windows.Forms.CheckBox
    Private WithEvents m_lblImportContact As System.Windows.Forms.Label
    Private WithEvents m_lblImportAuthor As System.Windows.Forms.Label
    Private WithEvents m_lblImportDescription As System.Windows.Forms.Label
    Private WithEvents m_cmbImportDataset As System.Windows.Forms.ComboBox
    Private WithEvents m_lblImportDataset As System.Windows.Forms.Label
    Private WithEvents m_dgvImportPreview As System.Windows.Forms.DataGridView
    Private WithEvents m_lblImportDecimalSeparator As System.Windows.Forms.Label
    Private WithEvents m_lblImportDelimiter As System.Windows.Forms.Label
    Private WithEvents m_rbImportSourceClipboard As System.Windows.Forms.RadioButton
    Private WithEvents m_btnImportBrowse As System.Windows.Forms.Button
    Private WithEvents m_rbImportSourceTextFile As System.Windows.Forms.RadioButton
    Private WithEvents m_tpWeights As System.Windows.Forms.TabPage
    Private WithEvents m_gridWeights As gridWeightTS
    Private WithEvents m_btnApplyCheckNone As System.Windows.Forms.Button
    Private WithEvents m_btnApplyCheckAll As System.Windows.Forms.Button
    Private WithEvents m_tpLoad As System.Windows.Forms.TabPage
    Private WithEvents m_cbLoadEnableOnLoad As System.Windows.Forms.CheckBox
    Private WithEvents m_lvLoadDatasets As System.Windows.Forms.ListView
    Private WithEvents m_colLoadDataset As System.Windows.Forms.ColumnHeader
    Private WithEvents m_colLoaded As System.Windows.Forms.ColumnHeader
    Private WithEvents m_colDescription As System.Windows.Forms.ColumnHeader
    Private WithEvents m_tcMain As System.Windows.Forms.TabControl
    Private WithEvents m_hdrPreview As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrSource As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrTarget As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_cmbImportInterval As System.Windows.Forms.ComboBox
    Private WithEvents m_lblImportInterval As System.Windows.Forms.Label
    Private WithEvents m_clInterval As System.Windows.Forms.ColumnHeader

End Class
