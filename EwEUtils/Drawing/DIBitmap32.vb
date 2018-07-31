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
' Copyright 1991-2013 UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

Option Strict On
Imports System
Imports System.Data
Imports System.Drawing

Namespace Drawing

    ''' <summary>
    ''' Represents a 32-bit ARGB device-independant bitmap.
    ''' </summary>
    ''' <remarks>
    ''' <para>Original code written by</para>
    ''' <para>Author: Michael A Seel</para>
    ''' <para>Email: mseel1976[at]gmail[dot]com</para>
    ''' <para>url: http://www.freevbcode.com/ShowCode.asp?ID=7801</para>
    ''' </remarks>
    Public Class DIBitmap32
        Private m_pxPixels As Pixel32(,)
        Private m_szSize As Size

        ''' <summary>
        ''' Copies one or more Pixel32 structures to the specified memory address.
        ''' </summary>
        ''' <param name="toAddress"></param>
        ''' <param name="fromPixel32"></param>
        ''' <param name="bytesToCopy"></param>
        Private Declare Sub CopyMemoryPtr Lib "kernel32" Alias "RtlMoveMemory" (ByVal toAddress As IntPtr, ByRef fromPixel32 As Pixel32, ByVal bytesToCopy As Integer)

        ''' <summary>
        ''' Copies one or more Pixel32 structures from the specified memory address.
        ''' </summary>
        ''' <param name="toPixel32"></param>
        ''' <param name="fromAddress"></param>
        ''' <param name="bytesToCopy"></param>
        Private Declare Sub CopyMemoryPtr Lib "kernel32" Alias "RtlMoveMemory" (ByRef toPixel32 As Pixel32, ByVal fromAddress As IntPtr, ByVal bytesToCopy As Integer)

        Public Sub New(ByVal width As Integer, ByVal height As Integer)
            ReDim m_pxPixels(height - 1, width - 1)
            m_szSize = New Size(width, height)
        End Sub

        Public Sub New(ByVal size As Size)
            ReDim m_pxPixels(size.Height - 1, size.Width - 1)
            m_szSize = size
        End Sub

        Public Sub New(ByVal original As DIBitmap32)
            With original
                m_pxPixels = .m_pxPixels
                m_szSize = .m_szSize
            End With
        End Sub

        Public Sub New(ByVal original As Image)
            Me.New(CType(original, DIBitmap32))
        End Sub

        Public Function Clone() As DIBitmap32
            Return New DIBitmap32(Me)
        End Function

        Public ReadOnly Property Width() As Integer
            Get
                Return m_szSize.Width
            End Get
        End Property

        Public ReadOnly Property Height() As Integer
            Get
                Return m_szSize.Height
            End Get
        End Property

        Public ReadOnly Property Size() As Size
            Get
                Return m_szSize
            End Get
        End Property

        Public Property Pixel(ByVal x As Integer, ByVal y As Integer) As Pixel32
            Get
                Return m_pxPixels(y, x)
            End Get
            Set(ByVal value As Pixel32)
                m_pxPixels(y, x) = value
            End Set
        End Property

        ' Provides a method for CType() conversion or implicit conversion of a DIBitmap32 to a System.Drawing.Bitmap.
        Public Shared Widening Operator CType(ByVal dib As DIBitmap32) As Bitmap
            If dib Is Nothing Then
                Return Nothing
            Else
                With dib
                    ' Create an empty bitmap to recieve the bit information.
                    Dim bmpTmp As New Bitmap(.Width, .Height, Imaging.PixelFormat.Format32bppArgb)
                    ' Create a BitmapData instance for working with the bitmap's bits.
                    Dim datTmp As Imaging.BitmapData = bmpTmp.LockBits(New Rectangle(0, 0, .Width, .Height), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppArgb)
                    ' Copy the bit data to the memory address of the bitmap's bits.
                    CopyMemoryPtr(datTmp.Scan0, dib.m_pxPixels(0, 0), .Width * .Height * 4)
                    ' Release the lock on the bits.
                    bmpTmp.UnlockBits(datTmp)
                    ' Return the new bitmap.
                    Return bmpTmp
                End With
            End If
        End Operator

        ' Provides a method for CType() conversion of a System.Drawing.Image to a DIBitmap32. Narrows because all images are converted to 32-bit ARGB.
        Public Shared Narrowing Operator CType(ByVal img As Image) As DIBitmap32
            If img Is Nothing Then
                Return Nothing
            Else
                ' Create a temporary bitmap clone of the provided image.
                Dim bmpTmp As Bitmap = New Bitmap(img)
                With bmpTmp
                    ' Create a BitmapData instance for working with the bitmap bits.
                    Dim datTmp As Imaging.BitmapData = .LockBits(New Rectangle(0, 0, .Width, .Height), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
                    ' Create an empty DIBitmap32 to receive the bit information.
                    Dim dibTmp As New DIBitmap32(.Width, .Height)
                    ' Copy the bit data from the memory address of the bitmap's bits.
                    CopyMemoryPtr(dibTmp.m_pxPixels(0, 0), datTmp.Scan0, .Width * .Height * 4)
                    ' Release the lock on the bits.
                    .UnlockBits(datTmp)
                    ' Dispose of the temporary bitmap.
                    .Dispose()
                    ' Return the new DIBitmap32.
                    Return dibTmp
                End With
            End If
        End Operator
    End Class


    ''' <summary>
    ''' Represents a 32-bit ARGB pixel.
    ''' </summary>
    Public Structure Pixel32
        ' The following 4 variables must be declared in the order Blue, Green, Red, Alpha.
        Public Blue As Byte
        Public Green As Byte
        Public Red As Byte
        Public Alpha As Byte

        Public Sub New(ByVal red As Byte, ByVal green As Byte, ByVal blue As Byte, Optional ByVal alpha As Byte = 255)
            With Me
                .Alpha = alpha
                .Red = red
                .Green = green
                .Blue = blue
            End With
        End Sub

        ''' <summary>
        ''' Creates a pixel from a HSBvalue
        ''' </summary>
        ''' <param name="hue"></param>
        ''' <param name="saturation"></param>
        ''' <param name="brightness"></param>
        ''' <param name="alpha"></param>
        ''' <returns></returns>
        ''' <remarks>Uses 0 to 360 (degree) hue, 0.0 to 1.0 saturation, 0.0 to 1.0 brightness.</remarks>
        Public Shared Function FromHSB(ByVal hue As Single, ByVal saturation As Single, ByVal brightness As Single, Optional ByVal alpha As Byte = 255) As Pixel32
            If saturation = 0 Then
                Dim bTmp As Byte = CByte(brightness * 255)
                Return New Pixel32(bTmp, bTmp, bTmp)
            Else
                hue /= 360
                Dim fA As Single
                If brightness > 0.5 Then
                    fA = brightness + saturation - (brightness * saturation)
                Else
                    fA = brightness * (1 + saturation)
                End If
                Dim fB As Single = (2 * brightness) - fA
                Dim fC() As Single = {CSng(hue + (1 / 3)), hue, CSng(hue - (1 / 3))}
                Dim fClr(2) As Single
                For i As Integer = 0 To 2
                    If fC(i) < 0 Then fC(i) += 1
                    If fC(i) > 1 Then fC(i) -= 1
                    If 6 * fC(i) < 1 Then
                        fClr(i) = fB + ((fA - fB) * fC(i) * 6)
                    ElseIf 2 * fC(i) < 1 Then
                        fClr(i) = fA
                    ElseIf 3 * fC(i) < 2 Then
                        fClr(i) = CSng(fB + ((fA - fB) * ((2 / 3) - fC(i)) * 6))
                    Else
                        fClr(i) = fB
                    End If
                Next i
                Return New Pixel32(CByte(fClr(0) * 255), CByte(fClr(1) * 255), CByte(fClr(2) * 255), alpha)
            End If
        End Function

        Public Function GetHue() As Single
            Return CType(Me, Color).GetHue()
        End Function

        Public Function GetSaturation() As Single
            Return CType(Me, Color).GetSaturation()
        End Function

        Public Function GetBrightness() As Single
            Return CType(Me, Color).GetBrightness()
        End Function

        Public Function GetGrayValue(Optional ByVal redContent As Single = 0.222, Optional ByVal greenContent As Single = 0.707, Optional ByVal blueContent As Single = 0.071) As Byte
            Dim fTotal As Single = redContent + greenContent + blueContent
            Return CByte(Me.Red * redContent / fTotal + Me.Green * greenContent / fTotal + Me.Blue * blueContent / fTotal)
        End Function

        Public Function BlendTo(ByVal destination As Pixel32, Optional ByVal additionalAlpha As Byte = 255) As Pixel32
            Dim pxRet As Pixel32 = destination
            Dim fSrc As Single = CSng((Me.Alpha / 255) * (additionalAlpha / 255))
            Dim fDst As Single = 1 - fSrc
            With pxRet
                .Red = CByte(CInt((Me.Red * fSrc) + (.Red * fDst)))
                .Green = CByte(CInt((Me.Green * fSrc) + (.Green * fDst)))
                .Blue = CByte(CInt((Me.Blue * fSrc) + (.Blue * fDst)))
                .Alpha = CByte(.Alpha + ((255 - .Alpha) * fSrc))
            End With
            Return pxRet
        End Function

        Public Shared Widening Operator CType(ByVal px As Pixel32) As Color
            With px
                Return Color.FromArgb(.Alpha, .Red, .Green, .Blue)
            End With
        End Operator

        Public Shared Widening Operator CType(ByVal px As Pixel32) As Integer
            With px
                Return Color.FromArgb(.Alpha, .Red, .Green, .Blue).ToArgb()
            End With
        End Operator

        Public Shared Widening Operator CType(ByVal cl As Color) As Pixel32
            With cl
                Return New Pixel32(.R, .G, .B, .A)
            End With
        End Operator

        Public Shared Widening Operator CType(ByVal i As Integer) As Pixel32
            Dim vRet As Pixel32 = Color.FromArgb(i)
        End Operator

        Public Shared Operator =(ByVal px1 As Pixel32, ByVal px2 As Pixel32) As Boolean
            Return (px1.Red = px2.Red) AndAlso (px1.Green = px2.Green) AndAlso (px1.Blue = px2.Blue) AndAlso (px1.Alpha = px2.Alpha)
        End Operator

        Public Shared Operator <>(ByVal px1 As Pixel32, ByVal px2 As Pixel32) As Boolean
            Return (px1.Red <> px2.Red) OrElse (px1.Green <> px2.Green) OrElse (px1.Blue <> px2.Blue) OrElse (px1.Alpha <> px2.Alpha) ' <- OrElse instead of Or increases speed.
        End Operator

        ' Similar to the = operator, but ignores the alpha channel.
        Public Shared Operator Like(ByVal px1 As Pixel32, ByVal px2 As Pixel32) As Boolean
            Return (px1.Red = px2.Red) AndAlso (px1.Green = px2.Green) AndAlso (px1.Blue = px2.Blue) ' <- AndAlso instead of And increases speed.
        End Operator
    End Structure

End Namespace ' Drawing
