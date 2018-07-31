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
    ''' The NavigationCommand class implements a <see cref="cCommand">Command</see>
    ''' that is used in EwE6 to navigate to embedded and plugin-provided 
    ''' <see cref="System.Windows.Forms.Form">Forms</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cNavigationCommand
        Inherits cCommand

        ''' <summary>A human readable name for the page to open. This text may be
        ''' used in tabs, in tooltips and other user interface elements.</summary>
        Private m_strPageName As String = ""

        ''' <summary>A unique ID for the page to open. This ID is used internally
        ''' in the GUI to manage opened pages.</summary>
        Private m_strPageID As String = ""

        ''' <summary>The core state that this page requires to display its
        ''' content.</summary>
        Private m_coreExecutionState As eCoreExecutionState = eCoreExecutionState.Idle

        ''' <summary>The Type of the Form to invoke.</summary>
        Private m_typeClass As Type = Nothing

        ''' <summary>Help URL for this form</summary>
        Private m_strHelpURL As String = ""

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' <example>
        ''' The folowing shows how to retrieve the one instance of the cNavigationCommand
        ''' from anywhere in the EwE6 user interface:
        ''' <code>
        ''' ' Get the one and only command 
        ''' Dim cdmH As cCommandHandler = cCommandHandler.GetInstance()
        ''' ' Get the one and only navigation command
        ''' Dim cmd As cNavigationCommand = DirectCast(GetCommand(cNavigationCommand.COMMAND_NAME), cNavigationCommand)
        ''' ' Invoke the command
        ''' ...
        ''' </code>
        ''' </example>
        ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "~navigate"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of the NavigationCommand class.
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
        ''' <param name="strPageName">A pleasantly legible name of the page to 
        ''' navigate to.</param>
        ''' <param name="strPageID">A unique page ID, used by the EwE6 GIU to
        ''' check whether this page is already open and needs merely focusing,
        ''' or whether this page needs to be constructed.</param>
        ''' <param name="coreExecutionState"><para>An enumerated value obtained
        ''' from the EwE6 Core State monitor, indicating what state the EwE6
        ''' core should be running at to be able to provide data for the form
        ''' that will be launched from this command.</para>
        ''' <para>The EwE6 GUI will attempt to bring the core up to this desired 
        ''' running state prior to launching the form.</para></param>
        ''' <param name="typeClass">A Type of a Windows.Forms derived user
        ''' interface that is to be created.</param>
        ''' <param name="strHelpURL">Help URL for this page.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal strPageName As String, ByVal strPageID As String, _
                ByVal coreExecutionState As eCoreExecutionState, _
                ByVal typeClass As Type, _
                Optional ByVal strHelpURL As String = "")

            Me.m_strPageName = strPageName
            Me.m_strPageID = strPageID
            Me.m_coreExecutionState = coreExecutionState
            Me.m_typeClass = typeClass
            Me.m_strHelpURL = strHelpURL

            MyBase.Invoke()
        End Sub

        ''' <summary>
        ''' Get the <see cref="m_strPageName">Page name</see>.
        ''' </summary>
        Public ReadOnly Property PageName() As String
            Get
                Return Me.m_strPageName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="m_strPageID">Page ID</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property PageID() As String
            Get
                Return Me.m_strPageID
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="m_coreExecutionState">Core execution state</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property CoreExecutionState() As eCoreExecutionState
            Get
                Return Me.m_coreExecutionState
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="Type">Type</see> of the <see cref="m_typeClass">Form</see>
        ''' to create for this command.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ClassType() As Type
            Get
                Return Me.m_typeClass
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="m_strHelpURL">help URL</see> for this page.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property HelpURL() As String
            Get
                Return Me.m_strHelpURL
            End Get
        End Property

    End Class

End Namespace
