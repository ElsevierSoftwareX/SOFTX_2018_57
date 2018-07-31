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
Imports System.Drawing
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Definitions
Imports ZedGraph
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwECore.Style
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Style

#End Region ' Imports

' ToDo: show transect cell coordinates in value tooltip. This requires overriding zedgraph 

Public Class frmTransectSummary

    Private WithEvents m_data As cTransectDatastructures = Nothing
    Private m_zgh As cZedGraphHelper = Nothing
    Private m_tick As Integer = 1

    Public Sub New(uic As cUIContext)
        Me.InitializeComponent()
        Me.UIContext = uic
        Me.m_data = cTransectDatastructures.Instance(uic.Core)
        Me.Text = My.Resources.CAPTION_OUT
        Me.TabText = Me.Text
    End Sub

#Region " Overrides "

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        Dim cmdh As cCommandHandler = Me.CommandHandler
        Dim cmd As cCommand = Nothing

        ' Make pretty
        Me.m_tsbnPlay.Image = SharedResources.PlayHS
        Me.m_tsbnStop.Image = SharedResources.StopHS
        Me.m_tsbnSaveToCSV.Image = SharedResources.saveHS
        Me.m_tsbnAutosave.Image = SharedResources.saveOutputHS

        Me.m_zgh = New cZedGraphHelper()
        Me.m_zgh.Attach(Me.UIContext, Me.m_graph, 4)
        Me.m_zgh.ConfigurePane(My.Resources.CAPTION_DEPTH, My.Resources.LABEL_CELL, cUnits.Depth, False, iPane:=1)
        Me.m_zgh.ConfigurePane(My.Resources.CAPTION_MPA, My.Resources.LABEL_CELL, My.Resources.UNIT_MPACOUNT, False, iPane:=2)
        Me.m_zgh.ConfigurePane(My.Resources.CAPTION_BIOMASS, My.Resources.LABEL_CELL, cUnits.Currency, False, iPane:=3)
        Me.m_zgh.ConfigurePane(My.Resources.CAPTION_CATCH, My.Resources.LABEL_CELL, cUnits.Currency, False, iPane:=4)

        For i As Integer = 1 To 4
            AddHandler Me.m_zgh.GetPane(i).XAxis.ScaleFormatEvent, AddressOf OnFormatXScale
        Next

        ' Display Groups
        cmd = cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
        If cmd IsNot Nothing Then
            cmd.AddControl(Me.m_tsbtnShowHideGroups)
        End If

        AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged
        Me.FillTransectBox()
        Me.UpdateGraph()
        Me.UpdateControls()

        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.Core}

    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)

        Me.m_data = Nothing
        Me.m_timerPlay.Enabled = False
        Me.m_zgh.Detach()

        ' Show/Hide Groups
        Dim cmdh As cCommandHandler = Me.CommandHandler
        Dim cmd As cCommand = cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
        If cmd IsNot Nothing Then
            cmd.RemoveControl(Me.m_tsbtnShowHideGroups)
        End If

        Me.CoreComponents = Nothing
        MyBase.OnFormClosed(e)

    End Sub

    Private m_bNeedUpdateControls As Boolean = False

    Public Overrides Sub OnCoreMessage(msg As cMessage)
        If (msg.Type = eMessageType.GlobalSettingsChanged) Then
            If Me.m_bNeedUpdateControls = False Then
                Me.m_bNeedUpdateControls = True
                BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
            End If
        End If
        MyBase.OnCoreMessage(msg)
    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Me.m_bNeedUpdateControls = False

        If (Me.UIContext Is Nothing) Then Return

        Dim sm As cCoreStateMonitor = Me.Core.StateMonitor
        Dim bHasResults As Boolean = Not sm.IsBusy And sm.HasEcospaceRan
        Dim bIsPlaying As Boolean = Me.m_timerPlay.Enabled

        Me.m_tsbnPlay.Enabled = bHasResults And Not bIsPlaying
        Me.m_tsbnStop.Enabled = bIsPlaying
        Me.m_tsbnSaveToCSV.Enabled = bHasResults

        Dim w As cTransectResultWriterPlugin = Me.GetWriter()
        If (w IsNot Nothing) Then
            Me.m_tsbnAutosave.Checked = w.Enabled
            Me.m_tsbnAutosave.Enabled = True
        Else
            Me.m_tsbnAutosave.Enabled = False
        End If

    End Sub

    Protected Overrides Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
        If (ct And cStyleGuide.eChangeType.GroupVisibility) > 0 Then
            Me.UpdateGraph(True)
        End If
    End Sub

#End Region ' Overrides 

#Region " Control events "

    Private Sub OnSelectTransect(sender As Object, e As EventArgs) _
        Handles m_tscmbTransect.SelectedIndexChanged
        Try
            Me.UpdateGraph()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnTransectAdded(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectAdded
        Try
            If Me.IsDisposed Then Return
            Me.m_tscmbTransect.Items.Add(transect)
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnTransectRemoved(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectRemoved
        Try
            If Me.IsDisposed Then Return
            Me.m_tscmbTransect.Items.Remove(transect)
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnTransectChanged(sender As cTransectDatastructures, transect As cTransect) _
        Handles m_data.OnTransectChanged
        Try
            If Me.IsDisposed Then Return
            Me.UpdateGraph()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Function OnFormatXScale(pane As GraphPane, axis As Axis, val As Double, index As Integer) As String
        Dim t As cTransect = DirectCast(Me.m_tscmbTransect.SelectedItem, cTransect)
        If (t IsNot Nothing) Then
            Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
            Dim cells As Point() = t.Cells(bm)
            If (cells.Count > 0) And (val < t.NumCells) Then
                Dim pt As Point = cells(CInt(val))
                Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_POINT, pt.X, pt.Y)
            End If
        End If
        Return ""
    End Function

    Private Sub OnCoreExecutionStateChanged(statemonitor As cCoreStateMonitor)
        Try
            Me.m_timerPlay.Enabled = False
            Me.UpdateControls()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnPlay(sender As Object, e As EventArgs) Handles m_tsbnPlay.Click
        Try
            Me.m_timerPlay.Enabled = True
            Me.UpdateControls()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnStop(sender As Object, e As EventArgs) Handles m_tsbnStop.Click
        Try
            Me.m_timerPlay.Enabled = False
            Me.UpdateControls()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnTick(sender As Object, e As EventArgs) Handles m_timerPlay.Tick
        Try
            Me.m_tick += 1
            If (Me.m_tick > Core.nEcospaceTimeSteps) Then Me.m_tick = 1
            Me.UpdateGraph()
            Me.UpdateControls()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnSaveTransectsToCSV(sender As Object, e As EventArgs) _
        Handles m_tsbnSaveToCSV.Click

        Dim w As cTransectResultWriterPlugin = Me.GetWriter()
        If (w IsNot Nothing) Then
            ' We happen to know how this thing works
            w.EndWrite()
        End If

    End Sub

    Private Sub OnToggleAutosave(sender As Object, e As EventArgs) _
        Handles m_tsbnAutosave.CheckedChanged

        Dim w As cTransectResultWriterPlugin = Me.GetWriter()
        If (w IsNot Nothing) Then
            w.Enabled = m_tsbnAutosave.Checked
        End If

        Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters
        Dim bAutosaving As Boolean = False
        For i As Integer = 1 To Me.Core.nEcospaceResultWriters
            Dim test As IEcospaceResultsWriter = parms.ResultWriter(i)
            bAutosaving = bAutosaving Or test.Enabled
        Next
        Me.Core.Autosave(eAutosaveTypes.EcospaceResults) = bAutosaving

    End Sub

#End Region ' Control events

#Region " Internals "

    Private Function GetWriter() As cTransectResultWriterPlugin
        Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters
        For i As Integer = 1 To Me.Core.nEcospaceResultWriters
            Dim test As IEcospaceResultsWriter = parms.ResultWriter(i)
            If (TypeOf (test) Is cTransectResultWriterPlugin) Then
                Return DirectCast(test, cTransectResultWriterPlugin)
            End If
        Next
        Return Nothing
    End Function

    Private Sub FillTransectBox()
        Me.m_tscmbTransect.Items.Clear()
        For Each t As cTransect In Me.m_data.Transects
            Me.m_tscmbTransect.Items.Add(t)
        Next
    End Sub

    Private Sub UpdateGraph(Optional bOutOnly As Boolean = False)

        Dim t As cTransect = DirectCast(Me.m_tscmbTransect.SelectedItem, cTransect)

        If (Not bOutOnly) Then
            Me.FillInputPane(1, t, eVarNameFlags.LayerDepth)
            Me.FillInputPane(2, t, eVarNameFlags.LayerMPA)
        End If

        Me.FillOutputPane(3, t, cTransect.eSummaryType.Biomass)
        Me.FillOutputPane(4, t, cTransect.eSummaryType.Catch)

        Me.m_zgh.RescaleAndRedraw()

    End Sub

    Private Sub FillInputPane(iPane As Integer, t As cTransect, var As eVarNameFlags)

        Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
        Dim gp As GraphPane = Me.m_zgh.GetPane(iPane)
        Dim strName As String = ""

        Select Case var
            Case eVarNameFlags.LayerDepth : strName = Me.Core.EcospaceBasemap.LayerDepth.Name
            Case eVarNameFlags.LayerMPA : strName = My.Resources.UNIT_MPACOUNT
        End Select

        gp.CurveList.Clear()

        If (t IsNot Nothing) Then

            Dim cells As Point() = t.Cells(Me.Core.EcospaceBasemap)
            Dim values(cells.Count) As Single

            Try
                For Each l As cEcospaceLayer In bm.Layers(var)

                    Dim s As cTransectSummary = t.Summary(bm, l, -1)
                    For i As Integer = 0 To t.NumCells - 1

                        Dim pt As Point = cells(i)
                        Dim sVal As Single = s.Value(i)

                        If ((bm.IsModelledCell(pt.Y, pt.X)) Or (var = eVarNameFlags.LayerDepth)) And (sVal >= 0) Then
                            values(i) += sVal
                        End If
                    Next
                Next

                Dim ppl As New PointPairList()
                Dim bIsMissing As Boolean = False

                For i As Integer = 0 To t.NumCells - 1

                    Dim pt As Point = cells(i)
                    Dim sVal As Single = values(i)

                    If ((bm.IsModelledCell(pt.Y, pt.X)) Or (var = eVarNameFlags.LayerDepth)) And (sVal >= 0) Then
                        If bIsMissing Then
                            ppl.Add(i, 0)
                            bIsMissing = False
                        End If
                        values(i) += sVal
                        ppl.Add(i, sVal)
                        ppl.Add(i + 1, sVal)
                    Else
                        If Not bIsMissing Then
                            ppl.Add(i, 0)
                            bIsMissing = True
                        End If
                        ppl.Add(i, PointPair.Missing)
                        ppl.Add(i + 1, PointPair.Missing)
                    End If

                Next
                gp.CurveList.Add(Me.m_zgh.CreateLineItem(strName, eSketchDrawModeTypes.Line, Color.Blue, ppl, t))

                With gp.XAxis.Scale
                    .Min = 0
                    .MinAuto = False
                    .MinGrace = 0
                    .Max = t.NumCells - 1
                    .MaxAuto = False
                    .MaxGrace = 0
                End With

            Catch ex As Exception
                ' NOP
            End Try
            Me.m_zgh.RescaleAndRedraw()
        End If

    End Sub

    Private Sub FillOutputPane(iPane As Integer, t As cTransect, var As cTransect.eSummaryType)

        Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
        Dim gp As GraphPane = Me.m_zgh.GetPane(iPane)
        Dim vnf As New cVarnameTypeFormatter()

        gp.CurveList.Clear()

        If (t IsNot Nothing) Then

            Dim cells As Point() = t.Cells(Me.Core.EcospaceBasemap)

            Try
                For iGroup As Integer = 1 To Me.Core.nGroups

                    If Me.StyleGuide.GroupVisible(iGroup) Then

                        Dim s As cTransectSummary = t.Summary(Me.m_tick, iGroup, var)
                        Dim ppl As New PointPairList()
                        Dim bIsMissing As Boolean = False

                        If (s IsNot Nothing) Then
                            For i As Integer = 0 To t.NumCells - 1

                                Dim pt As Point = cells(i)
                                Dim sVal As Single = s.Value(i)

                                If (bm.IsModelledCell(pt.Y, pt.X) And (sVal >= 0)) Then
                                    If bIsMissing Then
                                        ppl.Add(i, 0)
                                        bIsMissing = False
                                    End If
                                    ppl.Add(i, sVal)
                                    ppl.Add(i + 1, sVal)
                                Else
                                    If Not bIsMissing Then
                                        ppl.Add(i, 0)
                                        bIsMissing = True
                                    End If
                                    ppl.Add(i, PointPair.Missing)
                                    ppl.Add(i + 1, PointPair.Missing)
                                End If

                            Next
                        End If

                        gp.CurveList.Add(Me.m_zgh.CreateLineItem(Me.Core.EcoPathGroupInputs(iGroup), ppl, tag:=t))

                        Dim strData As String = If(var = cTransect.eSummaryType.Biomass, vnf.GetDescriptor(eVarNameFlags.Biomass), vnf.GetDescriptor(eVarNameFlags.TotalCatch))
                        Dim strTime As String = If(t.HasSummaries, cStringUtils.Localize(My.Resources.LABEL_TIMESTEP, Me.m_tick, Me.Core.nEcospaceTimeSteps), My.Resources.LABEL_NODATA)
                        gp.Title.Text = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DOUBLE, strData, strTime.ToLower())

                    End If
                Next

                With gp.XAxis.Scale
                    .Min = 0
                    .MinAuto = False
                    .MinGrace = 0
                    .Max = t.NumCells - 1
                    .MaxAuto = False
                    .MaxGrace = 0
                End With

            Catch ex As Exception
                ' NOP
            End Try
            Me.m_zgh.RescaleAndRedraw()

        End If

    End Sub

#End Region ' Internals

End Class