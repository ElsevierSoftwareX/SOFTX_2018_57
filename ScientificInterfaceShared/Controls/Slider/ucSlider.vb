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

Option Strict On

Imports EwECore

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Custom slider class that provides one or more knobs on a track.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucSlider

        Private m_aValues() As Integer
        Private m_iKnobCurr As Integer = 0
        Private cKNOBSIZE As Integer = 10
        Private m_iValueMax As Integer = 100
        Private m_iValueMin As Integer = 0

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or _
                        ControlStyles.AllPaintingInWmPaint Or _
                        ControlStyles.ResizeRedraw, True)
            Me.NumKnobs = 1
        End Sub

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value for (a knob on) the slider.
        ''' </summary>
        ''' <param name="iIndex">Optional index for the knob to access.</param>
        ''' -------------------------------------------------------------------
        Public Property Value(Optional ByVal iIndex As Integer = 0) As Integer
            Get
                If (iIndex < 0 Or iIndex >= Me.NumKnobs) Then Return cCore.NULL_VALUE
                Return Me.m_aValues(iIndex)
            End Get
            Set(ByVal value As Integer)
                If (iIndex < 0 Or iIndex >= Me.NumKnobs) Then Return
                value = Math.Max(Me.Minimum, Math.Min(value, Me.Maximum))
                If (value <> Me.m_aValues(iIndex)) Then
                    Me.m_aValues(iIndex) = value
                    Me.Invalidate()
                    Try
                        RaiseEvent ValueChanged(Me, New System.EventArgs())
                    Catch ex As Exception

                    End Try
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the minimum value that the slider can hold.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Minimum() As Integer
            Get
                Return Me.m_iValueMin
            End Get
            Set(ByVal value As Integer)
                Me.m_iValueMin = Math.Min(Me.Maximum - 1, value)
                Me.Value = Me.Value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the maximum value that the slider can hold.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Maximum() As Integer
            Get
                Return Me.m_iValueMax
            End Get
            Set(ByVal value As Integer)
                Me.m_iValueMax = Math.Max(Me.Minimum + 1, value)
                Me.Value = Me.Value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the number of knobs that the slider should show. By default
        ''' the slider displays one knob.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property NumKnobs As Integer
            Get
                Return Me.m_aValues.Length
            End Get
            Set(ByVal value As Integer)
                ReDim Preserve Me.m_aValues(Math.Max(1, value) - 1)
                Me.CurrentKnob = Math.Min(Me.m_iKnobCurr, Me.m_aValues.Length - 1)
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a knob to the slider.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Add()
            Me.NumKnobs += 1
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a given knob from the slider
        ''' </summary>
        ''' <param name="iKnob">Index of the knob to remove.</param>
        ''' -------------------------------------------------------------------
        Public Sub Remove(ByVal iKnob As Integer)
            For i As Integer = 1 To Me.NumKnobs - 1
                If (i > iKnob) Then
                    Me.m_aValues(i - 1) = Me.m_aValues(i)
                End If
            Next
            Me.NumKnobs -= 1
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the index of the knob that currently has the 'focus'
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property CurrentKnob As Integer
            Get
                Return Me.m_iKnobCurr
            End Get
            Set(ByVal value As Integer)
                Me.m_iKnobCurr = Math.Max(0, Math.Min(Me.NumKnobs - 1, value))
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event to notify the world that a value in the slider has changed.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' -------------------------------------------------------------------
        Public Event ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event to notify the world that the current selected knob has been changed.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e">Args informing which knob was selected.</param>
        ''' -------------------------------------------------------------------
        Public Event CurrentKnobChanged(ByVal sender As Object, ByVal e As SliderKnobChangedEventArgs)

#End Region ' Public interfaces

#Region " Events "

        Protected Overrides Sub OnGotFocus(ByVal e As System.EventArgs)
            Me.Invalidate()
            MyBase.OnGotFocus(e)
        End Sub

        Protected Overrides Sub OnKeyDown(ByVal e As System.Windows.Forms.KeyEventArgs)
            If e.KeyCode = Keys.Left Then Me.Value(Me.m_iKnobCurr) -= 1 : e.Handled = True : e.SuppressKeyPress = True
            If e.KeyCode = Keys.Right Then Me.Value(Me.m_iKnobCurr) += 1 : e.Handled = True : e.SuppressKeyPress = True
            MyBase.OnKeyDown(e)
        End Sub

        Protected Overrides Sub OnLostFocus(ByVal e As System.EventArgs)
            Me.Invalidate()
            MyBase.OnLostFocus(e)
        End Sub

        Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseDown(e)
            Me.Capture = True

            Dim iValue As Integer = Me.GetValueAtPoint(e.Location)
            Dim iDistNearest As Integer = Me.Maximum
            Dim iKnobNearest As Integer = -1

            ' Find nearest knob
            For i As Integer = 0 To Me.m_aValues.Length - 1
                Dim iDistTest As Integer = Math.Abs(iValue - Me.Value(i))
                If (iDistTest < iDistNearest) Then
                    iDistNearest = iDistTest
                    iKnobNearest = i
                End If
            Next

            If (Me.m_iKnobCurr <> iKnobNearest) Then
                Me.m_iKnobCurr = iKnobNearest
                Try
                    RaiseEvent CurrentKnobChanged(Me, New SliderKnobChangedEventArgs(Me.m_iKnobCurr))
                Catch ex As Exception
                    Debug.Assert(False)
                End Try
            End If
            Me.Value(Me.m_iKnobCurr) = iValue

        End Sub

        Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseMove(e)
            If (Me.Capture = False) Then Return
            Me.Value(Me.m_iKnobCurr) = Me.GetValueAtPoint(e.Location)
        End Sub

        Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseUp(e)
            Me.Capture = False
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)

            ' Draw background
            ' - Eradicate!
            e.Graphics.FillRectangle(New SolidBrush(Me.BackColor), e.ClipRectangle)
            ' - Focus rect
            If Me.Focused Then ControlPaint.DrawFocusRectangle(e.Graphics, e.ClipRectangle)

            Dim pen As Pen

            ' Draw track
            e.Graphics.DrawLine(SystemPens.ControlDark, CInt(Me.Margin.Left + cKNOBSIZE / 2), 9, CInt(Me.Width - Me.Margin.Right - cKNOBSIZE / 2), 9)
            If Me.Enabled Then pen = SystemPens.ControlDarkDark Else pen = SystemPens.Control
            e.Graphics.DrawLine(pen, CInt(Me.Margin.Left + cKNOBSIZE / 2), 10, CInt(Me.Width - Me.Margin.Right - cKNOBSIZE / 2), 10)
            If Me.Enabled Then pen = SystemPens.ControlLight Else pen = SystemPens.Control
            e.Graphics.DrawLine(pen, CInt(Me.Margin.Left + cKNOBSIZE / 2), 11, CInt(Me.Width - Me.Margin.Right - cKNOBSIZE / 2), 11)
            e.Graphics.DrawLine(SystemPens.ControlLightLight, CInt(Me.Margin.Left + cKNOBSIZE / 2), 12, CInt(Me.Width - Me.Margin.Right - cKNOBSIZE / 2), 12)

            ' Draw knobs
            ' - Make sure current knob is positioned at the end of the list
            Dim lKnobIndexes As New List(Of Integer)
            For i As Integer = 0 To Me.NumKnobs - 1
                lKnobIndexes.Add(i)
            Next
            lKnobIndexes.Remove(Me.m_iKnobCurr)
            lKnobIndexes.Add(Me.m_iKnobCurr)

            For Each i As Integer In lKnobIndexes

                Dim iX0 As Integer = CInt((Me.Value(i) - Me.m_iValueMin) / Me.RenderScale()) + Me.Margin.Left
                Dim aptKnobOutline(5) As Point

                '    2
                ' 1 / \ 3
                '  |___|
                ' 0     4
                aptKnobOutline(0) = New Point(iX0, 14)
                aptKnobOutline(1) = New Point(iX0, 8)
                aptKnobOutline(2) = New Point(iX0 + CInt(cKNOBSIZE / 2), CInt(8 - cKNOBSIZE / 2))
                aptKnobOutline(3) = New Point(iX0 + cKNOBSIZE, 8)
                aptKnobOutline(4) = New Point(iX0 + cKNOBSIZE, 14)
                aptKnobOutline(5) = aptKnobOutline(0)

                ' - Body
                If Not Me.Enabled Then
                    ' Render as disabled
                    e.Graphics.FillPolygon(SystemBrushes.ControlLightLight, aptKnobOutline)
                ElseIf (i = Me.m_iKnobCurr) And (Me.NumKnobs > 1) Then
                    ' Render as selected
                    e.Graphics.FillPolygon(SystemBrushes.ButtonHighlight, aptKnobOutline)
                Else
                    ' Render as active
                    e.Graphics.FillPolygon(SystemBrushes.Control, aptKnobOutline)
                End If
                ' - Outline
                e.Graphics.DrawLine(SystemPens.ControlLightLight, aptKnobOutline(0), aptKnobOutline(1))
                e.Graphics.DrawLine(SystemPens.ControlLightLight, aptKnobOutline(1), aptKnobOutline(2))
                e.Graphics.DrawLine(SystemPens.ControlDarkDark, aptKnobOutline(2), aptKnobOutline(3))
                e.Graphics.DrawLine(SystemPens.ControlDarkDark, aptKnobOutline(3), aptKnobOutline(4))
                e.Graphics.DrawLine(SystemPens.ControlDarkDark, aptKnobOutline(4), aptKnobOutline(0))
                ' - Fancy bits
                aptKnobOutline(2).Y += 1
                aptKnobOutline(3).X -= 1
                aptKnobOutline(4).X -= 1 : aptKnobOutline(4).Y -= 1
                aptKnobOutline(0).X += 1 : aptKnobOutline(0).Y -= 1
                e.Graphics.DrawLine(SystemPens.ControlDark, aptKnobOutline(2), aptKnobOutline(3))
                e.Graphics.DrawLine(SystemPens.ControlDark, aptKnobOutline(3), aptKnobOutline(4))
                e.Graphics.DrawLine(SystemPens.ControlDark, aptKnobOutline(4), aptKnobOutline(0))

            Next

        End Sub

        Protected Overrides Sub OnSizeChanged(ByVal e As System.EventArgs)
            Me.Invalidate()
            MyBase.OnSizeChanged(e)
        End Sub

#End Region ' Events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Filter useful input key presses for a slider.
        ''' </summary>
        ''' <param name="keyData">The key to validate.</param>
        ''' <returns>True if the given key should be considered an input key.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function IsInputKey(ByVal keyData As System.Windows.Forms.Keys) As Boolean
            Select Case keyData
                Case Keys.Left, Keys.Right, Keys.Up, Keys.Down
                    Return True
            End Select
            Return MyBase.IsInputKey(keyData)
        End Function

        Protected Function RenderScale() As Single
            Return CSng((Me.Maximum - Me.Minimum) / Math.Max(1, Me.Width - cKNOBSIZE - Me.Margin.Left - Me.Margin.Right))
        End Function

        Private Function GetValueAtPoint(ByVal ptMouse As Point) As Integer
            Dim sMouseX As Single = CSng(Math.Max(0, Math.Min(Me.Width - cKNOBSIZE - Me.Margin.Left - Me.Margin.Right, ptMouse.X - cKNOBSIZE / 2)))
            Dim iValue As Integer = Me.Minimum + CInt(sMouseX * Me.RenderScale())
            Return iValue
        End Function
#End Region ' Internals 

    End Class

    Public Class SliderKnobChangedEventArgs
        Inherits EventArgs

        Private m_iKnob As Integer
        Public Sub New(ByVal iKnob As Integer)
            Me.m_iKnob = iKnob
        End Sub
        ReadOnly Property Knob As Integer
            Get
                Return Me.m_iKnob
            End Get
        End Property
    End Class

End Namespace ' Controls
