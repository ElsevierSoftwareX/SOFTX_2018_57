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
''' <para>Database update 6.0.3.2:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Change EcospaceScenario Description column to MEMO</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_03_02
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
        ' - Ecospace description field has changed to type MEMO
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ALTER COLUMN Description MEMO")
        ' - Change a range of numeric columns from Integer to Long
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathFleet ALTER COLUMN Sequence LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroup ALTER COLUMN Sequence LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenario ALTER COLUMN TotalTime LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenario ALTER COLUMN NutForceNumber LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimTimeSeries ALTER COLUMN Sequence LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ALTER COLUMN Inrow LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ALTER COLUMN Incol LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ALTER COLUMN IDH_SS LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioHabitat ALTER COLUMN Sequence LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioMPA ALTER COLUMN Sequence LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE Stanza ALTER COLUMN HatchCode LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE StanzaLifeStage ALTER COLUMN Sequence LONG")

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
            Return "Fixes sequence field types, description field types"
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
            ' The DB version does not reflect a EwE release version
            Return 6.013!
        End Get
    End Property

End Class
