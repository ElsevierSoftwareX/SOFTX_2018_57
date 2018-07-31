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
Imports EwECore
Imports EwECore.Ecosim
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Utilities
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' The perturb engine that does all the work: modifies forcing functions, runs 
''' Ecosim, and organizes extraction of results.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class cEngine
    Inherits cThreadWaitBase

#Region " Private helper classes "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper item to cache the data of a forcing function.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cFFCache

        Private m_ff As cForcingFunction = Nothing
        Private m_asData As Single() = Nothing
        Private m_bIsChanged As Boolean = False

        Public Sub New(ff As cForcingFunction)
            Me.m_ff = ff
            Me.m_asData = ff.ShapeData
        End Sub

        Public Sub Restore()

            If Not Me.m_bIsChanged Then Return

            Me.m_ff.LockUpdates()
            For i As Integer = 0 To Me.m_ff.ShapeData.Length - 1
                Me.m_ff.ShapeData(i) = Me.m_asData(i)
            Next
            Me.m_ff.UnlockUpdates(False)

        End Sub

        ''' <summary>
        ''' Restore shape data to its original state.
        ''' </summary>
        Public Sub StartEdit()

            Me.m_ff.LockUpdates()
            For i As Integer = 0 To Me.m_ff.ShapeData.Length - 1
                Me.m_ff.ShapeData(i) = Me.m_asData(i)
            Next
            Me.m_bIsChanged = False

        End Sub

        Public Sub EndEdit()

            Me.m_ff.UnlockUpdates(False)

        End Sub

        Public Sub SetData(i As Integer, s As Single)

            If (i < Me.m_ff.ShapeData.Length) Then
                If (s <> Me.m_ff.ShapeData(i)) Then
                    Me.m_ff.ShapeData(i) = s
                    Me.m_bIsChanged = True
                End If
            End If

        End Sub

        Public Function BelongsTo(man As cBaseShapeManager) As Boolean
            Return man.Contains(Me.m_ff)
        End Function

        Public Function IsChanged() As Boolean
            Return Me.m_bIsChanged
        End Function

        Public Sub Update()
            Me.m_ff.LockUpdates()
            Me.m_ff.Update()
            Me.m_ff.UnlockUpdates(bUpdate:=False)
        End Sub

    End Class

#End Region ' Private helper classes

#Region " Privates "

    Private m_uic As cUIContext = Nothing
    Private m_core As cCore = Nothing
    Private m_lManagers As New List(Of cBaseShapeManager)
    Private m_types As eFunctionTypes = eFunctionTypes.Forcing

    Private m_astrFiles As String()
    Private m_strOutFolder As String = ""
    Private m_bReadMonthly As Boolean = False
    Private m_options As cEcosimResultWriter.eResultTypes() = Nothing
    Private m_FFCache As New Dictionary(Of String, cFFCache)

    Private m_dgtProgress As cEngine.RunProgressDelegate = Nothing
    Private m_dgtComplete As cEngine.RunCompletedDelegate = Nothing
    Private m_dgtDisableFile As DisableFileDelegate = Nothing
    Private m_bStopRun As Boolean = False

    ' -- progress 
    Private m_iNumSteps As Integer
    Private m_iStep As Integer

    Private m_valDetails As New List(Of cVariableStatus)
    Private m_valStatus As eStatusFlags = eStatusFlags.OK

    Private m_log As cMultiSimLog = Nothing

#End Region ' Privates

#Region " Public bits "

    ''' <summary>
    ''' Bit flags of forcing types to include
    ''' </summary>
    <Flags()>
    Public Enum eFunctionTypes As Byte
        Forcing = 0
        Effort = 1
        Mortality = 2
        Eggsies = 4
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create the engine.
    ''' </summary>
    ''' <param name="uic">The UI context to operate onto.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext)

        Me.m_uic = uic
        Me.m_core = uic.Core
        Me.m_log = New cMultiSimLog(Me.m_core, Me)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the run creates a new folder for every run.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property CreateUniqueRunFolder As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get current run progress.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Progress As Single
        Get
            If Not Me.IsRunning Then Return 0
            Return CSng(Me.m_iStep / Math.Max(1, Me.m_iNumSteps))
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dgtComplete"></param>
    ''' <param name="astrFiles"></param>
    ''' <param name="strOutFolder">The output folder to write a log file to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub ValidateFiles(ByVal dgtComplete As RunCompletedDelegate,
                             ByVal dgtDisableFile As DisableFileDelegate,
                             ByVal astrFiles As String(),
                             ByVal strOutFolder As String,
                             ByVal types As eFunctionTypes)

        If (Me.IsRunning) Then Return

        Me.m_strOutFolder = strOutFolder
        Me.m_astrFiles = astrFiles
        Me.m_types = types

        Me.m_dgtProgress = Nothing
        Me.m_dgtComplete = dgtComplete
        Me.m_dgtDisableFile = dgtDisableFile

        If Not cFileUtils.IsDirectoryAvailable(Me.m_strOutFolder, True) Then
            ' ToDo: panic
            Return
        End If

        Me.SetWait()

        Try
            Dim thrd As New Threading.Thread(AddressOf ValidateFilesThreaded)
            thrd.Start()
        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

    Public Delegate Sub RunProgressDelegate(strMessage As String)
    Public Delegate Sub RunCompletedDelegate()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run!
    ''' </summary>
    ''' <param name="dgtComplete">Delegate to call when the run has finished.</param>
    ''' <param name="astrFiles">The files to read and apply.</param>
    ''' <param name="strOutFolder">Output folder.</param>
    ''' <param name="bReadMonthly">States whether files should be read as monthly (true) or annual (false) values.
    ''' annual values (<see cref="TriState.[False]"/>), or in both modes (<see cref="TriState.UseDefault"/>).</param>
    ''' <param name="options"><see cref="cEcosimResultWriter.eResultTypes">Output options</see>.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Run(ByVal dgtProgress As RunProgressDelegate,
                   ByVal dgtComplete As RunCompletedDelegate,
                   ByVal astrFiles As String(),
                   ByVal strOutFolder As String,
                   ByVal types As eFunctionTypes,
                   ByVal bReadMonthly As Boolean,
                   ByVal options As cEcosimResultWriter.eResultTypes())

        If (Me.IsRunning) Then Return
        If (Not Me.m_core.SaveChanges(False, cCore.eBatchChangeLevelFlags.Ecosim)) Then Return

        Dim strDate As String = Date.Now.ToString("yy-MM-dd hh-mm")
        Dim strScope As String = If(bReadMonthly, "monthly", "annual")

        Me.m_bReadMonthly = bReadMonthly
        Me.m_astrFiles = astrFiles
        Me.m_types = types
        Me.m_options = options
        Me.m_iStep = 1
        Me.m_iNumSteps = Me.m_astrFiles.Length

        If Me.CreateUniqueRunFolder Then
            Me.m_strOutFolder = Path.Combine(strOutFolder, cFileUtils.ToValidFileName(cStringUtils.Localize("Run {0} {1}", strDate, strScope), False))
        Else
            Me.m_strOutFolder = strOutFolder
        End If

        Me.m_dgtProgress = dgtProgress
        Me.m_dgtComplete = dgtComplete
        Me.SetWait()

        Try
            Dim thrd As New Threading.Thread(AddressOf RunThreaded)
            thrd.Start()
        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stop a run.
    ''' </summary>
    ''' <param name="WaitTimeInMillSec"></param>
    ''' <returns>Always true. Why not?!</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function StopRun(Optional WaitTimeInMillSec As Integer = -1) As Boolean
        Me.m_bStopRun = True
        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Generate sample MultiSim template file.
    ''' </summary>
    ''' <param name="types">The FF types to include in the sample.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GenerateSample(ByVal types As eFunctionTypes, bMonthly As Boolean, ByRef strFileSample As String) As Boolean

        Dim scenario As cEwEScenario = Me.m_core.EcosimScenarios(Me.m_core.ActiveEcosimScenarioIndex)
        Dim strOutFolder As String = Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecosim)
        Dim lShapes As New List(Of cForcingFunction)
        Dim sw As StreamWriter = Nothing
        Dim i As Integer = 0
        Dim strError As String = ""
        Dim bSuccess As Boolean = True

        Me.m_types = types

        Me.ClearStatus()
        Me.BuildFFNameCache(True)

        If Not cFileUtils.IsDirectoryAvailable(strOutFolder, True) Then Return False

        Try
            strFileSample = Path.Combine(strOutFolder, "multisim_sample.csv")
            sw = New StreamWriter(strFileSample)

            For Each man As cBaseShapeManager In Me.m_lManagers
                For Each ff As cForcingFunction In man
                    If Not Me.IsAllFleet(man, ff) Then
                        lShapes.Add(ff)
                        If (i > 0) Then sw.Write(",")
                        sw.Write(cStringUtils.ToCSVField(ff.Name))
                        i += 1
                    End If
                Next
            Next
            sw.WriteLine()

            i = 1
            If (bMonthly) Then
                While i <= Me.m_core.nEcosimTimeSteps
                    For j As Integer = 0 To lShapes.Count - 1
                        If (j > 0) Then sw.Write(",")
                        sw.Write(cStringUtils.ToCSVField(lShapes(j).ShapeData(i)))
                    Next
                    sw.WriteLine()
                    i += 1
                End While
            Else
                While i < Me.m_core.nEcosimYears
                    For j As Integer = 0 To lShapes.Count - 1
                        If (j > 0) Then sw.Write(",")
                        sw.Write(cStringUtils.ToCSVField(lShapes(j).ShapeData(i * cCore.N_MONTHS)))
                    Next
                    sw.WriteLine()
                    i += 1
                End While
            End If

            sw.Close()

        Catch ex As Exception
            Me.m_valStatus = Me.m_valStatus Or eStatusFlags.ErrorEncountered
            strError = cStringUtils.Localize(My.Resources.EXAMPLE_EXPORT_FAILED, strFileSample, ex.Message)
            strOutFolder = ""
            bSuccess = False
        End Try

        Me.BroadcastStatus(cStringUtils.Localize(My.Resources.EXAMPLE_EXPORT_SUCCESS, strFileSample),
                           cStringUtils.Localize(My.Resources.EXAMPLE_EXPORT_WARNING, strFileSample, scenario.Name),
                           strError, strOutFolder)

        Return bSuccess

    End Function

#End Region ' Public bits

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Build the cache of forcing function names (lower-case).
    ''' </summary>
    ''' <param name="bCheckDuplicates">Flag to check for duplicate function names.</param>
    ''' <returns>True if no duplicates found.</returns>
    ''' -----------------------------------------------------------------------
    Private Function BuildFFNameCache(ByVal bCheckDuplicates As Boolean) As Boolean

        Dim strKey As String = ""
        Dim dupl As cFFCache = Nothing

        Me.m_FFCache.Clear()
        Me.m_lManagers.Clear()

        ' Build manager list. The order is important here for applications!
        ' First forcing and eggs. Then Fmortality, and effort last
        ' Using effort and Fmortality together is not recommended, as Fmort is derived from effort.

        Me.m_lManagers.Add(Me.m_core.ForcingShapeManager)
        If ((Me.m_types And eFunctionTypes.Eggsies) = eFunctionTypes.Eggsies) Then
            Me.m_lManagers.Add(Me.m_core.EggProdShapeManager)
        End If
        If ((Me.m_types And eFunctionTypes.Mortality) = eFunctionTypes.Mortality) Then
            Me.m_lManagers.Add(Me.m_core.FishMortShapeManager)
        End If
        If ((Me.m_types And eFunctionTypes.Effort) = eFunctionTypes.Effort) Then
            Me.m_lManagers.Add(Me.m_core.FishingEffortShapeManager)
        End If

        ' Explore all managers
        For i As Integer = 0 To Me.m_lManagers.Count - 1
            Dim man As cBaseShapeManager = Me.m_lManagers(i)

            ' Explore all functions
            For j As Integer = 0 To man.Count - 1
                Dim ff As cForcingFunction = man(j)
                strKey = Key(ff.Name)

                If (bCheckDuplicates And Me.m_FFCache.ContainsKey(strKey)) Then

                    dupl = Me.m_FFCache(strKey)

                    ' Add error
                    Me.m_valDetails.Add(New cVariableStatus(eStatusFlags.FailedValidation,
                                                                cStringUtils.Localize(My.Resources.VALIDATION_DETAIL_FN_DUPLICATE, ff.Name),
                                                                eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0))
                    Me.m_valStatus = Me.m_valStatus Or eStatusFlags.FailedValidation
                End If

                ' Add function
                Me.m_FFCache(strKey) = New cFFCache(ff)
            Next
        Next

        ' Return whether there are no errors
        Return ((Me.m_valStatus And eStatusFlags.ErrorEncountered) = 0)

    End Function

    Private Function Key(strName As String) As String
        Return strName.Trim().ToLowerInvariant()
    End Function

    Private Function IsAllFleet(man As cBaseShapeManager, ff As cForcingFunction) As Boolean

        If ((TypeOf man Is cFishingEffortShapeManger) Or (TypeOf man Is cFishingMortalityShapeManger)) Then
            Return ff.Index > Me.m_core.nFleets
        End If
        Return False

    End Function

    Private Function IsAllFleetName(man As cBaseShapeManager, strName As String) As Boolean

        If ((TypeOf man Is cFishingEffortShapeManger) Or (TypeOf man Is cFishingMortalityShapeManger)) Then
            Dim ff As cForcingFunction = man.Item(Me.m_core.nFleets)
            Return String.Compare(strName, ff.Name, True) = 0
        End If
        Return False

    End Function

#Region " Running "

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strFileName"></param>
    Private Sub ReadCSVIntoFF(ByVal strFileName As String)

        Dim reader As StreamReader = Nothing
        Dim values() As String = Nothing
        Dim strKey As String = ""
        Dim month As Integer = 0
        Dim value As Single = 0.0!
        Dim lff As New List(Of cFFCache)
        Dim iRepetitions As Integer = 0

        If Me.m_bReadMonthly Then iRepetitions = 1 Else iRepetitions = cCore.N_MONTHS

        If File.Exists(strFileName) Then
            Me.m_log.Add("- Opened file " & strFileName & "OK")

            ' Prevent 'our' FFs from updating prematurely while reading CSV file
            ' This also resets the FFs to their original values before the MultiSim run to make sure this iteration starts afresh
            For Each ffc As cFFCache In m_FFCache.Values
                ffc.StartEdit()
            Next

            ' (4) Open the CSV file for reading
            reader = New StreamReader(strFileName) 'read in csv files x1,x2,x3 etc

            ' First line holds FF names
            values = cStringUtils.SplitQualified(reader.ReadLine(), ","c)

            ' Map to FF Cache items
            For i As Integer = 0 To values.Length - 1
                strKey = Me.Key(values(i))
                If Me.m_FFCache.ContainsKey(strKey) Then
                    lff.Add(Me.m_FFCache(strKey))
                    m_log.Add("- Using function " & values(i))
                Else
                    lff.Add(Nothing)
                    m_log.Add("- Skipping function " & values(i) & ", function not found")
                End If
            Next

            ' Check if the end of the file is not reached, the peek would return 0 if at the end of the file
            'From While to end While is a loop that runs if certain conditions are true (as long as there are characters to read left in the file.
            While reader.Peek() > 0

                'split the line into individual values (seperated by commas)
                values = cStringUtils.SplitQualified(reader.ReadLine, ","c)

                For j As Integer = 1 To iRepetitions

                    'month from above +1 for the output of months and also to input for forcing functions
                    month = month + 1

                    For i As Integer = 0 To Math.Min(values.Length, lff.Count) - 1

                        If lff(i) IsNot Nothing Then
                            ' By default, do not force a value
                            value = 0.0
                            ' Is a value?
                            If Not String.IsNullOrWhiteSpace(values(i)) Then
                                ' Try to convert this value and set it into the FF
                                If Not Single.TryParse(values(i), value) Then
                                    Debug.Assert(False, "Value '" & values(i) & "' unreadable, a number was expected")
                                    ' ToDo: log error 
                                End If
                            End If

                            ' Set value into FF for a given month
                            lff(i).SetData(month, value)
                        End If
                    Next
                Next

            End While

            ' CSV has been read, now release the update lock on FFs and apply the content of FFs to Ecosim
            For Each ffc As cFFCache In m_FFCache.Values
                ffc.EndEdit()
            Next

            ' Commit shape changes to the core in a logical order that ensures that fishing mortality is correctly derived from effort
            Me.CommitShapes()

        Else
            Me.m_log.Add("- Unable to open file " & strFileName)
        End If

        ' Close reader to release the csv file
        reader.Close()

    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    Private Sub RunThreaded()

        Me.m_bStopRun = False

        'Me.m_core.SetBatchLock(cCore.eBatchLockType.Update)
        'Me.m_core.SetStopRunDelegate(AddressOf StopRun)
        cApplicationStatusNotifier.StartProgress(Me.m_core, My.Resources.STATUS_INITIALIZING, -1)

        Dim scenario As cEcoSimScenario = Me.m_core.EcosimScenarios(Me.m_core.ActiveEcosimScenarioIndex)

        Me.m_log.Open()

        ' Abort if an error occurred!
        If Not Me.BuildFFNameCache(False) Then
            Dim msg As New cMessage(My.Resources.VALIDATION_RESULT_FAILED, eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
            For Each vs As cVariableStatus In Me.m_valDetails
                msg.AddVariable(vs)
            Next
            Me.m_core.Messages.SendMessage(msg)
            Me.m_log.Add(msg)
            Return
        End If

        Try
            Dim iNum As Integer = Me.m_astrFiles.Length
            Dim i As Integer = 0

            While i < iNum And Not Me.m_bStopRun

                Dim strFile As String = Me.m_astrFiles(i)
                Dim strFileShort As String = Path.GetFileNameWithoutExtension(strFile)
                Dim strFolder As String = Path.Combine(Me.m_strOutFolder, strFileShort)

                If Not cFileUtils.IsDirectoryAvailable(strFolder, True) Then
                    Me.m_bStopRun = True
                End If

                If Not Me.m_bStopRun Then
                    Me.m_core.SetStopRunDelegate(AddressOf StopRun)
                    cApplicationStatusNotifier.UpdateProgress(Me.m_core, cStringUtils.Localize(My.Resources.STATUS_LOADING, strFileShort), CSng((1 + i * 4) / (iNum * 4)))
                    m_log.Add("Reading CSV file " & strFile)
                    Me.ReadCSVIntoFF(strFile)
                    m_log.Add("- Done")
                End If

                If Not Me.m_bStopRun Then
                    Me.m_core.SetStopRunDelegate(AddressOf StopRun)
                    cApplicationStatusNotifier.UpdateProgress(Me.m_core, cStringUtils.Localize(My.Resources.STATUS_RUNNING, strFileShort), CSng((2 + i * 4) / (iNum * 4)))
                    m_log.Add("Running Ecosim scenario " & scenario.Name & ":")
                    Me.m_core.RunEcoSim(Nothing, False)
                    m_log.Add("- Done")
                End If

                If Not Me.m_bStopRun Then
                    Me.m_core.SetStopRunDelegate(AddressOf StopRun)
                    cApplicationStatusNotifier.UpdateProgress(Me.m_core, cStringUtils.Localize(My.Resources.STATUS_SAVING, strFileShort), CSng((3 + i * 4) / (iNum * 4)))
                    m_log.Add("Writing MultiSim results to " & strFolder & ":")
                    Me.WriteResults(strFolder, strFile, Me.m_options)
                    m_log.Add("- Done")
                End If

                i += 1

                'Threading.Thread.Sleep(10000)

            End While
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        cApplicationStatusNotifier.UpdateProgress(Me.m_core, My.Resources.STATUS_RESTORING, -1)
        For Each ffc As cFFCache In Me.m_FFCache.Values
            ffc.Restore()
        Next

        ' Commit shape changes to the core in a logical order that ensures that fishing mortality is correctly derived from effort
        Me.CommitShapes()

        Me.m_core.DiscardChanges()
        GC.Collect()

        'Me.m_core.SetStopRunDelegate(Nothing)
        'Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)
        cApplicationStatusNotifier.EndProgress(Me.m_core)
        Me.m_log.Close()

        '= agk ='
        Me.ReleaseWait()
        If (Me.m_dgtComplete IsNot Nothing) Then Me.m_dgtComplete.Invoke()

    End Sub

    Public ReadOnly Property OutputPath As String
        Get
            Return Me.m_strOutFolder
        End Get
    End Property

#End Region ' Running

#Region " File validation "

    Public Delegate Sub DisableFileDelegate(strFile As String)

    ''' <summary>
    ''' 
    ''' </summary>
    Private Sub ValidateFilesThreaded()

        Dim scenario As cEwEScenario = Me.m_core.EcosimScenarios(Me.m_core.ActiveEcosimScenarioIndex)
        Dim msg As cMessage = Nothing

        Me.ClearStatus()
        Me.m_log.Open()

        Me.BuildFFNameCache(True)

        Me.m_bStopRun = False
        Me.m_core.SetBatchLock(cCore.eBatchLockType.Update)
        Me.m_core.SetStopRunDelegate(AddressOf StopRun)


        Try
            For Each strFileName As String In Me.m_astrFiles
                Me.m_valStatus = Me.m_valStatus Or Me.ValidateFile(strFileName)
            Next
        Catch ex As Exception
            ' Panic
        End Try

        GC.Collect()

        Me.m_core.SetStopRunDelegate(Nothing)
        Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)
        cApplicationStatusNotifier.EndProgress(Me.m_core)

        '= agk =
        Me.ReleaseWait()
        If (Me.m_dgtComplete IsNot Nothing) Then Me.m_dgtComplete.Invoke()

        Me.m_log.Close()

        ' == Process results ==
        Me.BroadcastStatus(cStringUtils.Localize(My.Resources.VALIDATION_RESULT_SUCCESS, scenario.Name),
                           cStringUtils.Localize(My.Resources.VALIDATION_RESULT_WARNING, Me.m_log.FileName),
                           cStringUtils.Localize(My.Resources.VALIDATION_RESULT_FAILED, Me.m_log.FileName))

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Validate a single CSV file.
    ''' </summary>
    ''' <param name="strFileName">The CSV file to validate.</param>
    ''' -----------------------------------------------------------------------
    Private Function ValidateFile(ByVal strFileName As String) As eStatusFlags

        Dim reader As StreamReader = Nothing
        Dim values() As String = Nothing
        Dim item As cFFCache = Nothing
        Dim strName As String = ""
        Dim iNumMissing As Integer = 0
        Dim status As eStatusFlags = eStatusFlags.OK
        Dim vsInfo As cVariableStatus = Nothing
        Dim bOK As Boolean = True

        ' For effort / mort validation
        Dim manEffort As cBaseShapeManager = Me.m_core.FishingEffortShapeManager
        Dim manFMort As cBaseShapeManager = Me.m_core.FishMortShapeManager
        Dim bHasEffort As Boolean = False
        Dim bHasFMort As Boolean = False
        Dim bHasFleet As Boolean = False
        Dim bHasAllFleet As Boolean = False

        Me.m_log.Add(cStringUtils.Localize(My.Resources.VAL_CSV_READ, strFileName))

        If File.Exists(strFileName) Then

            Try
                ' Open the CSV file for reading
                reader = New StreamReader(strFileName)

                ' Chop off the path for status message purposes
                strFileName = Path.GetFileNameWithoutExtension(strFileName)

                ' First line holds FF names
                values = cStringUtils.SplitQualified(reader.ReadLine(), ","c)
                ' Validate FF names
                For i As Integer = 0 To values.Length - 1
                    ' Get file
                    strName = values(i).Trim()
                    ' Does not exist?
                    If Not Me.m_FFCache.ContainsKey(Me.Key(strName)) Then
                        ' #No: count missing
                        iNumMissing += 1
                        ' Log event
                        vsInfo = New cVariableStatus(eStatusFlags.MissingParameter,
                                                     cStringUtils.Localize(My.Resources.VAL_CSV_FN_MISSING, strFileName, strName),
                                                     eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                        Me.m_valDetails.Add(vsInfo)
                        status = status Or vsInfo.Status

                        ' Can call home?
                        If (Me.m_dgtDisableFile IsNot Nothing) Then
                            ' #Yes: call home
                            Me.m_dgtDisableFile.Invoke(strName)
                        End If
                    Else
                        item = Me.m_FFCache(Me.Key(strName))
                        Dim bIsEffort As Boolean = item.BelongsTo(manEffort)
                        Dim bIsFMort As Boolean = item.BelongsTo(manFMort)
                        If (bIsEffort Or bIsFMort) Then
                            If Me.IsAllFleetName(manEffort, strName) Then bHasAllFleet = True Else bHasFleet = True
                            If bIsEffort Then bHasEffort = True Else bHasFMort = True
                        End If
                    End If
                Next

                ' Summarize
                bOK = (iNumMissing = 0) And (bHasEffort = False Or bHasFMort = False) And (bHasFleet = False Or bHasAllFleet = False)

                ' Log summary
                If (bOK) Then
                    vsInfo = New cVariableStatus(eStatusFlags.OK,
                                                 My.Resources.VAL_CSV_SUMMARY_OK,
                                                 eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                    Me.m_valDetails.Add(vsInfo)

                Else
                    ' Missing parameters
                    If (iNumMissing > 0) Then
                        vsInfo = New cVariableStatus(eStatusFlags.MissingParameter,
                                                     cStringUtils.Localize(My.Resources.VAL_CSV_SUMMARY_MISSING, strFileName, iNumMissing),
                                                     eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                        Me.m_valDetails.Add(vsInfo)
                    End If

                    ' Flag possible effort / mort problems
                    If (bHasEffort And bHasFMort) Then
                        vsInfo = New cVariableStatus(eStatusFlags.MissingParameter,
                                                     cStringUtils.Localize(My.Resources.VAL_CSV_SUMMARY_EFFORT_FMORT_WARNING, strFileName),
                                                     eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                        Me.m_valDetails.Add(vsInfo)
                    End If

                    ' Flag single fleet / all fleet problemts
                    If (bHasFleet And bHasAllFleet) Then
                        vsInfo = New cVariableStatus(eStatusFlags.ErrorEncountered,
                                                     cStringUtils.Localize(My.Resources.VAL_CSV_SUMMARY_FLEET_ERROR, strFileName),
                                                     eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                        Me.m_valDetails.Add(vsInfo)
                    End If

                End If

            Catch ex As Exception
                vsInfo = New cVariableStatus(eStatusFlags.ErrorEncountered,
                                             cStringUtils.Localize(My.Resources.VAL_CSV_READ_ERROR, strFileName, ex.Message),
                                             eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                Me.m_valDetails.Add(vsInfo)
            End Try
        Else
            vsInfo = New cVariableStatus(eStatusFlags.ErrorEncountered,
                                         cStringUtils.Localize(My.Resources.VAL_CSV_READ_MISSING, strFileName),
                                         eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
            Me.m_valDetails.Add(vsInfo)
        End If

        ' Combine status
        For Each vsInfo In Me.m_valDetails
            status = status Or vsInfo.Status
        Next
        Return status

    End Function

#End Region ' File validation

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strPath"></param>
    ''' <param name="strFile"></param>
    ''' <param name="outputs"></param>
    Private Sub WriteResults(ByVal strPath As String, ByVal strFile As String,
                             ByVal outputs As cEcosimResultWriter.eResultTypes())

        Dim resultsWriter As New cEcosimResultWriter(Me.m_core)

        If resultsWriter.WriteResults(strPath, outputs) Then
            Dim fmt As New cEcosimResultTypeFormatter()
            Dim sbFormat As New Text.StringBuilder()
            For Each output As cEcosimResultWriter.eResultTypes In outputs
                If sbFormat.Length > 0 Then sbFormat.Append(", ")
                sbFormat.Append(fmt.GetDescriptor(output))
            Next
            Me.m_log.Add("- Written Ecosim " & sbFormat.ToString)
        Else
            Me.m_log.Add("- Failed to write Ecosim outputs")
        End If

    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="status"></param>
    ''' <remarks></remarks>
    Private Sub UpdateProgress(ByVal strMessage As String, status As eStatusFlags)

        Try
            Dim msg As New cMessage(strMessage, eMessageType.Any, eCoreComponentType.External, eMessageImportance.Information)
            Me.m_core.Messages.SendMessage(msg)
        Catch ex As Exception

        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Commit shape changes to the core in a logical order that ensures that 
    ''' fishing mortality is correctly derived from effort.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub CommitShapes()

        ' I'm sure this can be done much more elegantly, but this will do for a start.
        ' Make sure that every manager gets a shot at committing content to the core.
        ' This commit ensures that fishing mortality is correctly derived from effort, etc

        ' For this, the order that the commits take place is crucial, and also that every shape
        ' type commits only once IF multi-sim has varied any shape of that shape type. 

        ' The code kinda inverts this logic. 

        ' First, step over the managers in the proper commit order (which ensures that Fmort 
        ' is committed before any possibly overriding effort)
        For iMan As Integer = 0 To Me.m_lManagers.Count - 1

            Dim man As cBaseShapeManager = Me.m_lManagers(iMan)
            Dim bUpdate As Boolean = False
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ' JB Aug 2017 - HACK Warning!
            ' Fishing Effort shapes include the "All Fleets" shape. If the effort manager updates 
            ' the 'All Fleets' shape it will overwrite the other shapes with the 'All Fleets' values.
            ' This can be stopped via cShapeBaseManager.Update(bUpdateAll:=False)
            Dim bUpdateAll As Boolean = True ' True for all manager except Fishing Effort
            If TypeOf (man) Is cFishingEffortShapeManger Then bUpdateAll = False
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            ' Check and Update all cached shapes
            For Each ff As cFFCache In Me.m_FFCache.Values
                ' Is this shape changed and does the manager owns this shape?
                If ff.IsChanged() And ff.BelongsTo(man) Then
                    ' Ok, update the shape
                    ff.Update()
                    bUpdate = True
                End If 'ff.IsChanged() And ff.BelongsTo(man)
            Next ff

            ' Now update the manager itself
            If bUpdate Then man.Update(bUpdateAll)

        Next iMan

    End Sub

    Private Sub BroadcastStatus(strSuccess As String, strWarning As String, strError As String, Optional strHyperlink As String = "")

        Dim msg As cMessage = Nothing

        If (Me.m_valStatus = eStatusFlags.OK) Then
            msg = New cFeedbackMessage(strSuccess,
                                       eCoreComponentType.External, eMessageType.DataExport,
                                       eMessageImportance.Information, eMessageReplyStyle.OK)
        ElseIf ((Me.m_valStatus And eStatusFlags.ErrorEncountered) = 0) Then
            msg = New cMessage(strWarning,
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
        Else
            msg = New cMessage(strError,
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
        End If
        msg.Hyperlink = strHyperlink

        ' Attach details
        For Each vs As cVariableStatus In Me.m_valDetails
            msg.AddVariable(vs)
        Next
        msg.Hyperlink = Me.m_strOutFolder

        ' Send!
        Me.m_core.Messages.SendMessage(msg)
        Me.m_log.Add(msg)

        Me.ClearStatus()

    End Sub

    Private Sub ClearStatus()

        Me.m_valDetails.Clear()
        Me.m_valStatus = eStatusFlags.OK

    End Sub


#End Region ' Internals

End Class
