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
''' Interface for implementing plugin points that are invoked from the EwE
''' searches
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface ISearchPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search has been initialized by the core.
    ''' </summary>
    ''' <param name="SearchDatastructures">cSearchDataStructures</param>
    ''' -----------------------------------------------------------------------
    Sub SearchInitialized(ByVal SearchDatastructures As Object)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The user selected minimization routine has made a call to the function 
    ''' being minimized.
    ''' </summary>
    ''' <param name="SearchDatastructures">cSearchDataStructures</param>
    ''' -----------------------------------------------------------------------
    Sub PostRunSearchResults(ByVal SearchDatastructures As Object)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search iteration are about to start.
    ''' </summary>
    ''' <remarks>
    ''' The minimization is about to run for the user selected number of 
    ''' iteration. 
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Sub SearchIterationsStarting()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search is completed.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Sub SearchCompleted(ByVal SearchDatastructures As Object)

End Interface
