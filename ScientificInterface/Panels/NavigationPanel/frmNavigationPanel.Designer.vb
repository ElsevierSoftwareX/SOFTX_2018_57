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

Partial Class frmNavigationPanel
    Inherits frmEwEDockContent

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmNavigationPanel))
        Me.m_ilTreeIcons = New System.Windows.Forms.ImageList(Me.components)
        Me.m_tvNavigation = New ScientificInterfaceShared.Controls.cThemedTreeView()
        Me.SuspendLayout()
        '
        'm_ilTreeIcons
        '
        Me.m_ilTreeIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.m_ilTreeIcons, "m_ilTreeIcons")
        Me.m_ilTreeIcons.TransparentColor = System.Drawing.Color.Transparent
        '
        'm_tvNavigation
        '
        resources.ApplyResources(Me.m_tvNavigation, "m_tvNavigation")
        Me.m_tvNavigation.FullRowSelect = True
        Me.m_tvNavigation.HideSelection = False
        Me.m_tvNavigation.HotTracking = True
        Me.m_tvNavigation.ImageList = Me.m_ilTreeIcons
        Me.m_tvNavigation.Name = "m_tvNavigation"
        Me.m_tvNavigation.Nodes.AddRange(New System.Windows.Forms.TreeNode() {CType(resources.GetObject("m_tvNavigation.Nodes"), System.Windows.Forms.TreeNode)})
        Me.m_tvNavigation.ShowImages = True
        Me.m_tvNavigation.ShowLines = False
        '
        'frmNavigationPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.CloseButtonVisible = False
        Me.ControlBox = False
        Me.Controls.Add(Me.m_tvNavigation)
        Me.HideOnClose = True
        Me.Name = "frmNavigationPanel"
        Me.ShowIcon = False
        Me.TabText = ""
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_ilTreeIcons As System.Windows.Forms.ImageList
    Private WithEvents m_tvNavigation As cThemedTreeView
End Class
