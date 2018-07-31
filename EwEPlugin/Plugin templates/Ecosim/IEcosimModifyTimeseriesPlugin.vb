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

''' ---------------------------------------------------------------------------
''' <summary>
''' Plugin points for the initialization of Ecosim data cEcosimDataStructures 
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcosimModifyTimeseriesPlugin
    Inherits IPlugin

    ''' <summary>
    ''' Ecosim is about to initialize for a run. This point allows plug-ins to 
    ''' adjust loaded reference data prior to a run.
    ''' </summary>
    ''' <param name="TimeSeriesDataStructures">cTimeSeriesDataStructures instance.</param>
    ''' <remarks>Call prior to initialization of run data.</remarks>
    Sub EcosimModifyTimeseries(ByVal TimeSeriesDataStructures As Object)

End Interface
