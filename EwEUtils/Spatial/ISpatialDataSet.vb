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
Imports System.Data
Imports System.Drawing
Imports System.Xml
Imports EwEUtils.Core

#End Region ' Imports

Namespace SpatialData

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Interface for classes that provide access to sets of spatio-temporal data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface ISpatialDataSet
        Inherits IExternalDataSource
        Inherits ISummarizable

#Region " Information "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Legible name of the dataset.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property DisplayName As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Description of the data in the dataset.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property DataDescription As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the absolute source of the dataset.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property Source As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Unique <see cref="GUID"/>, assigned by the dataset manager, to uniquely identify the data set.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property GUID As Guid

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the start time of the data in this set.
        ''' </summary>
        ''' <remarks>
        ''' If no data is loaded or this property does not apply, this method is expected to return <see cref="DateTime.MaxValue"/>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        ReadOnly Property TimeStart() As DateTime

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the end time of the data in this set.
        ''' </summary>
        ''' <remarks>
        ''' If no data is loaded or this property does not apply, this method is expected to return <see cref="DateTime.MinValue"/>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        ReadOnly Property TimeEnd() As DateTime

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eVarNameFlags">core variable</see> associated 
        ''' with the dataset, if any.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property VarName As eVarNameFlags

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a descriptor for the data in the data set. This value is used
        ''' to find converters that are compatible with this dataformat.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property ConversionFormat As String

#End Region ' Information

#Region " Configuration "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the dataset is configured for delivering data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function IsConfigured() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the configuration information for the converter.
        ''' </summary>
        ''' <param name="doc"><see cref="XmlDocument"/> for creating and parsing nodes.</param>
        ''' <param name="strFolderRoot">The root folder where the configuration is read from.</param>
        ''' <remarks>Automatic serialization is not used here because of difficulties 
        ''' that may derive from serializing complex data structures and other 
        ''' headaches. It is deemed more cost-effective to allow full developer 
        ''' control over the persistence logic.</remarks>
        ''' -------------------------------------------------------------------
        Property Configuration(ByVal doc As XmlDocument, ByVal strFolderRoot As String) As XmlNode

#End Region ' Configuration

#Region " Data "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the data set has data for a given date.
        ''' </summary>
        ''' <param name="dateTime">The time to query data for. For practical
        ''' purposes, time is assumed to be rounded to days.</param>
        ''' <returns>True if data is available.</returns>
        ''' -------------------------------------------------------------------
        Function HasDataAtT(ByVal datetime As DateTime) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Lock the dataset for a given time and spatial extent.
        ''' </summary>
        ''' <param name="dateTime">The time to lock data for. For practical
        ''' purposes, time is assumed to be rounded to months.</param>
        ''' <param name="dCellSize">Map cell size that is requested.</param>
        ''' <param name="ptfNE">North-east corner of the area to load data for. 
        ''' Values are interpreted as decimal degrees, <see cref="Point.X"/> as longitude, 
        ''' <see cref="Point.Y"/> as latiude.</param>
        ''' <param name="ptfSW">South-west corner of the area to load data for. 
        ''' Values are interpreted as decimal degrees, <see cref="Point.X"/> as longitude, 
        ''' <see cref="Point.Y"/> as latiude.</param>
        ''' <param name="strProjectionString">WKT projection string for the target raster.</param>
        ''' <returns>True if data was successfully locked.</returns>
        ''' -------------------------------------------------------------------
        Function LockDataAtT(ByVal datetime As DateTime, _
                             ByVal dCellSize As Double, _
                             ByVal ptfNE As PointF, _
                             ByVal ptfSW As PointF, _
                             ByVal strProjectionString As String) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether data has been <see cref="LockDataAtT">locked</see>.
        ''' </summary>
        ''' <returns>True if data has been <see cref="LockDataAtT">locked</see>.</returns>
        ''' -------------------------------------------------------------------
        Function IsLocked() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Release any <see cref="LockDataAtT">locked</see> data.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function Unlock() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the indexed spatial extent of data at a given time. Extent(s) should
        ''' be gathered via <see cref="UpdateIndexAtT"/>
        ''' </summary>
        ''' <param name="dateTime">The time to query data for. For practical
        ''' purposes, time is assumed to be rounded to months.</param>
        ''' <param name="ptfNW">Point to receive top-left extent value.</param>
        ''' <param name="ptfSE">Point to receive bottom-right extent value.</param>
        ''' <returns>True if valid values were retrieved.</returns>
        ''' -------------------------------------------------------------------
        Function GetExtentAtT(ByVal datetime As DateTime, _
                              ByRef ptfNW As PointF, _
                              ByRef ptfSE As PointF) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Build the spatial extent index for the dataset for the Ecospace
        ''' run time.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Sub UpdateIndexAtT(ByVal datetime As DateTime)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Possible status flags for source data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Enum eIndexStatus As Integer
            ''' <summary>Data has not been indexed yet.</summary>
            NotIndexed
            ''' <summary>Data has been indexed.</summary>
            Indexed
            ''' <summary>Data could not be indexed.</summary>
            Failed
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether data at a given time is indexed.
        ''' </summary>
        ''' <param name="dateTime">The time to query data for. For practical
        ''' purposes, time is assumed to be rounded to months.</param>
        ''' <returns>True if data at a given time is indexed.</returns>
        ''' -------------------------------------------------------------------
        Function IndexStatusAtT(ByVal datetime As DateTime) As eIndexStatus

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the names of all attributes for <see cref="LockDataAtT">locked</see> data. 
        ''' </summary>
        ''' <returns>The names of all attributes for <see cref="LockDataAtT">locked</see> data.</returns>
        ''' -------------------------------------------------------------------
        Function GetAttributes() As String()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the data types of all attributes for <see cref="LockDataAtT">locked</see> data. 
        ''' </summary>
        ''' <returns>The data types of all attributes for <see cref="LockDataAtT">locked</see> data.</returns>
        ''' -------------------------------------------------------------------
        Function GetAttributeDataTypes() As Type()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the attribute <see cref="DataTable"/> for the <see cref="LockDataAtT">locked</see> data.
        ''' </summary>
        ''' <returns>The attribute <see cref="DataTable"/> for the <see cref="LockDataAtT">locked</see> data.</returns>
        ''' -------------------------------------------------------------------
        Function GetAttributeValues() As DataTable

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtains a <see cref="ISpatialRaster"/> from the <see cref="LockDataAtT">locked</see> data.
        ''' </summary>
        ''' <param name="converter">Spatial data converter to perform the magic.</param>
        ''' <param name="strLayerName">Name of the layer data will be retrieved for.</param>
        ''' <returns>A <see cref="ISpatialRaster">spatial raster</see>.</returns>
        ''' -------------------------------------------------------------------
        Function GetRaster(ByVal converter As ISpatialDataConverter, _
                           ByVal strLayerName As String) As ISpatialRaster

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the dialog read filter for all supported file types.
        ''' </summary>
        ''' <param name="bRaster">Allowed to include raster file types.</param>
        ''' <param name="bImage">Allowed to include image file types.</param>
        ''' <param name="bVector">Allowed to include vector file types.</param>
        ''' -------------------------------------------------------------------
        ReadOnly Property DialogReadFilter(ByVal bRaster As Boolean, ByVal bImage As Boolean, ByVal bVector As Boolean) As String

#End Region ' Data

#Region " Cache "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the cache to use for the dataset.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property Cache As ISpatialDataCache

#End Region ' Cache

#Region " Import / export "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Export the dataset for transfer to another computer.
        ''' </summary>
        ''' <param name="strPath"></param>
        ''' <returns>A new dataset with the same <see cref="ISpatialDataSet.GUID"/>
        ''' as the source dataset.</returns>
        ''' -------------------------------------------------------------------
        Function ExportTo(ByVal strPath As String) As ISpatialDataSet

#End Region ' Import / export

    End Interface

End Namespace

