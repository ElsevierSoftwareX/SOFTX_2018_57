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
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class cEcospaceASCMapContaminants
    Inherits cEcospaceASCBaseResultsWriter

    Public Sub New()
        MyBase.New()

        Me.vars = New eVarNameFlags() {eVarNameFlags.Concentration}
    End Sub


    Public Overrides Sub Init(theCore As Object)
        MyBase.Init(theCore)

        Me.m_selGroups = New Boolean(Me.m_core.nGroups) {}
        'Include the zero index for the environmental concentrations
        For igrp As Integer = 0 To Me.EcopathData.NumGroups
            m_selGroups(igrp) = True
        Next igrp

    End Sub

    Protected Overrides Function GetFileName(ByVal varname As eVarNameFlags,
                                                    ByVal iGrp As Integer,
                                                    ByVal strExt As String,
                                                    Optional ByVal iModelTimeStep As Integer = cCore.NULL_VALUE) As String
        Return Me.GetGroupFileName(varname, iGrp, strExt, iModelTimeStep)
    End Function


    Protected Overrides Function GetGroupFileName(ByVal varname As eVarNameFlags,
                                                    ByVal iGrp As Integer,
                                                    ByVal strExt As String,
                                                    Optional ByVal iModelTimeStep As Integer = cCore.NULL_VALUE) As String

        Dim Filename As String
        Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
        Dim strTimestep As String
        Dim grpName As String

        If iGrp > 0 Then
            grpName = Me.m_core.m_EcoPathData.GroupName(iGrp)
        Else
            grpName = "Environment"
        End If

        strTimestep = cStringUtils.Localize("-{0:00000}", iModelTimeStep)

        Filename = EwEUtils.Utilities.cFileUtils.ToValidFileName(cStringUtils.Localize("{0}-{1}{2}.{3}",
                                                                        cin.GetVarName(varname), grpName, strTimestep, strExt.Replace(".", "")), False)

        Return System.IO.Path.Combine(Me.OutputDirectory, Filename.Replace("..", "."))

    End Function


    Public Overrides Sub WriteResults(ByVal SpaceTimeStepResults As Object)

        'Only if Contaminant Tracer is ON
        If Me.m_core.m_tracerData.EcoSpaceConSimOn Then
            MyBase.WriteResults(SpaceTimeStepResults)
        End If

    End Sub


    Public Overrides ReadOnly Property DisplayName As String
        Get
            Return My.Resources.CoreDefaults.ECOSPACE_WRITER_ASC_CONTAMINANTS
        End Get
    End Property

End Class
