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
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' EwEGridVisualizerBase is a base class visualizer that provides 
    ''' <see cref="cStyleGuide.eStyleFlags">status</see> colour feedback.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public MustInherit Class cEwEGridVisualizerBase
        Inherits SourceGrid2.VisualModels.Common

#Region " Private bits "

        ''' <summary>Border width for Highlighted cells</summary>
        Private m_nHighlightBorderWidth As Integer = 2
        ''' <summary>Text indentation level.</summary>
        Private m_iTextIndent As Integer = 0

#End Region ' Private bits 

#Region " Constructor "

        Public Sub New()
            MyBase.New(False)
        End Sub

#End Region ' Constructor

#Region " Public configuration bits "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the text indentation.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Indentation() As Integer
            Get
                Return Me.m_iTextIndent
            End Get
            Set(ByVal value As Integer)
                Me.m_iTextIndent = Math.Max(0, value)
            End Set
        End Property

#End Region ' Public configuration bits 

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

            If (sg IsNot Nothing) Then
                ' Get style colors, but exclude remarks style because remarks are rendered in a different manner
                sg.GetStyleColors(style And Not cStyleGuide.eStyleFlags.Remarks, clrFore, clrBack)
            End If

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

            If (sg IsNot Nothing) Then
                If (sg.ShowPedigree) Then
                    Dim sPedigree As Single = Me.Pedigree(cell)
                    If (sPedigree > 0) Then
                        cPedigreeIndicator.Paint(sg, rc, e.Graphics, sPedigree)
                    End If
                End If
            End If

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
        ''' Overidden to draw cell content using EwE color styles
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub DrawCell_ImageAndText( _
                ByVal cell As SourceGrid2.Cells.ICellVirtual, _
                ByVal pos As SourceGrid2.Position, _
                ByVal e As System.Windows.Forms.PaintEventArgs, _
                ByVal rc As System.Drawing.Rectangle, _
                ByVal status As SourceGrid2.DrawCellStatus)

            If cell Is Nothing Then Return

            Dim rcBorder As RectangleBorder = Me.Border
            Dim rcClient As New Rectangle(rc.X, rc.Y, rc.Width, rc.Height)

            Dim sg As cStyleGuide = Me.StyleGuide(cell)
            Dim style As cStyleGuide.eStyleFlags = Me.Style(cell)
            Dim clrBack As Color = Me.BackColor
            Dim clrFore As Color = Me.ForeColor

            If (sg IsNot Nothing) Then
                sg.GetStyleColors(style, clrFore, clrBack)
            End If

            If Not cell.Grid.Enabled Then
                clrFore = EwEUtils.Utilities.cColorUtils.GetVariant(clrFore, 0.5)
            End If

            ' Does cell have focus?
            If (status = DrawCellStatus.Focus) Then
                ' #Yes: obtain standard focus border properties
                rcBorder = Me.FocusBorder
                clrFore = Me.FocusForeColor
                ' Is cell selected?
            ElseIf (status = DrawCellStatus.Selected) Then
                ' #Yes: obtain standard selected border properties
                rcBorder = Me.SelectionBorder
                clrFore = Me.SelectionForeColor
            End If

            ' Include indentation, if any
            rcClient.X += Me.m_iTextIndent
            rcClient.Width -= Me.m_iTextIndent

            ' Render Image and Text
            Dim ftCell As Font = Me.GetCellFont()
            Using ft As New Font(ftCell,
                                 ftCell.Style Or If((style And cStyleGuide.eStyleFlags.Taxon) > 0, FontStyle.Italic, ftCell.Style))

                Dim strText As String = cell.GetDisplayText(pos)
                Dim al As ContentAlignment = Me.ImageAlignment

                If String.IsNullOrWhiteSpace(strText) Then
                    al = ContentAlignment.MiddleCenter
                Else
                    If cSystemUtils.IsRightToLeft() Then
                        al = ContentAlignment.MiddleRight
                    Else
                        al = ContentAlignment.MiddleLeft
                    End If
                End If

                Utility.PaintImageAndText(e.Graphics, rcClient,
                    Me.Image, al, Me.ImageStretch,
                    strText, Me.StringFormat, Me.AlignTextToImage, rcBorder,
                    clrFore, ft)
            End Using

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

            If cell Is Nothing Then Return

            Dim sg As cStyleGuide = Me.StyleGuide(cell)
            Dim style As cStyleGuide.eStyleFlags = Me.Style(cell)
            Dim clrFore As Color = Me.ForeColor
            Dim rcBorder As RectangleBorder = Me.Border

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
                ' Rendering an EwE base cell?
                If (TypeOf cell Is IEwECell) Then
                    ' #Yes: obtain cell style
                    Return DirectCast(cell, IEwECell).Style
                End If
                Return cStyleGuide.eStyleFlags.OK
            End Get
        End Property

        Protected ReadOnly Property Pedigree(ByVal cell As SourceGrid2.Cells.ICellVirtual) As Single
            Get
                ' Rendering an EwE base cell?
                If (TypeOf cell Is EwECellBase) Then
                    ' #Yes: obtain cell style
                    Return DirectCast(cell, EwECellBase).Pedigree
                End If
                Return 0
            End Get
        End Property

#End Region ' Internals

    End Class

End Namespace

