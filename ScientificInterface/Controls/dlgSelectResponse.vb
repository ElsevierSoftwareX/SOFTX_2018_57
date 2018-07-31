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

Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports EwECore
Imports ScientificInterface.Other
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared

#End Region ' Imports

<Obsolete("Use dlgSelectCapacityResponse instead")>
Public Class dlgSelectResponse

    Private m_shapeManager As cBaseShapeManager
    Private m_lFFs As New List(Of cForcingFunction)
    Private m_driver As EwECore.IEnviroInputData
    Private m_shapeGUI As cShapeGUIHandler
    Private m_iSelGrp As Integer = cCore.NULL_VALUE
    Private m_iSelDriver As Integer = cCore.NULL_VALUE

    ''' <summary>Small thumbnails</summary>
    Private m_ilSmall As New ImageList()
    ''' <summary>Large thumbnails</summary>
    Private m_ilLarge As New ImageList()

    Private m_nGroups As Integer = 0
    Private m_seltype As eEnvironmentalResponseSelectionType = eEnvironmentalResponseSelectionType.DriverGroup
    Private m_drivermanager As IEnvironmentalResponseManager = Nothing

    Private m_bEcosim As Boolean = False
    Private m_bEcospace As Boolean = False

#Region " Construction "

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="uic">UI context to use.</param>
    ''' <param name="responseManager">Manager providing available environmental response functions.</param>
    ''' <param name="driverManager">Manager providing available environmental response drivers.</param>
    ''' <param name="iDriver">Index of selected driver in the <paramref name="driverManager">driver manager</paramref>.</param>
    ''' <param name="iSelGroup"></param>
    ''' <param name="selection">Flag indicating <see cref="eEnvironmentalResponseSelectionType">how the dialog was invoked</see>.</param>
    Public Sub New(ByVal uic As cUIContext,
                   ByVal responseManager As cBaseShapeManager,
                   ByVal driverManager As IEnvironmentalResponseManager,
                   ByVal iDriver As Integer,
                   ByVal iSelGroup As Integer,
                   ByVal selection As eEnvironmentalResponseSelectionType)

        Me.UIContext = uic
        Me.m_seltype = selection
        Me.m_shapeManager = responseManager
        Me.m_drivermanager = driverManager
        Me.m_shapeGUI = cShapeGUIHandler.GetShapeUIHandler(Me.m_shapeManager.DataType, uic)

        Me.m_iSelDriver = iDriver
        Me.m_iSelGrp = iSelGroup

        Me.m_bEcosim = (TypeOf driverManager Is cEcosimEnviroResponseManager)
        Me.m_bEcospace = (TypeOf driverManager Is cEcospaceEnviroResponseManager)
        ' One or the other needs to be set
        Debug.Assert(Me.m_bEcosim <> Me.m_bEcospace)

        Me.InitializeComponent()
        Me.Init()

    End Sub

#End Region ' Construction

#Region " Overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        Me.m_tslbFilter.Image = SharedResources.FilterHS

        Dim p As New cSettingsParser(Me.Settings)
        Me.m_tstbFilter.Text = p("filter", "")
        Me.m_tsbnCaseSensitive.Checked = (p("casesensitive") = "1")

        Me.LoadAvailableShapes()
        Me.LoadAppliedShapes()

    End Sub

    Protected Overrides Sub OnClosing(e As System.ComponentModel.CancelEventArgs)

        Dim p As New cSettingsParser()
        p("filter") = Me.m_tstbFilter.Text
        p("casesensitive") = If(Me.m_tsbnCaseSensitive.Checked, "1", "0")
        Me.Settings = p
        MyBase.OnClosing(e)

    End Sub

#End Region ' Overrides

#Region " Control Event handlers "

    Private Sub OnFilterChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_tstbFilter.TextChanged

        Try
            Me.LoadAvailableShapes()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnCaseSensitiveFilterChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnCaseSensitive.Click

        Try
            If Not String.IsNullOrWhiteSpace(Me.m_tstbFilter.Text) Then
                Me.LoadAvailableShapes()
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnAdd(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnAdd.Click, m_lvAllShapes.DoubleClick
        Try
            Me.AddShapes()
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnRemove(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnRemove.Click, m_lvAppliedShapes.DoubleClick
        Try
            Me.RemoveShapes()
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnAppliedShapesSelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_lvAppliedShapes.SelectedIndexChanged
        Try
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnAvailableShapesSelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_lvAllShapes.SelectedIndexChanged
        Try
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.UpdateSelectedResponseDriver()
        Me.Close()
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

#End Region ' Control Event handlers

#Region " Private methods "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Populate the dialog.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub Init()

        If Me.m_iSelDriver > 0 Then
            Me.m_driver = Me.m_drivermanager.EnviroData(Me.m_iSelDriver)
        End If

        ' Get the available shapes that can be applied
        For Each shape As cForcingFunction In Me.m_shapeManager
            Me.m_lFFs.Add(shape)
        Next

        ' Generate thumbnails from shapes
        Me.m_ilSmall.ImageSize = New Size(SmallIconSize, SmallIconSize)
        Me.GenerateShapeThumbnails(Me.m_ilSmall, SmallIconSize)

        Me.m_ilLarge.ImageSize = New Size(LargeIconSize, LargeIconSize)
        Me.GenerateShapeThumbnails(Me.m_ilLarge, LargeIconSize)

        Me.m_lvAllShapes.LargeImageList = Me.m_ilLarge
        Me.m_lvAllShapes.SmallImageList = Me.m_ilSmall

        Me.m_lvAppliedShapes.LargeImageList = Me.m_ilLarge
        Me.m_lvAppliedShapes.SmallImageList = Me.m_ilSmall

        Me.m_nGroups = Me.UIContext.Core.nGroups

    End Sub

    Private ReadOnly Property LargeIconSize() As Integer
        Get
            Debug.Assert(Me.UIContext.StyleGuide IsNot Nothing)
            Return CInt(Me.UIContext.StyleGuide.ThumbnailSize)
        End Get
    End Property

    Private ReadOnly Property SmallIconSize() As Integer
        Get
            Debug.Assert(Me.UIContext.StyleGuide IsNot Nothing)
            Return CInt(Math.Ceiling(Me.UIContext.StyleGuide.ThumbnailSize / 3))
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
        Dim bFound As Boolean = False

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

            ' Not found
            If (Not bFound) Then
                'Only one shape can be applied at a time for Response functions
                Me.m_lvAppliedShapes.Items.Clear()

                itemSrc = New ListViewItem(String.Format(SharedResources.GENERIC_LABEL_INDEXED, shapeSelected.Index, shapeSelected.Name))
                itemSrc.ImageIndex = Me.m_lFFs.IndexOf(shapeSelected)
                itemSrc.Tag = shapeSelected

                Me.m_lvAppliedShapes.Items.Add(itemSrc)
                Me.m_lvAppliedShapes.Items(0).Selected = True

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
        ' Remove all shapes
        Me.m_lvAppliedShapes.Items.Clear()
        ' Yoho
        Me.UpdateControls()
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Limit user interactions.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Protected Overrides Sub UpdateControls()

        ' Can add only one shape
        Me.m_btnAdd.Enabled = (Me.m_lvAllShapes.SelectedItems.Count = 1)
        ' Can only remove selected shape(s)
        Me.m_btnRemove.Enabled = (Me.m_lvAppliedShapes.SelectedItems.Count > 0)

        ' Can OK on only one or less applied shape
        Me.OK_Button.Enabled = (Me.m_lvAppliedShapes.Items.Count <= 1)

    End Sub

    Private Sub UpdateAppliedShape(ByVal item As ListViewItem, ByVal appl As eForcingFunctionApplication)

        ' Hmm, may not be accurate
        Dim fmt As New cFFApplicationTargetTypeFormatter()
        Dim shape As cForcingFunction = Me.Shape(item)

        item.SubItems(1).Text = fmt.GetDescriptor(appl)
        item.SubItems(1).Tag = appl

    End Sub

    Private Sub GenerateShapeThumbnails(ByVal Icons As ImageList, ByVal IconSize As Integer)

        Dim xMax As Integer = Me.m_shapeGUI.XAxisMaxValue

        ' For all selectable shapes
        For Each shape As cForcingFunction In Me.m_lFFs
            ' Create and Add the thumbnail image
            Icons.Images.Add(cShapeImage.IconImage(Me.UIContext, shape, Me.m_shapeGUI.Color, eSketchDrawModeTypes.Fill,
                                                   xMax, DirectCast(shape, cEnviroResponseFunction).YMax, False))
        Next

    End Sub

    Private Sub LoadAvailableShapes()

        Dim item As ListViewItem = Nothing
        Dim bUseShape As Boolean = True
        Dim strFilter As String = Me.m_tstbFilter.Text
        Dim i As Integer = 0

        Me.m_lvAllShapes.Items.Clear()

        For Each ff As cForcingFunction In Me.m_lFFs

            If Not String.IsNullOrWhiteSpace(strFilter) Then
                If (Me.m_tsbnCaseSensitive.Checked) Then
                    bUseShape = (ff.Name.IndexOf(strFilter, StringComparison.CurrentCulture) > -1)
                Else
                    bUseShape = (ff.Name.IndexOf(strFilter, StringComparison.CurrentCultureIgnoreCase) > -1)
                End If
            Else
                bUseShape = True
            End If

            If (bUseShape) Then
                item = New ListViewItem(String.Format(SharedResources.GENERIC_LABEL_INDEXED, ff.Index, ff.Name))
                item.ImageIndex = Me.m_lFFs.IndexOf(ff)
                item.Tag = ff
                Me.m_lvAllShapes.Items.Add(item)
                i += 1
            End If
        Next

        If Me.m_lvAllShapes.Items.Count > 0 Then
            Me.m_lvAllShapes.Items(0).Selected = True
        End If

        Me.UpdateControls()

    End Sub

    Private Sub LoadAppliedShapes()

        Try
            Dim isp As Integer = 0
            Dim lShapes As New List(Of Integer)

            Me.m_lvAppliedShapes.Items.Clear()

            'Only populate the selected shapes if the user selected a cell
            'If it's a row or col then there is potentially more than one shape selected
            If Me.m_seltype = eEnvironmentalResponseSelectionType.DriverGroup Then

                isp = Me.m_driver.ResponseIndexForGroup(Me.m_iSelGrp)
                If isp < 1 Then
                    'No Shape selected for this Map/Group
                    Exit Sub
                End If

                Me.AddShapeToApplied(isp)

            ElseIf Me.m_seltype = eEnvironmentalResponseSelectionType.Driver Then

                For igrp As Integer = 1 To Me.m_nGroups
                    isp = Me.m_driver.ResponseIndexForGroup(igrp)
                    If (isp > 0) And (Not lShapes.Contains(isp)) Then
                        Me.AddShapeToApplied(isp)
                        lShapes.Add(isp)
                    End If
                Next

            End If


        Catch ex As Exception

        End Try

    End Sub

    Private Sub AddShapeToApplied(ByVal isp As Integer)

        Try

            Dim shape As cForcingFunction = Me.m_lFFs.Item(isp - 1)
            Dim item As New ListViewItem(String.Format(SharedResources.GENERIC_LABEL_INDEXED, shape.Index, shape.Name))

            item.ImageIndex = Me.m_lFFs.IndexOf(shape)
            item.Tag = shape
            Me.m_lvAppliedShapes.Items.Add(item)

            ' Me.m_lvAppliedShapes.Items(0).Selected = True
            Me.m_lvAppliedShapes.LargeImageList = Me.m_ilLarge
            Me.m_lvAppliedShapes.SmallImageList = Me.m_ilSmall

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub

    Private Function UpdateSelectedResponseDriver() As Boolean

        Dim core As cCore = Me.UIContext.Core
        Dim iSelResponseShape As Integer = Me.AppliedResponseIndex
        Dim bCanCommit As Boolean = True

        Try
            If Me.m_seltype = eEnvironmentalResponseSelectionType.DriverGroup Then
                If Me.m_iSelGrp > 0 And Me.m_iSelGrp <= Me.m_nGroups Then
                    If (Me.m_bEcospace) Then
                        bCanCommit = Me.CanCommit(core.EcospaceGroupInputs(Me.m_iSelGrp).CapacityCalculationType)
                    Else
                        bCanCommit = True
                    End If
                    If (bCanCommit) Then Me.m_driver.ResponseIndexForGroup(m_iSelGrp) = iSelResponseShape
                    Return True
                End If
            ElseIf Me.m_seltype = eEnvironmentalResponseSelectionType.Driver Then
                'Apply the same shape to all the groups of the current map
                For igrp As Integer = 1 To Me.m_nGroups
                    If (Me.m_bEcospace) Then
                        bCanCommit = Me.CanCommit(core.EcospaceGroupInputs(igrp).CapacityCalculationType)
                    Else
                        bCanCommit = True
                    End If
                    If (bCanCommit) Then Me.m_driver.ResponseIndexForGroup(igrp) = iSelResponseShape
                Next
            End If

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".UpdateSelectedResponseDriver() Exception " & ex.Message)
        End Try

        Return False

    End Function

    Private Function CanCommit(t As eEcospaceCapacityCalType) As Boolean
        Return ((t And eEcospaceCapacityCalType.EnvResponses) = eEcospaceCapacityCalType.EnvResponses)
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns the index of the shape in the Applied Shapes list view control
    ''' </summary>
    ''' <returns>Index of the Applied shape, or cCore.NULL_VALUE if nothing is Applied</returns>
    ''' -------------------------------------------------------------------
    Private Function AppliedResponseIndex() As Integer
        'response index < 0 clears out the selected response index for this group
        Dim index As Integer = cCore.NULL_VALUE
        Try
            'There can only be one item in the Applied Shapes list 
            'Get the index from the shape or return the default cCore.NULL_VALUE
            Dim shape As cForcingFunction
            If Me.m_lvAppliedShapes.Items.Count > 0 Then
                shape = DirectCast(Me.m_lvAppliedShapes.Items(0).Tag, cForcingFunction)
                index = shape.Index
            End If
        Catch ex As Exception

        End Try

        Return index

    End Function

#End Region

End Class