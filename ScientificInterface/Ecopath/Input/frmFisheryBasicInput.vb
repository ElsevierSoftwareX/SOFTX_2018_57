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
    ''' Form implementing the Ecopath Fisheries fleet definitions interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmFisheryBasicInput

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
            Me.Grid = Me.m_grid
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            If (Me.CommandHandler Is Nothing) Then Return

            Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditFleets")
            If (cmd IsNot Nothing) Then cmd.AddControl(Me.m_tsbnEditFleets)
        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            If (Me.CommandHandler IsNot Nothing) Then
                Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditFleets")
                If (cmd IsNot Nothing) Then cmd.RemoveControl(Me.m_tsbnEditFleets)
            End If
            MyBase.OnFormClosed(e)
        End Sub

    End Class

End Namespace
