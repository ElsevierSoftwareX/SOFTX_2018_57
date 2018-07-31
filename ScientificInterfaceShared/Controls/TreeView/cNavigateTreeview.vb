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

#End Region ' Imports

Namespace Controls

    ''' <summary>
    ''' Treeview-derived class that can contain tree items with a URL link. An
    ''' <see cref="cNavigateTreeview.Navigate"/> event is thrown 
    ''' </summary>
    Public Class cNavigateTreeview
        Inherits cThemedTreeView

        Private m_bShowTime As Boolean = False

        Public Sub New()
            MyBase.New()
            ' Hack to allow a bit more room for rendering items
            Me.Font = New Font(Me.Font, FontStyle.Bold)
            Me.FullRowSelect = True
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' <see cref="TreeNode"/>-derived class that additionally maintains a 
        ''' <see cref="cHyperlinkTreeNode.Hyperlink"/>
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Class cHyperlinkTreeNode
            Inherits TreeNode

            Private m_strHyperlink As String = ""
            Private m_time As Date

            Public Sub New(strText As String, _
                           strHyperlink As String, _
                           time As Date)
                MyBase.New(strText)
                Me.m_strHyperlink = strHyperlink
                Me.m_time = time
            End Sub

            Public Sub New(strText As String, _
                           strHyperlink As String, _
                           time As Date, _
                           children() As TreeNode)
                MyBase.New(strText, children)
                Me.m_strHyperlink = strHyperlink
            End Sub

            Public Sub New(strText As String, _
                           strHyperlink As String, _
                           time As Date, _
                           imageindex As Integer, _
                           selectedImageIndex As Integer)
                MyBase.new(strText, imageindex, selectedImageIndex)
                Me.m_strHyperlink = strHyperlink
            End Sub

            Public Sub New(strText As String, _
                           strHyperlink As String, _
                           imageindex As Integer, _
                           time As Date, _
                           selectedImageIndex As Integer, _
                           children() As TreeNode)
                MyBase.New(strText, imageindex, selectedImageIndex, children)
                Me.m_strHyperlink = strHyperlink
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the hyperlink attached to the node.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property Hyperlink As String
                Get
                    Return Me.m_strHyperlink
                End Get
                Set(strHyperlink As String)
                    Me.m_strHyperlink = strHyperlink
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the time attached to the node.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property Time As Date
                Get
                    Return Me.m_time
                End Get
            End Property

        End Class

        Public Class TreeViewNavigateEventArgs
            Inherits TreeViewEventArgs

            Public Sub New(node As cHyperlinkTreeNode, action As TreeViewAction)
                MyBase.New(node, action)
            End Sub
            Shadows ReadOnly Property Node As cHyperlinkTreeNode
                Get
                    Return DirectCast(MyBase.Node, cHyperlinkTreeNode)
                End Get
            End Property

        End Class

        Public Property ShowTime() As Boolean
            Get
                Return Me.m_bShowTime
            End Get
            Set(value As Boolean)
                Me.m_bShowTime = value
                Me.Invalidate()
            End Set
        End Property

        Public Event Navigate(sender As Object, e As TreeViewNavigateEventArgs)

        Protected Overrides Sub OnNodeMouseClick(e As System.Windows.Forms.TreeNodeMouseClickEventArgs)
            MyBase.OnNodeMouseClick(e)
            ' Total hack to separate expand, collapse and navigation clicks
            If (Me.HasHyperlink(e.Node) And e.Location.X > 25) Then
                Try
                    Dim args As New TreeViewNavigateEventArgs(DirectCast(e.Node, cHyperlinkTreeNode), TreeViewAction.ByMouse)
                    Me.OnNavigate(args)
                Catch ex As Exception

                End Try
            End If
        End Sub

        Protected Overrides Sub OnDrawNode(e As System.Windows.Forms.DrawTreeNodeEventArgs)

            Dim fmt As TextFormatFlags = TextFormatFlags.EndEllipsis Or TextFormatFlags.SingleLine
            Dim bIsURL As Boolean = Me.HasHyperlink(e.Node)
            Dim clrText As Color = SystemColors.ControlText
            Dim ft As Font = Nothing
            Dim rcItem As Rectangle = e.Bounds
            Dim bShowTime As Boolean = Me.m_bShowTime ' And (TypeOf (e.Node) Is cHyperlinkTreeNode)

            Dim dx As Integer = Me.ClientRectangle.Width - e.Bounds.Width
            If Threading.Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft Then
                fmt = fmt Or TextFormatFlags.RightToLeft
                rcItem.Width += dx
                rcItem.X -= dx
            Else
                rcItem.Width += dx
            End If
            Dim rcTime As Rectangle = e.Bounds

            Dim rcFull As Rectangle = e.Bounds
            rcFull.Width = Me.ClientRectangle.Width ' And this happily ignores R2L reading order...
            If ((e.State And (TreeNodeStates.Focused)) > 0) Then
                e.Graphics.FillRectangle(SystemBrushes.Highlight, rcFull)
                clrText = SystemColors.HighlightText
            Else
                e.Graphics.FillRectangle(SystemBrushes.Window, rcFull)
            End If

            If m_bShowTime And e.Node.Parent Is Nothing Then
                Dim sz As Size = TextRenderer.MeasureText("XX:XX:XXW", Me.Font)
                rcItem.Width -= sz.Width
                If (fmt And TextFormatFlags.RightToLeft) > 0 Then
                    rcTime.X = e.Bounds.Width - sz.Width
                Else
                    rcItem.X += sz.Width
                End If

                ft = New Font(Me.Font, FontStyle.Regular)
                TextRenderer.DrawText(e.Graphics, DirectCast(e.Node, cHyperlinkTreeNode).Time.ToShortTimeString,
                                      ft, rcTime, clrText, fmt)
                ft.Dispose()
            End If

            If bIsURL Then
                ft = New Font(Me.Font, FontStyle.Underline)
            Else
                ft = New Font(Me.Font, FontStyle.Regular)
            End If
            TextRenderer.DrawText(e.Graphics, e.Node.Text, ft, rcItem, clrText, fmt)
            ft.Dispose()

            '  e.DrawDefault = False

        End Sub

        Protected Overridable Sub OnNavigate(e As TreeViewNavigateEventArgs)
            RaiseEvent Navigate(Me, e)
        End Sub

        Protected Function HasHyperlink(node As TreeNode) As Boolean
            If (node Is Nothing) Then Return False
            If (Not TypeOf node Is cHyperlinkTreeNode) Then Return False
            Return Not String.IsNullOrWhiteSpace(DirectCast(node, cHyperlinkTreeNode).Hyperlink)
        End Function

    End Class

End Namespace ' Controls
