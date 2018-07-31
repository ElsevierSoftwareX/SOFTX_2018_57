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

Option Explicit On
Option Strict On
Imports EwECore
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.Map.Layers

#End Region ' Imports

Namespace Ecospace.Basemap.Layers

    ''' =======================================================================
    ''' <summary>
    ''' Dialog, implementing the Ecospace Edit Layer user interface.
    ''' </summary>
    ''' =======================================================================
    Public Class dlgEditLayer

#Region " Private variables "

        Private m_uic As cUIContext = Nothing
        Private m_qehGrid As cQuickEditHandler = Nothing

        ''' <summary>Original layer this dialog was invoked for.</summary>
        Private m_layerOriginal As cDisplayLayerRaster = Nothing
        Private m_layerDepth As cDisplayLayerRaster = Nothing
        Private m_edittype As eLayerEditTypes

        ''' <summary>Work layer (a copy of the original) for this dialog to work on.</summary>
        Private m_layerWork As cDisplayLayerRaster = Nothing
        ''' <summary>Editor to transmogrify the representation of the layer.</summary>
        Private m_ucEditVisualStyle As ucEditVisualStyle = Nothing

        Private m_fpName As cEwEFormatProvider = Nothing
        Private m_fpWeight As cEwEFormatProvider = Nothing
        Private m_fpDescription As cEwEFormatProvider = Nothing

        ' -- Hackerdihack

        Private m_bIsVectorData As Boolean = False
        Private m_iVectorData As Integer = 0

#End Region ' Private variables

#Region " Constructors "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uic"></param>
        ''' <param name="layer"></param>
        ''' <param name="edittype"></param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext,
                       ByRef layer As cDisplayLayerRaster,
                       ByVal edittype As eLayerEditTypes)

            Debug.Assert(layer IsNot Nothing)

            Me.InitializeComponent()

            ' Set the references
            Me.m_uic = uic
            Me.m_grid.UIContext = Me.m_uic
            Me.m_zoommap.UIContext = Me.m_uic

            Me.m_layerOriginal = layer

            ' Resolve depth layer
            If Not (TypeOf layer.Data Is cEcospaceLayerDepth) Then
                Dim fact As New cLayerFactoryInternal()
                Me.m_layerDepth = fact.GetLayers(uic, eVarNameFlags.LayerDepth)(0)
            End If
            Me.m_edittype = edittype

            Me.m_layerWork = New cDisplayLayerRaster(uic, layer) ' Work on a clone
            Me.m_layerWork.AllowValidation = False
            Me.m_layerWork.IsSelected = True ' Select layer, otherwise its content may not be rendered

            ' First set default index, then make vector stuff 'live' if need be ;)
            Me.m_tscmbVectorData.SelectedIndex = 0
            Me.m_bIsVectorData = (TypeOf Me.m_layerWork.Data Is cEcospaceLayerVelocity)

        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_grid.DataName = Me.m_layerOriginal.Name

            Me.m_qehGrid = New cQuickEditHandler()
            'Me.m_qehGrid.ShowImportExport = False
            Me.m_qehGrid.Attach(Me.m_grid, Me.m_uic, Me.m_tsGrid)
            Me.m_qehGrid.IsOutputGrid = Me.m_layerWork.Editor.IsReadOnly

            ' Show your stuff
            Me.m_zoommap.Map.AddLayer(Me.m_layerWork)

            ' Do not add depth layer if already showing depth layer
            If ((Not ReferenceEquals(Me.m_layerOriginal, Me.m_layerDepth)) And
                (Me.m_layerDepth IsNot Nothing)) Then
                Me.m_zoommap.Map.AddLayer(Me.m_layerDepth)
            End If

            Select Case Me.m_edittype
                Case eLayerEditTypes.EditVisuals
                    Me.m_tcLayerView.SelectedTab = Me.m_tpAppearance
                Case eLayerEditTypes.EditData
                    Me.m_tcLayerView.SelectedTab = Me.m_tpData
            End Select

            ' Set up format providers
            Me.m_fpName = New cEwEFormatProvider(Me.m_uic, Me.m_tbNameValue, GetType(String))
            Me.m_fpWeight = New cEwEFormatProvider(Me.m_uic, Me.m_nudWeight, GetType(Single))
            Me.m_fpDescription = New cEwEFormatProvider(Me.m_uic, Me.m_tbDescription, GetType(String))

            Me.LoadLayer()
            Me.UpdateControls()
            Me.DrawPreview()

            If (Me.m_ucEditVisualStyle IsNot Nothing) Then
                AddHandler Me.m_ucEditVisualStyle.OnVisualStyleChanged, AddressOf OnVisualStyleChanged
            End If
            AddHandler Me.m_fpName.OnValueChanged, AddressOf OnNameChanged

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If (Me.m_ucEditVisualStyle IsNot Nothing) Then
                RemoveHandler Me.m_ucEditVisualStyle.OnVisualStyleChanged, AddressOf OnVisualStyleChanged
            End If

            Me.m_qehGrid.Detach()
            Me.m_qehGrid = Nothing
            Me.m_grid.UIContext = Nothing

            RemoveHandler Me.m_fpName.OnValueChanged, AddressOf OnNameChanged

            Me.m_fpName.Release()
            Me.m_fpWeight.Release()
            Me.m_fpDescription.Release()

            Me.m_layerDepth = Nothing
            Me.m_layerOriginal = Nothing
            Me.m_layerWork.Dispose()
            Me.m_layerWork = Nothing

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Overrides

#Region " Local events "

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles OK_Button.Click

            If Not Me.ApplyChanges() Then Return
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles Cancel_Button.Click

            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()

        End Sub

        Private Sub OnApply(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles Apply_Button.Click

            Me.ApplyChanges()

        End Sub

        Private Sub OnVisualStyleChanged(ByVal sender As ucEditVisualStyle)

            ' Update work layer Visual Style
            Me.m_ucEditVisualStyle.Apply(Me.m_layerWork.Renderer.VisualStyle)
            Me.m_layerWork.Update(cDisplayLayer.eChangeFlags.VisualStyle)

        End Sub

#Region " Import "

        Private Sub OnImportCSV(sender As System.Object, e As System.EventArgs) _
            Handles m_tsmiImportCSV.Click
            Try
                Me.m_qehGrid.ImportGridFromCSV()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnImportXYZ(sender As System.Object, e As System.EventArgs) _
            Handles m_tsmiImportXYZ.Click

            Try
                Dim cmd As cImportLayerCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cImportLayerCommand.cCOMMAND_NAME), cImportLayerCommand)
                Dim l As cEcospaceLayer = Me.m_layerWork.Data
                If (TypeOf l Is cEcospaceLayerVelocity) Then
                    cmd.Invoke(DirectCast(l, cEcospaceLayerVelocity).VelocityLayers, eNativeLayerFileFormatTypes.XYZ)
                Else
                    cmd.Invoke(New cEcospaceLayer() {l}, eNativeLayerFileFormatTypes.XYZ)
                End If
                Me.m_layerWork.Update(cDisplayLayer.eChangeFlags.Map)
                Me.m_grid.RefreshContent()
            Catch ex As Exception

            End Try

        End Sub

        Private Sub OnImportAscii(sender As System.Object, e As System.EventArgs) _
            Handles m_tsmiImportAsc.Click

            Try
                Dim cmd As cImportLayerCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cImportLayerCommand.cCOMMAND_NAME), cImportLayerCommand)
                Dim l As cEcospaceLayer = Me.m_layerWork.Data
                If (TypeOf l Is cEcospaceLayerVelocity) Then
                    l = DirectCast(l, cEcospaceLayerVelocity).VelocityLayers(Me.m_tscmbVectorData.SelectedIndex)
                End If
                cmd.Invoke(New cEcospaceLayer() {l}, eNativeLayerFileFormatTypes.ASCII)
                Me.m_layerWork.Update(cDisplayLayer.eChangeFlags.Map)
                Me.m_grid.RefreshContent()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Import

#Region " Export "

        Private Sub OnExportCSV(sender As System.Object, e As System.EventArgs) _
            Handles m_tsmiExportCSV.Click
            Try
                Me.m_qehGrid.ExportGridToCSV()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnExportAsc(sender As System.Object, e As System.EventArgs) _
            Handles m_tsmiExportAsc.Click

            Try
                Dim cmd As cExportLayerCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cExportLayerCommand.cCOMMAND_NAME), cExportLayerCommand)
                Dim l As cEcospaceLayer = Me.m_layerWork.Data
                If (TypeOf l Is cEcospaceLayerVelocity) Then
                    l = DirectCast(l, cEcospaceLayerVelocity).VelocityLayers(Me.m_tscmbVectorData.SelectedIndex)
                End If
                cmd.Invoke(New cEcospaceLayer() {l}, eNativeLayerFileFormatTypes.ASCII)
                Me.UpdateControls()
            Catch ex As Exception

            End Try

        End Sub

        Private Sub OnExportXYZ(sender As System.Object, e As System.EventArgs) Handles m_tsmiExportXYZ.Click

            Try
                Dim cmd As cExportLayerCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cExportLayerCommand.cCOMMAND_NAME), cExportLayerCommand)
                Dim l As cEcospaceLayer = Me.m_layerWork.Data
                If (TypeOf l Is cEcospaceLayerVelocity) Then
                    cmd.Invoke(DirectCast(l, cEcospaceLayerVelocity).VelocityLayers, eNativeLayerFileFormatTypes.XYZ)
                Else
                    cmd.Invoke(New cEcospaceLayer() {l}, eNativeLayerFileFormatTypes.XYZ)
                End If
                Me.UpdateControls()
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Export

        Private Sub OnNameChanged(sender As Object, e As System.EventArgs)
            Try
                Me.UpdateControls()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnSelectData(sender As System.Object, e As System.EventArgs) _
            Handles m_tscmbVectorData.SelectedIndexChanged

            If (Me.m_bIsVectorData) Then
                Me.m_grid.VectorFieldIndex = Me.m_tscmbVectorData.SelectedIndex
                Me.m_grid.RefreshContent()
            End If

        End Sub

#End Region ' Local events

#Region " Internal implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Diagnostic method, states if a layer has a unique core variable 
        ''' link. Layers with unique sources support extra's that can be stored
        ''' in the database such as remarks and visual styles.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function HasUniqueSource() As Boolean
            If (Me.m_layerOriginal.Source Is Nothing) Then Return False
            If (TypeOf Me.m_layerOriginal.Source Is cEcospaceBasemap) Then Return False
            Return True
        End Function

        Private Sub LoadLayer()

            Dim vs As cVisualStyle = Me.m_layerWork.Renderer.VisualStyle
            Dim src As cCoreInputOutputBase = Me.m_layerWork.Source

            Me.m_lblWeight.Visible = False
            Me.m_nudWeight.Visible = False
            Me.m_lblDescription.Visible = False
            Me.m_tbDescription.Visible = False

            If (Me.HasUniqueSource()) Then
                Me.m_fpName.Enabled = True
                Me.m_tbRemarks.Text = src.Remark
                Me.m_tbRemarks.Enabled = True

                If TypeOf src Is cEcospaceLayerImportance Then
                    Me.m_lblWeight.Visible = True
                    Me.m_nudWeight.Visible = True
                    Me.m_lblDescription.Visible = True
                    Me.m_tbDescription.Visible = True

                    Me.m_fpWeight.Value = src.GetVariable(eVarNameFlags.ImportanceWeight)
                    Me.m_fpDescription.Value = src.GetVariable(eVarNameFlags.Description)
                End If
            Else
                Me.m_fpName.Enabled = False
                Me.m_tbRemarks.Text = My.Resources.STATUS_REMARKS_NOT_SUPPORTED
                Me.m_tbRemarks.Enabled = False
            End If

            ' Do not use display text; user may want to edit this
            Me.m_fpName.Value = m_layerWork.Name

            Me.m_ucEditVisualStyle = ucEditVisualStyle.GetEditor(Me.m_uic, vs, Me.m_layerWork.Renderer.VisualStyleFlags)

            If (Me.m_ucEditVisualStyle IsNot Nothing) Then
                Me.m_plAppearance.Height = Me.m_ucEditVisualStyle.Height
                Me.m_ucEditVisualStyle.Dock = DockStyle.Fill
                Me.m_plAppearance.Controls.Add(Me.m_ucEditVisualStyle)
            End If

            Me.m_grid.Layer = Me.m_layerWork
            Me.m_grid.VectorFieldIndex = Me.m_iVectorData
            Me.m_grid.RefreshContent()

            Me.m_tlpDetails.PerformLayout()
            Me.m_tlpBits.PerformLayout()

        End Sub

        Private Sub DrawPreview()
            Me.m_zoommap.Map.Refresh()
        End Sub

        Private Sub UpdateControls()

            Dim bEditable As Boolean = True

            If (Me.m_layerOriginal.Editor IsNot Nothing) Then
                bEditable = (Me.m_layerOriginal.Editor.IsReadOnly = False)
            End If

            Me.m_tsddImport.Enabled = bEditable
            Me.Text = cStringUtils.Localize(My.Resources.ECOSPACE_CAPTION_EDITLAYER, Me.m_tbNameValue.Text)

            Me.m_tscmbVectorData.Visible = Me.m_bIsVectorData

        End Sub

        Private Function ApplyChanges() As Boolean

            Dim cf As cDisplayLayer.eChangeFlags = 0
            Dim src As cCoreInputOutputBase = Me.m_layerOriginal.Source

            If Me.m_tbNameValue.Enabled Then
                Me.m_layerOriginal.Name = CStr(Me.m_fpName.Value)
            End If

            If (HasUniqueSource()) Then

                If Me.m_tbRemarks.Enabled Then
                    src.Remark = Me.m_tbRemarks.Text
                End If

                If TypeOf Me.m_layerOriginal.Source Is cEcospaceLayerImportance Then
                    src.SetVariable(eVarNameFlags.ImportanceWeight, Me.m_fpWeight.Value)
                    src.SetVariable(eVarNameFlags.Description, Me.m_fpDescription.Value)
                End If

            End If

            If (Me.m_ucEditVisualStyle IsNot Nothing) Then
                ' Apply changes
                Me.m_ucEditVisualStyle.Apply(Me.m_layerOriginal.Renderer.VisualStyle)
                cf = cf Or cDisplayLayer.eChangeFlags.VisualStyle
            End If

            Me.m_grid.Apply(Me.m_layerOriginal)
            cf = cf Or cDisplayLayer.eChangeFlags.Map

            ' Fire layer changed notification
            Me.m_layerOriginal.Update(cf)

            Return True

        End Function

#End Region ' Internal implementation

    End Class

End Namespace