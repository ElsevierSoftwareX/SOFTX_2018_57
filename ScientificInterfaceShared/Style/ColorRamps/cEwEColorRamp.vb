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
    ''' Implements the default EwE5 color ramp that has been used since 2000. This color ramp,
    ''' running from light blue to red, was designed to work well in both digital and
    ''' printed media.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cEwEColorRamp
        Inherits cColorRamp

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return an ARGB colour for a given value.
        ''' </summary>
        ''' <param name="dValue">The value to return the colour for.</param>
        ''' <param name="dValueMax">The maximum value to scale the value to. By default, it is assumed that a colour must be retrieved on a scale from [0..1]</param>
        ''' <returns>The colour for a given value.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function GetColor(ByVal dValue As Double, Optional ByVal dValueMax As Double = 1.0) As Color

            Const sMaxColor As Double = 250

            Dim sRed As Double = 0.0
            Dim sGrn As Double = 0.0
            Dim sBlu As Double = 0.0
            Dim sMid As Double = 0.0

            ' Brutal safety catch
            If (Not dValueMax > 0.0) Then
                Return Nothing
            End If

            ' Apply color offsets
            dValue = Me.RecalcValue(dValue, dValueMax)
            dValueMax = 1.0

            ' Original algorithm (see below, OrigColorGrad) always inverts colours, aargh
            dValue = 1.0 - dValue

            sMid = dValueMax / 2.0

            ' Red
            Select Case dValue
                Case 0 To 2.0 * (dValueMax / 3.0)
                    sRed = 245.0 * ((2.5 * dValueMax / 3.0 - dValue) / sMid)
                Case Else 'dValue >= sMid 
                    sRed = 255.0 * ((dValue - sMid) / sMid)
            End Select

            ' Green
            Select Case dValue
                Case 0 To sMid
                    sGrn = 300.0 * (dValue / sMid)
                Case Else 'dValue >= sMid
                    sGrn = 255.0 + 45.0 * ((dValue - sMid) / sMid)
            End Select

            ' Blue
            Select Case dValue
                Case 0 To dValueMax / 3.0
                    sBlu = 5.0 * (dValue / sMid)
                Case Else 'dValue >= sMid
                    sBlu = 55.0 + 455.0 * ((dValue - sMid) / sMid)
            End Select

            ' Truncate
            sRed = Math.Max(0.0, Math.Min(255.0, Math.Round(sRed)))
            sGrn = Math.Max(0.0, Math.Min(255.0, Math.Round(sGrn)))
            sBlu = Math.Max(0.0, Math.Min(255.0, Math.Round(sBlu)))

            ' Avoid white
            If sBlu > sMaxColor Then
                sRed = Math.Min(sRed, sMaxColor)
                sGrn = Math.Min(sGrn, sMaxColor)
            End If

            Return Color.FromArgb(255, CByte(Math.Round(sRed)), CByte(Math.Round(sGrn)), CByte(Math.Round(sBlu)))
        End Function

#If 0 Then

            ' The original VB6 Color Gradient function as obtained from VC (EcoSim), 20 jun 2005
            Private Function OrigColorGrad(ByVal color_array() As Integer) As Boolean

                Dim maxloop As Integer, i As Integer, Z As Single = 0
                Dim R As Long, G As Long, B As Long
                Dim M2 As Integer
                'Villy: we used an inherited color scale (one of Carls students) up to 12 June 2002;
                'It's the one with 4th order terms remarked out below
                'I programmed a simpler one which has what we're aiming for to Replace(it)
                '
                'get a value between 0 and 1 for z
                'this gives continuous color scale purple - blue - green yellow red maxloop = UBound(color_array)
                'VC17Aug98: Has changed this so as not to use the two first and two last(colors)
                'VC17Aug98: Don't want the brown on top nor the purple at bottom
                M2 = maxloop / 2
                If M2 < 1 Then M2 = 1
                'maxloop = maxloop + 5
                For i = 0 To maxloop 'To 1 Step -1
                    'Z = i / maxloop
                    'R% = 200 - 470 * Z - 2046 * Z ^ 2 + 6963 * Z ^ 3 - 4486! * Z ^ 4:
                    'G% = 5.45 - 252! * Z + 3138 * Z ^ 2 - 3982 * Z ^ 3 + 970! * Z ^ 4:
                    'B% = 169! + 605! * Z - 1903! * Z ^ 2 + 552! * Z ^ 3 + 650! * Z ^ 4
                    Select Case i
                        Case 0 To 2 * (maxloop / 3)
                            R = 245 * ((2.5 * maxloop / 3 - i) / M2)
                        Case Else 'i >= M2 '031230VC: Extra brackets in eq's below are required when M2 is big (integer overflow else)
                            R = 255 * ((i - M2) / M2)
                    End Select
                    Select Case i
                        Case 0 To M2
                            G = 300 * (i / M2)
                        Case Else 'i >= M2
                            G = 255 + 45 * ((i - M2) / M2)
                    End Select
                    Select Case i
                        Case 0 To maxloop / 3
                            B = 5 * (i / M2)
                        Case Else 'i >= M2
                            B = 55 + 455 * ((i - M2) / M2)
                    End Select
                    'If UBound(color_array) > 14 Then Debug.Print i, R, G, B
                    If R < 0 Then R = 0
                    If R > 255 Then R = 255
                    If G < 0 Then G = 0
                    If G > 255 Then G = 255
                    If B < 0 Then B = 0
                    If B > 255 Then B = 255
                    If R > 250 And G > 250 And B > 250 Then R = 250 : G = 250
                    'If i <= maxloop - 1 And i >= 4 Then
                    color_array(maxloop - i) = RGB(R, G, B)
                    'Debug.Print maxloop - i, R, G, B
                Next
                'color_array(0) = QBColor(0)
                'on return c() will contain red to purple to blue gradient
                'the number of colors is based on the array size
                'must be indexed from 0
            End Function

#End If

    End Class

End Namespace
