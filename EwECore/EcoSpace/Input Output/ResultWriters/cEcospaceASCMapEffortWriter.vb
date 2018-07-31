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
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Implementation of <see cref="IEcospaceResultsWriter"/> to write Ecospace effort
''' distributions maps to ESRI ASCII files. 
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcospaceASCMapEffortWriter
    Inherits cEcospaceASCBaseResultsWriter

    Public Sub New()
        MyBase.New()
        Me.vars = New eVarNameFlags() {eVarNameFlags.EcospaceMapEffort}
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ''For debugging CreateInstance exceptions
        'Throw New Exception("Test Exception cEcospaceASCMapEffortWriter.New()")
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    End Sub

    Public Overrides Sub Init(theCore As Object)
        MyBase.Init(theCore)
        Me.setAllFleetsSelected()
    End Sub

    Protected Overrides Function NMaps() As Integer
        Return Me.EcopathData.NumFleet
    End Function

    Public Overrides ReadOnly Property DisplayName As String
        Get
            Return My.Resources.CoreDefaults.ECOSPACE_WRITER_ASC_EFFORT
        End Get
    End Property

    Protected Overrides Function GetFileName(ByVal varname As eVarNameFlags,
                                             ByVal iGrp As Integer,
                                             ByVal strExt As String,
                                             Optional ByVal iModelTimeStep As Integer = cCore.NULL_VALUE) As String
        Return MyBase.GetFleetFileName(varname, iGrp, strExt, iModelTimeStep)
    End Function

End Class


