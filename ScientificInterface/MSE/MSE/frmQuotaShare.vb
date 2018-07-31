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

#End Region ' Imports 

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the MSE Quota Shared interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmQuotaShare
        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
            Me.Grid = Me.m_grid
        End Sub

        Private Sub OnSumSharesToOne(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsSumtoOneBtn.Click
            Me.Core.NormalizeQuotaShare()
        End Sub

        Private Sub OnDefaultShares(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnDefaults.Click
            Me.Core.SetDefaultQuotaShare()
        End Sub
    End Class

End Namespace
