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

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for extending the Ecospace events before and after a spatial layer 
''' receives content through the spatial-temporal data framework.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcospaceLayerChangePlugin
    Inherits IPlugin

    ''' <summary>
    ''' An Ecospace layer is about to receive data through the spatial-temporal
    ''' data framework. This call allows plug-ins to gather the data in the 
    ''' layer before it will be altered.
    ''' </summary>
    ''' <param name="iTime">Cumulative time step.</param>
    ''' <param name="dt">Absolute time for the time step.</param>
    ''' <param name="layer">The Ecospace basemap layer that is about to receive data.</param>
    Sub EcospaceBeginLayerChange(ByVal iTime As Integer, dt As Date, layer As Object)

    ''' <summary>
    ''' An Ecospace layer has just received data through the spatial-temporal
    ''' data framework. This call allows plug-ins to gather the data in the 
    ''' layer after it has been altered and integrated into Ecospace.
    ''' </summary>
    ''' <param name="iTime">Cumulative time step.</param>
    ''' <param name="dt">Absolute time for the time step.</param>
    ''' <param name="layer">The Ecospace basemap layer that received data.</param>
    Sub EcospaceEndLayerChange(ByVal iTime As Integer, dt As Date, layer As Object)

End Interface
