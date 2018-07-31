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


Imports EwECore
Imports System.IO
Imports EwEUtils.Utilities
Imports EwEMSEPlugin.HCR_GroupNS

Public Class cTimeFrameRule
    'This object handles the application of a time frame rule.
    'A time frame rule is a rule that overrides the HCR and steps the F used to calc the quota down uniformly from the F during the last year of hindcast
    'to the maximum F (Fmsy) of the harvest control rule over a user specified number of years
    'This object is created for each HCR but only does anything if the number of years it is specified for is > 0

    Private m_F() As Single
    Private m_nTimeStepsInHindcast As Integer
    Private m_EcosimData As cEcosimDatastructures
    Private m_HCR As HCR_Group
    'Private FGreaterThanHCRF As Boolean
    Private m_MSE As cMSE
    Private init_F As Single 'This is the real average F in the last year of the hindcast

    Public Property NYears As Integer

    Public Sub New(ByRef EcosimDatastructures As cEcosimDatastructures, ByRef HCR As HCR_Group, ByRef MSE As cMSE)
        m_EcosimData = EcosimDatastructures
        m_HCR = HCR
        m_nTimeStepsInHindcast = EcosimDatastructures.NTimes
        m_MSE = MSE
    End Sub

    Public Function F(ByVal iCurrentTimeStep As Integer, ByVal iYearProjecting As Integer, ByVal HCR_F As Single) As Single

        If iYearProjecting = 1 Then
            init_F = calcAveragePrevYearF(iCurrentTimeStep)
            If init_F < HCR_F Then init_F = HCR_F
        End If

        'Dim MeanPrevYearF As Double = calcAveragePrevYearF(iCurrentTimeStep)

#If DEBUG Then
        If iYearProjecting = 1 Then
            Dim strmWriter As StreamWriter
            Dim strFile As String = cFileUtils.ToValidFileName("Diagnostics_F_Steps.csv", False)
            strmWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(m_MSE.DataPath, cMSEUtils.eMSEPaths.Results, strFile), True)
            strmWriter.WriteLine(m_MSE.CurrentModelID & "," & m_MSE.currentStrategy.Name & "," & Me.m_HCR.GroupF.Name & "," & init_F)
            cMSEUtils.ReleaseWriter(strmWriter)
        End If
#End If

        'Dim Distance_From_HCR_F As Single = MeanPrevYearF - HCR_F

        Return CSng(init_F - (iYearProjecting / (NYears + 1)) * (init_F - HCR_F))

        'Return HCR_F + ((NYears + 1 - iYearProjecting) / (NYears + 1)) * Distance_From_HCR_F


    End Function

    Public Function CheckValidRule(iYearProjecting As Integer, ByVal HCR_F As Single, ByVal CurrentTimeStep As Integer) As Boolean
        'A time frame rule is valid only if the number of years field for it is >0 and the year into projection is 1 upto that number
        'and also the F during the last year of the hindcast is greater than the Fmsy

        'If NYears > 0 And iYearProjecting >= 1 And iYearProjecting <= NYears And FGreaterThanHCRF(HCR_F, CurrentTimeStep) Then
        If NYears > 0 And iYearProjecting >= 1 And iYearProjecting <= NYears Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function calcAveragePrevYearF(iCurrentTimestep As Integer) As Single

        Dim MeanF As Single
        Dim BiomassAtT As Single
        Dim Q As Single
        Dim GroupIndex As Integer = m_HCR.GroupF.Index

        Debug.Assert(iCurrentTimestep > 12, "TimeFrameRules must have a hind cast period > 12 months. See cTimeFrameRule.calcAverageFLastYearHindCast()")

        'Get the average from the last year of the hindcast
        'iCurrentTimestep is the first time step of the forecast
        For iTimeStep = (iCurrentTimestep - 12) To (iCurrentTimestep - 1)
            BiomassAtT = m_EcosimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, GroupIndex, iTimeStep)

            'time series include density dependent q implicity so dont need to multiply by it here (set Q =1). In all other cases do so.
            If m_EcosimData.FisForced(GroupIndex) Then
                Q = 1
            Else
                Q = m_EcosimData.QmQo(GroupIndex) / (1 + (m_EcosimData.QmQo(GroupIndex) - 1) * BiomassAtT / m_EcosimData.StartBiomass(GroupIndex))
            End If
            MeanF += m_EcosimData.FishRateNo(m_HCR.GroupF.Index, iTimeStep) * Q
        Next
        MeanF /= 12

        Return MeanF

    End Function

    Private Function FGreaterThanHCRF(ByVal HCR_F As Double, ByVal iCurrentTime As Integer) As Boolean

        Dim MeanPrevYearF As Double = calcAveragePrevYearF(iCurrentTime)

        If MeanPrevYearF > HCR_F Then
            Return True
        Else
            Return False
        End If


    End Function

    '    Public Sub calcFsfromTimeFrameRules(iCurrentTimestep As Integer)
    '        'It performs this during the very first timestep of the projection and saves the results to an array for 
    '        ' extraction at the beginning of each year

    '        Dim MeanHindcastF As Double
    '        Dim Fmsy As Double = m_HCR.MaxF
    '        Dim Interval As Double

    '        MeanHindcastF = calcAveragePrevYearF(iCurrentTimestep)

    '#If DEBUG Then
    '        Dim strmWriter As StreamWriter
    '        Dim strFile As String = cFileUtils.ToValidFileName("Diagnostics_F_Steps.csv", False)
    '        strmWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(m_MSE.DataPath, cMSEUtils.eMSEPaths.Results, strFile), True)
    '        strmWriter.WriteLine(m_MSE.CurrentModelID & "," & m_MSE.currentStrategy.Name & "," & Me.m_HCR.GroupF.Name & "," & MeanHindcastF)
    '        strmWriter.Close()
    '        strmWriter.Dispose()
    '#End If

    '        If MeanHindcastF > Fmsy Then
    '            FGreaterThanHCRF = True
    '            If NYears > 0 Then
    '                Interval = (MeanHindcastF - Fmsy) / NYears
    '                ReDim m_F(NYears - 1)
    '                For iYear = 1 To NYears
    '                    m_F(iYear - 1) = MeanHindcastF - iYear * Interval
    '                Next iYear

    '            End If
    '        End If
    '    End Sub


End Class
