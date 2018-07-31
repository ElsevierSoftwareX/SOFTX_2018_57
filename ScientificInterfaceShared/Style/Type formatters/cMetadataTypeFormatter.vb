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
Imports EwECore
Imports EwEUtils.SystemUtilities
Imports System.Text
Imports EwECore.Style

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="cVariableMetadata"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cMetadataTypeFormatter
        Implements ITypeFormatter

        Private m_sg As cStyleGuide = Nothing
        Private m_units As cUnits = Nothing

        Public Sub New(core As cCore, sg As cStyleGuide)
            Me.m_sg = sg
            Me.m_units = New cUnits(core)
        End Sub

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            If (value Is Nothing) Then Return ""

            Debug.Assert(value.GetType.IsAssignableFrom(Me.GetDescribedType()))

            Dim md As cVariableMetaData = DirectCast(value, cVariableMetaData)

            Dim strUnits As String = Me.UnitText(md)
            Dim strDescr As String = Me.ValueText(md)

            Dim n As Integer = If(String.IsNullOrWhiteSpace(strUnits), 0, 1) + If(String.IsNullOrWhiteSpace(strDescr), 0, 1)
            Select Case n
                Case 1 : Return strUnits & strDescr
                Case 2 : Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_DOUBLE, strDescr, strUnits)
            End Select

            Return ""

        End Function

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(cVariableMetaData)
        End Function

#Region " Internals "

        Private Function ValueText(md As cVariableMetaData) As String

            ' ToDo: globalize this method

            Dim sbDescr As New StringBuilder()

            Select Case md.VarType
                Case ValueWrapper.eValueTypes.Bool, ValueWrapper.eValueTypes.BoolArray
                    sbDescr.Append(My.Resources.METADATA_BOOLEAN)

                Case ValueWrapper.eValueTypes.Int, ValueWrapper.eValueTypes.IntArray
                    If (md.Min > Integer.MinValue) Then
                        sbDescr.Append(CStr(md.Min) & " " & CStr(If((TypeOf md.MinOperator Is cGreaterThan), "<", "≤")) & " ")
                    End If
                    sbDescr.Append(My.Resources.METADATA_INTEGER)
                    If (md.Max < Integer.MaxValue) Then
                        sbDescr.Append(" " & CStr(If((TypeOf md.MinOperator Is cLessThan), "<", "≤")) & " " & CStr(md.Max))
                    End If

                Case ValueWrapper.eValueTypes.Sng, ValueWrapper.eValueTypes.SingleArray
                    If (md.Min > Single.MinValue) Then
                        sbDescr.Append(CStr(md.Min) & " " & CStr(If((TypeOf md.MinOperator Is cGreaterThan), "<", "≤")) & " ")
                    End If
                    sbDescr.Append(My.Resources.METADATA_SINGLE)
                    If (md.Max < Single.MaxValue) Then
                        sbDescr.Append(" " & CStr(If((TypeOf md.MinOperator Is cLessThan), "<", "≤")) & " " & CStr(md.Max))
                    End If

                Case ValueWrapper.eValueTypes.Str
                    Dim iMax As Integer = Math.Min(CInt(2 ^ 16) - 1, md.Length)
                    sbDescr.Append(String.Format(My.Resources.METADATA_TEXT, iMax))

            End Select

            Return sbDescr.ToString()
        End Function

        Private Function UnitText(md As cVariableMetaData) As String
            Return Me.m_units.ToString(md.Units)
        End Function

#End Region ' Internals

    End Class

End Namespace ' Style
