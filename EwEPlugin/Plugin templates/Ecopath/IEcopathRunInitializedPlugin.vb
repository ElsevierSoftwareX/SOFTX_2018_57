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

''' ---------------------------------------------------------------------------
''' <summary>
''' Plugin point called at the start of an Ecopath run.
''' After all the data has been loaded but before Ecopath has started to compute 
''' the missing parameters. 
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcopathRunInitializedPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plugin point called at the start of an Ecopath run.
    ''' After all the data has been loaded but before Ecopath has started to compute the missing parameters. 
    ''' </summary>
    ''' <param name="EcopathDataAsObject">cEcopathDataStructures as an object.</param>
    ''' <param name="TaxonDataAsObject">cTanonDataStructures as an object.</param>
    ''' <param name="StanzaDataAsObject">cStanzaDataStructures as an object.</param>
    ''' -----------------------------------------------------------------------
    Sub EcopathRunInitialized(ByVal EcopathDataAsObject As Object, _
                              ByVal TaxonDataAsObject As Object, _
                              ByVal StanzaDataAsObject As Object)

End Interface
