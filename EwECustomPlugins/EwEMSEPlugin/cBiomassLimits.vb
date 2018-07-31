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
Imports EwEMSEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class cBiomassLimits
    Implements IMSEData

    Implements IList(Of cBiomassLimit)

    Public lstBiomassLimits As List(Of cBiomassLimit)

    Private mPlugin As cMSEPluginPoint
    Private mMSE As cMSE
    Private mCore As cCore
    Private mFileName As String
    Private m_bChanged As Boolean
    Const mFileNameOnly As String = "BiomassLimits.csv"

#Region "Internal Class"

    Public Class cBiomassLimit

        Public mGroup As cEcoPathGroupInput
        Public mLowerLimit As Double
        Public mUpperLimit As Double

        Private mCore As cCore

        Public Sub New(Core As cCore)
            'mPlugin = Plugin
            'mMSE = mPlugin.MSE
            mCore = Core
            'mFileName = cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.BiomassLimits, mFileNameOnly)
        End Sub

    End Class

#End Region

    Public Sub New(MSE As cMSE)

        'mPlugin = Plugin
        mMSE = MSE
        mCore = mMSE.Core
        mFileName = cMSEUtils.MSEFile(mMSE.DataPath, cMSEUtils.eMSEPaths.BiomassLimits, mFileNameOnly)
        lstBiomassLimits = New List(Of cBiomassLimit)
        Me.Defaults()

    End Sub

    'Public Sub Init()
    '    LoadLimitsFromCSV()
    'End Sub

    Private Function ResolveGroup(strName As String, iIndex As Integer) As cEcoPathGroupInput
        If (iIndex < 1) Or (iIndex > Me.mCore.nGroups) Then Return Nothing
        Dim grp As cEcoPathGroupInput = Me.mCore.EcoPathGroupInputs(iIndex)
        Dim grpName As String = cMSEUtils.FromCSVField(strName)
        If String.Compare(grp.Name, grpName, True) <> 0 Then
            Return Nothing
        End If
        Return grp
    End Function

    Public Sub Add(item As cBiomassLimit) Implements ICollection(Of cBiomassLimit).Add
        If Not Me.Contains(item) Then
            Me.lstBiomassLimits.Add(item)
        End If
    End Sub

    'Public Function SaveLimitsToCSV() As Boolean

    'End Function



    Public Function GetUpperLimit(iGrp As Integer) As Double
        For Each iBiomassLimit In lstBiomassLimits
            If iBiomassLimit.mGroup.Index = iGrp Then Return iBiomassLimit.mUpperLimit
        Next

        Return 100000
    End Function

    Public Function GetLowerLimit(iGrp As Integer) As Double
        For Each iBiomassLimit In lstBiomassLimits
            If iBiomassLimit.mGroup.Index = iGrp Then Return iBiomassLimit.mLowerLimit
        Next
        Return 1.0E-20
    End Function

    Public Function Exist(iGrp As Integer) As Boolean
        For Each iBiomassLimit In lstBiomassLimits
            If iBiomassLimit.mGroup.Index = iGrp Then Return True
        Next
        Return False
    End Function

    Public Sub Clear() _
        Implements ICollection(Of cBiomassLimit).Clear
        Me.lstBiomassLimits.Clear()
    End Sub

    Public Function Contains(item As cBiomassLimit) As Boolean _
        Implements ICollection(Of cBiomassLimit).Contains
        'If Me.lstBiomassLimits.Count = 0 Then Return False
        For Each iLimit As cBiomassLimit In Me.lstBiomassLimits
            If ReferenceEquals(item.mGroup, iLimit.mGroup) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub CopyTo(array() As cBiomassLimit, arrayIndex As Integer) _
        Implements ICollection(Of cBiomassLimit).CopyTo
        ' NOP
    End Sub

    Public ReadOnly Property Count As Integer _
        Implements ICollection(Of cBiomassLimit).Count
        Get
            Return lstBiomassLimits.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean _
        Implements ICollection(Of cBiomassLimit).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Function Remove(item As cBiomassLimit) As Boolean _
        Implements ICollection(Of cBiomassLimit).Remove
        Return Me.lstBiomassLimits.Remove(item)
    End Function

    Public Function IndexOf(item As cBiomassLimit) As Integer _
        Implements IList(Of cBiomassLimit).IndexOf
        Return Me.lstBiomassLimits.IndexOf(item)
    End Function

    Public Sub Insert(index As Integer, item As cBiomassLimit) _
        Implements IList(Of cBiomassLimit).Insert
        Me.lstBiomassLimits.Insert(index, item)
    End Sub

    Default Public Property Item(index As Integer) As cBiomassLimit _
        Implements IList(Of cBiomassLimit).Item
        Get
            Return Me.lstBiomassLimits.Item(index)
        End Get
        Set(value As cBiomassLimit)
            Me.lstBiomassLimits(index) = value
        End Set
    End Property

    Public Sub RemoveAt(index As Integer) _
        Implements IList(Of cBiomassLimit).RemoveAt
        Me.lstBiomassLimits.RemoveAt(index)
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of cBiomassLimit) _
        Implements IEnumerable(Of cBiomassLimit).GetEnumerator
        Return Me.lstBiomassLimits.GetEnumerator()
    End Function

    Private Function Bogus() As IEnumerator _
        Implements IEnumerable.GetEnumerator
        'NOP
        Return Nothing
    End Function

    Public Function Load(Optional msg As cMessage = Nothing, Optional strFilename As String = "") As Boolean Implements IMSEData.Load
        Dim datadir As String = cMSEUtils.MSEFolder(mMSE.DataPath, cMSEUtils.eMSEPaths.BiomassLimits)
        Dim strVal As String = ""
        Dim StratCounter As Integer = 1
        Dim lstFailedFiles As New List(Of String)
        Dim buff As String
        Dim recs() As String
        Dim breturn As Boolean = False

        Me.Clear()

        ' If nothing to load then run with defaults
        If Not File.Exists(Me.mFileName) Then Return True

        Try

            Dim reader As StreamReader = cMSEUtils.GetReader(Me.mFileName)
            If (reader IsNot Nothing) Then

                buff = reader.ReadLine()        'Skip the row of headers

                breturn = True

                Do Until reader.EndOfStream

                    buff = reader.ReadLine()
                    recs = buff.Split(","c)

                    Dim tempBiomassLimit As cBiomassLimit
                    'Each HCR Group needs to be a new object
                    tempBiomassLimit = New cBiomassLimit(mCore)

                    If (recs(0).Contains("_"c)) Then
                        ' Work-around for values such as "EcopathGroupInput_#"
                        tempBiomassLimit.mGroup = mCore.EcoPathGroupInputs(cStringUtils.ConvertToInteger(recs(0).Substring(recs(0).LastIndexOf("_") + 1)))
                    Else
                        tempBiomassLimit.mGroup = mCore.EcoPathGroupInputs(cStringUtils.ConvertToInteger(recs(0)))
                    End If
                    tempBiomassLimit.mLowerLimit = cStringUtils.ConvertToDouble(recs(1))
                    tempBiomassLimit.mUpperLimit = cStringUtils.ConvertToDouble(recs(2))

                    Dim strMsg As String = ""
                    ' Only add valid BiomassLimits!
                    Me.Add(tempBiomassLimit)

                Loop

            End If 'reader IsNot Nothing
            cMSEUtils.ReleaseReader(reader)

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString + ".Read() Exception: " + ex.Message)
        End Try

        'for debugging
        Debug.Assert(breturn, Me.ToString + ".Read() Failed to read biomass limits from file.")

        'Warn the user if anything failed
        If breturn = False Then
            ' ToDo_JS: globalize this. Add delete prompt?
            Me.mCore.Messages.SendMessage(New cMessage("Cefas MSE Failed to read the biomass limits file",
                                                       eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Information))
        End If

        Return True
    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean Implements IMSEData.Save

        'Dim strFile As String = ""
        'Dim strPath As String = ""
        'Dim msg As cMessage = Nothing
        'Dim breturn As Boolean = True
        'Try

        '    If msg Is Nothing Then
        '        strPath = Path.GetDirectoryName(Me.mFileName)
        '        msg = New cMessage(String.Format(My.Resources.STATUS_SAVED_BIOMASSLIMITS, My.Resources.CAPTION, strPath), eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
        '        msg.Hyperlink = strPath
        '    End If
        '    'Save the Strategy to file
        '    'The filename was passed into the Strategy in its constructor
        '    Me.Save()

        'Catch ex As Exception
        '    breturn = False
        '    'Me.Save() will throw exceptions out to here
        '    Me.mCore.Messages.SendMessage(New cMessage("Exception saving Biomass Limits to file.", eMessageType.ErrorEncountered, eCoreComponentType.Plugin, eMessageImportance.Warning))
        'End Try

        'If msg IsNot Nothing Then
        '    Me.mCore.Messages.SendMessage(msg)
        'End If

        'Return breturn


        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If

        'Create a new file
        Dim writer As StreamWriter = cMSEUtils.GetWriter(strFilename, False)
        Dim bSuccess As Boolean = False

        If (writer Is Nothing) Then Return bSuccess

        Try
            writer.WriteLine("GroupNumber,LowerLimit,UpperLimit")

            For Each iBiomassLimit As cBiomassLimit In Me.lstBiomassLimits
                'writer.WriteLine(cStringUtils.ToCSVField(iBiomassLimit.mGroup.DBID) & "," &
                writer.WriteLine(cStringUtils.ToCSVField(iBiomassLimit.mGroup.Index) & "," &
                                 cStringUtils.ToCSVField(iBiomassLimit.mLowerLimit) & "," &
                                 cStringUtils.ToCSVField(iBiomassLimit.mUpperLimit))
            Next

            bSuccess = True

        Catch ex As Exception

        End Try
        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess

        Return True
    End Function

    Public Function IsChanged() As Boolean Implements IMSEData.IsChanged
        Return Me.m_bChanged
    End Function

    Public Sub Defaults() Implements IMSEData.Defaults
        For i As Integer = 1 To Me.mMSE.Core.nGroups
            'Me.Value(i) = cEffortLimits.NoHCR_F
            Me.lstBiomassLimits.Add(New cBiomassLimit(Me.mMSE.Core))
            Me.lstBiomassLimits(i - 1).mGroup = mMSE.Core.EcoPathGroupInputs(i)
            Me.lstBiomassLimits(i - 1).mLowerLimit = 0
            Me.lstBiomassLimits(i - 1).mUpperLimit = 1000000
        Next
        Me.m_bChanged = False
    End Sub

    Public Function FileExists(Optional strFilename As String = "") As Boolean Implements IMSEData.FileExists
        If String.IsNullOrWhiteSpace(strFilename) Then
            strFilename = Me.DefaultFileName()
        End If
        Return File.Exists(strFilename)
    End Function

    Private Function DefaultFileName() As String
        Return cMSEUtils.MSEFile(Me.mMSE.DataPath, cMSEUtils.eMSEPaths.BiomassLimits, "BiomassLimits.csv")
    End Function

End Class
