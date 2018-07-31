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
Public Class gridStrategiesOverview
    Inherits EwEGrid

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        StrategyName
        RunStrategy
    End Enum

#End Region ' Internal defs

    Private m_data As Strategies = Nothing
    Private m_iUpdateLock As Integer = 0

#Region " Constructor "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Sub Init(data As Strategies)
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

        Me(0, eColumnTypes.StrategyName) = New EwEColumnHeaderCell(My.Resources.HEADER_NAME)
        Me(0, eColumnTypes.RunStrategy) = New EwEColumnHeaderCell(My.Resources.HEADER_RUN_YES_NO)
        'Me(0, eColumnTypes.LowerLimit) = New EwEColumnHeaderCell(My.Resources.HEADER_LOWER_LIMIT_VALID)
        'Me(0, eColumnTypes.UpperLimit) = New EwEColumnHeaderCell(My.Resources.HEADER_UPPER_LIMIT_VALID)

        'Me.FixedColumns = 2
        Me.FixedColumns = 1
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

            'Dim igroup As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
            'Me(iRow, eColumnTypes.GroupIndex) = New EwERowHeaderCell(CStr(igroup.Index))
            'Me(iRow, eColumnTypes.GroupName) = New EwERowHeaderCell(CStr(igroup.Name))

            Me(iRow, eColumnTypes.StrategyName) = New EwERowHeaderCell(CStr(Me.m_data(i - 1).Name))
            'Me(iRow, eColumnTypes.StrategyName) = PropertyRowHeaderCell(Me.PropertyManager, fleetMSE, eVarNameFlags.Name)

            Me(iRow, eColumnTypes.RunStrategy) = New SourceGrid2.Cells.Real.CheckBox(Me.m_data(i - 1).RunThisStrategy)
            Me(iRow, eColumnTypes.RunStrategy).Behaviors.Add(Me.EwEEditHandler)

            Me.Rows(iRow).Tag = m_data(i-1)

            Me.UpdateRow(iRow)

            'Me(iRow, eColumnTypes.GroupName) = New EwERowHeaderCell(CStr(Me.m_data(i - 1).mGroup.Name))
            'Me(iRow, eColumnTypes.RunStrategy) = Me.DataCell(CSng(Me.m_data(i - 1).mLowerLimit))
            'Me(iRow, eColumnTypes.UpperLimit) = Me.DataCell(CSng(Me.m_data(i - 1).mUpperLimit))

            ' No need to use tags here: row number = fleet number
            ' Me.Rows(iRow).Tag = i

            '====================================================================================================
            ''Get the flt info
            'fleetMSE = Core.MSEManager.FleetInputs(iFleet)

            'Me.AddRow()

            'Me(iFleet, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iFleet))
            'Me(iFleet, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, fleetMSE, eVarNameFlags.Name)

            'Me(iFleet, eColumnTypes.OptionNotUsed) = New SourceGrid2.Cells.Real.CheckBox(True)
            'Me(iFleet, eColumnTypes.OptionNotUsed).Behaviors.Add(Me.EwEEditHandler)

            'Me(iFleet, eColumnTypes.OptionWeakest) = New SourceGrid2.Cells.Real.CheckBox(False)
            'Me(iFleet, eColumnTypes.OptionWeakest).Behaviors.Add(Me.EwEEditHandler)

            'Me(iFleet, eColumnTypes.OptionStrongest) = New SourceGrid2.Cells.Real.CheckBox(False)
            'Me(iFleet, eColumnTypes.OptionStrongest).Behaviors.Add(Me.EwEEditHandler)

            'Me(iFleet, eColumnTypes.OptionSelective) = New SourceGrid2.Cells.Real.CheckBox(False)
            'Me(iFleet, eColumnTypes.OptionSelective).Behaviors.Add(Me.EwEEditHandler)

            'Me.Rows(iFleet).Tag = fleetMSE

            'Me.UpdateRow(iFleet)

        Next

        'Me.Columns(eColumnTypes.GroupIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        'Me.Columns(eColumnTypes.GroupIndex)
        Me.Columns(eColumnTypes.StrategyName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.RunStrategy).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.AutoSizeColumn(eColumnTypes.StrategyName, 150)
        'Me.Columns(eColumnTypes.LowerLimit).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.AutoSizeColumn(eColumnTypes.RunStrategy, 150)
        'Me.Columns(eColumnTypes.UpperLimit).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        'Me.AutoSizeColumn(eColumnTypes.UpperLimit, 150)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.NotSet
        End Get
    End Property

    'Private Function DataCell(dValue As Single) As EwECell

    '    Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
    '    Dim cell As EwECell = Nothing

    '    If (dValue = cCore.NULL_VALUE) Then
    '        style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
    '    End If

    '    cell = New EwECell(CSng(dValue), GetType(Single), style)
    '    cell.Behaviors.Add(Me.EwEEditHandler)
    '    Return cell

    'End Function

    'Private Function DataCell(dValue As CheckBoxStatus) As EwECell

    '    Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
    '    Dim cell As EwECell = Nothing

    '    'If (dValue.Is Nothing) Then
    '    '    style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
    '    'End If

    '    cell = New EwECell(dValue, GetType(chec), style)
    '    cell.Behaviors.Add(Me.EwEEditHandler)
    '    Return cell

    'End Function

    'Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

    '    If (Me.m_data Is Nothing) Then Return False
    '    If (Not MyBase.OnCellEdited(p, cell)) Then Return False

    '    ' Check column
    '    If (p.Column = eColumnTypes.RunStrategy) Then
    '        ' Store value
    '        MessageBox.Show("We need to write the code that executes in gridStrategiesOverview.OnCellEdited")
    '        'Me.m_data(p.Row).RunThisStrategy = Convert.ToSingle(cell.GetValue(p))
    '    End If

    '    ' Yippee
    '    Me.RaiseDataChangeEvent()

    '    ' Done
    '    Return True

    'End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Called Update local admin based on cell value changes.
    ''' </summary>
    ''' <returns>
    ''' True if the value change is allowed, False to block the value change.
    ''' </returns>
    ''' <remarks>
    ''' This method differs from OnCellValueEdited; during a cell value 
    ''' change notification (at the end of an edit operation) it is unsafe
    ''' to modify the value of the cell being edited. However, the end edit 
    ''' event will not be triggered for particular specialized cells which
    ''' makes this method mandatory.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function OnCellValueChanged(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

        If Not Me.AllowUpdates Then Return True

        Dim strat As Strategy = Nothing
        Dim ri As RowInfo = Nothing

        ri = Me.Rows(p.Row)
        strat = DirectCast(ri.Tag, Strategy)


        If DirectCast(cell.GetValue(p), Boolean) = True Then
            strat.RunThisStrategy = False
        Else
            strat.RunThisStrategy = True
        End If

        Me.UpdateRow(p.Row)

        'If DirectCast(cell.GetValue(p), CheckState) = CheckState.Checked Then
        '    strat.RunThisStrategy = True
        'Else
        '    strat.RunThisStrategy = False
        'End If

        'Select Case DirectCast(p.Column, eColumnTypes)

        '    Case eColumnTypes.RunStrategy
        '        'strat.QuotaType = eQuotaTypes.NoControls
        '        strat.RunThisStrategy = True
        '        Me.UpdateRow(p.Row)

        '        'Case eColumnTypes.OptionSelective
        '        '    flt.QuotaType = eQuotaTypes.Selective
        '        '    Me.UpdateRow(p.Row)

        '        'Case eColumnTypes.OptionStrongest
        '        '    flt.QuotaType = eQuotaTypes.HighestValue
        '        '    Me.UpdateRow(p.Row)

        '        'Case eColumnTypes.OptionWeakest
        '        '    flt.QuotaType = eQuotaTypes.Weakest
        '        '    Me.UpdateRow(p.Row)

        'End Select

        Return True

    End Function

    Private Sub UpdateRow(ByVal iRow As Integer)

        Dim strat As Strategy = Nothing
        Dim ri As RowInfo = Nothing

        ri = Me.Rows(iRow)
        strat = DirectCast(ri.Tag, Strategy)

        Me.AllowUpdates = False

        ' Set option checks
        Me(iRow, eColumnTypes.RunStrategy).Value = (strat.RunThisStrategy)
        'Me(iRow, eColumnTypes.OptionWeakest).Value = (flt.QuotaType = eQuotaTypes.Weakest)
        'Me(iRow, eColumnTypes.OptionStrongest).Value = (flt.QuotaType = eQuotaTypes.HighestValue)
        'Me(iRow, eColumnTypes.OptionSelective).Value = (flt.QuotaType = eQuotaTypes.Selective)

        Me.AllowUpdates = True

    End Sub

    Public Sub CheckAll()
        For iRow = 1 To Me.RowsCount - 1
            Me(iRow, eColumnTypes.RunStrategy).Value = False
        Next
    End Sub

    Public Sub UncheckAll()
        For iRow = 1 To Me.RowsCount - 1
            Me(iRow, eColumnTypes.RunStrategy).Value = True
        Next
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update lock, should be set when modifying cell values from the code
    ''' to prevent recursive update/notification loops.
    ''' </summary>
    ''' <returns>True when no update lock is active.</returns>
    ''' <remarks>
    ''' Update locks are cumulative: setting this lock twice will require 
    ''' clearing it twice to allow updates to happen.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Property AllowUpdates() As Boolean
        Get
            Return (Me.m_iUpdateLock = 0)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                Me.m_iUpdateLock += 1
            Else
                Me.m_iUpdateLock -= 1
            End If
        End Set
    End Property

    'Public Property Data As Strategies
    '    Get
    '        Return Me.m_data
    '    End Get
    '    Set(value As Strategies)
    '        Me.m_data = value
    '        Me.FillData()
    '    End Set

    'End Property



    Private Sub RaiseDataChangeEvent()
        Try
            RaiseEvent onEdited()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Overrides

End Class


