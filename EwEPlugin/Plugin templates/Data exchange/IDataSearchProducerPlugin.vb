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
Imports EwEUtils.Core

#End Region ' Imports

Namespace Data

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in interface for data providers that allow data to be searched.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Interface IDataSearchProducerPlugin
        Inherits IDataProducerPlugin

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Starts an asynchronous search for data.
        ''' </summary>
        ''' <param name="data">The data providing search terms.</param>
        ''' <param name="iMaxResults">The max number of results to return.</param>
        ''' <returns>True if started successful.</returns>
        ''' -------------------------------------------------------------------
        Function StartSearch(ByVal data As Object, iMaxResults As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Interrupt a current search.
        ''' </summary>
        ''' <returns>True if stopped succesfully.</returns>
        ''' -------------------------------------------------------------------
        Function StopSearch() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Queries a data producer if a search is in progress.
        ''' </summary>
        ''' <returns>True if a search is in progress.</returns>
        ''' -------------------------------------------------------------------
        Function IsSeaching() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns search results.
        ''' </summary>
        ''' <param name="dataTerm">The search term that was used.</param>
        ''' <param name="results">Returned search results.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SearchResults(ByVal dataTerm As Object, ByRef results As IDataSearchResults) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a search term for an interface to substitute data into.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Function CreateSearchTerm() As Object

    End Interface

End Namespace
