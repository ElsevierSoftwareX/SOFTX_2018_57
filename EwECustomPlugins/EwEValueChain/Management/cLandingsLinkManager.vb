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

#End Region ' Imports

Public Class cLandingsLinkManager

    Private m_data As cData = Nothing
    Private m_core As cCore = Nothing

    Public Sub New(data As cData, core As cCore)
        Me.m_data = data
        Me.m_core = core
    End Sub

    Public Sub OnEcopathMessage(msg As cMessage)

        If (msg.Source <> EwEUtils.Core.eCoreComponentType.EcoPath) Then Return
        If (msg.DataType <> EwEUtils.Core.eDataTypes.FleetInput) Then Return
        If (Not msg.HasVariable(eVarNameFlags.Landings)) Then Return

        Me.ManageLinks()

    End Sub

    Public Sub ManageLinks()

        Dim aLinks As cLink() = Nothing
        Dim link As cLinkLandings = Nothing
        Dim fleet As cEcopathFleetInput = Nothing
        Dim group As cEcoPathGroupInput = Nothing
        Dim dtTarget As New Dictionary(Of cUnit, List(Of Integer))
        Dim landings As List(Of Integer) = Nothing
        Dim bDummy As Boolean

        ' Delete all invisible links
        aLinks = Me.m_data.GetLinks(GetType(cLinkLandings), True)
        For Each link In aLinks
            If (Not link.IsVisible) Then
                ' Delete link
                Console.WriteLine("> VC: Link {0} no longer has landings, delete", link)
                Me.m_data.DeleteLink(link)
            End If
        Next link

        ' Add for missing links to producers
        For Each prod As cProducerUnit In Me.m_data.GetUnits(cUnitFactory.eUnitType.Producer)

            ' Get fleet
            fleet = prod.Fleet

            ' Count all existing links by target
            For iLink As Integer = 0 To prod.LinkOutCount - 1
                ' Get link
                link = DirectCast(prod.LinkOut(iLink), cLinkLandings)
                ' Only handle relevant links
                If (link.IsVisible) Then
                    ' Update admin
                    If Not dtTarget.ContainsKey(link.Target) Then
                        dtTarget(link.Target) = New List(Of Integer)
                    End If
                    dtTarget(link.Target).Add(link.Group.Index)
                End If
            Next

            ' Check if has all landings exist for targets
            For Each unit As cUnit In dtTarget.Keys
                ' Get links
                landings = dtTarget(unit)
                ' Check if every landing is represented
                For iGroup As Integer = 1 To Me.m_core.nGroups
                    ' Is Ecopath landing missing a link?
                    If (fleet.Landings(iGroup) > 0) And (landings.IndexOf(iGroup) = -1) Then
                        ' Get group
                        group = Me.m_core.EcoPathGroupInputs(iGroup)
                        ' Create link
                        Console.WriteLine("> VC: Fleet {0}, group {1} missing landings link, added", fleet.Name, group.Name)
                        Me.m_data.CreateLandingsLink(prod, unit, group, bDummy)
                    End If
                Next
            Next

            ' Reset admin
            dtTarget.Clear()

        Next prod

    End Sub

End Class
