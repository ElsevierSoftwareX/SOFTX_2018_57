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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Public MustInherit Class cResultsCollector_Base

    Protected m_nStrategies As Integer
    Protected m_ModelID As Integer
    Protected m_Yearly As Boolean

    Public MustOverride ReadOnly Property NumberOfTimeRecords As Integer

    Public MustOverride Sub Initialise(MSE As cMSE)

    Public MustOverride Sub Populate()

    Public MustOverride ReadOnly Property DataName As String

    Public MustOverride ReadOnly Property FileNamePrefix As String

    Protected MustOverride ReadOnly Property DefaultValue As Object

    Protected MustOverride Sub SetDefaults(ByVal DefaultValue As Object)

    Public MustOverride ReadOnly Property Yearly As Boolean

    Public Sub New()

    End Sub

    Public ReadOnly Property nStrategies As Integer
        Get
            Return m_nStrategies
        End Get
    End Property

    Public ReadOnly Property ModelID As Integer
        Get
            Return m_ModelID
        End Get
    End Property

    Public Sub Init_for_iModel(iModel As Integer)
        m_ModelID = iModel
        SetDefaults(DefaultValue)
    End Sub

End Class
