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
Imports EwEUtils.Utilities

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.2.0.06:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Discontinued regions as objects, merged into a single map.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_02_00_06
    Inherits cDBUpdate

    Private Class cRegionInfo
        Public iScenarioID As Integer
        Public strMap As String
        Public iNumRegions As Integer
    End Class

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.120006!
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
            Return "Discontinued regions as separate objects."
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim info As cRegionInfo = Nothing
        Dim readerScenario As IDataReader = db.GetReader("SELECT ScenarioID, InRow, InCol, DepthMap FROM EcospaceScenario")
        Dim readerRegions As IDataReader = Nothing
        Dim depth(,) As Single = Nothing
        Dim regions(,) As Integer = Nothing
        Dim region(,) As Integer = Nothing
        Dim nRows, nCols As Integer
        Dim lRegions As New List(Of cRegionInfo)
        Dim bSuccess As Boolean = True

        ' For each scenario
        While readerScenario.Read()

            ' Prepare buffers
            info = New cRegionInfo()
            lRegions.Add(info)

            ' Read scenario bits
            info.iScenarioID = CInt(readerScenario("ScenarioID"))
            nRows = CInt(readerScenario("InRow"))
            nCols = CInt(readerScenario("InCol"))

            ' Allocate memory
            ReDim depth(nRows, nCols)
            ReDim regions(nRows, nCols)
            ReDim region(nRows, nCols)

            ' Read depth map
            cStringUtils.StringToArray(CStr(readerScenario("DepthMap")), depth, nRows, nCols)

            ' Read region maps and merge 'em
            readerRegions = db.GetReader(String.Format("SELECT * FROM EcospaceScenarioRegion WHERE (ScenarioID={0}) ORDER BY Sequence", info.iScenarioID))
            While readerRegions.Read()

                ' Account for region
                info.iNumRegions += 1

                ' Read region map
                Array.Clear(region, 0, region.Length)
                cStringUtils.StringToArray(CStr(readerRegions("RegionMap")), region, nRows, nCols)

                ' Merge region map into final
                For iRow As Integer = 1 To nRows
                    For iCol As Integer = 1 To nCols
                        If (region(iRow, iCol) > 0) Then
                            regions(iRow, iCol) = info.iNumRegions
                        End If
                    Next iCol
                Next iRow

            End While

            ' Preserve map
            info.strMap = cStringUtils.ArrayToString(regions, nRows, nCols, depth)

            ' Clean up
            db.ReleaseReader(readerRegions)
            readerRegions = Nothing
            depth = Nothing
            regions = Nothing
            region = Nothing

        End While

        ' Clean up
        db.ReleaseReader(readerScenario)
        readerScenario = Nothing

        ' Update receiving end
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN NumRegions LONG")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN RegionMap MEMO")

        ' Store maps
        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcospaceScenario")
        Dim dt As DataTable = writer.GetDataTable
        For Each info In lRegions
            Dim drow As DataRow = dt.Rows.Find(info.iScenarioID)
            Try
                drow.BeginEdit()
                drow("RegionMap") = info.strMap
                drow("NumRegions") = info.iNumRegions
                drow.EndEdit()
            Catch ex As Exception
                bSuccess = False
            End Try
        Next
        db.ReleaseWriter(writer, bSuccess)
        lRegions.Clear()

        ' Destroy region table
        If bSuccess Then
            bSuccess = bSuccess And db.Execute("DROP TABLE EcospaceScenarioRegion")
        End If

        Return bSuccess

    End Function

End Class
