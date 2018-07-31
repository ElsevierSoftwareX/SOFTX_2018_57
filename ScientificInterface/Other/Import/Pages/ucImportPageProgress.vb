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
Imports System.Threading
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.Wizard

#End Region ' Imports

Namespace Import

    ''' =======================================================================
    ''' <summary>
    ''' Import wizard progress page.
    ''' </summary>
    ''' =======================================================================
    Public Class ucImportPageProgress
        Implements IWizardPage

#Region " Private vars "

        ''' <summary>The wizard to operate on.</summary>
        Private m_wizard As cImportWizard = Nothing
        ''' <summary>Core message handler to intercept progress messages.</summary>
        Private m_mh As cMessageHandler = Nothing
        ''' <summary>Sync object to deal with progress messages.</summary>
        Private m_syncobj As SynchronizationContext = Nothing
        ''' <summary>Importing state flag.</summary>
        Private m_bImporting As Boolean = False
        ''' <summary>UI context</summary>
        Private m_uic As cUIContext = Nothing
        Private m_threadImport As Thread = Nothing

#End Region ' Private vars

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#Region " IWizardPage implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the import progress page by subscribing to the core
        ''' import messages and immediately starting the import process.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Init(ByVal wizard As cWizard, ByVal uic As cUIContext) _
               Implements IWizardPage.Init

            ' Sanity checks
            Debug.Assert(TypeOf (wizard) Is cImportWizard)

            Me.m_wizard = DirectCast(wizard, cImportWizard)
            Me.m_uic = uic

            Me.m_pb.Minimum = 0
            Me.m_pb.Maximum = 100

            ' Make sure we have a sync object
            Me.m_syncobj = SynchronizationContext.Current
            ' If there is no sync object then create a new one on this thread
            If (Me.m_syncobj Is Nothing) Then Me.m_syncobj = New System.Threading.SynchronizationContext()

            ' Subscribe to core import messages
            Me.m_mh = New cMessageHandler(AddressOf Me.ProgressMessageHandler, eCoreComponentType.External, eMessageType.DataImport, Me.m_syncobj)
#If DEBUG Then
            Me.m_mh.Name = "ucImportPageProcess"
#End If
            Me.m_wizard.Core.Messages.AddMessageHandler(Me.m_mh)

            ' Start the import process
            Me.StartImport()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close the page by detaching from core messages.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Close() _
            Implements IWizardPage.Close

            Me.m_wizard.Core.Messages.RemoveMessageHandler(Me.m_mh)
            Me.m_mh = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the import progress page allows the wizard to 
        ''' navigate backward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function AllowNavBack() As Boolean _
              Implements IWizardPage.AllowNavBack
            ' No restrictions
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the import progress page allows the wizard to 
        ''' navigate forward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function AllowNavForward() As Boolean _
            Implements IWizardPage.AllowNavForward
            ' No restrictions
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the import progress page is busy.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsBusy() As Boolean _
             Implements IWizardPage.IsBusy
            ' This page is busy as long as an import is in progress.
            Return (Me.m_bImporting = True)
        End Function

#End Region ' IWizardPage implementation

#Region " Events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Core messages handler to process import messages.
        ''' </summary>
        ''' <param name="msg">An arriving message.</param>
        ''' -------------------------------------------------------------------
        Private Sub ProgressMessageHandler(ByRef msg As cMessage)

            ' Discard any message that is not a progress message
            If (Not TypeOf (msg) Is cProgressMessage) Then Return

            Dim pmsg As cProgressMessage = DirectCast(msg, cProgressMessage)

            ' Update progress bar
            If Me.InvokeRequired Then
                Me.Invoke(New UpdateProgressDelegate(AddressOf UpdateProgress), New Object() {CInt(100 * pmsg.Progress), pmsg.Message})
            Else
                Me.UpdateProgress(CInt(100 * pmsg.Progress), pmsg.Message)
            End If
        End Sub

        Private Delegate Sub UpdateProgressDelegate(ByVal iProgress As Integer, ByVal strMsg As String)

        Private Sub UpdateProgress(ByVal iProgress As Integer, ByVal strMsg As String)

            ' Update progress bar
            Me.m_pb.Value = Math.Max(0, Math.Min(100, iProgress))
            ' Update progress listbox
            Me.m_lbSummary.Items.Add(cStringUtils.vbTab & strMsg)
            Me.m_lbSummary.TopIndex = Me.m_lbSummary.Items.Count - 1

        End Sub

#End Region ' Events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the page is importing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property IsImporting() As Boolean
            Get
                Return Me.m_bImporting
            End Get
            Set(ByVal value As Boolean)
                ' Optimization
                If (Me.m_bImporting <> value) Then
                    ' Update importing status
                    Me.m_bImporting = value
                    ' Notify parent wizard that the page state has changed
                    Me.m_wizard.PageChanged(Me)
                End If
            End Set
        End Property

#Region " Threaded import "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Start the import of user selected models in a separate thread in
        ''' order not to block the EwE6 UI.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub StartImport()

            Me.m_threadImport = New Thread(AddressOf PerformImportThread)
            Me.m_threadImport.Start()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The actual thread procedure that performs the import process.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub PerformImportThread()

            ' Flag m'self as busy
            Me.SetImportStatus(True, "")

            ' For every importer model setting
            For Each setting As cImportWizard.cImportSettings In Me.m_wizard.ImportSettings
                ' Does the user want to import this model?
                If (setting.SelectedForImport = True) Then
                    ' #Yes: perform import of this model

                    ' Set global application status
                    cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, cStringUtils.Localize(My.Resources.STATUS_MODEL_IMPORTING, setting.EwE6ModelName))

                    ' Set local import status
                    Me.SetImportStatus(True, cStringUtils.Localize(My.Resources.STATUS_IMPORTING_MODEL, setting.ModelInfo.Name))

                    ' Actual import successful?
                    If Me.m_wizard.Import(setting) Then
                        ' #Yes: report succes
                        Me.SetImportStatus(True, cStringUtils.Localize(My.Resources.STATUS_IMPORTING_MODEL_SUCCESS, setting.ModelInfo.Name, setting.EwE6ModelName))
                    Else
                        ' #No: report failure
                        If String.IsNullOrEmpty(setting.LogFile) Then
                            Me.SetImportStatus(True, cStringUtils.Localize(My.Resources.STATUS_IMPORTING_MODEL_CANCELLED, setting.ModelInfo.Name))
                        Else
                            Me.SetImportStatus(True, cStringUtils.Localize(My.Resources.STATUS_IMPORTING_MODEL_FAILED, setting.ModelInfo.Name, setting.LogFile))
                        End If
                    End If

                    ' Clear application status
                    cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

                End If
            Next setting

            ' Clear import status
            Me.SetImportStatus(False, "")

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Delegate, used to marshall import status information from importing
        ''' thread to UI thread.
        ''' </summary>
        ''' <param name="bImporting">Flag stating whether busy importing.</param>
        ''' <param name="strStatus">Optional message attached to status.</param>
        ''' -------------------------------------------------------------------
        Private Delegate Sub SetImportStatusDelegate(ByVal bImporting As Boolean, ByVal strStatus As String)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Import status update helper
        ''' </summary>
        ''' <param name="bImporting">Flag stating whether busy importing.</param>
        ''' <param name="strStatus">Optional message attached to status.</param>
        ''' -------------------------------------------------------------------
        Private Sub SetImportStatus(ByVal bImporting As Boolean, ByVal strStatus As String)
            Try
                If Me.InvokeRequired Then
                    Me.Invoke(New SetImportStatusDelegate(AddressOf Me.SetImportStatus), New Object() {bImporting, strStatus})
                Else
                    Me.IsImporting = bImporting

                    If Not String.IsNullOrEmpty(strStatus) Then
                        Me.m_lbSummary.Items.Add(strStatus)
                    End If
                End If
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Threaded import

#End Region ' Internals

    End Class

End Namespace
