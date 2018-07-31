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

''' <summary>
''' Interface for implementing MSY search plugin points that are invoked from the EwE core.
''' </summary>
''' <remarks></remarks>
Public Interface IMSYPlugin
    Inherits IPlugin

    ''' <summary>
    ''' MSY has been initialized
    ''' </summary>
    ''' <param name="MSEDataStructure">MSE data structures</param>
    ''' <param name="EcosimDatastructures">Ecosim data structures</param>
    ''' <remarks></remarks>
    Sub MSYInitialized(ByVal MSEDataStructure As Object, ByVal EcosimDatastructures As Object)

    ''' <summary>
    ''' The MSY variables have been initialized and search is about to start.
    ''' </summary>
    ''' <param name="MSEDataStructure"></param>
    ''' <param name="EcosimDatastructures"></param>
    ''' <remarks></remarks>
    Sub MSYRunStarted(ByVal MSEDataStructure As Object, ByVal EcosimDatastructures As Object)

    ''' <summary>
    ''' MSY search has completed all its iteration and computed effort for all fleets. Interface objects have not been populated at this time.
    ''' </summary>
    ''' <param name="MSYEffortByFleet">MSY effort for all fleets</param>
    ''' <param name="MSYFbyGroup">MSY Fishing mortality for groups</param>
    ''' <remarks></remarks>
    Sub MSYEffortCompleted(ByVal MSYEffortByFleet() As Single, ByVal MSYFbyGroup() As Single)

    ''' <summary>
    ''' MSY search is completed all iterface object have been populated.
    ''' </summary>
    ''' <remarks></remarks>
    Sub MSYRunCompleted()


End Interface
