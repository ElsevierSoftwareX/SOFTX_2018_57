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

Imports System.IO

Imports EwECore.MSE
Imports EwECore.MSEBatchManager
Imports EwEUtils.Core
Imports EwEUtils.Utilities

Namespace MSECommandFile

#Region "Interface"

    Public Interface IMSEParameter

        ''' <summary>
        ''' Init to the string read from the control file
        ''' </summary>
        ''' <param name="ParameterString"></param>
        ''' <remarks></remarks>
        Function Init(ByVal ParameterString As String) As Boolean

        ''' <summary>
        ''' Update underlying MSE variable with values
        ''' </summary>
        ''' <remarks></remarks>
        Sub Update()

        Function Validate() As Boolean

        Function getIndexes() As Integer()

        ReadOnly Property Tag() As String

        Property Index() As Integer

    End Interface

#End Region

#Region "Must implement base classes"

    Public MustInherit Class cParameterBase
        Implements IMSEParameter

        Protected m_manager As cMSECommandFileReader
        Protected m_DataTag As String
        Protected m_IndexTag As String
        Protected m_index As Integer

        Public MustOverride Function getIndexes() As Integer() Implements IMSEParameter.getIndexes

        Public MustOverride Function Init(ByVal ParameterString As String) As Boolean Implements IMSEParameter.Init

        Public MustOverride Sub Update() Implements IMSEParameter.Update

        Public MustOverride Function Validate() As Boolean Implements IMSEParameter.Validate


        Public Sub New(ByVal BatchManager As cMSECommandFileReader)
            Me.m_manager = BatchManager
        End Sub

        Public Property Index() As Integer Implements IMSEParameter.Index
            Get
                Return Me.m_index
            End Get
            Set(ByVal value As Integer)
                Me.m_index = value
            End Set
        End Property

        Public ReadOnly Property Tag() As String Implements IMSEParameter.Tag
            Get
                Return Me.m_DataTag
            End Get
        End Property


        Public ReadOnly Property Manager() As cMSECommandFileReader
            Get
                Return Me.m_manager
            End Get
        End Property

        Public Sub SendMessage(ByVal Message As String)
            Try
                Me.m_manager.Manager.MarshallMessage(Message)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".SendMessage() Exception: " & ex.Message)
            End Try
        End Sub


    End Class





    ''' <summary>
    ''' Base class for parameters were there is a single value with multiple enteries
    ''' </summary>
    ''' <remarks>Used for parameters like FixedF where this is one value across multiple groups. 
    ''' Indexes are read and stored in a different object and can be retrieved from the MSEBatchManager.getTagData(string)
    ''' </remarks>
    Public MustInherit Class cParameterListBase
        Inherits cParameterBase

        Protected m_lstPs As List(Of Single)


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

        End Sub

        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Try

                Dim values() As String
                values = ParameterString.Split(","c)

                If (String.Compare(values(0), Me.Tag) = 0) Then
                    'Data
                    Return Me.readData(ParameterString)
                End If

                'incorrect string type
                Return False

            Catch ex As Exception
                Me.SendMessage("ERROR: reading tag '" & Me.m_DataTag & "' from command file.")
                Me.SendMessage(cStringUtils.vbTab & ex.Message)
                System.Console.WriteLine(Me.ToString & ".Init() Exception: " & ex.Message)
            End Try

            Return False

        End Function


        Private Function readData(ByVal ParameterString As String) As Boolean
            Dim values() As String
            values = ParameterString.Split(","c)

            m_lstPs = New List(Of Single)

            Dim n As Integer = values.Length
            Dim ips As Integer = 1
            Do While ips < n ' - 1
                'look at the next
                If Equals(values(ips), String.Empty) Then
                    Exit Do
                End If
                m_lstPs.Add(Single.Parse(values(ips)))
                ips += 1
            Loop

            Return True

        End Function

        Protected Function ValidateIndexes(ByVal nParameters As Integer) As Boolean

            Try
                Dim lstIndexes As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
                If lstIndexes.Count = 0 Then
                    Me.SendMessage("ERROR: Tag '" & Me.m_IndexTag & "' failed to find tag in command file.")
                    Return False
                End If

                Dim Indexes() As Integer = lstIndexes.Item(0).getIndexes
                If Indexes.Length > nParameters Then
                    Me.SendMessage("ERROR: Tag '" & Me.m_IndexTag & "' number of enteries must not be greater than " & nParameters.ToString & ".")
                    ' Me.SendMessage("ERROR: Constant F number of enteries must not be greater than number of groups.")
                    Return False
                End If

                For Each igrp As Integer In Indexes
                    If igrp > nParameters Then
                        Me.SendMessage("ERROR: Tag '" & Me.m_IndexTag & "' index in command file must not be greater than " & nParameters.ToString & ".")
                        'Me.SendMessage("ERROR: Constant F invalid group index.")
                        Return False
                    End If
                Next

            Catch ex As Exception
                Me.SendMessage("ERROR: Error during data validation.")
                Return False
            End Try

            Return True

        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function

        Public Overrides Sub Update()

        End Sub

        Public Overrides Function Validate() As Boolean

        End Function
    End Class


    ''' <summary>
    ''' Base class for parameter objects that have only one value String or Numeric
    ''' </summary>
    ''' <remarks>Data from command file is stored as a string. For parameters that are numeric see cParameterNumericBase</remarks>
    Public MustInherit Class cParameterObjectBase
        Inherits cParameterBase

        Protected m_data As String


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

        End Sub

        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Try
                Dim values() As String
                values = ParameterString.Split(","c)

                If (String.Compare(values(0), Me.Tag) = 0) Then
                    'Data
                    m_data = values(1)
                    Return True
                End If

                'incorrect string type
                Return False

            Catch ex As Exception
                Me.SendMessage("ERROR: reading tag '" & Me.m_DataTag & "' from command file.")
                Me.SendMessage(cStringUtils.vbTab & ex.Message)
                System.Console.WriteLine(Me.ToString & ".Init() Exception: " & ex.Message)
            End Try

            Return False

        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function

    End Class


    ''' <summary>
    ''' Base class for parameter object that contain one parameter value and are numeric
    ''' </summary>
    ''' <remarks>This provides a generic Validate() method so each numeric class does not need to supply its own. </remarks>
    Public MustInherit Class cParameterNumericBase
        Inherits cParameterObjectBase


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)


        End Sub

        Public Overrides Function Validate() As Boolean
            Dim x As Double
            If Double.TryParse(Me.m_data, x) Then
                Return True
            End If
            Me.SendMessage("ERROR: invalid format for data '" & Me.m_DataTag & "'.")
        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function

    End Class

#End Region

#Region "Implementations"


    Public Class cOutputDirParameter
        Inherits cParameterObjectBase

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.OUTPUT_DATA_TAG

        End Sub


        Public Overrides Sub Update()
            Try
                Me.Manager.BatchData.OuputDir = DirectCast(Me.m_data, String)
            Catch ex As Exception

            End Try

        End Sub

        Public Overrides Function Validate() As Boolean

            Dim dir As String = DirectCast(Me.m_data, String)
            If Directory.Exists(dir) Then
                Return True
            End If


            Try
                Directory.CreateDirectory(dir)
            Catch ex As Exception
                Me.SendMessage("ERROR: Ouput Directory does not exist.")
                Return False
            End Try

            Me.SendMessage("WARNING: Ouput Directory in command file has been created.")
            Return True

        End Function

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.OUTPUT_DATA_TAG, ControlString)

        End Function
    End Class

    Public Class cModelNameParameter
        Inherits cParameterObjectBase

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.MODEL_NAME_TAG

        End Sub


        Public Overrides Sub Update()
            'nothing to update...
        End Sub

        Public Overrides Function Validate() As Boolean
            'DirectCast(Me.m_core.DataSource.Connection, Database.cEwEAccessDatabase).Name
            Dim data As String = DirectCast(Me.m_data, String)
            Dim ModelName As String = System.IO.Path.GetFileName(DirectCast(Me.Manager.Manager.Core.DataSource.Connection, Database.cEwEAccessDatabase).Name)
            If String.Compare(data, ModelName) <> 0 Then
                Me.SendMessage("WARNING: Currently loaded EwE model is not the same as model in command file!")
                '    Me.SendMessage(CORE_TAB & "Currently loaded EwE model is not the same as model in command file!")
            End If

            Return True

        End Function

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.MODEL_NAME_TAG, ControlString)

        End Function
    End Class



    Public Class cPPDevParameter
        Inherits cParameterNumericBase


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.PPDEV_DATA_TAG

        End Sub

        Public Overrides Sub Update()

            Try

                Me.Manager.BatchData.STDevForcing = Single.Parse(Me.m_data)
            Catch ex As Exception

            End Try

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.PPDEV_DATA_TAG, ControlString)

        End Function


    End Class


    Public Class cNumSimsParameter
        Inherits cParameterNumericBase


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.NSIMS_DATA_TAG

        End Sub

        Public Overrides Sub Update()

            Me.Manager.MSEData.NTrials = Integer.Parse(Me.m_data)

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.NSIMS_DATA_TAG, ControlString)

        End Function

    End Class

    Public Class cStartYearParameter
        Inherits cParameterNumericBase


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.STARTYEAR_DATA_TAG

        End Sub

        Public Overrides Sub Update()

            Me.Manager.MSEData.StartYear = Integer.Parse(Me.m_data)

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.STARTYEAR_DATA_TAG, ControlString)

        End Function

    End Class


    Public Class cRunTypeParameter
        Inherits cParameterNumericBase

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.RUNTYPE_DATA_TAG

        End Sub

        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Dim bsuccess As Boolean
            Try

                'read the data from the file
                bsuccess = MyBase.Init(ParameterString)
                'update the run type in the manager
                Me.Update()

            Catch ex As Exception
                bsuccess = False
            End Try


            Return bsuccess

        End Function

        Public Overrides Sub Update()

            Dim data As Integer = Integer.Parse(CStr(Me.m_data))

            'update the cMSEBatchManagers RunType
            Dim RunT As eMSEBatchRunTypes = Me.Manager.RunIndexToRunType(data)
            Me.Manager.BatchData.RunType = RunT

            'Debug.Assert(RunT <> eMSEBatchRunTypes.Any, Me.ToString & ".Invalid RunType!")

        End Sub


        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.RUNTYPE_DATA_TAG, ControlString)

        End Function

    End Class


    Public Class cErrorCVParameter
        Inherits cParameterNumericBase

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.CV_DATA_TAG

        End Sub


        Public Overrides Sub Update()
            Dim data As Single = Single.Parse(Me.m_data)

            For iyr As Integer = 1 To Me.Manager.MSEData.CVBiomT.GetUpperBound(1)
                For i As Integer = 1 To Me.Manager.nGroups
                    Me.Manager.MSEData.CVBiomT(i, iyr) = data
                Next
            Next

        End Sub


        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.CV_DATA_TAG, ControlString)

        End Function

    End Class


    Public Class cFParameter
        Inherits cParameterListBase


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.F_DATA_TAG
            Me.m_IndexTag = cMSECommandFileReader.F_INDEX_TAG

        End Sub

        Public Overrides Sub Update()

            Try
                Dim batchDat As cMSEBatchDataStructures = Me.m_manager.BatchData
                Dim lstIndexs As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
                If lstIndexs.Count < 1 Then
                    Me.SendMessage("Warning F tag not in command file.")
                    Return
                End If

                Dim indexs() As Integer = lstIndexs.Item(0).getIndexes
                Dim igrp As Integer

                'save the currently loaded data
                For igrp = 1 To Me.Manager.nGroups
                    batchDat.FixedF(Me.Index, igrp) = Me.Manager.MSEData.FixedF(igrp)
                Next

                Dim val As Single
                'update the batch data with the values from the command file 
                For i As Integer = 0 To indexs.Length - 1
                    val = Me.m_lstPs(i)
                    If batchDat.RunType = eMSEBatchRunTypes.FixedF And val = 0 Then val = Single.Epsilon
                    batchDat.FixedF(Me.Index, indexs(i)) = val
                Next

            Catch ex As Exception
                Throw New Exception("Exception in Fixed Fishing Mortality. " & ex.Message)
            End Try

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.F_DATA_TAG, ControlString)

        End Function

        Public Overrides Function Validate() As Boolean

            Return MyBase.ValidateIndexes(Me.Manager.nGroups)


            'Try
            '    Dim lstIndexes As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
            '    If lstIndexes.Count < 1 Then
            '        Me.SendMessage("ERROR: Constant F failed to find index tag '" & Me.m_IndexTag & "' in command file.")
            '        Return False
            '    End If

            '    Dim Indexes() As Integer = lstIndexes.Item(0).getIndexes
            '    If Indexes.Length > Me.Manager.nGroups Then
            '        'ValidationFailedMessage = "Constant F: Number of enteries must not be greater than number of groups."
            '        Me.SendMessage("ERROR: Constant F number of enteries must not be greater than number of groups.")
            '        Return False
            '    End If

            '    For Each igrp As Integer In Indexes
            '        If igrp > Me.Manager.nGroups Then
            '            '   ValidationFailedMessage = "Constant F: Invalid group index."
            '            Me.SendMessage("ERROR: Constant F invalid group index.")
            '            Return False
            '        End If
            '    Next

            'Catch ex As Exception
            '    Me.SendMessage("ERROR: Error during data validation.")
            '    Return False
            'End Try

            'Return True

        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function
    End Class

    Public Class cControlTypeParameter
        Inherits cParameterListBase



        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.CONTROLTYPE_DATA_TAG
            Me.m_IndexTag = cMSECommandFileReader.CONTROLTYPE_INDEX_TAG

        End Sub

        Public Overrides Sub Update()
            Try
                Dim data As cMSEBatchDataStructures = Me.m_manager.BatchData
                Dim lstIndexs As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
                If lstIndexs.Count = 0 Then
                    Me.SendMessage("Control_Type tag not found in command file. Currently loaded values will be used")
                    Return
                End If

                Dim fltIndexes() As Integer = lstIndexs.Item(0).getIndexes
                Dim i As Integer

                i = 0
                For Each Val As Single In Me.m_lstPs
                    data.ControlType(Me.Index, fltIndexes(i)) = Me.m_manager.ControlToQuotaType(CInt(Val))
                    i += 1
                Next

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Update() Exception: " & ex.Message)
                Throw New Exception("Exception setting Control Type data!", ex)
            End Try

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.CONTROLTYPE_DATA_TAG, ControlString)

        End Function

        Public Overrides Function Validate() As Boolean
            Return MyBase.ValidateIndexes(Me.Manager.nFleets)
            'Try

            '    Dim Indexes() As Integer = Me.m_manager.getTagData(Me.m_IndexTag).Item(0).getIndexes

            '    If Indexes.Length > Me.Manager.nFleets Then
            '        Me.SendMessage("ERROR: Control Type number of enteries must not be greater than number of fleets.")
            '        'ValidationFailedMessage = "Control Type: Number of enteries must not be greater than number of fleets."
            '        Return False
            '    End If

            '    For igrp As Integer = 0 To Indexes.Length - 1
            '        If Indexes(igrp) > Me.Manager.nFleets Then
            '            Me.SendMessage("ERROR: Control Type invalid fleet index.")
            '            '   ValidationFailedMessage = "Control Type: Invalid group index."
            '            Return False
            '        End If
            '    Next

            'Catch ex As Exception
            '    Me.SendMessage("ERROR: Control Type error during data validation.")
            '    Return False
            'End Try

            'Return True

        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function
    End Class

    Public Class cTACParameter
        Inherits cParameterListBase

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.Y_DATA_TAG
            Me.m_IndexTag = cMSECommandFileReader.Y_INDEX_TAG

        End Sub

        Public Overrides Sub Update()
            Try
                Dim batchDat As cMSEBatchDataStructures = Me.m_manager.BatchData
                Dim lstIndexs As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
                If lstIndexs.Count = 0 Then
                    Me.SendMessage("TAC tag not found in command file.")
                End If
                Dim indexs() As Integer = lstIndexs.Item(0).getIndexes
                Dim tac As Single
                Dim igrp As Integer

                'save the currently loaded data
                For igrp = 1 To Me.Manager.nGroups
                    batchDat.TAC(Me.Index, igrp) = Me.Manager.MSEData.TAC(igrp)
                Next

                'update the batch data with the values from the command file 
                For i As Integer = 0 To indexs.Length - 1
                    tac = Me.m_lstPs(i)
                    If batchDat.RunType = eMSEBatchRunTypes.TAC And tac = 0 Then tac = Single.Epsilon
                    batchDat.TAC(Me.Index, indexs(i)) = tac
                Next


            Catch ex As Exception
                Throw New Exception("Exception in Fixed Fishing Mortality. " & ex.Message)
            End Try

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean
            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.Y_DATA_TAG, ControlString)
        End Function

        Public Overrides Function Validate() As Boolean
            Return MyBase.ValidateIndexes(Me.Manager.nGroups)

        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function
    End Class


    Public Class cIndexParameter
        Implements IMSEParameter

        Private m_tag As String
        Private m_lstIndexes As List(Of Integer)
        Private m_index As Integer

        Public Sub New(ByVal IndexTag As String)
            m_tag = IndexTag
        End Sub

        Public ReadOnly Property DataTag() As String Implements IMSEParameter.Tag
            Get
                Return Me.m_tag
            End Get
        End Property


        Public Function Init(ByVal ParameterString As String) As Boolean Implements IMSEParameter.Init
            Try

                Dim values() As String
                values = ParameterString.Split(","c)

                m_lstIndexes = New List(Of Integer)

                Dim n As Integer = values.Length
                Dim ips As Integer = 1
                Do While ips < n
                    'look at the next
                    If Equals(values(ips), String.Empty) Then
                        Exit Do
                    End If
                    m_lstIndexes.Add(Integer.Parse(values(ips)))
                    ips += 1
                Loop

                Return True


            Catch ex As Exception
                'Me.SendMessage("ERROR: reading tag '" & Me.m_DataTag & "' from command file.")
                'Me.SendMessage(CORE_TAB & ex.Message)
                System.Console.WriteLine(Me.ToString & ".Init() Exception: " & ex.Message)
            End Try

            Return False


        End Function

        Public Sub Update() Implements IMSEParameter.Update

        End Sub

        Public Function getIndexes() As Integer() Implements IMSEParameter.getIndexes
            Return Me.m_lstIndexes.ToArray
        End Function

        Public Function Validate() As Boolean Implements IMSEParameter.Validate
            Return True
        End Function

        Public Property Index() As Integer Implements IMSEParameter.Index
            Get
                Return Me.m_index
            End Get
            Set(ByVal value As Integer)
                Me.m_index = value
            End Set
        End Property

    End Class



    Public Class cTFMParameter
        Inherits cParameterBase

        'Private m_lstIndexes As List(Of Integer)
        Private m_blims As List(Of Single)
        Private m_bbases As List(Of Single)
        Private m_Fmaxs As List(Of Single)
        Private m_Fmins As List(Of Single)

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.TFM_DATA_TAG
            Me.m_IndexTag = cMSECommandFileReader.TFM_INDEX_TAG
        End Sub

        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Try
                Dim values() As String
                values = ParameterString.Split(","c)

                If (String.Compare(values(0), Me.Tag) = 0) Then
                    Return Me.readData(ParameterString)
                End If

                Return True


            Catch ex As Exception
                Me.SendMessage("ERROR: reading tag '" & Me.m_DataTag & "' from command file.")
                Me.SendMessage(cStringUtils.vbTab & ex.Message)
                System.Console.WriteLine(Me.ToString & ".Init() Exception: " & ex.Message)
            End Try

            Return False


        End Function


        Private Function readData(ByVal ParameterString As String) As Boolean
            Dim values() As String
            values = ParameterString.Split(","c)

            ' m_lstIndexes = New List(Of Integer)

            m_blims = New List(Of Single)
            m_bbases = New List(Of Single)
            m_Fmaxs = New List(Of Single)
            m_Fmins = New List(Of Single)

            Dim n As Integer = values.Length
            Dim nps As Integer = 4 'number of parameters per record/group
            Dim ips As Integer = 1
            Do While ips <= n - nps
                'look at the next
                If Equals(values(ips), String.Empty) Then
                    Exit Do
                End If

                m_blims.Add(Single.Parse(values(ips)))
                m_bbases.Add(Single.Parse(values(ips + 1)))
                m_Fmins.Add(Single.Parse(values(ips + 2)))
                m_Fmaxs.Add(Single.Parse(values(ips + 3)))
                ips += nps
            Loop

            Return True

        End Function


        Public Overrides Sub Update()

            Try

                Dim batchDat As cMSEBatchDataStructures = Me.Manager.BatchData
                Dim mseDat As MSE.cMSEDataStructures = Me.Manager.MSEData

                For igrp As Integer = 1 To Me.Manager.nGroups
                    batchDat.tfmBlim(Me.Index, igrp) = mseDat.Blim(igrp)
                    batchDat.tfmBbase(Me.Index, igrp) = mseDat.Bbase(igrp)
                    batchDat.tfmFmax(Me.Index, igrp) = mseDat.Fopt(igrp)
                    batchDat.tfmFmin(Me.Index, igrp) = mseDat.Fmin(igrp)
                    'tfmFmin
                Next

                Dim lstIndexs As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
                If lstIndexs.Count = 0 Then
                    Return
                End If

                Dim index() As Integer = lstIndexs.Item(0).getIndexes
                Dim i As Integer
                For Each igrp As Integer In index
                    batchDat.tfmBlim(Me.Index, igrp) = Me.m_blims.Item(i)
                    batchDat.tfmBbase(Me.Index, igrp) = Me.m_bbases.Item(i)
                    batchDat.tfmFmax(Me.Index, igrp) = Me.m_Fmaxs.Item(i)
                    batchDat.tfmFmin(Me.Index, igrp) = Me.m_Fmins.Item(i)
                    i += 1
                Next

            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".Update() Exception: " & ex.Message)
            End Try


        End Sub


        Public Shared Function CanRead(ByVal ControlString As String) As Boolean
            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.TFM_DATA_TAG, ControlString)
        End Function

        Public Overrides Function getIndexes() As Integer()
            Dim lstIndexs As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
            Dim index() As Integer = lstIndexs.Item(0).getIndexes
            Return index
        End Function


        Public Overrides Function Validate() As Boolean '

            Dim igrp As Integer
            Dim lstIndexes As List(Of IMSEParameter) = Me.m_manager.getTagData(Me.m_IndexTag)
            Dim bSuccess As Boolean = True
            Try

                If lstIndexes.Count < 1 Then
                    Me.SendMessage("ERROR: Tag '" & Me.m_IndexTag & "' failed to find tag in command file.")
                    bSuccess = False
                End If

                Dim Indexes() As Integer = lstIndexes.Item(0).getIndexes

                If Indexes.Length > Me.Manager.nGroups Then
                    ' ValidationFailedMessage = "Constant Yield: Number of enteries must not be greater than number of groups."
                    Me.SendMessage("ERROR: Tag '" & Me.m_IndexTag & "' enteries must not be greater than number of groups.")
                    bSuccess = False
                End If

                For igrp = 0 To Indexes.Length - 1
                    If Indexes(igrp) > Me.Manager.nGroups Then
                        '  ValidationFailedMessage = "Constant Yield: Invalid group index."
                        Me.SendMessage("ERROR: Tag '" & Me.m_IndexTag & "' invalid group index.")
                        bSuccess = False
                    End If
                Next

                Dim bBioFailed As Boolean
                For igrp = 0 To Me.m_blims.Count - 1
                    If Me.m_blims(igrp) > Me.m_bbases(igrp) Then
                        bBioFailed = True
                    End If
                Next
                If bBioFailed Then
                    Me.SendMessage("WARNING: Tag '" & Me.m_DataTag & "' parameters Bbase must be greater than Blim. Inputs will be zero.")
                End If

                Dim bFfailed As Boolean
                For igrp = 0 To Me.m_blims.Count - 1
                    If Me.m_Fmins(igrp) > Me.m_Fmaxs(igrp) Then
                        bFfailed = True
                    End If
                Next
                If bFfailed Then
                    Me.SendMessage("WARNING: Tag '" & Me.m_DataTag & "' parameters Fmin must be greater than Fmax. Please check your data.")
                End If

            Catch ex As Exception
                bSuccess = False
            End Try

            Return bSuccess

        End Function


    End Class

    Public Class cPPParameter
        Inherits cParameterBase

        Private m_mseData As cMSEDataStructures
        Private m_FFIndex As Integer
        Private m_GroupIndex As Integer
        Private m_FFName As String

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.PP_DATA_TAG

        End Sub

        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Dim values() As String
            values = ParameterString.Split(","c)

            If (String.Compare(values(0), Me.Tag) = 0) Then
                Return Me.readData(ParameterString)
            End If

            Return True

        End Function


        Private Function readData(ByVal ParameterString As String) As Boolean
            Dim values() As String
            values = ParameterString.Split(","c)

            Debug.Assert(values.Length > 3, Me.ToString & ".Init() invalid data format.")

            Dim ips As Integer = 1

            m_FFIndex = Integer.Parse(values(ips))
            m_FFName = values(ips + 1)
            m_GroupIndex = Integer.Parse(values(ips + 2))

            If Me.m_FFIndex >= 1 Then
                Me.m_manager.BatchData.bForcingLoaded = True
            End If

            Return True

        End Function


        Public Overrides Sub Update()

            Try

                Dim data As cMSEBatchDataStructures = Me.m_manager.BatchData

                Debug.Assert(Me.Index <= data.nForcing, Me.ToString & ".Update() Index out of bounds!")

                data.ForcingGroup(Me.Index) = Me.m_GroupIndex
                data.ForcingIndexes(Me.Index) = Me.m_FFIndex
                data.ForcingNames(Me.Index) = Me.m_FFName


            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Update() Exception: " & ex.Message)
            End Try


        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean
            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.PP_DATA_TAG, ControlString)
        End Function

        Public Overrides Function Validate() As Boolean

            Dim core As cCore = Me.Manager.Core
            Try

                Dim ff As cForcingFunction = core.ForcingShapeManager.Item(m_FFIndex - 1)

                If ff Is Nothing Then
                    Me.SendMessage("ERROR: Primary Production forcing no forcing function with an index of " & m_FFIndex.ToString)
                    Return False
                End If

                If String.Compare(ff.Name, Me.m_FFName) <> 0 Then
                    Me.SendMessage("WARNING: Primary Production forcing name does not match. Command file name = '" & Me.m_FFName & "' database name = '" & ff.Name & "'")
                End If

                Dim bIsProducer As Boolean = False
                If core.MediatedInteractionManager.isPredPrey(Me.m_GroupIndex, Me.m_GroupIndex) Then
                    Dim PP As cPredPreyInteraction = core.MediatedInteractionManager.PredPreyInteraction(Me.m_GroupIndex, Me.m_GroupIndex)
                    If PP.isProdRate Then
                        bIsProducer = True
                    End If
                End If

                If Not bIsProducer Then
                    Me.SendMessage("ERROR: Primary Production forcing the group index is not a valid primary producer.")
                    Return False
                End If

                Return True

            Catch ex As Exception

            End Try


        End Function

        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function

    End Class

    Public Class cOuputParameter
        Inherits cParameterBase

        Private m_mseData As cMSEDataStructures
        Private m_EnumTag As eMSEBatchOuputTypes
        Private m_Value As Integer

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

        End Sub

        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Try
                Dim values() As String
                values = ParameterString.Split(","c)

                Debug.Assert(values.Length > 1, Me.ToString & ".Init() invalid data format.")
                Me.m_DataTag = cMSECommandFileReader.SAVE_OUTPUT_TAG
                m_EnumTag = Me.m_manager.OuputTagToOuputType(values(0))
                m_Value = Integer.Parse(values(1))

                Return True


            Catch ex As Exception
                Me.SendMessage("ERROR: reading tag '" & Me.m_DataTag & "' from command file.")
                Me.SendMessage(cStringUtils.vbTab & ex.Message)
                System.Console.WriteLine(Me.ToString & ".Init() Exception: " & ex.Message)
            End Try

            Return False


        End Function


        Public Overrides Sub Update()

            Try

                Dim data As cMSEBatchDataStructures = Me.m_manager.BatchData
                data.OuputType(Me.Index) = Me.m_EnumTag
                data.isOuputSaved(Me.Index) = CBool(Me.m_Value)

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Update() Exception: " & ex.Message)
            End Try

        End Sub


        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            If ControlString.Contains("_OUTPUT") Then
                Return True
            End If
            Return False

        End Function

        Public Overrides Function Validate() As Boolean

            Return True

        End Function


        Public Overrides Function getIndexes() As Integer()
            Return Nothing
        End Function

    End Class


    Public Class cEndYearParameter
        Inherits cParameterNumericBase

        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.ENDYEAR_DATA_TAG

        End Sub


        Public Overrides Sub Update()
            Me.m_manager.MSEData.EndYear = Integer.Parse(Me.m_data)
        End Sub


        Public Shared Function CanRead(ByVal ControlString As String) As Boolean
            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.ENDYEAR_DATA_TAG, ControlString)
        End Function

    End Class


    Public Class cVersionNumberParameter
        Inherits cParameterNumericBase


        Public Sub New(ByVal FileReader As cMSECommandFileReader)
            MyBase.New(FileReader)

            Me.m_DataTag = cMSECommandFileReader.VERSION_DATA_TAG

        End Sub


        Public Overrides Function Init(ByVal ParameterString As String) As Boolean
            Dim bSuccess As Boolean = MyBase.Init(ParameterString)
            If Me.Validate Then
                Me.Update()
            End If
            Return bSuccess
        End Function


        Public Overrides Sub Update()
            'update the version number
            Me.Manager.BatchData.VersionNumber = CSng(Me.m_data)

        End Sub

        Public Shared Function CanRead(ByVal ControlString As String) As Boolean

            Return cMSECommandFileReader.CanRead(cMSECommandFileReader.VERSION_DATA_TAG, ControlString)

        End Function

        Public Overrides Function Validate() As Boolean
            'is the data in a valid format
            If MyBase.Validate() Then

                'Ok test the version number
                Dim data As Single = Single.Parse(m_data)
                If data >= 1.0 Then
                    Return True
                Else
                    Me.SendMessage("ERROR: Invalid version number in command file.")
                End If

            End If

            Return False
        End Function

    End Class


#End Region

End Namespace




