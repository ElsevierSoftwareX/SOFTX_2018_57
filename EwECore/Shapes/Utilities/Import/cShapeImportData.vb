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
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Shapes.Utility

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Data for <see cref="cShapeImporter">importing shapes</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cShapeImportData

#Region " Helper classes "

        Public Class cFunctionDefinition

#Region " Private vars "

            Private m_strName As String
            Private m_fn As IShapeFunction
            Private m_parms As Single()

#End Region ' Private vars

            Public Sub New(strName As String, fn As IShapeFunction, parms As Single())
                Me.m_strName = strName
                Me.m_fn = fn
                If (parms IsNot Nothing) Then
                    Me.m_parms = CType(parms.Clone(), Single())
                End If
            End Sub

            Public ReadOnly Property Name As String
                Get
                    Return Me.m_strName
                End Get
            End Property

            Public ReadOnly Property ShapeFunction As IShapeFunction
                Get
                    Return Me.m_fn
                End Get
            End Property

            Public ReadOnly Property ShapeParameters As Single()
                Get
                    Return Me.m_parms
                End Get
            End Property

        End Class

#End Region ' Helper classes

#Region " Private vars "

        Private m_core As cCore = Nothing
        Private m_defs As New List(Of cFunctionDefinition)
        Private m_fns As New Dictionary(Of Long, IShapeFunction)

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(core As cCore)
            Me.m_core = core

            Dim fns As IShapeFunction() = cShapeFunctionFactory.GetShapeFunctions(pm:=Me.m_core.PluginManager)
            For Each fn As IShapeFunction In fns
                'If (fn.ShapeFunctionType <> eShapeFunctionType.NotSet) Then
                Me.m_fns(fn.ShapeFunctionType) = fn
                'End If
            Next
        End Sub

#End Region ' Constructor

#Region " Public properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the CSV text separator character to use when importing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Delimiter As Char = ","c

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the decimal separator character character to use when importing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property DecimalSeparator As Char = "."c

#End Region ' Public properties

#Region " Public methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clear the shape import data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Clear()
            Me.m_defs.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Read the shape import text.
        ''' </summary>
        ''' <param name="text">The text to read.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' <para>The CSV separated text must follow a strict layout:</para>
        ''' <code>
        ''' ShapeName,ShapeType,Parameter1,Parameter2,Parameter3,Parameter4,Parameter5
        ''' {text},{number},{value},{value},{value},{value},{value} 
        ''' </code>
        ''' <para>The header line must be present but is not parsed (yet).</para>
        ''' <para>The shape type value must match one of the pre-defined <see cref="eShapeFunctionType"/>
        ''' values, or a numerical value that matches a <see cref="IShapeFunction.ShapeFunctionType"/>
        ''' value delivered plug-ins that offer extra shape functions.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function Read(text As System.IO.TextReader) As Boolean

            Me.Clear()

            Dim strLine As String = text.ReadLine()
            Dim bSucces As Boolean = True

            ' Skip header line, for now expect fixed order

            strLine = text.ReadLine()
            While Not String.IsNullOrWhiteSpace(strLine)
                Dim bits As String() = cStringUtils.SplitQualified(strLine, Delimiter)
                Dim fn As IShapeFunction = Me.ShapeFunction(Long.Parse(bits(1)))
                If (fn IsNot Nothing) Then
                    Dim parms(4) As Single
                    For i As Integer = 0 To 4
                        If bits.Length > i + 2 Then
                            parms(i) = cStringUtils.ConvertToSingle(bits(i + 2), 0, Me.DecimalSeparator)
                        Else
                            parms(i) = cCore.NULL_VALUE
                        End If
                    Next
                    Dim f As New cFunctionDefinition(bits(0), fn, parms)
                    Me.m_defs.Add(f)
                Else
                    bSucces = False
                End If
                strLine = text.ReadLine()
            End While

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns all the <see cref="cFunctionDefinition">function definitions</see>
        ''' read from the import text via <seealso cref="Read"/>, and that are
        ''' <see cref="IShapeFunction.IsCompatible">compatible</see> with the 
        ''' provided <paramref name="dt">data type</paramref>
        ''' </summary>
        ''' <returns>An array of <see cref="cFunctionDefinition">function definitions</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Function FunctionDefinitions(ByVal dt As eDataTypes) As cFunctionDefinition()
            Dim lDefs As New List(Of cFunctionDefinition)
            For Each fn As cFunctionDefinition In Me.m_defs
                If fn.ShapeFunction.IsCompatible(dt) Then
                    lDefs.Add(fn)
                End If
            Next
            Return lDefs.ToArray()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns all avaliable <see cref="IShapeFunction">shape functions</see>, provuded
        ''' by the EwE core and plug-ins.
        ''' </summary>
        ''' <returns>All avaliable <see cref="IShapeFunction">shape functions</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Function ShapeFunctions() As IEnumerable(Of IShapeFunction)
            Return From fn As IShapeFunction In Me.m_fns.Values Order By fn.ShapeFunctionType
        End Function

#End Region ' Public methods

#Region " Internals "

        Private Function ShapeFunction(ShapeFunctionType As Long) As IShapeFunction
            If (Me.m_fns.ContainsKey(ShapeFunctionType)) Then
                Return Me.m_fns(ShapeFunctionType)
            End If
            Return Nothing
        End Function

#End Region ' Internals

    End Class

End Namespace
