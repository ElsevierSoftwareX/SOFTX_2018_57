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

Option Explicit On
Option Strict On

Imports System.ComponentModel
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Properties
Imports System.Reflection
Imports System.Security.Permissions
Imports EwEUtils.SystemUtilities
Imports EwECore.Style
Imports EwEUtils.Utilities

#End Region ' Imports


#Const DRAW_THREADED = 0

Namespace Controls.Map

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Control that provides an interface to a series core data map layers.
    ''' </summary>
    ''' <remarks>
    ''' To provide zoom functionality, use <see cref="ucMapZoom">ucMapZoom</see>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class ucMap
        Implements IUIElement

        ''' <summary>UI context to work against.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>The bitmap to draw on.</summary>
        Private m_bmp As Bitmap = Nothing
        ''' <summary>Map title.</summary>
        Private m_strTitle As String = ""
        ''' <summary>List of layers.</summary>
        Private m_layers As New List(Of cDisplayLayer)
        ''' <summary>Selected layer</summary>
        Private m_layerSelected As cDisplayLayer = Nothing
        ''' <summary>States whether map must be refreshed</summary>
        Private m_bRefreshMap As Boolean = False

        Private m_cmdPropSelect As cPropertySelectionCommand = Nothing

        Public Sub New()

            Me.InitializeComponent()

            '' Enable double buffering
            'Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            'Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            'Me.SetStyle(ControlStyles.ResizeRedraw, True)
            'Me.SetStyle(ControlStyles.UserPaint, True)

            Me.BackColor = Color.White
            Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle

        End Sub

        ''' <inheritdocs cref="IUIElement.UIContext"/>
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal uic As cUIContext)
                If (Me.m_uic IsNot Nothing) Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
                Me.m_uic = uic
                If (Me.m_uic IsNot Nothing) Then
                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
                Me.Clear()
            End Set
        End Property

#Region " Public interfaces "

        Public Function SaveToBitmap(ByVal strFileName As String, ByVal format As System.Drawing.Imaging.ImageFormat) As Boolean

            Dim bm As cEcospaceBasemap = Me.Basemap
            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Dim szCellSize As SizeF = Me.GetCellSize()
            Dim strFilenameLegend As String = ""

            Try
                Dim bmp As Bitmap = sg.GetImage(CInt(Me.NumCols * szCellSize.Width), CInt(Me.NumRows * szCellSize.Height), format, strFileName)
                Me.UpdateMap(bmp, New Point(1, 1), New Point(Me.Basemap.InCol, Me.Basemap.InRow))
                bmp.Save(strFileName, format)

                Dim lgd As cLegend = cLegend.FromMap(Me)
                Dim strExt As String = Path.GetExtension(strFileName)

                strFilenameLegend = Path.Combine(Path.GetDirectoryName(strFileName), Path.GetFileNameWithoutExtension(strFileName) & "_legend" & strExt)
                lgd.Save(strFilenameLegend, format)

                ' ToDo: globalize this
                Dim msg As New cMessage(String.Format("Map image has been saved to {0}, legend to {1}", strFileName, strFilenameLegend), _
                                        eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(strFileName)
                Me.m_uic.Core.Messages.SendMessage(msg)
            Catch ex As Exception
                cLog.Write(ex, "ucMap(" & Me.Name & ")::SaveToBitmap(" & strFileName & ")")
            End Try

            Return True

        End Function

#End Region ' Public interfaces

#Region " Public properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the map title.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True)> _
        <Category("Appearance")> _
        <Description("Title of the map to display")> _
        Public Property Title() As String
            Get
                Return Me.m_strTitle
            End Get
            Set(ByVal strTitle As String)
                Me.m_strTitle = strTitle
            End Set
        End Property

        ' ''' -------------------------------------------------------------------
        ' ''' <summary>
        ' ''' Get a legend for the current map.
        ' ''' </summary>
        ' ''' -------------------------------------------------------------------
        'Public ReadOnly Property Legend() As cLegend
        '    Get
        '        Return cLegend.FromMap(Me, Me.m_uic)
        '    End Get
        'End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the map.
        ''' </summary>
        ''' <remarks>Redrawing the map entirely may be slow!</remarks>
        ''' -------------------------------------------------------------------
        Public Overloads Sub Refresh()
            Me.UpdateMap()
        End Sub

        Public Property Editable() As Boolean = False

        Public ReadOnly Property NumCols() As Integer
            Get
                If (Me.Basemap Is Nothing) Then Return 20
                Return Me.Basemap.InCol
            End Get
        End Property

        Public ReadOnly Property NumRows() As Integer
            Get
                If (Me.Basemap Is Nothing) Then Return 20
                Return Me.Basemap.InRow
            End Get
        End Property

        Public ReadOnly Property CellSize As Single
            Get
                If (Me.Basemap Is Nothing) Then Return 0.5
                Return Me.Basemap.CellSize
            End Get
        End Property

#End Region ' Public properties

#Region " Event handlers "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clean-up.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ucBaseMap_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles Me.Disposed
            Me.Clear()
        End Sub

#If DRAW_THREADED Then
        Private m_thread As Threading.Thread
#End If

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Paint handler; selectively redraws the bitmap.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)

            If Me.Basemap Is Nothing Then Return

            Try

                ' Needs new bitmap?
                If (Me.m_bmp Is Nothing) Then
                    ' #Yes: create new bitmap
                    Me.m_bmp = New Bitmap(Me.Width, Me.Height)
                    Me.BackgroundImage = Me.m_bmp
                    Me.m_bRefreshMap = True
                End If

                If (Me.m_bRefreshMap) Then
                    Me.m_bRefreshMap = False
                    Try
#If DRAW_THREADED Then
                        If (Me.m_thread Is Nothing) Then
                            Me.m_thread = New Threading.Thread(AddressOf RedrawMapThreaded)
                            Me.m_thread.Start()
                        End If
#Else
                        Me.UpdateMap(Me.m_bmp, New Point(1, 1), New Point(Me.Basemap.InCol, Me.Basemap.InRow))
#End If
                    Catch ex As Exception

                    End Try

                End If

                MyBase.OnPaint(e)
            Catch ex As Exception
                ResetExceptionState(Me)
            End Try

        End Sub

#If DRAW_THREADED Then
        Private Sub RedrawMapThreaded()
            Me.UpdateMap(Me.m_bmp, New Point(1, 1), New Point(Me.Basemap.InCol, Me.Basemap.InRow))
            Me.m_thread = Nothing
        End Sub
#End If

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Mouse down handler; intializes map drawing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)

            Dim bm As cEcospaceBasemap = Me.Basemap
            Dim InRow As Integer = bm.InRow
            Dim InCol As Integer = bm.InCol

            If (Me.CanEdit = False) Then Return

            Dim edt As cLayerEditor = Me.m_layerSelected.Editor

            ' It's up to the editor to start editing
            edt.ProcessMouseClick(e, Me)

            If (edt.IsEditing) Then
                Me.Capture = True
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Mouse move handler; performs a map drawing step.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)

            ' Get value in selected layer
            Dim bm As cEcospaceBasemap = Me.Basemap
            Dim l As cDisplayLayer = Me.m_layerSelected

            If (Me.CanEdit) Then
                Me.Cursor = Me.m_layerSelected.Editor.Cursor(e.Location, Me)
            Else
                Me.Cursor = Cursors.Default
            End If

            If (Me.CanEdit And Me.Capture) Then
                Me.ProcessMouseMove(e)
            End If

            Dim ptCell As Point = Me.GetCellIndex(e.Location)
            Dim pos As PointF = Me.GetLocation(e.Location)
            Dim strVal As String = ""
            Dim strFeedback As String = ""

            If (l IsNot Nothing) Then
                If (TypeOf l Is cDisplayLayerRaster) Then
                    strVal = l.Renderer.GetDisplayText(DirectCast(l, cDisplayLayerRaster).Value(ptCell.Y, ptCell.X))
                End If
            End If

            Dim strLat As String = Me.UIContext.StyleGuide.FormatNumber(pos.Y)
            Dim strLon As String = Me.UIContext.StyleGuide.FormatNumber(pos.X)
            Dim fmt As New cMapUnitFormatter()
            Dim strUnit As String = If(bm.AssumeSquareCells,
                                                     fmt.GetDescriptor(eUnitMapRefType.m, eDescriptorTypes.Symbol),
                                                     fmt.GetDescriptor(eUnitMapRefType.dd, eDescriptorTypes.Symbol))

            If Not String.IsNullOrWhiteSpace(strVal) Then
                strFeedback = String.Format(My.Resources.GENERIC_VALUE_MAPPOS_VALUE,
                                            strLon, strLat, strUnit,
                                            ptCell.Y, ptCell.X, strVal)
            Else
                strFeedback = String.Format(My.Resources.GENERIC_VALUE_MAPPOS,
                                            strLon, strLat, strUnit,
                                            ptCell.Y, ptCell.X)
            End If

            cApplicationStatusNotifier.UpdateStatus(Me.m_uic.Core, strFeedback)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Mouse up handler; finalizes map drawing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseUp(ByVal e As MouseEventArgs)

            If (Me.CanEdit = False) Then Return
            If (Me.Capture = False) Then Return

            Me.m_layerSelected.Editor.ProcessMouseUp()

            ' Process pending layer changes
            For Each l As cDisplayLayer In m_layers
                If (TypeOf l Is cDisplayLayerRaster) Then
                    Dim rl As cDisplayLayerRaster = DirectCast(l, cDisplayLayerRaster)
                    If rl.IsModified Then rl.Update(cDisplayLayer.eChangeFlags.Map) : rl.IsModified = False
                End If
            Next

            Me.Capture = False

        End Sub

        Protected Overrides Sub OnMouseLeave(e As System.EventArgs)
            MyBase.OnMouseLeave(e)
            cApplicationStatusNotifier.UpdateStatus(Me.m_uic.Core, "")
        End Sub

        Protected Overrides Sub OnSizeChanged(e As System.EventArgs)
            MyBase.OnSizeChanged(e)

            If (Me.m_bmp IsNot Nothing) Then
                Me.BackgroundImage = Nothing
                Me.m_bmp.Dispose()
                Me.m_bmp = Nothing
            End If

            ' Schedule paint job
            Me.Invalidate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Layer changed event
        ''' </summary>
        ''' <param name="l">The layer that changed</param>
        ''' -------------------------------------------------------------------
        Private Sub OnLayerChanged(ByVal l As cDisplayLayer, ByVal cf As cDisplayLayer.eChangeFlags)

            ' Ignore sole descriptive layer changes
            If (cf = cDisplayLayer.eChangeFlags.Descriptive) Then Return

            ' Handle selection changes
            If ((cf And cDisplayLayer.eChangeFlags.Selected) > 0) Then
                ' Update selection
                Me.UpdateSelection(l)
            End If

            If ((cf And (cDisplayLayer.eChangeFlags.Map Or
                                 cDisplayLayer.eChangeFlags.Visibility Or
                                 cDisplayLayer.eChangeFlags.VisualStyle Or
                                 cDisplayLayer.eChangeFlags.Selected)) > 0) Then
                ' Update Map
                Me.UpdateMap()
            End If

            If ((cf And (cDisplayLayer.eChangeFlags.Editable Or cDisplayLayer.eChangeFlags.Selected)) > 0) Then
                ' Refresh edit environment
                ' Nothing to do right now...
            End If

        End Sub

        Private Sub OnStyleGuideChanged(ct As cStyleGuide.eChangeType)
            If (ct And cStyleGuide.eChangeType.Colours) > 0 Then
                Me.UpdateMap()
            End If
        End Sub

#End Region ' Event handlers

#Region " Internals "

        Public ReadOnly Property Basemap As cEcospaceBasemap
            Get
                If (Me.m_uic Is Nothing) Then Return Nothing
                Return Me.m_uic.Core.EcospaceBasemap
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Performs a draw step by updating the memory bitmap.
        ''' </summary>
        ''' <param name="e"></param>
        ''' -------------------------------------------------------------------
        Private Sub ProcessMouseMove(ByVal e As MouseEventArgs)

            If (Me.CanEdit = False) Then Return
            If (Me.Capture = False) Then Return

            Me.m_layerSelected.Editor.ProcessMouseDraw(e, Me)

        End Sub

        Public Sub UpdateMap(ByVal ptCellFrom As Point, ByVal ptCellTo As Point)
            Me.UpdateMap(Me.m_bmp, ptCellFrom, ptCellTo)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the entire map image.
        ''' </summary>
        ''' <remarks>
        ''' This will invalidate the entire map screen area.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Sub UpdateMap()

            ' Sanity check
            If Me.Basemap Is Nothing Then Return
            ' Set reminder
            Me.m_bRefreshMap = True
            ' Refresh
            Me.Invalidate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update a range of cells in the map image.
        ''' </summary>
        ''' <remarks>
        ''' This will invalidate the map screen area encompassing the range 
        ''' of indicated cells.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Sub UpdateMap(ByVal bmp As Bitmap, ByVal ptCellFrom As Point, ByVal ptCellTo As Point)

            ' Sanity check
            If Me.Basemap Is Nothing Then Return

            Dim bm As cEcospaceBasemap = Me.Basemap
            Dim g As Graphics = Graphics.FromImage(bmp)
            Dim l As cDisplayLayer = Nothing
            Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
            Dim layDepth As cEcospaceLayerDepth = Me.Basemap.LayerDepth()
            Dim layExcl As cEcospaceLayerExclusion = Me.Basemap.LayerExclusion()
            Dim szCell As SizeF = Me.GetCellSize()
            Dim ptCell As Point = Nothing
            Dim rcScreen As Rectangle = Nothing
            Dim bDrawCell As Boolean = False

            ' Calc area to invalidate
            Dim p1 As Point = Me.GetCellPos(ptCellFrom)
            Dim p2 As Point = Me.GetCellPos(ptCellTo)

            ' Sort coords
            Dim iXFrom As Integer = Math.Min(p1.X, p2.X)
            Dim iXTo As Integer = Math.Max(p1.X, p2.X)
            Dim iYFrom As Integer = Math.Min(p1.Y, p2.Y)
            Dim iYTo As Integer = Math.Max(p1.Y, p2.Y)

            ' Clear and invalidate the area
            rcScreen = New Rectangle(iXFrom, iYFrom, iXTo - iXFrom + CInt(szCell.Width), iYTo - iYFrom + CInt(szCell.Height))
            g.FillRectangle(New SolidBrush(Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND)), rcScreen)
            Me.Invalidate(rcScreen)

            ' Draw surrounding cells as well to avoid anomalies
            iXFrom = Math.Max(1, Math.Min(ptCellFrom.X, ptCellTo.X) - 1)
            iYFrom = Math.Max(1, Math.Min(ptCellFrom.Y, ptCellTo.Y) - 1)
            iXTo = Math.Min(Me.Basemap.InCol, Math.Max(ptCellFrom.X, ptCellTo.X) + 1)
            iYTo = Math.Min(Me.Basemap.InRow, Math.Max(ptCellFrom.Y, ptCellTo.Y) + 1)

            Dim ptTL As New PointF(bm.ColToLon(iXFrom), bm.RowToLat(iYFrom))
            Dim ptBR As New PointF(bm.ColToLon(iXTo + 1), bm.RowToLat(iYTo + 1))

            Dim layers As New List(Of cDisplayLayer)
            Dim displayDepth As cDisplayLayer = Nothing
            Dim dtGroup As eDataTypes = Nothing

            If (Me.m_layerSelected IsNot Nothing) Then
                If (Me.m_layerSelected.RenderMode = Definitions.eLayerRenderType.Grouped) Then
                    If (TypeOf Me.m_layerSelected Is cDisplayLayerRaster) Then
                        dtGroup = DirectCast(Me.m_layerSelected, cDisplayLayerRaster).Data.DataType
                    End If
                End If
            End If

            For Each l In Me.m_layers
                Dim bDrawLayer As Boolean = (l.Renderer.IsVisible)

                If (TypeOf l Is cDisplayLayerRaster) Then

                    Dim rl As cDisplayLayerRaster = DirectCast(l, cDisplayLayerRaster)
                    Dim dt As eDataTypes = rl.Data.DataType

                    Select Case rl.RenderMode
                        Case Definitions.eLayerRenderType.Always
                            ' NOP
                        Case Definitions.eLayerRenderType.Selected
                            bDrawLayer = bDrawLayer And rl.IsSelected()
                        Case Definitions.eLayerRenderType.Grouped
                            If (rl.Data.DataType = dtGroup) And (dtGroup <> eDataTypes.NotSet) Then
                                ' NOP
                            Else
                                bDrawLayer = False
                            End If
                    End Select

                    ' Special cases
                    If (dt = eDataTypes.EcospaceLayerExclusion And Me.UIContext.StyleGuide.ShowMapsExcludedCells) Then
                        bDrawLayer = True
                    End If

                    If (dt = eDataTypes.EcospaceLayerDepth) Then
                        displayDepth = rl
                        bDrawLayer = True
                    End If

                End If

                If (l.RenderMode = Definitions.eLayerRenderType.Always) Then
                    bDrawLayer = True
                End If

                If bDrawLayer Then layers.Add(l)
            Next

            ' Draw raster layers in reverse order
            For iLayer As Integer = layers.Count - 1 To 0 Step -1

                l = layers(iLayer)
                If (l.Renderer.IsVisible) Then

                    If (TypeOf l Is cDisplayLayerRaster) Then

                        Dim rl As cDisplayLayerRaster = DirectCast(l, cDisplayLayerRaster)
                        Dim dt As eDataTypes = rl.Data.DataType

                        If (rl.HasData) Then

                            For X As Integer = iXFrom To iXTo
                                For Y As Integer = iYFrom To iYTo

                                    ptCell = New Point(X, Y)
                                    Dim rcCell As Rectangle = Me.GetCellRect(ptCell)

                                    Select Case dt
                                        Case eDataTypes.EcospaceLayerExclusion,
                                             eDataTypes.EcospaceLayerDepth,
                                             eDataTypes.EcospaceLayerPort
                                            bDrawCell = True
                                        Case Else
                                            bDrawCell = layDepth.IsWaterCell(Y, X) And CBool(layExcl.Cell(Y, X)) = False
                                    End Select

                                    If bDrawCell Then
                                        Dim objValue As Object = rl.Value(ptCell.Y, ptCell.X)
                                        If rl.IsValue(objValue) Then
                                            ' Build style flags
                                            style = cStyleGuide.eStyleFlags.OK
                                            If l.IsSelected Or l.RenderMode = Definitions.eLayerRenderType.Always Then
                                                style = (style Or cStyleGuide.eStyleFlags.Highlight)
                                            End If
                                            ' Render cell
                                            DirectCast(l.Renderer, cRasterLayerRenderer).RenderCell(g, rcCell, rl.Data, objValue, style)
                                        End If
                                    End If

                                Next Y
                            Next X
                        End If

                    ElseIf (TypeOf l.Renderer Is cVectorLayerRenderer) Then
                        style = cStyleGuide.eStyleFlags.OK
                        If l.IsSelected Then style = (style Or cStyleGuide.eStyleFlags.Highlight)
                        DirectCast(l.Renderer, cVectorLayerRenderer).Render(g, l, rcScreen, ptTL, ptBR, style)
                    End If
                End If
            Next iLayer

            g.Dispose()

        End Sub

        Private Sub UpdateSelection(ByVal l As cDisplayLayer)

            ' Sanity check
            If Me.Basemap Is Nothing Then Return

            ' New selection?
            If l.IsSelected Then
                ' #Yes: set selected layer
                Me.m_layerSelected = l
            Else
                ' #No: current selection being cleared?
                If ReferenceEquals(Me.m_layerSelected, l) Then
                    ' #Yes: clear selection
                    Me.m_layerSelected = Nothing
                End If
            End If

        End Sub

        Private Function CanEdit() As Boolean

            If (Me.Editable = False) Then Return False
            If (Me.m_layerSelected Is Nothing) Then Return False
            If (Me.m_layerSelected.Renderer.IsVisible = False) Then Return False

            If (Me.m_layerSelected.Editor IsNot Nothing) Then
                Return Me.m_layerSelected.Editor.IsEditable
            End If

            Return False

        End Function

        <ReflectionPermission(SecurityAction.Demand, MemberAccess:=True)>
        Private Sub ResetExceptionState(ByVal control As Control)
            ' Reset exception state on drawing errors
            Dim args() As Object = {&H400000, False}
            GetType(Control).InvokeMember("SetState",
                                          BindingFlags.NonPublic Or BindingFlags.InvokeMethod Or BindingFlags.Instance,
                                          Nothing, control, args)
        End Sub

#End Region ' Internals

#Region " Layers "

        Public Event LayerAdded(sender As ucMap, layer As cDisplayLayer)
        Public Event LayerRemoved(sender As ucMap, layer As cDisplayLayer)

        Public Sub Clear()

            ' Unplug background image
            If (Me.m_bmp IsNot Nothing) Then
                Me.BackgroundImage = Nothing
                Me.m_bmp.Dispose()
                Me.m_bmp = Nothing
            End If

            ' Clean up layers to prevent dangling event handlers, which in turn keep disposed objects alive.
            Dim alayers As cDisplayLayer() = Me.m_layers.ToArray()
            For iLayer As Integer = 0 To alayers.Length - 1
                Me.RemoveLayer(alayers(iLayer))
            Next
            ' Should be neatly cleaned out
            Debug.Assert(m_layers.Count = 0)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a layer to the basemap.
        ''' </summary>
        ''' <param name="layer">The layer to add.</param>
        ''' <param name="layerPosition">The layer to add the layer before, if any.</param>
        ''' -------------------------------------------------------------------
        Public Sub AddLayer(ByVal layer As cDisplayLayer,
                            Optional ByVal layerPosition As cDisplayLayer = Nothing)

            ' Sanity check
            If (layer Is Nothing) Then Return

            If layerPosition IsNot Nothing Then
                Me.m_layers.Insert(Me.m_layers.IndexOf(layerPosition), layer)
            Else
                Me.m_layers.Add(layer)
            End If

            AddHandler layer.LayerChanged, AddressOf Me.OnLayerChanged

            ' Manually update selected state on new layers
            If layer.IsSelected Then Me.UpdateSelection(layer)

            Try
                RaiseEvent LayerAdded(Me, layer)
            Catch ex As Exception
                cLog.Write(ex, "ucMap " & Me.Name & "::AddLayer")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a layer from the basemap.
        ''' </summary>
        ''' <param name="layer">The layer to remove.</param>
        ''' -------------------------------------------------------------------
        Public Sub RemoveLayer(ByVal layer As cDisplayLayer)

            ' Sanity check
            If (layer Is Nothing) Then Return

            RemoveHandler layer.LayerChanged, AddressOf Me.OnLayerChanged

            ' Clear selection
            If ReferenceEquals(layer, Me.m_layerSelected) Then
                Me.m_layerSelected = Nothing
            End If

            Me.m_layers.Remove(layer)

            Try
                RaiseEvent LayerRemoved(Me, layer)
            Catch ex As Exception
                cLog.Write(ex, "ucMap " & Me.Name & "::RemoveLayer")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get all layers currently active in the map.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Layers() As cDisplayLayer()
            Get
                Return Me.m_layers.ToArray
            End Get
        End Property

#End Region ' Layers

#Region " Helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the width and height of a cell in pixels, as drawn in the map.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetCellSize() As SizeF
            Return New SizeF(CSng(Me.Width / Me.NumCols), CSng(Me.Height / Me.NumRows))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the cell screen rectangle of a cell, given its index.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetCellRect(ByVal ptCellIndex As Point) As Rectangle

            Dim ptCell As Point = Me.GetCellPos(ptCellIndex)
            Dim szCell As SizeF = Me.GetCellSize()

            Return New Rectangle(
                    ptCell.X,
                    ptCell.Y,
                    CInt(Math.Ceiling(szCell.Width)),
                    CInt(Math.Ceiling(szCell.Height))
            )

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the top left screen coordinates of a cell, given its index.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetCellPos(ByVal ptCellIndex As Point) As Point

            Dim szCell As SizeF = Me.GetCellSize()
            Return New Point(
                    CInt(Math.Floor((ptCellIndex.X - 1) * szCell.Width)),
                    CInt(Math.Floor((ptCellIndex.Y - 1) * szCell.Height))
            )

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the one-based index of a cell, based on a given screen point.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetCellIndex(ByVal ptScreen As Point) As Point

            Dim szCell As SizeF = Me.GetCellSize()
            Dim iColIndex As Integer = CInt((ptScreen.X + 0.5 * szCell.Width) / szCell.Width)
            Dim iRowIndex As Integer = CInt((ptScreen.Y + 0.5 * szCell.Height) / szCell.Height)

            ' Truncate
            iRowIndex = Math.Max(Math.Min(iRowIndex, Me.NumRows), 1)
            iColIndex = Math.Max(Math.Min(iColIndex, Me.NumCols), 1)

            Return New Point(iColIndex, iRowIndex)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the georeferenced location of a given screen point.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetLocation(ByVal ptScreen As Point) As PointF

            Dim bm As cEcospaceBasemap = Me.Basemap
            If (bm Is Nothing) Then Return Nothing

            Dim tl As PointF = bm.PosTopLeft
            Dim br As PointF = bm.PosBottomRight
            Dim lon As Single = tl.X + ptScreen.X * (br.X - tl.X) / Me.ClientRectangle.Width
            Dim lat As Single = tl.Y - ptScreen.Y * (tl.Y - br.Y) / Me.ClientRectangle.Height

            Return New PointF(lon, lat)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the screen point for a georeferenced location.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetScreenPoint(ptLocation As PointF) As Point

            Dim bm As cEcospaceBasemap = Me.Basemap
            If (bm Is Nothing) Then Return Nothing

            Dim tl As PointF = bm.PosTopLeft
            Dim br As PointF = bm.PosBottomRight
            Dim x As Integer = CInt((ptLocation.X - tl.X) * Me.ClientRectangle.Width / (br.X - tl.X))
            Dim y As Integer = CInt((tl.Y - ptLocation.Y) * Me.ClientRectangle.Height / (tl.Y - br.Y))

            Return New Point(x, y)


        End Function

#End Region ' Helper methods

    End Class

End Namespace

