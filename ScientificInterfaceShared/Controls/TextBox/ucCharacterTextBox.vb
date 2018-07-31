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
Imports System.Windows.Forms
Imports System.ComponentModel
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Text box-derived class managing a single character. It displays 
    ''' descriptive strings for otherwise illegible characters.
    ''' </summary>
    ''' ===========================================================================
    <DefaultProperty("Character")> _
    Public Class ucCharacterTextBox
        Inherits TextBox

#Region " Private variables "

        ''' <summary>Default character code.</summary>
        Private m_iChar As Integer = 32
        ''' <summary></summary>
        Private m_strCharMask As String = ""
        ''' <summary>Flag stating how to interpret the char mask.
        ''' True: only chars in the mask are allowed
        ''' False: only chars in the mask are excluded
        ''' </summary>
        Private m_bMaskInclusive As Boolean = False

#End Region ' Private variables

#Region " Constructor "

        Public Sub New()
            Me.AcceptsTab = True
            Me.AcceptsReturn = True
            ' Multiline = true to allow Enter / Tab key trapping
            Me.Multiline = True
            ' Prevent copy/paste
            Me.ShortcutsEnabled = False
        End Sub

#End Region ' Constructor

#Region " Public interfaces "

        <Browsable(False), Bindable(False)> _
        Public Overrides Property Text() As String
            Get
                Return MyBase.Text
            End Get
            Set(ByVal value As String)
                MyBase.Text = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the character value to display in the box
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(True), Category("Appearance")> _
        Public Property Character() As Char
            Get
                Return Convert.ToChar(Me.CharCode)
            End Get
            Set(ByVal value As Char)
                Me.CharCode = Convert.ToInt32(value)
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the numerical code for the character to display in the box.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Property CharCode() As Int32
            Get
                Return Me.m_iChar
            End Get
            Set(ByVal value As Int32)

                ' Check if character is supported by the control
                If Not Me.SupportsChar(DirectCast(value, Keys)) Then Return

                ' #Yes: ok, gobble it up
                Me.m_iChar = value

                Dim fmt As New cCharFormatter()
                Me.Text = fmt.GetDescriptor(Me.Character)

                'Dim strText As String = ""
                '' Create texttual representation of the char
                'Select Case DirectCast(m_iChar, Keys)
                '    Case Keys.None, Keys.F1 To Keys.F24
                '        strText = DirectCast(m_iChar, Keys).ToString
                '    Case Keys.Enter
                '        strText = My.Resources.GENERIC_CHAR_ENTER
                '    Case Keys.Escape
                '        strText = My.Resources.GENERIC_CHAR_ESCAPE
                '    Case Keys.Space
                '        strText = My.Resources.GENERIC_CHAR_SPACE
                '    Case Keys.Tab
                '        strText = My.Resources.GENERIC_CHAR_TAB
                '    Case Else
                '        Select Case Convert.ToChar(m_iChar)
                '            Case "."c : strText = My.Resources.GENERIC_CHAR_PERIOD
                '            Case ","c : strText = My.Resources.GENERIC_CHAR_COMMA
                '            Case ":"c : strText = My.Resources.GENERIC_CHAR_COLON
                '            Case ";"c : strText = My.Resources.GENERIC_CHAR_SEMICOLON
                '            Case Else
                '                strText = Me.Character()
                '        End Select
                'End Select
                'Me.Text = strText
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set a mask to allow only specific characters to be used in the box
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Property CharacterMask() As String
            Get
                Return Me.m_strCharMask
            End Get
            Set(ByVal value As String)
                Me.m_strCharMask = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set stating how the contents of the 
        ''' <see cref="CharacterMask">Character mask</see> is interpreted.
        ''' <list type="table">
        ''' <item><term>True</term><description>Only chars in the mask are allowed.</description></item>
        ''' <item><term>False</term><description>only chars in the mask are excluded.</description></item>
        ''' </list>
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Property MaskInclusive() As Boolean
            Get
                Return Me.m_bMaskInclusive
            End Get
            Set(ByVal value As Boolean)
                Me.m_bMaskInclusive = value
            End Set
        End Property

#End Region ' Public interfaces

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Key down handler, overridden to eat up 'Del' and 'Back' key presses.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnKeyDown(ByVal e As System.Windows.Forms.KeyEventArgs)
            If (e.KeyCode = Keys.Delete) Or (e.KeyCode = Keys.Back) Then
                e.Handled = True
                e.SuppressKeyPress = True
            End If
            MyBase.OnKeyDown(e)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Key press handler, overridden to seize total and utter control over entered 
        ''' text. There shalt be no doubt.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnKeyPress(ByVal e As System.Windows.Forms.KeyPressEventArgs)
            e.Handled = True
            Me.Character = e.KeyChar
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, returns whether a given character is allowed to be 
        ''' entered in the control.
        ''' </summary>
        ''' <param name="iChar">The character code to check.</param>
        ''' <returns>True if allowed to be entered.</returns>
        ''' -----------------------------------------------------------------------
        Private Function SupportsChar(ByVal iChar As Integer) As Boolean
            Dim bSupported As Boolean = String.IsNullOrEmpty(Me.m_strCharMask)

            If m_bMaskInclusive Then
                bSupported = bSupported Or Me.m_strCharMask.Contains(Convert.ToChar(iChar))
            Else
                bSupported = bSupported Or (Not Me.m_strCharMask.Contains(Convert.ToChar(iChar)))
            End If
            Return bSupported
        End Function


#End Region ' Internals

    End Class

End Namespace ' Controls
