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
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Arguments for a <see cref="cDatasetPicker.OnPicked">dataset selection event</see>.
''' </summary>
''' ===========================================================================
Public Class cDatasetPickedArgs
    Inherits EventArgs

    Private m_dataset As ISpatialDataSet = Nothing
    Private m_tag As Object = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="ds">The selected <see cref="ISpatialDataSet"/>.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ds As ISpatialDataSet, tag As Object)
        Me.m_dataset = ds
        Me.m_tag = tag
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the selected <see cref="ISpatialDataSet"/>, or Nothing if a selection was cleared.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Dataset As ISpatialDataSet
        Get
            Return Me.m_dataset
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the selected tag.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Tag As Object
        Get
            Return Me.m_tag
        End Get
    End Property

End Class

''' ===========================================================================
''' <summary>
''' Interface to select and configure a <see cref="ISpatialDataSet"/> via 
''' a simple dropdown menu.
''' </summary>
''' ===========================================================================
Public Class cDatasetPicker

#Region " Private classes "

    Private Class cDatasetSorter
        Implements IComparer(Of ISpatialDataSet)

        Public Function Compare(x As EwEUtils.SpatialData.ISpatialDataSet, _
                                y As EwEUtils.SpatialData.ISpatialDataSet) As Integer _
                            Implements System.Collections.Generic.IComparer(Of EwEUtils.SpatialData.ISpatialDataSet).Compare
            Return String.Compare(x.DisplayName, y.DisplayName)
        End Function

    End Class

#End Region ' Private classes

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_cmsData As ContextMenuStrip = Nothing
    Private m_varname As eVarNameFlags = eVarNameFlags.NotSet
    Private m_tag As Object = Nothing

#End Region ' Private vars

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event, notifies the world that a dataset was selected.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="args">Yippee.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnPicked(sender As Object, args As cDatasetPickedArgs)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of the converterpicker
    ''' </summary>
    ''' <param name="uic"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext, ByVal varname As eVarNameFlags)
        Me.m_cmsData = New ContextMenuStrip()
        Me.m_uic = uic
        Me.m_varname = varname
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the dataset picker is allowed to show an option to clear
    ''' the selected dataset without setting a new one.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property AllowClear As Boolean = True

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the dataset picker is allowed to show an option to create 
    ''' new datasets.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property AllowCreateNew As Boolean = True

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start the selection process.
    ''' </summary>
    ''' <param name="control">The user control for placing the dropdown menu.</param>
    ''' <param name="datasets">The list of currently available <see cref="ISpatialDataSet"/>s to pick from.</param>
    ''' <param name="datasetCurrent">The currently selected <see cref="ISpatialDataSet"/>.</param>
    ''' <param name="tag">Optional tag to attach to the pick event.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Pick(ByVal control As Control, _
                    ByVal datasets As ISpatialDataSet(), _
                    ByVal datasetCurrent As ISpatialDataSet, _
                    Optional ByVal tag As Object = Nothing)

        Me.BuildMenu(datasets, datasetCurrent)

        Me.m_tag = tag
        Me.m_cmsData.Show(control, control.Width, 0)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start the selection process.
    ''' </summary>
    ''' <param name="pt">The location for placing the dropdown menu, specified
    ''' in screen coordinates.</param>
    ''' <param name="datasets">The list of currently available <see cref="ISpatialDataSet"/>s to pick from.</param>
    ''' <param name="datasetCurrent">The currently selected <see cref="ISpatialDataSet"/>.</param>
    ''' <param name="tag">Optional tag to attach to the pick event.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Pick(ByVal pt As Point, _
                    ByVal datasets As ISpatialDataSet(), _
                    ByVal datasetCurrent As ISpatialDataSet, _
                    Optional ByVal tag As Object = Nothing)

        Me.BuildMenu(datasets, datasetCurrent)

        Me.m_tag = tag
        Me.m_cmsData.Show(pt, ToolStripDropDownDirection.Default)
    End Sub

    Private Sub BuildMenu(ByVal datasets As ISpatialDataSet(), _
                          ByVal datasetCurrent As ISpatialDataSet)

        Dim tsi As ToolStripMenuItem = Nothing
        Dim tsiNew As ToolStripMenuItem = Nothing
        Dim man As cSpatialDataConnectionManager = Me.m_uic.Core.SpatialDataConnectionManager
        Dim datasetsSorted() As ISpatialDataSet = Nothing

        Me.m_cmsData.Items.Clear()

        Try

            ' No connection item, if allowed
            If (Me.AllowClear) Then
                tsi = New ToolStripMenuItem(My.Resources.HEADER_SPATTEMP_NODRIVER, Nothing, AddressOf OnUseDefault)
                tsi.Checked = (datasetCurrent Is Nothing)
                Me.m_cmsData.Items.Add(tsi)
            End If

            ' Create 'new datasets' popup, if allowed
            If (Me.AllowCreateNew) Then
                datasetsSorted = man.DatasetTemplates
                If (datasetsSorted IsNot Nothing) Then
                    If (datasetsSorted.Length > 0) Then
                        tsiNew = New ToolStripMenuItem(My.Resources.HEADER_SPATTEMP_CREATE_CONNECTION)
                        Array.Sort(datasetsSorted, New cDatasetSorter())
                        For Each dsTempl As ISpatialDataSet In datasetsSorted
                            tsi = New ToolStripMenuItem(dsTempl.DisplayName, Nothing, AddressOf OnCreateDataset)
                            tsi.Tag = dsTempl
                            tsi.ToolTipText = dsTempl.DataDescription
                            tsiNew.DropDownItems.Add(tsi)
                        Next
                        Me.m_cmsData.Items.Add(tsiNew)
                    End If
                End If
            End If

            ' Create menu items for existing datasets
            If (datasets IsNot Nothing) Then
                If (datasets.Length > 0) Then

                    datasetsSorted = DirectCast(datasets.Clone(), ISpatialDataSet())
                    Array.Sort(datasetsSorted, New cDatasetSorter())

                    If (Me.m_cmsData.Items.Count > 0) Then
                        Me.m_cmsData.Items.Add(New ToolStripSeparator())
                    End If

                    For Each ds As ISpatialDataSet In datasetsSorted
                        tsi = New ToolStripMenuItem(ds.DisplayName, Nothing, AddressOf OnUseDataset)
                        tsi.Tag = ds
                        tsi.ToolTipText = ds.DataDescription
                        tsi.Checked = ds.Equals(datasetCurrent)
                        Me.m_cmsData.Items.Add(tsi)
                    Next
                End If
            End If

        Catch ex As Exception
            ' Aargh
        End Try

    End Sub

    Private Sub OnUseDefault(sender As System.Object, e As System.EventArgs)
        Me.DatasetPicked(Nothing)
    End Sub

    Private Sub OnUseDataset(sender As System.Object, e As System.EventArgs)

        Try
            Dim item As ToolStripItem = DirectCast(sender, ToolStripItem)
            Dim ds As ISpatialDataSet = DirectCast(item.Tag, ISpatialDataSet)

            If Not Me.TryConfigure(ds) Then Return

            Me.DatasetPicked(ds)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnCreateDataset(sender As System.Object, e As System.EventArgs)

        Try

            Dim tsi As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
            Dim ds As ISpatialDataSet = DirectCast(tsi.Tag, ISpatialDataSet)

            Try
                ds = DirectCast(Activator.CreateInstance(ds.GetType), ISpatialDataSet)
                ' Only allow configuration for selected variable
                ds.VarName = Me.m_varname
            Catch ex As Exception
                Return
            End Try

            If Not Me.TryConfigure(ds) Then Return

            Me.DatasetPicked(ds)

        Catch ex As Exception

        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Configure a configurable ;)
    ''' </summary>
    ''' <returns>True if successfully configured.</returns>
    ''' -----------------------------------------------------------------------
    Private Function TryConfigure(ByVal ds As ISpatialDataSet) As Boolean

        If (ds Is Nothing) Then Return False

        If (TypeOf ds Is IConfigurable) Then

            Dim cfg As IConfigurable = DirectCast(ds, IConfigurable)
            Dim strTitle As String = ds.DisplayName

            Try
                Dim dlg As New ScientificInterfaceShared.Controls.dlgConfig(Me.m_uic)
                Return (dlg.ShowDialog(cStringUtils.Localize(SharedResources.CAPTION_GENERIC_EDIT, strTitle), cfg.GetConfigUI()) = DialogResult.OK)
            Catch ex As Exception
                ' Whoah!
                Return False
            End Try
        End If

        Return True

    End Function

    Private Sub DatasetPicked(dataset As ISpatialDataSet)

        Try
            RaiseEvent OnPicked(Me, New cDatasetPickedArgs(dataset, Me.m_tag))
        Catch ex As Exception

        End Try
    End Sub

End Class
