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
''' Plugin Point to modify effort during an Ecosim run 
''' </summary>
''' <remarks></remarks>
Public Interface IEcosimModifyEffort
    Inherits IPlugin

    ''' <summary>
    ''' Call at each Ecosim timestep before fishing mortality is set. If the bEffortModified = True then a new fishing mortality will be computed from Effort().
    ''' </summary>
    ''' <param name="bEffortModified">
    ''' If True then fishing mortality will be computed from effort. 
    ''' If False then Effort() will be ignored and fishing mortality will not be modified. 
    ''' </param>
    ''' <param name="Effort">Fishing effort at the current timestep. Alter this and set bEffortModified = True to change fishing effort. </param>
    ''' <param name="BB">Biomass at the current timestep</param>
    ''' <param name="iTimeIndex">Time index of the current timestep</param>
    ''' <param name="iYearIndex">Year index of the current timestep</param>
    ''' <param name="EcosimDataStructures">cEcosimDataStructures as an Object</param>
    ''' <remarks></remarks>
    Sub EcosimModifyEffort(ByRef bEffortModified As Boolean, ByVal Effort() As Single, ByVal BB() As Single, ByVal iTimeIndex As Integer, iYearIndex As Integer, ByVal EcosimDataStructures As Object)

End Interface


