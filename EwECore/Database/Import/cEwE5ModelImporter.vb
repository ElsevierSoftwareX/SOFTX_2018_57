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
Imports EwEUtils.Database
Imports EwEUtils.Core
Imports System.Text
Imports System.IO

#End Region ' Imports

Namespace Database

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing an importer to convert an EwE5 document
    ''' into an EwE6 database.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public MustInherit Class cEwE5ModelImporter
        Inherits cModelImporter

#Region " Private vars "

        ''' <summary>EWE5 NULL value.</summary>
        Protected Const cEWE5_NULL As Integer = -90

#End Region ' Private vars

#Region " Construction "

        Public Sub New(ByVal core As cCore)
            MyBase.New(core)
        End Sub

#End Region ' Construction

    End Class

End Namespace
