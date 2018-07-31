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

Namespace Ecospace

    <CLSCompliant(False)> _
    Public Class cGridEcospaceResultsRegion
        : Inherits gridResultsBase

        Private m_SelRegionIndex As Integer

        Public Property SelRegionIndex() As Integer
            Get
                Return m_SelRegionIndex
            End Get
            Set(ByVal value As Integer)
                m_SelRegionIndex = value
                Me.UpdateData()
            End Set
        End Property

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            'Define column header
            Me.Redim(1, 8)
            Me(0, 0) = New EwEColumnHeaderCell("")
            'Group name
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            'Biomass (Start)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_START)
            'Biomass (End)
            Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASSEND)
            'Biomass (E/S)
            Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASSES)
            'Catch (Start)
            Me(0, 5) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_START)
            'Catch (End)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_END)
            'Catch (E/S)
            Me(0, 7) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_E_OVER_S)

            Me.FixedColumnWidths = True

        End Sub

        Protected Overrides Sub FillData()

            If (Me.UIContext Is Nothing) Then Return

            Dim core As cCore = Me.Core

            Dim aName(core.nGroups) As String
            For i As Integer = 1 To core.nGroups
                aName(i) = core.EcospaceGroupOutput(i).Name
            Next

            Dim aCalc() As Integer = {4, 7}

            Me.InitCells(core.nGroups + 1, aName, aCalc)

            ReDim aName(core.nFleets)
            For i As Integer = 1 To core.nFleets
                aName(i) = core.EcospaceFleetOutput(i).Name
            Next

            Me.InitCells(core.nFleets + 1, aName, aCalc)

            Me.UpdateData()

        End Sub

        Private Sub UpdateData()

            If (Me.UIContext Is Nothing) Then Return

            Dim core As cCore = Me.Core
            Try

                Dim source As cEcospaceRegionOutput = core.EcospaceRegionOutput(Me.SelRegionIndex)

                'The array for storing total values
                Dim totalValue(0 To 7) As Single
                Me.InitTotalArray(totalValue)

                'Add group output
                For groupIndex As Integer = 1 To core.nGroups

                    Dim gBS As Single = source.BiomassStart(groupIndex)
                    Dim gBE As Single = source.BiomassEnd(groupIndex)

                    SetCellValue(groupIndex, 2, gBS, totalValue)
                    SetCellValue(groupIndex, 3, gBE, totalValue)
                    If gBS > 0 And gBE > 0 Then
                        SetCellValue(groupIndex, 4, CSng(gBE / gBS), totalValue)
                    End If

                    'sum of catch by group
                    Dim sCatch As Single = 0, eCatch As Single = 0
                    For iflt As Integer = 1 To core.nFleets
                        sCatch += source.CatchFleetGroupStart(iflt, groupIndex)
                        eCatch += source.CatchFleetGroupEnd(iflt, groupIndex)
                    Next iflt
                    SetCellValue(groupIndex, 5, sCatch)
                    SetCellValue(groupIndex, 6, eCatch)
                    If sCatch > 0 And eCatch > 0 Then
                        SetCellValue(groupIndex, 7, CSng(eCatch / sCatch))
                    End If

                Next groupIndex

                Dim rowIndex As Integer = core.nGroups + 1

                For columnIndex As Integer = 2 To 4
                    Me(rowIndex, columnIndex).Value = totalValue(columnIndex)
                Next

                'Add fleet output
                Dim cntGroups As Integer = core.nGroups + 1

                For fleetIndex As Integer = 1 To core.nFleets
                    Dim sum1 As Single = 0
                    Dim sum2 As Single = 0
                    For groupIndex As Integer = 1 To core.nGroups
                        Dim fgCS As Single = source.CatchFleetGroupStart(fleetIndex, groupIndex)
                        Dim fgCE As Single = source.CatchFleetGroupEnd(fleetIndex, groupIndex)
                        If fgCS >= 0 Then sum1 += fgCS
                        If fgCE >= 0 Then sum2 += fgCE
                    Next

                    rowIndex = cntGroups + fleetIndex
                    SetCellValue(rowIndex, 5, sum1, totalValue)
                    SetCellValue(rowIndex, 6, sum2, totalValue)

                    If sum1 > 0 And sum2 > 0 Then
                        SetCellValue(rowIndex, 7, CSng(sum2 / sum1), totalValue)
                    End If
                Next

                'The row for total value - Fleet
                For columnIndex As Integer = 5 To Me.ColumnsCount - 1
                    Me(Me.RowsCount - 1, columnIndex).Value = totalValue(columnIndex)
                Next
            Catch ex As Exception
                Debug.Assert(False, "Error in " & Me.ToString & ".UpdateData() " & ex.Message)
            End Try


        End Sub

    End Class

End Namespace
