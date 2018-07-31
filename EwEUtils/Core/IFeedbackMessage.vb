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

Namespace Core

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Interface for requesting user feedback via the EwE messaging system.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface IFeedbackMessage
        Inherits IMessage

        ''' <summary>
        ''' Get or set the reply to this message.
        ''' </summary>
        Property Reply() As eMessageReply

        ''' <summary>
        ''' Get or set the reply style to this message.
        ''' </summary>
        Property ReplyStyle() As eMessageReplyStyle

    End Interface

End Namespace
