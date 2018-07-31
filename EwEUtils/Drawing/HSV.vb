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

Namespace Drawing

    Public Class HSV

        Private m_iHue As Integer = 0
        Private m_iSaturation As Integer = 0
        Private m_iValue As Integer = 0

        Public Sub New(ByVal h As Integer, ByVal s As Integer, ByVal v As Integer)
            Me.m_iHue = h
            Me.m_iSaturation = s
            Me.m_iValue = v
        End Sub

        Public Property Hue() As Integer
            Get
                Return Me.m_iHue
            End Get
            Set(ByVal p_iHue As Integer)
                Me.m_iHue = p_iHue
            End Set
        End Property

        Public Property Saturation() As Integer
            Get
                Return Me.m_iSaturation
            End Get
            Set(ByVal p_iSaturation As Integer)
                Me.m_iSaturation = p_iSaturation
            End Set
        End Property

        Public Property Value() As Integer
            Get
                Return Me.m_iValue
            End Get
            Set(ByVal p_iValue As Integer)
                Me.m_iValue = p_iValue
            End Set
        End Property

        Public Shared Function ToColor(ByVal hsv As HSV) As Color

            Dim h As Decimal
            Dim s As Decimal
            Dim v As Decimal

            Dim r As Decimal
            Dim g As Decimal
            Dim b As Decimal

            'Console.WriteLine("{0},{1},{2}", hsv.H, hsv.S, hsv.V)

            ' Scale Hue to be between 0 and 360. Saturation
            ' and Value scale to be between 0 and 1.
            h = (hsv.Hue / 255D * 360) Mod 360
            s = hsv.Saturation / 255D
            v = hsv.Value / 255D

            If s = 0 Then
                ' If s is 0, all colors are the same.
                ' This is some flavor of gray.
                r = v
                g = v
                b = v
            Else
                Dim p As Decimal
                Dim q As Decimal
                Dim t As Decimal

                Dim fractionalSector As Decimal
                Dim sectorNumber As Integer
                Dim sectorPos As Decimal

                ' The color wheel consists of 6 sectors.
                ' Figure out which sector you're in.
                sectorPos = h / 60
                sectorNumber = CInt(Math.Floor(sectorPos))

                ' Get the fractional part of the sector.
                ' That is, how many degrees into the sector
                ' are you?
                fractionalSector = sectorPos - sectorNumber

                ' Calculate values for the three axes
                ' of the color. 
                p = v * (1 - s)
                q = v * (1 - (s * fractionalSector))
                t = v * (1 - (s * (1 - fractionalSector)))

                ' Assign the fractional colors to r, g, and b
                ' based on the sector the angle is in.
                Select Case sectorNumber
                    Case 0
                        r = v
                        g = t
                        b = p

                    Case 1
                        r = q
                        g = v
                        b = p

                    Case 2
                        r = p
                        g = v
                        b = t

                    Case 3
                        r = p
                        g = q
                        b = v

                    Case 4
                        r = t
                        g = p
                        b = v

                    Case 5
                        r = v
                        g = p
                        b = q
                End Select
            End If

            Return Color.FromArgb(CInt(Math.Min(255, Math.Max(0, r * 255))), _
                CInt(Math.Min(255, Math.Max(0, g * 255))), _
                CInt(Math.Min(255, Math.Max(0, b * 255))))

        End Function

    End Class

End Namespace ' Drawing
