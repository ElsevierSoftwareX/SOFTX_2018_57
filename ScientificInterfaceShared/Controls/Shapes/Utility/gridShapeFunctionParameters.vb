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
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Grid to edit the varying number of parameters of a <see cref="IShapeFunction">shape function</see>.
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public Class gridShapeFunctionParameters
    Inherits EwEGrid

    Private Enum eColumnTypes As Integer
        Name = 0
        Value
        Units
    End Enum

    Private m_shapefunction As IShapeFunction = Nothing
    Private m_bIsFreehand As Boolean = False

    Public Property ShapeFunction As IShapeFunction
        Get
            Return Me.m_shapefunction
        End Get
        Set(value As IShapeFunction)

            If (Me.m_shapefunction IsNot Nothing) Then
                ' Cleanup
                Me.RowsCount = 1
                Me.m_bIsFreehand = False
            End If

            Me.m_shapefunction = value

            If (Me.m_shapefunction IsNot Nothing) Then
                ' Set new
                Me.m_bIsFreehand = TypeOf (Me.m_shapefunction) Is cFreehandShapeFunction
                Me.RowsCount = If(Me.m_bIsFreehand, DirectCast(Me.m_shapefunction, cFreehandShapeFunction).nPoints, Me.m_shapefunction.nParameters) + 1
                Me.FillData()
            End If

        End Set
    End Property

    Protected Overrides Sub InitLayout()

        MyBase.InitLayout()

        Me.Redim(1, 3)

        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(My.Resources.HEADER_PARAMETER)
        Me(0, eColumnTypes.Value) = New EwEColumnHeaderCell(My.Resources.HEADER_VALUE)
        Me(0, eColumnTypes.Units) = New EwEColumnHeaderCell(My.Resources.HEADER_UNITS)

        Me.Selection.SelectionMode = SourceGrid2.GridSelectionMode.Row
        Me.Selection.EnableMultiSelection = False

        Me.AllowBlockSelect = False
        Me.FixedColumns = 1

    End Sub

    Protected Overrides Sub FillData()

        If (Me.m_bIsFreehand) Then
            Dim fh As cFreehandShapeFunction = DirectCast(Me.m_shapefunction, cFreehandShapeFunction)
            For iRow As Integer = 1 To Me.RowsCount - 1
                Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(CStr(iRow))
                Me(iRow, eColumnTypes.Value) = New EwECell(fh.ShapeData(iRow))
                Me(iRow, eColumnTypes.Value).Behaviors.Add(Me.EwEEditHandler)
                Me(iRow, eColumnTypes.Units) = New EwECell("", cStyleGuide.eStyleFlags.NotEditable)
            Next iRow
        Else
            For iRow As Integer = 1 To Me.RowsCount - 1
                Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(Me.m_shapefunction.ParamName(iRow))
                Me(iRow, eColumnTypes.Value) = New EwECell(Me.m_shapefunction.ParamValue(iRow), CType(Me.m_shapefunction.ParamStatus(iRow), Style.cStyleGuide.eStyleFlags))
                Me(iRow, eColumnTypes.Value).Behaviors.Add(Me.EwEEditHandler)
                Me(iRow, eColumnTypes.Units) = New EwECell(Me.m_shapefunction.ParamUnit(iRow), cStyleGuide.eStyleFlags.NotEditable)
            Next iRow
        End If

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.AutoStretchColumnsToFitWidth = True
        Me.FixedColumnWidths = False
    End Sub

    Public Event OnShapeFunctionChanged()

    Protected Overrides Function OnCellValueChanged(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Select Case DirectCast(p.Column, eColumnTypes)
            Case eColumnTypes.Value
                Dim iParam As Integer = p.Row
                Dim sValue As Single = CSng(cell.GetValue(p))
                If (Me.m_bIsFreehand) Then
                    Dim fh As cFreehandShapeFunction = DirectCast(Me.m_shapefunction, cFreehandShapeFunction)
                    fh.ShapeData(iParam) = sValue
                Else
                    Me.m_shapefunction.ParamValue(iParam) = sValue
                End If
                RaiseEvent OnShapeFunctionChanged()
        End Select

        Return True

    End Function

    Public Overloads Sub Update()
        MyBase.Update()
        Me.UpdateValues()
    End Sub


    Private Sub UpdateValues()
        Try
            If (Me.m_bIsFreehand) Then
                Dim fh As cFreehandShapeFunction = DirectCast(Me.m_shapefunction, cFreehandShapeFunction)
                For iRow As Integer = 1 To Me.RowsCount - 1
                    Me(iRow, eColumnTypes.Value).Value = fh.ShapeData(iRow)
                Next iRow
            Else
                For iRow As Integer = 1 To Me.RowsCount - 1
                    Me(iRow, eColumnTypes.Value).Value = Me.m_shapefunction.ParamValue(iRow)
                Next iRow
            End If
        Catch ex As Exception

        End Try
    End Sub

End Class
