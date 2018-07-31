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
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="eUnitCurrencyType"/> objects.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cCurrencyUnitFormatter
        Implements ITypeFormatter

        Private m_strCustom As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new type <see cref="eUnitCurrencyType"/>formatter.
        ''' </summary>
        ''' <param name="strCustom">Any custom unit text as entered by the user. 
        ''' by the user.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal strCustom As String)
            Me.m_strCustom = strCustom
        End Sub

        Public Function GetDescribedType() As System.Type Implements ITypeFormatter.GetDescribedType
            Return GetType(eUnitCurrencyType)
        End Function

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                  Implements ITypeFormatter.GetDescriptor

            Dim unit As eUnitCurrencyType = DirectCast(value, eUnitCurrencyType)

            Select Case unit
                Case eUnitCurrencyType.Calorie
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_CALORIE
                Case eUnitCurrencyType.Carbon
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_CARBON
                Case eUnitCurrencyType.DryWeight
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_DRYWEIGHT
                Case eUnitCurrencyType.Joules
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_JOULES
                Case eUnitCurrencyType.Nitrogen
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_NITROGEN
                Case eUnitCurrencyType.Phosporous
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_PHOSPOROUS
                Case eUnitCurrencyType.WetWeight
                    Return My.Resources.CoreDefaults.UNIT_CURRENCY_WETWEIGHT
            End Select

            If Not String.IsNullOrWhiteSpace(Me.m_strCustom) Then
                Return Me.m_strCustom
            End If

            Return String.Empty
        End Function

    End Class

End Namespace