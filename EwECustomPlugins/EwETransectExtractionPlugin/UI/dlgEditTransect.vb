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
Imports System.Drawing
Imports System.Windows.Forms
Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style.cStyleGuide

#End Region ' Imports

Public Class dlgEditTransect

    Private m_uic As cUIContext = Nothing
    Private m_core As cCore = Nothing
    Private m_data As cTransectDatastructures = Nothing
    Private m_transect As cTransect = Nothing

    Public Sub New(uic As cUIContext, t As cTransect)
        Me.InitializeComponent()
        Me.m_uic = uic
        Me.m_core = uic.Core
        Me.m_data = cTransectDatastructures.Instance(Me.m_core)
        Me.m_transect = t
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        ' ToDo: globalize this method

        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap

        Me.m_tbxName.Text = Me.m_transect.Name

        Me.m_colName.DefaultCellStyle.BackColor = Me.m_uic.StyleGuide.ApplicationColor(eApplicationColorType.NAMES_BACKGROUND)
        Me.m_dgvPos.RowHeadersDefaultCellStyle.BackColor = SystemColors.ButtonFace

        Me.m_dgvPos.Rows.Add("X0", bm.LonToCol(Me.m_transect.Start.X), Me.m_transect.Start.X)
        Me.m_dgvPos.Rows.Add("Y0", bm.LatToRow(Me.m_transect.Start.Y), Me.m_transect.Start.Y)
        Me.m_dgvPos.Rows.Add("X1", bm.LonToCol(Me.m_transect.End.X), Me.m_transect.End.X)
        Me.m_dgvPos.Rows.Add("Y1", bm.LatToRow(Me.m_transect.End.Y), Me.m_transect.End.Y)

        Me.UpdateControls()

    End Sub

#Region " Control events "

    Private Sub OnNameChanged(sender As Object, e As EventArgs) Handles m_tbxName.TextChanged
        Me.UpdateControls()
    End Sub

    Private Sub OnCellEdited(sender As Object, e As DataGridViewCellEventArgs) _
        Handles m_dgvPos.CellEndEdit

        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap

        Dim row As DataGridViewRow = Me.m_dgvPos.Rows(e.RowIndex)
        Dim v1 As Integer = CInt(row.Cells(1).Value)
        Dim v2 As Single = CSng(row.Cells(2).Value)

        Select Case e.ColumnIndex
            Case 1
                ' Convert cell to coordinate
                If (e.RowIndex Mod 2 = 0) Then
                    v2 = bm.ColToLon(v1) + bm.CellSize / 2
                Else
                    v2 = bm.RowToLat(v1) + bm.CellSize / 2
                End If
                row.Cells(2).Value = v2
            Case 2
                ' Convert coordinate to cell
                If (e.RowIndex Mod 2 = 0) Then
                    v1 = bm.LonToCol(v2)
                Else
                    v1 = bm.LatToRow(v2)
                End If
                row.Cells(1).Value = v1
        End Select

    End Sub

    Private Sub OnOK(sender As Object, e As EventArgs) Handles m_btnOK.Click

        Me.m_transect.Name = Me.m_tbxName.Text

        Me.m_transect.Start = New PointF(CSng(Me.m_dgvPos.Rows(0).Cells(2).Value), CSng(Me.m_dgvPos.Rows(1).Cells(2).Value))
        Me.m_transect.End = New PointF(CSng(Me.m_dgvPos.Rows(2).Cells(2).Value), CSng(Me.m_dgvPos.Rows(3).Cells(2).Value))

        Me.DialogResult = DialogResult.OK
        Me.Close()

        ' Shazaam
        Me.m_data.OnChanged(Me.m_transect)

    End Sub

    Private Sub OnCancel(sender As Object, e As EventArgs) Handles m_btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

#End Region ' Control events

#Region " Internals "

    Private Sub UpdateControls()

        Me.m_btnOK.Enabled = IsUniqueName(Me.m_tbxName.Text)

    End Sub

    Private Function IsUniqueName(strName As String) As Boolean
        Dim bIsUnique As Boolean = Not String.IsNullOrWhiteSpace(strName)
        For Each t As cTransect In Me.m_data.Transects
            If (Not ReferenceEquals(t, Me.m_transect)) Then
                bIsUnique = (bIsUnique And String.Compare(t.Name, strName, True) <> 0)
            End If
        Next
        Return bIsUnique
    End Function

#End Region ' Internals

End Class