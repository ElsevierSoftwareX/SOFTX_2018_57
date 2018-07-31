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
    ''' Data Adapter specific to Relative PP.
    ''' </summary>
    ''' <remarks>
    ''' Does not actually scale the data rather it sets <see cref="cEcospaceDataStructures.PPScale"/> 
    ''' which is used by <see cref="cSpaceSolver">cSpaceSolver.derivtRed</see> to scale RelPP.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class cRelPPDataAdapter
        Inherits cSpatialScalarDataAdapterBase

#Region " Private vars "

        Private m_sPreservedScale As Double = cCore.NULL_VALUE
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
        ''' <inheritdocs cref="cSpatialDataAdapter.InitRun"/>
        ''' <remarks>
        ''' Overridden to clear the PP scale factor.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Sub InitRun()
            MyBase.InitRun()

            ' Reset preserved PP scale
            Me.m_sPreservedScale = cCore.NULL_VALUE

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
                                                  ByVal dNullValue As Double) As Boolean

            Try
                'If this is the first time step?
                'Get the base line PP Scalar
                If (Me.m_sPreservedScale = cCore.NULL_VALUE) And (Me.m_spaceData.PPScale <> cCore.NULL_VALUE) Then
                    Me.m_sPreservedScale = Me.m_spaceData.PPScale
                    'In Ecospace PP is scaled as  [PP = RelPP(i, j) / PPScale] 
                    'PPScale is the mean over the base line map [Total PP] / [n water cells]
                    'DataScale() in the spatial temporal is calculate as [n water cells]/[total]
                    Me.m_spaceData.PPScale = (1 / conn.Scale)
                End If
            Catch ex As Exception
                System.Console.WriteLine("Exception: " & Me.ToString & ".Adapt() " & ex.Message)
                Return False
            End Try

            'Return True
            Return MyBase.Adapt(bm, layer, conn, iTime, dt, dataExternal, dNullValue)

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialDataAdapter.EndRun"/>
        ''' <summary>
        ''' Overridden to restore PP scale factor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub EndRun()
            MyBase.EndRun()

            ' Has preserved PP scale?
            If (Me.m_sPreservedScale <> cCore.NULL_VALUE) Then
                ' #Yes: Restore preserved PP scale
                Me.m_spaceData.PPScale = Me.m_sPreservedScale
                Me.m_sPreservedScale = cCore.NULL_VALUE
            End If

        End Sub

#End Region ' Overrides

    End Class

End Namespace



