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
Imports System.IO
Imports EwECore.Database
Imports EwECore.DataSources
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' Class to export an Ecosim time step to a new Ecopath model.
''' </summary>
Public Class cEcopathModelFromEcosim

#Region " Private variables "

    ''' <summary>The core that holds the source model.</summary>
    Private m_core As cCore = Nothing

    ''' <summary>Progress of a run.</summary>
    Private m_msgStatus As cMessage = Nothing

#End Region ' Private variables

#Region " Construction "

    Public Sub New(ByVal core As cCore)
        Me.m_core = core
    End Sub

#End Region ' Construction

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type indicating how BA should be calculated.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eBACalcTypes
        ''' <summary>BA is calculated from an average over X number of years.</summary>
        FromEcosimYearsAverage

        FromEcosimYearsWeightedAverage
        ''' <summary>BA is taken as the change in group biomass over the Ecosim run.</summary>
        FromEcosimStart
        ''' <summary>BA kept at Ecopath base value.</summary>
        FromEcopath
        ''' <summary>BA is set to 0.</summary>
        SetToZero
    End Enum

    Public Function InitRun(strOutputPath As String) As Boolean

        Me.m_msgStatus = New cMessage(My.Resources.CoreMessages.MODELFROMSIM_GENERATED, eMessageType.DataExport, eCoreComponentType.EcoSim, eMessageImportance.Information)
        Me.m_msgStatus.Hyperlink = strOutputPath
        Return True

    End Function

    Public Function EndRun() As Boolean

        If (Me.m_msgStatus IsNot Nothing) Then
            If (Me.m_msgStatus.Variables.Count > 0) Then
                Me.m_core.Messages.SendMessage(Me.m_msgStatus)
            End If
            Me.m_msgStatus = Nothing
        End If
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a model from the current Ecosim time step.
    ''' </summary>
    ''' <param name="strFileName">Full path to the model file to create.</param>
    ''' <param name="strModelName">Name of the model to create.</param>
    ''' <param name="iTime">The Ecosim time step to populate data from.</param>
    ''' <param name="BACalculation"><see cref="eBACalcTypes">Flag</see> 
    ''' stating how BA should be calculated.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function SaveModel(ByVal strFileName As String,
                              ByVal strModelName As String,
                              ByVal iTime As Integer,
                              ByVal BACalculation As eBACalcTypes,
                              ByVal iNumYearsAverage As Integer,
                              ByVal WeightPower As Single) As eDatasourceAccessType

        Dim atResult As eDatasourceAccessType = eDatasourceAccessType.Failed_Unknown

        Try

            Dim coreDest As New cCore()
            Dim db As cEwEDatabase = New cEwEAccessDatabase()
            Dim bSucces As Boolean = False

            coreDest.PluginManager = Nothing

            If String.IsNullOrEmpty(Path.GetExtension(strFileName)) Then
                strFileName &= cDataSourceFactory.GetDefaultExtension(eDataSourceTypes.Access2003)
            End If

            atResult = db.Create(strFileName, strModelName, True, strAuthor:=Me.m_core.DefaultAuthor)
            If (atResult = eDatasourceAccessType.Created) Then

                Dim ds As IEwEDataSource = cDataSourceFactory.Create(strFileName)
                If ds.Open(strFileName, coreDest) = eDatasourceAccessType.Opened Then
                    If coreDest.LoadModel(ds) Then
                        If Me.CreateItems(coreDest) Then
                            Me.PopulateItems(coreDest, iTime, BACalculation, iNumYearsAverage, WeightPower)
                        End If
                    End If
                End If

                coreDest.CloseModel()

                db = Nothing
                ds = Nothing
                coreDest = Nothing

            End If

        Catch ex As Exception
            atResult = eDatasourceAccessType.Failed_Unknown
            cLog.Write(ex)
        End Try

        Return atResult

    End Function

    Public Sub LogStatus(strStatus As String, status As eStatusFlags)

        Debug.Assert(Not Me.m_msgStatus Is Nothing)

        Dim vs As New cVariableStatus(status, strStatus, eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0)
        Me.m_msgStatus.AddVariable(vs)

    End Sub

#End Region ' Public access

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create groups, fleets and stanza configurations in the new Ecopath model.
    ''' </summary>
    ''' <param name="coreNew">The core that holds the new Ecopath model.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function CreateItems(ByVal coreNew As cCore) As Boolean

        Dim bSuccess As Boolean = True
        Dim pathSrc As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim stanzaSrc As cStanzaDatastructures = Me.m_core.m_Stanza
        Dim taxonSrc As cTaxonDataStructures = Me.m_core.m_TaxonData

        Dim aiGroupID(pathSrc.NumGroups) As Integer
        Dim aiFleetID(pathSrc.NumFleet) As Integer
        Dim aiStanzaID(stanzaSrc.Nsplit) As Integer
        Dim aiTaxonID(taxonSrc.NumTaxon) As Integer

        If Not coreNew.SetBatchLock(cCore.eBatchLockType.Restructure) Then Return False

        ' Items are created in the new model in exactly the same order as they occur
        ' in the source model. That way a simple data structure copy is warranted
        ' for transferring object details as long as database keys are left unique to
        ' the new model.

        Try
            ' Delete default group(s) and fleet(s)
            For iGroup As Integer = 1 To coreNew.nGroups
                coreNew.RemoveGroup(iGroup)
            Next

            For iFleet As Integer = 1 To coreNew.nFleets
                coreNew.RemoveFleet(iFleet)
            Next

            For iGroup As Integer = 1 To pathSrc.NumGroups
                Dim iNew As Integer = iGroup
                Dim iIDNew As Integer = 0
                bSuccess = bSuccess And coreNew.AddGroup(pathSrc.GroupName(iGroup), pathSrc.PP(iGroup), pathSrc.vbK(iGroup), iNew, iIDNew)
                aiGroupID(iGroup) = iIDNew
            Next

            For iFleet As Integer = 1 To pathSrc.NumFleet
                Dim iNew As Integer = iFleet
                Dim iIDNew As Integer = 0
                bSuccess = bSuccess And coreNew.AddFleet(pathSrc.FleetName(iFleet), iNew, iIDNew)
                aiFleetID(iFleet) = iIDNew
            Next

            For iStanza As Integer = 1 To Me.m_core.nStanzas

                Dim NStanza As Integer = stanzaSrc.Nstanza(iStanza)
                Dim aiLifeStageID(NStanza - 1) As Integer
                Dim aiLifeStageAge(NStanza - 1) As Integer
                Dim iIDNew As Integer = 0

                For iLifeStage As Integer = 1 To NStanza
                    Dim iGroup As Integer = stanzaSrc.EcopathCode(iStanza, iLifeStage)
                    aiLifeStageID(iLifeStage - 1) = aiGroupID(iGroup)
                    aiLifeStageAge(iLifeStage - 1) = stanzaSrc.Age1(iStanza, iLifeStage)
                Next
                bSuccess = bSuccess And coreNew.AppendStanza(stanzaSrc.StanzaName(iStanza), aiLifeStageID, aiLifeStageAge, iIDNew)
                aiStanzaID(iStanza) = iIDNew
            Next

        Catch ex As Exception
            bSuccess = False
        End Try

        coreNew.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecopath, bSuccess)

        ' Define taxa in a second step AFTER all groups and stanza have been created
        coreNew.SetBatchLock(cCore.eBatchLockType.Restructure)
        For iTaxon As Integer = 1 To Me.m_core.nTaxon
            Dim iIDNew As Integer = 0
            If taxonSrc.IsTaxonStanza(iTaxon) Then
                bSuccess = bSuccess And coreNew.AddTaxon(taxonSrc.TaxonTarget(iTaxon), True, Nothing, 1, iIDNew)
            Else
                bSuccess = bSuccess And coreNew.AddTaxon(taxonSrc.TaxonTarget(iTaxon), False, Nothing, taxonSrc.TaxonPropBiomass(iTaxon), iIDNew)
            End If
            aiTaxonID(iTaxon) = iIDNew
        Next
        coreNew.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecopath, bSuccess)

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Populate groups, fleets and stanza configurations in a new Ecopath model.
    ''' </summary>
    ''' <param name="coreNew">The core that holds the new model.</param>
    ''' <param name="iTime">The Ecosim time step to populate data from.</param>
    ''' <param name="BACalculation"><see cref="eBACalcTypes">Flag</see> 
    ''' stating how BA should be calculated.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function PopulateItems(ByVal coreNew As cCore, _
                                   ByVal iTime As Integer, _
                                   ByVal BACalculation As eBACalcTypes, _
                                   ByVal nNumYearsAverage As Integer, _
                                   ByVal WeightPower As Single) As Boolean

        Debug.Assert(iTime >= cCore.N_MONTHS, Me.ToString & ".PopulateItems(...) iTime must fall after the first year.")

        Dim pathSrc As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim pathDest As cEcopathDataStructures = coreNew.m_EcoPathData
        Dim GroupDBIDs(coreNew.nGroups) As Integer
        Dim FleetDBIDs(coreNew.nFleets + 1) As Integer

        Dim stanzaSrc As cStanzaDatastructures = Me.m_core.m_Stanza
        Dim stanzaDest As cStanzaDatastructures = coreNew.m_Stanza
        Dim StanzaDBIDs(coreNew.nStanzas) As Integer
        Dim TaxonDBIDs(coreNew.nTaxon) As Integer

        Dim taxonSrc As cTaxonDataStructures = Me.m_core.m_TaxonData
        Dim taxonDest As cTaxonDataStructures = coreNew.m_TaxonData

        Dim simSrc As cEcosimDatastructures = Me.m_core.m_EcoSimData

        Dim bSuccess As Boolean = True
        Dim BiomassAtT As Single

        'number of time steps to average BA over
        Dim nBAtimesteps As Integer

        'Time index for the start year of the BA averaging
        Dim iStartIndex As Integer

        Dim StepsPerYear As Integer = cCore.N_MONTHS * simSrc.StepsPerMonth

        'Number of years up to current t
        Dim nYears As Integer = iTime \ StepsPerYear

        'Set time step indexes for averaging BA
        Me.SetStartEndTimesteps(iTime, BACalculation, nNumYearsAverage, iStartIndex, nBAtimesteps)

        ' Dirty destination core
        coreNew.DataSource.SetChanged(eCoreComponentType.EcoPath)
        coreNew.StateMonitor.UpdateDataState(coreNew.DataSource)

        ' Copy Ecopath data but do not redim - preserve original data such as DBIDs
        Array.Copy(pathDest.GroupDBID, GroupDBIDs, pathDest.GroupDBID.Length)
        Array.Copy(pathDest.FleetDBID, FleetDBIDs, pathDest.FleetDBID.Length)
        Array.Copy(stanzaDest.StanzaDBID, StanzaDBIDs, stanzaDest.StanzaDBID.Length)
        Array.Copy(taxonDest.TaxonDBID, TaxonDBIDs, taxonDest.TaxonDBID.Length)

        ' Copy bulk of data
        pathSrc.copyTo(pathDest, False)
        stanzaSrc.copyTo(stanzaDest)
        taxonSrc.copyto(taxonDest)

        ' Restore DBIDs
        Array.Copy(GroupDBIDs, pathDest.GroupDBID, pathDest.GroupDBID.Length)
        Array.Copy(FleetDBIDs, pathDest.FleetDBID, pathDest.FleetDBID.Length)
        Array.Copy(StanzaDBIDs, stanzaDest.StanzaDBID, stanzaDest.StanzaDBID.Length)
        Array.Copy(TaxonDBIDs, taxonDest.TaxonDBID, taxonDest.TaxonDBID.Length)

        ' Clear data that is not going to be copied
        pathDest.NumEcosimScenarios = 0
        pathDest.NumEcospaceScenarios = 0
        pathDest.NumEcotracerScenarios = 0
        pathDest.NumPedigreeLevels = 0
        pathDest.NumPedigreeVariables = 0

        ' Overwrite bits with Ecosim data at time step 'iTime'
        Dim sArea As Single = Me.m_core.EwEModel.Area

        Dim simBB() As Single = Me.m_core.m_EcoSim.BB

        ' Populate groups
        For iGroup As Integer = 1 To Me.m_core.nGroups

            'jb 20-Nov-2012 remove DCPct() and populate the Ecopath variable directly from the Ecosim Variables
            'this makes it easier to tell what and how the Ecopath value are computed from the current Ecosim run
            pathDest.Binput(iGroup) = simBB(iGroup) 'simSrc.DCPct(iGroup, 1)
            ' Catch(i) = Bi(i) * FishTime(i)
            pathDest.fCatch(iGroup) = simBB(iGroup) * simSrc.FishTime(iGroup)
            ' Ex(i) = Catch(i)
            pathDest.Ex(iGroup) = pathDest.fCatch(iGroup)

            ' PBi(i) = loss(i) / Bi(i)
            pathDest.PBinput(iGroup) = simSrc.loss(iGroup) / simBB(iGroup)
            ' QBi(i) = DCPct(i, 2) 'the following has been updated: Eatenby(i) / bb(i)
            pathDest.QBinput(iGroup) = simSrc.Eatenby(iGroup) / simBB(iGroup) ' simSrc.DCPct(iGroup, 2)
            ' EEi(i) = -99
            pathDest.EEinput(iGroup) = cCore.NULL_VALUE

            ' BAi(i) = (Bi(i) - DCPct(i, 0)) * StepsPerYear ' / TimeStep 'dcpct() stores the bb() from previous round
            Select Case BACalculation

                Case eBACalcTypes.FromEcosimYearsAverage
                    BiomassAtT = simSrc.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGroup, iStartIndex)
                    pathDest.BAInput(iGroup) = (simBB(iGroup) - BiomassAtT) / nNumYearsAverage
                    pathDest.BaBi(iGroup) = 0

                Case eBACalcTypes.FromEcosimYearsWeightedAverage
                    Dim b1 As Single, b2 As Single, w As Single, bsum As Single, wsum As Single
                    'Inverse distance weighted average
                    For i As Integer = 0 To nBAtimesteps - 2
                        'inverse distance weight
                        w = CSng(1 / (nBAtimesteps - (i + 1)) ^ WeightPower)
                        'BA
                        b1 = simSrc.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGroup, iStartIndex + i)
                        b2 = simSrc.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGroup, iStartIndex + i + 1)
                        'sum of weighted BA
                        bsum += (b2 - b1) * w
                        'sum of weight
                        wsum += w
                    Next
                    If wsum = 0.0 Then wsum = 1
                    'Weighted monthly average converted to annual BA for Ecopath
                    pathDest.BAInput(iGroup) = CSng(bsum / wsum * StepsPerYear)
                    pathDest.BaBi(iGroup) = 0

                Case eBACalcTypes.FromEcosimStart
                    'BA is the Annual Accumulation of B 
                    'So get the annual average accumulation (B(t)-B(0))/ number of years
                    'Attributes the annual average change in Biomass to BiomassAccumulation
                    pathDest.BAInput(iGroup) = (simBB(iGroup) - pathSrc.B(iGroup)) / nYears
                    pathDest.BaBi(iGroup) = 0

                Case eBACalcTypes.FromEcopath
                    'Explicitly copy BA and BA rate from the Ecopath source so you can tell it worked
                    pathDest.BAInput(iGroup) = pathSrc.BA(iGroup)
                    pathDest.BaBi(iGroup) = pathSrc.BaBi(iGroup)

                Case eBACalcTypes.SetToZero
                    pathDest.BAInput(iGroup) = 0
                    pathDest.BaBi(iGroup) = 0

            End Select

            ' Emigrationi(i) = Emig(i) * Bi(i) '
            pathDest.Emigration(iGroup) = pathDest.Emig(iGroup) * simBB(iGroup)
            ' BHi(i) = Bi(i) / Area(i)
            pathDest.BHinput(iGroup) = simBB(iGroup) / pathSrc.Area(iGroup)

        Next

        For iPred As Integer = 1 To Me.m_core.nGroups
            For iPrey As Integer = 1 To Me.m_core.nGroups
                'DCi(i, j) = 0        'don't leave any dc leftovers
                pathDest.DCInput(iPred, iPrey) = 0

                If simSrc.Eatenby(iPred) > 0 Then
                    'simDCAtT(pred,prey) contains biomass eaten by a predator on a prey populated in derivt()
                    'Eatenby(pred) is the total biomass eaten by a predator
                    'DC(pred,prey) is the proportion of diet made up by a prey
                    'So get the proportion of diet 
                    pathDest.DCInput(iPred, iPrey) = simSrc.simDCAtT(iPred, iPrey) / simSrc.Eatenby(iPred)
                End If

            Next
        Next
        pathDest.SumDCToOne()

        'immigration is constant rate and is not changed by ecosim so no need to change
        For i As Integer = 1 To Me.m_core.nGroups
            Dim SumEf As Single = 0.0
            For j As Integer = 1 To pathSrc.NumFleet
                ' SumEf = SumEf + FishRateGear(j, itime) * FishMGear(j, i)
                SumEf += simSrc.FishRateGear(j, iTime) * simSrc.FishMGear(j, i)
            Next
            For j As Integer = 1 To Me.m_core.nFleets
                Dim Sum As Single = 0
                Dim Z As Single = pathSrc.Landing(j, i) + pathSrc.Discard(j, i)
                ' If SumEf > 0 Then Sum = BB(i) * FishTime(i) * FishRateGear(j, iTime) * FishMGear(j, i) / SumEf
                If SumEf > 0 And Z > 0 Then
                    Dim BB As Single = simBB(i) 'results.Biomass(i) * simSrc.StartBiomass(i)
                    Sum = BB * simSrc.FishTime(i) * simSrc.FishRateGear(j, iTime) * simSrc.FishMGear(j, i) / SumEf
                    pathDest.Landing(j, i) = Sum * pathSrc.Landing(j, i) / Z
                    pathDest.Discard(j, i) = Sum * pathSrc.Discard(j, i) / Z
                Else
                    pathDest.Landing(j, i) = 0
                    pathDest.Discard(j, i) = 0
                End If
            Next j
        Next i

        coreNew.SaveChanges(True, cCore.eBatchChangeLevelFlags.Ecopath)

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Determine the start and end time steps for computing BA.
    ''' </summary>
    ''' <param name="iTime"></param>
    ''' <param name="BACalculation"></param>
    ''' <param name="nYearsAverage"></param>
    ''' <param name="iStartIndex"></param>
    ''' <param name="nBAtimesteps"></param>
    ''' -----------------------------------------------------------------------
    Private Sub SetStartEndTimesteps(ByVal iTime As Integer, ByVal BACalculation As eBACalcTypes, ByVal nYearsAverage As Integer, _
                                    ByRef iStartIndex As Integer, ByRef nBAtimesteps As Integer)

        'number of Ecosim time steps per year
        Dim nStepsPerYear As Integer = cCore.N_MONTHS * Me.m_core.m_EcoSimData.StepsPerMonth

        'number of time steps to average BA over
        nBAtimesteps = nYearsAverage * nStepsPerYear
        'Time index for the start year of the BA averaging
        iStartIndex = iTime - nBAtimesteps + 1 'Time indexes are one based

        ' JS 01May13: only send out messages if BACalculation requires the time step range
        Select Case BACalculation
            Case eBACalcTypes.FromEcopath, eBACalcTypes.FromEcosimStart, eBACalcTypes.SetToZero
                Return
            Case eBACalcTypes.FromEcosimYearsAverage, eBACalcTypes.FromEcosimYearsWeightedAverage
                ' NOP
            Case Else
                Debug.Assert(False, "BA calculation type not explicitly considered")
        End Select

        'Constrain the start and end years
        If (iStartIndex < 1) Then
            Dim vs As cVariableStatus
            vs = New cVariableStatus(eStatusFlags.FailedValidation, My.Resources.CoreMessages.MODELFRIMSIM_BA_STARTYEAR_ADJ, _
                                     eVarNameFlags.NotSet, eDataTypes.EwEModel, eCoreComponentType.EcoSim, -1)
            Me.m_msgStatus.AddVariable(vs)
            iStartIndex = 1
        End If

        If ((iStartIndex + nBAtimesteps - 1) > iTime) Then
            Dim vs As cVariableStatus
            vs = New cVariableStatus(eStatusFlags.FailedValidation, My.Resources.CoreMessages.MODELFRIMSIM_BA_ENDYEAR_ADJ, eVarNameFlags.NotSet, eDataTypes.EwEModel, eCoreComponentType.EcoSim, -1)
            Me.m_msgStatus.AddVariable(vs)
            nBAtimesteps = iTime - iStartIndex + 1
        End If

    End Sub

#End Region ' Internals

#Region " Original code "

#If 0 Then ' From Ecopath v5, modSimEdit

Public Sub SaveEcopathFromEcosim()
Dim i As Integer
Dim j As Integer
Dim SaveRunFile As String
Dim SBi() As Double
Dim SBHi() As Double   'habitat biomass
Dim SCatch() As Single
Dim SEx() As Single
Dim SPBi() As Double
Dim SQBi() As Double
Dim SDC() As Single
Dim SEE() As Single
Dim SBAi() As Single
Dim SEmi() As Single
Dim SImmi() As Single
Dim SLandi() As Single
Dim SDisci() As Single
Dim titi As String
Dim Response As Variant
    ReDim SBi(NumGroups) As Double
    ReDim SBHi(NumGroups) As Double   'habitat biomass
    ReDim SCatch(NumGroups) As Single
    ReDim SEx(NumGroups) As Single
    ReDim SPBi(NumGroups) As Double
    ReDim SQBi(NumGroups) As Double
    ReDim SDC(NumGroups + 1, NumGroups + 1) As Single
    ReDim SEE(NumGroups) As Single
    ReDim SBAi(NumGroups) As Single
    ReDim SEmi(NumGroups) As Single
    ReDim SImmi(NumGroups) As Single
    ReDim SLandi(NumGear, NumGroups) As Single
    ReDim SDisci(NumGear, NumGroups) As Single
    Dim t As Variant
    For i = 1 To NumGroups
        SBi(i) = Bi(i)
        Bi(i) = DCPct(i, 1)
        SCatch(i) = Catch(i)
        Catch(i) = Bi(i) * FishTime(i)
        SEx(i) = Ex(i)
        Ex(i) = Catch(i)
        SPBi(i) = PBi(i)
        PBi(i) = loss(i) / Bi(i)
        SQBi(i) = QBi(i)
        QBi(i) = DCPct(i, 2) 'the following has been updated: Eatenby(i) / bb(i)
        SEE(i) = EEi(i)
        EEi(i) = -99
        SBAi(i) = BAi(i)
        BAi(i) = (Bi(i) - DCPct(i, 0)) * StepsPerYear ' / TimeStep 'dcpct() stores the bb() from previous round
        'BAi(i) = DCPct(i, 3) * StepsPerYear '/ TimeStep
        SEmi(i) = Emigrationi(i)
        Emigrationi(i) = Emig(i) * Bi(i) '
        SBHi(i) = BHi(i)
        BHi(i) = Bi(i) / Area(i)
    Next
    For i = 1 To NumGroups
        For j = 1 To NumGroups
            SDC(i, j) = DC(i, j)
            DCi(i, j) = 0        'don't leave any dc leftovers
            If QBi(i) > 0 Then DCi(i, j) = DCMean(i, j) '/ (QBi(i) * Bi(i))
        Next
    Next
    'immigration is constant rate and is not changed by ecosim so no need to change
    For i = 1 To NumGear
        For j = 1 To NumGroups
            SLandi(i, j) = Landing(i, j)
            Landing(i, j) = DCMin(i, j)
            SDisci(i, j) = Discard(i, j)
            Discard(i, j) = DCMax(i, j)
        Next j
    Next i
    titi = modelRemarks
    modelRemarks = "Ecosim output file; " + CStr(Date) + "; " + CStr(time) + "; " + modelRemarks

    GetValidFileName SaveRunFile
    If Mid(dbFilepath, Len(dbFilepath), 1) <> "\" Then
        SaveRunFile = dbFilepath + "\" + SaveRunFile + ".eii" 'Left(lastModel, 8) + ".txt"
    Else
        SaveRunFile = dbFilepath + SaveRunFile + ".eii"  'Left(lastModel, 8) + ".txt"
    End If

    'SaveEiiFile SaveRunFile
    Response = "Ecopath file saved to " + SaveRunFile + Environment.NewLine  + Environment.NewLine  + "You can import the file as a text-file (eii) from the File menu" + Environment.NewLine  + "Do you want to keep this file?"
    Response = MsgBox(Response, vbInformation + vbYesNo, "Save Ecopath model from Ecosim")
    If Response = vbYes Then SaveEiiFile SaveRunFile
    modelRemarks = titi
    Erase DCMin(), DCMean(), DCMax()
    'Restore Ecopath parameters
    For i = 1 To NumGroups
        Bi(i) = SBi(i)
        Catch(i) = SCatch(i)
        Ex(i) = SEx(i)
        PBi(i) = SPBi(i)
        QBi(i) = SQBi(i)
        EEi(i) = SEE(i)
        BAi(i) = SBAi(i)
        Emigrationi(i) = SEmi(i)
        BHi(i) = SBHi(i)
    Next
    For i = 1 To NumGroups
        For j = 1 To NumGroups
            DCi(i, j) = SDC(i, j)
        Next
    Next
    For i = 1 To NumGear
        For j = 1 To NumGroups
            Landing(i, j) = SLandi(i, j)
            Discard(i, j) = SDisci(i, j)
        Next j
    Next i
End Sub
#End If
#End Region ' Original code

End Class
