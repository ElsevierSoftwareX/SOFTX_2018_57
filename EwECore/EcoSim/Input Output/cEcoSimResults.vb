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

''' <summary>
''' Results from the last completed EcoSim Time Step
''' Passed out by EcoSim via the EcoSimTimeStepDelegate(iTime,Results) delegate
''' </summary>
''' <remarks></remarks>
Public Class cEcoSimResults

    ''' <summary>
    ''' Number of groups
    ''' </summary>
    Public nGroups As Integer
    ''' <summary>
    ''' Number of fleets
    ''' </summary>
    Public nFleets As Integer

    ''' <summary>
    ''' Timestep counter at the currrent model timestep
    ''' </summary>
    Public CurrentT As Long

    ''' <summary>
    ''' Relative biomass by group
    ''' </summary>
    Public Biomass() As Single

    Public TLCatch As Single
    Public FIB As Single

    ''' <summary>
    ''' Fishing effort used for the current timestep by fleet
    ''' </summary>
    Public Effort() As Single

    ''' <summary>
    ''' Catch by group relative to Ecopath base
    ''' </summary>
    ''' <remarks></remarks>
    Public Yield() As Single

    ''' <summary>Catch {group x fleet}</summary>
    Public BCatch(,) As Single ' by group, by fleet

    ''' <summary>Landings discards not included {group x fleet}</summary>
    Public Landings(,) As Single

    ''' <summary>
    ''' Number of multi stanza groups. Used by BStock(nStanza,nMaxLifeStages) and BRecruitment(nStanza,nMaxLifeStages)
    ''' </summary>
    Public nStanza As Integer

    ''' <summary>
    ''' Maximum number of life stages across all the multi stanza groups
    ''' </summary>
    ''' <remarks></remarks>
    Public nMaxLifeStages As Integer

    ''' <summary>
    ''' Relative biomass of stock, BStock(nStanza,nMaxLifeStages)
    ''' </summary>
    ''' <remarks>X axis in EwE5 sr plot</remarks>
    Public BStock(,) As Single

    ''' <summary>
    ''' Relative biomass of recruits, BRecruitment(nStanza,nMaxLifeStages)
    ''' </summary>
    ''' <remarks>Y axis in EwE5 sr plot</remarks>
    Public BRecruitment(,) As Single

    Private m_hasSRData(,) As Boolean
    Private m_hasData As Boolean

    ''' <summary>
    ''' Is there stock recruitment data for Multi stanza , Life stage pair
    ''' </summary>
    ''' <remarks></remarks>
    Public Property hasSRData(ByVal iMulitStanzaGroup As Integer, ByVal iLifeStage As Integer) As Boolean
        Get
            Return m_hasSRData(iMulitStanzaGroup, iLifeStage)
        End Get
        Set(ByVal value As Boolean)
            m_hasSRData(iMulitStanzaGroup, iLifeStage) = value
        End Set
    End Property


    ''' <summary>
    ''' Is there stock recruitment data for this time step
    ''' </summary>
    Public Property hasSRData() As Boolean
        Get
            Return m_hasData
        End Get
        Set(ByVal value As Boolean)
            m_hasData = value
        End Set
    End Property

    Public Sub New(ByVal nGroups As Integer, ByVal nStanzas As Integer, ByVal nMaxLifeStages As Integer, ByVal nFleets As Integer)

        Me.nGroups = nGroups
        Me.nStanza = nStanzas
        Me.nFleets = nFleets
        Me.nMaxLifeStages = nMaxLifeStages

        ReDim Biomass(nGroups)
        ReDim Yield(nGroups)
        ReDim BCatch(nGroups, nFleets)
        ReDim Landings(nGroups, nFleets)

        ReDim m_hasSRData(nStanza, nMaxLifeStages)
        ReDim BStock(nStanza, nMaxLifeStages)
        ReDim BRecruitment(nStanza, nMaxLifeStages)
        ReDim Effort(nFleets)

    End Sub


    Public Sub clear()

        Array.Clear(Me.Biomass, 0, nGroups)
        Array.Clear(Me.Yield, 0, nGroups)

        Array.Clear(Me.BCatch, 0, BCatch.Length)

        Array.Clear(Me.Landings, 0, Landings.Length)
        Array.Clear(Me.m_hasSRData, 0, m_hasSRData.Length)
        Array.Clear(Me.BRecruitment, 0, BRecruitment.Length)
        Array.Clear(Me.BStock, 0, BStock.Length)
        Array.Clear(Me.Effort, 0, nFleets)

    End Sub

    ''' <summary>
    ''' Return a deep copy of the current results.
    ''' </summary>
    ''' <returns></returns>
    Public Function Clone() As cEcoSimResults

        Dim copy As New cEcoSimResults(Me.nGroups, Me.nStanza, Me.nMaxLifeStages, Me.nFleets)

        Array.Copy(Me.Biomass, copy.Biomass, Me.Biomass.Length)
        Array.Copy(Me.Yield, copy.Yield, Me.Yield.Length)
        Array.Copy(Me.BCatch, copy.BCatch, Me.BCatch.Length)
        Array.Copy(Me.Landings, copy.Landings, Me.Landings.Length)
        Array.Copy(Me.m_hasSRData, copy.m_hasSRData, Me.m_hasSRData.Length)
        Array.Copy(Me.BStock, copy.BStock, Me.BStock.Length)
        Array.Copy(Me.BRecruitment, copy.BRecruitment, Me.BRecruitment.Length)
        Array.Copy(Me.Effort, copy.Effort, Me.Effort.Length)

        Return copy

    End Function

End Class
