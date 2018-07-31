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

#Region " Imports "

Option Strict On

#End Region ' Imports

Namespace DataSources

    ''' =======================================================================
    ''' <summary>
    ''' Base interface for implementing a datasource that reads and writes 
    ''' contaminant tracing data.
    ''' </summary>
    ''' =======================================================================
    Public Interface IEcotracerDatasource
        Inherits IEcopathDataSource

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the datasource has unsaved changes for Ecotracer.
        ''' </summary>
        ''' <returns>True if the datasource has pending changes for Ecotracer.</returns>
        ''' -------------------------------------------------------------------
        Function IsEcotracerModified() As Boolean

#End Region ' Diagnostics

#Region " Ecotracer Scenarios "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads an Ecotracer scenario from the datasource.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to load.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>An implementing class should ensure that this load will cascade to
        ''' load all information pertaining to a scenario.</remarks>
        ''' -------------------------------------------------------------------
        Function LoadEcotracerScenario(ByVal iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current active Ecotracer scenario in the datasource under
        ''' a given database ID.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID to save the current scenario to.
        ''' If this parameter is left blank, the current scenario is saved
        ''' under its own database ID.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SaveEcotracerScenario(ByVal iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an Ecotracer scenario to the datasource.
        ''' </summary>
        ''' <param name="strScenarioName">Name to assign to new scenario.</param>
        ''' <param name="strDescription">Description to assign to new scenario.</param>
        ''' <param name="strAuthor">Author to assign to the new scenario.</param>
        ''' <param name="strContact">Contact info to assign to the new scenario.</param>
        ''' <param name="iScenarioID">Database ID assigned to the new scenario.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendEcotracerScenario(ByVal strScenarioName As String, ByVal strDescription As String, _
                ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an Ecotracer scenario from the datasource.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcotracerScenario(ByVal iScenarioID As Integer) As Boolean

#End Region ' Ecotracer scenarios 

    End Interface

End Namespace
