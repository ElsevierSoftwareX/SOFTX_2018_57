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

Public Interface IContentFilter
    Event FilterChanged(sender As IContentFilter)
End Interface

Public Interface IGroupFilter
    Inherits IContentFilter
    ''' <summary>
    ''' One-based group index
    ''' </summary>
    Property Group As Integer
End Interface

Public Interface IFleetFilter
    Inherits IContentFilter
    ''' <summary>
    ''' Zero-based fleet index (0 to account for the 'all' fleet)
    ''' </summary>
    Property Fleet As Integer
End Interface

Public Interface IMonthFilter
    Inherits IContentFilter
    ''' <summary>
    ''' One-based month index 
    ''' </summary>
    Property Month As Integer
End Interface