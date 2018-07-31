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

Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' TimeSeries import class
''' </summary>
''' <remarks>
''' This reminds me so much about programming COBOL that I'm downright terrified...
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cTimeSeriesImport
    Inherits cTimeSeries

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="iNumYears">Number of years in this time series.</param>
    ''' <param name="timeSeriesType"><see cref="eTimeSeriesType">Type</see> of this time series.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub New(ByVal iNumYears As Integer, ByVal timeSeriesType As eTimeSeriesType)
        MyBase.New(Nothing, -1)
        Me.m_timeSeriesType = timeSeriesType
        Me.ResizeData(iNumYears)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to prevent this type of time series to interact with the 
    ''' EwE core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Update() As Boolean
        ' Suppress this
        Return True
    End Function

End Class
