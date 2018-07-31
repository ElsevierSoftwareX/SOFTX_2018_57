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
Imports ScientificInterfaceShared.Forms

#End Region

''' =======================================================================
''' <summary>
''' Form baseclass for implementing an Ecosim 'Apply Forcing' or 'Apply 
''' Mediation' interface.
''' </summary>
''' =======================================================================
Public Class frmApplyShapeBase
    Inherits frmEwE

    Public Sub New()
        Me.InitializeComponent()
    End Sub

#Region " Baseclass overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If Me.UIContext Is Nothing Then Return
        Me.Grid.UIContext = Me.UIContext

        ' Hook up to core messages
        ' * Shapes manager to refresh lists of avialable FFs
        ' * Ecopath to refresh lists of available groups
        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.ShapesManager, eCoreComponentType.EcoPath, eCoreComponentType.MediatedInteractionManager}

    End Sub

#End Region ' Baseclass overrides

#Region " Base functionality "

    Protected Sub ClearAll()
        Me.Grid.ClearAllPairs()
    End Sub

    Protected Sub SetAll()
        Me.Grid.SetAllPairs()
    End Sub

#End Region ' Base functionality

#Region " Mandatory overrides "

    <CLSCompliant(False)> _
    Protected Overridable ReadOnly Property Grid() As gridApplyShapeBase
        Get
            Return Nothing
        End Get
    End Property

    Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)

        Dim bMustRedimension As Boolean = False
        Dim bMustUpdate As Boolean = False

        If (msg.Source = eCoreComponentType.ShapesManager) Then
            If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                ' Redimension when number of shapes has changed
                bMustRedimension = True
            End If
        End If

        If (msg.Source = eCoreComponentType.MediatedInteractionManager) Then
            ' Update content to show new assignments
            bMustUpdate = True
        End If

        If bMustRedimension Then
            Me.Grid.RefreshContent()
        Else
            If bMustUpdate Then
                Me.Grid.UpdateContent()
            End If
        End If
    End Sub

#End Region ' Mandatory overrides

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmApplyShapeBase))
        Me.SuspendLayout()
        '
        'frmApplyShapeBase
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Name = "frmApplyShapeBase"
        Me.TabText = ""
        Me.ResumeLayout(False)

    End Sub
End Class