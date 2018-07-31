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
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Forms
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Form class showing a browser window and a mini-toolbar for navigation.
''' </summary>
''' ===========================================================================
Public Class frmStartPanel

    Private m_strURL As String = ""

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="uic">UI context to link to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext)
        MyBase.New()
        Me.InitializeComponent()
        Me.UIContext = uic
        Me.TabText = Me.Text
    End Sub

#End Region ' Constructor

#Region " Public acess "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the URL that the web browser is currently displaying.
    ''' </summary>
    ''' <remarks>
    ''' If left emtpy, the browser will navigate to the EwE start page.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property URL() As String
        Get
             Return Me.m_strURL
        End Get
        Set(ByVal strURL As String)
            Try
                If String.IsNullOrWhiteSpace(strURL) Then
                    Dim link As New cWebLinks(Me.UIContext.Core)
                    strURL = link.GetURL(cWebLinks.eLinkType.Start)
                End If

                If (strURL <> Me.m_strURL) Then
                    Me.m_strURL = strURL
                    Me.m_browser.Navigate(strURL)
                End If
            Catch ex As Exception
                cLog.Write(ex)
            End Try
        End Set
    End Property

#End Region ' Public acess

#Region " Form overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

#If BETA = 1 Then
        Me.m_tsbnBetaFeedback.Visible = True
#Else
        Me.m_tsbnBetaFeedback.Visible = False
#End If
        Me.m_tsbnStartPage.Image = SharedResources.HomeHS
        Me.m_tsbnBack.Image = SharedResources.Back
        Me.m_tsbnForward.Image = SharedResources.forward
        Me.m_tsbnRefresh.Image = SharedResources.Refresh
        Me.m_tsbnEcopathSite.Image = SharedResources.Ecopath_32x32
        Me.m_tsbnBugTracker.Image = SharedResources.bug
        Me.m_tsbnBetaFeedback.Image = My.Resources.logo_sm

        AddHandler Me.m_browser.CanGoBackChanged, AddressOf OnUpdateNav
        AddHandler Me.m_browser.CanGoForwardChanged, AddressOf OnUpdateNav

        Me.Icon = Icon.FromHandle(ScientificInterfaceShared.My.Resources.HomeHS.GetHicon)

        ' Navigate to current URL
        Me.URL = Me.URL
        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        Me.Icon.Dispose()

        RemoveHandler Me.m_browser.CanGoBackChanged, AddressOf OnUpdateNav
        RemoveHandler Me.m_browser.CanGoForwardChanged, AddressOf OnUpdateNav

        MyBase.OnFormClosed(e)

    End Sub

    Public Overrides Function PanelType() As frmEwEDockContent.ePanelType
        Return ePanelType.SystemPanel
    End Function

#End Region ' Form overrides

#Region " Events "

    Private Sub OnBrowserNavBack(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbnBack.Click
        Try
            Me.m_browser.GoBack()
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnBrowserNavForward(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbnForward.Click
        Try
            Me.m_browser.GoForward()
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnBrowserRefresh(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbnRefresh.Click
        Try
            Me.m_browser.Refresh()
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnBrowserStart(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbnStartPage.Click
        Try
            Me.Browse(cWebLinks.eLinkType.Start)
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnBrowserFacebook(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnFacebook.Click
        Try
            Me.Browse(cWebLinks.eLinkType.Facebook)
        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub OnUpdateNav(ByVal sender As Object, ByVal e As EventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(New UpdateControlsDelegate(AddressOf UpdateControls))
        Else
            Me.UpdateControls()
        End If
    End Sub

    'Private Sub OnViewRSS(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    '    Handles m_tsbnRSS.Click
    '    Try
    '        Me.Browse(cWebLinks.eLinkType.HomeRSS)
    '    Catch ex As Exception
    '        cLog.Write(ex)
    '    End Try
    'End Sub

    Private Sub OnGoHome(sender As System.Object, e As System.EventArgs) Handles m_tsbnEcopathSite.Click
        Try
            Me.Browse(cWebLinks.eLinkType.Home)
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnVisitTrac(sender As System.Object, e As System.EventArgs) Handles m_tsbnBugTracker.Click
        Try
            Me.Browse(cWebLinks.eLinkType.Trac)
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnVisitSurvey(sender As System.Object, e As System.EventArgs) Handles m_tsbnBetaFeedback.Click
        Try
            Me.Browse(cWebLinks.eLinkType.Feedback)
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

    Private Sub OnBrowserNavigating(sender As Object, e As System.Windows.Forms.WebBrowserNavigatingEventArgs) _
        Handles m_browser.Navigating

        ' Overridden to intercept ewe-ecobase clicks
        Try
            Dim url As String = e.Url.ToString()
            If (url.ToLower().StartsWith("ewe-ecobase")) Then
                e.Cancel = True
                Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
                Dim cmd As cBrowserCommand = CType(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                cmd.Invoke(url)
            End If
        Catch ex As Exception

        End Try

    End Sub

#End Region ' Events

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Delegate for marshalling browser events.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Delegate Sub UpdateControlsDelegate()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update control states in the form
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub UpdateControls()

        Me.m_tsbnBack.Enabled = (Me.m_browser.CanGoBack)
        Me.m_tsbnForward.Enabled = (Me.m_browser.CanGoForward)

    End Sub

    Protected Sub Browse(link As cWebLinks.eLinkType)

        If (Me.UIContext Is Nothing) Then Return

        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)

        If (cmd Is Nothing) Then Return

        cmd.Invoke(link)

    End Sub

#End Region ' Internals

End Class
