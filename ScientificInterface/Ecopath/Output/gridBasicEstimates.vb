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
Option Explicit On

Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)>
    Public Class gridBasicEstimates
        Inherits EwEGrid

        Enum eColumnTypes As Integer
            Index = 0
            Name
            TL
            Area
            BArea
            B
            Z
            PB
            QB
            EE
            GE
            BA
            BArate
        End Enum

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell()
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, eColumnTypes.TL) = New EwEColumnHeaderCell(eVarNameFlags.TTLX)
            Me(0, eColumnTypes.Area) = New EwEColumnHeaderCell(eVarNameFlags.HabitatArea)
            Me(0, eColumnTypes.BArea) = New EwEColumnHeaderCell(eVarNameFlags.BiomassAreaOutput)
            Me(0, eColumnTypes.B) = New EwEColumnHeaderCell(eVarNameFlags.Biomass)
            Me(0, eColumnTypes.Z) = New EwEColumnHeaderCell(eVarNameFlags.Z)
            Me(0, eColumnTypes.PB) = New EwEColumnHeaderCell(eVarNameFlags.PBOutput)
            Me(0, eColumnTypes.QB) = New EwEColumnHeaderCell(eVarNameFlags.QBOutput)
            Me(0, eColumnTypes.EE) = New EwEColumnHeaderCell(eVarNameFlags.EEOutput)
            Me(0, eColumnTypes.GE) = New EwEColumnHeaderCell(eVarNameFlags.GEOutput)
            Me(0, eColumnTypes.BA) = New EwEColumnHeaderCell(eVarNameFlags.BioAccumOutput)
            Me(0, eColumnTypes.BArate) = New EwEColumnHeaderCell(eVarNameFlags.BioAccumRate, eDescriptorTypes.Abbreviation)

            Me.FixedColumns = 2
            Me.FixedColumnWidths = True

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cEcoPathGroupOutput = Nothing
            Dim cell As EwECellBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim iStanzaPrev As Integer = -1
            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            'Remove existing rows
            Me.RowsCount = 1

            ' Create rows for all groups
            For i As Integer = 0 To groups.Count - 1

                ' Get corresponding Ecopath output group 
                group = Me.Core.EcoPathGroupOutputs(groups(i).Index)

                If Not group.IsMultiStanza Then

                    iRow = Me.AddRow
                    UpdateRow(iRow, group)

                Else
                    ' Group is stanza
                    sg = Core.StanzaGroups(group.iStanza)
                    If group.iStanza <> iStanzaPrev Then

                        ' Complete row with dummy cells
                        iRow = Me.AddRow()
                        For j As Integer = 0 To Me.ColumnsCount - 1 : Me(iRow, j) = New EwERowHeaderCell() : Next

                        hgcStanza = New EwEHierarchyGridCell()
                        Me(iRow, eColumnTypes.Index) = hgcStanza
                        Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)

                        iStanzaPrev = group.iStanza
                        iRow = Me.AddRow
                    Else
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If

                    'Display group info
                    hgcStanza.AddChildRow(iRow)
                    UpdateRow(iRow, group, True)
                End If
            Next i

        End Sub

        Private Sub UpdateRow(ByVal iRow As Integer, ByVal source As cCoreInputOutputBase, Optional ByVal bIsStanza As Boolean = False)

            Dim cell As EwECellBase = Nothing

            Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
            If bIsStanza Then
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderChildCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Else
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            End If

            Me(iRow, eColumnTypes.TL) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.TTLX)
            Me(iRow, eColumnTypes.Area) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.HabitatArea)
            Me(iRow, eColumnTypes.BArea) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BiomassAreaOutput)
            Me(iRow, eColumnTypes.B) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.Biomass)
            Me(iRow, eColumnTypes.BA) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BioAccumOutput)
            Me(iRow, eColumnTypes.BArate) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BioAccumRatePerYear)

            If bIsStanza Then
                Me(iRow, eColumnTypes.Z) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.PBOutput)
            Else
                cell = New EwECell("", GetType(String))
                cell.Style = cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.Z) = cell
            End If

            If Not bIsStanza Then
                Me(iRow, eColumnTypes.PB) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.PBOutput)
            Else
                cell = New EwECell("", GetType(String))
                cell.Style = cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.PB) = cell
            End If

            Me(iRow, eColumnTypes.QB) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.QBOutput)
            Me(iRow, eColumnTypes.EE) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.EEOutput)
            Me(iRow, eColumnTypes.GE) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.GEOutput)

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()

            Dim ci As ColumnInfo = Me.Columns(eColumnTypes.Z)

            Me.Rows(0).Height = 60
            Me.Columns(eColumnTypes.Index).Width = 24
            Me.Columns(eColumnTypes.Name).Width = 120
            Me.Columns(eColumnTypes.Name).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize

            For i As Integer = 2 To Me.ColumnsCount - 1
                Me(0, i).VisualModel.TextAlignment = ContentAlignment.MiddleLeft
            Next

            If (Me.Core Is Nothing) Then Return

            ci.Visible = (Me.Core.nStanzas > 0)

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
