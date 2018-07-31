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
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecopath

    Public MustInherit Class cEcopathMergeSplitGroups

#Region " Private vars "

        ''' <summary>The core that holds the source model.</summary>
        Protected m_core As cCore = Nothing

#End Region ' Private vars

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the current Ecopath model is ready to merge or split groups.
        ''' </summary>
        ''' <param name="bSendMessage"></param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Function CanMergeSplitGroups(Optional bSendMessage As Boolean = False) As Boolean

            Dim sm As cCoreStateMonitor = Me.m_core.StateMonitor

            If Not sm.HasEcopathLoaded() Then
                If bSendMessage Then Me.SendMessage(My.Resources.CoreMessages.ECOPATH_MERGESPLIT_ERROR_NOMODEL, False)
                Return False
            End If

            If Me.m_core.nEcosimScenarios > 0 Then
                If bSendMessage Then Me.SendMessage(My.Resources.CoreMessages.ECOPATH_MERGESPLIT_ERROR_HASECOSIM, False)
                Return False
            End If

            If Not Me.m_core.SaveChanges() Then Return False

            Return True

        End Function

#End Region ' Public access

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Send a message.
        ''' </summary>
        ''' <param name="strMessage"></param>
        ''' <param name="bSuccess"></param>
        ''' -----------------------------------------------------------------------
        Protected Sub SendMessage(strMessage As String, bSuccess As Boolean)
            Dim msg As New cMessage(strMessage, eMessageType.Any, eCoreComponentType.EcoPath,
                                If(bSuccess, eMessageImportance.Information, eMessageImportance.Critical))
            Me.m_core.Messages.SendMessage(msg)
        End Sub

#End Region ' Internals

    End Class

End Namespace

