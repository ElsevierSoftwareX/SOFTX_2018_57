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
Imports EwEUtils.Core
Imports SourceGrid2.Cells
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

<CLSCompliant(False)> _
Public Class gridFixedEscapement
    Inherits EwEGrid

    Public Sub New()
        MyBase.new()
    End Sub

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Me.Redim(1, 5)
        Me(0, 0) = New EwEColumnHeaderCell("")
        Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
        Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_FIXEDESCAPE)
        Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_FIXEDF)
        Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_TAC)

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False

    End Sub

    Protected Overrides Sub FillData()

        Dim MSEGrp As cMSEGroupInput = Nothing
        Dim group As cCoreInputOutputBase = Nothing
        Dim cell As ICell = Nothing

        ' For each group
        For iGroup As Integer = 1 To Me.Core.nLivingGroups

            Me.AddRow()

            ' Get the group info
            group = Me.Core.EcoPathGroupInputs(iGroup)
            MSEGrp = Me.Core.MSEManager.GroupInputs(iGroup)

            Me(iGroup, 0) = New EwERowHeaderCell(CStr(iGroup))

            'Group name as row header
            Me(iGroup, 1) = New PropertyRowHeaderCell(Me.PropertyManager, MSEGrp, eVarNameFlags.Name)
            Me(iGroup, 2) = New PropertyCell(Me.PropertyManager, MSEGrp, eVarNameFlags.MSEFixedEscapement)
            Me(iGroup, 3) = New PropertyCell(Me.PropertyManager, MSEGrp, eVarNameFlags.MSEFixedF)
            Me(iGroup, 4) = New PropertyCell(Me.PropertyManager, MSEGrp, eVarNameFlags.MSETAC)

        Next

    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.MSE
        End Get
    End Property

End Class
