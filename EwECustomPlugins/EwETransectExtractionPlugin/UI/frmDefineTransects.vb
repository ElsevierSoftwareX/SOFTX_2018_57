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
Imports System.Drawing
Imports System.Windows.Forms
Imports EwETransectExtractionPlugin
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Style.cStyleGuide

#End Region ' Imports

Public Class frmDefineTransects

    Private m_layerRaster As cTransectLayer = Nothing
    Private WithEvents m_data As cTransectDatastructures = Nothing

    Public Sub New(uic As cUIContext)
        Me.InitializeComponent()
        Me.UIContext = uic
        Me.m_data = cTransectDatastructures.Instance(uic.Core)

        Me.Text = My.Resources.CAPTION_IN
        Me.TabText = Me.Text
    End Sub

#Region " Form overrides "

    Public Overrides Property UIContext As cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(value As cUIContext)
            MyBase.UIContext = value
            Me.m_mapzoom.UIContext = Me.UIContext
            Me.m_toolstrip.UIContext = Me.UIContext
        End Set
    End Property

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Dim factory As New cLayerFactoryBase()

        Dim DisplayVector As New cTransectVectorDisplay(Me.UIContext)
        DisplayVector.IsSelected = True
        DisplayVector.Data = Me.m_data

        Me.m_layerRaster = New cTransectLayer(Me.Core, Me.m_data)
        Dim DisplayRaster As New cTransectRasterDisplay(Me.UIContext, Me.m_layerRaster)

        Me.m_mapzoom.Map.Editable = True
        Me.m_mapzoom.Map.AddLayer(DisplayVector)
        Me.m_mapzoom.Map.AddLayer(DisplayRaster)

        Me.m_toolstrip.AddZoomContainer(Me.m_mapzoom)

        For Each l As cDisplayLayer In factory.GetLayers(Me.UIContext, eVarNameFlags.LayerMPA)
            Me.m_mapzoom.Map.AddLayer(l)
        Next

        For Each l As cDisplayLayer In factory.GetLayers(Me.UIContext, eVarNameFlags.LayerDepth)
            l.RenderMode = ScientificInterfaceShared.Definitions.eLayerRenderType.Always
            Me.m_mapzoom.Map.AddLayer(l)
        Next

        Me.UpdateTransects()

    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Me.m_btnDeleteTransect.Enabled = (Me.m_data.Selection IsNot Nothing)

    End Sub

#End Region ' Form overrides

#Region " Events "

    Private Sub OnDeleteTransect(sender As Object, e As EventArgs) _
        Handles m_btnDeleteTransect.Click
        Me.m_data.Delete(Me.m_data.Selection)
    End Sub

    Private Sub OnTransectAdded(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectAdded
        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True
        Me.UpdateTransects()
        Me.UpdateControls()
        Me.m_bInUpdate = False
    End Sub

    Private Sub OnTransectRemoved(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectRemoved
        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True
        Me.UpdateTransects()
        Me.UpdateMap()
        Me.UpdateControls()
        Me.m_bInUpdate = False
    End Sub

    Private Sub OnTransectSelected(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectSelected
        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True
        Me.m_lbxTransects.SelectedItem = transect
        Me.UpdateMap()
        Me.UpdateControls()
        Me.m_bInUpdate = False
    End Sub

    Private Sub OnTransectsChanged(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectChanged
        Me.m_layerRaster.Invalidate()
    End Sub

    Private Sub OnTransitSelected(sender As Object, e As EventArgs) _
        Handles m_lbxTransects.SelectedIndexChanged
        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True
        Dim t As cTransect = Me.SelectedTransect()
        Me.m_data.Selection = t
        Me.UpdateMap()
        Me.UpdateControls()
        Me.m_bInUpdate = False
    End Sub

    Private Sub OnTransectDoubleClick(sender As Object, e As EventArgs) _
        Handles m_lbxTransects.DoubleClick

        Dim t As cTransect = Me.SelectedTransect
        Dim dlg As New dlgEditTransect(Me.UIContext, t)
        If dlg.ShowDialog(Me) = DialogResult.OK Then
            ' Ugh
            Me.m_lbxTransects.BeginUpdate()
            Me.m_lbxTransects.Items(Me.m_lbxTransects.SelectedIndex) = t
            Me.m_lbxTransects.EndUpdate()
        End If

    End Sub


#End Region ' Events

#Region " Internals "

    Private m_bInUpdate As Boolean = False

    Private Function SelectedTransect() As cTransect
        Return DirectCast(Me.m_lbxTransects.SelectedItem, cTransect)
    End Function

    Private Sub UpdateMap()
        Me.m_mapzoom.Map.Refresh()
    End Sub

    Private Sub UpdateTransects()
        Me.m_bInUpdate = True
        Me.m_lbxTransects.Items.Clear()
        For Each t As cTransect In Me.m_data.Transects
            Me.m_lbxTransects.Items.Add(t)
        Next
        Me.m_bInUpdate = False
    End Sub

#End Region ' Internals

End Class