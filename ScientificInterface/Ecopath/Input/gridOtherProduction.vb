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
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Other Production user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)>
    Public Class gridOtherProduction
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
        End Sub

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            Immig
            Emig
            EmigRate
            BioAccum
            BioAccumRate
        End Enum

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()


            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)
            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell()
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(eVarNameFlags.Name)
            Me(0, eColumnTypes.Immig) = New EwEColumnHeaderCell(eVarNameFlags.Immig)
            Me(0, eColumnTypes.Emig) = New EwEColumnHeaderCell(eVarNameFlags.Emig)
            Me(0, eColumnTypes.EmigRate) = New EwEColumnHeaderCell(eVarNameFlags.EmigRate)
            Me(0, eColumnTypes.BioAccum) = New EwEColumnHeaderCell(eVarNameFlags.BioAccumInput)
            Me(0, eColumnTypes.BioAccumRate) = New EwEColumnHeaderCell(eVarNameFlags.BioAccumRate, eDescriptorTypes.Abbreviation)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim groups As cCoreGroupBase() = Me.StyleGuide.Groups(Me.Core)
            Dim group As cCoreGroupBase = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = -1
            Dim iStanzaPrev As Integer = -1
            Dim hgcStanza As EwEHierarchyGridCell = Nothing

            'Remove existing rows
            Me.RowsCount = 1

            'Create rows for all groups
            For i As Integer = 0 To groups.Count - 1

                group = groups(i)

                If group.IsMultiStanza Then
                    sg = Core.StanzaGroups(group.iStanza)
                    If (group.iStanza <> iStanzaPrev) Then
                        ' Create stanza header row
                        iRow = Me.AddRow
                        hgcStanza = New EwEHierarchyGridCell()
                        Me(iRow, eColumnTypes.Index) = hgcStanza
                        Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)
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

                Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderChildCell(Me.PropertyManager, group, eVarNameFlags.Name)
                Me(iRow, eColumnTypes.Immig) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.Immig)
                Me(iRow, eColumnTypes.Emig) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.Emig)
                Me(iRow, eColumnTypes.EmigRate) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.EmigRate)
                Me(iRow, eColumnTypes.BioAccum) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.BioAccumInput)
                Me(iRow, eColumnTypes.BioAccumRate) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.BioAccumRate)

            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
