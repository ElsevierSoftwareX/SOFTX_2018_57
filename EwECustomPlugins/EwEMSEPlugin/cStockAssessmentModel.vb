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

#Region "Imports"
Imports System.IO
Imports EwECore

Imports EwEUtils.Utilities

#End Region


Public Class cStockAssessmentModel
    Implements IMSEData

    'Stock assessment model and strategies
    'For now there will be one stock assessment model for all the strategies
    'Groups CV's should be by strategy, I'm not sure what this means for the code or interface right now so I'll just ignore it...

    'ToDo Saving and restoration of the data. 
    '   Set defaults from core data. 
    '   Save and read to file.

    'ToDo Sort out how the Stratigies interact with the model
    'Todo Debug initialization of Stock Assessment model and Ecosim, figure out how this works with the strategies

    'ToDo use of CVbiomEst() in the Kalman Gain filter. Should this be it's own variable?


#Region "Private data"

    Private Const MODEL_HEADER As String = "'ModelParametersName','Value'"

    Private m_MSE As cMSE
    Private m_core As cCore

    Private Bestimate() As Single
    Private BestimateLast() As Single
    Private KalmanGain() As Single

    ' ''' <summary>
    ' ''' Random normal distribution with a mean = 0 standard deviation = 1
    ' ''' </summary>
    ' ''' <remarks></remarks>
    'Private m_NormalDist As Troschuetz.Random.NormalDistribution

    Private m_strmBobsB As StreamWriter

    Private m_lstParams As List(Of cStockAssessmentParameters)

    Private m_lstFleets As List(Of cStockAssessmentFleetParameters)

    Private m_simdata As cEcosimDatastructures
    Private m_pathdata As cEcopathDataStructures

    Private m_isChanged As Boolean

    Private m_rand As Random

    Private m_iTrial As Integer

#End Region

#Region "Pubic data"

    Public Rmax() As Single
    Public BhalfT() As Single

    ''' <summary>
    ''' CV of observation error by group
    ''' </summary>
    ''' <remarks></remarks>
    Public CVbiomEst() As Single

    Public GstockPred() As Single
    Public RstockRatio() As Single
    Public RStock0() As Single

    ''' <summary>
    ''' Input Ratio of Bt to B0 needed for 50% of max recruitment 
    ''' </summary>
    Public RHalfB0Ratio() As Single

    Public cvRec() As Single

    ''' <summary>
    ''' CV of implementation error by fleet
    ''' </summary>
    ''' <remarks></remarks>
    Public CVImpError() As Single

    Public CVRecruitError() As Single

    Public UseAssessment As Boolean

#End Region

#Region "Construction and Initialization"

    Public Sub New(ByVal MSE As cMSE)
        Me.m_MSE = MSE
        Me.m_core = m_MSE.Core

        Me.m_simdata = Me.MSE.EcosimData
        Me.m_pathdata = Me.MSE.EcopathData

        Me.m_isChanged = False
        'Me.UseAssessment = True

        Debug.Assert(Me.m_MSE IsNot Nothing, "cStockAssessmentModel must have a valid cMSE object during initialization!")
        Debug.Assert(Me.m_core IsNot Nothing, "cStockAssessmentModel must have a valid cCore object during initialization!")

        Me.Init()

    End Sub

    Public Sub InitForRun()

        Me.InitBiomass()
        Me.InitStockAssessment()

    End Sub

    Public Sub ResetSeed(ByVal iModel As Integer)
        m_rand = New Random(iModel)
    End Sub

    Private Sub Init()
        Try

            Me.InitData()

            'Me.m_NormalDist = New Troschuetz.Random.NormalDistribution()
            Me.m_rand = New Random(666)

            ''This will reset the random number generator with the same seed each time 
            ''the same sequence of random numbers will be generated on each call to NextDouble()
            ''see http://www.codeproject.com/Articles/15102/NET-random-number-generators-and-distributions
            ''The Reset() will need to be called each time the MSE is run
            ''Right now this is called just once when the Ecosim scenario is loaded
            'Me.m_NormalDist.Reset()
            'Me.m_NormalDist.Mu = 0
            'Me.m_NormalDist.Sigma = 1

            ''For now just copy the data from the core into local arrays
            ''Me.InitToCoreData()

            Me.InitStockAssessment()
            Me.InitInterfaceParameters()

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            'Me.MSE.InformUser(...)
        End Try

    End Sub


    Private Sub InitInterfaceParameters()
        Try

            Me.m_lstParams = New List(Of cStockAssessmentParameters)
            'Include the zero element so the indexing matches up with the core one based indexes
            For igrp As Integer = 0 To Me.m_core.nGroups
                Me.m_lstParams.Add(New cStockAssessmentParameters(igrp, Me, Me.m_simdata, Me.m_pathdata))
            Next

            m_lstFleets = New List(Of cStockAssessmentFleetParameters)
            'Include the zero element so the indexing matches up with the core one based indexes
            For iflt As Integer = 0 To Me.m_core.nGroups
                Me.m_lstFleets.Add(New cStockAssessmentFleetParameters(iflt, Me, Me.m_pathdata))
            Next

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            'Me.MSE.InformUser(...)
        End Try

    End Sub


    Private Sub InitBiomass()
        'Init Bestimate() and BestimateLast() to the start biomass with some error
        For igrp As Integer = 1 To Me.Core.nLivingGroups
            Bestimate(igrp) = m_simdata.StartBiomass(igrp) * CSng(Math.Exp(CVbiomEst(igrp) * getNextRandNormal()))
            BestimateLast(igrp) = Bestimate(igrp)
        Next igrp

    End Sub

    Private Sub InitStockAssessment()
        Dim BaB As Single
        Try

            'init RstockPred from GstockPred
            'GstockPred could have been altered by an interface
            For igrp = 1 To Me.Core.nLivingGroups
                'BaB is correct for Stanza groups because Ecopath.BA() gets updated with Stanza.BaBsplit()
                BaB = m_pathdata.BA(igrp) / m_pathdata.B(igrp)
                'gstockpred=exp(bab)-rstockratio, rather than 1-rstockratio.  Check to insure gstockpred>0

                'Me.m_data.GstockPred(igrp) = 1 - Me.m_data.RstockRatio(igrp)
                GstockPred(igrp) = CSng(Math.Exp(BaB) - RstockRatio(igrp))
                If GstockPred(igrp) < 0 Then GstockPred(igrp) = 0
                BhalfT(igrp) = RHalfB0Ratio(igrp) * m_pathdata.B(igrp)

                RStock0(igrp) = RstockRatio(igrp) * m_simdata.StartBiomass(igrp)
                Rmax(igrp) = RStock0(igrp) * (RHalfB0Ratio(igrp) + 1)

            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            'Me.MSE.InformUser(...)
        End Try

    End Sub

    Private Sub InitToCoreData()

        Dim MSEData As MSE.cMSEDataStructures = Me.MSE.CoreMSEData

        Array.Copy(MSEData.Rmax, Rmax, Me.Core.nLivingGroups)
        Array.Copy(MSEData.BhalfT, BhalfT, Me.Core.nLivingGroups)
        Array.Copy(MSEData.CVbiomEst, CVbiomEst, Me.Core.nLivingGroups)
        Array.Copy(MSEData.GstockPred, GstockPred, Me.Core.nLivingGroups)
        Array.Copy(MSEData.RstockRatio, RstockRatio, Me.Core.nLivingGroups)
        Array.Copy(MSEData.RStock0, RStock0, Me.Core.nLivingGroups)
        Array.Copy(MSEData.RHalfB0Ratio, RHalfB0Ratio, Me.Core.nLivingGroups)
        Array.Copy(MSEData.cvRec, cvRec, Me.Core.nLivingGroups)

        'HACK WARNING EwE does not have a seperate error on recruitment
        'it uses the observed biomass error
        Array.Copy(MSEData.CVbiomEst, CVRecruitError, Me.Core.nLivingGroups)

        Array.Copy(MSEData.CVFest, CVImpError, Me.Core.nFleets)

    End Sub

    Private Sub InitData()

        Bestimate = New Single(Me.Core.nLivingGroups) {}
        BestimateLast = New Single(Me.Core.nLivingGroups) {}

        Rmax = New Single(Me.Core.nLivingGroups) {}
        BhalfT = New Single(Me.Core.nLivingGroups) {}
        CVbiomEst = New Single(Me.Core.nLivingGroups) {}
        GstockPred = New Single(Me.Core.nLivingGroups) {}
        RstockRatio = New Single(Me.Core.nLivingGroups) {}
        RStock0 = New Single(Me.Core.nLivingGroups) {}
        RHalfB0Ratio = New Single(Me.Core.nLivingGroups) {}
        cvRec = New Single(Me.Core.nLivingGroups) {}
        KalmanGain = New Single(Me.Core.nLivingGroups) {}

        CVRecruitError = New Single(Me.Core.nLivingGroups) {}

        CVImpError = New Single(Me.Core.nFleets) {}

    End Sub

    Private Sub InitRandom()

        'HACK Troschuetz.Random.NormalDistribution
        'Calling Reset() is suppose to set the seed back to the default and generate the same random sequence
        'BUT IT DOESN'T bitches... so we'll do it ourselves
        'Me.m_NormalDist.Mu = 0
        'Me.m_NormalDist.Sigma = 1
        'Me.m_NormalDist.Reset()

        Me.m_rand = New Random(666)

        'System.Console.WriteLine()
        'System.Console.WriteLine("---------New Random-----------")

    End Sub


#End Region

#Region "Public Methods"


    Public Function DoAnnualStockAssessment(ByVal Strategy As Strategy, iTimestep As Integer, Biomass() As Single) As Single()
        Try
            'Run the Stock assessment model at the start of each year.
            'This returns the biomass for the comming year that the quotas will be based on. 

            '1. Get the observed biomass 
            '2. Added observation error to the biomass
            '3. Use the observed biomass with error as input to the stock recruitment model
            '4. Return the biomass predicted by the stock recruitment model as the true biomass for the comming year
            If Me.UseAssessment Then

                Dim nGrps As Integer = Me.Core.nLivingGroups
                Dim Bobs() As Single = New Single(nGrps) {}

                'get average biomass for the last year
                Dim Bavg() As Single = Me.getAvgB(iTimestep)

                For igrp As Integer = 1 To nGrps
                    'Get the Observed Biomass
                    'Average biomass from the last year with sampling error
                    Bobs(igrp) = Bavg(igrp) * CSng(Math.Exp(CVbiomEst(igrp) * getNextRandNormal()))

                    'Get the estimated biomass base on the observed biomass plus uncertainty
                    'Using the stock recruitment curve from the EwE6 MSE interface
                    Me.Bestimate(igrp) = Me.stockRecruitment(iTimestep, igrp, Bobs(igrp), Me.Bestimate(igrp))

                Next igrp

                'For debugging dump BioEst/B to file
                Me.dumpBioEstOverB(Strategy, iTimestep, Me.Bestimate, Biomass)

                Return Me.Bestimate

            Else
                'Not using the Stock Assessment model so just return the unaltered biomass
                Return Biomass
            End If

        Catch ex As Exception
            Debug.Assert(False, "Opps Exception in DoAnnualStockAssessment(). " + ex.Message)
        End Try

        Return Biomass

    End Function


    Public Sub RunEnded()

        cMSEUtils.ReleaseWriter(m_strmBobsB)

    End Sub

    Public Sub BeginRun()

        Me.InitOutputFiles()
        Me.InitRandom()

    End Sub

    Public ReadOnly Property Parameter(ByVal iGroupIndex As Integer) As cStockAssessmentParameters
        Get
            Try
                Return Me.m_lstParams.Item(iGroupIndex)
            Catch ex As Exception
                Debug.Assert(False, "Opps bug. Indexing error in Parameter()")
            End Try
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property FleetParameter(ByVal iFleetIndex As Integer) As cStockAssessmentFleetParameters
        Get
            Try
                Return Me.m_lstFleets.Item(iFleetIndex)
            Catch ex As Exception
                Debug.Assert(False, "Opps bug. Indexing error in FleetParameter()")
            End Try
            Return Nothing
        End Get
    End Property


    Public Sub OnParameterChanged(ByVal iGroupIndex As Integer)
        'InitStockAssessment()
        Me.m_isChanged = True
    End Sub

    Public Function getImplementationError(iFleet As Integer) As Single

        Return CSng(Math.Exp(Me.CVImpError(iFleet) * getNextRandNormal()))
        'Return CSng(Math.Exp(Me.CVImpError(iFleet) * getNextRandNormal(True, iFleet)))

    End Function

    Public WriteOnly Property TrialNumber As Integer
        Set(value As Integer)
            Me.m_iTrial = value
        End Set
    End Property

#End Region

#Region "Private Properties and  Methods"

    Private Function getNextRandNormal() As Single
        Dim val As Single
        Dim V1 As Double, V2 As Double

        'Me.m_rand() gets re-seeded for every run in InitRandom()

        'Box Muller transformation 
        'http://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform
        'Mean = 0 SD = 1
        Do
            V1 = Me.m_rand.NextDouble
            V2 = Me.m_rand.NextDouble
        Loop Until V1 > 0
        val = CSng(Math.Sqrt(-2 * Math.Log(V1)) * Math.Cos(2 * Math.PI * V2))

        'OK This should have worked but didn't
        'It generated a difference sequence every time it was reset for a new run
        'I must have done something wrong but can't figure out what...
        'val = CSng(Me.m_NormalDist.NextDouble())

        'System.Console.Write(val.ToString & ",")
        Return val
    End Function

    Private Function getNextRandNormal(ByVal Save2File As Boolean, ByVal iFleet As Integer) As Single

        Dim val As Single
        Dim V1 As Double, V2 As Double

        If Save2File = False Then Return Nothing

        'Me.m_rand() gets re-seeded for every run in InitRandom()  ### Since making changes not sure this is true now!!!!! ####

        'Box Muller transformation 
        'http://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform
        'Mean = 0 SD = 1
        Do
            V1 = Me.m_rand.NextDouble
            V2 = Me.m_rand.NextDouble
        Loop Until V1 > 0
        val = CSng(Math.Sqrt(-2 * Math.Log(V1)) * Math.Cos(2 * Math.PI * V2))

#If DEBUG Then
        Dim test_file_exists As Boolean = True
        If Not File.Exists(cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.Results, "test_errors.csv")) Then
            test_file_exists = False
        End If
        Dim fn As String = cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.Results, "test_errors.csv")
        Dim strmImpErrors As StreamWriter
        strmImpErrors = cMSEUtils.GetWriter(fn, True)
        'Headers
        If Not test_file_exists Then
            strmImpErrors.WriteLine("Fleet,ModelID,Strategy,TimeStep, Error")
        End If
        strmImpErrors.Write(Me.m_pathdata.FleetName(iFleet))
        strmImpErrors.Write("," & Me.m_MSE.CurrentModelID)
        strmImpErrors.Write("," & Me.m_MSE.currentStrategy.Name)
        strmImpErrors.Write("," & Me.m_MSE.m_currentTimeStep)
        strmImpErrors.Write("," & val)
        strmImpErrors.WriteLine()
        strmImpErrors.Close()
        strmImpErrors.Dispose()
#End If

        'OK This should have worked but didn't
        'It generated a difference sequence every time it was reset for a new run
        'I must have done something wrong but can't figure out what...
        'val = CSng(Me.m_NormalDist.NextDouble())

        'System.Console.Write(val.ToString & ",")
        Return val

    End Function


    Private Sub dumpBioEstOverB(Strategy As Strategy, iTimestep As Integer, BioEst() As Single, B() As Single)
        Try
            Me.m_strmBobsB.Write(m_iTrial.ToString & "," & Strategy.Name & "," & iTimestep.ToString)
            For i As Integer = 1 To Me.Core.nLivingGroups
                Me.m_strmBobsB.Write("," & (BioEst(i) / B(i)).ToString)
            Next
            Me.m_strmBobsB.WriteLine()
            Me.m_strmBobsB.Flush()
        Catch ex As Exception

        End Try

    End Sub

    Private Function getAvgB(iModelTimeStep As Integer) As Single()
        Dim ngrps As Integer = Me.Core.nLivingGroups
        Dim simdata As cEcosimDatastructures = Me.MSE.EcosimData
        Dim avgB() As Single = New Single(ngrps) {}

        Debug.Assert(iModelTimeStep > 12, Me.ToString + ".getAvgB() Can only be called after the end of the first year!")
        Dim StartT As Integer = iModelTimeStep - 12

        'Sum the biomass from the previous year
        For it As Integer = StartT To StartT + 11
            For igrp As Integer = 1 To ngrps
                avgB(igrp) += Me.m_MSE.EcosimData.ResultsOverTime(0, igrp, it)
            Next igrp
        Next it

        'Get the average
        For igrp As Integer = 1 To ngrps
            avgB(igrp) /= 12
        Next igrp

        Return avgB

    End Function

    Private Function stockRecruitment(ByVal iTime As Integer, ByVal iGroup As Integer, ByVal BioEst As Single, ByVal Blast As Single) As Single
        'B is the biomass calculated by Ecosim
        'BioEst is the observed biomass(Ecosim biomass + random variation)
        'Blast is the biomass predicted for the last timestep ( Blast = stockRecruitment(t-1) )

        Dim RstockPred As Single
        Dim vPred As Single
        Dim Best As Single

        Dim simdata As cEcosimDatastructures = Me.MSE.EcosimData

        Dim CatchYear() As Single = Me.getCatch(iTime)
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ' What this correction basically does is to increase the year-to-year Biomass gain factor in the delaydifference model (effective GstockPred by year)
        ' for situations where F has been reduced relative to ecopath base, and reduce the factor for years when F is higher than ecopath base.  
        'In the original code, we were just doing a factor reduction based on current F (catchyeargroup/Blast), without correcting relative to the ecopath base value of GstockPred.
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        Me.BestimateLast(iGroup) = Blast * CSng(Math.Exp(-CatchYear(iGroup) / Blast + simdata.Fish1(iGroup)))

        RstockPred = CSng(Rmax(iGroup) * Me.BestimateLast(iGroup) / (BhalfT(iGroup) + Me.BestimateLast(iGroup)))
        vPred = CSng((RstockRatio(iGroup) * cvRec(iGroup)) ^ 2 / (1 - GstockPred(iGroup) ^ 2))
        KalmanGain(iGroup) = CSng(vPred / (vPred + CVbiomEst(iGroup) ^ 2))

        Best = KalmanGain(iGroup) * BioEst + (1 - KalmanGain(iGroup)) * (GstockPred(iGroup) * Me.BestimateLast(iGroup) + RstockPred)

        'If BioEst is tiny Best can be an invalid number
        If Best = 0 Or Single.IsNaN(Best) Then
            Best = 1.0E-20
        End If

        Return Best

    End Function

    Private Function getCatch(itime As Integer) As Single()
        Dim ngrps As Integer = Me.Core.nLivingGroups
        Dim simdata As cEcosimDatastructures = Me.MSE.EcosimData
        Dim avgCatch() As Single = New Single(ngrps) {}

        Debug.Assert(itime > 12, Me.ToString + ".getCatch() Can only be called after the end of the first year!")
        Dim StartT As Integer = itime - 12

        'Sum the biomass from the previous year
        For it As Integer = StartT To StartT + 11
            For igrp As Integer = 1 To ngrps
                avgCatch(igrp) += Me.m_MSE.EcosimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, igrp, it)
            Next igrp
        Next it

        'Get the average
        For igrp As Integer = 1 To ngrps
            avgCatch(igrp) /= 12
        Next igrp

        Return avgCatch

    End Function


    Friend ReadOnly Property Core As cCore
        Get
            Return Me.m_core
        End Get
    End Property


    Private ReadOnly Property MSE As cMSE
        Get
            Return Me.m_MSE
        End Get
    End Property

    Private Function DefaultFileName() As String
        Return cMSEUtils.MSEFile(m_MSE.DataPath, cMSEUtils.eMSEPaths.StockAssessment, "StockAssessment.csv")
    End Function

  
    Private Sub InitOutputFiles()
        Try

            If m_strmBobsB IsNot Nothing Then
                cMSEUtils.ReleaseWriter(m_strmBobsB)
            End If

            Dim fn As String = cMSEUtils.MSEFile(Me.MSE.DataPath, cMSEUtils.eMSEPaths.StockAssessment, "BobsOverB.csv")
            m_strmBobsB = cMSEUtils.GetWriter(fn, False)
            'Headers
            m_strmBobsB.WriteLine("B_StockAssessment / B_Ecosim")
            m_strmBobsB.Write("TrialNumber,Strategy,TimeStep")
            For igrp As Integer = 1 To Me.Core.nGroups
                m_strmBobsB.Write("," & Me.m_pathdata.GroupName(igrp))
            Next
            m_strmBobsB.WriteLine()

        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "IMSEData Implementation"

    Public Sub Defaults() Implements IMSEData.Defaults
        'Me.InitToCoreData()
        Try
            'On by default
            Me.UseAssessment = True

            For igrp As Integer = 1 To Me.Core.nLivingGroups
                Me.RstockRatio(igrp) = CSng(1 - Math.Exp(-Me.m_pathdata.PB(igrp)))
                Me.RHalfB0Ratio(igrp) = 0.2F

                'Stock Sampling error
                Me.CVbiomEst(igrp) = 0.3F

                'Kalman filter weights/error
                Me.CVRecruitError(igrp) = 0.3F
                Me.cvRec(igrp) = 0.8F
            Next

            'Fleet implementation error
            For iflt As Integer = 1 To Me.Core.nFleets
                Me.CVImpError(iflt) = 0.3F
            Next

            Me.InitStockAssessment()

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Public Function IsChanged() As Boolean Implements IMSEData.IsChanged
        Return Me.m_isChanged
    End Function

    Public Function Load(Optional msg As cMessage = Nothing, Optional strFilename As String = "") As Boolean Implements IMSEData.Load
        'Dim buff As String
        'Dim igrp As Integer
        Dim breturn As Boolean = True

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If

        Dim reader As StreamReader = cMSEUtils.GetReader(strFilename)

        Try
            ' JS 18Sep14: try not to rely on exceptions; it's much more slow than using a simple test
            If (reader IsNot Nothing) Then
                breturn = Me.ReadGroupData(reader) And _
                          Me.ReadFleetData(reader) And _
                          Me.ReadModelData(reader)
            Else
                breturn = False
            End If

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString + ".Read() Exception: " + ex.Message)
            'I don't think I've used this correctly, this message is not posted to the core.
            'Anyway the user will be informed below just not that it was an error
            cMSEUtils.LogError(New cMessage("Stock Assessment could not load from file " & strFilename & ". " & ex.Message, EwEUtils.Core.eMessageType.ErrorEncountered, EwEUtils.Core.eCoreComponentType.Plugin, EwEUtils.Core.eMessageImportance.Warning), ex.Message)
            breturn = False
        End Try
        cMSEUtils.ReleaseReader(reader)

        If breturn Then
            'Read the file 
            Me.m_isChanged = False
        Else
            'Failed to read the file 
            'set some defaults and tell the user
            Me.Defaults()
            Me.MSE.InformUser("CEFAS MSE Stock Assessment model failed to load parameters from file. Defaults will be used.", EwEUtils.Core.eMessageImportance.Information)
        End If

        Me.InitStockAssessment()

        Return breturn
    End Function


    Private Function ReadGroupData(strm As StreamReader) As Boolean
        Dim buff As String
        Dim breturn As Boolean = True

        Try
            'read the header line
            strm.ReadLine()
            For igrp = 1 To Me.Core.nLivingGroups
                'igroup indexing assumes the file was written in order
                'which it was
                buff = strm.ReadLine()
                'Let the parameter object figure out the format of the data
                Me.Parameter(igrp).FromCSVString(buff)
            Next

        Catch ex As Exception
            breturn = False
        End Try

        Return breturn

    End Function

    Private Function ReadFleetData(strm As StreamReader) As Boolean
        Dim buff As String
        Dim breturn As Boolean = True

        If strm.EndOfStream Then
            'Fleet data was not even in the file
            'This can happen with older files
            'Return False and use the defaults
            'next time the file is saved the data will be there
            Return False
        End If

        Try
            'Header
            buff = strm.ReadLine()
            'Fleet data
            For iflt As Integer = 1 To Core.nFleets
                buff = strm.ReadLine()
                'Let the parameter object figure out the format of the data
                Me.FleetParameter(iflt).FromCSVString(buff)
            Next
        Catch ex As Exception
            breturn = False
        End Try

        Return breturn

    End Function

    Private Function ReadModelData(strm As StreamReader) As Boolean
        Dim buff As String
        Dim recs() As String
        Dim breturn As Boolean = True

        If strm.EndOfStream Then
            'Model data was not even in the file
            'This can happen with older files
            'Return False and use the defaults
            'next time the file is saved the data will be there
            Return False
        End If

        Try
            'Header data
            buff = strm.ReadLine()
            'Debug.Assert(buff.Contains(MODEL_HEADER), "Opps could be a problem reading Model data from StockAssessment file.")
            'Data
            buff = strm.ReadLine()
            recs = cStringUtils.SplitQualified(buff, ",")
            Me.UseAssessment = Boolean.Parse(recs(1))

        Catch ex As Exception
            breturn = False
        End Try

        Return breturn

    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean Implements IMSEData.Save

        Dim strm As StreamWriter = Nothing
        Dim breturn As Boolean

        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If

        strm = cMSEUtils.GetWriter(strFilename, False)

        Try


            If (strm IsNot Nothing) Then

                strm.WriteLine(cStockAssessmentParameters.toCSVHeader)
                For iGrp As Integer = 1 To Me.m_core.nLivingGroups
                    strm.WriteLine(Me.Parameter(iGrp).toCSVString)
                Next

                strm.WriteLine(cStockAssessmentFleetParameters.toCSVHeader)
                For iflt As Integer = 1 To Me.m_core.nFleets
                    strm.WriteLine(Me.FleetParameter(iflt).toCSVString)
                Next

                strm.WriteLine(MODEL_HEADER)
                strm.WriteLine("UseStockAssessmentModel," + Me.UseAssessment.ToString)

                breturn = True
            End If

        Catch ex As Exception
            breturn = False
        End Try

        cMSEUtils.ReleaseWriter(strm)

        If breturn Then
            Me.m_isChanged = False
        End If

        cMSEUtils.ReleaseWriter(strm)

        Return breturn
    End Function

    Public Function FileExists(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.FileExists
        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If
        Return File.Exists(strFilename)
    End Function

#End Region


End Class
