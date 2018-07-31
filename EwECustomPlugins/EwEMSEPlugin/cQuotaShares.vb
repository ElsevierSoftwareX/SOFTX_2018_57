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
Imports System.Collections.ObjectModel
Imports EwECore
Imports EwEUtils.Utilities
Imports LumenWorks.Framework.IO.Csv

#End Region ' Imports

Public Class cQuotaShares
    Implements IMSEData

#Region " Internal Structure "

    Public Class QuotaShare

        Public mGroupNo As Integer
        Public mFleetNo As Integer
        Public mShare As Single

        Public Sub New(GroupNo As Integer, FleetNo As Integer, Share As Single)
            mGroupNo = GroupNo
            mFleetNo = FleetNo
            mShare = Share
        End Sub

    End Class

#End Region

#Region " Internal Variables "
    Private m_lstQuotaShares As New List(Of QuotaShare)
    Private m_core As cCore
    Private m_MSE As cMSE
    Private mQuotaShareFileExists As Boolean
    Private m_bQuotaShareFileValid As Boolean
#End Region

#Region " Construction initialiaztion"

    Public Sub New(MSE As cMSE, core As EwECore.cCore)
        Me.m_core = core
        Me.m_MSE = MSE
        Me.m_lstQuotaShares = New List(Of QuotaShare)
        Me.Defaults()

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Checks whether the quota file is valid
    ''' </summary>
    Private Function QuotaFileValid() As Boolean
        Throw New NotImplementedException("QuotaFileValid not implemented")
        Return False
    End Function

    Public ReadOnly Property GetLstGrpShares As List(Of QuotaShare)
        Get
            Return m_lstQuotaShares
        End Get
    End Property

    ''' <summary>
    ''' Returns whether the Quota share file exists
    ''' </summary>
    Public Property QuotaFileExists() As Boolean
        Get
            Return mQuotaShareFileExists
        End Get
        Set(ByVal value As Boolean)
            mQuotaShareFileExists = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the number of elements in the quota share list
    ''' </summary>
    Public ReadOnly Property CountDist() As Integer
        Get
            Return m_lstQuotaShares.Count
        End Get
    End Property

#End Region

#Region " Functions "

    ''' <summary>
    ''' Checks whether the quota share file is valid
    ''' </summary>
    Private Function Check_QuotaShares_File_Okay() As Boolean

        '    Dim reader As StreamReader = Nothing
        '    Dim csv As CsvReader = Nothing
        '    'Dim correct(mCore.nGroups - 1) As Integer
        '    'Dim TotalFound As Integer = 0
        '    Dim bOK As Boolean = True

        '    reader = cMSEUtils.GetReader(strPath)
        '    If (reader Is Nothing) Then Return False

        '    '' Initialise correct to all zeros
        '    'For i = 1 To mCore.nGroups
        '    '    correct(i - 1) = 0
        '    'Next

        '    csv = New CsvReader(reader, True)
        '    Try
        '        'cycle through each of the living functional groups each time checking if it exists in the file
        '        ' JS 13Oct13: Changed the looping structure here. If csvreader fails to load a record it will repeat the last record!
        '        '             This created double-counting when a CSV file did not contain enough records
        '        While Not csv.EndOfStream
        '            If csv.ReadNextRecord() Then
        '                '            For xgrp = 1 To mCore.nGroups
        '                '                If (cStringUtils.ConvertToInteger(csv(0)) = xgrp) And (String.Compare(cMSEUtils.FromCSVField(csv(1)), _ecopath.EcopathData.GroupName(xgrp), True) = 0) Then
        '                '                    correct(xgrp - 1) += 1
        '                '                    ' Exit For ' JS: keep on checking to find duplicates
        '                '                End If
        '                '            Next
        '            End If
        '        End While
        '    Catch ex As Exception
        '        bOK = False
        '    End Try

        '    'csv.Dispose()
        '    'cMSEUtils.ReleaseReader(reader)

        '    '' Report file read error
        '    'If (bOK = False) Then
        '    '    Me.InformUser(String.Format(My.Resources.ERROR_CSV_MALFORMED, Path.GetFileName(strPath)), eMessageImportance.Warning)
        '    '    Return False
        '    'End If

        '    ''check that there are no replicates
        '    'For igrp = 1 To mCore.nGroups
        '    '    If correct(igrp - 1) > 1 Then
        '    '        Me.InformUser(String.Format(My.Resources.ERROR_DISTRPARAM_GROUPS_REPLICATED, Path.GetFileName(strPath)), eMessageImportance.Warning)
        '    '        Return False
        '    '    End If
        '    'Next

        '    ''sum all the values in correct to be use to diagnose whether there are the correct number of groups in the file
        '    'For Each i In correct
        '    '    TotalFound += i
        '    'Next

        '    '' Check whether there are too few groups in the file
        '    'If TotalFound < mCore.nLivingGroups Then
        '    '    Me.InformUser(String.Format(My.Resources.ERROR_DISTRFILE_GROUPS_LIVING_MISSING, Path.GetFileName(strPath)), eMessageImportance.Warning)
        '    '    Return False
        '    'ElseIf TotalFound > mCore.nLivingGroups Then 'Check whether there are too many groups in the file
        '    '    Me.InformUser(String.Format(My.Resources.ERROR_DISTRFILE_GROUPS_HASNONLIVING, Path.GetFileName(strPath)), eMessageImportance.Warning)
        '    '    Return False
        '    'End If

        ' Phew
        Return True

    End Function

    ''' <summary>
    ''' Adds a quota share value to the list of quota shares
    ''' and if it can't returns FALSE
    ''' </summary>
    Public Function AddQuotaShare(GroupNo As Integer, FleetNo As Integer, Share As Single) As Boolean

        'Check Fleet Number
        If FleetNo < 0 Or FleetNo > m_core.nFleets Then Return False

        'Check GroupNo
        If GroupNo < 0 Or GroupNo > m_core.nGroups Then Return False

        'Check Alpha and Beta
        If Share < 0 Or Share > 1 Then Return False

        'Add it to the list
        m_lstQuotaShares.Add(New QuotaShare(GroupNo, FleetNo, Share))

        Return True

    End Function

    ''' <summary>
    ''' Reads the iRow_th from the list of quotas
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <returns></returns>
    ''' <remarks>iRow is zero-based</remarks>
    Public Function ReadRowDist(iRow As Integer) As QuotaShare
        If iRow < 0 Then Return Nothing
        If iRow > m_lstQuotaShares.Count - 1 Then Return Nothing
        Return m_lstQuotaShares(iRow)
    End Function

    ''' <summary>
    ''' Reads the quota share that iFleet has of iGroup
    ''' </summary>
    ''' <param name="iFleet"></param>
    ''' <param name="iGroup"></param>
    ''' <returns></returns>
    ''' <remarks>iFleet and iGroup are zero-based</remarks>
    Public Function ReadiFleetiGroupQuotaShare(iFleet As Integer, iGroup As Integer, ByVal FlagNoQuotaShare As Boolean) As QuotaShare

        If iFleet < 1 Or iFleet > m_core.nFleets Then Return Nothing
        If iGroup < 1 Or iGroup > m_core.nGroups Then Return Nothing

        For iRow As Integer = 0 To m_lstQuotaShares.Count - 1
            If m_lstQuotaShares(iRow).mFleetNo = iFleet And m_lstQuotaShares(iRow).mGroupNo = iGroup Then Return m_lstQuotaShares(iRow)
        Next
        Return Nothing

    End Function

    Public Function QuotaSharesExistForGroup(iGrp As Integer) As Boolean

        For iRow As Integer = 0 To m_lstQuotaShares.Count - 1
            If m_lstQuotaShares(iRow).mGroupNo = iGrp Then
                Return True
            End If
        Next

        Return False

    End Function

    Public Function QuotaShareExistsForGroupFleet(ByVal iGrp As Integer, ByVal iFleet As Integer) As Boolean

        For iRow As Integer = 0 To m_lstQuotaShares.Count - 1
            If m_lstQuotaShares(iRow).mFleetNo = iFleet And m_lstQuotaShares(iRow).mGroupNo = iGrp Then Return True
        Next
        Return False

    End Function

#End Region

    Public Sub Defaults() _
        Implements IMSEData.Defaults

        Me.m_lstQuotaShares.Clear()

        'Dim TotalCatch As Double

        'For iGroup = 1 To m_core.nLivingGroups

        '    'Count how many fleets catch this group
        '    TotalCatch = 0
        '    For iFleet = 1 To m_core.nFleets
        '        TotalCatch += m_core.FleetInputs(iFleet).Landings(iGroup) + m_core.FleetInputs(iFleet).Discards(iGroup)
        '    Next

        '    For iFleet = 1 To m_core.nFleets
        '        If m_core.FleetInputs(iFleet).Landings(iGroup) > 0 Then
        '            AddQuotaShare(iGroup, iFleet, (m_core.FleetInputs(iFleet).Landings(iGroup) + m_core.FleetInputs(iFleet).Discards(iGroup)) / TotalCatch)
        '        End If
        '    Next

        'Next
    End Sub

    Public Function IsChanged() As Boolean Implements IMSEData.IsChanged
        Return True
    End Function

    Public Function Load(Optional msg As cMessage = Nothing,
                         Optional strFilename As String = "") As Boolean Implements IMSEData.Load

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If

        Dim reader As StreamReader = Nothing
        Dim csv As CsvReader = Nothing
        Dim iQuotaShare As QuotaShare
        Dim bSuccess As Boolean = True
        Dim filePath As String = strFilename

        Me.m_lstQuotaShares.Clear()

        If File.Exists(filePath) Then

            reader = cMSEUtils.GetReader(filePath)
            If (reader IsNot Nothing) Then
                Try
                    csv = New CsvReader(reader, True)
                    mQuotaShareFileExists = True
                    While Not csv.EndOfStream
                        iQuotaShare = ExtractQuotaShare(csv)
                        If (iQuotaShare IsNot Nothing) Then
                            AddQuotaShare(iQuotaShare.mGroupNo, iQuotaShare.mFleetNo, iQuotaShare.mShare)
                        End If
                    End While
                    csv.Dispose()

                Catch ex As Exception
                    'Debug.Assert(False, Me.ToString & ".LoadEcosimParameters() Exception: " & ex.Message)
                    cMSEUtils.LogError(msg, "Quota shared cannot load from " & strFilename & ". " & ex.Message)
                    bSuccess = False
                End Try
            End If
            cMSEUtils.ReleaseReader(reader)
        Else
            bSuccess = False
        End If

        Return bSuccess

    End Function

    ''' <summary>
    ''' Extracts a single quota share from the csv file
    ''' </summary>
    ''' <param name="csv">The CSV object linking to the quota share file</param>
    ''' <returns></returns>
    Private Function ExtractQuotaShare(ByVal csv As CsvReader) As QuotaShare

        ' Sanity checks
        If (csv Is Nothing) Then Return Nothing
        If (Not csv.ReadNextRecord()) Then Return Nothing

        Dim TGroupNumber As Integer
        Dim TFleetNumber As Integer
        Dim TShare As Single

        Try
            TGroupNumber = cStringUtils.ConvertToInteger(csv(0))
            TFleetNumber = cStringUtils.ConvertToInteger(csv(2))
            TShare = cStringUtils.ConvertToSingle(csv(4))

        Catch ex As Exception
            ' ToDo_JS: respond to error
            Return Nothing
        End Try

        Return New QuotaShare(TGroupNumber, TFleetNumber, TShare)

    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Save

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If

        Dim writer As StreamWriter = cMSEUtils.GetWriter(strFilename, False)
        Dim bSuccess As Boolean = False

        If (writer Is Nothing) Then Return bSuccess

        Try
            writer.WriteLine("GroupNumber,GroupName,FleetNumber,FleetName,QuotaShare")

            For Each entry As QuotaShare In m_lstQuotaShares
                writer.WriteLine(cStringUtils.ToCSVField(entry.mGroupNo) & "," &
                                 cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(entry.mGroupNo).Name) & "," &
                                 cStringUtils.ToCSVField(entry.mFleetNo) & "," &
                                 cStringUtils.ToCSVField(m_core.EcopathFleetInputs(entry.mFleetNo).Name) & "," &
                                 cStringUtils.ToCSVField(entry.mShare))
            Next

            bSuccess = True

        Catch ex As Exception

        End Try
        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess


    End Function

    Public Function FileExists(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.FileExists
        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If
        Return File.Exists(strFilename)
    End Function

    Private Function DefaultFileName() As String
        Return cMSEUtils.MSEFile(m_MSE.DataPath, cMSEUtils.eMSEPaths.Fleet, "QuotaShares.csv")
    End Function

    'Something that might only be used for testing purposes
    'Public Sub SetDefault()
    '    Dim nFleetsCatch As Integer

    '    If Not m_lstQuotaShares Is Nothing Then m_lstQuotaShares.Clear()

    '    For iGroup As Integer = 1 To m_core.nLivingGroups

    '        'Count how many fleets catch this group
    '        nFleetsCatch = 0
    '        For iFleet = 1 To m_core.nFleets
    '            If m_core.FleetInputs(iFleet).Landings(iGroup) > 0 Then nFleetsCatch += 1
    '        Next

    '        For iFleet = 1 To m_core.nFleets
    '            If m_core.FleetInputs(iFleet).Landings(iGroup) > 0 Then
    '                AddQuotaShare(iGroup, iFleet, 1 / nFleetsCatch)
    '            End If
    '        Next

    '    Next

    'End Sub

    ''' <summary>
    ''' Delete the shares if there is no longer a hcr associated with it
    ''' </summary>
    ''' <param name="strategies"></param>
    Sub RemoveUnnecessaryShares(strategies As Strategies)

        'Loop through each group
        For iGroup = 1 To m_core.nGroups
            'Check that the group has no hcr associated with it and whether there are shares associated with this group
            If Not strategies.HCRExistsForGroup(iGroup) And Me.QuotaSharesExistForGroup(iGroup) Then
                'If there are then delete them
                For iShare As Integer = Me.m_lstQuotaShares.Count - 1 To 0 Step -1
                    If Me.m_lstQuotaShares(iShare).mGroupNo = iGroup Then Me.m_lstQuotaShares.RemoveAt(iShare)
                Next
            End If
        Next

    End Sub

    Sub ModifyWithNewDefaults(strategies As Strategies)
        'This routine adds default values to quotashares object and csv because values are now necessary since hcrs have been created for new groups
        Dim Share As Single
        Dim TotalLandingsF As Single
        Dim TimeStep As Integer = m_MSE.EcosimData.NumStepsPerYear * m_MSE.EcosimData.NumYears

        If m_MSE.RunEcosim() Then

            Dim RelQModifier(,) As Single = Me.m_MSE.calcQModifiers(TimeStep)

            'Loop though each group
            For iGrp = 1 To m_core.nGroups
                'Check whether the group has an hcr for it and check whether there are quotashares set up for it
                If strategies.HCRExistsForGroup(iGrp) And Not Me.QuotaSharesExistForGroup(iGrp) Then
                    'if a strategy exists for the group but there aren't any quotashares create default quotashares in memory
                    TotalLandingsF = 0
                    For iFleet = 1 To m_core.nFleets
                        'TotalLandings += m_MSE.EcosimData.ResultsSumCatchByGroupGear(iGrp, iFleet, TimeStep) * m_MSE.EcopathData.PropLanded(iFleet, iGrp)
                        TotalLandingsF += m_MSE.EcosimData.relQ(iFleet, iGrp) * RelQModifier(iFleet, iGrp) * m_MSE.EcosimData.FishRateGear(iFleet, TimeStep) * m_MSE.EcopathData.PropLanded(iFleet, iGrp)
                    Next

                    For iFleet As Integer = 1 To m_core.nFleets
                        If m_core.EcopathFleetInputs(iFleet).Landings(iGrp) + m_core.EcopathFleetInputs(iFleet).Discards(iGrp) > 0 Then
                            Share = m_MSE.EcosimData.relQ(iFleet, iGrp) * RelQModifier(iFleet, iGrp) * m_MSE.EcosimData.FishRateGear(iFleet, TimeStep) * m_MSE.EcopathData.PropLanded(iFleet, iGrp) / TotalLandingsF
                            Me.AddQuotaShare(iGrp, iFleet, Share)
                        End If
                    Next

                End If
            Next
        Else
            'We should probably have an assert here because I believe it can't calculate the quotashares unless it can run ecosim
            'I believe this because there might be complications in calculating quotashares because they need to match up with the changes in
            'q that are applied to adjust for any forcing fs

            ''Loop though each group
            'For iGrp = 1 To m_core.nGroups
            '    'Check whether the group has an hcr for it and check whether there are quotashares set up for it
            '    If strategies.HCRExistsForGroup(iGrp) And Not Me.QuotaSharesExistForFleet(iGrp) Then
            '        'if a strategy exists for the group but there aren't any quotashares create default quotashares in memory
            '        TotalLandingsF = 0
            '        For iFleet = 1 To m_core.nFleets
            '            TotalLandingsF += m_core.FleetInputs(iFleet).Landings(iGrp)
            '        Next

            '        For iFleet As Integer = 1 To m_core.nFleets
            '            If m_core.FleetInputs(iFleet).Landings(iGrp) > 0 Then
            '                Share = m_core.FleetInputs(iFleet).Landings(iGrp) / TotalLandingsF
            '                Me.AddQuotaShare(iGrp, iFleet, Share)
            '            End If
            '        Next

            '    End If
            'Next
        End If



    End Sub

End Class
