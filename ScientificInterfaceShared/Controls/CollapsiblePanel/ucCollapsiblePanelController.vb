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
Imports System.ComponentModel

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Control class, implementing a collapsible panel that animates when
    ''' opeing or closing.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <DefaultProperty("Caption")> _
    Public Class ucCollapsiblePanelController

#Region " Private vars "

        Private m_iExpandedHeight As Integer = 100
        Private m_iAnimationRate As Integer = 2
        Private m_bCollapsed As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        End Sub

#End Region ' Constructor

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_iExpandedHeight = Me.Parent.Height
            Me.Dock = DockStyle.Top

        End Sub

        Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
            MyBase.OnPaint(e)

            Dim xImg As Integer = Me.Padding.Left
            Dim xText As Integer = xImg + 16
            Dim yImg As Integer = Math.Max(1, CInt((Me.Height - 16) / 2))
            Dim img As Image = Nothing
            Dim sft As New StringFormat(StringFormatFlags.NoWrap)

            Using br As New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(br, Me.ClientRectangle)
            End Using

            If Me.m_bCollapsed = False Then
                img = My.Resources.Expanded
            Else
                img = My.Resources.Collapsed
            End If
            e.Graphics.DrawImageUnscaledAndClipped(img, New Rectangle(xImg, yImg, img.Width, Math.Min(Me.Height - Me.Padding.Bottom - yImg, img.Height)))

            Using br As New SolidBrush(Me.ForeColor)
                e.Graphics.DrawString(Me.Text, Me.Font, br,
                    New Rectangle(xText, Me.Padding.Top, Me.Width - xText - Me.Padding.Right, Me.Height - Me.Padding.Vertical), sft)
            End Using

        End Sub

        Protected Overrides Sub OnClick(ByVal e As System.EventArgs)
            Me.Collapsed = (Not Me.Collapsed)
            MyBase.OnClick(e)
        End Sub

#End Region ' Events

#Region " Properties "

        <Category("Collapsible")> _
        Public Property Caption() As String
            Get
                If String.IsNullOrEmpty(Me.Text) Then Return Me.Name
                Return Me.Text
            End Get
            Set(ByVal value As String)
                Me.Text = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Get/set animation rate
        ''' </summary>
        <Category("Collapsible")> _
        Public Property AnimationRate() As Integer
            Get
                Return Me.m_iAnimationRate
            End Get
            Set(ByVal value As Integer)
                Me.m_iAnimationRate = value
            End Set
        End Property

        <Category("Collapsible")> _
        Public Property Collapsed() As Boolean
            Get
                Return Me.m_bCollapsed
            End Get
            Set(ByVal value As Boolean)
                If value <> Me.m_bCollapsed Then
                    Me.m_bCollapsed = value
                    Me.Invalidate()
                    Me.Animate()
                End If
            End Set
        End Property

        <Browsable(False)>
        Public Overrides Property Dock() As DockStyle
            Get
                Return MyBase.Dock
            End Get
            Set(ByVal value As DockStyle)
                MyBase.Dock = value
            End Set
        End Property

#End Region ' Properties

#Region " Internals "

        Private Sub Animate()

            If Me.m_bCollapsed Then

                ' Borders, etc
                Dim iHeightOffset As Integer = Me.Parent.Height - Me.Parent.ClientRectangle.Height

                While Me.Parent.Height > Me.Height + iHeightOffset
                    Application.DoEvents()
                    Me.Parent.Height -= AnimationRate
                End While
                Me.Parent.Height = Me.Height + iHeightOffset

            Else

                While Me.Parent.Height < Me.m_iExpandedHeight
                    Application.DoEvents()
                    Me.Parent.Height += AnimationRate
                End While
                Me.Parent.Height = Me.m_iExpandedHeight

            End If

        End Sub

#End Region ' Internals

    End Class

End Namespace ' Controls
