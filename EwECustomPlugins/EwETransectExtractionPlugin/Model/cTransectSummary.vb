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
Imports System.Drawing
Imports EwECore
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Cell summaryt for a single transect.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTransectSummary

    Private m_values As Single()

    Public Sub New(t As cTransect, bm As cEcospaceBasemap, l As cEcospaceLayer, iIndex As Integer)

        Me.Transect = t
        Me.Name = l.Name

        Dim cells As Point() = t.Cells(bm)
        ReDim m_values(cells.Count - 1)
        For iCell As Integer = 0 To cells.Count - 1
            Dim pt As Point = cells(iCell)
            Dim sValue As Single = cCore.NULL_VALUE
            If bm.IsModelledCell(pt.Y, pt.X) Or l.VarName = eVarNameFlags.LayerDepth Then
                sValue = CSng(l.Cell(pt.Y, pt.X, iIndex))
            End If
            Me.m_values(iCell) = sValue
        Next

    End Sub

    Public Sub New(t As cTransect, bm As cEcospaceBasemap, strName As String, ecospaceoutput As Single(,,), iIndex As Integer)

        Me.Transect = t
        Me.Name = strName

        Dim cells As Point() = t.Cells(bm)
        ReDim m_values(cells.Count - 1)
        For iCell As Integer = 0 To cells.Count - 1
            Dim pt As Point = cells(iCell)
            Dim sValue As Single = cCore.NULL_VALUE
            If bm.IsModelledCell(pt.Y, pt.X) Then
                sValue = ecospaceoutput(pt.Y, pt.X, iIndex)
            End If
            Me.m_values(iCell) = sValue
        Next

    End Sub

    Public ReadOnly Property Transect As cTransect
    Public ReadOnly Property Name As String

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="i">Zero-based index</param>
    ''' <returns></returns>
    Public ReadOnly Property Value(i As Integer) As Single
        Get
            Return Me.m_values(i)
        End Get
    End Property

End Class