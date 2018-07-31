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

Partial Class dlgSelectResponse
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgSelectResponse))
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.m_lvAllShapes = New System.Windows.Forms.ListView()
        Me.m_lvAppliedShapes = New System.Windows.Forms.ListView()
        Me.m_btnRemove = New System.Windows.Forms.Button()
        Me.m_btnAdd = New System.Windows.Forms.Button()
        Me.m_tlMain = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plButtons = New System.Windows.Forms.Panel()
        Me.m_hdrResp = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrApplied = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnCaseSensitive = New System.Windows.Forms.ToolStripButton()
        Me.m_tstbFilter = New System.Windows.Forms.ToolStripTextBox()
        Me.m_tslbFilter = New System.Windows.Forms.ToolStripLabel()
        Me.m_tlMain.SuspendLayout()
        Me.m_plButtons.SuspendLayout()
        Me.m_tsMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'OK_Button
        '
        resources.ApplyResources(Me.OK_Button, "OK_Button")
        Me.OK_Button.Name = "OK_Button"
        '
        'Cancel_Button
        '
        resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Name = "Cancel_Button"
        '
        'm_lvAllShapes
        '
        resources.ApplyResources(Me.m_lvAllShapes, "m_lvAllShapes")
        Me.m_lvAllShapes.FullRowSelect = True
        Me.m_lvAllShapes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.m_lvAllShapes.HideSelection = False
        Me.m_lvAllShapes.MultiSelect = False
        Me.m_lvAllShapes.Name = "m_lvAllShapes"
        Me.m_lvAllShapes.ShowItemToolTips = True
        Me.m_lvAllShapes.UseCompatibleStateImageBehavior = False
        Me.m_lvAllShapes.View = System.Windows.Forms.View.List
        '
        'm_lvAppliedShapes
        '
        resources.ApplyResources(Me.m_lvAppliedShapes, "m_lvAppliedShapes")
        Me.m_lvAppliedShapes.FullRowSelect = True
        Me.m_lvAppliedShapes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.m_lvAppliedShapes.HideSelection = False
        Me.m_lvAppliedShapes.Name = "m_lvAppliedShapes"
        Me.m_lvAppliedShapes.UseCompatibleStateImageBehavior = False
        '
        'm_btnRemove
        '
        resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
        Me.m_btnRemove.Name = "m_btnRemove"
        Me.m_btnRemove.UseVisualStyleBackColor = True
        '
        'm_btnAdd
        '
        resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
        Me.m_btnAdd.Name = "m_btnAdd"
        Me.m_btnAdd.UseVisualStyleBackColor = True
        '
        'm_tlMain
        '
        resources.ApplyResources(Me.m_tlMain, "m_tlMain")
        Me.m_tlMain.Controls.Add(Me.m_lvAllShapes, 0, 1)
        Me.m_tlMain.Controls.Add(Me.m_lvAppliedShapes, 2, 1)
        Me.m_tlMain.Controls.Add(Me.m_plButtons, 1, 1)
        Me.m_tlMain.Controls.Add(Me.m_hdrResp, 0, 0)
        Me.m_tlMain.Controls.Add(Me.m_hdrApplied, 2, 0)
        Me.m_tlMain.Name = "m_tlMain"
        '
        'm_plButtons
        '
        Me.m_plButtons.Controls.Add(Me.m_btnAdd)
        Me.m_plButtons.Controls.Add(Me.m_btnRemove)
        resources.ApplyResources(Me.m_plButtons, "m_plButtons")
        Me.m_plButtons.Name = "m_plButtons"
        '
        'm_hdrResp
        '
        Me.m_hdrResp.CanCollapseParent = False
        Me.m_hdrResp.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrResp, "m_hdrResp")
        Me.m_hdrResp.IsCollapsed = False
        Me.m_hdrResp.Name = "m_hdrResp"
        '
        'm_hdrApplied
        '
        Me.m_hdrApplied.CanCollapseParent = False
        Me.m_hdrApplied.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrApplied, "m_hdrApplied")
        Me.m_hdrApplied.IsCollapsed = False
        Me.m_hdrApplied.Name = "m_hdrApplied"
        '
        'm_tsMain
        '
        Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnCaseSensitive, Me.m_tstbFilter, Me.m_tslbFilter})
        resources.ApplyResources(Me.m_tsMain, "m_tsMain")
        Me.m_tsMain.Name = "m_tsMain"
        Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnCaseSensitive
        '
        Me.m_tsbnCaseSensitive.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tsbnCaseSensitive.AutoToolTip = False
        Me.m_tsbnCaseSensitive.CheckOnClick = True
        Me.m_tsbnCaseSensitive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnCaseSensitive, "m_tsbnCaseSensitive")
        Me.m_tsbnCaseSensitive.Name = "m_tsbnCaseSensitive"
        '
        'm_tstbFilter
        '
        Me.m_tstbFilter.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tstbFilter.Name = "m_tstbFilter"
        resources.ApplyResources(Me.m_tstbFilter, "m_tstbFilter")
        '
        'm_tslbFilter
        '
        Me.m_tslbFilter.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tslbFilter.Name = "m_tslbFilter"
        resources.ApplyResources(Me.m_tslbFilter, "m_tslbFilter")
        '
        'dlgSelectResponse
        '
        Me.AcceptButton = Me.OK_Button
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.Cancel_Button
        Me.Controls.Add(Me.m_tsMain)
        Me.Controls.Add(Me.m_tlMain)
        Me.Controls.Add(Me.OK_Button)
        Me.Controls.Add(Me.Cancel_Button)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgSelectResponse"
        Me.ShowInTaskbar = False
        Me.TabText = Global.ScientificInterfaceShared.My.Resources.Resources.STYLEFLAGS_OK
        Me.m_tlMain.ResumeLayout(False)
        Me.m_plButtons.ResumeLayout(False)
        Me.m_tsMain.ResumeLayout(False)
        Me.m_tsMain.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_lvAllShapes As System.Windows.Forms.ListView
    Private WithEvents m_lvAppliedShapes As System.Windows.Forms.ListView
    Private WithEvents m_btnRemove As System.Windows.Forms.Button
    Private WithEvents m_btnAdd As System.Windows.Forms.Button
    Private WithEvents OK_Button As System.Windows.Forms.Button
    Private WithEvents Cancel_Button As System.Windows.Forms.Button
    Private WithEvents m_tlMain As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_plButtons As System.Windows.Forms.Panel
    Private WithEvents m_hdrResp As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrApplied As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tslbFilter As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tstbFilter As System.Windows.Forms.ToolStripTextBox
    Private WithEvents m_tsbnCaseSensitive As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsMain As ScientificInterfaceShared.Controls.cEwEToolstrip

End Class