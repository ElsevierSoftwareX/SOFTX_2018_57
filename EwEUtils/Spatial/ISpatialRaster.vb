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
Imports System
Imports System.Drawing

#End Region ' Imports

Namespace SpatialData

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Wrapper to present raster data to Ecospace.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Interface ISpatialRaster
        Inherits IDisposable

#Region " Data access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the number of rows in the raster.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function NumRows() As Integer

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the number of columns in the raster.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function NumCols() As Integer

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the size of square cells in the raster.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function CellSize() As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the top-left location of the data in the raster.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function TopLeft() As PointF

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a cell value for a given Ecospace row and column.
        ''' </summary>
        ''' <param name="iRow">One-based Ecospace row index</param>
        ''' <param name="iCol">One-based Ecospace column index</param>
        ''' <param name="dNoDataValue">No data value to use if either row or 
        ''' column are invalid, or if the cell does not hold any data.</param>
        ''' <returns>A value, or <paramref name="dNoDataValue"/> if either row or 
        ''' column are invalid, or if the cell does not hold any data.</returns>
        ''' -------------------------------------------------------------------
        Function Cell(ByVal iRow As Integer, ByVal iCol As Integer, _
                      Optional ByVal dNoDataValue As Double = -9999) As Double

#End Region ' Data access

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the raster is connected to actual data.
        ''' </summary>
        ''' <returns>True if the raster is connected to actual data.</returns>
        ''' -------------------------------------------------------------------
        Function IsValid() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the mean value across all data values the raster.
        ''' </summary>
        ''' <returns>The mean value across all data values the raster. This
        ''' excludes cells with <see cref="NoData">no data</see>.</returns>
        ''' <remarks>A return value of -9999 is expected to signal that an error 
        ''' occurred and that no usable value is available.</remarks>
        ''' -------------------------------------------------------------------
        Function Mean() As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the min value across all data values in the raster.
        ''' </summary>
        ''' <returns>The min value across all data values in the raster.</returns>
        ''' <remarks>A return value of -9999 is expected to signal that an error 
        ''' occurred and that no usable value is available.</remarks>
        ''' -------------------------------------------------------------------
        Function Min() As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the max value across all data values in the raster.
        ''' </summary>
        ''' <returns>The max value across all data values in the raster.</returns>
        ''' <remarks>A return value of -9999 is expected to signal that an error 
        ''' occurred and that no usable value is available.</remarks>
        ''' -------------------------------------------------------------------
        Function Max() As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the standard deviation of values in the raster.
        ''' </summary>
        ''' <returns>The standard deviation in the raster.</returns>
        ''' <remarks>A return value of -9999 is expected to signal that an error 
        ''' occurred and that no usable value is available.</remarks>
        ''' -------------------------------------------------------------------
        Function StandardDeviation() As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the no data value in the raster.
        ''' </summary>
        ''' <returns>The no data value in the raster.</returns>
        ''' <remarks>A return value of -9999 is expected to signal that an error 
        ''' occurred and that no usable value is available.</remarks>
        ''' -------------------------------------------------------------------
        Function NoData() As Single

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the number of data cells (e.g. cells that do not have 
        ''' <see cref="NoData"/> values)
        ''' </summary>
        ''' <returns>The number of data cells.</returns>
        ''' <remarks>A return value of -9999 is expected to signal that an error 
        ''' occurred and that no usable value is available.</remarks>
        ''' -------------------------------------------------------------------
        Function NumValueCells() As Long

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the raster to file.
        ''' </summary>
        ''' <param name="strFile"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Function Save(strFile As String) As Boolean

#End Region 'Diagnostics

    End Interface

End Namespace
