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
Imports System.Windows.Forms
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Controls

#Region " cTreeViewNodeController "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Helper class, maintains a collection of <see cref="cNodeInfo">NodeInfo</see>
    ''' objects.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cTreeViewNodeController

        ''' <summary>UI context to connect to.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>TreeView that is being controlled.</summary>
        ''' <remarks>M_TV? haha</remarks>
        Private m_tv As TreeView = Nothing
        ''' <summary>List of added nodes</summary>
        Private m_lNodeInfo As New List(Of cNodeInfo)
        ''' <summary>Default node to select.</summary>
        Private m_niDefault As cNodeInfo = Nothing

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Connects an instance to a tree view.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub Attach(ByVal uic As cUIContext, ByVal tv As TreeView)

            Debug.Assert(Me.m_tv Is Nothing)
            Debug.Assert(tv IsNot Nothing)

            ' Store refs
            Me.m_tv = tv
            Me.m_uic = uic

            AddHandler Me.m_tv.AfterSelect, AddressOf OnAfterSelect
            AddHandler Me.m_tv.AfterExpand, AddressOf OnAfterExpand
            AddHandler Me.m_tv.VisibleChanged, AddressOf OnVisibleChanged

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Connects an instance to a tree view.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub Detach()

            Debug.Assert(Me.m_tv IsNot Nothing)

            RemoveHandler Me.m_tv.AfterSelect, AddressOf OnAfterSelect
            RemoveHandler Me.m_tv.AfterExpand, AddressOf OnAfterExpand
            RemoveHandler Me.m_tv.VisibleChanged, AddressOf OnVisibleChanged

            Me.m_tv = Nothing
            Me.m_uic = Nothing

        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strlabel"></param>
        ''' <param name="strInternalName"></param>
        ''' <param name="execstate"></param>
        ''' <param name="tClass"></param>
        ''' <param name="parent"></param>
        ''' <param name="strHelpURL"></param>
        ''' <param name="bIsDefault"></param>
        ''' <returns></returns>
        Public Function Add(strlabel As String, strInternalName As String, execstate As eCoreExecutionState, tClass As Type, imgkey As Integer,
                            Optional parent As TreeNode = Nothing, Optional strHelpURL As String = "",
                            Optional bIsDefault As Boolean = False) As TreeNode

            Dim node As New TreeNode(strlabel, imgkey, imgkey)
            node.Name = strInternalName

            Dim ni As New cNodeInfo(strInternalName, execstate, tClass, strHelpURL)
            Me.m_lNodeInfo.Add(ni)
            If bIsDefault Then
                Me.m_niDefault = ni
            End If

            If (parent Is Nothing) Then
                Me.m_tv.Nodes.Add(node)
            Else
                parent.Nodes.Add(node)
            End If

            Return node

        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="var"></param>
        ''' <param name="strInternalName"></param>
        ''' <param name="execstate"></param>
        ''' <param name="tClass"></param>
        ''' <param name="parent"></param>
        ''' <param name="strHelpURL"></param>
        ''' <param name="bIsDefault"></param>
        ''' <returns></returns>
        Public Function Add(var As eVarNameFlags, strInternalName As String, execstate As eCoreExecutionState, tClass As Type, imgkey As Integer,
                            Optional parent As TreeNode = Nothing, Optional strHelpURL As String = "",
                            Optional bIsDefault As Boolean = False) As TreeNode

            Dim fmt As New EwECore.Style.cVarnameTypeFormatter()
            Dim node As New TreeNode(fmt.GetDescriptor(var), imgkey, imgkey)
            node.Name = strInternalName

            Dim ni As New cNodeInfo(strInternalName, execstate, tClass, strHelpURL)
            Me.m_lNodeInfo.Add(ni)
            If bIsDefault Then Me.m_niDefault = ni

            If (parent Is Nothing) Then
                Me.m_tv.Nodes.Add(node)
            Else
                parent.Nodes.Add(node)
            End If

            Return node

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add node info to this controller.
        ''' </summary>
        ''' <param name="strTreeNodeName"><see cref="TreeNode.Name">Name</see> of the tree node.</param>
        ''' <param name="execstate"><see cref="eCoreExecutionState">Core execution state flag</see>
        ''' indicating the state of the EwE Core this node should listen to.</param>
        ''' <param name="tClass">Class type of the Form to build when invoking this tree node from
        ''' the application navigation tree.</param>
        ''' <param name="strHelpURL">Help URL for this node.</param>
        ''' -----------------------------------------------------------------------
        Private Sub Add(ByVal strTreeNodeName As String,
                       ByVal execstate As eCoreExecutionState,
                       ByVal tClass As Type,
                       Optional ByVal strHelpURL As String = "")

            Dim ni As New cNodeInfo(strTreeNodeName, execstate, tClass, strHelpURL)
            Me.m_lNodeInfo.Add(ni)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Searches added nodes for <see cref="cNodeInfo">NodeInfo</see> by a given node <see cref="cNodeInfo.NodeName">Name</see>.
        ''' </summary>
        ''' <param name="strNodeName">Name to find</param>
        ''' <returns>The <see cref="cNodeInfo">NodeInfo</see> for the requested node
        ''' <see cref="cNodeInfo.NodeName">Name</see>, or Nothing if no such nodeInfo
        ''' was added.</returns>
        ''' -----------------------------------------------------------------------
        Public Function SearchNodeByName(ByVal strNodeName As String) As cNodeInfo
            For Each eachNode As cNodeInfo In m_lNodeInfo
                If strNodeName = eachNode.NodeName Then
                    '' Load the selection
                    Return eachNode
                End If
            Next
            Return Nothing
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, expands a nested series of child nodes with one child.
        ''' </summary>
        ''' <param name="node">The node to expand cascading.</param>
        ''' <param name="bExpand">Flag indicating whether node should expand (True)
        ''' or collapse (False).</param>
        ''' -----------------------------------------------------------------------
        Public Sub ExpandCollapseNodes(ByVal node As TreeNode, Optional ByVal bExpand As Boolean = True)
            If bExpand Then
                node.Expand()
                Me.ExpandChildren(node)
            Else
                node.Collapse()
            End If
            node.EnsureVisible()
        End Sub

        ''' -------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name of the default node to select in case the current
        ''' node is cleared.
        ''' </summary>
        ''' -------------------------------------------------------------------------------------------
        Public ReadOnly Property DefaultNodeName() As String
            Get
                If Me.m_niDefault Is Nothing Then Return ""
                Return Me.m_niDefault.NodeName
            End Get
        End Property

#End Region ' Public access

#Region " Internals "

        Private Sub ExpandChildren(ByVal node As TreeNode)
            If node.GetNodeCount(False) = 1 Then
                node.Expand()
                ExpandChildren(node.FirstNode)
            End If
        End Sub

        ''' -------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; handles a node selection. Invokes a<see cref="cNavigationCommand">Navigation command</see>
        ''' for any tree node bearing <see cref="cNodeInfo">rich node information</see>.
        ''' </summary>
        ''' <param name="sender">The tree</param>
        ''' <param name="e">Event info</param>
        ''' -------------------------------------------------------------------------------------------
        Private Sub OnAfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs)

            ' Sanity check
            Debug.Assert(Me.m_tv IsNot Nothing, "Node detached?!")

            Dim ni As cNodeInfo = Me.SearchNodeByName(e.Node.Name)
            Dim cmdH As cCommandHandler = Nothing
            Dim cmd As cCommand = Nothing
            Dim cmdNav As cNavigationCommand = Nothing

            ' Is this a registered node, i.e. does this node have a form attached?
            If (ni IsNot Nothing) Then
                ' #Yes: launch form via central Navigate command
                ' Get command handler
                cmdH = Me.m_uic.CommandHandler
                ' Get the navigation command
                cmd = cmdH.GetCommand(cNavigationCommand.COMMAND_NAME)
                ' Does this command exist?
                If cmd IsNot Nothing Then
                    ' #Yes: is typeof NavigateCommand?
                    If (TypeOf cmd Is cNavigationCommand) Then
                        ' #Yes: Good, now cast it
                        cmdNav = DirectCast(cmd, cNavigationCommand)
                        ' ..and launch
                        cmdNav.Invoke(e.Node.Text, ni.NodeName, ni.ExecutionState, ni.Type, ni.HelpURL)
                    End If
                End If
            End If

            Me.m_tv.Visible = True
            Me.ExpandCollapseNodes(e.Node, True)

        End Sub

        Private Sub OnAfterExpand(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs)
            ExpandCollapseNodes(e.Node)
        End Sub

        ''' -------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; handles a treeview visible state change event. Implemented to make sure
        ''' that the current selected node is visible whenever this control is made visible.
        ''' </summary>
        ''' <param name="sender">The tree</param>
        ''' <param name="e">Event info</param>
        ''' -------------------------------------------------------------------------------------------
        Private Sub OnVisibleChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

            If m_tv.Visible Then

                Dim selNd As TreeNode = m_tv.SelectedNode

                If selNd IsNot Nothing Then
                    ExpandCollapseNodes(selNd)
                    selNd.EnsureVisible()
                End If
            End If

        End Sub

#End Region ' Internals

    End Class

#End Region ' cTreeViewNodeController

#Region " cNodeInfo "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Helper class, maintains information for a single Navigation tree node.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cNodeInfo

        ''' <summary><see cref="TreeNode.Name">Name</see> of the node.</summary>
        Private m_strName As String = ""
        ''' <summary>Flag indicating the EwE execution state this node belongs to.</summary>
        Private m_executionState As eCoreExecutionState = eCoreExecutionState.Idle
        ''' <summary>Type of the Form class that must be created for this node.</summary>
        Private m_type As Type
        ''' <summary>Help URL for this node.</summary>
        Private m_strHelpURL As String

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' <param name="strName"><see cref="TreeNode.Name">Name</see> of the
        ''' corresponding <see cref="TreeNode">TreeNode</see>.</param>
        ''' <param name="executionState">The <see cref="eCoreExecutionState">Core execution state</see>
        ''' that this node belongs to.</param>
        ''' <param name="tClass">The Type of the Form that needs to be instantiated
        ''' when the corresponding <see cref="TreeNode">TreeNode</see> is selected.</param>
        ''' ---------------------------------------------------------------------------
        Public Sub New(ByVal strName As String,
                        ByVal executionState As eCoreExecutionState,
                        ByVal tClass As Type,
                        ByVal strHelpURL As String)
            Me.m_strName = strName
            Me.m_executionState = executionState
            Me.m_type = tClass
            Me.m_strHelpURL = strHelpURL
        End Sub

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="TreeNode.Name">Name</see> of the
        ''' corresponding <see cref="TreeNode">TreeNode</see>.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public ReadOnly Property NodeName() As String
            Get
                Return Me.m_strName
            End Get
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eCoreExecutionState">Core execution state</see>
        ''' that this node belongs to.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public ReadOnly Property ExecutionState() As eCoreExecutionState
            Get
                Return Me.m_executionState
            End Get
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get/Set the Type of the Form that needs to be instantiated
        ''' when the corresponding <see cref="TreeNode">TreeNode</see> is selected.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public Property Type() As Type
            Get
                Return Me.m_type
            End Get
            Set(ByVal classType As Type)
                Me.m_type = classType
            End Set
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get the help url for this node.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public ReadOnly Property HelpURL() As String
            Get
                Return Me.m_strHelpURL
            End Get
        End Property

    End Class

#End Region ' cNodeInfo

End Namespace ' Controls
