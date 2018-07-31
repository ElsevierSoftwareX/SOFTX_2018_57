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


''' <summary>
''' Wrapper class to hold Stock Assessment parameters
''' </summary>
''' <remarks>Used by the interface to access Stock Assessment parameters</remarks>
Public Class cStockAssessmentParameters
    Private m_Assessment As cStockAssessmentModel
    Private m_iGrp As Integer
    Private m_simdata As cEcosimDatastructures
    Private m_pathdata As cEcopathDataStructures

    Public Property isLoading As Boolean

    Public Event onParameterChanged(ByVal iGroupIndex As Integer)

    Public Sub New(ByVal iGroup As Integer, ByVal StockAssessmentModel As cStockAssessmentModel, ByVal EcoSimData As cEcosimDatastructures, ByVal EcoPathData As cEcopathDataStructures)
        Me.m_iGrp = iGroup
        Me.m_Assessment = StockAssessmentModel
        Me.m_simdata = EcoSimData
        Me.m_pathdata = EcoPathData
    End Sub

    Public Property RHalfB0Ratio As Single
        Get
            Return Me.m_Assessment.RHalfB0Ratio(Me.iGroupIndex)
        End Get
        Set(value As Single)
            Me.m_Assessment.RHalfB0Ratio(Me.iGroupIndex) = value
            FireOnChanged()
        End Set
    End Property

    Public Property ForcastGain As Single
        Get
            Return Me.m_Assessment.RstockRatio(Me.iGroupIndex)
        End Get
        Set(value As Single)
            Me.m_Assessment.RstockRatio(Me.iGroupIndex) = value
            Me.FireOnChanged()
        End Set
    End Property

    Public Property HalfRecruitmentBiomass As Single
        Get
            Return Me.m_Assessment.RstockRatio(Me.iGroupIndex)
        End Get
        Set(value As Single)
            Me.m_Assessment.RstockRatio(Me.iGroupIndex) = value
            Me.FireOnChanged()
        End Set
    End Property

    Public Property cvRec As Single
        Get
            Return Me.m_Assessment.cvRec(Me.iGroupIndex)
        End Get
        Set(value As Single)
            Me.m_Assessment.cvRec(Me.iGroupIndex) = value
            Me.FireOnChanged()
        End Set
    End Property

    Public Property CVObservationError As Single
        Get
            Return Me.m_Assessment.CVbiomEst(Me.iGroupIndex)
        End Get
        Set(value As Single)
            Me.m_Assessment.CVbiomEst(Me.iGroupIndex) = value
            Me.FireOnChanged()
        End Set
    End Property

    Public Property CVRecruitmentError As Single
        Get
            Return Me.m_Assessment.CVRecruitError(Me.iGroupIndex)
        End Get
        Set(value As Single)
            Me.m_Assessment.CVRecruitError(Me.iGroupIndex) = value
            FireOnChanged()
        End Set
    End Property


    Public ReadOnly Property Name As String
        Get
            Return Me.m_pathdata.GroupName(Me.iGroupIndex)
        End Get
    End Property

    Public ReadOnly Property iGroupIndex As Integer
        Get
            Return Me.m_iGrp
        End Get
    End Property

    Public ReadOnly Property isFished As Boolean
        Get
            Return Me.m_Assessment.Core.EcoPathGroupInputs(iGroupIndex).IsFished
        End Get
    End Property


    Private Sub FireOnChanged()

        If Me.isLoading Then Return

        Me.m_Assessment.OnParameterChanged(Me.iGroupIndex)
        Try
            RaiseEvent onParameterChanged(Me.iGroupIndex)
        Catch ex As Exception

        End Try

    End Sub

    Public Function FromCSVString(csvBuffer As String) As Boolean
        Dim recs() As String
        recs = cStringUtils.SplitQualified(csvBuffer, ",")

        Me.isLoading = True

        Me.m_iGrp = cStringUtils.ConvertToInteger(recs(1))
        Me.ForcastGain = cStringUtils.ConvertToSingle(recs(2))
        Me.RHalfB0Ratio = cStringUtils.ConvertToSingle(recs(3))
        Me.cvRec = cStringUtils.ConvertToSingle(recs(4))

        Me.CVObservationError = cStringUtils.ConvertToSingle(recs(5))
        Me.CVRecruitmentError = cStringUtils.ConvertToSingle(recs(6))

        Me.isLoading = False

        Return True

    End Function


    Public Function toCSVString() As String
        Return cStringUtils.ToCSVField(Me.Name) + "," + cStringUtils.ToCSVField(Me.iGroupIndex) + "," + _
            cStringUtils.ToCSVField(Me.ForcastGain) + "," + cStringUtils.ToCSVField(Me.RHalfB0Ratio) + "," + _
            cStringUtils.ToCSVField(Me.cvRec) + "," + cStringUtils.ToCSVField(Me.CVObservationError) + "," + _
            cStringUtils.ToCSVField(Me.CVRecruitmentError)
    End Function

    Public Shared Function toCSVHeader() As String
        Return "'GroupName','GroupIndex','Recruitment/total_pop','Bt/Ecopath_B_50%','Recruitment_CV','Biomass_Observation_Error','Kalman_Error'"
    End Function

End Class