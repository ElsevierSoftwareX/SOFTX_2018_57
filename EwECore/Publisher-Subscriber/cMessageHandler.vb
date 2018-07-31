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
Imports System.ComponentModel
Imports EwEUtils.Core

''' <summary>
''' <para>This class is the message handler portion of the Message-Publisher/Message-Handler pattern use to pass messages from the core(publisher) to an interface(handler).</para>
''' <para>This pattern is based on the Subject/Observer or Publisher/Subscriber patterns.
''' Basically the cMessageHandler provides a wrapper for a delegate in the interface that handles the message. 
''' This allows the interface to implement the actual message handling without having to expose its internal structure.</para>
''' <para>It also allows an interface to split the message handling into a series of smaller routines that only know how to handle one type of message.</para>
''' </summary>
''' <remarks>
''' <para>How to use cMessageHandler:</para>
''' <para>Define a method in your interface that will do the actual message handling with the same signature as EwECore.cCore.CoreMessageDelegate(cMessage).</para>
''' <para>Create a cMessageHandler object and in its constructor pass in 3 arguments</para>
''' <para>1.) The AddressOf the delegate that will handle the message</para>
''' <para>2.) Source of the message (eCoreComponentType)</para>
''' <para>3.) The message to handle (eMessageType)</para>
''' <para>This tells the handler what type of message to handle and where to send the message.</para>
''' <para>Next</para>
''' <para>Add the cMessageHandler object to the cCore.Messages.AddMessageHandler(cMessageHandler) interface. This will register the message handler with the core and any messages of this type will be sent to the delegate defined in the interface.</para>
''' <para>For a default message handler set the eMessageType flag to 'eMessageType.Any'. This handler will be sent any messages that do not have a specific handler.</para>
'''  </remarks>
''' <history>
''' <revision>jb 15/mar/06 Removed SendMessageUseDefaults()</revision>
'''</history>
Public Class cMessageHandler
    Implements IDisposable

#Region " Private vars "

    ''' <summary>Delegate to use for handling the message.</summary>
    Private m_DelegateNotifier As EwECore.cCore.CoreMessageDelegate = Nothing
    ''' <summary>Sync object to use for handling the message.</summary>
    Private m_syncobj As System.Threading.SynchronizationContext = Nothing
    ''' <summary>Core component filter. Cannot be <see cref="eCoreComponentType.NotSet">NotSet</see>.</summary>
    Private m_corecomponent As eCoreComponentType = eCoreComponentType.NotSet
    ''' <summary>Message type filter. Can be anything.</summary>
    Private m_msgtype As eMessageType = eMessageType.Any

#End Region ' Private vars

    ''' <summary>
    ''' Constructs a new cMessageHandler object that will send messages of a given type to the DelegateToCall argument.
    ''' </summary>
    ''' <param name="DelegateToCall">Delegate that will handle the message</param>
    ''' <param name="SourceToHandle">Source of the message i.e EcoPath. This parameter cannot be <see cref="eCoreComponentType.NotSet">NotSet</see>.</param>
    ''' <param name="MessageTypeToHandle">Type of message to handle i.e. DietComp this message will only handle the DietComp not summing to one message</param>
    ''' <remarks>
    ''' <para>For a default handler set the MessageTypeToHandle flag to eMessageType.Any this will send any unhandled message to this delegate.</para>
    ''' <para>To have s single delegate handle multiple messages create a new cMessageHandler with this same 'DelegateToCall' argument and a different MessageTypeToHandle flag.</para>
    ''' </remarks>
    Sub New(ByVal DelegateToCall As EwECore.cCore.CoreMessageDelegate, _
            ByVal SourceToHandle As eCoreComponentType, _
            ByVal MessageTypeToHandle As eMessageType, _
            ByVal syncobj As System.Threading.SynchronizationContext)

        Debug.Assert(DelegateToCall IsNot Nothing, "Must specify a valid delegate")
        Debug.Assert(SourceToHandle <> eCoreComponentType.NotSet, "Must specify a valid source")
        ' Debug.Assert(syncobj IsNot Nothing, Me.ToString & ".New() SynchronizationContext must not be Nothing!")

        Me.m_DelegateNotifier = DelegateToCall
        Me.m_corecomponent = SourceToHandle
        Me.m_msgtype = MessageTypeToHandle
        Me.m_syncobj = syncobj

    End Sub

    ''' <inheritdocs cref="IDisposable.Dispose"/>
    Public Sub Dispose() Implements IDisposable.Dispose
        If (Me.m_DelegateNotifier IsNot Nothing) Then
            Me.m_DelegateNotifier = Nothing
            Me.m_syncobj = Nothing
            Me.m_corecomponent = eCoreComponentType.NotSet
            Me.m_msgtype = eMessageType.NotSet
        End If
        GC.SuppressFinalize(Me)
    End Sub

#If DEBUG Then

    ''' <summary>
    ''' Get/set the name of a message handler for ease of debugging.
    ''' </summary>
    ''' <remarks>
    ''' Property available in DEBUG mode only.
    ''' </remarks>
    Public Property Name() As String

#End If

    ''' <summary>
    ''' Called by the cMessagePublisher to send a message to a message specific handler. 
    ''' If this cMessageHandler can handle this type of message the message will be sent to the Delegate passed in when this object was constructed.
    ''' </summary>
    ''' <param name="message">Message to send. This handler will used the Type and Source flags of the message to see if it can handle this type of message</param>
    ''' <returns>
    ''' <para>True if this message handler can handle this type of message.</para>
    ''' <para>False if the message was not handled or a problem was encountered.</para>
    ''' </returns>
    ''' <remarks>
    ''' For the message to be handled it must have the same Type and Source as this handler.
    ''' </remarks>
    Friend Function SendMessage(ByRef message As cMessage) As Boolean

        Try
            'test for a NULL delegate this should not be possible but check anyway
            If (Not m_DelegateNotifier Is Nothing) Then

                'test the type and source of the message
                ' JS 15Mar06: test for MessageType.Any
                If (message.Type = m_msgtype Or m_msgtype = eMessageType.Any) And _
                   (message.Source = m_corecomponent) Then

                    Try
                        If Me.m_syncobj Is Nothing Then
                            Me.marshallSendMessage(message)
                        Else
                            'marshall the call to the delegate onto the thread that created this handler
                            Me.m_syncobj.Send(New System.Threading.SendOrPostCallback(AddressOf Me.marshallSendMessage), message)
                            ' Me.m_syncobj.Invoke(Me.m_DelegateNotifier, New Object() {message})
                        End If
                    Catch ex As Threading.ThreadAbortException
                        ' A thread is dying, do not assert
                        cLog.Write(ex)
                    Catch ex As InvalidAsynchronousStateException
                        ' Message target has evaporated. Ignore
                    Catch ex As Exception
                        'Error thrown in the handler by an interface that was not handled 
                        'we have no idea if this message got handled or not
                        cLog.Write(ex)
                        Debug.Assert(False, Me.ToString & ".SendMessage(cMessage) Error thrown by an interface message handler.")
                        Return False
                    End Try

                    'this message was handled so return True
                    Return True
                End If 'If message.MessageType = m_Type And message.MessageSource = m_source Then

            Else 'If Not m_DelegateNotifier Is Nothing Then

                'delegate = NULL
                'can't really send a message now can we!!!
                cLog.Write(Me.ToString & ".SendMessage(cMessage) Delegate has not been initialized.")
                Return False

            End If 'If Not m_DelegateNotifier Is Nothing Then

        Catch ex As Exception
            cLog.Write(ex, Me.ToString & ".SendMessage(cMessage)")
            Debug.Assert(False, "Error in cMessageHandler.SendMessage")
            Return False
        End Try

        'this handler can not handle this type of message so return False
        Return False

    End Function

    Private Sub marshallSendMessage(ByVal Message As Object)
        Debug.Assert(TypeOf Message Is cMessage, "Invalid type passed to cMessageHandler.SendMessage()!")
        If (Me.m_DelegateNotifier IsNot Nothing) Then
            Try
                Me.m_DelegateNotifier.Invoke(DirectCast(Message, cMessage))
            Catch ex As Exception
                cLog.Write(ex, "cMessageHandler.marshallSendMessage(" & Message.ToString & ")")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Test for equality of Delegates.
    ''' </summary>
    ''' <param name="Handler">cMessageHandler object to test</param>
    ''' <returns>True if this Message Handlers delagate is equal to the one bing passed in. False otherwise</returns>
    ''' <remarks>This tests the underlying delegates for equality NOT the cMessageHandlers them selves.</remarks>
    Public Overrides Function Equals(ByVal Handler As Object) As Boolean

        Try
            Debug.Assert(TypeOf Handler Is cMessageHandler, "Invalid type passed to cMessageHandler.Equals()")
            If m_DelegateNotifier.Equals(DirectCast(Handler, cMessageHandler).getDelegate) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            System.Console.WriteLine("MessageHandler.Equals() Exception: " & ex.Message)
            Debug.Assert(False, "MessageHandler.Equals() Exception: " & ex.Message)
        End Try

    End Function

    ''' <summary>
    ''' Return the underlying Delagate.
    ''' </summary>
    ''' <returns>CoreMessageDelegate delagate object.</returns>
    ''' <remarks>This is used by  Equals() to test for equality of two cMessageHandler objects.</remarks>
    Friend Function getDelegate() As cCore.CoreMessageDelegate
        Return m_DelegateNotifier
    End Function

End Class

