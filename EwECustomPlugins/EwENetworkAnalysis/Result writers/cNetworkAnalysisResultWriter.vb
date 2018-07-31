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
Imports System.IO
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class to write network analysis results to a CSV file.
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class cNetworkAnalysisResultWriter

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Shazaam
    ''' </summary>
    ''' <param name="manager"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal manager As cNetworkManager)
        Me.Manager = manager
    End Sub

    Protected ReadOnly Property Manager As cNetworkManager

    Public MustOverride Function WriteResults(ByVal strPath As String) As Boolean

#Region " Internals "

    Protected Function WriteFile(ByVal strFileName As String, ByVal strData As String) As Boolean

        Dim strPath As String = Path.GetDirectoryName(strFileName)
        If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then
            Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_SAVE_NOACCESS, strPath), True, strPath)
            Return False
        End If

        Dim sw As New StreamWriter(strFileName)
        If (sw IsNot Nothing) Then
            sw.Write(strData)
            sw.Close()
            Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_SAVE_SUCCESS, strFileName), False, strPath)
            Return True
        End If

        Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_SAVE_FAILED, strFileName), True, strPath)
        Return False

    End Function

    Protected Sub SendMessage(strMessage As String, Optional bError As Boolean = False, Optional strURL As String = "")
        Dim msg As New cMessage(strMessage, eMessageType.DataExport, EwEUtils.Core.eCoreComponentType.External,
                                If(bError, eMessageImportance.Warning, eMessageImportance.Information))
        msg.Hyperlink = strURL
        Me.Manager.Core.Messages.SendMessage(msg)
    End Sub

#End Region ' Internals

End Class
