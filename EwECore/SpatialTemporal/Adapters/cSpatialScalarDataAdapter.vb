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

#Region " cSpatialScalarDataAdapter "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of <see cref="cSpatialScalarDataAdapterBase"/> to scale data by 
    ''' a given scale.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cSpatialScalarDataAdapter
        Inherits cSpatialScalarDataAdapterBase


#Region " Constructor "

        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal cc As eCoreCounterTypes)
            MyBase.New(core, varName, cc)
        End Sub

#End Region ' Constructor

#Region " Overrides "

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

            If (conn.ScaleType = eScaleType.Relative) And (sValueAtT <> cCore.NULL_VALUE) Then
                sValueAtT /= conn.Scale
            End If
            Return MyBase.SetCell(layer, conn, iRow, iCol, sValueAtT)

        End Function

#End Region ' Overrides

      
    End Class

#End Region

#Region " Base class "

    ''' <summary>
    ''' Derived spatial data adapter to insert scaled external spatial/temporal map data into
    ''' the Ecospace data structures at any given moment. This adapter maintains a scale
    ''' for every map layer attached to the adapter, and will translate map values
    ''' to relative values when <see cref="cSpatialScalarDataAdapter.eScaleType"/> is set to
    ''' <see cref="cSpatialScalarDataAdapter.eScaleType.Relative">relative</see>.
    ''' </summary>
    Public MustInherit Class cSpatialScalarDataAdapterBase
        Inherits cSpatialDataAdapter

#Region " Constructor "

        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal cc As eCoreCounterTypes)
            MyBase.New(core, varName, cc)
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Supported value conversion modes for a <see cref="cSpatialScalarDataAdapter"/>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eScaleType As Byte
            ''' <summary>Values are applied as-is: no scaling is performed.</summary>
            Absolute = 0
            ''' <summary>Value are scaled before being applied.</summary>
            Relative
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the average of a dataset for a year, starting at a given
        ''' Ecospace time step.
        ''' </summary>
        ''' <param name="iFirstTimeStep">The time step that the calculation
        ''' is requested for.</param>
        ''' <param name="dScale">The calculated scale.</param>
        ''' <returns>A <see cref="cDatasetCompatilibity.Compatibility"/> result
        ''' indicating the outcome of the calculation.</returns>
        ''' -------------------------------------------------------------------
        Public Function CalculateScaleFromEcopathTimePeriod(ByVal iLayerIndex As Integer, _
                                                            ByVal conn As cSpatialDataConnection, _
                                                            ByVal iFirstTimeStep As Integer, _
                                                            ByRef dScale As Double) As cDatasetCompatilibity.eCompatibilityTypes

            Dim manConn As cSpatialDataConnectionManager = Me.m_core.SpatialDataConnectionManager
            Dim manSets As cSpatialDataSetManager = manConn.DatasetManager
            Dim result As cDatasetCompatilibity.eCompatibilityTypes = cDatasetCompatilibity.eCompatibilityTypes.Errors

            ' Early bail-out
            If Not Me.IsConnected(iLayerIndex) Then Return result

            ' Suspend indexing
            manSets.SuspendIndexing()

            Dim ds As ISpatialDataSet = conn.Dataset
            Dim cv As ISpatialDataConverter = conn.Converter
            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim iInRow As Integer = bm.InRow
            Dim iInCol As Integer = bm.InCol
            Dim dCellSize As Double = bm.CellSize
            Dim layer As cEcospaceLayer = bm.Layer(Me.VarName, iLayerIndex - 1)
            Dim depth As cEcospaceLayerDepth = bm.LayerDepth
            Dim excl As cEcospaceLayerExclusion = bm.LayerExclusion
            Dim strLayerName As String = layer.Name
            Dim ldtData As New List(Of DateTime)
            Dim iTSMin As Integer = Integer.MaxValue
            Dim iTSMax As Integer = 1
            Dim rs As ISpatialRaster = Nothing
            Dim dMapTotValue As Double = 0.0
            Dim iNumWaterCells As Integer = 0
            Dim msg As cProgressMessage = Nothing

            ' Determine time steps with overlap
            'Console.Write("Calculating map average for ")
            For i As Integer = iFirstTimeStep To iFirstTimeStep + CInt(Me.m_core.EcospaceModelParameters.NumberOfTimeStepsPerYear) - 1
                Dim dt As Date = Me.m_core.EcospaceTimestepToAbsoluteTime(i)
                If ds.HasDataAtT(dt) Then
                    iTSMin = Math.Min(iTSMin, i)
                    iTSMax = Math.Max(iTSMax, i)
                    ldtData.Add(dt)
                    'Console.Write(dt.ToShortDateString & ", ")
                End If
            Next
            'Console.WriteLine()

            Me.m_core.Messages.SendMessage(New cProgressMessage(eProgressState.Start, 1.0!, 0.0!, "", eMessageType.Progress))
            Try
                For i As Integer = 0 To ldtData.Count - 1

                    Dim dt As DateTime = ldtData(i)
                    msg = New cProgressMessage(eProgressState.Running, 1.0!, CSng((i + 1) / ldtData.Count), _
                                               String.Format(My.Resources.CoreMessages.STATUS_SPATIALTERMPORAL_CALCULATING, strLayerName, ds.DisplayName, ""), _
                                               eMessageType.Progress, eDataTypes.EcospaceSpatialDataConnection)
                    Me.m_core.Messages.SendMessage(msg)

                    ' Do the spatial magics
                    If (ds.LockDataAtT(dt, dCellSize, bm.PosTopLeft, bm.PosBottomRight, bm.ProjectionString)) Then
                        rs = ds.GetRaster(cv, strLayerName)
                        If rs IsNot Nothing Then
                            For iRow As Integer = 1 To iInRow
                                For iCol As Integer = 1 To iInCol
                                    'If depth.IsWaterCell(iRow, iCol) Then
                                    If depth.IsWaterCell(iRow, iCol) And (Not excl.IsExcludedCell(iRow, iCol)) Then
                                        Dim dval As Double = rs.Cell(iRow, iCol)
                                        If (dval <> cCore.NULL_VALUE And dval <> rs.NoData) Then
                                            iNumWaterCells += 1
                                            dMapTotValue += dval
                                        End If
                                    End If
                                Next
                            Next
                        Else
                            'Log message raster not read
                        End If

                        ds.Unlock()
                    End If

                Next
            Catch ex As Exception
                ' Log error
                cLog.Write(ex, eVerboseLevel.Detailed, "Failed to calculate scaling factor for dataset " & ds.DisplayName)
            End Try
            Me.m_core.Messages.SendMessage(New cProgressMessage(eProgressState.Finished, 1.0!, 1.0!, "", eMessageType.Progress))

            If dMapTotValue = 0 Then dMapTotValue = 1
            If iNumWaterCells = 0 Then iNumWaterCells = 1

            'Get the scalar value from a function
            'So different adapters can use a different scalar calculation
            dScale = Me.CalculateScalar(dMapTotValue, iNumWaterCells)

            manSets.ResumeIndexing()

            ' Report for the calculation period
            Dim man As cSpatialDataSetManager = Me.m_core.SpatialDataConnectionManager.DatasetManager
            Dim comp As cDatasetCompatilibity = man.Compatibility(ds)
            comp.Refresh()
            Return comp.Compatibility

        End Function

        ''' <summary>
        ''' Calculates a scale value base on the sum and number of cells over the time period.
        ''' </summary>
        ''' <param name="SumOverPeriod">Sum of values over the time period.</param>
        ''' <param name="nMapCells">Total number of cells included in the sum.</param>
        ''' <returns> (1 / mean)</returns>
        ''' <remarks>Default scalar for relative adapters. Return the mean scalar as a multiplier.</remarks>
        Public Overridable Function CalculateScalar(SumOverPeriod As Double, nMapCells As Double) As Double
            Try
                'This is the default value for a relative scalar that is used as multiplier 
                'RelPP and Relative biomass can use this 
                'Biomass forcing overrides this to return the average
                Return nMapCells / SumOverPeriod
            Catch ex As Exception
                cLog.Write(ex, "Failed to calculate scale value.")
            End Try
            'Ohh... my...
            Return 1.0
        End Function

#End Region ' Public access

    End Class

#End Region ' Base class

End Namespace
