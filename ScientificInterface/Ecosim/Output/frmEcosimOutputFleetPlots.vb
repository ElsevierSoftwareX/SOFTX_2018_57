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
    ''' Form, implementing the Ecosim fleet output plots interface.
    ''' </summary>
    Public Class frmEcosimOutputFleetPlots

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

                ' ToDo: globalize this method

                Select Case DirectCast(value, ePlot)
                    Case ePlot.FleetFishingMortality : Return SharedResources.HEADER_FISHINGMORTALITY
                    Case ePlot.Value : Return SharedResources.HEADER_VALUE
                    Case ePlot.[Catch] : Return SharedResources.HEADER_CATCH
                    Case ePlot.DiscardsMortality : Return "Discards mortality"
                    Case ePlot.DiscardsSurvival : Return "Discards survival"
                    Case ePlot.Landings : Return "Landings"
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

        Dim m_TSInterval As eTSDataSetInterval

        Private Enum ePlot As Integer
            [Catch]
            Value
            Landings
            FleetFishingMortality
            DiscardsMortality
            DiscardsSurvival
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
                If (Me.UIContext IsNot Nothing) Then
                    Me.m_lbFleets.Detach()
                    Me.m_lbGroups.Detach()
                End If
                MyBase.UIContext = value
                If (Me.UIContext IsNot Nothing) Then
                    Me.m_lbFleets.Attach(Me.UIContext)
                    Me.m_lbGroups.Attach(Me.UIContext)
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
            Me.m_lbFleets.SelectedIndex = 0

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

        Private Sub OnSaveData(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnSaveData.Click

            Dim cmd As cEcosimSaveDataCommand = DirectCast(Me.CommandHandler.GetCommand("ExportEcosimResultsToCSV"), cEcosimSaveDataCommand)
            Dim aResults As New List(Of cEcosimResultWriter.eResultTypes)

            If Me.m_cbSaveVisibleOnly.Checked Then
                For Each plot As ePlot In [Enum].GetValues(GetType(ePlot))
                    If Me.m_plotVisible(plot) Then
                        Select Case plot
                            Case ePlot.Catch
                                aResults.Add(cEcosimResultWriter.eResultTypes.Catch)
                            Case ePlot.FleetFishingMortality
                                aResults.Add(cEcosimResultWriter.eResultTypes.Mortality)
                            Case ePlot.Value
                                aResults.Add(cEcosimResultWriter.eResultTypes.Value)
                            Case ePlot.Landings
                                aResults.Add(cEcosimResultWriter.eResultTypes.Landings)
                            Case ePlot.DiscardsMortality
                                aResults.Add(cEcosimResultWriter.eResultTypes.DiscardMortalityFleetGroup)
                            Case ePlot.DiscardsSurvival
                                aResults.Add(cEcosimResultWriter.eResultTypes.DiscardSurvivalFleetGroup)
                            Case Else
                                Debug.Assert(False, "Plot type not supported")
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
                        Me.m_plotVisible(plot) = (Array.IndexOf(dlg.Selection, CInt(plot)) >= 0)
                    Catch ex As Exception

                    End Try
                Next
            End If
            Me.ConfigurePlots()
            Me.UpdatePlots()

        End Sub

        ''' <summary>
        ''' Event hander to display results for another fleet
        ''' </summary>
        Private Sub OnFleetSelectionChanged(sender As Object, e As EventArgs) _
            Handles m_lbFleets.SelectedIndexChanged

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
                Me.ConfigurePane(data, Me.GetPlotTitle(data), Me.GetPlotYAxisLabel(data))
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure a plot on the main graph
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ConfigurePane(ByVal plot As ePlot, ByVal strTitle As String, strYAxisLabel As String)

            If Not Me.m_plotVisible(plot) Then Return
            ' Sanity check
            Debug.Assert(Me.m_plotPanel(plot) > 0)
            ' Configure pane
            Me.m_zgh.ConfigurePane(strTitle,
                       SharedResources.HEADER_TIME,
                       Me.Core.EcosimFirstYear,
                       Me.Core.EcosimFirstYear + (Me.Core.nEcosimTimeSteps / cCore.N_MONTHS),
                       strYAxisLabel, 0, 0,
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
            Dim iFleet As Integer = Math.Max(1, Me.m_lbFleets.SelectedFleetIndex)
            Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(iFleet)

            'Set the master pane title
            Me.m_zgh.Configure(fleet.Name)

            ' Clear all panes
            For Each pane As GraphPane In Me.m_graph.MasterPane.PaneList
                pane.CurveList.Clear()
                pane.AxisChange()
            Next

            ' Do not render when sim has not ran. This state should be impossible.
            If (Not Me.Core.StateMonitor.HasEcosimRan) Then Return

            For iGroup As Integer = 1 To Me.Core.nGroups
                If (fleet.Landings(iGroup) > 0 Or fleet.Discards(iGroup) > 0) Then
                    Dim grp As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
                    Dim clr As Color = Me.UIContext.StyleGuide.GroupColor(Me.Core, iGroup)
                    Dim grpOut As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

                    Dim pplCatch As New PointPairList()
                    Dim pplLandings As New PointPairList()
                    Dim pplValueFleet As New PointPairList()
                    Dim pplFishMortFleet As New PointPairList()
                    Dim pplDiscardsMortality As New PointPairList()
                    Dim pplDiscardsSurvival As New PointPairList()

                    ' Work-around for ZedGraph bug - ZedGraph does not include the axis grace range for entirely flat lines
                    pplCatch.Add(Me.Core.EcosimFirstYear, 0)
                    pplLandings.Add(Me.Core.EcosimFirstYear, 0)
                    pplValueFleet.Add(Me.Core.EcosimFirstYear, 0)
                    pplFishMortFleet.Add(Me.Core.EcosimFirstYear, 0)
                    pplDiscardsMortality.Add(Me.Core.EcosimFirstYear, 0)
                    pplDiscardsSurvival.Add(Me.Core.EcosimFirstYear, 0)

                    For i As Integer = 1 To Me.Core.nEcosimTimeSteps
                        ' ToDo: take Ecosim time step size into account?
                        dXValue = Me.Core.EcosimFirstYear + ((i - 1) / cCore.N_MONTHS)

                        pplCatch.Add(dXValue, grpOut.CatchByFleet(iFleet, i))
                        pplLandings.Add(dXValue, grpOut.LandingsByFleet(iFleet, i))
                        pplValueFleet.Add(dXValue, grpOut.ValueByFleet(iFleet, i))
                        pplFishMortFleet.Add(dXValue, grpOut.FishingMortByFleet(iFleet, i))
                        pplDiscardsMortality.Add(dXValue, grpOut.DiscardMortByFleet(iFleet, i))
                        pplDiscardsSurvival.Add(dXValue, grpOut.DiscardSurvivedByFleet(iFleet, i))
                    Next

                    Me.AddCurveToGraphPane(ePlot.[Catch], Me.m_zgh.CreateLineItem(grp, pplCatch))
                    Me.AddCurveToGraphPane(ePlot.Landings, Me.m_zgh.CreateLineItem(grp, pplLandings))
                    Me.AddCurveToGraphPane(ePlot.Value, Me.m_zgh.CreateLineItem(grp, pplValueFleet))
                    Me.AddCurveToGraphPane(ePlot.FleetFishingMortality, Me.m_zgh.CreateLineItem(grp, pplFishMortFleet))
                    Me.AddCurveToGraphPane(ePlot.DiscardsMortality, Me.m_zgh.CreateLineItem(grp, pplDiscardsMortality))
                    Me.AddCurveToGraphPane(ePlot.DiscardsSurvival, Me.m_zgh.CreateLineItem(grp, pplDiscardsSurvival))
                End If
            Next

        End Sub

        Private Sub UpdatePlots()
            Try
                Me.AddCurves()
                Me.m_zgh.RescaleAndRedraw()
                Me.ShowFleet()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub ShowFleet()

            Dim fleet As cEcopathFleetInput = Me.m_lbFleets.SelectedFleet
            Dim iFleet As Integer = fleet.Index
            Dim lGroupIndex As New List(Of Integer)
            Dim lCatch As New List(Of Single)

            ' Show all relevant groups, sorted by sum of catch
            For i As Integer = 1 To Me.UIContext.Core.nGroups
                If fleet.Discards(i) > 0 Or fleet.Landings(i) > 0 Then
                    Dim grp As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(i)
                    Dim sCatch As Single = 0
                    For j As Integer = 1 To Me.Core.nEcosimTimeSteps
                        sCatch += grp.CatchByFleet(iFleet, j)
                    Next
                    lCatch.Add(sCatch)
                    lGroupIndex.Add(i)
                End If
            Next
            Me.PopulateGroupListBox(Me.m_lbGroups, lGroupIndex.ToArray(), lCatch.ToArray())
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
                                        ByVal li As LineItem)
            Dim lli As New List(Of ZedGraph.LineItem)
            lli.Add(li)
            Me.AddCurvesToGraphPane(paneType, lli)
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
                                         ByVal lli As List(Of LineItem))

            If Not Me.m_plotVisible(paneType) Then Return
            ' Sanity check
            Debug.Assert(Me.m_plotPanel(paneType) > 0)
            Try
                Me.m_zgh.PlotLines(lli.ToArray, Me.m_plotPanel(paneType), True, False, False)
            Catch ex As Exception

            End Try

        End Sub

        Private Sub UpdateColors()
            m_paneMaster.Fill = New Fill(Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND))
            For Each p As GraphPane In Me.m_paneMaster.PaneList
                p.Chart.Fill = New Fill(Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND))
            Next
        End Sub

        Private Function GetPlotTitle(ByVal data As ePlot) As String

            Dim fmt As New cSimPlotFormatter()
            Return fmt.GetDescriptor(data)

        End Function

        Private Function GetPlotYAxisLabel(data As ePlot) As String

            Select Case data

                Case ePlot.FleetFishingMortality
                    Return StyleGuide.FormatUnitString(cUnits.OverTime)

                Case ePlot.Value
                    Return StyleGuide.FormatUnitString(cUnits.MonetaryOverBiomass)

                Case ePlot.[Catch]
                    Return StyleGuide.FormatUnitString(cUnits.CurrencyOverTime)
            End Select

            Return ""

        End Function

#End Region ' Helper methods

    End Class

End Namespace