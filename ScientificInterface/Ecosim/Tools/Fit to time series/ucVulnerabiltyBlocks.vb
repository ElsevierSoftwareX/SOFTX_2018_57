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
Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecosim

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control that implements a grid of coloured cells. Every cell can be 
    ''' assigned a block value in a graphical interface. The number of available 
    ''' blocks and the graphical representation of each block is defined via 
    ''' <see cref="ucVulnerabiltyBlocks.BlockColors">BlockColors</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucVulnerabiltyBlocks
        Implements IUIElement

        Private m_uic As cUIContext = Nothing

        ''' <summary>Two-dim arr of integer representing vulnerability blocks layout.</summary>
        Private m_a2iVulBlocks(,) As Integer
        ''' <summary>Block colours to show.</summary>
        Private m_acolors As Color()
        ''' <summary>Index of the selected block with the list of block colours.</summary>
        Private m_iSelectedBlockCodeIndex As Integer = 0
        ''' <summary>Helper var; remembers the last processed mouse position while drawing.</summary>
        ''' <remarks>When drawing, all grid cells on a line between the previous mouse position
        ''' and the current mouse position are considered.</remarks>
        Private m_ptPosPrevious As Point = Nothing

#Region " Constructor "

        Public Sub New()

            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
                Me.UIContext = Nothing
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Constructor

#Region " Public Interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the control to a given instance of the EwE core.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
                If (Me.m_uic IsNot Nothing) Then
                    Me.RefreshContent()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the control to the core content.
        ''' </summary>
        ''' <remarks>
        ''' This method should be called whenever this number of groups in the 
        ''' core has changed. The control will not keep track of the number of
        ''' groups itself.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Sub RefreshContent()
            ReDim Me.m_a2iVulBlocks(Me.m_uic.Core.nGroups, Me.m_uic.Core.nGroups)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the block colours that this control should reflect. The
        ''' number of available block colours is set to the number of colours
        ''' passed to this property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property BlockColors() As Color()
            Get
                Return Me.m_acolors
            End Get
            Set(ByVal value As Color())
                Me.m_acolors = value
                Me.Invalidate()
            End Set
        End Property

        Public Event OnSelectedBlockChanged(ByVal sender As Object, ByVal block As Integer)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the index of block in the list of 
        ''' <see cref="BlockColors">block colours</see> that the user will be 
        ''' drawing with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property SelectedBlockNum() As Integer
            Get
                Return Me.m_iSelectedBlockCodeIndex
            End Get
            Set(ByVal value As Integer)
                Me.m_iSelectedBlockCodeIndex = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the 2-dimensional array of vulnerability block values that 
        ''' this control maintains. This one-based array is dimensioned by 
        ''' the number of groups in the core.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Vulblocks() As Integer(,)
            Get
                Return Me.m_a2iVulBlocks
            End Get
        End Property

#End Region ' Public interfaces

#Region " Events "

        Protected Overrides Sub OnMouseHover(ByVal e As System.EventArgs)
            MyBase.OnMouseHover(e)
            Me.UpdateToolTip(Me.PointToClient(Cursor.Position))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Start drawing
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseDown(e)

            If (Me.m_uic Is Nothing) Then Return

            If (e.Button And System.Windows.Forms.MouseButtons.Right) > 0 Then
                Dim ptClick As Point = Me.PointToPredPrey(e.Location)
                Dim manager As cMediatedInteractionManager = Me.m_uic.Core.MediatedInteractionManager

                If ptClick.X > 0 And ptClick.Y > 0 Then
                    If manager.isPredPrey(ptClick.X, ptClick.Y) Then
                        Me.SelectedBlockNum = Me.m_a2iVulBlocks(ptClick.X, ptClick.Y)
                        Try
                            RaiseEvent OnSelectedBlockChanged(Me, Me.m_iSelectedBlockCodeIndex)
                        Catch ex As Exception
                            ' NOP
                        End Try
                    End If
                End If
                Return
            End If

            Me.Capture = True

            ' Release the last mouse pos
            Me.m_ptPosPrevious = Nothing
            ' Process mouse input
            Me.ProcessMouseInput(e)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Process a draw step or hover information.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseMove(e)

            If (Me.m_uic Is Nothing) Then Return

            ' Process mouse hover info
            Me.UpdateToolTip(e.Location)

            If (Me.Capture = False) Then Return

            ' Process mouse input
            Me.ProcessMouseInput(e)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Stop drawing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseUp(e)

            If (Me.m_uic Is Nothing) Then Return

            If Not Me.Capture Then Return
            Me.Capture = False
            Me.Invalidate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Paint the control
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)

            ' Possible performance boost:
            ' - Render onto bitmap, render ichanged cells only (like basemap)

            Try

                If (Me.m_uic Is Nothing) Then Return

                Dim szCell As SizeF = Me.CellSize()
                Dim manager As cMediatedInteractionManager = Me.m_uic.Core.MediatedInteractionManager
                Dim iBlock As Integer = 0

                ' Clear the picture box
                Using tmpBrush As New SolidBrush(Color.White)
                    e.Graphics.FillRectangle(tmpBrush, Me.ClientRectangle)
                End Using

                ' Draw vulnerability blocks
                For i As Integer = 0 To Me.m_uic.Core.nLivingGroups
                    For j As Integer = 0 To Me.m_uic.Core.nGroups
                        If (i = 0 Or j = 0) Then
                            ' Draw row and/or column header cell
                            e.Graphics.FillRectangle(SystemBrushes.Control, i * szCell.Width, j * szCell.Height, szCell.Width, szCell.Height)
                        Else
                            ' Draw content cell
                            If (manager.isPredPrey(i, j)) Then
                                iBlock = Me.m_a2iVulBlocks(i, j)
                                If iBlock < Me.m_acolors.Count - 1 Then
                                    ' Render solid block
                                    Using tmpBrush As New SolidBrush(Me.m_acolors(iBlock))
                                        e.Graphics.FillRectangle(tmpBrush, i * szCell.Width, j * szCell.Height, szCell.Width, szCell.Height)
                                    End Using
                                Else
                                    ' JS 22mar08: added crash protection. The m_a2iVulBlocks array contains block codes that exceed the number
                                    '             of blocks that this interface is supposed to use
                                    Using tmpBrush As New HatchBrush(HatchStyle.DiagonalCross, Color.White)
                                        e.Graphics.FillRectangle(tmpBrush, i * szCell.Width, j * szCell.Height, szCell.Width, szCell.Height)
                                    End Using
                                End If
                            End If
                        End If
                    Next j
                Next i

                ' Draw grid lines
                For i As Integer = 1 To Me.m_uic.Core.nGroups
                    e.Graphics.DrawLine(Pens.LightGray, 0, i * szCell.Height, Me.ClientRectangle.Width, i * szCell.Height) '(0, i)-(NumLiving + 1, i)
                Next
                For i As Integer = 1 To Me.m_uic.Core.nLivingGroups
                    e.Graphics.DrawLine(Pens.LightGray, i * szCell.Width, 0, i * szCell.Width, Me.ClientRectangle.Height) '(i, 0)-(i, NumGroups + 1)
                Next

                ' Draw row and column labels
                For i As Integer = 1 To Me.m_uic.Core.nGroups
                    e.Graphics.DrawString(CStr(i), Me.Font, SystemBrushes.ControlText, 0, i * szCell.Height)
                Next
                For i As Integer = 1 To Me.m_uic.Core.nLivingGroups
                    e.Graphics.DrawString(CStr(i), Me.Font, SystemBrushes.ControlText, i * szCell.Width, 0)
                Next

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.StackTrace)
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Invalidate control when resized.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnSizeChanged(ByVal e As System.EventArgs)
            MyBase.OnSizeChanged(e)
            Me.Invalidate()
        End Sub

#End Region ' Events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Process mouse input to affect colour blocks in this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ProcessMouseInput(ByVal e As System.Windows.Forms.MouseEventArgs)

            If Not Me.Enabled Then Return
            If Not Me.Capture Then Return

            Dim bLeftBtnDown As Boolean = (e.Button = MouseButtons.Left)
            Dim ptPredPreyFrom As Point = Nothing
            Dim ptPredPreyTo As Point = Nothing
            Dim ptPosCurrent As Point = New Point(e.X, e.Y)
            Dim pfPredPrey As PointF = Nothing
            Dim pfIncrement As New PointF(0, 0)
            Dim iNumSteps As Integer = 0

            If (Me.m_ptPosPrevious = Nothing) Then Me.m_ptPosPrevious = ptPosCurrent

            If bLeftBtnDown Then

                ' Translate m_ptPosPrevious into pred/prey prev
                ptPredPreyFrom = Me.PointToPredPrey(Me.m_ptPosPrevious)
                ' Translate ptPosCurrent into pred/prey curr
                ptPredPreyTo = Me.PointToPredPrey(ptPosCurrent)

                ' Calc number of steps to draw
                ' - Determine number of steps to draw (horz or vert)
                Dim iDX As Integer = (ptPredPreyTo.X - ptPredPreyFrom.X)
                Dim iDY As Integer = (ptPredPreyTo.Y - ptPredPreyFrom.Y)
                iNumSteps = Math.Abs(Math.Max(iDX, iDY))
                ' - Determine stepwise pred/prey increment
                If (iNumSteps > 0) Then
                    pfIncrement = New PointF(CSng(iDX / iNumSteps), CSng(iDY / iNumSteps))
                End If

                ' - Start
                pfPredPrey = New PointF(ptPredPreyFrom.X, ptPredPreyFrom.Y)
                ' For each step:
                For iStep As Integer = 0 To iNumSteps
                    ' Set pred/prey block
                    Me.FillBlocks(CInt(Math.Floor(pfPredPrey.X + 0.5)), CInt(Math.Floor(pfPredPrey.Y + 0.5)), Me.m_iSelectedBlockCodeIndex)
                    ' Next pred/prey
                    pfPredPrey.X += pfIncrement.X
                    pfPredPrey.Y += pfIncrement.Y
                Next
                ' End
            End If

            m_ptPosPrevious = ptPosCurrent

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, fills a range of block code cells with a given block
        ''' code value.
        ''' </summary>
        ''' <param name="iPred">The index of the predator block to fill.</param>
        ''' <param name="iPrey">The index of the prey block to fill.</param>
        ''' <param name="iBlockCode">The block code value to fill with.</param>
        ''' <remarks>
        ''' <para>Fill will behave as follows:</para>
        ''' <list type="bullet">
        ''' <item><description>When a non-zero predator and prey cell are given, a single cell is filled.</description></item>
        ''' <item><description>When a non-zero predator and zero prey cell are given, a predator column is filled.</description></item>
        ''' <item><description>When a zero predator and non-zero prey cell are given, a prey row is filled.</description></item>
        ''' <item><description>When both predator and prey are zero, the entire grid is filled with the given block code value.</description></item>
        ''' </list>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Sub FillBlocks(ByVal iPred As Integer, ByVal iPrey As Integer, ByVal iBlockCode As Integer)

            Dim manager As cMediatedInteractionManager = Me.m_uic.Core.MediatedInteractionManager

            ' Sanity check
            If iPrey < 0 Then Return
            If iPred < 0 Then Return
            If iPrey > Me.m_uic.Core.nGroups Then Return
            If iPred > Me.m_uic.Core.nLivingGroups Then Return

            ' Row or col header clicked?
            If iPrey = 0 Or iPred = 0 Then
                ' #Yes: Col header clicked?
                If iPred = 0 Then
                    ' #Yes: Also row header clicked?
                    If iPrey = 0 Then
                        ' #Yes: fill entire grid
                        For iPred = 1 To Me.m_uic.Core.nLivingGroups
                            For iPrey = 1 To Me.m_uic.Core.nGroups
                                If manager.isPredPrey(iPred, iPrey) Then Me.m_a2iVulBlocks(iPred, iPrey) = iBlockCode
                            Next iPrey
                        Next iPred
                    Else
                        ' #No: Fill entire prey column
                        For iPred = 1 To Me.m_uic.Core.nLivingGroups
                            If manager.isPredPrey(iPred, iPrey) Then Me.m_a2iVulBlocks(iPred, iPrey) = iBlockCode
                        Next iPred
                    End If
                Else
                    ' #No: Fill entire predator row
                    For iPrey = 1 To Me.m_uic.Core.nGroups
                        If manager.isPredPrey(iPred, iPrey) Then Me.m_a2iVulBlocks(iPred, iPrey) = iBlockCode
                    Next iPrey
                End If
            Else
                ' #No: fill single cell
                If manager.isPredPrey(iPred, iPrey) Then Me.m_a2iVulBlocks(iPred, iPrey) = iBlockCode
            End If

            ' Redraw at your leasure
            Me.Invalidate()

        End Sub

        Dim m_ptLast As New Point(-1, -1)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Process mouse hover info to show and populate a tooltip or to hide
        ''' when applicable.
        ''' </summary>
        ''' <param name="ptHover">The hover point to update the tooltip for.</param>
        ''' <remarks>
        ''' The tooltip is hidden if the hover point is absent, or the given 
        ''' hover location indicates an invalid predator and prey index.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Sub UpdateToolTip(ByVal ptHover As Point)

            Dim ptPredPrey As New Point(0, 0)
            Dim strToolTip As String = ""
            Dim strPred As String = ""
            Dim strPrey As String = ""

            If (Me.m_uic Is Nothing) Then Return

            ' Get hover info, if any
            If (ptHover <> Nothing) Then
                ptPredPrey = Me.PointToPredPrey(ptHover)
            End If

            ' Sanity checks
            If ptPredPrey.X > Me.m_uic.Core.nGroups Then Return
            If ptPredPrey.X < 0 Then Return
            If ptPredPrey.Y > Me.m_uic.Core.nGroups Then Return
            If ptPredPrey.Y < 0 Then Return

            If Me.m_ptLast.X = ptPredPrey.X And Me.m_ptLast.Y = ptPredPrey.Y Then Return
            Me.m_ptLast.X = ptPredPrey.X
            Me.m_ptLast.Y = ptPredPrey.Y

            If ptPredPrey.X > 0 Then strPred = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, ptPredPrey.X, Me.m_uic.Core.EcoPathGroupInputs(ptPredPrey.X).Name)
            If ptPredPrey.Y > 0 Then strPrey = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, ptPredPrey.Y, Me.m_uic.Core.EcoPathGroupInputs(ptPredPrey.Y).Name)

            ' Format tooltip
            If (ptPredPrey.X <> 0) Or (ptPredPrey.Y <> 0) Then
                If (ptPredPrey.X = 0) Then
                    strToolTip = cStringUtils.Localize(My.Resources.GENERIC_TOOLTIP_PREY, strPrey)
                ElseIf (ptPredPrey.Y = 0) Then
                    strToolTip = cStringUtils.Localize(My.Resources.GENERIC_TOOLTIP_PREDATOR, strPred)
                Else
                    strToolTip = cStringUtils.Localize(My.Resources.GENERIC_TOOLTIP_PREDPREY, strPred, strPrey)
                End If
            End If

            ' Show or hide tooltip
            cToolTipShared.GetInstance().SetToolTip(Me, strToolTip)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; calculates pred/prey index from a given point in the control.
        ''' </summary>
        ''' <param name="pt">The point to request predator and prey index for.</param>
        ''' <returns>A point holding the predator index (X) and prey index (Y) for the
        ''' requested point.</returns>
        ''' -------------------------------------------------------------------
        Private Function PointToPredPrey(ByVal pt As Point) As Point
            Dim szCell As SizeF = Me.CellSize()
            Return New Point(CInt(Math.Max(0, Math.Floor(pt.X / szCell.Width))), _
                             CInt(Math.Max(0, Math.Floor(pt.Y / szCell.Height))))
        End Function

        Private Function CellSize() As SizeF
            If (Me.m_uic Is Nothing) Then Return New SizeF(1, 1)
            Return New SizeF(CSng(Me.ClientRectangle.Width / (Me.m_uic.Core.nLivingGroups + 1)), CSng(Me.ClientRectangle.Height / (Me.m_uic.Core.nGroups + 1)))
        End Function

#End Region ' Internals

    End Class

End Namespace
