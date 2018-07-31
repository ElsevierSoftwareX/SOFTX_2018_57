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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

#Region " Imports "

Option Strict On

Imports ScientificInterfaceShared.Forms
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports 

Namespace Ecopath.Input

    Partial Class frmTaxonInput
        Inherits frmEwEGrid

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
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTaxonInput))
            Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnEditTaxa = New System.Windows.Forms.ToolStripButton()
            Me.m_grid = New ScientificInterface.Ecopath.Input.gridTaxonInput()
            Me.m_tscmbUpdate = New System.Windows.Forms.ToolStripSplitButton()
            Me.m_ts.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_ts
            '
            Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnEditTaxa, Me.m_tscmbUpdate})
            Me.m_ts.Location = New System.Drawing.Point(0, 0)
            Me.m_ts.Name = "m_ts"
            Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            Me.m_ts.Size = New System.Drawing.Size(655, 25)
            Me.m_ts.TabIndex = 0
            '
            'm_tsbnEditTaxa
            '
            Me.m_tsbnEditTaxa.Image = SharedResources.taxon
            Me.m_tsbnEditTaxa.ImageTransparentColor = System.Drawing.Color.Magenta
            Me.m_tsbnEditTaxa.Name = "m_tsbnEditTaxa"
            Me.m_tsbnEditTaxa.Size = New System.Drawing.Size(94, 22)
            Me.m_tsbnEditTaxa.Text = "Define taxa..."
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
            Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_grid.FixedColumnWidths = True
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.Location = New System.Drawing.Point(0, 25)
            Me.m_grid.Name = "m_grid"
            Me.m_grid.Size = New System.Drawing.Size(655, 237)
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.TabIndex = 1
            Me.m_grid.UIContext = Nothing
            '
            'm_tscmbUpdate
            '
            Me.m_tscmbUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            Me.m_tscmbUpdate.Image = SharedResources.Refresh
            Me.m_tscmbUpdate.Name = "m_tscmbUpdate"
            Me.m_tscmbUpdate.Size = New System.Drawing.Size(32, 22)
            Me.m_tscmbUpdate.Text = "Update"
            Me.m_tscmbUpdate.Visible = False
            '
            'frmTaxonInput
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(655, 262)
            Me.Controls.Add(Me.m_grid)
            Me.Controls.Add(Me.m_ts)
            Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Name = "frmTaxonInput"
            Me.ShowInTaskbar = False
            Me.Text = "Taxonomy"
            Me.m_ts.ResumeLayout(False)
            Me.m_ts.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_ts As cEwEToolstrip
        Private WithEvents m_grid As ScientificInterface.Ecopath.Input.gridTaxonInput
        Private WithEvents m_tsbnEditTaxa As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tscmbUpdate As System.Windows.Forms.ToolStripSplitButton
    End Class

End Namespace