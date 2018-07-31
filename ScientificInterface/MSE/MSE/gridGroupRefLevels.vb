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
Imports EwECore.MSE
Imports SourceGrid2.Cells.Real
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

<CLSCompliant(False)> _
Public Class gridGroupRefLevels
    : Inherits EwEGrid

    Public Sub New()
    End Sub

    Protected Overrides Sub InitStyle()

        MyBase.InitStyle()

        Me.Redim(1, 6)
        Me(0, 0) = New EwEColumnHeaderCell("")
        Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
        Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_LOWER)
        Me(0, 3) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS_UPPER)
        Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_LOWER)
        Me(0, 5) = New EwEColumnHeaderCell(SharedResources.HEADER_CATCH_UPPER)

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False

    End Sub

    Protected Overrides Sub FillData()
        Try

            Dim mse As cMSEManager = Me.UIContext.Core.MSEManager
            If mse Is Nothing Then Exit Sub

            For i As Integer = 1 To Me.UIContext.Core.nLivingGroups

                Me.Rows.Insert(i)
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, mse.GroupInputs(i), eVarNameFlags.Name)
                Me(i, 2) = New PropertyCell(Me.PropertyManager, mse.GroupInputs(i), eVarNameFlags.MSERefBioLower)
                Me(i, 3) = New PropertyCell(Me.PropertyManager, mse.GroupInputs(i), eVarNameFlags.MSERefBioUpper)
                Me(i, 4) = New PropertyCell(Me.PropertyManager, mse.GroupInputs(i), eVarNameFlags.MSERefGroupCatchLower)
                Me(i, 5) = New PropertyCell(Me.PropertyManager, mse.GroupInputs(i), eVarNameFlags.MSERefGroupCatchUpper)

            Next

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.MSE
        End Get
    End Property

End Class
