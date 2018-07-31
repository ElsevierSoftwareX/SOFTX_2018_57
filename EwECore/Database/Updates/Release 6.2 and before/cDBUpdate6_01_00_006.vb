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
''' <para>Database update 6.1.0.006:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added pedigree level name.</description></item>
''' <item><description>Added advection fields.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_00_006
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
            Return 6.100006!
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
            Return "Added pedigree level name" & Environment.NewLine & "Added Ecospace advection fields"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Return Me.AddPedigreeName(db) And AddEcospaceAdvectionFields(db)

    End Function

    Private Function AddPedigreeName(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim drow As DataRow = Nothing
        Dim lstrDescriptions As New List(Of String)
        Dim liDBID As New List(Of Integer)
        Dim iDBID As Integer = 0
        Dim bSucces As Boolean = True

        If db.Execute("ALTER TABLE Pedigree ADD COLUMN LevelName TEXT(50)") Then

            ' Read Ecosim scenario IDs
            reader = db.GetReader("SELECT * FROM Pedigree")
            Try
                While reader.Read : lstrDescriptions.Add(CStr(reader("Description"))) : liDBID.Add(CInt(reader("LevelID"))) : End While
            Catch ex As Exception
            End Try
            db.ReleaseReader(reader)
            reader = Nothing

            writer = db.GetWriter("Pedigree")
            Try
                dt = writer.GetDataTable()

                For i As Integer = 0 To liDBID.Count - 1

                    iDBID = liDBID(i)
                    drow = dt.Rows.Find(iDBID)
                    Debug.Assert(drow IsNot Nothing)

                    drow.BeginEdit()
                    drow("LevelName") = lstrDescriptions(i).Substring(0, Math.Min(lstrDescriptions(i).Length, 49))
                    drow.EndEdit()

                Next i
            Catch ex As Exception
                bSucces = False
            End Try

            db.ReleaseWriter(writer, bSucces)
        End If
        Return bSucces

    End Function

    Private Function AddEcospaceAdvectionFields(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioBasemap ADD COLUMN XVel SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioBasemap ADD COLUMN YVel SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioBasemap ADD COLUMN DepthA LONG")
        Return bSucces

    End Function

End Class
