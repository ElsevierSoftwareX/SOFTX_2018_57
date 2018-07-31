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

Namespace ValueWrapper

    Public Class cValueArrayTripleIndex
        Inherits cValueArrayIndexed

        Shadows m_statusarray(,) As eStatusFlags

        Protected m_counter2 As eCoreCounterTypes
        ' Protected m_counter3 As eCoreCounterTypes

        Public Property iThirdIndex As Integer

        Sub New(ByVal theValueType As eValueTypes, ByVal VarName As eVarNameFlags, ByVal Status As eStatusFlags,
                ByVal CounterType As eCoreCounterTypes, ByVal CounterType2 As eCoreCounterTypes,
                ByRef CounterDelegate As CoreCounterDelegate, ByVal iFixedIndex1 As Integer, ByVal DataType As eDataTypes)

            MyBase.New(theValueType, VarName, Status, CounterType2, Nothing, iFixedIndex1, DataType)

            m_Countertype = CounterType
            m_counter2 = CounterType2

            m_CounterDelegate = CounterDelegate
            'm_counter3 = CounterType3

            varType = theValueType
            m_varName = VarName
            'm_dataType = DataType
            m_iArrayIndex = iFixedIndex1
            Me.Index = iFixedIndex1


            If SetSize() Then 'this will redim the arrays and set m_nObjects

                Dim n1 As Integer = m_CounterDelegate(m_Countertype)
                Dim n2 As Integer = m_CounterDelegate(m_counter2)
                For i As Integer = 1 To n1
                    For j As Integer = 1 To n2
                        m_statusarray(i, j) = Status
                    Next
                Next

            Else
                Debug.Assert(False, "Something is wrong in " & Me.ToString & ".New()")
            End If

        End Sub



        ''' <summary>
        ''' Set the size of the array to the value in the cores data counter i.e. nGroups
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>This will only dimension the array data if the core counter is of a different size then the existing data.
        '''  Once the data has been resized it will need to be repopulated.</remarks>
        Public Overrides Function SetSize() As Boolean

            Dim bCountersSet As Boolean = Not (m_Countertype = eCoreCounterTypes.NotSet Or m_counter2 = eCoreCounterTypes.NotSet)
            If m_CounterDelegate IsNot Nothing And bCountersSet Then

                Dim index1 As Integer = m_CounterDelegate(m_Countertype)
                Dim index2 As Integer = m_CounterDelegate(m_counter2)

                'only resize the data if it is different
                If index1 <> m_nObjects Then
                    m_nObjects = index1
                    Select Case Me.varType
                        Case eValueTypes.BoolArray
                            Dim b(,) As Boolean = New Boolean(index1, index2) {}
                            m_values = b
                        Case eValueTypes.IntArray
                            Dim i(,) As Integer = New Integer(index1, index2) {}
                            m_values = i
                        Case eValueTypes.SingleArray
                            Dim s(,) As Single = New Single(index1, index2) {}
                            m_values = s
                    End Select

                    m_statusarray = New eStatusFlags(index1, index2) {}

                End If

                Return True

            Else
                'System.Console.WriteLine(Me.ToString & ".setSize() not implemented.")
                'When a cValueArrayIndexed object in constructed it will call the base class constructor will a null m_CounterDelegate
                'which in turn calls this method before cValueArrayIndexed has had a chance to set m_CounterDelegate
                Return False
            End If

        End Function


        ''' <summary>
        ''' Get/set the actual value of a Value.
        ''' </summary>
        ''' <param name="iIndex2">Optional value index.</param>
        Public Overrides Property Value(Optional ByVal iIndex2 As Integer = cCore.NULL_VALUE, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object
            Get
                'Return m_values(Me.Index, iIndex, iIndex2)

                If iIndex2 = cCore.NULL_VALUE And iIndex3 = cCore.NULL_VALUE Then
                    'pass it on down the chain
                    Return MyBase.Value(iIndex2)
                Else
                    'Index 1 is implied
                    'It's the Me.Index property of this group or fleet the object was created for
                    Return DirectCast(m_values, Array).GetValue(iIndex2, iIndex3)
                End If


            End Get
            Set(ByVal value As Object)

                If iIndex2 = cCore.NULL_VALUE And iIndex3 = cCore.NULL_VALUE Then
                    'pass it on down the chain
                    Validate(value)
                Else
                    Me.iSecondIndex = iIndex2
                    Me.iThirdIndex = iIndex3
                    'Index 1 is implied
                    'It's the Me.Index property of this group or fleet the object was created for
                    Validate(value, iIndex2, iIndex3)
                End If

            End Set
        End Property



        ''' <summary>
        ''' Validate an array value object
        ''' </summary>
        ''' <param name="NewValue"></param>
        ''' <param name="iSecondIndex"></param>
        ''' <returns></returns>
        ''' <remarks>This can not be handled by the cValue base class because the underlying data is handled differently. Array values are stored in an array (duh...)</remarks>
        Protected Overrides Function Validate(ByRef NewValue As Object,
                                            Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE,
                                            Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean

            'convert null or empty inputs into something that can be used
            NewValue = Me.convertEmptyInputs(NewValue)

            ' JS 06Mar11: Array.SetValue cannot perform certain type conversions, such as Single to Integer.
            '             If an integer array receives a single value Array.SetValue will throw an exception.
            '             A dynamic type conversion will prevent this problem.

            ' Determine the type that this array accepts
            Dim arr As Array = DirectCast(m_values, Array)
            Dim tArr As Type = arr.GetType.GetElementType

            'set the value to the newvalue 
            'keep the old value in case the newvalue fails validation
            Me.m_orgvalue = arr.GetValue(iSecondIndex, iThirdIndex)
            arr.SetValue(Convert.ChangeType(NewValue, tArr), iSecondIndex, iThirdIndex)

            ' Unable to validate? Boot out of here
            If (Not Me.AllowValidation) Or (Me.m_validator Is Nothing) Then
                Me.m_validationstatus = eStatusFlags.OK
                Return False
            End If

            'Ok run the validator
            If m_validator.Validate(Me, m_metadata, iSecondIndex, iThirdIndex) Then

                If m_validationstatus = eStatusFlags.FailedValidation Then
                    'if the new value failed validation then set the value back to it's original value
                    Try
                        arr.SetValue(Me.m_orgvalue, iSecondIndex, iThirdIndex)
                    Catch ex As Exception
                        Debug.Assert(False, "Failed to reset value")
                    End Try
                End If

                If m_statusarray(iSecondIndex, iThirdIndex) = eStatusFlags.Null Then
                    ' m_values(iSecondaryIndex) = m_metadata.NullValue
                    Try
                        arr.SetValue(Convert.ChangeType(m_metadata.NullValue, tArr), iSecondIndex, iThirdIndex)
                    Catch ex As Exception
                        Debug.Assert(False, "Failed to set default value")
                    End Try
                End If

            End If

            Return True ' validation run

        End Function



        ''' <summary>
        ''' Run the validator to set the status flag without setting the value
        ''' </summary>
        ''' <param name="iSecondIndex"></param>
        ''' <remarks>This is use be the cCoreInputOutputBase to set the status flags of all its values </remarks>
        Public Overrides Sub setStatusFlag(Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE,
                                            Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE)

            If m_validator IsNot Nothing Then
                '  Me.Status(iSecondIndex, iThirdIndex) = eStatusFlags.OK

                Dim iGrp As Integer = iSecondIndex
                ' Dim n1 As Integer = m_CounterDelegate(m_counter2)
                Dim nTimeSteps As Integer = m_CounterDelegate(m_counter2)
                ' For i As Integer = 1 To n1
                For its As Integer = 1 To nTimeSteps
                    'm_statusarray(i, j) = eStatusFlags.OK
                    m_validator.Validate(Me, m_metadata, iGrp, its)
                Next
                '   Next

                ' m_validator.Validate(Me, m_metadata, iSecondIndex, iThirdIndex)
            Else
                ' System.Console.WriteLine("No validator definded for " & m_varType.ToString)
            End If

        End Sub

        ''' <summary>
        ''' Get/set the status flag for a Value.
        ''' </summary>
        ''' <param name="iSecondIndex">Optional value index.</param>
        Public Overrides Property Status(Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE, Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As eStatusFlags
            Get
                Return m_statusarray(iSecondIndex, iThirdIndex)
            End Get
            Friend Set(ByVal value As eStatusFlags)
                m_statusarray(iSecondIndex, iThirdIndex) = value
            End Set
        End Property



        Public Overrides ReadOnly Property Length() As Integer
            Get
                Return m_CounterDelegate(m_Countertype)
            End Get
        End Property



        Public Overrides Sub Dispose()
            MyBase.Dispose()

            Me.m_CounterDelegate = Nothing

        End Sub

    End Class


End Namespace
