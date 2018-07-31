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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.SpatialData

#End Region ' Imports

''' =======================================================================
''' <summary>
''' Dialog, implementing the Ecospace Edit Basemap user interface.
''' </summary>
''' =======================================================================
Public Class dlgEditBasemap

#Region " Private variables "

    Private m_uic As cUIContext = Nothing
    Private m_basemap As cEcospaceBasemap = Nothing

    Private m_fpInCol As cEwEFormatProvider = Nothing
    Private m_fpInRow As cEwEFormatProvider = Nothing
    Private m_fpLat As cEwEFormatProvider = Nothing
    Private m_fpLon As cEwEFormatProvider = Nothing
    Private m_fpCellLength As cEwEFormatProvider = Nothing
    Private m_fpCellSize As cEwEFormatProvider = Nothing
    Private m_fpSquareCells As cEwEFormatProvider = Nothing

    Private m_bInitialized As Boolean = False
    Private m_bInUpdate As Boolean = False

    ''' <summary>Imported raster, if any</summary>
    Private m_rs As ISpatialRaster = Nothing

#End Region ' Private variables

    Public Sub New(ByVal uic As cUIContext)
        Me.m_uic = uic
        Me.m_basemap = Me.m_uic.Core.EcospaceBasemap
        Me.InitializeComponent()
    End Sub

#Region " Events "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_pbLink.Image = ScientificInterfaceShared.My.Resources.chain_vert

        Me.m_fpInCol = New cEwEFormatProvider(Me.m_uic, Me.m_nudColCount, GetType(Integer), Me.m_basemap.GetVariableMetadata(eVarNameFlags.InCol))
        Me.m_fpInCol.Value = Me.m_basemap.InCol

        Me.m_fpInRow = New cEwEFormatProvider(Me.m_uic, Me.m_nudRowCount, GetType(Integer), Me.m_basemap.GetVariableMetadata(eVarNameFlags.InRow))
        Me.m_fpInRow.Value = Me.m_basemap.InRow

        Me.m_fpLat = New cEwEFormatProvider(Me.m_uic, Me.m_nudLatTL, GetType(Single), Me.m_basemap.GetVariableMetadata(eVarNameFlags.Latitude))
        Me.m_fpLat.Value = Me.m_basemap.Latitude

        Me.m_fpLon = New cEwEFormatProvider(Me.m_uic, Me.m_nudLonTL, GetType(Single), Me.m_basemap.GetVariableMetadata(eVarNameFlags.Longitude))
        Me.m_fpLon.Value = Me.m_basemap.Longitude

        Me.m_fpCellLength = New cEwEFormatProvider(Me.m_uic, Me.m_nudCellLength, GetType(Single), Me.m_basemap.GetVariableMetadata(eVarNameFlags.CellLength))
        Me.m_fpCellLength.Value = Me.m_basemap.CellLength

        Me.m_fpCellSize = New cEwEFormatProvider(Me.m_uic, Me.m_nudCellSize, GetType(Single), Me.m_basemap.GetVariableMetadata(eVarNameFlags.CellSize))
        Me.m_fpCellSize.Value = Me.m_basemap.CellSize

        Me.m_fpSquareCells = New cEwEFormatProvider(Me.m_uic, Me.m_cbAssumeSquareCells, GetType(Boolean))
        Me.m_fpSquareCells.Value = Me.m_basemap.AssumeSquareCells
        AddHandler Me.m_fpSquareCells.OnValueChanged, AddressOf OnAssumeSquareCellsChanged

        Me.UpdateControls()

        Me.m_bInitialized = True

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        RemoveHandler Me.m_fpSquareCells.OnValueChanged, AddressOf OnAssumeSquareCellsChanged
        Me.m_fpSquareCells.Release()

        Me.m_fpCellLength.Release()
        Me.m_fpCellSize.Release()
        Me.m_fpInCol.Release()
        Me.m_fpInRow.Release()
        Me.m_fpLat.Release()
        Me.m_fpLon.Release()
        MyBase.OnFormClosed(e)

    End Sub

    Private Sub OnImportASCIIMap(sender As System.Object, e As System.EventArgs) _
        Handles m_tsmiImportFromASCII.Click

        Dim cmd As cFileOpenCommand = CType(Me.m_uic.CommandHandler.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)
        cmd.Invoke(SharedResources.FILEFILTER_ASC)
        If (cmd.Result = System.Windows.Forms.DialogResult.OK) Then
            Dim imp As New cEcospaceImportExportASCIIData()
            If imp.Read(cmd.FileName) Then

                Me.m_rs = imp.ToRaster()
                Me.m_fpInCol.Value = Me.m_rs.NumCols
                Me.m_fpInCol.Style = cStyleGuide.eStyleFlags.NotEditable
                Me.m_fpInRow.Value = Me.m_rs.NumRows
                Me.m_fpInRow.Style = cStyleGuide.eStyleFlags.NotEditable
                Me.m_fpCellSize.Value = Me.m_rs.CellSize
                Me.m_fpCellSize.Style = cStyleGuide.eStyleFlags.NotEditable
                Me.m_fpCellLength.Style = cStyleGuide.eStyleFlags.NotEditable
                Me.m_fpLon.Value = Me.m_rs.TopLeft.X
                Me.m_fpLon.Style = cStyleGuide.eStyleFlags.NotEditable
                Me.m_fpLat.Value = Me.m_rs.TopLeft.Y
                Me.m_fpLat.Style = cStyleGuide.eStyleFlags.NotEditable

            End If

        End If

    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnOk.Click
        If Me.Apply() Then
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnCancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub OnCellLengthChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_nudCellLength.ValueChanged
        Me.CalcCellSize()
    End Sub

    Private Sub OnCellSizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_nudCellSize.ValueChanged
        Me.CalcCellLength()
    End Sub

    Private Sub OnAssumeSquareCellsChanged(sender As System.Object, e As System.EventArgs)
        Me.CalcCellSize()
        Me.UpdateControls()
    End Sub

#End Region ' Events 

#Region " Implementation "

    Private Sub UpdateControls()

        Dim fmt As New EwECore.Style.cMapUnitFormatter()
        ' ToDo: AssumeSquareCells should be centralized at EwE model level?
        Dim strUnit As String = If(Me.m_cbAssumeSquareCells.Checked,
                                                 fmt.GetDescriptor(eUnitMapRefType.m),
                                                 fmt.GetDescriptor(eUnitMapRefType.dd))

        Me.m_lblUnitLon.Text = strUnit
        Me.m_lblUnitLat.Text = strUnit
        Me.m_lblUnitCellSize.Text = strUnit
        Me.m_lblUnitCellLen.Text = fmt.GetDescriptor(eUnitMapRefType.km)

        Me.m_btnOk.Enabled = True

    End Sub

    Private Sub CalcCellSize()

        If Not Me.m_bInitialized Or Me.m_bInUpdate Then Return

        Me.m_bInUpdate = True
        Dim sLen As Single = CSng(Me.m_nudCellLength.Value)
        Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
        Me.m_fpCellSize.Value = bm.ToCellSize(sLen, Me.m_cbAssumeSquareCells.Checked)
        Me.m_bInUpdate = False

    End Sub

    Private Sub CalcCellLength()

        If Not Me.m_bInitialized Or Me.m_bInUpdate Then Return

        Me.m_bInUpdate = True
        Dim sSize As Single = CSng(Me.m_nudCellSize.Value)
        Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
        Me.m_fpCellLength.Value = bm.ToCellLength(sSize, Me.m_cbAssumeSquareCells.Checked)
        Me.m_bInUpdate = False

    End Sub

    Private Function Apply() As Boolean

        If Not Me.m_uic.Core.SaveChanges() Then Return False

        Dim iColCount As Integer = CInt(Me.m_fpInCol.Value)
        Dim iRowCount As Integer = CInt(Me.m_fpInRow.Value)
        Dim fmsg As cFeedbackMessage = Nothing
        Dim core As cCore = Me.m_uic.Core
        Dim bResizeMap As Boolean = False

        If ((iRowCount <> Me.m_basemap.InRow) Or (iColCount <> Me.m_basemap.InCol)) Then
            bResizeMap = True
            If ((iRowCount < Me.m_basemap.InRow) Or (iColCount < Me.m_basemap.InCol)) Then
                ' Prompt user
                fmsg = New cFeedbackMessage(My.Resources.ECOSPACE_BASEMAP_SHRINK_PROMPT,
                                            eCoreComponentType.External, eMessageType.Any,
                                            eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                fmsg.Reply = eMessageReply.NO
                core.Messages.SendMessage(fmsg)
                If (fmsg.Reply = eMessageReply.NO) Then
                    Return False
                End If
            End If
        End If

        ' Apply other data first
        Me.m_basemap.AssumeSquareCells = CBool(Me.m_fpSquareCells.Value)
        Me.m_basemap.CellLength = CSng(Me.m_fpCellLength.Value)
        Me.m_basemap.CellSize = CSng(Me.m_fpCellSize.Value)
        Me.m_basemap.Latitude = CSng(Me.m_fpLat.Value)
        Me.m_basemap.Longitude = CSng(Me.m_fpLon.Value)
        Me.m_uic.Core.SaveChanges(True)

        If (bResizeMap = True) Then
            cApplicationStatusNotifier.StartProgress(core, My.Resources.STATUS_RESIZING_MAP, -1)
            Try
                Me.m_uic.Core.ResizeEcospaceBasemap(iRowCount, iColCount)
            Catch ex As Exception
                cLog.Write(ex, "dlgEditBasemap.Apply:resizing")
            End Try
            cApplicationStatusNotifier.EndProgress(core)
        End If

        If (Me.m_rs IsNot Nothing) Then
            cApplicationStatusNotifier.StartProgress(core, My.Resources.STATUS_RESIZING_MAP, -1)
            Try
                ' Use adapter here?
                Dim l As cEcospaceLayerDepth = Me.m_uic.Core.EcospaceBasemap.LayerDepth
                For ir As Integer = 1 To Me.m_rs.NumRows
                    For ic As Integer = 1 To Me.m_rs.NumCols
                        Dim val As Single = CSng(Me.m_rs.Cell(ir, ic))
                        If (val = Me.m_rs.NoData) Then val = 0
                        l.Cell(ir, ic) = val
                    Next
                Next
                l.Invalidate()
                Me.m_uic.Core.onChanged(l)
            Catch ex As Exception
                Debug.Assert(False)
                cLog.Write(ex, "dlgEditBasemap.Apply:reading_depth")
            End Try
            cApplicationStatusNotifier.EndProgress(core)
        End If
        Return True

    End Function

#End Region ' Implementation

End Class
