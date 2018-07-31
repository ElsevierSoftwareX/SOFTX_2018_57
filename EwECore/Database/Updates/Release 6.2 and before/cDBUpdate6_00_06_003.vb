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
Imports System
Imports System.Data
Imports System.Text
Imports EwEPlugin
Imports EwEUtils.Database
Imports EwEUtils.Core

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.0.6.003:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Fixed .</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_06_003
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
            Return 6.06003!
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
            Return "Added missing Ecosim fleet definitions; Fixed Ecosim effort duplication issue"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Do yer thang.
    ''' </summary>
    ''' <param name="db"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean
        Return Me.FixEcosimFleets(db) And _
               Me.FixMultipleLinkedEffortShapes(db)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update: create missing fleet entries for Ecosim.
    ''' </summary>
    ''' <param name="db"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function FixEcosimFleets(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True
        Dim reader As IDataReader = Nothing
        Dim liFleets As New List(Of Integer)
        Dim liScenarios As New List(Of Integer)
        Dim aiNumFleets(,) As Integer
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim drow As DataRow = Nothing

        ' 1) get list of all Ecopath fleets
        reader = db.GetReader("SELECT FleetID FROM EcopathFleet")
        While reader.Read
            liFleets.Add(CInt(reader("FleetID")))
        End While
        db.ReleaseReader(reader)

        ' 2) get list of all Ecosim scenarios
        reader = db.GetReader("SELECT ScenarioID FROM EcosimScenario")
        While reader.Read
            liScenarios.Add(CInt(reader("ScenarioID")))
        End While
        db.ReleaseReader(reader)

        ' 3) get fleet/scenario inventory
        ReDim aiNumFleets(liFleets.Count - 1, liScenarios.Count - 1)
        reader = db.GetReader("SELECT * FROM EcosimScenarioFleet")
        While reader.Read
            Dim iScenario As Integer = liScenarios.IndexOf(CInt(reader("ScenarioID")))
            Dim iFleet As Integer = liFleets.IndexOf(CInt(reader("EcopathFleetID")))
            aiNumFleets(iFleet, iScenario) += 1
        End While
        db.ReleaseReader(reader)

        ' 4) if not exists fleet (sim, path) then create it. 
        '    Leave effort shape empty, DB loader will fix this
        writer = db.GetWriter("EcosimScenarioFleet")
        Try

            For iFleet As Integer = 0 To liFleets.Count - 1
                For iScenario As Integer = 0 To liScenarios.Count - 1
                    If aiNumFleets(iFleet, iScenario) = 0 Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = liScenarios(iScenario)
                        drow("EcopathFleetID") = liFleets(iFleet)
                        drow("FishRateShapeID") = 0 ' To be fixed when the app reloads
                        drow("MaxEffort") = cCore.NULL_VALUE
                        drow("QuotaType") = eQuotaTypes.NoControls
                        writer.AddRow(drow)
                    End If
                Next iScenario
            Next iFleet

        Catch ex As Exception
            bSucces = False
        End Try

        db.ReleaseWriter(writer, bSucces)

        ' Dear debugger,
        '
        ' Do you understand the need for the statement below? If you do, please
        ' let us know because we are clueless. For an unknown reason, subsequent 
        ' database updates using the table 'EcosimScenarioFleet' would fail, reporting
        ' that the table was locked. The culprit appeared to be this very function. 
        ' Yes, this method both reads and writes 'EcosimScenarioFleet', but all 
        ' readers and writers are properly and timely released, and no readers are
        ' assigned while writers are in use, or vice-versa. So in brief all database
        ' interactions seem legitimate and terminated properly.
        '
        ' Despite all this, the 'table in use' issue kept appearing on Windows 7
        ' machines. When the table tickle was inserted below, supposedly as a stress
        ' test, the table lock no longer appeared in consecutive database updates.
        '
        ' There seems to be no logical explanation, other than some hidden caching
        ' and release behaviour that somehow gets invoked by the tickle. For now, the
        ' statement is left in place.
        '
        ' However, by nesting each database update in an individual transaction, the
        ' need for tickling seems to have gone away. Sheesh, this is bizarre...
        'Me.TickleTable(db, "EcosimScenarioFleet")

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update: clear references to effort shapes that are used MORE than once.
    ''' Every fleet in every scenario must have its own effort shape which was
    ''' not always the case due to a bug in the database import logic.
    ''' </summary>
    ''' <param name="db"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function FixMultipleLinkedEffortShapes(ByVal db As cEwEDatabase) As Boolean

        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim drow As DataRow = Nothing
        Dim liShapes As New List(Of Integer)
        Dim iShape As Integer
        Dim bSucces As Boolean = True

        writer = db.GetWriter("EcosimScenarioFleet")
        Try
            dt = writer.GetDataTable()
            For Each drow In dt.Rows
                iShape = CInt(drow("FishRateShapeID"))
                If liShapes.Contains(iShape) Then
                    drow.BeginEdit()
                    drow("FishRateShapeID") = 0
                    drow.EndEdit()
                Else
                    liShapes.Add(iShape)
                End If
            Next
        Catch ex As Exception
            bSucces = False
        End Try

        db.ReleaseWriter(writer, bSucces)
        Return bSucces

    End Function

    '''' -----------------------------------------------------------------------
    '''' <summary>
    '''' Tickle a table...
    '''' </summary>
    '''' <param name="db"></param>
    '''' <param name="strTable"></param>
    '''' -----------------------------------------------------------------------
    'Private Sub TickleTable(ByRef db As cEwEDatabase, ByVal strTable As String)
    '    ' Sanity test
    '    Debug.Assert(db.Execute("ALTER TABLE " & strTable & " ADD COLUMN Ping SINGLE"))
    '    Debug.Assert(db.Execute("ALTER TABLE " & strTable & " DROP COLUMN Ping"))
    'End Sub

End Class
