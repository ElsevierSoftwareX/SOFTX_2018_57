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

Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Extensions
Imports ScientificInterfaceShared.Forms
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' The status panel tracks core messages, and relevant messages are displayed
''' in the panel. Messages with hyperlinks are properly displayed, and hyperlink
''' activation is dispatched via the regular command structure.
''' </summary>
''' -----------------------------------------------------------------------
Public Class frmStatusPanel

#Region " Private vars "

    Private Const iICON_SIZE As Integer = 8

    Private m_il As New ImageList()
    Private m_uic As cUIContext = Nothing
    Private m_hist As cMessageHistory = Nothing

#End Region ' Private vars

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of the RemarkPanel.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext, ByVal hist As cMessageHistory)

        Me.InitializeComponent()
        Me.m_uic = uic
        Me.m_hist = hist
        Me.TabText = SharedResources.HEADER_STATUS

        ' Prepare image list for all defined importance types
        ' JS 28Oct13: do this before OnLoad because messages may already have been added before the panel is visible.
        For Each imp As eMessageImportance In [Enum].GetValues(GetType(eMessageImportance))
            Dim img As Image = cStyleGuide.GetImage(imp)
            If (img IsNot Nothing) Then Me.m_il.Images.Add(Me.GetImageKey(imp, False), img)
        Next

    End Sub

#End Region ' Constructor

#Region " Form overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.m_uic Is Nothing) Then Return

        ' Set image list
        Me.m_tvStatus.ImageList = Me.m_il
        Me.m_tvStatus.ImageIndex = -1
        Me.m_tvStatus.SelectedImageIndex = -1
        Me.m_tvStatus.SelectedImageKey = ""

        Me.Icon = Icon.FromHandle(SharedResources.History.GetHicon)

        Me.SyncHistory()

        ' Go live
        AddHandler Me.m_hist.OnHistoryItemAdded, AddressOf OnHistoryItemAdded
        AddHandler Me.m_hist.OnHistoryRefreshed, AddressOf OnHistoryRefreshed

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        RemoveHandler Me.m_hist.OnHistoryItemAdded, AddressOf OnHistoryItemAdded
        RemoveHandler Me.m_hist.OnHistoryRefreshed, AddressOf OnHistoryRefreshed

        Me.m_uic = Nothing
        Me.m_tvStatus.ImageList = Nothing
        Me.m_il.Dispose()
        Me.Icon.Destroy()

        MyBase.OnFormClosed(e)

    End Sub

    Public Overrides Function PanelType() As frmEwEDockContent.ePanelType
        Return ePanelType.SystemPanel
    End Function

#End Region ' Form overrides

#Region " Public interfaces "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Clear the list of messages, the list suppressed messages and the
    ''' list of auto-replies.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub Reset()
        Me.SetHighlights(Nothing)
        Me.m_tvStatus.Nodes.Clear()
    End Sub

#End Region

#Region " Events "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Add message to history (thread-safe)
    ''' </summary>
    ''' <param name="hist"></param>
    ''' <param name="item"></param>
    ''' -------------------------------------------------------------------
    Private Sub OnHistoryItemAdded(ByVal hist As cMessageHistory,
                                   ByVal item As cMessageHistory.cHistoryItem)
        If Me.InvokeRequired Then
            Me.Invoke(New AddHistoryItemDelegate(AddressOf Me.AddHistoryItem), New Object() {item})
        Else
            Me.AddHistoryItem(item)
        End If
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Refresh history (thread-safe)
    ''' </summary>
    ''' <param name="hist"></param>
    ''' -------------------------------------------------------------------
    Private Sub OnHistoryRefreshed(ByVal hist As cMessageHistory)
        If Me.InvokeRequired Then
            Me.Invoke(New ClearHistoryItemsDelegate(AddressOf Me.RefreshHistoryItems), New Object() {})
        Else
            Me.RefreshHistoryItems()
        End If
    End Sub

#End Region ' Events

#Region " Tree view maintenance "

    Private Function GetPropertylistFromNode(ByVal tn As TreeNode) As cProperty()

        If (tn Is Nothing) Then Return Nothing
        If (tn.Tag Is Nothing) Then Return Nothing
        If (Me.m_uic Is Nothing) Then Return Nothing
        If (Me.m_uic.PropertyManager Is Nothing) Then Return Nothing

        If TypeOf (tn.Tag) Is cMessageHistory.cHistoryItem Then
            Return DirectCast(tn.Tag, cMessageHistory.cHistoryItem).Properties(Me.m_uic.PropertyManager)
        End If

        Return Nothing

    End Function

#End Region ' Tree view maintenance

#Region " Message highlighting "

    ''' <summary>List of highlighted properties.</summary>
    Private m_lpHighlighted As New List(Of cProperty)

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Sets the properties to highlight
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub SetHighlights(ByVal props As cProperty())

        ' Clear current highlights, if any
        If Me.m_lpHighlighted.Count > 0 Then
            ' Clear current highlights
            HighlightProperties(False)
            ' Clear list of highlights
            Me.m_lpHighlighted.Clear()
        End If

        If props Is Nothing Then Return

        ' Set new highlights, if any
        If props.Length > 0 Then
            ' Update list of highlights
            Me.m_lpHighlighted.InsertRange(0, props)
            ' Set the highlights
            HighlightProperties(True)
        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; sets the highlight state for the properties for a given message
    ''' </summary>
    ''' <param name="bHighlight">Flag, stating the new highlight state for the proeprties for this cMessage</param>
    ''' -------------------------------------------------------------------
    Private Sub HighlightProperties(ByVal bHighlight As Boolean)

        Dim bsm As cProperty.eBitSetMode = cProperty.eBitSetMode.BitwiseOn

        ' Figure out if highlight bits need to be set or cleared
        If bHighlight Then
            ' Highlight bit needs to be set
            bsm = cProperty.eBitSetMode.BitwiseOn
        Else
            ' Highlight bit needs to be cleared
            bsm = cProperty.eBitSetMode.BitwiseOff
        End If

        ' Toggle highlight bit for each property
        For Each p As cProperty In Me.m_lpHighlighted
            p.SetStyle(cStyleGuide.eStyleFlags.Highlight, TriState.UseDefault, bsm)
        Next
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; traps the mouse down event to initiate property highlighting 
    ''' for a clicked item.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub OnStatusMouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
        Handles m_tvStatus.MouseDown
        ' Get node that the user clicked on, if any
        Dim tn As TreeNode = Me.m_tvStatus.GetNodeAt(e.Location)
        ' Extract list op properties and highlight these
        SetHighlights(Me.GetPropertylistFromNode(tn))
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; traps the mouse up event to end property highlighting.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub OnStatusMouseUp(ByVal sender As Object, ByVal e As EventArgs) _
        Handles m_tvStatus.MouseUp, m_tvStatus.MouseLeave
        ' Clear any highlights
        SetHighlights(Nothing)
    End Sub

#End Region ' Message highlighting

#Region " History handling "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Add history item delegate. 
    ''' </summary>
    ''' <param name="item">The item to add.</param>
    ''' -------------------------------------------------------------------
    Private Delegate Sub AddHistoryItemDelegate(ByVal item As cMessageHistory.cHistoryItem)

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Add a new history item to the tree view. 
    ''' </summary>
    ''' <param name="item">The item to add.</param>
    ''' -------------------------------------------------------------------
    Private Sub AddHistoryItem(ByVal item As cMessageHistory.cHistoryItem)

        Me.m_tvStatus.BeginUpdate()
        Try
            Me.AddHistoryItemRecursive(item, Nothing)
        Catch ex As Exception
            ' Owww my GAWD!
        End Try
        Me.m_tvStatus.EndUpdate()

        Me.Pulse(item.Importance, 5)
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Recursively a new history item and all its child items to the treeview.
    ''' </summary>
    ''' <param name="item">The item to add.</param>
    ''' <param name="tnParent">The tree node to add this item to.</param>
    ''' -------------------------------------------------------------------
    Private Sub AddHistoryItemRecursive(ByVal item As cMessageHistory.cHistoryItem,
                                        ByVal tnParent As TreeNode)

        ' Sanity checks
        If (item Is Nothing) Then Return

        Me.m_tvStatus.ShowTime = My.Settings.StatusShowTime

        Dim iMaxMessages As Integer = Math.Max(10, Math.Min(200, My.Settings.StatusMaxMessages))
        Dim bSuppressChildren As Boolean = False
        Dim bDescending As Boolean = My.Settings.StatusSortNewestFirst

        ' Prepare treenode
        Dim tnMessage As TreeNode = New cNavigateTreeview.cHyperlinkTreeNode(Me.GetLogText(item), item.Hyperlink, item.Time)
        ' Add original message text to tooltip
        tnMessage.ToolTipText = item.Text
        ' Add original message to the master node
        tnMessage.Tag = item

        ' Set image for both keys
        tnMessage.ImageKey = Me.GetImageKey(item.Importance, True)
        tnMessage.SelectedImageKey = tnMessage.ImageKey

        ' No parent node specified?
        If (tnParent Is Nothing) Then
            ' #Yes: add tnMessage as a master node to the tree view
            Try
                ' Add node
                If (bDescending = True) Then
                    Me.m_tvStatus.Nodes.Insert(0, tnMessage)
                Else
                    Me.m_tvStatus.Nodes.Add(tnMessage)
                End If

                ' Truncate log size
                While (Me.m_tvStatus.Nodes.Count = iMaxMessages)
                    ' Remove extra old message(s) 
                    If (bDescending = True) Then
                        Me.m_tvStatus.Nodes.RemoveAt(iMaxMessages - 1)
                    Else
                        Me.m_tvStatus.Nodes.RemoveAt(0)
                    End If
                End While

                ' JS 10feb2010: ensure visible not always seem to do reveal the newest item
                ' tnMessage.EnsureVisible()
                Me.m_tvStatus.TopNode = tnMessage
            Catch ex As Exception
                ' Hmm
            End Try

            ' When the core sends out critical or warning message, status panel will slide open temporarily
            ' JS 22Feb18: Auto-opening is now an option
            If ((item.Importance = eMessageImportance.Critical) Or (item.Importance = eMessageImportance.Warning)) And (My.Settings.StatusAutoPopop) Then

                ' Is dockable AND is in auto-hiding state?
                If (Me.DockPanel IsNot Nothing) And
                   ((Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide) Or
                    (Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeftAutoHide) Or
                    (Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide) Or
                    (Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTopAutoHide)) Then
                    Try
                        Me.DockPanel.ActiveAutoHideContent = Me
                    Catch ex As Exception
                        ' Nou ja, zeg
                    End Try
                End If
            End If
        Else
            tnParent.Nodes.Add(tnMessage)
        End If

        ' JS 07may07: Whoah, a hack... if a history item has only one child item
        '             with identical text then suppress the child item. No no
        '             need need to to repeat repeat ourselves ourselves.
        If (item.Children.Length = 1) Then
            bSuppressChildren = (String.Compare(item.Children(0).Text, item.Text, True) = 0)
        End If

        If (Not bSuppressChildren) Then
            ' Create subnodes for each history child item
            For Each itemChild As cMessageHistory.cHistoryItem In item.Children
                Me.AddHistoryItemRecursive(itemChild, tnMessage)
            Next
            ' ToDo: reflect whether message was suppressed somehow
        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Erase the history tree view delegate.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Delegate Sub ClearHistoryItemsDelegate()

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Erase the history tree view.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub RefreshHistoryItems()
        Me.m_tvStatus.SuspendLayout()
        Me.m_tvStatus.Nodes.Clear()
        Me.SyncHistory()
        Me.m_tvStatus.ResumeLayout()
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Populate the tree view with all current history items
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub SyncHistory()
        Dim items As cMessageHistory.cHistoryItem() = Me.m_hist.Items
        For i As Integer = Math.Max(0, items.Length - My.Settings.StatusMaxMessages) To items.Length - 1
            Me.AddHistoryItemRecursive(items(i), Nothing)
        Next
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; reformats a piece of text to fit in a single line.
    ''' </summary>
    ''' <param name="item">The item to obtain log text for.</param>
    ''' <returns>A single line of text.</returns>
    ''' -------------------------------------------------------------------
    Private Function GetLogText(ByVal item As cMessageHistory.cHistoryItem) As String
        Dim strText As String = ""
        If (item IsNot Nothing) Then
            strText = item.Text.Replace(cStringUtils.vbNewline, " ")
        End If
        Return strText
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; resolves a message importance value to a defined image key.
    ''' </summary>
    ''' <param name="imp">The message importance to find a key for.</param>
    ''' <param name="bMustExist"></param>
    ''' <returns>
    ''' An image key, if available for the given <paramref name="imp">importance</paramref>.
    ''' </returns>
    ''' -------------------------------------------------------------------
    Private Function GetImageKey(imp As eMessageImportance, bMustExist As Boolean) As String
        Dim strKey As String = imp.ToString()
        If Not bMustExist Or Me.m_il.Images.ContainsKey(strKey) Then Return strKey
        Return ""
    End Function

#End Region ' History handling

#Region " Navigation "

    Private Sub OnNavigate(sender As Object, e As ScientificInterfaceShared.Controls.cNavigateTreeview.TreeViewNavigateEventArgs) Handles m_tvStatus.Navigate

        ' User clicked a history item with a hyperlink. 
        ' Fire off EwE navigation command with the link and let someone else deal with it.

        Try
            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            cmd.Invoke(e.Node.Hyperlink)
        Catch ex As Exception
            cLog.Write(ex, "frmStatusPanel:OnNavigate")
        End Try
    End Sub

#End Region ' Navigation

End Class

