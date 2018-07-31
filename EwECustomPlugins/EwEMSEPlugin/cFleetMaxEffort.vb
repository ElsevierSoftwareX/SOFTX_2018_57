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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Option Strict On
Option Explicit On

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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================

' The cFleetMaxEffort object holds all the data and methods for decaying maximum efforts
' param iFleet
' param final_hindcast_effort is the effort at the very last timestep of the hindcast
' param m_max_percentage_change_in_max_effort is the maximum percentage that the max effort can change each year and is taken from the ChangesInEffortLimits.csv file
Public Class cFleetMaxEffort

    Private m_iFleet As Integer 'index for the fleet
    Private m_max_effort As Single 'the current max effort for a given fleet
    Private m_max_percentage_change_in_max_effort As Single 'this is taken from the ChangesInEffortLimits.csv file
    Private m_max_effort_type As eMaxEffortType 'whether it be the newer decaying type of the older proportion of last year method

    Enum eMaxEffortType As Integer
        Decaying
        LastYearProp
    End Enum

    ' returns the max effort for a fleet in the current year
    Public ReadOnly Property MaxEffort As Single
        Get
            Return m_max_effort
        End Get
    End Property

    Public Sub New(iFleet As Integer, final_hindcast_effort As Single, max_percentage_change_in_max_effort As Single, decaying_max_effort As Boolean)

        m_iFleet = iFleet
        m_max_effort = final_hindcast_effort
        m_max_percentage_change_in_max_effort = max_percentage_change_in_max_effort

        If decaying_max_effort Then
            m_max_effort_type = eMaxEffortType.Decaying
        ElseIf Not decaying_max_effort Then
            m_max_effort_type = eMaxEffortType.LastYearProp
        End If

    End Sub

    'Updates the max effort
    ' param end_previous_year_effort is the effort at the last time step for the previous year
    Public Sub UpdateLimit(end_previous_year_effort As Single)

        Select Case m_max_effort_type
            Case eMaxEffortType.Decaying
                UpdateUsingDecaying(end_previous_year_effort)
            Case eMaxEffortType.LastYearProp
                UpdateUsingProp(end_previous_year_effort)
        End Select

    End Sub

    ' Calculates the max effort using the improved decaying max effort method
    Private Sub UpdateUsingDecaying(end_previous_year_effort As Single)

        Dim max_reduction_in_max_effort As Single
        Dim min_increase_in_max_effort As Single

        max_reduction_in_max_effort = m_max_effort - m_max_effort * m_max_percentage_change_in_max_effort
        min_increase_in_max_effort = end_previous_year_effort + end_previous_year_effort * m_max_percentage_change_in_max_effort

        m_max_effort = Math.Max(max_reduction_in_max_effort, min_increase_in_max_effort)

    End Sub

    ' Calculates the max effort as a proportion of the final effort from the previous year
    Private Sub UpdateUsingProp(end_previous_year_effort As Single)
        m_max_effort = end_previous_year_effort * (1 + m_max_percentage_change_in_max_effort)
    End Sub

End Class
