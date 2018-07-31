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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.BehaviorModels

#End Region

Namespace Ecospace

    ''' =======================================================================
    ''' <summary>
    ''' Grid control, implements the Ecospace interface to assign species to habitats.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class gridHabitatPreference
        : Inherits EwEGrid

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
        End Sub

#End Region ' Construction / destruction

#Region " Overrides "

        Protected Overrides Sub InitStyle()

            'Call base class InitStyle method. 
            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreInputOutputBase = Nothing

            'Define grid dimensions
            Me.Redim(Me.Core.nGroups + 2, Me.Core.nHabitats + 2)

            'Set header cells # (0,0)
            Me(0, 0) = New EwEColumnHeaderCell(My.Resources.ECOSPACE_HEADER_GROUP_HABITAT)
            Me(0, 0).ColumnSpan = 2

            'Dynamic row header - group name 
            For i As Integer = 1 To Me.Core.nGroups
                source = Me.Core.EcospaceGroupInputs(i)
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                ' # Group name row header cells
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
            Next

            'Dynamic column header - Habitat name
            For j As Integer = 0 To Me.Core.nHabitats - 1
                source = Me.Core.EcospaceHabitats(j)
                ' +1 to compensate for header column, +1 to compensate for zero-based habitat index.
                Me(0, j + 2) = New EwEColumnHeaderCell(source.Name)
            Next

            Me.FixedColumns = 2
            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Sub FillData()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim groupEcospace As cEcospaceGroupInput = Nothing
            Dim groupEcopath As cEcoPathGroupInput = Nothing
            Dim hab As cEcospaceHabitat = Nothing
            Dim cell As EwECellBase = Nothing

            For iGroup As Integer = 1 To Me.Core.nGroups

                ' Get sources
                groupEcospace = Me.Core.EcospaceGroupInputs(iGroup)
                groupEcopath = Me.Core.EcoPathGroupInputs(iGroup)

                For iHabitat As Integer = 0 To Me.Core.nHabitats - 1

                    hab = Me.Core.EcospaceHabitats(iHabitat)

                    ' Create proportion cell (was checkbox)
                    cell = New PropertyCell(Me.PropertyManager, groupEcospace, eVarNameFlags.PreferredHabitat, hab)
                    cell.Behaviors.Add(Me.EwEEditHandler)
                    cell.SuppressZero = True
                    Me(iGroup, iHabitat + 2) = cell

                Next

            Next

        End Sub

        Public Overrides ReadOnly Property CoreComponents() As eCoreComponentType()
            Get
                Return New eCoreComponentType() {eCoreComponentType.EcoPath, eCoreComponentType.EcoSpace}
            End Get
        End Property

#End Region ' Overrides

    End Class

End Namespace

