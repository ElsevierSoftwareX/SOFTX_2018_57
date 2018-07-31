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
Imports EwECore.Style
Imports EwEUtils.Core
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports 

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid displaying Ecopath Basic Input information.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)>
    Public Class gridBasicInput
        Inherits EwEGrid

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            Area
            BA
            Z
            PB
            QB
            EE
            OtherMort
            GE
            GS
            DetImp
        End Enum

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell()
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, eColumnTypes.Area) = New EwEColumnHeaderCell(eVarNameFlags.HabitatArea)
            Me(0, eColumnTypes.BA) = New EwEColumnHeaderCell(eVarNameFlags.BiomassAreaInput)
            Me(0, eColumnTypes.Z) = New EwEColumnHeaderCell(eVarNameFlags.Z)
            Me(0, eColumnTypes.PB) = New EwEColumnHeaderCell(eVarNameFlags.PBInput)
            Me(0, eColumnTypes.QB) = New EwEColumnHeaderCell(eVarNameFlags.QBInput)
            Me(0, eColumnTypes.EE) = New EwEColumnHeaderCell(eVarNameFlags.EEInput)
            Me(0, eColumnTypes.OtherMort) = New EwEColumnHeaderCell(eVarNameFlags.OtherMortInput)
            Me(0, eColumnTypes.GE) = New EwEColumnHeaderCell(eVarNameFlags.GEInput)
            Me(0, eColumnTypes.GS) = New EwEColumnHeaderCell(eVarNameFlags.GS)
            Me(0, eColumnTypes.DetImp) = New EwEColumnHeaderCell(eVarNameFlags.DetImp)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cCoreGroupBase = Nothing
            Dim cell As EwECellBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim iStanzaPrev As Integer = -1
            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            ' Remove existing rows
            Me.RowsCount = 1

            ' Create rows for all groups
            For i As Integer = 0 To groups.Count - 1

                group = groups(i)

                If Not group.IsMultiStanza Then 'If group is non-stanza Then display group info
                    iRow = Me.AddRow()
                    Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                    Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
                    Me(iRow, eColumnTypes.Area) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.HabitatArea)

                    cell = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.BiomassAreaInput)
                    cell.SuppressZero = True
                    Me(iRow, eColumnTypes.BA) = cell

                    cell = New EwECell("", GetType(String))
                    cell.Style = cStyleGuide.eStyleFlags.NotEditable
                    Me(iRow, eColumnTypes.Z) = cell

                    cell = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.PBInput)
                    cell.SuppressZero = True
                    Me(iRow, eColumnTypes.PB) = cell

                    cell = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.QBInput)
                    cell.SuppressZero = True
                    Me(iRow, eColumnTypes.QB) = cell

                    Me(iRow, eColumnTypes.EE) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.EEInput)
                    Me(iRow, eColumnTypes.OtherMort) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.OtherMortInput)
                    Me(iRow, eColumnTypes.GE) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.GEInput)
                    Me(iRow, eColumnTypes.GS) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.GS)
                    Me(iRow, eColumnTypes.DetImp) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.DetImp)

                    ' Forget any stanza links
                    iStanzaPrev = -1

                Else 'Group is stanza

                    sg = Core.StanzaGroups(group.iStanza)

                    ' Create hierarchy cell if entering a new stanza config
                    If group.iStanza <> iStanzaPrev Then

                        ' Fill row with dummy cells. We'll do something fancy here one day
                        iRow = Me.AddRow()
                        For j As Integer = 0 To Me.ColumnsCount - 1 : Me(iRow, j) = New EwERowHeaderCell() : Next

                        hgcStanza = New EwEHierarchyGridCell()
                        Me(iRow, eColumnTypes.Index) = hgcStanza
                        Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)

                        iStanzaPrev = group.iStanza
                        iRow = Me.AddRow()
                    Else
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If

                    Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                    Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderChildCell(Me.PropertyManager, group, eVarNameFlags.Name)
                    Me(iRow, eColumnTypes.Area) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.HabitatArea)

                    cell = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.BiomassAreaInput)
                    cell.Behaviors.Add(Me.EwEEditHandler)
                    cell.SuppressZero = True
                    Me(iRow, eColumnTypes.BA) = cell

                    cell = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.PBInput)
                    cell.Behaviors.Add(Me.EwEEditHandler)
                    cell.SuppressZero = True
                    Me(iRow, eColumnTypes.Z) = cell

                    cell = New EwECell("", GetType(String))
                    cell.Style = cStyleGuide.eStyleFlags.NotEditable
                    cell.Behaviors.Add(Me.EwEEditHandler)
                    Me(iRow, eColumnTypes.PB) = cell

                    cell = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.QBInput)
                    cell.Behaviors.Add(Me.EwEEditHandler)
                    cell.SuppressZero = True
                    Me(iRow, eColumnTypes.QB) = cell

                    Me(iRow, eColumnTypes.EE) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.EEInput)
                    Me(iRow, eColumnTypes.OtherMort) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.OtherMortInput)
                    Me(iRow, eColumnTypes.GE) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.GEInput)
                    Me(iRow, eColumnTypes.GE).Behaviors.Add(Me.EwEEditHandler)
                    Me(iRow, eColumnTypes.GS) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.GS)
                    Me(iRow, eColumnTypes.DetImp) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.DetImp)

                    hgcStanza.AddChildRow(iRow)

                End If

            Next i

        End Sub

        Protected Overrides Sub OnCellDoubleClicked(ByVal p As Position, ByVal cell As Cells.ICellVirtual)
            Dim dlg As EditMultiStanza = Nothing
            Dim prop As cProperty = Nothing
            Dim group As cEcoPathGroupInput = Nothing

            If Not TypeOf cell Is PropertyCell Then Return
            prop = DirectCast(cell, PropertyCell).GetProperty()
            group = DirectCast(prop.Source, cEcoPathGroupInput)

            dlg = New EditMultiStanza(Me.UIContext, group)
            dlg.ShowDialog(Me)
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

            If Me.UIContext Is Nothing Then Return

            ci.Visible = (Me.Core.nStanzas > 0)

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
