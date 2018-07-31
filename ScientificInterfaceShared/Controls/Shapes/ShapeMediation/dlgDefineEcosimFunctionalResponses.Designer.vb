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

Partial Class dlgDefineEcosimFunctionalResponses
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgDefineEcosimFunctionalResponses))
        Me.m_btnOk = New System.Windows.Forms.Button()
        Me.m_tvDrivers = New System.Windows.Forms.TreeView()
        Me.m_btnRemove = New System.Windows.Forms.Button()
        Me.m_btnAdd = New System.Windows.Forms.Button()
        Me.m_hdrReponse = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lbxGroups = New ScientificInterfaceShared.Controls.cGroupListBox()
        Me.m_hdrConfig = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblDrivers = New System.Windows.Forms.Label()
        Me.m_lblGroups = New System.Windows.Forms.Label()
        Me.m_graph = New ScientificInterfaceShared.ucDriverResponseView()
        Me.SuspendLayout()
        '
        'm_btnOk
        '
        resources.ApplyResources(Me.m_btnOk, "m_btnOk")
        Me.m_btnOk.Name = "m_btnOk"
        '
        'm_tvDrivers
        '
        resources.ApplyResources(Me.m_tvDrivers, "m_tvDrivers")
        Me.m_tvDrivers.FullRowSelect = True
        Me.m_tvDrivers.HideSelection = False
        Me.m_tvDrivers.Name = "m_tvDrivers"
        Me.m_tvDrivers.ShowRootLines = False
        '
        'm_btnRemove
        '
        resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
        Me.m_btnRemove.Image = Global.ScientificInterfaceShared.My.Resources.Resources.DeleteHS
        Me.m_btnRemove.Name = "m_btnRemove"
        Me.m_btnRemove.UseVisualStyleBackColor = True
        '
        'm_btnAdd
        '
        resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
        Me.m_btnAdd.Image = Global.ScientificInterfaceShared.My.Resources.Resources.forward
        Me.m_btnAdd.Name = "m_btnAdd"
        Me.m_btnAdd.UseVisualStyleBackColor = True
        '
        'm_hdrReponse
        '
        resources.ApplyResources(Me.m_hdrReponse, "m_hdrReponse")
        Me.m_hdrReponse.CanCollapseParent = False
        Me.m_hdrReponse.CollapsedParentHeight = 0
        Me.m_hdrReponse.IsCollapsed = False
        Me.m_hdrReponse.Name = "m_hdrReponse"
        '
        'm_lbxGroups
        '
        Me.m_lbxGroups.AllGroupsItemColor = System.Drawing.Color.Transparent
        Me.m_lbxGroups.AllGroupsItemText = "(All)"
        resources.ApplyResources(Me.m_lbxGroups, "m_lbxGroups")
        Me.m_lbxGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.m_lbxGroups.FormattingEnabled = True
        Me.m_lbxGroups.GroupListTracking = ScientificInterfaceShared.Controls.cGroupListBox.eGroupTrackingType.Manual
        Me.m_lbxGroups.IsAllGroupsItemSelected = False
        Me.m_lbxGroups.Name = "m_lbxGroups"
        Me.m_lbxGroups.SelectedGroup = Nothing
        Me.m_lbxGroups.SelectedGroupIndex = -1
        Me.m_lbxGroups.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.m_lbxGroups.ShowAllGroupsItem = False
        Me.m_lbxGroups.SortThreshold = -9999.0!
        '
        'm_hdrConfig
        '
        Me.m_hdrConfig.CanCollapseParent = False
        Me.m_hdrConfig.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrConfig, "m_hdrConfig")
        Me.m_hdrConfig.IsCollapsed = False
        Me.m_hdrConfig.Name = "m_hdrConfig"
        '
        'm_lblDrivers
        '
        resources.ApplyResources(Me.m_lblDrivers, "m_lblDrivers")
        Me.m_lblDrivers.Name = "m_lblDrivers"
        '
        'm_lblGroups
        '
        resources.ApplyResources(Me.m_lblGroups, "m_lblGroups")
        Me.m_lblGroups.Name = "m_lblGroups"
        '
        'm_graph
        '
        resources.ApplyResources(Me.m_graph, "m_graph")
        Me.m_graph.Driver = Nothing
        Me.m_graph.Name = "m_graph"
        Me.m_graph.Shape = Nothing
        Me.m_graph.UIContext = Nothing
        '
        'dlgDefineEcosimFunctionalResponses
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_graph)
        Me.Controls.Add(Me.m_lblDrivers)
        Me.Controls.Add(Me.m_lblGroups)
        Me.Controls.Add(Me.m_hdrReponse)
        Me.Controls.Add(Me.m_lbxGroups)
        Me.Controls.Add(Me.m_hdrConfig)
        Me.Controls.Add(Me.m_btnRemove)
        Me.Controls.Add(Me.m_btnAdd)
        Me.Controls.Add(Me.m_btnOk)
        Me.Controls.Add(Me.m_tvDrivers)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgDefineEcosimFunctionalResponses"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_btnOk As System.Windows.Forms.Button
    Private WithEvents m_btnRemove As System.Windows.Forms.Button
    Private WithEvents m_btnAdd As System.Windows.Forms.Button
    Private WithEvents m_tvDrivers As System.Windows.Forms.TreeView
    Private WithEvents m_lbxGroups As cGroupListBox
    Private m_hdrReponse As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private m_hdrConfig As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_lblDrivers As Label
    Private WithEvents m_lblGroups As Label
    Private WithEvents m_graph As ucDriverResponseView

End Class
