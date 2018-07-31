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
Imports EwEPlugin.Data
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Implementation of the SAUP Taxon search results class.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cWoRMSTaxonSearchResults
    Implements IDataSearchResults

#Region " Private bits "

    ''' <summary>Term used to start the search.</summary>
    Private m_term As ITaxonSearchData = Nothing
    ''' <summary>
    ''' Taxa returned in response to the search.
    ''' </summary>
    Private m_taxa As ITaxonSearchData() = Nothing
    ''' <summary>Mandatory bit: plug-in name that returned the search results.</summary>
    Private m_strPluginName As String = ""

#End Region ' Private bits

    Public Sub New(ByVal term As ITaxonSearchData, _
                   ByVal results As ITaxonSearchData(), _
                   ByVal strPluginName As String)
        Me.m_term = term
        Me.m_taxa = results
        Me.m_strPluginName = strPluginName
    End Sub

    ''' <inheritdoc cref="IDataSearchResults.SearchResults"/>
    Public ReadOnly Property SearchResults() As Object() _
        Implements EwEPlugin.Data.IDataSearchResults.SearchResults
        Get
            Return Me.m_taxa
        End Get
    End Property

    ''' <inheritdoc cref="IDataSearchResults.SearchScores"/>
    Public ReadOnly Property SearchScores() As Single() _
        Implements EwEPlugin.Data.IDataSearchResults.SearchScores
        Get
            Dim asScores(Me.m_taxa.Length) As Single
            Return asScores
        End Get
    End Property

    ''' <inheritdoc cref="IDataSearchResults.SearchTerm"/>
    Public ReadOnly Property SearchTerm() As Object _
        Implements EwEPlugin.Data.IDataSearchResults.SearchTerm
        Get
            Return Me.m_term
        End Get
    End Property

    ''' <inheritdoc cref="IDataSearchResults.PluginName"/>
    Public ReadOnly Property PluginName() As String _
        Implements EwEPlugin.Data.IPluginData.PluginName
        Get
            Return Me.m_strPluginName
        End Get
    End Property

    ''' <inheritdoc cref="IDataSearchResults.RunType"/>
    Public ReadOnly Property RunType() As EwEUtils.Core.IRunType _
        Implements EwEPlugin.Data.IPluginData.RunType
        Get
            Return Nothing
        End Get
    End Property

End Class
