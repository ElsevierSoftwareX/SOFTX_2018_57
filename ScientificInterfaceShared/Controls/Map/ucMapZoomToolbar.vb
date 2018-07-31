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

Imports System.Drawing.Imaging
Imports EwECore
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Controls.Map

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Toolbar that provides a user interface for synchronized zooming and 
    ''' stretching of one or more <see cref="ucMapZoom">zoom map controls</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucMapZoomToolbar
        Implements IUIElement

#Region " Private vars "

        ''' <summary>Flag to prevent looped updates.</summary>
        Private m_bInUpdate As Boolean = False

        ''' <summary>List of attached zoom maps that need to be synchronized.</summary>
        Private m_lZoomContainers As New List(Of ucMapZoom)
        ''' <summary>List of attached layers.</summary>
        Private m_lLayers As New List(Of cDisplayLayer)

#End Region ' Private vars

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                ' Just to be sure
                For Each zc As ucMapZoom In Me.m_lZoomContainers.ToArray
                    Me.RemoveZoomContainer(zc)
                Next

                Debug.Assert(Me.m_lLayers.Count = 0)
                Debug.Assert(Me.m_lZoomContainers.Count = 0)

                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Construction / destruction

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a zoom container to the toolbar.
        ''' </summary>
        ''' <param name="zoomContainer">A <see cref="ucMapZoom">zoom container</see> 
        ''' that this toolbar needs to manage.</param>
        ''' -------------------------------------------------------------------
        Public Sub AddZoomContainer(ByVal zoomContainer As ucMapZoom)

            Debug.Assert(Not Me.m_lZoomContainers.Contains(zoomContainer))

            AddHandler zoomContainer.MouseWheel, AddressOf OnMapMousewheel
            AddHandler zoomContainer.OnPositionChanged, AddressOf OnMapPositionChanged

            Me.m_lZoomContainers.Add(zoomContainer)
            ' All all existing layers manually - 'cause we may have missed addition events
            For Each l As cDisplayLayer In zoomContainer.Map.Layers
                Me.m_lLayers.Add(l)
            Next

            AddHandler zoomContainer.Map.LayerAdded, AddressOf OnMapLayerAdded
            AddHandler zoomContainer.Map.LayerRemoved, AddressOf OnMapLayerRemoved

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Remove a zoom container from the toolbar.
        ''' </summary>
        ''' <param name="zoomContainer">A <see cref="ucMapZoom">zoom container</see> 
        ''' that this toolbar no longer needs to manage.</param>
        ''' -----------------------------------------------------------------------
        Public Sub RemoveZoomContainer(ByVal zoomContainer As ucMapZoom)

            Debug.Assert(Me.m_lZoomContainers.Contains(zoomContainer))

            RemoveHandler zoomContainer.Map.LayerAdded, AddressOf OnMapLayerAdded
            RemoveHandler zoomContainer.Map.LayerRemoved, AddressOf OnMapLayerRemoved

            RemoveHandler zoomContainer.MouseWheel, AddressOf OnMapMousewheel
            RemoveHandler zoomContainer.OnPositionChanged, AddressOf OnMapPositionChanged
            Me.m_lZoomContainers.Remove(zoomContainer)

            ' Remove all layers manually - 'cause we're going to miss removal events
            For Each l As cDisplayLayer In zoomContainer.Map.Layers
                Me.m_lLayers.Remove(l)
            Next

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get all <see cref="ucMapZoom">zoom containers</see> that are
        ''' <see cref="AddZoomContainer">added</see> to this control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property ZoomContainers As ucMapZoom()
            Get
                Return Me.m_lZoomContainers.ToArray
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get all layers that this control has pilfered from its registered
        ''' <see cref="ZoomContainers">containers</see>. The control tracks the 
        ''' layers to provide layer data export and import interface elements.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Layers As cDisplayLayer()
            Get
                Return Me.m_lLayers.ToArray
            End Get
        End Property

        Public ReadOnly Property Toolstrip As cEwEToolstrip
            Get
                Return Me.m_tsZoom
            End Get
        End Property

#End Region ' Public access

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return
            ' NOP

        End Sub

#End Region ' Overrides

#Region " Child control events "

#Region " Zoom controls "

        Private Sub OnZoomIn(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbZoomIn.Click, m_tsmiZoomIn.Click
            Me.Zoom(ucMapZoom.eZoomTypes.ZoomIn, False)
            Me.UpdateControls()
        End Sub

        Private Sub OnZoomOut(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbZoomOut.Click, m_tsmiZoomOut.Click
            Me.Zoom(ucMapZoom.eZoomTypes.ZoomOut, False)
            Me.UpdateControls()
        End Sub

        Private Sub OnZoomReset(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbZoomReset.Click, m_tsmiZoomReset.Click
            Me.Zoom(ucMapZoom.eZoomTypes.ZoomReset, False)
            Me.UpdateControls()
        End Sub

        Private Sub OnMapMousewheel(ByVal sender As Object, ByVal e As MouseEventArgs)
            If (e.Delta > 0) Then
                Me.Zoom(ucMapZoom.eZoomTypes.ZoomIn, True)
            Else
                Me.Zoom(ucMapZoom.eZoomTypes.ZoomOut, True)
            End If
            Me.UpdateControls()
        End Sub

        Private Sub OnMapPositionChanged(ByVal sender As ucMapZoom)
            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            For Each ctrlZoom As ucMapZoom In Me.m_lZoomContainers
                If Not ReferenceEquals(ctrlZoom, sender) Then
                    ctrlZoom.UpdatePosition(sender)
                End If
            Next
            Me.m_bInUpdate = False
        End Sub

#End Region ' Zoom controls

#Region " Save "

        Private Sub m_tsbSaveImage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbSaveImage.Click

            If (Me.UIContext Is Nothing) Then Return

            Dim format As ImageFormat = ImageFormat.Bmp
            Dim core As cCore = Me.UIContext.Core
            Dim model As cEwEModel = core.EwEModel
            Dim scenario As cEcospaceScenario = core.EcospaceScenarios(core.ActiveEcospaceScenarioIndex)
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim strFileName As String = ""

            cmdFS.Invoke(cFileUtils.ToValidFileName(cStringUtils.Localize("{0}_{1}", model.Name, scenario.Name), False), SharedResources.FILEFILTER_IMAGE)
            If cmdFS.Result = System.Windows.Forms.DialogResult.OK Then

                Select Case cmdFS.FilterIndex
                    Case 0, 1
                        format = ImageFormat.Bmp
                    Case 2
                        format = ImageFormat.Jpeg
                    Case 3
                        format = ImageFormat.Gif
                    Case 4
                        format = ImageFormat.Png
                    Case 5
                        format = ImageFormat.Tiff
                    Case Else
                        Debug.Assert(False)
                End Select

                For Each ctrlZoom As ucMapZoom In Me.m_lZoomContainers
                    strFileName = cStringUtils.Localize("{0}-{1}", cmdFS.FileName, ctrlZoom.Map.Text)
                    ctrlZoom.Map.SaveToBitmap(cmdFS.FileName, format)
                Next
            End If

        End Sub

#End Region ' Save

#Region " Map events "

        Private Sub OnMapLayerAdded(sender As ucMap, layer As cDisplayLayer)
            Me.m_lLayers.Add(layer)
            AddHandler layer.LayerChanged, AddressOf OnLayerChanged
        End Sub

        Private Sub OnMapLayerRemoved(sender As ucMap, layer As cDisplayLayer)
            RemoveHandler layer.LayerChanged, AddressOf OnLayerChanged
            Me.m_lLayers.Remove(layer)
        End Sub

        Private Sub OnLayerChanged(layer As cDisplayLayer, cf As cDisplayLayer.eChangeFlags)
            Try
                Me.UpdateControls()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Map events

#End Region ' Child control events

#Region " Internal bits "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set the map <see cref="ucMapZoom.eZoomTypes">Zoom level</see> for displaying the map.
        ''' </summary>
        ''' <param name="zoomType">The <see cref="ucMapZoom.eZoomTypes">Zoom level</see> to use.</param>
        ''' -----------------------------------------------------------------------
        Private Sub Zoom(ByVal zoomType As ucMapZoom.eZoomTypes, bZoomToCursor As Boolean)

            ' Apply
            For Each ctrlZoom As ucMapZoom In Me.m_lZoomContainers
                'If (sZoomCenterX >= 0 And sZoomCenterY >= 0) Then
                '    ctrlZoom.ZoomLocation = New PointF(sZoomCenterX, sZoomCenterY)
                'End If

                Select Case zoomType
                    Case ucMapZoom.eZoomTypes.ZoomIn
                        ' Increase zoom rate to next increment
                        ctrlZoom.ZoomScale(bZoomToCursor) *= 1.5!
                    Case ucMapZoom.eZoomTypes.ZoomOut
                        ' Decrease zoom rate to prev increment
                        ctrlZoom.ZoomScale(bZoomToCursor) /= 1.5!
                    Case ucMapZoom.eZoomTypes.ZoomReset
                        ' Zoom to 100%
                        ctrlZoom.ZoomScale = 1.0!
                End Select
            Next

        End Sub

        Private Sub UpdateControls()

            If (Me.IsDisposed) Then Return

            ' NOP

        End Sub

#End Region ' Internal bits

    End Class

End Namespace
