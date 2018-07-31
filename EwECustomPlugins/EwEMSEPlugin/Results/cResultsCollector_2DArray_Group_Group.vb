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

Public MustInherit Class cResultsCollector_2DArray_Group_Group
    Inherits cResultsCollector_Base

    Private m_DataArray(,,,) As Object
    Protected m_MSE As cMSE

    Public MustOverride ReadOnly Property TotalAcrossPred As Boolean

    Public MustOverride ReadOnly Property TotalAcrossPrey As Boolean

    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides Sub Initialise(MSE As cMSE)

        m_MSE = MSE
        SetSize(MSE.Strategies.Count, Me.nPred, Me.nPrey, NumberOfTimeRecords)

    End Sub

    Public ReadOnly Property nPred As Integer
        Get
            Return m_MSE.Core.nGroups
        End Get
    End Property

    Public ReadOnly Property nPrey As Integer
        Get
            Return m_MSE.Core.nGroups
        End Get
    End Property

    Public ReadOnly Property GetValue(ByVal iStrategy As Integer, ByVal iPred As Integer, ByVal iPrey As Integer,
                                      ByVal iTime As Integer) As Object
        Get
            Return m_DataArray(iStrategy, iPred, iPrey, iTime)
        End Get
    End Property

    Protected WriteOnly Property SetValue(ByVal iStrategy As Integer, ByVal iPred As Integer, ByVal iPrey As Integer,
                                          ByVal iTime As Integer) As Object
        Set(value As Object)
            m_DataArray(iStrategy, iPred, iPrey, iTime) = value
        End Set
    End Property

    Protected Overrides Sub SetDefaults(ByVal DefaultValue As Object)
        For iStrategy = 0 To m_nStrategies
            For iPred = 0 To Me.nPred
                For iPrey = 0 To Me.nPrey
                    For iTime = 0 To NumberOfTimeRecords
                        Me.SetValue(iStrategy, iPred, iPrey, iTime) = DefaultValue
                    Next
                Next
            Next
        Next
    End Sub

    Protected Sub SetSize(nStrategy As Integer, nPred As Integer, nPrey As Integer, nTime As Integer)
        ReDim m_DataArray(nStrategy, nPred, nPrey, nTime)
        m_nStrategies = nStrategy
    End Sub
End Class

