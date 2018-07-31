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
Option Explicit On

Imports EwECore.SpatialData
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Arguments for a <see cref="cConverterPicker.OnPicked">converter selection event</see>.
''' </summary>
''' ===========================================================================
Public Class cConverterPickedArgs
    Inherits EventArgs

#Region " Private vars "

    Private m_converter As ISpatialDataConverter = Nothing
    Private m_tag As Object = Nothing

#End Region ' Private vars

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="cv">The selected <see cref="ISpatialDataConverter"/>.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub New(ByVal cv As ISpatialDataConverter, ByVal tag As Object)
        Me.m_converter = cv
        Me.m_tag = tag
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the selected <see cref="ISpatialDataConverter"/>, or Nothing if a selection was cleared.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Converter As ISpatialDataConverter
        Get
            Return Me.m_converter
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the tag attached to the pick event.
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
''' Interface to select and configure a <see cref="ISpatialDataConverter"/> via 
''' a simple dropdown menu.
''' </summary>
''' ===========================================================================
Public Class cConverterPicker

#Region " Private classes "

    Private Class cConverterSorter
        Implements IComparer(Of ISpatialDataConverter)

        Public Function Compare(x As EwEUtils.SpatialData.ISpatialDataConverter, _
                                y As EwEUtils.SpatialData.ISpatialDataConverter) As Integer _
                            Implements System.Collections.Generic.IComparer(Of EwEUtils.SpatialData.ISpatialDataConverter).Compare
            Return String.Compare(x.DisplayName, y.DisplayName)
        End Function
    End Class

#End Region ' Private classes

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_cmsData As ContextMenuStrip = Nothing
    Private m_man As cSpatialDataConnectionManager = Nothing
    Private m_tag As Object = Nothing

#End Region ' Private vars

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event, notifies the world that a converter was selected.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="args">Yippee.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnPicked(ByVal sender As Object, ByVal args As cConverterPickedArgs)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of the converterpicker
    ''' </summary>
    ''' <param name="uic"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext)
        Me.m_cmsData = New ContextMenuStrip()
        Me.m_man = uic.Core.SpatialDataConnectionManager
        Me.m_uic = uic
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start the selection process.
    ''' </summary>
    ''' <param name="control">The user control for placing the dropdown menu.</param>
    ''' <param name="dataset">The <see cref="ISpatialDataSet"/> to pick a <see cref="ISpatialDataConverter"/> for.</param>
    ''' <param name="converterCurrent">The currently selected <see cref="ISpatialDataConverter"/>.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Pick(ByVal control As Control, _
                    ByVal dataset As ISpatialDataSet, _
                    ByVal converterCurrent As ISpatialDataConverter, _
                    Optional tag As Object = Nothing)

        Me.BuildMenu(dataset, converterCurrent)
        Me.m_tag = tag
        Me.m_cmsData.Show(control, control.Width, 0)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start the selection process.
    ''' </summary>
    ''' <param name="pt">The point to place the dropdown menu at.</param>
    ''' <param name="dataset">The <see cref="ISpatialDataSet"/> to pick a <see cref="ISpatialDataConverter"/> for.</param>
    ''' <param name="converterCurrent">The currently selected <see cref="ISpatialDataConverter"/>.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Pick(ByVal pt As Point, _
                    ByVal dataset As ISpatialDataSet, _
                    ByVal converterCurrent As ISpatialDataConverter, _
                    Optional tag As Object = Nothing)

        Me.BuildMenu(dataset, converterCurrent)
        Me.m_tag = tag
        Me.m_cmsData.Show(pt, ToolStripDropDownDirection.Default)

    End Sub

#Region " Internals "

    Private Sub BuildMenu(ByVal dataset As ISpatialDataSet, _
                          ByVal converterCurrent As ISpatialDataConverter)

        Dim tsi As ToolStripMenuItem = Nothing
        Dim iNumAdded As Integer = 0
        Dim templates() As ISpatialDataConverter = Me.m_man.ConverterTemplates(dataset)

        Me.m_cmsData.Items.Clear()

        Array.Sort(templates, New cConverterSorter())
        For Each conv As ISpatialDataConverter In templates
            tsi = New ToolStripMenuItem("", Nothing, AddressOf OnUseConverter)

            If (converterCurrent IsNot Nothing) Then
                If (conv.GetType().Equals(converterCurrent.GetType())) Then
                    conv = converterCurrent
                    tsi.Checked = True
                End If
            End If

            tsi.Text = conv.DisplayName
            tsi.Tag = conv
            tsi.ToolTipText = DirectCast(conv, ISpatialDataConverter).Description

            ' Add link to Dataset for config purposes
            conv.Dataset = dataset
            Me.m_cmsData.Items.Add(tsi)
            iNumAdded += 1
        Next

        If (iNumAdded = 0) Then
            Me.m_cmsData.Items.Add("Oh my, no compatible converters found!")
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, called in response to a converter selection.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnUseDefault(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.ConverterPicked(Nothing)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, called in response to a converter selection.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnUseConverter(sender As System.Object, e As System.EventArgs)

        Dim item As ToolStripItem = DirectCast(sender, ToolStripItem)
        Dim conv As ISpatialDataConverter = DirectCast(item.Tag, ISpatialDataConverter)

        ' Can converter be configured?
        If (TypeOf conv Is IConfigurable) Then
            ' #Yes: only resume when configuration correctly finished
            If Not Me.Configure(DirectCast(conv, IConfigurable), conv.DisplayName) Then Return
        End If

        ' Let world know
        Me.ConverterPicked(conv)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Configure a configurable ;)
    ''' </summary>
    ''' <param name="cfg">The <see cref="IConfigurable"/> to configure.</param>
    ''' <param name="strTitle">The title to provide.</param>
    ''' <returns>True if successfully configured.</returns>
    ''' -----------------------------------------------------------------------
    Private Function Configure(ByVal cfg As IConfigurable, ByVal strTitle As String) As Boolean

        Try
            Dim dlg As New ScientificInterfaceShared.Controls.dlgConfig(Me.m_uic)
            Return (dlg.ShowDialog(cStringUtils.Localize(SharedResources.CAPTION_GENERIC_EDIT, strTitle), _
                                   cfg.GetConfigUI()) = DialogResult.OK)
        Catch ex As Exception
            ' Whoah!
        End Try
        Return False

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Let the world know that a converter has been selected.
    ''' </summary>
    ''' <param name="converter">The converter that has been selected.</param>
    ''' -----------------------------------------------------------------------
    Private Sub ConverterPicked(converter As ISpatialDataConverter)

        Try
            RaiseEvent OnPicked(Me, New cConverterPickedArgs(converter, Me.m_tag))
        Catch ex As Exception

        End Try

    End Sub

#End Region ' Internals

End Class
