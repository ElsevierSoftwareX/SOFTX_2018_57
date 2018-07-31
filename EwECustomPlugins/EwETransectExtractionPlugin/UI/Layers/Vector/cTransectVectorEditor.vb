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
Imports System.Windows.Forms
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers

#End Region ' Imports

Public Class cTransectVectorEditor
    Inherits cLayerEditorVector

    Private Enum eEditModeType As Integer
        NotSet = 0
        Start
        [End]
        Line
    End Enum

    Private m_transectEdit As cTransect = Nothing
    Private m_transectEditMode As eEditModeType = eEditModeType.NotSet

    ' For line editing
    Private m_ptfStartOrg As PointF
    Private m_ptfEndOrg As PointF
    Private m_ptClickOrg As Point

    Public Sub New()
        Me.IsEditable = True
    End Sub

    Public Overrides Sub ProcessMouseClick(e As MouseEventArgs, map As ucMap)

        ' ToDo: globalize this method

        Dim td As cTransectVectorDisplay = DirectCast(Me.Layer, cTransectVectorDisplay)
        Dim data As cTransectDatastructures = td.Data

        If Not TransectAt(e.Location, map, Me.m_transectEdit, Me.m_transectEditMode) Then

            ' Get transect names
            Dim lNames As New List(Of String)
            Dim strMask As String = "Transect {0}"
            For Each t As cTransect In data.Transects
                lNames.Add(t.Name)
            Next
            Dim n As Integer = EwEUtils.Utilities.cStringUtils.GetNextNumber(lNames.ToArray, strMask)

            Me.m_transectEdit = New cTransect(String.Format(strMask, n)) With {
                .Start = map.GetLocation(e.Location),
                .End = .Start
            }
            Me.m_transectEditMode = eEditModeType.End
            data.Add(Me.m_transectEdit)

        Else
            If (Me.m_transectEditMode = eEditModeType.Line) Then
                Me.m_ptClickOrg = e.Location
                Me.m_ptfStartOrg = Me.m_transectEdit.Start
                Me.m_ptfEndOrg = Me.m_transectEdit.End
            End If
        End If

        data.Selection = Me.m_transectEdit

        Me.StartEdit(e, map)
        map.UpdateMap()

    End Sub

    Public Overrides Sub ProcessMouseDraw(e As MouseEventArgs, map As ucMap)

        If (Me.IsEditing) Then

            Dim loc As PointF = map.GetLocation(e.Location)
            Dim td As cTransectVectorDisplay = DirectCast(Me.Layer, cTransectVectorDisplay)
            Dim data As cTransectDatastructures = td.Data

            Select Case Me.m_transectEditMode
                Case eEditModeType.Start
                    Me.m_transectEdit.Start = loc
                Case eEditModeType.End
                    Me.m_transectEdit.End = loc
                Case eEditModeType.Line
                    Dim locOrg As PointF = map.GetLocation(Me.m_ptClickOrg)
                    Dim dx As Single = loc.X - locOrg.X
                    Dim dy As Single = loc.Y - locOrg.Y
                    Me.m_transectEdit.Start = New PointF(Me.m_ptfStartOrg.X + dx, Me.m_ptfStartOrg.Y + dy)
                    Me.m_transectEdit.End = New PointF(Me.m_ptfEndOrg.X + dx, Me.m_ptfEndOrg.Y + dy)
            End Select

            data.OnChanged(Me.m_transectEdit)
            map.UpdateMap()

        End If

    End Sub

    Public Overrides Sub ProcessMouseUp()
        If Me.IsEditing Then
            Me.EndEdit()
        End If
    End Sub

    Protected Overrides Sub StartEdit(e As MouseEventArgs, map As ucMap)
        Me.IsEditing = True
    End Sub

    Protected Overrides Sub EndEdit()
        If (Me.m_transectEdit IsNot Nothing) Then
            Me.m_transectEdit.SortLocations()
            Me.m_transectEdit = Nothing
        End If
        Me.IsEditing = False
    End Sub

    Public Overrides Function Cursor(ptMouse As Point, map As ucMap) As Cursor

        Dim t As cTransect = Nothing
        Dim editmode As eEditModeType = eEditModeType.NotSet

        If TransectAt(ptMouse, map, t, editmode) Then
            Return Cursors.Hand
        End If
        Return Cursors.Default

    End Function

    Private Function TransectAt(ptMouse As Point, map As ucMap, ByRef t As cTransect, ByRef editMode As eEditModeType) As Boolean

        Dim td As cTransectVectorDisplay = DirectCast(Me.Layer, cTransectVectorDisplay)
        Dim data As cTransectDatastructures = td.Data
        Dim ptStart As Point = Nothing
        Dim ptEnd As Point = Nothing
        Dim sDist As Single = 10.0
        For Each t In data.Transects

            ptStart = map.GetScreenPoint(t.Start)
            If IsNear(ptStart, ptMouse, sDist) Then
                editMode = eEditModeType.Start
                Return True
            End If

            ptEnd = map.GetScreenPoint(t.End)
            If IsNear(ptEnd, ptMouse, sDist) Then
                editMode = eEditModeType.End
                Return True
            End If

            ' Distance to line segment
            ' http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment

            Dim a As Single = ptMouse.X - ptStart.X
            Dim b As Single = ptMouse.Y - ptStart.Y
            Dim c As Single = ptEnd.X - ptStart.X
            Dim d As Single = ptEnd.Y - ptStart.Y
            Dim dot As Single = a * c + b * d
            Dim len_sq As Single = c * c + d * d
            Dim parm As Single = -1

            If (len_sq <> 0) Then parm = dot / len_sq
            Dim x, y As Single

            If (parm < 0) Then
                x = ptStart.X
                y = ptStart.Y
            ElseIf (parm > 1) Then
                x = ptEnd.X
                y = ptEnd.Y
            Else
                x = ptStart.X + parm * c
                y = ptStart.Y + parm * d
            End If

            Dim dx As Single = ptMouse.X - x
            Dim dy As Single = ptMouse.Y - y
            If (Math.Sqrt(dx * dx + dy * dy) < sDist) Then
                editMode = eEditModeType.Line
                Return True
            End If
        Next
        t = Nothing
        Return False

    End Function

    Private Function IsNear(pt1 As Point, pt2 As Point, sDist As Single) As Boolean
        Dim dx As Single = pt1.X - pt2.X
        Dim dy As Single = pt1.Y - pt2.Y
        Return Math.Sqrt(dx * dx + dy * dy) <= sDist
    End Function

End Class
