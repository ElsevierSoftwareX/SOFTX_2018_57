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

    Partial Class ucChangeShapeType
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.m_tlpInput = New System.Windows.Forms.TableLayoutPanel()
            Me.m_hdrShape = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lbShapeFunctionTypes = New System.Windows.Forms.ListBox()
            Me.m_hdrParams = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_grid = New ScientificInterfaceShared.gridShapeFunctionParameters()
            Me.m_tlpInput.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tlpInput
            '
            Me.m_tlpInput.ColumnCount = 1
            Me.m_tlpInput.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpInput.Controls.Add(Me.m_hdrShape, 0, 0)
            Me.m_tlpInput.Controls.Add(Me.m_lbShapeFunctionTypes, 0, 1)
            Me.m_tlpInput.Controls.Add(Me.m_hdrParams, 0, 2)
            Me.m_tlpInput.Controls.Add(Me.m_grid, 0, 3)
            Me.m_tlpInput.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tlpInput.Location = New System.Drawing.Point(0, 0)
            Me.m_tlpInput.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tlpInput.Name = "m_tlpInput"
            Me.m_tlpInput.RowCount = 4
            Me.m_tlpInput.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpInput.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpInput.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpInput.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpInput.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.m_tlpInput.Size = New System.Drawing.Size(229, 341)
            Me.m_tlpInput.TabIndex = 3
            '
            'm_hdrShape
            '
            Me.m_hdrShape.CanCollapseParent = False
            Me.m_hdrShape.CollapsedParentHeight = 0
            Me.m_hdrShape.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_hdrShape.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrShape.IsCollapsed = False
            Me.m_hdrShape.Location = New System.Drawing.Point(3, 0)
            Me.m_hdrShape.Name = "m_hdrShape"
            Me.m_hdrShape.Size = New System.Drawing.Size(223, 18)
            Me.m_hdrShape.TabIndex = 0
            Me.m_hdrShape.Text = "Shape type"
            Me.m_hdrShape.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_lbShapeFunctionTypes
            '
            Me.m_lbShapeFunctionTypes.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_lbShapeFunctionTypes.FormattingEnabled = True
            Me.m_lbShapeFunctionTypes.IntegralHeight = False
            Me.m_lbShapeFunctionTypes.Location = New System.Drawing.Point(0, 18)
            Me.m_lbShapeFunctionTypes.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
            Me.m_lbShapeFunctionTypes.Name = "m_lbShapeFunctionTypes"
            Me.m_lbShapeFunctionTypes.Size = New System.Drawing.Size(229, 148)
            Me.m_lbShapeFunctionTypes.Sorted = True
            Me.m_lbShapeFunctionTypes.TabIndex = 1
            '
            'm_hdrParams
            '
            Me.m_hdrParams.CanCollapseParent = False
            Me.m_hdrParams.CollapsedParentHeight = 0
            Me.m_hdrParams.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_hdrParams.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrParams.IsCollapsed = False
            Me.m_hdrParams.Location = New System.Drawing.Point(3, 170)
            Me.m_hdrParams.Name = "m_hdrParams"
            Me.m_hdrParams.Size = New System.Drawing.Size(223, 18)
            Me.m_hdrParams.TabIndex = 2
            Me.m_hdrParams.Text = "Parameters"
            Me.m_hdrParams.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
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
            Me.m_grid.DataName = "ShapeFunction"
            Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Location = New System.Drawing.Point(0, 188)
            Me.m_grid.Margin = New System.Windows.Forms.Padding(0)
            Me.m_grid.Name = "m_grid"
            Me.m_grid.ShapeFunction = Nothing
            Me.m_grid.Size = New System.Drawing.Size(229, 153)
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.TabIndex = 5
            Me.m_grid.TrackPropertySelection = False
            Me.m_grid.UIContext = Nothing
            '
            'ucChangeShapeType
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.m_tlpInput)
            Me.Name = "ucChangeShapeType"
            Me.Size = New System.Drawing.Size(229, 341)
            Me.m_tlpInput.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub

        Private WithEvents m_tlpInput As TableLayoutPanel
        Private WithEvents m_hdrShape As Controls.cEwEHeaderLabel
        Private WithEvents m_lbShapeFunctionTypes As ListBox
        Private WithEvents m_hdrParams As Controls.cEwEHeaderLabel
        Private WithEvents m_grid As gridShapeFunctionParameters
    End Class

End Namespace