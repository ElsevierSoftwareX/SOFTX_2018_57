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
Imports EwECore.SearchObjectives
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class gridFitToTimeSeriesGroup
        : Inherits EwEGrid

        Private m_manager As ISearchObjective

        Private Enum eColumnTypes As Integer
            Index = 0
            Group
            FLimit
        End Enum

        Public Sub New()
            MyBase.New()
        End Sub

        Public Property Manager() As ISearchObjective
            Get
                Return Me.m_manager
            End Get
            Set(ByVal value As ISearchObjective)
                Me.m_manager = value
                Me.RefreshContent()
            End Set
        End Property

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Group) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUP)
            Me(0, eColumnTypes.FLimit) = New EwEColumnHeaderCell(SharedResources.HEADER_MAXFISHINGMORTAILITY)

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreGroupBase = Nothing

            If Me.Manager Is Nothing Then Return
            If Me.UIContext Is Nothing Then Return

            For i As Integer = 1 To Me.Core.nGroups
                source = m_manager.GroupObjectives(i)

                Me.Rows.Insert(i)
                Me(i, eColumnTypes.Index) = New EwERowHeaderCell(CStr(i))
                Me(i, eColumnTypes.Group) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                Me(i, eColumnTypes.FLimit) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSFishingLimit)
            Next

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumns = 1
            Me.FixedColumnWidths = False
        End Sub

    End Class

End Namespace


