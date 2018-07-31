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

Imports System.Math
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Definitions

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class that renderes a <see cref="IFlowDiagramData">flow diagram</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cFlowDiagramManager

#Region " Private vars "

        Private m_iHighlight As Integer = 0
        Private m_bIsMouseDown As Boolean = False
        Private m_tree As IFlowDiagramRenderer = Nothing
        Private m_data As IFlowDiagramData = Nothing

        Private Enum eDragMode As Integer
            None
            Label
            Node
        End Enum

        Private m_dragMode As eDragMode = eDragMode.None
        Private m_ptDragOffset As PointF = Nothing

#End Region ' Private vars

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor for a flow diagram renderer.
        ''' </summary>
        ''' <param name="data">The <see cref="IFlowDiagramData">data</see> for the flow diagram.</param>
        ''' <param name="tree">The <see cref="IFlowDiagramRenderer"/> tree to do 
        ''' the actual rendering and UI interactions.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal data As IFlowDiagramData, _
                       ByVal tree As IFlowDiagramRenderer)

            Me.m_data = data
            Me.m_tree = tree

        End Sub

#End Region ' Constructor

#Region " Configuration "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the node to highlight.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property HighlightNode() As Integer
            Get
                Return Me.m_iHighlight
            End Get
            Set(ByVal value As Integer)
                If (Me.m_iHighlight <> value) Then
                    Me.m_iHighlight = value
                End If
            End Set
        End Property

#End Region ' Configuration

#Region " Public access "

        Private Shared s_draworder As IFlowDiagramRenderer.eFDHighlightType() = _
            New IFlowDiagramRenderer.eFDHighlightType() {IFlowDiagramRenderer.eFDHighlightType.GrayedOut, IFlowDiagramRenderer.eFDHighlightType.None, IFlowDiagramRenderer.eFDHighlightType.Selected}

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Master draw instruction. There can be only one.
        ''' </summary>
        ''' <param name="g">Graphics to draw onto.</param>
        ''' <param name="rc">Rectangle to draw within.</param>
        ''' -------------------------------------------------------------------
        Public Sub DrawFlowDiagram(ByVal g As Graphics, ByVal rc As Rectangle)

            Me.m_tree.DrawBackground(g, rc)
            Me.m_tree.DrawTitle(g, rc)
            Me.m_tree.DrawLegend(g, New Point(5, 5))

            For Each hl As IFlowDiagramRenderer.eFDHighlightType In s_draworder
                DrawFlow(g, rc, hl)
            Next

        End Sub

        Private Sub DrawFlow(ByVal g As Graphics, ByVal rc As Rectangle, ByVal focus As IFlowDiagramRenderer.eFDHighlightType)

            Dim hl As IFlowDiagramRenderer.eFDHighlightType = IFlowDiagramRenderer.eFDHighlightType.None
            Dim bDraw As Boolean = False

            ' Draw connections
            For iPred As Integer = 1 To Me.m_data.NumItems()

                For iPrey As Integer = 1 To Me.m_data.NumItems()

                    Dim sDiet As Single = Me.m_data.LinkValue(iPred, iPrey)
                    If (sDiet > 0) Then

                        ' Determine highlight state
                        hl = IFlowDiagramRenderer.eFDHighlightType.None
                        bDraw = True

                        If (Not Me.m_data.IsItemVisible(iPred)) Or (Not Me.m_data.IsItemVisible(iPrey)) Then
                            bDraw = (Me.m_tree.ShowHiddenNodes = eFDShowHiddenType.GrayedOut)
                            hl = IFlowDiagramRenderer.eFDHighlightType.GrayedOut
                        End If

                        If (Me.m_iHighlight > 0) Then
                            If (Me.HighlightNode = iPred) Then
                                hl = IFlowDiagramRenderer.eFDHighlightType.LinkIn
                            ElseIf (Me.HighlightNode = iPrey) Then
                                hl = IFlowDiagramRenderer.eFDHighlightType.LinkOut
                            End If
                        Else
                        End If

                        Select Case focus
                            Case IFlowDiagramRenderer.eFDHighlightType.GrayedOut
                                bDraw = bDraw And (hl = IFlowDiagramRenderer.eFDHighlightType.GrayedOut)
                            Case IFlowDiagramRenderer.eFDHighlightType.None
                                bDraw = bDraw And (hl = IFlowDiagramRenderer.eFDHighlightType.None)
                            Case IFlowDiagramRenderer.eFDHighlightType.Selected
                                bDraw = bDraw And (hl = IFlowDiagramRenderer.eFDHighlightType.LinkIn) Or
                                                  (hl = IFlowDiagramRenderer.eFDHighlightType.LinkOut) Or
                                                  (hl = IFlowDiagramRenderer.eFDHighlightType.Selected)
                        End Select

                        If bDraw Then
                            Me.m_tree.DrawConnection(g, rc, iPred, iPrey, hl)
                        End If

                    End If
                Next
            Next

            ' Draw nodes
            For j As Integer = 1 To Me.m_data.NumItems

                ' Determine node highlight state
                hl = IFlowDiagramRenderer.eFDHighlightType.None
                bDraw = True

                If (Not Me.m_data.IsItemVisible(j)) Then
                    bDraw = (Me.m_tree.ShowHiddenNodes = eFDShowHiddenType.GrayedOut)
                    hl = IFlowDiagramRenderer.eFDHighlightType.GrayedOut
                End If

                If (Me.m_iHighlight > 0) Then
                    If (Me.m_data.LinkValue(Me.HighlightNode, j) > 0) Then
                        hl = IFlowDiagramRenderer.eFDHighlightType.LinkIn
                    ElseIf (Me.m_data.LinkValue(j, Me.HighlightNode) > 0) Then
                        hl = IFlowDiagramRenderer.eFDHighlightType.LinkOut
                    ElseIf (Me.HighlightNode = j) Then
                        hl = IFlowDiagramRenderer.eFDHighlightType.Selected
                     End If
                End If

                Select Case focus
                    Case IFlowDiagramRenderer.eFDHighlightType.GrayedOut
                        bDraw = bDraw And (hl = IFlowDiagramRenderer.eFDHighlightType.GrayedOut)
                    Case IFlowDiagramRenderer.eFDHighlightType.None
                        ' JS 06Apr18: draw nodes on top of lines, even when grayed-out
                        bDraw = bDraw And ((hl = IFlowDiagramRenderer.eFDHighlightType.None) Or (hl = IFlowDiagramRenderer.eFDHighlightType.GrayedOut))
                    Case IFlowDiagramRenderer.eFDHighlightType.Selected
                        bDraw = bDraw And (hl = IFlowDiagramRenderer.eFDHighlightType.LinkIn) Or _
                                          (hl = IFlowDiagramRenderer.eFDHighlightType.LinkOut) Or _
                                          (hl = IFlowDiagramRenderer.eFDHighlightType.Selected)
                End Select

                If (bDraw) Then
                    Me.m_tree.DrawNode(g, rc, j, hl)
                End If

            Next j

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Handle a mouse move operation.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rc"></param>
        ''' <param name="pt"></param>
        ''' -------------------------------------------------------------------
        Public Sub ProcessMouseMove(ByVal g As Graphics, ByVal rc As Rectangle, ByVal pt As PointF)

            Dim iNode As Integer = 0

            ' Dragging?
            Select Case Me.m_dragMode

                Case eDragMode.None

                    ' Not dragging: determine which node to highlight
                    Me.HighlightNode = 0

                    Using ft As Font = Me.m_tree.RenderFont
                        iNode = Me.GetLabelAtPoint(rc, pt, g, ft)
                    End Using

                    If iNode = 0 Then
                        iNode = Me.GetNodeAtPoint(rc, pt)
                    End If

                    If (iNode > 0) Then
                        Me.HighlightNode = iNode
                    End If

                Case eDragMode.Label
                    Me.m_tree.MoveLabel(rc, New PointF(pt.X - Me.m_ptDragOffset.X, pt.Y - Me.m_ptDragOffset.Y), Me.HighlightNode)

                Case eDragMode.Node
                    Me.m_tree.MoveNode(rc, New PointF(pt.X - Me.m_ptDragOffset.X, pt.Y - Me.m_ptDragOffset.Y), Me.HighlightNode)

            End Select

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the diagram layout to a file.
        ''' </summary>
        ''' <param name="settings">The <see cref="cXMLSettings">settings</see> to save to.</param>
        ''' <param name="ctrl">The control that renders the diagram.</param>
        ''' -------------------------------------------------------------------
        Public Function Save(ByVal settings As cXMLSettings, ByVal ctrl As Control) As Boolean

            Try
                Dim rc As Rectangle = ctrl.ClientRectangle()

                settings.SaveSetting("Global", "NumGroups", Me.m_data.NumItems)
                settings.SaveSetting("Global", "Width", rc.Width)
                settings.SaveSetting("Global", "Height", rc.Height)
                For i As Integer = 1 To Me.m_data.NumItems
                    settings.SaveSetting("Locations", i.ToString + "x", cStringUtils.FormatNumber(Me.m_tree.NodeLocation(i, rc).X))
                    settings.SaveSetting("Locations", i.ToString + "y", cStringUtils.FormatNumber(Me.m_tree.NodeLocation(i, rc).Y))
                    settings.SaveSetting("Locations", i.ToString + "xlabel", cStringUtils.FormatNumber(Me.m_tree.LabelLocation(i, rc).X))
                    settings.SaveSetting("Locations", i.ToString + "ylabel", cStringUtils.FormatNumber(Me.m_tree.LabelLocation(i, rc).Y))
                Next i
                settings.Flush()

            Catch ex As Exception
                ' ToDo: send an error message
                cLog.Write(ex, "FlowDiagram.SaveToFile")
                Return False
            End Try
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load the diagram layout from a file.
        ''' </summary>
        ''' <param name="settings">The <see cref="cXMLSettings">file</see> to load from.</param>
        ''' <param name="ctrl">The control that draws the diagram.</param>
        ''' -------------------------------------------------------------------
        Public Function Load(ByVal settings As cXMLSettings, ByVal ctrl As Control) As Boolean

            Try

                Dim ptf As PointF
                Dim iNumGroups As Integer = Math.Min(CInt(settings.GetSetting("Global", "NumGroups", "0")), Me.m_data.NumItems)
                ctrl.ClientSize = New Size(CInt(settings.GetSetting("Global", "Width", "100")), CInt(settings.GetSetting("Global", "Height", "100")))
                Dim rc As Rectangle = ctrl.ClientRectangle()

                For i As Integer = 1 To iNumGroups
                    ptf.X = cStringUtils.ConvertToSingle(settings.GetSetting("Locations", i.ToString + "x", "0"))
                    ptf.Y = cStringUtils.ConvertToSingle(settings.GetSetting("Locations", i.ToString + "y", "0"))
                    Me.m_tree.NodeLocation(i, rc) = ptf
                    ptf.X = cStringUtils.ConvertToSingle(settings.GetSetting("Locations", i.ToString + "xlabel", "10"))
                    ptf.Y = cStringUtils.ConvertToSingle(settings.GetSetting("Locations", i.ToString + "ylabel", "10"))
                    Me.m_tree.LabelLocation(i, rc) = ptf
                Next i

            Catch ex As Exception
                ' ToDo: send an error message
                cLog.Write(ex, "FlowDiagram.SaveToFile")
                Return False
            End Try
            Return True

        End Function

#End Region ' Public access

#Region " Dragging "

        Public Sub BeginDrag(ByVal rc As Rectangle, ByVal pt As PointF, ByVal g As Graphics)

            If (Me.IsDragging) Then Return
            If (Me.HighlightNode = 0) Then Return

            Dim iNode As Integer = -1
            Dim ptItem As PointF

            Using ft As Font = Me.m_tree.RenderFont
                iNode = Me.GetLabelAtPoint(rc, pt, g, ft)
                If (iNode = Me.HighlightNode) Then
                    Me.m_dragMode = eDragMode.Label
                    ptItem = Me.m_tree.LabelLocation(iNode, rc)
                    Me.m_ptDragOffset = New PointF(pt.X - ptItem.X, pt.Y - ptItem.Y)
                    Return
                End If
            End Using

            iNode = Me.GetNodeAtPoint(rc, pt)
            If iNode = HighlightNode Then
                Me.m_dragMode = eDragMode.Node
                ptItem = Me.m_tree.NodeLocation(iNode, rc)
                Me.m_ptDragOffset = New PointF(pt.X - ptItem.X, pt.Y - ptItem.Y)
            End If

        End Sub

        Public Sub EndDrag(ByVal fdData As IFlowDiagramData, ByVal pt As PointF)
            Me.m_dragMode = eDragMode.None
        End Sub

        Public Function IsDragging() As Boolean
            Return (Me.m_dragMode <> eDragMode.None)
        End Function

#End Region ' Dragging

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the index of a node at a given location.
        ''' <seealso cref="GetLabelAtPoint"/>
        ''' </summary>
        ''' <param name="rc">Flow diagram area to find the node within.</param>
        ''' <param name="pt">Point to test for.</param>
        ''' <returns>
        ''' Returns the index of a <see cref="IFlowDiagramData.IsItemVisible">visible</see>
        ''' node at the location, or, of not found, return a non-visible node at the
        ''' location only if <see cref="IFlowDiagramRenderer.ShowHiddenNodes"/> is not set to 
        ''' <see cref="eFDShowHiddenType.Invisible"/>. If no node was found, 0 
        ''' is returned.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function GetNodeAtPoint(ByVal rc As Rectangle, ByVal pt As PointF) As Integer

            Dim iNodeViz As Integer = 0
            Dim iNodeHid As Integer = 0
            Dim iNodeTmp As Integer = 1

            While (iNodeTmp <= Me.m_data.NumItems) And (iNodeViz = 0)
                If Me.m_tree.IsNodeAtPoint(rc, pt, iNodeTmp, Me.m_data.Value(iNodeTmp)) Then
                    If (Me.m_data.IsItemVisible(iNodeTmp)) Then
                        iNodeViz = iNodeTmp
                    Else
                        If (Me.m_tree.ShowHiddenNodes <> eFDShowHiddenType.Invisible) Then
                            iNodeHid = iNodeTmp
                        End If
                    End If
                End If
                iNodeTmp += 1
            End While

            Return If(iNodeViz > 0, iNodeViz, iNodeHid)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the index of a visible node label at a given location.
        ''' <seealso cref="GetLabelAtPoint"/>
        ''' </summary>
        ''' <param name="rc">Flow diagram area to find the node label within.</param>
        ''' <param name="pt">Point to test for.</param>
        ''' <returns>
        ''' Returns the index of a <see cref="IFlowDiagramData.IsItemVisible">visible</see>
        ''' node at the location, or 0 if no node was found.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function GetLabelAtPoint(ByVal rc As Rectangle,
                                         ByVal pt As PointF,
                                         ByVal g As Graphics,
                                         ByVal font As Font) As Integer

            Dim iNodeViz As Integer = 0
            Dim iNodeHid As Integer = 0
            Dim iNodeTmp As Integer = 1

            While (iNodeTmp <= Me.m_data.NumItems) And (iNodeViz = 0)
                If Me.m_tree.IsLabelAtPoint(rc, pt, iNodeTmp, Me.m_tree.FormatLabelText(iNodeTmp), g, font) Then
                    If (Me.m_data.IsItemVisible(iNodeTmp)) Then
                        iNodeViz = iNodeTmp
                    Else
                        If (Me.m_tree.ShowHiddenNodes <> eFDShowHiddenType.Invisible) Then
                            iNodeHid = iNodeTmp
                        End If
                    End If
                End If
                iNodeTmp += 1
            End While

            Return If(iNodeViz > 0, iNodeViz, iNodeHid)

        End Function

#End Region ' Internals

    End Class

End Namespace
