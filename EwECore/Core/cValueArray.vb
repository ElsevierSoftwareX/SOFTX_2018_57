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

    ''' <summary>
    ''' Provides an implemention of cValue that is used for Array values
    ''' </summary>
    ''' <remarks>At this time the internal array is weak typed as an object</remarks>
    Public Class cValueArray
        Inherits cValue

        Protected m_statusarray() As eStatusFlags
        Protected m_values As Object
        Protected m_nObjects As Integer = cCore.NULL_VALUE 'number of object in the array
        Protected m_CounterDelegate As CoreCounterDelegate = Nothing
        Protected m_Countertype As eCoreCounterTypes


        Sub New(ByVal theValueType As eValueTypes, ByVal VarName As eVarNameFlags, ByVal Status As eStatusFlags, ByVal CounterType As eCoreCounterTypes,
                ByVal CounterDelegate As CoreCounterDelegate, ByVal MetaData As cVariableMetaData, ByVal Validator As cValidatorDefault)
            MyBase.New(Nothing, VarName, Status, theValueType, MetaData, Validator)

            varType = theValueType
            m_varName = VarName

            ' Sanity check
            Debug.Assert(Me.m_metadata IsNot Nothing)

            ' JS 01may17: Do not overwrite base class smartness
            'm_validator = Validator

            m_CounterDelegate = CounterDelegate
            m_Countertype = CounterType
            Me.m_bStored = True

            If SetSize() Then 'this will redim the arrays and set m_nObjects
                For i As Integer = 0 To m_nObjects
                    m_statusarray(i) = Status
                Next
            End If

        End Sub

        ''' <summary>
        ''' Construct a value object of array data that does not do data validation
        ''' </summary>
        ''' <param name="VarName">eVarNameFlags of the data to hold</param>
        ''' <param name="Status">Default status</param>
        ''' <param name="CounterType">Type of core counter to use for dimensioning the array</param>
        ''' <param name="CounterDelegate">Delegate supplied by the core use to retrieve the size of the data</param>
        ''' <remarks></remarks>
        Sub New(ByVal theValueType As eValueTypes, ByVal VarName As eVarNameFlags, ByVal Status As eStatusFlags,
                ByVal CounterType As eCoreCounterTypes, ByVal CounterDelegate As CoreCounterDelegate)
            Me.New(theValueType, VarName, Status, CounterType, CounterDelegate, Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' Set the size of the array to the value in the cores data counter i.e. nGroups
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>This will only dimension the array data if the core counter is of a different size then the existing data.
        '''  Once the data has been resized it will need to be repopulated.</remarks>
        Public Overrides Function SetSize() As Boolean

            If m_CounterDelegate IsNot Nothing Then

                Dim newsize As Integer = m_CounterDelegate(m_Countertype)

                'only resize the data if it is different
                If newsize <> m_nObjects Then
                    m_nObjects = newsize

                    Select Case Me.varType
                        Case eValueTypes.BoolArray
                            Dim s(m_nObjects) As Boolean
                            m_values = s
                        Case eValueTypes.IntArray
                            Dim s(m_nObjects) As Integer
                            m_values = s
                        Case eValueTypes.SingleArray
                            Dim s(m_nObjects) As Single
                            m_values = s
                    End Select

                    ReDim m_statusarray(m_nObjects)

                    For i As Integer = 0 To m_nObjects
                        m_statusarray(i) = eStatusFlags.Null
                    Next
                End If

                Return True

            Else
                System.Console.WriteLine(Me.ToString & ".setSize() not implemented.")
                Return False
            End If

        End Function

        Public Overrides Property Status(Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE,
                                         Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As eStatusFlags
            Get
                If iSecondaryIndex <> cCore.NULL_VALUE Then
                    Return m_statusarray(iSecondaryIndex)
                Else
                    'if iSecondaryIndex is NULL for an arrayed value then return NULL
                    'we have no way of know what the user wanted
                    Return eStatusFlags.Null
                End If
            End Get
            Friend Set(ByVal value As eStatusFlags)
                If iSecondaryIndex <> cCore.NULL_VALUE Then
                    m_statusarray(iSecondaryIndex) = value
                Else
                    'no index so set all status flags to the new value
                    For i As Integer = 1 To m_nObjects
                        m_statusarray(i) = value
                    Next
                End If
            End Set
        End Property

        Public Overrides Property Value(Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE, Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Object

            Get
                Try
                    If iSecondaryIndex <> cCore.NULL_VALUE Then
                        'Debug.Assert(iSecondaryIndex <= m_nObjects And iSecondaryIndex >= 0, String.Format("{0}.Value({1}, {2}) secondary index out of bounds", Me.ToString(), Me.m_varName, iSecondaryIndex))
                        Return DirectCast(m_values, Array).GetValue(iSecondaryIndex)
                    Else
                        Return m_values
                    End If
                Catch ex As Exception
                    Debug.Assert(False, Me.ToString & ".Value Error: " & ex.Message)
                    Return Nothing
                End Try

            End Get

            Set(ByVal value As Object)

                Try
                    If TypeOf value Is System.Array Then
                        'no data validation on arrays
                        'Oh my..........
                        Try
                            System.Array.Copy(DirectCast(value, Array), DirectCast(m_values, Array), DirectCast(m_values, Array).Length)
                        Catch ex As Exception
                            Debug.Assert(False, Me.ToString & ".Value() Failed to convert value to array.")
                            Me.Status = eStatusFlags.ErrorEncountered ' I think this will work???
                        End Try

                    Else
                        Debug.Assert(iSecondaryIndex <= m_nObjects And iSecondaryIndex >= 0, Me.ToString & ".Value() iGroup out of bounds.")
                        Validate(value, iSecondaryIndex)
                    End If
                Catch ex As Exception
                    Debug.Assert(False, Me.ToString & ".Value Error: " & ex.Message)
                End Try

            End Set

        End Property

        Public Overrides ReadOnly Property Length() As Integer
            Get
                Return m_nObjects
            End Get
        End Property

        Public ReadOnly Property CoreCounterType() As eCoreCounterTypes
            Get
                Return Me.m_Countertype
            End Get
        End Property

        Public Overrides ReadOnly Property IsArray() As Boolean
            Get
                Return True
            End Get
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
            Me.m_orgvalue = arr.GetValue(iSecondIndex)
            arr.SetValue(Convert.ChangeType(NewValue, tArr), iSecondIndex)

            ' Unable to validate? Boot out of here
            If (Not Me.AllowValidation) Or (Me.m_validator Is Nothing) Then
                Me.m_validationstatus = eStatusFlags.OK
                Return False
            End If

            'Ok run the validator
            If m_validator.Validate(Me, m_metadata, iSecondIndex) Then

                If m_validationstatus = eStatusFlags.FailedValidation Then
                    'if the new value failed validation then set the value back to it's original value
                    Try
                        arr.SetValue(Me.m_orgvalue, iSecondIndex)
                    Catch ex As Exception
                        Debug.Assert(False, "Failed to reset value")
                    End Try
                End If

                If m_statusarray(iSecondIndex) = eStatusFlags.Null Then
                    ' m_values(iSecondaryIndex) = m_metadata.NullValue
                    Try
                        arr.SetValue(Convert.ChangeType(m_metadata.NullValue, tArr), iSecondIndex)
                    Catch ex As Exception
                        Debug.Assert(False, "Failed to set default value")
                    End Try
                End If

            End If

            Return True ' validation run

        End Function

        Public Overrides Sub Dispose()
            MyBase.Dispose()
            Me.m_values = Nothing
            Me.m_CounterDelegate = Nothing
        End Sub

    End Class

End Namespace
