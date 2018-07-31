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

#Region "Enumerators"

Public Enum eInitOption As Integer
    EcopathBaseF
    CurrentF
    RandomF
End Enum

Public Enum eOptimizeOptionTypes As Integer
    BatchRun
    MaxPortUtil
    PrevCostEarning
    UseEcospace
    IncludeComp
End Enum

Public Enum eSearchOptionTypes As Integer
    Fletch
    DFPmin
    BaseProfitability
End Enum

Public Enum eOptimizeApproachTypes As Integer
    SystemObjective
    FleetValues
End Enum

#End Region

#Region "Fishing Policy parameters"

Public Class cFishingPolicyParameters
    Inherits cCoreInputOutputBase


    Public Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)

        Me.AllowValidation = False
        Me.DBID = DBID
        Me.m_dataType = eDataTypes.FishingPolicyParameters
        Me.m_coreComponent = eCoreComponentType.FishingPolicySearch
        AllowValidation = False

        'default OK status used for setVariable
        'see comment setVariable(...)
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        Dim val As cValue

        'FPSMaxNumEval
        val = New cValue(New Single, eVarNameFlags.FPSMaxNumEval, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'FPSMaxEffChange
        val = New cValue(New Single, eVarNameFlags.FPSMaxEffChange, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'xxxxxxxxxxxxxxxxxxxxxx
        'Enumerators are stored as Integer!!
        'FPSInitOption 
        val = New cValue(New Integer, eVarNameFlags.FPSInitOption, eStatusFlags.Null, eValueTypes.Int)
        val.Stored = False
        m_values.Add(val.varName, val)

        'FPSSearchOption
        val = New cValue(New Integer, eVarNameFlags.FPSSearchOption, eStatusFlags.Null, eValueTypes.Int)
        val.Stored = False
        m_values.Add(val.varName, val)

        'FPSOptimizeApproach
        val = New cValue(New Integer, eVarNameFlags.FPSOptimizeApproach, eStatusFlags.Null, eValueTypes.Int)
        val.Stored = False
        m_values.Add(val.varName, val)

        'FPSOptimizeApproach
        val = New cValue(New Integer, eVarNameFlags.FPSOptimizeOptions, eStatusFlags.Null, eValueTypes.Int)
        val.Stored = False
        m_values.Add(val.varName, val)

        'Number of runs 500 Max ???
        val = New cValue(New Integer, eVarNameFlags.FPSNRuns, eStatusFlags.Null, eValueTypes.Int)
        val.Stored = False
        m_values.Add(val.varName, val)

        'Me.AllowValidation = True

        'Boolean parameters
        'FPSMaxPortUtil
        val = New cValue(New Boolean, eVarNameFlags.FPSMaxPortUtil, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        ''FPSPrevCostEarning
        'meta = New cVariableMetaData()
        'val = New cValue(New Boolean, eVarNameFlags.SearchPrevCostEarning, eStatusFlags.Null, eValueTypes.Bool)
        'val.Stored = False
        'm_values.Add(val.varName, val)

        'FPSIncludeComp
        val = New cValue(New Boolean, eVarNameFlags.FPSIncludeComp, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        'FPSBatchRun
        val = New cValue(New Boolean, eVarNameFlags.FPSBatchRun, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        'FPSUseEcospace
        val = New cValue(New Boolean, eVarNameFlags.FPSUseEcospace, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.FPSUseEconomicPlugin, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        'meta = New cVariableMetaData()
        'val = New cValue(New Boolean, eVarNameFlags.isEconomicAvailable, eStatusFlags.Null, eValueTypes.Bool)
        'val.Stored = False
        'm_values.Add(val.varName, val)

        Me.ResetStatusFlags()

        AllowValidation = True

    End Sub

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean

        If Not MyBase.ResetStatusFlags(bForceReset) Then Return False
        Me.m_core.Set_EconomicAvailable_Flags(Me, eVarNameFlags.FPSUseEconomicPlugin)
        Return True

    End Function

    Public Property InitOption() As eInitOption
        Get
            Return CType(GetVariable(eVarNameFlags.FPSInitOption), eInitOption)
        End Get

        Set(ByVal value As eInitOption)
            SetVariable(eVarNameFlags.FPSInitOption, value)
        End Set
    End Property


    Public Property MaxNumEval() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.FPSMaxNumEval), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.FPSMaxNumEval, value)
        End Set
    End Property


    Public Property MaxEffChange() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.FPSMaxEffChange), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.FPSMaxEffChange, value)
        End Set
    End Property


    Public Property SearchOption() As eSearchOptionTypes
        Get
            Return CType(GetVariable(eVarNameFlags.FPSSearchOption), eSearchOptionTypes)
        End Get

        Set(ByVal value As eSearchOptionTypes)
            SetVariable(eVarNameFlags.FPSSearchOption, value)
        End Set
    End Property


    Public Property OptimizeApproach() As eOptimizeApproachTypes
        Get
            Return CType(GetVariable(eVarNameFlags.FPSOptimizeApproach), eOptimizeApproachTypes)
        End Get

        Set(ByVal value As eOptimizeApproachTypes)
            SetVariable(eVarNameFlags.FPSOptimizeApproach, value)
        End Set
    End Property

    Public Property nRuns() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.FPSNRuns))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.FPSNRuns, value)
        End Set
    End Property


    Public Property MaxPortUtil() As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.FPSMaxPortUtil))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.FPSMaxPortUtil, value)
        End Set
    End Property


    Public Property IncludeComp() As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.FPSIncludeComp))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.FPSIncludeComp, value)
        End Set
    End Property


    Public Property BatchRun() As Boolean
        Get
            Return False
        End Get

        Set(ByVal value As Boolean)
            Debug.Assert(False, Me.ToString & ".BatchRun() has not been implemented yet!")
        End Set
    End Property

    Public Property UseEcospace() As Boolean
        Get
            Return False
        End Get

        Set(ByVal value As Boolean)
            Debug.Assert(False, Me.ToString & ".UseEcospace() has not been implemented yet!")
        End Set
    End Property

    Public Property UseEconomicPlugin() As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.FPSUseEconomicPlugin))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.FPSUseEconomicPlugin, value)
        End Set
    End Property

    'Public Property isEconomicAvailable() As Boolean
    '    Get
    '        Return CBool(GetVariable(eVarNameFlags.isEconomicAvailable))
    '    End Get

    '    Set(ByVal value As Boolean)
    '        SetVariable(eVarNameFlags.isEconomicAvailable, value)
    '    End Set
    'End Property

End Class




#End Region


