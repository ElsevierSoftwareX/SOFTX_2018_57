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
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Reflection
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports Microsoft.Glee
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Flow editor control, including flow area, relevant toolbar(s) and 
''' property grid(s).
''' </summary>
''' <remarks>
''' JS: this diagram is so 'hack' it's not even funny. This code should be replaced
''' by a proper diagramming tool that uses less resources, has straight lines,
''' etc. How about itegrating with Diagram.Net?
''' </remarks>
''' ===========================================================================
Public Class plFlow
    Inherits ucSmoothPanel

#Region " Private variables "

    ''' <summary>The one reference to underlying data for manipulating objects.</summary>
    Private m_data As cData = Nothing
    ''' <summary>The one reference to underlying diagram data.</summary>
    Private m_diagram As cFlowDiagram = Nothing
    ''' <summary>Bitmap to test for click hits.</summary>
    Private m_bmpClickDetection As Bitmap = Nothing
    ''' <summary>List of cUnit representations.</summary>
    Private m_dtControls As New Dictionary(Of cUnit, plUnitControl)
    ''' <summary>List of link wrappers.</summary>
    Private m_lDiagramLinks As New List(Of LinkWrapper)
    ''' <summary>Current interaction mode.</summary>
    Private m_editMode As eEditMode = eEditMode.Move
    ''' <summary>UI Context</summary>
    Private m_uic As cUIContext = Nothing
    ''' <summary>Data selector</summary>
    Private m_selector As ucSelector2 = Nothing
    ''' <summary>Item to filter for in the flow, if any.</summary>
    Private m_itemFilter As cCoreInputOutputBase = Nothing
    ''' <summary>Unit to filter for in the flow, if any.</summary>
    Private m_unitFilter As cUnit = Nothing

    ''' <summary>Selected flow element.</summary>
    Private m_selection As Object = Nothing
    Private m_hover As LinkWrapper = Nothing

    '' ToDo: get rid of cUnitControl, render all in this graph

    ''' <summary>Drag/drop mouse offset.</summary>
    ''' <remarks>The (x,y) distance from a control's origin during a drag/drop operation.</remarks>
    Private m_ptMouseOffset As Point = Nothing
    ''' <summary>Unit control being dragged.</summary>
    Private m_ucDrag As plUnitControl = Nothing

    ''' <summary>Zoom factor</summary>
    Private m_sScale As Single = 1.0!

    ' Grid bits
    Private m_iCellWidth As Integer = 80
    Private m_iCellHeight As Integer = 40
    Private m_sGridMarginRatio As Single = 0.25 ' top/bottom and left/right margin
    Private m_bShowGrid As Boolean = False

    Private m_iNumControls As Integer = 0

    ' Private stock cursors
    Private m_crsDeleteGeneric As Cursor = cCursorFactory.GetCursorOverlay(Cursors.Arrow, SharedResources.DeleteHS)
    Private m_crsDeleteItem As Cursor = cCursorFactory.GetCursorOverlay(Cursors.Hand, SharedResources.DeleteHS)
    Private m_crsLinkGeneric As Cursor = cCursorFactory.GetCursorOverlay(Cursors.Arrow, SharedResources.chain_horz)
    Private m_crsLinkItem As Cursor = cCursorFactory.GetCursorOverlay(Cursors.Hand, SharedResources.chain_horz)
    Private m_crsMoveGeneric As Cursor = cCursorFactory.GetCursorOverlay(Cursors.Arrow, SharedResources.move)
    Private m_crsMoveItem As Cursor = cCursorFactory.GetCursorOverlay(Cursors.Hand, SharedResources.move)

#End Region ' Private variables

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating the current edit mode.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eEditMode As Byte
        ''' <summary>
        ''' Units are moved when clicked.
        ''' </summary>
        Move = 0
        ''' <summary>
        ''' Units are linked when clicked.
        ''' </summary>
        Link
        ''' <summary>
        ''' Units are deleted when clicked.
        ''' </summary>
        Delete
        ''' <summary>
        ''' The flow is not editable.
        ''' </summary>
        [ReadOnly]
    End Enum

    Public Event EditModeChanged(ByVal sender As plFlow, ByVal mode As eEditMode)
    Public Event ZoomChanged(ByVal sender As plFlow, ByVal sZoom As Single)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()
        MyBase.New()
        Me.AutoScroll = True
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If (disposing) Then
            Me.ClearFlow()

            If Me.m_uic IsNot Nothing Then
                RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleguideChanged
            End If

            Me.m_crsDeleteGeneric.Dispose()
            Me.m_crsDeleteItem.Dispose()
            Me.m_crsLinkGeneric.Dispose()
            Me.m_crsLinkItem.Dispose()
            Me.m_crsMoveGeneric.Dispose()
            Me.m_crsMoveItem.Dispose()

            Me.m_selector = Nothing
            Me.m_data = Nothing
            Me.m_uic = Nothing
            Me.m_dtControls = Nothing
            Me.m_lDiagramLinks = Nothing
            Me.m_bmpClickDetection.Dispose()
            Me.m_bmpClickDetection = Nothing

        End If
        MyBase.Dispose(disposing)
    End Sub

#Region " Public bits "

#Region " Scale "

    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property ZoomFactor() As Single
        Get
            Return Me.m_sScale
        End Get
        Set(ByVal value As Single)
            If (value = Me.m_sScale) Then Return
            Me.m_sScale = value
            For Each uc As plUnitControl In Me.m_dtControls.Values
                uc.ZoomFactor = value
            Next
            Me.Invalidate(True)
            RaiseEvent ZoomChanged(Me, Me.m_sScale)
        End Set
    End Property

    Public Sub Zoom(ByVal bZoomIn As Boolean)
        Dim i As Integer = -1
        Dim levels As Single() = Me.ZoomLevels
        For j As Integer = 0 To levels.Length - 1
            If Me.ZoomFactor = levels(j) Then i = j
        Next
        If (i = -1) Then Return
        If bZoomIn Then i += 1 Else i -= 1
        Me.ZoomFactor = levels(Math.Max(0, Math.Min(i, levels.Length - 1)))
    End Sub

    Public ReadOnly Property ZoomLevels() As Single()
        Get
            Return New Single() {0.5!, 0.75!, 1.0!, 1.25!, 1.5!, 2.0!}
        End Get
    End Property

#End Region ' Scale

#Region " Filters "

    ''' <summary>
    ''' Get/set the fleet to filter flow layout by.
    ''' </summary>
    Public Property ItemFilter() As cCoreInputOutputBase
        Get
            Return Me.m_itemFilter
        End Get
        Set(ByVal value As cCoreInputOutputBase)
            Me.m_itemFilter = value
            Me.m_unitFilter = Nothing
        End Set
    End Property

    ''' <summary>
    ''' Get/set the unit to filter flow layout by.
    ''' </summary>
    Public Property UnitFilter() As cUnit
        Get
            Return Me.m_unitFilter
        End Get
        Set(ByVal value As cUnit)
            Me.m_unitFilter = value
            Me.m_itemFilter = Nothing
        End Set
    End Property

#End Region ' Filters

#Region " Flow management "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the flow diagram with existing data.
    ''' </summary>
    ''' <param name="fd">The <see cref="cFlowDiagram">data</see> to connect the flow to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Init(ByVal uic As cUIContext, _
                    ByVal data As cData, _
                    ByVal fd As cFlowDiagram, _
                    ByVal sel As ucSelector2)

        If (Not Me.m_data Is Nothing) Then
            ' Init only once!
            Debug.Assert(False, "Already initialized!")
            Return
        End If

        Debug.Assert(uic IsNot Nothing)
        Debug.Assert(data IsNot Nothing)

        ' Store references
        Me.m_data = data
        Me.m_diagram = fd
        Me.m_selector = sel
        Me.m_uic = uic

        If Me.m_uic IsNot Nothing Then
            AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleguideChanged
        End If

        ' Load the layout of the flow (this will re-position created units to their saved positions)
        Me.RebuildFlow()

        ' Create click detection bitmap
        Me.CreateClickDetectionBitmap()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eEditMode">edit mode</see> of the panel.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EditMode() As eEditMode
        Get
            Return Me.m_editMode
        End Get
        Set(ByVal value As eEditMode)
            If Me.m_editMode <> value Then
                Me.m_editMode = value
                Me.Selection = Nothing
                RaiseEvent EditModeChanged(Me, Me.m_editMode)
            End If
        End Set
    End Property

    ' ''' -----------------------------------------------------------------------
    ' ''' <summary>
    ' ''' Auto-arrange the units in the flow panel.
    ' ''' </summary>
    ' ''' <remarks>
    ' ''' The initial version of this algorithm is pretty blunt and should be 
    ' ''' seriously refined.
    ' ''' </remarks>
    ' ''' -----------------------------------------------------------------------
    'Public Sub Arrange()

    '    Dim ptUnitMargin As New Point(CInt(Me.m_iCellWidth * Me.m_sGridMarginRatio * 0.5), CInt(Me.m_iCellHeight * Me.m_sGridMarginRatio * 0.5))
    '    Dim uc As plUnitControl = Nothing
    '    Dim iUnitColumn As Integer = 0
    '    Dim aiUnitCount([Enum].GetValues(GetType(cUnitFactory.cUnitFormatter)).Length) As Integer

    '    ' Align each unit in its column, where row position is based on unit index
    '    '    ToDo: include branches, merges into algorithm
    '    For Each unit As cUnit In Me.m_dtControls.Keys
    '        iUnitColumn = CInt(unit.UnitType) - 1
    '        uc = Me.m_dtControls(unit)
    '        With uc.FlowPos
    '            .AllowEvents = False
    '            .Xpos = CInt(ptUnitMargin.X + iUnitColumn * Me.m_iCellWidth)
    '            .Ypos = CInt(ptUnitMargin.Y + aiUnitCount(iUnitColumn) * Me.m_iCellHeight)
    '            .AllowEvents = True
    '        End With
    '        aiUnitCount(iUnitColumn) += 1
    '    Next

    '    ' Switch to 'move' mode upon arranging if NOT readonly
    '    If (Me.EditMode <> eEditMode.ReadOnly) Then
    '        Me.EditMode = eEditMode.Move
    '    End If

    '    Me.Refresh()

    'End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Auto-arrange the units in the flow panel using the GLEE library.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub ArrangeGLEE()

        Dim ptUnitMargin As New Point(CInt(Me.m_iCellWidth * Me.m_sGridMarginRatio * 0.5), _
                                      CInt(Me.m_iCellHeight * Me.m_sGridMarginRatio * 0.5))

        ' Switch to 'move' mode upon arranging if NOT readonly
        If (Me.EditMode <> eEditMode.ReadOnly) Then
            Me.EditMode = eEditMode.Move
        End If

        Dim g As New GleeGraph()
        Dim i As Integer = 0
        Dim nodes As New Dictionary(Of cUnit, Node)
        Dim layers As New Dictionary(Of Type, Microsoft.Glee.LayerEdge)

        g.NodeSeparation = Math.Max(Me.m_sGridMarginRatio * 0.5, Me.m_sGridMarginRatio * 0.5)

        ' Feed graph with nodes
        For Each unit As cUnit In Me.m_dtControls.Keys
            Dim uc As plUnitControl = Me.m_dtControls(unit)
            ' Reverse height and width, 'cause the graph will be flipped Vert to Horz
            Dim n As New Microsoft.Glee.Node(CStr(i), Splines.CurveFactory.CreateBox(uc.Height, uc.Width, New Splines.Point(0, 0)))
            g.AddNode(n)
            nodes(unit) = n
            i += 1
        Next

        ' Feed graph with connections
        For Each l As LinkWrapper In Me.m_lDiagramLinks
            ' We may be arranging a partial diagram - test for completeness
            If nodes.ContainsKey(l.Target) And nodes.ContainsKey(l.Source) Then
                g.AddEdge(New Edge(nodes(l.Target), nodes(l.Source)))
            End If
        Next

        ' Shazam
        g.CalculateLayout()

        ' Hack: find layouted graph offset. Bounding box cannot be limited by GleeGraph
        Dim dx As Integer = Integer.MaxValue
        Dim dy As Integer = Integer.MaxValue
        For Each n As Node In nodes.Values
            Dim ptc As Splines.Point = n.Center
            dx = Math.Min(dx, CInt(ptc.X))
            dy = Math.Min(dy, CInt(ptc.Y))
        Next

        ' Apply layout
        For Each unit As cUnit In Me.m_dtControls.Keys
            Dim uc As plUnitControl = Me.m_dtControls(unit)
            Dim n As Node = nodes(unit)
            Dim ptc As Splines.Point = n.Center
            ' Switch x and Y to get a horizontal graph
            uc.FlowPos.AllowEvents = False
            uc.FlowPos.Xpos = CInt(ptc.Y - dy + ptUnitMargin.X)
            uc.FlowPos.Ypos = CInt(ptc.X - dx + ptUnitMargin.Y)
            uc.FlowPos.AllowEvents = True
        Next

        Me.Refresh()

    End Sub

    Private Sub ArrangeQuiet()

        Dim ptUnitMargin As New Point(CInt(Me.m_iCellWidth * Me.m_sGridMarginRatio * 0.5), _
                                      CInt(Me.m_iCellHeight * Me.m_sGridMarginRatio * 0.5))

        Dim iMinX As Integer = Integer.MaxValue
        Dim iMinY As Integer = Integer.MaxValue

        ' Feed graph with nodes
        For Each unit As cUnit In Me.m_dtControls.Keys
            Dim uc As plUnitControl = Me.m_dtControls(unit)
            Dim fp As cFlowPosition = uc.FlowPos
            iMinX = Math.Min(fp.Xpos, iMinX) : iMinY = Math.Min(fp.Ypos, iMinY)
        Next
        If (iMinX <> 0) Or (iMinY <> 0) Then
            For Each unit As cUnit In Me.m_dtControls.Keys
                Dim uc As plUnitControl = Me.m_dtControls(unit)
                Dim fp As cFlowPosition = uc.FlowPos

                fp.AllowEvents = False
                fp.Xpos = CInt(fp.Xpos - iMinX + ptUnitMargin.X)
                fp.Ypos = CInt(fp.Ypos - iMinY + ptUnitMargin.Y)
                fp.AllowEvents = True

            Next
        End If
    End Sub

    Public Property ShowGrid() As Boolean
        Get
            Return Me.m_bShowGrid
        End Get
        Set(ByVal value As Boolean)
            If value <> Me.m_bShowGrid Then
                Me.m_bShowGrid = value
                Me.Refresh()
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Rebuild a flow with all units that match present filter settings.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub RebuildFlow()

        Dim lUnits As New List(Of cUnit)

        Me.Visible = False

        ' Has unit filter?
        If (Me.m_unitFilter IsNot Nothing) Then
            ' #Yes: grab unit only
            lUnits.Add(Me.m_unitFilter)
        ElseIf (Me.ItemFilter IsNot Nothing) Then
            ' #No: Has fleet filter?
            lUnits.AddRange(Me.m_data.GetUnits(Me.ItemFilter))
        Else
            ' #No: grab all units
            lUnits.AddRange(Me.m_data.GetUnits(cUnitFactory.eUnitType.All))
        End If

        Me.ClearFlow()

        ' Generate unit elements for all units
        For Each unit As cUnit In lUnits
            Me.AddUnit(unit)
        Next

        ' Generate link elements for unit in the flow
        For Each unit As cUnit In Me.m_dtControls.Keys
            For j As Integer = 0 To unit.LinkOutCount - 1
                Me.AddLink(unit.LinkOut(j), False)
            Next j
        Next unit

        Try
            ' Rendering for a temporary diagram?
            If (Me.m_diagram Is Nothing) Then
                ' #Yes: auto-layout
                Me.ArrangeGLEE()
            Else
                Me.ArrangeQuiet()
            End If
        Catch ex As Exception

        End Try

        Me.Visible = True

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, get all the units currently in the flow.
    ''' </summary>
    ''' <returns>A list with all units currently in the flow.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GetFlowUnits() As cUnit()
        Dim lUnits As New List(Of cUnit)
        lUnits.AddRange(Me.m_dtControls.Keys)
        Return lUnits.ToArray
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clear the flow
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub ClearFlow()

        Dim llinks As New List(Of cLink)
        Dim lunits As New List(Of cUnit)

        ' Get links
        For Each w As LinkWrapper In Me.m_lDiagramLinks
            llinks.AddRange(w.Links)
        Next
        ' Remove 'em all
        For Each l As cLink In llinks
            Me.RemoveLink(l)
        Next

        ' Get all units
        For Each u As cUnit In Me.m_dtControls.Keys
            lunits.Add(u)
        Next
        ' Remove 'em all
        For Each u As cUnit In lunits
            Me.RemoveUnit(u)
        Next

        If (Me.m_selector IsNot Nothing) Then
            Me.m_selector.Selection = Nothing
        End If

        Debug.Assert(Me.Controls.Count = 0)
        Debug.Assert(Me.m_dtControls.Count = 0)
        Debug.Assert(Me.m_lDiagramLinks.Count = 0)

    End Sub

#End Region ' Flow management

#End Region ' Public interfaces

#Region " Event handling "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, processes mouse clicks to operate on UnitConnectors.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnMouseClick(ByVal e As System.Windows.Forms.MouseEventArgs)
        Me.ProcessConnectorClick(e.Location)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, processes mouse movement to provide cursor feedback.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim lw As LinkWrapper = ConnectorFromPoint(e.Location)

        Select Case Me.m_editMode

            Case eEditMode.Link
                If (Me.Selection IsNot Nothing) Then
                    Me.Cursor = Me.m_crsLinkItem
                Else
                    Me.Cursor = Me.m_crsLinkGeneric
                End If

            Case eEditMode.Delete
                If (lw IsNot Nothing) Then
                    Me.Cursor = Me.m_crsDeleteItem
                Else
                    Me.Cursor = Me.m_crsDeleteGeneric
                End If

            Case Else
                Me.Cursor = Cursors.Default

        End Select
    End Sub

    Protected Overrides Sub OnMouseHover(ByVal e As System.EventArgs)
        Me.Focus()
    End Sub

    Protected Overrides Sub OnMouseWheel(e As System.Windows.Forms.MouseEventArgs)
        If My.Computer.Keyboard.CtrlKeyDown Then
            Me.Zoom(e.Delta > 0)
        Else
            MyBase.OnMouseWheel(e)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Paint the panel and all unit connectors
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)

        e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        MyBase.OnPaint(e)

        If (Me.m_uic Is Nothing) Then Return

        Dim g As Graphics = Graphics.FromImage(Me.m_bmpClickDetection)
        Dim ctrlSource As plUnitControl = Nothing
        Dim ctrlTarget As plUnitControl = Nothing
        Dim clrFore As Color = Color.Black
        Dim clrBack As Color = Color.Black
        Dim sg As cStyleGuide = Me.m_uic.StyleGuide

        ' Draw form background
        Using br As New SolidBrush(Me.BackColor)
            e.Graphics.FillRectangle(br, Me.ClientRectangle)
        End Using

        ' Need to draw grid?
        If Me.ShowGrid Then
            ' #Yes: Let's draw that grid then
            ' Use a subtle colour variation on the background by inverting the third bit of its RGB values
            Using p As New Pen(Color.FromArgb(255, Me.BackColor.R Xor 16, Me.BackColor.G Xor 16, Me.BackColor.B Xor 16), 1)
                For i As Integer = CInt(Me.m_sScale * Me.m_iCellHeight) To Me.Height Step CInt(Me.m_sScale * Me.m_iCellHeight)
                    e.Graphics.DrawLine(p, 0, i, Me.Width, i)
                Next
                For i As Integer = CInt(Me.m_sScale * Me.m_iCellWidth) To Me.Width Step CInt(Me.m_sScale * Me.m_iCellWidth)
                    e.Graphics.DrawLine(p, i, 0, i, Me.Height)
                Next
            End Using
        End If

        ' Draw hit detection bitmap
        g.FillRectangle(Brushes.White, 0, 0, Me.Width, Me.Height)

        For Each c As LinkWrapper In Me.m_lDiagramLinks
            Try
                ctrlSource = Me.m_dtControls(c.Source)
                ctrlTarget = Me.m_dtControls(c.Target)

                Dim ptT As Point = Me.FindIntersect(ctrlSource.Center, ctrlTarget.Center, ctrlTarget)

                ' Paint link on visible canvas
                If ReferenceEquals(Me.Selection, c) Then
                    clrFore = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
                ElseIf ReferenceEquals(Me.Selection, ctrlSource) Then
                    clrFore = sg.ApplicationColor(cStyleGuide.eApplicationColorType.PREY)
                ElseIf ReferenceEquals(Me.Selection, ctrlTarget) Then
                    clrFore = sg.ApplicationColor(cStyleGuide.eApplicationColorType.PREDATOR)
                Else
                    clrFore = Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT)
                    'Me.m_uic.StyleGuide.GetStyleColors(c.Style, clrFore, clrBack)
                End If

                PaintLink(e.Graphics, ctrlSource.Center, ptT, clrFore, c.Width, c.External)

                ' Paint detection link on detection bitmap with a fixed width to make the link better clickable
                PaintLink(g, ctrlSource.Center, ptT, c.Color, 2)

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        Next

    End Sub

    Protected Overrides Sub OnScroll(se As System.Windows.Forms.ScrollEventArgs)
        Me.Invalidate()
        MyBase.OnScroll(se)
    End Sub

    Protected Overrides Sub OnResize(ByVal eventargs As System.EventArgs)
        MyBase.OnResize(eventargs)
        ' When panel is resized, the link detection bitmap needs to be resized accordingly
        Me.CreateClickDetectionBitmap()
    End Sub

    Private Sub OnStyleguideChanged(ByVal changeFlags As cStyleGuide.eChangeType)
        If ((changeFlags And cStyleGuide.eChangeType.Colours) > 0) Then
            Me.Invalidate(True)
        End If
    End Sub

#End Region ' Event handling

#Region " Selection "

    ''' <summary>
    ''' Panel selection change event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="obj"></param>
    Public Event SelectionChanged(ByVal sender As plFlow, ByVal obj As Object)

    Public Property Selection() As Object
        Get
            Return Me.m_selection
        End Get

        Private Set(ByVal value As Object)
            ' Optimization
            If ReferenceEquals(Me.m_selection, value) Then Return

            If TypeOf (Me.m_selection) Is plUnitControl Then
                DirectCast(Me.m_selection, plUnitControl).Selected = False
            End If

            ' Assign
            Me.m_selection = value

            If (Me.m_selector IsNot Nothing) Then

                If TypeOf (Me.m_selection) Is plUnitControl Then
                    ' Update property grid
                    Me.m_selector.Selection = DirectCast(Me.m_selection, plUnitControl).Unit
                    ' Update selected state
                    DirectCast(Me.m_selection, plUnitControl).Selected = True
                ElseIf TypeOf (Me.m_selection) Is LinkWrapper Then
                    ' Update property grid
                    Me.m_selector.Selection = DirectCast(Me.m_selection, LinkWrapper).Links
                ElseIf TypeOf (Me.m_selection) Is Array Then
                    Dim lSel As New List(Of cLink)
                    For Each conn As LinkWrapper In DirectCast(Me.m_selection, LinkWrapper())
                        lSel.AddRange(conn.Links)
                    Next
                    ' Update property grid
                    Me.m_selector.Selection = lSel.ToArray
                End If
            End If

            Try
                ' Pass out selection
                RaiseEvent SelectionChanged(Me, Me.m_selection)
            Catch ex As Exception

            End Try

            Me.UpdateControls()
            Me.Invalidate()

        End Set
    End Property

#End Region ' Selection

#Region " Item admin "

#Region " Unit creation, deletion, modification "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a producer for every Ecopath fleet.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub CreateProducersForFleets()

        ' For all Ecopath fleets:

        Dim lUnits As cUnit() = Me.m_data.GetUnits(cUnitFactory.eUnitType.Producer)
        Dim core As cCore = Me.m_data.Core
        Dim fleet As cEcopathFleetInput = Nothing
        Dim pu As cProducerUnit = Nothing
        Dim bProducerExists As Boolean = False

        For iFleet As Integer = 1 To core.nFleets
            ' Find unit
            bProducerExists = False
            fleet = core.EcopathFleetInputs(iFleet)
            For Each unit As cUnit In lUnits
                pu = DirectCast(unit, cProducerUnit)
                If (ReferenceEquals(fleet, pu.Fleet)) Then
                    bProducerExists = True
                    Exit For
                End If
            Next unit
            ' Not found?
            If Not bProducerExists Then
                ' #Yes: create it
                pu = DirectCast(Me.CreateUnit(cUnitFactory.eUnitType.Producer), cProducerUnit)
                pu.AllowEvents = False
                pu.Fleet = fleet
                pu.AllowEvents = True
            End If
        Next iFleet
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new unit
    ''' </summary>
    ''' <param name="unitType"></param>
    ''' -----------------------------------------------------------------------
    Public Function CreateUnit(ByVal unitType As cUnitFactory.eUnitType) As cUnit

        Dim unit As cUnit = Nothing
        Dim lstrNames As New List(Of String)
        Dim strMask As String = ""
        Dim strName As String = ""

        ' Gather names
        For Each unit In Me.m_data.GetUnits(unitType)
            lstrNames.Add(unit.Name)
        Next

        ' Determine mask name
        Select Case unitType
            Case cUnitFactory.eUnitType.Wholesaler
                strMask = My.Resources.CORE_DEFAULT_WHOLESALER
            Case cUnitFactory.eUnitType.Retailer
                strMask = My.Resources.CORE_DEFAULT_RETAILER
            Case cUnitFactory.eUnitType.Processing
                strMask = My.Resources.CORE_DEFAULT_PROCESSING
            Case cUnitFactory.eUnitType.Producer
                strMask = ""
            Case cUnitFactory.eUnitType.Distribution
                strMask = My.Resources.CORE_DEFAULT_DISTRIBUTION
            Case cUnitFactory.eUnitType.Consumer
                strMask = My.Resources.CORE_DEFAULT_CONSUMER
        End Select

        ' Has a mask?
        If Not String.IsNullOrEmpty(strMask) Then
            ' #Yes: concoct a name with an autonumber
            strName = cStringUtils.Localize(strMask, EwEUtils.Utilities.cStringUtils.GetNextNumber(lstrNames.ToArray, strMask))
        End If

        ' (try to) create unit
        unit = Me.m_data.CreateUnit(unitType, strName)

        ' Successfully created?
        If unit IsNot Nothing Then
            ' #Yes: add unit 
            Me.AddUnit(unit, True)
            ' Switch to move mode
            Me.EditMode = eEditMode.Move
        End If

        Me.CheckMissingParameters()
        Return unit

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add an existing unit to the flow.
    ''' </summary>
    ''' <param name="unit">The unit to add.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function AddUnit(ByVal unit As cUnit, Optional ByVal bSelect As Boolean = False) As plUnitControl

        Dim fp As cFlowPosition = Nothing

        ' Try to get existing flow position for registered diagrams
        If (Me.m_diagram IsNot Nothing) Then
            fp = Me.m_data.FlowPosition(unit, Me.m_diagram)
        End If

        ' No flow position available?
        If fp Is Nothing Then
            ' #Yes: create one
            fp = New cFlowPosition()
            fp.Unit = unit
            fp.Diagram = Me.m_diagram
            ' Need to store new flow position for registered diagram?
            If (Me.m_diagram IsNot Nothing) Then
                ' #Yes: store it
                Me.m_data.AddFlowPosition(fp)
            End If

            fp.AllowEvents = False
            fp.Xpos = 10
            fp.Ypos = 10
            fp.Width = CInt(Me.m_iCellWidth * (1 - Me.m_sGridMarginRatio))
            fp.Height = CInt(Me.m_iCellHeight * (1 - Me.m_sGridMarginRatio))
            fp.AllowEvents = True

        End If

        Dim uc As New plUnitControl(Me.m_uic, fp)

        uc.ZoomFactor = Me.ZoomFactor

        Me.Controls.Add(uc)
        Me.m_dtControls(fp.Unit) = uc
        uc.BringToFront()

        If bSelect Then Me.Selection = uc

        Me.m_iNumControls += 1

        ' Start listening for unit changes
        AddHandler fp.Unit.OnChanged, AddressOf OnElementChanged

        Return uc

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove a flow element from this control.
    ''' </summary>
    ''' <param name="unit"></param>
    ''' -----------------------------------------------------------------------
    Public Sub RemoveUnit(ByVal unit As cUnit)

        Dim uc As plUnitControl = Me.m_dtControls(unit)

        ' Detach event handlers
        RemoveHandler unit.OnChanged, AddressOf OnElementChanged

        ' Remove all source links
        For i As Integer = 0 To unit.LinkInCount - 1
            Me.RemoveLink(unit.LinkIn(i))
        Next
        ' Remove all target links
        For i As Integer = 0 To unit.LinkOutCount - 1
            Me.RemoveLink(unit.LinkOut(i))
        Next

        ' Clear selection if neccesary
        If ReferenceEquals(Me.m_selection, unit) Then Me.m_selection = Nothing
        ' Clear dragged object if neccesary
        If ReferenceEquals(Me.m_ucDrag, uc) Then Me.m_ucDrag = Nothing

        ' Remove control
        Me.m_dtControls.Remove(unit)
        Me.Controls.Remove(uc)
        ' Manually clear this
        uc.Dispose()

        ' Rerender
        Me.Invalidate()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove an existing unit from the flow, and remove it from the underlying data
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DeleteUnit(ByVal unit As cUnit, ByVal fp As cFlowPosition) As Boolean

        If Me.m_data.Parameters.DeletePrompt Then

            Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.PROMPT_DELETEUNIT, unit.Name), _
                                             eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            Me.m_uic.Core.Messages.SendMessage(fmsg)
            If (fmsg.Reply <> eMessageReply.YES) Then Return False

        End If

        Return Me.m_data.DeleteUnit(unit)

    End Function

    Public Function ConvertUnit(ByVal unit As cUnit, ByVal convertTo As cUnitFactory.eUnitType) As Boolean

        Dim fmt As New cUnitTypeFormatter()
        Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.PROMPT_CONVERT_UNIT, unit.Name, fmt.GetDescriptor(unit.UnitType), fmt.GetDescriptor(convertTo)), _
                                         eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
        fmsg.Reply = eMessageReply.YES
        fmsg.Suppressable = True
        Me.m_uic.Core.Messages.SendMessage(fmsg)
        If (fmsg.Reply <> eMessageReply.YES) Then Return False

        Dim unitNew As cUnit = Me.CreateUnit(convertTo)
        Dim llinks As New List(Of cLink)
        Dim api As PropertyInfo() = cPropertyInfoHelper.GetAllowedProperties(unit.GetType)

        ' Copy properties
        For Each pi As PropertyInfo In api
            Try
                Dim objVal As Object = pi.GetValue(unit, Nothing)
                pi.SetValue(unitNew, objVal, Nothing)
            Catch ex As Exception
                ' Whaaah!
            End Try
        Next

        For i As Integer = 0 To unit.LinkInCount - 1
            llinks.Add(unit.LinkIn(i))
        Next
        For Each l As cLink In llinks
            Dim src As cUnit = l.Source
            src.RemoveLink(l)
            l.Target = unitNew
            src.AddLink(l)
        Next

        llinks.Clear()
        For i As Integer = 0 To unit.LinkOutCount - 1
            llinks.Add(unit.LinkOut(i))
        Next
        For Each l As cLink In llinks
            unit.RemoveLink(l)
            l.Source = unitNew
            unitNew.AddLink(l)
        Next

        Dim pos As cFlowPosition = Me.m_data.FlowPosition(unit, Me.m_diagram)
        pos.Unit = unitNew
        Me.m_data.DeleteUnit(unit)

        Me.RebuildFlow()

    End Function

#End Region ' Unit creation, deletion, modification 

#Region " Link creation, deletion, modification "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add an existing link to the flow
    ''' </summary>
    ''' <param name="link"></param>
    ''' <param name="bRefresh"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function AddLink(ByVal link As cLink, Optional ByVal bRefresh As Boolean = True) As LinkWrapper

        ' Sanity checks
        If (link Is Nothing) Then Return Nothing
        If (Not Me.m_dtControls.ContainsKey(link.Source)) Then Return Nothing
        If (Not Me.m_dtControls.ContainsKey(link.Target)) Then Return Nothing

        AddHandler link.OnChanged, AddressOf OnElementChanged

        Dim w As LinkWrapper = Me.FindLinkWrapper(link)
        If w Is Nothing Then
            w = New LinkWrapper(link)
            Me.m_lDiagramLinks.Add(w)
        Else
            w.AddLink(link)
        End If
        Me.Invalidate(True)
        Return w

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove a link from the flow
    ''' </summary>
    ''' <param name="link"></param>
    ''' <remarks></remarks>
    ''' -----------------------------------------------------------------------
    Public Sub RemoveLink(ByVal link As cLink)
        If link Is Nothing Then Return
        RemoveHandler link.OnChanged, AddressOf OnElementChanged

        ' Clear selection if neccesary
        If ReferenceEquals(Me.m_selection, link) Then Me.m_selection = Nothing

        Dim w As LinkWrapper = Me.FindLinkWrapper(link)
        If w IsNot Nothing Then
            w.RemoveLink(link)
            If w.LinkCount = 0 Then
                Me.m_lDiagramLinks.Remove(w)
            End If
            Me.Invalidate()
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove a link from the flow, and delete it in the underlying data.
    ''' </summary>
    ''' <param name="link"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function DeleteLink(ByVal link As cLink) As Boolean

        If Me.m_data.Parameters.DeletePrompt Then

            Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.PROMPT_DELETELINK, link.Name), EwEUtils.Core.eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            Me.m_uic.Core.Messages.SendMessage(fmsg)
            If (fmsg.Reply <> eMessageReply.YES) Then Return False

        End If

        Me.RemoveLink(link)
        Me.m_data.DeleteLink(link)
        Me.Invalidate(True)
        Return True
    End Function

#End Region ' Link creation, deletion, modification

#End Region ' Item admin

#Region " Item UI "

#Region " Units "

    Public Sub OnUnitMouseDown(ByVal uc As plUnitControl)

        Select Case Me.EditMode

            Case eEditMode.Move
                Me.StartUnitDrag(uc)
                Me.Selection = uc

            Case eEditMode.Link
                uc.Cursor = cCursorFactory.GetCursorOverlay(Cursors.Hand, SharedResources.chain_horz)
                If TypeOf Me.Selection Is plUnitControl Then
                    Dim unitSelected As cUnit = DirectCast(Me.Selection, plUnitControl).Unit
                    Dim bError As Boolean = False
                    If TypeOf unitSelected Is cProducerUnit Then
                        ' Do not create link if there are no landings
                        Dim prodSelected As cProducerUnit = DirectCast(unitSelected, cProducerUnit)
                        If (prodSelected.Fleet Is Nothing) Then
                            Me.m_data.SendMessage(My.Resources.ERROR_LINK_NEEDFLEET)
                            Return
                        End If

                        ' Create link for every group
                        For iGroup As Integer = 1 To Me.m_data.Core.nGroups
                            If Not bError Then
                                Dim group As cEcoPathGroupInput = Me.m_data.Core.EcoPathGroupInputs(iGroup)
                                Dim link As cLinkLandings = Me.m_data.CreateLandingsLink(DirectCast(unitSelected, cProducerUnit), uc.Unit, group, bError)
                                If (link IsNot Nothing) Then
                                    Me.AddLink(link)
                                End If
                            End If
                        Next
                        Me.CheckMissingParameters()
                    Else
                        Dim link As cLink = Me.m_data.CreateLink(unitSelected, uc.Unit)
                        If link IsNot Nothing Then
                            Me.AddLink(link)
                            Me.CheckMissingParameters()
                        End If
                    End If
                    ' Clear selection
                    Me.Selection = Nothing
                Else
                    Me.Selection = uc
                End If

            Case eEditMode.Delete
                If Me.DeleteUnit(uc.Unit, uc.FlowPos) Then
                    ' Do not delete again
                    Me.EditMode = eEditMode.Move
                    ' Yeah!
                    Me.RebuildFlow()
                    Me.CheckMissingParameters()
                End If

        End Select

    End Sub

    Public Sub OnUnitMouseHover(ByVal uc As plUnitControl, ByVal bHover As Boolean)

        Select Case Me.EditMode

            Case eEditMode.ReadOnly
                ' Do not change the cursor

            Case eEditMode.Move
                uc.Cursor = Me.m_crsMoveItem

            Case eEditMode.Link
                uc.Cursor = Me.m_crsLinkItem

            Case eEditMode.Delete
                uc.Cursor = Me.m_crsDeleteItem

            Case Else
                uc.Cursor = Cursors.Hand

        End Select

    End Sub

#End Region ' Units

#Region " Links "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, creates the bitmap for detecting line clicks
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub CreateClickDetectionBitmap()
        Dim bmp As Bitmap = Me.m_bmpClickDetection
        If bmp IsNot Nothing Then bmp.Dispose() : bmp = Nothing
        Me.m_bmpClickDetection = New Bitmap(Me.ClientRectangle.Width + 1, Me.ClientRectangle.Height + 1, Imaging.PixelFormat.Format32bppArgb)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, returns the color of a pixel in the click detection bitmap.
    ''' </summary>
    ''' <param name="pt">The location of the pixel to return the color for.</param>
    ''' <returns>The color of the indicated pixel in the detection bitmap.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ColorAtPoint(ByVal pt As Point) As Color
        Try
            Return Me.m_bmpClickDetection.GetPixel(pt.X, pt.Y)
        Catch ex As Exception
            Return Color.Transparent
        End Try
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, states whether a color value is a valid line color in the click detection bitmap.
    ''' </summary>
    ''' <param name="clr">The color to test.</param>
    ''' <returns>True if the color could be used for a line</returns>
    ''' -----------------------------------------------------------------------
    Private Function IsLineColor(ByVal clr As Color) As Boolean
        Return clr.R <> 255 Or clr.G <> 255 Or clr.B <> 255
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, attempts to return the unit connector for a given color value.
    ''' </summary>
    ''' <param name="clr">The color to find the unit connector for.</param>
    ''' <returns>A unit connector instance, or nothing if this connector could not be found.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ConnectorFromColor(ByVal clr As Color) As LinkWrapper
        If IsLineColor(clr) Then
            For Each uc As LinkWrapper In Me.m_lDiagramLinks
                If uc.Color = clr Then
                    Return uc
                End If
            Next
        End If
        Return Nothing
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, attempts to return the unit connector for a given color value.
    ''' </summary>
    ''' <param name="pt">The location to test.</param>
    ''' <returns>A unit connector instance, or nothing if this connector could not be found.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ConnectorFromPoint(ByVal pt As Point) As LinkWrapper
        Return Me.ConnectorFromColor(Me.ColorAtPoint(pt))
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether the point under the mouse cursor belongs to a connector.
    ''' </summary>
    ''' <param name="pt">The location to test.</param>
    ''' <returns>True if there is a unit connector at (or very near to) this location.</returns>
    ''' -----------------------------------------------------------------------
    Private Function HasConnectorUnderCursor(ByVal pt As Point) As Boolean
        Dim conn As LinkWrapper = Me.ConnectorFromPoint(pt)
        Return (conn IsNot Nothing)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Process a mouse click to find and operate on a UnitConnector.
    ''' </summary>
    ''' <param name="pt">The location that was clicked.</param>
    ''' -----------------------------------------------------------------------
    Private Sub ProcessConnectorClick(ByVal pt As Point)

        Dim w As LinkWrapper = Me.ConnectorFromColor(Me.ColorAtPoint(pt))
        If (w IsNot Nothing) Then

            Dim alinks As cLink() = w.Links

            Select Case Me.EditMode
                Case eEditMode.Delete
                    For Each link As cLink In alinks : Me.DeleteLink(link) : Next
                    Me.CheckMissingParameters()
                    Me.Refresh()
                Case eEditMode.Move
                    Me.Selection = w
                Case eEditMode.Link
                    Me.Selection = Nothing
            End Select

        Else
            Me.Selection = Nothing
        End If

    End Sub

    Private Function FindLinkWrapper(ByVal link As cLink) As LinkWrapper
        For Each w As LinkWrapper In Me.m_lDiagramLinks
            If w.HasLink(link) Then Return w
        Next
        Return Nothing
    End Function

#End Region ' Links

#Region " Drag/drop "

    Private Sub StartUnitDrag(ByVal uc As plUnitControl)
        Dim ptMouse As Point = Cursor.Position
        Dim ptControl As Point = uc.Location

        If (Me.m_ucDrag Is Nothing) Then
            Me.m_ucDrag = uc
            Me.m_ucDrag.BringToFront()
            Me.m_ptMouseOffset = New Point(ptMouse.X - ptControl.X, ptMouse.Y - ptControl.Y)

            AddHandler Me.m_ucDrag.MouseMove, AddressOf TrackMouseMove
            AddHandler Me.m_ucDrag.MouseUp, AddressOf TrackMouseUp
        End If
    End Sub

    Private Sub EndUnitDrag()
        If (Me.m_ucDrag IsNot Nothing) Then
            RemoveHandler Me.m_ucDrag.MouseMove, AddressOf TrackMouseMove
            RemoveHandler Me.m_ucDrag.MouseUp, AddressOf TrackMouseUp
        End If
        Me.m_ucDrag = Nothing
    End Sub

    Private Sub TrackMouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        Dim ptUnitMargin As New Point(CInt(Me.m_iCellWidth * Me.m_sGridMarginRatio * 0.5), _
                                      CInt(Me.m_iCellHeight * Me.m_sGridMarginRatio * 0.5))
        Dim ptMouse As Point = Cursor.Position

        ' Drag via flow position
        With Me.m_ucDrag.FlowPos
            .AllowEvents = False
            .Xpos = CInt((ptMouse.X - Me.m_ptMouseOffset.X) / Me.m_sScale)
            .Ypos = CInt((ptMouse.Y - Me.m_ptMouseOffset.Y) / Me.m_sScale)
            If Me.m_bShowGrid Then
                ' Truncate pos
                .Xpos = ptUnitMargin.X + CInt(Me.m_iCellWidth * Math.Round(.Xpos / Me.m_iCellWidth))
                .Ypos = ptUnitMargin.Y + CInt(Me.m_iCellHeight * Math.Round(.Ypos / Me.m_iCellHeight))
            End If

            .AllowEvents = True
        End With
        Me.Refresh()
    End Sub

    Private Sub TrackMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        Me.EndUnitDrag()
    End Sub

#End Region ' Drag/drop

#End Region ' Item UI

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; trapped to redraw live data changes.
    ''' </summary>
    ''' <param name="obj">The item that changed.</param>
    ''' -----------------------------------------------------------------------
    Private Sub OnElementChanged(ByVal obj As cEwEDatabase.cOOPStorable)
        Me.Invalidate()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Find the point where the line (<paramref name="ptFrom"/>-<paramref name="ptTo"/>)
    ''' intersects with unit <paramref name="unitTo"/>.
    ''' </summary>
    ''' <param name="ptFrom"></param>
    ''' <param name="ptTo"></param>
    ''' <param name="unitTo"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function FindIntersect(ByVal ptFrom As Point, ByVal ptTo As Point, ByVal unitTo As plUnitControl) As Point
        If (unitTo Is Nothing) Then Return ptTo

        'The slope of the line is s = (Ay - By)/(Ax - Bx).

        'If -h/2 <= s * w/2 <= h/2 then the line intersects:
        '    The right edge if Ax > Bx
        '    The left edge if Ax < Bx.
        'If -w/2 <= (h/2)/s <= w/2 then the line intersects:
        '    The top edge if Ay > By
        '    The bottom edge if Ay < By.

        Dim dx As Single = ptFrom.X - ptTo.X
        Dim dy As Single = ptFrom.Y - ptTo.Y
        Dim ptIntersect As Point = ptTo
        Dim rc As New Rectangle(unitTo.Location, unitTo.Size)

        If dx <> 0 And dy <> 0 Then
            Dim sSlope As Single = (dy / rc.Height) / (dx / rc.Width)
            If Math.Abs(sSlope * rc.Width / dx) <= Math.Abs(rc.Height / dy) Then
                If dx < 0 Then
                    ptIntersect = New Point(rc.Left, CInt(ptTo.Y - rc.Height * (0.5 * sSlope)))
                Else
                    ptIntersect = New Point(rc.Right, CInt(ptTo.Y + rc.Height * (0.5 * sSlope)))
                End If
            Else
                If dy < 0 Then
                    ptIntersect = New Point(CInt(ptTo.X - rc.Width * (0.5 / sSlope)), rc.Top)
                Else
                    ptIntersect = New Point(CInt(ptTo.X + rc.Width * (0.5 / sSlope)), rc.Bottom)
                End If
            End If
        Else
            If dx = 0 Then
                ptIntersect = New Point(ptTo.X, CInt(If(dy < 0, rc.Top, rc.Bottom)))
            Else
                ptIntersect = New Point(CInt(If(dx < 0, rc.Left, rc.Right)), ptTo.Y)
            End If
        End If
        Return ptIntersect
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Paint a link between two units.
    ''' </summary>
    ''' <param name="g">Graphics to draw onto.</param>
    ''' <param name="ptStart">Start point to draw from.</param>
    ''' <param name="ptEnd">End point to draw to.</param>
    ''' <param name="clr">Colour to use for the link.</param>
    ''' <param name="sWeight">Weight of the link [0, 1]. 
    ''' If 0, the link is rendered in gray.</param>
    ''' <param name="bExternal">Flag stating whether this link represents an 
    ''' 'External' connection.</param>
    ''' -----------------------------------------------------------------------
    Private Sub PaintLink(ByVal g As Graphics, _
                          ByVal ptStart As Point, ByVal ptEnd As Point, _
                          ByVal clr As Color, ByVal sWeight As Single, _
                          Optional ByVal bExternal As Boolean = False)

        Dim p As Pen = Nothing
        Dim ptStartScaled As New Point(CInt(ptStart.X), CInt(ptStart.Y))
        Dim ptEndScaled As New Point(CInt(ptEnd.X), CInt(ptEnd.Y))

        Dim dx As Integer = ptEndScaled.X - ptStartScaled.X
        Dim dy As Integer = ptEndScaled.Y - ptStartScaled.Y

        ' Get pen to draw with. Zero weight?
        If sWeight = 0 Then
            ' #Yes: render a dotted, thin line
            p = New Pen(clr, 1)
            p.DashStyle = Drawing2D.DashStyle.Dot
        Else
            ' 'No: Render a line of a width representing this weight. Weight is a value between
            '      0 and 1. Pen sizes of this magnitude do not show up well, therefore the actual
            '      pen width is an arbitrary 3 * sWeight to make it look better.
            p = New Pen(clr, sWeight * 2.5!)
        End If

        ' External link?
        If bExternal Then
            ' #Yes: render the line dashed
            p.DashStyle = Drawing2D.DashStyle.Dash
        End If

        p.CustomEndCap = New AdjustableArrowCap(4, 4)
        g.DrawLine(p, ptStartScaled, ptEndScaled)

        p.Dispose()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Paint a unit
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="pt"></param>
    ''' <param name="unit"></param>
    ''' <param name="bSelected"></param>
    ''' <remarks>
    ''' JS 11mar09: method not used, painting still handled by cUnitControl
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub PaintUnit(ByVal g As Graphics, ByVal pt As Point, ByVal unit As cUnit, ByVal bSelected As Boolean, ByVal bHover As Boolean)

        Dim rc As Rectangle = Me.ClientRectangle
        rc.Width -= 1
        rc.Height -= 1

        g.FillRectangle(SystemBrushes.Window, rc)
        If bSelected Then
            Using p As New Pen(Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT))
                g.DrawRectangle(p, rc)
            End Using
        Else
            Using p As New Pen(Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT))
                g.DrawRectangle(Pens.Black, rc)
            End Using
        End If
        Using ft As New Font(Me.Font.FontFamily, CSng(Me.Font.Size * Me.Width / 80), FontStyle.Regular, Me.Font.Unit)
            g.DrawString(unit.Name, ft, SystemBrushes.WindowText, Me.ClientRectangle)
        End Using

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the angle between two points in radians. 0 degrees is straigt up,
    ''' angle rotates clockwise.
    ''' </summary>
    ''' <param name="dx"></param>
    ''' <param name="dy"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function Angle(ByVal dx As Integer, ByVal dy As Integer) As Single

        Dim sHalfPI As Single = CSng(Math.PI / 2)
        Dim sAngle As Single = 0

        If dy = 0 Then
            If dx >= 0 Then
                sAngle = sHalfPI
            Else
                sAngle = 3 * sHalfPI
            End If
        Else
            sAngle = CSng(Math.Atan(dx / -dy))

            ' Find quadrant
            If dy > 0 Then
                sAngle += 2 * sHalfPI
            Else
                If dx < 0 Then
                    sAngle += 4 * sHalfPI
                End If
            End If
        End If

        Return sAngle

    End Function

    Private Sub CheckMissingParameters()

        If Me.EditMode = eEditMode.ReadOnly Then Return

    End Sub

    Private Sub UpdateControls()

    End Sub

#End Region ' Internals

End Class
