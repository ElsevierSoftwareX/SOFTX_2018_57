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

Imports EwEUtils.Core

''' <summary>
''' Interface for defining Ecospace Environmental Input maps
''' </summary>
''' <remarks></remarks>
Public Interface IEnviroInputData

    ''' <summary>
    ''' Return the value of the map as a function of the applied Response Function
    ''' </summary>
    ''' <param name="iGroup">Index of the Group that this Response is for</param>
    ''' <param name="iRow">Row of the map</param>
    ''' <param name="iCol">Column of the map</param>
    Function ResponseFunction(ByVal iGroup As Integer, ByVal iRow As Integer, ByVal iCol As Integer) As Single

    ''' <summary>
    ''' Initialize the map with the cMediationDataStructures containing all the available response functions and cEcospaceDataStructures
    ''' </summary>
    ''' <param name="MediationData">cMediationDataStructures that contains the Response Function (mediation functions) that can be used by this Map</param>
    ''' <param name="SpaceData"></param>
    Function Init(ByVal MediationData As cMediationDataStructures, ByVal SpaceData As cEcospaceDataStructures) As Boolean

    ''' <summary>
    ''' Get or Set the index of the Response function applied to a Group
    ''' </summary>
    ''' <param name="iGroup">One-based index of the Group that the response function is applied to</param>
    ''' <param name="bUpdateMaps">Optional flag to suppress (possibly expensive) map updates</param>
    ''' <value></value>
    ''' <returns>Index of a response function.</returns>
    ''' <remarks>
    ''' <code>
    ''' dim ResponseIndex as integer
    ''' dim iGroup as integer
    ''' iGroup = 1
    ''' 'Set the Response function index for iGroup
    '''  IEnviroInputMap.ResponseIndexForGroup(iGroup) = 2
    ''' 'Get the Response functon index for iGroup
    ''' ResponseIndex = IEnviroInputMap.ResponseIndexForGroup(iGroup) 
    ''' </code>
    ''' </remarks>
    Property ResponseIndexForGroup(ByVal iGroup As Integer, Optional ByVal bUpdateMaps As Boolean = True) As Integer

    ''' <summary>
    ''' Response function for Ecosim
    ''' </summary>
    ''' <param name="iGroup"></param>
    ''' <param name="iTimeStep"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function ResponseFunction(ByVal iGroup As Integer, iTimeStep As Integer) As Single

    ''' <summary>
    ''' Initialize the map with the cMediationDataStructures containing all the available response functions and cEcospaceDataStructures
    ''' </summary>
    ''' <param name="MediationData">cMediationDataStructures that contains the Response Function (mediation functions) that can be used by this Map</param>
    ''' <param name="EcosimData"></param>
    Function Init(ByVal MediationData As cMediationDataStructures, ByVal EcosimData As cEcosimDatastructures) As Boolean


    ''' <summary>
    ''' Max value of the map
    ''' </summary>
    ReadOnly Property Max() As Single

    ''' <summary>
    ''' Minimum value of the map
    ''' </summary>
    ReadOnly Property Min() As Single

    ''' <summary>
    ''' Mean value of the map
    ''' </summary>
    ReadOnly Property Mean() As Single

    ''' <summary>
    ''' Histogram of the map values
    ''' </summary>
    ''' <remarks>
    ''' Values in the Histogram will be normalized.
    ''' Re-computed on each call to Histogram.
    ''' </remarks>
    Function Histogram() As Drawing.PointF()

    ''' <summary>
    ''' Width of a histogram bin
    ''' </summary>
    ReadOnly Property HistogramBinWidth() As Single

    ''' <summary>
    ''' Updates the map stats on the underlying data
    ''' </summary>
    ''' <remarks>Caluculates Min, Max and Mean</remarks>
    Function Update() As Boolean

    ''' <summary>
    ''' Set the cMapResponseInteractionManager that this map uses
    ''' </summary>
    ''' <param name="theManager"></param>
    Sub SetManager(ByVal theManager As IEnvironmentalResponseManager)

    Property IsDriverActive As Boolean

    ReadOnly Property Name As String


End Interface
