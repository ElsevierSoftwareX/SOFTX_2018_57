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
Imports ZedGraph
Imports EwEUtils.Utilities
Imports ScientificInterface

#End Region ' Imports

' ToDo: In order to enable in-place shape editing, all shapes need to be duplicated,
'       and on OK, changed shapes will need to be applied. That is a big change

''' ---------------------------------------------------------------------------
''' <summary>
''' Dialog to select an environmental response for a given driver and group(s).
''' </summary>
''' <remarks>
''' This dialog has been reworked with EwE 6.6, and can no longer operate on
''' more than one driver.
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class dlgSelectEnvironmentalResponse

#Region " Private vars "

    Private Enum eModelType As Integer
        NotSet = 0
        Ecosim
        Ecospace
    End Enum

    Private m_shapeManager As cBaseShapeManager
    Private m_lFFs As New List(Of cEnviroResponseFunction)
    Private m_driver As EwECore.IEnviroInputData
    Private m_shapeGUI As cShapeGUIHandler
    Private m_iSelGrp As Integer = cCore.NULL_VALUE

    Private m_seltype As eEnvironmentalResponseSelectionType = eEnvironmentalResponseSelectionType.DriverGroup
    Private m_drivermanager As IEnvironmentalResponseManager = Nothing

#End Region ' Private vars

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

        Me.InitializeComponent()

        Me.m_changeshape.UIContext = uic

        If (TypeOf driverManager Is cEcosimEnviroResponseManager) Then
            Me.Modeltype = eModelType.Ecosim
        ElseIf (TypeOf driverManager Is cEcospaceEnviroResponseManager) Then
            Me.Modeltype = eModelType.Ecospace
        End If

        ' Sanity checks
        Debug.Assert(Me.Modeltype <> eModelType.NotSet)
        Debug.Assert(iDriver > 0)

        Me.m_seltype = selection
        Me.m_shapeManager = responseManager
        Me.m_drivermanager = driverManager
        Me.m_shapeGUI = cShapeGUIHandler.GetShapeUIHandler(Me.m_shapeManager.DataType, uic)
        Me.m_driver = Me.m_drivermanager.EnviroData(iDriver)
        Me.m_iSelGrp = iSelGroup

        Debug.Assert(Me.m_driver IsNot Nothing)

        Me.Text = cStringUtils.Localize(Me.Text, Me.m_driver.Name)
        Me.m_changeshape.Visible = False
        Me.m_graph.ShowShapeControls = False

    End Sub

#End Region ' Construction

#Region " Overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

        Me.Visible = False
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        Me.m_tslbFilter.Image = SharedResources.FilterHS

        ' Get the available shapes that can be applied
        For Each shape As cEnviroResponseFunction In Me.m_shapeManager
            ' ToDo: work on cloned shape
            'Me.m_lFFs.Add(DirectCast(shape.Clone(), cEnviroResponseFunction))
            Me.m_lFFs.Add(shape)
        Next

        ' Generate thumbnails from shapes
        Dim size As Integer = LargeIconSize
        Dim images As New ImageList()
        images.ImageSize = New Size(size, size)
        Me.GenerateShapeThumbnails(images)
        Me.m_lvAllShapes.SmallImageList = images
        Me.m_lvAllShapes.LargeImageList = images

        Me.m_graph.Init(Me.UIContext)
        Me.m_graph.Driver = Me.m_driver

        Dim p As New cSettingsParser(Me.Settings)
        Me.m_tstbFilter.Text = p("filter", "")
        Me.m_tsbnCaseSensitive.Checked = (p("casesensitive") = "1")

        Me.LoadAvailableShapes()
        Me.LoadAppliedShapes()

        Me.CenterToScreen()
        Me.Visible = True

    End Sub

    Protected Overrides Sub OnClosing(e As System.ComponentModel.CancelEventArgs)

        Dim p As New cSettingsParser()
        p("filter") = Me.m_tstbFilter.Text
        p("casesensitive") = If(Me.m_tsbnCaseSensitive.Checked, "1", "0")
        Me.Settings = p
        MyBase.OnClosing(e)

    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)

        Me.m_graph.Dispose()
        MyBase.OnFormClosed(e)

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
        Me.m_btnRemove.Enabled = (Me.Shape IsNot Nothing)

        Me.m_btnChangeShape.Enabled = (Me.Shape IsNot Nothing)

        If (Me.Shape Is Nothing) And Me.m_changeshape.Visible Then Me.m_changeshape.Visible = False
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
            Me.SelectShape()
            Me.UpdateControls()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnRemove(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_btnRemove.Click
        Try
            Me.RemoveShapes()
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

    Private Sub OnShapeReshaped() Handles m_changeshape.OnShapeFunctionChanged
        ' Update shape
        Dim fn As IShapeFunction = Me.m_changeshape.SelectedShapeFunction
        If (fn IsNot Nothing And Me.Shape IsNot Nothing) Then
            fn.Apply(Me.Shape)
        End If
        ' Refresh thumbnail
        Me.UpdateShapeImage(Me.m_lvAllShapes.LargeImageList, Me.Shape)
        Me.m_graph.Shape = Me.Shape
        ' Totally redraw
        Me.Invalidate()
    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles OK_Button.Click

        Me.DialogResult = DialogResult.OK

        If Me.UpdateSelectedResponseDriver() Then
            ' ToDo: update original shapes from changes made to the cloned shapes
            'For i As Integer = 0 To Me.m_lFFs.Count - 1
            '    Dim src As cEnviroResponseFunction = Me.m_lFFs(i)
            '    Dim tgt As cForcingFunction = Me.m_shapeManager.Item(i)
            '    tgt.LockUpdates()
            '    tgt.ShapeData = src.ShapeData
            '    tgt.ShapeFunctionType = src.ShapeFunctionType
            '    tgt.ShapeFunctionParameters = src.ShapeFunctionParameters
            '    tgt.UnlockUpdates()
            'Next
            Me.m_shapeManager.Update()
        End If
        Me.Close()
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

#End Region ' Control Event handlers

#Region " Private methods "

    Private ReadOnly Property LargeIconSize() As Integer
        Get
            Debug.Assert(Me.UIContext.StyleGuide IsNot Nothing)
            Return CInt(Me.UIContext.StyleGuide.ThumbnailSize)
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the selected shape for a list view item.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Property Shape(ByVal lvi As ListViewItem) As cEnviroResponseFunction
        Get
            Return DirectCast(lvi.Tag, cEnviroResponseFunction)
        End Get
        Set(ByVal value As cEnviroResponseFunction)
            lvi.Tag = value
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Add avaliable shapes to the applications.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub SelectShape()

        For Each itemSrc As ListViewItem In Me.m_lvAllShapes.SelectedItems
            Me.Shape = Shape(itemSrc)
            Exit For
        Next

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Remove applications.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub RemoveShapes()
        Me.Shape = Nothing
    End Sub

    Private Property Shape As cEnviroResponseFunction
        Get
            Return Me.m_graph.Shape
        End Get
        Set(value As cEnviroResponseFunction)
            Me.m_graph.Shape = value
            Me.m_changeshape.Shape = value
            Me.UpdateControls()
        End Set
    End Property

    Private ReadOnly Property Modeltype As eModelType

    Private Sub GenerateShapeThumbnails(ByVal il As ImageList)

        Dim xMax As Integer = Me.m_shapeGUI.XAxisMaxValue

        ' For all selectable shapes
        For Each shape As cForcingFunction In Me.m_lFFs
            ' Create and add the thumbnail image
            il.Images.Add(cShapeImage.IconImage(Me.UIContext, shape, Me.m_shapeGUI.Color, eSketchDrawModeTypes.Fill, xMax, DirectCast(shape, cEnviroResponseFunction).YMax, False))
        Next

    End Sub

    Private Sub UpdateShapeImage(ByVal il As ImageList, shape As cEnviroResponseFunction)
        Dim iShape As Integer = Me.m_lFFs.IndexOf(shape)
        Dim xMax As Integer = Me.m_shapeGUI.XAxisMaxValue
        If (iShape < 0) Then Return
        il.Images.Item(iShape) = cShapeImage.IconImage(Me.UIContext, shape, Me.m_shapeGUI.Color, eSketchDrawModeTypes.Fill, xMax, DirectCast(shape, cEnviroResponseFunction).YMax, False)
        Me.m_lvAllShapes.Invalidate()
    End Sub

    Private Sub LoadAvailableShapes()

        Dim item As ListViewItem = Nothing
        Dim bUseShape As Boolean = True
        Dim strFilter As String = Me.m_tstbFilter.Text
        Dim i As Integer = 0

        Me.m_lvAllShapes.Items.Clear()

        For Each ff As cEnviroResponseFunction In Me.m_lFFs

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

        Dim shape As cEnviroResponseFunction = Nothing
        Dim lShapes As New List(Of cEnviroResponseFunction)

        Dim isp As Integer = 0

        'Only populate the selected shapes if the user selected a cell
        'If it's a row or col then there is potentially more than one shape selected
        If Me.m_seltype = eEnvironmentalResponseSelectionType.DriverGroup Then

            isp = Me.m_driver.ResponseIndexForGroup(Me.m_iSelGrp)
            If isp < 1 Then
                'No Shape selected for this Map/Group
                Return
            End If

            lShapes.Add(CType(Me.m_shapeManager.Item(isp - 1), cEnviroResponseFunction))

        ElseIf Me.m_seltype = eEnvironmentalResponseSelectionType.Driver Then

            For igrp As Integer = 1 To Me.UIContext.Core.nGroups
                isp = Me.m_driver.ResponseIndexForGroup(igrp)
                If (isp > 0) Then
                    shape = CType(Me.m_shapeManager.Item(isp - 1), cEnviroResponseFunction)
                    If Not lShapes.Contains(shape) Then lShapes.Add(shape)
                End If
            Next

        End If

        ' Not very refined, but hey...
        If (lShapes.Count > 0) Then Me.Shape = lShapes(0)

    End Sub

    Private Function UpdateSelectedResponseDriver() As Boolean

        Dim core As cCore = Me.UIContext.Core
        Dim iSelResponseShape As Integer = Me.AppliedResponseIndex
        Dim bCanCommit As Boolean = True

        Try
            If Me.m_seltype = eEnvironmentalResponseSelectionType.DriverGroup Then
                If Me.m_iSelGrp > 0 And Me.m_iSelGrp <= Me.UIContext.Core.nGroups Then
                    If (Me.Modeltype = eModelType.Ecospace) Then
                        bCanCommit = Me.CanCommit(core.EcospaceGroupInputs(Me.m_iSelGrp).CapacityCalculationType)
                    Else
                        bCanCommit = True
                    End If
                    If (bCanCommit) Then Me.m_driver.ResponseIndexForGroup(m_iSelGrp) = iSelResponseShape
                    Return True
                End If
            ElseIf Me.m_seltype = eEnvironmentalResponseSelectionType.Driver Then
                'Apply the same shape to all the groups of the current map
                For igrp As Integer = 1 To Me.UIContext.Core.nGroups
                    If (Me.Modeltype = eModelType.Ecospace) Then
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
        If (Me.Shape Is Nothing) Then Return cCore.NULL_VALUE
        Return Me.Shape.Index
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles m_btnChangeShape.Click
        Me.m_changeshape.Visible = Not Me.m_changeshape.Visible
    End Sub

#End Region

End Class