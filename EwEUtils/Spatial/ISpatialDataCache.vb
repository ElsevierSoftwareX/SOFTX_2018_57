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

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Interface for classes that cache converted spatial-temporal data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface ISpatialDataCache

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the path to the cache root folder.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property RootFolder As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the path to a cache for a dataset.
        ''' </summary>
        ''' <param name="ds"><see cref="ISpatialDataSet"/> to obtain the cache path for.</param>
        ''' <param name="ptfTL">Top-left location (in decimal degrees lon,lat) of the bounding box of the data.</param>
        ''' <param name="ptfBR">Bottom-right location (in decimal degrees lon,lat) of the bounding box of the data.</param>
        ''' <param name="dCellSize">Cell size to obtain the cache path for.</param>
        ''' <param name="time">Time to create the file name for.</param>
        ''' <param name="strFilter">Optional filter, may be empty.</param>
        ''' <param name="strExt">File extension to create the file name for.</param>
        ''' <returns>A cache path.</returns>
        ''' -------------------------------------------------------------------
        Function GetFileName(ds As ISpatialDataSet, _
                             ptfTL As PointF, ptfBR As PointF, dCellSize As Double, time As DateTime, _
                             strFilter As String, strExt As String) As String
    End Interface

End Namespace

