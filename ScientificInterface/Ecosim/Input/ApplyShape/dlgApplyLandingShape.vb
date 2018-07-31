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
Option Explicit On

Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecosim

    ''' <summary>
    ''' Interface to manage assign forcing functions to fleet/group interactions.
    ''' </summary>
    Public Class dlgApplyLandingShape

        Public Enum eEditMode As Integer
            ''' <summary>Dialog opened for a single landing.</summary>
            FleetGroup = 0
            ''' <summary>Dialog opened for landings of a specific group.</summary>
            Group
            ''' <summary>Dialog opened for landings by a specific fleet.</summary>
            Fleet
            ''' <summary>Dialog opened for all landings.</summary>
            All
        End Enum

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_InteractionManager As cMediatedInteractionManager
        Private m_lInteractions As New List(Of cMediatedInteraction)
        Private m_lFFs As New List(Of cForcingFunction)
        Private m_appl As eForcingFunctionApplication = eForcingFunctionApplication.OffVesselPrice

        Private m_iSelGroup As Integer = -1
        Private m_strSelGroup As String = ""
        Private m_iSelFleet As Integer = -1
        Private m_strSelFleet As String = ""

        ''' <summary>Image list used for displaying small thumbnails.</summary>
        Private m_ilSmall As New ImageList()

        Private m_editMode As eEditMode = eEditMode.FleetGroup
        Private m_nGroups As Integer = 0

#End Region ' Private vars

#Region " Constructors "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Created the dialog for a single fleet or group.
        ''' </summary>
        ''' <param name="iFleet">The fleet to assign shapes to.</param>
        ''' <param name="iGroup">The group to assign shapes to.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, ByVal iGroup As Integer, ByVal iFleet As Integer)
            Try

                Me.Init(uic, eEditMode.FleetGroup)

                ' Remember target indices
                Me.m_iSelGroup = iGroup
                Me.m_iSelFleet = iFleet

                ' Remember target names
                Me.m_strSelGroup = Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelGroup).Name
                Me.m_strSelFleet = Me.m_uic.Core.EcopathFleetInputs(Me.m_iSelFleet).Name

                Me.m_lInteractions.Add(Me.m_InteractionManager.LandingInteraction(Me.m_iSelFleet, Me.m_iSelGroup))

            Catch ex As Exception
                ' NOP
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Created the dialog for a single fleet or group.
        ''' </summary>
        ''' <param name="iTarget">Target this dialog was opened for, can either
        ''' be a fleet or a group depending on the <paramref name="editMode"/>.</param>
        ''' <param name="editMode">Flag stating how this group should be interpreted.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal iTarget As Integer, ByVal editMode As eEditMode)

            Me.Init(uic, editMode)

            Select Case editMode

                Case eEditMode.Group
                    Me.m_iSelGroup = iTarget
                    Me.m_strSelGroup = Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelGroup).Name

                    For i As Integer = 1 To m_nGroups
                        If Me.m_InteractionManager.isLandings(i, Me.m_iSelGroup) Then
                            Me.m_lInteractions.Add(Me.m_InteractionManager.LandingInteraction(i, Me.m_iSelGroup))
                        End If
                    Next

                Case eEditMode.Fleet
                    Me.m_iSelFleet = iTarget
                    Me.m_strSelFleet = Me.m_uic.Core.EcopathFleetInputs(Me.m_iSelFleet).Name

                    For i As Integer = 1 To m_nGroups
                        If Me.m_InteractionManager.isLandings(Me.m_iSelFleet, i) Then
                            Me.m_lInteractions.Add(Me.m_InteractionManager.LandingInteraction(Me.m_iSelFleet, i))
                        End If
                    Next

                Case Else
                    Debug.Assert(False, cStringUtils.Localize("Invalid editmode {0} provided", editMode.ToString))

            End Select

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create the dialog for all diets
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext)

            Me.Init(uic, eEditMode.All)

            For iFleet As Integer = 1 To Me.m_uic.Core.nFleets
                For iGroup As Integer = 1 To Me.m_uic.Core.nGroups
                    If Me.m_InteractionManager.isLandings(iFleet, iGroup) Then
                        Me.m_lInteractions.Add(Me.m_InteractionManager.LandingInteraction(iFleet, iGroup))
                    End If
                Next
            Next
        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            Debug.Assert(Me.m_uic IsNot Nothing)

            Me.LoadAvailableShapes()
            Me.LoadAppliedShapes()

            ' Set labels
            Select Case m_editMode
                Case eEditMode.FleetGroup
                    Me.Text = cStringUtils.Localize(My.Resources.CAPTION_PRICEELASTICITY_LANDING, _
                                            Me.m_iSelFleet, Me.m_strSelFleet, _
                                            Me.m_iSelGroup, Me.m_strSelGroup)
                Case eEditMode.Group
                    Me.Text = cStringUtils.Localize(My.Resources.CAPTION_PRICEELASTICITY_GROUP, Me.m_iSelGroup, Me.m_strSelGroup)
                Case eEditMode.Fleet
                    Me.Text = cStringUtils.Localize(My.Resources.CAPTION_PRICEELASTICITY_FLEET, Me.m_iSelFleet, Me.m_strSelFleet)
                Case eEditMode.All
                    Me.Text = My.Resources.CAPTION_PRICEELASTICITY_ALL
            End Select

            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            For Each img As Image In Me.m_ilSmall.Images
                img.Dispose()
            Next
            Me.m_ilSmall.Images.Clear()
            Me.m_ilSmall.Dispose()

            MyBase.OnFormClosed(e)
        End Sub

#End Region ' Overrides

#Region " Event handlers "

#Region " Termination "

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles OK_Button.Click

            Dim iNumApplied As Integer = m_lvAppliedShapes.Items.Count
            Dim lvItem As ListViewItem = Nothing
            Dim shape As cForcingFunction = Nothing
            Dim iApplication As Integer = 0
            Dim interaction As cMediatedInteraction = Nothing
            Dim ffappl As eForcingFunctionApplication = Nothing

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_APPLYVALUES)
            Me.m_uic.Core.SetBatchLock(cCore.eBatchLockType.Update)

            ' Update
            Try
                ' FG: Jan 17, 2007 
                ' After discussing with Joe about one bug in UI, how to clear the list when no shape is being applied
                ' The solution is we will always loop through the MaxNumShapes 
                ' If one shape is applied, we will set it to the core
                ' If not, we will set it to nothing
                For iInteraction As Integer = 0 To Me.m_lInteractions.Count - 1
                    ' Get PPI
                    interaction = Me.m_lInteractions(iInteraction)
                    ' JS 10sep07: optimized by minimizing the amount of unnecessary updates to the core
                    interaction.LockUpdates = True
                    For iApplicationSlot As Integer = 1 To Me.m_InteractionManager.MaxNShapes
                        If iApplicationSlot <= iNumApplied Then ' The shape is being applied
                            lvItem = m_lvAppliedShapes.Items(iApplicationSlot - 1)
                            shape = DirectCast(lvItem.Tag, cForcingFunction)
                            ffappl = DirectCast(lvItem.SubItems(1).Tag, eForcingFunctionApplication)
                            interaction.setShape(iApplicationSlot, shape, ffappl)
                        Else
                            interaction.setShape(iApplicationSlot, Nothing)
                        End If
                    Next
                    interaction.LockUpdates = False
                Next
            Catch ex As Exception

            End Try

            Me.m_uic.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim, True)
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles Cancel_Button.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

#End Region ' Termination

#Region " Add and remove "

        Private Sub lvAllShapes_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_lvAllShapes.DoubleClick
            Me.AddShapes()
        End Sub

        Private Sub lvAppliedShapes_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_lvAppliedShapes.DoubleClick
            Me.RemoveShapes()
        End Sub

        Private Sub OnAdd(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnAdd.Click
            Me.AddShapes()
        End Sub

        Private Sub OnRemove(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnRemove.Click
            Me.RemoveShapes()
        End Sub

#End Region ' Add and remove

#Region " Selections "

        Private Sub lvAppliedShapes_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_lvAppliedShapes.SelectedIndexChanged, m_lvAllShapes.SelectedIndexChanged
            Me.UpdateControls()
        End Sub

        Private Sub OnAllShapesGotFocus(sender As Object, e As System.EventArgs) Handles m_lvAllShapes.GotFocus
            Me.m_lvAppliedShapes.SelectedIndices.Clear()
        End Sub

        Private Sub OnAppliedShapesGotFocus(sender As Object, e As System.EventArgs) Handles m_lvAppliedShapes.GotFocus
            Me.m_lvAllShapes.SelectedIndices.Clear()
        End Sub

#End Region ' Selections

#End Region ' Event handlers

#Region " Private methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Populate the dialog.
        ''' </summary>
        ''' <param name="uic"></param>
        ''' <param name="editMode"></param>
        ''' -------------------------------------------------------------------
        Private Sub Init(ByVal uic As cUIContext, _
                         ByVal editMode As eEditMode)

            Me.InitializeComponent()
            Me.m_uic = uic

            ' Get the interaction manager
            Me.m_InteractionManager = Me.m_uic.Core.MediatedInteractionManager

            Me.m_editMode = editMode

            ' Get the available shapes that can be applied
            For Each shape As cForcingFunction In Me.m_uic.Core.LandingsShapeManager
                If (Me.IsAllowedShape(shape)) Then
                    Me.m_lFFs.Add(shape)
                End If
            Next

            ' Generate thumbnails from shapes
            Me.m_ilSmall.ImageSize = New Size(SmallIconSize, SmallIconSize)
            Me.GenerateShapeThumbnails()

            Me.m_nGroups = Me.m_uic.Core.nGroups
            Me.m_lInteractions.Clear()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Change the default multiplier, and update all selected appls.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub SetMultiplier(ByVal appl As eForcingFunctionApplication)
            ' Store appl mode
            Me.m_appl = appl
            ' Update all selected items
            For Each item As ListViewItem In Me.m_lvAppliedShapes.SelectedItems
                If Me.IsAllowedShape(Me.Shape(item)) Then
                    Me.UpdateAppliedShape(item, appl)
                End If
            Next
        End Sub

        Private ReadOnly Property SmallIconSize() As Integer
            Get
                Debug.Assert(Me.m_uic.StyleGuide IsNot Nothing)
                Return CInt(Math.Ceiling(Me.m_uic.StyleGuide.ThumbnailSize / 3))
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selected shape for a list view item.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property Shape(ByVal lvi As ListViewItem) As cForcingFunction
            Get
                Return DirectCast(lvi.Tag, cForcingFunction)
            End Get
            Set(ByVal value As cForcingFunction)
                lvi.Tag = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add avaliable shapes to the applications.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub AddShapes()

            Dim colSelected As ListView.SelectedIndexCollection = m_lvAllShapes.SelectedIndices
            Dim shapeSelected As cForcingFunction = Nothing
            Dim shapeTest As cForcingFunction = Nothing
            Dim iNumApplied As Integer = 0
            Dim bFound As Boolean = False

            For Each item As ListViewItem In Me.m_lvAppliedShapes.Items
                If Me.IsAllowedShape(Shape(item)) Then
                    iNumApplied += 1
                End If
            Next

            For Each itemSrc As ListViewItem In Me.m_lvAllShapes.SelectedItems

                'Get the shape data
                shapeSelected = Shape(itemSrc)

                ' Sanity check
                Debug.Assert(shapeSelected IsNot Nothing, "Unable to locate applied forcing function")

                ' Check if already used
                bFound = False
                For Each itemTest As ListViewItem In Me.m_lvAppliedShapes.Items
                    shapeTest = Shape(itemTest)
                    If ReferenceEquals(shapeSelected, shapeTest) Then bFound = True
                Next

                ' Not found, and still room for more?
                If (Not bFound) And _
                   (iNumApplied < Me.m_InteractionManager.MaxNShapes) Then
                    ' #Yes: add
                    itemSrc = New ListViewItem(cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, shapeSelected.Index, shapeSelected.Name))
                    itemSrc.ImageIndex = Me.m_lFFs.IndexOf(shapeSelected)
                    itemSrc.SubItems.Add("")
                    itemSrc.Tag = shapeSelected
                    itemSrc.Selected = True
                    Me.UpdateAppliedShape(itemSrc, Me.m_appl)

                    Me.m_lvAppliedShapes.Items.Add(itemSrc)
                    iNumApplied += 1
                End If
            Next
            Me.UpdateControls()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove applications.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub RemoveShapes()
            ' Remove all allowed shapes
            For Each item As ListViewItem In Me.m_lvAppliedShapes.SelectedItems
                If Me.IsAllowedShape(Me.Shape(item)) Then
                    Me.m_lvAppliedShapes.Items.Remove(item)
                End If
            Next
            ' Update selection
            If Me.m_lvAppliedShapes.Items.Count > 0 Then
                Me.m_lvAppliedShapes.Items(Me.m_lvAppliedShapes.Items.Count - 1).Selected = True
            End If
            ' Yoho
            Me.UpdateControls()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Limit user interactions.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateControls()

            Dim colSelected As ListView.SelectedIndexCollection = Me.m_lvAppliedShapes.SelectedIndices
            Dim iAppliedSelected As Integer = 0
            Dim iApplied As Integer = 0
            Dim iAvailableSelected As Integer = Me.m_lvAllShapes.SelectedItems.Count

            ' Check selected item status
            For Each lvi As ListViewItem In Me.m_lvAppliedShapes.Items
                If Me.IsAllowedShape(Me.Shape(lvi)) Then
                    iApplied += 1
                    If lvi.Selected Then iAppliedSelected += 1
                End If
            Next

            Me.m_btnAdd.Enabled = (iAvailableSelected > 0) And (iApplied < Me.m_InteractionManager.MaxNShapes)
            Me.m_btnRemove.Enabled = (iAppliedSelected > 0)

        End Sub

        Private Sub UpdateAppliedShape(ByVal item As ListViewItem, ByVal appl As eForcingFunctionApplication)

            Dim fmt As New cFFApplicationTargetTypeFormatter()
            Dim shape As cForcingFunction = Me.Shape(item)

            item.SubItems(1).Text = fmt.GetDescriptor(appl)
            item.SubItems(1).Tag = appl

        End Sub

        Private Sub GenerateShapeThumbnails()

            Dim dtHandlers As New Dictionary(Of eDataTypes, cShapeGUIHandler)
            Dim handler As cShapeGUIHandler = Nothing
            Dim rc As New Rectangle(0, 0, Me.SmallIconSize, Me.SmallIconSize)
            Dim bmp As Bitmap = Nothing

            ' For all selectable shapes
            For Each shape As cForcingFunction In Me.m_lFFs
                ' Get handler
                If Not dtHandlers.ContainsKey(shape.DataType) Then
                    dtHandlers(shape.DataType) = cShapeGUIHandler.GetShapeUIHandler(shape, Me.m_uic)
                End If
                ' Create bmp
                bmp = New Bitmap(rc.Width, rc.Height)
                ' Get graphics content
                Using g As Graphics = Graphics.FromImage(bmp)
                    cShapeImage.DrawShape(Me.m_uic, shape, rc, g, dtHandlers(shape.DataType).Color, eSketchDrawModeTypes.Line)
                End Using
                ' Add image
                Me.m_ilSmall.Images.Add(bmp)
            Next
            ' Forget
            dtHandlers.Clear()

        End Sub

        Private Sub LoadAvailableShapes()

            Dim item As ListViewItem = Nothing
            Dim i As Integer = 0

            Me.m_lvAllShapes.Items.Clear()

            If Me.m_lFFs.Count > 0 Then

                For Each ff As cForcingFunction In Me.m_lFFs
                    item = New ListViewItem(cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, ff.Index, ff.Name))
                    item.ImageIndex = Me.m_lFFs.IndexOf(ff)
                    item.Tag = ff
                    Me.m_lvAllShapes.Items.Add(item)
                    i += 1
                Next

                Me.m_lvAllShapes.View = View.SmallIcon
                Me.m_lvAllShapes.Items(0).Selected = True
                Me.m_lvAllShapes.SmallImageList = Me.m_ilSmall

            End If

        End Sub

        Private Sub LoadAppliedShapes()

            If (m_editMode = eEditMode.FleetGroup) Then

                Dim ppi As cMediatedInteraction = Me.m_lInteractions(0)
                Dim item As ListViewItem = Nothing
                Dim shape As cForcingFunction = Nothing
                Dim ffappl As eForcingFunctionApplication

                If ppi Is Nothing Then Return
                For i As Integer = 1 To ppi.nAppliedShapes

                    ppi.getShape(i, shape, ffappl)

                    item = New ListViewItem(cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, shape.Index, shape.Name))
                    item.ImageIndex = Me.m_lFFs.IndexOf(shape)
                    item.SubItems.Add("")
                    item.Tag = shape

                    If Not Me.IsAllowedShape(shape) Then item.ForeColor = SystemColors.GrayText

                    Me.UpdateAppliedShape(item, ffappl)
                    Me.m_lvAppliedShapes.Items.Add(item)

                Next
            End If

            Me.m_lvAppliedShapes.View = View.Details
            Me.m_lvAppliedShapes.SmallImageList = m_ilSmall

        End Sub

        Private Function IsAllowedShape(ByVal shape As cShapeData) As Boolean
            Return True
        End Function

#End Region ' Private methods

    End Class

End Namespace

