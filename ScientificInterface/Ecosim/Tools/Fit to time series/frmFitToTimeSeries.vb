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
Imports EwECore.FitToTimeSeries
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the Fit To Time Series user interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmFitToTimeSeries

#Region " Private variables "

        Private m_F2TSManager As cF2TSManager = Nothing
        Private m_shapeHandler As cAnomalySearchShapeGUIHandler = Nothing
        Private m_SensitivityByPredatorResults As cSensitivityToVulResults = Nothing
        Private m_cmdTSWeights As cCommand = Nothing
        Private m_shapeSelected As cShapeData = Nothing
        Private m_bInUpdate As Boolean = False

        Private m_fpNoAICPts As cEwEFormatProvider = Nothing
        Private m_fpUseDefaultVs As cEwEFormatProvider = Nothing
        Private m_bIsRunOwner As Boolean = False

#End Region 'Private variables

#Region " Constructor "

        Public Sub New()
            InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Form overrides "

        Public Overrides ReadOnly Property IsRunForm() As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Try

                Me.m_F2TSManager = Me.Core.EcosimFitToTimeSeries

                Me.m_grid.Manager = Me.Core.FishingPolicyManager
                Me.m_grid.UIContext = Me.UIContext
                Me.m_gridOutput.UIContext = Me.UIContext

                Me.m_cmdTSWeights = Me.UIContext.CommandHandler.GetCommand("WeightTimeSeries")
                If (Me.m_cmdTSWeights IsNot Nothing) Then
                    AddHandler Me.m_cmdTSWeights.OnPostInvoke, AddressOf OnPostInvokeTSCommand
                End If

                Me.m_fpNoAICPts = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxAICDataPts, Me.m_F2TSManager, eVarNameFlags.F2TSNAICData)
                Me.m_fpUseDefaultVs = New cPropertyFormatProvider(Me.UIContext, Me.m_cbResetVs, Me.m_F2TSManager, eVarNameFlags.F2TSUseDefaultV)

                Me.m_shapeHandler = New cAnomalySearchShapeGUIHandler(Me.UIContext)
                Me.m_shapeHandler.Attach(Me.m_shapeToolBox, Me.m_sketchPad)

                Me.m_cbAnomalySearch.Checked = Me.m_F2TSManager.AnomalySearch
                Me.m_cbVulnerabilitySearch.Checked = Me.m_F2TSManager.VulnerabilitySearch

                Me.m_nudSplinePts.Value = Me.m_F2TSManager.NumSplinePoints
                Me.m_nudVariance.Value = CDec(Me.m_F2TSManager.VulnerabilityVariance)
                Me.m_nudVariancePrimaryProd.Value = CDec(Me.m_F2TSManager.PPVariance)
                Me.m_vulnerabilityBlockCodeSelector.SelectedBlock = 1
                Me.m_vulnerabilityBlockMatrix.UIContext = Me.UIContext
                Me.m_vulnerabilityBlockMatrix.BlockColors = Me.m_vulnerabilityBlockCodeSelector.BlockColors
                Me.m_vulnerabilityBlockMatrix.SelectedBlockNum = Me.m_vulnerabilityBlockCodeSelector.SelectedBlock
                Me.m_sketchPad.FirstYear = CInt(Me.m_nudFirstYear.Value)
                Me.m_sketchPad.LastYear = CInt(Me.m_nudLastYear.Value)
                Me.m_sketchPad.NumDataPoints = Me.Core.nTimeSeriesYears

                AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateEvent

                Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.TimeSeries, eCoreComponentType.EcoPath, eCoreComponentType.ShapesManager, eCoreComponentType.MediatedInteractionManager}
                Me.UpdateMaxSplinePoints()
                Me.ReloadControls()

                Me.m_shapeToolBox.XAxisMaxValue = 0

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            If (Me.UIContext Is Nothing) Then Return

            RemoveHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateEvent

            If (Me.m_bIsRunOwner) Then
                Me.m_F2TSManager.Disconnect()
                Me.m_bIsRunOwner = False
            End If

            Try

                Me.m_fpNoAICPts.Release()
                Me.m_fpNoAICPts = Nothing

                Me.m_fpUseDefaultVs.Release()
                Me.m_fpUseDefaultVs = Nothing

                ' Detach from event handlers
                Me.m_vulnerabilityBlockCodeSelector = Nothing
                Me.m_F2TSManager = Nothing

                Me.m_shapeHandler.Detach()
                Me.m_shapeHandler = Nothing

                If (Me.m_cmdTSWeights IsNot Nothing) Then
                    RemoveHandler Me.m_cmdTSWeights.OnPostInvoke, AddressOf OnPostInvokeTSCommand
                    Me.m_cmdTSWeights = Nothing
                End If

                Me.m_grid.UIContext = Nothing
                Me.m_gridOutput.UIContext = Nothing

            Catch ex As Exception

            End Try

            MyBase.OnFormClosed(e)

        End Sub

        Private Sub ReloadControls()

            Try

                'set the max number of year to the same as the time series data
                Me.m_nudFirstYear.Maximum = Me.m_F2TSManager.nTimeSeriesYears
                Me.m_nudFirstYear.Value = Math.Max(0, Math.Min(Me.m_F2TSManager.FirstYear - 1, Me.m_F2TSManager.nTimeSeriesYears))

                Me.m_nudLastYear.Maximum = Me.m_F2TSManager.nTimeSeriesYears
                Me.m_nudLastYear.Value = Math.Min(Me.m_F2TSManager.LastYear, Me.m_F2TSManager.nTimeSeriesYears)

                Me.m_nudSplinePts.Value = Me.m_F2TSManager.NumSplinePoints
                Me.m_nudVariance.Value = CDec(Me.m_F2TSManager.VulnerabilityVariance)
                Me.m_nudVariancePrimaryProd.Value = CDec(Me.m_F2TSManager.PPVariance)

            Catch ex As Exception
                cLog.Write(ex)
            End Try

            Me.UpdateControls()
        End Sub

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            Select Case msg.Source

                Case eCoreComponentType.EcoPath
                    ' Hmm, this can be quite disastrous. Consider what to do here!
                    If ((msg.Type = eMessageType.DataAddedOrRemoved) And (msg.DataType = eDataTypes.EcoPathGroupInput)) Then
                        ' Make the vul control update itself
                        Me.m_vulnerabilityBlockMatrix.RefreshContent()
                        ' Etc...
                    End If

                Case eCoreComponentType.TimeSeries
                    Me.m_sketchPad.NumDataPoints = Me.Core.nTimeSeriesYears

                Case eCoreComponentType.ShapesManager
                    ' Refresh the Anomaly search content
                    If (msg.DataType = eDataTypes.Forcing) Then
                        Me.m_shapeHandler.Refresh()
                        Me.ReloadControls()
                    End If

                Case eCoreComponentType.MediatedInteractionManager
                    ' Refresh on shape assignment changes
                    Me.m_shapeHandler.Refresh()

            End Select
        End Sub

        Private Sub OnCoreExecutionStateEvent(ByVal csm As cCoreStateMonitor)
            Try
                ' Form may be closing because the core is shutting down. Nasty
                If Not Me.IsDisposed Then
                    Me.BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
                End If
            Catch ex As Exception

            End Try
        End Sub

        Public Overrides Property UIContext() As cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As cUIContext)
                MyBase.UIContext = value
                If Me.m_vulnerabilityBlockCodeSelector IsNot Nothing Then
                    Me.m_vulnerabilityBlockCodeSelector.UIContext = value
                End If
            End Set
        End Property

#End Region ' Form overrides

#Region " Private control event handlers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnSearch(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnSearch.Click

            Dim shapeSelected As cShapeData = Nothing

            ' Try to make sure TS are loaded
            If (Me.Core.nTimeSeriesEnabled = 0) And (Me.m_cmdTSWeights IsNot Nothing) Then
                Me.m_cmdTSWeights.Invoke()
            End If

            If (Me.Core.nTimeSeriesEnabled = 0) Then Return

            ' Update TS
            Me.Core.UpdateTimeSeries()

            shapeSelected = Me.m_shapeHandler.SelectedShape
            If shapeSelected Is Nothing Then
                Me.m_F2TSManager.AnomalySearchShapeNumber = 0
            Else
                Me.m_F2TSManager.AnomalySearchShapeNumber = shapeSelected.Index
            End If
            Me.m_F2TSManager.AnomalySearch = Me.m_cbAnomalySearch.Checked
            Me.m_F2TSManager.VulnerabilitySearch = Me.m_cbVulnerabilitySearch.Checked

            Me.m_F2TSManager.FirstYear = CInt(Me.m_nudFirstYear.Text) + 1
            Me.m_F2TSManager.LastYear = CInt(Me.m_nudLastYear.Text)
            Me.m_F2TSManager.NumSplinePoints = CInt(Me.m_nudSplinePts.Text)
            Me.m_F2TSManager.PPVariance = CSng(Me.m_nudVariancePrimaryProd.Value)
            Me.m_F2TSManager.VulnerabilityVariance = CSng(Me.m_nudVariance.Value)
            Me.m_F2TSManager.VulnerabilityBlocks = Me.m_vulnerabilityBlockMatrix.Vulblocks
            Me.m_F2TSManager.nBlockCodes = Me.m_vulnerabilityBlockCodeSelector.NumBlocks
            'Me.m_F2TSManager.NAICDataPoints = CInt(Me.m_nudAICDataPts.Value)

            Me.m_bIsRunOwner = True
            Me.m_F2TSManager.Connect(Me, AddressOf OnRunStarted, AddressOf OnRunStep, AddressOf OnRunStopped, AddressOf OnModelRun)
            Me.m_F2TSManager.RunSearch()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnStop(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnStop.Click
            'this will stop any running model Search or Sensitivity
            Me.m_F2TSManager.StopRun(0)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' -------------------------------------------------------------------
        Private Sub m_tsbSensOfSS2V_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbSensOfSS2V.Click

            Dim dlgSensOfSS As New dlgSensitivityOfSStoV(Me.UIContext, Me.m_F2TSManager)
            dlgSensOfSS.NumBlocks = Me.m_vulnerabilityBlockCodeSelector.NumBlocks

            ' Init vulnerabiltiy blocks
            For iPred As Integer = 1 To Me.Core.nGroups
                For iPrey As Integer = 1 To Me.Core.nGroups
                    dlgSensOfSS.VulnerabilityBlocks(iPred, iPrey) = Me.m_vulnerabilityBlockMatrix.Vulblocks(iPred, iPrey)
                Next iPrey
            Next iPred

            m_F2TSManager.VulnerabilityBlocks = Me.m_vulnerabilityBlockMatrix.Vulblocks
            m_F2TSManager.nBlockCodes = m_vulnerabilityBlockCodeSelector.NumBlocks

            If dlgSensOfSS.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then

                ' Transfer values from the Sensitivity form to this form
                ' Number of blocks, colors on the main form should match those set by the user on the Sensitivity form
                Me.m_vulnerabilityBlockCodeSelector.NumBlocks = dlgSensOfSS.NumBlocks
                ' Init block colors via selector
                Me.m_vulnerabilityBlockMatrix.BlockColors = Me.m_vulnerabilityBlockCodeSelector.BlockColors

                ' Transfer vulnerabiltiy blocks
                For iPred As Integer = 1 To Me.Core.nGroups
                    For iPrey As Integer = 1 To Me.Core.nGroups
                        Me.m_vulnerabilityBlockMatrix.Vulblocks(iPred, iPrey) = dlgSensOfSS.VulnerabilityBlocks(iPred, iPrey)
                    Next iPrey
                Next iPred

                ' Adjust numblocks
                Me.m_vulnerabilityBlockMatrix.Invalidate()
            End If

        End Sub

        Private Sub OnBlockSelected(ByVal sender As IBlockSelector) _
            Handles m_vulnerabilityBlockCodeSelector.OnBlockSelected
            Me.m_vulnerabilityBlockMatrix.SelectedBlockNum = sender.SelectedBlock
            Me.UpdateControls()
        End Sub

        Private Sub OnNumBlocksChanged(ByVal sender As IBlockSelector) _
            Handles m_vulnerabilityBlockCodeSelector.OnNumBlocksChanged
            Me.m_vulnerabilityBlockMatrix.BlockColors = sender.BlockColors
            Me.UpdateControls()
        End Sub

        Private Sub OnFirstYearChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudFirstYear.ValueChanged
            If (Not Me.m_bInUpdate) Then Me.m_sketchPad.FirstYear = CInt(Me.m_nudFirstYear.Value)
            Me.UpdateControls()
        End Sub

        Private Sub OnLastYearChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudLastYear.ValueChanged

            If (Not Me.m_bInUpdate) Then
                Me.m_sketchPad.LastYear = CInt(Me.m_nudLastYear.Value)
                Me.UpdateMaxSplinePoints()
            End If

            Me.UpdateControls()

        End Sub

        Private Sub OnNoSplinePtsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudSplinePts.ValueChanged

            If (Not Me.m_bInUpdate) Then Me.m_sketchPad.NumSplinePoints = CInt(Me.m_nudSplinePts.Value)
            Me.UpdateControls()

        End Sub

        Private Sub OnShapeSelectionChanged(ByVal ashapes As EwECore.cShapeData()) _
            Handles m_shapeToolBox.OnSelectionChanged

            If Me.UIContext Is Nothing Then Return

            Dim iMax As Integer = Me.Core.nEcosimYears

            ' Initialize Component will cause this event to be triggered when the form is
            ' not up and running yet. Here's a sanity check:
            If (Me.m_shapeHandler Is Nothing) Then Return

            Dim shape As cShapeData = Me.m_shapeHandler.SelectedShape

            ' Reset year range when new shape selected
            If (Not ReferenceEquals(m_shapeSelected, shape)) Then

                ' Remember newly selected shape
                Me.m_shapeSelected = shape

            End If

            Me.UpdateControls()

        End Sub

        Private Sub m_sketchPad_OnYearRangeChanged(ByVal sender As ucAnomalySearchSketchPad) Handles m_sketchPad.OnYearRangeChanged
            Me.m_bInUpdate = True
            Me.m_nudFirstYear.Value = Math.Min(Math.Max(Me.m_nudFirstYear.Minimum, sender.FirstYear), Me.m_nudFirstYear.Maximum)
            Me.m_nudLastYear.Value = Math.Min(Math.Max(Me.m_nudLastYear.Minimum, sender.LastYear), Me.m_nudLastYear.Maximum)
            Me.m_bInUpdate = False
        End Sub

        Private Sub m_btnTimeSeries_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnTimeSeriesWeights.Click
            If (Me.m_cmdTSWeights IsNot Nothing) Then
                Me.m_cmdTSWeights.Invoke()
            End If
        End Sub

        Private Sub OnAnomalySearchChecked(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_cbAnomalySearch.CheckedChanged
            Me.UpdateControls()
        End Sub

        Private Sub m_tsbSearchGroup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbSearchGroup.Click

            Dim nBlocks As Integer = 0 ' Me.m_vulnerabilityBlockCodeSelector.NumBlocks
            Dim iBlock As Integer = 1
            Dim ts As cTimeSeries = Nothing
            Dim gts As cGroupTimeSeries = Nothing
            Dim abUseBlock(Core.nGroups) As Boolean

            Me.Core.CheckResetDefaultVulnerabilities()

            For iTS As Integer = 1 To Core.nTimeSeries - 1
                ts = Me.Core.EcosimTimeSeries(iTS)
                If (TypeOf (ts) Is cGroupTimeSeries) And (ts.Enabled = True) Then
                    gts = DirectCast(ts, cGroupTimeSeries)
                    If (gts.TimeSeriesType = eTimeSeriesType.BiomassAbs) Or _
                       (gts.TimeSeriesType = eTimeSeriesType.BiomassRel) Or _
                       (gts.TimeSeriesType = eTimeSeriesType.CatchesRel) Or _
                       (gts.TimeSeriesType = eTimeSeriesType.Catches) Then

                        abUseBlock(gts.GroupIndex) = True
                        nBlocks += 1
                    End If
                End If
            Next

            ' Bump up the number of blocks if neccessary
            Me.m_vulnerabilityBlockCodeSelector.NumBlocks = _
                Math.Max(Me.m_vulnerabilityBlockCodeSelector.NumBlocks, nBlocks)

            iBlock = 1
            For i As Integer = 1 To Me.Core.nGroups
                For j As Integer = 1 To Me.Core.nGroups
                    If abUseBlock(i) Then
                        Me.m_vulnerabilityBlockMatrix.Vulblocks(i, j) = iBlock
                    Else
                        Me.m_vulnerabilityBlockMatrix.Vulblocks(i, j) = 0
                    End If
                Next j
                If abUseBlock(i) Then iBlock += 1
            Next i
            Me.m_vulnerabilityBlockMatrix.Invalidate()

        End Sub

        Private Sub OnClearOutputs(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnClearOutputs.Click
            Me.m_gridOutput.Clear()
        End Sub

        Private Sub OnVulBlocksColourChanged(ByVal sender As Object, ByVal iBlock As Integer) _
            Handles m_vulnerabilityBlockMatrix.OnSelectedBlockChanged
            Try
                Me.m_vulnerabilityBlockCodeSelector.SelectedBlock = iBlock
            Catch ex As Exception
                ' NOP
            End Try
        End Sub

        Private Sub OnShowAllData_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbShowAllData.CheckedChanged
            Try
                Me.m_shapeHandler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ShowExtraData, Nothing, Me.m_cbShowAllData.Checked)
            Catch ex As Exception
            End Try
        End Sub

#End Region ' Private control event handlers

#Region " Private manager event handlers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="runType"></param>
        ''' <param name="nSteps"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnRunStarted(ByVal runType As eRunType, ByVal nSteps As Integer)

            Me.m_lbResults.Items.Clear()

            Dim data As cF2TSResults = Me.m_F2TSManager.Results
            Me.LogProgress(cStringUtils.Localize(My.Resources.FIT2TS_PROGRESS_RUNSTARTED, data.BaseSS))

            Me.IsRunning = True
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnRunStep()

            If (Not Me.m_bIsRunOwner) Then Return

            Dim data As cF2TSResults = Me.m_F2TSManager.Results
            Dim runtype As eRunType = data.RunType

            Select Case runtype

                Case eRunType.Search
                    'retrieve search analysis result
                    Dim rsltSearch As cSearchResults = CType(data, cSearchResults)
                    Me.LogProgress(cStringUtils.Localize(My.Resources.FIT2TS_PROGRESS_RUNSTEP, rsltSearch.iStep, rsltSearch.IterSS))

                    ' Reload shape
                    If Me.m_F2TSManager.AnomalySearch Then
                        Me.Core.ForcingShapeManager.Load()
                        ' Ugh, there must be a better way to do this
                        For Each shape As cShapeData In Me.m_shapeHandler.SelectedShapes
                            shape.Update()
                        Next
                    End If

            End Select
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The model run has completed
        ''' </summary>
        ''' <param name="runType"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnRunStopped(ByVal runType As eRunType)

            If (Me.m_bIsRunOwner) Then
                Me.m_F2TSManager.Disconnect()

                Me.LogProgress(cStringUtils.Localize(My.Resources.FIT2TS_PROGRESS_RUNCOMPLETED, Date.Now().ToShortTimeString))

                If (TypeOf Me.m_F2TSManager.Results Is cSearchResults) Then
                    Dim res As cSearchResults = DirectCast(Me.m_F2TSManager.Results, cSearchResults)
                    Me.m_gridOutput.AddFitToTimeSeriesOutput(res.nAICPars, res.IterSS)
                End If

                Me.IsRunning = False
                Me.m_bIsRunOwner = False
            End If

            ' Always do this though
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="runType"></param>
        ''' <param name="iCurrentIterationStep"></param>
        ''' <param name="nTotalIterationSteps"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnModelRun(ByVal runType As eRunType, ByVal iCurrentIterationStep As Integer, ByVal nTotalIterationSteps As Integer)
            '    System.Console.WriteLine("Ecosim run " & iCurrentIterationStep.ToString & " of " & nTotalIterationSteps.ToString)
        End Sub

#End Region ' Private search event handlers

#Region " Private command handler "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Command handler, invoked after the user has changed the enabled
        ''' time series configuration.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnPostInvokeTSCommand(ByVal cmd As cCommand)
            Me.UpdateControls()
        End Sub

#End Region ' Private command handler

#Region " Internal implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update state of main controls based on user selections.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub UpdateControls()

            If (Me.m_F2TSManager Is Nothing) Then Return
            If (Me.m_sketchPad Is Nothing) Then Return

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True

            Dim bInputsValid As Boolean = Me.Core.HasAppliedTimeSeries()
            Dim bIsRunning As Boolean = Me.IsRunning Or Me.Core.StateMonitor.IsSearching()

            If Me.m_cbAnomalySearch.Checked Then
                bInputsValid = bInputsValid And _
                               (Me.m_shapeHandler.SelectedShape IsNot Nothing) And _
                               (Me.m_nudLastYear.Value > Me.m_nudFirstYear.Value)
            Else
                'bInputsValid = True
            End If

            If (bIsRunning) Then
                Me.m_split1.Enabled = Me.m_bIsRunOwner
            Else
                Me.m_split1.Enabled = True
            End If

            Me.m_btnStop.Enabled = Me.IsRunning
            Me.m_btnSearch.Enabled = (Not bIsRunning) And bInputsValid
            Me.m_sketchPad.Enabled = (Not bIsRunning)
            Me.m_shapeToolBox.Enabled = (Not bIsRunning)
            Me.m_nudFirstYear.Enabled = (Not bIsRunning)
            Me.m_nudLastYear.Enabled = (Not bIsRunning)
            Me.m_nudSplinePts.Enabled = (Not bIsRunning)
            Me.m_nudVariance.Enabled = (Not bIsRunning)
            Me.m_nudVariancePrimaryProd.Enabled = (Not bIsRunning)
            Me.m_cbVulnerabilitySearch.Enabled = (Not bIsRunning)
            Me.m_cbAnomalySearch.Enabled = (Not bIsRunning)
            Me.m_cbResetVs.Enabled = (Not bIsRunning)

            'constrain the number of years to the number of years in the time series data
            If Me.m_nudLastYear.Value > Me.m_F2TSManager.nTimeSeriesYears Then
                Me.m_nudLastYear.Value = Me.m_F2TSManager.nTimeSeriesYears
            End If

            Me.m_nudFirstYear.Maximum = Me.m_F2TSManager.nTimeSeriesYears
            Me.m_nudLastYear.Maximum = Me.m_F2TSManager.nTimeSeriesYears

            Me.m_bInUpdate = False

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strEntry"></param>
        ''' -------------------------------------------------------------------
        Private Sub LogProgress(ByVal strEntry As String)
            Me.m_lbResults.Items.Insert(0, strEntry)
        End Sub

        Private Sub UpdateMaxSplinePoints()

            Dim nMax As Integer = CInt(Math.Min(Me.m_nudSplinePts.Value, Me.m_nudLastYear.Value))
            If (nMax < Me.m_nudSplinePts.Value) Then
                Me.m_nudSplinePts.Value = nMax
                Me.m_sketchPad.NumSplinePoints = CInt(Me.m_nudSplinePts.Value)
            End If
            Me.m_nudSplinePts.Maximum = Me.m_nudLastYear.Value

        End Sub

#End Region ' Internal implementation

    End Class

End Namespace