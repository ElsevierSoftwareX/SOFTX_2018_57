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
Option Explicit On

Imports EwECore
Imports ScientificInterface.Other

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the Ecosim 'Apply Forcing to Consumer' interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmApplyFFConsumer
        Inherits frmApplyShapeBase

#Region " Constructor "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        <CLSCompliant(False)> _
        Protected Overrides ReadOnly Property Grid() As gridApplyShapeBase
            Get
                Return Me.m_grid
            End Get
        End Property

#End Region

#Region " Event handlers "

        Private Sub OnClearAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles tsBtnClearAll.Click
            Me.ClearAll()
        End Sub

        Private Sub OnSetAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles tsBtnSetAll.Click
            Me.SetAll()
        End Sub

#End Region ' Event handlers

    End Class

End Namespace
