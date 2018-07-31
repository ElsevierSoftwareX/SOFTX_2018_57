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

Imports System.Windows.Forms
Imports EwEUtils.Database
Imports ScientificInterfaceShared.Controls

Public Class ucSelector2

    Private m_uic As cUIContext = Nothing
    Private m_data As cData = Nothing
    Private m_pg As PropertyGrid = Nothing
    Private m_pl As plFlow = Nothing
    Private m_selection As cEwEDatabase.cOOPStorable() = Nothing
    Private m_iSel As Integer = -1
    Private m_bCanAddRemoveItems As Boolean = False

    Private m_unitSrc As cUnit = Nothing
    Private m_unitTgt As cUnit = Nothing

    Public Sub Init(uic As cUIContext, data As cData, pl As plFlow, pg As PropertyGrid)
        Me.m_uic = uic
        Me.m_data = data
        Me.m_pl = pl
        Me.m_pg = pg
    End Sub

    Public ReadOnly Property SelectedUnit As cUnit
        Get
            If (Me.m_selection.Length = 0) Then Return Nothing
            If (TypeOf Me.m_selection(0) Is cUnit) Then Return DirectCast(Me.m_selection(0), cUnit)
            Return Nothing
        End Get
    End Property

    Public Property Selection() As Object
        Get
            Return Me.m_selection
        End Get
        Set(ByVal value As Object)

            If (Me.m_selection IsNot Nothing) Then
                For Each obj As cEwEDatabase.cOOPStorable In Me.m_selection
                    RemoveHandler obj.OnChanged, AddressOf OnItemChanged
                Next
            End If

            ' Gather selected objects
            Dim lObj As New List(Of cEwEDatabase.cOOPStorable)

            ' Assume the worst
            Me.m_bCanAddRemoveItems = False
            Me.m_unitSrc = Nothing
            Me.m_unitTgt = Nothing

            ' Explore incoming parameters
            If (value IsNot Nothing) Then
                If (TypeOf value Is Array) Then
                    For Each obj As Object In DirectCast(value, Array)
                        If (TypeOf obj Is cLink) Then
                            If DirectCast(obj, cLink).IsVisible Then
                                lObj.Add(DirectCast(obj, cLink))

                                If (TypeOf (obj) Is cLink And Not TypeOf (obj) Is cLinkLandings) Then
                                    Me.m_bCanAddRemoveItems = True
                                End If

                                Me.m_unitSrc = DirectCast(obj, cLink).Source
                                Me.m_unitTgt = DirectCast(obj, cLink).Target

                            End If
                        ElseIf (TypeOf obj Is cEwEDatabase.cOOPStorable) Then
                            lObj.Add(DirectCast(obj, cEwEDatabase.cOOPStorable))
                        End If
                    Next
                ElseIf (TypeOf value Is cEwEDatabase.cOOPStorable) Then
                    lObj.Add(DirectCast(value, cEwEDatabase.cOOPStorable))
                End If

                Me.m_selection = lObj.ToArray
                Me.m_iSel = 0

            End If

            If (Me.m_selection IsNot Nothing) Then
                For Each obj As cEwEDatabase.cOOPStorable In Me.m_selection
                    AddHandler obj.OnChanged, AddressOf OnItemChanged
                Next
            End If

            Me.PopulateListbox()
            Me.UpdateControls()
            Me.UpdateSelection()

        End Set
    End Property

    Private m_bInUpdate As Boolean = False

    Private Sub PopulateListbox()

        Dim bCheckboxes As Boolean = False
        Dim iTop As Integer = Me.m_lbxBits.TopIndex

        If Me.m_bInUpdate Then Return

        Me.m_bInUpdate = True
        Me.m_lbxBits.Items.Clear()
        ' Update
        If (Me.m_selection IsNot Nothing) Then
            Try
                If (Me.m_selection.Length > 0) Then
                    For i As Integer = 0 To Me.m_selection.Length - 1
                        Dim itm As cEwEDatabase.cOOPStorable = Me.m_selection(i)
                        Dim pos As Integer = Me.m_lbxBits.Items.Add(itm)

                        If (TypeOf itm Is cLink) Then
                            Me.m_lbxBits.SetItemChecked(pos, DirectCast(itm, cLink).BiomassRatio > 0)
                        End If

                    Next
                End If
                Me.m_lbxBits.SelectedIndex = Math.Min(Me.m_selection.Length - 1, Me.m_iSel)
                Me.m_lbxBits.TopIndex = Math.Min(iTop, Me.m_lbxBits.Items.Count - 1)
            Catch ex As Exception

            End Try
        End If
        Me.m_bInUpdate = False

    End Sub

    Private Sub UpdateSelection()

        Dim obj As Object = Nothing

        Try
            If (Me.m_selection IsNot Nothing) Then
                If (Me.m_selection.Length > 0) Then
                    obj = Me.m_selection(Me.m_iSel)
                End If
            End If
        Catch ex As Exception

        End Try

        Me.UpdateControls()

        If Me.m_pg IsNot Nothing Then
            Me.m_pg.SelectedObject = obj
        End If

    End Sub

    Private Sub UpdateControls()

        If (Me.m_lbxBits.Items.Count > 1) Or (Me.m_bCanAddRemoveItems) Then
            Me.m_btnAdd.Enabled = Me.m_bCanAddRemoveItems
            Me.m_btnRemove.Enabled = (Me.m_lbxBits.Items.Count > 1) And Me.m_bCanAddRemoveItems
            Me.m_tlpButtons.Visible = Me.m_bCanAddRemoveItems
            Me.Visible = True
        Else
            Me.Visible = False
        End If

    End Sub

    Private Sub OnAddItem(sender As System.Object, e As System.EventArgs) _
        Handles m_btnAdd.Click

        Dim link As cLink = Me.m_data.CreateLink(Me.m_unitSrc, Me.m_unitTgt)
        Me.m_pl.AddLink(link)

        ' Ugh, this is getting ugly
        Dim sel As New List(Of Object)
        sel.AddRange(Me.m_selection)
        sel.Add(link)
        Me.Selection = sel.ToArray
        Me.m_lbxBits.SelectedIndex = sel.Count - 1

    End Sub

    Private Sub OnRemoveItem(sender As System.Object, e As System.EventArgs) _
        Handles m_btnRemove.Click

        Dim link As cLink = DirectCast(Me.m_lbxBits.SelectedItem, cLink)
        Me.m_data.DeleteLink(link)
        Me.m_pl.DeleteLink(link)

        ' Ugh, this is getting ugly
        Dim sel As New List(Of Object)
        sel.AddRange(Me.m_selection)
        sel.Remove(link)
        Me.Selection = sel.ToArray
        Me.m_lbxBits.SelectedIndex = sel.Count - 1

    End Sub

    Private Sub m_lbxBits_ItemCheck(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
        Handles m_lbxBits.ItemCheck
        If Me.m_bInUpdate Then Return
        Try
            Me.m_bInUpdate = True
            Dim item As cEwEDatabase.cOOPStorable = DirectCast(Me.m_lbxBits.Items(e.Index), cEwEDatabase.cOOPStorable)
            If (e.NewValue = CheckState.Unchecked) And (TypeOf (item) Is cLink) Then
                DirectCast(item, cLink).BiomassRatio = 0
            End If
            Me.m_bInUpdate = False
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnSelectItem(sender As System.Object, e As System.EventArgs) _
        Handles m_lbxBits.SelectedIndexChanged

        If Me.m_bInUpdate Then Return
        Me.m_iSel = Me.m_lbxBits.SelectedIndex
        Me.UpdateSelection()

    End Sub

    Private Sub OnItemChanged(obj As cEwEDatabase.cOOPStorable)
        Me.PopulateListbox()
    End Sub

End Class
