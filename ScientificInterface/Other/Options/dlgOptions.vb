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

' ToDo: integrate option pages provided through plug-ins

#Region " Imports "

Option Strict On
Option Explicit On

Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Dialog; implements the shell for the Options interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class dlgOptions

#Region " Private variables "

        ''' <summary></summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>List of active pages.</summary>
        Private m_lPages As New List(Of IOptionsPage)
        ''' <summary>Current page.</summary>
        Private m_pageCurrent As IOptionsPage = Nothing
        ''' <summary></summary>
        Private m_optStartup As eApplicationOptionTypes = eApplicationOptionTypes.General

        ' ToDo: track changes in pages, and only show prompts after changes occurred. Not very important right now.
        Private m_bHasFiredPrompt As Boolean = False

#End Region ' Private variables

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext, Optional opt As eApplicationOptionTypes = eApplicationOptionTypes.General)

            Me.m_uic = uic
            Me.InitializeComponent()
            Me.m_optStartup = opt

        End Sub

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Create nodes
            Me.m_tvOptions.Nodes.Clear()

            Me.m_tvOptions.Nodes.Add(Me.CreateNode("General", GetType(ucOptionsGeneral)))

            Dim tnDisplay As TreeNode = Me.CreateNode("Display", GetType(ucOptionsStatusColors))
            tnDisplay.Nodes.Add(Me.CreateNode("Colors", GetType(ucOptionsStatusColors)))
            tnDisplay.Nodes.Add(Me.CreateNode("Fonts", GetType(ucOptionsGraphs)))
            tnDisplay.Nodes.Add(Me.CreateNode("Maps", GetType(ucOptionsMap)))
            tnDisplay.Nodes.Add(Me.CreateNode("Pedigree", GetType(ucOptionsPedigree)))
            tnDisplay.Nodes.Add(Me.CreateNode("Main window", GetType(ucOptionsPresentation)))
            Me.m_tvOptions.Nodes.Add(tnDisplay)

            Me.m_tvOptions.Nodes.Add(Me.CreateNode("File management", GetType(ucOptionsFileManagement)))
            Me.m_tvOptions.Nodes.Add(Me.CreateNode("Plug-ins", GetType(ucOptionsPlugins)))
            Me.m_tvOptions.Nodes.Add(Me.CreateNode("External data", GetType(ucOptionsSpatialTemporal)))

            ' Add plug-ins
            Dim pm As cPluginManager = Me.m_uic.Core.PluginManager
            If (pm IsNot Nothing) Then
                ' ToDo: sort
                For Each pi As IPlugin In pm.GetPlugins(GetType(IEwEOptionsPlugin))
                    Dim opt As IEwEOptionsPlugin = DirectCast(pi, IEwEOptionsPlugin)
                    Dim page As Control = opt.GetConfigUI()
                    Debug.Assert(TypeOf page Is IOptionsPage)
                    Me.m_lPages.Add(DirectCast(page, IOptionsPage))
                    Me.m_tvOptions.Nodes.Add(Me.CreateNode(opt.Label, page.GetType()))
                Next
            End If

            Me.m_tvOptions.ExpandAll()

            Me.SelectPage(Me.GetPage(ToPageType(Me.m_optStartup)))

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            ' Bye
            Me.m_scContent.Panel2.Controls.Clear()
            Me.m_pageCurrent = Nothing

            ' Manually dispose
            For Each optionspage As IOptionsPage In Me.m_lPages
                DirectCast(optionspage, Control).Dispose()
            Next
            Me.m_lPages.Clear()

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Constructor

#Region " Event handlers "

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnOk.Click

            Try
                Me.Apply()
            Catch ex As Exception
                cLog.Write(ex, "dlgOptions::OnOK")
            End Try
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnSetDefaults(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_btnSetDefaults.Click

            Try
                Me.SetDefaults()
            Catch ex As Exception
                cLog.Write(ex, "dlgOptions::OnSetDefaults")
            End Try

        End Sub

        Private Sub OnApply(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles m_btnApply.Click

            Try
                Me.Apply()
            Catch ex As Exception
                cLog.Write(ex, "dlgOptions::OnApply")
            End Try

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnCancel.Click

            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()

        End Sub

        Private Sub OnSelectedNode(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) _
            Handles m_tvOptions.AfterSelect

            If (e.Node Is Nothing) Then Return
            ' Suppress auto-select event when dialog initializes (indicated by unknown action), 
            ' because this will switch away from the page that was selected at launch.
            If (e.Action = TreeViewAction.Unknown) Then Return

            Try
                Dim tag As Object = e.Node.Tag
                If (tag IsNot Nothing) Then
                    Dim page As IOptionsPage = Me.GetPage(DirectCast(tag, Type))
                    Me.SelectPage(page)
                End If

            Catch ex As Exception
                cLog.Write(ex, "dlgOptions::OnSelectedNode(" & e.Node.Name & ")")
            End Try

        End Sub

#End Region ' Event handlers

#Region " Internals "

        Private Function CreateNode(strLabel As String, type As Type) As TreeNode
            Return New TreeNode(strLabel) With {.Tag = type}
        End Function

        Private Function CreateNode(strLabel As String, plugin As IEwEOptionsPlugin) As TreeNode
            Return New TreeNode(strLabel) With {.Tag = plugin}
        End Function

        Private Function ToPageType(opt As eApplicationOptionTypes) As Type

            Dim t As Type = GetType(ucOptionsGeneral)

            Select Case opt
                Case eApplicationOptionTypes.General,
                     eApplicationOptionTypes.Messages

                Case eApplicationOptionTypes.PresentationMode
                    t = GetType(ucOptionsPresentation)

                Case eApplicationOptionTypes.Colours
                    t = GetType(ucOptionsStatusColors)

                Case eApplicationOptionTypes.Graphs,
                     eApplicationOptionTypes.Fonts
                    t = GetType(ucOptionsGraphs)

                Case eApplicationOptionTypes.ReferenceMaps
                    t = GetType(ucOptionsMap)

                Case eApplicationOptionTypes.Autosave, eApplicationOptionTypes.FileLocations
                    t = GetType(ucOptionsFileManagement)

                Case eApplicationOptionTypes.Plugins
                    t = GetType(ucOptionsPlugins)

                Case eApplicationOptionTypes.SpatialTemporal
                    t = GetType(ucOptionsSpatialTemporal)

                Case Else
                    Debug.Assert(False, "Option not recognized")
            End Select
            Return t

        End Function

        Private Sub SelectNode(t As Type, nodes As TreeNodeCollection)
            For Each n As TreeNode In nodes
                If ReferenceEquals(n.Tag, t) Then
                    Me.m_tvOptions.SelectedNode = n
                    Return
                Else
                    SelectNode(t, n.Nodes)
                End If
            Next

        End Sub

        Private Function CreatePage(ByVal t As Type) As IOptionsPage

            ' Sanity check
            Debug.Assert(GetType(IOptionsPage).IsAssignableFrom(t))
            Debug.Assert(GetType(Control).IsAssignableFrom(t))

            Dim optionspage As IOptionsPage = Nothing

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core)
            Try
                optionspage = DirectCast(Activator.CreateInstance(t, New Object() {Me.m_uic}), IOptionsPage)
                DirectCast(optionspage, Control).Dock = DockStyle.Fill
                Me.m_lPages.Add(optionspage)
            Catch ex As Exception
                cLog.Write(ex, "dlgOptions::CreatePage " & t.ToString())
            End Try
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            Return optionspage

        End Function

        Private Function GetPage(ByVal t As Type) As IOptionsPage

            ' Sanity check - if this fails something is really wrong
            Debug.Assert(t IsNot Nothing, "Page type not know, cannot continue")

            For Each optionspage As IOptionsPage In Me.m_lPages
                If optionspage.GetType().Equals(t) Then
                    Return optionspage
                End If
            Next
            Return Me.CreatePage(t)

        End Function

        Private Sub SetDefaults()

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core)
            Try
                If Me.m_pageCurrent IsNot Nothing Then
                    Me.m_pageCurrent.SetDefaults()
                End If
            Catch ex As Exception
                cLog.Write(ex, "dlgOptions.SetDefaults(" & Me.m_pageCurrent.GetType().ToString & ")")
            End Try
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

        End Sub

        Private Sub Apply()

            Dim msgs As cMessagePublisher = Me.m_uic.Core.Messages
            Dim msg As cMessage = Nothing
            Dim result As IOptionsPage.eApplyResultType = IOptionsPage.eApplyResultType.Success

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core)
            msgs.SetMessageLock()
            Try
                For Each optionspage As IOptionsPage In Me.m_lPages
                    result = DirectCast(Math.Max(result, optionspage.Apply()), IOptionsPage.eApplyResultType)
                Next
            Catch ex As Exception
                ' Whoah
                cLog.Write(ex, "dlgOptions::Apply")
            End Try
            msgs.RemoveMessageLock()
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            Select Case result
                Case IOptionsPage.eApplyResultType.Success
                    msg = New cMessage(SharedResources.PROMPT_OPTIONS_APPLIED_SUCCESS, eMessageType.Any, eCoreComponentType.External, eMessageImportance.Information)
                    Me.m_bHasFiredPrompt = False
                Case IOptionsPage.eApplyResultType.Success_restart
                    msg = New cMessage(SharedResources.PROMPT_REQUIRES_RESTART, eMessageType.Any, eCoreComponentType.External, eMessageImportance.Warning)
                Case IOptionsPage.eApplyResultType.Success_administrator
                    msg = New cMessage(SharedResources.PROMPT_REQUIRES_ADMINISTRATOR, eMessageType.Any, eCoreComponentType.External, eMessageImportance.Warning)
                Case IOptionsPage.eApplyResultType.Failed
                    msg = New cMessage(SharedResources.PROMPT_OPTIONS_APPLIED_FAILED, eMessageType.Any, eCoreComponentType.External, eMessageImportance.Information)
                    Me.m_bHasFiredPrompt = False
            End Select

            ' Need to send message?
            If (msg IsNot Nothing) And (Me.m_bHasFiredPrompt = False) Then
                ' #Yes: notify user
                Me.m_uic.Core.Messages.SendMessage(msg)
                Me.m_bHasFiredPrompt = True
            End If

        End Sub

        Private Sub SelectPage(ByVal page As IOptionsPage)

            Me.SuspendLayout()

            ' Optimization
            If Object.ReferenceEquals(page, Me.m_pageCurrent) Then Return
            ' Set new page
            Me.m_pageCurrent = page
            ' Yo
            Me.m_scContent.Panel2.Controls.Clear()
            Dim ctrl As Control = DirectCast(Me.m_pageCurrent, Control)
            ctrl.Dock = DockStyle.Fill
            Me.m_scContent.Panel2.Controls.Add(ctrl)

            Me.ResumeLayout()

        End Sub

        Private Sub ExpandNode(ByVal node As TreeNode)
            For Each nodeChild As TreeNode In node.Nodes
                Me.ExpandNode(nodeChild)
            Next
            node.Expand()
        End Sub

#End Region ' Internals

    End Class

End Namespace