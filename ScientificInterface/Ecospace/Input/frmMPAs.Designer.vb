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

Namespace Ecospace

    Partial Class frmMPAs
        Inherits ScientificInterfaceShared.Forms.frmEwEGrid

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMPAs))
            Me.m_tsMain = New cEwEToolstrip()
            Me.m_tsbnQuickHelp = New System.Windows.Forms.ToolStripButton()
            Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsbnDefineMPAs = New System.Windows.Forms.ToolStripButton()
            Me.m_grid = New ScientificInterface.Ecospace.gridMPAs()
            Me.m_tlpContent = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lblInfo = New System.Windows.Forms.Label()
            Me.m_tsMain.SuspendLayout()
            Me.m_tlpContent.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tsMain
            '
            resources.ApplyResources(Me.m_tsMain, "m_tsMain")
            Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnQuickHelp, Me.ToolStripSeparator1, Me.m_tsbnDefineMPAs})
            Me.m_tsMain.Name = "m_tsMain"
            '
            'm_tsbnQuickHelp
            '
            Me.m_tsbnQuickHelp.AutoToolTip = False
            Me.m_tsbnQuickHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsbnQuickHelp, "m_tsbnQuickHelp")
            Me.m_tsbnQuickHelp.Name = "m_tsbnQuickHelp"
            '
            'ToolStripSeparator1
            '
            Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
            resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
            '
            'm_tsbnDefineMPAs
            '
            resources.ApplyResources(Me.m_tsbnDefineMPAs, "m_tsbnDefineMPAs")
            Me.m_tsbnDefineMPAs.Name = "m_tsbnDefineMPAs"
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
            Me.m_grid.ContextMenuStyle = SourceGrid2.ContextMenuStyle.None
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "MPA closure"
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
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
            Me.m_grid.UIContext = Nothing
            '
            'm_tlpContent
            '
            resources.ApplyResources(Me.m_tlpContent, "m_tlpContent")
            Me.m_tlpContent.Controls.Add(Me.m_lblInfo, 0, 1)
            Me.m_tlpContent.Controls.Add(Me.m_tsMain, 0, 0)
            Me.m_tlpContent.Controls.Add(Me.m_grid, 0, 2)
            Me.m_tlpContent.Name = "m_tlpContent"
            '
            'm_lblInfo
            '
            resources.ApplyResources(Me.m_lblInfo, "m_lblInfo")
            Me.m_lblInfo.Name = "m_lblInfo"
            '
            'frmMPAs
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_tlpContent)
            Me.Name = "frmMPAs"
            Me.TabText = ""
            Me.m_tsMain.ResumeLayout(False)
            Me.m_tsMain.PerformLayout()
            Me.m_tlpContent.ResumeLayout(False)
            Me.m_tlpContent.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_tsMain As cEwEToolstrip
        Private WithEvents m_tsbnDefineMPAs As System.Windows.Forms.ToolStripButton
        Private WithEvents m_grid As gridMPAs
        Private WithEvents m_tlpContent As TableLayoutPanel
        Private WithEvents m_lblInfo As Label
        Private WithEvents m_tsbnQuickHelp As ToolStripButton
        Private WithEvents ToolStripSeparator1 As ToolStripSeparator
    End Class

End Namespace
