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
    ''' A cell visualizer that reflects a cell value as a color.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cEwEGridColorVisualizer
        Inherits SourceGrid2.VisualModels.Common

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overidden to render cell value as a color
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub DrawCell_ImageAndText( _
                ByVal p_Cell As SourceGrid2.Cells.ICellVirtual, _
                ByVal p_CellPosition As SourceGrid2.Position, _
                ByVal e As System.Windows.Forms.PaintEventArgs, _
                ByVal p_ClientRectangle As System.Drawing.Rectangle, _
                ByVal p_Status As SourceGrid2.DrawCellStatus)

            Dim value As Object = p_Cell.GetValue(p_CellPosition)

            If Not (TypeOf value Is Color) Then Return

            Dim clr As Color = DirectCast(value, Color)
            Dim rcColor As New Rectangle(p_ClientRectangle.X + 2, p_ClientRectangle.Y + 2, p_ClientRectangle.Width - 4, p_ClientRectangle.Height - 4)

            ' Draw the background
            Using br As New SolidBrush(clr)
                e.Graphics.FillRectangle(br, rcColor)
            End Using

        End Sub

    End Class

End Namespace