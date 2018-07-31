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

    Public Class cMSEBatchTFMGroup
        Inherits cCoreGroupBase

        'Private m_BLimValues() As Single
        'Private m_BBaseValues() As Single
        'Private m_FMaxValues() As Single
        Private m_BatchData As MSEBatchManager.cMSEBatchDataStructures

        Public Sub New(ByRef theCore As cCore, ByRef MSEBatchData As MSEBatchManager.cMSEBatchDataStructures, ByVal theGroupDBID As Integer)
            MyBase.New(theCore)

            Dim val As cValue

            Me.m_dataType = eDataTypes.MSEBatchTFMInput
            Me.m_coreComponent = eCoreComponentType.MSE
            Me.AllowValidation = False
            Me.DBID = theGroupDBID

            Me.m_BatchData = MSEBatchData

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            val = New cValue(New Single, eVarNameFlags.MSETFMBLimLower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSETFMBLimUpper, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSETFMBBaseLower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSETFMBBaseUpper, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSETFMFOptLower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            val = New cValue(New Single, eVarNameFlags.MSETFMFOptUpper, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)


            'bBase
            val = New cValue(New Single, eVarNameFlags.MSEBBase, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)
            'bLim
            val = New cValue(New Single, eVarNameFlags.MSEBLim, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)
            'FOpt
            val = New cValue(New Single, eVarNameFlags.MSEFmax, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)
            'Fmin
            val = New cValue(New Single, eVarNameFlags.MSEFmin, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'Used
            val = New cValue(New Boolean, eVarNameFlags.MSEBatchTFMManaged, eStatusFlags.Null, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            'Iteration values 
            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSETFMFOptValues, eStatusFlags.Null, eCoreCounterTypes.nMSEBatchTFM, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSETFMBBaseValues, eStatusFlags.Null, eCoreCounterTypes.nMSEBatchTFM, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.MSETFMBLimValues, eStatusFlags.Null, eCoreCounterTypes.nMSEBatchTFM, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            Me.AllowValidation = True

        End Sub


        Public Property BLim As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEBLim))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEBLim, value)
            End Set
        End Property

        Public Property BLimLower As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMBLimLower))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMBLimLower, value)
            End Set
        End Property

        Public Property BLimUpper As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMBLimUpper))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMBLimUpper, value)
            End Set
        End Property



        Public Property BBase As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEBBase))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEBBase, value)
            End Set
        End Property

        Public Property BBaseLower As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMBBaseLower))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMBBaseLower, value)
            End Set
        End Property

        Public Property BBaseUpper As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMBBaseUpper))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMBBaseUpper, value)
            End Set
        End Property

        Public Property FMax As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSEFmax))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSEFmax, value)
            End Set
        End Property

        Public Property FMaxLower As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMFOptLower))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMFOptLower, value)
            End Set
        End Property

        Public Property isManaged As Boolean
            Get
                Return CBool(GetVariable(eVarNameFlags.MSEBatchTFMManaged))
            End Get

            Set(ByVal value As Boolean)
                SetVariable(eVarNameFlags.MSEBatchTFMManaged, value)
            End Set
        End Property

        Public Property FMaxUpper As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMFOptUpper))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMFOptUpper, value)
            End Set
        End Property

        Public Property FMaxValue(IterationIndex As Integer) As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMFOptValues, IterationIndex))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMFOptValues, value, IterationIndex)
            End Set

        End Property

        Public Property BLimValue(IterationIndex As Integer) As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMBLimValues, IterationIndex))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMBLimValues, value, IterationIndex)
            End Set
            'Get
            '    ' Debug.Assert(IterationIndex <= Me.m_BatchData.nTFM, Me.ToString & ".BLimValue() Index out of range!")
            '    If IterationIndex <= Me.m_BatchData.nTFM Then
            '        Return Me.m_BatchData.tfmBlim(IterationIndex, Me.Index)
            '    End If
            '    'OH My.....
            '    Return cCore.NULL_VALUE
            'End Get

            'Set(ByVal value As Single)
            '    'Debug.Assert(IterationIndex <= Me.m_BatchData.nTFM, Me.ToString & ".BLimValue() Index out of range!")
            '    If IterationIndex <= Me.m_BatchData.nTFM Then
            '        Me.m_BatchData.tfmBlim(IterationIndex, Me.Index) = value
            '    End If
            'End Set
        End Property


        Public Property BBaseValue(IterationIndex As Integer) As Single

            Get
                Return CSng(GetVariable(eVarNameFlags.MSETFMBBaseValues, IterationIndex))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.MSETFMBBaseValues, value, IterationIndex)
            End Set

            'Get
            '    'Debug.Assert(IterationIndex <= Me.m_BatchData.nTFM, Me.ToString & ".BLimValue() Index out of range!")
            '    If IterationIndex <= Me.m_BatchData.nTFM Then
            '        Return Me.m_BatchData.tfmBbase(IterationIndex, Me.Index)
            '    End If
            '    'OH My.....
            '    Return cCore.NULL_VALUE
            'End Get

            'Set(ByVal value As Single)
            '    'Debug.Assert(IterationIndex <= Me.m_BatchData.nTFM, Me.ToString & ".BLimValue() Index out of range!")
            '    If IterationIndex <= Me.m_BatchData.nTFM Then
            '        Me.m_BatchData.tfmBbase(IterationIndex, Me.Index) = value
            '    End If
            'End Set
        End Property

        'Public Overrides Function GetVariable(VarName As EwEUtils.Core.eVarNameFlags, Optional iIndex As Integer = -9999, Optional iIndex2 As Integer = -9999, Optional iIndex3 As Integer = -9999) As Object

        '    Select Case VarName
        '        Case eVarNameFlags.MSETFMBLimValues
        '            Return Me.BLimValue(Index)
        '        Case eVarNameFlags.MSETFMBBaseValues
        '            Return Me.BBaseValue(Index)
        '            'Case eVarNameFlags.MSETFMFOptValues
        '            '    Return Me.FMaxValue(Index)
        '    End Select

        '    Return MyBase.GetVariable(VarName, iIndex, iIndex2, iIndex3)

        'End Function


        'Public Overrides Function SetVariable(VarName As EwEUtils.Core.eVarNameFlags, newValue As Object, Optional iSecondaryIndex As Integer = -9999) As Boolean
        '    Dim bdone As Boolean
        '    Select Case VarName
        '        Case eVarNameFlags.MSETFMBLimValues
        '            Me.BLimValue(iSecondaryIndex) = CSng(newValue)
        '            bdone = True
        '        Case eVarNameFlags.MSETFMBBaseValues
        '            Me.BBaseValue(Index) = CSng(newValue)
        '            bdone = True
        '            'Case eVarNameFlags.MSETFMFOptValues
        '            '    Me.m_BatchData.tfmFmax(Index, iSecondaryIndex) = CSng(newValue)
        '            '    bdone = True
        '    End Select

        '    If bdone Then
        '        Me.m_core.Messages.SendMessage(New cMessage("Values update.", eMessageType.DataModified, eCoreComponentType.MSE, _
        '                                                eMessageImportance.Maintenance, eDataTypes.MSEBatchTFMInput))
        '        Return True
        '    Else
        '        Return MyBase.SetVariable(VarName, newValue, iSecondaryIndex)
        '    End If


        'End Function


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
