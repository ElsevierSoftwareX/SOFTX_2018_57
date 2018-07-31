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
Imports EwECore.Database
Imports System.Drawing
Imports System.Windows.Forms
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class frmMain

#Region " Vars "

    ''' <summary>
    ''' The pages supported by the value chain.
    ''' </summary>
    Public Enum eValueChainPageTypes As Integer
        NotSet = 0
        Parameters
        Flow
        Defaults
        TableProducers
        TableProcessors
        TableDistributors
        TableWholesellers
        TableRetailer
        TableConsumers
        TableLinks
        TableLandingsLinks
        Run
        FlowDiagram
    End Enum

    Private m_plugin As cValueChainPlugin = Nothing
    Private m_pageCurrent As eValueChainPageTypes = eValueChainPageTypes.NotSet
    Private m_bInUpdate As Boolean = False

#End Region ' Vars

#Region " Constructor "

    Public Sub New(ByVal plugin As cValueChainPlugin)

        Me.InitializeComponent()

        Me.m_plugin = plugin

        Me.Text = My.Resources.GENERIC_CAPTION
        Me.TabText = My.Resources.GENERIC_CAPTION

    End Sub

#End Region ' Constructor

#Region " Public interfaces "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Switch to a form within the value chain plug-in with a given name.
    ''' </summary>
    ''' <param name="page">Indicator of the page to show.</param>
    ''' -----------------------------------------------------------------------
    Public Sub ShowForm(ByVal page As eValueChainPageTypes)

        If Me.m_pageCurrent = page Then Return
        If Me.m_bInUpdate Then Return

        Me.m_bInUpdate = True
        Me.m_pageCurrent = page

        Select Case Me.m_pageCurrent
            Case eValueChainPageTypes.Parameters
                Me.ShowForm(New ucParameters(Me.m_plugin.Data, Me.m_plugin.Context), My.Resources.NAVTREE_INPUT_PARAMETERS)
            Case eValueChainPageTypes.TableProducers
                Dim grid As New ucUnitGrid(Me.m_plugin.Context, Me.m_plugin.Data, cUnitFactory.eUnitType.Producer)
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_PRODUCERS)
            Case eValueChainPageTypes.TableProcessors
                Dim grid As New ucUnitGrid(Me.m_plugin.Context, Me.m_plugin.Data, cUnitFactory.eUnitType.Processing)
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_PROCESSORS)
            Case eValueChainPageTypes.TableDistributors
                Dim grid As New ucUnitGrid(Me.m_plugin.Context, Me.m_plugin.Data, cUnitFactory.eUnitType.Distribution)
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_DISTRIBUTORS)
            Case eValueChainPageTypes.TableWholesellers
                Dim grid As New ucUnitGrid(Me.m_plugin.Context, Me.m_plugin.Data, cUnitFactory.eUnitType.Wholesaler)
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_WHOLESALERS)
            Case eValueChainPageTypes.TableRetailer
                Dim grid As New ucUnitGrid(Me.m_plugin.Context, Me.m_plugin.Data, cUnitFactory.eUnitType.Retailer)
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_RETAILERS)
            Case eValueChainPageTypes.TableConsumers
                Dim grid As New ucUnitGrid(Me.m_plugin.Context, Me.m_plugin.Data, cUnitFactory.eUnitType.Consumer)
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_CONSUMERS)
            Case eValueChainPageTypes.Flow
                Me.ShowForm(New ucEditFlow(Me.m_plugin.Context, Me.m_plugin.Data, Me.m_plugin.Data.FlowDiagram(0)), My.Resources.NAVTREE_INPUT_FLOW)
            Case eValueChainPageTypes.Defaults
                Me.ShowForm(New ucDefaults(Me.m_plugin.Context, Me.m_plugin.Data), My.Resources.NAVTREE_INPUT_DEFAULTS)
            Case eValueChainPageTypes.TableLinks
                Dim grid As New ucLinkGrid(Me.m_plugin.Context, Me.m_plugin.Data, GetType(cLink))
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_LINKS)
            Case eValueChainPageTypes.TableLandingsLinks
                Dim grid As New ucLinkGrid(Me.m_plugin.Context, Me.m_plugin.Data, GetType(cLinkLandings))
                Dim view As New ucGridView(grid)
                Me.ShowForm(view, My.Resources.NAVTREE_INPUT_TABLE_LANDINGLINKS)
            Case eValueChainPageTypes.Run
                Me.ShowForm(New ucResults(Me.m_plugin.Context, Me.m_plugin.Data, Me.m_plugin.Model, Me.m_plugin.Results), My.Resources.NAVTREE_OUTPUT_RUN)
            Case eValueChainPageTypes.FlowDiagram
                Me.ShowForm(New ucFlowDiagram(Me.m_plugin.Context, Me.m_plugin.Data, Me.m_plugin.Model, Me.m_plugin.Results), My.Resources.NAVTREE_OUTPUT_FLOWDIAGRAM)
            Case Else
                Debug.Assert(False)
        End Select
        Me.m_bInUpdate = False

    End Sub

#End Region ' Public interfaces

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Translate pageless node names to valid pages.
    ''' </summary>
    ''' <param name="strFormName"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function ResolveFormName(ByVal strFormName As String) As String
        Select Case strFormName
            Case "" : Return "ndParameters"
            Case "ndTables" : Return "ndProducer"
        End Select
        Return strFormName
    End Function

    Private Sub ShowForm(ByVal f As Control, ByVal strTitle As String)

        Debug.Assert(f IsNot Nothing)

        Dim ctrl As Control = Nothing

        Me.SuspendLayout()

        Try
            If TypeOf f Is IUIElement Then
                DirectCast(f, IUIElement).UIContext = Me.m_plugin.Context
            End If

            f.Dock = DockStyle.Fill
            While Me.Controls.Count > 0
                ctrl = Me.Controls(0)
                Me.Controls.Remove(ctrl)
                ctrl.Dispose()
            End While

            Me.Controls.Add(f)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Me.TabText = String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_INDEXED, My.Resources.GENERIC_CAPTION, strTitle)
        Me.ResumeLayout()

    End Sub

#End Region ' Event handlers

End Class