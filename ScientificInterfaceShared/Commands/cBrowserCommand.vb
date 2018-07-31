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
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Command to launch a hyperlink, which can be either a url or a file/folder path.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cBrowserCommand
        Inherits cCommand

        ''' <summary>URL to show.</summary>
        Private m_strURL As String = ""
        ''' <summary>URL alias to show.</summary>
        Private m_type As cWebLinks.eLinkType = cWebLinks.eLinkType.NotSet

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' <example>
        ''' The folowing shows how to retrieve the one instance of the cBrowserCommand
        ''' from anywhere in the EwE6 user interface:
        ''' <code>
        ''' ' Get the one and only command 
        ''' Dim cdmH As CommandHandler = CommandHandler.GetInstance()
        ''' ' Get the one and only navigation command
        ''' Dim cmd As cBrowserCommand = DirectCast(GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
        ''' ' Invoke the command
        ''' ...
        ''' </code>
        ''' </example>
        ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "~browse"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of the <see cref="cBrowserCommand"/> class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invokes the command to make the EwE6 GUI navigate to user interface
        ''' element defined by this call.
        ''' </summary>
        ''' <param name="strURL">URL to navigate to.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal strURL As String)
            Me.m_strURL = strURL
            MyBase.Invoke()
            Me.m_strURL = ""
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invokes the command to make the EwE6 GUI navigate to user interface
        ''' element defined by this call.
        ''' </summary>
        ''' <param name="link">Symbolic <see cref="cWebLinks.eLinkType"/> to navigate to.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(link As cWebLinks.eLinkType)
            Me.m_type = link
            MyBase.Invoke()
            Me.m_type = cWebLinks.eLinkType.NotSet
        End Sub

        ''' <summary>
        ''' Get the <see cref="m_strURL">URL</see> to navigate to.
        ''' </summary>
        Public ReadOnly Property URL(decoder As cWebLinks) As String
            Get
                If (Me.m_type = cWebLinks.eLinkType.NotSet) Then Return Me.m_strURL
                Return decoder.GetURL(Me.m_type)
            End Get
        End Property

    End Class

End Namespace
