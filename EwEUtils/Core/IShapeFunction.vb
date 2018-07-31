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

Namespace Core

    Public Interface IShapeFunction

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialize to a given shape.
        ''' </summary>
        ''' <param name="shape">The shape to init to.</param>
        ''' -----------------------------------------------------------------------
        Sub Init(shape As Object)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set a shape function parameters to their default values.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Sub Defaults()

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of parameters needed to configure a shape function.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        ReadOnly Property nParameters() As Integer

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the human legible name of a parameter of a shape function.
        ''' </summary>
        ''' <param name="iParam">The index of the parameters [1,<see cref="nParameters"/>]
        ''' to obtain the human legible name for.</param>
        ''' -----------------------------------------------------------------------
        ReadOnly Property ParamName(ByVal iParam As Integer) As String

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value of a parameter of the shape function.
        ''' </summary>
        ''' <param name="iParam">The index of the parameters [1,<see cref="nParameters"/>]
        ''' to access the value for.</param>
        ''' -----------------------------------------------------------------------
        Property ParamValue(ByVal iParam As Integer) As Single

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the human legible unit for a parameter of a shape function.
        ''' </summary>
        ''' <param name="iParam">The index of the parameters [1,<see cref="nParameters"/>]
        ''' to obtain the human legible unit for.</param>
        ''' -----------------------------------------------------------------------
        ReadOnly Property ParamUnit(ByVal iParam As Integer) As String

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the order a parameter needs to appear in the UI. Any UI should honour
        ''' this flag sorting parameters from low to high order.
        ''' </summary>
        ''' <param name="iParam">The index of the parameters [1,<see cref="nParameters"/>]
        ''' to obtain order for.</param>
        ''' -----------------------------------------------------------------------
        ReadOnly Property ParamStatus(ByVal iParam As Integer) As eStatusFlags

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the actual function data points, as computed from the <see cref="ParamValue">parameters values</see>.
        ''' </summary>
        ''' <param name="nPoints">The number of points to calculate the shape for.</param>
        ''' <returns>An array of points.</returns>
        ''' -----------------------------------------------------------------------
        Function Shape(ByVal nPoints As Integer) As Single()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return whether a shape function is relevant for a given <see cref="EwEUtils.Core.eDataTypes">data type</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function IsCompatible(ByVal datatype As EwEUtils.Core.eDataTypes) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update a shape from the shape function.
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function Apply(ByVal shape As Object) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a (hopefully) unique identifier for a particular shape function,
        ''' regardless if this function is built-in to EwE or is provided by a plug-in.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property ShapeFunctionType() As Long

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the shape function is a true distribution, with fixed
        ''' min and max values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property IsDistribution() As Boolean

    End Interface

End Namespace

