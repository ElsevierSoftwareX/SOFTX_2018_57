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
Imports EwECore.SpatialData
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Helper class; maintains content of the status strip panes in the AppLauncher.
''' </summary>
''' -----------------------------------------------------------------------
Public Class cEwEStatusBar

    ''' <summary>The ui context to use.</summary>
    Private m_frmEwE6 As frmEwE6 = Nothing
    Private m_uic As cUIContext = Nothing
    Private m_csm As cCoreStateMonitor = Nothing
    Private m_selmon As cSelectionMonitor = Nothing
    Private m_mhSpatConfig As cMessageHandler = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of this class.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()
        ' Load
        Me.InitializeComponent()
        ' At runtime set visible state of controls
        If (Not Me.DesignMode) Then
            ' Hide all items at startup
            For Each item As ToolStripItem In Me.Items
                item.Visible = False
            Next
            ' .. except for springy status label, which needs to push the model and scenario controls to the right
            Me.m_tsStatus.Visible = True
            ' Configure host IP
            Try
                Me.m_tsIP.Text = cSystemUtils.GetHostIP()
                Me.m_tsIP.ToolTipText = cSystemUtils.GetHostName()
            Catch ex As Exception
                '  Hmm
            End Try
            Me.m_selmon = New cSelectionMonitor()
        End If
    End Sub

    Public Sub Attach(ByVal uic As cUIContext, frm As frmEwE6)

        ' Sanity checks
        Debug.Assert(Me.m_uic Is Nothing)

        Me.m_frmEwE6 = frm
        Me.m_uic = uic
        Me.m_csm = Me.m_uic.Core.StateMonitor
        Me.m_selmon.Attach(uic)

        AddHandler Me.m_csm.CoreDataStateEvent, AddressOf OnCoreDataStateEvent
        AddHandler Me.m_csm.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateEvent
        AddHandler Me.m_selmon.OnSelectionChanged, AddressOf OnSelectionChanged

        Me.m_mhSpatConfig = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoSpace, eMessageType.DataModified, Me.m_uic.SyncObject)
        Me.m_uic.Core.Messages.AddMessageHandler(Me.m_mhSpatConfig)
#If DEBUG Then
        Me.m_mhSpatConfig.Name = "cEwEStatusBar:Ecospace"
#End If

    End Sub

    Public Sub Detach()

        ' Sanity checks
        Debug.Assert(Me.m_uic IsNot Nothing)

        Me.m_uic.Core.Messages.RemoveMessageHandler(Me.m_mhSpatConfig)
        Me.m_mhSpatConfig.Dispose()
        Me.m_mhSpatConfig = Nothing

        RemoveHandler Me.m_csm.CoreDataStateEvent, AddressOf OnCoreDataStateEvent
        RemoveHandler Me.m_csm.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateEvent
        RemoveHandler Me.m_selmon.OnSelectionChanged, AddressOf OnSelectionChanged

        Me.m_selmon.Detach()
        Me.m_csm = Nothing
        Me.m_uic = Nothing

    End Sub

#Region " Events "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Core state monitor data changed event handler; handled to update the
    ''' content of the status panes.
    ''' </summary>
    ''' <remarks>
    ''' Refer to <see cref="cCoreStateMonitor.CoreDataStateEvent">data change event</see>
    ''' for a detailed description of this event.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub OnCoreDataStateEvent(ByVal csm As EwECore.cCoreStateMonitor)
        Me.BeginInvoke(New MethodInvoker(AddressOf UpdateModelPanes))
        'Me.UpdateModelPanes()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Core state monitor execution state changed event handler; handled to 
    ''' update the content of the status panes.
    ''' </summary>
    ''' <remarks>
    ''' Refer to <see cref="cCoreStateMonitor.CoreDataStateEvent">data change event</see>
    ''' for a detailed description of this event.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub OnCoreExecutionStateEvent(ByVal csm As cCoreStateMonitor)
        Me.UpdateModelPanes()
    End Sub

    Private Sub OnSelectionChanged(mon As cSelectionMonitor)
        Me.SetStatusText(Me.m_strLastStatusText, Me.m_sLastProgress)
    End Sub

    Private Sub OnStopRun(sender As Object, e As System.EventArgs) Handles m_tslStop.Click
        Try
            Me.m_uic.Core.StopRun()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnCoreMessage(ByRef msg As cMessage)
        If msg.Type = eMessageType.DataModified Then
            Me.UpdateModelPanes()
        End If
    End Sub

#End Region ' Events

#Region " Pane content handling "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Bluntly updates the content of all status strip panes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub UpdateModelPanes()

        If (Me.m_uic Is Nothing) Then Return

        Dim core As cCore = Me.m_uic.Core
        If (core Is Nothing) Then Return

        Dim eweModel As cEwEModel = Me.m_uic.Core.EwEModel
        Dim simScenario As cEcoSimScenario = Nothing
        Dim tsds As cTimeSeriesDataset = Nothing
        Dim spaceScenario As cEcospaceScenario = Nothing
        Dim tracerScenario As cEcotracerScenario = Nothing
        Dim fmt As New cTimeSeriesDatasetIntervalTypeFormatter()
        Dim strName As String = ""
        Dim strTooltip As String = ""

        ' Is Ecopath model loaded?
        If Me.m_csm.IsExecutionStateSuperceded(eCoreExecutionState.EcopathLoaded) Then
            ' #Yes: set content for status panes

            ' ----------------------
            ' Datasource and Ecopath
            ' ----------------------
            strTooltip = cStringUtils.Localize(My.Resources.STATUSSTRIP_ECOPATH_TOOLTIP,
                                       eweModel.Name,
                                       m_frmEwE6.SelectedFileName)
            Me.UpdateToolstripItem(Me.m_tsEcopathModel, eweModel.Name, strTooltip)

            ' -------
            ' Ecosim
            ' -------
            If Me.m_uic.Core.ActiveEcosimScenarioIndex >= 0 Then
                simScenario = core.EcosimScenarios(core.ActiveEcosimScenarioIndex)

                If core.ActiveTimeSeriesDatasetIndex > 0 Then
                    tsds = core.TimeSeriesDataset(core.ActiveTimeSeriesDatasetIndex)
                    strName = tsds.Name
                    strTooltip = cStringUtils.Localize(My.Resources.STATUSSTRIP_ECOSIM_TOOLTIP,
                                               simScenario.Name,
                                               strName,
                                               Me.ToTooltipLabel(simScenario.Description))
                    strName = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, simScenario.Name, strName)
                Else
                    strTooltip = cStringUtils.Localize(My.Resources.STATUSSTRIP_ECOSIM_TOOLTIP,
                                               simScenario.Name,
                                               Me.ToTooltipLabel(""),
                                               Me.ToTooltipLabel(simScenario.Description))
                    strName = simScenario.Name
                End If
                Me.UpdateToolstripItem(Me.m_tsEcosimScenario, strName, strTooltip)
            Else
                Me.UpdateToolstripItem(Me.m_tsEcosimScenario)
            End If

            ' -------
            ' Ecospace
            ' -------
            If (core.ActiveEcospaceScenarioIndex >= 0) Then
                spaceScenario = core.EcospaceScenarios(core.ActiveEcospaceScenarioIndex)
                strTooltip = cStringUtils.Localize(My.Resources.STATUSSTRIP_ECOSPACE_TOOLTIP,
                                           spaceScenario.Name,
                                           Me.ToTooltipLabel(spaceScenario.Description))

                strName = spaceScenario.Name

                Dim n As Integer = core.SpatialDataConnectionManager.NumConnectedAdapters
                Dim parms As cEcospaceModelParameters = core.EcospaceModelParameters
                Dim strTimeSeries As String = ""

                ' -- prepare time series appendix --
                If (n > 0) Then
                    strTimeSeries = Me.AppendTimSeries(strTimeSeries, cStringUtils.Localize(My.Resources.STATUSSTRIP_ECOSPACE_CONNECTIONS, n))
                End If
                If core.EcospaceModelParameters.UseEcosimBiomassForcing Then
                    strTimeSeries = Me.AppendTimSeries(strTimeSeries, My.Resources.STATUSSTRIP_ECOSPACE_ECOSIMBIOMASSFORCING)
                End If
                If core.EcospaceModelParameters.UseEcosimDiscardForcing Then
                    strTimeSeries = Me.AppendTimSeries(strTimeSeries, My.Resources.STATUSSTRIP_ECOSPACE_ECOSIMDISCARDFORCING)
                End If
                If Not String.IsNullOrWhiteSpace(strTimeSeries) Then
                    strName = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, strName, strTimeSeries)
                End If
                Me.UpdateToolstripItem(Me.m_tsEcospaceScenario, strName, strTooltip)
            Else
                Me.UpdateToolstripItem(Me.m_tsEcospaceScenario)
            End If

            ' -------
            ' Ecotracer
            ' -------
            If (core.ActiveEcotracerScenarioIndex >= 0) Then
                tracerScenario = core.EcotracerScenarios(core.ActiveEcotracerScenarioIndex)
                strTooltip = cStringUtils.Localize(My.Resources.STATUSSTRIP_ECOTRACER_TOOLTIP,
                                           cStringUtils.vbNewline,
                                           tracerScenario.Name,
                                           Me.ToTooltipLabel(tracerScenario.Description))
                Me.UpdateToolstripItem(Me.m_tsEcotracerScenario, tracerScenario.Name, strTooltip)
            Else
                Me.UpdateToolstripItem(Me.m_tsEcotracerScenario)
            End If

        Else
            ' #No: clear all status panes
            Me.UpdateToolstripItem(Me.m_tsEcopathModel)
            Me.UpdateToolstripItem(Me.m_tsEcosimScenario)
            Me.UpdateToolstripItem(Me.m_tsEcospaceScenario)
            Me.UpdateToolstripItem(Me.m_tsEcotracerScenario)
        End If

        ' Show machine ID when wanted by the user AND a remote desktop session is active
        Me.m_tsIP.Visible = My.Settings.ShowHostInfo And cSystemUtils.IsRDC

    End Sub

    Private Function ToTooltipLabel(ByVal str As String) As String
        If String.IsNullOrEmpty(str) Then Return SharedResources.GENERIC_VALUE_NONE
        Return str
    End Function

    Private Function AppendTimSeries(strLabel As String, strSeries As String) As String
        If (String.IsNullOrWhiteSpace(strSeries)) Then Return strLabel
        If (String.IsNullOrWhiteSpace(strLabel)) Then Return strSeries
        If cSystemUtils.IsRightToLeft() Then
            Return cStringUtils.Localize("{1} ,{0}", strLabel, strSeries)
        Else
            Return cStringUtils.Localize("{0}, {1}", strLabel, strSeries)
        End If
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update the content of a single tool strip item.
    ''' </summary>
    ''' <param name="tsi">The item to update.</param>
    ''' <param name="strText">Text to assign to the item. If no text is provided the
    ''' item will not be displayed.</param>
    ''' <param name="strTooltipText">Tooltip text to assign to the item. If this value
    ''' is an empty string no tooltip will appear.</param>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateToolstripItem(ByVal tsi As ToolStripItem,
            Optional ByVal strText As String = "",
            Optional ByVal strTooltipText As String = "")

        ' Abort if something went wrong
        If tsi Is Nothing Then Return

        ' Configure the item that was found
        With tsi
            .Text = strText
            .ToolTipText = strTooltipText
            ' Hide item if item has no text
            .Visible = (Not String.IsNullOrWhiteSpace(strText))
            .Invalidate()
        End With
    End Sub

    Private m_strLastStatusText As String = ""
    Private m_sLastProgress As Single = 0

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Set the text of the main status strip item.
    ''' </summary>
    ''' <param name="strText">The text to set.</param>
    ''' <param name="sProgress">Progress ([0, 1] or -1 )to set in a continuous progress bar,
    ''' 0.0 to hide progress bar, or -1 to show a marquee progress bar.</param>
    ''' -------------------------------------------------------------------
    Public Sub SetStatusText(ByVal strText As String, Optional ByVal sProgress As Single = 0.0)

        ' Early bail-out
        If (Me.m_tsStatus Is Nothing) Then Return

        ' Truncate progress
        sProgress = CInt(sProgress * 50) / 50.0!

        ' Store original text
        Me.m_strLastStatusText = strText

        ' Obtain alternate text from selection monitor
        If String.IsNullOrEmpty(strText) Then
            strText = New cSelectionMonitorFormatter(Me.m_uic.Core).GetDescriptor(Me.m_selmon, eDescriptorTypes.Name)
        End If

        ' Optimization
        If (String.Compare(strText, Me.m_tsStatus.Text, True) = 0) And (sProgress = m_sLastProgress) Then
            Return
        End If

        ' Now store progress
        Me.m_sLastProgress = sProgress

        ' Update
        Me.m_tsStatus.Text = strText
        If sProgress = 0 Then
            Me.m_tsbProgress.Visible = False
            Me.m_tslStop.Visible = False
        ElseIf sProgress > 0 Then
            Me.m_tsbProgress.Style = ProgressBarStyle.Continuous
            Me.m_tsbProgress.Visible = True
            Me.m_tsbProgress.Value = CInt(Math.Ceiling(Math.Max(Math.Min(100, sProgress * 100), 0)))
            Me.m_tslStop.Visible = Me.m_uic.Core.CanStopRun
        Else
            Me.m_tsbProgress.Style = ProgressBarStyle.Marquee
            Me.m_tsbProgress.Visible = True
            Me.m_tslStop.Visible = Me.m_uic.Core.CanStopRun
        End If

        ' Redraw status bar immediately
        '   This is a known performace killer (issue #937)
        Me.Refresh()

    End Sub

#End Region ' Pane content handling

End Class
