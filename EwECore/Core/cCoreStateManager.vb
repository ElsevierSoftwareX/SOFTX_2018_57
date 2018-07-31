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

Option Strict On
Imports EwEUtils.Core

''' <summary>
''' Manager class used to bring the core execution state up to date.
''' </summary>
''' <remarks></remarks>
Public Class cCoreStateManager

#Region " Private data "

    Private m_core As cCore

#End Region ' Private data

#Region " Construction "

    Friend Sub New(ByRef theCore As cCore)
        m_core = theCore
    End Sub

#End Region ' Construction

#Region " Public methods "

    ''' <summary>
    ''' Bring the core state up to the requested execution state
    ''' </summary>
    ''' <param name="ExecutionState">State to bring the core up to</param>
    ''' <returns>True if successful. False otherwise.</returns>
    Public Function LoadState(ByVal ExecutionState As eCoreExecutionState) As Boolean
        Try
            Dim sm As cCoreStateMonitor = Me.m_core.StateMonitor

            'Try to bring to core up to the requested execution state
            Select Case ExecutionState

                Case eCoreExecutionState.EcopathCompleted
                    If Not sm.HasEcopathLoaded Then Return False
                    If sm.HasEcopathRan Then Return True
                    Return m_core.RunEcoPath()

                Case eCoreExecutionState.EcosimInitialized
                    If Not sm.HasEcosimLoaded Then Return False
                    If sm.HasEcosimInitialized Then Return True
                    If m_core.m_EcoSim.Init(False) Then
                        sm.SetEcoSimInitialized()
                        Return True
                    End If
                    Return False

                Case eCoreExecutionState.EcosimCompleted
                    If Not sm.HasEcosimLoaded Then Return False
                    If sm.HasEcosimRan Then Return True
                    Return m_core.RunEcoSim()

                Case Else
                    ' Not implemented (yet)
            End Select

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".LoadState(" & ExecutionState.ToString & ") Error: " & ex.Message)
        End Try

        Return False

    End Function

#End Region ' Public methods

#Region " Core methods "

    ''' <summary>
    ''' Copy the Ecopath dietcomp matrix (DC(ngroup,ngroups)) into the Ecosim dietcomp matrix (SimDC(ngroups,ngroups))
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>This is not really an Execution State thing... but hey it has to go somewhere...</remarks>
    Friend Function updateDietComp() As Boolean

        Try

            'Only load the dietcomp into ecosim if it is loaded
            If Not m_core.StateMonitor.HasEcosimLoaded Then
                Return False
            End If

            'this will copy diet comp into Ecosim SimDC()
            m_core.m_EcoSim.RemoveImportFromEcosim()

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".DietComp() Failed to copy DietComp into Ecosim." & ex.Message)
            Return False
        End Try

        Return True

    End Function

#End Region ' Core methods

End Class
