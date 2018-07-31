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

Public Class cValidatorNumericSetToNull
    Inherits cValidatorDefault

    Public Overrides Function Validate(ByVal ValueObject As cValue, ByVal MetaData As cVariableMetaData,
                                         Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE,
                                         Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean

        Dim fmt As New Style.cVarnameTypeFormatter()

        ' JS 10Jan08: First check whether value is the one allowed NULL value. Secondly check
        ' whether the value fits within the allowed metadata range.
        ' The null value check is performed first because the allowed NULL value may fit within 
        ' the allowed metadata range; in this special case the variable status will be set to OK
        ' instead of NULL which is not correct.

        ' Check whether value equals the one allowed metadata null value
        If (CSng(ValueObject.Value(iSecondaryIndex)) = CSng(MetaData.NullValue)) Then
            'passed validation
            ValueObject.ValidationMessage = String.Format(My.Resources.CoreMessages.VARIABLE_VALIDATION_CLEARED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value)
            ValueObject.ValidationStatus = eStatusFlags.OK
            ValueObject.Status(iSecondaryIndex) = eStatusFlags.Null
            Return True
        End If

        ' Check whether value fits the allowed metadata range
        If MetaData.MinOperator.Compare(CSng(ValueObject.Value(iSecondaryIndex)), MetaData.Min) And
                MetaData.MaxOperator.Compare(CSng(ValueObject.Value(iSecondaryIndex)), MetaData.Max) Then
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

        ''failed the validation 
        'If Not MetaData.MinOperator.Compare(CType(ValueObject.Value(iSecondaryIndex), Single), MetaData.Min) Then
        '    'if the value is less than the min then status is FailedValidation
        '    ValueObject.ValidationMessage = String.Format(My.Resources.CoreMessages.VARIABLE_VALIDATION_CLEARED, fmt.GetDescriptor(ValueObject.varName))
        '    ValueObject.ValidationStatus = eStatusFlags.FailedValidation
        '    ValueObject.Status(iSecondaryIndex) = eStatusFlags.Null
        '    Return True
        'End If

        'If Not MetaData.MaxOperator.Compare(CType(ValueObject.Value(iSecondaryIndex), Single), MetaData.Max) Then
        '    'if the value is greater than max then status is FailedValidation
        '    ValueObject.ValidationMessage = String.Format(My.Resources.CoreMessages.VARIABLE_VALIDATION_FAILED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value)
        '    ValueObject.ValidationStatus = eStatusFlags.FailedValidation
        '    ' ValueObject.Status(iSecondaryIndex) = eStatusFlags.FailedValidation
        '    Return True
        'End If

    End Function

End Class