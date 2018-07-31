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

Option Explicit On
Option Strict On

Imports EwECore
Imports ZedGraph
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style

#End Region

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Dialog class, implements the Ecosim interface for defining mediation
    ''' percentages.
    ''' </summary>
    ''' <remarks>
    ''' Drivers may be two-dimensional (fleet, group landings) or one dimensional 
    ''' (fleet or group).
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class dlgDefineMediationAssignments

#Region " Private variables "

        ''' <summary>Core ref.</summary>
        Private m_uic As cUIContext
        ''' <summary>The med function being edited.</summary>
        Private m_medfn As cMediationBaseFunction = Nothing
        ''' <summary>Selected object.</summary>
        Private m_objSelected As cCoreInputOutputBase = Nothing

        Private m_bIsLandingsInteractions As Boolean = False

#End Region ' Private variables

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="medfn"></param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal UIC As cUIContext, ByVal medfn As cMediationBaseFunction)

            Me.InitializeComponent()

            ' Sanity checks
            Debug.Assert(medfn IsNot Nothing)

            ' Store medfn
            Me.m_medfn = medfn
            Me.m_uic = UIC

            Me.m_bIsLandingsInteractions = (TypeOf Me.m_medfn Is cLandingsMediationFunction)
            Me.m_grid.IsLandings = Me.m_bIsLandingsInteractions

            ' Pass UI context on
            Me.m_graph.UIContext = Me.m_uic
            Me.m_grid.UIContext = Me.m_uic

        End Sub

#End Region ' Constructor

#Region " Event handlers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (m_uic Is Nothing) Then Return

            If Not Me.m_bIsLandingsInteractions Then
                ' Add existing mediations
                For iGroup As Integer = 1 To m_uic.Core.nGroups
                    Dim grp As cEcoPathGroupInput = m_uic.Core.EcoPathGroupInputs(iGroup)
                    Dim fleet As cEcopathFleetInput = Nothing

                    For j As Integer = 0 To m_medfn.NumGroups - 1
                        Dim medGrp As cMediatingGroup = m_medfn.Group(j)
                        If iGroup = medGrp.iGroupIndex Then
                            Me.Add(grp, fleet, medGrp.Weight)
                            Exit For
                        End If
                    Next
                Next

                For iFleet As Integer = 1 To m_uic.Core.nFleets
                    Dim iIndex As Integer = m_uic.Core.nGroups + iFleet
                    Dim flt As cEcopathFleetInput = m_uic.Core.EcopathFleetInputs(iFleet)

                    For j As Integer = 0 To m_medfn.NumFleet - 1
                        Dim medFlt As cMediatingFleet = m_medfn.Fleet(j)
                        If iFleet = medFlt.iFleetIndex Then
                            Me.Add(flt, Nothing, medFlt.Weight)
                            Exit For
                        End If
                    Next
                Next
            Else
                For j As Integer = 0 To m_medfn.NumGroups - 1
                    Dim medGrp As cLandingsMediatingGroup = DirectCast(m_medfn.Group(j), cLandingsMediatingGroup)
                    Dim grp As cEcoPathGroupInput = Nothing
                    Dim fleet As cEcopathFleetInput = Nothing
                    If (medGrp.iGroupIndex > 0) Then
                        grp = Me.m_uic.Core.EcoPathGroupInputs(medGrp.iGroupIndex)
                        fleet = Me.m_uic.Core.EcopathFleetInputs(medGrp.iFleetIndex)

                        If grp IsNot Nothing And fleet IsNot Nothing Then
                            Me.Add(grp, fleet, medGrp.Weight)
                        End If
                    End If
                Next
            End If

            ' Update available list box
            Me.UpdateAvailableGroupsAndFleets(m_uic.Core.EcoPathGroupInputs(1))
            Me.UpdateGraph()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnOkidoki(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnOK.Click
            If Not Me.Apply() Then Return
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnCancel.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnAdd.Click, m_tvAvailable.DoubleClick

            Try
                Dim item As cCoreInputOutputControlItem = DirectCast(Me.m_tvAvailable.SelectedNode, cCoreInputOutputControlItem)
                Dim itemParent As cCoreInputOutputControlItem = DirectCast(Me.m_tvAvailable.SelectedNode, cCoreInputOutputControlItem)
                Dim src As cCoreInputOutputBase = Nothing
                Dim srcSec As cCoreInputOutputBase = Nothing

                If (item IsNot Nothing) Then
                    src = item.Source
                    If Not m_bIsLandingsInteractions Then
                        itemParent = DirectCast(item.Parent, cCoreInputOutputControlItem)
                        If (itemParent IsNot Nothing) Then
                            ' Could be just a decorative node, let's check
                            If (itemParent.Source IsNot Nothing) Then
                                ' Add parent as src, child as sec
                                srcSec = src
                                src = itemParent.Source
                            End If
                        End If
                        Me.Add(src, srcSec)
                    Else ' landings
                        If TypeOf src Is cEcopathFleetInput Then
                            ' Add an entry for each landing of this fleet
                            For Each nd As TreeNode In item.Nodes
                                Dim group As cCoreInputOutputBase = DirectCast(nd, cCoreInputOutputControlItem).Source
                                Me.Add(group, src, DirectCast(src, cEcopathFleetInput).OffVesselValue(group.Index))
                            Next
                        Else
                            ' Add as group, fleet
                            itemParent = DirectCast(item.Parent, cCoreInputOutputControlItem)
                            If (itemParent IsNot Nothing) Then
                                srcSec = itemParent.Source
                            End If
                            Me.Add(src, srcSec, DirectCast(srcSec, cEcopathFleetInput).OffVesselValue(src.Index))
                        End If
                    End If

                End If
            Catch ex As Exception

            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub btnRemove_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_btnRemove.Click
            For Each objz As cCoreInputOutputBase() In Me.m_grid.SelectedItems
                Me.Remove(objz(0), objz(1))
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnAvailableSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tvAvailable.AfterSelect
            Me.UpdateControls()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub m_grid_OnSelectionChanged() _
            Handles m_grid.OnSelectionChanged
            Me.UpdateControls()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub m_grid_OnWeightChanged(ByVal obj As EwECore.cCoreInputOutputBase, ByVal sWeight As Single) _
            Handles m_grid.OnWeightChanged
            Me.UpdateGraph()
        End Sub

#End Region ' Event handlers

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function Apply() As Boolean

            Dim d As ucMediationAssignments.cBioPercentData = Me.m_grid.Data

            Me.m_medfn.Clear()

            For Each grp As cMediatingGroup In d.Groups
                If Me.m_bIsLandingsInteractions Then
                    Debug.Assert(TypeOf grp Is cLandingsMediatingGroup)
                    Debug.Assert(TypeOf Me.m_medfn Is cLandingsMediationFunction)

                    Dim lmg As cLandingsMediatingGroup = DirectCast(grp, cLandingsMediatingGroup)
                    Dim lfn As cLandingsMediationFunction = DirectCast(Me.m_medfn, cLandingsMediationFunction)
                    lfn.AddGroup(lmg.iGroupIndex, lmg.Weight, lmg.iFleetIndex)
                Else
                    Me.m_medfn.AddGroup(grp.iGroupIndex, grp.Weight)
                End If
            Next

            For Each flt As cMediatingFleet In d.Fleets
                Me.m_medfn.AddFleet(flt.iFleetIndex, flt.Weight)
            Next
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateAvailableGroupsAndFleets(ByVal objSelected As cCoreInputOutputBase)

            Dim lChildren As List(Of cCoreInputOutputControlItem) = Nothing
            Dim group As cCoreGroupBase = Nothing
            Dim fleet As cEcopathFleetInput = Nothing
            Dim nodeSelected As TreeNode = Nothing
            Dim iSelectedIndex As Integer = -1

            Me.m_tvAvailable.Nodes.Clear()

            Try

                If Not m_bIsLandingsInteractions Then
                    lChildren = New List(Of cCoreInputOutputControlItem)
                    For iGroup As Integer = 1 To m_uic.Core.nGroups
                        group = m_uic.Core.EcoPathGroupInputs(iGroup)
                        Dim node As cCoreInputOutputControlItem = New cCoreInputOutputControlItem(group)
                        lChildren.Add(node)
                        If ReferenceEquals(group, objSelected) Then nodeSelected = node
                    Next
                    Me.m_tvAvailable.Nodes.Add(New cCoreInputOutputControlItem(My.Resources.HEADER_GROUPS, lChildren.ToArray))

                    lChildren = New List(Of cCoreInputOutputControlItem)
                    For iFleet As Integer = 1 To m_uic.Core.nFleets
                        fleet = m_uic.Core.EcopathFleetInputs(iFleet)
                        Dim node As cCoreInputOutputControlItem = New cCoreInputOutputControlItem(fleet)
                        lChildren.Add(node)
                        If ReferenceEquals(group, objSelected) Then nodeSelected = node
                    Next
                    Me.m_tvAvailable.Nodes.Add(New cCoreInputOutputControlItem(My.Resources.HEADER_FLEETS, lChildren.ToArray))
                Else
                    ' Landings: show as landings per fleet, per group
                    For iFleet As Integer = 1 To m_uic.Core.nFleets
                        fleet = m_uic.Core.EcopathFleetInputs(iFleet)
                        lChildren = New List(Of cCoreInputOutputControlItem)

                        For iGroup As Integer = 1 To m_uic.Core.nGroups
                            If fleet.Landings(iGroup) > 0 Then
                                group = m_uic.Core.EcoPathGroupInputs(iGroup)

                                Dim node As cCoreInputOutputControlItem = New cCoreInputOutputControlItem(group)
                                If ReferenceEquals(fleet, objSelected) Then nodeSelected = node
                                lChildren.Add(node)
                            End If
                        Next

                        If lChildren.Count > 0 Then
                            Dim nodeParent As cCoreInputOutputControlItem = New cCoreInputOutputControlItem(fleet, lChildren.ToArray)
                            Me.m_tvAvailable.Nodes.Add(nodeParent)
                            If ReferenceEquals(group, objSelected) Then nodeSelected = nodeParent
                        End If
                    Next
                End If

            Catch ex As Exception

            End Try

            Me.m_tvAvailable.SelectedNode = nodeSelected
            Me.m_tvAvailable.ExpandAll()
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateControls()

            Me.m_btnAdd.Enabled = (Me.m_tvAvailable.SelectedNode IsNot Nothing)
            Me.m_btnRemove.Enabled = (Me.m_grid.SelectedItems.Length > 0)

            Dim ftm As New cShapeDataFormatter()
            If Me.m_bIsLandingsInteractions Then
                Me.Text = cStringUtils.Localize(My.Resources.CAPTION_DEFINE_MEDIATING_LANDINGS, ftm.GetDescriptor(Me.m_medfn))
                Me.m_lblAvailable.Text = My.Resources.HEADER_AVAILABLE_LANDINGS
                Me.m_lblAssigned.Text = My.Resources.HEADER_ASSIGNED_LANDINGS
            Else
                Me.Text = cStringUtils.Localize(My.Resources.CAPTION_DEFINE_MEDIATING_GROUPSANDFLEETS, ftm.GetDescriptor(Me.m_medfn))
                Me.m_lblAvailable.Text = My.Resources.HEADER_AVAILABLE_GROUPS_FLEETS
                Me.m_lblAssigned.Text = My.Resources.HEADER_ASSIGNED_GROUPS_FLEETS
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Add(ByVal obj As cCoreInputOutputBase, _
                        Optional ByVal objSec As cCoreInputOutputBase = Nothing, _
                        Optional ByVal sWeight As Single = 1.0)

            ' Is landings?
            If Me.m_bIsLandingsInteractions Then
                ' #Yes: can only define landings for both group and fleet
                If objSec Is Nothing Or obj Is Nothing Then Return
            Else
                ' #No: can only add groups OR fleets, not both

            End If

            If Me.m_grid.Add(obj, objSec, sWeight) Then
                Me.UpdateGraph()
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Remove(ByVal obj As cCoreInputOutputBase, _
                           Optional ByVal objSec As cCoreInputOutputBase = Nothing)
            If Me.m_grid.Remove(obj, objSec) Then
                Me.UpdateGraph()
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateGraph()
            Me.m_graph.LoadGraphData(Me.m_grid.Data)
        End Sub

#End Region ' Internals

    End Class

End Namespace

