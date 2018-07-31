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
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports SharedResources = ScientificInterfaceShared.My.Resources

Friend Class ucGridView

    Private m_qe As cQuickEditHandler = Nothing
    Private m_grid As EwEGrid = Nothing

    Public Sub New(grid As EwEGrid)
        MyBase.New()
        Me.InitializeComponent()
        Me.m_grid = grid
        Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plGrid.Controls.Add(Me.m_grid)
    End Sub

    Protected Overrides Sub OnLoad(e As System.EventArgs)

        MyBase.OnLoad(e)

        Dim core As cCore = Me.m_grid.UIContext.Core
        Dim group As cEcoPathGroupInput = Nothing
        Dim fleet As cEcopathFleetInput = Nothing

        Me.m_qe = New cQuickEditHandler()
        Me.m_qe.Attach(Me.m_grid, Me.m_grid.UIContext, Me.m_ts)

        If Me.CanFilter Then

            Me.m_tscmbGroup.Items.Add(New cCoreInputOutputControlItem(SharedResources.GENERIC_VALUE_ALL))
            For igroup As Integer = 1 To core.nGroups
                group = core.EcoPathGroupInputs(igroup)
                If (group.IsFished) Then
                    Me.m_tscmbGroup.Items.Add(New cCoreInputOutputControlItem(group))
                End If
            Next
            Me.m_tscmbGroup.SelectedIndex = 0

            Me.m_tscmbFleet.Items.Add(New cCoreInputOutputControlItem(SharedResources.GENERIC_VALUE_ALL))
            For ifleet As Integer = 1 To core.nFleets
                fleet = core.EcopathFleetInputs(ifleet)
                Me.m_tscmbFleet.Items.Add(New cCoreInputOutputControlItem(fleet))
            Next
            Me.m_tscmbFleet.SelectedIndex = 0

        End If

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If (Me.m_qe IsNot Nothing) Then
                Me.m_qe.Detach()
                Me.m_qe = Nothing
            End If

            If (Me.m_grid IsNot Nothing) Then
                Me.m_plGrid.Controls.Clear()
                Me.m_grid.UIContext = Nothing
                Me.m_grid.Dispose()
                Me.m_grid = Nothing
            End If
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try

    End Sub

    Private Sub OnSelectGroup(sender As System.Object, e As System.EventArgs) _
        Handles m_tscmbGroup.SelectedIndexChanged
        If Me.CanFilter Then
            DirectCast(Me.m_grid, ucLinkGrid).Group = DirectCast(Me.m_tscmbGroup.SelectedItem, cCoreInputOutputControlItem).Source
        End If
    End Sub

    Private Sub OnSelectFleet(sender As System.Object, e As System.EventArgs) _
        Handles m_tscmbFleet.SelectedIndexChanged
        If Me.CanFilter Then
            DirectCast(Me.m_grid, ucLinkGrid).Fleet = DirectCast(Me.m_tscmbFleet.SelectedItem, cCoreInputOutputControlItem).Source
        End If
    End Sub

    Private Sub UpdateControls()

        Dim bCanFilter As Boolean = Me.CanFilter

        Me.m_tslGroup.Visible = bCanFilter
        Me.m_tscmbGroup.Visible = bCanFilter
        Me.m_tslFleet.Visible = bCanFilter
        Me.m_tscmbFleet.Visible = bCanFilter

    End Sub

    Private Function CanFilter() As Boolean
        If (TypeOf Me.m_grid Is ucLinkGrid) Then
            Return DirectCast(Me.m_grid, ucLinkGrid).CanFilter
        End If
        Return False
    End Function

End Class
