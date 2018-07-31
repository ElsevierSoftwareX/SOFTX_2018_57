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
Imports System.Drawing.Imaging
Imports System.IO
Imports EwECore
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecopath.Controls.FlowDiagram

    ''' =======================================================================
    ''' <summary>
    ''' Form presenting the Ecopath Flow Diagram interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmEcopathFlowDiagram
        Inherits frmEwE

#Region " Private variables "

        Private components As System.ComponentModel.IContainer = Nothing
        Private m_data As cEcopathFlowDiagramData = Nothing
        Private m_doodler As cFlowDiagramManager = Nothing
        Private m_tree As cTreeFlowDiagramRenderer = Nothing

        Private m_bMouseDown As Boolean = False
        Private WithEvents m_pbFlowDiagram As System.Windows.Forms.PictureBox
        Private WithEvents m_scContent As System.Windows.Forms.SplitContainer
        Private WithEvents m_tsFlowDiagram As cEwEToolstrip
        Private WithEvents m_pgFlowDiagram As System.Windows.Forms.PropertyGrid
        Private WithEvents m_tsmiSaveToImage As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tss1 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tsbtnShowHideGroups As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tss2 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tsmiSettings As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsmiResetLayout As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsmiFont As ToolStripButton
        Private WithEvents m_tslData As ToolStripLabel
        Friend WithEvents m_tscmbData As ToolStripComboBox
        Friend WithEvents m_tss3 As ToolStripSeparator
        Private WithEvents m_tsmiCenterLabels As ToolStripButton

#End Region ' Private variables

#Region " Constructor/Destructor "

        Public Sub New()

            Me.InitializeComponent()

            ' This draws the control whenever it is resized
            Me.SetStyle(ControlStyles.ResizeRedraw, True)
            ' This supports mouse movement such as the mouse wheel
            Me.SetStyle(ControlStyles.UserMouse, True)

        End Sub

        Public Sub New(ByVal text As String)
            Me.New()
            'Set tab text
            Me.TabText = text
            ' Set the windows text
            Me.Text = text
        End Sub

#End Region ' Constructor 

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_tsmiFont.Image = SharedResources.CaseSensitive
            Me.m_tsmiResetLayout.Image = SharedResources.ResetHS
            Me.m_tsmiCenterLabels.Image = SharedResources.AlignObjectsCenteredHorizontalHS

            Me.m_tscmbData.Items.Add(eFDNodeValueType.Biomass)
            Me.m_tscmbData.Items.Add(eFDNodeValueType.Production)
            Me.m_tscmbData.SelectedIndex = 0

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = Nothing

            Me.m_data = New cEcopathFlowDiagramData(Me.UIContext)
            Me.m_tree = New cTreeFlowDiagramRenderer(Me.m_data)
            Me.m_doodler = New cFlowDiagramManager(Me.m_data, Me.m_tree)

            Me.m_pgFlowDiagram.SelectedObject = Me.m_tree
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoPath}
            Me.UpdateControls()

            AddHandler Me.m_tree.OnChanged, AddressOf OnTreeChanged

            ' Display Groups
            cmd = cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If cmd IsNot Nothing Then
                cmd.AddControl(Me.m_tsbtnShowHideGroups)
            End If

            ' Fonts
            cmd = cmdh.GetCommand(cShowOptionsCommand.cCOMMAND_NAME)
            If cmd IsNot Nothing Then
                cmd.AddControl(Me.m_tsmiFont, eApplicationOptionTypes.Fonts)
            End If

            ' Restore last layout
            Dim ad As cAuxiliaryData = Me.Core.AuxillaryData(Me.DataName())
            Me.m_doodler.Load(ad.Settings, Me.m_pbFlowDiagram)
            Me.m_tree.Load(ad.Settings)

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            RemoveHandler Me.m_tree.OnChanged, AddressOf OnTreeChanged

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = Nothing

            ' Display Groups
            cmd = cmdh.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If cmd IsNot Nothing Then
                cmd.RemoveControl(Me.m_tsbtnShowHideGroups)
            End If

            ' Fonts
            cmd = cmdh.GetCommand(cShowOptionsCommand.cCOMMAND_NAME)
            If cmd IsNot Nothing Then
                cmd.RemoveControl(Me.m_tsmiFont)
            End If

            MyBase.OnFormClosed(e)

        End Sub

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            ' Refresh the diagram data when ecopath data has changed
            If (msg.Source = eCoreComponentType.EcoPath) And
               (msg.Type = eMessageType.DataModified) Then
                Me.m_data.Refresh()
                Me.m_pbFlowDiagram.Invalidate()
            End If

        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            Me.m_pbFlowDiagram.Invalidate()
        End Sub

        Protected Overrides Function GetPrintContent(ByVal rcPrint As Rectangle) As Image

            Dim img As New Bitmap(rcPrint.Width, rcPrint.Height, PixelFormat.Format32bppArgb)
            img.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)
            Dim g As Graphics = Graphics.FromImage(img)
            Me.m_doodler.DrawFlowDiagram(g, rcPrint)
            g.Dispose()
            Return img

        End Function

#End Region ' Overrides

#Region " Events "

#Region " Drawing "

        Private Sub OnFlowDiagramResize(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_pbFlowDiagram.Resize

            Me.m_pbFlowDiagram.Invalidate()

        End Sub

        Private Sub OnFlowDiagramPaint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) _
            Handles m_pbFlowDiagram.Paint

            Dim rc As Rectangle = Me.m_pbFlowDiagram.ClientRectangle
            Me.m_doodler.DrawFlowDiagram(e.Graphics, rc)

        End Sub

        ''' <summary>
        ''' Override the bakcground paint routine to elimate flickering.
        ''' </summary>
        ''' <param name="pevent"></param>
        Protected Overrides Sub OnPaintBackground(ByVal pevent As PaintEventArgs)
            ' NOP
        End Sub

#End Region ' Drawing

#Region " Mouse Events "

        Private Sub OnFlowDiagramMouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
            Handles m_pbFlowDiagram.MouseDown

            Using g As Graphics = Me.CreateGraphics()
                Me.m_doodler.BeginDrag(Me.m_pbFlowDiagram.ClientRectangle, e.Location, g)
            End Using

        End Sub

        Private Sub OnFlowDiagramMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
            Handles m_pbFlowDiagram.MouseUp
            Me.m_doodler.EndDrag(Me.m_data, e.Location)
            Me.OnTreeChanged(Me.m_tree)
        End Sub

        Private Sub OnFlowDiagramMouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
            Handles m_pbFlowDiagram.MouseMove

            Using g As Graphics = Me.CreateGraphics()
                Me.m_doodler.ProcessMouseMove(g, Me.m_pbFlowDiagram.ClientRectangle, e.Location)
                Me.m_pbFlowDiagram.Invalidate()
            End Using

        End Sub

#End Region ' Mouse Events

#Region " Tree events "

        Private Sub OnTreeChanged(ByVal sender As cTreeFlowDiagramRenderer)

            Me.m_pbFlowDiagram.Invalidate(True)

            ' Preserve layout
            Dim ad As cAuxiliaryData = Me.Core.AuxillaryData(Me.DataName())
            Me.m_doodler.Save(ad.Settings, Me.m_pbFlowDiagram)
            Me.m_tree.Save(ad.Settings)

        End Sub

#End Region ' Tree events

#Region " Commands "

        Private Sub OnCenterLabels(sender As Object, e As EventArgs) _
            Handles m_tsmiCenterLabels.Click

            Me.m_tree.CenterLabels()
            Me.m_pbFlowDiagram.Invalidate()

        End Sub

        Private Sub OnResetLayout(sender As System.Object, e As System.EventArgs) _
            Handles m_tsmiResetLayout.Click

            Me.m_tree.ResetLayout()
            Me.m_pbFlowDiagram.Invalidate()

        End Sub

        Private Sub OnSettings(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiSettings.Click

            Me.UpdateControls()

        End Sub

        Private Sub OnSaveToImage(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiSaveToImage.Click

            Dim fmt As Imaging.ImageFormat = Imaging.ImageFormat.Bmp
            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim fs As FileStream = Nothing
            Dim hdc As IntPtr = Nothing ' :)
            Dim mf As Metafile = Nothing
            Dim bmp As Bitmap = Nothing
            Dim rc As Rectangle = Me.m_pbFlowDiagram.ClientRectangle

            cmdFS.Invoke(Me.FileName, SharedResources.FILEFILTER_IMAGE & "|" & SharedResources.FILEFILTER_IMAGE_EMF, 6)
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
                        bmp = New Bitmap(Me.m_pbFlowDiagram.Width, Me.m_pbFlowDiagram.Height, PixelFormat.Format32bppArgb)
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

                bmp = Me.StyleGuide.GetImage(Me.m_pbFlowDiagram.Width, Me.m_pbFlowDiagram.Height, fmt, cmdFS.FileName)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                    g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                    g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                    Me.m_doodler.DrawFlowDiagram(g, rc)
                End Using

                Try
                    bmp.Save(cmdFS.FileName, fmt)

                    ' ToDo: globalize this
                    Dim msg As New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_SUCCES, "Flow diagram image", cmdFS.FileName),
                                            eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Information)
                    msg.Hyperlink = Path.GetDirectoryName(cmdFS.FileName)
                    Me.Core.Messages.SendMessage(msg)

                Catch ex As Exception
                    cLog.Write(ex, "frmFlowDiagram::SaveImage(" & cmdFS.FileName & ")")
                    Dim msg As New cMessage(String.Format(SharedResources.FILE_SAVE_ERROR_DETAIL, cmdFS.FileName, ex.Message),
                                eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                    Me.Core.Messages.SendMessage(msg)
                End Try
                bmp.Dispose()

            End If

        End Sub

        Private Sub OnShowDataChanged(sender As Object, e As EventArgs) _
            Handles m_tscmbData.SelectedIndexChanged

            ' To catch premature events
            If (Me.m_data Is Nothing) Then Return

            Try
                Me.m_data.ValueType = CType(Me.m_tscmbData.SelectedItem, eFDNodeValueType)
                ' ToDo: localize this
                Me.m_data.DataTitle = Me.m_data.ValueType.ToString()
                Me.m_pbFlowDiagram.Invalidate()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Commands

#End Region ' Events

#Region " Internals "

        Private Function DataName() As String
            Return "FD01"
        End Function

        Protected Function FileName() As String
            Dim model As cEwEModel = Me.UIContext.Core.EwEModel
            Return cFileUtils.ToValidFileName(model.Name & " " & Me.m_tree.Title, False)
        End Function

        Protected Overrides Sub UpdateControls()
            Me.m_scContent.Panel2Collapsed = Not Me.m_tsmiSettings.Checked
            Me.m_pgFlowDiagram.Refresh()
        End Sub

        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcopathFlowDiagram))
            Me.m_pbFlowDiagram = New System.Windows.Forms.PictureBox()
            Me.m_scContent = New System.Windows.Forms.SplitContainer()
            Me.m_pgFlowDiagram = New System.Windows.Forms.PropertyGrid()
            Me.m_tsFlowDiagram = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbtnShowHideGroups = New System.Windows.Forms.ToolStripButton()
            Me.m_tsmiSettings = New System.Windows.Forms.ToolStripButton()
            Me.m_tss2 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiSaveToImage = New System.Windows.Forms.ToolStripButton()
            Me.m_tss1 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiFont = New System.Windows.Forms.ToolStripButton()
            Me.m_tsmiCenterLabels = New System.Windows.Forms.ToolStripButton()
            Me.m_tsmiResetLayout = New System.Windows.Forms.ToolStripButton()
            Me.m_tslData = New System.Windows.Forms.ToolStripLabel()
            Me.m_tscmbData = New System.Windows.Forms.ToolStripComboBox()
            Me.m_tss3 = New System.Windows.Forms.ToolStripSeparator()
            CType(Me.m_pbFlowDiagram, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_scContent, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scContent.Panel1.SuspendLayout()
            Me.m_scContent.Panel2.SuspendLayout()
            Me.m_scContent.SuspendLayout()
            Me.m_tsFlowDiagram.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_pbFlowDiagram
            '
            Me.m_pbFlowDiagram.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_pbFlowDiagram, "m_pbFlowDiagram")
            Me.m_pbFlowDiagram.Name = "m_pbFlowDiagram"
            Me.m_pbFlowDiagram.TabStop = False
            '
            'm_scContent
            '
            resources.ApplyResources(Me.m_scContent, "m_scContent")
            Me.m_scContent.Name = "m_scContent"
            '
            'm_scContent.Panel1
            '
            Me.m_scContent.Panel1.Controls.Add(Me.m_pbFlowDiagram)
            '
            'm_scContent.Panel2
            '
            Me.m_scContent.Panel2.Controls.Add(Me.m_pgFlowDiagram)
            '
            'm_pgFlowDiagram
            '
            resources.ApplyResources(Me.m_pgFlowDiagram, "m_pgFlowDiagram")
            Me.m_pgFlowDiagram.Name = "m_pgFlowDiagram"
            '
            'm_tsFlowDiagram
            '
            Me.m_tsFlowDiagram.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsFlowDiagram.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbtnShowHideGroups, Me.m_tsmiSettings, Me.m_tss2, Me.m_tslData, Me.m_tscmbData, Me.m_tss1, Me.m_tsmiSaveToImage, Me.m_tss3, Me.m_tsmiFont, Me.m_tsmiCenterLabels, Me.m_tsmiResetLayout})
            resources.ApplyResources(Me.m_tsFlowDiagram, "m_tsFlowDiagram")
            Me.m_tsFlowDiagram.Name = "m_tsFlowDiagram"
            Me.m_tsFlowDiagram.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbtnShowHideGroups
            '
            resources.ApplyResources(Me.m_tsbtnShowHideGroups, "m_tsbtnShowHideGroups")
            Me.m_tsbtnShowHideGroups.Name = "m_tsbtnShowHideGroups"
            '
            'm_tsmiSettings
            '
            Me.m_tsmiSettings.CheckOnClick = True
            Me.m_tsmiSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsmiSettings, "m_tsmiSettings")
            Me.m_tsmiSettings.Name = "m_tsmiSettings"
            '
            'm_tss2
            '
            Me.m_tss2.Name = "m_tss2"
            resources.ApplyResources(Me.m_tss2, "m_tss2")
            '
            'm_tsmiSaveToImage
            '
            resources.ApplyResources(Me.m_tsmiSaveToImage, "m_tsmiSaveToImage")
            Me.m_tsmiSaveToImage.Name = "m_tsmiSaveToImage"
            '
            'm_tss1
            '
            Me.m_tss1.Name = "m_tss1"
            resources.ApplyResources(Me.m_tss1, "m_tss1")
            '
            'm_tsmiFont
            '
            Me.m_tsmiFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsmiFont, "m_tsmiFont")
            Me.m_tsmiFont.Name = "m_tsmiFont"
            '
            'm_tsmiCenterLabels
            '
            Me.m_tsmiCenterLabels.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsmiCenterLabels, "m_tsmiCenterLabels")
            Me.m_tsmiCenterLabels.Name = "m_tsmiCenterLabels"
            '
            'm_tsmiResetLayout
            '
            Me.m_tsmiResetLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsmiResetLayout, "m_tsmiResetLayout")
            Me.m_tsmiResetLayout.Name = "m_tsmiResetLayout"
            '
            'm_tslData
            '
            Me.m_tslData.Name = "m_tslData"
            resources.ApplyResources(Me.m_tslData, "m_tslData")
            '
            'm_tscmbData
            '
            Me.m_tscmbData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_tscmbData.Name = "m_tscmbData"
            resources.ApplyResources(Me.m_tscmbData, "m_tscmbData")
            '
            'm_tss3
            '
            Me.m_tss3.Name = "m_tss3"
            resources.ApplyResources(Me.m_tss3, "m_tss3")
            '
            'frmEcopathFlowDiagram
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_tsFlowDiagram)
            Me.Controls.Add(Me.m_scContent)
            Me.Name = "frmEcopathFlowDiagram"
            Me.TabText = ""
            CType(Me.m_pbFlowDiagram, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scContent.Panel1.ResumeLayout(False)
            Me.m_scContent.Panel2.ResumeLayout(False)
            CType(Me.m_scContent, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scContent.ResumeLayout(False)
            Me.m_tsFlowDiagram.ResumeLayout(False)
            Me.m_tsFlowDiagram.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

#End Region ' Internals

    End Class

End Namespace