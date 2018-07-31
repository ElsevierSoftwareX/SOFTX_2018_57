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
Imports SourceGrid2
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports System.Reflection
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Database.cEwEDatabase
Imports EwECore

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Grid for showing a whack of links.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class ucLinkGrid
    : Inherits EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class for sorting links by name
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cLinkSorter
        Implements IComparer(Of cLink)

        Public Function Compare(x As cLink, y As cLink) As Integer _
            Implements System.Collections.Generic.IComparer(Of cLink).Compare
            Return String.Compare(x.Name, y.Name)
        End Function

    End Class

    Private m_data As cData = Nothing
    Private m_api As PropertyInfo() = Nothing
    Private m_links As cLink() = Nothing
    Private m_group As cCoreInputOutputBase = Nothing
    Private m_fleet As cCoreInputOutputBase = Nothing
    Private m_typeData As Type = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext, _
                   ByVal data As cData, _
                   ByVal t As Type)

        'Sanity check
        Debug.Assert(GetType(cLink).IsAssignableFrom(t))

        Me.m_data = data
        Me.m_api = cPropertyInfoHelper.GetAllowedProperties(t)
        Me.m_typeData = t

        ' Go!
        Me.UIContext = uic

    End Sub

#Region " Properties "

    Public Property Group As cCoreInputOutputBase
        Get
            Return Me.m_group
        End Get
        Set(value As cCoreInputOutputBase)
            If (ReferenceEquals(value, Me.m_group)) Then Return
            Me.m_group = value
            Me.RefreshContent()
        End Set
    End Property

    Public Property Fleet As cCoreInputOutputBase
        Get
            Return Me.m_fleet
        End Get
        Set(value As cCoreInputOutputBase)
            If (ReferenceEquals(value, Me.m_fleet)) Then Return
            Me.m_fleet = value
            Me.RefreshContent()
        End Set
    End Property

    Public ReadOnly Property CanFilter As Boolean
        Get
            Return (Me.m_typeData Is GetType(cLinkLandings))
        End Get
    End Property

#End Region ' Properties

#Region " Internals "

    Protected Overrides Sub InitLayout()
        MyBase.InitLayout()

        Me.GridToolTipActive = True
        Me.Selection.SelectionMode = GridSelectionMode.Cell
        Me.Selection.ProtectReadOnly = True

        Me.FixedColumnWidths = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub FillData()

        ' Properties show as columns, links listed on rows
        Me.m_links = Me.GetLinks()

        Dim nCols As Integer = 1 + Me.m_api.Length
        Dim nRows As Integer = 1 + Me.m_links.Length
        Dim strHeader As String = ""

        Me.Redim(nRows, nCols)
        Me.FixedRows = 1
        Me.FixedColumns = 1
        Me.AutoSize = True

        ' Set column headers
        For iCol As Integer = 0 To nCols - 1
            If iCol = 0 Then
                ' Index row
                strHeader = ""
            Else
                strHeader = Me.m_api(iCol - 1).Name
            End If
            Me(0, iCol) = New EwERowHeaderCell(strHeader)
        Next iCol

        ' Add link rows
        For iRow As Integer = 0 To nRows - 1
            If (iRow > 0) Then
                Me.AddLink(Me.m_links(iRow - 1), iRow)
            End If
        Next

        Me.AutoSizeColumn(0, 140)

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
    ''' <param name="link"></param>
    ''' <param name="iRow"></param>
    ''' -----------------------------------------------------------------------
    Private Sub AddLink(ByVal link As cLink, ByVal iRow As Integer)
        For iCol As Integer = 0 To Me.ColumnsCount - 1
            Me.AddCell(link, iRow, iCol)
        Next
    End Sub

    Protected Sub AddCell(ByVal link As cLink, ByVal iRow As Integer, ByVal iCol As Integer)

        Dim cell As Cells.Real.Cell = Nothing

        If iCol = 0 Then
            cell = New EwERowHeaderCell(CStr(iRow))
        Else
            Try

                Dim pi As PropertyInfo = Me.m_api(iCol - 1)
                If GetType(cOOPStorable).IsAssignableFrom(pi.PropertyType) Then
                    Dim obj As cOOPStorable = DirectCast(pi.GetValue(link, Nothing), cOOPStorable)
                    Dim strLabel As String = ""
                    If (obj IsNot Nothing) Then strLabel = obj.ToString
                    cell = New EwECell(strLabel, GetType(String), ScientificInterfaceShared.Style.cStyleGuide.eStyleFlags.NotEditable)
                Else
                    cell = New cPropertyInfoCell(link, pi)
                End If
            Catch ex As Exception

            End Try

        End If
        Me(iRow, iCol) = cell

    End Sub

    Private Function GetLinks() As cLink()

        Dim lLinks As New List(Of cLink)
        Dim bUse As Boolean

        For Each l As cLink In Me.m_data.GetLinks(Me.m_typeData)
            bUse = True

            If (TypeOf l Is cLinkLandings) Then
                Dim ll As cLinkLandings = DirectCast(l, cLinkLandings)
                If (Me.m_group IsNot Nothing) Then
                    bUse = bUse And (Object.Equals(ll.Group, Me.m_group))
                End If
                If (Me.m_fleet IsNot Nothing) Then
                    bUse = bUse And (Object.Equals(DirectCast(ll.Source, cProducerUnit).Fleet, Me.m_fleet))
                End If
            End If

            If (bUse) Then
                lLinks.Add(l)
            End If

        Next

        ' Whahoo
        lLinks.Sort(New cLinkSorter())

        Return lLinks.ToArray

    End Function

#End Region ' Internals

End Class
