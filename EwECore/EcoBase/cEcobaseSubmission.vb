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
    ''' Base class for containing the data for a single model
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEcobaseSubmission

#Region " Variables "

        <XmlElement("result")> _
        Public Property Result As Integer
        <XmlElement("md5_key")> _
        Public Property Hash As String
        ''' <summary>Ecobase ID.</summary>
        <XmlElement("model_number")> _
        Public Property ModelNumber As String

        Public Enum eSubmisssionResultTypes As Integer
            Pending = 0
            Accepted = 1
            NotInEcobase = 2
        End Enum

        <XmlIgnore()> _
        Public Property ResultType As eSubmisssionResultTypes
            Get
                Return DirectCast(Me.Result, eSubmisssionResultTypes)
            End Get
            Set(value As eSubmisssionResultTypes)
                Me.Result = value
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
        ''' Factory method, create a cEcobaseData instance from WSDL output.
        ''' </summary>
        ''' <param name="strXML"></param>
        ''' <returns>A cEcobaseData instance, or nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FromXML(strXML As String) As cEcobaseSubmission

            ' Clean up
            If (String.IsNullOrWhiteSpace(strXML)) Then Return Nothing

            strXML = strXML.Replace(""" & vbLF && """, "")
            strXML = strXML.Replace("submission", "cEcobaseSubmission")

            Dim reader As New StringReader(strXML)
            Dim serializer As New XmlSerializer(GetType(cEcobaseSubmission))
            Dim selfie As cEcobaseSubmission = Nothing

            Try
                selfie = CType(serializer.Deserialize(reader), cEcobaseSubmission)
            Catch ex As Exception
                ' Hmm
                cLog.Write(ex, "cEcobaseSubmission.FromXML")
            End Try

            Return selfie

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a cEcobaseData instance to a chunk of XML for submission to EcoBase
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToXML(data As cEcobaseModelParameters) As String

            Dim writerText As New cFlexibleEncodingStringWriter()
            Dim writerXML As XmlWriter = XmlWriter.Create(writerText)
            Dim serializer As New XmlSerializer(GetType(cEcobaseModelParameters))
            serializer.Serialize(writerXML, data)
            Return writerText.ToString()

        End Function

#End Region ' Shared access

    End Class

End Namespace
