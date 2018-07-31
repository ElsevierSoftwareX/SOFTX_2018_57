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

Imports System.Xml

#End Region ' Imports

Namespace Utilities

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' XML helper methods.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cXMLUtils

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strRootElement"></param>
        ''' <param name="xnRoot"></param>
        ''' <param name="strEncoding"></param>
        ''' <returns></returns>
        Public Shared Function NewDoc(ByVal strRootElement As String, _
                                      Optional ByRef xnRoot As XmlNode = Nothing, _
                                      Optional strEncoding As String = "") As XmlDocument
            Dim doc As New XmlDocument()
            Dim xnData As XmlElement = Nothing
            Dim xaData As XmlAttribute = Nothing
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", strEncoding, "yes"))
            xnRoot = doc.CreateElement(strRootElement)
            doc.AppendChild(xnRoot)
            Return doc
        End Function

    End Class

End Namespace
