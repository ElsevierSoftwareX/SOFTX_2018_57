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
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports EwEUtils.Utilities
Imports Microsoft.Win32

#End Region ' Imports

Namespace Interop

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This class implements a simple connection to R. It facilitates <see cref="cRBridge.Execute">
    ''' execution of scripts from file or as a series of lines</see>.
    ''' </summary>
    ''' <remarks>
    ''' Note that the connection to R is established by rerouting the <see cref="Process.StandardInput"/>,
    ''' <see cref="Process.StandardOutput"/>, and <see cref="Process.StandardError"/> of the
    ''' R process. This code does not use COM to remain CRL-compliant. Neat, no?
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class cRBridge

#Region " Private vars "

        ''' <summary>Input sent to R</summary>
        Private m_RInput As New List(Of String)
        ''' <summary>Output produced by R</summary>
        Private m_ROutput As New List(Of String)
        ''' <summary>Errors produced by R</summary>
        Private m_RErrors As New List(Of String)
        ''' <summary>Full path to R</summary>
        Private m_strPathToR As String = ""
        ''' <summary>Disctionary of fields to replace in the script</summary>
        Private m_dtFields As New Dictionary(Of String, String)

        ''' <summary>Script running flag.</summary>
        Private m_bIsExecuting As Boolean = False
        ''' <summary>Last run result.</summary>
        Private m_bSuccess As Boolean = False
        ''' <summary>Script running thread, if mutli-threaded.</summary>
        Private m_thread As Threading.Thread = Nothing
        ''' <summary>The script being executed.</summary>
        Private m_script As ICollection(Of String)

#End Region ' Private vars

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new R connection.
        ''' </summary>
        ''' <param name="strPathToR">The path to the R executable.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(strPathToR As String)

            ' Sanity checks
            Debug.Assert(File.Exists(strPathToR), "Cannot find R at '" & strPathToR & "'")

            ' Store path
            Me.m_strPathToR = strPathToR

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether R should run as an elevated proecss. Set this to true
        ''' if your R script needs to install or update packages.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property RunElevated As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clean up left-overs from previous R runs.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Clear()
            If (Me.IsExecuting) Then Return
            Me.m_RInput.Clear()
            Me.m_RErrors.Clear()
            Me.m_ROutput.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the R installation locations reported in the registry.
        ''' </summary>
        ''' <returns>An array with R installation locations obtained from the
        ''' registry. This method is MONO-compliant, but may not yield usable
        ''' results on Linux.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function InstallLocations() As String()
            Dim rk As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\R-core")
            Dim lstrPaths As New List(Of String)
            If (rk IsNot Nothing) Then
                For Each strName As String In rk.GetSubKeyNames
                    Dim rkRSub As RegistryKey = rk.OpenSubKey(strName)
                    If (rkRSub IsNot Nothing) Then
                        Dim objVal As Object = rkRSub.GetValue("InstallPath")
                        If (objVal IsNot Nothing) Then
                            Dim strVal As String = CStr(objVal)
                            If (Directory.Exists(strVal)) Then
                                strVal = Path.Combine(strVal, "bin\R.exe")
                                If (File.Exists(strVal) And Not lstrPaths.Contains(strVal)) Then
                                    lstrPaths.Add(strVal)
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            Return lstrPaths.ToArray
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the R bridge is currently executing a script.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IsExecuting As Boolean
            Get
                SyncLock Me
                    Return Me.m_bIsExecuting
                End SyncLock
            End Get
            Private Set(value As Boolean)
                SyncLock Me
                    Me.m_bIsExecuting = value
                End SyncLock
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event, thrown when R has completed execution.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="args"></param>
        ''' -------------------------------------------------------------------
        Public Event RunCompleted(sender As Object, args As EventArgs)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Execute an R script.
        ''' </summary>
        ''' <param name="strScriptFile">The R script file to execute.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function ExecuteFile(strScriptFile As String) As Boolean

            Dim RScriptReader As StreamReader = Nothing

            Me.Clear()

            Try
                ' Try to open the file
                RScriptReader = New StreamReader(strScriptFile)
            Catch ex As Exception
                ' Kaboom
                Me.m_RErrors.Add(ex.Message)
                If (ex.InnerException IsNot Nothing) Then
                    Me.m_RErrors.Add(ex.InnerException.Message)
                End If
                Return False
            End Try

            ' Read script lines
            Dim strScript As String = RScriptReader.ReadToEnd
            ' Do not forget to clean up
            RScriptReader.Close()

            ' Execute R
            Return Me.Execute(strScript)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Execute an R script provided as a block of text.
        ''' </summary>
        ''' <param name="strScript">The script to execute.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Execute(strScript As String) As Boolean
            Return Me.Execute(New String() {strScript})
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Execute an R script provided as a collection of strings.
        ''' </summary>
        ''' <param name="RScriptLines">The script to execute.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Execute(RScriptLines As ICollection(Of String)) As Boolean

            If (Me.IsExecuting) Then
                Me.RaiseRunCompleted()
                Return False
            End If

            SyncLock Me

                Me.m_bIsExecuting = True
                Me.m_script = RScriptLines
                Me.m_bSuccess = False
                Me.Run()
                Me.m_bIsExecuting = False

            End SyncLock
            Return Me.LastRunSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set a field to replace in the script
        ''' </summary>
        ''' <param name="strFieldName"></param>
        ''' -------------------------------------------------------------------
        Public Property Field(strFieldName As String) As String
            Get
                If (Me.m_dtFields.ContainsKey(strFieldName)) Then
                    Return Me.m_dtFields(strFieldName)
                End If
                Return String.Empty
            End Get
            Set(value As String)

                If (String.IsNullOrWhiteSpace(strFieldName)) Then Return

                If String.IsNullOrWhiteSpace(value) Then
                    If (Me.m_dtFields.ContainsKey(strFieldName)) Then
                        Me.m_dtFields.Remove(strFieldName)
                    End If
                Else
                    Me.m_dtFields(strFieldName) = value
                End If

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Time in millisecnonds to wait for an R script to terminate. 
        ''' </summary>
        ''' <remarks>To wait indefinitely set value should be set to <see cref="Int32.MaxValue"/>.</remarks>
        ''' -------------------------------------------------------------------
        Public Property TimeOut As Int32 = Int32.MaxValue

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the input lines sent to during the last R run.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Input As String()
            Get
                Return Me.m_RInput.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the output produced by the last R run.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Output As String()
            Get
                Return Me.m_ROutput.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the error lines produced by the last R run.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Errors As String()
            Get
                Return Me.m_RErrors.ToArray
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the last R script ran successfully, i.e. when the script
        ''' ran without producing errors.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property LastRunSuccess As Boolean
            Get
                Return (Me.m_RErrors.Count = 0) And Me.m_bSuccess
            End Get
        End Property

#Region " Privates "

        Private Sub Run()

            Dim Rwrapper As New Process()
            Dim bSuccess As Boolean

            Me.Clear()

            ' ----------
            ' Configure how RProcess will run
            ' ----------

            ' Execute R in the current memory space
            Rwrapper.StartInfo.UseShellExecute = False

            ' Connect to the R input, output and error streams
            Rwrapper.StartInfo.RedirectStandardInput = True
            Rwrapper.StartInfo.RedirectStandardOutput = True
            Rwrapper.StartInfo.RedirectStandardError = True

            ' Point out the R executable
            Rwrapper.StartInfo.FileName = Me.m_strPathToR
            ' Set R command line options (see https://projects.uabgrid.uab.edu/r-group/wiki/CommandLineProcessing)
            Rwrapper.StartInfo.Arguments = "--slave"

            ' Suppress R user interface
            Rwrapper.StartInfo.CreateNoWindow = True

            ' Do the elevation bit if needed
            If (Me.RunElevated) Then
                Rwrapper.StartInfo.Verb = "runas"
            End If

            ' The process is ready to run
            Try
                ' Launch R
                Rwrapper.Start()
            Catch ex As Exception
                ' Shoot! Something went wrong. Pass error information out
                Me.m_RErrors.Add(ex.Message)
                If (ex.InnerException IsNot Nothing) Then
                    Me.m_RErrors.Add(ex.InnerException.Message)
                End If
                ' Abandon ship, women and debuggers first.
                Me.m_bSuccess = False
                ' Let world know
                Me.RaiseRunCompleted()
                Return
            End Try

            ' ----------
            ' The R program has been successfully launched. Now, start feeding it with lines of script
            ' ----------

            ' Process input lines
            For Each strLine As String In Me.m_script
                ' Write each individual script line to R
                For Each strField As String In Me.m_dtFields.Keys
                    ' Just to be sure!
                    If Not String.IsNullOrWhiteSpace(strField) Then
                        strLine = strLine.Replace(strField, Me.m_dtFields(strField))
                    End If
                Next
                Me.m_RInput.Add(strLine)
                Rwrapper.StandardInput.WriteLine(strLine)
            Next
            ' Tell R that this was it; script is at an end
            Rwrapper.StandardInput.Close()

            ' Wait for R to finish with a configurable time limit.
            bSuccess = Rwrapper.WaitForExit(Me.TimeOut)

            ' Read whatever output text is available
            While (Rwrapper.StandardOutput.Peek > 0)
                Me.m_ROutput.Add(Rwrapper.StandardOutput.ReadLine)
            End While

            ' Read whatever error text is available
            While (Rwrapper.StandardError.Peek > 0)
                Me.m_RErrors.Add(Rwrapper.StandardError.ReadLine)
                ' Script contained errors
                bSuccess = False
            End While

            ' Clean up
            Rwrapper.Close()
            Rwrapper.Dispose()

            Me.RaiseRunCompleted()

        End Sub

        Private Sub RaiseRunCompleted()
            Try
                RaiseEvent RunCompleted(Me, New EventArgs())
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Privates

    End Class

End Namespace
