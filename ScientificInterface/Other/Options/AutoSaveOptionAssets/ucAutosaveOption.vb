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
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control for reflecting an autosave option in the 
    ''' <see cref="ucOptionsFileManagement"/> interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Class ucAutosaveOption
        Implements IUIElement

        Private Const cINDENT_SIZE As Integer = 18

        Private m_uic As cUIContext = Nothing
        Private m_strOutputMask As String = ""
        Private m_autosavetype As eAutosaveTypes = eAutosaveTypes.NotSet
        Private m_iIndent As Integer = 0
        Private m_pi As IAutoSavePlugin = Nothing

        Private m_strPath As String = ""

#Region " Construction / destruction "

        Friend Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an autosave parent item. This item is not associated with
        ''' an <see cref="eAutosaveTypes"/> value.
        ''' </summary>
        ''' <param name="uic">UI Context to connect to the item.</param>
        ''' <param name="strLabel">Label to use for the item.</param>
        ''' <param name="iIndent">Checkbox indentation to use.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal strLabel As String, _
                       ByVal iIndent As Integer)

            MyBase.New()
            Me.InitializeComponent()

            Me.UIContext = uic
            Me.m_autosavetype = eAutosaveTypes.NotSet
            Me.m_iIndent = iIndent
            Me.m_cbOption.Text = strLabel

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an autosave item associated with an <see cref="eAutosaveTypes"/> value.
        ''' </summary>
        ''' <param name="uic">UI Context to connect to the item.</param>
        ''' <param name="autosavetype">The <see cref="eAutosaveTypes"/> value to
        ''' associate the item with.</param>
        ''' <param name="iIndent">Checkbox indentation to use.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal autosavetype As eAutosaveTypes, _
                       ByVal iIndent As Integer)
            Me.New()

            Me.UIContext = uic
            Me.m_autosavetype = autosavetype
            Me.m_iIndent = iIndent

            Dim fmt As New cAutosaveTypeFormatter()
            Me.m_cbOption.Text = fmt.GetDescriptor(Me.m_autosavetype, eDescriptorTypes.Name)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an autosave item associated with an <see cref="eAutosaveTypes"/> value.
        ''' </summary>
        ''' <param name="uic">UI Context to connect to the item.</param>
        ''' <param name="pi"><see cref="IAutoSavePlugin"/> to associate the 
        ''' item with.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal pi As IAutoSavePlugin, _
                       ByVal iIndent As Integer)
            Me.New()

            Me.UIContext = uic
            Me.m_pi = pi
            Me.m_autosavetype = pi.AutoSaveType
            Me.m_cbOption.Text = pi.AutoSaveName
            Me.m_iIndent = iIndent

        End Sub

        Private Property UIContext As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)
                Me.m_uic = value
            End Set
        End Property

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    Me.UIContext = Nothing
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Construction / destruction

#Region " Public interfaces "

        Public Sub SetOutputMask(ByVal strMask As String)
            Me.m_strOutputMask = strMask
            Me.UpdateControls()
        End Sub

        Public Function Checkbox() As CheckBox
            Return Me.m_cbOption
        End Function

        Public Sub Apply()

            Try
                ' Represents a plug-in?
                If (Me.m_pi IsNot Nothing) Then
                    ' #Yes: Update plug-in auto-save state, bypassing the core.
                    '       The plug-in is responsible for remembering this setting.
                    Me.m_pi.AutoSave = (Me.m_cbOption.Checked = True)
                    Me.m_uic.Core.OnSettingsChanged()
                Else
                    ' #No: Only update the core setting when representing a auto-save setting
                    If (Me.m_autosavetype <> eAutosaveTypes.NotSet) Then
                        If (Me.m_cbOption.Checked = True) Then
                            Me.m_uic.Core.Autosave(Me.m_autosavetype) = True
                        Else
                            Me.m_uic.Core.Autosave(Me.m_autosavetype) = False
                        End If
                    End If
                End If
            Catch ex As Exception
                ' Whoah!
                cLog.Write(ex, "ucAutoSaveOption.Apply(" & Me.m_autosavetype & ")")
            End Try

        End Sub

#End Region ' Public interfaces

#Region " Overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            ' Apply indentation
            If cSystemUtils.IsRightToLeft Then
                Me.m_cbOption.Location = New Point(Me.m_cbOption.Location.X - Me.m_iIndent * cINDENT_SIZE, Me.m_cbOption.Location.Y)
            Else
                Me.m_cbOption.Location = New Point(Me.m_cbOption.Location.X + Me.m_iIndent * cINDENT_SIZE, Me.m_cbOption.Location.Y)
            End If
            Me.m_cbOption.Width -= Me.m_iIndent * 20

            ' Set initial state
            If (Me.m_pi IsNot Nothing) Then
                Me.m_cbOption.Checked = Me.m_pi.AutoSave
            ElseIf (Me.m_autosavetype <> eAutosaveTypes.NotSet) Then
                Me.m_cbOption.Checked = (Me.UIContext.Core.Autosave(Me.m_autosavetype) = True)
            End If

            cToolTipShared.GetInstance().SetToolTip(Me.m_btnVisitFolder, SharedResources.TOOLTIP_VIEWFOLDER)

        End Sub

        Protected Overrides Sub OnSizeChanged(e As System.EventArgs)
            MyBase.OnSizeChanged(e)
            Me.UpdateControls()
        End Sub

#End Region ' Overrides

#Region " Events "

        Private Sub OnVisitFolder(sender As System.Object, e As System.EventArgs) _
            Handles m_btnVisitFolder.Click

            If (Me.m_uic IsNot Nothing) Then
                Try
                    Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
                    Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                    cmd.Invoke(Me.m_strPath)
                Catch ex As Exception
                    cLog.Write(ex, "ucAutoSaveOption::OnVisitPath")
                End Try
            End If

        End Sub

#End Region ' Events 

#Region " Internals "

        Private Sub UpdateControls()

            If (Me.m_autosavetype = eAutosaveTypes.NotSet) Then
                Me.m_lblPath.Visible = False
                Me.m_btnVisitFolder.Visible = False
            Else
                Dim strPath As String = ""
                If (Me.m_pi IsNot Nothing) Then
                    strPath = Me.m_pi.AutoSaveOutputPath
                End If
                If (String.IsNullOrWhiteSpace(strPath)) Then
                    strPath = Me.UIContext.Core.DefaultOutputPath(Me.m_autosavetype, Me.m_strOutputMask)
                End If
                Me.m_strPath = strPath

                Me.m_lblPath.Text = cStringUtils.CompactString(strPath, Me.m_lblPath.ClientSize.Width, Me.Font)
                Me.m_lblPath.Visible = True
                Me.m_btnVisitFolder.Visible = True
                Me.m_btnVisitFolder.Enabled = Directory.Exists(Me.m_strPath)
            End If

        End Sub

#End Region ' Internals

    End Class

End Namespace
