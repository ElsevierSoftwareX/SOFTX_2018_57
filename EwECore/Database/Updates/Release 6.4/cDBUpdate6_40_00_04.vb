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
''' <para>Database update 6.40.0.04:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Fixed ref integrity to Ecospace groups (not Ecopath)</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_40_00_04
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.400004!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Fixed capacity map constraints"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = True
        Dim strKey As String = db.GetFkKeyName("EcopathGroup", "EcospaceScenarioCapacityDrivers", "GroupID")

        If Not String.IsNullOrWhiteSpace(strKey) Then
            bSuccess = False
            If db.Execute("ALTER TABLE EcospaceScenarioCapacityDrivers DROP CONSTRAINT " & strKey) Then
                bSuccess = db.Execute("ALTER TABLE EcospaceScenarioCapacityDrivers ADD CONSTRAINT fkGroupID FOREIGN KEY (GroupID) REFERENCES EcospaceScenarioGroup (GroupID)")
            End If
        End If

        Me.LogProgress("UpdateEcospaceScenarioCapacityDrivers", bSuccess)

        Return bSuccess

    End Function

End Class
