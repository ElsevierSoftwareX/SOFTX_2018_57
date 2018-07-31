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
    ''' Numeric up/down derived control to improve editing.
    ''' </summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/30899/Extended-NumericUpDown-Control
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class cEwENumericUpDown
        Inherits NumericUpDown

#Region " Private vars "

        ''' <summary>Reference to the underlying TextBox control.</summary>
        Private m_tbx As TextBox = Nothing
        ''' <summary>Reference to the underlying UpDownButtons control.</summary>
        Private m_updown As Control = Nothing

        ''' <summary>Flag to track mouse position.</summary>
        Private m_bMouseOver As Boolean = False
        ''' <summary>Flag to track focus.</summary>
        Private m_bHasFocus As Boolean = False

        Private m_showUpdownMode As eShowUpDownButtonsType = eShowUpDownButtonsType.Always

#End Region ' Private vars

#Region " Construction / destruction "

        ''' <summary>
        ''' Constructor
        ''' </summary>
        Public Sub New()

            MyBase.New()

            ' Get a reference to the underlying UpDownButtons field
            Me.m_updown = MyBase.Controls(0)

            ' Sanity checks
            Debug.Assert(Me.m_updown IsNot Nothing)
            Debug.Assert(Me.m_updown.GetType().FullName = "System.Windows.Forms.UpDownBase+UpDownButtons")

            ' Get a reference to the underlying TextBox field.
            Me.m_tbx = TryCast(MyBase.Controls(1), TextBox)

            ' Underlying private type is System.Windows.Forms.UpDownBase+UpDownButtons
            If (Me.m_tbx Is Nothing) OrElse (Me.m_tbx.GetType().FullName <> "System.Windows.Forms.UpDownBase+UpDownEdit") Then
                Throw New ArgumentNullException(Me.GetType.FullName & ": Can't get a reference to internal TextBox field.")
            End If

            ' Add handlers (MouseEnter and MouseLeave events of NumericUpDown are not working properly)
            AddHandler m_tbx.MouseEnter, AddressOf HiddenMouseEnterLeave
            AddHandler m_tbx.MouseLeave, AddressOf HiddenMouseEnterLeave
            AddHandler m_updown.MouseEnter, AddressOf HiddenMouseEnterLeave
            AddHandler m_updown.MouseLeave, AddressOf HiddenMouseEnterLeave
            AddHandler MyBase.MouseEnter, AddressOf HiddenMouseEnterLeave
            AddHandler MyBase.MouseLeave, AddressOf HiddenMouseEnterLeave

        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)

            ' Remove handlers
            RemoveHandler m_tbx.MouseEnter, AddressOf HiddenMouseEnterLeave
            RemoveHandler m_tbx.MouseLeave, AddressOf HiddenMouseEnterLeave
            RemoveHandler m_updown.MouseEnter, AddressOf HiddenMouseEnterLeave
            RemoveHandler m_updown.MouseLeave, AddressOf HiddenMouseEnterLeave
            RemoveHandler MyBase.MouseEnter, AddressOf HiddenMouseEnterLeave
            RemoveHandler MyBase.MouseLeave, AddressOf HiddenMouseEnterLeave
            MyBase.Dispose(disposing)

        End Sub

#End Region ' Construction / destruction

#Region " Overrides "

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            If (Me.m_updown.Visible = False) Then
                e.Graphics.Clear(Me.BackColor)
            End If
            MyBase.OnPaint(e)
        End Sub

        ''' <summary>
        ''' WndProc override to kill WN_MOUSEWHEEL message
        ''' </summary>
        Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
            Const WM_MOUSEWHEEL As Integer = &H20A

            If (m.Msg = WM_MOUSEWHEEL) Then
                Select Case InterceptMouseWheel
                    Case eInterceptMouseWheelType.Always
                        ' standard message
                        MyBase.WndProc(m)
                    Case eInterceptMouseWheelType.WhenMouseOver
                        If m_bMouseOver Then
                            ' standard message
                            MyBase.WndProc(m)
                        End If
                    Case eInterceptMouseWheelType.Never
                        ' kill the message
                        Exit Sub
                End Select
            Else
                MyBase.WndProc(m)
            End If

        End Sub

#End Region ' Overrides

#Region " Extended properties "

        <DefaultValue(False)>
        <Category("Behavior")>
        <Description("Automatically select control text when it receives focus.")>
        Public Property AutoSelect() As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="TextBox.SelectionStart"/>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionStart() As Integer
            Get
                Return m_tbx.SelectionStart
            End Get
            Set(ByVal value As Integer)
                m_tbx.SelectionStart = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="TextBox.SelectionLength"/>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionLength() As Integer
            Get
                Return m_tbx.SelectionLength
            End Get
            Set(ByVal value As Integer)
                m_tbx.SelectionLength = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="TextBox.SelectedText"/>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectedText() As String
            Get
                Return m_tbx.SelectedText
            End Get
            Set(ByVal value As String)
                m_tbx.SelectedText = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Define <see cref="eInterceptMouseWheelType">when</see> the mouse wheel 
        ''' is intercepted.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Behavior")>
         <DefaultValue(GetType(eInterceptMouseWheelType), "Always")>
         <Description("Enables MouseWheel only under certain conditions.")>
        Public Property InterceptMouseWheel() As eInterceptMouseWheelType = eInterceptMouseWheelType.WhenMouseOver

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type defining possible mouse wheel capture modes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eInterceptMouseWheelType As Integer
            ''' <summary>MouseWheel always works (defauld behavior)</summary>
            Always
            ''' <summary>MouseWheel works only when mouse is over the (focused) control</summary>
            WhenMouseOver
            ''' <summary>MouseWheel never works</summary>
            Never
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Define <see cref="eShowUpDownButtonsType">when</see> the updown 
        ''' buttons can be shown.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Behavior")>
        <DefaultValue(GetType(eShowUpDownButtonsType), "Always")>
        <Description("Set UpDownButtons visibility mode.")>
        Public Property ShowUpDownButtons() As eShowUpDownButtonsType
            Get
                Return m_showUpdownMode
            End Get
            Set(ByVal value As eShowUpDownButtonsType)
                m_showUpdownMode = value
                ' update UpDownButtons visibility
                UpdateUpDownButtonsVisibility()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type defining possible updown button visibility modes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eShowUpDownButtonsType As Integer
            ''' <summary>UpDownButtons are always visible (defauld behavior)</summary>
            Always
            ''' <summary>UpDownButtons are visible only when mouse is over the control</summary>
            WhenMouseOver
            ''' <summary>UpDownButtons are visible only when control has the focus</summary>
            WhenFocus
            ''' <summary>UpDownButtons are visible when control has focus or mouse is over the control</summary>
            WhenFocusOrMouseOver
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the value in the control will wrap around when exceeding 
        ''' the <see cref="Maximum"/>, or dropping below the <see cref="Minimum"/>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <DefaultValue(False)>
        <Category("Behavior")>
        <Description("If set, incrementing value will cause it to restart from Minimum when Maximum is reached (and viceversa).")>
        Public Property WrapValue() As Boolean

#End Region ' Extended properties

#Region "  Text selection "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Select all the text on focus enter
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnGotFocus(ByVal e As System.EventArgs)

            Me.m_bHasFocus = True
            If Me.AutoSelect Then Me.m_tbx.SelectAll()
            ' Update UpDownButtons visibility
            If (Me.m_showUpdownMode = eShowUpDownButtonsType.WhenFocus) Or (Me.m_showUpdownMode = eShowUpDownButtonsType.WhenFocusOrMouseOver) Then
                Me.UpdateUpDownButtonsVisibility()
            End If

            MyBase.OnGotFocus(e)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Indicate that we have lost the focus
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLostFocus(e As EventArgs)

            Me.m_bHasFocus = False
            ' Update UpDownButtons visibility
            If (Me.m_showUpdownMode = eShowUpDownButtonsType.WhenFocus) Or (Me.m_showUpdownMode = eShowUpDownButtonsType.WhenFocusOrMouseOver) Then
                Me.UpdateUpDownButtonsVisibility()
            End If
            MyBase.OnLostFocus(e)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' MouseUp will kill the SelectAll made on GotFocus.
        ''' Will restore it, but only if user have not made a partial text selection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseUp(ByVal mevent As System.Windows.Forms.MouseEventArgs)
            If AutoSelect AndAlso m_tbx.SelectionLength = 0 Then
                m_tbx.SelectAll()
            End If
            MyBase.OnMouseUp(mevent)
        End Sub

#End Region ' Text selection

#Region " Additional events "

        ' these events will be raised correctly, when mouse enters on the textbox
        Public Shadows Event MouseEnter As EventHandler(Of EventArgs)
        Public Shadows Event MouseLeave As EventHandler(Of EventArgs)

        ' Events raised BEFORE value decrement/increment
        Public Event BeforeValueDecrement As CancelEventHandler
        Public Event BeforeValueIncrement As CancelEventHandler

        ' this handler is called at each mouse Enter/Leave movement
        Private Sub HiddenMouseEnterLeave(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim cr As Drawing.Rectangle = RectangleToScreen(ClientRectangle)
            Dim mp As Drawing.Point = MousePosition

            ' actual state
            Dim bIsOver As Boolean = cr.Contains(mp)

            ' test if status changed
            If m_bMouseOver Xor bIsOver Then
                ' update state
                m_bMouseOver = bIsOver
                If m_bMouseOver Then
                    RaiseEvent MouseEnter(Me, EventArgs.Empty)
                Else
                    RaiseEvent MouseLeave(Me, EventArgs.Empty)
                End If
            End If

            ' update UpDownButtons visibility
            If m_showUpdownMode <> eShowUpDownButtonsType.Always Then
                UpdateUpDownButtonsVisibility()
            End If

        End Sub

#End Region ' Additional events

#Region " Value increment/decrement management "

        ''' <summary>
        ''' Raise the down button event.
        ''' </summary>
        Public Overrides Sub DownButton()

            If MyBase.ReadOnly Then Exit Sub
            Dim e As New CancelEventArgs
            RaiseEvent BeforeValueDecrement(Me, e)
            If e.Cancel Then Exit Sub
            ' decrement with wrap
            If WrapValue AndAlso Value - Increment < Minimum Then
                Value = Maximum
            Else
                MyBase.DownButton()
            End If
        End Sub

        ''' <summary>
        ''' Raise the up button event.
        ''' </summary>
        Public Overrides Sub UpButton()
            If MyBase.ReadOnly Then Exit Sub
            Dim e As New CancelEventArgs
            RaiseEvent BeforeValueIncrement(Me, e)
            If e.Cancel Then Exit Sub
            ' increment with wrap
            If WrapValue AndAlso Value + Increment > Maximum Then
                Value = Minimum
            Else
                MyBase.UpButton()
            End If
        End Sub

#End Region ' Value increment/decrement management

#Region " UpDownButtons visibility management "

        ''' <summary>
        ''' Show or hide the UpDownButtons, according to ShowUpDownButtons property value
        ''' </summary>
        Sub UpdateUpDownButtonsVisibility()

            ' test new state
            Dim newVisible As Boolean = False
            Select Case m_showUpdownMode
                Case eShowUpDownButtonsType.WhenMouseOver
                    newVisible = m_bMouseOver
                Case eShowUpDownButtonsType.WhenFocus
                    newVisible = m_bHasFocus
                Case eShowUpDownButtonsType.WhenFocusOrMouseOver
                    newVisible = m_bHasFocus OrElse m_bMouseOver
                Case Else
                    newVisible = True
            End Select

            ' assign only if needed
            If m_updown.Visible <> newVisible Then
                If newVisible Then
                    m_tbx.Width = Me.ClientRectangle.Width - m_updown.Width
                Else
                    m_tbx.Width = Me.ClientRectangle.Width
                End If
                m_updown.Visible = newVisible
                OnTextBoxResize(m_tbx, EventArgs.Empty)
                Me.Invalidate()
            End If

        End Sub

        ''' <summary>
        ''' Custom textbox size management
        ''' </summary>
        Protected Overrides Sub OnTextBoxResize(ByVal source As Object, ByVal e As System.EventArgs)
            If m_tbx Is Nothing Then Exit Sub
            If m_showUpdownMode = eShowUpDownButtonsType.Always Then
                ' standard management
                MyBase.OnTextBoxResize(source, e)
            Else
                ' custom management

                ' change position if RTL
                Dim fixPos As Boolean = Me.RightToLeft = RightToLeft.Yes Xor Me.UpDownAlign = LeftRightAlignment.Left

                If m_bMouseOver Then
                    m_tbx.Width = Me.ClientSize.Width - m_tbx.Left - m_updown.Width - 2
                    If fixPos Then m_tbx.Location = New Point(16, m_tbx.Location.Y)
                Else
                    If fixPos Then m_tbx.Location = New Point(2, m_tbx.Location.Y)
                    m_tbx.Width = Me.ClientSize.Width - m_tbx.Left - 2
                End If

            End If

        End Sub

#End Region ' UpDownButtons visibility management

    End Class

End Namespace
