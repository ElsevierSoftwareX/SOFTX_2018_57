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
Imports SourceGrid2.Cells.Real
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Detritus Fate user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridDetritusFate
        : Inherits EwEGrid

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreGroupBase = Nothing

            'Define grid dimensions
            Me.Redim(Core.nGroups + 1, 4 + Core.nDetritusGroups)

            'Header cell (0,0) Source \ fate
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_SOURCEFATE)

            ' Detritus column header cells
            For i As Integer = 1 To Core.nDetritusGroups
                source = Core.EcoPathGroupInputs(Core.nGroups - Core.nDetritusGroups + i)
                Me(0, i + 1) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Next

            ' The export header cell
            Me(0, Core.nDetritusGroups + 2) = New EwEColumnHeaderCell(SharedResources.HEADER_EXPORT)
            ' The sum header cell
            Me(0, Core.nDetritusGroups + 3) = New EwEColumnHeaderCell(SharedResources.HEADER_SUM)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cCoreGroupBase = Nothing
            Dim groupSec As cCoreInputOutputBase = Nothing

            Dim prop As cProperty = Nothing
            Dim propSum As cSingleProperty = Nothing
            Dim propExport As cFormulaProperty = Nothing

            Dim alProp As New ArrayList()
            Dim propSumAll As cFormulaProperty = Nothing
            Dim opSumAll As cMultiOperation = Nothing
            Dim opMinus As cBinaryOperation = Nothing

            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim iStanzaPrev As Integer = -1

            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            'Remove existing rows
            Me.RowsCount = 1

            ' Configure static SUM prop
            propSum = New cSingleProperty()
            propSum.SetValue(1.0)
            propSum.SetStyle(cStyleGuide.eStyleFlags.Sum Or cStyleGuide.eStyleFlags.NotEditable)

            ' Create rows for all groups
            For i As Integer = 0 To groups.Count - 1

                group = groups(i)
                alProp.Clear()

                If (Not group.IsMultiStanza) Then
                    iRow = Me.AddRow
                    For iCol As Integer = 1 To Core.nDetritusGroups

                        groupSec = Core.EcoPathGroupInputs(Core.nGroups - Core.nDetritusGroups + iCol)

                        Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                        Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
                        prop = Me.PropertyManager.GetProperty(group, eVarNameFlags.DetritusFate, groupSec, True, Core.nGroups - Core.nDetritusGroups)
                        Me(iRow, iCol + 1) = New PropertyCell(prop)
                        alProp.Add(prop)
                    Next

                    opSumAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alProp.ToArray)
                    propSumAll = Me.Formula(opSumAll)
                    opMinus = New cBinaryOperation(cBinaryOperation.eOperatorType.Subtract, propSum, propSumAll)
                    propExport = Me.Formula(opMinus)

                    ' Export column 
                    Me(iRow, Me.ColumnsCount - 2) = New PropertyCell(propExport)

                    ' JS 140606: Use static single property here. Seems overkill where a simple Cell(1.0) would have
                    '            been sufficient, but this way the cell inherits StyleGuide colour and decimals feedback.
                    Me(iRow, Me.ColumnsCount - 1) = New PropertyCell(propSum)

                Else ' Group is stanza

                    sg = Core.StanzaGroups(group.iStanza)
                    ' Entering a new stanza group?
                    If (group.iStanza <> iStanzaPrev) Then
                        iRow = Me.AddRow()
                        hgcStanza = New EwEHierarchyGridCell()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
                        'Complete row with dummy cells
                        For j As Integer = 2 To Me.ColumnsCount - 1 : Me(iRow, j) = New EwERowHeaderCell() : Next
                        iStanzaPrev = group.iStanza
                        iRow = Me.AddRow()
                    Else
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If

                    'Display group info
                    hgcStanza.AddChildRow(iRow)
                    For iCol As Integer = 1 To Core.nDetritusGroups

                        groupSec = Core.EcoPathGroupInputs(Core.nGroups - Core.nDetritusGroups + iCol)

                        Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                        Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, group, eVarNameFlags.Name)
                        prop = Me.PropertyManager.GetProperty(group, eVarNameFlags.DetritusFate, groupSec, True, Core.nGroups - Core.nDetritusGroups)
                        Me(iRow, iCol + 1) = New PropertyCell(prop)
                        alProp.Add(prop)
                    Next

                    opSumAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alProp.ToArray)
                    propSumAll = Me.Formula(opSumAll)
                    opMinus = New cBinaryOperation(cBinaryOperation.eOperatorType.Subtract, propSum, propSumAll)
                    propExport = Me.Formula(opMinus)

                    ' Export column 
                    Me(iRow, Me.ColumnsCount - 2) = New PropertyCell(propExport)
                    Me(iRow, Me.ColumnsCount - 1) = New PropertyCell(propSum)
                End If
            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
