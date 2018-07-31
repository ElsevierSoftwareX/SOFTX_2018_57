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
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Remote controller for Network Analysis plug-in
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cNetworkAnalysisRemote

    Private m_uic As cUIContext = Nothing
    Private m_manager As cNetworkManager = Nothing

    Public Const CMD_SAVE_INDICES As String = "na_save_indices"
    Public Const CMD_SAVE_INDICES_P1_PPR As String = "ppr"
    Public Const CMD_SAVE_INDICES_P2_PATH As String = "path"
    Public Const CMD_SAVE_INDICES_P3_FINDCYCLES As String = "findcycles"

    Public Sub Attach(ByVal uic As cUIContext, _
                      ByVal manager As cNetworkManager)

        Me.m_uic = uic

        Try
            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand(cExecuteCommand.COMMAND_NAME)
            If (cmd IsNot Nothing) Then
                AddHandler cmd.OnInvoke, AddressOf OnExecuteCommand
            End If
        Catch ex As Exception

        End Try

        Me.m_manager = manager

    End Sub

    Public Sub Detach()

        Try
            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand(cExecuteCommand.COMMAND_NAME)

            If (cmd IsNot Nothing) Then
                RemoveHandler cmd.OnInvoke, AddressOf OnExecuteCommand
            End If
        Catch ex As Exception

        End Try

        Me.m_manager = Nothing
    End Sub

    Private Sub OnExecuteCommand(ByVal cmd As cCommand)

        If Not (TypeOf (cmd) Is cExecuteCommand) Then Return

        Dim cmdX As cExecuteCommand = DirectCast(cmd, cExecuteCommand)

        Select Case cmdX.Command.ToLower
            Case CMD_SAVE_INDICES.ToLower
                Try
                    Dim strParmPPR As String = cmdX.Parameter(CMD_SAVE_INDICES_P1_PPR)
                    Dim strPath As String = cmdX.Parameter(CMD_SAVE_INDICES_P2_PATH)
                    Dim strFindCycles As String = cmdX.Parameter(CMD_SAVE_INDICES_P3_FINDCYCLES)
                    Dim bPPR As Boolean = False
                    Dim bFindCycles As Boolean = False

                    If Not String.IsNullOrWhiteSpace(strParmPPR) Then Boolean.TryParse(strParmPPR, bPPR)
                    If Not String.IsNullOrWhiteSpace(strFindCycles) Then Boolean.TryParse(strFindCycles, bFindCycles)

                    If Not Me.SaveIndices(strPath, bPPR, bFindCycles) Then
                        'cmd.Status = "Failed"
                    End If
                Catch ex As Exception
                    ' Aargh
                End Try

        End Select

    End Sub

    Private Function SaveIndices(ByVal strPath As String, _
                                 ByVal bWithPPR As Boolean, _
                                 ByVal bUseTimer As Boolean) As Boolean

        Dim writer As New cResultWriter(Me.m_manager)
        Dim bUseTimeCurr As Boolean = Me.m_manager.UseAbortTimer
        Dim lTimeoutCurr As Long = Me.m_manager.TimeOutMilSecs
        Dim bSuccess As Boolean = True

        Me.m_manager.UseAbortTimer = bUseTimer
        If String.IsNullOrEmpty(strPath) Then strPath = Me.m_uic.Core.OutputPath

        Try
            bSuccess = writer.WriteCurrentResults(strPath)
        Catch ex As Exception
            bSuccess = False
        End Try
        Me.m_manager.UseAbortTimer = bUseTimeCurr
        Return bSuccess

    End Function

End Class
