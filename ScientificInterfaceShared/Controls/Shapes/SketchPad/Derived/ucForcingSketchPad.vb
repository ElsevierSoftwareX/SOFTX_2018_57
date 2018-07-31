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

Imports EwECore
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Definitions
Imports EwEUtils.Utilities
Imports System.Text
Imports EwEUtils.Core

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control, implements a control for sketching Ecosim forcing function shapes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucForcingSketchPad

        'Default display as Absolute value
        Private m_AxisYMarks As eAxisTickmarkDisplayModeTypes = eAxisTickmarkDisplayModeTypes.Absolute

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Public Property AxisTickMarkDisplayMode() As eAxisTickmarkDisplayModeTypes
            Get
                Return Me.m_AxisYMarks
            End Get
            Set(ByVal value As eAxisTickmarkDisplayModeTypes)
                Me.m_AxisYMarks = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the labels to display along the X axis.
        ''' </summary>
        ''' <param name="iWidth">Width of the X axis for the labels.</param>
        ''' <param name="sScale">Label placement scale factor along the X axis.</param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub GetXAxisLabels(ByVal iWidth As Integer, ByRef astrLabels As String(), ByRef sScale As Single)

            Dim lstrLabels As New List(Of String)

            If (Me.Shape IsNot Nothing) Then
                If Not Me.IsSeasonal Then
                    Dim iYearStart As Integer = Me.UIContext.Core.EcosimFirstYear
                    If (iYearStart <= 0) Then iYearStart = 1
                    Dim iYearMax As Integer = CInt(Me.XAxisMaxValue / cCore.N_MONTHS)
                    Dim iStepSize As Integer = (iYearMax + 9) \ 10
                    For i As Integer = 0 To iYearMax Step iStepSize
                        lstrLabels.Add((iYearStart + i).ToString)
                        sScale = CSng(Me.YearToX(i, iWidth) / iWidth)
                    Next
                Else
                    For i As Integer = 1 To cCore.N_MONTHS
                        lstrLabels.Add(New Date(1, i, 1).ToString("MMM"))
                    Next
                    ' Hack: one extra to center labels under value ranges
                    lstrLabels.Add("")
                    sScale = 1
                End If
            End If

            astrLabels = lstrLabels.ToArray
        End Sub

        Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
            MyBase.OnResize(e)
            Me.OnShapeChanged()
        End Sub

        Protected Overrides Sub DrawShape(ByVal shape As EwECore.cShapeData, _
                ByVal rcImage As System.Drawing.Rectangle, _
                ByVal g As System.Drawing.Graphics, _
                ByVal clr As System.Drawing.Color, _
                ByVal bDrawLabels As Boolean, _
                ByVal drawMode As eSketchDrawModeTypes, _
                ByVal iXMax As Integer, _
                ByVal sYMax As Single)

            ' ToDo: localize this

            Dim strLabel As String = ""
            Dim sLabelXScale As Single = 1.0!
            Dim sLabelXPos As Single = 0.0!
            Dim astrXMarks As String() = Nothing
            Dim sfmt As StringFormat = Nothing
            Dim sBtnSpace As Single = Me.Font.Height
            Dim brTmp As SolidBrush = Nothing
            Dim penTmp As Pen = Nothing
            Dim tmpFont As Font = Nothing
            Dim yStep As Integer = 0
            Dim sg As cStyleGuide = Me.UIContext.StyleGuide

            MyBase.DrawShape(shape, rcImage, g, clr, bDrawLabels, drawMode, iXMax, sYMax)

            If Not bDrawLabels Then Return
            If Me.Shape Is Nothing Then Return

            'Draw the line with y's value equal to 1
            g.DrawLine(Pens.Black, _
                cShapeImage.ToImagePoint(New PointF(0, 1), Me.ClientRectangle, Me.XAxisMaxValue, sYMax), _
                cShapeImage.ToImagePoint(New PointF(Me.XAxisMaxValue, 1), Me.ClientRectangle, Me.XAxisMaxValue, sYMax))

            ' Draw the axis when this mode is on
            If Me.m_bShowAxis Then

                ' Draw X and Y axises
                g.DrawLine(Pens.Gray, New PointF(rcImage.Left, rcImage.Bottom), New PointF(rcImage.Right, rcImage.Bottom))
                g.DrawLine(Pens.Gray, New PointF(rcImage.Left, rcImage.Top), New PointF(rcImage.Left, rcImage.Bottom))

                ' Draw X axis labels
                Me.GetXAxisLabels(rcImage.Width, astrXMarks, sLabelXScale)

                sfmt = New StringFormat
                sfmt.Alignment = StringAlignment.Center
                sfmt.LineAlignment = StringAlignment.Center

                sBtnSpace = Me.Font.Height
                brTmp = New SolidBrush(System.Drawing.Color.FromArgb(128, 0, 0, 0))
                penTmp = New Pen(System.Drawing.Color.FromArgb(128, 0, 0, 0))
                tmpFont = sg.Font(cStyleGuide.eApplicationFontType.Scale)

                For i As Integer = 0 To astrXMarks.Length - 1
                    If Me.Shape.IsSeasonal Then
                        sLabelXPos = CSng((i + 0.5!) * rcImage.Width / (astrXMarks.Length - 1))
                    Else
                        sLabelXPos = CSng(i * rcImage.Width * sLabelXScale / (astrXMarks.Length - 1))
                    End If
                    g.DrawString(astrXMarks(i), Me.Font, brTmp, rcImage.Left + sLabelXPos, rcImage.Bottom - sBtnSpace, sfmt)
                    g.DrawLine(penTmp, rcImage.Left + sLabelXPos, rcImage.Bottom, rcImage.Left + sLabelXPos, rcImage.Bottom - sBtnSpace / 2)
                Next

                'Draw Axis Y marks
                yStep = CInt(sYMax / 3)
                If yStep = 0 Then yStep = 1

                For j As Double = 0 To sYMax Step yStep * 0.5
                    Dim yPos As Integer = CInt(cShapeImage.ToImagePoint(New PointF(0, CSng(j)), rcImage, 0, sYMax).Y)

                    strLabel = j.ToString
                    If m_AxisYMarks = eAxisTickmarkDisplayModeTypes.Relative Then
                        strLabel = cStringUtils.Localize("x{0}", strLabel)
                    End If
                    g.DrawString(strLabel, Me.Font, brTmp, rcImage.Left + 5, yPos)
                    g.DrawLine(penTmp, rcImage.Left, yPos, rcImage.Left + 3, yPos)
                Next

                If sYMax < 0.5 And sYMax >= 0.01 Then
                    For j As Integer = 0 To 2
                        Dim yPos As Integer = CInt(rcImage.Bottom - rcImage.Height * (3 - j) / 3)

                        strLabel = sg.FormatNumber(sYMax * (3 - j) / 3)

                        If m_AxisYMarks = eAxisTickmarkDisplayModeTypes.Relative Then
                            strLabel = cStringUtils.Localize("x{0}", strLabel)
                        End If
                        g.DrawString(strLabel, Me.Font, brTmp, rcImage.Left + 5, yPos)
                        g.DrawLine(penTmp, rcImage.Left, yPos, rcImage.Left + 3, yPos)
                    Next
                End If

                ' Display shape ID (=index in manager list) + 1
                sfmt.LineAlignment = StringAlignment.Near
                g.DrawString(Me.GetShapeTitle(), tmpFont, brTmp, CSng(rcImage.Width / 2), rcImage.Top + 15, sfmt)

                ' Dispose the pen, brush and font we created and let the system garbage collect them.
                brTmp.Dispose()
                penTmp.Dispose()
                tmpFont.Dispose()

            End If

        End Sub

        Protected Overridable Function GetShapeTitle() As String

            Dim sb As New StringBuilder()
            sb.AppendLine(cStringUtils.Localize(My.Resources.GENERIC_LABEL_INDEXED, (DirectCast(Me.Shape, cShapeData).Index), Me.Shape.Name))

            If (TypeOf Me.Shape Is cForcingFunction) Then
                Dim ff As cForcingFunction = DirectCast(Me.Shape, cForcingFunction)
                If (ff.ShapeFunctionType <> eShapeFunctionType.NotSet) Then
                    Dim fn As IShapeFunction = cShapeFunctionFactory.GetShapeFunction(ff, Me.UIContext.Core.PluginManager)
                    Dim fmt As New cShapeFunctionFormatter()
                    sb.AppendLine(fmt.GetDescriptor(fn))
                End If
            End If

            Return sb.ToString()

        End Function

    End Class

End Namespace
