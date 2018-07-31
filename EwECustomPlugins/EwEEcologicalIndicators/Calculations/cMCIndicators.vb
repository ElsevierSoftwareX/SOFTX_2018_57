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
''' Class that computes all MonteCarlo-based indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cMCIndicators
    Inherits cEcosimIndicators

#Region " Private variables "

    Private m_iIteration As Integer = 0

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
                    ByVal iIter As Integer, _
                    ByVal iTime As Integer, _
                    ByVal stanzaDS As cStanzaDatastructures, _
                    ByVal taxonDS As cTaxonDataStructures, _
                    ByVal lookup As cTaxonAnalysis)

        MyBase.New(core, ecopathDS, ecosimDS, iTime, stanzaDS, taxonDS, lookup)
        Me.m_iIteration = iTime

    End Sub

#End Region ' Constructor

#Region " Core data access and public bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the Ecosim time that this indicator represents.
    ''' </summary>
    ''' <returns>The Ecosim time that these indicators represent.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Iteration() As Integer
        Return Me.m_iIteration
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cIndicators.ModelTLCatch"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function ModelTLCatch() As Single
        Return Me.EcosimDS.TLC(Me.Time)
    End Function

#End Region ' Core data access and public bits

End Class
