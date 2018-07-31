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

Option Strict On
Imports System.Drawing.Drawing2D
Imports EwEUtils.Utilities

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Helper class, draws an arrow. Yippee.
    ''' </summary>
    ''' ===========================================================================
    Public Class cArrowIndicator

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Draws an arrow in the indicated rectangle with an indicated colour, 
        ''' at a given angle with a given relative size.
        ''' </summary>
        ''' <param name="g">Graphics to draw onto.</param>
        ''' <param name="clr">Colour of the arrow to draw.</param>
        ''' <param name="rc">Rectangle to draw the arrow into.</param>
        ''' <param name="sAngle">Clockwise angle (in degrees, NOT radians) for the arrow. 0 is straight up.</param>
        ''' <param name="sSize">Size of the angle. [0, 1], 0 is smallest size, 1 will
        ''' size the arrow to optimally fit in the rectangle with 1 pixel margin.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub DrawArrow(ByVal g As Graphics, _
                                    ByVal clr As Color, _
                                    ByVal rc As Rectangle, _
                                    ByVal sAngle As Single, _
                                    ByVal sSize As Single, _
                                    Optional ByVal bFilledArrow As Boolean = True)

            Dim matOrg As Matrix = g.Transform
            Dim matArr As New Matrix()
            ' Arrow is 10 px high. Scale with 1 px all around 
            Dim sScale As Single = CSng(Math.Min(rc.Width / 12, rc.Height / 12)) * sSize
            Dim sDX As Single = CSng(Math.Max((rc.Width - rc.Height) / 2, 0))
            Dim sDY As Single = CSng(Math.Max((rc.Height - rc.Width) / 2, 0))

            ' Anything to draw?
            If (sSize > 0) Then

                ' #Yes: Prepare transformation matrix
                ' - Scale arrow to fit rect
                matArr.Scale(sScale, sScale)
                ' - Move arrow to center of rect
                matArr.Translate((6 / sSize) + ((rc.X + sDX) / sScale), (6 / sSize) + ((rc.Y + sDY) / sScale))
                ' - Rotate arrow to given value
                matArr.Rotate(sAngle)

                ' Apply arrow transformation matrix
                g.Transform = matArr
                ' Draw arrow in requested color
                Using p As New Pen(clr)
                    p.StartCap = LineCap.Round
                    p.CustomEndCap = New AdjustableArrowCap(2.2, 2.2, bFilledArrow)
                    g.DrawLine(p, 0, -5, 0, 5)
                End Using
                ' Clean up borrowed DC by restoring original transformation matrix
                g.Transform = matOrg

            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Draws an arrow in the indicated rectangle with an indicated colour, 
        ''' at a given horizontal and vertical verlocity.
        ''' </summary>
        ''' <param name="g">Graphics to draw onto.</param>
        ''' <param name="clr">Colour of the arrow to draw.</param>
        ''' <param name="rc">Rectangle to draw the arrow into.</param>
        ''' <param name="dx">X velocity.</param>
        ''' <param name="dy">Y velocity.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub DrawArrowDxDy(ByVal g As Graphics, _
                                    ByVal clr As Color, _
                                    ByVal rc As Rectangle, _
                                    ByVal dx As Single, _
                                    ByVal dy As Single, _
                                    Optional ByVal bFilledArrow As Boolean = True)

            ' Leave a margin
            'rc.Inflate(-2, -2)
            ' Calc center
            Dim ptfCenter As New PointF(CSng(rc.X + rc.Width / 2), CSng(rc.Y + rc.Height / 2))
            ' Calc arrow size
            Dim szfHalfArrow As New SizeF(rc.Width * dx / 2.0!, rc.Height * dy / 2.0!)

            Using p As New Pen(clr, 0.001!)
                p.StartCap = LineCap.Round
                p.CustomEndCap = New AdjustableArrowCap(3, 3, bFilledArrow)
                g.DrawLine(p, _
                           ptfCenter.X - szfHalfArrow.Width, ptfCenter.Y - szfHalfArrow.Height, _
                           ptfCenter.X + szfHalfArrow.Width, ptfCenter.Y + szfHalfArrow.Height)
            End Using

        End Sub

#End Region ' Public access

    End Class

End Namespace ' Controls
