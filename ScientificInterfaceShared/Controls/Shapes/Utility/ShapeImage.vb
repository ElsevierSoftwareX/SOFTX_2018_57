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
Imports System.Drawing.Drawing2D
Imports EwECore
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class for rendering <see cref="cShapeData"/> as a graph.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cShapeImage

        Public Const cDOT_SIZE As Integer = 6
        'Public Const cICON_WIDTH As Integer = 48
        'Public Const cICON_HEIGHT As Integer = 48

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' This helper method converts the coordinates of model point to those of the image point
        ''' </summary>
        ''' <param name="ptModel">Data point to convert</param>
        ''' <param name="rcClip">Clip rectangle to convert point to.</param>
        ''' <param name="sXMax">Clip rectangle horz. axis corresponds to [0, sxMax].</param>
        ''' <param name="sYMax">Clip rectangle vert. axis corresponds to [0, syMax].</param>
        ''' <returns>A point in the clip rectangle that corresponds to ptModel.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToImagePoint(ByVal ptModel As PointF, _
                                    ByVal rcClip As Rectangle, _
                                    ByVal sXMax As Single, ByVal sYMax As Single) As PointF

            Dim ptImage As PointF = Nothing

            ' Division by zero prevention
            If (sXMax = 0.0!) Then sXMax = 1.0!
            If (sYMax = 0.0!) Then sYMax = 1.0!
            ptImage = New PointF(ptModel.X * rcClip.Width / sXMax, rcClip.Height - ptModel.Y * rcClip.Height / sYMax)
            Return ptImage

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Transforms a screen point to an underlying model point.
        ''' </summary>
        ''' <param name="ptImage">The screen point to translate.</param>
        ''' <param name="rcClip">The screen clip area for interpreting the screen point.</param>
        ''' <param name="sXMax">X max scale to translate the point to. This code assumes that the x min scale always equals 0.</param>
        ''' <param name="sYMax">Y max scale to translate the point to. This code assumes that the y min scale always equals 0.</param>
        ''' -------------------------------------------------------------------
        Public Shared Function ToModelPoint(ByVal ptImage As PointF,
                                    ByVal rcClip As Rectangle,
                                    ByVal sXMax As Single, ByVal sYMax As Single) As PointF

            Dim ptModel As New PointF(CInt(Math.Ceiling((ptImage.X - rcClip.Left) * sXMax / rcClip.Width)),
                                (rcClip.Height + rcClip.Top - ptImage.Y) * sYMax / rcClip.Height)

            ' Clip values
            ptModel.X = Math.Min(Math.Max(0, ptModel.X), sXMax)
            ptModel.Y = Math.Max(0, ptModel.Y)

            Return ptModel

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Draws a <see cref="cShapeData">shape</see>.
        ''' </summary>
        ''' <param name="uic">UIContext that provides contextual information.</param>
        ''' <param name="shape">The shape to draw.</param>
        ''' <param name="rcImage">The dimensions of the area to render the shape onto.</param>
        ''' <param name="g">The graphics to draw the image onto.</param>
        ''' <param name="clr">The colour to use rendering the image.</param>
        ''' <param name="drawMode">The <see cref="eSketchDrawModeTypes">mode</see> to render the shape with.</param>
        ''' <param name="iXMax">The max X value to draw, or cCore.NULL_VALUE to use all points in the shape.</param>
        ''' <param name="sYMax">The max Y value to scale the shape to, or cCore.NULL_VALUE to use the default.</param>
        ''' <param name="strXMarkLabel">Label to draw along the XMark line.</param>
        ''' <param name="strYMarkLabel">Label to draw along the YMark line.</param>
        ''' <param name="sXMark">X mark line position, expressed in the same units as the X-axis values of the shape. Provide cCore.NULL_VALUE to use the default.</param>
        ''' <param name="sYMark">Y mark line position, expressed in the same units as the Y-axis values of the shape. Provide cCore.NULL_VALUE to use the default.</param>
        ''' -------------------------------------------------------------------
        Public Shared Sub DrawShape(ByVal uic As cUIContext,
                                ByVal shape As cShapeData,
                                ByVal rcImage As Rectangle,
                                ByVal g As Graphics,
                                ByVal clr As Color,
                                ByVal drawMode As eSketchDrawModeTypes,
                                Optional ByVal iXMax As Integer = cCore.NULL_VALUE,
                                Optional ByVal sYMax As Single = cCore.NULL_VALUE,
                                Optional ByVal sXMark As Single = cCore.NULL_VALUE,
                                Optional ByVal sYMark As Single = cCore.NULL_VALUE,
                                Optional ByVal strXMarkLabel As String = "",
                                Optional ByVal strYMarkLabel As String = "")

            If (shape Is Nothing) Then Return

            ' Provide defaults
            If (sYMax = cCore.NULL_VALUE) Then sYMax = shape.YMax * 1.2!
            If (sYMark = cCore.NULL_VALUE) Then sYMark = If(TypeOf (shape) Is cMediationFunction, 0.5!, 1.0!)
            If (iXMax <= 0) Then iXMax = shape.nPoints

            Dim bDrawZero As Boolean = False

            If (TypeOf shape Is cTimeSeries) Then
                bDrawZero = DirectCast(shape, cTimeSeries).SupportsNull
            End If

            cShapeImage.DrawShapeDirect(uic,
                    shape.ShapeData, iXMax, shape.IsSeasonal,
                    rcImage, g, clr,
                    drawMode,
                    sYMax,
                    sXMark, sYMark, strXMarkLabel, strYMarkLabel, bDrawZero)

        End Sub

        Public Shared Sub DrawShapeDirect(ByVal uic As cUIContext,
                                          ByVal asData As Single(), ByVal nPoints As Integer, ByVal bIsSeasonal As Boolean,
                                          ByVal rcImage As Rectangle,
                                          ByVal g As Graphics,
                                          ByVal clr As Color,
                                          ByVal drawMode As eSketchDrawModeTypes,
                                          ByVal sYMax As Single,
                                          ByVal sXMark As Single,
                                          ByVal sYMark As Single,
                                          Optional ByVal strXMarkLabel As String = "",
                                          Optional ByVal strYMarkLabel As String = "",
                                          Optional bDrawZero As Boolean = False)

            Dim sg As cStyleGuide = uic.StyleGuide
            Dim brShape As New SolidBrush(clr)
            Dim pnShape As New Pen(clr, 1)
            Dim pnMark As New Pen(Color.Blue, 1)

            pnMark.DashStyle = DashStyle.Dash

            Select Case drawMode

                Case eSketchDrawModeTypes.Fill

                    Dim gp As New GraphicsPath
                    Dim pt1 As PointF = Nothing
                    Dim pt2 As PointF = Nothing

                    If bIsSeasonal Then
                        nPoints = cCore.N_MONTHS
                    End If

                    'jb 27-04-09
                    'new drawing method draws data as discreet line from x-1 to x
                    'same logic for both seasonal and complete time series

                    pt2 = cShapeImage.ToImagePoint(New PointF(0, 0), rcImage, nPoints, sYMax)
                    For i As Integer = 1 To nPoints
                        pt1 = pt2
                        pt2 = cShapeImage.ToImagePoint(New PointF(i - 1.0!, asData(i)), rcImage, nPoints, sYMax)
                        gp.AddLine(pt1, pt2)

                        pt1 = pt2
                        pt2 = cShapeImage.ToImagePoint(New PointF(i, asData(i)), rcImage, nPoints, sYMax)
                        gp.AddLine(pt1, pt2)
                    Next

                    pt1 = pt2
                    pt2 = cShapeImage.ToImagePoint(New PointF(nPoints, 0), rcImage, nPoints, sYMax)
                    gp.AddLine(pt1, pt2)

                    Try
                        Select Case drawMode
                            Case eSketchDrawModeTypes.Fill
                                g.FillPath(brShape, gp)
                            Case Else
                                Debug.Assert(False)
                        End Select
                    Catch ex As Exception

                    End Try

                    gp.Dispose()

                Case eSketchDrawModeTypes.Line

                    Dim pt1 As PointF = Nothing
                    Dim pt2 As PointF = Nothing
                    Dim iNumPoints As Integer = 0

                    If bIsSeasonal Then
                        For i As Integer = 1 To 12
                            If If(bDrawZero, asData(i) > 0.0!, asData(i) <> cCore.NULL_VALUE) Then
                                pt1 = cShapeImage.ToImagePoint(New PointF(i - 0.5!, asData(i)), rcImage, nPoints, sYMax)
                                g.FillEllipse(brShape,
                                        CSng(pt1.X - cDOT_SIZE / 2), CSng(pt1.Y - cDOT_SIZE / 2),
                                        CSng(cDOT_SIZE), CSng(cDOT_SIZE))
                            End If
                        Next
                    Else
                        For i As Integer = 1 To nPoints
                            If If(bDrawZero, asData(i) >= 0, asData(i) > 0.0!) Then
                                pt2 = pt1
                                pt1 = cShapeImage.ToImagePoint(New PointF(i - 1.0!, asData(i)), rcImage, nPoints, sYMax)
                                iNumPoints += 1

                                If (iNumPoints >= 2) Then g.DrawLine(pnShape, pt1, pt2)
                            Else
                                ' Only one point last found?
                                If (iNumPoints = 1) Then
                                    ' #Yes: render this point
                                    g.DrawLine(pnShape, pt1.X, pt1.Y - 1, pt1.X, pt1.Y)
                                End If
                                iNumPoints = 0
                            End If
                        Next

                    End If

                Case Else

                    Dim pt As PointF = Nothing
                    Dim sOffset As Single = 1.0

                    If bIsSeasonal Then

                        nPoints = cCore.N_MONTHS
                        sOffset = 0.5!
                    End If

                    For i As Integer = 1 To nPoints
                        If If(bDrawZero, asData(i) >= 0, asData(i) > 0.0!) Then
                            pt = cShapeImage.ToImagePoint(New PointF(i - 0.5!, asData(i)), rcImage, nPoints, sYMax)

                            Select Case drawMode
                                Case eSketchDrawModeTypes.TimeSeriesDriver
                                    g.FillEllipse(brShape,
                                        CSng(pt.X - cDOT_SIZE / 2), CSng(pt.Y - cDOT_SIZE / 2),
                                        CSng(cDOT_SIZE), CSng(cDOT_SIZE))
                                Case eSketchDrawModeTypes.TimeSeriesRefAbs
                                    g.DrawRectangle(pnShape,
                                        CSng(pt.X - cDOT_SIZE / 2), CSng(pt.Y - cDOT_SIZE / 2),
                                        CSng(cDOT_SIZE), CSng(cDOT_SIZE))
                                Case eSketchDrawModeTypes.TimeSeriesRefRel
                                    Dim pts(3) As PointF
                                    pts(0) = New PointF(pt.X, CSng(pt.Y - cDOT_SIZE / 2))
                                    pts(1) = New PointF(CSng(pt.X + cDOT_SIZE / 2), pt.Y)
                                    pts(2) = New PointF(pt.X, CSng(pt.Y + cDOT_SIZE / 2))
                                    pts(3) = New PointF(CSng(pt.X - cDOT_SIZE / 2), pt.Y)
                                    g.DrawPolygon(pnShape, pts)
                            End Select
                        End If
                    Next

            End Select

            ' Draw YMark
            If (sYMark > 0) Then
                Try
                    Dim ptfFrom As PointF = cShapeImage.ToImagePoint(New PointF(0, sYMark), rcImage, nPoints, sYMax)
                    Dim ptfTo As PointF = cShapeImage.ToImagePoint(New PointF(nPoints, sYMark), rcImage, nPoints, sYMax)

                    g.DrawLine(Pens.Gray, ptfFrom, ptfTo)

                    ' Draw Ymark label, if any
                    If Not String.IsNullOrEmpty(strYMarkLabel) Then
                        Using ft As Font = sg.Font(cStyleGuide.eApplicationFontType.Scale)
                            Using br As New SolidBrush(sg.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT))
                                ' Position label on the right end of the graph
                                ptfTo.X -= g.MeasureString(strYMarkLabel, ft).Width
                                g.DrawString(strYMarkLabel, ft, br, ptfTo)
                            End Using
                        End Using
                    End If

                Catch ex As Exception
                    ' Error drawing a point out of range
                End Try
            End If

            ' Draw axis
            g.DrawLine(Pens.Gray, New PointF(0, 0), New PointF(0, rcImage.Height))
            g.DrawLine(Pens.Gray, New PointF(0, rcImage.Height), New PointF(rcImage.Width, rcImage.Height))

            ' Draw XMark
            If (sXMark > 0) Then

                Dim ptfTmp As PointF = cShapeImage.ToImagePoint(New PointF(sXMark, 0), rcImage, nPoints, sYMax)
                Dim ptfFrom As New PointF(ptfTmp.X, 0)
                Dim ptfTo As New PointF(ptfTmp.X, rcImage.Height)

                g.DrawLine(pnMark, ptfFrom, ptfTo)

                ' Draw Xmark label, if any
                If Not String.IsNullOrEmpty(strXMarkLabel) Then
                    Using ft As Font = sg.Font(cStyleGuide.eApplicationFontType.SubTitle)
                        Using br As New SolidBrush(Color.Blue)
                            Dim szfText As SizeF = g.MeasureString(strXMarkLabel, ft)
                            ' Position label on the top of the graph, on top of the line
                            ptfFrom = cShapeImage.ToImagePoint(New PointF(sXMark, sYMark), rcImage, nPoints, sYMax)
                            ptfFrom.Y = Math.Max(0, ptfFrom.Y - szfText.Height)
                            ptfFrom.X -= szfText.Width / 2
                            g.DrawString(strXMarkLabel, ft, br, ptfFrom)
                        End Using
                    End Using
                End If

            End If

            pnShape.Dispose()
            brShape.Dispose()
            pnMark.Dispose()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a thumbnail image of a given shape.
        ''' </summary>
        ''' <param name="shape">
        ''' The shape to obtain an image for. If this parameter is not specified,
        ''' a thumbnail image will be returned for the current shape.
        ''' </param>
        ''' <param name="uic">UI context for looking up style information.</param>
        ''' <param name="clr">Colour to render the thumbnail image with.</param>
        ''' <param name="dm"><see cref="eSketchDrawModeTypes">Mode</see> for rendering lines.</param>
        ''' <param name="iXMax">Number of points to draw, or <see cref="cCore.NULL_VALUE"/> to draw all available points.</param>
        ''' <param name="sYMax">Y-scale to use for rendering the image.</param>
        ''' <param name="bShowWarning">Flag stating whether a warning icon
        ''' should be displayed in the lower left corner of the shape
        ''' (or lower right, depending on locale reading order).</param>
        ''' -------------------------------------------------------------------
        Public Shared Function IconImage(ByVal uic As cUIContext, _
                ByVal shape As cShapeData, _
                ByVal clr As Color, _
                ByVal dm As eSketchDrawModeTypes, _
                ByVal iXMax As Integer, _
                Optional ByVal sYMax As Single = cCore.NULL_VALUE, _
                Optional ByVal bShowWarning As Boolean = False) As System.Drawing.Image

            Dim sg As cStyleGuide = uic.StyleGuide
            Dim bmp As New Bitmap(sg.ThumbnailSize, sg.ThumbnailSize)
            Dim g As Graphics = Graphics.FromImage(bmp)
            Dim iOverlaySize As Integer = Math.Min(16, sg.ThumbnailSize)

            ' Icons with dots become lines
            dm = DirectCast(Math.Min(dm, eSketchDrawModeTypes.Line), eSketchDrawModeTypes)

            Try
                DrawShape(uic, shape, New Rectangle(New Point(0, 0), bmp.Size), g, clr, dm, iXMax, sYMax, cCore.NULL_VALUE)
            Catch ex As Exception
                ' Draw error image
                g.FillRectangle(Brushes.White, New Rectangle(New Point(0, 0), bmp.Size))
                g.DrawLine(Pens.Red, 0, 0, bmp.Width, bmp.Height)
                g.DrawLine(Pens.Red, 0, bmp.Height, bmp.Width, 0)
            End Try

            ' Draw warning icon, if neccessary
            If bShowWarning Then
                ' Try to get system icon
                Using icoOverlay As New Icon(SystemIcons.Warning, iOverlaySize, iOverlaySize)
                    ' Did it work?
                    If (icoOverlay IsNot Nothing) Then
                        ' Calc rectangle to render icon
                        Dim rc As Rectangle = Nothing
                        If cSystemUtils.IsRightToLeft Then
                            ' RtoL reading order: draw image in lower left corner
                            rc = New Rectangle(0, _
                                               Math.Max(0, bmp.Height - iOverlaySize), _
                                               iOverlaySize, iOverlaySize)
                        Else
                            ' LtoR reading order: draw image in lower right corner
                            rc = New Rectangle(Math.Max(0, bmp.Width - iOverlaySize), _
                                               Math.Max(0, bmp.Height - iOverlaySize), _
                                               iOverlaySize, iOverlaySize)
                        End If
                        g.DrawIcon(icoOverlay, rc)
                    End If
                End Using
            End If

            g.Dispose()
            Return bmp

        End Function

    End Class

End Namespace
