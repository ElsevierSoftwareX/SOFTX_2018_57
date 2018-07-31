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
''' <para>Database update 6.50.0.11:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Add foraging time lower limit flag for Ecosim.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_11
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500011!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Lower foraging time limit for Ecosim can be altered"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        ' All updated models receive the former default of 0.1
        Dim bSuccess As Boolean = db.Execute("ALTER TABLE EcosimScenario ADD COLUMN ForagingTimeLowerLimit SINGLE")
        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcosimScenario")
        Dim dt As DataTable = writer.GetDataTable()
        For Each row As DataRow In dt.Rows
            row.BeginEdit()
            row("ForagingTimeLowerLimit") = 0.1!
            row.EndEdit()
        Next
        db.ReleaseWriter(writer, True)
        Me.LogProgress("Update EcosimScenario foragingtimelowerlimit", bSuccess)
        Return bSuccess

    End Function

End Class
