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
Imports System.Drawing
Imports EwECore
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Class that computes all Ecospace-based indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcospaceIndicators
    Inherits cIndicators

#Region " Private variables "

    ''' <summary>The map location (col, row) that this indicator represents.</summary>
    Private m_ptLocation As Point = Nothing
    ''' <summary>The <see cref="cEcopathDataStructures">Ecopath data structures</see> to operate onto.</summary>
    Private m_ecopathDS As cEcopathDataStructures = Nothing
    ''' <summary>The <see cref="cEcosimDataStructures">Ecosim data structures</see> to operate onto.</summary>
    Private m_ecosimDS As cEcosimDatastructures = Nothing
    ''' <summary>The <see cref="cEcospaceDataStructures">Ecospace data structures</see> to operate onto.</summary>
    Private m_ecospaceDS As cEcospaceDataStructures = Nothing

#End Region ' Private variables

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of this class.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">Core</see> to operate onto.</param>
    ''' <param name="ecopathDS">The <see cref="cEcopathDataStructures">Ecopath data structures</see> to operate onto.</param>
    ''' <param name="ecospaceDS">The <see cref="cEcospaceDataStructures">Ecospace data structures</see> to operate onto.</param>
    ''' <param name="ptLocation">The map location (col, row) that this indicator represents.</param>
    ''' <param name="stanzaDS">The <see cref="cStanzaDatastructures">Stanza data structures</see> to operate onto.</param>
    ''' <param name="taxonDS">The <see cref="cTaxonDataStructures">Taxonomy data structures</see> to operate onto.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, _
                   ByVal ecopathDS As cEcopathDataStructures, _
                   ByVal ecospaceDS As cEcospaceDataStructures, _
                   ByVal ptLocation As Point, _
                   ByVal stanzaDS As cStanzaDatastructures, _
                   ByVal taxonDS As cTaxonDataStructures, _
                   ByVal lookup As cTaxonAnalysis)

        MyBase.New(core, ecopathDS, stanzaDS, taxonDS, lookup)

        'Sanity check
        Debug.Assert(ecospaceDS IsNot Nothing, "Aargh!")

        Me.m_ecopathDS = ecopathDS
        Me.m_ecosimDS = m_ecosimDS
        Me.m_ecospaceDS = ecospaceDS
        Me.m_ptLocation = ptLocation

    End Sub

#End Region ' Constructor

#Region " Core data access and public bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the associated <see cref="cEcospaceDatastructures">Ecospace data structures</see>.
    ''' </summary>
    ''' <returns>The associated <see cref="cEcospaceDatastructures">Ecospace data structures</see>.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function EcospaceDS() As cEcospaceDataStructures
        Return Me.m_ecospaceDS
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the map location (col, row) that this indicator represents.
    ''' </summary>
    ''' <returns>The Ecosim time that these indicators represent.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Location() As Point
        Return Me.m_ptLocation
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelBiomass"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelBiomass(iGroup As Integer) As Single
        Return Me.m_ecospaceDS.Bcell(Me.m_ptLocation.Y, Me.m_ptLocation.X, iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelCatch(iGroup As Integer) As Single
        Return Me.m_ecospaceDS.CatchMap(Me.m_ptLocation.Y, Me.m_ptLocation.X, iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTL"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTL(iGroup As Integer) As Single
        Return Me.m_ecospaceDS.TL(Me.m_ptLocation.Y, Me.m_ptLocation.X, iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTLCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTLCatch() As Single
        Return Me.m_ecospaceDS.TLc(Me.m_ptLocation.Y, Me.m_ptLocation.X)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelDiscards"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelDiscards(iGroup As Integer) As Single
        Return Me.m_ecospaceDS.DiscardsMap(Me.m_ptLocation.Y, Me.m_ptLocation.X, iGroup)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelKemptionsQ"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelKemptionsQ() As Single
        Return Me.m_ecospaceDS.KemptonsQ(Me.m_ptLocation.Y, Me.m_ptLocation.X)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelShannonDiversity"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelShannonDiversity() As Single
        Return Me.m_ecospaceDS.ShannonDiversity(Me.m_ptLocation.Y, Me.m_ptLocation.X)
    End Function

#End Region ' Overrides Core data access and public bits

End Class
