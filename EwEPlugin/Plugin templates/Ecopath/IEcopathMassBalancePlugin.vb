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
''' Interface for implementing a plugin point that performs a custom Ecopath 
''' Mass Balance calculation. If provided, this plug-in point will replace
''' the native Mass Balance calculation provided with EwE6.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcopathMassBalancePlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Execute a Mass balance calculation.
    ''' </summary>
    ''' <param name="EcoPathDataStructures">A reference to the Ecopath data 
    ''' structures as defined in the EwE project.</param>
    ''' <param name="eEstimateFor">Enumerated value, stating the purpose of the mass 
    ''' balance calculation.</param>
    ''' <param name="iResult">The result of the mass balance calculation. For 
    ''' possible values refer to the eStatusFlags enumerated type in the EwE project.
    ''' </param>
    ''' <returns>True if a mass-balance calculation has been performed successfully.</returns>
    ''' <remarks>
    ''' This plug-in point is exclusive, meaning that only one IEcopathMassBalancePlugin 
    ''' plug-in is allowed to successfully perform this calculation. The first plug-in
    ''' of this type that successfully executes blocks the execution of any other
    ''' plug-in of this type.</remarks>
    ''' -----------------------------------------------------------------------
    Function EcopathMassBalance(ByVal EcoPathDataStructures As Object, ByVal eEstimateFor As Integer, ByRef iResult As Integer) As Boolean

End Interface
