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
''' Plugin points for begin and end of Ecosim sub timesteps. 
''' </summary>
''' <remarks>
''' Monthly timesteps in Ecosim can be divided into multiple sub timesteps. The number of sub timesteps in set via the cEcosimDatastructures.StepsPerMonth which has a default of one.  
''' This allows a plugin to run Ecosim with more then 12 timesteps per year. Once Ecosim has run it will reset cEcosimDatastructures.StepsPerMonth to its default value of one 
''' all subsequent runs of Ecosim will be on a monthly timestep unless cEcosimDatastructures.StepsPerMonth has been set after the run. 
''' This funtionality is only available via code and has no user interface. 
''' User interface objects e.g. cCore.EcosimGroupOutputs are NOT update for sub timesteps and will not be updated until the end of the monthly timestep.
''' </remarks>
''' ---------------------------------------------------------------------------
Public Interface IEcosimSubTimestepsPlugin
    Inherits IPlugin

    ''' <summary>
    ''' Plugin point called at the start of each sub timestep by Ecosim
    ''' </summary>
    ''' <param name="BiomassAtTimestep">Biomass at the start of the sub timestep</param>
    ''' <param name="TimeInYears">Time of the current sub timestep in years</param>
    ''' <param name="DeltaT">Delta t of the timestep</param>
    ''' <param name="SubTimestepIndex">Index of the current sub timestep 1 to cEcosimDatastructures.StepsPerMonth</param>
    ''' <param name="EcosimDatastructures">Ecosim data structures cEcosimDatastructures cast to an object</param>
    ''' <remarks> 
    ''' In Ecosim the number of sub time steps to run per month is set via the cEcosimDatastructures.StepsPerMonth with a default of one.
    ''' EcosimSubTimeStepBegin() is called once at the start of each Ecosim sub timestep. 
    ''' A sub timestep only runs the numeric integration routine (rk4) and does not call the timestep delegate passed to cCore.RunEcosim(EcoSimTimeStepDelegate) 
    ''' or set any of the Core's Ecosim output objects e.g. cCore.EcosimGroupOutputs will not be update to the values of the new sub timestep. 
    ''' Only values used in the calculation of the sub timestep will be updated(the EcosimDatastructures argument).
    ''' </remarks>
    Sub EcosimSubTimeStepBegin(ByRef BiomassAtTimestep() As Single, ByVal TimeInYears As Single, ByVal DeltaT As Single, ByVal SubTimestepIndex As Integer, ByVal EcosimDatastructures As Object)

    ''' <summary>
    ''' Plugin point called at the end of each sub timestep by Ecosim
    ''' </summary>
    ''' <param name="BiomassAtTimestep">Biomass at the end of the sub timestep</param>
    ''' <param name="TimeInYears">Time of the current sub timestep in years</param>
    ''' <param name="DeltaT">Delta t of the timestep</param>
    ''' <param name="SubTimestepIndex">Index of the current sub timestep 1 to cEcosimDatastructures.StepsPerMonth</param>
    ''' <param name="EcosimDatastructures">Ecosim data structures cEcosimDatastructures cast to an object</param>
    ''' <remarks> 
    ''' In Ecosim the number of sub time steps to run per month is set via the cEcosimDatastructures.StepsPerMonth with a default of one.
    ''' EcosimSubTimeStepEnd() is called once at the end of each Ecosim sub timestep. 
    ''' A sub timestep only runs the numeric integration routine (rk4) and does not call the timestep delegate passed to cCore.RunEcosim(EcoSimTimeStepDelegate) 
    ''' or set any of the Core's Ecosim output objects e.g. cCore.EcosimGroupOutputs will not be update to the values of the new sub timestep. 
    ''' Only values used in the calculation of the sub timestep will be updated(the EcosimDatastructures argument).
    ''' </remarks>
    Sub EcosimSubTimeStepEnd(ByRef BiomassAtTimestep() As Single, ByVal TimeInYears As Single, ByVal DeltaT As Single, ByVal SubTimestepIndex As Integer, ByVal EcosimDatastructures As Object)

End Interface
