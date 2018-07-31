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
Imports System.Globalization
Imports System.Threading

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Factory class for customizing cursors.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cCursorFactory

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a custom cursor, combining a given cursor and an overlay image.
        ''' </summary>
        ''' <param name="crsBase"></param>
        ''' <param name="imgAdd"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetCursorOverlay(ByVal crsBase As Cursor, imgAdd As Image) As Cursor

            If (imgAdd Is Nothing) Then Return crsBase

            Dim ci As CultureInfo = Thread.CurrentThread.CurrentUICulture

            ' Hotspot always at center of new cursor. Not handy!
            Dim rcBase As New Rectangle(0, 0, crsBase.Size.Width, crsBase.Size.Height)
            Dim ptOffset As Point = New Point(rcBase.Width - crsBase.HotSpot.X, rcBase.Height - crsBase.HotSpot.Y)
            rcBase.Offset(ptOffset)

            Dim crsOut As Cursor = Nothing
            Dim rcOut As New Rectangle(0, 0, 2 * CInt(Math.Max(rcBase.Width, rcBase.Width / 2 + imgAdd.Width)), 2 * CInt(Math.Max(rcBase.Height, rcBase.Height / 2 + imgAdd.Height)))
            Dim bmp As New Bitmap(rcOut.Width, rcOut.Height, Imaging.PixelFormat.Format32bppArgb)

            Using g As Graphics = Graphics.FromImage(bmp)
                ' Draw cursor, positioned at hotspot
                crsBase.Draw(g, rcBase)
                ' ToDo: this is hack, need to properly position overlay
                If ci.TextInfo.IsRightToLeft Then
                    g.DrawImage(imgAdd, New Rectangle(0, CInt(rcOut.Height - imgAdd.Height - 1), imgAdd.Width, imgAdd.Height))
                Else
                    g.DrawImage(imgAdd, New Rectangle(CInt(rcOut.Width - imgAdd.Width - 1), CInt(rcOut.Height - imgAdd.Height - 1), imgAdd.Width, imgAdd.Height))
                End If
            End Using

            crsOut = New Cursor(bmp.GetHicon())
            bmp.Dispose()

            Return crsOut

        End Function

    End Class

End Namespace

