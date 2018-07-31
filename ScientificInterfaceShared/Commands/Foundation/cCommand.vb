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
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports EwEUtils.Core
Imports System.Text

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' The Command class implements a generic user interface command that can be 
    ''' linked to a series of User Interface Controls. All linked Controls will
    ''' be updated whenever the Command state changes.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cCommand

#Region " Private vars "

        ''' <summary>Command handler to connect to.</summary>
        Private m_cmdh As cCommandHandler = Nothing
        ''' <summary>Name of the command.</summary>
        Private m_strName As String = ""
        ''' <summary>Description of the command.</summary>
        Private m_strDescription As String = ""
        ''' <summary>Update lock flag to prevent involuntary loops.</summary>
        Private m_bLockUpdates As Boolean = False
        ''' <summary>Helper flag, stating whether a command is being invoked.</summary>
        Private m_bInvoking As Boolean = False
        ''' <summary>Command enabled state.</summary>
        Private m_bEnabled As Boolean = True
        ''' <summary>Command checked state.</summary>
        Private m_bChecked As Boolean = False
        ''' <summary>Entirely disable a command.</summary>
        Private m_bAvailable As Boolean = True

#End Region ' Private vars

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' <param name="strName">The name to assign to the command.</param>
        ''' <param name="cmdh">The <see cref="cCommandHandler"/> to associate this command with.</param>
        ''' 
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler, _
                       ByVal strName As String, _
                       Optional ByVal strDescription As String = "")

            ' Store ref to the handler
            Me.m_cmdh = cmdh

            ' Store name
            Me.m_strName = strName
            ' Store description
            Me.m_strDescription = strDescription

            ' Create storage for associated controls
            Me.m_dictControls = New Dictionary(Of Object, cControlHandler)

            cmdh.Add(Me)

        End Sub

#End Region ' Construction

#Region " Adding and removing GUI controls "

        ''' <summary>Controls connected to this command.</summary>
        Private m_dictControls As Dictionary(Of Object, cControlHandler)

        ''' ----------------------------------------------------------------------
        ''' <summary>
        ''' Connect the command to a User Interface Control. The command is fired 
        ''' when the control is interacted with.
        ''' </summary>
        ''' <param name="objGUI">The control to add.</param>
        ''' <remarks>
        ''' The <see cref="cCommandHandler"/> predefines a few <see cref="cControlHandler"/> 
        ''' types that interact with specific User Interface control classes. Ensure
        ''' that the objGUI object has an associated cControlHandler available,
        ''' otherwise the given Control will not be updated whenever the Command
        ''' state is changed.
        ''' </remarks>
        ''' ----------------------------------------------------------------------
        Public Sub AddControl(ByVal objGUI As Object)
            Me.AddControl(objGUI, Nothing)
        End Sub

        ''' ----------------------------------------------------------------------
        ''' <summary>
        ''' Connect the command to a User Interface Control. The command is fired 
        ''' when the control is interacted with.
        ''' </summary>
        ''' <param name="objGUI">The control to add.</param>
        ''' <param name="param">An parameter to pass to the command when it is invoked.</param>
        ''' <remarks>
        ''' The <see cref="cCommandHandler"/> predefines a few <see cref="cControlHandler"/> 
        ''' types that interact with specific User Interface control classes. Ensure
        ''' that the objGUI object has an associated cControlHandler available,
        ''' otherwise the given Control will not be updated whenever the Command
        ''' state is changed.
        ''' </remarks>
        ''' ----------------------------------------------------------------------
        Public Sub AddControl(ByVal objGUI As Object, param As Object)
            Dim parms As Object() = Nothing
            If param IsNot Nothing Then parms = New Object() {param}
            Me.AddControl(objGUI, parms)
        End Sub

        ''' ----------------------------------------------------------------------
        ''' <summary>
        ''' Connect the command to a User Interface Control. The command is fired 
        ''' when the control is interacted with.
        ''' </summary>
        ''' <param name="objGUI">The control to add.</param>
        ''' <param name="params">An array of parameters to pass to the command when
        ''' it is invoked.</param>
        ''' <remarks>
        ''' The <see cref="cCommandHandler"/> predefines a few <see cref="cControlHandler"/> 
        ''' types that interact with specific User Interface control classes. Ensure
        ''' that the objGUI object has an associated cControlHandler available,
        ''' otherwise the given Control will not be updated whenever the Command
        ''' state is changed.
        ''' </remarks>
        ''' ----------------------------------------------------------------------
        Public Sub AddControl(ByVal objGUI As Object, params As Object())

            Dim cmdh As cCommandHandler = Me.m_cmdh
            Dim t As Type = cmdh.GetControlHandlerType(objGUI)
            Dim objControlHandler As Object = Nothing
            Dim objParms() As Object = {Me, objGUI, params}
            Try
                Debug.Assert(t IsNot Nothing, "Control type not supported for automatic command handling!")
                objControlHandler = Activator.CreateInstance(t, objParms)
                If (TypeOf objControlHandler Is cControlHandler) Then
                    Me.m_dictControls(objGUI) = DirectCast(objControlHandler, cControlHandler)
                End If
            Catch ex As Exception
                ' Cannot log this!
                Debug.Assert(False, "UI init error on type " & t.ToString)
            End Try
        End Sub

        ''' ----------------------------------------------------------------------
        ''' <summary>
        ''' Call to remove a User Interface Control from a command.
        ''' </summary>
        ''' <param name="objGUI">The control to remove.</param>
        ''' ----------------------------------------------------------------------
        Public Sub RemoveControl(ByVal objGUI As Object)
            Try
                If (Me.m_dictControls.ContainsKey(objGUI)) Then
                    Me.m_dictControls.Remove(objGUI)
                End If
            Catch ex As Exception
                ' Hmm, command already detached?!
            End Try
        End Sub

        Public Sub Clear()
            Try
                Dim lobs As New List(Of Object)
                For Each obj As Object In Me.m_dictControls.Keys : lobs.Add(obj) : Next
                For Each obj As Object In lobs : Me.RemoveControl(obj) : Next
            Catch ex As Exception

            End Try
            Debug.Assert(Me.m_dictControls.Count = 0)
        End Sub

#End Region ' Adding and removing GUI controls 

#Region " Execution and updating "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that will be called before when a Command is executed.
        ''' </summary>
        ''' <param name="cmd">The command that is invoked.</param>
        ''' -----------------------------------------------------------------------
        Public Event OnPreInvoke(ByVal cmd As cCommand)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that will be called when a Command is executed.
        ''' </summary>
        ''' <param name="cmd">The command that is invoked.</param>
        ''' -----------------------------------------------------------------------
        Public Event OnInvoke(ByVal cmd As cCommand)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that will be called after a Command is executed.
        ''' </summary>
        ''' <param name="cmd">The command that is invoked.</param>
        ''' -----------------------------------------------------------------------
        Public Event OnPostInvoke(ByVal cmd As cCommand)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Base implementation for invoking a Command. Use either this implementation
        ''' or subclass the Command and implement a complex Invoke() variant.
        ''' </summary>
        ''' <remarks>
        ''' We realize that the method name 'Invoke' is somewhat awkward, suggesting
        ''' .NET invoke, which refers to asynchronous or cross-threaded method activation.
        ''' Ouch. This was accidental. Invoke could have been called 'Run', 'Execute',
        ''' 'AttaBoy', or anything else. We just picked 'Invoke' because it had a darn 
        ''' good ring to it. Note that command invokation is synchronous and blocking!
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Overridable Sub Invoke()

            Dim bIntercepted As Boolean = False

            ' Force the command to update
            Me.Update()

            ' Command enabled?
            If Me.Enabled Then
                ' Set invoking flag
                Me.m_bInvoking = True

                If (Me.m_cmdh.PluginManager IsNot Nothing) Then
                    ' Is intercepted (and hopefully handled) by a plug-in?
                    bIntercepted = (Me.m_cmdh.PluginManager.HandleCommand(Me))
                End If

                If (Not bIntercepted) Then

                    Try
                        ' 1. Pre-invoke
                        RaiseEvent OnPreInvoke(Me)
                    Catch ex As Exception
                        ' NOP
                    End Try

                    Try
                        ' 2. Invoke
                        RaiseEvent OnInvoke(Me)
                    Catch ex As Exception
                        ' NOP
                    End Try

                    Try
                        ' 3. Post-invoke
                        RaiseEvent OnPostInvoke(Me)
                    Catch ex As Exception
                        ' NOP
                    End Try

                End If

                ' Clear invoking flag
                Me.m_bInvoking = False
                ' Update associated user controls
                Me.Update()
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that will be called when a command is updated.
        ''' </summary>
        ''' <param name="cmd">The command that is updated.</param>
        ''' -----------------------------------------------------------------------
        Public Event OnUpdate(ByVal cmd As cCommand)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Base implementation for updating a Command. Use either this implementation
        ''' or subclass the Command and implement a complex Update() variant.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Sub Update()
            ' Can update?
            If Not m_bLockUpdates Then
                ' #Yes: lock to prevent loops
                m_bLockUpdates = True
                ' Call for changes
                RaiseEvent OnUpdate(Me)
                ' Dispatch changes
                For Each ctrlh As cControlHandler In Me.m_dictControls.Values
                    ctrlh.Update()
                Next
                ' Unlock
                m_bLockUpdates = False
            End If
        End Sub

#End Region ' Execution and updating

#Region " Public properties "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/Set the command available state. Unavailable commands cannot be 
        ''' enabled.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property IsAvailable() As Boolean
            Get
                Return Me.m_bAvailable
            End Get
            Set(ByVal bAvailable As Boolean)
                If (Me.m_bAvailable <> bAvailable) Then
                    Me.m_bAvailable = bAvailable
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/Set the command enabled state.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Enabled() As Boolean
            Get
                Return Me.m_bEnabled
            End Get
            Set(ByVal bEnable As Boolean)
                If (Me.m_bEnabled <> bEnable) Then
                    Me.m_bEnabled = bEnable
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/Set the command checked state.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Checked() As Boolean
            Get
                Return Me.m_bChecked
            End Get
            Set(ByVal bCheck As Boolean)
                If (Me.m_bChecked <> bCheck) Then
                    Me.m_bChecked = bCheck
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the command name.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Name() As String
            Get
                Return Me.m_strName
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Optional command description, which can be used for tool tips in user
        ''' interface elements, etc.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property Description As String
            Get
                Dim sb As New StringBuilder()
                If Not String.IsNullOrWhiteSpace(Me.m_strDescription) Then sb.AppendLine(Me.m_strDescription)
                If Not String.IsNullOrWhiteSpace(Me.Status) Then sb.AppendLine(Me.Status)
                Return sb.ToString()
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the command tag.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Tag() As Object

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the command is currently invoking.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property IsInvoking() As Boolean
            Get
                Return Me.m_bInvoking
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Dyanmic status of the command.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Status As String

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the command was successfully handled by the user.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property UserHandled As Boolean

#End Region ' Public properties

    End Class

End Namespace
