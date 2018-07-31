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
Imports EwECore.WebServices
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports System.IO
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports System.Net

#End Region ' Imports 

Namespace Database

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Imports a model from Ecobase into an EwE6 database
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEcobaseImporter
        Inherits cModelImporter

#Region " Private vars "

        ''' <summary>Data buffer.</summary>
        Private m_data As Ecobase.cEcobaseModelParameters = Nothing
        Private m_bHasLoaded As Boolean = False

#End Region ' Private vars

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' <param name="core">The core to import into.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal core As cCore)
            MyBase.New(core)
        End Sub

#End Region ' Construction

#Region " Overrides "

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Open"/>
        ''' <param name="strSource">The key of the Ecobase model to open.</param>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Open(ByVal strSource As String) As Boolean

            If strSource.ToLower().StartsWith("ewe-ecobase:") Then
                strSource = strSource.Substring(strSource.IndexOf(":"c) + 1)
            End If

            Me.m_strSource = strSource

            Try
                If File.Exists(Me.m_strSource) Then
                    Using reader As New StreamReader(Me.m_strSource)
                        Me.m_data = Ecobase.cEcobaseModelParameters.FromXML(reader.ReadToEnd())
                    End Using
                Else
                    Dim wdsl As New cEcoBaseWDSL()
                    Dim strModel As String = wdsl.getModel("all_data", Integer.Parse(strSource))
                    Me.m_data = Ecobase.cEcobaseModelParameters.FromXML(strModel)
                End If
                Me.m_bHasLoaded = (Me.m_data IsNot Nothing)
            Catch ex As Exception
                Me.m_bHasLoaded = False
            End Try

            Return Me.m_bHasLoaded

        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Close"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub Close()

            Me.m_data = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.IsOpen"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function IsOpen() As Boolean

            Return Me.m_bHasLoaded

        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Models"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Models() As cExternalModelInfo()

            If Not Me.m_bHasLoaded Then Return Nothing

            Dim info As New cExternalModelInfo(Me.m_data.Model.EcobaseCode, Me.m_data.Model.Name, Me.m_data.Model.Country, 0)
            Return New cExternalModelInfo() {info}

        End Function

        Public Overrides Function CanImportFrom(ByVal strSource As String) As Boolean
            ' Obtain model from source URL here
            Return True
        End Function

#End Region ' Overrides 

#Region " The import "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Imports and converts a model in an EwE5 database into a provided EwE6 database.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function PerformImport() As Boolean

            ' Pre
            Debug.Assert(Me.m_dbTarget IsNot Nothing, "Needs a valid EwE6 database instance")
            Debug.Assert(Me.m_dbTarget.GetConnection().State = ConnectionState.Open, "EwE6 database must already be open")

            Dim dbUpd As cDatabaseUpdater = Nothing

            ' Set progress info (fixed)
            Me.m_iNumSteps = 10
            Me.m_iStep = 0

            ' Start the actual import process.
            ' Note that VB6 function names are used here to make it easier to map to the old code.

            ' -------
            ' ECOPATH
            ' -------

            Me.LogProgress(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_PROGRESS_MODEL, Me.m_strModelName))
            Me.ImportModel()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOPATHGROUPS)
            Me.ImportGroups()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_STANZA)
            Me.ImportGroupStanza()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_POGRESS_DIETCOMP)
            Me.ImportDiets()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FLEET)
            Me.ImportFleets()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH)
            Me.ImportCatch()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH)
            Me.ImportDiscardFate()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_PEDIGREE)
            Me.ImportPedigree()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_PEDIGREE)
            Me.ImportPedigreeAssignments()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_TAXONOMY)
            Me.ImportTaxonomy()

            ' Set version
            Me.m_dbTarget.SetVersion(Me.m_dbTarget.GetVersion(), "Imported from Ecobase model '" & Me.m_strSource & "'")

            ' Now run all available updates on the new EwE6 database
            dbUpd = New cDatabaseUpdater(Me.m_core, 6.0!)
            dbUpd.UpdateDatabase(Me.m_dbTarget)
            dbUpd = Nothing

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_COMPLETE, Me.m_iNumSteps)

            Return True

        End Function

#End Region ' The import 

#Region " Loading "

        ''' <summary>
        ''' Obtain model data from Ecobase
        ''' </summary>
        ''' <returns></returns>
        Private Function LoadModel() As Boolean

            Return True

        End Function

#End Region ' Loading

        Private Function ImportModel() As Boolean

            Dim md As Ecobase.cModelData = Me.m_data.Model
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strUnit As String = ""
            Dim unitCurrency As eUnitCurrencyType = 0
            Dim unitTime As eUnitTimeType = 0
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathModel")
            writer = m_dbTarget.GetWriter("EcopathModel")

            Try

                drow = writer.NewRow()

                drow("ModelID") = 1
                drow("Name") = WebUtility.HtmlDecode(md.Name)
                drow("Description") = WebUtility.HtmlDecode(md.Description)
                drow("CodeEcobase") = md.EcobaseCode

                drow("Author") = WebUtility.HtmlDecode(md.Author)
                drow("Contact") = WebUtility.HtmlDecode(md.Contact)

                drow("Area") = md.Area
                drow("NumDigits") = Math.Min(md.NumDigits, 3)
                drow("GroupDigits") = False

                ' Translate Currency unit
                If (md.UnitCurrencyIsCustom) Then
                    unitCurrency = eUnitCurrencyType.CustomNutrient
                    strUnit = md.UnitCurrency
                Else
                    If Not [Enum].TryParse(md.UnitCurrency, unitCurrency) Then
                        unitCurrency = eUnitCurrencyType.WetWeight
                        strUnit = ""
                    End If
                End If
                drow("UnitCurrency") = unitCurrency
                drow("UnitCurrencyCustom") = strUnit

                drow("FirstYear") = md.FirstYear
                drow("NumYears") = md.NumYears

                drow("MinLat") = md.South
                drow("MaxLat") = md.North
                drow("MinLon") = md.West
                drow("MaxLon") = md.East

                drow("Country") = WebUtility.HtmlDecode(md.Country)
                drow("EcosystemType") = md.EcosystemType

                drow("PublicationDOI") = WebUtility.HtmlDecode(md.DOI)
                drow("PublicationURI") = WebUtility.HtmlDecode(md.URI)
                drow("PublicationRef") = WebUtility.HtmlDecode(md.Reference)
                drow("LastSaved") = cDateUtils.DateToJulian()

                drow("UnitTime") = eUnitTimeType.Year
                drow("UnitTimeCustom") = ""
                drow("UnitMonetary") = ""

                writer.AddRow(drow)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportGroups() As Boolean

            Dim gd As Ecobase.cGroupData = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim sTemp As Single = 0.0
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathGroup")
            writer = m_dbTarget.GetWriter("EcopathGroup")

            Try
                For i As Integer = 1 To Me.m_data.NumGroups

                    gd = Me.m_data.Groups(i - 1)
                    drow = writer.NewRow()

                    drow("GroupID") = gd.Index
                    drow("Sequence") = gd.Index
                    drow("GroupName") = WebUtility.HtmlDecode(gd.Name)
                    drow("Type") = gd.PP
                    drow("Area") = gd.Area
                    drow("BiomAcc") = gd.BA
                    drow("BiomAccRate") = gd.BaBi
                    drow("Unassim") = gd.GS
                    drow("DtImports") = gd.DtImp
                    drow("Export") = gd.Ex
                    drow("ImpVar") = gd.ImpVar
                    'drow("GroupIsFish") = ecopathDS.GroupIsFish(iGroup)
                    'drow("GroupIsInvert") = ecopathDS.GroupIsInvert(iGroup)
                    drow("NonMarketValue") = gd.Shadow
                    drow("Respiration") = gd.Respiration

                    'variable with input/output pair only the input gets saved
                    drow("EcoEfficiency") = If(gd.EEIsInput, gd.EE, cCore.NULL_VALUE)
                    drow("OtherMort") = gd.OtherMort
                    drow("ProdBiom") = If(gd.PBIsInput, gd.PB, cCore.NULL_VALUE)
                    drow("ConsBiom") = If(gd.QBIsInput, gd.QB, cCore.NULL_VALUE)
                    drow("ProdCons") = If(gd.GEIsInput, gd.GE, cCore.NULL_VALUE)
                    drow("Biomass") = If(gd.BHIsInput, gd.B, cCore.NULL_VALUE)

                    drow("Immigration") = gd.Immig
                    drow("Emigration") = gd.Emig
                    drow("EmigRate") = gd.EmigRate
                    drow("PoolColor") = gd.Color
                    drow("vbk") = gd.vbK

                    ''PSD
                    'drow("VBK") = ecopathDS.vbK(iGroup)
                    'drow("Tcatch") = psdDS.Tcatch(iGroup)
                    'drow("AinLW") = psdDS.AinLWInput(iGroup)
                    'drow("BinLW") = psdDS.BinLWInput(iGroup)
                    'drow("Loo") = psdDS.LooInput(iGroup)
                    'drow("Winf") = psdDS.WinfInput(iGroup)
                    'drow("t0") = psdDS.t0Input(iGroup)
                    'drow("Tmax") = psdDS.TmaxInput(iGroup)

                    writer.AddRow(drow)

                Next i
            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportGroupStanza() As Boolean

            Dim stz As Ecobase.cStanzaData = Nothing
            Dim stl As Ecobase.cStanzaLifeStageData = Nothing
            Dim writerStanza As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerLifeStages As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM Stanza")

            writerStanza = m_dbTarget.GetWriter("Stanza")
            writerLifeStages = m_dbTarget.GetWriter("StanzaLifeStage")

            Try

                For i As Integer = 1 To Me.m_data.NumStanza

                    stz = Me.m_data.Stanzas(i - 1)

                    ' Store stanza configuration
                    drow = writerStanza.NewRow()

                    drow("StanzaID") = stz.Index
                    drow("StanzaName") = WebUtility.HtmlDecode(stz.Name)
                    drow("RecPower") = stz.RecPower
                    drow("BABsplit") = stz.BaBSplit
                    drow("WmatWinf") = stz.WmatWinf
                    drow("FixedFecundity") = stz.FixedFecundity
                    drow("EggAtSpawn") = stz.EggAtSpawn
                    drow("LeadingLifeStage") = stz.LeadingB
                    drow("LeadingCB") = stz.LeadingQB

                    writerStanza.AddRow(drow)
                    writerStanza.Commit()

                    ' Define life stages
                    For j As Integer = 1 To stz.LifeStages.Count

                        stl = stz.LifeStages(j - 1)
                        drow = writerLifeStages.NewRow()

                        drow("StanzaID") = stz.Index
                        drow("GroupID") = stl.GroupIndex
                        drow("Sequence") = stl.Index
                        drow("AgeStart") = stl.Age
                        drow("Mortality") = stl.Z

                        writerLifeStages.AddRow(drow)

                    Next j
                Next i

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces And _
                Me.m_dbTarget.ReleaseWriter(writerLifeStages, bSucces) And _
                Me.m_dbTarget.ReleaseWriter(writerStanza, bSucces)

        End Function

        Private Function ImportDiets() As Boolean

            Dim pred As Ecobase.cGroupData = Nothing
            Dim diet As Ecobase.cDietData = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathDietComp")
            writer = m_dbTarget.GetWriter("EcopathDietComp")

            Try
                For i As Integer = 1 To Me.m_data.NumGroups
                    pred = Me.m_data.Groups(i - 1)
                    For j As Integer = 0 To pred.Diets.Count - 1
                        diet = pred.Diets(j)
                        If (diet IsNot Nothing) Then

                            drow = writer.NewRow()

                            drow("PredID") = pred.Index
                            drow("PreyID") = diet.PreyIndex
                            drow("Diet") = diet.Amount
                            drow("DetritusFate") = diet.DetritusFate

                            writer.AddRow(drow)

                        End If

                    Next j
                Next i
            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportFleets() As Boolean

            Dim fd As Ecobase.cFleetData = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim i As Integer = 0
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathFleet")
            writer = Me.m_dbTarget.GetWriter("EcopathFleet")

            Try
                For i = 1 To Me.m_data.NumFleets
                    fd = Me.m_data.Fleets(i - 1)

                    drow = writer.NewRow()

                    drow("Sequence") = fd.Index
                    drow("FleetID") = fd.Index
                    drow("FleetName") = WebUtility.HtmlDecode(fd.Name)
                    drow("FixedCost") = fd.FixedCost
                    drow("SailingCost") = fd.SailCost
                    drow("variableCost") = fd.VarCost
                    drow("PoolColor") = fd.Color

                    writer.AddRow(drow)

                Next i

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while importing EcopathFleet", ex.Message))
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportCatch() As Boolean

            Dim fd As Ecobase.cFleetData = Nothing
            Dim cd As Ecobase.cCatchData = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Group catch data into something that is easier to use

            Dim Landing(Me.m_data.NumFleets, Me.m_data.NumGroups) As Single
            Dim Discard(Me.m_data.NumFleets, Me.m_data.NumGroups) As Single
            Dim Market(Me.m_data.NumFleets, Me.m_data.NumGroups) As Single
            Dim PropDiscardMort(Me.m_data.NumFleets, Me.m_data.NumGroups) As Single

            For i As Integer = 1 To Me.m_data.NumFleets
                fd = Me.m_data.Fleets(i - 1)
                For j As Integer = 1 To fd.NumCatches
                    cd = fd.Catches(j - 1)
                    Select Case cd.CatchType
                        Case Ecobase.cCatchData.eCatchType.Landing
                            Landing(fd.Index, cd.GroupIndex) = cd.Amount
                        Case Ecobase.cCatchData.eCatchType.Discards
                            Discard(fd.Index, cd.GroupIndex) = cd.Amount
                        Case Ecobase.cCatchData.eCatchType.Market
                            Market(fd.Index, cd.GroupIndex) = cd.Amount
                        Case Ecobase.cCatchData.eCatchType.PropDiscardMort
                            PropDiscardMort(fd.Index, cd.GroupIndex) = cd.Amount
                    End Select
                Next
            Next

            Me.m_dbTarget.Execute("DELETE * FROM EcopathCatch")
            writer = Me.m_dbTarget.GetWriter("EcopathCatch")

            Try
                For i As Integer = 1 To Me.m_data.NumFleets
                    For j As Integer = 1 To Me.m_data.NumGroups

                        If (Landing(i, j) > 0.0!) Or _
                           (Discard(i, j) > 0.0!) Or _
                           (Market(i, j) > 0.0!) Or _
                           (PropDiscardMort(i, j) > 0.0!) Then

                            drow = writer.NewRow()
                            drow("FleetID") = i
                            drow("GroupID") = j
                            drow("Landing") = Landing(i, j)
                            drow("Discards") = Discard(i, j)
                            drow("Price") = Market(i, j)
                            drow("DiscardMortality") = PropDiscardMort(i, j)
                            writer.AddRow(drow)

                        End If

                    Next j
                Next i

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving catch", ex.Message))
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportDiscardFate() As Boolean

            Dim fd As Ecobase.cFleetData = Nothing
            Dim dd As Ecobase.cDiscardFateData = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathDiscardFate")
            writer = Me.m_dbTarget.GetWriter("EcopathDiscardFate")

            Try
                For i As Integer = 1 To Me.m_data.NumFleets
                    fd = Me.m_data.Fleets(i - 1)
                    For j As Integer = 1 To fd.NumDiscardFate
                        dd = fd.DiscardFates(j - 1)

                        drow = writer.NewRow()
                        drow("FleetID") = fd.Index
                        drow("GroupID") = dd.GroupIndex + Me.m_data.NumLiving
                        drow("DiscardFate") = dd.Amount
                        writer.AddRow(drow)

                    Next j
                Next i

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportPedigree() As Boolean

            Dim md As Ecobase.cModelData = Me.m_data.Model
            Dim pd As Ecobase.cPedigreeData = Nothing
            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim i As Integer = 0
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM Pedigree")
            writer = Me.m_dbTarget.GetWriter("Pedigree")

            Try
                For i = 1 To Me.m_data.NumPedigree

                    pd = Me.m_data.PedigreeLevels(i - 1)

                    If (pd IsNot Nothing) Then

                        drow = writer.NewRow()

                        drow("LevelID") = pd.Index
                        drow("Sequence") = pd.Index
                        drow("LevelName") = WebUtility.HtmlDecode(pd.Name)
                        drow("Description") = pd.Description
                        drow("VarName") = cin.GetVarName(pd.VarName)
                        drow("IndexValue") = pd.IndexValue
                        drow("Confidence") = pd.ConfidenceValue

                        writer.AddRow(drow)

                    End If

                Next i

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving pedigree level", ex.Message))
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportPedigreeAssignments() As Boolean

            Dim gd As Ecobase.cGroupData = Nothing
            Dim pd As Ecobase.cPedigreeAssignmentData = Nothing
            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            bSucces = Me.m_dbTarget.Execute("DELETE * FROM EcopathGroupPedigree")
            writer = Me.m_dbTarget.GetWriter("EcopathGroupPedigree")

            Try

                For i As Integer = 1 To Me.m_data.NumGroups
                    gd = Me.m_data.Groups(i - 1)
                    For j As Integer = 1 To gd.NumPedigreeAssignments
                        pd = gd.PedigreeAssignments(j - 1)

                        If (pd IsNot Nothing) Then

                            drow = writer.NewRow()
                            drow("GroupID") = gd.Index
                            drow("VarName") = cin.GetVarName(pd.VarName)
                            drow("LevelID") = pd.LevelIndex
                            writer.AddRow(drow)

                        End If
                    Next
                Next

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while importing pedigree level", ex.Message))
                bSucces = False
            End Try

            Return bSucces And Me.m_dbTarget.ReleaseWriter(writer, bSucces)

        End Function

        Private Function ImportTaxonomy() As Boolean

            Dim td As Ecobase.cTaxonData = Nothing
            Dim gd As Ecobase.cGroupData = Nothing
            Dim sd As Ecobase.cStanzaData = Nothing
            Dim wr As cEwEDatabase.cEwEDbWriter = Nothing
            Dim lt As New List(Of Ecobase.cTaxonData)
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            bSucces = Me.m_dbTarget.Execute("DELETE FROM EcopathStanzaTaxon") And _
                      Me.m_dbTarget.Execute("DELETE FROM EcopathGroupTaxon") And _
                      Me.m_dbTarget.Execute("DELETE FROM EcopathTaxon")

            wr = Me.m_dbTarget.GetWriter("EcopathTaxon")

            For iGroup As Integer = 1 To Me.m_data.NumGroups
                gd = Me.m_data.Groups(iGroup - 1)
                lt.AddRange(gd.Taxa)
            Next

            For iStanza As Integer = 1 To Me.m_data.NumStanza
                sd = Me.m_data.Stanzas(iStanza - 1)
                lt.AddRange(sd.Taxonomy)
            Next

            Try
                For Each td In lt

                    drow = wr.NewRow()
                    drow("TaxonID") = td.TaxonIndex
                    drow("CodeSAUP") = td.CodeSAUP
                    drow("CodeFB") = td.CodeFB
                    drow("CodeSLB") = td.CodeSLB
                    drow("CodeFAO") = td.CodeFAO
                    drow("CodeLCID") = td.CodeLSID ' Field issue
                    drow("ClassName") = WebUtility.HtmlDecode(td.Class)
                    drow("OrderName") = WebUtility.HtmlDecode(td.Order)
                    drow("FamilyName") = WebUtility.HtmlDecode(td.Family)
                    drow("GenusName") = WebUtility.HtmlDecode(td.Genus)
                    drow("SpeciesName") = WebUtility.HtmlDecode(td.Species)
                    drow("CommonName") = WebUtility.HtmlDecode(td.CommonName)
                    drow("SourceName") = WebUtility.HtmlDecode(td.Source)
                    drow("SourceKey") = WebUtility.HtmlDecode(td.SourceKey)
                    drow("EcologyType") = td.EcologyType
                    drow("OrganismType") = td.OrganismType
                    drow("ConservationStatus") = td.IUCNConservationStatusType
                    drow("Exploited") = td.ExploitationStatusType
                    drow("OccurrenceStatus") = td.OccurrenceStatusType
                    drow("MeanWeight") = td.MeanWeight
                    drow("MeanLength") = td.MeanLength
                    drow("MaxLength") = td.MaxLength
                    drow("Winf") = td.Winf
                    drow("vbgfK") = td.vbk
                    drow("MeanLifeSpan") = td.MeanLifeSpan
                    drow("VulnerabiltyIndex") = td.VulnerabilityIndex
                    'drow("LastUpdated") = taxonDS.TaxonLastUpdated(iTaxon)

                    wr.AddRow(drow)

                Next td

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcopathTaxa", ex.Message))
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_dbTarget.ReleaseWriter(wr, bSucces)

            wr = Me.m_dbTarget.GetWriter("EcopathGroupTaxon")
            Try
                For iGroup As Integer = 1 To Me.m_data.NumGroups
                    gd = Me.m_data.Groups(iGroup - 1)
                    For Each td In gd.Taxa

                        drow = wr.NewRow()
                        drow("TaxonID") = td.TaxonIndex
                        drow("EcopathGroupID") = gd.Index
                        drow("Proportion") = td.PropBiomass
                        drow("PropCatch") = td.PropCatch
                        wr.AddRow(drow)
                    Next
                Next
            Catch ex As Exception
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_dbTarget.ReleaseWriter(wr, bSucces)

            wr = Me.m_dbTarget.GetWriter("EcopathStanzaTaxon")
            Try
                For iStanza As Integer = 1 To Me.m_data.NumStanza
                    sd = Me.m_data.Stanzas(iStanza - 1)
                    For Each td In sd.Taxonomy
                        drow = wr.NewRow()
                        drow("TaxonID") = td.TaxonIndex
                        drow("StanzaID") = sd.Index
                        drow("Proportion") = td.PropBiomass
                        drow("PropCatch") = td.PropCatch
                        wr.AddRow(drow)
                    Next
                Next
            Catch ex As Exception
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_dbTarget.ReleaseWriter(wr, bSucces)

            Return bSucces

        End Function

    End Class

End Namespace ' Database
