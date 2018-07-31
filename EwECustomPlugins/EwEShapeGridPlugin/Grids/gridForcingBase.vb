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
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Base grid class for showing <see cref="cForcingFunction">forcing function</see>-derived
''' shapes.
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class gridForcingBase
    Inherits gridShapeBase

    ''' <summary>Rows in the grid</summary>
    Protected Enum eRowType As Integer
        Header = 0
        Thumbnail
        Name
        FirstTime
    End Enum

    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides Property ShowAllData As Boolean
        Get
            If (Me.UIContext Is Nothing) Then Return False
            Return DirectCast(Me.Handler, cForcingShapeGUIHandler).DisplayFullXAxis
        End Get
        Set(value As Boolean)
            If (Me.UIContext Is Nothing) Then Return
            DirectCast(Me.Handler, cForcingShapeGUIHandler).DisplayFullXAxis = value
            Me.RefreshContent()
        End Set
    End Property

#Region " Grid overrides "

    Protected Overridable Function XAxisMax() As Integer
        Return DirectCast(Me.Handler, cForcingShapeGUIHandler).XAxisMaxValue
    End Function

    Protected Overrides Sub FillData()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.Handler.UIContext Is Nothing) Then Return

        Dim shapes As cShapeData() = Me.Shapes
        Dim iNumShapes As Integer = shapes.Length
        Dim iNumPoints As Integer = 0
        Dim iNumHeaders As Integer = 0
        Dim cell As SourceGrid2.Cells.ICell = Nothing
        Dim bSeasonal As Boolean = Me.IsSeasonal
        Dim bMonthly As Boolean = Me.IsMonthly
        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        If bSeasonal Then
            iNumPoints = cCore.N_MONTHS
        Else
            iNumPoints = Me.XAxisMax
        End If
        Me.Redim(iNumPoints + [Enum].GetValues(GetType(eRowType)).Length, iNumShapes + 1)

        cApplicationStatusNotifier.StartProgress(Me.UIContext.Core, SharedResources.STATUS_UPDATING)

        ' Create row headers
        Me(eRowType.Header, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_INDEX)
        Me(eRowType.Thumbnail, 0) = New EwERowHeaderCell(SharedResources.HEADER_IMAGE)
        Me(eRowType.Name, 0) = New EwERowHeaderCell(SharedResources.HEADER_NAME)

        ' Create row header cells
        For i As Integer = 0 To iNumPoints - 1
            cell = New EwERowHeaderCell(Me.Label(i))
            If ((i Mod cCore.N_MONTHS) > 0) Or (Not bMonthly) Then
                cell.VisualModel.TextAlignment = Drawing.ContentAlignment.MiddleCenter
            End If
            Me(eRowType.FirstTime + i, 0) = cell
        Next

        ' Populate shape columns
        For i As Integer = 0 To iNumShapes - 1

            Me.Shape(i + 1) = shapes(i)
            style = If(Me.Handler.CanEditPoints(shapes(i)), cStyleGuide.eStyleFlags.OK, cStyleGuide.eStyleFlags.NotEditable)

            Me(eRowType.Header, i + 1) = New EwEColumnHeaderCell(CStr(shapes(i).Index))

            cell = New SourceGrid2.Cells.Real.Cell
            cell.Value = shapes(i)
            cell.VisualModel = New cVisualModelThumbnail(Me.Handler)
            Me(eRowType.Thumbnail, i + 1) = cell

            cell = New EwECell(shapes(i).Name, GetType(String))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.Name, i + 1) = cell

            For j As Integer = 0 To Math.Min(iNumPoints, shapes(i).nPoints) - 1
                cell = New EwECell(shapes(i).ShapeData(j + 1), GetType(Single), style)
                cell.Behaviors.Add(Me.EwEEditHandler)
                Me(eRowType.FirstTime + j, i + 1) = cell
            Next
            For j As Integer = shapes(i).nPoints To iNumPoints - 1
                cell = New EwECell(0, GetType(Integer), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)
                Me(eRowType.FirstTime + j, i + 1) = cell
            Next
        Next

        cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.Handler.UIContext Is Nothing) Then Return

        Me.Rows(eRowType.Thumbnail).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
        Me.Rows(eRowType.Thumbnail).Height = 48
        For i As Integer = 1 To Me.ColumnsCount - 1
            Me.Columns(i).Width = Math.Max(Me.Columns(i).Width, 48)
        Next
        ' Fix rows up to (not including) name, because name needs to be editable. Fixed cells cannot be editable
        Me.FixedRows = eRowType.Name
        ' Fix header column
        Me.FixedColumns = 1
    End Sub

#End Region ' Grid overrides

#Region " Edits "

    Dim m_bInLocalEdit As Boolean = False

    Protected Overrides Function OnCellEdited(ByVal p As SourceGrid2.Position, _
                                              ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        ' Encapsulate cell edit in a safety net to prevent grid update loops
        Dim shape As cShapeData = Me.Shape(p.Column)

        Me.m_bInLocalEdit = True
        If (Me.IsInBatchEdit) Then shape.LockUpdates()

        Me.SafeCellEdit(p, cell)

        If (Me.IsInBatchEdit) Then
            shape.UnlockUpdates(False)
            Me.InvalidateShape(shape)
        End If
        Me.m_bInLocalEdit = False

        Return MyBase.OnCellEdited(p, cell)
    End Function

    Protected Overrides Function OnCellValueChanged(ByVal p As SourceGrid2.Position, _
                                                    ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean
        Me.OnCellEdited(p, cell)
        Return MyBase.OnCellValueChanged(p, cell)
    End Function

    Protected Overridable Function SafeCellEdit(ByVal p As SourceGrid2.Position, _
                                              ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Dim shape As cShapeData = Me.Shape(p.Column)
        Select Case DirectCast(p.Row, eRowType)
            Case eRowType.Name
                shape.Name = CStr(cell.GetValue(p))
            Case Else
                Dim iTime As Integer = p.Row - eRowType.FirstTime
                shape.ShapeData(iTime) = CSng(cell.GetValue(p))
        End Select
        Return True

    End Function

#End Region ' Edits

#Region " Updates "

    Protected Overrides Sub OnRefreshed(ByVal sender As cShapeGUIHandler)

        If Me.IsInBatchEdit Then
            Return
        End If

        ' Unpleasant: a refresh can be triggered from an external edit or by 
        ' this very interface in response to a cell edit. If a cell edit is in
        ' progress the grid content cannot be refreshed.

        ' In local cell edit?
        If Me.m_bInLocalEdit Then
            ' #Yes: just invalidate the thumbnail
            Me.InvalidateRange(New SourceGrid2.Range(eRowType.Thumbnail, 0, eRowType.Thumbnail, Me.ColumnsCount - 1))
        Else
            ' #No: refresh the whole lot
            Me.RefreshContent()
        End If

    End Sub

#End Region ' Updates

    Protected Overrides Function Label(ByVal iPoint As Integer) As String

        Dim iMonth As Integer = (iPoint Mod 12) + 1
        If Not Me.IsSeasonal And (iMonth = 1) Then
            Return CStr(Math.Floor(iPoint / cCore.N_MONTHS) + Me.Core.EcosimFirstYear)
        End If
        Return cDateUtils.GetMonthName(iMonth)

    End Function

End Class
