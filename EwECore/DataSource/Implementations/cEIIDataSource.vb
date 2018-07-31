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
'
#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Data access for an EwE5 .EII file
''' </summary>
''' ===========================================================================
Public Class cEIIDataSource
    Implements IEwEDataSource
    Implements IEcopathDataSource

    Private m_strFilename As String = ""
    Private m_core As cCore = Nothing

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

        If (Not String.IsNullOrEmpty(Me.m_strFilename)) Then Return eDatasourceAccessType.Failed_UnknownType
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

#Region " Generic datasource "

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

#End Region

#Region " Ecopath "

#Region " Load "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Initiates a full load of an ecopath model.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function LoadModel() As Boolean _
        Implements IEcopathDataSource.LoadModel

        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim i As Integer

        If Me.LoadEcopath() Then

            For i = 1 To ecopathDS.NumGroups
                If ecopathDS.QB(i) = 0 And ecopathDS.PP(i) = 1 Then ecopathDS.GS(i) = 0
                If ecopathDS.PP(i) = 2 Then ecopathDS.GS(i) = 0
            Next i

            ecopathDS.GS(ecopathDS.NumGroups) = 0

            For i = 1 To ecopathDS.NumGroups
                If ecopathDS.Area(i) <= 0 Or ecopathDS.Area(i) > 1 Then ecopathDS.Area(i) = 1
                If ecopathDS.BH(i) <= 0 And ecopathDS.B(i) > 0 Then ecopathDS.BH(i) = ecopathDS.B(i) / ecopathDS.Area(i)
            Next i

            ecopathDS.bInitialized = True
            ecopathDS.NumEcosimScenarios = 0
            ecopathDS.NumEcospaceScenarios = 0
            ecopathDS.NumEcotracerScenarios = 0

            Me.LoadStanza()

            ' Make sure that the core knows not to expect anything else
            ecopathDS.RedimEcospaceScenarios()
            ecopathDS.RedimEcotracerScenarios()

            ' Invoke plugin point
            If (Me.m_core.PluginManager IsNot Nothing) Then Me.m_core.PluginManager.LoadModel(Me)

            Return True

        End If

        Return False

    End Function

    Private Function LoadEcopath() As Boolean

        'read the contents of the eii file into an EcopathParameters object
        'this is written using vb file access instead of a filestream to keep it as close to the original vb code as possible
        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData
        Dim pvar As Single
        Dim i As Integer
        Dim j As Integer
        Dim K As Integer

        Dim quotes() As Char = {CChar(""""), CChar(" ")}
        Dim eiiStrm As System.IO.StreamReader

        If Not File.Exists(m_strFilename) Then
            cLog.Write(Me.ToString + ".LoadEcopath(...) No file name specified.")
            Return False
        End If

        Try
            eiiStrm = New System.IO.StreamReader(m_strFilename)
        Catch ex As Exception
            cLog.Write(Me.ToString + ".LoadEcopath(...) Error opening eii file. '" & Me.m_strFilename & "' Error:" + ex.Message())
            Return False
        End Try

        'fake model data
        ecopathDS.ModelDBID = 1
        ecopathDS.ModelName = Path.GetFileName(m_strFilename)
        ecopathDS.ModelNumDigits = 3
        ecopathDS.ModelDescription = "Model read from EII file " & Me.m_strFilename

        'read the file
        Try
            Dim buff As String
            Dim recs() As String
            buff = eiiStrm.ReadLine()
            recs = buff.Split(CChar(","))

            Integer.TryParse(recs(0), ecopathDS.NumGroups)
            Integer.TryParse(recs(1), ecopathDS.NumLiving)
            ecopathDS.ModelUnitCurrencyCustom = recs(2)
            Integer.TryParse(recs(3), ecopathDS.ModelUnitCurrency)

            If Not ecopathDS.redimGroupVariables() Or Not psdDS.redimGroupVariables() Then
                cLog.Write(Me.ToString + ".LoadModel(...) Failed to Re-Dimension group parameter arrays.")
                Return False
            End If
            Dim iNextIndex As Integer

            ' Read groups
            For K = 1 To ecopathDS.NumGroups

                ' Replace double spaces with single space
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                iNextIndex = 0

                'Debug.Assert(data.Length = 10, "EII DataSource wrong number of recs in group section.")
                ecopathDS.GroupName(K) = Me.GetNextValue(recs, iNextIndex).Trim(quotes)
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), pvar)
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.DtImp(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.Ex(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.fCatch(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.DC(K, 0))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.Binput(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.PBinput(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.EEinput(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.GEinput(K))
                Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.QBinput(K))

                ecopathDS.BHinput(K) = ecopathDS.Binput(K) / ecopathDS.Area(K)
                ecopathDS.GroupDBID(K) = K
                ecopathDS.PP(K) = pvar - 2

                If K > ecopathDS.NumLiving Then ecopathDS.PP(K) = 2
                If ecopathDS.GE(K) = 0 Then ecopathDS.GE(K) = cCore.NULL_VALUE

            Next K


            ' Read DietComp
            ReDim ecopathDS.DietChanged(1, 0)
            For K = 1 To ecopathDS.NumGroups
                ' Replace double spaces with single space
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                iNextIndex = 0
                For j = 1 To ecopathDS.NumGroups

                    Single.TryParse(Me.GetNextValue(recs, iNextIndex), ecopathDS.DCInput(K, j))
                    ' Input(fnum, ecopathDS.DCInput(K, j))
                    If ecopathDS.DCInput(K, j) > 0 Then
                        ecopathDS.DietWasChanged(K, j)
                    End If
                Next j
            Next K

            If eiiStrm.EndOfStream Then Return True
            'If EOF(fnum) Then Return True

            'junk 
            buff = eiiStrm.ReadLine()

            ''jb totp read in original routine using a string will read the entire line
            'Input(fnum, jnk)
            ''jb I have no idea what this is all about 
            'If Import < 0 Then Import = 0

            ''Unassimilated food
            'Data looks like this
            '-91  20  -91  20  -91  20  -91  20  -91  20  -91  20  -91  20  -91  20  -91  0  -92  0 
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
            Dim iRec As Integer = 1
            For j = 1 To ecopathDS.NumGroups

                Single.TryParse(recs(iRec), ecopathDS.GS(j))
                iRec += 2
                ecopathDS.GS(j) = ecopathDS.GS(j)
                If ecopathDS.GS(j) > 1 Then ecopathDS.GS(j) = ecopathDS.GS(j) / 100
            Next j

            'junk
            buff = eiiStrm.ReadLine()
            'Input(fnum, jnk)

            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")

            ''the time unit name
            ecopathDS.TimeUnitName = recs(0)
            If ecopathDS.TimeUnitName.Contains("year") Then
                ecopathDS.ModelUnitTime = eUnitTimeType.Year
            ElseIf ecopathDS.TimeUnitName.Contains("day") Then
                ecopathDS.ModelUnitTime = eUnitTimeType.Day
            End If

            'the ecosystem remarks.
            'junk
            buff = eiiStrm.ReadLine()

            'parms.Bomass accumulation added March 95/VC
            '-91  20  -91  20  -91  20  -91  20  -91  20  -91  20  -91  20  -91  20  -91  0  -92  0 
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
            For i = 1 To ecopathDS.NumGroups
                Single.TryParse(recs(i - 1), ecopathDS.BAInput(i))
            Next i

            ' Diet Fate array added July 1994/VC
            'If EOF(fnum) = False And NumGroups > NumLiving + 1 Then
            'More than 1 detritusbox Any reason for this??
            For i = 1 To ecopathDS.NumGroups
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                For j = ecopathDS.NumLiving + 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(j - ecopathDS.NumLiving - 1), ecopathDS.DF(i, j - ecopathDS.NumLiving))
                    ' Input(fnum, ecopathDS.DF(i, j - ecopathDS.NumLiving))    
                Next j
            Next i

            ' Emigration added Dec 98/VC
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Emigration"), "EII datasource file format may be wrong!")
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
            'Input(fnum, jnk) ' 
            For i = 1 To ecopathDS.NumGroups
                Single.TryParse(recs(i - 1), ecopathDS.Emigration(i))
                ' Input(fnum, ecopathDS.Emigration(i))
            Next i

            'immigration added Dec 98/VC
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Immig"), "EII datasource file format may be wrong!")
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
            For i = 1 To ecopathDS.NumGroups
                Single.TryParse(recs(i - 1), ecopathDS.Immig(i))
                ' Input(fnum, ecopathDS.Immig(i))
            Next i

            'NumGear
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("NumGear"), "EII datasource file format may be wrong!")
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            Integer.TryParse(buff, ecopathDS.NumFleet)
            ecopathDS.RedimFleetVariables(True)

            'Gearnames
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Gearnames"), "EII datasource file format may be wrong!")
            For i = 1 To ecopathDS.NumFleet
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                ecopathDS.FleetName(i) = buff.Trim(quotes) ' Added Dec 98/VC
                '  Input(fnum, ecopathDS.FleetName(i))
                ecopathDS.FleetDBID(i) = i
            Next i

            'cost
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("cost"), "EII datasource file format may be wrong!")
            'Input(fnum, jnk)  
            For i = 1 To ecopathDS.NumFleet
                'First is fixed cost, second is cost per unit effort' Added Dec 98/VC
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                Single.TryParse(recs(0), ecopathDS.CostPct(i, eCostIndex.Fixed))
                Single.TryParse(recs(1), ecopathDS.CostPct(i, eCostIndex.CUPE))
                Single.TryParse(recs(2), ecopathDS.CostPct(i, eCostIndex.Sail))
            Next i

            'landing
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("landing"), "EII datasource file format may be wrong!")
            'Input(fnum, jnk)  
            For i = 1 To ecopathDS.NumFleet
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                For j = 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(j - 1), ecopathDS.Landing(i, j))
                    '  Input(fnum, ecopathDS.Landing(i, j))    ' Landing added Dec 98/VC
                Next j
            Next i

            'discard
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Discard"), "EII datasource file format may be wrong!")
            'Input(fnum, jnk)  
            For i = 1 To ecopathDS.NumFleet
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                For j = 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(j - 1), ecopathDS.Discard(i, j))
                    '  Input(fnum, ecopathDS.Landing(i, j))    ' Landing added Dec 98/VC
                Next j
            Next i

            'discard fate
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("DiscardFate"), "EII datasource file format may be wrong!")
            For i = 1 To ecopathDS.NumFleet
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                For j = 1 To ecopathDS.NumGroups - ecopathDS.NumLiving
                    Single.TryParse(recs(j - 1), ecopathDS.DiscardFate(i, j))
                    ' Input(fnum, ecopathDS.DiscardFate(i, j))   ' Added Dec 98/VC
                Next j
            Next i

            'market
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Market"), "EII datasource file format may be wrong!")
            'Input(fnum, jnk)  
            For i = 1 To ecopathDS.NumFleet
                buff = eiiStrm.ReadLine().Replace("  ", " ")
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
                For j = 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(j - 1), ecopathDS.Market(i, j))
                    '  Input(fnum, ecopathDS.Landing(i, j))    ' Landing added Dec 98/VC
                Next j
            Next i

            'ecopathDS.NoGearData = False

            ''shadow
            'Input(fnum, jnk)
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Shadow"), "EII datasource file format may be wrong!")
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
            For i = 1 To ecopathDS.NumGroups             ' Added Dec 98/VC
                Single.TryParse(recs(i - 1), ecopathDS.Shadow(i))
                '  Input(fnum, ecopathDS.Shadow(i))
            Next i

            ''Habitatarea
            buff = eiiStrm.ReadLine()
            Debug.Assert(buff.Contains("Area&HabitatBiomass(BH)"), "EII datasource file format may be wrong!")
            buff = eiiStrm.ReadLine().Replace("  ", " ")
            recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, " ")
            iRec = 0
            For i = 1 To ecopathDS.NumGroups
                Single.TryParse(recs(iRec), ecopathDS.Area(i))
                iRec += 1
                Single.TryParse(recs(iRec), ecopathDS.BH(i))
                iRec += 1
            Next i

            eiiStrm.Close()
            ecopathDS.RedimPedigree()

        Catch ex As Exception 'catch any error during the reading of the data
            'FileClose(fnum)
            'some kind of a reading error better find out what happend
            cLog.Write(Me.ToString + ".LoadEcopath() Error reading eii file. Error: " + ex.Message())
            Debug.Assert(False)
            Return False
        End Try

        Return True

    End Function

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
    Function AddGroup(ByVal strGroupName As String, ByVal sPP As Single, ByVal sVBK As Single, _
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

#Region " Pedigree "

    Public Function AddPedigreeLevel(ByVal iPosition As Integer, ByVal strName As String, ByVal iColor As Integer, ByVal strDescription As String, ByVal varName As eVarNameFlags, ByVal sIndexValue As Single, ByVal sConfidence As Single, ByRef iDBID As Integer) As Boolean _
     Implements DataSources.IEcopathDataSource.AddPedigreeLevel
        Return False
    End Function

    Public Function MovePedigreeLevel(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean Implements DataSources.IEcopathDataSource.MovePedigreeLevel
        Return False
    End Function

    Public Function RemovePedigreeLevel(ByVal iDBID As Integer) As Boolean Implements DataSources.IEcopathDataSource.RemovePedigreeLevel
        Return False
    End Function

#End Region ' Pedigree

#Region " Taxon "

    Public Function AddTaxon(ByVal iTargetDBID As Integer, ByVal bIsStanza As Boolean, ByVal data As ITaxonSearchData, ByVal sProportion As Single, ByRef iDBID As Integer) As Boolean _
        Implements DataSources.IEcopathDataSource.AddTaxon
        Return False
    End Function

    Public Function RemoveTaxon(ByVal iTaxonID As Integer) As Boolean _
        Implements DataSources.IEcopathDataSource.RemoveTaxon
        Return False
    End Function

#End Region ' Taxon

#End Region ' Ecopath

#Region " Stanza "

    Private Function LoadStanza() As Boolean
        Dim m_stanzaData As cStanzaDatastructures = m_core.m_Stanza

        ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ''HACK WARNING
        ''jb this is totaly bogus 
        ''is is just to get the stanza variables initialized so that I can test the Stanza Groups interface
        ''go with 2 stanza groups 

        ''init the cores stanza data structures
        'm_stanzaData.MaxStanza = 3
        'm_stanzaData.Nsplit = 2
        'm_stanzaData.MaxAgeSplit = 400 '???? 

        'm_stanzaData.redimStanza()

        ''populate the arrays
        'm_stanzaData.Nstanza(1) = 2
        'm_stanzaData.Nstanza(2) = 3

        ''stanza group 1
        ''fish groups 2 and 3
        'm_stanzaData.EcopathCode(1, 1) = 2
        'm_stanzaData.EcopathCode(1, 2) = 3

        ''stanza group 2
        ''fish groups 5,6 and 7
        'm_stanzaData.EcopathCode(2, 1) = 5
        'm_stanzaData.EcopathCode(2, 2) = 6
        'm_stanzaData.EcopathCode(2, 3) = 7
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'fake a database ID for the EII datasource
        For i As Integer = 1 To m_stanzaData.Nsplit
            m_stanzaData.StanzaDBID(i) = 1
        Next
        m_stanzaData.OnPostInitialization()

        Return True

    End Function

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

#End Region ' Helper methods

#Region " Methods replaced for Mono compatibility "

#If 0 Then

    Private Function LoadEII_org() As Boolean
    'The original EI reading used VB IO 
    'this was replaced with System.IO stream classes
        'read the contents of the eii file into an EcopathParameters object
        'this is written using vb file access instead of a filestream to keep it as close to the original vb code as possible
        Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData
        Dim pvar As Single
        Dim i As Integer
        Dim j As Integer
        Dim K As Integer
        Dim Dummy As Single
        Dim jnk As String
        Dim Import As Integer
        Dim fnum As Integer

        fnum = FreeFile()

        If m_strFilename = "" Then
            cLog.Write(Me.ToString + ".LoadEcopath(...) No file name specified.")
            Return False
        End If

        Try
            FileOpen(fnum, m_strFilename, OpenMode.Input)
        Catch ex As Exception
            cLog.Write(Me.ToString + ".LoadEcopath(...) Error opening eii file. " + vbCrLf + m_strFilename + vbCrLf + "Error:" + ex.Message())
            Return False
        End Try

        'fake model data
        ecopathDS.ModelDBID = 1
        ecopathDS.ModelName = Path.GetFileName(m_strFilename)
        ecopathDS.ModelNumDigits = 3
        ecopathDS.ModelDescription = "Simulated model read from EII file " & m_strFilename

        'read the file
        Try
            Input(fnum, ecopathDS.NumGroups)
            Input(fnum, ecopathDS.NumLiving)
            Input(fnum, ecopathDS.ModelUnitCurrencyCustom)
            Input(fnum, ecopathDS.currUnitIndex)

            If Not ecopathDS.redimGroupVariables() Or Not psdDS.redimGroupVariables() Then
                cLog.Write(Me.ToString + ".LoadModel(...) Failed to Re-Dimension group parameter arrays.")
                Return False
            End If

            'groups
            For K = 1 To ecopathDS.NumGroups
                Input(fnum, ecopathDS.GroupName(K)) : Input(fnum, pvar) : Input(fnum, ecopathDS.DtImp(K))
                Input(fnum, ecopathDS.Ex(K)) : Input(fnum, ecopathDS.fCatch(K)) : Input(fnum, ecopathDS.DC(K, 0))
                Input(fnum, ecopathDS.Binput(K)) : Input(fnum, ecopathDS.PBinput(K)) : Input(fnum, ecopathDS.EEinput(K))
                Input(fnum, ecopathDS.GEinput(K)) : Input(fnum, ecopathDS.QBinput(K))

                ecopathDS.BHinput(K) = ecopathDS.Binput(K) / ecopathDS.Area(K)

                ecopathDS.GroupDBID(K) = K

                'Input #fnum, GroupName(K), Pvar, DtImp(K), Ex(K), Catch(K), parms.DC(K, 0), parms.B(K), parms.pb(K), parms.ee(K), parms.ge(K), parms.qb(K)
                'jb this does not make any sence
                'it uses the Primary Porduction as the version number ????
                'If pvar < -1.99 Then
                '    txt = "It is not possible to import your old version of the " _
                '        + "Ecopath data file. " _
                '        + "You may have to reenter your data.  " _
                '        + "Open the eii file in Notepad, and check it. " _
                '        + "A testversion of Ecopath with Ecosim had a bug where it would place, " _
                '        + "e.g., '-94-95' instead of '-94 -95' in the eii file. If this is the case then add spaces where needed. " _
                '        + "If not, please email v.christensen@cgiar.org " + Environment.NewLine  _
                '        + "Please edit data.  Press any key to abort. "

                '    MsgBox(txt, vbCritical + vbOKOnly, "Problem importing old file type")

                '    FileClose(fnum)
                '    ReadEii = False
                '    Exit Function
                'End If

                ecopathDS.PP(K) = pvar - 2
                If K > ecopathDS.NumLiving Then ecopathDS.PP(K) = 2
                If ecopathDS.GE(K) = 0 Then ecopathDS.GE(K) = -9

            Next K

            ' "Read DietComp"
            ReDim ecopathDS.DietChanged(1, 0)
            For K = 1 To ecopathDS.NumGroups
                For j = 1 To ecopathDS.NumGroups
                    Input(fnum, ecopathDS.DCInput(K, j))
                    If ecopathDS.DCInput(K, j) > 0 Then
                        ecopathDS.DietWasChanged(K, j)
                    End If
                Next j
            Next K

            If EOF(fnum) Then Return True

            'jb totp read in original routine using a string will read the entire line
            Input(fnum, jnk)
            'jb I have no idea what this is all about 
            If Import < 0 Then Import = 0

            'Unassimilated food
            For j = 1 To ecopathDS.NumGroups
                Input(fnum, Dummy) : Input(fnum, ecopathDS.GS(j))
                If Dummy < 0 Then Dummy = 0
                ecopathDS.GS(j) = Dummy + ecopathDS.GS(j)
                If ecopathDS.GS(j) > 1 Then ecopathDS.GS(j) = ecopathDS.GS(j) / 100
            Next j

            Input(fnum, jnk)

            'the time unit name
            If EOF(fnum) = False Then
                Dim tmpbuff As String
                Input(fnum, tmpbuff)
                ecopathDS.TimeUnitName = tmpbuff.Trim
                Select Case LCase(ecopathDS.TimeUnitName)
                    Case "year"
                        ecopathDS.ModelUnitTime = eUnitTimeType.Year
                    Case "day"
                        ecopathDS.ModelUnitTime = eUnitTimeType.Day
                    Case Else
                        ecopathDS.ModelUnitTime = eUnitTimeType.Custom
                        ecopathDS.ModelUnitTimeCustom = ecopathDS.TimeUnitName

                End Select
            End If

            'the ecosystem remarks.
            Input(fnum, jnk)

            For i = 1 To ecopathDS.NumGroups             ' parms.Bomass accumulation added March 95/VC
                Input(fnum, ecopathDS.BA(i))
            Next i

            'If EOF(fnum) = False And NumGroups > NumLiving + 1 Then
            'More than 1 detritusbox Any reason for this??
            For i = 1 To ecopathDS.NumGroups
                For j = ecopathDS.NumLiving + 1 To ecopathDS.NumGroups
                    Input(fnum, ecopathDS.DF(i, j - ecopathDS.NumLiving))     ' Diet Fate array added July 1994/VC
                Next j
            Next i

            Input(fnum, jnk) ' 
            For i = 1 To ecopathDS.NumGroups             ' Emigration added Dec 98/VC
                Input(fnum, ecopathDS.Emigration(i))
            Next i

            Input(fnum, jnk)
            For i = 1 To ecopathDS.NumGroups                 ' immigration added Dec 98/VC
                Input(fnum, ecopathDS.Immig(i))
            Next i

            Input(fnum, jnk)  'NumGear
            Input(fnum, ecopathDS.NumFleet)

            ecopathDS.RedimFleetVariables(True)

            Input(fnum, jnk) 'Gearnames
            For i = 1 To ecopathDS.NumFleet             ' Added Dec 98/VC
                Input(fnum, ecopathDS.FleetName(i))
                ecopathDS.FleetDBID(i) = i
            Next i

            Input(fnum, jnk)  'cost
            For i = 1 To ecopathDS.NumFleet
                'First is fixed cost, second is cost per unit effort' Added Dec 98/VC
                Input(fnum, ecopathDS.CostPct(i, eCostIndex.Fixed))
                Input(fnum, ecopathDS.CostPct(i, eCostIndex.CUPE))
                Input(fnum, ecopathDS.CostPct(i, eCostIndex.Sail))
            Next i

            Input(fnum, jnk)  'landing
            For i = 1 To ecopathDS.NumFleet
                For j = 1 To ecopathDS.NumGroups
                    Input(fnum, ecopathDS.Landing(i, j))    ' Landing added Dec 98/VC
                Next j
            Next i

            Input(fnum, jnk)  'discard
            For i = 1 To ecopathDS.NumFleet
                For j = 1 To ecopathDS.NumGroups
                    Input(fnum, ecopathDS.Discard(i, j))    ' Added Dec 98/VC
                Next j
            Next i

            Input(fnum, jnk)  'discard
            For i = 1 To ecopathDS.NumFleet
                For j = 1 To ecopathDS.NumGroups - ecopathDS.NumLiving
                    Input(fnum, ecopathDS.DiscardFate(i, j))   ' Added Dec 98/VC
                Next j
            Next i

            Input(fnum, jnk)  'market
            For i = 1 To ecopathDS.NumFleet
                For j = 1 To ecopathDS.NumGroups
                    Input(fnum, ecopathDS.Market(i, j))    ' Added Dec 98/VC
                Next j
            Next i

            ecopathDS.NoGearData = False

            'shadow
            Input(fnum, jnk)
            For i = 1 To ecopathDS.NumGroups             ' Added Dec 98/VC
                Input(fnum, ecopathDS.Shadow(i))
            Next i

            'Habitatarea
            Input(fnum, jnk)  '
            For i = 1 To ecopathDS.NumGroups             ' Added Dec 98/VC
                Input(fnum, ecopathDS.Area(i))
                Input(fnum, ecopathDS.BH(i))
            Next i

            FileClose(fnum)

            ecopathDS.RedimPedigree()

        Catch ex As Exception 'catch any error during the reading of the data
            FileClose(fnum)
            'some kind of a reading error better find out what happend
            cLog.Write(Me.ToString + ".LoadEcopath() Error reading eii file. Error: " + ex.Message())
            Debug.Assert(False)
            Return False
        End Try

        Return True

    End Function

#End If

#End Region ' Methods replaced for Mono compatibility

End Class

