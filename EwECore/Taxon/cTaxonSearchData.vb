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
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Container for transferring Taxonomy data
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTaxonSearchData
    Implements ITaxonSearchData
    Implements ITaxonDetailsData

#Region " Constructor "

    Public Sub New(strSource As String)
        Me.Source = strSource
    End Sub

#End Region ' Constructor

#Region " Properties "

    ''' <inheritdocs cref="ITaxonSearchData.Phylum"/>
    Public Property Phylum() As String Implements ITaxonSearchData.Phylum

    ''' <inheritdocs cref="ITaxonSearchData.[Class]"/>
    Public Property [Class]() As String Implements ITaxonSearchData.Class

    ''' <inheritdocs cref="ITaxonSearchData.Common"/>
    Public Property Common() As String Implements ITaxonSearchData.Common

    ''' <inheritdocs cref="ITaxonSearchData.Family"/>
    Public Property Family() As String Implements ITaxonSearchData.Family

    ''' <inheritdocs cref="ITaxonSearchData.Genus"/>
    Public Property Genus() As String Implements ITaxonSearchData.Genus

    ''' <inheritdocs cref="ITaxonSearchData.Order"/>
    Public Property Order() As String Implements ITaxonSearchData.Order

    ''' <inheritdocs cref="ITaxonSearchData.Species"/>
    Public Property Species() As String Implements ITaxonSearchData.Species

    ''' <inheritdocs cref="ITaxonSearchData.CodeSAUP"/>
    Public Property CodeSAUP() As Long Implements ITaxonSearchData.CodeSAUP

    ''' <inheritdocs cref="ITaxonSearchData.CodeFB"/>
    Public Property CodeFB As Long Implements ITaxonSearchData.CodeFB

    ''' <inheritdocs cref="ITaxonSearchData.CodeSLB"/>
    Public Property CodeSLB As Long Implements ITaxonSearchData.CodeSLB

    ''' <inheritdocs cref="ITaxonSearchData.CodeLSID"/>
    Public Property CodeLSID() As String Implements ITaxonSearchData.CodeLSID

    ''' <inheritdocs cref="ITaxonSearchData.CodeFAO"/>
    Public Property CodeFAO() As String Implements ITaxonSearchData.CodeFAO

    ''' <inheritdocs cref="ITaxonSearchData.Source"/>
    Public Property Source() As String Implements ITaxonSearchData.Source

    ''' <inheritdocs cref="ITaxonSearchData.SourceKey"/>
    Public Property SourceKey() As String Implements ITaxonSearchData.SourceKey

    Public Property SearchFields As eTaxonClassificationType Implements ITaxonSearchData.SearchFields

    ''' <inheritdocs cref="ITaxonSearchData.North"/>
    Public Property North() As Single = cCore.NULL_VALUE Implements ITaxonSearchData.North

    ''' <inheritdocs cref="ITaxonSearchData.South"/>
    Public Property South() As Single = cCore.NULL_VALUE Implements ITaxonSearchData.South
    
    ''' <inheritdocs cref="ITaxonSearchData.East"/>
    Public Property East() As Single = cCore.NULL_VALUE Implements ITaxonSearchData.East

    ''' <inheritdocs cref="ITaxonSearchData.West"/>
    Public Property West() As Single = cCore.NULL_VALUE Implements ITaxonSearchData.West

    ''' <inheritdocs cref="ITaxonDetailsData.EcologyType"/>
    Public Property EcologyType() As eEcologyTypes = eEcologyTypes.NotSet Implements ITaxonDetailsData.EcologyType

    ''' <inheritdocs cref="ITaxonDetailsData.IUCNConservationStatus"/>
    Public Property IUCNConservationStatus() As eIUCNConservationStatusTypes = eIUCNConservationStatusTypes.NotSet Implements ITaxonDetailsData.IUCNConservationStatus

    ''' <inheritdocs cref="ITaxonDetailsData.ExploitationStatus"/>
    Public Property ExploitationStatus() As eExploitationTypes = eExploitationTypes.NotSet Implements ITaxonDetailsData.ExploitationStatus

    ''' <inheritdocs cref="ITaxonDetailsData.LastUpdated"/>
    Public Property LastUpdated() As Double = cDateUtils.DateToJulian(Date.Now()) Implements ITaxonDetailsData.LastUpdated

    ''' <inheritdocs cref="ITaxonDetailsData.MaxLength"/>
    Public Property MaxLength() As Single = cCore.NULL_VALUE Implements ITaxonDetailsData.MaxLength

    ''' <inheritdocs cref="ITaxonDetailsData.MeanLength"/>
    Public Property MeanLength() As Single = cCore.NULL_VALUE Implements ITaxonDetailsData.MeanLength

    ''' <inheritdocs cref="ITaxonDetailsData.MeanLifespan"/>
    Public Property MeanLifespan() As Single = cCore.NULL_VALUE Implements ITaxonDetailsData.MeanLifespan

    ''' <inheritdocs cref="ITaxonDetailsData.MeanWeight"/>
    Public Property MeanWeight() As Single = cCore.NULL_VALUE Implements ITaxonDetailsData.MeanWeight

    ''' <inheritdocs cref="ITaxonDetailsData.OccurrenceStatus"/>
    Public Property OccurrenceStatus() As eOccurrenceStatusTypes = eOccurrenceStatusTypes.NotSet Implements ITaxonDetailsData.OccurrenceStatus

    ''' <inheritdocs cref="ITaxonDetailsData.OrganismType"/>
    Public Property OrganismType() As eOrganismTypes = eOrganismTypes.NotSet Implements ITaxonDetailsData.OrganismType

    ''' <inheritdocs cref="ITaxonDetailsData.VulnerabilityIndex"/>
    Public Property VulnerabilityIndex() As Integer = cCore.NULL_VALUE Implements ITaxonDetailsData.VulnerabilityIndex

    ''' <inheritdocs cref="ITaxonDetailsData.vbgfK"/>
    Public Property vbgfK As Single = cCore.NULL_VALUE Implements EwEUtils.Core.ITaxonDetailsData.vbgfK

    ''' <inheritdocs cref="ITaxonDetailsData.Winf"/>
    Public Property Winf As Single = cCore.NULL_VALUE Implements EwEUtils.Core.ITaxonDetailsData.Winf

#End Region ' Properties

End Class
