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
Imports EwEUtils.Core

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.2.0.02:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Max time shape length will be remembered in the EwE model
''' since this setting is Ecosim scenario independent.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_02_00_02
    Inherits cDBUpdate


    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.120002!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Max forcing time remembered in model" & Environment.NewLine & _
                   "Added capacity map tables" & Environment.NewLine &
                   "Applied fix to Stanza table"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean
        Return Me.UpdateForcePoints(db) And _
               Me.AddCapacityMapTable(db) And _
               Me.AddCapacityMapAssignmentTable(db) And _
               Me.UpdateMediationTable(db) And _
               Me.UpdateEcospaceParmsTable(db)
    End Function

    Private Function UpdateForcePoints(ByVal db As cEwEDatabase) As Boolean

        Dim iForcePoints As Integer = cEcosimDatastructures.DEFAULT_N_FORCINGPOINTS
        Dim readerScenario As IDataReader = Nothing
        Dim bSuccess As Boolean = True

        ' Read ecosim run length
        bSuccess = bSuccess And db.Execute("CREATE TABLE EcosimModel (ModelID LONG, ForcePoints LONG)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimModel ADD PRIMARY KEY (ModelID)")
        Me.LogProgress("CREATE TABLE EcosimModel", bSuccess)

        readerScenario = db.GetReader("SELECT TotalTime FROM EcosimScenario")
        Try
            While readerScenario.Read()
                iForcePoints = Math.Max(CInt(readerScenario(0)) * cCore.N_MONTHS, iForcePoints)
            End While
        Catch ex As Exception
            iForcePoints = Integer.MaxValue
        End Try
        db.ReleaseReader(readerScenario)

        ' Write max forcing time
        bSuccess = bSuccess And db.Execute("INSERT INTO EcosimModel ( ModelID, ForcePoints ) VALUES (1, " & iForcePoints & ")")

        Me.LogProgress("UPDATE TABLE EcosimModel", bSuccess)
        Return bSuccess

    End Function

    Private Function AddCapacityMapTable(ByVal db As cEwEDatabase) As Boolean
        Dim bSuccess As Boolean = True

        ' Read ecosim run length
        bSuccess = bSuccess And db.Execute("CREATE TABLE EcospaceScenarioDriverLayer (ScenarioID LONG, LayerID LONG, Sequence LONG, LayerName TEXT(50), LayerDescription MEMO, LayerMAP MEMO)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioDriverLayer ADD PRIMARY KEY (ScenarioID, LayerID)")
        bSuccess = bSuccess And db.Execute("CREATE UNIQUE INDEX idMap ON EcospaceScenarioDriverLayer(LayerID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioDriverLayer ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")

        Me.LogProgress("ADD TABLE EcospaceScenarioDriverLayer", bSuccess)
        Return bSuccess

    End Function

    Private Function AddCapacityMapAssignmentTable(ByVal db As cEwEDatabase) As Boolean
        Dim bSuccess As Boolean = True

        bSuccess = bSuccess And db.Execute("CREATE TABLE EcospaceScenarioCapacitDrivers (ScenarioID LONG, GroupID LONG, VarName TEXT(50), VarDBID LONG, ShapeID LONG)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioCapacitDrivers ADD PRIMARY KEY (ScenarioID, GroupID, VarName, VarDBID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioCapacitDrivers ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioCapacitDrivers ADD FOREIGN KEY (GroupID) REFERENCES EcopathGroup(GroupID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioCapacitDrivers ADD FOREIGN KEY (ShapeID) REFERENCES EcosimShape(ShapeID)")

        Me.LogProgress("ADD TABLE EcospaceScenarioCapacitDrivers", bSuccess)
        Return bSuccess

    End Function

    Private Function UpdateMediationTable(ByVal db As cEwEDatabase) As Boolean
        Dim bSuccess As Boolean = True

        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimShapeMediation ADD COLUMN XAxisMin SINGLE")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimShapeMediation ADD COLUMN XAxisMax SINGLE")

        Me.LogProgress("Updated table EcosimShapeMediation", bSuccess)
        Return bSuccess
    End Function

    Private Function UpdateStanzaTable(ByVal db As cEwEDatabase) As Boolean

        db.Execute("ALTER TABLE Stanza ADD COLUMN EggAtSpawn SHORT")
        Me.LogProgress("Updated table UpdateStanzaTable", True)
        Return True

    End Function

    Private Function UpdateEcospaceParmsTable(ByVal db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = True
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN CapacityCalType SHORT")
        'bSuccess = bSuccess And db.Execute(String.Format("UPDATE EcospaceScenario SET CapacityCalType={0}", CInt(eEcospaceCapacityCalType.Habitat)))
        bSuccess = bSuccess And db.Execute(String.Format("UPDATE EcospaceScenario SET CapacityCalType={0}", 1))
        Me.LogProgress("Updated table EcospaceScenario", True)
        Return bSuccess

    End Function

End Class
