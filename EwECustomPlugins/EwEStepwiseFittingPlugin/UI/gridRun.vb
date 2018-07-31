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
'    UBC Centre for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
'
' Stepwise Fitting Procedure by Sheila Heymans, Erin Scott, Jeroen Steenbeek
' Copyright 2015- Scottish Association for Marine Science, Oban, Scotland
'
' Erin Scott was funded by the Scottish Informatics and Computer Science
' Alliance (SICSA) Postgraduate Industry Internship Programme.
' ===============================================================================
'
#Region " Imports "

Option Strict On
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style.cStyleGuide
Imports EwECore

#End Region ' Imports

Public Class gridRun
    Inherits EwEGrid

    Private m_manager As cSFPManager = Nothing

    ''' <summary>
    ''' Enumerated type, defining the columns to display in this grid
    ''' </summary>
    ''' <remarks>
    ''' To reorder of columns just change the order of the enumerated values
    ''' </remarks>
    Private Enum eColumnTypes As Integer
        Index = 0
        Name
        Enabled
        K
        EstimatedV
        SplinePoints
        SS
        AIC
        AICc
        State
    End Enum

    Public Sub Initialize(ByVal manager As cSFPManager)
        Me.m_manager = manager
        Me.RefreshContent()
    End Sub

    ''' <summary>
    ''' Refresh the iterations currently displayed in the grid 
    ''' </summary>
    Public Sub UpdateContent()

        For iRow As Integer = 1 To Me.RowsCount - 1
            Me.UpdateIterationRow(iRow)
        Next

        ' Re-fire selection event
        Me.RaiseSelectionChangeEvent()

    End Sub

    Public ReadOnly Property SelectedIteration As ISFPIterations
        Get
            Dim iRow As Integer = Me.SelectedRow
            If (iRow < 1) Then Return Nothing
            Return CType(Me.Rows(iRow).Tag, ISFPIterations)
        End Get
    End Property

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

        ' ToDo: globalize this
        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell("Name")
        Me(0, eColumnTypes.Enabled) = New EwEColumnHeaderCell("Enabled")
        Me(0, eColumnTypes.K) = New EwEColumnHeaderCell("K")
        Me(0, eColumnTypes.EstimatedV) = New EwEColumnHeaderCell("# Vs")
        Me(0, eColumnTypes.SplinePoints) = New EwEColumnHeaderCell("# spline")
        Me(0, eColumnTypes.SS) = New EwEColumnHeaderCell("SS")
        Me(0, eColumnTypes.AIC) = New EwEColumnHeaderCell("AIC")
        Me(0, eColumnTypes.AICc) = New EwEColumnHeaderCell("AICc")
        Me(0, eColumnTypes.State) = New EwEColumnHeaderCell("State")

        Me.AllowBlockSelect = False
        Me.FixedColumnWidths = False
        Me.Selection.SelectionMode = SourceGrid2.GridSelectionMode.Row

    End Sub

    Protected Overrides Sub FillData()

        ' Sanity checks
        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_manager Is Nothing) Then Return

        Me.RowsCount = 1

        Dim iRow As Integer = 0
        Dim iterations As ISFPIterations() = Me.m_manager.Iterations
        Dim iteration As ISFPIterations = Nothing
        Dim cell As EwECellBase = Nothing

        For i As Integer = 0 To iterations.Length - 1

            iteration = iterations(i)
            iRow = Me.AddRow()

            Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(i + 1))
            Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(iteration.Name)

            Me(iRow, eColumnTypes.Enabled) = New EwECheckboxCell(False)
            Me(iRow, eColumnTypes.Enabled).Behaviors.Add(Me.EwEEditHandler)

            Me(iRow, eColumnTypes.EstimatedV) = New EwECell(cCore.NULL_VALUE, GetType(Integer), eStyleFlags.NotEditable)
            Me(iRow, eColumnTypes.SplinePoints) = New EwECell(cCore.NULL_VALUE, GetType(Integer), eStyleFlags.NotEditable)
            Me(iRow, eColumnTypes.K) = New EwECell(cCore.NULL_VALUE, GetType(Integer), eStyleFlags.NotEditable)

            cell = New EwECell(cCore.NULL_VALUE, GetType(Single), eStyleFlags.NotEditable)
            cell.SuppressZero(0) = True
            Me(iRow, eColumnTypes.SS) = cell

            cell = New EwECell(cCore.NULL_VALUE, GetType(Single), eStyleFlags.NotEditable)
            cell.SuppressZero(0) = True
            Me(iRow, eColumnTypes.AIC) = cell

            cell = New EwECell(cCore.NULL_VALUE, GetType(Single), eStyleFlags.NotEditable)
            cell.SuppressZero(0) = True
            Me(iRow, eColumnTypes.AICc) = cell

            cell = New EwECell("", GetType(String), eStyleFlags.NotEditable)
            Me(iRow, eColumnTypes.State) = cell

            Me.Rows(iRow).Tag = iteration

            Me.UpdateIterationRow(iRow)
        Next

    End Sub

    Private Sub UpdateIterationRow(iRow As Integer)

        Dim iteration As ISFPIterations = CType(Me.Rows(iRow).Tag, ISFPIterations)
        Dim style As eStyleFlags = 0

        If (iteration.IsBestFit) Then style = eStyleFlags.Checked

        Me(iRow, eColumnTypes.Enabled).Value = iteration.Enabled
        DirectCast(Me(iRow, eColumnTypes.Enabled), IEwECell).Style = style

        Me(iRow, eColumnTypes.K).Value = iteration.K
        DirectCast(Me(iRow, eColumnTypes.K), IEwECell).Style = style Or eStyleFlags.NotEditable

        Me(iRow, eColumnTypes.EstimatedV).Value = iteration.EstimatedV
        DirectCast(Me(iRow, eColumnTypes.EstimatedV), IEwECell).Style = style Or eStyleFlags.NotEditable

        Me(iRow, eColumnTypes.SplinePoints).Value = iteration.SplinePoints
        DirectCast(Me(iRow, eColumnTypes.SplinePoints), IEwECell).Style = style Or eStyleFlags.NotEditable

        Me(iRow, eColumnTypes.SS).Value = iteration.SS
        DirectCast(Me(iRow, eColumnTypes.SS), IEwECell).Style = style Or eStyleFlags.NotEditable

        Me(iRow, eColumnTypes.AIC).Value = iteration.AIC
        DirectCast(Me(iRow, eColumnTypes.AIC), IEwECell).Style = style Or eStyleFlags.NotEditable

        Me(iRow, eColumnTypes.AICc).Value = iteration.AICc
        DirectCast(Me(iRow, eColumnTypes.AICc), IEwECell).Style = style Or eStyleFlags.NotEditable

        Me(iRow, eColumnTypes.State).Value = Me.State(iteration)
        DirectCast(Me(iRow, eColumnTypes.State), IEwECell).Style = style Or eStyleFlags.NotEditable

    End Sub

    Protected Overrides Function OnCellValueChanged(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Select Case DirectCast(p.Column, eColumnTypes)
            Case eColumnTypes.Enabled

                Dim iteration As ISFPIterations = CType(Me.Rows(p.Row).Tag, ISFPIterations)
                iteration.Enabled = CBool(cell.GetValue(p))

                ' Cheat!
                Me.RaiseSelectionChangeEvent()

        End Select
        Return MyBase.OnCellValueChanged(p, cell)

    End Function

    Protected Function State(iteration As ISFPIterations) As String

        ' ToDo: globalize this
        Select Case iteration.RunState
            Case ISFPIterations.eRunState.Idle
                Return ""
            Case ISFPIterations.eRunState.Completed
                Return "OK"
            Case ISFPIterations.eRunState.Error
                Return "error"
            Case ISFPIterations.eRunState.Running
                Return "Running"
        End Select
        Return "?"

    End Function

#End Region ' Overrides

End Class
