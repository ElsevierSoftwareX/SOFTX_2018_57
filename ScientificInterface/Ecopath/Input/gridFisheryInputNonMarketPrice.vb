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
Imports EwECore.Style
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Non-Market price user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)>
    Public Class gridFisheryInputNonMarketPrice
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()
            Me.Redim(1, 3)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            ' The header text changed. Is that ok?
            Me(0, 2) = New EwEColumnHeaderCell(eVarNameFlags.NonMarketValue)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cCoreGroupBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim dt As New Dictionary(Of cStanzaGroup, EwEHierarchyGridCell)
            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            'Remove existing rows
            Me.RowsCount = 1

            'Create rows for all groups
            For i As Integer = 0 To groups.Count - 1
                group = groups(i)
                If Not group.IsMultiStanza Then
                    iRow = Me.AddRow
                    FillInRows(iRow, group)
                Else
                    sg = Core.StanzaGroups(group.iStanza)
                    If Not dt.ContainsKey(sg) Then
                        hgcStanza = New EwEHierarchyGridCell()
                        iRow = Me.AddRow()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
                        For j As Integer = 2 To 2 : Me(iRow, j) = New EwERowHeaderCell() : Next
                        dt(sg) = hgcStanza
                        iRow = Me.AddRow
                    Else
                        hgcStanza = dt(sg)
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If
                    hgcStanza.AddChildRow(iRow)
                    FillInRows(iRow, group, True)
                End If
            Next

        End Sub

        Private Sub FillInRows(ByVal iRow As Integer, _
                               ByVal group As cCoreGroupBase, _
                               Optional ByVal isIndented As Boolean = False)

            ' Get the group name from EcopathInput
            Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)

            If isIndented Then
                Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, group, eVarNameFlags.Name)
            Else
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
            End If

            Me(iRow, 2) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.NonMarketValue)

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
