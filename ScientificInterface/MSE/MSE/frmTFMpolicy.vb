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
Imports EwECore
Imports EwEUtils.Core
Imports EwECore.MSE
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports System.Windows.Forms
Imports ZedGraph

#End Region ' Imports

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form, implementing the Ecosim Fishing policy mortality (a.k.a hockey stick) 
    ''' interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmTargetFishingMortalityPolicy

#Region " Internals "

        Private Enum eDragType As Integer
            None = 0
            BLim
            BBaseFopt
            Fopt
        End Enum

        ''' <summary><see cref="cZedGraphHelper">Helper</see> to manipulate the graph.</summary>
        Private m_zgh As cZedGraphHelper = Nothing
        ''' <summary>Group selected in the form.</summary>
        Private m_group As cMSEGroupInput = Nothing
        ''' <summary>Graph drag mode.</summary>
        Private m_dragtype As eDragType = eDragType.None

#End Region ' Internals

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If Me.UIContext Is Nothing Then Return

            Me.m_zgh = New cZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_graph)
            Me.m_zgh.ConfigurePane("", SharedResources.HEADER_BIOMASS, SharedResources.HEADER_TFM, True)

            Me.m_zgh.AllowZoom = False
            Me.m_zgh.AllowPan = False
            Me.m_zgh.AllowEdit = True

            Me.m_grid.UIContext = Me.UIContext
            If (Core.nGroups > 0) Then
                Me.m_grid.Group = Me.Core.MSEManager.GroupInputs(1)
            End If

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If Me.m_zgh IsNot Nothing Then
                Me.Group = Nothing
                Me.m_zgh.Detach()
                Me.m_zgh = Nothing
            End If

            MyBase.OnFormClosed(e)
        End Sub

        Private Sub HandleGridSelectionChanged() _
            Handles m_grid.OnSelectionChanged
            ' Update group selection according to user actions in the grid
            Me.Group = Me.m_grid.Group
        End Sub

        Private Sub HandlePropertyChanged(ByVal prop As cProperty, ByVal cf As cProperty.eChangeFlags)
            ' A relevant property has changed: redraw the graph
            Me.Redraw()
        End Sub

        Private Sub tsbDefaultTFM_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbDefaultTFM.Click
            Try
                Me.UIContext.Core.SetDefaultTFM()
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Sub

#End Region ' Events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group in the form
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property Group() As cMSEGroupInput
            Get
                Return Me.m_group
            End Get
            Set(ByVal value As cMSEGroupInput)

                Dim pm As cPropertyManager = Me.PropertyManager

                ' Unregister
                If (Me.m_group IsNot Nothing) Then
                    RemoveHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEBLim).PropertyChanged, AddressOf HandlePropertyChanged
                    RemoveHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEBBase).PropertyChanged, AddressOf HandlePropertyChanged
                    RemoveHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEFmax).PropertyChanged, AddressOf HandlePropertyChanged
                End If

                ' Update
                Me.m_group = value

                ' Register
                If (Me.m_group IsNot Nothing) Then
                    AddHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEBLim).PropertyChanged, AddressOf HandlePropertyChanged
                    AddHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEBBase).PropertyChanged, AddressOf HandlePropertyChanged
                    AddHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEFmax).PropertyChanged, AddressOf HandlePropertyChanged
                End If

                ' Ledlaw the glaph
                Me.Redraw()

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Redraw the quota curve.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Redraw()

            If Me.m_zgh Is Nothing Then Return

            Dim lpts As New PointPairList
            Dim line As LineItem = Nothing
            Dim lLines As New List(Of LineItem)

            If (Me.m_group IsNot Nothing) Then
                ' Group has data?
                If (Me.m_group.GetStatus(eVarNameFlags.MSEBBase) And eStatusFlags.Null) = 0 Then
                    ' #Yes: plot stick
                    Dim bsum As Single = Me.m_group.BLim + Me.m_group.BBase
                    If bsum > 0 Then
                        ' Add points
                        lpts.Add(0, 0)
                        lpts.Add(Me.m_group.BLim, 0)
                        lpts.Add(Me.m_group.BBase, Me.m_group.FOpt) ' Point order?
                        lpts.Add(Me.m_group.BBase * 4, Me.m_group.FOpt) ' Max X value?
                    Else
                        'Zero biomass values user has only entered F
                        'draw a square line at zero up to F
                        lpts.Add(-1, 0)
                        lpts.Add(0, 0)
                        lpts.Add(0, Me.m_group.FOpt) ' Point order?
                        lpts.Add(4, Me.m_group.FOpt) ' Max X value?
                    End If

                    line = New LineItem(Me.m_group.Name, _
                    lpts, _
                    Me.StyleGuide.GroupColor(Me.Core, Me.m_group.Index), _
                    SymbolType.Circle)
                    line.Line.Width = 2.0

                    lLines.Add(line)

                End If

                End If

                If lLines.Count > 0 Then
                    ' Plot graph, but rescale ONLY when not dragging
                    Me.m_zgh.PlotLines(lLines.ToArray, 1, (Me.m_dragtype = eDragType.None))
                    Me.m_graph.Cursor = Cursors.Default
                Else
                    ' Clear graph
                    Me.m_zgh.PlotLines(Nothing)
                    Me.m_graph.Cursor = Cursors.No
                End If

        End Sub

#End Region ' Internals

#Region " Dragging "

        Private Function HandleGraphMouseDownEvent(ByVal sender As ZedGraphControl, ByVal e As MouseEventArgs) As Boolean _
                Handles m_graph.MouseDownEvent

            Dim pane As GraphPane = sender.GraphPane
            Dim pt As PointF = New PointF(e.X, e.Y)
            Dim curve As CurveItem = Nothing
            Dim iIndex As Integer = 0

            ' Find the point that was clicked, and make sure the point list is editable
            If (pane.FindNearestPoint(pt, curve, iIndex)) Then
                If (curve IsNot Nothing) Then
                    If (TypeOf curve.Points Is PointPairList) Then
                        ' Set drag operation type
                        Me.m_dragtype = DirectCast(iIndex, eDragType)
                    End If
                End If
            End If

            Return False

        End Function

        Private Function m_graph_MouseMoveEvent(ByVal sender As ZedGraphControl, ByVal e As MouseEventArgs) As Boolean _
            Handles m_graph.MouseMoveEvent

            Dim pane As GraphPane = sender.GraphPane
            Dim pt As PointF = New PointF(e.X, e.Y)
            Dim curve As CurveItem = Nothing
            Dim iIndex As Integer = 0
            Dim bIsNear As Boolean = False

            ' Find the point that was clicked, and make sure the point list is editable
            If (pane.FindNearestPoint(pt, curve, iIndex)) Then
                bIsNear = (curve IsNot Nothing) 
            End If

            If bIsNear Then
                Me.m_graph.Cursor = Cursors.Hand
            Else
                Me.m_graph.Cursor = Cursors.Default
            End If
            Return True

        End Function

        Private Function HandleGraphMouseMoveEvent(ByVal sender As ZedGraphControl, ByVal e As MouseEventArgs) As Boolean _
                Handles m_graph.MouseMoveEvent

            Dim pane As GraphPane = sender.GraphPane
            Dim pt As PointF = New PointF(e.X, e.Y)
            Dim dX As Double = 0.0
            Dim dy As Double = 0.0

            ' Dragging?
            If (Me.m_dragtype <> eDragType.None) Then
                ' Translate value
                pane.ReverseTransform(pt, dX, dy)

                Select Case Me.m_dragtype
                    Case eDragType.BLim
                        Me.m_group.BLim = Math.Max(0, Math.Min(CSng(dX), Me.m_group.BBase))
                    Case eDragType.BBaseFopt
                        Me.m_group.BBase = Math.Max(Me.m_group.BLim, CSng(dX))
                        Me.m_group.FOpt = Math.Max(0, CSng(dy))
                    Case eDragType.Fopt
                        Me.m_group.FOpt = Math.Max(0, CSng(dy))
                End Select

            End If
            Return True

        End Function

        Private Function HandleGraphMouseUpEvent(ByVal sender As ZedGraphControl, ByVal e As MouseEventArgs) As Boolean _
                Handles m_graph.MouseUpEvent

            Me.m_dragtype = eDragType.None
            Me.m_zgh.RescaleAndRedraw()
            Return True

        End Function

#End Region ' Dragging

    End Class

End Namespace
