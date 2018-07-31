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
''' Interface for a plug-in that is invoked when the Ecosim model is about to
''' start computing a time step, after all instances of <see cref="IEcosimBeginTimestepPlugin"/>
''' points have been called.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcosimBeginTimestepPostPlugin
    Inherits IPlugin

    ''' <summary>
    ''' Ecosim is about to compute a time step, and all instances of <see cref="IEcosimBeginTimestepPlugin"/>
    ''' have had the opportunity to run.
    ''' </summary>
    ''' <param name="BiomassAtTimestep">The biomasses at the beginning at the time step.</param>
    ''' <param name="EcosimDatastructures">The Ecosim data structures that you can poke around in.</param>
    ''' <param name="iTime">The time step that will be executed.</param>
    Sub EcosimBeginTimeStepPost(ByRef BiomassAtTimestep() As Single, ByVal EcosimDatastructures As Object, ByVal iTime As Integer)

End Interface
