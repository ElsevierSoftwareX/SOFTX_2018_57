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
Imports ScientificInterfaceShared.Commands

#End Region ' Imports 

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the Ecopath Basic Input interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmBasicInput

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
            Me.Grid = Me.m_grid
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            Dim cmd As cCommand = Nothing
            MyBase.OnLoad(e)

            If (Me.CommandHandler Is Nothing) Then Return

            cmd = Me.CommandHandler.GetCommand("EditGroups")
            If (cmd IsNot Nothing) Then cmd.AddControl(Me.m_tsbnEditGroups)
            cmd = Me.CommandHandler.GetCommand("EditMultiStanza")
            If (cmd IsNot Nothing) Then cmd.AddControl(Me.m_tsbnEditMultiStanza)
        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If (Me.CommandHandler IsNot Nothing) Then
                Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditGroups")
                If (cmd IsNot Nothing) Then cmd.RemoveControl(Me.m_tsbnEditGroups)
                cmd = Me.CommandHandler.GetCommand("EditMultiStanza")
                If (cmd IsNot Nothing) Then cmd.RemoveControl(Me.m_tsbnEditMultiStanza)
            End If
            MyBase.OnFormClosed(e)

        End Sub

    End Class

End Namespace
