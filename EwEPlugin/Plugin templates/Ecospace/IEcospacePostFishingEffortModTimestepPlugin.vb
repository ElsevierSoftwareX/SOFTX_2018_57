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
''' Interface for extending the Ecospace fishing effort logic. Plug-ins of this
''' type are invoked as soon as Ecospace fishing effort has been calculated.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcospacePostFishingEffortModTimestepPlugin
    Inherits IPlugin

    Sub EcospacePostFishingEffortModTimestep(ByVal EcospaceDatastructures As Object, ByVal iTime As Integer)

End Interface
