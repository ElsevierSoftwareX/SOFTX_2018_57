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
Imports System.Reflection
Imports System.Text
Imports EwEUtils.Core
Imports EwEUtils.Database

Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Database

    ''' =======================================================================
    ''' <summary>
    ''' Utility class to update a database across minor versions within one major version.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Friend Class cDatabaseUpdater

#Region " Private bits "

        ''' -----------------------------------------------------------------------
        ''' <summary>Helper class to sort database update plug-ins by 
        ''' <see cref="cDBUpdate.UpdateVersion">Version</see>, in
        ''' ascending order.</summary>
        ''' -----------------------------------------------------------------------
        Private Class cDBUpdatePluginContextSort
            Implements IComparer(Of cDBUpdate)

            Public Function Compare(ByVal x As cDBUpdate, ByVal y As cDBUpdate) As Integer _
                    Implements IComparer(Of cDBUpdate).Compare
                Return CInt(if(x.UpdateVersion < y.UpdateVersion, -1, 1))
            End Function

        End Class

        ''' <summary>Core to operate onto.</summary>
        Private m_core As cCore = Nothing
        ''' <summary>The baseline database version that this updater can update from</summary>
        Private m_sBaselineVersion As Single = 0.0

#End Region ' Private bits

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal core As cCore, ByVal sBaselineVersion As Single)
            ' Lemembel the cole
            Me.m_core = core
            ' Store baseline version number
            Me.m_sBaselineVersion = sBaselineVersion
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if there are updates available for a given database.
        ''' </summary>
        ''' <param name="db"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function HasUpdates(ByVal db As cEwEDatabase) As Boolean
            Return Me.HasDatabaseUpdates(db, Me.m_sBaselineVersion)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform a database update.
        ''' </summary>
        ''' <param name="db">The <see cref="cEwEDatabase">database</see> to update</param>
        ''' <returns>True if successful</returns>
        ''' -------------------------------------------------------------------
        Public Function UpdateDatabase(ByVal db As cEwEDatabase) As Boolean
            Return Me.RunAllUpdates(db)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the max supported core version of the database.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Shared Function MaxSupportedVersion() As Single
            Dim sVersion As Single = 6.0! ' Should obtain this from cEwEDatabase, but ok
            Dim upd As cDBUpdate() = cDatabaseUpdater.GetUpdates(sVersion)
            ' Has updates?
            If upd.Length > 0 Then
                ' #Yes: return version of last update (updates are sorted by version ASC)
                sVersion = upd(upd.Length - 1).UpdateVersion
            End If
            Return sVersion
        End Function

#End Region ' Updating

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns all available update objects in this assembly.
        ''' </summary>
        ''' <returns>An array of available updates.</returns>
        ''' -------------------------------------------------------------------
        Private Shared Function GetUpdates(ByVal sVersion As Single) As cDBUpdate()

            Dim lUpdates As New List(Of cDBUpdate)
            Dim clsType As Type = Nothing
            Dim clsAssembly As Assembly
            Dim upd As cDBUpdate = Nothing

            Try
                ' Get assembly that declared the database updater class
                clsAssembly = Assembly.GetAssembly(GetType(cDatabaseUpdater))
                ' For every class in this assembly
                For Each clsType In clsAssembly.GetTypes
                    ' Is cDBUpdate derived?
                    If GetType(cDBUpdate).IsAssignableFrom(clsType) Then
                        ' #Yes: Is NOT cDBUpdate itself ('cause this is an abstract class)?
                        If Not Type.Equals(clsType, GetType(cDBUpdate)) Then
                            ' #Yes: Create update instance
                            upd = DirectCast(Activator.CreateInstance(clsType, New Object() {}), cDBUpdate)
                            ' Is a valid update?
                            If (upd.UpdateVersion > sVersion) Then
                                ' #Yes: add to the list of updates
                                lUpdates.Add(upd)
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
            End Try

            ' Sort list, ascending by update number
            lUpdates.Sort(New cDBUpdatePluginContextSort())
            ' Done
            Return lUpdates.ToArray()

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether plug-ins have been found that can upgrade an
        ''' <see cref="cEwEDatabase">EwE database</see> to a newer version that
        ''' exceeds a requested <paramref name="sBaselineVersion">baseline version</paramref>.
        ''' </summary>
        ''' <param name="db">The EwE database to test for upgrades.</param>
        ''' <param name="sBaselineVersion">The baseline database version required 
        ''' by the EwE software.</param>
        ''' <returns>True if updates are available.</returns>
        ''' -----------------------------------------------------------------------
        Public Function HasDatabaseUpdates(ByVal db As cEwEDatabase, ByVal sBaselineVersion As Single) As Boolean

            ' Sanity checks
            If (db Is Nothing) Then Return False

            Dim sDBVersion As Single = db.GetVersion()
            If (sDBVersion < sBaselineVersion) Then Return False

            Return (cDatabaseUpdater.GetUpdates(sDBVersion).Length > 0)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Run available database update plug-ins.
        ''' </summary>
        ''' <param name="db">The database to update.</param>
        ''' <remarks>
        ''' This method does not attempt to cross thread boundaries.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function RunAllUpdates(ByVal db As cEwEDatabase) As Boolean

            Dim sDBVersion As Single = 0.0!
            Dim iUpdate As Integer = 0
            Dim update As cDBUpdate = Nothing
            Dim aUpdates As cDBUpdate() = Nothing
            Dim bSucces As Boolean = True
            Dim messages As New List(Of String)

            ' Sanity checks
            If (db Is Nothing) Then Return False
            If (db.IsReadOnly) Then Return True

            ' Get DB version
            sDBVersion = db.GetVersion()

            Me.ReportUpdateProgress(eProgressState.Start, "", 0)

            ' For all updates
            aUpdates = cDatabaseUpdater.GetUpdates(sDBVersion)
            While (iUpdate < aUpdates.Length) And (bSucces = True)

                ' Get update
                update = aUpdates(iUpdate)

                Me.ReportUpdateProgress(eProgressState.Running,
                                        cStringUtils.Localize(My.Resources.CoreMessages.UPDATE_RUNNING, update.UpdateVersion),
                                        CSng(iUpdate / aUpdates.Length))

                ' Able to start transaction?
                If db.BeginTransaction() Then
                    ' Do not publicly report updates that always run
                    Me.ReportUpdateError(cStringUtils.Localize(My.Resources.CoreMessages.STATUS_DATABASE_UPDATE, update.UpdateVersion, update.UpdateDescription), _
                                          eMessageImportance.Maintenance)

                    Try
                        ' #Yes: run the update
                        bSucces = update.ApplyUpdate(db)
                        ' Update ran successful?
                        If bSucces Then
                            ' #Yes: Update database version
                            db.SetVersion(update.UpdateVersion, Me.ToShortDescription(update.UpdateDescription))
                            ' Keep user actions
                            If (Not String.IsNullOrWhiteSpace(update.UserAction)) Then
                                messages.Add(update.UserAction)
                            End If
                        Else
                            ' #No: report a generic error
                            Me.ReportUpdateError(cStringUtils.Localize(My.Resources.CoreMessages.DATABASE_UPDATE_FAILED, update.UpdateVersion))
                        End If

                    Catch ex As Exception
                        ' Woops!
                        Me.ReportUpdateError(cStringUtils.Localize(My.Resources.CoreMessages.DATABASE_UPDATE_FAILED_DETAIL, update.UpdateVersion, ex.Message))
                        bSucces = False
                    End Try

                    ' Update ran succesfully?
                    If bSucces Then
                        ' #Yes: commit changes
                        bSucces = db.CommitTransaction(True)
                    Else
                        ' #No: rollback changes
                        db.RollbackTransaction()
                    End If

                Else
                    ' #No: failed to start transaction - an update did not clean up well
                    Debug.Assert(False, "Database version " & sDBVersion & " update sequence failed for update " & update.UpdateVersion)
                    bSucces = False
                End If

                ' Next
                iUpdate += 1
            End While

            Me.ReportUpdateProgress(eProgressState.Finished, "", 100)

            If (bSucces) Then
                If (messages.Count > 0) Then
                    Dim msg As New cMessage(My.Resources.CoreMessages.UPDATE_NEED_REVIEW, eMessageType.DataImport, eCoreComponentType.DataSource, eMessageImportance.Warning)
                    For i As Integer = 0 To messages.Count - 1
                        Dim vs As New cVariableStatus(eStatusFlags.InvalidModelResult, messages(0), eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                        msg.AddVariable(vs)
                    Next
                    Me.m_core.Messages.SendMessage(msg)
                End If
            End If
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a database update description into a short description.
        ''' </summary>
        ''' <param name="strDescription"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function ToShortDescription(ByVal strDescription As String) As String

            Dim sbDescription As New StringBuilder()
            Dim strBit As String = ""
            Dim iBit As Integer = 0

            For Each strBit In strDescription.Split(New String() {"." & Environment.NewLine, Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                strBit = strBit.Trim
                If Not String.IsNullOrEmpty(strBit) Then
                    If iBit > 0 Then sbDescription.Append("; ")
                    sbDescription.Append(strBit)
                    iBit += 1
                End If
            Next
            Return sbDescription.ToString()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Report an update error to the core.
        ''' </summary>
        ''' <param name="strStatus">Message text.</param>
        ''' <param name="importance">Message importance.</param>
        ''' <remarks></remarks>
        ''' -------------------------------------------------------------------
        Private Sub ReportUpdateError(ByVal strStatus As String,
                                      Optional ByVal importance As eMessageImportance = eMessageImportance.Critical)

            Dim msg As cMessage = New cMessage(strStatus, eMessageType.DataImport, eCoreComponentType.DataSource, importance)
            Try
                Me.m_core.Messages.SendMessage(msg)
                cLog.Write("Database update failure: " & strStatus)
            Catch ex As Exception
                cLog.Write(ex, "cDatabaseUpdate.ReportUpdateError(" & strStatus & ")")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Report update status to the core.
        ''' </summary>
        ''' <param name="strStatus">Status message.</param>
        ''' <param name="sProgress">Progress indicator.</param>
        ''' -------------------------------------------------------------------
        Private Sub ReportUpdateProgress(ByVal status As eProgressState,
                                         ByVal strStatus As String,
                                         ByVal sProgress As Single)

            Dim msg As cMessage = New cProgressMessage(status, 1, sProgress, strStatus, eMessageType.Progress)

            Try
                Me.m_core.Messages.SendMessage(msg)
            Catch ex As Exception
                cLog.Write(ex, "cDatabaseUpdate.ReportUpdateProgress")
            End Try

        End Sub

#End Region ' Internals

    End Class

End Namespace
