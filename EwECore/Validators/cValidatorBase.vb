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
Imports EwEUtils.Utilities

''' <summary>
''' Default validator for all data types
''' </summary>
Public Class cValidatorDefault

    Protected m_VarName As eVarNameFlags

    Sub New(ByVal VarName As eVarNameFlags)
        m_VarName = VarName
    End Sub

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    ''' <remarks></remarks>
    Sub New()
        m_VarName = eVarNameFlags.NotSet
    End Sub

    ''' <summary>
    ''' Variable name of variable to validate.
    ''' </summary>
    ''' <remarks>This is set in the constructor.</remarks>
    Public ReadOnly Property VarName() As eVarNameFlags
        Get
            Return m_VarName
        End Get
    End Property

    Public Overridable Function Validate(ByVal ValueObject As cValue, ByVal MetaData As cVariableMetaData,
                                         Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE,
                                         Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean

        Dim bCleared As Boolean = False

        Select Case ValueObject.varType

            Case eValueTypes.Int, eValueTypes.IntArray, eValueTypes.Sng, eValueTypes.SingleArray
                'numeric values
                Dim sValue As Single = CSng(ValueObject.Value(iSecondaryIndex, iThirdIndex))
                If MetaData.MinOperator.Compare(sValue, MetaData.Min) And MetaData.MaxOperator.Compare(sValue, MetaData.Max) Then
                    'passed validation
                    ValueObject.ValidationStatus = eStatusFlags.OK
                    ValueObject.Status(iSecondaryIndex, iThirdIndex) = eStatusFlags.OK
                Else
                    ' Check vs default out of [min, max] range
                    If (sValue = CSng(MetaData.NullValue)) Then
                        'passed validation
                        ValueObject.ValidationStatus = eStatusFlags.OK
                    Else
                        'failed the validation 
                        ValueObject.ValidationStatus = eStatusFlags.FailedValidation
                    End If

                    ' Always flag successfully validated cCore.NULL_VALUE values as Null
                    If sValue = cCore.NULL_VALUE And ValueObject.ValidationStatus = eStatusFlags.OK Then
                        ValueObject.Status(iSecondaryIndex) = eStatusFlags.Null
                        bCleared = True
                    End If

                End If


            Case eValueTypes.Str
                'strings

                'no null strings
                If ValueObject.Value Is Nothing Then
                    ValueObject.ValidationStatus = eStatusFlags.FailedValidation
                    ValueObject.Status(iSecondaryIndex) = eStatusFlags.Null
                End If

                If ValueObject.Value.ToString.Length <= MetaData.Length Then
                    ValueObject.ValidationStatus = eStatusFlags.OK
                    ValueObject.Status(iSecondaryIndex) = eStatusFlags.OK
                Else
                    ValueObject.ValidationStatus = eStatusFlags.FailedValidation
                End If

            Case eValueTypes.Bool, eValueTypes.BoolArray
                'all boolean values are OK
                ValueObject.ValidationStatus = eStatusFlags.OK
                ValueObject.Status(iSecondaryIndex) = eStatusFlags.OK

        End Select

        Dim fmt As New Style.cVarnameTypeFormatter()

        ' Prepare message
        If ValueObject.ValidationStatus = eStatusFlags.OK Then
            If bCleared Then
                ValueObject.ValidationMessage = cStringUtils.Localize(My.Resources.CoreMessages.VARIABLE_VALIDATION_CLEARED, fmt.GetDescriptor(ValueObject.varName))
            Else
                If TypeOf ValueObject.Value Is System.Array And iSecondaryIndex >= 0 Then
                    ValueObject.ValidationMessage = cStringUtils.Localize(My.Resources.CoreMessages.VARIABLE_VALIDATION_PASSED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value(iSecondaryIndex, iThirdIndex))
                Else
                    ValueObject.ValidationMessage = cStringUtils.Localize(My.Resources.CoreMessages.VARIABLE_VALIDATION_PASSED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value)
                End If
            End If
        Else
            If TypeOf ValueObject.Value Is System.Array And iSecondaryIndex >= 0 Then
                ValueObject.ValidationMessage = cStringUtils.Localize(My.Resources.CoreMessages.VARIABLE_VALIDATION_FAILED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value(iSecondaryIndex, iThirdIndex))
            Else
                ValueObject.ValidationMessage = cStringUtils.Localize(My.Resources.CoreMessages.VARIABLE_VALIDATION_FAILED, fmt.GetDescriptor(ValueObject.varName), ValueObject.Value)
            End If
        End If

        Return True

    End Function

End Class

