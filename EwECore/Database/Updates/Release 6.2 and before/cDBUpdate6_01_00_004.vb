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

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.1.0.004:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecopath taxonomy support.</description></item>
''' <item><description>Added Ecopath model area support.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_00_004
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
            Return 6.100004!
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
            Return "Added Ecopath taxonomy support" & Environment.NewLine & "Added Ecopath model area" & Environment.NewLine & "Fixed date columns to Float"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = Me.DiscardSpeciesTable(db)
        bSucces = bSucces And Me.CreateTaxonTable(db)
        bSucces = bSucces And Me.AddModelAreaName(db)
        bSucces = bSucces And Me.FixJulianDates(db)

        Return bSucces

    End Function

    Private Function DiscardSpeciesTable(ByVal db As cEwEDatabase) As Boolean

        Return db.Execute("DROP TABLE SPECIES")

    End Function

    Private Function CreateTaxonTable(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        bSucces = bSucces And db.Execute("CREATE TABLE EcopathGroupTaxon (TaxonID LONG, EcopathGroupID LONG, CodeISCAAP TEXT(3), CodeTaxon TEXT(14), Code3A TEXT(4), ClassName TEXT(50), OrderName TEXT(50), FamilyName TEXT(50), GenusName TEXT(50), SpeciesName TEXT(50), CommonName TEXT(50), Proportion SINGLE, SourceName TEXT(50), SourceKey MEMO, LastUpdated FLOAT)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroupTaxon ADD CONSTRAINT PK_INDEX PRIMARY KEY (TaxonID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroupTaxon ADD FOREIGN KEY (EcopathGroupID) REFERENCES EcopathGroup(GroupID)")

        Return bSucces

    End Function

    Private Function AddModelAreaName(ByVal db As cEwEDatabase) As Boolean

        Return db.Execute("ALTER TABLE EcopathModel ADD COLUMN AreaName TEXT(255)")

    End Function

    Private Function FixJulianDates(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        bSucces = bSucces And db.Execute("ALTER TABLE EcopathModel ALTER COLUMN LastSaved FLOAT")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimScenario ALTER COLUMN LastSaved FLOAT")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ALTER COLUMN LastSaved FLOAT")
        bSucces = bSucces And db.Execute("ALTER TABLE EcotracerScenario ALTER COLUMN LastSaved FLOAT")

        Return bSucces

    End Function

End Class
