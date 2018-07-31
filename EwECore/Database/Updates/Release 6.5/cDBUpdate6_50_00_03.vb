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
''' <para>Database update 6.50.0.02:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Re-issued 6.4.04 to fix development time updates</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_03
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500003!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Enable multiple connections for a single dataset layer"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim strKey As String = db.GetPkKeyName("EcospaceScenarioDataAdapters")
        Dim bSuccess As Boolean = Not String.IsNullOrWhiteSpace(strKey)

        Try
            bSuccess = db.Execute("ALTER TABLE EcospaceScenarioDataAdapters ADD COLUMN ConnectionIndex INTEGER")
            bSuccess = bSuccess And db.Execute("UPDATE EcospaceScenarioDataAdapters SET ConnectionIndex=1")
            If db.Execute("ALTER TABLE EcospaceScenarioDataAdapters DROP CONSTRAINT " & strKey) Then
                bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioDataAdapters ADD CONSTRAINT pk PRIMARY KEY (ScenarioID, VarName, LayerIndex, ConnectionIndex)")
            End If
        Catch ex As Exception
            bSuccess = False
        End Try
        Me.LogProgress(Me.UpdateDescription, True)
        Return True

    End Function
End Class
