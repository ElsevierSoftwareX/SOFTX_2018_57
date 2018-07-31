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
''' <para>Database update 6.0.6.005:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecopath year.</description></item>
''' <item><description>Added Ecosim quote fields.</description></item>
''' <item><description>Moved Fleet size dynamics data to Ecosim tables.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_06_005
    Inherits cDBUpdate

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
            Return 6.06005!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the text that will be entered in the update log in
    ''' the database.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Moved Fleet size dynamics data to Ecosim tables." & Environment.NewLine & "Added Ecopath year"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean
        Return Me.AddEcopathYear(db) And _
               Me.MoveFleetSizeDynamics(db) And _
               Me.AddEcosimQuotaFields(db)
    End Function

    Private Function AddEcopathYear(ByVal db As cEwEDatabase) As Boolean
        Dim bSucces As Boolean = True

        bSucces = bSucces And db.Execute("ALTER TABLE EcopathModel ADD COLUMN FirstYear LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathModel ADD COLUMN NumYears SINGLE")
        Return bSucces

    End Function

    Private Function MoveFleetSizeDynamics(ByVal db As cEwEDatabase) As Boolean

        Dim nFleets As Integer = CInt(db.GetValue("SELECT COUNT(*) FROM EcopathFleet"))
        Dim iFleetID As Integer
        Dim lScenarioID As New List(Of Integer)
        Dim iScenarioID As Integer

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim drow As DataRow = Nothing
        Dim objKeys() As Object = {Nothing, Nothing}

        Dim sEpower As Single
        Dim sPCapBase As Single
        Dim sCapDepreciate As Single
        Dim sCapBaseGrowth As Single

        Dim bSucces As Boolean = True

        ' Update Ecosim fleet table
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN Epower SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN PCapBase SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN CapDepreciate SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN CapBaseGrowth SINGLE")

        If bSucces = False Then Return False

        ' Read Ecosim scenario IDs
        reader = db.GetReader("SELECT ScenarioID FROM EcosimScenario")
        Try
            While reader.Read : lScenarioID.Add(CInt(reader("ScenarioID"))) : End While
        Catch ex As Exception
        End Try
        db.ReleaseReader(reader)
        reader = Nothing

        reader = db.GetReader("SELECT * FROM EcopathFleet ORDER BY Sequence ASC")
        writer = db.GetWriter("EcosimScenarioFleet")
        Try
            dt = writer.GetDataTable()
            While reader.Read

                ' Read current fleet dynamics values
                iFleetID = CInt(reader("FleetID"))
                Try
                    sEpower = CInt(reader("Epower"))
                Catch ex As Exception
                    sEpower = 0.0!
                End Try
                If sEpower <= 0.0! Then sEpower = 3

                Try
                    sPCapBase = CInt(reader("PCapBase"))
                Catch ex As Exception
                    sPCapBase = 0.0!
                End Try
                If sPCapBase <= 0.0! Then sPCapBase = 0.5!

                Try
                    sCapDepreciate = CInt(reader("CapDepreciate"))
                Catch ex As Exception
                    sCapDepreciate = 0.0!
                End Try
                If sCapDepreciate <= 0.0! Then sCapDepreciate = 0.06!

                Try
                    sCapBaseGrowth = CInt(reader("CapBaseGrowth"))
                Catch ex As Exception
                    sCapBaseGrowth = 0.0!
                End Try
                If sCapBaseGrowth <= 0.0! Then sCapBaseGrowth = 0.2!

                For Each iScenarioID In lScenarioID

                    objKeys(0) = iScenarioID
                    objKeys(1) = iFleetID

                    drow = dt.Rows.Find(objKeys)
                    If drow IsNot Nothing Then

                        drow.BeginEdit()
                        drow("Epower") = sEpower
                        drow("PCapBase") = sPCapBase
                        drow("CapDepreciate") = sCapDepreciate
                        drow("CapBaseGrowth") = sCapBaseGrowth
                        drow.EndEdit()

                    End If

                Next
            End While
        Catch ex As Exception
            bSucces = False
        End Try

        db.ReleaseWriter(writer, bSucces)
        db.ReleaseReader(reader)

        If bSucces Then
            ' Clean up Ecopath fleet table
            bSucces = bSucces And db.Execute("ALTER TABLE EcopathFleet DROP COLUMN Epower SINGLE")
            bSucces = bSucces And db.Execute("ALTER TABLE EcopathFleet DROP COLUMN PCapBase SINGLE")
            bSucces = bSucces And db.Execute("ALTER TABLE EcopathFleet DROP COLUMN CapDepreciate SINGLE")
            bSucces = bSucces And db.Execute("ALTER TABLE EcopathFleet DROP COLUMN CapBaseGrowth SINGLE")
        End If

        Return bSucces

    End Function

    Private Function AddEcosimQuotaFields(ByVal db As cEwEDatabase) As Boolean
        Dim bSucces As Boolean = True

        'bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN FishMortMax SINGLE") ' Already added by update 6.0.6.021
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN Blim SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN Bbase SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN Kalwt SINGLE")

        Return bSucces

    End Function
End Class
