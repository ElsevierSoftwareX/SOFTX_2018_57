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
Imports EwECore
Imports System.Collections
Imports System.Windows.Forms

#End Region ' Imports

Namespace Controls.Wizard

    ''' =======================================================================
    ''' <summary>
    ''' Base class for implementing a GUI-driven wizard.
    ''' </summary>
    ''' <remarks>
    ''' Note that this class can be severely improved. For one, it does
    ''' not support branches in the logic. Pages be connected in a parent/child
    ''' tree structure, etc. For now I have not bothered.
    ''' </remarks>
    ''' =======================================================================
    <CLSCompliant(True)> _
    Public Class cWizard
        Implements IDisposable

#Region " Private vars "

        ''' <summary>UI Context that a wizard operates on.</summary>
        Private m_uic As cUIContext

        ''' <summary>List of wizard pages.</summary>
        Private m_lPages As New List(Of Type)
        ''' <summary>Index of active page.</summary>
        Private m_iPageActive As Integer = -1
        ''' <summary>The current active page.</summary>
        Private m_page As IWizardPage = Nothing

        ''' <summary>Navigator attached to this wizard.</summary>
        Private m_nav As IWizardNavigation = Nothing
        ''' <summary>Form hosting this wizard.</summary>
        Private m_parent As Form = Nothing
        ''' <summary>Panel where wizard can display its content.</summary>
        Private m_content As Panel = Nothing

#End Region ' Private vars 

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, create a new wizard and embeds itself into your form, muahahaha!
        ''' </summary>
        ''' <param name="uic">UI context to operate on.</param>
        ''' <param name="parent">Form hosting this wizard.</param>
        ''' <param name="content">Panel where wizard can display its content.</param>
        ''' <param name="nav">Navigator attached to this wizard.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal parent As Form, _
                       ByVal content As Panel, _
                       ByVal nav As IWizardNavigation)

            ' Sanity checks
            Debug.Assert(nav IsNot Nothing)

            Me.m_uic = uic

            Me.m_parent = parent
            Me.m_content = content

            Me.m_nav = nav
            Me.m_nav.Attach(Me)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IDisposable.Dispose"/>.
        ''' -------------------------------------------------------------------
        Public Overridable Sub Dispose() Implements IDisposable.Dispose

            ' Close any current pages
            Me.ClosePage()

            If (Me.m_nav IsNot Nothing) Then
                Me.m_nav.Detach()
                Me.m_nav = Nothing
            End If
            GC.SuppressFinalize(Me)
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a page to the wizard.
        ''' </summary>
        ''' <param name="tpage">Type of page class to add to the wizard. Note
        ''' that a page must inherit from <see cref="Control">System.Windows.Forms.Control</see>,
        ''' and must implement the <see cref="IWizardPage">IWizardPage</see>
        ''' interface.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Sub AddPage(ByVal tpage As Type)

            ' Sanity checks
            Debug.Assert(GetType(IWizardPage).IsAssignableFrom(tpage), "Page must implement IWizardPage")
            Debug.Assert(GetType(IWizardPage).IsAssignableFrom(tpage), "Page must be a valid Windows Forms Control")

            ' Add page type to the list of candidate pages
            Me.m_lPages.Add(tpage)
            ' Is this the first page added?
            If (Me.m_iPageActive = -1) Then
                ' #Yes: show this page
                Me.SwitchPage(0)
            Else
                ' #No: just update the navigation
                Me.m_nav.UpdateNavigation()
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for wizard pages to inform the wizard that a pages content
        ''' has changed.
        ''' </summary>
        ''' <param name="page">The page whose content changed.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Sub PageChanged(ByVal page As IWizardPage)
            ' Is this the current active page?
            If (ReferenceEquals(page, Me.m_page)) Then
                ' #Yes: refresh navigation
                Me.m_nav.UpdateNavigation()
                ' Set parent wait cursor
                If page.IsBusy Then
                    Me.m_parent.Cursor = Cursors.WaitCursor
                Else
                    Me.m_parent.Cursor = Cursors.Default
                End If

            End If
        End Sub

#End Region ' Public access

#Region " Context "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the core that this wizard operates on.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Core() As cCore
            Get
                Return Me.m_uic.Core
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the active page in the wizard.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property ActivePage() As IWizardPage
            Get
                Return Me.m_page
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the parent form hosting the wizard.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property Parent() As Form
            Get
                Return Me.m_parent
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the navigator controlling this wizard.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property Navigation() As IWizardNavigation
            Get
                Return Me.m_nav
            End Get
        End Property

#End Region ' Context

#Region " Navigation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Switch to a new wizard page.
        ''' </summary>
        ''' <param name="iPage">Index of the page to switch to.</param>
        ''' -------------------------------------------------------------------
        Protected Sub SwitchPage(ByVal iPage As Integer)

            ' Optimization
            If (iPage = Me.m_iPageActive) Then Return

            Dim page As IWizardPage = Me.ActivePage()
            If (page IsNot Nothing) Then
                If (TypeOf page Is IOptionsPage) Then
                    DirectCast(page, IOptionsPage).Apply()
                End If
            End If

            ' Hold layout while switching
            Me.m_parent.Cursor = Cursors.WaitCursor
            Me.m_content.SuspendLayout()

            ' Truncate page number
            Me.m_iPageActive = Math.Max(0, Math.Min(Me.m_lPages.Count - 1, iPage))
            Me.OpenPage()

            ' Resume rendering
            Me.m_content.ResumeLayout()
            Me.m_parent.Cursor = Cursors.Default

            Me.m_nav.UpdateNavigation()

        End Sub

        Private Function OpenPage() As Boolean

            Dim pageNew As IWizardPage = Nothing
            Dim ctrl As Control = Nothing

            pageNew = DirectCast(Activator.CreateInstance(Me.m_lPages(Me.m_iPageActive)), IWizardPage)

            If (pageNew IsNot Nothing) Then
                pageNew.Init(Me, Me.m_uic)

                Me.ClosePage()

                Me.m_page = pageNew

                ctrl = DirectCast(Me.m_page, Control)
                ctrl.Dock = DockStyle.Fill
                Me.m_content.Controls.Add(ctrl)
                ctrl.Show()

                If (TypeOf ctrl Is IOptionsPage) Then
                    AddHandler DirectCast(ctrl, IOptionsPage).OnChanged, AddressOf OnOptionsChanged
                End If
            End If

            Return True

        End Function

        Private Function ClosePage() As Boolean

            If (Me.m_page IsNot Nothing) Then
                Me.m_page.Close()

                If (TypeOf Me.m_page Is IOptionsPage) Then
                    RemoveHandler DirectCast(Me.m_page, IOptionsPage).OnChanged, AddressOf OnOptionsChanged
                End If

                Try
                    DirectCast(Me.m_page, Control).Dispose()
                Catch ex As Exception
                End Try
                Me.m_content.Controls.Clear()
            End If

        End Function

        Private Sub OnOptionsChanged(sender As Object, args As EventArgs)
            Try
                Me.m_nav.UpdateNavigation()
            Catch ex As Exception

            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to poll whether the wizard is
        ''' allowed to navigate backward.
        ''' </summary>
        ''' <returns>
        ''' True if allowed.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Friend Function CanNavBack() As Boolean

            Dim page As IWizardPage = Me.ActivePage()

            If (page Is Nothing) Then Return False
            If (page.IsBusy) Then Return False
            If (Me.m_iPageActive = 0) Then Return False

            Return page.AllowNavBack

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to navigate backward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Friend Overridable Sub NavigateBack()
            ' Sanity check
            If (Me.CanNavBack = False) Then Return
            ' Navigate back
            Me.SwitchPage(Me.m_iPageActive - 1)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to poll whether the wizard is
        ''' allowed to navigate forward.
        ''' </summary>
        ''' <returns>
        ''' True if allowed.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Friend Function CanNavForward() As Boolean

            Dim page As IWizardPage = Me.ActivePage()

            If (page Is Nothing) Then Return False
            If (page.IsBusy) Then Return False
            If (Me.m_iPageActive >= Me.m_lPages.Count - 1) Then Return False

            Return page.AllowNavForward

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to navigate forward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Friend Overridable Sub NavigateNext()
            ' Sanity check
            If (Me.CanNavForward = False) Then Return
            ' Navigate back
            Me.SwitchPage(Me.m_iPageActive + 1)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to poll whether the wizard is
        ''' allowed to close.
        ''' </summary>
        ''' <returns>
        ''' True if allowed.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Friend Function CanClose() As Boolean

            Dim page As IWizardPage = Me.ActivePage()

            If (page Is Nothing) Then Return False
            Return (page.IsBusy = False)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to poll whether the wizard is
        ''' allowed to finish after all steps completed succesfully.
        ''' </summary>
        ''' <returns>
        ''' True if allowed.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Friend Function CanFinish() As Boolean

            Dim page As IWizardPage = Me.ActivePage()

            If (page Is Nothing) Then Return False
            If (page.IsBusy) Then Return False

            Return (Me.m_iPageActive >= Me.m_lPages.Count - 1)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback for the navigation system to close the wizard.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Friend Overridable Sub Close(ByVal result As DialogResult)
            Me.m_parent.DialogResult = result
            Me.m_parent.Close()
        End Sub

#End Region ' Navigation

    End Class

End Namespace ' Controls.Wizard
