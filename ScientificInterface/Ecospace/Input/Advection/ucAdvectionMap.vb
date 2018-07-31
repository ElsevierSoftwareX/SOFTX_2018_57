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
Imports EwEUtils.Core
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace.Advection

    ''' <summary>
    ''' Base control for implementing maps on the advection form.
    ''' </summary>
    Public Class ucAdvectionMap
        Implements IUIElement

#Region " Private vars "

        ''' <summary>UI context to operate on.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>The raster layer that can be edited in this map, if any.</summary>
        Private m_layerData As cDisplayLayerRaster = Nothing

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Private vars

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                If Me.m_uic IsNot Nothing Then
                    Me.ClearMap()
                End If
                Me.m_uic = value
                Me.m_zoomctrl.UIContext = Me.m_uic
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="ucMapZoom">Zoom control</see> that wraps the
        ''' embedded <see cref="Map">Map</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ZoomCtrl() As ucMapZoom
            Get
                Return Me.m_zoomctrl
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="ucMap">Map control</see> displayed here.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Map() As ScientificInterfaceShared.Controls.Map.ucMap
            Get
                Return Me.m_zoomctrl.Map
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the layer that the user can edit in this map.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property DataLayer() As cDisplayLayerRaster
            Get
                Return Me.m_layerData
            End Get
        End Property

#End Region ' Public access

#Region " Overridables "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Specify the <see cref="eVarNameFlags">var name</see> identifying the
        ''' one editable layer in the attached <see cref="Map">map</see>.
        ''' </summary>
        ''' <returns>A variable name, or <see cref="eVarNameFlags.NotSet">NotSet</see>
        ''' if the user cannot edit this map.</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function DataLayerVariable() As eVarNameFlags
            Return eVarNameFlags.NotSet
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Specify whether the <see cref="DataLayer">data layer</see> is
        ''' editable.
        ''' </summary>
        ''' <returns>
        ''' True by default.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function IsDataInput() As Boolean
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Specify which background layers to display in the map, in addition
        ''' to the already present <see cref="eVarNameFlags.LayerDepth">depth layer</see>.
        ''' </summary>
        ''' <returns>An array of <see cref="eVarNameFlags">variable names</see>
        ''' specifying additional background layers.</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function BackgroundLayers() As eVarNameFlags()
            Return New eVarNameFlags() {eVarNameFlags.LayerDepth}
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the state and content of local controls.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub UpdateControls()

            ' Set pane title to the name of the data layer
            Me.m_hdrTitle.Text = Me.DataLayer.Name

        End Sub

#End Region ' Overridables

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            If (Me.DesignMode = True) Then Return
            Me.PopulateMap()
            Me.UpdateControls()
        End Sub

        Protected Overrides Sub OnEnabledChanged(ByVal e As System.EventArgs)
            MyBase.OnEnabledChanged(e)
            Me.UpdateControls()
        End Sub

        Protected Overridable Sub OnLayerChanged(ByVal l As cDisplayLayer, ByVal changeFlags As cDisplayLayer.eChangeFlags)
            If ((changeFlags And cDisplayLayer.eChangeFlags.Editable) > 0) Then Me.UpdateControls()
        End Sub

#End Region ' Events

#Region " Internal implementation "

        Private Sub PopulateMap()

            ' Sanity checks
            Debug.Assert(Me.DataLayerVariable <> eVarNameFlags.NotSet)

            ' Add data layer
            Me.m_layerData = Me.AddLayer(Me.DataLayerVariable, Me.IsDataInput)
            Me.m_zoomctrl.Map.Editable = Me.IsDataInput

            ' Add depth layer
            Me.AddLayer(eVarNameFlags.LayerDepth, False)

            ' Add additional background layers
            If (Me.BackgroundLayers IsNot Nothing) Then
                For Each vn As eVarNameFlags In Me.BackgroundLayers
                    Me.AddLayer(vn, False)
                Next
            End If

            ' Start observing data layer changes
            AddHandler Me.m_layerData.LayerChanged, AddressOf OnLayerChanged

        End Sub

        Protected Overridable Sub ClearMap()

            If (Me.m_layerData IsNot Nothing) Then
                RemoveHandler Me.m_layerData.LayerChanged, AddressOf OnLayerChanged
                Me.m_layerData = Nothing
            End If

            Me.m_zoomctrl.Map.Clear()

        End Sub

        Private Function AddLayer(ByVal vn As eVarNameFlags, ByVal bEditable As Boolean) As cDisplayLayerRaster

            Dim factory As New cLayerFactoryInternal()
            Dim layers() As cDisplayLayerRaster = factory.GetLayers(Me.m_uic, vn)
            Dim l As cDisplayLayerRaster = Nothing

            If (layers Is Nothing) Then Return Nothing
            If (layers.Length = 0) Then
                ' Do not assert; there may be no habitat layers defined, for instance. This case it 100% valid.
                ' Debug.Assert(False, "No layers found for varname " & vn)
                Return Nothing
            End If

            For Each l In layers
                l.RenderMode = eLayerRenderType.Always
                If bEditable Then
                    l.Editor.IsEditable = True
                    l.IsSelected = True
                Else
                    l.Editor.IsEditable = False
                End If
                Me.m_zoomctrl.Map.AddLayer(l)
            Next

            Return l

        End Function

#End Region ' Internal implementation

    End Class

End Namespace
