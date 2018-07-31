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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Option Strict On
Option Explicit On

#Region " Imports "

Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class cMSEPluginPoint
    Implements EwEPlugin.IMenuItemPlugin
    Implements EwEPlugin.ICorePlugin
    Implements EwEPlugin.IUIContextPlugin
    Implements EwEPlugin.IEcosimInitializedPlugin
    Implements EwEPlugin.IEcosimBeginTimestepPlugin
    Implements EwEPlugin.IEcosimEndTimestepPlugin
    Implements EwEPlugin.IMessageFilterPlugin
    Implements EwEPlugin.IEcopathPlugin
    Implements EwEPlugin.IEcosimPlugin
    Implements EwEPlugin.IEcopathRunInitializedPlugin
    Implements EwEPlugin.IMSEInitialized
    Implements EwEPlugin.IEcosimDataInitializedPlugin
    Implements EwEPlugin.IDisposedPlugin

#Region " Internal vars "

    Private m_MSE As cMSE

    Private m_frm As frmMSE = Nothing
    Private m_core As cCore = Nothing
    Private m_uic As cUIContext = Nothing
    Private m_ecosim As EwECore.Ecosim.cEcoSimModel = Nothing
    Private m_ecopath As Ecopath.cEcoPathModel
    Private m_simdata As cEcosimDatastructures
    Private m_pathdata As cEcopathDataStructures
    Private m_coreMSEData As EwECore.MSE.cMSEDataStructures

    Private m_EcosimTimeStepDelegate As EwECore.Ecosim.EcoSimTimeStepDelegate

    Private m_monitor As New cMSEStateMonitor(Me)

    Private m_mhEcopath As cMessageHandler = Nothing
    Private m_mhEcosim As cMessageHandler = Nothing
    Private m_mhSettings As cMessageHandler = Nothing

    ''' <summary>
    ''' Flag, stating that an MSE session has been explicitly activated by the user.
    ''' </summary>
    ''' <remarks>
    ''' This prevents needless messages and processing if the MSE plug-in is loaded, 
    ''' but has not been activated by the user.
    ''' </remarks>
    Private m_bSessionActive As Boolean = True

#End Region ' Internal vars

#Region " Public Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="cMSEStateMonitor">MSE state monitor</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Monitor As cMSEStateMonitor
        Get
            Return Me.m_monitor
        End Get
    End Property

    Public ReadOnly Property MSE As cMSE
        Get
            Return Me.m_MSE
        End Get
    End Property

    Public Sub UIContext(ByVal uic As Object) Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

    Friend ReadOnly Property Core As cCore
        Get
            Return Me.m_core
        End Get
    End Property

#End Region ' Public Properties

#Region " Construction "

    Public Sub New()
        Me.m_MSE = New cMSE(m_monitor, Me)
    End Sub

#End Region ' Construction

#Region " Diagnostics and state management "

    Friend Sub InvalidateConfiguration()

        Me.MSE.InvalidateConfigurationState()
        Me.m_monitor.Invalidate()

    End Sub

#End Region ' Diagnostics and state management

#Region " EwE app flow plugins "

    Public Function CloseModel() As Boolean Implements EwEPlugin.IEcopathPlugin.CloseModel
        ' NOP
        Return True
    End Function

    Public Function LoadModel(dataSource As Object) As Boolean Implements EwEPlugin.IEcopathPlugin.LoadModel

        If (Not Me.HasUI) Then Return True
        Me.InvalidateConfiguration()
        Return True

    End Function

    Public Function SaveModel(dataSource As Object) As Boolean Implements EwEPlugin.IEcopathPlugin.SaveModel
        Return True
    End Function

    Public Sub CloseEcosimScenario() Implements EwEPlugin.IEcosimPlugin.CloseEcosimScenario
        ' NOP
    End Sub

    Public Sub LoadEcosimScenario(dataSource As Object) Implements EwEPlugin.IEcosimPlugin.LoadEcosimScenario

        If (Not Me.HasUI) Then Return
        Me.InvalidateConfiguration()

    End Sub

    Public Sub SaveEcosimScenario(dataSource As Object) Implements EwEPlugin.IEcosimPlugin.SaveEcosimScenario
        ' NOP
    End Sub

    Public Sub onInitialize(ByVal core As Object) Implements EwEPlugin.IPlugin.Initialize
        Me.m_core = CType(core, cCore)
        Units.Init(m_core)
    End Sub

    Public Sub onCoreInitialized(ByRef objEcoPath As Object, ByRef objEcoSim As Object, ByRef objEcoSpace As Object) _
        Implements EwEPlugin.ICorePlugin.CoreInitialized

        Me.m_ecopath = CType(objEcoPath, Ecopath.cEcoPathModel)
        Me.m_ecosim = CType(objEcoSim, Ecosim.cEcoSimModel)

        Debug.Assert(Me.m_uic IsNot Nothing)

        Me.MSE.onCoreInitialized(Me.m_core, m_ecopath, m_ecosim)

        ' Set message handlers
        Me.m_mhEcopath = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoPath, eMessageType.Any, Me.m_uic.SyncObject)
        Me.m_mhEcosim = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoSim, eMessageType.DataAddedOrRemoved, Me.m_uic.SyncObject)
        Me.m_mhSettings = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.Core, eMessageType.GlobalSettingsChanged, Me.m_uic.SyncObject)

#If DEBUG Then
        Me.m_mhEcopath.Name = "CefasMSE_mhEcopath"
        Me.m_mhEcosim.Name = "CefasMSE_mhEcosim"
        Me.m_mhSettings.Name = "CefasMSE_mhSettings"
#End If

        Me.Core.Messages.AddMessageHandler(Me.m_mhEcopath)
        Me.Core.Messages.AddMessageHandler(Me.m_mhEcosim)
        Me.Core.Messages.AddMessageHandler(Me.m_mhSettings)

    End Sub

    Public Sub Dispose() _
        Implements EwEPlugin.IDisposedPlugin.Dispose

        Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcopath)
        Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcosim)
        Me.Core.Messages.RemoveMessageHandler(Me.m_mhSettings)

        ' Clean up the mess of OnCoreInitialized
        Me.m_mhEcopath.Dispose()
        Me.m_mhEcosim.Dispose()
        Me.m_mhSettings.Dispose()


    End Sub


    Public Sub EcopathRunInitialized(EcopathDataAsObject As Object, TaxonDataAsObject As Object, StanzaDataAsObject As Object) _
                Implements EwEPlugin.IEcopathRunInitializedPlugin.EcopathRunInitialized

        Me.m_pathdata = DirectCast(EcopathDataAsObject, cEcopathDataStructures)
        Me.MSE.onEcopathInitialized(Me.m_pathdata)

    End Sub

    Public Sub onEcosimInitialized(ByVal EcosimDatastructures As Object) Implements EwEPlugin.IEcosimInitializedPlugin.EcosimInitialized
        Debug.Assert(TypeOf EcosimDatastructures Is cEcosimDatastructures, "EcosimInitialized() failed to pass in valid Ecosim Data!")

        If TypeOf EcosimDatastructures Is cEcosimDatastructures Then

            Me.m_simdata = DirectCast(EcosimDatastructures, cEcosimDatastructures)
            Me.MSE.onEcosimInitialized(Me.m_simdata)

        End If

    End Sub

    Public Sub MSEInitialized(MSEModel As Object, MSEDataStructure As Object, EcosimDatastructures As Object) _
        Implements EwEPlugin.IMSEInitialized.MSEInitialized

        Try
            Me.m_coreMSEData = DirectCast(MSEDataStructure, MSE.cMSEDataStructures)
            Me.m_MSE.CoreMSEData = Me.m_coreMSEData
        Catch ex As Exception
            cLog.Write(ex, "MSEInitialized(...) Failed to cast MSEDataStructure to cMSEDataStructures.")
        End Try

    End Sub

    Public Sub EcosimPreDataInitialized(EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimDataInitializedPlugin.EcosimPreDataInitialized

    End Sub

    Public Sub EcosimPreRunInitialized(EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimDataInitializedPlugin.EcosimPreRunInitialized

        ' A session is only active IF the user has the MSE interface open when starting Ecosim
        Me.m_bSessionActive = Me.HasUI
        If (Not Me.m_bSessionActive) Then Return

        Try
            Dim data As cEcosimDatastructures = DirectCast(EcosimDatastructures, cEcosimDatastructures)
            Me.m_MSE.onEcosimRunBeginning(data)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub onEcosimBeginTimeStep(ByRef BiomassAtTimestep() As Single, ByVal EcosimDatastructures As Object, ByVal iTime As Integer) _
        Implements EwEPlugin.IEcosimBeginTimestepPlugin.EcosimBeginTimeStep

        If (Not Me.m_bSessionActive) Then Return

        Try
            Me.MSE.onEcosimBeginTimeStep(BiomassAtTimestep, iTime)
        Catch ex As Exception
            If ex.Message = "EwEMSEPlugin.cQuotaShares.ReadiFleetiGroupQuotaShare(). " Then
                Throw ex
            End If
        End Try

    End Sub


    Public Sub EcosimEndTimeStep(ByRef BiomassAtTimestep() As Single, EcosimDatastructures As Object, iTime As Integer, Ecosimresults As Object) _
        Implements EwEPlugin.IEcosimEndTimestepPlugin.EcosimEndTimeStep
        Try
            Me.MSE.onEcosimEndTimeStep(BiomassAtTimestep, iTime)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) Implements EwEPlugin.IGUIPlugin.OnControlClick

        If Not Me.HasUI Then
            Me.InvalidateConfiguration()
            Me.m_frm = New frmMSE(Me, Me.m_uic)
        End If

        ' Let EwE show the form
        frmPlugin = m_frm

    End Sub

#End Region ' EwE app flow plugins

#Region " UI integration "

    Public ReadOnly Property ControlText As String _
        Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return My.Resources.CAPTION
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String _
        Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return My.Resources.CAPTION_TOOLTIP
        End Get
    End Property

    Public ReadOnly Property EnabledState As EwEUtils.Core.eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            'Return EwEUtils.Core.eCoreExecutionState.EcosimCompleted
            Return EwEUtils.Core.eCoreExecutionState.EcosimLoaded
        End Get
    End Property

    Public ReadOnly Property MenuItemLocation() As String _
        Implements EwEPlugin.IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuTools"
        End Get
    End Property

    Public ReadOnly Property Author() As String _
        Implements EwEPlugin.IPlugin.Author
        Get
            Return "Mark Platts CEFAS"
        End Get
    End Property

    Public ReadOnly Property Contact() As String _
        Implements EwEPlugin.IPlugin.Contact
        Get
            Return "ewedevlowestoft@gmail.com"
        End Get
    End Property

    Public ReadOnly Property Description() As String _
        Implements EwEPlugin.IPlugin.Description
        Get
            Return "Plug-in to run CEFAS MSE"
        End Get
    End Property

    Public ReadOnly Property Name As String Implements EwEPlugin.IPlugin.Name
        Get
            Return "ndCefasMSE"
        End Get
    End Property

    Public ReadOnly Property ControlImage As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

#End Region ' UI integration

#Region " Helper methods "

    Private Function HasUI() As Boolean
        If (Me.m_frm Is Nothing) Then Return False
        Return Not Me.m_frm.IsDisposed
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Resolve a name and index to a <see cref="cEcoPathGroupInput"/> instance.
    ''' </summary>
    ''' <param name="strName">The name to resolve.</param>
    ''' <param name="iIndex">The index to resolve.</param>
    ''' <returns>A <see cref="cEcoPathGroupInput"/> instance, or Nothing if
    ''' the index or name did not match any of the present groups.</returns>
    ''' <remarks>Note that name comparison is not case sensitive.</remarks>
    ''' -----------------------------------------------------------------------
    Private Function ResolveGroup(strName As String, iIndex As Integer) As cEcoPathGroupInput
        If (iIndex < 1) Or (iIndex > Me.Core.nGroups) Then Return Nothing
        Dim grp As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iIndex)
        If String.Compare(grp.Name, strName, True) <> 0 Then
            Return Nothing
        End If
        Return grp
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Resolve a name and index to a <see cref="cEcopathFleetInput"/> instance.
    ''' </summary>
    ''' <param name="strName">The name to resolve.</param>
    ''' <param name="iIndex">The index to resolve.</param>
    ''' <returns>A <see cref="cEcopathFleetInput"/> instance, or Nothing if
    ''' the index or name did not match any of the present fleets.</returns>
    ''' <remarks>Note that name comparison is not case sensitive.</remarks>
    ''' -----------------------------------------------------------------------
    Private Function ResolveFleet(strName As String, iIndex As Integer) As cEcopathFleetInput
        If (iIndex < 1) Or (iIndex > Me.Core.nFleets) Then Return Nothing
        Dim flt As cEcopathFleetInput = Me.Core.EcopathFleetInputs(iIndex)
        If String.Compare(flt.Name, strName, True) <> 0 Then
            Return Nothing
        End If
        Return flt
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Notify the user of an event.
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="importance"></param>
    ''' <param name="strHyperlink"></param>
    ''' -----------------------------------------------------------------------
    Friend Sub InformUser(strMessage As String, importance As eMessageImportance, _
                          Optional strHyperlink As String = "", _
                          Optional astrSubMessages As String() = Nothing)

        If (Me.Core Is Nothing) Then Return

        Dim msg As New cMessage(strMessage, eMessageType.Any, eCoreComponentType.External, importance)
        msg.Hyperlink = strHyperlink
        If (astrSubMessages IsNot Nothing) Then
            For Each strSubMessage As String In astrSubMessages
                msg.AddVariable(New cVariableStatus(eStatusFlags.OK, strSubMessage, eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0))
            Next
        End If
        Me.Core.Messages.SendMessage(msg)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ask the user a question.
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="style"></param>
    ''' <param name="importance"></param>
    ''' <param name="replyDefault"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Friend Function AskUser(strMessage As String, _
                            style As eMessageReplyStyle, _
                            Optional importance As eMessageImportance = eMessageImportance.Question, _
                            Optional replyDefault As eMessageReply = eMessageReply.OK) As eMessageReply

        If (Me.Core Is Nothing) Then Return replyDefault

        Dim fmsg As New cFeedbackMessage(strMessage, eCoreComponentType.External, eMessageType.Any, importance, style)
        fmsg.Reply = replyDefault
        Me.Core.Messages.SendMessage(fmsg)
        Return fmsg.Reply

    End Function

    Private Sub OnCoreMessage(ByRef msg As cMessage)

        Dim bRefresh As Boolean = False

        ' Refresh when Core settings have changed
        If (msg.Type = eMessageType.GlobalSettingsChanged) Then
            bRefresh = True
        End If

        If (msg.Source = eCoreComponentType.EcoPath) Then
            bRefresh = (msg.Type = eMessageType.DataModified Or msg.Type = eMessageType.DataAddedOrRemoved Or msg.Type = eMessageType.DataValidation)
        End If

        ' Refresh upon ecosim scenario load
        If (msg.Type = eMessageType.DataAddedOrRemoved And msg.Source = eCoreComponentType.EcoSim) Then
            bRefresh = True
        End If

        If (bRefresh = True And Me.HasUI) Then
            Me.InvalidateConfiguration()
        End If

    End Sub

    Private Sub onPreProcessMessage(ByVal msg As EwEUtils.Core.IMessage, ByRef bCancelMessage As Boolean) _
        Implements EwEPlugin.IMessageFilterPlugin.PreProcessMessage

        ' JS 03Oct13: ONLY SUPPRESS MESSAGES WHEN MSE IS RUNNING! 
        If (Me.MSE.RunState = cMSE.eRunStates.Idle) Then Return

        'Plugin Point called to cancel a message
        Select Case msg.Type

            Case eMessageType.Estimate_BA, _
                 eMessageType.Estimate_Net_Migration, _
                 eMessageType.EE
                Console.WriteLine("! MSE suppressed message " & msg.Message)
                bCancelMessage = True

            Case Else
                bCancelMessage = False

        End Select

    End Sub

#End Region ' Helper methods

End Class
