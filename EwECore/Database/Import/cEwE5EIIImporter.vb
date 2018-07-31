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
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.Utilities

#End Region ' Imports 

Namespace Database

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Imports an EwE5 .eii into an EwE6 database
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEwE5EIIImporter
        Inherits cEwE5ModelImporter

#Region " Private helper class "

        Private Class cImportData
            Inherits cEcopathDataStructures

            Public Sub New(ByVal CoreMessagePublisher As cMessagePublisher)
                MyBase.New(CoreMessagePublisher)
            End Sub

            Public UnitTime As eUnitTimeType = eUnitTimeType.Year
            Public UnitTimeCustom As String = ""
            Public UnitCurrencyCustom As String = ""

        End Class

#End Region ' Private helper class

#Region " Private vars "

        ''' <summary>Data buffer.</summary>
        Private m_data As cImportData

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

            Me.m_data = New cImportData(Me.m_core.Messages)

        End Sub

#End Region ' Construction

#Region " Overrides "

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Close"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Open(ByVal strSource As String) As Boolean
            Debug.Assert(False, Me.ToString & ".Open() removed for Mono compatibility.")
            Return True
        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Close"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub Close()
            Debug.Assert(False, Me.ToString & ".Close() removed for Mono compatibility.")
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.IsOpen"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function IsOpen() As Boolean
            Debug.Assert(False, Me.ToString & ".IsOpen() removed for Mono compatibility.")
            Return False
        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="cEwE5ModelImporter.Models"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Models() As cExternalModelInfo()
            Dim info As New cExternalModelInfo("1", Path.GetFileNameWithoutExtension(Me.m_strSource), "Ecopath 5 EII file", 0)
            Return New cExternalModelInfo() {info}
        End Function

        Public Overrides Function CanImportFrom(strSource As String) As Boolean
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

            Dim dbUpd As cDatabaseUpdater = Nothing
            Dim bSucces As Boolean = False

            Me.m_iNumSteps = 7

            ' ToDo: globalize this

            Me.LogProgress("Loading eii file...")
            If Me.LoadEII() Then
                bSucces = Me.Save()
            End If

            ' Set version
            Me.m_dbTarget.SetVersion(Me.m_dbTarget.GetVersion(), "Imported from EII file '" & Me.m_strSource & "'")

            ' Now run all available updates on the new EwE6 database
            dbUpd = New cDatabaseUpdater(Me.m_core, 6.0!)
            dbUpd.UpdateDatabase(Me.m_dbTarget)
            dbUpd = Nothing

            ' Release DB
            Me.m_dbTarget = Nothing

            Me.LogMessage(My.Resources.CoreMessages.IMPORT_PROGRESS_COMPLETE)

            Return bSucces

        End Function

#End Region ' The import 

#Region " Loading "

        Private Function LoadEII() As Boolean

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

            If Not File.Exists(Me.m_strSource) Then
                cLog.Write(Me.ToString + ".LoadEcopath(...) No file name specified.")
                Return False
            End If

            Try
                eiiStrm = New System.IO.StreamReader(Me.m_strSource)
            Catch ex As Exception
                cLog.Write(Me.ToString + ".LoadEcopath(...) Error opening eii file. '" & Me.m_strSource & "' Error:" + ex.Message())
                Return False
            End Try

            'fake model data
            ecopathDS.ModelDBID = 1
            ecopathDS.ModelName = Path.GetFileName(Me.m_strSource)
            ecopathDS.ModelNumDigits = 3
            ecopathDS.ModelDescription = "Simulated model read from EII file " & Me.m_strSource

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
                'groups
                For K = 1 To ecopathDS.NumGroups

                    buff = eiiStrm.ReadLine()
                    'delimiter is 2 spaces "  " yeah....
                    recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, "  ")
                    iNextIndex = 0

                    'Debug.Assert(data.Length = 10, "EII DataSource wrong number of recs in group section.")
                    ecopathDS.GroupName(K) = Me.GetNextValidValue(recs, iNextIndex).Trim(quotes)
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), pvar)
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.DtImp(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.Ex(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.fCatch(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.DC(K, 0))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.Binput(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.PBinput(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.EEinput(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.GEinput(K))
                    Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.QBinput(K))

                    ecopathDS.BHinput(K) = ecopathDS.Binput(K) / ecopathDS.Area(K)

                    ecopathDS.GroupDBID(K) = K

                    ecopathDS.PP(K) = pvar - 2
                    If K > ecopathDS.NumLiving Then ecopathDS.PP(K) = 2
                    If ecopathDS.GE(K) = 0 Then ecopathDS.GE(K) = -9

                Next K


                '' "Read DietComp"
                ReDim ecopathDS.DietChanged(1, 0)
                For K = 1 To ecopathDS.NumGroups
                    buff = eiiStrm.ReadLine()
                    recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, "  ")
                    iNextIndex = 0
                    For j = 1 To ecopathDS.NumGroups

                        Single.TryParse(Me.GetNextValidValue(recs, iNextIndex), ecopathDS.DCInput(K, j))
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
                buff = eiiStrm.ReadLine()
                Dim seperators() As String = {" ", "  "}
                recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                'recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, "  ")
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


                buff = eiiStrm.ReadLine()
                recs = EwEUtils.Utilities.cStringUtils.SplitQualified(buff, "  ")

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
                buff = eiiStrm.ReadLine()
                recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                For i = 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(i - 1), ecopathDS.BAInput(i))
                Next i

                ' Diet Fate array added July 1994/VC
                'If EOF(fnum) = False And NumGroups > NumLiving + 1 Then
                'More than 1 detritusbox Any reason for this??
                For i = 1 To ecopathDS.NumGroups
                    buff = eiiStrm.ReadLine()
                    recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                    For j = ecopathDS.NumLiving + 1 To ecopathDS.NumGroups
                        Single.TryParse(recs(j - ecopathDS.NumLiving - 1), ecopathDS.DF(i, j - ecopathDS.NumLiving))
                        ' Input(fnum, ecopathDS.DF(i, j - ecopathDS.NumLiving))    
                    Next j
                Next i

                ' Emigration added Dec 98/VC
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("Emigration"), "EII datasource file format may be wrong!")
                buff = eiiStrm.ReadLine()
                recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                'Input(fnum, jnk) ' 
                For i = 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(i - 1), ecopathDS.Emigration(i))
                    ' Input(fnum, ecopathDS.Emigration(i))
                Next i

                'immigration added Dec 98/VC
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("Immig"), "EII datasource file format may be wrong!")
                buff = eiiStrm.ReadLine()
                recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                For i = 1 To ecopathDS.NumGroups
                    Single.TryParse(recs(i - 1), ecopathDS.Immig(i))
                    ' Input(fnum, ecopathDS.Immig(i))
                Next i

                'NumGear
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("NumGear"), "EII datasource file format may be wrong!")
                buff = eiiStrm.ReadLine()
                Integer.TryParse(buff, ecopathDS.NumFleet)
                ecopathDS.RedimFleetVariables(True)

                'Gearnames
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("Gearnames"), "EII datasource file format may be wrong!")
                For i = 1 To ecopathDS.NumFleet
                    buff = eiiStrm.ReadLine()
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
                    buff = eiiStrm.ReadLine()
                    recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                    Single.TryParse(recs(0), ecopathDS.CostPct(i, eCostIndex.Fixed))
                    Single.TryParse(recs(1), ecopathDS.CostPct(i, eCostIndex.CUPE))
                    Single.TryParse(recs(2), ecopathDS.CostPct(i, eCostIndex.Sail))
                Next i

                'landing
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("landing"), "EII datasource file format may be wrong!")
                'Input(fnum, jnk)  
                For i = 1 To ecopathDS.NumFleet
                    buff = eiiStrm.ReadLine()
                    recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
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
                    buff = eiiStrm.ReadLine()
                    recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                    For j = 1 To ecopathDS.NumGroups
                        Single.TryParse(recs(j - 1), ecopathDS.Discard(i, j))
                        '  Input(fnum, ecopathDS.Landing(i, j))    ' Landing added Dec 98/VC
                    Next j
                Next i

                'discard fate
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("DiscardFate"), "EII datasource file format may be wrong!")
                For i = 1 To ecopathDS.NumFleet
                    buff = eiiStrm.ReadLine()
                    recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
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
                    buff = eiiStrm.ReadLine()
                    recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
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
                buff = eiiStrm.ReadLine()
                recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
                For i = 1 To ecopathDS.NumGroups             ' Added Dec 98/VC
                    Single.TryParse(recs(i - 1), ecopathDS.Shadow(i))
                    '  Input(fnum, ecopathDS.Shadow(i))
                Next i

                ''Habitatarea
                buff = eiiStrm.ReadLine()
                Debug.Assert(buff.Contains("Area&HabitatBiomass(BH)"), "EII datasource file format may be wrong!")
                buff = eiiStrm.ReadLine()
                recs = buff.Split(seperators, System.StringSplitOptions.RemoveEmptyEntries)
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

        ''' <summary>
        ''' Returns the next valid, non-empty string from a series of input entries.
        ''' </summary>
        ''' <param name="data">The input entries to scan.</param>
        ''' <param name="iNextIndex">The index of that string.</param>
        ''' <returns></returns>
        Private Function GetNextValidValue(ByVal data() As String, ByRef iNextIndex As Integer) As String
            Dim str As String
            Do While String.IsNullOrWhiteSpace(str)
                str = data(iNextIndex)
                iNextIndex += 1
            Loop
            Return str
        End Function

#Region "Old LoadEII (not Mono compatible)"

#If 0 Then

        ''' <summary>
        ''' The old datasource code, to be transmogrified into database import logic
        ''' Replaced with Mono compatible streams 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function LoadEII() As Boolean

            'read the contents of the eii file into a private EcopathParameters object
            'this is written using vb file access instead of a filestream to keep it as close to the original vb code as possible
            Dim pvar As Single
            Dim i As Integer
            Dim j As Integer
            Dim K As Integer
            Dim Dummy As Single
            Dim jnk As String
            Dim Import As Integer

            Debug.Assert(Me.IsOpen())

            'read the file
            Try
                Input(Me.m_iFNum, m_data.NumGroups)
                Input(Me.m_iFNum, m_data.NumLiving)
                Input(Me.m_iFNum, m_data.UnitCurrencyCustom)
                Input(Me.m_iFNum, m_data.currUnitIndex)

                If Not m_data.redimGroupVariables() Then
                    Me.LogMessage(".LoadModel(...) Failed to Re-Dimension group parameter arrays.")
                    Return False
                End If

                'groups
                For K = 1 To m_data.NumGroups
                    Input(Me.m_iFNum, m_data.GroupName(K)) : Input(Me.m_iFNum, pvar) : Input(Me.m_iFNum, m_data.DtImp(K))
                    Input(Me.m_iFNum, m_data.Ex(K)) : Input(Me.m_iFNum, m_data.fCatch(K)) : Input(Me.m_iFNum, m_data.DCInput(K, 0))
                    Input(Me.m_iFNum, m_data.Binput(K)) : Input(Me.m_iFNum, m_data.PBinput(K)) : Input(Me.m_iFNum, m_data.EEinput(K))
                    Input(Me.m_iFNum, m_data.GEinput(K)) : Input(Me.m_iFNum, m_data.QBinput(K))

                    m_data.BHinput(K) = m_data.Binput(K) / m_data.Area(K)

                    m_data.GroupDBID(K) = K

                    m_data.PP(K) = pvar - 2
                    If K > m_data.NumLiving Then m_data.PP(K) = 2
                    If m_data.GE(K) = 0 Then m_data.GE(K) = -9

                Next K

                ' "Read DietComp"
                ReDim m_data.DietChanged(1, 0)
                For K = 1 To m_data.NumGroups
                    For j = 1 To m_data.NumGroups
                        Input(Me.m_iFNum, m_data.DCInput(K, j))
                    Next j
                Next K

                If EOF(Me.m_iFNum) Then Return True

                'jb totp read in original routine using a string will read the entire line
                Input(Me.m_iFNum, jnk)
                'jb I have no idea what this is all about 
                If Import < 0 Then Import = 0

                'Unassimilated food
                For j = 1 To m_data.NumGroups
                    Input(Me.m_iFNum, Dummy) : Input(Me.m_iFNum, m_data.GS(j))
                    If Dummy < 0 Then Dummy = 0
                    m_data.GS(j) = Dummy + m_data.GS(j)
                    If m_data.GS(j) > 1 Then m_data.GS(j) = m_data.GS(j) / 100
                Next j

                Input(Me.m_iFNum, jnk)

                'the time unit name
                If EOF(Me.m_iFNum) = False Then
                    Dim tmpbuff As String
                    Input(Me.m_iFNum, tmpbuff)
                    m_data.TimeUnitName = tmpbuff.Trim
                End If

                'the ecosystem remarks.
                Input(Me.m_iFNum, jnk)

                For i = 1 To m_data.NumGroups             ' parms.Bomass accumulation added March 95/VC
                    Input(Me.m_iFNum, m_data.BA(i))
                Next i

                'If EOF(me.m_iFNum) = False And NumGroups > NumLiving + 1 Then
                'More than 1 detritusbox Any reason for this??
                For i = 1 To m_data.NumGroups
                    For j = m_data.NumLiving + 1 To m_data.NumGroups
                        Input(Me.m_iFNum, m_data.DF(i, j - m_data.NumLiving))     ' Diet Fate array added July 1994/VC
                    Next j
                Next i

                Input(Me.m_iFNum, jnk) ' 
                For i = 1 To m_data.NumGroups             ' Emigration added Dec 98/VC
                    Input(Me.m_iFNum, m_data.Emigration(i))
                Next i

                Input(Me.m_iFNum, jnk)
                For i = 1 To m_data.NumGroups                 ' immigration added Dec 98/VC
                    Input(Me.m_iFNum, m_data.Immig(i))
                Next i

                Input(Me.m_iFNum, jnk)  'NumGear
                Input(Me.m_iFNum, m_data.NumFleet)

                m_data.RedimFleetVariables(True)

                Input(Me.m_iFNum, jnk) 'Gearnames
                For i = 1 To m_data.NumFleet             ' Added Dec 98/VC
                    Input(Me.m_iFNum, m_data.FleetName(i))
                    m_data.FleetDBID(i) = i
                Next i

                Input(Me.m_iFNum, jnk)  'cost
                For i = 1 To m_data.NumFleet
                    'First is fixed cost, second is cost per unit effort' Added Dec 98/VC
                    Input(Me.m_iFNum, m_data.CostPct(i, eCostIndex.Fixed))
                    Input(Me.m_iFNum, m_data.CostPct(i, eCostIndex.CUPE))
                    Input(Me.m_iFNum, m_data.CostPct(i, eCostIndex.Sail))
                Next i

                Input(Me.m_iFNum, jnk)  'landing
                For i = 1 To m_data.NumFleet
                    For j = 1 To m_data.NumGroups
                        Input(Me.m_iFNum, m_data.Landing(i, j))    ' Landing added Dec 98/VC
                    Next j
                Next i

                Input(Me.m_iFNum, jnk)  'discard
                For i = 1 To m_data.NumFleet
                    For j = 1 To m_data.NumGroups
                        Input(Me.m_iFNum, m_data.Discard(i, j))    ' Added Dec 98/VC
                    Next j
                Next i

                Input(Me.m_iFNum, jnk)  'discard
                For i = 1 To m_data.NumFleet
                    For j = 1 To m_data.NumGroups - m_data.NumLiving
                        Input(Me.m_iFNum, m_data.DiscardFate(i, j))   ' Added Dec 98/VC
                    Next j
                Next i

                Input(Me.m_iFNum, jnk)  'market
                For i = 1 To m_data.NumFleet
                    For j = 1 To m_data.NumGroups
                        Input(Me.m_iFNum, m_data.Market(i, j))    ' Added Dec 98/VC
                    Next j
                Next i

                m_data.NoGearData = False

                'shadow
                Input(Me.m_iFNum, jnk)
                For i = 1 To m_data.NumGroups             ' Added Dec 98/VC
                    Input(Me.m_iFNum, m_data.Shadow(i))
                Next i

                'Habitatarea
                Input(Me.m_iFNum, jnk)  '
                For i = 1 To m_data.NumGroups             ' Added Dec 98/VC
                    Input(Me.m_iFNum, m_data.Area(i))
                    Input(Me.m_iFNum, m_data.BH(i))
                Next i

            Catch ex As Exception 'catch any error during the reading of the data
                'some kind of a reading error better find out what happend
                Me.LogMessage(".LoadEcopath() Error reading eii file. Error: " + ex.Message())
                Debug.Assert(False)
                Return False
            End Try

            Return True

        End Function

#End If

#End Region

#End Region ' Loading

#Region " Saving "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save imported data to the EwE6 database.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function Save() As Boolean
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_MODEL)
            Me.SaveModel()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_ECOPATHGROUPS)
            Me.SaveGroups()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_POGRESS_DIETCOMP)
            Me.SaveDietComp()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_FLEET)
            Me.SaveFleets()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH)
            Me.SaveCatch()
            Me.LogProgress(My.Resources.CoreMessages.IMPORT_PROGRESS_CATCH)
            Me.SaveDiscardFate()
            Return True
        End Function

#Region " Model "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save Ecopath model
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub SaveModel()

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strYear As String = ""
            Dim dt As DateTime = Nothing

            ' Clear table
            Me.m_dbTarget.Execute("DELETE * FROM EcopathModel")

            writer = m_dbTarget.GetWriter("EcopathModel")

            drow = writer.NewRow()
            drow("ModelID") = 1
            drow("Name") = Path.GetFileNameWithoutExtension(Me.m_strSource)
            drow("Description") = "Imported from EII file '" & Path.GetFileName(Me.m_strSource) & "'"
            drow("NumDigits") = 3

            drow("UnitCurrency") = Me.m_data.ModelUnitCurrency
            drow("UnitCurrencyCustom") = Me.m_data.UnitCurrencyCustom

            Select Case Me.m_data.UnitTimeCustom.Trim.ToLower()
                Case "year", "" : Me.m_data.UnitTime = eUnitTimeType.Year : Me.m_data.UnitTimeCustom = ""
                Case "day" : Me.m_data.UnitTime = eUnitTimeType.Day : Me.m_data.UnitTimeCustom = ""
                Case Else : Me.m_data.UnitTime = eUnitTimeType.Custom
            End Select
            drow("UnitTime") = Me.m_data.UnitTime
            drow("UnitTimeCustom") = Me.m_data.UnitTimeCustom

            drow("MonetaryUnit") = "EUR"
            writer.AddRow(drow)
            Me.m_dbTarget.ReleaseWriter(writer, True)

        End Sub

#End Region ' Model

#Region " Groups "

        ''' <summary>
        ''' Save Ecopath groups
        ''' </summary>
        Private Function SaveGroups() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Clear table(s)
            Me.m_dbTarget.Execute("DELETE * FROM EcopathGroup")
            writer = m_dbTarget.GetWriter("EcopathGroup")

            Try
                For iGroup As Integer = 1 To Me.m_data.NumGroups

                    drow = writer.NewRow()

                    drow("GroupID") = iGroup
                    drow("Sequence") = iGroup
                    drow("GroupName") = Me.m_data.GroupName(iGroup)
                    drow("Type") = Me.m_data.PP(iGroup)
                    drow("Area") = Me.m_data.Area(iGroup)
                    drow("BiomAcc") = Me.m_data.BAInput(iGroup)
                    drow("BiomAccRate") = Me.m_data.BaBi(iGroup)
                    drow("Unassim") = Me.m_data.GS(iGroup)
                    drow("DtImports") = Me.m_data.DtImp(iGroup)
                    drow("Export") = Me.m_data.Ex(iGroup)
                    drow("Catch") = Me.m_data.fCatch(iGroup)
                    drow("ImpVar") = Me.m_data.DCInput(iGroup, 0)
                    drow("GroupIsFish") = Me.m_data.GroupIsFish(iGroup)
                    drow("GroupIsInvert") = Me.m_data.GroupIsInvert(iGroup)
                    drow("NonMarketValue") = Me.m_data.Shadow(iGroup)
                    drow("Respiration") = Me.m_data.Resp(iGroup)

                    'variable with input/output pair only the input gets saved
                    drow("EcoEfficiency") = Me.m_data.EEinput(iGroup)
                    drow("ProdBiom") = Me.m_data.PBinput(iGroup)
                    drow("ConsBiom") = Me.m_data.QBinput(iGroup)
                    drow("ProdCons") = Me.m_data.GEinput(iGroup)
                    drow("Biomass") = Me.m_data.Binput(iGroup)

                    drow("Immigration") = Me.m_data.Immig(iGroup)
                    drow("Emigration") = Me.m_data.Emigration(iGroup)
                    drow("EmigRate") = Me.m_data.Emig(iGroup)
                    drow("PoolColor") = cStringUtils.Localize("{0:x8}", 0)

                    writer.AddRow(drow)

                Next iGroup

            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_dbTarget.ReleaseWriter(writer)
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save Ecopath diets
        ''' </summary>
        ''' <returns>True if successful</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveDietComp() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathDietComp")
            writer = Me.m_dbTarget.GetWriter("EcopathDietComp")

            Try
                For iPred As Integer = 1 To Me.m_data.NumGroups
                    For iPrey As Integer = 1 To Me.m_data.NumGroups

                        drow = writer.NewRow()

                        drow("PredID") = iPred
                        drow("PreyID") = iPrey
                        drow("Diet") = Me.m_data.DCInput(iPred, iPrey)
                        If iPrey > Me.m_data.NumLiving Then
                            drow("DetritusFate") = Me.m_data.DF(iPred, iPrey - Me.m_data.NumLiving)
                        Else
                            drow("DetritusFate") = 0
                        End If

                        writer.AddRow(drow)

                    Next iPrey
                Next iPred
            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_dbTarget.ReleaseWriter(writer, True)

            Return bSucces
        End Function

#End Region ' Groups

#Region " Fleets "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save Ecopath fleets
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveFleets() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            writer = Me.m_dbTarget.GetWriter("EcopathFleet")
            Try
                For iFleet As Integer = 1 To Me.m_data.NumFleet

                    drow = writer.NewRow()

                    drow("Sequence") = iFleet
                    drow("FleetID") = iFleet
                    drow("FleetName") = Me.m_data.FleetName(iFleet)
                    drow("FixedCost") = Me.m_data.CostPct(iFleet, eCostIndex.Fixed)
                    drow("SailingCost") = Me.m_data.CostPct(iFleet, eCostIndex.Sail)
                    drow("variableCost") = Me.m_data.CostPct(iFleet, eCostIndex.CUPE)
                    drow("PoolColor") = cStringUtils.Localize("{0:x8}", 0)

                    writer.AddRow(drow)

                Next iFleet

            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while saving EcopathFleet", ex.Message))
                bSucces = False
            End Try
            Me.m_dbTarget.ReleaseWriter(writer, True)

            Return bSucces
        End Function

        ''' <summary>
        ''' Save Ecopath catch data
        ''' </summary>
        Private Function SaveCatch() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathCatch")
            writer = Me.m_dbTarget.GetWriter("EcopathCatch")
            Try
                For iFleet As Integer = 1 To Me.m_data.NumFleet
                    For iGroup As Integer = 1 To Me.m_data.NumGroups

                        ' JS 04aug08: only save rows with data
                        If (Me.m_data.Landing(iFleet, iGroup) > 0.0!) Or _
                           (Me.m_data.Discard(iFleet, iGroup) > 0.0!) Or _
                           ((Me.m_data.Market(iFleet, iGroup) > 0.0!) And (Me.m_data.Market(iFleet, iGroup) < 1.0!)) Or _
                           (Me.m_data.PropDiscardMort(iFleet, iGroup) > 0.0!) Then

                            drow = writer.NewRow()
                            drow("FleetID") = iFleet
                            drow("GroupID") = iGroup
                            drow("Landing") = Me.m_data.Landing(iFleet, iGroup)
                            drow("Discards") = Me.m_data.Discard(iFleet, iGroup)
                            drow("Price") = Me.m_data.Market(iFleet, iGroup)
                            drow("DiscardMortality") = Me.m_data.PropDiscardMort(iFleet, iGroup)
                            writer.AddRow(drow)

                        End If

                    Next iGroup
                Next iFleet
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while saving catch", ex.Message))
                bSucces = False
            End Try
            Me.m_dbTarget.ReleaseWriter(writer)

            Return bSucces
        End Function

        ''' <summary>
        ''' Save Ecopath discard fate
        ''' </summary>
        Private Function SaveDiscardFate() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_dbTarget.Execute("DELETE * FROM EcopathDiscardFate")
            writer = Me.m_dbTarget.GetWriter("EcopathDiscardFate")
            Try
                For iFleet As Integer = 1 To Me.m_data.NumFleet
                    For iGroup As Integer = 1 To Me.m_data.NumGroups - Me.m_data.NumLiving

                        drow = writer.NewRow()

                        drow("FleetID") = iFleet
                        drow("GroupID") = (iGroup + Me.m_data.NumLiving)
                        drow("DiscardFate") = Me.m_data.DiscardFate(iFleet, iGroup)

                        writer.AddRow(drow)

                    Next iGroup
                Next iFleet
            Catch ex As Exception
                Me.LogMessage(cStringUtils.Localize("Error {0} occurred while saving DiscardFate", ex.Message))
                bSucces = False
            End Try

            Me.m_dbTarget.ReleaseWriter(writer)
            Return bSucces

        End Function

#End Region ' Fleets

#End Region ' Saving

    End Class

End Namespace ' Database
