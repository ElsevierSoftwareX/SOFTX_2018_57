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
    Public Class dlgApplyPredPreyShape

        Public Enum eEditMode As Integer
            ''' <summary>Dialog opened for a single pred/prey combination.</summary>
            PredPrey = 0
            ''' <summary>Dialog opened for all diets involving this unfortunate prey.</summary>
            Prey
            ''' <summary>Dialog opened for all diets of a predator.</summary>
            Predator
            ''' <summary>Dialog opened for all diets.</summary>
            All
        End Enum

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_InteractionManager As cMediatedInteractionManager
        Private m_lInteractions As New List(Of cMediatedInteraction)
        Private m_lFFs As New List(Of cForcingFunction)
        Private m_appl As eForcingFunctionApplication = eForcingFunctionApplication.SearchRate

        Private m_iSelPrey As Integer = -1
        Private m_iSelPred As Integer = -1

        ''' <summary>Image list used for displaying small thumbnails.</summary>
        Private m_ilSmall As New ImageList()

        Private m_editMode As eEditMode = eEditMode.PredPrey
        Private m_nGroups As Integer = 0

        Private m_shapeMode As eShapeCategoryTypes = eShapeCategoryTypes.NotSet
        Private m_groupfilter As eGroupFilter = eGroupFilter.Consumer

        Private m_iMaxShapes As Integer = 0

#End Region ' Private vars

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext,
                       ByVal iPrey As Integer, ByVal iPred As Integer,
                       ByVal shapeType As eShapeCategoryTypes,
                       ByVal bConsumers As eGroupFilter)
            Try

                Me.Init(uic, eEditMode.PredPrey, shapeType, bConsumers)

                ' the index for selected prey and predator index
                Me.m_iSelPrey = iPrey
                Me.m_iSelPred = iPred

                Me.m_lInteractions.Add(Me.m_InteractionManager.PredPreyInteraction(Me.m_iSelPred, Me.m_iSelPrey))

            Catch ex As Exception
                ' NOP
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Created the dialog for a single predator or prey.
        ''' </summary>
        ''' <param name="iGroup">Group this dialog was opened for.</param>
        ''' <param name="editMode">Flag stating how this group should be interpreted.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext,
                       ByVal iGroup As Integer,
                       ByVal editMode As eEditMode,
                       ByVal shapeType As eShapeCategoryTypes,
                       ByVal bConsumers As eGroupFilter)

            Me.Init(uic, editMode, shapeType, bConsumers)

            Select Case editMode

                Case eEditMode.Prey
                    Me.m_iSelPrey = iGroup

                    For i As Integer = 1 To m_nGroups
                        If Me.m_InteractionManager.isPredPrey(i, Me.m_iSelPrey) Then
                            Me.m_lInteractions.Add(m_InteractionManager.PredPreyInteraction(i, Me.m_iSelPrey))
                        End If
                    Next

                Case eEditMode.Predator
                    Me.m_iSelPred = iGroup

                    For i As Integer = 1 To m_nGroups
                        If Me.m_InteractionManager.isPredPrey(Me.m_iSelPred, i) Then
                            Me.m_lInteractions.Add(m_InteractionManager.PredPreyInteraction(Me.m_iSelPred, i))
                        End If
                    Next

                Case Else
                    Debug.Assert(False, cStringUtils.Localize("Invalid editmode {0} provided, expected Pred or Prey", editMode.ToString))

            End Select

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create the dialog for all diets
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext,
                       ByVal shapeType As eShapeCategoryTypes,
                       ByVal bConsumers As eGroupFilter)

            Me.Init(uic, eEditMode.All, shapeType, bConsumers)

            For iPred As Integer = 1 To Me.m_uic.Core.nLivingGroups
                ' For each row (rowIndex - Prey)
                For iPrey As Integer = 1 To Me.m_uic.Core.nGroups
                    ' Can assign FF at this spot in the matrix?
                    If Me.m_InteractionManager.isPredPrey(iPred, iPrey) Then
                        Me.m_lInteractions.Add(m_InteractionManager.PredPreyInteraction(iPred, iPrey))
                    End If
                Next
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
            Me.ConfigureApplicationControls()
            Me.LoadAppliedShapes()

            ' Load Prey and predator pair name
            Select Case m_editMode
                Case eEditMode.PredPrey
                    Me.m_lblTarget.Text = cStringUtils.Localize(Me.m_lblTarget.Text, fmt.GetDescriptor(Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelPrey)))
                    Me.m_lblSource.Text = cStringUtils.Localize(Me.m_lblSource.Text, fmt.GetDescriptor(Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelPred)))
                Case eEditMode.Prey
                    Me.m_lblTarget.Text = cStringUtils.Localize(Me.m_lblTarget.Text, fmt.GetDescriptor(Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelPrey)))
                    Me.m_lblSource.Text = cStringUtils.Localize(Me.m_lblSource.Text, SharedResources.GENERIC_VALUE_ALL)
                Case eEditMode.Predator
                    Me.m_lblTarget.Text = cStringUtils.Localize(Me.m_lblTarget.Text, SharedResources.GENERIC_VALUE_ALL)
                    Me.m_lblSource.Text = cStringUtils.Localize(Me.m_lblSource.Text, fmt.GetDescriptor(Me.m_uic.Core.EcoPathGroupInputs(Me.m_iSelPred)))
                Case eEditMode.All
                    Me.m_lblSource.Text = cStringUtils.Localize(Me.m_lblSource.Text, SharedResources.GENERIC_VALUE_ALL)
                    Me.m_lblTarget.Text = cStringUtils.Localize(Me.m_lblTarget.Text, SharedResources.GENERIC_VALUE_ALL)
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
            Dim ppi As cMediatedInteraction = Nothing
            Dim ffappl As eForcingFunctionApplication = Nothing

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_APPLYVALUES)
            Me.m_uic.Core.SetBatchLock(cCore.eBatchLockType.Update)

            ' Update Applied Shape info for this Pred Prey Pair
            Try
                ' FG: Jan 17, 2007 
                ' After discussing with Joe about one bug in UI, how to clear the list when no shape is being applied
                ' The solution is we will always loop through the MaxNumShapes 
                ' If one shape is applied, we will set it to the core
                ' If not, we will set it to nothing
                For iPPI As Integer = 0 To Me.m_lInteractions.Count - 1
                    ' Get PPI
                    ppi = Me.m_lInteractions(iPPI)

                    ' JS 10sep07: optimized by minimizing the amount of unnecessary updates to the core
                    ppi.LockUpdates = True

                    iApplication = 1
                    ' For all applied shapes (note that this contains ff and med shapes!)
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

#Region " Multipliers "

        Private Sub OnSetMultiplier(sender As Object, e As System.EventArgs) Handles m_rbOpt1.Click, m_rbOpt2.Click, m_rbOpt3.Click, m_rbOpt4.Click

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
        ''' <param name="shapeType"></param>
        ''' <param name="bConsumers"></param>
        ''' -------------------------------------------------------------------
        Private Sub Init(ByVal uic As cUIContext,
                         ByVal editMode As eEditMode,
                         ByVal shapeType As eShapeCategoryTypes,
                         ByVal bConsumers As eGroupFilter)

            Me.InitializeComponent()
            Me.m_uic = uic

            ' Get the Prey - Pred interaction manager
            Me.m_InteractionManager = Me.m_uic.Core.MediatedInteractionManager

            Me.m_editMode = editMode
            Me.m_shapeMode = shapeType
            Me.m_groupfilter = bConsumers

            ' Set title
            Select Case Me.m_shapeMode
                Case eShapeCategoryTypes.Forcing
                    Me.Text = My.Resources.ECOSIM_CAPTION_APPLYFF
                Case eShapeCategoryTypes.Mediation
                    Me.Text = My.Resources.ECOSIM_CAPTION_APPLYMED
                Case Else
                    Debug.Assert(False, cStringUtils.Localize("Mode {0} not supported by dialog", Me.m_shapeMode.ToString()))
            End Select

            For Each shape As cForcingFunction In Me.m_uic.Core.ForcingShapeManager
                If (Me.IsAllowedShape(shape)) Then
                    Me.m_lFFs.Add(shape)
                End If
            Next
            For Each shape As cForcingFunction In Me.m_uic.Core.MediationShapeManager
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

            If (m_editMode = eEditMode.PredPrey) Then

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
            If (TypeOf shape Is cMediationBaseFunction) Then
                Return (Me.m_shapeMode = eShapeCategoryTypes.Mediation)
            Else
                Return (Me.m_shapeMode = eShapeCategoryTypes.Forcing)
            End If
        End Function

        Private Sub ConfigureApplicationControls()

            If Me.m_groupfilter = eGroupFilter.Consumer Then
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt1, eForcingFunctionApplication.SearchRate)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt2, eForcingFunctionApplication.Vulnerability)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt3, eForcingFunctionApplication.ArenaArea)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt4, eForcingFunctionApplication.VulAndArea)
            ElseIf Me.m_groupfilter = eGroupFilter.Producer Then
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt1, eForcingFunctionApplication.ProductionRate)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt2, eForcingFunctionApplication.NotSet)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt3, eForcingFunctionApplication.NotSet)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt4, eForcingFunctionApplication.NotSet)
            ElseIf Me.m_groupfilter = eGroupFilter.Detritus Then
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt1, eForcingFunctionApplication.Import)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt2, eForcingFunctionApplication.NotSet)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt3, eForcingFunctionApplication.NotSet)
                Me.ConfigureApplicationRadioButton(Me.m_rbOpt4, eForcingFunctionApplication.NotSet)
            Else
                Debug.Assert(False, "Dialog not properly invoked")
            End If

            ' Yo
            Me.m_appl = DirectCast(Me.m_rbOpt1.Tag, eForcingFunctionApplication)
            Me.m_rbOpt1.Checked = True

        End Sub

        Private Sub ConfigureApplicationRadioButton(ByVal rb As RadioButton, ByVal tag As eForcingFunctionApplication)

            Dim fmt As New cFFApplicationTargetTypeFormatter()
            rb.Text = fmt.GetDescriptor(tag)
            rb.Tag = tag
            rb.Visible = (tag <> eForcingFunctionApplication.NotSet)

        End Sub

#End Region ' Private methods

    End Class

End Namespace
