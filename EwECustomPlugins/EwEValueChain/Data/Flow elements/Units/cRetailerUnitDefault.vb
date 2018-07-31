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
Imports System.ComponentModel
Imports EwEUtils.Utilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' This class holds defaults for <see cref="cRetailerUnit">retailer units</see>
''' in the Ecost model. Defaults are used as blueprints for spawning their base 
''' class objects.
''' </summary>
''' ===========================================================================
<TypeConverter(GetType(cPropertySorter)), _
    Serializable()> _
Public Class cRetailerUnitDefault
    Inherits cRetailerUnit

    <Browsable(False)> _
    Public Overrides Property Name() As String
        Get
            Return "Default"
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    <Browsable(False)> _
    Public Overrides ReadOnly Property Category() As String
        Get
            Return ""
        End Get
    End Property

End Class
