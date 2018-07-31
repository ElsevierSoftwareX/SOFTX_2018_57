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
''' <para>Database update 6.50.0.04:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Re-issued 6.4.04 to fix development time updates</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_04
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500004!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Reworked storage of Ecospace external data connections"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
        Dim reader As IDataReader = Nothing
        Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        Dim bSucces As Boolean = db.Execute("CREATE TABLE EcospaceScenarioDataConnection (ScenarioID LONG, VarName TEXT(50), LayerID LONG, Sequence INTEGER, DatasetGUID TEXT(140), DatasetTypeName TEXT(255), DatasetCfg MEMO, ConverterTypeName TEXT(255), ConverterCfg MEMO, Scale SINGLE, ScaleType BYTE)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioDataConnection ADD PRIMARY KEY (ScenarioID, VarName, LayerID, Sequence)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcospaceScenarioDataConnection ADD FOREIGN KEY (ScenarioID) REFERENCES EcospaceScenario(ScenarioID)")

        ' -- Try to migrate existing connections --

        ' First: unindexed layers
        reader = db.GetReader(String.Format("SELECT * FROM EcospaceScenarioDataConnection WHERE (Varname='{0}') OR (VarName='{1}') OR (VarName='{2}')", _
                                            cin.GetVarName(EwEUtils.Core.eVarNameFlags.LayerDepth), _
                                            cin.GetVarName(EwEUtils.Core.eVarNameFlags.LayerRelPP), _
                                            cin.GetVarName(EwEUtils.Core.eVarNameFlags.LayerRelCin)))
        If (reader IsNot Nothing) Then
            writer = db.GetWriter("EcospaceScenarioDataConnection")
            While reader.Read()
                Dim drow As DataRow = writer.NewRow()
                drow("ScenarioID") = reader("ScenarioID")
                drow("VarName") = reader("VarName")
                drow("LayerID") = 1
                drow("Sequence") = 1
                drow("DatasetGUID") = reader("Dataset")
                drow("ConverterTypeName") = reader("Converter")
                drow("ConverterCfg") = reader("ConverterCfg")
                drow("Scale") = reader("Scale")
                drow("ScaleType") = reader("ScaleType")
                writer.AddRow(drow)
            End While
            db.ReleaseWriter(writer)
            db.ReleaseReader(reader)
        End If

        ' - Driver layers -
        reader = db.GetReader("SELECT A.ScenarioID, A.VarName, A.ConnectionIndex, A.Dataset, A.Converter, A.ConverterCfg, A.Scale, A.ScaleType, B.LayerID FROM EcospaceScenarioDataAdapters A INNER JOIN EcospaceScenarioDriverLayer B ON A.LayerIndex = B.Sequence AND A.ScenarioID = B.ScenarioID")
        If (reader IsNot Nothing) Then
            writer = db.GetWriter("EcospaceScenarioDataConnection")
            While reader.Read()
                Dim drow As DataRow = writer.NewRow()
                drow("ScenarioID") = reader("ScenarioID")
                drow("VarName") = reader("VarName")
                drow("LayerID") = reader("LayerID")
                drow("Sequence") = reader("ConnectionIndex")
                drow("DatasetGUID") = reader("Dataset")
                drow("ConverterTypeName") = reader("Converter")
                drow("ConverterCfg") = reader("ConverterCfg")
                drow("Scale") = reader("Scale")
                drow("ScaleType") = reader("ScaleType")
                writer.AddRow(drow)
            End While
            db.ReleaseWriter(writer)
            db.ReleaseReader(reader)
        End If

        ' No need to migrate biomass forcing, capacity. Not used yet.

        bSucces = bSucces And db.Execute("DROP TABLE EcospaceScenarioDataAdapters")

        Return bSucces

    End Function
End Class
