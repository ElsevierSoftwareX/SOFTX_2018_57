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

#If 0 Then

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.60.0.09:</para>
''' <para>
''' Adds new shape data to Ecosim, indexed by fleet x group.
''' </para>
''' </summary>
''' <remarks>
''' VC + JB + JS decided this was needed in June 2018, Vancouver. JS can't really 
''' recall what it was for. Candidate update has been disabled until it is needed again.
''' </remarks>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_60_00_09
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.600009!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Added fleet x group shape support"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = True

        ' Add Ecosim fleet x group shape table
        bSuccess = bSuccess And db.Execute("CREATE TABLE EcosimScenarioFleetGroupShape (ScenarioID LONG, GroupID LONG, FleetID LONG, ShapeID LONG)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioFleetGroupShape ADD PRIMARY KEY (ScenarioID, GroupID, FleetID, ShapeID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioFleetGroupShape ADD FOREIGN KEY (ScenarioID) REFERENCES EcosimScenario(ScenarioID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioFleetGroupShape ADD FOREIGN KEY (GroupID) REFERENCES EcosimScenarioGroup(GroupID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioFleetGroupShape ADD FOREIGN KEY (FleetID) REFERENCES EcosimScenarioFleet(FleetID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenarioFleetGroupShape ADD FOREIGN KEY (ShapeID) REFERENCES EcosimShape(ShapeID)")

        Return bSuccess

    End Function

End Class

#End If
