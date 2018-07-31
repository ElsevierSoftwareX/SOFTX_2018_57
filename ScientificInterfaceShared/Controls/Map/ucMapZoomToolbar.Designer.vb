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

Namespace Controls.Map

    Partial Class ucMapZoomToolbar
        Inherits UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucMapZoomToolbar))
            Me.m_tsZoom = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbSaveImage = New System.Windows.Forms.ToolStripButton()
            Me.m_ts1 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_ts2 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsbZoomIn = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbZoomOut = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbZoomReset = New System.Windows.Forms.ToolStripButton()
            Me.m_cmsZoom = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.m_tsmiViewCenter2 = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiViewStretch2 = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_ts3 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiZoomIn = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiZoomOut = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiZoomReset = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsZoom.SuspendLayout()
            Me.m_cmsZoom.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tsZoom
            '
            Me.m_tsZoom.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsZoom.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbSaveImage, Me.m_ts1, Me.m_ts2, Me.m_tsbZoomIn, Me.m_tsbZoomOut, Me.m_tsbZoomReset})
            Me.m_tsZoom.Location = New System.Drawing.Point(0, 0)
            Me.m_tsZoom.Name = "m_tsZoom"
            Me.m_tsZoom.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            Me.m_tsZoom.Size = New System.Drawing.Size(100, 25)
            Me.m_tsZoom.TabIndex = 1
            Me.m_tsZoom.Text = "m_tzZoom"
            '
            'm_tsbSaveImage
            '
            Me.m_tsbSaveImage.Image = CType(resources.GetObject("m_tsbSaveImage.Image"), System.Drawing.Image)
            Me.m_tsbSaveImage.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbSaveImage.Name = "m_tsbSaveImage"
            Me.m_tsbSaveImage.Size = New System.Drawing.Size(96, 20)
            Me.m_tsbSaveImage.Text = "Save image..."
            '
            'm_ts1
            '
            Me.m_ts1.Name = "m_ts1"
            Me.m_ts1.Size = New System.Drawing.Size(6, 25)
            '
            'm_ts2
            '
            Me.m_ts2.Name = "m_ts2"
            Me.m_ts2.Size = New System.Drawing.Size(6, 25)
            '
            'm_tsbZoomIn
            '
            Me.m_tsbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            Me.m_tsbZoomIn.Image = CType(resources.GetObject("m_tsbZoomIn.Image"), System.Drawing.Image)
            Me.m_tsbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbZoomIn.Name = "m_tsbZoomIn"
            Me.m_tsbZoomIn.Size = New System.Drawing.Size(23, 20)
            Me.m_tsbZoomIn.ToolTipText = "Zoom in"
            '
            'm_tsbZoomOut
            '
            Me.m_tsbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            Me.m_tsbZoomOut.Image = CType(resources.GetObject("m_tsbZoomOut.Image"), System.Drawing.Image)
            Me.m_tsbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbZoomOut.Name = "m_tsbZoomOut"
            Me.m_tsbZoomOut.Size = New System.Drawing.Size(23, 20)
            Me.m_tsbZoomOut.ToolTipText = "Zoom out"
            '
            'm_tsbZoomReset
            '
            Me.m_tsbZoomReset.Image = CType(resources.GetObject("m_tsbZoomReset.Image"), System.Drawing.Image)
            Me.m_tsbZoomReset.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbZoomReset.Name = "m_tsbZoomReset"
            Me.m_tsbZoomReset.Size = New System.Drawing.Size(55, 20)
            Me.m_tsbZoomReset.Text = "Reset"
            '
            'm_cmsZoom
            '
            Me.m_cmsZoom.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiViewCenter2, Me.m_tsmiViewStretch2, Me.m_ts3, Me.m_tsmiZoomIn, Me.m_tsmiZoomOut, Me.m_tsmiZoomReset})
            Me.m_cmsZoom.Name = "m_cmsControl"
            Me.m_cmsZoom.Size = New System.Drawing.Size(177, 120)
            '
            'm_tsmiViewCenter2
            '
            Me.m_tsmiViewCenter2.Name = "m_tsmiViewCenter2"
            Me.m_tsmiViewCenter2.Size = New System.Drawing.Size(176, 22)
            Me.m_tsmiViewCenter2.Text = "Center"
            '
            'm_tsmiViewStretch2
            '
            Me.m_tsmiViewStretch2.Name = "m_tsmiViewStretch2"
            Me.m_tsmiViewStretch2.Size = New System.Drawing.Size(176, 22)
            Me.m_tsmiViewStretch2.Text = "Stretch"
            '
            'm_ts3
            '
            Me.m_ts3.Name = "m_ts3"
            Me.m_ts3.Size = New System.Drawing.Size(173, 6)
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
            'ucMapZoomToolbar
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSize = True
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_tsZoom)
            Me.MinimumSize = New System.Drawing.Size(100, 25)
            Me.Name = "ucMapZoomToolbar"
            Me.Size = New System.Drawing.Size(100, 25)
            Me.m_tsZoom.ResumeLayout(False)
            Me.m_tsZoom.PerformLayout()
            Me.m_cmsZoom.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_tsZoom As cEwEToolstrip
        Private WithEvents m_cmsZoom As System.Windows.Forms.ContextMenuStrip
        Private WithEvents m_tsbSaveImage As System.Windows.Forms.ToolStripButton
        Private WithEvents m_ts1 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_ts2 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_ts3 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tsbZoomIn As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbZoomOut As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbZoomReset As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsmiZoomIn As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiZoomOut As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiZoomReset As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiViewCenter2 As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiViewStretch2 As System.Windows.Forms.ToolStripMenuItem

    End Class

End Namespace
