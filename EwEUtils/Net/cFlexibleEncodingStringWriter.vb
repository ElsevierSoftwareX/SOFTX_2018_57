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

Imports System.IO
Imports System.Text

#End Region ' Imports

Namespace NetUtilities

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' String writer that accepts setting of diffferent <see cref="encoding">encoding formats</see>.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cFlexibleEncodingStringWriter
        Inherits StringWriter

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The <see cref="System.Text.encoding"/> format to use for this string writer.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property CustomEncoding As Encoding = Encoding.UTF8

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="StringWriter.Encoding"/>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return Me.CustomEncoding
            End Get
        End Property
    End Class

End Namespace
