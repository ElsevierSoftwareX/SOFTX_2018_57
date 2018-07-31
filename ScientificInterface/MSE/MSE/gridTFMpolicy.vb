﻿' ===============================================================================
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
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells

#End Region ' Imports

Namespace Ecosim

    ''' ===========================================================================
    ''' <summary>
    ''' Grid to allow species quota interaction.
    ''' </summary>
    ''' ===========================================================================
    <CLSCompliant(False)> _
    Public Class gridTargetFishingMortalityPolicy
        Inherits EwEGrid

#Region " Internal defs "

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            BLim
            BBase
            FOpt
        End Enum

#End Region ' Internal defs

#Region " Constructor "

        Public Sub New()
            MyBase.new()
        End Sub

#End Region ' Constructor

#Region " Public interfaces "

        Public Property Group() As cMSEGroupInput
            Get
                If Me.Selection.SelectedRows.Length = 1 Then
                    Return DirectCast(Me.Selection.SelectedRows(0).Tag, cMSEGroupInput)
                End If
                Return Nothing
            End Get
            Set(ByVal value As cMSEGroupInput)
                Me.Selection.Clear()
                If value IsNot Nothing Then
                    Me.Selection.Add(New Position(value.Index, 0))
                End If
                Me.RaiseSelectionChangeEvent()
            End Set
        End Property

#End Region ' Public interfaces

#Region " Overrides "

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

            Me.Redim(1, iNumCols)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, eColumnTypes.BBase) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_BASE)
            Me(0, eColumnTypes.BLim) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_LIMIT)
            Me(0, eColumnTypes.FOpt) = New EwEColumnHeaderCell(SharedResources.HEADER_FOPT)

            Me.FixedColumns = 2
            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Sub FillData()

            Dim group As cMSEGroupInput = Nothing

            ' For each group
            For iGroup As Integer = 1 To Core.nLivingGroups

                'Get the group info
                group = Core.MSEManager.GroupInputs(iGroup)

                Me.AddRow()

                Me(iGroup, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iGroup))
                Me(iGroup, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)

                Me(iGroup, eColumnTypes.BBase) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.MSEBBase)
                Me(iGroup, eColumnTypes.BLim) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.MSEBLim)
                Me(iGroup, eColumnTypes.FOpt) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.MSEFmax)

                Me.Rows(iGroup).Tag = group

            Next iGroup

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.Selection.SelectionMode = GridSelectionMode.Row
        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoSim
            End Get
        End Property

#End Region ' Overrides

    End Class

End Namespace ' Ecosim