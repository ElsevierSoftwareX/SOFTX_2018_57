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
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Public Class ucFlowDiagram
    Inherits UserControl
    Implements IUIElement

#Region " Private bits "

    ''' <summary>Instance of the Ecost model to poke and prod.</summary>
    Private m_model As cModel = Nothing
    ''' <summary>Instance of model results to reflect.</summary>
    Private m_result As cResults = Nothing
    ''' <summary>UI context to operate on.</summary>
    Private m_uic As cUIContext = Nothing
    Private m_data As cFlowDiagramData = Nothing

    Private m_doodler As cFlowDiagramManager = Nothing
    Private m_tree As cTreeFlowDiagramRenderer = Nothing

    'Private m_hovermenu As ucHoverMenu = Nothing

    'Private Enum eFDCommands As Integer
    '    ExportPicture
    'End Enum

    Private Class cGraphDataItem
        Private m_gdt As cResults.eGraphDataType
        Public Sub New(gdt As cResults.eGraphDataType)
            Me.m_gdt = gdt
        End Sub
        Public ReadOnly Property GraphDataType As cResults.eGraphDataType
            Get
                Return Me.m_gdt
            End Get
        End Property
        Public Overrides Function ToString() As String
            Dim fmt As New cGraphDataTypeFormatter()
            Return fmt.GetDescriptor(Me.m_gdt)
        End Function
    End Class

#End Region ' Private bits

    Public Sub New(ByVal uic As cUIContext, _
                   ByVal data As cData, _
                   ByVal model As cModel, _
                   ByVal result As cResults)

        Me.InitializeComponent()

        Me.m_uic = uic
        Me.m_model = model
        Me.m_result = result

        Me.m_data = New cFlowDiagramData(uic, model, data, result)
        Me.m_tree = New cTreeFlowDiagramRenderer(Me.m_data)
        Me.m_doodler = New cFlowDiagramManager(Me.m_data, Me.m_tree)

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing And (Me.components IsNot Nothing) Then
                Me.components.Dispose()
                Me.components = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_tscbmValue.Items.Clear()
        For Each gd As cResults.eGraphDataType In [Enum].GetValues(GetType(cResults.eGraphDataType))
            Me.m_tscbmValue.Items.Add(New cGraphDataItem(gd))
        Next
        Me.m_tscbmValue.SelectedIndex = 0
        Me.m_tsbnOptions.Checked = Not Me.m_scFD.Panel2Collapsed

        Me.m_pgFD.SelectedObject = Me.m_tree

    End Sub

    Public Property UIContext() As cUIContext _
      Implements IUIElement.UIContext
        Get
            Return Me.m_uic
        End Get
        Set(ByVal value As cUIContext)
            If (Me.m_uic IsNot Nothing) Then
                RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            End If
            Me.m_uic = value
            If (Me.m_uic IsNot Nothing) Then
                AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            End If
        End Set
    End Property

    Protected ReadOnly Property StyleGuide() As cStyleGuide
        Get
            Return Me.m_uic.StyleGuide
        End Get
    End Property

    Protected Overridable Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
        ' Yo!
        Me.m_pbFlowDiagram.Invalidate()
    End Sub

    Private Sub OnPaintPictureBox(sender As Object, e As System.Windows.Forms.PaintEventArgs) _
        Handles m_pbFlowDiagram.Paint

        Try
            Dim rc As Rectangle = Me.m_pbFlowDiagram.ClientRectangle
            Me.m_doodler.DrawFlowDiagram(e.Graphics, rc)
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' Overridden to elimate flickering.
    ''' </summary>
    Protected Overrides Sub OnPaintBackground(ByVal pevent As PaintEventArgs)
        ' NOP
    End Sub

    Private Sub OnShowDifferentValue(sender As Object, e As System.EventArgs) _
        Handles m_tscbmValue.SelectedIndexChanged
        Try
            Me.m_data.DisplayValue = DirectCast(Me.m_tscbmValue.SelectedItem, cGraphDataItem).GraphDataType
            Me.m_pbFlowDiagram.Invalidate()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnToggleShowOptions(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnOptions.Click
        Me.m_scFD.Panel2Collapsed = Not Me.m_tsbnOptions.Checked
    End Sub

#Region " Mouse Events "

    Private Sub OnFDMouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
        Handles m_pbFlowDiagram.MouseDown
        Using g As Graphics = Me.CreateGraphics()
            Me.m_doodler.BeginDrag(Me.m_pbFlowDiagram.ClientRectangle, e.Location, g)
        End Using
    End Sub

    Private Sub OnFDMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
        Handles m_pbFlowDiagram.MouseUp
        Me.m_doodler.EndDrag(Me.m_data, e.Location)
    End Sub

    Private Sub OnFDMouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
        Handles m_pbFlowDiagram.MouseMove
        Using g As Graphics = Me.CreateGraphics()
            Me.m_doodler.ProcessMouseMove(g, Me.m_pbFlowDiagram.ClientRectangle, e.Location)
            Me.m_pbFlowDiagram.Invalidate()
        End Using
    End Sub

#End Region ' Mouse Events

#Region " Commands "

    Private Sub OnLoadLayout(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnLoadLayout.Click

        If (Me.m_uic Is Nothing) Then Return

        Dim ifData As cXMLSettings = Nothing
        Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
        Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

        cmdFO.Invoke(SharedResources.FILEFILTER_FLOWDIAGRAM, 1)

        If (cmdFO.Result = DialogResult.OK) Then
            Try
                ifData = New cXMLSettings()
                ifData.LoadFromFile(cmdFO.FileName)
                m_doodler.Load(ifData, Me.m_pbFlowDiagram)
            Catch ex As Exception
                Dim msg As New cMessage(String.Format(SharedResources.FILE_LOAD_ERROR_DETAIL, cmdFO.FileName, ex.Message), _
                                        eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                Me.m_uic.Core.Messages.SendMessage(msg)
            End Try
        End If

    End Sub

    Private Sub OnSaveLayout(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnSaveLayout.Click

        If (Me.m_uic Is Nothing) Then Return

        Dim ifData As cXMLSettings = Nothing
        Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
        Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
        Dim strModelName As String = Me.m_uic.Core.EwEModel.Name

        cmdFS.Invoke(cFileUtils.ToValidFileName(strModelName, False), SharedResources.FILEFILTER_FLOWDIAGRAM, 1)

        If cmdFS.Result = System.Windows.Forms.DialogResult.OK Then
            Try
                ifData = New cXMLSettings()
                ifData.LoadFromFile(cmdFS.FileName)
                m_doodler.Save(ifData, Me.m_pbFlowDiagram)
            Catch ex As Exception
                Dim msg As New cMessage(String.Format(SharedResources.FILE_SAVE_ERROR_DETAIL, cmdFS.FileName, ex.Message), _
                                        eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                Me.m_uic.Core.Messages.SendMessage(msg)
            End Try
        End If

    End Sub

    Private Sub OnSaveToImage(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnSaveImage.Click

        If (Me.m_uic Is Nothing) Then Return

        Dim fmt As Imaging.ImageFormat = Imaging.ImageFormat.Bmp
        Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
        Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
        Dim fs As FileStream = Nothing
        Dim hdc As IntPtr = Nothing ' :)
        Dim mf As Metafile = Nothing
        Dim bmp As Bitmap = New Bitmap(Me.m_pbFlowDiagram.Width, Me.m_pbFlowDiagram.Height, PixelFormat.Format32bppArgb)
        Dim rc As Rectangle = Me.m_pbFlowDiagram.ClientRectangle
        Dim strModelName As String = Me.m_uic.Core.EwEModel.Name

        cmdFS.Invoke(cFileUtils.ToValidFileName(strModelName & "value chain flow diagram", False), SharedResources.FILEFILTER_IMAGE & "|" & SharedResources.FILEFILTER_IMAGE_EMF, 6)
        If cmdFS.Result = DialogResult.OK Then
            Select Case cmdFS.FilterIndex
                Case 2
                    fmt = Imaging.ImageFormat.Jpeg
                Case 3
                    fmt = Imaging.ImageFormat.Gif
                Case 4
                    fmt = Imaging.ImageFormat.Png
                Case 5
                    fmt = Imaging.ImageFormat.Tiff
                Case 6
                    fs = New FileStream(cmdFS.FileName, FileMode.Create)
                    Using g As Graphics = Graphics.FromImage(bmp)
                        hdc = g.GetHdc()
                        mf = New Metafile(fs, hdc, EmfType.EmfOnly)
                        g.ReleaseHdc(hdc)
                    End Using
                    Using g As Graphics = Graphics.FromImage(mf)
                        Me.m_doodler.DrawFlowDiagram(g, rc)
                    End Using
                    fs.Close()
                    mf.Dispose()
                    bmp.Dispose()
                    Return
                Case Else
                    fmt = Imaging.ImageFormat.Bmp
            End Select

            bmp.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)
            Using g As Graphics = Graphics.FromImage(bmp)
                Me.m_doodler.DrawFlowDiagram(g, rc)
            End Using

            Try
                bmp.Save(cmdFS.FileName, fmt)

                ' ToDo: globalize this
                Dim msg As New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_SUCCES, "Value Chain flow diagram image", cmdFS.FileName), _
                                        eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(cmdFS.FileName)
                Me.m_uic.Core.Messages.SendMessage(msg)

            Catch ex As Exception
                cLog.Write(ex, "VC::ucFlowDiagram::OnSaveImage(" & cmdFS.FileName & ")")
                Dim msg As New cMessage(String.Format(SharedResources.FILE_SAVE_ERROR_DETAIL, cmdFS.FileName, ex.Message), _
                            eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                Me.m_uic.Core.Messages.SendMessage(msg)
            End Try
            bmp.Dispose()

        End If

    End Sub

#End Region ' Commands

End Class
