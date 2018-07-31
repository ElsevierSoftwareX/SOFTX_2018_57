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

Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2

#End Region ' Imports

<CLSCompliant(False)> _
Public Class gridTaxonSearchResults
    Inherits EwEGrid

#Region " Private vars "

    Private m_results As IDataSearchResults = Nothing
    Private m_dgtIsTaxonUseCallback As IsTaxonUsedDelegate = Nothing

    ''' <summary>Enumerated type defining the columns in this grid.</summary>
    ''' <remarks>The logic that shows and hides code columns depends on the position 
    ''' of the status column, which is presumed to reside before the code columns.
    ''' Please do not alter the position of the status and code columns.</remarks>
    Private Enum eColumnTypes As Integer
        Index = 0
        Name
        Genus
        Species
        Family
        Order
        [Class]
        'Code
        CodeSAUP
        CodeFB
        CodeSLB
        CodeFAO
        CodeLSID
    End Enum

    Private m_bShowCodes As Boolean = False

#End Region ' Private vars

    Public Sub New()
    End Sub

    Public Delegate Function IsTaxonUsedDelegate(ti As ITaxonSearchData) As Boolean

    Public Sub Init(ByVal uic As cUIContext, Optional dgt As IsTaxonUsedDelegate = Nothing)

        Me.UIContext = uic
        Me.m_dgtIsTaxonUseCallback = dgt

        Try
            Me.m_results = Nothing
            Me.InitLayout()
        Catch ex As Exception
            ' Aargh
        End Try

    End Sub

    Public Sub AddResults(ByVal results As IDataSearchResults)

        Try
            Me.m_results = results
            Me.RowsCount = 1
            Me.FillData()
        Catch ex As Exception
            ' Aargh
        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the various keys need to be shown in the grid.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ShowCodes As Boolean
        Get
            Return Me.m_bShowCodes
        End Get
        Set(value As Boolean)
            If (Me.m_bShowCodes <> value) Then
                Me.m_bShowCodes = value
                Me.RefreshContent()
            End If
        End Set
    End Property

    Public Event OnResultSelected(ByVal result As Object)

    Public Property TaxonAtRow(Optional iRow As Integer = -1) As ITaxonSearchData
        Get
            If (iRow <= 0) Then
                iRow = Me.SelectedRow()
            End If
            If iRow < 1 Then
                Return Nothing
            End If
            Return DirectCast(Me.Rows(iRow).Tag, ITaxonSearchData)
        End Get
        Set(value As ITaxonSearchData)
            If (iRow < 1) Then Return
            Me.Rows(iRow).Tag = value
        End Set
    End Property

    Public Sub OnUsedTaxaChanged()
        Me.UpdateTaxaUsedStatus()
    End Sub

#Region " Internals "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Me.Selection.SelectionMode = GridSelectionMode.Row
        Me.FixedColumnWidths = False

        Dim iNumCols As Integer = CInt(if(Me.m_bShowCodes, System.Enum.GetValues(GetType(eColumnTypes)).Length, eColumnTypes.Class + 1))
        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(eVarNameFlags.Name)
        Me(0, eColumnTypes.Species) = New EwEColumnHeaderCell(eVarNameFlags.Species)
        Me(0, eColumnTypes.Genus) = New EwEColumnHeaderCell(eVarNameFlags.Genus)
        Me(0, eColumnTypes.Family) = New EwEColumnHeaderCell(eVarNameFlags.Family)
        Me(0, eColumnTypes.Order) = New EwEColumnHeaderCell(eVarNameFlags.Order)
        Me(0, eColumnTypes.Class) = New EwEColumnHeaderCell(eVarNameFlags.Class)
        'Me(0, eColumnTypes.Phylum) = New EwEColumnHeaderCell(SharedResources.HEADER_PHYLUM)
        If (Me.m_bShowCodes) Then
            'Me(0, eColumnTypes.Code) = New EwEColumnHeaderCell(SharedResources.HEADER_CODE)
            Me(0, eColumnTypes.CodeFB) = New EwEColumnHeaderCell(eVarNameFlags.CodeFB)
            Me(0, eColumnTypes.CodeSLB) = New EwEColumnHeaderCell(eVarNameFlags.CodeSLB)
            Me(0, eColumnTypes.CodeSAUP) = New EwEColumnHeaderCell(eVarNameFlags.CodeSAUP)
            Me(0, eColumnTypes.CodeFAO) = New EwEColumnHeaderCell(eVarNameFlags.CodeFAO)
            Me(0, eColumnTypes.CodeLSID) = New EwEColumnHeaderCell(eVarNameFlags.CodeLSID)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub FillData()

        If Me.UIContext Is Nothing Then Return
        If Me.m_results Is Nothing Then Return

        Dim results As Object() = Me.m_results.SearchResults
        For i As Integer = 0 To results.Count - 1
            Dim res As Object = results(i)
            If (TypeOf res Is ITaxonSearchData) Then
                Me.AddResult(DirectCast(res, ITaxonSearchData))
            End If
        Next

    End Sub

    Private Function IsNullOrEmpty(res As ITaxonSearchData) As Boolean
        If (res Is Nothing) Then Return True
        Return String.IsNullOrWhiteSpace(res.Common) And _
               String.IsNullOrWhiteSpace(res.Species) And _
               String.IsNullOrWhiteSpace(res.Genus) And _
               String.IsNullOrWhiteSpace(res.Family) And _
               String.IsNullOrWhiteSpace(res.Order) And _
               String.IsNullOrWhiteSpace(res.Class)
    End Function

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()

        For iCol As Integer = 1 To Me.ColumnsCount - 1
            Select Case DirectCast(iCol, eColumnTypes)
                Case eColumnTypes.Index
                    Me.Columns(iCol).Width = 20
                Case Else
                    Me.Columns(iCol).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
                    Me.AutoSizeColumn(iCol, 40)
            End Select
        Next

    End Sub

    Protected Overrides Sub OnCellDoubleClicked(ByVal p As SourceGrid2.Position, ByVal cell As SourceGrid2.Cells.ICellVirtual)
        Try
            If (Me.m_dgtIsTaxonUseCallback IsNot Nothing) Then
                If Me.m_dgtIsTaxonUseCallback.Invoke(Me.TaxonAtRow(p.Row)) Then
                    Return
                End If
            End If

            RaiseEvent OnResultSelected(Me.TaxonAtRow)
            Me.UpdateTaxaUsedStatus()

        Catch ex As Exception

        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="result"></param>
    ''' -----------------------------------------------------------------------
    Private Sub AddResult(ByVal result As ITaxonSearchData)

        ' Last line of defense
        If Me.IsNullOrEmpty(result) Then Return

        Try

            Dim iRow As Integer = Me.AddRow()
            Me.TaxonAtRow(iRow) = result
            For iCol As Integer = 0 To Me.ColumnsCount - 1
                Me.AddCell(result, iRow, DirectCast(iCol, eColumnTypes))
            Next
            Me.UpdateTaxaUsedStatus(iRow)

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub AddCell(ByVal result As ITaxonSearchData, ByVal iRow As Integer, ByVal col As eColumnTypes)

        Dim value As Object = Nothing
        Dim cell As EwECell = Nothing
        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        Select Case col
            Case eColumnTypes.Index : value = iRow
            Case eColumnTypes.Name : value = result.Common
            Case eColumnTypes.Species : value = result.Species : style = style Or cStyleGuide.eStyleFlags.Taxon
            Case eColumnTypes.Genus : value = result.Genus : style = style Or cStyleGuide.eStyleFlags.Taxon
            Case eColumnTypes.Family : value = result.Family
            Case eColumnTypes.Order : value = result.Order
            Case eColumnTypes.Class : value = result.Class
                'Case eColumnTypes.Phylum : value = result.Phylum
                'Case eColumnTypes.Code: value = result.SourceKey
            Case eColumnTypes.CodeFB : value = result.CodeFB
            Case eColumnTypes.CodeSLB : value = result.CodeSLB
            Case eColumnTypes.CodeSAUP : value = result.CodeSAUP
            Case eColumnTypes.CodeFAO : value = result.CodeFAO
            Case eColumnTypes.CodeLSID : value = result.CodeLSID

        End Select

        If (value Is Nothing) Then value = ""

        cell = New EwECell(value, value.GetType(), style)
        cell.Behaviors.Add(EwEEditHandler)
        cell.SuppressZero(0) = True
        cell.EnableEdit = False

        Me(iRow, col) = cell

    End Sub

    Private Sub UpdateTaxaUsedStatus(Optional iRow As Integer = 0)

        Dim ti As ITaxonSearchData = Nothing
        Dim cell As EwECell = Nothing
        Dim iRowMin As Integer = 1
        Dim iRowMax As Integer = Me.RowsCount - 1

        If (Me.m_dgtIsTaxonUseCallback Is Nothing) Then Return

        If (iRow > 0) Then iRowMin = iRow : iRowMax = iRow

        For iRow = iRowMin To iRowMax

            cell = DirectCast(Me(iRow, eColumnTypes.Index), EwECell)
            ti = Me.TaxonAtRow(iRow)

            If Me.m_dgtIsTaxonUseCallback.Invoke(ti) Then
                cell.Style = cell.Style Or cStyleGuide.eStyleFlags.Checked
            Else
                cell.Style = cell.Style And Not cStyleGuide.eStyleFlags.Checked
            End If
            cell.Invalidate()
        Next

    End Sub

#End Region ' Internals

End Class
