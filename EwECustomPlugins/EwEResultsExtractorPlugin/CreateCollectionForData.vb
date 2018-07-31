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

Option Strict Off
Imports EwECore

Public MustInherit Class CreateCollectionForData

    Protected m_SelectionData As cSelectionData
    Private ObjectUnderFocus As cCreatedObjects
    Protected m_core As cCore


    Public Sub New(ByVal SelectionData As cSelectionData, ByVal Core As cCore)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.


        ' Set a reference to the singleton instance of cCore
        Me.m_core = Core

        ' Store SelectionData sent to object
        m_SelectionData = SelectionData

        'Load group names into selected & unselected lists
        For Each x In SelectionData.UnSelectedNames
            lstUnSelected.Items.Add(x)
        Next
        For Each x In SelectionData.SelectedNames
            lstSelected.Items.Add(x)
        Next

        If lstSelected.Items.Count = 0 Then
            btnRemoveAll.Enabled = False
            btnRemoveSelected.Enabled = False
        End If
        If lstUnSelected.Items.Count = 0 Then
            btnAddAll.Enabled = False
            btnAddSelected.Enabled = False
        End If

    End Sub

    'Don't use this now because unpredictable - seemed to conflict with SelectedIndexChanged
    Private Sub lstSelected_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstSelected.MouseDoubleClick

        'Dim IndexSaved As Integer

        ''http://msdn.microsoft.com/en-us/library/kfw3x8dc.aspx
        ''Prevents anything from happening when white space double clicked at bottom of listbox
        'If lstSelected.IndexFromPoint(e.Location) = -1 Then Exit Sub

        ''Get index of selected item
        'IndexSaved = lstSelected.SelectedIndex

        ''Add selected back to unselected
        'lstUnSelected.Items.Add(lstSelected.SelectedItem)
        'lstSelected.SelectedIndex = lstSelected.Items.Count - 1

        ''Remove from selection object
        'm_SelectionData.Remove(lstSelected.SelectedItem.ToString)

        ''Remove from selected
        'lstSelected.Items.RemoveAt(IndexSaved)

        ''If selection at top of list select 1 less else select same index as began with
        'If lstSelected.Items.Count = IndexSaved Or IndexSaved = 0 Then
        '    lstSelected.SelectedIndex = IndexSaved - 1
        'Else
        '    lstSelected.SelectedIndex = IndexSaved
        'End If

        'SetStateAddRemove()

    End Sub

    Private Sub lstSelected_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstSelected.SelectedIndexChanged

        If lstSelected.SelectedIndex = -1 Then
            m_SelectionData.SetFocus = Nothing
            Me.chklstAttached.Items.Clear()
            Exit Sub
        End If

        PopulateAttachedList(lstSelected.SelectedItem.ToString)
        m_SelectionData.SetFocus = lstSelected.SelectedItem.ToString

        ' Ticks childs that are part of current parent
        For x = 0 To chklstAttached.Items.Count - 1
            For Each i In CType(m_SelectionData.GetFocus, cCreatedObjects).ChildNames
                If chklstAttached.Items(x).ToString = i.ToString Then
                    chklstAttached.SetItemChecked(x, True)
                End If
            Next
        Next


    End Sub

    Public MustOverride Sub PopulateAttachedList(ByVal i As String)

    Private Sub btnAddSelected_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddSelected.Click
        Dim PositionSelectedPredator As Integer

        If lstUnSelected.SelectedIndex = -1 Then Exit Sub

        'Buttons to remove are now enabled
        btnRemoveSelected.Enabled = True
        btnRemoveAll.Enabled = True

        ' Add to virtual object current selection
        m_SelectionData.Add(lstUnSelected.SelectedItem.ToString)

        ' Remove from 1st list box and add to 2nd
        lstSelected.Items.Add(lstUnSelected.SelectedItem)
        lstSelected.SelectedIndex = lstSelected.Items.Count - 1
        PositionSelectedPredator = lstUnSelected.SelectedIndex
        lstUnSelected.Items.Remove(lstUnSelected.SelectedItem)

        'Depending on position of selection and number of items in list select next item
        If PositionSelectedPredator = lstUnSelected.Items.Count Then
            lstUnSelected.SelectedIndex = PositionSelectedPredator - 1
        ElseIf PositionSelectedPredator > 0 Then
            lstUnSelected.SelectedIndex = PositionSelectedPredator - 1
        ElseIf lstUnSelected.Items.Count > 0 Then
            lstUnSelected.SelectedIndex = 0
        End If

        SetStateAddRemove()

    End Sub

    Private Sub SetStateAddRemove()

        btnAddSelected.Enabled = False
        btnAddAll.Enabled = False
        btnRemoveSelected.Enabled = False
        btnRemoveAll.Enabled = False

        If lstUnSelected.Items.Count > 0 Then
            btnAddSelected.Enabled = True
            btnAddAll.Enabled = True
        End If
        If lstSelected.Items.Count > 0 Then
            btnRemoveSelected.Enabled = True
            btnRemoveAll.Enabled = True
        End If

    End Sub

    Private Sub btnAddAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddAll.Click

        While lstUnSelected.Items.Count > 0
            m_SelectionData.Add(lstUnSelected.Items(0).ToString)
            lstSelected.Items.Add(lstUnSelected.Items(0))
            lstSelected.SelectedIndex = lstSelected.Items.Count - 1
            lstUnSelected.Items.RemoveAt(0)
        End While

        SetStateAddRemove()

    End Sub

    Private Sub btnRemoveSelected_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveSelected.Click

        Dim PositionSelectedPredator As Integer

        If lstSelected.SelectedIndex = -1 Then Exit Sub

        'Buttons to remove are now enabled
        btnAddSelected.Enabled = True
        btnAddAll.Enabled = True

        ' Remove in virtual object current selection
        m_SelectionData.Remove(lstSelected.SelectedItem.ToString)

        ' Remove from 1st list box and add to 2nd
        lstUnSelected.Items.Add(lstSelected.SelectedItem)
        lstUnSelected.SelectedIndex = lstUnSelected.Items.Count - 1
        PositionSelectedPredator = lstSelected.SelectedIndex
        lstSelected.Items.Remove(lstSelected.SelectedItem)

        'Depending on position of selection and number of items in list select next item
        If PositionSelectedPredator = lstSelected.Items.Count Then
            lstSelected.SelectedIndex = PositionSelectedPredator - 1
        ElseIf PositionSelectedPredator > 0 Then
            lstSelected.SelectedIndex = PositionSelectedPredator - 1
        ElseIf lstUnSelected.Items.Count > 0 Then
            lstSelected.SelectedIndex = 0
        End If

        SetStateAddRemove()

    End Sub

    Private Sub btnRemoveAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAll.Click

        While lstSelected.Items.Count > 0
            m_SelectionData.Remove(lstSelected.Items(0).ToString)
            lstUnSelected.Items.Add(lstSelected.Items(0))
            lstUnSelected.SelectedIndex = lstUnSelected.Items.Count - 1
            lstSelected.Items.RemoveAt(0)
        End While

        SetStateAddRemove()

    End Sub

    Private Sub lstUnSelected_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstUnSelected.MouseDoubleClick
        Dim IndexSaved As Integer

        'Remove from unselection object
        m_SelectionData.Add(lstUnSelected.SelectedItem.ToString)

        'http://msdn.microsoft.com/en-us/library/kfw3x8dc.aspx
        'Prevents anything from happening when white space double clicked at bottom of listbox
        If lstUnSelected.IndexFromPoint(e.Location) = -1 Then Exit Sub

        'Get index of unselected item
        IndexSaved = lstUnSelected.SelectedIndex

        'Add unselected back to selected
        lstSelected.Items.Add(lstUnSelected.SelectedItem)
        lstUnSelected.SelectedIndex = lstUnSelected.Items.Count - 1

        'Remove from unselected list
        lstUnSelected.Items.RemoveAt(IndexSaved)

        'If selection at top of list select 1 less else select same index as began with
        If lstUnSelected.Items.Count = IndexSaved Or IndexSaved = 0 Then
            lstUnSelected.SelectedIndex = IndexSaved - 1
        Else
            lstUnSelected.SelectedIndex = IndexSaved
        End If

        SetStateAddRemove()

    End Sub

    Private Sub chklstAttached_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles chklstAttached.ItemCheck

        Dim temp As cCreatedObjects

        If e.NewValue = System.Windows.Forms.CheckState.Checked Then
            'Attach checked item to currently selected parent
            temp = m_SelectionData.GetFocus
            temp.Add(chklstAttached.Items(e.Index))
        End If

        If e.NewValue = System.Windows.Forms.CheckState.Unchecked Then
            'Attach checked item to currently selected parent
            temp = m_SelectionData.GetFocus
            temp.Remove(chklstAttached.Items(e.Index))
        End If

        'SetSaveResultsState()
    End Sub

    Private Sub btnAttachAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAttachAll.Click
        For i = 0 To chklstAttached.Items.Count - 1
            chklstAttached.SetItemChecked(i, True)
        Next
    End Sub

    Private Sub btnAttachNone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAttachNone.Click
        For i = 0 To chklstAttached.Items.Count - 1
            chklstAttached.SetItemChecked(i, False)
        Next
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Me.Close()
    End Sub
End Class