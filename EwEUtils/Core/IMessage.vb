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
    ''' Interface for broadcasting messages via the EwE messaging system.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface IMessage

        ''' <summary>
        ''' Get/set the text of the message.
        ''' </summary>
        Property Message As String

        ''' <summary>
        ''' Get/set the <see cref="eMessageType">event type</see> of the message.
        ''' </summary>
        Property Type() As eMessageType

        ''' <summary>
        ''' Get/set the <see cref="eCoreComponentType">source witin EwE</see> that
        ''' the message originates from.
        ''' </summary>
        Property Source() As eCoreComponentType

        ''' <summary>
        ''' Get/set the <see cref="eMessageImportance">importance</see> of the message.
        ''' </summary>
        Property Importance() As eMessageImportance

        ''' <summary>
        ''' Get/set the <see cref="eDataTypes">core objects</see> that the message describes.
        ''' </summary>
        Property DataType() As eDataTypes

        ''' <summary>
        ''' Get/set whether an user interface may suppress repeated instances of a message.
        ''' </summary>
        Property Suppressable() As Boolean

        ''' <summary>
        ''' Get/set whether a message was suppressed.
        ''' </summary>
        Property Suppressed() As Boolean

    End Interface

End Namespace
