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
''' <para>Database update 6.0.4.022:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecopath discard mortality.</description></item>
''' <item><description>Added Ecosim fisheries regulation.</description></item>
''' <item><description>Updated group x group indexes.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_022
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
            Return 6.04022!
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
            Return "Added Ecopath discards mortality." & Environment.NewLine & _
                   "Added Ecosim fisheries regulation." & Environment.NewLine & _
                   "Updated group x group indexes."
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Return Me.AddDiscardMortality(db) And _
               Me.UpdateEcosimFleets(db) And _
               Me.AddQuotaTable(db) And _
               Me.FlipVulMult(db) And _
               Me.FlipPredPreyShapes(db)

    End Function

    Private Function AddDiscardMortality(ByVal db As cEwEDatabase) As Boolean

        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim bSucces As Boolean = True

        Try
            If db.Execute("ALTER TABLE EcopathCatch ADD COLUMN DiscardMortality SINGLE") Then

                writer = db.GetWriter("EcopathCatch")
                dt = writer.GetDataTable()
                For Each drow As DataRow In dt.Rows
                    drow.BeginEdit()
                    drow("DiscardMortality") = 1.0!
                    drow.EndEdit()
                Next
                db.ReleaseWriter(writer)

            End If
        Catch ex As Exception
            bSucces = False
        End Try
        Return bSucces

    End Function

    Private Function UpdateEcosimFleets(ByVal db As cEwEDatabase) As Boolean

        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim bSucces As Boolean = True

        Try
            db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN MaxEffort SINGLE")
            db.Execute("ALTER TABLE EcosimScenarioFleet ADD COLUMN QuotaType INTEGER")

            writer = db.GetWriter("EcosimScenarioFleet")
            dt = writer.GetDataTable()

            For Each drow As DataRow In dt.Rows
                drow.BeginEdit()
                drow("MaxEffort") = -9999
                drow("QuotaType") = 0
                drow.EndEdit()
            Next
            db.ReleaseWriter(writer)

        Catch ex As Exception

        End Try

        Return bSucces

    End Function

    Private Function AddQuotaTable(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        Try

            bSucces = bSucces And db.Execute("CREATE TABLE EcosimScenarioQuota (ScenarioID LONG, FleetID LONG, EcosimGroupID LONG, Quota SINGLE)")
            bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioQuota ADD PRIMARY KEY (ScenarioID, FleetID, EcosimGroupID)")
            bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioQuota ADD FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario(ScenarioID)")
            bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioQuota ADD FOREIGN KEY (FleetID) REFERENCES EcopathFleet(FleetID)")
            bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioQuota ADD FOREIGN KEY (EcosimGroupID) REFERENCES EcosimScenarioGroup(GroupID)")

        Catch ex As Exception

        End Try

        Return bSucces

    End Function

    Private Structure cVulRowRecord
        Public m_iScenario As Integer
        Public m_iPredator As Integer
        Public m_iPrey As Integer
        Public m_sVulnerability As Single
    End Structure

    Private Function FlipVulMult(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim drow As DataRow = Nothing
        Dim entry As cVulRowRecord = Nothing
        Dim lEntries As New List(Of cVulRowRecord)
        Dim bSucces As Boolean = True

        Try
            Try
                reader = db.GetReader("SELECT * FROM EcosimScenarioForcingMatrix")
                While reader.Read
                    entry = New cVulRowRecord()
                    entry.m_iScenario = CInt(reader("ScenarioID"))
                    entry.m_iPredator = CInt(reader("PredID"))
                    entry.m_iPrey = CInt(reader("PreyID"))
                    entry.m_sVulnerability = CInt(reader("Vulnerability"))
                    lEntries.Add(entry)
                End While
                db.ReleaseReader(reader)

                db.Execute("DELETE * FROM EcosimScenarioForcingMatrix")

                writer = db.GetWriter("EcoSimScenarioForcingMatrix")
                For Each entry In lEntries
                    drow = writer.NewRow()
                    drow("ScenarioID") = entry.m_iScenario
                    ' FLIP!
                    drow("PredID") = entry.m_iPrey
                    drow("PreyID") = entry.m_iPredator
                    ' Copy vul
                    drow("Vulnerability") = entry.m_sVulnerability
                    writer.AddRow(drow)
                Next
                db.ReleaseWriter(writer)

                bSucces = bSucces And db.Execute("ALTER TABLE EcoSimScenarioForcingMatrix DROP COLUMN flowtype")

            Catch ex As Exception
                ' All good, no sim groups
            End Try

        Catch ex As Exception
            bSucces = False
        End Try

        Return bSucces

    End Function

    Private Structure cPredPreyShapeRowRecord
        Public m_iScenario As Integer
        Public m_iPredator As Integer
        Public m_iPrey As Integer
        Public m_iShapeID As Integer
        Public m_iFunctionType As eForcingFunctionApplication
    End Structure

    Private Function FlipPredPreyShapes(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim drow As DataRow = Nothing
        Dim entry As cPredPreyShapeRowRecord = Nothing
        Dim lEntries As New List(Of cPredPreyShapeRowRecord)
        Dim bSucces As Boolean = True

        Try
            Try
                reader = db.GetReader("SELECT * FROM EcosimScenarioPredPreyShape")
                While reader.Read
                    entry = New cPredPreyShapeRowRecord()
                    entry.m_iScenario = CInt(reader("ScenarioID"))
                    entry.m_iPredator = CInt(reader("PredID"))
                    entry.m_iPrey = CInt(reader("PreyID"))
                    entry.m_iShapeID = CInt(reader("ShapeID"))
                    entry.m_iFunctionType = DirectCast(reader("FunctionType"), eForcingFunctionApplication)
                    lEntries.Add(entry)
                End While
                db.ReleaseReader(reader)

                db.Execute("DELETE * FROM EcosimScenarioPredPreyShape")

                writer = db.GetWriter("EcosimScenarioPredPreyShape")
                For Each entry In lEntries
                    drow = writer.NewRow()
                    drow("ScenarioID") = entry.m_iScenario
                    ' FLIP!
                    drow("PredID") = entry.m_iPrey
                    drow("PreyID") = entry.m_iPredator
                    ' Copy vul
                    drow("ShapeID") = entry.m_iShapeID
                    drow("FunctionType") = entry.m_iFunctionType
                    writer.AddRow(drow)
                Next
                db.ReleaseWriter(writer)

            Catch ex As Exception
                ' All good, no sim groups
            End Try

        Catch ex As Exception
            bSucces = False
        End Try

        Return bSucces

    End Function

End Class
