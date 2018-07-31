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

Imports EwEUtils.SpatialData
Imports System.Drawing

#End Region ' Imports

Namespace Core

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Interface for directly importing and exporting spatial data into Ecospace
    ''' without the intervention of fancy spatial engines.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface IEcospaceImportExport

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set a grid value.
        ''' </summary>
        ''' <param name="iRow">One-based row index to access a value for.</param>
        ''' <param name="iCol">One-based column index to access a value for.</param>
        ''' <param name="strField">Optional field to access a value for.</param>
        ''' -------------------------------------------------------------------
        Property Value(ByVal iRow As Integer, ByVal iCol As Integer, Optional ByVal strField As String = "") As Object

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns data in the form of a <see cref="ISpatialRaster"/>.
        ''' </summary>
        ''' <param name="strField">Optional field name for filtering data if 
        ''' imported data is multi-dimensional.</param>
        ''' <returns>A raster.</returns>
        ''' -------------------------------------------------------------------
        Function ToRaster(Optional ByVal strField As String = "") As ISpatialRaster

        ReadOnly Property CellSize As Double
        ReadOnly Property InCol As Integer
        ReadOnly Property InRow As Integer
        ReadOnly Property NoDataValue As Double
        ReadOnly Property PosTopLeft As PointF
        ReadOnly Property ProjectionString As String

    End Interface

End Namespace
