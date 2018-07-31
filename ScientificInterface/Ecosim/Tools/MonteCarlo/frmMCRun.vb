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
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterface.Controls
Imports ZedGraph
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    ''' <summary>
    ''' Form class that implements the Ecosim Monte Carlo interface.
    ''' </summary>
    Public Class frmMCRun

#Region " Private vars "

        Private m_mcmanager As cMonteCarloManager = Nothing
        Private m_plothelper As cEcosimOutputPlotHelper = Nothing

        Private WithEvents m_cmdRunMonteCarlo As cCommand = Nothing
        Private WithEvents m_cmdStopMonteCarlo As cCommand = Nothing
        Private WithEvents m_cmdLoadTS As cCommand = Nothing

        ''' <summary>Live monitoring of Ecosim NYears</summary>
        Private m_propNYears As cSingleProperty = Nothing

        Private m_fpNumTrials As cEwEFormatProvider = Nothing
        Private m_fpTrial As cEwEFormatProvider = Nothing
        Private m_fpERun As cEwEFormatProvider = Nothing
        Private m_fpSSorg As cEwEFormatProvider = Nothing
        Private m_fpSS As cEwEFormatProvider = Nothing
        Private m_fpSSBest As cEwEFormatProvider = Nothing

        Private m_fpEETol As cEwEFormatProvider = Nothing
        Private m_fpFMratio As cEwEFormatProvider = Nothing

        Private m_lpplIteration As New List(Of PointPairList)
        Private m_lSS As New List(Of Single)

        ''' <summary>
        ''' Local counter for the number of trials run
        ''' </summary>
        ''' <remarks>Zeroed when the MC completes its run MonteCarloCompletedHandler(), incremented in newRun(). 
        ''' We can not use the MC counter because it is not zeroed until the run is started by the MC. 
        ''' We need to know what run it about to happen before the run so we can store the local data.
        ''' </remarks>
        Private m_nTrials As Integer

        Private m_sYMax As Single = 1.0!

        Private m_qeB As New cQuickEditHandler()
        Private m_qePB As New cQuickEditHandler()
        Private m_qeQB As New cQuickEditHandler()
        Private m_qeEE As New cQuickEditHandler()
        Private m_qeBA As New cQuickEditHandler()
        Private m_qeBaBi As New cQuickEditHandler()
        Private m_qeDC As New cQuickEditHandler()
        Private m_qeLandings As New cQuickEditHandler()
        Private m_qeDiscards As New cQuickEditHandler()

        Private m_bShowBetterSS As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()

            Me.InitializeComponent()

        End Sub

#End Region ' Constructor

#Region " Form overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            Me.m_bInUpdate = True

            ' Fix for disappearing toolstrips
            ' http://stackoverflow.com/questions/57208/toolstrips-in-tabpages-frequently-disappear-from-windows-forms-designer
            Me.m_tsB.Visible = True
            Me.m_tsPB.Visible = True
            Me.m_tsQB.Visible = True
            Me.m_tsEE.Visible = True
            Me.m_tsBA.Visible = True
            Me.m_tsBaBi.Visible = True

            Me.m_tsbnShowGroups.Image = SharedResources.fish
            Me.m_tsbnShowBestOnly.Image = SharedResources.FilterHS

            If (Me.UIContext Is Nothing) Then Return

            ' Add any initialization after the InitializeComponent() call.
            Me.m_mcmanager = Me.Core.EcosimMonteCarlo
            Me.m_mcmanager.Load()

            For Each par As eMCParams In Me.m_mcmanager.SupportedVariables
                Me.m_clbEnabledVariables.Items.Add(par)
            Next

            For Each method As eMCDietSamplingMethod In [Enum].GetValues(GetType(eMCDietSamplingMethod))
                Me.m_tscmbMethodDC.Items.Add(method)
            Next
            Me.m_tscmbMethodDC.SelectedItem = Me.m_mcmanager.DietSamplingMethod

            'set the call back delegates for the monte carlo trials and ecopath iteration
            ' ToDo: replace time step handlers with events to allow simulatenous use by other tools
            Me.m_mcmanager.MonteCarloStepHandler = AddressOf MonteCarloStepHandler
            Me.m_mcmanager.MonteCarloEcopathStepHandler = AddressOf Me.MonteCarloEcopathStepHandler
            Me.m_mcmanager.MonteCarloCompletedHandler = AddressOf Me.MonteCarloCompletedHandler
            Me.m_mcmanager.EcosimTimeStepHandler = AddressOf Me.EcoSimTimeStepHandler
            Me.m_mcmanager.SyncObject = Me

            Me.m_fpNumTrials = New cEwEFormatProvider(Me.UIContext, Me.m_nudNumTrials, GetType(Integer))
            Me.m_fpNumTrials.Value = m_mcmanager.nTrials

            Me.m_fpTrial = New cEwEFormatProvider(Me.UIContext, Me.m_lblTrialValue, GetType(Integer))
            Me.m_fpTrial.Value = 0

            Me.m_fpERun = New cEwEFormatProvider(Me.UIContext, Me.m_lblERunValue, GetType(Integer))
            Me.m_fpERun.Value = 0

            Me.m_fpSSorg = New cEwEFormatProvider(Me.UIContext, Me.m_lblSSorgValue, GetType(Single))
            Me.m_fpSSorg.Value = Me.m_mcmanager.SSorg

            Me.m_fpSS = New cEwEFormatProvider(Me.UIContext, Me.m_lblSScurrValue, GetType(Single))
            Me.m_fpSS.Value = 0.0!

            Me.m_fpSSBest = New cEwEFormatProvider(Me.UIContext, Me.m_lblSSbestValue, GetType(Single))
            Me.m_fpSSBest.Value = 0.0!

            Me.m_fpEETol = New cEwEFormatProvider(Me.UIContext, Me.m_tbxEETol, GetType(Single))
            Me.m_fpEETol.Value = Me.m_mcmanager.EcopathEETolerance
            AddHandler Me.m_fpEETol.OnValueChanged, AddressOf OnEETolChanged

            Me.m_fpFMratio = New cEwEFormatProvider(Me.UIContext, Me.m_tbxFMratio, GetType(Single))
            Me.m_fpFMratio.Value = Me.m_mcmanager.FMRatioForSRA
            AddHandler Me.m_fpFMratio.OnValueChanged, AddressOf OnFMratioChanged

            ' me.m_mcManager.UseFishingPattern = cbRetainCurPattern.Checked
            Me.m_mcmanager.RetainFits = m_cbRetainEstimates.Checked

            'Set the interface checkbox with the value from the core
            Me.m_cbSave.Checked = Me.m_mcmanager.IsSaveOutput

            Me.m_plothelper = New cEcosimOutputPlotHelper()
            Me.m_plothelper.Attach(Me.UIContext, Me.m_graph)
            Me.m_plothelper.ShowMultipleRuns = True
            Me.m_plothelper.ConfigurePane(SharedResources.HEADER_MCTRIALS, SharedResources.HEADER_TIME, SharedResources.HEADER_BIOMASS, False)
            Me.m_plothelper.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.MaxOnly

            ' Configure grids
            Me.m_gridB.UIContext = Me.UIContext
            Me.m_gridBA.UIContext = Me.UIContext
            Me.m_gridBaBi.UIContext = Me.UIContext
            Me.m_gridEE.UIContext = Me.UIContext
            Me.m_gridPB.UIContext = Me.UIContext
            Me.m_gridQB.UIContext = Me.UIContext
            Me.m_gridDiets.UIContext = Me.UIContext
            Me.m_gridLandings.UIContext = Me.UIContext
            Me.m_gridDiscards.UIContext = Me.UIContext
            Me.m_gridBestFit.UIContext = Me.UIContext

            Me.m_cmdRunMonteCarlo = New cCommand(Me.CommandHandler, "RunMonteCarlo")
            Me.m_cmdRunMonteCarlo.AddControl(Me.m_btnRunTrials)

            Me.m_cmdStopMonteCarlo = New cCommand(Me.CommandHandler, "StopMonteCarlo")
            Me.m_cmdStopMonteCarlo.AddControl(Me.m_btnStop)

            Me.m_cmdLoadTS = Me.CommandHandler.GetCommand("LoadTimeSeries")
            Me.m_cmdLoadTS.AddControl(Me.m_btnTS)

            Me.m_propNYears = New cSingleProperty(Me.Core.EcoSimModelParameters, eVarNameFlags.EcoSimNYears)
            AddHandler Me.m_propNYears.PropertyChanged, AddressOf OnPropNumYearsChanged

            Me.m_lbGroups.Attach(Me.UIContext)
            Me.m_lbGroups.SelectedIndex = 0

            Me.m_tcMain.SelectedTab = Me.m_tbpB

            Me.m_tsbnShowGroups.Checked = Not Me.m_spPlot.Panel2Collapsed

            Me.m_qeB.Attach(Me.m_gridB, Me.UIContext, Me.m_tsB, False)
            Me.m_qePB.Attach(Me.m_gridPB, Me.UIContext, Me.m_tsPB, False)
            Me.m_qeQB.Attach(Me.m_gridQB, Me.UIContext, Me.m_tsQB, False)
            Me.m_qeEE.Attach(Me.m_gridEE, Me.UIContext, Me.m_tsEE, False)
            Me.m_qeBA.Attach(Me.m_gridBA, Me.UIContext, Me.m_tsBA, False)
            Me.m_qeBaBi.Attach(Me.m_gridBaBi, Me.UIContext, Me.m_tsBaBi, False)
            Me.m_qeDC.Attach(Me.m_gridDiets, Me.UIContext, Me.m_tsDiets, False)
            Me.m_qeLandings.Attach(Me.m_gridLandings, Me.UIContext, Me.m_tsLandings, False)
            Me.m_qeDiscards.Attach(Me.m_gridDiscards, Me.UIContext, Me.m_tsDiscards, False)

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSim, eCoreComponentType.EcoSimMonteCarlo, eCoreComponentType.Core}

            For Each var As eMCParams In Me.m_mcmanager.SupportedVariables
                Dim bIsEnabled As Boolean = Me.m_mcmanager.Enable(var)
                Dim i As Integer = Me.m_clbEnabledVariables.Items.IndexOf(var)
                If (i >= 0) Then
                    Me.m_clbEnabledVariables.SetItemChecked(i, bIsEnabled)
                End If
                Me.UpdateUI(var)
            Next

            For i As Integer = 1 To Me.m_mcmanager.nResultWriters
                Dim writer As IMonteCarloResultsWriter = Me.m_mcmanager.ResultWriter(i)
                Me.m_cmbSaveFormat.Items.Add(Me.m_mcmanager.ResultWriter(i))
                If (ReferenceEquals(Me.m_mcmanager.ActiveResultWriter, writer)) Then
                    Me.m_cmbSaveFormat.SelectedItem = writer
                End If
            Next
            If (Me.m_cmbSaveFormat.SelectedIndex = -1) Then Me.m_cmbSaveFormat.SelectedIndex = 0

            Me.m_bInUpdate = False

            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If (Me.UIContext Is Nothing) Then Return

            Me.CoreComponents = Nothing

            Try
                Me.StopRun()

                ' -- detach from manager --
                Me.m_mcmanager.MonteCarloStepHandler = Nothing
                Me.m_mcmanager.MonteCarloEcopathStepHandler = Nothing
                Me.m_mcmanager.MonteCarloCompletedHandler = Nothing
                Me.m_mcmanager.EcosimTimeStepHandler = Nothing
                Me.m_mcmanager.SyncObject = Nothing
                'Me.m_mcmanager.ActiveResultWriter = Nothing

                ' -- cleanup grids  --
                Me.m_qeB.Detach()
                Me.m_qePB.Detach()
                Me.m_qeQB.Detach()
                Me.m_qeEE.Detach()
                Me.m_qeBA.Detach()
                Me.m_qeBaBi.Detach()
                Me.m_qeLandings.Detach()
                Me.m_qeDiscards.Detach()

                Me.m_gridB.UIContext = Nothing
                Me.m_gridBA.UIContext = Nothing
                Me.m_gridBaBi.UIContext = Nothing
                Me.m_gridEE.UIContext = Nothing
                Me.m_gridPB.UIContext = Nothing
                Me.m_gridQB.UIContext = Nothing
                Me.m_gridDiets.UIContext = Nothing
                Me.m_gridLandings.UIContext = Nothing
                Me.m_gridDiscards.UIContext = Nothing
                Me.m_gridBestFit.UIContext = Nothing

                ' -- cleanup commands --
                Me.CommandHandler.Remove(Me.m_cmdRunMonteCarlo)
                Me.m_cmdRunMonteCarlo.RemoveControl(Me.m_btnRunTrials)
                Me.m_cmdRunMonteCarlo = Nothing

                Me.CommandHandler.Remove(Me.m_cmdStopMonteCarlo)
                Me.m_cmdStopMonteCarlo.RemoveControl(Me.m_btnStop)
                Me.m_cmdStopMonteCarlo = Nothing

                Me.m_cmdLoadTS.RemoveControl(Me.m_btnTS)
                Me.m_cmdLoadTS = Nothing

                ' -- controls and format providers --
                Me.m_lbGroups.Detach()

                Me.m_fpNumTrials.Release()
                Me.m_fpTrial.Release()
                Me.m_fpERun.Release()
                Me.m_fpSSorg.Release()
                Me.m_fpSS.Release()
                Me.m_fpSSBest.Release()
                RemoveHandler Me.m_fpEETol.OnValueChanged, AddressOf OnEETolChanged
                Me.m_fpEETol.Release()
                RemoveHandler Me.m_fpFMratio.OnValueChanged, AddressOf OnFMratioChanged
                Me.m_fpFMratio.Release()

                Me.m_plothelper.Detach()
                Me.m_plothelper = Nothing

                ' -- local properties
                RemoveHandler Me.m_propNYears.PropertyChanged, AddressOf OnPropNumYearsChanged
                Me.m_propNYears = Nothing

            Catch ex As Exception
                Debug.Assert(False)
            End Try

            MyBase.OnFormClosed(e)

        End Sub

        Public Overrides ReadOnly Property IsRunForm As Boolean
            Get
                Return True
            End Get
        End Property

#End Region ' Form overrides

#Region " Events "

        Private Sub OnSaveFormatSelected(sender As Object, e As EventArgs) _
            Handles m_cmbSaveFormat.SelectedIndexChanged
            Try
                Me.m_mcmanager.ActiveResultWriter = DirectCast(Me.m_cmbSaveFormat.SelectedItem, IMonteCarloResultsWriter)
            Catch ex As Exception
            End Try
        End Sub


        Private Sub m_cmbSaveFormat_Format(sender As Object, e As ListControlConvertEventArgs) _
            Handles m_cmbSaveFormat.Format
            Try
                e.Value = DirectCast(e.ListItem, IMonteCarloResultsWriter).DisplayName
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnStop(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_btnStop.Click
            Try
                Me.StopRun()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnApply(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnApply.Click
            If Not Me.m_mcmanager Is Nothing Then
                Try
                    Me.m_mcmanager.ApplyBestFits()
                Catch ex As Exception

                End Try
            End If
        End Sub

        Private Sub OnSelectSamplingMethod(sender As Object, e As EventArgs) _
            Handles m_tscmbMethodDC.SelectedIndexChanged
            If Not Me.m_mcmanager Is Nothing Then
                Try
                    Me.m_mcmanager.DietSamplingMethod = DirectCast(Me.m_tscmbMethodDC.SelectedItem, eMCDietSamplingMethod)
                    Me.UpdateControls()
                    Me.m_gridDiets.RefreshContent()
                Catch ex As Exception

                End Try
            End If
        End Sub


        'Private Sub cbRetainCurPattern_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        '    Handles m_cbRetainCurPattern.CheckedChanged, m_cbSRA.CheckedChanged
        '    If Not Me.m_mcmanager Is Nothing Then
        '        ' me.m_mcManager.UseFishingPattern = cbRetainCurPattern.Checked
        '    End If
        'End Sub

        Private Sub OnToggleRetainEstimates(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbRetainEstimates.CheckedChanged
            If Not Me.m_mcmanager Is Nothing Then
                Try
                    Me.m_mcmanager.RetainFits = Me.m_cbRetainEstimates.Checked
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnNumTrialsChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_nudNumTrials.ValueChanged
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.nTrials = CInt(Me.m_nudNumTrials.Value)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnAutosaveToggled(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_cbSave.CheckedChanged
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.IsSaveOutput = Me.m_cbSave.Checked
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnLoadBFromPedigree(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnLoadPedB.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.LoadFromPedigree(eVarNameFlags.BiomassAreaInput)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnLoadPBFromPedigree(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnLoadPedPB.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.LoadFromPedigree(eVarNameFlags.PBInput)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnLoadQBFromPedigree(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnLoadPedQB.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.LoadFromPedigree(eVarNameFlags.QBInput)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnLoadDietsFromPedigree(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnLoadPedDC.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.LoadFromPedigree(eVarNameFlags.DietComp)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnLoadLandingsFromPedigree(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnLoadPedLandings.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.LoadFromPedigree(eVarNameFlags.TCatchInput)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnLoadDiscardsFromPedigree(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnLoadPedDiscards.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.LoadFromPedigree(eVarNameFlags.TCatchInput)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub m_cbSRA_CheckedChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_cbSRA.CheckedChanged
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.IncludeFpenalty = Me.m_cbSRA.Checked
                    Me.UpdateControls()
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnFMratioChanged(ByVal sender As Object, ByVal e As System.EventArgs)
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.FMRatioForSRA = CSng(Me.m_fpFMratio.Value)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnEETolChanged(ByVal sender As Object, ByVal e As System.EventArgs)
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.EcopathEETolerance = CSng(Me.m_fpEETol.Value)
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnDefaultTol(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btDefaultTol.Click
            If Me.m_mcmanager IsNot Nothing Then
                Try
                    Me.m_mcmanager.setDefaultTol()
                    Me.m_fpEETol.Value = Me.m_mcmanager.EcopathEETolerance
                Catch ex As Exception
                End Try
            End If
        End Sub

        Private Sub OnShowBetterRunsCheckChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnShowBestOnly.CheckedChanged
            Try
                Me.m_bShowBetterSS = Me.m_tsbnShowBestOnly.Checked
                Me.ToggleLineViz()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnShowGroupsCheckChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnShowGroups.CheckedChanged
            If (Me.m_bInUpdate) Then Return
            Me.UpdateControls()
        End Sub

        Private Sub OnFormatVariable(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_clbEnabledVariables.Format

            Dim fmt As New cVarnameTypeFormatter()

            Select Case DirectCast(e.ListItem, eMCParams)
                Case eMCParams.BA : e.Value = fmt.GetDescriptor(eVarNameFlags.BioAccumOutput)
                Case eMCParams.BaBi : e.Value = fmt.GetDescriptor(eVarNameFlags.BioAccumRate)
                Case eMCParams.Biomass : e.Value = fmt.GetDescriptor(eVarNameFlags.Biomass)
                Case eMCParams.Diets : e.Value = fmt.GetDescriptor(eVarNameFlags.DietComp)
                Case eMCParams.Discards : e.Value = fmt.GetDescriptor(eVarNameFlags.Discards)
                Case eMCParams.Landings : e.Value = fmt.GetDescriptor(eVarNameFlags.Landings)
                Case eMCParams.EE : e.Value = fmt.GetDescriptor(eVarNameFlags.EEInput)
                Case eMCParams.PB : e.Value = fmt.GetDescriptor(eVarNameFlags.PBInput)
                Case eMCParams.QB : e.Value = fmt.GetDescriptor(eVarNameFlags.QBInput)
                Case Else
                    Debug.Assert(False, "variable not supported")
            End Select

        End Sub

        Private Sub OnItemCheckChanged(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
            Handles m_clbEnabledVariables.ItemCheck

            If (Me.m_bInUpdate) Then Return

            Dim par As eMCParams = DirectCast(Me.m_clbEnabledVariables.Items(e.Index), eMCParams)
            Me.m_mcmanager.Enable(par) = (e.NewValue = CheckState.Checked)
            Me.UpdateUI(par)

        End Sub

#End Region ' Events

#Region " MC Run callbacks "

        Private Sub MonteCarloStepHandler()

            Try
                ' Be conservative in providing status feedback
                'If (Me.m_mcmanager.nTrialIterations Mod cCore.N_MONTHS = 0) Then
                cApplicationStatusNotifier.UpdateProgress(Me.Core,
                                                          My.Resources.STATUS_SEARCH_SEARCHING,
                                                          Me.m_mcmanager.nTrialIterations / Me.m_mcmanager.nTrials)
                'End If

                Me.m_fpTrial.Value = Me.m_mcmanager.nTrialIterations
                Me.m_fpSS.Value = Me.m_mcmanager.SS
                Me.m_fpSSBest.Value = Me.m_mcmanager.SSBestFit

                'this will draw the currently loaded data
                Me.UpdateGraphHighlights()

                'get ready for the next run if there isn't one then on big deal the data will not be used
                Me.NewIteration()

            Catch ex As Exception
                Debug.Assert(False, ex.StackTrace)
            End Try

        End Sub

        Private Sub MonteCarloEcopathStepHandler()

            Try
                Me.m_fpERun.Value = Me.m_mcmanager.nEcopathIterations
            Catch ex As Exception
                Debug.Assert(False, ex.StackTrace)
            End Try

        End Sub

        Private Sub MonteCarloCompletedHandler()

            cApplicationStatusNotifier.EndProgress(Me.Core)

            Me.m_nTrials = 0

            Try
                'Show the Best Fit if there is time series loaded
                If Me.Core.HasAppliedTimeSeries() Then
                    'populate the grid with new values (biomass....)
                    Me.m_gridBestFit.RefreshContent()

                    ' Select outputs
                    Me.m_tcMain.SelectedTab = m_tbpBestTrial
                End If

            Catch ex As Exception
                Debug.Assert(False, ex.StackTrace)
            End Try
            Me.UpdateControls()

        End Sub

        ''' <summary>
        ''' Time Step handler for Ecosim results
        ''' </summary>
        ''' <remarks>This will be called at each ecosim timestep for plotting the data</remarks>
        Private Sub EcoSimTimeStepHandler(ByVal lTime As Long, ByVal results As cEcoSimResults)

            Dim ppl As PointPairList = Nothing

            If (Me.m_lpplIteration.Count = 0) Then Return
            '  System.Console.WriteLine("Interface Ecosim handler Start.")
            Try

                ' Store results
                For iGroup As Integer = 1 To Me.Core.nLivingGroups
                    ppl = Me.m_lpplIteration(iGroup - 1)
                    ppl.Add(New PointPair(Me.Core.EcosimFirstYear + CSng(lTime / cCore.N_MONTHS), results.Biomass(iGroup)))
                    Me.m_sYMax = Math.Max(Me.m_sYMax, results.Biomass(iGroup) * 1.1!)
                Next

            Catch ex As Exception

            End Try
            '  System.Console.WriteLine("Interface Ecosim handler End.")
        End Sub

#End Region ' MC Run callbacks

#Region " Run Trials "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Command handler; executes the 
        ''' <see cref="m_cmdRunMonteCarlo">Run Monte Carlo command</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub m_cmdRunMonteCarlo_OnInvoke(ByVal cmd As cCommand) _
            Handles m_cmdRunMonteCarlo.OnInvoke

            Dim bCheckTimeseries As Boolean = True
            While bCheckTimeseries

                If Me.Core.HasAppliedTimeSeries() Then
                    bCheckTimeseries = False

                ElseIf Not Me.Core.HasAppliedTimeSeries() Then

                    Dim fmsg As New cFeedbackMessage(My.Resources.MONTECARLO_PROMPT_RUNWITHOUTTS, eCoreComponentType.EcoSimMonteCarlo,
                                                     eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                    fmsg.Reply = eMessageReply.NO
                    Me.Core.Messages.SendMessage(fmsg)

                    Select Case fmsg.Reply
                        Case eMessageReply.YES
                            ' Continue execution without time series
                            bCheckTimeseries = False

                        Case eMessageReply.NO
                            ' Load time series
                            Me.m_cmdLoadTS.Invoke()

                        Case eMessageReply.CANCEL
                            ' Abort attempt to run
                            Return
                    End Select
                End If

            End While

            Me.m_fpSSorg.Value = Me.m_mcmanager.SSorg
            Me.m_fpTrial.Value = 0
            Me.m_fpERun.Value = 0
            Me.m_fpSS.Value = 0.0!
            Me.m_fpSSBest.Value = 0.0!
            Me.m_sYMax = 1.0!

            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_SEARCH_INITIALIZING, -1.0)

            ' Clear out the old data
            Me.m_plothelper.Clear()
            Me.m_plothelper.XScaleMax = Me.Core.EcosimFirstYear + Me.Core.nEcosimYears

            Me.Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf StopRun))
            Me.NewIteration()
            Me.m_mcmanager.Run()

            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Command update handler; enables and disables the 
        ''' <see cref="m_cmdRunMonteCarlo">Run Monte Carlo command</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub m_cmdRunMonteCarlo_OnUpdate(ByVal cmd As cCommand) _
            Handles m_cmdRunMonteCarlo.OnUpdate

            cmd.Enabled = Me.Core.StateMonitor.HasEcosimLoaded() And
                          Not Me.m_mcmanager.IsRunning

            If Me.Core.HasAppliedTimeSeries() Then
                ' JS 11dec07: is this necessary?
                Me.m_fpSSorg.Value = Me.m_mcmanager.SSorg
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Command update handler; enables and disables the 
        ''' <see cref="m_cmdStopMonteCarlo">Stop Monte Carlo command</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub m_cmdStopMonteCarlo_OnUpdate(ByVal cmd As cCommand) Handles m_cmdStopMonteCarlo.OnUpdate
            cmd.Enabled = Me.m_mcmanager.IsRunning
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The Apply time series Command/button has been invoked
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub m_cmdApplyTS_OnPostInvoke(ByVal cmd As cCommand) _
            Handles m_cmdLoadTS.OnPostInvoke
            'this means the time series data could have changed
            'reload the data into the manager
            'jb 14-Mar-2011 MonteCarlo manager does not need to reload if timeseries is loaded
            'In fact, this will overwrite user edited Parameter Limit values
            'Me.m_mcmanager.Load()
            Me.UpdateGraphXAxis()
        End Sub

        Private Sub OnPropNumYearsChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)
            Me.UpdateGraphXAxis()
        End Sub

        Private Sub m_lbGroups_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_lbGroups.SelectedIndexChanged
            Me.UpdateGraphHighlights()
        End Sub

        Private Sub StopRun()
            Try
                If Me.m_mcmanager.IsRunning Then
                    Me.m_mcmanager.StopRun(0)
                    Me.Core.SetStopRunDelegate(Nothing)
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub UpdateGraphXAxis()

            Dim pane As GraphPane = Me.m_plothelper.GetPane(1)

            With pane.XAxis.Scale
                .Min = Me.Core.EcosimFirstYear
                .Max = Me.Core.EcoSimModelParameters.NumberYears + Me.Core.EcosimFirstYear
                .MaxAuto = False
                .MinAuto = False
            End With

            Me.m_graph.AxisChange()
        End Sub

        Private Sub UpdateGraphHighlights()

            'Only Highlight if the graphs are drawing
            If Me.m_tsbnUpdatePlot.Checked Then

                ' Start setting highlights
                Me.m_plothelper.ClearHighlights()

                For Each i As Integer In Me.m_lbGroups.SelectedIndices
                    Me.m_plothelper.Highlight(Me.m_lbGroups.GroupIndex(i), -1)
                Next

                Me.m_plothelper.YScaleMax = Me.m_sYMax
                Me.m_plothelper.Redraw()

            End If

        End Sub

#End Region ' Run Trials

#Region " Internals "

        Public Overrides Sub OnCoreMessage(msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            If (msg.Source = eCoreComponentType.Core And msg.Type = eMessageType.GlobalSettingsChanged) Then
                Me.m_cbSave.Checked = Me.m_mcmanager.IsSaveOutput
            End If
        End Sub

        ''' <summary>
        ''' Update the UI to reflect that the <see cref="cMonteCarloManager.Enable">enabled state</see>
        ''' of a given <see cref="eMCParams">parameter</see> has changed.
        ''' </summary>
        ''' <param name="par"></param>
        Private Sub UpdateUI(par As eMCParams)

            Dim bEnabled As Boolean = Me.m_mcmanager.Enable(par)
            Select Case par
                Case eMCParams.BA
                    Me.m_tcMain.IsVisible(Me.m_tbpBA) = bEnabled
                Case eMCParams.BaBi
                    Me.m_tcMain.IsVisible(Me.m_tbpBABi) = bEnabled
                Case (eMCParams.Biomass)
                    Me.m_tcMain.IsVisible(Me.m_tbpB) = bEnabled
                Case eMCParams.Diets
                    ' ToDo: consider adding a DietsBestFit tab, and show/hiding it here
                    Me.m_tcMain.IsVisible(Me.m_tbpDiets) = bEnabled
                Case eMCParams.Discards
                    ' ToDo: consider adding a FisheriesBestFit tab, and show/hiding it here
                    Me.m_tcMain.IsVisible(Me.m_tbpDiscards) = bEnabled
                Case eMCParams.Landings
                    ' ToDo: consider adding a FisheriesBestFit tab, and show/hiding it here
                    Me.m_tcMain.IsVisible(Me.m_tbpLandings) = bEnabled
                Case eMCParams.EE
                    Me.m_tcMain.IsVisible(Me.m_tbpEE) = bEnabled
                Case eMCParams.PB
                    Me.m_tcMain.IsVisible(Me.m_tbpPB) = bEnabled
                Case eMCParams.QB
                    Me.m_tcMain.IsVisible(Me.m_tbpQB) = bEnabled
                Case Else
                    Debug.Assert(False, "variable not supported")

            End Select
        End Sub

        Private Sub NewIteration()

            Dim lLines As New List(Of LineItem)
            Dim line As LineItem = Nothing

            Me.m_nTrials += 1
            Me.m_plothelper.CreateRun(cStringUtils.Localize(SharedResources.GENERIC_VALUE_ITERATION, Me.m_nTrials))
            Me.m_lpplIteration.Clear()

            If (Me.m_tsbnUpdatePlot.Checked) Then

                For iGroup As Integer = 1 To Me.Core.nLivingGroups
                    Me.m_lpplIteration.Add(New PointPairList())
                Next

                For iGroup As Integer = 1 To Me.Core.nLivingGroups
                    Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
                    Dim strGroupName As String = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, iGroup, group.Name)
                    'Dim strTrialLabel As String = cStringUtils.Localize(My.Resources.GENERIC_LABEL_TRIAL, Me.m_nTrials, strGroupName)
                    line = Me.m_plothelper.CreateLine(group, Me.m_lpplIteration(iGroup - 1), strGroupName)
                    Me.m_plothelper.Metadata(line, "SS") = Me.m_mcmanager.SS

                    line.IsVisible = Not m_bShowBetterSS Or (Me.m_mcmanager.SS < Me.m_mcmanager.SSorg)
                    lLines.Add(line)
                Next iGroup

            End If

            Me.m_plothelper.YScaleMax = Me.m_sYMax
            Me.m_plothelper.PlotLines(lLines.ToArray, 1, True, False)

        End Sub

        Private Sub ToggleLineViz()

            For Each ci As CurveItem In Me.m_plothelper.DataLines()
                Dim sSS As Single = CSng(Me.m_plothelper.Metadata(ci, "SS"))
                ci.IsVisible = Not m_bShowBetterSS Or (sSS < Me.m_mcmanager.SSorg)
            Next
            Me.m_plothelper.RescaleAndRedraw()

        End Sub

        Private m_bInUpdate As Boolean = False

        Protected Overrides Sub UpdateControls()
            MyBase.UpdateControls()

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_mcmanager Is Nothing) Then Return
            If (Me.m_bInUpdate) Then Return

            Me.m_bInUpdate = True

            Dim bIsBusy As Boolean = Me.Core.StateMonitor.IsBusy

            Me.m_spPlot.Panel2Collapsed = Not Me.m_tsbnShowGroups.Checked
            Me.m_btnApply.Enabled = Not bIsBusy And (Me.m_mcmanager.SSBestFit < Me.m_mcmanager.SSorg)
            Me.m_cbRetainEstimates.Enabled = Not bIsBusy
            Me.m_nudNumTrials.Enabled = Not bIsBusy
            Me.m_btnTS.Enabled = Not bIsBusy
            Me.m_cbSave.Enabled = Not bIsBusy
            Me.m_cmbSaveFormat.Enabled = Not bIsBusy
            Me.m_cbSRA.Enabled = Not bIsBusy

            Me.m_fpFMratio.Style = If(Me.m_mcmanager.IncludeFpenalty, cStyleGuide.eStyleFlags.OK, cStyleGuide.eStyleFlags.NotEditable)
            Me.m_tsbnLoadPedDC.Visible = (Me.m_mcmanager.DietSamplingMethod = eMCDietSamplingMethod.NormalDistribution)
            Me.m_bInUpdate = False

        End Sub

#End Region ' Internals

    End Class

End Namespace
