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

Namespace Controls


    Partial Class ucShapeToolbox
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.m_lvContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.AddToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ApplyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.DuplicateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
            Me.ImportToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ExportToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
            Me.ChangeShapeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ValuesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.RenameToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
            Me.ResetToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.RemoveToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_lvShapes = New ScientificInterfaceShared.Controls.cSmoothListView()
            Me.m_lvContextMenuStrip.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_lvContextMenuStrip
            '
            Me.m_lvContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AddToolStripMenuItem, Me.ApplyToolStripMenuItem, Me.DuplicateToolStripMenuItem, Me.ToolStripSeparator1, Me.ImportToolStripMenuItem, Me.ExportToolStripMenuItem, Me.ToolStripSeparator2, Me.ChangeShapeToolStripMenuItem, Me.ValuesToolStripMenuItem, Me.RenameToolStripMenuItem, Me.ToolStripSeparator3, Me.ResetToolStripMenuItem, Me.RemoveToolStripMenuItem})
            Me.m_lvContextMenuStrip.Name = "lvContextMenuStrip"
            Me.m_lvContextMenuStrip.Size = New System.Drawing.Size(159, 242)
            '
            'AddToolStripMenuItem
            '
            Me.AddToolStripMenuItem.Name = "AddToolStripMenuItem"
            Me.AddToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.AddToolStripMenuItem.Text = "A&dd..."
            '
            'ApplyToolStripMenuItem
            '
            Me.ApplyToolStripMenuItem.Name = "ApplyToolStripMenuItem"
            Me.ApplyToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.ApplyToolStripMenuItem.Text = "A&pply..."
            '
            'DuplicateToolStripMenuItem
            '
            Me.DuplicateToolStripMenuItem.Name = "DuplicateToolStripMenuItem"
            Me.DuplicateToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.DuplicateToolStripMenuItem.Text = "&Duplicate"
            '
            'ToolStripSeparator1
            '
            Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
            Me.ToolStripSeparator1.Size = New System.Drawing.Size(155, 6)
            '
            'ImportToolStripMenuItem
            '
            Me.ImportToolStripMenuItem.Name = "ImportToolStripMenuItem"
            Me.ImportToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.ImportToolStripMenuItem.Text = "&Import..."
            '
            'ExportToolStripMenuItem
            '
            Me.ExportToolStripMenuItem.Name = "ExportToolStripMenuItem"
            Me.ExportToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.ExportToolStripMenuItem.Text = "E&xport..."
            '
            'ToolStripSeparator2
            '
            Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
            Me.ToolStripSeparator2.Size = New System.Drawing.Size(155, 6)
            '
            'ChangeShapeToolStripMenuItem
            '
            Me.ChangeShapeToolStripMenuItem.Name = "ChangeShapeToolStripMenuItem"
            Me.ChangeShapeToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.ChangeShapeToolStripMenuItem.Text = "&Change shape..."
            '
            'ValuesToolStripMenuItem
            '
            Me.ValuesToolStripMenuItem.Name = "ValuesToolStripMenuItem"
            Me.ValuesToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.ValuesToolStripMenuItem.Text = "Values..."
            '
            'RenameToolStripMenuItem
            '
            Me.RenameToolStripMenuItem.Name = "RenameToolStripMenuItem"
            Me.RenameToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.RenameToolStripMenuItem.Text = "Re&name"
            '
            'ToolStripSeparator3
            '
            Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
            Me.ToolStripSeparator3.Size = New System.Drawing.Size(155, 6)
            '
            'ResetToolStripMenuItem
            '
            Me.ResetToolStripMenuItem.Name = "ResetToolStripMenuItem"
            Me.ResetToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.ResetToolStripMenuItem.Text = "Re&set"
            '
            'RemoveToolStripMenuItem
            '
            Me.RemoveToolStripMenuItem.Name = "RemoveToolStripMenuItem"
            Me.RemoveToolStripMenuItem.Size = New System.Drawing.Size(158, 22)
            Me.RemoveToolStripMenuItem.Text = "Re&move"
            '
            'm_lvShapes
            '
            Me.m_lvShapes.BackColor = System.Drawing.SystemColors.Window
            Me.m_lvShapes.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.m_lvShapes.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_lvShapes.HideSelection = False
            Me.m_lvShapes.Location = New System.Drawing.Point(0, 0)
            Me.m_lvShapes.Name = "m_lvShapes"
            Me.m_lvShapes.Size = New System.Drawing.Size(10, 24)
            Me.m_lvShapes.TabIndex = 0
            Me.m_lvShapes.UseCompatibleStateImageBehavior = False
            '
            'ucShapeToolbox
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSize = True
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.ContextMenuStrip = Me.m_lvContextMenuStrip
            Me.Controls.Add(Me.m_lvShapes)
            Me.Margin = New System.Windows.Forms.Padding(0)
            Me.MinimumSize = New System.Drawing.Size(10, 10)
            Me.Name = "ucShapeToolbox"
            Me.Size = New System.Drawing.Size(10, 24)
            Me.m_lvContextMenuStrip.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_lvShapes As cSmoothListView
        Private WithEvents m_lvContextMenuStrip As System.Windows.Forms.ContextMenuStrip
        Private WithEvents AddToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents DuplicateToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents RemoveToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents RenameToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents ApplyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents ImportToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
        Friend WithEvents ExportToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
        Friend WithEvents ResetToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents ChangeShapeToolStripMenuItem As ToolStripMenuItem
        Friend WithEvents ValuesToolStripMenuItem As ToolStripMenuItem
        Friend WithEvents ToolStripSeparator3 As ToolStripSeparator
    End Class

End Namespace

