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
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.0.25:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecopath sample tables</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_25
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500025!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Added Ecospace migration area movement, Ecosim environmental driver table"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Return Me.AddMigField(db) And
               Me.AddEcosimDriverTable(db) 

    End Function

    Private Function AddMigField(ByVal db As cEwEDatabase) As Boolean
        Return db.Execute("ALTER TABLE EcospaceScenarioGroup ADD COLUMN InMigAreaMovement Single")
    End Function

    Private Function AddEcosimDriverTable(ByVal db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = db.Execute("CREATE TABLE EcosimScenarioCapacityDrivers (ScenarioID LONG, GroupID LONG, DriverID LONG, ResponseID LONG)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioCapacityDrivers ADD CONSTRAINT pk PRIMARY KEY (ScenarioID, GroupID, DriverID, ResponseID)")

        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioCapacityDrivers ADD FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario (ScenarioID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioCapacityDrivers ADD FOREIGN KEY (GroupID) REFERENCES EcosimScenarioGroup (GroupID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioCapacityDrivers ADD FOREIGN KEY (DriverID) REFERENCES EcosimShape (ShapeID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioCapacityDrivers ADD FOREIGN KEY (ResponseID) REFERENCES EcosimShape (ShapeID)")

        Return bSuccess

    End Function

End Class
