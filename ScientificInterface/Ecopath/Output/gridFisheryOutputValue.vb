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
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Properties
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2.Cells.Real
Imports EwECore.Style

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)>
    Public Class gridFisheryOutputValue
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            Dim source As cCoreInputOutputBase = Nothing

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            MyBase.InitStyle()

            'Define grid dimensions
            Me.Redim(1, Core.nFleets + 5)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            ' Dynamic column header - fleet name
            For fleetIndex As Integer = 1 To Core.nFleets
                source = Core.EcopathFleetInputs(fleetIndex)
                Me(0, fleetIndex + 1) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Next

            ' Catch value column
            Me(0, Core.nFleets + 2) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCHVALUE, cUnits.MonetaryOverTime)
            Me(0, Core.nFleets + 3) = New EwEColumnHeaderCell(SharedResources.HEADER_NONMARKETVALUE, cUnits.MonetaryOverTime)
            Me(0, Core.nFleets + 4) = New EwEColumnHeaderCell(SharedResources.HEADER_TOTALVALUE, cUnits.MonetaryOverTime)

            Me.FixedColumns = 2
            Me.FixedColumnWidths = True
        End Sub

        Protected Overrides Sub FillData()

            Dim source As cEcoPathGroupInput = Nothing
            Dim iRow As Integer = -1

            ' Remove existing rows
            Me.RowsCount = 1

            ' Done?
            If Core.nFleets = 0 Then Return

            ' Create rows for all groups and sum values in each row
            For rowIndex As Integer = 1 To Core.nGroups
                source = Core.EcoPathGroupInputs(rowIndex)
                iRow = Me.AddRow()
                FillRows(iRow, source)
            Next rowIndex

            'Create "Total value" row (sum values in each column)
            FillTotalValueRow()

            'Create "Total cost" row
            FillTotalCostRow()

            'Create "Total profit" row
            FillTotalProfitRow()

        End Sub

        Private Sub FillRows(ByVal iRow As Integer, ByVal source As cEcoPathGroupInput)

            Dim sourceSec As cCoreInputOutputBase = Nothing
            Dim propLandings As cProperty = Nothing

            ' Single marketprice property
            Dim propMarketPrice As cProperty = Nothing
            Dim alProdLandingsMarketPrice As New ArrayList()
            Dim opProdLandingsMarketPrice As cMultiOperation = Nothing
            Dim propProdLandingsMarketPrice As cFormulaProperty = Nothing

            ' Operation to sum landings non-market price
            Dim alNonMarketValue As New ArrayList()
            Dim opNonMarketValue As cBinaryOperation = Nothing
            Dim propProdNonMarketValue As cProperty = Nothing

            Dim propCell As PropertyCell = Nothing
            Dim alSumRow As ArrayList = New ArrayList()
            Dim opSumMarketValues As cMultiOperation = Nothing
            Dim propSumMarketValues As cFormulaProperty = Nothing

            ' Total value
            Dim opTotalValue As cBinaryOperation = Nothing
            Dim propTotalValue As cFormulaProperty = Nothing

            Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
            Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)

            alSumRow.Clear()
            ' For each fleet (each column) 
            For iFleet As Integer = 1 To Core.nFleets
                alProdLandingsMarketPrice.Clear()
                ' Get the fleet object 
                sourceSec = Core.EcopathFleetInputs(iFleet)
                ' Get the index landing property
                propLandings = Me.PropertyManager.GetProperty(sourceSec, eVarNameFlags.Landings, source)
                alProdLandingsMarketPrice.Add(propLandings)
                ' Get the index market price property
                propMarketPrice = Me.PropertyManager.GetProperty(sourceSec, eVarNameFlags.OffVesselPrice, source)
                alProdLandingsMarketPrice.Add(propMarketPrice)
                ' Set the property to the cell
                opProdLandingsMarketPrice = New cMultiOperation(cMultiOperation.eOperatorType.Multiply, alProdLandingsMarketPrice.ToArray())
                propProdLandingsMarketPrice = Me.Formula(opProdLandingsMarketPrice)
                propCell = New PropertyCell(propProdLandingsMarketPrice)
                ' Configure the cell
                propCell.SuppressZero = True
                propCell.Style = cStyleGuide.eStyleFlags.Sum Or cStyleGuide.eStyleFlags.NotEditable
                ' Set the cell
                Me(iRow, iFleet + 1) = propCell

                'Sum values in a row
                alSumRow.Add(propProdLandingsMarketPrice)

            Next

            'Display the sum of market values
            opSumMarketValues = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumRow.ToArray())
            propSumMarketValues = Me.Formula(opSumMarketValues)
            propCell = New PropertyCell(propSumMarketValues)
            Me(iRow, Me.ColumnsCount - 3) = propCell

            ' Non-market value
            ' .. multiply group non-market value by calculated broup biomass
            opNonMarketValue = New cBinaryOperation(cBinaryOperation.eOperatorType.Multiply, _
                Me.PropertyManager.GetProperty(source, eVarNameFlags.NonMarketValue), _
                Me.PropertyManager.GetProperty(Core.EcoPathGroupOutputs(source.Index), eVarNameFlags.Biomass))
            propProdNonMarketValue = Me.Formula(opNonMarketValue)
            propCell = New PropertyCell(CType(propProdNonMarketValue, cProperty))
            propCell.SuppressZero = True
            Me(iRow, Me.ColumnsCount - 2) = propCell

            ' Total value
            opTotalValue = New cBinaryOperation(cBinaryOperation.eOperatorType.Add, propSumMarketValues, propProdNonMarketValue)
            propTotalValue = Me.Formula(opTotalValue)
            propCell = New PropertyCell(propTotalValue)
            Me(iRow, Me.ColumnsCount - 1) = propCell

        End Sub

        Private Sub FillTotalValueRow()

            Dim iRow As Integer
            Dim source As cCoreInputOutputBase = Nothing
            Dim sourceSec As cCoreInputOutputBase = Nothing

            Dim propLandings As cProperty = Nothing
            Dim propMarketPrice As cProperty = Nothing
            Dim alProdLandingsMarketPrice As ArrayList = New ArrayList()
            Dim opProdLandingsMarketPrice As cMultiOperation = Nothing
            Dim propProdLandingsMarketPrice As cFormulaProperty = Nothing

            Dim alSumCol As New ArrayList()
            Dim opSumCol As cMultiOperation = Nothing
            Dim propSumCol As cFormulaProperty = Nothing

            Dim alSumAll As New ArrayList()
            Dim opSumAll As cMultiOperation = Nothing
            Dim propSumAll As cFormulaProperty = Nothing

            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell("")
            Me(iRow, 1) = New EwERowHeaderCell(SharedResources.HEADER_TOTALVALUE, cUnits.Monetary)

            alSumAll.Clear()
            For fleetIndex As Integer = 1 To Core.nFleets
                source = Core.EcopathFleetInputs(fleetIndex)
                alSumCol.Clear()

                For rowIndex As Integer = 1 To Core.nGroups
                    sourceSec = Core.EcoPathGroupInputs(rowIndex)
                    alProdLandingsMarketPrice.Clear()
                    ' Get the index landing property
                    propLandings = Me.PropertyManager.GetProperty(source, eVarNameFlags.Landings, sourceSec)
                    alProdLandingsMarketPrice.Add(propLandings)
                    ' Get the index market price property
                    propMarketPrice = Me.PropertyManager.GetProperty(source, eVarNameFlags.OffVesselPrice, sourceSec)
                    alProdLandingsMarketPrice.Add(propMarketPrice)
                    ' Set the property 
                    opProdLandingsMarketPrice = New cMultiOperation(cMultiOperation.eOperatorType.Multiply, alProdLandingsMarketPrice.ToArray())
                    propProdLandingsMarketPrice = Me.Formula(opProdLandingsMarketPrice)

                    'Sum values in a column
                    alSumCol.Add(propProdLandingsMarketPrice)

                    'Sum all values
                    alSumAll.Add(propProdLandingsMarketPrice)
                Next

                'Display the sum of values in a column
                opSumCol = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumCol.ToArray())
                propSumCol = Me.Formula(opSumCol)
                Me(Me.RowsCount - 1, fleetIndex + 1) = New PropertyCell(CType(propSumCol, cProperty))
            Next

            'Display the sum of all values
            opSumAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumAll.ToArray())
            propSumAll = Me.Formula(opSumAll)
            Me(Me.RowsCount - 1, Me.ColumnsCount - 3) = New PropertyCell(CType(propSumAll, cProperty))

        End Sub

        Private Sub FillTotalCostRow()

            Dim iRow As Integer
            Dim source As cCoreInputOutputBase = Nothing
            Dim propFixedCost As cProperty = Nothing
            Dim propCPUECost As cProperty = Nothing
            Dim propSailCost As cProperty = Nothing

            Dim alSumFixedCPUESailCost As New ArrayList()
            Dim opSumFixedCPUESailCost As cMultiOperation = Nothing
            Dim propSumFixedCPUESailCost As cFormulaProperty = Nothing

            Dim alProdCostValue As New ArrayList()
            Dim opProdCostValue As cMultiOperation = Nothing
            Dim propProdCostValue As cFormulaProperty = Nothing

            Dim alSumCost As New ArrayList
            Dim opSumCost As cMultiOperation = Nothing
            Dim propSumCost As cFormulaProperty = Nothing

            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell("")
            Me(iRow, 1) = New EwERowHeaderCell(SharedResources.HEADER_TOTALCOST, cUnits.Monetary)

            alSumCost.Clear()
            For fleetIndex As Integer = 1 To Core.nFleets

                ' Clear the arrayList for the new row
                alSumFixedCPUESailCost.Clear()

                source = Core.EcopathFleetInputs(fleetIndex)

                'Fixed cost 
                propFixedCost = Me.PropertyManager.GetProperty(source, eVarNameFlags.FixedCost)
                alSumFixedCPUESailCost.Add(propFixedCost)

                'Effort related cost
                propCPUECost = Me.PropertyManager.GetProperty(source, eVarNameFlags.CPUECost)
                alSumFixedCPUESailCost.Add(propCPUECost)

                'Sailing related cost
                propSailCost = Me.PropertyManager.GetProperty(source, eVarNameFlags.SailCost)
                alSumFixedCPUESailCost.Add(propSailCost)

                'Total cost
                opSumFixedCPUESailCost = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumFixedCPUESailCost.ToArray())
                propSumFixedCPUESailCost = Me.Formula(opSumFixedCPUESailCost)

                alProdCostValue.Clear()
                alProdCostValue.Add(propSumFixedCPUESailCost)
                alProdCostValue.Add(0.01)
                alProdCostValue.Add(Me(Me.RowsCount - 2, fleetIndex + 1)) 'total value
                opProdCostValue = New cMultiOperation(cMultiOperation.eOperatorType.Multiply, alProdCostValue.ToArray()) 'total cost as a percent of total value
                propProdCostValue = Me.Formula(opProdCostValue)
                Me(Me.RowsCount - 1, fleetIndex + 1) = New PropertyCell(propProdCostValue)

                alSumCost.Add(propProdCostValue)
            Next

            opSumCost = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumCost.ToArray())
            propSumCost = Me.Formula(opSumCost)
            Me(Me.RowsCount - 1, Me.ColumnsCount - 3) = New PropertyCell(propSumCost)
        End Sub

        Private Sub FillTotalProfitRow()

            Dim iRow As Integer
            Dim opMinusValueCost As cBinaryOperation = Nothing
            Dim propMinusValueCost As cFormulaProperty = Nothing
            Dim alSumProfit As New ArrayList()
            Dim opSumProfit As cMultiOperation = Nothing
            Dim propSumProfit As cFormulaProperty = Nothing

            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell("")
            Me(iRow, 1) = New EwERowHeaderCell(SharedResources.HEADER_TOTALPROFIT, cUnits.Monetary)

            alSumProfit.Clear()
            For fleetIndex As Integer = 1 To Core.nFleets

                opMinusValueCost = New cBinaryOperation(cBinaryOperation.eOperatorType.Subtract, _
                                                CType(Me(Me.RowsCount - 3, fleetIndex + 1), Object), _
                                                CType(Me(Me.RowsCount - 2, fleetIndex + 1), Object))  'total value - total cost
                propMinusValueCost = Me.Formula(opMinusValueCost)
                alSumProfit.Add(propMinusValueCost)
                Me(Me.RowsCount - 1, fleetIndex + 1) = New PropertyCell(CType(propMinusValueCost, cProperty))
            Next

            opSumProfit = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumProfit.ToArray())
            propSumProfit = Me.Formula(opSumProfit)
            Me(Me.RowsCount - 1, Me.ColumnsCount - 3) = New PropertyCell(propSumProfit)
        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
