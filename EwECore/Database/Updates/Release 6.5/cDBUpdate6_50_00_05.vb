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
''' <para>Database update 6.50.0.05:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Enabled monthly time series</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_05
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500005!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Enabled monthly time series"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True

        bSucces = db.Execute("ALTER TABLE EcosimTimeSeriesDataset ADD COLUMN DataInterval LONG")
        bSucces = bSucces And db.Execute("UPDATE EcosimTimeSeriesDataset SET DataInterval = " & CStr(CInt(eTSDataSetInterval.Annual)))
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimTimeSeriesDataset ADD COLUMN NumPoints LONG")
        bSucces = bSucces And db.Execute("UPDATE EcosimTimeSeriesDataset SET NumPoints = NumYears")
        bSucces = bSucces And db.Execute("ALTER TABLE EcosimTimeSeriesDataset DROP COLUMN NumYears")
        Return bSucces

    End Function
End Class
