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

Imports System.IO
Imports System.Text
Imports EwECore
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style
Imports ZedGraph

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class, wraps a <see cref="ZedGraph">ZedGraph</see> graph control
    ''' to standardize look and feel. Additionally, this class implements 
    ''' generic cursor behaviour on the graph, and provides standardized data 
    ''' export.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)>
    Public Class cZedGraphHelper

#Region " Helper classes "

        ''' <summary>
        ''' cZedGraphHelper internal class, manages curve contextual information.
        ''' </summary>
        Protected Class cCurveInfo

            ' == Contextual properties ==

            Private m_uic As cUIContext = Nothing
            Private m_source As ICoreInterface = Nothing
            Private m_iGroup As Integer = cCore.NULL_VALUE
            Private m_iFleet As Integer = cCore.NULL_VALUE
            Private m_data As Dictionary(Of String, Object)

            ' == Fixed properties ==

            Private m_strLabel As String = ""
            Private m_colour As Color = Color.Aqua
            Private m_lineType As eSketchDrawModeTypes = eSketchDrawModeTypes.NotSet
            Private m_liOffset As LineItem = Nothing

            ' == Status flags ==

            Private m_bGrayedOut As Boolean = False
            Private m_bHighlighted As Boolean = False

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Constructor type 1: manage a curve connected to a core data object.
            ''' </summary>
            ''' <param name="src">Core data source to connect to.</param>
            ''' <param name="uic">UI context to use for colours, visibility, etc.</param>
            ''' <param name="strLabel">Label of the curve. If not provided, the
            ''' curve label is obtained from the core data object.</param>
            ''' ---------------------------------------------------------------
            Public Sub New(ByVal src As ICoreInterface,
                           ByVal uic As cUIContext,
                           Optional ByVal strLabel As String = "",
                           Optional ByVal tag As Object = Nothing)

                ' Sanity checks
                Debug.Assert(src IsNot Nothing)
                Debug.Assert(uic IsNot Nothing)

                Me.m_source = src
                Me.m_uic = uic
                Me.m_strLabel = strLabel
                Me.Data("") = tag

                Dim h As New cTimeSeriesShapeGUIHandler(Me.m_uic)

                If (TypeOf src Is cCoreInputOutputBase) Then
                    If (TypeOf src Is cEcoPathGroupInput) Then
                        Me.m_iGroup = src.Index
                        Me.m_lineType = eSketchDrawModeTypes.Line
                    ElseIf (TypeOf src Is cEcopathFleetInput) Then
                        Me.m_iFleet = src.Index
                        Me.m_lineType = eSketchDrawModeTypes.Line
                    End If
                Else
                    If (TypeOf src Is cGroupTimeSeries) Then
                        Me.m_iGroup = DirectCast(src, cGroupTimeSeries).GroupIndex
                        Me.m_lineType = h.SketchDrawMode(DirectCast(src, cTimeSeries))
                    ElseIf (TypeOf src Is cFleetTimeSeries) Then
                        Me.m_iFleet = DirectCast(src, cFleetTimeSeries).FleetIndex
                        Me.m_lineType = h.SketchDrawMode(DirectCast(src, cTimeSeries))
                    End If
                End If

                ' Post-anaysis sanity checks
                Debug.Assert(Me.m_iGroup <> cCore.NULL_VALUE Or Me.m_iFleet <> cCore.NULL_VALUE)

            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Constructor type 2: manage a curve with custom attributes.
            ''' </summary>
            ''' <param name="strLabel">Label of the curve.</param>
            ''' <param name="colour">Colour of the curve.</param>
            ''' <param name="lineType">Data type of the curve that will determine
            ''' the curve display style.</param>
            ''' ---------------------------------------------------------------
            Public Sub New(ByVal strLabel As String,
                           ByVal colour As Color,
                           ByVal lineType As eSketchDrawModeTypes,
                           Optional ByVal tag As Object = Nothing)

                Me.m_strLabel = strLabel
                Me.m_colour = colour
                Me.m_lineType = lineType
                Me.Data("") = tag

            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="eSketchDrawModeTypes">line type</see> of the curve.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property LineType() As eSketchDrawModeTypes
                Get
                    Return Me.m_lineType
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the label for the curve.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property Label() As String
                Get
                    ' Return overriding name, if any
                    If Not String.IsNullOrEmpty(Me.m_strLabel) Then Return Me.m_strLabel
                    ' Deduct from source
                    If Me.m_source IsNot Nothing Then
                        Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_INDEXED,
                                             Me.m_source.Index, Me.m_source.Name)
                    End If
                    ' Hmm...
                    Return ""
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the index for the curve, if any.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property Index() As Integer
                Get
                    If Me.m_iGroup <> cCore.NULL_VALUE Then Return Me.m_iGroup
                    If Me.m_iFleet <> cCore.NULL_VALUE Then Return Me.m_iFleet
                    Return cCore.NULL_VALUE
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the colour for the curve.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property Colour() As Color
                Get
                    If Me.m_bGrayedOut Then Return Color.LightGray

                    If Me.m_iGroup <> cCore.NULL_VALUE Then Return Me.m_uic.StyleGuide.GroupColor(Me.m_uic.Core, Me.m_iGroup)
                    If Me.m_iFleet <> cCore.NULL_VALUE Then Return Me.m_uic.StyleGuide.FleetColor(Me.m_uic.Core, Me.m_iFleet)
                    Return Me.m_colour
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the visibility state for the curve.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property IsVisible() As Boolean
                Get
                    If Me.m_iGroup <> cCore.NULL_VALUE Then Return Me.m_uic.StyleGuide.GroupVisible(Me.m_iGroup)
                    If Me.m_iFleet <> cCore.NULL_VALUE Then Return Me.m_uic.StyleGuide.FleetVisible(Me.m_iFleet)
                    Return True
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set grayed-out state of a curve.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property IsGrayedOut() As Boolean
                Get
                    Return Me.m_bGrayedOut
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bGrayedOut = value
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set highlight state of a curve.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property IsHighlighted() As Boolean
                Get
                    Return Me.m_bHighlighted
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bHighlighted = value
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the offset line that a given line was added to
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property Offset() As LineItem
                Get
                    Return Me.m_liOffset
                End Get
                Set(ByVal value As LineItem)
                    Me.m_liOffset = value
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get whether this line is cumulative to an <see cref="Offset">offset line</see>.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property IsCumulative() As Boolean
                Get
                    Return (Me.m_liOffset IsNot Nothing)
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set a custom tag for the curve.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property Tag() As Object
                Get
                    Return Me.Data("")
                End Get
                Set(ByVal value As Object)
                    Me.Data("") = value
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set custom data items for this line.
            ''' </summary>
            ''' <param name="strKey">The name of the item to obtain.</param>
            ''' ---------------------------------------------------------------
            Public Property Data(strKey As String) As Object
                Get
                    If (Me.m_data Is Nothing) Then Return Nothing
                    If String.IsNullOrWhiteSpace(strKey) Then strKey = "Default"
                    If (Not Me.m_data.ContainsKey(strKey)) Then Return Nothing
                    Return Me.m_data(strKey)
                End Get
                Set(value As Object)
                    If String.IsNullOrWhiteSpace(strKey) Then strKey = "Default"
                    If (value Is Nothing) Then
                        If (Me.m_data Is Nothing) Then Return
                        If (Not Me.m_data.ContainsKey(strKey)) Then Return
                        Me.m_data.Remove(strKey)
                    Else
                        If (Me.m_data Is Nothing) Then Me.m_data = New Dictionary(Of String, Object)
                        Me.m_data(strKey) = value
                    End If
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns whether this info references a given core input/output item.
            ''' </summary>
            ''' <param name="src">The core input/output item to test.</param>
            ''' <returns>True if true. Well, that is a surprise.</returns>
            ''' ---------------------------------------------------------------
            Public Function IsReferenceTo(ByVal src As cCurveInfo) As Boolean
                Return ReferenceEquals(src.m_source, Me.m_source)
            End Function

        End Class

#End Region ' Helper classes

#Region " Private vars "

        Private m_uic As cUIContext = Nothing

        ''' <summary>Wrapped ZedGraph control.</summary>
        Private m_zgc As ZedGraphControl = Nothing
        ''' <summary>Number of panels wanted in the zed graph</summary>
        Private m_nPanels As Integer = 1
        ''' <summary>Registered axis that need to display units.</summary>
        Private m_dtAxisLabels As New Dictionary(Of Axis, String)
        ''' <summary>Grace value for manual scaled Y-axis.</summary>
        Private m_sYScaleGrace As Single = 0.1!

        ' == Legend ==
        ''' <summary>States whether this instance should show a legend if left to 'default'</summary>
        Private m_bShowLegend As Boolean = True

        '== Axis labels ==
        ''' <summary>States whether this instance should show axis labels.</summary>
        Private m_bShowAxisLabels As Boolean = True

        ' == Cursor ==
        Private m_abShowCursor() As Boolean
        Private m_asCursorPos() As Single
        Private m_aliCursors() As LineItem

        ' == Cumulative ==
        Private m_bCumulative() As Boolean

        ' == Visibility tracking ==

        ''' <summary>Flag stating whether styleguide item visibility should be tracked.</summary>
        Private m_bTrackVisibility As Boolean = True

        ' == Context menu ==
        ' Menu items to add to the context menu. The menu items are member vars so eventhandlers 
        ' can be properly detached preventing memory leaks.

        ''' <summary>Enumerated type defining supported max and min auto scale options.</summary>
        Public Enum eScaleOptionTypes As Byte
            MaxOnly
            MinOnly
            Both
            None
        End Enum

        ' == Hover menu ==

        ''' <summary>Enumerated type defining supported hoover menu commands.</summary>
        Protected Enum eHoverCommands As Integer
            Zoomin
            Zoomout
            ZoomReset
            ShowLegend
            ShowLabels
            ExportToCSV
        End Enum

        ''' <summary>Hover menu state flag.</summary>
        Private m_bShowHoverMenu As Boolean = True

        ''' <summary>The hover menu to display on top of graph areas.</summary>
        Private m_hovermenu As ucHoverMenu = Nothing

#End Region ' Private vars

#Region " Construction / destruction "

        Public Sub New()
        End Sub

        Protected Overrides Sub Finalize()
            ' Check proper usage
            If (Me.IsAttached) Then
                Try
                    Me.Detach()
                Catch ex As Exception
                    Debug.Assert(False)
                End Try
            End If
            ' Go ahead
            MyBase.Finalize()
        End Sub

#End Region ' Construction / destruction

#Region " Selection "

        Public Event OnCurveClicked(ByVal curve As CurveItem, ByVal iPoint As Integer)

#End Region ' Selection

#Region " Public interfaces "

        ''' <summary>
        ''' Diagnostics, returns whether the helper is currently attached to an
        ''' existing graph.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function IsAttached() As Boolean
            Return Me.m_zgc IsNot Nothing
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attach a zedgraph helper to a zedgraph control.
        ''' </summary>
        ''' <param name="uic"><see cref="cUIContext"/> providing UI contextual information.</param>
        ''' <param name="zgc"><see cref="ZedGraphControl"/> to style and interact with.</param>
        ''' <param name="iNumPanels">Number of panels to create.</param>
        ''' <remarks>
        ''' Make sure to cleanup using <see cref="Detach">Detach</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Attach(ByVal uic As cUIContext,
                                      ByVal zgc As ZedGraphControl,
                                      Optional ByVal iNumPanels As Integer = 1)

            ' Sanity checks
            Debug.Assert(uic IsNot Nothing)
            Debug.Assert(zgc IsNot Nothing)

            If Me.m_zgc IsNot Nothing Then Me.Detach()

            Me.m_uic = uic
            Me.m_zgc = zgc
            Me.m_nPanels = iNumPanels

            Me.ChangeNumPanels()

            AddHandler Me.m_zgc.MouseDownEvent, AddressOf OnMouseDownEvent
            AddHandler Me.m_zgc.MouseMoveEvent, AddressOf OnMouseMoveEvent
            AddHandler Me.m_zgc.MouseUpEvent, AddressOf OnMouseUpEvent
            AddHandler Me.m_zgc.PointValueEvent, AddressOf OnPointValueEvent

            AddHandler Me.m_zgc.ContextMenuBuilder, AddressOf OnBuildContextMenu

            AddHandler Me.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged

            Me.AllowZoom = True
            Me.AllowPan = False

            ' Configure graph control
            Me.UpdateStyle()
            Me.UpdateColours()

            ' Kick the hover menu. Kick.
            Me.ShowHoverMenu = Me.ShowHoverMenu

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Detach a zedgraph control that was previously 
        ''' <see cref="Attach">attached</see>.
        ''' </summary>
        ''' <remarks>
        ''' Failing to detach an attached control causes memory leaks.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Detach()

            If (Me.m_zgc Is Nothing) Then Return

            RemoveHandler Me.m_zgc.MouseDownEvent, AddressOf OnMouseDownEvent
            RemoveHandler Me.m_zgc.MouseMoveEvent, AddressOf OnMouseMoveEvent
            RemoveHandler Me.m_zgc.MouseUpEvent, AddressOf OnMouseUpEvent
            RemoveHandler Me.m_zgc.ContextMenuBuilder, AddressOf OnBuildContextMenu
            RemoveHandler Me.m_zgc.PointValueEvent, AddressOf OnPointValueEvent

            RemoveHandler Me.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged

            Me.ShowHoverMenu = False

            Me.m_dtAxisLabels.Clear()

            Me.m_zgc = Nothing
            Me.m_nPanels = 0 ' To ensure that avid child control detaches do not stumble

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of panels in the <see cref="ZedGraph">graph</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property NumPanes() As Integer
            Get
                Return Me.m_nPanels
            End Get
            Set(ByVal value As Integer)
                Me.m_nPanels = value
                Me.ChangeNumPanels()
            End Set
        End Property

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the legend is allowed to show multiple items for
        ''' a single group or fleet.
        ''' </summary>
        ''' <remarks>
        ''' Note that this setting only affects newly added lines; existing
        ''' graph content will not be affected.
        ''' </remarks>
        ''' ---------------------------------------------------------------
        Public Property AllowDuplicatesOnLegend As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cStyleGuide">style guide</see> attached to this
        ''' instance.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property StyleGuide() As cStyleGuide
            Get
                Return Me.m_uic.StyleGuide
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCore">core</see> attached to this instance.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Core() As cCore
            Get
                Return Me.m_uic.Core
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="ZedGraphControl">graph</see> attached to this instance.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Graph() As ZedGraphControl
            Get
                Return Me.m_zgc
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cUIContext"/> attached to this instance.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property UIContext() As cUIContext
            Get
                Return Me.m_uic
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a graph pane.
        ''' </summary>
        ''' <param name="iPane">The one-based index of the pane to return. This 
        ''' index should be between 1 and <see cref="NumPanes">NumPanes</see>.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function GetPane(ByVal iPane As Integer) As ZedGraph.GraphPane

            Dim pane As GraphPane = Nothing

            If Me.m_nPanels = 1 Then pane = Me.m_zgc.GraphPane
            pane = Me.m_zgc.MasterPane.PaneList(iPane - 1)

            Debug.Assert(pane IsNot Nothing, "ZedGraphHelper already disconnected")

            Return pane

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns one-based index of a pane.
        ''' </summary>
        ''' <param name="pane">The pane to obtain the index for.</param>
        ''' <returns>The one-based index of a pane.</returns>
        ''' -------------------------------------------------------------------
        Public Function GetPaneIndex(ByVal pane As ZedGraph.GraphPane) As Integer

            For i As Integer = 1 To Me.m_zgc.MasterPane.PaneList.Count
                If Object.Equals(pane, Me.GetPane(i)) Then Return i
            Next
            Return 1

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure main panel
        ''' </summary>
        ''' <param name="strTitle">The title to set to the master pane.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Configure(ByVal strTitle As String)

            With Me.m_zgc.MasterPane
                .Title.Text = strTitle
                .Title.IsVisible = Not String.IsNullOrEmpty(strTitle)
            End With

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure a single <see cref="GraphPane">pane</see> in the graph.
        ''' </summary>
        ''' <param name="strTitle">Title for the pane.</param>
        ''' <param name="strXAxisLabel">Label for the X-axis.</param>
        ''' <param name="dXAxisMin">X-axis min scale.</param>
        ''' <param name="dXAxisMax">X-axis max scale.</param>
        ''' <param name="strYAxisLabel">Label for the Y-axis.</param>
        ''' <param name="dYAxisMin">Y-axis min scale.</param>
        ''' <param name="dYAxisMax">Y-axis max scale.</param>
        ''' <param name="bShowLegend">Flag stating whether the legend should be shown.</param>
        ''' <param name="legendPos">Legend <see cref="LegendPos">position</see>.</param>
        ''' <param name="iPane">The pane to configure. If not specified, the main pane
        ''' is configured.</param>
        ''' <returns>The configured <see cref="GraphPane">GraphPane</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function ConfigurePane(ByVal strTitle As String,
                                                  ByVal strXAxisLabel As String, ByVal dXAxisMin As Double, ByVal dXAxisMax As Double,
                                                  ByVal strYAxisLabel As String, ByVal dYAxisMin As Double, ByVal dYAxisMax As Double,
                                                  ByVal bShowLegend As Boolean, Optional ByVal legendPos As LegendPos = LegendPos.TopCenter,
                                                  Optional ByVal iPane As Integer = 1) As GraphPane

            Dim gp As GraphPane = Me.ConfigurePane(strTitle,
                                                   strXAxisLabel,
                                                   strYAxisLabel,
                                                   bShowLegend, legendPos, iPane)
            With gp

                .XAxis.Scale.Min = dXAxisMin
                .XAxis.Scale.MinGrace = 0.0#
                .XAxis.Scale.MaxGrace = 0.0#
                If dXAxisMin <> dXAxisMax Then .XAxis.Scale.Max = dXAxisMax

                .YAxis.Scale.Min = dYAxisMin
                .YAxis.Scale.MinGrace = Me.m_sYScaleGrace
                .YAxis.Scale.MaxGrace = Me.m_sYScaleGrace
                If dYAxisMin <> dYAxisMax Then .YAxis.Scale.Max = dYAxisMax

            End With
            Me.RescaleAndRedraw()

            Return gp

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure a single <see cref="GraphPane">pane</see> in the graph.
        ''' </summary>
        ''' <param name="strTitle">Title for the pane.</param>
        ''' <param name="strXAxisLabel">Label for the X-axis.</param>
        ''' <param name="strYAxisLabel">Label for the Y-axis.</param>
        ''' <param name="bShowLegend">Flag stating whether the legend should be shown.</param>
        ''' <param name="legendPos">Legend <see cref="LegendPos">position</see>.</param>
        ''' <param name="iPane">The pane to configure. If not specified, the main pane
        ''' is configured.</param>
        ''' <returns>The configured <see cref="GraphPane">GraphPane</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function ConfigurePane(ByVal strTitle As String,
                                                  ByVal strXAxisLabel As String,
                                                  ByVal strYAxisLabel As String,
                                                  ByVal bShowLegend As Boolean, Optional ByVal legendPos As LegendPos = LegendPos.TopCenter,
                                                  Optional ByVal iPane As Integer = 1) As GraphPane

            Me.m_bShowLegend = bShowLegend

            Dim gp As GraphPane = Me.GetPane(iPane)
            With gp

                ' Set title
                .Title.Text = strTitle
                .Title.IsVisible = Not String.IsNullOrEmpty(strTitle)

                ' Configure axis
                Me.AxisLabel(.XAxis, strXAxisLabel)
                .XAxis.Title.IsVisible = Not String.IsNullOrEmpty(strXAxisLabel)
                .XAxis.MinorTic.IsAllTics = False
                .XAxis.MinorTic.IsOpposite = False
                .XAxis.MajorTic.IsOpposite = False
                .XAxis.Scale.MinGrace = 0.0#
                .XAxis.Scale.MaxGrace = 0.0#

                Me.AxisLabel(.YAxis, strYAxisLabel)
                .YAxis.Title.IsVisible = Not String.IsNullOrEmpty(strYAxisLabel)
                .YAxis.MinorTic.IsAllTics = False
                .YAxis.MinorTic.IsOpposite = False
                .YAxis.MajorTic.IsOpposite = False
                .YAxis.Scale.MinGrace = Me.m_sYScaleGrace
                .YAxis.Scale.MaxGrace = Me.m_sYScaleGrace

                .Legend.Position = legendPos

                .Border.IsVisible = False
                .Chart.Border.IsVisible = True
                .Chart.Border.Color = Color.FromArgb(16, 0, 0, 0)

                Me.UpdateLegends(gp)
                Me.UpdateAxisLabels(gp)

            End With

            Me.UpdateStyle(iPane)
            Me.RescaleAndRedraw()

            Return gp

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a series of lines to the ZedGraph.
        ''' </summary>
        ''' <param name="lines">The <see cref="LineItem">lines</see> to add.</param>
        ''' <param name="iPane">The panel to assign these lines to (optional).</param>
        ''' <param name="bRescale">Flag stating whether the graph needs to be
        ''' rescaled (optional).</param>
        ''' <remarks>Note that this method clears out all lines existing in the
        ''' indicated panel.</remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Sub PlotLines(ByVal lines() As LineItem,
                                         Optional ByVal iPane As Integer = 1,
                                         Optional ByVal bRescale As Boolean = True,
                                         Optional ByVal bClear As Boolean = True,
                                         Optional ByVal bCumulative As Boolean = False)
            Try

                If (Me.IsPaneCumulative(iPane) <> bCumulative) Then
                    bClear = True
                    Me.IsPaneCumulative(iPane) = bCumulative
                End If

                Dim li As LineItem = Nothing
                Dim linetype As eSketchDrawModeTypes = eSketchDrawModeTypes.NotSet

                With Me.GetPane(iPane)

                    If bClear Then .CurveList.Clear()

                    If (lines IsNot Nothing) Then
                        For i As Integer = 0 To lines.Length - 1
                            ' Get line
                            li = lines(i)
                            ' Just to make sure
                            If (li IsNot Nothing) Then

                                ' Has no line title?
                                If String.IsNullOrEmpty(li.Label.Text) Then
                                    ' #Yes: use pane title to identify line
                                    li.Label.Text = .Title.Text
                                End If

#If DEBUG Then
                                '' Validate line content
                                'For ipt As Integer = 0 To li.Points.Count - 1
                                '   Dim pt As PointPair = li.Points(ipt)
                                '   Debug.Assert(cNumberUtils.IsFinite(CSng(pt.X)) And cNumberUtils.IsFinite(CSng(pt.Y)), "Point contains infinite values")
                                'Next
#End If
                                Select Case Me.CurveType(li)

                                    Case eSketchDrawModeTypes.Line, eSketchDrawModeTypes.Fill

                                        If Me.IsPaneCumulative(iPane) Then

                                            ' Note that this code assumes that every line added here has the 
                                            ' exact number of points in the exact same X-axis order. No validations 
                                            ' are performed whether this is indeed the case

                                            ' ZedGraph renders curvelists last to first. Higher cumulative curves are
                                            ' thus stored with increasing indices in the list
                                            Dim iLastLine As Integer = Me.FindLastCurvePos(eSketchDrawModeTypes.Line, iPane)

                                            If (iLastLine > -1) Then
                                                Me.SumLines(DirectCast(.CurveList(iLastLine), LineItem), li)
                                            End If

                                            ' Set cumulative colour style
                                            li.Line.Fill = New Fill(li.Color)
                                            li.Line.Fill.IsVisible = True
                                            li.Line.Color = Color.SlateGray

                                            ' Add the curve to the end 
                                            .CurveList.Insert(iLastLine + 1, li)
                                        Else
                                            ' Extract info
                                            Dim info As cCurveInfo = Me.CurveInfo(li)
                                            If (info IsNot Nothing) Then
                                                ' #1172: hide duplicate legend items
                                                li.Label.IsVisible = li.Label.IsVisible And
                                                                     ((Me.ContainsCurve(info, iPane) = False) Or (Me.AllowDuplicatesOnLegend = True))
                                            End If
                                            ' Add curve
                                            .CurveList.Add(li)
                                        End If

                                    Case eSketchDrawModeTypes.TimeSeriesDriver,
                                         eSketchDrawModeTypes.TimeSeriesRefAbs,
                                         eSketchDrawModeTypes.TimeSeriesRefRel

                                        ' Reference curves should be rendered on top of everything else.
                                        .CurveList.Insert(0, li)

                                    Case eSketchDrawModeTypes.NotSet

                                        ' Unknow data type: just append curve to end of the list
                                        .CurveList.Add(li)

                                End Select
                            Else
                                Debug.Assert(False)
                            End If
                        Next i

                    End If
                End With

                If bRescale Then Me.RescaleAndRedraw(iPane) Else Me.Redraw()

            Catch ex As Exception
                cLog.Write(ex)
            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Redraw the wrapped ZedGraph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Redraw()
            Me.m_zgc.Invalidate()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Totally redraw the wrapped ZedGraph by recalculating the axis and 
        ''' invalidating all panels.
        ''' </summary>
        ''' <param name="iPane">The pane to rescale and redraw, or -1 to
        ''' update all panes in the graph.</param>
        ''' <remarks>When using cursors please use this method to rescale the
        ''' graph axis.</remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Sub RescaleAndRedraw(Optional ByVal iPane As Integer = -1)

            Dim iMin As Integer = 1
            Dim iMax As Integer = Me.m_nPanels
            Dim abCursor(Me.m_nPanels) As Boolean

            If iPane > -1 Then
                iMin = Math.Max(iMin, iPane)
                iMax = Math.Min(iMax, iPane)
            End If

            ' Hide cursors, but remember settings
            For iPane = iMin To iMax
                abCursor(iPane) = Me.ShowCursor(iPane)
                Me.ShowCursor(iPane) = False
                With Me.GetPane(iPane).YAxis.Scale
                    .MaxGrace = Me.YScaleGrace
                    .MinGrace = Me.YScaleGrace
                End With
                With Me.GetPane(iPane).XAxis.Scale
                    .MaxGrace = 0
                    .MinGrace = 0
                End With
            Next

            ' Restore cursors
            For iPane = iMin To iMax
                Me.ShowCursor(iPane) = abCursor(iPane)
            Next

            'Me.Redraw()
            Me.m_zgc.BeginInvoke(New MethodInvoker(AddressOf Me.DoRescaleAndRedraw))

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether Y values in a given pane should be auto-scaled.
        ''' </summary>
        ''' <param name="iPane"></param>
        ''' -------------------------------------------------------------------
        Public Property AutoscalePane(Optional ByVal iPane As Integer = 1) As Boolean
            Get
                With Me.GetPane(iPane).YAxis.Scale
                    Return .MaxAuto And .MinAuto
                End With
            End Get
            Set(ByVal bAutoscale As Boolean)
                With Me.GetPane(iPane).YAxis.Scale
                    .MinAuto = bAutoscale
                    .MaxAuto = bAutoscale
                    Me.RescaleAndRedraw(iPane)
                End With
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the auto-scale behaviour of a pane Y axis.
        ''' </summary>
        ''' <param name="iPane"></param>
        ''' -------------------------------------------------------------------
        Public Property AutoScaleYOption(Optional ByVal iPane As Integer = 1) As eScaleOptionTypes
            Get
                With Me.GetPane(iPane).YAxis.Scale
                    If .MinAuto And .MaxAuto Then
                        Return eScaleOptionTypes.Both
                    ElseIf .MaxAuto And Not .MinAuto Then
                        Return eScaleOptionTypes.MaxOnly
                    ElseIf Not .MaxAuto And .MinAuto Then
                        Return eScaleOptionTypes.MinOnly
                    ElseIf Not .MaxAuto And Not .MinAuto Then
                        Return eScaleOptionTypes.None
                    End If
                    Return eScaleOptionTypes.None
                End With
            End Get
            Set(ByVal value As eScaleOptionTypes)
                Dim gp As GraphPane = Me.GetPane(iPane)
                With gp.YAxis.Scale
                    Select Case value
                        Case eScaleOptionTypes.Both
                            .MinAuto = True
                            .MaxAuto = True
                        Case eScaleOptionTypes.MaxOnly
                            .MaxAuto = True
                            .MinAuto = False
                        Case eScaleOptionTypes.MinOnly
                            .MaxAuto = False
                            .MinAuto = True
                        Case eScaleOptionTypes.None
                            .MinAuto = False
                            .MaxAuto = False
                    End Select
                    gp.ZoomStack.Clear()
                    Me.RescaleAndRedraw(iPane)
                End With
            End Set
        End Property

        Public Property YScaleMin(Optional ByVal iPane As Integer = 1) As Double
            Get
                Return Me.GetPane(iPane).YAxis.Scale.Min
            End Get
            Set(ByVal value As Double)
                Me.GetPane(iPane).YAxis.Scale.Min = value
                Me.RescaleAndRedraw(iPane)
            End Set
        End Property

        Public Property XScaleMax(Optional ByVal iPane As Integer = 1) As Double
            Get
                Return Me.GetPane(iPane).XAxis.Scale.Max
            End Get
            Set(ByVal value As Double)
                Dim scale As Scale = Me.GetPane(iPane).XAxis.Scale
                ' Fudge
                If (scale.Min = value) Then value += 1
                ' Apply
                scale.Max = value
                Me.RescaleAndRedraw(iPane)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the grace margin for Y axis.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property YScaleGrace() As Single
            Get
                Return Me.m_sYScaleGrace
            End Get
            Set(ByVal value As Single)
                Me.m_sYScaleGrace = value
                Me.RescaleAndRedraw()
            End Set
        End Property

        Public Property XScaleMin(Optional ByVal iPane As Integer = 1) As Double
            Get
                Return Me.GetPane(iPane).XAxis.Scale.Min
            End Get
            Set(ByVal value As Double)
                'Me.AutoScaleYOption(iPane) = eScaleOptionTypes.None ' WHy is the Y axis affected here?
                Dim scale As Scale = Me.GetPane(iPane).XAxis.Scale
                If (scale.Max = value) Then
                    scale.Max += 1
                End If
                scale.Min = value
                Me.RescaleAndRedraw(iPane)
            End Set
        End Property

        Public Property YScaleMax(Optional ByVal iPane As Integer = 1) As Double
            Get
                Return Me.GetPane(iPane).YAxis.Scale.Max
            End Get
            Set(ByVal value As Double)
                Me.AutoScaleYOption(iPane) = eScaleOptionTypes.None
                Me.GetPane(iPane).YAxis.Scale.Max = value
                Me.RescaleAndRedraw(iPane)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set whether vetical zoom is allowed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property AllowZoom() As Boolean
            Set(ByVal value As Boolean)
                Me.m_zgc.IsZoomOnMouseCenter = value
                Me.m_zgc.IsEnableVZoom = value
                Me.m_zgc.IsEnableHZoom = False
                Me.m_zgc.IsEnableWheelZoom = value
                Me.m_zgc.IsEnableZoom = value
                If value Then
                    Me.m_zgc.ZoomButtons = MouseButtons.Left
                Else
                    Me.m_zgc.ZoomButtons = MouseButtons.None
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set whether vetical pan is allowed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property AllowPan() As Boolean
            Set(ByVal value As Boolean)
                Me.m_zgc.IsEnableVPan = value
                Me.m_zgc.IsEnableHPan = False
                If value Then
                    Me.m_zgc.PanButtons = MouseButtons.Left
                Else
                    Me.m_zgc.EditButtons = MouseButtons.None
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set whether value edits are allowed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property AllowEdit() As Boolean
            Set(ByVal value As Boolean)
                Me.m_zgc.IsEnableHEdit = value
                Me.m_zgc.IsEnableVEdit = value
                If value Then
                    Me.m_zgc.EditButtons = MouseButtons.Right
                Else
                    Me.m_zgc.EditButtons = MouseButtons.None
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the values in a pane should be added cumulatively.
        ''' </summary>
        ''' <param name="iPane"></param>
        ''' -------------------------------------------------------------------
        Public Property IsPaneCumulative(Optional ByVal iPane As Integer = 1) As Boolean
            Get
                Return Me.m_bCumulative(iPane)
            End Get
            Protected Set(ByVal value As Boolean)
                Me.m_bCumulative(iPane) = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the graph tracks styleguide item visibility settings.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IsTrackVisiblity() As Boolean
            Get
                Return Me.m_bTrackVisibility
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bTrackVisibility) Then
                    value = Me.m_bTrackVisibility
                    Me.UpdateCurveVisibility()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether to display legends.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IsLegendVisible() As Boolean
            Get
                Dim gp As GraphPane = Me.GetPane(1)
                Return gp.Legend.IsVisible
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bShowLegend) Then
                    Me.m_bShowLegend = value
                    For i As Integer = 1 To Me.m_nPanels
                        Dim gp As GraphPane = Me.GetPane(i)
                        Me.UpdateLegends(gp)
                    Next
                    Me.m_zgc.AxisChange()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether to display axis labels.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IsAxisLabelsVisible() As Boolean
            Get
                Return Me.m_bShowAxisLabels
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bShowAxisLabels) Then
                    Me.m_bShowAxisLabels = value
                    Me.UpdateAxisLabels()
                    Me.m_zgc.AxisChange()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an EwE-styled line for either an Ecopath group or fleet.
        ''' </summary>
        ''' <param name="ppl"></param>
        ''' <remarks>All other source types will be rejected.</remarks>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function CreateLineItem(ByVal src As ICoreInterface,
                                                   ByVal ppl As PointPairList,
                                                   Optional ByVal strLabel As String = "",
                                                   Optional ByVal tag As Object = Nothing) As LineItem
            ' SAnity check
            Debug.Assert(TypeOf (src) Is cCoreGroupBase Or TypeOf (src) Is cEcopathFleetInput Or
                         TypeOf (src) Is cGroupTimeSeries Or TypeOf (src) Is cFleetTimeSeries)
            Return Me.CreateLineItem(New cCurveInfo(src, Me.m_uic, strLabel, tag), ppl)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an EwE-styled line.
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="curveType"></param>
        ''' <param name="clr"></param>
        ''' <param name="ppl"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function CreateLineItem(ByVal strName As String,
                                                   ByVal curveType As eSketchDrawModeTypes,
                                                   ByVal clr As Color,
                                                   ByVal ppl As PointPairList,
                                                   Optional ByVal tag As Object = Nothing) As LineItem
            Return Me.CreateLineItem(New cCurveInfo(strName, clr, curveType, tag), ppl)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the <see cref="eSketchDrawModeTypes">type</see> of a curve.
        ''' </summary>
        ''' <param name="ci">The curve to extract information for.</param>
        ''' <returns>A <see cref="eSketchDrawModeTypes">type</see>, or NotSet if this 
        ''' information could not be found.</returns>
        ''' <remarks>
        ''' Note that this information only works on curves created via 
        ''' <see cref="CreateLineItem">CreateLineItem</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function CurveType(ByVal ci As CurveItem) As eSketchDrawModeTypes
            Dim info As cCurveInfo = Me.CurveInfo(ci)
            If (info Is Nothing) Then Return eSketchDrawModeTypes.Line
            Return info.LineType
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the <see cref="cCurveInfo">curve info</see> for a curve.
        ''' </summary>
        ''' <param name="ci">The curve to extract information for.</param>
        ''' <returns>
        ''' A <see cref="cCurveInfo">curve info</see> instance, or Nothing if
        ''' this information could not be found.
        ''' </returns>
        ''' <remarks>
        ''' Note that this information only works on curves created via 
        ''' <see cref="CreateLineItem">CreateLineItem</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Function CurveInfo(ByVal ci As CurveItem) As cCurveInfo
            If (ci Is Nothing) Then Return Nothing
            If (ci.Tag Is Nothing) Then Return Nothing
            If Not (TypeOf ci.Tag Is cCurveInfo) Then Return Nothing
            Return DirectCast(ci.Tag, cCurveInfo)
        End Function

#Region " Line metadata "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set optional metadata for lines created with <see cref="CreateLineItem"/>.
        ''' </summary>
        ''' <param name="ci">The line to access metadata for.</param>
        ''' <param name="strKey">The name of the data to access metadata for. Can
        ''' be left empty.</param>
        ''' -------------------------------------------------------------------
        Public Property Metadata(ByVal ci As CurveItem, Optional ByVal strKey As String = "") As Object
            Get
                Dim info As cCurveInfo = Me.CurveInfo(ci)
                If (info IsNot Nothing) Then Return info.Data(strKey)
                Return Nothing
            End Get
            Set(value As Object)
                Dim info As cCurveInfo = Me.CurveInfo(ci)
                If (info IsNot Nothing) Then
                    info.Data(strKey) = value
                Else
                    Debug.Assert(False, "Metadata only allowed on curves created with CurveItem")
                End If
            End Set
        End Property

#End Region ' Line metadata

#Region " Tooltip "

        Private Function OnPointValueEvent(ByVal sender As Object, ByVal pane As GraphPane, ByVal curve As CurveItem, ByVal iPoint As Integer) As String
            Dim strTooltip As String = ""
            Try
                strTooltip = Me.FormatTooltip(pane, curve, iPoint)
            Catch ex As Exception
                ' Whoa!
            End Try
            Return strTooltip
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the graph should display a value tooltip when 
        ''' hovering with the mouse over a pane.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowPointValue() As Boolean
            Get
                Return Me.m_zgc.IsShowPointValues
            End Get
            Set(ByVal value As Boolean)
                Me.m_zgc.IsShowPointValues = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Format the text for the point value tool tip for a given hover 
        ''' location. The tooltip text includes the title of the given pane and 
        ''' a value for a point in the graph.
        ''' </summary>
        ''' <param name="pane">The <see cref="GraphPane"/> that contains the mouse location.</param>
        ''' <param name="curve">The <see cref="CurveItem"/> that the mouse is hovering over.</param>
        ''' <param name="iPoint">The point on the curve that the mouse is hovering over.</param>
        ''' <returns>A formatted tooltip text.</returns>
        ''' <remarks>Override this method to alter the entire tooltip. If you
        ''' are only want to customize the value component of the tooltip (but 
        ''' want to leave the pane title component intact) just override 
        ''' <see cref="FormatTooltipValue"/>.</remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Function FormatTooltip(ByVal pane As GraphPane, ByVal curve As CurveItem, ByVal iPoint As Integer) As String

            Dim sb As New StringBuilder()
            If Not String.IsNullOrEmpty(pane.Title.Text) Then
                sb.Append(pane.Title.Text)
            End If
            Dim strValueBit As String = Me.FormatTooltipValue(pane, curve, iPoint)
            If Not String.IsNullOrEmpty(strValueBit) Then
                If sb.Length > 0 Then sb.AppendLine("")
                sb.Append(strValueBit)
            End If
            Return sb.ToString

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' For the the value component of the tooltip for a given hover location.
        ''' </summary>
        ''' <param name="pane">The <see cref="GraphPane"/> that contains the mouse location.</param>
        ''' <param name="curve">The <see cref="CurveItem"/> that the mouse is hovering over.</param>
        ''' <param name="iPoint">The point on the curve that the mouse is hovering over.</param>
        ''' <returns>A formatted tooltip value text.</returns>
        ''' <remarks>Override this method to customize the value component of the 
        ''' tooltip text. If you want to modify the entire tooltip you should
        ''' override <see cref="FormatTooltip"/> instead.</remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Function FormatTooltipValue(ByVal pane As GraphPane, ByVal curve As CurveItem, ByVal iPoint As Integer) As String

            If curve.IsLine Then

                ' Suppress value part for cumulative panes
                If Me.IsPaneCumulative(Me.GetPaneIndex(pane)) Then
                    Return curve.Label.Text
                End If

                Dim pp As PointPair = curve(iPoint)
                Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_POINT,
                                     curve.Label.Text,
                                     Me.StyleGuide.FormatNumber(pp.X),
                                     Me.StyleGuide.FormatNumber(pp.Y))
            ElseIf curve.IsPie Then
                Dim slice As PieItem = DirectCast(curve, PieItem)
                Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED,
                                     slice.Label.Text,
                                     Me.StyleGuide.FormatNumber(slice.Value))
            End If
            Return ""

        End Function

#End Region ' Tooltip

#Region " Axis label management "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the axix labels, substituting units where possible.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub RefreshAxisLabels()
            Dim unit As New cUnits(Me.UIContext.Core)
            For Each axis As Axis In Me.m_dtAxisLabels.Keys
                axis.Title.Text = unit.ToString(Me.m_dtAxisLabels(axis))
            Next
            Me.m_zgc.Refresh()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set or clear an axis label.
        ''' </summary>
        ''' <param name="axis"></param>
        ''' <param name="strLabel"></param>
        ''' -------------------------------------------------------------------
        Public Sub AxisLabel(ByVal axis As Axis, ByVal strLabel As String)

            If String.IsNullOrWhiteSpace(strLabel) Then
                Try
                    Me.m_dtAxisLabels.Remove(axis)
                Catch ex As Exception
                End Try
            Else
                Me.m_dtAxisLabels(axis) = strLabel
                Dim unit As New cUnits(Me.UIContext.Core)
                axis.Title.Text = unit.ToString(strLabel)
            End If

            axis.Scale.IsUseTenPower = True
            axis.Scale.MagAuto = Not String.IsNullOrEmpty(strLabel)

        End Sub

#End Region ' Axis label management

#Region " Cursor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event for public consumption called when the cursor position changes
        ''' in a graph pane.
        ''' </summary>
        ''' <param name="zgh">The zed graph helper that sent out the event.</param>
        ''' <param name="iPane">The index of the pane that the cursor change event
        ''' pertains to.</param>
        ''' <param name="sPos">The new cursor position.</param>
        ''' -------------------------------------------------------------------
        Public Event OnCursorPos(ByVal zgh As cZedGraphHelper, ByVal iPane As Integer, ByVal sPos As Single)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Show or hide a vertical cursor to a graph pane.
        ''' </summary>
        ''' <param name="iPane">The index of the pane to show or hide the cursor for.</param>
        ''' <remarks>
        ''' <para>Note that ZedGraph does not support a real cursor. Instead, 
        ''' cursors are simulated by manually setting a vertical line in the 
        ''' pane, which will conflict with the Y-axis autoscale ability.</para>
        ''' <para>You will need to manually remove and restore the cursor
        ''' around <see cref="ZedGraphControl.AxisChange">AxisChange</see> 
        ''' events. The ZedGraphHelper method 
        ''' <see cref="RescaleAndRedraw">RescaleAndRedraw</see> performs this
        ''' for you.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property ShowCursor(Optional ByVal iPane As Integer = 1) As Boolean
            Get
                Return Me.m_abShowCursor(iPane)
            End Get
            Set(ByVal value As Boolean)
                Dim gp As GraphPane = Me.GetPane(iPane)
                If (value <> Me.m_abShowCursor(iPane)) Then
                    Me.RemoveCursor(iPane)
                    Me.m_abShowCursor(iPane) = value
                    Me.SetCursor(iPane)
                End If
                Me.m_zgc.IsEnableZoom = (Me.m_abShowCursor(iPane) = False)
                Me.m_zgc.Cursor = If(value, Cursors.Hand, Cursors.Default)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the cursor position for a graph pane.
        ''' </summary>
        ''' <param name="iPane">The pane to access the cursor position for.</param>
        ''' <remarks>Note that this will not make a cursor appear. The cursor
        ''' visibility state should be set with 
        ''' <see cref="ShowCursor">ShowCursor</see> first.</remarks>
        ''' -------------------------------------------------------------------
        Public Property CursorPos(Optional ByVal iPane As Integer = 1) As Single
            Get
                Return Me.m_asCursorPos(iPane)
            End Get
            Set(ByVal value As Single)
                If (value <> Me.m_asCursorPos(iPane)) Then
                    Me.RemoveCursor(iPane)
                    If value <> Me.m_asCursorPos(iPane) Then
                        Me.m_asCursorPos(iPane) = value
                        RaiseEvent OnCursorPos(Me, iPane, value)
                    End If
                    Me.SetCursor(iPane)
                End If
            End Set
        End Property

#End Region ' Cursor

#Region " Data extraction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get all tagged data lines.
        ''' from a graph pane.
        ''' </summary>
        ''' <param name="iPane">The index of the graph pane to check.</param>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property DataLines(Optional ByVal iPane As Integer = 1) As CurveItem()
            Get
                Dim gp As GraphPane = Me.GetPane(iPane)
                Dim lLines As New List(Of CurveItem)
                Dim info As cCurveInfo = Nothing
                Dim bIncludeCurve As Boolean = True

                If gp IsNot Nothing Then
                    ' For each curve
                    For Each ci As CurveItem In gp.CurveList
                        ' Get curve info
                        info = Me.CurveInfo(ci)
                        bIncludeCurve = True
                        If (info IsNot Nothing) Then
                            bIncludeCurve = (info.LineType <> eSketchDrawModeTypes.NotSet)
                        End If
                        If bIncludeCurve Then
                            lLines.Add(ci)
                        End If
                    Next

                End If
                Return lLines.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the data in a given graph pane is sequential, e.g. if
        ''' X axis values steadily increment from first to last point.
        ''' </summary>
        ''' <param name="iPane">The index of the graph pane to check.</param>
        ''' <returns>True if data is sequential, False if data is scattered.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable ReadOnly Property IsDataSequential(Optional ByVal iPane As Integer = 1) As Boolean
            Get
                Dim bSequential As Boolean = True
                For Each ci As CurveItem In Me.DataLines(iPane)
                    For i As Integer = 1 To ci.Points.Count - 1
                        bSequential = bSequential And (ci.Points(i - 1).X < ci.Points(i).X)
                    Next
                Next
                Return bSequential
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Extract the data in the graph to text. The format that data is extracted
        ''' if the data is <see cref="IsDataSequential">sequential or scattered</see>.
        ''' </summary>
        ''' <returns>A massive string.</returns>
        ''' -----------------------------------------------------------------------
        Public Overridable Function ExtractData() As String

            Dim bSequential As Boolean = True
            For iPane As Integer = 1 To Me.NumPanes
                bSequential = bSequential And Me.IsDataSequential(iPane)
            Next

            If bSequential Then
                Return Me.ExtractDataSequential()
            Else
                Return Me.ExtractDataScattered()
            End If

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Extract data in a sequential manner, e.g. as a table with a column for every
        ''' line, and a row for every X value. This only works for graphs that contain
        ''' <see cref="IsDataSequential">sequential</see> data.
        ''' </summary>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Protected Overridable Function ExtractDataSequential() As String

            Dim sb As New StringBuilder()
            Dim lAllCurves As New List(Of CurveItem)
            Dim ci As CurveItem = Nothing
            Dim bIsCumulative As Boolean = False
            Dim info As cCurveInfo = Nothing
            Dim gp As GraphPane = Nothing
            Dim dValue As Double = 0.0#

            ' == Write pane header line ==
            sb.Append("Pane") ' Row header
            ' For each pane
            For iPane As Integer = 1 To Me.NumPanes
                ' Get pane
                gp = Me.GetPane(iPane)
                ' Print the title of the pane
                sb.Append(",""" & gp.Title.Text & """")
                ' Get all data lines for the pane
                Dim aCurves As CurveItem() = Me.DataLines(iPane)
                ' Append columns for child curves
                '   NB: Add one less comma than there are curves because the next pane name will add this last comma.
                '       We do not want the csv file to end with a dummy column!
                For iCurve As Integer = 2 To aCurves.Length
                    sb.Append(",")
                Next
                lAllCurves.AddRange(aCurves)
            Next
            sb.AppendLine()

            ' == Write title header line
            sb.Append("Data") ' Row header
            ' For each curve
            For iCurve As Integer = 0 To lAllCurves.Count - 1
                ' Print the title of the curve
                sb.Append(",""" & lAllCurves(iCurve).Label.Text & """")
            Next
            sb.AppendLine()

            ' Build temporary line admin
            Dim iNumActiveLines As Integer = lAllCurves.Count
            Dim aiLineIndex(iNumActiveLines) As Integer
            Dim abLineDone(iNumActiveLines) As Boolean
            Dim dXLabel As Double = Single.MaxValue

            ' == Write curve values
            While (iNumActiveLines > 0)

                ' Determine next min
                dXLabel = Single.MaxValue
                For iCurve As Integer = 0 To lAllCurves.Count - 1
                    ci = lAllCurves(iCurve)
                    If (Not abLineDone(iCurve)) Then
                        If (aiLineIndex(iCurve) = ci.Points.Count) Then
                            abLineDone(iCurve) = True
                            iNumActiveLines -= 1
                        Else
                            dXLabel = Math.Min(ci.Points(aiLineIndex(iCurve)).X, dXLabel)
                        End If
                    End If
                Next

                ' Plot row
                ' 1) plot label value
                sb.Append(cStringUtils.FormatDouble(dXLabel))
                ' 2) For all curves
                For iCurve As Integer = 0 To lAllCurves.Count - 1
                    ' Get curve
                    ci = lAllCurves(iCurve)
                    ' Add comma to finish last value (if any)
                    sb.Append(",")
                    ' Curve not done yet?
                    If (Not abLineDone(iCurve)) Then
                        ' Curve point = current label?
                        If (ci.Points(aiLineIndex(iCurve)).X = dXLabel) Then
                            ' Is cumulative?
                            info = Me.CurveInfo(ci)
                            If (info IsNot Nothing) Then
                                bIsCumulative = info.IsCumulative
                            Else
                                bIsCumulative = False
                            End If
                            ' Yeeehaw, plot value
                            ' JS 13May10: Addressed issue 645 - do not export as cumulative data
                            dValue = ci.Points(aiLineIndex(iCurve)).Y
                            If (bIsCumulative) Then
                                dValue -= info.Offset.Points(aiLineIndex(iCurve)).Y
                            End If
                            sb.Append(cStringUtils.FormatDouble(dValue))
                            ' Next
                            aiLineIndex(iCurve) += 1
                        End If ' Is label
                    End If ' Curve not done
                Next
                ' Add line terminator
                sb.AppendLine()

            End While

            Return sb.ToString()
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Extract data in a sequential manner, e.g. as a table with a column for every
        ''' line, and a row for every X value. This format will be used for graphs that 
        ''' do not contain <see cref="IsDataSequential">sequential</see> data.
        ''' </summary>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Protected Overridable Function ExtractDataScattered() As String

            Dim sb As New StringBuilder()
            Dim aCurves As CurveItem() = Nothing
            Dim bIsCumulative As Boolean = False
            Dim info As cCurveInfo = Nothing
            Dim sbX As StringBuilder = Nothing
            Dim sbY As StringBuilder = Nothing
            Dim dValue As Double = 0.0#
            Dim gp As GraphPane = Nothing

            ' For each pane
            For iPane As Integer = 1 To Me.NumPanes
                ' Get pane
                gp = Me.GetPane(iPane)
                ' Print the title
                sb.AppendLine("""" & gp.Title.Text & """")
                ' Get curves
                aCurves = Me.DataLines(iPane)
                ' For each curve
                For Each ci As CurveItem In aCurves
                    ' Get curve info
                    info = Me.CurveInfo(ci)

                    If (info IsNot Nothing) Then
                        bIsCumulative = info.IsCumulative
                    Else
                        bIsCumulative = False
                    End If

                    ' Print Item
                    sb.AppendLine("""" & ci.Label.Text & """")
                    sbX = New StringBuilder("x")
                    sbY = New StringBuilder("y")
                    For i As Integer = 0 To ci.NPts - 1
                        sbX.Append(",")
                        sbX.Append(cStringUtils.FormatDouble(ci.Points(i).X))
                        sbY.Append(",")

                        ' JS 13May10: Addressed issue 645 - do not export as cumulative data
                        dValue = ci.Points(i).Y
                        If (bIsCumulative) Then
                            dValue -= info.Offset.Points(i).Y
                        End If
                        sbY.Append(cStringUtils.FormatDouble(dValue))
                    Next

                    sb.AppendLine(sbX.ToString())
                    sb.AppendLine(sbY.ToString())
                Next
            Next
            Return sb.ToString()

        End Function

#End Region ' Data extraction 

#Region " Context Menu "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Extract the data in the ZedGraph to a comma-separated file.
        ''' </summary>
        ''' <param name="strFileName">The file to extract the data to.</param>
        ''' <returns>
        ''' True if successful.
        ''' </returns>
        ''' -----------------------------------------------------------------------
        Public Function WriteDataToCSV(ByVal strFileName As String) As Boolean

            Dim sw As StreamWriter = Nothing

            Try
                sw = New StreamWriter(strFileName, False)
                If (sw IsNot Nothing) Then
                    Try
                        ' Write the stream
                        sw.Write(Me.ExtractData())
                    Catch ex As Exception
                        ' Woops
                    End Try
                    ' Always close
                    sw.Close()
                End If
            Catch ex As Exception
                Return False
            End Try
            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Extract the data in the ZedGraph to a comma-separated (.CSV) file.
        ''' </summary>
        ''' <returns>
        ''' True if successful.
        ''' </returns>
        ''' -----------------------------------------------------------------------
        Public Function ExtractDataToCSV() As Boolean

            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim model As cEwEModel = Me.Core.EwEModel
            Dim strFN As String = ""
            Dim strBit As String = ""

            ' Concoct file name for graph
            strFN = model.Name & "_graph"
            If Me.NumPanes = 1 Then
                strBit = Me.GetPane(1).Title.Text
            Else
                strBit = Me.m_zgc.MasterPane.Title.Text
            End If
            If Not String.IsNullOrEmpty(strBit) Then strFN &= "_" & strBit
            If Me.m_zgc.MasterPane.PaneList.Count = 1 Then
                strFN = cFileUtils.ToValidFileName(strFN, False)
            End If

            cmdFS.Invoke(strFN, My.Resources.FILEFILTER_CSV, 0)

            If cmdFS.Result = DialogResult.OK Then
                If Me.WriteDataToCSV(cmdFS.FileName) Then
                    ' ToDo: globalize this
                    Dim msg As New cMessage(cStringUtils.Localize(My.Resources.GENERIC_FILESAVE_SUCCES, "Graph data", cmdFS.FileName),
                                            eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                    msg.Hyperlink = Path.GetDirectoryName(cmdFS.FileName)
                    Me.m_uic.Core.Messages.SendMessage(msg)
                End If
            End If

            Return True

        End Function

        Public Function ExtractDataToClipboard() As Boolean
            Try
                Clipboard.SetText(Me.ExtractData())
            Catch ex As Exception
                ' NOP
            End Try
        End Function

#End Region ' Context Menu

#Region " Pane value querying "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get mean value of all <see cref="eSketchDrawModeTypes.Line">model data</see>
        ''' in a pane.
        ''' </summary>
        ''' <param name="iPane"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function GetValueAvg(Optional ByVal iPane As Integer = 1) As Single

            Dim gp As GraphPane = Me.GetPane(iPane)
            Dim ci As CurveItem = Nothing
            Dim dTotal As Double = 0.0#
            Dim iNumValues As Integer = 0

            For iCurve As Integer = 0 To gp.CurveList.Count - 1
                ci = gp.CurveList(iCurve)
                If Me.CurveType(ci) = eSketchDrawModeTypes.Line Then
                    For iPT As Integer = 0 To ci.Points.Count - 1
                        dTotal += ci.Points(iPT).Y
                        iNumValues += 1
                    Next
                End If
            Next

            If (iNumValues = 0) Then
                Return 0
            Else
                Return CSng(dTotal / iNumValues)
            End If

        End Function

#End Region ' Pane value querying

#Region " Regression line "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find a linear regression line for a list of points. The regression
        ''' includes standard errors for slope and intercept, and provides the
        ''' measure of correlation.
        ''' </summary>
        ''' <param name="ppl"></param>
        ''' <param name="sSlope"></param>
        ''' <param name="sSlopeStdErr"></param>
        ''' <param name="sIntercept"></param>
        ''' <param name="sInterceptStdErr"></param>
        ''' <param name="sCorrelation"></param>
        ''' <param name="sMin"></param>
        ''' <param name="sMax"></param>
        ''' <param name="iSampleSize"></param>
        ''' <remarks>
        ''' After Joe Hui's Particle Size Distribution implementation.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Sub FindRegression(ByVal ppl As PointPairList,
                                     ByRef sSlope As Single, ByRef sSlopeStdErr As Single,
                                     ByRef sIntercept As Single, ByRef sInterceptStdErr As Single,
                                     ByRef sCorrelation As Single, ByRef sMin As Single, ByRef sMax As Single,
                                     ByRef iSampleSize As Integer)

            Dim ptp As PointPair = Nothing
            Dim dXValue As Double = 0
            Dim dYValue As Double = 0
            Dim dSumX As Double = 0
            Dim dSumY As Double = 0
            Dim dSumXSq As Double = 0
            Dim dSumYSq As Double = 0
            Dim dSumXY As Double = 0
            Dim iNum As Integer = 0
            Dim dXMin As Double = Double.MaxValue
            Dim dXMax As Double = Double.MinValue
            Dim dXMean As Double
            Dim dYMean As Double
            Dim dSumXdevYdev As Double = 0
            Dim dSumXdevSq As Double = 0
            Dim dSumYdevSq As Double = 0
            Dim dXStdDev As Double
            Dim dYStdDev As Double
            Dim dEstStdErr As Double

            For i As Integer = 0 To ppl.Count - 1
                ptp = ppl(i)
                dXValue = ptp.X
                dYValue = ptp.Y
                dSumX += dXValue
                dSumY += dYValue
                dSumXSq += dXValue * dXValue
                dSumYSq += dYValue * dYValue
                dSumXY += dXValue * dYValue

                dXMin = Math.Min(dXValue, dXMin)
                dXMax = Math.Max(dXValue, dXMax)
                iNum += 1
            Next

            If (iNum > 0) Then
                dXMean = dSumX / iNum
                dYMean = dSumY / iNum
            End If

            For i As Integer = 0 To ppl.Count - 1
                ptp = ppl(i)
                dXValue = ptp.X
                dYValue = ptp.Y
                dSumXdevYdev += ((dXValue - dXMean) * (dYValue - dYMean))
                dSumXdevSq += ((dXValue - dXMean) ^ 2)
                dSumYdevSq += ((dYValue - dYMean) ^ 2)
            Next

            sSlope = CSng(dSumXdevYdev / dSumXdevSq)
            sIntercept = CSng(dYMean - sSlope * dXMean)

            dXStdDev = Math.Sqrt(dSumXdevSq / (iNum - 1))
            dYStdDev = Math.Sqrt(dSumYdevSq / (iNum - 1))
            dEstStdErr = Math.Sqrt((iNum - 1) * (dYStdDev ^ 2 - sSlope ^ 2 * dXStdDev ^ 2) / (iNum - 2))
            sSlopeStdErr = CSng(dEstStdErr / (Math.Sqrt(iNum - 1) * dXStdDev))
            sInterceptStdErr = CSng(dEstStdErr * Math.Sqrt((1 / iNum) + (dXMean ^ 2 / ((iNum - 1) * dXStdDev ^ 2))))

            sCorrelation = CSng((iNum * dSumXY - dSumX * dSumY) /
                           (Math.Sqrt(iNum * dSumXSq - dSumX ^ 2) * Math.Sqrt(iNum * dSumYSq - dSumY ^ 2)))
            sMin = CSng(dXMin)
            sMax = CSng(dXMax)
            iSampleSize = iNum

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find a linear regression line for a list of points.
        ''' </summary>
        ''' <param name="ppl"></param>
        ''' <param name="sSlope"></param>
        ''' <param name="sIntercept"></param>
        ''' <param name="iSampleSize"></param>
        ''' -------------------------------------------------------------------       
        Protected Sub FindRegression(ByVal ppl As PointPairList,
                                     ByRef sSlope As Single, ByRef sIntercept As Single,
                                     ByRef iSampleSize As Integer)

            Dim ptp As PointPair = Nothing
            Dim s0 As Integer = 0
            Dim s1, s2, t0, t1 As Double

            For i As Integer = 0 To ppl.Count - 1
                ptp = ppl(i)
                s0 += 1
                s1 = s1 + ptp.X
                s2 = s2 + ptp.X * ptp.X
                t0 = t0 + ptp.Y
                t1 = t1 + ptp.X * ptp.Y
            Next

            sSlope = CSng((s0 * t1 - s1 * t0) / (s0 * s2 - s1 * s1))
            sIntercept = CSng((s2 * t0 - s1 * t1) / (s0 * s2 - s1 * s1))

        End Sub

#End Region ' Regression line

#End Region ' Public interfaces

#Region " Events "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="changeType"></param>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)

            ' Do not do this if not yet initialized
            If (Me.m_zgc.GraphPane Is Nothing) Then Return

            If ((changeType And cStyleGuide.eChangeType.Fonts) > 0) Then
                Me.UpdateStyle()
            End If

            If ((changeType And cStyleGuide.eChangeType.Colours) > 0) Then
                Me.UpdateColours()
            End If

            If ((changeType And cStyleGuide.eChangeType.GroupVisibility) > 0) Or
               ((changeType And cStyleGuide.eChangeType.FleetVisibility) > 0) Then
                Me.UpdateCurveVisibility()
            End If

            If ((changeType And cStyleGuide.eChangeType.Units) > 0) Then
                Me.RefreshAxisLabels()
            End If

            If ((changeType And cStyleGuide.eChangeType.GraphStyle) > 0) Then
                Me.UpdateLegends()
                Me.UpdateAxisLabels()
            End If
            Me.m_zgc.Invalidate()

        End Sub

        Private Function OnMouseDownEvent(ByVal zg As ZedGraphControl, ByVal args As MouseEventArgs) As Boolean

            Dim iPane As Integer = GetPaneAtPoint(args.Location)
            Dim pane As GraphPane = Nothing
            Dim ciNearest As CurveItem = Nothing
            Dim iNearest As Integer = -1

            If iPane > -1 Then

                ' Get the clicked pane
                pane = Me.GetPane(iPane)

                'If (pane.FindNearestPoint(args.Location, ciNearest, iNearest)) Then
                '    RaiseEvent OnCurveClicked(ciNearest, iNearest)
                'End If

                ' Cursor?
                If Me.m_abShowCursor(iPane) Then
                    Me.CursorPos = GraphToScale(New PointF(args.Location.X, args.Location.Y)).X
                    Return True
                End If

            End If
            Return False
        End Function

        Private Function OnMouseMoveEvent(ByVal zg As ZedGraphControl, ByVal args As MouseEventArgs) As Boolean

            Dim iPane As Integer = GetPaneAtPoint(args.Location)
            Dim ciNearest As CurveItem = Nothing
            Dim iNearest As Integer = -1

            If iPane > -1 Then

                ' Get the clicked pane
                Dim pane As GraphPane = Nothing

                ' Cursor?
                If Me.m_abShowCursor(iPane) Then
                    Me.CursorPos = GraphToScale(New PointF(args.Location.X, args.Location.Y)).X
                    Return True
                End If

            End If
            Return False
        End Function

        Private Function OnMouseUpEvent(ByVal zg As ZedGraphControl, ByVal args As MouseEventArgs) As Boolean
            Dim iPanel As Integer = GetPaneAtPoint(args.Location)
            If iPanel > -1 Then
                If Me.m_abShowCursor(iPanel) Then
                    Me.CursorPos = CSng(Math.Round(Me.CursorPos))
                    Return True
                End If
            End If
            Return False
        End Function

#End Region ' Events

#Region " Internal bits "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the current instance of the hover menu.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Function HoverMenu() As ucHoverMenu
            Return Me.m_hovermenu
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an EwE-styled line.
        ''' </summary>
        ''' <param name="ppl"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function CreateLineItem(ByVal info As cCurveInfo,
                                                      ByVal ppl As PointPairList) As LineItem

            Dim li As LineItem = Nothing
            Dim bShowInLegend As Boolean = True

            Select Case info.LineType()

                Case eSketchDrawModeTypes.TimeSeriesDriver

                    li = New ZedGraph.LineItem(info.Label, ppl, info.Colour, SymbolType.Circle, 1)

                    li.Line.IsVisible = False
                    li.Line.Fill.IsVisible = False

                    ' ToDo_JS: obtain symbol size from style guide
                    li.Symbol.Size = 4
                    li.Symbol.Border.Color = info.Colour
                    li.Symbol.Border.IsVisible = True
                    li.Symbol.Fill.Color = info.Colour
                    li.Symbol.Fill.IsVisible = False
                    li.Symbol.IsVisible = True

                Case eSketchDrawModeTypes.TimeSeriesRefAbs

                    li = New ZedGraph.LineItem(info.Label, ppl, info.Colour, SymbolType.Square, 1)

                    li.Line.IsVisible = False
                    li.Line.Fill.IsVisible = False

                    ' ToDo_JS: obtain symbol size from style guide
                    li.Symbol.Size = 4
                    li.Symbol.Border.Color = info.Colour
                    li.Symbol.Border.IsVisible = True
                    li.Symbol.Fill.Color = info.Colour
                    li.Symbol.Fill.IsVisible = False
                    li.Symbol.IsVisible = True

                Case eSketchDrawModeTypes.TimeSeriesRefRel

                    li = New ZedGraph.LineItem(info.Label, ppl, info.Colour, SymbolType.Diamond, 1)

                    li.Line.IsVisible = False
                    li.Line.Fill.IsVisible = False

                    ' ToDo_JS: obtain symbol size from style guide
                    li.Symbol.Size = 4
                    li.Symbol.Border.Color = info.Colour
                    li.Symbol.Border.IsVisible = True
                    li.Symbol.Fill.Color = info.Colour
                    li.Symbol.Fill.IsVisible = False
                    li.Symbol.IsVisible = True

                Case Else
                    li = New ZedGraph.LineItem(info.Label, ppl, info.Colour, SymbolType.None, 1)

            End Select

            li.IsVisible = info.IsVisible
            li.Label.IsVisible = info.IsVisible
            li.Tag = info

            ' Detect if this line is a duplicate of an already existing core item. In that case the 
            ' line will be hidden from the legend
            Return li

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find the next curve of a given <see cref="eSketchDrawModeTypes">type</see>.
        ''' </summary>
        ''' <param name="curvetype">The <see cref="eSketchDrawModeTypes">type</see> of the
        ''' curve to locate.</param>
        ''' <param name="iPane">Index of the graph pane to look into.</param>
        ''' <param name="iStart">Search start index, 0 by default.</param>
        ''' <returns>Index of the curve that matches the line type, or -1 if
        ''' no such curve could be found.</returns>
        ''' -------------------------------------------------------------------
        Protected Function FindNextCurvePos(ByVal curvetype As eSketchDrawModeTypes,
                                            Optional ByVal iPane As Integer = 1,
                                            Optional ByVal iStart As Integer = 0) As Integer

            Dim pane As GraphPane = Me.GetPane(iPane)
            Dim ci As CurveItem = Nothing

            If (pane Is Nothing) Then Return -1

            For iCurve As Integer = iStart To pane.CurveList.Count - 1
                ci = pane.CurveList(iCurve)
                If Me.CurveType(ci) = curvetype Then Return iCurve
            Next
            Return -1

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find the previous curve of a given <see cref="eSketchDrawModeTypes">type</see>.
        ''' </summary>
        ''' <param name="curvetype">The <see cref="eSketchDrawModeTypes">type</see> of the
        ''' curve to locate.</param>
        ''' <param name="iPane">Index of the graph pane to look into.</param>
        ''' <param name="iStart">Search start index, provide -1 to start searching
        ''' at the end of the curve list.</param>
        ''' <returns>Index of the curve that matches the line type, or -1 if
        ''' no such curve could be found.</returns>
        ''' -------------------------------------------------------------------
        Protected Function FindLastCurvePos(ByVal curvetype As eSketchDrawModeTypes,
                                            Optional ByVal iPane As Integer = 1,
                                            Optional ByVal iStart As Integer = -1) As Integer

            Dim pane As GraphPane = Me.GetPane(iPane)
            Dim ci As CurveItem = Nothing

            If (pane Is Nothing) Then Return -1

            ' Fix default
            If (iStart = -1) Then iStart = pane.CurveList.Count

            For iCurve As Integer = Math.Min(iStart, pane.CurveList.Count - 1) To 0 Step -1
                ci = pane.CurveList(iCurve)
                If Me.CurveType(ci) = curvetype Then Return iCurve
            Next
            Return -1

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a graph pane already contains a line for core object.
        ''' </summary>
        ''' <param name="item">The core object to search for.</param>
        ''' <param name="iPane"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Protected Function ContainsCurve(ByVal item As cCurveInfo,
                                         Optional ByVal iPane As Integer = 1) As Boolean

            Dim pane As GraphPane = Me.GetPane(iPane)
            Dim ci As CurveItem = Nothing
            Dim infoTest As cCurveInfo = Nothing

            If (pane Is Nothing) Then Return False

            For iCurve As Integer = 0 To pane.CurveList.Count - 1
                ci = pane.CurveList(iCurve)
                If (TypeOf ci Is LineItem) Then
                    infoTest = Me.CurveInfo(ci)
                    If (infoTest IsNot Nothing) Then
                        If (infoTest.IsReferenceTo(item)) Then Return True
                    End If
                End If
            Next
            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return all <see cref="LineItem">line items</see> of a given 
        ''' <see cref="eSketchDrawModeTypes">line type</see>.
        ''' </summary>
        ''' <param name="curvetype">The <see cref="eSketchDrawModeTypes">line type</see> of
        ''' lines to retrieve.</param>
        ''' <param name="iPane">Index of the pane to obtain lines from.</param>
        ''' <returns>An array of <see cref="LineItem">line item</see> instances.</returns>
        ''' ------------------------------------------------------------------- 
        Protected Function GetLineItems(ByVal curvetype As eSketchDrawModeTypes,
                                        Optional ByVal iPane As Integer = 1) As LineItem()

            Dim lLines As New List(Of LineItem)
            Dim li As LineItem = Nothing
            Dim info As cCurveInfo = Nothing

            For Each ci As CurveItem In Me.GetPane(iPane).CurveList
                If (TypeOf ci Is LineItem) Then
                    li = DirectCast(ci, LineItem)
                    info = Me.CurveInfo(ci)
                    If (info Is Nothing) Then
                        If (curvetype = eSketchDrawModeTypes.NotSet) Then
                            lLines.Add(li)
                        End If
                    Else
                        If (curvetype = info.LineType) Then
                            lLines.Add(li)
                        End If
                    End If
                End If
            Next
            Return lLines.ToArray

        End Function

        ''' <summary>
        ''' Change the number of panels in the graph. Note that user interfaces 
        ''' should NOT maintain references to existing panes anymore!
        ''' </summary>
        Private Sub ChangeNumPanels()

            Me.m_zgc.MasterPane.PaneList.Clear()
            For ipn As Integer = 1 To Me.m_nPanels
                Me.m_zgc.MasterPane.PaneList.Add(New GraphPane())
            Next

            'While Me.m_zgc.MasterPane.PaneList.Count < Me.m_nPanels
            '    Me.m_zgc.MasterPane.PaneList.Add(New GraphPane())
            'End While

            'While Me.m_zgc.MasterPane.PaneList.Count > Me.m_nPanels
            '    Me.m_zgc.MasterPane.PaneList.RemoveAt(Me.m_nPanels)
            'End While

            ReDim Me.m_abShowCursor(Me.m_nPanels)
            ReDim Me.m_aliCursors(Me.m_nPanels)
            ReDim Me.m_asCursorPos(Me.m_nPanels)
            ReDim Me.m_bCumulative(Me.m_nPanels)

        End Sub

#Region " Styling "

        Protected Overridable Sub UpdateColours()

            Dim info As cCurveInfo = Nothing
            Dim gp As GraphPane = Nothing
            Dim ci As CurveItem = Nothing
            Dim acurves() As CurveItem = Nothing
            Dim iFirstDataLinePos As Integer = -1
            Dim bPaneCumulative As Boolean = False
            Dim iNumHighlights As Integer = 0

            For iPane As Integer = 1 To Me.m_nPanels
                gp = Me.GetPane(iPane)
                gp.Chart.Fill = New Fill(Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND))
                gp.Chart.Border.IsVisible = False

                bPaneCumulative = Me.IsPaneCumulative(iPane)
                Me.RemoveCursor(iPane)
                Me.SetCursor(iPane)

                acurves = gp.CurveList.ToArray
                iFirstDataLinePos = Me.FindNextCurvePos(eSketchDrawModeTypes.Line, iPane)
                iNumHighlights = 0

                For iCurve As Integer = 0 To acurves.Length - 1
                    ci = acurves(iCurve)
                    info = Me.CurveInfo(ci)
                    If (info IsNot Nothing) Then
                        If (info.IsHighlighted) Then iNumHighlights += 1
                    End If
                Next

                For iCurve As Integer = 0 To acurves.Length - 1

                    ci = acurves(iCurve)
                    info = Me.CurveInfo(ci)
                    If info IsNot Nothing Then

                        ' Not cumulative pane?
                        If (Not bPaneCumulative) Then
                            ' #Yes: Reorder coloured data lines
                            If (info.LineType = eSketchDrawModeTypes.Line) And
                               (info.IsGrayedOut = False) Then
                                gp.CurveList.Remove(ci)
                                gp.CurveList.Insert(iFirstDataLinePos, ci)
                            End If
                        End If

                        ci.Color = info.Colour

                        If TypeOf ci Is LineItem Then
                            With DirectCast(ci, LineItem)
                                If bPaneCumulative Then
                                    .Line.Color = Color.Gray
                                    .Line.Fill.Color = info.Colour
                                Else
                                    .Line.Color = info.Colour
                                    .Line.Fill.Color = Color.White
                                End If
                                .Line.Width = If(info.IsHighlighted And iNumHighlights = 1, 3.0!, 1.0!)
                                .Symbol.Border.Color = .Line.Color
                                .Symbol.Fill.Color = .Line.Color
                            End With
                        End If

                    End If
                Next

            Next iPane

        End Sub

        Public Sub UpdateCurveVisibility()

            For iPane As Integer = 1 To Me.m_nPanels
                With Me.GetPane(iPane)
                    For Each ci As CurveItem In .CurveList
                        ci.IsVisible = Me.IsCurveVisible(ci)
                        ci.Label.IsVisible = ci.IsVisible ' Hide from legend too
                    Next
                End With
            Next iPane

            ' Axis may have changed: reflect this properly
            Me.RescaleAndRedraw()

        End Sub

        Protected Overridable Function IsCurveVisible(ByVal ci As CurveItem) As Boolean
            Dim info As cCurveInfo = Me.CurveInfo(ci)
            If info IsNot Nothing Then
                Return info.IsVisible Or (Me.IsTrackVisiblity = False)
            End If
            Return ci.IsVisible
        End Function

        Protected Overridable Sub UpdateStyle(Optional iPane As Integer = -1)

            Dim iPaneMin As Integer = 1
            Dim iPaneMax As Integer = Me.m_nPanels
            Dim lAxis As New List(Of ZedGraph.Axis)

            If (iPane > 0) Then
                iPaneMin = iPane
                iPaneMax = iPane
            End If

            For iPane = iPaneMin To iPaneMax
                With Me.GetPane(iPane)

                    lAxis.Clear()
                    lAxis.Add(.YAxis)
                    lAxis.Add(.Y2Axis)
                    lAxis.Add(.XAxis)
                    lAxis.Add(.X2Axis)

                    .IsFontsScaled = False

                    .Title.FontSpec.Family = Me.StyleGuide.FontFamilyName(cStyleGuide.eApplicationFontType.Title)
                    .Title.FontSpec.Size = Me.StyleGuide.FontSize(cStyleGuide.eApplicationFontType.Title)
                    .Title.FontSpec.IsBold = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Title) And FontStyle.Bold) = FontStyle.Bold)
                    .Title.FontSpec.IsItalic = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Title) And FontStyle.Italic) = FontStyle.Italic)
                    .Title.FontSpec.IsUnderline = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Title) And FontStyle.Underline) = FontStyle.Underline)

                    For Each ax As Axis In lAxis
                        ax.Title.FontSpec.Family = Me.StyleGuide.FontFamilyName(cStyleGuide.eApplicationFontType.SubTitle)
                        ax.Title.FontSpec.Size = Me.StyleGuide.FontSize(cStyleGuide.eApplicationFontType.SubTitle)
                        ax.Title.FontSpec.IsBold = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.SubTitle) And FontStyle.Bold) = FontStyle.Bold)
                        ax.Title.FontSpec.IsItalic = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.SubTitle) And FontStyle.Italic) = FontStyle.Italic)
                        ax.Title.FontSpec.IsUnderline = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.SubTitle) And FontStyle.Underline) = FontStyle.Underline)

                        ax.Scale.FontSpec.Family = Me.StyleGuide.FontFamilyName(cStyleGuide.eApplicationFontType.Scale)
                        ax.Scale.FontSpec.Size = Me.StyleGuide.FontSize(cStyleGuide.eApplicationFontType.Scale)
                        ax.Scale.FontSpec.IsBold = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Scale) And FontStyle.Bold) = FontStyle.Bold)
                        ax.Scale.FontSpec.IsItalic = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Scale) And FontStyle.Italic) = FontStyle.Italic)
                        ax.Scale.FontSpec.IsUnderline = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Scale) And FontStyle.Underline) = FontStyle.Underline)
                    Next

                    .Legend.FontSpec.Family = Me.StyleGuide.FontFamilyName(cStyleGuide.eApplicationFontType.Legend)
                    .Legend.FontSpec.Size = Me.StyleGuide.FontSize(cStyleGuide.eApplicationFontType.Legend)
                    .Legend.FontSpec.IsBold = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Legend) And FontStyle.Bold) = FontStyle.Bold)
                    .Legend.FontSpec.IsItalic = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Legend) And FontStyle.Italic) = FontStyle.Italic)
                    .Legend.FontSpec.IsUnderline = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Legend) And FontStyle.Underline) = FontStyle.Underline)

                End With
            Next

            With Me.m_zgc.MasterPane
                .Border.IsVisible = False
                .Title.FontSpec.Family = Me.StyleGuide.FontFamilyName(cStyleGuide.eApplicationFontType.Title)
                .Title.FontSpec.Size = Me.StyleGuide.FontSize(cStyleGuide.eApplicationFontType.Title)
                .Title.FontSpec.IsBold = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Title) And FontStyle.Bold) = FontStyle.Bold)
                .Title.FontSpec.IsItalic = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Title) And FontStyle.Italic) = FontStyle.Italic)
                .Title.FontSpec.IsUnderline = ((Me.StyleGuide.FontStyle(cStyleGuide.eApplicationFontType.Title) And FontStyle.Underline) = FontStyle.Underline)

                Using g As Graphics = Me.m_zgc.CreateGraphics()
                    .SetLayout(g, PaneLayout.SquareColPreferred)
                End Using
            End With

            ' Redraw at your convenience
            Me.m_zgc.Invalidate()

        End Sub

        Private Sub UpdateLegends(Optional ByVal gp As GraphPane = Nothing)

            Dim bShow As Boolean = (Me.StyleGuide.ShowLegends = TriState.True) Or _
                                   (Me.StyleGuide.ShowLegends = TriState.UseDefault And Me.m_bShowLegend = True)
            If gp Is Nothing Then
                For Each gp In Me.m_zgc.MasterPane.PaneList
                    gp.Legend.IsVisible = bShow
                Next
            Else
                gp.Legend.IsVisible = bShow
            End If

            Me.m_zgc.Invalidate()

        End Sub

        Private Sub UpdateAxisLabels(Optional ByVal gp As GraphPane = Nothing)

            Dim bShow As Boolean = (Me.StyleGuide.ShowAxisLabels = TriState.True) Or _
                                   (Me.StyleGuide.ShowAxisLabels = TriState.UseDefault And Me.m_bShowAxisLabels = True)
            If (gp Is Nothing) Then
                For Each gp In Me.m_zgc.MasterPane.PaneList
                    gp.XAxis.Title.IsVisible = bShow
                    gp.X2Axis.Title.IsVisible = bShow
                    gp.YAxis.Title.IsVisible = bShow
                    gp.Y2Axis.Title.IsVisible = bShow
                    gp.Title.IsVisible = bShow
                Next
            Else
                gp.XAxis.Title.IsVisible = bShow
                gp.X2Axis.Title.IsVisible = bShow
                gp.YAxis.Title.IsVisible = bShow
                gp.Y2Axis.Title.IsVisible = bShow
                gp.Title.IsVisible = bShow
            End If

        End Sub

#End Region ' Styling

#Region " Mouse support "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the graph pane located at a given point.
        ''' </summary>
        ''' <param name="pt">The point to test.</param>
        ''' <returns>Index of a pane, or -1 if no pane was found at the given
        ''' location.</returns>
        ''' -------------------------------------------------------------------
        Protected Function GetPaneAtPoint(ByVal pt As Point) As Integer
            For i As Integer = 1 To Me.m_nPanels
                Dim gp As GraphPane = Me.GetPane(i)
                If gp.Rect.Contains(pt) Then Return i
            Next
            Return -1
        End Function

#End Region ' Mouse support

#Region " Cumulative plot support "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add an offset line to a target line
        ''' </summary>
        ''' <param name="liOffset"></param>
        ''' <param name="lTarget"></param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub SumLines(ByVal liOffset As LineItem, ByVal lTarget As LineItem)

            Dim ci As cCurveInfo = Me.CurveInfo(lTarget)

            If (liOffset Is Nothing) Or (lTarget Is Nothing) Then Return

            For iPt As Integer = 0 To lTarget.Points.Count - 1
                lTarget(iPt).Y += liOffset.Points(iPt).Y
            Next
            ci.Offset = liOffset

        End Sub

#End Region ' Cumulative plot support

#Region " Cursor "

        Protected Function GraphToScale(ByVal ptf As PointF) As PointF
            Dim myPane As GraphPane = Me.m_zgc.GraphPane
            Dim dX As Double = 0.0
            Dim dY As Double = 0.0
            myPane.ReverseTransform(ptf, dX, dY)
            Return New PointF(CSng(dX), CSng(dY))
        End Function

        Protected Sub RemoveCursor(ByVal iPane As Integer)
            If Me.m_abShowCursor(iPane) Then
                Me.GetPane(iPane).CurveList.Remove(Me.m_aliCursors(iPane))
                Me.m_aliCursors(iPane) = Nothing
                Me.m_zgc.Invalidate()
            End If
        End Sub

        Protected Sub SetCursor(ByVal iPane As Integer)
            If Me.m_abShowCursor(iPane) Then

                Dim gp As GraphPane = Me.GetPane(iPane)
                Dim dYMin As Double = gp.YAxis.Scale.Min
                Dim dYMax As Double = gp.YAxis.Scale.Max

                ' Clean up if necessary
                If Me.m_aliCursors(iPane) IsNot Nothing Then Me.RemoveCursor(iPane)
                ' Set cursor
                Me.m_aliCursors(iPane) = New LineItem(My.Resources.GENERIC_TEXT_CURSOR, _
                        New Double() {Me.m_asCursorPos(iPane), Me.m_asCursorPos(iPane)}, _
                        New Double() {dYMin, dYMax}, _
                        Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT), _
                        SymbolType.None, _
                        3)

                gp.CurveList.Add(Me.m_aliCursors(iPane))
                Me.m_zgc.Invalidate()
            End If
        End Sub

#End Region ' Cursor

#Region " Context Menu "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Handler to extend the context menu for the wrapped ZedGraph.
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="menuStrip"></param>
        ''' <param name="mousePt"></param>
        ''' <param name="objState"></param>
        ''' -----------------------------------------------------------------------
        Protected Sub OnBuildContextMenu(ByVal control As ZedGraphControl,
                                         ByVal menuStrip As ContextMenuStrip,
                                         ByVal mousePt As Point,
                                         ByVal objState As ZedGraphControl.ContextMenuObjectState)

            Dim item As ToolStripMenuItem = Nothing

            '' Remove 'Set_to_default' item
            '' (After http://zedgraph.org/wiki/index.php?title=Edit_the_Context_Menu)
            'For Each tsmi As ToolStripMenuItem In menuStrip.Items
            '    If String.Compare(CStr(tsmi.Tag), "set_default", True) = 0 Then
            '        menuStrip.Items.Remove(tsmi)
            '        Exit For
            '    End If
            'Next

            item = New ToolStripMenuItem(My.Resources.GENERIC_SHOW_LEGEND, My.Resources.LegendHS, AddressOf OnShowHideLegend)
            item.ShortcutKeys = Keys.Control Or Keys.L
            item.ShowShortcutKeys = True
            item.Checked = Me.IsLegendVisible
            menuStrip.Items.Add(item)

            item = New ToolStripMenuItem(My.Resources.GENERIC_SHOW_LABELS, Nothing, AddressOf OnShowHideAxisLabels)
            item.ShowShortcutKeys = True
            item.Checked = Me.IsAxisLabelsVisible
            menuStrip.Items.Add(item)

            item = New ToolStripMenuItem(My.Resources.GENERIC_SAVE_TO_CSV, My.Resources.ExportHS, AddressOf OnExtractToCSV)
            item.ShowShortcutKeys = True
            menuStrip.Items.Add(item)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler for extracting data to a CSV file.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Sub OnExtractToCSV(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Me.ExtractDataToCSV()
            Catch ex As Exception
                cLog.Write(ex, "ZedGraphHelper::OnExtractToCSV " & sender.ToString())
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler for toggling legend visibility.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Sub OnShowHideLegend(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Me.IsLegendVisible = Not Me.IsLegendVisible
            Catch ex As Exception
                cLog.Write(ex, "ZedGraphHelper::OnShowHideLegend " & sender.ToString())
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler for toggling axis label visibility.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Sub OnShowHideAxisLabels(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Me.IsAxisLabelsVisible = Not Me.IsAxisLabelsVisible
            Catch ex As Exception
                cLog.Write(ex, "ZedGraphHelper::OnShowHideAxisLabels " & sender.ToString())
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler for extracting data to the clipboard.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Sub OnExtractToClipboard(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Me.ExtractDataToClipboard()
            Catch ex As Exception
                cLog.Write(ex, "ZedGraphHelper::OnExtractToClipboard " & sender.ToString())
            End Try
        End Sub

#End Region ' Context menu

#Region " Hover menu handling "

        ''' <summary>
        ''' Get/set whether a floating hover menu should be displayed on the graph.
        ''' </summary>
        Public Property ShowHoverMenu As Boolean
            Get
                Return Me.m_bShowHoverMenu
            End Get
            Set(value As Boolean)

                If (Me.m_hovermenu IsNot Nothing) Then
                    RemoveHandler Me.m_hovermenu.OnHoverVisible, AddressOf OnShowHoverMenu
                    Me.DestroyHoverMenu()
                End If

                Me.m_bShowHoverMenu = value

                If (Me.m_bShowHoverMenu And Me.IsAttached()) Then
                    Me.CreateHoverMenu()
                    AddHandler Me.m_hovermenu.OnHoverVisible, AddressOf OnShowHoverMenu
                End If

            End Set
        End Property

        ''' <summary>Cross-threading delegate.</summary>
        ''' <param name="cmd"></param>
        Private Delegate Sub OnHoverMenuCommandCallbackDelegate(ByVal cmd As Object)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for hover menu events.
        ''' </summary>
        ''' <param name="cmd"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnHoverMenuCommand(ByVal cmd As Object)

            Dim gp As GraphPane = Nothing
            Dim zs As ZoomState = Nothing
            Dim sValueAvg As Single = 0.0

            If (Not TypeOf cmd Is eHoverCommands) Then Return

            If Me.m_zgc.InvokeRequired Then
                Me.m_zgc.Invoke(New OnHoverMenuCommandCallbackDelegate(AddressOf OnHoverMenuCommand), New Object() {cmd})
                Return
            End If

            For iPane As Integer = 1 To Me.NumPanes
                ' Get pane
                gp = Me.GetPane(iPane)
                ' Get pane avg
                sValueAvg = Me.GetValueAvg(iPane)

                ' Manually zoom in, place zoom in zoom stack. Zoom out means recalling zoom positions
                Select Case DirectCast(cmd, eHoverCommands)
                    Case eHoverCommands.Zoomin
                        zs = New ZoomState(gp, ZoomState.StateType.Zoom)
                        gp.ZoomStack.Add(zs)
                        gp.YAxis.Scale.Max -= (gp.YAxis.Scale.Max - sValueAvg) / 4
                        gp.YAxis.Scale.Min += (sValueAvg - gp.YAxis.Scale.Min) / 4
                        Me.m_zgc.AxisChange()

                    Case eHoverCommands.Zoomout
                        zs = gp.ZoomStack.Pop(gp)
                        If (zs IsNot Nothing) Then
                            zs.ApplyState(gp)
                            Me.m_zgc.AxisChange()
                        End If

                    Case eHoverCommands.ZoomReset
                        While gp.ZoomStack.Count > 0
                            zs = gp.ZoomStack.Pop(gp)
                            If (zs IsNot Nothing) Then
                                zs.ApplyState(gp)
                                Me.m_zgc.AxisChange()
                            End If
                        End While

                    Case eHoverCommands.ExportToCSV
                        Me.ExtractDataToCSV()
                        Exit For

                    Case eHoverCommands.ShowLegend
                        Me.IsLegendVisible = Not Me.IsLegendVisible
                        Exit For

                    Case eHoverCommands.ShowLabels
                        Me.IsAxisLabelsVisible = Not IsAxisLabelsVisible
                        Exit For

                End Select
            Next
            Me.m_zgc.Refresh()
            Me.UpdateHoverMenuItems()

        End Sub

        Private Sub OnShowHoverMenu(sender As Object, args As EventArgs)
            Try
                Me.UpdateHoverMenuItems()
            Catch ex As Exception

            End Try
        End Sub

        Protected Overridable Sub UpdateHoverMenuItems()

            If (Me.m_hovermenu Is Nothing) Then Return

            Dim bCanZoomOut As Boolean = False
            For iPane As Integer = 1 To Me.NumPanes
                Dim gp As GraphPane = Me.GetPane(iPane)
                bCanZoomOut = bCanZoomOut Or (gp.ZoomStack.Count > 0)
            Next

            Me.m_hovermenu.IsEnabled(eHoverCommands.Zoomin) = True
            Me.m_hovermenu.IsEnabled(eHoverCommands.Zoomout) = bCanZoomOut
            Me.m_hovermenu.IsChecked(eHoverCommands.ShowLegend) = Me.IsLegendVisible
            Me.m_hovermenu.IsChecked(eHoverCommands.ShowLabels) = Me.IsAxisLabelsVisible

        End Sub

        Private Sub CreateHoverMenu()

            If (Me.m_hovermenu IsNot Nothing) Then Return

            Me.m_hovermenu = New ucHoverMenu(Me.m_uic)
            Me.m_hovermenu.AddItem(My.Resources.ZoomInHS, My.Resources.GENERIC_ZOOM_IN, eHoverCommands.Zoomin)
            Me.m_hovermenu.AddItem(My.Resources.ZoomOutHS, My.Resources.GENERIC_ZOOM_OUT, eHoverCommands.Zoomout)
            Me.m_hovermenu.AddItem(My.Resources.ZoomHS, My.Resources.GENERIC_ZOOM_RESET, eHoverCommands.ZoomReset)
            Me.m_hovermenu.AddSeparator()
            Me.m_hovermenu.AddItem(My.Resources.LegendHS, My.Resources.GENERIC_SHOW_LEGEND, eHoverCommands.ShowLegend)
            Me.m_hovermenu.AddItem(My.Resources.tag, My.Resources.GENERIC_SHOW_LABELS, eHoverCommands.ShowLabels)
            Me.m_hovermenu.AddSeparator()
            Me.m_hovermenu.AddItem(My.Resources.ExportHS, My.Resources.GENERIC_SAVE_TO_CSV, eHoverCommands.ExportToCSV)
            AddHandler Me.m_hovermenu.OnUserCommand, AddressOf OnHoverMenuCommand

            Me.m_hovermenu.Attach(Me.m_zgc)

        End Sub

        Private Sub DestroyHoverMenu()

            If (Me.m_hovermenu Is Nothing) Then Return
            RemoveHandler Me.m_hovermenu.OnUserCommand, AddressOf OnHoverMenuCommand
            Me.m_hovermenu.Detach()
            Me.m_hovermenu.Dispose()
            Me.m_hovermenu = Nothing

        End Sub

#End Region ' Hover menu handling

#Region " Refresh "

        ''' <summary>
        ''' Async full-on refresh
        ''' </summary>
        Private Sub DoRescaleAndRedraw()
            If (Me.m_zgc Is Nothing) Then Return
            If (Me.m_zgc.IsDisposed) Then Return

            Try
                Me.m_zgc.AxisChange()
                Me.m_zgc.Invalidate()
            Catch ex As Exception
                ' Whoah!
            End Try
        End Sub

#End Region ' Refresh

#End Region ' Internal bits

    End Class

End Namespace ' Controls
