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
Option Explicit On

Imports EwECore
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Utilities

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Dialog class, implements the generic show/hide items interface.
    ''' </summary>
    ''' =======================================================================
    Public Class dlgShowHideItems

#Region " Private variables "

        Private m_uic As cUIContext = Nothing
        Private m_bInSync As Boolean = False
        Private m_il As ImageList = Nothing

        Private m_groupOptions As cDisplayGroupsCommand.eGroupDisplayOptions = cDisplayGroupsCommand.eGroupDisplayOptions.All

#End Region ' Private variables

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new dialog.
        ''' </summary>
        ''' <param name="uic">The UI context to connect to.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       Optional groupOptions As cDisplayGroupsCommand.eGroupDisplayOptions = cDisplayGroupsCommand.eGroupDisplayOptions.All)
            Me.InitializeComponent()
            Debug.Assert(uic IsNot Nothing)
            Me.m_uic = uic
            Me.m_groupOptions = groupOptions
        End Sub

#End Region ' Constructor

#Region " Form overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.m_uic Is Nothing) Then Return

            Dim group As cEcoPathGroupInput = Nothing
            Dim fleet As cEcopathFleetInput = Nothing
            Dim bShowGroup As Boolean = True

            Me.m_bInSync = True

            Me.m_clbGroups.Items.Clear()
            For iGroup As Integer = 1 To Me.m_uic.Core.nGroups
                group = Me.m_uic.Core.EcoPathGroupInputs(iGroup)
                If (Me.IncludeGroup(group)) Then
                    Me.m_clbGroups.Items.Add(New cCoreInputOutputControlItem(group),
                                             Me.m_uic.StyleGuide.GroupVisible(iGroup))
                End If
            Next

            Me.m_clbFleets.Items.Clear()
            For iFleet As Integer = 1 To Me.m_uic.Core.nFleets
                fleet = Me.m_uic.Core.EcopathFleetInputs(iFleet)
                Me.m_clbFleets.Items.Add(New cCoreInputOutputControlItem(fleet),
                                         Me.m_uic.StyleGuide.FleetVisible(iFleet))
            Next

            Me.m_bInSync = False
            Me.m_cbSyncViaFishing.Checked = My.Settings.SelectionLinkThroughFishing
            Me.m_cbSyncViaPredation.Checked = My.Settings.SelectionLinkThroughPredation

            Me.m_il = New ImageList()
            Me.m_il.Images.Add(SharedResources.fish)
            Me.m_il.Images.Add(SharedResources.fishing_gear)
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
            My.Settings.SelectionLinkThroughFishing = Me.m_cbSyncViaFishing.Checked
            My.Settings.SelectionLinkThroughPredation = Me.m_cbSyncViaPredation.Checked
            My.Settings.Save()
            MyBase.OnFormClosed(e)
        End Sub

#End Region ' Form overrides

#Region " Events "

        Public Delegate Sub MethodInvoker2(iIndex As Integer)

        Private Sub OnGroupChecked(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
            Handles m_clbGroups.ItemCheck
            ' Abort if triggered by a sync call
            If Me.m_bInSync Then Return

            If (e.NewValue = CheckState.Checked) Then
                ' Delay invoke until check state has been processed
                Me.BeginInvoke(New MethodInvoker(AddressOf SyncCatchingFleets), Nothing)
                Me.BeginInvoke(New MethodInvoker2(AddressOf SyncPredation), New Object() {e.Index + 1})
            End If
        End Sub

        Private Sub OnFleetChecked(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
            Handles m_clbFleets.ItemCheck
            ' Abort if triggered by a sync call
            If Me.m_bInSync Then Return
            ' Delay invoke until check state has been processed
            Me.BeginInvoke(New MethodInvoker(AddressOf SyncLandedGroups), Nothing)
        End Sub

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles OK_Button.Click

            Me.m_uic.StyleGuide.SuspendEvents()

            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                Dim grp As cCoreGroupBase = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    Me.m_uic.StyleGuide.GroupVisible(grp.Index) = Me.m_clbGroups.GetItemChecked(i)
                End If
            Next

            For iFleet As Integer = 1 To Me.m_uic.Core.nFleets
                Me.m_uic.StyleGuide.FleetVisible(iFleet) = Me.m_clbFleets.GetItemChecked(iFleet - 1)
            Next

            Me.m_uic.StyleGuide.ResumeEvents()
            Me.m_uic.StyleGuide.ItemVisibilityChanged()

            ' And done
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles Cancel_Button.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub OnSelectAllGroups(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnAllGroups.Click

            ' Check all items
            Me.m_clbGroups.SuspendLayout()
            For iItem As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                Me.m_clbGroups.SetItemChecked(iItem, True)
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectNoneGroups(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnNoneGroups.Click

            ' Uncheck all items
            Me.m_clbGroups.SuspendLayout()
            For iItem As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                Me.m_clbGroups.SetItemChecked(iItem, False)
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.UpdateControls()

        End Sub

        Private Sub OnSelectProducers(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnProducers.Click

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (grp.IsProducer) Then
                        Me.m_clbGroups.SetItemChecked(i, True)
                        Me.SyncPredation(grp.Index)
                    End If
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectConsumers(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnConsumers.Click

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (grp.IsConsumer) Then
                        Me.m_clbGroups.SetItemChecked(i, True)
                        Me.SyncPredation(grp.Index)
                    End If
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectDetritus(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnNonLiving.Click

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (grp IsNot Nothing) Then
                        If (grp.IsDetritus) Then
                            Me.m_clbGroups.SetItemChecked(i, True)
                            Me.SyncPredation(grp.Index)
                        End If
                    End If
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectLiving(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnLiving.Click

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    Me.m_clbGroups.SetItemChecked(i, grp.IsLiving Or Me.m_clbGroups.GetItemChecked(i))
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectFished(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnFished.Click

            Dim core As cCore = Me.m_uic.Core
            Dim asIsFished(core.nGroups) As Boolean

            For iFleet As Integer = 1 To core.nFleets
                Dim fleet As cEcopathFleetInput = core.EcopathFleetInputs(iFleet)
                For iGroup As Integer = 1 To core.nGroups
                    asIsFished(iGroup) = asIsFished(iGroup) Or ((fleet.Landings(iGroup) > 0) Or (fleet.Discards(iGroup) > 0))
                Next
            Next

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (asIsFished(grp.Index)) Then
                        Me.m_clbGroups.SetItemChecked(i, True)
                        Me.SyncPredation(grp.Index)
                    End If
                End If
            Next

            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectNonFished(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnNonFished.Click

            Dim core As cCore = Me.m_uic.Core
            Dim IsFished(core.nGroups) As Boolean

            For iFleet As Integer = 1 To core.nFleets
                Dim fleet As cEcopathFleetInput = core.EcopathFleetInputs(iFleet)
                For iGroup As Integer = 1 To core.nGroups
                    IsFished(iGroup) = IsFished(iGroup) Or ((fleet.Landings(iGroup) > 0) Or (fleet.Discards(iGroup) > 0))
                Next
            Next

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (Not IsFished(grp.Index)) Then
                        Me.m_clbGroups.SetItemChecked(i, True)
                        Me.SyncPredation(grp.Index)
                    End If
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectStanza(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnStanza.Click

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (grp.IsMultiStanza) Then
                        Me.m_clbGroups.SetItemChecked(i, True)
                        Me.SyncPredation(grp.Index)
                    End If
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectNonStanza(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnNonStanza.Click

            Dim grp As cCoreGroupBase = Nothing

            Me.m_clbGroups.SuspendLayout()
            For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                grp = Me.GroupAt(i)
                If (grp IsNot Nothing) Then
                    If (Not grp.IsMultiStanza) Then
                        Me.m_clbGroups.SetItemChecked(i, True)
                        Me.SyncPredation(grp.Index)
                    End If
                End If
            Next
            Me.m_clbGroups.ResumeLayout()
            Me.SyncCatchingFleets()

        End Sub

        Private Sub OnSelectAllFleets(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnAllFleets.Click

            ' Check all items
            Me.m_clbFleets.SuspendLayout()
            For iItem As Integer = 0 To Me.m_clbFleets.Items.Count - 1
                Me.m_clbFleets.SetItemChecked(iItem, True)
            Next
            Me.m_clbFleets.ResumeLayout()
            Me.SyncLandedGroups()

        End Sub

        Private Sub OnSelectNoneFleets(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnNoneFleets.Click

            ' Uncheck all items
            Me.m_clbFleets.SuspendLayout()
            For iItem As Integer = 0 To Me.m_clbFleets.Items.Count - 1
                Me.m_clbFleets.SetItemChecked(iItem, False)
            Next
            Me.m_clbFleets.ResumeLayout()
            Me.SyncLandedGroups()

        End Sub

#End Region ' Events

#Region " Internals "

        Private Sub UpdateControls()

            Dim strLabel As String = ""
            Dim i As Integer = 0
            Dim j As Integer = 0

            i = Me.m_clbGroups.CheckedItems().Count
            j = Me.m_clbGroups.Items.Count

            If (j <= 0) Then
                strLabel = SharedResources.HEADER_GROUPS
            Else
                strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED,
                                                 SharedResources.HEADER_GROUPS,
                                                 cStringUtils.Localize(SharedResources.GENERIC_LABEL_N_OF_M, i, j))
            End If
            Me.m_hdrGroups.Text = strLabel

            i = Me.m_clbFleets.CheckedItems().Count
            j = Me.m_clbFleets.Items.Count

            If (j <= 0) Then
                strLabel = SharedResources.HEADER_FLEETS
            Else
                strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED,
                                                 SharedResources.HEADER_FLEETS,
                                                 cStringUtils.Localize(SharedResources.GENERIC_LABEL_N_OF_M, i, j))
            End If
            Me.m_hdrFleets.Text = strLabel

        End Sub

        Private Sub SyncCatchingFleets()

            ' Bail-out
            If (Not Me.m_cbSyncViaFishing.Checked) Then Return

            If Me.m_bInSync Then Return
            Me.m_bInSync = True

            Dim core As cCore = Me.m_uic.Core
            Dim bIsLinked(core.nFleets) As Boolean
            For iFleet As Integer = 1 To core.nFleets
                Dim fleet As cEcopathFleetInput = core.EcopathFleetInputs(iFleet)
                For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                    If Me.m_clbGroups.GetItemChecked(i) Then
                        Dim grp As cCoreGroupBase = Me.GroupAt(i)
                        If (grp IsNot Nothing) Then
                            Dim iGroup As Integer = grp.Index
                            bIsLinked(iFleet) = bIsLinked(iFleet) Or ((fleet.Landings(iGroup) > 0) Or (fleet.Discards(iGroup) > 0))
                        End If
                    End If
                Next
            Next

            Me.m_clbFleets.SuspendLayout()

            For iFleet As Integer = 1 To core.nFleets
                Me.m_clbFleets.SetItemChecked(iFleet - 1, bIsLinked(iFleet))
            Next

            Me.m_clbFleets.ResumeLayout()


            Me.m_bInSync = False
            Me.UpdateControls()

        End Sub

        Private Sub SyncLandedGroups()

            ' Bail-out
            If (Not Me.m_cbSyncViaFishing.Checked) Then Return

            If Me.m_bInSync Then Return
            Me.m_bInSync = True

            Dim core As cCore = Me.m_uic.Core
            Dim bIsLinked(core.nGroups) As Boolean
            For iFleet As Integer = 1 To core.nFleets
                Dim fleet As cEcopathFleetInput = core.EcopathFleetInputs(iFleet)
                If Me.m_clbFleets.GetItemChecked(iFleet - 1) Then
                    For i As Integer = 0 To Me.m_clbGroups.Items.Count - 1
                        Dim grp As cCoreGroupBase = Me.GroupAt(i)
                        If (grp IsNot Nothing) Then
                            Dim iGroup As Integer = grp.Index
                            bIsLinked(iGroup) = bIsLinked(iGroup) Or ((fleet.Landings(iGroup) > 0) Or (fleet.Discards(iGroup) > 0))
                        End If
                    Next
                End If
            Next

            Me.m_clbGroups.SuspendLayout()

            For iGroup As Integer = 1 To core.nGroups
                Me.m_clbGroups.SetItemChecked(iGroup - 1, bIsLinked(iGroup))
            Next

            Me.m_clbGroups.ResumeLayout()

            Me.m_bInSync = False
            Me.UpdateControls()

        End Sub

        Private Sub SyncPredation(iGroup As Integer)

            ' Bail-out
            If (Not Me.m_cbSyncViaPredation.Checked) Then Return

            If Me.m_bInSync Then Return
            Me.m_bInSync = True

            Dim core As cCore = Me.m_uic.Core
            Dim grp As cEcoPathGroupInput = core.EcoPathGroupInputs(iGroup)

            Me.m_clbGroups.SuspendLayout()
            For iGroupTest As Integer = 1 To core.nGroups
                If (grp.IsPred(iGroupTest) Or grp.IsPrey(iGroupTest)) Then
                    Me.m_clbGroups.SetItemChecked(iGroupTest - 1, True)
                End If
            Next
            Me.m_clbGroups.ResumeLayout()

            Me.m_bInSync = False
            Me.UpdateControls()

        End Sub

        Private Function IncludeGroup(grp As cEcoPathGroupInput) As Boolean

            Dim bInclude As Boolean = False

            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.Consumers) > 0 Then bInclude = bInclude Or grp.IsConsumer
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.Producers) > 0 Then bInclude = bInclude Or grp.IsProducer
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.Living) > 0 Then bInclude = bInclude Or grp.IsLiving
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.NonLiving) > 0 Then bInclude = bInclude Or (Not grp.IsLiving)
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.Fished) > 0 Then bInclude = bInclude Or grp.IsFished
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.NonFished) > 0 Then bInclude = bInclude Or (Not grp.IsFished)
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.Detritus) > 0 Then bInclude = bInclude Or (grp.IsDetritus)
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.Stanza) > 0 Then bInclude = bInclude Or (grp.IsMultiStanza)
            If (Me.m_groupOptions And cDisplayGroupsCommand.eGroupDisplayOptions.NonStanza) > 0 Then bInclude = bInclude Or (Not grp.IsMultiStanza)

            Return bInclude

        End Function

        Private Function GroupAt(i As Integer) As cCoreGroupBase
            Dim item As Object = Me.m_clbGroups.Items(i)
            If Not TypeOf item Is cCoreInputOutputControlItem Then Return Nothing
            Dim cci As cCoreInputOutputControlItem = DirectCast(item, cCoreInputOutputControlItem)
            If Not TypeOf cci.Source Is cCoreGroupBase Then Return Nothing
            Return DirectCast(cci.Source, cCoreGroupBase)
        End Function

#End Region ' Internals

    End Class

End Namespace
