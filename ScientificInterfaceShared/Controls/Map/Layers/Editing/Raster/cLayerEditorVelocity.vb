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

#End Region ' Imports 

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer editor that supports interactions with <see cref="cEcospaceLayerVelocity">velocity layers</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerEditorVelocity
        Inherits cLayerEditorRaster
        Implements IMonthFilter

#Region " Private vars "

        Private m_ptfDelta As PointF = Nothing
        Private m_szfCell As SizeF = Nothing

#End Region ' Private vars

#Region " Construction "

        Public Sub New()
            'MyBase.New(GetType(ucLayerEditorRange))
            MyBase.New(GetType(ucLayerEditorMonthVelocity))
        End Sub

#End Region ' Construction

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cLayerEditor.ProcessMouseDraw"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub Edit(ByVal ptFrom As Point,
                                  ByVal ptTo As Point,
                                  ByVal ptDeltaMouse As Point,
                                  ByVal szfCell As SizeF,
                                  ByVal args As MouseEventArgs,
                                  ByRef ptUpdateMin As Point,
                                  ByRef ptUpdateMax As Point)

            Me.m_ptfDelta = New PointF(ptDeltaMouse.X, ptDeltaMouse.Y)
            Me.m_szfCell = New SizeF(szfCell.Width, szfCell.Height)

            MyBase.Edit(ptFrom, ptTo, ptDeltaMouse, szfCell, args, ptUpdateMin, ptUpdateMax)

        End Sub

#End Region ' Public interfaces

#Region " Internal overrides "

        Public Overrides Sub Reset()

            If (Not Me.IsEditable) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth
            Dim l As cEcospaceLayerVelocity = CType(Me.Layer.Data, cEcospaceLayerVelocity)
            Dim v As Single = CSng(Me.CellValue)

            For i As Integer = 1 To bm.InRow
                For j As Integer = 1 To bm.InCol
                    If layerDepth.IsWaterCell(i, j) Then
                        l.XVelocity(i, j) = v
                        l.YVelocity(i, j) = v
                    End If
                Next j
            Next i
            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Smooth layer data across water cells.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Smooth()

            If (Not Me.IsEditable) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth
            Dim layer As cEcospaceLayerVelocity = CType(Me.Layer.Data, cEcospaceLayerVelocity)
            Dim cnewx(,) As Single
            Dim cnewy(,) As Single
            Dim i As Integer, j As Integer
            Dim tx As Single
            Dim ty As Single
            Dim n As Integer

            ReDim cnewx(bm.InRow, bm.InCol)
            ReDim cnewy(bm.InRow, bm.InCol)

            For i = 1 To bm.InRow
                For j = 1 To bm.InCol
                    tx = 0
                    ty = 0
                    n = 0
                    For ii As Integer = i - 1 To i + 1
                        For jj As Integer = j - 1 To j + 1
                            If Not (ii = 0 Or jj = 0 Or ii = bm.InRow + 1 Or jj = bm.InCol + 1) And layerDepth.IsWaterCell(ii, jj) Then
                                tx += layer.XVelocity(ii, jj)
                                ty += layer.YVelocity(ii, jj)
                                n += 1
                            End If
                        Next jj
                    Next ii
                    If n > 0 Then
                        cnewx(i, j) = tx / n
                        cnewy(i, j) = ty / n
                    End If
                Next j
            Next i

            For i = 1 To bm.InRow
                For j = 1 To bm.InCol
                    If layerDepth.IsWaterCell(i, j) Then
                        layer.XVelocity(i, j) = cnewx(i, j)
                        layer.YVelocity(i, j) = cnewy(i, j)
                    End If
                Next
            Next
            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Duplicate layer data across indexed layers.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Duplicate(ByVal iFrom As Integer)

            If (Not Me.IsEditable) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim layer As cEcospaceLayerVelocity = CType(Me.Layer.Data, cEcospaceLayerVelocity)
            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth
            Dim cc As Integer = Me.UIContext.Core.GetCoreCounter(Me.Layer.Data.SecundaryIndexCounter)
            Dim sX As Single = 0
            Dim sY As Single = 0

            For i As Integer = 0 To bm.InRow + 1
                For j As Integer = 0 To bm.InCol + 1
                    If (layerDepth.IsWaterCell(i, j)) Then
                        sX = layer.XVelocity(i, j, iFrom)
                        sY = layer.YVelocity(i, j, iFrom)
                        For k As Integer = 1 To cc
                            If (k <> iFrom) Then
                                layer.XVelocity(i, j, k) = sX
                                layer.YVelocity(i, j, k) = sY
                            End If
                        Next k
                    End If
                Next j
            Next i

            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overidden to set a vector into a single cell.
        ''' </summary>
        ''' <param name="ptSet">The cell location (Col, Row) to set.</param>
        ''' <param name="value">A array of 2 Single values</param>
        ''' <param name="e">Mouse event args accompanying this action.</param>
        ''' <param name="ptClick">The cell location (Col, Row) in the cursor.</param>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub SetCellValue(ByVal ptSet As Point,
                                             ByVal value As Object,
                                             ByVal e As MouseEventArgs,
                                             ByVal ptClick As Point)

            If (Not Me.IsEditable) Then Return

            ' Calc the distance the mouse has travelled
            Dim dx As Single = CSng(Math.Sqrt(Me.m_ptfDelta.X * Me.m_ptfDelta.X + Me.m_ptfDelta.Y * Me.m_ptfDelta.Y))
            ' Only process significant changes
            If dx <= 2 Then Return

            Dim sVal As Single = CSng(Me.CellValue)
            Me.Layer.Value(ptSet.Y, ptSet.X) = New Single() {Me.m_ptfDelta.X * sVal / dx,
                                                             Me.m_ptfDelta.Y * sVal / dx}

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Pick up the cell value at a given point, and store this value in the
        ''' layer editor as the next value that will be set.
        ''' Overridden to pick up the scale factor at a given location.
        ''' </summary>
        ''' <param name="pt">The cell location to pick up a value from.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Pickup(ByVal pt As System.Drawing.Point)

            Try
                ' JS: pt(X,Y) translated to value(row, col); it never fails to confuse me. Even if I wrote this code...
                Dim asValue As Single() = DirectCast(Me.Layer.Value(pt.Y, pt.X), Single())
                Me.CellValue = CSng(Math.Sqrt(asValue(0) * asValue(0) + asValue(1) * asValue(1)))

                ' Notify the editor GUI, if any
                If Me.GUI IsNot Nothing Then
                    Me.GUI.UpdateContent(Me)
                End If

            Catch ex As Exception
            End Try

        End Sub

#End Region ' Internal overrides

#Region " Month filter "

        Public Event OnFilterChanged(sender As IContentFilter) Implements IContentFilter.FilterChanged

        Public Property Month As Integer Implements IMonthFilter.Month
            Get
                Dim l As cEcospaceLayerVelocity = CType(Me.Layer.Data, cEcospaceLayerVelocity)
                Return l.Month
            End Get
            Set(value As Integer)
                Dim l As cEcospaceLayerVelocity = CType(Me.Layer.Data, cEcospaceLayerVelocity)
                ' Will month index change?
                If (value <> l.Month) Then
                    ' #Yes: update month index in the underlying Ecospace layer
                    l.Month = value
                    ' Force map update
                    Me.Layer.Update(cDisplayLayer.eChangeFlags.Map Or cDisplayLayer.eChangeFlags.Descriptive, False)
                    Try
                        RaiseEvent OnFilterChanged(Me)
                    Catch ex As Exception
                        ' NOP
                    End Try
                    ' Last update UI
                    If (Me.GUI IsNot Nothing) Then
                        Me.GUI.UpdateContent(Me)
                    End If
                End If
            End Set
        End Property

#End Region ' Month filter

    End Class

End Namespace