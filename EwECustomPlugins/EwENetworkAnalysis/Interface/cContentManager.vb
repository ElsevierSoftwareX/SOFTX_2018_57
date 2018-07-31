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
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ZedGraph

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Class for populating NA view controls.
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public MustInherit Class cContentManager

#Region " Private variables "

    ''' <summary></summary>
    Private m_manager As cNetworkManager = Nothing
    ''' <summary></summary>
    Private m_graph As ZedGraphControl = Nothing
    ''' <summary></summary>
    Private m_plot As ucPlot = Nothing
    ''' <summary></summary>
    Private m_datagrid As DataGridView = Nothing
    ''' <summary></summary>
    Private m_toolstrip As ToolStrip = Nothing
    ''' <summary></summary>
    Private m_uic As cUIContext = Nothing

    Private m_groupfilter1 As eGroupFilterTypes = eGroupFilterTypes.Living
    Private m_groupfilter2 As eGroupFilterTypes = eGroupFilterTypes.Living

#End Region ' Private variables

#Region " Attach / detach "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="manager"></param>
    ''' <param name="datagrid"></param>
    ''' <param name="graph"></param>
    ''' <param name="plot"></param>
    ''' <remarks>
    ''' The default implementation stores all controls and hides them.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Attach(ByVal manager As cNetworkManager, _
                                       ByVal datagrid As DataGridView, _
                                       ByVal graph As ZedGraphControl, _
                                       ByVal plot As ucPlot, _
                                       ByVal toolstrip As ToolStrip, _
                                       ByVal uic As cUIContext) As Boolean

        ' Sanity checks
        Debug.Assert(uic IsNot Nothing)

        ' Store all references
        Me.m_manager = manager
        Me.m_datagrid = datagrid
        Me.m_graph = graph
        Me.m_plot = plot
        Me.m_toolstrip = toolstrip
        Me.m_uic = uic

        ' Hide all managed controls
        Me.HideControls()

        AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' The default implementation hides all controls and then releases them.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub Detach()

        ' Hide all controls
        Me.HideControls()

        RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged

        Me.m_manager = Nothing
        Me.m_datagrid = Nothing
        Me.m_graph = Nothing
        Me.m_plot = Nothing
        Me.m_toolstrip = Nothing
        Me.m_uic = Nothing

    End Sub

#End Region ' Attach / detach

#Region " Overrides "

    Public MustOverride Sub DisplayData()

    Public MustOverride Function PageTitle() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update a 'view' to a selection.
    ''' </summary>
    ''' <param name="iGroup1">One-based EwE group index of the first selected 
    ''' group, if any.</param>
    ''' <param name="iGroup2">One-based EwE group index of the second selected 
    ''' group, if any.</param>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub UpdateData(ByVal iGroup1 As Integer, ByVal iGroup2 As Integer)
        ' NOP
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clear managed data.
    ''' </summary>
    ''' <remarks>
    ''' The default implementation hides all controls.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub ClearData()
        ' Hide all controls
        Me.HideControls()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the default file name - without extension - for saving the data 
    ''' managed here to a file of any type.
    ''' </summary>
    ''' <param name="strComponent">Network Analysis component to get the file name for.</param>
    ''' <remarks>
    ''' Default implementation does not return a file name.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Filename(ByVal strComponent As String) As String
        Return IO.Path.Combine(Me.m_uic.Core.DefaultOutputPath(EwEUtils.Core.eAutosaveTypes.Ecopath), strComponent)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implement to save the content of a view to a EMF file.
    ''' </summary>
    ''' <param name="strFileName"></param>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub SaveToEMF(ByVal strFileName As String)
        ' NOP
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Flag stating whether the data being displayed has a time component.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property IsDataOverTime() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Flag stating whether the data being displayed is based on Ecosim.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property UsesEcosim() As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overridable Function OptionsControl() As UserControl
        Return Nothing
    End Function

#End Region ' Overrides

#Region " Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the one and only network manager.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property NetworkManager() As cNetworkManager
        Get
            Return Me.m_manager
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the one and only data grid view control.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property Grid() As DataGridView
        Get
            Return Me.m_datagrid
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the one and only graph control.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property Graph() As ZedGraphControl
        Get
            Return Me.m_graph
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the one and only plot control.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property Plot() As ucPlot
        Get
            Return Me.m_plot
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the one and only tool strip.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property Toolstrip() As ToolStrip
        Get
            Return Me.m_toolstrip
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the one and only ui context.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property UIContext() As cUIContext
        Get
            Return Me.m_uic
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the style guide provided by the UI context.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property StyleGuide() As cStyleGuide
        Get
            Return Me.m_uic.StyleGuide
        End Get
    End Property

    Public ReadOnly Property GroupFilter1() As eGroupFilterTypes
        Get
            Return Me.m_groupfilter1
        End Get
    End Property

    Public ReadOnly Property GroupFilter2() As eGroupFilterTypes
        Get
            Return Me.m_groupfilter2
        End Get
    End Property

#End Region ' Properties

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Hide all controls. Override this to do your own magic.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overridable Sub HideControls()

        ' Hide all controls
        Me.Graph.Visible = False
        Me.Plot.Visible = False
        Me.Grid.Visible = False
        Me.Toolstrip.Visible = True ' False

        ' Clear grid
        Me.Grid.Rows.Clear()
        Me.Grid.Columns.Clear()
        Me.Grid.ReadOnly = True

        ' Hide toolstrip items
        For Each tsi As ToolStripItem In Me.Toolstrip.Items
            Select Case tsi.Name
                Case "tsmiRun", "tsbtnOptions", "tsbnFonts"
                    tsi.Visible = True
                Case Else
                    tsi.Visible = False
            End Select
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, responding to styleguide changes.
    ''' </summary>
    ''' <param name="cf"></param>
    ''' -----------------------------------------------------------------------
    Protected Overridable Sub OnStyleGuideChanged(ByVal cf As cStyleGuide.eChangeType)

        Dim bContentVisible As Boolean = False

        Try

            If (Me.Graph IsNot Nothing) Then bContentVisible = bContentVisible Or Me.Graph.Visible
            If (Me.Grid IsNot Nothing) Then bContentVisible = bContentVisible Or Me.Grid.Visible
            If (Me.Plot IsNot Nothing) Then bContentVisible = bContentVisible Or Me.Plot.Visible

            If (bContentVisible) Then
                Me.DisplayData()
            End If

        Catch ex As Exception

        End Try

    End Sub

    Protected Sub ToolstripShowGroupSelections(Optional ByVal strLabel1 As String = "", _
                                               Optional ByVal groupfilter1 As eGroupFilterTypes = eGroupFilterTypes.Living, _
                                               Optional ByVal strLabel2 As String = "", _
                                               Optional ByVal groupfilter2 As eGroupFilterTypes = eGroupFilterTypes.Living)

        Dim tslbl1 As ToolStripItem = Me.Toolstrip.Items("tslblSelection1")
        Dim tslbl2 As ToolStripItem = Me.Toolstrip.Items("tslblSelection2")
        Dim tscmb1 As ToolStripItem = Me.Toolstrip.Items("tscmbSelection1")
        Dim tscmb2 As ToolStripItem = Me.Toolstrip.Items("tscmbSelection2")

        tslbl1.Text = strLabel1
        tslbl1.Visible = Not String.IsNullOrEmpty(strLabel1)
        tscmb1.Visible = Not String.IsNullOrEmpty(strLabel1)

        tslbl2.Text = strLabel2
        tslbl2.Visible = Not String.IsNullOrEmpty(strLabel2)
        tscmb2.Visible = Not String.IsNullOrEmpty(strLabel2)

        Me.m_groupfilter1 = groupfilter1
        Me.m_groupfilter2 = groupfilter2

    End Sub

    Protected Sub ToolstripShowDisplayGroups(Optional ByVal bShow As Boolean = True)
        Dim tsi As ToolStripItem = Me.Toolstrip.Items("tsmiDisplayGroups")
        If (tsi IsNot Nothing) Then
            tsi.Visible = bShow
        End If
    End Sub

    Protected Sub ToolstripShowOptionCSV(Optional ByVal bShow As Boolean = True)
        Dim tsi As ToolStripItem = Me.Toolstrip.Items("tsbtnOutputIndicesCSV")
        If (tsi IsNot Nothing) Then
            tsi.Visible = bShow
        End If
    End Sub

    Protected Sub ToolstripShowOptionEMF(Optional ByVal bShow As Boolean = True)
        Dim tsi As ToolStripItem = Me.Toolstrip.Items("tsbtnOutputGraphEMF")
        If (tsi IsNot Nothing) Then
            tsi.Visible = bShow
        End If
    End Sub

    Protected Sub ToolstripShowOptionOptions(Optional ByVal bShow As Boolean = True)
        Dim tsi As ToolStripItem = Me.Toolstrip.Items("tsbtnOptions")
        If (tsi IsNot Nothing) Then
            tsi.Visible = bShow
        End If
    End Sub

    Protected Sub SendMessage(ByVal strMessage As String, _
                              Optional ByVal importance As eMessageImportance = eMessageImportance.Critical)
        Dim msg As New cMessage(strMessage, eMessageType.Any, EwEUtils.Core.eCoreComponentType.External, importance)
        Me.NetworkManager.Core.Messages.SendMessage(msg)
    End Sub

#End Region ' Internals

End Class
