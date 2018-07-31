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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Extensions
Imports ScientificInterfaceShared.Style
Imports WeifenLuo.WinFormsUI.Docking

#End Region ' Imports

Namespace Forms

    ''' =======================================================================
    ''' <summary>
    ''' <see cref="DockContent">DockContent</see>-derived
    ''' foundation class for EwE forms and panels, extending the Docking library by
    ''' adding a simple AutoHide toggle.
    ''' </summary>
    ''' =======================================================================
    Public Class frmEwEDockContent
        Inherits DockContent

#Region " Private variables "

        Private m_icoOrg As Icon = Nothing
        Private m_icoPulse As Icon = Nothing
        Private m_timerPulse As Timer = Nothing
        Private m_iNumPulses As Integer = 0
        Private m_importancePulse As eMessageImportance = eMessageImportance.Maintenance

#End Region ' Private variables

#Region " Overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="Form.Icon"/>
        ''' <remarks>
        ''' Overridden to update visuals.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shadows Property Icon As Icon
            Get
                Return MyBase.Icon
            End Get
            Set(value As Icon)
                Try
                    MyBase.Icon = value
                    If (Me.Pane IsNot Nothing) Then
                        Me.BeginInvoke(New MethodInvoker(AddressOf Me.Pane.UpdateTabs))
                    End If
                Catch ex As Exception

                End Try
            End Set
        End Property

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.TabText = Me.Text
        End Sub

        Protected Overrides Sub OnClosing(e As System.ComponentModel.CancelEventArgs)
            Me.StopPulsing()
            MyBase.OnClosing(e)
        End Sub

#End Region ' Overrides

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Panel categoy types.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum ePanelType As Byte
            ''' <summary>Panels flagged as Documents for the EwE MDI framework.</summary>
            Document = 0
            ''' <summary>Panels flagged as System Panels for the EwE MDI framework.
            ''' Close All Documents will not close this panels.</summary>
            SystemPanel = 1
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the auto-hide setting for a panel.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property AutoHide() As Boolean
            Get
                Return Me.IsHiding()
            End Get
            Set(ByVal value As Boolean)
                MyBase.DockState = Me.TranslateDockState(MyBase.DockState, value)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the DockState for a document without interrupting the current
        ''' hidden state.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Shadows Property DockState() As DockState
            Get
                Return MyBase.DockState
            End Get
            Set(ByVal value As DockState)
                MyBase.DockState = Me.TranslateDockState(value, Me.IsHiding)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the dock content is currently hiding.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsHiding() As Boolean
            Return (Me.DockState = DockState.DockTopAutoHide) Or _
                   (Me.DockState = DockState.DockBottomAutoHide) Or _
                   (Me.DockState = DockState.DockLeftAutoHide) Or _
                   (Me.DockState = DockState.DockRightAutoHide)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States the type of content that this form displays.
        ''' </summary>
        ''' <returns>Returns <see cref="ePanelType.Document"/> by default.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function PanelType() As ePanelType
            Return ePanelType.Document
        End Function

#End Region ' Public access

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Pulse a given bitmap into the icon area of the dock panel for a given 
        ''' number of times.
        ''' </summary>
        ''' <param name="bmp">The bitmap to pulse. Please use a small (16x16) bitmap ;).</param>
        ''' <param name="iNumPulses">The number of pulses to show.</param>
        ''' -------------------------------------------------------------------
        Protected Sub Pulse(bmp As Bitmap, iNumPulses As Integer)

            Dim bPulse As Boolean = (iNumPulses > 0) And (Me.IsHiding) And (cSystemUtils.IsWindows) And (bmp IsNot Nothing)

            If (Not bPulse) Then
                Me.StopPulsing()
            Else
                If (Me.m_timerPulse Is Nothing) Then

                    Me.m_icoOrg = Me.Icon

                    Me.m_timerPulse = New Timer()
                    Me.m_timerPulse.Interval = 500
                    Me.m_timerPulse.Start()

                    AddHandler Me.m_timerPulse.Tick, AddressOf OnPulseIcon
                End If

                Me.m_iNumPulses = iNumPulses * 2
                Me.UpdatePulseIcon(bmp)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Pulse a given message importance in the icon area of the dock panel
        ''' for a given number of times. If a pulse is already active, the most
        ''' severe message importance state is preserved while pulsing.
        ''' </summary>
        ''' <param name="importance">The <see cref="eMessageImportance">importance</see>
        ''' for which to show the icon.</param>
        ''' <param name="iNumPulses">The number of pulses to show.</param>
        ''' -------------------------------------------------------------------
        Protected Sub Pulse(importance As eMessageImportance, iNumPulses As Integer)

            Dim bPulse As Boolean = (iNumPulses > 0) And (Me.IsHiding) And (cSystemUtils.IsWindows)

            ' Only pulse on relevant messages
            Select Case importance
                Case eMessageImportance.Critical, eMessageImportance.Information, eMessageImportance.Question, eMessageImportance.Warning
                    bPulse = bPulse And True
                Case Else
                    bPulse = False
            End Select

            If (bPulse) Then
                Me.m_importancePulse = CType(Math.Max(Me.m_importancePulse, importance), eMessageImportance)
            Else
                Me.m_importancePulse = 0
            End If

            Me.Pulse(cStyleGuide.GetImage(Me.m_importancePulse), iNumPulses)

        End Sub

#End Region ' Internals

#Region " Privates "

        Private Function TranslateDockState(ByVal state As DockState, ByVal bHide As Boolean) As DockState
            Select Case state
                Case DockState.DockBottom
                    If bHide Then state = DockState.DockBottomAutoHide
                Case DockState.DockBottomAutoHide
                    If bHide Then state = DockState.DockBottom
                Case DockState.DockLeft
                    If bHide Then state = DockState.DockLeftAutoHide
                Case DockState.DockLeftAutoHide
                    If bHide Then state = DockState.DockLeft
                Case DockState.DockRight
                    If bHide Then state = DockState.DockRightAutoHide
                Case DockState.DockRightAutoHide
                    If bHide Then state = DockState.DockRight
                Case DockState.DockTop
                    If bHide Then state = DockState.DockTopAutoHide
                Case DockState.DockTopAutoHide
                    If bHide Then state = DockState.DockTop
            End Select
            Return state
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Pulse timer callback.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnPulseIcon(serder As Object, args As EventArgs)

            Me.m_iNumPulses -= 1
            If Me.m_iNumPulses Mod 2 = 1 Then
                Me.Icon = Me.m_icoPulse
            Else
                Me.Icon = Me.m_icoOrg
            End If

            If Me.m_iNumPulses <= 0 Then
                Me.StopPulsing()
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cancel current pulse plan.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub StopPulsing()

            If (Me.m_icoOrg Is Nothing) Then Return
            If (Me.m_timerPulse Is Nothing) Then Return

            ' Stop timer
            RemoveHandler Me.m_timerPulse.Tick, AddressOf OnPulseIcon
            Me.m_timerPulse.Stop()
            Me.m_timerPulse.Dispose()
            Me.m_timerPulse = Nothing

            ' Restore icon
            Me.Icon = Me.m_icoOrg
            Me.m_icoOrg = Nothing

            ' Dispose current pulsing icon
            Me.m_importancePulse = 0
            Me.UpdatePulseIcon(Nothing)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Manage the current pulse icon and associated GDI+ resources.
        ''' </summary>
        ''' <param name="bmp">The bitmap for which to create a new pulse icon.</param>
        ''' -------------------------------------------------------------------
        Private Sub UpdatePulseIcon(bmp As Bitmap)

            ' Update pulse icon
            If (Me.m_icoPulse IsNot Nothing) Then
                Me.m_icoPulse.Destroy()
                Me.m_icoPulse = Nothing
            End If

            If (bmp IsNot Nothing) Then
                Me.m_icoPulse = Icon.FromHandle(bmp.GetHicon)
            End If

        End Sub

#End Region ' Internals

    End Class

End Namespace
