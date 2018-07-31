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

    ''' =======================================================================
    ''' <summary>
    ''' Grid allowing setting of Fleet search objectives.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
      Public Class gridSearchObjectivesFleet
        : Inherits EwEGrid

        Private m_Manager As ISearchObjective
        Private m_bIsMaxByFleetValue As Boolean = False

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = False
            Me.m_bIsMaxByFleetValue = False
        End Sub

        Public Property Manager() As ISearchObjective
            Get
                Return Me.m_Manager
            End Get
            Set(ByVal value As ISearchObjective)
                Me.m_Manager = value
                Me.RefreshContent()
            End Set
        End Property

        Public Property IsMaximizeByFleetValue() As Boolean
            Get
                Return m_bIsMaxByFleetValue
            End Get
            Set(ByVal value As Boolean)
                m_bIsMaxByFleetValue = value
                Me.RefreshContent()
            End Set
        End Property

#Region " Overrides "

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            If Not m_bIsMaxByFleetValue Then
                Me.Redim(1, 3)
                Me(0, 0) = New EwEColumnHeaderCell("")
                Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEET)
                Me(0, 2) = New EwEColumnHeaderCell(My.Resources.FPS_FG_JOBS)
            Else
                Me.Redim(1, 4)
                Me(0, 0) = New EwEColumnHeaderCell("")
                Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEET)
                Me(0, 2) = New EwEColumnHeaderCell(My.Resources.FPS_FG_JOBS)
                Me(0, 3) = New EwEColumnHeaderCell(My.Resources.FPS_FG_TP)
            End If

        End Sub

        Protected Overrides Sub FillData()

            If (Me.Manager Is Nothing) Then Return
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreGroupBase = Nothing

            For i As Integer = 1 To Me.UIContext.Core.nFleets
                source = Me.m_Manager.FleetObjectives(i)
                Me.Rows.Insert(i)

                If Not m_bIsMaxByFleetValue Then
                    Me(i, 0) = New EwERowHeaderCell(CStr(i))
                    Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                    Me(i, 2) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSFleetJobCatchValue)
                Else
                    Me(i, 0) = New EwERowHeaderCell(CStr(i))
                    Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                    Me(i, 2) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSFleetJobCatchValue)
                    Me(i, 3) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSFleetTargetProfit)
                End If
            Next

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumns = 1
            Me.FixedColumnWidths = False
            Me.Columns(0).Width = 20
        End Sub

#End Region ' Overrides

    End Class

End Namespace


