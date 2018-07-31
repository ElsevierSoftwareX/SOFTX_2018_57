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

'==============================================================================
'
' $Log: ucGlyph.vb,v $
' Revision 1.1  2008/09/26 07:31:16  sherman
' --== DELETED HISTORY ==--
'
' Revision 1.1  2008/06/01 23:45:09  jeroens
' Separated from Scientific Interface
'
' Revision 1.3  2007/12/02 03:39:46  jeroens
' * Selection rendered w system highlight color
' * Fixed selection / image alignment
'
' Revision 1.2  2007/12/01 22:07:22  jeroens
' * Fixed rendering bug
'
' Revision 1.1  2007/12/01 19:39:00  jeroens
' Initial version
'
'==============================================================================

Option Strict On

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Control managing a single image in a <see cref="ucGlyphSelect">Glyph select control</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class ucGlyph

#Region " Private vars "

        ''' <summary>The glyph image maintained by this control.</summary>
        Private m_image As Image = Nothing
        ''' <summary>Glyph selection state.</summary>
        Private m_bSelected As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="img">Image to maintain in an instance.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal img As Image)
            Me.m_image = img
            Me.InitializeComponent()

            ' Prettynize
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)
        End Sub

#End Region ' Constructor

#Region " Public interfaces "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selection state of this glyph.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Selected() As Boolean
            Get
                Return Me.m_bSelected
            End Get
            Set(ByVal value As Boolean)
                Me.m_bSelected = value
                Me.Invalidate()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the image kept in an instance.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Image() As Image
            Get
                Return Me.m_image
            End Get
        End Property

#End Region ' Public interfaces

#Region " Events "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Draw me! Draw me! Yes! ME!
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub DoPaint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

            If (Me.m_image Is Nothing) Then Return

            Dim rcHatch As Rectangle = New Rectangle(Me.ClientRectangle.X + 1, Me.ClientRectangle.Y + 1, Me.ClientRectangle.Width - 2, Me.ClientRectangle.Height - 2)

            e.Graphics.FillRectangle(SystemBrushes.Window, Me.ClientRectangle)
            If Me.m_bSelected Then
                e.Graphics.DrawRectangle(New Pen(System.Drawing.SystemColors.Highlight, 2), rcHatch)
            End If
            rcHatch.Offset(New System.Drawing.Point(2, 2))
            rcHatch.Width -= 5
            rcHatch.Height -= 5

            e.Graphics.FillRectangle(New TextureBrush(Me.m_image), rcHatch)
            e.Graphics.DrawRectangle(Pens.Black, rcHatch)

        End Sub

#End Region ' Events

    End Class

End Namespace ' Controls
