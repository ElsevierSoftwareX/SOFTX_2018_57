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

Public MustInherit Class cShoulderShapeFunction
    Inherits cShapeFunction

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Shape"/>
    ''' <summary>
    ''' Returns the points for a shoulder-contoured shape.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Shape(nPoints As Integer) As Single()

        If (Me.ParamsChanged) Then

            Dim LeftPt As Single = Me.ParamValue(1)
            Dim RightPt As Single = Me.ParamValue(2)
            Dim WidthX As Single = Me.ParamValue(3)
            Dim xpt As Single
            Dim width As Single = Me.ParamValue(3)
            'x0 is the value of the first x point
            'if the user set the first point to < zero 
            'then shift x0 over by one point to get a bit of room for the shoulder 
            Dim x0 As Single = 0
            If LeftPt < 0 Then
                x0 = LeftPt - 1.0F
                width = WidthX - x0
            End If

            Dim dx As Single = width / nPoints

            If WidthX = 0 Then WidthX = 1
            If LeftPt > RightPt Then RightPt = LeftPt
            If WidthX < LeftPt Or WidthX < RightPt Then WidthX = RightPt + 1

            Dim yVal() As Single
            If (Me.ShapeFunctionType = eShapeFunctionType.LeftShoulder) Then
                yVal = New Single() {1, 1, 0, 0}
            Else
                yVal = New Single() {0, 0, 1, 1}
            End If

            Dim xVal() As Single = New Single() {x0, LeftPt, RightPt, WidthX}
            'Break the line up into segments based on the xpoints the user entered
            'The location of the shoulder in the response function is determined by it's index position in the points array
            Dim iSegment() As Integer = New Integer() {0, Me.getIndex(LeftPt, x0, WidthX, nPoints), Me.getIndex(RightPt, x0, WidthX, nPoints), nPoints}

            ' JS 160914: This is not right; the original shape cannot be modified until the user clicks 'OK'
            '            This has to move to some kind of 'Apply' function
            'Dim shape As cEnviroResponseFunction = TryCast(Me.m_shape, cEnviroResponseFunction)
            'If shape IsNot Nothing Then
            '    'set the extent of the data in the shape
            '    shape.ResponseLeftLimit = x0
            '    shape.ResponseRightLimit = sYBase
            'End If

            'loop over the segments and interpolate the points on the line
            For i As Integer = 0 To 2
                xpt = xVal(i)
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
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.IsCompatible"/>
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
    ''' <inheritdocs cref="cShapeFunction.ParamName"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ParamName(iParam As Integer) As String
        Get
            Select Case iParam
                Case 1 : Return My.Resources.CoreDefaults.PARAM_LEFTPOINT
                Case 2 : Return My.Resources.CoreDefaults.PARAM_RIGHTPOINT
                Case 3 : Return My.Resources.CoreDefaults.PARAM_WIDTH
            End Select
            Return "?"
        End Get
    End Property

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

    Public Overrides Function Apply(obj As Object) As Boolean
        If MyBase.Apply(obj) Then
            Dim shape As cEnviroResponseFunction = TryCast(obj, cEnviroResponseFunction)
            If shape IsNot Nothing Then
                'set the extent of the data in the shape
                Dim left As Single = 0 'Me.ParamValue(1)
                If Me.ParamValue(1) < 0 Then left = Me.ParamValue(1) - 1.0F
                shape.ResponseLeftLimit = left
                shape.ResponseRightLimit = Me.ParamValue(3)
            End If

        End If
    End Function

End Class

Public Class cRightShoulderShapeFunction
    Inherits cShoulderShapeFunction

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ShapeFunctionType As Long
        Get
            Return eShapeFunctionType.RightShoulder
        End Get
    End Property

End Class

Public Class cLeftShoulderShapeFunction
    Inherits cShoulderShapeFunction

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ShapeFunctionType As Long
        Get
            Return eShapeFunctionType.LeftShoulder
        End Get
    End Property

End Class
