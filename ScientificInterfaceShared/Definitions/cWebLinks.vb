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
Imports System.Reflection
Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' All web links
''' </summary>
''' <remarks>
''' ToDo: provide this content via a web service
''' </remarks>
Public Class cWebLinks

    Private Const g_Start As String = "http://www.ecopath.org/ewestart"
    Private Const g_Home As String = "http://www.ecopath.org"
    Private Const g_UsersRSS As String = "https://groups.google.com/forum/feed/eweusers/msgs/rss.xml?num=15"
    Private Const g_Trac As String = "http://sources.ecopath.org/trac/Ecopath/report/1"
    Private Const g_Course As String = "http://www.ecopath.org/courses"
    Private Const g_Facebook As String = "http://www.facebook.com/eweconsortium"
    Private Const g_BetaFeedback As String = "http://www.surveymonkey.com/s/5XD6HKC"
    Private Const g_EcoBaseModelInfo As String = "http://ecobase.ecopath.org/index.php?ident=base_eco&pass=base_eco&provenance=ecopath&action=base&menu=0&model={0}"
    Private Const g_Access2010 As String = "https://www.microsoft.com/en-us/download/details.aspx?id=13255"

    Private m_core As cCore = Nothing

    Public Sub New(core As cCore)
        Me.m_core = core
    End Sub

    Public Enum eLinkType As Integer
        NotSet = 0
        Start
        Home
        UsersRSS
        Trac
        Facebook
        Courses
        Feedback
        EcoBaseModelInfo
        Access2010
    End Enum

    Public Function GetURL(type As eLinkType) As String

        Select Case type
            Case eLinkType.Start : Return Me.EwEHomeURL()
            Case eLinkType.Home : Return cWebLinks.g_Home
            Case eLinkType.UsersRSS : Return cWebLinks.g_UsersRSS
            Case eLinkType.Trac : Return cWebLinks.g_Trac
            Case eLinkType.Courses : Return cWebLinks.g_Course
            Case eLinkType.Facebook : Return cWebLinks.g_Facebook
            Case eLinkType.Feedback : Return cWebLinks.g_BetaFeedback
            Case eLinkType.EcoBaseModelInfo : Return cWebLinks.g_EcoBaseModelInfo
            Case eLinkType.Access2010 : Return cWebLinks.g_Access2010
        End Select
        Return ""

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Conjure the EwE base URL for invoking the EwE start page, including
    ''' version check.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function EwEHomeURL() As String

        Dim pm As cPluginManager = Me.m_core.PluginManager
        Dim aAssemblyNames As AssemblyName() = cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.EwECore)
        Dim ub As New cUriBuilder(g_Start)

        For Each an As AssemblyName In aAssemblyNames
            ' Keep ewe component list really short; it's the plug-ins we're interested in
            If ((String.Compare(an.Name, "ewecore", True) = 0) Or (String.Compare(an.Name, "ewe6", True) = 0)) Then
                If Not ub.QueryString.ContainsKey(an.Name) Then ub.QueryString(an.Name) = an.Version.ToString
            End If
        Next an

        If (pm IsNot Nothing) Then
            aAssemblyNames = pm.PluginAssemblyNames
            For Each an As AssemblyName In aAssemblyNames
                If Not ub.QueryString.ContainsKey(an.Name) Then ub.QueryString(an.Name) = an.Version.ToString
            Next an
        End If

        Return ub.ToString()

    End Function

End Class
