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

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.0.07:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added AssumeSquareCells flag</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_08
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500008!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Added WKT support, removed CellSize field"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = db.GetReader("SELECT * FROM EcospaceScenario")
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim drow As DataRow = Nothing
        Dim lID As New List(Of Integer)
        Dim lLen As New List(Of Single)

        If (reader IsNot Nothing) Then

            While reader.Read
                Dim szSize As Single = CSng(db.ReadSafe(reader, "CellSize", 0))
                Dim sLen As Single = CSng(db.ReadSafe(reader, "CellLength", 0))

                If (sLen = 0) Then
                    lLen.Add(szSize * cEcospaceDataStructures.c_sEquatorLength / 360.0!)
                    lID.Add(CInt(reader("ScenarioID")))
                End If
            End While
            db.ReleaseReader(reader)

            If (lID.Count > 0) Then
                writer = db.GetWriter("EcospaceScenario")
                dt = writer.GetDataTable()
                For i As Integer = 0 To lID.Count - 1
                    drow = dt.Rows.Find(lID(i))
                    drow.BeginEdit()
                    drow("CellLength") = lLen(i)
                    drow.EndEdit()
                Next i
                db.ReleaseWriter(writer)
            End If

        End If

        Return db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN CoordinateSystemWKT MEMO")
    End Function

End Class
