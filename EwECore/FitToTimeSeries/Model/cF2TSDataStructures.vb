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

Option Strict On

Public Class cF2TSDataStructures

    Public bVulnerabilitySearch As Boolean = True
    Public bCatchAnomaly As Boolean = False
    Public bAnomalySearch As Boolean = False
    Public FirstYear As Integer = 1
    Public LastYear As Integer = 1
    Public VulnerabilityVariance As Single = 10.0!
    Public PPVariance As Single = 0.1!
    Public iCatchAnomalySearchShapeNumber As Integer = 0
    Public nNumSplinePoints As Integer = 0
    Public RunSilent As Boolean = False

    ''' <summary>
    ''' Number of AIC parameters
    ''' </summary>
    Public nAICPars As Integer

    ''' <summary>
    ''' Number of AIC data points
    ''' </summary>
    Public nAICData As Integer

    ''' <summary>
    ''' Akaike Information Criteria for the last run
    ''' </summary>
    Public AIC As Single

    Public UseDefaultV As Boolean = True

End Class
