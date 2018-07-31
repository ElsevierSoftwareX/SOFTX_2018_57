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

Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' <summary>
''' Class to encapsulate scenario parameters for a single scenario in the cEcoSim Model
''' </summary>
Public Class cEcoSimScenario
    Inherits cEwEScenario

#Region "Constructor"

    Sub New(ByVal theCore As cCore)
        MyBase.New(theCore)
        Me.m_dataType = eDataTypes.EcoSimScenario
        Me.m_ValidationStatus.DataType = Me.m_dataType
    End Sub

#End Region

    Public Overrides Function IsLoaded() As Boolean
        Return (Me.m_core.ActiveEcosimScenarioIndex = Me.Index)
    End Function

End Class
