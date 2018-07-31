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
''' Validate output objects. This will set the status flag to a value appropriate to the output
''' </summary>
''' <remarks></remarks>
Public Class cValidatorOutput
    Inherits cValidatorDefault

    Dim m_defaultstatus As eStatusFlags

    Public Sub New(ByVal DefaultStatus As eStatusFlags)

        m_defaultstatus = DefaultStatus

    End Sub

    Public Overrides Function Validate(ByVal ValueObject As cValue, ByVal MetaData As cVariableMetaData, Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE) As Boolean
        'Ok for now there is no validation of output values!!! this just sets the status flag
        'if the model set the value it is assumed to be OK
        'if there is a problem then the core will need to set the status flag some other way
        'For Now

        ValueObject.Status(iSecondaryIndex) = m_defaultstatus 'the default status was passed in during construction of this object 
        ValueObject.ValidationStatus = eStatusFlags.OK
        Return True

    End Function

End Class