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

    <CLSCompliant(False)> _
    Public Class gridMortalityCoefficients
        : Inherits EwEGrid

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            PBZ
            FishMort
            PredMort
            BioAccum
            NetMig
            OtherMort
            'Spacer
            MortTot
            MortNat
        End Enum

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, eColumnTypes.PBZ) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_PBZ)
            Me(0, eColumnTypes.FishMort) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_FISHINGMORTRATE)
            Me(0, eColumnTypes.PredMort) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_PREDMORTRATE, cUnits.OverTime)
            Me(0, eColumnTypes.BioAccum) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_BIOMACCURATE, cUnits.OverTime)
            Me(0, eColumnTypes.NetMig) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_NETMIGRATE, cUnits.OverTime)
            Me(0, eColumnTypes.OtherMort) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_OTHERMORTRATE, cUnits.OverTime)
            Me(0, eColumnTypes.MortTot) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_FISHMORT_OVER_TOTMORT)
            Me(0, eColumnTypes.MortNat) = New EwEColumnHeaderCell(My.Resources.HEADER_MORTALITIES_PROP_NAT_MORT)

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

                If (group.IsLiving) Then

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
                            Me(iRow, eColumnTypes.Index) = hgcStanza
                            Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderParentCell(Me.PropertyManager, sg, eVarNameFlags.Name, Nothing, hgcStanza)

                            iStanzaPrev = group.iStanza
                            iRow = Me.AddRow
                        Else
                            iRow = Me.AddRow(hgcStanza.Row + hgcStanza.NumChildRows + 1)
                        End If

                        'Display group info
                        hgcStanza.AddChildRow(iRow)
                        FillInRows(iRow, group, True)
                    End If
                End If
            Next i

        End Sub

        Private Sub FillInRows(ByVal iRow As Integer, ByVal source As cEcoPathGroupOutput, Optional ByVal isIndented As Boolean = False)

            Dim cell As PropertyCell = Nothing
            Dim bMortAlert As Boolean = (source.MortCoOtherMort < 0) And (Not source.IsDetritus)
            Dim bCatchAlert As Boolean = (source.MortCoFishRate > source.PBOutput) And (Not source.IsDetritus)

            Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
            If isIndented Then
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderChildCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Else
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            End If
            Me(iRow, eColumnTypes.PBZ) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.PBOutput)
            Me(iRow, eColumnTypes.FishMort) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.MortCoFishRate)
            Me(iRow, eColumnTypes.PredMort) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.MortCoPredMort)
            Me(iRow, eColumnTypes.BioAccum) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BioAccumRatePerYear)
            Me(iRow, eColumnTypes.NetMig) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.MortCoNetMig)
            Me(iRow, eColumnTypes.OtherMort) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.MortCoOtherMort)
            'Me(iRow, eColumnTypes.Spacer) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.NotEditable)
            Me(iRow, eColumnTypes.MortTot) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FishMortTotMort)
            Me(iRow, eColumnTypes.MortNat) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.NatMortPerTotMort)

            Me.SetCellAlert(DirectCast(Me(iRow, eColumnTypes.Name), EwECellBase), bMortAlert)
            Me.SetCellAlert(DirectCast(Me(iRow, eColumnTypes.FishMort), EwECellBase), bMortAlert And bCatchAlert)
            Me.SetCellAlert(DirectCast(Me(iRow, eColumnTypes.OtherMort), EwECellBase), bMortAlert)

            'Me.SetCellComputed(DirectCast(Me(iRow, eColumnTypes.MortTot), EwECellBase))
            'Me.SetCellComputed(DirectCast(Me(iRow, eColumnTypes.MortNat), EwECellBase))

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            'With Me.Columns(eColumnTypes.Spacer)
            '    .AutoSizeMode = SourceGrid2.AutoSizeMode.None
            '    .Width = 3
            'End With
        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

        Private Sub SetCellAlert(ByVal cell As EwECellBase, ByVal bSetAlert As Boolean)
            If bSetAlert Then
                cell.Style = cell.Style Or cStyleGuide.eStyleFlags.Checked
            Else
                cell.Style = cell.Style And (Not cStyleGuide.eStyleFlags.Checked)
            End If
        End Sub

        Private Sub SetCellComputed(ByVal cell As EwECellBase)
            cell.Style = cell.Style Or cStyleGuide.eStyleFlags.ValueComputed
        End Sub

    End Class

End Namespace
