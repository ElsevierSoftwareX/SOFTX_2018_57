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
Imports EwECore.Style
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecotracer

    <CLSCompliant(False)>
    Public Class gridEcotracerInput
        Inherits EwEGrid

        Private Enum eColumnTypes As Integer
            Index = 0
            GroupName
            ConcEnv
            ConcImmBiomass
            DirectAbsorptionRate
            PhysicalDecayRate
            AssimProp
            MetablismRate
        End Enum

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim core As cCore = Me.UIContext.Core
            Dim source As cCoreGroupBase = Nothing

            ' Define grid dimensions
            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Dim rowCnt As Integer = Me.RowsCount
            ' Set header cells
            ' # (0,0)
            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.GroupName) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, eColumnTypes.ConcEnv) = New EwEColumnHeaderCell(SharedResources.HEADER_CONCENTRATION_INITIAL)
            Me(0, eColumnTypes.ConcImmBiomass) = New EwEColumnHeaderCell(SharedResources.HEADER_CONCENTRATION_IN_IMM_B)
            Me(0, eColumnTypes.DirectAbsorptionRate) = New EwEColumnHeaderCell(SharedResources.HEADER_DIRECT_ABSORPTION_RATE)
            Me(0, eColumnTypes.PhysicalDecayRate) = New EwEColumnHeaderCell(SharedResources.HEADER_PHYSICAL_DECAY_RATE)
            Me(0, eColumnTypes.AssimProp) = New EwEColumnHeaderCell(SharedResources.HEADER_EXCRETION_RATE)
            Me(0, eColumnTypes.MetablismRate) = New EwEColumnHeaderCell(SharedResources.HEADER_METABOLISMRATE)

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim core As cCore = Me.UIContext.Core
            Dim group As cEcotracerGroupInput = Nothing
            Dim iRow As Integer = -1

            ' Remove existing rows
            Me.RowsCount = 1

            ' Create rows for all groups
            For iGroup As Integer = 1 To core.nGroups

                group = core.EcotracerGroupInputs(iGroup)

                iRow = Me.AddRow()
                Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                Me(iRow, eColumnTypes.GroupName) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
                Me(iRow, eColumnTypes.ConcEnv) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.CZero)
                Me(iRow, eColumnTypes.ConcImmBiomass) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.CImmig)
                Me(iRow, eColumnTypes.DirectAbsorptionRate) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.CEnvironment)
                Me(iRow, eColumnTypes.PhysicalDecayRate) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.CPhysicalDecayRate)
                Me(iRow, eColumnTypes.AssimProp) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.CAssimilationProp)
                Me(iRow, eColumnTypes.MetablismRate) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.CMetablismRate)

            Next iGroup

        End Sub

    End Class

End Namespace ' Ecotracer
