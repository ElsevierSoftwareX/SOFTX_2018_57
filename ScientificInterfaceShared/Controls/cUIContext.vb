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

Imports System.Threading
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' =======================================================================
    ''' <summary>
    ''' Helper class, provides <see cref="IUIElement">User Interface</see>
    ''' elements with contextual information such as the core instance to
    ''' use, a style guide reference, and other centrally accessible elements
    ''' that some day may require multiple instances.
    ''' </summary>
    ''' =======================================================================
    Public Class cUIContext

#Region " Privates vars "

        ''' <summary>The core that a UI can interact with.</summary>
        Private m_core As cCore = Nothing
        ''' <summary>The style guide that a UI can interact with.</summary>
        Private m_sg As cStyleGuide = Nothing
        ''' <summary>The property manager that a UI can interact with.</summary>
        Private m_propman As cPropertyManager = Nothing
        ''' <summary>The command handler that a UI can interact with.</summary>
        Private m_cmdhandler As cCommandHandler = Nothing
        ''' <summary>The EwE main form.</summary>
        Private m_frmMain As Form = Nothing
        ''' <summary>The form positions settings that a UI can interact with.</summary>
        Private m_formsettings As cFormSettings = Nothing
        ''' <summary>Application help.</summary>
        Private m_help As cHelp = Nothing
        ''' <summary>UI thread sync object.</summary>
        Private m_syncObj As SynchronizationContext = Nothing

#End Region ' Privates vars

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="core">The <see cref="cCore">core</see> that a UI can interact with.</param>
        ''' <param name="sg">The <see cref="cStyleGuide">style guide</see> that a UI can interact with.</param>
        ''' <param name="propman">The <see cref="PropertyManager">property manager</see> that any UI can interact with, if available.</param>
        ''' <param name="cmdhandler">The <see cref="cCommandHandler">EwE command handler</see>, if available.</param>
        ''' <param name="formpos">The central <see cref="cFormSettings">EwE-wide form settings provider</see>, if available.</param>
        ''' <param name="frmMain">The EwE main form, if available.</param>
        ''' <param name="help">The <see cref="cHelp">EwE help provider</see>, if available.</param>
        ''' <param name="syncObj">The <see cref="SynchronizationContext"/> for marshalling calls to the main EwE thread, if available.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal core As cCore, _
                       ByVal sg As cStyleGuide, _
                       Optional ByVal propman As cPropertyManager = Nothing, _
                       Optional ByVal cmdhandler As cCommandHandler = Nothing, _
                       Optional ByVal frmMain As Form = Nothing, _
                       Optional ByVal formpos As cFormSettings = Nothing, _
                       Optional ByVal help As cHelp = Nothing, _
                       Optional ByVal syncObj As SynchronizationContext = Nothing)

            Debug.Assert(core IsNot Nothing)
            Debug.Assert(sg IsNot Nothing)

            Me.m_core = core
            Me.m_sg = sg
            Me.m_propman = propman
            Me.m_cmdhandler = cmdhandler
            Me.m_frmMain = frmMain
            Me.m_formsettings = formpos
            Me.m_help = help
            Me.m_syncObj = syncObj

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCore">core</see> that a UI can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Core() As cCore
            Get
                Return Me.m_core
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cStyleGuide">style guide</see> that a UI can
        ''' interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property StyleGuide() As cStyleGuide
            Get
                Return Me.m_sg
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cPropertyManager">property manager</see> that a 
        ''' UI can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property PropertyManager() As cPropertyManager
            Get
                Return Me.m_propman
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCommandHandler">command handler</see> that a 
        ''' UI can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property CommandHandler() As cCommandHandler
            Get
                Return m_cmdhandler
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the main EwE form for centering pop-up dialogs, obtaining the
        ''' application title, etc.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property FormMain() As Form
            Get
                Return Me.m_frmMain
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cFormSettings">form settings manager</see> that a 
        ''' UI can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property FormSettings() As cFormSettings
            Get
                Return Me.m_formsettings
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cHelp">application help</see> that a UI can 
        ''' interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Help() As cHelp
            Get
                Return Me.m_help
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="SynchronizationContext">synchronization object</see> 
        ''' that the user interface was created on.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SyncObject() As SynchronizationContext
            Get
                Return Me.m_syncObj
            End Get
        End Property

    End Class

End Namespace ' Controls
