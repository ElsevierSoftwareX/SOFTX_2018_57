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

''' <summary>
''' Group and Weight of a Group that make up a Mediating Group for a Mediation function. There can be more then one cMediatingGroup for a Mediation Function
''' </summary>
''' <remarks>This is the Group(s) that provide the Biomass for the X axis of a mediation function</remarks>
Public Class cMediatingGroup

    Public iGroupIndex As Integer
    Public Weight As Single

    ''' <summary>
    ''' Build a new Mediation Group
    ''' </summary>
    ''' <param name="iGroup">Index to the EcoPath/EcoSIm group this is the iGroup</param>
    ''' <param name="theWeight">Weight that is applied to this group 0-1</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal iGroup As Integer, ByVal theWeight As Single)

        iGroupIndex = iGroup
        'weight does not have to one or zero it can be any value it 
        Weight = theWeight

    End Sub

    Public Sub New()
        iGroupIndex = 0
        Weight = 0
    End Sub

    Public Overrides Function ToString() As String
        Return "Group Index=" & iGroupIndex.ToString & " Weight=" & Weight.ToString
    End Function

End Class
