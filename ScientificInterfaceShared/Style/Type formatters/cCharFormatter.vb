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

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of a <see cref="Char">character</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cCharFormatter
        Implements ITypeFormatter

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Debug.Assert(value.GetType.IsAssignableFrom(Me.GetDescribedType()))

            Dim iChar As Integer = 0
            Dim c As Char = Nothing
            Dim strText As String

            c = DirectCast(value, Char)
            iChar = Convert.ToInt16(c)

            ' Create texttual representation of the char
            Select Case DirectCast(iChar, Keys)

                Case Keys.None, Keys.F1 To Keys.F24
                    strText = DirectCast(iChar, Keys).ToString
                Case Keys.Enter
                    strText = My.Resources.GENERIC_CHAR_ENTER
                Case Keys.Escape
                    strText = My.Resources.GENERIC_CHAR_ESCAPE
                Case Keys.Space
                    strText = My.Resources.GENERIC_CHAR_SPACE
                Case Keys.Tab
                    strText = My.Resources.GENERIC_CHAR_TAB
                Case Else
                    Select Case c
                        Case "."c : strText = My.Resources.GENERIC_CHAR_PERIOD
                        Case ","c : strText = My.Resources.GENERIC_CHAR_COMMA
                        Case ":"c : strText = My.Resources.GENERIC_CHAR_COLON
                        Case ";"c : strText = My.Resources.GENERIC_CHAR_SEMICOLON
                        Case Else
                            strText = c
                    End Select
            End Select
            Return strText

        End Function

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(Char)
        End Function

    End Class

End Namespace ' Style
