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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Option Strict On
Option Explicit On

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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Imports System.IO
Imports EwECore
Imports EwEUtils.Utilities
Imports LumenWorks.Framework.IO.Csv
Imports Troschuetz.Random

#End Region

Public Class cSurvivability
    Implements IMSEData

#Region " Internal Variables "

    ''' <summary>
    ''' Stores a list of all the information for each survivability distribution
    ''' </summary>
    Private mListofSuriveDistParams As List(Of cSurvivabilityDistributonParam)
    ''' <summary>
    ''' Stores a list of all the sampled survivabilities
    ''' </summary>
    ''' <remarks></remarks>
    Private m_ListOfSampledSurvivabilities As List(Of cSampledSurvivability)
    ''' <summary>
    ''' Reference to the EwE core
    ''' </summary>
    ''' <remarks></remarks>
    Private mcore As EwECore.cCore
    ''' <summary>
    ''' Reference to the MSE plugin
    ''' </summary>
    ''' <remarks></remarks>
    Private mMSE As cMSE
    Private mSimData As cEcosimDatastructures
    Private mPathData As cEcopathDataStructures
    ''' <summary>
    ''' Equals True if the survivability distribution parameters file exists
    ''' </summary>
    ''' <remarks></remarks>
    Private mSurvDistFileExists As Boolean
    ''' <summary>
    ''' Equals TRUE if the survivability distribution parameters file is formatted correctly
    ''' </summary>
    ''' <remarks></remarks>
    Private mSurvDistFileValid As Boolean
    ''' <summary>
    ''' Equals True if the sampled survivability parameters file exists
    ''' </summary>
    ''' <remarks></remarks>
    Private mSurvParamFileExists As Boolean
    ''' <summary>
    ''' Equals TRUE if the sampled survivability parameters file is formatted correctly
    ''' </summary>
    ''' <remarks></remarks>
    Private mSurvParamFileValid As Boolean

    Private mChangesToSave As Boolean

#End Region

#Region " Construction "

    Public Sub New(MSE As cMSE, core As EwECore.cCore, EcosimDataStructures As cEcosimDatastructures, EcopathDataStructures As cEcopathDataStructures)
        Me.mcore = core
        Me.mMSE = MSE
        Me.mPathData = EcopathDataStructures
        Me.mListofSuriveDistParams = New List(Of cSurvivabilityDistributonParam)
        Me.m_ListOfSampledSurvivabilities = New List(Of cSampledSurvivability)
        Me.Defaults()
    End Sub

#End Region

#Region " Internal Classes "

    ''' <summary>
    ''' Stores a single survivability probabability distribution
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cSurvivabilityDistributonParam

        Public Sub New()

        End Sub

        Public Sub New(ByVal Index As Integer, FleetNumber As Integer, ByVal GroupNumber As Integer, _
                       ByVal Alpha As Double, ByVal Beta As Double)
            Me.Index = Index
            Me.FleetNo = FleetNumber
            Me.GroupNo = GroupNumber
            Me.Alpha = Alpha
            Me.Beta = Beta

        End Sub

        Public Sub New(ByVal FleetNumber As Integer, ByVal GroupNumber As Integer, _
                       ByVal Alpha As Double, ByVal Beta As Double)
            Me.FleetNo = FleetNumber
            Me.GroupNo = GroupNumber
            Me.Alpha = Alpha
            Me.Beta = Beta
        End Sub

        Public Property Index As Integer
        Public Property FleetNo As Integer
        Public Property GroupNo As Integer
        Public Property Alpha As Double
        Public Property Beta As Double

    End Class

    ''' <summary>
    ''' Stores a single sampled survivability parameter
    ''' </summary>
    ''' <remarks></remarks>
    Private Class cSampledSurvivability

        Public Property Iteration As Integer
        Public Property FleetNo As Integer
        Public Property GroupNo As Integer
        Public Property Survivability As Single

        Public Sub New(ByVal Iteration As Integer, ByVal FleetNumber As Integer, ByVal GroupNumber As Integer, ByVal Survivability As Single)

            Me.Iteration = Iteration
            Me.FleetNo = FleetNumber
            Me.GroupNo = GroupNumber
            Me.Survivability = Survivability

        End Sub

    End Class

#End Region

#Region " Distribution Parameter Elements "

#Region " Properties "

    ''' <summary>
    ''' Returns the list of all the survivability distribution parameters
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property ListofSurvDistParams() As List(Of cSurvivabilityDistributonParam)
        Get
            Return mListofSuriveDistParams
        End Get
    End Property

    ''' <summary>
    ''' Returns the number of elements in the survivability distribution list
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property CountDist() As Integer
        Get
            Return mListofSuriveDistParams.Count
        End Get
    End Property

#End Region

#Region " Functions "

    ''' <summary>
    ''' Adds a distribution parameter to the list of survivability distribution parameters
    ''' and if it can't returns FALSE
    ''' </summary>
    ''' <param name="param"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AddDist(param As cSurvivabilityDistributonParam) As Boolean
        Dim nextIndex As Integer

        'Check Fleet Number
        If param.FleetNo < 0 Or param.FleetNo > mcore.nFleets Then Return False

        'Check Fleet Name
        'ValuesOkay = False
        'For iFleet = 1 To mcore.nFleets
        '    If mcore.FleetInputs(iFleet).Name = param.FleetName Then ValuesOkay = True
        'Next
        'If ValuesOkay = False Then Return False

        'Check GroupNo
        If param.GroupNo < 0 Or param.GroupNo > mcore.nGroups Then Return False

        'Check GroupName
        'ValuesOkay = False
        'For iGroup = 1 To mcore.nGroups
        '    If mcore.EcoPathGroupInputs(iGroup).Name = param.GroupName Then ValuesOkay = True
        'Next
        'If ValuesOkay = False Then Return False

        'Check Alpha and Beta
        If param.Alpha <= 0 Or param.Beta <= 0 Then Return False

        nextIndex = mListofSuriveDistParams.Count + 1
        mListofSuriveDistParams.Add(New cSurvivabilityDistributonParam(nextIndex, param.FleetNo, param.GroupNo, param.Alpha, param.Beta))

        Return True

    End Function

    ''' <summary>
    ''' Reads the iRow_th from the list of survivability distribution parameters
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <returns></returns>
    ''' <remarks>iRow is zero-based</remarks>
    Public Function ReadRowDist(iRow As Integer) As cSurvivabilityDistributonParam
        If iRow < 0 Then Return Nothing
        If iRow > mListofSuriveDistParams.Count - 1 Then Return Nothing
        Return mListofSuriveDistParams(iRow)
    End Function

    ''' <summary>
    ''' Reads the survivability distribution parameters for iGroup as discarded by iFleet
    ''' </summary>
    ''' <param name="iFleet"></param>
    ''' <param name="iGroup"></param>
    ''' <returns></returns>
    ''' <remarks>iFleet and iGroup are zero-based</remarks>
    Public Function ReadiFleetiGroupDist(iFleet As Integer, iGroup As Integer) As cSurvivabilityDistributonParam

        If iFleet < 1 Or iFleet > mcore.nFleets Then Return Nothing
        If iGroup < 1 Or iGroup > mcore.nGroups Then Return Nothing

        For iRow As Integer = 0 To mListofSuriveDistParams.Count - 1
            If mListofSuriveDistParams(iRow).FleetNo = iFleet And mListofSuriveDistParams(iRow).GroupNo = iGroup Then Return mListofSuriveDistParams(iRow)
        Next

        Return Nothing

    End Function

#End Region

#End Region

#Region " Sampled Parameter Elements "

#Region " Properties"

    ''' <summary>
    ''' Checks whether the Sampled Parameters file is valid
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ParamFileValid() As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Returns whether the sampled param file is valid
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property SurvParamFileValid As Boolean
        Get
            Return mSurvParamFileValid
        End Get
    End Property

    ''' <summary>
    ''' Returns whether the Sampled survivability file exists
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SurvivabilityParamFileExists() As Boolean
        Get
            Return mSurvParamFileExists
        End Get
        Set(ByVal value As Boolean)
            mSurvParamFileExists = value
        End Set
    End Property

#End Region

#Region " Functions "


    ''' <summary>
    ''' Samples the survivability parameters
    ''' </summary>
    ''' <param name="nParams">The number of models to generate</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SampleParams(nParams As Integer) As Boolean
        Dim TempSurvDistParam As cSurvivabilityDistributonParam
        Dim TempSampledParam As Single
        Dim BetaGenerator As New BetaDistribution

        Try
            For iParameter = 1 To nParams
                For iRow = 0 To mListofSuriveDistParams.Count - 1
                    TempSurvDistParam = Me.ReadRowDist(iRow)
                    BetaGenerator.Alpha = TempSurvDistParam.Alpha
                    BetaGenerator.Beta = TempSurvDistParam.Beta
                    TempSampledParam = Convert.ToSingle(BetaGenerator.NextDouble())
                    m_ListOfSampledSurvivabilities.Add(New cSampledSurvivability(iParameter, TempSurvDistParam.FleetNo, _
                                                                        TempSurvDistParam.GroupNo, _
                                                                        TempSampledParam))
                Next
            Next
        Catch ex As Exception
            Return False
        End Try

        Return True

    End Function

    ''' <summary>
    ''' Load the sampled parameters from CSV
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LoadSampledParamsFromCSV() As Boolean

        Dim reader As StreamReader = Nothing
        Dim csv As CsvReader = Nothing
        Dim param As cSampledSurvivability
        Dim bSuccess As Boolean = True
        Dim filePath As String = cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.ParamsOut, "Survivabilities_out.csv")

        If File.Exists(filePath) Then
            reader = cMSEUtils.GetReader(filePath)
            If (reader IsNot Nothing) Then
                Try
                    csv = New CsvReader(reader, True)
                    mSurvParamFileExists = False
                    While Not csv.EndOfStream
                        param = ExtractSampledParam(csv)

                        If (param IsNot Nothing) Then
                            m_ListOfSampledSurvivabilities.Add(New cSampledSurvivability(param.Iteration, param.FleetNo, param.GroupNo, _
                                                          param.Survivability))
                        End If
                    End While
                    csv.Dispose()

                Catch ex As Exception
                    Debug.Assert(False, Me.ToString & ".LoadEcosimParameters() Exception: " & ex.Message)
                    bSuccess = False
                End Try
            End If
            cMSEUtils.ReleaseReader(reader)
        Else
            Return bSuccess = False
        End If

        Return bSuccess

    End Function

    ''' <summary>
    ''' Extract a single line from the sampled survivability parameter file
    ''' </summary>
    ''' <param name="csv">The csv object that links to the file</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ExtractSampledParam(ByVal csv As CsvReader) As cSampledSurvivability
        ' Sanity checks
        If (csv Is Nothing) Then Return Nothing
        If (Not csv.ReadNextRecord()) Then Return Nothing

        Dim TIteration As Integer
        Dim TFleetNumber As Integer
        Dim TGroupNumber As Integer
        Dim TSurvivability As Single

        Try
            TIteration = cStringUtils.ConvertToInteger(csv(0))
            TFleetNumber = cStringUtils.ConvertToInteger(csv(1))
            TGroupNumber = cStringUtils.ConvertToInteger(csv(3))
            TSurvivability = cStringUtils.ConvertToSingle(csv(5))

        Catch ex As Exception
            ' ToDo_JS: respond to error
            Return Nothing
        End Try

        Return New cSampledSurvivability(TIteration, TFleetNumber, TGroupNumber, TSurvivability)


    End Function

    ''' <summary>
    ''' Saves the sampled survivabilities to CSV
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SaveSampledToCSV() As Boolean

        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.ParamsOut, "Survivabilities_out.csv"), False)

        Dim bSuccess As Boolean = False

        If (writer Is Nothing) Then Return bSuccess

        Try
            writer.WriteLine("Iteration,FleetNumber,FleetName,GroupNumber,GroupName,Survivability")

            For Each entry As cSampledSurvivability In m_ListOfSampledSurvivabilities
                writer.WriteLine(cStringUtils.ToCSVField(entry.Iteration) & "," &
                                 cStringUtils.ToCSVField(entry.FleetNo) & "," &
                                 cStringUtils.ToCSVField(mcore.EcopathFleetInputs(entry.FleetNo).Name) & "," &
                                 cStringUtils.ToCSVField(entry.GroupNo) & "," &
                                 cStringUtils.ToCSVField(mcore.EcoPathGroupInputs(entry.GroupNo).Name) & "," &
                                 cStringUtils.ToCSVField(entry.Survivability))
            Next

            bSuccess = True

        Catch ex As Exception

        End Try
        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess

    End Function

#End Region

#Region " Subroutines "

    Public Sub ConfigCoreWithSurvivabilities(ByVal iModel As Integer)
        ' TODO MP
        For iFleet = 1 To mcore.nFleets
            For iGroup = 1 To mcore.nGroups
                mPathData.PropDiscardMort(iFleet, iGroup) = 1 - GetSurvivability_FleetGroupModel(iFleet, iGroup, iModel)
            Next
        Next
    End Sub

#End Region

#End Region

#Region " General Elements "

    ' ''' <summary>
    ' ''' Runs when the MSE plugin has been loaded up
    ' ''' </summary>
    ' ''' <remarks></remarks>
    'Public Sub PluginLoaded()
    '    Dim reader As StreamReader = Nothing

    '    'Todo MP
    '    ' check file exists for surivability distribution parameters
    '    If Not File.Exists(cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "Survivabilities_dist.csv")) Then
    '        SurvivabilityDistFileExists = False
    '        mSurvDistFileValid = False
    '    Else
    '        ' check file is correct
    '        If Not DistFileValid() Then
    '            mSurvDistFileValid = False
    '        Else
    '            'If it is load the file into memory
    '            mSurvDistFileValid = True
    '            LoadDistFromCSV()
    '        End If
    '    End If

    '    If Not File.Exists(cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.ParamsOut, "Survivabilites_out.csv")) Then
    '        SurvivabilityParamFileExists = False
    '        mSurvParamFileValid = False
    '    Else
    '        If Not ParamFileValid() Then
    '            mSurvParamFileValid = False
    '        Else
    '            mSurvParamFileValid = True
    '            LoadParamFromCSV()
    '        End If
    '    End If

    'End Sub

#End Region

    Public ReadOnly Property EcopathData As cEcopathDataStructures
        Get
            Return Me.mPathData
        End Get
    End Property

    Public ReadOnly Property EcosimData As cEcosimDatastructures
        Get
            Return Me.mSimData
        End Get
    End Property

    Public Sub Defaults() Implements IMSEData.Defaults
        Me.ListofSurvDistParams.Clear()
    End Sub

    Private Function GetSurvivability_FleetGroupModel(iFleet As Integer, iGroup As Integer, iModel As Integer) As Single

        Const DefaultSurvivability = 0

        For Each iSurvivability In m_ListOfSampledSurvivabilities
            If iSurvivability.FleetNo = iFleet And iSurvivability.GroupNo = iGroup And iSurvivability.Iteration = iModel Then
                Return iSurvivability.Survivability
            End If
        Next

        Return DefaultSurvivability

    End Function


    Public Function IsChanged() As Boolean _
        Implements IMSEData.IsChanged
        Return False
    End Function

    Public Function Load(Optional msg As cMessage = Nothing, _
                         Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Load

        Dim reader As StreamReader = Nothing
        Dim csv As CsvReader = Nothing
        Dim param As cSurvivabilityDistributonParam = New cSurvivabilityDistributonParam()
        Dim bSuccess As Boolean = True

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFilePath()
        End If

        Me.Defaults()

        If File.Exists(strFilename) Then

            reader = cMSEUtils.GetReader(strFilename)
            If (reader IsNot Nothing) Then
                Try
                    csv = New CsvReader(reader, True)

                    While ExtractSurvivabilityDist(msg, csv, param)
                        Me.AddDist(param)
                    End While
                    csv.Dispose()

                Catch ex As Exception
                    'Debug.Assert(False, Me.ToString & ".LoadEcosimParameters() Exception: " & ex.Message)
                    bSuccess = False
                End Try
            End If
            cMSEUtils.ReleaseReader(reader)
        End If

        ' Add defaults for all missing catches
        'If (Me.mListofSuriveDistParams.Count = 0) Then
        '    For iFleet = 1 To mcore.nFleets
        '        For iGroup = 1 To mcore.nGroups
        '            If mcore.FleetInputs(iFleet).Landings(iGroup) + mcore.FleetInputs(iFleet).Discards(iGroup) > 0 Then
        '                Dim bFound As Boolean = False
        '                For iParam As Integer = 0 To Me.mListofSuriveDistParams.Count - 1
        '                    param = Me.mListofSuriveDistParams(iParam)
        '                    bFound = bFound Or ((param.FleetNo = iFleet) And (param.GroupNo = iGroup))
        '                Next
        '                If (Not bFound) Then
        '                    AddDist(New cSurvivabilityDistributonParam(iFleet, iGroup, Alpha:=2, beta:=2))
        '                End If
        '            End If
        '        Next
        '    Next
        'End If

        Return bSuccess
    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Save

        Const DefaultAlpha As Single = 10
        Const DefaultBeta As Single = 90

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFilePath()
        End If

        Dim writer As StreamWriter = cMSEUtils.GetWriter(strFilename)
        Dim bSuccess As Boolean = False

        If (writer Is Nothing) Then Return bSuccess

        Try
            writer.WriteLine("FleetNumber,FleetName,GroupNumber,GroupName,Alpha,Beta")
            If mListofSuriveDistParams.Count = 0 Then
                For iFleet = 1 To mcore.nFleets
                    For iGroup = 1 To mcore.nGroups
                        If mcore.EcopathFleetInputs(iFleet).Landings(iGroup) + mcore.EcopathFleetInputs(iFleet).Discards(iGroup) > 0 Then
                            writer.WriteLine(cStringUtils.ToCSVField(iFleet) & "," &
                                     cStringUtils.ToCSVField(mcore.EcopathFleetInputs(iFleet).Name) & "," &
                                     cStringUtils.ToCSVField(iGroup) & "," &
                                     cStringUtils.ToCSVField(mcore.EcoPathGroupInputs(iGroup).Name) & "," &
                                     cStringUtils.ToCSVField(DefaultAlpha) & "," &
                                     cStringUtils.ToCSVField(DefaultBeta))
                        End If
                    Next
                Next
            Else
                For Each entry As cSurvivabilityDistributonParam In mListofSuriveDistParams
                    writer.WriteLine(cStringUtils.ToCSVField(entry.FleetNo) & "," &
                                     cStringUtils.ToCSVField(mcore.EcopathFleetInputs(entry.FleetNo).Name) & "," &
                                     cStringUtils.ToCSVField(entry.GroupNo) & "," &
                                     cStringUtils.ToCSVField(mcore.EcoPathGroupInputs(entry.GroupNo).Name) & "," &
                                     cStringUtils.ToCSVField(entry.Alpha) & "," &
                                     cStringUtils.ToCSVField(entry.Beta))
                Next
            End If

            bSuccess = True

        Catch ex As Exception

        End Try
        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess

    End Function


    ''' <summary>
    ''' Extracts a survivability distribution parameter + information from csv
    ''' </summary>
    ''' <param name="csv">The CSV object linking to the survivability distribution parameter file</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ExtractSurvivabilityDist(ByVal msg As cMessage, _
                                              ByVal csv As CsvReader, _
                                              ByRef param As cSurvivabilityDistributonParam) As Boolean

        ' Sanity checks
        If (csv Is Nothing) Then Return False
        If (Not csv.ReadNextRecord()) Then Return False

        Dim TFleetNumber As Integer
        Dim TGroupNumber As Integer
        Dim TAlpha As Double
        Dim TBeta As Double

        Try
            TFleetNumber = cStringUtils.ConvertToInteger(csv(0))
            TGroupNumber = cStringUtils.ConvertToInteger(csv(2))
            TAlpha = cStringUtils.ConvertToDouble(csv(4))
            TBeta = cStringUtils.ConvertToDouble(csv(5))

        Catch ex As Exception
            cMSEUtils.LogError(msg, "Failed to read survivabilities from ?" & ex.Message)
            Return Nothing
        End Try

        param = New cSurvivabilityDistributonParam(TFleetNumber, TGroupNumber, TAlpha, TBeta)
        Return True

    End Function

    Private Function DefaultFilePath() As String
        Return cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "Survivabilities_dist.csv")
    End Function

    Public Function FileExists(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.FileExists
        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFilePath()
        End If
        Return File.Exists(strFilename)
    End Function

End Class

