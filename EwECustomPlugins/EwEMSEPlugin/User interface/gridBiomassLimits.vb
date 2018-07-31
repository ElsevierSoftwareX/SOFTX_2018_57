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
''' Grid to allow biomass limits to be specified for credible results.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)>
Public Class gridBiomassLimits
    Inherits EwEGrid

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        GroupIndex = 0
        GroupName
        LowerLimit
        UpperLimit
    End Enum

#End Region ' Internal defs

    Private m_data As cBiomassLimits = Nothing

#Region " Constructor "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Sub Init(data As cBiomassLimits)
        Me.m_data = data
        Me.FillData()
    End Sub

    Public Event onEdited()

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length
        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.GroupIndex) = New EwEColumnHeaderCell(My.Resources.HEADER_GROUP_INDEX)
        Me(0, eColumnTypes.GroupName) = New EwEColumnHeaderCell(My.Resources.HEADER_GROUP_NAME)
        Me(0, eColumnTypes.LowerLimit) = New EwEColumnHeaderCell(My.Resources.HEADER_LOWER_LIMIT_VALID)
        Me(0, eColumnTypes.UpperLimit) = New EwEColumnHeaderCell(My.Resources.HEADER_UPPER_LIMIT_VALID)

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False
        Me.AllowBlockSelect = False
        'Me.Columns(0).Width = 200

    End Sub

    Protected Overrides Sub FillData()

        If (Me.m_data Is Nothing) Then Return

        Dim iRow As Integer = -1
        Dim cell As EwECell = Nothing

        Me.RowsCount = 1

        For i As Integer = 1 To Me.m_data.Count
            iRow = Me.AddRow()

            Dim igroup As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
            'Me(iRow, eColumnTypes.GroupIndex) = New EwERowHeaderCell(CStr(igroup.Index))
            'Me(iRow, eColumnTypes.GroupName) = New EwERowHeaderCell(CStr(igroup.Name))

            Me(iRow, eColumnTypes.GroupIndex) = New EwERowHeaderCell(CStr(Me.m_data(i - 1).mGroup.Index))
            Me(iRow, eColumnTypes.GroupName) = New EwERowHeaderCell(CStr(Me.m_data(i - 1).mGroup.Name))
            Me(iRow, eColumnTypes.LowerLimit) = Me.DataCell(CSng(Me.m_data(i - 1).mLowerLimit))
            Me(iRow, eColumnTypes.UpperLimit) = Me.DataCell(CSng(Me.m_data(i - 1).mUpperLimit))

            ' No need to use tags here: row number = fleet number
            ' Me.Rows(iRow).Tag = i

        Next

        Me.Columns(eColumnTypes.GroupIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        'Me.Columns(eColumnTypes.GroupIndex)
        Me.Columns(eColumnTypes.GroupName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.LowerLimit).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.AutoSizeColumn(eColumnTypes.LowerLimit, 150)
        Me.Columns(eColumnTypes.UpperLimit).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.AutoSizeColumn(eColumnTypes.UpperLimit, 150)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.NotSet
        End Get
    End Property

    Private Function DataCell(dValue As Single) As EwECell

        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
        Dim cell As EwECell = Nothing

        If (dValue = cCore.NULL_VALUE) Then
            style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
        End If

        cell = New EwECell(CSng(dValue), GetType(Single), style)
        cell.Behaviors.Add(Me.EwEEditHandler)
        Return cell

    End Function

    Public Property Data As cBiomassLimits
        Get
            Return Me.m_data
        End Get
        Set(value As cBiomassLimits)
            Me.m_data = value
            Me.FillData()
        End Set

    End Property

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        If (Me.m_data Is Nothing) Then Return False
        If (Not MyBase.OnCellEdited(p, cell)) Then Return False

        ' Check column
        If (p.Column = eColumnTypes.LowerLimit) Then
            ' Store value
            Me.m_data(p.Row - 1).mLowerLimit = Convert.ToSingle(cell.GetValue(p))
        End If
        If (p.Column = eColumnTypes.UpperLimit) Then
            ' Store value
            Me.m_data(p.Row - 1).mUpperLimit = Convert.ToSingle(cell.GetValue(p))
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


