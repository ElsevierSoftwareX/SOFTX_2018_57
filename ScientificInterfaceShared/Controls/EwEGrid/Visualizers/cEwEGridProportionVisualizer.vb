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
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A visualizer that renders cell values [0, 1] as a progress bar.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cEwEGridProportionVisualizer
        Inherits cEwEGridVisualizerBase

        Protected Overrides Sub DrawCell_ImageAndText(ByVal cell As SourceGrid2.Cells.ICellVirtual, _
                                                      ByVal pos As SourceGrid2.Position, _
                                                      ByVal e As System.Windows.Forms.PaintEventArgs, _
                                                      ByVal rc As System.Drawing.Rectangle, _
                                                      ByVal status As SourceGrid2.DrawCellStatus)

            Dim objVal As Object = cell.GetValue(pos)
            If Not (TypeOf objVal Is Single) Then Return

            Dim sVal As Single = CSng(objVal)
            Dim rcBox As New Rectangle(rc.Left + 3, rc.Top + 4, Math.Max(0, rc.Width - 6), Math.Max(0, rc.Height - 9))
            Dim rcFill As New Rectangle(rcBox.Left, rcBox.Top, CInt(Math.Min(sVal, 1) * rcBox.Width), rcBox.Height)

            e.Graphics.FillRectangle(SystemBrushes.Window, rcBox)
            e.Graphics.FillRectangle(SystemBrushes.Highlight, rcFill)
            e.Graphics.DrawRectangle(SystemPens.ControlDark, rcBox)

        End Sub

    End Class

End Namespace
