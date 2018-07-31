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
Imports System.Globalization
Imports System.Web
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace GeoCode

    Public Class cMarineRegionsLookup
        Implements IGeoCodeLookup

        Private Shared s_wsdl As New cMarineRegionsWSDL()
        Private m_ci As CultureInfo = CultureInfo.GetCultureInfo("en-US")

        Public Property Term As String Implements IGeoCodeLookup.Term

        Public Function FindPlaces(strTerm As String) As cGeoCodeLocation() _
            Implements IGeoCodeLookup.FindPlaces

            Dim lLocations As New List(Of cGeoCodeLocation)

            Try
                For Each r As gazetteerRecord In s_wsdl.getGazetteerRecordsByName(strTerm, False, True)
                    Dim x0, x1, y0, y1 As Single
                    If (Single.TryParse(r.minLongitude, NumberStyles.Float, m_ci, x0) And Single.TryParse(r.maxLongitude, NumberStyles.Float, m_ci, x1)) And
                       (Single.TryParse(r.minLatitude, NumberStyles.Float, m_ci, y0) And Single.TryParse(r.maxLatitude, NumberStyles.Float, m_ci, y1)) Then

                        Dim strName As String = ""
                        If String.IsNullOrWhiteSpace(r.placeType) Then
                            strName = r.preferredGazetteerName
                        Else
                            strName = cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, r.preferredGazetteerName, r.placeType)
                        End If
                        Dim loc As New cGeoCodeLocation(strName, x0, y1, x1, y0)
                        lLocations.Add(loc)
                    End If

                Next
            Catch ex As Exception

            End Try

            Return lLocations.ToArray()

        End Function

    End Class

End Namespace
