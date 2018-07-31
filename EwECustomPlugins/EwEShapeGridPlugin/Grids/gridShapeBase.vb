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
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.DataModels
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Foundation class for showing <see cref="cShapeData"/> in a grid.
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class gridShapeBase
    Inherits EwEGrid

    Private m_bIsSeasonal As Boolean = False
    Private m_lInvalidatedShapes As New List(Of cShapeData)

    Public Sub New()
        Me.TrackPropertySelection = False
    End Sub

    Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
            If (MyBase.UIContext IsNot Nothing) Then
                RemoveHandler Me.Handler.OnRefreshed, AddressOf OnRefreshed
                Me.Handler.Detach()
            End If
            MyBase.UIContext = value
            If (MyBase.UIContext IsNot Nothing) Then
                Me.Handler.Attach(Nothing, Nothing, Nothing, Nothing)
                AddHandler Me.Handler.OnRefreshed, AddressOf OnRefreshed
                Me.RefreshContent()
            End If
        End Set
    End Property

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()
        Me.FixedColumnWidths = False
        Me.FixedColumns = 1
    End Sub

    Public Property IsSeasonal() As Boolean
        Get
            Return Me.m_bIsSeasonal
        End Get
        Set(ByVal value As Boolean)
            If (Me.m_bIsSeasonal = value) Then Return
            Me.m_bIsSeasonal = value
            Me.RefreshContent()
        End Set
    End Property

    Public Overridable ReadOnly Property IsMonthly As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overridable Property ShowAllData As Boolean
        Get
            Return True
        End Get
        Set(value As Boolean)
            ' NOP
        End Set
    End Property

    Public MustOverride ReadOnly Property Handler() As cShapeGUIHandler
    Public MustOverride ReadOnly Property Manager() As IEnumerable
    Protected MustOverride Sub OnRefreshed(ByVal sender As cShapeGUIHandler)

    Public ReadOnly Property Shapes() As EwECore.cShapeData()
        Get
            Dim lShapes As New List(Of cShapeData)
            Dim shape As cShapeData = Nothing
            Dim man As IEnumerable = Me.Manager

            If man IsNot Nothing Then
                For Each item As Object In man
                    If TypeOf item Is cShapeData Then
                        shape = DirectCast(item, cShapeData)
                        If Me.Include(shape) Then
                            lShapes.Add(shape)
                        End If
                    End If
                Next
            End If

            Return lShapes.ToArray
        End Get
    End Property

    Protected Overridable Function Include(shape As cShapeData) As Boolean
        Return (shape.IsSeasonal = Me.IsSeasonal)
    End Function

    Protected Property Shape(ByVal iCol As Integer) As cShapeData
        Get
            Return DirectCast(Me.Columns(iCol).Tag, cShapeData)
        End Get
        Set(ByVal value As cShapeData)
            Me.Columns(iCol).Tag = value
        End Set
    End Property

    Protected Sub InvalidateShape(ByVal iCol As Integer)
        Me.InvalidateShape(Me.Shape(iCol))
    End Sub

    Protected Sub InvalidateShape(ByVal shape As cShapeData)

        If Not Me.m_lInvalidatedShapes.Contains(shape) Then
            Me.m_lInvalidatedShapes.Add(shape)
        End If

    End Sub

    Public Overrides Sub BeginBatchEdit()
        Me.m_lInvalidatedShapes.Clear()
        MyBase.BeginBatchEdit()
    End Sub

    Public Overrides Sub EndBatchEdit()
        Try
            For Each sh As cShapeData In Me.m_lInvalidatedShapes
                sh.Update()
            Next
        Catch ex As Exception

        End Try
        Me.m_lInvalidatedShapes.Clear()
        MyBase.EndBatchEdit()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return a formatted X-axis label for a data point.
    ''' </summary>
    ''' <param name="iPoint">The zero-based point index to return the label for.</param>
    ''' <returns>A formatted X-axis label for a data point.</returns>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function Label(ByVal iPoint As Integer) As String

End Class
