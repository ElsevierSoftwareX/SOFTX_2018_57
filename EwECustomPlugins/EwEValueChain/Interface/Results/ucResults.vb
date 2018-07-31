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

Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph
Imports EwECore.Style
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class ucResults

#Region " Helper classes "

    Private Class cCoreComboItem

        Private m_source As cCoreInputOutputBase
        Private m_strLabel As String

        Public Sub New(ByVal source As cCoreInputOutputBase)
            Me.m_source = source
        End Sub

        Public Sub New(ByVal strLabel As String)
            Me.m_strLabel = strLabel
        End Sub

        Public Overrides Function ToString() As String
            If Me.m_source Is Nothing Then Return Me.m_strLabel
            Dim ftm As New cCoreInterfaceFormatter()
            Return ftm.GetDescriptor(Me.m_source)
        End Function

        Public ReadOnly Property Source() As cCoreInputOutputBase
            Get
                Return Me.m_source
            End Get
        End Property

    End Class

    Private Class cUnitComboItem

        Private m_source As cUnit

        Public Sub New(ByVal source As cUnit)
            Me.m_source = source
        End Sub

        Public Overrides Function ToString() As String
            If Me.m_source Is Nothing Then Return SharedResources.GENERIC_VALUE_ALL
            Return Me.m_source.Name.Trim
        End Function

        Public ReadOnly Property Source() As cUnit
            Get
                Return Me.m_source
            End Get
        End Property

    End Class

    Private Class cYearComboItem

        Private m_iYear As Integer
        Private m_strLabel As String

        Public Sub New(ByVal iYear As Integer, ByVal strLabel As String)
            Me.m_iYear = iYear
            Me.m_strLabel = strLabel
        End Sub

        Public Overrides Function ToString() As String
            Return Me.m_strLabel
        End Function

        Public ReadOnly Property Year() As Integer
            Get
                Return Me.m_iYear
            End Get
        End Property

    End Class

    Private Class cAggregationComboItem

        Private m_agg As cParameters.eAggregationModeType

        Public Sub New(agg As cParameters.eAggregationModeType)
            Me.m_agg = agg
        End Sub

        Public Overrides Function ToString() As String
            Dim fmt As New cAggregationModeTypeFormatter()
            Return fmt.GetDescriptor(Me.m_agg)
        End Function

        Public ReadOnly Property AggregationMode As cParameters.eAggregationModeType
            Get
                Return Me.m_agg
            End Get
        End Property

    End Class

#End Region ' Helper classes

#Region " Private bits "

    Private Shared g_iLastItem As Integer = 0
    Private Shared g_iLastUnit As Integer = 0

    Private Enum eViewModeType As Integer
        Grid = 0
        Graph
        GraphEquilibrium
    End Enum

    ''' <summary>Value chain data that provides, units, links etc.</summary>
    Private m_data As cData = Nothing
    ''' <summary>Instance of the Ecost model to poke and prod.</summary>
    Private m_model As cModel = Nothing
    ''' <summary>Instance of model results to reflect.</summary>
    Private m_result As cResults = Nothing
    ''' <summary>UI context to operate on.</summary>
    Private m_uic As cUIContext = Nothing

    ''' <summary>Viewmode dictates what type of result screen the user sees.</summary>
    Private m_viewMode As eViewModeType = eViewModeType.Graph
    ''' <summary>Graphmode dictates what data is viewed in result graphs.</summary>
    Private m_graphmode As cResults.eGraphDataType = cResults.eGraphDataType.CostRevenue
    ''' <summary>Current view to update when triggers arrive.</summary>
    Private m_view As IResultView = Nothing
    ''' <summary>Update feedback prevention flaggibit.</summary>
    Private m_bInUpdate As Boolean = False

    ''' <summary>Local command to for running Ecopath.</summary>
    Private m_cmdRunEcopath As cCommand = Nothing
    ''' <summary>Local command to for running Ecosim.</summary>
    Private m_cmdRunEcosim As cCommand = Nothing
    ''' <summary>Local command to for running Equilibrium.</summary>
    Private m_cmdRunEqulibrium As cCommand = Nothing

#End Region ' Private bits

#Region " Constructor "

    Public Sub New(ByVal uic As cUIContext, _
                   ByVal data As cData, _
                   ByVal model As cModel, _
                   ByVal result As cResults)

        Me.InitializeComponent()

        Me.m_uic = uic
        Me.m_data = data
        Me.m_model = model
        Me.m_result = result

        Dim cmdH As cCommandHandler = Me.m_uic.CommandHandler

        ' Set up commands
        Me.m_cmdRunEcopath = New cCommand(cmdH, "VC_RunEcopath")
        Me.m_cmdRunEcopath.AddControl(Me.m_btnRunEcopath)
        AddHandler Me.m_cmdRunEcopath.OnInvoke, AddressOf OnInvokeRunEcopath
        AddHandler Me.m_cmdRunEcopath.OnUpdate, AddressOf OnUpdateRunEcopath

        Me.m_cmdRunEcosim = New cCommand(cmdH, "VC_RunEcosim")
        Me.m_cmdRunEcosim.AddControl(Me.m_btnRunEcosim)
        AddHandler Me.m_cmdRunEcosim.OnInvoke, AddressOf OnInvokeRunEcosim
        AddHandler Me.m_cmdRunEcosim.OnUpdate, AddressOf OnUpdateRunEcosim

        Me.m_cmdRunEqulibrium = New cCommand(cmdH, "VC_RunEqulibrium")
        Me.m_cmdRunEqulibrium.AddControl(Me.m_btnRunEquilibrium)
        AddHandler Me.m_cmdRunEqulibrium.OnInvoke, AddressOf OnInvokeRunEquilibrium
        AddHandler Me.m_cmdRunEqulibrium.OnUpdate, AddressOf OnUpdateRunEquilibrium

        Me.Initialize()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler

                cmdh.Remove(Me.m_cmdRunEcopath)
                RemoveHandler Me.m_cmdRunEcopath.OnInvoke, AddressOf OnInvokeRunEcosim
                RemoveHandler Me.m_cmdRunEcopath.OnUpdate, AddressOf OnUpdateRunEcosim
                Me.m_cmdRunEcopath = Nothing

                cmdh.Remove(Me.m_cmdRunEcosim)
                RemoveHandler Me.m_cmdRunEcosim.OnInvoke, AddressOf OnInvokeRunEcosim
                RemoveHandler Me.m_cmdRunEcosim.OnUpdate, AddressOf OnUpdateRunEcosim
                Me.m_cmdRunEcosim = Nothing

                cmdh.Remove(Me.m_cmdRunEqulibrium)
                RemoveHandler Me.m_cmdRunEqulibrium.OnInvoke, AddressOf OnInvokeRunEquilibrium
                RemoveHandler Me.m_cmdRunEqulibrium.OnUpdate, AddressOf OnUpdateRunEquilibrium
                Me.m_cmdRunEcosim = Nothing

                If components IsNot Nothing Then
                    components.Dispose()
                End If

            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region ' Constructor

#Region " Events "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_tsbnSave.Image = SharedResources.saveHS

        Dim iSel As Integer = 0

        Me.m_tscmbGraphData.Items.Clear()
        For Each gd As cResults.eGraphDataType In [Enum].GetValues(GetType(cResults.eGraphDataType))
            Me.m_tscmbGraphData.Items.Add(gd)
        Next
        Me.m_tscmbGraphData.SelectedIndex = 0

        Me.m_cmbAgg.Items.Clear()
        For Each agg As cParameters.eAggregationModeType In [Enum].GetValues(GetType(cParameters.eAggregationModeType))
            Dim i As Integer = Me.m_cmbAgg.Items.Add(New cAggregationComboItem(agg))
            If agg = Me.m_data.Parameters.AggregationMode Then iSel = i
        Next
        Me.m_cmbAgg.SelectedIndex = iSel

        ' Restore last selections
        Me.m_tscmbItems.SelectedIndex = Math.Min(Me.m_tscmbItems.Items.Count - 1, Math.Max(-1, ucResults.g_iLastItem))
        Me.m_tscmbUnit.SelectedIndex = Math.Min(Me.m_tscmbUnit.Items.Count - 1, Math.Max(-1, ucResults.g_iLastUnit))

        ' Initialize view
        Select Case Me.m_result.RunType
            Case cModel.eRunTypes.Ecopath : Me.SetViewMode(eViewModeType.Grid)
            Case cModel.eRunTypes.Ecosim : Me.SetViewMode(eViewModeType.Graph)
            Case cModel.eRunTypes.Equilibrium : Me.SetViewMode(eViewModeType.GraphEquilibrium)
        End Select

        Me.UpdateYearCombo()
        Me.UpdateResults()
        Me.UpdateControls()

    End Sub

    Private Sub OnFilterByItem(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tscmbItems.SelectedIndexChanged

        ' Filter by fleet
        Dim item As cCoreComboItem = Nothing
        Dim coreitem As cCoreInputOutputBase = Nothing

        If Me.m_bInUpdate Then Return
        Me.m_bInUpdate = True

        ucResults.g_iLastItem = Me.m_tscmbItems.SelectedIndex
        item = DirectCast(Me.m_tscmbItems.SelectedItem, cCoreComboItem)
        Me.m_plFlow.ItemFilter = item.Source

        Me.UpdateControls()
        Me.UpdateFilter()

        Me.m_bInUpdate = False

    End Sub

    Private Sub OnFilterByUnit(ByVal sender As Object, ByVal e As EventArgs) _
        Handles m_tscmbUnit.SelectedIndexChanged

        ' Filter by unit
        Dim item As cUnitComboItem = Nothing
        Dim unit As cUnit = Nothing

        If Me.m_bInUpdate Then Return
        Me.m_bInUpdate = True

        ucResults.g_iLastUnit = Me.m_tscmbUnit.SelectedIndex
        item = DirectCast(Me.m_tscmbUnit.SelectedItem, cUnitComboItem)
        If item IsNot Nothing Then unit = item.Source

        Me.m_plFlow.UnitFilter = unit

        Me.UpdateControls()
        Me.UpdateFilter()

        Me.m_bInUpdate = False

    End Sub

    Private Sub OnDoubleClickedFlow(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_scResults.DoubleClick
        Me.m_tsbShowFlow.Checked = Not m_tsbShowFlow.Checked
        Me.UpdateControls()
    End Sub

    Private Sub OnShowFlow(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbShowFlow.Click
        Me.m_tsbShowFlow.Checked = Not m_tsbShowFlow.Checked
        Me.UpdateControls()
    End Sub

    Private Sub OnShowEcopath(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbEcopath.Click
        Me.SetViewMode(eViewModeType.Grid)
        Me.UpdateResults()
        Me.UpdateControls()
    End Sub

    Private Sub OnShowEcosim(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbEcosim.Click
        Me.SetViewMode(eViewModeType.Graph)
        Me.UpdateResults()
        Me.UpdateControls()
    End Sub

    Private Sub OnShowEquilibrium(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbEquilibrium.Click
        Me.SetViewMode(eViewModeType.GraphEquilibrium)
        Me.UpdateResults()
        Me.UpdateControls()
    End Sub

    Private Sub OnGraphDataSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tscmbGraphData.SelectedIndexChanged
        Me.SetGraphData(DirectCast(Me.m_tscmbGraphData.SelectedItem, cResults.eGraphDataType))
        Me.UpdateResults()
    End Sub

    Private Sub OnYearSelected(sender As Object, e As System.EventArgs) _
        Handles m_tscbYear.SelectedIndexChanged
        Me.UpdateResults()
    End Sub

    Private Sub OnAggregationSelected(sender As System.Object, e As System.EventArgs) _
        Handles m_cmbAgg.SelectedIndexChanged
        Dim sel As Object = Me.m_cmbAgg.SelectedItem
        If (sel Is Nothing) Then Return
        If (Not TypeOf sel Is cAggregationComboItem) Then Return
        Me.m_data.Parameters.AggregationMode = DirectCast(sel, cAggregationComboItem).AggregationMode
    End Sub

    Private Sub OnSaveResults(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnSave.Click
        Try
            Me.m_model.SaveResults(Me.m_data, Me.m_result)
        Catch ex As Exception
            ' Whahoo!
        End Try
    End Sub

#Region " Commands "

    Private Sub OnInvokeRunEcopath(ByVal cmd As cCommand)

        Dim bOldRunFlag As Boolean = Me.m_data.Parameters.RunWithEcopath

        ' Switch to manual run mode
        Me.m_model.IsManualRunMode = True
        Me.m_data.Parameters.RunWithEcopath = True

        cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_RUNNING_ECOPATH)

        Try

            ' Reset results and prepare for receiving Ecopath results
            '    In manual mode the calling process becomes responsible for resetting results
            Me.m_result.Reset(cModel.eRunTypes.Ecopath)
            ' Prepare to display Ecopath results
            Me.SetViewMode(eViewModeType.Grid)
            ' Run Ecopath
            Me.m_data.Core.RunEcoPath()

        Catch ex As Exception
            cLog.Write(ex, "ValueChain::OnInvokeRunEcopath")
        End Try

        ' Switch back to auto run mode
        Me.m_model.IsManualRunMode = False
        Me.m_data.Parameters.RunWithEcopath = bOldRunFlag

        cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

        Me.UpdateFilterCombos()

        ' Reflect results
        Me.UpdateResults()
        Me.UpdateControls()

    End Sub

    Private Sub OnUpdateRunEcopath(ByVal cmd As cCommand)
        Dim csm As cCoreStateMonitor = Me.m_data.Core.StateMonitor
        cmd.Enabled = csm.HasEcopathLoaded And (Not csm.IsEcopathRunning)
    End Sub

    Private Sub OnInvokeRunEcosim(ByVal cmd As cCommand)

        Dim bOldRunFlag As Boolean = Me.m_data.Parameters.RunWithEcosim

        cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_RUNNING_ECOSIM)

        ' Switch to manual run mode
        Me.m_model.IsManualRunMode = True
        Me.m_data.Parameters.RunWithEcosim = True

        Try

            ' Reset cached results
            '    In manual mode the calling process becomes responsible for resetting results
            Me.m_result.Reset(cModel.eRunTypes.Ecosim)
            ' Prepare view
            Me.SetViewMode(eViewModeType.Graph)
            Me.m_data.Core.RunEcoSim()

        Catch ex As Exception
            cLog.Write(ex, "ValueChain::OnInvokeRunEcosim")
        End Try

        ' Switch back to auto run mode
        Me.m_model.IsManualRunMode = False
        Me.m_data.Parameters.RunWithEcosim = bOldRunFlag

        cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

        ' Update results
        Me.UpdateFilterCombos()
        Me.UpdateYearCombo()
        Me.UpdateResults()
        Me.UpdateControls()

    End Sub

    Private Sub OnUpdateRunEcosim(ByVal cmd As cCommand)
        Dim csm As cCoreStateMonitor = Me.m_data.Core.StateMonitor
        cmd.Enabled = csm.HasEcosimLoaded And (Not csm.IsEcosimRunning)
    End Sub

    Private Sub OnInvokeRunEquilibrium(ByVal cmd As cCommand)

        cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_RUNNING_EQUILIBRIUM)
        ' Switch to manual run mode
        Me.m_model.IsManualRunMode = True

        Try
            ' Reset cached results
            '    In manual mode the calling process becomes responsible for resetting results
            Me.m_result.Reset(cModel.eRunTypes.Equilibrium)
            ' Prepare view
            Me.SetViewMode(eViewModeType.GraphEquilibrium)
            ' Run
            Me.m_model.RunEquilibrium(Me.m_data, Me.m_result)
        Catch ex As Exception
            cLog.Write(ex, "ValueChain::OnInvokeRunEquilibrium")
        End Try

        ' Switch back to auto run mode
        Me.m_model.IsManualRunMode = False

        cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

        Me.UpdateFilterCombos()
        '' Process results
        'Me.UpdateResults()
        'Me.UpdateControls()

    End Sub

    Private Sub OnUpdateRunEquilibrium(ByVal cmd As cCommand)
        Dim csm As cCoreStateMonitor = Me.m_data.Core.StateMonitor
        cmd.Enabled = csm.HasEcosimLoaded And (Not csm.IsEcosimRunning)
    End Sub

#End Region ' Commands

#End Region ' Events

#Region " Internals "

    Private m_bInitializing As Boolean = False

    Private Sub Initialize()

        Dim item As cCoreInputOutputBase = Nothing

        Me.m_bInitializing = True

        Me.m_plFlow.Init(Me.m_uic, Me.m_data, Nothing, Nothing)
        Me.UpdateFilterCombos()
        Me.SetViewMode(Me.m_viewMode)

        Me.UpdateFilter()
        Me.UpdateControls()

        Me.m_bInitializing = False

        Me.UpdateResults()

    End Sub

    Private Sub UpdateFilter()
        Me.m_plFlow.SuspendLayout()
        Me.m_plFlow.RebuildFlow()
        Me.m_plFlow.ArrangeGLEE()
        Me.m_plFlow.ResumeLayout()
        Me.UpdateResults()
    End Sub

    Private Sub UpdateResults()

        If Me.m_bInitializing Then Return
        If Me.m_bInUpdate Then Return

        Dim item As cCoreInputOutputBase = Me.m_plFlow.ItemFilter
        Dim iItem As Integer = 0

        If (item IsNot Nothing) Then
            iItem = item.Index
        End If

        If Me.m_view IsNot Nothing Then
            Me.m_view.ShowResults(iItem, Me.m_plFlow.GetFlowUnits(), Me.m_result, Me.SelectedYear())
        End If

    End Sub

    Private Sub UpdateControls()

        Dim bHasSim As Boolean = False
        If Me.m_data IsNot Nothing Then
            If Me.m_data.Core IsNot Nothing Then
                bHasSim = Me.m_data.Core.StateMonitor.HasEcosimRan
            End If
        End If

        Me.m_bInUpdate = True

        Me.m_scResults.Panel1Collapsed = (Me.m_tsbShowFlow.Checked = False)
        Me.m_tsbEcopath.Checked = (Me.m_viewMode = eViewModeType.Grid)
        Me.m_tsbEcosim.Checked = (Me.m_viewMode = eViewModeType.Graph)
        Me.m_tsbEquilibrium.Checked = (Me.m_viewMode = eViewModeType.GraphEquilibrium)

        Me.m_tslItem.Visible = (Me.m_data.Parameters.AggregationMode <> cParameters.eAggregationModeType.FullModel)
        Me.m_tscmbItems.Visible = (Me.m_data.Parameters.AggregationMode <> cParameters.eAggregationModeType.FullModel)
        Me.m_tscmbItems.SelectedItem = Me.GetCoreComboItem(Me.m_plFlow.ItemFilter, Me.m_tscmbItems)

        Me.m_tscmbUnit.SelectedItem = Me.GetUnitComboItem(Me.m_plFlow.UnitFilter, Me.m_tscmbUnit)

        Me.m_tslbYear.Visible = (Me.m_viewMode = eViewModeType.Grid) And (bHasSim)
        Me.m_tscbYear.Visible = (Me.m_viewMode = eViewModeType.Grid) And (bHasSim)
        Me.m_tslblData.Visible = (Me.m_viewMode <> eViewModeType.Grid)
        Me.m_tscmbGraphData.Visible = (Me.m_viewMode <> eViewModeType.Grid)

        Me.m_bInUpdate = False

    End Sub

    Private Sub UpdateYearCombo()

        ' Fill time step drop down
        Dim iYearStart As Integer = Me.m_data.Core.EcosimFirstYear
        Dim iStepsPerYear As Integer = CInt(Me.m_data.Core.nEcosimTimeSteps / Math.Max(1, Me.m_data.Core.nEcosimYears))

        Me.m_tscbYear.Items.Clear()
        For iYear As Integer = 1 To Me.m_data.Core.nEcosimYears
            Me.m_tscbYear.Items.Add(New cYearComboItem(iYear, CStr(iYearStart + iYear)))
        Next
        If (Me.m_tscbYear.Items.Count > 0) Then Me.m_tscbYear.SelectedIndex = 0

    End Sub

    Private Sub UpdateFilterCombos()

        ' Populate items combo
        Me.m_tscmbItems.Items.Clear()
        Select Case Me.m_data.Parameters.AggregationMode

            Case cParameters.eAggregationModeType.FullModel
                ' Nop

            Case cParameters.eAggregationModeType.ByFleet
                Me.m_tscmbItems.Items.Add(New cCoreComboItem(SharedResources.GENERIC_VALUE_ALLFLEETS))
                For i As Integer = 1 To Me.m_data.Core.nFleets
                    Me.m_tscmbItems.Items.Add(New cCoreComboItem(Me.m_data.Core.EcopathFleetInputs(i)))
                Next
                Me.m_tscmbItems.SelectedIndex = 0

            Case cParameters.eAggregationModeType.ByGroup
                Me.m_tscmbItems.Items.Add(New cCoreComboItem(SharedResources.GENERIC_VALUE_ALLGROUPS))
                For i As Integer = 1 To Me.m_data.Core.nGroups
                    Dim grp As cEcoPathGroupInput = Me.m_data.Core.EcoPathGroupInputs(i)
                    If grp.IsFished Then
                        Me.m_tscmbItems.Items.Add(New cCoreComboItem(grp))
                    End If
                Next
                Me.m_tscmbItems.SelectedIndex = 0

        End Select

        ' Populate units combo
        Me.m_tscmbUnit.Items.Clear()
        Me.m_tscmbUnit.Items.Add(New cUnitComboItem(Nothing))
        For iSeq As Integer = 0 To Me.m_data.UnitCount - 1
            Me.m_tscmbUnit.Items.Add(New cUnitComboItem(Me.m_data.Unit(iSeq)))
        Next
        Me.m_tscmbUnit.SelectedIndex = 0

    End Sub

    Private Sub SetViewMode(ByVal viewMode As eViewModeType)

        Dim ctrl As ScrollableControl = Nothing

        ' Store view mode type
        Me.m_viewMode = viewMode

        ' Create new view
        Me.m_scResults.SuspendLayout()
        Me.m_scResults.Panel2.SuspendLayout()
        Me.m_scResults.Panel2.Controls.Clear()

        Select Case viewMode

            Case eViewModeType.Grid
                ctrl = New gridEcopathResult(Me.m_uic)

            Case eViewModeType.Graph
                ctrl = New ucEcosimGraph(Me.m_data, Me.m_uic)

            Case eViewModeType.GraphEquilibrium
                ctrl = New ucEquilibriumGraph(Me.m_uic)

            Case Else
                Debug.Assert(False, "View mode {0} not supported", viewMode.ToString())

        End Select

        Debug.Assert(ctrl IsNot Nothing)
        Debug.Assert(TypeOf ctrl Is IResultView)
        Me.m_view = DirectCast(ctrl, IResultView)

        Debug.Assert(TypeOf ctrl Is Control)
        ctrl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_scResults.Panel2.Controls.Add(ctrl)

        Me.m_scResults.Panel2.ResumeLayout()
        Me.m_scResults.ResumeLayout()

        ' Yippee
        Me.SetGraphData(Me.m_graphmode)

    End Sub

    Private Sub SetGraphData(ByVal graphmode As cResults.eGraphDataType)

        Me.m_graphmode = graphmode
        Me.UpdateControls()

        If Not TypeOf (Me.m_view) Is IGraphView Then Return

        Dim gv As IGraphView = DirectCast(Me.m_view, IGraphView)
        Dim strGraphTitle As String = ""

        ' ToDo: globalize this
        Dim strXAxisLabel As String = If(Me.m_viewMode = eViewModeType.GraphEquilibrium, "Effort", "Year")
        Dim strYAxisLabel As String = ""
        Dim avars() As cResults.eVariableType = Nothing

        Select Case graphmode

            Case cResults.eGraphDataType.CostRevenue
                strGraphTitle = My.Resources.HEADER_REV_COST
                strYAxisLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, My.Resources.HEADER_REV_COST, cUnits.Monetary)
                avars = New cResults.eVariableType() {cResults.eVariableType.RevenueTotal,
                                                      cResults.eVariableType.Cost,
                                                      cResults.eVariableType.Profit}

            Case cResults.eGraphDataType.Cost
                strGraphTitle = My.Resources.HEADER_COST
                strYAxisLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, My.Resources.HEADER_COST, cUnits.Monetary)
                avars = New cResults.eVariableType() {cResults.eVariableType.CostAgriculture,
                                                      cResults.eVariableType.CostInput,
                                                      cResults.eVariableType.CostManagementRoyaltyCertification,
                                                      cResults.eVariableType.CostManagementRoyaltyCertificationObservers,
                                                      cResults.eVariableType.CostRawmaterial}

            Case cResults.eGraphDataType.Revenue
                strGraphTitle = My.Resources.HEADER_REVENUE
                strYAxisLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, My.Resources.HEADER_REVENUE, cUnits.Monetary)
                avars = New cResults.eVariableType() {cResults.eVariableType.RevenueTickets,
                                                      cResults.eVariableType.RevenueSubsidies,
                                                      cResults.eVariableType.RevenueProductsMain,
                                                      cResults.eVariableType.RevenueProductsOther,
                                                      cResults.eVariableType.RevenueAgriculture}

            Case cResults.eGraphDataType.Jobs
                strGraphTitle = My.Resources.HEADER_JOBS
                strYAxisLabel = My.Resources.HEADER_JOBS
                avars = New cResults.eVariableType() {cResults.eVariableType.NumberOfJobsTotal,
                                                      cResults.eVariableType.NumberOfJobsMaleTotal,
                                                      cResults.eVariableType.NumberOfJobsFemaleTotal}
            Case cResults.eGraphDataType.Dependents
                strGraphTitle = My.Resources.HEADER_DEPENDENTS
                strYAxisLabel = My.Resources.HEADER_DEPENDENTS
                avars = New cResults.eVariableType() {cResults.eVariableType.NumberOfDependentsTotal,
                                                      cResults.eVariableType.NumberOfWorkerDependents,
                                                      cResults.eVariableType.NumberOfWorkerFemales,
                                                      cResults.eVariableType.NumberOfWorkerMales,
                                                      cResults.eVariableType.NumberOfOwnerMales,
                                                      cResults.eVariableType.NumberOfOwnerFemales,
                                                      cResults.eVariableType.NumberOfOwnerDependents}

            Case Else
                Debug.Assert(False)

        End Select

        gv.SetData(strGraphTitle, strXAxisLabel, strYAxisLabel, avars)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, returns a cCoreComboItem for a given cCoreInputOutputBase
    ''' instance from a given combo box.
    ''' </summary>
    ''' <param name="source">The source to locate.</param>
    ''' <param name="cmb">The combo box to plunder.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GetCoreComboItem(ByVal source As cCoreInputOutputBase, ByVal cmb As ToolStripComboBox) As cCoreComboItem
        Dim item As cCoreComboItem = Nothing
        For i As Integer = 0 To cmb.Items.Count - 1
            item = DirectCast(cmb.Items(i), cCoreComboItem)
            If ReferenceEquals(source, item.Source) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, returns a cUnitComboItem for a given cUnit.
    ''' instance from a given combo box.
    ''' </summary>
    ''' <param name="source">The source to locate.</param>
    ''' <param name="cmb">The combo box to plunder.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GetUnitComboItem(ByVal source As cUnit, ByVal cmb As ToolStripComboBox) As cUnitComboItem
        Dim item As cUnitComboItem = Nothing
        For i As Integer = 0 To cmb.Items.Count - 1
            item = DirectCast(cmb.Items(i), cUnitComboItem)
            If ReferenceEquals(source, item.Source) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Function SelectedYear() As Integer
        Dim item As cYearComboItem = DirectCast(Me.m_tscbYear.SelectedItem, cYearComboItem)
        If (item Is Nothing) Then Return 0
        Return item.Year
    End Function

#End Region ' Internals 

End Class
