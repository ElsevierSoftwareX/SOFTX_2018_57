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

Public Class cNormalShapeFunction
    Inherits cShapeFunction

    Private Enum eParNames
        SDLeft = 1
        DataWidth = 2
        SDRight = 3
        Mean = 4
        Max = 5
    End Enum

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ParamName"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ParamName(iParam As Integer) As String
        Get
            Select Case iParam
                Case 1 : Return My.Resources.CoreDefaults.PARAM_SD_LEFT
                Case 2 : Return My.Resources.CoreDefaults.PARAM_SD_WIDTH
                Case 3 : Return My.Resources.CoreDefaults.PARAM_SD_RIGHT
                Case 4 : Return My.Resources.CoreDefaults.PARAM_MEAN
                Case 5 : Return My.Resources.CoreDefaults.PARAM_MAX
            End Select
            Return MyBase.ParamName(iParam)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ParamStatus"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ParamStatus(iParam As Integer) As eStatusFlags
        Get
            If (iParam = 2) Then Return eStatusFlags.NotEditable ' width
            Return MyBase.ParamStatus(iParam)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Shape"/>
    ''' <summary>
    ''' Returns the points for an normal distributed shape.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Shape(nPoints As Integer) As Single()

        If (Me.ParamsChanged) Then
            Dim sdRight As Single = Me.ParamValue(eParNames.SDRight)
            'normal distribution with a mean of Zero
            'User defines 
            '   Standard deviation on the left and right
            '   Width of the data in standard deviations 
            '   Width is important because values outside the bounds 
            '       are just the first or last value in the shape

            'Normal and Beta shapes are not used for Forcing functions
            'so it is only the shape we are interested in not that actual data
            'how the shape affects the data is defined by the user by where they place the baseline
            'If these are to be used as Forcing Function then we will need a way to 'scale' the data
            'as there is no way to in the Forcing Function interface to select where the baseline is.
            'Dim nPtHalf As Integer = CInt(nPoints * (ParamValue(eParNames.SDLeft) / ParamValue(eParNames.SDRight)))
            Dim nPtHalf As Integer = nPoints \ 2
            'SD left
            Dim sd As Single = Me.ParamValue(eParNames.SDLeft) + 0.0000001F
            'width in SD
            Me.ParamValue(eParNames.DataWidth) = ParamValue(eParNames.SDLeft) * 5 + ParamValue(eParNames.SDRight) * 5
            'Dim Wsd As Single = Me.ParamValue(3)
            Dim Wsd As Single = Me.ParamValue(eParNames.DataWidth)

            'Delta X 
            Dim dx As Single = Wsd / (nPoints - 1)
            'Start X
            Dim x0 As Single = -Wsd * 0.5F
            'Dim x0 As Single = Me.ParamValue(eParNames.Mean) - ParamValue(eParNames.SDLeft) * 5
            Dim max As Single = Me.ParamValue(eParNames.Max)
            Dim x As Single
            For i As Integer = 1 To nPoints
                If i > nPtHalf Then
                    'swap the sd at the half way point
                    sd = sdRight + 0.0000001F
                End If
                x = x0 + dx * (i - 1)
                Me.m_points(i) = CSng(Math.Exp(-0.5 * (x / sd) ^ 2)) * max
                ' System.Console.WriteLine(x.ToString + ", " + Me.m_points(i).ToString)
            Next

        End If

        Return Me.m_points

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub Defaults()
        Me.ParamValue(1) = 1  'sd left
        Me.ParamValue(2) = 10 'width
        Me.ParamValue(3) = 1 'right
        Me.ParamValue(4) = 0 'mean
        Me.ParamValue(5) = 1
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
            Return 5
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cShapeFunction.ShapeFunctionType"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property ShapeFunctionType As Long
        Get
            Return eShapeFunctionType.Normal
        End Get
    End Property

    Public Overrides Sub Init(obj As Object)
        MyBase.Init(obj)

        If (Me.ShapeFunctionType <> eShapeFunctionType.Normal) Then Return

        ' Max cannot be stored yet. For now, deduct max from shape
        Dim max As Single = Single.MinValue
        For Each val As Single In Me.m_points
            max = Math.Max(val, max)
        Next
        If (max <= 0) Then max = 1
        Me.ParamValue(5) = max

    End Sub


    Public Overrides Function Apply(obj As Object) As Boolean
        If Not MyBase.Apply(obj) Then
            Return False
        End If
        Try
            If (TypeOf obj Is cEnviroResponseFunction) Then
                Dim shp As cEnviroResponseFunction = DirectCast(obj, cEnviroResponseFunction)
                Debug.Assert(shp.ShapeFunctionType = eShapeFunctionType.Normal)
                'ResponseLeftLimit  = mean - [half the data width]
                shp.ResponseLeftLimit = Me.Mean - Me.DataWidth / 2
                'ResponseRightLimit  = mean + [half the data width]
                shp.ResponseRightLimit = Me.Mean + Me.DataWidth / 2
            End If
        Catch ex As Exception

        End Try

        Return True

    End Function

    Public Property SDLeft As Single
        Get
            Return Me.ParamValue(eParNames.SDLeft)
        End Get
        Set(value As Single)
            Me.ParamValue(eParNames.SDLeft) = value
        End Set
    End Property

    Public Property SDRight As Single
        Get
            Return Me.ParamValue(eParNames.SDRight)
        End Get
        Set(value As Single)
            Me.ParamValue(eParNames.SDRight) = value
        End Set
    End Property

    Public Property DataWidth As Single
        Get
            Return Me.ParamValue(eParNames.DataWidth)
        End Get
        Set(value As Single)
            Me.ParamValue(eParNames.DataWidth) = value
        End Set
    End Property

    Public Property Mean As Single
        Get
            Return Me.ParamValue(eParNames.Mean)
        End Get
        Set(value As Single)
            Me.ParamValue(eParNames.Mean) = value
        End Set
    End Property

    Public Property NormalMax As Single
        Get
            Return Me.ParamValue(eParNames.Max)
        End Get
        Set(value As Single)
            Me.ParamValue(eParNames.Max) = value
        End Set
    End Property


    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.ParamValue"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Property ParamValue(iParam As Integer) As Single

        Get
            Debug.Assert((iParam >= 1) And (iParam <= Me.nParameters))
            Return Me.m_parameters(iParam - 1)
        End Get
        Set(value As Single)
            Debug.Assert((iParam >= 1) And (iParam <= Me.nParameters))
            If (Me.m_parameters(iParam - 1) <> value) Then
                Me.m_parameters(iParam - 1) = value
                Me.ParamsChanged = True
            End If
        End Set
    End Property

End Class
