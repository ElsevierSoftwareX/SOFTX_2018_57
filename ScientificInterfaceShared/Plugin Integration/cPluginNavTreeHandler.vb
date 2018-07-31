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
Imports EwEPlugin
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Integration

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' GUI utility class, handles the placement of
    ''' <see cref="EwEPlugin.INavigationTreeItemPlugin">INavigationTreeItemPlugin</see>-
    ''' derived plugins in a <see cref="TreeView">TreeView</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cPluginNavTreeHandler
        Inherits cPluginGUIHandler

#Region " Private parts "

#Region " Private helper class "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class, sorts INavigationTreeItemPlugin instances by tree node
        ''' name in ascending order.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class NavTreePluginComparer
            Implements IComparer(Of INavigationTreeItemPlugin)

            Public Function Compare(ByVal x As INavigationTreeItemPlugin, _
                                    ByVal y As INavigationTreeItemPlugin) As Integer _
                                    Implements IComparer(Of INavigationTreeItemPlugin).Compare

                Return String.Compare(x.NavigationTreeItemLocation, y.NavigationTreeItemLocation, True)

            End Function

        End Class

#End Region ' Private helper class

        ''' <summary>The tree view to modify.</summary>
        Private WithEvents m_tv As TreeView = Nothing

#End Region ' Private parts

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of a cPluginManuHandler.
        ''' </summary>
        ''' <param name="tv"><see cref="TreeView">TreeView</see> that contains the 
        ''' navigation structure that must be modified.</param>
        ''' <param name="pm"><see cref="cPluginManager">Plugin manager</see>
        ''' that holds the plugins to place in the control.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal tv As TreeView, _
                       ByVal pm As cPluginManager, _
                       ByVal cmdh As cCommandHandler)
            MyBase.new(pm, cmdh)
            ' Remember tree view
            Me.m_tv = tv
        End Sub

#End Region ' Construction 

#Region " Tree item handling "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Sort a list of navigation plug-ins by tree node position.
        ''' </summary>
        ''' <param name="aip">The plug-ins to sort.</param>
        ''' <returns>A ham sandwich with a cork in it.</returns>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function SortPlugins(ByVal aip() As IGUIPlugin) As IGUIPlugin()

            Dim lPlugins As New List(Of INavigationTreeItemPlugin)

            For Each ip As IGUIPlugin In aip
                If TypeOf ip Is INavigationTreeItemPlugin Then
                    lPlugins.Add(DirectCast(ip, INavigationTreeItemPlugin))
                End If
            Next

            lPlugins.Sort(New NavTreePluginComparer())

            Return lPlugins.ToArray()

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Place or remove a plug-in tree item.
        ''' </summary>
        ''' <param name="ip">The <see cref="INavigationTreeItemPlugin">INavigationTreeItemPlugin</see> to place.</param>
        ''' <param name="bPlace">States whether the tree item should be placed (True)
        ''' or removed (False).</param>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub PlacePlugin(ByVal ip As IGUIPlugin, ByVal bPlace As Boolean)

            Dim tnc As TreeNodeCollection = Me.m_tv.Nodes
            Dim tn As TreeNode = Nothing
            Dim ipNavTree As INavigationTreeItemPlugin = Nothing
            Dim strLocation As String = Nothing
            Dim aLocations() As String = Nothing
            Dim iLocation As Integer = 0
            Dim iItem As Integer = 0
            Dim bAdded As Boolean = False
            Dim bError As Boolean = False
            Dim bFound As Boolean = False

            ' Sanity check
            If Not TypeOf ip Is INavigationTreeItemPlugin Then Return

            ' Get the real node
            ipNavTree = DirectCast(ip, INavigationTreeItemPlugin)
            ' Get node location
            strLocation = ipNavTree.NavigationTreeItemLocation
            ' Split locations path
            If strLocation.IndexOf("\") >= 0 Then
                ' New style: split locations by path separator char '\'
                aLocations = strLocation.Split("\"c)
            Else
                ' Old style: split locations by pipe char '|'
                aLocations = strLocation.Split("|"c)
            End If

            ' Already there?
            bFound = (String.IsNullOrEmpty(strLocation))

            ' Trace locations across existing node levels to find where to position this plug-in
            While iLocation < aLocations.Length And Not bError
                ' Reset level search
                iItem = 0
                bFound = False
                ' Find node that matches this locations' name
                While iItem < tnc.Count And Not bFound
                    tn = DirectCast(tnc.Item(iItem), TreeNode)
                    bFound = (String.Compare(tn.Name.Trim(), aLocations(iLocation).Trim(), True) = 0)
                    iItem += 1
                End While
                ' Found a node?
                If bFound Then
                    ' #Yes: move to next level
                    tnc = tn.Nodes
                    iLocation += 1
                Else
                    ' #No: error encountered
                    bError = True
                End If
            End While

            Try
                ' Found place to add node item?
                If Not bError Then
                    ' #Yes, handle the item.
                    ' Adding or removing an item?
                    If (bPlace) Then
                        ' #Adding: create new node
                        tn = New TreeNode(ipNavTree.ControlText)
                        ' Set name
                        tn.Name = ipNavTree.Name
                        ' Set tooltip text
                        tn.ToolTipText = ipNavTree.ControlTooltipText
                        ' Attach plugin info to node tag
                        tn.Tag = ipNavTree

                        ' Attach an image, if any
                        If (Me.m_tv.ImageList IsNot Nothing) Then
                            tn.ImageIndex = Me.m_tv.ImageList.Images.Count
                            tn.SelectedImageIndex = Me.m_tv.ImageList.Images.Count
                            If (ipNavTree.ControlImage IsNot Nothing) Then
                                Me.m_tv.ImageList.Images.Add(ipNavTree.ControlImage)
                            Else
                                Me.m_tv.ImageList.Images.Add(My.Resources.plugin)
                            End If
                        End If
                        ' Regular font
                        tn.NodeFont = New System.Drawing.Font(m_tv.Font, Drawing.FontStyle.Regular)

                        ' Insert the node alphabetically sorted by name
                        bAdded = False
                        iItem = 0
                        While (bAdded = False) And (iItem < tnc.Count)
                            If (String.Compare(tn.Name, tnc(iItem).Name, True) < 0) Then
                                tnc.Insert(iItem, tn)
                                bAdded = True
                            End If
                            iItem += 1
                        End While
                        If (Not bAdded) Then tnc.Add(tn)
                    Else
                        ' #Removing: try to remove the node
                        tn = tnc.Item(ipNavTree.Name)
                        If (tn IsNot Nothing) Then tnc.Remove(tn)
                    End If
                End If
            Catch ex As Exception
                ' For now pretend all is well. Even if it is not ;)
            End Try

        End Sub

        Protected Overrides Sub EnablePlugin(ByVal ip As IGUIPlugin, ByVal bEnable As Boolean)
            ' Always enabled
        End Sub


#End Region ' Tree item handling

#Region " Tree node events "

        Private Sub tvNavigation_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) _
            Handles m_tv.AfterSelect

            ' Sanity checks
            If Not (TypeOf e.Node.Tag Is INavigationTreeItemPlugin) Then Return
            ' Fire plugin
            Me.RunPlugin(DirectCast(e.Node.Tag, INavigationTreeItemPlugin), sender, e)

        End Sub

#End Region ' Tree node events

    End Class

End Namespace
