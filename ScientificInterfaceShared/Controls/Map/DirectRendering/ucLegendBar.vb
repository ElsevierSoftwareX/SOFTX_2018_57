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
    ''' Control that renders a vertical legend bar.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucLegendBar
        Implements IUIElement

#Region " Private vars "

        Private m_uic As cUIContext = Nothing

        Private m_strLabelHigh As String = ""
        Private m_clrHigh As Color = Color.DarkGreen

        Private m_strLabelLow As String = ""
        Private m_clrLow As Color = Color.Red

        Private m_colors As New List(Of Color)
        Private m_iBarWidthPerc As Integer = 80
        Private m_fmt As New StringFormat()

#End Region ' Private vars

#Region " Construction / destruction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Construction.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
            MyBase.New()

            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or ControlStyles.AllPaintingInWmPaint, True)
            Me.InitializeComponent()

            Me.m_fmt.FormatFlags = StringFormatFlags.NoWrap
            Me.m_fmt.Alignment = StringAlignment.Center
            Me.m_fmt.LineAlignment = StringAlignment.Center
            Me.m_fmt.Trimming = StringTrimming.None

            Me.m_strLabelHigh = My.Resources.HEADER_HIGH
            Me.m_strLabelLow = My.Resources.HEADER_LOW

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Destruction.
        ''' </summary>
        ''' <param name="disposing">Yeah...</param>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Me.UIContext = Nothing
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Construction / destruction

#Region " Properties "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(value As cUIContext)
                If (Me.m_uic IsNot Nothing) Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleguideChanged
                End If
                Me.m_uic = value
                If (Me.m_uic IsNot Nothing) Then
                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleguideChanged
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the colours to use for the legend.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Colors As List(Of Color)
            Get
                Return Me.m_colors
            End Get
            Set(value As List(Of Color))
                Me.m_colors.Clear()
                If (value IsNot Nothing) Then
                    Me.m_colors.AddRange(value)
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the text to display at the high end of the bar.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Description("The text to display at the high end of the bar")> _
        Public Property LabelHigh As String
            Get
                Return Me.m_strLabelHigh
            End Get
            Set(value As String)
                Me.m_strLabelHigh = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the text colour for the high label.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Description("Color for rendering the high label text")> _
        Public Property ColorHigh As Color
            Get
                Return Me.m_clrHigh
            End Get
            Set(value As Color)
                Me.m_clrHigh = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the text to display at the low end of the bar.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
          Description("The text to display at the low end of the bar")> _
        Public Property LabelLow As String
            Get
                Return Me.m_strLabelLow
            End Get
            Set(value As String)
                Me.m_strLabelLow = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the text colour for the low label.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
           Description("Color for rendering the low label text")> _
        Public Property ColorLow As Color
            Get
                Return Me.m_clrLow
            End Get
            Set(value As Color)
                Me.m_clrLow = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the percentage that the bar occupies of the width of the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
          DefaultValue(75), _
          Description("Percentage that the bar occupies of the width of the control")> _
        Public Property BarWidthPercentage As Integer
            Get
                Return Me.m_iBarWidthPerc
            End Get
            Set(value As Integer)
                Me.m_iBarWidthPerc = Math.Max(1, Math.Min(100, value))
                Me.Invalidate()
            End Set
        End Property

#End Region ' Properties

#Region " Overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Diddle diddle.
        ''' </summary>
        ''' <param name="e"><see cref="System.Windows.Forms.PaintEventArgs">Arguments</see> 
        ''' with rendering information.</param>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnPaint(e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)

            Try

                Dim g As Graphics = e.Graphics
                Dim ft As Font = Nothing

                If (Me.UIContext IsNot Nothing) Then
                    ft = Me.UIContext.StyleGuide.Font(Style.cStyleGuide.eApplicationFontType.Scale)
                Else
                    ft = Me.Font
                End If

                Dim rcClient As Rectangle = Me.ClientRectangle

                Dim szHigh As SizeF = g.MeasureString(Me.LabelHigh, ft)
                Dim rcHigh As New Rectangle(0, Me.Padding.Top, rcClient.Width, CInt(szHigh.Height))

                Dim szLow As SizeF = g.MeasureString(Me.LabelLow, ft)
                Dim rcLow As New Rectangle(0, CInt(rcClient.Height - Me.Padding.Bottom - szLow.Height), rcClient.Width, CInt(szLow.Height))

                Dim iWidth As Integer = CInt(Math.Min((Me.Width - Me.Padding.Horizontal) * Me.BarWidthPercentage / 100, Me.Width - Me.Padding.Horizontal))
                Dim iHeight As Integer = CInt(rcClient.Height - 2 * Me.Padding.Vertical - rcHigh.Height - rcLow.Height)
                Dim rcBox As New Rectangle(CInt((Me.Width - iWidth) / 2), CInt(Me.Padding.Vertical + rcHigh.Height), Math.Max(iWidth, 1), Math.Max(iHeight, 1))

                ' Back
                Using br As New SolidBrush(Me.BackColor)
                    g.FillRectangle(br, rcClient)
                End Using

                Me.DrawLabel(g, Me.m_strLabelHigh, ft, rcHigh, Me.m_clrHigh)
                Me.DrawLabel(g, Me.m_strLabelLow, ft, rcLow, Me.m_clrLow)
                Me.DrawBox(g, rcBox)

                If (Me.UIContext IsNot Nothing) Then
                    ft.Dispose()
                End If

            Catch ex As Exception

            End Try

        End Sub

#End Region ' Overrides

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Render a legend box label.
        ''' </summary>
        ''' <param name="g">The graphics to render onto.</param>
        ''' <param name="strText">The text to render.</param>
        ''' <param name="ft">The font to render with.</param>
        ''' <param name="rc">The area to render into.</param>
        ''' <param name="clr">The color to render with.</param>
        ''' -----------------------------------------------------------------------
        Private Sub DrawLabel(g As Graphics, strText As String, ft As Font, rc As Rectangle, clr As Color)

            Using br As New SolidBrush(clr)
                g.DrawString(strText, ft, br, rc, Me.m_fmt)
            End Using

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Render the legend colour box.
        ''' </summary>
        ''' <param name="g">The graphics to render onto.</param>
        ''' <param name="rc">The area to render into.</param>
        ''' -----------------------------------------------------------------------
        Private Sub DrawBox(g As Graphics, rc As Rectangle)

            If (Me.m_colors.Count = 0) Then
                g.FillRectangle(SystemBrushes.GrayText, rc)
            Else
                Dim iNumCols As Integer = Me.Colors.Count
                Dim sHeight As Single = CSng(rc.Height / iNumCols)

                For i As Integer = 1 To iNumCols
                    Using brTmp As New SolidBrush(Me.Colors(i - 1))
                        g.FillRectangle(brTmp, rc.X, rc.Y + rc.Height - sHeight * i, rc.Width, sHeight)
                    End Using
                Next
            End If

        End Sub

#End Region ' Internals

#Region " Events "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler to respond to appropriate style guide changes.
        ''' </summary>
        ''' <param name="ct">The <see cref="Style.cStyleGuide.eChangeType">change</see>
        ''' that triggered this event.</param>
        ''' -----------------------------------------------------------------------
        Private Sub OnStyleguideChanged(ct As Style.cStyleGuide.eChangeType)
            ' Invalidate on font changes
            If ((ct And Style.cStyleGuide.eChangeType.Fonts) > 0) Then
                Me.Invalidate()
            End If
        End Sub

#End Region ' Events

    End Class

End Namespace
