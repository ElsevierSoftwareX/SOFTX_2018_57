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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ZedGraph
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' Main interface to define the functional responses of groups to environmental drivers.
''' </summary>
Public NotInheritable Class dlgDefineEcospaceForagingResponse

    'ToDo  update graph interface from edit dialog 

#Region " Private variables "

    Protected m_uic As cUIContext = Nothing
    Protected m_manager As IEnvironmentalResponseManager = Nothing

    Private m_bInUpdate As Boolean = False

#End Region ' Private variables

#Region " Construction Initialization "

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="uic"></param>
    ''' <param name="shape"></param>
    ''' <param name="manager"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal uic As cUIContext,
                   ByVal shape As EwECore.cEnviroResponseFunction,
                   ByVal manager As EwECore.IEnvironmentalResponseManager)

        Me.InitializeComponent()

        Me.m_uic = uic
        Me.m_manager = manager

        Me.m_graph.Init(Me.m_uic)
        Me.m_graph.Shape = shape

        Debug.Print("Load dialogue " + Me.m_graph.Shape.ToCSVString())

        Try
            Me.Text = cStringUtils.Localize(Me.Text, New cShapeDataFormatter().GetDescriptor(shape))
        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.m_uic Is Nothing) Then Return

        Try
            Me.m_bInUpdate = True

            Me.m_lbxGroups.Attach(Me.m_uic)
            Me.m_lbxGroups.Populate(Me.GetGroupList())

            Me.LoadDrivers()

        Catch ex As Exception

        End Try

        Me.m_bInUpdate = False

    End Sub

    Protected Function GetGroupList() As Integer()
        Dim lstGroups As New List(Of Integer)
        For iGrp As Integer = 1 To Me.m_uic.Core.nGroups
            Dim grp As cEcospaceGroupInput = Me.m_uic.Core.EcospaceGroupInputs(iGrp)
            If ((grp.CapacityCalculationType And eEcospaceCapacityCalType.EnvResponses) = eEcospaceCapacityCalType.EnvResponses) Then
                lstGroups.Add(iGrp)
            End If
        Next
        Return lstGroups.ToArray()
    End Function

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        If (Me.m_uic Is Nothing) Then Return

        Me.m_lbxGroups.Detach()
        Me.m_graph.Dispose()

        MyBase.OnFormClosed(e)

    End Sub

#End Region ' Construction Initialization

#Region " Control Event Handlers "

    Private Sub OnGroupSelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_lbxGroups.SelectedValueChanged
        Try
            Me.UpdateControls()
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' Add the selected groups to the currently selected map
    ''' </summary>
    Private Sub OnAddGroup(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnAdd.Click

        Try
            Dim driver As IEnviroInputData = Me.m_graph.Driver
            Dim shape As cEnviroResponseFunction = Me.m_graph.Shape

            ' Abort if no selected map
            If (driver Is Nothing) Then Return

            'Yes add all the groups 
            For Each i As Integer In Me.m_lbxGroups.SelectedIndices
                driver.ResponseIndexForGroup(Me.m_lbxGroups.GetGroupIndexAt(i)) = shape.Index
            Next

            'bluntly reload the map tree
            Me.LoadDrivers()

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub

    Private Sub OnRemoveGroup(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnRemove.Click

        Try
            Dim driver As IEnviroInputData = Me.m_graph.Driver
            If (driver Is Nothing) Then Return

            Dim node As TreeNode
            node = Me.m_tvDrivers.SelectedNode
            If (node IsNot Nothing) Then
                ' Is group node?
                If (TypeOf (node.Tag) Is cCoreGroupBase) Then
                    ' #Yes: group was put in the tag when the tree was populated
                    Dim grp As cCoreGroupBase = DirectCast(node.Tag, cCoreGroupBase)
                    driver.ResponseIndexForGroup(grp.Index) = cCore.NULL_VALUE
                    node.Remove()
                Else
                    Dim lGroupNodes As New List(Of TreeNode)
                    For Each ndChild As TreeNode In node.Nodes
                        lGroupNodes.Add(ndChild)
                    Next
                    For Each ndChild As TreeNode In lGroupNodes
                        Dim grp As cCoreGroupBase = DirectCast(ndChild.Tag, cCoreGroupBase)
                        driver.ResponseIndexForGroup(grp.Index) = cCore.NULL_VALUE
                        ndChild.Remove()
                    Next
                End If
            End If

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnOk.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()

    End Sub

    Private Sub OnMapTreeExpanded(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) _
        Handles m_tvDrivers.AfterExpand

        Try
            Me.m_graph.Driver = Me.GetSelectedDriver(e.Node)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnDriverSelected(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) _
        Handles m_tvDrivers.AfterSelect
        Try
            Me.m_graph.Driver = GetSelectedDriver(e.Node)
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Control Event Handlers

#Region " Private Methods "

    Private Sub UpdateControls()

        Dim bCanAddGroup As Boolean = (Me.m_lbxGroups.SelectedItems.Count > 0)
        Dim bCanRemoveGroup As Boolean = (Me.m_tvDrivers.SelectedNode IsNot Nothing)

        Me.m_btnAdd.Enabled = bCanAddGroup
        Me.m_btnRemove.Enabled = bCanRemoveGroup

    End Sub

    Protected Sub LoadDrivers()

        Dim map As IEnviroInputData = Nothing
        Dim fmt As New cCoreInterfaceFormatter()

        Dim shape As cEnviroResponseFunction = Me.m_graph.Shape
        Debug.Assert(shape IsNot Nothing)

        Try
            Me.m_tvDrivers.Nodes.Clear()

            For imap As Integer = 1 To Me.m_manager.nEnviroData

                map = Me.m_manager.EnviroData(imap)
                Dim ndApply As TreeNode = Me.m_tvDrivers.Nodes.Add(map.Name)
                ndApply.Tag = map

                For igrp As Integer = 1 To Me.m_uic.Core.nGroups
                    'Is the current shape selected as the response function for any group
                    If (shape IsNot Nothing) Then
                        If (shape.Index = map.ResponseIndexForGroup(igrp)) Then
                            'Yes this shape is set for this group
                            'add a group node
                            Dim grp As cEcospaceGroupInput = Me.m_uic.Core.EcospaceGroupInputs(igrp)
                            If ((grp.CapacityCalculationType And eEcospaceCapacityCalType.EnvResponses) = eEcospaceCapacityCalType.EnvResponses) Then

                                Dim ndgrp As TreeNode = ndApply.Nodes.Add(fmt.GetDescriptor(grp))
                                ndgrp.Tag = grp

                                If Not ndApply.IsExpanded Then
                                    'if there are groups assigned to this Map/Node then expand it the tree to this point
                                    ndApply.ExpandAll()
                                End If
                            End If
                        End If
                    End If
                Next
            Next

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".LoadDrivers() Exception: " & ex.Message)
        End Try

    End Sub

    Private Function GetSelectedDriver(ByVal node As TreeNode) As IEnviroInputData
        Try

            Dim ob As Object = Nothing

            'No node has been selected just return nothing
            If (node Is Nothing) Then Return Nothing

            Do While node.Parent IsNot Nothing
                node = node.Parent
            Loop
            ob = node.Tag

            If ob IsNot Nothing Then
                If TypeOf ob Is IEnviroInputData Then
                    Return DirectCast(ob, IEnviroInputData)
                End If
            End If

        Catch ex As Exception

        End Try

        Return Nothing

    End Function

#End Region ' Private Methods

End Class