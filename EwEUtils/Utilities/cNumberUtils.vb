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
Imports System.Collections.Generic
Imports System.IO
Imports System.Security.AccessControl

#End Region ' Imports

Namespace Utilities

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper with utility methods for dealing with numbers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cNumberUtils

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a value has a finite value that can be determined.
        ''' </summary>
        ''' <param name="sValue">Value to evaluate.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function IsFinite(ByVal sValue As Single) As Boolean
            If Single.IsInfinity(sValue) Or Single.IsNaN(sValue) Then Return False
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 'Fix' a number by translating NaN, negative and positive infinity
        ''' values to user-defined values.
        ''' </summary>
        ''' <param name="sValue">Value to test.</param>
        ''' <param name="sNaN">Not a number value to substitute.</param>
        ''' <param name="sNegInf">Negative infinity value to substitute.</param>
        ''' <param name="sPosInf">Positive infinity value to substitute.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FixValue(ByVal sValue As Single, _
                                        Optional ByVal sNaN As Single = 0, _
                                        Optional ByVal sNegInf As Single = -1, _
                                        Optional ByVal sPosInf As Single = 1) As Single

            If cNumberUtils.IsFinite(sValue) Then Return sValue

            If Single.IsNegativeInfinity(sValue) Then
                Return sNegInf
            End If
            If Single.IsPositiveInfinity(sValue) Then
                Return sPosInf
            End If
            Return sNaN

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether two numbers can be considered equal.
        ''' </summary>
        ''' <param name="dVal1">First value to compare.</param>
        ''' <param name="dVal2">Second value to compare.</param>
        ''' <param name="dThreshold">Max difference for the two values to be considered equal.</param>
        ''' <returns>True if the two values differ by no more than the given threshold.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function Approximates(dVal1 As Double, dVal2 As Double, dThreshold As Double) As Boolean
            Return (Math.Abs(dVal1 - dVal2) <= dThreshold)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether two numbers can be considered equal.
        ''' </summary>
        ''' <param name="sVal1">First value to compare.</param>
        ''' <param name="sVal2">Second value to compare.</param>
        ''' <param name="sThreshold">Max difference for the two values to be considered equal.</param>
        ''' <returns>True if the two values differ by no more than the given threshold.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function Approximates(sVal1 As Single, sVal2 As Single, sThreshold As Single) As Boolean
            Return (Math.Abs(sVal1 - sVal2) <= sThreshold)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number of relevant decimals in a value.
        ''' </summary>
        ''' <param name="value">The value to check.</param>
        ''' <param name="iNumDigits">The preferred number of digits to display.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function NumRelevantDecimals(value As Object, iNumDigits As Integer) As Integer

            ' Some default
            If (iNumDigits < 0) Then iNumDigits = 3

            If (TypeOf value Is Double) Then
                Dim d As Double = CDbl(value)
                If (Double.IsNaN(d)) Then Return 0
                If (d <= Decimal.MinValue) Or (d >= Decimal.MaxValue) Then Return 0
            End If

            If (TypeOf value Is Single) Then
                Dim s As Single = CSng(value)
                If (Single.IsNaN(s)) Then Return 0
                If (s <= Decimal.MinValue) Or (s >= Decimal.MaxValue) Then Return 0
            End If

            Try
                Dim dValue As Decimal = CDec(value)
                Dim dTest As Decimal = dValue
                Dim iMinPrecision As Integer = 0
                Dim iMaxPrecision As Integer = Math.Min(iNumDigits * 2, 10)

                ' Need to try to figure out num of decimal digits?
                If (dTest <> 0.0) Then
                    ' #Yes: find min number of relevant decimal digits
                    While Math.Floor(dTest) = 0
                        dTest = Decimal.Multiply(dTest, 10)
                        iMinPrecision += 1
                    End While
                    ' First relevant decimal digit found: show iNumDigits decimals including this first value
                    iMinPrecision += (iNumDigits - 1)

                    ' Has decimals?
                    If (Math.Abs(dValue) > 1) Then
                        ' #Yes: Find max number of decimal digits
                        dTest = Decimal.One
                        For iTest As Integer = iNumDigits To 0 Step -1
                            dTest = Decimal.Multiply(dTest, 10)
                            iMaxPrecision = iTest
                            If (dValue <= dTest) Then Exit For
                        Next
                    End If
                End If
                Return Math.Min(Math.Max(iNumDigits, iMinPrecision), iMaxPrecision)
            Catch ex As Exception
                System.Diagnostics.Debug.Assert(False, ex.Message)
            End Try
            Return 0

        End Function


    End Class

End Namespace
