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
Imports EwEUtils.Database
Imports EwEUtils.Utilities

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.0.20:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Updated to new migration map system.
''' </description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_20
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.50002!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Updated to new migration maps"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        ' Create new table to hold migration maps per scenario, group, and month
        bSucces = bSucces And db.Execute("CREATE TABLE EcospaceScenarioGroupMigration (ScenarioID LONG, GroupID LONG, MonthID BYTE, Map MEMO, Concentration SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroupMigration ADD PRIMARY KEY (ScenarioID, GroupID, MonthID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroupMigration ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroupMigration ADD FOREIGN KEY (GroupID) REFERENCES EcospaceScenarioGroup(GroupID)")

        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcospaceScenarioGroupMigration")

        ' For each scenario
        '    read depth map
        Dim readerScenario As IDataReader = db.GetReader("SELECT * FROM EcospaceScenario")
        While readerScenario.Read()

            Dim iScenarioID As Long = CLng(readerScenario("ScenarioID"))
            Dim InRow As Integer = CInt(readerScenario("InRow"))
            Dim InCol As Integer = CInt(readerScenario("InCol"))
            Dim depth(InRow, InCol) As Single
            bSucces = bSucces And cStringUtils.StringToArray(CStr(db.ReadSafe(readerScenario, "DepthMap", "")), depth, InRow, InCol)

            ' For each migratory group
            Dim readerGroup As IDataReader = db.GetReader(String.Format("SELECT * FROM EcospaceScenarioGroup WHERE (ScenarioID={0})", iScenarioID))
            While readerGroup.Read()

                Dim iGroupID As Long = CLng(readerGroup("GroupID"))
                Dim bIsMig As Boolean = (CInt(readerGroup("IsMigratory")) <> 0)

                If (bIsMig) Then

                    Dim MigConcRow As Single = CSng(readerGroup("MigConcRow"))
                    Dim MigConcCol As Single = CSng(readerGroup("MigConcCol"))
                    Dim PrefRow(cCore.N_MONTHS) As Integer, PrefCol(cCore.N_MONTHS) As Integer
                    Dim astrSplit As String()

                    ' read pref row and cols for months
                    astrSplit = CStr(readerGroup("PrefRow")).Split(CChar(" "))
                    For iMonth As Integer = 1 To Math.Min(cCore.N_MONTHS, astrSplit.Length)
                        PrefRow(iMonth) = cStringUtils.ConvertToInteger(astrSplit(iMonth - 1))
                    Next

                    astrSplit = CStr(readerGroup("PrefCol")).Split(CChar(" "))
                    For iMonth As Integer = 1 To Math.Min(cCore.N_MONTHS, astrSplit.Length)
                        PrefCol(iMonth) = cStringUtils.ConvertToInteger(astrSplit(iMonth - 1))
                    Next

                    ' For each month
                    For iMonth As Integer = 1 To cCore.N_MONTHS

                        Dim map(InRow, InCol) As Boolean
                        Dim iRow As Integer = Math.Max(1, Math.Min(InRow, PrefRow(iMonth)))
                        Dim iCol As Integer = Math.Max(1, Math.Min(InCol, PrefCol(iMonth)))

                        map(iRow, iCol) = True

                        ' Add this map to EcospaceScenarioGroupMigration
                        Dim drow As DataRow = writer.NewRow()
                        drow("ScenarioID") = iScenarioID
                        drow("GroupID") = iGroupID
                        drow("MonthID") = iMonth
                        drow("Map") = cStringUtils.ArrayToString(map, InRow, InCol, depth)
                        drow("Concentration") = CSng((Math.Abs(MigConcRow) + Math.Abs(MigConcCol)) / 2)

                        writer.AddRow(drow)
                    Next iMonth
                End If
            End While
            db.ReleaseReader(readerGroup)
        End While
        db.ReleaseReader(readerScenario)

        db.ReleaseWriter(writer, bSucces)

        ' DO NOT YET CHANGE THE STRUCTURE OF THE OLD DATABASE. 
        ' This will happen in a new database update when the migration code has been finalized
#If 0 Then
        ' Delete EcospaceScenarioGroup fields MigConcRow, MigConcCol, PrefRow, PrefCol
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroup DROP COLUMN MigConcRow")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroup DROP COLUMN MigConcCol")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroup DROP COLUMN PrefRow")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioGroup DROP COLUMN PrefCol")
#End If

        Return bSucces

    End Function

End Class
