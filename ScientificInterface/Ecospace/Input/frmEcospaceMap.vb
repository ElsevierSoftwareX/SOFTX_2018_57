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
Imports EwEUtils.Core
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers

#End Region ' Imports

Namespace Ecospace.Basemap

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' This is the master class organizing and combining the different
    ''' basemap view components. In MVC terms, this class is the global
    ''' controller for the ecospace basemap interface.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class frmEcospaceMap

#Region " Private vars "

        ''' <summary>The one and only administration of layers.</summary>
        Private m_layers As New List(Of cDisplayLayer)
        ''' <summary>The one and only control that renders the basemap.</summary>
        Private m_ucBasemap As ucMap = Nothing

        Private m_cmdEditBasemap As cCommand = Nothing

#End Region ' Private vars

#Region " Constructors "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructors

#Region " Public properties "

        Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
                MyBase.UIContext = value
                Me.m_zoomContainer.UIContext = value
                Me.m_zoomToolbar.UIContext = value
            End Set
        End Property

#End Region ' Public properties

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim pm As cPropertyManager = Me.PropertyManager
            Dim source As cEcospaceModelParameters = Me.Core.EcospaceModelParameters()

            ' Initalize m_ucBasemap
            Me.m_ucBasemap = Me.m_zoomContainer.Map()
            Me.m_ucBasemap.UIContext = Me.UIContext
            Me.m_ucBasemap.Title = Me.Core.EcospaceScenarios(Me.Core.ActiveEcospaceScenarioIndex).Name
            Me.m_ucLayers.UIContext = Me.UIContext

            Me.m_ucBasemap.Editable = True
            Me.m_zoomToolbar.AddZoomContainer(Me.m_zoomContainer)

            Me.m_cmdEditBasemap = cmdh.GetCommand("EditBasemap")
            If (Me.m_cmdEditBasemap IsNot Nothing) Then
                Me.m_cmdEditBasemap.AddControl(Me.tsbEditBasemap)
                AddHandler Me.m_cmdEditBasemap.OnPreInvoke, AddressOf OnPreIvokeEditcommand
                AddHandler Me.m_cmdEditBasemap.OnPostInvoke, AddressOf OnPostIvokeEditcommand
            End If

            ' Initialize layers from core data
            Me.LoadCoreValuesToBasemap()

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSpace}

            Me.m_plEditor.Visible = False

        End Sub

        Protected Overrides Sub OnFormClosing(e As System.Windows.Forms.FormClosingEventArgs)

            ' Addresses issue #1251
            ' Store settings before calling baseclass OnFormClosing
            Me.Settings = cMapSettings.Save(Me.m_ucBasemap)

            MyBase.OnFormClosing(e)

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            Me.Visible = False
            Me.SelectedLayer = Nothing

            ' Detach from message sources
            Me.CoreComponents = Nothing
            ' Clean up
            Me.RemoveAllLayers()

            Me.m_zoomToolbar.RemoveZoomContainer(Me.m_zoomContainer)
            Me.m_ucBasemap.UIContext = Nothing
            Me.m_ucLayers.UIContext = Nothing

            Dim cmdh As cCommandHandler = Me.CommandHandler

            If (Me.m_cmdEditBasemap IsNot Nothing) Then
                Me.m_cmdEditBasemap.RemoveControl(Me.tsbEditBasemap)
                RemoveHandler Me.m_cmdEditBasemap.OnPreInvoke, AddressOf OnPreIvokeEditcommand
                RemoveHandler Me.m_cmdEditBasemap.OnPostInvoke, AddressOf OnPostIvokeEditcommand
                Me.m_cmdEditBasemap = Nothing
            End If

            MyBase.OnFormClosed(e)

        End Sub

        Private Sub OnPreIvokeEditcommand(ByVal cmd As cCommand)
            Me.m_ucLayers.LockUpdates()
        End Sub

        Private Sub OnPostIvokeEditcommand(ByVal cmd As cCommand)
            Me.m_ucLayers.UnlockUpdates()
            ' Update map
            Me.m_ucBasemap.Refresh()
        End Sub

        Private Sub OnLayerChanged(ByVal layer As cDisplayLayer, ByVal changeFlag As cDisplayLayer.eChangeFlags)
            Dim layerSelect As cDisplayLayer = Nothing
            ' Is selection change?
            If ((changeFlag And cDisplayLayer.eChangeFlags.Selected) > 0) Then
                ' #Yes: Find newly selected layer
                For Each layerTemp As cDisplayLayer In Me.m_layers
                    ' Got it?
                    If layerTemp.IsSelected Then
                        ' #Yes: remember this
                        layerSelect = layerTemp
                        Exit For
                    End If
                Next
                ' Set selection
                Me.SelectedLayer = layerSelect
            End If
        End Sub

#End Region ' Events

#Region " Load Core Helpers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load fixed core layers from the core basemap data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub LoadCoreValuesToBasemap()

            Me.m_ucLayers.LockUpdates()

            ' Clean-up
            Me.RemoveAllLayers()

            Me.AddData(eVarNameFlags.LayerMPA)
            Me.AddData(eVarNameFlags.LayerPort)
            Me.AddData(eVarNameFlags.LayerSail, False)

            '  -- Reference layer
            Dim l As New cStyleguideImageLayer(Me.UIContext)
            l.Name = ScientificInterfaceShared.My.Resources.HEADER_REFERENCE
            Me.AddLayer(l, l.Name, "EditRefMap")
            l.Renderer.IsVisible = False

            Me.AddData(eVarNameFlags.LayerMigration)
            Me.AddData(eVarNameFlags.LayerRegion)
            Me.AddData(eVarNameFlags.LayerRelPP)
            Me.AddData(eVarNameFlags.LayerRelCin, False)
            Me.AddData(eVarNameFlags.LayerDepth, False)
            Me.AddData(eVarNameFlags.LayerExclusion, False)
            Me.AddData(eVarNameFlags.LayerAdvection, False)
            Me.AddData(eVarNameFlags.LayerHabitatCapacityInput)
            Me.AddData(eVarNameFlags.LayerDriver)
            Me.AddData(eVarNameFlags.LayerImportance)
            Me.AddData(eVarNameFlags.LayerHabitat)

            ' Addresses issue #1251
            cMapSettings.Load(Me.Settings, Me.m_ucBasemap)

            Me.m_ucLayers.UnlockUpdates()

            ' Update map
            Me.m_ucBasemap.Refresh()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper function to create the layers.  
        ''' </summary>
        ''' <param name="varName">The core variable to load basemap data for.</param>
        ''' -------------------------------------------------------------------
        Private Sub AddData(ByVal varName As eVarNameFlags, Optional ByVal bClearGroup As Boolean = True)

            Dim factory As New cLayerFactoryInternal()
            Dim alayers As cDisplayLayer() = factory.GetLayers(Me.UIContext, varName)
            Dim strGroup As String = factory.GetLayerGroup(varName)
            Dim strCommand As String = factory.GetLayerEditCommand(varName)

            ' Need to clear group?
            If bClearGroup Then
                ' #Yes; first remove all layers in the group
                For Each l As cDisplayLayer In Me.m_ucLayers.Layers(strGroup)
                    Me.RemoveLayer(l)
                Next
            End If

            ' (Re)define group
            Me.m_ucLayers.AddGroup(strGroup, strCommand, True, bClearGroup)

            For iLayer As Integer = 0 To alayers.Length - 1
                Me.AddLayer(alayers(iLayer), strGroup, strCommand)
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove all layers.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub RemoveAllLayers()
            Dim alayers As cDisplayLayer() = Me.m_layers.ToArray()
            For Each layer As cDisplayLayer In alayers
                Me.RemoveLayer(layer)
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a single layer.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub AddLayer(ByVal l As cDisplayLayer, ByVal strGroup As String, strCommand As String)

            Me.m_layers.Add(l)
            Me.m_ucBasemap.AddLayer(l)
            Me.m_ucLayers.AddLayer(l, strGroup, strCommand)

            AddHandler l.LayerChanged, AddressOf OnLayerChanged

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a single layer.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub RemoveLayer(ByVal l As cDisplayLayer)

            Me.m_layers.Remove(l)
            Me.m_ucBasemap.RemoveLayer(l)
            Me.m_ucLayers.RemoveLayer(l)

            If (ReferenceEquals(Me.SelectedLayer, l)) Then
                Me.SelectedLayer = Nothing
            End If

            RemoveHandler l.LayerChanged, AddressOf OnLayerChanged
            l.Dispose()
        End Sub

#End Region ' Load core helpers

#Region " Internals "

        ''' <summary>The layer currently selected by the user.</summary>
        Private m_layerSelected As cDisplayLayer = Nothing
        ''' <summary>The editor belonging to the selected layer, if any.</summary>
        Private m_editorGUISelected As ucLayerEditor = Nothing

        Private Property SelectedLayer() As cDisplayLayer
            Get
                Return Me.m_layerSelected
            End Get
            Set(ByVal layer As cDisplayLayer)

                If ReferenceEquals(layer, Me.m_layerSelected) Then Return

                Me.SuspendLayout()

                If (Me.m_layerSelected IsNot Nothing) Then
                    ' Has editor GUI?
                    If (Me.m_editorGUISelected IsNot Nothing) Then
                        ' #Yes: remove layer editor GUI
                        RemoveHandler Me.m_editorGUISelected.OnChanged, AddressOf OnLayerEditorChanged
                        Me.m_plEditor.Controls.Remove(Me.m_editorGUISelected)
                        Me.m_editorGUISelected = Nothing
                    End If

                    If (TypeOf Me.m_layerSelected Is cDisplayLayerRaster) Then
                        DirectCast(Me.m_layerSelected, cDisplayLayerRaster).Editor.DestroyEditorControl()
                    End If
                End If

                Me.m_layerSelected = layer

                If (Me.m_layerSelected IsNot Nothing) Then

                    ' Add layer editor GUI
                    If (TypeOf Me.m_layerSelected Is cDisplayLayerRaster) Then
                        Me.m_editorGUISelected = DirectCast(Me.m_layerSelected, cDisplayLayerRaster).Editor.CreateEditorControl()
                    End If

                    If (Me.m_editorGUISelected IsNot Nothing) Then
                        Me.m_plEditor.Height = Me.m_editorGUISelected.Height
                        Me.m_editorGUISelected.Dock = DockStyle.Fill
                        Me.m_plEditor.Controls.Add(Me.m_editorGUISelected)
                        AddHandler Me.m_editorGUISelected.OnChanged, AddressOf OnLayerEditorChanged
                    End If
                End If

                Me.ResumeLayout()

                Me.m_plEditor.Visible = (Me.m_editorGUISelected IsNot Nothing)

            End Set
        End Property

        Private Sub OnLayerEditorChanged(ByVal editor As ucLayerEditor)
            ' NOP
        End Sub

#End Region ' Internals

#Region " Mandatory overrides "

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)

            If Me.IsDisposed Then Return

            If (msg.Source = eCoreComponentType.EcoSpace) Then
                ' Refresh basemap on ANY data added or removed message from Ecospace
                If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                    ' Redraw it all
                    Me.Invalidate()
                ElseIf (msg.Type = eMessageType.DataValidation And msg.HasVariable(eVarNameFlags.IsMigratory)) Then
                    ' Refresh the migration map group
                    Me.AddData(eVarNameFlags.LayerMigration, True)
                ElseIf (msg.Type = eMessageType.DataModified) Then
                    ' Refresh only map
                    Me.m_ucBasemap.Invalidate()
                End If
            End If

            ' Trigger refresh for external data icons. Should really not be here, but ok
            If ((msg.DataType = eDataTypes.EcospaceSpatialDataConnection) Or (msg.DataType = eDataTypes.EcospaceSpatialDataSource)) Then
                Me.m_ucLayers.Invalidate(True)
            End If

        End Sub

#End Region ' Mandatory overrides

    End Class

End Namespace
