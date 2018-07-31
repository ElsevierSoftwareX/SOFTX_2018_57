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
Imports System

#End Region ' Imports 

Namespace Utilities

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type specifying the types of descriptors that an ITypeFormatter can return.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eDescriptorTypes As Integer
        ''' <summary>A single letter; the briefest of representations.</summary>
        Symbol = 0
        ''' <summary>An abbreviation.</summary>
        Abbreviation
        ''' <summary>A spelled-out name.</summary>
        Name
        ''' <summary>A full description.</summary>
        Description
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing classes that provide string representations
    ''' for objects and enumerated types in EwE.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface ITypeFormatter

        Function GetDescribedType() As Type

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtains a textual representation for an object.
        ''' </summary>
        ''' <param name="value">The object to provide a textual representation for.</param>
        ''' <param name="descriptor">The <see cref="eDescriptorTypes">representation</see> to provide.</param>
        ''' <returns>A textual representation.</returns>
        ''' -------------------------------------------------------------------
        Function GetDescriptor(ByVal value As Object, _
                               Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String

    End Interface

End Namespace
