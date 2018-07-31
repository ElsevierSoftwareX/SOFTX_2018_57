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

Public Interface IMSEBatch
    Inherits IPlugin

    ''' <summary>
    ''' The MSE Batch Manager has been initialized
    ''' </summary>
    ''' <param name="MSEBatchManager">Instance of cMSEBatchManager as an object.</param>
    ''' <param name="MSEBatchManagerDataStrucures">Instance of cMSEBatchManagerDataStructures as an object.</param>
    ''' <remarks></remarks>
    Sub MSEBatchInitialized(ByVal MSEBatchManager As Object, ByVal MSEBatchManagerDataStrucures As Object)

End Interface
