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
    ''' Data Adapter specific to Biomass forcing.
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class cBiomassRelativeAdapter
        Inherits cForcingAdapterBase

#Region " Private vars "

        ''' <summary>Ragged array used to store the base map by Layer</summary>
        Private m_baseLayers()(,) As Single
        ''' <summary>Has the base map for this layer been initialized?</summary>
        Private m_IsBaseInitialized() As Boolean

        Private m_baseMeanBio() As Single

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal cc As eCoreCounterTypes)
            MyBase.New(core, varName, cc)
        End Sub

#End Region ' Constructor

#Region " Overrides "

        Public Overrides Sub InitRun()
            MyBase.InitRun()

            'Called at the start of each run
            'Allocate arrays for the base layers and boolean flags 
            Dim n As Integer = Me.m_core.GetCoreCounter(m_coreCounter)
            'Just allocate the layers array 
            'Each map will be initialized once on the first call
            m_baseLayers = New Single(n)(,) {}
            m_IsBaseInitialized = New Boolean(n) {}
            m_baseMeanBio = New Single(n) {}

        End Sub


        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialDataAdapter.SetCell"/>.
        ''' <remarks>Overridden to scale values prior to being set in the 
        ''' Ecospace data structures.</remarks>
        ''' -------------------------------------------------------------------
        Protected Overrides Function SetCell(ByVal layer As cEcospaceLayer, _
                                             ByVal conn As cSpatialDataConnection, _
                                             ByVal iRow As Integer, _
                                             ByVal iCol As Integer, _
                                             ByVal sValueAtT As Double) As Boolean
            Try

                Dim value As Double
                If sValueAtT <> cCore.NULL_VALUE Then

                    'xxxxxxxxxxxxxxxxxxxxxxxx
                    'jb 9-Sept-2016 changed to just use the external pattern and change over time
                    'External data is the pattern of biomass distribution relative to the Ecospace base biomass
                    'B = [B base at t=zero] * [B external] * [1/mean B external at t=zero]
                    'value = CDbl(Me.m_baseLayers(layer.Index)(iRow, iCol)) * sValueAtT * conn.Scale
                    'xxxxxxxxxxxxxxxxxxxxxx

                    'jb 9-Sept-2016 still needs to be debugged
                    'Use the pattern of the external data scaled to the ecospace base biomass
                    'conn.Scale = [number of water cells] / [sum of external data across depth map] = 1/[mean external data]
                    'Me.m_baseMeanBio() = average ecospace biomass across map
                    value = sValueAtT * conn.Scale * Me.m_baseMeanBio(layer.Index)
                Else
                    value = sValueAtT
                End If

                Me.saveForcedCell(layer.Index, iRow, iCol, value)

                Return MyBase.SetCell(layer, conn, iRow, iCol, value)

            Catch ex As Exception
                Debug.Assert(False, Me.ToString + ".SetCell() Exception: " + ex.Message)
            End Try

            Return False

        End Function


        Protected Friend Overrides Function Adapt(ByVal bm As cEcospaceBasemap, ByVal layer As cEcospaceLayer,
                                                  ByVal conn As cSpatialDataConnection, ByVal iTime As Integer, ByVal dt As Date,
                                                  ByVal dataExternal As ISpatialRaster, ByVal dNoData As Double) As Boolean

            'This is a "First Chance" initialization of the base layers 
            Me.InitializeBaseLayer(layer.Index)

            Return MyBase.Adapt(bm, layer, conn, iTime, dt, dataExternal, dNoData)

        End Function

#End Region ' Overrides

#Region "Internal methods"

        ''' <summary>
        ''' Keep a copy of the base map for this layer
        ''' </summary>
        ''' <param name="iLayer"></param>
        ''' <remarks>First Chance initialization. This should only be called once.</remarks>
        Private Sub InitializeBaseLayer(iLayer As Integer)

            If Me.m_IsBaseInitialized(iLayer) Then
                'First Chance Initialization
                'Only do this once at the start of the run
                'Me.m_IsBaseInitialized(iLayer) is reset at the start of each run in InitRun()
                Return
            End If

            Try
                'If this Assert fires something is wrong in the code
                'InitializeBaseLayer() should only be called once at the start of each run and cleared between runs
                Debug.Assert(m_baseLayers(iLayer) Is Nothing, Me.ToString + ".InitializeBaseLayer() Trying to initialize a layer that has already been initialized.")

                Dim n As Integer
                Dim layer() As cEcospaceLayer = Me.m_core.EcospaceBasemap.Layers(Me.m_varName)

                'Allocate base map storeage for this layer
                m_baseLayers(iLayer) = New Single(m_spaceData.InRow, m_spaceData.InCol) {}
                'Copy the data from the layer into the base layers
                'used to scale the values relative to the Ecospace base
                For ir As Integer = 1 To m_spaceData.InRow
                    For ic As Integer = 1 To m_spaceData.InCol
                        m_baseLayers(iLayer)(ir, ic) = CSng(layer(iLayer - 1).Cell(ir, ic))
                        If Me.m_spaceData.Depth(ir, ic) > 0 Then
                            Me.m_baseMeanBio(iLayer) += CSng(layer(iLayer - 1).Cell(ir, ic))
                            n += 1
                        End If
                    Next
                Next

                'average biomass across the map
                If n = 0 Then n = 1
                Me.m_baseMeanBio(iLayer) /= n

                'Stop initialization from being called multiple times...
                Me.m_IsBaseInitialized(iLayer) = True

            Catch ex As Exception
                Me.m_IsBaseInitialized(iLayer) = False
                cLog.Write(ex, Me.ToString + ".InitializeBaseLayer() Failed to save base map layer.")
            End Try

        End Sub

#End Region

    End Class

End Namespace
