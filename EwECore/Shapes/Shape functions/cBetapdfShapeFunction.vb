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
Imports EwEUtils.Core

#End Region ' Imports

Public Class cBetapdfShapeFunction
    Inherits cShapeFunction

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ParamName"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ParamName(iParam As Integer) As String
        Get
            Select Case iParam
                Case 1 : Return My.Resources.CoreDefaults.PARAM_ALPHA
                Case 2 : Return My.Resources.CoreDefaults.PARAM_BETA
                Case 3 : Return My.Resources.CoreDefaults.PARAM_YSCALAR
            End Select
            Return "?"
        End Get
    End Property


    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Shape"/>
    ''' <summary>
    ''' Returns the points for an Betapdf shape.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Shape(nPoints As Integer) As Single()

        If (Me.ParamsChanged) Then
            Dim A As Single = Me.ParamValue(1)
            Dim B As Single = Me.ParamValue(2)
            For i As Integer = 1 To nPoints
                Dim x As Single = CSng(i / (nPoints + 1))
                Me.m_points(i) = CSng(Me.betaPDF(A, B, x))
            Next i
        End If

        Me.ScaleData(nPoints, Me.ParamValue(3))

        Return MyBase.Shape(nPoints)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub Defaults()
        Me.ParamValue(1) = 2.0F
        Me.ParamValue(2) = 3.0F
        Me.ParamValue(3) = 1.0F
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.IsCompatible(eDataTypes)"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function IsCompatible(datatype As eDataTypes) As Boolean
        Return Me.IsMediation(datatype)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.IsDistribution()"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property IsDistribution As Boolean
        Get
            Return True
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.nParameters"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property nParameters As Integer
        Get
            Return 3
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ShapeFunctionType"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ShapeFunctionType As Long
        Get
            Return eShapeFunctionType.Betapdf
        End Get
    End Property

#Region " Calculations "

    Private Function betaPDF(ByVal a As Single, ByVal b As Single, ByVal x As Single) As Single
        'Beta Distribution pdf from Wikipedia
        'http://en.wikipedia.org/wiki/Beta_distribution
        Return CSng((x ^ (a - 1) * (1 - x) ^ (b - 1)) / beta(a, b))
    End Function

    Private Function beta(ByVal a As Single, ByVal b As Single) As Single
        'Beta function from Wikipedia
        'http://en.wikipedia.org/wiki/Beta_function
        Return CSng(Gamma(a) * Gamma(b) / Gamma(a + b))
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Gamma function
    ''' </summary>
    ''' <param name="xx"></param>
    ''' -------------------------------------------------------------------
    Private Function Gamma(ByVal xx As Double) As Double
        'HACK gammln(x) returns the log n gamma used by Numeric Recipies in C betai(a,b,x) 
        'we need gamma for beta(x) so remove the log
        Return Math.Exp(Me.gammln(xx))
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Gamma Log n from Numeric Recipies in C
    ''' </summary>
    ''' <param name="xx"></param>
    ''' <returns></returns>
    ''' -------------------------------------------------------------------
    Private Function gammln(ByVal xx As Double) As Double
        'from NRC-2
        Dim x As Double, y As Double, tmp As Double, ser As Double
        Dim cof() As Double = {76.180091729471457, -86.505320329416776, _
                              24.014098240830911, -1.231739572450155, _
                              0.001208650973866179, -0.000005395239384953}
        Dim j As Integer
        x = xx
        tmp = x + 5.5
        tmp -= (x + 0.5) * Math.Log(tmp)
        ser = 1.0000000001900149

        For j = 0 To 5
            y += 1
            ser += cof(j) / (x + y)
        Next

        Return -tmp + Math.Log(2.5066282746310007 * ser / x)

    End Function

#End Region ' Calculations

End Class
