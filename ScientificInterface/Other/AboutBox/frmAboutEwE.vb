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
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.SystemUtilities
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports 

Namespace Other

    ''' =======================================================================
    ''' <summary>
    ''' EwE about box form.
    ''' </summary>
    ''' =======================================================================
    Public Class frmAboutEwE

        Private m_uic As cUIContext = Nothing
        Private m_qehTech As cQuickEditHandler = Nothing
        Private m_bInUpdate As Boolean = False

        Public Sub New(ByVal uic As cUIContext)
            Me.InitializeComponent()
            Me.m_uic = uic
            Me.m_gridTechnical.UIContext = uic
            Me.m_gridDatabase.UIContext = uic
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.m_uic Is Nothing) Then Return

            Me.m_bInUpdate = True

            Dim strTitle As String = My.Resources.GENERIC_CAPTION
            Dim strBitApp As String = If(cSystemUtils.Is64BitProcess, SharedResources.ABOUT_64BIT, SharedResources.ABOUT_32BIT)

            Dim cmd As cCommand = Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME)
            cmd.AddControl(Me.m_rtbAcknowledgements)
            cmd.AddControl(Me.m_rtbDisclaimer)
            cmd.AddControl(Me.m_rtbDistribution)
            cmd.AddControl(Me.m_rtbLicense)

            ' Format generic page
            Me.Text = cStringUtils.Localize(My.Resources.ABOUT_CAPTION, strTitle)
            Me.m_lbTitle.Text = strTitle
            Me.m_lbVersion.Text = cStringUtils.Localize(My.Resources.ABOUT_VERSION, cCore.Version(True), strBitApp)
            Me.m_lbCopyright.Text = cStringUtils.Localize(My.Resources.ABOUT_COPYRIGHT, My.Application.Info.Copyright, My.Application.Info.CompanyName)

            ' Format RTF content pages
            Me.m_rtbTeam.Rtf = StyleRTF(My.Resources.team)
            Me.m_rtbLicense.Rtf = StyleRTF(My.Resources.license)
            Me.m_rtbAcknowledgements.Rtf = StyleRTF(My.Resources.acknowledgements)

            ' Format technical page
            Me.m_lblNetVersion.Text = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DOUBLE, cSysConfig.OSVersion(), cSysConfig.NETVersion())

            Me.m_qehTech = New cQuickEditHandler()
            Me.m_qehTech.Attach(Me.m_gridTechnical, Me.m_uic, Me.m_tsTechnical)

            Me.m_tsbnShowEwEAssembliesOnly.Image = SharedResources.FilterHS
            Me.m_tsbnShowEwEAssembliesOnly.Checked = Me.m_gridTechnical.ShowEwEComponentsOnly

            ' Format database page
            If Not Me.m_uic.Core.StateMonitor.HasEcopathLoaded Then
                Me.m_tcMain.TabPages.Remove(Me.m_tpDatabase)
            End If

            Me.m_bInUpdate = False

        End Sub

        Protected Overrides Sub OnClosed(e As System.EventArgs)

            Dim cmd As cCommand = Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME)
            cmd.RemoveControl(Me.m_rtbAcknowledgements)
            cmd.RemoveControl(Me.m_rtbDisclaimer)
            cmd.RemoveControl(Me.m_rtbDistribution)
            cmd.RemoveControl(Me.m_rtbLicense)

            Me.m_qehTech.Detach()
            MyBase.OnClosed(e)
        End Sub

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnOK.Click
            Me.Close()
        End Sub

        Private Sub OnToggleEwECOmponentView(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnShowEwEAssembliesOnly.Click

            If (Me.m_bInUpdate) Then Return
            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core)
            Try
                Me.m_gridTechnical.ShowEwEComponentsOnly = Me.m_tsbnShowEwEAssembliesOnly.Checked
            Catch ex As Exception
                Debug.Assert(False)
            End Try
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Switch RTF text from 9 pt Arial to local form font and size.
        ''' </summary>
        ''' <param name="strRTF">Text to convert, needing to specify Arial font at 18 pt</param>
        ''' <returns>A transmogrified text.</returns>
        ''' -------------------------------------------------------------------
        Private Function StyleRTF(strRTF As String) As String

            Dim strFont As String = Me.Font.FontFamily.Name
            Dim szFont As Single = Me.Font.Size

            strRTF = strRTF.Replace("Arial;", strFont & ";")
            strRTF = strRTF.Replace("\fs18", "\fs" & CInt(szFont * 2))

            Return strRTF
        End Function

    End Class

End Namespace

