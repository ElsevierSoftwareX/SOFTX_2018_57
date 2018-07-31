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
Option Explicit On

Imports EwECore
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports SourceGrid2.Cells.Real

#End Region

''' <summary>
''' EwEGrid that handles selection of color blocks for a ucCVBlockSelector
''' </summary>
''' <remarks>Color and values for the cells come from the ucCVBlockSelector(parent control)</remarks>
<CLSCompliant(False)> _
Public Class gridSelectColorBlock
    Inherits EwEGrid

#Region " Helper class "

    ''' =======================================================================
    ''' <summary>
    ''' Helper class for drawing a colored CV cell.
    ''' </summary>
    ''' <remarks>
    ''' This class is hard-wired to gridSelectColorBlock.
    ''' </remarks>
    ''' =======================================================================
    Private Class cCVCellVisualizer
        Inherits VisualModels.Common

        Private m_parent As gridSelectColorBlock = Nothing

        Public Sub New(ByVal parent As gridSelectColorBlock)
            Debug.Assert(parent IsNot Nothing)
            Me.m_parent = parent
        End Sub

        Protected Overrides Sub DrawCell_Background(ByVal p_Cell As SourceGrid2.Cells.ICellVirtual, _
                                                    ByVal p_CellPosition As SourceGrid2.Position, _
                                                    ByVal e As PaintEventArgs, _
                                                    ByVal p_ClientRectangle As System.Drawing.Rectangle, _
                                                    ByVal p_Status As SourceGrid2.DrawCellStatus)

            Me.BackColor = Me.m_parent.BlockColor(p_CellPosition.Column)
            MyBase.DrawCell_Background(p_Cell, p_CellPosition, e, p_ClientRectangle, DrawCellStatus.Normal)

        End Sub

        Protected Overrides Sub DrawCell_Border(ByVal p_Cell As SourceGrid2.Cells.ICellVirtual, _
                                                ByVal p_CellPosition As SourceGrid2.Position, _
                                                ByVal e As PaintEventArgs, _
                                                ByVal p_ClientRectangle As Rectangle, _
                                                ByVal p_Status As SourceGrid2.DrawCellStatus)

            Dim border As Border = Nothing

            If (p_CellPosition.Column = Me.m_parent.SelectedBlock) Then
                border = New Border(Me.m_parent.HighlightColor, 3)
                Me.Border = New RectangleBorder(border)
            Else
                Me.Border = Nothing
            End If

            MyBase.DrawCell_Border(p_Cell, p_CellPosition, e, p_ClientRectangle, DrawCellStatus.Normal)

        End Sub

    End Class

#End Region ' Helper class

#Region " Private vars "

    ''' <summary>Block selector connected to this grid</summary>
    Private m_parentSelector As ucCVBlockSelector = Nothing
    ''' <summary>Cell renderer.</summary>
    Private m_vm As cCVCellVisualizer = Nothing
    ''' <summary>Number of blocks.</summary>
    Private m_iNumBlocks As Integer = 0
    ''' <summary>ONE-based index of a selected block.</summary>
    Private m_iBlock As Integer = 1
    ''' <summary>Selected block CV value.</summary>
    Private m_sBlockValue As Single = cCore.NULL_VALUE

    ''' <summary>=Event, informing the world that the selected block has changed.</summary>
    ''' <param name="sNewValue">The value of the selected block.</param>
    ''' <param name="iBlock">The index of the newly selected block.</param>
    Public Event OnValueChanged(ByVal sNewValue As Single, ByVal iBlock As Integer)

#End Region ' Private vars

#Region " Constructor "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Public "

    Public Sub Attach(ByVal parentSelector As ucCVBlockSelector)
        Me.m_parentSelector = parentSelector
        Me.RefreshContent()
    End Sub

    Public Sub Detach()
        Me.m_parentSelector = Nothing
    End Sub

    Public Property SelectedBlock() As Integer
        Get
            Return Me.m_iBlock
        End Get
        Set(ByVal value As Integer)
            If (value <> Me.m_iBlock) Then
                Me.m_iBlock = value
                Me.InvalidateCells()
            End If
        End Set
    End Property

#End Region ' Public

#Region " Events "

    Private Sub gridSelectColorBlock_CellGotFocus(ByVal sender As Object, ByVal e As SourceGrid2.PositionCancelEventArgs) _
        Handles Me.CellGotFocus

        'Don't set the value if this is not a valid value column
        If e.Position.Column < 1 Then Exit Sub

        Try
            ' Parse using UI default number formatting
            Me.m_sBlockValue = Single.Parse(CStr(Me(0, e.Position.Column).Value))
        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & " CellGotFocus() Exception: " & ex.Message)
        End Try
        ' Set selected block
        Me.SelectedBlock = e.Position.Column

    End Sub

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        'Don't set the value if this is not a valid value column
        If p.Column < 1 Then Exit Function

        Try
            ' Parse using UI default number formatting
            Dim newvalue As Single = Single.Parse(CStr(Me(0, p.Column).Value))
            Dim dif As Single = CSng(Math.Round(newvalue - Me.m_sBlockValue, 2))

            'has the cell been edited
            If dif <> 0.0 Then
                Dim col As Integer = p.Column
                Me.m_parentSelector.BlockValues(col) = newvalue
                RaiseEvent OnValueChanged(newvalue, col)
            End If

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & " CellLostFocus() Exception: " & ex.Message)
        End Try
        Return MyBase.OnCellEdited(p, Cell)

    End Function

#End Region ' Events

#Region " Internals "

    Protected Overrides Sub InitStyle()

        If Me.m_parentSelector Is Nothing Then Return

        MyBase.InitStyle()

        Me.Redim(2, Me.m_parentSelector.NumBlocks + 1)
        Me(0, 0) = New EwERowHeaderCell(SharedResources.HEADER_CV)
        Me(1, 0) = New EwERowHeaderCell(SharedResources.HEADER_COLOR)

        'hide the first row
        ' JB: sourcegrid will explode if you try to edit the first row so hide it and put the cv values in the second row
        ' JS: this is because the first row is set as fixed. Turn this off and you're ok
        'Me.Rows(0).Height = 0
        Me.FixedRows = 0

        Me.FixedColumns = 1
        Me.HScroll = True

    End Sub

    Protected Overrides Sub FillData()

        Dim cell As Cell = Nothing

        If (Me.m_parentSelector Is Nothing) Then Return
        If (Me.StyleGuide Is Nothing) Then Return

        If (Me.m_vm Is Nothing) Then
            Me.m_vm = New cCVCellVisualizer(Me)
        End If

        'Color and values come from parent control
        Dim cvs() As Single = Me.m_parentSelector.BlockValues

        ' Define rows of CV cells, where each row index corresponds to a one-based CV colour and value
        '   in the underlying arrays Me.m_parent.BlockColor (colours) and Me.m_parentSelector.BlockValues (values)
        For i As Integer = 1 To Me.m_parentSelector.NumBlocks

            ' Top row (row 0) holds CV values
            cell = New EwECell(cvs(i), cvs(i).GetType)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(0, i) = cell

            ' Bottom row (row 1) holds CV colours
            cell = New Cell("", GetType(String))
            cell.VisualModel = Me.m_vm
            cell.EditableMode = EditableMode.None
            cell.EnableEdit = False
            Me(1, i) = cell

        Next

    End Sub

    Protected Overrides Sub InitLayout()
        If Me.UIContext Is Nothing Then Return
        Me.InitStyle()
        Me.FillData()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the block colour for a given block.
    ''' </summary>
    ''' <param name="i">The one-based index of the block.</param>
    ''' <returns>The colour of the block.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function BlockColor(ByVal i As Integer) As Color
        If (i <= Me.m_parentSelector.NumBlocks) Then
            Return Me.m_parentSelector.BlockColor(i)
        End If
        Return Color.White
    End Function

    Protected Function HighlightColor() As Color
        If (Me.StyleGuide IsNot Nothing) Then
            Return Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
        End If
        Return Color.Orange
    End Function

#End Region ' Internals

End Class
