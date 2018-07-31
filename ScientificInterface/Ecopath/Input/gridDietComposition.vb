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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core
Imports SourceGrid2.Cells.Real

#End Region

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Diet user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridDietComposition
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

            ' Define grid dimensions
            Me.Redim(Core.nGroups + 4, 2)

            Dim rowCnt As Integer = Me.RowsCount
            ' Set header cells
            ' # (0,0)
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_PREYPREDATOR)

            Dim columnIndex As Integer = 2

            For i As Integer = 1 To Core.nGroups
                source = Core.EcoPathGroupInputs(i)
                ' Group index header cell
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                ' # Group name row header cells
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                If source.PP < 1 Then
                    Me.Columns.Insert(columnIndex)
                    ' # Group name column header cells
                    Me(0, columnIndex) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
                    columnIndex = columnIndex + 1
                End If
            Next

            ' # DietImport header cell
            Me(rowCnt - 3, 0) = New EwERowHeaderCell()
            Me(rowCnt - 3, 1) = New EwERowHeaderCell(SharedResources.HEADER_IMPORT)

            ' # Sum header cell
            Me(rowCnt - 2, 0) = New EwERowHeaderCell()
            Me(rowCnt - 2, 1) = New EwERowHeaderCell(SharedResources.HEADER_SUM)

            ' # Sum - 1 header cell
            Me(rowCnt - 1, 0) = New EwERowHeaderCell()
            Me(rowCnt - 1, 1) = New EwERowHeaderCell(SharedResources.HEADER_1_MINUS_SUM)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreGroupBase = Nothing
            Dim sourceSec As cCoreGroupBase = Nothing
            Dim pm As cPropertyManager = Me.PropertyManager
            Dim prop As cProperty = Nothing
            Dim propSum As cFormulaProperty = Nothing
            Dim prop1MinusSum As cFormulaProperty = Nothing
            Dim opSumAll As cMultiOperation = Nothing
            Dim op1MinusSumAll As cBinaryOperation = Nothing
            Dim alPropSumAll As New ArrayList()
            Dim cell As EwECellBase = Nothing

            Dim visDiagonal As New SourceGrid2.VisualModels.Common
            visDiagonal.BackColor = Color.LightGray
            visDiagonal.TextAlignment = ContentAlignment.MiddleCenter

            ' Populate grid data cells
            Dim iCol As Integer = 2
            Dim nRows As Integer = Me.RowsCount
            ' For each column
            For iGroup As Integer = 1 To Core.nLivingGroups
                ' Get the group
                source = Core.EcoPathGroupInputs(iGroup)

                If source.PP < 1 Then

                    ' Prepare for collection new range of properties to sum
                    alPropSumAll.Clear()

                    ' For each row
                    For iRow As Integer = 1 To Core.nGroups
                        ' Get index group
                        sourceSec = Core.EcoPathGroupInputs(iRow)

                        ' Get the indexed dietcomp property
                        prop = pm.GetProperty(source, eVarNameFlags.DietComp, sourceSec)
                        ' Add property to destined cell
                        cell = New PropertyCell(prop)

                        ' Fixes issue #845
                        If iRow = source.Index Then
                            cell.VisualModel = visDiagonal
                        End If

                        ' DC value cells suppress zeroes to increase legibility of the grid
                        cell.SuppressZero = True
                        ' Activate the cell
                        Me(iRow, iCol) = cell
                        ' Add this property to the list of props to sum
                        alPropSumAll.Add(prop)

                    Next iRow

                    ' Define DietImport cell
                    ' # Get the property
                    prop = pm.GetProperty(source, eVarNameFlags.ImpDiet)
                    ' # Add to cell
                    Me(nRows - 3, iCol) = New PropertyCell(prop)
                    ' Add this property to the list of props to sum
                    alPropSumAll.Add(prop)

                    ' Now create the formula property that will calculate the sum of all DietComp props
                    opSumAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alPropSumAll.ToArray())
                    ' Create Sum property for the SUM row (all the way at the bottom)
                    propSum = Me.Formula(opSumAll)
                    ' Define sum cell
                    Me(nRows - 2, iCol) = New PropertyCell(propSum)

                    ' Create 1-Sum property for the SUM row (all the way at the bottom)
                    op1MinusSumAll = New cBinaryOperation(cBinaryOperation.eOperatorType.Subtract, 1, propSum)
                    prop1MinusSum = Me.Formula(op1MinusSumAll)
                    ' Define sum cell
                    Me(nRows - 1, iCol) = New PropertyCell(prop1MinusSum)

                    ' Next column
                    iCol += 1

                End If

            Next iGroup

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

        Public Overrides Sub OnCoreMessage(ByRef msg As EwECore.cMessage)
            ' Repopulate grid when ecopath group PP values have changed
            If (msg.Source = eCoreComponentType.EcoPath) And _
               (msg.DataType = eDataTypes.EcoPathGroupInput) And _
               (msg.Type = eMessageType.DataValidation) Then
                If msg.HasVariable(eVarNameFlags.PP) Then
                    Me.FillData()
                End If
            End If
        End Sub

    End Class

End Namespace
