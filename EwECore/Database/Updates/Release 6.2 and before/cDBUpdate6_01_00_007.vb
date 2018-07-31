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
''' <para>Database update 6.1.0.007:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added pedigree colour.</description></item>
''' <item><description>Changed pedigree storage location.</description></item>
''' <item><description>Fixed pedigree designation error.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_00_007
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
            Return 6.100007!
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
            Return "Added pedigree level colours" & Environment.NewLine & "Changed pedigree storage location" & Environment.NewLine & "Fixed pedigree designation error"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Return Me.AddPedigreeColor(db) And _
               Me.FixPedigreeLandingsColumn(db) And _
               Me.ChangePedigreeStorage(db) And _
               Me.MovePedigreeFromAuxillary(db) And _
               Me.PurgePedigreeAuxillary(db)

    End Function

    Private Function AddPedigreeColor(ByVal db As cEwEDatabase) As Boolean

        ' No need to set defaults; an integer of 0 will mean a 100% transparent colour,
        ' which is the indicator for any GIU to use a default colour for pedigree.
        ' This is identical to the colour behaviour of groups and fleets
        Return db.Execute("ALTER TABLE Pedigree ADD COLUMN LevelColor LONG")

    End Function

    Private Function FixPedigreeLandingsColumn(ByVal db As cEwEDatabase) As Boolean

        Return db.Execute("UPDATE Pedigree SET VarName='TCatchInput' WHERE VarName='Landings'")

    End Function

    Private Function ChangePedigreeStorage(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True
        bSucces = bSucces And db.Execute("CREATE TABLE EcopathGroupPedigree (GroupID LONG, VarName TEXT(50), LevelID LONG)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroupPedigree ADD PRIMARY KEY (GroupID, VarName)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroupPedigree ADD FOREIGN KEY (GroupID) REFERENCES EcopathGroup(GroupID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroupPedigree ADD FOREIGN KEY (LevelID) REFERENCES Pedigree(LevelID)")
        Me.LogProgress("ChangePedigreeStorage", bSucces)
        Return bSucces

    End Function

    Private Function MovePedigreeFromAuxillary(ByVal db As cEwEDatabase) As Boolean

        Dim astrVars() As String = New String() {"Biomass", "PBInput", "QBInput", "DietComp", "Landings"}
        Dim strGroupID As String = "EcopathGroupInput"
        Dim reader As IDataReader = db.GetReader("SELECT * FROM Auxillary")
        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcopathGroupPedigree")
        Dim drow As DataRow = Nothing
        Dim bSucces As Boolean = True

        Try

            While reader.Read()

                Dim strKey As String = CStr(reader("ValueID"))
                Dim iPedigreeLevelID As Long = 0
                Dim iGroupID As Integer = 0
                Dim strVarName As String = ""

                If Not Convert.IsDBNull(reader("PedigreeLevelID")) Then
                    Try
                        iPedigreeLevelID = CLng(reader("PedigreeLevelID"))
                    Catch ex As Exception
                        ' Huh?!
                    End Try
                End If

                If strKey.IndexOf(strGroupID) = 0 And iPedigreeLevelID > 0 Then

                    Dim astrBits As String() = strKey.Split(":"c)
                    iGroupID = CInt(astrBits(1))
                    strVarName = astrBits(2)

                    If strVarName = "Landings" Then strVarName = "TCathInput"

                    drow = writer.NewRow
                    drow("GroupID") = iGroupID
                    drow("LevelID") = iPedigreeLevelID
                    drow("VarName") = strVarName
                    writer.AddRow(drow)

                End If

            End While

        Catch ex As Exception
            ' bSucces = False 
        End Try

        db.ReleaseWriter(writer, True)
        db.ReleaseReader(reader)

        Return bSucces

    End Function

    Private Function PurgePedigreeAuxillary(ByVal db As cEwEDatabase) As Boolean

        ' Need to remove relationship first, may fail
        db.Execute("ALTER TABLE Auxillary DROP COLUMN PedigreeLevelID")
        Return True

    End Function

End Class
