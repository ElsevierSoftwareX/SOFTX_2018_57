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

#End Region ' Imports

Namespace Ecosim

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="ucSketchPad">Sketchpad-derived</see> control that renders
    ''' a forcing function for use in the Anomaly search panel.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucAnomalySearchSketchPad

#Region " Private bits "

        Private Enum eDragModeTypes As Integer
            None
            FirstYear
            EndYear
        End Enum

        Private m_dragMode As eDragModeTypes = eDragModeTypes.None
        Private m_iYearFirstDragPos As Integer = -1
        Private m_iYearLastDragPos As Integer = -1
        Private m_iYearFirst As Integer = 0
        Private m_iYearLast As Integer = 0
        Private m_iNumSplinePoints As Integer = 0

#End Region ' Private bits

#Region " Public properties "

        Public Overrides Property Editable() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
                ' NOP
            End Set
        End Property

        Public Property FirstYear() As Integer
            Get
                Return Me.m_iYearFirst
            End Get
            Set(ByVal value As Integer)
                Me.m_iYearFirst = value
                Me.Invalidate()
            End Set
        End Property

        Public Property LastYear() As Integer
            Get
                Return Me.m_iYearLast
            End Get
            Set(ByVal value As Integer)
                Me.m_iYearLast = value
                Me.Invalidate()
            End Set
        End Property

        Public Property NumSplinePoints() As Integer
            Get
                Return Me.m_iNumSplinePoints
            End Get
            Set(ByVal value As Integer)
                Me.m_iNumSplinePoints = value
                Me.Invalidate()
            End Set
        End Property

#End Region ' Public properties

#Region " Public events "

        Public Event OnYearRangeChanged(ByVal sender As ucAnomalySearchSketchPad)

#End Region ' Public events

#Region " Internal implementation "

        Protected Overrides Sub DrawShape(ByVal shape As EwECore.cShapeData, _
                                          ByVal rcImage As System.Drawing.Rectangle, _
                                          ByVal g As System.Drawing.Graphics, _
                                          ByVal clr As System.Drawing.Color, _
                                          ByVal bDrawLabels As Boolean, _
                                          ByVal drawMode As eSketchDrawModeTypes, _
                                          ByVal iXMax As Integer, _
                                          ByVal sYMax As Single)

            Dim iYear1 As Integer = 0
            Dim iYear2 As Integer = 0
            Dim iSpline As Integer = 0
            Dim iWidth As Integer = rcImage.Width

            ' Designer mode test
            If (Me.Shape Is Nothing) Then Return

            MyBase.DrawShape(shape, rcImage, g, clr, bDrawLabels, drawMode, iXMax, sYMax)

            If Me.m_dragMode = eDragModeTypes.None Then
                iYear1 = Me.YearToX(Me.m_iYearFirst, iWidth)
                iYear2 = Me.YearToX(Me.m_iYearLast, iWidth)
            Else
                iYear1 = Me.m_iYearFirstDragPos
                iYear2 = Me.m_iYearLastDragPos
            End If

            Me.DrawYearLine(g, iYear1)
            Me.DrawYearLine(g, iYear2)

            Dim n As Integer = Me.m_iNumSplinePoints
            If (n < 2) Then n = Me.XToYear(iYear2, iWidth) - Me.XToYear(iYear1, iWidth)

            ' Draw spline points
            If (iYear2 = 0) Then iYear2 = iWidth
            For i As Integer = 1 To n - 2
                iSpline = CInt(iYear1 + Math.Round(i * (iYear2 - iYear1) / (n - 1)))
                Me.DrawSplineLine(g, iSpline)
            Next

        End Sub

        Private Sub DrawYearLine(ByRef g As Graphics, ByVal x As Integer)
            Using penLine As New Pen(Drawing.Color.Black, 2)
                penLine.DashStyle = Drawing2D.DashStyle.Dot
                g.DrawLine(penLine, New Point(x, 0), New Point(x, Me.Height))
            End Using
        End Sub

        Private Sub DrawSplineLine(ByRef g As Graphics, ByVal x As Integer)
            Using penLine As New Pen(Drawing.Color.Orange, 1)
                penLine.DashStyle = Drawing2D.DashStyle.Dot
                g.DrawLine(penLine, New Point(x, 0), New Point(x, Me.Height))
            End Using
        End Sub

        Private cMOUSE_TOLERANCE As Integer = 3

        Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)

            Dim w As Integer = Me.ClientRectangle.Width

            If Me.Shape Is Nothing Then Return

            If (Math.Abs(e.X - Me.YearToX(Me.m_iYearFirst, w)) <= cMOUSE_TOLERANCE) Then
                Me.m_dragMode = eDragModeTypes.FirstYear
                Me.m_iYearFirstDragPos = e.X
                Me.m_iYearLastDragPos = Me.YearToX(Me.m_iYearLast, w)
                Me.Capture = True
            ElseIf (Math.Abs(e.X - Me.YearToX(Me.m_iYearLast, w)) <= cMOUSE_TOLERANCE) Then
                Me.m_dragMode = eDragModeTypes.EndYear
                Me.m_iYearFirstDragPos = Me.YearToX(Me.m_iYearFirst, w)
                Me.m_iYearLastDragPos = e.X
                Me.Capture = True
            End If
        End Sub

        Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)

            If Me.Shape Is Nothing Then Return

            Dim width As Integer = Me.ClientRectangle.Width
            Dim maxdrag As Integer = width

            ' Do not drag past NumDataPoints
            maxdrag = Math.Min(maxdrag, Me.YearToX(Me.NumDataPoints, maxdrag))

            If Me.Capture Then

                Select Case Me.m_dragMode
                    Case eDragModeTypes.FirstYear
                        Me.m_iYearFirstDragPos = Math.Max(0, Math.Min(maxdrag, e.X))
                    Case eDragModeTypes.EndYear
                        Me.m_iYearLastDragPos = Math.Max(0, Math.Min(maxdrag, e.X))
                End Select

                ' switch drag mode if first passed last
                If (Me.m_iYearFirstDragPos > Me.m_iYearLastDragPos) Then
                    Dim iSwitch As Integer = Me.m_iYearFirstDragPos
                    Me.m_iYearFirstDragPos = Me.m_iYearLastDragPos
                    Me.m_iYearLastDragPos = iSwitch
                    Me.m_dragMode = If(Me.m_dragMode = eDragModeTypes.FirstYear, eDragModeTypes.EndYear, eDragModeTypes.FirstYear)
                End If
                Me.Invalidate()

            Else
                If (Math.Abs(e.X - Me.YearToX(Me.m_iYearFirst, width)) <= cMOUSE_TOLERANCE) Or _
                   (Math.Abs(e.X - Me.YearToX(Me.m_iYearLast, width)) <= cMOUSE_TOLERANCE) Then
                    Me.Cursor = Cursors.SizeWE
                Else
                    Me.Cursor = Cursors.Default
                End If
            End If
        End Sub

        Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)

            Dim w As Integer = Me.ClientRectangle.Width

            If Me.Shape Is Nothing Then Return

            If Me.Capture Then
                ' Calc resulting years
                Me.m_iYearFirst = Math.Max(0, Me.XToYear(Me.m_iYearFirstDragPos, w))
                Me.m_iYearLast = Math.Min(Me.NumDataPoints, Me.XToYear(Me.m_iYearLastDragPos, w))

                ' Notify the world
                RaiseEvent OnYearRangeChanged(Me)

                ' Refresh
                Me.Invalidate()

                ' Stop drag
                Me.Capture = False
                Me.m_dragMode = eDragModeTypes.None

            End If

        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
