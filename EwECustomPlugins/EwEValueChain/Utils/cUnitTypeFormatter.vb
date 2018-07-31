﻿' ===============================================================================
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
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' 
''' </summary>
Public Class cUnitTypeFormatter
    Implements ITypeFormatter

    Public Function GetDescribedType() As System.Type _
        Implements ITypeFormatter.GetDescribedType
        Return GetType(cUnitFactory.eUnitType)
    End Function

    Public Function GetDescriptor(value As Object, Optional descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String Implements _
        ITypeFormatter.GetDescriptor

        Dim strValue As String = value.ToString
        Dim strDescr As String = cResourceUtils.LoadString("UNIT_TYPE_" & strValue.ToUpper, Me.GetType.Assembly)
        Dim astrBits As String() = Nothing
        Dim iNumBits As Integer = 0
        Dim strBit As String = ""

        If (strDescr IsNot Nothing) Then
            astrBits = strDescr.Split("|"c)
            iNumBits = astrBits.Length
        End If

        For i As Integer = 0 To Math.Min(descriptor, iNumBits)

            ' Is first part?
            If (i = 0) Then
                ' #Yes: remember default
                strBit = strValue
            End If

            If i < iNumBits Then
                ' Has a part?
                If Not String.IsNullOrEmpty(astrBits(i)) Then
                    ' #Yes: update bit
                    strBit = astrBits(i).Trim
                End If
            End If

        Next
        Return strBit
    End Function

End Class
