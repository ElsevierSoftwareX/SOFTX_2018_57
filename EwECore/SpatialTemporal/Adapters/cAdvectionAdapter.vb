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
    ''' Adapter to populate the Advection monthly maps
    ''' </summary>
    ''' <remarks>
    ''' A scalar is needed to perform unit conversions, direction swapping, etc
    ''' </remarks>
    Public Class cAdvectionAdapter
        Inherits cSpatialScalarDataAdapter

        Private m_spaceData As cEcospaceDataStructures
        Private m_iMonthIndex As Integer
        Private m_orgXData()(,) As Single
        Private m_orgYData()(,) As Single

        ''' <summary>The month that last received data.</summary>
        Private m_iLastReceived As Integer = -1
        ''' <summary>The most recently received map.</summary>
        Private m_lastXData(,) As Double = Nothing
        Private m_lastYData(,) As Double = Nothing

        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal cc As eCoreCounterTypes)
            MyBase.New(core, varName, cc)
        End Sub

        Friend Overrides Sub SaveLayerData()
            Try
                'saving and restoring values via the base SaveLayerData() RestoreLayerData
                'would require modifications to that code
                'this is a lot simpler
                Me.m_orgXData = New Single(11)(,) {}
                Me.m_orgYData = New Single(11)(,) {}

                For i As Integer = 0 To 11
                    Me.m_orgXData(i) = New Single(Me.m_spaceData.InRow + 1, Me.m_spaceData.InCol + 1) {}
                    Me.m_orgYData(i) = New Single(Me.m_spaceData.InRow + 1, Me.m_spaceData.InCol + 1) {}
                    For ir As Integer = 0 To Me.m_spaceData.InRow + 1
                        For ic As Integer = 0 To Me.m_spaceData.InCol + 1
                            Me.m_orgXData(i)(ir, ic) = Me.m_spaceData.MonthlyXvel(i + 1)(ir, ic)
                            Me.m_orgYData(i)(ir, ic) = Me.m_spaceData.MonthlyYvel(i + 1)(ir, ic)
                        Next
                    Next

                Next
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
            ReDim Me.m_lastXData(Me.m_spaceData.InRow + 1, Me.m_spaceData.InCol + 1)
            ReDim Me.m_lastYData(Me.m_spaceData.InRow + 1, Me.m_spaceData.InCol + 1)

        End Sub

        Friend Overrides Sub RestoreLayerData()

            Try
                For i As Integer = 0 To 11
                    For ir As Integer = 0 To Me.m_spaceData.InRow + 1
                        For ic As Integer = 0 To Me.m_spaceData.InCol + 1
                            Me.m_spaceData.MonthlyXvel(i + 1)(ir, ic) = Me.m_orgXData(i)(ir, ic)
                            Me.m_spaceData.MonthlyYvel(i + 1)(ir, ic) = Me.m_orgYData(i)(ir, ic)
                        Next
                    Next
                Next
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

            ' Forget last received map
            Me.m_iLastReceived = -1
            Me.m_lastXData = Nothing
            Me.m_lastYData = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialScalarDataAdapter.Initialize"/>.
        ''' -------------------------------------------------------------------
        Public Overrides Sub Initialize()

            MyBase.Initialize()
            Me.m_spaceData = Me.m_core.m_EcoSpaceData

        End Sub

        ''' <summary>
        ''' About to receive advection data. Overridden to remember the last received advection pattern.
        ''' </summary>
        ''' <param name="bm"></param>
        ''' <param name="layer"></param>
        ''' <param name="conn"></param>
        ''' <param name="iTime"></param>
        ''' <param name="dt"></param>
        ''' <param name="dataExternal"></param>
        ''' <param name="dNoData"></param>
        ''' <returns></returns>
        Protected Friend Overrides Function Adapt(bm As cEcospaceBasemap, layer As cEcospaceLayer, conn As cSpatialDataConnection, iTime As Integer, dt As Date, dataExternal As ISpatialRaster, dNoData As Double) As Boolean

            ' Init appropriate buffer with no_data values. The buffer will be filled with proper data in SetCell(..)
            Select Case layer.Index
                Case 1
                    For ir As Integer = 0 To Me.m_spaceData.InRow + 1
                        For ic As Integer = 0 To Me.m_spaceData.InCol + 1
                            Me.m_lastXData(ir, ic) = cCore.NULL_VALUE
                        Next ic
                    Next ir
                Case 2
                    For ir As Integer = 0 To Me.m_spaceData.InRow + 1
                        For ic As Integer = 0 To Me.m_spaceData.InCol + 1
                            Me.m_lastYData(ir, ic) = cCore.NULL_VALUE
                        Next ic
                    Next ir
            End Select

            Return MyBase.Adapt(bm, layer, conn, iTime, dt, dataExternal, dNoData)

        End Function

        ''' <summary>
        ''' OVerridden to re-inject the last received advection pattern
        ''' </summary>
        ''' <param name="iTime"></param>
        ''' <param name="dNoData"></param>
        ''' <param name="layer"></param>
        ''' <returns></returns>
        Public Overrides Function Populate(iTime As Integer, dNoData As Double, Optional layer As cEcospaceLayer = Nothing) As Boolean

            If MyBase.Populate(iTime, dNoData, layer) Then
                ' Is there external data waiting to be copied to a new timestep?
                If (Me.m_iLastReceived >= 0) And (Me.m_iLastReceived <> Me.m_spaceData.MonthNow) Then
                    ' #Yes: integrate all non-NULL external data values into this month's advection pattern
                    For ir As Integer = 0 To Me.m_spaceData.InRow + 1
                        For ic As Integer = 0 To Me.m_spaceData.InCol + 1
                            If Me.m_lastXData(ir, ic) <> cCore.NULL_VALUE Then
                                Me.m_spaceData.MonthlyXvel(Me.m_spaceData.MonthNow)(ir, ic) = CSng(Me.m_lastXData(ir, ic))
                            End If
                            If Me.m_lastYData(ir, ic) <> cCore.NULL_VALUE Then
                                Me.m_spaceData.MonthlyYvel(Me.m_spaceData.MonthNow)(ir, ic) = CSng(Me.m_lastYData(ir, ic))
                            End If
                        Next
                    Next
                End If
            End If

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cSpatialDataAdapter.SetCell"/>.
        ''' <remarks>Overridden to scale values prior to being set in the 
        ''' Ecospace data structures.</remarks>
        ''' -------------------------------------------------------------------
        Protected Overrides Function SetCell(ByVal layer As cEcospaceLayer,
                                             ByVal conn As cSpatialDataConnection,
                                             ByVal iRow As Integer,
                                             ByVal iCol As Integer,
                                             ByVal sValueAtT As Double) As Boolean

            Try
                'MonthNow is the current month set Ecospace 1-12
                'Advection layer are stored by month
                layer.Cell(iRow, iCol, Me.m_spaceData.MonthNow) = sValueAtT

                ' Also store this in the last map for beautiful copy and paste work
                Select Case layer.Index
                    Case 1
                        Me.m_lastXData(iRow, iCol) = sValueAtT
                    Case 2
                        Me.m_lastYData(iRow, iCol) = sValueAtT
                End Select
                Me.m_iLastReceived = Me.m_spaceData.MonthNow

            Catch ex As Exception

                Dim strMsg As String = "cSpatialDataAdapter::SetCell({0}) at ({1},{2})={3}: exception {4}"
                cLog.Write(ex, cStringUtils.Localize(strMsg, layer.ToString, iCol, iRow, sValueAtT))

                Me.m_core.SpatialOperationLog.LogOperation(cStringUtils.Localize(My.Resources.CoreMessages.STATUS_SPATIALTEMPORAL_ADAPTERROR, iRow, iCol, sValueAtT, ex.Message),
                                                        eStatusFlags.MissingParameter)
                Return False
            End Try

            Return True


        End Function

    End Class

End Namespace
