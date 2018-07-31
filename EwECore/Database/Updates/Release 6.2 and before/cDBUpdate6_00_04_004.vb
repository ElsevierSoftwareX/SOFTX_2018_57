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
Imports EwEPlugin
Imports EwEUtils.Database
Imports System.Data
Imports EwEUtils.Core

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.0.4.004:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Fixes relative primary production issue in existing Ecospace scenarios.</description></item>
''' <item><description>Fixes units.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_0004
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean


        Return Me.FixRelPP(db) And Me.FixUnits(db)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the text that will be entered in the update log in
    ''' the database.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Fixes relative primary production issue in existing Ecospace scenarios." & Environment.NewLine _
                & "Fixes system units."
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the update version number that will be entered in
    ''' the update log of the database. This version number is also used to check
    ''' whether an update should run.
    ''' </summary>
    ''' <remarks>
    ''' If <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> is provided, the
    ''' update is ran regardless of version number.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.04004!
        End Get
    End Property

    Private Function FixRelPP(ByVal db As EwEUtils.Database.cEwEDatabase) As Boolean

        ' Query to count # of non-zero cells in the basemap
        Dim strQueryCheck As String = "SELECT COUNT(*) FROM EcospaceScenarioBasemap WHERE RelPP<>0 AND ScenarioID={0}"
        Dim strQuerySet As String = "UPDATE EcospaceScenarioBasemap SET RelPP=1 WHERE ScenarioID={0} AND Depth>0"
        Dim iScenarioID As Integer = 0
        Dim iNumCells As Integer = 0
        Dim reader As IDataReader = Nothing
        Dim bSucces As Boolean = True

        Try
            reader = db.GetReader("SELECT ScenarioID FROM EcospaceScenario")
            While reader.Read
                iScenarioID = CInt(reader("ScenarioID"))
                iNumCells = CInt(db.GetValue(String.Format(strQueryCheck, iScenarioID)))
                If (iNumCells = 0) Then
                    db.Execute(String.Format(strQuerySet, iScenarioID))
                End If
            End While
            db.ReleaseReader(reader)
        Catch ex As Exception
            bSucces = False
        End Try
        Return bSucces

    End Function

    Private Structure cUnitMapping
        Public ModelID As Integer
        Public CurrencyUnit As eUnitCurrencyType
        Public CurrencyCustom As String
        Public TimeUnit As eUnitTimeType
        Public TimeCustom As String

        Public Sub New(ByVal id As Integer, ByVal cut As eUnitCurrencyType, ByVal strCurr As String, ByVal utt As eUnitTimeType, ByVal strTime As String)
            Me.ModelID = id
            Me.CurrencyUnit = cut
            Me.CurrencyCustom = strCurr
            Me.TimeUnit = utt
            Me.TimeCustom = strTime
        End Sub

    End Structure

    Private Function FixUnits(ByVal db As EwEUtils.Database.cEwEDatabase) As Boolean

        Dim strSQL As String = ""
        Dim lMappings As New List(Of cUnitMapping)
        Dim reader As IDataReader = Nothing
        Dim bSucces As Boolean = True

        ' Fix previous mistake, if any
        db.Execute("ALTER TABLE EcopathModel DROP COLUMN UnitCurrency")
        db.Execute("ALTER TABLE EcopathModel DROP COLUMN UnitTime")

        ' Add proper columns
        db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitCurrency LONG")
        db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitCurrencyCustom TEXT(30)")
        db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitTime LONG")
        db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitTimeCustom TEXT(30)")

        ' Transfer unit values
        Try

            reader = db.GetReader("SELECT * FROM EcopathModel")
            While reader.Read

                Dim uct As eUnitCurrencyType = eUnitCurrencyType.WetWeight
                Dim strCurrencyCustom As String = ""
                Dim utt As eUnitTimeType = eUnitTimeType.Year
                Dim strTimeCustom As String = ""

                Try
                    uct = DirectCast(reader("CurrencyIndex"), eUnitCurrencyType)
                Catch ex As Exception
                    uct = eUnitCurrencyType.WetWeight
                End Try
                Try
                    strCurrencyCustom = CStr(reader("CurrencyUnit"))
                Catch ex As Exception
                    strCurrencyCustom = ""
                End Try

                Select Case strCurrencyCustom.Trim.ToLower()
                    Case "", "t/km²" : uct = eUnitCurrencyType.WetWeight : strCurrencyCustom = ""
                    Case "kcal/m²" : uct = eUnitCurrencyType.Calorie : strCurrencyCustom = ""
                    Case "g/m²" : uct = DirectCast(uct + 1, eUnitCurrencyType) : strCurrencyCustom = ""
                    Case "j/m²" : uct = eUnitCurrencyType.Joules : strCurrencyCustom = ""
                    Case "mg n/m²" : uct = eUnitCurrencyType.Nitrogen : strCurrencyCustom = ""
                    Case "mg p/m²" : uct = eUnitCurrencyType.Phosporous : strCurrencyCustom = ""
                End Select

                Try
                    strTimeCustom = CStr(reader("TimeUnit"))
                Catch ex As Exception
                    strTimeCustom = "year"
                End Try
                Select Case strTimeCustom.ToLower()
                    Case "", "year" : utt = eUnitTimeType.Year : strTimeCustom = ""
                    Case "day" : utt = eUnitTimeType.Day : strTimeCustom = ""
                    Case Else : utt = eUnitTimeType.Custom
                End Select

                lMappings.Add(New cUnitMapping(CInt(reader("ModelID")), uct, strCurrencyCustom, utt, strTimeCustom))

            End While

        Catch ex As Exception

        End Try
        db.ReleaseReader(reader)
        reader = Nothing

        ' Now apply
        For Each m As cUnitMapping In lMappings
            strSQL = String.Format("UPDATE EcopathModel SET UnitCurrency={0}, UnitCurrencyCustom='{1}', UnitTime={2}, UnitTimeCustom='{3}' WHERE ModelID={4}", _
                        CInt(m.CurrencyUnit), m.CurrencyCustom, m.TimeUnit, m.TimeCustom, m.ModelID)
            db.Execute(strSQL)
        Next

        db.Execute("ALTER TABLE EcopathModel DROP COLUMN TimeUnit")
        db.Execute("ALTER TABLE EcopathModel DROP COLUMN CurrencyUnit")
        db.Execute("ALTER TABLE EcopathModel DROP COLUMN CurrencyIndex")

        Return bSucces

    End Function

End Class
