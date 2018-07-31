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

''' ---------------------------------------------------------------------------
''' <summary>
''' Base class for implementing EwE core shape functions.
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class cShapeFunction
    Implements IShapeFunction

#Region " Private vars "

    ''' <summary>This original value is extracted from EwE5.</summary>
    Protected Const xBase As Single = 0.3

    ''' <summary>The parameters that define the shape</summary>
    Protected m_parameters As Single() = Nothing
    ''' <summary>The points of the shape</summary>
    Protected m_points As Single() = Nothing

#End Region ' Private vars

    Public Sub New()

        ' Redim with defaults
        ReDim Me.m_points(1200)
        ReDim Me.m_parameters(Me.nParameters)

        Defaults()

    End Sub

    Public Overridable Sub Init(obj As Object) _
        Implements IShapeFunction.Init

        If (Not TypeOf obj Is cForcingFunction) Then Return

        Dim shp As cForcingFunction = DirectCast(obj, cForcingFunction)
        Me.m_points = shp.ShapeData

        If (shp.ShapeFunctionType <> Me.ShapeFunctionType) Then Return

        For i As Integer = 1 To Me.nParameters
            Me.ParamValue(i) = shp.ShapeFunctionParameter(i)
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get one of the pre-defined shape function types for this shape.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>
    ''' <para>This will have to change once shape functions are delivered by 
    ''' plug-ins. Then, a class name will have to be used instead of an
    ''' enum to locate the function that was used.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property ShapeFunctionType As Long _
        Implements IShapeFunction.ShapeFunctionType

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.Defaults"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride Sub Defaults() _
        Implements IShapeFunction.Defaults

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.IsCompatible"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride Function IsCompatible(datatype As eDataTypes) As Boolean _
        Implements IShapeFunction.IsCompatible

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.IsDistribution()"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property IsDistribution As Boolean _
        Implements IShapeFunction.IsDistribution

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.nParameters"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property nParameters As Integer _
        Implements IShapeFunction.nParameters

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the array of function parameters.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property Parameters As Single()
        Get
            Return Me.m_parameters
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the name of a parameter. By default, one of the four standard
    ''' shape paramter names is returned (e.g., YZero, YEnd, YBase and Steepness)
    ''' </summary>
    ''' <param name="iParam">The one-based parameter index [1, <see cref="nParameters"/>]
    ''' to obtain the name for.</param>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property ParamName(iParam As Integer) As String _
        Implements IShapeFunction.ParamName
        Get
            Debug.Assert((iParam >= 1) And (iParam <= Me.nParameters))
            Select Case iParam
                Case 1 : Return My.Resources.CoreDefaults.PARAM_YZERO
                Case 2 : Return My.Resources.CoreDefaults.PARAM_YBASE
                Case 3 : Return My.Resources.CoreDefaults.PARAM_YEND
                Case 4 : Return My.Resources.CoreDefaults.PARAM_STEEPNESS
            End Select
            Return "?"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.IsCompatible"/>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property ParamUnit(iParam As Integer) As String _
         Implements IShapeFunction.ParamUnit
        Get
            Return ""
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.ParamStatus"/>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property ParamStatus(iParam As Integer) As eStatusFlags _
         Implements IShapeFunction.ParamStatus
        Get
            Return eStatusFlags.OK
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Flag, indicating that parameter values have changed and that the shape 
    ''' will be recalculated next time the <see cref="Shape"/> is requested.
    ''' </summary>
    ''' <returns>True if parameter values have recently changed.</returns>
    ''' -----------------------------------------------------------------------
    Protected Property ParamsChanged As Boolean = True

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.ParamValue"/>
    ''' -----------------------------------------------------------------------
    Public Overridable Property ParamValue(iParam As Integer) As Single _
        Implements IShapeFunction.ParamValue
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

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.ParamValue"/>
    ''' <summary>
    ''' By default, the shape is filled to the end with the value at <paramref name="nPoints"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Shape(ByVal nPoints As Integer) As Single() _
        Implements IShapeFunction.Shape

        If (Me.ParamsChanged) Then
            ' Iterpolate to the end
            For i As Integer = nPoints To Me.m_points.Length - 1
                Me.m_points(i) = Me.m_points(nPoints)
            Next
            Me.ParamsChanged = False
        End If
        Return Me.m_points

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IShapeFunction.Apply"/>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Apply(obj As Object) As Boolean _
        Implements IShapeFunction.Apply

        If (Not TypeOf (obj) Is cForcingFunction) Then Return False
        Dim shp As cForcingFunction = DirectCast(obj, cForcingFunction)
        Debug.Assert(Me.IsCompatible(shp.DataType))

        ' Update shape to this function
        shp.Reshape(Me)
        Return True

    End Function

    Public Overridable Sub ScaleData(nPoints As Integer, scaler As Single)
        Dim dataMax As Single = Me.Max
        If dataMax = 0 Then dataMax = 1
        For ipt As Integer = 1 To nPoints
            Me.m_points(ipt) = (Me.m_points(ipt) / dataMax) * scaler
        Next
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the max value in the shape buffer.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Protected Function Max() As Single
        Dim sMax As Single = 0
        For Each s As Single In Me.m_points
            sMax = Math.Max(s, sMax)
        Next
        Return sMax
    End Function

    Protected Function IsMediation(datatype As eDataTypes) As Boolean
        Return (datatype = eDataTypes.Mediation) Or _
               (datatype = eDataTypes.PriceMediation) Or _
               (datatype = eDataTypes.CapacityMediation)
    End Function

    Protected Function IsForcing(datatype As eDataTypes) As Boolean
        Return (datatype = eDataTypes.Forcing) Or _
               (datatype = eDataTypes.EggProd) Or _
               (datatype = eDataTypes.FishingEffort) Or _
               (datatype = eDataTypes.FishMort)
    End Function

End Class
