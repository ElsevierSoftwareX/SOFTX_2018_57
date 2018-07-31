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
''' <para>Database update 6.50.0.09:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Habitat capacity calculation type made per group.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_09
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating supported Ecospace habitat foraging capacity calculation methods.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Private Enum cOldCapCalcType As Integer
        ''' <summary>Only environmental responses and capacity input layers are used to calculate capacity. Habitats are ignored for this purpose</summary>
        EnvResponses = 0
        ''' <summary>Only habitats are used to calculate capacity. Capacity inputs are ignored.</summary>
        Habitat = 1
        ''' <summary>Both environmental responses and habitats are used to calculate capacity.</summary>
        Both = 2
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500009!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Added capacity calculation types per group"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        If Not db.Execute("ALTER TABLE EcospaceScenarioGroup ADD COLUMN CapacityCalType SHORT") Then
            Return False
        End If

        Dim ct As cOldCapCalcType = cOldCapCalcType.Habitat
        Dim nScenarios As Integer = CInt(db.GetValue("SELECT COUNT(*) FROM EcospaceScenario"))
        Dim hasDrivers(nScenarios) As Boolean
        Dim capmode(nScenarios) As cOldCapCalcType
        Dim iScenarioDBID(nScenarios) As Integer
        Dim iScenario As Integer = 1
        Dim bSuccess As Boolean = True

        Dim reader As IDataReader = db.GetReader("SELECT * FROM EcospaceScenario ORDER BY ScenarioID ASC")
        If (reader IsNot Nothing) Then
            While reader.Read()
                iScenarioDBID(iScenario) = CInt(reader("ScenarioID"))
                hasDrivers(iScenario) = (CInt(db.GetValue(String.Format("SELECT COUNT(*) FROM EcospaceScenarioCapacityDrivers WHERE ScenarioID={0}", iScenarioDBID(iScenario)))) > 0)
                capmode(iScenario) = DirectCast(CInt(db.ReadSafe(reader, "CapacityCalType", 0)), cOldCapCalcType)
                iScenario += 1
            End While
        End If
        db.ReleaseReader(reader)

        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcospaceScenarioGroup")
        Dim dt As DataTable = writer.GetDataTable()
        For Each drow As DataRow In dt.Rows
            iScenario = Array.IndexOf(iScenarioDBID, CInt(drow("ScenarioID")))
            If (iScenario > -1) Then
                Dim bHasCapAssignments As Boolean = (CInt(db.GetValue("SELECT COUNT (*) FROM EcospaceScenarioCapacityDrivers WHERE (GroupID=" & CStr(drow("GroupID")) & ")")) > 0)
                Select Case capmode(iScenario)

                    Case cOldCapCalcType.Both, cOldCapCalcType.EnvResponses
                        If Not bHasCapAssignments Or Not hasDrivers(iScenario) Then
                            ct = cOldCapCalcType.Habitat
                        End If

                    Case cOldCapCalcType.Habitat
                        If bHasCapAssignments And hasDrivers(iScenario) Then
                            ct = cOldCapCalcType.Both
                        End If

                End Select
            End If

            drow.BeginEdit()
            drow("CapacityCalType") = ct
            drow.EndEdit()

            bSuccess = bSuccess And writer.Commit()
        Next
        db.ReleaseWriter(writer)

        If bSuccess Then
            bSuccess = bSuccess And db.DropColumn("EcospaceScenario", "CapacityCalType")
        End If

        Return bSuccess

    End Function

End Class
