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
Imports EwECore.SpatialData
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace

    Public Class frmSpatialTimeSeries

#Region " Private vars "

        Private m_ds As ISpatialDataSet = Nothing
        Private m_manDS As cSpatialDataSetManager = Nothing

#End Region ' Private vars

#Region " Private helper classes "

        ''' <summary>
        ''' Private class for the adapter filter
        ''' </summary>
        Private Class cSpatialDataAdapterFilterItem

            Private m_adt As cSpatialDataAdapter
            Private m_fmt As New cSpatialDataAdapterFormatter()

            Public Sub New(adt As cSpatialDataAdapter)
                Me.m_adt = adt
            End Sub

            Public Overrides Function ToString() As String
                If (Me.m_adt Is Nothing) Then Return ScientificInterfaceShared.My.Resources.GENERIC_VALUE_ALL
                Return Me.m_fmt.GetDescriptor(Me.m_adt)
            End Function

            Public ReadOnly Property Adapter As cSpatialDataAdapter
                Get
                    Return Me.m_adt
                End Get
            End Property
        End Class

#End Region ' Private helper classes

#Region " Form overrides "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)

                ' Cleanup
                If (Me.UIContext IsNot Nothing) Then
                    Me.m_manDS = Nothing
                End If

                ' Set
                MyBase.UIContext = value
                Me.m_toolbox.UIContext = value
                Me.m_gridApply.UIContext = value

                ' Config
                If (value IsNot Nothing) Then
                    Me.m_manDS = value.Core.SpatialDataConnectionManager.DatasetManager
                End If

            End Set
        End Property

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_tsbnConnections.Image = SharedResources.Database
            Me.m_tslbFilter.Image = SharedResources.FilterHS

            Try
                Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("EditSpatialDatasets")
                cmd.AddControl(Me.m_tsbnConnections)
            Catch ex As Exception
                Debug.Assert(False)
            End Try


            ' Fill filter combo
            Me.m_tscmbLayerVariable.Items.Add(New cSpatialDataAdapterFilterItem(Nothing))
            For Each adt As cSpatialDataAdapter In Me.Core.SpatialDataConnectionManager.Adapters
                Me.m_tscmbLayerVariable.Items.Add(New cSpatialDataAdapterFilterItem(adt))
            Next
            Me.m_tscmbLayerVariable.SelectedIndex = 0

            Me.m_toolbox.SelectedTimeStep = 0

            Me.m_tsbnOnlyShowConnectedLayers.Checked = Me.m_gridApply.OnlyShowConnected

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSpace, eCoreComponentType.Core}

        End Sub

        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)

            If (Me.UIContext Is Nothing) Then Return

            Try
                Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("EditSpatialDatasets")
                cmd.RemoveControl(Me.m_tsbnConnections)
            Catch ex As Exception
                Debug.Assert(False)
            End Try

            Me.m_manDS.IndexDataset = Nothing ' Stop indexing
            Me.m_manDS.Save()

            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ct As ScientificInterfaceShared.Style.cStyleGuide.eChangeType)
            MyBase.OnStyleGuideChanged(ct)

            If ((ct And cStyleGuide.eChangeType.Colours) > 0) Then
                Me.m_toolbox.Invalidate()
            End If

        End Sub

#End Region ' Form overrides

#Region " Control events "

        Private Sub OnSelectedDatasetChanged(owner As Object, ds As EwEUtils.SpatialData.ISpatialDataSet) _
            Handles m_toolbox.OnSelectedDatasetChanged

            If (ReferenceEquals(ds, Me.m_ds)) Then Return

            If (Me.m_ds IsNot Nothing) Then
                ' Clear selection
            End If

            Me.m_ds = ds
            Me.m_manDS.IndexDataset = ds

        End Sub

        Private Sub OnFilterByLayerVariable(sender As System.Object, e As System.EventArgs) _
            Handles m_tscmbLayerVariable.SelectedIndexChanged

            Dim t As cSpatialDataAdapterFilterItem = DirectCast(Me.m_tscmbLayerVariable.SelectedItem, cSpatialDataAdapterFilterItem)
            Dim vn As eVarNameFlags = eVarNameFlags.NotSet

            If (t IsNot Nothing) Then
                If (t.Adapter IsNot Nothing) Then
                    vn = t.Adapter.VarName
                End If
            End If

            Me.m_gridApply.Filter = vn
            Me.m_toolbox.Filter = vn

        End Sub

        Private Sub OnSelectedTimestepChanged(owner As Object, iTimeStep As Integer, dt As Date) _
            Handles m_toolbox.OnSelectedTimestepChanged
        End Sub

        Private Sub OnToggleOnlyShowConnectedLayers(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnOnlyShowConnectedLayers.Click
            Me.m_gridApply.OnlyShowConnected = Me.m_tsbnOnlyShowConnectedLayers.Checked
        End Sub

#End Region ' Control events

#Region " Callbacks "

        Public Overrides Sub OnCoreMessage(msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            ' Dataset changes are passed on via core layer changes
            Select Case msg.DataType

                Case eDataTypes.EcospaceBasemap
                    ' NOP

                Case eDataTypes.EcospaceSpatialDataConnection, eDataTypes.EcospaceSpatialDataSource

                    ' Optimization
                    If (msg.Type <> eMessageType.Progress) Then
                        Me.m_gridApply.RefreshContent()
                    End If

                    ' Toolbox takes care of itself

                Case Else
                    ' Could be a layer change. This test could be massively improved ;)
                    If msg.Type = eMessageType.DataModified Then
                        Me.m_gridApply.RefreshContent()
                    End If

            End Select

            Select Case msg.Type
                Case eMessageType.GlobalSettingsChanged
                    Me.UpdateControls()

            End Select

        End Sub

#End Region ' Callbacks

    End Class

End Namespace
