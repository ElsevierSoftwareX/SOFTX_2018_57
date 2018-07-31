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

Imports EwEUtils.Core

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class that monitors the run and data state of the MSE engine, and its
''' compatibility with the model loaded in EwE.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cMSEStateMonitor

#Region " Private vars "

    ''' <summary>The <see cref="cMSEPluginPoint"/> this monitor keeps an eye on.</summary>
    Private m_plugin As cMSEPluginPoint = Nothing
    ''' <summary>Cache of pre-determined states.</summary>
    Private m_statecache([Enum].GetValues(GetType(eState)).Length) As TriState

#End Region ' Private vars

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="plugin">The <see cref="cMSEPluginPoint"/> this monitor
    ''' will keep an eye on.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(plugin As cMSEPluginPoint)
        Me.m_plugin = plugin
        Me.Invalidate()
    End Sub

#End Region ' Constructor

#Region " Public bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Possible states for the MSE plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eState As Byte
        ''' <summary>MSE is ready to operate. The default state, and always true.</summary>
        Idle = 0
        ''' <summary>MSE has input parameters and a number of strategies.</summary>
        HasParams
        ''' <summary>MSE has generated models.</summary>
        HasModels
        ''' <summary>MSE has run.</summary>
        HasResults
        ''' <summary>MSE is running.</summary>
        IsRunning
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event that will be thrown when the state monitor is invalidated.
    ''' In response, user interfaces may want to re-assess their content.
    ''' </summary>
    ''' <param name="mon">The <see cref="cMSEStateMonitor">monitor</see>
    ''' that sent the event.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnInvalidated(mon As cMSEStateMonitor)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Checks whether a <see cref="eState">state</see> is met.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <returns>True if the requested state has been met.</returns>
    ''' <remarks>
    ''' <para>Checks cascade recursively, starting at the foundation state.
    ''' A state can only be met if its preceeding states are met. Each
    ''' state may involve a combination of MSE configuration checks, 
    ''' core data validations, and MSE - core compatibility checks.</para>
    ''' <para>For performance reasons the states are cached once determined.
    ''' This will lead to bugs if the state monitor is not explicitly 
    ''' cleared by outside managing code when needed by calling
    ''' <see cref="Invalidate"/>.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Function IsStateAvailable(state As eState) As Boolean

        Dim bHasState As Boolean = True

        ' Optimization: return pre-determined state if available.
        If (Me.m_statecache(state) <> TriState.UseDefault) Then
            Return (Me.m_statecache(state) = TriState.True)
        End If

        Select Case state

            Case eState.Idle
                bHasState = True

            Case eState.HasParams
                bHasState = Me.IsStateAvailable(eState.Idle) And _
                    Me.MSE.IsInputStructureAvailable() And _
                    Me.MSE.IsInputDataCompatible()

            Case eState.HasModels
                bHasState = Me.IsStateAvailable(eState.HasParams) And _
                    (Me.MSE.IsRunDataCompatible()) And _
                    (Me.MSE.NumModelsAvailable > 0)

            Case eState.HasResults
                bHasState = Me.IsStateAvailable(eState.HasModels) And _
                    Me.MSE.HasResults()

            Case eState.IsRunning
                Return (Me.MSE.RunState <> cMSE.eRunStates.Idle)

        End Select

        ' Update state cache once state has been determined.
        If bHasState Then
            Me.m_statecache(state) = TriState.True
        Else
            Me.m_statecache(state) = TriState.False
        End If

        Return bHasState

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Forget the cached states in the monitor, requiring new state assessments.
    ''' </summary>
    ''' <remarks>
    ''' In response, the monitor may broadcast an <see cref="OnInvalidated"/> event.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Sub Invalidate()

        Dim bInvalidated As Boolean = False
        For i As Integer = 0 To Me.m_statecache.Count - 1
            If (Me.m_statecache(i) <> TriState.UseDefault) Then
                Me.m_statecache(i) = TriState.UseDefault
                bInvalidated = True
            End If
        Next

        If bInvalidated Then
            Try
                RaiseEvent OnInvalidated(Me)
            Catch ex As Exception
                cLog.Write(ex, "cMSEStateMonitor::Invalidate")
            End Try
        End If

    End Sub

#End Region ' Public bits

#Region " Internals "

    Private ReadOnly Property MSE As cMSE
        Get
            Return Me.m_plugin.MSE
        End Get
    End Property

#End Region ' Internals

End Class
