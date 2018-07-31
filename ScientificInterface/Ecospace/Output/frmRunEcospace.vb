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

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading
Imports EwECore
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecospace

    'ToDo For Graph Plot Options added after 6.4 release
    'Move Options panel up to below "Distribution of" panel
    'Make it clear that one controls the maps and the other controls the graphs
    'Enable/Disable the panels based on the Map or Plots tab selection
    'Added an option to plot relative to Ecopath base (how it works now) or relative to the end of the last timestep (now biomass plot works now) 
    'Relative plotting option will need to be integrated with the Spin-Up period some how
    'This requires that the core pass out different base line values and the plots decide which type to use

    ''' <summary>
    ''' Form, implementing the Ecospace Run interface.
    ''' </summary>
    Public Class frmRunEcospace

        ''' <summary>number of legend bins is arbitrary</summary>
        Private Const cColourBins As Integer = 200

        Private m_FishingMortMax As Single = 2.0

        Public Enum eShowItemType
            ShowAll = 0
            ShowCustom
            ShowSingle
        End Enum

        Public Enum ePlotTypes As Integer
            RelB
            FOverB
            F
            Contaminant
            CoverB
            Effort
            FishingMortGraph
            CatchGraph
            Discards
            PredMortRateGraph
            ConsumpRateGraph
            Driver
            ComputedCapacity
        End Enum

#Region " Variables "

        Private m_bInUpdate As Boolean = False

        ''' <summary>The previous number of timesteps UI has drawn.</summary>
        Private m_iTimeStepPrev As Integer
        ''' <summary>The current number of timesteps available to draw.</summary>
        Private m_iTimeStepCur As Integer

        ' === Timestep and derived values ===
        Private m_dataTimeStep As cEcospaceTimestep = Nothing
        ''' <summary>The array to hold the Ecospace average biomass results.</summary>
        Private m_RelBiomassResults(,) As Single
        ''' <summary>The array to hold the Ecospace base biomass results.</summary>
        Private m_BaseBiomassResults() As Single
        ''' <summary>Contaminants over Biomass.</summary>
        Private m_ConcOverB(,,) As Single
        ''' <summary>Effort over Biomass.</summary>
        Private m_FoverB(,,) As Single

        Private m_graphData As Dictionary(Of ePlotTypes, Single(,))

        ' -- bits to remember, ugh --

        Private m_BaseCatch() As Single
        Private m_BaseBiomass() As Single
        Private m_FishingMortScaler() As Single
        Private m_CBScaler() As Single
        Private m_BaseC() As Single
        Private m_baseDiscards() As Single

        Private m_layerDepth As cEcospaceLayer = Nothing

        ''' <summary>The row and col number of map plots.</summary>
        Private m_iNumPlotsVert As Integer, m_iNumPlotsHorz As Integer
        ''' <summary>Number of rows and columns if basen</summary>
        ''' <remarks>???</remarks>
        Private m_iInRow As Integer, m_iInCol As Integer

        Private m_drawers As List(Of cMapDrawerBase)
        Private m_nMapsPerThread As Integer

        Private m_bmpMap As Bitmap = Nothing

        'jb added
        Private m_spaceStats As cEcospaceStats = Nothing

        ' -- map plot settings --
        Private m_mapPlotType As ePlotTypes = ePlotTypes.RelB
        ''' <summary>Tracker to detect plot type changes. Initial value indicates fleet stuff, not groups</summary>
        Private m_mapPlotTypeLast As ePlotTypes = ePlotTypes.Effort
        Private m_bOverlay As Boolean = False
        Private m_bShowIBM As Boolean = True
        Private m_bpConTracing As cBooleanProperty = Nothing
        Private m_showitemMode As eShowItemType = eShowItemType.ShowAll
        Private m_iItemToShow As Integer = 1

        ' -- graph plot settings --
        Private m_graphPlotType As ePlotTypes = ePlotTypes.RelB
        Private m_zgh As cEcospaceZedGraphHelper = Nothing

        ''' <summary>Exposing m_sMaxEffort to the interface would allow the user to set the Effort legend sensitivity.</summary>
        Private m_sMaxEffort As Single = 5
        Private m_cmdDisplayGroups As cCommand = Nothing

        ' Properties to monitor for setting run mode states
        Private WithEvents m_bpUseIBM As cBooleanProperty = Nothing
        Private WithEvents m_bpUseNewStanza As cBooleanProperty = Nothing

        ' -- Hoover menu --
        Private m_hoverMenu As ucHoverMenu = Nothing

        ' -- Autosave images --
        Private m_iAutosaveTS As Integer()

        Protected Enum eHoverCommands As Integer
            SaveImage
            SaveImageGeoRef
        End Enum

#End Region ' Variables

#Region " Construction and Destruction "

        Public Sub New()
            MyBase.New()
            ' To prevent premature control events from tryingto update Styleguide etc. Nasty
            Me.m_bInUpdate = True
            Me.InitializeComponent()
        End Sub

#End Region ' Construction and Destruction

#Region " Initialization and Updating "

        Private Sub InitCoreParams()

            'Get the basemap
            Me.m_layerDepth = Me.Core.EcospaceBasemap.LayerDepth

            'Redim relative biomass results array
            ReDim Me.m_RelBiomassResults(Me.Core.nGroups, Me.Core.nEcospaceTimeSteps)

            Me.m_graphData = New Dictionary(Of ePlotTypes, Single(,))

            Me.m_graphData.Add(ePlotTypes.RelB, New Single(Me.Core.nGroups, Me.Core.nEcospaceTimeSteps) {})
            Me.m_graphData.Add(ePlotTypes.FishingMortGraph, New Single(Me.Core.nGroups, Me.Core.nEcospaceTimeSteps) {})
            Me.m_graphData.Add(ePlotTypes.PredMortRateGraph, New Single(Me.Core.nGroups, Me.Core.nEcospaceTimeSteps) {})
            Me.m_graphData.Add(ePlotTypes.ConsumpRateGraph, New Single(Me.Core.nGroups, Me.Core.nEcospaceTimeSteps) {})
            Me.m_graphData.Add(ePlotTypes.CatchGraph, New Single(Me.Core.nGroups, Me.Core.nEcospaceTimeSteps) {})

            'Redim base biomass base result array
            ReDim Me.m_BaseBiomassResults(Me.Core.nGroups)

            'get the ecospace stats object from the core
            Me.m_spaceStats = Me.Core.EcospaceStats

        End Sub

        Private Sub InitUIParams()

            Me.m_iTimeStepCur = 0
            Me.m_iTimeStepPrev = 0

            Me.CheckRefreshSingleItemDropdown()

        End Sub

        ''' <summary>
        ''' Initialization of BioMapPlot
        ''' </summary>
        Private Sub InitMapPlot()

            'Hack warning: For initialization the map dimensions are set to the value supplied by the core base map.
            'The actual size of the map must be set from the EcoSpace Timestep results(See EcospaceTimeStepDelegate())
            'This should not be called once Ecospace has been run because the map dims can be out of sync!

            Me.m_iInCol = Me.Core.EcospaceBasemap.InCol
            Me.m_iInRow = Me.Core.EcospaceBasemap.InRow
            'Core.nGroups --> updated to nLivingGroups? Non - hidden groups? Check EwE5

            Me.CalcMapDimension(Me.Core.nGroups, Me.m_iNumPlotsVert, Me.m_iNumPlotsHorz)
            Me.CalcMapDimension(Me.Core.nFleets, Me.m_iNumPlotsVert, Me.m_iNumPlotsHorz)

        End Sub

        Private Sub CalcMapDimension(ByVal iTotal As Integer, ByRef iNumPlotsVert As Integer, ByRef iNumPlotsHorz As Integer)
            iNumPlotsHorz = CInt(Math.Ceiling(Math.Sqrt(iTotal) * Me.m_iInRow / Me.m_iInCol * Me.m_pbMap.Width / Me.m_pbMap.Height))
            If iNumPlotsHorz = 0 Then
                iNumPlotsVert = iTotal
            Else
                iNumPlotsVert = CInt(Math.Ceiling(iTotal / iNumPlotsHorz))
            End If
        End Sub

        Private Sub InitDrawingThreads()

            Dim drawer As cMapDrawerBase
            Dim nThreads As Integer = Environment.ProcessorCount
            Dim sg As cStyleGuide = Me.StyleGuide
            Dim nItems As Integer = -1

            Select Case Me.m_mapPlotType
                Case ePlotTypes.Effort : Return
                Case ePlotTypes.Driver
                    nItems = Me.Core.nEnvironmentalDriverLayers
                Case Else
                    nItems = Me.Core.nGroups
            End Select

            Me.m_nMapsPerThread = (nItems + nThreads - 1) \ nThreads
            If Me.m_drawers Is Nothing Then
                Me.m_drawers = New List(Of cMapDrawerBase)
            Else
                Me.m_drawers.Clear()
            End If

            Me.InitOutputBitmaps()

            For i As Integer = 1 To nThreads
                Select Case Me.m_mapPlotType
                    Case ePlotTypes.Driver
                        drawer = New cMapDrawerLayer(Me.Core, Me.StyleGuide, eVarNameFlags.LayerDriver)
                    Case ePlotTypes.ComputedCapacity
                        drawer = New cMapDrawerLayer(Me.Core, Me.StyleGuide, eVarNameFlags.LayerHabitatCapacity)
                    Case Else
                        drawer = New cMapDrawerGroup(Me.Core, Me.StyleGuide)
                End Select
                drawer.Graphics = Graphics.FromImage(Me.m_bmpMap)
                drawer.Colors = Me.m_legend.Colors
                drawer.ShowExcluded = Me.StyleGuide.ShowMapsExcludedCells
                Me.m_drawers.Add(drawer)
            Next

        End Sub

        Private Sub InitOutputBitmaps()
            Me.m_bmpMap = New Bitmap(Me.m_pbMap.Width, Me.m_pbMap.Height)
            For Each drawer As cMapDrawerBase In Me.m_drawers
                drawer.Graphics = Graphics.FromImage(m_bmpMap)
            Next
        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
            If ((changeType And cStyleGuide.eChangeType.Colours) = cStyleGuide.eChangeType.Colours) Then
                Me.UpdateStyleColors()
            End If
            If ((changeType And cStyleGuide.eChangeType.Map) = cStyleGuide.eChangeType.Map) Then
                For Each d As cMapDrawerBase In Me.m_drawers
                    d.ShowExcluded = Me.StyleGuide.ShowMapsExcludedCells
                Next
                Me.RefreshPlot()
            End If
            If ((changeType And cStyleGuide.eChangeType.GroupVisibility) = cStyleGuide.eChangeType.GroupVisibility) Then
                Me.RefreshPlot()
            End If
        End Sub

        Private Sub UpdateStyleColors()
            Me.m_pbMap.BackColor = Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND)
        End Sub

#End Region ' Initialization and Updating

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_legend.UIContext = Me.UIContext
            Me.m_legend.Colors = Me.StyleGuide.GetEwE5ColorRamp(cColourBins)

            Dim pm As cPropertyManager = Me.PropertyManager
            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters()

            ' Start listening to props
            Me.m_bpConTracing = DirectCast(pm.GetProperty(parms, eVarNameFlags.ConSimOnEcoSpace), cBooleanProperty)
            Me.m_bpUseIBM = DirectCast(pm.GetProperty(parms, eVarNameFlags.UseIBM), cBooleanProperty)
            Me.m_bpUseNewStanza = DirectCast(pm.GetProperty(parms, eVarNameFlags.UseNewMultiStanza), cBooleanProperty)

            ' Initially collapse some panels to save screen estate
            Me.m_hdrLabelOptions.IsCollapsed = True
            Me.m_hdrAutosave.IsCollapsed = Not Me.m_cbAutoSavePNG.Checked

            Me.InitCoreParams()
            Me.InitUIParams()
            Me.InitMapPlot()
            Me.InitDrawingThreads()

            Me.m_zgh = New cEcospaceZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_zgPlotLarge)
            Me.m_zgh.ShowPointValue = True

            Me.m_cmdDisplayGroups = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If (Me.m_cmdDisplayGroups IsNot Nothing) Then
                Me.m_cmdDisplayGroups.AddControl(Me.m_btnDisplayGroups1)
                AddHandler Me.m_cmdDisplayGroups.OnPostInvoke, AddressOf OnDisplayGroupsInvoked
            End If

            Me.ShowItemMode = eShowItemType.ShowAll

            Dim nGrps As Integer = Me.Core.nGroups
            ReDim Me.m_BaseC(nGrps)
            ReDim Me.m_BaseCatch(nGrps)
            ReDim Me.m_FishingMortScaler(nGrps)
            ReDim Me.m_BaseBiomass(nGrps)
            ReDim Me.m_baseDiscards(nGrps)

            For igrp As Integer = 1 To nGrps
                Me.m_FishingMortScaler(igrp) = 1
                Me.m_BaseC(igrp) = 1
                Me.m_BaseBiomass(igrp) = Me.Core.StartBiomass(igrp)
                For iflt As Integer = 1 To Me.Core.nFleets
                    Me.m_BaseCatch(igrp) += Me.Core.EcopathFleetInputs(iflt).Landings(igrp) + Me.Core.EcopathFleetInputs(iflt).Discards(igrp)
                    Me.m_baseDiscards(igrp) += Me.Core.EcopathFleetInputs(iflt).Discards(igrp)
                Next
            Next

            'Scaler for the fishing mort map legend
            Me.m_txFMax.Text = Me.m_FishingMortMax.ToString

            ' Connect hover menu
            Me.m_hoverMenu = New ucHoverMenu(Me.UIContext)
            Me.m_hoverMenu.Attach(Me.m_pbMap)
            Me.m_hoverMenu.AddItem(SharedResources.InsertPictureHS, SharedResources.TOOLTIP_SAVETOIMAGE, eHoverCommands.SaveImage)
            Me.m_hoverMenu.AddItem(SharedResources.map, SharedResources.TOOLTIP_SAVETOMAP, eHoverCommands.SaveImageGeoRef)
            AddHandler Me.m_hoverMenu.OnUserCommand, AddressOf OnHoverMenuCommand

            Me.m_bInUpdate = False

            'Start tracking ConcTracing setting
            AddHandler Me.m_bpConTracing.PropertyChanged, AddressOf OnPropertyChanged
            ' Start tracking core state monitor for Ecospace run states
            AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged

            Me.IsRunning = Me.Core.StateMonitor.IsEcospaceRunning

            Me.ClearResults()
            Me.UpdateStyleColors()
            Me.UpdateControls()

            Me.m_rbRelBiomassGraph.Tag = ePlotTypes.RelB
            Me.m_rbPredMortGraph.Tag = ePlotTypes.PredMortRateGraph
            Me.m_rbFishMortGraph.Tag = ePlotTypes.FishingMortGraph
            Me.m_rbConsumpGraph.Tag = ePlotTypes.ConsumpRateGraph
            Me.m_rbCatchGraph.Tag = ePlotTypes.CatchGraph

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace}

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            Me.m_bpUseIBM = Nothing
            Me.m_bpUseNewStanza = Nothing

            Try

                If (Me.m_cmdDisplayGroups IsNot Nothing) Then
                    Me.m_cmdDisplayGroups.RemoveControl(Me.m_btnDisplayGroups1)
                    RemoveHandler Me.m_cmdDisplayGroups.OnPostInvoke, AddressOf OnDisplayGroupsInvoked
                    Me.m_cmdDisplayGroups = Nothing
                End If

                RemoveHandler Me.m_hoverMenu.OnUserCommand, AddressOf OnHoverMenuCommand
                Me.m_hoverMenu.Detach()

                Me.Core.StopEcospace()
                Me.m_drawers.Clear()

                Me.m_zgh.Detach()
                Me.m_zgh = Nothing

                ' Stop tracking core state monitor for Ecospace run states
                RemoveHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged
                ' Stop tracking ConcTracing setting
                RemoveHandler Me.m_bpConTracing.PropertyChanged, AddressOf OnPropertyChanged
                Me.m_bpConTracing = Nothing

            Catch ex As Exception
                'make sure something in the interface does not stop the base from cleaning up
                Debug.Assert(False, Me.ToString & ".OnFormClosed() Exception: " & ex.Message)
            End Try

            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub OnResizeEnd(ByVal e As EventArgs)
            Me.InitOutputBitmaps()
        End Sub

        Public Overrides ReadOnly Property IsRunForm() As Boolean
            Get
                Return True
            End Get
        End Property

        Private Sub OnMapMouseDouble(ByVal sender As Object, ByVal e As EventArgs) _
            Handles m_pbMap.DoubleClick
            Me.OnHoverMenuCommand(eHoverCommands.SaveImage)
        End Sub

        Private Sub OnMapMouseClick(ByVal sender As Object, ByVal e As MouseEventArgs) _
            Handles m_pbMap.MouseClick
            If e.Button = System.Windows.Forms.MouseButtons.Right Then
                Me.OnHoverMenuCommand(eHoverCommands.SaveImage)
            End If
        End Sub

        Private Sub OnPaintMap(ByVal sender As Object, ByVal e As PaintEventArgs) _
            Handles m_pbMap.Paint
            Me.PlotMap(e.Graphics)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Display groups command has been invoked: entirely invalidate the map plot.
        ''' This is rather hack but necessary, since this form is entirely responsible
        ''' for rendering the map picture box.
        ''' </summary>
        ''' <param name="cmd"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnDisplayGroupsInvoked(ByVal cmd As cCommand)
            Me.m_showitemMode = eShowItemType.ShowCustom
            Me.UpdateControls()
            Me.RefreshMap()
        End Sub

#End Region ' Events 

#Region " Biomass graph "

        Private Sub AppendPlotData()
            Try
                'get the data from the dictionary of graph plot types
                Dim data(,) As Single = Me.m_graphData.Item(Me.m_graphPlotType)
                For iGroup As Integer = 1 To Core.nGroups
                    For iTimeStep As Integer = Me.m_iTimeStepPrev To Me.m_iTimeStepCur - 1
                        Me.m_zgh.AddValue(iGroup, iTimeStep, data(iGroup, iTimeStep + 1))
                    Next
                Next
                Me.m_zgh.RescaleAndRedraw()
            Catch ex As Exception
                cLog.Write(ex, Me.ToString + ".UpdateBiomassPlot()")
            End Try
        End Sub

#End Region ' Biomass graph

#Region " Map plot "

        Private Sub PlotMap(ByVal g As Graphics)
            Try
                Select Case m_mapPlotType
                    Case ePlotTypes.Effort
                        PlotFleetMap(g)
                    Case ePlotTypes.Driver
                        PlotLayerMap(g, eVarNameFlags.LayerDriver)
                    Case ePlotTypes.ComputedCapacity
                        PlotLayerMap(g, eVarNameFlags.LayerHabitatCapacity)
                    Case Else
                        PlotGroupMap(g)
                End Select
            Catch ex As Exception
                ' Whoah!
            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Plot group-related data via multiple threads.
        ''' </summary>
        ''' <param name="g"></param>
        ''' -------------------------------------------------------------------
        Private Sub PlotGroupMap(ByVal g As Graphics)

            ' Sanity check
            If (Me.m_dataTimeStep Is Nothing) Then Return

            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters
            Dim dtTime As Date = Me.Core.EcospaceTimestepToAbsoluteTime(Me.m_iTimeStepCur)
            Dim iYear As Integer = dtTime.Year
            Dim iMonth As Integer = dtTime.Month
            Dim drawer As cMapDrawerBase = Nothing
            Dim iNumVisItems As Integer = 0
            Dim lVisItems As New List(Of cCoreGroupBase)
            Dim bShowItem As Boolean = False

            For iGroup As Integer = 1 To Me.Core.nGroups

                Select Case Me.ShowItemMode
                    Case eShowItemType.ShowAll
                        bShowItem = True
                    Case eShowItemType.ShowSingle
                        bShowItem = (iGroup = Me.ItemToShow)
                    Case eShowItemType.ShowCustom
                        bShowItem = Me.StyleGuide.GroupVisible(iGroup)
                End Select

                If bShowItem Then
                    lVisItems.Add(Me.Core.EcoPathGroupInputs(iGroup))
                    iNumVisItems += 1
                End If
            Next

            ' JS05Mar10: disabled console output to keep moving fast
            'Console.WriteLine("Step {0} = year {1}, month {2} at {3}", Me.m_iTimeStepCur, iYear, iMonth, Me.Core.EcospaceModelParameters.NumberOfTimeStepsPerYear)
            Dim originList As New List(Of PointF)
            Dim rectList As New List(Of Rectangle)

            cMapDrawerBase.CalcMapAreas(Me.m_pbMap.ClientRectangle, iNumVisItems, Me.m_iInRow, Me.m_iInCol,
                                        Me.m_iNumPlotsHorz, Me.m_iNumPlotsVert, originList, rectList)

            ' Clear background
            Me.InitOutputBitmaps()

            Try

                Dim maptype As cMapDrawerBase.eMapType
                Dim RelScaler() As Single = Nothing
                Dim ifirst As Integer = 0
                Dim ilast As Integer = 0
                Dim strDate As String = dtTime.ToShortDateString()

                For Each drawer In m_drawers

                    If drawer.AllowedToRun Then

                        'init the drawer to the latest values
                        drawer.OriginList = originList
                        drawer.RectList = rectList
                        drawer.Date = strDate

                        drawer.StanzaDS = Nothing

                        Select Case Me.m_mapPlotType

                            Case ePlotTypes.RelB
                                drawer.Map = Me.m_dataTimeStep.BiomassMap
                                maptype = cMapDrawerBase.eMapType.RelBiomass
                                RelScaler = Me.m_BaseBiomass

                                If parms.UseIBM And Me.m_bShowIBM Then
                                    drawer.StanzaDS = Me.m_dataTimeStep.StanzaDS
                                End If

                            Case ePlotTypes.FOverB
                                drawer.Map = Me.m_FoverB
                                maptype = cMapDrawerBase.eMapType.FishingMortRate
                                RelScaler = Me.m_FishingMortScaler

                            Case ePlotTypes.F
                                drawer.Map = Me.m_dataTimeStep.CatchMap
                                maptype = cMapDrawerBase.eMapType.RelCatch
                                RelScaler = Me.m_BaseCatch

                            Case ePlotTypes.Contaminant
                                drawer.Map = Me.m_dataTimeStep.ContaminantMap
                                maptype = cMapDrawerBase.eMapType.RelContam
                                RelScaler = Me.m_BaseC

                            Case ePlotTypes.CoverB
                                drawer.Map = Me.m_ConcOverB
                                maptype = cMapDrawerBase.eMapType.ContamRate
                                RelScaler = Me.m_BaseC

                            Case ePlotTypes.Discards
                                drawer.Map = Me.m_dataTimeStep.DiscardMortalityMap
                                maptype = cMapDrawerBase.eMapType.Discards
                                RelScaler = Me.m_baseDiscards

                            Case ePlotTypes.Effort
                                ' This type of map cannot be drawn threaded because cMapDrawers are hard-wired
                                ' to render groups. Ugh.

                            Case ePlotTypes.Driver
                                drawer.Map = Nothing ' The drawer will pick up what it needs
                                RelScaler = Nothing

                            Case ePlotTypes.ComputedCapacity
                                Debug.Assert(False)

                        End Select

                        Dim mapArgs As New cMapDrawerArgs(maptype, RelScaler, Me.m_FishingMortMax)

                        drawer.InCol = Me.m_iInCol
                        drawer.InRow = Me.m_iInRow
                        drawer.Month = iMonth

                        ilast = Math.Min(ifirst + Me.m_nMapsPerThread - 1, iNumVisItems - 1)

                        drawer.ClearItems()
                        For i As Integer = ifirst To ilast
                            drawer.AddItem(lVisItems(i), i)
                        Next

                        drawer.SignalState.Reset()

                        drawer.AllowedToRun = False
                        ThreadPool.QueueUserWorkItem(AddressOf drawer.Draw, mapArgs)

                        ifirst += m_nMapsPerThread
                    End If
                Next

                For Each drawer In m_drawers
                    drawer.SignalState.WaitOne()
                Next

                g.DrawImage(m_bmpMap, 0, 0)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Plot group-related data via multiple threads.
        ''' </summary>
        ''' <param name="g"></param>
        ''' -------------------------------------------------------------------
        Private Sub PlotLayerMap(ByVal g As Graphics, varname As eVarNameFlags)

            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters
            Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
            Dim dtTime As Date = Me.Core.EcospaceTimestepToAbsoluteTime(Me.m_iTimeStepCur)
            Dim iYear As Integer = dtTime.Year
            Dim iMonth As Integer = dtTime.Month
            Dim iNumVisItems As Integer = 0
            Dim lVisItems As New List(Of cEcospaceLayer)
            Dim bShowItem As Boolean = False
            Dim layers As cEcospaceLayer() = bm.Layers(varname)

            For iLayer As Integer = 1 To layers.Count

                Select Case Me.ShowItemMode
                    Case eShowItemType.ShowAll
                        bShowItem = True
                    Case eShowItemType.ShowSingle
                        bShowItem = (iLayer = Me.ItemToShow)
                        'Case eShowItemType.ShowNonHidden
                        '    bShowItem = Me.StyleGuide.GroupVisible(iGroup)
                End Select

                If bShowItem Then
                    lVisItems.Add(layers(iLayer - 1))
                    iNumVisItems += 1
                End If
            Next

            ' JS05Mar10: disabled console output to keep moving fast
            'Console.WriteLine("Step {0} = year {1}, month {2} at {3}", Me.m_iTimeStepCur, iYear, iMonth, Me.Core.EcospaceModelParameters.NumberOfTimeStepsPerYear)
            Dim originList As New List(Of PointF)
            Dim rectList As New List(Of Rectangle)

            cMapDrawerBase.CalcMapAreas(Me.m_pbMap.ClientRectangle, iNumVisItems, Me.m_iInRow, Me.m_iInCol,
                                        Me.m_iNumPlotsHorz, Me.m_iNumPlotsVert, originList, rectList)

            ' Clear background
            Me.InitOutputBitmaps()

            Try

                Dim iFrom As Integer = 0
                Dim iTo As Integer = 0
                Dim strDate As String = dtTime.ToShortDateString()

                For Each drawer As cMapDrawerBase In m_drawers

                    If drawer.AllowedToRun Then

                        'init the drawer to the latest values
                        drawer.OriginList = originList
                        drawer.RectList = rectList
                        drawer.Date = strDate

                        drawer.StanzaDS = Nothing

                        drawer.InCol = Me.m_iInCol
                        drawer.InRow = Me.m_iInRow
                        drawer.Month = iMonth
                        DirectCast(drawer, cMapDrawerLayer).Map = varname

                        iTo = Math.Min(iFrom + Me.m_nMapsPerThread - 1, iNumVisItems - 1)

                        drawer.ClearItems()
                        For i As Integer = iFrom To iTo
                            drawer.AddItem(lVisItems(i), i)
                        Next

                        drawer.SignalState.Reset()

                        drawer.AllowedToRun = False
                        ThreadPool.QueueUserWorkItem(AddressOf drawer.Draw, Nothing)

                        iFrom += m_nMapsPerThread
                    End If
                Next

                For Each drawer As cMapDrawerBase In m_drawers
                    drawer.SignalState.WaitOne()
                Next

                g.DrawImage(m_bmpMap, 0, 0)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Private Sub SetFleetsForSelGroups()

            'Turn all the fleets off
            For iflt As Integer = 1 To Me.Core.nFleets
                Me.StyleGuide.FleetVisible(iflt) = False
            Next

            'Now enable just the ones for the selected group
            For igrp As Integer = 1 To Me.Core.nGroups
                If Me.StyleGuide.GroupVisible(igrp) Then
                    For iflt As Integer = 1 To Me.Core.nFleets
                        Dim flt As cEcopathFleetInput = Me.Core.EcopathFleetInputs(iflt)
                        If (flt.Landings(igrp) + flt.Discards(igrp) > 0) Then
                            Me.StyleGuide.FleetVisible(iflt) = True
                        End If
                    Next
                End If
            Next

        End Sub

        Private Sub PlotFleetMap(ByVal g As Graphics)

            Dim iNumVizFleets As Integer = 0
            Dim lVizFleets As New List(Of Integer)
            Dim lOrigins As New List(Of PointF)
            Dim lMaps As New List(Of Rectangle)

            Debug.Assert(Me.m_mapPlotType = ePlotTypes.Effort, "Only allowed for effort maps due to limitations in cMapDrawer. Ugh!")

            If m_iTimeStepCur > 0 Then

                If Me.m_showitemMode = eShowItemType.ShowSingle Then
                    lVizFleets.Add(Me.m_cmbDisplayItem.SelectedIndex() + 1)
                Else
                    For iFleet As Integer = 1 To Me.Core.nFleets
                        If Me.StyleGuide.FleetVisible(iFleet) Then
                            lVizFleets.Add(iFleet)
                        End If
                    Next
                End If
                iNumVizFleets = lVizFleets.Count

                If iNumVizFleets = 0 Then Return

                cMapDrawerBase.CalcMapAreas(Me.m_pbMap.ClientRectangle, iNumVizFleets, Me.m_iInRow, Me.m_iInCol,
                                            Me.m_iNumPlotsHorz, Me.m_iNumPlotsVert, lOrigins, lMaps)

                For i As Integer = 0 To iNumVizFleets - 1
                    Try
                        DrawFishingBaseMap(Me.m_dataTimeStep.FishingEffortMap, lVizFleets(i), lMaps(i), g)
                    Catch ex As Exception

                    End Try
                Next

            End If

        End Sub

        Private Sub DrawFishingBaseMap(ByVal mapFishing(,,) As Single,
                                       iFleet As Integer, ByVal rcPos As Rectangle, ByVal g As Graphics)

            Dim sg As cStyleGuide = Me.StyleGuide
            Dim lColors As List(Of Color) = sg.GetEwE5ColorRamp(cColourBins)
            Dim cScaler As Single = cColourBins / 2 'Me.m_sMaxEffort
            Dim brCell As Brush = Nothing
            Dim sTSpy As Single = Me.Core.EcospaceModelParameters.NumberOfTimeStepsPerYear
            Dim iYear As Integer = CInt(Math.Floor(Me.m_iTimeStepCur / sTSpy))
            Dim iMonth As Integer = CInt(cCore.N_MONTHS / sTSpy * (Me.m_iTimeStepCur - (iYear * sTSpy)))
            Dim depth As cEcospaceLayerDepth = Me.Core.EcospaceBasemap.LayerDepth
            Dim excl As cEcospaceLayerExclusion = Me.Core.EcospaceBasemap.LayerExclusion
            Dim brExcluded As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.Red, Color.FromArgb(&H88FF4500))
            Dim dtTime As Date = Me.Core.EcospaceTimestepToAbsoluteTime(Me.m_iTimeStepCur)
            Dim strDate As String = dtTime.ToShortDateString()

            Using br As New SolidBrush(sg.ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND))
                g.FillRectangle(br, rcPos)
            End Using

            For i As Integer = 1 To m_iInRow
                For j As Integer = 1 To m_iInCol

                    Dim tmpRect As RectangleF = New RectangleF(CSng(rcPos.Left + (j - 1) * rcPos.Width() / m_iInCol),
                        CSng(rcPos.Top + (i - 1) * rcPos.Height() / m_iInRow),
                        CSng(rcPos.Width() / m_iInCol),
                        CSng(rcPos.Height() / m_iInRow))
                    Dim tmpBrush As SolidBrush = Nothing

                    If (depth.IsWaterCell(i, j) = True) Then
                        If (Not excl.IsExcludedCell(i, j)) Then

                            'Effort for a single fleet
                            Dim icc As Single = mapFishing(iFleet, i, j) * cScaler

                            'Convert to effort per unit of area
                            'icc = baseMap(iFleet, i, j) * cScaler / cEcospaceDataStructures.Width(i)

                            'Boundary check
                            icc = Math.Max(Math.Min(cColourBins, icc), 0)
                            If (Not Single.IsNaN(icc)) Then
                                tmpBrush = New SolidBrush(lColors(CInt(icc)))
                                g.FillRectangle(tmpBrush, tmpRect)
                                tmpBrush.Dispose()
                            End If

                            ' Draw MPA
                            If Me.StyleGuide.ShowMapsMPAs Then
                                For k As Integer = 1 To Me.Core.nMPAs - 1
                                    Dim iMPA As Integer = CInt(Me.Core.EcospaceBasemap.LayerMPA(k).Cell(i, j))
                                    If iMPA > 0 Then
                                        If Me.Core.EcospaceMPAs(iMPA).MPAMonth(iMonth) Then
                                            brCell = New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.LightGray, Color.Transparent)
                                        Else
                                            brCell = New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.Black, Color.Transparent)
                                        End If
                                        g.FillRectangle(brCell, tmpRect)
                                        brCell.Dispose()
                                    End If
                                Next
                            End If
                        End If
                    Else
                        tmpBrush = New SolidBrush(Color.Gray)
                        g.FillRectangle(tmpBrush, tmpRect)
                        tmpBrush.Dispose()
                    End If

                    If Me.StyleGuide.ShowMapsExcludedCells And (excl.IsExcludedCell(i, j)) Then
                        g.FillRectangle(brExcluded, tmpRect)
                    End If
                Next
            Next

            'Draw the black frame of base map
            g.DrawRectangle(Pens.Black, rcPos)

            ' Probably don't have to do this as these brushes are only create once and will be disposed when they go out of scope
            ' JS: No, must dispose GDI objects because they are not garbage collected in .NET
            brExcluded.Dispose()
            brExcluded = Nothing

            'Display the group name
            If Me.StyleGuide.ShowMapLabels Then

                Dim strLabel As String = ""
                If Me.StyleGuide.ShowMapsDateInLabels Then
                    strLabel = String.Format(SharedResources.GENERIC_LABEL_DOUBLE, Core.EcospaceFleetInputs(iFleet).Name, strDate)
                Else
                    strLabel = Core.EcospaceFleetInputs(iFleet).Name
                End If

                Dim br As Brush = Brushes.Black
                Dim fmt As New StringFormat()

                fmt.Alignment = Me.StyleGuide.MapLabelPosHorizontal
                fmt.LineAlignment = Me.StyleGuide.MapLabelPosVertical

                If Me.StyleGuide.InvertMapLabelColor Then br = Brushes.White

                g.DrawString(strLabel, Me.StyleGuide.Font(cStyleGuide.eApplicationFontType.Legend), br, rcPos, fmt)
            End If

        End Sub

#End Region ' Map plot

#Region " Events "

        Private Sub OnOutputTabSelected(sender As Object, e As System.EventArgs) _
            Handles m_tcOutputs.SelectedIndexChanged

            Me.UpdateControls()

        End Sub

        Private Sub OnRun(ByVal sender As Object, ByVal e As EventArgs) Handles m_btnRun.Click

            Me.ClearResults()

            '22-Aug-2013 Changed Me.IsRunning to be set by Core.RunEcoSpace()
            'so if the run fails Me.IsRunning will be set to False and the interface will update correctly
            'Me.IsRunning = True
            Me.m_iTimeStepCur = 0
            Me.Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.Core.StopEcospace))
            Me.IsRunning = Me.Core.RunEcoSpace(AddressOf OnEcospaceTimeStep)

            ' Hack: make a once-per-run assessment for which time steps to save images

            ' Save map image
            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters()
            If (Me.m_cbAutoSavePNG.Checked) Then
                Me.m_iAutosaveTS = cStringUtils.Range(Me.m_tbxAutosaveTimeSteps.Text, Me.Core.nEcospaceTimeSteps)
            Else
                Me.m_iAutosaveTS = Nothing
            End If

        End Sub

        Private Sub OnStop(ByVal sender As Object, ByVal e As EventArgs) _
            Handles m_btnStop.Click

            Me.Core.StopEcospace()
            Me.Core.SetStopRunDelegate(Nothing)

            ' Controls wil be updated via Core state monitor events
            'Me.UpdateControls()
        End Sub

        Private Sub OnSelectDataChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_rbDisplayRelBiomass.CheckedChanged,
                    m_rbDisplayFishingEffort.CheckedChanged,
                    m_rbDisplayCoverB.CheckedChanged,
                    m_rbDisplayContaminantC.CheckedChanged,
                    m_rbDisplayF.CheckedChanged, m_rbDisplayFOverB.CheckedChanged, m_rbDisplayEnvDriver.CheckedChanged,
                    m_rbDisplayDiscards.CheckedChanged, m_rbDisplayComputedHabitatCapacity.CheckedChanged,
                    m_rbDisplayComputedHabitatCapacity.CheckedChanged

            ' To catch premature events
            If (Me.UIContext Is Nothing) Then Return

            If Me.m_rbDisplayRelBiomass.Checked Then
                Me.m_mapPlotType = ePlotTypes.RelB
            ElseIf Me.m_rbDisplayFishingEffort.Checked Then
                Me.m_mapPlotType = ePlotTypes.Effort
                Me.SetFleetsForSelGroups()
            ElseIf Me.m_rbDisplayCoverB.Checked Then
                Me.m_mapPlotType = ePlotTypes.CoverB
            ElseIf Me.m_rbDisplayContaminantC.Checked Then
                Me.m_mapPlotType = ePlotTypes.Contaminant
            ElseIf Me.m_rbDisplayF.Checked Then
                Me.m_mapPlotType = ePlotTypes.F
            ElseIf Me.m_rbDisplayFOverB.Checked Then
                Me.m_mapPlotType = ePlotTypes.FOverB
            ElseIf Me.m_rbDisplayEnvDriver.Checked Then
                Me.m_mapPlotType = ePlotTypes.Driver
            ElseIf m_rbDisplayDiscards.Checked Then
                Me.m_mapPlotType = ePlotTypes.Discards
            ElseIf Me.m_rbDisplayComputedHabitatCapacity.Checked Then
                Me.m_mapPlotType = ePlotTypes.ComputedCapacity
            End If

            Me.InitDrawingThreads()
            Me.CheckRefreshSingleItemDropdown()
            Me.UpdateControls()
            Me.RefreshPlot()
            Me.RefreshMap()

        End Sub

        Private Sub OnOverlay(ByVal sender As Object, ByVal e As EventArgs) _
            Handles m_cbOverlay.Click
            Me.m_bOverlay = m_cbOverlay.Checked
        End Sub

        Private Sub OnSelectItemToDisplay(ByVal sender As Object, ByVal e As EventArgs) _
            Handles m_cmbDisplayItem.SelectedIndexChanged

            If (Me.m_bInUpdate) Then Return

            Try
                Me.ItemToShow = (Me.m_cmbDisplayItem.SelectedIndex + 1)
            Catch ex As Exception

            End Try
        End Sub

        'Private Sub OnSelectDriverToShow(ByVal sender As System.Object, ByVal e As System.EventArgs)

        '    Dim iSel As Integer = Math.Max(Math.Min(9, Me.m_cmbLabelPos.SelectedIndex), 0)
        '    Me.m_labelposHorz = DirectCast(CInt(iSel Mod 3), StringAlignment)
        '    Me.m_labelposVert = DirectCast(CInt(Math.Floor(iSel / 3)), StringAlignment)
        '    Me.RefreshMap()

        'End Sub

        Private Sub OnAutosaveTimeStepsChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_cbAutoSavePNG.CheckedChanged
            Try
                Me.UpdateControls()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnDisplayOptionCheckChanged(ByVal sender As Object, ByVal e As EventArgs) _
                Handles m_rbShowAll.CheckedChanged, m_rbShowNonHidden.CheckedChanged, m_rbShowSingle.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            If Me.m_rbShowAll.Checked Then Me.ShowItemMode = eShowItemType.ShowAll
            If Me.m_rbShowNonHidden.Checked Then Me.ShowItemMode = eShowItemType.ShowCustom
            If Me.m_rbShowSingle.Checked Then Me.ShowItemMode = eShowItemType.ShowSingle
            Me.UpdateControls()

            Me.RefreshPlot()
            Me.RefreshMap()

        End Sub

        Private Sub m_cbMPA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbMPA.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            Me.StyleGuide.ShowMapsMPAs = m_cbMPA.Checked
            Me.UpdateControls()
            Me.RefreshMap()

        End Sub

        Private Sub m_cbShowIBMPackets_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbShowIBMPackets.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            Me.m_bShowIBM = m_cbShowIBMPackets.Checked
            Me.UpdateControls()
            Me.RefreshMap()

        End Sub

        Private Sub OnShowLabelsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbShowLabels.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            Me.StyleGuide.ShowMapLabels = Me.m_cbShowLabels.Checked
            Me.UpdateControls()
            Me.RefreshMap()

        End Sub

        Private Sub OnShowLabelTimeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbShowDateInLabel.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            Me.StyleGuide.ShowMapsDateInLabels = Me.m_cbShowDateInLabel.Checked
            Me.RefreshMap()

        End Sub

        Private Sub OnInvertLabelsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbInvertColor.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            Me.StyleGuide.InvertMapLabelColor = Me.m_cbInvertColor.Checked
            Me.RefreshMap()

        End Sub

        Private Sub OnChangeLabelPos(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbLabelPos.SelectedIndexChanged

            If (Me.m_bInUpdate) Then Return

            Dim iSel As Integer = Math.Max(Math.Min(9, Me.m_cmbLabelPos.SelectedIndex), 0)
            Me.StyleGuide.MapLabelPosHorizontal = DirectCast(CInt(iSel Mod 3), StringAlignment)
            Me.StyleGuide.MapLabelPosVertical = DirectCast(CInt(Math.Floor(iSel / 3)), StringAlignment)
            Me.RefreshMap()

        End Sub

        Private Sub OnRunTypeSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbRunType.SelectedIndexChanged

            If Me.m_bInUpdate Then Return

            Select Case Me.m_cmbRunType.SelectedIndex
                Case 0
                    Me.m_bpUseNewStanza.SetValue(True)
                    Me.m_bpUseIBM.SetValue(False)
                Case 1
                    Me.m_bpUseNewStanza.SetValue(False)
                    Me.m_bpUseIBM.SetValue(True)
                Case 2
                    Me.m_bpUseNewStanza.SetValue(False)
                    Me.m_bpUseIBM.SetValue(False)
            End Select
        End Sub

        Private Sub OnPause(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnPause.Click

            Me.Core.EcospacePaused = Not Me.Core.EcospacePaused
            Me.UpdateControls()

        End Sub


        Private Sub onGraphTypeCheckedChanged(sender As Object, e As System.EventArgs) _
            Handles m_rbRelBiomassGraph.CheckedChanged, m_rbConsumpGraph.CheckedChanged, m_rbFishMortGraph.CheckedChanged, m_rbPredMortGraph.CheckedChanged, m_rbCatchGraph.CheckedChanged

            Try
                Dim rb As RadioButton = DirectCast(sender, RadioButton)
                'During initialization this handler can be fired before the tag has been populated
                If rb.Tag Is Nothing Then Return

                If rb.Checked Then
                    Me.m_graphPlotType = DirectCast(rb.Tag, ePlotTypes)
                    Me.UpdateGraph()
                End If
            Catch ex As Exception

            End Try
        End Sub

#Region " FishingMort legend scaling "

        'Added for the EcoOcean model to scale Catch/Bio legend
        'if there is some way to scale legends then this will go
        Private Sub m_txFMax_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles m_txFMax.KeyUp
            If e.KeyCode = Keys.Enter Then
                Dim newMax As Single = CSng(Val(Me.m_txFMax.Text))
                If ValidateFMax(newMax) Then
                    Me.m_FishingMortMax = newMax
                    Me.RefreshPlot()
                    Me.RefreshMap()
                End If
            End If
        End Sub

        Private Function ValidateFMax(ByVal newFMax As Single) As Boolean
            Return (newFMax > 0 And newFMax < 10)
        End Function

        Private Sub OntxFMaxValidated(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_txFMax.Validated
            Try
                Dim newMax As Single = CSng(Val(Me.m_txFMax.Text))
                If newMax <> Me.m_FishingMortMax Then
                    Me.m_FishingMortMax = newMax
                    Me.RefreshPlot()
                    Me.RefreshMap()
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OntxFMaxValidating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_txFMax.Validating
            Dim newVal As Single = CSng(Val(Me.m_txFMax.Text))
            If Me.ValidateFMax(newVal) Then
                e.Cancel = False
                Return
            End If
            e.Cancel = True
            Return
        End Sub

#End Region ' FishingMort legend scaling

#Region " Hover menu "

        ''' <summary>Cross-threading delegate.</summary>
        ''' <param name="cmd"></param>
        Private Delegate Sub OnHoverMenuCommandCallbackDelegate(ByVal cmd As Object)

        Private Sub OnHoverMenuCommand(cmd As Object)

            If (Not TypeOf cmd Is eHoverCommands) Then Return

            If Me.InvokeRequired Then
                Me.Invoke(New OnHoverMenuCommandCallbackDelegate(AddressOf OnHoverMenuCommand), New Object() {cmd})
                Return
            End If

            Dim fmt As ImageFormat = ImageFormat.Png
            Dim strFile As String = Me.AskMapFileName(fmt)

            If String.IsNullOrWhiteSpace(strFile) Then Return

            Select Case DirectCast(cmd, eHoverCommands)
                Case eHoverCommands.SaveImage
                    Me.SaveMapImage(strFile, fmt)
                Case eHoverCommands.SaveImageGeoRef
                    Me.SaveMapGeoRefImages(strFile, fmt)
            End Select

        End Sub

#End Region ' Hover menu 

#End Region ' Events

#Region " cProperty events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when either of the two model state properties changes.
        ''' </summary>
        ''' <param name="prop">The property that changed.</param>
        ''' <param name="changeFlags">The extent of the change.</param>
        ''' -------------------------------------------------------------------
        Private Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags) _
                Handles m_bpUseIBM.PropertyChanged, m_bpUseNewStanza.PropertyChanged
            Me.UpdateControls()
        End Sub

#End Region ' cProperty events

#Region " Ecospace Events/Delegates "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Ecospace end-of-timestep callback. 
        ''' </summary>
        ''' <param name="TimeStepData">Data from the current time step</param>
        ''' -------------------------------------------------------------------
        Private Sub OnEcospaceTimeStep(ByRef TimeStepData As cEcospaceTimestep)

            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters()

            'Init memory for C/B, F/B
            Me.InitLocalMemory(TimeStepData)

            ' Biomass plotting
            ' For each time step, we get the Biomass from the core and store it into our array
            ' The following algorithm was extracted from EwE5. Biomass Log plotting, the value between 0.1 to 10. 
            For groupIndex As Integer = 1 To Core.nGroups
                If TimeStepData.iTimeStep = 1 Then
                    m_BaseBiomassResults(groupIndex) = TimeStepData.RelativeBiomass(groupIndex)
                    m_RelBiomassResults(groupIndex, 1) = 0
                Else
                    m_RelBiomassResults(groupIndex, TimeStepData.iTimeStep) = CSng(Math.Log10(TimeStepData.RelativeBiomass(groupIndex)))
                End If

                Me.m_graphData.Item(ePlotTypes.RelB)(groupIndex, TimeStepData.iTimeStep) = CSng(Math.Log10(TimeStepData.RelativeBiomass(groupIndex)))
                Me.m_graphData.Item(ePlotTypes.FishingMortGraph)(groupIndex, TimeStepData.iTimeStep) = TimeStepData.FishingMort(groupIndex)
                Me.m_graphData.Item(ePlotTypes.PredMortRateGraph)(groupIndex, TimeStepData.iTimeStep) = TimeStepData.PredMortRate(groupIndex)
                Me.m_graphData.Item(ePlotTypes.ConsumpRateGraph)(groupIndex, TimeStepData.iTimeStep) = TimeStepData.ConsumptRate(groupIndex)
                Me.m_graphData.Item(ePlotTypes.CatchGraph)(groupIndex, TimeStepData.iTimeStep) = TimeStepData.Catch(groupIndex)

                'JB ECOTRACER_HACK
                'jb 22-Nov-2016 For debugging set contaminant legend scaler to max across the map
                'doing this at every timestep is incorrect but shows the spatial distribution 
                'Some how we need to figure out what the concentration is over time?????
                Me.m_BaseC(groupIndex) = TimeStepData.ConcMax(groupIndex) * 0.25F

            Next

            'Temporary variables to store the timesteps for plotting. 
            m_iTimeStepPrev = m_iTimeStepCur
            m_iTimeStepCur = TimeStepData.iTimeStep

            'Update the running simulation years progress label.
            cApplicationStatusNotifier.UpdateProgress(Me.Core, My.Resources.STATUS_ECOSPACE_RUNNING, CSng(Me.m_iTimeStepCur / Me.Core.nEcospaceTimeSteps))
            Me.m_dataTimeStep = TimeStepData

            For iRow As Integer = 1 To TimeStepData.inRows
                For iCol As Integer = 1 To TimeStepData.inCols
                    For iGroup As Integer = 1 To Me.Core.nGroups
                        Dim sB As Single = TimeStepData.BiomassMap(iRow, iCol, iGroup)
                        If (sB > 0) Then
                            If (TimeStepData.ContaminantMap IsNot Nothing) Then
                                Me.m_ConcOverB(iRow, iCol, iGroup) = TimeStepData.ContaminantMap(iRow, iCol, iGroup) / sB
                            End If
                            Me.m_FoverB(iRow, iCol, iGroup) = TimeStepData.CatchMap(iRow, iCol, iGroup) / sB
                        End If '(sB > 0)

                    Next iGroup
                Next iCol
            Next iRow

            'if the size of the map has changed reset the interface
            If m_iInRow <> TimeStepData.inRows Or m_iInCol <> TimeStepData.inCols Then
                'set the map dims these are passed to the drawing threads in PlotBiomassMapThreaded()
                m_iInRow = TimeStepData.inRows
                m_iInCol = TimeStepData.inCols

                CalcMapDimension(Core.nGroups, m_iNumPlotsVert, m_iNumPlotsHorz)
                CalcMapDimension(Core.nFleets, m_iNumPlotsVert, m_iNumPlotsHorz)
            End If


            Me.AppendPlotData()
            Me.m_pbMap.Invalidate()
            'Me.UpdateControls()

            ' Save map image
            If (Me.m_cbAutoSavePNG.Checked) Then

                Dim bSave As Boolean = False
                If (Me.m_iAutosaveTS IsNot Nothing) Then
                    bSave = Me.m_iAutosaveTS.Contains(TimeStepData.iTimeStep)
                End If

                If (bSave) Then
                    Dim strPath As String = Path.Combine(Me.Core.DefaultOutputPath(eAutosaveTypes.EcospaceResults), "png")
                    Dim strFile As String = Path.Combine(strPath, Me.m_mapPlotType.ToString & String.Format("-{0:00000}", TimeStepData.iTimeStep))
                    Me.SaveMapGeoRefImages(strFile, ImageFormat.Png)
                End If
            End If

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'jb HACK auto pause every time step for debugging
            'AutoPause()
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        End Sub

#End Region ' Ecospace Delegates

#Region " Overrides "

        Private Sub OnCoreStateChanged(ByVal cms As cCoreStateMonitor)

            If cms.IsEcospaceRunning <> Me.IsRunning Then

                ' Update state flag
                Me.IsRunning = cms.IsEcospaceRunning

                ' Update status feedback
                If Me.IsRunning Then
                    cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_ECOSPACE_RUNNING)
                Else
                    cApplicationStatusNotifier.EndProgress(Me.Core)
                End If

                ' Update controls
                '    Me.m_lblProgress.Text = ""
                Me.UpdateControls()

            End If
        End Sub

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            Dim bHasRunInit As Boolean

            Try

                'jb Fixes bug 1134 
                'EcoSpace interface was not responding to messages from cCore.setEcosimRunLength() eMessageType.EcosimNYearsChanged
                'This can change the Run Length for Ecospace as well
                'It only responded if the variable was changed from the interface and there was a VarName in the cMessage.Variables() list 
                'The number of years can change by loading time series data or clicking the Reset All button on the Ecosim interface
                If msg.Type = eMessageType.EcosimNYearsChanged Then
                    If Not bHasRunInit Then
                        Me.InitCoreParams()
                        Me.InitUIParams()
                        bHasRunInit = True
                    End If
                End If

                For Each vStat As cVariableStatus In msg.Variables

                    Select Case vStat.VarName

                        Case eVarNameFlags.TotalTime, eVarNameFlags.NumTimeStepsPerYear, eVarNameFlags.EcoSimNYears

                            If Not bHasRunInit Then
                                Me.InitCoreParams()
                                Me.InitUIParams()
                                bHasRunInit = True
                            End If

                    End Select

                Next

            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".OnCoreMessage() Exception: " & ex.Message)
            End Try

        End Sub

#End Region ' Overrides

#Region " Internal implementation "

        Private Sub ClearResults()

            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters

            For i As Integer = 1 To Me.Core.nGroups - 1
                For j As Integer = 1 To Me.Core.nEcospaceTimeSteps - 1
                    Me.m_RelBiomassResults(i, j) = 0
                Next j
                Me.m_BaseBiomassResults(i) = 0
            Next i

            'clear the arrays in the graph data dictionary
            'this will leave them dimensioned but clear of the old data
            For Each dataarray As Single(,) In Me.m_graphData.Values
                Array.Clear(dataarray, 0, dataarray.Length)
            Next

            If Me.m_bOverlay = False Then
                Me.ResetGraph()
            Else
                Me.m_zgh.Overlay(Me.Core.nGroups)
            End If
            Me.RefreshPlot()
            Me.RefreshMap()
        End Sub

        Private Sub ResetGraph()

            ' ToDo: globalize this

            ' THSI CODE IS NOT DONE YET! NEED TO PROVIDE UNITS TO AXIS!!

            Dim strTitle As String = ""
            Dim strYLabel As String = ""

            Select Case Me.m_graphPlotType
                Case ePlotTypes.RelB
                    strTitle = My.Resources.ECOSPACE_HEADER_LOGBREL
                    strYLabel = My.Resources.ECOSPACE_HEADER_LOGBREL
                Case ePlotTypes.CatchGraph
                    strTitle = "Relative catch"
                    strYLabel = "?"
                Case ePlotTypes.ConsumpRateGraph
                    strTitle = "Relative consumption rates"
                    strYLabel = "?"
                Case ePlotTypes.Contaminant
                    strTitle = "Relative contaminants"
                    strYLabel = "?"
                Case ePlotTypes.CoverB
                    strTitle = "C/B"
                    strYLabel = "?"
                Case ePlotTypes.Effort
                    strTitle = "Effort"
                    strYLabel = "?"
                Case ePlotTypes.F
                    strTitle = "F"
                    strYLabel = "?"
                Case ePlotTypes.FishingMortGraph
                    strTitle = "Relative fishing mortality"
                    strYLabel = "?"
                Case ePlotTypes.FOverB
                    strTitle = "F/B"
                    strYLabel = "?"
                Case ePlotTypes.PredMortRateGraph
                    strTitle = "Relative predation mortality rate"
                    strYLabel = "?"
                Case Else
                    Debug.Assert(False)

            End Select
            Me.m_zgh.Reset(strTitle, strYLabel, Me.Core.nGroups, Me.Core.nEcospaceTimeSteps, Me.Core.EcosimFirstYear, Me.Core.EcospaceModelParameters.NumberOfTimeStepsPerYear)

        End Sub

        Private Sub UpdateGraph()

            Me.m_zgh.LogScale = (Me.m_graphPlotType = ePlotTypes.RelB)
            Me.ResetGraph()

            Dim orgTime As Integer = Me.m_iTimeStepPrev
            Me.m_iTimeStepPrev = 0
            Me.RefreshPlot()
            Me.AppendPlotData()
            Me.m_iTimeStepPrev = orgTime

        End Sub

        Private Property ShowItemMode() As eShowItemType
            Get
                Return Me.m_showitemMode
            End Get
            Set(ByVal value As eShowItemType)
                If (value <> Me.m_showitemMode) Then
                    Me.m_showitemMode = value
                    Me.UpdateControls()
                    Me.RefreshMap()
                    Me.RefreshPlot()
                End If
            End Set
        End Property

        Private Property ItemToShow() As Integer
            Get
                Return Me.m_iItemToShow
            End Get
            Set(ByVal value As Integer)
                If (value <> Me.m_iItemToShow) Then
                    Me.m_iItemToShow = value
                    Me.RefreshMap()
                    Me.RefreshPlot()
                End If
                Me.ShowItemMode = eShowItemType.ShowSingle
            End Set
        End Property

        Protected Overrides Sub UpdateControls()

            ' Sanity check
            If Me.Core Is Nothing Then Return
            If Me.m_bInUpdate = True Then Return

            Dim csm As cCoreStateMonitor = Me.Core.StateMonitor
            Dim bUseIBM As Boolean = CBool(Me.m_bpUseIBM.GetValue())
            Dim bUseNewStanza As Boolean = CBool(Me.m_bpUseNewStanza.GetValue())
            Dim bHasDrivers As Boolean = (Me.Core.nEnvironmentalDriverLayers > 0)

            Me.m_bInUpdate = True

            ' Enable run and stop buttons based on Ecospace run state
            Me.m_btnRun.Enabled = (Me.IsRunning = False)
            Me.m_btnStop.Enabled = (Me.IsRunning = True)

            Me.m_btnPause.Enabled = (Me.IsRunning = True)
            If Me.Core.EcospacePaused Then
                Me.m_btnPause.Text = My.Resources.ECOSPACE_RESUME
            Else
                Me.m_btnPause.Text = My.Resources.ECOSPACE_PAUSE
            End If

            ' Enable contaminant options based on space tracer enabled state
            Me.m_rbDisplayContaminantC.Enabled = CBool(Me.m_bpConTracing.GetValue())
            Me.m_rbDisplayCoverB.Enabled = CBool(Me.m_bpConTracing.GetValue())

            ' Enable driver options
            Me.m_rbDisplayEnvDriver.Enabled = bHasDrivers

            Select Case Me.ShowItemMode
                Case eShowItemType.ShowAll
                    Me.m_rbShowAll.Checked = True
                Case eShowItemType.ShowCustom
                    Me.m_rbShowNonHidden.Checked = True
                Case eShowItemType.ShowSingle
                    Me.m_rbShowSingle.Checked = True
            End Select

            Me.m_cbOverlay.Checked = Me.m_bOverlay
            Me.m_cbOverlay.Enabled = Me.Core.StateMonitor.IsEcospaceRunning

            Me.m_cbMPA.Checked = Me.StyleGuide.ShowMapsMPAs
            Me.m_cbMPA.Enabled = (Me.m_rbDisplayFishingEffort.Checked Or Me.m_rbDisplayRelBiomass.Checked)

            Me.m_cbShowIBMPackets.Checked = Me.m_bShowIBM
            Me.m_cbShowIBMPackets.Enabled = bUseIBM

            Dim iIndex As Integer = 2
            If bUseIBM Then iIndex = 1
            If bUseNewStanza Then iIndex = 0

            Me.m_cmbRunType.SelectedIndex = iIndex
            Me.m_cmbRunType.Enabled = (Me.IsRunning = False)

            Me.m_cbShowDateInLabel.Enabled = Me.StyleGuide.ShowMapLabels
            Me.m_cmbLabelPos.Enabled = Me.m_cbShowDateInLabel.Enabled
            Me.m_cbInvertColor.Enabled = Me.m_cbShowDateInLabel.Enabled

            Me.m_cbShowLabels.Checked = Me.StyleGuide.ShowMapLabels
            Me.m_cbShowDateInLabel.Checked = Me.StyleGuide.ShowMapsDateInLabels
            Me.m_cbInvertColor.Checked = Me.StyleGuide.InvertMapLabelColor
            Me.m_cmbLabelPos.SelectedIndex = Me.StyleGuide.MapLabelPosVertical * 3 + Me.StyleGuide.MapLabelPosHorizontal

            Me.m_hoverMenu.IsEnabled(eHoverCommands.SaveImageGeoRef) = (Me.m_mapPlotType <> ePlotTypes.Effort) And (Me.Core.StateMonitor.HasEcospaceRan)

            Me.m_tbxAutosaveTimeSteps.Enabled = (Me.m_cbAutoSavePNG.Checked = True)

            Dim bShowMap As Boolean = (Me.m_tcOutputs.SelectedIndex = 0)
            Me.m_plMapData.Visible = bShowMap
            Me.m_plMapLabels.Visible = bShowMap
            Me.m_plDisplayOptions.Visible = True
            Me.m_plMapSaveImages.Visible = bShowMap
            Me.m_plGraphData.Visible = Not bShowMap

            Me.m_cbMPA.Enabled = bShowMap And (Me.Core.nMPAs > 0)
            Me.m_cbShowIBMPackets.Enabled = bShowMap And (CBool(Me.m_bpUseIBM.GetValue()) = True)
            Me.m_cbOverlay.Enabled = Not bShowMap

            Me.m_bInUpdate = False

        End Sub

        Private Sub CheckRefreshSingleItemDropdown()

            ' No need to refresh if nothing changes
            If (Me.m_mapPlotTypeLast = Me.m_mapPlotType) Then Return

            Dim bNeedRebuilding As Boolean = False
            Select Case Me.m_mapPlotTypeLast
                Case ePlotTypes.Effort
                    bNeedRebuilding = Me.m_mapPlotType <> ePlotTypes.Effort
                Case ePlotTypes.Driver
                    bNeedRebuilding = Me.m_mapPlotType <> ePlotTypes.Driver
                Case Else
                    bNeedRebuilding = (Me.m_mapPlotType = ePlotTypes.Effort) Or (Me.m_mapPlotType = ePlotTypes.Driver)
            End Select
            Me.m_mapPlotTypeLast = Me.m_mapPlotType

            If (bNeedRebuilding) Then

                Dim desc As New cCoreInterfaceFormatter()
                Me.m_cmbDisplayItem.Items.Clear()

                Select Case Me.m_mapPlotType
                    Case ePlotTypes.Effort
                        For i As Integer = 1 To Me.Core.nFleets
                            Me.m_cmbDisplayItem.Items.Add(desc.GetDescriptor(Me.Core.EcospaceFleetInputs(i), eDescriptorTypes.Name))
                        Next i
                    Case ePlotTypes.Driver
                        Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
                        For i As Integer = 1 To Me.Core.nEnvironmentalDriverLayers
                            Me.m_cmbDisplayItem.Items.Add(desc.GetDescriptor(bm.LayerDriver(i), eDescriptorTypes.Name))
                        Next
                    Case Else
                        For i As Integer = 1 To Me.Core.nGroups
                            Me.m_cmbDisplayItem.Items.Add(desc.GetDescriptor(Me.Core.EcospaceGroupInputs(i), eDescriptorTypes.Name))
                        Next i
                End Select

                ' This is a confusing bit: bInUpdate flag prohibits a item selection from flipping view mode to single view
                ' However, we want to always select an item when this combo box has been (re)populated
                ' We certainly do not want the Ecospace form to flip back to single mode as it used to to (bug #1570)
                ' As a solution, the item selection does not affect display but is only cosmetic if we're not currently in single item mode. There.
                Me.m_bInUpdate = (Me.ShowItemMode <> eShowItemType.ShowSingle)
                Me.m_cmbDisplayItem.SelectedIndex = Math.Min(Me.m_cmbDisplayItem.Items.Count - 1, 0)
                Me.m_bInUpdate = False

            End If

        End Sub

        Private Sub RefreshMap()

            If Me.Core Is Nothing Then Return
            Me.m_pbMap.Refresh()

        End Sub

        Private Sub RefreshPlot()

            If Me.Core Is Nothing Then Return
            If (Me.m_zgh IsNot Nothing) Then

                'UpdateGraph()

                Me.m_zgh.ItemShowMode = Me.ShowItemMode
                Me.m_zgh.ItemToShow = Me.ItemToShow
                Me.m_zgh.UpdateCurveVisibility()

                Me.m_zgh.Redraw()

            End If

        End Sub


        Private Sub InitLocalMemory(TimeStepData As cEcospaceTimestep)
            Dim size As Integer = (TimeStepData.inCols + 1) * (TimeStepData.inRows + 1) * (Me.Core.nGroups + 1)
            Dim bAllocNew As Boolean = False

            'Has the memory already been allocated
            If (m_ConcOverB Is Nothing) Or (Me.m_FoverB Is Nothing) Then
                'nope we need to allocate new memory
                bAllocNew = True
            End If

            'Memory has already been allocated 
            'make sure it's the correct size
            If Not bAllocNew Then
                If (m_ConcOverB.Length <> size) Or (m_FoverB.Length <> size) Then
                    'Nope allocate new memory
                    bAllocNew = True
                End If
            End If

            'Allocate new memory
            'Or Clear the existing
            If bAllocNew Then
                'Allocate new memory
                Me.m_ConcOverB = New Single(TimeStepData.inRows, TimeStepData.inCols, Me.Core.nGroups) {}
                Me.m_FoverB = New Single(TimeStepData.inRows, TimeStepData.inCols, Me.Core.nGroups) {}
            Else
                'if we didn't allocate new memory 
                'then clear the existing data
                Array.Clear(Me.m_ConcOverB, 0, size)
                Array.Clear(Me.m_FoverB, 0, size)
            End If

        End Sub

        Private Sub AutoPause()
            Me.Core.EcospacePaused = True
            Me.UpdateControls()
        End Sub

#End Region ' Internal implementation

#Region " Image saving "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Ask the user for a location to save a map image.
        ''' </summary>
        ''' <param name="imgFormat"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function AskMapFileName(ByRef imgFormat As ImageFormat) As String

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim scenario As cEwEScenario = Me.Core.EcospaceScenarios(Me.Core.ActiveEcospaceScenarioIndex)

            If (cmdFS Is Nothing) Then Return ""

            ' ToDo: include type of plot in name

            cmdFS.Invoke(cFileUtils.ToValidFileName(scenario.Name & " results", False), SharedResources.FILEFILTER_IMAGE)

            If (cmdFS.Result = System.Windows.Forms.DialogResult.OK) Then
                imgFormat = cFileUtils.ImageFormat(cmdFS.FileName)
                Return cmdFS.FileName
            End If
            Return ""

        End Function

        Private Sub SaveMapImage(strFileName As String, imgFormat As ImageFormat)

            Dim bmp As New Bitmap(Me.m_pbMap.Width, Me.m_pbMap.Height, Imaging.PixelFormat.Format32bppArgb)
            Dim g As Graphics = Graphics.FromImage(bmp)
            Dim fmt As New cRunEcospacePlotTypeFormatter()
            Dim msg As cMessage = Nothing

            Try
                bmp.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)
                Using br As New SolidBrush(Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND))
                    g.FillRectangle(br, 0, 0, bmp.Width, bmp.Height)
                End Using
                Me.PlotMap(g)
                bmp.Save(strFileName, imgFormat)

                Me.SaveMapLegendImage(strFileName, imgFormat, fmt.GetDescriptor(Me.m_mapPlotType), SharedResources.SCALE_LOG)

                msg = New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_SUCCES, My.Resources.HEADER_MAP_IMAGES, strFileName),
                                   eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = IO.Path.GetDirectoryName(strFileName)
            Catch ex As Exception
                msg = New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_FAILURE, My.Resources.HEADER_MAP_IMAGES, strFileName, ex.Message),
                                   eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
            End Try

            g.Dispose() : g = Nothing
            bmp.Dispose() : bmp = Nothing

            If (msg IsNot Nothing) Then
                Me.Core.Messages.SendMessage(msg)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save a legend image.
        ''' </summary>
        ''' <param name="strFileName"></param>
        ''' <param name="imgFormat"></param>
        ''' <param name="strValueName">Name of the plotted variable.</param>
        ''' <param name="strDataName"></param>
        ''' -------------------------------------------------------------------
        Private Sub SaveMapLegendImage(strFileName As String, imgFormat As ImageFormat,
                                       strValueName As String, strDataName As String)

            Dim strExt As String = IO.Path.GetExtension(strFileName)
            Dim strFile As String = Path.Combine(Path.GetDirectoryName(strFileName), Path.GetFileNameWithoutExtension(strFileName))
            Dim strFilenameLegend As String = strFile & "_legend" & strExt

            'Big hack: scale images between 
            Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
            Dim nCols As Integer = bm.InCol
            Dim nRows As Integer = bm.InRow
            Dim sdummy(nRows, nCols) As Single
            Dim i As Integer = 0
            For iRow As Integer = 1 To nRows
                For iCol As Integer = 1 To nCols
                    If bm.IsModelledCell(iRow, iCol) Then
                        sdummy(iRow, iCol) = If(i = 0, 10, -10)
                        i = (i + 1) Mod 2 'FlipFlop
                    End If
                Next iCol
            Next iRow

            Dim lgd As New cLegend(Me.UIContext, strValueName)
            Dim r As cLayerRenderer = New cLayerRendererValue(New cVisualStyle())
            Dim data As New cEcospaceLayerSingle(Me.Core, sdummy, strDataName)
            Dim l As New cDisplayLayerRaster(Me.UIContext, data, r, Nothing)

            lgd.AddLayer(l)
            lgd.Save(strFilenameLegend, imgFormat)

            ' Clean up!!
            l.Dispose()

        End Sub

        Private Sub SaveMapGeoRefImages(strFileName As String, imgFormat As ImageFormat)

            Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
            Dim rc As New Rectangle(0, 0, bm.InCol * 10, bm.InRow * 10)
            Dim bmp As New Bitmap(rc.Width, rc.Height, Imaging.PixelFormat.Format32bppArgb)
            bmp.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)

            Dim maptype As cMapDrawerBase.eMapType
            Dim scaler As Single() = Nothing
            Dim drawer As New cMapDrawerGroup(Me.Core, Me.StyleGuide)
            drawer.Graphics = Graphics.FromImage(bmp)
            drawer.Colors = Me.m_legend.Colors
            drawer.ShowLand = False
            drawer.ShowBorder = False
            drawer.InCol = Me.m_iInCol
            drawer.InRow = Me.m_iInRow

            Dim fmt As New cRunEcospacePlotTypeFormatter()
            Select Case Me.m_mapPlotType

                Case ePlotTypes.RelB
                    drawer.Map = Me.m_dataTimeStep.BiomassMap
                    maptype = cMapDrawerBase.eMapType.RelBiomass
                    scaler = Me.m_BaseBiomass

                    'If parms.UseIBM And Me.m_bShowIBM Then
                    '    drawer.StanzaDS = Me.m_dataTimeStep.StanzaDS
                    'End If

                Case ePlotTypes.FOverB
                    drawer.Map = Me.m_FoverB
                    maptype = cMapDrawerBase.eMapType.FishingMortRate
                    scaler = Me.m_FishingMortScaler

                Case ePlotTypes.F
                    drawer.Map = Me.m_dataTimeStep.CatchMap
                    maptype = cMapDrawerBase.eMapType.RelCatch
                    scaler = Me.m_BaseCatch

                Case ePlotTypes.Contaminant
                    drawer.Map = Me.m_dataTimeStep.ContaminantMap
                    maptype = cMapDrawerBase.eMapType.RelContam
                    scaler = Me.m_BaseC

                Case ePlotTypes.CoverB
                    drawer.Map = Me.m_ConcOverB
                    maptype = cMapDrawerBase.eMapType.ContamRate
                    scaler = Me.m_BaseC

                Case ePlotTypes.Effort
                    ' This type of map cannot be drawn threaded because cMapDrawers are hard-wired
                    ' to render groups. Ugh.
                    Return

            End Select

            Dim mapArgs As New cMapDrawerArgs(maptype, scaler, Me.m_FishingMortMax)
            Dim strExt As String = "." & imgFormat.ToString.ToLower
            Dim strDir As String = Path.GetDirectoryName(strFileName)
            Dim strFile As String = Path.Combine(strDir, Path.GetFileNameWithoutExtension(strFileName))
            Dim msg As cMessage = Nothing
            Dim g As Graphics = Nothing

            If Not cFileUtils.IsDirectoryAvailable(strDir, True) Then
                Debug.Assert(False)
                bmp.Dispose()
                Return
            End If

            Try

                For iGroup As Integer = 1 To Me.Core.nGroups

                    Dim grp As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
                    Dim strFileSub As String = Path.Combine(strDir, cFileUtils.ToValidFileName(grp.Name, False) & "_" & Path.GetFileNameWithoutExtension(strFileName) & strExt)
                    Dim bShowGroup As Boolean = False

                    Select Case Me.ShowItemMode
                        Case eShowItemType.ShowAll
                            bShowGroup = True
                        Case eShowItemType.ShowSingle
                            bShowGroup = (iGroup = Me.ItemToShow)
                        Case eShowItemType.ShowCustom
                            bShowGroup = Me.StyleGuide.GroupVisible(iGroup)
                    End Select

                    If bShowGroup Then
                        g = Graphics.FromImage(bmp)
                        g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height)

                        drawer.DrawMap(iGroup, rc, mapArgs)
                        g.Dispose()
                        bmp.Save(strFileSub, imgFormat)

                        ' Add world file
                        Using sw As New StreamWriter(cFileUtils.ToWorldFileName(strFileSub))
                            ' Horz. pixel size, in map units
                            sw.WriteLine(cStringUtils.FormatNumber(bm.CellSize))
                            ' Rotation around y axis
                            sw.WriteLine(0)
                            ' Rotation around x axis
                            sw.WriteLine(0)
                            ' Vert. pixel size, in map units
                            sw.WriteLine(cStringUtils.FormatNumber(-bm.CellSize))
                            ' Longitude centroid of TL pixel, in map units
                            sw.WriteLine(cStringUtils.FormatNumber(bm.PosTopLeft.X + bm.CellSize / 2))
                            ' Lattitude centroid of TL pixel, in map units
                            sw.WriteLine(cStringUtils.FormatNumber(bm.PosTopLeft.Y - bm.CellSize / 2))
                            sw.Flush()
                            sw.Close()
                        End Using

                        ' Add legend file
                        Me.SaveMapLegendImage(strFileSub, imgFormat,
                                              String.Format(SharedResources.GENERIC_LABEL_DOUBLE, fmt.GetDescriptor(Me.m_mapPlotType), grp.Name), SharedResources.SCALE_LOG)

                    End If
                Next

                msg = New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_SUCCES, My.Resources.HEADER_MAP_IMAGES, strFileName), _
                       eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = strDir
            Catch ex As Exception
                msg = New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_FAILURE, My.Resources.HEADER_MAP_IMAGES, strFileName, ex.Message), _
                                   eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
            End Try

            g.Dispose() : g = Nothing
            bmp.Dispose() : bmp = Nothing

            If (msg IsNot Nothing) Then
                Me.Core.Messages.SendMessage(msg)
                cLog.Write(msg)
            End If

        End Sub

#End Region

    End Class

End Namespace
