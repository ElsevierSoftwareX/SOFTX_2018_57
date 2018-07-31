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

Imports EwEUtils.Core

Public Class cTaxonDataStructures

    Private m_ecopathDS As cEcopathDataStructures = Nothing
    Private m_stanzaDS As cStanzaDatastructures = Nothing

    ''' <summary>Total number of taxonomy codes.</summary>
    Public NumTaxon As Integer = 0
    ''' <summary>Taxonomy code DBID (xNumTaxa).</summary>
    Public TaxonDBID() As Integer
    ''' <summary>Taxon assignments (xNumTaxa) -> <see cref="IsTaxonStanza">iGroup / iStanza</see></summary>
    ''' <remarks>This number should be interpreted as a group index if
    ''' <see cref="IsTaxonStanza">IsTaxonStanza(i)</see> is set to False,
    ''' or denotes a stanza index otherwise.</remarks>
    Public TaxonTarget() As Integer
    ''' <summary>Flag stating whether TaxonTarget(i) refers to a stanza (true) or a group (false)</summary>
    Public IsTaxonStanza() As Boolean
    ''' <summary>Taxon proportion of biomass (xNumTaxa)</summary>
    Public TaxonPropBiomass() As Single
    ''' <summary>Taxa proportion of catch (xNumTaxa)</summary>
    Public TaxonPropCatch() As Single
    ''' <summary>Taxonomy class names (xNumTaxa).</summary>
    Public TaxonClass() As String
    ''' <summary>Taxonomy order names (xNumTaxa).</summary>
    Public TaxonOrder() As String
    ''' <summary>Taxonomy family names (xNumTaxa).</summary>
    Public TaxonFamily() As String
    ''' <summary>Taxonomy genus names (xNumTaxa).</summary>
    Public TaxonGenus() As String
    ''' <summary>Taxonomy species names (xNumTaxa).</summary>
    Public TaxonSpecies() As String
    ''' <summary>Taxonomy (common) names (xNumTaxa).</summary>
    Public TaxonName() As String
    ''' <summary>Taxonomy FAO codes (xNumTaxa).</summary>
    Public TaxonCodeFAO() As String
    ''' <summary>Taxonomy TDWG LSID codes (xNumTaxa).</summary>
    Public TaxonCodeLSID() As String
    ''' <summary>Taxonomy SAUP codes (xNumTaxa).</summary>
    Public TaxonCodeSAUP() As Long
    ''' <summary>Taxonomy FishBase codes (xNumTaxa).</summary>
    Public TaxonCodeFB() As Long
    ''' <summary>Taxonomy SeaLifeBase codes (xNumTaxa).</summary>
    Public TaxonCodeSLB() As Long
    ''' <summary>Taxonomy source names where Taxon information was derived from (xNumTaxa).</summary>
    Public TaxonSource() As String
    ''' <summary>Taxonomy source keys to access Taxon information in <see cref="TaxonSource">a source</see>(xNumTaxa).</summary>
    Public TaxonSourceKey() As String
    ''' <summary>Taxonomy last updated dates (xNumTaxa) in julian day format.</summary>
    Public TaxonLastUpdated() As Double
    ''' <summary>Northern limit of taxon occurrence bounding box</summary>
    Public TaxonNorth() As Single
    ''' <summary>Southern limit of taxon occurrence bounding box</summary>
    Public TaxonSouth() As Single
    ''' <summary>Eastern limit of taxon occurrence bounding box</summary>
    Public TaxonEast() As Single
    ''' <summary>Western limit of taxon occurrence bounding box</summary>
    Public TaxonWest() As Single
    ''' <summary>Ecology types for taxa</summary>
    Public TaxonEcologyType() As eEcologyTypes
    ''' <summary>Organism types for taxa</summary>
    Public TaxonOrganismType() As eOrganismTypes
    ''' <summary>Taxa IUCN csonservation status</summary>
    Public TaxonIUCNConservationStatus() As eIUCNConservationStatusTypes
    ''' <summary>Taxa exploitaiton status</summary>
    Public TaxonExploitationStatus() As eExploitationTypes
    ''' <summary>Taxa occurrence status</summary>
    Public TaxonOccurrenceStatus() As eOccurrenceStatusTypes
    Public TaxonMeanWeight() As Single
    Public TaxonMeanLength() As Single
    Public TaxonMaxLength() As Single
    Public TaxonMeanLifeSpan() As Single
    Public TaxonVulnerabilityIndex() As Integer
    Public TaxonWinf() As Single
    Public TaxonK() As Single


    ''' <summary>Group taxon index - may be used by model, initially designed for quick taxon access code.</summary>
    Private m_alGroupTaxa() As List(Of Integer)

    ''' <summary>
    ''' Create a new instance of cTaxonDataStructures
    ''' </summary>
    ''' <param name="ecopathDS"><see cref="cEcopathDataStructures"/> to use for base data.</param>
    ''' <param name="stanzaDS"><see cref="cStanzaDatastructures"/> to use for base data.</param>
    Public Sub New(ByVal ecopathDS As cEcopathDataStructures, ByVal stanzaDS As cStanzaDatastructures)
        Me.m_ecopathDS = ecopathDS
        Me.m_stanzaDS = stanzaDS
    End Sub

    ''' <summary>
    ''' 'Forget' internal data.
    ''' </summary>
    Public Sub Clear()
        Me.NumTaxon = 0
    End Sub

    ''' <summary>
    ''' Allocate memory for data.
    ''' </summary>
    Public Sub RedimTaxon()

        ReDim Me.TaxonDBID(Me.NumTaxon)
        ReDim Me.TaxonTarget(Me.NumTaxon)
        ReDim Me.IsTaxonStanza(Me.NumTaxon)
        ReDim Me.TaxonPropBiomass(Me.NumTaxon)
        ReDim Me.TaxonPropCatch(Me.NumTaxon)
        ReDim Me.TaxonClass(Me.NumTaxon)
        ReDim Me.TaxonCodeSAUP(Me.NumTaxon)
        ReDim Me.TaxonCodeFB(Me.NumTaxon)
        ReDim Me.TaxonCodeSLB(Me.NumTaxon)
        ReDim Me.TaxonCodeFAO(Me.NumTaxon)
        ReDim Me.TaxonCodeLSID(Me.NumTaxon)
        ReDim Me.TaxonName(Me.NumTaxon)
        ReDim Me.TaxonFamily(Me.NumTaxon)
        ReDim Me.TaxonGenus(Me.NumTaxon)
        ReDim Me.TaxonOrder(Me.NumTaxon)
        ReDim Me.TaxonSourceKey(Me.NumTaxon)
        ReDim Me.TaxonSource(Me.NumTaxon)
        ReDim Me.TaxonSpecies(Me.NumTaxon)
        ReDim Me.TaxonNorth(Me.NumTaxon)
        ReDim Me.TaxonSouth(Me.NumTaxon)
        ReDim Me.TaxonEast(Me.NumTaxon)
        ReDim Me.TaxonWest(Me.NumTaxon)
        ReDim Me.TaxonEcologyType(Me.NumTaxon)
        ReDim Me.TaxonOrganismType(Me.NumTaxon)
        ReDim Me.TaxonIUCNConservationStatus(Me.NumTaxon)
        ReDim Me.TaxonExploitationStatus(Me.NumTaxon)
        ReDim Me.TaxonOccurrenceStatus(Me.NumTaxon)
        ReDim Me.TaxonMeanWeight(Me.NumTaxon)
        ReDim Me.TaxonMeanLength(Me.NumTaxon)
        ReDim Me.TaxonMaxLength(Me.NumTaxon)
        ReDim Me.TaxonMeanLifeSpan(Me.NumTaxon)
        ReDim Me.TaxonVulnerabilityIndex(Me.NumTaxon)
        ReDim Me.TaxonLastUpdated(Me.NumTaxon)
        ReDim Me.TaxonWinf(Me.NumTaxon)
        ReDim Me.TaxonK(Me.NumTaxon)

        Me.m_alGroupTaxa = Nothing

    End Sub

    Friend Sub copyTo(ByRef d As cTaxonDataStructures)

        d.NumTaxon = Me.NumTaxon
        d.RedimTaxon()

        TaxonDBID.CopyTo(d.TaxonDBID, 0)
        TaxonTarget.CopyTo(d.TaxonTarget, 0)
        IsTaxonStanza.CopyTo(d.IsTaxonStanza, 0)
        TaxonPropBiomass.CopyTo(d.TaxonPropBiomass, 0)
        TaxonPropCatch.CopyTo(d.TaxonPropCatch, 0)
        TaxonClass.CopyTo(d.TaxonClass, 0)
        TaxonCodeSAUP.CopyTo(d.TaxonCodeSAUP, 0)
        TaxonCodeFB.CopyTo(d.TaxonCodeFB, 0)
        TaxonCodeSLB.CopyTo(d.TaxonCodeSLB, 0)
        TaxonCodeFAO.CopyTo(d.TaxonCodeFAO, 0)
        TaxonCodeLSID.CopyTo(d.TaxonCodeLSID, 0)
        TaxonName.CopyTo(d.TaxonName, 0)
        TaxonGenus.CopyTo(d.TaxonGenus, 0)
        TaxonFamily.CopyTo(d.TaxonFamily, 0)
        TaxonGenus.CopyTo(d.TaxonGenus, 0)
        TaxonOrder.CopyTo(d.TaxonOrder, 0)
        TaxonSourceKey.CopyTo(d.TaxonSourceKey, 0)
        TaxonSource.CopyTo(d.TaxonSource, 0)
        TaxonSpecies.CopyTo(d.TaxonSpecies, 0)
        TaxonNorth.CopyTo(d.TaxonNorth, 0)
        TaxonSouth.CopyTo(d.TaxonSouth, 0)
        TaxonEast.CopyTo(d.TaxonEast, 0)
        TaxonWest.CopyTo(d.TaxonWest, 0)
        TaxonEcologyType.CopyTo(d.TaxonEcologyType, 0)
        TaxonOrganismType.CopyTo(d.TaxonOrganismType, 0)
        TaxonIUCNConservationStatus.CopyTo(d.TaxonIUCNConservationStatus, 0)
        TaxonExploitationStatus.CopyTo(d.TaxonExploitationStatus, 0)
        TaxonOccurrenceStatus.CopyTo(d.TaxonOccurrenceStatus, 0)
        TaxonMeanWeight.CopyTo(d.TaxonMeanWeight, 0)
        TaxonMeanLength.CopyTo(d.TaxonMeanLength, 0)
        TaxonMaxLength.CopyTo(d.TaxonMaxLength, 0)
        TaxonMeanLifeSpan.CopyTo(d.TaxonMeanLifeSpan, 0)
        TaxonVulnerabilityIndex.CopyTo(d.TaxonVulnerabilityIndex, 0)
        TaxonLastUpdated.CopyTo(d.TaxonLastUpdated, 0)
        TaxonWinf.CopyTo(d.TaxonWinf, 0)
        TaxonK.CopyTo(d.TaxonK, 0)

    End Sub

#Region " Taxon index "

    ''' <summary>
    ''' Get the number of taxa assigned to a given group.
    ''' </summary>
    ''' <param name="iGroup">The one-based group index to get the number of taxa for.</param>
    ''' <remarks>Taxa can be <see cref="TaxonTarget">assigned</see> directly to a group
    ''' or indirectly via a multi-stanza configuration, determined by the state of the 
    ''' <see cref="IsTaxonStanza"/> field. Regardless, this method returns the number of
    ''' taxa assigned to a group - directly or indirectly.</remarks>
    Public ReadOnly Property NumGroupTaxa(ByVal iGroup As Integer) As Integer
        Get
            If Me.m_alGroupTaxa Is Nothing Then Me.UpdateTaxonIndex()
            Try
                Return Me.m_alGroupTaxa(iGroup).Count
            Catch ex As Exception
                Debug.Assert(False)
            End Try
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' Get the one-based index of a taxon for a given group.
    ''' </summary>
    ''' <param name="iGroup">The one-based group index to get the taxon information for.</param>
    ''' <param name="iIndex">The one-based index [1, <see cref="NumGroupTaxa"/>] of the taxon to obtain.</param>
    ''' <remarks>Taxa can be <see cref="TaxonTarget">assigned</see> directly to a group
    ''' or indirectly via a multi-stanza configuration, determined by the state of the 
    ''' <see cref="IsTaxonStanza"/> field. Regardless, this method returns the number of
    ''' taxa assigned to a group - directly or indirectly.</remarks>
    Public ReadOnly Property GroupTaxa(ByVal iGroup As Integer, ByVal iIndex As Integer) As Integer
        Get
            If Me.m_alGroupTaxa Is Nothing Then Me.UpdateTaxonIndex()
            Try
                Return Me.m_alGroupTaxa(iGroup)(iIndex - 1)
            Catch ex As Exception
                Debug.Assert(False)
            End Try
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' Rebuild the taxon / group index.
    ''' </summary>
    Private Sub UpdateTaxonIndex()

        ReDim Me.m_alGroupTaxa(Me.m_ecopathDS.NumGroups)
        For iGroup As Integer = 0 To Me.m_ecopathDS.NumGroups
            Me.m_alGroupTaxa(iGroup) = New List(Of Integer)
        Next

        For iTaxon As Integer = 1 To Me.NumTaxon
            If Me.IsTaxonStanza(iTaxon) Then
                Dim iStanza As Integer = Me.TaxonTarget(iTaxon)
                For iIndex As Integer = 1 To Me.m_stanzaDS.Nstanza(iStanza)
                    Dim iGroup As Integer = Me.m_stanzaDS.EcopathCode(iStanza, iIndex)
                    Me.m_alGroupTaxa(iGroup).Add(iTaxon)
                Next
            Else
                Dim iGroup As Integer = Me.TaxonTarget(iTaxon)
                Me.m_alGroupTaxa(iGroup).Add(iTaxon)
            End If
        Next

    End Sub

#End Region ' Taxon index

End Class
