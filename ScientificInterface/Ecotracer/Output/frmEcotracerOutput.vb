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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ZedGraph
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Form class, implementing the Ecotracer (contaminant tracing) output interface.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class frmEcotracerOutput

    'ToDo 9-Nov-2016 (First Trump Day!) Stop button on Status Notifier doesn't work. 
    '           When it's used to stop Ecospace it leaves the form controls disabled
    '           and the cursor as a hourglass
    'ToDo 2-Dec-2016 Need a more robust way to tell if Ecospace is running from the Run Ecospace form instead of locally.
    '           When run form Ecospace UI it doesn't know current timestep so it plots past the end of the data.

    'ToDo 6-Feb-2017 If Ecospace w Tracer from the Ecospace UI the Tracer UI needs to know and Update so you can't start a new run
    'ToDo 6-Feb-2017 It is still possible to 'trick' the Tracer UI into thinking Tracer not running when it is. This really needs a good debug.

#Region " Definitions "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type, indicates the form result display mode.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Enum eDisplayModeTypes As Byte
        ''' <summary>No mode has been set yet this make it easier to init the form.</summary>
        NotInitialized
        ''' <summary>No results have been computed yet.</summary>
        NoResults
        ''' <summary>Show Ecosim results.</summary>
        Ecosim
        ''' <summary>Show Ecospace results.</summary>
        Ecospace
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type, indicates possible plot types.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Enum ePlotTypes As Byte
        ''' <summary>Concentration plot.</summary>
        Conc
        ''' <summary>Concentration over biomass plot.</summary>
        CB
    End Enum

#End Region ' Public definitions

#Region " Private vars "

    ''' <summary>Zed graph helper to make the graph look purdy with.</summary>
    Private m_zgh As cZedGraphHelper = Nothing
    ''' <summary>Form display mode.</summary>
    Private m_curDisplayMode As eDisplayModeTypes = eDisplayModeTypes.NotInitialized
    ''' <summary>Form type of plot.</summary>
    Private m_plottype As ePlotTypes = ePlotTypes.CB
    ''' <summary>Thing to gather the data for the form.</summary>
    Private m_DisplayHelper As IDisplayModeHelper = Nothing
    ''' <summary>Update loop prevention flag.</summary>
    Private m_bInUpdate As Boolean = False

    ''' <summary>Value tracker for Conc Sim.</summary>
    Private m_propConcSimOn As cProperty = Nothing
    ''' <summary>Value tracker for Conc Space.</summary>
    Private m_propConcSpaceOn As cProperty = Nothing


    Private m_CurTimeStep As Integer

#End Region ' Private vars

    Public Sub New()
        MyBase.New()
        Me.InitializeComponent()
    End Sub

#Region " Form overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        Me.m_zgh = New cZedGraphHelper()
        Me.m_zgh.Attach(Me.UIContext, Me.m_zgc)
        Me.m_zgh.ConfigurePane("", "", "", True)
        Me.m_lbGroups.Attach(Me.UIContext)

        Me.RefreshData()

        Me.PlotType = ePlotTypes.CB

        Me.m_propConcSimOn = Me.PropertyManager.GetProperty(Me.Core.EcoSimModelParameters, eVarNameFlags.ConSimOnEcoSim)
        Me.m_propConcSpaceOn = Me.PropertyManager.GetProperty(Me.Core.EcospaceModelParameters, eVarNameFlags.ConSimOnEcoSpace)
        AddHandler Me.m_propConcSimOn.PropertyChanged, AddressOf OnConcPropChanged
        AddHandler Me.m_propConcSpaceOn.PropertyChanged, AddressOf OnConcPropChanged

        AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged

        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.Core, eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace}

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

        RemoveHandler Me.m_propConcSimOn.PropertyChanged, AddressOf OnConcPropChanged
        RemoveHandler Me.m_propConcSpaceOn.PropertyChanged, AddressOf OnConcPropChanged
        Me.m_propConcSimOn = Nothing
        Me.m_propConcSpaceOn = Nothing

        Me.m_lbGroups.Detach()
        Me.m_zgh.Detach()
        Me.m_zgh = Nothing

        MyBase.OnFormClosed(e)

    End Sub

    Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)

        ' JS10Apr10: this probably needs to be refined to ONLY include run completed states
        If (msg.Source = eCoreComponentType.EcoSim) Or
           (msg.Source = eCoreComponentType.EcoSpace) Then
            'let the interface update to all core states
            Me.RefreshData()
        End If

        If (msg.Source = eCoreComponentType.Core And msg.Type = eMessageType.GlobalSettingsChanged) Then
            Me.m_cbAutosaveResults.Checked = Me.Core.Autosave(eAutosaveTypes.Ecotracer)
        End If

    End Sub

    Private Sub OnCoreStateChanged(ByVal cms As cCoreStateMonitor)
        Try

            ' If Me.IsActivated Then
            If cms.IsEcospaceRunning <> Me.IsRunning Then

                    Me.m_CurTimeStep = Me.Core.nEcospaceTimeSteps

                    '' Update state flag
                    'Me.IsRunning = cms.IsEcospaceRunning

                    '' Update status feedback
                    'If Me.IsRunning Then
                    '    cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_ECOSPACE_RUNNING)
                    'Else
                    '    cApplicationStatusNotifier.EndProgress(Me.Core)
                    'End If

                    '' Update controls
                    ''    Me.m_lblProgress.Text = ""
                    'Me.UpdateControls()

                End If
            '   End If
        Catch ex As Exception
            'Just swallow it for now???
        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="ScientificInterfaceShared.Forms.frmEwE.IsRunForm" />
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property IsRunForm() As Boolean
        Get
            Return True
        End Get
    End Property

#End Region ' Form overrides

#Region " Events "

    Private Sub OnGroupSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_lbGroups.SelectedIndexChanged
        Me.PlotSelectedGroups()
    End Sub

    Protected Overrides Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
        If ((changeType And cStyleGuide.eChangeType.Colours) > 0) Then
            ' Respond to group colour changes
            Me.PlotSelectedGroups()
        End If
    End Sub

    Private Sub OnRunEcosim(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnRunSim.Click

        Try
            'An Ecosim scenario was loaded when this form was loaded
            'so there is no need to check
            Me.m_bInUpdate = True
            Me.Core.EcoSimModelParameters.ContaminantTracing = True
            Me.m_bInUpdate = False
            Me.StartModelRun()
            Me.Core.RunEcoSim(AddressOf Me.EcosimCallback, True)
            ' Restore state
            Me.RefreshGraph()

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".btRunSim_Click() Error: " & ex.Message)
        End Try

    End Sub

    Private Sub OnRunEcospace(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnRunSpace.Click

        Try
            'No Ecospace scenario has been load
            If Me.Core.StateMonitor.HasEcospaceLoaded = False Then
                'Ask the user for a Ecospace scenario via the command
                Dim cmd As cCommand = Me.CommandHandler.GetCommand("LoadEcospaceScenario")
                Debug.Assert(cmd IsNot Nothing, Me.ToString & ".btRunSpace_Click() LoadEcospaceScenario Command could not be found.")
                cmd.Invoke()
            End If

            'Make sure the scenario loaded successfully before trying to run Ecospace
            If Me.Core.StateMonitor.HasEcospaceLoaded Then
                Me.m_bInUpdate = True
                Me.Core.EcospaceModelParameters.ContaminantTracing = True
                Me.m_bInUpdate = False
                Me.StartModelRun()
                Me.Core.RunEcoSpace(AddressOf Me.EcospaceCallback)
                Me.RefreshGraph()
            End If

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".btRunSpace_Click() Error: " & ex.Message)
        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, called when a plot type radio button checked state has changed.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnPlotTypeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbConc.CheckedChanged, m_rbCB.CheckedChanged

        If (Me.m_DisplayHelper Is Nothing) Then Return

        If Me.m_rbConc.Checked Then
            Me.PlotType = ePlotTypes.Conc
        ElseIf Me.m_rbCB.Checked Then
            Me.PlotType = ePlotTypes.CB
        Else
            Debug.Assert(False, "Radio button group is out of whack")
        End If

    End Sub

    Private Sub OnRegionSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cmbRegions.SelectedIndexChanged
        Me.RefreshGraph()
    End Sub

    'Private Sub OnSortedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    '    Me.RefreshData()
    'End Sub

    Private Sub OnDisplayGroups(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnShowHideGroups.Click
        Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
        Debug.Assert(cmd IsNot Nothing, Me.ToString & ".OnDisplayGroups() DisplayGroups Command could not be found.")
        cmd.Invoke()
    End Sub

    Private Sub OnConcPropChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)

        If Me.m_bInUpdate Then Return
        Me.RefreshData()

    End Sub

    Private Sub OnAutoSaveChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_cbAutosaveResults.CheckedChanged
        Try
            If (Me.UIContext Is Nothing) Then Return
            Me.Core.Autosave(eAutosaveTypes.Ecotracer) = Me.m_cbAutosaveResults.Checked
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Events

#Region " Internal bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the graph plot type.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Property PlotType() As ePlotTypes
        Get
            Return Me.m_plottype
        End Get
        Set(ByVal value As ePlotTypes)
            If (value <> Me.m_plottype) Then
                Me.m_plottype = value
                Me.UpdateControls()
                Me.RefreshData()
                Me.RefreshGraph()
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Prepare the UI for running Ecosim or Ecospace.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub StartModelRun()
        ' Reset progress
        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_ECOTRACER_RUNNING)
        Me.m_CurTimeStep = 0
        Me.UpdateControls()
        Me.IsRunning = True
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update the progress bar in response to a model time step.
    ''' </summary>
    ''' <param name="sProgress">Progress to set [0, 1].</param>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateProgess(ByVal sProgress As Single)

        'the rounding is for Ecospace it never actually gets to 1
        If (Math.Round(sProgress, 3) < 0.999F) Then
            cApplicationStatusNotifier.UpdateProgress(Me.Core, My.Resources.STATUS_ECOTRACER_RUNNING, sProgress)
        Else
            cApplicationStatusNotifier.EndProgress(Me.Core)
            Me.IsRunning = False
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub UpdateControls()

        If Me.m_bInUpdate Then Return

        Me.m_bInUpdate = True

        Me.m_rbCB.Checked = (Me.PlotType = ePlotTypes.CB)
        Me.m_rbConc.Checked = (Me.PlotType = ePlotTypes.Conc)

        Me.m_cbAutosaveResults.Checked = (Me.Core.Autosave(eAutosaveTypes.Ecotracer))

        ' Config controls based on the display helper
        Me.m_zgc.GraphPane.Title.Text = Me.m_DisplayHelper.Title
        Me.m_cmbRegions.Enabled = m_DisplayHelper.SupportsRegions

        Me.m_btnRunSim.Enabled = (Not Me.IsRunning)
        Me.m_btnRunSpace.Enabled = (Not Me.IsRunning)

        Me.m_bInUpdate = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the current display mode.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private ReadOnly Property DisplayMode() As eDisplayModeTypes
        Get
            Dim mode As eDisplayModeTypes = eDisplayModeTypes.NoResults

            'Ecosim
            If (Me.Core.StateMonitor.HasEcosimLoaded) Then
                If Me.Core.EcoSimModelParameters.ContaminantTracing Then
                    mode = eDisplayModeTypes.Ecosim
                End If
            End If

            'Ecospace
            If Me.Core.StateMonitor.HasEcospaceLoaded Then
                If Me.Core.EcospaceModelParameters.ContaminantTracing Then
                    mode = eDisplayModeTypes.Ecospace
                End If
            End If

            Return mode
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Re-populate the interface
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub RefreshData()

        ' Refresh region combobox
        Dim iReg As Integer = Me.m_cmbRegions.SelectedIndex
        Me.m_cmbRegions.Items.Clear()

        If Me.UIContext Is Nothing Then Return

        Dim modeNew As eDisplayModeTypes = Me.DisplayMode

        Try
            'build the correct display mode helper based on the new display mode flag from getDisplayMode
            Me.m_DisplayHelper = Me.DisplayHelperFactory(modeNew)

            If (Me.Core.ActiveEcospaceScenarioIndex > 0) And (Me.m_DisplayHelper.SupportsRegions) Then
                ' 0 item = 0 region; e.g. all cells not assigned to a region
                ' 1 - n = n region
                ' n + 1 = all cells
                Me.m_cmbRegions.Items.Add(SharedResources.GENERIC_VALUE_NONE)
                For i As Integer = 1 To Me.Core.nRegions
                    Me.m_cmbRegions.Items.Add(i)
                Next
                Me.m_cmbRegions.SelectedIndex = Math.Max(0, Math.Min(iReg, Me.m_cmbRegions.Items.Count - 1))
                Me.m_cmbRegions.Items.Add(SharedResources.GENERIC_VALUE_ALL)
            End If

            Me.UpdateControls()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Redraw the graph
    ''' </summary>
    Private Sub RefreshGraph()

        ' Configure display helper
        Me.m_DisplayHelper.PlotType = Me.m_plottype
        If (Me.m_DisplayHelper.SupportsRegions) Then
            Me.m_DisplayHelper.RegionIndex = Me.m_cmbRegions.SelectedIndex
        End If

        ' Configure graph
        Me.m_zgc.GraphPane.Title.Text = m_DisplayHelper.Title
        Me.m_zgc.GraphPane.XAxis.Title.Text = m_DisplayHelper.XAxisLabel
        Me.m_zgc.GraphPane.YAxis.Title.Text = m_DisplayHelper.YAxisLabel
        Me.m_zgc.GraphPane.XAxis.Scale.Min = m_DisplayHelper.FirstYear
        Me.m_zgc.GraphPane.XAxis.Scale.Max = m_DisplayHelper.FirstYear + m_DisplayHelper.nYears

        'JB force the y axis to scale from zero
        'This forces the data from running locally and remotely to plot the same
        Me.m_zgc.GraphPane.YAxis.Scale.Min = 0.0D

        ' Update plot 
        Me.PlotSelectedGroups()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub PlotSelectedGroups()

        Dim lLinesPlot As New List(Of LineItem)
        Dim aLinesGroup() As LineItem = Nothing
        Dim source As cCoreInputOutputBase = Nothing

        If Me.m_DisplayHelper.CanPlot Then
            If Me.m_CurTimeStep > 0 Then

                Try

                    ' Iterate over all selected listbox items
                    For Each iListboxItem As Integer In Me.m_lbGroups.SelectedIndices
                        ' Get source at this item
                        source = Me.m_lbGroups.GetGroupAt(iListboxItem)
                        ' Is environment node?
                        If (source Is Nothing) Then
                            ' #Yes: get environment lines
                            aLinesGroup = Me.m_DisplayHelper.GetGroupLines(0, Me.m_CurTimeStep)
                        Else
                            ' #No: get group lines
                            aLinesGroup = Me.m_DisplayHelper.GetGroupLines(source.Index, Me.m_CurTimeStep)
                        End If
                        ' Add all lines
                        For Each li As LineItem In aLinesGroup
                            ' Is a line?
                            If (li IsNot Nothing) Then
                                ' #Yes: add it
                                lLinesPlot.Add(li)
                            End If
                        Next
                    Next

                    ' Plot all encountered lines
                    Me.m_zgh.PlotLines(lLinesPlot.ToArray())

                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                End Try

            End If
        End If

        Me.m_zgh.RescaleAndRedraw()

    End Sub

    Private Sub EcosimCallback(ByVal iTime As Long, ByVal data As cEcoSimResults)
        Try
            Me.m_CurTimeStep = CInt(iTime)
            If (iTime Mod cCore.N_MONTHS) = 0 Then
                'Me.UpdateProgess(CSng(iTime / Me.m_DisplayHelper.nStepPerYear))
                Me.UpdateProgess(CSng(iTime / Me.Core.nEcosimTimeSteps))
                Me.PlotSelectedGroups()
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub EcospaceCallback(ByRef EcospaceResults As cEcospaceTimestep)
        Try
            ' Me.UpdateProgess(CSng(EcospaceResults.TimeStepinYears / Me.m_DisplayHelper.nYears))
            Me.UpdateProgess(CSng(EcospaceResults.iTimeStep / Me.Core.nEcospaceTimeSteps))
            Me.m_CurTimeStep = EcospaceResults.iTimeStep
            Me.PlotSelectedGroups()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

#Region " Helper methods "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a Display mode helper object based on the newDisplayMode parameter
    ''' </summary>
    ''' <param name="newDisplayMode"></param>
    ''' <returns>If the current display mode matches the newDisplayMode parameter this will return the current IDisplayModeHelper object</returns>
    ''' -----------------------------------------------------------------------
    Private Function DisplayHelperFactory(ByVal newDisplayMode As eDisplayModeTypes) As IDisplayModeHelper

        Dim helper As IDisplayModeHelper = Nothing

        'This will only build a new IDisplayModeHelper if newDisplayMode is different from the current m_curDisplayMode
        If newDisplayMode <> Me.m_curDisplayMode Then

            Me.m_curDisplayMode = newDisplayMode

            'build a new IDisplayModeHelper object
            Select Case newDisplayMode
                Case eDisplayModeTypes.NoResults
                    helper = New cNoResultsDisplayHelper(Me.UIContext)
                Case eDisplayModeTypes.Ecosim
                    helper = New cEcoSimDisplayHelper(Me.UIContext, Me.m_zgh)
                Case eDisplayModeTypes.Ecospace
                    helper = New cEcoSpaceDisplayHelper(Me.UIContext, Me.m_zgh)
                Case Else
                    'something went wrong
                    Debug.Assert(False, "DisplayHelperFactory() Invalid DisplayMode")
                    helper = New cNoResultsDisplayHelper(Me.UIContext)
            End Select
        Else
            Debug.Assert(m_DisplayHelper IsNot Nothing, Me.ToString & ".DisplayHelperFactory() Current display mode has not been set! Something is wrong!")
            helper = Me.m_DisplayHelper
        End If

        Return helper

    End Function

#End Region ' Helper methods

#End Region ' Internal bits

#Region " Overrides "

#End Region ' Overrides

#Region " Display helpers "

#Region " Interface definition "

    ''' =======================================================================
    ''' <summary>
    ''' Interface for an Ecotracer display mode helper implementation.
    ''' </summary>
    ''' =======================================================================
    Private Interface IDisplayModeHelper
        Inherits IUIElement

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the line(s) to draw on the graph for a single group.
        ''' </summary>
        ''' <param name="iGroup">Index of the group to get line(s) for.</param>
        ''' <remarks>For Ecospace results, lines may be returned for every 
        ''' relevant region.</remarks>
        ''' -------------------------------------------------------------------
        Function GetGroupLines(ByVal iGroup As Integer, ByVal nTimeStepToRetrieve As Integer) As LineItem()

        Function GetGroupMax(ByVal iGroup As Integer) As Single

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a refecence to the EwE Core.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property Core() As cCore

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a refecence to the EwE StyleGuide.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property StyleGuide() As cStyleGuide

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the title for the data plot.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property Title() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the X axis label for the data plot.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property XAxisLabel() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the Y axis label for the data plot.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property YAxisLabel() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the underlying model has Ecotracer data to plot.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property CanPlot() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether regions are supported.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property SupportsRegions() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the first years that the underlying model startd runnin at.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property FirstYear As Integer

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of years that the underlying model has run for.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property nYears() As Integer

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="ePlotTypes">type of data to plot</see>. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property PlotType() As ePlotTypes

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the index of the region to display. Possible values should be
        ''' interpreted as follows:
        ''' <list type="bullet">
        ''' <item><term>0</term><description>The 0 region, e.g., all cells not 
        ''' assigned to a specific region</description></item>
        ''' <item><term>1 to n</term><description>Region n</description></item>
        ''' <item><term>n + 1</term><description>All regions [0, n] at once</description></item>
        ''' </list>
        ''' </summary>
        ''' -------------------------------------------------------------------
        Property RegionIndex() As Integer

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of time steps per year that the underlying model has
        ''' ran for.
        ''' </summary>
        ''' -------------------------------------------------------------------
        ReadOnly Property nStepPerYear() As Integer

    End Interface

#End Region ' Interface definition

#Region " No Results "

    Private Class cNoResultsDisplayHelper
        Implements IDisplayModeHelper

        Sub New(ByVal uic As cUIContext)
            ' Sanity check
            UIContext = uic
            Debug.Assert(uic IsNot Nothing)
        End Sub

        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext

        Public ReadOnly Property Core() As cCore _
            Implements IDisplayModeHelper.Core
            Get
                Return UIContext.Core
            End Get
        End Property

        Public ReadOnly Property StyleGuide() As cStyleGuide _
            Implements IDisplayModeHelper.StyleGuide
            Get
                Return Nothing
            End Get
        End Property

        Public Function GetGroupLines(ByVal iGroup As Integer, ByVal nTimeStepToRetrieve As Integer) As LineItem() _
            Implements IDisplayModeHelper.GetGroupLines
            Debug.Assert(False, Me.ToString & ".GetGroupLine() Warning this should not be called!")
            Return New LineItem() {}
        End Function

        Public Function GetGroupMax(ByVal iGroup As Integer) As Single _
            Implements IDisplayModeHelper.GetGroupMax
            Return 0.0
        End Function

        Public ReadOnly Property Title() As String _
            Implements IDisplayModeHelper.Title
            Get
                Return SharedResources.GENERIC_VALUE_NO_DATA
            End Get
        End Property

        Public ReadOnly Property FirstYear As Integer _
            Implements IDisplayModeHelper.FirstYear
            Get
                Return Me.Core.EcosimFirstYear
            End Get
        End Property

        Public ReadOnly Property nYears() As Integer _
            Implements IDisplayModeHelper.nYears
            Get
                Return 1
            End Get
        End Property

        Public Property PlotType() As ePlotTypes _
            Implements IDisplayModeHelper.PlotType

        Public Property RegionIndex() As Integer _
            Implements IDisplayModeHelper.RegionIndex

        Public ReadOnly Property SupportsRegions() As Boolean _
            Implements IDisplayModeHelper.SupportsRegions
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property XAxisLabel() As String _
            Implements IDisplayModeHelper.XAxisLabel
            Get
                Return ""
            End Get
        End Property

        Public ReadOnly Property YAxisLabel() As String _
            Implements IDisplayModeHelper.YAxisLabel
            Get
                Return ""
            End Get
        End Property

        Public ReadOnly Property CanPlot() As Boolean _
            Implements IDisplayModeHelper.CanPlot
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property nStepPerYear() As Integer Implements IDisplayModeHelper.nStepPerYear
            Get
                Return 0
            End Get
        End Property

    End Class

#End Region ' No Results

#Region " Ecosim "

    Private Class cEcoSimDisplayHelper
        Implements IDisplayModeHelper

        Private m_zgh As cZedGraphHelper

        Sub New(ByRef uic As cUIContext, ByVal ZedGraphHelper As cZedGraphHelper)
            ' Sanity check
            Debug.Assert(uic IsNot Nothing)
            Me.UIContext = uic
            Me.m_zgh = ZedGraphHelper
        End Sub

        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext

        ReadOnly Property Core() As cCore _
            Implements IDisplayModeHelper.Core
            Get
                Return Me.UIContext.Core
            End Get
        End Property

        Public ReadOnly Property StyleGuide() As cStyleGuide _
            Implements IDisplayModeHelper.StyleGuide
            Get
                Return Me.UIContext.StyleGuide
            End Get
        End Property

        Private Function BuildLine(ByVal iGroup As Integer, ByVal iCurTimeStep As Integer) As LineItem

            If iGroup < 0 Then Return Nothing ' Safety first

            Dim td As cEcotracerGroupOutput = Me.Core.EcotracerGroupResults
            Dim SimBio As cEcosimGroupOutput
            Dim vList As New PointPairList()
            Dim strLabel As String = SharedResources.HEADER_ENVIRONMENT
            Dim clrLine As Color = Color.Black
            Dim yVal As Double
            Dim dPos As Double

            If iGroup > 0 Then
                Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
                strLabel = group.Name
                clrLine = Me.UIContext.StyleGuide.GroupColor(Me.Core, iGroup)
            End If

            'decide the plot type outside the loop 
            'so that there does not have to be an "If Me.m_plottype = ePlotTypes.CB And iGroup > 0 Then" inside the loop
            If Me.PlotType = ePlotTypes.CB And iGroup > 0 Then

                SimBio = Me.Core.EcoSimGroupOutputs(iGroup)

                For iTimeStep As Integer = 1 To iCurTimeStep 'Me.Core.nEcosimTimeSteps
                    dPos = Me.Core.EcosimFirstYear + (iTimeStep / cCore.N_MONTHS)
                    yVal = CDbl(td.ConBio(iGroup, iTimeStep))
                    vList.Add(dPos, yVal)
                Next iTimeStep

            Else

                For iTimeStep As Integer = 1 To iCurTimeStep 'Me.Core.nEcosimTimeSteps
                    dPos = Me.Core.EcosimFirstYear + (iTimeStep / cCore.N_MONTHS)
                    yVal = CDbl(td.Concentration(iGroup, iTimeStep))
                    vList.Add(dPos, yVal)
                Next iTimeStep

            End If

            Return Me.m_zgh.CreateLineItem(strLabel, eSketchDrawModeTypes.Line, clrLine, vList)

        End Function

        Public Function GetGroupLines(ByVal iGroup As Integer, ByVal nTimeStepToRetrieve As Integer) As LineItem() _
            Implements IDisplayModeHelper.GetGroupLines

            Return New LineItem() {BuildLine(iGroup, nTimeStepToRetrieve)}

        End Function

        Public Function GetGroupMax(ByVal iGroup As Integer) As Single _
            Implements IDisplayModeHelper.GetGroupMax

            Dim smax As Single
            Try
                'there is no biomass for the environment index so there is no way to compute C/B
                'in that case use Concentration(group,time)
                If Me.PlotType = ePlotTypes.CB And iGroup > 0 Then

                    Dim grpbio As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)
                    For iTimeStep As Integer = 1 To Me.Core.nEcosimTimeSteps
                        smax = Math.Max(Me.Core.EcotracerGroupResults.ConBio(iGroup, iTimeStep), smax)
                    Next

                Else

                    For iTimeStep As Integer = 1 To Me.Core.nEcosimTimeSteps
                        smax = Math.Max(Me.Core.EcotracerGroupResults.Concentration(iGroup, iTimeStep), smax)
                    Next

                End If

                Return smax

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.StackTrace)
                Return smax
            End Try


        End Function

        Public ReadOnly Property Title() As String Implements IDisplayModeHelper.Title
            Get
                Return My.Resources.GENERIC_ECOSIM
            End Get
        End Property

        Public ReadOnly Property FirstYear As Integer _
            Implements IDisplayModeHelper.FirstYear
            Get
                Return Me.Core.EcosimFirstYear
            End Get
        End Property

        Public ReadOnly Property nYears() As Integer _
            Implements IDisplayModeHelper.nYears
            Get
                Return Me.Core.nEcosimYears
            End Get
        End Property

        Public Property PlotType() As ePlotTypes _
            Implements IDisplayModeHelper.PlotType

        Public Property RegionIndex() As Integer _
            Implements IDisplayModeHelper.RegionIndex

        Public ReadOnly Property SupportsRegions() As Boolean _
            Implements IDisplayModeHelper.SupportsRegions
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property XAxisLabel() As String _
            Implements IDisplayModeHelper.XAxisLabel
            Get
                Return SharedResources.HEADER_ECOSIM_YEARS
            End Get
        End Property

        Public ReadOnly Property YAxisLabel() As String _
            Implements IDisplayModeHelper.YAxisLabel
            Get
                Select Case Me.PlotType
                    Case ePlotTypes.CB
                        Return SharedResources.HEADER_CONCENTRATION_OVER_B
                    Case ePlotTypes.Conc
                        Return SharedResources.HEADER_CONCENTRATION
                    Case Else
                        Debug.Assert(False, "Plot type not handled")
                End Select
                Return "?"
            End Get
        End Property

        Public ReadOnly Property CanPlot() As Boolean _
            Implements IDisplayModeHelper.CanPlot
            Get
                Return Me.Core.StateMonitor.IsEcosimRunning Or Me.Core.StateMonitor.HasEcotracerRanForEcosim
                '  Return Me.Core.StateMonitor.HasEcotracerRanForEcosim
            End Get
        End Property

        Public ReadOnly Property nStepPerYear() As Integer _
            Implements IDisplayModeHelper.nStepPerYear
            Get
                Return cCore.N_MONTHS
            End Get
        End Property
    End Class

#End Region ' Ecosim

#Region " EcoSpace "

    Private Class cEcoSpaceDisplayHelper
        Implements IDisplayModeHelper

        Private m_zgh As cZedGraphHelper

        Sub New(ByVal uic As cUIContext, ByVal ZedGraphHelper As cZedGraphHelper)
            ' Sanity check
            Debug.Assert(uic IsNot Nothing)
            Me.UIContext = uic
            Me.m_zgh = ZedGraphHelper
        End Sub

        Private Property UIContext() As cUIContext _
            Implements IUIElement.UIContext

        Private ReadOnly Property Core() As cCore _
            Implements IDisplayModeHelper.Core
            Get
                Return Me.UIContext.Core
            End Get
        End Property

        Public ReadOnly Property StyleGuide() As cStyleGuide _
            Implements IDisplayModeHelper.StyleGuide
            Get
                Return Me.UIContext.StyleGuide
            End Get
        End Property

        Public Function GetGroupMax(ByVal iGroup As Integer) As Single Implements IDisplayModeHelper.GetGroupMax
            Dim smax As Single

            If Me.PlotType = ePlotTypes.Conc Then
                For ireg As Integer = 0 To Me.UIContext.Core.nRegions
                    For iTimeStep As Integer = 1 To Me.Core.nEcosimTimeSteps
                        smax = Math.Max(Me.Core.EcotracerRegionGroupResults.Concentration(ireg, iGroup, iTimeStep), smax)
                    Next iTimeStep
                Next ireg
            Else
                For ireg As Integer = 0 To Me.Core.nRegions
                    For iTimeStep As Integer = 1 To Me.Core.nEcosimTimeSteps
                        smax = Math.Max(Me.Core.EcotracerRegionGroupResults.CB(ireg, iGroup, iTimeStep), smax)
                    Next iTimeStep
                Next ireg
            End If

            Return smax
        End Function

        Public Function GetGroupLines(ByVal iGroup As Integer, ByVal nTimeStepToRetrieve As Integer) As LineItem() _
            Implements IDisplayModeHelper.GetGroupLines

            If iGroup < 0 Then Return Nothing

            Dim iRegStart As Integer = Math.Max(0, Me.RegionIndex)
            Dim iRegEnd As Integer = iRegStart
            Dim lstLines As New List(Of LineItem)

            If (Me.RegionIndex > Me.UIContext.Core.nRegions) Then
                iRegStart = 1
                iRegEnd = Me.Core.nRegions
            End If

            For iReg As Integer = iRegStart To iRegEnd
                lstLines.Add(BuildLine(iGroup, iReg, nTimeStepToRetrieve))
            Next

            Return lstLines.ToArray

        End Function

        Private Function BuildLine(ByVal iGroup As Integer, ByVal iRegion As Integer, ByVal iCurTimeStep As Integer) As LineItem

            Dim td As cEcotracerRegionGroupOutput = Me.Core.EcotracerRegionGroupResults
            Dim list As New PointPairList()
            Dim clrLine As Color = Color.Black
            Dim strFilter As String = ""
            Dim strRegionName As String = ""
            Dim strLabel As String
            Dim dPos As Double
            Dim sY As Single
            Dim ntsYear As Single

            ntsYear = Me.Core.EcospaceModelParameters.NumberOfTimeStepsPerYear

            ' Build the line label
            If iGroup > 0 Then
                strFilter = Me.Core.EcoPathGroupInputs(iGroup).Name
                clrLine = Me.StyleGuide.GroupColor(Me.Core, iGroup)
            Else
                strFilter = SharedResources.HEADER_ENVIRONMENT
            End If

            Select Case iRegion
                Case 0
                    strRegionName = My.Resources.VALUE_REGION_UNDEFINED
                Case Else
                    strRegionName = cStringUtils.Localize(My.Resources.VALUE_REGION_N, iRegion)
            End Select
            strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, strFilter, strRegionName)

            ' Figure out which varname to display based on the selected group and the ePlotTypes enum
            Dim varName As eVarNameFlags = GetVarName(iGroup)

            For iTimeStep As Integer = 1 To iCurTimeStep 'Me.Core.nEcospaceTimeSteps
                dPos = Me.Core.EcosimFirstYear + (iTimeStep / ntsYear)
                sY = td.GetVariable(varName, iRegion, iGroup, iTimeStep)
                list.Add(dPos, CDbl(sY))
            Next iTimeStep

            Return Me.m_zgh.CreateLineItem(strLabel, eSketchDrawModeTypes.Line, clrLine, list)

        End Function

        ''' <summary>
        ''' Get the correct variable to display based on the selected Group and the ePlotTypes
        ''' </summary>
        ''' <param name="iGroup"></param>
        Private Function GetVarName(ByVal iGroup As Integer) As eVarNameFlags

            If iGroup = 0 Then
                'The zero group is the environment variable 
                If Me.PlotType = ePlotTypes.Conc Then
                    Return eVarNameFlags.CEnvironment
                Else
                    Return eVarNameFlags.CBEnvironment
                End If

            Else
                'normal groups
                If Me.PlotType = ePlotTypes.Conc Then
                    Return eVarNameFlags.Concentration
                Else
                    Return eVarNameFlags.ConcBio
                End If

            End If

        End Function

        Public ReadOnly Property Title() As String _
            Implements IDisplayModeHelper.Title
            Get
                Dim strRegionName As String = ""
                Select Case Me.RegionIndex
                    Case 0
                        strRegionName = My.Resources.VALUE_REGION_UNDEFINED
                    Case 1 To Me.UIContext.Core.nRegions
                        strRegionName = cStringUtils.Localize(My.Resources.VALUE_REGION_N, Me.RegionIndex)
                    Case Else
                        strRegionName = My.Resources.VALUE_REGION_ALL
                End Select
                Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, My.Resources.GENERIC_ECOSPACE, strRegionName)
            End Get
        End Property

        Public ReadOnly Property FirstYear As Integer _
            Implements IDisplayModeHelper.FirstYear
            Get
                Return Me.Core.EcosimFirstYear
            End Get
        End Property

        Public ReadOnly Property nYears() As Integer _
            Implements IDisplayModeHelper.nYears
            Get
                Return Me.Core.nEcospaceYears
            End Get
        End Property

        Public Property PlotType() As ePlotTypes _
            Implements IDisplayModeHelper.PlotType

        Public Property RegionIndex() As Integer _
            Implements IDisplayModeHelper.RegionIndex

        Public ReadOnly Property SupportsRegions() As Boolean _
            Implements IDisplayModeHelper.SupportsRegions
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property XAxisLabel() As String Implements IDisplayModeHelper.XAxisLabel
            Get
                Return SharedResources.HEADER_ECOSPACE_YEARS
            End Get
        End Property

        Public ReadOnly Property YAxisLabel() As String Implements IDisplayModeHelper.YAxisLabel
            Get
                Select Case Me.PlotType
                    Case ePlotTypes.CB
                        Return SharedResources.HEADER_CONCENTRATION_OVER_B
                    Case ePlotTypes.Conc
                        Return SharedResources.HEADER_CONCENTRATION
                    Case Else
                        Debug.Assert(False, "Plot type not supported")
                End Select
                Return "?"
            End Get
        End Property

        Public ReadOnly Property CanPlot() As Boolean _
            Implements IDisplayModeHelper.CanPlot
            Get
                'JB ECOTRACER_HACK This may think it's OK to plot if even if Ecotracer has not been activated
                'I didn't check that. 
                '  Return Me.Core.StateMonitor.HasEcospaceInitialized
                Return Me.Core.StateMonitor.IsEcospaceRunning Or Me.Core.StateMonitor.HasEcotracerRanForEcospace
            End Get
        End Property

        Public ReadOnly Property nStepPerYear() As Integer Implements IDisplayModeHelper.nStepPerYear
            Get
                Try
                    Return Me.Core.nEcospaceTimeSteps \ Me.Core.nEcospaceYears
                Catch ex As Exception
                    Return 0
                End Try
            End Get
        End Property
    End Class

#End Region ' Ecospace

#End Region ' Display helpers

End Class