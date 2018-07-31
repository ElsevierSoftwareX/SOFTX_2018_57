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
''' <para>Database update 6.50.0.12:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Add Ecospace monthly data table.</description></item>
''' <item><description>Add Ecospace flow map for advection.</description></item>
''' <item><description>Remvoed Ecospace advection fields that have been computed.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_12
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500012!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Added Ecospace monthly maps, flow map, and removed stored advection maps"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = True

        ' Add monthly maps table
        bSuccess = bSuccess And db.Execute("CREATE TABLE EcospaceScenarioMonth (ScenarioID LONG, MonthID BYTE, WindXVelMap MEMO, WindYVelMap MEMO)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioMonth ADD PRIMARY KEY (ScenarioID, MonthID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenarioMonth ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")

        ' Add flow field to Ecospace maps
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcospaceScenario ADD COLUMN FlowMap MEMO")

        Return bSuccess

    End Function

End Class
