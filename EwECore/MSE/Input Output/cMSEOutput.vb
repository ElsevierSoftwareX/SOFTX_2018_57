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

Namespace MSE

#Region "MSE Groups outputs"

    Public Class cMSEGroupOutput
        Inherits cCoreGroupBase

        'dictionary of vars and wrappers that directly access the core data
        Private m_coreData As New Dictionary(Of eVarNameFlags, IResultsWrapper)
        Private m_MSEData As cMSEDataStructures


#Region "Construction"

        Public Sub New(ByRef theCore As cCore, ByVal MSEData As cMSEDataStructures, ByVal GroupDBID As Integer, ByVal groupIndex As Integer)
            MyBase.New(theCore)

            Dim val As cValue
            Dim meta As cVariableMetaData
            m_MSEData = MSEData

            m_dataType = eDataTypes.MSEGroupOutputs
            m_coreComponent = eCoreComponentType.MSE
            Me.DBID = GroupDBID
            Me.Index = groupIndex

            'Allow validation should be false for MSE output values
            'the status flag is set in Me.ResetStatusFlags() and should always stay the same eStatusFlags.NotEditable Or eStatusFlags.OK not via the validation
            'If a validator is used then it must be made thread safe as outputs for the MSE are set on a different thread then the core/interface thread
            'the default validator will throw a threading error
            Me.AllowValidation = False

            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            'Risk

            meta = New cVariableMetaData(0, Integer.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
            val = New cValue(New Single, eVarNameFlags.MSELowerRiskPercent, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
            m_values.Add(val.varName, val)

            meta = New cVariableMetaData(0, Integer.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
            val = New cValue(New Single, eVarNameFlags.MSEUpperRiskPercent, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
            m_values.Add(val.varName, val)


            ' meta = New cVariableMetaData(1, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThan))
            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSEBiomass, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eCoreCounterTypes.nEcosimTimeSteps, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSECatchByGroup, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eCoreCounterTypes.nEcosimTimeSteps, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            'MSEHistogram
        End Sub


        Public Sub Init()

            'the results arrays of ecosim are redim for each run
            'this means the reference to the results data is lost on each run 
            'so reset the reference
            m_coreData.Clear()

            'cEcosimDataStrucures.ResultsOverTime(var,group,time) Var and Group are fixed
            m_coreData.Add(eVarNameFlags.MSEBiomass, New c3DResultsWrapper2Fixed(Me.m_core.m_EcoSimData.ResultsOverTime, cEcosimDatastructures.eEcosimResults.Biomass, Me.Index))
            m_coreData.Add(eVarNameFlags.MSECatchByGroup, New c3DResultsWrapper2Fixed(Me.m_core.m_EcoSimData.ResultsOverTime, cEcosimDatastructures.eEcosimResults.Yield, Me.Index))


        End Sub

#End Region

#Region "Overridden base class methods"


        Public Overrides Function GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex1 As Integer = -9999, Optional ByVal iIndex2 As Integer = -9999, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object

            If Not m_coreData.ContainsKey(VarName) Then
                'NOT in list of sim vars so get the value from the base class GetVariable(...)
                Return MyBase.GetVariable(VarName, iIndex1, iIndex2)
            Else
                'Varname is access directly via the core data
                Return m_coreData.Item(VarName).Value(iIndex1, iIndex2)
            End If

        End Function

#End Region

#Region "Variable access via dot operators"

        Public Property LowerRiskPercent() As Single
            Get
                Return CInt(GetVariable(eVarNameFlags.MSELowerRiskPercent))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSELowerRiskPercent, value)
            End Set
        End Property


        Public Property UpperRiskPercent() As Single
            Get
                Return CInt(GetVariable(eVarNameFlags.MSEUpperRiskPercent))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEUpperRiskPercent, value)
            End Set
        End Property


        Public Property LowerRiskCountStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MSELowerRiskPercent)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSELowerRiskPercent, value)
            End Set
        End Property


        Public Property UpperRiskCountStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MSEUpperRiskPercent)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSEUpperRiskPercent, value)
            End Set
        End Property


        Public ReadOnly Property Biomass(ByVal iTime As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEBiomass, iTime))
            End Get
        End Property

        Public Property BiomassStatus(ByVal iTime As Integer) As eStatusFlags
            Get
                Return Me.GetStatus(eVarNameFlags.MSEBiomass, iTime)
            End Get

            Set(ByVal value As eStatusFlags)
                Me.SetStatusFlags(eVarNameFlags.MSEBiomass, value, iTime)
            End Set
        End Property

        Public ReadOnly Property GroupCatch(ByVal iTime As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSECatchByGroup, iTime))
            End Get
        End Property

#End Region

#Region "Over ridden methods"

        Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
            Dim i As Integer

            Dim statusflag As eStatusFlags = eStatusFlags.NotEditable Or eStatusFlags.OK

            Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
            Dim value As cValue
            For Each keyvalue In m_values
                Try
                    value = keyvalue.Value

                    Select Case value.varType
                        Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray
                            For i = 0 To value.Length : value.Status(i) = statusflag : Next i
                        Case Else
                            value.Status = statusflag
                    End Select
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    Return False
                End Try
            Next keyvalue
            Return True

        End Function

#End Region

    End Class

#End Region

#Region "MSE Fleet outputs"

    Public Class cMSEFleetOutput
        Inherits cCoreGroupBase

        'dictionary of vars and wrappers that directly access the core data
        Private m_coreData As New Dictionary(Of eVarNameFlags, IResultsWrapper)
        Private m_MSEData As cMSEDataStructures


#Region "Construction"

        Public Sub New(ByRef theCore As cCore, ByVal MSEData As cMSEDataStructures, ByVal GroupDBID As Integer, ByVal groupIndex As Integer)
            MyBase.New(theCore)

            Dim val As cValue = Nothing
            Dim meta As cVariableMetaData = Nothing
            m_MSEData = MSEData

            m_dataType = eDataTypes.MSEGroupOutputs
            m_coreComponent = eCoreComponentType.MSE
            Me.DBID = GroupDBID
            Me.Index = groupIndex

            'Allow validation should be false for MSE output values
            'the status flag is set in Me.ResetStatusFlags() and should always stay the same eStatusFlags.NotEditable Or eStatusFlags.OK not via the validation
            'If a validator is used then it must be made thread safe as outputs for the MSE are set on a different thread then the core/interface thread
            'the default validator will throw a threading error
            Me.AllowValidation = False

            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.NotEditable Or eStatusFlags.OK, "", eVarNameFlags.NotSet)

            'val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSECatchByFleet, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eCoreCounterTypes.nEcosimTimeSteps, AddressOf m_core.GetCoreCounter)
            'm_values.Add(val.varName, val)

            'MSEHistogram
        End Sub


        Public Sub Init()

            'the results arrays of ecosim are redim for each run
            'this means the reference to the results data is lost on each run 
            'so reset the reference
            m_coreData.Clear()

            m_coreData.Add(eVarNameFlags.MSECatchByFleet, New c2DResultsWrapper(Me.m_core.m_EcoSimData.ResultsSumCatchByGear, Me.Index))
            m_coreData.Add(eVarNameFlags.MSEValueByFleet, New c2DResultsWrapper(Me.m_core.m_EcoSimData.ResultsSumValueByGear, Me.Index))
            m_coreData.Add(eVarNameFlags.MSEEffort, New c2DResultsWrapper(Me.m_core.m_EcoSimData.ResultsEffort, Me.Index))

        End Sub

#End Region

#Region "Overridden base class methods"


        Public Overrides Function GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex1 As Integer = -9999, Optional ByVal iIndex2 As Integer = -9999, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object

            If Not m_coreData.ContainsKey(VarName) Then
                'NOT in list of sim vars so get the value from the base class GetVariable(...)
                Return MyBase.GetVariable(VarName, iIndex1, iIndex2)
            Else
                'Varname is access directly via the core data
                Return m_coreData.Item(VarName).Value(iIndex1, iIndex2)
            End If

        End Function

#End Region

#Region "Variable access via dot operators"

        Public ReadOnly Property FleetCatch(ByVal iTime As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSECatchByFleet, iTime))
            End Get
        End Property

        Public ReadOnly Property Value(ByVal iTime As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEValueByFleet, iTime))
            End Get
        End Property

        Public ReadOnly Property Effort(ByVal iTime As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEEffort, iTime))
            End Get
        End Property

#End Region

#Region "Over ridden methods"

        Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
            Dim i As Integer

            Dim statusflag As eStatusFlags = eStatusFlags.NotEditable Or eStatusFlags.OK

            Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
            Dim value As cValue
            For Each keyvalue In m_values
                Try
                    value = keyvalue.Value

                    Select Case value.varType
                        Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray
                            For i = 0 To value.Length : value.Status(i) = statusflag : Next i
                        Case Else
                            value.Status = statusflag
                    End Select
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    Return False
                End Try
            Next keyvalue
            Return True

        End Function

#End Region

    End Class

#End Region

#Region "MSE Stats Results"

#Region "Stats wrapper"

    ''' <summary>
    ''' Wrapper class for a cMSESummaryStats object used by the cMSEStats objects
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cMSEResultsStatsWrapper
        Private m_data As cMSESummaryStats
        Private m_index As Integer
        Private m_VarToStat As Dictionary(Of eVarNameFlags, MSE.eMSEStatNames)

        Friend Sub New(ByVal Index As Integer, ByVal Stats As cMSESummaryStats, ByVal CoreVarToMSEStat As Dictionary(Of eVarNameFlags, MSE.eMSEStatNames))
            m_index = Index
            m_data = Stats
            m_VarToStat = CoreVarToMSEStat
        End Sub


        Public Function Contains(ByVal VarName As EwEUtils.Core.eVarNameFlags) As Boolean
            Return Me.m_VarToStat.ContainsKey(VarName)
        End Function

        Public ReadOnly Property GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags, _
                                             Optional ByVal iIndex1 As Integer = -9999, Optional ByVal iIndex2 As Integer = -9999, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object
            Get
                Try

                    If Not Me.m_VarToStat.ContainsKey(VarName) Then
                        Debug.Assert(False, "Invalid eVarNameFlags in MSE Results")
                        Return Nothing
                    End If
                    'convert the VarName into a StatsName
                    Dim statName As MSE.eMSEStatNames = m_VarToStat(VarName)

                    'lookup the value on the StatsName
                    Select Case statName

                        Case MSE.eMSEStatNames.BinWidth
                            Return Me.BinWidths

                        Case MSE.eMSEStatNames.CV
                            Return Me.CV

                        Case MSE.eMSEStatNames.MeanRun
                            If iIndex1 = cCore.NULL_VALUE Then
                                Return Me.MeanValues
                            Else
                                Return Me.MeanValues(iIndex1)
                            End If

                        Case MSE.eMSEStatNames.Max
                            Return Me.Max

                        Case MSE.eMSEStatNames.Min
                            Return Me.Min

                        Case MSE.eMSEStatNames.nBins
                            Return Me.nBins

                        Case MSE.eMSEStatNames.PercentageHistogram
                            If iIndex1 = cCore.NULL_VALUE Then
                                Return Me.Histogram
                            Else
                                Return Me.Histogram(iIndex1)
                            End If

                        Case MSE.eMSEStatNames.Values
                            Return Me.Values(iIndex1)

                        Case MSE.eMSEStatNames.AboveLimit
                            Return Me.AboveLimit

                        Case MSE.eMSEStatNames.BelowLimit
                            Return Me.BelowLimit

                        Case MSE.eMSEStatNames.nIterations
                            Return Me.nIterations

                        Case MSE.eMSEStatNames.Std
                            Return Me.Std

                    End Select

                Catch ex As Exception
                    Debug.Assert(False, Me.ToString & ".GetVariable() Exception: " & ex.Message)
                End Try
                Return Nothing
            End Get

        End Property

        Public ReadOnly Property Histogram() As Single()
            Get
                Return m_data.Histogram(Me.m_index)
            End Get
        End Property

        Public ReadOnly Property MeanValues() As Single()
            Get
                Return m_data.MeanValues(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property Histogram(ByVal iBin As Integer) As Single
            Get
                iBin -= 1
                Return m_data.Histogram(Me.m_index)(iBin)
            End Get
        End Property

        Public ReadOnly Property MeanValues(ByVal iTime As Integer) As Single
            Get
                Return m_data.MeanValues(Me.m_index)(iTime)
            End Get
        End Property

        Public ReadOnly Property Mean() As Single
            Get
                Return m_data.Mean(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property BinWidths() As Single
            Get
                Return m_data.HistoBinWidths(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property CV() As Single
            Get
                Return m_data.CV(Me.m_index)
            End Get
        End Property

        Public ReadOnly Property Std() As Single
            Get
                Return m_data.Std(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property nBins() As Integer
            Get
                Return Me.m_data.HistoNBins(Me.m_index)
            End Get
        End Property

        Public ReadOnly Property Min() As Single
            Get
                Return Me.m_data.Min(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property Max() As Single
            Get
                Return Me.m_data.Max(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property Values(ByVal Iteration As Integer) As Single()
            Get
                Return Me.m_data.Values(Me.m_index, Iteration)
            End Get
        End Property

        ''' <summary>
        ''' Percentage of data points above the upper limit
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AboveLimit() As Single
            Get
                Return Me.m_data.AboveLimit(Me.m_index)
            End Get
        End Property

        ''' <summary>
        ''' Percentage of data points below the lower limit
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property BelowLimit() As Single
            Get
                Return Me.m_data.BelowLimit(Me.m_index)
            End Get
        End Property


        Public ReadOnly Property nIterations() As Integer
            Get
                Return Me.m_data.nIterations(Me.m_index)
            End Get
        End Property

        Public ReadOnly Property nTimeSteps() As Integer
            Get
                Return Me.m_data.nTimeSteps
            End Get
        End Property


        Public ReadOnly Property nStepsPerYear() As Integer
            Get
                Return Me.m_data.nStepsPerYear
            End Get
        End Property

    End Class

#End Region

#Region "Core input/output object mMSEStats"

    Public Class cMSEStats
        Inherits cCoreGroupBase

        Protected m_MSEStats As cMSESummaryStats
        Protected m_Stats As cMSEResultsStatsWrapper
        Protected m_varLookup As Dictionary(Of eVarNameFlags, MSE.eMSEStatNames)


#Region "Construction"

        Public Sub New(ByRef theCore As cCore, ByVal MSEData As cMSESummaryStats, ByVal DataType As eDataTypes, _
                       ByVal CoreVarToMSEStatsLookup As Dictionary(Of eVarNameFlags, MSE.eMSEStatNames), ByVal GroupDBID As Integer, ByVal groupIndex As Integer)
            MyBase.New(theCore)

            Me.m_MSEStats = MSEData

            Me.m_dataType = DataType
            Me.m_coreComponent = eCoreComponentType.MSE
            Me.DBID = GroupDBID
            Me.Index = groupIndex

            Me.m_Stats = New cMSEResultsStatsWrapper(groupIndex, MSEData, CoreVarToMSEStatsLookup)

            'Allow validation should be false for MSE output values
            'the status flag is set in Me.ResetStatusFlags() and should always stay the same eStatusFlags.NotEditable Or eStatusFlags.OK not via the validation
            'If a validator is used then it must be made thread safe as outputs for the MSE are set on a different thread then the core/interface thread
            'the default validator will throw a threading error
            Me.AllowValidation = False

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.NotEditable Or eStatusFlags.OK, "", eVarNameFlags.NotSet)

        End Sub

#End Region

#Region "Overridden base class methods"


        Public Overrides Function GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex1 As Integer = -9999, Optional ByVal iIndex2 As Integer = -9999, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object

            If Me.m_Stats.Contains(VarName) Then
                Return Me.m_Stats.GetVariable(VarName, iIndex1, iIndex2, iIndex3)
            Else
                Return MyBase.GetVariable(VarName, iIndex1, iIndex2, iIndex3)
            End If

            Return Nothing

        End Function

#End Region

#Region "Variable access via dot operators"

        Public ReadOnly Property Histogram(ByVal iBin As Integer) As Single
            Get
                Return Me.m_Stats.Histogram(iBin)
            End Get
        End Property

        Public ReadOnly Property nBins() As Integer
            Get
                Return Me.m_Stats.nBins
            End Get
        End Property

        Public ReadOnly Property BinWidths() As Single
            Get
                Return Me.m_Stats.BinWidths
            End Get
        End Property

        Public ReadOnly Property Min() As Single
            Get
                Return Me.m_Stats.Min
            End Get
        End Property

        Public ReadOnly Property Max() As Single
            Get
                Return Me.m_Stats.Max
            End Get
        End Property

        Public ReadOnly Property Mean(ByVal TimeStepIndex As Integer) As Single
            Get
                Return Me.m_Stats.MeanValues(TimeStepIndex)
            End Get
        End Property

        Public ReadOnly Property AboveLimit() As Single
            Get
                Return Me.m_Stats.AboveLimit
            End Get
        End Property

        Public ReadOnly Property BelowLimit() As Single
            Get
                Return Me.m_Stats.BelowLimit
            End Get
        End Property

        Public ReadOnly Property CV() As Single
            Get
                Return Me.m_Stats.CV
            End Get
        End Property


        Public ReadOnly Property Std() As Single
            Get
                Return Me.m_Stats.Std
            End Get
        End Property


        Public ReadOnly Property nIterations() As Integer
            Get
                Return Me.m_Stats.nIterations
            End Get
        End Property

        Public ReadOnly Property Mean() As Single
            Get
                Return Me.m_Stats.Mean
            End Get
        End Property

        Public ReadOnly Property nTimeSteps() As Integer
            Get
                Return Me.m_Stats.nTimeSteps
            End Get
        End Property

        Public ReadOnly Property nStepsPerYear() As Integer
            Get
                Return Me.m_Stats.nStepsPerYear
            End Get
        End Property

        ''' <summary>
        ''' Returns a zero based array of values for an iteration
        ''' </summary>
        ''' <param name="IterationIndex"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Values(ByVal IterationIndex As Integer) As Single()
            Get
                Dim data() As Single
                If IterationIndex <= Me.m_Stats.nIterations Then
                    data = Me.m_Stats.Values(IterationIndex)
                End If
                Debug.Assert(IterationIndex <= Me.m_Stats.nIterations, "MSE Stats Values() IterationIndex out of bounds!")
                Return data
            End Get
        End Property

#End Region

#Region "Over ridden methods"

        Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
            Dim i As Integer

            Dim statusflag As eStatusFlags = eStatusFlags.NotEditable Or eStatusFlags.OK

            Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
            Dim value As cValue
            For Each keyvalue In m_values
                Try
                    value = keyvalue.Value

                    Select Case value.varType
                        Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray
                            For i = 0 To value.Length : value.Status(i) = statusflag : Next i
                        Case Else
                            value.Status = statusflag
                    End Select
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    Return False
                End Try
            Next keyvalue
            Return True

        End Function


        Public Overrides Sub Clear()
            MyBase.Clear()
            Me.m_MSEStats = Nothing

        End Sub

#End Region

    End Class

#End Region

#End Region

#Region "MSE non dimensioned outputs. I.e. Values"

Public Class cMSEOutput
    Inherits cCoreGroupBase

#Region "Construction"

    Public Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)

        Dim val As cValue
        Dim meta As cVariableMetaData

        m_dataType = eDataTypes.MSEOutput
        m_coreComponent = eCoreComponentType.MSE

        'Allow validation should be false for MSE output values
        'the status flag is set in Me.ResetStatusFlags() and should always stay the same eStatusFlags.NotEditable Or eStatusFlags.OK not via the validation
        'If a validator is used then it must be made thread safe as outputs for the MSE are set on a different thread then the core/interface thread
        'the default validator will throw a threading error
        Me.AllowValidation = False
        Me.DBID = cCore.NULL_VALUE

            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.NotEditable Or eStatusFlags.OK, "", eVarNameFlags.NotSet)

        'Trial Number 
        meta = New cVariableMetaData(0, Integer.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Integer, eVarNameFlags.MSETrialNumber, eStatusFlags.Null, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        'Total values
        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEEconomicValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEEcologicalValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEEmployValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEMandatedValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        'Mean values
        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEWeightedTotalValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEMeanEconomicValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEMeanEcologicalValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEMeanEmployValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEMeanMandatedValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

        meta = New cVariableMetaData(0, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        val = New cValue(New Single, eVarNameFlags.MSEBestTotalValue, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng, meta)
        m_values.Add(val.varName, val)

    End Sub

#End Region

#Region "Variable access via dot operators"

    Public Property EmployValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEEmployValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEEmployValue, value)
        End Set
    End Property

    Public Property MandatedValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEMandatedValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEMandatedValue, value)
        End Set
    End Property

    Public Property EcologicalValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEEcologicalValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEEcologicalValue, value)
        End Set
    End Property

    Public Property EconomicValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEEconomicValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEEconomicValue, value)
        End Set
    End Property


    'mean
    Public Property MeanEconomicValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEMeanEconomicValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEMeanEconomicValue, value)
        End Set
    End Property

    Public Property MeanEmployValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEMeanEmployValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEMeanEmployValue, value)
        End Set
    End Property

    Public Property MeanMandatedValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEMeanMandatedValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEMeanMandatedValue, value)
        End Set
    End Property

    Public Property MeanEcologicalValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEMeanEcologicalValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEMeanEcologicalValue, value)
        End Set
    End Property

    Public Property WeightedMeanTotalValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MSEWeightedTotalValue))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEWeightedTotalValue, value)
        End Set
    End Property

    Public Property BestTotalValue() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.MSEBestTotalValue), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MSEBestTotalValue, value)
        End Set
    End Property


    Public Property TrialNumber() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MSETrialNumber))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.MSETrialNumber, value)
        End Set
    End Property

#End Region

End Class

#End Region

End Namespace
