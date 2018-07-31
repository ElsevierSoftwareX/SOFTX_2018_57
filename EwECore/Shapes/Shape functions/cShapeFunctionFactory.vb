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
Imports System.Reflection
Imports EwEPlugin
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Factory for delivering <see cref="IShapeFunction">shape functions</see> that
''' can be used to reshape a particular <see cref="cForcingFunction"/>. This class
''' also takes <see cref="EwEPlugin.IEcosimShapeFunctionPlugin">shape functions 
''' delivered by plug-ins</see> into account.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cShapeFunctionFactory

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a <see cref="IShapeFunction"/> for a given <paramref name="ft">Shape function type</paramref>
    ''' </summary>
    ''' <param name="ft">The function type</param>
    ''' <param name="pm"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function GetShapeFunction(ft As Long,
                                            Optional ByVal pm As cPluginManager = Nothing) As IShapeFunction

        For Each sf As IShapeFunction In GetShapeFunctions(pm)
            If (ft = sf.ShapeFunctionType) Then Return sf
        Next
        Return Nothing

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get an initialized <see cref="IShapeFunction"/> for a given <see cref="cForcingFunction"/>
    ''' </summary>
    ''' <param name="shape"></param>
    ''' <param name="pm"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function GetShapeFunction(ByVal shape As cForcingFunction,
                                            Optional ByVal pm As cPluginManager = Nothing) As IShapeFunction

        If (shape Is Nothing) Then Return Nothing

        For Each sf As IShapeFunction In GetShapeFunctions(pm)
            If (shape.ShapeFunctionType = sf.ShapeFunctionType) Then
                sf.Init(shape)
                Return sf
            End If
        Next
        Return Nothing

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all <see cref="IShapeFunction">shape functions</see>, optionally
    ''' filtered for compatibility with - and initialized to - a given 
    ''' <see cref="cForcingFunction"/>.
    ''' </summary>
    ''' <param name="shape">The optional <see cref="cForcingFunction"/> to find 
    ''' compatible <see cref="IShapeFunction">shape functions</see> for.</param>
    ''' <param name="pm">The <see cref="cPluginManager"/> to find plug-in shapes for.</param>
    ''' <returns>
    ''' An array of compatible <see cref="IShapeFunction">shape function</see> instances.
    ''' </returns> 
    ''' -----------------------------------------------------------------------
    Public Shared Function GetShapeFunctions(ByVal shape As cForcingFunction,
                                             Optional ByVal pm As cPluginManager = Nothing) As IShapeFunction()

        Dim lfs As New List(Of IShapeFunction)
        For Each fs As IShapeFunction In GetShapeFunctions(pm)
            Dim bCompatible As Boolean = False

            Try
                If (shape Is Nothing) Then
                    bCompatible = True
                Else
                    bCompatible = (fs.IsCompatible(shape.DataType))
                End If

                If (bCompatible) Then
                    If (shape IsNot Nothing) Then fs.Init(shape)
                    lfs.Add(fs)
                End If
            Catch ex As Exception

            End Try
        Next
        Return lfs.ToArray()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all <see cref="IShapeFunction">shape functions</see> compatible
    ''' with a given <see cref="eDataTypes">data type</see>.
    ''' </summary>
    ''' <param name="pm">The <see cref="cPluginManager"/> to find plug-in shapes for.</param>
    ''' <returns>
    ''' An array of compatible <see cref="IShapeFunction">shape function</see> instances.
    ''' </returns> 
    ''' -----------------------------------------------------------------------
    Public Shared Function GetShapeFunctions(Optional ByVal pm As cPluginManager = Nothing) As IShapeFunction()

        Dim lfs As New List(Of IShapeFunction)
        Dim fs As IShapeFunction = Nothing

        ' Get all shape functions provided by the core
        For Each c As Type In Assembly.GetAssembly(GetType(cCore)).GetTypes()
            If (c.IsPublic) And (Not c.IsAbstract) And (GetType(cShapeFunction).IsAssignableFrom(c)) Then
                Try
                    fs = CType(Activator.CreateInstance(c), IShapeFunction)
                    lfs.Add(fs)
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    cLog.Write(ex, "cShapeFunctionFactory.GetShapeFunctions(" & c.ToString & ")")
                End Try

            End If
        Next

        If (pm IsNot Nothing) Then
            ' Get all shape functions provided as plug-ins
            For Each c As IPlugin In pm.GetPlugins(GetType(IEcosimShapeFunctionPlugin))
                fs = CType(c, IShapeFunction)
                lfs.Add(fs)
            Next
        End If

        Return lfs.ToArray()
    End Function

End Class
