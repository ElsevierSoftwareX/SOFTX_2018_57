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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ucSelector2
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.m_tlpBits = New System.Windows.Forms.TableLayoutPanel()
        Me.m_tlpButtons = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnAdd = New System.Windows.Forms.Button()
        Me.m_btnRemove = New System.Windows.Forms.Button()
        Me.m_hdrSelection = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lbxBits = New System.Windows.Forms.CheckedListBox()
        Me.m_tlpBits.SuspendLayout()
        Me.m_tlpButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tlpBits
        '
        Me.m_tlpBits.ColumnCount = 1
        Me.m_tlpBits.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpBits.Controls.Add(Me.m_tlpButtons, 0, 2)
        Me.m_tlpBits.Controls.Add(Me.m_hdrSelection, 0, 0)
        Me.m_tlpBits.Controls.Add(Me.m_lbxBits, 0, 1)
        Me.m_tlpBits.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpBits.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpBits.Margin = New System.Windows.Forms.Padding(0)
        Me.m_tlpBits.Name = "m_tlpBits"
        Me.m_tlpBits.RowCount = 3
        Me.m_tlpBits.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.m_tlpBits.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpBits.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpBits.Size = New System.Drawing.Size(266, 258)
        Me.m_tlpBits.TabIndex = 1
        '
        'm_tlpButtons
        '
        Me.m_tlpButtons.ColumnCount = 2
        Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpButtons.Controls.Add(Me.m_btnAdd, 0, 0)
        Me.m_tlpButtons.Controls.Add(Me.m_btnRemove, 1, 0)
        Me.m_tlpButtons.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpButtons.Location = New System.Drawing.Point(0, 229)
        Me.m_tlpButtons.Margin = New System.Windows.Forms.Padding(0)
        Me.m_tlpButtons.Name = "m_tlpButtons"
        Me.m_tlpButtons.RowCount = 1
        Me.m_tlpButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpButtons.Size = New System.Drawing.Size(266, 29)
        Me.m_tlpButtons.TabIndex = 1
        '
        'm_btnAdd
        '
        Me.m_btnAdd.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnAdd.Location = New System.Drawing.Point(3, 3)
        Me.m_btnAdd.Name = "m_btnAdd"
        Me.m_btnAdd.Size = New System.Drawing.Size(127, 23)
        Me.m_btnAdd.TabIndex = 0
        Me.m_btnAdd.Text = "&Add"
        Me.m_btnAdd.UseVisualStyleBackColor = True
        '
        'm_btnRemove
        '
        Me.m_btnRemove.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnRemove.Location = New System.Drawing.Point(136, 3)
        Me.m_btnRemove.Name = "m_btnRemove"
        Me.m_btnRemove.Size = New System.Drawing.Size(127, 23)
        Me.m_btnRemove.TabIndex = 0
        Me.m_btnRemove.Text = "&Remove"
        Me.m_btnRemove.UseVisualStyleBackColor = True
        '
        'm_hdrSelection
        '
        Me.m_hdrSelection.CanCollapseParent = False
        Me.m_hdrSelection.CollapsedParentHeight = 0
        Me.m_hdrSelection.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_hdrSelection.IsCollapsed = False
        Me.m_hdrSelection.Location = New System.Drawing.Point(3, 0)
        Me.m_hdrSelection.Name = "m_hdrSelection"
        Me.m_hdrSelection.Size = New System.Drawing.Size(260, 20)
        Me.m_hdrSelection.TabIndex = 2
        Me.m_hdrSelection.Text = "Selection"
        Me.m_hdrSelection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_lbxBits
        '
        Me.m_lbxBits.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbxBits.FormattingEnabled = True
        Me.m_lbxBits.Location = New System.Drawing.Point(3, 23)
        Me.m_lbxBits.Name = "m_lbxBits"
        Me.m_lbxBits.Size = New System.Drawing.Size(260, 203)
        Me.m_lbxBits.TabIndex = 3
        '
        'ucSelector2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.m_tlpBits)
        Me.Name = "ucSelector2"
        Me.Size = New System.Drawing.Size(266, 258)
        Me.m_tlpBits.ResumeLayout(False)
        Me.m_tlpButtons.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_tlpBits As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_btnRemove As System.Windows.Forms.Button
    Private WithEvents m_btnAdd As System.Windows.Forms.Button
    Private WithEvents m_tlpButtons As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_hdrSelection As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_lbxBits As System.Windows.Forms.CheckedListBox

End Class
