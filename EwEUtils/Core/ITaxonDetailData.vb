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

Namespace Core

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Template for exchanging Taxonomy details data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface ITaxonDetailsData
        Inherits ITaxonSearchData

        ''' <summary>
        ''' Get/set the <see cref="eEcologyTypes"/> for a taxon.
        ''' </summary>
        Property EcologyType() As eEcologyTypes
        ''' <summary>
        ''' Get/set the <see cref="eOrganismTypes"/> for a taxon.
        ''' </summary>
        Property OrganismType() As eOrganismTypes
        ''' <summary>
        ''' Get/set the <see cref="eIUCNConservationStatusTypes"/> for a taxon.
        ''' </summary>
        Property IUCNConservationStatus() As eIUCNConservationStatusTypes
        ''' <summary>
        ''' Get/set the <see cref="eExploitationTypes"/> for a taxon.
        ''' </summary>
        Property ExploitationStatus() As eExploitationTypes
        ''' <summary>
        ''' Get/set the <see cref="eOccurrenceStatusTypes"/> for a taxon.
        ''' </summary>
        Property OccurrenceStatus() As eOccurrenceStatusTypes
        ''' <summary>
        ''' Get/set the mean weight for a taxon.
        ''' </summary>
        Property MeanWeight() As Single
        ''' <summary>
        ''' Get/set the mean life span for a taxon.
        ''' </summary>
        Property MeanLifespan() As Single
        ''' <summary>
        ''' Get/set the mean length for a taxon.
        ''' </summary>
        Property MeanLength() As Single
        ''' <summary>
        ''' Get/set the max length for a taxon.
        ''' </summary>
        Property MaxLength() As Single
        ''' <summary>
        ''' Get/set the vulnerability index for a taxon.
        ''' </summary>
        Property VulnerabilityIndex() As Integer
        ''' <summary>
        ''' Get/set the asymptotic weight for a taxon.
        ''' </summary>
        Property Winf() As Single
        ''' <summary>
        ''' Get/set the asymptotic weight for a taxon.
        ''' </summary>
        Property vbgfK() As Single
        ''' <summary>Julian date when record was last updated.</summary>
        Property LastUpdated() As Double

    End Interface

End Namespace
