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

Imports EwECore
Imports EwEUtils.Core

#End Region ' Imports

Public Class cTrapezoidShapeFunction
    Inherits cShapeFunction

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Shape"/>
    ''' <summary>
    ''' Returns the points for a trapezoid shape.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Shape(ByVal nPoints As Integer) As Single()

        If (Me.ParamsChanged) Then
            Dim LeftBott As Single = Me.ParamValue(1)
            Dim LeftTop As Single = Me.ParamValue(2)
            Dim RightBot As Single = Me.ParamValue(3)
            Dim RightTop As Single = Me.ParamValue(4)

            ' JS: IT MAY BE THAT PARAMETERS ARE REVERSED IN THIS METHOD!!!! TO TEST!!

            Dim xpt As Single
            Dim width As Single = RightTop
            Dim x0 As Single = 0
            If LeftBott < 0 Then
                x0 = LeftBott
                width = RightTop - LeftBott
            End If

            Dim dx As Single = width / nPoints

            If RightBot = 0 Then RightBot = 1
            If LeftBott > LeftTop Then LeftTop = LeftBott
            If RightBot < LeftBott Or RightBot < LeftTop Then RightBot = LeftTop + 1

            Dim yVal() As Single = New Single() {0, 0, 1, 1, 0, 0}
            Dim xVal() As Single = New Single() {x0, LeftBott, LeftTop, RightBot, RightTop, width}

            'Break the line up into segments based on the xpoints the user entered
            'The location of the shoulder in the response function is determined by it's index position in the points array
            Dim iSegment() As Integer = New Integer() {0, Me.getIndex(LeftBott, x0, RightTop, nPoints), Me.getIndex(LeftTop, x0, RightTop, nPoints), Me.getIndex(RightBot, x0, RightTop, nPoints), Me.getIndex(RightTop, x0, RightTop, nPoints), nPoints}

            'loop over the segments and interpolate the points on the line
            For i As Integer = 0 To 4
                xpt = xVal(i)
                'loop from the start to the end position in this segment
                'and interpolate the y point on the line
                For j As Integer = iSegment(i) To iSegment(i + 1)
                    Me.m_points(j) = Me.LinearInterp(xpt, xVal(i), xVal(i + 1), yVal(i), yVal(i + 1))
                    xpt += dx
                Next j
            Next i
        End If

        Return MyBase.Shape(nPoints)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub Defaults()
        Me.ParamValue(1) = 1.0F
        Me.ParamValue(2) = 2.0F
        Me.ParamValue(3) = 3.0F
        Me.ParamValue(4) = 4.0F
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
    ''' <summary>
    ''' Get/set parameter 1.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property LeftBottom As Single
        Get
            Return Me.ParamValue(1)
        End Get
        Set(value As Single)
            Me.ParamValue(1) = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set parameter 2.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property LeftTop As Single
        Get
            Return Me.ParamValue(2)
        End Get
        Set(value As Single)
            Me.ParamValue(2) = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set parameter 3.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property RightTop As Single
        Get
            Return Me.ParamValue(3)
        End Get
        Set(value As Single)
            Me.ParamValue(3) = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set parameter 4.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property RightBottom As Single
        Get
            Return Me.ParamValue(4)
        End Get
        Set(value As Single)
            Me.ParamValue(4) = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ParamName"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ParamName(iParam As Integer) As String
        Get
            Select Case iParam
                Case 1 : Return My.Resources.CoreDefaults.PARAM_LEFT_BOTTOM
                Case 2 : Return My.Resources.CoreDefaults.PARAM_LEFT_TOP
                Case 3 : Return My.Resources.CoreDefaults.PARAM_RIGHT_TOP
                Case 4 : Return My.Resources.CoreDefaults.PARAM_RIGHT_BOTTOM
            End Select
            Return "?"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.nParameters"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property nParameters As Integer
        Get
            Return 4
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ShapeFunctionType"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ShapeFunctionType As Long
        Get
            Return eShapeFunctionType.Trapezoid
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Apply"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Apply(obj As Object) As Boolean
        If MyBase.Apply(obj) Then
            Dim shape As cEnviroResponseFunction = TryCast(obj, cEnviroResponseFunction)
            If shape IsNot Nothing Then
                'set the extent of the data in the shape
                Dim left As Single = 0
                If Me.ParamValue(1) < 0 Then left = Me.ParamValue(1)
                shape.ResponseLeftLimit = left
                shape.ResponseRightLimit = Me.ParamValue(4)
            End If

        End If
    End Function

#Region " Internals "

    Private Function getIndex(Xvalue As Single, x0 As Single, x1 As Single, TotalNPoints As Integer) As Integer
        'Debug.Assert(Xvalue >= x0 And Xvalue <= x1, Me.ToString + ".getIndex() value out of bounds.")
        'use the linear interpolator to find the index positon of Value
        'In this case we are interpolating the number of data points Xvalue is along the line
        'x0 and x1 are the first and last values of the x axis
        '0 and TotalNPoints are the number of data points/array indexes
        Return CInt(LinearInterp(Xvalue, x0, x1, 0, TotalNPoints))
    End Function

    Private Function LinearInterp(ByVal x As Single, x0 As Single, x1 As Single, y0 As Single, y1 As Single) As Single
        If ((x1 - x0) = 0) Then
            'mid point on the y axis
            Return (y0 + y1) / 2.0F
        Else
            Return y0 + (y1 - y0) * ((x - x0) / (x1 - x0))
        End If
    End Function

#End Region ' Internals 

End Class
