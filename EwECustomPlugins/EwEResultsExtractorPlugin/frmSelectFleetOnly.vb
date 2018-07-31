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

Imports EwECore

Public Class frmSelectFleetOnly
    Inherits CreateCollectionForData

    Public Event FormExited()

    Public Sub New(ByVal i As cSelectionData, ByVal m_core As cCore)
        MyBase.New(i, m_core)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Width = 380
        chklstAttached.Hide()
        btnAttachAll.Hide()
        btnAttachNone.Hide()
        btnOk.Left = 280
        Me.Show()

    End Sub
    Public Overrides Sub PopulateAttachedList(ByVal i As String)

    End Sub

    Private Sub frmSelectFleetOnly_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If frmResults.FireChecked = False Then
            frmResults.NextAction()
        End If
        RaiseEvent FormExited()
    End Sub
End Class