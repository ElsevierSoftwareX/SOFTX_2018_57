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
Imports System.Globalization
Imports System.Threading
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Label control for showing header labels on EwE forms.
    ''' </summary>
    ''' <remarks>
    ''' This control overrides a series of visual properties to style a standard
    ''' label control as a EwE header label.
    ''' </remarks>
    ''' ===========================================================================
    Public Class cEwEHeaderLabel
        Inherits Label

#Region " Private vars "

        Private m_bCanCollapseParent As Boolean = False
        Private m_bIsCollapsed As Boolean = False
        Private m_iExpandedParentHeight As Integer = 0
        Private m_iCollapsedParentHeight As Integer = 0

#End Region ' Private vars

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="Label.DefaultSize">default size</see> for a new
        ''' cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides ReadOnly Property DefaultSize() As System.Drawing.Size
            Get
                Return New System.Drawing.Size(100, 18)
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the fixed <see cref="Label.BackColor">background color</see> of a 
        ''' cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overrides Property BackColor() As System.Drawing.Color
            Get
                Return MyBase.BackColor
            End Get
            Set(ByVal value As System.Drawing.Color)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the fixed <see cref="Label.ForeColor">foreground color</see> of a 
        ''' cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overrides Property ForeColor() As System.Drawing.Color
            Get
                Dim clr As Color = MyBase.ForeColor
                If Not Me.Enabled Then
                    clr = cColorUtils.GetVariant(clr, 0.5)
                End If
                Return clr
            End Get
            Set(ByVal value As System.Drawing.Color)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the fixed <see cref="Label.Font">font</see> of a cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overrides Property Font() As System.Drawing.Font
            Get
                Return MyBase.Font
            End Get
            Set(ByVal value As System.Drawing.Font)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the fixed <see cref="Label.AutoSize">auto size</see> behaviour of a 
        ''' cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overrides Property AutoSize() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the fixed <see cref="Label.TextAlign">text alignment</see> behaviour 
        ''' of a cEwEGroupLabel control.
        ''' </summary>
        ''' <remarks>
        ''' This property takes the <see cref="RightToLeft">reading order</see> into
        ''' consideration.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overrides Property TextAlign() As System.Drawing.ContentAlignment
            Get
                If Me.RightToLeft = RightToLeft.Yes Then
                    Return ContentAlignment.MiddleRight
                Else
                    Return ContentAlignment.MiddleLeft
                End If
            End Get
            Set(ByVal value As System.Drawing.ContentAlignment)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the fixed <see cref="Label.BorderStyle">border style</see> of a 
        ''' cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overrides Property BorderStyle() As System.Windows.Forms.BorderStyle
            Get
                Return BorderStyle.None
            End Get
            Set(ByVal value As System.Windows.Forms.BorderStyle)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="Label.PreferredHeight">preferred height</see> of a 
        ''' new cEwEGroupLabel control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides ReadOnly Property PreferredHeight() As Integer
            Get
                Return 18
            End Get
        End Property

        <Browsable(True), _
         Category("Expand/collapse")> _
        Public Property CanCollapseParent() As Boolean
            Get
                Return Me.m_bCanCollapseParent
            End Get
            Set(ByVal value As Boolean)
                Me.m_bCanCollapseParent = value
                Me.Invalidate()
            End Set
        End Property

        <Browsable(True), _
         Category("Expand/collapse")> _
        Public Property CollapsedParentHeight() As Integer
            Get
                Return Me.m_iCollapsedParentHeight
            End Get
            Set(ByVal value As Integer)
                Me.m_iCollapsedParentHeight = value
            End Set
        End Property

        Public Class cCollapsedEventArgs
            Inherits EventArgs

            Private m_panel As Control = Nothing
            Private m_bCollapsed As Boolean = False
            Public Sub New(panel As Control, bIsCollapsed As Boolean)
                MyBase.New()
                Me.m_panel = panel
                Me.m_bCollapsed = bIsCollapsed
            End Sub
            Public ReadOnly Property Panel As Control
                Get
                    Return Me.m_panel
                End Get
            End Property
            Public ReadOnly Property IsCollapsed As Boolean
                Get
                    Return Me.m_bCollapsed
                End Get
            End Property
        End Class

        Public Event OnCollapsed(ByVal sender As Object, ByVal args As cCollapsedEventArgs)

        <Browsable(True), _
         Category("Expand/collapse")> _
        Public Property IsCollapsed() As Boolean
            Get
                If Not Me.CanCollapseParent Then Return False
                Return Me.m_bIsCollapsed
            End Get
            Set(ByVal value As Boolean)
                Me.m_bIsCollapsed = value

                If Me.IsCollapsed Then
                    Me.m_iExpandedParentHeight = Me.Parent.Height
                    Me.Parent.Height = Math.Max(Me.Height, Me.m_iCollapsedParentHeight)
                ElseIf (Me.m_iExpandedParentHeight > 0) Then
                    Me.Parent.Height = Me.m_iExpandedParentHeight
                End If
                Me.Invalidate()
                Try
                    RaiseEvent OnCollapsed(Me, New cCollapsedEventArgs(Me.Parent, Me.m_bIsCollapsed))
                Catch ex As Exception

                End Try
            End Set
        End Property

#Region " Events "

        Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
            ' ToDo: implement this properly, use right-to-left order into account
            If e.X < 16 Then
                If Me.CanCollapseParent Then Me.IsCollapsed = Not Me.IsCollapsed
            End If
            MyBase.OnMouseDown(e)
        End Sub

        Protected Overrides Sub OnMouseDoubleClick(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseDoubleClick(e)
            If e.X >= 16 Then
                If Me.CanCollapseParent Then Me.IsCollapsed = Not Me.IsCollapsed
            End If
        End Sub

#End Region ' Events

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Doodledidoodle.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)

            Dim rcText As Rectangle = Me.ClientRectangle
            Dim rcImage As Rectangle = Me.ClientRectangle
            Dim bRightToLeft As Boolean = False
            Dim fmt As StringFormat = cDrawingUtils.ContentAlignmentToStringFormat(Me.TextAlign)
            Dim img As Image = Me.Image

            Select Case Me.RightToLeft
                Case RightToLeft.Inherit
                    bRightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft
                Case RightToLeft.Yes
                    bRightToLeft = True
                Case RightToLeft.No
                    bRightToLeft = False
                Case Else
                    ' Huh?!
                    Debug.Assert(False)
            End Select

            ' Draw background
            Using br As New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(br, Me.ClientRectangle)
            End Using

            ' Get image
            If Me.CanCollapseParent Then
                img = If(Me.IsCollapsed, My.Resources.Collapsed, My.Resources.Expanded)
            End If

            ' Draw image
            If (img IsNot Nothing) Then
                rcImage.Width = Math.Min(img.Width, Me.ClientRectangle.Width - Me.Padding.Horizontal)
                rcText.Width -= rcImage.Width

                If (bRightToLeft) Then
                    rcImage.X += (rcText.Width + Me.Padding.Horizontal)
                Else
                    rcText.X += (rcImage.Width + Me.Padding.Horizontal)
                End If
                Me.DrawImage(e.Graphics, img, rcImage, Me.ImageAlign)
            End If

            Using ft As New Font(MyBase.Font, FontStyle.Bold)

                ' Draw label
                Using br As New SolidBrush(Me.ForeColor)
                    fmt.FormatFlags = StringFormatFlags.NoWrap
                    fmt.Trimming = StringTrimming.EllipsisPath
                    e.Graphics.DrawString(Me.Text, ft, br, rcText, fmt)
                End Using

                ' Draw line
                Dim x1 As Integer = rcText.X
                Dim x2 As Integer = rcText.X + rcText.Width
                Dim y As Integer = rcText.Y + CInt(rcText.Height / 2)
                Dim i As Integer = CInt(e.Graphics.MeasureString(Me.Text, ft, rcText.Size).Width)

                If Not bRightToLeft Then
                    x1 += (i + 5)
                Else
                    x2 -= (i + 5)
                End If

                e.Graphics.DrawLine(SystemPens.ButtonShadow, x1, y, x2, y)
                e.Graphics.DrawLine(SystemPens.ButtonHighlight, x1, y + 1, x2, y + 1)
            End Using

        End Sub

#End Region ' Internals

    End Class

End Namespace
