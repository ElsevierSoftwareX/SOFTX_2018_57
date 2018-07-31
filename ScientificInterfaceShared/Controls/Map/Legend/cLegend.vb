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

Imports System.Drawing.Imaging
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Style
Imports EwECore
Imports System.IO
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls.Map

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Simple map legend rendererfor <see cref="cDisplayLayer">display layers</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLegend

#Region " Private classes "

        ''' <summary>
        ''' An entry in the legend.
        ''' </summary>
        Private MustInherit Class cLegendEntry
            MustOverride ReadOnly Property Label As String
            MustOverride ReadOnly Property Renderer As cLayerRenderer
            MustOverride ReadOnly Property Max As Single
            MustOverride ReadOnly Property Min As Single
        End Class

        ''' <summary>
        ''' A static legend entry - one that does not vary.
        ''' </summary>
        Private Class cStaticEntry
            Inherits cLegendEntry

            Public Sub New(strName As String, strUnits As String, sMin As Single, sMax As Single)

                If String.IsNullOrWhiteSpace(strUnits) Then
                    Me.Label = strName
                Else
                    Me.Label = cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, strName, strUnits)
                End If
                Me.Min = sMin
                Me.Max = sMax
                Me.Renderer = New cLayerRendererValue(New Auxiliary.cVisualStyle())
                Me.Renderer.ScaleMin = Me.Min
                Me.Renderer.ScaleMax = Me.Max
            End Sub

            Public Overrides ReadOnly Property Renderer As Layers.cLayerRenderer

            Public Overrides ReadOnly Property Label As String

            Public Overrides ReadOnly Property Max As Single

            Public Overrides ReadOnly Property Min As Single

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' A legend entry for a single <see cref="cDisplayLayer">display layer</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class cLayerEntry
            Inherits cLegendEntry

            Private m_layer As cDisplayLayer

            Public Sub New(layer As cDisplayLayer)
                Me.m_layer = layer
            End Sub

            Public Overrides ReadOnly Property Renderer As Layers.cLayerRenderer
                Get
                    Return Me.m_layer.Renderer
                End Get
            End Property

            Public Overrides ReadOnly Property Label As String
                Get
                    If String.IsNullOrWhiteSpace(Me.m_layer.Units) Then
                        Return Me.m_layer.Name
                    Else
                        Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, Me.m_layer.Name, Me.m_layer.Units)
                    End If
                End Get
            End Property

            Public Overrides ReadOnly Property Max As Single
                Get
                    If (TypeOf Me.m_layer Is cDisplayLayerRaster) Then
                        Return DirectCast(Me.m_layer, cDisplayLayerRaster).Data.MaxValue
                    End If
                    Return cCore.NULL_VALUE
                End Get
            End Property

            Public Overrides ReadOnly Property Min As Single
                Get
                    If (TypeOf Me.m_layer Is cDisplayLayerRaster) Then
                        Return DirectCast(Me.m_layer, cDisplayLayerRaster).Data.MinValue
                    End If
                    Return cCore.NULL_VALUE
                End Get
            End Property

        End Class

#End Region ' Private helper classes

#Region " Private vars "

        Private Enum eLayerRenderStyle As Integer
            Symbol
            Element
            Gradient
        End Enum

        Private m_uic As cUIContext = Nothing
        Private m_strTitle As String = ""
        Private m_lLayers As New List(Of cLegendEntry)
        Private m_fmt As New StringFormat(StringFormat.GenericTypographic)

#End Region ' Private vars

#Region " Constructors "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a legend for all the current layers in a map.
        ''' </summary>
        ''' <param name="map">The map to populate the legend from.</param>
        ''' -------------------------------------------------------------------
        Private Sub New(ByVal map As ucMap)

            Me.New(map.UIContext, map.Title)

            Dim al As cDisplayLayer() = map.Layers
            Dim l As cDisplayLayer = Nothing
            Dim r As cLayerRenderer = Nothing

            For i As Integer = 0 To al.Length - 1
                l = al(i)
                If (l IsNot Nothing) Then
                    r = l.Renderer
                    If (r IsNot Nothing) Then
                        If r.IsVisible Then
                            ' Only add layers with a value range
                            Me.m_lLayers.Add(New cLayerEntry(l))
                        End If
                    End If
                End If
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, create a new legend with a fixed name.
        ''' </summary>
        ''' <param name="uic"><see cref="cUIContext"/> to use.</param>
        ''' <param name="strTitle">Map title.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(uic As cUIContext, strTitle As String)
            Me.m_uic = uic
            Me.m_strTitle = strTitle
            Me.m_fmt.Alignment = If(cSystemUtils.IsRightToLeft, StringAlignment.Far, StringAlignment.Near)
            Me.m_fmt.LineAlignment = StringAlignment.Center
        End Sub

#End Region ' Constructors

#Region " Shared interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Construct a new legend for a map.
        ''' </summary>
        ''' <param name="map">The map to populate the legend from.</param>
        ''' <returns>A <see cref="cLegend">legend</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FromMap(ByVal map As ucMap) As cLegend
            Debug.Assert(map IsNot Nothing)
            Return New cLegend(map)
        End Function

#End Region ' Shared interfaces

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the legend should show its title.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowTitle As Boolean = True

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the vertical spacing between the legend title and the first 
        ''' layer box.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property TitleVSpacing As Integer = 8

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the width of a layer box.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property LayerBoxWidth As Integer = 24

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the height of a layer box.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property LayerBoxHeight As Integer = 16

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the horizontal spacing between a layer box and its label.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property LayerBoxHSpacing As Integer = 4

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the vertical spacing between two layer boxes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property LayerBoxVSpacing As Integer = 6

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a layer to the legend.
        ''' </summary>
        ''' <param name="l"></param>
        ''' -------------------------------------------------------------------
        Public Sub AddLayer(l As cDisplayLayer)
            Me.m_lLayers.Add(New cLayerEntry(l))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a static value range to the legend, that will be displayed as a gradient.
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="sMin"></param>
        ''' <param name="sMax"></param>
        ''' <param name="strUnits">Units mask to show</param>
        ''' -------------------------------------------------------------------
        Public Sub AddGradient(strName As String, sMin As Single, sMax As Single, Optional strUnits As String = "")
            Me.m_lLayers.Add(New cStaticEntry(strName, strUnits, sMin, sMax))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the size of the legend, when rendered with the current
        ''' <see cref="cStyleGuide.Font">styleguide font settings</see> and
        ''' content. 
        ''' </summary>
        ''' <param name="g">The graphics to calculate for.</param>
        ''' <returns>A size.</returns>
        ''' -------------------------------------------------------------------
        Private Function Measure(g As Graphics) As Size

            Dim ftTitle As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Title)
            Dim ftLayer As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Legend)
            Dim ftScale As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Scale)

            ' Measure size of legend
            Dim iWidth As Integer = 0
            Dim iHeight As Integer = 0
            Dim szfItem As SizeF = Nothing

            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias

            Try
                If Me.ShowTitle Then
                    szfItem = Me.RenderTitleSize(g, ftTitle)
                    iWidth = Math.Max(iWidth, CInt(Math.Ceiling(szfItem.Width)))
                    iHeight += CInt(Math.Ceiling(szfItem.Height)) + Me.TitleVSpacing
                End If

                For iLayer As Integer = 0 To Me.m_lLayers.Count - 1
                    szfItem = Me.MeasureLayer(g, ftLayer, ftScale, Me.m_lLayers(iLayer))
                    If iLayer > 0 Then iHeight += 2 * Me.LayerBoxVSpacing
                    iWidth = Math.Max(iWidth, CInt(Math.Ceiling(szfItem.Width)))
                    iHeight += CInt(Math.Ceiling(szfItem.Height))
                Next iLayer
            Catch ex As Exception

            End Try

            iHeight += 1 + 2 * Me.LayerBoxVSpacing
            iWidth += 1 + 2 * Me.LayerBoxHSpacing

            ftScale.Dispose()
            ftTitle.Dispose()
            ftLayer.Dispose()

            Return New Size(iWidth, iHeight)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Draw the legend on a graphics device.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="ptOrigin">Top-left location to draw the legend.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Draw(g As Graphics, ptOrigin As Point) As Boolean

            Dim ftTitle As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Title)
            Dim ftLayer As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Legend)
            Dim ftScale As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Scale)
            Dim szfItem As SizeF = Nothing
            Dim iHeight As Integer
            Dim bSuccess As Boolean = True

            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias

            ptOrigin.X += Me.LayerBoxHSpacing
            ptOrigin.Y += Me.LayerBoxHSpacing

            Try

                If Me.ShowTitle Then
                    szfItem = Me.RenderTitleSize(g, ftTitle)
                    Me.DrawTitle(g, ftTitle, New Point(ptOrigin.X, ptOrigin.Y + iHeight))
                    iHeight += CInt(Math.Ceiling(szfItem.Height)) + Me.TitleVSpacing
                End If

                For iLayer As Integer = 0 To Me.m_lLayers.Count - 1
                    szfItem = Me.MeasureLayer(g, ftLayer, ftScale, Me.m_lLayers(iLayer))
                    If iLayer > 0 Then iHeight += 2 * Me.LayerBoxVSpacing
                    Me.DrawLayer(g, ftLayer, ftScale, Me.m_lLayers(iLayer), New Point(ptOrigin.X, ptOrigin.Y + iHeight))
                    iHeight += CInt(Math.Ceiling(szfItem.Height))
                Next iLayer

            Catch ex As Exception
                bSuccess = False
            End Try

            ftScale.Dispose()
            ftTitle.Dispose()
            ftLayer.Dispose()

            Return bSuccess
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the legend image to a file.
        ''' </summary>
        ''' <param name="strFileName"></param>
        ''' <param name="format"></param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Save(ByVal strFileName As String, ByVal format As ImageFormat) As Boolean

            If (Me.m_uic Is Nothing) Then Return False

            Dim sg As cStyleGuide = Me.m_uic.StyleGuide
            Dim szLegend As Size = Nothing
            Dim bSuccess As Boolean = True

            Using bmp As Bitmap = sg.GetImage(1000, 300, format)
                Using g As Graphics = Graphics.FromImage(bmp)
                    szLegend = Me.Measure(g)
                End Using ' g
            End Using ' bmp

            Try
                Using bmp As Bitmap = sg.GetImage(szLegend.Width, szLegend.Height, format, strFileName)
                    Using g As Graphics = Graphics.FromImage(bmp)
                        bSuccess = Me.Draw(g, New Point(0, 0))
                    End Using ' g
                    bmp.Save(strFileName, format)
                End Using ' bmp
            Catch ex As Exception
                bSuccess = False
            End Try

            Return bSuccess

        End Function

#End Region ' Public interfaces

#Region " Internals "

        Private Function RenderTitleSize(ByVal g As Graphics, ByVal ft As Font) As SizeF
            Return g.MeasureString(Me.m_strTitle, ft, 10000, Me.m_fmt)
        End Function

        Private Sub DrawTitle(ByVal g As Graphics, ByVal ft As Font, ByVal pt As Point)
            g.DrawString(Me.m_strTitle, ft, Brushes.Black, pt)
        End Sub

        Private Function MeasureLayer(ByVal g As Graphics, ByVal ftLabel As Font, ByVal ftScale As Font, ByVal l As cLegendEntry) As SizeF

            Dim style As eLayerRenderStyle = Me.GetRenderStyle(l)
            Dim szBox As New SizeF(0, 0)
            Dim strText As String = ""

            For i As Integer = 0 To l.Renderer.nSymbols

                If (i = 0) Then
                    strText = l.Label
                Else
                    strText = l.Renderer.SymbolName(i)
                    style = eLayerRenderStyle.Element
                    szBox.Height += Me.LayerBoxVSpacing
                End If

                Dim szName As SizeF = g.MeasureString(If(String.IsNullOrWhiteSpace(strText), "X", strText), ftLabel, 10000, Me.m_fmt)

                Select Case style
                    Case eLayerRenderStyle.Element, eLayerRenderStyle.Symbol
                        szName.Height = Math.Max(Me.LayerBoxHeight, szName.Height)
                    Case eLayerRenderStyle.Gradient
                        szName.Height = Math.Max(Me.LayerBoxHeight, szName.Height * 3)
                End Select

                szBox.Width = Me.LayerBoxWidth + Me.LayerBoxHSpacing + szName.Width
                szBox.Height += szName.Height

            Next

            Return szBox

        End Function

        Private Sub DrawLayer(ByVal g As Graphics, ByVal ftLabel As Font, ByVal ftScale As Font, ByVal l As cLegendEntry, ByVal pt As Point)

            Dim style As eLayerRenderStyle = Me.GetRenderStyle(l)
            Dim strText As String = ""

            For i As Integer = 0 To l.Renderer.nSymbols

                If (i = 0) Then
                    strText = l.Label
                Else
                    strText = l.Renderer.SymbolName(i)
                    style = eLayerRenderStyle.Element
                End If

                Dim szName As SizeF = g.MeasureString(If(String.IsNullOrWhiteSpace(strText), "X", strText), ftLabel, 10000, Me.m_fmt)
                Dim rcPreview As Rectangle = New Rectangle(pt.X, pt.Y, Me.LayerBoxWidth, CInt(Math.Max(Me.LayerBoxHeight, szName.Height)))
                Dim iSymbolKey As Integer = l.Renderer.SymbolKey(i)
                Dim fmt As StringFormat = CType(Me.m_fmt.Clone(), StringFormat)
                Dim rcItem As New RectangleF(pt.X + Me.LayerBoxHSpacing + rcPreview.Width, pt.Y, szName.Width, szName.Height)

                Select Case style

                    Case eLayerRenderStyle.Element
                        l.Renderer.RenderPreview(g, rcPreview, iSymbolKey)
                        g.DrawRectangle(Pens.Black, rcPreview)
                        g.DrawString(strText, ftLabel, Brushes.Black, rcItem, fmt)

                    Case eLayerRenderStyle.Symbol
                        l.Renderer.RenderPreview(g, rcPreview, iSymbolKey)
                        g.DrawString(strText, ftLabel, Brushes.Black, rcItem, fmt)

                    Case eLayerRenderStyle.Gradient
                        fmt.LineAlignment = StringAlignment.Near
                        g.DrawString(Me.m_uic.StyleGuide.FormatNumber(l.Max), ftScale, Brushes.Black, rcItem, fmt)

                        rcItem.Y += rcPreview.Height
                        fmt.LineAlignment = StringAlignment.Center
                        g.DrawString(l.Label, ftLabel, Brushes.Black, rcItem, fmt)

                        rcItem.Y += rcPreview.Height
                        fmt.LineAlignment = StringAlignment.Far
                        g.DrawString(Me.m_uic.StyleGuide.FormatNumber(l.Min), ftScale, Brushes.Black, rcItem, fmt)

                        rcPreview.Height *= 3
                        l.Renderer.RenderPreview(g, rcPreview)
                        g.DrawRectangle(Pens.Black, rcPreview)

                End Select

                pt.Y += Me.LayerBoxHSpacing + rcPreview.Height

            Next

        End Sub

        Private Function GetRenderStyle(ByVal l As cLegendEntry) As eLayerRenderStyle
            If (TypeOf (l.Renderer) Is cLayerRendererValue) Then
                Return eLayerRenderStyle.Gradient
            ElseIf (TypeOf (l.Renderer) Is cLayerRendererSymbol) Then
                Return eLayerRenderStyle.Symbol
            Else
                Return eLayerRenderStyle.Element
            End If
        End Function

#End Region ' Internals

    End Class

End Namespace
