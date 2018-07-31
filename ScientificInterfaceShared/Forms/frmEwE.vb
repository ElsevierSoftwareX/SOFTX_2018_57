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

Imports System.ComponentModel
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports WeifenLuo.WinFormsUI.Docking
Imports ScientificInterfaceShared.Properties
Imports System.Xml
Imports System.Drawing.Printing

#End Region ' Imports

Namespace Forms

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' <para>Base class for forms in EwE6. This class provides a mechanism to respond to
    ''' core messages for updating its content without having to construct each
    ''' required message handler. Instead, a central message handling administration
    ''' dispatched messages to inherited forms.</para>
    ''' <para>
    ''' To build an EwE6 form based on this class to gain the benefits of automatic
    ''' message delivery, perform the following steps:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Inherit your form from EwE6Form,</description></item>
    ''' <item><description>In the Load event, specify the message source(s) that
    ''' the form class should respond to via 
    ''' <see cref="frmEwE.CoreComponents">frmEwE.CoreComponents</see></description>,</item>
    ''' <item><description>In the Unload event, clear the message sources(s) by
    ''' setting <see cref="frmEwE.CoreComponents">frmEwE.CoreComponents</see>
    ''' to Nothing,</description></item>
    ''' <item>Override <see cref="frmEwE.OnCoreMessage">frmEwE.OnCoreMessage</see>
    ''' and implement the response to the message.</item>
    ''' </list>
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class frmEwE
        Inherits frmEwEDockContent
        Implements IUIElement

#Region " Private helper classes "

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Helper class; automatically instructs registered EwEForms to refresh their 
        ''' content whenever core data has been added or removed.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Private Class cEwEFormRefresh

#Region " Private vars "

            ''' <summary>Administration of registered forms per message source type.</summary>
            Private m_dictSourceToForm As New Dictionary(Of eCoreComponentType, List(Of frmEwE))
            Private m_so As System.Threading.SynchronizationContext = Nothing
            Private m_core As cCore = Nothing

#End Region ' Private vars

#Region " Constructor "

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Private constructor to enforce singleton.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub New(ByVal uic As cUIContext)

                ' Eek!
                Debug.Assert(uic IsNot Nothing)

                Me.m_so = uic.SyncObject
                Me.m_core = uic.Core
                Me.Initialize()
            End Sub

#End Region ' Constructor

#Region " Public access "

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Register a form to automated refresh instructions.
            ''' </summary>
            ''' <param name="form">The <see cref="frmEwE">frmEwE</see> to register.</param>
            ''' <param name="messageSource">The <see cref="eCoreComponentType">message source</see> to monitor 
            ''' for <see cref="eMessageType.DataAddedOrRemoved">DataAddedOrRemoved</see> messages.</param>
            ''' <remarks>
            ''' A registered form will receive <see cref="frmEwE.OnCoreMessage">OnCoreMessage</see>
            ''' calls for <see cref="eMessageType.DataAddedOrRemoved">DataAddedOrRemoved</see>
            ''' messages originating from the given <paramref name="messageSource">message source</paramref>.
            ''' </remarks>
            ''' -----------------------------------------------------------------------
            Public Sub RegisterForm(ByVal form As frmEwE, ByVal messageSource As eCoreComponentType)
                Dim lForms As List(Of frmEwE) = Nothing
                If Me.m_dictSourceToForm.ContainsKey(messageSource) Then
                    lForms = Me.m_dictSourceToForm(messageSource)
                Else
                    lForms = New List(Of frmEwE)
                    Me.m_dictSourceToForm.Add(messageSource, lForms)
                End If

                If Not lForms.Contains(form) Then lForms.Add(form)
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Unregister a form from automated refresh instructions.
            ''' </summary>
            ''' <param name="form">The <see cref="frmEwE">frmEwE</see> to unregister.</param>
            ''' <param name="messageSource">The <see cref="eCoreComponentType">message source</see> to 
            ''' stop monitoring for <see cref="eMessageType.DataAddedOrRemoved">DataAddedOrRemoved</see>
            ''' messages.</param>
            ''' <remarks>
            ''' The form will no longer receive <see cref="frmEwE.OnCoreMessage">OnCoreMessage</see>
            ''' calls for <see cref="eMessageType.DataAddedOrRemoved">DataAddedOrRemoved</see>
            ''' messages originating from the given <paramref name="messageSource">message source</paramref>.
            ''' </remarks>
            ''' -----------------------------------------------------------------------
            Public Sub UnregisterForm(ByVal form As frmEwE, ByVal messageSource As eCoreComponentType)
                Debug.Assert(Me.m_dictSourceToForm.ContainsKey(messageSource), String.Format("Form not defined for message source {0}", messageSource.ToString()))
                Me.m_dictSourceToForm(messageSource).Remove(form)
            End Sub

#End Region ' Public access

#Region " Message handling "

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Initialize this class to listen to messages from an EwE Core instance.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub Initialize()
                Me.ConfigMessageHandlers(True)
            End Sub

            Dim m_dtMessageHanders As New Dictionary(Of eCoreComponentType, cMessageHandler)

            Private Sub ConfigMessageHandler(ByVal src As eCoreComponentType, ByVal bSet As Boolean)

                Dim mh As cMessageHandler = Nothing

                If (src = eCoreComponentType.NotSet) Then Return

                If bSet Then
                    mh = New cMessageHandler(AddressOf AllMessagesHandler, src, eMessageType.Any, Me.m_so)
#If DEBUG Then
                    mh.Name = "cEwEFormRefresh::Any"
#End If
                    Me.m_dtMessageHanders(src) = mh
                    Me.m_core.Messages.AddMessageHandler(mh)
                Else
                    mh = Me.m_dtMessageHanders(src)
                    Me.m_dtMessageHanders.Remove(src)
                    Me.m_core.Messages.RemoveMessageHandler(mh)
                    mh = Nothing
                End If

            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Hook up to core messages
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
            ''' Universal messages listener.
            ''' </summary>
            ''' <param name="msg">The message to listen to.</param>
            ''' -------------------------------------------------------------------
            Private Sub AllMessagesHandler(ByRef msg As cMessage)

                Dim lForms As List(Of frmEwE) = Nothing

                If Me.m_dictSourceToForm.ContainsKey(msg.Source) Then
                    lForms = Me.m_dictSourceToForm(msg.Source)
                    If lForms IsNot Nothing Then
                        For Each form As frmEwE In lForms
                            form.OnCoreMessage(msg)
                        Next
                    End If
                End If

            End Sub

#End Region ' Message handling 

        End Class

#End Region

#Region " Private variables "

        Private Shared s_refresh As cEwEFormRefresh = Nothing

        ''' <summary>Almighty UI context holding form UI contextual information.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>Array of message sources that invalidate the information displayed in a form.</summary>
        Private m_aMessageSources As eCoreComponentType() = Nothing
         ''' <summary>States whether the form is running. Only valid for forms 
        ''' that are flagged as <see cref="IsRunForm"/>.</summary>
        Private m_bIsRunning As Boolean = False

        Private m_printDoc As PrintDocument = Nothing
        Private m_iPrintPage As Integer = 0

#End Region ' Private variables

#Region " Constructors "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
        End Sub

#End Region ' Constructors

#Region " Form overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Form load event override, retrieves and applies the original form position.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext IsNot Nothing) Then

                AddHandler Me.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged

                If (Me.DesignMode = False) And (Me.UIContext.FormSettings IsNot Nothing) Then
                    Me.UIContext.FormSettings.Apply(Me)
                End If
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Form close event override, stores the final form position.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            If (Me.UIContext IsNot Nothing) Then

                If (Me.UIContext.Help IsNot Nothing) Then
                    ' JS28Feb14: this is one of the memleak culprits that is keeping forms alive
                    ' Explicitly detach from help
                    Me.UIContext.Help.HelpTopic(Me) = ""
                End If

                RemoveHandler Me.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                Me.UIContext = Nothing
            End If

            Me.CoreComponents = Nothing

            MyBase.OnFormClosed(e)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cUIContext">UI context</see> that 
        ''' </summary>
        ''' <remarks>
        ''' Override this method to connect to the EwE Core and other UI-context
        ''' settings.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overridable Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
                If (frmEwE.s_refresh Is Nothing) Then frmEwE.s_refresh = New cEwEFormRefresh(Me.m_uic)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCore">core</see> that this form connects to.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public ReadOnly Property Core() As cCore
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.Core
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cStyleGuide">style guide</see> that this form 
        ''' connects to.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public ReadOnly Property StyleGuide() As cStyleGuide
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.StyleGuide
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cPropertyManager">property manager</see> that 
        ''' this form can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public ReadOnly Property PropertyManager() As cPropertyManager
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.PropertyManager
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCommandHandler">command handler</see> that 
        ''' this form can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public ReadOnly Property CommandHandler() As cCommandHandler
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.CommandHandler
            End Get
        End Property

#End Region ' Form overrides

#Region " Overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Method that is called whenever messages for a form arrive for any of the
        ''' <see cref="CoreComponents"/> that the form is registered to.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Sub OnCoreMessage(ByVal msg As cMessage)
            ' NOP
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the core execution state that a form needs for its content.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property CoreExecutionState() As eCoreExecutionState

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to prevent active forms reflecting active runs from closing.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnFormClosing(ByVal e As System.Windows.Forms.FormClosingEventArgs)

            If (Me.UIContext IsNot Nothing) And (Me.DesignMode = False) Then
                If (Me.UIContext.FormSettings IsNot Nothing) Then
                    If (Me.UIContext.FormSettings IsNot Nothing) Then
                        ' Store form position BEFORE form is closed
                        Me.UIContext.FormSettings.Store(Me)
                    End If
                End If
            End If

            ' Just in case
            Me.EndPrint()

            MyBase.OnFormClosing(e)

            ' Prevent active run forms from closing.
            If Me.IsRunForm And Me.IsRunning Then e.Cancel = True

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Callback, invoked when the style guide has changed in response to
        ''' <see cref="cStyleGuide.StyleGuideChanged"/>
        ''' </summary>
        ''' <param name="ct"></param>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            ' NOP
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set local settings for this form.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property Settings() As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the content of the form as an image for printing.
        ''' </summary>
        ''' <param name="rcPrint">The print area.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function GetPrintContent(ByVal rcPrint As Rectangle) As Image
            Dim bmp As New Bitmap(Me.ClientRectangle.Width, Me.ClientRectangle.Height, Imaging.PixelFormat.Format32bppArgb)
            bmp.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)
            Me.DrawToBitmap(bmp, Me.ClientRectangle)
            Return bmp
        End Function

#End Region ' Overrides

#Region " Printing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Starts multi-page printing.
        ''' </summary>
        ''' <returns>A <see cref="PrintDocument"/> to print with.</returns>
        ''' -------------------------------------------------------------------
        Public Function BeginPrint() As PrintDocument

            If (Me.m_printDoc Is Nothing) Then
                Me.m_printDoc = New PrintDocument
                Me.m_printDoc.DocumentName = Me.Text
                AddHandler Me.m_printDoc.PrintPage, AddressOf OnPrintMe
            End If
            Me.m_iPrintPage = 0
            Return Me.m_printDoc

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' End printing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub EndPrint()
            If (Me.m_printDoc IsNot Nothing) Then
                RemoveHandler Me.m_printDoc.PrintPage, AddressOf OnPrintMe
                Me.m_printDoc.Dispose()
                Me.m_printDoc = Nothing
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="args"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnPrintMe(ByVal sender As Object, ByVal args As PrintPageEventArgs)

            Try
                Dim img As Image = Me.GetPrintContent(args.MarginBounds)

                ' Determine number of pages
                Dim iDX As Integer = CInt(Math.Ceiling(img.Width / args.MarginBounds.Width))
                Dim iDY As Integer = CInt(Math.Ceiling(img.Height / args.MarginBounds.Height))
                Dim iNumPages As Integer = iDX * iDY

                Dim iX As Integer = Me.m_iPrintPage Mod iDX
                Dim iY As Integer = Me.m_iPrintPage \ iDX

                ' Check which part of the print area is represented by m_iPrintPage
                Dim rcPrint As New Rectangle(iX * args.MarginBounds.Width, _
                                             iY * args.MarginBounds.Height, _
                                             Math.Min(img.Width - iX * args.MarginBounds.Width, args.MarginBounds.Width), _
                                             Math.Min(img.Height - iY * args.MarginBounds.Height, args.MarginBounds.Height))
                ' Draw
                args.Graphics.DrawImage(img, args.MarginBounds.X, args.MarginBounds.Y, rcPrint, GraphicsUnit.Pixel)
                Me.m_iPrintPage += 1

                ' Done
                img.Dispose()

                args.HasMorePages = (Me.m_iPrintPage < iNumPages)

            Catch ex As Exception
                cLog.Write(ex, "OnPrint")
                args.HasMorePages = False
            End Try

        End Sub

#End Region ' Printing

#Region " Core messages "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the message sources that supply this form with data. See
        ''' <see cref="OnCoreMessage">OnCoreMessage</see> for a desciption
        ''' how these flags are being used.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Property CoreComponents() As eCoreComponentType()
            Get
                Return m_aMessageSources
            End Get

            Set(ByVal value As eCoreComponentType())

                ' Detach
                If Me.m_aMessageSources IsNot Nothing Then
                    For Each ms As eCoreComponentType In Me.m_aMessageSources
                        If (ms <> eCoreComponentType.NotSet) Then
                            frmEwE.s_refresh.UnregisterForm(Me, ms)
                        End If
                    Next
                End If

                ' Remember new
                Me.m_aMessageSources = value

                ' Attach
                If Me.m_aMessageSources IsNot Nothing Then
                    For Each ms As eCoreComponentType In Me.m_aMessageSources
                        If (ms <> eCoreComponentType.NotSet) Then
                            frmEwE.s_refresh.RegisterForm(Me, ms)
                        End If
                    Next
                End If

            End Set
        End Property

#End Region ' Core messages

#Region " Share and enjoy "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this form is an input form (true) or output form (false).
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsInputForm(ByVal state As eCoreExecutionState) As Boolean
            Return (state = eCoreExecutionState.EcopathLoaded) Or _
                   (state = eCoreExecutionState.EcosimLoaded) Or _
                   (state = eCoreExecutionState.EcospaceLoaded) Or _
                   (state = eCoreExecutionState.EcotracerLoaded)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this form is an input form (true) or output form (false).
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsOutputForm(ByVal state As eCoreExecutionState) As Boolean
            Return (state = eCoreExecutionState.EcopathCompleted) Or _
                   (state = eCoreExecutionState.EcosimCompleted) Or _
                   (state = eCoreExecutionState.EcospaceCompleted)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Flag stating whether a form is used to trigger model runs from.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Public Overridable ReadOnly Property IsRunForm() As Boolean
            Get
                Return False
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this form is running. A <see cref="IsRunForm"/> form
        ''' will not close when a run is still active.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <Browsable(False)> _
        Protected Overridable Property IsRunning() As Boolean
            Get
                Return Me.IsRunForm And Me.m_bIsRunning
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bIsRunning) Then
                    Me.m_bIsRunning = value
                    Me.UpdateControls()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Override to update the state of controls.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub UpdateControls()
            ' NOP
        End Sub

#End Region ' Share and enjoy

    End Class

End Namespace
