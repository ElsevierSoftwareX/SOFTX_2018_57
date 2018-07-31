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


Public Class cSigmoidShapeFunction
    Inherits cShapeFunction

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Shape"/>
    ''' <summary>
    ''' Returns the points for a sigmoid shape.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Shape(nPoints As Integer) As Single()

        If (Me.ParamsChanged) Then

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Sigmoid base on entering the data range
            'x min, x max, x mid point And slope
            Dim xZero As Single = Me.ParamValue(1)
            Dim xMax As Single = Me.ParamValue(2)
            Dim xMid As Single = Me.ParamValue(3)
            Dim XOpt As Single = Me.ParamValue(4)
            Dim k As Single = Me.ParamValue(5)
            Dim scalar As Single = Me.ParamValue(6)

            'If the user has supplied an XOpt (x axis at optimum) or no K (steepness or slope)
            'then calculate K at XOpt
            If (XOpt <> 0.0) Or (k = 0.0) Then
                k = calSlope()
                If Single.IsNaN(k) Or Single.IsInfinity(k) Then
                    k = Me.ParamValue(5)
                Else
                    Me.ParamValue(5) = k
                End If
            End If


            Dim dx As Single = (xMax - xZero) / nPoints
            For i As Integer = 1 To nPoints
                Dim x As Single = xZero + (i - 1) * dx
                Me.m_points(i) = CSng(1 / (1 + Math.Exp(-k * (x - xMid))))
                ' System.Console.WriteLine(x.ToString + ", " + Me.m_points(i).ToString)
            Next i
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            ' Dim tmpSlp As Single = calSlope()

            Me.ScaleData(nPoints, scalar)

        End If
        Return MyBase.Shape(nPoints)

    End Function


    Private Function calSlope() As Single
        Dim xMid As Single = Me.ParamValue(3)
        Dim Xopt As Single = Me.ParamValue(4)
        Dim Yopt As Single = 0.998
        'solve for k 
        'Yopt = 1 / (1 + Exp(-k * x))
        Dim slope As Single = CSng(Math.Log(-Yopt / (Yopt - 1)) / (Xopt - xMid))
        Return CSng(slope)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub Defaults()
        Me.ParamValue(1) = 0.0
        Me.ParamValue(2) = 1
        Me.ParamValue(3) = 0.5
        Me.ParamValue(4) = 0.9
        Me.ParamValue(5) = 10.0
        Me.ParamValue(6) = 1.0

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.IsCompatible(eDataTypes)"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function IsCompatible(datatype As eDataTypes) As Boolean
        Return Me.IsForcing(datatype) Or Me.IsMediation(datatype)
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
            Return 6
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ParamName"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ParamName(iParam As Integer) As String
        Get
            Select Case iParam
                Case 1 : Return My.Resources.CoreDefaults.PARAM_SIGMOID_XMIN'"X min."'My.Resources.CoreDefaults.PARAM_SD_LEFT
                Case 2 : Return My.Resources.CoreDefaults.PARAM_SIGMOID_XMAX'"X max."'My.Resources.CoreDefaults.PARAM_SD_WIDTH
                Case 3 : Return My.Resources.CoreDefaults.PARAM_SIGMOID_XMID'"X mid point"'My.Resources.CoreDefaults.PARAM_SD_RIGHT

                Case 4 : Return My.Resources.CoreDefaults.PARAM_SIGMOID_XOPT'"X opt. (y = 0.998)" 'My.Resources.CoreDefaults.PARAM_MEAN
                Case 5 : Return My.Resources.CoreDefaults.PARAM_SIGMOID_STEEP'"Steep" 'My.Resources.CoreDefaults.PARAM_MEAN
                Case 6 : Return My.Resources.CoreDefaults.PARAM_SIGMOID_SCALAR '"Y axis max." ' My.Resources.CoreDefaults.PARAM_MAX
            End Select
            Return MyBase.ParamName(iParam)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ShapeFunctionType"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ShapeFunctionType As Long
        Get
            Return eShapeFunctionType.Sigmoid
        End Get
    End Property

    Public Overrides Function Apply(obj As Object) As Boolean
        If Not MyBase.Apply(obj) Then
            Return False
        End If
        Try
            If (TypeOf obj Is cEnviroResponseFunction) Then
                Dim shp As cEnviroResponseFunction = DirectCast(obj, cEnviroResponseFunction)
                Debug.Assert(shp.ShapeFunctionType = eShapeFunctionType.Sigmoid)
                shp.ResponseLeftLimit = Me.ParamValue(1)
                shp.ResponseRightLimit = Me.ParamValue(2)
            End If
        Catch ex As Exception

        End Try

        Return True

    End Function


End Class
