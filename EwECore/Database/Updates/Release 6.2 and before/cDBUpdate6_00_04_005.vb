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
''' <para>Database update 6.0.4.005:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Adds Ecospace weight layer tables.</description></item>
''' <item><description>Fixed field lengths.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_0005
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
            Return 6.04005!
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
            Return "Adds Ecospace weight layer tables" & Environment.NewLine & "Fixed field lengths"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Return Me.AddEcospaceWeightTables(db) And Me.FixFieldLengths(db)

    End Function


    Private Function AddEcospaceWeightTables(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        ' + Add EcospaceScenarioWeightLayer
        bSucces = bSucces And db.Execute("CREATE TABLE EcospaceScenarioWeightLayer (ScenarioID LONG, LayerID LONG, Sequence INTEGER, Name TEXT(50), Description MEMO, Weight SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioWeightLayer ADD PRIMARY KEY (LayerID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioWeightLayer ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")
        ' + Add EcospaceScenarioWeightLayerCell
        bSucces = bSucces And db.Execute("CREATE TABLE EcospaceScenarioWeightLayerCell (ScenarioID LONG, LayerID LONG, InRow INTEGER, InCol INTEGER, Weight SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioWeightLayerCell ADD PRIMARY KEY (ScenarioID, LayerID, InRow, InCol)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioWeightLayerCell ADD FOREIGN KEY (LayerID) REFERENCES EcospaceScenarioWeightLayer(LayerID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioWeightLayerCell ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")

        Return bSucces

    End Function

    Private Function FixFieldLengths(ByVal db As cEwEDatabase) As Boolean
        Return db.Execute("ALTER TABLE EcosimTimeSeriesDataset ALTER COLUMN DatasetName TEXT(255)")
    End Function

End Class
