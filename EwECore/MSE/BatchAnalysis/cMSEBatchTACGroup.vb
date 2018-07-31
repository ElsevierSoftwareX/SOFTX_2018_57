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

#End Region

Namespace MSE

    Public Class cMSEBatchTACGroup
        Inherits cCoreGroupBase

        'Private m_BLimValues() As Single
        'Private m_BBaseValues() As Single
        'Private m_FMaxValues() As Single
        Private m_BatchData As MSEBatchManager.cMSEBatchDataStructures

        Public Sub New(ByRef theCore As cCore, ByRef MSEBatchData As MSEBatchManager.cMSEBatchDataStructures, ByVal theGroupDBID As Integer)
            MyBase.New(theCore)

            Dim val As cValue

            Me.m_dataType = eDataTypes.MSEBatchFixedFInput
            Me.m_coreComponent = eCoreComponentType.MSE
            Me.AllowValidation = False
            Me.DBID = theGroupDBID

            Me.m_BatchData = MSEBatchData

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            val = New cValue(New Single, eVarNameFlags.MSEBatchTACLower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSEBatchTACUpper, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSETAC, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'Used
            val = New cValue(New Boolean, eVarNameFlags.MSEBatchTACManaged, eStatusFlags.Null, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            Me.AllowValidation = True

        End Sub


        Public Property TAC As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETAC))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETAC, value)
            End Set
        End Property

        Public Property TACLower As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEBatchTACLower))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEBatchTACLower, value)
            End Set
        End Property

        Public Property TACUpper As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEBatchTACUpper))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEBatchTACUpper, value)
            End Set
        End Property



        Public Property isManaged As Boolean
            Get
                Return CBool(GetVariable(eVarNameFlags.MSEBatchTACManaged))
            End Get

            Set(ByVal value As Boolean)
                SetVariable(eVarNameFlags.MSEBatchTACManaged, value)
            End Set
        End Property


        Public Property TACValue(IterationIndex As Integer) As Single
            Get
                ' Debug.Assert(IterationIndex <= Me.m_BatchData.nTFM, Me.ToString & ".BLimValue() Index out of range!")
                If IterationIndex <= Me.m_BatchData.nTAC Then
                    Return Me.m_BatchData.TAC(IterationIndex, Me.Index)
                End If
                'OH My.....
                Return cCore.NULL_VALUE
            End Get

            Set(ByVal value As Single)
                ' Debug.Assert(IterationIndex <= Me.m_BatchData.nTFM, Me.ToString & ".BLimValue() Index out of range!")
                If IterationIndex <= Me.m_BatchData.nTAC Then
                    Me.m_BatchData.TAC(IterationIndex, Me.Index) = value
                End If
            End Set
        End Property



        Public Overrides Function GetVariable(VarName As EwEUtils.Core.eVarNameFlags, Optional iIndex As Integer = -9999, Optional iIndex2 As Integer = -9999, Optional iIndex3 As Integer = -9999) As Object

            Select Case VarName
                Case eVarNameFlags.MSEBatchTACValues
                    Return Me.m_BatchData.TAC(iIndex, Index)

            End Select

            Return MyBase.GetVariable(VarName, iIndex, iIndex2, iIndex3)

        End Function


        Public Overrides Function SetVariable(VarName As EwEUtils.Core.eVarNameFlags, newValue As Object, Optional iSecondaryIndex As Integer = -9999, Optional ByVal iThirdIndex As Integer = -9999) As Boolean

            Select Case VarName

                Case eVarNameFlags.MSEBatchTACValues

                    Me.m_BatchData.TAC(iSecondaryIndex, Index) = CSng(newValue)
                    Return True
            End Select

            Return MyBase.SetVariable(VarName, newValue, iSecondaryIndex)

        End Function


        Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
            MyBase.ResetStatusFlags(bForceReset)

            Me.AllowValidation = False
            Dim tcatch As Single

            For iflt As Integer = 1 To Me.m_core.nFleets
                Dim fleet As cEcopathFleetInput = Me.m_core.EcopathFleetInputs(iflt)
                tcatch += fleet.Landings(Me.Index) + fleet.Discards(Me.Index)
            Next

            If tcatch = 0.0! Then
                For Each var As cValue In Me.m_values.Values
                    If var.varName <> eVarNameFlags.Name And var.varName <> eVarNameFlags.Index And var.varName <> eVarNameFlags.DBID Then
                        Me.SetStatusFlags(var.varName, eStatusFlags.Null Or eStatusFlags.NotEditable)
                    End If
                Next
            End If

            Return True

        End Function


    End Class

End Namespace
