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
#Region "cLandingsMediatingGroup"

''' <summary>
''' Mediation group for Price Elasticity.
''' cPriceMediatingGroup "Is A" cMediatingGroup with a fleet index that tell you what Fleet to get the Landings from
''' </summary>
''' <remarks></remarks>
Public Class cLandingsMediatingGroup
    Inherits cMediatingGroup

    Public iFleetIndex As Integer

    ''' <summary>
    ''' Build a new Mediation Group
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal iGroup As Integer, ByVal iFleet As Integer, ByVal theWeight As Single)
        MyBase.New(iGroup, theWeight)

        iFleetIndex = iFleet

    End Sub

    Public Sub New()
        MyBase.New()
        iFleetIndex = 0
    End Sub

End Class

#End Region

