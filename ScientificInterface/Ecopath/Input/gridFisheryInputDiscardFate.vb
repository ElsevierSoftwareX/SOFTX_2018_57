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
Imports SourceGrid2.Cells.Real

#End Region

Namespace Ecopath.Input

    ''' =======================================================================
    ''' <summary>
    ''' Grid accepting Ecopath Discard Fate user input.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridFisheryInputDiscardFate
        : Inherits EwEGrid

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreInputOutputBase = Nothing

            Me.Redim(Core.nFleets + 1, Core.nDetritusGroups + 4)

            ' Grid Cell (0, 0) - Fleet name
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)

            ' Dynamic column header - Detritus groups
            For columnIndex As Integer = 1 To Core.nDetritusGroups
                source = Core.EcoPathGroupInputs(Core.nGroups - Core.nDetritusGroups + columnIndex)
                Me(0, columnIndex + 1) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Next

            ' Export header cell
            Me(0, Me.ColumnsCount - 2) = New EwEColumnHeaderCell(SharedResources.HEADER_EXPORT)

            ' Sum header cell
            Me(0, Me.ColumnsCount - 1) = New EwEColumnHeaderCell(SharedResources.HEADER_SUM)

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreInputOutputBase = Nothing
            Dim sourceSec As cCoreInputOutputBase = Nothing

            Dim sum As New cSingleProperty()
            sum.SetValue(1.0)
            sum.SetStyle(cStyleGuide.eStyleFlags.Sum Or cStyleGuide.eStyleFlags.NotEditable)

            Dim prop As cProperty = Nothing
            Dim alSumAll As New ArrayList()

            Dim opSumAll As cMultiOperation = Nothing
            Dim propSumAll As cFormulaProperty = Nothing
            Dim opMinus As cBinaryOperation = Nothing
            Dim propExport As cFormulaProperty = Nothing

            ' For each fleet
            For iRow As Integer = 1 To core.nFleets
                'Get the fleet info
                source = core.EcopathFleetInputs(iRow)
                ' Clear the arrayList for the sum of new row
                alSumAll.Clear()
                ' Fleet name As row header
                Me(iRow, 0) = New EwERowHeaderCell(CStr(iRow))
                Me(iRow, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                For columnIndex As Integer = 2 To core.nDetritusGroups + 1
                    ' Get the ecopath input
                    sourceSec = Me.Core.EcoPathGroupInputs(columnIndex - 1)
                    ' Dynamic indexed Discard fate property 
                    prop = Me.PropertyManager.GetProperty(source, eVarNameFlags.DiscardFate, sourceSec)
                    ' Add prop to the arraylist
                    alSumAll.Add(prop)
                    'assigned it to destined cell
                    Me(iRow, columnIndex) = New PropertyCell(prop)
                Next

                ' Get the sum of discard fate of all detritus groups
                opSumAll = New cMultiOperation(cMultiOperation.eOperatorType.Sum, alSumAll.ToArray)
                propSumAll = Me.Formula(opSumAll)

                ' Calculate the export
                opMinus = New cBinaryOperation(cBinaryOperation.eOperatorType.Subtract, sum, propSumAll)

                ' Get the export property
                propExport = Me.Formula(opMinus)

                Me(iRow, Me.ColumnsCount - 2) = New PropertyCell(propExport)
                ' The property cell for the sum column, which is not editable and equal to 1
                Me(iRow, Me.ColumnsCount - 1) = New PropertyCell(sum)
            Next

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace

