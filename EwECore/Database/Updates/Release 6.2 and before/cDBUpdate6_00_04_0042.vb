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
Imports EwECore.Auxiliary

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.0.4.004:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Fixes Ecotracer.</description></item>
''' <item><description>Add Pedigree table.</description></item>
''' <item><description>Add monetary unit to models.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_00042
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Return Me.FixEcotracer(db) And Me.AddPedigree(db) And Me.AddMonetaryUnit(db)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the text that will be entered in the update log in
    ''' the database.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Adds Description to Ecotracer Scenarios" & Environment.NewLine & _
                "Adds Pedigree" & Environment.NewLine & _
                "Adds monetary unit"
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
            Return 6.040042!
        End Get
    End Property

    Private Function FixEcotracer(ByVal db As cEwEDatabase) As Boolean
        Try
            db.Execute("ALTER TABLE EcotracerScenario ADD COLUMN Description MEMO")
        Catch
            ' All good
        End Try
        Return True
    End Function

    Private Function AddPedigree(ByVal db As cEwEDatabase) As Boolean

        ' Supported pedigree variables
        Dim vnPedigree As eVarNameFlags() = {eVarNameFlags.Biomass, eVarNameFlags.PBInput, eVarNameFlags.QBInput, eVarNameFlags.DietComp, eVarNameFlags.Landings}
        Dim strValueID As String = ""
        Dim readerPedigree As IDataReader = Nothing
        Dim iPedigree As Integer = 0
        Dim writerRemark As cEwEDatabase.cEwEDbWriter = Nothing
        Dim dt As DataTable = Nothing
        Dim drow As DataRow = Nothing
        Dim bSucces As Boolean = True
        Dim key As cValueID = Nothing

        ' Just in case
        db.Execute("DROP TABLE Pedigree")

        bSucces = db.Execute("CREATE TABLE PedigreeLevel (VarName TEXT(50), Sequence INTEGER, IndexValue SINGLE, Confidence SINGLE, Description MEMO)")
        bSucces = bSucces And db.Execute("ALTER TABLE PedigreeLevel ADD PRIMARY KEY (VarName, Sequence)")
        bSucces = db.Execute("ALTER TABLE Remark ADD COLUMN Pedigree INTEGER)")

        ' Copy existing pedigree levels from group info, if any
        Try
            readerPedigree = db.GetReader("SELECT GroupID, Pedigree1, Pedigree2, Pedigree3, Pedigree4 FROM EcopathGroup")

            writerRemark = db.GetWriter("Remark")
            dt = writerRemark.GetDataTable()

            While readerPedigree.Read

                For i As Integer = 1 To 4
                    Try
                        ' Get pedigree level
                        iPedigree = CInt(readerPedigree("Pedigree" & i))
                        ' Correct to Ewe6 way of life
                        If (iPedigree < 0) Then iPedigree = cCore.NULL_VALUE

                        ' Concoct ID for remark entry that will hold pedigree data
                        key = New cValueID(eDataTypes.EcoPathGroupInput, CInt(readerPedigree("GroupID")), vnPedigree(i - 1))
                        strValueID = key.ToString()

                        ' Find remark row
                        drow = dt.Rows.Find(strValueID)

                        ' Row already esists?
                        If (drow IsNot Nothing) Then
                            ' #Yes: add pedigree to existing row
                            drow.BeginEdit()
                            drow("Pedigree") = iPedigree
                            drow.EndEdit()
                        Else
                            ' #No: create new row
                            drow = writerRemark.NewRow
                            drow("Pedigree") = iPedigree
                            writerRemark.AddRow(drow)
                        End If

                    Catch ex As Exception

                    End Try
                Next i

            End While
            db.ReleaseReader(readerPedigree)
            db.ReleaseWriter(writerRemark)
            readerPedigree = Nothing

        Catch ex As Exception

        End Try

        ' Destroy obsolete pedigree columns
        db.Execute("ALTER TABLE EcopathGroup DROP COLUMN Pedigree1")
        db.Execute("ALTER TABLE EcopathGroup DROP COLUMN Pedigree2")
        db.Execute("ALTER TABLE EcopathGroup DROP COLUMN Pedigree3")
        db.Execute("ALTER TABLE EcopathGroup DROP COLUMN Pedigree4")
        db.Execute("ALTER TABLE EcopathGroup DROP COLUMN Pedigree5")

        Return bSucces
    End Function

    Private Function AddMonetaryUnit(ByVal db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitMonetary LONG")
        Return bSucces And db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitMonetaryCustom TEXT(30)")

    End Function

End Class
