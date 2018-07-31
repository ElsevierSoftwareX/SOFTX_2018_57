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
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Xml
Imports EwEUtils.Core

#End Region ' Imports

Namespace SpatialData

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing spatial data conversions.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Interface ISpatialDataConverter
        Inherits ISummarizable

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the dataset to link to this converter.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property Dataset As ISpatialDataSet

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name for displaying the converter in a user interface.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property DisplayName As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the description for displaying the converter in a user interface.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property Description As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set an attribute filter, if needed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property AttributeFilter As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the name of the attribute to rasterize, if needed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property AttributeName As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Optional mappings for rasterizing features
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property AttributeValueMappings() As Dictionary(Of Object, Object)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert data with a given extent and cell size into a <see cref="ISpatialRaster">raster</see>.
        ''' </summary>
        ''' <param name="data">Data to convert.</param>
        ''' <param name="ptfNE">North-east corner of the area to load data for. 
        ''' Values are interpreted as decimal degrees, <see cref="Point.X"/> as longitude, 
        ''' <see cref="Point.Y"/> as latiude.</param>
        ''' <param name="ptfSW">South-west corner of the area to load data for. 
        ''' Values are interpreted as decimal degrees, <see cref="Point.X"/> as longitude, 
        ''' <see cref="Point.Y"/> as latiude.</param>
        ''' <param name="dCellSize">Cell size (in decimal degrees) to convert data to.</param>
        ''' <param name="strProjToWkt">The target WKT projection string.</param>
        ''' <param name="strFile">Name of the file to store the converted raster.</param>
        ''' <returns>A <see cref="ISpatialRaster">raster</see> with data, trimmed to the Ecospace 
        ''' bounding box indicated by <paramref name="ptfNE"/>, <paramref name="ptfSW"/> and 
        ''' <paramref name="dCellSize">cell size</paramref>.</returns>
        ''' -------------------------------------------------------------------
        Function Convert(ByVal data As Object, _
                         ByVal ptfNE As PointF, _
                         ByVal ptfSW As PointF, _
                         ByVal dCellSize As Double, _
                         ByVal strProjToWkt As String, _
                         ByVal strFile As String) As ISpatialRaster

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the configuration information for the converter.
        ''' </summary>
        ''' <param name="doc"><see cref="XmlDocument"/> for creating and parsing nodes.</param>
        ''' -------------------------------------------------------------------
        Property Configuration(ByVal doc As XmlDocument) As XmlNode

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the converter is configured and ready to operate.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function IsConfigured() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the converter is compatible with the data provided 
        ''' by a given dataset.
        ''' </summary>
        ''' <param name="ds">The dataset to the data.</param>
        ''' <returns>True if compatible.</returns>
        ''' -------------------------------------------------------------------
        Function IsCompatible(ByVal ds As ISpatialDataSet) As Boolean

    End Interface

End Namespace
