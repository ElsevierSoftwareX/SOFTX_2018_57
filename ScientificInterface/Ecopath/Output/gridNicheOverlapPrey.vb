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
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Properties
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)> _
    Public Class gridNicheOverlapPrey
        : Inherits EwEGrid

        Public Sub New()
            MyBase.new()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreGroupBase = Nothing

            ' Define grid dimensions
            Me.Redim(Core.nLivingGroups + 1, 2)

            ' Set header cells
            ' # (0,0)
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            Dim columnIndex As Integer = 2

            ' For every living groups
            For i As Integer = 1 To Core.nLivingGroups
                'Get group output
                source = Core.EcoPathGroupOutputs(i)
                ' Define column header cell
                Me.Columns.Insert(columnIndex)
                Me(0, columnIndex) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
                ' Define row header cell
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                Me(i, 1) = New EwERowHeaderCell(source.Name)
                columnIndex = columnIndex + 1
            Next

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreGroupBase = Nothing
            Dim sourceSec As cCoreGroupBase = Nothing
            Dim prop As cProperty = Nothing

            For columnIndex As Integer = 2 To core.nLivingGroups + 1
                source = core.EcoPathGroupOutputs(columnIndex - 1)
                For rowIndex As Integer = 1 To core.nLivingGroups
                    ' Get the group output
                    sourceSec = core.EcoPathGroupOutputs(rowIndex)

                    If columnIndex <= rowIndex + 1 Then
                        If source.PP() <= 1 Then
                            Dim cell As PropertyCell = Nothing

                            ' Get the indexed property by (rowIndex, columnIndex)
                            prop = Me.PropertyManager.GetProperty(sourceSec, eVarNameFlags.Plap, source)
                            ' Add property to the cell
                            cell = New PropertyCell(prop)
                            ' Config cell
                            cell.SuppressZero = True
                            ' Place cell into grid
                            Me(rowIndex, columnIndex) = cell
                        End If
                    Else
                        Dim cell As cellNichePropertyColour = Nothing

                        ' Get the indexed property by (rowIndex, columnIndex)
                        prop = Me.PropertyManager.GetProperty(sourceSec, eVarNameFlags.Plap, source)
                        ' Add property to the cell
                        cell = New cellNichePropertyColour(prop)
                        ' Place cell into grid
                        Me(rowIndex, columnIndex) = cell
                    End If
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
