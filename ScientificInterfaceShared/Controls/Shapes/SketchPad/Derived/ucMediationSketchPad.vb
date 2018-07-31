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
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style

#End Region

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control, implements a control for sketching Ecosim mediation shapes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucMediationSketchPad

        Private m_strXAxisLabel As String

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub DrawShape(ByVal shape As EwECore.cShapeData, _
                ByVal rcImage As System.Drawing.Rectangle, _
                ByVal g As System.Drawing.Graphics, _
                ByVal clr As System.Drawing.Color, _
                ByVal bDrawLabels As Boolean, _
                ByVal drawMode As eSketchDrawModeTypes, _
                ByVal iXMax As Integer, _
                ByVal sYMax As Single)

            If (Me.UIContext Is Nothing) Then Return

            MyBase.DrawShape(shape, rcImage, g, clr, bDrawLabels, drawMode, iXMax, sYMax)

            ' Sanity checks
            If (shape Is Nothing) Then Return
            If (Not TypeOf (shape) Is cMediationBaseFunction) Then Return
            If (Not bDrawLabels) Then Return

            Dim sfmt As StringFormat = Nothing
            Dim strCaption As String = ""
            Dim strLabel As String = ""
            Dim iYStep As Integer = 0
            Dim iYPos As Integer = 0
            Dim sYScale As Single = 1
            Dim sg As cStyleGuide = Me.UIContext.StyleGuide

            sYScale = Me.YMarkValue

            If (sYScale = 0) Then sYScale = 1

            sfmt = New StringFormat()
            sfmt.Alignment = StringAlignment.Center
            sfmt.LineAlignment = StringAlignment.Center

            ' Write mediation caption
            strCaption = Me.GetShapeTitle()

            Using br As New SolidBrush(System.Drawing.Color.FromArgb(128, 0, 0, 0))
                Using ft As Font = sg.Font(cStyleGuide.eApplicationFontType.Scale)
                    Using pn As New Pen(Color.FromArgb(128, 0, 0, 0))

                        g.DrawString(strCaption, ft, br, CSng(rcImage.Width / 2), rcImage.Top + 15, sfmt)
                        g.DrawString(Me.XAxisLabel, ft, br, CSng(rcImage.Width / 2), rcImage.Bottom - 15, sfmt)

                        ' Scale sYMax to the value at YMark
                        sYMax /= sYScale

                        ' Draw Axis Y marks
                        iYStep = CInt(sYMax / 3)
                        If iYStep = 0 Then iYStep = 1

                        ' Draw Y-axis scale
                        For j As Double = 0 To sYMax Step iYStep * 0.5
                            iYPos = CInt(cShapeImage.ToImagePoint(New PointF(0, CSng(j)), rcImage, 0, sYMax).Y)

                            strLabel = j.ToString
                            g.DrawString(strLabel, Me.Font, br, rcImage.Left + 5, iYPos)
                            g.DrawLine(pn, rcImage.Left, iYPos, rcImage.Left + 3, iYPos)
                        Next

                        ' Draw ticks
                        If sYMax < 0.5 And sYMax >= 0.01 Then
                            For j As Integer = 0 To 2
                                iYPos = CInt(rcImage.Bottom - rcImage.Height * (3 - j) / 3)
                                strLabel = sg.FormatNumber(sYMax * (3 - j) / 3)
                                g.DrawString(strLabel, Me.Font, br, rcImage.Left + 5, iYPos)
                                g.DrawLine(pn, rcImage.Left, iYPos, rcImage.Left + 3, iYPos)
                            Next
                        End If

                    End Using
                End Using
            End Using
        End Sub

        <Browsable(True), _
         Category("Mediation")> _
        Public Property XAxisLabel() As String
            Get
                Return Me.m_strXAxisLabel
            End Get
            Set(ByVal value As String)
                Me.m_strXAxisLabel = value
            End Set
        End Property

        Public Overrides ReadOnly Property XMarkLabel() As String
            Get
                If (Not TypeOf (Shape) Is cMediationBaseFunction) Then Return MyBase.XMarkLabel
                Dim medfn As cMediationBaseFunction = DirectCast(Me.Shape, cMediationBaseFunction)
                Return Me.UIContext.StyleGuide.FormatNumber(medfn.XBase)
            End Get
        End Property

        <Browsable(False)> _
        Public Overrides Property NumDataPoints() As Integer
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(ByVal value As Integer)
                '
            End Set
        End Property

        Protected Overrides Sub DragXMark(ByVal ptPrev As System.Drawing.Point, ByVal ptCur As System.Drawing.Point)
            MyBase.DragXMark(ptPrev, ptCur)
            Me.CalculateYMark()
        End Sub

        Protected Overrides Sub DragYMark(ByVal ptPrev As System.Drawing.Point, ByVal ptCur As System.Drawing.Point)
            MyBase.DragYMark(ptPrev, ptCur)
            Me.YMarkValue = Math.Min(Me.YMarkValue, Me.Shape.YMax)
        End Sub

        Protected Overrides Sub OnShapeChanged()
            MyBase.OnShapeChanged()
            If (Me.EditMode <> eMouseInteractionMode.DragYMark) Then
                Me.CalculateYMark()
            End If
            If (Me.EditMode <> eMouseInteractionMode.DragXMark) Then
                ' Me.CalculateXMark()
            End If
        End Sub

        Protected Overridable Sub CalculateYMark()
            If (Me.Shape Is Nothing) Then Return
            If (Me.XMarkValue < 0) Then Return
            Me.YMarkValue = Me.Shape.ShapeData(CInt(Math.Max(0, Math.Min(Me.Shape.ShapeData.Length - 1, Me.XMarkValue))))
        End Sub

        Protected Overridable Function GetShapeTitle() As String

            Dim sb As New StringBuilder()
            Dim ff As cMediationBaseFunction = DirectCast(Me.Shape, cMediationBaseFunction)

            sb.AppendLine(cStringUtils.Localize(My.Resources.GENERIC_LABEL_INDEXED, ff.ID + 1, Me.Shape.Name))

            If (ff.ShapeFunctionType <> eShapeFunctionType.NotSet) Then
                Dim fn As IShapeFunction = cShapeFunctionFactory.GetShapeFunction(ff, Me.UIContext.Core.PluginManager)
                Dim fmt As New cShapeFunctionFormatter()
                sb.AppendLine(fmt.GetDescriptor(fn))
            End If

            Return sb.ToString()

        End Function

    End Class

End Namespace
