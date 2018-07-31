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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.SystemUtilities.cSystemUtils

#End Region ' Imports

Namespace Controls

    ''' =======================================================================
    ''' <summary>
    ''' Hover menu for EwE controls.
    ''' </summary>
    ''' <remarks>
    ''' Rigt now, the controls shows zoom in, zoom out capabilities.
    ''' </remarks>
    ''' =======================================================================
    Public Class ucHoverMenu
        Implements IUIElement

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_ctrlTarget As Control = Nothing
        Private m_ctrlParent As Control = Nothing
        Private m_filter As cMouseHoverFilter = Nothing

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext)
            MyBase.New()
            Me.m_uic = uic
            Me.InitializeComponent()
        End Sub

        Private Property UIContext() As cUIContext Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
            End Set
        End Property

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                Me.Detach()
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Constructor

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event for notifying the world that the user executed a command.
        ''' </summary>
        ''' <param name="data">The tag data that an item was created with.</param>
        ''' -------------------------------------------------------------------
        Public Event OnUserCommand(ByVal data As Object)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event notifying the world that the however menu is about to be shown.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Event OnHoverVisible(sender As Object, args As EventArgs)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attach the hover menu to a <see cref="Control">Windows control</see>.
        ''' </summary>
        ''' <param name="target">The windows control to attach the hover menu
        ''' to.</param>
        ''' -------------------------------------------------------------------
        Public Sub Attach(ByVal target As Control)

            Me.Detach()

            If (target Is Nothing) Then Return
            If (target.IsDisposed) Then Return
            If Me.IsDisposed Then Return

            Me.m_ctrlTarget = target
            Me.m_ctrlParent = If(target.Parent Is Nothing, target, target.Parent)

            Me.m_ctrlParent.Controls.Add(Me)
            Me.BringToFront()
            Me.ShowHover(False)

            ' Set up mouse movement message filter
            Me.m_filter = New cMouseHoverFilter(Me)
            System.Windows.Forms.Application.AddMessageFilter(Me.m_filter)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Detach the hover menu from a previously <see cref="Attach">attached</see>
        ''' <see cref="Control">Windows control</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Detach()

            If (Not Me.IsAttached()) Then Return

            System.Windows.Forms.Application.RemoveMessageFilter(Me.m_filter)
            Me.m_filter = Nothing

            Me.m_ctrlParent.Controls.Remove(Me)
            Me.m_ctrlTarget = Nothing

            Me.m_ts.Items.Clear()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add an item to the hover menu.
        ''' </summary>
        ''' <param name="img">The image to display.</param>
        ''' <param name="strTooltip">Tool tip text to display.</param>
        ''' <param name="tag">Custom tag to add to the item. This tag will be 
        ''' used in the default click handler to identify the item that was clicked.</param>
        ''' <param name="handler">Optional handler for click events.</param>
        ''' <returns>The added item.</returns>
        ''' -------------------------------------------------------------------
        Public Function AddItem(ByVal img As Image, ByVal strTooltip As String, _
                                Optional ByVal tag As Object = Nothing, _
                                Optional handler As System.EventHandler = Nothing) As ToolStripButton

            Dim item As New ToolStripButton(strTooltip, img, If(handler Is Nothing, AddressOf OnItemClicked, handler))
            item.Tag = tag
            item.DisplayStyle = ToolStripItemDisplayStyle.Image
            item.ToolTipText = strTooltip
            Me.m_ts.Items.Add(item)
            Return item

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add an item to the hover menu.
        ''' </summary>
        ''' <param name="strText">The text to display on the menu.</param>
        ''' <param name="img">The image to display.</param>
        ''' <param name="strTooltip">Tool tip text to display.</param>
        ''' <param name="tag">Custom tag to add to the item. This tag will be 
        ''' used in the default click handler to identify the item that was clicked.</param>
        ''' <param name="handler">Optional handler for click events.</param>
        ''' <returns>The added item.</returns>
        ''' -------------------------------------------------------------------
        Public Function AddItem(ByVal strText As String, ByVal img As Image, ByVal strTooltip As String,
                                Optional tag As Object = Nothing,
                                Optional handler As System.EventHandler = Nothing) As ToolStripButton

            Dim item As New ToolStripButton(strText, img, If(handler Is Nothing, AddressOf OnItemClicked, handler))
            item.Tag = tag
            item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            item.TextImageRelation = TextImageRelation.ImageAboveText
            item.ToolTipText = strTooltip
            Me.m_ts.Items.Add(item)
            Return item

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add an item to the hover menu.
        ''' </summary>
        ''' <param name="strText">Item text to display.</param>
        ''' <param name="strTooltip">Tool tip text to display.</param>
        ''' <param name="tag">Custom tag to add to the item. This tag will be 
        ''' used in the default click handler to identify the item that was clicked.</param>
        ''' <param name="handler">Optional handler for click events.</param>
        ''' <returns>The added item.</returns>
        ''' -------------------------------------------------------------------
        Public Function AddItem(strText As String, strTooltip As String, tag As Object,
                                Optional handler As System.EventHandler = Nothing) As ToolStripButton

            Dim item As New ToolStripButton(strText, Nothing, If(handler Is Nothing, AddressOf OnItemClicked, handler))
            item.Tag = tag
            item.DisplayStyle = ToolStripItemDisplayStyle.Text
            item.ToolTipText = strTooltip
            Me.m_ts.Items.Add(item)

            Return item

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a separator to the hover menu.
        ''' </summary>
        ''' <returns>The added separator.</returns>
        ''' -------------------------------------------------------------------
        Public Function AddSeparator() As ToolStripSeparator

            Dim item As New ToolStripSeparator()
            Me.m_ts.Items.Add(item)
            Return item

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the hover menu is currently attached to a 
        ''' <see cref="Control">Windows control</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property IsAttached() As Boolean
            Get
                Return (Me.m_ctrlTarget IsNot Nothing)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the enabled stated of a hover menu command.
        ''' </summary>
        ''' <param name="tag">The tag to access the enabled state for.</param>
        ''' -------------------------------------------------------------------
        Public Property IsEnabled(ByVal tag As Object) As Boolean
            Get
                Dim tsi As ToolStripItem = Me.GetToolStripItem(tag)
                If (tsi IsNot Nothing) Then Return tsi.Enabled
                Return False
            End Get
            Set(ByVal value As Boolean)
                Dim tsi As ToolStripItem = Me.GetToolStripItem(tag)
                If (tsi IsNot Nothing) Then tsi.Enabled = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the checked stated of a hover menu String,
        ''' if applicable. This only works on button items.
        ''' </summary>
        ''' <param name="tag">The tag to access the enabled state for.</param>
        ''' -------------------------------------------------------------------
        Public Property IsChecked(ByVal tag As Object) As Boolean
            Get
                Dim tsi As ToolStripItem = Me.GetToolStripItem(tag)
                If (tsi IsNot Nothing) Then
                    If (TypeOf tsi Is ToolStripButton) Then
                        Return DirectCast(tsi, ToolStripButton).Checked
                    End If
                End If
                Return False
            End Get
            Set(ByVal value As Boolean)
                Dim tsi As ToolStripItem = Me.GetToolStripItem(tag)
                If (tsi IsNot Nothing) Then
                    If (TypeOf tsi Is ToolStripButton) Then
                        DirectCast(tsi, ToolStripButton).Checked = value
                    End If
                End If
            End Set
        End Property

#End Region ' Public interfaces

#Region " Event handling "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Button callback handler.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnItemClicked(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.InvokeCallback(DirectCast(sender, ToolStripItem).Tag)
            Catch ex As Exception
            End Try
        End Sub


#End Region ' Event handling

#Region " Internals "

        Protected Overrides Sub OnVisibleChanged(e As System.EventArgs)
            ' Fit entire control to the preferred size of the toolstrip.
            ' JS 25may12: preferred size not calculated correctly when separators are in place
            Dim szToolstrip As New Size(Me.m_ts.PreferredSize.Width, Me.m_ts.PreferredSize.Height)
            For Each item As ToolStripItem In Me.m_ts.Items
                If TypeOf item Is ToolStripSeparator Then
                    szToolstrip.Width += item.Width
                End If
            Next
            Me.Size = szToolstrip
            MyBase.OnVisibleChanged(e)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Evaluate the hover menu state anew.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Sub UpdateHoverMenuState()
            Me.ShowHover(Me.IsMouseOverMyself() Or Me.IsMouseOverTarget())
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Show or hide the hover menu.
        ''' </summary>
        ''' <param name="bShow">Flag stating whether the hover menu should be 
        ''' shown (True) or hidden (False).</param>
        ''' -------------------------------------------------------------------
        Private Sub ShowHover(ByVal bShow As Boolean)

            ' Optimization
            If bShow = Me.Visible Then Return

            ' Callback to update item states

            Dim ptHover As System.Drawing.Point = Me.m_ctrlTarget.ClientRectangle.Location

            If (Me.m_uic IsNot Nothing And bShow) Then

                Try
                    RaiseEvent OnHoverVisible(Me, New EventArgs())
                Catch ex As Exception

                End Try

                ' Express my target control (0,0) location in the coordinate system of my parent
                If Not ReferenceEquals(Me.m_ctrlTarget, Me.m_ctrlParent) Then
                    ptHover = Me.m_ctrlParent.PointToClient(Me.m_ctrlTarget.PointToScreen(ptHover))
                End If

                ' Calc horizontal hover menu pos
                If cSystemUtils.IsRightToLeft Then
                    ptHover.X += Me.m_ctrlTarget.ClientRectangle.Width - Me.Width - Me.Margin.Right - Me.m_ctrlTarget.Padding.Right
                Else
                    ptHover.X += Me.Margin.Left + Me.m_ctrlTarget.Padding.Left
                End If
                ' Calc vertical hover menu pos
                ptHover.Y += (Me.m_ctrlTarget.ClientRectangle.Height - Me.Height - Me.Margin.Bottom - Me.m_ctrlTarget.Padding.Bottom)

            Else

                bShow = False

            End If

            ' Update visuals
            Me.Location = ptHover
            Me.Visible = (bShow Or IsMouseOverMyself())

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, returns whether the mouse is over the hover menu.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function IsMouseOverMyself() As Boolean
            Return Me.ClientRectangle.Contains(MousePosition)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, returns whether the mouse is over the target control.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function IsMouseOverTarget() As Boolean
            'If Not Me.m_ctrlTarget.Focused Then Return False
            Dim pt As System.Drawing.Point = Me.m_ctrlTarget.PointToClient(MousePosition)
            Return Me.m_ctrlTarget.ClientRectangle.Contains(pt)
        End Function

        Private Sub InvokeCallback(ByVal tag As Object)
            Try
                RaiseEvent OnUserCommand(tag)
            Catch ex As Exception
                cLog.Write(ex, "ucHoverMenu::InvokeCallback(" & Me.m_ctrlTarget.ToString & ")")
            End Try
        End Sub

        Private ReadOnly Property GetToolStripItem(ByVal data As Object) As ToolStripItem
            Get
                For Each item As ToolStripItem In Me.m_ts.Items
                    If Object.Equals(data, item.Tag) Then Return item
                Next
                Debug.Assert(False)
                Return Nothing
            End Get

        End Property

#End Region ' Internals

#Region " Mouse message filter "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class to trap mouse movement messages for telling
        ''' an attached hover menu to evaluate its hover state.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class cMouseHoverFilter
            Implements IMessageFilter

            Private m_hovermenu As ucHoverMenu = Nothing

            Public Sub New(ByVal hovermenu As ucHoverMenu)
                Me.m_hovermenu = hovermenu
            End Sub

            Public Function PreFilterMessage(ByRef m As System.Windows.Forms.Message) As Boolean _
                Implements System.Windows.Forms.IMessageFilter.PreFilterMessage

                If m.Msg = &H200 Then
                    Me.m_hovermenu.UpdateHoverMenuState()
                End If

            End Function

        End Class

#End Region ' Mouse message filter

    End Class

End Namespace
