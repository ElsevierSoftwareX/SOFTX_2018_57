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

Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Data for one time series contained in an Ecosim scenario.
''' </summary>
''' <remarks>
''' This class is implemented as a <see cref="cShapeData">cShapeData</see>.
''' </remarks>
''' ---------------------------------------------------------------------------
Public MustInherit Class cTimeSeries
    Inherits cShapeData

#Region " Protected variables "

    ''' <summary>The <see cref="eTimeSeriesType">type</see> of a time series.</summary>
    Protected m_timeSeriesType As eTimeSeriesType = eTimeSeriesType.NotSet
    ''' <summary>Applied flag</summary>
    Protected m_bEnabled As Boolean = False

    ''' <summary>The core a TS belongs to.</summary>
    Protected m_core As cCore = Nothing

#End Region ' Protected variables

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub New(ByRef core As cCore, ByVal DBID As Integer)
        MyBase.New(0)

        Me.m_core = core
        Me.DBID = DBID

        Me.m_datatype = eDataTypes.NotSet
        Me.m_coreComponent = eCoreComponentType.TimeSeries
        Me.m_timeresolution = eTSDataSetInterval.Annual
        Me.m_coreComponent = eCoreComponentType.TimeSeries

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eTimeSeriesType">type</see> of a time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property TimeSeriesType() As eTimeSeriesType
        Get
            Return Me.m_timeSeriesType
        End Get

        Set(ByVal tstype As eTimeSeriesType)
            ' It is not allowed to switch between TS types once a type has been assigned
            Dim tscatCurr As eTimeSeriesCategoryType = cTimeSeriesFactory.TimeSeriesCategory(Me.m_timeSeriesType)
            Select Case tscatCurr
                Case eTimeSeriesCategoryType.NotSet
                    Me.m_timeSeriesType = tstype
                Case Else
                    If cTimeSeriesFactory.TimeSeriesCategory(tstype) = tscatCurr Then
                        Me.m_timeSeriesType = tstype
                    Else
                        Debug.Assert(False, "Illegal assignment; a TS cannot switch categories")
                    End If
            End Select
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the Covariance for a time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property CV As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the weight of a time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property WtType() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the index of the target that a time series applies to. The
    ''' type of the target is implied by the <see cref="TimeSeriesType">type</see>
    ''' of the time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DatPool() As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the index of the secundary target that a time series applies to. The
    ''' type of the target is implied by the <see cref="TimeSeriesType">type</see>
    ''' of the time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DatPoolSec() As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the annual values for a time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DatVal(ByVal iIndex As Integer) As Single
        Get
            Return CSng(Me.ShapeData(iIndex))
        End Get

        Set(ByVal sValue As Single)
            Me.ShapeData(iIndex) = sValue
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the apply flag on the Time Series. Call <see cref="cCore.UpdateTimeSeries">cCore.UpdateTimeSeries</see>
    ''' to enable all flagged time series to the Ecosim model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Enabled() As Boolean
        Get
            Return (Me.m_bEnabled) And (Me.ValidationStatus = eStatusFlags.OK)
        End Get

        Set(ByVal bEnable As Boolean)
            Me.m_bEnabled = bEnable
        End Set
    End Property


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Weighted Sum of squares for the fit of a data set to the predicted value SSPredErr
    ''' </summary>
    ''' <remarks>
    ''' sumof(log(observed(i)/predicted(i))^2) * [timeseries weight(i)].
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property DataSS() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Average zstat sumof(Log(observed/predicted))/nobs Datq
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DataQ() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' exp(DataQ) average prediction error
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property eDataQ() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, call this to inform the EwE core that a Time Series has changed.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Update() As Boolean
        Try
            Me.m_core.onChanged(Me, eMessageType.DataModified)
            Return True
        Catch ex As Exception
            Debug.Assert(False, String.Format("Failed to update time series {0}", Me.Name))
            Return False
        End Try
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, states whether a time series is a reference series.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsReference() As Boolean
        Return Not Me.IsDriver()
    End Function

    Public Function IsDriver() As Boolean
        Return Me.m_core.m_EcoSim.IsDatTypeDriver(Me.m_timeSeriesType)
    End Function

    Public Function IsRelative() As Boolean
        Return (Me.m_timeSeriesType = eTimeSeriesType.BiomassRel) Or
               (Me.m_timeSeriesType = eTimeSeriesType.CatchesRel) ' Or
        '(Me.TimeSeriesType = eTimeSeriesType.EcotracerConcRel)
    End Function

    Public Function IsAbsolute() As Boolean
        Return Not Me.IsRelative()
    End Function

    <Obsolete("Remove when time series properly use cCore.NULL_VALUE")>
    Public Function SupportsNull() As Boolean
        Return Me.m_timeSeriesType = eTimeSeriesType.DiscardMortality Or
               Me.m_timeSeriesType = eTimeSeriesType.DiscardProportion Or
               Me.m_timeSeriesType = eTimeSeriesType.Landings Or
               Me.m_timeSeriesType = eTimeSeriesType.Discards
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether a time series can be used.
    ''' </summary>
    ''' <returns>A <see cref="eStatusFlags"/> stating whether the time series
    ''' can be used.</returns>
    ''' -----------------------------------------------------------------------
    Public Property ValidationStatus() As eStatusFlags = eStatusFlags.Null

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a textual message explaining the time series <see cref="ValidationStatus"/>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ValidationMessage() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="eTSDataSetInterval">interval</see> for the time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Interval As eTSDataSetInterval

End Class
