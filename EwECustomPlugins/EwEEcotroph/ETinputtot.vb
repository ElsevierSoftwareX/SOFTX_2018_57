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
Imports System.Xml.Serialization
Imports EwEUtils.Core

#End Region ' Imports

Public Class ETinputtot

    Public ModelName As String
    Public ModelDescription As String
    Public Comments As String
    Public NumGroups As Integer
    Public NumLivingGroups As Integer
    Public NumFleet As Integer
    Public GroupName() As String
    Public FleetName() As String
    ''' <summary>This is a model output</summary>
    Public TL() As Single
    ''' <summary>Is this absolute B?</summary>
    Public B() As Single
    ''' <summary>Is this absolute P?</summary>
    Public PROD() As Single
    ''' <summary>What is this?</summary>
    Public accessibility() As Single
    ''' <summary>What is this?</summary>
    Public OI() As Single
    ''' <summary>How is this indexed, (Fleet x group) or (group x fleet)?</summary>
    Public Catches()() As Single

End Class
