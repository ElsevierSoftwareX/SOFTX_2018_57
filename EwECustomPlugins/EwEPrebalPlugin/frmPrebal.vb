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

Option Explicit On
Option Strict On

Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls

#End Region

Namespace Ecopath

    ''' <summary>
    ''' Main interface for the Pre-bal plug-in.
    ''' </summary>
    Public Class frmPrebal

        Private m_eng As cPrebalModel = Nothing
        Private m_helper As cPrebalZedGraphHelper = Nothing
        'Private m_cmdShowGroups As cCommand = Nothing

        Public Sub New(uic As cUIContext, eng As cPrebalModel)
            MyBase.New()
            Me.InitializeComponent()
            Me.m_eng = eng
            Me.UIContext = uic
        End Sub

#Region " Overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_helper = New cPrebalZedGraphHelper()
            Me.m_helper.Attach(Me.UIContext, Me.m_graph, Me.m_eng, My.Resources.HEADER_PLOT)

            If (Me.UIContext Is Nothing) Then Return

            'Me.m_cmdShowGroups = Me.UIContext.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            'Me.m_cmdShowGroups.AddControl(m_tsbnDisplayGroups)

            'Me.m_tsbnDisplayGroups.ToolTipText = SharedResources.COMMAND_DISPLAYGROUPS

            'AddHandler Me.m_cmdShowGroups.OnPostInvoke, AddressOf OnShowGroupsChanged

            Me.m_helper.Refresh()
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            Try
                'If (Me.m_cmdShowGroups IsNot Nothing) Then
                'RemoveHandler Me.m_cmdShowGroups.OnPostInvoke, AddressOf OnShowGroupsChanged
                'Me.m_cmdShowGroups.RemoveControl(m_tsbnDisplayGroups)
                'Me.m_cmdShowGroups = Nothing
                'End If
                Me.m_helper.Detach()
            Catch ex As Exception
                cLog.Write(ex)
            End Try

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Overrides

#Region " Event handlers "

        'Private Sub OnShowGroupsChanged(sender As cCommand)
        '    Me.m_helper.Refresh()
        'End Sub

#End Region ' Event handlers

    End Class

End Namespace
