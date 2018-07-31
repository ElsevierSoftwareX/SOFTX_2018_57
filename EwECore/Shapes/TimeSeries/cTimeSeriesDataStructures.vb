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

Option Strict On
Imports EwEUtils.Core

''' <summary>
''' Class that holds and manages all time series-related data in the EwE core.
''' </summary>
Public Class cTimeSeriesDataStructures

    Public Const ANNUAL_DATA_MONTH As Integer = 6

    Private m_SimData As cEcosimDatastructures
    Private m_PathData As cEcopathDataStructures

    Public nGroups As Integer
    Public nFleets As Integer

    ' ------------------------------------------------
    ' Dataset structures
    ' ------------------------------------------------

    Public ActiveDatasetIndex As Integer = cCore.NULL_VALUE

    ''' <summary>Number of available datasets.</summary>
    Public nDatasets As Integer = 0
    ''' <summary>Dataset database IDs</summary>
    Public iDatasetDBID() As Integer
    ''' <summary>Names of available datasets.</summary>
    Public strDatasetNames() As String
    ''' <summary>Authors of available datasets.</summary>
    Public strDatasetAuthor() As String
    Public strDatasetContact() As String
    ''' <summary>Descriptions of available datasets.</summary>
    Public strDatasetDescription() As String
    ''' <summary>Number of time series contained in each dataset.</summary>
    Public nDatasetNumTimeSeries() As Integer
    Public nDatasetFirstYear() As Integer
    ''' <summary>The number of data points in the dataset.</summary>
    Public nDatasetNumPoints() As Integer

    Public DataSetIntervals() As eTSDataSetInterval

    ' ------------------------------------------------
    ' Interface structures
    ' ------------------------------------------------

    ''' <summary>Number of time series in the model.</summary>
    Public nTimeSeries As Integer

    ''' <summary>Maximum number of years across all time series.</summary>
    Public nMaxYears As Integer
    ''' <summary>Database ID for each time series.</summary>
    Public iTimeSeriesDBID() As Integer
    ''' <summary>Name of each time series.</summary>
    Public strName() As String
    '''' <summary>Number of years of each time series.</summary>
    'Public iNumYears() As Integer
    ''' <summary>Array of flags indicating which a time series must be applied.</summary>
    Public bEnable() As Boolean
    ''' <summary>Type of each time series.</summary>
    Public TimeSeriesType() As eTimeSeriesType
    ''' <summary>Index of the core object that each time series links to. The type
    ''' of the core object is implied by <see cref="TimeSeriesType">TimeSeriesType</see>.</summary>
    Public iPool() As Integer
    ''' <summary>Index of the core object of a secundary time series target, if applicable. The type
    ''' of the core object is implied by <see cref="TimeSeriesType">TimeSeriesType</see>.</summary>
    Public iPoolSec() As Integer
    ''' <summary>Weight type for each time series.</summary>
    Public sWeight() As Single
    ''' <summary>CV for each time series.</summary>
    Public sCV() As Single
    '''' <summary>First year of each time series.</summary>
    'Public iFirstYear() As Integer
    ''' <summary>Annual values for each time series, indexed as (iYear, iSeries).</summary>
    Public sValues(,) As Single
    Public sDatSS() As Single
    Public sDatQ() As Single
    Public sEDatQ() As Single 'exp(sDatQ)

    ''' <summary>Weighted Sum of Squared Prediction Error by time series data set sumof(log(observed(i)/predicted(i))^2) * [timeseries weight(i)].</summary>
    Public sSSPredErr() As Single

    ' ------------------------------------------------
    ' Applied structures
    ' ------------------------------------------------

    ''' <summary>Number of applied time series.</summary>
    Public NdatType As Integer

    ''' <summary>Number of datum points across all applied time series.</summary>
    Public nDatPoints As Integer

    ' ''' <summary>Max number of years across all applied time series.</summary>
    'Public NdatYear As Integer


    Public nAICTimeSeries As Integer

    ''' <summary><see cref="eTimeSeriesType">Type</see> of each applied time series.</summary>
    Public DatType() As eTimeSeriesType
    ''' <summary>Index of the core object that each applied time series links to. The type
    ''' of the core object is implied by <see cref="DatType">DatType</see>.</summary>
    Public DatPool() As Integer
    ''' <summary>Index of the second core object that each applied time series links to. The type
    ''' of the core object is implied by <see cref="DatType">DatType</see>.</summary>
    Public DatPoolSec() As Integer
    ''' <summary>Weight type for each applied time series.</summary>
    Public WtType() As Single
    ' ''' <summary>Annual values for each applied time series, indexed as (iYear, iSeries).</summary>
    Public DatVal(,) As Single
    ''' <summary>Year of the datum point.</summary>
    Public DatYear() As Integer
    Public DatSS() As Single

    ''' <summary>Time interval of the currently selected dataset (monthly or annual).</summary>
    Public DataSetInterval As eTSDataSetInterval

    ''' <summary>Sum of Squared Prediction Error by time series data set sumof(log(observed(i)/predicted(i))^2) * [timeseries weight(i)].</summary>
    Public SSPredErr() As Single

    ''' <summary>mean(sumof(log(observed/predicted))) by data type</summary>
    Public DatQ() As Single
    Public eDatQ() As Single

    Public PoolForceBB(,) As Single
    Public PoolForceZ(,) As Single
    Public PoolForceCatch(,) As Single
    Public ForcedFs(,) As Single

    ''' <summary>
    ''' Proportion of total catch that is discarded. By Fleet,Group,Time
    ''' </summary>
    Public PoolForceDiscardProp(,,) As Single

    ''' <summary>
    ''' Proportion of discards that incur mortality. By Fleet,Group,Time
    ''' </summary>
    Public PoolForceDiscardMort(,,) As Single

    ''' <summary>
    ''' Index to the current year/datatype
    ''' </summary>
    ''' <remarks>This is increment for each data type each time the stats are collected. Once a year.</remarks>
    Public Iobs As Integer
    Public Wt() As Single

    Public Yhat() As Single

    ''' <summary>log(observed/predicted) by observation</summary>
    Public Erpred() As Single

    Public Sub New(ByVal EcopathData As cEcopathDataStructures, ByVal EcosimData As cEcosimDatastructures)
        Me.m_PathData = EcopathData
        Me.m_SimData = EcosimData
    End Sub

    ''' <summary>
    ''' Clear all time series data and free memory
    ''' </summary>
    Friend Sub Clear()
        Me.nDatasets = 0
        Me.ActiveDatasetIndex = cCore.NULL_VALUE
        Me.RedimTimeSeriesDatasets()
        Me.ClearTimeSeries()
    End Sub

    ''' <summary>
    ''' Number of years in the reference data set
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property nYears As Integer
        Get
            Dim n As Integer
            Select Case Me.DataSetInterval
                Case eTSDataSetInterval.Annual
                    n = Me.nDatPoints
                Case eTSDataSetInterval.TimeStep
                    n = Me.nDatPoints \ cCore.N_MONTHS
                Case Else
                    Debug.Assert(False, Me.ToString + " Undefined DataSetInterval type.")
            End Select

            Return n

        End Get

    End Property

    Public Function toForcingTimeStep(iModelTimeStep As Integer, iModelYear As Integer) As Integer
        Dim its As Integer
        If Me.DataSetInterval = eTSDataSetInterval.Annual Then
            its = iModelYear
        ElseIf Me.DataSetInterval = eTSDataSetInterval.TimeStep Then
            its = iModelTimeStep
        End If

        'jb let the forcing index exceed the originaly loaded forcing data
        'If the run length was extened then the data in the forcing arrays will be zero
        ''Constrain the time step index 
        ''to the last reference data time step
        'If its > Me.nDatPoints Then
        '    'really just for debugging
        '    its = its
        'End If

        Return its

    End Function


    Public Function isTimeStepValid(iModelTimeStep As Integer) As Boolean
        ' System.Console.WriteLine(iModelTimeStep.ToString)
        If Me.DataSetInterval = eTSDataSetInterval.Annual Then
            If (iModelTimeStep / cCore.N_MONTHS) <= Me.nYears Then Return True
        ElseIf Me.DataSetInterval = eTSDataSetInterval.TimeStep Then
            If iModelTimeStep <= Me.nDatPoints Then Return True
        End If

        Return False

    End Function

    Private Sub ClearForcing()

        Array.Clear(Me.PoolForceBB, 0, Me.PoolForceBB.Length)
        Array.Clear(Me.PoolForceCatch, 0, Me.PoolForceCatch.Length)
        Array.Clear(Me.PoolForceZ, 0, Me.PoolForceZ.Length)

        Array.Clear(Me.PoolForceDiscardMort, 0, Me.PoolForceDiscardMort.Length)
        Array.Clear(Me.PoolForceDiscardProp, 0, Me.PoolForceDiscardProp.Length)

        Me.InitForcedDiscards()

    End Sub

    <Obsolete("Please use nTimeSeries instead")>
    Public Property nNumTimeSeries As Integer
        Get
            Return Me.nTimeSeries
        End Get
        Set(value As Integer)
            Me.nTimeSeries = value
        End Set
    End Property

    Friend Sub RedimTimeSeriesDatasets()

        ReDim Me.iDatasetDBID(nDatasets)
        ReDim Me.strDatasetNames(nDatasets)
        ReDim Me.strDatasetDescription(nDatasets)
        ReDim Me.strDatasetAuthor(nDatasets)
        ReDim Me.strDatasetContact(nDatasets)
        ReDim Me.nDatasetFirstYear(nDatasets)
        ReDim Me.nDatasetNumPoints(nDatasets)
        ReDim Me.nDatasetNumTimeSeries(nDatasets)

        ReDim Me.DataSetIntervals(nDatasets)

    End Sub

    Friend Sub ClearTimeSeries()

        Me.nTimeSeries = 0
        Me.nMaxYears = 0
        Me.nDatPoints = 0
        Me.NdatType = 0

        Me.RedimTimeSeries()
        Me.RedimEnabledTimeSeries()

        'jb 16-Feb-2010 Fixed bug that caused RedimForcingData() to throw a redim preserve exception when loading a second model that contained more groups
        'RedimForcingData() tries to preserve existing data if the number of timesteps changed  
        'this clears out the existing forcing data which forces RedimForcingData() to allocate new memory instead of trying to preserve the existing values
        Erase Me.PoolForceBB
        Erase Me.PoolForceCatch
        Erase Me.PoolForceZ

        Erase Me.PoolForceDiscardProp
        Erase Me.PoolForceDiscardMort

    End Sub

    Friend Sub RedimTimeSeries()

        Debug.Assert(nTimeSeries >= 0, Me.ToString & ".RedimTimeSeries() nNumTimeSeries cannot be negative")
        Debug.Assert(nMaxYears >= 0, Me.ToString & ".RedimTimeSeries() NdatYear cannot be negative")

        ' Redim interface time series arrays
        ReDim iTimeSeriesDBID(nTimeSeries)
        ReDim strName(nTimeSeries)
        ReDim bEnable(nTimeSeries)
        ReDim iPool(nTimeSeries)
        ReDim iPoolSec(nTimeSeries)
        ReDim sWeight(nTimeSeries)
        ReDim sCV(nTimeSeries)
        ReDim TimeSeriesType(nTimeSeries)
        ReDim sValues(nMaxYears + 1, nTimeSeries)
        ReDim sDatSS(nTimeSeries)
        ReDim sSSPredErr(nTimeSeries)

        ReDim sDatQ(nTimeSeries)
        ReDim sEDatQ(nTimeSeries)

        ReDim DatSS(nTimeSeries)
        ReDim DatQ(nTimeSeries)
        ReDim eDatQ(nTimeSeries)
        ReDim SSPredErr(nTimeSeries)

    End Sub

    Public Sub RedimEnabledTimeSeries()

        Debug.Assert(NdatType >= 0, Me.ToString & ".RedimAppliedTimeSeries() NdatType cannot be negative")
        Debug.Assert(nDatPoints >= 0, Me.ToString & ".RedimAppliedTimeSeries() NdatYear cannot be negative")

        ' Redim applied time series arrays
        ReDim DatPool(NdatType)
        ReDim DatPoolSec(NdatType)
        ReDim DatType(NdatType)
        ReDim WtType(NdatType)
        ReDim DatSS(NdatType)
        ReDim DatQ(NdatType)
        ReDim eDatQ(NdatType)

        ReDim DatYear(nDatPoints)
        ReDim DatVal(nDatPoints + 1, NdatType)

    End Sub

    ''' <summary>
    ''' Is there reference data for this model timestep
    ''' </summary>
    ''' <param name="iCumTimeStep"></param>
    ''' <param name="iMonth"></param>
    ''' <param name="iYear"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function HasData(ByVal iCumTimeStep As Integer, ByVal iMonth As Integer, ByVal iYear As Integer) As Boolean
        Dim breturn As Boolean = False

        If Me.DataSetInterval = eTSDataSetInterval.Annual Then
            If iYear <= Me.nYears And iMonth = ANNUAL_DATA_MONTH Then
                'Annual data
                'Within the time period
                'On the 6 month
                breturn = True
            End If

        ElseIf Me.DataSetInterval = eTSDataSetInterval.TimeStep Then

            If iCumTimeStep <= Me.nDatPoints Then
                breturn = True
            End If
        End If

        Return breturn

    End Function

    ''' <summary>
    ''' Get the reference data timestep index for this model timestep
    ''' </summary>
    ''' <param name="iIndexToSet"> Index of the data in DatVal(,)</param>
    ''' <param name="iCumTimeStep">Model cumulative timestep 1 - n timestep</param>
    ''' <param name="iMonth">Model month 1 - 12</param>
    ''' <param name="iYear">Model year 1 - n years</param>
    ''' <returns>True if there is reference data for this model timestep</returns>
    ''' <remarks></remarks>
    Public Function setRefDataIndex(ByRef iIndexToSet As Integer, ByVal iCumTimeStep As Integer,
                                    ByVal iMonth As Integer, ByVal iYear As Integer) As Boolean
        Dim breturn As Boolean = False
        iIndexToSet = cCore.NULL_VALUE

        If Me.HasData(iCumTimeStep, iMonth, iYear) Then
            If Me.DataSetInterval = eTSDataSetInterval.Annual Then
                'Set the time step index to the data
                iIndexToSet = iYear
                breturn = True

            ElseIf Me.DataSetInterval = eTSDataSetInterval.TimeStep Then

                iIndexToSet = iCumTimeStep
                breturn = True
            End If
        End If

        Return breturn

    End Function

    ''' <summary>
    ''' Set whether a given group is biomass forced through time series.
    ''' </summary>
    ''' <param name="IsBiomassForced"></param>
    Public Sub SetBiomassForcing(IsBiomassForced() As Boolean)

        Try
            ' Abort if not initialized properly
            If (IsBiomassForced.Length <> Me.nGroups + 1) Then Return

            ' Set all group forcing to false
            Array.Clear(IsBiomassForced, 0, IsBiomassForced.Length)

            ' Abort if no time series loaded
            If (PoolForceBB Is Nothing) Then Return

            For igrp As Integer = 1 To Me.nGroups
                Dim iDatPt As Integer = 1
                While Not IsBiomassForced(igrp) And iDatPt <= Me.nDatPoints
                    IsBiomassForced(igrp) = (Me.PoolForceBB(igrp, iDatPt) > 0)
                    iDatPt += 1
                End While
            Next igrp

        Catch ex As Exception
            Debug.Assert(False, Me.ToString + ".SetBiomassForcing() something went really wrong!")
            cLog.Write(ex, "cTimeSeriesDataStructures.SetBiomassForcing()")
        End Try

    End Sub

    ''' <summary>
    ''' Set whether a given group is discard forced through time series.
    ''' </summary>
    ''' <param name="IsDiscardForced"></param>
    Public Sub SetDiscardForcing(IsDiscardForced() As Boolean)

        Try
            ' Abort if not initialized properly
            If (IsDiscardForced.Length <> Me.nGroups + 1) Then Return

            ' Set all group forcing to false
            Array.Clear(IsDiscardForced, 0, IsDiscardForced.Length)

            ' Abort if no time series loaded
            If (PoolForceDiscardMort Is Nothing) Then Return

            For igrp As Integer = 1 To Me.nGroups
                Dim iflt As Integer = 1
                While Not IsDiscardForced(igrp) And iflt <= nFleets
                    Dim iDatPt As Integer = 1
                    While Not IsDiscardForced(igrp) And iDatPt <= Me.nDatPoints
                        IsDiscardForced(igrp) = (Me.PoolForceDiscardMort(iflt, igrp, iDatPt) > 0) Or (Me.PoolForceDiscardProp(iflt, igrp, iDatPt) > 0)
                        iDatPt += 1
                    End While
                    iflt += 1
                End While
            Next igrp

        Catch ex As Exception
            Debug.Assert(False, Me.ToString + ".SetBiomassForcing() something went really wrong!")
            cLog.Write(ex, "cTimeSeriesDataStructures.SetBiomassForcing()")
        End Try

    End Sub

    ''' <summary>
    ''' Redim time series forcing data PoolForceBB(nGroups, nYears),PoolForceZ(nGroups, nYears) and PoolForceCatch(nGroups, nYears)
    ''' </summary>
    ''' <param name="RunLengthYears">Ecosim run length in years</param>
    ''' <remarks></remarks>
    Public Sub redimForcingData(RunLengthYears As Integer)

        Try
            'What is the max number of datapoints that will be needed for this Ecosim run length
            'If the Ecosim run length is greater than the forcing data
            'Then we need to increase the number of forcing data points 
            'leaving the extra data points with zeros/no data
            Dim npoints As Integer
            If Me.DataSetInterval = eTSDataSetInterval.TimeStep Then
                npoints = Math.Max(Me.nDatPoints, RunLengthYears * cCore.N_MONTHS)
            Else
                npoints = Math.Max(Me.nDatPoints, RunLengthYears)
            End If

            If PoolForceBB Is Nothing Then
                'This is a first time initialization of the forcing data
                'Create the arrays
                'Populate the discard arrays with -9999, not a valid data point
                ReDim PoolForceBB(nGroups, npoints)
                ReDim PoolForceZ(nGroups, npoints)
                ReDim PoolForceCatch(nGroups, npoints)

                ReDim ForcedFs(nGroups, RunLengthYears * cCore.N_MONTHS)

                For igrp As Integer = 0 To nGroups
                    For ipt As Integer = 0 To RunLengthYears * cCore.N_MONTHS
                        ForcedFs(igrp, ipt) = cCore.NULL_VALUE
                    Next
                Next

                ReDim PoolForceDiscardMort(nFleets, nGroups, npoints)
                ReDim PoolForceDiscardProp(nFleets, nGroups, npoints)

                Me.InitForcedDiscards()
                Return
            End If

            If npoints > Me.nDatPoints Then
                'number of years the model is running for is greater then the forcing data
                'preserve the existing forcing data 
                ReDim Preserve PoolForceBB(nGroups, npoints)
                ReDim Preserve PoolForceZ(nGroups, npoints)
                ReDim Preserve PoolForceCatch(nGroups, npoints)
                '   ReDim Preserve ForcedFs(nGroups, npoints)

                ReDim Preserve PoolForceDiscardMort(nFleets, nGroups, npoints)
                ReDim Preserve PoolForceDiscardProp(nFleets, nGroups, npoints)

                ReDim Preserve ForcedFs(nGroups, RunLengthYears * cCore.N_MONTHS)

                For igrp As Integer = 0 To nGroups
                    For ipt As Integer = nDatPoints * cCore.N_MONTHS + 1 To RunLengthYears * cCore.N_MONTHS
                        ForcedFs(igrp, ipt) = cCore.NULL_VALUE
                    Next
                Next

            End If

            If RunLengthYears > Me.m_SimData.NumYears Then
                'Special case 
                'The code has extended the Ecosim run length(in years) 
                'The Fishing Policy Search does this
                'Set the discard forcing data in the extended period to -9999, not valid data
                Dim n As Integer

                If Me.DataSetInterval = eTSDataSetInterval.TimeStep Then
                    n = Math.Max(Me.nDatPoints, Me.m_SimData.NumYears * cCore.N_MONTHS)
                Else
                    n = Math.Max(Me.nDatPoints, Me.m_SimData.NumYears)
                End If

                For iflt As Integer = 0 To nFleets
                    For igrp As Integer = 0 To nGroups

                        For ipt As Integer = n + 1 To npoints
                            PoolForceDiscardMort(iflt, igrp, ipt) = cCore.NULL_VALUE
                            PoolForceDiscardProp(iflt, igrp, ipt) = cCore.NULL_VALUE
                        Next

                    Next
                Next
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Apply all flagged time series to the Ecosim model.
    ''' </summary>
    Friend Sub loadEnabled(Optional ByVal iTSIndex As Integer = -1)

        Dim iTS As Integer = -1
        Dim iTSEnable As Integer = -1
        Dim bFound As Boolean = False

        ' Single TS index given?
        If (iTSIndex > 0) Then
            ' Try to reload applied data for a single TS
            iTSEnable = 0
            iTS = 0

            ' Determine Applied index 
            While iTS < Math.Min(iTSIndex, nTimeSeries)
                ' Try next
                iTS += 1
                ' Is an applied TS?
                If Me.bEnable(iTS) Then
                    ' #Yes: count it
                    iTSEnable += 1
                    ' Check if found
                    bFound = (iTSIndex = iTS)
                End If
            End While

            If bFound Then
                ' Sanity check
                If (iTSEnable <= NdatType) Then
                    Me.LoadEnabledTS(iTS, iTSEnable)
                    Return
                End If
            End If
        End If

        ' Default: reload all enabled
        Me.LoadEnabledTimeSeries()

    End Sub

    Protected Sub LoadEnabledTimeSeries()
        Dim iTS As Integer = 0
        Dim iYear As Integer = 0
        Dim iTSEnable As Integer = 0

        NdatType = 0
        nDatPoints = nMaxYears
        nAICTimeSeries = 0

        ' Determine no. of time series to enable
        For iTS = 1 To nTimeSeries
            If Me.bEnable(iTS) Then NdatType += 1
        Next iTS

        RedimEnabledTimeSeries()

        If nTimeSeries > 0 Then

            Dim dt As Double = 1
            If Me.DataSetInterval = eTSDataSetInterval.TimeStep Then dt = 1 / cCore.N_MONTHS
            DatYear(1) = Me.nDatasetFirstYear(Me.ActiveDatasetIndex)
            For iYear = 2 To nDatPoints
                'DatYear(iYear) = DatYear(iYear - 1) + 1
                'Year for each datum point
                DatYear(iYear) = CInt(Math.Truncate(DatYear(1) + (iYear - 1) * dt))
            Next

            For iTS = 1 To nTimeSeries
                If Me.bEnable(iTS) Then
                    iTSEnable += 1
                    Me.LoadEnabledTS(iTS, iTSEnable)

                    'count up the number of time series use for the AIC
                    If Me.UseForAIC(Me.DatType(iTSEnable)) Then
                        nAICTimeSeries += 1
                    End If

                End If
            Next iTS

        End If

    End Sub

    Private Sub LoadEnabledTS(ByVal iTS As Integer, ByVal iTSEnable As Integer)
        Debug.Assert(Me.bEnable(iTS))

        DatPool(iTSEnable) = iPool(iTS)
        DatPoolSec(iTSEnable) = iPoolSec(iTS)
        DatType(iTSEnable) = TimeSeriesType(iTS)
        WtType(iTSEnable) = sWeight(iTS)
        For iYear As Integer = 0 To nDatPoints
            DatVal(iYear, iTSEnable) = sValues(iYear, iTS)
        Next iYear

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a <see cref="eTimeSeriesType">timeseries type</see>
    ''' contributes to AIC calculations.
    ''' </summary>
    ''' <param name="TimeSeriesType"></param>
    ''' <returns>True if a timeseries type contributes to AIC Calculations</returns>
    ''' -----------------------------------------------------------------------
    Friend Function UseForAIC(TimeSeriesType As eTimeSeriesType) As Boolean
        Return (TimeSeriesType = eTimeSeriesType.BiomassAbs) Or
               (TimeSeriesType = eTimeSeriesType.BiomassRel) Or
               (TimeSeriesType = eTimeSeriesType.Catches) Or
               (TimeSeriesType = eTimeSeriesType.CatchesForcing) Or
               (TimeSeriesType = eTimeSeriesType.CatchesRel) Or
               (TimeSeriesType = eTimeSeriesType.TotalMortality)
    End Function

    Friend Sub Update()

        Dim iTS As Integer = 0
        Dim iTSenabled As Integer = 0

        For iTS = 1 To nTimeSeries
            If Me.bEnable(iTS) Then
                iTSenabled += 1 'DatSS and DatQ are indexed from one
                sDatSS(iTS) = DatSS(iTSenabled)
                sDatQ(iTS) = DatQ(iTSenabled)
                sEDatQ(iTS) = eDatQ(iTSenabled)
                sSSPredErr(iTS) = SSPredErr(iTSenabled)
            Else
                sDatSS(iTS) = 0.0!
                sDatQ(iTS) = 0.0!
                sEDatQ(iTS) = 0.0!
                sSSPredErr(iTS) = 0.0!
            End If
        Next iTS

    End Sub


    Public Sub LoadForcingData()
        'Forcing data is loaded from the database into the same data structures as the other time series data DatVal(ipoint,itype)
        'This allocates arrays for each forcing type PoolForceBB(group,point),PoolForceZ(group,point) and PoolForceCatch(group,point)
        'and loads the data from DatVal(ipoint,itype) into the arrays used by the core
        Try
            'redimForcingData() will expand the forcing data to cover the number of ecosim years
            'while preserving the currently loaded data
            Me.redimForcingData(Me.m_SimData.NumYears)

            Me.InitForcedDiscards()
            'Load the data from DatVal(ipoint,itype) into the core arrays PoolForceBB(group,point)...
            Me.DoDatValCalculations()

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Load data from datval() into forcing arrays used by the models. Calculate the 
    ''' </summary>
    ''' <remarks>This needs to be called after the time series data is loaded to update other data arrays.</remarks>
    Public Sub DoDatValCalculations()

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'CAUTION
        'jb Ecosim.SetFFromGear() needs to be call after this 
        'this works now because SetFFromGear() gets called when ecosim is initialized after the scenario is loaded
        'if this is moved to the interface SetFFromGear() will no longer be called
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        Dim bDisFailedValidation As Boolean = False
        Dim iDatPt As Integer
        Dim iDType As Integer
        Dim K As Integer
        Dim Tim As Integer
        Dim ig As Integer
        'Dim ip As Integer
        Dim HoldIobs As Integer
        HoldIobs = Iobs
        Iobs = 0

        'clear out the FishForced flag
        Me.m_SimData.clearFishForced()

        Me.ClearForcing()

        Try

            For iDatPt = 1 To Me.nDatPoints
                For iDType = 1 To NdatType
                    Select Case DatType(iDType)

                        Case eTimeSeriesType.BiomassRel

                            If DatVal(iDatPt, iDType) > 0 Then Iobs = Iobs + 1
                            'PoolForceBB(DatPool(j), i) = 0

                        Case eTimeSeriesType.BiomassAbs

                            If DatVal(iDatPt, iDType) > 0 Then Iobs = Iobs + 1
                            PoolForceBB(DatPool(iDType), iDatPt) = 0

                        Case eTimeSeriesType.BiomassForcing 'pool biomass forcing

                            PoolForceBB(DatPool(iDType), iDatPt) = DatVal(iDatPt, iDType)


                        Case eTimeSeriesType.TimeForcing 'time forcing data
                            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                            'jb Time forcing data (Shapes) are handled through the Shape manager and not loaded with the Time series in EwE6
                            'this is the code from EwE5
                            '        If DatPool(j) > ForcingShapes + 3 Then
                            '            ForcingShapes = DatPool(j) - 3
                            'ReDim Preserve ForcingTitle(ForcingShapes) As String
                            'ReDim Preserve SeasonTitle(3) As String
                            'ReDim Preserve zscale(ForcePoints, ForcingShapes + 3) As Single
                            '            ReDim Preserve tval(ForcingShapes + 3)
                            '        End If
                            'If DatPool(j) > 3 And DatPool(j) <= ForcingShapes + 3 Then 'a valid long term shape
                            '    ForcingTitle(DatPool(j) - 3) = DatName(j)
                            '    For K = 1 To 12
                            '        Tim = 12 * (DatYear(i) - DatYear(1)) + K    ': If Tim > 1200 Then Tim = 1200
                            '        zscale(Tim, DatPool(j)) = DatVal(i, j)
                            '    Next
                            'End If
                            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        Case eTimeSeriesType.FishingEffort 'effort data by gear type

                            If DatPool(iDType) > 0 And DatPool(iDType) <= Me.nFleets Then
                                If Me.DataSetInterval = eTSDataSetInterval.Annual Then
                                    For K = 1 To 12
                                        Tim = 12 * (DatYear(iDatPt) - DatYear(1)) + K    ': If Tim > 1200 Then Tim = 1200
                                        ig = DatPool(iDType)
                                        Me.m_SimData.FishRateGear(ig, Tim) = DatVal(iDatPt, iDType)
                                    Next
                                ElseIf Me.DataSetInterval = eTSDataSetInterval.TimeStep Then

                                    Me.m_SimData.FishRateGear(DatPool(iDType), iDatPt) = DatVal(iDatPt, iDType)

                                End If
                            End If

                        Case eTimeSeriesType.FishingMortality 'F by pool

                            If DatPool(iDType) >= 0 And DatPool(iDType) <= nGroups Then
                                Me.m_SimData.FisForced(DatPool(iDType)) = True

                                If Me.DataSetInterval = eTSDataSetInterval.Annual Then
                                    For K = 1 To 12
                                        Tim = 12 * (DatYear(iDatPt) - DatYear(1)) + K

                                        Me.ForcedFs(DatPool(iDType), Tim) = DatVal(iDatPt, iDType)

                                        Me.m_SimData.FishRateNo(DatPool(iDType), Tim) = DatVal(iDatPt, iDType)
                                        If Me.m_SimData.FishRateMax(DatPool(iDType)) < Me.m_SimData.FishRateNo(DatPool(iDType), Tim) Then
                                            Me.m_SimData.FishRateMax(DatPool(iDType)) = CSng(Me.m_SimData.FishRateNo(DatPool(iDType), Tim) * 1.01)
                                        End If
                                    Next

                                ElseIf Me.DataSetInterval = eTSDataSetInterval.TimeStep Then

                                    Me.ForcedFs(DatPool(iDType), iDatPt) = DatVal(iDatPt, iDType)

                                    Me.m_SimData.FishRateNo(DatPool(iDType), iDatPt) = DatVal(iDatPt, iDType)
                                    If Me.m_SimData.FishRateMax(DatPool(iDType)) < Me.m_SimData.FishRateNo(DatPool(iDType), iDatPt) Then
                                        Me.m_SimData.FishRateMax(DatPool(iDType)) = CSng(Me.m_SimData.FishRateNo(DatPool(iDType), iDatPt) * 1.01)
                                    End If

                                End If

                            End If

                        Case eTimeSeriesType.TotalMortality, eTimeSeriesType.ConstantTotalMortality 'Z by pool

                            If Math.Abs(DatVal(iDatPt, iDType)) > 0 Then Iobs = Iobs + 1 'now also with forced Z
                            If DatType(iDType) = eTimeSeriesType.ConstantTotalMortality Then
                                PoolForceZ(DatPool(iDType), iDatPt) = DatVal(iDatPt, iDType)

                            Else
                                PoolForceZ(DatPool(iDType), iDatPt) = 0
                            End If

                        Case eTimeSeriesType.Catches, eTimeSeriesType.CatchesForcing, eTimeSeriesType.CatchesRel  'Catches, -6 is forced
                            If Math.Abs(DatVal(iDatPt, iDType)) > 0 Then Iobs = Iobs + 1 '....Added by SM for Catch Fitting.
                            If DatType(iDType) = eTimeSeriesType.CatchesForcing Then
                                PoolForceCatch(DatPool(iDType), iDatPt) = DatVal(iDatPt, iDType)
                            Else
                                PoolForceCatch(DatPool(iDType), iDatPt) = 0
                            End If

                            'Martell playing here!
                        Case eTimeSeriesType.AverageWeight 'Mean Body Weight data for split pool groups
                            'jb EwE6 does not have split pools! I'm not sure if this also applies to multi stanza groups??
                            If DatVal(iDatPt, iDType) > 0 Then Iobs = Iobs + 1

                        Case eTimeSeriesType.DiscardMortality

                            Dim value As Single = DatVal(iDatPt, iDType)
                            If value > 1.0 Then
                                value = 1.0
                                bDisFailedValidation = True
                            End If
                            PoolForceDiscardMort(DatPool(iDType), DatPoolSec(iDType), iDatPt) = value

                        Case eTimeSeriesType.DiscardProportion

                            Dim value As Single = DatVal(iDatPt, iDType)
                            If value > 1.0 Then
                                value = 1.0
                                bDisFailedValidation = True
                            End If
                            PoolForceDiscardProp(DatPool(iDType), DatPoolSec(iDType), iDatPt) = value

                        Case eTimeSeriesType.Discards, eTimeSeriesType.Landings
                            Iobs = Iobs + 1

                    End Select
                    '      End If 'If IsDatShown(j) = True Then
                Next
            Next
            iDType = 0
            For iDatPt = 1 To NdatType
                If WtType(iDatPt) > 0 Then iDType = iDType + 1
            Next

            'jb was????? 
            ' If ReadingCsvFile Or j = 0 Then
            If iDType = 0 Then
                For iDatPt = 1 To NdatType
                    If WtType(iDatPt) = 0 And (DatType(iDatPt) = 0 Or DatType(iDatPt) = 1 Or DatType(iDatPt) = 5 Or
                                               Math.Abs(DatType(iDatPt)) = 6 Or DatType(iDatPt) = 7) Then WtType(iDatPt) = 1
                Next
            End If

            If Iobs = 0 Then Iobs = HoldIobs
            ReDim Wt(Iobs)

            If bDisFailedValidation Then
                cLog.Write("Time series Discard Mortality Rate or Discard Proportion contained values > 1.0. These values cap a 1.0")
            End If

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'CAUTION
            'jb Ecosim.SetFFromGear() needs to be called after this 
            'this works now because SetFFromGear() gets called when ecosim is initialized after the scenario is loaded
            'if this is moved to the interface SetFFromGear() will no longer be called
            'EwE5 reset fishing rates by group to values predicted from effort except for forced groups
            ' SetFFromGear()
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
            Throw New ApplicationException(Me.ToString & ".DoDatValCalculations(). ", ex)
        End Try

    End Sub

    ''' <summary>
    ''' Is there a timeseries of a data type loaded for a group/fleet
    ''' </summary>
    ''' <param name="TSDataType">Type of data to test for</param>
    ''' <param name="iGroupIndex">Index of Group or Fleet</param>
    ''' <returns>True if there is data loaded for this datatype, group</returns>
    Friend Function DataLoadedForTypeGroup(ByVal TSDataType As eTimeSeriesType, ByVal iGroupIndex As Integer) As Boolean

        Try
            For its As Integer = 1 To Me.NdatType
                If Me.DatType(its) = TSDataType Then
                    If Me.DatPool(its) = iGroupIndex Then
                        Return True
                    End If
                End If
            Next
            Return False
        Catch ex As Exception
            cLog.Write(ex)
        End Try

        Return False

    End Function


    Private Sub InitForcedDiscards()

        Dim nSimPoints As Integer
        If Me.DataSetInterval = eTSDataSetInterval.TimeStep Then
            nSimPoints = Math.Max(Me.nDatPoints, Me.m_SimData.NumYears * cCore.N_MONTHS)
        Else
            nSimPoints = Math.Max(Me.nDatPoints, Me.m_SimData.NumYears)
        End If

        'jb 27-Oct-2016 I'm not sure about this 
        'set all points past the reference data to the default Ecopath values!
        For iflt As Integer = 0 To nFleets
            For igrp As Integer = 0 To nGroups

                For ipt As Integer = 0 To nSimPoints
                    PoolForceDiscardMort(iflt, igrp, ipt) = cCore.NULL_VALUE
                    PoolForceDiscardProp(iflt, igrp, ipt) = cCore.NULL_VALUE
                Next

            Next
        Next

    End Sub



End Class


#Region "Obsolete "

#If 0 Then


'jb 12-July-2016 Removed the cEcospaceTimeSeriesDataStructures with the implementation of Ecosim biomass forcing time series in Ecospace
'Just use the Cores cTimeSeriesDataStructures object until we need something more advanced  
''' <summary>
''' Time series reference data for Ecospace
''' </summary>
''' <remarks></remarks>
Public Class cEcospaceTimeSeriesDataStructures
    Inherits cTimeSeriesDataStructures

    ' ------------------------------------------------
    ' Interface structures
    ' ------------------------------------------------
    Public iSPRegion() As Integer

    ' ------------------------------------------------
    ' Applied structures used by the models
    ' ------------------------------------------------
    Public SPRegion() As Integer


    Friend Overloads Sub RedimTimeSeries()
        MyBase.RedimTimeSeries()

        ReDim iSPRegion(nTimeSeries)

    End Sub


    Friend Overloads Sub RedimAppliedTimeSeries()
        MyBase.RedimEnabledTimeSeries()

        ReDim iSPRegion(NdatType)

    End Sub

    ''' <summary>
    ''' EwE5 DoSpaceDatValCalculations
    ''' </summary>
    ''' <remarks></remarks>
    Friend Overloads Sub DoDatValCalculations(ByRef EcospaceData As cEcospaceDataStructures)

    End Sub

    ''' <summary>
    ''' Enable all flagged time series to the Ecosim model.
    ''' </summary>
    Friend Overloads Sub Apply(ByRef EcospaceData As cEcospaceDataStructures)

        'load the the user selected data into the data used by the model
        MyBase.LoadEnabledTimeSeries()

        'load the data from datval() into the ecosim data 
        DoDatValCalculations(EcospaceData)

    End Sub

End Class

#End If

#End Region
