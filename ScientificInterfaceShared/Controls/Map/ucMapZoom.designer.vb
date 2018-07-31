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
' Copyright 1991- UBC Fisheries Centre, Vancouver BC, Canada.
' ===============================================================================
'

Namespace Controls.Map

    Partial Class ucMapZoom
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
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.m_map = New ScientificInterfaceShared.Controls.Map.ucMap()
            Me.m_cmsZoom = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.PositionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiViewCenter2 = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiViewStretch2 = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiZoomIn = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiZoomOut = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiZoomReset = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_cmsZoom.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_map
            '
            Me.m_map.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_map.BackColor = System.Drawing.SystemColors.Window
            Me.m_map.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_map.Editable = False
            Me.m_map.Location = New System.Drawing.Point(123, 79)
            Me.m_map.Margin = New System.Windows.Forms.Padding(0)
            Me.m_map.Name = "m_map"
            Me.m_map.Size = New System.Drawing.Size(200, 200)
            Me.m_map.TabIndex = 0
            Me.m_map.Title = ""
            Me.m_map.UIContext = Nothing
            '
            'm_cmsZoom
            '
            Me.m_cmsZoom.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.PositionToolStripMenuItem, Me.ToolStripSeparator2, Me.m_tsmiZoomIn, Me.m_tsmiZoomOut, Me.m_tsmiZoomReset})
            Me.m_cmsZoom.Name = "m_cmsControl"
            Me.m_cmsZoom.Size = New System.Drawing.Size(177, 98)
            '
            'PositionToolStripMenuItem
            '
            Me.PositionToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiViewCenter2, Me.m_tsmiViewStretch2})
            Me.PositionToolStripMenuItem.Enabled = False
            Me.PositionToolStripMenuItem.Name = "PositionToolStripMenuItem"
            Me.PositionToolStripMenuItem.Size = New System.Drawing.Size(176, 22)
            Me.PositionToolStripMenuItem.Text = "Position"
            '
            'm_tsmiViewCenter2
            '
            Me.m_tsmiViewCenter2.Name = "m_tsmiViewCenter2"
            Me.m_tsmiViewCenter2.Size = New System.Drawing.Size(111, 22)
            Me.m_tsmiViewCenter2.Text = "Center"
            '
            'm_tsmiViewStretch2
            '
            Me.m_tsmiViewStretch2.Name = "m_tsmiViewStretch2"
            Me.m_tsmiViewStretch2.Size = New System.Drawing.Size(111, 22)
            Me.m_tsmiViewStretch2.Text = "Stretch"
            '
            'ToolStripSeparator2
            '
            Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
            Me.ToolStripSeparator2.Size = New System.Drawing.Size(173, 6)
            '
            'm_tsmiZoomIn
            '
            Me.m_tsmiZoomIn.Enabled = False
            Me.m_tsmiZoomIn.Name = "m_tsmiZoomIn"
            Me.m_tsmiZoomIn.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.I), System.Windows.Forms.Keys)
            Me.m_tsmiZoomIn.Size = New System.Drawing.Size(176, 22)
            Me.m_tsmiZoomIn.Text = "Zoom in"
            '
            'm_tsmiZoomOut
            '
            Me.m_tsmiZoomOut.Enabled = False
            Me.m_tsmiZoomOut.Name = "m_tsmiZoomOut"
            Me.m_tsmiZoomOut.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.O), System.Windows.Forms.Keys)
            Me.m_tsmiZoomOut.Size = New System.Drawing.Size(176, 22)
            Me.m_tsmiZoomOut.Text = "Zoom out"
            '
            'm_tsmiZoomReset
            '
            Me.m_tsmiZoomReset.Enabled = False
            Me.m_tsmiZoomReset.Name = "m_tsmiZoomReset"
            Me.m_tsmiZoomReset.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.R), System.Windows.Forms.Keys)
            Me.m_tsmiZoomReset.Size = New System.Drawing.Size(176, 22)
            Me.m_tsmiZoomReset.Text = "Reset zoom"
            '
            'ucMapZoom
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.AutoScroll = True
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_map)
            Me.Name = "ucMapZoom"
            Me.Size = New System.Drawing.Size(443, 364)
            Me.m_cmsZoom.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents PositionToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiViewCenter2 As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiViewStretch2 As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tsmiZoomIn As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiZoomOut As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiZoomReset As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_cmsZoom As System.Windows.Forms.ContextMenuStrip
        Private WithEvents m_map As ucMap
    End Class

End Namespace