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

Option Strict On

Public Class cENARDataStructures

    Public nGroups As Integer

    Public b() As Single
    Public Resp() As Single
    Public Consumpt(,) As Single

    ''' <summary>
    ''' Catch for all fished groups. Exported detritus for detrius groups
    ''' </summary>
    Public CatchExport() As Single

    ''' <summary>
    ''' Production for Primary Producers. Imported diet/consumption of consumer groups
    ''' </summary>
    Public Import() As Single


    Public Sub New(NumberOfGroups As Integer)

        nGroups = NumberOfGroups

        b = New Single(nGroups) {}
        Resp = New Single(nGroups) {}
        CatchExport = New Single(nGroups) {}
        Import = New Single(nGroups) {}
        Consumpt = New Single(nGroups, nGroups) {}

    End Sub

End Class

