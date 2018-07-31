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


    ''' <summary>
    ''' Adapter base class for forcing data.
    ''' </summary>
    ''' <remarks>
    ''' Handles base class functionality for forcing data. 
    ''' Restores data to forced state during the timestep so interface displays forced data instead of predicted. 
    ''' The option to display forced of predicted values could be added to the interface.
    ''' Stops data from being overwritten at the end of the run so data for the last run timestep is displayed, instead of overwritten.  
    ''' </remarks>
    Public MustInherit Class cForcingAdapterBase
        Inherits cSpatialScalarDataAdapterBase

#Region "Internal classes"

        ''' <summary>
        ''' Class to buffer forcing data from the last timestep so it can be restored later in the timestep.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Class cForcingMapIndexPair

            ''' <summary>
            ''' Forcing data from the last timestep
            ''' </summary>
            ''' <remarks></remarks>
            Public data(,) As Single

            ''' <summary>
            ''' Index of the layer/Group that this data is applied to
            ''' </summary>
            ''' <remarks></remarks>
            Public iLayerIndex As Integer

            ''' <summary>
            ''' Has this timestep been forced
            ''' </summary>
            ''' <remarks></remarks>
            Public isTimeStepForced As Boolean

            Private m_spacedata As cEcospaceDataStructures

            ''' <summary>
            ''' Mask of cells that where not forced in the timestep
            ''' </summary>
            ''' <remarks>This could have been done with cCore.NULL_VALUE but this makes debugging easier. You can tell which cells were never set to forcing.</remarks>
            Public Const NULL_CELL As Integer = -6666

            Public Sub New(IndexOfLayer As Integer, EcospaceData As cEcospaceDataStructures)
                iLayerIndex = IndexOfLayer
                m_spacedata = EcospaceData
                data = New Single(m_spacedata.InRow, m_spacedata.InCol) {}
                Me.clear()
            End Sub

            Public Sub clear()
                For ir As Integer = 1 To m_spacedata.InRow
                    For ic As Integer = 1 To m_spacedata.InCol
                        data(ir, ic) = NULL_CELL
                    Next
                Next

            End Sub

        End Class

#End Region

#Region " Variables"


        Protected m_spaceData As cEcospaceDataStructures

        Protected m_ForcingMaps As cForcingMapIndexPair()

#End Region

#Region "Construction and Initialization"


        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal cc As eCoreCounterTypes)
            MyBase.New(core, varName, cc)

        End Sub


        Public Overrides Sub InitRun()
            MyBase.InitRun()

            Me.InitForcingMaps()

        End Sub


        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialScalarDataAdapter.Initialize"/>.
        ''' -------------------------------------------------------------------
        Public Overrides Sub Initialize()
            MyBase.Initialize()
            Me.m_spaceData = Me.m_core.m_EcoSpaceData
        End Sub


#End Region

#Region "Overrides"

        ''' <summary>
        ''' Intercepts the Adapt functionality to set the isTimeStepForced. 
        ''' This tells the <see cref="cForcingAdapterBase.RestoreForcing">RestoreForcing</see> method that data has been forced for this timestep 
        ''' and it should used to overwrite the predicted values.
        ''' </summary>
        ''' <param name="bm"></param>
        ''' <param name="layer"></param>
        ''' <param name="conn"></param>
        ''' <param name="iTime"></param>
        ''' <param name="dt"></param>
        ''' <param name="dataExternal"></param>
        ''' <param name="dNoData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function Adapt(ByVal bm As cEcospaceBasemap, ByVal layer As cEcospaceLayer,
                                                ByVal conn As cSpatialDataConnection, ByVal iTime As Integer, ByVal dt As Date,
                                                ByVal dataExternal As ISpatialRaster, ByVal dNoData As Double) As Boolean

            Me.setIsForced(layer.Index)

            Return MyBase.Adapt(bm, layer, conn, iTime, dt, dataExternal, dNoData)

        End Function

        Friend Overrides Sub RestoreLayerData()
            'Don't restore forcing data to it's original state
        End Sub

        Friend Overrides Sub SaveLayerData()
            'Don't restore forcing data to it's original state
        End Sub

#End Region

#Region "Forcing base functionality"

        ''' <summary>
        ''' Initialize buffer for holding forced data from the start of the timestep. 
        ''' This is the data that will be restored in <see cref="RestoreForcing"> </see>.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overridable Sub InitForcingMaps()

            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim iNumRow As Integer = bm.InRow
            Dim iNumCol As Integer = bm.InCol

            'If there is old data then clear it out
            If Me.m_ForcingMaps IsNot Nothing Then
                Me.m_ForcingMaps = Nothing
            End If
            Me.m_ForcingMaps = New cForcingMapIndexPair(Me.m_core.nGroups) {}

            Try

                ' For all layers
                For Each layer As cEcospaceLayer In bm.Layers(Me.m_varName)
                    ' Is driven by external data?
                    If (Me.IsConnected(layer.Index) And layer.IsExternalData And Me.IsEnabled(layer.Index)) Then
                        Me.m_ForcingMaps(layer.Index) = New cForcingMapIndexPair(layer.Index, Me.m_spaceData)
                    End If

                Next layer

            Catch ex As Exception

            End Try

        End Sub


        ''' <summary>
        ''' Restores forced data to forced state, overwritting the predicted values. 
        ''' Cells in the external data that are cCore.NULL_VALUE will not be restored.
        ''' This allows for forcing of partial areas while leaving the other cells to be predicted by Ecospace.
        ''' </summary>
        ''' <param name="SpaceData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function RestoreForcing(SpaceData As cEcospaceDataStructures) As Boolean
            Dim i As Integer
            Dim n As Integer
            Try

                For Each pair As cForcingMapIndexPair In Me.m_ForcingMaps
                    i += 1
                    n = 0
                    If pair IsNot Nothing Then
                        'Only restore the biomass if this timestep was forced
                        'If not leave the predicted biomass in place for the next timestep
                        If pair.isTimeStepForced Then

                            Try
                                For ir As Integer = 1 To SpaceData.InRow
                                    For ic As Integer = 1 To SpaceData.InCol
                                        'Don't restore data that is cCore.NULL_VALUE(-9999) or cForcingMapIndexPair.NULL_CELL(-6666)in the original forcing data
                                        'At the start of each timestep all the values in cForcingMapIndexPair.data(r,c) are set to NULL_CELL 
                                        'this acts as a mask for cells that were not forced.
                                        'The external data only sets modeled cells that are <> cCore.NULL_VALUE to forcing values. 
                                        'In cForcingMapIndexPair.data() cCore.NULL_VALUE = not modeled,  cForcingMapIndexPair.NULL_CELL = modeled but not forced
                                        If SpaceData.Depth(ir, ic) > 0 And _
                                            pair.data(ir, ic) <> cCore.NULL_VALUE And _
                                            pair.data(ir, ic) <> cForcingMapIndexPair.NULL_CELL Then

                                            SpaceData.Bcell(ir, ic, pair.iLayerIndex) = pair.data(ir, ic)
                                        End If
                                        'For debugging missing data from external forcing
                                        'If pair.data(ir, ic) = cForcingMapIndexPair.NULL_CELL Then n += 1
                                    Next ic
                                Next ir
                                'Debug.Assert(n = 0)
                            Catch ex As Exception
                                Debug.Assert(False, "Oppss... " + ex.Message)
                                cLog.Write(ex, "Failed to restore forced biomass for group " + i.ToString)
                            End Try
                        End If

                        'Re-set the isTimeStepForced Flag
                        'this will be set to True next time the adapter loads data for a timestep
                        pair.isTimeStepForced = False

                    End If 'If pair IsNot Nothing Then
                Next pair

            Catch ex As Exception
                cLog.Write(ex, "Failed to restore forced data.")
            End Try

        End Function

        Protected Overridable Sub saveForcedCell(iLayerIndex As Integer, ByVal iRow As Integer, ByVal iCol As Integer, ByVal sValueAtT As Double)

            Debug.Assert(Me.m_ForcingMaps(iLayerIndex) IsNot Nothing, Me.ToString + ".saveForcedCell() Layer index not set to valid layer!")
            Try
                'Store the forced biomass
                'So it can be restored later in the timestep
                Me.m_ForcingMaps(iLayerIndex).data(iRow, iCol) = CSng(sValueAtT)
            Catch ex As Exception

            End Try

        End Sub


        Protected Sub setIsForced(iLayerIndex As Integer)

            Debug.Assert(Me.m_ForcingMaps(iLayerIndex) IsNot Nothing, Me.ToString + ".setIsForced() Layer index not set to valid layer!")

            Try
                Dim pair As cForcingMapIndexPair = Me.m_ForcingMaps(iLayerIndex)
                pair.isTimeStepForced = True
                'Set all the cells in the map to NULL_CELL(-6666)
                'this acts as a mask so only cells that were forced (Not NULL_CELL) will be restored
                pair.clear()
            Catch ex As Exception
                'If this happens it should just be in development
                'I hope...
                Debug.Assert(False, ex.Message)
            End Try
        End Sub

#End Region


    End Class

End Namespace

