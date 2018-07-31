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

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' MTI graph with circles
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class cPlotOfMixedTrophicImpact
    Inherits cContentManager

    Private m_asData(,) As Single
    Private m_astrLabelsX() As String
    Private m_astrLabelsY() As String
    Private m_style As cArrayGraphRenderer.eRenderStyle = cArrayGraphRenderer.eRenderStyle.Circles
    Private m_bFillPlot As Boolean = False
    Private m_bDrawGrid As Boolean = False
    Private m_bDrawSlanted As Boolean = False
    Private m_bDrawLegend As Boolean = False
    Private m_labelstyle As eLabelStyle = eLabelStyle.All

    Public Sub New()
        '
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Mixed tropic level impacts"
    End Function

    Public Overrides Function Attach(ByVal manager As cNetworkManager, _
                                     ByVal datagrid As DataGridView, _
                                     ByVal graph As ZedGraphControl, _
                                     ByVal plot As ucPlot, _
                                     ByVal toolstrip As ToolStrip, _
                                     ByVal uic As cUIContext) As Boolean
        Dim bSucces As Boolean = MyBase.Attach(manager, datagrid, graph, plot, toolstrip, uic)
        Me.Plot.Visible = bSucces
        Me.Toolstrip.Visible = bSucces
        Me.ToolstripShowOptionEMF()
        Me.ToolstripShowOptionOptions()
        Me.ToolstripShowDisplayGroups()

        AddHandler Me.Plot.Content.Paint, AddressOf OnPaintPlot
        AddHandler Me.Plot.Resize, AddressOf OnResizePlot
        Return bSucces
    End Function

    Public Overrides Sub Detach()
        RemoveHandler Me.Plot.Content.Paint, AddressOf OnPaintPlot
        RemoveHandler Me.Plot.Resize, AddressOf OnResizePlot
        ' Restore fill
        Me.Plot.Content.Dock = DockStyle.Fill
        MyBase.Detach()
    End Sub

    Public Overrides Sub DisplayData()

        ' ID mapper
        Dim aIDS(NetworkManager.nGroups + NetworkManager.nFleets) As Integer
        Dim iNumItems As Integer = 0
        For i As Integer = 1 To NetworkManager.nGroups + NetworkManager.nFleets
            aIDS(i) = -1
            If i <= NetworkManager.nGroups Then
                If Me.StyleGuide.GroupVisible(i) Then
                    aIDS(i) = iNumItems
                    iNumItems += 1
                End If
            Else
                If Me.StyleGuide.FleetVisible(i) Then
                    aIDS(i) = iNumItems
                    iNumItems += 1
                End If
            End If
        Next

        ReDim m_asData(iNumItems - 1, iNumItems - 1)
        ReDim m_astrLabelsX(iNumItems - 1)
        ReDim m_astrLabelsY(iNumItems - 1)

        For i As Integer = 1 To NetworkManager.nGroups + NetworkManager.nFleets
            For j As Integer = 1 To NetworkManager.nGroups + NetworkManager.nFleets
                If (aIDS(j) >= 0) And (aIDS(i) >= 0) Then
                    Dim strLabel As String = ""

                    If j <= NetworkManager.nGroups Then
                        Select Case Me.m_labelstyle
                            Case eLabelStyle.Name
                                strLabel = NetworkManager.GroupName(j)
                            Case eLabelStyle.Number
                                strLabel = CStr(j)
                            Case eLabelStyle.All
                                strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, j, NetworkManager.GroupName(j))
                        End Select
                    Else
                        Select Case Me.m_labelstyle
                            Case eLabelStyle.Name
                                strLabel = NetworkManager.FleetName(j - NetworkManager.nGroups)
                            Case eLabelStyle.Number
                                strLabel = CStr(j - NetworkManager.nGroups)
                            Case eLabelStyle.All
                                strLabel = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, j - NetworkManager.nGroups, NetworkManager.FleetName(j - NetworkManager.nGroups))
                        End Select
                    End If
                    m_astrLabelsX(aIDS(j)) = strLabel
                    m_astrLabelsY(aIDS(j)) = strLabel
                    m_asData(aIDS(i), aIDS(j)) = NetworkManager.MixedTrophicImpacts(j, i)
                End If
            Next j
        Next i

        Me.Plot.Invalidate()
        GC.Collect()

    End Sub

    Public Overloads Function Filename(ByVal strFilter As String) As String
        Return MyBase.Filename("MTI")
    End Function

    Public Overrides Sub SaveToEMF(ByVal strFileName As String)

        Dim bmp As Bitmap = Nothing
        Dim hdc As IntPtr = Nothing ' :)
        Dim mf As Metafile = Nothing
        Dim msg As cMessage = Nothing

        Me.Plot.Refresh()
        bmp = New Bitmap(Me.Plot.Content.Width, Me.Plot.Content.Height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            PlotToEMF(g)
        End Using
        Try
            bmp.Save(strFileName)
            msg = New cMessage(cStringUtils.Localize(SharedResources.GENERIC_FILESAVE_SUCCES, "MTI plot", strFileName), _
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
            msg.Hyperlink = Path.GetDirectoryName(strFileName)
        Catch ex As Exception
            msg = New cMessage(cStringUtils.Localize(SharedResources.GENERIC_FILESAVE_FAILURE, strFileName), _
                                  eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
        End Try
        bmp.Dispose()

        Me.UIContext.Core.Messages.SendMessage(msg)

    End Sub

    Public Overrides Function OptionsControl() As UserControl
        Return New ucPlotOfMTIOptions(Me)
    End Function

    Private Sub OnPaintPlot(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs)
        PlotToScreen(e.Graphics)
    End Sub

    Private Sub OnResizePlot(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.Plot.Invalidate(True)
    End Sub

    Private Sub PlotToScreen(ByVal g As Graphics)

        Dim ag As New cArrayGraphRenderer()
        Dim astrLegends() As String = {My.Resources.LBL_POSITIVE, My.Resources.LBL_NEGATIVE}

        If Not Me.DrawLegend Then astrLegends = Nothing

        If Me.m_bFillPlot Then
            Me.Plot.Content.Dock = DockStyle.Fill
        Else
            Me.Plot.Content.Dock = DockStyle.None
            Me.Plot.Content.Size = ag.MeasureGraph(Me.UIContext.StyleGuide, g, Me.m_style,
                                                   Me.m_asData,
                                                   My.Resources.LBL_IMPACTED_GP, m_astrLabelsX,
                                                   My.Resources.LBL_IMPACTING_GP, m_astrLabelsY,
                                                   astrLegends,
                                                   Me.m_bDrawGrid,
                                                   If(Me.m_bDrawSlanted, 30, 0))
        End If

        ag.Draw(Me.UIContext.StyleGuide, g, Me.Plot.Content.ClientRectangle, Me.m_style,
                Me.m_asData,
                My.Resources.LBL_IMPACTED_GP, m_astrLabelsX,
                My.Resources.LBL_IMPACTING_GP, m_astrLabelsY,
                astrLegends,
                Me.m_bDrawGrid,
                If(Me.m_bDrawSlanted, 30, 0))
    End Sub

    Private Sub PlotToEMF(ByVal g As Graphics)

        Dim ag As New cArrayGraphRenderer()
        Dim astrLegends() As String = {My.Resources.LBL_POSITIVE, My.Resources.LBL_NEGATIVE}

        If Not Me.DrawLegend Then astrLegends = Nothing

        ' Draw on client area only; me.width and me.height include space occupied by borders, caption bar, etc
        ag.Draw(Me.UIContext.StyleGuide, g, Me.Plot.Content.ClientRectangle, Me.m_style,
                Me.m_asData,
                My.Resources.LBL_IMPACTED_GP, Me.m_astrLabelsX,
                My.Resources.LBL_IMPACTING_GP, Me.m_astrLabelsY,
                astrLegends,
                Me.m_bDrawGrid,
                If(Me.m_bDrawSlanted, 30, 0))

    End Sub

    Public Property DrawCircles() As Boolean
        Get
            Return Me.m_style = cArrayGraphRenderer.eRenderStyle.Circles
        End Get
        Set(ByVal value As Boolean)
            Me.m_style = cArrayGraphRenderer.eRenderStyle.Circles
            Me.Plot.Invalidate(True)
        End Set
    End Property

    Public Property DrawRectangles() As Boolean
        Get
            Return Me.m_style = cArrayGraphRenderer.eRenderStyle.Bars
        End Get
        Set(ByVal value As Boolean)
            Me.m_style = cArrayGraphRenderer.eRenderStyle.Bars
            Me.Plot.Invalidate(True)
        End Set
    End Property

    Public Property DrawColors() As Boolean
        Get
            Return Me.m_style = cArrayGraphRenderer.eRenderStyle.Colours
        End Get
        Set(ByVal value As Boolean)
            Me.m_style = cArrayGraphRenderer.eRenderStyle.Colours
            Me.Plot.Invalidate(True)
        End Set
    End Property


    Public Property DrawGrid() As Boolean
        Get
            Return Me.m_bDrawGrid
        End Get
        Set(ByVal value As Boolean)
            Me.m_bDrawGrid = value
            Me.Plot.Invalidate(True)
        End Set
    End Property

    Public Property DrawSlanted() As Boolean
        Get
            Return Me.m_bDrawSlanted
        End Get
        Set(ByVal value As Boolean)
            Me.m_bDrawSlanted = value
            Me.Plot.Invalidate(True)
        End Set
    End Property

    Public Enum eLabelStyle As Integer
        Number
        Name
        All
    End Enum

    Public Property LabelStyle As eLabelStyle
        Get
            Return Me.m_labelstyle
        End Get
        Set(value As eLabelStyle)
            Me.m_labelstyle = value
            Me.DisplayData()
            Me.Plot.Invalidate(True)
        End Set
    End Property

    Public Property DrawLegend As Boolean
        Get
            Return Me.m_bDrawLegend
        End Get
        Set(ByVal value As Boolean)
            Me.m_bDrawLegend = value
            Me.Plot.Invalidate(True)
        End Set
    End Property

    Public Property FillPlotToArea As Boolean
        Get
            Return Me.m_bFillPlot
        End Get
        Set(ByVal value As Boolean)
            Me.m_bFillPlot = value
            If Me.m_bFillPlot Then
                Me.Plot.Content.Dock = DockStyle.Fill
            Else
                Dim ag As New cArrayGraphRenderer()
                'Dim astrLegends() As String = {My.Resources.LBL_POSITIVE, My.Resources.LBL_NEGATIVE}
                Dim g As Graphics = Graphics.FromHwnd(Me.Plot.Content.Handle)

                Me.Plot.Content.Dock = DockStyle.None
                Me.Plot.Content.Size = ag.MeasureGraph(Me.UIContext.StyleGuide, g, Me.m_style,
                                                       Me.m_asData,
                                                       My.Resources.LBL_IMPACTED_GP, m_astrLabelsX,
                                                       My.Resources.LBL_IMPACTING_GP, m_astrLabelsY,
                                                       Nothing,
                                                       Me.m_bDrawGrid,
                                                       If(Me.m_bDrawSlanted, 30, 0))

                g.Dispose()
            End If

            Me.Plot.Invalidate(True)
        End Set
    End Property

End Class
