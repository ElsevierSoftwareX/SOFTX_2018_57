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

Namespace Ecopath.Output

    <CLSCompliant(False)>
    Public Class gridRespiration
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, 7)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, 2) = New EwEColumnHeaderCell(eVarNameFlags.Respiration)
            Me(0, 3) = New EwEColumnHeaderCell(eVarNameFlags.Assimilation)
            Me(0, 4) = New EwEColumnHeaderCell(eVarNameFlags.RespAssim)
            Me(0, 5) = New EwEColumnHeaderCell(eVarNameFlags.ProdResp)
            Me(0, 6) = New EwEColumnHeaderCell(eVarNameFlags.RespBiom)

            Me.FixedColumns = 2
            Me.FixedColumnWidths = True

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cEcoPathGroupOutput = Nothing
            Dim cell As EwECellBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim iStanzaPrev As Integer = -1
            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            'Remove existing rows
            Me.RowsCount = 1

            ' Create rows for all groups
            For i As Integer = 0 To groups.Count - 1

                ' Get corresponding Ecopath output group 
                group = Me.Core.EcoPathGroupOutputs(groups(i).Index)

                If Not group.IsMultiStanza Then

                    iRow = Me.AddRow
                    FillInRows(iRow, group)

                Else
                    ' Group is stanza
                    sg = Core.StanzaGroups(group.iStanza)
                    If group.iStanza <> iStanzaPrev Then

                        ' Complete row with dummy cells
                        iRow = Me.AddRow()
                        For j As Integer = 0 To Me.ColumnsCount - 1 : Me(iRow, j) = New EwERowHeaderCell() : Next

                        hgcStanza = New EwEHierarchyGridCell()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)

                        iStanzaPrev = group.iStanza
                        iRow = Me.AddRow
                    Else
                        iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                    End If

                    'Display group info
                    hgcStanza.AddChildRow(iRow)
                    FillInRows(iRow, group, True)
                End If
            Next i

        End Sub

        Private Sub FillInRows(ByVal iRow As Integer, ByVal source As cCoreInputOutputBase, Optional ByVal isIndented As Boolean = False)
            Me(iRow, 0) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
            If isIndented Then
                Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Else
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            End If
            Me(iRow, 2) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.Respiration)
            Me(iRow, 3) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.Assimilation)
            Me(iRow, 4) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.RespAssim)
            Me(iRow, 5) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.ProdResp)
            Me(iRow, 6) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.RespBiom)
        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace

