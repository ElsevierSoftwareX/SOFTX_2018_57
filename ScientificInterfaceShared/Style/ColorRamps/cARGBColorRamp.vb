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
Imports System.Drawing

Namespace Style

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' <para>Implements a <see cref="cColorRamp">color ramp</see>, where colours are specified in ARGB values.</para>
    ''' </summary>
    ''' <remarks>
    ''' <para>For examples on how to use this class, refer to the following methods:
    ''' <list type="bullet">
    ''' <item><description><see cref="cARGBColorRamp">Constructor</see></description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Public Class cARGBColorRamp
        Inherits cColorRamp

        ''' <summary>Gradient break colours</summary>
        Private m_aclr() As Color
        ''' <summary>Gradient break values - ABSOLUTE</summary>
        Private m_adBreakAbs() As Double
        ''' <summary>Gradient break values - RELATIVE</summary>
        Private m_adBreakRel() As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initializes a new instance of the ARGBColorRamp class.
        ''' </summary>
        ''' <param name="aColors">The colour breaks to use.</param>
        ''' <param name="adPositions">The position of each colour break, 
        ''' relative to its predessesor.</param>
        ''' <remarks>
        ''' The following snippet illustrates how to create a valid ARGB color ramp:
        ''' <code>
        ''' ' Define a three level colour ramp
        ''' Dim aclr(2) as Color
        ''' Dim adPositions(2) as Integer
        ''' 
        ''' ' Ramp begins with light blue at position 0
        ''' aclr(0) = Color.FromARGB(255, 200, 200, 255)
        ''' adPositions(0) = 0
        ''' ' At 40%, the ramp is a green tone
        ''' aclr(1) = Color.FromARGB(255, 100, 255, 100)
        ''' adPositions(1) = 0.4
        ''' ' At 100% (0.4 + 0.6) the ramp is deep red
        ''' aclr(2) = Color.FromARGB(255, 255, 25, 25)
        ''' adPositions(2) = 0.6
        ''' 
        ''' ' Create the ramp
        ''' Dim crARGB as New ARGBColorRamp(aclr, adPositions)
        ''' 
        ''' ' Now get the value at 50%, let's see what happens...
        ''' Dim clr as Color = crARGB.GetColor(0.5)
        ''' </code>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal aColors() As Color, ByVal adPositions() As Double)

            MyBase.New()

            Dim clr As Color = Nothing
            Dim dTotalPos As Double = 0.0

            ' Validate input
            If (aColors Is Nothing) Then Throw New Exception("Missing required parameter aColors")
            If (adPositions Is Nothing) Then Throw New Exception("Missing required parameter adPositions")
            If (aColors.Length <> adPositions.Length) Then Throw New Exception("Number of colors and positions do not match")

            Me.GradientColors = aColors
            Me.GradientBreaks = adPositions

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return an ARGB colour for a given value.
        ''' </summary>
        ''' <param name="dValue">The value to return the colour for.</param>
        ''' <param name="dValueMax">The maximum value to scale the value to. By default, it is assumed that a colour must be retrieved on a scale from [0..1]</param>
        ''' <returns>The colour for a given value.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function GetColor(ByVal dValue As Double, Optional ByVal dValueMax As Double = 1.0) As Color

            ' Pre
            Debug.Assert(Me.m_adBreakRel.Length = Me.m_aclr.Length)

            ' Normalize nValue to nValueMax
            Dim nIndex As Integer = 0
            Dim bFound As Boolean = False

            ' Apply color offsets
            dValue = Me.RecalcValue(dValue, dValueMax)
            dValueMax = 1.0

            ' Find first index
            bFound = (dValue <= Me.m_adBreakAbs(0))
            While Not bFound
                nIndex += 1
                bFound = (nIndex = Me.m_adBreakAbs.Length)
                If Not bFound Then
                    bFound = (dValue <= Me.m_adBreakAbs(nIndex))
                End If
            End While

            ' Below first level? Return first colour without interpolating
            If (nIndex = 0) Then Return Me.m_aclr(0)
            ' Past last level? Return formar-last level without interpolating
            If (nIndex = Me.m_adBreakAbs.Length) Then Return Me.m_aclr(nIndex - 1)
            ' Exactly at a known level? Return the level colour withour interpolating
            If dValue = Me.m_adBreakAbs(nIndex) Then Return Me.m_aclr(nIndex)

            ' must interpolate
            Dim c1 As Color = Me.m_aclr(nIndex - 1)
            Dim c2 As Color = Me.m_aclr(nIndex)
            Dim dX As Double = Me.m_adBreakAbs(nIndex) - Me.m_adBreakAbs(nIndex - 1)
            Dim dPosX As Double = dValue - Me.m_adBreakAbs(nIndex - 1)

            Dim dRatio As Double = (dPosX / dX)

            If (dRatio > 1.0) Then
                dRatio = 1.0
            End If

            Return Color.FromArgb(Me.Interpolate(c1.A, c2.A, dRatio), _
                                  Me.Interpolate(c1.R, c2.R, dRatio), _
                                  Me.Interpolate(c1.G, c2.G, dRatio), _
                                  Me.Interpolate(c1.B, c2.B, dRatio))

        End Function

        Private Function Interpolate(ByVal nVal1 As Integer, ByVal nVal2 As Integer, ByVal dRatio As Double) As Integer
            Try
                Return CInt(Math.Round(nVal1 + (nVal2 - nVal1) * dRatio))
            Catch ex As Exception
                Return 0
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the colours to use for every <see cref="GradientBreaks">gradient stop</see>.
        ''' </summary>
        ''' <remarks>
        ''' Note that the number of breaks and colors must match when trying to
        ''' use the gradient.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property GradientColors As Color()
            Get
                Return Me.m_aclr
            End Get
            Set(ByVal value As Color())
                Me.m_aclr = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the position for gradient breaks, relative to its predessesor.
        ''' </summary>
        ''' <remarks>
        ''' Note that the number of breaks and colors must match when trying to
        ''' use the gradient.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property GradientBreaks() As Double()
            Get
                Return Me.m_adBreakRel
            End Get
            Set(ByVal value As Double())
                ReDim Me.m_adBreakAbs(value.Length - 1)
                ReDim Me.m_adBreakRel(value.Length - 1)
                Dim dTotalPos As Double = 0.0#
                For i As Integer = 0 To value.Length - 1
                    dTotalPos += CDbl(Math.Abs(value(i)))
                    Me.m_adBreakAbs(i) = dTotalPos
                    Me.m_adBreakRel(i) = value(i)
                Next
            End Set
        End Property

    End Class

End Namespace
