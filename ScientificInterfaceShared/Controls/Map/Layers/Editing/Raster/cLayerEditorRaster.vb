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
Imports EwEUtils.Utilities

#End Region ' Imports 

Namespace Controls.Map.Layers

    Public MustInherit Class cLayerEditorRaster
        Inherits cLayerEditor

#Region " Private vars "

        ''' <summary>The current value 'under the cursor'.</summary>
        Private m_sValue As Single = 0
        ''' <summary>Draw helper flag: previous draw point.</summary>
        Private m_ptScreenPrevious As Point = Nothing

        ' === FEEDBACK SUPPORT ===
        Private Shared s_iCursorSize As Integer = 1

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal typeGUI As Type)
            MyBase.New(typeGUI)
        End Sub

#End Region ' Constructor

#Region " Initialization "

        Public Overrides Sub Initialize(uic As cUIContext, layer As cDisplayLayer)
            MyBase.Initialize(uic, layer)

            If (layer IsNot Nothing) Then
                Dim rl As cDisplayLayerRaster = DirectCast(layer, cDisplayLayerRaster)
                If (rl.Data IsNot Nothing) Then
                    Dim md As cVariableMetaData = rl.Data.MetadataCell
                    If (md IsNot Nothing) Then
                        Me.CellValueMin = md.Min
                        Me.CellValueMax = md.Max
                        Me.CellValue = md.NullValue
                    End If
                End If
            End If
        End Sub

#End Region ' Initialization

#Region " Properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value for the next cell that is to be edited.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property CellValue() As Object
            Get
                Return Me.m_sValue
            End Get
            Set(ByVal value As Object)
                Dim sValue As Single = Math.Max(Math.Min(CSng(value), Me.CellValueMax), Me.CellValueMin)
                If (sValue <> Me.m_sValue) Then
                    Me.m_sValue = sValue
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Pick up the cell value at a given point, and store this value in the
        ''' layer editor as the next value that will be set.
        ''' </summary>
        ''' <param name="pt">The cell location to pick up a value from.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Pickup(ByVal pt As Point)

            Try
                Me.CellValue = CDec(Me.Layer.Value(pt.Y, pt.X))
                If (Me.GUI IsNot Nothing) Then
                    Me.GUI.UpdateContent(CType(Me, cLayerEditorRaster))
                End If
            Catch ex As Exception
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the max value allowed in a cell.
        ''' </summary>
        ''' <remarks>
        ''' Ideally, this value would be obtained from core meta data. For now,
        ''' the UI is required to manually control this property.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property CellValueMax() As Single

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the min value allowed in a cell.
        ''' </summary>
        ''' <remarks>
        ''' Ideally, this value would be obtained from core meta data. For now,
        ''' the UI is required to manually control this property.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property CellValueMin() As Single

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the layer to attach to this Editor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overloads Property Layer() As cDisplayLayerRaster
            Get
                Return CType(MyBase.Layer, cDisplayLayerRaster)
            End Get
            Protected Set(ByVal value As cDisplayLayerRaster)
                MyBase.Layer = value
            End Set
        End Property

#End Region ' Properties

#Region " Cursor feedback "

        Protected Shared s_cursor As Cursor = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the size of the cursor.
        ''' </summary>
        ''' <remarks>
        ''' This value is persistent across layer editors.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Property CursorSize() As Integer
            Get
                Return s_iCursorSize
            End Get
            Set(ByVal iCursorSize As Integer)
                s_iCursorSize = iCursorSize
                If (s_cursor IsNot Nothing) Then
                    s_cursor.Dispose()
                    s_cursor = Nothing
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cursor feedback for the current location of the cursor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Function Cursor(ByVal ptMouse As Point, ByVal map As ucMap) As Cursor

            If (s_cursor Is Nothing) Then
                Dim szCell As SizeF = map.GetCellSize()
                s_cursor = cLayerEditorRaster.EditorCursor(Me.CursorSize, szCell)
            End If
            Return s_cursor

        End Function

        Public Shared Function EditorCursor(ByVal iCursorSize As Integer, ByVal szCell As SizeF) As Cursor

            Dim ptIconSize As New Size(CInt(szCell.Width * iCursorSize), CInt(szCell.Height * iCursorSize))
            Dim cursor As Cursor = Cursors.Hand

            If (iCursorSize > 0) Then
                Try
                    Dim bm As New Bitmap(ptIconSize.Width + 1, ptIconSize.Height + 1)
                    Dim g As Graphics = Graphics.FromImage(bm)

                    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    g.FillRectangle(Brushes.Transparent, New Rectangle(0, 0, bm.Width, bm.Height))
                    g.DrawEllipse(Pens.White, 1, 1, ptIconSize.Width - 2, ptIconSize.Height - 2)
                    g.DrawEllipse(Pens.Black, 0, 0, ptIconSize.Width, ptIconSize.Height)
                    Using br As New SolidBrush(Color.FromArgb(45, 0, 0, 0))
                        g.FillEllipse(br, 0, 0, ptIconSize.Width, ptIconSize.Height)
                    End Using
                    cursor = New Cursor(bm.GetHicon())
                    g.Dispose()
                    bm.Dispose()

                Catch e As Exception
                    Debug.WriteLine(e.Message)
                End Try
            End If
            Return cursor
        End Function

#End Region ' Cursor feedback

#Region " Mouse input handling "

        Public Overrides Sub ProcessMouseClick(e As MouseEventArgs, map As ucMap)

            If Not Me.IsEditable Then Return

            If ((e.Button And MouseButtons.Right) > 0) Then
                Me.Pickup(map.GetCellIndex(e.Location))
            ElseIf ((e.Button And MouseButtons.Left) > 0) Then
                Me.StartEdit(e, map)
            End If

        End Sub

        Public Overrides Sub ProcessMouseDraw(e As MouseEventArgs, map As ucMap)

            If Not Me.IsEditing Then Return

            Dim ptScreenCur As Point = New Point(e.X, e.Y)

            If (Me.m_ptScreenPrevious = Nothing) Then Me.m_ptScreenPrevious = ptScreenCur

            Dim ptCellFrom As Point = map.GetCellIndex(Me.m_ptScreenPrevious)
            Dim ptCellTo As Point = map.GetCellIndex(ptScreenCur)
            Dim ptUpdateMin As New Point(Math.Min(ptCellFrom.X, ptCellTo.X), Math.Min(ptCellFrom.Y, ptCellTo.Y))
            Dim ptUpdateMax As New Point(Math.Max(ptCellFrom.X, ptCellTo.X), Math.Max(ptCellFrom.Y, ptCellTo.Y))

            Me.Edit(ptCellFrom, ptCellTo,
                           New Point(ptScreenCur.X - Me.m_ptScreenPrevious.X, ptScreenCur.Y - Me.m_ptScreenPrevious.Y),
                           map.GetCellSize(),
                           e,
                           ptUpdateMin, ptUpdateMax)

            ' Flag layer as changed
            DirectCast(Me.m_layer, cDisplayLayerRaster).IsModified = True

            map.UpdateMap(ptUpdateMin, ptUpdateMax)

            Me.m_ptScreenPrevious = ptScreenCur

        End Sub

        Public Overrides Sub ProcessMouseUp()
            If Not Me.IsEditing Then Return
            Me.EndEdit()
        End Sub

#End Region ' Mouse input handling

#Region " Editing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' User has started editing the layer.
        ''' </summary>
        ''' <param name="args">Click <see cref="MouseEventArgs">mouse state</see>
        ''' information.</param>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub StartEdit(ByVal args As MouseEventArgs, map As ucMap)

            Me.IsEditing = True

            ' If NOT Shift key pressed, release the last mouse pos
            If Not (Control.ModifierKeys = Keys.Shift) Then Me.m_ptScreenPrevious = Nothing

            If (Me.GUI Is Nothing) Or (Not Me.IsEditable) Then Return
            ' Notify the editor GUI, if any
            Me.GUI.StartEdit(Me)
            ' Perform edit
            Me.ProcessMouseDraw(args, map)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' User is done editing the layer.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub EndEdit()

            ' Last-minute abort
            If (Not Me.IsEditable) Then Return
            ' Notify the editor GUI, if any
            If (Me.GUI IsNot Nothing) Then Me.GUI.EndEdit(Me)
            ' Update layer
            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

            Me.IsEditing = False

        End Sub

        Public ReadOnly Property CanSmooth() As Boolean
            Get
                Return Me.Layer.ValueType Is GetType(Single)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Smooth layer data across water cells.
        ''' </summary>
        ''' <remarks>
        ''' Cells with NoData values are not considered in the smoothing, and
        ''' will not receive new values.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Smooth()

            If (Not Me.IsEditable) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth
            Dim cnew(,) As Single, i As Integer, j As Integer
            Dim t As Single
            Dim n As Integer

            ReDim cnew(bm.InRow, bm.InCol)

            For i = 1 To bm.InRow
                For j = 1 To bm.InCol
                    t = 0
                    n = 0
                    For ii As Integer = i - 1 To i + 1
                        For jj As Integer = j - 1 To j + 1
                            If Not (ii = 0 Or jj = 0 Or ii = bm.InRow + 1 Or jj = bm.InCol + 1) And (layerDepth.IsWaterCell(ii, jj)) Then
                                Dim v As Single = CSng(Me.Layer.Value(ii, jj))
                                If (v <> cCore.NULL_VALUE) Then
                                    t += CSng(Me.Layer.Value(ii, jj))
                                    n += 1
                                End If
                            End If
                        Next jj
                    Next ii
                    If n > 0 Then cnew(i, j) = t / n
                Next j
            Next i

            For i = 1 To bm.InRow
                For j = 1 To bm.InCol
                    If layerDepth.IsWaterCell(i, j) Then
                        Dim v As Single = CSng(Me.Layer.Value(i, j))
                        If (v <> cCore.NULL_VALUE) Then
                            Me.Layer.Value(i, j) = cnew(i, j)
                        End If
                    End If
                Next
            Next
            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Fill the layer with the current <see cref="CellValue"/>
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Reset()

            If (Not Me.IsEditable) Then Return

            Dim v As Object = Me.CellValue

            ' ToDo: globalize this
            Dim msg As New cFeedbackMessage(cStringUtils.Localize("Are you sure you want to set all cells in this map to {0}?", v),
                                            eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question)
            msg.ReplyStyle = eMessageReplyStyle.YES_NO
            msg.Reply = eMessageReply.YES

            Me.UIContext.Core.Messages.SendMessage(msg)

            If (msg.Reply <> eMessageReply.YES) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth

            For i As Integer = 1 To bm.InRow
                For j As Integer = 1 To bm.InCol
                    If layerDepth.IsWaterCell(i, j) Then
                        Me.Layer.Value(i, j) = v
                    End If
                Next j
            Next i
            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

        End Sub

#End Region ' Editing

#Region " Cloning "

        Public Overridable ReadOnly Property CanDuplicate() As Boolean
            Get
                Return (Me.Layer.Data.SecundaryIndexCounter <> eCoreCounterTypes.NotSet)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Duplicate layer data across indexed layers.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Duplicate(ByVal iFrom As Integer)

            If (Not Me.IsEditable) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth
            Dim cc As Integer = Me.UIContext.Core.GetCoreCounter(Me.Layer.Data.SecundaryIndexCounter)
            Dim val As Object = Nothing

            For i As Integer = 1 To bm.InRow
                For j As Integer = 1 To bm.InCol
                    If (layerDepth.IsWaterCell(i, j)) Then
                        val = Me.Layer.Data.Cell(i, j, iFrom)
                        For k As Integer = 1 To cc
                            If (k <> iFrom) Then
                                Me.Layer.Data.Cell(i, j, k) = val
                            End If
                        Next k
                    End If
                Next j
            Next i

            Me.Layer.Update(cDisplayLayer.eChangeFlags.Map)

        End Sub

#End Region ' Cloning
#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the value of a cell in the current layer with the designated 
        ''' <see cref="CellValue">set value</see>.
        ''' </summary>
        ''' <param name="ptSet">The cell location (Col, Row) to set.</param>
        ''' <param name="ptClick">The cell location (Col, Row) in the cursor.</param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub SetCellValue(ByVal ptSet As Point,
                                               ByVal value As Object,
                                               ByVal e As MouseEventArgs,
                                               ByVal ptClick As Point)
            If (Not Me.IsEditable) Then Return
            Me.Layer.Value(ptSet.Y, ptSet.X) = value
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Edit the layer from one point to a next.
        ''' </summary>
        ''' <param name="ptFrom">The mouse location to edit from.</param>
        ''' <param name="ptTo">The mouse location to edit to.</param>
        ''' <param name="ptDelta">Mouse distance travelled since the last edit operation.</param>
        ''' <param name="szfCell">Size of a single cell.</param>
        ''' <param name="args">Click <see cref="MouseEventArgs">mouse state</see>
        ''' information.</param>
        ''' <param name="ptUpdateMin">Top-left cell position affected by
        ''' the edit operation.</param>
        ''' <param name="ptUpdateMax">Bottom-right cell position affected by
        ''' the edit operation.</param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub Edit(ByVal ptFrom As Point,
                                    ByVal ptTo As Point,
                                    ByVal ptDelta As Point,
                                    ByVal szfCell As SizeF,
                                    ByVal args As MouseEventArgs,
                                    ByRef ptUpdateMin As Point,
                                    ByRef ptUpdateMax As Point)

            ' Calc positions between current and last draw point
            Dim iNumSteps As Integer = Math.Max(1, Math.Max(Math.Abs(ptFrom.X - ptTo.X), Math.Abs(ptFrom.Y - ptTo.Y)))
            Dim dDX As Double = (ptTo.X - ptFrom.X) / iNumSteps
            Dim dX As Double = ptFrom.X
            Dim dDY As Double = (ptTo.Y - ptFrom.Y) / iNumSteps
            Dim dY As Double = ptFrom.Y

            Dim ptDraw As Point = Nothing
            Dim ptCell As Point = Nothing

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap

            ' Draw every step between the two draw points
            For iStep As Integer = 1 To iNumSteps

                dX += dDX
                dY += dDY

                For iX As Integer = 0 To Me.CursorSize - 1
                    For iY As Integer = 0 To Me.CursorSize - 1

                        Dim ptfCursor As New PointF(CSng(iX - (Me.CursorSize - 1) / 2),
                                                    CSng(iY - (Me.CursorSize - 1) / 2))

                        If (Math.Sqrt(ptfCursor.X * ptfCursor.X + ptfCursor.Y * ptfCursor.Y) <= (Me.CursorSize / 2)) Then

                            ptCell = New Point(CInt(Math.Floor(dX + ptfCursor.X)), CInt(Math.Floor(dY + ptfCursor.Y)))

                            ' JS 26Feb15: This is the only spot to protect for invalid row/col access.
                            '             Should this check not have been here ages ago?!
                            If (bm.IsValidCellPosition(ptCell.Y, ptCell.X)) Then
                                Me.SetCellValue(ptCell, Me.CellValue, args, New Point(iX, iY))

                                ptUpdateMin.X = Math.Min(ptCell.X, ptUpdateMin.X)
                                ptUpdateMin.Y = Math.Min(ptCell.Y, ptUpdateMin.Y)
                                ptUpdateMax.X = Math.Max(ptCell.X, ptUpdateMax.X)
                                ptUpdateMax.Y = Math.Max(ptCell.Y, ptUpdateMax.Y)
                            End If
                        End If
                    Next iY
                Next iX

            Next iStep

        End Sub

#End Region ' Internals

    End Class

End Namespace
