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

#End Region ' Imports 

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.60.0.06:</para>
''' <para>
''' An error was identified in the database loading logic. This update cannot 
''' apply any fixes as the bug obscures the users intentions. The update thus 
''' merely checks wich scenarios may have been affected and warns the user.
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_60_00_07
    Inherits cDBUpdate

    Private m_strAction As String = ""

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.600007!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Validated Ecospace MPAs"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        ' ToDo: globalize this method

        ' Abort if no possible issues
        If (CInt(db.Execute("SELECT COUNT(*) FROM EcospaceScenarioMPAFishery")) = 0) Then Return True

        ' Find all scenario IDs where the MPA fishery refers to fleet IDs that differ between Ecopath and Ecospace
        Dim rd As IDataReader = db.GetReader("SELECT DISTINCT(ScenarioID) FROM EcospaceScenarioMPAFishery AS M WHERE EXISTS (SELECT FleetID FROM EcospaceScenarioFleet WHERE ScenarioID = M.ScenarioID AND M.FleetID = FleetID AND M.FleetID <> EcopathFleetID)")
        Dim lID As New List(Of Integer)
        While rd.Read
            lID.Add(CInt(rd("ScenarioID")))
        End While
        db.ReleaseReader(rd)

        ' Abort if no possible issues
        If (lID.Count = 0) Then Return True

        Dim strScenarios As String = ""
        lID.Sort()
        For i As Integer = 0 To lID.Count - 1
            Dim strScenario As String = "'" & CStr(db.GetValue("SELECT ScenarioName FROM EcospaceScenario WHERE ScenarioID=" & lID(i))) & "'"
            If Not String.IsNullOrWhiteSpace(strScenario) Then
                strScenarios = strScenario & ", "
            Else
                strScenarios = strScenario
            End If
        Next

        Me.m_strAction = cStringUtils.Localize(My.Resources.CoreMessages.UPDATE_600007_ERROR, strScenarios)
        Return True

    End Function

    Public Overrides ReadOnly Property UserAction As String
        Get
            Return Me.m_strAction
        End Get
    End Property

End Class
