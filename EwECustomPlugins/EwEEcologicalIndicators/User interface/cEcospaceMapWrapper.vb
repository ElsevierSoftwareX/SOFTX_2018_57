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
Imports System.Drawing
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports System.Windows.Forms
Imports ScientificInterfaceShared.Style
Imports System.Threading

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class to update the map that reflects Ecospace biodiversity indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcospaceMapWrapper

#Region " Private variables "

    ''' <summary>UIContext to operate onto.</summary>
    Private m_uic As cUIContext = Nothing

    ''' <summary>Settings to use in the map.</summary>
    Private m_settings As cIndicatorSettings = Nothing
    ''' <summary>Computed Ecospace indicators, organized by point (col, row).</summary>
    Private m_dtIndicators As Dictionary(Of Point, cEcospaceIndicators)
    Private m_indPath As cEcopathIndicators = Nothing

    ''' <summary>Ecospace depth layer for finding water cells and for showing context.</summary>
    Private m_layerDepth As cDisplayLayer = Nothing
    ''' <summary>Indicator layer data.</summary>
    Private m_layerData As cDisplayLayer = Nothing

    ''' <summary>Current indicator group to display in the graph.</summary>
    Private m_groupCurrent As cIndicatorInfoGroup = Nothing
    ''' <summary>Current indicator to display in the graph.</summary>
    Private m_indCurrent As cIndicatorInfo = Nothing

    Private m_picbox As PictureBox = Nothing
    Private m_drawers As New List(Of cEcospaceMapDrawer)
    Private m_bmp As Bitmap = Nothing
    Private m_colors As List(Of Color)

#End Region ' Private variables

#Region " Attach + detach "

    Public Sub Attach(ByVal uic As cUIContext, _
                      ByVal indicators As Dictionary(Of Point, cEcospaceIndicators), _
                      ByVal settings As cIndicatorSettings, _
                      ByVal picbox As PictureBox, _
                      ByVal indEcopath As cEcopathIndicators, _
                      ByVal colors As List(Of Color))

        Me.m_uic = uic
        Me.m_settings = settings
        Me.m_dtIndicators = indicators
        Me.m_indPath = indEcopath
        Me.m_picbox = picbox
        Me.m_colors = colors

        AddHandler Me.m_picbox.Resize, AddressOf OnResizePanel
        AddHandler Me.m_picbox.Paint, AddressOf OnPaintPicbox
        AddHandler Me.m_picbox.MouseEnter, AddressOf OnSetTooltip
        AddHandler Me.m_picbox.MouseMove, AddressOf OnSetTooltip
        AddHandler Me.m_picbox.MouseLeave, AddressOf OnClearTooltip

    End Sub

    Public Sub Detach()

        RemoveHandler Me.m_picbox.Resize, AddressOf OnResizePanel
        RemoveHandler Me.m_picbox.Paint, AddressOf OnPaintPicbox
        RemoveHandler Me.m_picbox.MouseEnter, AddressOf OnSetTooltip
        RemoveHandler Me.m_picbox.MouseMove, AddressOf OnSetTooltip
        RemoveHandler Me.m_picbox.MouseLeave, AddressOf OnClearTooltip

        Me.m_settings = Nothing
        Me.m_dtIndicators = Nothing
        Me.m_picbox = Nothing
        Me.m_uic = Nothing

    End Sub

#End Region ' Attach + detach

    Public Sub RefreshContent(indSingle As cIndicatorInfo, indGroup As cIndicatorInfoGroup)

        If (Me.m_uic Is Nothing) Then Return

        Me.m_indCurrent = indSingle

        Dim lInfo As New List(Of cIndicatorInfo)

        If (Me.m_indCurrent Is Nothing) Then
            ' Group mode
            If (Not ReferenceEquals(Me.m_groupCurrent, indGroup)) Then
                ' Get stuff
                Me.m_groupCurrent = indGroup
                For i As Integer = 0 To Me.m_groupCurrent.NumIndicators - 1
                    lInfo.Add(Me.m_groupCurrent.Indicator(i))
                Next
            End If
        Else
            If (Not ReferenceEquals(Me.m_indCurrent, indSingle)) Then
                ' Indicator mode
                Me.m_indCurrent = indSingle
                lInfo.Add(Me.m_indCurrent)
            End If
        End If

        If (lInfo.Count > 0) Then
            Me.m_drawers.Clear()
            For Each info In lInfo
                Dim drawer As New cEcospaceMapDrawer(Me.m_uic.Core, Me.m_uic.StyleGuide, info)
                drawer.Colors = Me.m_colors
                Me.m_drawers.Add(drawer)
            Next
            Me.m_bmp = Nothing
        End If

        Me.m_picbox.Invalidate()

    End Sub

    Private Sub OnResizePanel(sender As Object, args As EventArgs)

        If (Me.m_bmp IsNot Nothing) Then
            Me.m_bmp = Nothing
        End If
        Me.m_picbox.Invalidate()

    End Sub

    Private Sub OnPaintPicbox(sender As Object, args As EventArgs)

        If (Me.m_uic Is Nothing) Then Return

        If (Me.m_bmp Is Nothing) Then
            Me.m_bmp = New Bitmap(Me.m_picbox.Width, Me.m_picbox.Height, Imaging.PixelFormat.Format32bppArgb)
        End If

        Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap

        If (bm Is Nothing) Then Return

        Dim iInRow As Integer = bm.InRow
        Dim iInCol As Integer = bm.InCol
        Dim sg As cStyleGuide = Me.m_uic.StyleGuide
        Dim info As cIndicatorInfo = Nothing
        Dim ind As cEcospaceIndicators = Nothing
        Dim settings As cIndicatorSettings = Me.m_settings

        Dim n As Integer = Me.m_drawers.Count

        If (n > 0) Then

            ' Ugh!
            Dim asData(iInRow, iInCol, n) As Single
            Dim sValue As Single = 0
            Dim asScaler(n) As Single
            Dim astrLabels(n) As String
            Dim astrDescriptions(n) As String

            ' Populate result array from computed indicators
            For i As Integer = 0 To Me.m_drawers.Count - 1
                info = Me.m_drawers(i).Indicator

                For Each pt As Point In Me.m_dtIndicators.Keys
                    ind = Me.m_dtIndicators(pt)
                    sValue = info.GetValue(ind)
                    asData(pt.Y, pt.X, i) = sValue
                Next

                asScaler(i) = info.GetValue(Me.m_indPath)
                If (asScaler(i) = 0) Then asScaler(i) = 1.0
                astrLabels(i) = info.Name
                astrDescriptions(i) = info.Description
            Next

            Dim iNumPlotsHorz As Integer = 0
            Dim iNumPlotsVert As Integer = 0
            Dim originList As New List(Of PointF)
            Dim rectList As New List(Of Rectangle)
            Dim mapArgs As New cMapDrawerArgs(cMapDrawerBase.eMapType.RelBiomass, asScaler, 0)

            cMapDrawerBase.CalcMapAreas(Me.m_picbox.ClientRectangle, n, iInRow, iInCol,
                                        iNumPlotsHorz, iNumPlotsVert, originList, rectList)

            Using g As Graphics = Graphics.FromImage(Me.m_bmp)
                Using br As New SolidBrush(Color.White)
                    g.FillRectangle(br, 0, 0, Me.m_bmp.Width, Me.m_bmp.Height)
                End Using
            End Using

            For i As Integer = 0 To n - 1
                Dim drawer As cEcospaceMapDrawer = Me.m_drawers(i)

                'init the drawer to the latest values
                drawer.OriginList = originList
                drawer.RectList = rectList

                drawer.StanzaDS = Nothing

                drawer.InCol = iInCol
                drawer.InRow = iInRow
                drawer.Month = 0

                drawer.ClearItems()
                drawer.AddItem(i, i)

                drawer.Labels = astrLabels
                drawer.Descriptions = astrDescriptions
                drawer.Map = asData
                drawer.Graphics = Graphics.FromImage(Me.m_bmp)

                drawer.AllowedToRun = False
                drawer.SignalState.Reset()
                ThreadPool.QueueUserWorkItem(AddressOf drawer.Draw, mapArgs)

            Next

            For Each drawer In m_drawers
                drawer.SignalState.WaitOne()
            Next

            Me.m_picbox.Image = Me.m_bmp

        End If

    End Sub

    Private m_strTipLast As String = ""

    Private Sub OnSetTooltip(sender As Object, args As EventArgs)

        Dim ptScreen As Point = Control.MousePosition
        Dim ptControl As Point = Me.m_picbox.PointToClient(ptScreen)
        Dim strTip As String = ""

        ' Check which drawer this is in
        For Each d As cEcospaceMapDrawer In Me.m_drawers
            If (d.RectList IsNot Nothing) Then
                For i As Integer = 0 To d.RectList.Count - 1
                    Dim rc As Rectangle = d.RectList(i)
                    If rc.Contains(ptControl) Then
                        strTip = d.Descriptions(i)
                        Exit For
                    End If
                Next
            End If
        Next

        If (strTip <> Me.m_strTipLast) Then
            cToolTipShared.GetInstance().SetToolTip(Me.m_picbox, strTip)
            Me.m_strTipLast = strTip
        End If

    End Sub

    Private Sub OnClearTooltip(sender As Object, args As EventArgs)
        cToolTipShared.GetInstance().SetToolTip(Me.m_picbox, "")
        Me.m_strTipLast = ""
    End Sub

End Class
