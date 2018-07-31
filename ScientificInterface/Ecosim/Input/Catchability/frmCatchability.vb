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

Imports System.Drawing.Drawing2D
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports ScientificInterface.Other
Imports EwECore
Imports EwEUtils.Core

#End Region

Namespace Ecosim

    Public Class frmCatchability

        Public Sub New()
            Me.InitializeComponent()
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSim}
            Me.TabText = Me.Text
            Me.Grid = Me.m_grdCatchability
        End Sub

        Overrides Property UIContext As cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As cUIContext)
                MyBase.UIContext = value
                Me.m_grdCatchability.UIContext = Me.UIContext
            End Set
        End Property

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            For iflt As Integer = 1 To Me.Core.nFleets
                m_tscbFleets.Items.Add(Core.EcosimFleetInputs(iflt).Name)
            Next
            m_tscbFleets.SelectedIndex = 0

        End Sub

        Private Sub m_tscbFleets_SelectedIndexChanged(sender As Object, e As EventArgs) Handles m_tscbFleets.SelectedIndexChanged
            Me.m_grdCatchability.SelectedFleetIndex = m_tscbFleets.SelectedIndex + 1
        End Sub


        Public Overrides Sub OnCoreMessage(ByVal msg As cMessage)
            MyBase.OnCoreMessage(msg)
            If msg.HasVariable(eVarNameFlags.EcoSimNYears) Then
                Me.m_grdCatchability.RefreshContent()
            End If
        End Sub

    End Class

End Namespace

