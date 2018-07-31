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
    ''' Ecosim data.
    ''' </summary>
    ''' =======================================================================
    Public Interface IEcosimDatasource
        Inherits IEcopathDataSource

#Region " Generic "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Copies all current Ecosim data to a target datasource.
        ''' </summary>
        ''' <param name="ds">The datasource to copy data to.</param>
        ''' <returns>True if sucessful.</returns>
        ''' -------------------------------------------------------------------
        Overloads Function CopyTo(ByVal ds As IEcosimDatasource) As Boolean

#End Region ' Generic

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the datasource has unsaved changes for Ecosim.
        ''' </summary>
        ''' <returns>True if the datasource has pending changes for Ecosim.</returns>
        ''' -------------------------------------------------------------------
        Function IsEcosimModified() As Boolean

#End Region ' Diagnostics

#Region " Ecosim Scenarios "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads an ecosim scenario from the datasource.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to load.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>An implementing class should ensure that this load will cascade to
        ''' load all information pertaining to a scenario.</remarks>
        ''' -------------------------------------------------------------------
        Function LoadEcosimScenario(ByVal iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current active Ecosim scenario in the datasource under
        ''' a given database ID.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID to save the current scenario to.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SaveEcosimScenario(ByVal iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current active Ecosim scenario in the datasource under
        ''' a given database ID.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID to save the current scenario to.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SaveEcosimScenarioAs(ByVal strScenarioName As String, ByVal strDescription As String, _
                ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a new and empty ecosim scenario to the datasource.
        ''' </summary>
        ''' <param name="strScenarioName">Name to assign to new scenario.</param>
        ''' <param name="strDescription">Description to assign to new scenario.</param>
        ''' <param name="strAuthor">Author to assign to the new scenario.</param>
        ''' <param name="strContact">Contact info to assign to the new scenario.</param>
        ''' <param name="iScenarioID">Database ID assigned to the new scenario.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendEcosimScenario(ByVal strScenarioName As String, ByVal strDescription As String, _
                ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an ecosim scenario from the datasource.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveEcosimScenario(ByVal iScenarioID As Integer) As Boolean

#End Region ' Ecosim scenarios 

#Region " Forcing and Mediation shapes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Appends a forcing shape to the datasource.
        ''' </summary>
        ''' <param name="strShapeName">Name to assign to new shape.</param>
        ''' <param name="shapeDataType"><see cref="eDataTypes">Type of the shape</see> to add.</param>
        ''' <param name="iDBID">Database ID assigned to the new shape.</param>
        ''' <param name="asData">Shape point data.</param>
        ''' <param name="functionType">Primitive function type shape was created from.</param>
        ''' <param name="parms">Parameters for the <paramref name="functiontype"/> </param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendShape(ByVal strShapeName As String,
                             ByVal shapeDataType As eDataTypes,
                             ByRef iDBID As Integer,
                             ByVal asData As Single(),
                             ByVal functionType As Long,
                             ByVal parms As Single()) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Deletes a forcing shape from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the shape to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>Note that an implementing datasource will have to ensure the
        ''' shape is removed from the correct scenario.</remarks>
        ''' -------------------------------------------------------------------
        Function RemoveShape(ByVal iDBID As Integer) As Boolean

#End Region ' Forcing and Mediation shapes

#Region " Time Series "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load all time series for a given dataset.
        ''' </summary>
        ''' <param name="iDataset">Index of the dataset to load.</param>
        ''' <returns>True if successful</returns>
        ''' -------------------------------------------------------------------
        Function LoadTimeSeriesDataset(ByVal iDataset As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an time series dataset to the datasource.
        ''' </summary>
        ''' <param name="strDatasetName">Name to assign to new dataset.</param>
        ''' <param name="strDescription">Description to assign to new dataset.</param>
        ''' <param name="strAuthor">Author to assign to the new dataset.</param>
        ''' <param name="strContact">Contact info to assign to the new dataset.</param>
        ''' <param name="iFirstYear">First year of the dataset.</param>
        ''' <param name="iNumPoints">Number of data points in the dataset.</param>
        ''' <param name="interval"><see cref="eTSDataSetInterval">Interval</see>
        ''' between two points in the dataset.</param>
        ''' <param name="iDatasetID">Database ID assigned to the new dataset.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendTimeSeriesDataset(ByVal strDatasetName As String, _
                                         ByVal strDescription As String, _
                                         ByVal strAuthor As String, _
                                         ByVal strContact As String, _
                                         ByVal iFirstYear As Integer, _
                                         ByVal iNumPoints As Integer, _
                                         ByVal interval As eTSDataSetInterval, _
                                         ByRef iDatasetID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes all time series belonging to a specific dataset from the datasource.
        ''' </summary>
        ''' <param name="iDataset">Index of the dataset to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveTimeSeriesDataset(ByVal iDataset As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Import a complete <see cref="cTimeSeriesImport">cTimeSeriesImport</see>
        ''' instance into the datasource.
        ''' </summary>
        ''' <param name="ts">The time series data to import.</param>
        ''' <param name="iDataset">Index of the dataset to add time series to.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function ImportTimeSeries(ByVal ts As cTimeSeriesImport, ByVal iDataset As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a time series to the datasource.
        ''' </summary>
        ''' <param name="strName">Name of the new Time Series to add.</param>
        ''' <param name="timeSeriesType"><see cref="eTimeSeriesType">Type</see> of the time series.</param>
        ''' <param name="asValues">Initial values to set in the TS.</param>
        ''' <param name="iDBID">Database ID assigned to the new TS.</param>
        ''' <param name="iPool">Target index</param>
        ''' <param name="iPoolSec">Secundary target index, if applicable.</param>
        ''' <param name="sWeight"></param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AppendTimeSeries(ByVal strName As String,
                                  ByVal iPool As Integer, ByVal iPoolSec As Integer,
                                  ByVal timeSeriesType As eTimeSeriesType,
                                  ByVal sWeight As Single, ByVal asValues() As Single,
                                  ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a time series from the datasource.
        ''' </summary>
        ''' <param name="iTimeSeriesID">Database ID of the time series to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveTimeSeries(ByVal iTimeSeriesID As Integer) As Boolean

#End Region ' Time series

    End Interface

End Namespace
