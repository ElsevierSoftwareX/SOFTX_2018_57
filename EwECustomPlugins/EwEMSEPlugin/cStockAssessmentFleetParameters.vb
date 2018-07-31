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

Option Strict On
Option Explicit On

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

#Region "Imports"

Imports EwECore

Imports EwEUtils.Utilities

#End Region

Public Class cStockAssessmentFleetParameters
    Private m_Assessment As cStockAssessmentModel
    Private m_epData As cEcopathDataStructures
    Private m_iFlt As Integer

    Public Property isLoading As Boolean

    Public Event onParameterChanged(ByVal iGroupIndex As Integer)

    Public Sub New(ByVal iFleet As Integer, ByVal StockAssessmentModel As cStockAssessmentModel, EcoPathData As cEcopathDataStructures)
        Me.m_iFlt = iFleet
        Me.m_Assessment = StockAssessmentModel
        Me.m_epData = EcoPathData
    End Sub


    Public ReadOnly Property Name As String
        Get
            Return Me.m_epData.FleetName(Me.m_iFlt)
        End Get
    End Property


    Public Property cvImpError As Single
        Get
            Return Me.m_Assessment.CVImpError(Me.m_iFlt)
        End Get
        Set(value As Single)
            Me.m_Assessment.CVImpError(Me.m_iFlt) = value
        End Set
    End Property

    Public ReadOnly Property iFleetIndex As Integer
        Get
            Return Me.m_iFlt
        End Get
    End Property

    Public Function FromCSVString(csvBuffer As String) As Boolean
        Dim recs() As String
        recs = cStringUtils.SplitQualified(csvBuffer, ",")

        Me.isLoading = True
        Me.m_iFlt = cStringUtils.ConvertToInteger(recs(1))
        Me.cvImpError = cStringUtils.ConvertToSingle(recs(2))

        Debug.Assert(recs(0).Contains(Me.Name), "Oppsss Names do not match. Could be a problem reading Fleets from StockAssessment file.")
        Me.isLoading = False

        Return True

    End Function


    Public Function toCSVString() As String
        Return cStringUtils.ToCSVField(Me.Name) + "," + cStringUtils.ToCSVField(Me.iFleetIndex) + "," + _
            cStringUtils.ToCSVField(Me.cvImpError)
    End Function


    Public Shared Function toCSVHeader() As String
        Return "'FleetName','FleetIndex','FleetImplementationError'"
    End Function
End Class


