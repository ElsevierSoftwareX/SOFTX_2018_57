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
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region ' Import

Namespace Ecospace

    Public Class frmMPAOptimizations

#Region " Helper classes "

        ''' <summary>
        ''' Utility class for maintaining a list of results in the output (zed)graph
        ''' </summary>
        Private Class ResultPoints
            Implements ZedGraph.IPointList

            Private m_list As New List(Of ZedGraph.PointPair)

            Public Sub Clear()
                Me.m_list.Clear()
            End Sub

            Public Function Clone() As Object Implements System.ICloneable.Clone
                Return Nothing
            End Function

            Public ReadOnly Property Count() As Integer Implements ZedGraph.IPointList.Count
                Get
                    Return Me.m_list.Count
                End Get
            End Property

            Default Public ReadOnly Property Item(ByVal index As Integer) As ZedGraph.PointPair Implements ZedGraph.IPointList.Item
                Get
                    Return Me.m_list(index)
                End Get
            End Property

            Public Sub AddItem(ByVal sValue As Single)
                Me.m_list.Add(New ZedGraph.PointPair(Me.Count, sValue))
            End Sub

        End Class

#End Region ' Helper classes

#Region " Private vars "

        Private Enum eFormModeTypes As Integer
            ''' <summary>User is entering values for a new search.</summary>
            Prepare
            ''' <summary>Search has been started.</summary>
            Searching
            ''' <summary>Search is running.</summary>
            Initializing
            ''' <summary>Search is stopping.</summary>
            Stopping
            ''' <summary>Search is done, results are available.</summary>
            Results
        End Enum

        ' == Data ==

        Private m_manager As cMPAOptManager = Nothing
        Private m_basemap As cEcospaceBasemap = Nothing

        ' == Layer cache ==

        ''' <summary>All layers in the basemap.</summary>
        Private m_lLayers As New List(Of cDisplayLayer)
        ''' <summary>All layers that reflect search progress.</summary>
        ''' <remarks>The data for these layers orginates from the core.</remarks>
        Private m_alayerFeedback() As cDisplayLayerRaster = Nothing
        Private m_layerSeed As cDisplayLayerRaster = Nothing
        Private m_alayerMPA() As cDisplayLayerRaster = Nothing
        ''' <summary>Data structure to update with feedback data.</summary>
        Private m_aiFeedback As Integer(,) = Nothing

        ' == Parameter IO ==

        Private m_fpStartYear As cEwEFormatProvider = Nothing
        Private m_fpEndYear As cEwEFormatProvider = Nothing
        Private m_fpMinArea As cEwEFormatProvider = Nothing
        Private m_fpMaxArea As cEwEFormatProvider = Nothing
        Private m_fpStepSize As cEwEFormatProvider = Nothing
        Private m_fpIterations As cEwEFormatProvider = Nothing
        Private m_fpBestPercentile As cEwEFormatProvider = Nothing
        Private m_fpMPA As cEwEFormatProvider = Nothing
        Private m_propSearchType As cIntegerProperty = Nothing
        Private m_fpDiscRate As cPropertyFormatProvider = Nothing
        Private m_fpGenDiscRate As cPropertyFormatProvider = Nothing
        Private m_fpBaseYear As cPropertyFormatProvider = Nothing

        ' == UI components ==

        ''' <summary>The one and only control that provides the layers interface.</summary>
        Private m_ucLayers As ucLayersControl = Nothing

        ''' <summary>Progress graph helper.</summary>
        Private m_zghProgress As cZedGraphHelper = Nothing
        ''' <summary>Progress graph data.</summary>
        Private m_aptsProgress(5) As ResultPoints

        ''' <summary>Results graph helper.</summary>
        Private m_zghResults As cZedGraphHelper = Nothing
        ''' <summary>Results graph data.</summary>
        Private m_aptsResults(6) As ResultPoints

        ''' <summary>The mode that this form is in.</summary>
        Private m_mode As eFormModeTypes = eFormModeTypes.Prepare

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()

            Me.InitializeComponent()

        End Sub

#End Region ' Constructor

#Region " Events "

#Region " Form "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim SpaceOpt As cCoreInputOutputBase = Me.UIContext.Core.EcospaceModelParameters
            Dim MPAOpt As cMPAOptParameters = Nothing

            Me.m_manager = UIContext.Core.MPAOptimizationManager
            Me.m_manager.Connect(Me, AddressOf Me.OnSeedCellCallback, AddressOf OnRunStateChanged)

            MPAOpt = Me.m_manager.MPAOptimizationParamters

            ' Add LayersControl
            Me.m_ucLayers = New ucLayersControl()
            Me.m_ucLayers.UIContext = Me.UIContext
            Me.m_ucLayers.Dock = DockStyle.Fill
            Me.m_plLayers.Controls.Add(Me.m_ucLayers)

            ' Configure objective grids
            Me.m_gridObjectives.ShowMPAOptParams = True
            Me.m_gridObjectives.Manager = Me.m_manager
            Me.m_gridObjectives.UIContext = Me.UIContext

            Me.m_gridFleet.Manager = Me.m_manager
            Me.m_gridFleet.UIContext = Me.UIContext

            Me.m_gridGroup.Manager = Me.m_manager
            Me.m_gridGroup.UIContext = Me.UIContext

            Me.m_gridProgress.UIContext = Me.UIContext
            Me.m_gridResults.UIContext = Me.UIContext

            ' Configure map
            Me.m_ucZoom.UIContext = Me.UIContext
            Me.m_ucZoomBar.UIContext = Me.UIContext
            Me.m_ucZoomBar.AddZoomContainer(Me.m_ucZoom)

            Me.m_propSearchType = New cIntegerProperty(MPAOpt, eVarNameFlags.MPAOptSearchType)
            AddHandler Me.m_propSearchType.PropertyChanged, AddressOf OnSearchTypeChanged

            ' Connect to controls
            Me.m_fpStartYear = New cPropertyFormatProvider(Me.UIContext, Me.m_nudStartYear, MPAOpt, eVarNameFlags.MPAOptStartYear)
            Me.m_fpEndYear = New cPropertyFormatProvider(Me.UIContext, Me.m_nudEndYear, MPAOpt, eVarNameFlags.MPAOptEndYear)
            Me.m_fpBaseYear = New cPropertyFormatProvider(Me.UIContext, Me.m_nudBaseYear, Me.m_manager.ObjectiveParameters, eVarNameFlags.SearchBaseYear)
            'Me.m_fpStartYear.Value = Math.Max(CSng(Me.m_fpStartYear.Value), 3)
            'Me.m_fpEndYear.Value = Math.Max(CSng(Me.m_fpEndYear.Value), 5)

            Me.m_fpMinArea = New cPropertyFormatProvider(Me.UIContext, Me.m_nudMinArea, MPAOpt, eVarNameFlags.MPAOptMinArea)
            Me.m_fpMaxArea = New cPropertyFormatProvider(Me.UIContext, Me.m_nudMaxArea, MPAOpt, eVarNameFlags.MPAOptMaxArea)
            Me.m_fpStepSize = New cPropertyFormatProvider(Me.UIContext, Me.m_nudStep, MPAOpt, eVarNameFlags.MPAOptStepSize)
            Me.m_fpIterations = New cPropertyFormatProvider(Me.UIContext, Me.m_nudIterations, MPAOpt, eVarNameFlags.MPAOptIterations)
            Me.m_fpBestPercentile = New cEwEFormatProvider(Me.UIContext, Me.m_nudBestPercentile, GetType(Single))
            Me.m_fpDiscRate = New cPropertyFormatProvider(Me.UIContext, Me.m_nudDiscRate, Me.m_manager.ObjectiveParameters, eVarNameFlags.SearchDiscountRate)
            Me.m_fpGenDiscRate = New cPropertyFormatProvider(Me.UIContext, Me.m_nudGenDiscRate, Me.m_manager.ObjectiveParameters, eVarNameFlags.SearchGenDiscRate)
            Me.m_fpMPA = New cPropertyFormatProvider(Me.UIContext, Me.m_cmbMPA, MPAOpt, eVarNameFlags.iMPAOptToUse)

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSpace, eCoreComponentType.Core}

            ' -- Sponsors --
            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME)
            cmd.AddControl(Me.m_pbLenfest, New Object() {"http://www.lenfestocean.org/"})
            cmd.AddControl(Me.m_pbDuke, New Object() {"http://mgel.env.duke.edu/"})

            ' Configure graphs
            Me.InitProgressGraph()
            Me.InitOutputGraph()

            Me.ReloadMPAChoices()

            ' Kick off
            Me.Reload()
            Me.OnSearchTypeChanged(Me.m_propSearchType, cProperty.eChangeFlags.All)

            ' Respond to current run state
            Me.OnRunStateChanged(Me.m_manager.RunState)

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            ' Terminate any run state feedback
            Me.ExitMode()

            ' -- Sponsors --
            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME)
            cmd.RemoveControl(Me.m_pbLenfest)
            cmd.RemoveControl(Me.m_pbDuke)

            Dim alays As cDisplayLayer() = Me.m_lLayers.ToArray

            Me.m_ucZoomBar.RemoveZoomContainer(Me.m_ucZoom)
            For Each l As cDisplayLayer In alays
                Me.RemoveLayer(l)
            Next
            Me.m_lLayers = Nothing

            RemoveHandler Me.m_zghResults.OnCursorPos, AddressOf OnResultCursorPos
            Me.m_zghResults.Detach()
            Me.m_zghProgress.Detach()

            RemoveHandler Me.m_propSearchType.PropertyChanged, AddressOf OnSearchTypeChanged
            Me.m_propSearchType = Nothing

            Me.CoreComponents = Nothing

            Me.m_fpMPA.Release()
            Me.m_fpBaseYear.Release()
            Me.m_fpBestPercentile.Release()
            Me.m_fpDiscRate.Release()
            Me.m_fpEndYear.Release()
            Me.m_fpGenDiscRate.Release()
            Me.m_fpIterations.Release()
            Me.m_fpMaxArea.Release()
            Me.m_fpMinArea.Release()
            Me.m_fpMPA.Release()
            Me.m_fpStartYear.Release()
            Me.m_fpStepSize.Release()

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Form

#Region " Controls "

        Private Sub OnRun(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_btnRun.Click

            ' Abort if not all inputs valid
            If Not Me.ValidateInputs Then Return
            ' Start run
            Me.m_manager.Run()
        End Sub

        Private Sub OnStop(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_btnStop.Click

            Me.RunMode = eFormModeTypes.Stopping
            Me.m_manager.StopRun()

        End Sub

        Private Sub OnClearSeedCells(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_tsmClearSeed.Click
            Me.m_manager.clearSeedCells()
            ' Re-render the map
            Me.m_ucZoom.Map.Refresh()
        End Sub

        Private Sub OnClearMPACells(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_tsmClearMPA.Click
            Me.m_manager.clearMPAs()
            ' Re-render the map
            Me.m_ucZoom.Map.Refresh()
        End Sub

        Private Sub OnSetAllSeedCells(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_tsmSetAllSeed.Click
            Me.m_manager.setAllCellsToSeed(Me.SelectedMPA())
            ' Re-render the map
            Me.m_ucZoom.Map.Refresh()
        End Sub

        Private Sub OnSetAllMPACells(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_tsmSetAllMPA.Click
            Me.m_manager.setAllCellsToMPA(Me.SelectedMPA())
            ' Re-render the map
            Me.m_ucZoom.Map.Refresh()
        End Sub

        Private Sub OnEditLayers(ByVal sender As Object, ByVal e As System.EventArgs) _
                Handles m_tsbEditLayers.Click

            ' Note that the command is invoked manually here because in THIS FORM only the command will be enabled when
            ' preparing Ecoseed. Yes, it's a half-ass solution while in fact the entire GUI should become aware the 
            ' running of a model by blocking out any possibility to enter/edit data.
            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand("EditImportanceMaps")
            If cmd IsNot Nothing Then cmd.Invoke()

        End Sub

        Private Sub OnModeEcoseed(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_rbEcoseed.CheckedChanged

            If Me.m_rbEcoseed.Checked Then
                Me.SearchType = eMPAOptimizationModels.EcoSeed
                Me.UpdateControls()
            End If

        End Sub

        Private Sub OnModeRandom(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_rbRandom.CheckedChanged

            If Me.m_rbRandom.Checked Then
                Me.SearchType = eMPAOptimizationModels.RandomSearch
                Me.UpdateControls()
            End If

        End Sub

        Private Sub OnResetMPAs(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_btnResetMPAs.Click

            Me.m_ucZoom.SuspendLayout()

            Try
                ' Set the layer
                For i As Integer = 1 To Me.Core.nMPAs
                    Me.SetLayer(Me.m_manager.OrgMPA(i), Me.m_basemap.LayerMPA(i))
                Next i

                ' Update MPAs (JS: is this necessary?)
                For Each l As cDisplayLayer In Me.m_alayerMPA
                    l.Update(cDisplayLayer.eChangeFlags.Map)
                Next
            Catch ex As Exception
                ' NOP
            End Try
            Me.m_ucZoom.ResumeLayout()

        End Sub

        Private Sub OnReset(ByVal sender As System.Object, ByVal e As System.EventArgs)


            Me.RunMode = eFormModeTypes.Prepare

        End Sub

        Private Sub OnSelectAreaClosed(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbAreaClosed.SelectedIndexChanged

            If (Me.m_propSearchType Is Nothing) Then Return
            Try
                Me.UpdateBestCountMap()
                Me.UpdateResultsGraph()
            Catch ex As Exception
                ' nop
            End Try

        End Sub

        Private Sub OnBestPercentileChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_nudBestPercentile.ValueChanged

            If (Me.m_propSearchType Is Nothing) Then Return

            Try
                Me.ShowBestPercentage()
                Me.UpdateResultsGraph()
            Catch ex As Exception
                ' NOP
            End Try

        End Sub

        ''' <summary>
        ''' Event handler, responds to the user exploring the progress graph.
        ''' </summary>
        Private Sub OnResultCursorPos(ByVal zgh As cZedGraphHelper, ByVal iPane As Integer, ByVal sPos As Single)
            Try
                Me.ShowIteration(CInt(Math.Round(Me.m_zghResults.CursorPos)))
            Catch ex As Exception
                ' NOP
            End Try
        End Sub

        Private Sub OnConvertToMPA(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnConvertToMpa.Click

            Dim aiMap As Integer(,) = Nothing
            Dim iNumResults As Integer = 0

            Select Case Me.SearchType

                Case eMPAOptimizationModels.EcoSeed
                    Me.SetLayer(Me.m_aiFeedback, Me.m_basemap.LayerMPA(SelectedMPA()), Me.SelectedMPA())

                Case eMPAOptimizationModels.RandomSearch
                    ' Get cell map at 100% best cells
                    aiMap = Me.m_manager.CellSelectedMap(Me.SelectedBestPercentile(), _
                                                         Me.SelectedClosedPercentage(), iNumResults)
                    ' Convert to MPA
                    Me.ConvertToMPA(aiMap, Me.SelectedClosedPercentage, Me.SelectedMPA())

            End Select

            ' Refresh the MPA layer that has been affected
            For Each l As cDisplayLayer In Me.m_alayerMPA
                'If l.Data.Index = Me.SelectedMPA Then
                l.Update(cDisplayLayer.eChangeFlags.Map)
                ' End If
            Next

        End Sub

        Private Sub OnSave(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnSave.Click

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand("ExportLayerData")
            Dim lLayers As New List(Of cDisplayLayer)
            Dim layerTmp As cDisplayLayer = Nothing
            Dim ldataTmp As cEcospaceLayerInteger = Nothing
            Dim iAreaClosed As Integer = 0
            Dim iNumResults As Integer = 0

            If cmd Is Nothing Then Return

            ' Conjure a best 100% layer for every AreaPercClosed level
            For iLevel As Integer = 0 To Me.m_cmbAreaClosed.Items.Count - 1

                ' Get area closed
                iAreaClosed = CInt(Me.m_cmbAreaClosed.Items(iLevel))
                ' Wrap this in a core map layer to handle projections
                ldataTmp = New cEcospaceLayerInteger(Me.UIContext.Core, _
                                                     Me.m_manager.CellSelectedMap(100, iAreaClosed, iNumResults), _
                                                     cStringUtils.Localize(My.Resources.ECOSPACE_LAYER_MPABESTCOUNT, iAreaClosed))
                ' Wrap THIS in turn in a GUI layer, required by the exporter
                layerTmp = New cDisplayLayerRaster(Me.UIContext, ldataTmp, Nothing, Nothing)
                ' Add the layer to the stash to save
                lLayers.Add(layerTmp)

            Next iLevel

            cmd.Tag = lLayers.ToArray()
            cmd.Invoke()

        End Sub

        Private Sub OnAutoSaveOutputChecked(sender As System.Object, e As System.EventArgs) _
            Handles m_cbAutoSave.CheckedChanged
            Try
                Me.Core.Autosave(eAutosaveTypes.MPAOpt) = Me.m_cbAutoSave.Checked
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Controls

#Region " Search manager "

        Private Sub OnSeedCellCallback()
            Try
                Me.HandleSeedCellCallback()
            Catch ex As Exception
                ' Protect calling process from potential UI madness
            End Try
        End Sub

        Private Sub OnRunStateChanged(ByVal runstate As cMPAOptManager.eRunStates)

            Try
                Select Case runstate

                    Case cMPAOptManager.eRunStates.Initializing
                        Me.RunMode = eFormModeTypes.Initializing

                    Case cMPAOptManager.eRunStates.Searching
                        Me.RunMode = eFormModeTypes.Searching

                    Case cMPAOptManager.eRunStates.Completed
                        Me.RunMode = eFormModeTypes.Results

                    Case cMPAOptManager.eRunStates.NewCellSelected
                        Me.HandleNewCellSelected()

                    Case cMPAOptManager.eRunStates.NewBestResultFound
                        Me.HandleNewBestResultFound()

                End Select
            Catch ex As Exception
                ' Protect calling process from potential UI madness
            End Try

        End Sub

#End Region ' Search manager

#Region " Core "

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            Select Case msg.Source

                Case eCoreComponentType.EcoSpace
                    If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                        ' Reload data
                        Me.Reload()
                        ' Cascade mode down
                        Me.RunMode = eFormModeTypes.Prepare
                    End If

                Case eCoreComponentType.Core
                    If (msg.Type = eMessageType.GlobalSettingsChanged) Then
                        Me.UpdateControls()
                    End If

            End Select

        End Sub

#End Region ' Core

#Region " Properties "

        Private m_bInUpdate As Boolean = False

        Private Sub OnSearchTypeChanged(ByVal prop As cProperty, ByVal change As cProperty.eChangeFlags)
            Debug.Assert(ReferenceEquals(prop, Me.m_propSearchType))

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            Select Case CInt(prop.GetValue())
                Case eMPAOptimizationModels.EcoSeed
                    Me.m_rbEcoseed.Checked = True
                Case eMPAOptimizationModels.RandomSearch
                    Me.m_rbRandom.Checked = True
                Case Else
                    Debug.Assert(False, cStringUtils.Localize("Unsupported search type selected {0}", CInt(prop.GetValue())))
            End Select
            Me.m_bInUpdate = False
        End Sub

#End Region ' Properties

#Region " Map "

        Private Sub OnLayerChanged(ByVal l As cDisplayLayer, ByVal changeFlags As cDisplayLayer.eChangeFlags)
            If ((changeFlags And cDisplayLayer.eChangeFlags.Selected) > 0) Then Me.UpdateControls()
        End Sub

#End Region ' Map

#End Region ' Events

#Region " Internals "

#Region " One-time initialization "

        Private Sub InitProgressGraph()

            Dim zgcr As New ZedGraph.ColorSymbolRotator

            ' Flush first color to make sure that the two graps (progress and output) use the same colour scheme
            Dim clrFlush As Color = zgcr.NextColor
            Dim gp As GraphPane = Nothing

            For i As Integer = 0 To 5
                Me.m_aptsProgress(i) = New ResultPoints()
            Next

            Me.m_zghProgress = New cZedGraphHelper()
            Me.m_zghProgress.Attach(Me.UIContext, Me.m_graphProgress)
            gp = Me.m_zghProgress.GetPane(1)

            With gp

                .Legend.Position = ZedGraph.LegendPos.Right
                .Title.IsVisible = False
                .XAxis.Title.Text = "" ' Config with form mode
                .YAxis.Title.Text = "" ' Config with form mode

                ' JS 19nov08: let graph figure out the ticks
                '' Only show major ticks
                'Me.m_graphProgress.GraphPane.XAxis.Scale.MajorStep = 5
                'Me.m_graphProgress.GraphPane.XAxis.Scale.MinorStep = 1

                .AddCurve(SharedResources.HEADER_NET_ECONOMIC_VALUE, Me.m_aptsProgress(0), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_SOCIAL_VALUE_EMPLOYMENT, Me.m_aptsProgress(1), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_MANDATED_REBUILDING, Me.m_aptsProgress(2), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_ECOSYSTEM_STRUCTURE, Me.m_aptsProgress(3), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_BIODIVERSITY, Me.m_aptsProgress(4), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_BOUNDARYWEIGHT, Me.m_aptsProgress(5), zgcr.NextColor, ZedGraph.SymbolType.None)

            End With

            Me.m_zghProgress.AutoscalePane = True

        End Sub

        Private Sub InitOutputGraph()

            Dim zgcr As New ZedGraph.ColorSymbolRotator
            Dim gp As GraphPane = Nothing

            Me.m_zghResults = New cZedGraphHelper()
            Me.m_zghResults.Attach(Me.UIContext, Me.m_graphResults)
            Me.m_zghResults.ShowCursor = True

            For i As Integer = 0 To 6
                Me.m_aptsResults(i) = New ResultPoints()
            Next

            gp = Me.m_zghResults.GetPane(1)
            With gp

                .Legend.Position = ZedGraph.LegendPos.Right
                .Title.IsVisible = False
                .Title.Text = "" ' Config with form mode
                .Title.Text = "" ' Config with form mode

                ' JS 19nov08: let graph figure out the ticks
                '' Only show major ticks
                'Me.m_graphResults.GraphPane.XAxis.Scale.MajorStep = 5
                'Me.m_graphResults.GraphPane.XAxis.Scale.MinorStep = 1

                .AddCurve(SharedResources.HEADER_NET_ECONOMIC_VALUE, Me.m_aptsResults(1), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_SOCIAL_VALUE_EMPLOYMENT, Me.m_aptsResults(2), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_MANDATED_REBUILDING, Me.m_aptsResults(3), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_ECOSYSTEM_STRUCTURE, Me.m_aptsResults(4), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_BIODIVERSITY, Me.m_aptsResults(5), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(SharedResources.HEADER_BOUNDARYWEIGHT, Me.m_aptsResults(6), zgcr.NextColor, ZedGraph.SymbolType.None)
                .AddCurve(My.Resources.SEARCH_LABEL_TOTAL_WEIGHTED, Me.m_aptsResults(0), zgcr.NextColor, ZedGraph.SymbolType.None)

            End With

            Me.m_zghResults.AutoscalePane = True
            AddHandler Me.m_zghResults.OnCursorPos, AddressOf OnResultCursorPos

        End Sub

#End Region ' One-time initialization

#Region " Run mode specific updates "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The one controller that determines what is displayed in the form.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property RunMode() As eFormModeTypes
            Get
                Return Me.m_mode
            End Get
            Set(ByVal value As eFormModeTypes)
                ' Switching?
                If value <> Me.m_mode Then
                    ' Exit previous mode
                    Me.ExitMode()
                    ' Store mode
                    Me.m_mode = value
                    ' Enter new mode
                    Me.EnterMode()
                    ' Reflect changes
                    Me.UpdateControls()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Toggle search type, only valid in <see cref="eFormModeTypes.Prepare">Prepare</see> mode.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property SearchType() As eMPAOptimizationModels
            Get
                Return DirectCast(Me.m_propSearchType.GetValue(), eMPAOptimizationModels)
            End Get
            Set(ByVal value As eMPAOptimizationModels)
                ' Only valid while preparing a run
                If (Me.RunMode <> eFormModeTypes.Prepare) Then Return

                ' Clean up
                Me.ClearMapFeedback()
                Me.ClearResults()

                Dim factory As New cLayerFactoryInternal()

                ' Set search type
                If (Me.m_propSearchType IsNot Nothing) Then Me.m_propSearchType.SetValue(value)

                ' Polute again
                Me.InitMapFeedback()

                ' Update visible state of existing layers
                Me.ShowLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerMPASeed), _
                    SearchType = eMPAOptimizationModels.EcoSeed, SearchType = eMPAOptimizationModels.EcoSeed)
                Me.ShowLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerMPARandom), _
                    SearchType = eMPAOptimizationModels.RandomSearch, SearchType = eMPAOptimizationModels.RandomSearch)
                Me.ShowLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerImportance), _
                     SearchType = eMPAOptimizationModels.RandomSearch, SearchType = eMPAOptimizationModels.RandomSearch)

                ' Update graph labels
                Select Case SearchType
                    Case eMPAOptimizationModels.EcoSeed
                        Me.m_graphProgress.GraphPane.XAxis.Title.Text = My.Resources.MPAOPT_AXISLABEL_ECOSEED
                        Me.m_graphResults.GraphPane.XAxis.Title.Text = My.Resources.MPAOPT_AXISLABEL_ECOSEED
                    Case eMPAOptimizationModels.RandomSearch
                        Me.m_graphProgress.GraphPane.XAxis.Title.Text = My.Resources.MPAOPT_AXISLABEL_RANDOMSEARCH
                        Me.m_graphResults.GraphPane.XAxis.Title.Text = My.Resources.MPAOPT_AXISLABEL_BESTITERATIONS
                End Select

                Me.m_zghProgress.RescaleAndRedraw()

            End Set
        End Property

        Private Function SelectedClosedPercentage() As Integer
            Dim iPerc As Integer = 20
            Try
                iPerc = CInt(Me.m_cmbAreaClosed.Items(Me.m_cmbAreaClosed.SelectedIndex))
            Catch ex As Exception
                ' Wow
            End Try
            Return iPerc
        End Function

        Private Function SelectedBestPercentile() As Single
            Return CSng(Me.m_nudBestPercentile.Value)
        End Function

        Private Function SelectedMPA() As Integer
            If (Me.m_fpMPA Is Nothing) Then Return 0
            Return CInt(Me.m_fpMPA.Value())
        End Function

        Private Sub EnterMode()

            Select Case Me.m_mode
                Case eFormModeTypes.Prepare
                    ' User is about to start entering data

                Case eFormModeTypes.Initializing
                    ' Set stop delegate
                    Me.Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.m_manager.StopRun))
                    ' Set running status text
                    cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_SEARCH_INITIALIZING, -1)
                    ' Switch to 'Results' page
                    Me.m_tcResults.SelectedIndex = 0

                Case eFormModeTypes.Searching
                    ' Set stop delegate
                    Me.Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.m_manager.StopRun))
                    ' Set running status text
                    cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_SEARCH_SEARCHING, -1)

                Case eFormModeTypes.Stopping
                    ' Set running status text
                    cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_SEARCH_STOPPING, -1)

                Case eFormModeTypes.Results
                    ' Switch to 'Results' page
                    Me.m_tcResults.SelectedIndex = 1
                    ' Show results if possible
                    If Me.m_cmbAreaClosed.Items.Count > 0 Then
                        Me.m_cmbAreaClosed.SelectedIndex = 0
                    End If

            End Select

        End Sub

        Private Sub ExitMode()
            Select Case Me.m_mode

                Case eFormModeTypes.Prepare ' Prepare for running mode

                Case eFormModeTypes.Searching
                    ' Cancel running status text
                    Me.Core.SetStopRunDelegate(Nothing)
                    cApplicationStatusNotifier.EndProgress(Me.Core)

                Case eFormModeTypes.Initializing
                    ' Cancel running status text
                    Me.Core.SetStopRunDelegate(Nothing)
                    cApplicationStatusNotifier.EndProgress(Me.Core)

                Case eFormModeTypes.Stopping
                    ' Cancel running status text
                    cApplicationStatusNotifier.EndProgress(Me.Core)

                Case eFormModeTypes.Results ' Show results
                    ' Clear results
                    Me.ClearResults()

            End Select

        End Sub

        Private Sub Reload()
            ' Store ref
            Me.m_basemap = Me.UIContext.Core.EcospaceBasemap
            Me.ReloadMap()
            Me.ReloadMPAChoices()
            Me.Invalidate(True)
        End Sub

        Private Sub ReloadMap()

            Me.m_ucZoom.Map.SuspendLayout()
            Me.m_ucLayers.LockUpdates()

            Me.m_ucZoom.Map.Clear()

            Me.m_alayerMPA = Me.AddBaseLayers(eVarNameFlags.LayerMPA, True)
            Me.m_layerSeed = Me.AddBaseLayers(eVarNameFlags.LayerMPASeed, True)(0)
            Me.AddBaseLayers(eVarNameFlags.LayerMPARandom, True)
            Me.AddBaseLayers(eVarNameFlags.LayerImportance, False)
            Me.AddBaseLayers(eVarNameFlags.LayerHabitat, False)
            Me.AddBaseLayers(eVarNameFlags.LayerDepth, False)

            ' Hide habitat layers at startup
            Dim factory As New cLayerFactoryInternal()
            Me.ShowLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerHabitat), False, True)

            Me.m_ucLayers.UnlockUpdates()
            Me.m_ucZoom.Map.ResumeLayout()

            Me.m_layerSeed.IsSelected = True

        End Sub

        Private Sub ReloadMPAChoices()

            ' Get MPA optimization params to connect start MPA to
            Dim MPAOpt As cMPAOptParameters = Me.UIContext.Core.MPAOptimizationManager.MPAOptimizationParamters
            ' Create list of available MPAs
            Dim alMPAs As New List(Of cCoreInputOutputBase)

            ' Build list of MPAs
            For iMPA As Integer = 1 To Me.UIContext.Core.nMPAs
                alMPAs.Add(Me.UIContext.Core.EcospaceMPAs(iMPA))
            Next

            Me.m_fpMPA.Items = alMPAs.ToArray

            ' Only one MPA available?
            If alMPAs.Count = 1 Then
                ' #Yes: select first MPA
                Me.m_fpMPA.Value = alMPAs(0).Index
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, called when a new seed cell has been selected.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub HandleSeedCellCallback()

            ' Sanity check
            If Not (Me.m_manager.IsRunning()) Then Return

            Dim output As cMPAOptOutput = Me.m_manager.CurrentRowColResults

            ' Perform search specific updates
            Select Case Me.SearchType

                Case eMPAOptimizationModels.EcoSeed
                    Try
                        ' Ecoseed: the seed cell configuration has changed. 
                        ' The seed cell map has to be updated, which is done in the GUI
                        ' Populate run state layer with current row/col results
                        For iRow As Integer = 0 To Me.m_basemap.InRow
                            For iCol As Integer = 0 To Me.m_basemap.InCol
                                Me.m_aiFeedback(iRow, iCol) = cLayerFactoryInternal.cECOSEED_LAYER_NOVALUE
                            Next iCol
                        Next iRow

                        If output.CurRow > 0 And output.CurCol > 0 Then
                            Me.m_aiFeedback(output.CurRow, output.CurCol) = cLayerFactoryInternal.cECOSEED_LAYER_CURRENTVALUE
                        End If
                        If output.BestRow > 0 And output.BestCol > 0 Then
                            Me.m_aiFeedback(output.BestRow, output.BestCol) = cLayerFactoryInternal.cECOSEED_LAYER_BESTVALUE
                        End If

                        ' Make the map redraw itself
                        Me.m_ucZoom.Map.Refresh()
                    Catch ex As Exception

                    End Try

                    Me.m_gridProgress.LogResult(output.EconomicValue, output.SocialValue, _
                                                output.MandatedValue, output.EcologicalValue, _
                                                output.BiomassDiversityValue, output.AreaBoundaryValue, _
                                                output.TotalValue, output.PercentageClosed)

                Case eMPAOptimizationModels.RandomSearch

                    ' MPA layout has changed
                    Me.m_ucZoom.Map.Refresh()

                    Me.LogProgress(output.EconomicValue, output.SocialValue, _
                                   output.MandatedValue, output.EcologicalValue, _
                                   output.BiomassDiversityValue, output.AreaBoundaryValue, _
                                   output.TotalValue, output.PercentageClosed)

            End Select

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, called when a new cell has been selected.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub HandleNewCellSelected()

            ' Sanity check
            Debug.Assert(Me.m_manager.IsRunning())

            Dim output As cMPAOptOutput = Me.m_manager.CurrentRowColResults

            Try

                Select Case Me.SearchType

                    Case eMPAOptimizationModels.EcoSeed
                        ' A new MPA cell has been selected out of the seed cells
                        ' Redraw MPA map
                        Me.m_ucZoom.Map.Refresh()
                        ' Show this in the graph
                        Me.LogProgress(output.EconomicValue, output.SocialValue, _
                                            output.MandatedValue, output.EcologicalValue, _
                                            output.BiomassDiversityValue, output.AreaBoundaryValue, _
                                            output.TotalValue, output.PercentageClosed)

                    Case eMPAOptimizationModels.RandomSearch
                        ' Does not apply to Random search

                End Select

            Catch ex As Exception

            End Try

        End Sub

        Private Sub HandleNewBestResultFound()

            Dim output As cMPAOptOutput = Me.m_manager.CurrentRowColResults

            ' Sanity check
            Debug.Assert(Me.m_manager.IsRunning())

            Try

                Select Case Me.SearchType

                    Case eMPAOptimizationModels.EcoSeed

                        ' Ecoseed: the seed cell configuration has changed. 
                        ' The seed cell map has to be updated, which is done in the GUI
                        ' Populate run state layer with current row/col results
                        For iRow As Integer = 0 To Me.m_basemap.InRow
                            For iCol As Integer = 0 To Me.m_basemap.InCol
                                Me.m_aiFeedback(iRow, iCol) = cLayerFactoryInternal.cECOSEED_LAYER_NOVALUE
                            Next iCol
                        Next iRow

                        If output.CurRow > 0 And output.CurCol > 0 Then
                            Me.m_aiFeedback(output.CurRow, output.CurCol) = cLayerFactoryInternal.cECOSEED_LAYER_CURRENTVALUE
                        End If
                        If output.BestRow > 0 And output.BestCol > 0 Then
                            Me.m_aiFeedback(output.BestRow, output.BestCol) = cLayerFactoryInternal.cECOSEED_LAYER_BESTVALUE
                        End If

                        ' Make the map redraw itself
                        Me.m_ucZoom.Map.Refresh()

                    Case eMPAOptimizationModels.RandomSearch
                        ' NOP

                End Select

            Catch ex As Exception

            End Try

        End Sub

#End Region ' Run-mode specific updates

#Region " Map updating "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper function to create layer(s) for a given varname.
        ''' </summary>
        ''' <param name="varName">The core variable to load basemap data for.</param>
        ''' -------------------------------------------------------------------
        Private Function AddBaseLayers(ByVal varName As eVarNameFlags, bEditable As Boolean) As cDisplayLayerRaster()

            Dim factory As New cLayerFactoryInternal()
            Dim strGroup As String = factory.GetLayerGroup(varName)
            Dim alayers As cDisplayLayerRaster() = factory.GetLayers(Me.UIContext, varName)
            Dim l As cDisplayLayer = Nothing
            Dim strCommand As String = ""

            If (bEditable) Then strCommand = factory.GetLayerEditCommand(varName)

            ' Add group, and collapse and hide habitat layers
            Me.m_ucLayers.AddGroup(strGroup, strCommand, True, False)

            ' Add individual layers
            For iLayer As Integer = 0 To alayers.Length - 1
                l = alayers(iLayer)
                If (TypeOf (l) Is cDisplayLayerRaster) Then
                    ' Add the layer to the control(s)
                    Dim rl As cDisplayLayerRaster = DirectCast(l, cDisplayLayerRaster)
                    rl.Editor.IsReadOnly = Not bEditable
                    Me.AddLayer(l, strGroup, strCommand)
                End If
            Next

            Return alayers

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub InitMapFeedback()

            Dim strGroup As String = ""
            Dim l As cDisplayLayerRaster = Nothing
            Dim alayers As cDisplayLayerRaster() = Nothing
            Dim lRunStateLayers As New List(Of cDisplayLayerRaster)
            Dim factory As New cLayerFactoryInternal()
            Dim datalayerTemp As cEcospaceLayerInteger = Nothing

            Me.m_ucLayers.LockUpdates()

            Try

                ' Redim data
                ReDim Me.m_aiFeedback(Me.m_basemap.InRow, Me.m_basemap.InCol)

                Select Case Me.SearchType

                    Case eMPAOptimizationModels.EcoSeed

                        ' Get group
                        strGroup = factory.GetLayerGroup(eVarNameFlags.LayerMPASeed)

                        ' DO NOT CHANGE THE ORDER OF LAYERS HERE TO ENSURE THAT THE 
                        ' SEED PROGRESS INDICATORS SHOW UP ON TOP OF THE RUNNING SEED CELLS,
                        ' AND THAT THE BEST CELL SHOWS UP ON TOP OF THE CURRENT CELL

                        ' Create best cell layer
                        datalayerTemp = New cEcospaceLayerInteger(Me.UIContext.Core, Me.m_aiFeedback, My.Resources.ECOSPACE_LAYER_SEEDBEST, Nothing, eVarNameFlags.LayerMPASeedBest)
                        alayers = factory.GetLayers(Me.UIContext, eVarNameFlags.LayerMPASeedBest, datalayerTemp)
                        For iLayer As Integer = 0 To alayers.Length - 1
                            l = alayers(iLayer)
                            l.Editor.IsReadOnly = True
                            Me.AddLayer(l, strGroup, "", Me.m_layerSeed)
                        Next
                        lRunStateLayers.AddRange(alayers)

                        ' Create current cell layer(s)
                        datalayerTemp = New cEcospaceLayerInteger(Me.UIContext.Core, Me.m_aiFeedback, My.Resources.ECOSPACE_LAYER_SEEDCURRENT, Nothing, eVarNameFlags.LayerMPASeedCurrent)
                        alayers = factory.GetLayers(Me.UIContext, eVarNameFlags.LayerMPASeedCurrent, datalayerTemp)
                        For iLayer As Integer = 0 To alayers.Length - 1
                            l = alayers(iLayer)
                            l.Editor.IsReadOnly = True
                            Me.AddLayer(l, strGroup, "", Me.m_layerSeed)
                        Next

                        lRunStateLayers.AddRange(alayers)

                    Case eMPAOptimizationModels.RandomSearch

                        ' Create random output layer
                        strGroup = factory.GetLayerGroup(eVarNameFlags.LayerMPARandom)
                        datalayerTemp = New cEcospaceLayerInteger(Me.UIContext.Core, Me.m_aiFeedback, My.Resources.ECOSPACE_LAYER_RANDOMBEST, Nothing, eVarNameFlags.LayerMPARandom)

                        ' Create current cell layer(s)
                        alayers = factory.GetLayers(Me.UIContext, eVarNameFlags.LayerMPARandom, datalayerTemp)
                        For iLayer As Integer = 0 To alayers.Length - 1
                            l = alayers(iLayer)
                            l.Editor.IsReadOnly = True
                            Me.AddLayer(l, strGroup, "")
                        Next
                        lRunStateLayers.AddRange(alayers)

                End Select

                Me.m_alayerFeedback = lRunStateLayers.ToArray()

            Catch ex As Exception

            End Try

            Me.m_ucLayers.UnlockUpdates()

        End Sub

        Private Sub ClearMapFeedback()
            If Me.m_alayerFeedback IsNot Nothing Then
                For Each l As cDisplayLayer In Me.m_alayerFeedback
                    Me.RemoveLayer(l)
                Next
                Me.m_alayerFeedback = Nothing
            End If
            Me.m_aiFeedback = Nothing
        End Sub

        Private Sub UpdateBestCountMap()

            Select Case Me.SearchType

                Case eMPAOptimizationModels.EcoSeed

                Case eMPAOptimizationModels.RandomSearch

                    Dim iNumResults As Integer = 0
                    Dim aiCells(,) As Integer = Me.m_manager.CellSelectedMap(Me.SelectedBestPercentile, _
                                                                             Me.SelectedClosedPercentage, _
                                                                             iNumResults)

                    For iRow As Integer = 1 To Me.m_basemap.InRow
                        For iCol As Integer = 1 To Me.m_basemap.InCol
                            Me.m_aiFeedback(iRow, iCol) = aiCells(iRow, iCol)
                        Next
                    Next

                    ' In Random MPA, layer(0) is the only feedback layer
                    ' Invalidate to recalc min, max
                    Me.m_alayerFeedback(0).IsModified = True
                    ' Trigger redraw
                    Me.m_alayerFeedback(0).Update(cDisplayLayer.eChangeFlags.Map)

            End Select

        End Sub

#End Region ' Map updating

#Region " Progress "

        Private Sub LogProgress(ByVal sEconomicValue As Single, ByVal sSocialValue As Single, _
                                     ByVal sMandatedValue As Single, ByVal sEcologicalValue As Single, _
                                     ByVal sBiomassDiversityValue As Single, ByVal sBoundaryWeightValue As Single, _
                                     ByVal sTotalValue As Single, ByVal sAreaPercentageClosed As Single)

            ' Show this in the graph
            Dim strPerc As String = CStr(Math.Round(sAreaPercentageClosed))
            Dim gp As GraphPane = Me.m_zghProgress.GetPane(1)

            ' All 0: do not log
            If (sEconomicValue + sSocialValue + sMandatedValue + sEcologicalValue + sBiomassDiversityValue) = 0.0 Then Return

            For iResult As Integer = 0 To Me.m_aptsProgress.Length - 1
                Dim rp As ResultPoints = Me.m_aptsProgress(iResult)
                Select Case iResult
                    Case 0 : rp.AddItem(sEconomicValue)
                    Case 1 : rp.AddItem(sSocialValue)
                    Case 2 : rp.AddItem(sMandatedValue)
                    Case 3 : rp.AddItem(sEcologicalValue)
                    Case 4 : rp.AddItem(sBiomassDiversityValue)
                    Case 5 : rp.AddItem(sBoundaryWeightValue)
                End Select
            Next

            Me.m_zghProgress.RescaleAndRedraw()

            Me.m_gridProgress.LogResult(sEconomicValue, sSocialValue, _
                                        sMandatedValue, sEcologicalValue, _
                                        sBiomassDiversityValue, sBoundaryWeightValue, _
                                        sTotalValue, sAreaPercentageClosed)

            If (Me.m_cmbAreaClosed.FindStringExact(strPerc) = -1) Then
                Me.m_cmbAreaClosed.Items.Add(strPerc)
            End If

        End Sub

#End Region ' Progress

#Region " Results "

        Private Class cObjectiveResultComparer
            Implements IComparer(Of cObjectiveResult)

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Sorts objective results in DESCENDING order!
            ''' </summary>
            ''' <param name="x"></param>
            ''' <param name="y"></param>
            ''' <returns></returns>
            ''' ---------------------------------------------------------------
            Public Function Compare(ByVal x As EwECore.cObjectiveResult, _
                                    ByVal y As EwECore.cObjectiveResult) As Integer _
                                    Implements IComparer(Of EwECore.cObjectiveResult).Compare
                ' DESCENDING ORDER! < 1, = 0, > -1 (instead of customary ascending order < -1, = 0, > 1)
                If x.objFuncTotal > y.objFuncTotal Then Return -1
                If x.objFuncTotal < y.objFuncTotal Then Return 1
                Return 0
            End Function

        End Class

        Private Sub UpdateResultsGraph()

            Dim lResults As List(Of cObjectiveResult) = Nothing

            Select Case SearchType
                Case eMPAOptimizationModels.EcoSeed
                    ' Get all results
                    lResults = Me.m_manager.Results
                Case eMPAOptimizationModels.RandomSearch
                    ' Get results, filtered by selected percentage area closed
                    lResults = Me.FilteredResults(Me.m_manager.Results, Me.SelectedClosedPercentage)
                    ' Sort the results
                    lResults.Sort(New cObjectiveResultComparer())
                    ' Only show top percentile
                    If lResults.Count > 0 Then
                        ' Strip off anything past top x %
                        Dim iIndex As Integer = CInt(Math.Ceiling(lResults.Count * Me.SelectedBestPercentile / 100.0!))
                        lResults.RemoveRange(iIndex, lResults.Count - iIndex)
                    End If

            End Select

            Try
                ' Fill output graph
                For iResult As Integer = 0 To lResults.Count - 1
                    Dim result As cObjectiveResult = lResults(iResult)
                    Me.m_aptsResults(0).AddItem(result.objFuncTotal)
                    Me.m_aptsResults(1).AddItem(result.objFuncEconomicValue)
                    Me.m_aptsResults(2).AddItem(result.objFuncSocialValue)
                    Me.m_aptsResults(3).AddItem(result.objFuncMandatedValue)
                    Me.m_aptsResults(4).AddItem(result.objFuncEcologicalValue)
                    Me.m_aptsResults(5).AddItem(result.objBiomassDiversity)
                    Me.m_aptsResults(6).AddItem(result.objFuncAreaBorder)
                Next
                Me.m_graphResults.GraphPane.XAxis.Scale.Max = lResults.Count - 1

                Me.m_zghResults.CursorPos = 0.0
                Me.m_zghResults.RescaleAndRedraw()

            Catch ex As Exception

            End Try

        End Sub

        Private Sub ShowIteration(ByVal iIteration As Integer)

            Dim lResults As List(Of cObjectiveResult) = Nothing
            Dim res As cObjectiveResult = Nothing
            Dim cell As cMPACell = Nothing
            Dim aiCellMPA(Me.m_basemap.InRow, Me.m_basemap.InCol) As Integer

            ' Update map
            ReDim aiCellMPA(Me.m_basemap.InRow, Me.m_basemap.InCol)

            Select Case Me.SearchType

                Case eMPAOptimizationModels.EcoSeed
                    lResults = Me.m_manager.Results()

                Case eMPAOptimizationModels.RandomSearch
                    lResults = Me.FilteredResults(Me.m_manager.Results, Me.SelectedClosedPercentage())

            End Select

            ' Truncate iteration index
            iIteration = Math.Max(0, Math.Min(lResults.Count - 1, iIteration))
            ' Get results
            If (iIteration < lResults.Count) Then
                res = lResults(iIteration)
                For iCell As Integer = 0 To res.Cells.Count - 1
                    cell = res.Cells(iCell)
                    aiCellMPA(cell.Row, cell.Col) = cell.iMPA
                Next iCell
            End If

            Me.SetLayer(aiCellMPA, Me.m_basemap.LayerMPA(Me.SelectedMPA()), Me.SelectedMPA())

            ' Update indicators
            Me.m_gridResults.LogResult(res.objFuncEconomicValue, res.objFuncSocialValue, _
                                       res.objFuncMandatedValue, res.objFuncEcologicalValue, _
                                       res.objBiomassDiversity, res.objFuncAreaBorder, _
                                       res.objFuncTotal, res.PercentageClosed)

            Me.m_ucZoom.Map.Refresh()

        End Sub

        Private Sub ShowBestPercentage()
            Me.UpdateBestCountMap()
        End Sub

        Private Function FilteredResults(ByVal lIn As List(Of cObjectiveResult), _
                                         Optional ByVal iPercAreaClosed As Integer = -1) As List(Of cObjectiveResult)

            If iPercAreaClosed = -1 Then Return lIn
            Dim lOut As New List(Of cObjectiveResult)

            For iResult As Integer = 0 To lIn.Count - 1
                If lIn(iResult).PercentageClosed = iPercAreaClosed Then
                    lOut.Add(lIn(iResult))
                End If
            Next

            lOut.Sort(New cObjectiveResultComparer)

            Return lOut
        End Function

#End Region ' Results

#Region " Helper methods "

#Region " Map "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a layer to the map.
        ''' </summary>
        ''' <param name="l">Layer to add.</param>
        ''' <param name="strGroup">Group to add the layer to.</param>
        ''' <param name="layerPosition">Layer to position this layer before, if any.</param>
        ''' -------------------------------------------------------------------
        Private Sub AddLayer(ByVal l As cDisplayLayer, ByVal strGroup As String, strCommand As String, Optional ByVal layerPosition As cDisplayLayer = Nothing)
            Me.m_lLayers.Add(l)
            Me.m_ucZoom.Map.AddLayer(l, layerPosition)
            Me.m_ucLayers.AddLayer(l, strGroup, strCommand, layerPosition)
            AddHandler l.LayerChanged, AddressOf OnLayerChanged
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remmove a layer from the map.
        ''' </summary>
        ''' <param name="l">Layer to remove.</param>
        ''' -------------------------------------------------------------------
        Private Sub RemoveLayer(ByVal l As cDisplayLayer)
            Me.m_lLayers.Remove(l)
            Me.m_ucZoom.Map.RemoveLayer(l)
            Me.m_ucLayers.RemoveLayer(l)
            RemoveHandler l.LayerChanged, AddressOf OnLayerChanged
        End Sub

        Private Sub ShowLayerGroup(ByVal strGroup As String, ByVal bShowLayers As Boolean, ByVal bShowGroup As Boolean)
            Me.m_ucLayers.ShowGroup(strGroup, bShowLayers, bShowGroup)
        End Sub

        Private Sub EnableLayerGroup(ByVal strGroup As String, ByVal bEditable As Boolean)
            Me.m_ucLayers.EnableGroup(strGroup, bEditable)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Sets a layer to a grid of values.
        ''' </summary>
        ''' <param name="src">NxN array of integer to copy from.</param>
        ''' <param name="lDest">Layer to copy to.</param>
        ''' <param name="iConvertTo">Variable to convert non-negative values
        ''' to, or <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> to 
        ''' directly copy the values.</param>
        ''' -------------------------------------------------------------------
        Private Sub SetLayer(ByVal src As Integer(,), ByVal lDest As cEcospaceLayer, _
            Optional ByVal iConvertTo As Integer = cCore.NULL_VALUE)

            Dim iValue As Integer = 0
            ' For all rows
            For iRow As Integer = 1 To Me.m_basemap.InRow
                ' For all cols
                For iCol As Integer = 1 To Me.m_basemap.InCol
                    ' Get value
                    iValue = src(iRow, iCol)
                    ' Must convert?
                    If iConvertTo <> cCore.NULL_VALUE Then
                        ' #Yes: transmogrify non-zero values
                        iValue = if(iValue = 0, iValue, iConvertTo)
                    End If
                    ' Apply!
                    lDest.Cell(iRow, iCol) = iValue
                Next iCol
            Next iRow

            ' Invalidate min/max
            lDest.Invalidate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Sets a layer to a grid of values.
        ''' </summary>
        ''' <param name="src">NxN array of single to copy from.</param>
        ''' <param name="lDest">Layer to copy to.</param>
        ''' <param name="iConvertTo">Variable to convert non-negative values
        ''' to, or <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> to 
        ''' directly copy the values.</param>
        ''' -------------------------------------------------------------------
        Private Sub SetLayer(ByVal src As Single(,), ByVal lDest As cEcospaceLayer, _
            Optional ByVal iConvertTo As Integer = cCore.NULL_VALUE)

            Dim sValue As Single = 0
            ' For all rows
            For iRow As Integer = 1 To Me.m_basemap.InRow
                ' For all cols
                For iCol As Integer = 1 To Me.m_basemap.InCol
                    ' Get value
                    sValue = src(iRow, iCol)
                    ' Must convert?
                    If iConvertTo <> cCore.NULL_VALUE Then
                        ' #Yes: ognotrizarp non-zero values
                        sValue = CInt(if(sValue = 0, sValue, iConvertTo))
                    End If
                    ' Apply!
                    lDest.Cell(iRow, iCol) = sValue
                Next iCol
            Next iRow

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert 'iAreaPercentToClose' cells in the map to MPA 'iMPA'
        ''' </summary>
        ''' <param name="aiMap">The best count map to convert.</param>
        ''' <param name="iAreaPercentToClose">Percent of water cells 
        ''' to close in addition to the current MPAs.</param>
        ''' <param name="iMPA">The MPA to assign new cells to.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Cells are selected from the best count map, aiMap, by descending
        ''' value until either the requested percentage is met or there are no 
        ''' convertable cells left.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function ConvertToMPA(ByVal aiMap As Integer(,), _
                                      ByVal iAreaPercentToClose As Integer, _
                                      ByVal iMPA As Integer) As Boolean

            ' Ecospace MPA layer
            Dim layerMPA As cEcospaceLayer = Me.m_basemap.LayerMPA(iMPA)
            ' Ecospace depth layer
            Dim layerDepth As cEcospaceLayer = Me.m_basemap.LayerDepth
            ' Dictionary with list of points, sorted by hit count
            Dim dtMapSorted As New Dictionary(Of Integer, List(Of Point))
            ' List of hit count values, keys to the dictionary
            Dim lKeys As New List(Of Integer)
            ' Helper var to reference lists in the dictionary
            Dim lPoints As List(Of Point) = Nothing
            ' Number of cells that can be converted
            Dim iNumConvertableCells As Integer = 0
            ' Number of water cells
            Dim iNumWaterCells As Integer = 0
            ' Number of cells to close
            Dim iNumCellsToClose As Integer = 0
            ' Row, col iterators
            Dim iRow, iCol As Integer
            ' Always handy
            Dim iIndex As Integer = 0
            ' Randomizer
            Dim rnd As New Random()

            ' Gather conversion info
            For iRow = 1 To Me.m_basemap.InRow
                For iCol = 1 To Me.m_basemap.InCol

                    ' Only consider water cells
                    If (CSng(layerDepth.Cell(iRow, iCol)) > 0) Then

                        ' Clear existing target MPA cells
                        If (CInt(layerMPA.Cell(iRow, iCol)) = iMPA) Then
                            layerMPA.Cell(iRow, iCol) = 0
                        End If

                        ' Only consider cells that can be converted to MPA:
                        ' when MPA=0 (not currently part of an mpa)
                        If (CInt(layerMPA.Cell(iRow, iCol)) = 0) Then
                            ' Get hit count value for this cell
                            iIndex = aiMap(iRow, iCol)

                            ' Add it to the dictionary
                            If Not dtMapSorted.ContainsKey(iIndex) Then
                                ' #Yes: create point list and add it to dictionary
                                lPoints = New List(Of Point)
                                dtMapSorted(iIndex) = lPoints
                                lKeys.Add(iIndex)
                            Else
                                ' #No: get point list
                                lPoints = dtMapSorted(iIndex)
                            End If
                            ' Add point as candidate cell
                            lPoints.Add(New Point(iRow, iCol))

                            ' Count candidate cell
                            iNumConvertableCells += 1

                        End If ' Is not assigned to MPA yet

                        ' Count water cells
                        iNumWaterCells += 1

                    End If ' Is water cell
                Next iCol
            Next iRow

            ' Need to bail out?
            If (lKeys.Count = 0) Then Return True

            ' Calculate #cells to close
            iNumCellsToClose = CInt(Math.Ceiling(iNumWaterCells * iAreaPercentToClose / 100))
            ' Cap to the max amount of available #water cells
            iNumCellsToClose = Math.Min(iNumCellsToClose, iNumConvertableCells)

            ' Sort keys in reverse order (highest hit count value first)
            lKeys.Sort()
            lKeys.Reverse()

            ' Get first cell list to iterate over
            lPoints = dtMapSorted(lKeys(0))
            lKeys.RemoveAt(0)

            ' Can we go home now?
            While (iNumCellsToClose > 0)

                ' Convert a random cell from this list
                ' VC, JS 14nov08: Instead of randomizing cells when hit counts are identical,
                '                 cells could be selected based on total weighted score
                iIndex = (rnd.Next(lPoints.Count * 13) Mod lPoints.Count)
                layerMPA.Cell(lPoints(iIndex).X, lPoints(iIndex).Y) = iMPA
                lPoints.RemoveAt(iIndex)

                ' One less to close
                iNumCellsToClose -= 1

                ' Handle empty list
                If lPoints.Count = 0 Then
                    lPoints = dtMapSorted(lKeys(0))
                    lKeys.RemoveAt(0)
                End If

            End While

            Return True

        End Function

#End Region ' Map

#Region " Generic "

        Protected Overrides Sub UpdateControls()

            ' The %^@#$^#@$ check boxes throw events even before the form OnLoad has been called. Nice.
            ' Added sanity check to prevent premature control handling
            If (Me.m_manager Is Nothing) Then Return

            Dim bIsPreparing As Boolean = (Me.RunMode = eFormModeTypes.Prepare)
            Dim bIsRunning As Boolean = (Me.RunMode = eFormModeTypes.Searching Or Me.RunMode = eFormModeTypes.Initializing Or Me.RunMode = eFormModeTypes.Stopping)
            Dim bIsResults As Boolean = (Me.RunMode = eFormModeTypes.Results)
            Dim bIsEcoseed As Boolean = (Me.SearchType = eMPAOptimizationModels.EcoSeed)
            Dim bIsRandom As Boolean = (Me.SearchType = eMPAOptimizationModels.RandomSearch)
            Dim bMPALayerSelected As Boolean = (Me.SelectedMPA() > 0)
            Dim factory As New cLayerFactoryInternal()

            ' Update input controls
            Me.m_rbEcoseed.Enabled = (bIsPreparing)
            Me.m_rbRandom.Enabled = (bIsPreparing)
            Me.m_fpStartYear.Enabled = bIsPreparing
            Me.m_fpEndYear.Enabled = bIsPreparing
            Me.m_fpBaseYear.Enabled = bIsPreparing
            Me.m_fpMinArea.Enabled = (bIsPreparing And bIsRandom)
            Me.m_fpMaxArea.Enabled = (bIsPreparing And bIsRandom)
            Me.m_fpStepSize.Enabled = (bIsPreparing And bIsRandom)
            Me.m_fpDiscRate.Enabled = bIsPreparing
            Me.m_fpGenDiscRate.Enabled = bIsPreparing
            Me.m_fpIterations.Enabled = (bIsPreparing And bIsRandom)
            Me.m_fpMPA.Enabled = bIsPreparing

            Me.m_gridObjectives.Enabled = (bIsPreparing)
            Me.m_gridFleet.Enabled = (bIsPreparing)
            Me.m_gridGroup.Enabled = (bIsPreparing)

            ' Results
            Me.m_graphResults.Enabled = bIsResults
            Me.m_cmbAreaClosed.Enabled = (bIsResults And bIsRandom)
            Me.m_nudBestPercentile.Enabled = (bIsResults And bIsRandom)
            Me.m_btnResetMPAs.Enabled = bIsResults

            ' Update run control buttons
            Me.m_btnRun.Enabled = (bIsPreparing Or bIsResults) And Not bIsRunning
            Me.m_btnStop.Enabled = bIsRunning
            Me.m_btnConvertToMpa.Enabled = bIsResults
            Me.m_btnSave.Enabled = (bIsResults And bIsRandom)

            ' Toggle toolbar controls
            Me.m_tsbMPA.Enabled = bIsPreparing And bMPALayerSelected
            Me.m_tsbSeed.Enabled = bIsPreparing And bMPALayerSelected And bIsEcoseed
            Me.m_tsbEditLayers.Enabled = bIsPreparing And bIsRandom

            ' Layers enabled state
            Me.EnableLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerDepth), Not bIsRunning)
            Me.EnableLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerMPA), Not bIsRunning)
            Me.EnableLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerHabitat), Not bIsRunning)
            Me.EnableLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerMPASeed), Not bIsRunning)
            Me.EnableLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerMPARandom), Not bIsRunning)
            Me.EnableLayerGroup(factory.GetLayerGroup(eVarNameFlags.LayerImportance), Not bIsRunning)

            ' Update map
            Me.m_ucZoom.Map.Editable = bIsPreparing

            Me.m_cbAutoSave.Checked = Me.Core.Autosave(eAutosaveTypes.MPAOpt)

        End Sub

        Private Function ValidateInputs() As Boolean

            Dim source As cCoreInputOutputBase = Me.m_manager.ValueWeights
            Dim bOk As Boolean = True

            ' Check MPA selection
            If Me.m_cmbMPA.SelectedIndex = -1 Then
                Me.UIContext.Core.Messages.SendMessage(New cMessage(My.Resources.PROMPT_MPAOPT_SELECTION, _
                                                                    eMessageType.Any, eCoreComponentType.MPAOptimization, _
                                                                    eMessageImportance.Warning))
                Return False
            End If

            ' Check mandated rebuilding
            If CSng(source.GetVariable(eVarNameFlags.FPSMandatedRebuildingWeight)) > 0.0 Then
                bOk = False
                For iGroup As Integer = 1 To Me.UIContext.Core.nGroups
                    source = Me.m_manager.GroupObjectives(iGroup)
                    bOk = bOk Or (CSng(source.GetVariable(eVarNameFlags.FPSGroupMandRelBiom)) > 0.0)
                Next
                If bOk = False Then
                    Me.UIContext.Core.Messages.SendMessage(New cMessage(My.Resources.PROMPT_MPAOPT_MANDATEDB, _
                                                                        eMessageType.Any, eCoreComponentType.MPAOptimization, _
                                                                        eMessageImportance.Warning))
                    Return False
                End If
            End If

            Return True

        End Function

        Private Sub ClearResults()

            For Each rp As ResultPoints In Me.m_aptsProgress
                If rp IsNot Nothing Then rp.Clear()
            Next
            For Each rp As ResultPoints In Me.m_aptsResults
                If rp IsNot Nothing Then rp.Clear()
            Next

            Me.m_graphProgress.Refresh()
            Me.m_graphResults.Refresh()

            Me.m_cmbAreaClosed.Items.Clear()
        End Sub

#End Region ' Generic

#End Region ' Helper methods

#End Region ' Internals

    End Class

End Namespace