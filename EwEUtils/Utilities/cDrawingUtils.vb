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
Imports System
Imports System.Drawing

#End Region ' Imports

Namespace Utilities

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class providing a collection of <see cref="Drawing">Drawing</see>-related utility methods.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cDrawingUtils

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a <see cref="ContentAlignment">ContentAlignment</see> flag
        ''' into a <see cref="StringFormat">StringFormat</see> flag.
        ''' </summary>
        ''' <param name="ca">Content alignment flag to convert.</param>
        ''' <returns>A StringFormat value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ContentAlignmentToStringFormat(ByVal ca As System.Drawing.ContentAlignment) As StringFormat
            Dim style As New StringFormat()

            Select Case ca
                Case ContentAlignment.BottomLeft
                    style.Alignment = StringAlignment.Near
                    style.LineAlignment = StringAlignment.Far
                Case ContentAlignment.BottomRight
                    style.Alignment = StringAlignment.Far
                    style.LineAlignment = StringAlignment.Far
                Case ContentAlignment.BottomCenter
                    style.Alignment = StringAlignment.Center
                    style.LineAlignment = StringAlignment.Far
                Case ContentAlignment.MiddleLeft
                    style.Alignment = StringAlignment.Near
                    style.LineAlignment = StringAlignment.Center
                Case ContentAlignment.MiddleRight
                    style.Alignment = StringAlignment.Far
                    style.LineAlignment = StringAlignment.Center
                Case ContentAlignment.MiddleCenter
                    style.Alignment = StringAlignment.Center
                    style.LineAlignment = StringAlignment.Center
                Case ContentAlignment.TopLeft
                    style.Alignment = StringAlignment.Near
                    style.LineAlignment = StringAlignment.Near
                Case ContentAlignment.TopRight
                    style.Alignment = StringAlignment.Far
                    style.LineAlignment = StringAlignment.Near
                Case ContentAlignment.TopCenter
                    style.Alignment = StringAlignment.Center
                    style.LineAlignment = StringAlignment.Near
            End Select
            Return style

        End Function

        Public Shared Function BitmapFromIcon(icon As Icon, Optional sz As Size = Nothing) As Bitmap

            If (sz = Nothing) Then
                sz = icon.Size
            End If

            Dim bmp As New Bitmap(sz.Width, sz.Height, Imaging.PixelFormat.Format32bppArgb)
            Using g As Graphics = Graphics.FromImage(bmp)
                Try
                    g.DrawIcon(icon, 0, 0)
                Catch ex As Exception
                    bmp = Nothing
                End Try
            End Using
            Return bmp

        End Function

    End Class

End Namespace

