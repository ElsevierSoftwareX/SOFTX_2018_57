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

Imports EwEUtils.Core

#End Region ' Imports

Namespace DataSources

    ''' =======================================================================
    ''' <summary>
    ''' Base interface for implementing a datasource that reads and writes 
    ''' Ecospace data.
    ''' </summary>
    ''' =======================================================================
    Public Interface IEcospaceDatasource
        Inherits IEcopathDataSource

#Region " Generic "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Copies all current Ecospace data to a target datasource.
        ''' </summary>
        ''' <param name="ds">The datasource to copy data to.</param>
        ''' <returns>True if sucessful.</returns>
        ''' -------------------------------------------------------------------
        Overloads Function CopyTo(ByVal ds As IEcospaceDatasource) As Boolean

#End Region ' Generic

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the datasource has unsaved changes for Ecospace.
        ''' </summary>
        ''' <returns>True if the datasource has pending changes for Ecospace.</returns>
        ''' -------------------------------------------------------------------
        Function IsEcospaceModified() As Boolean

#End Region ' Diagnostics

#Region " Scenarios "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads an ecospace scenario from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the scenario to load.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>An implementing class should ensure that this load will cascade to
        ''' load all information pertaining to a scenario.</remarks>
        ''' -------------------------------------------------------------------
        Function LoadEcospaceScenario(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Updates the active ecospace scenario under the given ID in the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the scenario to update the current
        ''' scenario to. This parameter is optional; if left blank the current scenario
        ''' is saved.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SaveEcospaceScenario(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current active Ecospace scenario in the datasource under
        ''' a given database ID.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID to save the current scenario to.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SaveEcospaceScenarioAs(ByVal strScenarioName As String, ByVal strDescription As String, _
                ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace scenario to the datasource.
        ''' </summary>
        ''' <param name="strScenarioName">Name to assign to new scenario.</param>
        ''' <param name="strDescription">Description to assign to new scenario.</param>
        ''' <param name="strAuthor">Author to assign to the new scenario.</param>
        ''' <param name="strContact">Contact info to assign to the new scenario.</param>
        ''' <param name="InRow">Number of rows in new basemap.</param>
        ''' <param name="InCol">Number of columns in new basemap.</param>
        ''' <param name="sOriginLat">Latitude of origin of basemap.</param>
        ''' <param name="sOriginLon">Longitude of origin of basemap.</param>
        ''' <param name="sCellLength">Cell length, in kilometers.</param>
        ''' <param name="iDBID">Database ID assigned to the new scenario.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendEcospaceScenario(ByVal strScenarioName As String, ByVal strDescription As String, _
            ByVal strAuthor As String, ByVal strContact As String, _
            ByVal InRow As Integer, ByVal InCol As Integer, _
            ByVal sOriginLat As Single, ByVal sOriginLon As Single, ByVal sCellLength As Single, _
            ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an ecospace scenario from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the scenario to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcospaceScenario(ByVal iDBID As Integer) As Boolean

#End Region ' Scenarios 

#Region " Basemap "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Resizes the basemap for the current Ecospace scenario.
        ''' </summary>
        ''' <param name="InRow">New number of rows to assign to the basemap.</param>
        ''' <param name="InCol">New number of columns to assign to the basemap.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function ResizeEcospaceBasemap(ByVal InRow As Integer, ByVal InCol As Integer) As Boolean

#End Region ' Basemap

#Region " Habitats "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace habitat to the data source.
        ''' </summary>
        ''' <param name="strHabitatName">Name to assign to new habitat.</param>
        ''' <param name="iHabitatID">Database ID assigned to the new habitat.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddEcospaceHabitat(ByVal strHabitatName As String, ByRef iHabitatID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an ecospace habitat from the data source.
        ''' </summary>
        ''' <param name="iHabitatID">Database ID of the habitat to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcospaceHabitat(ByVal iHabitatID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an Ecospace habitat to a different position in the habitat sequence.
        ''' </summary>
        ''' <param name="iHabitatID">Database ID of the habitat to move.</param>
        ''' <param name="iPosition">The new position of the habitat in the habitat sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function MoveHabitat(ByVal iHabitatID As Integer, ByVal iPosition As Integer) As Boolean

#End Region ' Habitats

#Region " MPAs "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace MPA to the active scenario in the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID assigned to the new MPA.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendEcospaceMPA(ByVal strScenarioName As String, ByVal bMPAMonths() As Boolean, ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an ecospace MPA from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the MPA to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcospaceMPA(ByVal iDBID As Integer) As Boolean

#End Region ' MPAs

#Region " Importance layers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace Importance Layer to the active scenario in the
        ''' datasource.
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="strDescription"></param>
        ''' <param name="sWeight"></param>
        ''' <param name="iDBID">Database ID assigned to the new layer.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendEcospaceImportanceLayer(ByVal strName As String, ByVal strDescription As String, ByVal sWeight As Single, ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace Importance Layer from the active scenario in the
        ''' datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the layer to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcospaceImportanceLayer(ByVal iDBID As Integer) As Boolean

#End Region ' Importance layers

#Region " Driver layers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace driver layer to the datasource.
        ''' </summary>
        ''' <param name="strName">Name to assign to new driver layer.</param>
        ''' <param name="strDescription">Description to assign to new driver layer.</param>
        ''' <param name="strUnits">Units to assign to new driver layer.</param>
        ''' <param name="iDBID">Database ID assigned to the new driver layer.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddEcospaceDriverLayer(ByVal strName As String, ByVal strDescription As String, strUnits As String, ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an ecospace driver layer from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the driver layer to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcospaceDriverLayer(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an ecospace driver layer to a different position in the sequence.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the layer to move.</param>
        ''' <param name="iPosition">The new position of the layer in the layer sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function MoveEcospaceDriverLayer(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean

#End Region ' Driver layers

    End Interface

End Namespace
