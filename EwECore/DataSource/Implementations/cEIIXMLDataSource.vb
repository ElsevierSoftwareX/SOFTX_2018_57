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
Imports EwECore.DataSources
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports System.Xml
Imports System.Text
Imports System.Data.OleDb
Imports EwEUtils.Utilities
Imports EwECore.MSE
Imports EwECore.SpatialData
Imports EwECore.Auxiliary

'
#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Data access for an EwE6 .eiixml file
''' </summary>
''' ===========================================================================
Public Class cEIIXMLDataSource
    Implements IEwEDataSource
    Implements IEcopathDataSource
    Implements IEcosimDatasource
    Implements IEcospaceDatasource

    Private m_strFilename As String = ""
    Private m_core As cCore = Nothing
    Private m_doc As XmlDocument = Nothing

    Private Shared s_dtExcludedDBEntries As New Dictionary(Of String, String())

    Public Sub New()
        s_dtExcludedDBEntries("EcopathGroup") = New String() {"PoolColor"}
        s_dtExcludedDBEntries("EcopathFleet") = New String() {"PoolColor"}
        ' s_dtExcludedDBEntries("Auxillary") = New String() {}
        s_dtExcludedDBEntries("Quote") = New String() {}
        s_dtExcludedDBEntries("UpdateLog") = New String() {}
        s_dtExcludedDBEntries("Pedigree") = New String() {}
        s_dtExcludedDBEntries("EcopathGroupPedigree") = New String() {}
        s_dtExcludedDBEntries("Taxon") = New String() {}
        s_dtExcludedDBEntries("EcopathGroupTaxon") = New String() {}
        s_dtExcludedDBEntries("EcopathStanzaTaxon") = New String() {}

        ' Exclude value chain
        s_dtExcludedDBEntries("cUnit") = New String() {}
        s_dtExcludedDBEntries("cConsumerUnitDefault") = New String() {}
        s_dtExcludedDBEntries("cConsumerUnit") = New String() {}
        s_dtExcludedDBEntries("cDistributionUnit") = New String() {}
        s_dtExcludedDBEntries("cDistributionUnitDefault") = New String() {}
        s_dtExcludedDBEntries("cEconomicUnit") = New String() {}
        s_dtExcludedDBEntries("cFlowDiagram") = New String() {}
        s_dtExcludedDBEntries("cFlowPosition") = New String() {}
        s_dtExcludedDBEntries("cLink") = New String() {}
        s_dtExcludedDBEntries("cLinkDefault") = New String() {}
        s_dtExcludedDBEntries("cLinkLandings") = New String() {}
        s_dtExcludedDBEntries("cOOPStorable") = New String() {}
        s_dtExcludedDBEntries("cParameters") = New String() {}
        s_dtExcludedDBEntries("cProcessingUnit") = New String() {}
        s_dtExcludedDBEntries("cProcessingUnitDefault") = New String() {}
        s_dtExcludedDBEntries("cProducerUnit") = New String() {}
        s_dtExcludedDBEntries("cProducerUnitDefault") = New String() {}
        s_dtExcludedDBEntries("cRetailerUnit") = New String() {}
        s_dtExcludedDBEntries("cRetailerUnitDefault") = New String() {}
        s_dtExcludedDBEntries("cWholesalerUnit") = New String() {}
        s_dtExcludedDBEntries("cWholesalerUnitDefault") = New String() {}
    End Sub

#Region " Generic "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Open an existing data source connection
    ''' </summary>
    ''' <param name="strName">Name of the EII file to open.</param>
    ''' <param name="core"><see cref="cCore">Core instance</see> that holds the 
    ''' datastructures to read to, and write from.</param>
    ''' <returns>True if opened successfully.</returns>
    ''' -------------------------------------------------------------------
    Public Function Open(ByVal strName As String, _
                         ByVal core As cCore, _
                         Optional ByVal datasourceType As eDataSourceTypes = eDataSourceTypes.NotSet, _
                         Optional ByVal bReadOnly As Boolean = False) As eDatasourceAccessType _
                     Implements DataSources.IEwEDataSource.Open

        If (String.IsNullOrWhiteSpace(strName)) Then Return eDatasourceAccessType.Failed_UnknownType
        If Not File.Exists(strName) Then Return eDatasourceAccessType.Failed_FileNotFound

        Me.m_strFilename = strName
        Me.m_core = core
        Return eDatasourceAccessType.Opened

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States whether a datasource is already open.
    ''' </summary>
    ''' <returns>True if the datasource is open.</returns>
    ''' -------------------------------------------------------------------
    Public Function IsOpen() As Boolean _
             Implements IEwEDataSource.IsOpen
        Return (Not String.IsNullOrEmpty(Me.m_strFilename))
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Create the EII datasource.
    ''' </summary>
    ''' <param name="strName">Name of the EII file to create.</param>
    ''' <param name="strModelName">Name to assign to the model.</param>
    ''' <param name="core"><see cref="cCore">Core instance</see> that holds the 
    ''' datastructures to read to, and write from.</param>
    ''' <returns>Always false.</returns>
    ''' <remarks>This action is not supported in EwE6.</remarks>
    ''' -------------------------------------------------------------------
    Public Function Create(ByVal strName As String, ByVal strModelName As String, ByVal core As cCore) As eDatasourceAccessType _
             Implements IEwEDataSource.Create
        ' Cannot write EII files (yet)
        Return eDatasourceAccessType.Failed_Unknown
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Close the EII datasource.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function Close() As Boolean _
         Implements IEwEDataSource.Close

        Me.m_strFilename = ""
        Me.m_core = Nothing
        Me.m_doc = Nothing

        Return True

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Flag a core object as changed in the datasource. Since the EIIDataSource
    ''' does not support saving of data, this method contains no implementation
    ''' </summary>
    ''' <param name="cc">The <see cref="eCoreComponentType">core component</see> that changed.</param>
    ''' -------------------------------------------------------------------
    Friend Sub SetChanged(ByVal cc As eCoreComponentType) _
            Implements IEwEDataSource.SetChanged
        ' Take no action
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Clear change flags in the datasource. Since the EIIDataSource does 
    ''' not support saving of data, this method contains no implementation
    ''' </summary>
    ''' -------------------------------------------------------------------
    Friend Sub ClearChanged() _
        Implements IEwEDataSource.ClearChanged
        ' Take no actions
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the name of the EII file that this datasource operates on.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property Connection() As Object Implements DataSources.IEwEDataSource.Connection
        Get
            Return Me.m_strFilename
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the name of the EII file that this datasource operates on.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Overrides Function ToString() As String Implements IEwEDataSource.ToString
        Return Me.m_strFilename
    End Function

    Private Overloads Function CopyEcopathTo(ByVal ds As DataSources.IEcopathDataSource) As Boolean Implements DataSources.IEcopathDataSource.CopyTo
        Return False
    End Function

    Private Overloads Function CopyEcosimTo(ByVal ds As DataSources.IEcosimDatasource) As Boolean Implements DataSources.IEcosimDatasource.CopyTo
        Return False
    End Function

    Public Function Version() As Single Implements IEwEDataSource.Version
        Return -1.0!
    End Function

    Public Function BeginTransaction() As Boolean Implements DataSources.IEwEDataSource.BeginTransaction
        Return True
    End Function

    Public Function EndTransaction(ByVal bCommit As Boolean) As Boolean Implements DataSources.IEwEDataSource.EndTransaction
        Return True
    End Function

#End Region ' Generic

#Region " Diagnostics "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States whether the datasource has unsaved changes that do not relate
    ''' to any of the supported sub-models.
    ''' </summary>
    ''' <returns>True if the datasource has pending changes.</returns>
    ''' -------------------------------------------------------------------
    Public Function IsModified() As Boolean Implements DataSources.IEwEDataSource.IsModified
        Return False
    End Function

#End Region ' Diagnostics

#Region " Load "

#Region " Ecopath "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Initiates a full load of an ecopath model.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function LoadModel() As Boolean _
        Implements IEcopathDataSource.LoadModel

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData

        Dim strEwEVersion As String
        Dim sDBVersion As Single
        Dim strModel As String = ""
        Dim bSucces As Boolean = True

        If (Me.m_core Is Nothing) Then Return False

        Me.ClearChanged()

        Me.m_doc = New XmlDocument()
        Dim xnModel As XmlNode = Nothing

        Try
            Me.m_doc.Load(Me.m_strFilename)
            xnModel = Me.m_doc.SelectSingleNode("EwEModel")
            Try
                sDBVersion = cStringUtils.ConvertToSingle(xnModel.Attributes("DBVersion").InnerText)
            Catch ex As Exception
                sDBVersion = 6.120011
            End Try

            Try
                strEwEVersion = xnModel.Attributes("EwEVersion").InnerText
            Catch ex As Exception

            End Try

            bSucces = Me.LoadModelInfo()
            If bSucces = False Then Return False

            bSucces = bSucces And Me.LoadEcopathGroups()
            bSucces = bSucces And Me.LoadEcopathTaxon()
            bSucces = bSucces And Me.LoadEcopathFleetInfo()
            'bSucces = bSucces And Me.LoadParticleSizeDistribution()
            bSucces = bSucces And Me.LoadPedigreeLevels()
            bSucces = bSucces And Me.LoadPedigreeAssignments()

            bSucces = bSucces And Me.LoadAuxillaryData()

            ecopathDS.bInitialized = bSucces
            ecopathDS.onPostInitialization()

            bSucces = bSucces And Me.LoadEcosimScenarioDefinitions()
            bSucces = bSucces And Me.LoadEcospaceScenarioDefinitions()
            bSucces = bSucces And Me.LoadEcotracerScenarioDefinitions()
            bSucces = bSucces And Me.LoadTimeSeriesDatasets()

            ' Clear changed admin
            Me.ClearChanged()

            Return bSucces
        Catch ex As Exception
            Return False
        End Try

        Return False

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, loads model info for the current model.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Private Function LoadModelInfo() As Boolean

        Dim dt As DataTable = Me.ReadTable("EcopathModel")
        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim sVal1 As Single = 0.0!
        Dim sVal2 As Single = 0.0!
        Dim bSucces As Boolean = True

        ' Crash prevention check
        If dt Is Nothing Then
            'Debug.Assert(False, "Failed to access table EcopathModel")
            Return False
        End If

        Try
            ' There is only one model in an EwE6 database
            For Each row As DataRow In dt.Rows

                ecopathDS.ModelDBID = CInt(row("ModelID"))
                ecopathDS.ModelName = CStr(row("Name"))
                ecopathDS.ModelDescription = CStr(row("Description"))
                ecopathDS.ModelAuthor = CStr(Me.ReadSafe(row, "Author", ""))
                ecopathDS.ModelContact = CStr(Me.ReadSafe(row, "Contact", ""))
                ecopathDS.ModelArea = CSng(Me.ReadSafe(row, "Area", 1.0))
                ecopathDS.ModelNumDigits = CInt(row("NumDigits"))
                ecopathDS.ModelGroupDigits = (CInt(Me.ReadSafe(row, "GroupDigits", False)) <> 0)
                ecopathDS.ModelUnitCurrency = DirectCast(CInt(Me.ReadSafe(row, "UnitCurrency", eUnitCurrencyType.WetWeight)), eUnitCurrencyType)
                ecopathDS.ModelUnitCurrencyCustom = CStr(Me.ReadSafe(row, "UnitCurrencyCustom", ""))
                ecopathDS.ModelUnitTime = DirectCast(CInt(Me.ReadSafe(row, "UnitTime", eUnitTimeType.Year)), eUnitTimeType)
                ecopathDS.ModelUnitTimeCustom = CStr(Me.ReadSafe(row, "UnitTimeCustom", ""))
                ecopathDS.ModelUnitMonetary = DirectCast(Me.ReadSafe(row, "UnitMonetary", "EUR"), String)
                ecopathDS.FirstYear = CInt(Me.ReadSafe(row, "FirstYear", 0))
                ecopathDS.NumYears = CInt(Me.ReadSafe(row, "NumYears", 1))
                ecopathDS.ModelCountry = CStr(Me.ReadSafe(row, "Country", ""))
                ecopathDS.ModelEcosystemType = CStr(Me.ReadSafe(row, "EcosystemType", ""))
                ecopathDS.ModelEcobaseCode = CStr(Me.ReadSafe(row, "CodeEcobase", ""))
                ecopathDS.ModelPublicationDOI = CStr(Me.ReadSafe(row, "PublicationDOI", ""))
                ecopathDS.ModelPublicationURI = CStr(Me.ReadSafe(row, "PublicationURI", ""))
                ecopathDS.ModelPublicationRef = CStr(Me.ReadSafe(row, "PublicationRef", ""))

                Dim sLat1 As Single = CSng(Me.ReadSafe(row, "MaxLat", cCore.NULL_VALUE))
                Dim sLat2 As Single = CSng(Me.ReadSafe(row, "MinLat", cCore.NULL_VALUE))
                ecopathDS.ModelNorth = Math.Max(sLat1, sLat2)
                ecopathDS.ModelSouth = Math.Min(sLat1, sLat2)

                ecopathDS.ModelWest = CSng(Me.ReadSafe(row, "MinLon", cCore.NULL_VALUE))
                ecopathDS.ModelEast = CSng(Me.ReadSafe(row, "MaxLon", cCore.NULL_VALUE))

                ecopathDS.ModelLastSaved = CDbl(Me.ReadSafe(row, "LastSaved", 0))

            Next

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EcopathModel", ex.Message))
            bSucces = False
        End Try

        dt.Clear()

        Return bSucces
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Loads Ecopath Group information.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Private Function LoadEcopathGroups() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData
        Dim dt As DataTable = Me.ReadTable("EcopathGroup")
        Dim iGroup As Integer = 1
        Dim bSucces As Boolean = True

        ' Init data structure
        ecopathDS.NumGroups = dt.Rows.Count
        ecopathDS.NumLiving = 0
        For Each row As DataRow In dt.Rows
            If (CInt(row("Type")) <= 1) Then ecopathDS.NumLiving += 1
        Next
        ecopathDS.NumDetrit = ecopathDS.NumGroups - ecopathDS.NumLiving

        ' Allocate space
        If (Not ecopathDS.redimGroupVariables() Or Not psdDS.redimGroupVariables()) Then
            ' It would be quite remarkable to fail here... log message?
            Return False
        End If

        dt.DefaultView.Sort = "Sequence ASC"
        For Each row As DataRow In dt.DefaultView.ToTable.Rows

            Try
                ecopathDS.GroupDBID(iGroup) = CInt(row("GroupID"))
                ecopathDS.GroupName(iGroup) = CStr(row("GroupName"))
                ecopathDS.PP(iGroup) = CSng(row("Type"))
                ecopathDS.Area(iGroup) = CSng(row("Area"))
                ecopathDS.BH(iGroup) = ecopathDS.B(iGroup) / ecopathDS.Area(iGroup)
                ecopathDS.BAInput(iGroup) = CSng(row("BiomAcc"))
                ' VERIFY_JS: Check default value for BiomAccRate. 0 is assumed
                ecopathDS.BaBi(iGroup) = CSng(row("BiomAccRate"))
                ecopathDS.GS(iGroup) = CSng(row("Unassim"))
                ecopathDS.DtImp(iGroup) = CSng(row("DtImports"))
                ecopathDS.Ex(iGroup) = CSng(row("Export"))
                ecopathDS.fCatch(iGroup) = CSng(row("Catch"))
                ecopathDS.DCInput(iGroup, 0) = CSng(row("ImpVar"))
                ecopathDS.GroupIsFish(iGroup) = ParseBoolean(CStr(row("GroupIsFish")))
                ecopathDS.GroupIsInvert(iGroup) = ParseBoolean(CStr(row("GroupIsInvert")))
                ecopathDS.Shadow(iGroup) = CSng(row("NonMarketValue"))
                ecopathDS.Resp(iGroup) = CSng(row("Respiration"))
                ecopathDS.Immig(iGroup) = CSng(row("Immigration"))
                ecopathDS.Emigration(iGroup) = CSng(row("Emigration"))
                ecopathDS.Emig(iGroup) = CSng(Me.ReadSafe(row, "EmigRate", 0.0!))

                ' PSD
                ecopathDS.vbK(iGroup) = CSng(Me.ReadSafe(row, "VBK", -1))
                psdDS.AinLWInput(iGroup) = CSng(row("AinLW"))
                psdDS.BinLWInput(iGroup) = CSng(row("BinLW"))
                psdDS.LooInput(iGroup) = CSng(row("Loo"))
                psdDS.WinfInput(iGroup) = CSng(row("Winf"))
                psdDS.t0Input(iGroup) = CSng(row("t0"))
                psdDS.TcatchInput(iGroup) = CSng(row("Tcatch"))
                psdDS.TmaxInput(iGroup) = CSng(row("Tmax"))

                'variables with input output pairs
                ecopathDS.EEinput(iGroup) = CSng(row("EcoEfficiency"))
                ecopathDS.OtherMortinput(iGroup) = CSng(Me.ReadSafe(row, "OtherMort", cCore.NULL_VALUE))
                ecopathDS.PBinput(iGroup) = CSng(row("ProdBiom"))
                ecopathDS.QBinput(iGroup) = CSng(row("ConsBiom"))
                ecopathDS.GEinput(iGroup) = CSng(row("ProdCons"))
                ecopathDS.Binput(iGroup) = CSng(row("Biomass"))
                ecopathDS.BHinput(iGroup) = ecopathDS.Binput(iGroup) / ecopathDS.Area(iGroup)

                'ecopathDS.GroupColor(iGroup) = Integer.Parse(CStr(Me.ReadSafe(row, "PoolColor", "0")), Globalization.NumberStyles.HexNumber)

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading group {1}", ex.Message, ecopathDS.GroupName(iGroup)))
                bSucces = False
            End Try

            iGroup += 1

        Next

        Debug.Assert(iGroup - 1 = ecopathDS.NumGroups)

        dt.Clear()
        dt = Nothing

        bSucces = bSucces And Me.LoadEcopathDietComp()
        bSucces = bSucces And Me.LoadStanza()

        Return bSucces

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Loads ecopath diet composition information.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Private Function LoadEcopathDietComp() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim dt As DataTable = Me.ReadTable("EcopathDietComp")
        Dim iPred As Integer = 0
        Dim iPrey As Integer = 0
        Dim sDiet As Single = 0.0!
        Dim bSucces As Boolean = True

        Try
            For Each row As DataRow In dt.Rows

                iPred = Array.IndexOf(ecopathDS.GroupDBID, CInt(row("PredID")))
                iPrey = Array.IndexOf(ecopathDS.GroupDBID, CInt(row("PreyID")))

                Debug.Assert(iPred >= 0 And iPrey >= 0)
                sDiet = CSng(row("Diet"))

                ' Set diet to 0 for non-living groups (fixes #878)
                If (sDiet > 0) And (iPred > ecopathDS.NumLiving) Then sDiet = 0
                ecopathDS.DCInput(iPred, iPrey) = sDiet

                If iPrey > ecopathDS.NumLiving Then
                    ecopathDS.DF(iPred, iPrey - ecopathDS.NumLiving) = CSng(row("DetritusFate"))
                End If

                ' 060528JS: ASSERT on "diet leftovers" from previous incarnations, including 041020VC fix for carbon groups
                ' The actual data fix is performed once during EwE5 import, and should not reoccur when running EwE6.
                If ecopathDS.PP(iPred) = 1 And ecopathDS.QB(iPred) <= 0 Then
                    If (ecopathDS.DCInput(iPred, iPrey) <> 0) Then
                        cLog.Write(cStringUtils.Localize("Database error on DCInput({0},{1})={2}, expected 0", iPred, iPrey, ecopathDS.DCInput(iPred, iPrey)))
                    End If
                End If

                ' VERIFY_JS: check mapping for MTI with JB
                ' ecopathDS.??(nPred, nPrey) = CSng(drow("MTI"))
                ' VERIFY_JS: check mapping for Electivity with JB
                ' ecopathDS.??(nPred, nPrey) = CSng(drow("Electivity"))
            Next
            dt.Clear()

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EcopathDietComp {1}, {2}", ex.Message, ecopathDS.GroupName(iPred), ecopathDS.GroupName(iPrey)))
            bSucces = False
        End Try

        Return True

    End Function

    Private Function LoadStanza() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
        Dim dtStanza As DataTable = Nothing
        Dim dtLifeStage As DataTable = Nothing
        Dim iStanza As Integer = 0
        Dim iLifeStage As Integer = 0
        Dim iGroup As Integer = 0
        Dim sTemp As Single = 0.0
        Dim bSucces As Boolean = True

        dtStanza = Me.ReadTable("Stanza")
        dtStanza.DefaultView.Sort = "StanzaID ASC"

        dtLifeStage = Me.ReadTable("StanzaLifeStage")

        ' Count the number of rows in StanzaInfo; this is the number of split groups that we're going to work with
        stanzaDS.Nsplit = dtStanza.Rows.Count
        ' Get max no of stanza
        stanzaDS.MaxStanza = 0

        If (stanzaDS.Nsplit > 0) Then
            'stanzaDS.MaxStanza = CInt(Me..GetValue("SELECT MAX(NumGroups) FROM (SELECT COUNT(*) AS NumGroups FROM StanzaLifeStage GROUP BY StanzaID) AS X", 0))
            Dim dic As New Dictionary(Of Integer, Integer)
            For Each row As DataRow In dtLifeStage.Rows
                iStanza = CInt(row("StanzaID"))
                If dic.ContainsKey(iStanza) Then iLifeStage = dic(iStanza) Else iLifeStage = 0
                iLifeStage += 1
                dic(iStanza) = iLifeStage
                stanzaDS.MaxStanza = Math.Max(stanzaDS.MaxStanza, iLifeStage)
            Next
        End If

        ' Get the number of groups from ecopath
        stanzaDS.nGroups = ecopathDS.NumGroups

        If stanzaDS.MaxAgeSplit < cCore.MAX_AGE Then
            'VILLY: NEED TO REPLACE THIS WITH DYNAMIC CALCULATION ALLOWING FOR CHANGES IN K DURING EXECUTION
            stanzaDS.MaxAgeSplit = cCore.MAX_AGE
        End If

        stanzaDS.redimStanza()

        ' First read Stanza
        iStanza = 0
        For Each row As DataRow In dtStanza.Rows

            ' JS 11May2010: Stanza configs without stanza groups are now loaded.
            '               This *could* screw up the core calculations, but in a way
            '               it already did by allowing empty stanza groups to be defined
            '               in the system by allowing stanzaDS.nGroups to be non-zero,
            '               even if stanzaDS.MaxStanza were 0. Based on this behaviour
            '               there seems little harm by allowing the empty stanza group
            '               names to be available in the core and to an interface.

            ' Read this stanza
            iStanza += 1

            Try

                stanzaDS.StanzaDBID(iStanza) = CInt(row("StanzaID"))
                ' JS 20jun06: StanzaName array 1-dimensional. GroupNames only seem to matter to the EwE5 GUI.
                '             EwE6 will resolve stanza group names via ICoreInputOutput objects to keep track of 'live' changes.
                stanzaDS.StanzaName(iStanza) = CStr(row("StanzaName"))

                stanzaDS.RecPowerSplit(iStanza) = CSng(row("RecPower"))
                stanzaDS.BABsplit(iStanza) = CSng(row("BabSplit"))
                stanzaDS.WmatWinf(iStanza) = CSng(row("WMatWinf"))
                ' stanzaDS.HatchCode(iStanza) = CInt(rdStanza("HatchCode"))
                stanzaDS.FixedFecundity(iStanza) = ParseBoolean(CStr(row("FixedFecundity")))
                stanzaDS.EggAtSpawn(iStanza) = ParseBoolean(CStr(Me.ReadSafe(row, "EggAtSpawn", True)))

                ' JS 23apr07: Leading B and QB groups are calculated at runtime, no longer stored in DB
                ' JS 23nov10: Hah, three and a half years later these values are stored again
                stanzaDS.BaseStanza(iStanza) = CInt(Me.ReadSafe(row, "LeadingLifeStage", cCore.NULL_VALUE))
                stanzaDS.BaseStanza(iStanza) = Math.Max(1, Math.Min(stanzaDS.Nstanza(iStanza), stanzaDS.BaseStanza(iStanza)))

                ' JS 14jun12: Leading CB separated from leading B
                stanzaDS.BaseStanzaCB(iStanza) = CInt(Me.ReadSafe(row, "LeadingCB", stanzaDS.BaseStanza(iStanza)))
                stanzaDS.BaseStanzaCB(iStanza) = Math.Max(1, Math.Min(stanzaDS.Nstanza(iStanza), stanzaDS.BaseStanzaCB(iStanza)))

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Stanza {1}", ex.Message, stanzaDS.StanzaName(iStanza)))
                bSucces = False
            End Try

            'rdLifeStage = Me..Getdrow(cStringUtils.Localize("SELECT * FROM StanzaLifeStage WHERE (StanzaID={0}) ORDER BY AgeStart ASC", rdStanza("StanzaID")))
            dtLifeStage.DefaultView.RowFilter = "StanzaID=" & CInt(row("StanzaID"))
            dtLifeStage.DefaultView.Sort = "AgeStart ASC"
            iLifeStage = 0

            For Each rowStage As DataRow In dtLifeStage.DefaultView.ToTable.Rows
                ' Next life stage in this stanza
                iLifeStage += 1

                ' Store Stanza configuration
                Try

                    ' Resolve group index
                    iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(rowStage("GroupID")))
                    ' JS 20jun06: Disabled (see comment above)
                    ' ecosimDS.StanzaName(nStanza, nGroup) = ecopathDS.GroupName(iGroup)
                    stanzaDS.EcopathCode(iStanza, iLifeStage) = iGroup
                    stanzaDS.Stanza_Z(iStanza, iLifeStage) = CSng(rowStage("Mortality"))
                    stanzaDS.SpeciesCode(iGroup, 0) = iStanza
                    stanzaDS.Age1(iStanza, iLifeStage) = CInt(rowStage("AgeStart"))

                Catch ex As Exception
                    Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading StanzaLifeStage {1}", ex.Message, stanzaDS.StanzaName(iStanza), ecopathDS.GroupName(iGroup)))
                    bSucces = False
                End Try

                ' Inform Ecopath
                ecopathDS.StanzaGroup(iGroup) = True
            Next
            ' Update number of groups in this stanza
            stanzaDS.Nstanza(iStanza) = iLifeStage
        Next

        dtStanza.Clear()
        dtLifeStage.Clear()

        Return bSucces
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Loads all fleet-related data.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' If there is no <see cref="IsFishing">fishing</see>, the fleet data will not be loaded.
    ''' This check is inherited from EwE5.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Private Function LoadEcopathFleetInfo() As Boolean

        Dim bSucces As Boolean = LoadEcopathFleets()
        bSucces = bSucces And LoadEcopathCatch()
        bSucces = bSucces And LoadEcopathDiscardFate()

        Return bSucces

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Loads all Ecopath fleets.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Private Function LoadEcopathFleets() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim dtFleets As DataTable = Me.ReadTable("EcopathFleet")
        Dim iFleet As Integer = 1
        Dim bSucces As Boolean = True

        ecopathDS.NoGearData = Not IsFishing()
        ecopathDS.NumFleet = dtFleets.Rows.Count()

        If Not ecopathDS.RedimFleetVariables(True) Then Return False

        Try
            dtFleets.DefaultView.Sort = "Sequence ASC"
            For Each drow As DataRow In dtFleets.DefaultView.ToTable.Rows

                ecopathDS.FleetDBID(iFleet) = CInt(drow("FleetID"))
                ecopathDS.FleetName(iFleet) = CStr(drow("FleetName"))
                ecopathDS.CostPct(iFleet, eCostIndex.Fixed) = CSng(drow("FixedCost"))
                ecopathDS.CostPct(iFleet, eCostIndex.Sail) = CSng(drow("SailingCost"))
                ecopathDS.CostPct(iFleet, eCostIndex.CUPE) = CSng(drow("variableCost"))
                'ecopathDS.FleetColor(iFleet) = Integer.Parse(CStr(drow("PoolColor")), Globalization.NumberStyles.HexNumber)
                iFleet += 1

            Next

            dtFleets.Clear()

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EcopathFleet {1}", ex.Message, iFleet))
            bSucces = False
        End Try

        Return bSucces

    End Function

    Private Function LoadEcopathCatch() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim dtCatch As DataTable = Me.ReadTable("EcopathCatch")
        Dim iFleet As Integer = 0
        Dim iGroup As Integer = 0
        Dim bSucces As Boolean = True

        Try

            For Each drow As DataRow In dtCatch.Rows

                iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(drow("GroupID")))
                iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(drow("FleetID")))

                If (iGroup >= 1 And iFleet >= 1) Then
                    ecopathDS.Landing(iFleet, iGroup) = CSng(drow("Landing"))
                    ecopathDS.Discard(iFleet, iGroup) = CSng(drow("discards"))
                    ecopathDS.Market(iFleet, iGroup) = CSng(drow("price"))
                    ecopathDS.PropDiscardMort(iFleet, iGroup) = CSng(Me.ReadSafe(drow, "DiscardMortality", 0.0!))
                Else
                    Me.LogMessage(cStringUtils.Localize("Error {0} occurred while appending loading catch for group {0}, fleet {1}", iGroup, iFleet))
                    bSucces = False
                End If

            Next

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading catch {1}, {2}", ex.Message, iGroup, iFleet))
            bSucces = False
        End Try

        dtCatch.Clear()

        Return bSucces

    End Function

    Private Function LoadEcopathDiscardFate() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim dt As DataTable = Me.ReadTable("EcopathDiscardFate")
        Dim iFleet As Integer = 0
        Dim iGroup As Integer = 0
        Dim bSucces As Boolean = True

        Try
            For Each drow As DataRow In dt.Rows

                iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(drow("GroupID")))
                iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(drow("FleetID")))

                If (iGroup > ecopathDS.NumLiving) Then
                    ecopathDS.DiscardFate(iFleet, iGroup - ecopathDS.NumLiving) = CSng(drow("DiscardFate"))
                End If

            Next
            dt.Clear()

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading DiscardFate {1}, {2}", ex.Message, iGroup, iFleet))
            bSucces = False
        End Try

        Return bSucces

    End Function

    Private Function LoadAuxillaryData() As Boolean

        Dim dt As DataTable = Me.ReadTable("Auxillary")
        Dim strValueID As String = ""
        Dim strRemark As String = ""
        Dim strVisualStyle As String = ""
        Dim ad As cAuxiliaryData = Nothing
        Dim bSucces As Boolean = True

        Me.m_core.m_dtAuxiliaryData.Clear()

        Try
            For Each drow As DataRow In dt.Rows

                strValueID = CStr(drow("ValueID"))
                strRemark = Web.HttpUtility.UrlDecode(CStr(Me.ReadSafe(drow, "Remark", "")))
                strVisualStyle = CStr(Me.ReadSafe(drow, "VisualStyle", ""))

                ad = Me.m_core.AuxillaryData(strValueID)
                ad.AllowValidation = False

                ad.DBID = CInt(drow("DBID"))
                ad.Remark = strRemark
                ad.VisualStyle = cVisualStyleReader.StringToStyle(strVisualStyle)
                ad.Settings.Load(CStr(Me.ReadSafe(drow, "Settings", "")))

                ad.AllowValidation = True

            Next
            dt.Clear()

        Catch ex As Exception
            Me.LogMessage(String.Format("Error {0} occurred while reading AuxillaryData", ex.Message))
            bSucces = False
        End Try

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the pedigree level definitions.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks>Not supported yet</remarks>
    ''' -----------------------------------------------------------------------
    Private Function LoadPedigreeLevels() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData

        ' Init data structure
        ecopathDS.NumPedigreeLevels = 0
        ecopathDS.RedimPedigree()

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the pedigree level assignments.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadPedigreeAssignments() As Boolean
        Return True
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Loads Ecopath taxonomy information.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Private Function LoadEcopathTaxon() As Boolean

        Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
        taxonDS.NumTaxon = 0
        taxonDS.RedimTaxon()
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the list of available Ecosim scenarios.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' Note that this will NOT load any actual Ecosim scenario. Scenario definitions 
    ''' merely provide a preview of available Ecosim scenarios in the database.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcosimScenarioDefinitions() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim dt As DataTable = Me.ReadTable("EcosimScenario")
        Dim iScenario As Integer = 1
        Dim bSucces As Boolean = True

        ecopathDS.NumEcosimScenarios = dt.Rows.Count
        ecopathDS.RedimEcosimScenarios()
        If (ecopathDS.NumEcosimScenarios = 0) Then Return bSucces

        dt.DefaultView.Sort = "ScenarioID ASC"

        Try
            For Each drow As DataRow In dt.Rows
                ecopathDS.EcosimScenarioDBID(iScenario) = CInt(drow("ScenarioID"))
                ecopathDS.EcosimScenarioName(iScenario) = CStr(drow("ScenarioName"))
                ecopathDS.EcosimScenarioDescription(iScenario) = CStr(drow("Description"))
                ecopathDS.EcosimScenarioAuthor(iScenario) = CStr(Me.ReadSafe(drow, "Author", ""))
                ecopathDS.EcosimScenarioContact(iScenario) = CStr(Me.ReadSafe(drow, "Contact", ""))
                ecopathDS.EcosimScenarioLastSaved(iScenario) = CDbl(Me.ReadSafe(drow, "LastSaved", 0))
                iScenario += 1
            Next
        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading ecosim scenario definition {1}", ex.Message, iScenario))
            bSucces = False
        End Try

        dt.Clear()

        Return bSucces
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the list of available Ecospace scenarios.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' Note that this will NOT load any actual Ecospace scenario. Scenario definitions 
    ''' merely provide a preview of available Ecospace scenarios in the database.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcospaceScenarioDefinitions() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenario")
        Dim iScenario As Integer = 1
        Dim bSucces As Boolean = True

        ecopathDS.NumEcospaceScenarios = dt.Rows.Count
        ecopathDS.RedimEcospaceScenarios()
        If (ecopathDS.NumEcospaceScenarios = 0) Then Return bSucces

        dt.DefaultView.Sort = "ScenarioID ASC"

        Try
            For Each drow As DataRow In dt.Rows
                ecopathDS.EcospaceScenarioDBID(iScenario) = CInt(drow("ScenarioID"))
                ecopathDS.EcospaceScenarioName(iScenario) = CStr(drow("ScenarioName"))
                ecopathDS.EcospaceScenarioDescription(iScenario) = CStr(drow("Description"))
                ecopathDS.EcospaceScenarioAuthor(iScenario) = CStr(Me.ReadSafe(drow, "Author", ""))
                ecopathDS.EcospaceScenarioContact(iScenario) = CStr(Me.ReadSafe(drow, "Contact", ""))
                ecopathDS.EcospaceScenarioLastSaved(iScenario) = CDbl(Me.ReadSafe(drow, "LastSaved", 0))
                iScenario += 1
            Next
        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading ecospace scenario definition {1}", ex.Message, iScenario))
            bSucces = False
        End Try

        dt.Clear()
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the list of available Ecotracer scenarios.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks>Not supported yet</remarks>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcotracerScenarioDefinitions() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData

        ecopathDS.NumEcotracerScenarios = 0
        ecopathDS.RedimEcotracerScenarios()

        ' dt.DefaultView.Sort = "ScenarioID ASC"

        Return True
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Load all time series dataset definitions for Ecopath.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks>Yeah, this is odd; time series can only be used with Ecosim
    ''' but this logic just reads which time series will be available for Ecosim
    ''' later on; it is convenient to know which data sets are provided with
    ''' the model, just as it is convenient to know which scenarios are
    ''' before they are loaded ;)</remarks>
    ''' -------------------------------------------------------------------
    Private Function LoadTimeSeriesDatasets() As Boolean

        Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
        Dim dt As DataTable = Me.ReadTable("EcosimTimeSeriesDataset")
        Dim iDataset As Integer = 1
        Dim bSucces As Boolean = True

        tsDS.nDatasets = dt.Rows.Count

        tsDS.RedimTimeSeriesDatasets()

        Try
            For Each drow As DataRow In dt.Rows
                tsDS.iDatasetDBID(iDataset) = CInt(drow("DatasetID"))
                tsDS.strDatasetNames(iDataset) = CStr(drow("DatasetName"))
                tsDS.strDatasetDescription(iDataset) = CStr(Me.ReadSafe(drow, "Description", ""))
                tsDS.strDatasetAuthor(iDataset) = CStr(Me.ReadSafe(drow, "Author", ""))
                tsDS.strDatasetContact(iDataset) = CStr(Me.ReadSafe(drow, "Contact", ""))
                tsDS.nDatasetFirstYear(iDataset) = CInt(drow("FirstYear"))
                tsDS.nDatasetNumTimeSeries(iDataset) = 0 ' CInt(Me.GetValue(cStringUtils.Localize("SELECT COUNT(*) FROM EcosimTimeSeries WHERE (DatasetID={0})", CInt(drow("DatasetID")))))

                'tsDS.nDatasetNumPoints(iDataset) = CInt(drow("NumYears"))
                tsDS.nDatasetNumPoints(iDataset) = CInt(drow("NumPoints"))
                tsDS.DataSetIntervals(iDataset) = CType(CInt(drow("DataInterval")), eTSDataSetInterval)

                iDataset += 1
            Next
        Catch ex As Exception
            bSucces = False
        End Try

        dt.Clear()

        Return bSucces

    End Function

#End Region ' Ecopath

#Region " Ecosim "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Loads an ecosim scenario from the DB.
    ''' </summary>
    ''' <param name="iScenarioID">Database ID of the scenario to load.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Friend Function LoadEcosimScenario(ByVal iScenarioID As Integer) As Boolean _
            Implements IEcosimDatasource.LoadEcosimScenario

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
        Dim dt As DataTable = Me.ReadTable("EcosimScenario")
        Dim bSucces As Boolean = True

        bSucces = Me.LoadEcosimModel()

        ecosimDS.RedimVars()
        ecosimDS.SetDefaultParameters()
        mseDS.RedimVars()
        mseDS.setDefaultRegValues()

        'jb 11-Oct-2012 Add MSY data
        Me.m_core.m_MSYData.RedimVars()

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows

            Try
                ecosimDS.NumYears = CInt(drow("TotalTime"))
                ecosimDS.StepSize = CSng(drow("StepSize"))
                ecosimDS.EquilibriumStepSize = CSng(drow("EquilibriumStepSize"))
                ecosimDS.EquilScaleMax = CSng(drow("EquilScaleMax"))
                ecosimDS.SorWt = CSng(drow("sorwt"))
                ecosimDS.SystemRecovery = CSng(drow("SystemRecovery"))
                ecosimDS.Discount = CSng(drow("Discount"))

                'ecosimDS.NudgeStart = CSng(drow("NudgeStart"))
                'ecosimDS.NudgeEnd = CSng(drow("NudgeEnd"))
                'ecosimDS.NudgeFactor = CSng(drow("NudgeFactor"))
                'ecosimDS.DoInteg = CSng(drow("DoInteg"))
                'ecosimDS.chkNudge = CBool(drow("UseNudge"))

                'drow("NMed") = Me.FixValue(drow("NMed"))                        ' DISCONTINUED
                'drow("NMedPoints") = Me.FixValue(drow("NMedPoints"))            ' DISCONTINUED

                ecosimDS.NutBaseFreeProp = CSng(drow("NutBaseFreeProp"))
                ecosimDS.NutPBmax = CSng(drow("NutPBmax"))

                'ecosimDS.UseVarPQ = CBool(drow("UseVarPQ"))
                'VC090403: the var P/Q was being set to true by default, It shouldn't be, this should be done in interface only
                ecosimDS.UseVarPQ = False
                ecosimDS.ForagingTimeLowerLimit = CSng(Me.ReadSafe(drow, "ForagingTimeLowerLimit", 0.01))

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try
        Next
        dt.Clear()

        'jb added to redim time variables in ecosim data structures
        ecosimDS.RedimTime()

        Me.m_core.m_MSEData.redimTime()

        ' Set active scenario
        ecopathDS.ActiveEcosimScenario = Array.IndexOf(ecopathDS.EcosimScenarioDBID, iScenarioID)

        bSucces = bSucces And Me.LoadEcosimGroups(iScenarioID)
        bSucces = bSucces And Me.LoadEcosimFleets(iScenarioID)
        bSucces = bSucces And Me.LoadShapes()
        'bSucces = bSucces And Me.LoadEcosimMSE(iScenarioID)
        'bSucces = bSucces And Me.LoadAuxillaryData()

        Me.ClearChanged()

        Return bSucces

    End Function

    Private Function LoadEcosimModel() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim dt As DataTable = Me.ReadTable("EcosimModel")
        Dim bSuccess As Boolean = True

        For Each drow As DataRow In dt.Rows
            Try
                ecosimDS.ForcePoints = CInt(Me.ReadSafe(drow, "ForcePoints", cEcosimDatastructures.DEFAULT_N_FORCINGPOINTS))
            Catch ex As Exception
                bSuccess = False
            End Try
        Next

        ecosimDS.nGroups = ecopathDS.NumGroups

        dt.Clear()
        Return bSuccess

    End Function

    Private Function LoadEcosimGroups(ByVal iScenarioID As Integer) As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioGroup")
        Dim bSucces As Boolean = True
        Dim iEcopathGroupID As Integer = 0
        Dim iGroup As Integer = 0

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows

            iEcopathGroupID = CInt(drow("EcopathGroupID"))
            iGroup = Array.IndexOf(ecopathDS.GroupDBID, iEcopathGroupID)

            Debug.Assert(iGroup > 0)

            Try

                ' Read fields
                ecosimDS.GroupDBID(iGroup) = CInt(drow("GroupID"))
                ecosimDS.PBmaxs(iGroup) = CSng(drow("pbmaxs"))
                ecosimDS.FtimeMax(iGroup) = CSng(drow("FtimeMax"))
                ecosimDS.FtimeAdjust(iGroup) = CSng(drow("FtimeAdjust"))
                ecosimDS.MoPred(iGroup) = CSng(drow("MoPred"))
                ecosimDS.FishRateMax(iGroup) = CSng(drow("FishRateMax"))
                ' ecosimDS.ShowGroup(i) = CBool(drow("Show"))

                ecosimDS.RiskTime(iGroup) = CSng(drow("RiskTime"))
                ecosimDS.QmQo(iGroup) = CSng(drow("QmQo"))
                ecosimDS.CmCo(iGroup) = CSng(drow("CmCo"))
                ecosimDS.SwitchPower(iGroup) = CSng(drow("SwitchPower"))
                ecosimDS.GroupFishRateNoDBID(iGroup) = CInt(drow("FishMortShapeID"))

                mseDS.Blim(iGroup) = CSng(Me.ReadSafe(drow, "Blim", mseDS.Blim(iGroup), cCore.NULL_VALUE))
                mseDS.Bbase(iGroup) = CSng(Me.ReadSafe(drow, "Bbase", mseDS.Bbase(iGroup), cCore.NULL_VALUE))
                mseDS.Fopt(iGroup) = CSng(Me.ReadSafe(drow, "Fopt", mseDS.Fopt(iGroup), cCore.NULL_VALUE))
                mseDS.FixedEscapement(iGroup) = CSng(Me.ReadSafe(drow, "FixedEscapement", 0.0!, cCore.NULL_VALUE))
                mseDS.FixedF(iGroup) = CSng(Me.ReadSafe(drow, "FixedF", 0.0!, cCore.NULL_VALUE))

                mseDS.CVbiomEst(iGroup) = CSng(Me.ReadSafe(drow, "BiomassCV", mseDS.CVbiomEst(iGroup), cCore.NULL_VALUE))
                mseDS.BioRiskValue(iGroup, 0) = CSng(Me.ReadSafe(drow, "LowerRisk", mseDS.BioRiskValue(iGroup, 0), cCore.NULL_VALUE))
                mseDS.BioRiskValue(iGroup, 1) = CSng(Me.ReadSafe(drow, "UpperRisk", mseDS.BioRiskValue(iGroup, 1), cCore.NULL_VALUE))

                mseDS.DefaultBioBounds(iGroup)
                mseDS.BioBounds(iGroup).Lower = CSng(Me.ReadSafe(drow, "BiomassRefLower", mseDS.BioBounds(iGroup).Lower, cCore.NULL_VALUE))
                mseDS.BioBounds(iGroup).Upper = CSng(Me.ReadSafe(drow, "BiomassRefUpper", mseDS.BioBounds(iGroup).Upper, cCore.NULL_VALUE))

                mseDS.DefaultCatchBoundsGroup(iGroup)
                mseDS.CatchGroupBounds(iGroup).Lower = CSng(Me.ReadSafe(drow, "CatchRefLower", mseDS.CatchGroupBounds(iGroup).Lower, cCore.NULL_VALUE))
                mseDS.CatchGroupBounds(iGroup).Upper = CSng(Me.ReadSafe(drow, "CatchRefUpper", mseDS.CatchGroupBounds(iGroup).Upper, cCore.NULL_VALUE))

                mseDS.RstockRatio(iGroup) = CSng(Me.ReadSafe(drow, "RStockRatio", mseDS.RstockRatio(iGroup), cCore.NULL_VALUE))
                mseDS.RHalfB0Ratio(iGroup) = CSng(Me.ReadSafe(drow, "RHalfB0Ratio", mseDS.RHalfB0Ratio(iGroup), cCore.NULL_VALUE))
                mseDS.cvRec(iGroup) = CSng(Me.ReadSafe(drow, "RecruitmentCV", mseDS.cvRec(iGroup), cCore.NULL_VALUE))

                ' Me.LoadFishMortShape(CInt(drow("FishMortShapeID")), iGroup)

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EcoSim group info for group {1}", ex.Message, iGroup))
                bSucces = False
            End Try
        Next
        dt.Clear()

        bSucces = bSucces And Me.LoadEcosimGroupYear(iScenarioID)
        Return bSucces

    End Function

    Private Function LoadEcosimGroupYear(ByVal iScenarioID As Integer) As Boolean

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioGroupYear")
        Dim iGroupID As Integer = -1
        Dim iGroup As Integer = -1
        Dim iYear As Integer = -1
        Dim bSucces As Boolean = True

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows
            Try
                iGroupID = CInt(drow("GroupID"))
                iGroup = Array.IndexOf(ecosimDS.GroupDBID, iGroupID)
                iYear = CInt(drow("TimeYear"))
                If (iGroup > 0) And (iGroup <= ecosimDS.nGroups) And
                   (iYear > 0) And (iYear <= mseDS.nYears) Then
                    mseDS.CVBiomT(iGroup, iYear) = CSng(drow("CVBiom"))
                End If
            Catch ex As Exception
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    Private Function LoadEcosimFleets(ByVal iScenarioID As Integer) As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioFleet")
        Dim dtFishMort As DataTable = Me.ReadTable("EcosimShapeFishRate")
        Dim iFleet As Integer = 0
        Dim iFleetID As Integer = -1
        Dim iShapeID As Integer = -1
        Dim bSucces As Boolean = True
        Dim asDummy(ecosimDS.NTimes) As Single

        Dim dtNewFleetShapes As New Dictionary(Of Integer, Integer)

        For iPt As Integer = 0 To ecosimDS.NTimes : asDummy(iPt) = 1.0 : Next

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        For Each drow As DataRow In dt.DefaultView.ToTable.Rows

            iFleetID = CInt(drow("EcopathFleetID"))
            iFleet = Array.IndexOf(ecopathDS.FleetDBID, iFleetID)
            Debug.Assert(iFleet > 0)

            iShapeID = CInt(Me.ReadSafe(drow, "FishRateShapeID", -1))
            Debug.Assert(iShapeID > 0, "No effort shape defined for fleet " & iFleet)

            If iShapeID > -1 Then
                ' JS 10Aug07: Don't fail in case FishRateShape is missing. Only those present are loaded, only those loaded are saved.
                '             Since these shapes do not need to be present we can be somewhat forgiving in this particular case.
                If Not LoadFishingRateShape(dtFishMort, iShapeID, iFleet) Then
                    Me.LogMessage(cStringUtils.Localize("Warning: Fishing rate shape {0} is referenced but not present in database for EcoSim fleet {1} (ID {2})", iShapeID, iFleet, iFleetID))
                End If
            End If

            Try
                ecosimDS.FleetDBID(iFleet) = CInt(drow("FleetID"))
                ecosimDS.Epower(iFleet) = CSng(Me.ReadSafe(drow, "Epower", 3))
                ecosimDS.PcapBase(iFleet) = CSng(Me.ReadSafe(drow, "PCapBase", 0.5))
                ecosimDS.CapDepreciate(iFleet) = CSng(Me.ReadSafe(drow, "CapDepreciate", 0.06))
                ecosimDS.CapBaseGrowth(iFleet) = CSng(Me.ReadSafe(drow, "CapBaseGrowth", 0.2))
                ecosimDS.EffortConversionFactor(iFleet) = CSng(Me.ReadSafe(drow, "EffortConversionFactor", 1.0!))

                mseDS.MaxEffort(iFleet) = CSng(Me.ReadSafe(drow, "MaxEffort", cCore.NULL_VALUE))
                mseDS.QuotaType(iFleet) = DirectCast(CInt(Me.ReadSafe(drow, "QuotaType", 0)), eQuotaTypes)
                mseDS.CVFest(iFleet) = CSng(Me.ReadSafe(drow, "CV", mseDS.CVFest(iFleet)))
                mseDS.Qgrow(iFleet) = CSng(Me.ReadSafe(drow, "QIncrease", mseDS.Qgrow(iFleet)))

                mseDS.DefaultCatchBoundsFleet(iFleet)
                mseDS.CatchFleetBounds(iFleet).Lower = CSng(Me.ReadSafe(drow, "CatchRefLower", mseDS.CatchFleetBounds(iFleet).Lower))
                mseDS.CatchFleetBounds(iFleet).Upper = CSng(Me.ReadSafe(drow, "CatchRefUpper", mseDS.CatchFleetBounds(iFleet).Upper))
                mseDS.EffortFleetBounds(iFleet).Lower = CSng(Me.ReadSafe(drow, "EffortRefLower", mseDS.EffortFleetBounds(iFleet).Lower))
                mseDS.EffortFleetBounds(iFleet).Upper = CSng(Me.ReadSafe(drow, "EffortRefUpper", mseDS.EffortFleetBounds(iFleet).Upper))
                'mseDS.MSYEvaluateFleet(iFleet) = (CInt(Me.ReadSafe(drow, "MSYEvaluateFleet", True)) = 1)

            Catch ex As Exception
                bSucces = False
            End Try

        Next
        dt.Clear()

        bSucces = bSucces And Me.LoadEcosimFleetYear(iScenarioID)
        bSucces = bSucces And Me.LoadEcosimQuota(iScenarioID)

        Return bSucces

    End Function

    Private Function LoadEcosimFleetYear(ByVal iScenarioID As Integer) As Boolean

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioFleetYear")
        Dim iFleetID As Integer = -1
        Dim iFleet As Integer = -1
        Dim iYear As Integer = -1
        Dim bSucces As Boolean = True

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows
            Try
                iFleetID = CInt(drow("FleetID"))
                iFleet = Array.IndexOf(ecosimDS.FleetDBID, iFleetID)
                iYear = CInt(drow("TimeYear"))
                If (iFleet > 0) And (iFleet <= ecosimDS.nGear) And
                   (iYear > 0) And (iYear <= mseDS.nYears) Then
                    mseDS.CVFT(iFleet, iYear) = CSng(drow("CV"))
                End If

            Catch ex As Exception
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    Private Function LoadEcosimQuota(ByVal iScenarioID As Integer) As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioQuota")
        Dim iFleetID As Integer = -1
        Dim iFleet As Integer = -1
        Dim iGroupID As Integer = -1
        Dim iGroup As Integer = -1
        Dim bSucces As Boolean = True

        For Each drow As DataRow In dt.Rows

            Try
                iFleetID = CInt(drow("FleetID"))
                iFleet = Array.IndexOf(ecopathDS.FleetDBID, iFleetID)

                iGroupID = CInt(drow("EcosimGroupID"))
                iGroup = Array.IndexOf(ecosimDS.GroupDBID, iGroupID)

                If (iFleet > 0) And (iGroup > 0) Then
                    mseDS.Quotashare(iFleet, iGroup) = CSng(Me.ReadSafe(drow, "QuotaShare", mseDS.Quotashare(iFleet, iGroup)))
                    mseDS.Fweight(iFleet, iGroup) = CSng(Me.ReadSafe(drow, "FWeight", 1.0))
                End If

            Catch ex As Exception
                bSucces = False
            End Try
        Next
        dt.Clear()
        Return bSucces

    End Function

    Private Function LoadShapes() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim PredPreyMedDS As cMediationDataStructures = Me.m_core.m_EcoSimData.BioMedData
        Dim LandingsMedDS As cMediationDataStructures = Me.m_core.m_EcoSimData.PriceMedData
        Dim CapEnvResMedDS As cMediationDataStructures = Me.m_core.m_EcoSimData.CapEnvResData
        Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
        Dim dt As DataTable = Me.ReadTable("EcosimShape")
        Dim iShapeID As Integer = 0
        Dim shapeDataType As eDataTypes = eDataTypes.NotSet
        Dim iForcingShape As Integer = 0
        Dim iPredPreyMediationShape As Integer = 0
        Dim iLandingsMediationShape As Integer = 0
        Dim iCapEnvResMediationShape As Integer = 0
        Dim iFishingMortShape As Integer = 0
        Dim iFishRateShape As Integer = 0
        Dim bSucces As Boolean = True

        ecosimDS.NumForcingShapes = 0
        PredPreyMedDS.MediationShapes = 0
        LandingsMedDS.MediationShapes = 0
        CapEnvResMedDS.MediationShapes = 0

        For Each drow As DataRow In dt.Rows
            Select Case DirectCast(CInt(drow("ShapeType")), eDataTypes)
                Case eDataTypes.EggProd, eDataTypes.Forcing : ecosimDS.NumForcingShapes += 1
                Case eDataTypes.Mediation : PredPreyMedDS.MediationShapes += 1
                Case eDataTypes.PriceMediation : LandingsMedDS.MediationShapes += 1
                Case eDataTypes.CapacityMediation : CapEnvResMedDS.MediationShapes += 1
            End Select
        Next

        ecosimDS.DimForcingShapes()
        ecosimDS.InitForcingShapes()
        PredPreyMedDS.ReDimMediation(ecosimDS.nGroups, ecosimDS.nGear)
        LandingsMedDS.ReDimMediation(ecosimDS.nGroups, ecosimDS.nGear)
        CapEnvResMedDS.ReDimMediation(ecosimDS.nGroups, ecosimDS.nGear)

        Dim dtEgg As DataTable = Me.ReadTable("EcosimShapeEggProd")
        Dim dtTime As DataTable = Me.ReadTable("EcosimShapeTime")
        Dim dtMed As DataTable = Me.ReadTable("EcosimShapeMediation")
        For Each drow As DataRow In dt.Rows

            Try

                iShapeID = CInt(drow("ShapeID"))
                shapeDataType = DirectCast(CInt(drow("ShapeType")), eDataTypes)

                Select Case shapeDataType

                    Case eDataTypes.EggProd
                        iForcingShape += 1
                        bSucces = bSucces And Me.LoadEggShape(dtEgg, iShapeID, iForcingShape, CInt(drow("IsSeasonal")) <> 0)

                    Case eDataTypes.Forcing
                        iForcingShape += 1
                        bSucces = bSucces And Me.LoadTimeShape(dtTime, iShapeID, iForcingShape, CInt(drow("IsSeasonal")) <> 0)

                    Case eDataTypes.Mediation
                        iPredPreyMediationShape += 1
                        bSucces = bSucces And Me.LoadMediationShape(dtMed, iShapeID, iPredPreyMediationShape, PredPreyMedDS)

                    Case eDataTypes.PriceMediation
                        iLandingsMediationShape += 1
                        bSucces = bSucces And Me.LoadMediationShape(dtMed, iShapeID, iLandingsMediationShape, LandingsMedDS)

                    Case eDataTypes.CapacityMediation
                        iCapEnvResMediationShape += 1
                        bSucces = bSucces And Me.LoadMediationShape(dtMed, iShapeID, iCapEnvResMediationShape, CapEnvResMedDS)

                    Case eDataTypes.FishingEffort
                        'iFishRateShape += 1
                        'bSucces = bSucces And Me.LoadFishingRateShape(iShapeID, iFishRateShape)

                    Case eDataTypes.FishMort
                        'iFishingMortShape += 1
                        'bSucces = bSucces And Me.LoadFishMortShape(iShapeID, iFishingMortShape)

                    Case Else
                        Debug.Assert(False, cStringUtils.Localize("Cannot load invalid shapetype {0} for shape ID {1}", shapeDataType, iShapeID))

                End Select

            Catch ex As Exception
                bSucces = False
            End Try
        Next

        dt = Me.ReadTable("EcosimScenario")
        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        Try
            Dim drow As DataRow = dt.DefaultView.ToTable.Rows(0)
            ' Read and assign scenario forcing shape number(s)
            iForcingShape = CInt(Me.ReadSafe(drow, "NutForcingShapeID", 0))
            ecosimDS.NutForceNumber = Math.Max(0, Array.IndexOf(ecosimDS.ForcingDBIDs, iForcingShape))
        Catch ex As Exception
            bSucces = False
        End Try
        dt.Clear()

        bSucces = bSucces And Me.LoadEcosimVulnerabilities()
        bSucces = bSucces And Me.LoadPredPreyInteractions()
        bSucces = bSucces And Me.LoadLandingInteractions()
        bSucces = bSucces And Me.LoadMediationWeights()
        bSucces = bSucces And Me.LoadStanzaShapeAssignments()
        bSucces = bSucces And Me.LoadEcosimCapacityDrivers()

        Return bSucces

    End Function

#Region " Shape load helpers "

    Private Function LoadEggShape(ByVal dt As DataTable,
                                  ByVal iShapeID As Integer,
                                  ByVal iForcingShape As Integer,
            Optional ByVal bIsSeasonal As Boolean = False) As Boolean

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim shapeParms As New cEcosimDatastructures.ShapeParameters()
        Dim drow As DataRow = Nothing
        Dim astrZScale() As String
        Dim bSucces As Boolean = True

        'drowShape = Me.Getdrow(cStringUtils.Localize("SELECT * FROM EcosimShapeEggProd WHERE (ShapeID={0})", iShapeID))
        dt.DefaultView.RowFilter = CStr("ShapeID=" & iShapeID)
        Try
            drow = dt.DefaultView.ToTable.Rows(0)

            shapeParms.ShapeFunctionType = CLng(Me.ReadSafe(drow, "FunctionType", 0))
            shapeParms.ShapeFunctionParams = cStringUtils.StringToParamArray(CStr(Me.ReadSafe(drow, "FunctionParams", "")))

            ' Read z-scale
            Dim sLast As Single = 1
            astrZScale = Me.SplitNumberString(CStr(drow("Zscale")))
            For ipt As Integer = 1 To Math.Min(ecosimDS.ForcePoints, astrZScale.Length)
                sLast = cStringUtils.ConvertToSingle(astrZScale(ipt - 1), 1)
                ecosimDS.zscale(ipt, iForcingShape) = sLast
            Next ipt
            For ipt As Integer = Math.Min(ecosimDS.ForcePoints, astrZScale.Length) + 1 To ecosimDS.ForcePoints
                ecosimDS.zscale(ipt, iForcingShape) = sLast
            Next

            ecosimDS.ForcingShapeParams(iForcingShape) = shapeParms
            ecosimDS.ForcingDBIDs(iForcingShape) = iShapeID
            ecosimDS.ForcingTitles(iForcingShape) = CStr(drow("Title"))
            ecosimDS.ForcingShapeType(iForcingShape) = eDataTypes.EggProd
            ecosimDS.isSeasonal(iForcingShape) = bIsSeasonal

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EggShape {1}", ex.Message, iShapeID))
            bSucces = False
        End Try
        dt.DefaultView.RowFilter = ""

        Return bSucces

    End Function

    Private Function LoadTimeShape(ByVal dtTime As DataTable,
                                   ByVal iShapeID As Integer,
                                   ByVal iForcingShape As Integer,
                                   Optional ByVal bIsSeasonal As Boolean = False) As Boolean

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim shapeParms As New cEcosimDatastructures.ShapeParameters()
        Dim drow As DataRow = Nothing
        Dim astrZScale() As String
        Dim bSucces As Boolean = True

        dtTime.DefaultView.RowFilter = CStr("ShapeID=" & iShapeID)
        drow = dtTime.DefaultView.ToTable.Rows(0)
        Try

            shapeParms.ShapeFunctionType = CLng(Me.ReadSafe(drow, "FunctionType", 0))
            shapeParms.ShapeFunctionParams = cStringUtils.StringToParamArray(CStr(Me.ReadSafe(drow, "FunctionParams", "")))

            ' Read z-scale
            Dim sLast As Single = 1.0!
            astrZScale = Me.SplitNumberString(CStr(drow("Zscale")))
            For ipt As Integer = 1 To Math.Min(ecosimDS.ForcePoints, astrZScale.Length)
                sLast = cStringUtils.ConvertToSingle(astrZScale(ipt - 1), 0)
                ecosimDS.zscale(ipt, iForcingShape) = sLast
            Next ipt
            For ipt As Integer = Math.Min(ecosimDS.ForcePoints, astrZScale.Length) + 1 To ecosimDS.ForcePoints
                ecosimDS.zscale(ipt, iForcingShape) = sLast
            Next

            ecosimDS.ForcingShapeParams(iForcingShape) = shapeParms
            ecosimDS.ForcingDBIDs(iForcingShape) = iShapeID
            ecosimDS.ForcingTitles(iForcingShape) = CStr(drow("Title"))
            ecosimDS.ForcingShapeType(iForcingShape) = eDataTypes.Forcing
            ecosimDS.isSeasonal(iForcingShape) = bIsSeasonal

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading TimeShape {1}", ex.Message, iShapeID))
            bSucces = False
        End Try
        dtTime.DefaultView.RowFilter = ""

        Return bSucces

    End Function

    Private Function LoadMediationShape(ByVal dtMed As DataTable,
                                        ByVal iShapeID As Integer,
                                        ByVal iMediationShape As Integer,
                                        ByVal medData As cMediationDataStructures) As Boolean

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim shapeParms As New cEcosimDatastructures.ShapeParameters()
        Dim astrZScale() As String
        Dim bSucces As Boolean = True

        'drowShape = Me.Getdrow(cStringUtils.Localize("SELECT * FROM EcosimShapeMediation WHERE (ShapeID={0})", iShapeID))
        dtMed.DefaultView.RowFilter = CStr("ShapeID=" & iShapeID)

        Try
            Dim drow As DataRow = dtMed.DefaultView.ToTable.Rows(0)

            shapeParms.ShapeFunctionType = CLng(Me.ReadSafe(drow, "FunctionType", 0))
            shapeParms.ShapeFunctionParams = cStringUtils.StringToParamArray(CStr(Me.ReadSafe(drow, "FunctionParams", "")))

            ' Read z-scale
            astrZScale = Me.SplitNumberString(CStr(drow("Zscale")))
            ' Write points
            For ipt As Integer = 1 To Math.Min(medData.NMedPoints, astrZScale.Length)
                medData.Medpoints(ipt, iMediationShape) = cStringUtils.ConvertToSingle(astrZScale(ipt - 1), 0)
            Next ipt
            For ipt As Integer = Math.Min(medData.NMedPoints, astrZScale.Length) + 1 To medData.NMedPoints
                medData.Medpoints(ipt, iMediationShape) = 1.0
            Next

            medData.MediationShapeParams(iMediationShape) = shapeParms
            medData.MediationDBIDs(iMediationShape) = iShapeID
            medData.MediationTitles(iMediationShape) = CStr(drow("Title"))
            medData.IMedBase(iMediationShape) = CInt(Me.ReadSafe(drow, "IMedBase", 1200 / 3))
            medData.XAxisMin(iMediationShape) = CSng(Me.ReadSafe(drow, "XAxisMin", 0))
            medData.XAxisMax(iMediationShape) = CSng(Me.ReadSafe(drow, "XAxisMax", 1))

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading MediationShape {1}", ex.Message, iShapeID))
            bSucces = False
        End Try

        Return bSucces

    End Function

    Private Function LoadEcosimVulnerabilities() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioForcingMatrix")
        Dim iPredator As Integer = 0
        Dim iPrey As Integer = 0
        Dim bSucces As Boolean = True

        For iPredator = 1 To Me.m_core.nGroups
            For iPrey = 1 To Me.m_core.nGroups
                ecosimDS.VulMult(iPrey, iPredator) = 2.0!
            Next iPrey
        Next iPredator

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows

            Try
                ' Find iPredator
                iPredator = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("PredID")))
                ' Find iPrey
                iPrey = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("PreyID")))

                If (iPredator > -1 And iPrey > -1) Then
                    ecosimDS.VulMult(iPrey, iPredator) = CSng(drow("vulnerability"))
                End If

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading ForcingMatrix", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    Private Function LoadPredPreyInteractions() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioPredPreyShape")
        Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
        Dim iPredator As Integer = 0
        Dim iPrey As Integer = 0
        Dim iShapeID As Integer = 0
        Dim iShape As Integer = 0
        Dim bSucces As Boolean = True
        Dim iFNo(ecosimDS.nGroups, ecosimDS.nGroups) As Integer

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows

            Try

                ' Find iPredator
                iPredator = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("PredID")))
                ' Find iPrey
                iPrey = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("PreyID")))
                ' Next shape
                iFNo(iPrey, iPredator) += 1
                ' Protect from data overflow
                If (iFNo(iPrey, iPredator) <= cMediationDataStructures.MAXFUNCTIONS) Then
                    ' Resolve shape ID
                    iShapeID = CInt(drow("ShapeID"))
                    ' Determine shape type
                    iShape = Array.IndexOf(ecosimDS.BioMedData.MediationDBIDs, iShapeID)
                    ' Is a mediation shape?
                    If iShape <> -1 Then
                        ' #Yes: flag as mediation shape
                        ecosimDS.BioMedData.IsMedFunction(iPrey, iPredator, iFNo(iPrey, iPredator)) = True
                    Else
                        ' #No: flag as other shape
                        ecosimDS.BioMedData.IsMedFunction(iPrey, iPredator, iFNo(iPrey, iPredator)) = False
                        ' Obtain forcing index
                        iShape = Array.IndexOf(ecosimDS.ForcingDBIDs, iShapeID)
                    End If

                    If iShape <> -1 Then
                        ' Update sim fields
                        ecosimDS.BioMedData.FunctionNumber(iPrey, iPredator, iFNo(iPrey, iPredator)) = iShape
                        Dim appl As eForcingFunctionApplication = CType(CInt(drow("FunctionType")), eForcingFunctionApplication)
                        ' Minor correction which does not warrant a database update.
                        If appl = eForcingFunctionApplication.SearchRate Then
                            If ecopathDS.PP(iPredator) = 1.0 Then
                                appl = eForcingFunctionApplication.ProductionRate
                            ElseIf ecopathDS.PP(iPredator) = 2.0 Then
                                appl = eForcingFunctionApplication.Import
                            End If
                        End If
                        ecosimDS.BioMedData.ApplicationType(iPrey, iPredator, iFNo(iPrey, iPredator)) = appl
                    Else
                        Me.LogMessage(cStringUtils.Localize("Shape {0} cannot be used for pred/prey interactions; assignment discarded", iShapeID))
                    End If
                End If

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading PredPreyInteraction", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    Private Function LoadLandingInteractions() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioLandingsShape")
        Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
        Dim iFleet As Integer = 0
        Dim iGroup As Integer = 0
        Dim iShapeID As Integer = 0
        Dim iShape As Integer = 0
        Dim bSucces As Boolean = True
        Dim iFNo(ecosimDS.nGroups, ecosimDS.nGear) As Integer

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows

            Try

                ' Find iFleet
                iFleet = Array.IndexOf(ecosimDS.FleetDBID, CInt(drow("FleetID")))
                ' Find iGroup
                iGroup = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("GroupID")))
                ' Next shape
                iFNo(iGroup, iFleet) += 1
                ' Resolve shape ID
                iShapeID = CInt(drow("ShapeID"))
                ' Resolve iShape
                iShape = Array.IndexOf(ecosimDS.PriceMedData.MediationDBIDs, iShapeID)

                If iShape > -1 Then
                    ecosimDS.PriceMedData.PriceMedFuncNum(iGroup, iFleet, iFNo(iGroup, iFleet)) = iShape
                Else
                    Me.LogMessage(cStringUtils.Localize("Shape {0} cannot be used for landings interactions; assignment discarded", iShapeID))
                End If

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Landing interaction", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the mediation weights for the active scenario.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadMediationWeights() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim medData As cMediationDataStructures = Nothing
        Dim dt As DataTable = Nothing
        Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
        Dim iGroup As Integer = 0
        Dim iFleet As Integer = 0
        Dim iShape As Integer = 0
        Dim bSucces As Boolean = True

        ' === Pred/prey mediations ===
        medData = ecosimDS.BioMedData
        dt = Me.ReadTable("EcosimScenarioShapeMedWeightsGroup")
        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows
            Try
                iShape = Array.IndexOf(medData.MediationDBIDs, CInt(drow("ShapeID")))
                iGroup = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("GroupID")))
                If (iGroup <> -1 And iShape <> -1) Then
                    medData.MedWeights(iGroup, iShape) = CSng(drow("MedWeights"))
                End If
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading group MediationWeights", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()
        dt = Nothing

        dt = Me.ReadTable("EcosimScenarioShapeMedWeightsFleet")
        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows
            Try
                iShape = Array.IndexOf(medData.MediationDBIDs, CInt(drow("ShapeID")))
                ' Unfortunate legacy: fleet refers to Ecopath fleet, not Ecosim as it should have
                iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(drow("FleetID")))
                If (iFleet <> -1 And iShape <> -1) Then
                    medData.MedWeights(iFleet + ecosimDS.nGroups, iShape) = CSng(drow("MedWeights"))
                End If
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading fleet MediationWeights", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()
        dt = Nothing

        ' === Landings mediations === 
        medData = ecosimDS.PriceMedData
        dt = Me.ReadTable("EcosimScenarioShapeMedWeightsLandings")
        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows
            Try
                iShape = Array.IndexOf(medData.MediationDBIDs, CInt(drow("ShapeID")))
                iGroup = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("GroupID")))
                iFleet = Array.IndexOf(ecosimDS.FleetDBID, CInt(drow("FleetID")))
                If (iGroup > 0 And iShape > 0) Then
                    iFleet = Math.Max(0, iFleet)
                    medData.MedPriceWeights(iGroup, iFleet, iShape) = CSng(drow("MedWeights"))
                End If
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading group MediationWeights", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()
        dt = Nothing

        Return bSucces

    End Function

    Private Function LoadStanzaShapeAssignments() As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
        Dim dt As DataTable = Me.ReadTable("EcosimStanzaShape")
        Dim iStanza As Integer = 0
        Dim iShape As Integer = 0
        Dim bSucces As Boolean = True

        For Each drow As DataRow In dt.Rows
            Try
                ' Get iStanza 
                iStanza = Array.IndexOf(stanzaDS.StanzaDBID, CInt(drow("StanzaID")))
                ' Is valid stanza?
                If (iStanza > 0) Then
                    ' #Yes: has egg production shape?
                    If Not Convert.IsDBNull(drow("EggprodShapeID")) Then
                        ' #Yes: resolve shape index iShape
                        iShape = Array.IndexOf(ecosimDS.ForcingDBIDs, CInt(drow("EggprodShapeID")))
                        ' Is a valid shape index?
                        If (iShape > 0) Then
                            ' #Yes: assign
                            stanzaDS.EggProdShapeSplit(iStanza) = iShape
                        End If
                    End If
                    ' #Yes: has hatch code forcing shape?
                    If Not Convert.IsDBNull(drow("HatchCodeShapeID")) Then
                        ' #Yes: resolve shape index iShape
                        iShape = Array.IndexOf(ecosimDS.ForcingDBIDs, CInt(drow("HatchCodeShapeID")))
                        ' Is a valid shape index?
                        If (iShape > 0) Then
                            ' #Yes: assign
                            stanzaDS.HatchCode(iStanza) = iShape
                        End If
                    End If
                End If ' Is valid stanza

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading stanza shape assignments", ex.Message))
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces
    End Function

    Private Function LoadFishingRateShape(ByVal dtFishRate As DataTable,
                                          ByVal iShapeID As Integer,
                                          ByVal iFishingRateShape As Integer) As Boolean

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim strMemo As String = ""
        Dim astrMemoBits() As String
        Dim bSucces As Boolean = True

        If iShapeID = 0 Then Return bSucces

        dtFishRate.DefaultView.RowFilter = CStr("ShapeID=" & iShapeID)
        For Each drow As DataRow In dtFishRate.DefaultView.ToTable.Rows
            Try
                ecosimDS.FishRateGearTitle(iFishingRateShape) = CStr(drow("Title"))
                strMemo = CStr(Me.ReadSafe(drow, "zScale", ""))
                astrMemoBits = strMemo.Trim.Split(CChar(" "))
                For j As Integer = 1 To Math.Min(ecosimDS.NTimes, astrMemoBits.Length)
                    ecosimDS.FishRateGear(iFishingRateShape, j) = cStringUtils.ConvertToSingle(astrMemoBits(j - 1), 1)
                Next
                ecosimDS.FishRateGearDBID(iFishingRateShape) = iShapeID

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading FishingRate {1}", ex.Message, iShapeID))
                bSucces = False
            End Try
        Next

        Return bSucces

    End Function

#End Region ' Shape load helpers

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Load all time series for a given dataset.
    ''' </summary>
    ''' <param name="iDataset">Index of dataset to load.</param>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Public Function LoadTimeSeriesDataset(ByVal iDataset As Integer) As Boolean _
         Implements DataSources.IEcosimDatasource.LoadTimeSeriesDataset

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
        Dim strSQL As String = ""
        Dim astrTimeValues() As String
        Dim iDatasetID As Integer = tsDS.iDatasetDBID(iDataset)
        Dim iTimeSeriesID As Integer = 0
        Dim iSeries As Integer = 1
        Dim iIndex As Integer = 0
        Dim iIndexSec As Integer = 0
        Dim iPoint As Integer = 0
        Dim bSucces As Boolean = True
        Dim dtTS As DataTable = Me.ReadTable("EcosimTimeSeries")
        Dim dtGrp As DataTable = Me.ReadTable("EcosimTimeSeriesGroup")
        Dim dtFlt As DataTable = Me.ReadTable("EcosimTimeSeriesFleet")

        dtTS.DefaultView.RowFilter = CStr("DatasetID=" & iDatasetID)
        dtTS.DefaultView.Sort = "Sequence ASC"

        Dim rows As DataRowCollection = dtTS.DefaultView.ToTable.Rows()

        tsDS.ClearTimeSeries()
        tsDS.ActiveDatasetIndex = iDataset
        tsDS.nMaxYears = tsDS.nDatasetNumPoints(iDataset)
        tsDS.DataSetInterval = tsDS.DataSetIntervals(iDataset)

        ' JS 20oct07: data source should NOT do this; is responsibility of core logic
        tsDS.nGroups = ecopathDS.NumGroups

        ' JS 02Nov12: The database structure cannot cascadingly delete time series when
        ' a pool code target is deleted. This is not a big problem, as PoolCode (indexes)
        ' are translated to database IDs (persistent) upon import. However, lingering 
        ' time series create a bit of a mess.

        If (iDataset > 0) Then
            Try
                tsDS.nTimeSeries = rows.Count
            Catch ex As Exception
                tsDS.nTimeSeries = 0
            End Try
        End If

        tsDS.RedimTimeSeries()
        tsDS.RedimEnabledTimeSeries()

        If tsDS.nTimeSeries = 0 Then Return bSucces

        Try
            For iRow As Integer = 0 To rows.Count - 1

                Dim drow As DataRow = rows(iRow)
                Dim iTSID As Integer = CInt(drow("TimeSeriesID"))

                tsDS.iTimeSeriesDBID(iSeries) = iTSID
                tsDS.strName(iSeries) = CStr(drow("DatName"))
                tsDS.TimeSeriesType(iSeries) = DirectCast(CInt(drow("DatType")), eTimeSeriesType)
                tsDS.sWeight(iSeries) = CSng(drow("WtType"))
                tsDS.sCV(iSeries) = CSng(Me.ReadSafe(drow, "CV", 0.0!))

                Select Case cTimeSeriesFactory.TimeSeriesCategory(CType(tsDS.TimeSeriesType(iSeries), eTimeSeriesType))

                    Case eTimeSeriesCategoryType.Group
                        For Each drowSub As DataRow In dtGrp.Select("TimeSeriesID=" & iTSID)
                            Try
                                iIndex = Array.IndexOf(ecopathDS.GroupDBID, CInt(drowSub("GroupID")))
                            Catch ex As Exception
                                iIndex = -1
                            End Try
                        Next

                    Case eTimeSeriesCategoryType.Fleet,
                         eTimeSeriesCategoryType.FleetGroup
                        For Each drowSub As DataRow In dtFlt.Select("TimeSeriesID=" & iTSID)
                            Try
                                iIndex = Array.IndexOf(ecopathDS.FleetDBID, CInt(drowSub("FleetID")))
                                iIndexSec = Array.IndexOf(ecopathDS.GroupDBID, CInt(drowSub("GroupID")))
                            Catch ex As Exception
                                iIndex = -1
                            End Try
                        Next

                    Case eTimeSeriesCategoryType.Forcing
                        Debug.Assert(False, String.Format("Time series {0} should have been imported as a forcing function", iTSID))
                        bSucces = False

                    Case eTimeSeriesCategoryType.NotSet
                        Debug.Assert(False, String.Format("Time series {0} is of an unknown type", iTSID))
                        bSucces = False

                End Select

                tsDS.iPool(iSeries) = iIndex
                tsDS.iPoolSec(iSeries) = Math.Max(0, iIndexSec)

                astrTimeValues = CStr(drow("TimeValues")).Split(CChar(" "))

                For iPoint = 1 To Math.Min(tsDS.nDatasetNumPoints(iDataset), astrTimeValues.Length)
                    Try
                        tsDS.sValues(iPoint, iSeries) = cStringUtils.ConvertToSingle(astrTimeValues(iPoint - 1))
                    Catch ex As Exception
                        ' Woops
                    End Try
                Next

                iSeries += 1
            Next iRow

        Catch ex As Exception
            bSucces = False
        End Try

        Return bSucces
    End Function

    Private Function LoadEcosimCapacityDrivers() As Boolean

        Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
        Dim dt As DataTable = Me.ReadTable("EcosimScenarioCapacityDrivers")
        Dim bSucces As Boolean = True

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        Dim rows As DataRowCollection = dt.DefaultView.ToTable.Rows()

        Try
            For iRow As Integer = 0 To rows.Count - 1
                Dim drow As DataRow = rows(iRow)

                Dim iGroup As Integer = Array.IndexOf(ecosimDS.GroupDBID, CInt(drow("GroupID")))
                Dim iShapeDriver As Integer = Array.IndexOf(ecosimDS.ForcingDBIDs, CInt(drow("DriverID")))
                Dim iShapeResponse As Integer = Array.IndexOf(Me.m_core.CapacityMapInteractionManager.MediationData.MediationDBIDs, CInt(drow("ResponseID")))

                If (iGroup > 0) And (iShapeDriver > 0) And (iShapeResponse > 0) Then
                    ecosimDS.EnvRespFuncIndex(iShapeDriver, iGroup) = iShapeResponse
                End If
            Next iRow
        Catch ex As Exception
            bSucces = False
        End Try

        dt.Dispose()

        Return bSucces

    End Function

#End Region ' Ecosim

#Region " Ecospace "

    Public Function LoadEcospaceScenario(ByVal iScenarioID As Integer) As Boolean _
         Implements DataSources.IEcospaceDatasource.LoadEcospaceScenario

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
        Dim spatialDS As cSpatialDataStructures = Me.m_core.m_SpatialData
        Dim drow As DataRow = Nothing
        Dim bSucces As Boolean = True
        Dim dtScenario As DataTable = Me.ReadTable("EcospaceScenario")

        Dim dtImp As DataTable = Me.ReadTable("EcospaceScenarioWeightLayer")
        dtImp.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        Dim dtDrv As DataTable = Me.ReadTable("EcospaceScenarioDriverLayer")
        dtDrv.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        ecospaceDS.NGroups = ecopathDS.NumGroups
        ecospaceDS.nFleets = ecopathDS.NumFleet
        ecospaceDS.nLiving = ecopathDS.NumLiving
        ecospaceDS.nImportanceLayers = dtImp.DefaultView.ToTable.Rows.Count()
        ecospaceDS.nEnvironmentalDriverLayers = dtDrv.DefaultView.ToTable.Rows.Count()

        ' Next is a dangerous solution that may need to be revamped. It is assumed that
        ' SetDefaults properly redimensions the ecospaceDS group variables, which
        ' may wreck havoc if the implementation of SetDefaults were to change.
        ecospaceDS.SetDefaults()

        dtScenario.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        Try
            ' Read the one record
            drow = dtScenario.DefaultView.ToTable.Rows(0)
            ' Remember link with Ecosim scenario, if any
            ecospaceDS.EcosimScenarioDBID = CInt(Me.ReadSafe(drow, "EcosimScenarioID", cCore.NULL_VALUE))
            ecospaceDS.InRow = CInt(drow("Inrow"))
            ecospaceDS.InCol = CInt(drow("Incol"))
            ecospaceDS.CellLength = CSng(drow("CellLength"))
            ecospaceDS.Lat1 = CSng(Me.ReadSafe(drow, "MinLat", 0))
            ecospaceDS.Lon1 = CSng(Me.ReadSafe(drow, "MinLon", 0))
            ecospaceDS.TimeStep = CSng(Me.ReadSafe(drow, "TimeStep", 0))
            ecospaceDS.PredictEffort = (CInt(Me.ReadSafe(drow, "PredictEffort", True)) <> 0)
            ecospaceDS.AssumeSquareCells = (CInt(Me.ReadSafe(drow, "AssumeSquareCells", True)) <> 0)
            ecospaceDS.ProjectionString = CStr(Me.ReadSafe(drow, "CoordinateSystemWKT", cEcospaceDataStructures.DEFAULT_COORDINATESYSTEM))

            ' JS 05apr08: pragmatic fix to prevent mayhem
            If ecospaceDS.TimeStep <= 0 Then ecospaceDS.TimeStep = 1.0! / cCore.N_MONTHS

            ecospaceDS.TotalTime = CSng(drow("TotalTime"))
            ecospaceDS.IFDPower = CSng(drow("IFDPower"))
            ecospaceDS.nSpaceSolverThreads = CInt(drow("NumThreads"))
            ecospaceDS.nGridSolverThreads = CInt(drow("NumThreads"))
            ecospaceDS.nEffortDistThreads = CInt(drow("NumThreads"))
            ecospaceDS.nRegions = CInt(Me.ReadSafe(drow, "NumRegions", 0))
            ecospaceDS.AdjustSpace = (CInt(drow("AdjustSpace")) <> 0)
            ecospaceDS.UseExact = (CInt(drow("UseExact")) <> 0)
            ecospaceDS.Tol = CSng(Me.ReadSafe(drow, "Tolerance", 0.01!))

            stanzaDS.NPacketsMultiplier = CSng(drow("NumPacketsMultiplier"))

            Select Case CInt(drow("ModelType"))
                Case 0
                    ecospaceDS.NewMultiStanza = False
                    ecospaceDS.UseIBM = False
                Case 1
                    ecospaceDS.UseIBM = True
                    ecospaceDS.NewMultiStanza = False
                Case Else
                    ecospaceDS.UseIBM = False
                    ecospaceDS.NewMultiStanza = True
            End Select

        Catch ex As Exception
            Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Ecospace Scenario {1}", ex.Message, iScenarioID))
            bSucces = False
        End Try

        ' JS 08Jl14: redimForRun is called too many times

        'set the size of the variables that hold the map data to InRow and InCol
        'Call cEcospace.redimForRun() First because it allocates bigger blocks of memory
        'this should help Out of Memory exceptions caused by heap fragmentation by doing the big stuff first
        Me.m_core.m_Ecospace.redimForRun()
        ecospaceDS.ReDimMapDims()
        ecospaceDS.ReDimFleets()

        ' Set active scenario
        ecopathDS.ActiveEcospaceScenario = Array.IndexOf(ecopathDS.EcospaceScenarioDBID, iScenarioID)

        ' Load base map first
        bSucces = bSucces And Me.LoadEcospaceMap(dtScenario, iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceHabitats(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceMPAs(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceGroups(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceFleets(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceMonthlyMaps(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceWeightLayers(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceDriverLayers(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceDataConnections(iScenarioID)
        'bSucces = bSucces And Me.LoadAuxillaryData()

        dtScenario.Clear()

        Me.ClearChanged()

        Return bSucces
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the spatial data associated with an Ecospace scenario.
    ''' </summary>
    ''' <param name="iScenarioID">The scenario to load the data for.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcospaceMap(dtScenario As DataTable, ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim drow As DataRow = Nothing
        Dim bSucces As Boolean = True

        dtScenario.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        Try
            drow = dtScenario.DefaultView.ToTable.Rows(0)

            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "DepthMap", "")), ecospaceDS.DepthInput, ecospaceDS.InRow, ecospaceDS.InCol)
            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "RelPPMap", "")), ecospaceDS.RelPP, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "RelCinMap", "")), ecospaceDS.RelCin, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "FlowMap", "")), ecospaceDS.Xvel, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "DepthAMap", "")), ecospaceDS.DepthA, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "RegionMap", "")), ecospaceDS.Region, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
            bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "ExclusionMap", "")), ecospaceDS.Excluded, ecospaceDS.InRow, ecospaceDS.InCol)

        Catch ex As Exception
            bSucces = False
        End Try
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the spatial data associated with an Ecospace scenario.
    ''' </summary>
    ''' <param name="iScenarioID">The scenario to load the data for.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcospaceMonthlyMaps(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dtMaps As DataTable = Me.ReadTable("EcospaceScenarioMonth")
        Dim bSucces As Boolean = True
        Dim iMonth As Integer = 0

        dtMaps.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        dtMaps.DefaultView.Sort = "MonthID ASC"

        For Each drow As DataRow In dtMaps.DefaultView.ToTable.Rows
            iMonth = CInt(Me.ReadSafe(drow, "MonthID", 0))
            If (1 <= iMonth And iMonth <= cCore.N_MONTHS) Then
                bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "WindXVelMap", "")), iMonth, cStringUtils.eFilterIndexTypes.LastIndex, ecospaceDS.Xv, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "WindYVelMap", "")), iMonth, cStringUtils.eFilterIndexTypes.LastIndex, ecospaceDS.Yv, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "AdvectionXVelMap", "")), ecospaceDS.MonthlyXvel(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "AdvectionYVelMap", "")), ecospaceDS.MonthlyYvel(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.ReadSafe(drow, "UpwellingMap", "")), ecospaceDS.MonthlyUpWell(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
            End If
        Next

        dtMaps.Clear()

        Return bSucces

    End Function

    Private Function LoadEcospaceHabitats(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dtHab As DataTable = Me.ReadTable("EcospaceScenarioHabitat")
        Dim strMap As String = ""
        Dim i As Integer = 0
        Dim iTime As Integer = 0
        Dim iSequence As Integer = 0
        Dim bSucces As Boolean = True

        dtHab.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        dtHab.DefaultView.Sort = "Sequence ASC"
        ecospaceDS.NoHabitats = dtHab.DefaultView.ToTable.Rows.Count()

        ecospaceDS.RedimHabitatVariables(False)

        For Each drow As DataRow In dtHab.DefaultView.ToTable.Rows
            Try
                ecospaceDS.HabitatDBID(i) = CInt(drow("HabitatID"))
                ecospaceDS.HabitatText(i) = CStr(drow("HabitatName"))
                strMap = CStr(Me.ReadSafe(drow, "HabitatMap", ""))
                ' Read only water cells with values for this habitat index

                cStringUtils.StringToArray(strMap, ecospaceDS.PHabType(i), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                i += 1
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Ecospace habitat for habitat {1}", ex.Message, i))
                bSucces = False
            End Try
        Next

        dtHab.Clear()

        Return bSucces

    End Function

    Private Function LoadEcospaceMPAs(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dtMPA As DataTable = Me.ReadTable("EcospaceScenarioMPA")
        Dim strMPAMonth As String = ""
        Dim strMap As String = ""
        Dim bSucces As Boolean = True
        Dim iMPA As Integer = 1

        dtMPA.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        dtMPA.DefaultView.Sort = "Sequence ASC"

        ecospaceDS.MPAno = dtMPA.DefaultView.ToTable.Rows.Count()
        ecospaceDS.RedimMPAVariables()

        For Each drow As DataRow In dtMPA.DefaultView.ToTable.Rows
            Try
                ecospaceDS.MPADBID(iMPA) = CInt(drow("MPAID"))
                ecospaceDS.MPAname(iMPA) = CStr(drow("MPAName"))
                ' Read month '0' or '1' pattern (yeah yeah, could have been done with 12-bit bitflags LONG value)
                strMPAMonth = CStr(drow("MPAMonth"))
                For iMonth As Integer = 0 To Math.Min(cCore.N_MONTHS, strMPAMonth.Length) - 1
                    ' MPAmonth is an array of boolean flags depicting wheter an MPA is open for fishing,
                    ' where closed months are stored as 0, and open months are stored as 1
                    ' EcospaceDS.MPAmonth: False if closed, True if open
                    ecospaceDS.MPAmonth(iMonth + 1, iMPA) = (strMPAMonth.Substring(iMonth, 1) = "1")
                Next iMonth
                strMap = CStr(Me.ReadSafe(drow, "MPAMap", ""))
                bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.MPA(iMPA), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True, True)
                iMPA += 1

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EcospaceScenarioMPA {1}", ex.Message, iMPA))
                bSucces = False
            End Try
        Next
        dtMPA.Clear()

        Return bSucces

    End Function

    Private Function LoadEcospaceGroups(ByVal iScenarioID As Integer) As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dtGroup As DataTable = Me.ReadTable("EcospaceScenarioGroup")
        Dim bSucces As Boolean = True
        Dim astrSplit As String() = Nothing
        Dim strMap As String = ""
        Dim iGroup As Integer = 0

        ' Clear
        For iGroup = 1 To Me.m_core.nGroups
            For iRow As Integer = 0 To ecospaceDS.InRow
                For iCol As Integer = 0 To ecospaceDS.InCol
                    ecospaceDS.HabCapInput(iGroup)(iRow, iCol) = 1.0
                Next iCol
            Next iRow
        Next iGroup

        dtGroup.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dtGroup.DefaultView.ToTable.Rows()
            Try
                ' Resolve group index
                iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(drow("EcopathGroupID")))
                ' Sanity check
                Debug.Assert(iGroup > -1)
                ' Load the data
                ecospaceDS.GroupDBID(iGroup) = CInt(drow("GroupID"))
                ecospaceDS.EcopathGroupDBID(iGroup) = CInt(drow("EcopathGroupID"))
                ecospaceDS.Mvel(iGroup) = CSng(drow("Mvel"))
                ecospaceDS.RelMoveBad(iGroup) = CSng(drow("RelMoveBad"))
                ecospaceDS.RelVulBad(iGroup) = CSng(drow("RelVulBad"))
                ecospaceDS.EatEffBad(iGroup) = 1 'CSng(drow("EatEffBad"))
                ' VERIFY_JS: RiskSens imported but not used in EwE5
                ' ecospaceDS.RiskSens(i) = CSng(drow("RiskSens"))
                ecospaceDS.IsAdvected(iGroup) = (CInt(drow("IsAdvected")) <> 0)
                ecospaceDS.IsMigratory(iGroup) = (CInt(drow("IsMigratory")) <> 0)
                ecospaceDS.barrierAvoidanceWeight(iGroup) = CSng(Me.ReadSafe(drow, "BarrierAvoidanceWeight", ecospaceDS.barrierAvoidanceWeight(iGroup)))
                ecospaceDS.CapCalType(iGroup) = DirectCast(CInt(Me.ReadSafe(drow, "CapacityCalType", eEcospaceCapacityCalType.Habitat)), eEcospaceCapacityCalType)

                '' ---------------------------
                '' MIGRATION_UPD: BEGIN REMOVE
                'ecospaceDS.MigConcRow(iGroup) = CSng(drow("MigConcRow"))
                'ecospaceDS.MigConcCol(iGroup) = CSng(drow("MigConcCol"))

                '' Monthly PrefRow
                'astrSplit = CStr(drow("PrefRow")).Split(CChar(" "))
                'For iMonth As Integer = 1 To Math.Min(cCore.N_MONTHS, astrSplit.Length)
                '    ecospaceDS.PrefRow(iGroup, iMonth) = cStringUtils.ConvertToInteger(astrSplit(iMonth - 1))
                'Next
                '' Monthly PrefCol
                'astrSplit = CStr(drow("PrefCol")).Split(CChar(" "))
                'For iMonth As Integer = 1 To Math.Min(cCore.N_MONTHS, astrSplit.Length)
                '    ecospaceDS.Prefcol(iGroup, iMonth) = cStringUtils.ConvertToInteger(astrSplit(iMonth - 1))
                'Next
                '' MIGRATION_UPD: END REMOVE
                '' ---------------------------

                strMap = CStr(Me.ReadSafe(drow, "CapacityMap", ""))
                cStringUtils.StringToArray(strMap, ecospaceDS.HabCapInput(iGroup), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Ecospace group {1}", ex.Message, iGroup))
                bSucces = False
            End Try
        Next

        dtGroup.Clear()

        ' Load habitat preferences and migration
        bSucces = bSucces And Me.LoadEcospaceGroupHabitats(iScenarioID) And Me.LoadEcospaceGroupMigration(iScenarioID)
        Return bSucces

    End Function

    Private Function LoadEcospaceGroupHabitats(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dtGH As DataTable = Me.ReadTable("EcospaceScenarioGroupHabitat")
        Dim iGroupID As Integer = 0
        Dim iGroup As Integer = -1
        Dim iHabitatID As Integer = 0
        Dim iHabitat As Integer = -1
        Dim sPreference As Single = 0.0!
        Dim bSucces As Boolean = True

        dtGH.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dtGH.DefaultView.ToTable.Rows()
            Try
                iGroupID = CInt(drow("GroupID"))
                iGroup = Array.IndexOf(ecospaceDS.GroupDBID, iGroupID)

                iHabitatID = CInt(drow("HabitatID"))
                iHabitat = Array.IndexOf(ecospaceDS.HabitatDBID, iHabitatID)

                sPreference = CSng(Me.ReadSafe(drow, "Preference", 1.0))
                ' Sanity check
                If (iGroup = -1) Or (iHabitat = -1) Then
                    If (iGroup = -1) Then Me.LogMessage(cStringUtils.Localize("LoadEcospaceGroupHabitats: Group ID {0} no longer exist", iGroupID))
                    If (iHabitat = -1) Then Me.LogMessage(cStringUtils.Localize("LoadEcospaceGroupHabitats: Habitat ID {1} no longer exist", iHabitatID))
                Else
                    ' Flag as preferred
                    ecospaceDS.PrefHab(iGroup, 0) = 0
                    ecospaceDS.PrefHab(iGroup, iHabitat) = sPreference
                End If

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Ecospace group preferred habitats", ex.Message))
                bSucces = False
            End Try
        Next
        dtGH.Clear()
        Return bSucces

    End Function

    Private Function LoadEcospaceGroupMigration(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dtGH As DataTable = Me.ReadTable("EcospaceScenarioGroupMigration")
        Dim iGroupID As Integer = 0
        Dim iGroup As Integer = -1
        Dim iMonth As Integer = 0
        'Dim sConcentration As Single = 0.0!
        Dim strMap As String = ""
        Dim bSucces As Boolean = True

        ecospaceDS.RedimMigrationMaps(bClearExisting:=False)

        dtGH.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dtGH.DefaultView.ToTable.Rows()
            Try
                iGroupID = CInt(drow("GroupID"))
                iGroup = Array.IndexOf(ecospaceDS.GroupDBID, iGroupID)
                iMonth = CInt(drow("MonthID"))
                'sConcentration = CSng(Me.ReadSafe(drow, "Concentration", 1.0))
                ' Sanity check
                If (iGroup = -1) Then
                    Me.LogMessage(cStringUtils.Localize("LoadEcospaceGroupHabitats: Group ID {0} no longer exist", iGroupID))
                Else
                    strMap = CStr(Me.ReadSafe(drow, "Map", ""))
                    cStringUtils.StringToArray(strMap, ecospaceDS.MigMaps(iGroup, iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                    '' Read monthly concentration value
                    'ecospaceDS.MigConc(iGroup, iMonth) = CSng(Me.ReadSafe(drow, "Concentration", 1.0))
                End If

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Ecospace group preferred habitats", ex.Message))
                bSucces = False
            End Try
        Next
        dtGH.Clear()
        Return bSucces

    End Function

    Private Function LoadEcospaceFleets(ByVal iScenarioID As Integer) As Boolean

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.EcopathDataStructures
        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioFleet")
        Dim strMap As String = ""
        Dim bSucces As Boolean = True
        Dim iFleet As Integer = 1

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)

        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(drow("EcopathFleetID")))
                ecospaceDS.FleetDBID(iFleet) = CInt(drow("FleetID"))
                ecospaceDS.EcopathFleetDBID(iFleet) = CInt(drow("EcopathFleetID"))
                ecospaceDS.EffPower(iFleet) = CSng(drow("EffPower"))

                ' Read port map for a given fleet and land cells only
                strMap = CStr(Me.ReadSafe(drow, "PortMap", ""))
                bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.Port(iFleet), ecospaceDS.InRow, ecospaceDS.InCol)

                ' Read sailing cost map for a given fleet and water cells only
                strMap = CStr(Me.ReadSafe(drow, "SailCostMap", ""))
                bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.Sail(iFleet), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                iFleet += 1

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading Ecospace fleet {1}", ex.Message, iFleet))
                bSucces = False
            End Try
        Next

        dt.Clear()

        bSucces = bSucces And Me.LoadEcospaceHabitatFishery(iScenarioID)
        bSucces = bSucces And Me.LoadEcospaceMPAFishery(iScenarioID)

        Return bSucces

    End Function

    Private Function LoadEcospaceHabitatFishery(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioHabitatFishery")
        Dim iFleet As Integer = 0
        Dim iHabitat As Integer = 0
        Dim bSucces As Boolean = True

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                iFleet = Array.IndexOf(ecospaceDS.FleetDBID, CInt(drow("FleetID")))
                iHabitat = Array.IndexOf(ecospaceDS.HabitatDBID, CInt(drow("HabitatID")))
                'jb habitats and fleets both use the zero index
                If (iFleet >= 0 And iHabitat >= 0) Then
                    ' Clear default 'all' habitat assignment
                    ecospaceDS.GearHab(iFleet, 0) = False
                    ecospaceDS.GearHab(iFleet, iHabitat) = True
                End If
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading EcospaceScenarioHabitatFishery for iFleet {1}, iHabitat {2}", ex.Message, iFleet, iHabitat))
                bSucces = False
            End Try
        Next
        dt.Clear()
        Return bSucces

    End Function

    Private Function LoadEcospaceMPAFishery(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioMPAFishery")
        Dim iFleet As Integer = 0
        Dim iMPA As Integer = 0
        Dim bSucces As Boolean = True

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                iFleet = Array.IndexOf(ecospaceDS.FleetDBID, CInt(drow("FleetID")))
                iMPA = Array.IndexOf(ecospaceDS.MPADBID, CInt(drow("MPAID")))
                ' Crash prevention, should not be necessary but hey
                If (iFleet >= 0 And iMPA > 0) Then
                    ' Clear default 'all' habitat assignment
                    ecospaceDS.MPAfishery(iFleet, 0) = False
                    ecospaceDS.MPAfishery(iFleet, iMPA) = True
                End If
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while reading ReadEcospaceMPAFishery for iFleet {1}, iMPA {2}", ex.Message, iFleet, iMPA))
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces
    End Function

    Private Function LoadEcospaceWeightLayers(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioWeightLayer")
        Dim bSucces As Boolean = True
        Dim iRow As Integer = 0
        Dim iCol As Integer = 0
        Dim iLayer As Integer = 0

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                iLayer += 1
                ' Populate it
                ecospaceDS.ImportanceLayerDBID(iLayer) = CInt(drow("LayerID"))
                ecospaceDS.ImportanceLayerName(iLayer) = CStr(drow("Name"))
                ecospaceDS.ImportanceLayerDescription(iLayer) = CStr(drow("Description"))
                ecospaceDS.ImportanceLayerWeight(iLayer) = CSng(drow("Weight"))

                Dim strMap As String = CStr(Me.ReadSafe(drow, "LayerMap", ""))
                bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.ImportanceLayerMap(iLayer),
                                                                 ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)

            Catch ex As Exception
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    Private Function LoadEcospaceDriverLayers(ByVal iScenarioID As Integer) As Boolean

        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioDriverLayer")
        Dim bSucces As Boolean = True
        Dim iRow As Integer = 0
        Dim iCol As Integer = 0
        Dim iLayer As Integer = 0

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        dt.DefaultView.Sort = "Sequence ASC"
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                iLayer += 1
                ecospaceDS.EnvironmentalLayerDBID(iLayer) = CInt(drow("LayerID"))
                ecospaceDS.EnvironmentalLayerName(iLayer) = CStr(drow("LayerName"))
                ecospaceDS.EnvironmentalLayerDescription(iLayer) = CStr(drow("LayerDescription"))
                ecospaceDS.EnvironmentalLayerUnits(iLayer) = CStr(ReadSafe(drow, "LayerUnits", ""))

                Dim strMap As String = CStr(Me.ReadSafe(drow, "LayerMap", ""))
                bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.EnvironmentalLayerMap(iLayer),
                                                                 ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)

            Catch ex As Exception
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces And Me.LoadCapacityDrivers(iScenarioID)

    End Function

    Private Function LoadCapacityDrivers(ByVal iScenarioID As Integer) As Boolean

        Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioCapacityDrivers")
        Dim bSucces As Boolean = True

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                Dim iGroup As Integer = Array.IndexOf(ecospaceDS.GroupDBID, CInt(drow("GroupID")))
                Dim iShape As Integer = Array.IndexOf(Me.m_core.CapacityMapInteractionManager.MediationData.MediationDBIDs, CInt(drow("ShapeID")))
                Dim iMap As Integer = Array.IndexOf(ecospaceDS.EnvironmentalLayerDBID, CInt(drow("VarDBID")))
                'Dim varName As eVarNameFlags = cin.GetVarName(CStr(drow("VarName")))

                'If (iGroup > 0) And (iShape > 0) And (varName <> eVarNameFlags.NotSet) Then
                If (iGroup > 0) And (iShape > 0) Then
                    ' Map pos 0 indicates Depth, any other ID indicates a Driver map
                    ecospaceDS.CapMapFunctions(Math.Max(0, iMap), iGroup) = iShape
                End If
            Catch ex As Exception
                bSucces = False
            End Try
        Next
        dt.Clear()

        Return bSucces

    End Function

    Private Function LoadEcospaceDataConnections(iScenarioID As Integer) As Boolean

        Dim spaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim spatialDS As cSpatialDataStructures = Me.m_core.m_SpatialData
        Dim dt As DataTable = Me.ReadTable("EcospaceScenarioDataConnection")
        Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
        Dim cfg As cSpatialDataStructures.cAdapaterConfiguration = Nothing
        Dim strDatasetGUID As String = ""
        Dim strConverterType As String = ""
        Dim strConverterCfg As String = ""
        Dim bSucces As Boolean = True

        spatialDS.SetDefaults()

        dt.DefaultView.RowFilter = CStr("ScenarioID=" & iScenarioID)
        For Each drow As DataRow In dt.DefaultView.ToTable.Rows()
            Try
                Dim var As eVarNameFlags = cin.GetVarName(CStr(drow("VarName")))
                Dim iLayer As Integer = Array.IndexOf(spaceDS.getLayerIDs(var), CInt(Me.ReadSafe(drow, "LayerID", 1)))
                Dim iConn As Integer = -1

                ' May link to unknown layer
                If (iLayer > 0) Then
                    ' Find next available connection slot
                    For i As Integer = 1 To cSpatialDataStructures.cMAX_CONN
                        Dim item As cSpatialDataStructures.cAdapaterConfiguration = spatialDS.Item(var, iLayer, i)
                        If (item IsNot Nothing) And (iConn = -1) Then
                            If String.IsNullOrWhiteSpace(item.DatasetGUID) Then
                                iConn = i
                            End If
                        End If
                    Next

                    If (iConn > 0) Then
                        Dim item As cSpatialDataStructures.cAdapaterConfiguration = spatialDS.Item(var, iLayer, iConn)
                        item.DatasetGUID = CStr(Me.ReadSafe(drow, "DatasetGUID", ""))
                        item.DatasetTypeName = CStr(Me.ReadSafe(drow, "DatasetTypeName", ""))
                        item.DatasetConfig = Web.HttpUtility.UrlDecode(CStr(Me.ReadSafe(drow, "DatasetCfg", "")))
                        item.ConverterTypeName = CStr(Me.ReadSafe(drow, "ConverterTypeName", ""))
                        item.ConverterConfig = Web.HttpUtility.UrlDecode(CStr(Me.ReadSafe(drow, "ConverterCfg", "")))
                        item.Scale = CSng(Me.ReadSafe(drow, "Scale", 1.0!))
                        item.ScaleType = CType(Me.ReadSafe(drow, "ScaleType", cSpatialScalarDataAdapterBase.eScaleType.Relative), cSpatialScalarDataAdapterBase.eScaleType)

                        ' These datasets are 'virtual', obtained from a foreign model but not properly defined

                    End If
                End If

            Catch ex As Exception
                bSucces = False
                cLog.Write(ex, "cEIIXMLDataSource::LoadDataAdapters")
            End Try

        Next
        dt.Clear()
        Return bSucces

    End Function

#End Region ' Ecospace

#End Region ' Load

#Region " Save "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Initiates a save of an EwE model
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Function SaveModel() As Boolean _
             Implements IEcopathDataSource.SaveModel
        Return False
    End Function

#End Region ' Save

#Region " Save from database "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save an Ecopath database to a file
    ''' </summary>
    ''' <param name="db"></param>
    ''' <param name="strFile"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function SaveFromDB(db As cEwEDatabase, strFile As String) As Boolean

        ' DB must be open!

        Dim conn As OleDbConnection = DirectCast(db.GetConnection(), OleDbConnection)
        Dim dtTables As DataTable = Nothing
        Dim drow As DataRow = Nothing
        Dim doc As New XmlDocument()

        If (conn Is Nothing) Then Return False

        dtTables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New String() {Nothing, Nothing, Nothing, Nothing})
        If (dtTables Is Nothing) Then Return False

        Dim decl As XmlDeclaration = doc.CreateXmlDeclaration("1.0", "", "")
        doc.AppendChild(decl)

        Dim elm As XmlElement = doc.CreateElement("EwEModel")
        Dim xa As XmlAttribute = doc.CreateAttribute("Name")
        xa.InnerText = db.FileName
        elm.Attributes.Append(xa)

        xa = doc.CreateAttribute("DBVersion")
        xa.InnerText = db.GetVersion().ToString
        elm.Attributes.Append(xa)

        xa = doc.CreateAttribute("EwEVersion")
        xa.InnerText = cAssemblyUtils.GetVersion().ToString
        elm.Attributes.Append(xa)
        doc.AppendChild(elm)

        For Each drow In dtTables.Rows
            Try
                Me.SaveTable(db, CStr(drow(2)), doc)
            Catch ex As Exception
                Debug.Assert(False, "Error saving table " & CStr(drow(2)) & ": " & ex.Message)
            End Try
        Next

        doc.Save(strFile)

        Return True

    End Function

    Private Function Columns(ByVal db As cEwEDatabase, strTable As String) As String()

        Dim conn As OleDbConnection = DirectCast(db.GetConnection(), OleDbConnection)
        Dim dtTables As DataTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, New String() {Nothing, Nothing, strTable, Nothing})
        Dim lstrColumns As New List(Of String)
        Dim astrExcl As String() = New String() {}

        ' Has exclusion entries for table?
        If s_dtExcludedDBEntries.ContainsKey(strTable) Then
            ' #Yes: get it
            astrExcl = s_dtExcludedDBEntries(strTable)
            ' Is an empty array?
            If (astrExcl.Length = 0) Then
                ' #Yes: No columns to write
                Return lstrColumns.ToArray()
            End If
        End If

        ' Summarize columns
        For Each drow As DataRow In dtTables.Rows
            Dim strColName As String = CStr(drow("COLUMN_NAME"))
            Dim strColType As String = Me.DataType(CInt(drow("DATA_TYPE"))).ToString()
            If Array.IndexOf(astrExcl, strColName) = -1 Then
                lstrColumns.Add(strColName & ":" & strColType)
            End If
        Next
        Return lstrColumns.ToArray()

    End Function

    Private Function ToFieldValue(rd As IDataReader, strCol As String) As String

        Dim data As Object = rd(strCol)
        Dim bIsXML As Boolean = False

        If Convert.IsDBNull(data) Then Return ""

        If (TypeOf data Is String) Then
            Dim strData As String = CStr(data)
            ' is XML?
            If (strData.IndexOf("<"c) > -1) Then
                Try
                    Dim reader As XmlReader = XmlReader.Create(New StringReader(strData))
                    bIsXML = reader.Read()
                Catch ex As Exception
                    bIsXML = False
                End Try
            End If

            If bIsXML Then
                strData = Web.HttpUtility.UrlEncode(strData)
            Else
                ' Remove all quotes from within a field value
                strData = strData.Replace("""", "")
                If (strData.IndexOfAny(New Char() {";"c, ","c}) > -1) Then
                    Return """" & strData & """"
                End If
            End If
            Return strData
        End If

        If (TypeOf data Is Boolean) Then Return data.ToString()

        Return cStringUtils.FormatNumber(data)

    End Function

    Private Function DataType(ByVal OLEDataType As Integer) As Type

        Select Case OLEDataType
            Case 2 : Return GetType(Integer)
            Case 3 : Return GetType(Long)
            Case 4 : Return GetType(Single)
            Case 5 : Return GetType(Double)
            Case 11 : Return GetType(Boolean)
            Case 17 : Return GetType(Byte)
            Case 131 : Return GetType(Decimal)
            Case Else : Return GetType(String)
        End Select
        Return Nothing
    End Function

    Private Function SaveTable(ByVal db As cEwEDatabase, ByVal strTable As String, ByVal doc As XmlDocument) As Boolean

        ' Skip system tables and bogus tables
        If (strTable.IndexOf("MSy") = 0) Then Return False
        If (strTable.IndexOfAny(New Char() {"_"c, " "c, "-"c, "."c}) > -1) Then Return False

        Dim astrColDefs As String() = Me.Columns(db, strTable)
        Dim astrCols(astrColDefs.Length - 1) As String

        ' Skip table if nothing to write
        If (astrColDefs.Length = 0) Then Return True

        Dim sb As New StringBuilder()
        Dim row As IDataReader = db.GetReader("SELECT * FROM [" & strTable & "]")
        Dim xn As XmlNode = doc.CreateElement("Table")
        Dim xa As XmlAttribute = Nothing
        Dim iNum As Integer = 0

        ' - Name
        xa = doc.CreateAttribute("Name")
        xa.InnerText = strTable
        xn.Attributes.Append(xa)

        ' - Columns
        For i As Integer = 0 To astrColDefs.Length - 1
            Dim astrColDef As String() = astrColDefs(i).Split(":"c)
            astrCols(i) = astrColDef(0)
            If (i > 0) Then sb.Append(",")
            sb.Append(astrColDefs(i))
        Next
        xa = doc.CreateAttribute("Columns")
        xa.InnerText = sb.ToString
        xn.Attributes.Append(xa)

        While row.Read
            Dim xnRow As XmlNode = doc.CreateElement("Row")
            sb.Length = 0
            For i As Integer = 0 To astrCols.Length - 1
                If (i > 0) Then sb.Append(",")
                Dim strValue As String = Me.ToFieldValue(row, astrCols(i))
                sb.Append(strValue)
                iNum += 1
            Next
            xnRow.InnerText = sb.ToString()
            xn.AppendChild(xnRow)

        End While

        ' Num rows
        xa = doc.CreateAttribute("Num")
        xa.InnerText = CStr(iNum)
        xn.Attributes.Append(xa)

        doc.DocumentElement.AppendChild(xn)

        Return True

    End Function

#End Region ' Save from database

#Region " Diagnostics "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States if the datasource has unsaved changes for Ecopath.
    ''' </summary>
    ''' <returns>True if the datasource has pending changes for Ecopath.</returns>
    ''' -------------------------------------------------------------------
    Public Function IsEcopathModified() As Boolean Implements DataSources.IEcopathDataSource.IsEcopathModified

        Return False

    End Function

#End Region ' Diagnostics

#Region " Modifications not allowed by this type of DS "

#Region " Groups "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Create a record for a new Ecopath group in the datasource.
    ''' </summary>
    ''' <param name="strGroupName">The name of the group to create.</param>
    ''' <param name="sPP">The Type of the new group; 0=consumer, 1=producer, 2=detritus.</param>
    ''' <param name="iPosition">The position of the new group in the group sequence.</param>
    ''' <param name="iDBID">Database ID assigned to the new Group.</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' Note that this will not adjust the data arrays. Due to the complex organization of the
    ''' core a full data reload is required after a group is created.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Function AddGroup(ByVal strGroupName As String, ByVal sPP As Single, ByVal sVBK As Single,
                      ByVal iPosition As Integer, ByRef iDBID As Integer) As Boolean _
            Implements IEcopathDataSource.AddGroup
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Remove a group from the datasource.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the group to remove.</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' Note that this will not adjust the data arrays. Due to the complex organization of the
    ''' core a full data reload is required after a group is removed.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Function RemoveGroup(ByVal iDBID As Integer) As Boolean _
            Implements IEcopathDataSource.RemoveGroup
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Move an Ecopath group to a different position in the group sequence.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the group to move.</param>
    ''' <param name="iPosition">The new position of the group in the group sequence.</param>
    ''' <returns>Always false.</returns>
    ''' <remarks>
    ''' For now, this method is not supported since all data arrays need to be adjusted
    ''' and there is no real need to implement this for EII datasources.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Function MoveGroup(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean _
             Implements IEcopathDataSource.MoveGroup
        Return False
    End Function

#End Region ' Groups

#Region " Fleets "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Adds a fleet to the datasource.
    ''' </summary>
    ''' <param name="strFleetName">Name of the new fleet.</param>
    ''' <param name="iDBID">Database ID assigned to the new fleet.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function AddFleet(ByVal strFleetName As String, ByVal iPosition As Integer, ByRef iDBID As Integer) As Boolean _
            Implements DataSources.IEcopathDataSource.AddFleet
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Removes a fleet from the datasource.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the fleet to remove.</param>
    ''' <returns>Always false.</returns>
    ''' <remarks>This action is not supported in EwE6.</remarks>
    ''' -------------------------------------------------------------------
    Function RemoveFleet(ByVal iDBID As Integer) As Boolean _
            Implements IEcopathDataSource.RemoveFleet
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Move an Ecopath fleet to a different position in the fleet sequence.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the fleet to move.</param>
    ''' <param name="iPosition">The new position of the fleet in the fleet sequence.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function MoveFleet(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean _
            Implements DataSources.IEcopathDataSource.MoveFleet
        Return False
    End Function

#End Region ' Fleets

#Region " Stanza "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Adds a stanza group to the EII.
    ''' </summary>
    ''' <returns>Always false; mutli-stanza logis is not supported in the EII data format.</returns>
    ''' -------------------------------------------------------------------
    Friend Function AppendStanza(ByVal strStanzaName As String, ByVal aiGroupID() As Integer, ByVal aiStartAge() As Integer, ByRef iDBID As Integer) As Boolean _
            Implements IEcopathDataSource.AppendStanza
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Removes a stanza group from the EII.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the stanza group to remove.</param>
    ''' <returns>Always false; mutli-stanza logis is not supported in the EII data format.</returns>
    ''' -------------------------------------------------------------------
    Function RemoveStanza(ByVal iDBID As Integer) As Boolean _
            Implements IEcopathDataSource.RemoveStanza
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Adds a life stage to an existing stanza configuration.
    ''' </summary>
    ''' <param name="iStanzaDBID">Database ID of the stanza group to add the life stage to.</param>
    ''' <param name="iGroupDBID">Group to add as a life stage.</param>
    ''' <param name="iStartAge">Start age of this life stage.</param>
    ''' <returns>Always false; mutli-stanza logis is not supported in the EII data format.</returns>
    ''' -------------------------------------------------------------------
    Public Function AddStanzaLifestage(ByVal iStanzaDBID As Integer, ByVal iGroupDBID As Integer,
                                       ByVal iStartAge As Integer) As Boolean _
            Implements DataSources.IEcopathDataSource.AddStanzaLifestage
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Removes a life stage from an existing stanza configuration.
    ''' </summary>
    ''' <param name="iStanzaDBID">Database ID of the stanza group to remove the life stage from.</param>
    ''' <param name="iGroupDBID">Group to remove as the life stage.</param>
    ''' <returns>Always false; mutli-stanza logis is not supported in the EII data format.</returns>
    ''' -------------------------------------------------------------------
    Public Function RemoveStanzaLifestage(ByVal iStanzaDBID As Integer, ByVal iGroupDBID As Integer) As Boolean _
            Implements DataSources.IEcopathDataSource.RemoveStanzaLifestage
        Return False
    End Function

#End Region ' Stanza

#Region " Pedigree "

    Public Function AddPedigreeLevel(iPosition As Integer, strName As String, iColor As Integer, strDescription As String, varName As eVarNameFlags, sIndexValue As Single, sConfidence As Single, ByRef iDBID As Integer) As Boolean _
     Implements DataSources.IEcopathDataSource.AddPedigreeLevel
        Return False
    End Function

    Public Function MovePedigreeLevel(iDBID As Integer, iPosition As Integer) As Boolean Implements DataSources.IEcopathDataSource.MovePedigreeLevel
        Return False
    End Function

    Public Function RemovePedigreeLevel(iDBID As Integer) As Boolean Implements DataSources.IEcopathDataSource.RemovePedigreeLevel
        Return False
    End Function

#End Region ' Pedigree

#Region " Taxon "

    Public Function AddTaxon(iTargetDBID As Integer, bIsStanza As Boolean, data As ITaxonSearchData, sProportion As Single, ByRef iDBID As Integer) As Boolean _
        Implements DataSources.IEcopathDataSource.AddTaxon
        Return False
    End Function

    Public Function RemoveTaxon(iTaxonID As Integer) As Boolean _
        Implements DataSources.IEcopathDataSource.RemoveTaxon
        Return False
    End Function

#End Region ' Taxon

#Region " Ecosim bits "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States if the datasource has unsaved changes for Ecosim.
    ''' </summary>
    ''' <returns>True if the datasource has pending changes for Ecosim.</returns>
    ''' -------------------------------------------------------------------
    Public Function IsEcosimModified() As Boolean Implements DataSources.IEcosimDatasource.IsEcosimModified
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Updates an ecosim scenario in the EII.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the scenario to update.</param>
    ''' <returns>Always false.</returns>
    ''' <remarks>This action is not supported in EwE6.</remarks>
    ''' -------------------------------------------------------------------
    Friend Function SaveEcosimScenario(ByVal iDBID As Integer) As Boolean _
            Implements IEcosimDatasource.SaveEcosimScenario
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Adds an ecosim scenario to the EII.
    ''' </summary>
    ''' <param name="strName">Name to assign to new scenario.</param>
    ''' <param name="strDescription">Description to assign to new scenario.</param>
    ''' <param name="strAuthor">Author to assign to the new scenario.</param>
    ''' <param name="strContact">Contact info to assign to the new scenario.</param>
    ''' <param name="iDBID">Database ID assigned to the new scenario.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Friend Function AppendEcosimScenario(ByVal strName As String, ByVal strDescription As String, ByVal strAuthor As String, ByVal strContact As String, ByRef iDBID As Integer) As Boolean _
            Implements IEcosimDatasource.AppendEcosimScenario
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Removes an ecosim scenario from the EII.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the scenario to remove.</param>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Friend Function RemoveEcosimScenario(ByVal iDBID As Integer) As Boolean _
            Implements IEcosimDatasource.RemoveEcosimScenario
        Return False
    End Function

    Public Function SaveEcosimScenarioAs(ByVal strScenarioName As String, ByVal strDescription As String,
     ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean _
            Implements IEcosimDatasource.SaveEcosimScenarioAs
        Return False
    End Function

#Region " Forcing Shapes "


    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Appends a forcing shape to the EII.
    ''' </summary>
    ''' <param name="strShapeName">Name to assign to new shape.</param>
    ''' <param name="shapeType"><see cref="eDataTypes">Type of the shape</see> to add.</param>
    ''' <param name="iDBID">Database ID assigned to the new shape.</param>
    ''' <param name="asData">Shape point data.</param>
    ''' <param name="functionType">Primitive function type shape was created from.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Function AppendShape(ByVal strShapeName As String,
                         ByVal shapeType As eDataTypes,
                         ByRef iDBID As Integer,
                         ByVal asData As Single(),
                         ByVal functionType As Long,
                         ByVal points As Single()) As Boolean _
            Implements IEcosimDatasource.AppendShape

        Dim bSuccess As Boolean
        'increment the number of forcing shapes and pass that into EcoSimDatastructure it will resize to the new number of shapes
        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData

        'a proper datasource will 
        'add a record to all tables that need it 
        'compute the new number of shapes and use that to resize the Ecosim Data
        'populate the Ecosim Data in memory with the values from the datasource
        'return the new Ecosim Index and Database ID

        If shapeType <> eDataTypes.Mediation Then
            Dim tmpNumberOfShapes As Integer = ecosimDS.NumForcingShapes + 1

            'add the shape to the underlying EcoSim data
            bSuccess = ecosimDS.ResizeForcingShapes(tmpNumberOfShapes, tmpNumberOfShapes)

            'fake DB id's
            For i As Integer = 1 To ecosimDS.NumForcingShapes
                ecosimDS.ForcingDBIDs(i) = i
            Next

            ''Fake a database ID because there are no database ID in the EII files
            ''this will allow for testing of database ID
            'newDBID = ecosimDS.ForcingEggProdDBIDs(newEcoSimIndex)
        End If
        Return bSuccess

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Deletes a forcing shape from the EII.
    ''' </summary>
    ''' <param name="iDBID">Database ID of the shape to remove.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Function RemoveShape(ByVal iDBID As Integer) As Boolean _
             Implements IEcosimDatasource.RemoveShape

        Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData

        Debug.Assert(ecosimDS.NumForcingShapes - 1 > 0, "No more shapes to remove")
        'jb this is just for testing 
        ecosimDS.ResizeForcingShapes(ecosimDS.NumForcingShapes - 1)

        'hack to fake database IDs
        For i As Integer = 1 To ecosimDS.NumForcingShapes
            ecosimDS.ForcingDBIDs(i) = i
        Next

        Return True
    End Function

#End Region ' Forcing Functions

#Region " Time series "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Adds a time series to the datasource.
    ''' </summary>
    ''' <param name="strName">Name of the new Time Series to add.</param>
    ''' <param name="timeSeriesType"><see cref="eTimeSeriesType">Type</see> of the time series.</param>
    ''' <param name="asValues">Initial values to set in the TS.</param>
    ''' <param name="iDBID">Database ID assigned to the new TS.</param>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Public Function AppendTimeSeries(ByVal strName As String, ByVal iPool As Integer, ByVal iPoolSec As Integer,
                                     ByVal timeSeriesType As eTimeSeriesType, ByVal sWeight As Single,
                                     ByVal asValues() As Single, ByRef iDBID As Integer) As Boolean _
            Implements DataSources.IEcosimDatasource.AppendTimeSeries
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Removes a time series from the datasource.
    ''' </summary>
    ''' <param name="iTimeSeriesID">Database ID of the time series to remove.</param>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Friend Function RemoveTimeSeries(ByVal iTimeSeriesID As Integer) As Boolean _
            Implements DataSources.IEcosimDatasource.RemoveTimeSeries
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <inheritdocs cref="IEcosimDatasource.AppendTimeSeriesDataset"/>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Public Function AppendTimeSeriesDataset(ByVal strDatasetName As String, ByVal strDescription As String,
                                            ByVal strAuthor As String, ByVal strContact As String,
                                            ByVal iFirstYear As Integer, ByVal iNumPoints As Integer, ByVal interval As eTSDataSetInterval,
                                            ByRef iDatasetID As Integer) As Boolean _
        Implements DataSources.IEcosimDatasource.AppendTimeSeriesDataset
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Import a complete <see cref="cTimeSeriesImport">cTimeSeriesImport</see>
    ''' instance into the datasource.
    ''' </summary>
    ''' <param name="ts">The time series data to import.</param>
    ''' <param name="iDataset">Index of the dataset to add time series to.</param>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Public Function ImportTimeSeries(ByVal ts As cTimeSeriesImport, ByVal iDataset As Integer) As Boolean Implements DataSources.IEcosimDatasource.ImportTimeSeries
        Return False
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Removes all time series belonging to a specific dataset from the datasource.
    ''' </summary>
    ''' <param name="iDataset">Index of the dataset to remove.</param>
    ''' <returns>Always false.</returns>
    ''' -------------------------------------------------------------------
    Public Function RemoveTimeSeriesDataset(ByVal iDataset As Integer) As Boolean _
            Implements DataSources.IEcosimDatasource.RemoveTimeSeriesDataset
        Return False
    End Function

#End Region ' Time series

#End Region ' Ecosim bits

#Region " Ecospace bits "

    Public Function AddEcospaceDriverLayer(strName As String, strDescription As String, strUnits As String, ByRef iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.AddEcospaceDriverLayer
        Return False
    End Function

    Public Function AddEcospaceHabitat(strHabitatName As String, ByRef iHabitatID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.AddEcospaceHabitat
        Return False
    End Function

    Public Function AppendEcospaceImportanceLayer(strName As String, strDescription As String, sWeight As Single, ByRef iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.AppendEcospaceImportanceLayer
        Return False
    End Function

    Public Function AppendEcospaceMPA(strScenarioName As String, bMPAMonths() As Boolean, ByRef iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.AppendEcospaceMPA
        Return False
    End Function

    Public Function AppendEcospaceScenario(strScenarioName As String, strDescription As String, strAuthor As String, strContact As String, InRow As Integer, InCol As Integer, sOriginLat As Single, sOriginLon As Single, sCellLength As Single, ByRef iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.AppendEcospaceScenario
        Return False
    End Function

    Public Overloads Function CopyTo(ds As DataSources.IEcospaceDatasource) As Boolean Implements DataSources.IEcospaceDatasource.CopyTo
        Return False
    End Function

    Public Function IsEcospaceModified() As Boolean Implements DataSources.IEcospaceDatasource.IsEcospaceModified
        Return False
    End Function

    Public Function RemoveEcospaceDriverLayer(iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.RemoveEcospaceDriverLayer
        Return False
    End Function

    Public Function MoveEcospaceDriverLayer(iDBID As Integer, iPosition As Integer) As Boolean Implements DataSources.IEcospaceDatasource.MoveEcospaceDriverLayer
        Return False
    End Function

    Public Function RemoveEcospaceHabitat(iHabitatID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.RemoveEcospaceHabitat
        Return False
    End Function

    Public Function MoveEcospaceHabitat(iHabitatID As Integer, iPosition As Integer) As Boolean Implements IEcospaceDatasource.MoveHabitat
        Return False
    End Function

    Public Function RemoveEcospaceImportanceLayer(iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.RemoveEcospaceImportanceLayer
        Return False
    End Function

    Public Function RemoveEcospaceMPA(iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.RemoveEcospaceMPA
        Return False
    End Function

    Public Function RemoveEcospaceScenario(iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.RemoveEcospaceScenario
        Return False
    End Function

    Public Function ResizeEcospaceBasemap(InRow As Integer, InCol As Integer) As Boolean Implements DataSources.IEcospaceDatasource.ResizeEcospaceBasemap
        Return False
    End Function

    Public Function SaveEcospaceScenario(iDBID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.SaveEcospaceScenario
        Return False
    End Function

    Public Function SaveEcospaceScenarioAs(strScenarioName As String, strDescription As String, strAuthor As String, strContact As String, ByRef iScenarioID As Integer) As Boolean Implements DataSources.IEcospaceDatasource.SaveEcospaceScenarioAs
        Return False
    End Function

#End Region ' Ecospace bits

#End Region ' Modifications not allowed by this type of DS

#Region " Interface Implementations "

    Public Function Compact(ByVal strTarget As String) As eDatasourceAccessType _
        Implements DataSources.IEwEDataSource.Compact
        Return eDatasourceAccessType.Failed_OSUnsupported
    End Function

    Public Function CanCompact(ByVal strTarget As String) As Boolean _
    Implements IEwEDataSource.CanCompact
        Return False
    End Function

    Public Function IsOSSupported(ByVal dst As EwEUtils.Core.eDataSourceTypes) As Boolean _
        Implements IEwEDataSource.IsOSSupported
        Return True ' We can do this!
    End Function

    Public Function Directory() As String Implements DataSources.IEwEDataSource.Directory
        Return Path.GetDirectoryName(Me.m_strFilename)
    End Function

    Public Function Extension() As String Implements DataSources.IEwEDataSource.Extension
        Return Path.GetExtension(Me.m_strFilename)
    End Function

    Public Function FileName() As String Implements DataSources.IEwEDataSource.FileName
        Return Path.GetFileName(Me.m_strFilename)
    End Function

    Public Function IsReadOnly() As Boolean Implements DataSources.IEwEDataSource.IsReadOnly
        Return True
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        GC.SuppressFinalize(Me)
    End Sub

#End Region ' Interface Implementations

#Region " Helper methods "

    Private Function GetNextValue(ByVal data() As String, ByRef iNextIndex As Integer) As String
        Dim strData As String = ""
        Do While String.IsNullOrWhiteSpace(strData)
            strData = data(iNextIndex)
            iNextIndex += 1
        Loop
        Return strData
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States if there is catch for at least one group.
    ''' </summary>
    ''' <returns>True if catch was found.</returns>
    ''' -------------------------------------------------------------------
    Private Function IsFishing() As Boolean
        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim bIsFishing As Boolean = False
        Dim iGroup As Integer = 1

        While iGroup < ecopathDS.NumGroups And Not bIsFishing
            bIsFishing = (ecopathDS.fCatch(iGroup) > 0.0)
            iGroup += 1
        End While

        Return bIsFishing
    End Function

    Private Function ReadTable(strTable As String) As DataTable

        Dim xn As XmlNode = Me.m_doc.SelectSingleNode("/EwEModel/Table[translate(@Name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '" + strTable.ToLower() + "']")
        Dim xnRow As XmlNode = Nothing
        Dim xaCols As XmlAttribute = xn.Attributes("Columns")
        Dim strRow As String = ""
        Dim astrColDefs As String() = cStringUtils.SplitQualified(xaCols.InnerText, ","c)
        Dim astrCols(astrColDefs.Length - 1) As String
        Dim atCols(astrColDefs.Length - 1) As Type
        Dim dt As New DataTable(xn.Name)

        For i As Integer = 0 To astrColDefs.Length - 1
            Dim astrColDef As String() = cStringUtils.SplitQualified(astrColDefs(i), ":"c)
            astrCols(i) = astrColDef(0)
            atCols(i) = Type.GetType(astrColDef(1))
            dt.Columns.Add(astrCols(i), atCols(i))
        Next i

        For Each xnRow In xn.ChildNodes
            strRow = xnRow.InnerText
            If Not String.IsNullOrWhiteSpace(strRow) Then
                Dim drow As DataRow = dt.NewRow()
                Dim astrData As String() = cStringUtils.SplitQualified(strRow, ",")
                For i As Integer = 0 To astrData.Length - 1
                    If Not String.IsNullOrWhiteSpace(astrData(i)) Or (atCols(i) Is GetType(String)) Then
                        Try
                            If (atCols(i)) Is GetType(String) Then
                                drow(astrCols(i)) = astrData(i)
                            ElseIf (atCols(i)) Is GetType(Boolean) Then
                                drow(astrCols(i)) = Convert.ToBoolean(astrData(i))
                            Else
                                drow(astrCols(i)) = cStringUtils.ConvertToNumber(astrData(i), atCols(i))
                            End If
                        Catch ex As Exception
                            Debug.Assert(False, "Exception loaded table " & strTable & ": " & ex.Message)
                        End Try
                    End If
                Next
                dt.Rows.Add(drow)
            End If
        Next

        Return dt

        Return Nothing

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, reads data from a column that may not exist. In that case,
    ''' an optional default value is returned
    ''' </summary>
    ''' <param name="row">The <see cref="DataRow"/> to read from.</param>
    ''' <param name="strField">The name of the DB field (column) to read.</param>
    ''' <param name="objValueDefault">A default value to return if the field could not be read.</param>
    ''' <param name="objValueIgnore">Value to interpret as 'no value. When encountered, the default value will be returned.</param>
    ''' <returns>The value of the requested column, or the provided default if an error occurred.</returns>
    ''' -----------------------------------------------------------------------
    Public Function ReadSafe(ByVal row As DataRow,
                             ByVal strField As String,
                             Optional ByVal objValueDefault As Object = Nothing,
                             Optional ByVal objValueIgnore As Object = CSng(-9999)) As Object

        Dim objResult As Object = Nothing

        If (row Is Nothing) Then Return objValueDefault

        Try
            If row.Table.Columns.Contains(strField) Then
                objResult = row(strField)
            End If
        Catch ex As IndexOutOfRangeException
            ' Ugh
        Catch ex As InvalidOperationException
            'Console.WriteLine("DB: field '{0}' has no value, returning provided default '{1}'", strField, objValueDefault)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Console.WriteLine("DB: Exception {2} occurred while accessing field '{0}', returning provided default '{1}'", strField, objValueDefault, ex.ToString)
        End Try

        If (objResult Is Nothing) Then
            objResult = objValueDefault
        ElseIf (objValueIgnore IsNot Nothing) _
            And Not (Convert.IsDBNull(objResult)) _
            And Not (Convert.IsDBNull(objValueIgnore)) Then

            ' Compare ignore values
            If TypeOf objResult Is String Then
                Try
                    If (String.Compare(CStr(objResult), Convert.ToString(objValueIgnore), True) = 0) Then
                        objResult = objValueDefault
                    End If
                Catch ex As Exception
                End Try
            ElseIf TypeOf objResult Is Boolean Then
                Try
                    If (CBool(objResult) = Convert.ToBoolean(objValueIgnore)) Then
                        objResult = objValueDefault
                    End If
                Catch ex As Exception
                End Try
            Else
                Try
                    If (CSng(objResult) = Convert.ToSingle(objValueIgnore)) Then
                        objResult = objValueDefault
                    End If
                Catch ex As Exception
                End Try
            End If

        End If

        If (Convert.IsDBNull(objResult)) Then
            objResult = objValueDefault
        End If

        Return objResult
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Logs a message to the application log.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub LogMessage(ByVal strMessage As String, _
            Optional ByVal msgType As eMessageType = eMessageType.DataModified, _
            Optional ByVal msgImportance As eMessageImportance = eMessageImportance.Information)

        If (Me.m_core IsNot Nothing) Then
            Me.m_core.m_publisher.AddMessage(New cMessage(strMessage, msgType, eCoreComponentType.DataSource, msgImportance))
        End If
        'Console.WriteLine(strMessage)

    End Sub

    Private Function ParseBoolean(strVal As String) As Boolean
        If String.IsNullOrWhiteSpace(strVal) Then Return False
        If strVal = "1" Then Return True
        If strVal = "0" Then Return False
        Return Boolean.Parse(strVal)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <para>Helper method, splits a string of numbers into an array of strings,
    ''' each string representing a number. This method assumes that numbers are
    ''' separated by a ASCII character 32, a single space.</para>
    ''' </summary>
    ''' <param name="strNumberString">A comma-seoarated string of numbers to split.</param>
    ''' <returns>
    ''' An array of strings, each representing a number in the string.
    ''' </returns>
    ''' <remarks>
    ''' <para>This method tries to resolve number formatting issues, introduced
    ''' in models written by systems with different locale settings.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Function SplitNumberString(ByRef strNumberString As String) As String()
        Dim charSeparators() As Char = {" "c}
        If strNumberString.IndexOf(CChar(",")) > -1 Then strNumberString = strNumberString.Replace(CChar(","), CChar("."))
        Return strNumberString.Trim().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries)
    End Function

#End Region ' Helper methods

End Class

