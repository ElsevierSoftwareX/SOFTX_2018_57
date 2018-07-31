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
Imports Microsoft.VisualBasic
Imports EwEMSEPlugin.HCR_GroupNS
Imports EwEMSEPlugin.HCR_GroupNS.HCR_Group


#End Region ' Imports 

''' <summary>
''' Class to group a list of Harvest Control Rules into an object
''' </summary>
Public Class Strategy
    Implements IList(Of HCR_Group)
    Implements IMSEData

    Private m_HCRsList As New List(Of HCR_Group)
    Private m_core As cCore = Nothing
    Private m_MSE As cMSE = Nothing
    Private m_EcosimData As cEcosimDatastructures = Nothing
    Public Property RunThisStrategy As Boolean = True
    Public Property Name As String = ""
    Public Property FileName As String = ""

    Const LatestVersion = "V2"

    Public Sub New(ByVal StrategyName As String, StrategyNumber As Integer, ByVal theFilename As String, Core As cCore, MSE As cMSE)
        Me.m_core = Core
        Me.m_MSE = MSE
        Me.m_EcosimData = MSE.EcosimData
        Me.Name = StrategyName
        Me.FileName = theFilename
        Me.Regulations = New cRegulations(MSE, Core)
        Me.StrategyNumber = StrategyNumber
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Resolve a name and index to a <see cref="cEcoPathGroupInput"/> instance.
    ''' </summary>
    ''' <param name="strName">The name to resolve.</param>
    ''' <param name="iIndex">The index to resolve.</param>
    ''' <returns>A <see cref="cEcoPathGroupInput"/> instance, or Nothing if
    ''' the index or name did not match any of the present groups.</returns>
    ''' <remarks>Note that name comparison is not case sensitive.</remarks>
    ''' -----------------------------------------------------------------------
    Private Function ResolveGroup(strName As String, iIndex As Integer) As cEcoPathGroupInput
        If (iIndex < 1) Or (iIndex > Me.m_core.nGroups) Then Return Nothing
        Dim grp As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(iIndex)
        Dim grpName As String = cMSEUtils.FromCSVField(strName)
        If String.Compare(grp.Name, grpName, True) <> 0 Then
            Return Nothing
        End If
        Return grp
    End Function

    Public Property StrategyNumber() As Integer
    Public Property Regulations As cRegulations

    Public Overrides Function ToString() As String
        Return MyBase.ToString() & ":" & Me.Name
    End Function

#Region " IList implementation "

    Public Sub Add(item As HCR_Group) Implements ICollection(Of HCR_Group).Add
        If Not Me.Contains(item) Or item.Targ_Or_Cons = eHCR_Targ_Or_Cons.Conservation Then
            Me.m_HCRsList.Add(item)
        End If
    End Sub

    Public Sub Clear() Implements ICollection(Of HCR_Group).Clear
        Me.m_HCRsList.Clear()
    End Sub

    Public Function Contains(item As HCR_Group) As Boolean Implements ICollection(Of HCR_Group).Contains
        For Each Rule As HCR_Group In Me
            If ReferenceEquals(item.GroupB, Rule.GroupB) And ReferenceEquals(item.GroupF, Rule.GroupF) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub CopyTo(array() As HCR_Group, arrayIndex As Integer) Implements ICollection(Of HCR_Group).CopyTo
        ' NOP
    End Sub

    Public ReadOnly Property Count As Integer Implements ICollection(Of HCR_Group).Count
        Get
            Return Me.m_HCRsList.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of HCR_Group).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Function Remove(item As HCR_Group) As Boolean Implements ICollection(Of HCR_Group).Remove
        Return Me.m_HCRsList.Remove(item)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of HCR_Group) Implements IEnumerable(Of HCR_Group).GetEnumerator
        Return Me.m_HCRsList.GetEnumerator()
    End Function

    Public Function IndexOf(item As HCR_Group) As Integer Implements IList(Of HCR_Group).IndexOf
        Return Me.m_HCRsList.IndexOf(item)
    End Function

    Public Sub Insert(index As Integer, item As HCR_Group) Implements IList(Of HCR_Group).Insert
        Me.m_HCRsList.Insert(index, item)
    End Sub

    Public Function StrategyContainsHCRforiGrp(ByVal iGrp As Integer) As Boolean
        'Checks to see whether this strategy has an HCR for the indexed group
        For Each iHCR In m_HCRsList
            If iHCR.GroupF.Index = iGrp Then
                Return True
            End If
        Next
        Return False
    End Function

    Default Public Property Item(index As Integer) As HCR_Group Implements IList(Of HCR_Group).Item
        Get
            Return Me.m_HCRsList.Item(index)
        End Get
        Set(value As HCR_Group)
            Me.m_HCRsList(index) = value
        End Set
    End Property

    Public Sub RemoveAt(index As Integer) Implements IList(Of HCR_Group).RemoveAt
        Me.m_HCRsList.RemoveAt(index)
    End Sub

    Private Function Bogus() As IEnumerator Implements IEnumerable.GetEnumerator
        ' NOP
        Return Nothing
    End Function

#End Region ' IList implementation

#Region " IMSEData implementation "

    Public Sub Defaults() _
        Implements IMSEData.Defaults
        Me.Clear()
    End Sub

    Public Function IsChanged() As Boolean _
        Implements IMSEData.IsChanged
        ' ToDo: implement this properly
        Return False
    End Function

    Function IsOldVersionHCRFile(Line1 As String()) As Boolean

        If Line1.Length() = 2 Then Return True
        If Line1.Length() > 2 Then
            If Line1(4) = LatestVersion Then
                Return False
            Else
                Return True
            End If
        End If

        Return Nothing

    End Function

    Public Sub UpdateHCRFile(strFilename As String)
        'Updates to latest version to latest version so that it works with latest modifications in the code

        Dim strMsg As String = ""
        Dim buff As String
        Dim recs() As String
        Dim bSuccess As Boolean = True
        Dim RunSetting As Integer
        Dim mod_strFilename As String

        Dim reader As StreamReader = cMSEUtils.GetReader(strFilename)

        'to help distinguish between reg and hcr files add _hcr onto end of file name if not already there
        If Microsoft.VisualBasic.Strings.Right(strFilename, 8) <> "_hcr.csv" Then
            mod_strFilename = Left(strFilename, Len(strFilename) - 4) & "_hcr.csv"
        Else
            mod_strFilename = strFilename
        End If

        If (reader IsNot Nothing) Then
            buff = reader.ReadLine()
            recs = buff.Split(","c)

            'Latest version of hcr files contains info that it is version v2 to distinguish it from previous versions
            'Use this to check whether latest version
            If IsOldVersionHCRFile(recs) Then

                'Save with v2 prefix so that it doesn't overwrite original hcr file because I need to be able to read from that
                'Remove prefix later
                Dim writer As StreamWriter = cMSEUtils.GetWriter(mod_strFilename & LatestVersion)

                'In an older update we added the info of whether the hcr was for a strategy to be run so that user
                'could specify a subset of the strategies to run and create results for
                'Check if this info exists and if not set to a default of 1
                If recs(0) = "Run" Then
                    RunSetting = cStringUtils.ConvertToInteger(recs(1))
                Else
                    RunSetting = 1
                End If

                'Write hcr file info and hcr headings
                writer.WriteLine("Run," & RunSetting & ",,Version No.," & LatestVersion)
                writer.WriteLine("GroupNameForBiomass,HCRType,GroupNumberForBiomass,LowerLimit,StepBiomass,UpperLimit,GroupNameForF,GroupNumberForF,MinF,MaxF,Target_or_Conservation,NYears(Time Frame Rule)")

                'Need write to new file the old file info but with values for new columns
                Do Until reader.EndOfStream
                    buff = reader.ReadLine()
                    recs = buff.Split(","c)
                    If recs(0) <> "Run" And recs(0) <> "GroupNameForBiomass" Then
                        writer.Write(recs(0) & ",")     'GroupNameForBiomass
                        writer.Write("0,")              'HCRType
                        writer.Write(recs(1) & ",")     'GroupNumberForBiomass
                        writer.Write(recs(2) & ",")     'LowerLimit
                        writer.Write(",")         'StepBiomass
                        writer.Write(recs(3) & ",")     'UpperLimit
                        writer.Write(recs(4) & ",")     'GroupNameForF
                        writer.Write(recs(5) & ",")     'GroupNumberForF
                        writer.Write(",")           'MinF
                        writer.Write(recs(6) & ",")     'MaxF
                        writer.Write(recs(7) & ",")     'Target_or_Conservation
                        writer.Write(recs(8) & ",")     'NYears(Time Frame Rule)

                        writer.WriteLine()
                    End If

                Loop

                writer.Close() : writer.Dispose()
                reader.Close() : reader.Dispose()

                'Replace the old file with the new one ready to be read and used by the plugin
                File.Delete(strFilename)
                My.Computer.FileSystem.RenameFile(mod_strFilename & LatestVersion, Path.GetFileName(mod_strFilename))
            Else
                reader.Close() : reader.Dispose()

            End If

        Else

            reader.Close() : reader.Dispose()

        End If

    End Sub

    Public Function Load(Optional msg As cMessage = Nothing,
                         Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Load

        Dim strMsg As String = ""
        Dim buff As String
        Dim recs() As String
        Dim bSuccess As Boolean = True

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.FileName
        End If

        If Not File.Exists(strFilename) Then
            ' OK: data loaded with defaults
            Return bSuccess
        End If

        UpdateHCRFile(strFilename)

        Dim reader As StreamReader = cMSEUtils.GetReader(strFilename)
        If (reader IsNot Nothing) Then

            Do Until reader.EndOfStream
                buff = reader.ReadLine()
                recs = buff.Split(","c)
                If recs(0) = "Run" Then
                    If cStringUtils.ConvertToInteger(recs(1)) = 1 Then
                        Me.RunThisStrategy = True
                    Else
                        Me.RunThisStrategy = False
                    End If
                    reader.ReadLine()
                ElseIf recs(0) <> "GroupNameForBiomass" Then
                    Dim tempHCRGroup As HCR_Group
                    'Each HCR Group needs to be a new object
                    tempHCRGroup = New HCR_Group(m_core, m_MSE)

                    Try
                        ' Resolve group
                        tempHCRGroup.GroupB = Me.ResolveGroup(recs(0), cStringUtils.ConvertToInteger(recs(2)))
                        tempHCRGroup.HCR_Type = CType(cStringUtils.ConvertToInteger(recs(1)), eHCR_Type)
                        tempHCRGroup.LowerLimit = cStringUtils.ConvertToSingle(recs(3))
                        tempHCRGroup.BStep = cStringUtils.ConvertToSingle(recs(4))
                        tempHCRGroup.UpperLimit = cStringUtils.ConvertToSingle(recs(5))
                        tempHCRGroup.GroupF = Me.ResolveGroup(recs(6), cStringUtils.ConvertToInteger(recs(7)))
                        tempHCRGroup.MinF = cStringUtils.ConvertToSingle(recs(8))
                        tempHCRGroup.MaxF = cStringUtils.ConvertToSingle(recs(9))
                        If Not [Enum].TryParse(recs(10), tempHCRGroup.Targ_Or_Cons) Then
                            tempHCRGroup.Targ_Or_Cons = CType(CInt(recs(10)), eHCR_Targ_Or_Cons)
                        End If
                        'backwards compatability with older file that don't contain Time Frame Rules
                        If recs.Length > 8 Then
                            tempHCRGroup.TimeFrameRule.NYears = cStringUtils.ConvertToInteger(recs(11))
                        Else
                            tempHCRGroup.TimeFrameRule.NYears = 0
                        End If
                    Catch ex As Exception
                        ' Whoah!
                        cMSEUtils.LogError(msg, "Strategy could not load from " & strFilename & ". " & ex.Message)
                        bSuccess = False
                        Exit Do
                    End Try

                    ' Only add valid strategies!
                    If tempHCRGroup.isValid(strMsg) Then
                        Me.Add(tempHCRGroup)
                    Else
                        cMSEUtils.LogError(msg, "Strategy loaded from " & strFilename & " is not valid.")
                        bSuccess = False
                    End If

                End If

            Loop
            End If 'cMSEUtils.readToTag(reader, START_TAG)

            cMSEUtils.ReleaseReader(reader)

        'for debugging
        Debug.Assert(bSuccess, Me.ToString + ".Load() Failed to read hcrs from file.")

        Return bSuccess
    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Save

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.FileName
        End If

        If Not strFilename.Contains("_hcr") Then
            Dim index_of_csv As Integer = strFilename.IndexOf(".csv")
            strFilename = strFilename.Insert(index_of_csv, "_hcr")
        End If

        Dim strm As StreamWriter = cMSEUtils.GetWriter(strFilename, False)
        If (strm IsNot Nothing) Then
            If Me.RunThisStrategy Then
                'strm.WriteLine("Run, 1")
                strm.WriteLine("Run,1" & ",,Version No.," & LatestVersion)
            Else
                'strm.WriteLine("Run, 0")
                strm.WriteLine("Run,0" & ",,Version No.," & LatestVersion)
            End If

            'msg.AddVariable(New cVariableStatus(eStatusFlags.OK, _
            '                                    String.Format(My.Resources.STATUS_SAVED_DETAIL, Path.GetFileName(iStrategy.FileName)), _
            '                                    eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))
            strm.WriteLine("GroupNameForBiomass,HCRType,GroupNumberForBiomass,LowerLimit,StepBiomass,UpperLimit,GroupNameForF,GroupNumberForF,Min,MaxF,Target_or_Conservation, NYears(Time Frame Rule)")
            For Each iHCR In Me
                Dim row2output As String = cStringUtils.ToCSVField(iHCR.GroupB.Name) & "," &
                                          cStringUtils.ToCSVField(iHCR.HCR_Type) & "," &
                                          cStringUtils.ToCSVField(iHCR.GroupB.Index) & "," &
                                          cStringUtils.ToCSVField(iHCR.LowerLimit) & "," &
                                          cStringUtils.ToCSVField(iHCR.BStep) & "," &
                                          cStringUtils.ToCSVField(iHCR.UpperLimit) & "," &
                                          cStringUtils.ToCSVField(iHCR.GroupF.Name) & "," &
                                          cStringUtils.ToCSVField(iHCR.GroupF.Index) & "," &
                                          cStringUtils.ToCSVField(iHCR.MinF) & "," &
                                          cStringUtils.ToCSVField(iHCR.MaxF) & "," &
                                          cStringUtils.ToCSVField(iHCR.Targ_Or_Cons) & "," &
                                          cStringUtils.ToCSVField(iHCR.TimeFrameRule.NYears)
                strm.WriteLine(row2output)
            Next
        End If
        cMSEUtils.ReleaseWriter(strm)

        Return True
    End Function

#End Region ' IMSEData implementation

    Public Function FileExists(Optional strFilename As String = "") As Boolean Implements IMSEData.FileExists
        Return FileExists(strFilename)
    End Function
End Class
