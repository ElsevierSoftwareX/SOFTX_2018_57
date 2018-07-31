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

Option Strict On
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style
Imports SourceGrid2

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Parent grid cell for collapsing and expanding a collection of child rows.
    ''' </summary>
    ''' <remarks>
    ''' Cells of this type maintain a list of child rows that can be collapsed
    ''' or expanded via <see cref="EwEHierarchyGridCell.Expanded">Expanded</see>.
    ''' Add child rows via <see cref="EwEHierarchyGridCell.AddChildRow">AddChildRow</see>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class EwEHierarchyGridCell
        : Inherits EwECellBase

#Region " Private vars "

        Private m_bExpanded As Boolean = True
        Private m_viz As New cVisualizerEwECollapseExpandRowHeader()
        Private m_lChildRows As New List(Of RowInfo)

#End Region ' Private vars

        Public Sub New()
            MyBase.New("", GetType(String))
            Me.VisualModel = m_viz
            Me.DataModel.EditableMode = SourceGrid2.EditableMode.None
            Me.Expanded = Me.m_bExpanded
        End Sub

        Public Property Expanded() As Boolean
            Get
                Return Me.m_bExpanded
            End Get
            Set(ByVal bExpanded As Boolean)
                ' Store flag
                Me.m_bExpanded = bExpanded
                ' Show/hide child rows
                Me.ShowHideChildren()
                ' Update viz
                Me.UpdateViz()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a child row to the hierarchy cell.
        ''' </summary>
        ''' <param name="iRow">Index of the row to add.</param>
        ''' -------------------------------------------------------------------
        Public Sub AddChildRow(ByVal iRow As Integer)

            Dim iPos As Integer = 0

            ' Add in descending order
            While iPos < Me.m_lChildRows.Count()
                If (Me.m_lChildRows(iPos).Index < iRow) Then Exit While
                iPos += 1
            End While
            Me.m_lChildRows.Insert(iPos, Me.Grid.Rows(iRow))
            Me.UpdateViz()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a child row from the hierarchy cell.
        ''' </summary>
        ''' <param name="iRow">Index of the row to remove.</param>
        ''' -------------------------------------------------------------------
        Public Sub RemoveChildRow(ByVal iRow As Integer)
            For Each ri As RowInfo In Me.m_lChildRows
                If ri.Index = iRow Then
                    Me.m_lChildRows.Remove(ri)
                    Exit For
                End If
            Next
            Me.UpdateViz()
        End Sub

        ''' <summary>
        ''' Get the number of child rows.
        ''' </summary>
        Public ReadOnly Property NumChildRows() As Integer
            Get
                Return Me.m_lChildRows.Count
            End Get
        End Property

        Private Sub ShowHideChildren()
            Dim g As GridVirtual = Me.Grid

            If g IsNot Nothing Then
                For Each ri As RowInfo In Me.m_lChildRows
                    If ri IsNot Nothing Then
                        ri.Visible = Me.m_bExpanded
                    End If
                Next
            End If
        End Sub

        Public Overrides Sub OnClick(ByVal e As SourceGrid2.PositionEventArgs)
            ' MyBase.OnClick(e)
            Me.Expanded = Not Me.Expanded
        End Sub

        Public Overrides Property Style() As cStyleGuide.eStyleFlags
            Get
                Return cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Names
            End Get
            Set(ByVal value As cStyleGuide.eStyleFlags)
                ' No style
            End Set
        End Property

        Private Sub UpdateViz()
            Try
                ' Update visualizer
                If Me.NumChildRows = 0 Then
                    Me.m_viz.SetCollapsedState(cVisualizerEwECollapseExpandRowHeader.eCollapsedState.NoChildren)
                ElseIf Me.m_bExpanded Then
                    Me.m_viz.SetCollapsedState(cVisualizerEwECollapseExpandRowHeader.eCollapsedState.Expanded)
                Else
                    Me.m_viz.SetCollapsedState(cVisualizerEwECollapseExpandRowHeader.eCollapsedState.Collapsed)
                End If
            Catch ex As Exception

            End Try
        End Sub

    End Class

End Namespace
