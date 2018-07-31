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
Imports System.Drawing
Imports System
Imports System.Windows.Forms
Imports System.ComponentModel
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core

#End Region ' Imports

Namespace Controls

    Friend Class frmCustomMessageBox
        Inherits Form
        Implements IUIElement

#Region " Private vars "

        ''' <summary>Check box prompt, if anything.</summary>
        Private m_strCheckPrompt As String
        ''' <summary>Message box icon to show.</summary>
        Private m_mbi As MessageBoxIcon
        ''' <summary>Message box buttons to show.</summary>
        Private m_mbb As MessageBoxButtons
        ''' <summary>Contains the result of the message box</summary>
        Private m_result As DialogResult
        ''' <summary>Contains the resulting check box checked state</summary>
        Private m_bChecked As Boolean = False

#End Region ' Private vars

        ''' ===================================================================
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="strPrompt">Message box prompt to show.</param>
        ''' <param name="strCaption">Message box caption to show.</param>
        ''' <param name="mbb">Message box buttons to show.</param>
        ''' <param name="mbi">Message box icon to show.</param>
        ''' <param name="strCheckboxPrompt">A prompt for a checkbox on the
        ''' message box. The checkbox will only be displayed if this string is 
        ''' not empty.</param>
        ''' ===================================================================
        Public Sub New(ByVal strPrompt As String, _
                       ByVal strCaption As String, _
                       ByVal mbb As MessageBoxButtons, _
                       ByVal mbi As MessageBoxIcon, _
                       Optional ByVal strCheckboxPrompt As String = "")

            MyBase.New()
            Me.InitializeComponent()

            ' Remember this
            Me.Text = strCaption
            Me.m_lblPrompt.Text = strPrompt
            Me.m_strCheckPrompt = strCheckboxPrompt
            Me.m_mbi = mbi
            Me.m_mbb = mbb

            ' Config content
            Me.m_lblPrompt.Text = strPrompt

            ' Config icon
            Me.m_pbIcon.Visible = False
            Try
                Dim icon As Icon = cResourceUtils.GetMessageBoxIcon(mbi)
                If icon IsNot Nothing Then
                    Me.m_pbIcon.Image = icon.ToBitmap
                End If
                Me.m_pbIcon.Visible = True
            Catch ex As Exception
            End Try

            ' Config checkbox
            Me.m_chkOption.Visible = False
            If Not String.IsNullOrEmpty(Me.m_strCheckPrompt) Then
                Me.m_chkOption.Text = Me.m_strCheckPrompt
                Me.m_chkOption.Visible = True
            End If

            ' Config buttons
            Me.ConfigureButton(m_btnThree, DialogResult.None)
            Me.ConfigureButton(m_btnTwo, DialogResult.None)
            Me.ConfigureButton(m_btnOne, DialogResult.None)

            Select Case Me.m_mbb

                Case MessageBoxButtons.AbortRetryIgnore
                    Me.ConfigureButton(m_btnOne, DialogResult.Abort)
                    Me.ConfigureButton(m_btnTwo, DialogResult.Retry)
                    Me.ConfigureButton(m_btnThree, DialogResult.Cancel)

                Case MessageBoxButtons.OK
                    Me.ConfigureButton(m_btnOne, DialogResult.OK)

                Case MessageBoxButtons.OKCancel
                    Me.ConfigureButton(m_btnOne, DialogResult.OK)
                    Me.ConfigureButton(m_btnTwo, DialogResult.Cancel)

                Case MessageBoxButtons.RetryCancel
                    Me.ConfigureButton(m_btnOne, DialogResult.Retry)
                    Me.ConfigureButton(m_btnTwo, DialogResult.Cancel)

                Case MessageBoxButtons.YesNo
                    Me.ConfigureButton(m_btnOne, DialogResult.Yes)
                    Me.ConfigureButton(m_btnTwo, DialogResult.No)

                Case MessageBoxButtons.YesNoCancel
                    Me.ConfigureButton(m_btnOne, DialogResult.Yes)
                    Me.ConfigureButton(m_btnTwo, DialogResult.No)
                    Me.ConfigureButton(m_btnThree, DialogResult.Cancel)

            End Select

            ' Perform layout
            Me.PositionControls()

        End Sub

#Region " Properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="DialogResult">result</see> that the dialog was 
        ''' closed with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Result() As DialogResult
            Get
                Return Me.m_result
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the checkbox checked state that the dialog was closed with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property IsChecked() As Boolean
            Get
                Return Me.m_bChecked
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext

#End Region ' Properties 

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            ' > Plink <
            cSoundUtilities.PlaySound(Me.m_mbi)
            ' Let base class do its magic
            MyBase.OnLoad(e)
            ' Center
            Me.CenterToScreen()
        End Sub

#End Region ' Overrides

#Region " Events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, handles any button click.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnThree.Click, m_btnTwo.Click, m_btnOne.Click

            Dim btn As Button = DirectCast(sender, Button)
            ' Store button result
            Me.m_result = DirectCast(btn.Tag, DialogResult)
            ' Store check box checked state
            Me.m_bChecked = Me.m_chkOption.Checked
            ' Done
            Me.Close()

        End Sub

#End Region ' Events

#Region " Internals "

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' Left-to-right organization without ANY flexibility
        ''' </remarks>
        Private Sub PositionControls()

            Dim iFormWidth As Integer = 0
            Dim iFormHeight As Integer = 0
            Dim iButtonsWidth As Integer = 0
            Dim iTmp As Integer = 0

            Dim dx As Integer = Me.Size.Width - Me.ClientSize.Width
            Dim dy As Integer = Me.Size.Height - Me.ClientSize.Height

            Me.AlignIconAndPrompt()

            ' Calc dimensions of picbox + label area
            If (Me.m_pbIcon.Image Is Nothing) Then
                iFormWidth = Me.Spacer * 2 + m_lblPrompt.Width
                iFormHeight = Me.Spacer * 2 + m_lblPrompt.Height
            Else
                iFormWidth = Me.Spacer * 3 + m_lblPrompt.Width + m_pbIcon.Width
                iFormHeight = Me.Spacer * 2 + Math.Max(m_lblPrompt.Height, m_pbIcon.Height)
            End If

            ' Position checkbox
            If (Not String.IsNullOrEmpty(Me.m_strCheckPrompt)) Then
                Me.m_chkOption.Location = New Point(Me.m_lblPrompt.Left, iFormHeight)
                iFormWidth = Math.Max(iFormWidth, Me.m_chkOption.Left + Me.m_chkOption.Width + Me.Spacer)
                iFormHeight += Me.m_chkOption.Height + Me.Spacer
            End If

            ' Consider buttons total width
            iButtonsWidth = Me.Spacer
            If (CInt(m_btnOne.Tag) > DialogResult.None) Then iButtonsWidth += m_btnOne.Width + Me.Spacer
            If (CInt(m_btnTwo.Tag) > DialogResult.None) Then iButtonsWidth += m_btnTwo.Width + Me.Spacer
            If (CInt(m_btnThree.Tag) > DialogResult.None) Then iButtonsWidth += m_btnThree.Width + Me.Spacer

            ' Calc final form width
            iFormWidth = Math.Max(iFormWidth, iButtonsWidth)

            iTmp = Me.Spacer + CInt((iFormWidth - iButtonsWidth) / 2)
            m_btnOne.Location = New Point(iTmp, iFormHeight)
            iTmp += m_btnOne.Width + Me.Spacer
            m_btnTwo.Location = New Point(iTmp, iFormHeight)
            iTmp += m_btnTwo.Width + Me.Spacer
            m_btnThree.Location = New Point(iTmp, iFormHeight)

            ' Resize form
            iFormHeight += m_btnThree.Height + Me.Spacer
            Me.Size = New Size(iFormWidth + dx, iFormHeight + dy)

        End Sub

        Private Sub AlignIconAndPrompt()

            Dim szScreen As Size = SystemInformation.PrimaryMonitorSize()
            Dim szfMax As New SizeF(szScreen.Width / 2.0!, szScreen.Height)
            Dim ftPrompt As Font = Me.m_lblPrompt.Font
            Dim szPrompt As Size = Nothing
            Dim g As Graphics = Me.m_lblPrompt.CreateGraphics()
            Dim iNumChars As Integer = Me.m_lblPrompt.Text.Length
            Dim iNumLines As Integer = 1

            szfMax = g.MeasureString(Me.m_lblPrompt.Text, ftPrompt, szfMax, _
                                     StringFormat.GenericDefault, iNumChars, iNumLines)

            Me.m_lblPrompt.AutoSize = False
            Me.m_lblPrompt.ClientSize = New Size(CInt(Math.Ceiling(szfMax.Width)) + 4, CInt(Math.Ceiling(szfMax.Height)) + 4)

            If (Me.m_pbIcon.Image Is Nothing) Then
                Me.m_lblPrompt.Location = New Point(Me.Spacer, Me.Spacer)
            Else
                If Me.m_lblPrompt.Height < m_pbIcon.Height Then
                    Me.m_lblPrompt.Location = New Point((Me.Spacer * 2) + Me.m_pbIcon.Width, _
                                                        CInt((Me.m_pbIcon.Top + (Me.m_pbIcon.Height / 2)) - (Me.m_lblPrompt.Height / 2)))
                Else
                    Me.m_lblPrompt.Location = New Point((Me.Spacer * 2) + Me.m_pbIcon.Width, Me.Spacer)
                End If
            End If

            g.Dispose()

        End Sub

        Private Sub ConfigureButton(ByVal btn As Button, ByVal result As DialogResult)

            ' Set text
            Select Case result
                Case DialogResult.Abort : btn.Text = My.Resources.BUTTON_ABORT
                Case DialogResult.Cancel : btn.Text = My.Resources.BUTTON_CANCEL
                Case DialogResult.Ignore : btn.Text = My.Resources.BUTTON_IGNORE
                Case DialogResult.No : btn.Text = My.Resources.BUTTON_NO
                Case DialogResult.None : btn.Text = ""
                Case DialogResult.OK : btn.Text = My.Resources.BUTTON_OK
                Case DialogResult.Retry : btn.Text = My.Resources.BUTTON_RETRY
                Case DialogResult.Yes : btn.Text = My.Resources.BUTTON_YES
            End Select

            ' Store tag
            btn.Tag = result
            ' Show/hide button
            btn.Visible = (result <> DialogResult.None)

            Select Case result
                Case DialogResult.Yes, _
                     DialogResult.OK
                    Me.AcceptButton = btn
                Case DialogResult.Cancel
                    Me.CancelButton = btn
            End Select

        End Sub

        Private ReadOnly Property Spacer() As Integer
            Get
                Return Me.Font.Height
            End Get
        End Property

#End Region ' Internals

#Region " Windows Form Designer generated code "

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not (components Is Nothing) Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        Private WithEvents m_btnOne As Button
        Private WithEvents m_btnTwo As Button
        Private WithEvents m_btnThree As Button
        Private WithEvents m_pbIcon As PictureBox
        Private WithEvents m_chkOption As CheckBox
        Private WithEvents m_lblPrompt As ucLinkLabel

        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCustomMessageBox))
            Me.m_btnOne = New Button()
            Me.m_btnTwo = New Button()
            Me.m_btnThree = New Button()
            Me.m_pbIcon = New PictureBox()
            Me.m_chkOption = New CheckBox()
            Me.m_lblPrompt = New ScientificInterfaceShared.Controls.ucLinkLabel()
            CType(Me.m_pbIcon, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_btnOne
            '
            resources.ApplyResources(Me.m_btnOne, "m_btnOne")
            Me.m_btnOne.Name = "m_btnOne"
            '
            'm_btnTwo
            '
            resources.ApplyResources(Me.m_btnTwo, "m_btnTwo")
            Me.m_btnTwo.Name = "m_btnTwo"
            '
            'm_btnThree
            '
            resources.ApplyResources(Me.m_btnThree, "m_btnThree")
            Me.m_btnThree.Name = "m_btnThree"
            '
            'm_pbIcon
            '
            resources.ApplyResources(Me.m_pbIcon, "m_pbIcon")
            Me.m_pbIcon.Name = "m_pbIcon"
            Me.m_pbIcon.TabStop = False
            '
            'm_chkOption
            '
            resources.ApplyResources(Me.m_chkOption, "m_chkOption")
            Me.m_chkOption.Name = "m_chkOption"
            '
            'm_lblPrompt
            '
            resources.ApplyResources(Me.m_lblPrompt, "m_lblPrompt")
            Me.m_lblPrompt.Name = "m_lblPrompt"
            Me.m_lblPrompt.UIContext = Nothing
            '
            'frmCustomMessageBox
            '
            Me.AcceptButton = Me.m_btnOne
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = AutoScaleMode.Dpi
            Me.ControlBox = False
            Me.Controls.Add(Me.m_chkOption)
            Me.Controls.Add(Me.m_pbIcon)
            Me.Controls.Add(Me.m_lblPrompt)
            Me.Controls.Add(Me.m_btnThree)
            Me.Controls.Add(Me.m_btnTwo)
            Me.Controls.Add(Me.m_btnOne)
            Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "frmCustomMessageBox"
            Me.ShowInTaskbar = False
            Me.TopMost = True
            CType(Me.m_pbIcon, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

#End Region

    End Class

End Namespace ' Controls

