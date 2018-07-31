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
Imports System.Text

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="cStyleGuide.eStyleFlags"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>This class tries to obtain a string from the ScientificShared resources.
    ''' The resource string is expected to be named and formatted as follows:</para>
    ''' <para>AUTOSAVE_[varname] = "[symbol]|[abbr]|[name]|[description]"</para>
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Class cStyleTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Debug.Assert(value.GetType.IsAssignableFrom(Me.GetDescribedType()))

            Dim sb As New StringBuilder()
            Dim flags As cStyleGuide.eStyleFlags = DirectCast(value, cStyleGuide.eStyleFlags)

            ' Perform bitwise comparision of status flags
            For Each sf As cStyleGuide.eStyleFlags In [Enum].GetValues(GetType(cStyleGuide.eStyleFlags))
                If ((flags And sf) = sf) Then

                    Dim strValue As String = sf.ToString
                    Dim strDescr As String = cResourceUtils.LoadString("STYLEFLAGS_" & strValue.ToUpper, My.Resources.ResourceManager)
                    Dim astrBits As String() = Nothing
                    Dim iNumBits As Integer = 0
                    Dim strBit As String = ""

                    If (strDescr IsNot Nothing) Then
                        astrBits = strDescr.Split("|"c)
                        iNumBits = astrBits.Length
                    End If

                    For i As Integer = 0 To Math.Min(descriptor, iNumBits)

                        If i < iNumBits Then
                            ' Has a part?
                            If Not String.IsNullOrEmpty(astrBits(i)) Then
                                ' #Yes: update bit
                                strBit = astrBits(i).Trim
                            End If
                        End If

                    Next

                    If Not String.IsNullOrWhiteSpace(strBit) Then
                        sb.AppendLine(strBit)
                    End If
                End If
            Next

            Return sb.ToString()

        End Function

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(cStyleGuide.eStyleFlags)
        End Function

    End Class

End Namespace ' Style
