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
Imports ScientificInterface.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class gridEcosimResultsFleet
        : Inherits gridResultsBase

        Private m_iNumVisibleFleets As Integer = 0

        Public Sub New()
            MyBase.new()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            'Define grid dimensions
            Me.Redim(1, 12)

            'Define column header
            Me(0, 0) = New EwEColumnHeaderCell("")
            'Gear name
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
            ' Catch (Start)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_START)
            ' Catch (End)
            Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_END)
            ' Catch (E/S)
            Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_E_OVER_S)
            ' Value (Start)
            Me(0, 5) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUESTART)
            ' Value (End)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUEEND)
            ' Value (E/S)
            Me(0, 7) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUEES)
            ' Cost (Start)
            Me(0, 8) = New EwEColumnHeaderCell(SharedResources.HEADER_COST_START)
            ' Cost (End)
            Me(0, 9) = New EwEColumnHeaderCell(SharedResources.HEADER_COST_END)
            ' Cost (E/S)
            Me(0, 10) = New EwEColumnHeaderCell(SharedResources.HEADER_COST_E_OVER_S)
            ' Effort (E/S)
            Me(0, 11) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORTES)

        End Sub

        'This method init the cells, its visual and data models. 
        Protected Overrides Sub FillData()

            Dim lName As New List(Of String)
            lName.Add(String.Empty)

            Me.m_iNumVisibleFleets = 0

            For iFleet As Integer = 1 To Core.nFleets
                If Me.StyleGuide.FleetVisible(iFleet) Then
                    lName.Add(Core.EcosimFleetInputs(iFleet).Name)
                    Me.m_iNumVisibleFleets += 1
                End If
            Next

            Dim aCalc() As Integer = {4, 7, 10}
            Me.InitCells(Me.m_iNumVisibleFleets + 1, lName.ToArray, aCalc)

            Me.updateData()

        End Sub

        Friend Sub updateData()

            Dim source As cEcosimFleetOutput = Nothing
            Dim ts As cTimeSeries = Nothing
            Dim styleCost As cStyleGuide.eStyleFlags = 0
            Dim iRow As Integer = 0

            Dim totalValue(0 To 11) As Single
            Me.InitTotalArray(totalValue)

            For iFleet As Integer = 1 To core.nFleets

                'Only display selected groups
                If Me.StyleGuide.FleetVisible(iFleet) Then

                    iRow += 1
                    source = Core.EcosimFleetOutput(iFleet)

                    If source.CatchStart > 0 Then SetCellValue(iRow, 2, source.CatchStart, totalValue)
                    If source.CatchEnd > 0 Then SetCellValue(iRow, 3, source.CatchEnd, totalValue)

                    If source.CatchStart > 0 And source.CatchEnd > 0 Then
                        SetCellValue(iRow, 4, CSng(source.CatchEnd / source.CatchStart), totalValue)
                    End If

                    If source.ValueStart > 0 Then SetCellValue(iRow, 5, source.ValueStart, totalValue)
                    If source.ValueEnd > 0 Then SetCellValue(iRow, 6, source.ValueEnd, totalValue)

                    If source.ValueStart > 0 And source.ValueEnd > 0 Then
                        SetCellValue(iRow, 7, CSng(source.ValueEnd / source.ValueStart), totalValue)
                    End If

                    If source.CostStart > 0 Then SetCellValue(iRow, 8, source.CostStart, totalValue, styleCost)
                    If source.CostEnd > 0 Then SetCellValue(iRow, 9, source.CostEnd, totalValue, styleCost)
                    If source.CostStart > 0 And source.CostEnd > 0 Then
                        SetCellValue(iRow, 10, CSng(source.CostEnd / source.CostStart), totalValue, styleCost)
                    End If

                    'jb feb??08 cEcosimFleetSummary.Effort is endEffort/StartEffort
                    SetCellValue(iRow, 11, CSng(source.Effort), totalValue)
                End If

            Next

            'Display total values
            ' Bug fix 413: will not sum last column
            For columnIndex As Integer = 2 To Me.ColumnsCount - 2
                ' Hmm, how about using constants here?
                If columnIndex = 4 Or columnIndex = 7 Or columnIndex = 10 Then
                    If totalValue(columnIndex - 2) > 0 And totalValue(columnIndex - 1) > 0 Then
                        Me(Me.RowsCount - 1, columnIndex).Value = totalValue(columnIndex - 1) / totalValue(columnIndex - 2)
                    End If
                Else
                    If totalValue(columnIndex) > 0 Then
                        Me(Me.RowsCount - 1, columnIndex).Value = totalValue(columnIndex)
                    End If
                End If
            Next

            Me.Refresh()

        End Sub

    End Class

End Namespace

