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
Imports EwECore.Ecosim
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace MSY

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' MSY search engine.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cMSY

#Region " ToDo "

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Known issues
        'DONE: MSY 18-Oct-2012 BaseLineResult populated in setBaselineValues() are populated from the results at the end of the BaseLine Run
        '                      the BaseLineResult should be set from the Ecopath base values
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Plotting
        'DONE: MSY 18-Oct-2012 Catch of the selected group should be plotted on the biomass graph
        'DONE: MSY 18-Oct-2012 When the selected group is multi stanza all the life stages should be shown, even if they are not fished.
        'DONE: MSY 18-Oct-2012 It should be possible to show/plot non fished groups from the Show/Hide items. This is important for the Compensatory graph
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'General stuff
        'ToDo: MSY 18-Oct-2012 Progress messages Started, Completed, Progress...
        'ToDo: MSY 18-Oct-2012 Error messages and return values for functions. Comunication back to the Manager->Core via delegates or message publisher.
        'ToDo: MSY 18-Oct-2012 More Error handling with messages.
        'ToDo: MSY 18-Oct-2012 Testing of baseline values to make sure they are > some tiny number
        'ToDo: MSY 18-Oct-2012 Add All Fleets support
        'ToDo: MSY 18-Oct-2012 Implementation of parameters Number of Years per equilibrium step, Max Relative F, F step size(as percentage of total change in F (MaxRelF))
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Debugging
        'DONE: MSY 18-Oct-2012 Test against EwE6 Ecosim output. Write a routine that dumps B for each time step to a csv file.
        '                      When in Full Compensatory mode (B not forced) this can be used to test against the Ecosim output if the F matches either via Effort or F forcing.
        'ToDo: MSY 18-Oct-2012 Test against EwE5 output. This is tricky because it has to be a model that gives the same Ecosim results in 5 and 6 and is multi stanza with no pair in 5
        '                      The MSY in 5 runs in a different mode when it's not a multi stanza model
        'DONE? MSY 18-Oct-2012 Make sure the F step size is robust to strange values in F
        '                      It looks like in some cases (Tampa Bay Snook 48-90) either the F step in wrong, to big, 
        '                      or it is not comming to equilibrium during the step, 
        '                      or the values are not getting caried from step to step correctly 
        '                      DONE (maybe) by adding Max. Rel. F. to the interface 
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'ToDo: MSY 18-Oct-2012 Documentation of code variable and methods and how the code implements the paper, that assumes it does...
        'ToDo: MSY 18-Oct-2012 Documentation of UI on the Wiki
        'DONE: MSY 19-Oct-2012 frmMSYSingleSpecies does not re-plot when the Selected groups/fleets changes (Show items...)
        'ToDo: MSY 30-Oct-2012 (DONE for time series NOT Forcing functions)  warn about Time Series Forcing function. These are Nutrient, Salinity.... anything that is by time
        '                       Group Interaction forcing function should be OK

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'ToDo FMSY Search
        'In EwE5
        'FMSY can be run with groups frozen or not this depends on the Freeze Groups check box in the interface
        'For each group
        '   Run the Single Species search and save FMSY(ngroups) across all the runs
        'next group
        '  
        '   Once all the groups have been run 
        '       For each group
        '           F(i) = FMSY(i) * FmsyRatio 
        '           ForceB(i) = False
        '       next
        '       Run EcosimRK4 
        '     
        '       If Include analysis for cross species.. 
        '           For each i (group)
        '               InitEcosim
        '               For each j (group)
        '                   F(j) = FMSY(j)
        '                   ForceB(j) = False
        '               next j
        '               F(i) = FMSY(i) * 0.9 10% reduction in F from FMSY
        '               EcosimRK4
        '               for each j (group)
        '                   CMSY(i,j) = Catch(j) - EcopathCatch(j)
        '               next j
        '           next i
        '
        '       End If
        '   SaveFile FMSY, CMSY
        'ToDo: FMSY in EwE6
        '   Maybe a bug in setFishingRates() when in fleet selection mode
        '   Change FBase to be by Group
        '  

#End Region ' ToDo

#Region " Private variables "

        Private m_Ecosim As cEcoSimModel
        Private m_msyData As cMSYDataStructures
        Private m_simData As cEcosimDatastructures
        Private m_pathData As cEcopathDataStructures

        ' -- progress --
        Private m_iNumSteps As Integer
        Private m_iStep As Integer
        Private m_strStatus As String = ""

        ''' <summary>
        ''' Local copy of biomass that is used for the long simulation
        ''' </summary>
        Public bb() As Single

        Public ValSum() As Single
        Public ValSumBase() As Single

        Public FmsySS() As Single
        Public CmsySS() As Single
        Public VmsySS() As Single

        Public CatchAtFmsy() As Single
        Public ValueAtFmsy() As Single

        ''' <summary>F at base line for the current group</summary>
        Private m_FBase As Single
        Private m_Fstep As Single = 0.03
        Private m_Fmax As Single = cMSYDataStructures.F_MAX

        Friend m_FOptTracker() As cFoptTracker

        Private m_RunStateDelegate As MSYRunStateDelegate
        Private m_MessageDelegate As cCore.CoreMessageDelegate = Nothing

#End Region

#Region " Construction and Initialization "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new instance of the MSY model.
        ''' </summary>
        ''' <param name="Ecosim"><see cref="cEcoSimModel">Ecosim model</see> to use.</param>
        ''' <param name="MsyData"><see cref="cMSYDataStructures">MSY data structures</see> to use.</param>
        ''' <param name="EcopathData"><see cref="cEcopathDataStructures">Ecopath data structures</see> to use.</param>
        ''' <param name="EcosimData"><see cref="cEcosimDatastructures">Ecosim data structures</see> to use.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal Ecosim As cEcoSimModel, ByVal MsyData As cMSYDataStructures, _
                       ByVal EcopathData As cEcopathDataStructures, ByVal EcosimData As cEcosimDatastructures)

            Debug.Assert(Ecosim IsNot Nothing, Me.ToString & ".New() Invalid Ecosim Model object!")
            Debug.Assert(Ecosim.EcosimData IsNot Nothing, Me.ToString & ".New() Invalid Ecosim data object!")
            Debug.Assert(MsyData IsNot Nothing, Me.ToString & ".New() Invalid MSY data object!")
            Debug.Assert(EcopathData IsNot Nothing, Me.ToString & ".New() Invalid MSY data object!")

            Me.m_Ecosim = Ecosim
            Me.m_msyData = MsyData
            Me.m_pathData = EcopathData
            Me.m_simData = EcosimData

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Connect the MSY model to a <see cref="MSYRunStateDelegate">run state delegate</see> 
        ''' through which this model can report its run state, and a <see cref="cCore.CoreMessageDelegate">message delegate</see>
        ''' through which this model can report internal events.
        ''' </summary>
        ''' <param name="RunStateDelegate">The <see cref="MSYRunStateDelegate">run state delegate</see>
        ''' to connect, or Nothing to <seealso cref="Disconnect"/>.</param>
        ''' <param name="MessageDelegate">The <see cref="cCore.CoreMessageDelegate">message delegate</see> 
        ''' through which the MSY model can send <see cref="cMessage">messages</see>.</param>
        ''' -------------------------------------------------------------------
        Public Sub Connect(ByVal RunStateDelegate As MSYRunStateDelegate,
                           ByVal MessageDelegate As cCore.CoreMessageDelegate)
            Me.m_RunStateDelegate = RunStateDelegate
            Me.m_MessageDelegate = MessageDelegate
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Disconnect from <seealso cref="Connect">previously connected delegates</seealso>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Disconnect()
            Me.Connect(Nothing, Nothing)
        End Sub

#End Region

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform an MSY run.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RunMSY() As Boolean

            Dim bRan As Boolean = False

            Try

                Me.m_msyData.bStopRun = False

                ' Do this once at the start of it all
                Me.InitTrackers()

                ' InitForSingleSpeciesRun will set m_FStep
                'If InitForSingleSpeciesRun Fails it will return false
                If Not InitForSingleRun() Then Return False

                Dim iNumSteps As Integer = CInt(Me.m_Fmax / Me.m_Fstep)
                Dim strAssessment As String = Me.m_msyData.AssessmentType.ToString

                Me.StartProgress(cStringUtils.Localize(My.Resources.CoreMessages.MSY_STATUS_RUNNING, strAssessment), iNumSteps)
                bRan = runSingleSpecies()
                Me.EndProgress()

            Catch ex As Exception

            End Try

            Return bRan

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform an Find FMSY run.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RunFindFMSY() As Boolean

            Dim strAssessment As String = Me.m_msyData.AssessmentType.ToString
            Dim iNumSteps As Integer = 0
            Dim bRan As Boolean = False

            Me.m_msyData.bStopRun = False

            ' Do this once at the start of it all
            Me.InitTrackers()

            Me.InitFMSY()

            ' Determine number of run steps
            For iGrp As Integer = 1 To Me.m_msyData.nLiving
                If Me.m_simData.Fish1(iGrp) > 0 Then
                    iNumSteps += CInt(Me.m_msyData.MaxRelF / (Me.m_msyData.MaxRelF * Me.m_msyData.FStepSize))
                End If
            Next
            Me.StartProgress(My.Resources.CoreMessages.GENERIC_STATUS_INITIALIZING, iNumSteps)

            Try

                For iGrp As Integer = 1 To Me.m_msyData.nLiving

                    If Me.m_simData.Fish1(iGrp) > 0 Then
                        Me.m_msyData.iSelGroupFleet = iGrp

                        If Me.InitForRun() Then
                            Me.m_strStatus = cStringUtils.Localize(My.Resources.CoreMessages.FMSY_STATUS_RUNNING, strAssessment, Me.m_pathData.GroupName(iGrp))
                            If Not Me.runSingleSpecies() Then
                                'WTF
                                'Failed to run the single species MSY search
                                'Really... what now... just plough on
                                cLog.Write(Me.ToString & ".RunFMSY() Failed to run MSY Search for group " & iGrp.ToString)
                            End If
                        Else
                            'message
                            cLog.Write(Me.ToString & ".RunFMSY() Failed to initialize MSY Search for group " & iGrp.ToString)
                        End If
                    End If

                Next iGrp

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Ok now re-run Ecosim with FMSY across all the groups
                Me.InitEcosimForRK4()

                'Set fishing mortality to FMSY across all the single species runs above
                For igrp As Integer = 1 To Me.m_msyData.nGroups
                    Me.m_simData.FishTime(igrp) = Me.FmsySS(igrp)
                    Me.m_msyData.ForceGroupB(igrp) = False
                Next

                'Run Ecosim for a longer time period normal MSY run
                'So it can stablize to FMSY for all groups
                Me.EcosimRK4(100)

                'populate Catch and Value from bb() at the last timestep from Ecosim
                Me.getFMSYResults()

                bRan = True
                'xxxxxxxxxxxxxxxxxxxxx

            Catch ex As Exception
                bRan = False
                cLog.Write(ex)
            End Try
            Me.EndProgress()

            Debug.Assert(bRan, Me.ToString & ".RunFMSY() Failed...")
            Return bRan

        End Function

#End Region ' Public interfaces 

#Region " Friendly bits "

        ''' <summary>
        ''' Initialize anything that is specific to a FMSY run
        ''' </summary>
        Friend Function InitFMSY() As Boolean

            Try
                'Not much right now
                Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups
                Me.m_msyData.MSYRunType = eMSYRunTypes.FMSY

                ReDim FmsySS(Me.m_msyData.nGroups)
                ReDim CmsySS(Me.m_msyData.nGroups)
                ReDim VmsySS(Me.m_msyData.nGroups)

                ReDim CatchAtFmsy(Me.m_msyData.nGroups)
                ReDim ValueAtFmsy(Me.m_msyData.nGroups)

                Return Me.InitForRun()

            Catch ex As Exception
                cLog.Write(ex)
                System.Console.WriteLine(Me.ToString & ".InitFMSY() Exception: " & ex.Message)
            End Try
            Return False

        End Function

        Friend Function InitForRun() As Boolean

            Try

                'Redim local arrays need for any run SS MSY or FMSY
                ReDim bb(Me.m_msyData.nGroups)
                ReDim ValSum(Me.m_msyData.nGroups)
                ReDim ValSumBase(Me.m_msyData.nGroups)

                If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                    Debug.Assert(Me.m_msyData.iSelGroupFleet <= Me.m_msyData.nGroups, Me.ToString & ".InitForRun() Selected group > ngroups.")
                    If Me.m_msyData.iSelGroupFleet > Me.m_msyData.nGroups Then Me.m_msyData.iSelGroupFleet = Me.m_msyData.nGroups

                ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                    Debug.Assert(Me.m_msyData.iSelGroupFleet <= Me.m_msyData.nFleets, Me.ToString & ".InitForRun() Selected fleet > nfleets.")
                    If Me.m_msyData.iSelGroupFleet > Me.m_msyData.nFleets Then Me.m_msyData.iSelGroupFleet = Me.m_msyData.nFleets

                End If

                Me.setForcedGroupB()
                Me.setFStepSize()

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.Message)
                Return False
            End Try

            Return True

        End Function


        Friend Function InitForSingleRun() As Boolean

            Try
                'other initializtion here???
                Me.m_msyData.bStopRun = False
                Me.m_msyData.lstResults.Clear()
                Me.m_msyData.MSYRunType = eMSYRunTypes.SingleRunMSY

                Return Me.InitForRun()
            Catch ex As Exception
                'At this time this should not happen
                'InitForRun() handles its own errors
                Return False
            End Try
            Return True

        End Function


        Friend Sub setForcedGroupB()

            Select Case Me.m_msyData.AssessmentType
                Case eMSYAssessmentTypes.StationarySystem

                    If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                        'GROUP SELECTED
                        'Selected Group and all Life Stages can responed to changes in F

                        For igrp As Integer = 1 To Me.m_msyData.nGroups
                            'Biomass is FORCED for all groups except the selected group(s)
                            Me.m_msyData.ForceGroupB(igrp) = True
                            'Only the selected group can respond to changes in F
                            If igrp = Me.m_msyData.iSelGroupFleet Then Me.m_msyData.ForceGroupB(igrp) = False
                        Next igrp

                        If Me.m_simData.NoIntegrate(Me.m_msyData.iSelGroupFleet) <> Me.m_msyData.iSelGroupFleet Then
                            'The currently selected group in a multi stanza group
                            'All life stages of this stanza group must be allowed to grow
                            'There biomass should NOT be forced

                            'Get the Stanza data from Ecosim
                            Dim stanzas As cStanzaDatastructures = Me.m_Ecosim.m_stanza

                            'Now find out which stanza group the selected group belongs to 
                            For iStz As Integer = 1 To stanzas.Nsplit
                                For iStage As Integer = 1 To stanzas.Nstanza(iStz)
                                    'Is the selected group a member of this stanza group
                                    If stanzas.EcopathCode(iStz, iStage) = Me.m_msyData.iSelGroupFleet Then
                                        'Yep Make sure all the life stages of this stanza can grow
                                        For iLF As Integer = 1 To stanzas.Nstanza(iStz)
                                            Me.m_msyData.ForceGroupB(stanzas.EcopathCode(iStz, iLF)) = False
                                        Next iLF
                                    End If 'stanzas.EcopathCode(iStz, iStage) = Me.m_msyData.iSelGroup
                                Next iStage
                            Next iStz
                        End If 'Me.m_simData.NoIntegrate(Me.m_msyData.iSelGroup) <> Me.m_msyData.iSelGroup 

                    ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                        'FLEET SELECTED
                        'All Groups fished by the selected fleet can responded to changes in F

                        For igrp As Integer = 1 To Me.m_msyData.nGroups
                            Me.m_msyData.ForceGroupB(igrp) = True
                            If Me.m_simData.FishMGear(Me.m_msyData.iSelGroupFleet, igrp) > 0 Then
                                Me.m_msyData.ForceGroupB(igrp) = False
                            End If
                        Next

                    End If

                Case eMSYAssessmentTypes.FullCompensation

                    'No Forced Biomass 
                    'All groups can respond to changes in F

                    For igrp As Integer = 1 To Me.m_msyData.nGroups
                        Me.m_msyData.ForceGroupB(igrp) = False
                    Next igrp

            End Select 'Me.m_msyData.Assessment

#If DEBUG Then
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Dump out the NON Forced groups for debugging
            System.Console.WriteLine("MSY groups that respond to changes in F:")
            For i As Integer = 1 To Me.m_msyData.nGroups
                If Not Me.m_msyData.ForceGroupB(i) Then
                    System.Console.Write(Me.m_pathData.GroupName(i) & ", ")
                End If
            Next
            System.Console.WriteLine()
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
#End If

        End Sub

        Friend Sub InitEcosimForRK4()

            Me.m_Ecosim.Init(True)

            'calculate pbbiomass parameter from pbbase and pbm
            Me.m_Ecosim.Set_pbm_pbbiomass()

            Me.m_Ecosim.SetFishTimetoFish1()
            For i As Integer = 1 To Me.m_msyData.nFleets
                Me.m_simData.FishRateGear(i, 0) = Me.m_simData.FishRateGear(i, 1)
            Next
            Me.m_Ecosim.CalcStartEatenOfBy()
            Me.m_Ecosim.InitialState()
            Me.m_Ecosim.SetTimeSteps()

            Me.m_Ecosim.SetBBtoStartBiomass(Me.m_msyData.nGroups)

            'Make a local copy of the base biomass
            For igrp As Integer = 1 To Me.m_msyData.nGroups
                Me.bb(igrp) = Me.m_simData.StartBiomass(igrp)
            Next

        End Sub

#End Region ' Friendly bits

#Region " Internals "

        Private Sub InitTrackers()
            ReDim Me.m_FOptTracker(Me.m_msyData.nGroups)
            For i As Integer = 1 To Me.m_msyData.nGroups
                Me.m_FOptTracker(i) = New cFoptTracker(i)
            Next
        End Sub

        Private Function runSingleSpecies() As Boolean
            'Select Case Me.m_msyData.RunLengthMode
            'Case eMSYRunLengthModeTypes.FixedF
            Return Me.runSingleSpeciesFixedF
            '    Case eMSYRunLengthModeTypes.ToDepletion
            '        Return Me.runSingleSpeciesToDepletion()
            'End Select
        End Function

        Private Sub setFStepSize()
            'Select Case Me.m_msyData.RunLengthMode
            '    Case eMSYRunLengthModeTypes.FixedF
            Me.setFStepSizeFixedF()
            '    Case eMSYRunLengthModeTypes.ToDepletion
            'Me.setFStepSizeToDepletion()
            'End Select
        End Sub

        ''' <summary>
        ''' Sets the F Step Size base on FSelectionMode, MaxRelF and FStepSize
        ''' </summary>
        ''' <remarks>
        ''' When FSelectionMode = eMSYFSelectionModeType.Groups F in runSingleSpecies() is the actual fishing mortality rate that get set in setFishingRates(F).
        ''' This means m_FBase is the Ecopath Base F, m_Fmax is Ecopath base * MaxRelF.
        ''' When FSelectionMode =  eMSYFSelectionModeType.Fleets then F in runSingleSpecies() is the relative F that is used by setFishingRates(F) to set F relative to the Ecopath base f.
        ''' </remarks>
        Private Sub setFStepSizeFixedF()

            Dim iSelGrp As Integer = Me.m_msyData.iSelGroupFleet

            If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                'In this case the F values used by the MSY are the actual F's that will be used by Ecosim
                'runSingleSpecies() will calculate F for each run of Ecosim

                'm_FBase is the base Ecopath F 
                Me.m_FBase = Me.m_simData.Fish1(iSelGrp)

                'm_Fmax is base * the user define relative max
                Me.m_Fmax = Me.m_simData.Fish1(iSelGrp) * Me.m_msyData.MaxRelF
                ''Make sure Fmax is big enough to cover a reasonable range
                'If Me.m_Fmax < 2 * (Me.m_pathData.PB(iSelGrp) - Me.m_simData.Fish1(iSelGrp)) Then Me.m_Fmax = 2 * Me.m_pathData.PB(iSelGrp) - Me.m_simData.Fish1(iSelGrp)
                ' JS: allow users to set any max range equal or higher to FBase
                Me.m_Fmax = Math.Max(Me.m_Fmax, Me.m_FBase)
                Me.m_Fstep = CSng(Me.m_msyData.FStepSize * Me.m_Fmax)

            ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                'In this case F is the relative F 
                'runSingleSpecies() sets the F as the F multiplier 
                'that will be used by setFishingRates(F) to set F relative to the base F cause by a fleet
                'fishingmort(group) = FishMGear(fleet,group) * FMult
                Me.m_FBase = 1.0
                ' JS: allow users to set any max range equal or higher to FBase
                Me.m_Fmax = Math.Max(Me.m_FBase, Me.m_msyData.MaxRelF)
                Me.m_Fstep = CSng(Me.m_msyData.FStepSize * Me.m_msyData.MaxRelF)

            End If

        End Sub

        Private Function runSingleSpeciesFixedF() As Boolean
            Dim bReturn As Boolean = True
            Try
                Me.InitEcosimForRK4()
                'Runs Ecosim with base line values
                Me.setBaseLineValues()

                'if SelectionMode = eMSYFSelectionModeType.Groups F = fishing mortality to use for the current run
                'if SelectionMode = eMSYFSelectionModeType.Fleets F = F multiplier to use for the current run

                For F As Single = Me.m_FBase To -0.00000000001 Step -Me.m_Fstep

                    If Me.m_msyData.bStopRun Then Exit For

                    Me.IncrementProgress()
                    Me.setFishingRates(F)
                    Me.EcosimRK4(Me.m_msyData.nYearsPerTrial)
                    Me.getEcosimRunResults(F, False)

                Next F

                'Re-init Ecosim to the base values
                InitEcosimForRK4()
                'This will re-run Ecosim with the base line values
                'which is needed to re-init to the base for the next set of runs
                Me.setBaseLineValues()

                'Start from one step past the base F
                'so that there is not two sets of results for the base F in the results list
                'A base line run was done above in setBaseLineValues()
                For F As Single = (Me.m_FBase + Me.m_Fstep) To Me.m_Fmax Step Me.m_Fstep

                    If Me.m_msyData.bStopRun Then Exit For

                    Me.IncrementProgress()
                    Me.setFishingRates(F)
                    Me.EcosimRK4(Me.m_msyData.nYearsPerTrial)
                    Me.getEcosimRunResults(F, True)

                Next F

                If Not Me.m_msyData.bStopRun Then
                    'Dont get the results if the user stopped the run
                    'Hope this is correct!!!
                    Me.GetOptimumResults()
                End If

#If DEBUG Then
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                System.Console.WriteLine("MSY Group, F, Total Value, B, Catch")
                Dim igrp As Integer = Me.m_msyData.iSelGroupFleet

                For Each result As cMSYFResult In Me.m_msyData.lstResults
                    System.Console.WriteLine(igrp.ToString + ", " + result.FCur.ToString + ",  " + result.TotalValue.ToString + _
                                             "," + result.B(igrp).ToString + ", " + result.[Catch](igrp).ToString)
                Next
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
#End If

            Catch ex As Exception
                cLog.Write(ex)
                System.Console.WriteLine(Me.ToString & ".runSingleSpecies() Exception: " & ex.Message)
                bReturn = False
            End Try

            If Me.m_msyData.bStopRun Then
                bReturn = False
            End If

            Return bReturn
        End Function

#Region " Running to depletion "

        ''' <summary>
        ''' Sets the F Step Size in auto-run mode.
        ''' </summary>
        Private Sub setFStepSizeToDepletion()

            Dim iSelGrp As Integer = Me.m_msyData.iSelGroupFleet

            If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                'In this case the F values used by the MSY are the actual F's that will be used by Ecosim
                'runSingleSpecies() will calculate F for each run of Ecosim
                Dim F As Single = Me.m_simData.Fish1(iSelGrp)
                Dim Z As Single = (Me.m_pathData.PB(iSelGrp) - Me.m_pathData.BA(iSelGrp) - CSng((Me.m_pathData.Emigration(iSelGrp) - Me.m_pathData.Immig(iSelGrp)) / Me.m_pathData.B(iSelGrp)))

                Me.m_FBase = F
                Me.m_Fmax = 2 * Z / F
                Me.m_Fstep = Me.m_Fmax / 100

            ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                'In this case F is the relative F 
                'runSingleSpecies() sets the F as the F multiplier 
                'that will be used by setFishingRates(F) to set F relative to the base F cause by a fleet
                'fishingmort(group) = FishMGear(fleet,group) * FMult
                Me.m_FBase = 1.0
                Me.m_Fmax = 200
                Me.m_Fstep = Me.m_Fmax / 100

            End If


        End Sub

        Private Function runSingleSpeciesToDepletion() As Boolean

            Dim F As Single
            Dim bDone As Boolean = False

            Try

                Me.InitEcosimForRK4()
                'Runs Ecosim with base line values
                Me.setBaseLineValues()

                'if SelectionMode = eMSYFSelectionModeType.Groups F = fishing mortality to use for the current run
                'if SelectionMode = eMSYFSelectionModeType.Fleets F = F multiplier to use for the current run

                F = Me.m_FBase : bDone = False
                While Not bDone

                    Me.IncrementProgress()
                    Me.setFishingRates(F)
                    Me.EcosimRK4(Me.m_msyData.nYearsPerTrial)
                    Me.getEcosimRunResults(F, False)

                    bDone = Me.IsDepleted(F)
                    F = Math.Max(0, F - Me.m_Fstep)

                End While

                'Re-init Ecosim to the base values
                InitEcosimForRK4()
                'This will re-run Ecosim with the base line values
                'which is needed to re-init to the base for the next set of runs
                Me.setBaseLineValues()

                'Start from one step past the base F
                'so that there is not two sets of results for the base F in the results list
                'A base line run was done above in setBaseLineValues()
                F = Me.m_FBase + Me.m_Fstep : bDone = False
                While Not bDone

                    'System.Console.WriteLine("F = " & F.ToString)

                    Me.IncrementProgress()
                    Me.setFishingRates(F)
                    Me.EcosimRK4(Me.m_msyData.nYearsPerTrial)
                    Me.getEcosimRunResults(F, True)

                    F += Me.m_Fstep
                    bDone = Me.IsDepleted(F)

                End While

                Me.GetOptimumResults()

#If DEBUG Then
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                System.Console.WriteLine("MSY Group, F, Total Value, B, Catch")
                Dim igrp As Integer = Me.m_msyData.iSelGroupFleet

                For Each result As cMSYFResult In Me.m_msyData.lstResults
                    System.Console.WriteLine(igrp.ToString & ", " & result.FCur.ToString & ",  " & result.TotalValue.ToString & _
                                             "," & result.B(igrp).ToString & ", " & result.[Catch](igrp).ToString)
                Next
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
#End If

            Catch ex As Exception
                cLog.Write(ex)
                System.Console.WriteLine(Me.ToString & ".runSingleSpecies() Exception: " & ex.Message)
                Return False
            End Try

            Return True
        End Function

        Private Function IsDepleted(FishingMort As Single) As Boolean

            If (FishingMort <= 0) Then Return True

            Dim igrp As Integer = 0
            Dim iflt As Integer = 0
            Dim [Catch](Me.m_msyData.nGroups) As Single
            Dim bDone As Boolean = False

            ' Gather up catches (should really be obtained from last result)
            For igrp = 1 To Me.m_msyData.nGroups
                [Catch](igrp) = bb(igrp) * Me.m_simData.FishTime(igrp)
            Next

            If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                'In GROUPS mode: done auto-running when Catch of target group reaches 0
                igrp = Me.m_msyData.iSelGroupFleet
                ' Zero-test is 'almost zero' test
                bDone = (Math.Round([Catch](igrp), 5) = 0)

            ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                'In FLEET mode: done auto-running when all Catches of target fleet reach 0
                Dim bAllZero As Boolean = True
                iflt = Me.m_msyData.iSelGroupFleet
                For igrp = 1 To Me.m_msyData.nGroups
                    If Me.m_pathData.Landing(iflt, igrp) > 0 Then
                        ' Zero-test is 'almost zero' test
                        bAllZero = bAllZero And (Math.Round([Catch](igrp), 5) = 0)
                    End If
                Next
                bDone = bAllZero

            End If
            Return bDone Or (FishingMort > Me.m_Fmax)

        End Function

#End Region ' Running to depletion

        ''' <summary>
        ''' Get the Base line values by running Ecosim with the base F for the selected group
        ''' </summary>
        Private Sub setBaseLineValues()
            Dim sumValue As Single

            For iGrp As Integer = 1 To Me.m_msyData.nGroups
                sumValue = 0
                If Me.m_pathData.fCatch(iGrp) > 0 Then
                    For iFlt As Integer = 1 To Me.m_msyData.nFleets
                        sumValue += Me.m_pathData.fCatch(iGrp) * Me.m_pathData.Market(iFlt, iGrp) * Me.m_pathData.Landing(iFlt, iGrp) / Me.m_pathData.fCatch(iGrp)
                    Next
                End If
                Me.ValSumBase(iGrp) = sumValue
            Next

            Me.setFishingRates(Me.m_FBase)

            'Run Ecosim with the Base F's to get Baseline 
            'this will populate bb() with biomass at base F
            If Me.EcosimRK4(Me.m_msyData.nYearsPerTrial) Then
                Me.saveBaselineValues()
            End If

        End Sub

        Private Sub saveBaselineValues()
            'In EwE5 this sets the base value of the Value of the Catch
            Try

                'Get Value from Ecopath base F() and B()
                Me.m_msyData.ValueBase = Me.getTotalValue(Me.m_simData.Fish1, Me.m_simData.StartBiomass)

                Me.m_msyData.BaseLineResult = Nothing
                Me.m_msyData.BaseLineResult = New cMSYFResult(Me.m_msyData.nGroups, Me.m_FBase, Me.m_msyData.ValueBase)

                'Populate the baseline results with Ecopath base values
                For igrp As Integer = 1 To Me.m_msyData.nGroups

                    Me.m_msyData.BaseLineResult.B(igrp) = Me.m_simData.StartBiomass(igrp)
                    Me.m_msyData.BaseLineResult.Catch(igrp) = Me.m_pathData.fCatch(igrp)
                    Me.m_msyData.BaseLineResult.FishingMort(igrp) = Me.m_simData.Fish1(igrp)

                Next

            Catch ex As Exception
                cLog.Write(ex)
                System.Console.WriteLine(Me.ToString & ".saveBaselineValues() Exception: " & ex.Message)
            End Try

        End Sub

        ''' <summary>
        ''' Set Fishing Mortality for the current group
        ''' </summary>
        ''' <param name="FishingMort"></param>
        Friend Sub setFishingRates(FishingMort As Single)

            If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                'FishingMort is the actual fishing mortality to use for the seleted group
                Me.m_simData.FishTime(Me.m_msyData.iSelGroupFleet) = FishingMort

            ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                'FishingMort is a multiplier that is used to compute F from the Ecopath base F
                Dim fgear As Single
                For igrp As Integer = 1 To Me.m_msyData.nGroups

                    If Me.m_simData.FishMGear(Me.m_msyData.iSelGroupFleet, igrp) > 0.0 Then
                        'Only Groups fished by this fleet
                        fgear = Me.m_simData.FishMGear(Me.m_msyData.iSelGroupFleet, igrp)
                        'First remove the F contributed by this fleet from the total F on this group
                        Me.m_simData.FishTime(igrp) = Me.m_simData.Fish1(igrp) - fgear
                        'Now add the weighted F back on to [base F] - [F from selected fleet]
                        Me.m_simData.FishTime(igrp) += fgear * FishingMort
                        'From 5
                        'FishTime(i) = Fish1(i) + FishMGear(iSelGroupFleet, i) * (fishingrate - 1)
                        Debug.Assert(Me.m_simData.FishTime(igrp) >= 0, Me.ToString & ".setFishingRates() Set F < 0.")
                        If Me.m_simData.FishTime(igrp) < 0 Then Me.m_simData.FishTime(igrp) = 0
                    End If
                Next

            End If


            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'EwE5
            'If GearShow = 0 Then
            '    FishTime(PoolShow) = fishingrate
            'Else
            'For i = 1 To NumGroups
            '    FishTime(i) = Fish1(i) + FishMGear(GearShow, i) * (fishingrate - 1)
            'Next
            'End If
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


        End Sub


        ''' <summary>
        ''' Runs the Ecosim rk4 for length of time set in Me.m_msyData.nYearsPerTrial
        ''' </summary>
        ''' <returns>True if successful.</returns>
        Friend Function EcosimRK4(nYears As Integer) As Boolean
            Dim t As Single
            Dim DeltaT As Single = 1.0 / cCore.N_MONTHS
            Dim nTrialMonths As Integer = nYears * cCore.N_MONTHS

            For imon As Integer = 1 To nTrialMonths

                'set any loaded forcing function for this time step
                Me.m_Ecosim.settval(imon)
                Me.m_Ecosim.setDenDepCatchMult(bb)

                'Clear out the monthly averaged multi-stanza variables
                Me.m_Ecosim.clearMonthlyStanzaVars()

                'Run the rk4 this is the main engine of Ecosim
                Me.m_Ecosim.rk4(bb, t, DeltaT, imon, True)
                t += DeltaT

                'Reset the biomass back to the start biomass if the group is forced
                'This will leave Biomass (for none forced groups) to the values computed at the end of the Ecosim run
                'for the next Ecosim run
                For igrp As Integer = 1 To Me.m_msyData.nGroups
                    If Me.m_msyData.ForceGroupB(igrp) = True Then
                        bb(igrp) = Me.m_simData.StartBiomass(igrp)
                    End If
                Next igrp

            Next imon

            Return True

        End Function


        Private Function getTotalValue(FishingMort() As Single, Biomass() As Single) As Single
            Dim value As Single

            'Total value
            For igrp As Integer = 1 To Me.m_msyData.nGroups
                For iflt As Integer = 1 To Me.m_msyData.nFleets
                    If Me.m_pathData.fCatch(igrp) > 0 Then
                        ' value += Me.m_simData.FishTime(igrp) * bb(igrp) * Me.m_pathData.Market(iflt, igrp) * Me.m_pathData.Landing(iflt, igrp) / Me.m_pathData.fCatch(igrp)
                        value += FishingMort(igrp) * Biomass(igrp) * Me.m_pathData.Market(iflt, igrp) * Me.m_pathData.Landing(iflt, igrp) / Me.m_pathData.fCatch(igrp)
                    End If
                Next
            Next

            Return value
        End Function


        Private Sub getEcosimRunResults(F As Single, ByVal bIncrementing As Boolean)
            Try
                If Me.m_msyData.MSYRunType = eMSYRunTypes.SingleRunMSY Then
                    'Single Run 
                    'Populate the cMSYFResults object for this F step 
                    'and store it in the list of results lstResults
                    If bIncrementing Then
                        Me.m_msyData.lstResults.Add(EcosimResultFactory(F))
                    Else
                        Me.m_msyData.lstResults.Insert(0, EcosimResultFactory(F))
                    End If

                ElseIf Me.m_msyData.MSYRunType = eMSYRunTypes.FMSY Then
                    'FMSY Run 
                    'This ONLY populates the results used for the FMSY Search cFMSYResults
                    'This leaves the results from a Single Species run intact for the interface to use
                    Dim igrp As Integer = Me.m_msyData.iSelGroupFleet
                    Dim curCatch As Single = bb(igrp) * Me.m_simData.FishTime(igrp)
                    Dim t As cFoptTracker = Me.m_FOptTracker(igrp)
                    t.Track(Me.m_simData.FishTime(igrp), curCatch)

                    ' IsFMSY indicates whether Fmsy is a maximum, e.g. when it is not the last F found.
                    If curCatch > Me.CmsySS(igrp) Then
                        Me.CmsySS(igrp) = curCatch
                        Me.FmsySS(igrp) = Me.m_simData.FishTime(igrp)
                        'we have to clear sum of value 
                        'so that it's not summed across Cmsy's
                        Me.VmsySS(igrp) = 0
                        For iflt As Integer = 1 To Me.m_msyData.nFleets
                            'is this group landed by this fleet
                            If Me.m_pathData.Landing(iflt, igrp) > 0 Then
                                'Yep so just sum value for the proportion of the total catch landed by this fleet
                                Me.VmsySS(igrp) += curCatch * Me.m_pathData.Market(iflt, igrp) * Me.m_pathData.Landing(iflt, igrp) / Me.m_pathData.fCatch(igrp)
                            End If
                        Next iflt
                    End If

                End If

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, "WTF!")
            End Try

        End Sub


        Private Sub getFMSYResults()
            Try

                For igrp As Integer = 1 To Me.m_msyData.nGroups
                    'Catch on a group across all the fleets
                    Dim curCatch As Single = bb(igrp) * Me.m_simData.FishTime(igrp)
                    Me.CatchAtFmsy(igrp) = curCatch

                    For iflt As Integer = 1 To Me.m_msyData.nFleets
                        'is this group landed by this fleet
                        If Me.m_pathData.Landing(iflt, igrp) > 0 Then
                            'Yep so just sum value for the proportion of the total catch landed by this fleet
                            Me.ValueAtFmsy(igrp) += curCatch * Me.m_pathData.Market(iflt, igrp) * Me.m_pathData.Landing(iflt, igrp) / Me.m_pathData.fCatch(igrp)
                        End If
                    Next iflt
                Next
            Catch ex As Exception
                Debug.Assert(False, "WFT MSY.getFMSYResults Exception: " & ex.Message)
                cLog.Write(ex)
            End Try

        End Sub

        Private Function EcosimResultFactory(FishingMort As Single) As cMSYFResult

            'Get Total value from Ecosim F and B 
            Dim igrp As Integer = 0
            Dim TotVal As Single = Me.getTotalValue(Me.m_simData.FishTime, bb)
            Dim result As New cMSYFResult(Me.m_msyData.nGroups, FishingMort, TotVal)
            Dim t As cFoptTracker = Nothing

            For igrp = 1 To Me.m_msyData.nGroups
                result.B(igrp) = bb(igrp)
                result.Catch(igrp) = bb(igrp) * Me.m_simData.FishTime(igrp)
                result.FishingMort(igrp) = Me.m_simData.FishTime(igrp)
            Next

            If Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Groups Then
                'In GROUPS mode 
                'only get the F and C msy for the selected group
                igrp = Me.m_msyData.iSelGroupFleet
                t = Me.m_FOptTracker(igrp)
                t.Track(Me.m_simData.FishTime(igrp), result.Catch(igrp))

            ElseIf Me.m_msyData.FSelectionMode = eMSYFSelectionModeType.Fleets Then
                'in FLEET mode 
                'In this case FishingMort is the relative multiplier used to calculate F from the Ecopath base
                'So in the eMSYFSelectionModeType.Fleets all Fishing Mortality in the interface is the multiplier relative to the base 
                'This is kind of dumb cause the interface can't turn this into a proper F for the group
                For igrp = 1 To Me.m_msyData.nGroups
                    t = Me.m_FOptTracker(igrp)
                    t.Track(FishingMort, result.Catch(igrp))
                Next

            End If

            Return result

        End Function

        Private Sub GetOptimumResults()

            Dim r As New cMSYOptimum(Me.m_msyData.nGroups)
            Dim t As cMSY.cFoptTracker = Nothing

            For i As Integer = 1 To Me.m_msyData.nGroups
                t = Me.m_FOptTracker(i)
                r.FOpt(i) = t.FOpt
                r.IsFopt(i) = t.IsFopt()
            Next
            Me.m_msyData.Optimum = r
        End Sub

#End Region ' Internals

#Region " Optimum detection "

        ''' <summary>
        ''' Helper class that tracks whether a local optimum has been reached over
        ''' a given set of samples.
        ''' </summary>
        Friend Class cFoptTracker

            Private Const SAMPLESIZE As Integer = 3
            Private m_lSamples As New List(Of Single)
            Private m_iGroup As Integer

            Private m_sCatchMax As Single = 0
            Private m_sFMax As Single = 0

            Public Sub New(ByVal iGroup As Integer)
                Me.m_iGroup = iGroup
            End Sub

            Public Sub Track(ByVal sF As Single, ByVal sCatch As Single)

                ' Fifo: add sample, and kick out old samples
                Me.m_lSamples.Add(sCatch)
                While Me.m_lSamples.Count > SAMPLESIZE
                    Me.m_lSamples.RemoveAt(0)
                End While

                ' Update max
                If (sCatch > m_sCatchMax) Then
                    Me.m_sCatchMax = sCatch
                    Me.m_sFMax = sF
                    'Me.m_bIsFOpt = False
                    'Console.WriteLine(Me.m_iGroup & ": new max " & Me.m_sCatchMax & " found at F " & Me.m_sFMax)
                    'Console.WriteLine(Me.m_iGroup & ": opt invalidated")
                    'Else
                    '    ' Produce some output for testing purposes
                    '    Dim bIsOpt As Boolean = Me.IsFopt()
                    '    If (bIsOpt <> Me.m_bIsFOpt) Then
                    '        Me.m_bIsFOpt = bIsOpt
                    '        If bIsOpt = True Then
                    '            Console.WriteLine(Me.m_iGroup & ": opt confirmed")
                    '        Else
                    '            Console.WriteLine(Me.m_iGroup & ": opt invalidated")
                    '        End If
                    '    End If
                End If
            End Sub

            Public Function IsFopt() As Boolean

                Dim sMean As Single = 0
                Dim nResults As Integer = m_lSamples.Count

                If (nResults < SAMPLESIZE) Then Return False

                For Each s As Single In Me.m_lSamples
                    ' Max cannot be among one of the recent samples
                    If s = Me.m_sCatchMax Then Return False
                    sMean += s
                Next
                sMean /= SAMPLESIZE

                Return Me.m_sCatchMax > sMean

            End Function

            Public Function CatchOpt() As Single
                Return Me.m_sCatchMax
            End Function

            Public Function FOpt() As Single
                Return Me.m_sFMax
            End Function

        End Class

#End Region ' Optimum detection

#Region " Progress reporting "

        Private Sub StartProgress(ByVal strStatus As String, ByVal iNumSteps As Integer)
            Try

                Me.ChangeRunState(eMSYRunStates.MSYRunStarted)

                If (Me.m_MessageDelegate IsNot Nothing) Then

                    Select Case Me.m_msyData.RunLengthMode
                        Case eMSYRunLengthModeTypes.FixedF
                            Me.m_iNumSteps = iNumSteps
                        Case eMSYRunLengthModeTypes.ToDepletion
                            ' NumSteps unknown, which is indicated by a negative value
                            Me.m_iNumSteps = -1
                    End Select
                    Me.m_strStatus = strStatus
                    Me.m_iStep = 1
                    Me.m_MessageDelegate.Invoke(New EwECore.cProgressMessage(eProgressState.Start, Me.m_iNumSteps, Me.m_iStep, strStatus))
                End If
            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".StartProgress() Exception: " & ex.Message)
            End Try

        End Sub

        Private Sub IncrementProgress(Optional strStatus As String = "")
            Try

                If (Me.m_MessageDelegate IsNot Nothing) Then
                    If Not String.IsNullOrWhiteSpace(strStatus) Then
                        Me.m_strStatus = strStatus
                    End If
                    Me.m_iStep += 1
                    Me.m_MessageDelegate.Invoke(New EwECore.cProgressMessage(eProgressState.Running, Me.m_iNumSteps, Me.m_iStep, Me.m_strStatus))
                End If
            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".IncrementProgress() Exception: " & ex.Message)
            End Try

        End Sub

        Private Sub EndProgress()
            Try
                'Figure out the run state from the AssessmentType
                Dim runState As eMSYRunStates
                If Me.m_msyData.AssessmentType = eMSYAssessmentTypes.FullCompensation Then
                    runState = eMSYRunStates.FullCompRunCompleted
                ElseIf Me.m_msyData.AssessmentType = eMSYAssessmentTypes.StationarySystem Then
                    runState = eMSYRunStates.StationaryRunCompleted
                End If
                Me.ChangeRunState(runState)

                If (Me.m_MessageDelegate IsNot Nothing) Then
                    Me.m_MessageDelegate.Invoke(New EwECore.cProgressMessage(eProgressState.Finished, Me.m_iNumSteps, Me.m_iNumSteps, ""))
                End If

            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".EndProgress() Exception: " & ex.Message)
            End Try

        End Sub

        ''' <summary>
        ''' Inform the user interface that something is happening
        ''' </summary>
        ''' <param name="runState"></param>
        Private Sub ChangeRunState(runState As eMSYRunStates)

            Try
                'I don't think this should happen
                'If it does better figure out why
                Debug.Assert(Me.m_RunStateDelegate IsNot Nothing, Me.ToString & ".ChangeRunState(eMSYRunStates) RunStateDelegate = Nothing!")
                If Me.m_RunStateDelegate IsNot Nothing Then
                    Me.m_RunStateDelegate.Invoke(runState)
                End If
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Progress reporting

    End Class

End Namespace



