﻿' ===============================================================================
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

Imports EwECore
Imports EwEUtils.Utilities

Public Class cResultsCollector_PredationMortality_PreyOnly
    Inherits cResultsCollector_1DArray

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

        For iPrey = 1 To m_MSE.Core.nGroups
            For iTime = 1 To NumberOfTimeRecords
                Me.SetValue(StrategyIndex, iPrey, iTime) = m_MSE.EcosimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.PredMort, iPrey, iTime)
            Next
        Next

    End Sub

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
            Return "PredationMortalityPreyOnly_"
        End Get
    End Property

    Public Overrides ReadOnly Property Dim_Name As String
        Get
            Return "Prey"
        End Get
    End Property

    Public Overrides ReadOnly Property nElements As Integer
        Get
            Return m_MSE.Core.nGroups
        End Get
    End Property

    Public Overrides ReadOnly Property ElementName(iElement As Integer) As String
        Get
            Return m_MSE.Core.EcoPathGroupInputs(iElement).Name
        End Get
    End Property

    Public Overrides ReadOnly Property GetValue_Formatted4CSV(iStrategy As Integer, iElement As Integer, iTime As Integer) As Object
        Get
            Return cStringUtils.FormatNumber(GetValue(iStrategy, iElement, iTime))
        End Get
    End Property
End Class