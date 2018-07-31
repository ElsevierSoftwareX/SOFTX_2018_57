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

Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceLibrary
Imports System.ComponentModel

#End Region

<CLSCompliant(False)> _
Public MustInherit Class gridApplyShapeBase
    Inherits EwEGrid

#Region " Private vars "

    Protected m_editor As DataModels.EditorTextBox = Nothing
    Protected m_interactionManager As cMediatedInteractionManager = Nothing
    Protected m_bmRowCol As BehaviorModels.CustomEvents = Nothing
    Protected m_bmCell As BehaviorModels.CustomEvents = Nothing

#End Region ' Private vars

#Region " Construction and destruction "

    Public Sub New()
        MyBase.New()

        Me.m_editor = New DataModels.EditorTextBox(GetType(String))
        Me.m_bmRowCol = New BehaviorModels.CustomEvents()
        Me.m_bmCell = New BehaviorModels.CustomEvents()

        AddHandler m_bmRowCol.Click, New SourceGrid2.PositionEventHandler(AddressOf OnRowColClicked)
        AddHandler m_bmCell.Click, AddressOf CellClick

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)

        If (Me.m_editor IsNot Nothing) Then
            RemoveHandler m_bmRowCol.Click, New SourceGrid2.PositionEventHandler(AddressOf OnRowColClicked)
            RemoveHandler m_bmCell.Click, AddressOf CellClick
            Me.m_editor = Nothing
            Me.m_bmCell = Nothing
            Me.m_bmRowCol = Nothing
        End If

    End Sub

#End Region ' Construction and destruction

#Region " Public access "

    <Browsable(False)> _
    Public Overrides Property UIContext() As cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As cUIContext)
            If (value IsNot Nothing) Then
                ' First set crucial properties
                Me.m_interactionManager = value.Core.MediatedInteractionManager
            Else
                Me.m_interactionManager = Nothing
            End If
            ' Always pass this to the grid
            MyBase.UIContext = value
        End Set
    End Property

    Public MustOverride Sub ClearAllPairs()

    Public MustOverride Sub SetAllPairs()

    ''' <summary>
    ''' Repopulate content without redimensioning
    ''' </summary>
    Public Sub UpdateContent()
        Me.FillData()
    End Sub

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.FixedColumnWidths = False
    End Sub

#End Region ' Overrides 

#Region " Internals "

    Protected MustOverride Sub CellClick(ByVal sender As Object, ByVal e As PositionEventArgs)

    Protected MustOverride Sub OnRowColClicked(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the group at a given row.
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' -------------------------------------------------------------------
    Protected Property GroupAtRow(ByVal iRow As Integer) As Integer
        Get
            If (1 <= iRow And iRow < Me.RowsCount) Then
                Return CInt(Me.Rows(iRow).Tag)
            End If
            Return 0
        End Get
        Set(value As Integer)
            If (1 <= iRow And iRow < Me.RowsCount) Then
                Me.Rows(iRow).Tag = value
            End If
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the group at a given column.
    ''' </summary>
    ''' <param name="iCol"></param>
    ''' -------------------------------------------------------------------
    Protected Property GroupAtColumn(ByVal iCol As Integer) As Integer
        Get
            If (2 <= iCol And iCol < Me.ColumnsCount) Then
                Return CInt(Me.Columns(iCol).Tag)
            End If
            Return 0
        End Get
        Set(value As Integer)
            If (2 <= iCol And iCol < Me.ColumnsCount) Then
                Me.Columns(iCol).Tag = value
            End If
        End Set
    End Property

#End Region ' Internals

End Class
