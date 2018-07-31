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
Imports EwECore.Style
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Landings user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridFisheryInputLandings
        : Inherits EwEGrid

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = True
        End Sub

        Protected Overrides Sub InitStyle()

            Dim source As cCoreInputOutputBase = Nothing
            Dim md As cVariableMetaData = Nothing

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            'Define grid dimensions
            Me.Redim(Core.nGroups + 2, Core.nFleets + 3)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            ' Dynamic column header - fleet name
            For fleetIndex As Integer = 1 To Core.nFleets
                source = Me.Core.EcopathFleetInputs(fleetIndex)

                md = source.GetVariableMetadata(eVarNameFlags.Landings)
                Me(0, fleetIndex + 1) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name, Nothing, md.Units)

            Next

            ' Total column
            Me(0, Core.nFleets + 2) = New EwEColumnHeaderCell(SharedResources.HEADER_TOTAL)
            ' Sum row
            Me(Core.nGroups + 1, 1) = New EwERowHeaderCell(SharedResources.HEADER_SUM)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cCoreGroupBase = Nothing
            Dim groupSec As cCoreGroupBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim fleet As cEcopathFleetInput = Nothing
            Dim iRow As Integer = -1
            Dim iStanzaPrev As Integer = -1

            Dim pm As cPropertyManager = Me.PropertyManager
            Dim prop As cProperty = Nothing

            Dim alSumRow As New ArrayList()
            Dim alSumAll As New ArrayList()
            Dim alSumCol As New ArrayList()

            Dim opSumAll As cMultiOperation = Nothing
            Dim opSumRow As cMultiOperation = Nothing
            Dim opSumCol As cMultiOperation = Nothing

            Dim propSumRow As cFormulaProperty = Nothing
            Dim propSumAll As cFormulaProperty = Nothing
            Dim propSumCol As cFormulaProperty = Nothing

            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            'Remove existing rows
            Me.RowsCount = 1

            ' Done?
            If Core.nFleets = 0 Then Return

            'Create rows for all groups
            For i As Integer = 0 To groups.Count - 1

                ' Clear the arrayList for the new row
                alSumRow.Clear()
                ' Get the Ecopath input for this specific group
                group = groups(i)

                'If group is non-stanza Then display group info
                If Not group.IsMultiStanza Then
                    iRow = Me.AddRow
                    FillInRows(iRow, group, alSumRow, alSumAll)
                    iStanzaPrev = -1
                Else 'Group is stanza
                    sg = Core.StanzaGroups(group.iStanza)
                    ' Switching stanza?
                    If (iStanzaPrev <> group.iStanza) Then
                        ' #Yes: create hierarchy box
                        hgcStanza = New EwEHierarchyGridCell()
                        iRow = Me.AddRow()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
                        For j As Integer = 2 To Core.nFleets + 2 : Me(iRow, j) = New EwERowHeaderCell() : Next
                        iStanzaPrev = group.iStanza
                    End If
                    iRow = Me.AddRow
                    hgcStanza.AddChildRow(iRow)
                    FillInRows(iRow, group, alSumRow, alSumAll, True)
                End If

                ' Set the property to the last cell of the row, which is the sum of the row
                opSumRow = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumRow.ToArray())
                propSumRow = Me.Formula(opSumRow)
                Me(iRow, Me.ColumnsCount - 1) = New PropertyCell(propSumRow)
            Next

            ' Sum row
            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell(CStr(iRow))
            Me(iRow, 1) = New EwERowHeaderCell(SharedResources.HEADER_SUM)
            For iFleet As Integer = 1 To Core.nFleets
                fleet = Core.EcopathFleetInputs(iFleet)
                alSumCol.Clear()

                For iGroup As Integer = 1 To Core.nGroups
                    groupSec = Core.EcoPathGroupInputs(iGroup)
                    prop = pm.GetProperty(fleet, eVarNameFlags.Landings, groupSec)
                    alSumCol.Add(prop)
                Next
                opSumCol = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumCol.ToArray())
                propSumCol = Me.Formula(opSumCol)
                ' Set the property to the last cell of the column, which is the sum of the column
                Me(Me.RowsCount - 1, iFleet + 1) = New PropertyCell(propSumCol)
            Next


            opSumAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumAll.ToArray())
            propSumAll = Me.Formula(opSumAll)
            ' Set the property to the bottom-right cell, which is the sum of all cells
            Me(Me.RowsCount - 1, Me.ColumnsCount - 1) = New PropertyCell(propSumAll)

        End Sub

        Private Sub FillInRows(ByVal iRow As Integer, ByVal source As cCoreInputOutputBase, _
            ByRef alSumRow As ArrayList, ByRef alSumAll As ArrayList, Optional ByVal isIndented As Boolean = False)

            Dim sourceSec As cCoreInputOutputBase = Nothing
            Dim pm As cPropertyManager = Me.PropertyManager
            Dim prop As cProperty = Nothing
            Dim propCell As PropertyCell = Nothing

            Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
            If isIndented Then
                Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Else
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            End If

            ' For each fleet (each column) 
            For fleetIndex As Integer = 1 To Core.nFleets
                ' Get the fleet object 
                sourceSec = Core.EcopathFleetInputs(fleetIndex)
                ' Get the index landing property
                prop = pm.GetProperty(sourceSec, eVarNameFlags.Landings, source)
                ' Set the property to the cell
                propCell = New PropertyCell(prop)
                ' Configure the cell
                propCell.SuppressZero = True
                ' Set the cell
                Me(iRow, fleetIndex + 1) = propCell
                ' Add the property to ArrayList; it is used for the sum
                alSumRow.Add(prop)
                alSumAll.Add(prop)
            Next
        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace

