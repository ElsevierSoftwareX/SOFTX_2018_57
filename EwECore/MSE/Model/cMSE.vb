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
Imports EwECore
Imports EwECore.Ecosim
Imports EwECore.ExternalData
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports EwEPlugin
Imports EwEPlugin.Data
Imports System.Text

Imports EwEUtils.SystemUtilities.cSystemUtils

Namespace MSE

    'ToDo MSE 6-Nov-2012 LPSolver need to decide what to do if the model in Non Optimal INFEASIBLE. Right now it writes to a temp file and uses the effort from the last time step, ignoring the Non Optimal results.
    'ToDo MSE 6-Nov-2012 LPSolver F Timeseries unloading can not be loaded when running LP solution see InitForTrial
    'ToDo MSE 6-Nov-2012 Debug F Timeseries unloading in  InitForTrial

#Region "Public definitions"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cMSYProgressArgs
        Public Iteration As Integer
        Public FleetIndex As Integer
        Public CurrentEffort As Single
        Public Sub New(ByVal curIteration As Integer, ByVal iFleet As Integer, ByVal Effort As Single)
            Iteration = curIteration
            FleetIndex = iFleet
            CurrentEffort = Effort
        End Sub
    End Class

    ''' <summary>
    ''' Run states of the MSE 
    ''' </summary>
    ''' <remarks>Passed out via the MSEProgressDelegate</remarks>
    Public Enum eMSERunStates
        Started
        RunCompleted
        IterationCompleted
        IterationStarted
    End Enum

    Public Delegate Sub MSEProgressDelegate(ByVal RunStateType As eMSERunStates)
    Public Delegate Sub MSYProgressDelegate(ByVal MSYProgress As cMSYProgressArgs)

#End Region

#Region "MSE Class"

    ''' <summary>
    ''' Management Strategy Evaluation
    ''' </summary>
    ''' <remarks>This was the Closed Loop Simulation in EwE5</remarks>
    Public Class cMSE

        'ToDo_jb 30-Dec-09 MSE note Number of years the MSE runs Ecosim for. At one point we had it running for an extra number of years(cSearchDataStructures.ExtraYearsForSearch) 
        'like the Fishing Policy search does.
        'I have removed this from cSearchDataStructures.InitSearch(). Now Ecosim is only run for the number of years it is set for in the interface. 
        'If we want to run it for some invisible number of years we need to sort out 
        ' ValueChain does it need to know about the extra years? Now it uses nEcosimYears which does not include the extra years.
        ' Histogram How does it bin the data from the extra years that is not displayed. If the data is not display the histogram can look wrong.
        ' Fishing effort figure out what happens to the effort for the extra years in the different modes the MSE runs in.

        'ToDo_jb 18-Jan-2010 cMSE output files need to have header and maybe more outputs

        'ToDo_jb 6-May-2010 cMSE Stats are gathered correctly when running with a start year > 1. 
        'ToDo_jb 6-May-2010 cMSE Histogram can deal with zero values
        'ToDo_jb 6-May-2010 cMSE Plugin data is used for value when selected

        'ToDo_jb 13-May-2010 cMSE Plugin points need xml comments

        'ToDo_jb 28-May-2010 check that changes to run order of plugin and interface calls did not messup the results

        'ToDo_jb 1-Sept-2010 Email from Villy 
        'Joe,
        'It would be useful to have the option in the MSE to run it up to a certain point with whatever was in the Ecosim run, 
        'and then only apply changes from the ‘start year’ (which already is in the MSE). 
        'So, it should use forced catches, F, and whatever is there. Currently it will uncheck the forced catches but that should be from the start year only
        '
        'This cannot be accomplished by simply loading and unloaded the timeseries data in the middle of the run. I don't think...
        'We will need to buffer any variables that could change between the two states(loaded and unloaded) then swap them during the run.
        'We will also need to make sure that there is no initialization that needs to happen for the loaded and unloaded timeseries data
        'FishRateNo(), FishForced(), PoolForceZ()?? See cTimeSeriesDataStructures.DoDatValCalculations(). I think Effort is already dealt with.


        'ToDo_jb 29-Sept-2010 why is there no variation when running Fixed F policy
        'ToDo_jb 29-Sept-2010 fix StartT it needs to be 1 when Start Year = 1 

        'ToDo_jb 18-Oct-2010 F timeseries can sometimes end up with the last year (12 months) at some value other then in the timeseries
        'Only noticable in Tracking (Ecosim Effort) this seems to happen when changing timeseries and run types 

        'ToDo_jb 27-Oct-2010 Moved PropdiscardTime and PropLandedTime() to Ecosim make sure this works.

        'Filenames prefixes for output file
        Public Const BIOMASS_DATA As String = "MSE_Biomass"
        Public Const CATCH_DATA As String = "MSE_CatchByGroup"
        Public Const EFFORT_DATA As String = "MSE_Effort"
        Public Const FLEETCATCH_DATA As String = "MSE_CatchByFleet"
        Public Const QUOTAGROUP_DATA As String = "MSE_QuotaByGroup"


        Public Enum eResultsData
            GroupQuota
            FleetQuota
        End Enum


#Region "Private data"

        ' Private Const DEFAULT_EFFORT As Single = 1000000000

        Private m_core As cCore
        Private m_data As cMSEDataStructures
        Private m_Ecosim As Ecosim.cEcoSimModel
        Private m_Search As cSearchDatastructures
        Private m_esData As cEcosimDatastructures
        Private m_epdata As cEcopathDataStructures
        Private m_refData As cTimeSeriesDataStructures

        Private m_batchManager As MSEBatchManager.cMSEBatchManager

        Private m_output As IMSEOutputWriter

        Dim m_rndGen As Random

        Private m_nTrials As Integer

        Private m_ProgressDelegate As MSEProgressDelegate

        Private BestTime() As Single
        Private EcoValueBase As Single, ManValueBase As Single
        Private TotValBase As Single, EmployBase As Single

        Private m_pluginManager As cPluginManager
        Private m_orgPredictEffort As Boolean
        Private m_orgUsePlugin As Boolean = False

        Private m_EconomicData As New cEconomicDataSource

        Private m_MSYCallBack As MSYProgressDelegate
        Private m_baseEffort(,) As Single 'base value relative effort FishRateGear()
        Private m_baseFishForced() As Boolean

        Private m_DataDir As String


        ''' <summary>Dictionary of arrays that are use to store results that are gathered by the MSE.</summary>
        ''' <remarks>Use to store results that are not computed by Ecosim.</remarks>
        Private m_lstData As Dictionary(Of eResultsData, Single(,))

        ''' <summary>Current time index(cumulative month) computed by Ecosim</summary>
        ''' <remarks>set when Ecosim calls SetTime()</remarks>
        Private m_curT As Integer

        ''' <summary>Current year index computed by Ecosim</summary>
        ''' <remarks>set when Ecosim calls SetTime()</remarks>
        Private m_curYear As Integer

        Private m_StartT As Integer
        Private m_EndT As Integer

        'Private m_LPSolver As SimplexSolver
        Private m_LPSolver As cLPSolver

        Private m_FleetCode() As Integer, m_GroupCode() As Integer, m_GoalRowID As Integer
        Private m_QStar(,) As Single

#End Region

#Region "Public Properties"

        Public ReadOnly Property Data() As cMSEDataStructures
            Get
                Return Me.m_data
            End Get
        End Property

        Public Property BatchManager() As MSEBatchManager.cMSEBatchManager
            Get
                Return Me.m_batchManager
            End Get
            Set(ByVal value As MSEBatchManager.cMSEBatchManager)
                Me.m_batchManager = value
            End Set
        End Property

#End Region

#Region "Modeling code"

#Region "Private Properties"

        Private ReadOnly Property UsePlugin() As Boolean
            Get
                If (Me.m_EconomicData IsNot Nothing) Then
                    Return (Me.m_Search.MSEUseEconomicPlugin = True) And _
                           (Me.m_EconomicData.EnableData(New cEcosimRunType) = True)
                End If
                Return False
            End Get
        End Property


        Private ReadOnly Property StartT() As Integer
            Get
                Return Me.m_StartT
            End Get
        End Property

        Private ReadOnly Property EndT() As Integer
            Get
                Return Me.m_EndT
            End Get
        End Property

#End Region

#Region "Initialization and Connection"

        Public Sub New(ByVal theCore As cCore)
            Me.m_core = theCore
        End Sub

        Public Sub Init(ByVal MSEData As cMSEDataStructures, ByVal Ecosim As Ecosim.cEcoSimModel, ByVal SearchData As cSearchDatastructures, ByVal EcopathData As cEcopathDataStructures, ByVal RefData As cTimeSeriesDataStructures, ByVal PluginManager As cPluginManager)

            Me.m_data = MSEData
            Me.m_Ecosim = Ecosim
            Me.m_Search = SearchData
            Me.m_esData = m_Ecosim.m_Data
            Me.m_epdata = EcopathData
            Me.m_pluginManager = PluginManager
            Me.m_refData = RefData

            Me.m_EconomicData = cEconomicDataSource.getInstance()
            Me.m_data.InitForRun()

            'VC added a boolean to ex/include fleets from MSY runs
            ReDim Data.MSYEvaluateFleet(m_epdata.NumFleet)
            For i As Integer = 1 To m_epdata.NumFleet
                Data.MSYEvaluateFleet(i) = True 'that's the default value
            Next
            ReDim Data.MSYEvaluateGroup(m_epdata.NumGroups)
            For i As Integer = 1 To m_epdata.NumGroups
                Data.MSYEvaluateGroup(i) = True
            Next

        End Sub

        Public Sub Connect(ByRef MSECallBack As MSEProgressDelegate, ByRef MSYCallBack As MSYProgressDelegate)
            Try
                Me.m_ProgressDelegate = MSECallBack
                Me.m_MSYCallBack = MSYCallBack
            Catch ex As Exception
                cLog.Write(ex)
            End Try
        End Sub

        Public Sub Disconnect(ByRef MSECallBack As MSEProgressDelegate, ByRef MSYCallBack As MSYProgressDelegate)
            Try
                Me.m_ProgressDelegate = Nothing
                Me.m_MSYCallBack = Nothing
            Catch ex As Exception
                cLog.Write(ex)
            End Try
        End Sub

        Public Sub Clear()
            Try
                Me.m_baseEffort = Nothing '(,) As Single '
                If Me.m_lstData IsNot Nothing Then Me.m_lstData.Clear()
                Me.m_lstData = Nothing

                Me.m_EconomicData = Nothing

            Catch ex As Exception
                cLog.Write(ex)
            End Try
        End Sub


        Friend Sub InitAssessment()
            Dim iGrp As Integer
            Try

                For iGrp = 1 To Me.m_esData.nGroups
                    m_data.Bestimate(iGrp) = Me.m_esData.StartBiomass(iGrp) * CSng(Math.Exp(Me.m_data.CVbiomEst(iGrp) * Me.RandomNormal()))
                    m_data.BestimateLast(iGrp) = m_data.Bestimate(iGrp)
                Next iGrp

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".InitAssessment() Exception: " & ex.Message)
                cLog.Write(ex)
            End Try

        End Sub


        Public Sub InitForRun()

            Try
                Dim igrp As Integer
                'output
                Me.m_output = Me.OutputWriterFactory
                Me.m_output.Init()

                ReDim BestTime(m_epdata.NumGroups)

                Dim rndSeed As Integer
                'is this a batch run
                If Me.m_data.bInBatch Then
                    'Yes batch run
                    'if in batch run use the same random number sequence for each run
                    rndSeed = 42
                Else
                    'No normal run
                    'create a new random seed for each run
                    rndSeed = CInt(CInt(Date.Now.Ticks Mod Integer.MaxValue))
                    'make sure Fmin(igroup) and EndYear have not been set somehow....
                    For igrp = 1 To Me.m_data.NGroups
                        Me.m_data.Fmin(igrp) = 0
                    Next
                    Me.m_data.EndYear = cCore.NULL_VALUE
                End If

                'create a new random number generator for each run
                'the seed will decide if the sequence is unique or not
                Me.m_rndGen = New Random(rndSeed)

                Dim ds As cEconomicDataSource = cEconomicDataSource.getInstance()
                If (ds IsNot Nothing) Then
                    Me.m_orgUsePlugin = ds.EnableData(New cEcosimRunType)
                    ds.EnableData(New cEcosimRunType) = Me.UsePlugin
                End If

                Me.m_data.StopRun = False

                Me.m_data.clearBioRisk()

                For igrp = 1 To m_epdata.NumFleet
                    'save qgrowth parameter so as not to interfere with value fitting simulations
                    Me.m_data.QGrowUsed(igrp) = m_data.Qgrow(igrp)
                Next

                ' B(t+1)=g(t)B(t)+Rt
                ''growth
                'g(t)=exp(-Zt)(alpha/wbar+rho)=exp(-Zt) * growth constant
                'g(0)=1-R(0)/B(0) where user inputs R/B
                'g(0)=exp(-Zo) * growth constant so growth constant = g(0)exp(Zo)
                'g(t)=exp(-Zt) * growth constant =exp(-Zt)g(0)exp(Zo)
                'g(t) = exp(-Ft + Fo) * g(0)
                'g(t)=exp(-Zt+Zo) * g(0)

                Dim BaB As Single
                'init RstockPred from GstockPred
                'GstockPred could have been altered by an interface
                For igrp = 1 To Me.m_epdata.NumLiving
                    'BaB is correct for Stanza groups because Ecopath.BA() gets updated with Stanza.BaBsplit()
                    BaB = Me.m_core.m_EcoPathData.BA(igrp) / Me.m_core.m_EcoPathData.B(igrp)
                    'gstockpred=exp(bab)-rstockratio, rather than 1-rstockratio.  Check to insure gstockpred>0

                    'Me.m_data.GstockPred(igrp) = 1 - Me.m_data.RstockRatio(igrp)
                    Me.m_data.GstockPred(igrp) = CSng(Math.Exp(BaB) - Me.m_data.RstockRatio(igrp))
                    If Me.m_data.GstockPred(igrp) < 0 Then Me.m_data.GstockPred(igrp) = 0
                    Me.m_data.BhalfT(igrp) = Me.m_data.RHalfB0Ratio(igrp) * Me.m_epdata.B(igrp)

                    Me.m_data.RStock0(igrp) = Me.m_data.RstockRatio(igrp) * Me.m_esData.StartBiomass(igrp)
                    Me.m_data.Rmax(igrp) = Me.m_data.RStock0(igrp) * (Me.m_data.RHalfB0Ratio(igrp) + 1)

                Next

                Me.m_Ecosim.TimeStepDelegate = AddressOf Me.onEcosimTimestep

                'initialize Ecosim
                m_Ecosim.Init(False)

                Me.InitResults()

                Me.InitLPSolver()

                'sets the start and end timesteps using StartYear and EndYear
                Me.setStartTEndT()

                Me.setEffortForRun()

                'jb 10-sept-2010 HACK fix 
                'some databases can contain -9999 for these values 
                'this messes up the quota calculation so set them to zero
                For igrp = 1 To Me.m_data.NGroups
                    If Me.m_data.Fopt(igrp) < 0 Then Me.m_data.Fopt(igrp) = 0
                    If Me.m_data.Blim(igrp) < 0 Then Me.m_data.Blim(igrp) = 0
                    If Me.m_data.Bbase(igrp) < 0 Then Me.m_data.Bbase(igrp) = 0
                Next

                For igrp = 1 To Me.m_data.NGroups
                    For iFlt As Integer = 1 To Me.m_data.nFleets
                        If Me.m_esData.relQ(iFlt, igrp) > 0 Then
                            Me.m_data.Fweight(iFlt, igrp) = 1
                        End If
                    Next
                Next


            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".InitForRun() Error:" & ex.Message, ex)
            End Try

        End Sub

        Public Sub FinalizeRun()

            Try

                'set the ecosim predict effort flag back to its original value
                Me.m_esData.PredictSimEffort = Me.m_orgPredictEffort

                Dim ds As cEconomicDataSource = cEconomicDataSource.getInstance()
                If (ds IsNot Nothing) Then
                    ds.EnableData(New cEcosimRunType) = Me.m_orgUsePlugin
                End If

                If Me.m_core.Autosave(eAutosaveTypes.MSE) Then
                    If Me.m_lstData IsNot Nothing Then
                        Me.m_lstData.Clear()
                        Me.m_lstData = Nothing
                    End If
                End If

                Me.resetEffort(True)

            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".FinalizeRun() Exception: " & ex.Message)
            End Try

        End Sub



        Private Sub InitResults()
            Try
                If Not Me.m_core.Autosave(eAutosaveTypes.MSE) Then Return

                If Me.m_lstData IsNot Nothing Then
                    Me.m_lstData.Clear()
                    Me.m_lstData = Nothing
                End If

                Me.m_lstData = New Dictionary(Of eResultsData, Single(,))
                Dim d(,) As Single
                ReDim d(m_epdata.NumGroups, Me.m_esData.NTimes)
                Me.m_lstData.Add(eResultsData.GroupQuota, d)

                'redim will create a new array
                ReDim d(m_epdata.NumFleet, Me.m_esData.NTimes)
                Me.m_lstData.Add(eResultsData.FleetQuota, d)

            Catch ex As Exception
                System.Console.WriteLine(ex.Message)
            End Try

        End Sub


        Private Sub setBestTotalValue()

            Try
                'Run Ecosim
                Me.m_Ecosim.Run()

                'get the base values from the search data
                Me.m_data.BaseTotalVal = Me.m_Search.totval
                Me.m_data.BaseEmployVal = Me.m_Search.Employ
                Me.m_data.BaseManValue = Me.m_Search.manvalue
                Me.m_data.BaseEcoVal = Me.m_Search.ecovalue

                'cal base BestTotalValue (TotValBase,EmployBase... were set in SetBaseValues()
                Me.m_data.BestTotalValue = Me.m_Search.ValWeight(eSearchCriteriaResultTypes.TotalValue) * Me.m_Search.totval / TotValBase + _
                                 Me.m_Search.ValWeight(eSearchCriteriaResultTypes.Employment) * Me.m_Search.Employ / EmployBase + _
                                 Me.m_Search.ValWeight(eSearchCriteriaResultTypes.MandateReb) * Me.m_Search.manvalue / ManValueBase + _
                                 Me.m_Search.ValWeight(eSearchCriteriaResultTypes.Ecological) * Me.m_Search.ecovalue / EcoValueBase

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException("MSE.setBestTotalValue() Error: " & ex.Message, ex)
            End Try


        End Sub


        Private Sub SetBaseValues()
            Dim i As Integer, j As Integer, Cval As Single

            'RunModelValue TotalTime, TotValBase, EmployBase, EcoValueBase, lnF(), N, False
            EcoValueBase = 0
            ManValueBase = 0
            TotValBase = 0
            EmployBase = 0

            For i = 1 To m_epdata.NumLiving
                EcoValueBase = EcoValueBase + Me.m_Search.BGoalValue(i)
                ManValueBase = ManValueBase + Me.m_Search.MGoalValue(i)
            Next

            EcoValueBase = EcoValueBase * Me.m_esData.NumYears
            ManValueBase = ManValueBase * Me.m_esData.NumYears
            If ManValueBase = 0 Then ManValueBase = 1 'to avoid division with zero

            For i = 1 To m_epdata.NumLiving
                For j = 1 To m_epdata.NumFleet
                    Cval = m_esData.StartBiomass(i) * Me.m_esData.relQ(j, i) * m_epdata.Market(j, i)
                    TotValBase = TotValBase + Cval  '.5 here assumes cost likely 80% of income
                    EmployBase = EmployBase + Cval * Me.m_Search.Jobs(j)
                Next
            Next

            'read the size of the array instead of using Ecosim.NTimes because it can be different if a timeseries has been loaded!!!
            Dim n As Integer = Me.m_esData.FishRateGear.GetUpperBound(1)
            ReDim Me.m_baseEffort(Me.m_esData.nGear, n)
            For iflt As Integer = 1 To m_core.nFleets
                For it As Integer = 1 To n
                    m_baseEffort(iflt, it) = Me.m_esData.FishRateGear(iflt, it)
                Next
            Next

            n = Me.m_esData.FisForced.Length
            ReDim Me.m_baseFishForced(n - 1)
            Array.Copy(Me.m_esData.FisForced, Me.m_baseFishForced, n)

            If Me.m_Search.DiscountFactor > 0 Then
                TotValBase = Math.Abs(TotValBase) / Me.m_Search.DiscountFactor
                EmployBase = Math.Abs(EmployBase) / Me.m_Search.DiscountFactor
            End If

            ManValueBase = Math.Abs(ManValueBase)
            EcoValueBase = Math.Abs(EcoValueBase)

            If TotValBase < 1.0E-20 Then TotValBase = 1.0E-20
            If EmployBase < 1.0E-20 Then EmployBase = 1.0E-20
            If ManValueBase < 1.0E-20 Then ManValueBase = 1.0E-20
            If EcoValueBase < 1.0E-20 Then EcoValueBase = 1.0E-20

        End Sub

        ''' <summary>
        ''' Set the Effort to a max value if running in default mode TrackUseQuota
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        Private Sub setEffortForRun()

            Try
                'Only set Effort high if using controls and EffortSource is NoCap
                If Me.m_data.RegulationMode = eMSERegulationMode.UseRegulations And Me.m_data.EffortSource = eMSEEffortSource.NoCap Then

                    For iflt As Integer = 1 To Me.m_data.nFleets
                        'Only if this fleet is regulated
                        If Me.m_data.QuotaType(iflt) <> eQuotaTypes.NoControls Then
                            For it As Integer = Me.StartT To Me.EndT
                                Me.m_esData.FishRateGear(iflt, it) = Me.m_data.MSEMaxEffort
                            Next it
                        End If 'Me.m_data.QuotaType(iflt) <> eQuotaTypes.NotUsed
                    Next iflt

                ElseIf Me.m_data.RegulationMode = eMSERegulationMode.NoRegulations Then

                    Me.m_Ecosim.SetBaseFFromGear()

                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub


        ''' <summary>
        ''' Set Effort back to its original value after a run
        ''' </summary>
        ''' <remarks>Always set effort base to its original value!</remarks>
        Private Sub setEffortToBase()

            Try

                Dim n As Integer = Me.m_esData.FishRateGear.GetUpperBound(1)
                'check the bounds
                Debug.Assert(Me.m_baseEffort.GetUpperBound(1) >= n, Me.ToString & ".setEffortToOriginal() Effort array out of bounds!")

                For iflt As Integer = 1 To m_core.nFleets
                    For it As Integer = 1 To n
                        Me.m_esData.FishRateGear(iflt, it) = Me.m_baseEffort(iflt, it)
                    Next
                Next

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.Message)
            End Try

        End Sub


        Private Function OutputWriterFactory() As IMSEOutputWriter
            Dim output As IMSEOutputWriter

            If Me.m_data.bInBatch Then
                output = Me.m_batchManager.OutputWriter
            Else
                output = New cMSECSVOutputWriter(Me.m_core, Me.m_data)
            End If

            Debug.Assert(output IsNot Nothing, Me.ToString & ".OutputFactory() Failed to create CSV output object.")
            Return output

        End Function

#End Region

#Region "Running and computational code"

        Public Function Run() As Boolean
            Dim itr As Integer
            Dim bSuccess As Boolean = True

            Try
                cLog.Write("MSE run started.")
                PostMessage(eMSERunStates.Started)

                'keep the original value of PredictEffort so we can set it back at the end of the run
                m_orgPredictEffort = Me.m_esData.PredictSimEffort

                'turn off regulatory models for initialization
                Me.m_esData.PredictSimEffort = False

                'put the search mode to initialization for setting of base values
                Me.m_Search.SearchMode = eSearchModes.InitializingSearch
                Me.m_esData.bTimestepOutput = True

                'sets MeanEmploy, MeanVal, MeanManVal, MeanEcoVal, MeanTotalValue
                Me.SetBaseValues()

                'init the MSE data
                Me.InitForRun()
                Me.m_data.InitForRun()

                Me.m_Search.initForRun(Me.m_epdata, Me.m_esData)
                Me.m_Search.setMinSearchBlocks() 'set number of search blocks to one and dim FblockCodes()
                If Me.m_Search.BaseYear = 0 Then Me.m_Search.BaseYear = 1
                Me.m_Search.setBaseYearEffort(Me.m_esData)

                'runs Ecosim and gets the base values
                Me.setBestTotalValue()

                'turn the evaluator on for the trials
                'this will vary Effort (Ecosim.Fgear) and Catability (Ecosim.Qyear) via MSE.YearTimeStep() and MSE.AccessFs
                Me.m_Search.SearchMode = eSearchModes.MSE

                'if we are predicting effort then make sure it is turned on in Ecosim
                Me.m_esData.PredictSimEffort = False
                If Me.m_data.RegulationMode = eMSERegulationMode.UseRegulations And Me.m_data.EffortSource = eMSEEffortSource.Predicted Then
                    Me.m_esData.PredictSimEffort = True
                End If

                For itr = 1 To m_data.NTrials

                    Me.InitForTrial()

                    m_data.CurrentIteration = itr
                    Me.AddIteration()

                    Me.PostMessage(eMSERunStates.IterationStarted)

                    'run ecosim
                    bSuccess = Me.m_Ecosim.Run()
                    If Not bSuccess Then Exit For
                    'Threading.Thread.Sleep(2000)

                    Me.summarizeEcosimEconomicData()

                    Me.SaveIteration()
                    'post the search data to plugins
                    Me.PostPluginData()

                    Me.SumValues()
                    Me.resetEffort()
                    ' System.Console.WriteLine("MSE PostMessage IterationCompleted " & itr.ToString)
                    Me.PostMessage(eMSERunStates.IterationCompleted)

                    If Me.m_data.StopRun Then
                        Exit For
                    End If

                Next itr

                Me.ComputeStats()

            Catch ex As Exception
                bSuccess = False
                cLog.Write(ex)
                Debug.Assert(False, "MSE Exception: " & ex.Message)
                ' ToDo: globalize this
                Me.m_core.Messages.SendMessage(New cMessage("Error while calculating MSE. " & ex.Message, eMessageType.ErrorEncountered, eCoreComponentType.MSE, eMessageImportance.Critical))
            End Try

            PostMessage(eMSERunStates.RunCompleted)

            'turn off the search
            Me.m_Search.SearchMode = eSearchModes.NotInSearch

            Return bSuccess

        End Function


        Private Sub InitForTrial()

            Me.InitAssessment()
            'Set MSE data back to initial values for a new run
            m_data.InitForTrial()

            For igrp As Integer = 1 To Me.m_epdata.NumLiving
                Me.m_Search.CatchYearGroup(igrp) = Me.m_epdata.fCatch(igrp)
            Next

            'Ecosim will not set F from Effort if there is F timeseries loaded
            'This will turn OFF the Forced F for groups that are fished by a Controlled Fleet
            For igrp As Integer = 1 To Me.m_data.nLiving
                If Me.m_esData.FisForced(igrp) = True Then
                    'Only if the group has forced F's
                    For iflt As Integer = 1 To Me.m_data.nFleets
                        If Me.m_esData.FishMGear(iflt, igrp) > 0 Then
                            'Only if this fleet catches this group
                            If Me.m_data.QuotaType(iflt) <> eQuotaTypes.NoControls Then
                                'Only if there are quota control on this fleet
                                Me.m_esData.FisForced(igrp) = False
                                'Once F is no longer forced for this group 
                                'there is no point in checking the other fleets that fish this group
                                Exit For
                            End If
                        End If
                    Next
                End If
            Next

        End Sub

        Private Sub ComputeStats()

            Me.m_data.BioStats.ComputeStats()
            Me.m_data.CatchFleetStats.ComputeStats()
            Me.m_data.CatchGroupStats.ComputeStats()
            Me.m_data.EffortStats.ComputeStats()
            Me.m_data.BioEstStats.ComputeStats()

            Me.m_data.FLPDualValue.ComputeStats()

            Me.m_data.ValueFleetStats.ComputeStats()

        End Sub

        ''' <summary>
        ''' Add an iteration to the stats data
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub AddIteration()

            Me.m_data.BioStats.AddIteration()
            Me.m_data.CatchFleetStats.AddIteration()
            Me.m_data.CatchGroupStats.AddIteration()
            Me.m_data.EffortStats.AddIteration()

            Me.m_data.FLPDualValue.AddIteration()
            ' Me.m_data.FActualStats.AddIteration()

            Me.m_data.BioEstStats.AddIteration()

            Me.m_data.ProfitSum.AddIteration()
            Me.m_data.JobsSum.AddIteration()
            Me.m_data.CostSum.AddIteration()

            Me.m_data.ValueFleetStats.AddIteration()

        End Sub

        Private Sub resetEffort(Optional ByVal LastCall As Boolean = False)

            Try
                'Effort is being regulated
                'Always set Effort back to it its base value (Effort from the Sceintific Interface)
                Me.setEffortToBase()

                'Set FisForced() (F timeseries loaded) back to it's base value
                Me.setFishForcedToBase()

                If Not LastCall Then
                    'Not the last call MSE will do another iteration
                    'Set effort for the run type
                    Me.setEffortForRun()

                Else
                    'Last Call the MSE is done
                    'reset F back to its base value based on the effort
                    Me.m_Ecosim.SetBaseFFromGear()

                End If

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".SetEffortToBaseValue() Exception: " & ex.Message)
                cLog.Write(ex)
            End Try

        End Sub


        Private Sub setFishForcedToBase()

            'reloads time series forcing data into core arrays and resets FisForced(groups)
            'F into FishRateNo()
            Me.m_core.m_TSData.DoDatValCalculations()

            'resets F in FishRateNo() based on forced Catches or Effort 
            Me.m_Ecosim.SetBaseFFromGear()

        End Sub

        Private Sub dumpStats()

            For i As Integer = 1 To Me.m_data.nLiving
                Dim histo() As Single = Me.m_data.BioStats.Histogram(i)
                'histogram stuff for debugging
                System.Console.WriteLine()
                For ihist As Integer = 1 To Me.m_data.BioStats.HistoNBins(i)
                    System.Console.Write(histo(ihist).ToString & ", ")
                Next

            Next

            System.Console.WriteLine("----P------")
            For i As Integer = 1 To Me.m_data.nLiving
                Dim Pless As Single = Me.m_data.BioStats.PercentageBelow(i, Me.m_data.BioBounds(i).Lower)
                Dim Pgreater As Single = Me.m_data.BioStats.PercentageAbove(i, Me.m_data.BioBounds(i).Upper)
                ' Debug.Assert(Pless + Pgreater <= 100, "MSE Probability calculation!!!!")
                System.Console.WriteLine("Group = " & Me.m_core.m_EcoPathData.GroupName(i) & ", less = " & Pless.ToString & ", greater = " & Pgreater.ToString)
            Next


            System.Console.WriteLine()

            System.Console.WriteLine("Biomass ranges")
            System.Console.Write(Me.m_data.BioStats.ToString)
            System.Console.WriteLine()

            System.Console.WriteLine("Catch by group ranges")
            System.Console.Write(Me.m_data.CatchGroupStats.ToString)
            System.Console.WriteLine()

            System.Console.WriteLine("Catch by fleet ranges")
            System.Console.Write(Me.m_data.CatchFleetStats.ToString)
            System.Console.WriteLine()

            System.Console.WriteLine("Profit")
            System.Console.Write(Me.m_data.ProfitSum.ToString)
            System.Console.WriteLine()

            System.Console.WriteLine("Cost")
            System.Console.Write(Me.m_data.CostSum.ToString)
            System.Console.WriteLine()

            System.Console.WriteLine("Jobs")
            System.Console.Write(Me.m_data.JobsSum.ToString)
            System.Console.WriteLine()

        End Sub

        Private Sub SaveIteration()

            Try
                Me.m_output.saveIteration(Me.m_lstData)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".SaveIteration() Exception: " & ex.Message)
            End Try

        End Sub

        Private Function BuildCSVFilename(ByVal strDataType As String, ByVal strDataName As String) As String
            ' Build a correct file name with a proper combined path
            Return Path.Combine(Me.m_DataDir, cFileUtils.ToValidFileName(strDataType & strDataName & ".csv", False))
        End Function

        ''' <summary>
        ''' Tell any plugin that a search iteration has completed
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub PostPluginData()
            If Me.m_Search.MSEUseEconomicPlugin And (Me.m_pluginManager IsNot Nothing) Then
                Me.m_pluginManager.PostRunSearchResults(Me.m_Search)
            End If
        End Sub

        Private Sub getMeanValues(ByVal NTrials As Integer)

            Me.m_data.sumEmployVal = Me.m_data.sumEmployVal / NTrials
            Me.m_data.SumTotVal = Me.m_data.SumTotVal / NTrials
            Me.m_data.sumManVal = Me.m_data.sumManVal / NTrials
            Me.m_data.sumEcoVal = Me.m_data.sumEcoVal / NTrials
            Me.m_data.sumWeightedValues = Me.m_data.sumWeightedValues / NTrials

        End Sub


        ''' <summary>
        ''' Sum results of Model run into Mean values
        ''' </summary>
        ''' <remarks>Once the trials have been finished the mean will be calculated from the sums in getMeanValues() (e.g. MeanEmploy) </remarks>
        Private Sub SumValues()

            m_data.sumEmployVal += Me.m_Search.Employ
            m_data.SumTotVal += Me.m_Search.totval
            m_data.sumManVal += Me.m_Search.manvalue
            m_data.sumEcoVal += Me.m_Search.ecovalue

            m_data.sumWeightedValues = m_data.sumWeightedValues + _
                    m_Search.ValWeight(eSearchCriteriaResultTypes.TotalValue) * Me.m_Search.totval / TotValBase + _
                    m_Search.ValWeight(eSearchCriteriaResultTypes.Employment) * Me.m_Search.Employ / EmployBase + _
                    m_Search.ValWeight(eSearchCriteriaResultTypes.MandateReb) * Me.m_Search.manvalue / ManValueBase + _
                    m_Search.ValWeight(eSearchCriteriaResultTypes.Ecological) * Me.m_Search.ecovalue / EcoValueBase

        End Sub



        Private Sub PostMessage(ByVal CurrentState As eMSERunStates)

            Try

                Try
                    Me.m_ProgressDelegate.Invoke(CurrentState)
                Catch ex As Exception
                    System.Console.WriteLine("MSE handled exception from progress delegate = " & ex.Message)
                    cLog.Write(ex)
                End Try

                Select Case CurrentState

                    Case eMSERunStates.IterationStarted
                        Me.m_core.PluginManager.MSEIterationStarted()

                    Case eMSERunStates.IterationCompleted
                        Me.m_core.PluginManager.MSEIterationCompleted()

                    Case eMSERunStates.RunCompleted
                        Me.FinalizeRun()
                        Me.m_core.PluginManager.MSERunCompleted()

                End Select

                'System.Console.WriteLine("MSE State = " & CurrentState.ToString)

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub


        ''' <summary>
        ''' Count the number of times the Biomass is outside the lower or upper risk boundry
        ''' </summary>
        ''' <param name="Biomass"></param>
        ''' <remarks>The biomass risk count can only be one per trial</remarks>
        Friend Sub AccessBioRisk(ByVal Biomass() As Single)

            Try

                For i As Integer = 1 To m_epdata.NumGroups

                    If m_data.BioR0(i) = False And Biomass(i) < m_data.BioRiskValue(i, 0) * Me.m_esData.StartBiomass(i) Then
                        m_data.BioRiskCount(i, 0) = m_data.BioRiskCount(i, 0) + 1
                        m_data.BioR0(i) = True
                    End If

                    If m_data.BioR1(i) = False And Biomass(i) > m_data.BioRiskValue(i, 1) * Me.m_esData.StartBiomass(i) Then
                        m_data.BioRiskCount(i, 1) = m_data.BioRiskCount(i, 1) + 1
                        m_data.BioR1(i) = True
                    End If

                Next

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".AccessBioRisk() Error: " & ex.Message, ex)
            End Try

        End Sub



        ''' <summary>
        ''' Set Fgear() and QYear() for a management strategy evaluation
        ''' </summary>
        ''' <param name="Fgear">Fishing Effort</param>
        ''' <param name="QYear">Catchability growth per year</param>
        ''' <param name="iYear"></param>
        ''' <remarks>Called from Ecosim during a management strategy evaluation</remarks>
        Friend Sub VaryEffortCatchability(ByRef Fgear() As Single, ByRef QYear() As Single, ByVal iYear As Integer)

            Try

                'Increase catchability with the annual growth factor, irrespective of regulation or closed loop type
                For i As Integer = 1 To Me.m_epdata.NumFleet
                    If iYear > 1 Then

                        If Me.isTStepRegulated(Me.m_curT) Then

                            'Regulated Vary QYear()
                            QYear(i) = QYear(i) * (1 + Me.m_data.QGrowUsed(i) * CSng(Me.m_rndGen.NextDouble))

                        Else
                            'Not Regulated 
                            'Catchability should not changed over time
                            QYear(i) = 1

                        End If 'If Me.isTStepRegulated(Me.m_curT) Then
                    End If 'If iYear > 1 Then
                Next i

                If Not Me.m_data.RegulationMode = eMSERegulationMode.NoRegulations Then
                    'Only vary effort here if we are in the Tracking mode(effort is set by the current Ecosim Effort). 
                    Exit Sub
                End If

                'Vary Effort
                For i As Integer = 1 To Me.m_epdata.NumFleet
                    If iYear > 1 Then
                        If m_data.Fwc(i, 1) > 0 Then Fgear(i) = Fgear(i) * m_data.Fwc(i, 0) / m_data.Fwc(i, 1)
                    Else
                        'First year
                        Fgear(i) = CSng(Fgear(i) * (1 + Me.Normal * Math.Sqrt(m_data.VarQest(i))))
                    End If

                    If Fgear(i) < 1.0E-20 Then Fgear(i) = 1.0E-20

                Next i

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".YearTimeStep() Error: " & ex.Message, ex)
            End Try


        End Sub


        Friend Sub AssessFs(ByVal Fgear() As Single, ByVal Bbar() As Single)
            'does assessment at end of simulated year in runmodelvalue if ploton=true,
            'returns fwc(i,1)=updated relative catchability for gear i to be used next year
            'for relative effort setting
            'uses: relative fishing efforts this year (Fgear(i)),end-year biomass bb(j)
            'wftot(i)=total fishing importance weight for gear i (computed at start of simulation)
            'yearly catch catchyear(i,j).  Note Fwc(i,0) has ecopath base weighted fishing impacts
            Dim i As Integer, j As Integer, Fwt As Single, Fest(,) As Single, Best() As Single
            Dim Fpred As Single

            Try

                'are we running the regulatory code
                If Not Me.m_data.RegulationMode = eMSERegulationMode.NoRegulations Then
                    'Yes so don't use this to assess the effort
                    Exit Sub
                End If

                Debug.Assert(Me.m_data.RegulationMode = eMSERegulationMode.NoRegulations, "MSE EffortMode incorrectly set!")

                ReDim Fest(m_epdata.NumFleet, m_epdata.NumLiving), Best(m_epdata.NumLiving)

                'first estimate fishing rates actually achieved by gear and species, Fest(ifleet,igroup) = catch(fleet,group)/biomass(group)
                Select Case m_data.AssessMethod

                    Case eAssessmentMethods.Exact 'biomasses and catch known exactly

                        For i = 1 To m_epdata.NumFleet
                            For j = 1 To m_epdata.NumLiving
                                If Bbar(j) > 0 Then Fest(i, j) = Me.m_Search.CatchYear(i, j) / Bbar(j) Else Fest(i, j) = 0
                            Next
                        Next

                    Case eAssessmentMethods.CatchEstmBio ' Fs from biomass estimates by pool
                        ' System.Console.WriteLine()
                        For j = 1 To m_epdata.NumLiving
                            Best(j) = CSng(Math.Exp(Normal2() * m_data.CVbiomEst(j)) * Me.m_esData.StartBiomass(j) * (Bbar(j) / Me.m_esData.StartBiomass(j)) ^ m_data.AssessPower)

                            If BestTime(j) > 0 Then  'have previous biomass estimate for this run
                                'jb 8-Oct-2010 changed to use the same stock recruitment model as MSE regulatory model
                                BestTime(j) = Me.stockRecruitment(j, Bbar(j), Best(j), BestTime(j))
                                'Bp = m_data.GstockPred(j) * BestTime(j) + m_data.RStock0(j)
                                'BestTime(j) = Bp + m_data.KalmanGain(j) * (Best(j) - Bp)
                            Else
                                BestTime(j) = Best(j)
                            End If

                            Best(j) = BestTime(j)

                            For i = 1 To m_epdata.NumFleet
                                If Best(j) > 0 Then Fest(i, j) = Me.m_Search.CatchYear(i, j) / Best(j) Else Fest(i, j) = 0
                            Next i

                        Next j

                    Case eAssessmentMethods.DirectExploitation ' Fs from direct exploitation method (eg tagging)

                        For i = 1 To m_epdata.NumFleet
                            For j = 1 To m_epdata.NumLiving
                                Fest(i, j) = (m_Search.CatchYear(i, j) / Bbar(j)) * CSng(Math.Exp(Normal2() * m_data.CVFest(j)))
                            Next
                        Next

                    Case Else
                        Debug.Assert(False, "Assessment Method index set incorrectly")
                        '  MsgBox("Assessment Method index set incorrectly")
                End Select

                'then update relative catchability estimates by gear
                For i = 1 To m_epdata.NumFleet
                    Fwt = 0
                    'If Fgear(i) = 0 Then Fgear(i) = 0.0000000001
                    For j = 1 To m_epdata.NumLiving
                        Fwt = Fwt + Fest(i, j) * m_data.Fweight(i, j)
                    Next
                    Fpred = m_data.Fwc(i, 1) * (1 + m_data.Qgrow(i) / 2)
                    If Fgear(i) > 0 And Fwt > 0 Then
                        m_data.Fwc(i, 1) = Fpred + m_data.KalGainQ(i) * (Fwt / (m_data.Wftot(i) * Fgear(i)) - Fpred)
                    End If
                Next

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".AssessFs() Error: " & ex.Message, ex)
            End Try


        End Sub


        Friend Sub VaryForcing(ByRef ForcingMultTime() As Single)

            Try

                If Me.m_data.bInBatch Then
                    Me.BatchManager.varyForcing(ForcingMultTime)
                End If

            Catch ex As Exception
                'Don't try again....
                Me.m_data.bInBatch = False
                Debug.Assert(False, ex.Message)
            End Try

        End Sub




        Public Sub DoRegulations(ByVal Biomass() As Single, ByVal Effort() As Single, ByVal QMult() As Single, ByVal QYear() As Single, ByVal iTimeStep As Integer, ByVal iMonth As Integer, ByVal iYear As Integer)

            Try

                Me.setTime(iTimeStep, iYear)

                'Is the effort regulated
                If Me.Data.UseQuotaRegs Then
                    'xxxxxxxxxxxxx
                    'Quota used
                    'xxxxxxxxxxxxx

                    'First month of the year
                    If iMonth = 1 Then
                        'Only do the assessment and update the quotas once at the start of each year
                        Me.DoAssessment(Biomass)
                        'update the Quota to the biomass from the assessment
                        Me.UpdateQuotas(Biomass)
                    End If

                    'Is this timestep regulated
                    If Me.isTStepRegulated(iTimeStep) Then
                        'xxxxxxxxxxxxxxxx
                        'Regulated and Quota
                        'xxxxxxxxxxxxxxxx

                        'Regulate the effort every month
                        Me.RegulateEffort(Biomass, QMult, QYear, iTimeStep, iMonth)
                        'Catch base on the regulated effort
                        'Me.CalcCatch(Biomass, QMult, QYear, iTimeStep)

                        '5-Nov-2012 jb Moved check of FisForced() to InitForTrial
                        'And changed it to only set FisForced()=False if the Fleet is controled
                        ''Ecosim will not set F from Effort if there is F timeseries loaded
                        ''this tell Ecosim that there is NO timeseries loaded, even if there is...
                        'For igrp As Integer = 1 To Me.m_data.nLiving
                        '    If Me.m_esData.FisForced(igrp) = True Then
                        '           Me.m_esData.FisForced(igrp) = False
                        '    End If
                        'Next

                        Me.m_Ecosim.SetFtimeFromGear(iTimeStep, QYear, Me.m_esData.PredictSimEffort)

                    Else 'Me.isTStepRegulated(iTimeStep)
                        'xxxxxxxxxxxxxxx
                        'Not Regulated or Quota
                        'xxxxxxxxxxxxxxx

                        'set FishTime(group) (F at timestep) using FishYear and load timeseries data
                        Me.setFishTime(Biomass, Me.m_Search.FishYear, QMult, iTimeStep, iYear)

                    End If 'Me.isTStepRegulated(iTimeStep)

                Else ' If Me.Data.UseQuotaRegs Then
                    'xxxxxxxxxxxxxxxxxxxxx
                    'No Quota
                    'xxxxxxxxxxxxxxxxxxxx

                    'set FishTime(group) (F at timestep) using FishYear and load timeseries data
                    Me.setFishTime(Biomass, Me.m_Search.FishYear, QMult, iTimeStep, iYear)

                End If 'Me.Data.UseQuotaRegs

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".DoRegulations() Exception: " & ex.Message)
                cLog.Write(ex)
                System.Console.WriteLine(Me.ToString & ".DoRegulations() Exception: " & ex.Message)
            End Try

        End Sub


        Public Sub setFishTime(ByVal Biomass() As Single, ByRef FishYear() As Single, ByVal Qmult() As Single, ByVal iTime As Integer, ByVal iyear As Integer)
            Dim igrp As Integer
            'Set FishYear(group) Fishing mortality rate
            'Use FishYear to set FishRateNo(group,time) fishing mortality rate by group,time
            'FishTime(group) F * [density dependant catchability] for the current timestep at the current effort

            '1 set FishYear()
            If Not Me.isTStepRegulated(iTime) Then
                'NOT in a regulated timestep
                'Ok to use timeseries data

                For igrp = 1 To Me.m_data.NGroups

                    'get the correct forcing time step index for this model time step
                    Dim iForced As Integer = Me.m_refData.toForcingTimeStep(iTime, iyear)

                    'Forced F
                    If Me.m_esData.FisForced(igrp) Then
                        'F forcing data was loaded into FishRateNo(group,time) by DoDatValCalculations()
                        FishYear(igrp) = Me.m_esData.FishRateNo(igrp, iTime)
                    End If

                    'forced Catches
                    If iyear <= Me.m_refData.nYears And Me.m_refData.PoolForceCatch(igrp, iForced) > 0 Then
                        FishYear(igrp) = Me.m_refData.PoolForceCatch(igrp, iForced) / Biomass(igrp)
                        If FishYear(igrp) > 3 Then FishYear(igrp) = 3
                    End If

                    'Forced Mortality (Z)
                    'PoolForceZ(iGroup,0) is used in Derivt() to force mortality PoolForceZ(group, 0) = 0 is No forcng
                    If iyear <= Me.m_refData.nDatPoints Then
                        Me.m_refData.PoolForceZ(igrp, 0) = CSng(if(Me.m_refData.PoolForceZ(igrp, iForced) > 0, Me.m_refData.PoolForceZ(igrp, iForced), 0))
                    End If

                Next igrp

            Else
                'Time step is Regulated
                'Turn OFF Forced Mortality PoolForcedZ
                For igrp = 1 To Me.m_data.NGroups
                    Me.m_refData.PoolForceZ(igrp, 0) = 0
                Next

            End If 'If Not Me.isTStepRegulated(iTime) Then

            'NOW
            '2 set FishRateNo(group,time) to FishYear(group)
            '3 set FishTime(group) to FishRateNo(group,time) * [density dep catchability]
            For igrp = 1 To Me.m_data.NGroups

                'set FishRateNo() to computed F from FishYear() and/or loaded timeseries data if not in regulated timestep
                Me.m_esData.FishRateNo(igrp, iTime) = FishYear(igrp)
                'FishTime() is F at current t for Ecosim (Derivt)
                Me.m_esData.FishTime(igrp) = Me.m_esData.FishRateNo(igrp, iTime) * Qmult(igrp)

            Next igrp

        End Sub

        Friend Sub RegulateEffort(ByVal Biomass() As Single, ByVal QMult() As Single, ByVal QYear() As Single, ByVal t As Integer, imonth As Integer)
            Dim i As Integer, ig As Integer, Elim As Single, Emax As Single
            Dim ci As Single

            If Not Me.isTStepRegulated(t) Then Return

            If Me.m_data.UseLPSolution Then

                'Only call the LP Solution for the first month of the year
                If imonth = 1 Then
                    'RegulateEffortViaLPSolve(Biomass, QMult, QYear, t)
                    RegulateLPEffort(Biomass, QMult, QYear, t)
                Else
                    For ig = 1 To Me.m_epdata.NumFleet
                        Me.m_esData.FishRateGear(ig, t) = Me.m_esData.FishRateGear(ig, t - 1)
                    Next
                End If

            Else

                'does regulatory reduction in FishRateGear(ig,t) for each ig (gear)
                For ig = 1 To m_esData.nGear

                    Select Case Me.m_data.QuotaType(ig)

                        Case eQuotaTypes.Effort
                            'NOT IMPLEMENTED at this time
                            Debug.Assert(Me.m_data.QuotaType(ig) = eQuotaTypes.Effort, "Effort regulations have not been implemented at this time!")

                        Case eQuotaTypes.Weakest 'limit effort to weakest stock

                            For i = 1 To m_data.NGroups
                                If (m_epdata.Landing(ig, i) + m_epdata.Discard(ig, i)) > 0 Then
                                    'Calculate the effort limitation, has quote been exceeded?
                                    Elim = CSng(Me.m_data.QuotaTime(ig, i) / (1.0E-20 + QMult(i) * QYear(ig) * m_esData.FishMGear(ig, i) * Biomass(i)))
                                    Debug.Assert(Elim >= 0)
                                    If m_esData.FishRateGear(ig, t) > Elim Then
                                        m_esData.FishRateGear(ig, t) = Elim
                                    End If
                                End If
                            Next i

                        Case eQuotaTypes.HighestValue, eQuotaTypes.Selective 'limit effort to highest economic value stock but discard overages on weaker stocks

                            Emax = 0
                            Dim vmax As Single = 0
                            Dim imax As Integer = 0
                            Dim v As Single
                            For i = 1 To m_data.NGroups
                                If (m_epdata.Landing(ig, i)) > 0 Then
                                    'find the stock with the biggest economic value
                                    v = CSng(Me.m_data.QuotaTime(ig, i) * Me.m_epdata.Market(ig, i))
                                    If v > vmax Then
                                        vmax = v
                                        imax = i
                                    End If

                                End If
                            Next i

                            'get the effort limit for the stock with the biggest value
                            Emax = CSng(Me.m_data.QuotaTime(ig, imax) / (1.0E-20 + QMult(imax) * QYear(ig) * m_esData.FishMGear(ig, imax) * Biomass(imax)))

                            'Limit the effort if it is greater than the max allowable 
                            If Emax < m_esData.FishRateGear(ig, t) Then m_esData.FishRateGear(ig, t) = Emax

                            For i = 1 To m_data.NGroups
                                If (m_epdata.Landing(ig, i)) > 0 Then
                                    ci = m_esData.FishRateGear(ig, t) * QMult(i) * QYear(ig) * m_esData.FishMGear(ig, i) * Biomass(i)

                                    If ci > Me.m_data.QuotaTime(ig, i) Then
                                        'fishing mortality exceeds quota
                                        Me.m_esData.PropLandedTime(ig, i) = CSng(Me.m_data.QuotaTime(ig, i) / (ci + 1.0E-20))
                                        If Me.m_data.QuotaType(ig) = eQuotaTypes.HighestValue Then
                                            'QuotaType = Strongest 
                                            'excess catch discarded and included in the fishing mortailtiy
                                            Me.m_esData.Propdiscardtime(ig, i) = (1 - Me.m_esData.PropLandedTime(ig, i)) * m_epdata.PropDiscardMort(ig, i)
                                        Else
                                            'QuotaType = Selective 
                                            'excess catch is NOT included in fishing mortaility all discards survive
                                            Me.m_esData.Propdiscardtime(ig, i) = 0
                                        End If

                                    Else
                                        'ci < QuotaTime
                                        Me.m_esData.PropLandedTime(ig, i) = m_epdata.PropLanded(ig, i)
                                        Me.m_esData.Propdiscardtime(ig, i) = m_epdata.PropDiscard(ig, i)
                                    End If

                                End If
                            Next i

                    End Select
                Next ig
            End If

            ''Write out Effort calculated by one of the options above
            'System.Console.WriteLine("Effort via option.")
            'For ig = 1 To m_esData.nGear
            '    System.Console.Write(m_esData.FishRateGear(ig, t).ToString & ", ")
            'Next
            'System.Console.WriteLine()

            Try
                Me.m_core.PluginManager.MSERegulateEffort(Biomass, QMult, QYear, t)
            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".DoAssessment()PluginManager.MSERegulateEffort Exception: " & ex.Message)
            End Try

        End Sub

        Private Sub InitLPSolver()

            'Me.m_LPSolver = New SimplexSolver
            Me.m_LPSolver = New cLPSolver

            ReDim m_FleetCode(Me.m_data.nFleets)
            ReDim m_GroupCode(Me.m_data.NGroups + 1)
            ReDim m_QStar(Me.m_data.NGroups, Me.m_data.nFleets)

            'Add the Fleets as Variables and get the Variable ID's into m_FleetCode
            For iflt As Integer = 1 To Me.m_data.nFleets
                Me.m_LPSolver.AddVariable(Me.m_epdata.FleetName(iflt), m_FleetCode(iflt))
                'Set the bounds 
                Me.m_LPSolver.SetBounds(m_FleetCode(iflt), Me.m_data.LowLPEffort(iflt), Me.m_data.UpperLPEffort(iflt)) 'Me.m_data.MaxEffort(iflt)
            Next

            For igrp As Integer = 1 To Me.m_data.nLiving
                Me.m_LPSolver.AddRow(Me.m_epdata.GroupName(igrp), m_GroupCode(igrp))
            Next

            Me.m_LPSolver.AddRow("VALUE", Me.m_GoalRowID)

            Me.m_LPSolver.AddGoal(m_GoalRowID, 1, False)

        End Sub

        ''' <summary>
        ''' Compute the effort regulations via the Linear Solution 
        ''' </summary>
        ''' <param name="Biomass">Biomass at the current time step for all the groups</param>
        ''' <param name="QMult">Density dependant Q (fishing mort) multiplier</param>
        ''' <param name="QYear">Growth in Catchabilty per year</param>
        ''' <param name="t">Time step counter for this time step</param>
        ''' <remarks></remarks>
        Private Sub RegulateLPEffort(ByVal Biomass() As Single, ByVal QMult() As Single, ByVal QYear() As Single, ByVal t As Integer)
            Dim iFlt As Integer, iGrp As Integer

            Dim VPerEffort() As Single
            ReDim VPerEffort(Me.m_data.nFleets)

            'Get fishing mortality at this time step
            For iFlt = 1 To Me.m_data.nFleets
                For iGrp = 1 To Me.m_data.nLiving
                    If t > 1 Then
                        'QStar(iGrp, iFlt) = Me.m_esData.FishMGear(iFlt, iGrp) * QYear(iFlt) * QMult(iGrp)
                        'Using Kalman filter to update catchability estimate
                        Me.m_data.Qest(iGrp, iFlt) = (1 - Me.m_data.KalGainQ(iFlt)) * (Me.m_data.CatchYear(iFlt, iGrp) / 12) / Me.m_data.BestimateLast(iGrp) / (Me.m_esData.FishRateGear(iFlt, t - 12) + 1.0E-20F) + Me.m_data.KalGainQ(iFlt) * Me.m_data.Qest(iGrp, iFlt)
                    End If
                    ' Me.m_data.Qest(iGrp, iFlt) = Me.m_esData.FishMGear(iFlt, iGrp) * QYear(iFlt) * QMult(iGrp)
                    Me.m_data.QStar(iGrp, iFlt) = Me.m_data.Qest(iGrp, iFlt) * (Me.m_esData.PropLandedTime(iFlt, iGrp) + (1 - Me.m_esData.PropLandedTime(iFlt, iGrp)) * m_epdata.PropDiscardMort(iFlt, iGrp))
                Next iGrp
            Next iFlt

            'Get value for the LP Solver
            For iFlt = 1 To Me.m_data.nFleets
                For iGrp = 1 To Me.m_data.nLiving
                    VPerEffort(iFlt) += Me.m_data.QStar(iGrp, iFlt) * Biomass(iGrp) * Me.m_epdata.Market(iFlt, iGrp) * Me.m_esData.PropLandedTime(iFlt, iGrp)
                Next iGrp
            Next iFlt
            Dim sumF As Single
            For iGrp = 1 To Me.m_data.nLiving
                sumF = 0
                For iFlt = 1 To Me.m_data.nFleets
                    Me.m_LPSolver.SetCoefficient(Me.m_GroupCode(iGrp), Me.m_FleetCode(iFlt), Me.m_data.QStar(iGrp, iFlt))
                    sumF += Me.m_data.QStar(iGrp, iFlt)
                Next
                'Debug.Assert(sumF <= Me.m_data.FTarget(iGrp))
                Me.m_LPSolver.SetBounds(Me.m_GroupCode(iGrp), 0, Me.m_data.FTarget(iGrp))
            Next

            For iFlt = 1 To Me.m_data.nFleets
                Me.m_LPSolver.SetCoefficient(Me.m_GoalRowID, Me.m_FleetCode(iFlt), VPerEffort(iFlt))
                Me.m_LPSolver.SetBounds(Me.m_GoalRowID, 0, Double.PositiveInfinity)
            Next

            Dim lpSolveReturnValue As EwEUtils.Core.eSolverReturnValues
            lpSolveReturnValue = Me.m_LPSolver.Solve(t)

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'MS Sensitivity Dual Values 
            'Dim solverParams As SimplexSolverParams = New SimplexSolverParams()
            'solverParams.GetSensitivityReport = True
            'solverParams.GetInfeasibilityReport = True

            'Me.m_LPSolver.Solve(solverParams)

            ''For LPSolver use the get_sensitivity_rhs() or get_dual_solution()to get the Dual values/Shadow values
            'Dim reportSensitivity As ILinearSolverReport = m_LPSolver.GetReport(LinearSolverReportType.Sensitivity)

            'If reportSensitivity IsNot Nothing Then
            '    Dim sensitivityReport As ILinearSolverSensitivityReport = TryCast(reportSensitivity, ILinearSolverSensitivityReport)
            'End if
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            'Dual or Shadow variables
            'Effort is regulated once a year at the first time step of the month
            'This populates all the time steps for this year with the dual values
            For iGrp = 1 To m_data.nLiving
                Dim dv As Single = Math.Abs(CSng(Me.m_LPSolver.GetDualValue(Me.m_GroupCode(iGrp))))
                't is the first month of this year
                For it As Integer = t To t + 11
                    Me.m_data.FLPDualValue.AddValue(iGrp, it, dv)
                Next
            Next

            If lpSolveReturnValue = eSolverReturnValues.OPTIMAL Then
                For iFlt = 1 To Me.m_data.nFleets
                    Me.m_esData.FishRateGear(iFlt, t) = CSng(Me.m_LPSolver.GetValue(Me.m_FleetCode(iFlt)))
                    'System.Console.Write("Fleet ID " & Me.m_LPSolver.GetValue(Me.m_FleetCode(iFlt)).ToString)
                Next
            Else
                'LP Solver failed to find an optimized solution 
                'add the failed time step to the list of non optimal solutions
                Me.m_data.lstNonOptSolutions.Add(t)

                'populate Effort with the effort from the last time step
                Dim tNonOpt As Integer = t - 1
                If t = 1 Then tNonOpt = 1
                For iFlt = 1 To Me.m_data.nFleets
                    Me.m_esData.FishRateGear(iFlt, t) = Me.m_esData.FishRateGear(iFlt, tNonOpt)
                    'System.Console.Write("Fleet ID " & Me.m_LPSolver.GetValue(Me.m_FleetCode(iFlt)).ToString)
                Next

            End If

        End Sub

        ''' <summary>
        ''' Get the LP Solution from the lpsolve55 API directly, instead of from the cLPSolver wrapper
        ''' </summary>
        ''' <param name="Biomass"></param>
        ''' <param name="QMult"></param>
        ''' <param name="QYear"></param>
        ''' <param name="t"></param>
        ''' <remarks>This is for debugging the setup of LPSolve via the API</remarks>
        Private Sub RegulateEffortViaLPSolve(ByVal Biomass() As Single, ByVal QMult() As Single, ByVal QYear() As Single, ByVal t As Integer)
            Dim iFlt As Integer, iGrp As Integer
            Dim VPerEffort() As Double

            Try

                cLPSolver.lpsolve55.Init()
                ReDim VPerEffort(Me.m_data.nFleets)

                Dim ptrLp As Integer = cLPSolver.lpsolve55.make_lp(0, Me.m_data.nFleets)
                Dim badded As Boolean
                'Add the Fleets as Variables and get the Variable ID's into m_FleetCode
                For iFlt = 1 To Me.m_data.nFleets
                    badded = cLPSolver.lpsolve55.set_bounds(ptrLp, iFlt, CDbl(Me.m_data.LowLPEffort(iFlt)), CDbl(Me.m_data.UpperLPEffort(iFlt)))
                Next


                'Get fishing mortality at this time step
                For iFlt = 1 To Me.m_data.nFleets
                    For iGrp = 1 To Me.m_data.NGroups
                        If t > 1 Then
                            'QStar(iGrp, iFlt) = Me.m_esData.FishMGear(iFlt, iGrp) * QYear(iFlt) * QMult(iGrp)
                            'Using Kalman filter to update catchability estimate
                            Me.m_data.Qest(iGrp, iFlt) = (1 - Me.m_data.KalGainQ(iFlt)) * (Me.m_data.CatchYear(iFlt, iGrp) / 12) / Me.m_data.BestimateLast(iGrp) / (Me.m_esData.FishRateGear(iFlt, t - 12) + 1.0E-20F) + Me.m_data.KalGainQ(iFlt) * Me.m_data.Qest(iGrp, iFlt)
                        End If
                        Me.m_data.Qest(iGrp, iFlt) = Me.m_esData.FishMGear(iFlt, iGrp) * QYear(iFlt) * QMult(iGrp)
                        Me.m_data.QStar(iGrp, iFlt) = Me.m_data.Qest(iGrp, iFlt) * (Me.m_esData.PropLandedTime(iFlt, iGrp) + (1 - Me.m_esData.PropLandedTime(iFlt, iGrp)) * m_epdata.PropDiscardMort(iFlt, iGrp))
                    Next iGrp
                Next iFlt

                'Get value for the LP Solver
                For iFlt = 1 To Me.m_data.nFleets
                    For iGrp = 1 To Me.m_data.NGroups
                        VPerEffort(iFlt) += Me.m_data.QStar(iGrp, iFlt) * Biomass(iGrp) * Me.m_epdata.Market(iFlt, iGrp) * Me.m_esData.PropLandedTime(iFlt, iGrp)
                    Next iGrp
                Next iFlt

                'Added the objective/goal before adding rows/constraints
                badded = cLPSolver.lpsolve55.set_obj_fn(ptrLp, VPerEffort)

                Dim constraint() As Double
                ReDim constraint(Me.m_data.nFleets)
                For iGrp = 1 To Me.m_data.NGroups
                    For iFlt = 1 To Me.m_data.nFleets
                        constraint(iFlt) = CDbl(Me.m_data.QStar(iGrp, iFlt))
                    Next
                    badded = cLPSolver.lpsolve55.add_constraint(ptrLp, constraint, cLPSolver.lpsolve55.lpsolve_constr_types.LE, Me.m_data.FTarget(iGrp))
                Next

                cLPSolver.lpsolve55.set_maxim(ptrLp)
                Dim rv As cLPSolver.lpsolve55.lpsolve_return
                rv = cLPSolver.lpsolve55.solve(ptrLp)
                If rv <> cLPSolver.lpsolve55.lpsolve_return.OPTIMAL Then
                    System.Console.WriteLine("LP Solver Non Optimal Solution: " & rv.ToString & " Timestep = " & t.ToString)
                End If

                Dim solution() As Double
                ReDim solution(1 + cLPSolver.lpsolve55.get_Ncolumns(ptrLp) + cLPSolver.lpsolve55.get_Nrows(ptrLp))
                cLPSolver.lpsolve55.get_primal_solution(ptrLp, solution)

                Dim dualValues() As Double
                ReDim dualValues(1 + cLPSolver.lpsolve55.get_Ncolumns(ptrLp) + cLPSolver.lpsolve55.get_Nrows(ptrLp))
                cLPSolver.lpsolve55.get_dual_solution(ptrLp, dualValues)

                For iFlt = 1 To Me.m_data.nFleets
                    Me.m_esData.FishRateGear(iFlt, t) = CSng(solution(Me.m_data.NGroups + iFlt))
                    '    System.Console.Write("Fleet ID " & Me.m_LPSolver.GetValue(Me.m_FleetCode(iFlt)).ToString)
                Next

                For iGrp = 1 To m_data.nLiving
                    For it As Integer = t To t + 11
                        Me.m_data.FLPDualValue.AddValue(iGrp, it, CSng(Math.Abs(dualValues(iGrp))))
                    Next
                Next

                'cLPSolver.lpsolve55.write_lp(ptrLp, "lp.txt")
                cLPSolver.lpsolve55.delete_lp(ptrLp)

            Catch ex As Exception

            End Try

        End Sub


        Private Sub setStartTEndT()
            Me.m_StartT = (Me.m_data.StartYear - 1) * Me.m_esData.NumStepsPerYear + 1
            If Me.m_data.EndYear > 0 Then
                Me.m_EndT = Me.m_data.EndYear * Me.m_esData.NumStepsPerYear
            Else
                Me.m_EndT = Me.m_esData.NTimes
            End If
        End Sub

        Public Function isTStepRegulated(ByVal t As Single) As Boolean

            'is this timestep in bounds
            If t >= Me.m_StartT And t <= Me.m_EndT Then
                Return True
            End If
            'don't regulate effort
            ' System.Console.WriteLine("MSE no regulation. Time step not in StartYear or EndYear.")
            Return False

        End Function


        Private Function stockRecruitment(ByVal iGroup As Integer, ByVal B As Single, ByVal BioEst As Single, ByVal Blast As Single) As Single
            'B is the biomass calculated by Ecosim
            'BioEst is the observed biomass(Ecosim biomass + random variation)
            'Blast is the biomass predicted for the last timestep ( Blast = stockRecruitment(t-1) )

            Dim RstockPred As Single
            Dim vPred As Single
            Dim Best As Single
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ' What this correction basically does is to increase the year-to-year Biomass gain factor in the delaydifference model (effective GstockPred by year)
            ' for situations where F has been reduced relative to ecopath base, and reduce the factor for years when F is higher than ecopath base.  
            'In the original code, we were just doing a factor reduction based on current F (catchyeargroup/Blast), without correcting relative to the ecopath base value of GstockPred.
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Me.m_data.BestimateLast(iGroup) = Blast * CSng(Math.Exp(-Me.m_Search.CatchYearGroup(iGroup) / Blast)) 
            Me.m_data.BestimateLast(iGroup) = Blast * CSng(Math.Exp(-Me.m_Search.CatchYearGroup(iGroup) / Blast + Me.m_esData.Fish1(iGroup)))
            Me.m_data.CatchYearGroup(iGroup) = 0

            RstockPred = CSng(m_data.Rmax(iGroup) * Me.m_data.BestimateLast(iGroup) / (m_data.BhalfT(iGroup) + Me.m_data.BestimateLast(iGroup)))
            vPred = CSng((m_data.RstockRatio(iGroup) * Me.m_data.cvRec(iGroup)) ^ 2 / (1 - m_data.GstockPred(iGroup) ^ 2))
            Me.m_data.KalmanGain(iGroup) = CSng(vPred / (vPred + Me.m_data.CVbiomEst(iGroup) ^ 2))

            'and then we estimate a biomass from assessments, so Bestimate is what will be used for e.g., the fixed escapement policy.
            'VC091107 fixed problem in eq below
            Best = Me.m_data.KalmanGain(iGroup) * BioEst + (1 - Me.m_data.KalmanGain(iGroup)) * (m_data.GstockPred(iGroup) * Me.m_data.BestimateLast(iGroup) + RstockPred)

            'store the pred/actual
            Dim val As Single
            val = Best / B
            Me.m_data.BioEstStats.AddValue(iGroup, Me.m_curYear, val)

            Return Best

        End Function

        ''' <summary>
        ''' Populates Bestimate() and KalmanGain() for regulated fisheries
        ''' </summary>
        ''' <remarks></remarks>
        Friend Sub DoAssessment(ByVal Biomass() As Single)

            Dim Bobs() As Single
            ReDim Bobs(Me.m_epdata.NumGroups)
            For i As Integer = 1 To Me.m_data.nLiving
                Bobs(i) = Biomass(i) * CSng(Math.Exp(Me.m_data.CVbiomEst(i) * Me.RandomNormal()))
                Me.m_data.Bestimate(i) = Me.stockRecruitment(i, Biomass(i), Bobs(i), Me.m_data.Bestimate(i))
            Next i

            Try
                'give the plugins a shot
                Me.m_core.PluginManager.MSEDoAssessment(Biomass)
            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".DoAssessment()PluginManager.MSEDoAssessment Exception: " & ex.Message)
            End Try

        End Sub


        ''' <summary>
        ''' Update fishing quotas for regulated fisheries
        ''' </summary>
        ''' <remarks>
        ''' Populates <see cref="cMSEDataStructures.QuotaTime">cMSEDataStructures.QuotaTime(ngroups)</see> 
        ''' with the quota for this year based on <see cref="cMSEDataStructures.Bestimate">cMSEDataStructures.Bestimate(ngroups)</see> 
        ''' , biomass from the stock assessment model.
        ''' </remarks>
        Friend Sub UpdateQuotas(ByVal Biomass() As Single)
            Dim iflt As Integer, igrp As Integer
            Dim tQuota() As Single

            ReDim tQuota(Me.m_epdata.NumGroups)
            Array.Clear(Me.m_data.FTarget, 0, Me.m_epdata.NumGroups)
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'HACK WARNING
            'BatchMode (cMSEBatchManager) needs to be able to set FixedF() and TAC() values to zero and still have them considered a valid value
            'It does this by setting values to Epsilon 1.401298E-45 when the user enters zero
            'This is interpreted as >0 then rounded off to zero
            'this allows the interface and database to remain the same Zero means TAC() and FixedF() are NOT USED.
            'It would be tricky to fix this with a flag and not break existing models.
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            '
            '1 Set the quota via Fixed Escapement, Fixed Fishing Mortality or Target Fishing Mortality(hockey stick)
            '2 Apply uncertainty to the Quota
            '3 Share the Quota between the fleets
            For igrp = 1 To Me.m_epdata.NumLiving

                If Me.m_data.TAC(igrp) > 0 Then
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'Total Allowable Catch
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    Dim tac As Single = CSng(Math.Round(m_data.TAC(igrp), 5))
                    tQuota(igrp) = tac

                ElseIf Me.m_data.FixedEscapement(igrp) > 0 Then
                    'xxxxxxxxxxxxxxxxxxxxxxx
                    'Fixed Escapement
                    'xxxxxxxxxxxxxxxxxxxxxxx

                    tQuota(igrp) = m_data.Bestimate(igrp) - m_data.FixedEscapement(igrp)
                    If tQuota(igrp) < 0 Then tQuota(igrp) = 0

                ElseIf m_data.FixedF(igrp) > 0 Then
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'Fixed Mortality
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxx
                    Dim f As Single = CSng(Math.Round(m_data.FixedF(igrp), 5))
                    tQuota(igrp) = f * Me.m_data.Bestimate(igrp)
                    Me.m_data.FTarget(igrp) = f

                Else
                    'xxxxxxxxxxxxxxxxxxxxxxxx
                    'Target Fishing Mortality
                    'xxxxxxxxxxxxxxxxxxxxxxxx
                    Dim brange As Single = Me.m_data.Bbase(igrp) - Me.m_data.Blim(igrp)
                    If brange <= 0 Then brange = 1.0E-20

                    'VC to JB: I think the Biomass below should be Bestimate instead; talked to Carl and he agrees. will be a double wham, which is OK.
                    Me.m_data.FTarget(igrp) = Me.m_data.Fopt(igrp) * (Me.m_data.Bestimate(igrp) - Me.m_data.Blim(igrp)) / brange

                    'constrain the fishing mortality to min and max values. 
                    'Fmin(igrp) only gets set by the MSEBatchManager for all other runs it must be zero. 
                    If Me.m_data.FTarget(igrp) < Me.m_data.Fmin(igrp) Then Me.m_data.FTarget(igrp) = Me.m_data.Fmin(igrp)
                    If Me.m_data.FTarget(igrp) > Me.m_data.Fopt(igrp) Then Me.m_data.FTarget(igrp) = Me.m_data.Fopt(igrp)

                    tQuota(igrp) = Me.m_data.FTarget(igrp) * Me.m_data.Bestimate(igrp)

                End If

                'Add uncertainty to the Quota set above
                'VC091104 There will also be uncertainty on how well this quota is implemented so add this:
                'but assume uncertainty is smaller?????? not done here
                tQuota(igrp) = tQuota(igrp) * CSng(Math.Exp(Me.m_data.CVbiomEst(igrp) * Me.RandomNormal() - 0.5 * Me.m_data.CVbiomEst(igrp) ^ 2))

            Next igrp

            'Share the Quota across the fleets for this timestep
            For iflt = 1 To Me.m_esData.nGear
                For igrp = 1 To Me.m_data.NGroups
                    Me.m_data.QuotaTime(iflt, igrp) = tQuota(igrp) * Me.m_data.Quotashare(iflt, igrp)
                Next
            Next

            Try
                Me.m_core.PluginManager.MSEUpdateQuotas(Biomass)
            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".DoAssessment()PluginManager.MSEUpdateQuotas Exception: " & ex.Message)
            End Try

            Me.StoreQuotas(tQuota)

        End Sub


        ''' <summary>
        ''' Save the quota values to memory
        ''' </summary>
        ''' <param name="QuotaT">Quota array by group</param>
        Private Sub StoreQuotas(ByVal QuotaT() As Single)
            Dim igrp As Integer

            If Not Me.m_core.Autosave(eAutosaveTypes.MSE) Then Return

            Try

                Dim d(,) As Single = Me.m_lstData.Item(eResultsData.GroupQuota)
                For igrp = 1 To Me.m_epdata.NumGroups
                    For it As Integer = 1 To 12
                        d(igrp, it + (Me.m_curYear - 1) * 12) = QuotaT(igrp)
                    Next
                Next

                Dim sumb As Single
                d = Me.m_lstData.Item(eResultsData.FleetQuota)
                For iflt As Integer = 1 To Me.m_epdata.NumFleet
                    sumb = 0
                    For igrp = 1 To Me.m_epdata.NumGroups
                        sumb += QuotaT(igrp) * Me.m_data.Quotashare(iflt, igrp)
                    Next
                    For it As Integer = 1 To 12
                        d(iflt, it + (Me.m_curYear - 1) * 12) = sumb
                    Next

                Next

            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".saveQuotas() Exception" & ex.Message)
            End Try

        End Sub

        Private Function Normal2() As Single
            Dim R As Single
            'R = -6
            'For i = 1 To 12
            '    R = R + Rnd
            'Next
            R = CSng(2 * Me.m_rndGen.NextDouble - 1)
            Normal2 = CSng(Math.Log((1 + R) / (1 - R)) / 1.82)

        End Function

        Function RandNormDist(ByVal stdev As Single, ByVal mean As Single) As Single
            Return Normal() * stdev + mean
        End Function

        ''' <summary>
        ''' Box-Muller normally distributed random number with a standard deviation of one
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Normal() As Single
            Dim V1 As Double, V2 As Double
            Do
                V1 = m_rndGen.NextDouble
                V2 = m_rndGen.NextDouble
            Loop Until V1 > 0
            Return CSng(Math.Sqrt(-2 * Math.Log(V1)) * Math.Cos(2 * 3.14159 * V2))
        End Function

#End Region

#Region "MSY"


        Public Sub RunMSYSearch()
            'WE'll run Ecosim for an additional 25 years to avoid the effort not being sustainable

            Me.m_data.StopRun = False

            Dim NumberOfYears As Integer = Me.m_esData.NumYears
            Dim extraYears As Integer = 25

            'Setup Ecosim 
            'timestep handler that ecosim will call where we can grab data during the run
            'see the Private Sub onMSYEcosimTimestep(...)
            Me.m_Ecosim.TimeStepDelegate = AddressOf Me.onMSYEcosimTimestep

            Me.m_esData.bTimestepOutput = True

            Dim MSYeffort(Me.m_esData.nGear) As Single
            Dim MSYF(Me.m_epdata.NumGroups) As Single
            Dim bMSY(Me.m_esData.nGear, Me.m_epdata.NumGroups) As Double
            Dim fMSY(Me.m_esData.nGear, Me.m_epdata.NumGroups) As Double

            'Dim GroupEffort() As Single

            Dim iDataset As Integer = Me.m_core.ActiveTimeSeriesDatasetIndex
            Dim DS As cTimeSeriesDataset

            If iDataset > -1 Then DS = Me.m_core.TimeSeriesDataset(iDataset)

            'this is required to set the base effort values :
            m_core.EcoSimModelParameters.NumberYears = NumberOfYears + extraYears
            SetBaseValues()

            If Me.m_core.PluginManager IsNot Nothing Then
                Me.m_pluginManager.MSYRunStarted(Me.m_data, Me.m_esData)
            End If

            'next is a vc temp fix for debugging
            'Data.MSYStartTimeIndex = 649
            Dim FinestEffortStep As Double = 0.01


            Try

                For iFlt As Integer = 1 To Me.m_esData.nGear
                    If Data.MSYEvaluateFleet(iFlt) Then
                        MSYeffort(iFlt) = 0
                        'ReDim GroupEffort(m_data.NGroups)
                        'For iGroup As Integer = 1 To Me.m_data.NGroups
                        'If Data.MSYEvaluateGroup(iGroup) Then
                        'If m_epdata.Landing(iFlt, iGroup) > 0 Then 'only if this fleet catches this group
                        Dim Done As Boolean = False
                        Dim CurValue As Double = 0
                        Dim lastValue As Double = 0
                        Dim maxValue As Double = 0

                        Dim lastEffort As Double = 0
                        Dim TooBigEffort As Double = -99
                        Dim TooLowEffort As Double = -0.5
                        Dim tryEffort As Double = 0.5

                        Dim TSdisabled As cTimeSeries = Nothing
                        Dim NumberOfSteps As Integer = 0
                        If iDataset > -1 Then
                            For Each ts As cTimeSeries In DS
                                'jb changed to FishingMortality it's forced FishingMortality that prevents the effort from being set...I think...
                                'If ts.TimeSeriesType = eTimeSeriesType.FishingEffort Then
                                If ts.TimeSeriesType = eTimeSeriesType.FishingMortality Then
                                    ' If DirectCast(ts, cFleetTimeSeries).FleetIndex = iflt And ts.Enabled = True Then
                                    'there is an effort in time series, so turn it off. 
                                    ts.Enabled = False
                                    TSdisabled = ts
                                    m_core.UpdateTimeSeries()
                                    'DS.Update()
                                    'End If
                                End If
                            Next

                        End If

                        'UpdateTimeSeries() will reset the NumberYears to the number of years in the timeseries
                        'make sure this did not make NumberYears smaller
                        If m_core.EcoSimModelParameters.NumberYears < NumberOfYears + extraYears Then
                            m_core.EcoSimModelParameters.NumberYears = m_core.EcoSimModelParameters.NumberYears + extraYears
                        End If

                        'when projecting the time series, the forcing functions shuld be set to the average over the ecosim run, not to 1

                        System.Console.WriteLine()
                        Dim EffortFactor As Integer = 2
                        Dim CheckTangent As Boolean = False
                        Dim LastLowerEffort As Double = 0
                        Do While Done = False

                            NumberOfSteps += 1

                            Me.SetFishingEffort(iFlt, CSng(tryEffort))

                            'let ecosim init to the new values
                            Me.m_Ecosim.Init(True)

                            'run ecosim with the current effort
                            Me.m_Ecosim.Run()

                            'evaluate the ecosim output for this fleet/effort combination
                            CurValue = Me.EvaluateMSY(iFlt)

                            'if a fishery catches a group with low catch but high biomass, it may cause the effort to skyrocket
                            'to avoid this: set a limit on the F values for the exploited groups:
                            'if that happens set the value to a low value, so that it may try a lower effort
                            ' If CheckIfFishingMortalitiesTooHigh(iflt) Then CurValue = CurValue / 2


                            If CurValue = 0 Then Done = True 'no effort or no value
                            If CurValue < 0 Then Stop

                            If CheckTangent Then   'only get in here if we are checking the neighbourhood of the max value
                                If CurValue < maxValue Then
                                    'go downward
                                    TooLowEffort = LastLowerEffort
                                    TooBigEffort = lastEffort
                                Else
                                    'move upward
                                    LastLowerEffort = TooLowEffort
                                    ' TooLowEffort = lastEffort
                                    ' maxValue = CurValue
                                End If
                                CheckTangent = False
                            ElseIf CurValue > maxValue Then   'this is the normal entrypont when moving up
                                LastLowerEffort = TooLowEffort
                                TooLowEffort = lastEffort
                                If maxValue > 0 And TooBigEffort < 0 Then  'move faster if just starting out, and long way to go
                                    If CurValue / maxValue > 0.97 * EffortFactor Then EffortFactor = 4 Else EffortFactor = 2
                                End If
                                maxValue = CurValue
                                MSYeffort(iFlt) = CSng(tryEffort)
                            Else
                                If TooBigEffort < 0 Then
                                    TooBigEffort = tryEffort
                                    CheckTangent = True
                                Else
                                    'we are now somewhere below the msy effort, but at what side?
                                    If tryEffort > MSYeffort(iFlt) Then  'on the right side
                                        'reduce the toobigeffor to the current
                                        TooBigEffort = tryEffort
                                    Else   'below MSY
                                        TooLowEffort = tryEffort
                                    End If
                                End If
                            End If

                            If CheckTangent Then
                                tryEffort = 1.001 * TooLowEffort
                                'LastLowerEffort = TooLowEffort / 2
                            Else
                                If TooBigEffort < 0 Then 'NOT YET FOUND THE TOP, SO DOUBLE UP
                                    tryEffort = tryEffort * EffortFactor
                                Else  'have previously found a bigger effort that gave lower value, so now we have bounds
                                    tryEffort = (TooBigEffort - TooLowEffort) / 2 + TooLowEffort
                                End If
                            End If

                            lastValue = CurValue

                            If tryEffort > 0 And CheckTangent = False Then
                                If Math.Abs(1 - lastEffort / tryEffort) < FinestEffortStep Then Done = True
                            End If
                            lastEffort = tryEffort

                            System.Console.WriteLine(NumberOfSteps.ToString & ", Fleet = " & iFlt.ToString & _
                                                     ", MSY effort = " & MSYeffort(iFlt).ToString & _
                                                     ", Cur effort = " & tryEffort.ToString & _
                                                     ", toolow = " & TooLowEffort.ToString & _
                                                     ", toobig = " & TooBigEffort.ToString & _
                                                     ", maxvalue = " & maxValue.ToString & _
                                                     ", curvalue = " & CurValue.ToString)

                            'tell the interface an iteration has been completed
                            Me.fireMSYProgress(New cMSYProgressArgs(NumberOfSteps, iFlt, CSng(MSYeffort(iFlt))))

                            If Me.m_data.StopRun Then Exit Do
                        Loop

                        If TSdisabled IsNot Nothing Then
                            TSdisabled.Enabled = True
                            DS.Update()
                        End If


                        '==================this part not needed for teeb ---------------------------
                        'We now know the MSY effort, so can estimate, oeh, something
                        Me.SetFishingEffort(iFlt, MSYeffort(iFlt))
                        'let ecosim init to the new values
                        Me.m_Ecosim.Init(True)
                        'run ecosim with the current effort
                        Me.m_Ecosim.Run()

                        'now store the average biomasses from this run as the "MSY-biomass" for this fleet run
                        Dim SumBio As Single
                        Dim SumCatch As Single
                        For igrp As Integer = 1 To Me.m_esData.nGroups
                            SumBio = 0
                            SumCatch = 0
                            If Me.m_epdata.Landing(iFlt, igrp) > 0 Then
                                For it As Integer = 1 To Me.m_esData.NTimes
                                    'get data storted by ecosim over time  
                                    SumBio += Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, it)
                                    SumCatch += Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, igrp, it)
                                Next
                            End If
                            bMSY(iFlt, igrp) = SumBio / m_esData.NTimes
                            If SumBio > 0 Then fMSY(iFlt, igrp) = SumCatch / SumBio
                        Next igrp
                        '==================this part not needed for teeb ---------------------------

                        If Me.m_data.StopRun Then Exit For

                        'Finally reset the effort to the original effort (for all fleets)
                        resetEffort(True)
                        'End If
                        'End If
                        'Next iGroup

                        'We now have the effort by group, and can look for the median value:
                        'Evaluate which effort to use, there will be one for each fleet/group, but some of them are 0
                        'Dim NonZeroValues As Integer = 0
                        'For iGrp As Integer = 1 To m_data.NGroups
                        '    If GroupEffort(iGrp) > 0 Then NonZeroValues += 1
                        'Next
                        'Dim GroupEffortText As String = ""
                        'If NonZeroValues > 0 Then
                        '    Dim EffortValues(NonZeroValues) As Single
                        '    Dim iC As Integer = 0

                        '    For iGrp As Integer = 1 To m_data.NGroups
                        '        If GroupEffort(iGrp) > 0 Then
                        '            iC += 1
                        '            EffortValues(iC) = GroupEffort(iGrp)
                        '            GroupEffortText += EffortValues(iC).ToString
                        '        End If
                        '    Next
                        '    'we now have a numeric array with all the non-zero values, so find the median value:
                        '    Array.Sort(EffortValues)

                        '    Dim iLength As Integer = EffortValues.Length
                        '    If iLength Mod 2 = 1 Then
                        '        'uneven so pick this one
                        '        Dim iNo As Integer = iLength \ 2 + 1
                        '        MSYeffort(iFlt) = EffortValues(iNo)
                        '    Else
                        '        'average of the one below and above
                        '        Dim iNo As Integer = iLength \ 2
                        '        MSYeffort(iFlt) = (EffortValues(iNo) + EffortValues(iNo + 1)) / 2
                        '    End If
                        '    'MSYeffort(iFlt) = CSng(Median(EffortValues))

                        'End If

                        'System.Console.WriteLine(GroupEffortText)
                        'Me.fireMSYProgress(New cMSYProgressArgs(NumberOfSteps, iFlt, MSYeffort(iFlt)))
                        'Me.fireMSYProgress(New cMSYProgressArgs(0, iFlt, MSYeffort(iFlt)))
                    End If
                Next iFlt

                'done plugin

                'VC091103: What MSY biomass to use? a group may be caught by several fleets
                'as a first approach I will use the MSY biomass for the fleet that catches most of the species
                For igrp As Integer = 1 To Me.m_esData.nGroups
                    Dim BiggestCatch As Single = -1
                    Dim BiggestFleet As Integer = -1
                    For iflt As Integer = 1 To Me.m_esData.nGear
                        If m_epdata.Landing(iflt, igrp) + m_epdata.Discard(iflt, igrp) > 0 Then
                            'this fleet is catching this group
                            If m_epdata.Landing(iflt, igrp) + m_epdata.Discard(iflt, igrp) > BiggestCatch Then
                                BiggestCatch = m_epdata.Landing(iflt, igrp) + m_epdata.Discard(iflt, igrp)
                                BiggestFleet = iflt
                            End If
                        End If
                    Next
                    'now we know the biggestcatch, so save the biomass from there to the Bmsy:
                    If BiggestFleet > 0 Then
                        m_data.Bbase(igrp) = CSng(bMSY(BiggestFleet, igrp))
                        'assume there's no fishing if the B is below half of the Bmsy
                        m_data.Blim(igrp) = CSng(bMSY(BiggestFleet, igrp) * 0.5)
                        m_data.Fopt(igrp) = CSng(fMSY(BiggestFleet, igrp))

                        'set the reference levels to Blim and Bbase
                        Me.m_data.BioBounds(igrp).Lower = m_data.Blim(igrp)
                        Me.m_data.BioBounds(igrp).Upper = m_data.Bbase(igrp)
                    End If
                Next

                'reset the number of years that Ecosim will run
                m_core.EcoSimModelParameters.NumberYears = NumberOfYears

                If Me.m_core.PluginManager IsNot Nothing Then
                    Me.m_pluginManager.MSYEffortCompleted(MSYeffort, MSYF)
                End If

                'MsgBox("MSY reference levels calculated")

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, Me.ToString & ".RunMSYSearch() Exception: " & ex.Message)
                ' ToDo: globalize this
                Me.m_core.Messages.SendMessage(New cMessage("Error while calculating MSY. " & ex.Message, eMessageType.ErrorEncountered, eCoreComponentType.MSE, eMessageImportance.Critical))
            End Try


        End Sub


        Public Sub RunMSYSearchUsingFishingMortalityInsteadOfEffort()
            'WE'll run Ecosim for an additional 25 years to avoid the effort not being sustainable

            Me.m_data.StopRun = False

            Dim NumberOfYears As Integer = Me.m_esData.NumYears
            Dim extraYears As Integer = 25
            Dim nGroups As Integer = Me.m_data.NGroups

            'Setup Ecosim 
            'timestep handler that ecosim will call where we can grab data during the run
            'see the Private Sub onMSYEcosimTimestep(...)
            Me.m_Ecosim.TimeStepDelegate = AddressOf Me.onMSYEcosimTimestep

            Me.m_esData.bTimestepOutput = True

            Dim MSYF(nGroups) As Single
            Dim MSYEffort(m_epdata.NumFleet) As Single

            'Dim GroupEffort() As Single

            'this is required to set the base effort values :
            m_core.EcoSimModelParameters.NumberYears = NumberOfYears + extraYears
            SetBaseValues()

            If Me.m_core.PluginManager IsNot Nothing Then
                Me.m_pluginManager.MSYRunStarted(Me.m_data, Me.m_esData)
            End If

            'next is a vc temp fix for debugging
            'Data.MSYStartTimeIndex = 649

            Try

                For iGrp As Integer = 1 To nGroups

                    If Data.MSYEvaluateGroup(iGrp) And Me.m_epdata.fCatch(iGrp) > 0 Then
                        'fCatch is the Ecopath sum of landings and discards 
                        Dim maxF As Single = 0
                        For iT As Integer = 1 To Data.MSYStartTimeIndex
                            If m_esData.FishRateNo(iGrp, iT) > maxF Then
                                maxF = Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.FishMort, iGrp, iT)
                                '.m_esData.FishRateNo(iGrp, iT)
                            End If
                        Next

                        MSYF(iGrp) = 0
                        'ReDim GroupEffort(m_data.NGroups)
                        'For iGroup As Integer = 1 To Me.m_data.NGroups
                        'If Data.MSYEvaluateGroup(iGroup) Then
                        'If m_epdata.Landing(iFlt, iGroup) > 0 Then 'only if this fleet catches this group
                        Dim Done As Boolean = False
                        Dim CurValue As Double = 0
                        Dim lastValue As Double = 0
                        Dim maxValue As Double = 0

                        Dim lastF As Double = 0
                        Dim TooBigF As Double = -99
                        Dim TooLowF As Double = -0.5
                        Dim tryF As Double = 0.01

                        Dim NumberOfSteps As Integer = 0

                        If m_core.EcoSimModelParameters.NumberYears < NumberOfYears + extraYears Then
                            m_core.EcoSimModelParameters.NumberYears = NumberOfYears + extraYears
                        End If

                        System.Console.WriteLine()
                        Dim FFactor As Integer = 2
                        Dim CheckTangent As Boolean = False
                        Dim LastLowerF As Double = 0

                        Dim StoreF(12 * m_core.EcoSimModelParameters.NumberYears) As Single
                        SetFishingMortalityOverTime(iGrp, StoreF, False)

                        Do While Done = False

                            NumberOfSteps += 1

                            'let ecosim init to the new values
                            Me.m_Ecosim.Init(True)

                            Me.SetFishingMortality(iGrp, CSng(tryF))


                            'run ecosim with the current effort
                            Me.m_Ecosim.Run()

                            'evaluate the ecosim output for this fleet/effort combination
                            CurValue = Me.EvaluateMSYF(iGrp)

                            'if a fishery catches a group with low catch but high biomass, it may cause the effort to skyrocket
                            'to avoid this: set a limit on the F values for the exploited groups:
                            'if that happens set the value to a low value, so that it may try a lower effort
                            ' If CheckIfFishingMortalitiesTooHigh(iflt) Then CurValue = CurValue / 2


                            If CurValue = 0 Then Done = True 'no effort or no value
                            If CurValue < 0 Then Stop

                            If CheckTangent Then   'only get in here if we are checking the neighbourhood of the max value
                                If CurValue < maxValue Then
                                    'go downward
                                    TooLowF = LastLowerF
                                    TooBigF = lastF
                                Else
                                    'move upward
                                    LastLowerF = TooLowF
                                    ' TooLowEffort = lastEffort
                                    ' maxValue = CurValue
                                End If
                                CheckTangent = False
                            ElseIf CurValue > maxValue Then   'this is the normal entrypont when moving up
                                LastLowerF = TooLowF
                                TooLowF = lastF
                                If maxValue > 0 And TooBigF < 0 Then  'move faster if just starting out, and long way to go
                                    If CurValue / maxValue > 0.97 * FFactor Then FFactor = 4 Else FFactor = 2
                                End If
                                maxValue = CurValue
                                MSYF(iGrp) = CSng(tryF)
                            Else
                                If TooBigF < 0 Then
                                    TooBigF = tryF
                                    CheckTangent = True
                                Else
                                    'we are now somewhere below the msy effort, but at what side?
                                    If tryF > MSYF(iGrp) Then  'on the right side
                                        'reduce the toobigeffor to the current
                                        TooBigF = tryF
                                    Else   'below MSY
                                        TooLowF = tryF
                                    End If
                                End If
                            End If

                            If CheckTangent Then
                                tryF = 1.001 * TooLowF
                                'LastLowerEffort = TooLowEffort / 2
                            Else
                                If TooBigF < 0 Then 'NOT YET FOUND THE TOP, SO DOUBLE UP
                                    tryF = tryF * FFactor
                                Else  'have previously found a bigger effort that gave lower value, so now we have bounds
                                    tryF = (TooBigF - TooLowF) / 2 + TooLowF
                                End If
                            End If
                            'Cap the max F:
                            If tryF > maxF Then
                                tryF = maxF
                                TooBigF = tryF
                            End If


                            lastValue = CurValue
                            If tryF > 0 And CheckTangent = False Then
                                If Math.Abs(1 - lastF / tryF) < 0.01 Then Done = True
                            End If
                            lastF = tryF

                            System.Console.WriteLine(NumberOfSteps.ToString & ", Group = " & iGrp.ToString & _
                                                  ", MSY F = " & MSYF(iGrp).ToString & _
                                                  ", Cur F = " & tryF.ToString & _
                                                  ", toolow = " & TooLowF.ToString & _
                                                  ", toobig = " & TooBigF.ToString & _
                                                  ", maxvalue = " & maxValue.ToString & _
                                                  ", curvalue = " & CurValue.ToString)

                            'tell the interface an iteration has been completed
                            Me.fireMSYProgress(New cMSYProgressArgs(NumberOfSteps, iGrp, CSng(MSYF(iGrp))))


                            If Me.m_data.StopRun Then Exit Do
                        Loop


                        If Me.m_data.StopRun Then Exit For

                        'Finally reset the F to the original value 
                        SetFishingMortalityOverTime(iGrp, StoreF, True)
                    End If
                Next iGrp

                'done plugin

                'reset the number of years that Ecosim will run
                m_core.EcoSimModelParameters.NumberYears = NumberOfYears

                If Me.m_core.PluginManager IsNot Nothing Then
                    Me.m_pluginManager.MSYEffortCompleted(MSYEffort, MSYF)
                End If

                'MsgBox("MSY reference levels calculated")

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, Me.ToString & ".RunMSYSearchUsingFishingMortalityInsteadOfEffort() Exception: " & ex.Message)
                ' ToDo: globalize this
                Me.m_core.Messages.SendMessage(New cMessage("Error while calculating MSYF. " & ex.Message, eMessageType.ErrorEncountered, eCoreComponentType.MSE, eMessageImportance.Critical))
            End Try


        End Sub



        Private Function EvaluateMSY(ByVal curFleet As Integer) As Single
            'MSY Search has just completed a run 
            'evaluate the value of the catch for this fleet with this effort level
            'Dim sumbio As Single

            'VC wants to change this so that it can calc Value or Biomass
            Dim FleetCatchValue As Single = 0
            Dim marketPrice As Single

            For igrp As Integer = 1 To Me.m_esData.nGroups

                If Me.m_epdata.Landing(curFleet, igrp) > 0 Then
                    If Me.m_data.MSYEvaluateValue Then
                        marketPrice = Me.m_epdata.Market(curFleet, igrp)
                    Else
                        marketPrice = 1
                    End If

                    'VC temp fix for debugging:
                    'marketPrice = 1

                    Dim GroupCatch As Single = 0
                    For it As Integer = Me.m_core.nEcosimTimeSteps To Me.m_core.nEcosimTimeSteps  'just one time step should do
                        'only evaluate for the last 25 years: LAST TIMESTEP
                        'For it As Integer = Me.m_esData.NTimes - 25 To Me.m_esData.NTimes
                        'get data stored by ecosim over time  
                        'Dim bio As Single = Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, it)
                        'sumbio += Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, it)
                        'FleetCatchValue += Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, igrp, it) * Me.m_epdata.Market(curFleet, igrp) ' * PropCaughtByThisGear
                        GroupCatch += m_esData.ResultsSumCatchByGroupGear(igrp, curFleet, it)
                        'System.Console.Write("Group " & igrp.ToString & " = " & FleetCatchValue.ToString & ", ")
                    Next
                    'average over the 25 years:
                    FleetCatchValue += (GroupCatch * m_data.MSYGroupWeight(igrp) * marketPrice)
                End If
            Next igrp
            Return FleetCatchValue

        End Function

        Private Function SetFishingEffort(ByVal Fleet As Integer, ByVal Val As Single) As Boolean

            Dim Manager As cFishingEffortShapeManger = Me.m_core.FishingEffortShapeManager
            Dim Shape As cShapeData = Nothing

            Dim StartStep As Integer
            Dim EndStep As Integer
            If Fleet = 0 Then
                StartStep = 0
                EndStep = Me.m_core.nFleets - 1
            Else
                StartStep = Fleet - 1
                EndStep = Fleet - 1
            End If

            For iFl As Integer = StartStep To EndStep
                Shape = Manager.Item(iFl)
                Shape.LockUpdates()
                Try
                    Shape.ShapeData(1) = 1
                    For iTimeStep As Integer = Me.m_data.MSYStartTimeIndex To Me.m_core.nEcosimTimeSteps 'Step cCore.N_MONTHS
                        Shape.ShapeData(iTimeStep) = Val
                        'set effort to unity 
                    Next
                Catch ex As Exception
                    'Return False
                End Try
                Shape.UnlockUpdates()
            Next
            Manager.Update()
            Return True

        End Function

        Private Function EvaluateMSYF(ByVal curGroup As Integer) As Single
            'MSY Search has just completed a run 
            'evaluate the value of the catch for this group with this Fishing mortality

            Dim GroupCatch As Single = 0
            For it As Integer = Me.m_core.nEcosimTimeSteps To Me.m_core.nEcosimTimeSteps
                'only evaluate for the last timestep
                GroupCatch += m_esData.FishRateNo(curGroup, it) * m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, curGroup, it)
                '.Fish1.ResultsOverTime.ResultsSumCatchByGroupGear(igrp, iflt, it)
                'System.Console.Write("Group " & igrp.ToString & " = " & FleetCatchValue.ToString & ", ")
            Next
            'average over the 25 years:
            Return GroupCatch

        End Function


        Private Function SetFishingMortality(ByVal Group As Integer, ByVal Val As Single) As Boolean

            'Dim FManager As cFishingMortalityManger = Me.m_core.FishMortShapeManager
            'Dim Shape As cShapeData = Nothing
            Dim bSucces As Boolean = True
            'Shape = FManager.Item(Group)
            'Shape.LockUpdates()

            Try
                For iTimeStep As Integer = Me.m_data.MSYStartTimeIndex To Me.m_core.nEcosimTimeSteps 'Step cCore.N_MONTHS
                    '    Shape.ShapeData(iTimeStep) = Val
                    m_esData.FishRateNo(Group, iTimeStep) = Val 'FishRateNo(nGroups,nTime)
                Next
            Catch ex As Exception
                bSucces = False
            End Try

            'Shape.UnlockUpdates()
            'FManager.Update()

            Return bSucces

        End Function


        Private Function SetFishingMortalityOverTime(ByVal Group As Integer, ByVal Ftime() As Single, ByVal Setting As Boolean) As Boolean

            'Could not get direct setting of cfishingmortalitymanger to work:
            'Dim FManager As cFishingMortalityManger = Me.m_core.FishMortShapeManager
            'Dim Shape As cShapeData = Nothing
            Dim bSucces As Boolean = True
            'Shape = FManager.Item(Group)
            'Shape.LockUpdates()

            Try
                For iTimeStep As Integer = Me.m_data.MSYStartTimeIndex To Me.m_core.nEcosimTimeSteps
                    If Setting Then
                        m_esData.FishRateNo(Group, iTimeStep) = Ftime(iTimeStep)
                    Else
                        Ftime(iTimeStep) = m_esData.FishRateNo(Group, iTimeStep)
                    End If
                Next
                'For iTimeStep As Integer = Me.m_data.MSYStartTimeIndex To Me.m_core.nEcosimTimeSteps 'Step cCore.N_MONTHS
                '    If Setting Then
                '        Shape.ShapeData(iTimeStep) = Ftime(iTimeStep)
                '    Else
                '        Ftime(iTimeStep) = Shape.ShapeData(iTimeStep)
                '    End If
                'Next

            Catch ex As Exception
                bSucces = False
            End Try

            'Shape.UnlockUpdates()
            'FManager.Update()

            Return bSucces

        End Function


        Private Function CheckIfFishingMortalitiesTooHigh(ByVal curFleet As Integer) As Boolean

            CheckIfFishingMortalitiesTooHigh = False

            For iGrp As Integer = 1 To Me.m_esData.nGroups
                If Me.m_epdata.Landing(curFleet, iGrp) > 0 Then
                    'need to limit the fishing mortality to avoid groups being crashed completely, making a temp fix here
                    'm_esData.FishRateMax(iGrp) = m_epdata.PB(iGrp)
                    'If Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.FishMort, iGrp, m_core.nEcosimTimeSteps) > 10 * m_epdata.PB(iGrp) Then
                    If Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp, m_core.nEcosimTimeSteps) < 0.000001 * m_epdata.B(iGrp) Then
                        ' should really use m_esData.FishRateMax(iGrp) Then
                        Return True
                    End If
                End If
            Next

        End Function

        Public Sub RunBoEstimation()

            'VC091202: We don't need the Bo reference now, using MSY levels instead
            'but leaving the code here for possible use later. 

            'We need a group-specific parameter Bo (unfished biomass). 
            'Its default value is obtained from the fitted model.  
            'We run  the  model for another 50 years. 
            'Then set the fishery for  the species  in question  to 0, 
            'leave other fisheries constant at  the last year’s  effort level. 
            'The  biomass for the species at the  end of the simulation  is our default Bo.      

            'Setup Ecosim 
            'timestep handler that ecosim will call where we can grab data during the run
            'see the Private Sub onMSYEcosimTimestep(...)

            'Try

            '    Me.m_Ecosim.TimeStepDelegate = AddressOf Me.onMSYEcosimTimestep

            '    Me.m_esData.bTimestepOutput = True

            '    Dim iDataset As Integer = Me.m_core.ActiveTimeSeriesDatasetIndex
            '    Dim DS As cTimeSeriesDataset

            '    If iDataset > -1 Then DS = Me.m_core.TimeSeriesDataset(iDataset)

            '    Dim NumberOfYears As Integer = Me.m_esData.NumYears
            '    m_core.EcoSimModelParameters.NumberYears = NumberOfYears + 100
            '    SetBaseValues()


            '    'Setup Ecosim 
            '    'timestep handler that ecosim will call where we can grab data during the run
            '    'see the Private Sub onMSYEcosimTimestep(...)
            '    Me.m_Ecosim.TimeStepDelegate = AddressOf Me.onMSYEcosimTimestep

            '    Me.m_esData.bTimestepOutput = True

            '    For igrp As Integer = 1 To Me.m_esData.nGroups

            '    Next

            '    'Dim MSYeffort(Me.m_esData.nGear) As Single
            '    'Dim MSYbiomass(Me.m_epdata.NumGroups, Me.m_esData.nGear) As Single
            '    'Finally reset the effort to the original effort (for all fleets)
            '    SetEffortToBaseValue(True)


            '    'reset the number of years that Ecosim will run
            '    m_core.EcoSimModelParameters.NumberYears = NumberOfYears

            'Catch ex As Exception

            'End Try


        End Sub

        Private Sub fireMSYProgress(ByVal MYSProgress As cMSYProgressArgs)

            If Me.m_data.MSYRunSilent Then Exit Sub

            Try
                If Me.m_MSYCallBack IsNot Nothing Then
                    Me.m_MSYCallBack(MYSProgress)
                End If
            Catch ex As Exception

            End Try

        End Sub


        ''' <summary>
        ''' Ecosim Timestep delegate handler for the MSY Search
        ''' </summary>
        ''' <param name="iTime"></param>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub onMSYEcosimTimestep(ByVal iTime As Long, ByVal data As cEcoSimResults)

            Try

                'Ecosim has run a time step for the MSY search
                'grab up anything you need during the time step

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".onEcosimTimestep() Error: " & ex.Message)
            End Try

        End Sub

#End Region

#End Region

#Region "Time step data summary"
        ''' <summary>
        ''' Set Biomass and F cv's to use for this year.
        ''' </summary>
        ''' <remarks>CV's can vary on a yearly timestep set by the interface. This sets the CV to use for this year. </remarks>
        Public Sub setTime(ByVal itime As Integer, ByVal iyr As Integer)

            If Me.m_Search.SearchMode <> eSearchModes.MSE Then Return

            Try

                Me.m_curT = itime
                Me.m_curYear = iyr

                'CVbiomEst(ngroups) and CVFest(nfleets) is the cv that is used to vary biomass and fishing mortality
                For igrp As Integer = 1 To Me.m_data.NGroups
                    Me.m_data.CVbiomEst(igrp) = Me.m_data.CVBiomT(igrp, iyr)
                Next

                For iflt As Integer = 1 To Me.m_data.nFleets
                    Me.m_data.CVFest(iflt) = Me.m_data.CVFT(iflt, iyr)
                Next

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".setTime() Exception: " & ex.Message)
                cLog.Write(ex)
            End Try

        End Sub

        ''' <summary>
        ''' Ecosim Timestep delegate handler 
        ''' </summary>
        ''' <param name="iTime"></param>
        ''' <param name="data"></param>
        Private Sub onEcosimTimestep(ByVal iTime As Long, ByVal data As cEcoSimResults)
            Try
                Dim iflt As Integer, igrp As Integer

                If (Me.m_Search.SearchMode <> eSearchModes.MSE) Then Return

                'After the first year

                If (iTime - 1) Mod 12 = 0 Then
                    'First month of a new year
                    Array.Clear(Me.m_data.CatchYear, 0, Me.m_data.CatchYear.Length)
                    ' Array.Clear(Me.m_data.EffortYear, 0, Me.m_data.EffortYear.Length)
                End If

                'grab effort and catch
                For iflt = 1 To Me.m_data.nFleets
                    ' Me.m_data.EffortYear(iflt) = Me.m_esData.FishRateGear(iflt, CInt(iTime))
                    For igrp = 1 To Me.m_data.NGroups
                        Me.m_data.CatchYear(iflt, igrp) += Me.m_esData.ResultsSumCatchByGroupGear(igrp, iflt, CInt(iTime))
                    Next
                Next
                ' System.Console.WriteLine()
                For igrp = 1 To Me.m_data.nLiving
                    Me.m_data.BioStats.AddValue(igrp, CInt(iTime), Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, CInt(iTime)))
                    Me.m_data.CatchGroupStats.AddValue(igrp, CInt(iTime), Me.m_esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, igrp, CInt(iTime)))
                    'Me.m_data.FStats.AddValue(igrp, CInt(iTime), 1 + (Me.m_esData.FishTime(igrp) - Me.m_data.FTarget(igrp)))
                    ' System.Console.Write((Me.m_esData.FishTime(igrp) - Me.m_data.FTarget(igrp)).ToString & ",")
                Next igrp

                Dim sumValue As Single

                For iflt = 1 To Me.m_esData.nGear
                    sumValue += Me.m_esData.ResultsSumValueByGear(iflt, CInt(iTime))
                    Me.m_data.CatchFleetStats.AddValue(iflt, CInt(iTime), Me.m_esData.ResultsSumCatchByGear(iflt, CInt(iTime)))
                    Me.m_data.EffortStats.AddValue(iflt, CInt(iTime), Me.m_esData.ResultsEffort(iflt, CInt(iTime)))
                    ' Me.m_data.EffortYear(iflt) = Me.m_esData.FishRateGear(iflt, CInt(iTime))
                Next iflt

                Me.m_data.ValueFleetStats.AddValue(1, CInt(iTime), sumValue)

            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".onEcosimTimestep() Error: " & ex.Message)
            End Try

        End Sub

        ''' <summary>
        ''' Event handler for Plugin Economic data
        ''' </summary>
        ''' <param name="EconomicData"></param>
        ''' <remarks></remarks>
        Private Sub onEconomicData(ByVal EconomicData As IEconomicData) ' Handles m_EconomicData.onEconomicData

            Try

                'Is there plugin economic data
                If Me.UsePlugin Then
                    'Plugin economic data from the ValueChain pluging is sent out every timestep
                    'Store the data in cMSESummaryStats objects

                    Me.m_data.ProfitSum.AddValue(1, EconomicData.TimeStep, EconomicData.Total.Profit)
                    Me.m_data.JobsSum.AddValue(1, EconomicData.TimeStep, EconomicData.Total.NumberOfJobsTotal)
                    Me.m_data.CostSum.AddValue(1, EconomicData.TimeStep, EconomicData.Total.Cost)

                End If

            Catch ex As Exception
                'make sure all exceptions are handled here and not back in the cEconomicDataSource object
                System.Console.WriteLine(Me.ToString & ".onEconomicData() Error: " & ex.Message)
                cLog.Write(ex)
            End Try

        End Sub


        ''' <summary>
        ''' Summarize the economic data gathered by Ecosim at the end of a trial
        ''' </summary>
        ''' <remarks>Economic data caculated by ecosim at the end of a run</remarks>
        Private Sub summarizeEcosimEconomicData()

            'ToDo_jb cMSE.summarizeEconomicData() figure out how to compute Economic data from the Ecosim data
            If Not Me.UsePlugin Then

                Dim sumValue As Single, sumEffort As Single, sumProfit As Single, sumJobs As Single, sumCost As Single
                For iflt As Integer = 0 To Me.m_esData.nGear

                    For it As Integer = 1 To Me.m_esData.nSumTimeSteps
                        sumValue += Me.m_esData.ResultsSumValueByGear(iflt, it)
                    Next
                    For it As Integer = 1 To Me.m_esData.nSumTimeSteps
                        sumEffort += Me.m_esData.ResultsEffort(iflt, it)
                    Next

                    sumCost += Me.m_Search.NetCost(iflt)

                    ' profit
                    '[sum of value] * [ecopath profit (percentage of catch value that is profit /per unit of effort)]
                    sumProfit = sumValue * (Me.m_epdata.cost(iflt, eCostIndex.Profit) / 100) * sumEffort

                    'TEMP just for something to work with until we have ECost up and running
                    '[value of catch] * [Jobs(fleet) from the search forms]
                    sumJobs = sumValue * Me.m_Search.Jobs(iflt) 'Jobs(Fleet) percentage of value that goes to Jobs default=1

                Next iflt

                'Me.m_data.ProfitSum.AddValue(1, Me.m_Search.totval)
                'Me.m_data.JobsSum.AddValue(1, sumJobs)
                'Me.m_data.CostSum.AddValue(1, sumCost)

            End If

        End Sub

        Public Sub RunFleetTradeoffs()

            Me.m_data.StopRun = False
            Dim buff As StringBuilder
            Dim strm As StreamWriter
            Try

                'Exit Sub

                'no need to set time just use default: m_core.EcoSimModelParameters.NumberYears = NumberOfYears + 25
                'this is required to set the base effort values :
                SetBaseValues()

                Me.m_Ecosim.TimeStepDelegate = AddressOf Me.onMSYEcosimTimestep

                Me.m_esData.bTimestepOutput = True
                'let ecosim init to the new values
                Me.m_Ecosim.Init(True)
                'run ecosim with the current effort
                Me.m_Ecosim.Run()

                Dim nFleets As Integer = Me.m_esData.nGear
                Dim FleetBaseValue(nFleets) As Single
                Dim CurValue() As Single
                'Store the total base value obtained by each fishery 
                For iFlt As Integer = 1 To nFleets
                    'For iGrp As Integer = 1 To m_epdata.NumGroups
                    For it As Integer = 1 To Me.m_esData.NTimes
                        FleetBaseValue(iFlt) += m_esData.ResultsSumValueByGear(iFlt, it)  'm_esData.ResultsSumCatchByGroupGear(iGrp, iFlt, it) * Me.m_epdata.Market(iFlt, iGrp)
                    Next
                    'all of these values are annual values (even if they are by time step), so divide by number of months:
                    FleetBaseValue(iFlt) /= m_esData.NTimes
                    'Next
                Next
                Dim ValueDifferenceFromTo(nFleets, nFleets) As Single


                For iFlt As Integer = 1 To nFleets
                    Dim Manager As cFishingEffortShapeManger = Me.m_core.FishingEffortShapeManager
                    Dim Shape As cShapeData = Nothing

                    Shape = Manager.Item(iFlt - 1)
                    Shape.LockUpdates()

                    For iT As Integer = 1 To Me.m_esData.NTimes
                        Shape.ShapeData(iT) = CSng(0.9 * m_baseEffort(iFlt, iT))
                    Next
                    Shape.UnlockUpdates()
                    Manager.Update()


                    'For it As Integer = 1 To Me.m_esData.NTimes
                    '    Me.m_esData.FishRateGear(iFlt, it) = CSng(1.1 * m_baseEffort(iFlt, it))
                    'Next
                    'let ecosim init to the new values ------ no init will overwrite the effort!!!!!
                    ' Me.m_Ecosim.Init(True)
                    'run ecosim with the current effort
                    Me.m_Ecosim.Run()

                    ReDim CurValue(nFleets)
                    For iTo As Integer = 1 To nFleets
                        For it As Integer = 1 To Me.m_esData.NTimes
                            CurValue(iTo) += m_esData.ResultsSumValueByGear(iTo, it)      'm_esData.ResultsSumCatchByGroupGear(iGrp, iFlt, it) * Me.m_epdata.Market(iFlt, iGrp)
                        Next
                        'divide by no months to get the average, which is the annual value:
                        CurValue(iTo) /= m_esData.NTimes
                    Next


                    'If MoreMoney = 0 Then Stop

                    For iTo As Integer = 1 To nFleets
                        ValueDifferenceFromTo(iFlt, iTo) = (CurValue(iTo) - FleetBaseValue(iTo)) '/ MoreMoney
                    Next


                    'get the directory to dump the data to
                    'Me.m_DataDir = AppDomain.CurrentDomain.BaseDirectory & "MSE\"
                    'strm = New StreamWriter(getFilename("FleetTradeOff", "_Effort"), True)
                    'For iFrom As Integer = 1 To nFleets
                    '    Try
                    '        buff = New StringBuilder
                    '        For iT As Integer = 1 To m_esData.NTimes Step 12
                    '            buff.Append(Me.m_esData.FishRateGear(iFrom, iT).ToString & ", ")
                    '        Next
                    '        strm.WriteLine(buff)

                    '        buff = Nothing
                    '    Catch ex As Exception
                    '        ' Debug.Assert(False, Me.ToString & " Exception saving results to file " & getFilename(BIOMASS_DATA, Me.m_epdata.GroupName(igrp)))
                    '        System.Console.WriteLine(Me.ToString & " Failed to write data to file " & getFilename("FleetTradeOff", Me.m_epdata.FleetName(iFrom)) & " Exception: " & ex.Message)
                    '    End Try
                    'Next
                    'strm.Close()

                    'Finally reset the effort to the original effort
                    'SetEffortToBaseValue(True)
                    Dim Manager2 As cFishingEffortShapeManger = Me.m_core.FishingEffortShapeManager
                    Dim Shape2 As cShapeData = Nothing
                    Shape2 = Manager2.Item(iFlt - 1)
                    'Reset the fishing values
                    Shape2.LockUpdates()
                    For iT As Integer = 1 To Me.m_esData.NTimes
                        Shape2.ShapeData(iT) = m_baseEffort(iFlt, iT)
                    Next
                    Shape2.UnlockUpdates()
                    Manager2.Update()

                Next


                'get the directory to dump the data to
                Me.m_DataDir = AppDomain.CurrentDomain.BaseDirectory & "Tradeoff\"
                Dim mName As String = Me.m_core.m_EcoPathData.ModelName

                strm = New StreamWriter(BuildCSVFilename("FleetTradeOff_", mName), False)
                buff = New StringBuilder()
                'First a line with a blank, then the fleet names
                'buff.Append("From\to ,")
                'For iTo As Integer = 1 To nFleets
                '   buff.Append(Me.m_epdata.FleetName(iTo).ToString & ", ")
                'Next
                'strm.WriteLine(buff)
                For iFrom As Integer = 1 To nFleets
                    Try
                        buff = New StringBuilder()

                        buff.Append(Me.m_epdata.FleetName(iFrom))
                        buff.Append(", ")

                        Dim vSum As Single = 0
                        For iTo As Integer = 1 To nFleets
                            buff.Append(cStringUtils.FormatSingle(ValueDifferenceFromTo(iFrom, iTo)))
                            buff.Append(", ")
                            vSum += ValueDifferenceFromTo(iFrom, iTo)
                        Next
                        buff.Append(cStringUtils.FormatSingle(vSum))
                        strm.WriteLine(buff)

                        buff = Nothing
                    Catch ex As Exception
                        System.Console.WriteLine(Me.ToString & " Failed to write data to file " & BuildCSVFilename("FleetTradeOff", Me.m_epdata.FleetName(iFrom)) & " Exception: " & ex.Message)
                    End Try
                Next

            Catch ex As Exception

            End Try
            strm.Close()
        End Sub

        ''' <summary>
        ''' Normally distrubute random number where mean = 0 std = 1
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function RandomNormal() As Single
            Dim X As Double
            Debug.Assert(m_rndGen IsNot Nothing, Me.ToString & ".RandomNormal() Random number generator has not been initialized!")
            X = -6
            For i As Integer = 1 To 12
                X = X + m_rndGen.NextDouble
            Next
            Return CSng(X)
        End Function


#End Region

    End Class

#End Region

    Public Interface IMSEOutputWriter

        'ReadOnly Property DataDir() As String

        Sub Init()

        Sub saveIteration(ByVal ListOfData As Dictionary(Of cMSE.eResultsData, Single(,)))

    End Interface


End Namespace
