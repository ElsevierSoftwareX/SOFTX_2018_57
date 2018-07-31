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

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class, renders a remarks indicator.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cRemarksIndicator

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Renders a remarks indicator onto a given canvas
        ''' </summary>
         ''' <param name="rcClip">Clip boundary to fit the remarks indicator in</param>
        ''' <param name="g">The canvas to render onto</param>
        ''' <param name="bHasRemarks">States whether the indicator is rendered as having remarks (true) or
        ''' as ready for receiving remarks (false)</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Paint(ByVal clrFill As Color, _
                                ByVal rcClip As Rectangle, _
                                ByVal g As Graphics, _
                                ByVal bHasRemarks As Boolean, _
                                ByVal bRightToLeft As Boolean)

            If (bHasRemarks) Then

                Dim pt() As Point = GetPoints(bRightToLeft, rcClip)
  
                Using br As New SolidBrush(clrFill)
                    g.FillPolygon(br, pt)
                End Using
                g.DrawLine(SystemPens.ControlDark, pt(1), pt(2))
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, determines the bounding box for a remarks indicator. This method could be handy when
        ''' testing for remarks indicator mouse hits.
        ''' </summary>
        ''' <param name="rcClip">Coordinates of the area to get the remarks indicator bounding box for.</param>
        ''' <returns>The bounding box that fully encapsulates the Remarks indicator.</returns>
        ''' -----------------------------------------------------------------------
        Private Shared Function GetBounds(ByVal bRightToLeft As Boolean, ByVal rcClip As Rectangle) As Rectangle
            Dim pt As Point() = cRemarksIndicator.GetPoints(bRightToLeft, rcClip)
            Return New Rectangle(Math.Min(pt(0).X, pt(1).X), pt(0).Y, Math.Abs(pt(1).X - pt(0).X), pt(2).Y - pt(0).Y)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the remarks indicator corner points for a given cell boundary. The current UI culture
        ''' reading order is evaluated to determine the position and layout of the remarks indicator.
        ''' </summary>
        ''' <param name="rcClip">Clip boundary to calculate the remarks indicator for</param>
        ''' <returns>A series of <see cref="Point">points</see></returns>
        ''' -----------------------------------------------------------------------
        Private Shared Function GetPoints(ByVal bRightToLeft As Boolean, _
                                          ByVal rcClip As Rectangle) As Point()
            Dim nSize As Integer = CInt(Math.Floor(rcClip.Height / 2.5))
            Dim pt(2) As Point

            If (bRightToLeft) Then
                ' 0--1---
                ' | /       
                ' 2
                ' |
                pt(0) = New Point(rcClip.X + 1, rcClip.Y)
                pt(1) = New Point(rcClip.X + 1 + nSize, rcClip.Y)
                pt(2) = New Point(rcClip.X + 1, rcClip.Y + nSize)
            Else
                ' ---1--0
                '     \ |
                '       2
                '       |
                pt(0) = New Point(rcClip.X + rcClip.Width - 1, rcClip.Y)
                pt(1) = New Point(rcClip.X + rcClip.Width - 1 - nSize, rcClip.Y)
                pt(2) = New Point(rcClip.X + rcClip.Width - 1, rcClip.Y + nSize)
            End If
            Return pt
        End Function

    End Class

End Namespace ' Controls
