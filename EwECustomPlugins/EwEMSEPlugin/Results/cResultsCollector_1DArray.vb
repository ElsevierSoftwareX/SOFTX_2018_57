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

Public MustInherit Class cResultsCollector_1DArray
    Inherits cResultsCollector_Base

    Private m_DataArray(,,) As Object
    Protected m_MSE As cMSE

    Public Sub New()
        MyBase.New()
    End Sub

    Public MustOverride ReadOnly Property Dim_Name As String

    Public MustOverride ReadOnly Property nElements As Integer

    Public MustOverride ReadOnly Property ElementName(iElement As Integer) As String

    Public MustOverride ReadOnly Property GetValue_Formatted4CSV(iStrategy As Integer, ByVal iElement As Integer, iTime As Integer) As Object

    Public Overrides Sub Initialise(MSE As cMSE)
        m_MSE = MSE
        SetSize(MSE.Strategies.Count, nElements, NumberOfTimeRecords)
    End Sub

    Protected ReadOnly Property GetValue(ByVal iStrategy As Integer, ByVal iElement As Integer, ByVal iTime As Integer) As Object
        Get
            Return m_DataArray(iStrategy - 1, iElement - 1, iTime - 1)
        End Get
    End Property


    Protected WriteOnly Property SetValue(ByVal iStrategy As Integer, ByVal iElement As Integer, ByVal iTime As Integer) As Object
        Set(value As Object)
            m_DataArray(iStrategy - 1, iElement - 1, iTime - 1) = value
        End Set
    End Property

    Protected Overrides Sub SetDefaults(ByVal DefaultValue As Object)
        For iStrategy = 1 To m_nStrategies
            For iElement = 1 To nElements
                For iTime = 1 To NumberOfTimeRecords
                    Me.SetValue(iStrategy, iElement, iTime) = DefaultValue
                Next
            Next
        Next
    End Sub

    Protected Sub SetSize(nStrategy As Integer, nElement As Integer, nTime As Integer)
        ReDim m_DataArray(nStrategy - 1, nElement - 1, nTime - 1)
        m_nStrategies = nStrategy
    End Sub

End Class
