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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Public Class frmMSE

#Region " Private vars "

    Private m_plugin As cMSEPluginPoint = Nothing
    Private m_survivability As cSurvivability = Nothing

    Private m_fpArea As cEwEFormatProvider = Nothing
    Private m_fpNModelsToRun As cEwEFormatProvider = Nothing
    Private m_fpNTrials As cEwEFormatProvider = Nothing
    Private m_fpNYearsToProject As cEwEFormatProvider = Nothing
    Private m_fpMassBalanceTol As cEwEFormatProvider = Nothing
    Private m_fpMaxAttempts As cEwEFormatProvider = Nothing
    Private m_fpMaxTime As cEwEFormatProvider = Nothing

    Private m_bInUpdate As Boolean = False

#End Region ' Private vars

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="MSEPluginPoint">The <see cref="cMSEPluginPoint"/> this form 
    ''' is created for.</param>
    ''' <param name="uic">The <see cref="cUIContext"/> of the current EwE instance.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(MSEPluginPoint As cMSEPluginPoint, uic As cUIContext)

        Me.InitializeComponent()
        Me.UIContext = uic
        Me.m_plugin = MSEPluginPoint
        Me.m_survivability = Me.MSE.Survivability

    End Sub

#End Region ' Construction

#Region " Form overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_plugin Is Nothing) Then Return

        Me.m_bInUpdate = True

        Me.TabText = Me.Text

        ' -- Set up control interactions --

        ' Connect area UI control to live Ecopath data
        Me.m_fpArea = New cPropertyFormatProvider(Me.UIContext, m_tbxArea, Me.Core.EwEModel, eVarNameFlags.Area)
        ' Area can be made editable from here by not setting the format provider style:
        'Me.m_fpArea.Style = cStyleGuide.eStyleFlags.NotEditable

        Me.m_fpNModelsToRun = New cEwEFormatProvider(Me.UIContext, Me.m_tbxNModels2Run, GetType(Integer))
        Me.m_fpNModelsToRun.Value = Me.MSE.NModels2Run
        AddHandler Me.m_fpNModelsToRun.OnValueChanged, AddressOf OnNModels2RunChanged

        Me.m_fpNTrials = New cEwEFormatProvider(Me.UIContext, Me.m_tbxNTrials, GetType(Integer), New cVariableMetaData(0, Me.MSE.NumModelsAvailable, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo)))
        Me.m_fpNTrials.Value = Me.MSE.NModels
        AddHandler Me.m_fpNTrials.OnValueChanged, AddressOf OnNTrialsChanged

        Me.m_fpNYearsToProject = New cEwEFormatProvider(Me.UIContext, m_tbxNYearsProject, GetType(Integer))
        Me.m_fpNYearsToProject.Value = Me.MSE.NYearsProject
        AddHandler Me.m_fpNYearsToProject.OnValueChanged, AddressOf OnNYearsToProjectChanged

        Me.m_fpMassBalanceTol = New cEwEFormatProvider(Me.UIContext, Me.m_tbxTolerance, GetType(Single), New cVariableMetaData(0, 0.1, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo)))
        Me.m_fpMassBalanceTol.Value = Me.MSE.MassBalanceTol
        AddHandler Me.m_fpMassBalanceTol.OnValueChanged, AddressOf OnMassBalanceTolChanged

        Me.m_fpMaxAttempts = New cEwEFormatProvider(Me.UIContext, Me.m_tbxMaxAttempts, GetType(Integer), New cVariableMetaData(1, 1000000, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo)))
        Me.m_fpMaxAttempts.Value = Me.MSE.NMaxAttempts
        AddHandler Me.m_fpMaxAttempts.OnValueChanged, AddressOf OnMaxAttemptsChanged

        Me.m_fpMaxTime = New cEwEFormatProvider(Me.UIContext, Me.m_tbxMaxTime, GetType(Single), New cVariableMetaData(0.08, 48, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo)))
        AddHandler Me.m_fpMaxTime.OnValueChanged, AddressOf OnMaxTimeChanged

        Me.m_rbEwEDefaultPath.Checked = Me.MSE.UseEwEPath
        Me.m_rbCustomPath.Checked = Not Me.MSE.UseEwEPath
        Me.m_rbWriteAlways.Checked = Me.MSE.WriteAllResults
        Me.m_hdrStep2.IsCollapsed = True
        Me.m_hdrStep3.IsCollapsed = True

        Me.m_bInUpdate = False

        Dim mon As cMSEStateMonitor = Me.m_plugin.Monitor
        AddHandler mon.OnInvalidated, AddressOf OnMSEStateChanged

        ' Show/hide debug buttons
#If DEBUG Then
        Me.m_btnSampleSurvivabilities.Visible = True
        Me.m_btnCreateSurvDist.Visible = True
        Me.m_btnDeleteResults.Visible = True
#Else
        Me.m_btnSampleSurvivabilities.Visible = False
        Me.m_btnCreateSurvDist.Visible = False
        Me.m_btnDeleteResults.Visible = False
#End If

        ' Credits
        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
        cmd.AddControl(Me.m_pbCefas, "http://www.cefas.defra.gov.uk")
        cmd.AddControl(Me.m_pbEII, "http://www.ecopathinternational.org")

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

        If (Me.UIContext IsNot Nothing) Then

            Me.m_fpArea.Release()

            RemoveHandler Me.m_fpNModelsToRun.OnValueChanged, AddressOf OnNModels2RunChanged
            Me.m_fpNModelsToRun.Release()

            RemoveHandler Me.m_fpNTrials.OnValueChanged, AddressOf OnNTrialsChanged
            Me.m_fpNTrials.Release()

            RemoveHandler Me.m_fpNYearsToProject.OnValueChanged, AddressOf OnNYearsToProjectChanged
            Me.m_fpNYearsToProject.Release()

            RemoveHandler Me.m_fpMassBalanceTol.OnValueChanged, AddressOf OnMassBalanceTolChanged
            Me.m_fpMassBalanceTol.Release()

            RemoveHandler Me.m_fpMaxAttempts.OnValueChanged, AddressOf OnMaxAttemptsChanged
            Me.m_fpMaxAttempts.Release()

            RemoveHandler Me.m_fpMaxTime.OnValueChanged, AddressOf OnMaxTimeChanged
            Me.m_fpMaxTime.Release()

            Dim mon As cMSEStateMonitor = Me.m_plugin.Monitor
            RemoveHandler mon.OnInvalidated, AddressOf OnMSEStateChanged

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            cmd.RemoveControl(Me.m_pbCefas)
            cmd.RemoveControl(Me.m_pbEII)

        End If

        MyBase.OnFormClosed(e)

    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        If (Me.m_plugin Is Nothing) Then Return
        If (Me.IsDisposed) Then Return

        Me.m_bInUpdate = True

        Dim mon As cMSEStateMonitor = Me.m_plugin.Monitor
        Dim img As Image = Nothing

        ' States
        Dim bHasParams As Boolean = mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams)
        Dim bHasModels As Boolean = mon.IsStateAvailable(cMSEStateMonitor.eState.HasModels)
        Dim bHasResults As Boolean = mon.IsStateAvailable(cMSEStateMonitor.eState.HasResults)

        Dim bIsRunningMSE As Boolean = (Me.MSE.RunState = cMSE.eRunStates.RunningMSE)
        Dim bIsRunningModels As Boolean = (Me.MSE.RunState = cMSE.eRunStates.RunningModels)
        Dim bIsRunning As Boolean = bIsRunningModels Or bIsRunningMSE

        Me.m_plStep1.Enabled = mon.IsStateAvailable(cMSEStateMonitor.eState.Idle) And Not bIsRunning
        Me.m_plStep3.Enabled = bHasParams And Not bIsRunning

        ' Enable/disable panel 2 in a loop
        'Me.m_plStep2.Enabled = mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams) And Not bIsRunning
        For Each ctrl As Control In Me.m_plStep2.Controls
            If (ReferenceEquals(ctrl, Me.m_btnStopCreateModels)) Then
                ctrl.Enabled = bHasParams And bIsRunningModels
            Else
                ctrl.Enabled = bHasParams And Not bIsRunning
            End If
        Next

        ' Enable/disable panel 4 in a loop
        'Me.m_plStep4.Enabled = bHasModels And Not bIsRunning
        For Each ctrl As Control In Me.m_plStep4.Controls
            If (ReferenceEquals(ctrl, Me.Button1)) Then
                ctrl.Enabled = bHasParams And bIsRunningMSE
            Else
                ctrl.Enabled = bHasParams And Not bIsRunning
            End If
        Next

        Me.m_rbEwEDefaultPath.Checked = Me.MSE.UseEwEPath
        Me.m_rbCustomPath.Checked = Not Me.MSE.UseEwEPath

        Me.m_lblPathValue.Text = cStringUtils.CompactString(Me.MSE.DataPath, Me.m_lblPathValue.ClientRectangle.Width, Me.m_lblPathValue.Font, TextFormatFlags.PathEllipsis)
        cToolTipShared.GetInstance().SetToolTip(Me.m_lblPathValue, Me.MSE.DataPath)

        ' Manage panel 1 control in detail, because this panel shares general configuration
        ' parameters (e.g., path stuff) with controls that act on input parameters once the
        ' MSE path has been validated
        If Me.MSE.IsInputStructureAvailable() Then
            If Me.MSE.IsInputDataCompatible() Then
                img = SharedResources.OK
                Me.m_btnEditBasicInputs.Enabled = True
                Me.m_btnEditSurvivabilities.Enabled = True
                Me.m_btnEditDiets.Enabled = True
            Else
                Me.m_btnEditBasicInputs.Enabled = False
                Me.m_btnEditSurvivabilities.Enabled = False
                Me.m_btnEditDiets.Enabled = False
                img = SharedResources.Critical
            End If
        Else
            img = Nothing
            Me.m_btnEditBasicInputs.Enabled = True
            Me.m_btnEditSurvivabilities.Enabled = True
            Me.m_btnEditDiets.Enabled = True
        End If
        Me.m_pbPathCompatible.Image = img

        img = Nothing
        If mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams) Then
            If Not mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams) Then
                img = SharedResources.Critical
            End If
        End If
        Me.m_pbModelsCompatible.Image = img

        ' Update trial buttons
        Me.m_fpNTrials.Enabled = mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams)
        Me.m_fpMassBalanceTol.Enabled = mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams)
        Me.m_btnDecreaseEffort.Enabled = mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams)

        ' Provide feedback about available models
        If mon.IsStateAvailable(cMSEStateMonitor.eState.HasParams) Then
            If String.IsNullOrWhiteSpace(Me.MSE.ModelCompatibilityInfo) Then
                Me.m_tbxNumAvailableModels.Text = CStr(Me.MSE.NumModelsAvailable)
            Else
                Me.m_tbxNumAvailableModels.Text = String.Format(SharedResources.GENERIC_LABEL_DETAILED, _
                                                                Me.MSE.NumModelsAvailable, _
                                                                Me.MSE.ModelCompatibilityInfo)
            End If
            Me.m_tbxNumAvailableFishingStrategies.Text = CStr(Me.MSE.NumStrategiesAvailable)
        Else
            Me.m_tbxNumAvailableModels.Text = ""
            Me.m_tbxNumAvailableFishingStrategies.Text = ""
        End If

        Me.m_btnDeleteResults.Enabled = bHasResults And Not bIsRunning

        Me.m_bInUpdate = False

    End Sub

#End Region ' Form overrides

#Region " Control events "

    Private Sub OnRunCreateModels(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnRunCreateModels.Click

        If (Me.m_plugin Is Nothing) Then Return

        Try
            ' Run is threaded; result of call does not matter
            Me.MSE.CreateModels()
        Catch ex As Exception
            cLog.Write(ex, "CefasMSE.frmMSE::OnRunCreateModels")
        End Try

    End Sub

    Private Sub OnStopCreateModels(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnStopCreateModels.Click

        If (Me.m_plugin Is Nothing) Then Return

        Try
            Me.MSE.StopRun()
        Catch ex As Exception
            cLog.Write(ex, "CefasMSE.frmMSE::OnStopCreateModels")
        End Try

    End Sub

    Private Sub OnPathClicked(sender As System.Object, e As System.EventArgs) _
        Handles m_lblPathValue.Click

        If (Me.m_plugin Is Nothing) Then Return

        Try
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            cmd.Invoke(Me.MSE.DataPath)
        Catch ex As Exception
            cLog.Write(ex, "CefasMSE.frmMSE::OnPathClicked(" & Me.MSE.DataPath & ")")
        End Try

    End Sub

    Private Sub OnPathPrefChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_rbEwEDefaultPath.CheckedChanged, m_rbCustomPath.CheckedChanged

        If (Me.m_plugin Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Try
            Me.MSE.UseEwEPath = Me.m_rbEwEDefaultPath.Checked
            'Me.ResolveMSEPathConflicts()
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnPathPrefChanged")
        End Try
        Me.UpdateControls()

    End Sub

    Private Sub OnRun(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnRun.Click
        Try
            Me.MSE.WriteAllResults = Me.m_rbWriteAlways.Checked
            Me.MSE.WriteYearlyOnly = Me.m_chkYearly.Checked
            Me.MSE.LoadSampledParams()
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnRun")
        End Try
    End Sub

    Private Sub OnSelectDataPath(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnChangePath.Click

        Me.m_bInUpdate = True
        Try
            If Me.BrowseDataPath() Then
                Me.MSE.IsInputStructureAvailable()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnSelectDataPath")
        End Try
        Me.m_bInUpdate = False

    End Sub

    Private Sub OnEditSurvivabilities(sender As System.Object, e As System.EventArgs) _
        Handles m_btnEditSurvivabilities.Click

        If Not Me.MSE.ResolveMSEPathConflicts(True) Then Return

        Try
            Dim frmSurvivabilities As New frmEditSurvivabilities(MSE)
            frmSurvivabilities.Init(Me.UIContext)
            If frmSurvivabilities.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                Me.MSE.Survivability.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnEditSurvivabilities")
        End Try

    End Sub

    Private Sub OnEditDiets(sender As System.Object, e As System.EventArgs) _
        Handles m_btnEditDiets.Click

        If Not Me.MSE.ResolveMSEPathConflicts(True) Then Return
        Try
            Dim frmDiets As New frmEditDiets(MSE)
            frmDiets.Init(Me.UIContext)
            If frmDiets.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                Me.MSE.Diets.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnEditDiets")
        End Try

    End Sub

    Private Sub OnEditQuotaShares(sender As System.Object, e As System.EventArgs) _
        Handles m_btnQuotaShares.Click

        If Not Me.MSE.ResolveMSEPathConflicts(True) Then Return

        Try
            Dim frmQuotaShares As New frmEditQuotaShares(Me.MSE)
            frmQuotaShares.Init(Me.UIContext)
            If frmQuotaShares.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                'Me.MSE.InvalidateConfigurationState(True)
                Me.MSE.QuotaShares.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnQuotaShares")
        End Try

    End Sub
    Private Sub OnShowTFM(sender As System.Object, e As System.EventArgs) _
        Handles m_btnReviewTFM.Click

        Try
            Dim frm As New frmTFMpolicy(Me.UIContext, Me.MSE)
            If frm.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                'Me.MSE.InvalidateConfigurationState(True)
                Me.MSE.Strategies.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnShowTFM")
        End Try

    End Sub

    Private Sub OnReviewDistParams(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnEditBasicInputs.Click

        Try
            Me.EditBasicInputs()
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnReviewDistParams")
        End Try

    End Sub

    Private Sub OnNModels2RunChanged(sender As Object, args As EventArgs)
        Try
            Me.MSE.NModels2Run = CInt(Me.m_fpNModelsToRun.Value)
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnNModels2RunChanged")
        End Try
    End Sub

    Private Sub OnNTrialsChanged(sender As Object, args As EventArgs)
        Try
            Me.MSE.NModels = CInt(Me.m_fpNTrials.Value)
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnNTrialsChanged")
        End Try
    End Sub

    Private Sub OnNYearsToProjectChanged(sender As Object, args As EventArgs)
        Try
            Me.MSE.NYearsProject = CInt(Me.m_fpNYearsToProject.Value)
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnNYearsToProjectChanged")
        End Try
    End Sub

    Private Sub OnMassBalanceTolChanged(sender As Object, args As EventArgs)
        Try
            Me.MSE.MassBalanceTol = CSng(Me.m_fpMassBalanceTol.Value)
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnMassBalanceTolChanged")
        End Try
    End Sub

    Private Sub OnMaxAttemptsChanged(sender As Object, args As EventArgs)
        Try
            Me.MSE.NMaxAttempts = CInt(Me.m_fpMaxAttempts.Value)
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnMaxAttemptsChanged")
        End Try
    End Sub

    Private Sub OnMaxTimeChanged(sender As Object, args As EventArgs)
        Try
            Me.MSE.NMaxTime = CSng(Me.m_fpMaxTime.Value)
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnMaxAttemptsChanged")
        End Try
    End Sub

    Private Sub OnDecreaseEffort(sender As Object, e As System.EventArgs) _
        Handles m_btnDecreaseEffort.Click

        Try

            Dim frmMaxDecreaseEfforts As New frmEditDecreaseEffort()
            frmMaxDecreaseEfforts.Init(Me.UIContext, Me.MSE)
            If frmMaxDecreaseEfforts.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                'Me.MSE.InvalidateConfigurationState(True)
                Me.MSE.EffortLimits.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnDecreaseEffort")
        End Try

    End Sub


    Private Sub OnGenerateSampleSurvivabilities(sender As System.Object, e As System.EventArgs) Handles m_btnSampleSurvivabilities.Click

        Try
            MSE.GenerateSurvivabilities()
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnGenerateSampleSurvivabilities")
        End Try
    End Sub


    Private Sub OnDeleteResults(sender As System.Object, e As System.EventArgs) _
        Handles m_btnDeleteResults.Click

        If Me.m_plugin.MSE.AskUser(My.Resources.PROMPT_DELETE_RESULTS, eMessageReplyStyle.YES_NO) = eMessageReply.YES Then
            Me.MSE.DeleteResults()
            Me.MSE.InvalidateConfigurationState(False)
            Me.UpdateControls()
        End If

    End Sub

    Private Sub btnCreateSurvDist_Click(sender As System.Object, e As System.EventArgs) Handles m_btnCreateSurvDist.Click
        'Creates a default set of survivability distribution parameters - we might not need this later

        ' JS 20Jun14: This should perhaps not have been added again:
        ' Survivabilities_dist files are already created with the setup of new folders.
        ' If you need the new files, let's at least re-use what MSE is already offering.

        Me.MSE.GenerateSurvivabilities()

    End Sub

    Private Sub onStockAssessment(sender As System.Object, e As System.EventArgs) Handles m_btnStockAssessment.Click

        Try
            Dim frm As New frmCEFASRecruitment(Me.UIContext, Me.MSE)
            If frm.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                'Me.MSE.InvalidateConfigurationState(True)
                Me.MSE.StockAssessment.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::onStockAssessment")
        End Try

    End Sub

    Private Sub onUncertainty(sender As System.Object, e As System.EventArgs) Handles m_btnSAError.Click

        Try
            Dim frm As New frmEditAssessmentError(Me.UIContext, Me.MSE)
            If frm.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                'Me.MSE.InvalidateConfigurationState(True)
                Me.MSE.StockAssessment.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnStopCreateModels_Click(sender As System.Object, e As System.EventArgs)
        Try
            Me.MSE.StopRun()
        Catch ex As Exception

        End Try
    End Sub


#End Region ' Control events

#Region " Plug-in callback "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Handle a (remote) request to update the form state. The request is handled
    ''' in idle time.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnMSEStateChanged(ByVal man As cMSEStateMonitor)
        Me.BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
    End Sub

#End Region ' Plug-in callback

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="cMSEPluginPoint"/> connected to this form.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private ReadOnly Property Plugin As cMSEPluginPoint
        Get
            Return Me.m_plugin
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="cMSE"/> connected to this form.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private ReadOnly Property MSE As cMSE
        Get
            Return Me.m_plugin.MSE
        End Get
    End Property

#End Region ' Internals

#Region " Path / model validation "

    Private Function EditBasicInputs() As Boolean

        If Not Me.MSE.ResolveMSEPathConflicts(True) Then Return False

        Dim frmDisParams As New frmDistributionParameters()
        frmDisParams.Init(Me.UIContext, Me.Plugin)

        If (frmDisParams.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK) Then
            Me.MSE.InvalidateConfigurationState(False)
            ' Nothing to reload
            Me.UpdateControls()
            Return True
        End If

        Return False

    End Function

    Private Function BrowseDataPath() As Boolean

        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmd As cDirectoryOpenCommand = DirectCast(cmdh.GetCommand(cDirectoryOpenCommand.COMMAND_NAME), cDirectoryOpenCommand)
        cmd.Invoke(Me.MSE.CustomPath, My.Resources.PROMPT_DATAPATH)

        If (cmd.Result = System.Windows.Forms.DialogResult.OK) Then
            Me.MSE.CustomPath = cmd.Directory
            Me.MSE.UseEwEPath = False
            Return True
        End If

        Return False

    End Function

#End Region ' Path / model validation
 


    Private Sub InsertValueIntoArray(ByRef LandingsArray As Double(,,,), ByRef iStrategy As Integer, ByRef iFleet As Integer, ByRef iGroup As Integer, ByRef iTimeStep As Integer, ByRef THEVALUE As Double)
        LandingsArray(iStrategy, iFleet, iGroup, iTimeStep) = 123456789
    End Sub

    Private Sub OnViewBiomassLimits(sender As Object, e As EventArgs) Handles m_btnBiomassLimits.Click

        If Not Me.MSE.ResolveMSEPathConflicts(True) Then Return

        Try
            Dim frmBiomassLimits As New frmBiomassLimits(Me.MSE)
            frmBiomassLimits.Init(Me.UIContext)
            If frmBiomassLimits.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                'Me.MSE.InvalidateConfigurationState(True)
                Me.MSE.BiomassLimits.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnBiomassLimits")
        End Try

    End Sub

    Private Sub m_btnSelectStrategies_Click(sender As Object, e As EventArgs) Handles m_btnSelectStrategies.Click

        Try
            Dim frmStrategiesOverview As New frmStrategiesOverview()
            frmStrategiesOverview.Init(Me.UIContext, Me.MSE)
            If frmStrategiesOverview.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                Me.MSE.Strategies.Load()
                Me.UpdateControls()
            End If
        Catch ex As Exception
            cLog.Write(ex, "CEFAS.frmMSE::OnStrategiesOverview")
        End Try

    End Sub


End Class
