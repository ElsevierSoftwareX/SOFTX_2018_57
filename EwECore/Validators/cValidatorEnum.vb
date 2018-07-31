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
Imports EwECore.ValueWrapper

Public Class cValidatorEnum
    Inherits cValidatorDefault

    Private m_type As Type = Nothing

    Public Sub New(ByVal t As Type)
        Debug.Assert(t.IsEnum)
        Me.m_type = t
    End Sub

    Public Overrides Function Validate(ByVal ValueObject As cValue, ByVal MetaData As cVariableMetaData,
                                         Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE,
                                         Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean

        ' ToDo: add support for FlagsAttribute enums

        ' Perform 'normal' validation
        If Not MyBase.Validate(ValueObject, MetaData, iSecondaryIndex) Then Return False
        ' Check type
        If Not [Enum].IsDefined(Me.m_type, ValueObject.Value(iSecondaryIndex)) Then
            ValueObject.ValidationStatus = eStatusFlags.FailedValidation
        Else
            ValueObject.ValidationStatus = eStatusFlags.OK
        End If
        Return True

    End Function

End Class