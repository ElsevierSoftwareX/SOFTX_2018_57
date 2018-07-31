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
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace SpatialData

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Data Adapter specific to Capacity layers .
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class cCapacityDataAdapter
        Inherits cSpatialDataAdapter

#Region " Private vars "

        Private m_spaceData As cEcospaceDataStructures

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal cc As eCoreCounterTypes)
            MyBase.New(core, varName, cc)
        End Sub

#End Region ' Constructor

#Region " Overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialScalarDataAdapter.Initialize"/>.
        ''' -------------------------------------------------------------------
        Public Overrides Sub Initialize()
            MyBase.Initialize()
            Me.m_spaceData = Me.m_core.m_EcoSpaceData
        End Sub

     
        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialScalarDataAdapter.Adapt"/>
        ''' <remarks>
        ''' Called before data from an external source is copied into <see cref="cEcospaceDataStructures.RelPP"/>
        ''' EcoSpace uses an internal scaler to scale PP data to Ecopath levels. <see cref="cEcospaceDataStructures.PPScale"/>
        ''' This is the mean relative PP across all water cells computed from the currently loaded  <see cref="cEcospaceDataStructures.RelPP"/> array.
        ''' <see cref="cSpatialScalarDataAdapter.SetCell"/> will scale external data to a the first timestep or a user defined value.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Friend Overrides Function Adapt(ByVal bm As cEcospaceBasemap, _
                                                  ByVal layer As cEcospaceLayer, _
                                                  ByVal conn As cSpatialDataConnection, _
                                                  ByVal iTime As Integer, _
                                                  ByVal dt As Date, _
                                                  ByVal dataExternal As ISpatialRaster, _
                                                  ByVal dNoData As Double) As Boolean
            Dim breturnVal As Boolean

            Me.m_spaceData.isCapacityChanged = True
            breturnVal = MyBase.Adapt(bm, layer, conn, iTime, dt, dataExternal, dNoData)

            'isGroupHabCapChanged(group) tells the habitat capacity model 
            'that the capacity inputs for a group have changed.
            'This is an optimization so only the groups that have changed will be recomputed

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'jb 3-July-2018
            'We could set isGroupHabCapChanged() = breturnVal
            'based on the group = layer.index 
            'this has to be correct because that's how other parts of the UI and core get the group index
            'xxxxxxxxxxxxxxxxxxxxxxxx
            'Me.m_spaceData.isGroupHabCapChanged(layer.Index) = breturnVal
            'xxxxxxxxxxxxxxxxxxxxxxxx
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            For iGroup As Integer = 1 To Me.m_spaceData.NGroups
                'Ok Turn on the groups that were changed
                Me.m_spaceData.isGroupHabCapChanged(iGroup) = breturnVal
            Next

            Return breturnVal

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialDataAdapter.EndRun"/>
        ''' <summary>
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub EndRun()
            MyBase.EndRun()
        End Sub

#End Region ' Overrides

    End Class

End Namespace
