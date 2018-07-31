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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2.Cells

#End Region ' Imports

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Discard Mortality user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridFisheryInputDiscardMort
        Inherits EwEGrid

        Public Sub New()
            MyBase.new()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Dim src As cCoreInputOutputBase = Nothing

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Me.Redim(1, 2 + Core.nFleets)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            For iFleet As Integer = 1 To Me.Core.nFleets
                src = Core.EcopathFleetInputs(iFleet)
                Me(0, 1 + iFleet) = New PropertyColumnHeaderCell(Me.PropertyManager, src, eVarNameFlags.Name)
            Next

            Me.FixedColumns = 2
            Me.FixedColumnWidths = True

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cCoreGroupBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim fleet As cEcopathFleetInput = Nothing
            Dim iRow As Integer = 0
            Dim iStanzaPrev As Integer = -1
            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            ' For each group
            For i As Integer = 0 To groups.Count - 1

                group = groups(i)
                If group.IsMultiStanza Then
                    sg = Core.StanzaGroups(group.iStanza)
                    If (group.iStanza <> iStanzaPrev) Then
                        ' Create stanza header row
                        iRow = Me.AddRow
                        hgcStanza = New EwEHierarchyGridCell()
                        Me(iRow, 0) = hgcStanza
                        Me(iRow, 1) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
                        For j As Integer = 2 To Me.ColumnsCount - 1 : Me(iRow, j) = New EwERowHeaderCell() : Next
                        iStanzaPrev = group.iStanza
                    End If
                    ' Add group row as child to stanza
                    iRow = Me.AddRow
                    hgcStanza.AddChildRow(iRow)
                Else
                    ' Add regular group row
                    iRow = Me.AddRow
                    iStanzaPrev = -1
                End If

                ' Fleet name As row header
                Me(iRow, 0) = New EwERowHeaderCell(CStr(i))
                Me(iRow, 1) = New PropertyRowHeaderChildCell(Me.PropertyManager, group, eVarNameFlags.Name)

                ' Fleet cells
                For iFleet As Integer = 1 To Me.Core.nFleets
                    fleet = Core.EcopathFleetInputs(iFleet)
                    Me(iRow, 1 + iFleet) = New PropertyCell(Me.PropertyManager, fleet, eVarNameFlags.DiscardMortality, group)
                Next
            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
