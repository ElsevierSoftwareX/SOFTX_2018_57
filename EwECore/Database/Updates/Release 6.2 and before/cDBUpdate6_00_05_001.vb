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
''' <para>Database update 6.0.5.001:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecopath discard mortality.</description></item>
''' <item><description>Added Ecosim fisheries regulation.</description></item>
''' <item><description>Updated group x group indexes.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_05_001
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
            Return 6.05001!
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
            Return "Updated pedigree." & Environment.NewLine & _
                   "Added particle size distribution tables."
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Return Me.UpdatePedigree(db) And _
               Me.AddPSD(db) And _
               Me.CleanupGroupInfo(db)

    End Function

    Private Function UpdatePedigree(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim drow As DataRow = Nothing
        Dim bSucces As Boolean = True

        Try

            If db.Execute("CREATE TABLE Pedigree (LevelID LONG, VarName TEXT(50), Sequence INTEGER, IndexValue SINGLE, Confidence SINGLE, Description MEMO)") Then

                bSucces = bSucces And db.Execute("ALTER TABLE Pedigree ADD CONSTRAINT PK_INDEX PRIMARY KEY (LevelID)")

                reader = db.GetReader("SELECT * FROM PedigreeLevel")
                writer = db.GetWriter("Pedigree")

                If reader IsNot Nothing Then
                    While reader.Read

                        drow = writer.NewRow()

                        drow("LevelID") = reader("LevelID")
                        drow("VarName") = reader("VarName")
                        drow("Sequence") = reader("Sequence")
                        drow("IndexValue") = reader("IndexValue")
                        drow("Confidence") = reader("Confidence")
                        drow("Description") = reader("Description")

                        writer.AddRow(drow)

                    End While

                    db.ReleaseWriter(writer)
                    db.ReleaseReader(reader)
                End If
            End If

            db.Execute("DROP Table PedigreeLevel")

        Catch ex As Exception
            bSucces = False
        End Try

        Return bSucces

    End Function

    Private Function AddPSD(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim bSucces As Boolean = True

        Try
            Try
                db.Execute("ALTER TABLE EcopathGroup ADD COLUMN vbK SINGLE")
            Catch ex As Exception
                ' All cool
            End Try

            ' Move vbK back to groups for Particle Size Distribution purposes
            writer = db.GetWriter("EcopathGroup")

            ' *DEEP sigh*
            reader = db.GetReader("SELECT EcopathGroup.GroupID AS GroupID, StanzaLifeStage.vbK AS vbK FROM EcopathGroup, StanzaLifeStage WHERE EcopathGroup.GroupID=StanzaLifeStage.GroupID")
            If reader IsNot Nothing Then
                While reader.Read
                    bSucces = bSucces And db.Execute(String.Format("UPDATE EcopathGroup SET vbK={0} WHERE GroupID={1}", reader("vbK"), reader("GroupID")))
                End While
            End If
            db.ReleaseReader(reader)

            ' Now drop the vbK column from StanzaLifeStage
            bSucces = bSucces And db.Execute("ALTER TABLE StanzaLifeStage DROP COLUMN vbK")
            db.ReleaseWriter(writer)

        Catch ex As Exception

        End Try

        Return bSucces

    End Function

    Private Function CleanupGroupInfo(ByVal db As cEwEDatabase) As Boolean
        ' Not crucial
        db.Execute("ALTER TABLE EcopathGroup DROP COLUMN AdultGroup")
        Return True
    End Function

End Class
