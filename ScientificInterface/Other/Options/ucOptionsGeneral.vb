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

Option Explicit On
Option Strict On

Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Commands

#End Region

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > General settings interface
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsGeneral
        Implements IOptionsPage

        Private m_uic As cUIContext = Nothing
        Private m_fpVerboseLevel As cEwEFormatProvider = Nothing

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext)
            Me.m_uic = uic
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)

            If disposing AndAlso components IsNot Nothing Then
                Me.m_fpVerboseLevel.Release()
                Me.m_fpVerboseLevel = Nothing
                components.Dispose()
            End If
            MyBase.Dispose(disposing)

        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Dim bHasMRU As Boolean = False

            If (My.Settings.MdbRecentlyUsedList IsNot Nothing) Then
                bHasMRU = (My.Settings.MdbRecentlyUsedList.Count <= 1)
            End If

            ' Enable button if there is something to clear
            Me.m_btnClearMRU.Enabled = bHasMRU

            Me.m_nudMRU.Value = CInt(Math.Min(Me.m_nudMRU.Maximum, _
                                     Math.Max(Me.m_nudMRU.Minimum, My.Settings.MdbRecentlyUsedCount)))

            Me.m_nudMaxNumMessages.Value = CInt(Math.Min(Me.m_nudMaxNumMessages.Maximum, _
                                                Math.Max(Me.m_nudMaxNumMessages.Minimum, My.Settings.StatusMaxMessages)))
            Me.m_fpVerboseLevel = New cEwEFormatProvider(Me.m_uic, Me.m_cmbLogLevel, New cVerboseLevelTypeFormatter(), Nothing)
            Me.m_fpVerboseLevel.Value = cLog.VerboseLevel

            Me.m_cbShowSplashScreen.Checked = My.Settings.ShowSplash
            Me.m_cbShowHost.Checked = My.Settings.ShowHostInfo
            Me.m_cbUseExternalBrowser.Checked = My.Settings.UseExternalBrowser

            Me.m_cbStatusShowTime.Checked = My.Settings.StatusShowTime
            Me.m_cbStatusShowNewestFirst.Checked = My.Settings.StatusSortNewestFirst
            Me.m_cbStatusShowVariableValidations.Checked = My.Settings.StatusShowVariableValidations
            Me.m_cbStatusAutoPopup.Checked = My.Settings.StatusAutoPopop
            Me.m_tbxAuthor.Text = My.Settings.Author
            Me.m_tbxContact.Text = My.Settings.Contact

            Me.UpdateControls()

        End Sub

#End Region ' Overrides

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.OnChanged"/>
        ''' -------------------------------------------------------------------
        Public Event OnOptionsGeneralChanged(sender As IOptionsPage, args As System.EventArgs) _
               Implements IOptionsPage.OnChanged

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanApply"/>
        ''' -------------------------------------------------------------------
        Public Function CanApply() As Boolean _
              Implements IOptionsPage.CanApply
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.Apply"/>
        ''' -------------------------------------------------------------------
        Public Function Apply() As IOptionsPage.eApplyResultType _
             Implements IOptionsPage.Apply

            Dim result As IOptionsPage.eApplyResultType = IOptionsPage.eApplyResultType.Success

            Try

                My.Settings.MdbRecentlyUsedCount = CInt(Me.m_nudMRU.Value)
                My.Settings.StatusMaxMessages = CInt(Me.m_nudMaxNumMessages.Value)
                My.Settings.ShowSplash = Me.m_cbShowSplashScreen.Checked
                My.Settings.StatusShowTime = Me.m_cbStatusShowTime.Checked
                My.Settings.StatusSortNewestFirst = Me.m_cbStatusShowNewestFirst.Checked
                My.Settings.StatusShowVariableValidations = Me.m_cbStatusShowVariableValidations.Checked
                My.Settings.StatusAutoPopop = Me.m_cbStatusAutoPopup.Checked
                My.Settings.ShowHostInfo = Me.m_cbShowHost.Checked
                My.Settings.LogVerboseLevel = DirectCast(Me.m_fpVerboseLevel.Value, eVerboseLevel)
                My.Settings.Author = Me.m_tbxAuthor.Text
                My.Settings.Contact = Me.m_tbxContact.Text
                My.Settings.UseExternalBrowser = Me.m_cbUseExternalBrowser.Checked

            Catch ex As Exception
                result = IOptionsPage.eApplyResultType.Failed
            End Try

            Return result

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.SetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() _
            Implements IOptionsPage.SetDefaults

            Try
                Me.m_nudMRU.Value = CInt(My.Settings.GetDefaultValue("MdbRecentlyUsedCount"))
                Me.m_nudMaxNumMessages.Value = CInt(My.Settings.GetDefaultValue("StatusMaxMessages"))
                Me.m_cbShowHost.Checked = CBool(My.Settings.GetDefaultValue("ShowHostInfo"))
                Me.m_cbShowSplashScreen.Checked = CBool(My.Settings.GetDefaultValue("ShowSplash"))
                Me.m_cbStatusShowTime.Checked = CBool(My.Settings.GetDefaultValue("StatusShowTime"))
                Me.m_cbStatusShowNewestFirst.Checked = CBool(My.Settings.GetDefaultValue("StatusSortNewestFirst"))
                Me.m_cbStatusShowVariableValidations.Checked = CBool(My.Settings.GetDefaultValue("StatusShowVariableValidations"))
                Me.m_cbStatusAutoPopup.Checked = CBool(My.Settings.GetDefaultValue("StatusAutoPopup"))
                Me.m_fpVerboseLevel.Value = My.Settings.GetDefaultValue("LogVerboseLevel")
                Me.m_nudMRU.Value = CInt(My.Settings.GetDefaultValue("MdbRecentlyUsedCount"))
                Me.m_cbUseExternalBrowser.Checked = CBool(My.Settings.GetDefaultValue("UseExternalBrowser"))

                Me.m_tbxAuthor.Text = Environment.UserName
                Me.m_tbxContact.Text = ""
            Catch ex As Exception

            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return True
        End Function

#End Region ' Public access

#Region " Event handlers "

        Private Sub OnClearMRU(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnClearMRU.Click

            Me.ClearFileList(My.Settings.MdbRecentlyUsedList)
            Me.m_btnClearMRU.Enabled = False
            Me.UpdateControls()

        End Sub

        Private Sub OnViewLogFileDir(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnViewLogFileDir.Click

            Dim cmd As cBrowserCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            If (cmd Is Nothing) Then Return

            Try
                ' Open directory with the log files
                cmd.Invoke(Path.GetDirectoryName(cLog.LogFile))
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Event handlers

#Region " Internals "

        Private Sub ClearFileList(ByVal fileList As ArrayList)

            If (fileList Is Nothing) Then Return

            Dim fmsg As New cFeedbackMessage(My.Resources.GENERIC_PROMPT_CLEAR_MRU, EwEUtils.Core.eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            Me.m_uic.Core.Messages.SendMessage(fmsg)

            If (fmsg.Reply = eMessageReply.YES) Then
                ' Clear confirmed
                fileList.Clear()
                ' This is a temporary solution to avoid returning null reference.
                fileList.Add(New System.Object)
            End If

        End Sub

        Private Sub UpdateControls()

            Dim bHasMRU As Boolean = (My.Settings.MdbRecentlyUsedList.Count > 0)
            Me.m_btnClearMRU.Enabled = bHasMRU

        End Sub

#End Region ' Internals

    End Class

End Namespace
