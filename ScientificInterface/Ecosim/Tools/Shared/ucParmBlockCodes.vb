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

Imports ScientificInterface.Other
Imports EwECore

#End Region ' Imports

Namespace Ecosim


    ''' =======================================================================
    ''' <summary>
    ''' User control allowing the user to set a number of colour blocks
    ''' and a block selection.
    ''' </summary>
    ''' =======================================================================
    Public Class ucParmBlockCodes
        Implements IBlockSelector


#Region " Private variables "

        ''' <summary>ToDo: obtain from style guide.</summary>
        Private Const g_iSelectionBorderWidth As Integer = 3

        ''' <summary>UI context.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>Number of blocks to show.</summary>
        Private m_iNumBlocks As Integer = 30
        ''' <summary>Selected color index.</summary>
        Private m_iSelectedBlock As Integer = 15

        ''' <summary>Update feedback loop prevention.</summary>
        Private m_bInUpdate As Boolean = False

#End Region

#Region " Constructor "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " IUIElement implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cUIContext">UI context</see> to use.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
            End Set
        End Property

#End Region ' IUIElement implementation

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the number of blocks.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property NumBlocks() As Integer Implements IBlockSelector.NumBlocks
            Get
                Return Me.m_iNumBlocks
            End Get

            Set(ByVal value As Integer)

                ' Truncate value
                value = Math.Max(0, Math.Min(CInt(Me.m_nudNumBlockCodes.Maximum), value))

                ' Optimization
                If (value = Me.m_iNumBlocks) Then Return

                ' Update number of blocks
                Me.m_iNumBlocks = value
                Me.SelectedBlock = Me.SelectedBlock ' Auto-truncate
                Me.UpdateControls()

                RaiseEvent OnNumBlocksChanged(Me)

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the index of the current selected block in the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property SelectedBlock() As Integer Implements IBlockSelector.SelectedBlock
            Get
                Return Me.m_iSelectedBlock
            End Get
            Set(ByVal value As Integer)
                Me.m_iSelectedBlock = Math.Max(0, Math.Min(Me.m_iNumBlocks, value))

                If (Me.m_uic Is Nothing) Then Return
                Me.UpdateControls()

                RaiseEvent OnBlockSelected(Me)

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the colors representing the blocks in this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property BlockColors() As Color() Implements IBlockSelector.BlockColors
            Get
                Dim lcolors As List(Of Color) = Me.m_uic.StyleGuide.GetEwE5ColorRamp(Me.m_iNumBlocks)
                lcolors.Insert(0, Color.Black)
                Return lcolors.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get colors for a single block in this control.
        ''' </summary>
        ''' <param name="iBlock">The index of the block to access the color for.</param>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property BlockColor(ByVal iBlock As Integer) As Color Implements IBlockSelector.BlockColor
            Get
                If iBlock >= 0 And iBlock <= Me.NumBlocks Then
                    Return Me.BlockColors(iBlock)
                End If
                Return Color.Black
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the color for the current selected block in the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SelectedBlockColor() As Color Implements IBlockSelector.SelectedBlockColor
            Get
                Return Me.BlockColor(Me.m_iSelectedBlock)
            End Get
        End Property

#End Region ' Public interfaces

#Region " Public events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event notifying that the number of blocks have changed.
        ''' </summary>
        ''' <param name="sender">
        ''' The <see cref="ucParmBlockCodes">block code parameters control</see>
        ''' that sent this event.
        ''' </param>
        ''' -------------------------------------------------------------------
        Public Event OnNumBlocksChanged(ByVal sender As IBlockSelector) Implements IBlockSelector.OnNumBlocksChanged

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event notifying that selected block has changed.
        ''' </summary>
        ''' <param name="sender">
        ''' The <see cref="ucParmBlockCodes">block code parameters control</see>
        ''' that sent this event.
        ''' </param>
        ''' -------------------------------------------------------------------
        Public Event OnBlockSelected(ByVal sender As IBlockSelector) Implements IBlockSelector.OnBlockSelected

        ''' <summary>
        ''' CV value has changed
        ''' </summary>
        ''' <param name="newValue"></param>
        ''' <param name="Index"></param>
        ''' <remarks>Not used for this implementation</remarks>
        Public Event OnValueChanged(ByVal newValue As Single, ByVal Index As Integer) Implements IBlockSelector.OnValueChanged


#End Region ' Public events

#Region " Public properties "

#End Region ' Public properties

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.UpdateControls()
        End Sub

        Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
            MyBase.OnResize(e)
            Me.m_slSelectedBlockCode.Left = Me.m_pbxBlockCodes.Left + CInt(Me.BlockWidth / 2)
            Me.m_slSelectedBlockCode.Width = CInt(Me.m_pbxBlockCodes.Width - Me.BlockWidth)
            Me.Invalidate()
        End Sub

#End Region ' Overrides

#Region " Control events "

        Private Sub OnNumBocksChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudNumBlockCodes.ValueChanged

            If (Me.m_uic Is Nothing) Then Return
            If (Me.m_bInUpdate = True) Then Return

            Me.NumBlocks = Convert.ToInt32(Me.m_nudNumBlockCodes.Value)

        End Sub

        Private Sub OnSelectedBlockCodeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudSelectedBlockCode.ValueChanged

            If (Me.m_uic Is Nothing) Then Return
            If (Me.m_bInUpdate = True) Then Return

            Me.SelectedBlock = Convert.ToInt32(Me.m_nudSelectedBlockCode.Value)

        End Sub

        Private Sub OnBlockSelectionChanged(ByVal sender As Object, ByVal e As MouseEventArgs) _
            Handles m_pbxBlockCodes.MouseDown

            If (Me.m_uic Is Nothing) Then Return
            If (Me.m_bInUpdate = True) Then Return

            Me.SelectedBlock = CInt(Math.Floor(CSng(e.X) / Me.BlockWidth()))

        End Sub

        Private Sub OnBlockSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_slSelectedBlockCode.ValueChanged, m_pbxBlockCodes.MouseDown

            If (Me.m_uic Is Nothing) Then Return
            If (Me.m_bInUpdate = True) Then Return

            Me.SelectedBlock = Me.m_slSelectedBlockCode.Value

        End Sub

        Private Sub OnPaintBlocks(ByVal sender As Object, ByVal e As PaintEventArgs) _
            Handles m_pbxBlockCodes.Paint

            If (Me.m_uic Is Nothing) Then Return
            Me.PaintBlocks(e.Graphics)

        End Sub

#End Region ' Control events

#Region " Internal implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update state and value of controls.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateControls()

            If Me.m_bInUpdate Then Return

            Me.m_bInUpdate = True

            Me.m_nudNumBlockCodes.Value = Me.m_iNumBlocks

            Try

                Me.m_nudSelectedBlockCode.Value = 0
                Me.m_slSelectedBlockCode.Value = 0

                Me.m_nudSelectedBlockCode.Maximum = Me.m_iNumBlocks
                Me.m_slSelectedBlockCode.Maximum = Me.m_iNumBlocks

                Me.m_nudSelectedBlockCode.Value = Me.m_iSelectedBlock
                Me.m_slSelectedBlockCode.Value = Me.m_iSelectedBlock

            Catch ex As Exception

            End Try

            Me.m_pbxBlockCodes.Invalidate()

            Me.m_bInUpdate = False

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Render the blocks picture box on a given graphics instance.
        ''' </summary>
        ''' <param name="g">The graphics to draw onto.</param>
        ''' -------------------------------------------------------------------
        Private Sub PaintBlocks(ByVal g As Graphics)

            If (Me.UIContext Is Nothing) Then Return

            Dim sBlockWidth As Single = Me.BlockWidth()
            Dim sX As Single = Me.m_iSelectedBlock * sBlockWidth
            Dim sY As Single = Me.m_pbxBlockCodes.ClientRectangle.Height
            Dim clrHighlight As Color = Me.UIContext.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)

            For iBlock As Integer = 0 To Me.m_iNumBlocks
                Using brTmp As New SolidBrush(Me.BlockColor(iBlock))
                    g.FillRectangle(brTmp, sBlockWidth * iBlock, 0, sBlockWidth, sY)
                End Using
            Next

            Using penTmp As New System.Drawing.Pen(clrHighlight, 3)
                g.DrawRectangle(penTmp, sX, 0, sBlockWidth, sY - g_iSelectionBorderWidth + 1)
            End Using

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the width of a single block to draw.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private ReadOnly Property BlockWidth() As Single
            Get
                Return CSng(m_pbxBlockCodes.ClientRectangle.Width / (Me.m_iNumBlocks + 1))
            End Get
        End Property

#End Region ' Internal implementation

        Public Function BlocktoValue(ByVal iBlock As Integer) As Single Implements IBlockSelector.BlocktoValue
            Return cCore.NULL_VALUE
        End Function

        Public Function ValuetoBlock(ByVal cv As Single) As Integer Implements IBlockSelector.ValuetoBlock
            Return cCore.NULL_VALUE
        End Function

    End Class

End Namespace
