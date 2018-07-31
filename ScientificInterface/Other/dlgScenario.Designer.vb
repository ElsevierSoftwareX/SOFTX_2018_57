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

Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Wizard

    Partial Class dlgScenario
        Inherits System.Windows.Forms.Form

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
            Dim chCreate2 As System.Windows.Forms.ColumnHeader
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgScenario))
            Dim chCreate3 As System.Windows.Forms.ColumnHeader
            Dim chLoad2 As System.Windows.Forms.ColumnHeader
            Dim chLoad3 As System.Windows.Forms.ColumnHeader
            Dim chDelete2 As System.Windows.Forms.ColumnHeader
            Dim chDelete3 As System.Windows.Forms.ColumnHeader
            Dim chSaveAs2 As System.Windows.Forms.ColumnHeader
            Dim chSaveAs3 As System.Windows.Forms.ColumnHeader
            Me.lblDescriptionCreate = New System.Windows.Forms.Label()
            Me.lblNameCreate = New System.Windows.Forms.Label()
            Me.tbNameCreate = New System.Windows.Forms.TextBox()
            Me.tbDescriptionCreate = New System.Windows.Forms.TextBox()
            Me.m_cmsListBox = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.tsmCreate = New System.Windows.Forms.ToolStripMenuItem()
            Me.tsmLoad = New System.Windows.Forms.ToolStripMenuItem()
            Me.tsmSave = New System.Windows.Forms.ToolStripMenuItem()
            Me.tsmRename = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
            Me.tsmDelete = New System.Windows.Forms.ToolStripMenuItem()
            Me.btnCancelCreate = New System.Windows.Forms.Button()
            Me.btnDelete = New System.Windows.Forms.Button()
            Me.tabctrlModes = New System.Windows.Forms.TabControl()
            Me.tabpageCreate = New System.Windows.Forms.TabPage()
            Me.lvCreate = New System.Windows.Forms.ListView()
            Me.chCreate1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.UcFormSeparator2 = New ScientificInterfaceShared.Controls.ucFormSeparator()
            Me.lblScenariosCreate = New System.Windows.Forms.Label()
            Me.lbContactCreate = New System.Windows.Forms.Label()
            Me.lbAuthorCreate = New System.Windows.Forms.Label()
            Me.tbAuthorCreate = New System.Windows.Forms.TextBox()
            Me.tbContactCreate = New System.Windows.Forms.TextBox()
            Me.btnCreate = New System.Windows.Forms.Button()
            Me.tabpageLoad = New System.Windows.Forms.TabPage()
            Me.lvLoad = New System.Windows.Forms.ListView()
            Me.chLoad1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.btnCancelLoad = New System.Windows.Forms.Button()
            Me.btnLoad = New System.Windows.Forms.Button()
            Me.lblScenariosLoad = New System.Windows.Forms.Label()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.lbAuthorLoad = New System.Windows.Forms.Label()
            Me.lblDescriptionLoad = New System.Windows.Forms.Label()
            Me.tbContactLoad = New System.Windows.Forms.TextBox()
            Me.tbAuthorLoad = New System.Windows.Forms.TextBox()
            Me.tbDescriptionLoad = New System.Windows.Forms.TextBox()
            Me.tabpageDelete = New System.Windows.Forms.TabPage()
            Me.lvDelete = New System.Windows.Forms.ListView()
            Me.chDelete1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.lbContactDelete = New System.Windows.Forms.Label()
            Me.lbAuthorDelete = New System.Windows.Forms.Label()
            Me.lbDescriptionDelete = New System.Windows.Forms.Label()
            Me.tbContactDelete = New System.Windows.Forms.TextBox()
            Me.tbAuthorDelete = New System.Windows.Forms.TextBox()
            Me.tbDescriptionDelete = New System.Windows.Forms.TextBox()
            Me.lblScenariosDelete = New System.Windows.Forms.Label()
            Me.btnCancelDelete = New System.Windows.Forms.Button()
            Me.tabpageSaveAs = New System.Windows.Forms.TabPage()
            Me.lvSaveAs = New System.Windows.Forms.ListView()
            Me.chSaveAs1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.lbContactSaveAs = New System.Windows.Forms.Label()
            Me.lbAuthorSaveAs = New System.Windows.Forms.Label()
            Me.tbAuthorSaveAs = New System.Windows.Forms.TextBox()
            Me.tbContactSaveAs = New System.Windows.Forms.TextBox()
            Me.btnCancelSave = New System.Windows.Forms.Button()
            Me.btnSave = New System.Windows.Forms.Button()
            Me.lblScenarioSaveAs = New System.Windows.Forms.Label()
            Me.lblDescriptionSaveAs = New System.Windows.Forms.Label()
            Me.tbNameSaveAs = New System.Windows.Forms.TextBox()
            Me.lblNameSaveAs = New System.Windows.Forms.Label()
            Me.tbDescriptionSaveAs = New System.Windows.Forms.TextBox()
            Me.UcFormSeparator1 = New ScientificInterfaceShared.Controls.ucFormSeparator()
            Me.ilPurdyPics = New System.Windows.Forms.ImageList(Me.components)
            chCreate2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chCreate3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chLoad2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chLoad3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chDelete2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chDelete3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chSaveAs2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            chSaveAs3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_cmsListBox.SuspendLayout()
            Me.tabctrlModes.SuspendLayout()
            Me.tabpageCreate.SuspendLayout()
            Me.tabpageLoad.SuspendLayout()
            Me.tabpageDelete.SuspendLayout()
            Me.tabpageSaveAs.SuspendLayout()
            Me.SuspendLayout()
            '
            'chCreate2
            '
            resources.ApplyResources(chCreate2, "chCreate2")
            '
            'chCreate3
            '
            resources.ApplyResources(chCreate3, "chCreate3")
            '
            'chLoad2
            '
            resources.ApplyResources(chLoad2, "chLoad2")
            '
            'chLoad3
            '
            resources.ApplyResources(chLoad3, "chLoad3")
            '
            'chDelete2
            '
            resources.ApplyResources(chDelete2, "chDelete2")
            '
            'chDelete3
            '
            resources.ApplyResources(chDelete3, "chDelete3")
            '
            'chSaveAs2
            '
            resources.ApplyResources(chSaveAs2, "chSaveAs2")
            '
            'chSaveAs3
            '
            resources.ApplyResources(chSaveAs3, "chSaveAs3")
            '
            'lblDescriptionCreate
            '
            resources.ApplyResources(Me.lblDescriptionCreate, "lblDescriptionCreate")
            Me.lblDescriptionCreate.Name = "lblDescriptionCreate"
            '
            'lblNameCreate
            '
            resources.ApplyResources(Me.lblNameCreate, "lblNameCreate")
            Me.lblNameCreate.Name = "lblNameCreate"
            '
            'tbNameCreate
            '
            resources.ApplyResources(Me.tbNameCreate, "tbNameCreate")
            Me.tbNameCreate.Name = "tbNameCreate"
            '
            'tbDescriptionCreate
            '
            resources.ApplyResources(Me.tbDescriptionCreate, "tbDescriptionCreate")
            Me.tbDescriptionCreate.Name = "tbDescriptionCreate"
            '
            'm_cmsListBox
            '
            Me.m_cmsListBox.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmCreate, Me.tsmLoad, Me.tsmSave, Me.tsmRename, Me.ToolStripSeparator1, Me.tsmDelete})
            Me.m_cmsListBox.Name = "m_cmsListBox"
            resources.ApplyResources(Me.m_cmsListBox, "m_cmsListBox")
            '
            'tsmCreate
            '
            resources.ApplyResources(Me.tsmCreate, "tsmCreate")
            Me.tsmCreate.Name = "tsmCreate"
            '
            'tsmLoad
            '
            resources.ApplyResources(Me.tsmLoad, "tsmLoad")
            Me.tsmLoad.Name = "tsmLoad"
            '
            'tsmSave
            '
            resources.ApplyResources(Me.tsmSave, "tsmSave")
            Me.tsmSave.Name = "tsmSave"
            '
            'tsmRename
            '
            Me.tsmRename.Name = "tsmRename"
            resources.ApplyResources(Me.tsmRename, "tsmRename")
            '
            'ToolStripSeparator1
            '
            Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
            resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
            '
            'tsmDelete
            '
            resources.ApplyResources(Me.tsmDelete, "tsmDelete")
            Me.tsmDelete.Name = "tsmDelete"
            '
            'btnCancelCreate
            '
            resources.ApplyResources(Me.btnCancelCreate, "btnCancelCreate")
            Me.btnCancelCreate.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.btnCancelCreate.Name = "btnCancelCreate"
            '
            'btnDelete
            '
            resources.ApplyResources(Me.btnDelete, "btnDelete")
            Me.btnDelete.Name = "btnDelete"
            Me.btnDelete.UseVisualStyleBackColor = True
            '
            'tabctrlModes
            '
            resources.ApplyResources(Me.tabctrlModes, "tabctrlModes")
            Me.tabctrlModes.Controls.Add(Me.tabpageCreate)
            Me.tabctrlModes.Controls.Add(Me.tabpageLoad)
            Me.tabctrlModes.Controls.Add(Me.tabpageDelete)
            Me.tabctrlModes.Controls.Add(Me.tabpageSaveAs)
            Me.tabctrlModes.ImageList = Me.ilPurdyPics
            Me.tabctrlModes.Name = "tabctrlModes"
            Me.tabctrlModes.SelectedIndex = 0
            '
            'tabpageCreate
            '
            Me.tabpageCreate.Controls.Add(Me.lvCreate)
            Me.tabpageCreate.Controls.Add(Me.UcFormSeparator2)
            Me.tabpageCreate.Controls.Add(Me.lblScenariosCreate)
            Me.tabpageCreate.Controls.Add(Me.lbContactCreate)
            Me.tabpageCreate.Controls.Add(Me.lbAuthorCreate)
            Me.tabpageCreate.Controls.Add(Me.lblDescriptionCreate)
            Me.tabpageCreate.Controls.Add(Me.tbNameCreate)
            Me.tabpageCreate.Controls.Add(Me.btnCancelCreate)
            Me.tabpageCreate.Controls.Add(Me.lblNameCreate)
            Me.tabpageCreate.Controls.Add(Me.tbAuthorCreate)
            Me.tabpageCreate.Controls.Add(Me.tbContactCreate)
            Me.tabpageCreate.Controls.Add(Me.tbDescriptionCreate)
            Me.tabpageCreate.Controls.Add(Me.btnCreate)
            resources.ApplyResources(Me.tabpageCreate, "tabpageCreate")
            Me.tabpageCreate.Name = "tabpageCreate"
            Me.tabpageCreate.UseVisualStyleBackColor = True
            '
            'lvCreate
            '
            resources.ApplyResources(Me.lvCreate, "lvCreate")
            Me.lvCreate.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chCreate1, chCreate2, chCreate3})
            Me.lvCreate.ContextMenuStrip = Me.m_cmsListBox
            Me.lvCreate.FullRowSelect = True
            Me.lvCreate.HideSelection = False
            Me.lvCreate.LabelEdit = True
            Me.lvCreate.MultiSelect = False
            Me.lvCreate.Name = "lvCreate"
            Me.lvCreate.ShowGroups = False
            Me.lvCreate.Sorting = System.Windows.Forms.SortOrder.Ascending
            Me.lvCreate.UseCompatibleStateImageBehavior = False
            Me.lvCreate.View = System.Windows.Forms.View.Details
            '
            'chCreate1
            '
            resources.ApplyResources(Me.chCreate1, "chCreate1")
            '
            'UcFormSeparator2
            '
            resources.ApplyResources(Me.UcFormSeparator2, "UcFormSeparator2")
            Me.UcFormSeparator2.Horizontal = True
            Me.UcFormSeparator2.Name = "UcFormSeparator2"
            Me.UcFormSeparator2.TabStop = False
            '
            'lblScenariosCreate
            '
            resources.ApplyResources(Me.lblScenariosCreate, "lblScenariosCreate")
            Me.lblScenariosCreate.Name = "lblScenariosCreate"
            '
            'lbContactCreate
            '
            resources.ApplyResources(Me.lbContactCreate, "lbContactCreate")
            Me.lbContactCreate.Name = "lbContactCreate"
            '
            'lbAuthorCreate
            '
            resources.ApplyResources(Me.lbAuthorCreate, "lbAuthorCreate")
            Me.lbAuthorCreate.Name = "lbAuthorCreate"
            '
            'tbAuthorCreate
            '
            resources.ApplyResources(Me.tbAuthorCreate, "tbAuthorCreate")
            Me.tbAuthorCreate.Name = "tbAuthorCreate"
            '
            'tbContactCreate
            '
            resources.ApplyResources(Me.tbContactCreate, "tbContactCreate")
            Me.tbContactCreate.Name = "tbContactCreate"
            '
            'btnCreate
            '
            resources.ApplyResources(Me.btnCreate, "btnCreate")
            Me.btnCreate.Name = "btnCreate"
            '
            'tabpageLoad
            '
            Me.tabpageLoad.Controls.Add(Me.lvLoad)
            Me.tabpageLoad.Controls.Add(Me.btnCancelLoad)
            Me.tabpageLoad.Controls.Add(Me.btnLoad)
            Me.tabpageLoad.Controls.Add(Me.lblScenariosLoad)
            Me.tabpageLoad.Controls.Add(Me.Label1)
            Me.tabpageLoad.Controls.Add(Me.lbAuthorLoad)
            Me.tabpageLoad.Controls.Add(Me.lblDescriptionLoad)
            Me.tabpageLoad.Controls.Add(Me.tbContactLoad)
            Me.tabpageLoad.Controls.Add(Me.tbAuthorLoad)
            Me.tabpageLoad.Controls.Add(Me.tbDescriptionLoad)
            resources.ApplyResources(Me.tabpageLoad, "tabpageLoad")
            Me.tabpageLoad.Name = "tabpageLoad"
            Me.tabpageLoad.UseVisualStyleBackColor = True
            '
            'lvLoad
            '
            resources.ApplyResources(Me.lvLoad, "lvLoad")
            Me.lvLoad.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chLoad1, chLoad2, chLoad3})
            Me.lvLoad.ContextMenuStrip = Me.m_cmsListBox
            Me.lvLoad.FullRowSelect = True
            Me.lvLoad.HideSelection = False
            Me.lvLoad.MultiSelect = False
            Me.lvLoad.Name = "lvLoad"
            Me.lvLoad.ShowGroups = False
            Me.lvLoad.Sorting = System.Windows.Forms.SortOrder.Ascending
            Me.lvLoad.UseCompatibleStateImageBehavior = False
            Me.lvLoad.View = System.Windows.Forms.View.Details
            '
            'chLoad1
            '
            resources.ApplyResources(Me.chLoad1, "chLoad1")
            '
            'btnCancelLoad
            '
            resources.ApplyResources(Me.btnCancelLoad, "btnCancelLoad")
            Me.btnCancelLoad.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.btnCancelLoad.Name = "btnCancelLoad"
            '
            'btnLoad
            '
            resources.ApplyResources(Me.btnLoad, "btnLoad")
            Me.btnLoad.Name = "btnLoad"
            '
            'lblScenariosLoad
            '
            resources.ApplyResources(Me.lblScenariosLoad, "lblScenariosLoad")
            Me.lblScenariosLoad.Name = "lblScenariosLoad"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'lbAuthorLoad
            '
            resources.ApplyResources(Me.lbAuthorLoad, "lbAuthorLoad")
            Me.lbAuthorLoad.Name = "lbAuthorLoad"
            '
            'lblDescriptionLoad
            '
            resources.ApplyResources(Me.lblDescriptionLoad, "lblDescriptionLoad")
            Me.lblDescriptionLoad.Name = "lblDescriptionLoad"
            '
            'tbContactLoad
            '
            resources.ApplyResources(Me.tbContactLoad, "tbContactLoad")
            Me.tbContactLoad.Name = "tbContactLoad"
            Me.tbContactLoad.ReadOnly = True
            '
            'tbAuthorLoad
            '
            resources.ApplyResources(Me.tbAuthorLoad, "tbAuthorLoad")
            Me.tbAuthorLoad.Name = "tbAuthorLoad"
            Me.tbAuthorLoad.ReadOnly = True
            '
            'tbDescriptionLoad
            '
            resources.ApplyResources(Me.tbDescriptionLoad, "tbDescriptionLoad")
            Me.tbDescriptionLoad.Name = "tbDescriptionLoad"
            Me.tbDescriptionLoad.ReadOnly = True
            '
            'tabpageDelete
            '
            Me.tabpageDelete.Controls.Add(Me.lvDelete)
            Me.tabpageDelete.Controls.Add(Me.lbContactDelete)
            Me.tabpageDelete.Controls.Add(Me.lbAuthorDelete)
            Me.tabpageDelete.Controls.Add(Me.lbDescriptionDelete)
            Me.tabpageDelete.Controls.Add(Me.tbContactDelete)
            Me.tabpageDelete.Controls.Add(Me.tbAuthorDelete)
            Me.tabpageDelete.Controls.Add(Me.tbDescriptionDelete)
            Me.tabpageDelete.Controls.Add(Me.lblScenariosDelete)
            Me.tabpageDelete.Controls.Add(Me.btnCancelDelete)
            Me.tabpageDelete.Controls.Add(Me.btnDelete)
            resources.ApplyResources(Me.tabpageDelete, "tabpageDelete")
            Me.tabpageDelete.Name = "tabpageDelete"
            Me.tabpageDelete.UseVisualStyleBackColor = True
            '
            'lvDelete
            '
            resources.ApplyResources(Me.lvDelete, "lvDelete")
            Me.lvDelete.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chDelete1, chDelete2, chDelete3})
            Me.lvDelete.ContextMenuStrip = Me.m_cmsListBox
            Me.lvDelete.FullRowSelect = True
            Me.lvDelete.HideSelection = False
            Me.lvDelete.MultiSelect = False
            Me.lvDelete.Name = "lvDelete"
            Me.lvDelete.ShowGroups = False
            Me.lvDelete.Sorting = System.Windows.Forms.SortOrder.Ascending
            Me.lvDelete.UseCompatibleStateImageBehavior = False
            Me.lvDelete.View = System.Windows.Forms.View.Details
            '
            'chDelete1
            '
            resources.ApplyResources(Me.chDelete1, "chDelete1")
            '
            'lbContactDelete
            '
            resources.ApplyResources(Me.lbContactDelete, "lbContactDelete")
            Me.lbContactDelete.Name = "lbContactDelete"
            '
            'lbAuthorDelete
            '
            resources.ApplyResources(Me.lbAuthorDelete, "lbAuthorDelete")
            Me.lbAuthorDelete.Name = "lbAuthorDelete"
            '
            'lbDescriptionDelete
            '
            resources.ApplyResources(Me.lbDescriptionDelete, "lbDescriptionDelete")
            Me.lbDescriptionDelete.Name = "lbDescriptionDelete"
            '
            'tbContactDelete
            '
            resources.ApplyResources(Me.tbContactDelete, "tbContactDelete")
            Me.tbContactDelete.Name = "tbContactDelete"
            Me.tbContactDelete.ReadOnly = True
            '
            'tbAuthorDelete
            '
            resources.ApplyResources(Me.tbAuthorDelete, "tbAuthorDelete")
            Me.tbAuthorDelete.Name = "tbAuthorDelete"
            Me.tbAuthorDelete.ReadOnly = True
            '
            'tbDescriptionDelete
            '
            resources.ApplyResources(Me.tbDescriptionDelete, "tbDescriptionDelete")
            Me.tbDescriptionDelete.Name = "tbDescriptionDelete"
            Me.tbDescriptionDelete.ReadOnly = True
            '
            'lblScenariosDelete
            '
            resources.ApplyResources(Me.lblScenariosDelete, "lblScenariosDelete")
            Me.lblScenariosDelete.Name = "lblScenariosDelete"
            '
            'btnCancelDelete
            '
            resources.ApplyResources(Me.btnCancelDelete, "btnCancelDelete")
            Me.btnCancelDelete.Name = "btnCancelDelete"
            Me.btnCancelDelete.UseVisualStyleBackColor = True
            '
            'tabpageSaveAs
            '
            Me.tabpageSaveAs.Controls.Add(Me.lvSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.lbContactSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.lbAuthorSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.tbAuthorSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.tbContactSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.btnCancelSave)
            Me.tabpageSaveAs.Controls.Add(Me.btnSave)
            Me.tabpageSaveAs.Controls.Add(Me.lblScenarioSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.lblDescriptionSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.tbNameSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.lblNameSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.tbDescriptionSaveAs)
            Me.tabpageSaveAs.Controls.Add(Me.UcFormSeparator1)
            resources.ApplyResources(Me.tabpageSaveAs, "tabpageSaveAs")
            Me.tabpageSaveAs.Name = "tabpageSaveAs"
            Me.tabpageSaveAs.UseVisualStyleBackColor = True
            '
            'lvSaveAs
            '
            resources.ApplyResources(Me.lvSaveAs, "lvSaveAs")
            Me.lvSaveAs.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chSaveAs1, chSaveAs2, chSaveAs3})
            Me.lvSaveAs.ContextMenuStrip = Me.m_cmsListBox
            Me.lvSaveAs.FullRowSelect = True
            Me.lvSaveAs.HideSelection = False
            Me.lvSaveAs.LabelEdit = True
            Me.lvSaveAs.MultiSelect = False
            Me.lvSaveAs.Name = "lvSaveAs"
            Me.lvSaveAs.ShowGroups = False
            Me.lvSaveAs.Sorting = System.Windows.Forms.SortOrder.Ascending
            Me.lvSaveAs.UseCompatibleStateImageBehavior = False
            Me.lvSaveAs.View = System.Windows.Forms.View.Details
            '
            'chSaveAs1
            '
            resources.ApplyResources(Me.chSaveAs1, "chSaveAs1")
            '
            'lbContactSaveAs
            '
            resources.ApplyResources(Me.lbContactSaveAs, "lbContactSaveAs")
            Me.lbContactSaveAs.Name = "lbContactSaveAs"
            '
            'lbAuthorSaveAs
            '
            resources.ApplyResources(Me.lbAuthorSaveAs, "lbAuthorSaveAs")
            Me.lbAuthorSaveAs.Name = "lbAuthorSaveAs"
            '
            'tbAuthorSaveAs
            '
            resources.ApplyResources(Me.tbAuthorSaveAs, "tbAuthorSaveAs")
            Me.tbAuthorSaveAs.Name = "tbAuthorSaveAs"
            '
            'tbContactSaveAs
            '
            resources.ApplyResources(Me.tbContactSaveAs, "tbContactSaveAs")
            Me.tbContactSaveAs.Name = "tbContactSaveAs"
            '
            'btnCancelSave
            '
            resources.ApplyResources(Me.btnCancelSave, "btnCancelSave")
            Me.btnCancelSave.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.btnCancelSave.Name = "btnCancelSave"
            '
            'btnSave
            '
            resources.ApplyResources(Me.btnSave, "btnSave")
            Me.btnSave.Name = "btnSave"
            '
            'lblScenarioSaveAs
            '
            resources.ApplyResources(Me.lblScenarioSaveAs, "lblScenarioSaveAs")
            Me.lblScenarioSaveAs.Name = "lblScenarioSaveAs"
            '
            'lblDescriptionSaveAs
            '
            resources.ApplyResources(Me.lblDescriptionSaveAs, "lblDescriptionSaveAs")
            Me.lblDescriptionSaveAs.Name = "lblDescriptionSaveAs"
            '
            'tbNameSaveAs
            '
            resources.ApplyResources(Me.tbNameSaveAs, "tbNameSaveAs")
            Me.tbNameSaveAs.Name = "tbNameSaveAs"
            '
            'lblNameSaveAs
            '
            resources.ApplyResources(Me.lblNameSaveAs, "lblNameSaveAs")
            Me.lblNameSaveAs.Name = "lblNameSaveAs"
            '
            'tbDescriptionSaveAs
            '
            resources.ApplyResources(Me.tbDescriptionSaveAs, "tbDescriptionSaveAs")
            Me.tbDescriptionSaveAs.Name = "tbDescriptionSaveAs"
            '
            'UcFormSeparator1
            '
            resources.ApplyResources(Me.UcFormSeparator1, "UcFormSeparator1")
            Me.UcFormSeparator1.Horizontal = True
            Me.UcFormSeparator1.Name = "UcFormSeparator1"
            Me.UcFormSeparator1.TabStop = False
            '
            'ilPurdyPics
            '
            Me.ilPurdyPics.ImageStream = CType(resources.GetObject("ilPurdyPics.ImageStream"), System.Windows.Forms.ImageListStreamer)
            Me.ilPurdyPics.TransparentColor = System.Drawing.Color.Transparent
            Me.ilPurdyPics.Images.SetKeyName(0, "")
            Me.ilPurdyPics.Images.SetKeyName(1, "")
            Me.ilPurdyPics.Images.SetKeyName(2, "")
            Me.ilPurdyPics.Images.SetKeyName(3, "DeleteHS.png")
            '
            'dlgScenario
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.tabctrlModes)
            Me.DoubleBuffered = True
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgScenario"
            Me.ShowInTaskbar = False
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
            Me.m_cmsListBox.ResumeLayout(False)
            Me.tabctrlModes.ResumeLayout(False)
            Me.tabpageCreate.ResumeLayout(False)
            Me.tabpageCreate.PerformLayout()
            Me.tabpageLoad.ResumeLayout(False)
            Me.tabpageLoad.PerformLayout()
            Me.tabpageDelete.ResumeLayout(False)
            Me.tabpageDelete.PerformLayout()
            Me.tabpageSaveAs.ResumeLayout(False)
            Me.tabpageSaveAs.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents lblDescriptionCreate As System.Windows.Forms.Label
        Friend WithEvents lblNameCreate As System.Windows.Forms.Label
        Friend WithEvents tbNameCreate As System.Windows.Forms.TextBox
        Friend WithEvents tbDescriptionCreate As System.Windows.Forms.TextBox
        Friend WithEvents btnCancelCreate As System.Windows.Forms.Button
        Friend WithEvents btnDelete As System.Windows.Forms.Button
        Friend WithEvents m_cmsListBox As System.Windows.Forms.ContextMenuStrip
        Friend WithEvents tsmCreate As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents tsmLoad As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents tsmSave As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents tsmRename As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
        Friend WithEvents tsmDelete As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents tabctrlModes As System.Windows.Forms.TabControl
        Friend WithEvents tabpageCreate As System.Windows.Forms.TabPage
        Friend WithEvents tabpageLoad As System.Windows.Forms.TabPage
        Friend WithEvents tabpageSaveAs As System.Windows.Forms.TabPage
        Friend WithEvents ilPurdyPics As System.Windows.Forms.ImageList
        Friend WithEvents btnCreate As System.Windows.Forms.Button
        Friend WithEvents lblDescriptionSaveAs As System.Windows.Forms.Label
        Friend WithEvents tbNameSaveAs As System.Windows.Forms.TextBox
        Friend WithEvents lblNameSaveAs As System.Windows.Forms.Label
        Friend WithEvents tbDescriptionSaveAs As System.Windows.Forms.TextBox
        Friend WithEvents lblScenariosCreate As System.Windows.Forms.Label
        Friend WithEvents lblDescriptionLoad As System.Windows.Forms.Label
        Friend WithEvents tbDescriptionLoad As System.Windows.Forms.TextBox
        Friend WithEvents lblScenariosLoad As System.Windows.Forms.Label
        Friend WithEvents lblScenarioSaveAs As System.Windows.Forms.Label
        Friend WithEvents tabpageDelete As System.Windows.Forms.TabPage
        Friend WithEvents lblScenariosDelete As System.Windows.Forms.Label
        Friend WithEvents btnCancelLoad As System.Windows.Forms.Button
        Friend WithEvents btnLoad As System.Windows.Forms.Button
        Friend WithEvents btnCancelSave As System.Windows.Forms.Button
        Friend WithEvents btnSave As System.Windows.Forms.Button
        Friend WithEvents btnCancelDelete As System.Windows.Forms.Button
        Private WithEvents UcFormSeparator1 As ucFormSeparator
        Private WithEvents UcFormSeparator2 As ucFormSeparator
        Friend WithEvents lbContactCreate As System.Windows.Forms.Label
        Friend WithEvents lbAuthorCreate As System.Windows.Forms.Label
        Friend WithEvents tbAuthorCreate As System.Windows.Forms.TextBox
        Friend WithEvents tbContactCreate As System.Windows.Forms.TextBox
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents lbAuthorLoad As System.Windows.Forms.Label
        Friend WithEvents tbContactLoad As System.Windows.Forms.TextBox
        Friend WithEvents tbAuthorLoad As System.Windows.Forms.TextBox
        Friend WithEvents lbContactDelete As System.Windows.Forms.Label
        Friend WithEvents lbAuthorDelete As System.Windows.Forms.Label
        Friend WithEvents lbDescriptionDelete As System.Windows.Forms.Label
        Friend WithEvents tbContactDelete As System.Windows.Forms.TextBox
        Friend WithEvents tbAuthorDelete As System.Windows.Forms.TextBox
        Friend WithEvents tbDescriptionDelete As System.Windows.Forms.TextBox
        Friend WithEvents lbContactSaveAs As System.Windows.Forms.Label
        Friend WithEvents lbAuthorSaveAs As System.Windows.Forms.Label
        Friend WithEvents tbAuthorSaveAs As System.Windows.Forms.TextBox
        Friend WithEvents tbContactSaveAs As System.Windows.Forms.TextBox
        Friend WithEvents lvCreate As System.Windows.Forms.ListView
        Friend WithEvents chCreate1 As System.Windows.Forms.ColumnHeader
        Friend WithEvents lvLoad As System.Windows.Forms.ListView
        Friend WithEvents chLoad1 As System.Windows.Forms.ColumnHeader
        Friend WithEvents lvDelete As System.Windows.Forms.ListView
        Friend WithEvents chDelete1 As System.Windows.Forms.ColumnHeader
        Friend WithEvents lvSaveAs As System.Windows.Forms.ListView
        Friend WithEvents chSaveAs1 As System.Windows.Forms.ColumnHeader

    End Class

End Namespace


