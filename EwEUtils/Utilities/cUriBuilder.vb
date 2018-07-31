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

Imports System
Imports System.Collections.Specialized
Imports System.Web
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace Utilities

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="System.UriBuilder">System.UriBuilder</see> extension.
    ''' </summary>
    ''' <remarks>
    ''' Original code by Lotuspro (http://www.codeproject.com/script/profile/whos_who.asp?vt=arts#amp;id=507953)
    ''' obtained from "a useful UriBuilder class", http://www.codeproject.com/aspnet/UrlBuilder.asp
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Class cUriBuilder
        Inherits System.UriBuilder

        Private m_dtQuery As StringDictionary = Nothing

#Region " Constructor overloads "

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal strURI As String)
            MyBase.New(strURI)
            Me.PopulateQueryString()
        End Sub

        Public Sub New(ByVal strURI As Uri)
            MyBase.New(strURI)
            Me.PopulateQueryString()
        End Sub

        Public Sub New(ByVal strSchemeName As String, ByVal strHostName As String)
            MyBase.New(strSchemeName, strHostName)
        End Sub

        Public Sub New(ByVal strScheme As String, ByVal strHost As String, ByVal portNumber As Integer)
            MyBase.New(strScheme, strHost, portNumber)
        End Sub

        Public Sub New(ByVal strScheme As String, ByVal strHost As String, ByVal iPort As Integer, ByVal pathValue As String)
            MyBase.New(strScheme, strHost, iPort, pathValue)
        End Sub

        Public Sub New(ByVal strScheme As String, ByVal strHost As String, ByVal iPort As Integer, ByVal path As String, ByVal extraValue As String)
            MyBase.New(strScheme, strHost, iPort, path, extraValue)
        End Sub

#End Region

#Region " Public methods "

        Public Shadows Function ToString() As String
            Me.GetQueryString()
            Return MyBase.Uri.AbsoluteUri
        End Function

        Public Sub Navigate(Optional ByVal bEndResponse As Boolean = True)
            Dim strURI As String = Me.ToString()
            HttpContext.Current.Response.Redirect(strURI, bEndResponse)
        End Sub

#End Region ' Public methods

#Region " Properties "

        Public ReadOnly Property QueryString() As StringDictionary
            Get
                If m_dtQuery Is Nothing Then
                    m_dtQuery = New StringDictionary()
                End If

                Return m_dtQuery
            End Get
        End Property

        Public Property PageName() As String
            Get
                Dim path As String = MyBase.Path
                Return path.Substring(path.LastIndexOf("/") + 1)
            End Get
            Set(ByVal Value As String)
                Dim path As String = MyBase.Path
                path = path.Substring(0, path.LastIndexOf("/"))
                MyBase.Path = String.Concat(path, "/", Value)
            End Set
        End Property

#End Region ' Properties

#Region " Private methods "

        Private Sub PopulateQueryString()
            Dim strQuery As String = MyBase.Query

            If strQuery Is String.Empty Or strQuery Is Nothing Then
                Return
            End If

            If Me.m_dtQuery Is Nothing Then
                Me.m_dtQuery = New StringDictionary()
            End If

            Me.m_dtQuery.Clear()

            strQuery = strQuery.Substring(1) 'remove the ?

            Dim astrPairs() As String = strQuery.Split(New Char() {"&"c})

            For Each s As String In astrPairs
                Dim astrPair() As String = s.Split(New Char() {"="c})
                Me.m_dtQuery(astrPair(0)) = CStr(If(astrPair.Length > 1, astrPair(1), String.Empty))
            Next
        End Sub

        Private Sub GetQueryString()
            Dim iCount As Integer = m_dtQuery.Count

            If iCount = 0 Then
                MyBase.Query = String.Empty
                Return
            End If

            Dim astrKeys() As String = New String(iCount) {}
            Dim astrValues() As String = New String(iCount) {}
            Dim astrPairs() As String = New String(iCount) {}
            Dim i As Integer

            Me.m_dtQuery.Keys.CopyTo(astrKeys, 0)
            Me.m_dtQuery.Values.CopyTo(astrValues, 0)

            For i = 0 To iCount - 1 Step i + 1
                astrPairs(i) = String.Concat(astrKeys(i), "=", astrValues(i))
            Next

            MyBase.Query = String.Join("&", astrPairs)
        End Sub

#End Region ' Private methods

#Region " Utility "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Test if a string is a valid URI
        ''' </summary>
        ''' <param name="strUri"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://stackoverflow.com/questions/4835269/how-to-check-that-a-uri-string-is-valid
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function IsValidURI(strUri As String) As Boolean

            If (Not Uri.IsWellFormedUriString(strUri, UriKind.Absolute)) Then
                Return False
            End If

            Dim tmp As Uri = Nothing

            If (Not Uri.TryCreate(strUri, UriKind.Absolute, tmp)) Then
                Return False
            End If

            Return tmp.Scheme = Uri.UriSchemeHttp Or tmp.Scheme = Uri.UriSchemeHttps

        End Function

#End Region ' Utility

    End Class

End Namespace ' Utilities
