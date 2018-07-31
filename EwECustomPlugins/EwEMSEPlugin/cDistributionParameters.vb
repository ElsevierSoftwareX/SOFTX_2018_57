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

Option Strict On
Option Explicit On

Imports System.IO
Imports EwECore
Imports EwEUtils.Utilities
Imports LumenWorks.Framework.IO.Csv

#End Region ' Imports

#Region " Base classes "

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for maintaining distribution parameters for a single functional group.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IDistributionParamsData
    ' NOP
End Interface

''' ---------------------------------------------------------------------------
''' <summary>
''' Base class for organizing, loading, and saving distribution parameters.
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class cDistributionParams

#Region " Internal Variables "

    Private m_core As EwECore.cCore
    Private m_MSE As cMSE
    Private m_bChangesToSave As Boolean

#End Region ' Internal Variables

#Region " Construction "

    Public Sub New(MSE As cMSE, core As EwECore.cCore)
        Me.m_core = core
        Me.m_MSE = MSE
        Me.Defaults()
    End Sub

#End Region ' Construction

#Region " Public bits "

    ''' <summary>
    ''' Enumerated type, providing all available distribution parameters.
    ''' </summary>
    Public Enum eDistrParamName As Byte
        B
        BA
        PB
        QB
        EE
        DenDepCatchability
        SwitchingPower
        QBMaxxQBio
        PredEffectFeedingTime
        OtherMortFeedingTime
        MaxRelFeedingTime
        FeedingTimeAdjustRate
    End Enum

    Public Function IsChanged() As Boolean
        Return Me.m_bChangesToSave
    End Function

    ''' <summary>
    ''' Generate default values
    ''' </summary>
    Public MustOverride Sub Defaults()

    ''' <summary>
    ''' Generate entire parameter set from CSV.
    ''' </summary>
    ''' <param name="msg">Panic message, if any.</param>
    Public MustOverride Function Load(ByVal msg As cMessage) As Boolean

    ''' <summary>
    ''' Save entire parameter set to CSV.
    ''' </summary>
    Public MustOverride Function Save() As Boolean

#End Region ' Public bits

#Region " Internal bits "

    Protected ReadOnly Property MSE As cMSE
        Get
            Return Me.m_MSE
        End Get
    End Property

    Protected ReadOnly Property Core As cCore
        Get
            Return Me.m_core
        End Get
    End Property

#End Region ' Internal bits

End Class

#End Region ' Base classes

#Region " Ecopath "

''' ---------------------------------------------------------------------------
''' <summary>
''' Container for Ecopath distribution parameters for a single functional group.
''' <seealso cref="IDistributionParamsData"/>.
''' <seealso cref="cEcosimDistributionParamsData"/>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcopathDistributionParamsData
    Implements IDistributionParamsData

    Public Sub New(ByVal GroupNumber As Integer, _
                   ByVal GroupName As String, _
                   ByVal Mean As Single, _
                   ByVal CV As Double, _
                   ByVal LowerBound As Double, _
                   ByVal UpperBound As Double)
        Me.GroupNo = GroupNumber
        Me.GroupName = GroupName
        Me.Mean = Mean
        Me.CV = CV
        Me.LowerBound = LowerBound
        Me.UpperBound = UpperBound
    End Sub

    Public Property CV As Double
    Public Property LowerBound As Double
    Public Property UpperBound As Double
    Public Property GroupNo As Integer
    Public Property GroupName As String
    Public Property Mean As Double

End Class

''' ---------------------------------------------------------------------------
''' <summary>
''' Manager for all <see cref="cEcopathDistributionParamsData"/> for the 
''' currently loaded model.
''' <seealso cref="cDistributionParams"/>
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcopathDistributionParams
    Inherits cDistributionParams

#Region " Internal variables "

    Private m_B As List(Of IDistributionParamsData)
    Private m_BA As List(Of IDistributionParamsData)
    Private m_QB As List(Of IDistributionParamsData)
    Private m_PB As List(Of IDistributionParamsData)
    Private m_EE As List(Of IDistributionParamsData)

#End Region ' Internal variables

#Region " Construction "

    Public Sub New(MSE As cMSE, core As EwECore.cCore)
        MyBase.New(MSE, core)
    End Sub

#End Region ' Construction

#Region " Public bits "

    Public Overrides Sub Defaults()
        If (Me.B Is Nothing) Then
            Me.m_B = New List(Of IDistributionParamsData)
            Me.m_BA = New List(Of IDistributionParamsData)
            Me.m_QB = New List(Of IDistributionParamsData)
            Me.m_PB = New List(Of IDistributionParamsData)
            Me.m_EE = New List(Of IDistributionParamsData)
        Else
            Me.m_B.Clear()
            Me.m_BA.Clear()
            Me.m_QB.Clear()
            Me.m_PB.Clear()
            Me.m_EE.Clear()
        End If
        Me.Load(Nothing)
    End Sub

    ''' <summary>
    ''' Get Ecopath <see cref="eDistrParamName.B">biomass</see> distribution parameters
    ''' </summary>
    Public ReadOnly Property B As List(Of IDistributionParamsData)
        Get
            Return Me.m_B
        End Get
    End Property

    Public ReadOnly Property BA As List(Of IDistributionParamsData)
        Get
            Return Me.m_BA
        End Get
    End Property

    Public ReadOnly Property QB As List(Of IDistributionParamsData)
        Get
            Return Me.m_QB
        End Get
    End Property

    Public ReadOnly Property PB As List(Of IDistributionParamsData)
        Get
            Return Me.m_PB
        End Get
    End Property

    Public ReadOnly Property EE As List(Of IDistributionParamsData)
        Get
            Return Me.m_EE
        End Get
    End Property

    ''' <inheritdocs cref="cDistributionParams.Load"/>
    Public Overrides Function Load(ByVal msg As cMessage) As Boolean
        Return LoadEcopathParamX(msg, B, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "B_Dist.csv"), eDistrParamName.B) And _
               LoadEcopathParamX(msg, PB, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "PB_Dist.csv"), eDistrParamName.PB) And _
               LoadEcopathParamX(msg, QB, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "QB_Dist.csv"), eDistrParamName.QB) And _
               LoadEcopathParamX(msg, EE, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "EE_Dist.csv"), eDistrParamName.EE) And _
               LoadEcopathParamX(msg, BA, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "BA_Dist.csv"), eDistrParamName.BA)
    End Function

    ''' <inheritdocs cref="cDistributionParams.Save"/>
    Public Overrides Function Save() As Boolean
        Return SaveEcopathParameters2CSV(B, "B_Dist") And _
               SaveEcopathParameters2CSV(BA, "BA_Dist") And _
               SaveEcopathParameters2CSV(PB, "PB_Dist") And _
               SaveEcopathParameters2CSV(QB, "QB_Dist") And _
               SaveEcopathParameters2CSV(EE, "EE_Dist")
    End Function

#End Region ' Public bits

#Region " Internals "

    Private Function LoadEcopathParamX(ByVal msg As cMessage, _
                                       ByVal ParamList As List(Of IDistributionParamsData), _
                                       ByVal strPath As String, _
                                       ByVal ParamName As eDistrParamName) As Boolean

        Dim csv As CsvReader
        Dim MonteCarlo As cMonteCarloManager = Me.Core.EcosimMonteCarlo
        Dim MCGroup As cMonteCarloGroup
        Dim TMean As Single
        Dim TCV As Double
        Dim TLowerBound As Double
        Dim TUpperBound As Double
        Dim reader As StreamReader = Nothing
        Dim params(Me.Core.nLivingGroups) As cEcopathDistributionParamsData
        Dim param As cEcopathDistributionParamsData = Nothing
        Dim nGroups As Integer = 0
        Dim bSuccess As Boolean = True

        If File.Exists(strPath) Then
            reader = cMSEUtils.GetReader(strPath)
            If (reader IsNot Nothing) Then
                Try
                    ParamList.Clear()
                    csv = New CsvReader(reader, True)
                    While Not csv.EndOfStream
                        param = Me.ExtractEcopathParam(csv, ParamName)
                        If (param IsNot Nothing) Then
                            ' Only add with valid group indexes and names
                            Dim bOK As Boolean = False
                            If (param.GroupNo >= 1 And param.GroupNo <= Me.Core.nLivingGroups) Then
                                If (String.Compare(param.GroupName, Me.Core.EcoPathGroupInputs(param.GroupNo).Name, True) = 0) Then
                                    params(param.GroupNo) = param
                                    nGroups += 1
                                    bOK = True
                                End If
                            End If
                            If (bOK = False) Then
                                cMSEUtils.LogError(msg, "Invalid group " & param.GroupNo & " encountered in " & Path.GetFileName(strPath))
                            End If
                            bSuccess = bSuccess And bOK
                        End If
                    End While
                    csv.Dispose()

                Catch ex As Exception
                    Debug.Assert(False, Me.ToString & ".LoadEcopathParameters() Exception: " & ex.Message)
                    bSuccess = False
                End Try

                If bSuccess And (nGroups <> Core.nLivingGroups) Then
                    If nGroups < Me.Core.nLivingGroups Then
                        cMSEUtils.LogError(msg, String.Format(My.Resources.ERROR_DISTRFILE_GROUPS_LIVING_MISSING, Path.GetFileName(strPath)))
                    ElseIf nGroups > Me.Core.nLivingGroups Then 'Check whether there are too many groups in the file
                        cMSEUtils.LogError(msg, String.Format(My.Resources.ERROR_DISTRFILE_GROUPS_HASNONLIVING, Path.GetFileName(strPath)))
                    End If
                    bSuccess = False
                End If
            End If
        End If
        cMSEUtils.ReleaseReader(reader)

        ' Complement list with defaults for missing groups
        For igrp = 1 To Me.Core.nLivingGroups
            If params(igrp) Is Nothing Then
                MCGroup = MonteCarlo.Groups(igrp)
                If ParamName = eDistrParamName.B Then
                    TMean = Me.Core.EcoPathGroupOutputs(igrp).Biomass
                    TCV = MCGroup.Bcv
                    TLowerBound = MCGroup.BLower
                    TUpperBound = MCGroup.BUpper
                ElseIf ParamName = eDistrParamName.BA Then
                    TMean = Me.Core.EcoPathGroupOutputs(igrp).BioAccum
                    TCV = MCGroup.BAcv
                    TLowerBound = MCGroup.BALower
                    TUpperBound = MCGroup.BAUpper
                ElseIf ParamName = eDistrParamName.EE Then
                    TMean = Me.Core.EcoPathGroupOutputs(igrp).EEOutput
                    TCV = MCGroup.EEcv
                    TLowerBound = MCGroup.EELower
                    TUpperBound = MCGroup.EEUpper
                ElseIf ParamName = eDistrParamName.PB Then
                    TMean = Me.Core.EcoPathGroupOutputs(igrp).PBOutput
                    TCV = MCGroup.PBcv
                    TLowerBound = MCGroup.PBLower
                    TUpperBound = MCGroup.PBUpper
                ElseIf ParamName = eDistrParamName.QB Then
                    TMean = Me.Core.EcoPathGroupOutputs(igrp).QBOutput
                    TCV = MCGroup.QBcv
                    TLowerBound = MCGroup.QBLower
                    TUpperBound = MCGroup.QBUpper
                End If
                params(igrp) = New cEcopathDistributionParamsData(igrp, Me.Core.EcoPathGroupInputs(igrp).Name, TMean, TCV, TLowerBound, TUpperBound)
            End If
        Next

        For Each param In params
            If (param IsNot Nothing) Then
                ParamList.Add(param)
            End If
        Next

        Return bSuccess

    End Function

    ''' <summary>
    ''' Extracts distribution parameters for one group from csv and Ecopath
    ''' </summary>
    ''' <param name="csv"></param>
    ''' <param name="ParameterType"></param>
    ''' <returns></returns>
    Private Function ExtractEcopathParam(ByVal csv As CsvReader, ByVal ParameterType As eDistrParamName) As cEcopathDistributionParamsData

        ' Sanity checks
        If (csv Is Nothing) Then Return Nothing
        If (Not csv.ReadNextRecord()) Then Return Nothing
        If (csv.FieldCount < 5) Then Return Nothing

        Dim TGroupName As String = ""
        Dim TGroupNumber As Integer
        Dim TMean As Single
        Dim TCV As Double
        Dim TLowerBound As Double
        Dim TUpperBound As Double

        Try
            TGroupNumber = cStringUtils.ConvertToInteger(csv(0))
            TGroupName = cMSEUtils.FromCSVField(csv(1))
            TCV = cStringUtils.ConvertToDouble(csv(2))
            TLowerBound = cStringUtils.ConvertToDouble(csv(3))
            TUpperBound = cStringUtils.ConvertToDouble(csv(4))

            ' JS 02Oct2013: Need to validate group number
            If TGroupNumber < 1 Or TGroupNumber >= Me.Core.nGroups Then
                ' ToDo:_JS: report error somehow
                Return Nothing
            End If

            If ParameterType = eDistrParamName.B Then
                TMean = Me.Core.EcoPathGroupInputs(TGroupNumber).BiomassAreaInput
            ElseIf ParameterType = eDistrParamName.BA Then
                TMean = Me.Core.EcoPathGroupInputs(TGroupNumber).BioAccumInput
            ElseIf ParameterType = eDistrParamName.QB Then
                TMean = Me.Core.EcoPathGroupInputs(TGroupNumber).QBInput
            ElseIf ParameterType = eDistrParamName.PB Then
                TMean = Me.Core.EcoPathGroupInputs(TGroupNumber).PBInput
            ElseIf ParameterType = eDistrParamName.EE Then
                TMean = Me.Core.EcoPathGroupInputs(TGroupNumber).EEInput
            End If

        Catch ex As Exception
            ' ToDo:_JS: report error somehow
            Return Nothing
        End Try

        Return New cEcopathDistributionParamsData(TGroupNumber, TGroupName, TMean, TCV, TLowerBound, TUpperBound)

    End Function

    Private Function SaveEcopathParameters2CSV(ByVal parms As List(Of IDistributionParamsData), ByVal strFileName As String) As Boolean

        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, strFileName & ".csv"), False)
        Dim bSuccess As Boolean = False

        If (writer Is Nothing) Then Return bSuccess

        Try
            writer.WriteLine("Group Number,Name,CV,""Lower Bound"",""Upper Bound""")

            For Each entry As IDistributionParamsData In parms
                If (TypeOf (entry) Is cEcopathDistributionParamsData) Then
                    Dim param As cEcopathDistributionParamsData = DirectCast(entry, cEcopathDistributionParamsData)
                    writer.WriteLine(cStringUtils.ToCSVField(param.GroupNo) & "," & _
                                 cStringUtils.ToCSVField(param.GroupName) & "," & _
                                 cStringUtils.ToCSVField(param.CV) & "," & _
                                 cStringUtils.ToCSVField(param.LowerBound) & "," & _
                                 cStringUtils.ToCSVField(param.UpperBound))
                End If
            Next
            bSuccess = True

        Catch ex As Exception

        End Try

        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess

    End Function

#End Region ' Internals

End Class

#End Region ' Ecopath

#Region " Ecosim "

''' ---------------------------------------------------------------------------
''' <summary>
''' Container for Ecosim distribution parameters for a single functional group.
''' <seealso cref="IDistributionParamsData"/>.
''' <seealso cref="cEcopathDistributionParamsData"/>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcosimDistributionParamsData
    Implements IDistributionParamsData

    Public Sub New(ByVal GroupNumber As Integer, _
                   ByVal GroupName As String, _
                   ByVal DistributionType As cMSE.DistributionType, _
                   ByVal LowerBound As Double, _
                   ByVal UpperBound As Double, _
                   ByVal MidPoint As Double)
        Me.GroupNo = GroupNumber
        Me.GroupName = GroupName
        Me.DistributionType = DistributionType
        Me.LowerBound = LowerBound
        Me.UpperBound = UpperBound
        Me.MidPoint = If(MidPoint < 0, cCore.NULL_VALUE, MidPoint)
    End Sub

    Public Property GroupNo As Integer
    Public Property GroupName As String
    Public Property DistributionType As cMSE.DistributionType
    Public Property LowerBound As Double
    Public Property UpperBound As Double
    Public Property MidPoint As Double

End Class

''' ---------------------------------------------------------------------------
''' <summary>
''' Manager for all <see cref="cEcosimDistributionParamsData"/> for the 
''' currently loaded model.
''' <seealso cref="cDistributionParams"/>
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcosimDistributionParams
    Inherits cDistributionParams

    Public DenDepCatchability As List(Of IDistributionParamsData)
    Public SwitchingPower As List(Of IDistributionParamsData)
    Public QBMaxxQBio As List(Of IDistributionParamsData)
    Public PredEffectFeedingTime As List(Of IDistributionParamsData)
    Public OtherMortFeedingTime As List(Of IDistributionParamsData)
    Public MaxRelFeedingTime As List(Of IDistributionParamsData)
    Public FeedingTimeAdjustRate As List(Of IDistributionParamsData)

    Public Sub New(MSE As cMSE, core As EwECore.cCore)
        MyBase.New(MSE, core)
    End Sub

    Public Overrides Sub Defaults()
        If (Me.DenDepCatchability Is Nothing) Then
            Me.DenDepCatchability = New List(Of IDistributionParamsData)
            Me.SwitchingPower = New List(Of IDistributionParamsData)
            Me.QBMaxxQBio = New List(Of IDistributionParamsData)
            Me.PredEffectFeedingTime = New List(Of IDistributionParamsData)
            Me.OtherMortFeedingTime = New List(Of IDistributionParamsData)
            Me.MaxRelFeedingTime = New List(Of IDistributionParamsData)
            Me.FeedingTimeAdjustRate = New List(Of IDistributionParamsData)
        Else
            Me.DenDepCatchability.Clear()
            Me.SwitchingPower.Clear()
            Me.QBMaxxQBio.Clear()
            Me.PredEffectFeedingTime.Clear()
            Me.OtherMortFeedingTime.Clear()
            Me.MaxRelFeedingTime.Clear()
            Me.FeedingTimeAdjustRate.Clear()
        End If
        Me.Load(Nothing)
    End Sub

    Public Overrides Function Load(ByVal msg As cMessage) As Boolean

        'loads all the ecosim csv files up and creates instances of lists of structures that hold it all in memory
        Return LoadEcosimParamX(msg, DenDepCatchability, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "DenDepCatchability.csv"), eDistrParamName.DenDepCatchability) And _
               LoadEcosimParamX(msg, SwitchingPower, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "SwitchingPower.csv"), eDistrParamName.SwitchingPower) And _
               LoadEcosimParamX(msg, QBMaxxQBio, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "QBMaxxQBio.csv"), eDistrParamName.QBMaxxQBio) And _
               LoadEcosimParamX(msg, PredEffectFeedingTime, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "PredEffectFeedingTime.csv"), eDistrParamName.PredEffectFeedingTime) And _
               LoadEcosimParamX(msg, OtherMortFeedingTime, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "OtherMortFeedingTime.csv"), eDistrParamName.OtherMortFeedingTime) And _
               LoadEcosimParamX(msg, MaxRelFeedingTime, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "MaxRelFeedingTime.csv"), eDistrParamName.MaxRelFeedingTime) And _
               LoadEcosimParamX(msg, FeedingTimeAdjustRate, cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "FeedingTimeAdjustRate.csv"), eDistrParamName.FeedingTimeAdjustRate)

    End Function

    Public Overrides Function Save() As Boolean

        Return SaveEcoSimParameters2CSV(DenDepCatchability, "DenDepCatchability") And _
               SaveEcoSimParameters2CSV(SwitchingPower, "SwitchingPower") And _
               SaveEcoSimParameters2CSV(QBMaxxQBio, "QBMaxxQBio") And _
               SaveEcoSimParameters2CSV(PredEffectFeedingTime, "PredEffectFeedingTime") And _
               SaveEcoSimParameters2CSV(OtherMortFeedingTime, "OtherMortFeedingTime") And _
               SaveEcoSimParameters2CSV(MaxRelFeedingTime, "MaxRelFeedingTime") And _
               SaveEcoSimParameters2CSV(FeedingTimeAdjustRate, "FeedingTimeAdjustRate")

    End Function

    ''' <summary>
    ''' Given a Ecosim csv object this extracts the data from the current line and uses it to return an EcosimParam structure object
    ''' </summary>
    ''' <param name="csv"></param>
    ''' <returns></returns>
    Private Function ExtractEcosimParam(ByVal csv As CsvReader) As cEcosimDistributionParamsData

        ' JS 12Oct13: made fail-proof
        ' JS 12Oct13: used fixed CSV field reading
        ' JS 02Dec13: added EndOfStream checks

        ' Sanity checks
        If (csv Is Nothing) Then Return Nothing
        If (csv.EndOfStream()) Then Return Nothing
        If (Not csv.ReadNextRecord()) Then Return Nothing

        Dim TGroupName As String = ""
        Dim TGroupNumber As Integer
        Dim TLowerBound As Double
        Dim TUpperBound As Double
        Dim TDistributionType As cMSE.DistributionType
        Dim TMidPoint As Double

        Try

            TGroupName = cMSEUtils.FromCSVField(csv(0))
            TGroupNumber = cStringUtils.ConvertToInteger(csv(1))
            Dim iDistr As Integer = cStringUtils.ConvertToInteger(csv(2))
            Try
                TDistributionType = DirectCast(iDistr, cMSE.DistributionType)
            Catch ex As Exception
                ' Default
                TDistributionType = cMSE.DistributionType.Uniform
            End Try
            TLowerBound = cStringUtils.ConvertToDouble(csv(3))
            TUpperBound = cStringUtils.ConvertToDouble(csv(4))
            TMidPoint = cStringUtils.ConvertToDouble(csv(5))

        Catch ex As Exception
            ' ToDo_JS: respond to error
            Return Nothing
        End Try

        Return New cEcosimDistributionParamsData(TGroupNumber, TGroupName, TDistributionType, TLowerBound, TUpperBound, TMidPoint)

    End Function

    Private Function LoadEcosimParamX(ByVal msg As cMessage, _
                                      ByRef ParamList As List(Of IDistributionParamsData), _
                                      ByVal strPath As String, _
                                      ByVal ParamName As eDistrParamName) As Boolean

        Dim reader As StreamReader = Nothing
        Dim csv As CsvReader = Nothing
        Dim TMean As Single
        Dim params(Me.Core.nLivingGroups) As cEcosimDistributionParamsData
        Dim param As cEcosimDistributionParamsData = Nothing
        Dim nGroups As Integer = 0
        Dim bSuccess As Boolean = True
        Dim nPrimaryProducers As Integer = 0

        'Count Primary producers
        For iGrp = 1 To Me.Core.nGroups
            If Me.Core.EcoPathGroupInputs(iGrp).IsProducer Then
                nPrimaryProducers += 1
            End If
        Next

        If File.Exists(strPath) Then
            reader = cMSEUtils.GetReader(strPath)
            If (reader IsNot Nothing) Then
                Try
                    ParamList.Clear()
                    csv = New CsvReader(reader, True)
                    While Not csv.EndOfStream
                        param = Me.ExtractEcosimParam(csv)
                        If (param IsNot Nothing) Then
                            Dim bOK As Boolean = False
                            ' Only add with valid group indexes and names
                            If (param.GroupNo >= 1 And param.GroupNo <= Me.Core.nLivingGroups) Then
                                If (String.Compare(param.GroupName, Me.Core.EcoPathGroupInputs(param.GroupNo).Name, True) = 0) Then
                                    If Me.Core.EcoPathGroupInputs(param.GroupNo).IsProducer And param.DistributionType = cMSE.DistributionType.NotSet And _
                                        param.LowerBound = -9999 And param.UpperBound = -9999 Then
                                        'This has been recognised at a primary producer that is correctly parameterised
                                        params(param.GroupNo) = param
                                        bOK = True
                                    ElseIf Me.Core.EcoPathGroupInputs(param.GroupNo).IsProducer And (Not param.DistributionType = cMSE.DistributionType.NotSet Or _
                                    param.LowerBound <> -9999 Or param.UpperBound <> -9999) Then
                                        'This is a primary producer that is incorrectly parameterised
                                        bOK = False
                                    ElseIf Not Me.Core.EcoPathGroupInputs(param.GroupNo).IsProducer Then
                                        'This isn't a primary producer
                                        params(param.GroupNo) = param
                                        nGroups += 1
                                        bOK = True
                                    End If
                                End If
                                End If
                                If (bOK = False) Then
                                    cMSEUtils.LogError(msg, "Invalid group " & param.GroupNo & " encountered in " & Path.GetFileName(strPath))
                                End If
                                bSuccess = bSuccess And bOK
                            End If
                    End While
                    csv.Dispose()

                Catch ex As Exception
                    Debug.Assert(False, Me.ToString & ".LoadEcosimParameters() Exception: " & ex.Message)
                    bSuccess = False
                End Try

                If bSuccess And (nGroups <> Core.nLivingGroups - nPrimaryProducers) Then
                    If nGroups < Me.Core.nLivingGroups - nPrimaryProducers Then
                        cMSEUtils.LogError(msg, String.Format(My.Resources.ERROR_DISTRPARAM_GROUPS_TOOFEW, Path.GetFileName(strPath)))
                    ElseIf nGroups > Me.Core.nLivingGroups - nPrimaryProducers Then 'Check whether there are too many groups in the file
                        cMSEUtils.LogError(msg, String.Format(My.Resources.ERROR_DISTRPARAM_GROUPS_TOOMANY, Path.GetFileName(strPath)))
                    End If
                    bSuccess = False
                End If
            End If
        End If
        cMSEUtils.ReleaseReader(reader)

        ' Complement list with defaults for missing groups
        For iGrp = 1 To Me.Core.nLivingGroups
            If (params(iGrp) Is Nothing) Then
                If ParamName = eDistrParamName.DenDepCatchability Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).DenDepCatchability
                ElseIf ParamName = eDistrParamName.FeedingTimeAdjustRate Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).FeedingTimeAdjustRate
                ElseIf ParamName = eDistrParamName.MaxRelFeedingTime Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).MaxRelFeedingTime
                ElseIf ParamName = eDistrParamName.OtherMortFeedingTime Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).OtherMortFeedingTime
                ElseIf ParamName = eDistrParamName.PredEffectFeedingTime Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).PredEffectFeedingTime
                ElseIf ParamName = eDistrParamName.QBMaxxQBio Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).QBMaxQBio
                ElseIf ParamName = eDistrParamName.SwitchingPower Then
                    TMean = Me.Core.EcoSimGroupInputs(iGrp).SwitchingPower
                End If
                If Core.EcoPathGroupInputs(iGrp).IsProducer Then
                    params(iGrp) = New cEcosimDistributionParamsData(iGrp, Me.Core.EcoPathGroupInputs(iGrp).Name, cMSE.DistributionType.NotSet, -9999, -9999, -9999)
                Else
                    params(iGrp) = New cEcosimDistributionParamsData(iGrp, Me.Core.EcoPathGroupInputs(iGrp).Name, cMSE.DistributionType.Triangular, TMean * (1 - 0.1), TMean * (1 + 0.1), TMean)
                End If
            End If
        Next

        For Each param In params
            If (param IsNot Nothing) Then
                Dim grp As cEcoPathGroupInput = Core.EcoPathGroupInputs(param.GroupNo)
                ' Only allow living non-producers
                If (grp.IsLiving) Then
                    ParamList.Add(param)
                End If
            End If
        Next

        Return bSuccess

    End Function

    Private Function SaveEcoSimParameters2CSV(ByVal params As List(Of IDistributionParamsData), ByVal strFileName As String) As Boolean

        Dim writer As StreamWriter = cMSEUtils.GetWriter(cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, strFileName & ".csv"), False)
        Dim bSuccess As Boolean = False

        If (writer Is Nothing) Then Return bSuccess

        Try
            writer.WriteLine("GroupName,GroupNumber,DistributionType,Lower,Upper,Mid")

            For Each entry As IDistributionParamsData In params
                If (TypeOf (entry) Is cEcosimDistributionParamsData) Then
                    Dim param As cEcosimDistributionParamsData = DirectCast(entry, cEcosimDistributionParamsData)
                    writer.WriteLine(cStringUtils.ToCSVField(param.GroupName) & "," & _
                                     cStringUtils.ToCSVField(param.GroupNo) & "," & _
                                     cStringUtils.ToCSVField(param.DistributionType) & "," & _
                                     cStringUtils.ToCSVField(param.LowerBound) & "," & _
                                     cStringUtils.ToCSVField(param.UpperBound) & "," & _
                                     cStringUtils.ToCSVField(param.MidPoint))
                End If
            Next

            bSuccess = True

        Catch ex As Exception

        End Try
        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess

    End Function

End Class

#End Region ' Ecosim
