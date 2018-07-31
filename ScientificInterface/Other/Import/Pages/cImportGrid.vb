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
Imports EwECore.Database
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports EwEUtils.Database

#End Region ' Imports

Namespace Import

    ''' =======================================================================
    ''' <summary>
    ''' EwE-derived grid to allow selection of models to import.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class cImportGrid
        Inherits EwEGrid

#Region " Private bits "

        ''' <summary>Grid columns.</summary>
        Private Enum eColumnTypes As Integer
            ''' <summary>Source model name column.</summary>
            Source = 0
            ''' <summary>Import selection toggle column.</summary>
            Import
            ''' <summary>Target model name column.</summary>
            Target
        End Enum

        ''' <summary>The attached import wizard that holds the import settings 
        ''' to display and edit in this grid.</summary>
        Private m_wizard As cImportWizard = Nothing

#End Region ' Private bits

#Region " Public bits "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the grid with wizardy stuff.
        ''' </summary>
        ''' <param name="wizard">The <see cref="cImportWizard">import wizard</see>
        ''' to initialize the grid with</param>
        ''' -----------------------------------------------------------------------
        Public Sub Init(ByVal wizard As cImportWizard)

            ' Sanity check
            Debug.Assert(wizard IsNot Nothing)

            ' Remember wizard
            Me.m_wizard = wizard
            ' Re-populate grid
            Me.RefreshContent()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that notifies the world at large that the user has made a change
        ''' to a value inside the grid.
        ''' </summary>
        ''' <param name="grid">The grid instance that fired the event.</param>
        ''' -----------------------------------------------------------------------
        Public Event OnEdited(ByVal grid As cImportGrid)

        Public ReadOnly Property SelectedModelInfo As cExternalModelInfo
            Get
                If Me.SelectedRow < 0 Then Return Nothing
                Return Me.ImportSettings(Me.SelectedRow).ModelInfo
            End Get
        End Property

#End Region ' Public bits

#Region " Internal overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Set selection mode
            Me.Selection.SelectionMode = GridSelectionMode.Cell
            Me.Selection.EnableMultiSelection = False

            ' Resize grid
            Me.Redim(1, System.Enum.GetValues(GetType(eColumnTypes)).Length)

            ' Create columns
            Me(0, eColumnTypes.Source) = New EwEColumnHeaderCell(SharedResources.HEADER_MODEL_SOURCE)
            Me(0, eColumnTypes.Import) = New EwEColumnHeaderCell(SharedResources.HEADER_IMPORT)
            Me(0, eColumnTypes.Target) = New EwEColumnHeaderCell(SharedResources.HEADER_MODEL_TARGET)

            ' Configure columns
            Me.FixedColumns = 1

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Populate grid with current import settings.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FillData()

            If (Me.m_wizard Is Nothing) Then Return

            Dim iRow As Integer = 1
            Dim pos As SourceGrid2.Position = Nothing
            Dim vm As VisualModels.Common = Nothing
            Dim ewec As EwECell = Nothing

            ' Clear existing rows
            Me.RowsCount = 1

            For Each imp As cImportWizard.cImportSettings In Me.m_wizard.ImportSettings

                iRow = Me.AddRow()

                ewec = New EwECell(imp.ModelInfo.Name, GetType(String))
                ewec.Style = cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.Source) = ewec

                Me(iRow, eColumnTypes.Import) = New Cells.Real.CheckBox(imp.SelectedForImport)
                Me(iRow, eColumnTypes.Import).Behaviors.Add(Me.EwEEditHandler)

                ewec = New EwECell("", GetType(String))
                ewec.Behaviors.Add(Me.EwEEditHandler)
                Me(iRow, eColumnTypes.Target) = ewec

                Me.ImportSettings(iRow) = imp

                ' Update EwE6 model cell to process import settings state
                Me.UpdateEwE6ModelCell(iRow)

            Next

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.Columns(eColumnTypes.Source).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.Columns(eColumnTypes.Target).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.StretchColumnsToFitWidth()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Cell has been edited: update underlying import settings.
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="cell"></param>
        ''' <returns>True to accept data.</returns>
        ''' <remarks>
        ''' This method differs by legacy reasons from 
        ''' <see cref="OnCellValueChanged">OnCellValueChanged</see>. Let's just
        ''' work with it.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function OnCellEdited(ByVal p As Position, _
                                                  ByVal cell As ICellVirtual) As Boolean

            Dim settings As cImportWizard.cImportSettings = Me.ImportSettings(p.Row)

            Select Case DirectCast(p.Column, eColumnTypes)

                Case eColumnTypes.Target
                    ' Update the name
                    settings.EwE6ModelName = CStr(cell.GetValue(p))
                    ' Refresh the cell since the model name may have been 
                    ' altered in the assignment
                    Me.UpdateEwE6ModelCell(p.Row)
                    ' Notify the world that data has been edited.
                    RaiseEvent OnEdited(Me)

            End Select

            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Cell value has been changed: update underlying import settings.
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="cell"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' This method differs by legacy reasons from 
        ''' <see cref="OnCellEdited">OnCellEdited</see>. Let's just
        ''' work with it.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function OnCellValueChanged(ByVal p As Position, _
                                                        ByVal cell As ICellVirtual) As Boolean

            Dim settings As cImportWizard.cImportSettings = Me.ImportSettings(p.Row)

            Select Case DirectCast(p.Column, eColumnTypes)

                Case eColumnTypes.Import
                    settings.SelectedForImport = CBool(cell.GetValue(p))
                    Me.UpdateEwE6ModelCell(p.Row)
                    RaiseEvent OnEdited(Me)

            End Select
            Return True

        End Function

#End Region ' Internal overrides

#Region " Internal helpers "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set import settings associated with a row in the grid.
        ''' </summary>
        ''' <param name="iRow">The row associated with the import settings.</param>
        ''' -----------------------------------------------------------------------
        Private Property ImportSettings(ByVal iRow As Integer) As cImportWizard.cImportSettings
            Get
                Dim ri As RowInfo = Me.Rows(iRow)
                Return DirectCast(ri.Tag, cImportWizard.cImportSettings)
            End Get
            Set(ByVal value As cImportWizard.cImportSettings)
                Dim ri As RowInfo = Me.Rows(iRow)
                ri.Tag = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update the read-only style and content of EwE6 model cells
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' -----------------------------------------------------------------------
        Private Sub UpdateEwE6ModelCell(ByVal iRow As Integer)

            Dim settings As cImportWizard.cImportSettings = Me.ImportSettings(iRow)
            Dim cellEwE As EwECell = DirectCast(Me(iRow, eColumnTypes.Target), EwECell)

            ' If a model is selected for import the EwE6 name cell is editable and displays data.
            ' If a model is NOT selected for import the EwE6 name cell is read-only and displays no data.

            ' Is model selected for import?
            If settings.SelectedForImport Then
                ' #Yes: show model name
                cellEwE.Value = settings.EwE6ModelName
                ' Make cell editable
                cellEwE.Style = cStyleGuide.eStyleFlags.OK
            Else
                ' #No: clear cell value
                cellEwE.Value = ""
                ' Clear cell value
                cellEwE.Style = cStyleGuide.eStyleFlags.NotEditable
            End If

            ' Re-render cell
            cellEwE.Invalidate()

        End Sub

#End Region ' Internal helpers

    End Class

End Namespace