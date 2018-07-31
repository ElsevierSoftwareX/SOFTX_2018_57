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

Imports System.Text
Imports EwECore
Imports EwECore.Ecosim
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ZedGraph
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    ''' <summary>
    ''' Form, implementing the Ecosim group output plots interface.
    ''' </summary>
    Public Class frmEcosimOutputGroupPlots

#Region " Private helper class "

        ''' <summary>
        ''' Helper class to convert plot types to readable names
        ''' </summary>
        Private Class cSimPlotFormatter
            Implements ITypeFormatter

            Public Function GetDescribedType() As System.Type Implements ITypeFormatter.GetDescribedType
                Return GetType(ePlot)
            End Function

            Public Function GetDescriptor(ByVal value As Object, Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String Implements ITypeFormatter.GetDescriptor

                Select Case DirectCast(value, ePlot)
                    Case ePlot.AvgWeightOrProdCons : Return SharedResources.HEADER_PRODCONS
                    Case ePlot.Biomass : Return SharedResources.HEADER_BIOMASS
                    Case ePlot.ConsumptionBiomass : Return SharedResources.HEADER_CONSUMPTION_OVER_BIOMASS
                    Case ePlot.FeedingTime : Return SharedResources.HEADER_FEEDINGTIME
                    Case ePlot.FleetFishingMortality : Return SharedResources.HEADER_FISHINGMORTALITY
                    Case ePlot.Mortality : Return My.Resources.ECOSIM_PLOT_CAPTION_MORT_CONS
                    Case ePlot.PredationMortality : Return SharedResources.HEADER_PREDMORT
                    Case ePlot.Prey : Return SharedResources.HEADER_PREY_PERCENTAGE
                    Case ePlot.Value : Return SharedResources.HEADER_VALUE
                    Case ePlot.[Catch] : Return SharedResources.HEADER_CATCH
                    Case ePlot.Discards : Return SharedResources.HEADER_TOTALDISCARDS
                    Case ePlot.DiscardsMortality : Return SharedResources.HEADER_DISCARD_MORT
                    Case ePlot.DiscardsSurvival : Return SharedResources.HEADER_DISCARD_SURV
                    Case ePlot.Landings : Return SharedResources.HEADER_LANDINGS
                End Select
                Return ""

            End Function

        End Class

#End Region ' Private helper class

#Region " Variables "

        Private m_parms As cEcoSimModelParameters
        Private m_paneMaster As MasterPane = Nothing
        Private m_zgh As cZedGraphHelper = Nothing
        Private m_plotPanel([Enum].GetValues(GetType(ePlot)).Length) As Integer
        Private m_plotVisible([Enum].GetValues(GetType(ePlot)).Length) As Boolean
        Private m_bContainsAggregatedFleet As Boolean = False

        Dim m_TSInterval As eTSDataSetInterval

        Private Enum ePlot As Integer
            Biomass
            ConsumptionBiomass
            PredationMortality
            Mortality
            FeedingTime
            Prey
            AvgWeightOrProdCons
            [Catch]
            FleetFishingMortality
            Discards
            DiscardsMortality
            DiscardsSurvival
            Landings
            Value
        End Enum

#End Region ' Variables

#Region " Constructors "

        Public Sub New()
            Me.InitializeComponent()

            ' Provide default plot visibility settings. This is done in the 
            ' constructor to ensure that defaults do not override possible 
            ' alternate values provided in the 'Settings' property.

            ' Defaults: only hide 'value' pane by default
            For Each plot As ePlot In [Enum].GetValues(GetType(ePlot))
                Me.m_plotVisible(plot) = True
            Next
            Me.m_plotVisible(ePlot.Value) = False

        End Sub

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)
                If (MyBase.UIContext IsNot Nothing) Then
                    Me.m_lbGroups.Detach()
                    Me.m_lbPredators.Detach()
                    Me.m_lbPrey.Detach()
                    Me.m_lbFleets.Detach()
                End If
                MyBase.UIContext = value
                If (MyBase.UIContext IsNot Nothing) Then
                    Me.m_lbGroups.Attach(Me.UIContext)
                    Me.m_lbPredators.Attach(Me.UIContext)
                    Me.m_lbPrey.Attach(Me.UIContext)
                    Me.m_lbFleets.Attach(Me.UIContext)
                End If
            End Set
        End Property

#End Region ' Constructors

#Region " Event handlers "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim group As cCoreGroupBase = Nothing

            Me.m_parms = Me.Core.EcoSimModelParameters()
            Me.m_paneMaster = Me.m_graph.MasterPane

            Me.m_zgh = New cZedGraphHelper()
            Me.ConfigurePlots(True)
            Me.m_zgh.ShowPointValue = True
            Me.m_zgh.IsTrackVisiblity = False

            Me.UpdateColors()
            Me.AddCurves()

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.TimeSeries}
            Me.m_lbGroups.SelectedIndex = 0

            If Me.Core.ActiveTimeSeriesDatasetIndex > 0 Then
                Me.m_TSInterval = Me.Core.TimeSeriesDataset(Me.Core.ActiveTimeSeriesDatasetIndex).TimeSeriesInterval
            Else
                Me.m_TSInterval = eTSDataSetInterval.Annual
            End If

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            Me.ConfigurePlots(False)

            Me.CoreComponents = Nothing

            Me.m_paneMaster = Nothing
            Me.m_zgh = Nothing

            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
            If ((changeType And cStyleGuide.eChangeType.Colours) = cStyleGuide.eChangeType.Colours) Then
                Me.UpdateColors()
            End If
        End Sub

        ''' <summary>
        ''' Event hander to display results for another group
        ''' </summary>
        Private Sub OnGroupSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_lbGroups.SelectedIndexChanged

            Me.UpdatePlots()

        End Sub

        Private Sub OnSaveData(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnSaveData.Click

            Dim cmd As cEcosimSaveDataCommand = DirectCast(Me.CommandHandler.GetCommand("ExportEcosimResultsToCSV"), cEcosimSaveDataCommand)
            Dim aResults As New List(Of cEcosimResultWriter.eResultTypes)

            ' ToDo: include new mortality outputs here

            If Me.m_cbSaveVisibleOnly.Checked Then
                For Each plot As ePlot In [Enum].GetValues(GetType(ePlot))
                    If Me.m_plotVisible(plot) Then
                        Select Case plot
                            Case ePlot.AvgWeightOrProdCons
                                aResults.Add(cEcosimResultWriter.eResultTypes.AvgWeightOrProdCons)
                            Case ePlot.Biomass
                                aResults.Add(cEcosimResultWriter.eResultTypes.Biomass)
                            Case ePlot.Catch
                                aResults.Add(cEcosimResultWriter.eResultTypes.Catch)
                            Case ePlot.ConsumptionBiomass
                                aResults.Add(cEcosimResultWriter.eResultTypes.ConsumptionBiomass)
                            Case ePlot.FeedingTime
                                aResults.Add(cEcosimResultWriter.eResultTypes.FeedingTime)
                            Case ePlot.FleetFishingMortality
                                aResults.Add(cEcosimResultWriter.eResultTypes.Mortality)
                            Case ePlot.Mortality
                                aResults.Add(cEcosimResultWriter.eResultTypes.Mortality)
                            Case ePlot.PredationMortality
                                aResults.Add(cEcosimResultWriter.eResultTypes.PredationMortality)
                            Case ePlot.Prey
                                aResults.Add(cEcosimResultWriter.eResultTypes.Prey)
                            Case ePlot.Value
                                aResults.Add(cEcosimResultWriter.eResultTypes.Value)
                            Case ePlot.Discards
                                aResults.Add(cEcosimResultWriter.eResultTypes.DiscardFleetGroup)
                            Case ePlot.DiscardsMortality
                                aResults.Add(cEcosimResultWriter.eResultTypes.DiscardMortalityFleetGroup)
                            Case ePlot.DiscardsSurvival
                                aResults.Add(cEcosimResultWriter.eResultTypes.DiscardSurvivalFleetGroup)
                            Case ePlot.Landings
                                aResults.Add(cEcosimResultWriter.eResultTypes.Landings)
                            Case Else
                                Debug.Assert(False, "Plot type not translated")
                        End Select
                    End If
                Next
            End If

            cmd.Invoke(aResults.ToArray)

        End Sub

        Private Sub OnShowHidePlots(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnChoosePlots.Click

            Dim dlg As New dlgSelectItems(GetType(ePlot), New cSimPlotFormatter())

            Dim lSelected As New List(Of Integer)
            For Each plot As ePlot In [Enum].GetValues(GetType(ePlot))
                If (Me.m_plotVisible(plot)) Then lSelected.Add(plot)
            Next

            If dlg.ShowDialog(Me, lSelected.ToArray) = System.Windows.Forms.DialogResult.OK Then
                For Each plot As ePlot In [Enum].GetValues(GetType(ePlot))
                    Try
                        Me.m_plotVisible(CInt(plot)) = (Array.IndexOf(dlg.Selection, CInt(plot)) >= 0)
                    Catch ex As Exception

                    End Try
                Next
            End If
            Me.ConfigurePlots()
            Me.UpdatePlots()

        End Sub

#End Region ' Event handlers

#Region " Helper methods "

        Public Overrides Property Settings() As String
            Get
                Dim sbSettings As New StringBuilder()
                Dim iNumPlots As Integer = [Enum].GetValues(GetType(ePlot)).Length
                For iPlot As Integer = 0 To iNumPlots - 1
                    sbSettings.Append(If(Me.m_plotVisible(DirectCast(iPlot, ePlot)), "1", "0"))
                Next
                Return sbSettings.ToString()
            End Get
            Set(ByVal strSettings As String)
                If String.IsNullOrEmpty(strSettings) Then Return

                Dim iNumPlots As Integer = Math.Min([Enum].GetValues(GetType(ePlot)).Length, strSettings.Length)
                For iPlot As Integer = 0 To iNumPlots - 1
                    Me.m_plotVisible(DirectCast(iPlot, ePlot)) = (strSettings.Substring(iPlot, 1) = "1"c)
                Next
            End Set
        End Property

        Protected Sub ConfigurePlots(Optional ByVal bFormOpen As Boolean = True)

            Dim iPane As Integer = 1
            Dim iMaxPanes As Integer = [Enum].GetValues(GetType(ePlot)).Length - 1

            ' Determine where panes will be placed
            For Each plot As ePlot In [Enum].GetValues(GetType(ePlot))
                If Me.m_plotVisible(plot) Then
                    Me.m_plotPanel(plot) = iPane
                    iPane += 1
                Else
                    Me.m_plotPanel(plot) = cCore.NULL_VALUE
                End If
            Next plot

            If Me.m_zgh.IsAttached Then
                Me.m_zgh.Detach()
            End If

            If Not bFormOpen Then Return

            Me.m_zgh.Attach(Me.UIContext, Me.m_graph, iPane - 1)

            For Each data As ePlot In [Enum].GetValues(GetType(ePlot))
                Dim strTitle As String = Me.GetPlotTitle(data)
                Dim strYAaxisLabel As String = Me.GetPlotYAxisLabel(data)
                Dim dAxisMax As Double = Me.GetPlotAxisMax(data)
                Me.ConfigurePane(data, strTitle, strYAaxisLabel, dAxisMax)
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure a plot on the main graph
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ConfigurePane(ByVal plot As ePlot, ByVal strTitle As String, strYAxisLabel As String, Optional ByVal dYAxisMax As Double = 0)

            If Not Me.m_plotVisible(plot) Then Return
            ' Sanity check
            Debug.Assert(Me.m_plotPanel(plot) > 0)
            ' Configure pane
            Me.m_zgh.ConfigurePane(strTitle,
                       SharedResources.HEADER_TIME,
                       CDbl(Me.Core.EcosimFirstYear),
                       CDbl(Me.Core.EcosimFirstYear + (Me.Core.nEcosimTimeSteps / cCore.N_MONTHS)),
                       strYAxisLabel, 0, dYAxisMax,
                       False, LegendPos.TopCenter, Me.m_plotPanel(plot))

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, creates a ready-to-eat list of PointPairList instances.
        ''' </summary>
        ''' <param name="iNumLists">Number of lists to create.</param>
        ''' -------------------------------------------------------------------
        Private Function InitLists(ByVal iNumLists As Integer) As List(Of PointPairList)

            Dim lPPL As New List(Of PointPairList)
            For i As Integer = 1 To iNumLists
                lPPL.Add(New PointPairList())
            Next
            Return lPPL

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the values from core and add them into graph
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub AddCurves()

            Dim iCount As Integer = 0
            Dim dXValue As Double = 0
            Dim iGroup As Integer = Math.Max(1, Me.m_lbGroups.SelectedGroupIndex)
            Dim groupSimOut As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)
            Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)

            Dim pplB As New PointPairList()
            Dim pplConsB As New PointPairList()
            Dim pplFeedTime As New PointPairList()
            Dim pplYield As New PointPairList()
            Dim pplAvgWorProdCons As New PointPairList()

            Dim pplMortTotal As New PointPairList()
            Dim pplMortPredation As New PointPairList()
            Dim pplMortFishing As New PointPairList()

            'Set the master pane title
            Me.m_zgh.Configure(groupSimOut.Name)
            Me.m_bContainsAggregatedFleet = groupSimOut.isCatchAggregated()

            ' Clear all panes
            For Each pane As GraphPane In Me.m_graph.MasterPane.PaneList
                pane.CurveList.Clear()
                pane.AxisChange()
            Next

            ' Do not render when sim has not ran
            If Not Me.Core.StateMonitor.HasEcosimRan Then Return

            Dim applCatchFleet(Me.Core.nFleets) As PointPairList
            Dim applFishMortFleet(Me.Core.nFleets) As PointPairList
            Dim applValueFleet(Me.Core.nFleets) As PointPairList
            Dim applDiscards(Me.Core.nFleets) As PointPairList
            Dim applDiscardsMortality(Me.Core.nFleets) As PointPairList
            Dim applDiscardsSurvival(Me.Core.nFleets) As PointPairList
            Dim applLandings(Me.Core.nFleets) As PointPairList

            For i As Integer = 0 To Me.Core.nFleets
                applCatchFleet(i) = New PointPairList()
                applFishMortFleet(i) = New PointPairList()
                applValueFleet(i) = New PointPairList()
                applDiscards(i) = New PointPairList()
                applDiscardsMortality(i) = New PointPairList()
                applDiscardsSurvival(i) = New PointPairList()
                applLandings(i) = New PointPairList()
            Next

            For i As Integer = 1 To Me.Core.nEcosimTimeSteps
                ' Time
                dXValue = Me.Core.EcosimFirstYear + (i / cCore.N_MONTHS)
                ' Get sim results
                pplB.Add(dXValue, groupSimOut.Biomass(i))
                pplConsB.Add(dXValue, groupSimOut.ConsumpBiomass(i))
                pplFeedTime.Add(dXValue, groupSimOut.FeedingTime(i))
                pplYield.Add(dXValue, groupSimOut.Catch(i))
                pplMortTotal.Add(dXValue, groupSimOut.TotalMort(i))
                pplMortPredation.Add(dXValue, groupSimOut.PredMort(i))
                pplMortFishing.Add(dXValue, groupSimOut.FishMort(i))

                ' Special case: is catch aggregated?
                If Me.m_bContainsAggregatedFleet Then
                    ' Report F from fleet 1 for all fleets only
                    applFishMortFleet(0).Add(dXValue, groupSimOut.FishingMortByFleet(0, i))
                    applCatchFleet(0).Add(dXValue, groupSimOut.CatchByFleet(0, i))
                    applValueFleet(0).Add(dXValue, groupSimOut.ValueByFleet(0, i))
                    applDiscards(0).Add(dXValue, groupSimOut.DiscardByFleet(0, i))
                    applDiscardsMortality(0).Add(dXValue, groupSimOut.DiscardMortByFleet(0, i))
                    applDiscardsSurvival(0).Add(dXValue, groupSimOut.DiscardSurvivedByFleet(0, i))
                    applLandings(0).Add(dXValue, groupSimOut.LandingsByFleet(0, i))
                Else
                    For iFleet As Integer = 1 To Me.Core.nFleets
                        applFishMortFleet(iFleet).Add(dXValue, groupSimOut.FishingMortByFleet(iFleet, i))
                        applCatchFleet(iFleet).Add(dXValue, groupSimOut.CatchByFleet(iFleet, i))
                        applValueFleet(iFleet).Add(dXValue, groupSimOut.ValueByFleet(iFleet, i))
                        applDiscards(iFleet).Add(dXValue, groupSimOut.DiscardByFleet(iFleet, i))
                        applDiscardsMortality(iFleet).Add(dXValue, groupSimOut.DiscardMortByFleet(iFleet, i))
                        applDiscardsSurvival(iFleet).Add(dXValue, groupSimOut.DiscardSurvivedByFleet(iFleet, i))
                        applLandings(iFleet).Add(dXValue, groupSimOut.LandingsByFleet(iFleet, i))
                    Next
                End If

                ' Special case: is multi-stanza?
                If groupSimOut.IsMultiStanza() Then
                    pplAvgWorProdCons.Add(dXValue, groupSimOut.AvgWeight(i))
                Else
                    pplAvgWorProdCons.Add(dXValue, groupSimOut.ProdConsump(i))
                End If

            Next

            Me.AddCurveToGraphPane(ePlot.Biomass, Me.m_zgh.CreateLineItem(group, pplB))
            For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.BiomassRel, iGroup, Color.Blue)
                Me.AddCurveToGraphPane(ePlot.Biomass, li)
            Next li
            ' Fixes issue 604:
            For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.BiomassAbs, iGroup, Color.Green)
                Me.AddCurveToGraphPane(ePlot.Biomass, li)
            Next li

            Me.AddCurveToGraphPane(ePlot.ConsumptionBiomass, Me.m_zgh.CreateLineItem(group, pplConsB))
            Me.AddCurveToGraphPane(ePlot.FeedingTime, Me.m_zgh.CreateLineItem(group, pplFeedTime))

            If Me.m_bContainsAggregatedFleet Then
                Dim strAll As String = SharedResources.GENERIC_VALUE_ALL
                Dim clrAll As Color = Me.StyleGuide.FleetColorDefault(0, 1)
                Me.AddCurveToGraphPane(ePlot.FleetFishingMortality, Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applFishMortFleet(0)))
                Me.AddCurveToGraphPane(ePlot.[Catch], Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applCatchFleet(0)), True)
                Me.AddCurveToGraphPane(ePlot.Value, Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applValueFleet(0)), True)
                Me.AddCurveToGraphPane(ePlot.Discards, Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applDiscards(0)), True)
                Me.AddCurveToGraphPane(ePlot.DiscardsMortality, Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applDiscardsMortality(0)), True)
                Me.AddCurveToGraphPane(ePlot.DiscardsSurvival, Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applDiscardsSurvival(0)), True)
                Me.AddCurveToGraphPane(ePlot.Landings, Me.m_zgh.CreateLineItem(strAll, eSketchDrawModeTypes.Line, clrAll, applLandings(0)), True)

                For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.FishingMortality, iGroup, Color.Black)
                    Me.AddCurveToGraphPane(ePlot.FleetFishingMortality, li)
                Next li

            Else
                For i As Integer = 1 To Me.Core.nFleets

                    Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(i)
                    Dim clr As Color = Me.UIContext.StyleGuide.FleetColor(Me.Core, i)
                    If fleet.Landings(iGroup) > 0 Then
                        Me.AddCurveToGraphPane(ePlot.[Catch], Me.m_zgh.CreateLineItem(fleet, applCatchFleet(i)), True)
                        Me.AddCurveToGraphPane(ePlot.Value, Me.m_zgh.CreateLineItem(fleet, applValueFleet(i)), True)
                        Me.AddCurveToGraphPane(ePlot.Landings, Me.m_zgh.CreateLineItem(fleet, applLandings(i)), True)
                    End If
                    If fleet.Discards(iGroup) > 0 Then
                        Me.AddCurveToGraphPane(ePlot.Discards, Me.m_zgh.CreateLineItem(fleet, applDiscards(i)), True)
                        Me.AddCurveToGraphPane(ePlot.DiscardsMortality, Me.m_zgh.CreateLineItem(fleet, applDiscardsMortality(i)), True)
                        Me.AddCurveToGraphPane(ePlot.DiscardsSurvival, Me.m_zgh.CreateLineItem(fleet, applDiscardsSurvival(i)), True)
                    End If
                    Me.AddCurveToGraphPane(ePlot.FleetFishingMortality, Me.m_zgh.CreateLineItem(fleet, applFishMortFleet(i)), True)
                Next

            End If

            For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.Catches, iGroup, Color.Red)
                Me.AddCurveToGraphPane(ePlot.[Catch], li, True)
            Next li
            For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.CatchesForcing, iGroup, Color.Blue)
                Me.AddCurveToGraphPane(ePlot.[Catch], li, True)
            Next li
            For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.CatchesRel, iGroup, Color.LightBlue)
                Me.AddCurveToGraphPane(ePlot.[Catch], li, True)
            Next li

            If groupSimOut.IsMultiStanza() Then

                Me.UpdateGraphPaneTitle(ePlot.AvgWeightOrProdCons, SharedResources.HEADER_AVGERAGEWEIGHT)

                Me.AddCurveToGraphPane(ePlot.AvgWeightOrProdCons, Me.m_zgh.CreateLineItem(group, pplAvgWorProdCons))
                For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.AverageWeight, iGroup, Color.Blue)
                    Me.AddCurveToGraphPane(ePlot.AvgWeightOrProdCons, li)
                Next li

            Else

                Me.UpdateGraphPaneTitle(ePlot.AvgWeightOrProdCons, SharedResources.HEADER_PRODCONS)
                Me.AddCurveToGraphPane(ePlot.AvgWeightOrProdCons, Me.m_zgh.CreateLineItem(group, pplAvgWorProdCons))

            End If

            Me.AddCurveToGraphPane(ePlot.Mortality, Me.m_zgh.CreateLineItem(SharedResources.HEADER_TOTAL, eSketchDrawModeTypes.Line, Color.Black, pplMortTotal))
            Me.AddCurveToGraphPane(ePlot.Mortality, Me.m_zgh.CreateLineItem(SharedResources.HEADER_PREDATION, eSketchDrawModeTypes.Line, Color.Red, pplMortPredation))
            Me.AddCurveToGraphPane(ePlot.Mortality, Me.m_zgh.CreateLineItem(SharedResources.HEADER_FISHING, eSketchDrawModeTypes.Line, Color.Blue, pplMortFishing))
            For Each li As LineItem In Me.GetTimeSeriesLineItems(eTimeSeriesType.TotalMortality, iGroup, Color.Green)
                Me.AddCurveToGraphPane(ePlot.Mortality, li)
            Next li

            'VC 07apr09: F values (type = 4) should not be plotted as they are used to drive the model
            'For Each ppl As PointPairList In Me.GetTSData(eTimeSeriesType.FishingMortality)
            '    Me.AddCurveToGraphPane(ePaneTypes.Mortality, Me.m_zgh.CreateLineItem("Fishing", ZedGraphHelper.eCurveTypes.TimeSeries, Color.Red, ppl), False)
            'Next ppl

            'Predation mortality 
            iCount = 0
            For i As Integer = 1 To Me.Core.nLivingGroups
                If group.IsPred(i) Then
                    Dim ppl As New PointPairList
                    Dim pred As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
                    For j As Integer = 1 To Me.Core.nEcosimTimeSteps
                        dXValue = Me.Core.EcosimFirstYear + (j / cCore.N_MONTHS)
                        ppl.Add(dXValue, groupSimOut.Predation(i, j))
                    Next
                    Me.AddCurveToGraphPane(ePlot.PredationMortality, Me.m_zgh.CreateLineItem(pred, ppl))
                    iCount += 1
                End If
            Next

            'Prey %
            iCount = 0
            For i As Integer = 1 To Me.Core.nLivingGroups
                If group.IsPrey(i) Then
                    Dim ppl As New PointPairList
                    Dim prey As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
                    For j As Integer = 1 To Me.Core.nEcosimTimeSteps
                        dXValue = Me.Core.EcosimFirstYear + (j / cCore.N_MONTHS)
                        ppl.Add(dXValue, groupSimOut.PreyPercentage(i, j) * 100)
                    Next
                    Me.AddCurveToGraphPane(ePlot.Prey, Me.m_zgh.CreateLineItem(prey, ppl))
                    iCount += 1
                End If
            Next

        End Sub

#Region " Time series "

        Private Function GetTimeSeriesLineItems(ByVal TSType As eTimeSeriesType, ByVal iGroup As Integer, ByVal clr As Color) As List(Of LineItem)

            Dim lli As New List(Of LineItem)
            Dim ppt As PointPairList = Nothing
            Dim ts As cTimeSeries = Nothing
            Dim gts As cGroupTimeSeries = Nothing
            Dim iNumLine As Integer = 0
            Dim iMaxLines As Integer = 1

            ' First count #TS (for colouring)
            For i As Integer = 1 To Me.Core.nTimeSeries
                ts = Me.Core.EcosimTimeSeries(i)
                If ts.TimeSeriesType = TSType Then
                    If TypeOf ts Is cGroupTimeSeries Then
                        gts = DirectCast(ts, cGroupTimeSeries)
                        If (gts.GroupIndex = iGroup) And gts.Enabled() Then
                            iMaxLines += 1
                        End If
                    End If
                End If
            Next

            ' Build lines
            For i As Integer = 1 To Me.Core.nTimeSeries
                ts = Me.Core.EcosimTimeSeries(i)
                If ts.TimeSeriesType = TSType Then
                    If TypeOf ts Is cGroupTimeSeries Then
                        gts = DirectCast(ts, cGroupTimeSeries)
                        If (gts.GroupIndex = iGroup) And gts.Enabled() Then
                            lli.Add(Me.ToTimeSeriesLineItem(gts, cColorUtils.GetVariant(clr, CSng(iNumLine / iMaxLines))))
                            iNumLine += 1
                        End If
                    End If
                End If
            Next
            Return lli

        End Function

        Private Function ToTimeSeriesLineItem(ByVal gts As cGroupTimeSeries, ByVal clr As Color) As LineItem

            Dim ppt As New PointPairList
            Dim dScale As Single = 1.0F
            Dim li As LineItem = Nothing
            Dim xpos As Double = 0.0
            Dim deltaT As Double = 1 / cCore.N_MONTHS
            Dim da() As Single = gts.ShapeData()
            Dim iYear As Integer = Me.Core.EcosimFirstYear
            Dim h As New cTimeSeriesShapeGUIHandler(Me.UIContext)

            If (gts.TimeSeriesType = eTimeSeriesType.BiomassRel) Or (gts.TimeSeriesType = eTimeSeriesType.AverageWeight) Or (gts.TimeSeriesType = eTimeSeriesType.CatchesRel) Then
                'VC091209: totalmortality is absolute, not relative
                If gts.eDataQ > 0 Then dScale = 1.0F / gts.eDataQ
            End If

            'Just in case...
            Debug.Assert(Me.m_TSInterval = eTSDataSetInterval.Annual Or Me.m_TSInterval = eTSDataSetInterval.TimeStep, "Plotting Ecosim Output unknown timeseries interval.")

            For j As Integer = 1 To da.Length - 1
                If (da(j) > 0) Then
                    Select Case Me.m_TSInterval
                        Case eTSDataSetInterval.TimeStep
                            xpos = iYear + j * deltaT - deltaT * 0.5
                        Case eTSDataSetInterval.Annual
                            xpos = iYear + j - 0.5
                    End Select
                    ppt.Add(xpos, da(j) * dScale)
                End If
            Next
            Return Me.m_zgh.CreateLineItem(gts.Name, h.SketchDrawMode(gts), clr, ppt)

        End Function

#End Region ' Time series

        Private Sub UpdatePlots()
            Try
                Me.AddCurves()
                Me.m_zgh.RescaleAndRedraw()
                Me.ShowGroup()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub ShowGroup()

            Dim iGroup As Integer = m_lbGroups.SelectedIndex + 1
            Dim grp As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
            Dim grpOutput As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

            Dim lAvgPredConsumption As New List(Of Single)
            Dim lAvgPredIndex As New List(Of Integer)

            Dim lAvgPreyConsumption As New List(Of Single)
            Dim lAvgPreyIndex As New List(Of Integer)

            Dim lCatch As New List(Of Single)
            Dim lFleetIndex As New List(Of Integer)

            For i As Integer = 1 To Me.Core.nLivingGroups

                If grp.IsPred(i) Then
                    lAvgPredConsumption.Add(grpOutput.AvgPredConsumption(i))
                    lAvgPredIndex.Add(i)
                End If

                If grp.IsPrey(i) Then
                    lAvgPreyConsumption.Add(grpOutput.AvgPreyConsumption(i))
                    lAvgPreyIndex.Add(i)
                End If

            Next

            Me.PopulateGroupListBox(Me.m_lbPredators, lAvgPredIndex.ToArray(), lAvgPredConsumption.ToArray())
            Me.PopulateGroupListBox(Me.m_lbPrey, lAvgPreyIndex.ToArray(), lAvgPreyConsumption.ToArray())

            ' Are fleet values aggregated?
            If Me.m_bContainsAggregatedFleet Then
                ' #Yes: show only 'All fleets' item
                Me.m_lbFleets.ShowAllFleetsItem = True
            Else
                ' #No: Show all relevant fleets, sorted by landings
                For i As Integer = 1 To Me.Core.nFleets
                    If Me.Core.EcopathFleetInputs(i).Landings(iGroup) > 0 Then
                        Dim sCatch As Single = 0
                        For j As Integer = 1 To Me.Core.nEcosimTimeSteps
                            sCatch += grpOutput.CatchByFleet(i, j)
                        Next
                        lCatch.Add(sCatch)
                        lFleetIndex.Add(i)
                    End If
                Next
                Me.m_lbFleets.ShowAllFleetsItem = False
            End If
            Me.PopulateFleetListBox(Me.m_lbFleets, lFleetIndex.ToArray(), lCatch.ToArray())

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Populate a group list box with Ecopath groups.
        ''' </summary>
        ''' <param name="l">Listbox to add items to.</param>
        ''' <param name="aiGroupIndex">Array of group index values.</param>
        ''' -------------------------------------------------------------------
        Private Sub PopulateGroupListBox(ByVal l As cGroupListBox,
                                         ByVal aiGroupIndex() As Integer,
                                         ByVal asValues() As Single)

            l.Populate(aiGroupIndex)
            For i As Integer = 0 To aiGroupIndex.Count - 1
                l.SortValue(aiGroupIndex(i)) = asValues(i)
            Next
            l.SortType = cGroupListBox.eSortType.ValueDesc
            l.Refresh()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Populate a fleet list box with Ecopath fleets.
        ''' </summary>
        ''' <param name="l">Listbox to add items to.</param>
        ''' <param name="aiFleetIndex">Array of fleet index values.</param>
        ''' -------------------------------------------------------------------
        Private Sub PopulateFleetListBox(ByVal l As cFleetListBox,
                                         ByVal aiFleetIndex() As Integer,
                                         ByVal asValues() As Single)

            l.Populate(aiFleetIndex)
            For i As Integer = 0 To aiFleetIndex.Count - 1
                l.SortValue(i) = asValues(i)
            Next
            l.SortType = cFleetListBox.eSortType.ValueDesc
            l.Refresh()

        End Sub

        Private Sub UpdateGraphPaneTitle(ByVal paneType As ePlot, ByVal strTitle As String)
            If Not Me.m_plotVisible(paneType) Then Return
            ' Sanity check
            Debug.Assert(Me.m_plotPanel(paneType) > 0)

            Try
                Dim gp As GraphPane = Me.m_zgh.GetPane(Me.m_plotPanel(paneType))
                gp.Title.Text = strTitle
            Catch ex As Exception

            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add one curve into the graph pane
        ''' </summary>
        ''' <param name="paneType">Index of the graph pane</param>
        ''' <param name="li">The curve</param>
        ''' -------------------------------------------------------------------
        Private Sub AddCurveToGraphPane(ByVal paneType As ePlot,
                                        ByVal li As LineItem,
                                        Optional ByVal bCumulative As Boolean = False)
            Dim lli As New List(Of ZedGraph.LineItem)
            lli.Add(li)
            Me.AddCurvesToGraphPane(paneType, lli, bCumulative)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add multiple curves into the graph pane
        ''' </summary>
        ''' <param name="paneType">The idnex of the graph pane</param>
        ''' <param name="lli">The lists of data points for the multiple curves</param>
        ''' <remarks>Overloaded method with different color options.</remarks>
        ''' -------------------------------------------------------------------
        Private Sub AddCurvesToGraphPane(ByVal paneType As ePlot,
                                         ByVal lli As List(Of LineItem),
                                         Optional ByVal bCumulative As Boolean = False)

            If Not Me.m_plotVisible(paneType) Then Return
            ' Sanity check
            Debug.Assert(Me.m_plotPanel(paneType) > 0)
            Try
                Me.m_zgh.PlotLines(lli.ToArray, Me.m_plotPanel(paneType), True, False, bCumulative)
            Catch ex As Exception

            End Try

        End Sub

        Private Sub UpdateColors()
            m_paneMaster.Fill = New Fill(Me.UIContext.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND))
            For Each p As GraphPane In Me.m_paneMaster.PaneList
                p.Chart.Fill = New Fill(Me.UIContext.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND))
            Next
        End Sub

        Private Function GetPlotTitle(ByVal data As ePlot) As String

            Dim iGroup As Integer = Math.Max(1, Me.m_lbGroups.SelectedGroupIndex)
            Dim group As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

            ' Configure mort pane caption
            If (data = ePlot.Mortality) Then
                If group.IsConsumer Then
                    Return My.Resources.ECOSIM_PLOT_CAPTION_MORT_CONS
                Else
                    Return My.Resources.ECOSIM_PLOT_CAPTION_MORT_PROD
                End If
            End If

            Dim fmt As New cSimPlotFormatter()
            Return fmt.GetDescriptor(data)

        End Function

        Private Function GetPlotYAxisLabel(data As ePlot) As String

            Dim iGroup As Integer = Math.Max(1, Me.m_lbGroups.SelectedGroupIndex)
            Dim group As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

            Select Case data

                Case ePlot.AvgWeightOrProdCons
                    If group.IsMultiStanza() Then
                        Return StyleGuide.FormatUnitString(cUnits.Currency)
                    Else
                        Return ""
                    End If

                Case ePlot.Biomass
                    Return StyleGuide.FormatUnitString(cUnits.Currency)

                Case ePlot.FeedingTime
                    Return ""

                Case ePlot.ConsumptionBiomass,
                     ePlot.FleetFishingMortality,
                     ePlot.Mortality,
                     ePlot.PredationMortality
                    Return StyleGuide.FormatUnitString(cUnits.OverTime)

                Case ePlot.Prey
                    Return SharedResources.HEADER_PREY_PERCENTAGE

                Case ePlot.Value
                    Return StyleGuide.FormatUnitString(cUnits.MonetaryOverBiomass)

                Case ePlot.[Catch]
                    Return StyleGuide.FormatUnitString(cUnits.CurrencyOverTime)
            End Select

            Return ""

        End Function

        Private Function GetPlotAxisMax(ByVal data As ePlot) As Double
            Select Case data
                Case ePlot.Prey : Return 100
            End Select
            Return 0
        End Function

#End Region ' Helper methods

    End Class

End Namespace