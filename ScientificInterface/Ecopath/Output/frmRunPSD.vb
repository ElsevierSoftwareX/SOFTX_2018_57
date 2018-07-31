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
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region 'Imports

Namespace Ecopath.Output

    Public Class RunPSD

#Region " Variables "

        ' -- Core connection
        Private m_coreStateMonitor As cCoreStateMonitor = Nothing

        ' -- To make life easier and a more fun place to be
        Private m_zgh As cZedGraphHelper = Nothing

        ' -- Format providers --
        Private m_fpNoOfPointsPSD As cPropertyFormatProvider = Nothing
        Private m_fpMinWeight As cPropertyFormatProvider = Nothing
        Private m_fpNoOfPointsMovAvg As cPropertyFormatProvider = Nothing
        Private m_fpClimateType As cPropertyFormatProvider = Nothing

        ' -- Internal admin --
        ''' <summary>Flag stating whether the current Ecopath results have been plotted.</summary>
        Private m_bEcopathResultsPlotted As Boolean = False

        Private m_cmdShowGroups As cDisplayGroupsCommand = Nothing

#End Region ' Variables

#Region " Constructor/Destructor "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor/Destructor

#Region " Form overrides "

        Public Overrides ReadOnly Property IsRunForm() As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim parms As cPSDParameters = Nothing
            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim pm As cPropertyManager = Me.PropertyManager

            Me.m_coreStateMonitor = Me.Core.StateMonitor
            Me.m_zgh = New cZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_zedgraph)

            ' Connect to show/hide groups command
            Me.m_cmdShowGroups = DirectCast(cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME), cDisplayGroupsCommand)
            If Me.m_cmdShowGroups IsNot Nothing Then
                Me.m_cmdShowGroups.AddControl(Me.m_bntShowGroups)
                AddHandler Me.m_cmdShowGroups.OnPostInvoke, AddressOf OnAfterShowGroups
            End If

            ' Connect format providers
            parms = Me.Core.ParticleSizeDistributionParameters
            Me.m_fpNoOfPointsPSD = New cPropertyFormatProvider(Me.UIContext, Me.NumericUpDown1, parms, eVarNameFlags.PSDNumWeightClasses)
            Me.m_fpMinWeight = New cPropertyFormatProvider(Me.UIContext, Me.m_nudLowestWtClass, parms, eVarNameFlags.PSDFirstWeightClass)
            Me.m_fpNoOfPointsMovAvg = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNoWtClasses, parms, eVarNameFlags.NumPtsMovAvg)
            Me.m_fpClimateType = New cPropertyFormatProvider(Me.UIContext, Me.m_cmbMeanLat, parms, eVarNameFlags.NumPtsMovAvg, Nothing, New cClimateTypeFormatter())

            ' Connect to core state monitor events
            AddHandler Me.m_coreStateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

            Try
                Me.SynchronizePSDGroups()
                Me.SynchronizePlot()
            Catch ex As Exception

            End Try
            ' Sync controls
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            ' Detach format providers
            Me.m_fpNoOfPointsPSD.Release()
            Me.m_fpMinWeight.Release()
            Me.m_fpNoOfPointsMovAvg.Release()
            Me.m_fpClimateType.Release()

            Me.m_zgh.Detach()
            Me.m_zgh = Nothing

            ' Detach from show/hide groups command
            If Me.m_cmdShowGroups IsNot Nothing Then
                RemoveHandler Me.m_cmdShowGroups.OnPostInvoke, AddressOf OnAfterShowGroups
                Me.m_cmdShowGroups.RemoveControl(Me.m_bntShowGroups)
            End If

            ' Detach from core state monitor events
            RemoveHandler Me.m_coreStateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Form overrides

#Region " Event handlers "

        Private Sub OnMortalityTypeSelected(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_rbGroupPB.CheckedChanged, m_rbLorenzen.CheckedChanged

            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters

            ' This should invalidate PSD results via cCore variable validation
            If Me.m_rbGroupPB.Checked Then
                parms.MortalityType = ePSDMortalityTypes.GroupZ
            Else
                parms.MortalityType = ePSDMortalityTypes.Lorenzen
            End If

            ' Reflect changes
            Me.UpdateControls()

        End Sub

        Private Sub OnRun(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnRun.Click

            'PSD Enabled needs to be true for Ecopath to run the PSD 
            'The PSDEnabled flag is set in the Model Description form and may be False preventing the PSD from running
            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            Dim orgEnabled As Boolean = parms.PSDEnabled
            parms.PSDEnabled = True

            ' Run Ecopath
            Me.Core.RunEcoPath()

            'set PSDEnabled back to it's original value 
            'This way if it's False (most likely) then it will not run in a normal Ecopath run
            parms.PSDEnabled = orgEnabled

        End Sub

        Private Sub OnCoreExecutionStateChanged(ByVal csm As cCoreStateMonitor)
            Try
                Me.SynchronizePlot()
            Catch ex As Exception
            End Try
        End Sub

        Private Sub OnAfterShowGroups(ByVal cmd As cCommand)
            Try
                Me.SynchronizePSDGroups()
            Catch ex As Exception
            End Try
        End Sub

        Private Sub OnLatChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_cmbMeanLat.SelectedIndexChanged
            Try
                Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
                parms.ClimateType = DirectCast(Me.m_cmbMeanLat.SelectedIndex, eClimateTypes)
            Catch ex As Exception
                ' Whoah
            End Try
        End Sub

#End Region ' Event handlers

#Region " Helper methods "

        ''' <summary>
        ''' Apply selected groups to PSD
        ''' </summary>
        Private Sub SynchronizePSDGroups()
            ' Update groups to include in PSD - which is driven by show groups interface
            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            For iGroup As Integer = 1 To Me.Core.nLivingGroups
                parms.GroupIncluded(iGroup) = Me.StyleGuide.GroupVisible(iGroup)
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Synchronize the plot area with Ecopath results.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub SynchronizePlot()

            ' This code is optimized to only plot when new results are available

            ' Are Ecopath results available?
            If Me.m_coreStateMonitor.HasPSDRan Then
                ' #Yes: are these results not plotted yet?
                If Me.m_bEcopathResultsPlotted = False Then
                    ' #Yes: Plot the curves
                    Me.PlotCurves()
                    ' Set flag to remind ourselves that these results are plotted
                    Me.m_bEcopathResultsPlotted = True
                End If
            Else
                '#No: Ecopath results have disappeared (or are not yet available)
                'Is the plot populated?
                If Me.m_bEcopathResultsPlotted = True Then
                    ' #Yes: clear the plot
                    Me.InitializePane()
                    ' Set local flag to remind ourselves that the plot is empty
                    Me.m_bEcopathResultsPlotted = False
                End If
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure the graph pane to look pretty and dandy.
        ''' </summary>
        ''' <returns>
        ''' The graph pane that was initialized.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function InitializePane() As GraphPane

            Dim pane As GraphPane = Me.m_zedgraph.GraphPane
            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters

            pane.CurveList.Clear()

            'JS 23Mar09: Zedgraph helper performs standardized label, axis styling
            Me.m_zgh.ConfigurePane(My.Resources.CAPTION_PSD, _
                                   SharedResources.HEADER_BODYWEIGHT_LOGg, _
                                   SharedResources.HEADER_BIOMASS_LOGg, _
                                   True)

            'JS 15Oct09: Fonts are set via StyleGuide
            'pane.Title.FontSpec.Size = 16
            'pane.Legend.FontSpec.Size = 14
            'pane.XAxis.Title.FontSpec.Size = 14
            'pane.YAxis.Title.FontSpec.Size = 14

            pane.XAxis.Scale.Min = CInt(Math.Log10(parms.FirstWeightClass))
            pane.XAxis.Scale.Max = Math.Round(Math.Log10(parms.FirstWeightClass * 2 ^ (Me.Core.nWeightClasses - 1)) + 0.4, 0, MidpointRounding.AwayFromZero)
            pane.YAxis.Scale.Min = 0

            pane.YAxis.Cross = 0
            pane.YAxis.CrossAuto = False

            Return pane

        End Function

        Private Sub AddCurves(ByVal pane As GraphPane)

            Dim resultLists As New List(Of PointPairList)
            Dim sXValue As Single = 0
            Dim sSystemPSD(Me.Core.nWeightClasses) As Single
            Dim sSlope As Single
            Dim sSlopeStdErr As Single
            Dim sIntercept As Single
            Dim sInterceptStdErr As Single
            Dim sCorrelation As Single
            Dim sLowWtClass As Single
            Dim sHighWtClass As Single
            Dim iSampleSize As Integer
            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            Dim sg As cStyleGuide = Me.StyleGuide
            Dim strLabel As String = ""

            Me.InitLists(resultLists, 2)

            'Find system PSD by summing the group PSD
            Me.FindSystemPSD(sSystemPSD)

            'Find regression of the system PSD
            Me.FindRegression(sSlope, sSlopeStdErr, sIntercept, sInterceptStdErr, sCorrelation, _
                              sLowWtClass, sHighWtClass, iSampleSize, sSystemPSD)

            For iWtClass As Integer = 1 To Me.Core.nWeightClasses
                If sSystemPSD(iWtClass) * 1000000000 > 0 Then
                    sXValue = CSng(parms.FirstWeightClass * 2 ^ (iWtClass - 1))

                    'PSD data
                    resultLists(0).Add(Math.Log10(sXValue), Math.Log10(sSystemPSD(iWtClass) * 1000000000)) '* 1000000000 for plotting purpose
                    'PSD regression data
                    resultLists(1).Add(Math.Log10(sXValue), sSlope * Math.Log10(sXValue) + sIntercept)

                End If
            Next

            'PSD plot
            Me.AddCurveToGraphPane(pane, resultLists(0), "", Color.Transparent)
            'PSD regression plot
            If iSampleSize = 2 Then
                'Without std err
                strLabel = String.Format(My.Resources.PSD_GRAPH_REGRESSION_LABEL_WO_STDERR, sg.FormatNumber(sSlope), _
                                    sg.FormatNumber(sIntercept), sg.FormatNumber(sCorrelation) & cStringUtils.vbCrLf, _
                                    sg.FormatNumber(sLowWtClass), sg.FormatNumber(sHighWtClass), sg.FormatNumber(iSampleSize))
            Else
                'With std err
                strLabel = String.Format(My.Resources.PSD_GRAPH_REGRESSION_LABEL_W_STDERR, sg.FormatNumber(sSlope), sg.FormatNumber(sSlopeStdErr), _
                                    sg.FormatNumber(sIntercept), sg.FormatNumber(sInterceptStdErr), sg.FormatNumber(sCorrelation) & cStringUtils.vbCrLf, _
                                    sg.FormatNumber(sLowWtClass), sg.FormatNumber(sHighWtClass), sg.FormatNumber(iSampleSize))
            End If
            Me.AddCurveToGraphPane(pane, resultLists(1), strLabel, Color.Black)
        End Sub

        Private Sub InitLists(ByRef lists As List(Of PointPairList), ByVal size As Integer)
            ' Init the result lists
            For i As Integer = 1 To size
                Dim list As New PointPairList()
                lists.Add(list)
            Next
        End Sub

        Private Sub AddCurveToGraphPane(ByVal pane As GraphPane, ByVal list As PointPairList, _
                                        ByVal strLabel As String, ByVal lineClr As Color)
            Dim lnItem As LineItem

            lnItem = pane.AddCurve(strLabel, list, lineClr)

            If lineClr = Color.Transparent Then
                lnItem.Line.IsVisible = False
                lnItem.Symbol.Type = SymbolType.Circle
                lnItem.Symbol.Border.IsVisible = False
                lnItem.Symbol.Fill.IsVisible = True
                lnItem.Symbol.Fill.Brush = Brushes.Black
            Else
                lnItem.Line.IsVisible = True
                lnItem.Symbol.Type = SymbolType.None
            End If

        End Sub

        Private Sub UpdatePlot()
            Me.m_zedgraph.AxisChange()
            Me.m_zedgraph.Refresh()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update toolstrip item states.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub UpdateControls()

            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            Dim grpInput As cEcoPathGroupInput = Nothing
            Dim sg As cStyleGuide = Me.StyleGuide

            Select Case parms.MortalityType
                Case ePSDMortalityTypes.GroupZ
                    Me.m_rbGroupPB.Checked = True
                    Me.m_cmbMeanLat.Enabled = False
                    Me.m_cmbMeanLat.SelectedIndex = parms.ClimateType

                Case ePSDMortalityTypes.Lorenzen
                    Me.m_rbLorenzen.Checked = True
                    Me.m_cmbMeanLat.Enabled = True
                    Me.m_cmbMeanLat.SelectedIndex = parms.ClimateType
            End Select

            MyBase.UpdateControls()

        End Sub

        Private Sub PlotCurves()
            Me.AddCurves(Me.InitializePane())
            Me.UpdatePlot()
        End Sub

        Private Sub FindSystemPSD(ByVal sSystemPSD() As Single)

            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            Dim grpOutput As cEcoPathGroupOutput = Nothing

            'Find the system PSD by summing the group PSD
            For iGroup As Integer = 1 To Me.Core.nLivingGroups
                If parms.GroupIncluded(iGroup) Then
                    grpOutput = Me.Core.EcoPathGroupOutputs(iGroup)
                    For iWtClass As Integer = 1 To Me.Core.nWeightClasses
                        sSystemPSD(iWtClass) = sSystemPSD(iWtClass) + grpOutput.PSD(iWtClass)
                    Next
                End If
            Next

        End Sub

        Private Sub FindRegression(ByRef sSlope As Single, ByRef sSlopeStdErr As Single, _
                                   ByRef sIntercept As Single, ByRef sInterceptStdErr As Single, _
                                   ByRef sCorrelation As Single, ByRef sLowWtClass As Single, ByRef sHighWtClass As Single, _
                                   ByRef iSampleSize As Integer, ByVal sSystemPSD() As Single)

            Dim sXValue As Single = 0
            Dim dSumX As Double = 0
            Dim dSumY As Double = 0
            Dim dSumXSq As Double = 0
            Dim dSumYSq As Double = 0
            Dim dSumXY As Double = 0
            Dim iNum As Integer = 0
            Dim sXMin As Single = -1
            Dim sXMax As Single
            Dim dXMean As Double
            Dim dYMean As Double
            Dim dSumXdevYdev As Double = 0
            Dim dSumXdevSq As Double = 0
            Dim dSumYdevSq As Double = 0
            Dim dXStdDev As Double
            Dim dYStdDev As Double
            Dim dEstStdErr As Double
            Dim parms As cPSDParameters = Me.Core.ParticleSizeDistributionParameters

            For iWtClass As Integer = 1 To Me.Core.nWeightClasses
                If sSystemPSD(iWtClass) * 1000000000 > 0 Then
                    sXValue = CSng(parms.FirstWeightClass * 2 ^ (iWtClass - 1))

                    dSumX = dSumX + Math.Log10(sXValue)
                    dSumY = dSumY + Math.Log10(sSystemPSD(iWtClass) * 1000000000.0)
                    dSumXSq = dSumXSq + Math.Log10(sXValue) ^ 2
                    dSumYSq = dSumYSq + Math.Log10(sSystemPSD(iWtClass) * 1000000000.0) ^ 2
                    dSumXY = dSumXY + Math.Log10(sXValue) * Math.Log10(sSystemPSD(iWtClass) * 1000000000.0)

                    'v.5
                    'sXValue = iWtClass

                    'dSumX = dSumX + sXValue
                    'dSumY = dSumY + Math.Log10(sSystemPSD(iWtClass))
                    'End v.5
                    If sXMin < 0 Then sXMin = sXValue
                    sXMax = sXValue
                    iNum = iNum + 1
                End If
            Next
            dXMean = dSumX / iNum
            dYMean = dSumY / iNum

            For iWtClass As Integer = 1 To Me.Core.nWeightClasses
                If sSystemPSD(iWtClass) * 1000000000 > 0 Then
                    sXValue = CSng(parms.FirstWeightClass * 2 ^ (iWtClass - 1))

                    dSumXdevYdev = dSumXdevYdev + (Math.Log10(sXValue) - dXMean) * (Math.Log10(sSystemPSD(iWtClass) * 1000000000) - dYMean)
                    dSumXdevSq = dSumXdevSq + (Math.Log10(sXValue) - dXMean) ^ 2
                    dSumYdevSq = dSumYdevSq + (Math.Log10(sSystemPSD(iWtClass) * 1000000000) - dYMean) ^ 2

                    'v.5
                    'sXValue = iWtClass

                    'dSumXdevYdev = dSumXdevYdev + ((sXValue) - dXMean) * (Math.Log10(sSystemPSD(iWtClass)) - dYMean)
                    'dSumXdevSq = dSumXdevSq + ((sXValue) - dXMean) ^ 2
                    'End v.5
                End If
            Next

            sSlope = CSng(dSumXdevYdev / dSumXdevSq)
            sIntercept = CSng(dYMean - sSlope * dXMean)

            dXStdDev = Math.Sqrt(dSumXdevSq / (iNum - 1))
            dYStdDev = Math.Sqrt(dSumYdevSq / (iNum - 1))
            dEstStdErr = Math.Sqrt((iNum - 1) * (dYStdDev ^ 2 - sSlope ^ 2 * dXStdDev ^ 2) / (iNum - 2))
            sSlopeStdErr = CSng(dEstStdErr / (Math.Sqrt(iNum - 1) * dXStdDev))
            sInterceptStdErr = CSng(dEstStdErr * Math.Sqrt((1 / iNum) + (dXMean ^ 2 / ((iNum - 1) * dXStdDev ^ 2))))

            sCorrelation = CSng((iNum * dSumXY - dSumX * dSumY) / _
                           (Math.Sqrt(iNum * dSumXSq - dSumX ^ 2) * Math.Sqrt(iNum * dSumYSq - dSumY ^ 2)))
            sLowWtClass = sXMin
            sHighWtClass = sXMax
            iSampleSize = iNum

        End Sub

#End Region ' Helper methods

    End Class

End Namespace