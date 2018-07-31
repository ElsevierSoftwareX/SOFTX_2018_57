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
' Copyright 1991-2013 UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
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
    ''' Class for providing a textual description of <see cref="eAutosaveTypes"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>This class tries to obtain a string from the ScientificShared resources.
    ''' The resource string is expected to be named and formatted as follows:</para>
    ''' <para>AUTOSAVE_[varname] = "[symbol]|[abbr]|[name]|[description]"</para>
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Class cShapeDataTypeFormatter
        Implements ITypeFormatter

        Private m_strNone As String = ""

        Public Sub New()
            Me.New(My.Resources.GENERIC_VALUE_NONE)
        End Sub

        Public Sub New(ByVal strNone As String)
            Me.m_strNone = strNone
        End Sub

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            If (value Is Nothing) Then Return Me.m_strNone

            Try
                Dim obj As cShapeData = DirectCast(value, cShapeData)
                ' Only include index in desciptor only if object has a valid index
                If (obj.Index >= 1) Then
                    Return String.Format(My.Resources.GENERIC_LABEL_INDEXED, obj.Index, obj.Name)
                End If
                Return obj.Name
            Catch ex As Exception
                Debug.Assert(False)
            End Try
            Return value.ToString

        End Function

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(cForcingFunction)
        End Function

    End Class

End Namespace ' Style
