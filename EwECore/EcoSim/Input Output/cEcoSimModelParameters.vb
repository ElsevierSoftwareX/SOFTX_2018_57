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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region ' Imports

''' <summary>
''' Contains the Model Run Parameters for EcoSim
''' i.e. 'NumberYears' number of years to run the model for
''' </summary>
''' <remarks>
''' This class is used by the interface to get/set parameters for running the EcoSim model
''' For Group related info see cEcoSimGroupInfo
''' </remarks>
Public Class cEcoSimModelParameters
    Inherits cCoreInputOutputBase

#Region " Constructor "

    Public Sub New(ByRef m_core As cCore)
        MyBase.New(m_core)

        Try
            'no data validation at this time
            Me.AllowValidation = False
            m_coreComponent = eCoreComponentType.EcoSim
            m_dataType = eDataTypes.EcoSimModelParameter

            Dim val As cValue

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            'StepSize
            val = New cValue(New Single, eVarNameFlags.StepSize, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'Discount
            val = New cValue(New Single, eVarNameFlags.Discount, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'EquilibriumStepSize
            val = New cValue(New Single, eVarNameFlags.EquilibriumStepSize, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'EquilMaxFishingRate
            val = New cValue(New Single, eVarNameFlags.EquilMaxFishingRate, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'NumStepAvg
            val = New cValue(New Single, eVarNameFlags.NumStepAvg, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'NutBaseFreeProp
            val = New cValue(New Single, eVarNameFlags.NutBaseFreeProp, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'NutPBMax
            val = New cValue(New Single, eVarNameFlags.NutPBMax, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'SystemRecovery
            val = New cValue(New Single, eVarNameFlags.SystemRecovery, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'ForagingTimeLowerLimit
            val = New cValue(New Single, eVarNameFlags.ForagingTimeLowerLimit, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'boolean
            'NudgeChecked
            val = New cValue(New Boolean, eVarNameFlags.NudgeChecked, eStatusFlags.Null, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            'UseVarPQ
            val = New cValue(New Boolean, eVarNameFlags.UseVarPQ, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            'BiomassOn
            val = New cValue(New Boolean, eVarNameFlags.BiomassOn, eStatusFlags.Null, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            ''integers
            'NutForceFunctionNumber
            val = New cValue(New Integer, eVarNameFlags.NutForceFunctionNumber, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            'EcoSimNYears max 1000 year?!
            val = New cValue(New Integer, eVarNameFlags.EcoSimNYears, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            'start summary
            val = New cValue(New Single, eVarNameFlags.EcosimSumStart, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            'end summary
            val = New cValue(New Single, eVarNameFlags.EcosimSumEnd, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            'summary num time steps
            val = New cValue(New Integer, eVarNameFlags.EcosimSumNTimeSteps, eStatusFlags.Null, eValueTypes.Int)
            val.Stored = False
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            'Contaminant tracing
            val = New cValue(New Boolean, eVarNameFlags.ConSimOnEcoSim, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            'PredictEffort
            val = New cValue(New Boolean, eVarNameFlags.PredictEffort, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            'end summary
            val = New cValue(New Single, eVarNameFlags.EcosimSORWt, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = True
            val.AffectsRunState = True
            m_values.Add(val.varName, val)

            '  Me.AllowValidation = True

        Catch ex As Exception

            Debug.Assert(False, ex.Message)
            cLog.Write(Me.ToString & ".New() Error: " & ex.Message)

        End Try

    End Sub


#End Region

#Region "Mustoverride Method implementation for this class"

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean

        MyBase.ResetStatusFlags(bForceReset)

        If (Me.m_core.ActiveEcotracerScenarioIndex >= 0) Then
            Me.ClearStatusFlags(eVarNameFlags.ConSimOnEcoSim, eStatusFlags.NotEditable)
        Else
            Me.SetStatusFlags(eVarNameFlags.ConSimOnEcoSim, eStatusFlags.NotEditable)
        End If

        'Try

        '    Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        '    Dim value As cValue

        '    For Each keyvalue In m_values
        '        value = keyvalue.Value

        '        Select Case value.varType

        '            Case eValueTypes.Sng, eValueTypes.Int

        '                If CInt(value.Value) = cCore.NULL_VALUE Then
        '                    value.Status = eStatusFlags.Null
        '                Else
        '                    value.Status = eStatusFlags.OK
        '                End If


        '            Case eValueTypes.Str

        '                If CStr(value.Value) = "" Then
        '                    value.Status = eStatusFlags.Null Or eStatusFlags.NotEditable
        '                Else
        '                    value.Status = eStatusFlags.OK Or eStatusFlags.NotEditable
        '                End If

        '            Case eValueTypes.Bool
        '                'all boolean values must be OK???????
        '                value.Status = eStatusFlags.OK

        '            Case Else
        '                Debug.Assert(False, Me.ToString & "UnKnown value type " & value.varType.ToString)

        '        End Select

        '    Next keyvalue

        '    Return True

        'Catch ex As Exception

        '    Debug.Assert(False)
        '    Return False

        'End Try

    End Function

    'Protected Overrides Sub variableValidated(ByRef variableWrapper As cValueValidationWrapper)

    'End Sub

#End Region

#Region "Variables via dot (.) operator"


    ''' <summary>
    ''' Number of years to run the EcoSim model for
    ''' </summary>
    ''' <value></value>
    ''' <remarks>
    ''' This is a property so that when the user changes NumberYears this class can tell the EcoSim model to redim 
    ''' all variables that are dimensioned by time
    ''' </remarks>
    Public Property NumberYears() As Integer

        Get
            Return CInt(GetVariable(eVarNameFlags.EcoSimNYears))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.EcoSimNYears, value)
        End Set

    End Property

    Public Property BiomassOn() As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.BiomassOn))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.BiomassOn, value)
        End Set

    End Property

    Public Property Discount() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Discount))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Discount, value)
        End Set

    End Property

    Public Property EquilibriumStepSize() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EquilibriumStepSize))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EquilibriumStepSize, value)
        End Set

    End Property

    Public Property EquilMaxFishingRate() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EquilMaxFishingRate))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EquilMaxFishingRate, value)
        End Set

    End Property

    Public Property NudgeChecked() As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.NudgeChecked))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.NudgeChecked, value)
        End Set

    End Property

    'Public Property NumStepAvg() As Single

    '    Get
    '        Return CSng(GetVariable(eVarNameFlags.NumStepAvg))
    '    End Get

    '    Set(ByVal value As Single)
    '        SetVariable(eVarNameFlags.NumStepAvg, value)
    '    End Set

    'End Property

    Public Property NutBaseFreeProp() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.NutBaseFreeProp))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.NutBaseFreeProp, value)
        End Set

    End Property

    Public Property NutForceFunctionNumber() As Integer

        Get
            Return CInt(GetVariable(eVarNameFlags.NutForceFunctionNumber))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.NutForceFunctionNumber, value)
        End Set

    End Property

    Public Property NutPBMax() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.NutPBMax))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.NutPBMax, value)
        End Set

    End Property

    Public Property StepSize() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.StepSize))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.StepSize, value)
        End Set

    End Property

    Public Property SystemRecovery() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.SystemRecovery))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.SystemRecovery, value)
        End Set

    End Property

    Public Property UseVarPQ() As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.UseVarPQ))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.UseVarPQ, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Compatibility flag. EwE up to version 6.4 limited the foraging time to 0.1, 
    ''' but this turned some ecosystems volaltile. The limit has been changed to
    ''' a min of 0.01. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ForagingTimeLowerLimit As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.ForagingTimeLowerLimit))
        End Get
        Set(value As Single)
            SetVariable(eVarNameFlags.ForagingTimeLowerLimit, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcosimDataStructures.SumStart">start</see>
    ''' of the first summary period (in years) for this model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property StartSummaryTime() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimSumStart))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimSumStart, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start of the last summary period (in years).
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EndSummaryTime() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimSumEnd))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimSumEnd, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Number to time steps to summarize the data over.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property NumberSummaryTimeSteps() As Integer

        Get
            Return CInt(GetVariable(eVarNameFlags.EcosimSumNTimeSteps))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.EcosimSumNTimeSteps, value)
        End Set

    End Property

    Public Property ContaminantTracing() As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.ConSimOnEcoSim))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.ConSimOnEcoSim, value)
        End Set

    End Property

    Public Property PredictEffort() As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.PredictEffort))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.PredictEffort, value)
        End Set

    End Property

    'Public Property RegFeedBack() As Boolean

    '    Get
    '        Return CBool(GetVariable(eVarNameFlags.RegFeedBack))
    '    End Get

    '    Set(ByVal value As Boolean)
    '        SetVariable(eVarNameFlags.RegFeedBack, value)
    '    End Set

    'End Property

    Public Property SORWt() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimSORWt))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimSORWt, value)
        End Set

    End Property

#End Region

#Region "Status via dot (.) operator"

    Public Property BiomassOnStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.BiomassOn)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BiomassOn, value)
        End Set

    End Property

    Public Property ContaminantTracingStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.ConSimOnEcoSim)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.ConSimOnEcoSim, value)
        End Set

    End Property


    Public Property PredictEffortStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.PredictEffort)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.PredictEffort, value)
        End Set

    End Property



    Public Property DiscountStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.Discount)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Discount, value)
        End Set

    End Property

    Public Property EquilibriumStepSizeStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.EquilibriumStepSize)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EquilibriumStepSize, value)
        End Set

    End Property

    Public Property EquilMaxFishingRateStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.EquilMaxFishingRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EquilMaxFishingRate, value)
        End Set

    End Property

    Public Property NudgeCheckStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.NudgeChecked)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NudgeChecked, value)
        End Set

    End Property

    Public Property NumberYearStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.EcoSimNYears)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcoSimNYears, value)
        End Set

    End Property

    Public Property NumStepAvgStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.NumStepAvg)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NumStepAvg, value)
        End Set

    End Property

    Public Property NutFreeBasePropStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.NutBaseFreeProp)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NutBaseFreeProp, value)
        End Set

    End Property

    Public Property NutForceFunctionNumberStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.NutForceFunctionNumber)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NutForceFunctionNumber, value)
        End Set

    End Property

    Public Property NutPBMaxStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.NutPBMax)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NutPBMax, value)
        End Set

    End Property

    Public Property StepSizeStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.StepSize)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.StepSize, value)
        End Set

    End Property

    Public Property SystemRecoveryStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.SystemRecovery)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.SystemRecovery, value)
        End Set

    End Property

    Public Property UseVarPQStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.UseVarPQ)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.UseVarPQ, value)
        End Set

    End Property

    Public Property SORWtStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.EcosimSORWt)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimSORWt, value)
        End Set

    End Property

#End Region

End Class
