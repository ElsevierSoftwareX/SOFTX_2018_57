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
''' Validate the value via one of the core counters
''' </summary>
''' <remarks></remarks>
Public Class cValidatorCounter
    Inherits cValidatorDefault

    Private m_core As cCore
    Private m_counter As eCoreCounterTypes

    Public Sub New(ByRef theCore As cCore, ByVal counterType As eCoreCounterTypes)
        m_core = theCore
        m_counter = counterType
    End Sub


    Public Overrides Function Validate(ByVal ValueObject As cValue, ByVal MetaData As cVariableMetaData,
                                         Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE,
                                         Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean

        Try
            Dim fmt As New Style.cVarnameTypeFormatter()
            Dim n As Integer = m_core.GetCoreCounter(m_counter)

            If MetaData.MinOperator.Compare(CSng(ValueObject.Value(iSecondaryIndex)), 0) And
             MetaData.MaxOperator.Compare(CSng(ValueObject.Value(iSecondaryIndex)), n) Then
                'passed validation
                ValueObject.ValidationMessage = String.Format(My.Resources.CoreMessages.VARIABLE_VALIDATION_PASSED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value)
                ValueObject.ValidationStatus = eStatusFlags.OK
                ValueObject.Status(iSecondaryIndex) = eStatusFlags.OK
                Return True
            End If

            ' JS 09Jan08: If validation failed, set status to Failed Validation at any time.
            ValueObject.ValidationMessage = String.Format(My.Resources.CoreMessages.VARIABLE_VALIDATION_FAILED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value)
            ValueObject.ValidationStatus = eStatusFlags.FailedValidation
            Return True


        Catch ex As Exception
            cLog.Write(ex)
            Return False
        End Try


    End Function

End Class