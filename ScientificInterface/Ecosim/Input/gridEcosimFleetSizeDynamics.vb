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

#End Region

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class gridEcosimFleetSizeDynamics
        : Inherits EwEGrid

        Public Sub New()
            MyBase.new()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()
            ' Redim the grid dimension
            Me.Redim(1, 6)

            ' Define column header
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
            Me(0, 2) = New EwEColumnHeaderCell(eVarNameFlags.EPower)
            Me(0, 3) = New EwEColumnHeaderCell(eVarNameFlags.PcapBase)
            Me(0, 4) = New EwEColumnHeaderCell(eVarNameFlags.CapDepreciate)
            Me(0, 5) = New EwEColumnHeaderCell(eVarNameFlags.CapBaseGrowth)

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreInputOutputBase = Nothing

            For iRow As Integer = 1 To core.nFleets
                source = core.EcosimFleetInputs(iRow)
                Me.Rows.Insert(iRow)
                Me(iRow, 0) = New EwERowHeaderCell(CStr(iRow))
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                Me(iRow, 2) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.EPower)
                Me(iRow, 3) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.PcapBase)
                Me(iRow, 4) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.CapDepreciate)
                Me(iRow, 5) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.CapBaseGrowth)
            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoSim
            End Get
        End Property

    End Class

End Namespace

