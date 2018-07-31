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
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for configuring a WoRMS web service connection.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class ucConfig
    Implements IOptionsPage
    Implements IUIElement

    ''' <summary>Plug-in to configure.</summary>
    Private m_plugin As cWoRMSPluginPoint = Nothing

    Public Sub New(ByVal plugin As cWoRMSPluginPoint)
        MyBase.New()
        Me.m_plugin = plugin
        Me.Text = My.Resources.ENGINE_NAME
        Me.InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        Me.m_nudConnTO.Value = Me.m_plugin.ConnectionTimeOut
        Me.m_nudReplyTO.Value = Me.m_plugin.ResponseTimeOut

        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmd As cBrowserCommand = CType(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
        cmd.AddControl(Me.m_pbBlueBridge, "http://www.i-marine.eu/Content/eLibrary.aspx?id=786ae7dd-f868-4c19-b611-3500b6697bee&li=0")
        cmd.AddControl(Me.m_pbWoRMS, "http://www.marinespecies.org")

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cBrowserCommand = CType(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            cmd.RemoveControl(Me.m_pbBlueBridge)
            cmd.RemoveControl(Me.m_pbWoRMS)
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Public Property UIContext As cUIContext _
        Implements IUIElement.UIContext

    Public Function Apply() As IOptionsPage.eApplyResultType Implements IOptionsPage.Apply
        Me.m_plugin.ConnectionTimeOut = CInt(Me.m_nudConnTO.Value)
        Me.m_plugin.ResponseTimeOut = CInt(Me.m_nudReplyTO.Value)
        Return IOptionsPage.eApplyResultType.Success
    End Function

    Public Function CanApply() As Boolean Implements IOptionsPage.CanApply
        Return True
    End Function

    Public Function CanSetDefaults() As Boolean Implements IOptionsPage.CanSetDefaults
        Return True
    End Function

    Public Event OnChanged(sender As IOptionsPage, args As System.EventArgs) Implements IOptionsPage.OnChanged

    Public Sub SetDefaults() Implements IOptionsPage.SetDefaults
        Me.m_nudConnTO.Value = 60
        Me.m_nudReplyTO.Value = 300
    End Sub

End Class