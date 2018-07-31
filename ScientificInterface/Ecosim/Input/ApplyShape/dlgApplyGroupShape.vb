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
    ''' Interface to manage assign forcing functions to pred/prey interactions.
    ''' </summary>
    Public Class dlgApplyGroupShape

        Private Enum eEditMode As Integer
            ''' <summary>Dialog opened for a single group.</summary>
            Group = 0
            ''' <summary>Dialog opened for all groups.</summary>
            All
        End Enum

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_InteractionManager As cMediatedInteractionManager
        Private m_lInteractions As New List(Of cMediatedInteraction)
        Private m_lFFs As New List(Of cForcingFunction)
        ' Fixed in this dialog
        Private m_appl As eForcingFunctionApplication = eForcingFunctionApplication.MortOther

        Private m_iSelGroup As Integer = -1

        ''' <summary>Image list used for displaying small thumbnails.</summary>
        Private m_ilSmall As New ImageList()

        Private m_editMode As eEditMode = eEditMode.Group
        Private m_nGroups As Integer = 0

        Private m_groupfilter As eGroupFilter = eGroupFilter.Consumer

        Private m_iMaxShapes As Integer = 0

#End Region ' Private vars

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext, ByVal iGroup As Integer)
            Try
                Me.Init(uic, eEditMode.Group)
                Me.m_iSelGroup = iGroup
                Me.m_lInteractions.Add(Me.m_InteractionManager.GroupInteraction(Me.m_iSelGroup))
            Catch ex As Exception
                ' NOP
            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create the dialog for all pred/prey combinations
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext)

            Me.Init(uic, eEditMode.All)

            For iGroup As Integer = 1 To Me.m_uic.Core.nLivingGroups
                Me.m_lInteractions.Add(m_InteractionManager.GroupInteraction(iGroup))
            Next

        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            Debug.Assert(Me.m_uic IsNot Nothing)

            Dim fmt As New cCoreInterfaceFormatter()

            For Each it As cMediatedInteraction In Me.m_lInteractions
                Me.m_iMaxShapes = Math.Max(Me.m_iMaxShapes, it.MaxNumShapes)
            Next

            Me.LoadAvailableShapes()
            Me.LoadAppliedShapes()

            ' Load Prey and predator pair name
            Select Case m_editMode
                Case eEditMode.Group
                    Me.m_lblSource.Text = cStringUtils.Localize(My.Resources.FF_APPLICATION_MORTOTHER, fmt.GetDescriptor(Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelGroup)))
                Case eEditMode.All
                    Me.m_lblSource.Text = cStringUtils.Localize(My.Resources.FF_APPLICATION_MORTOTHER, SharedResources.GENERIC_VALUE_ALL)
                Case Else
                    Debug.Assert(False, "Mode not supported")
            End Select

            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

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
            Dim ppi As cMediatedInteraction = Nothing
            Dim ffappl As eForcingFunctionApplication = Nothing

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_APPLYVALUES)
            Me.m_uic.Core.SetBatchLock(cCore.eBatchLockType.Update)

            ' Update Applied Shape info for this Pred Prey Pair
            Try
                For iPPI As Integer = 0 To Me.m_lInteractions.Count - 1
                    ' Get PPI
                    ppi = Me.m_lInteractions(iPPI)

                    ppi.LockUpdates = True

                    iApplication = 1
                    ' For all applied forcing functions
                    For Each lvItem In Me.m_lvAppliedShapes.Items
                        ' Get shape
                        shape = Me.Shape(lvItem)
                        ' Still room to apply? (which should be; the UI has been enforcing this already)
                        If (iApplication <= iNumApplied) Then
                            ' #Yes: add application
                            ffappl = DirectCast(lvItem.SubItems(1).Tag, eForcingFunctionApplication)
                            ppi.setShape(iApplication, shape, ffappl)
                        End If
                        iApplication += 1
                    Next

                    ' Clear remainging interactions
                    While iApplication <= Me.m_iMaxShapes
                        ppi.setShape(iApplication, Nothing)
                        iApplication += 1
                    End While

                    ppi.LockUpdates = False
                Next
            Catch ex As Exception

            End Try

            Me.m_uic.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim, True)
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            Me.DialogResult = DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles Cancel_Button.Click
            Me.DialogResult = DialogResult.Cancel
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

#Region " Multipliers "

        Private Sub OnSetMultiplier(sender As Object, e As System.EventArgs)

            Try
                Dim rb As RadioButton = DirectCast(sender, RadioButton)
                Dim appl As eForcingFunctionApplication = DirectCast(rb.Tag, eForcingFunctionApplication)
                If rb.Checked Then Me.SetMultiplier(appl)
            Catch ex As Exception
                ' Whoah!
            End Try

        End Sub

#End Region ' Multipliers 

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
        Private Sub Init(ByVal uic As cUIContext, ByVal editMode As eEditMode)

            Me.InitializeComponent()
            Me.m_uic = uic

            ' Get the Prey - Pred interaction manager
            Me.m_InteractionManager = Me.m_uic.Core.MediatedInteractionManager
            Me.m_editMode = editMode

            ' Set title
            Me.Text = My.Resources.ECOSIM_CAPTION_APPLYOTHERMORT

            For Each shape As cForcingFunction In Me.m_uic.Core.ForcingShapeManager
                Me.m_lFFs.Add(shape)
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
                Me.UpdateAppliedShape(item, appl)
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
                iNumApplied += 1
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
                If (Not bFound) And
                   (iNumApplied < Me.m_iMaxShapes) Then
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
                Me.m_lvAppliedShapes.Items.Remove(item)
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
                iApplied += 1
                If lvi.Selected Then iAppliedSelected += 1
            Next

            Me.m_btnAdd.Enabled = (iAvailableSelected > 0) And (iApplied < Me.m_iMaxShapes)
            Me.m_btnRemove.Enabled = (iAppliedSelected > 0)

        End Sub

        Private Sub UpdateAppliedShape(ByVal item As ListViewItem, ByVal appl As eForcingFunctionApplication)

            Dim fmt As New cFFApplicationTargetTypeFormatter()
            Dim shape As cForcingFunction = Me.Shape(item)

            item.SubItems(1).Tag = appl
            item.SubItems(1).Text = fmt.GetDescriptor(appl)

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

                Me.UpdateAppliedShape(item, ffappl)
                Me.m_lvAppliedShapes.Items.Add(item)

            Next

            Me.m_lvAppliedShapes.View = View.Details
            Me.m_lvAppliedShapes.SmallImageList = m_ilSmall

        End Sub

#End Region ' Private methods

    End Class

End Namespace
