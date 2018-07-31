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

#Region " Imports "

Option Strict On

Imports System.Threading
Imports EwECore.Ecopath
Imports EwECore.Ecosim
Imports EwEPlugin
Imports EwEUtils.Core

#End Region ' Imports 

Public Enum eMCParams As Integer
    NotSet = 0
    Biomass = 1
    PB = 2
    QB = 3
    EE = 4
    BA = 5
    Vulnerability = 6  '(one per consumer) same for all prey
    OtherMort = 7
    Landings = 8
    Discards = 9
    Diets = 10
    ''' <summary>Biomass accummulation rate</summary>
    BaBi = 11
End Enum

Public Enum eMCDietSamplingMethod As Integer
    Dirichlets = 0
    NormalDistribution = 1
End Enum

''' <summary>
''' Call each time a monte carlo trial has been completed
''' </summary>
Public Delegate Sub MonteCarloTrialProgressDelegate()

''' <summary>
''' Call each time a Ecopath model has been run
''' </summary>
Public Delegate Sub MonteCarloEcopathProgressDelegate()

''' <summary>
''' Call at the completion of the monte carlo trials
''' </summary>
''' <remarks>There can be multiple Ecopath model runs for each monte carlo trial</remarks>
Public Delegate Sub MonteCarloCompletedDelegate()

Public Delegate Sub MonteCarloSendMessageDelegate(ByRef Message As cMessage)

''' <summary>
''' Ecosim Monte Carlo routines
''' </summary>
Public Class cEcosimMonteCarlo

    Public Const EE_TOL As Single = 0.0005
    Public Const MAX_ECOPATH_TRIES As Integer = 10000

    'Public DietMultiplier() As Single

    ''' <summary>
    ''' Optional <see cref="EcoSimTimeStepDelegate">delegate</see> that will be called after a 
    ''' trial has been computed.
    ''' </summary>
    Friend EcosimTimeStep As EcoSimTimeStepDelegate

    ''' <summary>
    ''' Optional <see cref="MonteCarloEcopathProgressDelegate">delegate</see> that will be called 
    ''' each attempt to find a balanced Ecopath model.
    ''' </summary>
    Friend dlgEcopathIterationHandler As MonteCarloEcopathProgressDelegate

    ''' <summary>
    ''' Optional <see cref="MonteCarloTrialProgressDelegate">delegate</see> that will be called after a 
    ''' trial has been completed.
    ''' </summary>
    Friend dlgTrialStepHandler As MonteCarloTrialProgressDelegate

    ''' <summary>
    ''' Optional <see cref="MonteCarloCompletedDelegate">delegate</see> that will be called after a 
    ''' Monte Carlo run has completed.
    ''' </summary>
    Friend dlgMonteCarloCompletedHandler As MonteCarloCompletedDelegate

    ''' <summary>
    ''' Optional <see cref="MonteCarloSendMessageDelegate">delegate</see> that allows Monte Carlo
    ''' to send <see cref="cMessage">messages</see>.
    ''' </summary>
    Friend dlgMonteCarloMessageHandler As MonteCarloSendMessageDelegate

    Public nEcopathIterations As Integer
    Public nTrialIterations As Integer

    ''' <summary>
    ''' Best fitting Sum of Squares computed by Ecosim
    ''' </summary>
    Public Property SSBestFit As Single

    ''' <summary>
    ''' Sum of Squares computed by Ecosim of the current iteration.
    ''' </summary>
    Public Property SSCurrent As Single

    ''' <summary>
    ''' Sum of Squares prior to the Monte Carlo run.
    ''' </summary>
    Public Property SSorg As Single

    Public Property EcopathEETol As Single

    ''' <summary>
    ''' Flag stating if Monte Carlo should validate and reject negative respiration values.
    ''' </summary>
    Public Property ValidateRespiration As Boolean = False

    Public Property DietSamplingMethod As eMCDietSamplingMethod = eMCDietSamplingMethod.Dirichlets

    Private m_core As cCore
    Private m_ecopath As cEcoPathModel
    Private m_ecosim As cEcoSimModel
    Private m_epdata As cEcopathDataStructures
    Private m_esdata As cEcosimDatastructures
    Private m_tsdata As cTimeSeriesDataStructures
    Private m_stanza As cStanzaDatastructures 'needs to come in from the core
    Private m_tracerData As cContaminantTracerDataStructures
    Private m_pluginmanager As cPluginManager

    Private isCrashed() As Boolean
    Private isExploded() As Boolean
    Private m_iTrial As Integer
    Private m_bIsBestFit As Boolean = False

    ''' <summary>Ecopath parameters (<see cref="eMCParams">Parameter</see> x nGroup)</summary>
    Public Pmean(,) As Single
    ''' <summary>Ecopath landings (Fleet x Group)</summary>
    Public PMeanLanding(,) As Single
    ''' <summary>Ecopath discards (Fleet x Group)</summary>
    Public PMeanDiscard(,) As Single
    ''' <summary>Ecopath Diets (Group x Group)</summary>
    Public PMeanDC(,) As Single

    ''' <summary>
    ''' CV value (parameter x group)
    ''' </summary>
    Public CVpar(,) As Single
    ''' <summary>
    ''' CV value for landings (fleet x group)
    ''' </summary>
    Public CVparLanding(,) As Single
    ''' <summary>
    ''' CV value value for discards (fleet x group)
    ''' </summary>
    Public CVparDiscard(,) As Single
    ''' CV value value for diets (#methods x group)
    Public CVParDC(,) As Single

    ''' <summary>
    ''' Parameter limits for non-arrayed variables (2 x parameter x group)
    ''' </summary>
    Public ParLimit(,,) As Single
    ''' <summary>
    ''' Parameter limits for landings (2 x fleet x group)
    ''' </summary>
    Public ParLimitLanding(,,) As Single
    ''' <summary>
    ''' Parameter limits for discards (2 x fleet x group)
    ''' </summary>
    Public ParLimitDiscard(,,) As Single
    ''' <summary>
    ''' Parameter limits for diets (2 x fleet x group)
    ''' </summary>
    Public ParLimitDC(,,) As Single

    ''' <summary>Best fitting parameter to the last run Monte Carlo trials (eMCParam, iGrp)</summary>
    Public BestFit(,) As Single
    Public BestFitLanding(,) As Single
    Public BestFitDiscard(,) As Single
    ''' <summary>Best fitting parameter to the last run Monte Carlo trials for diets (iPred, iPrey)</summary>
    Public BestFitDiets(,) As Single
    Public RunsSinceLastWithLowerSS As Integer = 0

    ''' <summary>Original Ecopath parameters before trials were run (trialparam x group)</summary>
    ''' <remarks>This array holds the same data as PMean, and is obsolete</remarks>
    Private startValues(,) As Single
    ''' <summary>Original Ecopath parameters before trials were run (Pred x Prey)</summary>
    Private m_startValuesDiets(,) As Single

    'Private orgVul(,) As Single

    Private m_rand As Random

    ''' <summary>
    ''' Flag (x eMSParam) stating if a given variable can be pertubed at all.
    ''' </summary>
    Private m_isEnabled() As Boolean

    ''' <summary>
    ''' Flag (x group, eMCParam) stating if a given group value can be perturbed by Monte Carlo.
    ''' </summary>
    ''' <remarks>
    ''' The logic populating this array is a duplication of cMonteCarloManager.ToMCStatus. At the time of 
    ''' writing (end of May 2018) the Monte Carlo Manager is more thorough. This duplication needs to be 
    ''' resolved. Ideally, the better logic of cMonteCarloManager should be implemented in cEcosimMonteCarlo,
    ''' and make cMonteCarlo read this information to populate the user interface classes.
    ''' </remarks>
    Private m_isVariableItem(,) As Boolean

    Public Sub New(ByRef theCore As cCore)

        m_core = theCore

        m_ecopath = m_core.m_EcoPath
        m_ecosim = m_core.m_EcoSim
        m_epdata = m_core.m_EcoPathData
        m_esdata = m_core.m_EcoSimData
        m_tsdata = m_core.m_TSData
        'data from Ecosim
        m_stanza = m_ecosim.m_stanza
        m_tracerData = m_ecosim.TracerData

        Ntrials = 20 'default number of trials
        EcopathEETol = 0.0005 '0.05%

        m_rand = New Random(CInt(Date.Now.Ticks Mod Integer.MaxValue))

        ' Set default
        Me.ResultWriter = New cMonteCarloResultsWriterOneFile(Me, Me.m_core)

    End Sub

    Public Sub initRandomSequence(seed As Integer)
        m_rand = New Random(seed)
    End Sub

    Public Function SampleGamma(ByVal Alpha As Single, ByVal Theta As Single) As Double
        Dim n As Double = Math.Truncate(Alpha)
        Dim delta As Double = Alpha - n
        Dim xi As Double = 0
        Dim eta As Double
        Dim part1 As Double = 0
        Dim U As Double
        Dim V As Double
        Dim W As Double

        If (n > 0) Then
            For k As Integer = 1 To CInt(n)
                part1 = part1 + Math.Log(m_rand.NextDouble)
            Next
        End If

        If (delta > 0) Then
            Do
                U = m_rand.NextDouble
                V = m_rand.NextDouble
                W = m_rand.NextDouble

                If (U <= (Math.E / (Math.E + delta))) Then
                    xi = V ^ (1 / delta)
                    eta = W * xi ^ (delta - 1)
                Else
                    xi = 1 - Math.Log(V)
                    eta = W * Math.Exp(-xi)
                End If
            Loop Until eta <= (xi ^ (delta - 1) * Math.Exp(-xi))
        End If

        Return ((xi - part1) * Theta)

    End Function

    Public Function Init() As Boolean

        Try
            'Used to debug Fpenalty
            'Debug.Assert(False, "Include F Penalty has been set for debugging.")
            'IncludeFpenalty = True

            'set if a parameter can be varied
            'redimVariables() needs m_isVariable(group,parameter) to be set before it is called

            Me.maxEcopathTries = MAX_ECOPATH_TRIES
            Me.setIsVariable()

            Me.redimVariables()
            m_pluginmanager = Me.m_core.PluginManager

            ReDim Pmean(Me.NumParams(), m_core.nGroups)
            ReDim PMeanLanding(m_core.nFleets, m_core.nGroups)
            ReDim PMeanDiscard(m_core.nFleets, m_core.nGroups)
            ReDim PMeanDC(m_core.nGroups, m_core.nGroups)
            ReDim startValues(Me.NumParams(), m_epdata.NumGroups)
            ReDim m_startValuesDiets(m_epdata.NumGroups, m_epdata.NumGroups)
            ReDim BestFit(Me.NumParams(), m_core.nGroups)
            ReDim BestFitLanding(m_core.nFleets, m_core.nGroups)
            ReDim BestFitDiscard(m_core.nFleets, m_core.nGroups)
            ReDim BestFitDiets(m_epdata.NumGroups, m_epdata.NumGroups)
            ' ReDim orgVul(m_core.nGroups, m_core.nGroups)

            For igrp As Integer = 1 To m_core.nGroups
                Pmean(eMCParams.Biomass, igrp) = m_epdata.B(igrp)
                Pmean(eMCParams.PB, igrp) = m_epdata.PB(igrp)
                Pmean(eMCParams.QB, igrp) = m_epdata.QB(igrp)
                Pmean(eMCParams.EE, igrp) = m_epdata.EE(igrp)
                Pmean(eMCParams.BA, igrp) = m_epdata.BA(igrp)
                Pmean(eMCParams.BaBi, igrp) = m_epdata.BaBi(igrp)
                Pmean(eMCParams.Vulnerability, igrp) = m_esdata.VulnerabilityPredator(igrp)
                Pmean(eMCParams.OtherMort, igrp) = m_epdata.OtherMortinput(igrp)

                For iFleet As Integer = 1 To Me.m_core.nFleets
                    PMeanLanding(iFleet, igrp) = m_epdata.Landing(iFleet, igrp)
                    PMeanDiscard(iFleet, igrp) = m_epdata.Discard(iFleet, igrp)
                Next

                For iPrey As Integer = 0 To m_core.nGroups
                    PMeanDC(igrp, iPrey) = m_epdata.DC(igrp, iPrey)
                Next
            Next
            CalculateUpperLowerLimits(False)

            ' Fire plug-in point
            If Me.m_pluginmanager IsNot Nothing Then
                Try
                    Me.m_core.m_SearchData.SearchMode = eSearchModes.MonteCarlo
                    Me.m_pluginmanager.SearchInitialized(Me.m_core.m_SearchData)
                    Me.m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch
                    Me.m_pluginmanager.MontCarloInitialized(Me)
                Catch ex As Exception
                    cLog.Write(ex, "cEcosimMonteCarlo::Init")
                End Try
            End If

            Return True
        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
            Throw New ApplicationException(Me.ToString & ".Run", ex)
        End Try

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="IMonteCarloResultsWriter"/> to use for writing 
    ''' results to drive. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ResultWriter As IMonteCarloResultsWriter = Nothing

    ''' <summary>
    ''' Set the isVariable(group,parameter) boolean flag
    ''' </summary>
    ''' <remarks>Can the MonteCarlo vary an Ecopath parameter </remarks>
    Private Sub setIsVariable()

        ReDim m_isEnabled(Me.NumParams())
        Me.m_isEnabled(eMCParams.Biomass) = True
        Me.m_isEnabled(eMCParams.BA) = True
        Me.m_isEnabled(eMCParams.BaBi) = True
        Me.m_isEnabled(eMCParams.PB) = True
        Me.m_isEnabled(eMCParams.QB) = True
        Me.m_isEnabled(eMCParams.EE) = True
        Me.m_isEnabled(eMCParams.Landings) = False
        Me.m_isEnabled(eMCParams.Discards) = False
        Me.m_isEnabled(eMCParams.Diets) = False

        ReDim m_isVariableItem(m_core.nGroups, Me.NumParams())

        For iGrp As Integer = 1 To m_core.nGroups

            Me.m_isVariableItem(iGrp, eMCParams.Biomass) = Not (Me.m_ecopath.missing(iGrp, 1) Or Me.isStanzaGroupVariable(iGrp, eMCParams.Biomass))
            Me.m_isVariableItem(iGrp, eMCParams.BA) = (Me.m_epdata.BAInput(iGrp) <> 0) ' Can vary BA on estimated biomasses!
            Me.m_isVariableItem(iGrp, eMCParams.BaBi) = (Me.m_epdata.BaBi(iGrp) <> 0)
            Me.m_isVariableItem(iGrp, eMCParams.PB) = Not Me.m_ecopath.missing(iGrp, 2)
            Me.m_isVariableItem(iGrp, eMCParams.QB) = Not (Me.m_ecopath.missing(iGrp, 3) Or Me.isStanzaGroupVariable(iGrp, eMCParams.QB))
            Me.m_isVariableItem(iGrp, eMCParams.EE) = (Not Me.m_ecopath.missing(iGrp, 4)) And (iGrp <= Me.m_epdata.NumLiving)

        Next

        ' Not necessary to remember isVariable for Catches, just check PMeanLandings / PMeanDiscards when enabled
        ' Not necessary to remember isVariable for Diets, always perturb diets when enabled

    End Sub

    Public Sub Clear()

        Me.Pmean = Nothing : Me.PMeanLanding = Nothing : Me.PMeanDiscard = Nothing
        Me.startValues = Nothing
        Me.BestFit = Nothing : Me.BestFitLanding = Nothing : Me.BestFitDiscard = Nothing
        Me.BestFitDiets = Nothing
        Me.ParLimit = Nothing
        Me.ParLimitLanding = Nothing
        Me.ParLimitDiscard = Nothing
        Me.PMeanLanding = Nothing
        Me.PMeanDiscard = Nothing
        Me.CVpar = Nothing
        Me.CVparLanding = Nothing
        Me.CVparDiscard = Nothing

        'Me.orgVul = Nothing

    End Sub

    Private Function PedigreeVarToMCIndex(ByVal vn As eVarNameFlags) As eMCParams

        Select Case vn
            Case eVarNameFlags.BiomassAreaInput : Return eMCParams.Biomass
            Case eVarNameFlags.PBInput : Return eMCParams.PB
            Case eVarNameFlags.QBInput : Return eMCParams.QB
            Case eVarNameFlags.DietComp : Return eMCParams.Diets
            Case eVarNameFlags.TCatchInput
                Debug.Assert(False)
        End Select

        Console.WriteLine(Me.ToString & ".PedigreeVarToMCIndex() Invalid VarName '" & vn.ToString & "'")
        Return eMCParams.NotSet

    End Function

    ''' <summary>
    ''' Load CV values for a given variable from Pedigree.
    ''' </summary>
    ''' <param name="varname"></param>
    Friend Function LoadFromPedigree(varname As eVarNameFlags) As Boolean

        Dim opt As Integer ' Opt = CV
        Dim man As cPedigreeManager = Nothing
        Dim parm As eMCParams = eMCParams.NotSet
        Dim iVar As Integer = Me.m_core.PedigreeVariableIndex(varname)

        If (iVar <= 0) Then Return False

        ' For all groups
        For i As Integer = 1 To Me.m_epdata.NumGroups
            ' Read assigned pedigree level for a group (was 'Opt = ReadPedigreeFromDatabase(Par)')
            opt = Me.m_epdata.PedigreeEcopathGroupCV(i, iVar)
            If opt > 0 Then ' Non-estimated level
                Try

                    Select Case varname

                        Case eVarNameFlags.BiomassAreaInput,
                             eVarNameFlags.PBInput,
                             eVarNameFlags.QBInput,
                             eVarNameFlags.DietComp
                            parm = Me.PedigreeVarToMCIndex(varname)
                            CVpar(parm, i) = opt / 100.0! / 2.0!
                            Me.CalculateUpperLowerLimits(False, parm)

                        Case eVarNameFlags.TCatchInput
                            For iFleet As Integer = 1 To Me.m_core.nFleets
                                CVparLanding(iFleet, i) = opt / 100.0! / 2.0!
                                CVparDiscard(iFleet, i) = opt / 100.0! / 2.0!
                            Next

                            Me.CalculateUpperLowerLimits(False, eMCParams.Landings)
                            Me.CalculateUpperLowerLimits(False, eMCParams.Discards)
                    End Select

                Catch ex As Exception
                    cLog.Write(ex, "cEcosimMonteCarlo::LoadFromPedigree(" & varname.ToString & ")")
                    Return False
                End Try
            End If
        Next
        Return True

    End Function

    Public Sub initForRun()

        Try

            StopTrial = False
            m_esdata.SS = 0

            'This gives the same sequence of random numbers 
            'Used for debugging
            'm_rand = New Random(666)

            ReDim isCrashed(m_core.nGroups)
            ReDim isExploded(m_core.nGroups)

            m_ecosim.Init(True)

            m_core.m_EcoSimData.bTimestepOutput = True
            m_ecosim.TimeStepDelegate = Nothing

            'jb remove vulnerabilities until there is a proper interface
            'if it is left in place it causes problem because it changes the vulnerabilities
            ''Set the all vulnerabilities to a predator to the max across all prey
            ''This is the same as setting all the columns in the Vulnerabiltiy matrix to the same value
            'For iPred As Integer = 1 To m_core.nGroups
            '    Dim vul As Single = 0
            '    For iPrey As Integer = 1 To m_core.nGroups
            '        'jb 18-Nov-2011 Changed from first non zero vulnerability 
            '        'To max vulnerability across all prey for this pred  
            '        vul = Math.Max(vul, m_core.m_EcoSimData.VulMult(iPrey, iPred))
            '        'If m_core.m_EcoSimData.VulMult(iPrey, iPred) > 0 Then vul = m_core.m_EcoSimData.VulMult(iPrey, iPred) : Exit For
            '    Next
            '    'Max vulnerability to this predator
            '    m_core.m_EcoSimData.VulnerabilityPredator(iPred) = vul
            'Next

            'run ecosim to get the fit (SS) of the ref data to the current ecopath parameters
            m_ecosim.Run()

            For iGrp As Integer = 1 To m_core.nGroups
                Pmean(eMCParams.Biomass, iGrp) = m_epdata.B(iGrp)
                Pmean(eMCParams.PB, iGrp) = m_epdata.PB(iGrp)
                Pmean(eMCParams.EE, iGrp) = m_epdata.EE(iGrp)
                Pmean(eMCParams.BA, iGrp) = m_epdata.BA(iGrp)
                Pmean(eMCParams.BaBi, iGrp) = m_epdata.BaBi(iGrp)
                Pmean(eMCParams.QB, iGrp) = m_epdata.QB(iGrp)
                Pmean(eMCParams.OtherMort, iGrp) = m_epdata.OtherMortinput(iGrp)

                'Pmean(eMCParams.Vulnerability, iGrp) = m_esdata.VulnerabilityPredator(iGrp)

                For iFleet As Integer = 1 To Me.m_core.nFleets
                    PMeanLanding(iFleet, iGrp) = m_epdata.Landing(iFleet, iGrp)
                    PMeanDiscard(iFleet, iGrp) = m_epdata.Discard(iFleet, iGrp)
                Next

                For iPrey As Integer = 0 To m_core.nGroups ' JS: why 0?
                    PMeanDC(iGrp, iPrey) = m_epdata.DC(iGrp, iPrey)
                Next
            Next

            'make a copy for the best fitting data 
            Array.Copy(Pmean, BestFit, Pmean.Length)
            Array.Copy(PMeanDC, BestFitDiets, PMeanDC.Length)
            'make a copy of the original values so the user can restore the values
            Array.Copy(Pmean, startValues, Pmean.Length)
            ' MP Apr 2016 adding diets
            Array.Copy(PMeanDC, m_startValuesDiets, PMeanDC.Length)
            'vulnerabilities 
            'Array.Copy(m_core.m_EcoSimData.VulMult, Me.orgVul, m_core.m_EcoSimData.VulMult.Length)


            'Array.Copy(m_epdata.DC, Me.m_startValuesDiets, m_epdata.DC.Length)

            'jb Mar-24-2011 Do NOT reset Upper and Lower Parameter Limits 
            'they may have been edited by a user and this will overwrite the edits with defaults
            'CalculateUpperLowerLimits(True)

#If 0 Then

            Dim FromEcobio As Boolean = True
            If FromEcobio Then
                'Using sw As StreamWriter = New StreamWriter("c:\LME\UpperLowerLimits.csv", True)  'true makes it append
                '    sw.WriteLine(m_core.m_EwEModelName & ", " & Date.Now.ToString)
                For i As Integer = 1 To m_core.nLivingGroups
                    'sw.WriteLine(i.ToString & "," & _
                    '             ParLimit(0, 1, i).ToString & "," & _
                    '             ParLimit(1, 1, i).ToString & _
                    '             "," & ParLimit(0, 4, i).ToString & _
                    '             "," & ParLimit(1, 4, i).ToString & _
                    '             "," & ParLimit(0, 6, i).ToString & "," _
                    '             & ParLimit(1, 6, i).ToString)
                Next
                'sw.Close()
                'End Using
            End If
#End If
            SSorg = m_esdata.SS

            'make sure the ecopath type of run is correct for the monte carlo runs
            m_ecopath.ParameterEstimationType = eEstimateParameterFor.Sensitivity

            m_ecosim.TimeStepDelegate = EcosimTimeStep

            If Me.m_pluginmanager IsNot Nothing Then
                Try
                    Me.m_pluginmanager.MonteCarloRunInitialized()
                Catch ex As Exception
                    cLog.Write(ex, "cEcosimMonteCarlo::InitForRun")
                End Try
            End If

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
            Throw New ApplicationException(Me.ToString & ".initForRun()", ex)
        End Try

    End Sub

    Public ReadOnly Property IsBestFit As Boolean
        Get
            Return Me.m_bIsBestFit
        End Get
    End Property

    Public Property IsEnabled(var As eMCParams) As Boolean
        Get
            If (var = eMCParams.NotSet) Then Return False
            Return Me.m_isEnabled(var)
        End Get
        Set(value As Boolean)
            If (var = eMCParams.NotSet) Then Return
            Me.m_isEnabled(var) = value
        End Set
    End Property

    Public Function IsVariable(item As Integer, var As eMCParams) As Boolean
        Select Case var
            Case eMCParams.NotSet
                Return False
            Case eMCParams.Diets, eMCParams.Landings, eMCParams.Discards
                Return Me.m_isEnabled(var)
            Case Else
                Return Me.m_isEnabled(var) And Me.m_isVariableItem(item, var)
        End Select
    End Function

    Public Property Ntrials As Integer
    Public Property StopTrial As Boolean
    Public Property RetainBiomass As Boolean

    ''' <summary>
    ''' Flag, states whether to include Stock Reduction Analysis (SRA) for groups with forced catches
    ''' </summary>
    Public Property IncludeFpenalty As Boolean

    ''' <summary>
    ''' F/M ratio for SRA 
    ''' </summary>
    Public Property FMratioForSRA As Single = 1

    Public Property maxEcopathTries As Integer = MAX_ECOPATH_TRIES

    ''' <summary>
    ''' Get/set whether output should be saved to file automatically.
    ''' </summary>
    Public Property SaveOutput As Boolean
        Get
            Return Me.m_core.Autosave(eAutosaveTypes.MonteCarlo)
        End Get
        Set(value As Boolean)
            Me.m_core.Autosave(eAutosaveTypes.MonteCarlo) = value
        End Set
    End Property

    Public Sub Run(ByVal ob As Object)

        Dim iter As Integer 'number of ecopath interation to find new pararameters for each trial
        Dim Fpenalty As Single
        Dim bFirstRun As Boolean = True
        'Dim NtrialsPerThread As Integer
        'Dim nThreads As Integer

        'Dim MCthreadList As New List(Of cMonteCarloThread)
        'Dim MCthread As cMonteCarloThread
        Dim bForcedCatches(Me.m_epdata.NumGroups) As Boolean
        For its As Integer = 1 To m_tsdata.nTimeSeries
            If m_tsdata.TimeSeriesType(its) = eTimeSeriesType.CatchesForcing Then
                bForcedCatches(m_tsdata.iPool(its)) = True
            End If
        Next

        System.Console.WriteLine("----------Starting Monte Carlo----------")
        Try
            initForRun()

            If (Me.ResultWriter IsNot Nothing) Then
                Me.ResultWriter.Init()
            End If

            ' Fire plug-in point
            If Me.m_pluginmanager IsNot Nothing Then
                Try
                    Me.m_pluginmanager.SearchIterationsStarting()
                Catch ex As Exception
                    cLog.Write(ex, "cEcosimMonteCarlo::Run(SearchIterationsStarting)")
                End Try
            End If

            'nThreads = System.Environment.ProcessorCount
            'nThreads = 1
            'NtrialsPerThread = (Ntrials + nThreads - 1) \ nThreads
            'initThreads(MCthreadList, nThreads)

            'tell ecopath to run in silent mode
            'this does not turn off the core's messages just ecopath
            m_ecopath.suppressMessages = True

            'Ecosim was run in initForRun()
            'm_esdata.SS is the fit of the currently loaded reference data
            If Me.isTimeSeriesLoaded Then
                SSBestFit = m_esdata.SS
            Else
                SSBestFit = 0
            End If

            For m_iTrial = 1 To Ntrials 'PerThread

                If StopTrial = True Then Exit For

                'number of ecopath iterations to find new parameters
                iter = 0
                RunsSinceLastWithLowerSS += 1
                Me.m_bIsBestFit = False

                If BalanceEcopathWithNewPars(iter, maxEcopathTries) Then

                    Me.BalancedEcopathModel(m_iTrial, iter)

                    m_ecosim.Init(True)

                    'the ecosim time step delegate was set before the loop
                    m_ecosim.Run()

                    Me.m_bIsBestFit = Me.isTimeSeriesLoaded() And (m_esdata.SS < SSBestFit)

                    If Me.m_pluginmanager IsNot Nothing Then
                        Try
                            Me.m_pluginmanager.MonteCarloEcosimRunCompleted()
                        Catch ex As Exception
                            cLog.Write(ex, "cEcosimMonteCarlo::Run(" & m_iTrial & ")")
                        End Try
                    End If

                    'xxxxxxxxxxxxxxxxxxxx Below is for global Nereus model, June 2013 xxxxxxxxxxxxxxxxxx
                    'Calculate penalty for being away from reasonable fishing mortalityIsVariable
                    Fpenalty = Me.getFPenalty(bFirstRun, bForcedCatches)
                    m_esdata.SS += Fpenalty
                    'Debug.Print(Me.m_esdata.SS & " = " & Me.m_esdata.SS - Fpenalty & " + " & Fpenalty)
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    'Only keep the best fit if there is time series loaded
                    If Me.isTimeSeriesLoaded() And (m_esdata.SS < SSBestFit) Then
                        RunsSinceLastWithLowerSS = 0
                        'SSBestFit = MCthread.ESdata.SS
                        SSBestFit = m_esdata.SS
                        Console.WriteLine("Total trials: " & m_iTrial.ToString & ", " & SSBestFit.ToString & ", to fit last Ecopath: " & iter.ToString) '& ", total: " & Itertot.ToString)

                        CheckWhoIsCrashed()
                        'keep the best fits for applying later
                        For igrp As Integer = 1 To m_core.nGroups
                            BestFit(eMCParams.Biomass, igrp) = m_epdata.B(igrp)
                            BestFit(eMCParams.QB, igrp) = m_epdata.QB(igrp)
                            BestFit(eMCParams.PB, igrp) = m_epdata.PB(igrp)
                            BestFit(eMCParams.EE, igrp) = m_epdata.EE(igrp)
                            BestFit(eMCParams.BA, igrp) = m_epdata.BA(igrp)
                            BestFit(eMCParams.BaBi, igrp) = m_epdata.BaBi(igrp)
                            '  BestFit(eMCParams.Vulnerability, igrp) = m_esdata.VulnerabilityPredator(igrp)

                            For iFlt As Integer = 1 To Me.m_epdata.NumFleet
                                BestFitLanding(iFlt, igrp) = m_epdata.Landing(iFlt, igrp)
                                BestFitDiscard(iFlt, igrp) = m_epdata.Discard(iFlt, igrp)
                            Next

                            For iPrey As Integer = 1 To m_core.nGroups
                                BestFitDiets(igrp, iPrey) = m_epdata.DC(igrp, iPrey)
                            Next
                        Next

                        If RetainBiomass Then
                            Array.Copy(BestFit, Pmean, BestFit.Length)
                            'VC 2008 don't want it to stop just as it found a better fit so:
                            m_iTrial = Math.Min(m_iTrial, CInt(0.9 * Ntrials))

                        End If 'bRetainBiomass
                    End If ' m_esdata.SS < SSBestFit

                    If (Me.ResultWriter IsNot Nothing) Then
                        ' Only save when an alternative balanced model was found
                        Me.ResultWriter.Save(m_iTrial)
                    End If

                End If 'iter < maxEcopathTries 

                TrialProgress(m_iTrial, iter)
                EcopathIterationsProgress(iter)

                ' Fire plug-in point
                If Me.m_pluginmanager IsNot Nothing Then
                    Try
                        Me.m_pluginmanager.PostRunSearchResults(Me.m_core.m_SearchData)
                    Catch ex As Exception
                        cLog.Write(ex, "cEcosimMonteCarlo::Run(" & m_iTrial & ")")
                    End Try
                End If
                If RunsSinceLastWithLowerSS > 2000 Then Exit For
            Next m_iTrial

            'restore ecopath back to its original state
            Me.restoreOriginalState()

            Me.CompletedCallback()
            If Me.m_pluginmanager IsNot Nothing Then
                Try
                    Me.m_pluginmanager.SearchCompleted(Me.m_core.m_SearchData)
                Catch ex As Exception
                    cLog.Write(ex, "cEcosimMonteCarlo::Run SearchCompleted")
                End Try
            End If

            Me.m_ecopath.suppressMessages = False

        Catch ex As Exception
            cLog.Write(ex, "cEcosimMonteCarlo::Run(" & m_iTrial & ")")
            Debug.Assert(False, ex.StackTrace)
            m_ecopath.suppressMessages = False
        End Try

        If (Me.ResultWriter IsNot Nothing) Then
            Me.ResultWriter.Finish()
        End If

        If (Me.m_pluginmanager IsNot Nothing) Then
            Try
                Me.m_pluginmanager.MontCarloRunCompleted()
            Catch ex As Exception
                cLog.Write(ex, "cEcosimMonteCarlo::Run MontCarloRunCompleted")
            End Try
        End If

    End Sub

    Private Sub BalancedEcopathModel(ByVal iTrial As Integer, ByVal iter As Integer)
        If Me.m_pluginmanager IsNot Nothing Then
            Try
                Me.m_pluginmanager.MonteCarloBalancedEcopathModel(iTrial, iter)
            Catch ex As Exception
                cLog.Write(ex, "cEcosimMonteCarlo::Run BalancedEcopathModel(" & iTrial & ", " & iter & ")")
            End Try
        End If
    End Sub

    Public Sub setDefaults()
        Me.EcopathEETol = EE_TOL
    End Sub


    Private Function isTimeSeriesLoaded() As Boolean
        'Number of applied time series
        Return Me.m_tsdata.NdatType > 0
    End Function

    ''' <summary>
    ''' Calculate penalty for being away from reasonable fishing mortality
    ''' </summary>
    ''' <param name="bForcedCatches"></param>
    ''' <remarks></remarks>
    Private Function getFPenalty(ByRef bFirstRun As Boolean, bForcedCatches() As Boolean) As Single
        'Used for global Nereus model, June 2013
        Dim Fpenalty As Single

        If Me.IncludeFpenalty Then
            'If Fpenalty = 0 Then FirstRun = True
            Fpenalty = 0
            Dim sStr As String = ""
            For ii As Integer = 1 To Me.m_epdata.NumGroups
                If (bForcedCatches(ii)) Then
                    Dim lasttimestep As Integer = m_esdata.NTimes
                    Dim NatMort As Single = Me.m_epdata.M0(ii) + Me.m_epdata.M2(ii)
                    Dim SScont As Single = (Me.m_esdata.FishRateNo(ii, lasttimestep) - Me.FMratioForSRA * NatMort)
                    Fpenalty += CSng(100 * SScont ^ 2)
                    sStr += ii & " " & SScont & ","
                End If
            Next

            If bFirstRun Then
                SSBestFit = SSBestFit + Fpenalty
                bFirstRun = False
            End If

            System.Console.WriteLine("SS = " + m_esdata.SS.ToString + ", F Penalty = " + Fpenalty.ToString + ", SS + Fpenalty = " + (m_esdata.SS + Fpenalty).ToString)
        End If

        Return Fpenalty
    End Function

    ''' <summary>
    ''' Restore Ecopath to its original state
    ''' </summary>
    ''' <remarks>The Monte Carlo changed the basic input data of Ecopath. This will set it back to the state it was in when the Monte Carlo was run.</remarks>
    Public Sub restoreOriginalState()
        Dim bSuccess As Boolean

        Try

            'Set Ecopath inputs back to original values
            'VC Oct 02. below was setting, b, pb, ee, ba, but it needs to set input parameters,so I've changed this
            For i As Integer = 1 To Me.m_epdata.NumLiving
                If m_epdata.Binput(i) > 0 Then m_epdata.Binput(i) = startValues(eMCParams.Biomass, i)
                If m_epdata.PBinput(i) > 0 Then m_epdata.PBinput(i) = startValues(eMCParams.PB, i)
                If m_epdata.QBinput(i) > 0 Then m_epdata.QBinput(i) = startValues(eMCParams.QB, i)
                If m_epdata.EEinput(i) > 0 Then m_epdata.EEinput(i) = startValues(eMCParams.EE, i)
                If m_epdata.OtherMortinput(i) > 0 Then m_epdata.OtherMortinput(i) = startValues(eMCParams.OtherMort, i)

                m_epdata.BA(i) = startValues(eMCParams.BA, i)
                m_epdata.BaBi(i) = startValues(eMCParams.BaBi, i)
                ' m_esdata.VulnerabilityPredator(i) = startValues(eMCParams.Vulnerability, i)
            Next

            For iGrp As Integer = 1 To m_epdata.NumGroups
                For iFlt As Integer = 1 To m_epdata.NumFleet
                    Me.m_epdata.Landing(iFlt, iGrp) = PMeanLanding(iFlt, iGrp)
                    Me.m_epdata.Discard(iFlt, iGrp) = PMeanDiscard(iFlt, iGrp)
                Next
                For iPrey As Integer = 0 To m_epdata.NumGroups
                    Me.m_epdata.DC(iGrp, iPrey) = m_startValuesDiets(iGrp, iPrey)
                Next
            Next

            'set vulnerabilities back 
            'Array.Copy(Me.orgVul, m_core.m_EcoSimData.VulMult, m_core.m_EcoSimData.VulMult.Length)

            'copy the data from the input parameters into the modeling parameters
            Me.m_epdata.CopyInputToModelArrays()

            'run Ecopath with the original values to reset computed variables
            bSuccess = Me.m_ecopath.Run()

            'init stanza groups back to the original values
            Me.m_ecosim.InitStanza()

            'Me.m_ecosim.Init(True)

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            bSuccess = False
        End Try

        If Not bSuccess Then
            Me.m_core.Messages.AddMessage(New cMessage(My.Resources.CoreMessages.MONTECARLO_RESTORE_FAILED, eMessageType.ErrorEncountered, eCoreComponentType.EcoSimMonteCarlo, eMessageImportance.Warning))
        End If

    End Sub


    Private Sub TrialProgress(ByVal iTrial As Integer, ByVal iEcopathIterations As Integer)

        Try
            Me.nTrialIterations = iTrial
            Me.nEcopathIterations = iEcopathIterations
            Me.SSCurrent = Me.m_core.m_EcoSimData.SS
            If dlgTrialStepHandler IsNot Nothing Then
                Me.dlgTrialStepHandler()
            End If
        Catch ex As Exception
            'Bogus Dude.....the interface has thrown an error 
            'just keep ploughing on
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub EcopathIterationsProgress(ByVal iEcopathIterations As Integer)

        Try
            Me.nEcopathIterations = iEcopathIterations
            If dlgEcopathIterationHandler IsNot Nothing Then
                dlgEcopathIterationHandler.Invoke()
            End If
        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub CompletedCallback()
        Try
            If dlgMonteCarloCompletedHandler IsNot Nothing Then
                Me.dlgMonteCarloCompletedHandler.Invoke()
            End If
        Catch ex As Exception
            Debug.Assert(False, "Monte Carlo CompletedCallback Exception: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

    ''' <summary>
    ''' Wrapper around <see cref="cEcosimMonteCarlo.BalanceEcopathWithNewPars">BalanceEcopathWithNewPars</see>  
    ''' so the MonteCarloManager can expose this functionality via <see cref="cMonteCarloManager.selectNewEcopathParameters">selectNewEcopathParameters()</see>
    ''' </summary>
    ''' <param name="MaxIters">Maximum number of tries to find a balanced Ecopath Model.</param>
    ''' <returns>True if successful. False otherwise.</returns>
    ''' <remarks></remarks>
    Friend Function selectNewEcopathParameters(Optional MaxIters As Integer = MAX_ECOPATH_TRIES) As Boolean
        Try
            Dim nIters As Integer
            If BalanceEcopathWithNewPars(nIters, MaxIters) Then
                ''Used for debugging CEFAS MSE Plugin
                'If MaxIters > 1 Then
                '    System.Console.WriteLine("Balanced model in " + nIters.ToString)
                'End If
                Return True
            End If

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".selectNewEcopathParameters() Exception: " & ex.Message)
        End Try

        'Failed to find a balanced set of parameters within MaxIters
        'or
        'An error has been thrown some place along the line
        Return False

    End Function


    Private Sub dumpEstimatedParameters()

        System.Console.WriteLine("-------------Start Parameters Estimated by Ecopath----------------")
        For igrp As Integer = 1 To m_core.nLivingGroups
            For iPar As Integer = 1 To 4
                If m_ecopath.missing(igrp, iPar) = True Then
                    'Estimated by Ecopath
                    System.Console.WriteLine(Me.m_epdata.GroupName(igrp) + ", Index =  " + igrp.ToString + ", Parameter = " + iPar.ToString)
                End If
            Next
        Next
        System.Console.WriteLine("-------------End Parameters Estimated by Ecopath------------------")

    End Sub

    Private Function BalanceEcopathWithNewPars(ByRef iter As Integer,
                                               ByVal maxEcopathIterations As Integer) As Boolean
        'EwE5 StartEcosimWithNewPars(ByVal Pstartup(,) As Single, ByVal CVpar(,) As Single, ByVal iter As Long)
        Dim igrp As Integer
        Dim bEcopathNeedsBalancing As Boolean

        Try
            'for debugging which parameters are being estimated
            'dumpEstimatedParameters()

            bEcopathNeedsBalancing = True
            Do While bEcopathNeedsBalancing
                iter = iter + 1
                Me.m_epdata.CopyInputToModelArrays() 'MakeUnknownUnknown())

                For igrp = 1 To m_core.nLivingGroups

                    If Me.IsVariable(igrp, eMCParams.Biomass) Then
                        m_epdata.B(igrp) = ChooseFeasiblePar(eMCParams.Biomass,
                                                             Me.Pmean(eMCParams.Biomass, igrp),
                                                             Me.CVpar(eMCParams.Biomass, igrp),
                                                             Me.ParLimit(0, eMCParams.Biomass, igrp),
                                                             Me.ParLimit(1, eMCParams.Biomass, igrp))
                    End If

                    If Me.IsVariable(igrp, eMCParams.BA) Then
                        m_epdata.BA(igrp) = ChooseFeasibleBA(Pmean(eMCParams.Biomass, igrp), ' Must operate on original estimated biomass, not input (may be missing)
                                                             Me.Pmean(eMCParams.BA, igrp),
                                                             Me.CVpar(eMCParams.BA, igrp),
                                                             Me.ParLimit(0, eMCParams.BA, igrp),
                         ParLimit(1, eMCParams.BA, igrp))
                    End If

                    If Me.IsVariable(igrp, eMCParams.BaBi) Then
                        m_epdata.BaBi(igrp) = ChooseFeasiblePar(eMCParams.BaBi,
                                                             Me.Pmean(eMCParams.BaBi, igrp),
                                                             Me.CVpar(eMCParams.BaBi, igrp),
                                                             Me.ParLimit(0, eMCParams.BaBi, igrp),
                                                             Me.ParLimit(1, eMCParams.BaBi, igrp))
                    End If

                    If Me.IsVariable(igrp, eMCParams.PB) Then
                        m_epdata.PB(igrp) = ChooseFeasiblePar(eMCParams.PB,
                                                              Pmean(eMCParams.PB, igrp),
                                                              CVpar(eMCParams.PB, igrp),
                                                              ParLimit(0, eMCParams.PB, igrp),
                                                              ParLimit(1, eMCParams.PB, igrp))
                    End If

                    If Me.IsVariable(igrp, eMCParams.QB) Then
                        m_epdata.QB(igrp) = ChooseFeasiblePar(eMCParams.QB,
                                                              Me.Pmean(eMCParams.QB, igrp),
                                                              Me.CVpar(eMCParams.QB, igrp),
                                                              Me.ParLimit(0, eMCParams.QB, igrp),
                                                              Me.ParLimit(1, eMCParams.QB, igrp))
                    End If

                    If Me.IsVariable(igrp, eMCParams.EE) Then
                        m_epdata.EE(igrp) = ChooseFeasiblePar(eMCParams.EE,
                                                              Me.Pmean(eMCParams.EE, igrp),
                                                              Me.CVpar(eMCParams.EE, igrp),
                                                              Me.ParLimit(0, eMCParams.EE, igrp),
                                                              Me.ParLimit(1, eMCParams.EE, igrp))
                    End If

                    If Me.IsEnabled(eMCParams.Landings) Then
                        For iflt As Integer = 1 To m_epdata.NumFleet
                            If (Me.PMeanLanding(iflt, igrp) > 0) Then
                                Me.m_epdata.Landing(iflt, igrp) = ChooseFeasiblePar(eMCParams.Landings,
                                                                                    Me.PMeanLanding(iflt, igrp),
                                                                                    Me.CVparLanding(iflt, igrp),
                                                                                    Me.ParLimitLanding(0, iflt, igrp),
                                                                                    Me.ParLimitLanding(1, iflt, igrp))
                            End If
                        Next

                    End If

                    If Me.IsEnabled(eMCParams.Discards) Then
                        For iflt As Integer = 1 To m_epdata.NumFleet
                            If (Me.PMeanDiscard(iflt, igrp) > 0) Then
                                Me.m_epdata.Discard(iflt, igrp) = ChooseFeasiblePar(eMCParams.Discards,
                                                                                    Me.PMeanDiscard(iflt, igrp),
                                                                                    Me.CVparDiscard(iflt, igrp),
                                                                                    Me.ParLimitDiscard(0, iflt, igrp),
                                                                                    Me.ParLimitDiscard(1, iflt, igrp))
                            End If
                        Next
                    End If

                    If Me.IsEnabled(eMCParams.Diets) Then
                        Select Case Me.DietSamplingMethod
                            Case eMCDietSamplingMethod.Dirichlets
                                ChooseFeasibleDiet(PMeanDC, CVParDC(eMCDietSamplingMethod.Dirichlets, igrp), igrp, m_epdata.DC)
                            Case eMCDietSamplingMethod.NormalDistribution
                                For iPred As Integer = 1 To m_epdata.NumLiving
                                    If (Me.PMeanDC(iPred, igrp) > 0) Then
                                        Me.m_epdata.DC(iPred, igrp) = ChooseFeasiblePar(eMCParams.Diets,
                                                                                    Me.PMeanDC(iPred, igrp),
                                                                                    Me.CVParDC(eMCDietSamplingMethod.NormalDistribution, iPred),
                                                                                    Me.ParLimitDC(0, iPred, igrp),
                                                                                    Me.ParLimitDC(1, iPred, igrp))
                                    End If
                                Next
                            Case Else
                                Debug.Assert(False)
                        End Select
                    End If

                Next igrp

                If Me.IsEnabled(eMCParams.Diets) And (Me.DietSamplingMethod <> eMCDietSamplingMethod.Dirichlets) Then
                    Me.NormalizeDiet(Me.m_epdata.DC)
                End If

                Me.m_ecosim.InitStanza()

                'For debugging
                'dumpEcopathPars()

                'Estimate basic params
                If Me.m_ecopath.Run() Then

                    Me.m_ecopath.DetritusCalculations()

                    bEcopathNeedsBalancing = False

                    If Me.ValidateRespiration Then
                        bEcopathNeedsBalancing = bEcopathNeedsBalancing And (Me.m_epdata.Compute_M2_Resp_and_Stats(True) = False)
                    End If

                    For igrp = 1 To m_core.nGroups
                        If Me.m_epdata.EE(igrp) > 1.0 + Me.EcopathEETol Or Me.m_epdata.EE(igrp) < 0 And Me.m_epdata.EE(igrp) <> cCore.NULL_VALUE Then
                            'this loop did not balance Ecopath
                            bEcopathNeedsBalancing = True
                            Exit For
                        End If
                    Next

                Else
                    '' Failed to estimate parameters
                    'Dim status As eStatusFlags = m_ecopath.EstimationStatus
                    'Dim msg As cMessage
                    'If status = eStatusFlags.MissingParameter Then
                    '    msg = New cMessage(My.Resources.CoreMessages.MONTECARLO_ECOPATH_TOOMANYMISSING, eMessageType.TooManyMissingParameters, eCoreComponentType.EcoSim, eMessageImportance.Critical)
                    'Else
                    '    msg = New cMessage(My.Resources.CoreMessages.MONTECARLO_ECOPATH_ERROR, eMessageType.ErrorEncountered, eCoreComponentType.EcoSim, eMessageImportance.Critical)
                    'End If
                    '' m_manager.AddMessage(msg)
                    ''Return False
                End If

                'tell the interface
                'EcopathIterationsProgress(iter)

                If StopTrial = True Then Exit Do

                If iter >= maxEcopathIterations Then
                    'max number of iteration to find balanced ecopath model
                    'Exit the Do Loop
                    Exit Do
                End If

            Loop

        Catch ex As Exception
            Debug.Assert(False, ex.StackTrace)
            cLog.Write(ex)
            Throw New ApplicationException(Me.ToString & ".BalanceEcopathWithNewPars()", ex)
        End Try

        'bEcopathNeedsBalancing will be False if a balanced model was found(does not need balancing)
        'True if not balanced(the model does need balancing)
        'BalanceEcopathWithNewPars() will return True if the model was balanced, the opposite of bEcopathNeedsBalancing
        Return Not bEcopathNeedsBalancing

    End Function

    Private Sub NormalizeDiet(ByRef DietMatrix(,) As Single)
        Dim dietsum As Single
        Dim tol As Single = 0.001
        Dim bwarning As Boolean = False

        For iPred As Integer = 1 To m_epdata.NumLiving
            bwarning = False
            If m_epdata.PP(iPred) < 1 Then
                dietsum = 0
                For iPrey As Integer = 0 To m_epdata.NumGroups
                    dietsum = dietsum + DietMatrix(iPred, iPrey)
                Next
                'If dietsum <> 0 And Math.Abs(dietsum - 1) > tol Then
                If dietsum <> 0 Then
                    bwarning = True
                    For iPrey As Integer = 0 To m_epdata.NumGroups
                        DietMatrix(iPred, iPrey) = DietMatrix(iPred, iPrey) / dietsum
                    Next
                    'm_Data.DietsModified = True
                End If
            End If
        Next
        If bwarning Then
            Console.WriteLine("WARNING Normalized Diet after sampling.")
        End If
    End Sub

    ''' <summary>
    ''' Determines whether the given variable depends on other stanza.
    ''' </summary>
    ''' <param name="igrp">The igrp.</param>
    ''' <param name="varType">Type of the variable.</param>
    ''' <returns>
    ''' True if the variable depends on other life stages and thus cannot be varied.
    ''' </returns>
    Private Function isStanzaGroupVariable(igrp As Integer, varType As eMCParams) As Boolean

        'Not a multistanza group so OK to vary
        If Not m_epdata.StanzaGroup(igrp) Then Return True

        'Optimistic this group can be varied
        Dim bReturn As Boolean = True
        Select Case varType

            Case eMCParams.BA, eMCParams.BaBi, eMCParams.Landings, eMCParams.Discards, eMCParams.Diets
                ' Never variable for Stanza groups
                bReturn = False

            Case eMCParams.Biomass
                'For B and QB only the leading group can be varied
                If Not Me.m_epdata.isGroupLeadingB(igrp) Then bReturn = False

            Case eMCParams.QB
                'For B and QB only the leading group can be varied
                If Not Me.m_epdata.isGroupLeadingCB(igrp) Then bReturn = False

            Case Else
                Debug.Assert(False)

        End Select

        Return bReturn

    End Function

    Private Sub dumpEcopathPars()
        Try
            Dim strm As New System.IO.StreamWriter("EcopathPars.csv", True)
            strm.WriteLine("iter")
            For igrp As Integer = 1 To Me.m_epdata.NumGroups
                strm.WriteLine(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.m_epdata.GroupName(igrp)) + "," + Me.m_epdata.B(igrp).ToString + "," + Me.m_epdata.PB(igrp).ToString + "," + Me.m_epdata.QB(igrp).ToString + "," + Me.m_epdata.EE(igrp).ToString)
            Next
            strm.Close()
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Apply the results of the Monte Carlo trials (best fitting parameters) to the ecopath data
    ''' </summary>
    ''' <remarks>This does not update the Core's interface objects</remarks>
    Friend Sub ApplyBestFits()

        'user wants to keep the best fit parameters
        For iPred As Integer = 1 To m_core.nGroups
            If m_ecopath.missing(iPred, 1) = False Then
                m_epdata.Binput(iPred) = BestFit(eMCParams.Biomass, iPred)
                m_epdata.BHinput(iPred) = BestFit(eMCParams.Biomass, iPred) / m_epdata.Area(iPred)
            End If
            If m_ecopath.missing(iPred, 2) = False Then
                m_epdata.PBinput(iPred) = BestFit(eMCParams.PB, iPred)
            End If

            If m_ecopath.missing(iPred, 3) = False Then
                m_epdata.QBinput(iPred) = BestFit(eMCParams.QB, iPred)
            End If

            If m_ecopath.missing(iPred, 4) = False Then
                m_epdata.EEinput(iPred) = BestFit(eMCParams.EE, iPred)
            End If

            m_epdata.BA(iPred) = BestFit(eMCParams.BA, iPred)
            m_epdata.BaBi(iPred) = BestFit(eMCParams.BaBi, iPred)

            For iFleet As Integer = 1 To m_core.nFleets
                m_epdata.Landing(iFleet, iPred) = Me.BestFitLanding(iFleet, iPred)
                m_epdata.Discard(iFleet, iPred) = Me.BestFitDiscard(iFleet, iPred)
            Next

            For iPrey As Integer = 1 To m_core.nGroups
                m_epdata.DC(iPred, iPrey) = BestFitDiets(iPred, iPrey)
            Next

            'vc sep 2008: adding vulnerability to MC
            'm_esdata.VulnerabilityPredator(iPred) = BestFit(eMCParams.Vulnerability, iPred)
            'Also transfer to vulmult
            'For iPrey As Integer = 1 To m_core.nGroups
            '    m_esdata.VulMult(iPrey, iPred) = BestFit(eMCParams.Vulnerability, iPred)
            '    'jb this is done by the manager in ApplyBestFits core.onChanged() 
            '    m_core.EcoSimGroupInputs(iPrey).VulMult(iPred) = BestFit(eMCParams.Vulnerability, iPred)
            'Next


            'ToDo_jb cEcosimMonteCarlo.Run something is wrong here
            'I don't have a BAinput BA will contain the best fit parameters
            ' m_epdata.BAinput(i) = m_epdata.BA(i)
            '    optVary_Click(0)
        Next

    End Sub

    Private Function NumParams() As Integer
        ' Do not include 'not set' (thus not redim by length  + 1)
        Return [Enum].GetValues(GetType(eMCParams)).Length
    End Function

    Private Function NumDietSamplingMethods() As Integer
        Return [Enum].GetValues(GetType(eMCDietSamplingMethod)).Length
    End Function

    Private Sub redimVariables()
        Try

            ReDim CVpar(Me.NumParams, m_core.nGroups)
            ReDim CVparLanding(m_core.nFleets, m_core.nGroups)
            ReDim CVparDiscard(m_core.nFleets, m_core.nGroups)
            ReDim CVParDC(Me.NumDietSamplingMethods - 1, m_core.nGroups)

            ReDim ParLimit(1, NumParams(), m_core.nGroups)
            ReDim ParLimitLanding(1, m_core.nFleets, m_core.nGroups)
            ReDim ParLimitDiscard(1, m_core.nFleets, m_core.nGroups)
            ReDim ParLimitDC(1, m_core.nGroups, m_core.nGroups)

            For iGroup As Integer = 1 To m_core.nGroups
                For iVar As Integer = 1 To Me.NumParams

                    Select Case DirectCast(iVar, eMCParams)
                        Case eMCParams.BA
                            CVpar(iVar, iGroup) = 0.05
                        Case eMCParams.BaBi
                            CVpar(iVar, iGroup) = 0.05
                        Case eMCParams.Diets
                            CVParDC(eMCDietSamplingMethod.Dirichlets, iGroup) = 1
                            CVParDC(eMCDietSamplingMethod.NormalDistribution, iGroup) = 0.05
                        Case Else
                            CVpar(iVar, iGroup) = 0.1
                    End Select
                Next iVar

                For iFleet As Integer = 1 To m_core.nFleets
                    CVparLanding(iFleet, iGroup) = 0.1
                    CVparDiscard(iFleet, iGroup) = 0.1
                Next iFleet

            Next iGroup

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
            Throw New ApplicationException(Me.ToString & ".redimVariables()", ex)
        End Try

    End Sub


    ''' <summary>
    ''' Calculte the Upper and Lower Parameter limits from CV values
    ''' </summary>
    ''' <param name="IsCrashEvaluated">Not USED!</param>
    ''' <param name="param">The parameter to calculate, or <see cref="eMCParams.NotSet"/> to calculate all.</param>
    ''' <remarks>Called once during initialization to set default values or when CV values have been edited</remarks>
    Public Sub CalculateUpperLowerLimits(ByVal IsCrashEvaluated As Boolean, Optional param As eMCParams = eMCParams.NotSet)

        Try
            'jb set the Upper and Lower Limits to 2*CV
            Dim factor As Integer = 2

            'We want a wide range for searching, cv will still limit the steps
            For iGroup As Integer = 1 To m_core.nLivingGroups

                If (param = eMCParams.Biomass Or param = eMCParams.NotSet) Then
                    ' Upper
                    ParLimit(0, eMCParams.Biomass, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.B(iGroup) * (1 - factor * CVpar(eMCParams.Biomass, iGroup)))
                    ' Lower
                    ParLimit(1, eMCParams.Biomass, iGroup) = Me.m_epdata.B(iGroup) * (1 + factor * CVpar(eMCParams.Biomass, iGroup))
                    If ParLimit(1, eMCParams.Biomass, iGroup) < ParLimit(0, eMCParams.Biomass, iGroup) Then
                        ParLimit(1, eMCParams.Biomass, iGroup) = 10 * ParLimit(0, eMCParams.Biomass, iGroup)
                    End If
                End If

                If (param = eMCParams.PB Or param = eMCParams.NotSet) Then
                    ' Upper
                    ParLimit(0, eMCParams.PB, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.PB(iGroup) * (1 - factor * CVpar(eMCParams.PB, iGroup)))
                    ' Lower
                    ParLimit(1, eMCParams.PB, iGroup) = Me.m_epdata.PB(iGroup) * (1 + factor * CVpar(eMCParams.PB, iGroup))
                    If ParLimit(1, eMCParams.PB, iGroup) < ParLimit(0, eMCParams.PB, iGroup) Then ParLimit(1, eMCParams.PB, iGroup) = 10 * ParLimit(0, eMCParams.PB, iGroup)
                End If

                If (param = eMCParams.QB Or param = eMCParams.NotSet) Then
                    ' Upper
                    ParLimit(0, eMCParams.QB, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.QB(iGroup) * (1 - factor * CVpar(eMCParams.QB, iGroup)))
                    ' Lower
                    ParLimit(1, eMCParams.QB, iGroup) = Me.m_epdata.QB(iGroup) * (1 + factor * CVpar(eMCParams.QB, iGroup))
                    If ParLimit(1, eMCParams.QB, iGroup) < ParLimit(0, eMCParams.QB, iGroup) Then ParLimit(1, eMCParams.QB, iGroup) = 10 * ParLimit(0, eMCParams.QB, iGroup)
                End If

                If (param = eMCParams.EE Or param = eMCParams.NotSet) Then
                    ' Upper
                    ParLimit(0, eMCParams.EE, iGroup) = Math.Max(0, Me.m_epdata.EE(iGroup) * (1 - factor * CVpar(eMCParams.EE, iGroup)))
                    ' Lower
                    ParLimit(1, eMCParams.EE, iGroup) = Me.m_epdata.EE(iGroup) * (1 + factor * CVpar(eMCParams.EE, iGroup))
                    If ParLimit(1, eMCParams.EE, iGroup) > 1 Then ParLimit(1, eMCParams.EE, iGroup) = 1
                End If

                If (param = eMCParams.BA Or param = eMCParams.NotSet) Then
                    'BA is +- relative to B not to BA (which is usually zero)
                    ParLimit(0, eMCParams.BA, iGroup) = Me.m_epdata.BA(iGroup) + Me.m_epdata.B(iGroup) * (-factor * CVpar(eMCParams.BA, iGroup))

                    'BA is +- relative to B not to BA (which is usually zero)
                    ParLimit(1, eMCParams.BA, iGroup) = m_epdata.BA(iGroup) + m_epdata.B(iGroup) * (factor * CVpar(eMCParams.BA, iGroup))
                End If

                If (param = eMCParams.BaBi Or param = eMCParams.NotSet) Then
                    ParLimit(0, eMCParams.BaBi, iGroup) = Me.m_epdata.BaBi(iGroup) * (1 - factor * CVpar(eMCParams.BaBi, iGroup))
                    ParLimit(1, eMCParams.BaBi, iGroup) = Me.m_epdata.BaBi(iGroup) * (1 + factor * CVpar(eMCParams.BaBi, iGroup))

                    If ParLimit(0, eMCParams.BaBi, iGroup) > ParLimit(1, eMCParams.BaBi, iGroup) Then
                        Dim t As Single = ParLimit(0, eMCParams.BaBi, iGroup)
                        ParLimit(0, eMCParams.BaBi, iGroup) = ParLimit(1, eMCParams.BaBi, iGroup)
                        ParLimit(1, eMCParams.BaBi, iGroup) = t
                    End If
                End If

                'Vul is from 1 up
                ' ParLimit(0, eMCParams.Vulnerability, i) = m_esdata.VulnerabilityPredator(i) * (1 - factor * CVpar(eMCParams.Vulnerability, i)) : If ParLimit(0, eMCParams.Vulnerability, i) < 1.01 Then ParLimit(0, eMCParams.Vulnerability, i) = 1.01
                ' ParLimit(1, eMCParams.Vulnerability, i) = 1000 ' m_esdata.VulnerabilityPredator(i) * (1 + factor * CVpar(eMCParams.Vulnerability, i)) 'no upper limit for vulmult : If ParLimit(1, eMCParams.Vulnerability, i) > 1 Then ParLimit(1, eMCParams.Vulnerability, i) = 1

                If (param = eMCParams.Diets Or param = eMCParams.NotSet) Then
                    For iPred As Integer = 1 To Me.m_core.nLivingGroups
                        ParLimitDC(0, iPred, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.DCInput(iPred, iGroup) * (1 - factor * CVParDC(eMCDietSamplingMethod.NormalDistribution, iPred)))
                        ParLimitDC(1, iPred, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.DCInput(iPred, iGroup) * (1 + factor * CVParDC(eMCDietSamplingMethod.NormalDistribution, iPred)))
                    Next iPred
                End If

            Next iGroup

            For iGroup As Integer = 1 To Me.m_core.nGroups
                For iFleet As Integer = 1 To Me.m_core.nFleets
                    If (param = eMCParams.Landings Or param = eMCParams.NotSet) Then
                        ParLimitLanding(0, iFleet, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.Landing(iFleet, iGroup) * (1 - factor * CVparLanding(iFleet, iGroup)))
                        ParLimitLanding(1, iFleet, iGroup) = Math.Min(10 * ParLimitLanding(0, iFleet, iGroup), Me.m_epdata.Landing(iFleet, iGroup) * (1 + factor * CVparLanding(iFleet, iGroup)))
                    End If

                    If (param = eMCParams.Discards Or param = eMCParams.NotSet) Then
                        ParLimitDiscard(0, iFleet, iGroup) = Math.Max(1.0E-10!, Me.m_epdata.Discard(iFleet, iGroup) * (1 - factor * CVparDiscard(iFleet, iGroup)))
                        ParLimitDiscard(1, iFleet, iGroup) = Math.Min(10 * ParLimitDiscard(0, iFleet, iGroup), Me.m_epdata.Discard(iFleet, iGroup) * (1 + factor * CVparDiscard(iFleet, iGroup)))
                    End If

                Next iFleet
            Next iGroup

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
            Throw New ApplicationException(Me.ToString & ".Run", ex)
        End Try


    End Sub

    Private Function ChooseFeasiblePar(ByVal par As eMCParams,
                                       ByVal xbar As Single, ByVal CV As Single,
                                       ByVal ParMin As Single, ByVal ParMax As Single) As Single

        ' Sanity checks
        Debug.Assert((ParMin <= xbar) And (xbar <= ParMax))

        Dim X As Single
        Dim i As Integer

        While (i < MAX_ECOPATH_TRIES)
            'jb 7-Dec-2010 ChooseFeasiblePar() changed application of CV 
            ' X = xbar * (1 + 0.02 * CV * RandomNormal())
            X = xbar * (1 + CV * RandomNormal())
            If (X >= ParMin And X <= ParMax) Then Return X
            i += 1
        End While

        System.Console.WriteLine("ChooseFeasiblePar(" & par & ") Can't find acceptable parameter" & ParMin & "<=" & xbar & "<=" & ParMax & ", using mean")
        Return xbar

    End Function

    Private Function ChooseFeasibleBA(ByVal Biomass As Single,
                                      ByVal xbar As Single, ByVal CV As Single,
                                      ByVal ParMin As Single, ByVal ParMax As Single) As Single

        ' Sanity checks
        Debug.Assert((ParMin <= xbar) And (xbar <= ParMax))

        Dim X As Single
        Dim i As Integer = 0

        While (i < MAX_ECOPATH_TRIES)
            X = xbar + Biomass * (CV * RandomNormal())
            If (X >= ParMin And X <= ParMax) Then Return X
            i += 1
        End While

        System.Console.WriteLine("ChooseFeasibleBA() Can't find acceptable parameter" & ParMin & "<=" & Biomass & "<=" & ParMax & ", using 0")
        Return 0

    End Function

    Private Sub ChooseFeasibleDiet(ByVal Diets(,) As Single, ByVal cv As Single, ByVal iPred As Integer, ByRef EcopathDiet(,) As Single)

        Debug.Assert(Me.DietSamplingMethod = eMCDietSamplingMethod.Dirichlets)

        Dim MeanPropMod() As Single
        Dim SumInteractions As Integer = 0
        Dim TempDirichlet() As Single
        Dim iPointer As Integer = 0

        Const MIN_DIET_PROP As Single = 0.000001

        'SumInteractions(iPred - 1) += If(m_core.EcoPathGroupInputs(iPred).ImpDiet > 0, 1, 0)
        For iPrey As Integer = 0 To m_core.nGroups
            SumInteractions += If(Diets(iPred, iPrey) > 0, 1, 0)
        Next

        'mCore.EcoPathGroupInputs(iPred + 1).DietComp(0) = 0
        If (SumInteractions = 0) Then    'No need to do any of this unless there is at least 1 prey for this parameter
            'Set all values to zero - if running slow might want to consider how this could be skipped - possibly setting whole array to zero at start
            For iPrey As Integer = 0 To m_core.nGroups
                EcopathDiet(iPred, iPrey) = 0
            Next
        Else
            ' DirichStopWatch.Start()

            ReDim MeanPropMod(SumInteractions)
            iPointer = 1
            'If Diets(iPred, 0) > 0 Then
            '    MeanPropMod(iPointer) = Diets(iPred, 0)
            '    iPointer += 1
            'End If
            For iPrey As Integer = 0 To m_core.nGroups
                If Diets(iPred, iPrey) > 0 Then
                    MeanPropMod(iPointer) = Diets(iPred, iPrey)
                    iPointer += 1
                End If
            Next iPrey

            'Samples a set of Dirichlet distributed parameters
            TempDirichlet = DirichletSample2(SumInteractions, MeanPropMod, cv)

            Dim i As Integer = 1
            Dim dProp As Single

            For iPrey As Integer = 0 To m_core.nGroups
                If Diets(iPred, iPrey) > 0 Then
                    dProp = TempDirichlet(i)
                    If dProp < MIN_DIET_PROP Then
                        dProp = 0.0F
                    End If
                    EcopathDiet(iPred, iPrey) = dProp
                    i += 1
                End If
            Next iPrey

        End If

    End Sub

    Public Function DirichletSample2(ByVal nDimensions As Integer, ByVal alpha() As Single, ByVal DietMultiplier As Single) As Single()
        Dim gamma(nDimensions) As Single
        Dim dirichlet(nDimensions) As Single
        Dim sumofgamma As Single

        For i As Integer = 1 To nDimensions
            alpha(i) = CSng(alpha(i) * DietMultiplier)
        Next

        For i As Integer = 1 To nDimensions
            gamma(i) = CSng(SampleGamma(alpha(i), 1))
        Next

        sumofgamma = gamma.Sum()
        For i As Integer = 1 To nDimensions
            dirichlet(i) = gamma(i) / sumofgamma
        Next

        Return (dirichlet)

    End Function

    Private Function RandomNormal() As Single
        Dim i As Integer, X As Single
        X = -6
        For i = 1 To 12 : X = X + CSng(Me.m_rand.NextDouble()) : Next
        Return X
    End Function

    'Private Sub ChangeVulnerabilities(ByVal ParCurVal(,) As Single, ByVal CVpar(,) As Single)

    '    For iPred As Integer = 1 To m_core.nLivingGroups
    '        m_esdata.VulnerabilityPredator(iPred) = ChooseFeasiblePar(ParCurVal(eMCParams.Vulnerability, iPred),
    '                                                                 CVpar(6, iPred),
    '                                                                 ParLimit(0, eMCParams.Vulnerability, iPred),
    '                                                                 ParLimit(1, eMCParams.Vulnerability, iPred),
    '                                                                 False)
    '        For iPrey As Integer = 1 To m_core.nGroups
    '            m_esdata.VulMult(iPrey, iPred) = m_esdata.VulnerabilityPredator(iPred)
    '        Next
    '    Next

    'End Sub

    Private Sub CheckWhoIsCrashed()
        Dim EndTime As Integer = (m_core.EcoSimModelParameters.NumberYears - 1) * 12
        'Dim sStr As String = "Crashed: "
        For iGrp As Integer = 1 To m_core.nLivingGroups

            If Me.m_esdata.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGrp, EndTime) / m_core.EcoPathGroupOutputs(iGrp).Biomass < 0.01 Then
                'jb use the core arrays instead of the Ecosim Output objects because the output objects have not been initialized
                'If m_core.EcoSimGroupOutputs(iGrp).Biomass(EndTime) / m_core.EcoPathGroupOutputs(iGrp).Biomass < 0.01 Then
                isCrashed(iGrp) = True
                'sStr += iGrp.ToString & ", "
            Else
                isCrashed(iGrp) = False
            End If
            'If m_core.EcoSimGroupOutputs(iGrp).Biomass(EndTime) / m_core.EcoPathGroupOutputs(iGrp).Biomass > 10 Then
            '    isexploded(iGrp) = True
            'Else
            '    isExploded(iGrp) = False
            'End If
        Next
        'If sStr <> "Crashed: " Then
        '    Using sw As StreamWriter = New StreamWriter("c:\LME\Vulnerabilities.csv", True)  'true makes it append
        '        sw.WriteLine(iTrial.ToString & ", " & sStr)
        '        sw.Close()
        '    End Using
        '    Console.WriteLine(sStr)
        'End If
    End Sub


#Region "xxx DEAD CODE (Multi threaded Monte Carlo) xxx"

#If 0 Then

    ''' <summary>
    ''' Multi threaded Monte Carlo code has been disabled but left in place for future reference
    ''' </summary>
    Private Sub initThreads(ByVal trList As List(Of cMonteCarloThread), ByVal nThreads As Integer)
        'gives back a list (nThreads long) of fully initialized cMonteCarloThread objects

        Dim MCthread As cMonteCarloThread

        Try
            For i As Integer = 1 To nThreads
                MCthread = New cMonteCarloThread(i)
                MCthread.init(m_core.nGroups, m_core.nLivingGroups)

                'get ep data
                m_epdata.copyTo(MCthread.EPdata)
                MCthread.EP.ModelingData = MCthread.EPdata
                MCthread.EP.ParameterEstimationType = m_ecopath.ParameterEstimationType
                MCthread.EP.EstimateParameters()
                MCthread.ES.EcopathParameters = MCthread.EPdata
                MCthread.EP.missing = m_ecopath.missing.Clone

                'init ES and copy data
                m_esdata.CopyTo(MCthread.ESdata)
                m_ecosim.copyTo(MCthread.ES)
                MCthread.ES.m_Data = MCthread.ESdata
                MCthread.ES.SetCounters()
                'MCthread.ES.SetDefaultParameters()

                'get other data structures
                m_stanza.copyTo(MCthread.StanzaData)
                MCthread.ES.TracerData = New cContaminantTracerDataStructures
                m_tracerData.CopyTo(MCthread.ES.TracerData)

                'link models to data structures
                MCthread.ES.m_stanza = MCthread.StanzaData
                MCthread.ES.TimeSeriesData = m_ecosim.TimeSeriesData
                MCthread.ES.SearchData = m_ecosim.SearchData
                'MCthread.ES.TimeStepDelegate = m_ecosim.TimeStepDelegate

                'init some ecosim stuff

                'MCthread.ES.m_Data.RedimVars()
                MCthread.ES.InitStanza()

                'assign thread properties
                MCthread.pmean = Pmean.Clone
                MCthread.CVpar = CVpar.Clone
                MCthread.parLimit = ParLimit.Clone

                trList.Add(MCthread)
            Next

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
        'mcthread.iter=iter
    End Sub

#End If ' 0
#End Region

End Class
