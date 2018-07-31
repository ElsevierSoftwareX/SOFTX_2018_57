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
Imports System.IO
Imports EwECore.SpatialData
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities
Imports EwECore
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Ecospace.Controls

    Public Class dlgExportSpatialData

#Region " Private variables "

        Private Enum eSelectionMode As Integer
            None = 0
            Used
            All
        End Enum

        Private m_uic As cUIContext = Nothing
        Private m_manConn As cSpatialDataConnectionManager = Nothing
        Private m_manSets As cSpatialDataSetManager = Nothing
        Private m_htUsed As New HashSet(Of ISpatialDataSet)
        Private m_strPath As String = ""

#End Region ' Private variables

#Region " Constructor "

        Public Sub New(uic As cUIContext)
            Me.InitializeComponent()
            Me.m_uic = uic

            Me.m_manConn = Me.m_uic.Core.SpatialDataConnectionManager
            Me.m_manSets = Me.m_manConn.DatasetManager
            Me.m_strPath = Me.m_uic.Core.DefaultOutputPath(eAutosaveTypes.Ecospace)

        End Sub

#End Region ' Constructor

#Region " Form overloads "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Nice!
            Me.CenterToParent()

            If (Me.m_uic Is Nothing) Then Return

            ' Populate dataset box
            For Each ds As ISpatialDataSet In Me.m_manSets
                Me.m_clbDatsets.Items.Add(ds)
            Next

            If (Me.m_uic.Core.StateMonitor.HasEcospaceLoaded) Then
                ' Make snapshot of all used adapters
                For Each adt As cSpatialDataAdapter In Me.m_manConn.Adapters
                    For Each conn As cSpatialDataConnection In adt.Connections()
                        Dim ds As ISpatialDataSet = conn.Dataset
                        If (ds IsNot Nothing) Then
                            If (Not Me.m_htUsed.Contains(ds)) Then
                                Me.m_htUsed.Add(ds)
                            End If
                        End If
                    Next
                Next
            End If

            ' Start with a defaults
            Me.m_tbxName.Text = cFileUtils.ToValidFileName(Me.m_uic.Core.EwEModel.Name, False)
            Me.m_tbxAuthor.Text = Me.m_manSets.DataAuthor
            Me.m_tbxContact.Text = Me.m_manSets.DataContact
            Me.m_tbxDescription.Text = Me.m_manSets.DataDescription

            ' Shabang
            Me.SelectDatasets(eSelectionMode.Used)

        End Sub

        Protected Overrides Sub OnSizeChanged(e As System.EventArgs)
            MyBase.OnSizeChanged(e)
            Me.UpdateControls()
        End Sub

#End Region ' Form overloads

#Region " Events "

        Private Sub OnChooseFolder(sender As System.Object, e As System.EventArgs) _
            Handles m_btnChoose.Click

            Dim cmd As cDirectoryOpenCommand = CType(Me.m_uic.CommandHandler.GetCommand(cDirectoryOpenCommand.COMMAND_NAME), cDirectoryOpenCommand)
            cmd.Directory = Me.m_strPath
            cmd.Invoke()

            If (cmd.Result <> System.Windows.Forms.DialogResult.OK) Then Return

            'If Not cFileUtils.IsDirectoryEmpty(cmd.Directory) Then
            '    Dim msg As New cFeedbackMessage("The selected folder is not empty. Are you sure you want to use it?", _
            '                                     eCoreComponentType.External, eMessageType.DataExport, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            '    msg.Reply = eMessageReply.NO
            '    Me.m_uic.Core.Messages.SendMessage(msg)

            '    If (msg.Reply = eMessageReply.NO) Then Return
            'End If

            Me.m_strPath = cmd.Directory
            Me.UpdateControls()

        End Sub

        Private Sub OnTargetNameChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tbxName.TextChanged, m_tbxDescription.TextChanged, m_tbxContact.TextChanged, m_tbxAuthor.TextChanged
            Me.UpdateControls()
        End Sub

        Private Sub OnSelectUsed(sender As System.Object, e As System.EventArgs) _
            Handles m_btnUsed.Click
            Me.SelectDatasets(eSelectionMode.Used)
        End Sub

        Private Sub OnSelectAll(sender As System.Object, e As System.EventArgs) _
            Handles m_btnAll.Click
            Me.SelectDatasets(eSelectionMode.All)
        End Sub

        Private Sub OnSelectNone(sender As System.Object, e As System.EventArgs) _
            Handles m_btnNone.Click
            Me.SelectDatasets(eSelectionMode.None)
        End Sub

        Private Sub OnExport(sender As System.Object, e As System.EventArgs) _
            Handles m_btnExport.Click

            Dim sm As cCoreStateMonitor = Me.m_uic.Core.StateMonitor
            Dim bSuccess As Boolean = False

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, "Exporting data...", -1)
            Try
                bSuccess = Me.m_manSets.Save(Me.OutputLocation(), _
                                             Me.SelectedDatasets(), Me.m_tbxDescription.Text, _
                                             Me.m_tbxAuthor.Text, Me.m_tbxContact.Text, _
                                             Me.m_cbIncludeData.Checked)
            Catch ex As Exception

            End Try
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            If (bSuccess) Then
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()

                ' ToDo: globalize this
                Dim msg As New cMessage("Spatial data configuration has been exported to " & Me.OutputLocation, _
                                        eMessageType.DataExport, eCoreComponentType.DataSource, eMessageImportance.Information)
                Me.m_uic.Core.Messages.SendMessage(msg)
            End If

        End Sub

        Private Sub OnCancel(sender As System.Object, e As System.EventArgs) Handles m_btnCancel.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub OnFormatDataset(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_clbDatsets.Format

            Dim fmt As New cSpatialDatasetFormatter()
            e.Value = fmt.GetDescriptor(e.ListItem, eDescriptorTypes.Abbreviation)

        End Sub

        Private Sub OnItemCheckStateChanged(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
            Handles m_clbDatsets.ItemCheck

            ' Problem with CheckedListBox: event is thrown before event is processed.
            ' Therefore, perform a delayed response:
            Me.BeginInvoke(New MethodInvoker(AddressOf UpdateControls))

        End Sub

#End Region ' Events

#Region " Internals "

        Private Function OutputLocation() As String
            Dim strSelection As String = cFileUtils.ToValidFileName(Me.m_tbxName.Text, True)
            Dim strFile As String = Path.GetFileName(strSelection)
            Dim strPath As String = Path.Combine(Me.m_strPath, strSelection)
            Return Path.Combine(strPath, Path.ChangeExtension(strFile, ".xml"))
        End Function

        Private Sub SelectDatasets(ByVal mode As eSelectionMode)

            For i As Integer = 0 To Me.m_clbDatsets.Items.Count - 1

                Dim bCheck As Boolean = False

                Select Case mode
                    Case eSelectionMode.All
                        bCheck = True
                    Case eSelectionMode.None
                        bCheck = False
                    Case eSelectionMode.Used
                        Dim ds As ISpatialDataSet = CType(Me.m_clbDatsets.Items(i), ISpatialDataSet)
                        bCheck = Me.m_htUsed.Contains(ds)
                End Select

                Me.m_clbDatsets.SetItemChecked(i, bCheck)
            Next
            'Me.UpdateControls()

        End Sub

        Private Function SelectedDatasets() As ISpatialDataSet()

            Dim lds As New List(Of ISpatialDataSet)
            Try
                For i As Integer = 0 To Me.m_clbDatsets.Items.Count - 1
                    If (Me.m_clbDatsets.GetItemChecked(i)) Then
                        lds.Add(DirectCast(Me.m_clbDatsets.Items(i), ISpatialDataSet))
                    End If
                Next
            Catch ex As Exception

            End Try
            Return lds.ToArray

        End Function

        Private Sub UpdateControls()

            If (Me.m_uic Is Nothing) Then Return

            Dim bHasTarget As Boolean = Not String.IsNullOrWhiteSpace(cFileUtils.ToValidFileName(Me.m_tbxName.Text, False))
            Dim bHasSelection As Boolean = (Me.m_clbDatsets.CheckedIndices.Count > 0)

            Dim strPathOrg As String = Path.GetDirectoryName(Me.OutputLocation())
            Dim strPathFit As String = cStringUtils.CompactString(Path.GetDirectoryName(Me.OutputLocation()), _
                                                                  Me.m_lblFolderPreview.ClientSize.Width, _
                                                                  Me.m_lblFolderPreview.Font)
            Me.m_lblFolderPreview.Text = strPathFit
            cToolTipShared.GetInstance().SetToolTip(Me.m_lblFolderPreview, strPathOrg)


            Me.m_btnExport.Enabled = bHasTarget And bHasSelection

        End Sub

#End Region ' Internals

    End Class

End Namespace
