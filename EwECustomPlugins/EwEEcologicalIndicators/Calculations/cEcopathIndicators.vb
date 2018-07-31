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

Option Strict On ' To enforce dilligent programming
Imports EwECore

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Class that computes all Ecopath-based indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcopathIndicators
    Inherits cIndicators

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of this class.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">Core</see> to operate onto.</param>
    ''' <param name="ecopathDS">The <see cref="cEcopathDataStructures">Ecopath data structures</see> to operate onto.</param>
    ''' <param name="stanzaDS">The <see cref="cStanzaDatastructures">Stanza data structures</see> to operate onto.</param>
    ''' <param name="taxonDS">The <see cref="cTaxonDataStructures">Taxonomy data structures</see> to operate onto.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, _
                   ByVal ecopathDS As cEcopathDataStructures, _
                   ByVal stanzaDS As cStanzaDatastructures, _
                   ByVal taxonDS As cTaxonDataStructures, _
                   ByVal lookup As cTaxonAnalysis)
        MyBase.New(core, ecopathDS, stanzaDS, taxonDS, lookup)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelBiomass"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelBiomass(iGroup As Integer) As Single
        Return Me.EcopathDS.B(iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelCatch(iGroup As Integer) As Single
        Return Me.EcopathDS.fCatch(iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTL"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTL(iGroup As Integer) As Single
        Return Me.EcopathDS.TTLX(iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTLCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTLCatch() As Single
        Return Me.EcopathDS.TLcatch
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelDiscards"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelDiscards(iGroup As Integer) As Single
        Dim sDiscards As Single = 0
        For iFleet As Integer = 1 To Me.EcopathDS.NumFleet
            sDiscards += Me.EcopathDS.Discard(iFleet, iGroup)
        Next
        Return sDiscards
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelKemptionsQ"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelKemptionsQ() As Single
        Return Me.Core.EcoFunction.KemptonsQ(Me.EcopathDS.NumLiving, Me.EcopathDS.TTLX, Me.EcopathDS.B, 0.25)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelShannonDiversity"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelShannonDiversity() As Single
        Return Me.Core.EcoFunction.ShannonDiversityIndex(Me.EcopathDS.NumLiving, Me.EcopathDS.B)
    End Function

End Class
