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
Imports System.Reflection
Imports System.Windows.Forms
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports SourceGrid2

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' 
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class ucUnitGrid
    : Inherits EwEGrid

    Private Class cUnitSorter
        Implements IComparer(Of cUnit)

        Public Function Compare(x As cUnit, y As cUnit) As Integer _
            Implements System.Collections.Generic.IComparer(Of cUnit).Compare
            Return String.Compare(x.Name, y.Name)
        End Function

    End Class

    Private m_data As cData = Nothing
    Private m_unitType As cUnitFactory.eUnitType = cUnitFactory.eUnitType.Producer
    Private m_lUnits As List(Of cUnit) = Nothing

    Private m_dtProps As New Dictionary(Of String, List(Of PropertyInfo))
    Private m_api As PropertyInfo() = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="unitType"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext, _
                   ByVal data As cData, _
                   ByVal unitType As cUnitFactory.eUnitType)

        Me.m_data = data
        Me.m_unitType = unitType
        Me.DataName = "VC_" & CStr(unitType)

        ' Get all defined units of this type
        Me.m_lUnits = New List(Of cUnit)
        Me.m_lUnits.AddRange(Me.m_data.GetUnits(Me.m_unitType))

        ' Bingaling
        Me.m_lUnits.Sort(New cUnitSorter())

        ' Get list of properties supported by this type
        Me.m_api = cPropertyInfoHelper.GetAllowedProperties(cUnitFactory.MapType(Me.m_unitType))

        ' Go!
        Me.UIContext = uic

    End Sub

#Region " Events "

    Private Sub OnDisposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Me.m_lUnits = Nothing
    End Sub

#End Region ' Events

#Region " Internals "

    Protected Overrides Sub InitLayout()
        MyBase.InitLayout()

        Me.GridToolTipActive = True
        Me.Selection.SelectionMode = GridSelectionMode.Cell
        Me.Selection.ProtectReadOnly = True

        Me.FixedColumnWidths = False

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.FixedRows = 1
        Me.FixedColumns = 1
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub FillData()

        Dim strHeader As String = ""
        Dim pi As PropertyInfo = Nothing
        Dim pd As PropertyDescriptor = Nothing

        Me.Redim(Me.m_api.Length + 1, Me.m_lUnits.Count + 1)


        ' For every row
        For iRow As Integer = 0 To Me.RowsCount - 1
            If iRow = 0 Then
                strHeader = Me.m_unitType.ToString
            Else
                ' Get property info
                pi = Me.m_api(iRow - 1)
                ' Extract name
                strHeader = pi.Name
                ' Try to fing 'DisplayName' if available. This field is available through
                ' underlying PropertyDescriptor *sigh*
                pd = cPropertyConverter.FindOrigPropertyDescriptor(pi)
                ' Does pd exist?
                If pd IsNot Nothing Then
                    ' #Yes: has DisplayName?
                    If Not String.IsNullOrEmpty(pd.DisplayName) Then
                        ' #Yes: use it
                        strHeader = pd.DisplayName
                    End If
                End If
            End If

            ' Populate row
            Me(iRow, 0) = New EwERowHeaderCell(strHeader)

        Next iRow

        For iCol As Integer = 0 To Me.m_lUnits.Count - 1
            Me.AddUnit(Me.m_lUnits(iCol), iCol + 1)
        Next

        Me.AutoSizeColumn(0, 140)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <param name="iCol"></param>
    ''' -----------------------------------------------------------------------
    Private Sub AddUnit(ByVal unit As cUnit, ByVal iCol As Integer)
        For iRow As Integer = 0 To Me.RowsCount - 1
            Me.AddCell(unit, iRow, iCol)
        Next
    End Sub

    Protected Sub AddCell(ByVal unit As cUnit, ByVal iRow As Integer, ByVal iCol As Integer)
        Dim cell As Cells.Real.Cell = Nothing

        If iRow = 0 Then
            cell = New EwERowHeaderCell(CStr(iCol))
        Else
            cell = New cPropertyInfoCell(unit, Me.m_api(iRow - 1))
        End If
        Me(iRow, iCol) = cell

    End Sub

#End Region ' Internals

End Class
