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


    Partial Class dlgDefineMediationAssignments
        Inherits System.Windows.Forms.Form

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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgDefineMediationAssignments))
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_tvAvailable = New System.Windows.Forms.TreeView()
            Me.m_btnAdd = New System.Windows.Forms.Button()
            Me.m_btnRemove = New System.Windows.Forms.Button()
            Me.m_lblAvailable = New System.Windows.Forms.Label()
            Me.m_lblAssigned = New System.Windows.Forms.Label()
            Me.m_splitter = New System.Windows.Forms.SplitContainer()
            Me.m_grid = New ScientificInterfaceShared.Controls.ucMediationAssignmentsGrid()
            Me.m_graph = New ScientificInterfaceShared.Controls.ucMediationAssignments()
            CType(Me.m_splitter, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_splitter.Panel1.SuspendLayout()
            Me.m_splitter.Panel2.SuspendLayout()
            Me.m_splitter.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.Name = "m_btnOK"
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            '
            'm_tvAvailable
            '
            resources.ApplyResources(Me.m_tvAvailable, "m_tvAvailable")
            Me.m_tvAvailable.FullRowSelect = True
            Me.m_tvAvailable.HideSelection = False
            Me.m_tvAvailable.Name = "m_tvAvailable"
            Me.m_tvAvailable.Nodes.AddRange(New System.Windows.Forms.TreeNode() {CType(resources.GetObject("m_tvAvailable.Nodes"), System.Windows.Forms.TreeNode)})
            '
            'm_btnAdd
            '
            resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
            Me.m_btnAdd.Image = Global.ScientificInterfaceShared.My.Resources.Resources.forward
            Me.m_btnAdd.Name = "m_btnAdd"
            Me.m_btnAdd.UseVisualStyleBackColor = True
            '
            'm_btnRemove
            '
            resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
            Me.m_btnRemove.Image = Global.ScientificInterfaceShared.My.Resources.Resources.Back
            Me.m_btnRemove.Name = "m_btnRemove"
            Me.m_btnRemove.UseVisualStyleBackColor = True
            '
            'm_lblAvailable
            '
            resources.ApplyResources(Me.m_lblAvailable, "m_lblAvailable")
            Me.m_lblAvailable.Name = "m_lblAvailable"
            '
            'm_lblAssigned
            '
            resources.ApplyResources(Me.m_lblAssigned, "m_lblAssigned")
            Me.m_lblAssigned.Name = "m_lblAssigned"
            '
            'm_splitter
            '
            resources.ApplyResources(Me.m_splitter, "m_splitter")
            Me.m_splitter.Name = "m_splitter"
            '
            'm_splitter.Panel1
            '
            Me.m_splitter.Panel1.Controls.Add(Me.m_grid)
            '
            'm_splitter.Panel2
            '
            Me.m_splitter.Panel2.Controls.Add(Me.m_graph)
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = False
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = True
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLandings = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Name = "m_grid"
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.TrackPropertySelection = False
            Me.m_grid.UIContext = Nothing
            '
            'm_graph
            '
            Me.m_graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_graph.Data = Nothing
            resources.ApplyResources(Me.m_graph, "m_graph")
            Me.m_graph.Name = "m_graph"
            Me.m_graph.Shape = Nothing
            Me.m_graph.Title = Global.ScientificInterfaceShared.My.Resources.Resources.STYLEFLAGS_OK
            Me.m_graph.UIContext = Nothing
            Me.m_graph.ViewMode = ScientificInterfaceShared.Controls.ucMediationAssignments.eViewModeTypes.Pie
            Me.m_graph.XAxisLabel = Global.ScientificInterfaceShared.My.Resources.Resources.STYLEFLAGS_OK
            Me.m_graph.YAxisLabel = Global.ScientificInterfaceShared.My.Resources.Resources.STYLEFLAGS_OK
            '
            'dlgDefineMediationAssignments
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_splitter)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_lblAssigned)
            Me.Controls.Add(Me.m_lblAvailable)
            Me.Controls.Add(Me.m_tvAvailable)
            Me.Controls.Add(Me.m_btnRemove)
            Me.Controls.Add(Me.m_btnAdd)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgDefineMediationAssignments"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.m_splitter.Panel1.ResumeLayout(False)
            Me.m_splitter.Panel2.ResumeLayout(False)
            CType(Me.m_splitter, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_splitter.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_lblAvailable As System.Windows.Forms.Label
        Private WithEvents m_lblAssigned As System.Windows.Forms.Label
        Private WithEvents m_graph As ucMediationAssignments
        Private WithEvents m_tvAvailable As TreeView
        Private WithEvents m_btnAdd As System.Windows.Forms.Button
        Private WithEvents m_btnRemove As System.Windows.Forms.Button
        Private WithEvents m_splitter As System.Windows.Forms.SplitContainer
        Private WithEvents m_grid As ucMediationAssignmentsGrid

    End Class

End Namespace

