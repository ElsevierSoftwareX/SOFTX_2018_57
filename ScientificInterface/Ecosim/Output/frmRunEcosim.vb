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
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterface.Controls
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form, implementing the Run Ecosim interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmRunEcosim

#Region " Variables "

        ''' <summary>
        ''' Enumerated type, indicating whether the user is viewing fleet or
        ''' group related fishing shapes underneath the Ecosim plot.
        ''' </summary>
        Private Enum eSelectionModeType
            ''' <summary>User has not made any selection (yet).</summary>
            NotSet = 0
            ''' <summary>User is viewing Fleet fishing shapes.</summary>
            Fleets
            ''' <summary>User is viewing Group fishing shapes.</summary>
            Groups
        End Enum

        Private m_selectionMode As eSelectionModeType = eSelectionModeType.NotSet

        Private m_shapeGUIHandler As cForcingShapeGUIHandler = Nothing
        Private m_params As cEcoSimModelParameters = Nothing
        Private m_iTimeSteps As Integer = 0
        Private m_sChangeTrackSize As Single = 0.1!
        Private m_zgp As cEcosimOutputPlotHelper = Nothing

        Private m_simStats As cEcosimStats

        Private m_bInUpdate As Boolean = False

        ' === plot data ==
        Private m_plotData As eMSEPlotData = eMSEPlotData.Biomass
        Private m_bIsAnnual As Boolean = False
        Private m_bIsCumulative As Boolean = False
        Private m_bIsExploring As Boolean = False
        Private m_bIsEffortSelected As Boolean = False

#End Region ' Variables

#Region " Constructors "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)
                If MyBase.UIContext IsNot Nothing Then
                    Me.m_lbGroups.Detach()
                End If
                MyBase.UIContext = value
                If MyBase.UIContext IsNot Nothing Then
                    Me.m_lbGroups.Attach(Me.UIContext)
                End If
            End Set
        End Property

#End Region ' Constructors

#Region " Framework overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = Nothing

            Me.m_params = Core.EcoSimModelParameters()
            Me.m_simStats = Me.Core.EcosimStats

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoPath, eCoreComponentType.EcoSim, eCoreComponentType.ShapesManager, eCoreComponentType.Core, eCoreComponentType.TimeSeries}

            Me.m_zgp = New cEcosimOutputPlotHelper()
            Me.m_zgp.Attach(Me.UIContext, Me.m_graph)

            Me.m_zgp.ConfigurePane(SharedResources.HEADER_RELATIVEBIOMASS, SharedResources.HEADER_YEAR, SharedResources.HEADER_RELATIVEBIOMASS, False)
            Me.m_zgp.ShowMultipleRuns = Me.m_tsbnShowMultipleRuns.Checked

            Me.m_zgp.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.MaxOnly

            Me.m_zgp.YScaleMin = 0.0!
            Me.m_zgp.ShowPointValue = True

            ' Set the axis
            Me.m_graph.GraphPane.XAxis.Scale.Min = Core.EcosimFirstYear
            Me.m_graph.GraphPane.XAxis.Scale.Max = Core.EcoSimModelParameters.NumberYears + Core.EcosimFirstYear

            AddHandler Me.m_zgp.OnCursorPos, AddressOf OnSyncCursor
            AddHandler Me.m_graph.GraphPane.AxisChangeEvent, AddressOf OnAxisChanged

            Me.m_tsmiShowEffortAndMortalities.Checked = My.Settings.ShowEffortMortalityInRunSim
            Me.m_scPlots.Panel2Collapsed = (Not My.Settings.ShowEffortMortalityInRunSim)

            Me.UpdateControls()

            ' Display Groups
            cmd = cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If cmd IsNot Nothing Then
                cmd.AddControl(Me.m_tsbtnShowHideGroups)
            End If

            ' Track core monitor changes
            AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

            Me.PopulateGraph()
            Me.UpdateSS()

            Me.SelectionMode = eSelectionModeType.Fleets
            Me.m_lbGroups.SelectedIndex = 0

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            If (Me.UIContext Is Nothing) Then Return

            Me.OnStop(Nothing, Nothing)
            RemoveHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

            ' Unplug
            Me.IsExploring = False

            ' Clean up
            If Me.m_shapeGUIHandler IsNot Nothing Then
                Me.m_shapeGUIHandler.Detach()
                Me.m_shapeGUIHandler = Nothing
            End If

            ' Show/Hide Groups
            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If cmd IsNot Nothing Then
                cmd.RemoveControl(Me.m_tsbtnShowHideGroups)
            End If

            RemoveHandler Me.m_zgp.OnCursorPos, AddressOf OnSyncCursor
            RemoveHandler Me.m_graph.GraphPane.AxisChangeEvent, AddressOf OnAxisChanged
            Me.m_zgp.Detach()

            MyBase.OnFormClosed(e)
        End Sub

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)

            Select Case msg.Source

                Case eCoreComponentType.EcoSim
                    HandleEcosimMessage(msg)

                Case eCoreComponentType.ShapesManager
                    HandleShapeMessage(msg)

                Case eCoreComponentType.Core
                    If (msg.Type = eMessageType.GlobalSettingsChanged) Then
                        Me.UpdateControls()
                    End If

                Case eCoreComponentType.TimeSeries
                    HandleTimeseriesMessage(msg)

            End Select

        End Sub

        Public Overrides ReadOnly Property IsRunForm() As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            If (ct And cStyleGuide.eChangeType.GroupVisibility) > 0 Then
                Me.PopulateGraph()
            End If
        End Sub

#End Region ' Framework overrides

#Region " Events "

#Region " Controls "

        Private Sub OnRun(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnRun.Click

            Try
                If Not Me.IsRunning Then
                    Me.m_iTimeSteps = Me.Core.nEcosimTimeSteps
                    Me.m_graph.Refresh()
                    Me.Core.RunEcoSim(AddressOf HandleEcosimTimeStep, True)
                End If
            Catch ex As Exception
                cLog.Write(ex, "form frmRunEcosim.OnRun")
            End Try

        End Sub

        Private Sub OnStop(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnStop.Click
            Try
                If Me.IsRunning Then
                    Me.Core.StopEcoSim()
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnShowMultipleRuns(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnShowMultipleRuns.Click

            Me.m_zgp.ShowMultipleRuns = Me.m_tsbnShowMultipleRuns.Checked

            ' Multiple runs cannot show cumulative data (#992)
            If Me.m_zgp.ShowMultipleRuns Then Me.ShowData(Me.m_plotData, False)

            Me.PopulateRunsBox()
            Me.m_zgp.RescaleAndRedraw()
            Me.UpdateControls()

        End Sub

        Private Sub AnnualOutputToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiShowAnnualOutput.Click

            If Me.m_bInUpdate Then Return

            Me.IsAnnualPlot = Me.m_tsmiShowAnnualOutput.Checked
        End Sub

        Private Sub OnShowEffortAndMortalities(sender As Object, e As EventArgs) _
            Handles m_tsmiShowEffortAndMortalities.Click

            If (Me.m_bInUpdate) Then Return

            Me.m_scPlots.Panel2Collapsed = Not Me.m_tsmiShowEffortAndMortalities.Checked
            My.Settings.ShowEffortMortalityInRunSim = Me.m_tsmiShowEffortAndMortalities.Checked
            My.Settings.Save()

        End Sub

        Private Sub OnShowBiomassAbs(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiBiomassAbs.Click
            Me.ShowData(eMSEPlotData.Biomass, True)
        End Sub

        Private Sub OnShowBiomassRel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiBiomassRel.Click
            Me.ShowData(eMSEPlotData.Biomass, False)
        End Sub

        Private Sub OnShowCatchAbs(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiCatchAbs.Click
            Me.ShowData(eMSEPlotData.GroupCatch, True)
        End Sub

        Private Sub OnShowCatchRel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiCatchRel.Click
            Me.ShowData(eMSEPlotData.GroupCatch, False)
        End Sub

        Private Sub OnShowValueAbs(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiValueAbs.Click
            Me.ShowData(eMSEPlotData.Value, True)
        End Sub

        Private Sub m_tsmiValueRel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiValueRel.Click
            Me.ShowData(eMSEPlotData.Value, False)
        End Sub

        Private Sub AutoScaleToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiAutoscale.Click
            Me.m_zgp.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.MaxOnly
            Me.UpdateControls()
        End Sub

        Private Sub m_tstbxSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tstbMax.Click
            m_tsmiAutoscale.Checked = False
            m_tsmiCustomScaleLabel.Checked = True
        End Sub

        Private Sub OnCustomScale(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiCustomScaleLabel.Click
            Double.TryParse(Me.m_tstbMax.Text, Me.m_zgp.YScaleMax)
            Double.TryParse(Me.m_tstbMin.Text, Me.m_zgp.YScaleMin)
            Me.m_zgp.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.None
            Me.UpdateControls()
        End Sub

        Private Sub m_tstbxSetMax_LostFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tstbMax.LostFocus
            Double.TryParse(TryCast(sender, ToolStripTextBox).Text, Me.m_zgp.YScaleMax)
            Me.m_zgp.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.None
            Me.UpdateControls()
        End Sub

        Private Sub m_tstbxSetMin_LostFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tstbMin.LostFocus
            Double.TryParse(TryCast(sender, ToolStripTextBox).Text, Me.m_zgp.YScaleMin)
            Me.m_zgp.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.None
            Me.UpdateControls()
        End Sub

        Private Sub m_tsmiSortMostChanged_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnExplore.Click, m_tsmiSort.Click

            If Me.m_bInUpdate Then Return

            Me.m_tsmiSort.Checked = Not Me.m_tsmiSort.Checked
            Me.m_tsbnExplore.Checked = Me.m_tsmiSort.Checked
            Me.IsExploring = Me.m_tsmiSort.Checked

        End Sub

        Private Sub m_tstbChangeAmount_LostFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tstbChangeAmount.LostFocus

            Single.TryParse(Me.m_tstbChangeAmount.Text, Me.m_sChangeTrackSize)
            Me.m_sChangeTrackSize = Math.Max(0, Me.m_sChangeTrackSize)
            Me.m_tstbChangeAmount.Text = CStr(Me.m_sChangeTrackSize)

            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Listbox selected index change handler
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub lb_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_lbRuns.SelectedIndexChanged, m_lbGroups.SelectedIndexChanged

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True

            Try
                Dim lb As ListBox = DirectCast(sender, ListBox)
                ' Clear selection of all other groups when 'all' is clicked
                If (lb.GetSelected(0) = True) And (lb.SelectedIndices.Count > 1) Then
                    For i As Integer = 0 To lb.SelectedIndices.Count - 1
                        lb.SetSelected(i, (i = 0))
                    Next
                End If

                Me.UpdateGraphHighlights()
                Me.UpdateSS()
            Catch ex As Exception

            End Try
            Me.m_bInUpdate = False
        End Sub

        Private Sub OnSyncCursor(ByVal zgh As cZedGraphHelper, ByVal iPane As Integer, ByVal sPos As Single)
            If Me.IsExploring Then
                Try
                    ' Hmm, this logic fails when time series are loaded; in that case
                    ' the X value is corrected by the Ecosim year. Let's hack around this
                    ' for now.
                    Me.SortGroupsAtTimestep(CInt(Math.Round((sPos - Me.Core.EcosimFirstYear) * cCore.N_MONTHS)))
                Catch ex As Exception

                End Try
            End If
        End Sub

#End Region ' Controls

#Region " Core "

        Private Sub OnCoreExecutionStateChanged(ByVal csm As cCoreStateMonitor)

            ' Could be that we're closing
            If (Me.IsDisposed) Then Return

            Dim bEcosimRunning As Boolean = Core.StateMonitor.IsEcosimRunning
            Dim bHasEcosimResults As Boolean = Core.StateMonitor.HasEcosimRan

            ' Does not have ecosim results?
            If (Not Core.StateMonitor.HasEcopathRan) Then
                ' #Yes: clear run results
                Me.ResetGraph()
            End If

            ' Check whether ecosim is running
            ' Is this a state change?
            If (bEcosimRunning <> Me.IsRunning) Then
                ' #Yes: update to new state
                Me.IsRunning = bEcosimRunning
                If Me.IsRunning Then
                    Me.Core.SetStopRunDelegate(AddressOf Me.Core.StopEcoSim)
                    cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_ECOSIM_RUNNING)
                    Me.IsExploring = False
                Else
                    cApplicationStatusNotifier.EndProgress(Me.Core)
                    Me.Core.SetStopRunDelegate(Nothing)
                    If Not Me.m_zgp.ShowMultipleRuns Then
                        Me.m_zgp.Clear()
                    End If
                    Me.m_zgp.CreateRun(cStringUtils.Localize(SharedResources.GENERIC_VAUE_RUN_X, (Me.m_zgp.NumRuns + 1)))
                    Me.PopulateRunsBox()
                    Me.PopulateGroupBox()
                End If
                Me.UpdateControls()

            End If

        End Sub

        Private Sub HandleEcosimMessage(ByRef msg As cMessage)

            Try
                Select Case msg.Type
                    Case eMessageType.EcosimRunCompleted

                        'jb if Ecosim was not run by this interface ignore this message
                        If (Me.m_iTimeSteps > 0) Then
                            Me.UpdateSS()
                            Me.m_iTimeSteps = 0
                        End If

                        ' Plot the graph
                        Me.PopulateGraph()

                    Case eMessageType.EcosimNYearsChanged
                        ' NOP

                    Case eMessageType.DataModified
                        ' NOP

                End Select

            Catch ex As Exception
                cLog.Write(ex, "frmRunEcosim::HandleEcosimMessage")
            End Try

        End Sub

        Private Sub HandleShapeMessage(msg As cMessage)
            Try
                If (Me.m_shapeGUIHandler Is Nothing) Then Return

                If (((Me.SelectionMode = eSelectionModeType.Fleets) And (msg.DataType = eDataTypes.FishingEffort)) Or
                    ((Me.SelectionMode = eSelectionModeType.Groups) And (msg.DataType = eDataTypes.FishMort))) Then
                    Me.m_shapeGUIHandler.Refresh()
                End If
            Catch ex As Exception
                cLog.Write(ex, "frmRunEcosim::HandleShapeMessage")
            End Try
        End Sub

        Private Sub HandleTimeseriesMessage(msg As cMessage)
            Try
                Select Case msg.Type
                    Case eMessageType.DataAddedOrRemoved
                        Me.UpdateControls()
                End Select
            Catch ex As Exception
                cLog.Write(ex, "frmRunEcosim::HandleTimeseriesMessage")
            End Try
        End Sub

        Private Sub HandleEcosimTimeStep(ByVal iTime As Long, ByVal results As cEcoSimResults)

            Try
                ' Status update only every 12 months
                If (iTime Mod cCore.N_MONTHS) = 0 Then
                    cApplicationStatusNotifier.UpdateProgress(Me.Core, My.Resources.STATUS_ECOSIM_RUNNING, CSng(iTime / m_iTimeSteps))
                End If
            Catch ex As Exception
                cLog.Write(ex, "frmRunEcosim::HandleEcosimTimeStep")
            End Try

        End Sub

#End Region ' Core

#Region " Forcing function "

        Private Sub OnTargetSelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tscbTarget.SelectedIndexChanged
            Dim obj As ICoreInterface = GetSelectedGroupOrFleet()

            Select Case Me.SelectionMode
                Case eSelectionModeType.Fleets
                    Me.LoadFishingRateShape()
                Case eSelectionModeType.Groups
                    Me.LoadFishMortShape()
                Case Else
                    Debug.Assert(False)
            End Select
        End Sub

        Private Sub OnFValue_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbnSetToValue.Click

            Dim strCaption As String = My.Resources.RUN_ECOSIM_F_VALUE_CAPTION
            Dim strMessage As String = My.Resources.RUN_ECOSIM_F_VALUE_MSG
            Dim strDefault As String = "1"
            Dim strValue As String = String.Empty

            ' Sanity check
            If (Me.m_sketchPad.Shape Is Nothing) Then Return

            Dim box As New frmInputBox()
            If (box.Show(strMessage, strCaption, strDefault) = System.Windows.Forms.DialogResult.OK) Then
                strValue = box.Value
            End If

            If (strValue.Length <> 0) Then

                Dim astrEntered As String() = strValue.Split(CChar(" "))

                ' One character entered?
                If astrEntered.Length = 1 Then
                    ' #Yes: duplicate this char over the entire shape
                    Try
                        If (Me.m_shapeGUIHandler IsNot Nothing) Then
                            Me.m_shapeGUIHandler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Reset, _
                                        New cShapeData() {Me.m_sketchPad.Shape}, cSystemUtils.Val(astrEntered(0)))
                        End If
                    Catch ex As Exception
                    End Try

                ElseIf astrEntered.Length > 1 Then

                    Dim shape As cShapeData = Me.m_sketchPad.Shape

                    ' Translate individual values
                    Dim asValues(shape.nPoints) As Single
                    Dim sValue As Single = 0.0!

                    For i As Integer = 0 To shape.nPoints
                        If (i < (astrEntered.Length - 1)) Then
                            Try
                                sValue = CSng(cSystemUtils.Val(astrEntered(i)))
                            Catch ex As Exception
                                sValue = -1
                            End Try
                        End If
                        asValues(i) = sValue
                    Next

                    shape.LockUpdates()
                    shape.ShapeData = asValues
                    shape.UnlockUpdates()

                End If
            End If
        End Sub

        Private Sub OnFReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbnResetFs.Click

            Dim ts As cTimeSeries = Nothing

            ' JS 16May08: bypassed shape handler (which may be 0) to do a mass change
            Me.Core.FishingEffortShapeManager.ResetToDefaults()
            Me.Core.FishMortShapeManager.ResetToDefaults()

            ' JS 16Apr09: also disable time series
            For iTS As Integer = 1 To Me.Core.nTimeSeries
                Me.Core.EcosimTimeSeries(iTS).Enabled = False
            Next
            Me.Core.UpdateTimeSeries()

        End Sub

        Private Sub OnFZero_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbnSetTo0.Click
            If Me.m_shapeGUIHandler IsNot Nothing Then
                Me.m_shapeGUIHandler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Reset, _
                                                    New cShapeData() {Me.m_sketchPad.Shape}, 0.0!)
            End If
        End Sub

#End Region ' Forcing function

        Private Sub OnSelectTarget(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnFleet.Click, m_tsbnGroup.Click
            Try
                If ReferenceEquals(sender, Me.m_tsbnFleet) Then
                    Me.SelectionMode = eSelectionModeType.Fleets
                Else
                    Me.SelectionMode = eSelectionModeType.Groups
                End If
                Me.UpdateControls()
                Me.PopulateTargetComboBox()
            Catch ex As Exception
            End Try
        End Sub

        Private Sub OnSaveOutput(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnSaveOutput.Click
            Try
                Me.Core.Autosave(eAutosaveTypes.EcosimResults) = Me.m_tsbnSaveOutput.Checked
            Catch ex As Exception
                ' Plop
            End Try
        End Sub

        Private Sub OnAxisChanged(pane As GraphPane)
            If (Me.UIContext Is Nothing) Then Return
            Me.AlignSketchpad()
        End Sub

#End Region ' Events

#Region " Internal implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether plot displays annual values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property IsAnnualPlot() As Boolean
            Get
                Return Me.m_bIsAnnual
            End Get
            Set(ByVal value As Boolean)
                If (value = Me.m_bIsAnnual) Then Return

                Me.m_bIsAnnual = value
                Me.UpdateControls()
                Me.PopulateGraph()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the user is exploring values with the cursor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property IsExploring() As Boolean
            Get
                Return Me.m_bIsExploring
            End Get
            Set(ByVal value As Boolean)
                If (value = Me.m_bIsExploring) Then Return

                Me.m_bIsExploring = value

                ' Show or hide cursor
                Me.m_zgp.ShowCursor = Me.m_bIsExploring

                Me.UpdateControls()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set type of data to plot.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ShowData(ByVal data As eMSEPlotData, ByVal bCumulative As Boolean)
            ' Store props
            Me.m_plotData = data
            Me.m_bIsCumulative = bCumulative
            ' Cumulative graphs cannot show multiple runs (#991)
            If bCumulative Then Me.m_zgp.ShowMultipleRuns = False
            ' Update
            Me.UpdateControls()
            Me.PopulateGraph()
        End Sub

        Private Sub PopulateRunsBox()

            Me.m_lbRuns.SuspendLayout()

            Me.m_lbRuns.Items.Clear()
            Me.m_lbRuns.Items.Add(SharedResources.GENERIC_VALUE_ALL)
            For iRun As Integer = 1 To Me.m_zgp.NumRuns
                Me.m_lbRuns.Items.Add(Me.m_zgp.RunLabel(iRun - 1))
            Next
            Me.m_lbRuns.SelectedIndex = 0
            Me.m_lbRuns.ResumeLayout()

        End Sub

        Private Sub PopulateGroupBox()

            Me.m_lbGroups.Populate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Populate the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub PopulateGraph()

            Dim pplData As New PointPairList()
            Dim src As cEcosimGroupOutput = Nothing
            Dim dValue As Double = 0.0#
            Dim iGroup As Integer = 0
            Dim bIncludeDataPoint As Boolean = True
            Dim lLines As New List(Of LineItem)

            ' Safety checks
            If (Me.m_zgp Is Nothing) Then Return
            If (Not Me.m_zgp.IsReady) Then Me.m_zgp.Clear() : Return
            If (Not Me.Core.StateMonitor.HasEcosimRan) Then Return

            Dim groupPathOut As cEcoPathGroupOutput = Nothing

            ' Clear curves out of current run, if applicable
            Me.m_zgp.ResetRun()

            ' Set title
            Select Case Me.m_plotData
                Case eMSEPlotData.Biomass
                    If Me.m_bIsCumulative Then
                        m_zgp.DataName = SharedResources.HEADER_BIOMASS_CUMULATIVE
                    Else
                        m_zgp.DataName = SharedResources.HEADER_RELATIVEBIOMASS
                    End If
                Case eMSEPlotData.GroupCatch
                    If Me.m_bIsCumulative Then
                        m_zgp.DataName = SharedResources.HEADER_CATCH_CUMULATIVE
                    Else
                        m_zgp.DataName = SharedResources.HEADER_RELATIVE_CATCH
                    End If
                Case eMSEPlotData.Value
                    If Me.m_bIsCumulative Then
                        m_zgp.DataName = SharedResources.HEADER_VALUE_CUMULATIVE
                    Else
                        m_zgp.DataName = SharedResources.HEADER_RELATIVE_VALUE
                    End If
                Case Else
                    Debug.Assert(False, "Data " & Me.m_plotData.ToString & " not supported by this graph")
            End Select

            ' For all groups in the group list box
            For iGroupItem As Integer = 1 To Me.m_lbGroups.Items.Count - 1
                ' Get actual group index
                iGroup = Me.m_lbGroups.GetGroupIndexAt(iGroupItem)
                ' Is a group?
                If (iGroup > 0) Then

                    ' Yes: Create data list
                    pplData = New PointPairList()

                    pplData.Add(Me.Core.EcosimFirstYear, Me.GetStartValue(iGroup))
                    src = Me.Core.EcoSimGroupOutputs(iGroup)

                    For iTimeStep As Integer = 1 To Core.nEcosimTimeSteps

                        dValue = CDbl(Me.GetEcosimValue(iGroup, iTimeStep))

                        ' Determine if datapoint should be displayed
                        bIncludeDataPoint = (Me.IsAnnualPlot = False) Or (iTimeStep Mod cCore.N_MONTHS = 0)

                        ' Should datapoint be displayed?
                        If bIncludeDataPoint Then
                            ' #Yes: display it
                            pplData.Add(CDbl(iTimeStep / cCore.N_MONTHS) + Math.Max(1, Core.EcosimFirstYear), dValue)
                        End If

                    Next iTimeStep

                    ' Add line
                    lLines.Add(Me.m_zgp.CreateLine(Me.Core.EcoPathGroupInputs(iGroup), pplData))

                End If

            Next iGroupItem

            Me.m_graph.GraphPane.XAxis.Scale.Min = Math.Max(Core.EcosimFirstYear, 1)
            Me.m_graph.GraphPane.XAxis.Scale.Max = Core.EcoSimModelParameters.NumberYears + Core.EcosimFirstYear

            ' Draw timeseries 
            If Me.Core.nTimeSeriesEnabled > 0 Then
                Me.AddTimeSeriesLines(lLines)
            End If

            ' Calculate the Axis Scale Ranges
            Me.UpdateControls()
            Me.UpdateGraphHighlights()

            Me.m_zgp.PlotLines(lLines.ToArray(), 1, True, Me.m_zgp.ShowMultipleRuns = False, Me.m_bIsCumulative)

            Me.AlignSketchpad()

        End Sub

        Private Sub AlignSketchpad()
            Dim rcf As RectangleF = Me.m_zgp.GetPane(1).Chart.Rect()
            Me.m_sketchPad.SuspendLayout()
            Me.m_sketchPad.Dock = DockStyle.None
            Me.m_sketchPad.Location = New Point(CInt(rcf.X), Me.m_sketchPad.Location.Y)
            Me.m_sketchPad.Width = CInt(rcf.Width)
            Me.m_sketchPad.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
            Me.m_sketchPad.ResumeLayout()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Place all available time series on the graph for the current data
        ''' plot type.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub AddTimeSeriesLines(ByVal lLines As List(Of LineItem))

            Dim ppl As New PointPairList()
            Dim ts As cTimeSeries = Nothing
            Dim gts As cGroupTimeSeries = Nothing
            Dim group As cEcoPathGroupInput = Nothing
            Dim StartBio As Single = 0.0!
            Dim EDataQ As Single = 0.0!
            Dim tsInterval As eTSDataSetInterval

            ' Only plot time series for biomass 
            If (Me.m_plotData <> eMSEPlotData.Biomass) Then Return
            ' Only plot data when NOT showing cumulative data
            If (Me.m_bIsCumulative) Then Return
            ' Only if there is an Active Timeseries dataset
            If Core.ActiveTimeSeriesDatasetIndex < 1 Then Return

            'Get the time series interval for the currently loaded dataset
            tsInterval = Core.TimeSeriesDataset(Core.ActiveTimeSeriesDatasetIndex).TimeSeriesInterval

            ' For all time series
            For iTS As Integer = 1 To Core.nTimeSeries
                ' Get TS
                ts = Core.EcosimTimeSeries(iTS)
                ' Is ts usable?
                If ((ts.TimeSeriesType = eTimeSeriesType.BiomassRel) Or (ts.TimeSeriesType = eTimeSeriesType.BiomassAbs)) And _
                   (ts.Enabled = True) Then

                    ' Sanity check
                    Debug.Assert(TypeOf ts Is cGroupTimeSeries, "Relative Biomass TS should be cGroupTimeSeries object, check for import")

                    gts = DirectCast(ts, cGroupTimeSeries)
                    group = Me.Core.EcoPathGroupInputs(gts.GroupIndex)
                    ppl = New PointPairList()

                    'Scaling values for relative and actual observed biomass values (reference data)
                    'BiomassRel (relative value)scale values by exp(DataQ) DataQ = mle mean(sumof(log(observed/predicted))
                    'BiomassAbs (actual value) scale to relative [b(t)]/[b(0)] no statistical scaling
                    If (ts.TimeSeriesType = eTimeSeriesType.BiomassAbs) Then
                        'don't use the stat scaler for actual values
                        EDataQ = 1
                    Else
                        EDataQ = gts.eDataQ
                    End If
                    StartBio = Core.StartBiomass(gts.GroupIndex)

                    'delta t for the monthly data
                    Dim Dt As Double = 1
                    Dim halfDt As Double = 0.5
                    Dim xpos As Double
                    Dim ndatapoints As Integer = 0
                    Dim iYear As Integer = Core.EcosimFirstYear

                    'Set timestep variables based on the dataset interval
                    'number of data points 
                    'delta t 
                    'half delta t 
                    Select Case tsInterval
                        Case eTSDataSetInterval.Annual
                            ndatapoints = Core.EcoSimModelParameters.NumberYears
                            Dt = 1
                            halfDt = 0.5
                        Case eTSDataSetInterval.TimeStep
                            ndatapoints = Core.nEcosimTimeSteps
                            Dt = 1 / cCore.N_MONTHS
                            halfDt = Dt * 0.5
                        Case Else
                            Debug.Assert(False)
                    End Select

                    For iDataPoint As Integer = 1 To ndatapoints
                        If iDataPoint < gts.ShapeData().Length Then
                            If gts.ShapeData(iDataPoint) > 0 Then
                                'Select Case tsInterval
                                '    Case eTSDataSetInterval.Annual
                                '        'Shift the xpos to the middle of the timestep
                                '        xpos = iYear + iDataPoint - 0.5
                                '    Case eTSDataSetInterval.Monthly
                                '        xpos = iYear + iDataPoint * dt - dt * 0.5
                                'End Select
                                xpos = iYear + iDataPoint * Dt - halfDt
                                ppl.Add(xpos, (gts.ShapeData()(iDataPoint) / EDataQ) / StartBio)

                            End If
                        End If
                    Next

                    ' Add line to graph.
                    lLines.Add(Me.m_zgp.CreateLine(gts, ppl, cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, ts.Name, group.Name)))
                End If
            Next iTS

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get an Ecosim value for a given group and time step.
        ''' </summary>
        ''' <param name="iGroup"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function GetStartValue(ByVal iGroup As Integer) As Single

            Dim src As cEcoPathGroupOutput = Me.Core.EcoPathGroupOutputs(iGroup)

            ' Get data point value
            Select Case Me.m_plotData
                Case eMSEPlotData.Biomass
                    Return CSng(If(Me.m_bIsCumulative, src.Biomass, 1))
                Case eMSEPlotData.GroupCatch
                    Return CSng(If(Me.m_bIsCumulative, src.TcatchOutput, 1))
                Case eMSEPlotData.Value
                    ' ToDo: resolve group value
                    Return CSng(If(Me.m_bIsCumulative, 0, 1))
            End Select

            Return cCore.NULL_VALUE

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get an Ecosim value for a given group and time step.
        ''' </summary>
        ''' <param name="iGroup"></param>
        ''' <param name="iTimeStep"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function GetEcosimValue(ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Single

            Dim src As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

            ' Get data point value
            Select Case Me.m_plotData
                Case eMSEPlotData.Biomass
                    Return CSng(If(Me.m_bIsCumulative, src.Biomass(iTimeStep), src.BiomassRel(iTimeStep)))
                Case eMSEPlotData.GroupCatch
                    Return CSng(If(Me.m_bIsCumulative, src.Catch(iTimeStep), src.CatchRel(iTimeStep)))
                Case eMSEPlotData.Value
                    Return CSng(If(Me.m_bIsCumulative, src.Value(iTimeStep), src.ValueRel(iTimeStep)))
            End Select

            Return cCore.NULL_VALUE

        End Function

        Private Sub SortGroupsAtTimestep(ByVal iTimeStep As Integer)

            Dim iGroup As Integer = 0
            Dim sValue As Single = 0.0

            If (Me.m_zgp.NumRuns < 1) Then Return
            Debug.Assert(Me.IsExploring = True)

            'Me.m_lbGroups.Sorted = False

            For i As Integer = 0 To Me.m_lbGroups.Items.Count
                iGroup = Me.m_lbGroups.GetGroupIndexAt(i)
                If (iGroup > 0) Then
                    ' Grab value from data
                    'sValue = Me.GetDataPoint(iGroup, iTimeStep)
                    sValue = CSng(Me.m_zgp.GetValueAt(iGroup, Me.m_zgp.NumRuns - 1, iTimeStep))
                    ' Set sort value
                    If Me.m_bIsCumulative Then
                        ' Set this to sort value
                        Me.m_lbGroups.SortValue(iGroup) = sValue
                    Else
                        ' Set this to sort value
                        Me.m_lbGroups.SortValue(iGroup) = CSng(Math.Abs(sValue - 1.0))
                    End If
                End If
            Next

            'Me.m_lbGroups.Sorted = True

            Me.m_lbGroups.Refresh()

        End Sub

        Private Function GetSelectedGroupOrFleet() As ICoreInterface
            Dim item As Object = Me.m_tscbTarget.SelectedItem
            Return DirectCast(item, cCoreInputOutputControlItem).Source
        End Function

        ''' <summary>
        ''' Load fishing effort data from the Fishing Rate manager 
        ''' </summary>
        Private Sub LoadFishingRateShape()

            Dim item As ICoreInterface = Me.GetSelectedGroupOrFleet()
            Dim shape As cShapeData = Nothing

            ' Mortality shapes are 0-base indexed, fleets are 1-base indexed
            If (item Is Nothing) Then
                ' Get 'all fleets' effort shape
                shape = Core.FishingEffortShapeManager.Item(Me.Core.nFleets)
            Else
                ' Get individual effort shape
                shape = Core.FishingEffortShapeManager.Item(item.Index - 1)
            End If

            If (Not TypeOf Me.m_shapeGUIHandler Is cFishingEffortShapeGUIHandler) Then
                If (Not Me.m_shapeGUIHandler Is Nothing) Then
                    Me.m_shapeGUIHandler.Detach()
                    Me.m_shapeGUIHandler = Nothing
                End If
                Me.m_shapeGUIHandler = New cFishingEffortShapeGUIHandler(Me.UIContext)
                Me.m_shapeGUIHandler.Attach(Nothing, Nothing, Me.m_sketchPad, Nothing)
            End If

            Me.m_shapeGUIHandler.SelectedShape = shape
            Me.m_sketchPad.Editable = True
            Me.m_bIsEffortSelected = True
            Me.UpdateControls()

        End Sub

        'Fish Rate (Y/B)
        Private Sub LoadFishMortShape()

            Dim item As ICoreInterface = Me.GetSelectedGroupOrFleet()
            Dim shape As cShapeData = Nothing

            ' Mortality shapes are 0-base indexed, groups are 1-base indexed
            shape = Core.FishMortShapeManager.Item(item.Index - 1)

            If (Not TypeOf Me.m_shapeGUIHandler Is cFishingMortalityShapeGUIHandler) Then
                If (Not Me.m_shapeGUIHandler Is Nothing) Then
                    Me.m_shapeGUIHandler.Detach()
                    Me.m_shapeGUIHandler = Nothing
                End If
                Me.m_shapeGUIHandler = New cFishingMortalityShapeGUIHandler(Me.UIContext)
                Me.m_shapeGUIHandler.Attach(Nothing, Nothing, Me.m_sketchPad, Nothing)
            End If

            Me.m_shapeGUIHandler.SelectedShape = shape
            Me.m_bIsEffortSelected = False
            Me.UpdateControls()
        End Sub

        Private Sub ClearShape()
            Me.m_sketchPad.Shape = Nothing
            Me.UpdateControls()
        End Sub

        Private Property SelectionMode() As eSelectionModeType
            Get
                Return Me.m_selectionMode
            End Get
            Set(ByVal value As eSelectionModeType)

                If (value <> Me.SelectionMode) Then
                    Me.m_selectionMode = value
                    Me.PopulateTargetComboBox()
                    Me.UpdateControls()
                End If
            End Set
        End Property

        Protected Overrides Sub UpdateControls()

            If (Me.m_zgp Is Nothing) Then Return

            Me.m_bInUpdate = True

            Dim bHasTS As Boolean = (Me.Core.ActiveTimeSeriesDatasetIndex >= 1)

            Me.m_btnRun.Enabled = Not Me.IsRunning
            Me.m_btnStop.Enabled = Me.IsRunning

            ' Reset buttons
            Me.m_tsbnSetToValue.Enabled = (Me.m_sketchPad.Shape IsNot Nothing)
            Me.m_tsbnSetTo0.Enabled = (Me.m_sketchPad.Shape IsNot Nothing)
            Me.m_tsbnResetFs.Enabled = True

            Me.m_tsmiShowAnnualOutput.Checked = Me.m_bIsAnnual

            Me.m_tsmiBiomassAbs.Checked = (Me.m_plotData = eMSEPlotData.Biomass) And Me.m_bIsCumulative
            Me.m_tsmiBiomassRel.Checked = (Me.m_plotData = eMSEPlotData.Biomass) And Not Me.m_bIsCumulative
            Me.m_tsmiCatchAbs.Checked = (Me.m_plotData = eMSEPlotData.GroupCatch) And Me.m_bIsCumulative
            Me.m_tsmiCatchRel.Checked = (Me.m_plotData = eMSEPlotData.GroupCatch) And Not Me.m_bIsCumulative
            Me.m_tsmiValueAbs.Checked = (Me.m_plotData = eMSEPlotData.Value) And Me.m_bIsCumulative

            Me.m_tsmiAutoscale.Checked = (Me.m_zgp.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.MaxOnly)
            Me.m_tsmiCustomScaleLabel.Checked = Not m_tsmiAutoscale.Checked
            Me.m_tstbMax.Text = CStr(Me.m_zgp.YScaleMax)
            Me.m_tstbMin.Text = CStr(Me.m_zgp.YScaleMin)

            If Me.IsExploring Then
                Me.m_lbGroups.SortThreshold = Me.m_sChangeTrackSize
                Me.m_lbGroups.SortType = cGroupListBox.eSortType.ValueDesc
                Me.m_tsmiSort.Checked = True
                Me.m_tsbnExplore.Checked = True
            Else
                Me.m_lbGroups.SortThreshold = cCore.NULL_VALUE
                Me.m_lbGroups.SortType = cGroupListBox.eSortType.GroupIndexAsc
                Me.m_tsmiSort.Checked = False
                Me.m_tsbnExplore.Checked = False
            End If

            Me.m_tsbnFleet.Checked = (Me.SelectionMode = eSelectionModeType.Fleets)
            Me.m_tsbnGroup.Checked = (Me.SelectionMode = eSelectionModeType.Groups)

            Me.m_hdrRuns.Visible = Me.m_zgp.ShowMultipleRuns
            Me.m_lbRuns.Visible = Me.m_zgp.ShowMultipleRuns
            Me.m_tsbnShowMultipleRuns.Checked = Me.m_zgp.ShowMultipleRuns
            Me.m_tstbChangeAmount.Text = CStr(Me.m_sChangeTrackSize)

            Me.m_tsbnSetToValue.Enabled = Me.m_bIsEffortSelected
            Me.m_tsbnSetTo0.Enabled = Me.m_bIsEffortSelected

            Me.m_tsbnExplore.Enabled = (Me.Core.StateMonitor.HasEcosimRan)
            Me.m_tsbnSaveOutput.Checked = Me.Core.Autosave(eAutosaveTypes.EcosimResults)

            ' Show SS controls only when time series are loaded
            Me.m_tsblbSS.Visible = bHasTS
            Me.m_tslblSSValue.Visible = bHasTS

            Me.m_bInUpdate = False

        End Sub

        Public Sub ResetGraph()
            Me.m_zgp.Clear()
            Me.PopulateRunsBox()
            Me.PopulateGroupBox()
        End Sub

        ''' <summary>
        ''' Highlight selected groups
        ''' </summary>
        Private Sub UpdateGraphHighlights()

            Dim iItem As Integer = 0
            Dim iGroup As Integer = 0
            Dim iRun As Integer = 0

            Me.m_zgp.ClearHighlights()
            For Each iRun In Me.m_lbRuns.SelectedIndices
                For Each iItem In Me.m_lbGroups.SelectedIndices
                    iGroup = Math.Max(0, Me.m_lbGroups.GetGroupIndexAt(iItem))
                    Me.m_zgp.Highlight(iGroup, iRun - 1)
                Next
            Next iRun

            Me.m_graph.Invalidate()

        End Sub

        Private Sub UpdateSS()

            Dim sSS As Single = 0.0!
            Dim iGroup As Integer = 0
            Dim iNumGroupsSelected As Integer = 0

            For Each iItem As Integer In Me.m_lbGroups.SelectedIndices
                iGroup = Math.Max(0, Me.m_lbGroups.GetGroupIndexAt(iItem))
                If iGroup = 0 Then
                    sSS = Me.Core.EcosimStats.SS
                    Exit For
                Else
                    sSS += Me.Core.EcosimStats.SSGroup(iGroup)
                    iNumGroupsSelected += 1
                End If
            Next

            Try
                If iNumGroupsSelected = 0 Then
                    Me.m_tsblbSS.Text = My.Resources.ECOSIM_HEADER_SS
                Else
                    Me.m_tsblbSS.Text = My.Resources.ECOSIM_HEADER_SS_GROUPS
                End If
                Me.m_tslblSSValue.Text = Me.StyleGuide.FormatNumber(sSS)
            Catch ex As Exception

            End Try

        End Sub

        Private Sub PopulateTargetComboBox()

            Me.m_tscbTarget.Items.Clear()

            Select Case Me.SelectionMode
                Case eSelectionModeType.Fleets
                    Me.m_tscbTarget.Items.Add(New cCoreInputOutputControlItem(SharedResources.GENERIC_VALUE_ALL))
                    For i As Integer = 1 To Me.Core.nFleets
                        Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(i)
                        Me.m_tscbTarget.Items.Add(New cCoreInputOutputControlItem(fleet))
                    Next

                Case eSelectionModeType.Groups
                    For i As Integer = 1 To Me.Core.nGroups
                        Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
                        If (group.IsFished) Then
                            Me.m_tscbTarget.Items.Add(New cCoreInputOutputControlItem(group))
                        End If
                    Next

                Case Else
                    Debug.Assert(False)

            End Select

            If (Me.m_tscbTarget.Items.Count > 0) Then
                Me.m_tscbTarget.SelectedIndex = 0
            End If

        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
