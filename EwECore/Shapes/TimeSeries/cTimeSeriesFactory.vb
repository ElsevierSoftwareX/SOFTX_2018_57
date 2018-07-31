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

''' ---------------------------------------------------------------------------
''' <summary>
''' The one access point in EwE to create <see cref="cTimeSeries">cTimeSeries</see>
''' -derived objects, and to translate between time series <see cref="eTimeSeriesType">types</see>
''' and <see cref="eTimeSeriesCategoryType">categories</see>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTimeSeriesFactory

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Determine the <see cref="eTimeSeriesCategoryType">Time series category</see>
    ''' based on a <see cref="eTimeSeriesType">Time series type</see>. For instance,
    ''' time series types <see cref="eTimeSeriesType.Catches"/>
    ''' and <see cref="eTimeSeriesType.FishingEffort"/>
    ''' are <see cref="eTimeSeriesCategoryType.Fleet"/>-related time series. With the
    ''' Discardless changes time series category <see cref="eTimeSeriesCategoryType.FleetGroup"/>
    ''' was introduced.
    ''' </summary>
    ''' <param name="timeSeriesType">The type to get the category for.</param>
    ''' <remarks>
    ''' This method was added to centralize interpretation of the awkward enumerator 
    ''' <see cref="eTimeSeriesType">eTimeSeriesType</see>.
    ''' </remarks>
    ''' <returns>
    ''' A time series category for the provided time series type.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function TimeSeriesCategory(ByVal timeSeriesType As eTimeSeriesType) As eTimeSeriesCategoryType

        Select Case timeSeriesType

            Case eTimeSeriesType.NotSet
                Return eTimeSeriesCategoryType.NotSet

            Case eTimeSeriesType.TimeForcing
                Return eTimeSeriesCategoryType.Forcing

            Case eTimeSeriesType.FishingEffort
                Return eTimeSeriesCategoryType.Fleet

            Case eTimeSeriesType.DiscardMortality,
                 eTimeSeriesType.DiscardProportion,
                 eTimeSeriesType.Landings,
                 eTimeSeriesType.Discards
                Return eTimeSeriesCategoryType.FleetGroup

            Case Else
                Return eTimeSeriesCategoryType.Group

        End Select

        ' Add this for good manners.
        Return eTimeSeriesCategoryType.NotSet
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Factory method; the only location in EwE where actual <see cref="cTimeSeries">cTimeSeries-derived</see>
    ''' objects are created.
    ''' </summary>
    ''' <param name="timeSeriesType">The <see cref="eTimeSeriesType">type</see> of
    ''' the time series.</param>
    ''' <returns>A Time Series instance, or nothing if an error occurred.</returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function CreateTimeSeries(ByVal timeSeriesType As eTimeSeriesType,
            ByVal core As cCore, ByVal iDBID As Integer) As cTimeSeries

        Dim ts As cTimeSeries = Nothing

        Select Case TimeSeriesCategory(timeSeriesType)

            Case eTimeSeriesCategoryType.Forcing
                ts = Nothing ' No can do

            Case eTimeSeriesCategoryType.Fleet,
                 eTimeSeriesCategoryType.FleetGroup
                ts = New cFleetTimeSeries(core, iDBID)

            Case eTimeSeriesCategoryType.Group
                ts = New cGroupTimeSeries(core, iDBID)

            Case eTimeSeriesCategoryType.NotSet
                Debug.Assert(False, String.Format("Unknown category of time series for type {0}", timeSeriesType))

        End Select

        Return ts
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all <see cref="eTimeSeriesType"/> of the same <see cref="eTimeSeriesCategoryType">category</see>
    ''' as the provided <paramref name="type"/>.
    ''' </summary>
    ''' <param name="type">The <see cref="eTimeSeriesType">type</see> to find others for.</param>
    ''' <returns>Well...</returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function CompatibleTypes(ByVal type As eTimeSeriesType) As eTimeSeriesType()
        Return CompatibleTypes(TimeSeriesCategory(type))
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all <see cref="eTimeSeriesType"/> within a give <see cref="eTimeSeriesCategoryType">category</see>.
    ''' </summary>
    ''' <param name="cat">The <see cref="eTimeSeriesCategoryType">category</see> to find others for.</param>
    ''' <returns>Well...</returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function CompatibleTypes(ByVal cat As eTimeSeriesCategoryType) As eTimeSeriesType()
        Dim lTypes As New List(Of eTimeSeriesType)
        For Each t As eTimeSeriesType In [Enum].GetValues(GetType(eTimeSeriesType))
            If (TimeSeriesCategory(t) = cat) Or (cat = eTimeSeriesCategoryType.NotSet) Then
                lTypes.Add(t)
            End If
        Next
        Return lTypes.ToArray()
    End Function

End Class