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
Imports EwECore.Auxiliary
Imports EwECore.Style
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map

    ''' =======================================================================
    ''' <summary>
    ''' Factory for returning <see cref="cDisplayLayer">display layers</see> for 
    ''' given <see cref="cEcospaceLayer">Ecospace data layers.</see>
    ''' </summary>
    ''' =======================================================================
    Public Class cLayerFactoryBase

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Build user interface layer(s) for given core data.
        ''' </summary>
        ''' <param name="uic">UI context to connect layer to.</param>
        ''' <param name="varName">Name of the core variable to wrap</param>
        ''' <returns>An array of layers</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetLayers(ByVal uic As cUIContext,
                                              ByVal varName As eVarNameFlags) As cDisplayLayerRaster()

            Dim lLayers As New List(Of cDisplayLayerRaster)

            Dim core As cCore = uic.Core
            Dim bmd As cEcospaceBasemap = core.EcospaceBasemap
            Dim layer As cDisplayLayerRaster = Nothing
            Dim ad As cAuxiliaryData = Nothing
            Dim avs As cVisualStyle() = Nothing
            Dim renderer As cLayerRenderer = Nothing
            Dim editor As cLayerEditor = Nothing
            Dim vs As cVisualStyle = Nothing
            Dim fmt As New cVarnameTypeFormatter()

            Select Case varName

                Case eVarNameFlags.LayerDepth

                    ad = Me.GetAuxillaryData(core, varName)

                    vs = ad.VisualStyle
                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererDepth(vs)
                    editor = New cLayerEditorDepth()
                    layer = New cDisplayLayerRaster(uic, bmd.LayerDepth, renderer, editor, bmd, varName)
                    lLayers.Add(layer)

                Case eVarNameFlags.LayerHabitat

                    avs = uic.StyleGuide.GetVisualStyles(core.nHabitats, cStyleGuide.eBrushType.Glyphs)

                    For iHabitat As Integer = 1 To core.nHabitats - 1

                        Dim hab As cEcospaceHabitat = core.EcospaceHabitats(iHabitat)

                        ' Get or create Visual Style
                        ad = GetAuxillaryData(core, varName, iHabitat)
                        vs = ad.VisualStyle
                        If (vs Is Nothing) Then
                            vs = avs(iHabitat - 1)
                            ad.AllowValidation = False
                            ad.VisualStyle = vs
                            ad.AllowValidation = True
                        End If

                        ' Create layer
                        renderer = New cLayerRendererBitmap(vs)
                        renderer.RenderMode = Definitions.eLayerRenderType.Grouped

                        editor = New cLayerEditorHabitat()
                        layer = New cDisplayLayerRaster(uic, bmd.LayerHabitat(iHabitat), renderer, editor, hab, eVarNameFlags.Name, sValueClear:=0)
                        lLayers.Add(layer)

                    Next iHabitat

                Case eVarNameFlags.LayerHabitatCapacityInput

                    If (core.nGroups > 0) Then

                        ad = GetAuxillaryData(core, varName)
                        vs = ad.VisualStyle
                        If (vs Is Nothing) Then vs = New cVisualStyle(ad)

                        renderer = New cLayerRendererValue(vs)
                        renderer.ScaleMin = 0
                        renderer.RenderMode = Definitions.eLayerRenderType.Selected

                        editor = New cLayerEditorGroup(GetType(ucLayerEditorHabitatCapacity))
                        layer = New cDisplayLayerRasterBundle(uic, bmd.Layers(eVarNameFlags.LayerHabitatCapacityInput),
                                                renderer, editor, eCoreCounterTypes.nGroups, bmd, eVarNameFlags.LayerHabitatCapacityInput)

                        lLayers.Add(layer)
                    End If

                Case eVarNameFlags.LayerRegion

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle
                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererValue(vs)
                    renderer.ScaleMin = 0
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected

                    editor = New cLayerEditorRegion()

                    layer = New cDisplayLayerRaster(uic, bmd.LayerRegion, renderer, editor, bmd, eVarNameFlags.LayerRegion)
                    lLayers.Add(layer)

                Case eVarNameFlags.LayerMPA

                    avs = uic.StyleGuide.GetVisualStyles(core.nMPAs, cStyleGuide.eBrushType.HatchPattern)

                    For iMPA As Integer = 1 To core.nMPAs

                        Dim mpa As cEcospaceMPA = core.EcospaceMPAs(iMPA)
                        ad = GetAuxillaryData(core, varName, iMPA)

                        ' Get or create Visual Style
                        vs = ad.VisualStyle
                        If (vs Is Nothing) Then
                            vs = avs(iMPA)
                            ad.AllowValidation = False
                            ad.VisualStyle = vs
                            ad.AllowValidation = True
                        End If

                        ' Create layer
                        renderer = New cLayerRendererHatch(vs)
                        renderer.RenderMode = Definitions.eLayerRenderType.Always

                        editor = New cLayerEditorTwoState()
                        layer = New cDisplayLayerRaster(uic, bmd.LayerMPA(iMPA), renderer, editor, mpa, eVarNameFlags.Name, 1, 0)

                        lLayers.Add(layer)

                    Next iMPA

                Case eVarNameFlags.LayerRelPP

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle

                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererValue(vs)
                    'renderer.ScaleMin = 0
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected

                    editor = New cLayerEditorRange()
                    layer = New cDisplayLayerRaster(uic, bmd.LayerRelPP, renderer, editor, bmd, eVarNameFlags.LayerRelPP)

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerRelCin

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle

                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererValue(vs)
                    renderer.ScaleMin = 0
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected

                    editor = New cLayerEditorRange()
                    layer = New cDisplayLayerRaster(uic, bmd.LayerRelCin, renderer, editor, bmd, eVarNameFlags.LayerRelCin)

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerMigration

                    For iLayer As Integer = 1 To core.nGroups
                        Dim grp As cEcospaceGroupInput = core.EcospaceGroupInputs(iLayer)
                        If grp.IsMigratory Then
                            Dim src As cEcospaceLayerMigration = core.EcospaceBasemap.LayerMigration(iLayer)
                            ad = Me.GetAuxillaryData(core, varName, iLayer)
                            vs = ad.VisualStyle
                            If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                            renderer = New cLayerRendererValue(vs)
                            renderer.RenderMode = Definitions.eLayerRenderType.Selected
                            DirectCast(renderer, cLayerRendererValue).SuppressZero = True
                            editor = New cLayerEditorMigration()
                            layer = New cDisplayLayerRaster(uic, src, renderer, editor, src, eVarNameFlags.Name)
                            lLayers.Add(layer)
                        End If
                    Next

                Case eVarNameFlags.LayerAdvection

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle

                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererWindEwE5(vs)
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected
                    editor = New cLayerEditorVelocity()

                    Dim wrap As New cEcospaceLayerVelocity(core, fmt.GetDescriptor(eVarNameFlags.LayerAdvection), bmd, eVarNameFlags.LayerAdvection)
                    layer = New cDisplayLayerRaster(uic, wrap, renderer, editor, bmd, eVarNameFlags.LayerAdvection)

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerWind

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle

                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererWindEwE5(vs)
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected
                    editor = New cLayerEditorVelocity()

                    Dim wrap As New cEcospaceLayerVelocity(core, fmt.GetDescriptor(eVarNameFlags.LayerWind), bmd, eVarNameFlags.LayerWind)
                    layer = New cDisplayLayerRaster(uic, wrap, renderer, editor, bmd, eVarNameFlags.LayerWind)

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerUpwelling

                    ' ToDo: globalize this

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle

                    If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                    renderer = New cLayerRendererUpwelling(vs)
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected
                    editor = New cLayerEditorUpwelling()
                    layer = New cDisplayLayerRaster(uic, bmd.LayerUpwelling, renderer, editor, bmd, eVarNameFlags.LayerUpwelling)
                    layer.Name = "Upwelling"

                    lLayers.Add(layer)

                Case eVarNameFlags.LayerPort

                    If (core.nFleets > 0) Then
                        ad = GetAuxillaryData(core, varName)
                        vs = ad.VisualStyle

                        If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                        renderer = New cLayerRendererSymbol(vs)
                        renderer.RenderMode = Definitions.eLayerRenderType.Always
                        editor = New cLayerEditorPorts(GetType(ucLayerEditorPort))
                        layer = New cDisplayLayerRasterBundle(uic, bmd.Layers(eVarNameFlags.LayerPort), renderer, editor, eCoreCounterTypes.nFleets, bmd, eVarNameFlags.LayerPort, CSng(True), CSng(False))
                        lLayers.Add(layer)
                    End If

                Case eVarNameFlags.LayerSail

                    If (core.nFleets > 0) Then

                        ad = GetAuxillaryData(core, varName)
                        vs = ad.VisualStyle

                        If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                        renderer = New cLayerRendererValue(vs)
                        renderer.ScaleMin = 0
                        renderer.RenderMode = Definitions.eLayerRenderType.Selected
                        editor = New cLayerEditorSailCost(GetType(ucLayerEditorSailCost))
                        layer = New cDisplayLayerRasterBundle(uic, bmd.Layers(eVarNameFlags.LayerSail), renderer, editor, eCoreCounterTypes.nFleets, bmd, eVarNameFlags.LayerSail)

                        lLayers.Add(layer)
                    End If

                Case eVarNameFlags.LayerImportance

                    For iLayer As Integer = 1 To core.nImportanceLayers

                        Dim src As cEcospaceLayerImportance = core.EcospaceBasemap.LayerImportance(iLayer)
                        ad = GetAuxillaryData(core, varName, iLayer)

                        vs = ad.VisualStyle
                        If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                        renderer = New cLayerRendererValue(vs)
                        renderer.ScaleMin = 0
                        renderer.RenderMode = Definitions.eLayerRenderType.Selected
                        editor = New cLayerEditorRange()
                        layer = New cDisplayLayerRaster(uic, src, renderer, editor, src, eVarNameFlags.Name)

                        lLayers.Add(layer)

                    Next iLayer

                Case eVarNameFlags.LayerDriver

                    For iLayer As Integer = 1 To core.nEnvironmentalDriverLayers

                        Dim src As cEcospaceLayerDriver = core.EcospaceBasemap.LayerDriver(iLayer)
                        ad = GetAuxillaryData(core, varName, iLayer)
                        vs = ad.VisualStyle

                        If (vs Is Nothing) Then vs = New cVisualStyle(ad)
                        renderer = New cLayerRendererValue(vs)
                        'renderer.ScaleMin = 0
                        renderer.RenderMode = Definitions.eLayerRenderType.Selected
                        editor = New cLayerEditorRange()
                        layer = New cDisplayLayerRaster(uic, src, renderer, editor, src, eVarNameFlags.Name)

                        lLayers.Add(layer)

                    Next iLayer

                Case eVarNameFlags.LayerExclusion

                    Dim src As cEcospaceLayerExclusion = core.EcospaceBasemap.LayerExclusion

                    ad = GetAuxillaryData(core, varName)
                    vs = ad.VisualStyle

                    If (vs Is Nothing) Then
                        vs = New cVisualStyle()
                        vs.ForeColour = Color.Red
                        vs.BackColour = Color.OrangeRed
                        vs.HatchStyle = Drawing2D.HatchStyle.DiagonalCross
                        ad.AllowValidation = False
                        ad.VisualStyle = vs
                        ad.AllowValidation = True
                    End If
                    renderer = New cLayerRendererExclusion(vs)
                    renderer.RenderMode = Definitions.eLayerRenderType.Selected
                    editor = New cLayerEditorTwoState(GetType(ucLayerEditorExclusion), False)
                    layer = New cDisplayLayerRaster(uic, src, renderer, editor, src, eVarNameFlags.Name, CSng(True), CSng(False))

                    lLayers.Add(layer)

                Case Else
                    Debug.Assert(False, "No layers available for this varname")

            End Select

            Return lLayers.ToArray()

        End Function

        Public Overridable Function GetLayerGroup(ByVal varName As eVarNameFlags) As String

            Dim strGroup As String = ""
            Select Case varName

                Case eVarNameFlags.LayerDepth,
                     eVarNameFlags.LayerExclusion,
                     eVarNameFlags.LayerRelPP,
                     eVarNameFlags.LayerRelCin
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_BASEMAP

                Case eVarNameFlags.LayerHabitat
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_HABITATS

                Case eVarNameFlags.LayerHabitatCapacity,
                     eVarNameFlags.LayerHabitatCapacityInput
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_HABCAP

                Case eVarNameFlags.LayerRegion
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_REGIONS

                Case eVarNameFlags.LayerMPA
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_MPAS

                Case eVarNameFlags.LayerMigration
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_MIGRATION

                Case eVarNameFlags.LayerPort,
                      eVarNameFlags.LayerSail
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_FISHING

                Case eVarNameFlags.LayerImportance
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_IMPORTANCE

                Case eVarNameFlags.LayerDriver
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_ENVDRIVERS

                Case eVarNameFlags.LayerBiomassForcing
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_BIOMASSFORCING

                Case eVarNameFlags.LayerBiomassRelativeForcing
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_BIOMASSRELATIVEFORCING '"Relative biomass forcing"

                Case eVarNameFlags.LayerAdvection,
                     eVarNameFlags.LayerWind,
                     eVarNameFlags.LayerUpwelling
                    strGroup = My.Resources.ECOSPACE_LAYERGROUP_ADVECTION

            End Select
            Return strGroup

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of a <see cref="ScientificInterfaceShared.Commands.cCommand"/> that can
        ''' be triggered to modify the <see cref="ICoreInputOutput">core items </see>
        ''' reflected by a type of layer.
        ''' </summary>
        ''' <param name="varName">The <see cref="eVarNameFlags"/> to obtain the 
        ''' edit command for.</param>
        ''' <returns>A command name, or an empty string if not applicable.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetLayerEditCommand(ByVal varName As eVarNameFlags) As String

            Dim strCommand As String = ""
            Select Case varName

                Case eVarNameFlags.LayerHabitat
                    strCommand = cEditHabitatsCommand.cCOMMAND_NAME

                Case eVarNameFlags.LayerMPA
                    strCommand = cEditMPAsCommand.cCOMMAND_NAME

                Case eVarNameFlags.LayerRegion
                    strCommand = cEditRegionsCommand.cCOMMAND_NAME

                Case eVarNameFlags.LayerImportance
                    strCommand = cEditImportanceLayersCommand.cCOMMAND_NAME

                Case eVarNameFlags.LayerDriver
                    strCommand = cEditDriverLayersCommand.cCOMMAND_NAME

            End Select
            Return strCommand

        End Function

        Public Overridable Function GetAuxillaryData(core As cCore, l As cEcospaceLayer) As cAuxiliaryData

            If (l Is Nothing) Then Return Nothing
            Return GetAuxillaryData(core, l.VarName, l.Index)

        End Function

        Public Overridable Function GetAuxillaryData(core As cCore, vn As eVarNameFlags, Optional iIndex As Integer = 0) As cAuxiliaryData

            If (core Is Nothing) Then Return Nothing

            Dim iScenario As Integer = core.ActiveEcospaceScenarioIndex
            If (iScenario <= 0) Then Return Nothing

            Dim DBID As Integer = core.EcospaceScenarios(iScenario).DBID
            Dim dt As eDataTypes = eDataTypes.NotSet

            Dim layers As cEcospaceLayer() = core.EcospaceBasemap.Layers(vn)
            Dim layer As cEcospaceLayer = layers(Math.Max(iIndex - 1, 0))

            dt = layer.DataType

            Select Case vn
                Case eVarNameFlags.LayerHabitat
                    DBID = core.EcospaceHabitats(iIndex).DBID
                Case eVarNameFlags.LayerMPA
                    DBID = core.EcospaceMPAs(iIndex).DBID
            End Select

            Dim key As New cValueID(dt, DBID, eVarNameFlags.Name)
            Return core.AuxillaryData(key)

        End Function

    End Class

End Namespace
