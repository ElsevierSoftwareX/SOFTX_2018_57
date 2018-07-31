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

Public Class lstCreateMember

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        btnAddSelected.Enabled = False
        btnAddAll.Enabled = False
        btnRemoveSelected.Enabled = False
        btnRemoveAll.Enabled = False

    End Sub

    Public Sub AddToNonMember(ByRef InputArray() As String)
        For Each i In InputArray
            lstNonMember.Items.Add(i)
        Next
    End Sub

    Private Sub btnAddSelected_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddSelected.Click

        'Change the state of buttons
        btnRemoveSelected.Enabled = True
        btnRemoveAll.Enabled = True

        'Add item to member list
        lstMember.Items.Add(lstNonMember.SelectedItem)

        'If number in non-member list is only 1 remove and set index to -1
        If lstNonMember.Items.Count = 1 Then
            lstNonMember.Items.Remove(lstNonMember.SelectedItem)
            lstNonMember.SelectedIndex = -1

            'If top of list remove and set in index to count-1
        ElseIf lstNonMember.SelectedIndex = lstNonMember.Items.Count Then
            lstNonMember.Items.Remove(lstNonMember.SelectedItem)
            lstNonMember.SelectedIndex = lstNonMember.Items.Count

            'if bottom of list then remove and set index to 0
        ElseIf lstNonMember.SelectedIndex = 0 Then
            lstNonMember.Items.Remove(lstNonMember.SelectedItem)
            lstNonMember.SelectedIndex = 0

            ' must be in middle of list so move index up and remove item beneath
        Else
            lstNonMember.SelectedIndex = lstNonMember.SelectedIndex + 1
            lstNonMember.Items.RemoveAt(lstNonMember.SelectedIndex - 1)
        End If

    End Sub

    Private Sub btnAddAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddAll.Click

        'Move each individually from non-member list to member list
        While lstNonMember.Items.Count > 0
            lstMember.Items.Add(lstNonMember.Items(0))
            lstNonMember.Items.RemoveAt(0)
        End While

        'if needed modify button states
        SetAddRemoveState()

    End Sub

    Private Sub btnRemoveSelected_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveSelected.Click

        'Change the state of buttons
        btnAddSelected.Enabled = True
        btnAddAll.Enabled = True

        'Add item to non-member list
        lstNonMember.Items.Add(lstMember.SelectedItem)

        'If number in member list is only 1 remove and set index to -1
        If lstMember.Items.Count = 1 Then
            lstMember.Items.Remove(lstMember.SelectedItem)
            lstMember.SelectedIndex = -1

            'If top of list remove and set in index to count-1
        ElseIf lstMember.SelectedIndex = lstMember.Items.Count Then
            lstMember.Items.Remove(lstMember.SelectedItem)
            lstMember.SelectedIndex = lstMember.Items.Count

            'if bottom of list then remove and set index to 0
        ElseIf lstMember.SelectedIndex = 0 Then
            lstMember.Items.Remove(lstMember.SelectedItem)
            lstMember.SelectedIndex = 0

            ' must be in middle of list so move index up and remove item beneath
        Else
            lstMember.SelectedIndex = lstMember.SelectedIndex + 1
            lstMember.Items.RemoveAt(lstMember.SelectedIndex - 1)
        End If

    End Sub

    Private Sub btnRemoveAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAll.Click

        'Move each individually from non-member list to member list
        While lstMember.Items.Count > 0
            lstNonMember.Items.Add(lstMember.Items(0))
            lstMember.Items.RemoveAt(0)
        End While

        'if needed modify button states
        SetAddRemoveState()

    End Sub

    Private Sub SetAddRemoveState()

        'For adding buttons
        If lstNonMember.Items.Count = 0 Then
            btnAddSelected.Enabled = False
            btnAddAll.Enabled = False
        ElseIf lstNonMember.Items.Count > 0 Then
            btnAddSelected.Enabled = True
            btnAddAll.Enabled = True
        End If

        'For removing buttons
        If lstMember.Items.Count = 0 Then
            btnRemoveSelected.Enabled = False
            btnRemoveAll.Enabled = False
        ElseIf lstNonMember.Items.Count > 0 Then
            btnRemoveSelected.Enabled = True
            btnRemoveAll.Enabled = True
        End If

    End Sub

End Class
