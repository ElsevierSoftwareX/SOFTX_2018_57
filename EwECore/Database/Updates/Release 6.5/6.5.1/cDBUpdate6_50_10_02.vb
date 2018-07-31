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

#Region " Imports "

Option Strict On
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.1.02:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Updated Ecotracer</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_10_02
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.501002!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Advection, upwelling stored by month"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = db.Execute("ALTER TABLE EcospaceScenarioMonth ADD COLUMN AdvectionXVelMap MEMO") And
                                 db.Execute("ALTER TABLE EcospaceScenarioMonth ADD COLUMN AdvectionYVelMap MEMO") And
                                 db.Execute("ALTER TABLE EcospaceScenarioMonth ADD COLUMN UpwellingMap MEMO")

        If Not bSucces Then Return False

        ' Duplicate advection data to month fields
        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcospaceScenarioMonth")
        Dim dt As DataTable = writer.GetDataTable()
        Dim drow As DataRow = Nothing
        Dim keys() As Object = New Object() {0, 0}
        Dim reader As IDataReader = db.GetReader("SELECT * FROM EcospaceScenario")

        While reader.Read()
            Dim iScenarioID As Integer = CInt(reader("ScenarioID"))
            Dim strXVel As String = CStr(db.ReadSafe(reader, "XVelMap", ""))
            Dim strYVel As String = CStr(db.ReadSafe(reader, "YVelMap", ""))

            If Not String.IsNullOrWhiteSpace(strXVel) And Not String.IsNullOrWhiteSpace(strYVel) Then
                For iMonth As Integer = 1 To cCore.N_MONTHS
                    keys(0) = iScenarioID
                    keys(1) = iMonth
                    drow = dt.Rows.Find(keys)
                    Dim bNewRow As Boolean = (drow Is Nothing)

                    If (bNewRow) Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioID
                        drow("MonthID") = iMonth
                    Else
                        drow.BeginEdit()
                    End If

                    drow("AdvectionXVelMap") = strXVel
                    drow("AdvectionYVelMap") = strYVel

                    If (bNewRow) Then
                        writer.AddRow(drow)
                    Else
                        drow.EndEdit()
                    End If
                Next
                writer.Commit()
            End If
        End While

        db.ReleaseReader(reader)
        db.ReleaseWriter(writer)

        db.Execute("ALTER TABLE EcospaceScenario DROP COLUMN XVelMap")
        db.Execute("ALTER TABLE EcospaceScenario DROP COLUMN YVelMap")

        Return True

    End Function


End Class
