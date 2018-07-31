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
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' <para>Class that performs message functions:</para>
''' <list type="bullet">
''' <item><description>Keep a history of <see cref="cMessage">core messages</see>;</description></item>
''' <item><description>Invoke user prompts when the core requests <see cref="cFeedbackMessage">user feedback</see>;</description></item>
''' <item><description>Suppress user prompts.</description></item>
''' </list>
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cMessageHistory
    Implements IUIElement
    Implements IDisposable

#Region " Privates "

    ''' <summary>The connected UI context.</summary>
    Private m_uic As cUIContext = Nothing
    ''' <summary>Message suppressor.</summary>
    Private m_msh As New cMessageStateHandler()
    ''' <summary>Message history.</summary>
    Private m_lHistory As New List(Of cHistoryItem)
    ''' <summary>Core message handlers.</summary>
    Private m_dtMessageHanders As New Dictionary(Of eCoreComponentType, cMessageHandler)

#End Region ' Privates

#Region " Helper class "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A history item logged in the <see cref="cMessageHistory">message history</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cHistoryItem

#Region " Private vars "

        ''' <summary>Message text</summary>
        Private m_strText As String = ""
        ''' <summary>Message hyperlink</summary>
        Private m_strHyperlink As String = ""
        ''' <summary>Message importance</summary>
        Private m_importance As eMessageImportance = eMessageImportance.Information
        ''' <summary>History item children.</summary>
        Private m_lItems As New List(Of cHistoryItem)
        ''' <summary>Abstract representation of the value that corresponded to a message.</summary>
        Private m_strValueID As String = ""
        ''' <summary>Core component where this message came from.</summary>
        ''' <remarks>This value can be deducted from the value ID, but that is too
        ''' cumbersome. Instead, core component is cached for easy access.</remarks>
        Private m_source As eCoreComponentType = eCoreComponentType.NotSet
        ''' <summary>Date and time message was generated.</summary>
        Private m_time As DateTime = Nothing
        ''' <summary>Note whether the message was automatically suppressed.</summary>
        Private m_bSuppressed As Boolean = False

#End Region ' Private vars 

#Region " Construction "

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Create a history item for a message.
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see>
        ''' to obtain abstract value representation from.</param>
        ''' <param name="msg"><see cref="cMessage">Message</see> to create
        ''' history item for.</param>
        ''' <remarks>
        ''' This will create sub-items for all information attached to the
        ''' <paramref name="msg">message</paramref>, such as 
        ''' <see cref="cVariableStatus">variable status information</see>.
        ''' </remarks>
        ''' ---------------------------------------------------------------
        Public Sub New(ByVal pm As cPropertyManager, _
                       ByVal msg As cMessage)

            Me.New(msg.Message, msg.Importance, msg.Hyperlink)
            Me.m_source = msg.Source
            Me.m_bSuppressed = msg.Suppressed

            For Each vs As cVariableStatus In msg.Variables
                Me.m_lItems.Add(New cHistoryItem(pm, vs, Me.m_source))
            Next

            ' Is Feedback message?
            If (TypeOf msg Is cFeedbackMessage) Then

                ' #Yes: include reply as a child item
                Dim fmsg As cFeedbackMessage = DirectCast(msg, cFeedbackMessage)
                Dim strReply As String = ""

                Select Case fmsg.ReplyStyle

                    Case eMessageReplyStyle.OK_CANCEL
                        Select Case fmsg.Reply
                            Case eMessageReply.OK
                                strReply = My.Resources.GENERIC_REPLY_OK
                            Case eMessageReply.CANCEL
                                strReply = My.Resources.GENERIC_REPLY_CANCEL
                        End Select

                    Case eMessageReplyStyle.YES_NO, _
                         eMessageReplyStyle.YES_NO_CANCEL

                        Select Case fmsg.Reply
                            Case eMessageReply.YES
                                strReply = My.Resources.GENERIC_REPLY_YES
                            Case eMessageReply.NO
                                strReply = My.Resources.GENERIC_REPLY_NO
                            Case eMessageReply.CANCEL
                                strReply = My.Resources.GENERIC_REPLY_CANCEL
                        End Select

                End Select

                If (Not String.IsNullOrEmpty(strReply)) Then
                    ' Add reply node
                    Me.m_lItems.Add(New cHistoryItem(strReply, eMessageImportance.Information))
                End If
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Internal constructor, generate a history item for a
        ''' <see cref="cVariableStatus">variable status</see>.
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see>
        ''' to obtain abstract value representation from.</param>
        ''' <param name="vs"><see cref="cVariableStatus">variable status</see>
        ''' to create item for.</param>
        ''' -------------------------------------------------------------------
        Private Sub New(ByVal pm As cPropertyManager, _
                        ByVal vs As cVariableStatus, _
                        ByVal source As eCoreComponentType)

            Me.New(vs.Message, DirectCast(Math.Max(vs.Importance, eMessageImportance.Information), eMessageImportance))
            Me.m_strValueID = pm.ExtractPropertyID(vs)
            Me.m_source = source

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Internal constructor, generate a history item.
        ''' </summary>
        ''' <param name="strMessage">Text to generate history item for.</param>
        ''' <param name="imp"><see cref="eMessageImportance">message importance</see>,
        ''' inherited from the parent message.</param>
        ''' <param name="strHyperlink">Hyperlink to include in the message.</param>
        ''' -------------------------------------------------------------------
        Private Sub New(ByVal strMessage As String, _
                        ByVal imp As eMessageImportance, _
                        Optional ByVal strHyperlink As String = "")

            Me.m_strText = strMessage
            Me.m_strHyperlink = strHyperlink
            Me.m_importance = imp
            Me.m_time = DateTime.Now

        End Sub

#End Region ' Construction

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the text a message was logged with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Text() As String
            Get
                Return Me.m_strText
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eMessageImportance">message importance</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Importance() As eMessageImportance
            Get
                Return Me.m_importance
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the hyperlink that a message was logged with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Hyperlink As String
            Get
                Return Me.m_strHyperlink
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get all child history items for this item.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Children() As cHistoryItem()
            Get
                Return Me.m_lItems.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eCoreComponentType">source</see> that this message
        ''' originated from.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Source() As eCoreComponentType
            Get
                Return Me.m_source
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the time that this message was created.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Time() As DateTime
            Get
                Return Me.m_time
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract properties for logged history items.
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see>
        ''' to extract <see cref="cProperty">properties</see> from.</param>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Properties(ByVal pm As cPropertyManager) As cProperty()
            Get
                Dim lProps As New List(Of cProperty)
                If Me.IsValid Then Me.GetProperties(pm, lProps)
                Return lProps.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether this history item is still linked to its core data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property IsValid() As Boolean
            Get
                Return (Me.m_source <> eCoreComponentType.NotSet)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Invalidate the core data link of this history item.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Invalidate()
            If Not Me.IsValid Then Return
            Me.m_source = eCoreComponentType.NotSet
            For Each item As cHistoryItem In Me.m_lItems
                item.Invalidate()
            Next
        End Sub

#End Region ' Public interfaces 

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Recursively extract all properties from this item and onward.
        ''' </summary>
        ''' <param name="pm"></param>
        ''' <param name="lProps"></param>
        ''' -------------------------------------------------------------------
        Private Sub GetProperties(ByVal pm As cPropertyManager, ByVal lProps As List(Of cProperty))
            Dim prop As cProperty = pm.GetProperty(Me.m_strValueID)
            If (prop IsNot Nothing) Then lProps.Add(prop)
            For Each item As cHistoryItem In Me.m_lItems
                item.GetProperties(pm, lProps)
            Next
        End Sub

#End Region ' Internals

    End Class

#End Region ' Helper class

#Region " Construction / destruction "

    Public Sub New()
    End Sub

    Public Property UIContext() As ScientificInterfaceShared.Controls.cUIContext _
        Implements ScientificInterfaceShared.Controls.IUIElement.UIContext
        Get
            Return Me.m_uic
        End Get
        Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
            If (Object.Equals(Me.m_uic, value)) Then Return
            If (Me.m_uic IsNot Nothing) Then Me.ConfigMessageHandlers(False)
            Me.m_uic = value
            If (Me.m_uic IsNot Nothing) Then Me.ConfigMessageHandlers(True)
        End Set
    End Property

    Private m_bDisposed As Boolean = False        ' To detect redundant calls

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not Me.m_bDisposed Then
            Me.UIContext = Nothing
        End If
        Me.m_dtMessageHanders.Clear()
        Me.m_bDisposed = True
        GC.SuppressFinalize(Me)
    End Sub

#End Region ' Construction / destruction

#Region " Public interfaces "

    ''' <summary>Event to signify that an item was added to the history.</summary>
    ''' <param name="sender">The history instance the item was added to.</param>
    ''' <param name="item">The added <see cref="cHistoryItem">item</see>.</param>
    Public Event OnHistoryItemAdded(ByVal sender As cMessageHistory, ByVal item As cHistoryItem)

    ''' <summary>Event to signify that something big changed about the history log.</summary>
    ''' <param name="sender">The history instance that was refreshed.</param>
    Public Event OnHistoryRefreshed(ByVal sender As cMessageHistory)

    ''' <summary>
    ''' Clear the message suppress cache.
    ''' </summary>
    Public Sub Refresh()
        Try
            RaiseEvent OnHistoryRefreshed(Me)
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Get all history items.
    ''' </summary>
    Public ReadOnly Property Items() As cHistoryItem()
        Get
            Return Me.m_lHistory.ToArray
        End Get
    End Property


#End Region ' Public interfaces

#Region " Internals "

    Private Sub ConfigMessageHandler(ByVal src As eCoreComponentType, ByVal bSet As Boolean)

        Dim mh As cMessageHandler = Nothing

        If (src = eCoreComponentType.NotSet) Then Return

        If bSet Then
            mh = New cMessageHandler(AddressOf AllMessagesHandler, src, eMessageType.Any, Me.UIContext.SyncObject)
#If DEBUG Then
            ' Name the message handler for profiling
            mh.Name = "cMessageHistory::All"
#End If
            Me.m_dtMessageHanders(src) = mh
            Me.UIContext.Core.Messages.AddMessageHandler(mh)
        Else
            mh = Me.m_dtMessageHanders(src)
            Me.m_dtMessageHanders.Remove(src)
            Me.UIContext.Core.Messages.RemoveMessageHandler(mh)
            mh = Nothing
        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Hook up to, or connect from, core messages.
    ''' </summary>
    ''' <param name="bSet">True to set, False to clear.</param>
    ''' -------------------------------------------------------------------
    Private Sub ConfigMessageHandlers(ByVal bSet As Boolean)

        ' Set up message handlers
        For Each src As eCoreComponentType In [Enum].GetValues(GetType(eCoreComponentType))
            Me.ConfigMessageHandler(src, bSet)
        Next

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Universal messages listener
    ''' </summary>
    ''' <param name="msg">The message to listen to</param>
    ''' -------------------------------------------------------------------
    Private Sub AllMessagesHandler(ByRef msg As cMessage)

        Dim strMessage As String = msg.Message
        Dim bSuppressVarMessage As Boolean = False
        Dim iMaxMessages As Integer = Math.Max(10, Math.Min(200, My.Settings.StatusMaxMessages))

        If String.IsNullOrEmpty(msg.Message) Then Return

        ' Early bail-out for messages hidden from UI
        If Not TypeOf (msg) Is cFeedbackMessage Then
            Select Case msg.Importance
                Case eMessageImportance.Progress
                    ' Ignore progress messages
                    Return
                Case eMessageImportance.Maintenance
                    ' Validation messages may be shown
                    If (msg.Type <> eMessageType.DataValidation) Or (My.Settings.StatusShowVariableValidations = False) Then
                        Return
                    End If

            End Select
        End If

        ' Is not a suppressed message?
        If Not msg.Suppressed Then
            Try
                ' #Yes: Is a feedback message?
                If (TypeOf msg Is cFeedbackMessage) Then
                    ' #Yes: handle it
                    Me.HandleFeedbackMessage(DirectCast(msg, cFeedbackMessage))
                Else
                    ' #No: handle pop-up feedback for criticals and warnings only
                    Select Case msg.Importance
                        Case eMessageImportance.Critical
                            ' Always show critical messages
                            Me.ShowMessageBox(msg)

                        Case eMessageImportance.Warning
                            ' Only show wanring messages when core is not busy
                            Dim sm As cCoreStateMonitor = Me.UIContext.Core.StateMonitor
                            If (Not sm.IsBusy) Then Me.ShowMessageBox(msg)

                    End Select
                End If
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End If

        ' Now enter messages in message log
        Try
            Dim item As New cHistoryItem(Me.m_uic.PropertyManager, msg)
            Me.m_lHistory.Add(item)
            RaiseEvent OnHistoryItemAdded(Me, item)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        ' Has a major change occurred?
        If (msg.Type = eMessageType.DataAddedOrRemoved) Then

            Dim acomps() As eCoreComponentType = Me.GetChildComponents(msg.Source)

            ' Invalidate messages linking to 'old' data
            For Each item As cHistoryItem In Me.m_lHistory
                If Array.IndexOf(acomps, item.Source) > -1 Then item.Invalidate()
            Next

            ' Clear all suppressed message flags
            For Each comp As eCoreComponentType In acomps
                Me.m_msh.Clear(comp)
            Next

        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; handles a feedback message by presenting the user with
    ''' a message box.
    ''' </summary>
    ''' <param name="msg">The <see cref="cFeedbackMessage">feedback message</see>
    ''' to handle.</param>
    ''' -------------------------------------------------------------------
    Private Sub HandleFeedbackMessage(ByVal msg As cFeedbackMessage)

        Dim mbb As MessageBoxButtons = MessageBoxButtons.YesNo
        Dim mbi As MessageBoxIcon = MessageBoxIcon.Question
        Dim dlr As DialogResult = System.Windows.Forms.DialogResult.No
        Dim strMessage As String = ""

        ' Early bail-out
        If (msg Is Nothing) Then Return
        If (msg.Suppressed) Then Return

        ' Translate feedback style into .NET MessageBox style
        Select Case msg.ReplyStyle
            Case eMessageReplyStyle.OK_CANCEL
                mbb = MessageBoxButtons.OKCancel
            Case eMessageReplyStyle.YES_NO
                mbb = MessageBoxButtons.YesNo
            Case eMessageReplyStyle.YES_NO_CANCEL
                mbb = MessageBoxButtons.YesNoCancel
            Case eMessageReplyStyle.OK
                mbb = MessageBoxButtons.OK
        End Select

        Select Case msg.Importance
            Case eMessageImportance.Progress, eMessageImportance.Maintenance
                mbi = MessageBoxIcon.Question
            Case eMessageImportance.Information
                mbi = MessageBoxIcon.Information
            Case eMessageImportance.Warning
                mbi = MessageBoxIcon.Warning
            Case eMessageImportance.Critical
                mbi = MessageBoxIcon.Error
        End Select

        ' Pop the question
        Me.ToMessageBoxText(msg, strMessage)

        ' Is message suppressable?
        If msg.Suppressable Then

            ' #Yes: handle autoreply

            ' Sanity check
            Debug.Assert(msg.Type <> eMessageType.NotSet, "Feedback message not propery configured for auto-reply: messagetype not set")

            ' Get reply, if any
            dlr = Me.m_msh.AutoReply(msg.Source, msg.Type)
            ' Is 'none'?
            If (dlr = System.Windows.Forms.DialogResult.None) Then
                ' #Yes: prompt needed

                ' Assume to repeat the question
                Dim bChecked As Boolean = False
                ' Show dialog
                dlr = cCustomMessageBox.Show(Me.UIContext, strMessage, frmEwE6.GetInstance().Text, _
                                             mbb, mbi, _
                                             bChecked, My.Resources.PROMPT_MESSAGE_HIDE)
                ' Auto-reply requested?
                If bChecked Then
                    ' #Yes: store auto-reply
                    Me.m_msh.AutoReply(msg.Source, msg.Type) = dlr
                End If
            End If
        Else
            '' Invoke message box
            'cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_WAITING)
            Try
                dlr = cCustomMessageBox.Show(Me.UIContext, strMessage, frmEwE6.GetInstance().Text, mbb, mbi)
            Catch ex As Exception
                cLog.Write(ex, "cMessageHistory::HandleFeedbackMessage")
            End Try
            'cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)
        End If

        ' Translate .NET MessageBox result into reply
        Select Case dlr
            Case DialogResult.Cancel
                msg.Reply = eMessageReply.CANCEL
            Case DialogResult.OK
                msg.Reply = eMessageReply.OK
            Case DialogResult.Yes
                msg.Reply = eMessageReply.YES
            Case DialogResult.No
                msg.Reply = eMessageReply.NO
            Case Else
                Debug.Assert(False, String.Format("Message box result {0} not supported", dlr))
        End Select

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; invokes a Windows Message Box for a EwE Core
    ''' <see cref="cMessage">Message</see>.
    ''' </summary>
    ''' <param name="msg">The <see cref="cMessage">Message</see> to show a
    ''' Message Box for.</param>
    ''' <returns>
    ''' True if a problem occurred displaying the message
    ''' </returns>
    ''' -------------------------------------------------------------------
    Private Function ShowMessageBox(ByVal msg As cMessage) As Boolean

        Dim strMessage As String = ""
        Dim mbb As MessageBoxButtons = MessageBoxButtons.OK
        Dim mbi As MessageBoxIcon = MessageBoxIcon.Information
        Dim bError As Boolean = False

        ' Sanity check
        If (msg IsNot Nothing) Then

            bError = ToMessageBoxText(msg, strMessage)

            ' Resolve what icon to show
            Select Case msg.Importance
                Case eMessageImportance.Critical
                    mbi = MessageBoxIcon.Error
                Case eMessageImportance.Warning
                    mbi = MessageBoxIcon.Warning
                Case eMessageImportance.Information
                    mbi = MessageBoxIcon.Information
            End Select

            ' == Show the message ==

            ' Can the message be suppressed?
            ' JS 19Nov13: Can only suppress messages with suppressable features
            If msg.Suppressable And ((msg.Source <> eCoreComponentType.NotSet) Or (msg.Type <> eMessageType.NotSet)) Then

                ' #Yes: check suppressed state

                ' Sanity check
                Debug.Assert(msg.Type <> eMessageType.NotSet, "Message not propery configured for suppression: messagetype not set")

                If (Not Me.m_msh.IsSuppressed(msg.Source, msg.Type)) Then
                    ' #No: Good, prepare to show message
                    ' Assume message will not be suppressed
                    Dim bSuppress As Boolean = False
                    ' Invoke the special message box
                    cCustomMessageBox.Show(Me.UIContext, strMessage, frmEwE6.GetInstance().Text, _
                                           mbb, mbi, _
                                           bSuppress, My.Resources.PROMPT_MESSAGE_HIDE)
                    ' Set suppressed state in administration
                    Me.m_msh.IsSuppressed(msg.Source, msg.Type) = bSuppress
                End If
                ' Set message suppressed state
                msg.Suppressed = Me.m_msh.IsSuppressed(msg.Source, msg.Type)
            Else
                ' #No: show the message
                ' The one and only static popup message box in EwE
                MessageBox.Show(strMessage, frmEwE6.GetInstance().Text, mbb, mbi, MessageBoxDefaultButton.Button1)
            End If
        End If

        Return bError
    End Function

    Private Function ToMessageBoxText(ByVal msg As cMessage, ByRef strMessage As String) As Boolean

        Dim sb As New StringBuilder()
        Dim iNumSubLines As Integer = 0
        Dim strTmp As String = ""
        Dim bError As Boolean = False

        ' Sanity check
        If (msg IsNot Nothing) Then

            sb.AppendLine(msg.Message)

            ' Concatenate all child messages
            For Each vs As cVariableStatus In msg.Variables
                Select Case iNumSubLines

                    Case 0 To 9
                        strTmp = vs.Message
                        If (Not String.IsNullOrEmpty(strTmp)) And (String.Compare(vs.Message, msg.Message, True) <> 0) Then
                            sb.AppendLine()
                            sb.Append(strTmp)
                            iNumSubLines += 1
                        End If

                    Case 10
                        sb.AppendLine()
                        sb.AppendLine("...")
                        sb.AppendLine(My.Resources.PROMPT_STATUS_FURTHERDETAILS)
                        bError = True
                        Exit For

                End Select
            Next
        End If

        strMessage = sb.ToString().Replace("\n", cStringUtils.vbNewline)
        Return bError

    End Function

    Private Function GetChildComponents(ByVal source As eCoreComponentType) As eCoreComponentType()

        Select Case source

            Case eCoreComponentType.Ecotracer
                ' No children
                Return New eCoreComponentType() {eCoreComponentType.Ecotracer}

            Case eCoreComponentType.EcoSpace
                Return New eCoreComponentType() {eCoreComponentType.EcoSpace, eCoreComponentType.Ecotracer}

            Case eCoreComponentType.EcoSim
                Return New eCoreComponentType() {eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace, eCoreComponentType.Ecotracer}

            Case eCoreComponentType.EcoPath
                Return New eCoreComponentType() {eCoreComponentType.EcoPath, eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace, eCoreComponentType.Ecotracer}

            Case eCoreComponentType.Core
                Return New eCoreComponentType() {eCoreComponentType.Core, eCoreComponentType.EcoPath, eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace, eCoreComponentType.Ecotracer}

        End Select
        Return New eCoreComponentType() {source}

    End Function

#End Region ' Internals 

End Class
