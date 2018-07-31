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
''' <para>Database update 6.0.3.8:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added EcosimTimeseriesDataset.</description></item>
''' <item><description>Migrated existing Time Series data to new Dataset table.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_00
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim drow As DataRow = Nothing
        Dim strDataset As String = ""
        Dim strDatasetLast As String = ""
        Dim iDatasetID As Integer = 0
        Dim bSucces As Boolean = True

        ' + Add EcosimTimeSeriesDataset
        bSucces = bSucces And db.Execute("CREATE TABLE EcosimTimeseriesDataset (DatasetID INTEGER, DatasetName TEXT(50), Description MEMO, Author TEXT(64), Contact TEXT(255), FirstYear INTEGER, NumYears INTEGER)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimTimeseriesDataset ADD PRIMARY KEY (DatasetID)")
        ' + Add FK
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimTimeseries ADD COLUMN DatasetID LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimTimeseries ADD FOREIGN KEY (DatasetID) REFERENCES EcosimTimeseriesDataset(DatasetID)")

        ' Nothing to migrate?
        reader = db.GetReader("SELECT * FROM EcosimTimeSeries ORDER BY Dataset")
        If reader IsNot Nothing Then

            ' Start migrating

            ' Populate dataset table
            writer = db.GetWriter("EcosimTimeSeriesDataset")
            While reader.Read()
                strDataset = CStr(reader("Dataset"))
                If (String.Compare(strDatasetLast, strDataset, False) <> 0) Then
                    iDatasetID += 1
                    drow = writer.NewRow()
                    drow("DatasetID") = iDatasetID
                    drow("DatasetName") = strDataset
                    drow("FirstYear") = reader("FirstYear")
                    drow("NumYears") = reader("NumYears")
                    writer.AddRow(drow)
                    strDatasetLast = strDataset
                End If
            End While
            db.ReleaseWriter(writer, True)
            db.ReleaseReader(reader)

            ' Link existing time series to new datasets
            reader = db.GetReader("SELECT DatasetID, DatasetName FROM EcosimTimeseriesDataset")
            While reader.Read()
                bSucces = bSucces And db.Execute(String.Format("UPDATE EcosimTimeseries SET DatasetID={0} WHERE Dataset='{1}'", _
                        CInt(reader("DatasetID")), CStr(reader("DatasetName"))))
            End While
            db.ReleaseReader(reader)
        End If

        ' Try to delete columns Dataset, FirstYear, NumYears from table Time Series (not crucial)
        db.Execute("ALTER TABLE EcosimTimeseries DROP COLUMN Dataset")
        db.Execute("ALTER TABLE EcosimTimeseries DROP COLUMN FirstYear")
        db.Execute("ALTER TABLE EcosimTimeseries DROP COLUMN NumYears")

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
            Return "Adds Ecosim TimeseriesDataset table" + Environment.NewLine + _
                   "Migrates existing Time Series data to new Dataset table"
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
            Return 6.04!
        End Get
    End Property

End Class
