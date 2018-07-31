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
''' Interface for extending the Ecospace initialzation logic. Plug-ins of this
''' type are invoked as soon as all Ecospace data is loaded in the EwE Core.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcospaceInitializedPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when Ecospace has loaded a new scenario, is
    ''' initialized, and is ready to be used.
    ''' </summary>
    ''' <param name="EcospaceDatastructures">The ecospace datastructures that 
    ''' just received new scenario data.</param>
    ''' -----------------------------------------------------------------------
    Sub EcospaceInitialized(ByVal EcospaceDatastructures As Object)

End Interface
