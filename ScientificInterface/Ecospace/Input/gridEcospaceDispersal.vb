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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.BehaviorModels

#End Region

Namespace Ecospace

    ''' =======================================================================
    ''' <summary>
    ''' Grid control, implements the Ecospace interface to set dispersal rates.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridEcospaceDispersal
        : Inherits EwEGrid

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            DispersalRate
            RelDisp
            RelVul
            RelFeedRate
            Advected
            Migrating
            BarrierAvoidance
            InMigMovement
        End Enum

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
        End Sub

#End Region ' Construction / destruction

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            'Add column headers
            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, eColumnTypes.DispersalRate) = New EwEColumnHeaderCell(eVarNameFlags.MVel)
            Me(0, eColumnTypes.RelDisp) = New EwEColumnHeaderCell(eVarNameFlags.RelMoveBad)
            Me(0, eColumnTypes.RelVul) = New EwEColumnHeaderCell(eVarNameFlags.RelVulBad)
            Me(0, eColumnTypes.RelFeedRate) = New EwEColumnHeaderCell(eVarNameFlags.EatEffBad)
            Me(0, eColumnTypes.Advected) = New EwEColumnHeaderCell(eVarNameFlags.IsAdvected)
            Me(0, eColumnTypes.Migrating) = New EwEColumnHeaderCell(eVarNameFlags.IsMigratory)
            Me(0, eColumnTypes.BarrierAvoidance) = New EwEColumnHeaderCell(eVarNameFlags.BarrierAvoidanceWeight)
            Me(0, eColumnTypes.InMigMovement) = New EwEColumnHeaderCell(eVarNameFlags.InMigAreaMoveWeight)

            Me.FixedColumnWidths = True

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cEcospaceGroupInput = Nothing
            Dim cell As EwECellBase = Nothing

            For iGroup As Integer = 1 To Me.Core.nGroups
                Me.Rows.Insert(iGroup)

                source = Me.Core.EcospaceGroupInputs(iGroup)
                Me(iGroup, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
                Me(iGroup, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)

                'MVel - Base dispersal rate
                cell = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.MVel)
                cell.SuppressZero = False
                Me(iGroup, eColumnTypes.DispersalRate) = cell
                'Rel dispersal in bad habitat
                Me(iGroup, eColumnTypes.RelDisp) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.RelMoveBad)
                ' Rel. vul.to pred. in bad habitat
                Me(iGroup, eColumnTypes.RelVul) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.RelVulBad)
                'Rel. feed.rate in bad habitat
                Me(iGroup, eColumnTypes.RelFeedRate) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.EatEffBad)
                'Advected?
                Me(iGroup, eColumnTypes.Advected) = New PropertyCheckboxCell(Me.PropertyManager, source, eVarNameFlags.IsAdvected)
                'Migrating?
                Me(iGroup, eColumnTypes.Migrating) = New PropertyCheckboxCell(Me.PropertyManager, source, eVarNameFlags.IsMigratory)
                'Barrier avoidance weight
                Me(iGroup, eColumnTypes.BarrierAvoidance) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BarrierAvoidanceWeight)
                Me(iGroup, eColumnTypes.InMigMovement) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.InMigAreaMoveWeight)

            Next

        End Sub

        Public Overrides ReadOnly Property CoreComponents() As eCoreComponentType()
            Get
                ' Refresh on Ecopath notifications
                Return New eCoreComponentType() {eCoreComponentType.EcoPath, eCoreComponentType.EcoSpace}
            End Get
        End Property

    End Class

End Namespace
