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

Option Strict Off
Imports EwECore

Public Class cResultsCollector_PredationMortality
    Inherits cResultsCollector_2DArray_Group_Group

    Public Overrides ReadOnly Property DataName As String
        Get
            Return "PredationMortality"
        End Get
    End Property

    Protected Overrides ReadOnly Property DefaultValue As Object
        Get
            Return 0
        End Get
    End Property

    Public Overrides Sub Populate()

        Dim StrategyIndex = m_MSE.Strategies.IndexOf(m_MSE.currentStrategy) + 1 'Adding 1 to make it a non-zero index
        Dim Temp_PredationMortality As Single

        For iPred = 1 To m_MSE.Core.nGroups
            For iPrey = 1 To m_MSE.Core.nGroups
                For iTime = 1 To NumberOfTimeRecords
                    Temp_PredationMortality = m_MSE.EcosimData.PredPreyResultsOverTime(cEcosimDatastructures.eEcosimPreyPredResults.Consumption, iPrey, iPred, iTime) /
                                                m_MSE.EcosimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iPrey, iTime)
                    Me.SetValue(StrategyIndex, iPred, iPrey, iTime) = Temp_PredationMortality
                    Me.SetValue(StrategyIndex, iPred, 0, iTime) = Me.GetValue(StrategyIndex, iPred, 0, iTime) + Temp_PredationMortality 'Summing across prey
                    Me.SetValue(StrategyIndex, 0, iPrey, iTime) = Me.GetValue(StrategyIndex, 0, iPrey, iTime) + Temp_PredationMortality 'Summing across predators
                    Me.SetValue(StrategyIndex, 0, 0, iTime) = Me.GetValue(StrategyIndex, 0, 0, iTime) + Temp_PredationMortality ' summ across both predators and prey
                Next
            Next
        Next

    End Sub

    Public Overrides ReadOnly Property TotalAcrossPred As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property TotalAcrossPrey As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property NumberOfTimeRecords As Integer
        Get
            Return m_MSE.NYearsProject * m_MSE.EcosimData.NumStepsPerYear
        End Get
    End Property

    Public Overrides ReadOnly Property Yearly As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property FileNamePrefix As String
        Get
            Return "PredationMortality_"
        End Get
    End Property

End Class
