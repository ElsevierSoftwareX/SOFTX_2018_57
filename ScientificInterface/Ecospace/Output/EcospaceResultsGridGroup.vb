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

    ''' =======================================================================
    ''' <summary>
    ''' Grid, reflects Ecospace results per group.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class cGridEcospaceResultsGroup
        : Inherits gridResultsBase

        Private m_iFleetSelected As Integer
        Private m_iNumVisibleGroups As Integer

        Public Sub New()
            MyBase.New()
            Me.m_iNumVisibleGroups = 0
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                Me.UIContext = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub

        Public Property SelFleetIndex() As Integer
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

            Me.FixedColumnWidths = True

        End Sub

        Protected Overrides Sub FillData()

            If Me.UIContext Is Nothing Then Return

            'This method init the cells, its visual and data models. 
            Dim core As cCore = Me.Core
            Dim aCalc() As Integer = {4, 7, 10}

            Dim lName As New List(Of String)
            lName.Add(String.Empty)

            Me.m_iNumVisibleGroups = 0
            For iGroup As Integer = 1 To core.nGroups
                'If Me.StyleGuide.GroupVisible(iGroup) Then
                lName.Add(core.EcospaceGroupOutput(iGroup).Name)
                m_iNumVisibleGroups += 1
                ' End If

            Next

            Me.InitCells(m_iNumVisibleGroups + 1, lName.ToArray, aCalc)

            Me.UpdateData()

        End Sub

        Private Sub UpdateData()

            If Me.UIContext Is Nothing Then Return

            Dim core As cCore = Me.UIContext.Core
            Dim source As cEcospaceGroupOutput = Nothing
            Dim irow As Integer
            Dim totalValue(0 To 10) As Single
            Me.InitTotalArray(totalValue)

            For iGroup As Integer = 1 To core.nGroups

                'Only display selected groups
                'If Me.StyleGuide.GroupVisible(iGroup) Then
                irow += 1
                source = core.EcospaceGroupOutput(iGroup)

                SetCellValue(irow, 2, source.BiomassStart, totalValue)
                SetCellValue(irow, 3, source.BiomassEnd, totalValue)

                'The logic was pulled out from EwE5
                If source.BiomassStart > 0 And source.BiomassEnd > 0 Then
                    SetCellValue(irow, 4, CSng(source.BiomassEnd / source.BiomassStart), totalValue)
                End If

                Dim fCS As Single = source.CatchStart(Me.SelFleetIndex)
                SetCellValue(irow, 5, fCS, totalValue)

                Dim fCE As Single = source.CatchEnd(Me.SelFleetIndex)
                SetCellValue(irow, 6, fCE, totalValue)

                If fCS > 0 And fCE > 0 Then
                    SetCellValue(irow, 7, CSng(fCE / fCS), totalValue)
                End If

                Dim fVS As Single = source.ValueStart(Me.SelFleetIndex)
                SetCellValue(irow, 8, fVS, totalValue)

                Dim fVE As Single = source.ValueEnd(Me.SelFleetIndex)
                SetCellValue(irow, 9, fVE, totalValue)

                If fVS > 0 And fVE > 0 Then
                    SetCellValue(irow, 10, CSng(fVE / fVS), totalValue)
                End If

                'End If

            Next

            'Display total values
            For columnIndex As Integer = 2 To Me.ColumnsCount - 1
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

        End Sub

#Region " Events "

        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            If (ct And cStyleGuide.eChangeType.GroupVisibility) > 0 Then
                Me.RefreshContent()
            End If
        End Sub

#End Region ' Events

    End Class

End Namespace
