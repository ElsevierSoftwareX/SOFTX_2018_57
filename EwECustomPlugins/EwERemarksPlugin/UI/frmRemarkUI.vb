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
Imports System.Windows.Forms
Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Extensions
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' EcoWriter plug-in user interface.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class frmRemarkUI

#Region " Private classes "

    Private Class cSortItem

        Private m_sort As ePropertySortOrderTypes
        Private m_strDisplay As String = ""

        Public Sub New(ByVal strDisplay As String, ByVal sort As ePropertySortOrderTypes)
            Me.m_strDisplay = strDisplay
            Me.m_sort = sort
        End Sub

        Public Overrides Function ToString() As String
            Return Me.m_strDisplay
        End Function

        Public ReadOnly Property Sort As ePropertySortOrderTypes
            Get
                Return Me.m_sort
            End Get
        End Property

    End Class

#End Region ' Private classes

#Region " Private vars "

    Private m_monitor As cRemarkMonitor = Nothing
    Private m_bDataInvalidated As Boolean = False

#End Region ' Private vars

#Region " Constructor "

    Public Sub New(uic As cUIContext)
        MyBase.New()
        Me.InitializeComponent()
        Me.UIContext = uic
        Me.Grid = Me.m_grid

    End Sub

#End Region ' Constructor

#Region " Form overloads "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        ' Populate sort box
        Me.AddSortOption(My.Resources.HEADER_SOURCE, ePropertySortOrderTypes.Source)
        Me.AddSortOption(My.Resources.HEADER_SOURCE_SEC, ePropertySortOrderTypes.SourceSec)
        Me.AddSortOption(My.Resources.HEADER_PARAMETER, ePropertySortOrderTypes.VarName)
        Me.m_tscmbSort.SelectedIndex = 0

        ' Start tracking core state
        AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged

        ' Create selection monitor, start tracking the monitor
        Me.m_monitor = New cRemarkMonitor(Me.PropertyManager)
        AddHandler Me.m_monitor.OnRemarksListChanged, AddressOf OnRemarkListChanged

        ' Chop chop
        Me.Icon = Drawing.Icon.FromHandle(DirectCast(SharedResources.CommentHS, Bitmap).GetHicon)
        Me.InvalidateGrid()

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        ' Stop tracking core state
        RemoveHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged

        ' Get rid of monitor
        RemoveHandler Me.m_monitor.OnRemarksListChanged, AddressOf OnRemarkListChanged
        Me.m_monitor.Dispose()
        Me.m_monitor = Nothing

        Me.Icon.Destroy()
        MyBase.OnFormClosed(e)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to prevent panel from closing with 'close all docs'
    ''' </summary>
    ''' <returns>Cheese!</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function PanelType() As frmEwEDockContent.ePanelType
        Return ePanelType.SystemPanel
    End Function

#End Region ' Form overloads

#Region " Events "

    Private Sub OnSortChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tscmbSort.SelectedIndexChanged
        Me.InvalidateGrid()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Callback for <see cref="cRemarkMonitor.OnRemarksListChanged">Remark monitor
    ''' list change events</see>.
    ''' </summary>
    ''' <param name="monitor">The monitor that fired the event.</param>
    ''' -----------------------------------------------------------------------
    Private Sub OnRemarkListChanged(ByRef monitor As cRemarkMonitor)
        Me.InvalidateGrid()
    End Sub

    Private Sub OnCoreStateChanged(ByVal cms As cCoreStateMonitor)
        Me.UpdateControls()
    End Sub

#End Region ' Events

#Region " Form config helpers "

    ''' <summary>
    ''' Add an item to the sort combo box.
    ''' </summary>
    ''' <param name="strDisplay"></param>
    ''' <param name="sort"></param>
    Private Sub AddSortOption(ByVal strDisplay As String, ByVal sort As ePropertySortOrderTypes)
        Me.m_tscmbSort.Items.Add(New cSortItem(strDisplay, sort))
    End Sub

#End Region ' Form config helpers

#Region " Control handling "

    Private Sub InvalidateGrid()

        If (Me.m_monitor Is Nothing) Then Return
        SyncLock Me
            Me.m_bDataInvalidated = True
        End SyncLock
        Me.BeginInvoke(New MethodInvoker(AddressOf UpdateGrid))

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Delayed grid repopulate logic
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateGrid()

        SyncLock Me
            If Not Me.m_bDataInvalidated Then Return
            Me.m_bDataInvalidated = False
        End SyncLock

        Dim lData As New List(Of cProperty)

        For Each prop As cProperty In Me.m_monitor.Remarks
            If (prop.Source IsNot Nothing) Then
                If (Not prop.Source.Disposed) Then
                    ' ToDo: add filter?
                    lData.Add(prop)
                Else
                    ' Whoah!
                End If
            End If
        Next
        ' Apply sort order
        lData.Sort(New cPropertySorter(DirectCast(Me.m_tscmbSort.SelectedItem, cSortItem).Sort))
        ' Update the grid
        Me.m_grid.SetData(lData.ToArray())

    End Sub

    Protected Overrides Sub UpdateControls()

        Dim bHasModel As Boolean = Me.Core.StateMonitor.HasEcopathLoaded

        Me.m_ts.Enabled = bHasModel
        Me.m_grid.Visible = bHasModel

    End Sub

#End Region ' Control handling

End Class