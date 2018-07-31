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
Imports SourceGrid2.Cells.Real
Imports EwECore.Style

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)>
    Public Class gridFisheryOutputCatch
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            Dim source As cCoreInputOutputBase = Nothing

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            'Define grid dimensions
            Me.Redim(1, Core.nFleets + 3)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            ' Dynamic column header - fleet name
            For fleetIndex As Integer = 1 To Core.nFleets
                source = Core.EcopathFleetInputs(fleetIndex)
                Me(0, fleetIndex + 1) = New PropertyColumnHeaderCell(Me.PropertyManager,
                                                                     source, eVarNameFlags.Name, Nothing,
                                                                     cUnits.CurrencyOverTime)
            Next

            ' Total catch column
            Me(0, Core.nFleets + 2) = New EwEColumnHeaderCell(eVarNameFlags.TotalCatch)

            Me.FixedColumns = 2
        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumnWidths = True
        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreInputOutputBase = Nothing
            Dim iRow As Integer = -1

            'Remove existing rows
            Me.RowsCount = 1

            ' Done?
            If Core.nFleets = 0 Then Return

            ' Create rows for all groups and sum quantities in each row
            For iGroup As Integer = 1 To Core.nGroups
                iRow = Me.AddRow()
                FillRows(iRow, iGroup)
            Next iGroup

            ' Create "Total catch" row (sum values in each column)
            FillTotalCatchRow()

            ' Create "Trophic level" row
            FillTrophicLevelRow()

        End Sub

        Private Sub FillRows(ByVal iRow As Integer, ByVal iGroup As Integer)

            Dim cell As EwECell = Nothing
            Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
            Dim sGroupTotal As Single = 0

            Me(iRow, 0) = New EwERowHeaderCell(CStr(iGroup))
            Me(iRow, 1) = New EwERowHeaderCell(group.Name)

            ' For each fleet (each column) 
            For iFleet As Integer = 1 To Core.nFleets

                Dim fleet As cEcopathFleetOutput = Me.Core.EcoPathFleetOutputs(iFleet)
                Dim sLanding As Single = fleet.LandingsByGroup(iGroup)
                Dim sDeadDiscards As Single = fleet.DiscardMortByGroup(iGroup)

                cell = New EwECell(sLanding + sDeadDiscards, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.ValueComputed)
                cell.SuppressZero = True
                Me(iRow, iFleet + 1) = cell

                sGroupTotal += sLanding + sDeadDiscards
            Next

            Me(iRow, Me.ColumnsCount - 1) = New EwECell(sGroupTotal, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Sum)

        End Sub

        Private Sub FillTotalCatchRow()

            Dim iRow As Integer = Me.AddRow()
            Dim sTot As Single = 0

            Me(Me.RowsCount - 1, 0) = New EwERowHeaderCell()
            Me(Me.RowsCount - 1, 1) = New EwERowHeaderCell(SharedResources.HEADER_TOTALCATCH)

            For iFleet As Integer = 1 To Core.nFleets

                Dim sFleetTot As Single = 0
                Dim fleet As cEcopathFleetOutput = Core.EcoPathFleetOutputs(iFleet)

                For iGroup As Integer = 1 To Core.nGroups
                    sFleetTot += fleet.LandingsByGroup(iGroup) + fleet.DiscardMortByGroup(iGroup)
                Next
                Me(Me.RowsCount - 1, iFleet + 1) = New EwECell(sFleetTot, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Sum)
                sTot += sFleetTot
            Next
            Me(Me.RowsCount - 1, Me.ColumnsCount - 1) = New EwECell(sTot, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Sum)

        End Sub

        Private Sub FillTrophicLevelRow()

            ' Let's rewrite this when in a good mood to get rid of the property math. It's not needed for an output grid that does not refresh
            Dim iRow As Integer
            Dim sourceGrpIntput As cCoreInputOutputBase = Nothing
            Dim sourceGrpIntputSec As cCoreInputOutputBase = Nothing
            Dim sourceGrpOutput As cCoreInputOutputBase = Nothing
            Dim propLandings As cProperty = Nothing
            Dim propDiscards As cProperty = Nothing
            Dim propTTLX As cProperty = Nothing

            Dim alSumLandingsDiscards As ArrayList = New ArrayList()
            Dim opSumLandingsDiscards As cMultiOperation = Nothing
            Dim propSumLandingsDiscards As cFormulaProperty = Nothing

            Dim alProdQuantityTTLX As ArrayList = New ArrayList()
            Dim opProdQuantityTTLX As cMultiOperation = Nothing
            Dim propProdQuantityTTLX As cFormulaProperty = Nothing

            Dim alSumQuantityCol As New ArrayList()
            Dim opSumQuantityCol As cMultiOperation = Nothing
            Dim propSumQuantityCol As cFormulaProperty = Nothing

            Dim alSumQuantityTTLXCol As New ArrayList()
            Dim opSumQuantityTTLXCol As cMultiOperation = Nothing
            Dim propSumQuantityTTLXCol As cFormulaProperty = Nothing

            Dim alSumQuantityAll As New ArrayList()
            Dim opSumQuantityAll As cMultiOperation = Nothing
            Dim propSumQuantityAll As cFormulaProperty = Nothing

            Dim alSumQuantityTTLXAll As New ArrayList()
            Dim opSumQuantityTTLXAll As cMultiOperation = Nothing
            Dim propSumQuantityTTLXAll As cFormulaProperty = Nothing

            Dim opDivTTLXQuantity As cBinaryOperation = Nothing
            Dim propDivTTLXQuantity As cFormulaProperty = Nothing

            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell("")
            Me(iRow, 1) = New EwERowHeaderCell(eVarNameFlags.TTLX)

            alSumQuantityAll.Clear()
            alSumQuantityTTLXAll.Clear()
            For fleetIndex As Integer = 1 To Core.nFleets
                sourceGrpIntput = Core.EcopathFleetInputs(fleetIndex)
                alSumQuantityCol.Clear()
                alSumQuantityTTLXCol.Clear()

                For rowIndex As Integer = 1 To Core.nGroups
                    sourceGrpIntputSec = Core.EcoPathGroupInputs(rowIndex)
                    sourceGrpOutput = Core.EcoPathGroupOutputs(rowIndex)
                    alSumLandingsDiscards.Clear()
                    alProdQuantityTTLX.Clear()
                    ' Get the index landing property
                    propLandings = Me.PropertyManager.GetProperty(sourceGrpIntput, eVarNameFlags.Landings, sourceGrpIntputSec)
                    alSumLandingsDiscards.Add(propLandings)
                    ' Get the index discard property
                    propDiscards = Me.PropertyManager.GetProperty(sourceGrpIntput, eVarNameFlags.Discards, sourceGrpIntputSec)
                    alSumLandingsDiscards.Add(propDiscards)
                    ' Get the index TTLX property
                    propTTLX = Me.PropertyManager.GetProperty(sourceGrpOutput, eVarNameFlags.TTLX)
                    'propCell = New PropertyCell(CType(propTTLX, cProperty))
                    'MsgBox("TTLX" & CStr(propCell.Value), MsgBoxStyle.Information)
                    alProdQuantityTTLX.Add(propTTLX)
                    ' Set the property 
                    opSumLandingsDiscards = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumLandingsDiscards.ToArray())
                    propSumLandingsDiscards = Me.Formula(opSumLandingsDiscards)

                    alProdQuantityTTLX.Add(propSumLandingsDiscards)
                    opProdQuantityTTLX = New cMultiOperation(cMultiOperation.eOperatorType.Multiply, alProdQuantityTTLX.ToArray())
                    propProdQuantityTTLX = Me.Formula(opProdQuantityTTLX)

                    'Sum quantity in a column
                    alSumQuantityCol.Add(propSumLandingsDiscards)
                    'Sum quantity*TTLX in a column
                    alSumQuantityTTLXCol.Add(propProdQuantityTTLX)

                    'Sum all quantity
                    alSumQuantityAll.Add(propSumLandingsDiscards)
                    'Sum all quantity*TTLX
                    alSumQuantityTTLXAll.Add(propProdQuantityTTLX)
                Next

                'Display (sum of quantity*TTLX in a column) / (sum of quantity in a column)
                opSumQuantityCol = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumQuantityCol.ToArray())
                propSumQuantityCol = Me.Formula(opSumQuantityCol)
                opSumQuantityTTLXCol = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumQuantityTTLXCol.ToArray())
                propSumQuantityTTLXCol = Me.Formula(opSumQuantityTTLXCol)
                opDivTTLXQuantity = New cBinaryOperation(cBinaryOperation.eOperatorType.Divide, propSumQuantityTTLXCol, propSumQuantityCol)
                propDivTTLXQuantity = Me.Formula(opDivTTLXQuantity)
                Me(Me.RowsCount - 1, fleetIndex + 1) = New PropertyCell(propDivTTLXQuantity)
            Next

            'Display (sum of all quantity*TTLX) / (sum of all quantity)
            opSumQuantityAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumQuantityAll.ToArray())
            propSumQuantityAll = Me.Formula(opSumQuantityAll)
            opSumQuantityTTLXAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumQuantityTTLXAll.ToArray())
            propSumQuantityTTLXAll = Me.Formula(opSumQuantityTTLXAll)
            opDivTTLXQuantity = New cBinaryOperation(cBinaryOperation.eOperatorType.Divide, propSumQuantityTTLXAll, propSumQuantityAll)
            propDivTTLXQuantity = Me.Formula(opDivTTLXQuantity)
            Me(Me.RowsCount - 1, Me.ColumnsCount - 1) = New PropertyCell(propDivTTLXQuantity)

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
