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
Imports EwEUtils.Utilities

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.0.23:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecopath sample tables</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_23
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500023!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Addes Ecopath sample tables"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = db.Execute("CREATE TABLE EcopathSample (SampleID LONG, Hash TEXT(255), Source TEXT(255), Generated SINGLE, Rating SINGLE)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathSample ADD CONSTRAINT pk PRIMARY KEY (SampleID)")

        bSuccess = bSuccess And db.Execute("CREATE TABLE EcopathGroupSample (SampleID LONG, GroupID LONG, Biomass SINGLE, ProdBiom SINGLE, ConsBiom SINGLE, EcoEfficiency SINGLE, BiomAcc SINGLE)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupSample ADD CONSTRAINT pk PRIMARY KEY (SampleID, GroupID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupSample ADD FOREIGN KEY (SampleID) REFERENCES EcopathSample (SampleID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupSample ADD FOREIGN KEY (GroupID) REFERENCES EcopathGroup (GroupID)")

        bSuccess = bSuccess And db.Execute("CREATE TABLE EcopathDietCompSample (SampleID LONG, PredID LONG, PreyID LONG, Diet SINGLE)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathDietCompSample ADD CONSTRAINT pk PRIMARY KEY (SampleID, PredID, PreyID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathDietCompSample ADD FOREIGN KEY (SampleID) REFERENCES EcopathSample (SampleID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathDietCompSample ADD FOREIGN KEY (PredID) REFERENCES EcopathGroup (GroupID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathDietCompSample ADD FOREIGN KEY (PreyID) REFERENCES EcopathGroup (GroupID)")

        bSuccess = bSuccess And db.Execute("CREATE TABLE EcopathGroupCatchSample (SampleID LONG, GroupID LONG, FleetID LONG, Landing SINGLE, Discards SINGLE)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupCatchSample ADD CONSTRAINT pk PRIMARY KEY (SampleID, GroupID, FleetID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupCatchSample ADD FOREIGN KEY (SampleID) REFERENCES EcopathSample (SampleID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupCatchSample ADD FOREIGN KEY (GroupID) REFERENCES EcopathGroup (GroupID)")
        bSuccess = bSuccess And db.Execute("ALTER TABLE EcopathGroupCatchSample ADD FOREIGN KEY (FleetID) REFERENCES EcopathFleet (FleetID)")

        Return bSuccess

    End Function

End Class
