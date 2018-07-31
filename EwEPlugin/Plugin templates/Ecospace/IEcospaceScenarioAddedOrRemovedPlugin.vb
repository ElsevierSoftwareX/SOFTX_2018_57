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
''' Interface for responding to adding or removing an Ecospace scenario.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcospaceScenarioAddedOrRemovedPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when an Ecospace scenario has been added.
    ''' </summary>
    ''' <param name="dataSource">A reference to the EwE data source to which
    ''' the scenario was added.</param>
    ''' <param name="scenarioID">The database ID of the newly created Ecospace scenario.</param>
    ''' <remarks>This plugin point is non-exclusive; each implementation 
    ''' of this plugin point will be called.</remarks>
    ''' -----------------------------------------------------------------------
    Sub EcospaceScenarioAdded(dataSource As Object, scenarioID As Integer)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when an Ecospace scenario has been removed.
    ''' </summary>
    ''' <param name="dataSource">A reference to the EwE data source from which
    ''' the scenario was removed.</param>
    ''' <param name="scenarioID">The database ID of the newly created Ecospace scenario.</param>
    ''' <remarks>This plugin point is non-exclusive; each implementation 
    ''' of this plugin point will be called.</remarks>
    ''' -----------------------------------------------------------------------
    Sub EcospaceScenarioRemoved(dataSource As Object, scenarioID As Integer)

End Interface
