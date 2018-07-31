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
    Public Class gridEcosimResultsGroup
        : Inherits gridResultsBase

        Private m_iFleetSelected As Integer
        Private m_iNumVisibleGroups As Integer

        Public Sub New()
            MyBase.New()

            Me.m_iNumVisibleGroups = 0

        End Sub

        Public Property SelectedFleetIndex() As Integer
            Get
                Return Me.m_iFleetSelected
            End Get
            Set(ByVal value As Integer)
                Me.m_iFleetSelected = value
                Me.UpdateData()
            End Set
        End Property

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Define column headers
            Me.Redim(1, 11)
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
            'Value (Start)
            Me(0, 8) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUESTART)
            'Value (End)
            Me(0, 9) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUEEND)
            'Value (E/S)
            Me(0, 10) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUEES)

        End Sub

        Protected Overrides Sub FillData()

            Dim lName As New List(Of String)
            lName.Add(String.Empty)

            ' Indices of calculated columns
            Dim aCalc() As Integer = {4, 7, 10}

            Me.m_iNumVisibleGroups = 0
            For iGroup As Integer = 1 To core.nGroups
                If Me.StyleGuide.GroupVisible(iGroup) Then
                    lName.Add(Core.EcoSimGroupOutputs(iGroup).Name)
                    Me.m_iNumVisibleGroups += 1
                End If
            Next

            Me.InitCells(Me.m_iNumVisibleGroups + 1, lName.ToArray, aCalc)

            Me.UpdateData()

        End Sub

        Friend Sub UpdateData()

            Dim sg As cStyleGuide = Me.StyleGuide
            Dim source As cEcosimGroupOutput = Nothing
            Dim iRow As Integer = 0

            Dim asTotal(0 To 10) As Single
            Me.InitTotalArray(asTotal)

            For iGroup As Integer = 1 To Me.UIContext.Core.nGroups

                'Only display selected groups
                If sg.GroupVisible(iGroup) Then

                    iRow += 1
                    source = Me.UIContext.Core.EcoSimGroupOutputs(iGroup)

                    'clear all fleet cells
                    For icell As Integer = 5 To 10
                        SetCellValue(iRow, icell, "")
                    Next

                    If source.BiomassStart > 0 Then SetCellValue(iRow, 2, source.BiomassStart, asTotal)
                    If source.BiomassEnd > 0 Then SetCellValue(iRow, 3, source.BiomassEnd, asTotal)

                    'The logic was pulled out from EwE5
                    If source.BiomassStart > 0 And source.BiomassEnd > 0 Then
                        SetCellValue(iRow, 4, CSng(source.BiomassEnd / source.BiomassStart), asTotal)
                    End If

                    Dim fCS As Single = source.CatchStart(Me.SelectedFleetIndex)
                    If fCS > 0 Then SetCellValue(iRow, 5, fCS, asTotal)

                    Dim fCE As Single = source.CatchEnd(Me.SelectedFleetIndex)
                    If fCE > 0 Then SetCellValue(iRow, 6, fCE, asTotal)

                    If fCS > 0 And fCE > 0 Then
                        SetCellValue(iRow, 7, CSng(fCE / fCS), asTotal)
                    End If

                    Dim fVS As Single = source.ValueStart(Me.SelectedFleetIndex)
                    If fVS > 0 Then SetCellValue(iRow, 8, fVS, asTotal)

                    Dim fVE As Single = source.ValueEnd(Me.SelectedFleetIndex)
                    If fVE > 0 Then SetCellValue(iRow, 9, fVE, asTotal)

                    If fVS > 0 And fVE > 0 Then
                        SetCellValue(iRow, 10, CSng(fVE / fVS), asTotal)
                    End If

                End If

            Next

            'Display total values
            For columnIndex As Integer = 2 To Me.ColumnsCount - 1
                If columnIndex = 4 Or columnIndex = 7 Or columnIndex = 10 Then
                    If asTotal(columnIndex - 2) > 0 And asTotal(columnIndex - 1) > 0 Then
                        Me(Me.RowsCount - 1, columnIndex).Value = asTotal(columnIndex - 1) / asTotal(columnIndex - 2)
                    End If
                Else
                    If asTotal(columnIndex) > 0 Then
                        Me(Me.RowsCount - 1, columnIndex).Value = asTotal(columnIndex)
                    End If
                End If
            Next

            Me.Refresh()
        End Sub

    End Class

End Namespace
