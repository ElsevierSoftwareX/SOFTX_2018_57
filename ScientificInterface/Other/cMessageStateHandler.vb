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
Imports EwECore
Imports EwEUtils.Core

#End Region ' Imports

'''============================================================================
''' <summary>
''' Helper class, manages the suppressed state and auto-replies for core messages.
''' </summary>
'''============================================================================
Public Class cMessageStateHandler

#Region " Private helper classes "

    '''========================================================================
    ''' <summary>
    ''' Helper class, maintains suppressed state and auto-replies for a category
    ''' of messages.
    ''' </summary>
    '''========================================================================
    Private Class cMessageStateCache

#Region " Private variables "

        ''' <summary>List of suppressed messages</summary>
        Private m_lSuppressedMessageTypes As List(Of eMessageType) = Nothing
        ''' <summary>Dictionary of auto-replies</summary>
        Private m_dictAutoReplies As Dictionary(Of eMessageType, DialogResult) = Nothing

#End Region ' Private variables

#Region " Construction "

        Public Sub New()
            Me.m_lSuppressedMessageTypes = New List(Of eMessageType)
            Me.m_dictAutoReplies = New Dictionary(Of eMessageType, DialogResult)
        End Sub

#End Region ' Construction

#Region " Public bits "

        Public Property AutoReply(ByVal mt As eMessageType) As DialogResult
            Get
                If Me.m_dictAutoReplies.ContainsKey(mt) Then
                    Return Me.m_dictAutoReplies(mt)
                End If
                Return System.Windows.Forms.DialogResult.None
            End Get
            Set(ByVal value As DialogResult)
                If Me.m_dictAutoReplies.ContainsKey(mt) Then
                    Me.m_dictAutoReplies.Remove(mt)
                End If
                Me.m_dictAutoReplies(mt) = value
            End Set
        End Property

        Public Property Suppress(ByVal mt As eMessageType) As Boolean
            Get
                Return (Me.m_lSuppressedMessageTypes.IndexOf(mt) > -1)
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    Me.m_lSuppressedMessageTypes.Add(mt)
                Else
                    Me.m_lSuppressedMessageTypes.Remove(mt)
                End If
            End Set
        End Property

        Public Sub Clear()
            Me.m_lSuppressedMessageTypes.Clear()
            Me.m_dictAutoReplies.Clear()
        End Sub

#End Region ' Public bits

    End Class

#End Region ' Private helper classes

#Region " Private variables "

    Private m_dtMessageState As Dictionary(Of eCoreComponentType, cMessageStateCache)

#End Region ' Private variables

#Region " Construction "

    Public Sub New()
        Me.m_dtMessageState = New Dictionary(Of eCoreComponentType, cMessageStateCache)
    End Sub

#End Region ' Construction

#Region " Public bits "

    Public Property IsSuppressed(ByVal source As eCoreComponentType, ByVal mt As eMessageType) As Boolean
        Get
            Return Me.GetCache(source).Suppress(mt)
        End Get
        Set(ByVal value As Boolean)
            Me.GetCache(source).Suppress(mt) = value
        End Set
    End Property

    Public Property AutoReply(ByVal source As eCoreComponentType, ByVal mt As eMessageType) As DialogResult
        Get
            Return Me.GetCache(source).AutoReply(mt)
        End Get
        Set(ByVal value As DialogResult)
            Me.GetCache(source).AutoReply(mt) = value
        End Set
    End Property

    Public Sub Clear(ByVal src As eCoreComponentType)
        Me.GetCache(src).Clear()
    End Sub

#End Region ' Public bits

#Region " Internals "

    Private Function GetCache(ByVal source As eCoreComponentType) As cMessageStateCache

        Dim c As cMessageStateCache = Nothing

        Select Case source
            Case eCoreComponentType.EcoPath, eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace, eCoreComponentType.Ecotracer
                ' NOP
            Case Else
                source = eCoreComponentType.Core
        End Select

        If (Me.m_dtMessageState.ContainsKey(source) = False) Then
            c = New cMessageStateCache()
            Me.m_dtMessageState(source) = c
        Else
            c = Me.m_dtMessageState(source)
        End If
        Return c

    End Function


#End Region ' Internals

End Class
