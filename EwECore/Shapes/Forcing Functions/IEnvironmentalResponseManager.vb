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


Public Interface IEnvironmentalResponseManager

    ReadOnly Property nEnviroData As Integer

    ReadOnly Property EnviroData(ByVal MapIndex As Integer) As IEnviroInputData

    'ReadOnly Property Map(ByVal LayerName As String) As IEnviroInputData

    ReadOnly Property EnviroData(ByVal layer As cEcospaceLayer) As IEnviroInputData

    ReadOnly Property MediationData() As cMediationDataStructures

    ReadOnly Property SpaceData() As cEcospaceDataStructures

    ReadOnly Property SimData() As cEcosimDatastructures

    Function onChanged() As Boolean

   

End Interface
