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
Imports EwEUtils.Database
Imports EwEUtils.Core
Imports System.Text
Imports System.IO

#End Region ' Imports

Namespace Database

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing an importer to convert an EwE5 document
    ''' into an EwE6 database.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public MustInherit Class cModelImporter
        Implements IModelImporter

#Region " Public helper classes "

        Protected Class cExternalModelInfoSorter
            Implements IComparer(Of cExternalModelInfo)

            Public Function Compare(ByVal x As cExternalModelInfo, ByVal y As cExternalModelInfo) As Integer _
                Implements IComparer(Of cExternalModelInfo).Compare
                Return String.Compare(x.Name, y.Name)
            End Function

        End Class

#End Region ' Internal helper classes

#Region " Private vars "

        ''' <summary>Status log.</summary>
        Protected m_sbLog As New StringBuilder
        ''' <summary>Source database file name.</summary>
        Protected m_strSource As String = ""
        ''' <summary>Target database in EwE6 format.</summary>
        Protected m_dbTarget As cEwEDatabase ' Import to (write)
        ''' <summary>Name of the model to import.</summary>
        Protected m_strModelName As String = ""

        ''' <summary>The core to use when importing.</summary>
        Protected m_core As cCore = Nothing
        ''' <summary>Number of steps that the import process will take.</summary>
        Protected m_iNumSteps As Integer = 0
        ''' <summary>The current step processed by the import.</summary>
        Protected m_iStep As Integer = 0

#End Region ' Private vars

#Region " Construction "

        Public Sub New(ByVal core As cCore)
            Me.m_core = core
        End Sub

#End Region ' Construction

#Region " Overridables "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Connects the importer to an EwE5 source database. This database is
        ''' indicated as a file path, and is assumed to be an MS Access database.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Any database connection established via the Open method must be 
        ''' disconnected via the <see cref="Close">Close</see> method.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public MustOverride Function Open(ByVal strSource As String) As Boolean _
            Implements IModelImporter.Open

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Disconnects the importer from its EwE5 source document.
        ''' </summary>
        ''' <remarks>
        ''' Any database connection established via the <see cref="Open">Open</see>
        ''' method must be disconnected via the Close method.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public MustOverride Sub Close() _
            Implements IModelImporter.Close

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the importer was connected to its source document 
        ''' via the <see cref="Open">Open</see> method.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public MustOverride Function IsOpen() As Boolean _
            Implements IModelImporter.IsOpen

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform the actual import.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Import(ByVal info As cExternalModelInfo, _
                               ByVal db As cEwEDatabase, _
                               ByRef strLogfileName As String) As Boolean _
            Implements IModelImporter.Import

            Dim bSucces As Boolean = False

            Me.m_sbLog.Length = 0
            Me.m_strModelName = info.ID

            Me.LogMessage(String.Format(My.Resources.CoreMessages.IMPORT_PROGRESS_STARTED, _
                              Me.m_strModelName, Date.Now.ToString()), _
                              eMessageType.DataImport, eMessageImportance.Information, True)

            ' Set DB
            Me.m_dbTarget = db

            Me.Open(Me.m_strSource)
            Try
                bSucces = Me.PerformImport()
            Catch ex As Exception
                bSucces = False
            End Try
            Me.Close()

            ' Release DB
            Me.m_dbTarget = Nothing

            If bSucces Then
                Me.LogMessage(String.Format(My.Resources.CoreMessages.IMPORT_PROGRESS_SUCCES, _
                                            Me.m_strModelName, Date.Now.ToString()), _
                                            eMessageType.NotSet, eMessageImportance.Information, True)
            Else
                Me.LogMessage(String.Format(My.Resources.CoreMessages.IMPORT_PROGRESS_FAILED, _
                                            Me.m_strModelName, Date.Now.ToString()), _
                                            eMessageType.DataImport, eMessageImportance.Information, True)
            End If

            ' Concoct log file name
            strLogfileName = Path.Combine(Path.GetDirectoryName(db.Name), Path.GetFileNameWithoutExtension(db.Name))
            strLogfileName += "_import_log"
            strLogfileName = Path.ChangeExtension(strLogfileName, "txt")

            ' Write log to text file with the same file name as the destination db name
            cLog.WriteTextToFile(strLogfileName, Me.m_sbLog)

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a <see cref="cExternalModelInfo">descriptive list of models</see> 
        ''' that can be imported from an attached EwE5 document.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function Models() As cExternalModelInfo() _
            Implements IModelImporter.Models

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this importer can import from a given source.
        ''' </summary>
        ''' <param name="strSource">The source to explore.</param>
        ''' <returns>True if this importer can import from the given source.</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function CanImportFrom(ByVal strSource As String) As Boolean _
            Implements IModelImporter.CanImportFrom

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Actual implementation of the import process.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Protected MustOverride Function PerformImport() As Boolean

#End Region ' Overridables

#Region " Status logging "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Logs a progress message.
        ''' </summary>
        ''' <param name="strMessage">Progress message.</param>
        ''' <param name="iStep">Progress step. If -1, the internal step admin is 
        ''' automatically incremented.</param>
        ''' -------------------------------------------------------------------
        Protected Sub LogProgress(ByVal strMessage As String, Optional ByVal iStep As Integer = -1)

            Dim sProgress As Single = 0

            ' Need to auto-increment step?
            If (iStep < 0) Then
                ' #Yes: auto-increment
                Me.m_iStep += 1
            Else
                ' #No: set the step
                Me.m_iStep = iStep
            End If

            ' Calculate progress
            If (Me.m_iNumSteps <> 0) Then
                sProgress = CSng(Me.m_iStep / Me.m_iNumSteps)
            Else
                sProgress = 1.0
            End If

            ' Send to log
            Me.LogMessage(strMessage, eMessageType.DataImport, eMessageImportance.Information, False)

            ' Public to core if possible
            If (Me.m_core IsNot Nothing) Then
                ' Send progress message
                Me.m_core.Messages.SendMessage(New cProgressMessage(eProgressState.Running, 1.0!, sProgress, strMessage, eMessageType.DataImport))
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Logs a message
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Sub LogMessage(ByVal strMessage As String, _
                                 Optional ByVal msgType As eMessageType = eMessageType.DataImport, _
                                 Optional ByVal msgImportance As eMessageImportance = eMessageImportance.Information, _
                                 Optional ByVal bPublishToInterface As Boolean = False)

            ' Add message to log
            Me.m_sbLog.AppendLine(strMessage)

            ' Publicly log message
            If (bPublishToInterface = True) And (Me.m_core IsNot Nothing) Then
                Me.m_core.m_publisher.SendMessage(New cMessage(strMessage, msgType, eCoreComponentType.DataSource, msgImportance))
            End If

        End Sub

#End Region ' Status logging

    End Class

End Namespace
