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

Namespace Ecosim

    Partial Class frmEcosimOutputFleetPlots
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcosimOutputFleetPlots))
            Me.m_graph = New ZedGraph.ZedGraphControl()
            Me.m_lbGroups = New ScientificInterfaceShared.Controls.cGroupListBox()
            Me.m_btnSaveData = New System.Windows.Forms.Button()
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
            Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_btnChoosePlots = New System.Windows.Forms.ToolStripButton()
            Me.m_plGroups = New System.Windows.Forms.Panel()
            Me.m_lbFleets = New ScientificInterfaceShared.Controls.cFleetListBox()
            Me.m_hdrFleet = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plPredators = New System.Windows.Forms.Panel()
            Me.m_hdrGroups = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_cbSaveVisibleOnly = New System.Windows.Forms.CheckBox()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.m_tlpMain.SuspendLayout()
            Me.m_tsMain.SuspendLayout()
            Me.m_plGroups.SuspendLayout()
            Me.m_plPredators.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_graph
            '
            Me.m_graph.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_graph, "m_graph")
            Me.m_graph.Name = "m_graph"
            Me.m_graph.ScrollGrace = 0R
            Me.m_graph.ScrollMaxX = 0R
            Me.m_graph.ScrollMaxY = 0R
            Me.m_graph.ScrollMaxY2 = 0R
            Me.m_graph.ScrollMinX = 0R
            Me.m_graph.ScrollMinY = 0R
            Me.m_graph.ScrollMinY2 = 0R
            '
            'm_lbGroups
            '
            Me.m_lbGroups.AllGroupsItemColor = System.Drawing.Color.Transparent
            Me.m_lbGroups.AllGroupsItemText = "(All)"
            resources.ApplyResources(Me.m_lbGroups, "m_lbGroups")
            Me.m_lbGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_lbGroups.FormattingEnabled = True
            Me.m_lbGroups.GroupListTracking = ScientificInterfaceShared.Controls.cGroupListBox.eGroupTrackingType.Manual
            Me.m_lbGroups.IsAllGroupsItemSelected = False
            Me.m_lbGroups.Name = "m_lbGroups"
            Me.m_lbGroups.SelectedGroup = Nothing
            Me.m_lbGroups.SelectedGroupIndex = -1
            Me.m_lbGroups.ShowAllGroupsItem = False
            Me.m_lbGroups.SortThreshold = -9999.0!
            Me.m_lbGroups.SortType = ScientificInterfaceShared.Controls.cGroupListBox.eSortType.ValueDesc
            '
            'm_btnSaveData
            '
            resources.ApplyResources(Me.m_btnSaveData, "m_btnSaveData")
            Me.m_btnSaveData.Name = "m_btnSaveData"
            Me.m_btnSaveData.UseVisualStyleBackColor = True
            '
            'm_scMain
            '
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_graph)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_tlpMain)
            '
            'm_tlpMain
            '
            resources.ApplyResources(Me.m_tlpMain, "m_tlpMain")
            Me.m_tlpMain.Controls.Add(Me.m_tsMain, 0, 0)
            Me.m_tlpMain.Controls.Add(Me.m_btnSaveData, 0, 3)
            Me.m_tlpMain.Controls.Add(Me.m_plGroups, 0, 1)
            Me.m_tlpMain.Controls.Add(Me.m_plPredators, 0, 2)
            Me.m_tlpMain.Controls.Add(Me.m_cbSaveVisibleOnly, 0, 4)
            Me.m_tlpMain.Name = "m_tlpMain"
            '
            'm_tsMain
            '
            resources.ApplyResources(Me.m_tsMain, "m_tsMain")
            Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_btnChoosePlots})
            Me.m_tsMain.Name = "m_tsMain"
            Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_btnChoosePlots
            '
            Me.m_btnChoosePlots.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_btnChoosePlots, "m_btnChoosePlots")
            Me.m_btnChoosePlots.Name = "m_btnChoosePlots"
            Me.m_btnChoosePlots.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never
            '
            'm_plGroups
            '
            Me.m_plGroups.Controls.Add(Me.m_lbFleets)
            Me.m_plGroups.Controls.Add(Me.m_hdrFleet)
            resources.ApplyResources(Me.m_plGroups, "m_plGroups")
            Me.m_plGroups.Name = "m_plGroups"
            '
            'm_lbFleets
            '
            resources.ApplyResources(Me.m_lbFleets, "m_lbFleets")
            Me.m_lbFleets.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_lbFleets.FleetListTracking = ScientificInterfaceShared.Controls.cFleetListBox.eFleetTrackingType.Fleets
            Me.m_lbFleets.FormattingEnabled = True
            Me.m_lbFleets.Name = "m_lbFleets"
            Me.m_lbFleets.SelectedFleet = Nothing
            Me.m_lbFleets.SelectedFleetIndex = -1
            Me.m_lbFleets.ShowAllFleetsItem = False
            Me.m_lbFleets.SortThreshold = -9999.0!
            '
            'm_hdrFleet
            '
            resources.ApplyResources(Me.m_hdrFleet, "m_hdrFleet")
            Me.m_hdrFleet.CanCollapseParent = False
            Me.m_hdrFleet.CollapsedParentHeight = 0
            Me.m_hdrFleet.IsCollapsed = False
            Me.m_hdrFleet.Name = "m_hdrFleet"
            '
            'm_plPredators
            '
            Me.m_plPredators.Controls.Add(Me.m_hdrGroups)
            Me.m_plPredators.Controls.Add(Me.m_lbGroups)
            resources.ApplyResources(Me.m_plPredators, "m_plPredators")
            Me.m_plPredators.Name = "m_plPredators"
            '
            'm_hdrGroups
            '
            resources.ApplyResources(Me.m_hdrGroups, "m_hdrGroups")
            Me.m_hdrGroups.CanCollapseParent = False
            Me.m_hdrGroups.CollapsedParentHeight = 0
            Me.m_hdrGroups.IsCollapsed = False
            Me.m_hdrGroups.Name = "m_hdrGroups"
            '
            'm_cbSaveVisibleOnly
            '
            resources.ApplyResources(Me.m_cbSaveVisibleOnly, "m_cbSaveVisibleOnly")
            Me.m_cbSaveVisibleOnly.Name = "m_cbSaveVisibleOnly"
            Me.m_cbSaveVisibleOnly.UseVisualStyleBackColor = True
            '
            'frmEcosimOutputFleetPlots
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_scMain)
            Me.Name = "frmEcosimOutputFleetPlots"
            Me.ShowIcon = False
            Me.TabText = ""
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.m_tlpMain.ResumeLayout(False)
            Me.m_tlpMain.PerformLayout()
            Me.m_tsMain.ResumeLayout(False)
            Me.m_tsMain.PerformLayout()
            Me.m_plGroups.ResumeLayout(False)
            Me.m_plPredators.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_graph As ZedGraph.ZedGraphControl
        Private WithEvents m_lbGroups As cGroupListBox
        Private WithEvents m_btnSaveData As System.Windows.Forms.Button
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_tlpMain As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_plGroups As System.Windows.Forms.Panel
        Private WithEvents m_plPredators As System.Windows.Forms.Panel
        Private WithEvents m_hdrFleet As cEwEHeaderLabel
        Private WithEvents m_hdrGroups As cEwEHeaderLabel
        Private WithEvents m_lbFleets As cFleetListBox
        Private WithEvents m_tsMain As cEwEToolstrip
        Private WithEvents m_cbSaveVisibleOnly As System.Windows.Forms.CheckBox
        Private WithEvents m_btnChoosePlots As System.Windows.Forms.ToolStripButton
    End Class

End Namespace

