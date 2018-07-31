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
Imports EwECore.MSE
Imports EwECore.Style
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecosim

    ''' ===========================================================================
    ''' <summary>
    ''' 
    ''' </summary>
    ''' ===========================================================================
    <CLSCompliant(False)>
    Public Class gridQuotaShare
        Inherits EwEGrid

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Dim src As cCoreInputOutputBase = Nothing

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Me.Redim(1, 2 + Core.nFleets + 1) ' Include sum column

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            For iFleet As Integer = 1 To Core.nFleets
                src = Core.EcopathFleetInputs(iFleet)
                Me(0, 1 + iFleet) = New PropertyColumnHeaderCell(Me.PropertyManager,
                                                                 src, eVarNameFlags.Name, Nothing,
                                                                 cUnits.Currency)
            Next
            Me(0, 1 + Core.nFleets + 1) = New EwEColumnHeaderCell(SharedResources.HEADER_SUM)

            Me.FixedColumns = 2
            Me.FixedColumnWidths = True
        End Sub

        Protected Overrides Sub FillData()

            Dim fleet As cMSEFleetInput = Nothing
            Dim group As cCoreInputOutputBase = Nothing
            Dim prop As cProperty = Nothing
            Dim propSum As cFormulaProperty = Nothing
            Dim opSum As cMultiOperation = Nothing
            Dim alPropSum As New ArrayList()

            ' For each group
            For iGroup As Integer = 1 To Core.nLivingGroups

                Me.AddRow()

                'Get the group info
                group = Core.EcoPathGroupInputs(iGroup)

                ' Fleet name As row header
                Me(iGroup, 0) = New EwERowHeaderCell(CStr(iGroup))
                Me(iGroup, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)

                ' Clear row sum
                alPropSum.Clear()

                ' Fleet cells
                For iFleet As Integer = 1 To Me.Core.nFleets
                    fleet = Core.MSEManager.EcopathFleetInputs(iFleet)
                    prop = Me.PropertyManager.GetProperty(fleet, eVarNameFlags.QuotaShare, group)
                    Me(iGroup, 1 + iFleet) = New PropertyCell(prop)

                    ' Add to sum row
                    alPropSum.Add(prop)
                Next

                ' Now create the formula property that will calculate the sum of all cells in the row
                opSum = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alPropSum.ToArray())
                ' Create sum property
                propSum = Me.Formula(opSum)
                ' Define sum cell
                Me(iGroup, 1 + Me.Core.nFleets + 1) = New PropertyCell(propSum)

            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoSim
            End Get
        End Property

    End Class

End Namespace ' Ecosim
