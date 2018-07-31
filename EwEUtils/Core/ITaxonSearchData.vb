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
Imports System

#End Region ' Imports

Namespace Core

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Template for reporting taxonomic search capabilities
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface ITaxonSearchCapabilities

        ''' <summary>
        ''' Returns a bitwise pattern of <see cref="eTaxonClassificationType"/> enumerated
        ''' values stating which taxonomic classification fields can be searched.
        ''' </summary>
        ''' <returns>A bitwise pattern of <see cref="eTaxonClassificationType"/> enumerated
        ''' values stating which taxonomic classification fields can be searched.</returns>
        Function TaxonSearchCapabilities() As eTaxonClassificationType

        ''' <summary>
        ''' Returns whether the taxonomic search engine can search by spatial bounding box.
        ''' </summary>
        ''' <returns>True if the taxonomic search engine can search by spatial bounding box</returns>
        Function HasSpatialSearchCapabilities() As Boolean

        ''' <summary>
        ''' Returns whether the taxonomic search engine can search by depth range.
        ''' </summary>
        ''' <returns>True if the taxonomic search engine can search by depth range.</returns>
        Function HasDepthRangeSearchCapabilities() As Boolean

    End Interface

End Namespace
