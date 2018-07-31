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
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)> _
    Public Class gridPSDGrowthEstimates
        : Inherits EwEGrid

        Public Sub New()
            MyBase.new()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()
            Me.Redim(1, 10) '9)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_A_IN_LW)
            Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_B_IN_LW)
            Me(0, 4) = New EwEColumnHeaderCell(eVarNameFlags.LooInput)
            Me(0, 5) = New EwEColumnHeaderCell(eVarNameFlags.WinfOutput)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_K_VBGF)
            Me(0, 7) = New EwEColumnHeaderCell(SharedResources.HEADER_TZERO_VBGF)
            Me(0, 8) = New EwEColumnHeaderCell(eVarNameFlags.TCatchOutput)
            Me(0, 9) = New EwEColumnHeaderCell(eVarNameFlags.TmaxOutput)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreInputOutputBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim intStanzaGroupIndex(Core.nLivingGroups) As Integer 'Hold the stanza group index
            Dim intStanzaGroupIndexPrev As Integer = -1
            Dim hgcStanza As EwEHierarchyGridCell = Nothing
            Dim dtStanzaCells As New Dictionary(Of cStanzaGroup, EwEHierarchyGridCell)

            For i As Integer = 1 To Core.nLivingGroups : intStanzaGroupIndex(i) = -1 : Next

            'Tag stanza group
            For stanzaGroupIndex As Integer = 0 To Core.nStanzas - 1
                sg = Core.StanzaGroups(stanzaGroupIndex)

                For iStanza As Integer = 1 To sg.nLifeStages
                    source = Core.EcoPathGroupInputs(sg.iGroups(iStanza))
                    intStanzaGroupIndex(source.Index) = stanzaGroupIndex
                Next
            Next

            'Remove existing rows
            Me.RowsCount = 1

            'Create rows for all groups
            For groupIndex As Integer = 1 To Core.nLivingGroups
                source = Core.EcoPathGroupOutputs(groupIndex)

                If intStanzaGroupIndex(source.Index) = -1 Then 'If group is non-stanza Then display group info
                    iRow = Me.AddRow
                    FillInRows(iRow, source)
                Else 'Group is stanza
                    sg = Core.StanzaGroups(intStanzaGroupIndex(source.Index))
                    If intStanzaGroupIndex(source.Index) <> intStanzaGroupIndexPrev Then 'If stanza group appears the first time Then diplay the + control
                        hgcStanza = New EwEHierarchyGridCell()
                        dtStanzaCells.Add(sg, hgcStanza)
                        iRow = Me.AddRow()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
                        ' Complete row with dummy cells
                        For i As Integer = 2 To 9 : Me(iRow, i) = New EwERowHeaderCell() : Next
                        intStanzaGroupIndexPrev = intStanzaGroupIndex(source.Index)
                        iRow = Me.AddRow
                    Else
                        hgcStanza = dtStanzaCells(sg)
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If
                    'Display group info
                    hgcStanza.AddChildRow(iRow)
                    FillInRows(iRow, source, True)
                End If
            Next groupIndex

        End Sub

        Private Sub FillInRows(ByVal iRow As Integer, ByVal source As cCoreInputOutputBase, Optional ByVal isIndented As Boolean = False)

            Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)

            If isIndented Then
                Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Else
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            End If

            Me(iRow, 2) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.AinLWOutput)
            Me(iRow, 3) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BinLWOutput)
            Me(iRow, 4) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.LooOutput)
            Me(iRow, 5) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.WinfOutput)
            Me(iRow, 6) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.VBK)
            Me(iRow, 7) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.t0Output)
            Me(iRow, 8) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.TCatchOutput)
            Me(iRow, 9) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.TmaxOutput)

        End Sub

        Protected Overrides Sub FinishStyle()

            MyBase.FinishStyle()

            Me.Rows(0).Height = 60
            'Me.Columns(0).Width = 24
            'Me.Columns(1).Width = 120
            'Me.Columns(1).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            'Me.Columns(2).Width = 52
            'Me.Columns(3).Width = 53
            'Me.Columns(4).Width = 67
            'Me.Columns(5).Width = 58
            'Me.Columns(6).Width = 66
            'Me.Columns(7).Width = 82
            'Me.Columns(8).Width = 69
            'Me.Columns(9).Width = 76

            For iCol As Integer = 2 To Me.ColumnsCount - 1
                Me(0, iCol).VisualModel.TextAlignment = ContentAlignment.MiddleLeft
            Next

        End Sub

        Private Function IsGroupSelected() As Boolean()

            Dim bGroupSelected(Core.nLivingGroups) As Boolean
            For i As Integer = 1 To Core.nLivingGroups
                bGroupSelected(i) = StyleGuide.GroupVisible(i)
            Next
            Return bGroupSelected

        End Function

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
