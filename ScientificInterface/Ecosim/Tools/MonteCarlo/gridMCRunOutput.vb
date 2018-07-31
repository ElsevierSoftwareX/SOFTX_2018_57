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
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class gridMCRunOutput
        : Inherits EwEGrid

        Private m_mcmanager As cMonteCarloManager = Nothing

        Public Sub New()
            MyBase.New()
        End Sub

        Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
                If (value IsNot Nothing) Then
                    Me.m_mcmanager = value.Core.EcosimMonteCarlo
                Else
                    Me.m_mcmanager = Nothing
                End If
                MyBase.UIContext = value
            End Set
        End Property

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Me.Redim(Me.Core.nLivingGroups + 1, 7)
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMASS)
            Me(0, 3) = New EwEColumnHeaderCell(eVarNameFlags.PBOutput, eDescriptorTypes.Abbreviation)
            Me(0, 4) = New EwEColumnHeaderCell(SharedResources.HEADER_CB)
            Me(0, 5) = New EwEColumnHeaderCell(eVarNameFlags.EEInput, eDescriptorTypes.Abbreviation)
            Me(0, 6) = New EwEColumnHeaderCell(SharedResources.HEADER_BIOMACCUM_ABBR)
            'Me(0, 7) = New EwEColumnHeaderCell("Landings")
            'Me(0, 8) = New EwEColumnHeaderCell("Discards")

            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Sub FillData()

            Dim mcGrp As cCoreGroupBase = Nothing

            For i As Integer = 1 To Me.Core.nLivingGroups
                mcGrp = m_mcmanager.Groups(i)
                Me(i, 0) = New EwERowHeaderCell(CStr(mcGrp.Index))
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, mcGrp, eVarNameFlags.Name)
                Me(i, 2) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcBbf)
                Me(i, 3) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcPBbf)
                Me(i, 4) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcQBbf)
                Me(i, 5) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcEEbf)
                Me(i, 6) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcBAbf)
                'Me(i, 7) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcLandingsbf)
                'Me(i, 8) = New PropertyCell(Me.PropertyManager, mcGrp, eVarNameFlags.mcDiscardsbf)
            Next

        End Sub

    End Class

End Namespace


