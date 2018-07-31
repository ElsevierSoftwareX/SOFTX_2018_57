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
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells.Real

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' A visualizer for rendering EwE column header cells.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cEwECheckBoxVisualizer
        : Inherits SourceGrid2.VisualModels.CheckBox

        ''' <summary>Border width for Highlighted cells</summary>
        Private m_nHighlightBorderWidth As Integer = 4

        Public Sub New()
            MyBase.New(False)
        End Sub

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overidden to draw background using EwE color styles
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub DrawCell_Background( _
                ByVal cell As SourceGrid2.Cells.ICellVirtual, _
                ByVal pos As SourceGrid2.Position, _
                ByVal e As System.Windows.Forms.PaintEventArgs, _
                ByVal rc As System.Drawing.Rectangle, _
                ByVal status As SourceGrid2.DrawCellStatus)

            If cell Is Nothing Then Return

            Dim sg As cStyleGuide = Me.StyleGuide(cell)
            Dim style As cStyleGuide.eStyleFlags = Me.Style(cell)
            Dim clrBack As Color = Me.BackColor
            Dim clrFore As Color = Nothing ' Not used here

            If (sg Is Nothing) Then Return

            ' Get style colors, but exclude remarks style because remarks are 
            ' rendered in a different manner
            sg.GetStyleColors(style And Not cStyleGuide.eStyleFlags.Remarks, clrFore, clrBack)

            ' Does cell have focus?
            If (status = DrawCellStatus.Focus) Then
                ' #Yes: obtain standard focus bk color
                clrBack = FocusBackColor
                ' Is cell selected?
            ElseIf (status = DrawCellStatus.Selected) Then
                ' #Yes: obtain standard selection bk color
                clrBack = SelectionBackColor
            End If

            ' Draw the background
            Using br As New SolidBrush(clrBack)
                e.Graphics.FillRectangle(br, rc)
            End Using

            ' Check if need to render specific styles
            If (style = 0) Then
                ' #No styles to render: done drawing
                Return
            End If

            ' Need to draw remarks indicator?
            If ((style And cStyleGuide.eStyleFlags.Remarks) > 0) And (sg IsNot Nothing) Then
                ' #Yes: draw remarks indicator
                cRemarksIndicator.Paint(sg.ApplicationColor(cStyleGuide.eApplicationColorType.REMARKS_BACKGROUND), rc, e.Graphics, True, cSystemUtils.IsRightToLeft)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overidden to draw cell border using EwE color styles
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub DrawCell_Border(ByVal cell As SourceGrid2.Cells.ICellVirtual, _
                                                ByVal pos As SourceGrid2.Position, _
                                                ByVal e As System.Windows.Forms.PaintEventArgs, _
                                                ByVal rc As System.Drawing.Rectangle, _
                                                ByVal status As SourceGrid2.DrawCellStatus)

            If (cell Is Nothing) Then Return

            Dim sg As cStyleGuide = Me.StyleGuide(cell)
            Dim style As cStyleGuide.eStyleFlags = Me.Style(cell)
            Dim clrFore As Color = Me.ForeColor
            Dim rcBorder As RectangleBorder = Me.Border

            If (sg Is Nothing) Then Return

            ' Does cell have focus?
            If (status = DrawCellStatus.Focus) Then
                ' #Yes: obtain standard focus border properties
                rcBorder = FocusBorder
                ' Is cell selected?
            ElseIf (status = DrawCellStatus.Selected) Then
                ' #Yes: obtain standard selected border properties
                rcBorder = SelectionBorder
            End If

            ' Need to render highlightboder?
            If ((style And cStyleGuide.eStyleFlags.Highlight) > 0) And (sg IsNot Nothing) Then
                ' #Yes: render highlight border
                rcBorder = New RectangleBorder( _
                    New Border(sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT), Me.m_nHighlightBorderWidth))
            End If

            ' Draw the border
            ControlPaint.DrawBorder(e.Graphics, rc, _
                rcBorder.Left.Color, _
                rcBorder.Left.Width, _
                ButtonBorderStyle.Solid, _
                rcBorder.Top.Color, _
                rcBorder.Top.Width, _
                ButtonBorderStyle.Solid, _
                rcBorder.Right.Color, _
                rcBorder.Right.Width, _
                ButtonBorderStyle.Solid, _
                rcBorder.Bottom.Color, _
                rcBorder.Bottom.Width, _
                ButtonBorderStyle.Solid)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract the <see cref="cStyleGuide">style guide</see> from a cell, 
        ''' if possible.
        ''' </summary>
        ''' <param name="cell">The cell to query.</param>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property StyleGuide(ByVal cell As SourceGrid2.Cells.ICellVirtual) As cStyleGuide
            Get
                If (TypeOf cell Is IEwECell) Then
                    Dim uic As cUIContext = DirectCast(cell, IEwECell).UIContext
                    If (uic IsNot Nothing) Then
                        Return uic.StyleGuide
                    End If
                End If
                Return Nothing
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract the <see cref="cStyleGuide.eStyleFlags">style</see> from 
        ''' a given cell, if possible.
        ''' </summary>
        ''' <param name="cell">The cell to query.</param>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property Style(ByVal cell As SourceGrid2.Cells.ICellVirtual) As cStyleGuide.eStyleFlags
            Get
                ' Rendering a cell with an associated property?
                If (TypeOf cell Is IEwECell) Then
                    ' #Yes: obtain cell style
                    Return DirectCast(cell, IEwECell).Style()
                End If
                Return cStyleGuide.eStyleFlags.OK
            End Get
        End Property

#End Region ' Internals

    End Class

End Namespace