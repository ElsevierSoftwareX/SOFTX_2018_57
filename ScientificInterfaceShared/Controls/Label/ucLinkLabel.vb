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
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' EwE link label, that reroutes hyperlink clicks to navigation commands.
    ''' </summary>
    ''' <remarks>
    ''' <para>Note that hyperlinks must be provided via the <see cref="ucLinkLabel.Text"/>
    ''' in the form of &lt;a href=".."&gt;...&lt;/a&gt;.</para>
    ''' <para>Please refrain from setting <see cref="ucLinkLabel.LinkArea"/>, 
    ''' <see cref="ucLinkLabel.LinkBehavior"/> or other hyperlink control options
    ''' because they will conflict with the fragile EwE behaviour within.</para>
    ''' <para>Note also that only ONE hyperlink per control is supported.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class ucLinkLabel
        Inherits LinkLabel
        Implements IUIElement

        Private m_strHyperlink As String = ""
        Private m_strTextOrg As String = ""

        Public Sub New()
            MyBase.New()
        End Sub

        Public Property UIContext As cUIContext Implements IUIElement.UIContext

        Public ReadOnly Property Hyperlink As String
            Get
                Return Me.m_strHyperlink
            End Get
        End Property

        Public Overrides Property Text As String
            Get
                If Me.DesignMode Then
                    If (Me.m_strTextOrg Is Nothing) Then Return Me.GetType().ToString
                    Return Me.m_strTextOrg
                End If
                Return MyBase.Text
            End Get
            Set(strText As String)

                Me.m_strTextOrg = strText

                Dim iLinkStart As Integer = 0
                Dim iLinkEnd As Integer = 0

                ' Detect hyperlink (blunt blunt blunt)
                ' In Design mode do not strip out the hyperlink!
                Me.m_strHyperlink = cStringUtils.Hyperlink(strText, MyBase.Text, iLinkStart, iLinkEnd, Not Me.DesignMode)
                Me.LinkArea = New LinkArea(Math.Max(0, iLinkStart), Math.Max(iLinkEnd - iLinkStart, 0))

            End Set
        End Property

        Protected Overrides Sub OnLinkClicked(e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)

            If String.IsNullOrWhiteSpace(Me.m_strHyperlink) Then Return

            If (Me.UIContext IsNot Nothing) Then
                Dim cmd As cBrowserCommand = DirectCast(Me.UIContext.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                If (cmd IsNot Nothing) Then
                    cmd.Invoke(Me.m_strHyperlink)
                End If
            Else
                Try
                    ' Go!
                    Process.Start(Me.m_strHyperlink)
                Catch ex As Exception
                    cLog.Write(ex, "cEwELinkLabel::OnLinkClicked(" & Me.Name & ", " & Me.m_strHyperlink & ")")
                End Try
            End If

        End Sub

    End Class

End Namespace
