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
Imports EwECore.Shapes.Utility
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwECore

#End Region ' Imports

Namespace Controls

    Public Class dlgImportShapes

        Private m_uic As cUIContext = Nothing
        Private m_data As cShapeImportData = Nothing
        Private m_manager As cBaseShapeManager = Nothing

        Public Sub New(uic As cUIContext, manager As cBaseShapeManager)

            Me.m_uic = uic
            Me.m_manager = manager

            Me.m_data = New cShapeImportData(Me.m_uic.Core)

            Me.InitializeComponent()

        End Sub

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_grid.UIContext = Me.m_uic

            Me.m_pbHelp.Image = SharedResources.Info

            Me.CenterToScreen()
            Me.UpdateControls()

        End Sub

#Region " Events "

        Private Sub OnImportBrowseFile(sender As System.Object, e As System.EventArgs) _
            Handles m_btnImportBrowse.Click

            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

            cmdFO.Invoke(Me.m_tbImportFileName.Text, SharedResources.FILEFILTER_CSV & "|" & SharedResources.FILEFILTER_TEXT)

            If (cmdFO.Result = DialogResult.OK) Then
                Me.m_tbImportFileName.Text = cmdFO.FileName
                Me.Read()
            End If

        End Sub

        Private Sub OnFileNameTextChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tbImportFileName.TextChanged
            ' NOP
        End Sub

        Private Sub OnDelimiterChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tbImportDelimiter.TextChanged
            Me.m_data.Delimiter = Me.m_tbImportDelimiter.Character
            Me.Read()
        End Sub

        Private Sub OnSeparatorChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tbImportSeparator.TextChanged
            Me.m_data.DecimalSeparator = Me.m_tbImportSeparator.Character
            Me.Read()
        End Sub

        Private Sub OnHelp(sender As System.Object, e As System.EventArgs) _
            Handles m_pbHelp.Click
            Me.CheatSheet()
        End Sub

        Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
            Handles m_btnOk.Click

            Dim importer As New cShapeImporter(Me.m_uic.Core, Me.m_data)
            Dim bSuccess As Boolean = True

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, "", -1)
            Try
                bSuccess = importer.Import(Me.m_manager, Me.m_manager.NPoints)
            Catch ex As Exception

            End Try
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            If (bSuccess = True) Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If

        End Sub

        Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
            Handles m_btnCancel.Click

            Me.DialogResult = DialogResult.Cancel
            Me.Close()

        End Sub

#End Region ' Events

#Region " Drag and drop "

        Protected Overrides Sub OnDragOver(e As DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim astrFiles() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
                If astrFiles.Length = 1 Then
                    e.Effect = DragDropEffects.All
                End If
            End If
            MyBase.OnDragOver(e)
        End Sub

        Protected Overrides Sub OnDragDrop(e As DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Try
                    Dim astrFiles() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
                    If astrFiles.Length = 1 Then
                        Me.m_tbImportFileName.Text = astrFiles(0)
                        Me.Read()
                    End If
                Catch ex As Exception

                End Try
            End If
            MyBase.OnDragDrop(e)
        End Sub

#End Region ' Drag and drop

#Region " Internals "

        Private Function CheatSheet() As Boolean

            ' ToDo: globalize this

            Dim strPath As String = Path.Combine(Me.m_uic.Core.OutputPath, "Examples")
            Dim strFile As String = Path.Combine(strPath, "Shape import cheat sheet.csv")
            Dim fmt As New cShapeFunctionTypeFormatter()
            Dim wr As StreamWriter = Nothing

            If (Not cFileUtils.IsDirectoryAvailable(strPath, True)) Then
                ' ToDo: Alert user
                Return False
            End If

            Try
                wr = New StreamWriter(strFile, False)
            Catch ex As Exception
                ' ToDo: Alert user
                Return False
            End Try

            If Me.m_uic.Core.SaveWithFileHeader Then
                wr.WriteLine(Me.m_uic.Core.DefaultFileHeader(eAutosaveTypes.NotSet))
                wr.WriteLine()
            End If

            wr.WriteLine(cStringUtils.ToCSVField("CSV file format to import shapes:"))
            wr.WriteLine()
            wr.WriteLine("""Function name"",""Function type"",""Param 1"",""Param 2"",""Param 3"",""Param 4"",""Param 5""")
            wr.WriteLine("<text>,""<number>"",<value>,<value>,<value>,<value>,<value>")
            wr.WriteLine()
            wr.WriteLine(cStringUtils.ToCSVField("Where available function types and parameters are:"))
            wr.WriteLine()
            wr.WriteLine("""Function type"",Type,""Param 1"",""Param 2"",""Param 3"",""Param 4"",""Param 5""")

            For Each f As IShapeFunction In Me.m_data.ShapeFunctions
                wr.Write(cStringUtils.ToCSVField(f.ShapeFunctionType) & ", " & cStringUtils.ToCSVField(fmt.GetDescriptor(f.ShapeFunctionType)))
                For i As Integer = 1 To f.nParameters
                    wr.Write("," & cStringUtils.ToCSVField(f.ParamName(i)))
                Next
                For i As Integer = f.nParameters + 1 To 5
                    wr.Write(",")
                Next
                wr.WriteLine()
            Next

            wr.Flush()
            wr.Close()

            Try
                Dim cmd As cBrowserCommand = CType(Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                cmd.Invoke("file://" & strFile)
            Catch ex As Exception
                ' ToDo: Alert user
                Return False
            End Try

            Return True

        End Function

        Private Sub UpdateControls()

            Dim bCanImport As Boolean = (Me.m_grid.RowsCount > 1)
            Me.m_btnOk.Enabled = bCanImport

        End Sub

        Private Sub Read()

            Dim strFile As String = Me.m_tbImportFileName.Text
            Me.m_data.Clear()

            If (String.IsNullOrWhiteSpace(strFile)) Then Return

            If Not File.Exists(strFile) Then
                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.FILE_LOAD_ERROR_MISSING, strFile), _
                                        eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                Me.m_uic.Core.Messages.SendMessage(msg)
            Else
                Try
                    Using reader As New StreamReader(strFile)
                        Me.m_data.Read(reader)
                    End Using
                Catch ex As Exception
                    Dim msg As New cMessage(cStringUtils.Localize(My.Resources.FILE_LOAD_ERROR_DETAIL, strFile, ex.Message), _
                                            eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                    Me.m_uic.Core.Messages.SendMessage(msg)
                End Try
            End If

            Me.m_grid.Functions = Me.m_data.FunctionDefinitions(Me.m_manager.DataType)

            Me.UpdateControls()

        End Sub

#End Region ' Internals

    End Class

End Namespace
