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
''' <para>Database update 6.0.6.004:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Simplified port table layout.</description></item>
''' <item><description>Added GroupDigits to model.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_06_004
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
            Return 6.06004!
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
            Return "Simplified port table layout." & Environment.NewLine & "Added GroupDigits setting"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean
        Return Me.FixEcospaceFleetMap(db) And Me.AddGroupDigits(db)
    End Function

    Private Function AddGroupDigits(ByVal db As cEwEDatabase) As Boolean
        Return db.Execute("ALTER TABLE EcopathModel ADD COLUMN GroupDigits INTEGER")
    End Function

    Private Class cScenarioFleetMap

        Private m_nRows As Integer
        Private m_nCols As Integer
        Private m_dtFleetPorts As New Dictionary(Of Integer, List(Of Integer))

        Public Sub New(ByVal nRows As Integer, ByVal nCols As Integer)
            Me.m_nRows = nRows
            Me.m_nCols = nCols
        End Sub

        Public ReadOnly Property InRow() As Integer
            Get
                Return Me.m_nRows
            End Get
        End Property

        Public ReadOnly Property InCol() As Integer
            Get
                Return Me.m_nCols
            End Get
        End Property

        Public Sub SetPort(ByVal iFleetID As Integer, ByVal strPort As String)

            Dim astrPorts As String() = strPort.Split(" "c)
            Dim lPorts As New List(Of Integer)
            For i As Integer = 0 To astrPorts.Length - 1
                If (astrPorts(i) = "1"c) Then
                    lPorts.Add(i)
                End If
            Next
            m_dtFleetPorts(iFleetID) = lPorts
        End Sub

        Public ReadOnly Property HasPort(ByVal iFleetID As Integer, ByVal iRow As Integer, ByVal iCol As Integer) As Boolean
            Get
                Dim iCell As Integer = (iCol - 1) + ((iRow - 1) * Me.m_nCols)
                Dim lPorts As List(Of Integer) = Me.m_dtFleetPorts(iFleetID)
                Return (lPorts.IndexOf(iCell) > -1)
            End Get
        End Property

        Public Function FleetIDs() As Integer()
            Dim lFleetIDs As New List(Of Integer)
            For Each iFleetID As Integer In Me.m_dtFleetPorts.Keys
                lFleetIDs.Add(iFleetID)
            Next
            Return lFleetIDs.ToArray()
        End Function

    End Class

    Private Function FixEcospaceFleetMap(ByVal db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim sfm As cScenarioFleetMap = Nothing
        Dim iPortID As Integer = 1
        Dim dtSFM As New Dictionary(Of Integer, cScenarioFleetMap)
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim drow As DataRow = Nothing
        Dim bSucces As Boolean = True

        ' Read basemap dimensions per scenario
        reader = db.GetReader("SELECT * FROM EcospaceScenario")
        Try
            While reader.Read()
                dtSFM.Add(CInt(reader("ScenarioID")), New cScenarioFleetMap(CInt(reader("InRow")), CInt(reader("InCol"))))
            End While
            db.ReleaseReader(reader)
        Catch ex As Exception
        End Try
        reader = Nothing

        ' Read port maps
        reader = db.GetReader("SELECT * FROM EcospaceScenarioFleetMap")
        Try
            While reader.Read()
                sfm = dtSFM(CInt(reader("ScenarioID")))
                sfm.SetPort(CInt(reader("FleetID")), CStr(reader("Port")))
            End While
            db.ReleaseReader(reader)
        Catch ex As Exception
        End Try
        reader = Nothing

        bSucces = db.Execute("DROP TABLE EcospaceScenarioFleetMap")

        bSucces = bSucces And db.Execute("CREATE TABLE EcospaceScenarioFleetMap (ScenarioID LONG, FleetID LONG, PortID LONG, InRow INTEGER, InCol INTEGER)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioFleetMap ADD PRIMARY KEY (ScenarioID, FleetID, PortID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioFleetMap ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioFleetMap ADD FOREIGN KEY (FleetID) REFERENCES EcospaceScenarioFleet(FleetID)")

        writer = db.GetWriter("EcospaceScenarioFleetMap")
        For Each iScenarioID As Integer In dtSFM.Keys
            sfm = dtSFM(iScenarioID)
            For Each iFleetID As Integer In sfm.FleetIDs
                For iRow As Integer = 1 To sfm.InRow
                    For iCol As Integer = 1 To sfm.InCol
                        If sfm.HasPort(iFleetID, iRow, iCol) Then
                            Try
                                drow = writer.NewRow()
                                drow("ScenarioID") = iScenarioID
                                drow("FleetID") = iFleetID
                                drow("InRow") = iRow
                                drow("InCol") = iCol
                                drow("PortID") = iPortID
                                writer.AddRow(drow)
                                iPortID += 1 ' Haha
                            Catch ex As Exception
                                bSucces = False
                            End Try
                        End If
                    Next iCol
                Next iRow
            Next iFleetID

        Next iScenarioID
        db.ReleaseWriter(writer, bSucces)

        Return bSucces

    End Function

End Class
