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
''' <para>Database update 6.1.1.004:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Split Ecospace IDH_UL field in two components.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_01_004
    Inherits cDBUpdate

    Private Class cModelLocation
        Private m_iScenarioID As Integer
        Private m_sMinLat As Single
        Private m_sMinLon As Single
        Private m_sCellSize As Single

        Public Sub New(ByVal iScenarioID As Integer, _
                       ByVal lUDH_UL As Long, _
                       ByVal sLat As Single, _
                       ByVal sLon As Single, _
                       ByVal sIDH_SS As Single, _
                       ByVal sCellSize As Single)

            Me.m_iScenarioID = iScenarioID

            If lUDH_UL > 0 Then
                Dim iCut As Integer = CInt(Math.Floor(lUDH_UL / 10000))
                Me.m_sMinLat = (iCut / 10.0!) - 90.0!
                Me.m_sMinLon = (lUDH_UL - (iCut * 10000)) / 10.0! - 180.0!
            Else
                Me.m_sMinLat = sLat
                Me.m_sMinLon = sLon
            End If

            If sIDH_SS = 0 Then sIDH_SS = 2
            If (sCellSize <= 0) Then sCellSize = 1.0! / sIDH_SS
            Me.m_sCellSize = sCellSize

        End Sub

        Public ReadOnly Property ScenarioID() As Integer
            Get
                Return Me.m_iScenarioID
            End Get
        End Property

        Public ReadOnly Property MinLon() As Single
            Get
                Return Me.m_sMinLon
            End Get
        End Property

        Public ReadOnly Property MinLat() As Single
            Get
                Return Me.m_sMinLat
            End Get
        End Property

        Public ReadOnly Property CellLength() As Single
            Get
                Return Me.m_sCellSize
            End Get
        End Property

    End Class

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
            Return 6.101004!
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
            Return "Replaced Ecospace IDH_UL, IDH_SS fields with proper geospatial fields."
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim bSucces As Boolean = True
        Dim sLonDefault As Single = 0.0
        Dim sLatDefault As Single = 0.0
        Dim lModels As New List(Of cModelLocation)
        Dim loc As cModelLocation = Nothing

        ' Get default lat and lon
        reader = db.GetReader("SELECT * FROM EcopathModel")
        reader.Read()
        Try
            sLonDefault = CSng(reader("MinLon"))
            sLatDefault = CSng(reader("MinLat"))
        Catch ex As Exception
            ' Swallow this
        End Try
        db.ReleaseReader(reader)

        ' Read all maps
        reader = db.GetReader("SELECT * FROM EcospaceScenario")
        Try
            While reader.Read
                loc = New cModelLocation(CInt(reader("ScenarioID")), _
                                         CLng(reader("IDH_UL")), _
                                         sLatDefault, _
                                         sLonDefault, _
                                         CSng(reader("IDH_SS")), _
                                         CSng(reader("CellLength")))
                lModels.Add(loc)
            End While
        Catch ex As Exception
            Me.LogProgress(ex.Message, False)
            bSucces = False
        End Try
        db.ReleaseReader(reader)

        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario DROP COLUMN IDH_UL")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario DROP COLUMN IDH_SS")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN MinLon SINGLE")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN MinLat SINGLE")

        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcospaceScenario")
        Dim dt As DataTable = writer.GetDataTable

        For Each loc In lModels
            Dim drow As DataRow = dt.Select("ScenarioID=" & loc.ScenarioID)(0)
            drow.BeginEdit()
            drow("MinLon") = loc.MinLon
            drow("MinLat") = loc.MinLat
            drow("CellLength") = loc.CellLength
            drow.EndEdit()
        Next
        bSucces = db.ReleaseWriter(writer)

        Return bSucces

    End Function

End Class
