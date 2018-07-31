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
''' <para>Database update 6.120008:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added Ecosim effort conversion factor.</description></item>
''' <item><description>Added taxon growth parameters.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_12_00008
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.120008!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Added Ecosim effort conversion factor" & Environment.NewLine & "Added taxon growth parameters"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Return Me.UpdateTaxa(db) And Me.UpdateEcosimGroups(db)

    End Function

    Public Function UpdateTaxa(ByRef db As cEwEDatabase) As Boolean

        Return db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN Winf SINGLE") And _
               db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN vbgfK SINGLE")

    End Function

    Public Function UpdateEcosimGroups(ByRef db As cEwEDatabase) As Boolean

        Return db.Execute("ALTER TABLE EcoSimScenarioFleet ADD COLUMN EffortConversionFactor SINGLE")

    End Function

End Class
