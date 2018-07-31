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
Imports EwEUtils.Utilities

''' <summary>
''' Results from EcoPath for a single group.
''' </summary>
''' <remarks>
''' This class wraps the outputs from EcoPath for one group into a single object.
''' </remarks>
Public Class cEcoPathGroupOutput
    Inherits cCoreGroupBase

    Private m_nGroups As Integer
    Private m_pathData As cEcopathDataStructures
    Private m_coreData As New Dictionary(Of eVarNameFlags, IResultsWrapper)

    ' m_Area = 'A()
    ' m_Biomass = 'B()
    ' m_BiomassArea = 'BH()  Biomass/Area in t/km2 
    ' m_BioAccum = 'BA()  Biomass Accumulation 
    ' m_PB = ' PB() Production/Biomass
    ' m_QB = 'QB() consumption/biomass
    ' m_EE =
    ' m_GE = GE() 'Production/Consumption
    ' m_GS =  GS()'Unassimilated food
    ' m_DetImport = 
    ' m_predmort() = 

#Region "Functionality specific to this class"

    Private Enum eNullTestTypes As Integer
        ''' <summary>Value is not allowed to be 0 or core Null.</summary>
        NonZero
        ''' <summary>Value must be larger than 0 (and not core Null).</summary>
        GreaterThanZero
        ''' <summary>Value must not be core Null.</summary>
        NonCoreNull
    End Enum

    ''' <summary>
    ''' Set the status flag of this variable to NULL if it is less than zero
    ''' </summary>
    ''' <param name="varName">Name of the variable that will get the status flag set</param>
    ''' <param name="sValueToTest">
    ''' <para>Value of the variable to test.</para>
    ''' <para>The value is passed in so that the calling method can use either the core data structures or the output object. 
    ''' If just the eVarNameFlags is used then only the getVariable() method can be used to retrieve the value.</para>
    ''' </param>
    ''' <param name="Index">Optional variable index.</param>
    ''' <param name="nullTest">Flag stating how to test for NULL values.</param>
    Private Sub SetNullFlag(ByVal varName As eVarNameFlags, ByVal sValueToTest As Single,
            Optional ByVal Index As Integer = -9999, Optional ByVal nullTest As eNullTestTypes = eNullTestTypes.GreaterThanZero)

        Dim bIsNull As Boolean = False

        Select Case nullTest
            Case eNullTestTypes.NonZero
                'jb added test for NULL_VALUE
                bIsNull = (sValueToTest = 0.0!) Or (sValueToTest = cCore.NULL_VALUE)
            Case eNullTestTypes.GreaterThanZero
                bIsNull = (sValueToTest <= 0.0!)
            Case eNullTestTypes.NonCoreNull
                bIsNull = (sValueToTest = cCore.NULL_VALUE)
        End Select

        Try
            If bIsNull Then
                Me.SetStatusFlags(varName, eStatusFlags.Null, Index)
            Else
                Me.ClearStatusFlags(varName, eStatusFlags.Null, Index)
            End If
        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub


#End Region

#Region "Must Override Methods"

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        Dim sg As cStanzaGroup = Nothing

        MyBase.ResetStatusFlags(bForceReset)

        Try

            'Set the Status Flags to ValueComputed for input/output pairs 
            'if the modeled value is different than the input value.
            'The original data structure is needed to perform this.
            If (Not cNumberUtils.Approximates(Me.m_core.m_EcoPathData.EE(Me.Index), Me.m_core.m_EcoPathData.EEinput(Me.Index), 0.0001)) And
               (Me.m_core.m_EcoPathData.EE(Me.Index) <> (1 - Me.m_core.m_EcoPathData.OtherMortinput(Me.Index))) Then
                Me.SetStatusFlags(eVarNameFlags.EEOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.EEOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.EEOutput, m_core.m_EcoPathData.EE(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            If m_core.m_EcoPathData.PB(Me.Index) <> m_core.m_EcoPathData.PBinput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.PBOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.PBOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.PBOutput, m_core.m_EcoPathData.PB(Me.Index))

            If m_core.m_EcoPathData.QB(Me.Index) <> m_core.m_EcoPathData.QBinput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.QBOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.QBOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.QBOutput, m_core.m_EcoPathData.QB(Me.Index))

            If m_core.m_EcoPathData.GE(Me.Index) <> m_core.m_EcoPathData.GEinput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.GEOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.GEOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.GEOutput, m_core.m_EcoPathData.GE(Me.Index))

            If m_core.m_EcoPathData.B(Me.Index) <> m_core.m_EcoPathData.Binput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.Biomass, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.Biomass, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.Biomass, m_core.m_EcoPathData.B(Me.Index))

            If m_core.m_EcoPathData.BH(Me.Index) <> m_core.m_EcoPathData.BHinput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.BiomassAreaOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.BiomassAreaOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.BiomassAreaOutput, m_core.m_EcoPathData.BH(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            'Joeh
            'A in LW
            If m_core.m_PSDData.AinLW(Me.Index) <> m_core.m_PSDData.AinLWInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.AinLWOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.AinLWOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.AinLWOutput, m_core.m_PSDData.AinLW(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            'B in LW
            If m_core.m_PSDData.BinLW(Me.Index) <> m_core.m_PSDData.BinLWInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.BinLWOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.BinLWOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.BinLWOutput, m_core.m_PSDData.BinLW(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            'Loo 
            If m_core.m_PSDData.Loo(Me.Index) <> m_core.m_PSDData.LooInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.LooOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.LooOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.LooOutput, m_core.m_PSDData.Loo(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            'Winf 
            If m_core.m_PSDData.Winf(Me.Index) <> m_core.m_PSDData.WinfInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.WinfOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.WinfOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.WinfOutput, m_core.m_PSDData.Winf(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            't0
            If m_core.m_PSDData.t0(Me.Index) <> m_core.m_PSDData.t0Input(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.t0Output, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.t0Output, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.t0Output, m_core.m_PSDData.t0(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            'Tcatch
            If m_core.m_PSDData.Tcatch(Me.Index) <> m_core.m_PSDData.TcatchInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.TCatchOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.TCatchOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.TCatchOutput, m_core.m_PSDData.Tcatch(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)

            'Tmax
            If m_core.m_PSDData.Tmax(Me.Index) <> m_core.m_PSDData.TmaxInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.TmaxOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.TmaxOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.TmaxOutput, m_core.m_PSDData.Tmax(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)
            'End Joeh

            If m_core.m_EcoPathData.BA(Me.Index) <> m_core.m_EcoPathData.BAInput(Me.Index) Then
                Me.SetStatusFlags(eVarNameFlags.BioAccumOutput, eStatusFlags.ValueComputed)
            Else
                Me.ClearStatusFlags(eVarNameFlags.BioAccumOutput, eStatusFlags.ValueComputed)
            End If
            SetNullFlag(eVarNameFlags.BioAccumOutput, m_core.m_EcoPathData.BA(Me.Index), cCore.NULL_VALUE, eNullTestTypes.NonZero)

            'test for NULL values in other variables
            SetNullFlag(eVarNameFlags.BioAccumRatePerYear, Me.BioAccumRatePerYear, cCore.NULL_VALUE, eNullTestTypes.NonZero)

            SetNullFlag(eVarNameFlags.MortCoBioAcumRate, Me.MortCoBioAcumRate, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            SetNullFlag(eVarNameFlags.MortCoFishRate, Me.MortCoFishRate, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            SetNullFlag(eVarNameFlags.MortCoNetMig, Me.MortCoNetMig, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            ' This value can be negative
            SetNullFlag(eVarNameFlags.MortCoOtherMort, Me.MortCoOtherMort, cCore.NULL_VALUE, eNullTestTypes.NonCoreNull)
            SetNullFlag(eVarNameFlags.MortCoPB, Me.MortCoPB)
            SetNullFlag(eVarNameFlags.MortCoPredMort, Me.MortCoPredMort)

            ' Key indices
            SetNullFlag(eVarNameFlags.NetMigration, Me.NetMigration, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            SetNullFlag(eVarNameFlags.FlowToDet, Me.FlowToDet, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            SetNullFlag(eVarNameFlags.NetEfficiency, Me.NetEfficiency, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            SetNullFlag(eVarNameFlags.OmnivoryIndex, Me.OmnivoryIndex, cCore.NULL_VALUE, eNullTestTypes.NonZero)

            SetNullFlag(eVarNameFlags.Assimilation, Me.Assimilation)
            SetNullFlag(eVarNameFlags.Respiration, Me.Respiration)
            SetNullFlag(eVarNameFlags.RespAssim, Me.RespAssim, cCore.NULL_VALUE, eNullTestTypes.NonZero)
            SetNullFlag(eVarNameFlags.ProdResp, Me.ProdResp)
            SetNullFlag(eVarNameFlags.RespBiom, Me.RespBiom)

            For i As Integer = 1 To m_nGroups
                SetNullFlag(eVarNameFlags.Consumption, Me.Consumption(i), i)
                SetNullFlag(eVarNameFlags.PredMort, Me.PredMort(i), i, eNullTestTypes.NonZero)
                SetNullFlag(eVarNameFlags.SearchRate, Me.SearchRate(i), i, eNullTestTypes.NonZero)

                ' Set highlight on cannibalism cells (fixes bug 435)
                If i = Me.Index Then
                    Me.SetStatusFlags(eVarNameFlags.PredMort, eStatusFlags.CoreHighlight, i)
                End If
            Next

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Function

#End Region

#Region "Construction and Initialization"

    Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)

        Dim val As cValue

        'default is readonly
        m_bReadOnly = True
        AllowValidation = False

        'todo_jb f/z and m/z

        'get the number of groups from the core delegate
        m_nGroups = m_core.GetCoreCounter(eCoreCounterTypes.nGroups)
        m_dataType = eDataTypes.EcoPathGroupOutput

        ' Outputs should never send out messages
        m_coreComponent = eCoreComponentType.NotSet

        'default OK status used for SetVariable
        'see comment SetVariable(...)
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        Me.DBID = DBID
        'jb 25-Aug-2017 Allow EE-Output to be zero in the UI.
        'There is something strane going on here. This worked for some models with the default metadata operator, EE would show as zero, but not others...
        'val = New cValue(New Single, eVarNameFlags.EEOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        val = New cValue(New Single, eVarNameFlags.EEOutput, eStatusFlags.NotEditable, eValueTypes.Sng, New cVariableMetaData(0, 1, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo)))
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.PBOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.QBOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.GEOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.HabitatArea, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.BioAccumOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.Biomass, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.BiomassAreaOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.BioAccumRatePerYear, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.DetImp, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.GS, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.TTLX, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.ImportedConsumption, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        ' -- mortalities --
        val = New cValue(New Single, eVarNameFlags.MortCoPB, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MortCoFishRate, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.MortCoPredMort, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.MortCoBioAcumRate, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.MortCoNetMig, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.MortCoOtherMort, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.NetMigration, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.FlowToDet, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.NetEfficiency, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.OmnivoryIndex, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.Respiration, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.Assimilation, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.ProdResp, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.RespAssim, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.RespBiom, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        ' -- arrayed values --
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.Consumption, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.PredMort, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.SearchRate, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.Hlap, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.Plap, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.Alpha, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        ' -- TODO: VALIDATE UNITS --

        val = New cValue(New Single, eVarNameFlags.VBK, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.BiomassAvgSzWt, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.BiomassSzWt, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.TCatchOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.AinLWOutput, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.BinLWOutput, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.LooOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.WinfOutput, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.t0Output, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.TmaxOutput, eStatusFlags.NotEditable, eValueTypes.Sng, Nothing)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathWeight, eStatusFlags.NotEditable, eCoreCounterTypes.nEcopathAgeSteps, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathNumber, eStatusFlags.NotEditable, eCoreCounterTypes.nEcopathAgeSteps, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathBiomass, eStatusFlags.NotEditable, eCoreCounterTypes.nEcopathAgeSteps, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.LorenzenMortality, eStatusFlags.NotEditable, eCoreCounterTypes.nEcopathAgeSteps, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.PSD, eStatusFlags.NotEditable, eCoreCounterTypes.nWeightClasses, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.FishMortTotMort, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        val = New cValue(New Single, eVarNameFlags.NatMortPerTotMort, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

    End Sub

#End Region

#Region "Variables as Public Properties Via dot(.) operator"

    Public Property Area() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.HabitatArea))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.HabitatArea, newValue)
            End If
        End Set

    End Property

    Public Property Biomass() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Biomass))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Biomass, newValue)
            End If
        End Set

    End Property

    Public Property BiomassArea() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BiomassAreaOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.BiomassAreaOutput, newValue)
            End If
        End Set

    End Property

    Public Property BioAccum() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BioAccumOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.BioAccumOutput, newValue)
            End If
        End Set

    End Property

    Public Property BioAccumRatePerYear() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BioAccumRatePerYear))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.BioAccumRatePerYear, newValue)
            End If
        End Set

    End Property

    Public Property QBOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.QBOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.QBOutput, newValue)
            End If
        End Set

    End Property

    Public Property PBOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.PBOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.PBOutput, newValue)
            End If
        End Set

    End Property

    Public Property EEOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EEOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EEOutput, newValue)
            End If
        End Set

    End Property

    Public Property GEOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.GEOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.GEOutput, newValue)
            End If
        End Set

    End Property

    'Joeh
    Public Property VBK() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.VBK))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.VBK, newValue)
            End If
        End Set
    End Property

    Public Property BiomassAvgSzWt() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BiomassAvgSzWt))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.BiomassAvgSzWt, newValue)
            End If
        End Set

    End Property

    Public Property BiomassSzWt() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BiomassSzWt))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.BiomassSzWt, newValue)
            End If
        End Set

    End Property

    Public Property TcatchOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TCatchOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.TCatchOutput, newValue)
            End If
        End Set

    End Property

    Public Property AinLWOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.AinLWOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.AinLWOutput, newValue)
            End If
        End Set

    End Property

    Public Property BinLWOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BinLWOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.BinLWOutput, newValue)
            End If
        End Set

    End Property

    Public Property LooOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.LooOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.LooOutput, newValue)
            End If
        End Set

    End Property

    Public Property WinfOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.WinfOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.WinfOutput, newValue)
            End If
        End Set

    End Property

    Public Property t0Output() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.t0Output))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.t0Output, newValue)
            End If
        End Set

    End Property

    Public Property TmaxOutput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TmaxOutput))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.TmaxOutput, newValue)
            End If
        End Set

    End Property

    Public Property EcopathWeight(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathWeight, iTimeStep))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathWeight, newValue, iTimeStep)
            End If
        End Set
    End Property

    Public Property EcopathWeight() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.EcopathWeight), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathWeight, newValue)
            End If
        End Set
    End Property

    Public Property EcopathNumber(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathNumber, iTimeStep))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathNumber, newValue, iTimeStep)
            End If
        End Set
    End Property

    Public Property EcopathNumber() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.EcopathNumber), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathNumber, newValue)
            End If
        End Set
    End Property

    Public Property EcopathBiomass(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathBiomass, iTimeStep))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathBiomass, newValue, iTimeStep)
            End If
        End Set
    End Property

    Public Property EcopathBiomass() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.EcopathBiomass), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathBiomass, newValue)
            End If
        End Set
    End Property

    Public Property LorenzenMortality(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.LorenzenMortality, iTimeStep))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.LorenzenMortality, newValue, iTimeStep)
            End If
        End Set
    End Property

    Public Property LorenzenMortality() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.LorenzenMortality), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.LorenzenMortality, newValue)
            End If
        End Set
    End Property

    Public Property PSD(ByVal iWeightClass As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.PSD, iWeightClass))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.PSD, newValue, iWeightClass)
            End If
        End Set
    End Property

    Public Property PSD() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.PSD), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.PSD, newValue)
            End If
        End Set
    End Property

    Public Property GS() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.GS))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.GS, newValue)
            End If
        End Set
    End Property

    Public Property TTLX() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TTLX))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.TTLX, newValue)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Predation mortality on this group caused by this ipred
    ''' </summary>
    ''' <param name="iPred">iPredator group </param>
    ''' <value>Returns predation mortality on this group caused by this iPredator</value>
    ''' <remarks>
    ''' B(pred) * QB(pred) * DC(pred, prey) / B(prey) 
    '''</remarks>
    Public Property PredMort(ByVal iPred As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.PredMort, iPred))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.PredMort, newValue, iPred)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Predation mortality array
    ''' </summary>
    ''' <value>Returns an array of predation mortalities for this group</value>
    ''' <remarks> B(pred) * QB(pred) * DC(pred, prey) / B(prey) </remarks>
    Public Property PredMort() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.PredMort), Single())
        End Get

        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.PredMort, newValue)
            End If
        End Set
    End Property

    ''' <summary> PB(iGroup) </summary>
    Public Property MortCoPB() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MortCoPB))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MortCoPB, newValue)
            End If
        End Set
    End Property

    ''' <summary> Catch(i) / B(i) </summary>
    Public Property MortCoFishRate() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MortCoFishRate))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MortCoFishRate, newValue)
            End If
        End Set
    End Property

    ''' <summary> M2(i) </summary>
    Public Property MortCoPredMort() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MortCoPredMort))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MortCoPredMort, newValue)
            End If
        End Set

    End Property

    Public Property MortCoOtherMort() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MortCoOtherMort))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MortCoOtherMort, newValue)
            End If
        End Set

    End Property

    ''' <summary> BA(i) / B(i) </summary>
    Public Property MortCoBioAcumRate() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MortCoBioAcumRate))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MortCoBioAcumRate, newValue)
            End If
        End Set

    End Property

    ''' <summary> (Emigration(i) - Immig(i)) / B(i) </summary>
    Public Property MortCoNetMig() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MortCoNetMig))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MortCoNetMig, newValue)
            End If
        End Set

    End Property

    Public Property Consumption() As Single()

        Get
            Return DirectCast(GetVariable(eVarNameFlags.Consumption), Single())
        End Get

        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Consumption, newValue)
            End If
        End Set

    End Property

    Public Property Consumption(ByVal iGroup As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Consumption, iGroup))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Consumption, newValue, iGroup)
            End If
        End Set
    End Property

    Public Property ImportedConsumption() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.ImportedConsumption))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.ImportedConsumption, newValue)
            End If
        End Set

    End Property

    Public Property NetMigration() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.NetMigration))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.NetMigration, newValue)
            End If
        End Set
    End Property

    Public Property FlowToDet() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.FlowToDet))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.FlowToDet, newValue)
            End If
        End Set
    End Property

    Public Property NetEfficiency() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.NetEfficiency))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.NetEfficiency, newValue)
            End If
        End Set
    End Property

    Public Property OmnivoryIndex() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.OmnivoryIndex))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.OmnivoryIndex, newValue)
            End If
        End Set
    End Property

    Public Property Respiration() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Respiration))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Respiration, newValue)
            End If
        End Set
    End Property

    Public Property Assimilation() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Assimilation))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Assimilation, newValue)
            End If
        End Set
    End Property

    Public Property RespAssim() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.RespAssim))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.RespAssim, newValue)
            End If
        End Set
    End Property

    Public Property ProdResp() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.ProdResp))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.ProdResp, newValue)
            End If
        End Set
    End Property

    Public Property RespBiom() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.RespBiom))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.RespBiom, newValue)
            End If
        End Set
    End Property

    Public Property SearchRate(ByVal iPred As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.SearchRate, iPred))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.SearchRate, newValue, iPred)
            End If
        End Set
    End Property

    Public Property SearchRate() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.SearchRate), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.SearchRate, newValue)
            End If
        End Set
    End Property

    Public Property Hlap(ByVal iPred As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Hlap, iPred))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Hlap, newValue, iPred)
            End If
        End Set
    End Property

    Public Property Hlap() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.Hlap), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Hlap, newValue)
            End If
        End Set
    End Property

    Public Property Plap(ByVal iPred As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Plap, iPred))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Plap, newValue, iPred)
            End If
        End Set
    End Property

    Public Property Plap() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.Plap), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Plap, newValue)
            End If
        End Set
    End Property

    Public Property Alpha(ByVal iPred As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Alpha, iPred))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Alpha, newValue, iPred)
            End If
        End Set
    End Property

    ''' <summary>
    '''  Fishing mort / total mort 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>output.FishMortPerTotMort = output.MortCoFishRate / (m0 + m_EcoPathData.M2(iGroup) + output.MortCoFishRate) 'F/Z</remarks>
    Public Property FishMortPerTotMort() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.FishMortTotMort))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.FishMortTotMort, newValue)
            End If
        End Set
    End Property


    ''' <summary>
    '''  Proportion of mortality due to predation and other mort 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>output.NatMortPerTotMort = CSng(1.0 - output.FishMortPerTotMort) 'M/Z</remarks>
    Public Property NatMortPerTotMort() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.NatMortPerTotMort))
        End Get
        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.NatMortPerTotMort, newValue)
            End If
        End Set
    End Property

    Public Property Alpha() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.Alpha), Single())
        End Get
        Set(ByVal newValue As Single())
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.Alpha, newValue)
            End If
        End Set
    End Property

#End Region

#Region "Status Flags Via dot (.) operator"

    Public Property AreaStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.HabitatArea)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.HabitatArea, value)
        End Set

    End Property

    Public Property BiomassAccumStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.BioAccumOutput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BioAccumOutput, value)
        End Set

    End Property

    Public Property BiomassStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Biomass)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Biomass, value)
        End Set

    End Property

    Public Property BiomassAreaStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.BiomassAreaOutput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BiomassAreaOutput, value)
        End Set

    End Property

    Public Property EEOutputStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.EEOutput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EEOutput, value)
        End Set

    End Property

    Public Property GEOutputStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.GEOutput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.GEOutput, value)
        End Set

    End Property

    Public Property GSStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.GS)

        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.GS, value)
        End Set

    End Property

    Public Property ImportedConsumptionStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.ImportedConsumption)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.ImportedConsumption, value)
        End Set

    End Property

    Public Property MortCoBioAcumRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.MortCoBioAcumRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MortCoBioAcumRate, value)
        End Set

    End Property

    Public Property MortCoFishRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.MortCoFishRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MortCoFishRate, value)
        End Set


    End Property

    Public Property MortCoNetMigStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.MortCoNetMig)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MortCoNetMig, value)
        End Set

    End Property

    Public Property MortCoOtherMortStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.MortCoOtherMort)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MortCoOtherMort, value)
        End Set

    End Property

    Public Property MostCoPBStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.MortCoPB)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MortCoPB, value)
        End Set

    End Property

    Public Property MostCoPredMortStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.MortCoPredMort)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MortCoPredMort, value)
        End Set

    End Property

    Public Property PBStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.PBOutput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.PBInput, value)
        End Set

    End Property

    <Obsolete("Use PBStatus instead")> _
    Public Property PBOutputStatus() As eStatusFlags

        Get
            Return Me.PBStatus
        End Get

        Friend Set(ByVal value As eStatusFlags)
            Me.PBStatus = value
        End Set

    End Property

    Public Property QBStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.QBOutput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.QBOutput, value)
        End Set

    End Property

    Public Property TTLXStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.TTLX)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.TTLX, value)
        End Set

    End Property

    Public Property PredMortStatus(ByVal iPred As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.PredMort)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.PredMort, value)
        End Set

    End Property

    Public Property NetMigrationStatus(ByVal iGroup As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.NetMigration)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NetMigration, value)
        End Set

    End Property

    Public Property FlowToDetStatus(ByVal iGroup As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.FlowToDet)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.FlowToDet, value)
        End Set

    End Property

    Public Property NetEfficiencyStatus(ByVal iGroup As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.NetEfficiency)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NetEfficiency, value)
        End Set

    End Property

    Public Property OmnivoryIndexStatus(ByVal iGroup As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.OmnivoryIndex)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.OmnivoryIndex, value)
        End Set

    End Property

    Public Property RespirationStatus(ByVal iGroup As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Respiration)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Respiration, value)
        End Set

    End Property

    Public Property AssimilationStatus(ByVal iGroup As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Assimilation)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Assimilation, value)
        End Set

    End Property

    Public Property SearchRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.SearchRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.SearchRate, value)
        End Set

    End Property

    Public Property SearchRateStatus(ByVal iPred As Integer) As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.SearchRate, iPred)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.SearchRate, value, iPred)
        End Set

    End Property

#End Region

End Class


