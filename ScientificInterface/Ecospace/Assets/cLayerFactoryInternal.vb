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
Imports EwEUtils.Core
Imports EwECore.Auxiliary
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers

#End Region ' Imports

Namespace Ecospace.Basemap.Layers

    ''' =======================================================================
    ''' <summary>
    ''' Factory for returning a configured <see cref="cDisplayLayer">display layer</see> 
    ''' for <see cref="cEcospaceLayer">Ecospace map data.</see>
    ''' </summary>
    ''' =======================================================================
    Friend Class cLayerFactoryInternal
        Inherits cLayerFactoryBase

#Region " Constants "

        Public Const cECOSEED_LAYER_NOVALUE As Integer = 0
        Public Const cECOSEED_LAYER_CURRENTVALUE As Integer = 1
        Public Const cECOSEED_LAYER_BESTVALUE As Integer = 2

#End Region ' Constants

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Build layer(s) for a given core data layer name.
        ''' </summary>
        ''' <param name="uic">UI context to connect layer to.</param>
        ''' <param name="layerData">Optional data to attach to the layer. If no
        ''' data is given the layer will attempt to get its data from the 
        ''' Ecospace basemap.</param>
        ''' <returns>An array of layers</returns>
        ''' -------------------------------------------------------------------
        Public Overloads Function GetLayers(ByVal uic As cUIContext, _
                                            ByVal varName As eVarNameFlags, _
                                            Optional ByVal layerData As cEcospaceLayer = Nothing) As cDisplayLayerRaster()

            Dim lLayers As New List(Of cDisplayLayerRaster)

            Dim core As cCore = uic.Core
            Dim bmd As cEcospaceBasemap = core.EcospaceBasemap
            Dim layer As cDisplayLayerRaster = Nothing
            Dim key As cValueID = Nothing
            Dim ad As cAuxiliaryData = Nothing
            Dim avs As cVisualStyle() = Nothing
            Dim renderer As cRasterLayerRenderer = Nothing
            Dim editor As cLayerEditor = Nothing
            Dim vs As cVisualStyle = Nothing

            Select Case varName

                Case eVarNameFlags.LayerMPASeed

                    vs = New cVisualStyle()
                    vs.ForeColour = Color.CornflowerBlue

                    ' Represent MPA seeds as a solid colour
                    renderer = New cLayerRendererSymbol(vs)
                    renderer.RenderMode = eLayerRenderType.Always

                    editor = New cLayerEditorTwoState()
                    If layerData Is Nothing Then layerData = bmd.LayerMPASeed
                    layer = New cDisplayLayerRaster(uic, layerData, renderer, editor, bmd, eVarNameFlags.LayerMPASeed, 1, 0)

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerMPASeedCurrent

                    Debug.Assert(layerData IsNot Nothing, "Cannot link to core data")

                    vs = New cVisualStyle()
                    vs.ForeColour = Color.LightGreen

                    ' Represent MPA seeds as a solid colour
                    renderer = New cLayerRendererSymbol(vs)
                    renderer.RenderMode = eLayerRenderType.Always
                    editor = New cLayerEditorTwoState()
                    layer = New cDisplayLayerRaster(uic, layerData, renderer, editor, Nothing, eVarNameFlags.LayerMPASeedCurrent, cECOSEED_LAYER_CURRENTVALUE, cECOSEED_LAYER_NOVALUE)
                    layer.Name = layerData.Name
                    layer.Editor.IsReadOnly = True

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerMPASeedBest

                    Debug.Assert(layerData IsNot Nothing, "Cannot link to core data")

                    vs = New cVisualStyle()
                    vs.ForeColour = Color.DarkGreen
                    vs.BackColour = Color.Transparent

                    ' Represent MPA seeds as a solid colour
                    renderer = New cLayerRendererSymbol(vs)
                    renderer.RenderMode = eLayerRenderType.Always
                    editor = New cLayerEditorTwoState()
                    layer = New cDisplayLayerRaster(uic, layerData, renderer, editor, Nothing, eVarNameFlags.LayerMPASeedBest, cECOSEED_LAYER_BESTVALUE, cECOSEED_LAYER_NOVALUE)
                    layer.Name = layerData.Name
                    layer.Editor.IsReadOnly = True

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerMPARandom

                    If (layerData IsNot Nothing) Then
                        vs = New cVisualStyle()
                        vs.ForeColour = Color.Black
                        vs.BackColour = Color.Blue

                        renderer = New cLayerRendererValue(vs)
                        renderer.RenderMode = eLayerRenderType.Always
                        editor = New cLayerEditorRange()
                        layer = New cDisplayLayerRaster(uic, layerData, renderer, editor)
                        layer.Name = layerData.Name
                        layer.Editor.IsReadOnly = True

                        lLayers.Add(layer)
                    End If

                Case Else
                    ' Return default
                    lLayers.AddRange(MyBase.GetLayers(uic, varName))

            End Select

            Return lLayers.ToArray()

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cLayerFactoryBase.GetLayerGroup"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function GetLayerGroup(ByVal varName As eVarNameFlags) As String

            Dim strGroup As String = ""
            Select Case varName

                Case eVarNameFlags.LayerMPASeed,
                    eVarNameFlags.LayerMPASeedBest,
                    eVarNameFlags.LayerMPASeedCurrent
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_ECOSEED

                Case eVarNameFlags.LayerMPARandom
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_MPARANDOM

                Case Else
                    Return MyBase.GetLayerGroup(varName)

            End Select
            Return strGroup

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a collection with the EwE foundation layers.
        ''' </summary>
        ''' <param name="uic">The UI context to plunder layers from.</param>
        ''' <returns>An array of <see cref="cDisplayLayerRaster"/>s.</returns>
        ''' -------------------------------------------------------------------
        Public Function BaseRasterLayers(uic As cUIContext) As cDisplayLayerRaster()

            Dim lLayers As New List(Of cDisplayLayerRaster)

            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerDepth))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerMPA))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerHabitat))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerHabitatCapacityInput))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerRelPP))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerRelCin))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerImportance))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerSail))
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerPort))
            ' Add driver layers to the base list
            lLayers.AddRange(Me.GetLayers(uic, EwEUtils.Core.eVarNameFlags.LayerDriver))

            Return lLayers.ToArray()

        End Function
    End Class

End Namespace