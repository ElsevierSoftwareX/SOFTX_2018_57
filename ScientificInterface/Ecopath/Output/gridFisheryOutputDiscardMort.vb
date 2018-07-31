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

Namespace Ecopath.Output

    <CLSCompliant(False)>
    Public Class gridFisheryOutputDiscardMort
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

            ' Total column
            Me(0, Core.nFleets + 2) = New EwEColumnHeaderCell(SharedResources.HEADER_DISCARD_MORT)

            Me.FixedColumns = 2
        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumnWidths = True
        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreInputOutputBase = Nothing
            Dim iRow As Integer = -1

            ' Remove existing rows
            Me.RowsCount = 1

            ' Done?
            If Core.nFleets = 0 Then Return

            ' Create rows for all groups and sum quantities in each row
            For iGroup As Integer = 1 To Core.nGroups
                iRow = Me.AddRow()
                FillRows(iRow, iGroup)
            Next iGroup

            ' Create column totals
            FillTotalsRow()

        End Sub

        Private Sub FillRows(ByVal iRow As Integer, ByVal iGroup As Integer)

            Dim group As cEcoPathGroupInput = Core.EcoPathGroupInputs(iGroup)
            Dim fleetOut As cEcopathFleetOutput = Nothing
            Dim sVal As Single = 0
            Dim sTot As Single = 0

            Me(iRow, 0) = New EwERowHeaderCell(CStr(iGroup))
            Me(iRow, 1) = New EwERowHeaderCell(group.Name)

            ' For each fleet (each column) 
            For iFleet As Integer = 1 To Core.nFleets
                ' Get the fleet object 
                fleetOut = Core.EcoPathFleetOutputs(iFleet)
                sVal = fleetOut.DiscardMortByGroup(iGroup)
                Dim cell As New EwECell(sVal, cStyleGuide.eStyleFlags.ValueComputed Or cStyleGuide.eStyleFlags.NotEditable)
                cell.SuppressZero = True
                ' Set the cell
                Me(iRow, iFleet + 1) = cell

                sTot += sVal
            Next

            ' Total column
            Me(iRow, Me.ColumnsCount - 1) = New EwECell(sTot, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Sum)

        End Sub

        Private Sub FillTotalsRow()

            Dim fleetOut As cEcopathFleetOutput = Nothing
            Dim iRow As Integer = Me.AddRow()
            Dim sTot As Single = 0

            Me(Me.RowsCount - 1, 0) = New EwERowHeaderCell()
            Me(Me.RowsCount - 1, 1) = New EwERowHeaderCell(SharedResources.HEADER_DISCARD_MORT)

            For iFleet As Integer = 1 To Core.nFleets
                Dim sFleetTot As Single = 0
                fleetOut = Core.EcoPathFleetOutputs(iFleet)

                For iGroup As Integer = 1 To Core.nGroups
                    sFleetTot += fleetOut.DiscardMortByGroup(iGroup)
                Next
                Me(Me.RowsCount - 1, iFleet + 1) = New EwECell(sFleetTot, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Sum)
                sTot += sFleetTot
            Next
            Me(Me.RowsCount - 1, Me.ColumnsCount - 1) = New EwECell(sTot, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Sum)

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
