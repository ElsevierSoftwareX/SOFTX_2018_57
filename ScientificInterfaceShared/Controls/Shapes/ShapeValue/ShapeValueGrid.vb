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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Grid disaplying individual values of a <see cref="cShapeData">shape</see>
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public Class ShapeValueGrid
    Inherits EwEGrid

#Region " Private vars "

    Private m_iNumValues As Integer = 50
    Private m_bSuppressZeroes As Boolean = False
    Private m_shape As cShapeData = Nothing
    Private m_displayMode As frmShapeValue.eDisplayMode = frmShapeValue.eDisplayMode.Monthly

#End Region ' Private vars

#Region " Public interfaces "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Erase all values in the grid and prepare the grid for new use.
    ''' </summary>
    ''' <param name="iNumValues">The number of rows to display in the grid.</param>
    ''' <param name="bSuppressZeroes">States whether the grid will hide (True)
    ''' or show (False) zeroes.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Clear(ByVal iNumValues As Integer, _
                     ByVal bSuppressZeroes As Boolean)

        Me.m_iNumValues = iNumValues
        Me.m_bSuppressZeroes = bSuppressZeroes
        Me.m_shape = Nothing

        Me.InitLayout()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Populate the grid from a new shape.
    ''' </summary>
    ''' <param name="shape">The shape to display.</param>
    ''' <param name="iNumValues">The number of values to display.</param>
    ''' <param name="displayMode">Mode that indicates how to format label
    ''' values.</param>
    ''' -----------------------------------------------------------------------
    Public Sub SetValues(ByVal shape As cShapeData, _
                         ByVal iNumValues As Integer, _
                         ByVal displayMode As frmShapeValue.eDisplayMode)

        Me.m_iNumValues = iNumValues
        Me.m_bSuppressZeroes = (TypeOf shape Is cTimeSeries)
        Me.m_shape = shape
        Me.m_displayMode = displayMode

        Me.InitLayout()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Apply the values to a shape.
    ''' </summary>
    ''' <param name="shape">Optional alternate shape to apply values to. If not
    ''' specified, values will be applied to the shape currently connected to
    ''' the grid.</param>
    ''' -----------------------------------------------------------------------
    Public Sub ApplyValues(Optional ByVal shape As cShapeData = Nothing)

        Dim iNumValues As Integer = Me.m_iNumValues
        If (shape Is Nothing) Then shape = Me.m_shape
        If (shape.IsSeasonal) Then iNumValues = Me.m_shape.nPoints
        shape.ShapeData = Me.Values(iNumValues)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the values in the grid.
    ''' </summary>
    ''' <param name="iNumValues">Number of values to retrieve, or -1 to obtain
    ''' all possible values.</param>
    ''' <remarks>
    ''' Values returned are in the same format as <see cref="cShapeData.ShapeData">the shape data</see>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Values(Optional ByVal iNumValues As Integer = -1) As Single()
        Get

            Dim lValues As New List(Of Single)
            Dim iValueCol As Integer = Me.ValueColumn
            Dim cell As EwECell = Nothing

            If (iNumValues <= 0) Then iNumValues = Me.m_iNumValues
            iNumValues = Math.Min(iNumValues, Me.RowsCount - 1)

            ' Add (or preserve) zero data point
            If Me.m_shape Is Nothing Then
                lValues.Add(0.0!)
            Else
                lValues.Add(Me.m_shape.ShapeData(0))
            End If

            For iCell As Integer = 1 To iNumValues
                cell = DirectCast(Me(iCell, iValueCol), EwECell)
                lValues.Add(CSng(cell.Value))
            Next
            Return lValues.ToArray

        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the numeric equivalent for the very first value.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ValueStartRef() As Integer
        Get
            Try
                ' Parse value using UI number settings
                Return Integer.Parse(CStr(Me(0, 1).Value))
            Catch ex As Exception

            End Try
            Return cCore.NULL_VALUE
        End Get
    End Property

#End Region ' Public interfaces

#Region " Grid overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Select Case Me.m_displayMode

            Case frmShapeValue.eDisplayMode.Index
                Me.Redim(Me.m_iNumValues + 1, 2)
                Me(0, 0) = New EwEColumnHeaderCell(My.Resources.HEADER_INDEX)
                Me(0, 1) = New EwEColumnHeaderCell(My.Resources.HEADER_VALUE)

            Case frmShapeValue.eDisplayMode.Yearly
                Me.Redim(Me.m_iNumValues + 1, 2)
                Me(0, 0) = New EwEColumnHeaderCell(My.Resources.HEADER_YEAR)
                Me(0, 1) = New EwEColumnHeaderCell(My.Resources.HEADER_VALUE)

            Case frmShapeValue.eDisplayMode.Monthly
                Me.Redim(Me.m_iNumValues + 1, 3)
                Me(0, 0) = New EwEColumnHeaderCell(My.Resources.HEADER_YEAR)
                Me(0, 1) = New EwEColumnHeaderCell(My.Resources.HEADER_MONTH)
                Me(0, 2) = New EwEColumnHeaderCell(My.Resources.HEADER_VALUE)

        End Select

        Me.FixedColumnWidths = False
        Me.AllowBlockSelect = False

    End Sub

    Protected Overrides Sub FillData()

        Dim cell As EwECell = Nothing
        ' StartIndex used for display purposes only, has no effect on data whatsoever
        Dim iStartIndex As Integer = Me.Core.EcosimFirstYear
        Dim sValue As Single = 0.0!

        For iValue As Integer = 1 To Me.m_iNumValues

            ' Determine value for given time point
            sValue = 0.0!
            If (Me.m_shape IsNot Nothing) Then
                If iValue <= Me.m_shape.nPoints Then
                    sValue = Me.m_shape.ShapeData(iValue)
                End If
            End If

            Select Case Me.m_displayMode

                Case frmShapeValue.eDisplayMode.Index

                    cell = New EwECell(CStr(iValue), GetType(String))
                    cell.Style = cStyleGuide.eStyleFlags.NotEditable
                    Me(iValue, 0) = cell

                    cell = New EwECell(sValue, GetType(Single))
                    cell.SuppressZero = Me.m_bSuppressZeroes
                    Me(iValue, 1) = cell

                Case frmShapeValue.eDisplayMode.Yearly

                    cell = New EwECell(CStr(iValue + iStartIndex - 1), GetType(String))
                    cell.Style = cStyleGuide.eStyleFlags.NotEditable
                    Me(iValue, 0) = cell

                    cell = New EwECell(sValue, GetType(Single))
                    cell.SuppressZero = Me.m_bSuppressZeroes
                    cell.DataModel.DefaultValue = 0.0!
                    Me(iValue, 1) = cell

                Case frmShapeValue.eDisplayMode.Monthly

                    Dim strLabel0 As String = ""
                    Dim strLabel1 As String = ""
                    Dim iYear As Integer = iStartIndex + CInt(Math.Floor((iValue - 1) / 12))
                    Dim iMonth As Integer = 1 + ((iValue - 1) Mod 12)
                    Dim d As New Date(1, iMonth, 1)

                    If iMonth = 1 Then strLabel0 = CStr(iYear) Else strLabel0 = ""
                    strLabel1 = d.ToString("MMM")

                    cell = New EwECell(strLabel0, GetType(String))
                    cell.Style = cStyleGuide.eStyleFlags.NotEditable
                    Me(iValue, 0) = cell

                    cell = New EwECell(strLabel1, GetType(String))
                    cell.Style = cStyleGuide.eStyleFlags.NotEditable
                    Me(iValue, 1) = cell

                    cell = New EwECell(sValue, GetType(Single))
                    cell.SuppressZero = Me.m_bSuppressZeroes
                    cell.DataModel.DefaultValue = 0.0!
                    Me(iValue, 2) = cell

            End Select

        Next

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        ' No need for fixed columns
        Me.FixedColumns = 0
    End Sub

#End Region ' Grid overrides

#Region " Internals "

    ''' <summary>
    ''' Get the index of the value column in the grid.
    ''' </summary>
    Private ReadOnly Property ValueColumn() As Integer
        Get
            If Me.m_displayMode = frmShapeValue.eDisplayMode.Monthly Then
                Return 2
            Else
                Return 1
            End If
        End Get
    End Property

#End Region ' Internals

End Class
