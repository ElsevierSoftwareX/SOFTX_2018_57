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
''' Have the core do the data validation via it's cCore.Validate() method
''' </summary>
''' <remarks>This is used for variables that need to use values from other parts of the core for data validation</remarks>
Public Class cValidatorCore
    Inherits cValidatorDefault

    Private m_core As cCore

    Public Sub New(ByRef theCore As cCore)
        m_core = theCore
    End Sub

    Public Overrides Function Validate(ByVal ValueObject As cValue, ByVal MetaData As cVariableMetaData,
                                         Optional ByVal iSecondaryIndex As Integer = cCore.NULL_VALUE,
                                         Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean
        'Call Validate in the core to do the validation
        Return m_core.Validate(ValueObject, MetaData, iSecondaryIndex, iThirdIndex)

    End Function

End Class