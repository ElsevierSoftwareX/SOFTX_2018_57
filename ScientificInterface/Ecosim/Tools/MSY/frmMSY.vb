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
Imports EwECore.MSY
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region ' Imports

Namespace Ecosim

    ''' <summary>
    ''' Form class for MSY user interface.
    ''' </summary>
    Public Class frmMSY

        'ToDo frmMSY now that it is multithreaded it needs to protect against the user doing stupid things during a run
        'Closing the form!
        'Changing model parameters...


#Region " Privates "

        Private Class cMSYRunResults
            Public SelMode As eMSYFSelectionModeType = eMSYFSelectionModeType.Groups
            Public Selection As Integer = 0
            Public ResultsBase As cMSYFResult = Nothing
            Public ResultsFull As New List(Of cMSYFResult)
            Public OptFull As cMSYOptimum = Nothing
            Public ResultsStat As New List(Of cMSYFResult)
            Public OptStat As cMSYOptimum = Nothing
        End Class

        Private m_manager As cMSYManager = Nothing
        Private m_parms As cMSYParameters = Nothing
        Private m_results As cMSYRunResults = Nothing

        ' **************************
        ' Temporary result cache

        ' Target that cache was filled for
        ' **************************

        ''' <summary>
        ''' Data types that MSY produces.
        ''' </summary>
        Private Enum eViewDataModeType As Integer
            Biomass
            [Catch]
            BiomassAndCatch
            Value
        End Enum

        ''' <summary>Current data viewed in this form.</summary>
        Private m_dataMode As eViewDataModeType = eViewDataModeType.Biomass

        ''' <summary>Graph!</summary>
        Private m_zgh As cZedGraphHelper = Nothing
        ''' <summary>Control update loop detection flag.</summary>
        Private m_bInUpdate As Boolean = False

        ''' <summary>Flag, stating that full compensation assessment data should be shown.</summary>
        Private m_bFullAssessment As Boolean = True
        ''' <summary>Flag, stating that stationary stock assessment data should be shown.</summary>
        Private m_bStatAssessment As Boolean = True

        Private m_fpMaxF As cEwEFormatProvider = Nothing
        Private m_fpNumSteps As cEwEFormatProvider = Nothing
        Private m_fpNumTrialYears As cEwEFormatProvider = Nothing

#End Region ' Privates

#Region " Constructor "

        Public Sub New()
            MyBase.new()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Form overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_manager = Me.Core.MSYManager
            Me.m_parms = Me.m_manager.Parameters
            Me.m_manager.RunStateChangedDelegate = AddressOf Me.OnMSYRunStateChanged

            ' Set up zedgraph
            Me.m_zgh = New cZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_graph)
            Me.m_zgh.ConfigurePane("", "", "", True)
            Me.m_zgh.ShowPointValue = True
            Me.m_zgh.IsTrackVisiblity = True
            Me.m_zgh.AllowDuplicatesOnLegend = True

            Me.m_rbGroup.Checked = (Me.m_parms.FSelectionMode = eMSYFSelectionModeType.Groups)
            Me.m_rbFleet.Checked = (Me.m_parms.FSelectionMode = eMSYFSelectionModeType.Fleets)

            Me.m_fpNumTrialYears = New cEwEFormatProvider(Me.UIContext, Me.m_nudNumTrialYears, GetType(Integer), Me.m_parms.GetVariableMetadata(eVarNameFlags.MSYNumTrialYears))
            Me.m_fpNumTrialYears.Value = Me.m_parms.NumTrialYears

            Me.m_fpMaxF = New cEwEFormatProvider(Me.UIContext, Me.m_nudMaxF, GetType(Single), Me.m_parms.GetVariableMetadata(eVarNameFlags.MSYMaxFishingRate))
            Me.m_fpMaxF.Value = Me.m_parms.MaxFishingRate

            Me.m_fpNumSteps = New cEwEFormatProvider(Me.UIContext, Me.m_nudNumSteps, GetType(Integer))
            Me.m_fpNumSteps.Value = CInt(1 / Me.m_parms.EquilibriumStepSize)

            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            cmd.AddControl(Me.m_tsbnShowHide)

            Me.PopulateSelectionComboBox()
            Me.UpdateControls()

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.MSY, eCoreComponentType.Core}

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            If (Me.UIContext IsNot Nothing) Then

                Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
                cmd.RemoveControl(Me.m_tsbnShowHide)

                Me.m_fpNumTrialYears.Release()
                Me.m_fpMaxF.Release()
                Me.m_fpNumSteps.Release()

                Me.m_zgh.Detach()
                Me.m_zgh = Nothing

                'this may not matter
                Me.m_manager.RunStateChangedDelegate = Nothing

            End If

            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub UpdateControls()

            If (Me.UIContext Is Nothing) Then Return

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True

            Dim bCanRun As Boolean = (Me.m_cmbTarget.SelectedIndex >= 0) And Not Me.Core.StateMonitor.IsBusy
            Dim bIsRunning As Boolean = False
            Dim bHasResults As Boolean = (Me.m_results IsNot Nothing)

            Me.m_plAssessment.Enabled = bHasResults
            Me.m_plData.Enabled = bHasResults
            Me.m_tsbnSaveOutput.Checked = (Me.Core.Autosave(eAutosaveTypes.MSY))

            Me.m_rbBiomass.Checked = (Me.m_dataMode = eViewDataModeType.Biomass)
            Me.m_rbValue.Checked = (Me.m_dataMode = eViewDataModeType.Value)

            Me.m_fpNumSteps.Enabled = (Me.m_rbGroup.Checked)

            Me.m_bInUpdate = False
            Me.m_btnRun.Enabled = bCanRun

#If Not Debug Then
            Me.m_btnTest.Visible = False
#End If

        End Sub

#End Region ' Form overrides

#Region " Events "

        Private Sub OnSelectTarget(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_rbFleet.CheckedChanged, m_rbGroup.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return

            Me.UpdateControls()
            Me.PopulateSelectionComboBox()

        End Sub

        Private Sub OnRunMaxModeChanged(sender As System.Object, e As System.EventArgs)


            If (Me.UIContext Is Nothing) Then Return
            Me.UpdateControls()

        End Sub

        Private Sub OnSaveOutput(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnSaveOutput.Click

            If (Me.UIContext Is Nothing) Then Return

            Me.Core.Autosave(eAutosaveTypes.MSY) = Me.m_tsbnSaveOutput.Checked

            ' If pressed, save results
            If (Me.m_tsbnSaveOutput.Checked) Then Me.m_manager.SaveMSYOutput()

        End Sub

        Private Sub OnViewAssessment(sender As System.Object, e As System.EventArgs) _
            Handles m_rbBoth.CheckedChanged, m_rbFull.CheckedChanged, m_rbStationary.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return

            Me.m_bFullAssessment = Me.m_rbFull.Checked Or Me.m_rbBoth.Checked
            Me.m_bStatAssessment = Me.m_rbStationary.Checked Or Me.m_rbBoth.Checked

            Me.UpdatePlot()
            Me.UpdateControls()

        End Sub

        Private Sub OnRun(sender As System.Object, e As System.EventArgs) _
            Handles m_btnRun.Click
            ' Sanity checks
            Debug.Assert(Not Me.Core.StateMonitor.IsBusy)
            Me.Run(Me.Target())
        End Sub

        Private Sub OnShowDataChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_rbBiomass.CheckedChanged, m_rbCatch.CheckedChanged, m_rbValue.CheckedChanged, m_rbBiomassAndCatch.CheckedChanged

            If Me.m_bInUpdate Then Return

            Try
                If Me.m_rbBiomass.Checked Then
                    Me.DataMode = eViewDataModeType.Biomass
                ElseIf Me.m_rbCatch.Checked Then
                    Me.DataMode = eViewDataModeType.Catch
                ElseIf Me.m_rbBiomassAndCatch.Checked Then
                    Me.DataMode = eViewDataModeType.BiomassAndCatch
                ElseIf Me.m_rbValue.Checked Then
                    Me.DataMode = eViewDataModeType.Value
                End If
            Catch ex As Exception
                ' Smokin' ruins
            End Try

        End Sub

        Private Sub OnRunUnitTest(sender As System.Object, e As System.EventArgs) Handles m_btnTest.Click
            Try
                Me.m_parms.FSelectionMode = eMSYFSelectionModeType.Groups
                Me.m_parms.SelGroupFleetIndex = Me.Target.Index
                Me.m_parms.MaxFishingRate = CSng(Me.m_fpMaxF.Value)
                Me.m_parms.EquilibriumStepSize = 1.0! / CSng(Me.m_fpNumSteps.Value)
                Me.m_parms.NumTrialYears = CInt(Me.m_fpNumTrialYears.Value)
                Me.m_manager.RunMSYEcosimUnitTest()
            Catch ex As Exception
                cLog.Write(ex, "frmMSY::OnRunUnitTest")
            End Try
        End Sub

        Private Sub OnRunFindFMSY(sender As System.Object, e As System.EventArgs) Handles m_btnRunFMSY.Click
            Try
                Me.RunFindFMSY()
            Catch ex As Exception
                cLog.Write(ex, "frmMSY::OnRunFMSY")
            End Try
        End Sub

        Private Sub OnMSYRunStateChanged(ByVal RunState As eMSYRunStates)

            If (Me.m_results Is Nothing) Then Return

            Try

                If RunState = eMSYRunStates.FullCompRunCompleted Then
                    Me.m_results.ResultsFull.AddRange(Me.m_manager.MSYResults)
                    Me.m_results.ResultsBase = Me.m_manager.BaseLineResults
                    Me.m_results.OptFull = Me.m_manager.FMSY

                ElseIf RunState = eMSYRunStates.StationaryRunCompleted Then
                    Me.m_results.ResultsStat.AddRange(Me.m_manager.MSYResults)
                    Me.m_results.OptStat = Me.m_manager.FMSY

                End If

                'Only update the interface if the Run Completed
                'this will not update if the run was stopped
                If (RunState = eMSYRunStates.MSYRunComplete) Then
                    ' Trigger graph update
                    Me.UpdatePlot()
                    ' Update control states
                    Me.UpdateControls()

                End If

            Catch ex As Exception

            End Try

        End Sub

#End Region ' Events

#Region " Internals "

        Public Overrides Sub OnCoreMessage(msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            If (msg.Source = eCoreComponentType.Core And msg.Type = eMessageType.GlobalSettingsChanged) Then
                Me.m_tsbnSaveOutput.Checked = Me.m_manager.IsAutoSaveOutput
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="eViewDataModeType">type of data</see> to show in
        ''' the interface..
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property DataMode As eViewDataModeType
            Get
                Return Me.m_dataMode
            End Get
            Set(value As eViewDataModeType)
                If (value <> Me.m_dataMode) Then
                    Me.m_dataMode = value
                    Me.UpdatePlot()
                    Me.UpdateControls()
                End If
            End Set
        End Property

        Private ReadOnly Property Target As cCoreInputOutputBase
            Get
                ' Get selected group or fleet
                Dim sel As Object = Me.m_cmbTarget.SelectedItem
                If (sel Is Nothing) Then Return Nothing
                Return DirectCast(sel, cCoreInputOutputControlItem).Source
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the content of the group/fleet selection combo box.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub PopulateSelectionComboBox()

            Me.m_cmbTarget.Items.Clear()

            If Me.m_rbGroup.Checked Then
                For i As Integer = 1 To Me.Core.nGroups
                    Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(i)
                    If (group.IsFished) Then
                        Me.m_cmbTarget.Items.Add(New cCoreInputOutputControlItem(group))
                    End If
                Next
            ElseIf Me.m_rbFleet.Checked Then
                For i As Integer = 1 To Me.Core.nFleets
                    Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(i)
                    Me.m_cmbTarget.Items.Add(New cCoreInputOutputControlItem(fleet))
                Next
            End If

            If (Me.m_cmbTarget.Items.Count > 0) Then Me.m_cmbTarget.SelectedIndex = 0
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Run MSY for the selected group/fleet.
        ''' </summary>
        ''' <param name="item">The selected <see cref="cEcoPathGroupInput">group</see> 
        ''' or <see cref="cEcopathFleetInput">fleet</see>.</param>
        ''' -------------------------------------------------------------------
        Private Sub Run(item As cCoreInputOutputBase)

            ' Sanity checks
            Debug.Assert(item IsNot Nothing)

            ' Clear cache
            Me.m_results = New cMSYRunResults()

            If Not Me.m_manager.IsAllowedToRun() Then Return

            Try
                Me.m_parms.SelGroupFleetIndex = item.Index
                Me.m_parms.MaxFishingRate = CSng(Me.m_fpMaxF.Value)
                Me.m_parms.EquilibriumStepSize = 1.0! / CSng(Me.m_fpNumSteps.Value)
                Me.m_parms.NumTrialYears = CInt(Me.m_fpNumTrialYears.Value)

                'If Me.m_rbFixedMax.Checked Then
                Me.m_parms.RunLengthMode = eMSYRunLengthModeTypes.FixedF
                'Else
                'Me.m_parms.RunLengthMode = eMSYRunLengthModeTypes.ToDepletion
                'End If

                If Me.m_rbGroup.Checked Then
                    Me.m_parms.FSelectionMode = eMSYFSelectionModeType.Groups
                Else
                    Me.m_parms.FSelectionMode = eMSYFSelectionModeType.Fleets
                End If

                Me.m_results.SelMode = Me.m_parms.FSelectionMode
                Me.m_results.Selection = Me.m_parms.SelGroupFleetIndex

                'RunThreaded() will run both FullCompensation and StationarySystem on a thread 
                'me.onMSYRunStateChanged(eMSYRunState) will be called during the run with state info
                Me.m_manager.RunMSY()

                'All updating is handled in Me.onMSYRunStateChanged(eMSYRunState)

                'Me.m_parms.Assessment = eMSYAssessmentTypes.FullCompensation
                'Me.m_manager.Run()
                'Me.m_results.ResultsFull.AddRange(Me.m_manager.MSYResults)
                'Me.m_results.ResultsBase = Me.m_manager.BaseLineResults
                'Me.m_results.OptFull = Me.m_manager.FMSY

                'Me.m_parms.Assessment = eMSYAssessmentTypes.StationarySystem
                'Me.m_manager.Run()
                'Me.m_results.ResultsStat.AddRange(Me.m_manager.MSYResults)
                'Me.m_results.OptStat = Me.m_manager.FMSY

            Catch ex As Exception

            End Try

            '' Trigger graph update
            'Me.TotallyAndCompletelyRefreshPlot()
            '' Update control states
            'Me.UpdateControls()

        End Sub

#Region " Plotting "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' This is a H*A*C*K method for testing purposes which will need to be 
        ''' revised. The current implementation bluntly repopulates the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdatePlot()

            If (Me.UIContext Is Nothing) Then Return

            Debug.Assert(Me.m_parms IsNot Nothing)

            Dim grp As cEcoPathGroupInput = Nothing
            Dim lli As New List(Of LineItem)
            Dim strXAxisLabel As String = ""
            Dim strYAxisLabel As String = ""
            Dim strPostFix As String = ""
            Dim strTarget As String = ""
            Dim i As Integer = 0

            Dim axis As Axis = Nothing

            Me.m_zgh.GetPane(1).CurveList.Clear()
            If (Me.m_results Is Nothing) Then Return

            ' Prepare labels
            Select Case Me.DataMode
                Case eViewDataModeType.Biomass
                    ' Add data line to the list of lines to show
                    strYAxisLabel = SharedResources.HEADER_RELATIVEBIOMASS
                Case eViewDataModeType.Catch
                    strYAxisLabel = SharedResources.HEADER_RELATIVE_CATCH
                Case eViewDataModeType.BiomassAndCatch
                    strYAxisLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DOUBLE, _
                                                  SharedResources.HEADER_RELATIVEBIOMASS, _
                                                  SharedResources.HEADER_RELATIVE_CATCH)
                Case eViewDataModeType.Value
                    strYAxisLabel = SharedResources.HEADER_RELATIVE_VALUE
            End Select

            Select Case Me.m_results.SelMode

                Case eMSYFSelectionModeType.Groups
                    strXAxisLabel = SharedResources.HEADER_FISHINGMORTALITY
                    strTarget = Me.Core.EcoPathGroupInputs(Me.m_results.Selection).Name

                Case eMSYFSelectionModeType.Fleets
                    strXAxisLabel = My.Resources.HEADER_FISHING_EFFORT_MULTIPLIER
                    If Me.m_parms.SelGroupFleetIndex = 0 Then
                        strTarget = SharedResources.GENERIC_VALUE_ALLFLEETS
                    Else
                        strTarget = Me.Core.EcopathFleetInputs(Me.m_results.Selection).Name
                    End If

            End Select

            Me.m_zgh.ConfigurePane(cStringUtils.Localize(My.Resources.HEADER_MSY, strTarget), strXAxisLabel, strYAxisLabel, True)
            Dim gp As GraphPane = Me.m_zgh.GetPane(1)
            gp.CurveList.Clear()
            gp.YAxis.Scale.Min = 0
            gp.XAxis.Scale.MinGrace = 0
            gp.XAxis.Scale.Min = 0
            gp.XAxis.Scale.MaxGrace = 0
            gp.XAxis.Scale.MaxAuto = True
            Me.m_zgh.AutoScaleYOption = cZedGraphHelper.eScaleOptionTypes.MaxOnly
            Me.m_zgh.AllowDuplicatesOnLegend = True

            If Me.m_bFullAssessment Then
                strPostFix = ""
                lli.AddRange(Me.GetLines(strTarget, Me.m_results.ResultsFull.ToArray, Me.m_results.OptFull, "", Drawing2D.DashStyle.Solid, True))
                Me.m_zgh.PlotLines(lli.ToArray, bClear:=False)
                lli.Clear()

                i += 1
            End If

            If Me.m_bStatAssessment Then
                If (i > 0) Then
                    strPostFix = My.Resources.HEADER_STATIONARY
                    Me.m_zgh.AllowDuplicatesOnLegend = False
                End If
                lli.AddRange(Me.GetLines(strTarget, Me.m_results.ResultsStat.ToArray, Me.m_results.OptStat, strPostFix, Drawing2D.DashStyle.Dot, False))
                Me.m_zgh.PlotLines(lli.ToArray, bClear:=False)
            End If

            ' Plot lines
            Me.m_zgh.RescaleAndRedraw()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strTarget"></param>
        ''' <param name="results"></param>
        ''' <param name="optimum"></param>
        ''' <param name="strPostfix"></param>
        ''' <param name="style"></param>
        ''' <returns>A list of <see cref="LineItem"/> isntances.</returns>
        ''' -------------------------------------------------------------------
        Private Function GetLines(ByVal strTarget As String, _
                                  ByVal results As cMSYFResult(), _
                                  ByVal optimum As cMSYOptimum, _
                                  ByVal strPostfix As String, _
                                  ByVal style As Drawing2D.DashStyle, _
                                  ByVal bSolidSymbol As Boolean) As LineItem()

            Dim base As cMSYFResult = Me.m_manager.BaseLineResults()
            Dim lli As New List(Of LineItem)

            Dim ppl As PointPairList = Nothing
            Dim li As LineItem = Nothing

            Dim bPlotFMSY As Boolean = False
            Dim pplFMSY As PointPairList = Nothing
            Dim liFMSY As LineItem = Nothing
            Dim grp As cEcoPathGroupInput = Nothing
            Dim strDataName As String = ""
            Dim strLabel As String = ""

            If Me.DataMode = eViewDataModeType.Value Then

                ' Only plot one value line
                ppl = New PointPairList()
                strDataName = SharedResources.HEADER_TOTALVALUE

                For Each r As cMSYFResult In results
                    ' Add to line
                    ppl.Add(r.FCur, r.TotalValue / base.TotalValue)
                Next r

                ' Add value line to the list of lines to show
                li = Me.m_zgh.CreateLineItem(Me.GetLabel(strDataName, strPostfix), eSketchDrawModeTypes.Line, Color.Blue, ppl)
                li.Line.Style = style
                lli.Add(li)

            End If

            If (Me.DataMode = eViewDataModeType.Biomass Or Me.DataMode = eViewDataModeType.BiomassAndCatch) Then

                strDataName = SharedResources.HEADER_RELATIVEBIOMASS

                For i As Integer = 1 To Me.Core.nGroups
                    ' Get group
                    grp = Me.Core.EcoPathGroupInputs(i)
                    ' Determine if group should be shown
                    If Me.StyleGuide.GroupVisible(grp.Index) Then

                        Try
                            ' Make new line for this group
                            ppl = New PointPairList()
                            For Each r As cMSYFResult In results
                                ppl.Add(r.FCur, r.B(i) / base.B(i))
                            Next r

                            ' Create line
                            strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, grp.Name, strDataName)
                            li = Me.m_zgh.CreateLineItem(grp, ppl, Me.GetLabel(strLabel, strPostfix))
                            li.Line.Style = style
                            lli.Add(li)

                        Catch ex As Exception
                            ' Oh my
                            Debug.Assert(False, ex.Message)
                        End Try

                    End If
                Next i

            End If

            If (Me.DataMode = eViewDataModeType.Catch Or Me.DataMode = eViewDataModeType.BiomassAndCatch) Then

                strDataName = SharedResources.HEADER_RELATIVE_CATCH

                Dim flt As cEcopathFleetInput = Nothing
                If Me.m_results.SelMode = eMSYFSelectionModeType.Fleets Then
                    flt = Me.Core.EcopathFleetInputs(Me.m_results.Selection)
                End If

                For i As Integer = 1 To Me.Core.nGroups
                    ' Get group
                    grp = Me.Core.EcoPathGroupInputs(i)

                    ' Only plot fished groups
                    If Me.StyleGuide.GroupVisible(grp.Index) And grp.IsFished Then
                        Try
                            ' Make new line for this group
                            ppl = New PointPairList()
                            For Each r As cMSYFResult In results
                                ppl.Add(r.FCur, r.Catch(i) / base.Catch(i))

                                bPlotFMSY = False
                                If r.FCur = optimum.FOpt(i) Then
                                    If Me.m_results.SelMode = eMSYFSelectionModeType.Groups Then
                                        bPlotFMSY = (i = Me.m_results.Selection)
                                    Else
                                        bPlotFMSY = (flt.Landings(i) > 0 Or flt.Discards(i) > 0)
                                    End If
                                End If

                                If bPlotFMSY Then
                                    pplFMSY = New PointPairList()
                                    pplFMSY.Add(r.FCur, 0)
                                    pplFMSY.Add(r.FCur, r.Catch(i) / base.Catch(i))

                                    strLabel = cStringUtils.Localize(My.Resources.MSY_LABEL_FMSY, grp.Name)
                                    liFMSY = New LineItem(Me.GetLabel(strLabel, strPostfix), pplFMSY, _
                                                          Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT), SymbolType.Diamond)
                                    liFMSY.Line.Style = style
                                    liFMSY.Line.Width = 2
                                    liFMSY.Symbol.Fill.IsVisible = bSolidSymbol
                                    lli.Add(liFMSY)

                                    pplFMSY = Nothing
                                    liFMSY = Nothing
                                    Me.m_zgh.CursorPos = r.FCur
                                End If

                            Next r

                            ' Create line
                            strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, grp.Name, strDataName)
                            li = Me.m_zgh.CreateLineItem(grp, ppl, Me.GetLabel(strLabel, strPostfix))
                            li.Line.Style = style
                            li.Line.Width *= 2
                            lli.Add(li)

                        Catch ex As Exception
                            ' Oh my
                            Debug.Assert(False, ex.Message)
                        End Try

                    End If
                Next i

            End If

            Return lli.ToArray()

        End Function

        Private Function GetLabel(ByVal strLabel As String, ByVal strPostfix As String) As String
            If String.IsNullOrWhiteSpace(strPostfix) Then Return strLabel
            Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, strLabel, strPostfix)
        End Function

#End Region ' Plotting

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Run FMSY.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub RunFindFMSY()

            Dim bSucces As Boolean = True

            If Not Me.m_manager.IsAllowedToRun() Then Return

            Me.m_results = New cMSYRunResults()

            Me.m_parms.MaxFishingRate = CSng(Me.m_fpMaxF.Value)
            Me.m_parms.EquilibriumStepSize = 1.0! / CSng(Me.m_fpNumSteps.Value)
            Me.m_parms.NumTrialYears = CInt(Me.m_fpNumTrialYears.Value)

            'If Me.m_rbFixedMax.Checked Then
            Me.m_parms.RunLengthMode = eMSYRunLengthModeTypes.FixedF
            'Else
            'Me.m_parms.RunLengthMode = eMSYRunLengthModeTypes.ToDepletion
            'End If

            Try
                bSucces = bSucces Or Me.m_manager.RunFindFMSY()
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Internals

    End Class

End Namespace
