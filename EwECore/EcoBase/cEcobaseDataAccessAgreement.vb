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
Imports System.IO
Imports System.Xml.Serialization
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports System.Text
Imports System.Xml
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.SystemUtilities
Imports EwEUtils.NetUtilities

#End Region ' Imports

Namespace WebServices.Ecobase

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class for receiving the EcoBase data access agreement.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEcobaseDataAccessAgreement

        Private m_strAuthorAgreement As String = ""
        Private m_strUserAgreement As String = ""

#Region " Variables "

        Public Property AuthorAgreement As String
            Get
                Return Me.m_strAuthorAgreement
            End Get
            Set(value As String)
                Me.m_strAuthorAgreement = cStringUtils.Unwrap(value)
            End Set
        End Property

        Public Property UserAgreement As String
            Get
                Return Me.m_strUserAgreement
            End Get
            Set(value As String)
                Me.m_strUserAgreement = cStringUtils.Unwrap(value)
            End Set
        End Property

#End Region ' Variables

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Default contructor
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New()
            ' NOP
        End Sub

#End Region ' Construction

#Region " Shared access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Factory method, create a cEcobaseDataAccessAgreement instance from WSDL output.
        ''' </summary>
        ''' <param name="strXML"></param>
        ''' <returns>A cEcobaseDataAccessAgreement instance, or nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FromXML(strXML As String) As cEcobaseDataAccessAgreement

            ' Clean up
            If (String.IsNullOrWhiteSpace(strXML)) Then Return Nothing

            ' Parsing CData is no fun through XML serializers

            Try
                ' Patch up XML
                If Not strXML.StartsWith("<?") Then
                    strXML = "<?xml version=""1.0"" encoding=""utf-8""?><Agreements>" & strXML & "</Agreements>"
                End If

                Dim doc As New XmlDocument()
                doc.LoadXml(strXML)
                Dim selfie As New cEcobaseDataAccessAgreement()

                For Each node As XmlNode In doc.GetElementsByTagName("dissemination_agreement")
                    selfie.AuthorAgreement = node.InnerText
                Next

                For Each node As XmlNode In doc.GetElementsByTagName("agreement")
                   selfie.UserAgreement = node.InnerText
                Next

                Return selfie
            Catch ex As Exception
                ' Hmm
                cLog.Write(ex, "cEcobaseDataAccessAgreement.FromXML")
            End Try

            Return Nothing

        End Function

#End Region ' Shared access

    End Class

End Namespace
