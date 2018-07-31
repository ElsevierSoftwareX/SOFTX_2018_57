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

#Region "Imports Complier directives"

Option Strict On

Imports System.IO
Imports System.Text
Imports System.Threading

Imports EwECore
Imports EwECore.Ecosim
Imports EwECore.ExternalData
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwECore.SearchObjectives
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwECore.ValueWrapper

#End Region

Namespace MSY

    ''' <summary>
    ''' Parameters to configure an MSY search.
    ''' </summary>
    Public Class cMSYParameters
        Inherits cCoreInputOutputBase

#Region " Private variables "

        Private m_msyData As cMSYDataStructures

#End Region ' Private variables

#Region "Construction Initialization"

        Public Sub New(ByVal theCore As cCore, MSYData As cMSYDataStructures)
            MyBase.New(theCore)

            Dim val As cValue = Nothing

            Me.m_dataType = eDataTypes.MSYParameters
            Me.m_coreComponent = eCoreComponentType.MSY

            'create and set the status object to this source and OK
            Me.m_ValidationStatus = New cVariableStatus
            Me.m_ValidationStatus.CoreDataObject = Me
            Me.AllowValidation = False

            Me.DBID = cCore.NULL_VALUE

            Me.m_msyData = MSYData

            ' FSelection
            val = New cValue(New Integer, eVarNameFlags.MSYFSelection, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' FSelectionMode
            val = New cValue(New Integer, eVarNameFlags.MSYFSelectionMode, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' Assessment (frozen pools or not)
            val = New cValue(New Boolean, eVarNameFlags.MSYAssessment, eStatusFlags.Null, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            ' RunLengthMode
            val = New cValue(New Integer, eVarNameFlags.MSYRunLengthMode, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' MaxFishingRate
            val = New cValue(New Single, eVarNameFlags.MSYMaxFishingRate, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' NumTrialYears
            val = New cValue(New Integer, eVarNameFlags.MSYNumTrialYears, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' EquilibriumStepSize
            val = New cValue(New Single, eVarNameFlags.MSYEquilibriumStepSize, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

        End Sub

#End Region

#Region " .operators "

        Public Property SelGroupFleetIndex As Integer
            Set(value As Integer)
                Me.m_msyData.iSelGroupFleet = value
            End Set
            Get
                Return Me.m_msyData.iSelGroupFleet
            End Get
        End Property

        Public Property FSelectionMode As eMSYFSelectionModeType
            Get
                Return Me.m_msyData.FSelectionMode
            End Get
            Set(value As eMSYFSelectionModeType)
                Me.m_msyData.FSelectionMode = value
            End Set
        End Property

        Public Property Assessment As eMSYAssessmentTypes
            Set(value As eMSYAssessmentTypes)
                Me.m_msyData.AssessmentType = value
            End Set
            Get
                Return Me.m_msyData.AssessmentType
            End Get
        End Property

        Public Property RunLengthMode As eMSYRunLengthModeTypes
            Get
                Return Me.m_msyData.RunLengthMode
            End Get
            Set(value As eMSYRunLengthModeTypes)
                Me.m_msyData.RunLengthMode = value
            End Set
        End Property

        Public Property MaxFishingRate As Single
            Get
                Return Me.m_msyData.MaxRelF
            End Get
            Set(value As Single)
                Me.m_msyData.MaxRelF = value
            End Set
        End Property

        Public Property NumTrialYears As Integer
            Get
                Return Me.m_msyData.nYearsPerTrial
            End Get
            Set(value As Integer)
                Me.m_msyData.nYearsPerTrial = value
            End Set
        End Property

        Public Property EquilibriumStepSize As Single
            Get
                Return Me.m_msyData.FStepSize
            End Get
            Set(value As Single)
                Me.m_msyData.FStepSize = value
            End Set
        End Property

#End Region ' .operators

    End Class

End Namespace
