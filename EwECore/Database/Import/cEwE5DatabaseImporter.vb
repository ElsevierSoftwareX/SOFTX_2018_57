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
Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.Utilities

#End Region ' EwECore

Namespace Database

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Imports an EwE5 database into an EwE6 database
    ''' </summary>
    ''' <example>
    ''' The following example illustrates how to use this class:
    ''' <code>
    ''' Dim dbImp as IEwE5ModelImporter = cEwE5ModelImporterFactory.GetEwE5ModelImporter(Core, strEwE5File)
    ''' Dim info As List(Of IEwE5ModelImporter.cEwE5ModelInfo) = Nothing
    ''' Dim model As IEwE5ModelImporter.cEwE5ModelInfo = Nothing
    ''' Dim nSucces As Integer = 0
    ''' 
    ''' ' Attach to an MS Access database
    ''' If (dbImp.Open("my_ewe5.mdb")) Then
    '''     ' Can Import?
    '''     If (dmImp.CanImport()) Then
    '''         ' Get models
    '''         lModels = dmImp.GetModels()
    '''         ' Import each model
    '''         For i As Integer = 0 To lModels.Count - 1
    '''            ' Get model
    '''            model = lModes(i)
    '''            ' Import the model
    '''            If (dmImp.Import(model.ID, cStringUtils.Localize("EwE6_{0}.mdb", model.Name))) Then
    '''               ' Count Succes
    '''               nSucces += 1
    '''            End If
    '''         Next i
    '''     End If
    '''     ' Clean up
    '''     dbImp.Close()
    ''' End If
    ''' </code>
    ''' </example>
    ''' -----------------------------------------------------------------------
    Public Class cEwE5DatabaseImporter
        Inherits cEwE5ModelImporter

#Region " Private bits "

        ''' <summary>List of imported forcing shapes, used to check for duplicates.</summary>
        Private m_lImportedForcingShapes As New List(Of cForcingShapeData)
        ''' <summary>ID for next imported shape.</summary>
        Private m_iNextShapeID As Integer = 1

        ''' <summary>Dict (varname, List(of PedigreeLevelID))</summary>
        Private m_dicPedigreeLevels As New Dictionary(Of eVarNameFlags, List(Of Integer))
        ''' <summary>ID for next imported Auxillary data.</summary>
        Private m_iNextAuxID As Integer = 1

        ''' <summary>Dict of Ecospace depth profiles per scenario ID.</summary>
        Private m_dicDepthMaps As New Dictionary(Of Integer, Single(,))

        ' == Databases ==

        ''' <summary>Source database in EwE5 format.</summary>
        Private m_dbEwE5 As cEwEDatabase ' Import from (read)

        ' == Tables that will receive information throughout the import process ==

        ''' <summary>Primary keys lookup table</summary>
        Private m_adtKeys() As Dictionary(Of String, Integer)
        ''' <summary>Dictionaries, per datatype, of EwE group index to EwE6 DatabaseID.</summary>
        Private m_adtIndexes() As Dictionary(Of Integer, Integer)

#End Region ' Private bits 

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' <param name="core">The core to import into.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal core As cCore)

            MyBase.New(core)

            Me.m_lImportedForcingShapes.Clear()
            Me.m_dicPedigreeLevels.Clear()
            Me.m_iNextShapeID = 1
            Me.m_iStep = 0

            Me.m_dicPedigreeLevels(eVarNameFlags.Biomass) = New List(Of Integer)
            Me.m_dicPedigreeLevels(eVarNameFlags.PBInput) = New List(Of Integer)
            Me.m_dicPedigreeLevels(eVarNameFlags.QBInput) = New List(Of Integer)
            Me.m_dicPedigreeLevels(eVarNameFlags.DietComp) = New List(Of Integer)
            Me.m_dicPedigreeLevels(eVarNameFlags.TCatchInput) = New List(Of Integer)

        End Sub

#End Region ' Constructor

#Region " Interface implementation "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Open"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function Open(ByVal strSource As String) As Boolean

            ' Pre
            Debug.Assert(Not Me.IsOpen())

            ' Create db
            Dim db As New cEwEAccessDatabase()
            Me.m_strSource = strSource
            If db.Open(Me.m_strSource) = eDatasourceAccessType.Opened Then
                Me.m_dbEwE5 = db
            End If

            Return Me.IsOpen()
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Close"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Close()

            ' Pre
            Debug.Assert(Me.IsOpen())

            Me.m_dbEwE5.Close()
            Me.m_dbEwE5 = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.IsOpen"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function IsOpen() As Boolean

            Return (Me.m_dbEwE5 IsNot Nothing)

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Models"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function Models() As cExternalModelInfo()

            ' Pre
            Debug.Assert(Me.IsOpen())

            Dim l As New List(Of cExternalModelInfo)
            Dim mi As cExternalModelInfo = Nothing
            Dim r As IDataReader = Me.m_dbEwE5.GetReader("SELECT Models.modelName, Models.modelTitle, Models.remarks, (SELECT COUNT(*) FROM [Ecosim] WHERE (Models.modelName = Ecosim.modelName)) as NumScenarios FROM [Models] GROUP BY Models.modelName, Models.modelTitle, Models.remarks")

            If r Is Nothing Then Return Nothing

            While r.Read()
                mi = New cExternalModelInfo(CStr(r(0)), CStr(r(1)), _
                    CStr(Me.FixValue(r, "remarks", My.Resources.CoreMessages.IMPORT_NO_DESCRIPTION)), CInt(r(3)))
                l.Add(mi)
            End While

            ' Sort models
            l.Sort(New cExternalModelInfoSorter())

            Return l.ToArray()

        End Function

        Public Overrides Function CanImportFrom(strSource As String) As Boolean
            Return True
        End Function

#End Region ' Interface implementation

#Region " Implementation "

#Region " Generic "

        Private Function SplitNumberListString(ByVal strMemo As String, Optional ByVal cSplitChar As Char = CChar(" "), _
                Optional ByVal nDefaultNumberLen As Integer = 7) As String()

            Dim astrMemoBits() As String = {""}
            Dim sValue As Single = 0.0!

            If strMemo.Length = 0 Then
                Return astrMemoBits
            End If

            ' Remove irrelevant bits
            strMemo = strMemo.Trim

            ' Check for non-separating comma's
            If (strMemo.IndexOf(CChar(",")) > -1) And (cSplitChar <> CChar(",")) Then
                ' Has no decimal points?
                If strMemo.IndexOf(CChar(".")) = -1 Then
                    ' No decimal points? Assume comma's represent decimal separators and replace 'em all with decimal points
                    strMemo = strMemo.Replace(CChar(","), CChar("."))
                Else
                    ' String contains both comma's and decimal points. Assume that comma's represent thousand separators and remove them
                    strMemo = strMemo.Replace(CChar(","), CChar(""))
                End If
            End If

            ' Is splitter char available?
            If strMemo.IndexOf(cSplitChar) = -1 Then

                ' #Separator character not found. Try to calc length of each number strings by 
                ' examining how far decimal points are spaced apart in the memo string

                ' Find first decimal point location
                Dim iFirst As Integer = strMemo.IndexOf(CChar("."))
                ' Find second decimal point location
                Dim iSecond As Integer = strMemo.IndexOf(CChar("."), iFirst + 1)
                ' Take calculated number string length if two decimal points found. If this fails,
                ' take the default number string length provided as a parameter
                'Dim iNumLen As Integer = CInt(if(iFirst = -1 Or iSecond = -1, nDefaultNumberLen, iSecond - iFirst))
                Dim iNumLen As Integer
                If iFirst = -1 Or iSecond = -1 Then
                    iNumLen = nDefaultNumberLen
                Else
                    iNumLen = iSecond - iFirst
                End If
                ' Calculate the total of number strings in the memo string, rounded up
                Dim iNumBits As Integer = CInt(Math.Ceiling(strMemo.Length / iNumLen))

                ' Allocate space for all number strings
                ReDim astrMemoBits(iNumBits - 1)
                ' Extract 'em all
                For i As Integer = 0 To iNumBits - 1
                    astrMemoBits(i) = strMemo.Substring(i * iNumLen, Math.Min(strMemo.Length - i * iNumLen, iNumLen))
                Next
            Else
                ' #Separator character found: just split the memo string
                astrMemoBits = strMemo.Split(CChar(cSplitChar))
            End If

            ' Now remodel the memo string using real numbers
            For i As Integer = 0 To astrMemoBits.Length - 1
                Try
                    ' Convert number string into a real single
                    ' JS 05Apr10: use US number format settings
                    sValue = cStringUtils.ConvertToSingle(astrMemoBits(i), 0)
                Catch ex As Exception
                    ' Provide default in case of an exception
                    sValue = 0
                End Try
                astrMemoBits(i) = cStringUtils.FormatSingle(sValue)
            Next

            Return astrMemoBits
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, rebuilds a memo string that consists of a series of 
        ''' numbers.
        ''' </summary>
        ''' <param name="strMemo">Memo text to rebuild</param>
        ''' <param name="cSplitChar">Separator character that separates the 
        ''' numbers in the memo </param>
        ''' <param name="nDefaultNumberLen">When interpreting a string without
        ''' separators, this value indicates the number of characters that each
        ''' number occupies in the memo string.</param>
        ''' <param name="nRepetition">Optional field, indicating the number of
        ''' times a value for the source string should be repeated.</param>
        ''' <returns>A smaller string representing the same numbers.</returns>
        ''' -------------------------------------------------------------------
        Private Function RebuildNumberListString(ByVal strMemo As String, _
                Optional ByVal cSplitChar As Char = CChar(" "), _
                Optional ByVal nDefaultNumberLen As Integer = 7, _
                Optional ByVal nRepetition As Integer = 1) As String

            Dim astrMemoBits() As String
            Dim sb As New StringBuilder

            If strMemo.Length = 0 Then
                Return strMemo
            End If

            astrMemoBits = Me.SplitNumberListString(strMemo, cSplitChar, nDefaultNumberLen)

            ' Now remodel the memo string using real numbers
            For i As Integer = 0 To astrMemoBits.Length - 1
                For j As Integer = 1 To nRepetition
                    ' Separate numbers with a single space
                    If sb.Length > 0 Then sb.Append(CChar(" "))
                    ' Add the number
                    sb.Append(astrMemoBits(i))
                Next
            Next
            ' There
            Return sb.ToString()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; converts a string into a pattern of bit flags.
        ''' </summary>
        ''' <param name="strFlags">The string to convert into bit flags.</param>
        ''' <param name="strMatch">The character(s) to match in the string.</param>
        ''' <param name="bMatchAsOne">
        ''' <pata>Flag, indicates how a positive character match affects the bit pattern.</pata>
        ''' <list type="bullet">
        ''' <item><description>
        ''' When set to True, a positive character match generates a 1, and a negative character match generates a 0.
        ''' </description></item>
        ''' <item><description>
        ''' When set to False, a positive character match generates a 0, and a negative character match generates a 1.
        ''' </description></item>
        ''' </list>
        ''' </param>
        ''' <returns>A bit pattern of the provided string.</returns>
        ''' -------------------------------------------------------------------
        Private Function StringToBitFlags(ByVal strFlags As String, ByVal strMatch As String, Optional ByVal bMatchAsOne As Boolean = True) As Integer
            Dim iBitFlags As Integer = 0
            Dim iBit As Integer = 0
            Dim cTest As Char = Nothing

            ' Iterate through the characters in the string, starting at the least precision number (left-most value, highest number)
            ' all the way up to the right of the string, representing the highest precision number.
            For iBit = 0 To strFlags.Length - 1
                ' Shift pattern one bit to the left
                iBitFlags *= 2
                ' Get next bit char to test
                cTest = CChar(strFlags.Substring(iBit, 1))
                ' Is this a character from the match set?
                If (strMatch.IndexOf(cTest) >= 0) Then
                    ' #Yes: Add 1 or 0, depending on bMatchAsOne flag value
                    'CInt(if(bMatchAsOne, 1, 0))
                    If bMatchAsOne Then
                        iBitFlags += 1
                    End If
                Else
                    ' #No: Add 0 or 1, depending on bMatchAsOne flag value
                    ' iBitFlags += CInt(if(bMatchAsOne, 0, 1))
                    If bMatchAsOne Then
                        iBitFlags += 1
                    End If

                End If
            Next
            Return iBitFlags
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, generates a hash key for a data value, optionally 
        ''' in the context of a strong-typed scenario.
        ''' </summary>
        ''' <param name="strKey">The EwE5 value to generate the kay for.</param>
        ''' <param name="iScenarioID">Database ID of scenario this key belongs to, 
        ''' if any. No scenario filter is applied if this value is less than or equals to 0.</param>
        ''' <param name="dtScenario">Data type of this scenario, if any.</param>
        ''' -------------------------------------------------------------------
        Private Function MakeHashKey(ByVal strKey As String, ByVal iScenarioID As Integer, ByVal dtScenario As eDataTypes) As String
            If iScenarioID <= 0 Then
                Return strKey
            Else
                Return cStringUtils.Localize("{0}@{1}({2})", strKey, dtScenario.ToString, iScenarioID)
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set a value in the primary keys hashtable.
        ''' </summary>
        ''' <param name="dt">The <see cref="eDataTypes">data type</see> to access
        ''' the key for.</param>
        ''' <param name="strKey">The EwE5 value to hash</param>
        ''' <param name="iScenarioID">Database ID of scenario this key belongs to, if any.</param>
        ''' <param name="dtScenario">Data type of this scenario, if any.</param>
        ''' <remarks>
        ''' <para>EwE5 identifies objects by name. EwE6 uses database IDs. The 
        ''' <see cref="m_adtKeys">Primary Key hashtable</see> maintains an 
        ''' administration of EwE5 to EwE6 key mappings during the import 
        ''' process.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Property HashKey(ByVal dt As eDataTypes, ByVal strKey As String, _
                Optional ByVal dtScenario As eDataTypes = eDataTypes.NotSet, Optional ByVal iScenarioID As Integer = 0) As Integer
            Get
                ' Get proper dictionary
                Dim dict As Dictionary(Of String, Integer) = m_adtKeys(CInt(dt))
                Dim strKeyInt As String = Me.MakeHashKey(strKey, iScenarioID, dtScenario)

                If (dict Is Nothing) Then
                    'Console.WriteLine("Dictionary not defined, no data imported for datatype {0} ({1})", dt.ToString, strKey)
                    Return 0
                End If

                If Not dict.ContainsKey(strKeyInt) Then
                    'Console.WriteLine("Failed to resolve datatype {0} ({1})", dt.ToString, strKeyInt)
                    Return 0
                End If

                ' Return the item, let this crash if item cannot be found
                Return dict.Item(strKeyInt)
            End Get
            Set(ByVal iValue As Integer)
                ' Get proper dictionary
                Dim dict As Dictionary(Of String, Integer) = m_adtKeys(CInt(dt))
                Dim strKeyInt As String = Me.MakeHashKey(strKey, iScenarioID, dtScenario)
                ' Already allocated?
                If (dict Is Nothing) Then
                    ' #No: create new
                    dict = New Dictionary(Of String, Integer)
                    ' Store dict
                    m_adtKeys(CInt(dt)) = dict
                End If
                ' Store the item, let this crash if the key already exists
                dict(strKeyInt) = iValue
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set a group ID in the primary keys hashtable.
        ''' </summary>
        ''' <param name="dt">The <see cref="eDataTypes">data type</see> to access
        ''' the key for.</param>
        ''' <para>EwE5 identifies objects by name. EwE6 uses database IDs. The 
        ''' <see cref="m_adtKeys">Primary Key hashtable</see> maintains an 
        ''' administration of EwE5 to EwE6 key mappings during the import 
        ''' process.</para>
        ''' -------------------------------------------------------------------
        Private Property MappedID(ByVal dt As eDataTypes, ByVal iEwE5Index As Integer) As Integer
            Get
                ' Get proper dictionary
                Dim dict As Dictionary(Of Integer, Integer) = Me.m_adtIndexes(CInt(dt))

                If (dict Is Nothing) Then Return 0
                If (Not dict.ContainsKey(iEwE5Index)) Then Return 0
                ' Return the item, let this crash if item cannot be found
                Return dict.Item(iEwE5Index)
            End Get

            Set(ByVal iValue As Integer)
                ' Get proper dictionary
                Dim dict As Dictionary(Of Integer, Integer) = Me.m_adtIndexes(CInt(dt))
                ' Already allocated?
                If (dict Is Nothing) Then
                    ' #No: create new
                    dict = New Dictionary(Of Integer, Integer)
                    ' Store dict
                    Me.m_adtIndexes(CInt(dt)) = dict
                End If
                ' Store the item, let this crash if the key already exists
                dict(iValue) = iValue
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, prepares a database-derived value for use in EwE6. 
        ''' The following fixes are performed:
        ''' <list type="bullet">
        ''' <item><description>Strings are trimmed of leading and trailing whitespace.</description></item>
        ''' <item><description>Numeric values are translated from EwE5 Null values to <see cref="cCore.NULL_VALUE">EwE6 NULL</see>
        ''' values if this value is <paramref name="valDefault">allowed to be NULL</paramref>.</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="r">The <see cref="IDataReader">data reader</see> to obtain the value from.</param>
        ''' <param name="strField">Field in the reader to obtain the value from.</param>
        ''' <param name="valDefault">Optional default value to return if the value found in the reader is a DBNull value.</param>
        ''' <returns>The groomed and pruned value.</returns>
        ''' -------------------------------------------------------------------
        Private Function FixValue(ByRef r As IDataReader, ByVal strField As String, _
                Optional ByVal valDefault As Object = Nothing) As Object

            Dim value As Object = Nothing

            Try
                ' Try to get variable from DB
                value = r(strField)
            Catch ex As Exception
                ' Set to DBNull in case of an internal explosion
                value = Convert.DBNull
            End Try

            ' Value unknown?
            If Convert.IsDBNull(value) Then
                ' #Yes: is a default provided?
                If valDefault IsNot Nothing Then
                    ' #Yes: Set default
                    value = valDefault
                End If
                ' Return value
                Return value
            End If

            ' ================================== '
            ' Correct numerical EwE5 NULL values '
            ' ================================== '

            ' Is this a numerical value?
            If TypeOf (value) Is Integer Or TypeOf (value) Is Single Or TypeOf (value) Is Double Then
                ' #Yes: is this value an EWE5 NULL value?
                If (CDbl(valDefault) = cCore.NULL_VALUE) And (CDbl(value) <= cEWE5_NULL) Then
                    ' #Yes: translate to EwE6 NULL values
                    value = CInt(cCore.NULL_VALUE)
                End If
            End If

            ' Is a string value?
            If TypeOf (value) Is String Then
                ' #Yes: strip off white space
                value = DirectCast(value, String).Trim()
            End If

            Return value
        End Function

        Private Function ExtractLastSavedJulianDate(ByVal strDescription As String) As Single

            Dim strDate As String = ""
            Dim iLastSeparatorPos As Integer = -1

            If String.IsNullOrEmpty(strDescription) Then Return 0.0!

            iLastSeparatorPos = strDescription.LastIndexOf(";"c)
            If iLastSeparatorPos > -1 Then
                strDate = strDescription.Substring(iLastSeparatorPos + 1)
            Else
                iLastSeparatorPos = strDescription.IndexOf("Created:")
                If iLastSeparatorPos > -1 Then
                    strDate = strDescription.Substring(iLastSeparatorPos + "Created:".Length)
                Else
                    strDate = strDescription
                End If
            End If

            Try
                Dim dt As Date '= Convert.ToDateTime(strDate, New CultureInfo("en-US"))
                Date.TryParse(strDate, dt)
                Return CSng(dt.ToOADate())
            Catch ex As Exception
                ' Woops!
            End Try
            Return 0.0!

        End Function

#End Region ' Generic

#Region " The import "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Imports and converts a model in an EwE5 database into a provided EwE6 database.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function PerformImport() As Boolean

            ' Allocate primary key lookup tables
            ReDim m_adtKeys(System.Enum.GetValues(GetType(eDataTypes)).Length)
            ' Allocate object indexes lookup tables
            ReDim Me.m_adtIndexes(System.Enum.GetValues(GetType(eDataTypes)).Length)

            ' Pre
            Debug.Assert(Me.m_dbTarget IsNot Nothing, "Needs a valid EwE6 database instance")
            Debug.Assert(Me.m_dbTarget.GetConnection().State = ConnectionState.Open, "EwE6 database must already be open")

            Dim dbUpd As cDatabaseUpdater = Nothing

            ' Set progress info (fixed)
            Me.m_iNumSteps = 29
            Me.m_iStep = 0

            ' Start the actual import process.
            ' Note that VB6 function names are used here to make it easier to map to the old code.

            ' -------
            ' ECOPATH
            ' -------

            Me.LogProgress(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_PROGRESS_MODEL, Me.m_strModelName))
            Me.ImportModels()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_PEDIGREE)
            Me.ImportPedigree()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOPATHGROUPS)
            Me.ImportEcopathGroups()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOPATHGROUPS)
            Me.ImportGroupSize()
            Me.ImportBasicRemarks()

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_STANZA)
            Me.ImportGroupStanza()

            'ImportGroupTaxon Me.m_strModelName (discontinued in EwE5)
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_POGRESS_DIETCOMP)
            Me.ImportGroupxGroup()

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FLEET)
            Me.ImportGear()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH & " 1/3")
            Me.ImportCatch()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH & " 2/3")
            Me.ImportCatchCodes()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH & " 3/3")
            Me.ImportDiscardFate()

            ' Discontinued in EwE6, but throw a warning when EwE5 data exists
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECORANGER)
            Me.ImportEcoranger()
            'ImportEcoRangerN()
            'ImportEcoRangerNxN1()

            ' ------
            ' ECOSIM
            ' ------

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_SCENARIO)
            If Me.ImportEcosim() Then

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSIMGROUPS)
                Me.ImportEcosimN()

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FORCINGMEDIATION)
                Me.ImportEcosimnShapes()
                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FORCINGAPPLICATIONS & " 1/3")
                Me.ImportEcosimNxNInteraction()
                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FORCINGAPPLICATIONS & " 2/3")
                Me.ImportEcosimNxN()
                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FORCINGAPPLICATIONS & " 3/3")
                Me.ImportEcosimMedWeights()

                ' Discontinued in EwE6, but still throw a warning
                Me.ImportEcosimPairs()

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FLEET)
                Me.ImportEcosimFishGear()

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_TIMESERIES)
                Me.ImportTimeSeries()

            End If

            ' --------
            ' ECOSPACE
            ' --------

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSPACESCENARIOS)
            If (Me.ImportEcospaceScenario()) Then

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROPRESS_ECOSPACEHABITATS)
                Me.ImportEcospaceHabitats()

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSPACEGROUPS)
                Me.ImportEcospaceGroups()

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSPACEMPAS)
                Me.ImportEcospaceMPA()

                ' Import basemap after habitat, mpa and regions
                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSPACEBASEMAP)
                Me.ImportEcospaceBasemap()

                ' Import fleets after basemap
                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSPACEFLEETS)
                Me.ImportEcospaceFleets()

            End If

            ' ---------
            ' ECOTRACER
            ' ---------

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOTRACER)
            If (Me.ImportEcotracer()) Then

                Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOTRACERGROUPS)
                Me.ImportEcotracerN()

            End If

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_QUOTES)
            Me.ImportQuotes()

            'ImportFlowBox()
            'ImportFlowConnector()
            'ImportFlowLabel()
            'ImportFlowLineSource()
            'ImportFlowLines()
            'ImportGroupTaxon()
            'ImportOutputParam()
            'ImportPyramidMain()
            'ImportPyramidSource()
            'ImportSummaryStatistics()

            ' Set version
            Me.m_dbTarget.SetVersion(Me.m_dbTarget.GetVersion(), "Imported from Ecopath 5")

            ' Now run all available updates on the new EwE6 database
            dbUpd = New cDatabaseUpdater(Me.m_core, 6.0!)
            dbUpd.UpdateDatabase(Me.m_dbTarget)
            dbUpd = Nothing

            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_COMPLETE, Me.m_iNumSteps)

            Return True
        End Function

#End Region ' The import

#Region " Model "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Import generic model information
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ImportModels()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bWithSeason As Boolean = False
            Dim strYear As String = ""
            Dim dt As DateTime = Nothing
            Dim strUnit As String = ""
            Dim unitCurrency As eUnitCurrencyType = 0
            Dim unitTime As eUnitTimeType = 0

            ' Clear table
            Me.m_dbTarget.Execute("DELETE * FROM EcopathModel")

            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Models] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = m_dbTarget.GetWriter("EcopathModel")

            reader.Read()
            drow = writer.NewRow()
            drow("ModelID") = 1
            drow("Name") = Me.FixValue(reader, "modelTitle")
            drow("Description") = Me.FixValue(reader, "remarks", "")
            drow("NumDigits") = Me.FixValue(reader, "numDigits")

            ' Translate Currency unit
            strUnit = CStr(Me.FixValue(reader, "currencyUnit", ""))
            unitCurrency = DirectCast(CInt(Me.FixValue(reader, "currencyIndex", CInt(eUnitCurrencyType.WetWeight))), eUnitCurrencyType)
            Select Case unitCurrency
                Case eUnitCurrencyType.NotSet
                    unitCurrency = eUnitCurrencyType.WetWeight
                Case eUnitCurrencyType.CustomEnergy, eUnitCurrencyType.CustomNutrient
                Case Else
                    strUnit = ""
            End Select
            drow("UnitCurrency") = CInt(unitCurrency)
            drow("UnitCurrencyCustom") = strUnit

            ' Translate Time unit
            strUnit = CStr(Me.FixValue(reader, "timeUnit", ""))
            unitTime = eUnitTimeType.Custom
            Select Case strUnit.Trim.ToLower()
                Case "year" : unitTime = eUnitTimeType.Year : strUnit = ""
                Case "day" : unitTime = eUnitTimeType.Day : strUnit = ""
                Case Else : unitTime = eUnitTimeType.Custom
            End Select
            drow("UnitTime") = CInt(unitTime)
            drow("UnitTimeCustom") = strUnit

            drow("LastSaved") = Me.ExtractLastSavedJulianDate(CStr(Me.FixValue(reader, "remarks", "")))

            ' ToDo_JS: ImportModels - check if bWithSeason is relevant in EwE6
            Try
                bWithSeason = CBool(reader("WithSeason"))
            Catch ex As Exception
                bWithSeason = False
            End Try
            'drow("WithSeason") = bWithSeason

            If (bWithSeason) Then
                ' Convert Year1
                If Not Convert.IsDBNull(reader("Year1")) Then
                    strYear = CStr(reader("Year1"))
                    If strYear.Length > 7 Then
                        dt = New DateTime(CInt(strYear.Substring(0, 4)), CInt(strYear.Substring(5, 2)), CInt(strYear.Substring(7, 2)))
                        drow("DateStart") = dt
                        If (strYear.Length > 9) Then
                            ' ToDo_JS: ImportModels - check if no. of steps per year is relevant in EwE6
                            drow("StepsPerYear") = CInt(strYear.Substring(9))
                        End If
                    End If
                End If
                ' Convert Year2
                If Not Convert.IsDBNull(reader("Year2")) Then
                    strYear = CStr(reader("Year2"))
                    If strYear.Length > 7 Then
                        dt = New DateTime(CInt(strYear.Substring(0, 4)), CInt(strYear.Substring(5, 2)), CInt(strYear.Substring(7, 2)))
                        drow("DateEnd") = dt
                    End If
                End If
            End If

            drow("UnitMonetary") = Me.FixValue(reader, "monetaryUnit", "EUR")
            'drow("EcosimVulMultAll") = Me.FixValue(reader, "Ecosim vulMultAll")
            writer.AddRow(drow)
            If Not writer.Commit() Then
                Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_ERROR_COMMIT, "model"), _
                              eMessageType.DataImport, eMessageImportance.Critical, True)
            End If

            Try
                ' Me.AddRemark(reader("remarksCyclePath"), eDataTypes.EwEModel, CInt(drow("ModelID")), eVarNameFlags.CyclePath)
            Catch ex As InvalidOperationException
                ' NOP
            Catch ex As Exception

            End Try

            ' JS 061221: References do not need to be imported for now
            ' ImportRefCode("RefCode", "quickRef")
            ' ImportRefCode("RefCodeCyclePath", "quickRef")

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

#End Region ' Model

#Region " Stanza "

        Private Sub ImportGroupStanza()

            Dim readerStanza As IDataReader = Nothing
            Dim writerStanza As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerLifeStages As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim drowSelect() As DataRow = Nothing
            Dim strStanzaName As String = ""
            Dim iStanzaID As Integer = 0
            Dim iGroupID As Integer = 0
            Dim iSequence As Integer = 0

            ' Clear table(s)
            Me.m_dbTarget.Execute("DELETE * FROM Stanza")

            readerStanza = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Group Stanza] where modelName='{0}' ORDER BY StanzaName, Sequence ASC", Me.m_strModelName))
            If readerStanza Is Nothing Then Return

            writerStanza = m_dbTarget.GetWriter("Stanza")
            writerLifeStages = m_dbTarget.GetWriter("StanzaLifeStage")

            While readerStanza.Read()

                strStanzaName = CStr(readerStanza("StanzaName"))
                iSequence = CInt(readerStanza("Sequence"))

                ' Need to define group first?
                If (Me.HashKey(eDataTypes.Stanza, strStanzaName) = 0) Then

                    ' This must be the first row in the stanza sequence
                    If (iSequence = 1) Then
                        ' Store stanza configuratio
                        iStanzaID += 1

                        ' Write stanza-wide settings
                        drow = writerStanza.NewRow()
                        drow("StanzaID") = iStanzaID
                        drow("StanzaName") = strStanzaName
                        drow("BABsplit") = Me.FixValue(readerStanza, "BABsplit")
                        drow("WmatWinf") = Me.FixValue(readerStanza, "WmatWinf")
                        drow("RecPower") = Me.FixValue(readerStanza, "RecPower")

                        If (Me.m_dbEwE5.GetVersion >= 1.67) Then
                            drow("FixedFecundity") = Me.FixValue(readerStanza, "FixedFecundity")
                        Else
                            drow("FixedFecundity") = 0
                        End If

                        ' JS 060615: EggProd shapes are now scenario dependent, handled in table EcosimStanzaShapes.
                        ' drow("EggProdShape") = Me.FixValue(reader("EggProdShape"))
                        ' JS 070328: HatchCode is now scenario dependent, handled in table EcosimStanzaShapes.
                        ' drow("HatchCode") = Me.FixValue(readerStanzaNames, "HatchCode")

                        writerStanza.AddRow(drow)
                        writerStanza.Commit()

                        ' Remember stanza ID mapping
                        Me.HashKey(eDataTypes.Stanza, strStanzaName) = iStanzaID

                        Me.AddRemark(readerStanza("remarks"), eDataTypes.Stanza, iStanzaID, eVarNameFlags.Name)
                    Else
                        ' Import error: stanza config missing essential first stage
                        Me.LogMessage(cStringUtils.Localize("Multi-stanza configuration {0} missing essential first life stage. This stanza configuration cannot be imported.", strStanzaName), _
                                eMessageType.DataImport, eMessageImportance.Information, True)
                    End If
                End If

                ' Is Stanza configuration available?
                If (Me.HashKey(eDataTypes.Stanza, strStanzaName) = iStanzaID) Then

                    ' #Yes: define life stages
                    drow = writerLifeStages.NewRow()

                    ' Fix FK
                    iGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(readerStanza("groupName")))

                    drow("StanzaID") = iStanzaID
                    drow("GroupID") = iGroupID
                    drow("Sequence") = iSequence

                    ' Write per-group stanza settings
                    drow("AgeStart") = Me.FixValue(readerStanza, "ageStart")
                    drow("Mortality") = Me.FixValue(readerStanza, "Mortality")

                    ' vbK moved to groups
                    'drow("vbK") = Me.FixValue(readerStanza, "vbK", 0.3)

                    ' JS 060621: Removed unused fields
                    'drow("Loo") = Me.FixValue(reader,"Loo")
                    'drow("WtGrow") = Me.FixValue(reader,"WtGrow")
                    'drow("Prepo") = Me.FixValue(reader,"Prepo")
                    'drow("Spare") = Me.FixValue(reader,"spare")
                    'drow("FixAge") = Me.FixValue(reader,"FixAge")

                    ' ToDo_JS: add to stanza table row, not life stage table

                    If (CSng(Me.FixValue(readerStanza, "Biomass", 0)) > 0) Then
                        Try
                            Dim drowLead As DataRow = writerStanza.GetDataTable.Select("StanzaID=" & Me.HashKey(eDataTypes.Stanza, strStanzaName))(0)
                            drowLead.BeginEdit()
                            drowLead("LeadingLifeStage") = iSequence
                            drowLead.EndEdit()
                        Catch ex As Exception

                        End Try
                    End If
                    If (CSng(Me.FixValue(readerStanza, "QB", 0)) > 0) Then
                        Try
                            Dim drowLead As DataRow = writerStanza.GetDataTable.Select("StanzaID=" & Me.HashKey(eDataTypes.Stanza, strStanzaName))(0)
                            drowLead.BeginEdit()
                            drowLead("LeadingCB") = iSequence
                            drowLead.EndEdit()
                        Catch ex As Exception

                        End Try
                    End If

                    writerLifeStages.AddRow(drow)

                End If

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writerLifeStages)
            Me.m_dbTarget.ReleaseWriter(writerStanza)
            Me.m_dbEwE5.ReleaseReader(readerStanza)

        End Sub

#End Region ' Stanza

#Region " Ecopath "

        Private Sub ImportEcopathGroups()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerPedigree As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim nGroupID As Integer = 1
            Dim sTemp As Single = 0.0
            Dim nSequence As Integer = 1 ' Renumber sequence field

            Dim nNumGroups As Integer = CInt(m_dbEwE5.GetValue(cStringUtils.Localize("SELECT COUNT(*) FROM [Group Info] WHERE modelName='{0}'", Me.m_strModelName)))
            Dim nNumLiving As Integer = CInt(m_dbEwE5.GetValue(cStringUtils.Localize("SELECT COUNT(*) FROM [Group Info] WHERE modelName='{0}' AND (TYPE <= 1)", Me.m_strModelName)))

            If (nNumGroups = nNumLiving) Then
                ' Need to murder one group?
            End If

            ' Clear table(s)
            Me.m_dbTarget.Execute("DELETE * FROM EcopathGroup")

            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [Group Info] WHERE modelName='{0}' ORDER BY Sequence ASC", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = m_dbTarget.GetWriter("EcopathGroup")
            writerPedigree = Me.m_dbTarget.GetWriter("EcopathGroupPedigree")

            While reader.Read()

                drow = writer.NewRow()

                drow("GroupID") = nGroupID
                drow("GroupName") = Me.FixValue(reader, "groupName")
                drow("Sequence") = nSequence
                drow("Type") = Me.FixValue(reader, "Type")
                ' -- validate area --'
                sTemp = CSng(Me.FixValue(reader, "Area"))
                If (sTemp <= 0 Or sTemp > 1) Then
                    sTemp = 1.0
                    Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_FIX_GROUPAREA, _
                            CStr(reader("groupName")), _
                            sTemp), _
                            eMessageType.DataImport, eMessageImportance.Information, True)
                End If
                drow("Area") = sTemp
                ' -- end validate --'
                drow("EcoEfficiency") = Me.FixValue(reader, "EcoEfficiency", cCore.NULL_VALUE)
                drow("ProdBiom") = Me.FixValue(reader, "ProdBiom", cCore.NULL_VALUE)
                drow("ConsBiom") = Me.FixValue(reader, "ConsBiom", cCore.NULL_VALUE)
                drow("ProdCons") = Me.FixValue(reader, "ProdCons", cCore.NULL_VALUE)
                drow("Biomass") = Me.FixValue(reader, "Biomass", cCore.NULL_VALUE)
                drow("BiomAcc") = Me.FixValue(reader, "BiomAcc")
                drow("BiomAccRate") = Me.FixValue(reader, "BiomAccRate")
                ' -- validate unassim --'
                sTemp = CSng(Me.FixValue(reader, "Unassim"))
                If CInt(reader("Type")) = 1 Then
                    ' For producers set the GS to 0
                    sTemp = 0.0
                    Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_FIX_GROUPUNASSIM, _
                            CStr(reader("groupName")), _
                            sTemp), _
                            eMessageType.DataImport, eMessageImportance.Information, True)
                End If
                ' drow("Unassim") = CSng(if(sTemp > 1, sTemp / 100.0, sTemp))
                If sTemp > 1 Then
                    drow("Unassim") = CSng(sTemp / 100.0)
                Else
                    drow("Unassim") = CSng(sTemp)
                End If
                ' -- end validate --'
                drow("Unassim") = Me.FixValue(reader, "Unassim")
                drow("DtImports") = Me.FixValue(reader, "DtImports")
                drow("Export") = Me.FixValue(reader, "Export")
                drow("Catch") = Me.FixValue(reader, "Catch")
                drow("ImpVar") = Me.FixValue(reader, "ImpVar")
                drow("NonMarketValue") = Me.FixValue(reader, "Non-market value")
                drow("Immigration") = Me.FixValue(reader, "Immigration")
                drow("Emigration") = Me.FixValue(reader, "Emigration")
                drow("EmigRate") = Me.FixValue(reader, "EmigRate")
                drow("ProdResp") = Me.FixValue(reader, "ProdResp")
                drow("RespCons") = Me.FixValue(reader, "RespCons")
                drow("RespBiom") = Me.FixValue(reader, "RespBiom")
                drow("Consumption") = Me.FixValue(reader, "Consumption")
                ' -- validate respiration --'
                sTemp = CSng(reader("Respiration"))
                If CInt(reader("Type")) < 1 Then
                    sTemp = 0.0
                End If
                drow("Respiration") = sTemp
                ' -- end validate --'
                drow("Production") = Me.FixValue(reader, "Production")
                drow("Unassimilated") = Me.FixValue(reader, "Unassimilated")
                drow("GroupIsFish") = Me.FixValue(reader, "GroupIsFish")
                drow("GroupIsInvert") = Me.FixValue(reader, "GroupIsInvert")
                drow("Poolcolor") = Me.FixColor(CInt(Me.FixValue(reader, "PoolColor", &H0)))

                writer.AddRow(drow)
                ' Commit to allow FK in Remark
                writer.Commit()

                ' Remember group ID mapping
                Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupName"))) = nGroupID
                ' Remember group group index mapping
                MappedID(eDataTypes.EcoPathGroupInput, nSequence) = nGroupID

                ' Import Remarks
                Me.AddRemark(reader("remarks"), eDataTypes.EcoPathGroupInput, nGroupID, eVarNameFlags.Name)
                Me.AddRemark(reader("Non-market remarks"), eDataTypes.EcoPathGroupInput, nGroupID, eVarNameFlags.NonMarketValue)
                Me.AddRemark(reader("Migration remarks"), eDataTypes.EcoPathGroupInput, nGroupID, eVarNameFlags.Immig)

                ' Import pedigree
                Me.AddPedigree(writerPedigree, CInt(Me.FixValue(reader, "Pedigree1", cCore.NULL_VALUE)), nGroupID, eVarNameFlags.Biomass)
                Me.AddPedigree(writerPedigree, CInt(Me.FixValue(reader, "Pedigree2", cCore.NULL_VALUE)), nGroupID, eVarNameFlags.PBInput)
                Me.AddPedigree(writerPedigree, CInt(Me.FixValue(reader, "Pedigree3", cCore.NULL_VALUE)), nGroupID, eVarNameFlags.QBInput)
                Me.AddPedigree(writerPedigree, CInt(Me.FixValue(reader, "Pedigree4", cCore.NULL_VALUE)), nGroupID, eVarNameFlags.DietComp)
                Me.AddPedigree(writerPedigree, CInt(Me.FixValue(reader, "Pedigree5", cCore.NULL_VALUE)), nGroupID, eVarNameFlags.TCatchInput)

                ' ToDo_JS: 18Jul08: we do not have alternate input yet in EwE6
                ' AddRemark(reader("Altinput remarks"), drow, "GroupID", ?)

                ' JS 061221: References do not need to be imported for now
                ' ImportRefCode("RefCode", "quickRef")
                ' ImportRefCode("Non-market RefCode", "Non-market quickRef")
                ' ImportRefCode("Migration RefCode", "Migration quickRef")
                ' ImportRefCode("Altinput RefCode", "Altinput quickRef")

                nGroupID += 1
                nSequence += 1

            End While

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbTarget.ReleaseWriter(writerPedigree)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportGroupxGroup()

            Dim reader As IDataReader = Nothing
            Dim readerPred As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim nPreyID As Integer = 0
            Dim nPredatorID As Integer = 0
            Dim sValue As Single = 0.0

            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Group x Group] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = m_dbTarget.GetWriter("EcopathDietComp")

            While reader.Read()

                drow = writer.NewRow()

                nPredatorID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupName")))
                nPreyID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupColName")))

                ' Establish foreign key relationships
                drow("PreyID") = nPreyID
                drow("PredID") = nPredatorID

                ' -- correct diet --'
                '041019VC: remove diets for producers w/o q/b
                ' If there should be any leftover diet for a producer then get rid of it
                sValue = CSng(Me.FixValue(reader, "diet"))
                ' Is a producer with no q/b? (carbon models can have this)
                readerPred = m_dbTarget.GetReader(cStringUtils.Localize("SELECT Type, ConsBiom FROM EcopathGroup WHERE (GroupID={0})", nPredatorID))
                readerPred.Read()
                If CSng(readerPred("ConsBiom")) <= 0.0 And CSng(readerPred("Type")) = 1.0 Then
                    ' #Yes: set diet components to 0
                    sValue = 0.0
                End If
                Me.m_dbTarget.ReleaseReader(readerPred)
                drow("Diet") = sValue
                ' -- end correct --'

                drow("DetritusFate") = Me.FixValue(reader, "detritus fate")
                drow("MTI") = Me.FixValue(reader, "MTI")
                drow("Electivity") = Me.FixValue(reader, "electivity")
                writer.AddRow(drow)

                ' Import remarks
                AddRemark(reader("remarksDiet"), eDataTypes.EcoPathGroupInput, nPredatorID, eVarNameFlags.DietComp, eDataTypes.EcoPathGroupInput, nPreyID)
                AddRemark(reader("remarksDF"), eDataTypes.EcoPathGroupInput, nPredatorID, eVarNameFlags.DiscardFate, eDataTypes.EcoPathGroupInput, nPreyID)

                ' JS 061221: References do not need to be imported for now
                ' ImportRefCode("RefCodeDiet", "quickRefDiet")
                ' ImportRefCode("RefCodeDF", "quickRefDF")

            End While

            ' writer.Commit()
            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Public Sub ImportGroupSize()

            Dim strGroupName As String = ""
            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim nGroupID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim drowFK As DataRow = Nothing
            Dim drowSelect() As DataRow = Nothing
            Dim dt As DataTable = Nothing
            Dim sValue As Single = 0.0!

            ' Get writer
            writer = m_dbTarget.GetWriter("EcopathGroup")
            dt = writer.GetDataTable()

            ' Merge EwE5 Group Size data with EwE6 GroupInfo for non-stanza groups
            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Group size] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            While reader.Read()

                strGroupName = CStr(reader("groupName"))
                ' Get EwE6 GroupID for this record
                nGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, strGroupName)
                ' Find row(s) in GroupInfo that correspond to this GroupID
                drowSelect = dt.Select(cStringUtils.Localize("GroupID={0}", nGroupID))

                If (drowSelect.Length = 1) Then

                    drow = drowSelect(0)
                    drow.BeginEdit()

                    ' Import values
                    drow("AinLW") = Me.FixValue(reader, "AinLW")
                    drow("BinLW") = Me.FixValue(reader, "BinLW")
                    drow("Loo") = Me.FixValue(reader, "Loo")
                    drow("winf") = Me.FixValue(reader, "winf")
                    drow("t0") = Me.FixValue(reader, "t0", -9999)
                    drow("Tcatch") = Me.FixValue(reader, "Tcatch")
                    drow("Tmax") = Me.FixValue(reader, "Tmax")

                    ' Special case vbK merge
                    sValue = CSng(Me.FixValue(reader, "vbK", 0.0))
                    If sValue < 0 Then sValue = 0.0!
                    drow("vbK") = sValue

                    drow.EndEdit()

                End If
            End While
            Me.m_dbEwE5.ReleaseReader(reader)

            ' Read stanza vbK values
            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT groupName, vbK from [Group Stanza] where modelName='{0}'", Me.m_strModelName))
            If reader IsNot Nothing Then

                While reader.Read()
                    strGroupName = CStr(reader("groupName"))
                    ' Get EwE6 GroupID for this record
                    nGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, strGroupName)
                    drowSelect = dt.Select(cStringUtils.Localize("GroupID={0}", nGroupID))

                    If (drowSelect.Length = 1) Then
                        drow = drowSelect(0)
                        drow.BeginEdit()
                        drow("vbK") = Me.FixValue(reader, "vbK", 0.3!)
                        drow.EndEdit()
                    End If
                End While

                Me.m_dbEwE5.ReleaseReader(reader)
            End If

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

        Private Sub ImportGear()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim nFleetID As Integer = 1
            Dim nSequence As Integer = 1 ' Renumber sequence field

            ' Clear table(s)
            Me.m_dbTarget.Execute("DELETE * FROM EcopathFleet")

            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Gear] where modelName='{0}' ORDER BY Sequence ASC", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = m_dbTarget.GetWriter("EcopathFleet")

            While reader.Read()

                drow = writer.NewRow()

                drow("FleetID") = nFleetID
                drow("FleetName") = Me.FixValue(reader, "gearName")
                drow("Sequence") = nSequence
                drow("FixedCost") = Me.FixValue(reader, "fixedCost")
                drow("VariableCost") = Me.FixValue(reader, "variableCost")
                drow("SailingCost") = Me.FixValue(reader, "SailingCost")
                drow("Poolcolor") = Me.FixColor(CInt(Me.FixValue(reader, "PoolColor", &H0)))

                writer.AddRow(drow)
                writer.Commit()

                ' Remember Fleet ID mapping
                Me.HashKey(eDataTypes.FleetInput, CStr(reader("gearName"))) = nFleetID
                ' Remember fleet index mapping
                MappedID(eDataTypes.FleetInput, nSequence) = nFleetID

                ' Map remarks
                Me.AddRemark(reader("remarksCost"), eDataTypes.FleetInput, nFleetID, eVarNameFlags.FixedCost)
                ' JS 060622: Fleet size remark tied to FleetInput since EwE6 has no FleetSize variable
                Me.AddRemark(reader("remarkFleetSize"), eDataTypes.FleetInput, nFleetID, eVarNameFlags.Name)

                ' JS 061221: References do not need to be imported for now
                ' ImportRefCode("RefCode", "quickRef")
                ' ImportRefCode("RefcodeFleetSize", "quickRef")

                nFleetID += 1
                nSequence += 1

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportCatch()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim drowFK As DataRow = Nothing
            Dim nGroupID As Integer = 0
            Dim nFleetID As Integer = 0

            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Catch] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = m_dbTarget.GetWriter("EcopathCatch")

            While reader.Read()

                drow = writer.NewRow()

                ' Get GroupID
                nGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupName")))
                ' Get FleetID (a.k.a. Gear)
                nFleetID = Me.HashKey(eDataTypes.FleetInput, CStr(reader("gearName")))

                drow("GroupID") = nGroupID
                drow("FleetID") = nFleetID
                drow("Landing") = Me.FixValue(reader, "Landing")
                drow("Discards") = Me.FixValue(reader, "discards")
                drow("Price") = Me.FixValue(reader, "price")

                writer.AddRow(drow)
                writer.Commit()

                ' Map remarks
                Me.AddRemark(reader("remarksCatch"), eDataTypes.FleetInput, nFleetID, eVarNameFlags.Landings, eDataTypes.EcoPathGroupInput, nGroupID)
                Me.AddRemark(reader("remarksPrice"), eDataTypes.FleetInput, nFleetID, eVarNameFlags.OffVesselPrice, eDataTypes.EcoPathGroupInput, nGroupID)
                Me.AddRemark(reader("remarksDiscards"), eDataTypes.FleetInput, nFleetID, eVarNameFlags.Discards, eDataTypes.EcoPathGroupInput, nGroupID)

                ' JS 061221: References do not need to be imported for now
                'ImportRefCode("RefCodeCatch", "quickRefCatch")
                'ImportRefCode("RefCodeDiscards", "quickRefDiscards")
                'ImportRefCode("RefCodePrice", "quickRefPrice")

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportCatchCodes()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim drowFK As DataRow = Nothing
            Dim nGroupID As Integer = 0

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Catch Codes] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcopathCatchCode")

            While reader.Read()

                drow = writer.NewRow()

                ' Get GroupID
                nGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupName")))

                drow("GroupID") = nGroupID
                drow("Code") = Me.FixValue(reader, "code")
                drow("Proportion") = Me.FixValue(reader, "proportion")
                writer.AddRow(drow)

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportDiscardFate()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim nGroupID As Integer = 0
            Dim nFleetID As Integer = 0

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Discard Fate] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcopathDiscardFate")

            While reader.Read()

                drow = writer.NewRow()

                ' Get GroupID
                nGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupColName")))
                ' Get FleetID (EwE5: Gear)
                nFleetID = Me.HashKey(eDataTypes.FleetInput, CStr(reader("gearName")))

                drow("GroupID") = nGroupID
                drow("FleetID") = nFleetID
                drow("DiscardFate") = Me.FixValue(reader, "DiscardFate")

                writer.AddRow(drow)

                ' Map remarks
                Me.AddRemark(reader("remarks"), eDataTypes.FleetInput, nFleetID, eVarNameFlags.DiscardFate, eDataTypes.EcoPathGroupInput, nGroupID)

                ' JS 061221: References do not need to be imported for now
                'ImportRefCode("RefCode", "quickRefCatch")

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportBasicRemarks()

            Dim reader As IDataReader = Nothing
            Dim nGroupID As Integer = 0
            Dim varName As eVarNameFlags = eVarNameFlags.NotSet

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [BasicParam Remarks] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            While reader.Read()
                ' Get GroupID
                nGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupName")))
                ' Translate col to varname
                Select Case CInt(reader("paramNum"))
                    Case 1 ' feeding type, not used anymore in EwE5
                    Case 2 : varName = eVarNameFlags.Area
                    Case 3 : varName = eVarNameFlags.BiomassAreaInput
                    Case 4 : varName = eVarNameFlags.PBInput
                    Case 5 : varName = eVarNameFlags.QBInput
                    Case 6 : varName = eVarNameFlags.EEInput
                    Case 7 : varName = eVarNameFlags.GEInput
                    Case 8 : varName = eVarNameFlags.BioAccumOutput
                    Case 9 : varName = eVarNameFlags.GS
                    Case 10 : varName = eVarNameFlags.DetImp
                    Case Else : varName = eVarNameFlags.NotSet
                End Select

                If (varName <> eVarNameFlags.NotSet) And (nGroupID > 0) Then
                    Me.AddRemark(reader("remarks"), eDataTypes.EcoPathGroupInput, nGroupID, varName)
                End If

            End While

        End Sub

        Private Sub ImportPedigree()

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim lLevels As List(Of Integer) = Nothing
            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim varName As eVarNameFlags = eVarNameFlags.NotSet
            Dim strDescription As String = ""
            Dim drow As DataRow = Nothing
            Dim iSequence As Integer = 1
            Dim iLevelID As Integer = 1

            ' Check if defaults need to be discarded
            reader = Me.m_dbEwE5.GetReader("SELECT DISTINCT Parameter FROM Pedigree")
            Dim lVars As New List(Of eVarNameFlags)
            While reader.Read
                Select Case CInt(reader("Parameter"))
                    Case 1 : varName = eVarNameFlags.Biomass
                    Case 2 : varName = eVarNameFlags.PBInput
                    Case 3 : varName = eVarNameFlags.QBInput
                    Case 4 : varName = eVarNameFlags.DietComp
                    Case 5 : varName = eVarNameFlags.TCatchInput
                    Case Else : varName = eVarNameFlags.NotSet
                End Select
                lVars.Add(varName)
            End While
            Me.m_dbEwE5.ReleaseReader(reader)

            Try
                For Each varName In lVars
                    Me.m_dbTarget.Execute(cStringUtils.Localize("DELETE FROM [Pedigree] WHERE VarName='{0}'", cin.GetVarName(varName)))
                Next
            Catch ex As Exception

            End Try

            reader = Me.m_dbEwE5.GetReader("SELECT * from [Pedigree]")
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("Pedigree")

            While reader.Read()
                ' Translate col to varname
                Select Case CInt(reader("Parameter"))
                    Case 1 : varName = eVarNameFlags.Biomass
                    Case 2 : varName = eVarNameFlags.PBInput
                    Case 3 : varName = eVarNameFlags.QBInput
                    Case 4 : varName = eVarNameFlags.DietComp
                    Case 5 : varName = eVarNameFlags.TCatchInput
                    Case Else : varName = eVarNameFlags.NotSet
                End Select

                strDescription = CStr(Me.FixValue(reader, "Parameter description", ""))

                If (varName <> eVarNameFlags.NotSet) And (Not String.IsNullOrEmpty(strDescription)) Then

                    Try
                        drow = writer.NewRow()
                        iSequence = CInt(Me.FixValue(reader, "Option", 0))
                        drow("LevelID") = iLevelID
                        drow("VarName") = cin.GetVarName(varName)
                        drow("Sequence") = iSequence
                        drow("IndexValue") = CSng(Me.FixValue(reader, "Value", 0.0!))
                        drow("Confidence") = CInt(Me.FixValue(reader, "Var", 0))
                        If strDescription.Length > 49 Then
                            drow("LevelName") = strDescription.Substring(0, Math.Min(45, strDescription.Length)) & "..."
                            drow("Description") = strDescription
                        Else
                            drow("LevelName") = strDescription
                            drow("Description") = ""
                        End If
                        writer.AddRow(drow)
                    Catch ex As Exception

                    End Try

                    lLevels = Me.m_dicPedigreeLevels(varName)
                    While lLevels.Count <= iSequence
                        lLevels.Add(cCore.NULL_VALUE)
                    End While
                    lLevels(iSequence) = iLevelID

                    iLevelID += 1

                End If

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

#End Region ' Ecopath

#Region " Ecoranger "

        Private Sub ImportEcoranger()

            Dim reader As IDataReader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [EcoRanger] where modelName='{0}'", Me.m_strModelName))
            If (reader Is Nothing) Then Return
            Me.LogMessage(My.Resources.CoreMessages.IMPORT_WARNING_ECORANGER, eMessageType.DataImport, eMessageImportance.Information, True)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

#End Region ' Ecoranger

#Region " Ecosim "

        Private Function ImportEcosim() As Boolean

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim nScenarioID As Integer = 1
            Dim bHasScenarios As Boolean = False
            Dim nMaxForcePoints As Integer = 1200

            ' Clear table(s)
            Me.m_dbTarget.Execute("DELETE * FROM EcosimScenario")

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return False

            writer = Me.m_dbTarget.GetWriter("EcosimScenario")

            While reader.Read()

                drow = writer.NewRow()

                drow("ScenarioID") = nScenarioID
                drow("ScenarioName") = Me.FixValue(reader, "Scenario")
                drow("Description") = Me.FixValue(reader, "remarks", "")
                ' drow("npairs") = Me.FixValue(reader,"npairs")                   ' DISCONTINUED
                ' drow("TimeStep") = Me.FixValue(reader,"TimeStep")               ' DISCONTINUED
                drow("TotalTime") = Me.FixValue(reader, "TotalTime")
                drow("StepSize") = Me.FixValue(reader, "StepSize")
                drow("EquilibriumStepSize") = Me.FixValue(reader, "EquilibriumStepSize")
                drow("EquilScaleMax") = Me.FixValue(reader, "EquilScaleMax")
                drow("sorwt") = Me.FixValue(reader, "sorwt")
                drow("SystemRecovery") = Me.FixValue(reader, "SystemRecovery")
                drow("Discount") = Me.FixValue(reader, "Discount")
                drow("NudgeStart") = Me.FixValue(reader, "NudgeStart")
                drow("NudgeEnd") = Me.FixValue(reader, "NudgeEnd")
                drow("NudgeFactor") = Me.FixValue(reader, "NudgeFactor")
                drow("UseNudge") = Me.FixValue(reader, "chkNudge")
                drow("DoInteg") = Me.FixValue(reader, "DoInteg")
                drow("NutBaseFreeProp") = Me.FixValue(reader, "NutBaseFreeProp")
                drow("NutPBmax") = Me.FixValue(reader, "NutPBmax")
                'VC090403: UseVarPQ flag is no longer saved 
                'drow("UseVarPQ") = Me.FixValue(reader, "UseVarPQ")
                drow("LastSaved") = Me.ExtractLastSavedJulianDate(CStr(Me.FixValue(reader, "remarks", "")))

                ' Feb 15: make sure to retain old EwE5 defaults
                drow("ForagingTimeLowerLimit") = 0.1

                nMaxForcePoints = Math.Max(nMaxForcePoints, CInt(drow("TotalTime")) * 12)

                ' Nutrient forcing shape will be resolved when shapes are loaded
                writer.AddRow(drow)

                ' Remember scenario ID mapping
                Me.HashKey(eDataTypes.EcoSimScenario, CStr(reader("Scenario"))) = nScenarioID

                bHasScenarios = True
                nScenarioID += 1

            End While

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

            writer = Me.m_dbTarget.GetWriter("EcosimModel")
            dt = writer.GetDataTable()
            drow = dt.Rows(0)
            drow.BeginEdit()
            drow("ForcePoints") = nMaxForcePoints
            drow.EndEdit()
            Me.m_dbTarget.ReleaseWriter(writer)

            Return bHasScenarios

        End Function

        Private Structure SimEnvFunction
            Public Property Sopt As Single
            Public Property SLeft As Single
            Public Property SRight As Single
            Public Property GroupID As Integer
            Public Property GroupName As String
            Public Property ScenarioID As Integer
            Public Property ScenarioName As String
        End Structure

        Private Sub ImportEcosimN()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 0
            Dim iGroupID As Integer = 1

            Dim dtEcosimScenarios As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoSimScenario))
            Dim strScenario As String = ""
            Dim dtEcopathGroups As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoPathGroupInput))
            Dim strGroup As String = ""
            ' Flag stating whether an ecosim group was found for a given ecopath group
            Dim bHasGroup As Boolean = False

            ' JS 120117: Use Ecosim environmental drivers
            Dim lEnvFunctions As New List(Of SimEnvFunction)

            ' JS 070212: Every Ecopath group should have an Ecosim counterpart
            reader = Me.m_dbTarget.GetReader("SELECT * from EcopathGroup")
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcosimScenarioGroup")

            ' For every ecosim scenario
            For Each strScenario In dtEcosimScenarios.Keys

                ' Get scenario link
                iScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, strScenario)

                ' For every ecopath group...
                For Each strGroup In dtEcopathGroups.Keys

                    ' Report sub-status
                    Me.LogProgress(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSIMGROUPS_AT, strScenario, strGroup), Me.m_iStep)

                    ' .. create a new ecosim group

                    ' Check if an ecosim group exists for this ecopath group, ecosim scenario combination
                    reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim N] where modelName='{0}' AND Scenario='{1}' AND groupName='{2}'", _
                            Me.m_strModelName, strScenario, strGroup))

                    bHasGroup = reader.Read()

                    ' Create a new row
                    drow = writer.NewRow()

                    ' Link to ecopath group
                    drow("EcopathGroupID") = Me.HashKey(eDataTypes.EcoPathGroupInput, strGroup)
                    ' Link to scenario
                    drow("ScenarioID") = iScenarioID
                    ' Set group ID
                    drow("GroupID") = iGroupID

                    ' Does this ecosim group exist in the EwE5 database?
                    If bHasGroup Then
                        ' #Yes: copy content
                        drow("Pbmaxs") = Me.FixValue(reader, "pbmaxs")
                        drow("FtimeMax") = Me.FixValue(reader, "FtimeMax")
                        drow("FtimeAdjust") = Me.FixValue(reader, "FtimeAdjust")
                        drow("MoPred") = Me.FixValue(reader, "MoPred")
                        drow("FishRateMax") = Me.FixValue(reader, "FishRateMax")
                        drow("Show") = True  'reader("ShowHide") ' Show all groups when importing
                        drow("RiskTime") = Me.FixValue(reader, "RiskTime")
                        drow("QmQo") = Me.FixValue(reader, "QmQo")
                        drow("CmCo") = Me.FixValue(reader, "CmCo")
                        If (Me.m_dbEwE5.GetVersion >= 1.65) Then
                            drow("SwitchPower") = Me.FixValue(reader, "SwitchPower")
                        End If
                        If (Me.m_dbEwE5.GetVersion() >= 1.725) Then
                            Dim info As New SimEnvFunction()
                            info.Sopt = CSng(Me.FixValue(reader, "SalOpt", 35.0!))
                            info.SLeft = CSng(Me.FixValue(reader, "SdSal", 1000.0!))
                            info.SRight = CSng(Me.FixValue(reader, "SdSal", 1000.0!))
                            info.GroupName = strGroup
                            info.ScenarioName = strScenario
                            info.GroupID = iGroupID
                            info.ScenarioID = iScenarioID
                            lEnvFunctions.Add(info)
                        End If

                        ' No shape imported for this group yet?
                        If Me.HashKey(eDataTypes.FishMort, strGroup, eDataTypes.EcoSimScenario, iScenarioID) = 0 Then
                            ' Succesfully imported the shape?
                            If Me.ImportShape(Me.m_iNextShapeID, eDataTypes.FishMort, reader) = Me.m_iNextShapeID Then
                                ' Remember key for consecutive group instances in other scenarios
                                Me.HashKey(eDataTypes.FishMort, strGroup, eDataTypes.EcoSimScenario, iScenarioID) = Me.m_iNextShapeID
                                ' Assign shape
                                drow("FishMortShapeID") = Me.m_iNextShapeID
                                ' Next shape
                                Me.m_iNextShapeID += 1
                            End If
                        Else
                            drow("FishMortShapeID") = Me.HashKey(eDataTypes.FishMort, strGroup, eDataTypes.EcoSimScenario, iScenarioID)
                        End If
                    Else
                        ' #No: the new group will get all default values
                        Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_FIX_CREATEECOSIMGROUP,
                                iGroupID,
                                strGroup,
                                strScenario),
                                eMessageType.DataImport, eMessageImportance.Information)

                        ' No shape imported for this group yet?
                        If Me.HashKey(eDataTypes.FishMort, strGroup, eDataTypes.EcoSimScenario, iScenarioID) <> 0 Then
                            drow("FishMortShapeID") = Me.HashKey(eDataTypes.FishMort, strGroup, eDataTypes.EcoSimScenario, iScenarioID)
                        Else
                            ' Notify world
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_FIX_CREATEFISHMORTSHAPE,
                                    Me.m_iNextShapeID,
                                    strGroup,
                                    strScenario),
                                    eMessageType.DataImport, eMessageImportance.Information)

                            ' Create dummy shape
                            Me.CreateDummyShape(Me.m_iNextShapeID, eDataTypes.FishMort)
                            ' Assign shape
                            drow("FishMortShapeID") = Me.m_iNextShapeID
                            ' Next shape
                            Me.m_iNextShapeID += 1
                        End If

                        ' Populate group fields
                        drow("FishMortShapeID") = 0
                    End If

                    ' Add the row
                    writer.AddRow(drow)
                    ' Commit to row to allow FK links from Remarks
                    writer.Commit()

                    If (bHasGroup) Then
                        ' Import remarks
                        Me.AddRemark(reader("remarks"), eDataTypes.EcoSimScenario, iScenarioID, eVarNameFlags.Name, eDataTypes.EcoSimGroupInput, iGroupID)

                        ' JS 061221: References do not need to be imported for now
                        ' ImportRefCode("RefCode", "quickRef")
                    End If

                    ' Remember Ecosim group input ID mapping
                    Me.HashKey(eDataTypes.EcoSimGroupInput, strGroup, eDataTypes.EcoSimScenario, iScenarioID) = iGroupID

                    ' Next group
                    iGroupID += 1
                    Me.m_dbEwE5.ReleaseReader(reader)

                Next strGroup
            Next strScenario

            Me.m_dbTarget.ReleaseWriter(writer)

            ' Add Ecosim environmental forcing
            If (Me.m_dbEwE5.GetVersion() >= 1.725) Then
                Dim upd As New cDBUpdate6_50_00_27()
                For Each info As SimEnvFunction In lEnvFunctions
                    Dim iShapeID As Integer = 0
                    upd.CreateReponseCurve(Me.m_dbTarget, "Salinity", 0, info.GroupName, info.ScenarioName, info.Sopt, info.SLeft, info.SRight, iShapeID)
                    upd.AssignResponse(Me.m_dbTarget, info.ScenarioID, info.GroupID, Me.m_iNextShapeID, iShapeID)
                    Me.m_iNextShapeID += 1
                Next
            End If
        End Sub

        Private Sub ImportEcosimPairs()

            Dim reader As IDataReader = Nothing
            Dim readerTmp As IDataReader = Nothing
            Dim strAdult As String = ""
            Dim strJuvinile As String = ""
            Dim bWarned As Boolean = False

            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim Pairs] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            While reader.Read

                If Not bWarned Then
                    Me.LogMessage(My.Resources.CoreMessages.IMPORT_WARNING_PAIRSNOTSUPPORTED, eMessageType.DataImport, eMessageImportance.Information, True)
                    bWarned = True
                End If

                Try
                    readerTmp = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Group Info] where modelName='{0}' and sequence={1}", Me.m_strModelName, reader("iadult")))
                    readerTmp.Read()
                    strAdult = CStr(readerTmp("groupName"))
                    Me.m_dbEwE5.ReleaseReader(readerTmp)

                    readerTmp = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Group Info] where modelName='{0}' and sequence={1}", Me.m_strModelName, reader("ijuv")))
                    readerTmp.Read()
                    strJuvinile = CStr(readerTmp("groupName"))
                    Me.m_dbEwE5.ReleaseReader(readerTmp)

                    Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_PAIRDETAILS, _
                            CStr(reader("npairs")), _
                            strJuvinile, _
                            strAdult), _
                            eMessageType.DataImport, eMessageImportance.Information, True)

                Catch ex As Exception

                End Try

            End While

            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportEcosimFishGear()

            Dim reader As IDataReader = Nothing
            Dim readerEcopath As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 0
            Dim iEcopathFleetID As Integer = 0
            Dim iShapeID As Integer = 0
            Dim iFleetID As Integer = 1

            ' JS090826: first day to the EwE25 conference, and time to raise hell. The previous logic failed to import
            '           fleets for Ecosim when only defined in Ecopath.
            Dim dtFleets As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.FleetInput))
            Dim dtScenarios As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoSimScenario))

            writer = Me.m_dbTarget.GetWriter("EcosimScenarioFleet")

            For Each strScenario As String In dtScenarios.Keys

                iScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, strScenario)

                For Each strFleet As String In dtFleets.Keys

                    ' Report sub-status
                    Me.LogProgress(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOSIMFLEETS_AT, strScenario, strFleet), Me.m_iStep)

                    ' Grab foreign keys
                    iEcopathFleetID = Me.HashKey(eDataTypes.FleetInput, strFleet)

                    reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim FishGear] where modelName='{0}' and gearName='{1}' and Scenario='{2}'", _
                                                              Me.m_strModelName, strFleet, strScenario))
                    ' Assume no shape is read
                    iShapeID = 0
                    ' Try to read effort linkage
                    If reader IsNot Nothing Then
                        reader.Read()
                        Try
                            ' Check if shape already imported
                            iShapeID = Me.HashKey(eDataTypes.FishingEffort, CStr(reader("gearName")), eDataTypes.EcoSimScenario, iScenarioID)
                        Catch ex As Exception
                        End Try
                    End If

                    ' Not imported yet? Signal that import is needed after the fleet has been defined
                    If iShapeID = 0 Then iShapeID = Me.m_iNextShapeID

                    drow = writer.NewRow()
                    drow("ScenarioID") = iScenarioID
                    drow("EcopathFleetID") = iEcopathFleetID
                    drow("FleetID") = iFleetID
                    drow("FishRateShapeID") = iShapeID

                    readerEcopath = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [Gear] WHERE (gearName='{0}')", strFleet))
                    readerEcopath.Read()

                    drow("EPower") = Me.FixValue(readerEcopath, "EPower", 3.0!)
                    drow("PCapBase") = Me.FixValue(readerEcopath, "PCapBase", 0.5!)
                    drow("CapDepreciate") = Me.FixValue(readerEcopath, "CapDepreciate", 0.06!)
                    drow("CapBaseGrowth") = Me.FixValue(readerEcopath, "CapBaseGrowth", 0.2!)
                    drow("EffortConversionFactor") = 1.0!

                    m_dbEwE5.ReleaseReader(readerEcopath)
                    readerEcopath = Nothing

                    iFleetID += 1

                    writer.AddRow(drow)
                    writer.Commit()

                    If iShapeID = Me.m_iNextShapeID Then
                        iShapeID = Me.ImportShape(iShapeID, eDataTypes.FishingEffort, reader)
                        Me.HashKey(eDataTypes.FishingEffort, strFleet, eDataTypes.EcoSimScenario, iScenarioID) = iShapeID
                        Me.m_iNextShapeID += 1
                    End If

                    Me.m_dbEwE5.ReleaseReader(reader)

                Next strFleet
            Next strScenario

            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

#Region " Forcing shapes "

        Private Sub ImportEcosimnShapes()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim readerSub As IDataReader = Nothing
            Dim writerSub As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drowSub As DataRow = Nothing
            Dim drowSelect() As DataRow = Nothing
            ' EwE5: Shape number implicitly identifies a shape type by comparing its value to predefined value ranges
            Dim iShapeNumber As Integer = 0
            ' EwE6: Shape type explicitly identifies a shape type
            Dim shapeDataType As eDataTypes = eDataTypes.NotSet
            Dim bIsEggShape As Boolean = False
            Dim bIsTimeShape As Boolean = False
            Dim bIsSeasonal As Boolean = False
            Dim iScenarioID As Integer = 0

            ' JS061218: A great performance boost can be achieved by opening all shape writers here rather than
            '           once for every shape. These writers can be made global to the class, similar to the 
            '           remarks writer, or can have local scope, to be passed on to the writing methods.


            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [Ecosim nshapes] WHERE modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcosimShape")

            While reader.Read()

                ' Get shape number
                iShapeNumber = CInt(reader("nShapeNumber"))
                ' Get scenario number
                iScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, CStr(reader("Scenario")))
                ' Reset seasonal flag
                bIsSeasonal = False

                ' Determine shape type from shape number
                shapeDataType = eDataTypes.NotSet

                Select Case iShapeNumber

                    Case 1 To 99

                        ' EwE5 stores seasonal shapes as IDs 1..3
                        bIsSeasonal = (iShapeNumber <= 3)

                        ' Time and/or egg?
                        bIsEggShape = Me.IsUsedAsEggShape(iShapeNumber)
                        bIsTimeShape = Me.IsUsedAsTimeShape(iShapeNumber)

                        ' The Eggs win
                        If (bIsTimeShape) Then shapeDataType = eDataTypes.Forcing
                        If (bIsEggShape) Then shapeDataType = eDataTypes.EggProd

                        ' If shape type undetermined (e.g. not allocated, only defined), import as time forcing function
                        ' VERIFY_JS: Check with VC how to import Forcing shapes that are defined but not used in scenarios.
                        '            For now, unused shapes are assigned as Time shapes.
                        If shapeDataType = eDataTypes.NotSet Then shapeDataType = eDataTypes.Forcing

                        ' Found dual assignment?
                        If (bIsEggShape And bIsTimeShape) Then
                            ' VERIFY_JS: Check with VC how to import dual assigned Forcing shapes (egg and time)
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_FORCINGMULTIPLEASSIGNMENTS, _
                                    iShapeNumber, _
                                    shapeDataType.ToString()), _
                                    eMessageType.DataImport, eMessageImportance.Information, True)
                        End If

                    Case 100 To Integer.MaxValue
                        shapeDataType = eDataTypes.Mediation

                    Case Else
                        ' Do not use this shape
                        shapeDataType = eDataTypes.NotSet

                End Select

                ' Is valid shapetype
                If (shapeDataType <> eDataTypes.NotSet) Then
                    ' #Yes: is not imported yet?
                    If (Me.HashKey(shapeDataType, CStr(iShapeNumber), eDataTypes.EcoSimScenario, iScenarioID) = 0) Then

                        ' #Yes: try to import shape
                        Dim iAssignedShapeID As Integer = Me.ImportShape(Me.m_iNextShapeID, shapeDataType, reader, bIsSeasonal)

                        ' Import failed?
                        If (iAssignedShapeID = cCore.NULL_VALUE) Then
                            ' #Yes: report failure
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_FORCINGNOTIMPORTED, iShapeNumber, shapeDataType.ToString), _
                                    eMessageType.DataImport, eMessageImportance.Information, True)
                        Else
                            ' #No: remember how shape was imported
                            Me.HashKey(shapeDataType, CStr(iShapeNumber), eDataTypes.EcoSimScenario, iScenarioID) = iAssignedShapeID
                            ' Was imported as a new shape?
                            If (iAssignedShapeID = Me.m_iNextShapeID) Then
                                ' #Yes: prepare next ID
                                Me.m_iNextShapeID += 1
                            End If
                        End If

                    Else
                        ' This indicates a programming error
                        Debug.Assert(False, "Shape type " & shapeDataType & " unknown")
                        Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_FORCINGDUPLICATE, _
                                iShapeNumber, _
                                shapeDataType.ToString()), _
                                eMessageType.DataImport, eMessageImportance.Information, True)

                    End If ' Valid ShapeType is set
                Else
                    ' Invalid shape number
                    Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_FORCINGTYPEMISSING, _
                            iShapeNumber), _
                            eMessageType.DataImport, eMessageImportance.Information, True)
                End If ' Not imported yet
            End While

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

            Me.AssignEcosimScenarioForcingShapes()
            Me.AssignStanzaShapes()

        End Sub

#Region " Shape duplicates management "

        Private Class cForcingShapeData

            Public DBID As Integer = 0
            Public Title As String = ""
            Public ZScale As String = ""
            Public ZMaxScale As String = ""
            Public [Type] As String = ""
            Public ShapeDataType As eDataTypes = eDataTypes.NotSet
            Public ShapeType As eShapeFunctionType = eShapeFunctionType.NotSet
            Public Seasonal As Boolean = False
            'Public Yzero As Single = 0
            'Public YBase As Single = 0
            'Public YEnd As Single = 0
            'Public Steep As Single = 0
            Public IMedBase As Single = 0

            Public Overrides Function Equals(ByVal obj As Object) As Boolean

                If Not (TypeOf (obj) Is cForcingShapeData) Then Return False

                Dim src As cForcingShapeData = DirectCast(obj, cForcingShapeData)

                If Me.ShapeDataType <> src.ShapeDataType Then Return False
                If String.Compare(Me.Title, src.Title, True) <> 0 Then Return False
                If String.Compare(Me.ZScale, src.ZScale) <> 0 Then
                    Return False
                End If
                Return True

            End Function

        End Class

        Private Function GetDuplicate(ByVal fsd As cForcingShapeData) As cForcingShapeData
            For Each fsdTest As cForcingShapeData In Me.m_lImportedForcingShapes
                ' Is duplicate?
                If fsdTest.Equals(fsd) Then
                    ' #Yes: return duplicate
                    Return fsdTest
                End If
            Next
            Return Nothing
        End Function

#End Region ' Shape duplicates management

        Private Function ImportShape(ByVal iShapeID As Integer, ByVal shapeDataType As eDataTypes, _
                ByVal reader As IDataReader, Optional ByVal bIsSeasonal As Boolean = False) As Integer

            ' import shape specific data in subtable
            Select Case shapeDataType

                Case eDataTypes.Forcing, eDataTypes.EggProd, eDataTypes.Mediation
                    Return Me.ImportForcingShape(iShapeID, shapeDataType, reader, bIsSeasonal)

                Case eDataTypes.FishingEffort, eDataTypes.FishMort
                    Return Me.ImportFishingShape(iShapeID, shapeDataType, reader, bIsSeasonal)

                Case Else
                    ' Whoot

            End Select
            Return cCore.NULL_VALUE

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Imports a forcing shape, returning the actual ID assigned to the 
        ''' shape. This code will detect shape duplicates, and may return the ID 
        ''' of a previously imported shape in case of duplication.
        ''' </summary>
        ''' <param name="iShapeID">ID to assign to shape.</param>
        ''' <param name="shapeDataType">Type of shape.</param>
        ''' <param name="reader">Database reader to read shape from.</param>
        ''' <param name="bIsSeasonal">States whether shape is seasonal.</param>
        ''' <returns>The ID for the imported shape. Note that this ID may indicate
        ''' an earlier imported shape in case of duplicates.</returns>
        ''' -------------------------------------------------------------------
        Private Function ImportForcingShape(ByVal iShapeID As Integer, _
                                            ByVal shapeDataType As eDataTypes, _
                                            ByVal reader As IDataReader, _
                                            Optional ByVal bIsSeasonal As Boolean = False) As Integer

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim fsd As New cForcingShapeData()
            Dim fsdDuplicate As cForcingShapeData = Nothing

            ' 1. Read db, populate cForcingShapeData structure
            Try
                fsd.ShapeDataType = shapeDataType
                fsd.Seasonal = bIsSeasonal

                Select Case shapeDataType

                    Case eDataTypes.Forcing, eDataTypes.EggProd
                        Dim strZScale As String = ""
                        Me.SplitZScale(CStr(reader("zScale")), strZScale, fsd.Title)
                        fsd.ZScale = Me.RebuildNumberListString(strZScale)
                        fsd.ZMaxScale = CStr(Me.FixValue(reader, "zMaxScale", 0))

                    Case eDataTypes.Mediation
                        fsd.ZScale = Me.RebuildNumberListString(CStr(Me.FixValue(reader, "zScale", "")))
                        fsd.ZMaxScale = CStr(Me.FixValue(reader, "zMaxScale", 0))
                        fsd.IMedBase = CSng(Me.FixValue(reader, "XBaseLine", 0.25))

                End Select

            Catch ex As Exception
                ' Whoah!
                Return cCore.NULL_VALUE
            End Try

            ' 2. Find if duplicate shape exists
            fsdDuplicate = Me.GetDuplicate(fsd)
            If (fsdDuplicate IsNot Nothing) Then
                ' #Yes: return original DBID
                Console.WriteLine("Shape {0} '{1}' already exists as ID {2}", _
                                  fsdDuplicate.ShapeDataType, fsdDuplicate.Title, fsdDuplicate.DBID)
                Return fsdDuplicate.DBID
            End If

            ' Ah! Is a new shape!
            fsd.DBID = iShapeID

            Try
                ' Add new shape
                writer = Me.m_dbTarget.GetWriter("EcosimShape")
                drow = writer.NewRow()
                drow("ShapeID") = iShapeID
                ' ShapeNumber is no longer stored; determined at load
                ' drow("nShapeNumber") = nShapeNumber 
                drow("ShapeType") = fsd.ShapeDataType
                drow("IsSeasonal") = fsd.Seasonal
                writer.AddRow(drow)

                Me.m_dbTarget.ReleaseWriter(writer)
                writer = Nothing

            Catch ex As Exception
                ' No need to localize, send to log only
                Me.LogMessage(cStringUtils.Localize("Forcing data failed to import as type {1}: {2}", shapeDataType.ToString(), ex.Message), _
                        eMessageType.DataImport, eMessageImportance.Information)
                Return cCore.NULL_VALUE
            End Try

            ' import shape specific data in subtable
            Select Case shapeDataType
                Case eDataTypes.Forcing
                    writer = Me.m_dbTarget.GetWriter("EcosimShapeTime")
                    drow = writer.NewRow()
                    drow("Title") = fsd.Title
                    drow("zScale") = fsd.ZScale
                    drow("zMaxScale") = fsd.ZMaxScale
                    drow("FunctionType") = eShapeFunctionType.NotSet
                    drow("FunctionParams") = ""

                Case eDataTypes.EggProd
                    writer = Me.m_dbTarget.GetWriter("EcosimShapeEggProd")
                    drow = writer.NewRow()
                    drow("Title") = fsd.Title
                    drow("zScale") = fsd.ZScale
                    drow("zMaxScale") = fsd.ZMaxScale
                    drow("FunctionType") = eShapeFunctionType.NotSet
                    drow("FunctionParams") = ""

                Case eDataTypes.Mediation
                    Dim nShapeNumber As Integer = CInt(Me.m_dbTarget.GetValue("SELECT COUNT(*) FROM EcosimShapeMediation"))

                    ' JS 19April 2010 (Sascha is 5!!!): do NOT adjust title; this will cripple ability to find duplicates
                    'fsd.Title = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_MEDIATIONSHAPE, CInt(nShapeNumber + 1))

                    writer = Me.m_dbTarget.GetWriter("EcosimShapeMediation")
                    drow = writer.NewRow()
                    drow("IMedBase") = fsd.IMedBase
                    drow("zScale") = fsd.ZScale
                    drow("Title") = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_MEDIATIONSHAPE, CInt(nShapeNumber + 1))
                    drow("FunctionType") = eShapeFunctionType.NotSet
                    drow("FunctionParams") = ""

                Case Else
                    Debug.Assert(False, "Shape type not set during import; cannot continue")

            End Select

            ' Forge FK
            drow("ShapeID") = fsd.DBID

            writer.AddRow(drow)
            Me.m_dbTarget.ReleaseWriter(writer)
            writer = Nothing

            ' Log new shape
            Console.WriteLine("Shape {0} '{1}' imported as ID {2}", fsd.ShapeDataType, fsd.Title, iShapeID)
            Me.m_lImportedForcingShapes.Add(fsd)

            Return fsd.DBID
        End Function

        Private Function ImportFishingShape(ByVal iShapeID As Integer, _
                                            ByVal shapeDataType As eDataTypes, _
                                            ByVal reader As IDataReader, _
                                            Optional ByVal bIsSeasonal As Boolean = False) As Integer

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                ' Add new shape
                writer = Me.m_dbTarget.GetWriter("EcosimShape")
                drow = writer.NewRow()
                drow("ShapeID") = iShapeID
                drow("ShapeType") = CInt(shapeDataType)
                drow("IsSeasonal") = bIsSeasonal
                writer.AddRow(drow)

                Me.m_dbTarget.ReleaseWriter(writer)
                writer = Nothing

            Catch ex As Exception
                ' No need to localize, send to log only
                Me.LogMessage(cStringUtils.Localize("Effort or Mort shape data failed to import as type {1}: {2}", shapeDataType.ToString(), ex.Message), _
                        eMessageType.DataImport, eMessageImportance.Information)
                Return cCore.NULL_VALUE
            End Try

            Select Case shapeDataType

                Case eDataTypes.FishingEffort
                    Dim nShapeNumber As Integer = CInt(Me.m_dbTarget.GetValue("SELECT COUNT(*) FROM EcosimShapeFishRate"))

                    writer = Me.m_dbTarget.GetWriter("EcosimShapeFishRate")
                    drow = writer.NewRow()
                    drow("Title") = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_FISHRATESHAPE, CInt(nShapeNumber + 1))
                    drow("zScale") = Me.RebuildNumberListString(CStr(Me.FixValue(reader, "FishRateGear", "")))

                Case eDataTypes.FishMort
                    Dim nShapeNumber As Integer = CInt(Me.m_dbTarget.GetValue("SELECT COUNT(*) FROM EcosimShapeFishMort"))

                    writer = Me.m_dbTarget.GetWriter("EcosimShapeFishMort")
                    drow = writer.NewRow()
                    drow("Title") = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_FISHMORTSHAPE, CInt(nShapeNumber + 1))
                    drow("zScale") = Me.RebuildNumberListString(CStr(Me.FixValue(reader, "FishRateNo", "")))

                Case Else
                    Debug.Assert(False, "Shape type not set during import; cannot continue")

            End Select

            ' Forge FK
            drow("ShapeID") = iShapeID

            writer.AddRow(drow)
            Me.m_dbTarget.ReleaseWriter(writer)
            writer = Nothing

            Return iShapeID
        End Function

        Private Function CreateDummyShape(ByVal iShapeID As Integer, ByVal shapeDataType As eDataTypes, _
                Optional ByVal bIsSeasonal As Boolean = False) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strZScale As String = ""
            Dim strTitle As String = ""
            Dim strType As String = ""
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_dbTarget.GetWriter("EcosimShape")

                ' Add new shape
                drow = writer.NewRow()
                drow("ShapeID") = iShapeID
                ' ShapeNumber is no longer stored; determined at load
                ' drow("nShapeNumber") = nShapeNumber 
                drow("ShapeType") = CInt(shapeDataType)
                drow("IsSeasonal") = bIsSeasonal
                writer.AddRow(drow)

                Me.m_dbTarget.ReleaseWriter(writer)
                writer = Nothing

            Catch ex As Exception
                ' No need to localize, send to log only
                Me.LogMessage(cStringUtils.Localize("Failed to create dummy shape {0}: {1}", iShapeID, ex.Message), _
                        eMessageType.DataImport, eMessageImportance.Information)
                Return False
            End Try

            ' import shape specific data in subtable
            Select Case shapeDataType
                Case eDataTypes.Forcing
                    writer = Me.m_dbTarget.GetWriter("EcosimShapeTime")
                    drow = writer.NewRow()
                    drow("FunctionType") = eShapeFunctionType.NotSet

                Case eDataTypes.EggProd
                    writer = Me.m_dbTarget.GetWriter("EcosimShapeEggProd")
                    drow = writer.NewRow()
                    drow("Title") = strTitle
                    ' New in EwE6
                    drow("FunctionType") = eShapeFunctionType.NotSet

                Case eDataTypes.Mediation
                    Dim nShapeNumber As Integer = CInt(Me.m_dbTarget.GetValue("SELECT COUNT(*) FROM EcosimShapeMediation"))

                    writer = Me.m_dbTarget.GetWriter("EcosimShapeMediation")
                    drow = writer.NewRow()
                    ' New in EwE6
                    drow("Title") = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_MEDIATIONSHAPE, CInt(nShapeNumber + 1))
                    ' New in EwE6
                    drow("FunctionType") = eShapeFunctionType.NotSet

                Case eDataTypes.FishingEffort
                    Dim nShapeNumber As Integer = CInt(Me.m_dbTarget.GetValue("SELECT COUNT(*) FROM EcosimShapeFishRate"))

                    writer = Me.m_dbTarget.GetWriter("EcosimShapeFishRate")
                    drow = writer.NewRow()
                    drow("Title") = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_FISHRATESHAPE, CInt(nShapeNumber + 1))

                Case eDataTypes.FishMort
                    Dim nShapeNumber As Integer = CInt(Me.m_dbTarget.GetValue("SELECT COUNT(*) FROM EcosimShapeFishMort"))

                    writer = Me.m_dbTarget.GetWriter("EcosimShapeFishMort")
                    drow = writer.NewRow()
                    drow("Title") = cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_FISHMORTSHAPE, CInt(nShapeNumber + 1))

                Case Else
                    Debug.Assert(False, "Shape type not set during import; cannot continue")

            End Select

            ' Forge FK
            drow("ShapeID") = iShapeID

            writer.AddRow(drow)
            Me.m_dbTarget.ReleaseWriter(writer)
            writer = Nothing

            Return bSucces
        End Function

        Private Sub AssignEcosimScenarioForcingShapes()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim iScenarioID As Integer = 0
            Dim iNutShapeNumber As Integer = 0
            Dim iNutShapeID As Integer = 0
            Dim iSalinityShapeNumber As Integer = 0
            Dim iSalinityShapeID As Integer = 0
            Dim drowSelect() As DataRow = Nothing
            Dim drow As DataRow = Nothing

            ' Resolve Scenario dependent forcing shapes
            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [Ecosim] WHERE modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcosimScenario")

            While reader.Read()

                iScenarioID = HashKey(eDataTypes.EcoSimScenario, CStr(reader("Scenario")))
                iNutShapeID = 0
                iSalinityShapeID = 0

                iNutShapeNumber = CInt(Me.FixValue(reader, "NutForceNumber", 0))
                If Me.m_dbEwE5.GetVersion >= 1.725 Then
                    iSalinityShapeNumber = CInt(Me.FixValue(reader, "SalinityForceNo", 0))
                End If

                ' Resolve shape IDs
                If (iNutShapeNumber > 0) Then
                    iNutShapeID = Me.HashKey(eDataTypes.Forcing, CStr(iNutShapeNumber), eDataTypes.EcoSimScenario, iScenarioID)
                End If
                If (iSalinityShapeNumber > 0) Then
                    iSalinityShapeID = Me.HashKey(eDataTypes.Forcing, CStr(iSalinityShapeNumber), eDataTypes.EcoSimScenario, iScenarioID)
                End If

                ' Are there shapes to assign?
                If ((iNutShapeID + iSalinityShapeID) > 0) Then
                    ' #Yes: venture yonder, Jimmy
                    dt = writer.GetDataTable()
                    drowSelect = dt.Select(cStringUtils.Localize("ScenarioID={0}", iScenarioID))
                    If (drowSelect.Length = 1) Then
                        ' Sanity check
                        drow = drowSelect(0)
                        drow.BeginEdit()
                        drow("NutForcingShapeID") = iNutShapeID
                        drow("SalinityForcingShapeID") = iSalinityShapeID
                        drow.EndEdit()
                    End If
                End If

            End While
            Me.m_dbEwE5.ReleaseReader(reader)
            Me.m_dbTarget.ReleaseWriter(writer)
        End Sub

        Private Sub AssignStanzaShapes()

            Dim strEcosimScenario As String = ""
            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim iEggShape As Integer = 0
            Dim iEggShapeID As Integer = 0
            Dim iHatchShape As Integer = 0
            Dim iHatchShapeID As Integer = 0
            Dim dtEcosimScenarios As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoSimScenario))
            Dim iNumEcosimScenarios As Integer = 0
            Dim iEcosimScenarioID As Integer = 0
            Dim drow As DataRow = Nothing

            Me.m_dbTarget.Execute("DELETE * FROM EcosimStanzaShape")

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT groupName, stanzaName, EggProdShape, HatchCode FROM [Group Stanza] WHERE (modelName='{0}')", Me.m_strModelName))
            If reader Is Nothing Then Return

            ' Determine number of ecosim scenarios
            iNumEcosimScenarios = dtEcosimScenarios.Values.Count

            writer = Me.m_dbTarget.GetWriter("EcosimStanzaShape")
            Try
                While reader.Read()
                    iEggShape = CInt(Me.FixValue(reader, "EggProdShape", 0))

                    If (Me.m_dbEwE5.GetVersion >= 1.66) Then
                        iHatchShape = CInt(Me.FixValue(reader, "HatchCode", 0))
                    End If

                    ' Has shape assignments?
                    If (iEggShape + iHatchShape > 0) Then

                        ' JS 24nov07: EwE5 links stanza configs (non-sim scenario dept) to shapes (ecosim scenario dept) via an index that is only
                        '             meaningful from the context of a loaded scenario. EwE6 instead loads shapes ecosim scenario independent.
                        '             Therefore, the importer will be unable to import this link when importing more than one Ecosim scenario: 
                        '             What EwE5 scenario should this shape come from?!

                        ' Has only one Ecosim scenario?
                        If (iNumEcosimScenarios = 1) Then
                            ' Ugh, get one and only scenario ID
                            For Each iEcosimScenarioID In dtEcosimScenarios.Values : Next

                            ' Try to resolve egg prod shape ID for this scenario
                            If iEggShape > 0 Then
                                iEggShapeID = Me.HashKey(eDataTypes.EggProd, CStr(iEggShape), eDataTypes.EcoSimScenario, iEcosimScenarioID)
                            End If
                            ' Try to resolve forcing shape ID for this scenario
                            If iHatchShape > 0 Then
                                iHatchShapeID = Me.HashKey(eDataTypes.Forcing, CStr(iHatchShape), eDataTypes.EcoSimScenario, iEcosimScenarioID)
                            End If
                            ' Found shapes?
                            If (iEggShapeID + iHatchShapeID) > 0 Then
                                drow = writer.NewRow()
                                ' Map foreign keys
                                drow("StanzaID") = HashKey(eDataTypes.Stanza, CStr(reader("stanzaName")))
                                ' Link shapes (leave missing shape links to DBNull)
                                If (iEggShapeID > 0) Then drow("EggprodShapeID") = iEggShapeID
                                If (iHatchShapeID > 0) Then drow("HatchCodeShapeID") = iHatchShapeID
                                writer.AddRow(drow)
                            End If
                        Else
                            ' Multiple ecosim scenarios: do not import, throw a warning
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_MULTISTANZASHAPE, CStr(reader("stanzaName"))), _
                                        eMessageType.DataImport, eMessageImportance.Information, True)
                        End If
                    End If

                End While
            Catch ex As Exception
            End Try

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; splits EwE5 zscale memo into a title and zscale parts.
        ''' </summary>
        ''' <param name="strIn">The string to split.</param>
        ''' <param name="strZScale">Zscale part.</param>
        ''' <param name="strTitle">Title part.</param>
        ''' -------------------------------------------------------------------
        Private Sub SplitZScale(ByVal strIn As String, ByRef strZScale As String, ByRef strTitle As String)
            ' Separate title from Zscale data. EwE5 stores the title in the first 
            ' 20 characters of the ZScale data.
            strTitle = strIn.Substring(1, 19).Trim()
            strZScale = strIn.Substring(21).Trim()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, determines if a given shape number is used as a Time 
        ''' forcing shape.
        ''' </summary>
        ''' <param name="nShapeNumber">The shape number to check.</param>
        ''' <returns>
        ''' True if the given shape number, for both the given model and scenario,
        ''' is used as a Time forcing shape.</returns>
        ''' -------------------------------------------------------------------
        Private Function IsUsedAsTimeShape(ByVal nShapeNumber As Integer) As Boolean
            If (Me.m_dbEwE5.GetVersion < 1.705) Then
                Dim strDetectEggSQL As String = "SELECT COUNT(*) FROM [ECOSIM NXN] WHERE (modelName='{0}') AND (seasonType={1})"
                Return CInt(Me.m_dbEwE5.GetValue(cStringUtils.Localize(strDetectEggSQL, Me.m_strModelName, nShapeNumber))) > 0
            Else
                Dim strDetectEggSQL As String = "SELECT COUNT(*) FROM [ECOSIM NXN Forcing] WHERE (modelName='{0}') AND (FunctionNumber={1}) AND (IsMedFunction=False)"
                Return CInt(Me.m_dbEwE5.GetValue(cStringUtils.Localize(strDetectEggSQL, Me.m_strModelName, nShapeNumber))) > 0
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, determines if a given shape number is used as an
        ''' Egg Production forcing shape.
        ''' </summary>
        ''' <param name="nShapeNumber">The shape number to check.</param>
        ''' <returns>
        ''' True if the given shape number for the given model is used 
        ''' as an Egg production forcing shape.</returns>
        ''' -------------------------------------------------------------------
        Private Function IsUsedAsEggShape(ByVal nShapeNumber As Integer) As Boolean
            ' EggShapes in EwE5 are assigned to stanza groups independent of scenario!
            Dim strDetectEggSQL As String = "SELECT COUNT(*) FROM [GROUP STANZA] WHERE (modelName='{0}') AND (EggProdShape={1})"
            Return CInt(Me.m_dbEwE5.GetValue(cStringUtils.Localize(strDetectEggSQL, Me.m_strModelName, nShapeNumber))) > 0
        End Function

        Private Sub ImportEcosimNxN()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim sVul As Single = 0.0!
            Dim sDBVersion As Single = 0.0!
            Dim iScenarioID As Integer = 0
            Dim drow As DataRow = Nothing

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim NxN] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcosimScenarioForcingMatrix")

            sDBVersion = CSng(Me.m_dbEwE5.GetValue("SELECT MAX(Version) FROM [Database specifications]"))

            While reader.Read()

                drow = writer.NewRow()

                iScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, CStr(reader("Scenario")))
                ' Link scenario
                drow("ScenarioID") = iScenarioID

                ' JS 5oct08: vulmult indexed by (prey, pred). groupName referred to prey, groupColName to pred
                ' Link prey (group)
                drow("PreyID") = Me.HashKey(eDataTypes.EcoSimGroupInput, CStr(reader("groupName")), eDataTypes.EcoSimScenario, iScenarioID)
                ' Link predator (group)
                drow("PredID") = Me.HashKey(eDataTypes.EcoSimGroupInput, CStr(reader("groupColName")), eDataTypes.EcoSimScenario, iScenarioID)

                ' Vulnerability
                sVul = CSng(Me.FixValue(reader, "vulnerability", 2.0))
                If sVul < 1.0! Then sVul = 2.0!
                drow("vulnerability") = sVul

#If 0 Then ' Discontinued in 1.71, now allocated from [Ecosim NxN Forcing]
                ' Link to forcing shape
                iShape = CInt(reader("seasonType"))
                iShapeID = 0
                If (iShape > 0) Then
                    Try
                        ' EwE5 shape assigment may not be valid anymore - test for success
                        iShapeID = HashKey(eDataTypes.Forcing, CStr(iShape), iScenarioID)
                    Catch e As Exception
                    End Try
                End If
                drow("ForcingShapeID") = iShapeID

                ' Link to mediation shape
                iShape = CInt(reader("MediationType"))
                iShapeID = 0
                If (iShape > 0) Then
                    Try
                        ' EwE5 shape assigment may not be valid anymore - test for success
                        iShapeID = HashKey(eDataTypes.Mediation, CStr(CInt(100 + iShape)))
                    Catch e As Exception
                    End Try
                End If
                drow("MediationShapeID") = iShapeID
#End If

                ' Store the row
                writer.AddRow(drow)

            End While

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportEcosimMedWeights()

            Dim reader As IDataReader = Nothing
            Dim writerGroup As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerFleet As cEwEDatabase.cEwEDbWriter = Nothing
            Dim iScenarioID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim strKey As String = ""
            Dim iGroupID As Integer = 0
            Dim iFleetID As Integer = 0
            Dim iShapeID As Integer = 0

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim MedWeights] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writerGroup = Me.m_dbTarget.GetWriter("EcosimScenarioShapeMedWeightsGroup")
            writerFleet = Me.m_dbTarget.GetWriter("EcosimScenarioShapeMedWeightsFleet")

            While reader.Read()

                ' Group or Fleet?
                ' EwE6 will split this into two tables
                strKey = CStr(reader("groupName"))
                iScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, CStr(reader("Scenario")))
                iGroupID = Me.HashKey(eDataTypes.EcoSimGroupInput, strKey, eDataTypes.EcoSimScenario, iScenarioID)
                iFleetID = Me.HashKey(eDataTypes.FleetInput, strKey)
                iShapeID = Me.HashKey(eDataTypes.Mediation, CStr(100 + CInt(reader("CurPlot"))), eDataTypes.EcoSimScenario, iScenarioID)

                If (iGroupID > 0) Then
                    ' Add group
                    drow = writerGroup.NewRow()
                    drow("ScenarioID") = iScenarioID
                    drow("GroupID") = iGroupID
                    drow("ShapeID") = iShapeID
                    drow("MedWeights") = reader("MedWeights")
                    writerGroup.AddRow(drow)
                ElseIf (iFleetID > 0) Then
                    ' Add fleet
                    drow = writerFleet.NewRow()
                    drow("ScenarioID") = iScenarioID
                    drow("FleetID") = iFleetID
                    drow("ShapeID") = iShapeID
                    drow("MedWeights") = reader("MedWeights")
                    writerFleet.AddRow(drow)
                Else
                    ' Unknown: ignore
                End If

            End While

            Me.m_dbTarget.ReleaseWriter(writerGroup)
            Me.m_dbTarget.ReleaseWriter(writerFleet)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportEcosimNxNInteraction()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            ' Special field values
            Dim iScenarioID As Integer = 0
            ' EwE5: Shape number implicitly identifies a shape type by comparing its value to predefined value ranges
            Dim iShapeNumber As Integer = 0
            Dim iShapeID As Integer = 0
            Dim iPredID As Integer = 0
            Dim iPreyID As Integer = 0
            Dim bIsMediation As Boolean = False
            Dim iFFApplication As eForcingFunctionApplication = 0

            ' EwE6: Shape type explicitly identifies a shape type
            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Ecosim NxN Forcing] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcosimScenarioPredPreyShape")

            While reader.Read()

                ' Resolve scenario ID
                iScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, CStr(reader("Scenario")))
                ' Resolve shape ID, depending on shape type
                If (CBool(reader("IsMedFunction")) = True) Then
                    iShapeID = Me.HashKey(eDataTypes.Mediation, CStr(100 + CInt(reader("FunctionNumber"))), eDataTypes.EcoSimScenario, iScenarioID)
                Else
                    iShapeID = Me.HashKey(eDataTypes.Forcing, CStr(reader("FunctionNumber")), eDataTypes.EcoSimScenario, iScenarioID)
                End If
                iPreyID = Me.HashKey(eDataTypes.EcoSimGroupInput, CStr(reader("GroupName")), eDataTypes.EcoSimScenario, iScenarioID)
                iPredID = Me.HashKey(eDataTypes.EcoSimGroupInput, CStr(reader("GroupColName")), eDataTypes.EcoSimScenario, iScenarioID)

                ' MedFunction flag does not need importing since shape type can be looked up via iShapeID

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("ShapeID") = iShapeID
                drow("PredID") = iPredID
                drow("PreyID") = iPreyID
                drow("FunctionType") = Me.FixValue(reader, "FunctionType", 1)
                writer.AddRow(drow)

            End While

            Me.m_dbEwE5.ReleaseReader(reader)
            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub ' ImportEcosimNxNInteraction

#End Region ' Forcing shapes 

#Region " Time series "

        ''' <summary>
        ''' Import Time Series data
        ''' </summary>
        Private Sub ImportTimeSeries()
            Me.ImportTSDatasets()
            Me.ImportTS()
        End Sub

        Private Sub ImportTSDatasets()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iDatasetID As Integer = 0
            Dim strDataset As String = ""
            Dim strDatasetLast As String = ""
            Dim iNumPoints As Integer = 0

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [Time Series] WHERE modelName='{0}' ORDER BY Dataset", Me.m_strModelName))
            writer = Me.m_dbTarget.GetWriter("EcosimTimeSeriesDataset")

            While reader.Read()

                ' Get dataset name of this time series
                strDataset = CStr(reader("Dataset"))
                ' Is a new dataset?
                If (String.Compare(strDatasetLast, strDataset, False) <> 0) Then
                    ' #Yes: switch datasets
                    ' Was another dataset handled?
                    If (drow IsNot Nothing) Then
                        ' #Yes: Commit this dataset
                        writer.AddRow(drow)
                    End If

                    ' Next dataset!
                    iDatasetID += 1
                    drow = writer.NewRow()
                    drow("DatasetID") = iDatasetID
                    drow("DatasetName") = strDataset
                    drow("Description") = ""
                    drow("Author") = ""
                    drow("Contact") = ""
                    ' All TS within a dataset have the same start year
                    drow("FirstYear") = Me.FixValue(reader, "FirstYear", 1950)
                    ' Calculate number of years in this time series
                    Dim strData As String = CStr(Me.FixValue(reader, "MemoField", ""))
                    ' Set as initial max number of years for this dataset 
                    iNumPoints = CInt(strData.Length / 10)
                    drow("NumPoints") = iNumPoints
                    drow("DataInterval") = eTSDataSetInterval.Annual

                    Me.HashKey(eDataTypes.TimeSeriesDataset, strDataset) = iDatasetID
                    strDatasetLast = strDataset

                Else
                    ' #No: processing same dataset
                    ' Calculate number of years in this time series
                    Dim strData As String = CStr(Me.FixValue(reader, "MemoField", ""))
                    ' Find max across dataset so far
                    iNumPoints = Math.Max(iNumPoints, CInt(strData.Length / 10))
                    ' Store this max
                    drow("NumPoints") = iNumPoints
                End If

            End While

            ' Commit last dataset
            If (drow IsNot Nothing) Then writer.AddRow(drow)

            Me.m_dbTarget.ReleaseWriter(writer, True)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

        Private Sub ImportTS()

            Dim reader As IDataReader = Nothing
            Dim writerTimeSeries As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerGroup As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerFleet As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerShape As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerShapeTime As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iTimeSeriesID As Integer = 0
            Dim iDatasetID As Integer = 0
            Dim iGroupID As Integer = 0
            Dim iFleetID As Integer = 0
            Dim eType As eTimeSeriesType = 0
            Dim strMemo As String = ""

            If (Me.m_dbEwE5.GetVersion >= 1.62) Then
                reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Time Series] where modelName='{0}' ORDER BY SequenceNo ASC", Me.m_strModelName))
            Else
                reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [Time Series] where modelName='{0}'", Me.m_strModelName))
            End If

            If reader Is Nothing Then Return

            ' Time series are scenario-independent
            writerTimeSeries = Me.m_dbTarget.GetWriter("EcosimTimeSeries")
            writerGroup = Me.m_dbTarget.GetWriter("EcosimTimeSeriesGroup")
            writerFleet = Me.m_dbTarget.GetWriter("EcosimTimeSeriesFleet")
            writerShape = Me.m_dbTarget.GetWriter("EcosimShape")
            writerShapeTime = Me.m_dbTarget.GetWriter("EcosimShapeTime")

            While reader.Read()

                ' Map EwE5 time series type to EwE6 eTimeSeriesType enum
                Select Case CInt(Me.FixValue(reader, "DatType", 0))
                    Case 0
                        eType = eTimeSeriesType.BiomassRel
                    Case 1
                        eType = eTimeSeriesType.BiomassAbs
                    Case -1
                        eType = eTimeSeriesType.BiomassForcing
                    Case 2
                        eType = eTimeSeriesType.TimeForcing
                    Case 3
                        eType = eTimeSeriesType.FishingEffort
                    Case 4
                        eType = eTimeSeriesType.FishingMortality
                    Case 5
                        eType = eTimeSeriesType.TotalMortality
                    Case -5
                        eType = eTimeSeriesType.ConstantTotalMortality
                    Case 6
                        eType = eTimeSeriesType.Catches
                    Case -6
                        eType = eTimeSeriesType.CatchesForcing
                    Case 61
                        eType = eTimeSeriesType.CatchesRel
                    Case 7
                        eType = eTimeSeriesType.AverageWeight
                        'Case 8
                        '    eType = eTimeSeriesType.EcotracerConcRel
                        'Case 9
                        '    eType = eTimeSeriesType.EcotracerConcAbs
                End Select

                ' JS 07may07: time series assignments have changed in EwE6. A time series is always connected to either a fleet
                '             (via EcosimTimeSeriesFleet) or a group (via EcosimTimeSeriesGroup). Both tables then link one-on-one
                '             to the actual time series data in EcosimTimeSeries.
                Select Case cTimeSeriesFactory.TimeSeriesCategory(eType)

                    Case eTimeSeriesCategoryType.Forcing

                        ' Check for duplicates
                        Dim fsd As New cForcingShapeData()

                        fsd.ShapeDataType = eDataTypes.Forcing
                        fsd.Title = CStr(Me.FixValue(reader, "DatName", "")).Trim()
                        strMemo = CStr(Me.FixValue(reader, "MemoField", ""))
                        fsd.ZScale = Me.RebuildNumberListString(strMemo, CChar(" "), 10, cCore.N_MONTHS)

                        If Me.GetDuplicate(fsd) IsNot Nothing Then

                            drow = writerShape.NewRow()
                            drow("ShapeID") = Me.m_iNextShapeID
                            drow("ShapeType") = eDataTypes.Forcing
                            writerShape.AddRow(drow)
                            'writerShape.Commit()

                            drow = writerShapeTime.NewRow()
                            drow("ShapeID") = Me.m_iNextShapeID
                            drow("Title") = fsd.Title
                            drow("FunctionType") = eShapeFunctionType.NotSet
                            drow("FunctionParams") = ""

                            drow("Zscale") = fsd.ZScale
                            writerShapeTime.AddRow(drow)

                            Console.WriteLine("Time series {0} '{1}' imported as FF ID {2}", fsd.ShapeDataType, fsd.Title, Me.m_iNextShapeID)
                            Me.m_lImportedForcingShapes.Add(fsd)
                            Me.m_iNextShapeID += 1

                        End If

                    Case eTimeSeriesCategoryType.Fleet

                        iDatasetID = Me.HashKey(eDataTypes.TimeSeriesDataset, CStr(reader("Dataset")))
                        iFleetID = Me.MappedID(eDataTypes.FleetInput, CInt(reader("Pool")))

                        ' Is this fleet missing?
                        If (iFleetID = 0) Then
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_TIMESERIESFLEET, _
                                Me.FixValue(reader, "DatName", ""), _
                                Me.FixValue(reader, "Dataset", ""), _
                                CInt(reader("Pool"))), _
                                eMessageType.DataImport, eMessageImportance.Information, True)
                        Else
                            iTimeSeriesID += 1

                            drow = writerTimeSeries.NewRow()
                            drow("TimeSeriesID") = iTimeSeriesID
                            drow("Sequence") = iTimeSeriesID
                            drow("DatasetID") = iDatasetID
                            drow("DatType") = eType
                            drow("DatName") = Me.FixValue(reader, "DatName", "")
                            'drow("FirstYear") = Me.FixValue(reader, "FirstYear", 1950)

                            strMemo = CStr(Me.FixValue(reader, "MemoField", ""))
                            drow("TimeValues") = Me.RebuildNumberListString(strMemo, CChar(" "), 10)
                            'drow("NumYears") = CInt(strMemo.Length / 10)

                            ' JS 06Nov07: Time series imported with weight of 1 (not 0!)
                            If (Me.m_dbEwE5.GetVersion() >= 1.61) Then
                                Dim sWeight As Single = CSng(Me.FixValue(reader, "WtType", 1.0!))
                                If sWeight <= 0.0! Then sWeight = 1.0!
                                drow("WtType") = sWeight
                            Else
                                drow("WtType") = 1.0!
                            End If
                            writerTimeSeries.AddRow(drow)

                            drow = writerFleet.NewRow()
                            drow("TimeSeriesID") = iTimeSeriesID
                            drow("FleetID") = iFleetID
                            writerFleet.AddRow(drow)

                        End If

                    Case eTimeSeriesCategoryType.Group

                        iDatasetID = Me.HashKey(eDataTypes.TimeSeriesDataset, CStr(reader("Dataset")))
                        iGroupID = Me.MappedID(eDataTypes.EcoPathGroupInput, CInt(reader("Pool")))

                        ' Is this group missing?
                        If (iGroupID = 0) Then
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_TIMESIERIESGROUP, _
                                    Me.FixValue(reader, "DatName", ""), _
                                    Me.FixValue(reader, "Dataset", ""), _
                                    CInt(reader("Pool"))), _
                                    eMessageType.DataImport, eMessageImportance.Information, True)
                        Else
                            iTimeSeriesID += 1

                            drow = writerTimeSeries.NewRow()
                            drow("TimeSeriesID") = iTimeSeriesID
                            drow("Sequence") = iTimeSeriesID
                            drow("DatasetID") = iDatasetID
                            drow("DatType") = eType
                            drow("DatName") = Me.FixValue(reader, "DatName", "")
                            'drow("FirstYear") = Me.FixValue(reader, "FirstYear", 1950)

                            strMemo = CStr(Me.FixValue(reader, "MemoField", ""))
                            drow("TimeValues") = Me.RebuildNumberListString(strMemo, CChar(" "), 10)
                            'drow("NumYears") = CInt(strMemo.Length / 10)

                            ' JS 29Nov07: Time series imported with weight of 1 (not 0!)
                            drow("WtType") = 1.0! ' Me.FixValue(reader, "WtType", 1.0!)
                            writerTimeSeries.AddRow(drow)

                            drow = writerGroup.NewRow()
                            drow("TimeSeriesID") = iTimeSeriesID
                            drow("GroupID") = iGroupID
                            drow("VariableName") = ""
                            writerGroup.AddRow(drow)
                        End If

                    Case eTimeSeriesCategoryType.FleetGroup
                        Debug.Assert(False, "This can't be; this type of series was introduced 10 years after EwE5 died")

                    Case eTimeSeriesCategoryType.NotSet
                        'Trying to import unkown time series type - ignore this TS
                        Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_WARNING_TIMESERIESTYPE, _
                                Me.FixValue(reader, "DatName", ""), _
                                Me.FixValue(reader, "Dataset", ""), _
                                eType.ToString()), _
                                eMessageType.DataImport, eMessageImportance.Information, True)

                End Select

            End While

            Me.m_dbTarget.ReleaseWriter(writerTimeSeries, True)
            Me.m_dbTarget.ReleaseWriter(writerGroup, True)
            Me.m_dbTarget.ReleaseWriter(writerFleet, True)
            Me.m_dbTarget.ReleaseWriter(writerShape, True)
            Me.m_dbTarget.ReleaseWriter(writerShapeTime, True)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

#End Region ' Time series 

#End Region ' Ecosim

#Region " Ecospace "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>True if a scenario was imported.</returns>
        ''' -------------------------------------------------------------------
        Private Function ImportEcospaceScenario() As Boolean

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim nScenarioID As Integer = 1
            Dim iEcosimScenarioID As Integer = -1
            Dim depthmap As Single(,) = Nothing

            ' Clear table(s)
            Me.m_dbTarget.Execute("DELETE * FROM EcospaceScenario")

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [EcoSpace] WHERE modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return False

            writer = Me.m_dbTarget.GetWriter("EcospaceScenario")

            While reader.Read()

                drow = writer.NewRow()
                drow("ScenarioID") = nScenarioID
                drow("ScenarioName") = reader("Scenario")
                drow("Description") = Me.FixValue(reader, "remarks", "")
                ' Try to resolve ecosim scenario ID. The original Ecosim scenario may not exist anymore which is fine.
                iEcosimScenarioID = Me.HashKey(eDataTypes.EcoSimScenario, CStr(Me.FixValue(reader, "SimScenario", "")))
                drow("EcosimScenarioID") = iEcosimScenarioID
                drow("Inrow") = Me.FixValue(reader, "Inrow")
                drow("Incol") = Me.FixValue(reader, "Incol")

                Dim sCellLength As Single = CSng(Me.FixValue(reader, "CellLength"))
                If sCellLength = 0 Then
                    Dim sIDH_SS As Single = CSng(Me.FixValue(reader, "IDH_SS", 2))
                    If sIDH_SS = 0 Then sIDH_SS = 2
                    sCellLength = 1 / sIDH_SS
                End If
                drow("CellLength") = sCellLength

                Dim lUDH_UL As Long = CLng(Me.FixValue(reader, "IDH_UL", 0))
                drow("MinLat") = CSng((Math.Truncate(lUDH_UL / 10000) - 900) / 10)
                drow("MinLon") = CSng((lUDH_UL - Math.Truncate(lUDH_UL / 10000) * 10000) / 10 - 180)

                drow("TimeStep") = Me.FixValue(reader, "TimeStep", 0.25)
                ' PredictEffort could not be set in the EwE5 UI. Should be set true, 'cause False is going to screw up your model run
                drow("PredictEffort") = True ' Me.FixValue(reader, "PredictEffort", True)
                drow("LastSaved") = Me.ExtractLastSavedJulianDate(CStr(Me.FixValue(reader, "remarks", "")))

                ' JS 28nov06: habitats now db-linked in EcospaceScenarioHabitat table to allow any number of habitats
                'If sDBVers < 1.557 Then  'first read to old habitats below then update below
                '    For i As Integer = 0 To 8
                '        drow(cStringUtils.Localize("Habitat{0}", i)) = Me.FixValue(reader, cStringUtils.Localize("Habitat{0}", i))
                '    Next
                'End If

                ReDim depthmap(CInt(drow("InRow")), CInt(drow("InCol")))
                Me.m_dicDepthMaps(nScenarioID) = depthmap

                writer.AddRow(drow)
                writer.Commit()

                ' Import Remarks
                ' Me.AddRemark(reader("remarks"), eDataTypes.EcoSpaceScenario, nScenarioID)

                ' JS 061221: References do not need to be imported for now
                'ImportRefCode("RefCode", "quickRef")

                ' Remember scenario ID mapping
                Me.HashKey(eDataTypes.EcoSpaceScenario, CStr(reader("Scenario"))) = nScenarioID

                nScenarioID += 1

            End While

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

            ' Return whether at least one scenario was added
            Return (nScenarioID > 1)
        End Function

        Private Sub ImportEcospaceHabitats()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 0
            Dim iHabitatID As Integer = 1
            Dim strHabitat As String = ""

            ' First define 'All' habitat for every scenario
            ' The EwE6 database will contain a definition for the All habitat. EwE5 tables provide information for this habitat
            ' while this habitat is not explicitly defined in the EwE5 database. It merely exists in the EwE5 GUI.
            Dim dtEcospaceScenarios As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoSpaceScenario))
            Dim strEcospaceScenario As String = ""

            writer = Me.m_dbTarget.GetWriter("EcospaceScenarioHabitat")

            ' For every ecospace scenario
            For Each iScenarioID In dtEcospaceScenarios.Values
                ' Create new row
                drow = writer.NewRow()
                ' Set FKs
                drow("ScenarioID") = iScenarioID
                ' HabitatID is the unique database ID for an Ecospace habitat
                drow("HabitatID") = iHabitatID
                ' Sequence determines habitat order
                drow("Sequence") = iHabitatID
                drow("HabitatName") = My.Resources.CoreDefaults.CORE_DEFAULT_HABITAT_ALL
                ' There
                writer.AddRow(drow)
                ' Remember 'All' Habitat mapping
                Me.HashKey(eDataTypes.EcospaceHabitat, "0", eDataTypes.EcoSpaceScenario, iScenarioID) = iHabitatID
                ' Next
                iHabitatID += 1
            Next

            ' Now import habitat information using most recent EwE5 format
            reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [Ecospace habitats] WHERE modelName='{0}'", Me.m_strModelName))
            While reader.Read()
                ' Resolve scenario ID
                iScenarioID = Me.HashKey(eDataTypes.EcoSpaceScenario, CStr(reader("scenario")))
                ' Add new row
                drow = writer.NewRow()
                ' Populate FKs
                drow("ScenarioID") = iScenarioID
                ' HabitatID is the unique database ID for an Ecospace habitat
                drow("HabitatID") = iHabitatID
                ' Sequence determines habitat order
                drow("Sequence") = iHabitatID
                drow("HabitatName") = reader("HabitatText")
                writer.AddRow(drow)

                ' Remember habitat ID mapping
                Me.HashKey(eDataTypes.EcospaceHabitat, CStr(reader("HabitatNo")), eDataTypes.EcoSpaceScenario, iScenarioID) = iHabitatID

                ' Next
                iHabitatID += 1

            End While

            Me.m_dbEwE5.ReleaseReader(reader)
            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

        Private Sub ImportEcospaceMPA()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 1
            Dim nMPAID As Integer = 1
            Dim strMPA As String = ""
            Dim sbMPA As StringBuilder = Nothing

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [EcoSpace MPA] WHERE modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcospaceScenarioMPA")

            While reader.Read()

                iScenarioID = Me.HashKey(eDataTypes.EcoSpaceScenario, CStr(reader("Scenario")))

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("MPAID") = nMPAID
                drow("Sequence") = reader("MPANo")
                drow("MPAName") = reader("MPAName")
                ' MPAMonth: EwE5 uses a string to represent a field of 12 boolean flags, where 'O' indicates that
                ' the MPA is open for fishing, and 'C' that the MPA is closed for fishing.
                ' Ewe6 uses a '1' when the MPA is open for fishing, and '0' when the MPA is closed.
                strMPA = CStr(Me.FixValue(reader, "MPAMonth", ""))
                sbMPA = New StringBuilder()
                For i As Integer = 0 To Math.Min(strMPA.Length, 12) - 1
                    ' Closed for fishing: store as 0, open: store as 1
                    'sbMPA.Append(CChar(if("Cc".IndexOf(strMPA(i)) >= 0, "0", "1")))
                    If "Cc".IndexOf(strMPA(i)) >= 0 Then
                        sbMPA.Append(CChar("0"))
                    Else
                        sbMPA.Append(CChar("1"))
                    End If
                Next
                drow("MPAMonth") = sbMPA.ToString()
                writer.AddRow(drow)

                ' Remember MPA ID mapping
                Me.HashKey(eDataTypes.EcospaceMPA, CStr(reader("MPANo")), eDataTypes.EcoSpaceScenario, iScenarioID) = nMPAID

                nMPAID += 1
            End While

            Me.m_dbEwE5.ReleaseReader(reader)
            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

        Private Sub ImportEcospaceGroups()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerSub As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim drowSub As DataRow = Nothing
            Dim iScenarioID As Integer = 1
            Dim iGroupID As Integer = 1
            Dim iEcopathGroupID As Integer = -1
            Dim strPreferredHabitats As String = ""
            Dim sValue As Single = 0.0!
            Dim iHabitatID As Integer = 1

            Dim dtEcopathGroups As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoPathGroupInput))
            Dim strEcopathGroup As String = ""
            Dim dtEcospaceScenarios As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoSpaceScenario))
            Dim strEcospaceScenario As String = ""
            ' Flag stating whether an ecospace group was found for a given ecopath group
            Dim bHasGroup As Boolean = False

            writer = Me.m_dbTarget.GetWriter("EcospaceScenarioGroup")
            writerSub = Me.m_dbTarget.GetWriter("EcospaceScenarioGroupHabitat")

            ' For every ecopath group...
            For Each strEcopathGroup In dtEcopathGroups.Keys
                ' and for every ecospace scenario
                For Each strEcospaceScenario In dtEcospaceScenarios.Keys

                    ' .. create a new ecospace group
                    iScenarioID = Me.HashKey(eDataTypes.EcoSpaceScenario, strEcospaceScenario)
                    iEcopathGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, strEcopathGroup)

                    ' Check if an ecospace group exists for this ecopath group, ecosim scenario combination
                    reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [EcoSpace N] where modelName='{0}' AND Scenario='{1}' AND groupName='{2}'", _
                            Me.m_strModelName, strEcospaceScenario, strEcopathGroup))

                    bHasGroup = reader.Read()

                    ' Create new row
                    drow = writer.NewRow()
                    drow("ScenarioID") = iScenarioID
                    drow("GroupID") = iGroupID
                    drow("EcopathGroupID") = iEcopathGroupID

                    ' Copy whatever data can be copied
                    If (bHasGroup) Then
                        ' JS 070213: Discontinued fields 'PrefHab0' to 'PrefHab7'
                        ' JS 061201: Discontinued. Field 'PrefHab' is imported incorrectly and not read in EwE5
                        ' drow("PrefHab") = Me.FixValue(reader, "PrefHab")
                        sValue = CSng(Me.FixValue(reader, "Mvel", 0.0!))
                        If (sValue = 0.0!) Then sValue = 300.0!
                        drow("Mvel") = sValue

                        sValue = CSng(Me.FixValue(reader, "RelMoveBad", 0.0!))
                        If (sValue = 0.0!) Then sValue = 2.0!
                        drow("RelMoveBad") = sValue

                        sValue = CSng(Me.FixValue(reader, "RelVulBad", 0.0!))
                        If (sValue = 0.0!) Then sValue = 2.0!
                        drow("RelVulBad") = sValue

                        sValue = CSng(Me.FixValue(reader, "EatEffBad", 0.0!))
                        If (sValue = 0.0!) Then sValue = 0.5!
                        drow("EatEffBad") = sValue

                        ' JS 070116: Discontinued. Field 'RiskSens' is imported in EwE5, but never used
                        ' drow("RiskSens") = Me.FixValue(reader, "RiskSens")
                        drow("IsAdvected") = Me.FixValue(reader, "IsAdvected")
                        drow("IsMigratory") = Me.FixValue(reader, "IsMigratory")
                        drow("MigConcRow") = Me.FixValue(reader, "MigConcRow")
                        drow("MigConcCol") = Me.FixValue(reader, "MigConcCol")
                        drow("PrefRow") = Me.RebuildNumberListString(CStr(Me.FixValue(reader, "PrefRow", "0")), CChar(" "), 5)
                        drow("PrefCol") = Me.RebuildNumberListString(CStr(Me.FixValue(reader, "PrefCol", "0")), CChar(" "), 5)
                        ' Set default capacity calculation type
                        drow("CapacityCalType") = eEcospaceCapacityCalType.Habitat
                    Else
                        ' #No: the new group will get all default values
                        Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_FIX_CREATEECOSPACEGROUP, _
                                iGroupID, _
                                strEcopathGroup, _
                                strEcospaceScenario), _
                                eMessageType.DataImport, eMessageImportance.Information)
                    End If

                    writer.AddRow(drow)
                    writer.Commit()

                    If (bHasGroup) Then

                        ' Import preferred habitats to subtable
                        ' In EwE5, preferred habitats were stored as a string of '0' and '1' values,
                        ' where a '1' indicates a group preference for the habitat whose index
                        ' matches the position of the '1'.
                        strPreferredHabitats = CStr(Me.FixValue(reader, "habitat", ""))
                        ' For all habitat preferences (habitat '0' can also be preferred!)
                        For iHabitatNo As Integer = 0 To strPreferredHabitats.Length - 1
                            ' Does this habitat index represent a preferred habitat?
                            If (strPreferredHabitats.Substring(iHabitatNo, 1) = "1") Then
                                ' #Yes: try to find a matching habitat ID 
                                iHabitatID = Me.HashKey(eDataTypes.EcospaceHabitat, CStr(iHabitatNo), eDataTypes.EcoSpaceScenario, iScenarioID)
                                ' Is habitat ID valid?
                                If (iHabitatID > 0) Then
                                    ' #Yes: Add a habitat preference for this group
                                    drowSub = writerSub.NewRow()
                                    drowSub("ScenarioID") = iScenarioID
                                    drowSub("GroupID") = iGroupID
                                    drowSub("HabitatID") = iHabitatID
                                    writerSub.AddRow(drowSub)
                                End If
                            End If
                        Next

                        ' Import Remarks
                        Me.AddRemark(Me.FixValue(reader, "Remark", ""), eDataTypes.EcospaceGroup, iGroupID, eVarNameFlags.Name)
                        ' References are discarded
                        ' ImportRefCode("RefCode", "quickRef")
                    End If

                    ' Remember ecospace group mapping
                    Me.HashKey(eDataTypes.EcospaceGroup, strEcopathGroup, eDataTypes.EcoSpaceScenario, iScenarioID) = iGroupID

                    ' Next group
                    Me.m_dbEwE5.ReleaseReader(reader)
                    iGroupID += 1

                Next strEcospaceScenario
            Next strEcopathGroup

            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbTarget.ReleaseWriter(writerSub)

        End Sub

        Private Sub ImportEcospaceFleets()

            Dim reader As IDataReader = Nothing
            Dim readerSub As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 1
            Dim strFlags As String = ""
            Dim astrPort As String() ' Port read buffer
            Dim astrSail As String() ' Sailing cost read buffer
            Dim iFleetID As Integer = 1
            Dim iHabitatID As Integer = 1
            Dim iMPAID As Integer = 1
            Dim nRows As Integer = 0
            Dim nCols As Integer = 0
            Dim iCell As Integer = 0
            Dim iFleet As Integer = 1

            ' Generate an Ecospace fleet entry for every Ecopath fleet
            Dim dtEcopathFleets As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.FleetInput))
            Dim strEcopathFleet As String = ""
            Dim dtEcospaceScenarios As Dictionary(Of String, Integer) = Me.m_adtKeys(CInt(eDataTypes.EcoSpaceScenario))
            Dim strEcospaceScenario As String = ""
            Dim bHasFleet As Boolean = False

            Dim dataPort As Boolean(,)
            Dim dataSailCost As Single(,)

            ' Get writers
            writer = Me.m_dbTarget.GetWriter("EcospaceScenarioFleet")

            If dtEcospaceScenarios IsNot Nothing And dtEcopathFleets IsNot Nothing Then

                ' For each ecospace scenario..
                For Each strEcospaceScenario In dtEcospaceScenarios.Keys

                    ' Get scenario map dimensions
                    reader = Me.m_dbTarget.GetReader("SELECT InRow, InCol FROM EcospaceScenario WHERE ScenarioID=" & dtEcospaceScenarios(strEcospaceScenario))
                    reader.Read()
                    nRows = CInt(reader("InRow"))
                    nCols = CInt(reader("InCol"))
                    Me.m_dbTarget.ReleaseReader(reader)

                    ReDim dataPort(nRows, nCols)
                    ReDim dataSailCost(nRows, nCols)

                    ' ..and each ecopath fleet
                    For Each strEcopathFleet In dtEcopathFleets.Keys

                        Me.LogProgress("Importing maps for scenario " & strEcospaceScenario & ", fleet " & strEcopathFleet)

                        ' Generate an Ecospace fleet entry
                        reader = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [EcoSpace Gear] WHERE modelName='{0}' AND Scenario='{1}' AND GearName='{2}'", _
                                                                  Me.m_strModelName, strEcospaceScenario, strEcopathFleet))
                        bHasFleet = reader.Read()

                        ' Create new row
                        drow = writer.NewRow()

                        iScenarioID = Me.HashKey(eDataTypes.EcoSpaceScenario, strEcospaceScenario)
                        drow("ScenarioID") = iScenarioID
                        drow("FleetID") = iFleetID
                        drow("EcopathFleetID") = Me.HashKey(eDataTypes.FleetInput, strEcopathFleet)
                        If (bHasFleet) Then
                            drow("EffPower") = Me.FixValue(reader, "EffPower")
                            ' JS 070119: discontinued in favour of finer-grained MPAFish, see below
                            ' drow("MPAfishery") = Me.FixValue(reader, "MPAFishery", "T")
                        End If

                        'If bHasFleet Then
                        astrPort = Me.SplitNumberListString(CStr(Me.FixValue(reader, "Port", "0")), CChar(" "), 1)
                        astrSail = Me.SplitNumberListString(CStr(Me.FixValue(reader, "Sail", "0")), CChar(" "), 6)

                        ' Port data found?
                        If (astrPort.Length = 0) Then
                            ' #No: Try to read port data from old table [EcoSpace GearxNxN]
                            ReDim astrPort(nRows * nCols)
                            For iRow As Integer = 0 To nRows : For iCol As Integer = 0 To nCols : astrPort(iRow * nCols + iCol) = "" : Next : Next
                            Try
                                readerSub = m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [EcoSpace GearxNxN] WHERE modelName='{0}' AND Scenario='{1}' AND GearName='{2}'", _
                                                                             Me.m_strModelName, strEcospaceScenario, strEcopathFleet))

                                If readerSub IsNot Nothing Then
                                    While readerSub.Read()
                                        Try
                                            astrPort((CInt(reader("InCol")) - 1) * nCols + (CInt(reader("InCol")) - 1)) = CStr(Me.FixValue(readerSub, "Port", "0"))
                                        Catch ex As Exception
                                            ' Swallow
                                        End Try
                                    End While
                                    Me.m_dbEwE5.ReleaseReader(readerSub)
                                    readerSub = Nothing
                                End If
                            Catch ex As Exception
                                ' Swallow
                            End Try
                        End If

                        iCell = 0
                        For iRow As Integer = 1 To nRows
                            For iCol As Integer = 1 To nCols

                                Dim bPort As Boolean = False
                                Dim sCost As Single = 0.0!

                                bPort = False
                                If (astrPort.Length > iCell) Then bPort = (astrPort(iCell) = "1")
                                If (astrSail.Length > iCell) Then
                                    sCost = cStringUtils.ConvertToSingle(astrSail(iCell), 0.0!)
                                End If

                                dataPort(iRow, iCol) = bPort
                                dataSailCost(iRow, iCol) = sCost

                                iCell += 1
                            Next
                        Next

                        drow("SailCostMap") = cStringUtils.ArrayToString(dataSailCost, nRows, nCols, Me.m_dicDepthMaps(iScenarioID), True)
                        drow("PortMap") = cStringUtils.ArrayToString(dataPort, nRows, nCols)

                        writer.AddRow(drow)
                        writer.Commit()

                        Dim writerHabFish As cEwEDatabase.cEwEDbWriter = Me.m_dbTarget.GetWriter("EcospaceScenarioHabitatFishery")
                        ' Write GearHab flag field to proper table combining (ScenarioID, FleetID, HabitatID)
                        strFlags = CStr(Me.FixValue(reader, "GearHab", ""))
                        ' For all habitats (including habitat '0')
                        For iHabitat As Integer = 0 To strFlags.Length - 1
                            If (strFlags.Substring(iHabitat, 1) = "1") Then
                                iHabitatID = Me.HashKey(eDataTypes.EcospaceHabitat, CStr(iHabitat), eDataTypes.EcoSpaceScenario, iScenarioID)
                                ' Is this a valid habitat?
                                If (iHabitatID > 0) Then
                                    drow = writerHabFish.NewRow()
                                    drow("ScenarioID") = iScenarioID
                                    drow("FleetID") = iFleetID
                                    drow("HabitatID") = iHabitatID
                                    writerHabFish.AddRow(drow)
                                End If
                            End If
                        Next iHabitat
                        Me.m_dbTarget.ReleaseWriter(writerHabFish)

                        Dim writerMPAFish As cEwEDatabase.cEwEDbWriter = Me.m_dbTarget.GetWriter("EcospaceScenarioMPAFishery")
                        ' Write MPAfish flag field to proper table combining (ScenarioID, FleetID, MPAID)
                        strFlags = CStr(Me.FixValue(reader, "MPAFish", ""))
                        ' For all MPAs
                        For iMPA As Integer = 1 To strFlags.Length
                            If (strFlags.Substring(iMPA - 1, 1) = "T") Then
                                iMPAID = Me.HashKey(eDataTypes.EcospaceMPA, CStr(iMPA), eDataTypes.EcoSpaceScenario, iScenarioID)
                                ' Is this a valid MPA?
                                If (iMPAID > 0) Then
                                    drow = writerMPAFish.NewRow()
                                    drow("ScenarioID") = iScenarioID
                                    drow("FleetID") = iFleetID
                                    drow("MPAID") = iMPAID
                                    writerMPAFish.AddRow(drow)
                                End If
                            End If
                        Next iMPA
                        Me.m_dbTarget.ReleaseWriter(writerMPAFish)

                        If bHasFleet Then
                            Me.AddRemark(reader("remark"), eDataTypes.EcospaceFleet, iFleetID, eVarNameFlags.Name)
                        Else
                            Me.LogMessage(cStringUtils.Localize(My.Resources.CoreMessages.IMPORT_FIX_CREATEECOSPACEFLEET, _
                                    iFleetID, _
                                    strEcopathFleet, _
                                    strEcospaceScenario), _
                                    eMessageType.DataImport, eMessageImportance.Information)
                        End If

                        ' JS 061221: References do not need to be imported for now
                        'ImportRefCode("RefCode", "quickRef")

                        ' Remember fleet ID mapping
                        Me.HashKey(eDataTypes.EcospaceFleet, strEcopathFleet, eDataTypes.EcoSpaceScenario, iScenarioID) = iFleetID

                        ' Next fleet
                        iFleetID += 1

                        Me.m_dbEwE5.ReleaseReader(reader)

                    Next strEcopathFleet
                Next strEcospaceScenario
            End If

            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

        Private Sub ImportEcospaceBasemap()

            Dim reader As IDataReader = Nothing
            Dim readerSub As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerSub As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim dtSub As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim strScenario As String = ""
            Dim iScenarioID As Integer = 1
            Dim astrDepth() As String = Nothing : Dim dataDepth(,) As Single
            Dim astrHabType() As String = Nothing : Dim dataHabitat(,) As Integer
            Dim astrRegion() As String = Nothing : Dim dataRegion(,) As Integer
            Dim astrMPA() As String = Nothing : Dim dataMPA(,) As Integer
            Dim astrRelPP() As String = Nothing : Dim dataRelPP(,) As Single
            Dim astrRelCin() As String = Nothing : Dim dataRelCin(,) As Single
            Dim iCell As Integer = 0
            Dim nRows As Integer = 0
            Dim nCols As Integer = 1
            Dim iNumRegions As Integer = 0

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * FROM [EcoSpace] WHERE modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcospaceScenario")
            dt = writer.GetDataTable()

            While reader.Read()

                strScenario = CStr(reader("Scenario"))
                iScenarioID = Me.HashKey(eDataTypes.EcoSpaceScenario, strScenario)
                nRows = CInt(reader("Inrow"))
                nCols = CInt(reader("Incol"))

                ReDim dataDepth(nRows, nCols)
                ReDim dataHabitat(nRows, nCols)
                ReDim dataRegion(nRows, nCols)
                ReDim dataMPA(nRows, nCols)
                ReDim dataRelPP(nRows, nCols)
                ReDim dataRelCin(nRows, nCols)

                ' Depth: 2 formats encountered: '#####' and '#### '
                astrDepth = SplitNumberListString(CStr(Me.FixValue(reader, "Depth", "0")), CChar(" "), 5)
                ' Habtype: 2 formats encountered: '###' and '## '
                astrHabType = SplitNumberListString(CStr(Me.FixValue(reader, "HabType", "0")), CChar(" "), 3)
                ' Region: no live data seen, but spec'ed as 3 digits in length in EwE5 sources
                astrRegion = SplitNumberListString(CStr(Me.FixValue(reader, "Region", "0")), CChar(" "), 3)
                ' MPA: 2 formats encountered, '##' and '# '
                astrMPA = SplitNumberListString(CStr(Me.FixValue(reader, "MPA", "0")), CChar(" "), 2)
                ' RelPP: 2 formats encountered, '#######' and '###### '
                astrRelPP = SplitNumberListString(CStr(Me.FixValue(reader, "RelPP", "1.0")), CChar(" "), 7)
                ' RelCin: no live data encountered, but spec'ed as 7 digits in length in EwE5 sources
                astrRelCin = SplitNumberListString(CStr(Me.FixValue(reader, "RelCin", "1.0")), CChar(" "), 7)

                ' Reset cell counter
                iCell = 0

                For iRow As Integer = 1 To nRows
                    For iCol As Integer = 1 To nCols

                        ' Copy depth value
                        If astrDepth.Length > iCell Then
                            dataDepth(iRow, iCol) = cStringUtils.ConvertToInteger(astrDepth(iCell))
                        End If

                        If astrHabType.Length > iCell Then
                            dataHabitat(iRow, iCol) = cStringUtils.ConvertToInteger(astrHabType(iCell), 0)
                        End If

                        If astrRegion.Length > iCell Then
                            dataRegion(iRow, iCol) = cStringUtils.ConvertToInteger(astrRegion(iCell), 0)
                            iNumRegions = Math.Max(iNumRegions, dataRegion(iRow, iCol))
                        End If

                        If astrMPA.Length > iCell Then
                            dataMPA(iRow, iCol) = cStringUtils.ConvertToInteger(astrMPA(iCell), 0)
                        End If

                        If astrRelPP.Length > iCell Then
                            dataRelPP(iRow, iCol) = cStringUtils.ConvertToSingle(astrRelPP(iCell), 1.0!)
                        End If

                        If astrRelCin.Length > iCell Then
                            dataRelCin(iRow, iCol) = cStringUtils.ConvertToSingle(astrRelCin(iCell), 1.0!)
                        End If

                        ' Next cell
                        iCell += 1

                    Next iCol
                Next iRow

                drow = dt.Rows.Find(iScenarioID)
                drow.BeginEdit()
                drow("DepthMap") = cStringUtils.ArrayToString(dataDepth, nRows, nCols)
                drow("RelPPMap") = cStringUtils.ArrayToString(dataRelPP, nRows, nCols, dataDepth)
                drow("RelCinMap") = cStringUtils.ArrayToString(dataRelCin, nRows, nCols, dataDepth)
                drow("DepthAMap") = ""
                drow("RegionMap") = cStringUtils.ArrayToString(dataRegion, nRows, nCols, dataDepth)
                drow("NumRegions") = iNumRegions
                drow.EndEdit()

                Me.m_dicDepthMaps(iScenarioID) = dataDepth

                Dim keys As Object() = {iScenarioID, 0}
                Dim iKey As Integer = 0

                ' Habitats
                readerSub = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT HabitatNo FROM [Ecospace habitats] WHERE modelName='{0}' AND Scenario='{1}'", Me.m_strModelName, strScenario))
                writerSub = Me.m_dbTarget.GetWriter("EcospaceScenarioHabitat")
                dtSub = writerSub.GetDataTable()
                While readerSub.Read()
                    iCell = CInt(readerSub("HabitatNo"))
                    iKey = Me.HashKey(eDataTypes.EcospaceHabitat, CStr(iCell), eDataTypes.EcoSpaceScenario, iScenarioID)
                    keys(1) = iKey
                    drow = dtSub.Rows.Find(keys)
                    drow.BeginEdit()
                    drow("HabitatMap") = cStringUtils.ArrayToString(dataHabitat, nRows, nCols, dataDepth, True, iCell, 1.0!)
                    drow.EndEdit()
                End While
                dtSub = Nothing
                Me.m_dbEwE5.ReleaseReader(readerSub)
                Me.m_dbTarget.ReleaseWriter(writerSub)

                ' MPAs
                readerSub = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT MPANo FROM [EcoSpace MPA] WHERE modelName='{0}' AND Scenario='{1}'", Me.m_strModelName, strScenario))
                writerSub = Me.m_dbTarget.GetWriter("EcospaceScenarioMPA")
                dtSub = writerSub.GetDataTable()
                While readerSub.Read()
                    iCell = CInt(readerSub("MPANo"))
                    iKey = Me.HashKey(eDataTypes.EcospaceMPA, CStr(iCell), eDataTypes.EcoSpaceScenario, iScenarioID)
                    keys(1) = iKey
                    drow = dtSub.Rows.Find(keys)
                    drow.BeginEdit()
                    drow("MPAMap") = cStringUtils.ArrayToString(dataMPA, nRows, nCols, dataDepth, True, iCell)
                    drow.EndEdit()
                End While
                dtSub = Nothing
                Me.m_dbEwE5.ReleaseReader(readerSub)
                Me.m_dbTarget.ReleaseWriter(writerSub)

            End While

            Me.m_dbEwE5.ReleaseReader(reader)
            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

#End Region ' Ecospace

#Region " Ecotracer "

        Private Function ImportEcotracer() As Boolean

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 1

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [EcoTracer] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return False

            writer = Me.m_dbTarget.GetWriter("EcotracerScenario")

            While reader.Read()

                drow = writer.NewRow()

                drow("ScenarioID") = iScenarioID
                drow("ScenarioName") = Me.FixValue(reader, "Scenario")
                drow("Czero") = Me.FixValue(reader, "cZero", 0.0!)
                drow("Cinflow") = Me.FixValue(reader, "Cinflow", 0.0!)
                drow("Coutflow") = Me.FixValue(reader, "Coutflow", 0.0!)
                drow("Cdecay") = Me.FixValue(reader, "Cdecay", 0.0!)
                drow("LastSaved") = Me.ExtractLastSavedJulianDate(CStr(Me.FixValue(reader, "remarks", "")))

                ' Remember Ecotracer scenario ID mapping
                Me.HashKey(eDataTypes.EcotracerScenario, CStr(reader("Scenario"))) = iScenarioID

                writer.AddRow(drow)

                ' Map remarks
                Me.AddRemark(reader("remarks"), eDataTypes.EcotracerScenario, iScenarioID, eVarNameFlags.Name)

                ' JS 071124: References do not need to be imported for now
                'ImportRefCode("RefCode", "quickRefCatch")

                iScenarioID += 1

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

            Return True

        End Function

        Private Sub ImportEcotracerN()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 0
            Dim iGroupID As Integer = 0

            reader = Me.m_dbEwE5.GetReader(cStringUtils.Localize("SELECT * from [EcoTracer N] where modelName='{0}'", Me.m_strModelName))
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("EcotracerScenarioGroup")

            While reader.Read()

                ' Get tracer scenario ID
                iScenarioID = Me.HashKey(eDataTypes.EcotracerScenario, CStr(reader("Scenario")))
                ' Get group ID
                iGroupID = Me.HashKey(eDataTypes.EcoPathGroupInput, CStr(reader("groupName")))

                drow = writer.NewRow()

                drow("ScenarioID") = iScenarioID
                drow("EcopathGroupID") = iGroupID
                drow("CZero") = Me.FixValue(reader, "cZero", 0.0!)
                drow("Cimmig") = Me.FixValue(reader, "Cimmig", 0.0!)
                drow("Cenv") = Me.FixValue(reader, "Cenv", 0.0!)
                drow("Cdecay") = Me.FixValue(reader, "Cdecay", 0.0!)

                writer.AddRow(drow)

                ' No remarks or references to map

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

#End Region ' Ecotracer

#Region " Quotes "

        Private Sub ImportQuotes()

            Dim reader As IDataReader = Nothing
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = 0
            Dim iGroupID As Integer = 0

            reader = Me.m_dbEwE5.GetReader("SELECT * from Quote")
            If reader Is Nothing Then Return

            writer = Me.m_dbTarget.GetWriter("Quote")

            While reader.Read()

                drow = writer.NewRow()

                drow("Quote") = reader("Quote")
                drow("Source") = reader("Source")

                writer.AddRow(drow)

            End While

            ' Clean up, store changes
            Me.m_dbTarget.ReleaseWriter(writer)
            Me.m_dbEwE5.ReleaseReader(reader)

        End Sub

#End Region ' Quotes

#End Region ' Implementation 

#Region " Auxillary data "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a remark to the Auxillary data table
        ''' </summary>
        ''' <param name="objRemark">Remark text, may be DBNull</param>
        ''' <param name="dataType">The <see cref="eDataTypes">Core data type</see> 
        ''' representing the object to store the remark for.</param>
        ''' <param name="nID">The database ID of <paramref name="dataType">dataType</paramref>
        ''' to store the remark for.</param>
        ''' <param name="varName">The <see cref="eVarNameFlags">Core variable name</see>
        ''' to store the remark for.</param>
        ''' <param name="dataTypeSec">The <see cref="eDataTypes">Core data type</see> 
        ''' representing the secundary object (or index) to store the remark for.</param>
        ''' <param name="nIDSec">The database ID of <paramref name="dataTypeSec">dataTypeSec</paramref>.</param>
        ''' <remarks>
        ''' <para>All imported remarks should bear a relationship to an existing 
        ''' core object instance, variable type an optional subgroup.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Sub AddRemark(ByVal objRemark As Object, _
                ByVal dataType As eDataTypes, ByVal nID As Integer, _
                ByVal varName As eVarNameFlags, _
                Optional ByVal dataTypeSec As eDataTypes = eDataTypes.NotSet, Optional ByVal nIDSec As Integer = -1)

            Dim strRemark As String = ""

            ' No data? Abort
            If (objRemark Is Nothing) Then Return
            ' No data? Abort
            If Convert.IsDBNull(objRemark) Then Return
            ' Convert
            strRemark = objRemark.ToString().Trim()
            ' Still add?
            If String.IsNullOrEmpty(strRemark) Then Return
            ' Add
            Me.AddAuxillaryData(strRemark, dataType, nID, varName, dataTypeSec, nIDSec)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a pedigree indicator to the Auxillary data table
        ''' </summary>
        ''' <param name="iPedigree">Pedigree sequence number, may be cCore.NULL_VALUE</param>
        ''' <param name="iGroupID">The Group ID to store pedigree for.</param>
        ''' <param name="varName">The <see cref="eVarNameFlags">Core variable name</see>
        ''' to store pedigree for.</param>
        ''' -------------------------------------------------------------------
        Private Sub AddPedigree(ByVal writer As cEwEDatabase.cEwEDbWriter, _
                                ByVal iPedigree As Integer, _
                                ByVal iGroupID As Integer, _
                                ByVal varName As eVarNameFlags)

            ' Find pedigree levels for a variable
            Dim drow As DataRow = Nothing
            Dim lPedigreeLevelIDs As List(Of Integer) = Me.m_dicPedigreeLevels(varName)
            Dim iPedigreeLevelID As Integer = 0

            ' Abort if invalid
            If (iPedigree < 0 Or iPedigree >= lPedigreeLevelIDs.Count) Then Return

            iPedigreeLevelID = lPedigreeLevelIDs(iPedigree)
            If (iPedigreeLevelID = 0) Then Return

            drow = writer.NewRow()
            drow("GroupID") = iGroupID
            drow("LevelID") = iPedigreeLevelID
            drow("VarName") = varName.ToString()
            writer.AddRow(drow)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a record to the Auxillary data table
        ''' </summary>
        ''' <param name="strRemark">Remark text to add.</param>
        ''' <param name="dataType">The <see cref="eDataTypes">Core data type</see> 
        ''' representing the object to store the remark for.</param>
        ''' <param name="nID">The database ID of <paramref name="dataType">dataType</paramref>
        ''' to store the remark for.</param>
        ''' <param name="varName">The <see cref="eVarNameFlags">Core variable name</see>
        ''' to store the remark for.</param>
        ''' <param name="dataTypeSec">The <see cref="eDataTypes">Core data type</see> 
        ''' representing the secundary object (or index) to store the remark for.</param>
        ''' <param name="nIDSec">The database ID of <paramref name="dataTypeSec">dataTypeSec</paramref>.</param>
        ''' <remarks>
        ''' <para>All imported remarks should bear a relationship to an existing 
        ''' core object instance, variable type an optional subgroup.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Sub AddAuxillaryData(ByVal strRemark As String, _
                                     ByVal dataType As eDataTypes, ByVal nID As Integer, _
                                     ByVal varName As eVarNameFlags, _
                                     ByVal dataTypeSec As eDataTypes, _
                                     ByVal nIDSec As Integer)

            Dim writer As cEwEDatabase.cEwEDbWriter = Me.m_dbTarget.GetWriter("Auxillary")
            Dim dt As DataTable = Nothing
            Dim key As New cValueID(dataType, nID, varName, dataTypeSec, nIDSec)
            Dim strValueID As String = key.ToString()
            Dim drow As DataRow = Nothing
            Dim iRow As Integer = 0
            Dim bRowFound As Boolean = False

            ' Both null? Abort!
            If String.IsNullOrEmpty(strRemark) Then Return

            ' Sanity check
            Debug.Assert(dataType > eDataTypes.NotSet And nID > 0, "Auxillary data cannot be added without a valid object identifier")

            ' Find existing row
            dt = writer.GetDataTable()
            While (iRow < dt.Rows.Count) And (Not bRowFound)
                drow = dt.Rows(iRow)
                bRowFound = (String.Compare(strValueID, CStr(drow("ValueID"))) = 0)
                iRow += 1
            End While

            If (Not bRowFound) Then

                ' Create new row
                drow = writer.NewRow()
                drow("DBID") = Me.m_iNextAuxID
                drow("ValueID") = strValueID
                Me.m_iNextAuxID += 1

            Else

                ' Try to complete values
                If String.IsNullOrEmpty(strRemark) Then strRemark = CStr(drow("Remark"))

                ' Start editing existing row
                drow.BeginEdit()

            End If

            ' Store remark text
            drow("Remark") = strRemark
            ' The other thing...
            drow("VisualStyle") = ""

            If Not bRowFound Then
                ' Add new row 
                writer.AddRow(drow)
            Else
                ' Update exsting row
                drow.EndEdit()
            End If

            writer.Commit()
            Me.m_dbTarget.ReleaseWriter(writer)

        End Sub

#End Region ' Auxillary data

#Region " Local utilities "

        Private Function FixColor(ByVal iColor As Integer) As String

            Dim clrTemp As Color = Color.FromArgb(iColor)
            Dim a As Byte = clrTemp.A
            Dim r As Byte = clrTemp.R
            Dim g As Byte = clrTemp.G
            Dim b As Byte = clrTemp.B

            ' Alpha is not set in EwE5 - remove opacity when any color is present
            If (r > 0) Or (g > 0) Or (b > 0) Then a = 255

            Return cStringUtils.Localize("{0:x2}{1:x2}{2:x2}{3:x2}", a, r, g, b)

        End Function

#End Region ' utilities

    End Class

End Namespace
