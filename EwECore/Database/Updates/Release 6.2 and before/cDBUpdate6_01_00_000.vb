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
Imports EwEUtils.Core
Imports System.Data
Imports System.Data.OleDb
Imports EwEUtils.Utilities
Imports EwECore.DataSources.cDBDataSource

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.1.0.000:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added MSE fields.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_00_000
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
            Return 6.1!
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
            Return "Added MSE fields."
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Return Me.AddSimFleetID(db) And Me.UpdateMSETables(db)

    End Function

    Private Function AddSimFleetID(ByVal db As cEwEDatabase) As Boolean

        Dim iNextFleetID As Integer = 1
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim alFleets As New List(Of Integer)
        Dim drow As DataRow = Nothing
        Dim iFleetID As Integer = 0
        Dim iScenarioID As Integer = 0
        Dim bSucces As Boolean = db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN FleetID LONG")

        Try
            iNextFleetID = CInt(db.GetValue("SELECT MAX(EcopathFleetID) FROM EcosimScenarioFleet")) + 1
        Catch ex As Exception
            iNextFleetID = 1
        End Try

        writer = db.GetWriter("EcosimScenarioFleet")
        dt = writer.GetDataTable()

        For Each drow In dt.Rows

            iScenarioID = CInt(drow("ScenarioID"))
            iFleetID = CInt(drow("EcopathFleetID"))

            If Not alFleets.Contains(iFleetID) Then
                alFleets.Add(iFleetID)
            Else
                alFleets.Add(iNextFleetID)
                iFleetID = iNextFleetID
                iNextFleetID += 1
            End If

            drow.BeginEdit()
            drow("FleetID") = iFleetID
            drow.EndEdit()
        Next

        dt = Nothing
        db.ReleaseWriter(writer)
        writer = Nothing

        Return bSucces And db.Execute("CREATE UNIQUE INDEX FleetID ON EcosimScenarioFleet (FleetID)")

    End Function

    Private Function UpdateMSETables(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = db.Execute("CREATE TABLE EcosimScenarioGroupYear (ScenarioID LONG, GroupID LONG, TimeYear INTEGER, CVBiom SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroupYear ADD PRIMARY KEY (ScenarioID, GroupID, TimeYear)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroupYear ADD CONSTRAINT SimSGTScen FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario (ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioGroupYear ADD CONSTRAINT SimSGTGroup FOREIGN KEY (GroupID) REFERENCES EcopathGroup (GroupID)")

        bSucces = bSucces And db.Execute("CREATE TABLE EcosimScenarioFleetYear (ScenarioID LONG, FleetID LONG, TimeYear INTEGER, CV SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleetYear ADD PRIMARY KEY (ScenarioID, FleetID, TimeYear)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleetYear ADD CONSTRAINT SimSFTScen FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario (ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioFleetYear ADD CONSTRAINT SimSFTFleet FOREIGN KEY (FleetID) REFERENCES EcopathFleet (FleetID)")

        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioQuota ADD COLUMN QuotaShare SINGLE")

        Return bSucces

    End Function

End Class
