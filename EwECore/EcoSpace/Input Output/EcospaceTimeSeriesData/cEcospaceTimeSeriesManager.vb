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
Option Explicit On
Imports EwEUtils
Imports EwEUtils.Utilities
Imports EwEUtils.Core



Namespace EcospaceTimeSeries

    Public Enum eTimeSeriesRecValidations As Integer
        isReadValid = 0
        MalformedString
        InvalidDateFormat
        EmptyRec
    End Enum

    Public Class cEcospaceTimeSeriesManager

        'ToDo 27-July-2016 Add Error messages back to the core instead of just Asserts
        '   Done 28-July-2016 Added core messages if read throws an Exception 
        '   Done 28-July-2016 Also sends message if dates or extents are out of bounds
        '   Done 29-Jul-2016 Validates records and sends message

        'ToDo 29-July-2016 Added message strings to resources
        '   Done Ongoing 9-Aug-2016 there still may be more messages

        'ToDo 27-July-2016  Document the file formats (input and output) and how it works
        '   Done 3-Aug-2016

        'ToDo 27-July-2016 Let the user selected the output file name. 
        '   Maybe when the user is selecting the input file have them choose the output file
        '   Use the default filename
        '   Done 3-Aug-2016 the user selects the output file when selecting the input file
        '       A default is supplied.
        '   Added a UI to show the user what file is currently loaded
        '   Default output file uses core output path
        '   Done?? 9-Aug-2016 Output file can be set from the UI

        'ToDo 27-July-2016 Added SS output to the UI. Results form... Main Run UI some place?

        'ToDo 27-July-2016 Added Group SS output to Results form

        'ToDo 27-July-2016 remove the DebugDump
        '   Done 29-Jul-2016 

        'ToDo: Complete intellisense XML code comments


#Region "Private data"


        '  Private m_dcDataByDate As Dictionary(Of Date, List(Of cEcospaceTimeSeriesRec))
        Private m_core As cCore
        Private m_SpaceData As cEcospaceDataStructures

        'sum of squares by group
        Private m_ss() As Double

        'Naming convection for SS variables follows
        'EwE5 and Ecosim.AccumulateDataInfo() and PlotDataInfo()

        'stored log error, one record for each cell/timestep
        'log(obs/pred)
        Private Erpred As List(Of Double)

        'sum of log error
        'sumof(log(obs/pred))
        Private DatSumZ As Double

        'squared sum of log error
        'sumof(log(obs/pred)^2)
        Private DatSumZ2 As Double

        Private m_BiomassFileName As String
        Private m_ContamFileName As String
        Private m_OutputFilename As String


        ' Private m_DataTable As DataTable
        Private m_dataSets As DataSet

#End Region

#Region " Public data/properties "

        Public Property TimeStepFormatString As String = "yyyy-MM-dd"

        Public Property BiomassInputFileName As String
            Get
                Return Me.m_BiomassFileName
            End Get
            Set(value As String)
                'Only send out notifications when needed
                If (String.Compare(Me.m_BiomassFileName, value, True) <> 0) Then
                    Me.m_BiomassFileName = value
                    Me.m_core.Messages.SendMessage(New cMessage(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_OUTPUT_SET,
                                                       EwEUtils.Core.eMessageType.DataModified, EwEUtils.Core.eCoreComponentType.EcoSpace,
                                                       EwEUtils.Core.eMessageImportance.Information))
                End If
            End Set
        End Property


        Public ReadOnly Property ContaminantInputFileName As String
            Get
                Return Me.m_ContamFileName
            End Get
        End Property

        Public Property OutputFileName As String
            Get
                If (String.IsNullOrWhiteSpace(Me.m_OutputFilename)) Then Return Me.getDefaultOutputFileName(Me.BiomassInputFileName)
                Return Me.m_OutputFilename
            End Get
            Set(value As String)
                'Only send out notifications when needed
                If (String.Compare(Me.OutputFileName, value, True) <> 0) Then
                    Me.m_OutputFilename = value
                    Me.m_core.Messages.SendMessage(New cMessage(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_OUTPUT_SET,
                                                       EwEUtils.Core.eMessageType.DataModified, EwEUtils.Core.eCoreComponentType.EcoSpace,
                                                       EwEUtils.Core.eMessageImportance.Information))
                End If
            End Set
        End Property

#End Region ' Public data/properties



#Region "Construction Initialization"


        Public Sub New(Core As cCore, EcospaceData As cEcospaceDataStructures)

            Me.m_core = Core
            Me.m_SpaceData = EcospaceData

            'Create a new list of cEcospaceTimeSeriesRec
            ' Me.m_dcDataByDate = New Dictionary(Of Date, List(Of cEcospaceTimeSeriesRec))

            Me.m_dataSets = New DataSet()


        End Sub


        Public Sub InitForRun()

            Try

                'Clear out the results
                Me.m_ss = New Double(Me.m_core.nGroups) {}
                Erpred = New List(Of Double)

                Me.DatSumZ = 0.0
                Me.DatSumZ2 = 0.0

                ''Clear out the results part of the cEcospaceTimeSeriesRec objects
                'For Each recs As List(Of cEcospaceTimeSeriesRec) In Me.m_dcDataByDate.Values
                '    For Each rec As cEcospaceTimeSeriesRec In recs
                '        rec.ClearResults()
                '    Next
                'Next

            Catch ex As Exception

            End Try

        End Sub


        Private Sub InitForRead(VarName As eVarNameFlags)

            If Not Me.m_dataSets.Tables.Contains(VarName.ToString) Then
                Me.AddTable(VarName)
            End If

            'Create a new list of cEcospaceTimeSeriesRec
            ' Me.m_dcDataByDate = New Dictionary(Of Date, List(Of cEcospaceTimeSeriesRec))
        End Sub

        Private Sub AddTable(varName As eVarNameFlags)
            Dim table As DataTable
            table = New DataTable(varName.ToString)

            table.Columns.Add("Date", GetType(Date))
            table.Columns.Add("Rec", GetType(Object))

            m_dataSets.Tables.Add(table)
        End Sub

        Public Sub Clear()
            Me.m_dataSets.Tables.Clear()
            ' Me.m_dcDataByDate.Clear()
            Me.m_BiomassFileName = String.Empty
            Me.m_OutputFilename = String.Empty
            Me.m_ContamFileName = String.Empty
        End Sub

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Added a new cEcospaceTimeSeriesRec record. 
        ''' </summary>
        ''' <param name="TimeSeriesRec"></param>
        ''' <returns>True if successful.</returns>
        Public Function Add(TimeSeriesRec As cEcospaceTimeSeriesRec) As Boolean

            Try
                ''Add TimeSeriesRec to the list of cEcospaceTimeSeriesRec objects
                ''cEcospaceTimeSeriesRec are stored by date, all the recs with the same date will be in one list
                ''Me.ByDate(date,CreateNew:=True) will create a new list if it doesn't already exist
                'Me.RecsByDate(TimeSeriesRec.TimeStamp, CreateNew:=True).Add(TimeSeriesRec)

                Me.m_dataSets.Tables(TimeSeriesRec.VarType.ToString).Rows.Add(TimeSeriesRec.TimeStamp, TimeSeriesRec)
            Catch ex As Exception
                EwEUtils.Core.cLog.Write(ex, "Failed to add Ecospace time series record.")
                Return False
            End Try
            Return True
        End Function

        Public Sub Debug_DumpDataTableRows()

            For Each table As DataTable In Me.m_dataSets.Tables
                System.Console.WriteLine("----------------" + table.TableName + "----------------")
                For Each row As DataRow In table.Rows
                    Dim tsrec As cEcospaceTimeSeriesRec
                    tsrec = DirectCast(row("Rec"), cEcospaceTimeSeriesRec)
                    System.Console.WriteLine(tsrec.ToCSVString)
                Next
                System.Console.WriteLine("--------------------------------")
            Next


        End Sub

        ''' <summary>
        ''' Read the Ecospace time series XYZ formatted file 
        ''' </summary>
        ''' <param name="InputFilename"></param>
        ''' <returns>True if successful.</returns>
        Public Function Load(InputFilename As String, OutputFileName As String, VarName As eVarNameFlags) As Boolean
            Dim bReturn As Boolean = True

            If Not IO.File.Exists(InputFilename) Then
                System.Console.WriteLine(Me.ToString + ".Read() file does not exist!")
                Return False
            End If

            Me.setFileNames(InputFilename, OutputFileName, VarName)
            Me.InitForRead(VarName)

            Try
                Dim reader As New cEcospaceTimeSeriesXYZReader(InputFilename, Me)

                'Read will populate the managers list of time series records
                If reader.Read(VarName) Then
                    Me.checkDates(reader.StartDate, reader.EndDate)
                    Me.checkExtent(reader.MaxRow, reader.MaxCol)
                    bReturn = True
                End If 'If reader.Read() Then

            Catch ex As Exception
                'cEcospaceTimeSeriesXYZReader.Read() will throw the exception back here is there if there is an internal exception
                Me.m_core.Messages.AddMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_LOAD_EXCEPTION, ex.Message),
                                                            EwEUtils.Core.eMessageType.ErrorEncountered,
                                                            EwEUtils.Core.eCoreComponentType.EcoSpace, EwEUtils.Core.eMessageImportance.Warning))
                'Clear out any data that may been read
                Me.Clear()
                bReturn = False
            End Try

            If Me.ContainsData(VarName) Then


                Me.m_core.Messages.AddMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_NRECORDS_LOADED, Me.nRecords(VarName)),
                                                       EwEUtils.Core.eMessageType.DataModified, EwEUtils.Core.eCoreComponentType.EcoSpace,
                                                       EwEUtils.Core.eMessageImportance.Information))
            Else
                'No data read from file
                Me.m_core.Messages.AddMessage(New cMessage(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_LOAD_FAILED,
                                                       EwEUtils.Core.eMessageType.DataModified, EwEUtils.Core.eCoreComponentType.EcoSpace,
                                                       EwEUtils.Core.eMessageImportance.Warning))
                Me.Clear()
                bReturn = False

            End If

            Me.m_core.Messages.sendAllMessages()

            Return bReturn

        End Function

        ''' <summary>
        ''' Calculate stats for this time step 
        ''' </summary>
        ''' <param name="iTimeStep">Current model time step</param>
        ''' <param name="biomass">Predicted biomass</param>
        ''' <returns>True if successful.</returns>
        Public Function CalculateStats(iTimeStep As Integer, biomass(,,) As Single) As Boolean
            Dim zstat As Double
            Dim TimeStepDate As Date = Me.TimeStepToDate(iTimeStep)
            Try
                Dim table As DataTable = Me.m_dataSets.Tables(eVarNameFlags.EcospaceMapBiomass.ToString)
                Dim rows As DataRow() = table.Select("Date='" + TimeStepDate.ToLongDateString + "'")
                Dim Rec As cEcospaceTimeSeriesRec
                For Each row As DataRow In rows
                    Rec = DirectCast(row("Rec"), cEcospaceTimeSeriesRec)

                    'clear out any old values 
                    Rec.ClearResults()

                    If Me.isValid(biomass, Rec) Then

                        'log prediction error
                        zstat = Math.Log(Rec.CellValue / biomass(Rec.Row, Rec.Col, Rec.iGroupID))

                        'save the predicted and calculated SS values back into the record
                        Rec.PredictedValue = biomass(Rec.Row, Rec.Col, Rec.iGroupID)
                        Rec.PredError = zstat

                        'Debug.Assert(Not Double.IsNaN(zstat))
                        If Not Double.IsNaN(zstat) And Not Double.IsInfinity(zstat) Then
                            'By Group
                            Me.m_ss(Rec.iGroupID) += zstat ^ 2

                            Me.Erpred.Add(zstat)
                            Me.DatSumZ += zstat
                            Me.DatSumZ2 += zstat ^ 2
                        End If

                        'shouldn't happen!
                        Debug.Assert(Not Double.IsNaN(Me.DatSumZ2))
                    End If


                Next

                ''is there records for this model date
                'If Me.ContainsDate(TimeStepDate) Then

                '    'get a list of all the records for this date
                '    For Each Rec As cEcospaceTimeSeriesRec In Me.RecsByDate(TimeStepDate)

                '        'System.Console.WriteLine("Ecospace Timeseries group=" + Rec.iGroupID.ToString + ", Date=" + Rec.TimeStamp.ToShortDateString)

                '        'Trap errors for each record incase the validation missed something
                '        Try

                '            If Me.isValid(biomass, Rec) Then
                '                'log prediction error
                '                zstat = Math.Log(Rec.CellValue / biomass(Rec.Row, Rec.Col, Rec.iGroupID))

                '                'save the predicted and calculated SS values back into the record
                '                Rec.PredictedValue = biomass(Rec.Row, Rec.Col, Rec.iGroupID)
                '                Rec.PredError = zstat

                '                'Debug.Assert(Not Double.IsNaN(zstat))
                '                If Not Double.IsNaN(zstat) And Not Double.IsInfinity(zstat) Then
                '                    'By Group
                '                    Me.m_ss(Rec.iGroupID) += zstat ^ 2

                '                    Me.Erpred.Add(zstat)
                '                    Me.DatSumZ += zstat
                '                    Me.DatSumZ2 += zstat ^ 2
                '                End If

                '                'shouldn't happen!
                '                Debug.Assert(Not Double.IsNaN(Me.DatSumZ2))
                '            End If

                '        Catch ex As Exception
                '            'What to do if a data point throws an exception???
                '            System.Console.WriteLine(Me.ToString + ".CalculateStats() Invalid data point.")
                '        End Try

                '    Next Rec

                '    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                '    'for debugging
                '    'Me.dumpDebugData()
                '    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                'End If

            Catch ex As Exception
                'This shouldn't happen during normal execution!
                'If it does it's some kind of a programming error...Really...
                Debug.Assert(False, "Ecospace Time Series failed to calculate stats for timestep " + iTimeStep.ToString)
                EwEUtils.Core.cLog.Write(ex)
                Return False
            End Try

            Return True

        End Function


        Public Function ForceContaminantConcentrations(iTimeStep As Integer) As Boolean

            If Not Me.ContainsData(eVarNameFlags.Concentration) Then
                Return False
            End If

            Try
                'Clear current data from memory
                'Me.clearContaminantForcing()
                Dim TimeStepDate As Date = Me.TimeStepToDate(iTimeStep)

                Dim table As DataTable = Me.m_dataSets.Tables(eVarNameFlags.Concentration.ToString)
                Dim rows As DataRow() = table.Select("Date='" + TimeStepDate.ToLongDateString + "'")
                For Each row As DataRow In rows

                    Dim Rec As cEcospaceTimeSeriesRec = DirectCast(row("Rec"), cEcospaceTimeSeriesRec)
                    If Rec.Row <= Me.m_SpaceData.InRow And Rec.Col <= Me.m_SpaceData.InCol Then
                        Me.m_SpaceData.Ccell(Rec.Row, Rec.Col, Rec.iGroupID) = Rec.CellValue
                    End If

                Next

            Catch ex As Exception
                'This shouldn't happen during normal execution!
                'If it does it's some kind of a programming error...Really...
                Debug.Assert(False, "Ecospace Time Series failed to load contaminant concentration at timestep " + iTimeStep.ToString)
                EwEUtils.Core.cLog.Write(ex)
                Return False
            End Try

            Return True

        End Function


        Public Function CalculateStats_Old(iTimeStep As Integer, biomass(,,) As Single) As Boolean
            'Dim zstat As Double
            'Dim TimeStepDate As Date = Me.TimeStepToDate(iTimeStep)
            ' Try

            '    'is there records for this model date
            '    If Me.ContainsDate(TimeStepDate) Then

            '        'get a list of all the records for this date
            '        For Each Rec As cEcospaceTimeSeriesRec In Me.RecsByDate(TimeStepDate)

            '            'System.Console.WriteLine("Ecospace Timeseries group=" + Rec.iGroupID.ToString + ", Date=" + Rec.TimeStamp.ToShortDateString)

            '            'Trap errors for each record incase the validation missed something
            '            Try

            '                If Me.isValid(biomass, Rec) Then
            '                    'log prediction error
            '                    zstat = Math.Log(Rec.CellValue / biomass(Rec.Row, Rec.Col, Rec.iGroupID))

            '                    'save the predicted and calculated SS values back into the record
            '                    Rec.PredictedValue = biomass(Rec.Row, Rec.Col, Rec.iGroupID)
            '                    Rec.PredError = zstat

            '                    'Debug.Assert(Not Double.IsNaN(zstat))
            '                    If Not Double.IsNaN(zstat) And Not Double.IsInfinity(zstat) Then
            '                        'By Group
            '                        Me.m_ss(Rec.iGroupID) += zstat ^ 2

            '                        Me.Erpred.Add(zstat)
            '                        Me.DatSumZ += zstat
            '                        Me.DatSumZ2 += zstat ^ 2
            '                    End If

            '                    'shouldn't happen!
            '                    Debug.Assert(Not Double.IsNaN(Me.DatSumZ2))
            '                End If

            '            Catch ex As Exception
            '                'What to do if a data point throws an exception???
            '                System.Console.WriteLine(Me.ToString + ".CalculateStats() Invalid data point.")
            '            End Try

            '        Next Rec

            '        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            '        'for debugging
            '        'Me.dumpDebugData()
            '        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            '    End If

            'Catch ex As Exception
            '    'This shouldn't happen during normal execution!
            '    'If it does it's some kind of a programming error...Really...
            '    Debug.Assert(False, "Ecospace Time Series failed to calculate stats for timestep " + iTimeStep.ToString)
            '    EwEUtils.Core.cLog.Write(ex)
            '    Return False
            'End Try

            'Return True

        End Function


        Public ReadOnly Property SS As Double
            Get
                Return Me.DatSumZ2
            End Get
        End Property

        Public ReadOnly Property SSGroup(igrp As Integer) As Double
            Get
                Return Me.m_ss(igrp)
            End Get
        End Property

        Public Sub RunCompleted()
            Try
                Me.SaveResults()
            Catch ex As Exception

            End Try

            Me.Core.Messages.sendAllMessages()

        End Sub

        Friend ReadOnly Property Core As cCore
            Get
                Return Me.m_core
            End Get
        End Property

#End Region

#Region "Private Methods"

        Private Function isValid(biomass(,,) As Single, Rec As cEcospaceTimeSeriesRec) As Boolean

            Try

                If Not Rec.Row <= Me.m_SpaceData.InRow And Rec.Col <= Me.m_SpaceData.InCol Then
                    Return False
                End If

                If biomass(Rec.Row, Rec.Col, Rec.iGroupID) > 0.0 Then
                    Return True
                End If

            Catch ex As Exception
                Return False
            End Try

            'Failed
            Return False

        End Function


        Private Sub setFileNames(InputFileName As String, OutputFileName As String, VarName As eVarNameFlags)
            Select Case VarName
                Case eVarNameFlags.EcospaceMapBiomass
                    Me.m_BiomassFileName = InputFileName
                    Me.m_OutputFilename = OutputFileName
                Case eVarNameFlags.Concentration
                    Me.m_ContamFileName = InputFileName
            End Select
        End Sub


        '''' <summary>
        '''' Get a list of cEcospaceTimeSeriesRec objects for this date. 
        '''' If CreateNew = True add a new list and return it, if CreateNew = False return nothing.   
        '''' </summary>
        '''' <param name="RecDate"></param>
        '''' <param name="CreateNew"></param>
        '''' <returns></returns>
        'Private Function RecsByDate(RecDate As Date, Optional CreateNew As Boolean = False) As List(Of cEcospaceTimeSeriesRec)
        '    If m_dcDataByDate.ContainsKey(RecDate) Then
        '        Return m_dcDataByDate.Item(RecDate)
        '    End If

        '    If CreateNew Then
        '        Dim recs As New List(Of cEcospaceTimeSeriesRec)
        '        m_dcDataByDate.Add(RecDate, recs)
        '        Return recs
        '    End If

        '    Return Nothing

        'End Function

        Public ReadOnly Property nRecords(VarName As eVarNameFlags) As Integer
            Get
                If Me.m_dataSets.Tables.Contains(VarName.ToString) Then
                    Return Me.m_dataSets.Tables(VarName.ToString).Rows.Count
                End If
                Return 0
            End Get
        End Property


        '''' <summary>
        '''' Is there Ecospace time series data loaded
        '''' </summary>
        '''' <returns>True if there is loaded data, False otherwise. Does not test the map bounds or dates.</returns>
        'Public ReadOnly Property ContainsData_list(VarName As eVarNameFlags) As Boolean
        '    Get
        '        If Me.m_dcDataByDate IsNot Nothing Then
        '            Return Me.m_dcDataByDate.Count > 0
        '        End If
        '        Return False
        '    End Get
        'End Property



        ''' <summary>
        ''' Is there Ecospace time series data loaded
        ''' </summary>
        ''' <returns>True if there is loaded data, False otherwise. Does not test the map bounds or dates.</returns>
        Public ReadOnly Property ContainsData(VarName As eVarNameFlags) As Boolean
            Get

                Try
                    If Me.m_dataSets.Tables.Contains(VarName.ToString) Then
                        Dim table As DataTable = Me.m_dataSets.Tables(VarName.ToString)
                        Return table.Rows.Count > 0
                    End If

                Catch ex As Exception

                End Try
                'If Me.m_dcDataByDate IsNot Nothing Then
                '    Return Me.m_dcDataByDate.Count > 0
                'End If
                Return False
            End Get
        End Property


        '''' <summary>
        '''' Does the currently loaded data contain this date
        '''' </summary>
        '''' <param name="RecDate"></param>
        '''' <returns></returns>
        'Private Function ContainsDate(RecDate As Date) As Boolean

        '    If m_dcDataByDate.ContainsKey(RecDate) Then
        '        Return True
        '    End If
        '    Return False

        'End Function



        ''' <summary>
        ''' Get the calendar date for the current model time step
        ''' </summary>
        ''' <param name="itimestep"></param>
        ''' <returns></returns>
        Private Function TimeStepToDate(itimestep As Integer) As Date
            'convert Ecospace time step into date
            Dim stYear As Integer
            If Me.m_core.EwEModel.FirstYear <> 0 Then
                stYear = Me.m_core.EwEModel.FirstYear
            Else
                stYear = 1
            End If

            Dim StartDate As New Date(stYear, 1, 1)
            Dim nmonths As Integer = CInt(Math.Truncate((itimestep - 1) * Me.m_SpaceData.TimeStep * 12))
            Dim tsDate As Date = StartDate.AddMonths(CInt(nmonths))
            Return tsDate

        End Function

        ''' <summary>
        ''' Are the start date and the end date of the input time series data within the model run
        ''' </summary>
        ''' <param name="StartDate"></param>
        ''' <param name="EndDate"></param>
        ''' <returns>True if any part of the dates are in bounds, False otherwise. </returns>
        Private Function checkDates(StartDate As Date, EndDate As Date) As Boolean
            Dim msg As New System.Text.StringBuilder
            Dim bReturn As Boolean = True

            If Me.m_core.EwEModel.FirstYear <> 0 Then
                Dim mSD As New Date(Me.m_core.EwEModel.FirstYear, 1, 1)
                Dim mED As New Date(CInt(Me.m_core.EwEModel.FirstYear + Me.m_SpaceData.TotalTime), 1, 1)
                If StartDate > mED Or EndDate < mSD Then
                    'Failed date bounds
                    msg.Append(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_LOAD_DATES, StartDate.ToShortDateString, EndDate.ToShortDateString))
                    bReturn = False

                End If
            Else 'Me.m_core.EwEModel.FirstYear <> 0
                'First year = 0 
                'The user has not set a model data
                msg.Append(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_LOAD_NO_DATE)
                bReturn = False
            End If

            If msg.Length > 0 Then
                Me.m_core.Messages.AddMessage(New cMessage(msg.ToString, EwEUtils.Core.eMessageType.DataValidation, EwEUtils.Core.eCoreComponentType.EcoSpace, EwEUtils.Core.eMessageImportance.Information))
            End If

            Return bReturn

        End Function


        ''' <summary>
        ''' Check the Extent of the input time series data against the current Ecospace map extent
        ''' </summary>
        ''' <param name="MaxRow"></param>
        ''' <param name="MaxCol"></param>
        ''' <returns>Return True if the row and col are inbounds, False otherwise.</returns>
        Private Function checkExtent(MaxRow As Integer, MaxCol As Integer) As Boolean

            If MaxRow > Me.m_SpaceData.InRow Or MaxCol > Me.m_SpaceData.InCol Then
                'Debug.Assert(False, "Oppss Time Series map exceeds the Ecospace map extent.")
                Dim msg As New System.Text.StringBuilder
                msg.Append(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_LOAD_MAP_EXTENTS)
                Me.m_core.Messages.AddMessage(New cMessage(msg.ToString, EwEUtils.Core.eMessageType.DataValidation, EwEUtils.Core.eCoreComponentType.EcoSpace, EwEUtils.Core.eMessageImportance.Information))
                Return False
            End If

            Return True

        End Function

        Private Sub SaveResults()

            If Not Me.ContainsData(eVarNameFlags.EcospaceMapBiomass) Then
                'nothing the save
                Exit Sub
            End If

            'build the output directory if it doesn't exist
            'if this fails the streamwriter will throw an error and the user will get an error message
            Utilities.cFileUtils.IsDirectoryAvailable(IO.Path.GetDirectoryName(Me.OutputFileName), True)

            Try
                Dim header As String = "Row,Col,GroupID,Date(yyyy-MM-dd),ObservedValue,PredictedValue,PredictionError(LogN(ObservedValue/PredictedValue)"
                Dim strm As New IO.StreamWriter(Me.OutputFileName)
                strm.WriteLine(Me.m_core.DefaultFileHeader(EwEUtils.Core.eAutosaveTypes.Ecospace))
                strm.WriteLine(header)

                Dim table As DataTable = Me.m_dataSets.Tables(eVarNameFlags.EcospaceMapBiomass.ToString)
                For Each row As DataRow In table.Rows
                    Dim rec As cEcospaceTimeSeriesRec = DirectCast(row("Rec"), cEcospaceTimeSeriesRec)
                    If rec.PredictedValue <> cCore.NULL_VALUE Then
                        strm.WriteLine(rec.ToCSVString)
                    End If
                Next

                strm.Close()

                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_RESULTS_SAVED, Me.OutputFileName),
                                                           EwEUtils.Core.eMessageType.DataExport,
                                                           EwEUtils.Core.eCoreComponentType.EcoSpace,
                                                           EwEUtils.Core.eMessageImportance.Information)
                msg.Hyperlink = Me.OutputFileName
                Me.m_core.Messages.AddMessage(msg)

            Catch ex As Exception
                EwEUtils.Core.cLog.Write(ex, Me.ToString + ".SaveResults() Exception")

                Dim ExMsg As New Text.StringBuilder
                ExMsg.Append(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_SAVE_EXCEPTION, ex.Message))

                Me.m_core.Messages.AddMessage(New cMessage(ExMsg.ToString, EwEUtils.Core.eMessageType.ErrorEncountered,
                   EwEUtils.Core.eCoreComponentType.EcoSpace, EwEUtils.Core.eMessageImportance.Warning))
            End Try

        End Sub



        'Private Sub SaveResults_old()

        '    If Not Me.ContainsData(eVarNameFlags.EcospaceMapBiomass) Then
        '        'nothing the save
        '        Exit Sub
        '    End If

        '    'build the output directory if it doesn't exist
        '    'if this fails the streamwriter will throw an error and the user will get an error message
        '    Utilities.cFileUtils.IsDirectoryAvailable(IO.Path.GetDirectoryName(Me.m_OutputFilename), True)

        '    Try
        '        Dim header As String = "Row,Col,GroupID,Date(yyyy-MM-dd),ObservedValue,PredictedValue,PredictionError(LogN(ObservedValue/PredictedValue)"
        '        Dim strm As New IO.StreamWriter(Me.m_OutputFilename)
        '        strm.WriteLine(Me.m_core.DefaultFileHeader(EwEUtils.Core.eAutosaveTypes.Ecospace))
        '        strm.WriteLine(header)
        '        For Each recs As List(Of cEcospaceTimeSeriesRec) In Me.m_dcDataByDate.Values
        '            For Each rec As cEcospaceTimeSeriesRec In recs
        '                If rec.PredictedValue <> cCore.NULL_VALUE Then
        '                    strm.WriteLine(rec.ToCSVString)
        '                End If
        '            Next
        '        Next

        '        strm.Close()

        '        Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_RESULTS_SAVED, Me.OuputFileName),
        '                                                   EwEUtils.Core.eMessageType.DataExport,
        '                                                   EwEUtils.Core.eCoreComponentType.EcoSpace,
        '                                                   EwEUtils.Core.eMessageImportance.Information)
        '        msg.Hyperlink = Me.OuputFileName
        '        Me.m_core.Messages.AddMessage(msg)

        '    Catch ex As Exception
        '        EwEUtils.Core.cLog.Write(ex, Me.ToString + ".SaveResults() Exception")

        '        Dim ExMsg As New Text.StringBuilder
        '        ExMsg.Append(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_SAVE_EXCEPTION, ex.Message))

        '        Me.m_core.Messages.AddMessage(New cMessage(ExMsg.ToString, EwEUtils.Core.eMessageType.ErrorEncountered,
        '           EwEUtils.Core.eCoreComponentType.EcoSpace, EwEUtils.Core.eMessageImportance.Warning))
        '    End Try

        'End Sub


        Public Function getDefaultOutputFileName(InputFileName As String) As String
            If (String.IsNullOrWhiteSpace(InputFileName)) Then Return ""
            Dim tempFileName As String = IO.Path.GetFileNameWithoutExtension(InputFileName) + "_Residuals.csv"
            Return IO.Path.Combine(Core.DefaultOutputPath(EwEUtils.Core.eAutosaveTypes.Ecospace), tempFileName)
        End Function


        Private Sub dumpDebugData()
            'dump data to console window for debugging
            System.Console.WriteLine("sum of log(obs/pred)=" + Me.DatSumZ2.ToString)
            For igrp As Integer = 1 To Me.m_core.nGroups
                If Me.m_ss(igrp) > 0 Then
                    System.Console.WriteLine("Group=" + igrp.ToString + ", SS=" + Me.m_ss(igrp).ToString)
                End If
            Next
        End Sub

#End Region

    End Class

End Namespace
