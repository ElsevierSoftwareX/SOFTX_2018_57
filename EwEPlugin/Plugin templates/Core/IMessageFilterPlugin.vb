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

#End Region ' Imports 

''' ===========================================================================
''' <summary>
''' <para>Interface for a plug-in that is invoked when the EwE Core is about to 
''' broadcast a <see cref="IMessage"/> through the EwE application that may impact 
''' the user experience of the software. This plug-in point can be overridden to
''' alter or cancel the message if the message were to disrupt the flow of a
''' plug-in.</para>
''' <para>Note that not all messages may pass through this plug-in point for
''' two reasons.
''' <list type="bullet">
''' <item>
''' <term>Performance</term>
''' <description>The EwE core sends hundreds of messages to synchronize the internal
''' models and user interfaces. Filtering all these messages through the loose-
''' typed plug-in system would noticably affect performance of the EwE application.
''' </description>
''' </item>
''' <item>
''' <term>Reliability</term>
''' <description>Messages that are crucial to the functioning of the EwE application,
''' such as <see cref="EwEUtils.Core.eMessageImportance.Maintenance">maintenance</see>
''' and <see cref="EwEUtils.Core.eMessageImportance.Progress">progress</see> messages 
''' should not be disabled or altered, and are therefor not presented via this
''' plug-in point.</description>
''' </item>
''' </list></para>
''' </summary>
''' ===========================================================================
Public Interface IMessageFilterPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A publicly visible message, e.g. a message that is not meant for 
    ''' <see cref="eMessageImportance.Maintenance">internal EwE maintenance</see> or 
    ''' <see cref="eMessageImportance.Progress">progress feedback</see>, is about 
    ''' to be broadcasted to all its handlers. This is yer chance to shoot it down! 
    ''' Go for it!
    ''' </summary>
    ''' <param name="msg">The message that is about to be broadcasted.</param>
    ''' <param name="bCancelMessage">Flag, determining whether the message should
    ''' not be broadcasted. If True is returned here, the message will not be
    ''' broadcasted.</param>
    ''' <remarks>
    ''' Note that this filter is not intended as a substitute for EwE core
    ''' message handlers. Message filter plug-ins only receive messages that are
    ''' meant for the user, not for the internal workings of EwE. If you are 
    ''' interested in monitoring these messages you will have to set up a proper 
    ''' message handler in your code.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Sub PreProcessMessage(ByVal msg As IMessage, ByRef bCancelMessage As Boolean)

End Interface
