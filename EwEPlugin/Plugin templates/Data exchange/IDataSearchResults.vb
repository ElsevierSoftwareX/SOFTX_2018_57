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

#End Region ' Imports

Namespace Data

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Interface for building a container for plug-in search results provided
    ''' by <see cref="IDataSearchProducerPlugin">data search plug-ins.</see>
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Interface IDataSearchResults
        Inherits IPluginData

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the term that was used to obtain these results.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property SearchTerm() As Object

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get an array of search results that matched the term.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property SearchResults() As Object()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get an array of score results for the matches.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property SearchScores() As Single()

    End Interface

End Namespace
