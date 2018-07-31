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

#End Region ' Imports

Public Class cLinkFactory

    Public Enum eLinkType As Integer
        Unknown = 0
        ProducerToProcessing
        ProcessingToDistribution
        DistributionToWholeseller
        WholesellerToRetailer
        RetailerToConsumer
    End Enum

    Public Shared Function GetLinkType(ByVal src As cUnit, ByVal tgt As cUnit) As eLinkType
        If TypeOf src Is cProducerUnit And TypeOf tgt Is cProcessingUnit Then Return eLinkType.ProducerToProcessing
        If TypeOf src Is cProcessingUnit And TypeOf tgt Is cDistributionUnit Then Return eLinkType.ProcessingToDistribution
        If TypeOf src Is cDistributionUnit And TypeOf tgt Is cWholesalerUnit Then Return eLinkType.DistributionToWholeseller
        If TypeOf src Is cWholesalerUnit And TypeOf tgt Is cRetailerUnit Then Return eLinkType.WholesellerToRetailer
        If TypeOf src Is cRetailerUnit And TypeOf tgt Is cConsumerUnit Then Return eLinkType.RetailerToConsumer
        Return eLinkType.Unknown
    End Function

    Public Shared Function CanCreateLink(ByVal src As cUnit, ByVal tgt As cUnit) As Boolean
        ' Cannot link to producers
        If TypeOf (tgt) Is cProducerUnit Then Return False
        ' Cannot link from consumers
        If TypeOf (src) Is cConsumerUnit Then Return False
        ' For now all else is fine
        Return True
    End Function

    Public Shared Function CreateLinkDefault(ByVal linkType As eLinkType) As cLinkDefault
        Dim link As New cLinkDefault()
        link.LinkType = linkType
        Return link
    End Function

End Class
