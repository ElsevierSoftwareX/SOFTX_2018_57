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

Imports ZedGraph

Namespace Ecosim

    Partial Class dlgEstimateVs
        Inherits Form

        'Form overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEstimateVs))
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_rbPredMort = New System.Windows.Forms.RadioButton()
            Me.m_rbFMaxM = New System.Windows.Forms.RadioButton()
            Me.m_rbBuBo = New System.Windows.Forms.RadioButton()
            Me.m_rbBoBu = New System.Windows.Forms.RadioButton()
            Me.m_hdrMethodology = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_graph = New ZedGraph.ZedGraphControl()
            Me.m_lblPrompt = New System.Windows.Forms.Label()
            Me.m_grid = New ScientificInterface.Ecosim.gridEstimateVs()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_btnOK = New System.Windows.Forms.Button()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_scMain
            '
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbPredMort)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbFMaxM)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbBuBo)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbBoBu)
            Me.m_scMain.Panel1.Controls.Add(Me.m_hdrMethodology)
            Me.m_scMain.Panel1.Controls.Add(Me.m_graph)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_lblPrompt)
            Me.m_scMain.Panel2.Controls.Add(Me.m_grid)
            '
            'm_rbPredMort
            '
            resources.ApplyResources(Me.m_rbPredMort, "m_rbPredMort")
            Me.m_rbPredMort.Name = "m_rbPredMort"
            Me.m_rbPredMort.TabStop = True
            Me.m_rbPredMort.UseVisualStyleBackColor = True
            '
            'm_rbFMaxM
            '
            resources.ApplyResources(Me.m_rbFMaxM, "m_rbFMaxM")
            Me.m_rbFMaxM.Name = "m_rbFMaxM"
            Me.m_rbFMaxM.TabStop = True
            Me.m_rbFMaxM.UseVisualStyleBackColor = True
            '
            'm_rbBuBo
            '
            resources.ApplyResources(Me.m_rbBuBo, "m_rbBuBo")
            Me.m_rbBuBo.Name = "m_rbBuBo"
            Me.m_rbBuBo.TabStop = True
            Me.m_rbBuBo.UseVisualStyleBackColor = True
            '
            'm_rbBoBu
            '
            resources.ApplyResources(Me.m_rbBoBu, "m_rbBoBu")
            Me.m_rbBoBu.Name = "m_rbBoBu"
            Me.m_rbBoBu.TabStop = True
            Me.m_rbBoBu.UseVisualStyleBackColor = True
            '
            'm_hdrMethodology
            '
            Me.m_hdrMethodology.CanCollapseParent = False
            Me.m_hdrMethodology.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrMethodology, "m_hdrMethodology")
            Me.m_hdrMethodology.IsCollapsed = False
            Me.m_hdrMethodology.Name = "m_hdrMethodology"
            '
            'm_graph
            '
            resources.ApplyResources(Me.m_graph, "m_graph")
            Me.m_graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_graph.Name = "m_graph"
            Me.m_graph.ScrollGrace = 0.0R
            Me.m_graph.ScrollMaxX = 0.0R
            Me.m_graph.ScrollMaxY = 0.0R
            Me.m_graph.ScrollMaxY2 = 0.0R
            Me.m_graph.ScrollMinX = 0.0R
            Me.m_graph.ScrollMinY = 0.0R
            Me.m_graph.ScrollMinY2 = 0.0R
            '
            'm_lblPrompt
            '
            resources.ApplyResources(Me.m_lblPrompt, "m_lblPrompt")
            Me.m_lblPrompt.Name = "m_lblPrompt"
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = False
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            Me.m_grid.FixedColumnWidths = True
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.Name = "m_grid"
            Me.m_grid.SelectedGroupIndex = -1
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.UIContext = Nothing
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            Me.m_btnCancel.UseVisualStyleBackColor = True
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.m_btnOK.Name = "m_btnOK"
            Me.m_btnOK.UseVisualStyleBackColor = True
            '
            'dlgEstimateVs
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.ControlBox = False
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_scMain)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgEstimateVs"
            Me.ShowInTaskbar = False
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel1.PerformLayout()
            Me.m_scMain.Panel2.ResumeLayout(False)
            Me.m_scMain.Panel2.PerformLayout()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_grid As gridEstimateVs
        Private WithEvents m_graph As ZedGraphControl
        Private WithEvents m_hdrMethodology As cEwEHeaderLabel
        Private WithEvents m_rbBoBu As System.Windows.Forms.RadioButton
        Private WithEvents m_rbBuBo As System.Windows.Forms.RadioButton
        Private WithEvents m_rbFMaxM As System.Windows.Forms.RadioButton
        Private WithEvents m_rbPredMort As System.Windows.Forms.RadioButton
        Private WithEvents m_lblPrompt As System.Windows.Forms.Label

    End Class

End Namespace ' Ecosim