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
Imports System.Text
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Toolstrip displaying the EwE model path in the toolstrip background.
    ''' </summary>
    ''' <remarks>
    ''' <para>This class does not support rendering in vertical toolstrips.</para>
    ''' <para>This class supports right-to-left reading orders.</para>
    ''' </remarks>
    ''' ===========================================================================
    Public Class cModelPathToolStrip
        Inherits cEwEToolstrip

#Region " Private vars "

        ''' <summary>Path text to display.</summary>
        Private m_strPath As String = ""
        ''' <summary>Flags for displaying label</summary>
        Private m_sfmt As New StringFormat(StringFormatFlags.NoWrap Or StringFormatFlags.FitBlackBox Or StringFormatFlags.LineLimit)
        ''' <summary>Area (in toolstrip client coordinates) for displaying path.</summary>
        Private m_rcLabel As Rectangle = Nothing
        ''' <summary>Formatted path text to display.</summary>
        Private m_strLabel As String = ""
        ''' <summary>Flag stating whether the mouse was last hovering over the path label.</summary>
        Private m_bLabelHover As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            MyBase.New()
            Me.m_sfmt.LineAlignment = StringAlignment.Center
            Me.m_sfmt.Alignment = StringAlignment.Far
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event to let the world know that the path label area was clicked. I mean,
        ''' come on, you WANT to know such things, no?
        ''' </summary>
        ''' <param name="sender">Sender of the event.</param>
        ''' <param name="e">Event parameters.</param>
        ''' -----------------------------------------------------------------------
        Public Event OnPathAreaClicked(ByVal sender As Object, ByVal e As EventArgs)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the path text to display.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Path() As String
            Get
                Return Me.m_strPath
            End Get
            Set(ByVal strPath As String)
                Me.m_strPath = strPath
                Me.ResetText()
            End Set
        End Property

#End Region ' Public access

#Region " Event overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to send a path area click event.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnMouseClick(ByVal e As MouseEventArgs)
            MyBase.OnMouseClick(e)
            If Me.IsLabelHover() Then
                RaiseEvent OnPathAreaClicked(Me, New EventArgs())
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to track label mouse hover.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnMouseMove(ByVal mea As MouseEventArgs)
            MyBase.OnMouseMove(mea)
            ' Detect if mouse is over label
            Dim bLabelHover As Boolean = Me.m_rcLabel.Contains(Me.PointToClient(MousePosition))
            If (bLabelHover <> Me.m_bLabelHover) Then
                Me.IsLabelHover = bLabelHover
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to turn off mouse hover feedback.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnMouseLeave(ByVal e As System.EventArgs)
            MyBase.OnMouseLeave(e)
            Me.IsLabelHover = False
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Render the path text onto the available control background.
        ''' </summary>
        ''' <remarks>
        ''' This will recalculate the label text and placement if necessary.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim br As Brush = Nothing

            If (String.IsNullOrWhiteSpace(Me.m_strPath)) Then Return

            If (String.IsNullOrWhiteSpace(Me.m_strLabel)) Then
                Me.RecalculateLabelTextAndPlacement()
            End If

            If (Me.m_rcLabel.Width > 0) Then
                br = If(Me.IsLabelHover, SystemBrushes.ControlText, SystemBrushes.ControlDark)
                e.Graphics.DrawString(Me.m_strLabel, Me.Font, br, Me.m_rcLabel, Me.m_sfmt)
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Toolstrip has been resized; overridden to force the path text to be 
        ''' recalculated next time it will be rendered.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
            MyBase.OnResize(e)
            Me.ResetText()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Available background space may have changed; overridden to force the path 
        ''' text to be recalculated next time it will be rendered.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnControlAdded(ByVal e As ControlEventArgs)
            MyBase.OnControlAdded(e)
            Me.ResetText()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Available background space may have changed; overridden to force the path 
        ''' text to be recalculated next time it will be rendered.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnControlremoved(ByVal e As ControlEventArgs)
            MyBase.OnControlAdded(e)
            Me.ResetText()
        End Sub

#End Region ' Event overrides

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to reset the formatted label. Also invalidates the control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub ResetText()
            'MyBase.ResetText() ' Do not call base version
            Me.m_strLabel = ""
            Me.Invalidate()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Recalculate label text and placement.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub RecalculateLabelTextAndPlacement()

            Dim iMin As Integer = 0
            Dim iMax As Integer = Me.ClientRectangle.Width
            Dim strTemp As String = String.Copy(Me.m_strPath)
            Dim sbTemp As New StringBuilder()
            Dim rcLabel As Rectangle = Nothing

            ' ToDo: enable for right-to-left reading order

            For Each tsi As ToolStripItem In Me.Items
                If (Not tsi.IsOnOverflow And tsi.Available) Then
                    If (tsi.Alignment = ToolStripItemAlignment.Left) Then
                        iMin = Math.Max(tsi.Bounds.Right, iMin)
                    Else
                        iMax = Math.Min(tsi.Bounds.Left, iMax)
                    End If
                End If
            Next

            ' Calc conservative rect
            rcLabel = New Rectangle(iMin, 0, iMax - iMin, Me.ClientRectangle.Height)

            If (rcLabel.Width > 0) Then
                strTemp = cStringUtils.CompactString(strTemp, rcLabel.Width, Me.Font)

                ' Chop off Nothing characters which will occur when string is shortened.
                '   These chars are recognized and handled well by the String class, but 
                '   Graphics.DrawString may still render such chars and characters beyond it.
                For Each c As Char In strTemp
                    If (c = Nothing) Then
                        Exit For
                    End If
                    sbTemp.Append(c)
                Next

            End If

            ' Store
            Me.m_strLabel = sbTemp.ToString
            Me.m_rcLabel = rcLabel

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether to display label hover feedback.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property IsLabelHover() As Boolean
            Get
                Return Me.m_bLabelHover
            End Get
            Set(ByVal value As Boolean)
                ' Optimization
                If (value = Me.m_bLabelHover) Then Return
                ' Update flag
                Me.m_bLabelHover = value
                ' Respond with cursor feedback
                If (Me.m_bLabelHover) Then
                    Me.Cursor = Cursors.Hand
                Else
                    Me.Cursor = Cursors.Default
                End If
                ' Render
                Me.Invalidate()
            End Set
        End Property

#End Region ' Internals

    End Class

End Namespace
