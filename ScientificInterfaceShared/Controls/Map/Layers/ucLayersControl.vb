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

Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Properties
Imports EwECore

#End Region ' Imports

Namespace Controls.Map

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Container for <see cref="ucLayerGroup"/>s.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucLayersControl
        Implements IUIElement

        Private m_uic As cUIContext = Nothing
        Private m_dtGroups As New Dictionary(Of String, ucLayerGroup)
        Private m_dtLayerToGroup As New Dictionary(Of cDisplayLayer, String)

        Private m_lEditorsGroup As New List(Of IGroupFilter)
        Private m_lEditorsFleet As New List(Of IFleetFilter)
        Private m_lEditorsMonth As New List(Of IMonthFilter)

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Public Property UIContext As cUIContext Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
                If (Me.m_uic IsNot Nothing) Then
                    Me.Clear()
                End If
            End Set
        End Property

#Region " Item access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a <see cref="cDisplayLayer">display layer</see> to this control.
        ''' </summary>
        ''' <param name="layer">The <see cref="cDisplayLayer">display layer</see> to add.</param>
        ''' <param name="layerPosition">Layer to position this layer before, if any</param>
        ''' <remarks>A layer can only be added once.</remarks>
        ''' -------------------------------------------------------------------
        Public Sub AddLayer(ByVal layer As cDisplayLayer, ByVal strGroup As String, ByVal strCommand As String, Optional ByVal layerPosition As cDisplayLayer = Nothing)

            Dim ucg As ucLayerGroup = Me.FindGroup(strGroup)

            If (ucg Is Nothing) Then
                ' Add group
                Me.AddGroup(strGroup, strCommand)
                ucg = Me.FindGroup(strGroup)
            End If

            ' Add layer
            ucg.AddLayer(layer, layerPosition)
            Me.m_dtLayerToGroup.Add(layer, strGroup)
            AddHandler layer.LayerChanged, AddressOf OnLayerChanged

            ' Link to editors
            If (TypeOf layer Is cDisplayLayerRaster) Then
                Dim rl As cDisplayLayerRaster = DirectCast(layer, cDisplayLayerRaster)
                Dim edt As cLayerEditor = rl.Editor
                Dim bFilter As Boolean = False

                If (TypeOf edt Is IGroupFilter) Then
                    Me.m_lEditorsGroup.Add(DirectCast(edt, IGroupFilter))
                    bFilter = True
                End If
                If (TypeOf edt Is IFleetFilter) Then
                    Me.m_lEditorsFleet.Add(DirectCast(edt, IFleetFilter))
                    bFilter = True
                End If
                If (TypeOf edt Is IMonthFilter) Then
                    Me.m_lEditorsMonth.Add(DirectCast(edt, IMonthFilter))
                    bFilter = True
                End If

                If bFilter Then
                    AddHandler DirectCast(edt, IContentFilter).FilterChanged, AddressOf OnLayerFilterChanged
                End If
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a layer from this control.
        ''' </summary>
        ''' <param name="layer">The <see cref="cDisplayLayer">display layer</see> to remove.</param>
        ''' <remarks>A layer can only be removed once.</remarks>
        ''' -------------------------------------------------------------------
        Public Sub RemoveLayer(ByVal layer As cDisplayLayer)

            Dim ucg As ucLayerGroup = Me.FindGroup(layer)

            If ucg Is Nothing Then Return

            ' Remove link to editor, if any
            If (TypeOf layer Is cDisplayLayerRaster) Then

                Dim rl As cDisplayLayerRaster = DirectCast(layer, cDisplayLayerRaster)
                Dim edt As cLayerEditor = rl.Editor
                Dim bFilter As Boolean = False

                If (TypeOf edt Is IGroupFilter) Then
                    Me.m_lEditorsGroup.Remove(DirectCast(edt, IGroupFilter))
                    bFilter = True
                End If
                If (TypeOf edt Is IFleetFilter) Then
                    Me.m_lEditorsFleet.Remove(DirectCast(edt, IFleetFilter))
                    bFilter = True
                End If
                If (TypeOf edt Is IMonthFilter) Then
                    Me.m_lEditorsMonth.Remove(DirectCast(edt, IMonthFilter))
                    bFilter = True
                End If

                If bFilter Then
                    RemoveHandler DirectCast(edt, IContentFilter).FilterChanged, AddressOf OnLayerFilterChanged
                End If

            End If

            ' Remove layer
            RemoveHandler layer.LayerChanged, AddressOf OnLayerChanged
            ucg.RemoveLayer(layer)
            Me.m_dtLayerToGroup.Remove(layer)

        End Sub

        Public Function Layers(strGroup As String) As cDisplayLayer()
            Dim ucg As ucLayerGroup = Nothing
            If Me.m_dtGroups.ContainsKey(strGroup) Then
                ' #Yes: get group layer control
                ucg = Me.FindGroup(strGroup)
                Return ucg.Layers
            End If
            Return New cDisplayLayer() {}
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a layer group to this control.
        ''' </summary>
        ''' <param name="strGroup">Name of the group to add.</param>
        ''' -------------------------------------------------------------------
        Public Sub AddGroup(ByVal strGroup As String, _
                            ByVal strCommand As String, _
                            Optional ByVal bVisible As Boolean = True, _
                            Optional ByVal bClearGroup As Boolean = True)

            Dim ucg As ucLayerGroup = Nothing

            ' Group already exists?
            If Me.m_dtGroups.ContainsKey(strGroup) Then
                ' #Yes: get group layer control
                ucg = Me.FindGroup(strGroup)
                ' Must clear?
                If bClearGroup Then
                    ' #Yes: clear it
                    For Each l As cDisplayLayer In ucg.Layers
                        Me.RemoveLayer(l)
                    Next
                End If
            Else
                ' #No: create new group layer control
                ucg = New ucLayerGroup(Me.m_uic, strGroup, strCommand)
                Me.m_fpItems.Controls.Add(ucg)
                Me.m_dtGroups(strGroup) = ucg
            End If

            ' Configure group layer control
            ucg.ShowAllLayers(bVisible)
            ucg.SetCollapsed(Not bVisible)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a layer group from this control.
        ''' </summary>
        ''' <param name="strGroup">Name of the group to remove.</param>
        ''' -------------------------------------------------------------------
        Public Sub RemoveGroup(ByVal strGroup As String)

            If Not Me.m_dtGroups.ContainsKey(strGroup) Then Return

            Dim ucg As ucLayerGroup = Me.FindGroup(strGroup)

            If (ucg Is Nothing) Then Return

            For Each l As cDisplayLayer In ucg.Layers
                Me.RemoveLayer(l)
            Next

            Me.m_fpItems.Controls.Remove(ucg)
            Me.m_dtGroups.Remove(strGroup)

        End Sub

        Public Sub Clear()
            Dim lstrGroup As New List(Of String)
            For Each strGroup As String In Me.m_dtGroups.Keys
                lstrGroup.Add(strGroup)
            Next
            For Each strgroup As String In lstrGroup
                Me.RemoveGroup(strgroup)
            Next
        End Sub

        Public Sub ShowGroup(ByVal strGroup As String, ByVal bShow As Boolean, Optional ByVal bShowGroupControl As Boolean = True)
            If Not Me.m_dtGroups.ContainsKey(strGroup) Then Return

            Dim ucg As ucLayerGroup = Me.FindGroup(strGroup)
            If (ucg Is Nothing) Then Return

            ucg.ShowAllLayers(bShow)
            ucg.Visible = bShowGroupControl
        End Sub

        Public Sub EnableGroup(ByVal strGroup As String, ByVal bEditable As Boolean)
            If Not Me.m_dtGroups.ContainsKey(strGroup) Then Return

            Dim ucg As ucLayerGroup = Me.FindGroup(strGroup)
            If (ucg Is Nothing) Then Return

            ucg.EnableAllLayers(bEditable)
        End Sub

        Private m_iLockCount As Integer = 0

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Pause item reorganization on this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub LockUpdates()

            If (Me.m_iLockCount = 0) Then
                Me.m_fpItems.SuspendLayout()
                Application.DoEvents()
                For Each uc As UserControl In Me.m_fpItems.Controls
                    DirectCast(uc, ucLayerGroup).LockUpdates()
                Next
            End If

            Me.m_iLockCount += 1

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Resume item reorganization on this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub UnlockUpdates()

            Me.m_iLockCount -= 1

            If (Me.m_iLockCount = 0) Then
                For Each uc As UserControl In Me.m_fpItems.Controls
                    DirectCast(uc, ucLayerGroup).UnlockUpdates()
                Next
                Me.ResizeControls()
                Me.m_fpItems.ResumeLayout()
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get all the group controls in this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Groups As ucLayerGroup()
            Get
                Dim aug(Math.Max(0, Me.m_dtGroups.Count - 1)) As ucLayerGroup
                Me.m_dtGroups.Values.CopyTo(aug, 0)
                Return aug
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get all group names registered to this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property GroupNames As String()
            Get
                Dim lstr As New List(Of String)
                For Each str As String In Me.m_dtGroups.Keys
                    lstr.Add(str)
                Next
                Return lstr.ToArray()
            End Get
        End Property

#End Region ' Item access

#Region " Events "

        Private Sub OnLayerChanged(ByVal l As cDisplayLayer, ByVal updateType As cDisplayLayer.eChangeFlags)
            If ((updateType And cDisplayLayer.eChangeFlags.Selected) = cDisplayLayer.eChangeFlags.Selected) Then
                ' Make sure only one layer is selected at the time
                Me.UpdateSelectedLayer(l)
            End If
        End Sub

        Private m_bInEvent As Boolean = False

        Private Sub OnLayerFilterChanged(filter As IContentFilter)

            If (Me.m_bInEvent = True) Then Return
            Me.m_bInEvent = True

            Try
                If (TypeOf filter Is IGroupFilter) Then
                    Dim iGroup As Integer = DirectCast(filter, IGroupFilter).Group
                    For Each f As IGroupFilter In Me.m_lEditorsGroup
                        f.Group = iGroup
                    Next
                End If
                If (TypeOf filter Is IFleetFilter) Then
                    Dim iFleet As Integer = DirectCast(filter, IFleetFilter).Fleet
                    For Each f As IFleetFilter In Me.m_lEditorsFleet
                        f.Fleet = iFleet
                    Next
                End If
                If (TypeOf filter Is IMonthFilter) Then
                    Dim iMonth As Integer = DirectCast(filter, IMonthFilter).Month
                    For Each f As IMonthFilter In Me.m_lEditorsMonth
                        f.Month = iMonth
                    Next
                End If

            Catch ex As Exception

            End Try

            Me.m_bInEvent = False

        End Sub

        Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
            Me.m_fpItems.Width = Me.Width - Me.Margin.Horizontal
            Me.m_fpItems.Height = Me.Height - Me.Margin.Vertical
        End Sub

        Private Sub fpItems_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_fpItems.Resize
            Me.ResizeControls()
        End Sub

#End Region ' Events

#Region " Implementation "

        Private Sub ResizeControls()

            Dim iWidth As Integer = Me.m_fpItems.ClientRectangle.Width - Me.m_fpItems.Margin.Horizontal
            Me.m_fpItems.SuspendLayout()
            For Each uc As UserControl In Me.m_fpItems.Controls
                uc.Width = iWidth
            Next uc
            Me.m_fpItems.ResumeLayout()

        End Sub

        ''' <summary>Flag to prevent selection update recursion.</summary>
        Private m_bInSelectionUpdate As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Make sure only one layer is selected.
        ''' </summary>
        ''' <param name="layerSelect">The <see cref="cDisplayLayer">display layer</see> 
        ''' that has been selected.</param>
        ''' -------------------------------------------------------------------
        Private Sub UpdateSelectedLayer(ByVal layerSelect As cDisplayLayer)

            ' Abort if already busy
            If Me.m_bInSelectionUpdate = True Then Return

            ' Flag as busy
            Me.m_bInSelectionUpdate = True

            ' First call: fire selection command
            Me.FireSelectionCommand(layerSelect)

            ' Clean selection state of all other layer
            For Each layerTest As cDisplayLayer In Me.m_dtLayerToGroup.Keys
                ' #Yes: is it selected?
                If ((Not ReferenceEquals(layerTest, layerSelect)) And (layerTest.IsSelected() = True)) Then
                    ' #Yes: clear its selection state
                    layerTest.IsSelected = False
                    ' Make the world respond to this. Note that this call will call
                    ' OnLayerChanged, which in turn will call this method, UpdateSelectedLayer.
                    ' To prevent this from causing endless loops, the flag m_bInSelectionUpdate
                    ' allows only the first layer update (which is most likely a user-triggered
                    ' change in selection) to cause a deselect of all other layers.
                    layerTest.Update(cDisplayLayer.eChangeFlags.Selected)
                End If
            Next layerTest

            ' Done
            Me.m_bInSelectionUpdate = False

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Fire global selection command to allow users to manage remarks for
        ''' a <see cref="EwECore.cCoreInputOutputBase">source</see> attached a layer.
        ''' </summary>
        ''' <param name="layer">The <see cref="cDisplayLayer">display layer</see> 
        ''' that has been selected.</param>
        ''' -------------------------------------------------------------------
        Private Sub FireSelectionCommand(ByVal layer As cDisplayLayer)

            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand(cPropertySelectionCommand.COMMAND_NAME)
            Dim sc As cPropertySelectionCommand = Nothing
            Dim prop As cProperty = Nothing

            If (layer IsNot Nothing) Then
                prop = layer.GetNameProperty()
            End If

            If (cmd IsNot Nothing) And (prop IsNot Nothing) Then
                If (TypeOf cmd Is cPropertySelectionCommand) Then
                    sc = DirectCast(cmd, cPropertySelectionCommand)
                    sc.Invoke(prop)
                End If
            End If

        End Sub

        Private Function FindGroup(ByVal strGroup As String) As ucLayerGroup
            If (Not Me.m_dtGroups.ContainsKey(strGroup)) Then Return Nothing
            Return Me.m_dtGroups(strGroup)
        End Function

        Private Function FindGroup(ByVal l As cDisplayLayer) As ucLayerGroup
            If Me.m_dtLayerToGroup.ContainsKey(l) Then Return Me.FindGroup(Me.m_dtLayerToGroup(l))
            Return Nothing
        End Function

#End Region ' Implementation

    End Class

End Namespace