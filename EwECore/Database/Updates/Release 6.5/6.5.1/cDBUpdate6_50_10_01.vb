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
''' <para>Database update 6.50.1.01:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Updated Ecotracer</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_10_01
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.501001!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Updated Ecotracer"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        If db.Execute("ALTER TABLE EcotracerScenarioGroup ADD COLUMN CassimProp SINGLE") And
           db.Execute("ALTER TABLE EcotracerScenarioGroup ADD COLUMN CmetabolismRate SINGLE") Then

            db.Execute("UPDATE EcotracerScenarioGroup SET CassimProp = Cexcretionrate")
            db.Execute("UPDATE EcotracerScenarioGroup SET CmetabolismRate = 1")
            db.DropColumn("EcotracerScenarioGroup", "Cexcretionrate")

            Return True
        End If
        Return False

    End Function


End Class
