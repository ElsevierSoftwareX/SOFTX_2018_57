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
''' Class that computes all Ecosim-based indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcosimIndicators
    Inherits cIndicators

#Region " Private variables "

    Private m_iTime As Integer = 0
    Private m_ecosimDS As cEcosimDatastructures

#End Region ' Private variables

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of this class.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">Core</see> to operate onto.</param>
    ''' <param name="ecopathDS">The <see cref="cEcopathDataStructures">Ecopath data structures</see> to operate onto.</param>
    ''' <param name="stanzaDS">The <see cref="cStanzaDatastructures">Stanza data structures</see> to operate onto.</param>
    ''' <param name="taxonDS">The <see cref="cTaxonDataStructures">Taxonomy data structures</see> to operate onto.</param>
    ''' <param name="ecosimDS">The <see cref="cEcosimDatastructures">Ecosim data structures</see> to operate onto.</param>
    ''' <param name="iTime">The Ecosim time to calculate the indicators for.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, _
                   ByVal ecopathDS As cEcopathDataStructures, _
                   ByVal ecosimDS As cEcosimDatastructures, _
                   ByVal iTime As Integer, _
                   ByVal stanzaDS As cStanzaDatastructures, _
                   ByVal taxonDS As cTaxonDataStructures, _
                   ByVal lookup As cTaxonAnalysis)

        MyBase.New(core, ecopathDS, stanzaDS, taxonDS, lookup)

        Me.m_iTime = iTime
        Me.m_ecosimDS = ecosimDS

    End Sub

#End Region ' Constructor

#Region " Core data access and public bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the associated <see cref="cEcosimDatastructures">Ecosim data structures</see>.
    ''' </summary>
    ''' <returns>The associated <see cref="cEcosimDatastructures">Ecosim data structures</see>.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function EcosimDS() As cEcosimDatastructures
        Return Me.m_ecosimDS
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the Ecosim time that this indicator represents.
    ''' </summary>
    ''' <returns>The Ecosim time that these indicators represent.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Time() As Integer
        Return Me.m_iTime
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelBiomass"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelBiomass(iGroup As Integer) As Single
        Return Me.EcosimDS.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGroup, Me.Time)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelCatch(iGroup As Integer) As Single
        Return Me.EcosimDS.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, iGroup, Me.Time)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTL"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTL(iGroup As Integer) As Single
        Return Me.EcosimDS.TLSim(iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTLCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTLCatch() As Single
        Return Me.EcosimDS.TLC(Me.Time)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelDiscards"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelDiscards(iGroup As Integer) As Single
        Dim sLandings As Single = 0
        For iFleet As Integer = 1 To Me.EcopathDS.NumFleet
            sLandings += Me.EcosimDS.ResultsLandings(iGroup, iFleet)
        Next
        Return Me.EcosimDS.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, iGroup, Me.Time) - sLandings
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.KemptonsQ"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelKemptionsQ() As Single
        Return Me.EcosimDS.Kemptons(Me.Time)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ShannonDiversity"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelShannonDiversity() As Single
        Return Me.EcosimDS.ShannonDiversity(Me.Time)
    End Function

#End Region ' Core data access and public bits

End Class
