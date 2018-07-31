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

''' ===========================================================================
''' <summary>
''' Interface for a plug-in that is invoked when the EwE Core loads the three main
''' models Ecopath, Ecosim and Ecospace. Plug-in points in this interface
''' will allow an implementing plug-in to obtain a reference to the three models.
''' </summary>
''' ===========================================================================
Public Interface ICorePlugin
    Inherits IPlugin

    ''' <summary>
    ''' The core has loaded a model and initialized its internal data
    ''' </summary>
    ''' <param name="objEcoPath">The Ecopath model</param>
    ''' <param name="objEcoSim">The Ecosim model</param>
    ''' <param name="objEcoSpace">The Ecospace model</param>
    Sub CoreInitialized(ByRef objEcoPath As Object, ByRef objEcoSim As Object, ByRef objEcoSpace As Object)

End Interface
