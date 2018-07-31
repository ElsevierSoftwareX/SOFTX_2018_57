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

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.0.3.6:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecotracer tables</description></item>
''' <item><description>Removed Ecoranger tables</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_03_06
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        ' Update(s):
        ' - Add Ecotracer tables
        bSucces = bSucces And db.Execute("CREATE TABLE EcotracerScenario (ScenarioID INTEGER, ScenarioName TEXT(50), Czero SINGLE, Cinflow SINGLE, Coutflow SINGLE, Cdecay SINGLE, Author TEXT(64), Contact TEXT(255), Description MEMO)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcotracerScenario ADD PRIMARY KEY (ScenarioID)")

        bSucces = bSucces And db.Execute("CREATE TABLE EcotracerScenarioGroup (ScenarioID INTEGER, EcopathGroupID INTEGER, Czero SINGLE, Cimmig SINGLE, Cenv SINGLE, Cdecay SINGLE, Cexcretionrate SINGLE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcotracerScenarioGroup ADD PRIMARY KEY (ScenarioID, EcopathGroupID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcotracerScenarioGroup ADD FOREIGN KEY (ScenarioID) REFERENCES EcotracerScenario(ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcotracerScenarioGroup ADD FOREIGN KEY (EcopathGroupID) REFERENCES EcopathGroup(GroupID)")

        ' - Discontinue ecoranger info in table EcopathModel if available (not essential)
        db.Execute("ALTER TABLE EcopathModel DROP COLUMN EcoRangerRangerRun")

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the text that will be entered in the update log in
    ''' the database.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Adds Ecotracer tables" + Environment.NewLine + "Removes Ecoranger tables"
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
            Return 6.036!
        End Get
    End Property

End Class
