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
'    UBC Centre for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
'
' Stepwise Fitting Procedure by Sheila Heymans, Erin Scott, Jeroen Steenbeek
' Copyright 2015- Scottish Association for Marine Science, Oban, Scotland
'
' Erin Scott was funded by the Scottish Informatics and Computer Science
' Alliance (SICSA) Postgraduate Industry Internship Programme.
' ===============================================================================
'
#Region " Imports "

Option Strict On
Imports EwEPlugin
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Core
Imports EwECore
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class cSFPPluginPoint
    Implements IUIContextPlugin
    Implements IMenuItemPlugin
    Implements IAutoSavePlugin
    Implements IDisposedPlugin
    Implements IEcosimTimeSeriesPlugin

#Region " Variables "

    Private m_uic As cUIContext = Nothing
    Private m_frm As frmRun = Nothing
    Private m_engine As cSFPManager = Nothing

#End Region ' Variables

#Region " Standard bits "

    Public ReadOnly Property Name As String _
        Implements EwEPlugin.IPlugin.Name
        Get
            Return "StepWiseFittingPlugin"
        End Get
    End Property

    Public ReadOnly Property Author As String _
           Implements EwEPlugin.IPlugin.Author
        Get
            Return "Erin Scott, Sheila Heymans, Jeroen Steenbeek"
        End Get
    End Property

    Public ReadOnly Property Contact As String _
        Implements EwEPlugin.IPlugin.Contact
        Get
            Return "mailto:ewedevteam@gmail.com"
        End Get
    End Property

    Public ReadOnly Property Description As String _
        Implements EwEPlugin.IPlugin.Description
        Get
            Return My.Resources.DESCRIPTION
        End Get
    End Property

    Public Sub Initialize(core As Object) _
        Implements EwEPlugin.IPlugin.Initialize
        ' NOP - wait for UI context instead
    End Sub

#End Region ' Standard bits

#Region " UIContext "

    Public Sub UIContext(uic As Object) _
        Implements EwEPlugin.IUIContextPlugin.UIContext

        Try
            Me.m_uic = DirectCast(uic, cUIContext)
            If (uic IsNot Nothing) Then
                Me.m_engine = New cSFPManager(Me.m_uic.Core, Me.m_uic.FormMain)
            Else
                ' Cleaning up
                If Me.HasUI Then Me.m_frm.Close()
                Me.m_engine = Nothing
            End If
        Catch ex As Exception

        End Try

    End Sub

#End Region ' UIContext

#Region " Menu item "

    Public ReadOnly Property ControlImage As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return ScientificInterfaceShared.My.Resources.Ecosim_32x32
        End Get
    End Property

    Public ReadOnly Property ControlText As String _
        Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return My.Resources.CAPTION
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String _
        Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return My.Resources.DESCRIPTION
        End Get
    End Property

    Public ReadOnly Property EnabledState As eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return eCoreExecutionState.EcosimLoaded
        End Get
    End Property

    Public Sub OnControlClick(sender As Object, e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements EwEPlugin.IGUIPlugin.OnControlClick

        Try
            frmPlugin = Me.GetUI()
        Catch ex As Exception

        End Try

    End Sub

    Public ReadOnly Property MenuItemLocation As String _
        Implements EwEPlugin.IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuTools"
        End Get
    End Property

#End Region ' Menu item

#Region " Time series "

    Public Sub TimeSeriesClosed() _
        Implements EwEPlugin.IEcosimTimeSeriesPlugin.TimeSeriesClosed

        If (Me.HasUI) Then
            Me.m_frm.OnTimeSeriesLoaded(Nothing)
        End If

    End Sub

    Public Sub TimeSeriesLoaded() _
        Implements EwEPlugin.IEcosimTimeSeriesPlugin.TimeSeriesLoaded

        If (Me.HasUI) Then
            Dim core As cCore = Me.m_uic.Core
            Dim tsd As cTimeSeriesDataset = Nothing
            If (core.ActiveTimeSeriesDatasetIndex > 0) Then
                core.TimeSeriesDataset(core.ActiveTimeSeriesDatasetIndex)
            End If
            Me.m_frm.OnTimeSeriesLoaded(tsd)
        End If

    End Sub

#End Region ' Time series

#Region " Disposal "

    Public Sub Dispose() _
        Implements EwEPlugin.IDisposedPlugin.Dispose

        If (Me.HasUI) Then
            Me.m_frm.Close()
            Me.m_frm.Dispose()
        End If

        Me.m_frm = Nothing
        Me.m_engine = Nothing

    End Sub

#End Region ' Disposal

#Region " Autosave "

    Public Property AutoSave As Boolean _
        Implements EwEPlugin.IAutoSavePlugin.AutoSave
        Get
            Return (My.Settings.AutoSaveMode > 0)
        End Get
        Set(value As Boolean)
            If (Me.m_engine Is Nothing) Then Return
            If (value) Then
                If (My.Settings.AutoSaveMode = 0) Then
                    My.Settings.AutoSaveMode = cSFPParameters.eAutosaveMode.Aggregated
                End If
            Else
                My.Settings.AutoSaveMode = cSFPParameters.eAutosaveMode.None
            End If

            If Me.HasUI Then Me.m_frm.UpdateControls()

            My.Settings.Save()
        End Set
    End Property

    Public Function AutoSaveName() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveName
        Return Me.ControlText
    End Function

    Public Function AutoSaveOutputPath() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveOutputPath
        Return System.IO.Path.Combine(Me.m_uic.Core.DefaultOutputPath(Me.AutoSaveType), _
                                      cFileUtils.ToValidFileName(Me.ControlText, False))
    End Function

    Public Function AutoSaveType() As EwEUtils.Core.eAutosaveTypes _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveType
        ' Show underneath Ecosim autosave settings
        Return eAutosaveTypes.Ecosim
    End Function

#End Region ' Autosave

#Region " Internals "

    ''' <summary>
    ''' Create a new run form, or reuses a lvie form if still available.
    ''' </summary>
    Private Function GetUI() As frmRun

        If (Not Me.HasUI) Then
            Me.m_frm = New frmRun(Me.m_uic, Me.m_engine, Me)
        End If
        Return m_frm

    End Function

    Private Function HasUI() As Boolean
        If (Me.m_frm IsNot Nothing) Then
            Return (Me.m_frm.IsDisposed = False)
        End If
        Return False
    End Function

#End Region ' Internals

End Class
