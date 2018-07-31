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

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Particle Size Distribution Growth user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridGrowthParameters
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
            Me(0, 5) = New EwEColumnHeaderCell(eVarNameFlags.WinfInput)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_K_VBGF)
            Me(0, 7) = New EwEColumnHeaderCell(SharedResources.HEADER_TZERO_VBGF)
            Me(0, 8) = New EwEColumnHeaderCell(eVarNameFlags.TCatchInput)
            Me(0, 9) = New EwEColumnHeaderCell(eVarNameFlags.TmaxInput)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim group As cEcoPathGroupInput = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim intStanzaGroupIndex(core.nLivingGroups) As Integer 'Hold the stanza group index
            Dim hgcStanza As EwEHierarchyGridCell = Nothing
            Dim dtStanzaCells As New Dictionary(Of cStanzaGroup, EwEHierarchyGridCell)

            For i As Integer = 1 To core.nLivingGroups : intStanzaGroupIndex(i) = -1 : Next

            'Tag stanza group
            For stanzaGroupIndex As Integer = 0 To core.nStanzas - 1
                sg = core.StanzaGroups(stanzaGroupIndex)

                For iStanza As Integer = 1 To sg.nLifeStages
                    group = Core.EcoPathGroupInputs(sg.iGroups(iStanza))
                    intStanzaGroupIndex(group.Index) = stanzaGroupIndex
                Next
            Next

            'Remove existing rows
            Me.RowsCount = 1

            'Create rows for all groups
            For iGroup As Integer = 1 To core.nLivingGroups
                group = core.EcoPathGroupInputs(iGroup)
                ' Is group stanza?
                If intStanzaGroupIndex(group.Index) = -1 Then
                    ' #No: display group info
                    iRow = Me.AddRow
                    FillInRows(iRow, group)
                Else
                    '#Yes: Group is stanza
                    sg = core.StanzaGroups(intStanzaGroupIndex(group.Index))
                    If Not dtStanzaCells.ContainsKey(sg) Then
                        hgcStanza = New EwEHierarchyGridCell()
                        dtStanzaCells.Add(sg, hgcStanza)
                        iRow = Me.AddRow()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
                        ' Complete row with dummy cells
                        For i As Integer = 2 To 9 : Me(iRow, i) = New EwERowHeaderCell() : Next
                        iRow = Me.AddRow
                    Else
                        hgcStanza = dtStanzaCells(sg)
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If
                    'Display group info
                    hgcStanza.AddChildRow(iRow)
                    FillInRows(iRow, group, True)
                End If
            Next

        End Sub

        Private Sub FillInRows(ByVal iRow As Integer, ByVal group As cEcoPathGroupInput, Optional ByVal isIndented As Boolean = False)

            ' Get the group name from EcopathInput
            Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)

            If isIndented Then
                Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, group, eVarNameFlags.Name)
            Else
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
            End If

            Me(iRow, 2) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.AinLWInput)
            Me(iRow, 3) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.BinLWInput)
            Me(iRow, 4) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.LooInput)
            Me(iRow, 5) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.WinfInput)
            Me(iRow, 6) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.VBK)
            Me(iRow, 7) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.t0Input)
            Me(iRow, 8) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.TCatchInput)
            Me(iRow, 9) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.TmaxInput)

        End Sub

        Protected Overrides Sub FinishStyle()

            MyBase.FinishStyle()

            For iCol As Integer = 2 To Me.ColumnsCount - 1
                Me(0, iCol).VisualModel.TextAlignment = ContentAlignment.MiddleLeft
            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
