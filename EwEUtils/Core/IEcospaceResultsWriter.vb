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

Namespace Core

    ''' <summary>
    ''' Interface for writing Ecospace time step results to file
    ''' </summary>
    Public Interface IEcospaceResultsWriter

        ''' <summary>
        ''' Save time step data to file.
        ''' </summary>
        ''' <param name="SpaceTimeStepResults">cEcospaceTimestep as object containing the data to save.</param>
        Sub WriteResults(ByVal SpaceTimeStepResults As Object)

        ''' <summary>
        ''' Inititialize a writer for writing when an Ecospace run has started.
        ''' </summary>
        ''' <param name="theCore">The core to initialize with.</param>
        Sub Init(ByVal theCore As Object)

        ''' <summary>
        ''' Called when as Ecospace model run is about to start.
        ''' </summary>
        ''' <remarks>This can be used to initialized and file data at the start of a run.</remarks>
        Sub StartWrite()

        ''' <summary>
        ''' Called at the end of an Ecospace model run.
        ''' </summary>
        ''' <remarks>Cleanup after an Ecospace run has completed.</remarks>
        Sub EndWrite()

        ''' <summary>
        ''' Return a human-legible name of the data that this writer produces.
        ''' </summary>
        ReadOnly Property DisplayName() As String

        ''' <summary>
        ''' Get/set whether this writer is allowed to write outputs.
        ''' </summary>
        Property Enabled As Boolean

    End Interface

End Namespace
