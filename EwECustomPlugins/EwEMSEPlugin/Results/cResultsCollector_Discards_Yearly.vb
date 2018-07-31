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

Imports EwEUtils.Core

Public Class cResultsCollector_Discards_Yearly
    Inherits cResultsCollector_Catch

    Public Overrides ReadOnly Property ResultsThroughProjection(iGrp As Integer, iFleet As Integer, iTime As Integer) As Double
        Get
            Dim TempTotalGroupFleetLandingRate As Double = 0
            For iMonth = 1 To 12
                TempTotalGroupFleetLandingRate += m_MSE.DiscardsThroughoutProjection(iGrp, iFleet, (iTime - 1) * 12 + iMonth)
            Next
            TempTotalGroupFleetLandingRate /= 12
            Return TempTotalGroupFleetLandingRate
        End Get
    End Property

    Public Overrides ReadOnly Property Yearly As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property NumberOfTimeRecords As Integer
        Get

            Return m_MSE.NYearsProject
        End Get
    End Property

    Public Overrides ReadOnly Property DataName As String
        Get
            Dim fmt As New EwECore.Style.cCurrencyUnitFormatter("")
            Return "Discard Rate (" & fmt.GetDescriptor(eUnitCurrencyType.WetWeight) & "/year)"
        End Get
    End Property

    'Public Overrides Sub Populate()

    'Dim StrategyIndex = m_MSE.Strategies.IndexOf(m_MSE.currentStrategy) + 1 'Adding 1 to make it a non-zero index
    'For igrp = 1 To m_MSE.Core.nGroups
    '    For iFleet = 1 To m_MSE.Core.nFleets
    '        For iTime = 1 To NumberOfTimeRecords
    '            Me.SetValue(StrategyIndex, igrp, iFleet, iTime) = m_MSE.DiscardsThroughoutProjection(igrp, iFleet, iTime)
    '        Next
    '    Next
    'Next
    'End Sub

    Public Overrides ReadOnly Property FileNamePrefix As String
        Get
            Return "DiscardsYearly_"
        End Get
    End Property

End Class
