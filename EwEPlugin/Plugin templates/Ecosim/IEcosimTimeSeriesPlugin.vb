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
''' Interface for implementing plugin points that are invoked whenever major time
''' series events occur.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcosimTimeSeriesPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when Ecosim has loaded time series.
    ''' </summary>
    ''' <remarks>This plugin point is non-exclusive; each implementation 
    ''' of this plugin point will be called.</remarks>
    ''' -----------------------------------------------------------------------
    Sub TimeSeriesLoaded()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when Ecosim has unloaded time series.
    ''' </summary>
    ''' <remarks>This plugin point is non-exclusive; each implementation 
    ''' of this plugin point will be called.</remarks>
    ''' -----------------------------------------------------------------------
    Sub TimeSeriesClosed()

End Interface
