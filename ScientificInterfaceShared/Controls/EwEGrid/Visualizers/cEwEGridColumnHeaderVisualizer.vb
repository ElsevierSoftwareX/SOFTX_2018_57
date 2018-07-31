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

#Region " Imports "

Option Strict On
Imports System.Windows.Forms
Imports System.Drawing
Imports EwECore
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' A visualizer for rendering EwE column header cells.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cEwEGridColumnHeaderVisualizer
        : Inherits SourceGrid2.VisualModels.Header

        Public Sub New(Optional ByVal alignment As ContentAlignment = ContentAlignment.MiddleCenter)
            MyBase.New(False)
            Me.TextAlignment = alignment
            Me.WordWrap = True
            Me.AlignTextToImage = True
        End Sub

        Protected Overrides Sub DrawCell_Border(ByVal p_Cell As SourceGrid2.Cells.ICellVirtual, ByVal p_CellPosition As SourceGrid2.Position, ByVal e As System.Windows.Forms.PaintEventArgs, ByVal p_ClientRectangle As System.Drawing.Rectangle, ByVal p_Status As SourceGrid2.DrawCellStatus)

            Dim border As RectangleBorder = Me.Border
            Dim rc As Rectangle = p_ClientRectangle
            Dim l_BackColor As Color = Me.BackColor

            If (p_Status = DrawCellStatus.Focus) Then
                l_BackColor = FocusBackColor
            ElseIf (p_Status = DrawCellStatus.Selected) Then
                l_BackColor = SelectionBackColor
                l_BackColor = BackColor
            End If

            ' Draw the border
            ControlPaint.DrawBorder(e.Graphics, rc, _
                SystemColors.ButtonHighlight, 1, ButtonBorderStyle.Solid, _
                Color.Transparent, 0, ButtonBorderStyle.Solid, _
                SystemColors.ButtonShadow, 1, ButtonBorderStyle.Solid, _
                SystemColors.ButtonShadow, 1, ButtonBorderStyle.Solid)

        End Sub

        Protected Overrides Sub DrawCell_ImageAndText(cell As SourceGrid2.Cells.ICellVirtual, pos As SourceGrid2.Position, e As System.Windows.Forms.PaintEventArgs, rc As System.Drawing.Rectangle, p_Status As SourceGrid2.DrawCellStatus)
            If cell.Grid.Enabled Then
                Me.ForeColor = SystemColors.ControlText
            Else
                Me.ForeColor = EwEUtils.Utilities.cColorUtils.GetVariant(SystemColors.ControlText, 0.5)
            End If
            MyBase.DrawCell_ImageAndText(cell, pos, e, rc, p_Status)
        End Sub

    End Class

End Namespace