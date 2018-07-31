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
Imports EwECore.Ecospace.Advection
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers

#End Region ' Imports

Namespace Ecospace.Advection

    Public Class frmAdvection

#Region " Private vars "

        Private m_manager As cAdvectionManager = Nothing

        Private m_dlgtStarted As cAdvectionManager.ComputationStartedDelegate = Nothing
        Private m_dlgtProgress As cAdvectionManager.ComputationProgressDelegate = Nothing
        Private m_dlgtStopped As cAdvectionManager.ComputationCompletedDelegate = Nothing

        Private m_edtWind As cLayerEditorVelocity = Nothing

        ''' <summary>Flag stating whether this form started a search.</summary>
        Private m_bSearching As Boolean = False
        ''' <summary>Flag stating whether a search was completed from this form.</summary>
        Private m_bHasRun As Boolean = False

        Private m_fpUpwellingThreshold As cPropertyFormatProvider
        Private m_fpPPMult As cPropertyFormatProvider

#End Region ' Private vars

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

#Region " Form overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Design time bypasses
            If Me.UIContext Is Nothing Then Return
            If Me.DesignMode Then Return

            Me.m_manager = Me.Core.AdvectionManager

            ' Connect all layers to the zoom toolbar
            For Each uc As ucAdvectionMap In Me.Maps
                Me.m_ucZoomToolbar.AddZoomContainer(uc.ZoomCtrl)
            Next

            ' Initialize editors
            Me.m_edtWind = DirectCast(Me.m_ucWind.DataLayer.Editor, cLayerEditorVelocity)
            AddHandler Me.m_edtWind.OnFilterChanged, AddressOf OnMonthChanged

            Me.m_tlpControls.SuspendLayout()
            Dim ctrl As UserControl = Me.m_edtWind.CreateEditorControl()
            ctrl.TabIndex = Me.m_lblWIndEditorPlaceholder.TabIndex
            ctrl.Dock = DockStyle.Fill
            Dim iRow As Integer = Me.m_tlpControls.GetRow(Me.m_lblWIndEditorPlaceholder)
            Dim iCol As Integer = Me.m_tlpControls.GetColumn(Me.m_lblWIndEditorPlaceholder)
            Me.m_tlpControls.Controls.Remove(Me.m_lblWIndEditorPlaceholder)
            Me.m_tlpControls.Controls.Add(ctrl, iCol, iRow)
            Me.m_tlpControls.ResumeLayout()

            Me.m_dlgtStarted = New cAdvectionManager.ComputationStartedDelegate(AddressOf OnCalcStarted)
            Me.m_dlgtProgress = New cAdvectionManager.ComputationProgressDelegate(AddressOf OnCalcProgress)
            Me.m_dlgtStopped = New cAdvectionManager.ComputationCompletedDelegate(AddressOf OnCalcStopped)
            Me.m_manager.Connect(Me.m_dlgtStarted, Me.m_dlgtStopped, Me.m_dlgtProgress)

            ' Config EwEForm
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSpace}

            Me.m_fpUpwellingThreshold = New cPropertyFormatProvider(Me.UIContext, Me.m_txtUpwelling, Me.m_manager.ModelParameters, eVarNameFlags.AdvectionUpwellingThreshold)
            Me.m_fpPPMult = New cPropertyFormatProvider(Me.UIContext, Me.m_txtPPMult, Me.m_manager.ModelParameters, eVarNameFlags.AdvectionUpwellingPPMultiplier)

            Me.PopulateMapsCombo()
            Me.PopulateMonthsCombo()

            ' Add map control buttons
            If Me.m_ucZoomToolbar.Toolstrip.Merge(m_tsAdvection) Then
                Me.m_tsAdvection.Visible = False

                ' Do not dispose temporary toolstrip until EwEToolStrip.Merge fully clones menu strip items.
                ' For now, disposing the source toolstrip also deletes event handlers on its (former) toolstrip items. 
                'Me.m_tsAdvection.Dispose()
                'Me.m_tsAdvection = Nothing
            End If

            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            ' Stop any pending run, just in case
            Me.StopRun()

            ' Unplug editor
            RemoveHandler Me.m_edtWind.OnFilterChanged, AddressOf OnMonthChanged
            Me.m_edtWind = Nothing

            For Each uc As ucAdvectionMap In Me.Maps
                Me.m_ucZoomToolbar.RemoveZoomContainer(uc.ZoomCtrl)
            Next

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Form overrides

#Region " Public bits "

        Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
                MyBase.UIContext = value
                Me.m_ucZoomToolbar.UIContext = Me.UIContext
                For Each uc As ucAdvectionMap In Me.Maps
                    uc.UIContext = value
                Next
            End Set
        End Property

#End Region ' Public bits

#Region " Control events "

        Private Sub OnMonthChanged(sender As IContentFilter)

            If (TypeOf sender Is IMonthFilter) Then
                ' Sync advection and upwelling maps with Wind month selection
                Dim iMonth As Integer = DirectCast(sender, IMonthFilter).Month
                DirectCast(Me.m_ucAdvection.DataLayer.Editor, IMonthFilter).Month = iMonth
                DirectCast(Me.m_ucUpwelling.DataLayer.Editor, IMonthFilter).Month = iMonth
                Me.m_tscmbViewMonth.SelectedIndex = iMonth - 1
            End If

        End Sub

        Private Sub OnComputeVels(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnStart.Click
            Me.StartRun()
        End Sub

        Private Sub OnStopComputing(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnStop.Click
            Me.m_manager.StopRun()
        End Sub

        Private Sub OnSwitchViewMap(sender As Object, e As EventArgs) Handles m_tscmbViewMap.SelectedIndexChanged
            Me.SuspendLayout()
            Try
                Select Case Me.m_tscmbViewMap.SelectedIndex
                    Case 0 'all
                        Me.m_scMaps.Panel1Collapsed = False
                        Me.m_scMaps.Panel2Collapsed = False
                        Me.m_scOutputMaps.Panel1Collapsed = False
                        Me.m_scOutputMaps.Panel2Collapsed = False
                        Me.m_scOutputMaps.IsSplitterFixed = False
                    Case 1 'wind - top panel
                        Me.m_scOutputMaps.IsSplitterFixed = True
                        Me.m_scMaps.Panel1Collapsed = False
                        Me.m_scMaps.Panel2Collapsed = True
                    Case 2 'advection - bottom panel, left
                        Me.m_scOutputMaps.IsSplitterFixed = True
                        Me.m_scMaps.Panel1Collapsed = True
                        Me.m_scMaps.Panel2Collapsed = False
                        Me.m_scOutputMaps.Panel1Collapsed = False
                        Me.m_scOutputMaps.Panel2Collapsed = True
                    Case 3 'upwelling - bottom panel, right
                        Me.m_scOutputMaps.IsSplitterFixed = True
                        Me.m_scMaps.Panel1Collapsed = True
                        Me.m_scMaps.Panel2Collapsed = False
                        Me.m_scOutputMaps.Panel1Collapsed = True
                        Me.m_scOutputMaps.Panel2Collapsed = False
                End Select
            Catch ex As Exception
                Debug.Assert(False)
            End Try
            Me.ResumeLayout()

        End Sub

        Private Sub OnSwitchViewMonth(sender As Object, e As EventArgs) Handles m_tscmbViewMonth.SelectedIndexChanged
            Try
                Me.m_edtWind.Month = Me.m_tscmbViewMonth.SelectedIndex + 1
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Control events

#Region " Event handlers "

        'Private Sub OnVelocityChanged(ByVal sender As Object, args As EventArgs)
        '    Me.UpdateTransportVelocity()
        'End Sub

        Private Sub OnCalcStarted()
            Me.m_bHasRun = False
            Me.UpdateControls()
        End Sub

        Private Sub OnCalcProgress(ByVal iIter As Integer)

            'In the new mdoel
            'iIter will be the month that was just calculated
            'Could update the output map with this month???
            'May not be that important

            ' Update data layer
            Dim layer As cDisplayLayerRaster = Me.m_ucAdvection.DataLayer
            layer.IsModified = True
            layer.Update(cDisplayLayer.eChangeFlags.Map, False)

        End Sub

        Private Sub OnCalcStopped(ByVal iIter As Integer, ByVal bInterrupted As Boolean, ByVal bBadFlow As Boolean)
            Me.StopRun()
            Me.m_ucAdvection.Invalidate()

            'jb left the original message in place incase after testing this is a better way to do it
            'If bBadFlow Then
            '    Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_ADVECTION_INBALANCED,
            '                                     eCoreComponentType.EcoSpace, eMessageType.Any,
            '                                     eMessageImportance.Warning, eMessageReplyStyle.YES_NO, eDataTypes.NotSet, eMessageReply.YES)
            '    fmsg.Suppressable = True
            '    Me.Core.Messages.SendMessage(fmsg)
            '    If fmsg.Reply = eMessageReply.YES Then
            '        Me.Revert()
            '    End If
            'End If

            If bBadFlow Or bInterrupted Then
                Dim msg As New cMessage(My.Resources.ADVECTION_FAILED, eMessageType.ErrorEncountered,
                                        eCoreComponentType.EcoSpace, eMessageImportance.Warning)
                Me.Core.Messages.SendMessage(msg)
            End If

            Me.m_bHasRun = True
            Me.UpdateControls()

        End Sub

#End Region ' Event handlers

#Region " Internals "

        Private Sub PopulateMapsCombo()
            Me.m_tscmbViewMap.Items.Add(ScientificInterfaceShared.My.Resources.GENERIC_VALUE_ALL)
            Me.m_tscmbViewMap.Items.Add(Me.m_ucWind.DataLayer.Name)
            Me.m_tscmbViewMap.Items.Add(Me.m_ucAdvection.DataLayer.Name)
            Me.m_tscmbViewMap.Items.Add(Me.m_ucUpwelling.DataLayer.Name)
            Me.m_tscmbViewMap.SelectedIndex = 0
        End Sub

        Private Sub PopulateMonthsCombo()
            For i As Integer = 1 To 12
                Me.m_tscmbViewMonth.Items.Add(cDateUtils.GetMonthName(i))
            Next
            Me.m_tscmbViewMonth.SelectedIndex = 0
        End Sub

        Protected Overrides Sub UpdateControls()

            ' Gather stats
            Dim bBusy As Boolean = Me.m_manager.IsRunning

            Me.m_btnStart.Enabled = Not bBusy And Not Me.m_bSearching
            Me.m_btnStop.Enabled = Me.m_bSearching

        End Sub

        Private Function Maps() As ucAdvectionMap()
            Return New ucAdvectionMap() {Me.m_ucAdvection, Me.m_ucUpwelling, Me.m_ucWind}
        End Function

        Private Sub StartRun()

            ' Already running? Abort
            If Me.m_bSearching Then Return

            If Not Me.m_manager.IsRunning Then Me.m_manager.RunPhysicsModel(Me)
            Me.m_bSearching = Me.m_manager.IsRunning

            If m_bSearching Then
                Me.UpdateControls()
            End If

        End Sub

        Private Sub StopRun()

            If Not Me.m_bSearching Then Return

            Me.m_bSearching = False
            Me.m_manager.StopRun()
            Me.UpdateControls()

        End Sub

#End Region ' Internals

    End Class

End Namespace
