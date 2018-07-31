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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="IShapeFunction"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>This class provides a localized representation of a <see cref="IShapeFunction">shape function</see>.</para>
    ''' <para>If the function is defined as a predefined <see cref="eShapeFunctionType">primitive</see>, 
    ''' the <see cref="cShapeFunctionTypeFormatter">localized version of that primitive</see>
    ''' is returned.</para>
    ''' <para>If the shape function is derived from a <see cref="EwEPlugin.IEcosimShapeFunctionPlugin">plug-in</see>,
    ''' the <see cref="EwEPlugin.IEcosimShapeFunctionPlugin.DisplayName">display name</see></para> 
    ''' of that plug-in is returned instead.
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Class cShapeFunctionFormatter
        Implements ITypeFormatter

        ''' <inheritdocs cref="ITypeFormatter.GetDescribedType"/>
        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(IShapeFunction)
        End Function

        ''' <inheritdocs cref="ITypeFormatter.GetDescriptor"/>
        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            ' Plug-in name discovery takes presedence, as plug-ins may inherit cShapeFunction
            If (TypeOf value Is EwEPlugin.IEcosimShapeFunctionPlugin) Then
                Return DirectCast(value, EwEPlugin.IEcosimShapeFunctionPlugin).DisplayName
            ElseIf (TypeOf value Is cShapeFunction) Then
                Dim fmt As New cShapeFunctionTypeFormatter()
                Return fmt.GetDescriptor(DirectCast(value, cShapeFunction).ShapeFunctionType)
            End If
            ' Hmm
            Return value.ToString

        End Function

    End Class

End Namespace
