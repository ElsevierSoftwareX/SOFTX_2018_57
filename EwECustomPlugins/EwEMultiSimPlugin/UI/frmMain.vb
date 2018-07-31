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

Imports EwECore.Ecosim
Imports ScientificInterfaceShared.Style
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports System.IO
Imports System.Windows.Forms
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Main and only interface for this plug-in.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class frmMain

#Region " Private vars "

    ''' <summary>The engine that does all the work.</summary>
    Private m_engine As cEngine = Nothing

#End Region ' Private vars

#Region " Form overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        For Each out As cEcosimResultWriter.eResultTypes In [Enum].GetValues(GetType(cEcosimResultWriter.eResultTypes))
            Me.m_clbValues.Items.Add(out)
        Next

        Dim strPathIn As String = My.Settings.PathIn
        If (String.IsNullOrWhiteSpace(strPathIn)) Then
            strPathIn = Path.GetDirectoryName(Me.Core.DataSource.Directory)
        End If
        Me.m_tbxSource.Text = strPathIn

        Dim strPathOut As String = My.Settings.PathOut
        If (String.IsNullOrWhiteSpace(strPathOut)) Then
            strPathOut = Me.Core.DefaultOutputPath(eAutosaveTypes.EcosimResults, "MultiSim")
        End If
        Me.m_tbxDest.Text = strPathOut
        Me.m_cbCreateRunFolder.Checked = My.Settings.CreateUniqueRunFolder

        Me.m_cbFF.Checked = (My.Settings.FFtypes And cEngine.eFunctionTypes.Forcing) = cEngine.eFunctionTypes.Forcing
        Me.m_cbEffort.Checked = (My.Settings.FFtypes And cEngine.eFunctionTypes.Effort) = cEngine.eFunctionTypes.Effort
        Me.m_cbMort.Checked = (My.Settings.FFtypes And cEngine.eFunctionTypes.Mortality) = cEngine.eFunctionTypes.Mortality
        Me.m_cbEggProduction.Checked = (My.Settings.FFtypes And cEngine.eFunctionTypes.Eggsies) = cEngine.eFunctionTypes.Eggsies

        If My.Settings.ReadAsMonth Then
            Me.m_rbMonthly.Checked = True
        Else
            Me.m_rbAnnual.Checked = True
        End If

        Me.m_engine = New cEngine(Me.UIContext)

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
        ' Hmm
        Me.StoreSettings()
        ' Done
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Dim bHasFiles As Boolean = (Me.m_clbFilesSrc.CheckedItems.Count > 0)
        Dim bHasVars As Boolean = (Me.m_clbValues.CheckedItems.Count > 0)
        Dim bHasOutput As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxDest.Text)
        Dim bIsRunning As Boolean = Me.Core.StateMonitor.IsBusy()

        Me.m_scMain.Enabled = Not bIsRunning
        Me.m_btnRun.Enabled = bHasFiles And bHasOutput And bHasVars And Not bIsRunning

    End Sub

#End Region ' Form overrides

#Region " Event handlers "


    Private Sub OnBrowseIn(sender As System.Object, e As System.EventArgs) _
        Handles m_btnChooseSrc.Click

        Try
            Me.BrowseToTextbox(Me.m_tbxSource, "Select source folder with CSV files")
        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnBrowseIn")
        End Try

    End Sub

    Private Sub OnSourceFolderChanged(sender As Object, e As System.EventArgs) _
        Handles m_tbxSource.TextChanged

        Try
            Me.UpdateSourceFiles()
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnAllSrc(sender As System.Object, e As System.EventArgs) _
        Handles m_btnAllSrc.Click

        Try
            Me.SetAllOptionsChecked(Me.m_clbFilesSrc)
            Me.UpdateControls()
        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnAllScr")
        End Try

    End Sub

    Private Sub OnBrowseOut(sender As System.Object, e As System.EventArgs) _
        Handles m_btnChooseOut.Click

        Try
            Me.BrowseToTextbox(Me.m_tbxDest, "Select destination folder")
            Me.UpdateControls()
        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnBrowseOut")
        End Try

    End Sub

    Private Sub OnAllVars(sender As System.Object, e As System.EventArgs) _
        Handles m_btnAllVars.Click

        Try
            Me.SetAllOptionsChecked(Me.m_clbValues)
            Me.UpdateControls()
        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnAllVars")
        End Try

    End Sub

    ''' <summary>
    ''' Display file without path.
    ''' </summary>
    Private Sub OnFormatFile(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_clbFilesSrc.Format

        e.Value = Path.GetFileName(CStr(e.ListItem))

    End Sub

    Private Sub OnFormatVariable(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_clbValues.Format

        ' Provide text for ecosim run type
        Dim fmt As New cEcosimResultTypeFormatter()
        e.Value = fmt.GetDescriptor(e.ListItem)

    End Sub

    Private Sub OnItemChecking(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
        Handles m_clbValues.ItemCheck, m_clbFilesSrc.ItemCheck
        ' Call UpdateControls after check has been handled
        Me.BeginInvoke(New MethodInvoker(AddressOf UpdateControls), Nothing)
    End Sub

    Private Sub OnGenerateSample(sender As System.Object, e As System.EventArgs) _
        Handles m_btnGenerateSample.Click

        Try
            Dim strFile As String = ""
            Dim cmd As cBrowserCommand = Nothing

            If (Me.m_engine.GenerateSample(Me.SelectedApplications, Me.m_rbMonthly.Checked, strFile)) Then
                cmd = DirectCast(Me.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                cmd.Invoke(strFile)
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnValidateFiles(sender As System.Object, e As System.EventArgs) _
        Handles m_btnValidate.Click

        ' Premature event work-around
        If (Me.m_engine Is Nothing) Then Return
        If (Me.m_engine.IsRunning) Then Return

        Try
            Dim lFiles As New List(Of String)
            For Each item As Object In Me.m_clbFilesSrc.CheckedItems
                lFiles.Add(CStr(item))
            Next

            Me.m_engine.ValidateFiles(New cEngine.RunCompletedDelegate(AddressOf RunDoneCallback), _
                                      New cEngine.DisableFileDelegate(AddressOf DisableFileCallback), _
                                      lFiles.ToArray(), _
                                      Me.m_tbxDest.Text, _
                                      Me.SelectedApplications)

        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnValidateFiles")
        End Try

    End Sub

    Private Sub OnRun(sender As System.Object, e As System.EventArgs) _
        Handles m_btnRun.Click

        ' Premature event work-around
        If (Me.m_engine Is Nothing) Then Return
        If (Me.m_engine.IsRunning) Then Return

        Me.StoreSettings()
        Me.UpdateControls()

        Try
            Dim lFiles As New List(Of String)
            For Each item As Object In Me.m_clbFilesSrc.CheckedItems
                lFiles.Add(CStr(item))
            Next

            Dim lOptions As New List(Of cEcosimResultWriter.eResultTypes)
            For Each item As Object In Me.m_clbValues.CheckedItems
                lOptions.Add(DirectCast(item, cEcosimResultWriter.eResultTypes))
            Next

            Me.m_engine.Run(New cEngine.RunProgressDelegate(AddressOf RunProgressCallback), _
                            New cEngine.RunCompletedDelegate(AddressOf RunDoneCallback), _
                            lFiles.ToArray(), Me.m_tbxDest.Text, Me.SelectedApplications, _
                            Me.m_rbMonthly.Checked, lOptions.ToArray())

        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnRun")
        End Try

    End Sub

    Private Sub OnFFTypeChecked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbEffort.CheckedChanged, m_cbMort.CheckedChanged, m_cbEggProduction.CheckedChanged

        ' Premature event work-around
        If (Me.m_engine Is Nothing) Then Return
        If (Me.m_engine.IsRunning) Then Return

        Try
            My.Settings.FFtypes = Me.SelectedApplications
        Catch ex As Exception
            ' Whoah
            cLog.Write(ex, "OnFFTypeChecked")
        End Try

    End Sub

    Private Sub OnDFOclicked(sender As Object, e As System.EventArgs) Handles m_pbLogoDFO.Click
        Me.VisitSponsor("http://www.dfo-mpo.gc.ca")
    End Sub

    Private Sub OnBSCclicked(sender As Object, e As System.EventArgs) Handles m_pbLogoSU.Click
        Me.VisitSponsor("http://www.su.se/ostersjocentrum/")
    End Sub

#End Region ' Event handlers

#Region " Drag and drop "

    Private Function GetDragDropFiles(data As IDataObject) As String()
        Dim lstr As New List(Of String)
        If data.GetDataPresent(DataFormats.FileDrop) Then
            For Each str As String In CType(data.GetData(DataFormats.FileDrop), String())
                If File.Exists(str) Then
                    If (String.Compare(Path.GetExtension(str), ".csv", True) = 0) Then
                        lstr.Add(str)
                    End If
                End If
            Next
        End If
        Return lstr.ToArray
    End Function

    Private Function GetDragDropFolder(data As IDataObject) As String
        If data.GetDataPresent(DataFormats.FileDrop) Then
            Dim astrData As String() = CType(data.GetData(DataFormats.FileDrop), String())
            If (astrData.Length = 1) Then
                If Directory.Exists(astrData(0)) Then
                    Return astrData(0)
                End If
            End If
        End If
        Return ""
    End Function

    Protected Overrides Sub OnDragOver(e As System.Windows.Forms.DragEventArgs)
        If (Me.GetDragDropFiles(e.Data).Length > 0) Or Not String.IsNullOrWhiteSpace(GetDragDropFolder(e.Data)) Then
            e.Effect = DragDropEffects.All
        End If
        MyBase.OnDragOver(e)
    End Sub

    Protected Overrides Sub OnDragDrop(e As System.Windows.Forms.DragEventArgs)
        Dim astrFiles As String() = Me.GetDragDropFiles(e.Data)
        Dim strFolder As String = GetDragDropFolder(e.Data)

        If (astrFiles.Length > 0) Then
            Me.m_tbxSource.Text = ""
            Me.m_clbFilesSrc.Items.Clear()
            For Each strFile In astrFiles
                Me.m_clbFilesSrc.Items.Add(strFile)
            Next
            Me.SetAllOptionsChecked(Me.m_clbFilesSrc)
        ElseIf Not String.IsNullOrWhiteSpace(strFolder) Then
            Me.m_tbxSource.Text = strFolder
            Me.SetAllOptionsChecked(Me.m_clbFilesSrc)
        End If
        MyBase.OnDragDrop(e)
    End Sub

#End Region ' Drag and drop

#Region " Callbacks "

    Private Delegate Sub DisableFileMarshall(strFile As String)

    Private Sub DisableFileCallback(strFile As String)
        If Me.InvokeRequired Then
            Me.Invoke(New DisableFileMarshall(AddressOf DisableFileCallback), New Object() {strFile})
        Else
            Me.DisableFile(strFile)
        End If
    End Sub

    Private Delegate Sub RunProgressMarshall(strMessage As String)

    Private Sub RunProgressCallback(strMessage As String)
        If Me.InvokeRequired Then
            Me.Invoke(New RunProgressMarshall(AddressOf RunProgressCallback), New Object() {strMessage})
        Else
            Try
                Dim msg As New cMessage(strMessage, eMessageType.Any, eCoreComponentType.External, eMessageImportance.Information)
                Me.UIContext.Core.Messages.SendMessage(msg)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Delegate Sub RunDoneMarshall()

    Private Sub RunDoneCallback()
        If Me.InvokeRequired Then
            Me.Invoke(New RunDoneMarshall(AddressOf RunDoneCallback))
        Else
            Me.UpdateControls()
        End If
    End Sub

#End Region ' Callbacks

#Region " Internals "

    Private Property SelectedApplications() As cEngine.eFunctionTypes
        Get
            Dim appl As cEngine.eFunctionTypes = cEngine.eFunctionTypes.Forcing
            If (Me.UIContext IsNot Nothing) Then
                If Me.m_cbEffort.Checked Then appl = appl Or cEngine.eFunctionTypes.Effort
                If Me.m_cbMort.Checked Then appl = appl Or cEngine.eFunctionTypes.Mortality
                If Me.m_cbEggProduction.Checked Then appl = appl Or cEngine.eFunctionTypes.Eggsies
            End If
            Return appl
        End Get
        Set(ByVal value As cEngine.eFunctionTypes)

        End Set
    End Property

    Private Sub StoreSettings()

        Try
            My.Settings.PathIn = Me.m_tbxSource.Text
            My.Settings.PathOut = Me.m_tbxDest.Text
            My.Settings.ReadAsMonth = Me.m_rbMonthly.Checked
            My.Settings.CreateUniqueRunFolder = Me.m_cbCreateRunFolder.Checked
            My.Settings.FFtypes = Me.SelectedApplications
            My.Settings.Save()
        Catch ex As Exception
            cLog.Write(ex, "cMultiSim::frmMain.StoreSettings")
        End Try

    End Sub

    Private Sub UpdateSourceFiles()

        Me.m_clbFilesSrc.Items.Clear()
        If Not Directory.Exists(Me.m_tbxSource.Text) Then Return

        For Each strFile As String In Directory.GetFiles(Me.m_tbxSource.Text, "*.csv")
            Me.m_clbFilesSrc.Items.Add(strFile)
        Next
        Me.m_clbFilesSrc.Sorted = True

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set all items in a CheckedListBox to 'Checked'
    ''' </summary>
    ''' <param name="clb"></param>
    ''' -----------------------------------------------------------------------
    Private Sub SetAllOptionsChecked(ByVal clb As CheckedListBox)
        For i As Integer = 0 To clb.Items.Count - 1
            clb.SetItemChecked(i, True)
        Next
    End Sub

    Private Sub BrowseToTextbox(ByVal tbx As TextBox, ByVal strPrompt As String)
        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmdFO As cDirectoryOpenCommand = DirectCast(cmdh.GetCommand(cDirectoryOpenCommand.COMMAND_NAME), cDirectoryOpenCommand)
        cmdFO.Invoke(tbx.Text, strPrompt)
        If cmdFO.Result = System.Windows.Forms.DialogResult.OK Then
            tbx.Text = cmdFO.Directory
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Disable a file in the UI
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub DisableFile(strFile As String)
        ' Remove file from UI
        Me.m_clbFilesSrc.Items.Remove(strFile)
    End Sub

    Private Sub VisitSponsor(strURL As String)
        Try
            Dim cmd As cBrowserCommand = CType(Me.UIContext.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            cmd.Invoke(strURL)
        Catch ex As Exception
            cLog.Write(ex, "MultiSim.frmMain::VisitSponsor")
        End Try
    End Sub
#End Region ' Internals

End Class