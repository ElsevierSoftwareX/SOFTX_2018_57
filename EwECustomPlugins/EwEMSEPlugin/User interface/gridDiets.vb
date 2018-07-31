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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Grid to change diet multipliers.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridDiets
    Inherits EwEGrid

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        PredIndex = 0
        PredName
        Multiplier
        [Imports]
    End Enum

#End Region ' Internal defs

    Private m_data As cDiets = Nothing

#Region " Constructor "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Sub Init(data As cDiets)
        Me.m_data = data
        Me.RefreshContent()
    End Sub

    Public Event onEdited()

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        ' ToDo: globalize this

        If (Me.m_data Is Nothing) Then Return

        Dim nFixedCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length
        Dim iNumCols As Integer = nFixedCols + Me.m_data.Core.nGroups
        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.PredIndex) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.PredName) = New EwEColumnHeaderCell("Predator")
        Me(0, eColumnTypes.Multiplier) = New EwEColumnHeaderCell("Multiplier")
        Me(0, eColumnTypes.Imports) = New EwEColumnHeaderCell(SharedResources.HEADER_IMPORT)

        ' Headers
        For i As Integer = 1 To Me.m_data.Core.nGroups
            Me(0, i + nFixedCols - 1) = New EwEColumnHeaderCell(CStr(i))
        Next

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False
        Me.AllowBlockSelect = False

    End Sub

    Protected Overrides Sub FillData()

        If (Me.m_data Is Nothing) Then Return

        Dim iRow As Integer = -1

        Me.RowsCount = 1
        Dim nFixedCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

        For i As Integer = 1 To Me.m_data.Core.nLivingGroups
            iRow = Me.AddRow()
            Me.Rows(iRow).Tag = i

            Dim grp As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
            Me(iRow, eColumnTypes.PredIndex) = New EwERowHeaderCell(CStr(grp.Index))
            Me(iRow, eColumnTypes.PredName) = New EwERowHeaderCell(CStr(grp.Name))
            Me(iRow, eColumnTypes.Multiplier) = Me.DataCell(Me.m_data.DietPropMultipliers(i - 1))
            Me(iRow, eColumnTypes.Imports) = Me.DataCell(Me.m_data.InteractsImports(i - 1), cStyleGuide.eStyleFlags.NotEditable)

            For j As Integer = 1 To Me.m_data.Core.nGroups
                Dim val As Single = Me.m_data.MeanProportions(i - 1, j - 1)
                Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.NotEditable
                If (val = 0) Then style = style Or cStyleGuide.eStyleFlags.Null
                Me(iRow, nFixedCols + j - 1) = Me.DataCell(val, style)
            Next
        Next

        Me.Columns(eColumnTypes.PredIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.None
        Me.Columns(eColumnTypes.PredName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.Multiplier).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.AutoSizeColumn(eColumnTypes.Multiplier, 150)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.NotSet
        End Get
    End Property

    Private Function DataCell(dValue As Double, Optional style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK) As EwECell

        Dim cell As EwECell = Nothing

        If (dValue = cCore.NULL_VALUE) Then
            style = style Or cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
        End If

        cell = New EwECell(CSng(dValue), GetType(Single), style)
        cell.Behaviors.Add(Me.EwEEditHandler)
        Return cell

    End Function

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        If (Me.m_data Is Nothing) Then Return False
        If (Not MyBase.OnCellEdited(p, cell)) Then Return False

        ' Check column
        If (p.Column = eColumnTypes.Multiplier) Then

            ' Store value
            Try
                Dim iGroup As Integer = CInt(Me.Rows(p.Row).Tag)
                Me.m_data.DietPropMultipliers(iGroup - 1) = Convert.ToSingle(cell.GetValue(p))
            Catch ex As Exception

            End Try
        End If

        ' Yippee
        Me.RaiseDataChangeEvent()

        ' Done
        Return True

    End Function

    Private Sub RaiseDataChangeEvent()
        Try
            RaiseEvent onEdited()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Overrides

End Class


