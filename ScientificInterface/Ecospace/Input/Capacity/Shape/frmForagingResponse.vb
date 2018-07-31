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
Imports ScientificInterfaceShared

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Form implementing all functionality to add, remove and edit
''' <see cref="cEnviroResponseFunction">environmental response shapes</see>.
''' </summary>
''' -----------------------------------------------------------------------
Public Class frmForagingResponse

#Region " Private variables "

    ''' <summary>Controller for shape-related GUI components in this form.</summary>
    Private m_shapeguihandler As cCapacityShapeGUIHandler = Nothing

#End Region '  Private variables

#Region " Constructors "

    Public Sub New()
        Me.InitializeComponent()
    End Sub

#End Region ' Constructors

#Region " Overrides "

    Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
            MyBase.UIContext = value
            Me.m_assignments.UIContext = value
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; implemented to make sure that this form receives 
    ''' <see cref="cMessage">messages</see> from specific 
    ''' <see cref="eCoreComponentType">message sources</see>.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)
        If (Me.UIContext Is Nothing) Then Return
        Me.m_shapeguihandler = New cCapacityShapeGUIHandler(Me.UIContext)
        Me.m_shapeguihandler.Attach(Me.m_shapeToolBox, Me.m_shapeToolboxToolbar, _
                                    Me.m_sketchPad, Me.m_sketchPadToolbar, _
                                    Me.m_assignments, Me.m_assignmentsToolbar)
        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.ShapesManager}
    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
        Me.m_shapeguihandler.Detach()
        MyBase.OnFormClosed(e)
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Generic EwEForm message handler; implemented to respond to Forcing 
    ''' Function shape changes.
    ''' </summary>
    ''' <param name="msg">Incoming core <see cref="cMessage">messages</see>.</param>
    ''' -------------------------------------------------------------------
    Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)

        If msg.Source = eCoreComponentType.ShapesManager Then
            If (((msg.Type = eMessageType.DataAddedOrRemoved) Or (msg.Type = eMessageType.DataModified)) And _
                 (msg.DataType = eDataTypes.Mediation)) Then
                Me.m_shapeguihandler.Refresh()
            End If
        End If

    End Sub

#End Region ' Overrides

End Class




