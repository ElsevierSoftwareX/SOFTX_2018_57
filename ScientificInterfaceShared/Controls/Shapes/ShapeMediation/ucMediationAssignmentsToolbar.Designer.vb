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


    Partial Class ucMediationAssignmentsToolbar
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucMediationAssignmentsToolbar))
            Me.m_tsMenus = New ScientificInterfaceShared.Controls.cEwEToolstrip
            Me.m_tsbnDefineMediatingItems = New System.Windows.Forms.ToolStripButton
            Me.m_sep1 = New System.Windows.Forms.ToolStripSeparator
            Me.m_tsbnViewAsBar = New System.Windows.Forms.ToolStripButton
            Me.m_tsbnViewAsPie = New System.Windows.Forms.ToolStripButton
            Me.m_tsMenus.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tsMenus
            '
            Me.m_tsMenus.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMenus.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnDefineMediatingItems, Me.m_sep1, Me.m_tsbnViewAsBar, Me.m_tsbnViewAsPie})
            resources.ApplyResources(Me.m_tsMenus, "m_tsMenus")
            Me.m_tsMenus.Name = "m_tsMenus"
            Me.m_tsMenus.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnDefineMediatingItems
            '
            Me.m_tsbnDefineMediatingItems.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnDefineMediatingItems, "m_tsbnDefineMediatingItems")
            Me.m_tsbnDefineMediatingItems.Name = "m_tsbnDefineMediatingItems"
            '
            'm_sep1
            '
            Me.m_sep1.Name = "m_sep1"
            resources.ApplyResources(Me.m_sep1, "m_sep1")
            '
            'm_tsbnViewAsBar
            '
            Me.m_tsbnViewAsBar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            Me.m_tsbnViewAsBar.Image = Global.ScientificInterfaceShared.My.Resources.Resources.graphhs
            resources.ApplyResources(Me.m_tsbnViewAsBar, "m_tsbnViewAsBar")
            Me.m_tsbnViewAsBar.Name = "m_tsbnViewAsBar"
            '
            'm_tsbnViewAsPie
            '
            Me.m_tsbnViewAsPie.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            Me.m_tsbnViewAsPie.Image = Global.ScientificInterfaceShared.My.Resources.Resources.PieChart
            resources.ApplyResources(Me.m_tsbnViewAsPie, "m_tsbnViewAsPie")
            Me.m_tsbnViewAsPie.Name = "m_tsbnViewAsPie"
            '
            'ucMediationAssignmentsToolbar
            '
            resources.ApplyResources(Me, "$this")
            Me.Controls.Add(Me.m_tsMenus)
            Me.Name = "ucMediationAssignmentsToolbar"
            Me.m_tsMenus.ResumeLayout(False)
            Me.m_tsMenus.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tsbnDefineMediatingItems As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsMenus As cEwEToolstrip
        Private WithEvents m_tsbnViewAsBar As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnViewAsPie As System.Windows.Forms.ToolStripButton
        Private WithEvents m_sep1 As System.Windows.Forms.ToolStripSeparator

    End Class

End Namespace