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
    Public Class gridFleetFishingMortality
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = True
        End Sub

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim group As cCoreGroupBase = Nothing
            Dim fleet As cEcopathFleetInput = Nothing
            Dim iGroup As Integer = 0

            Me.Redim(Core.nLivingGroups + 1, 2 + Core.nFleets)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEET_GROUP)

            For iFleet As Integer = 1 To Core.nFleets
                fleet = Core.EcopathFleetInputs(iFleet)
                Me(0, 1 + iFleet) = New PropertyColumnHeaderCell(Me.PropertyManager,
                                                                 fleet, eVarNameFlags.Name, Nothing,
                                                                 cUnits.OverTime)
            Next iFleet

            For iGroup = 1 To Core.nLivingGroups
                group = Core.EcoPathGroupOutputs(iGroup)
                Me(iGroup, 0) = New EwERowHeaderCell(CStr(iGroup))
                Me(iGroup, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
            Next iGroup

            Me.FixedColumns = 2

        End Sub

        Protected Overrides Sub FillData()

            Dim group As cEcoPathGroupOutput = Nothing
            Dim fleet As cEcopathFleetInput = Nothing
            Dim cell As EwECell = Nothing
            Dim sLandings As Single = 0.0!
            Dim sDiscards As Single = 0.0!
            Dim sBiomass As Single = 0.0!
            Dim style As cStyleGuide.eStyleFlags = (cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.ValueComputed)

            For iFleet As Integer = 1 To Core.nFleets
                ' Get fleet
                fleet = Core.EcopathFleetInputs(iFleet)
                For iGroup As Integer = 1 To Core.nLivingGroups
                    group = Core.EcoPathGroupOutputs(iGroup)
                    ' Get values 
                    sLandings = fleet.Landings(iGroup)
                    'Only discards the suffer mortality
                    sDiscards = fleet.Discards(iGroup) * fleet.DiscardMortality(iGroup)
                    sBiomass = group.Biomass()

                    ' Create cell
                    If sBiomass > 0 Then
                        cell = New EwECell((sLandings + sDiscards) / sBiomass, GetType(Single), style)
                    Else
                        cell = New EwECell(0.0!, GetType(Single), style Or cStyleGuide.eStyleFlags.Null)
                    End If

                    ' Value cells suppress zeroes to increase legibility of the grid
                    cell.SuppressZero(0) = True

                    ' Activate the cell
                    Me(iGroup, 1 + iFleet) = cell
                    ' Next
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
