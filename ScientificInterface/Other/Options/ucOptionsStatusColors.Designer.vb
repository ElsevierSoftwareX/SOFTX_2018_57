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

Namespace Other

    Partial Class ucOptionsStatusColors
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptionsStatusColors))
            Me.m_lblItemForeColor = New System.Windows.Forms.Label()
            Me.m_cmbItemForeground = New System.Windows.Forms.ComboBox()
            Me.m_lblItemBackColor = New System.Windows.Forms.Label()
            Me.m_btnCustomForeColor = New System.Windows.Forms.Button()
            Me.m_cmbItemBackground = New System.Windows.Forms.ComboBox()
            Me.m_btnCustomBackColor = New System.Windows.Forms.Button()
            Me.m_hdrCaption = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_plPreview = New System.Windows.Forms.Panel()
            Me.m_lvItems = New System.Windows.Forms.ListView()
            Me.m_colItem = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_colDesc = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.SuspendLayout()
            '
            'm_lblItemForeColor
            '
            resources.ApplyResources(Me.m_lblItemForeColor, "m_lblItemForeColor")
            Me.m_lblItemForeColor.Name = "m_lblItemForeColor"
            '
            'm_cmbItemForeground
            '
            resources.ApplyResources(Me.m_cmbItemForeground, "m_cmbItemForeground")
            Me.m_cmbItemForeground.BackColor = System.Drawing.Color.White
            Me.m_cmbItemForeground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_cmbItemForeground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbItemForeground.FormattingEnabled = True
            Me.m_cmbItemForeground.Name = "m_cmbItemForeground"
            '
            'm_lblItemBackColor
            '
            resources.ApplyResources(Me.m_lblItemBackColor, "m_lblItemBackColor")
            Me.m_lblItemBackColor.Name = "m_lblItemBackColor"
            '
            'm_btnCustomForeColor
            '
            resources.ApplyResources(Me.m_btnCustomForeColor, "m_btnCustomForeColor")
            Me.m_btnCustomForeColor.Name = "m_btnCustomForeColor"
            Me.m_btnCustomForeColor.UseVisualStyleBackColor = True
            '
            'm_cmbItemBackground
            '
            resources.ApplyResources(Me.m_cmbItemBackground, "m_cmbItemBackground")
            Me.m_cmbItemBackground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_cmbItemBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbItemBackground.FormattingEnabled = True
            Me.m_cmbItemBackground.Name = "m_cmbItemBackground"
            '
            'm_btnCustomBackColor
            '
            resources.ApplyResources(Me.m_btnCustomBackColor, "m_btnCustomBackColor")
            Me.m_btnCustomBackColor.Name = "m_btnCustomBackColor"
            Me.m_btnCustomBackColor.UseVisualStyleBackColor = True
            '
            'm_hdrCaption
            '
            Me.m_hdrCaption.CanCollapseParent = False
            Me.m_hdrCaption.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrCaption, "m_hdrCaption")
            Me.m_hdrCaption.IsCollapsed = False
            Me.m_hdrCaption.Name = "m_hdrCaption"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_plPreview
            '
            resources.ApplyResources(Me.m_plPreview, "m_plPreview")
            Me.m_plPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_plPreview.Name = "m_plPreview"
            '
            'm_lvItems
            '
            resources.ApplyResources(Me.m_lvItems, "m_lvItems")
            Me.m_lvItems.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.m_colItem, Me.m_colDesc})
            Me.m_lvItems.FullRowSelect = True
            Me.m_lvItems.HideSelection = False
            Me.m_lvItems.MultiSelect = False
            Me.m_lvItems.Name = "m_lvItems"
            Me.m_lvItems.ShowGroups = False
            Me.m_lvItems.ShowItemToolTips = True
            Me.m_lvItems.Sorting = System.Windows.Forms.SortOrder.Ascending
            Me.m_lvItems.UseCompatibleStateImageBehavior = False
            Me.m_lvItems.View = System.Windows.Forms.View.Details
            '
            'm_colItem
            '
            resources.ApplyResources(Me.m_colItem, "m_colItem")
            '
            'm_colDesc
            '
            resources.ApplyResources(Me.m_colDesc, "m_colDesc")
            '
            'ucOptionsStatusColors
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_lvItems)
            Me.Controls.Add(Me.m_plPreview)
            Me.Controls.Add(Me.m_hdrCaption)
            Me.Controls.Add(Me.m_btnCustomBackColor)
            Me.Controls.Add(Me.m_cmbItemBackground)
            Me.Controls.Add(Me.m_btnCustomForeColor)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.m_lblItemBackColor)
            Me.Controls.Add(Me.m_cmbItemForeground)
            Me.Controls.Add(Me.m_lblItemForeColor)
            Me.Name = "ucOptionsStatusColors"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lblItemForeColor As System.Windows.Forms.Label
        Private WithEvents m_cmbItemForeground As System.Windows.Forms.ComboBox
        Private WithEvents m_lblItemBackColor As System.Windows.Forms.Label
        Private WithEvents m_btnCustomForeColor As System.Windows.Forms.Button
        Private WithEvents m_cmbItemBackground As System.Windows.Forms.ComboBox
        Private WithEvents m_btnCustomBackColor As System.Windows.Forms.Button
        Private m_hdrCaption As cEwEHeaderLabel
        Private WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_plPreview As System.Windows.Forms.Panel
        Private WithEvents m_lvItems As System.Windows.Forms.ListView
        Private WithEvents m_colItem As System.Windows.Forms.ColumnHeader
        Private WithEvents m_colDesc As System.Windows.Forms.ColumnHeader

    End Class
End Namespace

