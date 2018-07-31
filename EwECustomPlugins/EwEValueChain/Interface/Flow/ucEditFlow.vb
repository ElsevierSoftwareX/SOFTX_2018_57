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
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' 
''' </summary>
''' ===========================================================================
Public Class ucEditFlow

    Private m_uic As cUIContext = Nothing
    Private m_data As cData = Nothing
    Private m_diagram As cFlowDiagram = Nothing

    Public Sub New(ByVal uic As cUIContext, _
                   ByVal data As cData, _
                   ByVal diagram As cFlowDiagram)

        Me.InitializeComponent()

        Debug.Assert(data IsNot Nothing)
        Debug.Assert(diagram IsNot Nothing, "Cannot created diagram editor without a valid diagram")

        Me.m_selector.Visible = False

        Me.m_uic = uic
        Me.Data = data
        Me.Diagram = diagram

        Dim levels As Single() = Me.m_plFlow.ZoomLevels
        For i As Integer = 0 To levels.Length - 1
            Dim ctrl As New ToolStripMenuItem(cStringUtils.Localize(ScientificInterfaceShared.My.Resources.GENERIC_VALUE_PERCENTAGE, levels(i) * 100), Nothing, AddressOf OnZoom)
            ctrl.Tag = levels(i)
            Me.m_tsddZoom.DropDownItems.Add(ctrl)
        Next

        If (data.Parameters IsNot Nothing) Then
            Me.m_plFlow.ZoomFactor = data.Parameters.ZoomFactor
        End If
        Me.m_plFlow.ShowGrid = My.Settings.ShowGrid
        Me.UpdateControls()

        AddHandler Me.m_plFlow.EditModeChanged, AddressOf Me.OnEditModeChanged
        AddHandler Me.m_plFlow.ZoomChanged, AddressOf Me.OnZoomChanged
        AddHandler Me.m_plFlow.SelectionChanged, AddressOf Me.OnSelectionChanged

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            ' Store zoom factor
            If (Data.Parameters IsNot Nothing) Then
                Data.Parameters.ZoomFactor = Me.m_plFlow.ZoomFactor
            End If

            My.Settings.ShowGrid = Me.m_plFlow.ShowGrid
            Me.m_diagram = Nothing
            Me.m_data = Nothing
            Me.m_uic = Nothing

            ' Disconnect
            RemoveHandler Me.m_plFlow.EditModeChanged, AddressOf OnEditModeChanged
            RemoveHandler Me.m_plFlow.ZoomChanged, AddressOf Me.OnZoomChanged
            RemoveHandler Me.m_plFlow.SelectionChanged, AddressOf OnSelectionChanged

            ' Default cleanup
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#Region " Event handling "

#Region " Saving "

    Private Sub m_tsmiSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsmiSave.Click, m_tsbSave.ButtonClick

        Me.m_data.Save()

    End Sub

    Private Sub m_tsmiExportToImage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsmiExportToImage.Click

        Debug.Assert(False, "Image save functionality not yet implemented")

    End Sub

#End Region ' Saving

#Region " Diagram controls "

    'Private Sub OnDiagram(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    '    Handles m_tsddDiagram.Click
    '    ' ToDo: invoke add/remove diagram dialog
    '    Me.UpdateControls()
    'End Sub

    'Private Sub OnSelectDiagram(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '    Dim tsi As ToolStripItem = DirectCast(sender, ToolStripItem)
    '    Me.Diagram = DirectCast(tsi.Tag, cFlowDiagram)
    'End Sub

#End Region ' Diagram controls

#Region " Mode buttons "

    Private Sub tsbMove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbMove.Click
        Me.m_plFlow.EditMode = plFlow.eEditMode.Move
    End Sub

    Private Sub tsbLink_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbLink.Click
        Me.m_plFlow.EditMode = plFlow.eEditMode.Link
    End Sub

    Private Sub tsbDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbDelete.Click
        Me.m_plFlow.EditMode = plFlow.eEditMode.Delete
    End Sub

    Private Sub OnEditModeChanged(ByVal pl As plFlow, ByVal mode As plFlow.eEditMode)
        Me.UpdateControls()
    End Sub

#End Region ' Mode buttons

#Region " Creation buttons "

    Private Sub OnCreateProducersForFleet(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateProducersForFleets.Click
        Me.m_plFlow.CreateProducersForFleets()
        Me.UpdateControls()
    End Sub

    Private Sub OnCreateProducer(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateProducer.Click
        Me.m_plFlow.CreateUnit(cUnitFactory.eUnitType.Producer)
        Me.UpdateControls()
    End Sub

    Private Sub OnCreateProcessing(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateProcessing.Click
        Me.m_plFlow.CreateUnit(cUnitFactory.eUnitType.Processing)
        Me.UpdateControls()
    End Sub

    Private Sub OnCreateDistribution(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateDistribution.Click
        Me.m_plFlow.CreateUnit(cUnitFactory.eUnitType.Distribution)
        Me.UpdateControls()
    End Sub

    Private Sub OnCreateWholesaler(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateWholesaler.Click
        Me.m_plFlow.CreateUnit(cUnitFactory.eUnitType.Wholesaler)
        Me.UpdateControls()
    End Sub

    Private Sub OnCreateRetailer(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateRetailer.Click
        Me.m_plFlow.CreateUnit(cUnitFactory.eUnitType.Retailer)
        Me.UpdateControls()
    End Sub

    Private Sub OnCreateConsumer(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbCreateConsumer.Click
        Me.m_plFlow.CreateUnit(cUnitFactory.eUnitType.Consumer)
        Me.UpdateControls()
    End Sub

    Private Sub OnConvertSelection(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If (Not TypeOf sender Is ToolStripItem) Then Return
        Dim item As ToolStripItem = DirectCast(sender, ToolStripItem)

        If (item.Tag Is Nothing) Then Return
        Me.m_plFlow.ConvertUnit(Me.m_selector.SelectedUnit, DirectCast(item.Tag, cUnitFactory.eUnitType))
        Me.UpdateControls()

    End Sub

#End Region ' Creation buttons

#Region " Control buttons "

    Private Sub OnArrangeLayout(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbArrange.Click

        Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_AUTOLAYOUT, EwEUtils.Core.eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
        Me.m_uic.Core.Messages.SendMessage(fmsg)
        If (fmsg.Reply <> eMessageReply.YES) Then Return

        Try
            Me.m_plFlow.ArrangeGLEE()
        Catch ex As Exception
            cLog.Write(ex, "ValueChain::ArrangeGlee")
        End Try
        Me.UpdateControls()

    End Sub

    Private Sub OnShowGrid(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbShowGrid.Click
        Me.m_plFlow.ShowGrid = Not Me.m_plFlow.ShowGrid
        Me.UpdateControls()
    End Sub

    Private Sub OnToggleAltNames(sender As Object, e As System.EventArgs) _
        Handles m_tsbLocalNames.Click
        My.Settings.ShowAltNames = Not My.Settings.ShowAltNames
        Me.UpdateControls()
        Me.m_plFlow.Invalidate(True)
    End Sub

#End Region ' Control buttons

#Region " Zoomzoom "

    Private Sub OnZoom(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            Me.m_plFlow.ZoomFactor = CSng(DirectCast(sender, ToolStripMenuItem).Tag)
        Catch ex As Exception
            cLog.Write(ex, "ValueChain.ucEditFlow::OnZoom")
        End Try
    End Sub

    Private Sub OnZoomChanged(sender As plFlow, sZoom As Single)
        Try
            Me.UpdateControls()
        Catch ex As Exception
            cLog.Write(ex, "ValueChain.ucEditFlow::OnZoomChanged")
        End Try
    End Sub

    Private Sub OnSelectionChanged(sender As plFlow, selection As Object)
        Try
            Me.UpdateControls()
        Catch ex As Exception
            cLog.Write(ex, "ValueChain.ucEditFlow::OnSelectionChanged")
        End Try
    End Sub

#End Region ' moozmooZ

#End Region ' Event handling

#Region " Internals "

    Private Sub UpdateControls()

        Dim fd As cFlowDiagram = Nothing
        Dim tsi As ToolStripItem = Nothing
        Dim fmt As New cUnitTypeFormatter()
        Dim unit As cUnit = Me.m_selector.SelectedUnit
        Dim img As Image = Nothing
        Dim bCanConvert As Boolean = False

        Me.m_tsbMove.Checked = (Me.m_plFlow.EditMode = plFlow.eEditMode.Move)
        Me.m_tsbLink.Checked = (Me.m_plFlow.EditMode = plFlow.eEditMode.Link)
        Me.m_tsbDelete.Checked = (Me.m_plFlow.EditMode = plFlow.eEditMode.Delete)
        Me.m_tsbShowGrid.Checked = Me.m_plFlow.ShowGrid
        Me.m_tsbLocalNames.Checked = My.Settings.ShowAltNames

        For Each item As ToolStripMenuItem In Me.m_tsddZoom.DropDownItems
            item.Checked = (CSng(item.Tag) = Me.m_plFlow.ZoomFactor)
        Next

        '' Update list of avialable diagrams
        'With Me.m_tsddDiagram.DropDownItems
        '    .Clear()
        '    For i As Integer = 0 To Math.Max(0, Me.m_data.FlowDiagramCount - 1)
        '        fd = Me.m_data.FlowDiagram(i)
        '        tsi = New ToolStripMenuItem()
        '        tsi.Tag = fd
        '        tsi.Text = fd.Name
        '        tsi.ToolTipText = cStringUtils.Localize("View diagram '{0}'", fd.Name)
        '        AddHandler tsi.Click, AddressOf OnSelectDiagram
        '        .Add(tsi)
        '    Next
        'End With

        ' Update list of available conversion options
        Me.m_tssbConvert.DropDownItems.Clear()
        ' Has a selected unit?
        If (unit IsNot Nothing) Then
            img = cUnitImageFactory.GetImage(unit.UnitType)
            If (Not TypeOf unit Is cProducerUnit) Then
                For ut As cUnitFactory.eUnitType = cUnitFactory.eUnitType.Processing To cUnitFactory.eUnitType.Consumer
                    If (unit.UnitType <> ut) Then
                        Dim item As New ToolStripMenuItem(fmt.GetDescriptor(ut), cUnitImageFactory.GetImage(ut), AddressOf OnConvertSelection)
                        item.Tag = ut
                        Me.m_tssbConvert.DropDownItems.Add(item)
                        bCanConvert = True
                    End If
                Next
            End If
        End If

        ' Update conversion UI element
        Me.m_tssbConvert.Image = img
        Me.m_tssbConvert.Enabled = bCanConvert

    End Sub

    Public Property Diagram() As cFlowDiagram
        Get
            Return Me.m_diagram
        End Get
        Set(ByVal value As cFlowDiagram)
            If ReferenceEquals(value, Me.m_diagram) Then Return

            If (Me.m_diagram IsNot Nothing) Then
                Me.m_selector.Init(Nothing, Nothing, Nothing, Nothing)
                Me.m_plFlow.Init(Nothing, Nothing, Nothing, Nothing)
            End If

            Me.m_diagram = value

            If (Me.m_diagram IsNot Nothing) Then
                Me.m_selector.Init(Me.m_uic, Me.m_data, Me.m_plFlow, Me.m_pgDetails)
                Me.m_plFlow.Init(Me.m_uic, Me.m_data, Me.m_diagram, Me.m_selector)
            End If
        End Set
    End Property

    Public Property Data() As cData
        Get
            Return Me.m_data
        End Get
        Set(ByVal value As cData)
            Me.m_data = value
        End Set
    End Property

#End Region ' Internals

End Class
