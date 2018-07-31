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

''' <summary>
''' Inputs for EcoSim for a single group.
''' </summary>
''' <remarks>
''' This class wraps the inputs to EcoSim for one group into a single object.
''' </remarks>
Public Class cEcoSimGroupInput
    Inherits cCoreGroupBase

    Private m_nGroups As Integer

    ''' <summary>
    ''' Public access to set the status flags by calling each validator.
    ''' </summary>
    ''' <returns>True is successful. False otherwise</returns>
    ''' <remarks>This is the default behaviour for Input objects. An output will need to override this to provide its own implementation.</remarks>
    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        Dim i As Integer

        Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        Dim value As cValue
        For Each keyvalue In m_values
            Try
                value = keyvalue.Value
                'Status flag for VulMult and VulRate are set in cCore.LoadEcosimGroups
                'If value.varName <> eVarNameFlags.VulMult And value.varName <> eVarNameFlags.VulRate Then
                If value.varName <> eVarNameFlags.VulMult Then
                    Select Case value.varType
                        Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray
                            For i = 0 To value.Length
                                If bForceReset Then
                                    value.Status(i) = 0
                                Else
                                    value.setStatusFlag(i)
                                End If
                            Next i
                        Case Else
                            If bForceReset Then
                                value.Status = 0
                            Else
                                value.setStatusFlag()
                            End If
                    End Select
                End If

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return False
            End Try
        Next keyvalue

        Me.m_core.Set_PP_Flags(Me, False)
        Return True

    End Function

#Region "Mapping of variable names"
    'mapping to underlying data structure names
    ' MaxRelPB  =  'PBmaxs max rel P/B
    ' MaxRelFeedingTime  =  ' FtimeMax
    ' FeedingTimeAdjustRate  =  'FtimeAdjust
    ' OtherMortFeedingTime  =  'MoPred
    ' PerdEffectFeedingTime  =  'RiskTime
    ' DenDepCatchability  =  'QmQo
    ' QBMaxQBio  =  'CmCo
    ' SwitchingPower  =  'SwitchPower
    ' VBGF  =  'vbK
    ' VulRate()  =  'vulnerability rates of predation for this group (prey)
    ' VulMult()  =  'vulnerability multiplier
#End Region

#Region "Constructor"


    Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)

        Try

            m_nGroups = theCore.nGroups

            m_dataType = eDataTypes.EcoSimGroupInput
            m_coreComponent = eCoreComponentType.EcoSim
            Me.AllowValidation = False
            Me.DBID = DBID

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            Dim val As cValue

            'MaxRelPB
            val = New cValue(New Single, eVarNameFlags.MaxRelPB, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'MaxRelFeedingTime
            val = New cValue(New Single, eVarNameFlags.MaxRelFeedingTime, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'FeedingTimeAdjRate
            val = New cValue(New Single, eVarNameFlags.FeedingTimeAdjRate, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'OtherMortFeedingTime
            val = New cValue(New Single, eVarNameFlags.OtherMortFeedingTime, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'PredEffectFeedingTime
            val = New cValue(New Single, eVarNameFlags.PredEffectFeedingTime, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'DenDepCatchability
            val = New cValue(New Single, eVarNameFlags.DenDepCatchability, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'QBMaxQBio
            val = New cValue(New Single, eVarNameFlags.QBMaxQBio, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'Switching Power
            val = New cValue(New Single, eVarNameFlags.SwitchingPower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'Srrayed values
            ''VulRate
            'meta = New cVariableMetaData(1, Single.MaxValue, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThan))
            'val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.VulRate, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter, meta, m_core.m_validators.getValidator(eVarNameFlags.VulRate))
            'm_values.Add(val.varName, val)

            'VulMult
            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.VulMult, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcoSimGroupInfo.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEcoSimGroupInfo. Error: " & ex.Message)
        End Try

    End Sub

#End Region

#Region "Variable via dot(.) operator"

    Public Property DenDepCatchability() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.DenDepCatchability))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.DenDepCatchability, value)
        End Set
    End Property

    Public Property FeedingTimeAdjustRate() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.FeedingTimeAdjRate))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.FeedingTimeAdjRate, value)
        End Set
    End Property

    Public Property MaxRelFeedingTime() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MaxRelFeedingTime))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MaxRelFeedingTime, value)
        End Set
    End Property

    Public Property MaxRelPB() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MaxRelPB))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.MaxRelPB, value)
        End Set
    End Property

    Public Property OtherMortFeedingTime() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.OtherMortFeedingTime))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.OtherMortFeedingTime, value)
        End Set
    End Property

    Public Property PredEffectFeedingTime() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.PredEffectFeedingTime))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.PredEffectFeedingTime, value)
        End Set
    End Property

    Public Property QBMaxQBio() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.QBMaxQBio))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.QBMaxQBio, value)
        End Set
    End Property

    Public Property SwitchingPower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.SwitchingPower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.SwitchingPower, value)
        End Set
    End Property

#Region "Indexed variables"

    ''' <summary>
    ''' Vulnerability multiplier vulnerability of this group to predation
    ''' </summary>
    ''' <param name="iPredGroup">Group index of the predator group</param>
    ''' <value></value>
    Public Property VulMult(ByVal iPredGroup As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.VulMult, iPredGroup))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.VulMult, value, iPredGroup)
        End Set

    End Property

#End Region

#End Region

#Region "Status Flags via dot(.) operator"

    Public Property DenDepCatchabilityStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.DenDepCatchability)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.DenDepCatchability, value)
        End Set
    End Property

    Public Property FeedingTimeAdjustRateStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.FeedingTimeAdjRate)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.FeedingTimeAdjRate, value)
        End Set
    End Property

    Public Property MaxRelFeedingTimeStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.MaxRelFeedingTime)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MaxRelFeedingTime, value)
        End Set
    End Property

    Public Property MaxRelPBStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.MaxRelPB)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MaxRelPB, value)
        End Set
    End Property

    Public Property OtherMortFeedingTimeStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.OtherMortFeedingTime)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.OtherMortFeedingTime, value)
        End Set
    End Property

    Public Property PredEffectFeedingTimeStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.PredEffectFeedingTime)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.PredEffectFeedingTime, value)
        End Set
    End Property

    Public Property QBMaxBioStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.QBMaxQBio)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.QBMaxQBio, value)
        End Set
    End Property

    Public Property SwitchingPowerStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.SwitchingPower)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.SwitchingPower, value)
        End Set
    End Property

    Public Property VulMultiStatus(ByVal iGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.VulMult, iGroup)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.VulMult, value, iGroup)
        End Set
    End Property

    'Public Property VulRateStatus(ByVal iGroup As Integer) As eStatusFlags
    '    Get
    '        Return GetStatus(eVarNameFlags.VulRate, iGroup)
    '    End Get

    '    Set(ByVal value As eStatusFlags)
    '        SetStatus(eVarNameFlags.VulRate, value, iGroup)
    '    End Set
    'End Property

#End Region

End Class
