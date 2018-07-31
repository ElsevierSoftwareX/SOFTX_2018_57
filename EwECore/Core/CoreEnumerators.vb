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
Imports EwEUtils.Core

#Region "cCoreEnumNamesIndex"

' Singleton classes should not implement IDisposable; on core destruct 
' these classes will not re-initate and will thus be invalid when a new
' core attempts to use them.

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class; creates and maintains quick lookup tables of string 
''' representations of enumerated types defined in the Core.
''' </summary>
''' <remarks>
''' The dotNET mechanism for converting enum values to a string representation is
''' dreadfully slow. This class provides a redundant but bloody fast way to
''' find this string representation by indexing all string representations once.
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cCoreEnumNamesIndex
    'Implements IDisposable

    ''' <summary>Singleton instance</summary>
    Private Shared __inst__ As cCoreEnumNamesIndex = New cCoreEnumNamesIndex()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the one and only instance of this class.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Shared Function GetInstance() As cCoreEnumNamesIndex
        Return cCoreEnumNamesIndex.__inst__
    End Function

    'Public Sub Dispose() Implements IDisposable.Dispose
    '    If (Me.m_dictDataTypeEnumToName IsNot Nothing) Then
    '        Me.m_dictDataTypeEnumToName.Clear()
    '        Me.m_dictDataTypeEnumToName = Nothing
    '        Me.m_dictDataTypeNameToEnum.Clear()
    '        Me.m_dictDataTypeNameToEnum = Nothing
    '        Me.m_dictVarEnumToName.Clear()
    '        Me.m_dictVarEnumToName = Nothing
    '        Me.m_dictVarNameToEnum.Clear()
    '        Me.m_dictVarNameToEnum = Nothing
    '    End If
    '    GC.SuppressFinalize(Me)
    'End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Private constructor to enforce singleton.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub New()
        ' Make indexes
        Me.IndexEnum(GetType(eVarNameFlags), Me.m_dictVarEnumToName, Me.m_dictVarNameToEnum)
        Me.IndexEnum(GetType(eDataTypes), Me.m_dictDataTypeEnumToName, Me.m_dictDataTypeNameToEnum)
    End Sub

    ''' <summary>Index of eVarNameFlags enum names, by enum value.</summary>
    Private m_dictVarEnumToName As New Dictionary(Of Integer, String)
    ''' <summary>Index of eVarNameFlags enum values, by name.</summary>
    Private m_dictVarNameToEnum As New Dictionary(Of String, Integer)
    ''' <summary>Index of eDataType enum names, by enum value.</summary>
    Private m_dictDataTypeEnumToName As New Dictionary(Of Integer, String)
    ''' <summary>Index of eDataType enum values, by name.</summary>
    Private m_dictDataTypeNameToEnum As New Dictionary(Of String, Integer)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Make a name index of a given enumerated type.
    ''' </summary>
    ''' <param name="t">The enumerated type to generate the enum name index for.</param>
    ''' <param name="dict1">A dictionary to store the value/name pairs in.</param>
    ''' <param name="dict2">A dictionary to store the name/value pairs in.</param>
    ''' -----------------------------------------------------------------------
    Private Sub IndexEnum(ByVal t As Type, ByRef dict1 As Dictionary(Of Integer, String), ByRef dict2 As Dictionary(Of String, Integer))

        Dim aEnum As Array = System.Enum.GetValues(t)
        Dim strName As String = ""
        Dim iValue As Integer = 0
        ' Iterate through enum
        For i As Integer = aEnum.GetLowerBound(0) To aEnum.GetUpperBound(0)
            ' Acquire and store name for quick lookup
            iValue = CInt(aEnum.GetValue(i))
            strName = CStr(System.Enum.GetName(t, iValue))
            dict1(iValue) = strName
            dict2(strName) = iValue
        Next i

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a eVarNameFlags enum name.
    ''' </summary>
    ''' <param name="e">The <see cref="eVarNameFlags">eVarNameFlags</see> 
    ''' enumerated value to retrieve the name for.</param>
    ''' -----------------------------------------------------------------------
    Public Function GetVarName(ByVal e As eVarNameFlags) As String
        Return Me.m_dictVarEnumToName(e)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a eVarNameFlags enum value.
    ''' </summary>
    ''' <param name="strVarName">The string representation for a variable name.</param>
    ''' -----------------------------------------------------------------------
    Public Function GetVarName(ByVal strVarName As String) As eVarNameFlags
        If Me.m_dictVarNameToEnum.ContainsKey(strVarName) Then
            Return DirectCast(Me.m_dictVarNameToEnum(strVarName), eVarNameFlags)
        End If
        Return eVarNameFlags.NotSet
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a eDataTypes enum name.
    ''' </summary>
    ''' <param name="e">The <see cref="eDataTypes">eDataTypes</see> 
    ''' enumerated value to retrieve the name for.</param>
    ''' -----------------------------------------------------------------------
    Public Function GetDataTypeName(ByVal e As eDataTypes) As String
        Return Me.m_dictDataTypeEnumToName(e)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a eDataTypes enum value.
    ''' </summary>
    ''' <param name="strDataType">The string representation for a data type.</param>
    ''' -----------------------------------------------------------------------
    Public Function GetDataType(ByVal strDataType As String) As eDataTypes
        If Me.m_dictDataTypeNameToEnum.ContainsKey(strDataType) Then
            Return DirectCast(Me.m_dictDataTypeNameToEnum(strDataType), eDataTypes)
        Else
            Return eDataTypes.NotSet
        End If
    End Function

End Class

#End Region

#Region "Progress State"

Public Enum eProgressState
    ''' <summary>Process has just started this is the first call</summary>
    Start
    ''' <summary>Process is running </summary>
    Running
    ''' <summary>Process has finished </summary>
    Finished
End Enum

#End Region

#Region "Ecopath Parameter Estimation type"

''' ---------------------------------------------------------------------------
''' <summary>
''' Enumerated type that indicates for which purpose Ecopath parameters are being estimated.
''' </summary>
''' ---------------------------------------------------------------------------
Public Enum eEstimateParameterFor
    ''' <summary>
    ''' Indicates that parameters are being estimated for the 
    ''' main parameter estimation routine.
    ''' </summary>
    ParameterEstimation

    ''' <summary>
    ''' Indicates that parameters are being estimated for the 
    ''' sensitivity loop.
    ''' </summary>
    Sensitivity
End Enum

#End Region

#Region "Operators for cOperatorBase"

''' ---------------------------------------------------------------------------
''' <summary>
''' Enumerated type indicating logical operators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Enum eOperators
    ''' <summary>
    ''' Logical 'less than' operator.
    ''' </summary>
    LessThan

    ''' <summary>
    ''' Logical 'less than or equal to' operator.
    ''' </summary>
    LessThanOrEqualTo

    ''' <summary>
    ''' Logical 'greater than' operator.
    ''' </summary>
    GreaterThan

    ''' <summary>
    ''' Logical 'greater than or equal to' operator.
    ''' </summary>
    GreaterThanOrEqualTo

    ''' <summary>
    ''' Logical 'equal to' operator.
    ''' </summary>
    EqualTo
End Enum

#End Region

#Region "Primary Production Types"

''' ---------------------------------------------------------------------------
''' <summary>
''' Enumerated type specifying Group Primary Production types
''' </summary>
''' ---------------------------------------------------------------------------
Public Enum ePrimaryProductionTypes
    Consumer = 0
    Producer = 1
    Detritus = 2
End Enum

#End Region

#Region "Cost Index"

''' ---------------------------------------------------------------------------
''' <summary>
''' Enumerator for CostPct(nFleets, 3) array, 
''' i.e. fleet.FixedCost = CostPct(1, eCostIndex.Fixed) is the fixed cost for 
''' variable 'fleet' at index 1.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Enum eCostIndex
    Profit = 0
    Fixed = 1
    CUPE = 2
    Sail = 3
End Enum

#End Region

#Region "Ecospace results index"

''' ---------------------------------------------------------------------------
''' <summary>
''' Index of results from Ecospace saved over time by group
''' </summary>
''' <remarks>Index to data stored in the <see cref="cEcospaceDataStructures.ResultsByGroup">cEcospaceDataStructures.ResultsByGroup(eSpaceResultsGroups,group,time) array</see></remarks>
''' ---------------------------------------------------------------------------
Public Enum eSpaceResultsGroups
    Biomass
    RelativeBiomass
    CatchBio
    Value
    FishingMort
    PredMortRate
    ConsumpRate
    ''' <summary>Total Loss in tons summed across all cells. Not KM2</summary>
    TotalLoss
End Enum

Public Enum eSpaceResultsFleets
    SailingEffort
    FishingEffort
    CatchBio
    Value
End Enum


Public Enum eSpaceResultsFleetsGroups
    CatchBio
    Value
End Enum

#End Region

#Region " Time series types "

''' ---------------------------------------------------------------------------
''' <summary>
''' Types of time series
''' </summary>
''' <remarks>The enumerated values follow the original EwE5 scheme.</remarks>
''' ---------------------------------------------------------------------------
Public Enum eTimeSeriesType As Integer
    BiomassRel = 0
    BiomassAbs = 1
    BiomassForcing = -1
    TimeForcing = 2
    FishingEffort = 3
    FishingMortality = 4
    TotalMortality = 5
    ConstantTotalMortality = -5
    Catches = 6
    CatchesForcing = -6
    CatchesRel = 61
    AverageWeight = 7
    ' EcotracerConcRel = 8
    ' EcotracerConcAbs = 9
    ''' <summary>Absolute discard proportion (fleet x group, driver)</summary>
    DiscardProportion = 10
    ''' <summary>Absolute discard mortality rate (fleet x group, driver)</summary>
    DiscardMortality = 11
    ''' <summary>Absolute landings (fleet x group, reference)</summary>
    Landings = 12
    ''' <summary>Absolute discards (fleet x group, reference)</summary>
    Discards = 13
    FishingMortalityRef = 104
    NotSet = cCore.NULL_VALUE
End Enum

''' -----------------------------------------------------------------------
''' <summary>
''' Enumerated type stating whether a time series is 
''' <see cref="cGroupTimeSeries">group-related</see>,  
''' <see cref="cFleetTimeSeries">fleet-related</see> or is a
''' <see cref="cForcingFunction">forcing function</see>.
''' </summary>
''' -----------------------------------------------------------------------
Public Enum eTimeSeriesCategoryType
    ''' <summary>Unknown time series category.</summary>
    NotSet = 0
    ''' <summary>Group-related time series category.</summary>
    Group
    ''' <summary>Fleet-related time series category.</summary>
    Fleet
    ''' <summary>Forcing function time series category.</summary>
    Forcing
    ''' <summary>Fleet-related time series category, indexed by group.</summary>
    FleetGroup
End Enum

''' -----------------------------------------------------------------------
''' <summary>
''' Enumerated type, defining aliases for <see cref="eTimeSeriesType">time series types</see>.
''' </summary>
''' -----------------------------------------------------------------------
Public Enum eTimeSeriesAliases As Integer
    BRel = 0
    BiomassRel = BRel
    BAbs = 1
    BiomassAbs = BAbs
    BForced = -1
    BiomassForced = BForced
    Forcing = 2
    Effort = 3
    Z = 4
    Mort = Z
    F = 5
    FishingMort = F
    FConst = -5
    FishingMortConst = FConst
    C = 6
    [Catch] = 6
    CForced = -6
    CatchForced
    WAvg = 7
    WeightAvg = WAvg
    ConcRel = 8
    ConcAbs = 9
End Enum

''' -----------------------------------------------------------------------
''' <summary>
''' Enumerated type, defining time series data point spacing types.
''' </summary>
''' <note>
''' Enum are assigned fixed values because this enum is persistent in the
''' EwE database.
''' </note>
''' -----------------------------------------------------------------------
Public Enum eTSDataSetInterval
    Annual = 0
    TimeStep = 1
End Enum

#End Region ' Time series types

#Region " PSD mortality types "

''' -----------------------------------------------------------------------
''' <summary>
''' Mortality types for PSD analysis
''' </summary>
''' -----------------------------------------------------------------------
Public Enum ePSDMortalityTypes As Integer
    ''' <summary>Group P/B</summary>
    GroupZ = 0
    ''' <summary>Lorenzen-variable</summary>
    Lorenzen = 1
End Enum

#End Region ' PSD mortality types

#Region " PSD climate types "

''' -----------------------------------------------------------------------
''' <summary>
''' The three climate zones for PSD analysis.
''' </summary>
''' -----------------------------------------------------------------------
Public Enum eClimateTypes As Integer
    ''' <summary>Tropical climate</summary>
    Tropical = 0
    ''' <summary>Temperate climate</summary>
    Temperate = 1
    ''' <summary>Polar climate</summary>
    Polar = 2
End Enum

#End Region ' PSD climate types

#Region " Diversity index "

''' <summary>
''' Selected biodiversity index to display in the UI
''' </summary>
Public Enum eDiversityIndexType As Integer
    Shannon = 0
    KemptonsQ = 1
End Enum

#End Region ' Diversity index