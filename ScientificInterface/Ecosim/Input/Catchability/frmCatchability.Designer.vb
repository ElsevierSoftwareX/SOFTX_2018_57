﻿' ===============================================================================
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

    Partial Class frmCatchability
        Inherits frmEwEGrid

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCatchability))
            Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.ToolStripLabel1 = New System.Windows.Forms.ToolStripLabel()
            Me.m_tscbFleets = New System.Windows.Forms.ToolStripComboBox()
            Me.m_grdCatchability = New ScientificInterface.gridCatchability()
            Me.m_ts.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_ts
            '
            Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripLabel1, Me.m_tscbFleets})
            resources.ApplyResources(Me.m_ts, "m_ts")
            Me.m_ts.Name = "m_ts"
            Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'ToolStripLabel1
            '
            Me.ToolStripLabel1.Name = "ToolStripLabel1"
            resources.ApplyResources(Me.ToolStripLabel1, "ToolStripLabel1")
            '
            'm_tscbFleets
            '
            Me.m_tscbFleets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_tscbFleets.Name = "m_tscbFleets"
            resources.ApplyResources(Me.m_tscbFleets, "m_tscbFleets")
            '
            'm_grdCatchability
            '
            Me.m_grdCatchability.AllowBlockSelect = True
            Me.m_grdCatchability.AutoSizeMinHeight = 10
            Me.m_grdCatchability.AutoSizeMinWidth = 10
            Me.m_grdCatchability.AutoStretchColumnsToFitWidth = False
            Me.m_grdCatchability.AutoStretchRowsToFitHeight = False
            Me.m_grdCatchability.BackColor = System.Drawing.Color.White
            Me.m_grdCatchability.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grdCatchability.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grdCatchability.CustomSort = False
            Me.m_grdCatchability.DataName = ""
            resources.ApplyResources(Me.m_grdCatchability, "m_grdCatchability")
            Me.m_grdCatchability.FixedColumnWidths = True
            Me.m_grdCatchability.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grdCatchability.GridToolTipActive = True
            Me.m_grdCatchability.IsLayoutSuspended = False
            Me.m_grdCatchability.IsOutputGrid = False
            Me.m_grdCatchability.Name = "m_grdCatchability"
            Me.m_grdCatchability.SelectedFleetIndex = 1
            Me.m_grdCatchability.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grdCatchability.UIContext = Nothing
            '
            'frmCatchability
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_grdCatchability)
            Me.Controls.Add(Me.m_ts)
            Me.Name = "frmCatchability"
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.m_ts.ResumeLayout(False)
            Me.m_ts.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Friend WithEvents m_tscbFleets As ToolStripComboBox
        Friend WithEvents ToolStripLabel1 As ToolStripLabel
        Private WithEvents m_grdCatchability As gridCatchability
        Private WithEvents m_ts As cEwEToolstrip
    End Class



End Namespace
