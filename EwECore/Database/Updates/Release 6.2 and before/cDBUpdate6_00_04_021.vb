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
''' <para>Database update 6.0.4.021:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecosim group max fishing mortality.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_021
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
            Return 6.04021!
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
            Return "Added Ecosim group max fishing mortality." & Environment.NewLine & "Split salinity fields."
        End Get
    End Property

#Region " Apply "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Return Me.AddMaxFishingMortality(db) And Me.SplitSDSal(db)

    End Function

    Private Function AddMaxFishingMortality(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim bSucces As Boolean = True

        Try
            db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN FishMortMax SINGLE")
        Catch ex As Exception

        End Try

        Return bSucces

    End Function

    Private Function SplitSDSal(ByVal db As cEwEDatabase) As Boolean

        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim bSucces As Boolean = True

        Try
            db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN SdSalLeft SINGLE")
            db.Execute("ALTER TABLE EcosimScenarioGroup ADD COLUMN SdSalRight SINGLE")

            ' Copy SDSal to both
            writer = db.GetWriter("EcosimScenarioGroup")
            dt = writer.GetDataTable()

            For Each drow As DataRow In dt.Rows
                drow.BeginEdit()
                If Convert.IsDBNull(drow("SdSal")) Then
                    drow("SdSalLeft") = 0
                    drow("SdSalRight") = 0
                Else
                    drow("SdSalLeft") = drow("SdSal")
                    drow("SdSalRight") = drow("SdSal")
                End If
                drow.EndEdit()
            Next
            db.ReleaseWriter(writer)
            db.Execute("ALTER TABLE EcosimScenarioGroup DROP COLUMN SdSal")

        Catch ex As Exception

        End Try

        Return bSucces

    End Function

#End Region ' Apply

End Class
