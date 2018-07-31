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
Imports EwEUtils.Core

#End Region

Namespace Ecospace

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the Ecospace 'Apply capacity map' interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmApplyForagingResponses
        Inherits frmApplyShapeBase

#Region " Constructor "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Hook up to core messages
            ' For this form only
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcospaceResponseInteractionManager, eCoreComponentType.EcoSpace}

            Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditInputMaps")
            If (cmd IsNot Nothing) Then
                cmd.AddControl(Me.m_tsbnDefineInputMaps)
            End If

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditInputMaps")
            If (cmd IsNot Nothing) Then
                cmd.RemoveControl(Me.m_tsbnDefineInputMaps)
            End If
            MyBase.OnFormClosed(e)
        End Sub

        <CLSCompliant(False)>
        Protected Overrides ReadOnly Property Grid() As gridApplyShapeBase
            Get
                Return Me.m_grid
            End Get
        End Property

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            If (msg.Source = eCoreComponentType.EcospaceResponseInteractionManager) Then
                Me.Grid.UpdateContent()
            ElseIf (msg.Source = eCoreComponentType.EcoSpace And msg.DataType = eDataTypes.EcospaceLayerDriver) Then
                Me.Grid.UpdateContent()
            End If

        End Sub

#End Region

    End Class

End Namespace
