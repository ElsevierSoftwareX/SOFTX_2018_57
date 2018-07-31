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

Namespace MSEBatchManager

    Public Class cMSEBatchDataStructures

        Public RunType As eMSEBatchRunTypes
        ''' <summary>
        ''' Number of PP forcing functions iterations
        ''' </summary>
        Public nForcing As Integer

        ''' <summary>
        ''' Is there forcing data in the command file
        ''' </summary>
        ''' <remarks></remarks>
        Public bForcingLoaded As Boolean

        ''' <summary>
        ''' Number of Control type iterations
        ''' </summary>
        ''' <remarks></remarks>
        Public nControlTypes As Integer

        ''' <summary>
        ''' Number of TFM (hockey stick) iteration
        ''' </summary>
        ''' <remarks></remarks>
        Public nTFM As Integer

        ''' <summary>
        ''' Number of Fixed Fishing Mortality iterations
        ''' </summary>
        ''' <remarks></remarks>
        Public nFixedF As Integer

        ''' <summary>
        ''' Number of Total Allowable Catch iterations
        ''' </summary>
        ''' <remarks></remarks>
        Public nTAC As Integer

        ''' <summary>
        ''' Number of iterations for the selected run type 
        ''' </summary>
        ''' <remarks>If RunType = eMSEBatchRunTypes.TFM then nParIters = nTFMs</remarks>
        Public nParIters As Integer


        ''' <summary>
        ''' Names of the loaded forcing functions
        ''' </summary>
        Public ForcingNames() As String

        ''' <summary>
        ''' Index to forcing function to use
        ''' </summary>
        Public ForcingIndexes() As Integer
        ''' <summary>
        ''' Index to group PP forcing function is applied to
        ''' </summary>
        ''' <remarks></remarks>
        Public ForcingGroup() As Integer

        ''' <summary>
        ''' number of Control type
        ''' </summary>
        ''' <remarks>dimensioned NControlTypes, nFleets</remarks>
        Public ControlType(,) As EwEUtils.Core.eQuotaTypes

        Public OuputType() As eMSEBatchOuputTypes
        Public isOuputSaved() As Boolean

        ''' <summary>
        ''' MSE Blim
        ''' </summary>
        ''' <remarks>tfmBlim(nTFM,nGroups) </remarks>
        Public tfmBlim(,) As Single
        Public tfmBbase(,) As Single
        Public tfmFmax(,) As Single
        Public tfmFmin(,) As Single


        Public FixedF(,) As Single
        Public TAC(,) As Single
        Public STDevForcing As Single
        Public isInit As Boolean

        Public iCurRun As Integer

        Public OuputDir As String

        Public StopRun As Boolean

        Public m_nGroups As Integer
        Public m_nFleets As Integer


        Public m_orgBlim() As Single
        Public m_orgBbase() As Single
        Public m_orgFmax() As Single
        Public m_orgFmin() As Single

        Public m_orgFixedF() As Single
        Public m_orgTAC() As Single

        Public CommandFilename As String

        Public VersionNumber As Single

        ''' <summary>
        ''' Database ID of the currently loaded Batch Scenario
        ''' </summary>
        ''' <remarks></remarks>
        Public ScenarioDBID As Integer

        Public TFMDBIDs() As Integer

        Public BlimLower() As Single
        Public BlimUpper() As Single

        Public BBaseLower() As Single
        Public BBaseUpper() As Single

        Public FOptLower() As Single
        Public FOptUpper() As Single

        Public FixedFLower() As Single
        Public FixedFUpper() As Single

        Public TACLower() As Single
        Public TACUpper() As Single

        Public IterCalcType As eMSEBatchIterCalcTypes = eMSEBatchIterCalcTypes.Percent

        Public GroupRunType() As eMSEBatchRunTypes


        Public ReadOnly Property nGroups As Integer
            Get
                Return Me.m_nGroups
            End Get
        End Property

        Public ReadOnly Property nFleets As Integer
            Get
                Return Me.m_nFleets
            End Get
        End Property

        Public Sub redimForcing(ByVal nForcingFunctions As Integer)
            nForcing = nForcingFunctions
            If nForcing = 0 Then nForcing = 1

            ReDim ForcingGroup(nForcing)
            ReDim ForcingIndexes(nForcing)
            ReDim ForcingNames(nForcing)

        End Sub


        Public ReadOnly Property nOuputTypes() As Integer
            Get
                Return System.Enum.GetValues(GetType(eMSEBatchOuputTypes)).Length
            End Get
        End Property

        Public Sub redimTFM(ByVal nTFM As Integer, ByVal nGroups As Integer)
            Me.nTFM = nTFM
            If nTFM = 0 Then nTFM = 1

            ReDim tfmBlim(Me.nTFM, nGroups)
            ReDim tfmBbase(Me.nTFM, nGroups)
            ReDim tfmFmax(Me.nTFM, nGroups)
            ReDim tfmFmin(Me.nTFM, nGroups)

            ReDim TFMDBIDs(nGroups)

            ' Temporary fix: set group dbids to bogus values; needs to be configured from Datasource
            For i As Integer = 1 To nGroups
                TFMDBIDs(i) = i
            Next

            ReDim BlimLower(nGroups)
            ReDim BlimUpper(nGroups)

            ReDim BBaseLower(nGroups)
            ReDim BBaseUpper(nGroups)

            ReDim FOptLower(nGroups)
            ReDim FOptUpper(nGroups)

            '    Me.setDefaultTFM()

        End Sub

        Public Sub redimFixedF(ByVal nFIters As Integer, ByVal nGroups As Integer)
            Me.nFixedF = nFIters
            If nFixedF = 0 Then nFixedF = 1

            ReDim FixedF(nFixedF, nGroups)
            ReDim FixedFLower(nGroups)
            ReDim FixedFUpper(nGroups)

        End Sub

        Public Sub redimTAC(ByVal nTACIters As Integer, ByVal nGroups As Integer)
            Me.nTAC = nTACIters
            If nTAC = 0 Then nTAC = 1

            ReDim TAC(nTAC, nGroups)
            ReDim TACLower(nGroups)
            ReDim TACUpper(nGroups)

        End Sub

        Public Sub redimControlTypes(ByVal nTypes As Integer, ByVal nFleets As Integer)
            nControlTypes = nTypes
            If nControlTypes = 0 Then nControlTypes = 1

            ReDim ControlType(nControlTypes, nFleets)

        End Sub

        Public Sub redimOuputTypes()

            ReDim Me.OuputType(Me.nOuputTypes)
            ReDim Me.isOuputSaved(Me.nOuputTypes)

        End Sub

        Public Sub setDefaultLimits()
            Dim defautlLL As Single = 0.5
            Dim defautlUp As Single = 1.0
            For igrp As Integer = 1 To Me.nGroups

                GroupRunType(igrp) = eMSEBatchRunTypes.TFM

                BlimLower(igrp) = defautlLL
                BlimUpper(igrp) = defautlUp

                BBaseLower(igrp) = defautlLL
                BBaseUpper(igrp) = defautlUp

                FOptLower(igrp) = defautlLL
                FOptUpper(igrp) = defautlUp

                FixedFLower(igrp) = defautlLL
                FixedFUpper(igrp) = defautlUp

                TACLower(igrp) = defautlLL
                TACUpper(igrp) = defautlUp

            Next
        End Sub

        'Public Sub New()
        '    Me.bForcingLoaded = False
        'End Sub


        Public Sub New(MSEdata As MSE.cMSEDataStructures)
            Try
                Me.redimToMSE(MSEdata)
            Catch ex As Exception

            End Try
        End Sub

        Private Sub redimToMSE(MSEdata As MSE.cMSEDataStructures)

            Me.m_nGroups = MSEdata.NGroups
            Me.m_nFleets = MSEdata.nFleets

            ReDim GroupRunType(MSEdata.NGroups)

            Me.redimForcing(1)
            Me.redimControlTypes(1, MSEdata.nFleets)

            Me.redimOuputTypes()
            Me.redimTAC(1, MSEdata.NGroups)
            Me.redimTFM(1, MSEdata.NGroups)
            Me.redimFixedF(1, MSEdata.NGroups)

        End Sub


        ''' <summary>
        ''' Store the initial state of the MSE data so it can be restored later
        ''' </summary>
        ''' <param name="MSEdata"></param>
        ''' <remarks></remarks>
        Public Sub StoreMSEState(ByVal MSEdata As EwECore.MSE.cMSEDataStructures)
            ReDim m_orgBlim(Me.nGroups)
            ReDim m_orgBbase(Me.nGroups)
            ReDim m_orgFmax(Me.nGroups)
            ReDim m_orgFmin(Me.nGroups)

            ReDim m_orgFixedF(Me.nGroups)
            ReDim m_orgTAC(Me.nGroups)

            For igrp As Integer = 1 To Me.nGroups
                m_orgBlim(igrp) = MSEdata.Blim(igrp)
                m_orgBbase(igrp) = MSEdata.Bbase(igrp)
                m_orgFmax(igrp) = MSEdata.Fopt(igrp)
                m_orgFmin(igrp) = MSEdata.Fmin(igrp)

                m_orgFixedF(igrp) = MSEdata.FixedF(igrp)
                m_orgTAC(igrp) = MSEdata.TAC(igrp)
            Next

        End Sub

        ''' <summary>
        ''' Restore the MSE data to its original state
        ''' </summary>
        ''' <param name="MSEdata"></param>
        ''' <remarks></remarks>
        Public Sub ReStoreMSEState(ByVal MSEdata As EwECore.MSE.cMSEDataStructures)
            Try

                For igrp As Integer = 1 To Me.nGroups
                    MSEdata.Blim(igrp) = m_orgBlim(igrp)
                    MSEdata.Bbase(igrp) = m_orgBbase(igrp)
                    MSEdata.Fopt(igrp) = m_orgFmax(igrp)
                    MSEdata.Fmin(igrp) = m_orgFmin(igrp)

                    MSEdata.FixedF(igrp) = m_orgFixedF(igrp)
                    MSEdata.TAC(igrp) = m_orgTAC(igrp)
                Next

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.Message)
            End Try

        End Sub



    End Class


End Namespace