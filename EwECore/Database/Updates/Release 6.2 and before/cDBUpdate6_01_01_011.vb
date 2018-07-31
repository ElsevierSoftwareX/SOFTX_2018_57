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

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.1.1.011:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added landings mediation tables</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_01_011
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
            Return 6.101011!
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
            Return "Added landings mediation."
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Return AddLandingsMediationTable(db) And AddLandingsMediationWeightsTable(db)

    End Function

    Private Function AddLandingsMediationTable(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True
        bSucces = bSucces And db.Execute("CREATE TABLE EcosimScenarioLandingsShape (ScenarioID LONG, FleetID LONG, GroupID LONG, ShapeID LONG, FunctionType SHORT)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioLandingsShape ADD PRIMARY KEY (ScenarioID, FleetID, GroupID, ShapeID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioLandingsShape ADD FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario(ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioLandingsShape ADD FOREIGN KEY (FleetID) REFERENCES EcosimScenarioFleet(FleetID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioLandingsShape ADD FOREIGN KEY (GroupID) REFERENCES EcosimScenarioGroup(GroupID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioLandingsShape ADD FOREIGN KEY (ShapeID) REFERENCES EcosimShape(ShapeID)")
        Me.LogProgress("AddLandingsMediationTable", bSucces)
        Return bSucces

    End Function

    Private Function AddLandingsMediationWeightsTable(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True
        bSucces = bSucces And db.Execute("CREATE TABLE EcosimScenarioshapeMedWeightsLandings (ScenarioID LONG, GroupID LONG, FleetID LONG, ShapeID LONG, MedWeights SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioshapeMedWeightsLandings ADD PRIMARY KEY (ScenarioID, GroupID, FleetID, ShapeID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioshapeMedWeightsLandings ADD FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario(ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioshapeMedWeightsLandings ADD FOREIGN KEY (GroupID) REFERENCES EcosimScenarioGroup(GroupID)")
        ' No FK because field may be NULL
        'bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioshapeMedWeightsLandings ADD FOREIGN KEY (FleetID) REFERENCES EcosimScenarioFleet(FleetID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenarioshapeMedWeightsLandings ADD FOREIGN KEY (ShapeID) REFERENCES EcosimShape(ShapeID)")
        Me.LogProgress("EcosimScenarioshapeMedWeightsLandings", bSucces)
        Return bSucces

    End Function


End Class
