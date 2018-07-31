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

#End Region ' Imports

Namespace Ecopath

    Public Class cEcopathMergeGroupsDatastructures

        Public GroupName As String
        Public GroupColor As Integer

        Public IndexTarget As Integer
        Public IndexMerge As Integer
        Public Estimate As cEcopathMergeGroups.eEstimate

        Public Binput As Single
        Public BHinput As Single

        Public Area As Single

        Public BH As Single
        Public BAInput As Single
        Public BaBi As Single
        Public PBinput As Single
        Public QBinput As Single
        Public EEinput As Single
        Public GEinput As Single
        Public GS As Single

        Public OtherMortinput As Single
        Public Immig As Single
        Public Emigration As Single
        Public EmigRate As Single
        Public Det As Single
        Public DtImp As Single
        Public Shadow As Single

        Public DCInput(,) As Single

        Public Discard() As Single
        Public Landing() As Single
        Public Market() As Single
        Public DiscardFate() As Single

        ' Stanza
        Public iStanza As Integer = -1
        Public iLifeStage As Integer = -1
        Public EcopathCode(,) As Integer
        Public BaseStanza() As Integer
        Public BaseStanzaCB() As Integer
        Public Age1(,) As Integer
        Public StanzaZ(,) As Single

        ' Taxa
        Public TaxonPropBiomass As New Dictionary(Of Integer, Single)
        Public TaxonPropCatch As New Dictionary(Of Integer, Single)

        Public IsValid As Boolean = False

        Public Sub Init(ecopathds As cEcopathDataStructures, stanzaDS As cStanzaDatastructures)

            ReDim Discard(ecopathds.NumFleet)
            ReDim Landing(ecopathds.NumFleet)
            ReDim Market(ecopathds.NumFleet)
            ReDim DiscardFate(ecopathds.NumGroups)

            ReDim DCInput(ecopathds.NumGroups, ecopathds.NumGroups)
            For iPred As Integer = 1 To ecopathds.NumLiving
                For iPrey As Integer = 0 To ecopathds.NumGroups
                    DCInput(iPred, iPrey) = ecopathds.DCInput(iPred, iPrey)
                Next
            Next

            ReDim BaseStanza(stanzaDS.Nsplit)
            Array.Copy(stanzaDS.BaseStanza, BaseStanza, BaseStanza.Length)

            ReDim BaseStanzaCB(stanzaDS.Nsplit)
            Array.Copy(stanzaDS.BaseStanzaCB, BaseStanzaCB, BaseStanzaCB.Length)

            ReDim Age1(stanzaDS.Nsplit, stanzaDS.MaxStanza)
            Array.Copy(stanzaDS.Age1, Age1, Age1.Length)

            ReDim StanzaZ(stanzaDS.Nsplit, stanzaDS.MaxStanza)
            Array.Copy(stanzaDS.Stanza_Z, StanzaZ, StanzaZ.Length)

            Me.TaxonPropBiomass.Clear()
            Me.TaxonPropCatch.Clear()

        End Sub

    End Class

End Namespace
