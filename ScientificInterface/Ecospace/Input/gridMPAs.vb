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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.Utilities
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core

#End Region

Namespace Ecospace

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Grid to configure the open/closed states of MPAs
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class gridMPAs
        : Inherits EwEGrid

        ''' <summary>A number representing the row that contains the first MPA</summary>
        Private Const iFIRSTMPAROW As Integer = 1
        ''' <summary>Enumerated type defining the columns in this grid.</summary>
        Private Enum eColumnTypes
            MPAIndex = 0
            MPAName
            MPAAll
            MPAJan
            MPAFeb
            MPAMar
            MPAApr
            MPAMay
            MPAJun
            MPAJul
            MPAAug
            MPASep
            MPAOct
            MPANov
            MPADec
        End Enum

        Private m_bInUpdate As Boolean = False
        Private WithEvents m_bpEffort As cBooleanProperty = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create the grid
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()

            MyBase.New()
            Me.FixedColumnWidths = False

        End Sub

#Region " Grid interaction "

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)
                If (Me.UIContext IsNot Nothing) Then
                    Me.m_bpEffort = Nothing
                End If

                If (value IsNot Nothing) Then
                    Dim ecospaceModelParams As cEcospaceModelParameters = value.Core.EcospaceModelParameters()
                    Dim propMan As cPropertyManager = value.PropertyManager
                    Me.m_bpEffort = DirectCast(propMan.GetProperty(ecospaceModelParams, eVarNameFlags.PredictEffort), cBooleanProperty)
                End If

                MyBase.UIContext = value

             End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Selection.SelectionMode = GridSelectionMode.Row
            Me.Selection.EnableMultiSelection = False
            Me.ContextMenu = Nothing

            ' Redim columns
            Me.Redim(1, System.Enum.GetValues(GetType(eColumnTypes)).Length)

            ' MPA index cell
            Me(0, eColumnTypes.MPAIndex) = New EwEColumnHeaderCell()
            ' MPA name cell, editable this time
            Me(0, eColumnTypes.MPAName) = New EwEColumnHeaderCell(SharedResources.HEADER_MPA)
            Me(0, eColumnTypes.MPAAll) = New EwEColumnHeaderCell(SharedResources.HEADER_MPA_CLOSED)
            'Define column header Jan - Dec
            For iCol As Integer = eColumnTypes.MPAJan To eColumnTypes.MPADec
                Dim d As New Date(1, (iCol - eColumnTypes.MPAJan) + 1, 1)
                Me(0, iCol) = New EwEColumnHeaderCell(d.ToString("MMM"))
            Next

            ' Fix index column only; MPA name column cannot be fixed because it must be editable
            Me.FixedColumns = 1

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to first create a snapshot of the MPA/stanza configuration
        ''' in the current EwE model. The grid will be populated from this local
        ''' administration.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FillData()

            If (Me.UIContext Is Nothing) Then Return

            Dim mpa As cEcospaceMPA = Nothing
            Dim ewec As EwECellBase = Nothing
            Dim bEnable As Boolean = (CBool(Me.m_bpEffort.GetValue()) = True)

            ' Create missing rows
            For iMPA As Integer = 1 To Me.Core.nMPAs

                mpa = Me.Core.EcospaceMPAs(iMPA)
                Me.AddRow()

                Me(iMPA, eColumnTypes.MPAIndex) = New PropertyRowHeaderCell(Me.PropertyManager, mpa, eVarNameFlags.Index)
                Me(iMPA, eColumnTypes.MPAName) = New PropertyRowHeaderCell(Me.PropertyManager, mpa, eVarNameFlags.Name)

                If (bEnable) Then
                    Me(iMPA, eColumnTypes.MPAAll) = New Cells.Real.CheckBox(False)
                    Me(iMPA, eColumnTypes.MPAAll).Behaviors.Add(Me.EwEEditHandler)

                    For iMonth As Integer = 1 To cCore.N_MONTHS
                        Me(iMPA, eColumnTypes.MPAJan - 1 + iMonth) = New Cells.Real.CheckBox(False)
                        Me(iMPA, eColumnTypes.MPAJan - 1 + iMonth).Behaviors.Add(Me.EwEEditHandler)
                    Next iMonth
                Else
                    Me(iMPA, eColumnTypes.MPAAll) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)

                    For iMonth As Integer = 1 To cCore.N_MONTHS
                        Me(iMPA, eColumnTypes.MPAJan - 1 + iMonth) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)
                    Next iMonth
                End If

            Next

            ' Populate rows
            If (bEnable) Then
                For iRow As Integer = 1 To Me.RowsCount - 1
                    UpdateRow(iRow)
                Next iRow
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Finish the style
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.AutoSizeColumnRange(1, Me.ColumnsCount - 1, 1, Me.RowsCount - 1)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the content of the Row with the given index.
        ''' </summary>
        ''' <param name="iRow">The index of the row to refresh.</param>
        ''' -----------------------------------------------------------------------
        Private Sub UpdateRow(ByVal iRow As Integer)

            Dim mpa As cEcospaceMPA = Me.Core.EcospaceMPAs(iRow)
            Dim aCells() As Cells.ICellVirtual = Me.Rows(iRow).GetCells()
            Dim pos As SourceGrid2.Position = Nothing
            Dim iNumOpen As Integer = 0

            Me.m_bInUpdate = True

            ' Set monthly states
            For iMonth As Integer = 1 To cCore.N_MONTHS
                pos = New Position(iRow, eColumnTypes.MPAJan - 1 + iMonth)
                ' Display a check when the MPA is NOT open for fishing
                aCells(eColumnTypes.MPAJan - 1 + iMonth).SetValue(pos, Not mpa.IsOpen(iMonth))
                If mpa.IsOpen(iMonth) Then iNumOpen += 1
            Next

            ' Display a check when the MPA is NOT open for fishing
            aCells(eColumnTypes.MPAAll).SetValue(pos, (iNumOpen = 0))

            Me.m_bInUpdate = False

        End Sub

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
        ''' makes this method mandatory. We once again apologize for the confusion; )
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function OnCellValueChanged(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

            If Me.m_bInUpdate Then Return True

            Dim mpa As cEcospaceMPA = Me.Core.EcospaceMPAs(p.Row)

            Select Case DirectCast(p.Column, eColumnTypes)

                Case eColumnTypes.MPAAll
                    Dim bIsOpen As Boolean = Not CBool(cell.GetValue(p))
                    For i As Integer = 1 To cCore.N_MONTHS
                        mpa.IsOpen(i) = bIsOpen
                    Next
                    Me.UpdateRow(p.Row)

                Case eColumnTypes.MPAJan To eColumnTypes.MPADec
                    mpa.IsOpen(p.Column + 1 - CInt(eColumnTypes.MPAJan)) = Not CBool(cell.GetValue(p))
                    Me.UpdateRow(p.Row)

            End Select

            Return True

        End Function

#End Region ' Grid interaction

#Region " Event handlers "

        Private Sub m_bpEffort_PropertyChanged(prop As cProperty, changeFlags As cProperty.eChangeFlags) _
            Handles m_bpEffort.PropertyChanged

            Try
                BeginInvoke(New MethodInvoker(AddressOf RefreshContent))
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Event handlers

    End Class

End Namespace
