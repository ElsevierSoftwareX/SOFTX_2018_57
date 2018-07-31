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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace.Basemap

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Dialogue for exporting layer data to CSV.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class dlgExportLayerDataXYZ

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_lLayers As New List(Of cEcospaceLayer)
        Private m_bDataValid As Boolean = False
        Private m_data As cEcospaceImportExportXYData = Nothing

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext)
            Me.InitializeComponent()
            Me.m_uic = uic
        End Sub

#End Region ' Constructor

#Region " Public properties "

        Public Property Layers() As cEcospaceLayer()
            Get
                Return Me.m_lLayers.ToArray()
            End Get
            Set(ByVal aLayers As cEcospaceLayer())

                Me.m_lLayers.Clear()
                If (aLayers Is Nothing) Then Return
                If (aLayers.Length = 0) Then Return
                Me.m_lLayers.AddRange(aLayers)

            End Set
        End Property

#End Region ' Public properties

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.DesignMode = True) Then Return

            Debug.Assert(Me.m_uic IsNot Nothing)

            Dim f As New cLayerFactoryInternal()
            Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
            Dim lVars As New List(Of eVarNameFlags)
            Dim strFile As String = "layers.csv"

            ' Get default layers if needed
            If (Me.m_lLayers.Count = 0) Then
                For Each layer As cEcospaceLayer In bm.Layers(eVarNameFlags.NotSet)
                    Me.m_lLayers.Add(layer)
                Next
            End If

            ' Determine name of layer file
            For Each layer As cEcospaceLayer In Me.m_lLayers
                If Not lVars.Contains(layer.VarName) Then
                    lVars.Add(layer.VarName)
                End If
            Next

            If (lVars.Count = 1) Then
                strFile = cFileUtils.ToValidFileName(f.GetLayerGroup(lVars(0)), False) & ".csv"
            End If

            ' Set default file name (user can override)
            Me.m_tbTarget.Text = Path.Combine(Me.m_uic.Core.DefaultOutputPath(EwEUtils.Core.eAutosaveTypes.Ecospace), strFile)

            Me.m_grid.Layers = Me.m_lLayers.ToArray()
            Me.m_grid.UIContext = Me.m_uic

            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            Me.m_lLayers = Nothing
            MyBase.OnFormClosed(e)
        End Sub

        Private Sub OnBrowseTarget(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnBrowseTarget.Click

            ' Browse via EwE6 open file dialog 
            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim fsc As cFileSaveCommand = TryCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim strFileFilter As String = SharedResources.FILEFILTER_CSV

            ' Sanity check
            If fsc Is Nothing Then Return

            If String.IsNullOrEmpty(Me.m_tbTarget.Text) Then
                fsc.Invoke(strFileFilter)
            Else
                fsc.Invoke(Me.m_tbTarget.Text, strFileFilter)
            End If

            If (fsc.Result = System.Windows.Forms.DialogResult.OK) Then
                Me.m_tbTarget.Text = fsc.FileName
            End If

        End Sub

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_bntOK.Click

            If Not Me.SaveMappedLayers() Then Return

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

#End Region ' Events

#Region " Internals "

        Private Function SaveMappedLayers() As Boolean

            Dim dtMappings As Dictionary(Of cEcospaceLayer, String) = Me.m_grid.Mappings()
            Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
            Dim lstrFields As New List(Of String)
            Dim strField As String = ""
            Dim strFile As String = Me.m_tbTarget.Text
            Dim layer As cEcospaceLayer = Nothing
            Dim iRow As Integer = 0
            Dim iCol As Integer = 0
            Dim iCell As Integer = 0
            Dim bSuccess As Boolean = True
            Dim msg As cMessage = Nothing

            ' Populate local data
            For Each layer In dtMappings.Keys
                strField = dtMappings(layer)
                If Not String.IsNullOrWhiteSpace(strField) Then
                    If (lstrFields.IndexOf(strField) = -1) Then
                        lstrFields.Add(strField)
                    End If
                End If
            Next

            ' Create data
            Me.m_data = New cEcospaceImportExportXYData(Me.m_uic.Core, lstrFields.ToArray())

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_DATA_SAVING)

            Try

                ' Store layer
                For iRow = 1 To bm.InRow
                    For iCol = 1 To bm.InCol
                        ' Populate data
                        For Each layer In dtMappings.Keys
                            strField = dtMappings(layer)
                            If Not String.IsNullOrEmpty(strField.Trim) Then
                                Me.m_data.Value(iRow, iCol, strField) = layer.Cell(iRow, iCol)
                            End If
                        Next layer
                    Next iCol
                Next iRow

                bSuccess = Me.m_data.WriteXYFile(strFile, Me.ColField, Me.RowField, False)

            Catch ex As Exception

            End Try

            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            If (bSuccess) Then
                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_SUCCESS, strFile),
                                   eMessageType.DataExport, EwEUtils.Core.eCoreComponentType.EcoSpace, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(strFile)
            Else
                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_FAILURE, strFile),
                                   eMessageType.DataExport, EwEUtils.Core.eCoreComponentType.EcoSpace, eMessageImportance.Critical)
            End If

            Me.m_uic.Core.Messages.SendMessage(msg)

            Return bSuccess

        End Function

        Private Property RowField() As String
            Get
                Return Me.m_tbRow.Text
            End Get
            Set(ByVal value As String)
                Me.m_tbRow.Text = value
            End Set
        End Property

        Private Property ColField() As String
            Get
                Return Me.m_tbCol.Text
            End Get
            Set(ByVal value As String)
                Me.m_tbCol.Text = value
            End Set
        End Property

        Private Sub UpdateControls()

            'Me.m_cmbRow.Enabled = (Me.m_cmbRow.Items.Count > 0)
            'Me.m_cmbCol.Enabled = (Me.m_cmbCol.Items.Count > 0)

            'Me.m_grid.Enabled = Me.m_bDataValid
            'Me.m_bntOK.Enabled = Me.m_bDataValid

        End Sub

#End Region ' Internals

    End Class

End Namespace ' Ecospace.Basemap
