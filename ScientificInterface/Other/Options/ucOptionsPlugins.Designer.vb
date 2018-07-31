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

Imports ScientificInterfaceShared

Namespace Other

    Partial Class ucOptionsPlugins
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.m_hdrCaption = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tvPlugins = New ScientificInterfaceShared.Controls.cThemedTreeView()
            Me.m_ilPlugins = New System.Windows.Forms.ImageList(Me.components)
            Me.m_split = New System.Windows.Forms.SplitContainer()
            Me.m_cbEnablePlugin = New System.Windows.Forms.CheckBox()
            Me.m_cbEnableDisableAll = New System.Windows.Forms.CheckBox()
            CType(Me.m_split, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_split.Panel1.SuspendLayout()
            Me.m_split.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_hdrCaption
            '
            Me.m_hdrCaption.CanCollapseParent = False
            Me.m_hdrCaption.CollapsedParentHeight = 0
            Me.m_hdrCaption.Dock = System.Windows.Forms.DockStyle.Top
            Me.m_hdrCaption.IsCollapsed = False
            Me.m_hdrCaption.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrCaption.Name = "m_hdrCaption"
            Me.m_hdrCaption.Size = New System.Drawing.Size(414, 18)
            Me.m_hdrCaption.TabIndex = 0
            Me.m_hdrCaption.Text = "Plugins"
            Me.m_hdrCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_tvPlugins
            '
            Me.m_tvPlugins.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.m_tvPlugins.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tvPlugins.FullRowSelect = True
            Me.m_tvPlugins.HideSelection = False
            Me.m_tvPlugins.ImageIndex = 0
            Me.m_tvPlugins.ImageList = Me.m_ilPlugins
            Me.m_tvPlugins.Location = New System.Drawing.Point(0, 0)
            Me.m_tvPlugins.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tvPlugins.Name = "m_tvPlugins"
            Me.m_tvPlugins.SelectedImageIndex = 0
            Me.m_tvPlugins.ShowImages = True
            Me.m_tvPlugins.ShowLines = False
            Me.m_tvPlugins.Size = New System.Drawing.Size(136, 274)
            Me.m_tvPlugins.TabIndex = 0
            '
            'm_ilPlugins
            '
            Me.m_ilPlugins.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
            Me.m_ilPlugins.ImageSize = New System.Drawing.Size(16, 16)
            Me.m_ilPlugins.TransparentColor = System.Drawing.Color.Transparent
            '
            'm_split
            '
            Me.m_split.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_split.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_split.Location = New System.Drawing.Point(0, 25)
            Me.m_split.Margin = New System.Windows.Forms.Padding(0)
            Me.m_split.Name = "m_split"
            '
            'm_split.Panel1
            '
            Me.m_split.Panel1.Controls.Add(Me.m_tvPlugins)
            Me.m_split.Size = New System.Drawing.Size(414, 276)
            Me.m_split.SplitterDistance = 138
            Me.m_split.TabIndex = 1
            '
            'm_cbEnablePlugin
            '
            Me.m_cbEnablePlugin.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.m_cbEnablePlugin.AutoSize = True
            Me.m_cbEnablePlugin.Location = New System.Drawing.Point(0, 310)
            Me.m_cbEnablePlugin.Name = "m_cbEnablePlugin"
            Me.m_cbEnablePlugin.Size = New System.Drawing.Size(262, 17)
            Me.m_cbEnablePlugin.TabIndex = 2
            Me.m_cbEnablePlugin.Text = "&Load selected plug-in module next time EwE starts"
            Me.m_cbEnablePlugin.UseVisualStyleBackColor = True
            '
            'm_cbEnableDisableAll
            '
            Me.m_cbEnableDisableAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.m_cbEnableDisableAll.AutoSize = True
            Me.m_cbEnableDisableAll.Location = New System.Drawing.Point(0, 333)
            Me.m_cbEnableDisableAll.Name = "m_cbEnableDisableAll"
            Me.m_cbEnableDisableAll.Size = New System.Drawing.Size(155, 17)
            Me.m_cbEnableDisableAll.TabIndex = 3
            Me.m_cbEnableDisableAll.Text = "&Enable / disable all plug-ins"
            Me.m_cbEnableDisableAll.UseVisualStyleBackColor = True
            '
            'ucOptionsPlugins
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_cbEnableDisableAll)
            Me.Controls.Add(Me.m_cbEnablePlugin)
            Me.Controls.Add(Me.m_split)
            Me.Controls.Add(Me.m_hdrCaption)
            Me.Margin = New System.Windows.Forms.Padding(0)
            Me.Name = "ucOptionsPlugins"
            Me.Size = New System.Drawing.Size(414, 353)
            Me.m_split.Panel1.ResumeLayout(False)
            CType(Me.m_split, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_split.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tvPlugins As cThemedTreeView
        Private WithEvents m_split As System.Windows.Forms.SplitContainer
        Private WithEvents m_ilPlugins As System.Windows.Forms.ImageList
        Private WithEvents m_hdrCaption As cEwEHeaderLabel
        Private WithEvents m_cbEnablePlugin As System.Windows.Forms.CheckBox
        Private WithEvents m_cbEnableDisableAll As CheckBox
    End Class

End Namespace
