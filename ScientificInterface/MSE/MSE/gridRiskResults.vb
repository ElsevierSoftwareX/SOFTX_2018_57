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
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2.Cells.Real

#End Region

<CLSCompliant(False)> _
Public Class gridRiskResults
    : Inherits EwEGrid

    Public Enum eGridType As Integer
        Group = 0
        Fleet
    End Enum

    Private m_type As eGridType

    Public Sub New()
        Me.m_type = eGridType.Group
    End Sub

    Public Property GridType() As eGridType
        Get
            Return Me.m_type
        End Get
        Set(ByVal value As eGridType)
            Me.m_type = value
            Me.Update()
        End Set
    End Property

    Public Overloads Sub Update()
        MyBase.Update()
        If Me.UIContext Is Nothing Then Return
        Try
            Me.InitStyle()
            Me.FillData()
        Catch ex As Exception

        End Try
    End Sub

    Protected Overrides Sub InitStyle()

        MyBase.InitStyle()
        Me.Redim(1, 16)

        If Me.m_type = eGridType.Group Then
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUP)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_MEAN)
            Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_MIN)
            Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_MAX)
            Me(0, 5) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_CV)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_STDDEV)
            Me(0, 7) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_PERC_BELOW_REF)
            Me(0, 8) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_PERC_ABOVE_REF)

            Me(0, 9) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_MEAN)
            Me(0, 10) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_MIN)
            Me(0, 11) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_MAX)
            Me(0, 12) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_CV)
            Me(0, 13) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_STDDEV)
            Me(0, 14) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_PERC_BELOW_REF)
            Me(0, 15) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_PERC_ABOVE_REF)

        ElseIf Me.m_type = eGridType.Fleet Then
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_MEAN)
            Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_MIN)
            Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_MAX)
            Me(0, 5) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_CV)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_STDDEV)

            Me(0, 7) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_PERC_BELOW_REF)
            Me(0, 8) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_PERC_ABOVE_REF)

            Me(0, 9) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_MEAN)
            Me(0, 10) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_MIN)
            Me(0, 11) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_MAX)
            Me(0, 12) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_CV)
            Me(0, 13) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_STDDEV)
            Me(0, 14) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_PERC_BELOW_REF)
            Me(0, 15) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFORT_PERC_ABOVE_REF)

        End If

    End Sub

    Protected Overrides Sub FillData()
        Try
            'Why no property cells?????
            'PropertyCell() requires a ValueDescriptor property which cMSEStats objects can not populate
            'so we can not use PropertyCells with a cMSEStat object.
            'Grid cells need to be populated by hand
            Dim mse As cMSEManager = Me.Core.MSEManager
            If mse Is Nothing Then Exit Sub

            Dim lstData1 As cCoreInputOutputList(Of cCoreInputOutputBase) = Nothing
            Dim lstData2 As cCoreInputOutputList(Of cCoreInputOutputBase) = Nothing
            Dim lSources As New List(Of cCoreInputOutputBase)

            If Me.m_type = eGridType.Group Then
                lstData1 = mse.BiomassStats
                lstData2 = mse.GroupCatchStats
                For i As Integer = 1 To Me.UIContext.Core.nGroups
                    lSources.Add(Me.UIContext.Core.EcoPathGroupInputs(i))
                Next
            ElseIf Me.m_type = eGridType.Fleet Then
                lstData1 = mse.FleetStats
                lstData2 = mse.EffortStats
                For i As Integer = 1 To Me.UIContext.Core.nFleets
                    lSources.Add(Me.UIContext.Core.EcopathFleetInputs(i))
                Next
            End If

            Debug.Assert(lstData1 IsNot Nothing Or lstData2 IsNot Nothing, Me.ToString & ".FillData() Failed to find MSEStats object for " & Me.m_type.ToString)

            'WARNING if you add data or changed the column orders 
            'then you MUST change cell styles in InitCells() 
            Me.InitCells(lstData1.Count, lSources.ToArray())

            For Each grp As cMSEStats In lstData1
                Me.SetCellValue(grp.Index, 2, grp.Mean)
                Me.SetCellValue(grp.Index, 3, grp.Min)
                Me.SetCellValue(grp.Index, 4, grp.Max)
                Me.SetCellValue(grp.Index, 5, grp.CV)
                Me.SetCellValue(grp.Index, 6, grp.Std)
                Me.SetCellValue(grp.Index, 7, grp.BelowLimit)
                Me.SetCellValue(grp.Index, 8, grp.AboveLimit)
            Next

            For Each grp As cMSEStats In lstData2
                Me.SetCellValue(grp.Index, 9, grp.Mean)
                Me.SetCellValue(grp.Index, 10, grp.Min)
                Me.SetCellValue(grp.Index, 11, grp.Max)
                Me.SetCellValue(grp.Index, 12, grp.CV)
                Me.SetCellValue(grp.Index, 13, grp.Std)
                Me.SetCellValue(grp.Index, 14, grp.BelowLimit)
                Me.SetCellValue(grp.Index, 15, grp.AboveLimit)
            Next

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub

    Private Sub InitCells(ByVal iRow As Integer, ByVal aSources As cCoreInputOutputBase())

        Dim cell As EwECell = Nothing
        Dim cnt As Integer = Me.RowsCount '- 1

        For iIndex As Integer = cnt To iRow
            'Insert a new row
            Me.Rows.Insert(iIndex)

            Me(iIndex, 0) = New EwERowHeaderCell(CStr(iIndex))
            Me(iIndex, 1) = New PropertyRowHeaderCell(Me.PropertyManager, aSources(iIndex - 1), eVarNameFlags.Name)

            'not the best way to do this 
            'set the Style of the cell base on its column index not its contents
            For columnIndex As Integer = 2 To Me.ColumnsCount - 1

                cell = New EwECell(0.0!, GetType(Single))
                cell.Style = cStyleGuide.eStyleFlags.NotEditable

                'set the cell to Null if there is no catch or discards for this group
                If Me.m_type = eGridType.Group And columnIndex > 8 Then
                    Dim noCatch As Boolean = True
                    For iflt As Integer = 1 To Me.Core.nFleets
                        If Me.Core.EcopathFleetInputs(iflt).Landings(iIndex) + Me.Core.EcopathFleetInputs(iflt).Discards(iIndex) > 0 Then
                            noCatch = False
                            Exit For
                        End If
                    Next
                    If noCatch Then
                        'no catch so set the style to NotEditable Null
                        cell.Style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
                    End If
                End If

                Me(iIndex, columnIndex) = cell

            Next
        Next

    End Sub

    Private Sub SetCellValue(ByVal iRow As Integer, ByVal iCol As Integer, ByVal sValue As String)
        Try
            Me(iRow, iCol).Value = sValue
        Catch ex As Exception
            'do nothing??
        End Try
    End Sub

    Private Sub SetCellValue(ByVal iRow As Integer, ByVal iCol As Integer, ByVal sValue As Single)
        Try
            Me(iRow, iCol).Value = sValue
        Catch ex As Exception
            'do nothing??
        End Try
    End Sub


End Class
