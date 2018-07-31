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

#Region "Imports"
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
#End Region

Namespace MSE

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cMSEFleetInput
        Inherits cCoreGroupBase

        Public Sub New(ByRef theCore As cCore, ByVal theFleetDBID As Integer)
            MyBase.New(theCore)

            Dim val As cValue

            m_dataType = eDataTypes.MSEFleetInput
            m_coreComponent = eCoreComponentType.MSE
            Me.AllowValidation = False
            Me.DBID = theFleetDBID

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            val = New cValue(New Single, eVarNameFlags.MSEQIncrease, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'Bounds
            val = New cValue(New Single, eVarNameFlags.MSERefFleetCatchLower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSERefFleetCatchUpper, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSERefFleetEffortLower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSERefFleetEffortUpper, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Boolean, eVarNameFlags.MSYEvaluateFleet, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            'MaxEffort
            val = New cValue(New Single, eVarNameFlags.MaxEffort, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'LP Effort Lower bound
            val = New cValue(New Single, eVarNameFlags.MSELowerLPEffort, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'LP Effort Upper bound
            val = New cValue(New Single, eVarNameFlags.MSEUpperLPEffort, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'QuotaType
            val = New cValue(New Integer, eVarNameFlags.QuotaType, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' === arrays ===
            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSEFleetCV, eStatusFlags.Null, eCoreCounterTypes.nEcosimYears, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSEFleetWeight, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.QuotaShare, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            Me.AllowValidation = True

        End Sub

        ''' <summary>
        ''' Edit the CVs in batch mode no messages are sent out when BatchEdit = True when BatchEdit is toggled to False then the core is notified.
        ''' </summary>
        ''' <remarks>This turns off the AllowValidation flag which stops the object from calling core.OnValidate() vastly speeding up the editing</remarks>
        Public Property BatchEdit() As Boolean
            Get
                Return Not Me.AllowValidation
            End Get

            Set(ByVal value As Boolean)

                'if turning the BatchEdit On after it has been OFF tell the core that the values has been edited
                'this will allow the core to update the underlying data and send out a datamodified message
                If Me.BatchEdit = True And value = False Then
                    Me.m_core.OnValidated(m_values.Item(eVarNameFlags.MSEFleetCV), Me)
                End If
                Me.AllowValidation = Not value

            End Set

        End Property

        Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean

            For Each value As cValue In Me.m_values.Values

                Try
                    Select Case value.varName

                        Case eVarNameFlags.MSEFleetWeight, _
                             eVarNameFlags.QuotaShare

                            For igrp As Integer = 1 To m_core.nLivingGroups
                                If Me.m_core.m_EcoSimData.relQ(value.Index, igrp) > 0 Then
                                    value.Status(igrp) = eStatusFlags.OK
                                Else
                                    value.Status(igrp) = eStatusFlags.NotEditable Or eStatusFlags.Null
                                End If
                            Next

                        Case eVarNameFlags.MSEFleetCV

                            For i As Integer = 1 To Me.m_core.nEcosimYears
                                value.setStatusFlag(i)
                            Next

                        Case Else

                            value.setStatusFlag()

                    End Select

                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    System.Console.WriteLine(Me.ToString & ".ResetStatusFlags() Exception " & value.varName.ToString)
                End Try

            Next value

            Return Me.m_core.Set_Quota_Flags(Me, False)

        End Function

#Region "Variable via dot(.) operator"

        ''' <summary>
        ''' MSE increase in catchability by group per year (multiplier)
        ''' </summary>
        Public Property QIncrease() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEQIncrease))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEQIncrease, value)
            End Set
        End Property


        Public Property FleetCV(ByVal iTime As Integer) As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSEFleetCV, iTime))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEFleetCV, value, iTime)
            End Set

        End Property


        Public Property CatchRefLower() As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSERefFleetCatchLower))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSERefFleetCatchLower, value)
            End Set

        End Property

        Public Property CatchRefUpper() As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSERefFleetCatchUpper))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSERefFleetCatchUpper, value)
            End Set

        End Property


        Public Property EffortRefLower() As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSERefFleetEffortLower))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSERefFleetEffortLower, value)
            End Set

        End Property

        Public Property EffortRefUpper() As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSERefFleetEffortUpper))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSERefFleetEffortUpper, value)
            End Set

        End Property

        ''' <summary>
        ''' Importance weight of fleet on a group
        ''' </summary>
        ''' <param name="iGroup">impacted group</param>
        Public Property FleetWeight(ByVal iGroup As Integer) As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSEFleetWeight, iGroup))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEFleetWeight, value, iGroup)
            End Set

        End Property

        Public Property MSYEvaluateFleet() As Boolean

            Get
                Return CBool(GetVariable(eVarNameFlags.MSYEvaluateFleet))
            End Get

            Set(ByVal value As Boolean)
                SetVariable(eVarNameFlags.MSYEvaluateFleet, value)
            End Set

        End Property

        Public Property MaxEffort() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MaxEffort))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MaxEffort, value)
            End Set
        End Property

        Public Property QuotaType() As eQuotaTypes
            Get
                Return DirectCast(GetVariable(eVarNameFlags.QuotaType), eQuotaTypes)
            End Get

            Set(ByVal value As eQuotaTypes)
                SetVariable(eVarNameFlags.QuotaType, value)
            End Set
        End Property

        Public Property QuotaShare(ByVal iGroup As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.QuotaShare, iGroup))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.QuotaShare, value, iGroup)
            End Set
        End Property


        Public Property LowerLPEffortBound() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSELowerLPEffort))
            End Get
            Set(value As Single)
                SetVariable(eVarNameFlags.MSELowerLPEffort, value)
            End Set
        End Property


        Public Property UpperLPEffortBound() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEUpperLPEffort))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEUpperLPEffort, value)
            End Set
        End Property


#End Region

#Region "Status Flags via dot(.) operator"

        Public Property QIncreaseStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MSEQIncrease)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSEQIncrease, value)
            End Set
        End Property


        Public Property CatchRefUpperStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MSERefFleetCatchUpper)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSERefFleetCatchUpper, value)
            End Set
        End Property

        Public Property CatchRefLowerStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MSERefGroupCatchLower)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSERefGroupCatchLower, value)
            End Set
        End Property

        Public Property FleetCVStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MSEFleetCV)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSEFleetCV, value)
            End Set
        End Property

        Public Property FleetWeightStatus(ByVal iGroup As Integer) As eStatusFlags

            Get
                Return GetStatus(eVarNameFlags.MSEFleetWeight, iGroup)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MSEFleetWeight, value, iGroup)
            End Set

        End Property

        Public Property MaxEffortStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.MaxEffort)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.MaxEffort, value)
            End Set
        End Property

        Public Property QuotaTypeStatus() As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.QuotaType)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.QuotaType, value)
            End Set
        End Property

        Public Property QuotaStatus(ByVal iGroup As Integer) As eStatusFlags
            Get
                Return GetStatus(eVarNameFlags.QuotaShare, iGroup)
            End Get

            Set(ByVal value As eStatusFlags)
                SetStatus(eVarNameFlags.QuotaShare, value, iGroup)
            End Set
        End Property

#End Region

    End Class

End Namespace
