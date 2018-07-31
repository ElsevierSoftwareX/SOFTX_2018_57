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
Imports System.Collections.Generic
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Interface IResultView

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Show results for all fleets or for an inidividual fleet.
    ''' </summary>
    ''' <param name="iFleet"></param>
    ''' <param name="lUnits"></param>
    ''' <param name="iYear">Year to show.</param>
    ''' <param name="result"></param>
    ''' -----------------------------------------------------------------------
    Sub ShowResults(ByVal iFleet As Integer, _
                    ByVal lUnits As cUnit(), _
                    ByVal result As cResults, _
                    ByVal iYear As Integer)

End Interface
