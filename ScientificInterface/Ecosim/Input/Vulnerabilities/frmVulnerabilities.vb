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

Imports EwECore
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the Ecosim Vulnerabilities interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmVulnerabilities

#Region " Private vars "

        Private m_cmdEstimateVs As cCommand = Nothing

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            MyBase.New(New gridVulnerabilities())
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Overloads "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If Me.UIContext Is Nothing Then Return

            Me.m_cmdEstimateVs = Me.CommandHandler.GetCommand("EstimateVs")
            If (Me.m_cmdEstimateVs IsNot Nothing) Then
                Me.m_cmdEstimateVs.AddControl(Me.m_tsbEstimateVs)
            End If

#If Not Debug Then
            ' Remove estimate V's from release version while under development
            Me.m_tsbEstimateVs.Visible = False
#End If

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            If (Me.m_cmdEstimateVs IsNot Nothing) Then
                Me.m_cmdEstimateVs.RemoveControl(Me.m_tsbEstimateVs)
            End If
            MyBase.OnFormClosed(e)
        End Sub

        Private Sub OnScaleVtoTL(sender As Object, e As EventArgs) Handles m_tsbnScaleVtoTL.Click
            Dim dlg As New dlgScaleVs(Me.UIContext)
            If (dlg.ShowDialog(Me.UIContext.FormMain) = DialogResult.OK) Then
                'NOP
            End If
        End Sub

#End Region ' Overloads

    End Class

End Namespace

