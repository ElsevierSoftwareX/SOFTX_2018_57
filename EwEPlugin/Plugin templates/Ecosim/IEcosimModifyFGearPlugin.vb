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
''' Plugin Point to modify Ecosim Fishing Effort during a MSE or Fishing Policy Search.
''' </summary>
''' <remarks>This will not modify effort during a normal Ecosim run.</remarks>
Public Interface IEcosimModifyFGearPlugin
    Inherits IPlugin

    ''' <summary>
    ''' Method that gets called when a Fishing Policy or MSE search is modifying Fishing Effort.
    ''' </summary>
    ''' <param name="FGear">Array of Relative Fishing Effort dimensioned by fleet for the current timestep.</param>
    ''' <param name="BB">Array of Biomass by group for the current timestep</param>
    ''' <param name="EcosimDataStructures">Reference to the current EcosimDataStructures passed as an object.</param>
    ''' <param name="CurrentTimeStepIndex">Current timestep index.</param>
    ''' <remarks>At this time this only changes effort during a search there is no easy way to change effort during a normal run. </remarks>
    Sub EcosimModifyFGear(ByVal FGear() As Single, ByVal BB() As Single, ByVal EcosimDataStructures As Object, ByVal CurrentTimeStepIndex As Integer)

End Interface
