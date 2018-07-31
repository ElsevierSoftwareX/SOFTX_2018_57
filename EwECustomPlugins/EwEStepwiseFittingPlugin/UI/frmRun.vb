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
'    UBC Centre for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
'
' Stepwise Fitting Procedure by Sheila Heymans, Erin Scott, Jeroen Steenbeek
' Copyright 2015- Scottish Association for Marine Science, Oban, Scotland
'
' Erin Scott was funded by the Scottish Informatics and Computer Science
' Alliance (SICSA) Postgraduate Industry Internship Programme.
' ===============================================================================
'
#Region " Imports "

Option Strict On
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class frmRun

#Region " Private vars "

    Private Enum eRunMode As Integer
        NotSet = 0
        CommandLine
        StandAlone
        Plugin
    End Enum

    Private m_engine As cSFPManager = Nothing
    Private m_plugin As cSFPPluginPoint = Nothing
    Private m_runmode As eRunMode = eRunMode.NotSet
    Private m_bInUpdate As Boolean = False

    Private WithEvents m_cmdLoadTS As cCommand = Nothing

#End Region ' Private vars

#Region " Construction "

    Public Sub New(ByVal uic As cUIContext, ByVal engine As cSFPManager)

        MyBase.New()

        Me.InitializeComponent()
        Me.UIContext = uic
        Me.m_engine = engine

        Me.Text = My.Resources.CAPTION
        Me.TabText = Me.Text

        Me.m_runmode = eRunMode.StandAlone

    End Sub

    Public Sub New(ByVal uic As cUIContext, ByVal engine As cSFPManager, ByVal plugin As cSFPPluginPoint)

        Me.New(uic, engine)

        Me.m_plugin = plugin
        Me.m_runmode = eRunMode.Plugin

    End Sub

#End Region ' Construction

#Region " Overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_grid.UIContext = Me.UIContext
        Me.m_grid.Initialize(Me.m_engine)

        ' Populate controls
        Me.m_nudStepSize.Value = Me.m_engine.AnomalySearchSplineStepSize
        Me.m_cmbAutoSave.SelectedIndex = Me.m_engine.Parameters.AutosaveMode
        Me.m_rbPredator.Checked = (Me.m_engine.Parameters.PredOrPredPreySSToV = True)
        Me.m_rbPredPrey.Checked = (Me.m_engine.Parameters.PredOrPredPreySSToV = False)

        ' -- Handle run modes
        Select Case Me.m_runmode

            Case eRunMode.NotSet
                Debug.Assert(False)

            Case eRunMode.StandAlone
                Me.PopulateModelControls()
                Me.PopulateModelDropdowns()
                If Me.Core.ActiveEcosimScenarioIndex > 0 Then Me.SelectedEcosimScenario = Me.Core.EcosimScenarios(Me.Core.ActiveEcosimScenarioIndex)
                If Me.Core.ActiveTimeSeriesDatasetIndex > 0 Then Me.SelectedTimeSeries = Me.Core.TimeSeriesDataset(Me.Core.ActiveTimeSeriesDatasetIndex)
                'Hide the time series and apply button in standalone mode
                Me.m_btnTS.Hide()
                Me.m_btnApply.Hide()

            Case eRunMode.Plugin

                Me.PopulateAnomalyDropdown()

                ' Connect to ApplyTS command to time series button
                Me.m_cmdLoadTS = Me.CommandHandler.GetCommand("LoadTimeSeries")
                If Me.m_cmdLoadTS IsNot Nothing Then Me.m_cmdLoadTS.AddControl(Me.m_btnTS)

                ' Bring SFP manager up to date to running core
                Me.m_engine.UpdateToCore()
                Me.m_grid.RefreshContent()
                Me.m_engine.Parameters.CustomOutputFolder = ""

        End Select

        AddHandler Me.m_engine.OnIterationCompleted, AddressOf OnIterationCompleted

        Me.UpdateControls()

        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.MediatedInteractionManager}

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        ' Cleanup
        RemoveHandler Me.m_engine.OnIterationCompleted, AddressOf OnIterationCompleted

        'Detach time series load command from time series button
        If Me.m_cmdLoadTS IsNot Nothing Then Me.m_cmdLoadTS.RemoveControl(Me.m_btnTS)

        ' Done
        My.Settings.Save()
        MyBase.OnFormClosed(e)

    End Sub

    Protected Overrides Sub OnFormClosing(ByVal e As System.Windows.Forms.FormClosingEventArgs)
        MyBase.OnFormClosing(e)
    End Sub

    Public Overrides Sub OnCoreMessage(msg As EwECore.cMessage)
        MyBase.OnCoreMessage(msg)

        Select Case msg.Source
            Case eCoreComponentType.MediatedInteractionManager
                Me.PopulateAnomalyDropdown()
        End Select

    End Sub

    Public Overloads Sub UpdateControls()
        MyBase.UpdateControls()

        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True

        Dim bHasTimeSeries As Boolean = (Me.m_engine.TSIndex >= 1)
        Dim bHasEnabledIterations As Boolean = False
        Dim bHasEnabledIterationSelected As Boolean = False
        Dim bHasCompletedIterationSelected As Boolean = False
        Dim bNeedsAnomalyShape As Boolean = False
        Dim bHasAnomalyShape As Boolean = (Me.SelectedShape IsNot Nothing)
        Dim bContainsAnomaly As Boolean = False
        Dim bContainsVul As Boolean = False
        Dim bIsRunning As Boolean = (Me.m_engine.IsRunning)
        Dim parms As cSFPParameters = Me.m_engine.Parameters

        Dim it As ISFPIterations = Nothing
        For Each it In Me.m_engine.Iterations
            bContainsAnomaly = bContainsAnomaly Or (it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPAnomalySearch)) Or
                                           (it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPVandASearch))
            bContainsVul = bContainsVul Or (it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPVulnerabilitySearch)) Or
                                           (it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPVandASearch))
        Next

        it = Me.SelectedIteration
        If (it IsNot Nothing) Then
            bHasCompletedIterationSelected = (it.RunState = ISFPIterations.eRunState.Completed)
            bHasEnabledIterationSelected = it.Enabled
            bNeedsAnomalyShape = bNeedsAnomalyShape Or (it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPAnomalySearch)) Or
                                           (it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPVandASearch))
        End If

        Dim bAnomalyOk As Boolean = Not bNeedsAnomalyShape Or bHasAnomalyShape

        For Each it In Me.m_engine.Iterations
            bHasEnabledIterations = bHasEnabledIterations Or it.Enabled
        Next

        Try
            Me.m_nudK.Enabled = (parms.MaxK > parms.MinK)
            Me.m_nudK.Minimum = parms.MinK
            Me.m_nudK.Maximum = parms.MaxK
            Me.m_nudK.Value = parms.K
        Catch ex As Exception

        End Try

        ' -- Entire UI --
        Me.m_tlpContent.Enabled = Not bIsRunning

        ' -- Model panel --
        Me.m_plModel.Visible = (Me.m_runmode = eRunMode.StandAlone)
        Me.m_plModel.Enabled = (Me.m_runmode = eRunMode.StandAlone)
        Me.m_cmbScenario.Enabled = Me.Core.StateMonitor.HasEcopathLoaded
        Me.m_cmbTimeSeries.Enabled = Me.Core.StateMonitor.HasEcosimLoaded

        ' -- Parameters panel --
        Me.m_btnSelectA.Enabled = bContainsAnomaly
        Me.m_btnSelectV.Enabled = bContainsVul
        Me.m_btnSelectVandA.Enabled = bContainsVul Or bContainsAnomaly
        Me.m_btnApply.Enabled = bHasCompletedIterationSelected And bHasEnabledIterationSelected

        ' -- Run panel --

        Me.m_cmbAutoSave.SelectedIndex = Me.m_engine.Parameters.AutosaveMode

        ' Update output path entirely to resolve path placeholders
        Me.m_tbxOutputFolder.Text = Me.m_engine.OutputFolder

        'Run button enabled when at least one iteration is enabled, time series are loaded, and anomaly search is set up ok
        Me.m_btnRun.Enabled = bHasEnabledIterations And bHasTimeSeries And bAnomalyOk

        'Enable Absolute Biomass time series check box when time series are loaded
        Me.m_cbEnableAbsBioforBaseline.Enabled = bHasTimeSeries

        Me.m_bInUpdate = False

    End Sub

#End Region ' Overrides

#Region " Plug-in triggers "

    Public Sub OnTimeSeriesLoaded(tsd As cTimeSeriesDataset)

        ' This could very well be called while running
        If (Me.m_engine.IsRunning) Then
            Return
        End If

        Try
            Me.m_engine.UpdateToCore()
            Me.m_grid.RefreshContent()
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

#End Region ' Plug-in triggers

#Region " Control events "

    Private Sub OnSelectModel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectModel.Click

        If (Me.m_runmode <> eRunMode.StandAlone) Then Return

        Try
            ' Get a model file name from the user
            Dim strFileName As String = ShowSelectModelDialogue()
            Me.m_engine.LoadModel(strFileName)

        Catch ex As Exception

        End Try

        Me.PopulateModelControls()
        Me.PopulateModelDropdowns()

        Me.UpdateControls()

    End Sub

    Private Sub OnFormatScenario(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbScenario.Format

        Try
            e.Value = DirectCast(e.ListItem, cEcoSimScenario).Name
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnSelectedScenario(sender As Object, e As System.EventArgs) _
        Handles m_cmbScenario.SelectedIndexChanged

        If (Me.m_runmode <> eRunMode.StandAlone) Then Return

        Try
            Dim scenario As cEcoSimScenario = Me.SelectedEcosimScenario
            Dim isc As Integer = 0
            If (scenario IsNot Nothing) Then isc = scenario.Index
            Me.m_engine.LoadEcoSimScenario(isc)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Me.UpdateControls()

    End Sub

    Private Sub OnFormatTimeSeries(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbTimeSeries.Format

        Try
            e.Value = DirectCast(e.ListItem, cTimeSeriesDataset).Name
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnSelectedTimeseries(sender As Object, e As System.EventArgs) _
        Handles m_cmbTimeSeries.SelectedIndexChanged

        If (Me.m_runmode <> eRunMode.StandAlone) Then Return

        Try
            Dim ts As cTimeSeriesDataset = Me.SelectedTimeSeries
            Dim its As Integer = 0
            If (ts IsNot Nothing) Then its = ts.Index
            Me.m_engine.LoadTimeSeries(its)

            ' Repopulate the grid
            Me.m_grid.RefreshContent()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Me.UpdateControls()

    End Sub

    Private Sub OnFormatShape(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbAnomalyShape.Format

        Try
            Dim fmt As New ScientificInterfaceShared.Style.cShapeDataFormatter()
            e.Value = fmt.GetDescriptor(e.ListItem)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnShapeSelected(sender As System.Object, e As System.EventArgs) _
        Handles m_cmbAnomalyShape.SelectedIndexChanged

        Try
            Me.m_engine.Parameters.AppliedShape = Me.SelectedShape
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnSplinePointStepSizeChanged(sender As Object, e As System.EventArgs) _
        Handles m_nudStepSize.ValueChanged

        ' Safety catch: Numeric updown controls throw events on creation. Aargh.
        If (Me.m_engine Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Try
            Me.m_engine.AnomalySearchSplineStepSize = CInt(Me.m_nudStepSize.Value)
            Me.m_grid.RefreshContent()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.UpdateControls()

    End Sub

    Private Sub OnKChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_nudK.ValueChanged

        ' Safety catch: Numeric updown controls throw events on creation. Aargh.
        If (Me.m_engine Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return


        Try
            Me.m_engine.K = CInt(Me.m_nudK.Value)
            Me.m_grid.RefreshContent()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.UpdateControls()

    End Sub

    Private Sub OnSearchCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbPredator.CheckedChanged, _
                m_rbPredPrey.CheckedChanged

        Me.m_engine.Parameters.PredOrPredPreySSToV = Me.m_rbPredator.Checked
        Me.UpdateControls()

    End Sub

    Private Sub OnSelectAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectAll.Click

        Try
            ' Enable all iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                it.Enabled = True
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()

    End Sub

    Private Sub OnSelectNone(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectNone.Click, m_btnClearAll.Click

        Try
            ' Disable all iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                it.Enabled = False
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()

    End Sub

    Private Sub OnSelectVuls(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectV.Click
        Try
            ' Enable all Vulnerability iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                If it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPVulnerabilitySearch) Then
                    it.Enabled = True
                    'Else
                    '    it.Enabled = False
                End If
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()
    End Sub

    Private Sub OnSelectAnomaly(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectA.Click
        Try
            ' Enable all Anomaly iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                If it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPAnomalySearch) Then
                    it.Enabled = True
                    'Else
                    '    it.Enabled = False
                End If
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()
    End Sub

    Private Sub OnSelectVandA(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectVandA.Click
        Try
            ' Enable all V and A iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                If it.GetType Is GetType(EwEStepwiseFittingPlugin.cSFPVandASearch) Then
                    it.Enabled = True
                    'Else
                    '    it.Enabled = False
                End If
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()
    End Sub

    Private Sub OnSelectBaseline(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectBaseline.Click
        Try
            ' Enable Baseline iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                If it.BaseorFishValue Then
                    it.Enabled = True
                End If
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()
    End Sub

    Private Sub OnSelectFishin(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelectFishing.Click
        Try
            ' Enable Fishing iterations for running
            For Each it As ISFPIterations In Me.m_engine.Iterations
                If it.BaseorFishValue <> True Then
                    it.Enabled = True
                End If
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        Me.m_grid.UpdateContent()
    End Sub

    Private Sub OnIterationSelected() _
        Handles m_grid.OnSelectionChanged

        Try
            BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Load selected interation into the EwE user interface
    ''' </summary>
    Private Sub OnApplyIteration(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnApply.Click

        Try
            Dim iteration As ISFPIterations = Me.SelectedIteration
            If (iteration IsNot Nothing) Then
                iteration.Apply()
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Sub OnChooseOutputFolder(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnChooseFolder.Click

        Try
            Select Case Me.m_runmode
                Case eRunMode.NotSet
                    Debug.Assert(False)
                Case eRunMode.StandAlone
                    Dim dlg As New FolderBrowserDialog()
                    dlg.SelectedPath = Me.m_engine.Parameters.CustomOutputFolder
                    dlg.ShowNewFolderButton = True
                    If (dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
                        Me.m_engine.Parameters.CustomOutputFolder = dlg.SelectedPath
                        Me.UpdateControls()
                    End If
                Case eRunMode.Plugin
                    Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("AutosaveResults")
                    cmd.Invoke()
                    Me.UpdateControls()
            End Select
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnAutosaveOptionsChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_cmbAutoSave.SelectedIndexChanged

        ' Safety catch: some controls throw events on creation. Aargh.
        If (Me.m_engine Is Nothing) Then Return

        Try
            Me.m_engine.Parameters.AutosaveMode = CType(Me.m_cmbAutoSave.SelectedIndex, cSFPParameters.eAutosaveMode)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnEnableAbBioforBaselineChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_cbEnableAbsBioforBaseline.CheckedChanged

        ' Safety catch: some controls throw events on creation. Aargh.
        If (Me.m_engine Is Nothing) Then Return

        Try
            Me.m_engine.Parameters.EnableAbsoluteBiomass = Me.m_cbEnableAbsBioforBaseline.Checked
            Me.m_engine.Refresh(Me.m_engine.K)
            Me.m_grid.RefreshContent()
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnRun(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnRun.Click

        Try
            ' Run the Stepwise Fitting Procedure
            Me.m_engine.RunSFPIterationsThreaded()
            Me.m_grid.UpdateContent()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Private Delegate Sub OnIterationCompletedDelegate(ByVal sender As cSFPManager, ByVal iteration As ISFPIterations)

    Private Sub OnIterationCompleted(ByVal sender As cSFPManager, ByVal iteration As ISFPIterations)

        If Me.InvokeRequired Then
            Me.Invoke(New OnIterationCompletedDelegate(AddressOf OnIterationCompleted), New Object() {sender, iteration})
        Else
            ' Lazy update
            BeginInvoke(New MethodInvoker(AddressOf Me.m_grid.UpdateContent))
        End If

    End Sub

#End Region ' Control events

#Region " Internals "

    ''' <summary>
    ''' Presents the user with a standard Windows interface for selecting a file
    ''' </summary>
    ''' <returns>A user-selected file, or an empty string if the user did not select a file.</returns>
    ''' <remarks>
    ''' ToDo: use EwE file open command instead when ported to a plug-in
    ''' </remarks>
    Private Function ShowSelectModelDialogue() As String

        'Create a new open file dialogue
        Dim openFileDialogue As New OpenFileDialog()

        'Set the file filters
        openFileDialogue.Filter = ScientificInterfaceShared.My.Resources.FILEFILTER_MODEL_OPEN

        'Show the dialogue box and get the user-selected filename
        If (openFileDialogue.ShowDialog() = DialogResult.OK) Then
            Return openFileDialogue.FileName
        End If

        'The user did not select a file. Return an empty string
        Return String.Empty

    End Function

    Private Sub PopulateModelDropdowns()

        ' Scenarios
        Me.m_cmbScenario.Items.Clear()
        For i As Integer = 1 To Me.Core.nEcosimScenarios
            Me.m_cmbScenario.Items.Add(Me.Core.EcosimScenarios(i))
        Next

        ' Time series
        Me.m_cmbTimeSeries.Items.Clear()
        For i As Integer = 1 To Me.Core.nTimeSeriesDatasets
            Me.m_cmbTimeSeries.Items.Add(Me.Core.TimeSeriesDataset(i))
        Next

    End Sub

    Private Sub PopulateAnomalyDropdown()

        ' Anomaly shapes
        Me.m_cmbAnomalyShape.Items.Clear()
        For Each shape As cShapeData In Me.m_engine.GetAvailableAnomalyShapes()
            Me.m_cmbAnomalyShape.Items.Add(shape)
        Next

        If (Me.m_cmbAnomalyShape.Items.Count > 0) Then
            Me.m_cmbAnomalyShape.SelectedIndex = 0
        End If

    End Sub

    Private Sub PopulateModelControls()

        Me.m_tbxModel.Text = ""
        Dim model As cEwEModel = Me.Core.EwEModel
        If (model IsNot Nothing) Then
            Me.m_tbxModel.Text = model.Name
        End If

    End Sub

    Private Property SelectedEcosimScenario As cEcoSimScenario
        Get
            Return DirectCast(Me.m_cmbScenario.SelectedItem, cEcoSimScenario)
        End Get
        Set(ByVal scenario As cEcoSimScenario)
            Me.m_cmbScenario.SelectedItem = scenario
        End Set
    End Property

    Private Property SelectedTimeSeries As cTimeSeriesDataset
        Get
            Return DirectCast(Me.m_cmbTimeSeries.SelectedItem, cTimeSeriesDataset)
        End Get
        Set(ByVal dataset As cTimeSeriesDataset)
            Me.m_cmbTimeSeries.SelectedItem = dataset
        End Set
    End Property

    Private Property SelectedShape As cShapeData
        Get
            Return DirectCast(Me.m_cmbAnomalyShape.SelectedItem, cShapeData)
        End Get
        Set(ByVal dataset As cShapeData)
            Me.m_cmbAnomalyShape.SelectedItem = dataset
        End Set
    End Property

    Private ReadOnly Property SelectedIteration As ISFPIterations
        Get
            Return Me.m_grid.SelectedIteration
        End Get
    End Property

#End Region ' Internals

End Class