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
Imports EwEUtils.Database.cEwEDatabase
Imports System.Reflection

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' One single flow diagram.
''' </summary>
''' ===========================================================================
<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Name"), _
    Serializable()> _
Public Class cFlowDiagram
    : Inherits cOOPStorable

#Region " Properties "

    <Browsable(True), _
       DisplayName("Name"), _
       Description("Name of this diagram"),
       cPropertySorter.PropertyOrder(1)>
    Public Overridable Property Name() As String = "Default"

#End Region ' Properties

End Class
