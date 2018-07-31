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
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > file management interface
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsFileManagement
        Implements IOptionsPage
        Implements IUIElement

#Region " Private vars "

        Private m_strVersion As String = Application.ProductVersion.ToString
        Private m_strDocDir As String = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        Private m_cbh As cCheckboxHierarchy = Nothing
        Private m_options As New List(Of ucAutosaveOption)
        Private m_autosaveoptions As cAutoSaveItemEngine = Nothing

#End Region ' Private vars

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext)
            Me.UIContext = uic
            Me.InitializeComponent()

            ' Autosave
            Me.m_autosaveoptions = New cAutoSaveItemEngine(Me.UIContext)
            Me.m_autosaveoptions.Attach(Me.m_plAutoSave)

            ' Output path
            Me.m_fieldpickOutput.UIContext = Me.UIContext
            Me.m_fieldpickOutput.Fields = [Enum].GetValues(GetType(cPathUtility.ePathPlaceholderTypes))
            Me.m_tbOutputMask.Text = My.Settings.OutputPathMask

            ' Backup path masks
            Me.m_fieldpickBackup.UIContext = Me.UIContext
            Me.m_fieldpickBackup.Fields = [Enum].GetValues(GetType(cPathUtility.ePathPlaceholderTypes))
            Me.m_tbBackupMask.Text = My.Settings.BackupFileMask

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
                Me.m_autosaveoptions.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_btnVisitOutputFolder.Image = SharedResources.openOutputHS
            cToolTipShared.GetInstance().SetToolTip(Me.m_btnVisitOutputFolder, SharedResources.TOOLTIP_VIEWFOLDER)

            Me.m_btnVisitBackupFolder.Image = SharedResources.openOutputHS
            cToolTipShared.GetInstance().SetToolTip(Me.m_btnVisitBackupFolder, SharedResources.TOOLTIP_VIEWFOLDER)

            Me.m_cbSaveWithHeader.Checked = My.Settings.AutosaveHeaders
        End Sub

#End Region ' Overrides

#Region " Event handlers "

        Private Sub OnOutputFieldPicked(ByVal sender As ScientificInterfaceShared.Controls.ucFieldPicker, ByVal value As Object) _
            Handles m_fieldpickOutput.OnFieldPicked

            Me.InsertText(Me.m_tbOutputMask, "{" & value.ToString & "}")
            Me.UpdateControls()

        End Sub

        Private Sub OnOutputDirectoryPicked(ByVal sender As ScientificInterfaceShared.Controls.ucFieldPicker, ByVal strDirectory As String) _
            Handles m_fieldpickOutput.OnDirectoryPicked

            Me.m_tbOutputMask.SelectionStart = 0
            Me.m_tbOutputMask.SelectionLength = Math.Max(0, Me.m_tbOutputMask.Text.LastIndexOf("\"c))
            Me.InsertText(Me.m_tbOutputMask, strDirectory)
            Me.UpdateControls()

        End Sub

        Private Sub OnBackupFieldPicked(ByVal sender As ScientificInterfaceShared.Controls.ucFieldPicker, ByVal value As Object) _
            Handles m_fieldpickBackup.OnFieldPicked

            Me.InsertText(Me.m_tbBackupMask, "{" & value.ToString & "}")
            Me.UpdateControls()

        End Sub

        Private Sub OnBackupDirectoryPicked(ByVal sender As ScientificInterfaceShared.Controls.ucFieldPicker, ByVal strDirectory As String) _
            Handles m_fieldpickBackup.OnDirectoryPicked

            Me.m_tbBackupMask.SelectionStart = 0
            Me.m_tbBackupMask.SelectionLength = Math.Max(0, Me.m_tbBackupMask.Text.LastIndexOf("\"c))
            Me.InsertText(Me.m_tbBackupMask, strDirectory)
            Me.UpdateControls()

        End Sub

        Private Sub OnMaskChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tbBackupMask.TextChanged, m_tbOutputMask.TextChanged

            Me.UpdateControls()

        End Sub

        Private Sub OnVisitFolder(sender As System.Object, e As System.EventArgs) _
            Handles m_btnVisitBackupFolder.Click, m_btnVisitOutputFolder.Click

            If (Me.UIContext IsNot Nothing) Then
                Try
                    Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
                    Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                    cmd.Invoke(CStr(DirectCast(sender, Control).Tag))
                Catch ex As Exception
                    cLog.Write(ex, "ucOptionsFileManagement::OnVisitFolder")
                End Try
            End If
        End Sub

#End Region ' Event handlers

#Region " Internals "

        Private Sub UpdateControls()

            Dim core As cCore = Me.UIContext.Core

            Me.UpdateSample(Me.m_tbxOutputSample, Me.m_tbOutputMask.Text, Me.m_btnVisitOutputFolder, False)
            Me.UpdateSample(Me.m_tbxBackupSample, Me.m_tbBackupMask.Text, Me.m_btnVisitBackupFolder, True)

            Me.m_autosaveoptions.SetOutputMask(Me.m_tbOutputMask.Text)

        End Sub

        Private Sub UpdateSample(ByVal tbx As TextBox, ByVal strMask As String, btn As Button, bIsFile As Boolean)

            Dim strSample As String = ""
            Dim strPath As String = ""

            If Not cPathUtility.ResolvePath(strMask, Me.UIContext.Core, strSample) Then
                cPathUtility.ResolvePath(strMask, "{model}", m_strDocDir, ".eweaccdb", m_strVersion, strSample)
            End If

            If (String.IsNullOrWhiteSpace(strSample)) Then Return

            If (bIsFile) Then
                strPath = String.Copy(Path.GetDirectoryName(strSample))
            Else
                strPath = String.Copy(strSample)
            End If
            btn.Enabled = Directory.Exists(strPath)
            btn.Tag = strPath

            tbx.Text = cStringUtils.CompactString(strSample, tbx.ClientRectangle.Width, tbx.Font, TextFormatFlags.PathEllipsis)

        End Sub

        Private Sub InsertText(ByVal tb As TextBox, ByVal strText As String)
            Dim strSrc As String = tb.Text
            Dim strDest As String
            Dim iSelStart As Integer = tb.SelectionStart
            Dim iSelLen As Integer = tb.SelectionLength
            Dim iItemLen As Integer = strText.Length

            If (iSelLen = 0) Then
                strDest = strSrc & strText
                iSelStart = strDest.Length
            Else
                strDest = strSrc.Substring(0, iSelStart) & strText & strSrc.Substring(iSelStart + iSelLen)
                iSelStart += iItemLen
            End If

            tb.Text = strDest
            tb.SelectionStart = iSelStart
            tb.SelectionLength = 0
        End Sub

        Private Sub ReplaceText(ByVal tb As TextBox, ByVal strText As String)
            tb.Text = strText
        End Sub

#End Region ' Internals

#Region " interface implementation "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.OnChanged"/>
        ''' -------------------------------------------------------------------
        Public Event OnOptionsFileManagementChanged(sender As IOptionsPage, args As System.EventArgs) _
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

            If Not Me.CanApply Then Return IOptionsPage.eApplyResultType.Failed

            Try

                My.Settings.BackupFileMask = Me.m_tbBackupMask.Text
                My.Settings.OutputPathMask = Me.m_tbOutputMask.Text
                My.Settings.AutosaveHeaders = Me.m_cbSaveWithHeader.Checked

                Me.m_autosaveoptions.Apply()

            Catch ex As Exception
                cLog.Write(ex, "ucOptionsAutosave::Apply")
                Return IOptionsPage.eApplyResultType.Failed
            End Try

            Return IOptionsPage.eApplyResultType.Success

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.SetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() Implements IOptionsPage.SetDefaults

            Try
                Me.m_tbOutputMask.Text = CStr(My.Settings.GetDefaultValue("OutputPathMask"))
                Me.m_tbBackupMask.Text = CStr(My.Settings.GetDefaultValue("BackupFileMask"))
                Me.m_cbSaveWithHeader.Checked = CBool(My.Settings.GetDefaultValue("AutosaveHeaders"))
            Catch ex As Exception
                cLog.Write(ex, "ucOptionsAutosave::SetDefaults")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return True
        End Function

#End Region

    End Class

End Namespace
