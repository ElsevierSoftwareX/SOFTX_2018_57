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

Partial Class dlgDefineTaxonomy
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgDefineTaxonomy))
        Me.m_btnDefine = New System.Windows.Forms.Button()
        Me.m_btnRemove = New System.Windows.Forms.Button()
        Me.m_btnKeep = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.m_lblSearchTerm = New System.Windows.Forms.Label()
        Me.m_tbxSearchTerm = New System.Windows.Forms.TextBox()
        Me.m_cmbEngine = New System.Windows.Forms.ComboBox()
        Me.m_btnConfigure = New System.Windows.Forms.Button()
        Me.m_cbIncludeExtent = New System.Windows.Forms.CheckBox()
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_gridGroups = New ScientificInterface.gridDefineTaxonomy()
        Me.m_hdrEdit = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_btnProps = New System.Windows.Forms.Button()
        Me.m_hdrProps = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrSearch = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_pbSearching = New System.Windows.Forms.PictureBox()
        Me.m_cmbFilter = New System.Windows.Forms.ComboBox()
        Me.m_btnAdd = New System.Windows.Forms.Button()
        Me.m_lblIn = New System.Windows.Forms.Label()
        Me.m_gridResults = New ScientificInterface.gridTaxonSearchResults()
        Me.m_lblEngine = New System.Windows.Forms.Label()
        Me.m_cbShowCodes = New System.Windows.Forms.CheckBox()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        CType(Me.m_pbSearching, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_btnDefine
        '
        resources.ApplyResources(Me.m_btnDefine, "m_btnDefine")
        Me.m_btnDefine.Name = "m_btnDefine"
        Me.m_btnDefine.UseVisualStyleBackColor = True
        '
        'm_btnRemove
        '
        resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
        Me.m_btnRemove.Name = "m_btnRemove"
        Me.m_btnRemove.UseVisualStyleBackColor = True
        '
        'm_btnKeep
        '
        resources.ApplyResources(Me.m_btnKeep, "m_btnKeep")
        Me.m_btnKeep.Name = "m_btnKeep"
        Me.m_btnKeep.UseVisualStyleBackColor = True
        '
        'Cancel_Button
        '
        resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Name = "Cancel_Button"
        '
        'OK_Button
        '
        resources.ApplyResources(Me.OK_Button, "OK_Button")
        Me.OK_Button.Name = "OK_Button"
        '
        'm_lblSearchTerm
        '
        resources.ApplyResources(Me.m_lblSearchTerm, "m_lblSearchTerm")
        Me.m_lblSearchTerm.Name = "m_lblSearchTerm"
        '
        'm_tbxSearchTerm
        '
        resources.ApplyResources(Me.m_tbxSearchTerm, "m_tbxSearchTerm")
        Me.m_tbxSearchTerm.Name = "m_tbxSearchTerm"
        '
        'm_cmbEngine
        '
        resources.ApplyResources(Me.m_cmbEngine, "m_cmbEngine")
        Me.m_cmbEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbEngine.FormattingEnabled = True
        Me.m_cmbEngine.Items.AddRange(New Object() {resources.GetString("m_cmbEngine.Items"), resources.GetString("m_cmbEngine.Items1"), resources.GetString("m_cmbEngine.Items2")})
        Me.m_cmbEngine.Name = "m_cmbEngine"
        '
        'm_btnConfigure
        '
        resources.ApplyResources(Me.m_btnConfigure, "m_btnConfigure")
        Me.m_btnConfigure.Name = "m_btnConfigure"
        Me.m_btnConfigure.UseVisualStyleBackColor = True
        '
        'm_cbIncludeExtent
        '
        resources.ApplyResources(Me.m_cbIncludeExtent, "m_cbIncludeExtent")
        Me.m_cbIncludeExtent.Name = "m_cbIncludeExtent"
        Me.m_cbIncludeExtent.UseVisualStyleBackColor = True
        '
        'm_scMain
        '
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_gridGroups)
        Me.m_scMain.Panel1.Controls.Add(Me.m_hdrEdit)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnDefine)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnRemove)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnProps)
        Me.m_scMain.Panel1.Controls.Add(Me.m_hdrProps)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnKeep)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_hdrSearch)
        Me.m_scMain.Panel2.Controls.Add(Me.m_pbSearching)
        Me.m_scMain.Panel2.Controls.Add(Me.m_cmbFilter)
        Me.m_scMain.Panel2.Controls.Add(Me.m_btnAdd)
        Me.m_scMain.Panel2.Controls.Add(Me.m_lblIn)
        Me.m_scMain.Panel2.Controls.Add(Me.m_btnConfigure)
        Me.m_scMain.Panel2.Controls.Add(Me.m_cbIncludeExtent)
        Me.m_scMain.Panel2.Controls.Add(Me.m_gridResults)
        Me.m_scMain.Panel2.Controls.Add(Me.m_lblEngine)
        Me.m_scMain.Panel2.Controls.Add(Me.m_tbxSearchTerm)
        Me.m_scMain.Panel2.Controls.Add(Me.m_cmbEngine)
        Me.m_scMain.Panel2.Controls.Add(Me.m_lblSearchTerm)
        '
        'm_gridGroups
        '
        Me.m_gridGroups.AllowBlockSelect = False
        resources.ApplyResources(Me.m_gridGroups, "m_gridGroups")
        Me.m_gridGroups.AutoSizeMinHeight = 10
        Me.m_gridGroups.AutoSizeMinWidth = 10
        Me.m_gridGroups.AutoStretchColumnsToFitWidth = False
        Me.m_gridGroups.AutoStretchRowsToFitHeight = False
        Me.m_gridGroups.BackColor = System.Drawing.Color.White
        Me.m_gridGroups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_gridGroups.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_gridGroups.CustomSort = False
        Me.m_gridGroups.DataName = "grid content"
        Me.m_gridGroups.FixedColumnWidths = False
        Me.m_gridGroups.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_gridGroups.GridToolTipActive = True
        Me.m_gridGroups.IsLayoutSuspended = False
        Me.m_gridGroups.IsOutputGrid = True
        Me.m_gridGroups.Name = "m_gridGroups"
        Me.m_gridGroups.SelectedTaxon = Nothing
        Me.m_gridGroups.ShowCodes = False
        Me.m_gridGroups.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_gridGroups.UIContext = Nothing
        '
        'm_hdrEdit
        '
        resources.ApplyResources(Me.m_hdrEdit, "m_hdrEdit")
        Me.m_hdrEdit.CanCollapseParent = False
        Me.m_hdrEdit.CollapsedParentHeight = 0
        Me.m_hdrEdit.IsCollapsed = False
        Me.m_hdrEdit.Name = "m_hdrEdit"
        '
        'm_btnProps
        '
        resources.ApplyResources(Me.m_btnProps, "m_btnProps")
        Me.m_btnProps.Name = "m_btnProps"
        Me.m_btnProps.UseVisualStyleBackColor = True
        '
        'm_hdrProps
        '
        resources.ApplyResources(Me.m_hdrProps, "m_hdrProps")
        Me.m_hdrProps.CanCollapseParent = False
        Me.m_hdrProps.CollapsedParentHeight = 0
        Me.m_hdrProps.IsCollapsed = False
        Me.m_hdrProps.Name = "m_hdrProps"
        '
        'm_hdrSearch
        '
        resources.ApplyResources(Me.m_hdrSearch, "m_hdrSearch")
        Me.m_hdrSearch.CanCollapseParent = False
        Me.m_hdrSearch.CollapsedParentHeight = 0
        Me.m_hdrSearch.IsCollapsed = False
        Me.m_hdrSearch.Name = "m_hdrSearch"
        '
        'm_pbSearching
        '
        resources.ApplyResources(Me.m_pbSearching, "m_pbSearching")
        Me.m_pbSearching.Name = "m_pbSearching"
        Me.m_pbSearching.TabStop = False
        '
        'm_cmbFilter
        '
        resources.ApplyResources(Me.m_cmbFilter, "m_cmbFilter")
        Me.m_cmbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbFilter.FormattingEnabled = True
        Me.m_cmbFilter.Name = "m_cmbFilter"
        '
        'm_btnAdd
        '
        resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
        Me.m_btnAdd.Name = "m_btnAdd"
        Me.m_btnAdd.UseVisualStyleBackColor = True
        '
        'm_lblIn
        '
        resources.ApplyResources(Me.m_lblIn, "m_lblIn")
        Me.m_lblIn.Name = "m_lblIn"
        '
        'm_gridResults
        '
        Me.m_gridResults.AllowBlockSelect = False
        resources.ApplyResources(Me.m_gridResults, "m_gridResults")
        Me.m_gridResults.AutoSizeMinHeight = 10
        Me.m_gridResults.AutoSizeMinWidth = 10
        Me.m_gridResults.AutoStretchColumnsToFitWidth = False
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
        Me.m_gridResults.ShowCodes = False
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
        'm_lblEngine
        '
        resources.ApplyResources(Me.m_lblEngine, "m_lblEngine")
        Me.m_lblEngine.Name = "m_lblEngine"
        '
        'm_cbShowCodes
        '
        resources.ApplyResources(Me.m_cbShowCodes, "m_cbShowCodes")
        Me.m_cbShowCodes.Name = "m_cbShowCodes"
        Me.m_cbShowCodes.UseVisualStyleBackColor = True
        '
        'dlgDefineTaxonomy
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.Cancel_Button
        Me.ControlBox = False
        Me.Controls.Add(Me.m_cbShowCodes)
        Me.Controls.Add(Me.m_scMain)
        Me.Controls.Add(Me.Cancel_Button)
        Me.Controls.Add(Me.OK_Button)
        Me.Name = "dlgDefineTaxonomy"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel2.ResumeLayout(False)
        Me.m_scMain.Panel2.PerformLayout()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        CType(Me.m_pbSearching, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_btnRemove As System.Windows.Forms.Button
    Private WithEvents m_gridGroups As ScientificInterface.gridDefineTaxonomy
    Private WithEvents m_btnKeep As System.Windows.Forms.Button
    Private WithEvents m_hdrEdit As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents Cancel_Button As System.Windows.Forms.Button
    Private WithEvents OK_Button As System.Windows.Forms.Button
    Private WithEvents m_lblSearchTerm As System.Windows.Forms.Label
    Private WithEvents m_tbxSearchTerm As System.Windows.Forms.TextBox
    Private WithEvents m_cmbEngine As System.Windows.Forms.ComboBox
    Private WithEvents m_btnConfigure As System.Windows.Forms.Button
    Private WithEvents m_btnDefine As System.Windows.Forms.Button
    Private WithEvents m_cbIncludeExtent As System.Windows.Forms.CheckBox
    Private WithEvents m_lblEngine As System.Windows.Forms.Label
    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents m_hdrProps As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_btnProps As System.Windows.Forms.Button
    Private WithEvents m_cmbFilter As System.Windows.Forms.ComboBox
    Private WithEvents m_lblIn As System.Windows.Forms.Label
    Private WithEvents m_gridResults As ScientificInterface.gridTaxonSearchResults
    Private WithEvents m_pbSearching As System.Windows.Forms.PictureBox
    Private WithEvents m_cbShowCodes As System.Windows.Forms.CheckBox
    Private WithEvents m_btnAdd As System.Windows.Forms.Button
    Private WithEvents m_hdrSearch As ScientificInterfaceShared.Controls.cEwEHeaderLabel

End Class
