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

Namespace Core

    Public Interface IEcospaceLayerManager

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get managed layers.
        ''' </summary>
        ''' <param name="varName">The <see cref="eVarNameFlags">variable</see> to get layers for.
        ''' If <see cref="eVarNameFlags.NotSet"/> is provided this manager will return
        ''' all maintained layers.</param>
        ''' <returns>An array of all managed layers.</returns>
        ''' -----------------------------------------------------------------------
        Function Layers(Optional ByVal varName As eVarNameFlags = eVarNameFlags.NotSet) As cEcospaceLayer()

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get a single layer.
        ''' </summary>
        ''' <param name="varName">The <see cref="eVarNameFlags">variable</see> to get layers for.</param>
        ''' <param name="iIndex">Optional one-based index of the layer to retrieve.</param>
        ''' <returns>A single layer.</returns>
        ''' -----------------------------------------------------------------------
        Function Layer(ByVal varName As eVarNameFlags, Optional ByVal iIndex As Integer = cCore.NULL_VALUE) As cEcospaceLayer

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Method that managed layers can call to request their data.
        ''' </summary>
        ''' <param name="varName">The <see cref="eVarNameFlags">variable</see> to get layer data for.</param>
        ''' <param name="iIndex">Index of the layer to obtain data for.</param>
        ''' <returns>Data in a format that the layer should understand.</returns>
        ''' -----------------------------------------------------------------------
        Function LayerData(ByVal varName As eVarNameFlags, iIndex As Integer) As Object

    End Interface

End Namespace
