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
''' <para>Database update 6.0.3.7:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added salinity to Ecosim.</description></item>
''' <item><description>Added concentration FF to Ecotracer.</description></item>
''' <item><description>Fixed Ecospace group defaults.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_03_07
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

        ' - EcotracerScenario
        ' * Add concentration Forcing Function ID
        bSucces = bSucces And db.Execute("ALTER TABLE EcotracerScenario ADD COLUMN ConForcingShapeID LONG")

        ' - EcosimScenario
        ' * Add salinity Forcing Function ID
        bSucces = bSucces And db.Execute("ALTER TABLE EcoSimScenario ADD COLUMN SalinityForcingShapeID LONG")
        ' * Remove obsolete column NutForceNumber
        bSucces = bSucces And db.Execute("ALTER TABLE EcoSimScenario DROP COLUMN NutForceNumber")

        ' EcosimScenarioGroup
        ' * Add salinity columns
        bSucces = bSucces And db.Execute("ALTER TABLE EcoSimScenarioGroup ADD COLUMN SalOpt SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcoSimScenarioGroup ADD COLUMN SdSal SINGLE")

        ' Fixed wrong defaults in Ecospace
        bSucces = bSucces And db.Execute("UPDATE EcospaceScenarioGroup SET Mvel=300 WHERE Mvel=0")
        bSucces = bSucces And db.Execute("UPDATE EcospaceScenarioGroup SET RelMoveBad=2 WHERE RelMoveBad=0")
        bSucces = bSucces And db.Execute("UPDATE EcospaceScenarioGroup SET RelVulBad=2 WHERE RelVulBad=0")
        bSucces = bSucces And db.Execute("UPDATE EcospaceScenarioGroup SET EatEffBad=0.001 WHERE EatEffBad=0")

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
            Return "Added salinity to Ecosim" + Environment.NewLine + "Added concentration FF to Ecotracer" + Environment.NewLine + "Fixed missing Ecospace group defaults"
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
            Return 6.037!
        End Get
    End Property

End Class
