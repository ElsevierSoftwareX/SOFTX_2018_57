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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports System.Web
Imports ScientificInterfaceShared.BingMapsGeoLocatorService
Imports System.Net

#End Region ' Imports

Namespace GeoCode

    ''' <summary>
    ''' Google Maps based <see cref="IGeoCodeLookup">geo location lookup</see>.
    ''' </summary>
    ''' <remarks>
    ''' Usage example:
    ''' <code>
    ''' Dim lookup As New cGoogleMapsLookup()
    ''' Dim locations as cGeoCodeLocation() = lookup.FindLocations("waddenzee") 
    ''' Dim location as cGeoCodeLocation = Nothing
    ''' 
    ''' If (locations IsNot Nothing) Then
    '''    For each location in locations
    '''       Console.WriteLine("Area for '{0}': {1}x{2} to {3}x{4}", _
    '''                         location.Term, _
    '''                         location.East, location.North, _
    '''                         location.West, location.South)
    '''    Next location
    ''' End If
    ''' </code>
    ''' </remarks>
    Public Class cBingMapsLookup
        Implements IGeoCodeLookup
        Private m_strSearchTerm As String = ""

        <Obsolete("Do not use this class yet; web service geocoding not hooked up yet")> _
        Public Sub New()
            Throw New NotImplementedException("Do not use this class yet; web service geocoding not hooked up yet")
        End Sub

        Public Property Term As String Implements IGeoCodeLookup.Term
            Get
                Return Me.m_strSearchTerm
            End Get
            Set(value As String)
                Me.m_strSearchTerm = value
            End Set
        End Property

        Public Function FindPlaces(ByVal strTerm As String) As cGeoCodeLocation() _
            Implements IGeoCodeLookup.FindPlaces

            Me.Term = strTerm

            Dim strKey As String = "Ap_lhJ94cQGn56JsiXaxffd5O3HnAY6ug7BaaZap7zjBC-CBGjOnGrtyAK1442sr"
            Dim strRequest As String = String.Format("http://dev.virtualearth.net/REST/v1/Locations/{0}?output=xml&key={1}", _
                                                     strTerm, strKey)
            Dim request As WebRequest = WebRequest.Create(strRequest)
            Dim response As WebResponse = request.GetResponse()
            Dim lLocations As New List(Of cGeoCodeLocation)

            ' http://msdn.microsoft.com/en-us/library/hh534080.aspx

            '    Dim xmlDoc As New XmlDocument()
            '    xmlDoc.Load(response.GetResponseStream())
            'return (xmlDoc);

            '' Make the search request 
            'Dim searchService As New SearchServiceClient()
            'Dim searchResponse As SearchResponse = searchService.Search(SearchRequest)

            '' Parse and format results
            'If (searchResponse.ResultSets(0).Results.Length > 0) Then
            '    For i As Integer = 0 To searchResponse.ResultSets(0).Results.Length - 1
            '        'resultList.Append(String.Format("{0}. {1}\n", i+1, 
            '        '    searchResponse.ResultSets[0].Results[i].Name));                    
            '    Next
            'End If

            Return lLocations.ToArray

        End Function

    End Class

End Namespace

