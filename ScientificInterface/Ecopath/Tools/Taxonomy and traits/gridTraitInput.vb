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

Imports EwECore
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports EwECore.Style

#End Region ' Imports 

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid displaying Ecopath Basic Input information.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)>
    Public Class gridTaxonInput
        Inherits EwEGrid

#Region " Private vars "

        Private m_editorEcology As EwEComboBoxCellEditor = Nothing
        Private m_editorOrganism As EwEComboBoxCellEditor = Nothing
        Private m_editorOccurrence As EwEComboBoxCellEditor = Nothing
        Private m_editorExploitation As EwEComboBoxCellEditor = Nothing
        Private m_editorConservation As EwEComboBoxCellEditor = Nothing

        Private Enum eColumnTypes As Integer
            Hierarchy = 0
            Name
            Organism
            Ecology
            Occurrence
            PropBiomass
            PropCatch
            Conservation
            Exploitation
            VulIndex
            MeanLen
            MaxLen
            MeanWeight
            MeanLifeSpan
        End Enum

#End Region ' Private vars

        Public Sub New()
            MyBase.New()

            ' Prepare editors
            Me.m_editorEcology = New EwEComboBoxCellEditor(New cEcologyTypeFormatter())
            Me.m_editorOrganism = New EwEComboBoxCellEditor(New cOrganismTypeFormatter())
            Me.m_editorOccurrence = New EwEComboBoxCellEditor(New cOccurrenceTypeFormatter())
            Me.m_editorExploitation = New EwEComboBoxCellEditor(New cExploitationTypeFormatter())
            Me.m_editorConservation = New EwEComboBoxCellEditor(New cIUCNConservationTypeFormatter())

        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            If (Me.UIContext Is Nothing) Then Return

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Hierarchy) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(eVarNameFlags.Species)
            Me(0, eColumnTypes.Ecology) = New EwEColumnHeaderCell(eVarNameFlags.EcologyType)
            Me(0, eColumnTypes.Organism) = New EwEColumnHeaderCell(eVarNameFlags.OrganismType)
            Me(0, eColumnTypes.PropBiomass) = New EwEColumnHeaderCell(eVarNameFlags.TaxonPropBiomass)
            Me(0, eColumnTypes.PropCatch) = New EwEColumnHeaderCell(eVarNameFlags.TaxonPropCatch)
            Me(0, eColumnTypes.Conservation) = New EwEColumnHeaderCell(eVarNameFlags.IUCNConservationStatus)
            Me(0, eColumnTypes.Exploitation) = New EwEColumnHeaderCell(eVarNameFlags.ExploitationStatus)
            Me(0, eColumnTypes.Occurrence) = New EwEColumnHeaderCell(eVarNameFlags.OccurrenceStatus)
            Me(0, eColumnTypes.MeanLen) = New EwEColumnHeaderCell(eVarNameFlags.TaxonMeanLength)
            Me(0, eColumnTypes.MaxLen) = New EwEColumnHeaderCell(eVarNameFlags.TaxonMaxLength)
            Me(0, eColumnTypes.MeanWeight) = New EwEColumnHeaderCell(eVarNameFlags.TaxonMeanWeight)
            Me(0, eColumnTypes.MeanLifeSpan) = New EwEColumnHeaderCell(eVarNameFlags.TaxonMeanLifespan)
            Me(0, eColumnTypes.VulIndex) = New EwEColumnHeaderCell(eVarNameFlags.TaxonVulnerabilityIndex)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            If (Me.UIContext Is Nothing) Then Return

            Dim stanza As cStanzaGroup = Nothing
            Dim group As cEcoPathGroupInput = Nothing
            Dim taxon As cTaxon = Nothing
            Dim hgcParent As EwEHierarchyGridCell = Nothing
            Dim iRow As Integer = -1
            Dim abStanzaHandled(Me.Core.nStanzas) As Boolean

            ' Remove existing rows
            Me.RowsCount = 1

            For iGroup As Integer = 1 To Me.Core.nGroups

                group = Me.Core.EcoPathGroupInputs(iGroup)
                If group.IsMultiStanza Then

                    If Not abStanzaHandled(group.iStanza) Then
                        stanza = Me.Core.StanzaGroups(group.iStanza)
                        iRow = Me.AddRow()

                        hgcParent = New EwEHierarchyGridCell()
                        Me(iRow, eColumnTypes.Hierarchy) = hgcParent
                        Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderParentCell(Me.PropertyManager, stanza, eVarNameFlags.Name, Nothing, hgcParent)
                        For iCol As Integer = eColumnTypes.Name + 1 To Me.ColumnsCount - 1
                            Me(iRow, iCol) = New EwERowHeaderCell("")
                        Next

                        For iTaxon As Integer = 1 To Me.Core.nTaxon
                            taxon = Me.Core.Taxon(iTaxon)
                            If taxon.iStanza = stanza.Index Then
                                iRow += 1
                                Me.AddTaxonRow(taxon, iRow, hgcParent)
                            End If
                        Next
                        abStanzaHandled(group.iStanza) = True
                    End If

                Else
                    iRow = Me.AddRow()

                    hgcParent = New EwEHierarchyGridCell()
                    Me(iRow, eColumnTypes.Hierarchy) = hgcParent
                    Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(String.Format(SharedResources.GENERIC_LABEL_INDEXED, group.Index, group.Name))
                    For iCol As Integer = eColumnTypes.Name + 1 To Me.ColumnsCount - 1
                        Me(iRow, iCol) = New EwERowHeaderCell("")
                    Next

                    For iTaxon As Integer = 1 To Me.Core.nTaxon
                        taxon = Me.Core.Taxon(iTaxon)
                        If taxon.iGroup = group.Index Then
                            iRow += 1
                            Me.AddTaxonRow(taxon, iRow, hgcParent)
                        End If
                    Next
                End If
            Next

        End Sub

        Protected Property Taxon(ByVal iRow As Integer) As cTaxon
            Get
                Return DirectCast(Me.Rows(iRow).Tag, cTaxon)
            End Get
            Set(ByVal value As cTaxon)
                Me.Rows(iRow).Tag = value
            End Set
        End Property

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

        Public Function SelectedTaxa() As cTaxon()
            Dim lTaxa As New List(Of cTaxon)
            Dim taxon As cTaxon = Nothing
            For Each row As RowInfo In Me.Selection.SelectedRows
                taxon = Me.Taxon(row.Index)
                If (taxon IsNot Nothing) Then
                    lTaxa.Add(taxon)
                End If
            Next
            Return lTaxa.ToArray()
        End Function

        Protected Overrides Function OnCellValueChanged(ByVal p As SourceGrid2.Position, ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean
            Dim taxon As cTaxon = Me.Taxon(p.Row)

            Select Case DirectCast(p.Column, eColumnTypes)
                Case eColumnTypes.Conservation
                    taxon.IUCNConservationStatus = CType(cell.GetValue(p), eIUCNConservationStatusTypes)
                Case eColumnTypes.Ecology
                    taxon.EcologyType = CType(cell.GetValue(p), eEcologyTypes)
                Case eColumnTypes.PropCatch
                    taxon.ProportionCatch = CSng(cell.GetValue(p))
                Case eColumnTypes.Occurrence
                    taxon.OccurrenceStatus = CType(cell.GetValue(p), eOccurrenceStatusTypes)
                Case eColumnTypes.Organism
                    taxon.OrganismType = CType(cell.GetValue(p), eOrganismTypes)
                Case eColumnTypes.Exploitation
                    taxon.ExploitationStatus = CType(cell.GetValue(p), eExploitationTypes)
                Case Else

            End Select
            Return MyBase.OnCellValueChanged(p, cell)
        End Function

        Private Sub AddTaxonRow(ByVal taxon As cTaxon, ByVal iRow As Integer, ByVal hgcParent As EwEHierarchyGridCell)

            Dim cell As EwECellBase = Nothing
            Dim propScName As cScientificNameProperty = New cScientificNameProperty(Me.PropertyManager, taxon)

            Me.Rows.Insert(iRow)
            Me(iRow, eColumnTypes.Hierarchy) = New EwERowHeaderCell(CStr(taxon.Index))

            cell = New PropertyRowHeaderChildCell(propScName)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iRow, eColumnTypes.Name) = cell
            Me.RegisterLocalProperty(propScName)

            Me(iRow, eColumnTypes.Ecology) = New SourceGrid2.Cells.Real.Cell(taxon.EcologyType, Me.m_editorEcology)
            Me(iRow, eColumnTypes.Ecology).Behaviors.Add(Me.EwEEditHandler)
            Me(iRow, eColumnTypes.Organism) = New SourceGrid2.Cells.Real.Cell(taxon.OrganismType, Me.m_editorOrganism)
            Me(iRow, eColumnTypes.Organism).Behaviors.Add(Me.EwEEditHandler)

            Me(iRow, eColumnTypes.Conservation) = New SourceGrid2.Cells.Real.Cell(taxon.IUCNConservationStatus, Me.m_editorConservation)
            Me(iRow, eColumnTypes.Conservation).Behaviors.Add(Me.EwEEditHandler)
            Me(iRow, eColumnTypes.Occurrence) = New SourceGrid2.Cells.Real.Cell(taxon.OccurrenceStatus, Me.m_editorOccurrence)
            Me(iRow, eColumnTypes.Occurrence).Behaviors.Add(Me.EwEEditHandler)
            Me(iRow, eColumnTypes.Exploitation) = New SourceGrid2.Cells.Real.Cell(taxon.ExploitationStatus, Me.m_editorExploitation)
            Me(iRow, eColumnTypes.Exploitation).Behaviors.Add(Me.EwEEditHandler)

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonPropBiomass)
            cell.SuppressZero = True
            Me(iRow, eColumnTypes.PropBiomass) = cell

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonPropCatch)
            cell.SuppressZero = True
            Me(iRow, eColumnTypes.PropCatch) = cell

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonMeanLength)
            cell.SuppressZero = True
            Me(iRow, eColumnTypes.MeanLen) = cell

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonMaxLength)
            cell.SuppressZero = True
            Me(iRow, eColumnTypes.MaxLen) = cell

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonMeanWeight)
            cell.SuppressZero = True
            Me(iRow, eColumnTypes.MeanWeight) = cell

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonMeanLifespan)
            cell.SuppressZero = True
            Me(iRow, eColumnTypes.MeanLifeSpan) = cell

            cell = New PropertyCell(Me.PropertyManager, taxon, eVarNameFlags.TaxonVulnerabilityIndex)
            Me(iRow, eColumnTypes.VulIndex) = cell

            hgcParent.AddChildRow(iRow)
            Me.Taxon(iRow) = taxon

        End Sub

    End Class

End Namespace