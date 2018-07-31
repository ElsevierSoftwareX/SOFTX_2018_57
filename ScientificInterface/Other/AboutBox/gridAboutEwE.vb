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
Imports EwEPlugin
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports System.Reflection
Imports SourceGrid2

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Grid showing loaded EwE assembly details.
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public Class gridAboutEwE
    Inherits EwEGrid

    Private m_bShowEwEComponentsOnly As Boolean = True

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Populate the grid with data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub FillData()

        Dim pm As cPluginManager = Nothing
        Dim aanCore As AssemblyName() = Nothing
        Dim aanRef As AssemblyName() = Nothing
        Dim aanFramework As AssemblyName() = Nothing
        Dim aanPlugins As AssemblyName() = Nothing
        Dim iRow As Integer = 0

        aanCore = cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.EwECore)
        pm = Me.UIContext.Core.PluginManager()
        aanPlugins = pm.PluginAssemblyNames()

        ' Prepare grid
        If Me.m_bShowEwEComponentsOnly Then
            Me.Redim(aanCore.Length + 1 + aanPlugins.Length + 1, 2)
        Else
            aanRef = cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.Referenced)
            aanFramework = cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.Framework)
            Me.Redim(aanCore.Length + 1 + aanPlugins.Length + 1 + aanRef.Length + 1 + aanFramework.Length + 1, 2)
        End If

        ' -- Core section --

        Me(iRow, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_COMPONENTS_EWE)
        Me(iRow, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_VERSION)
        iRow += 1
        For Each an As AssemblyName In aanCore
            Me(iRow, 0) = New EwERowHeaderCell(an.Name)
            Me(iRow, 1) = New EwECell(an.Version.ToString, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
            ' Next
            iRow += 1
        Next

        ' -- Plug-in section
        Me(iRow, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_COMPONENTS_PLUGINS)
        Me(iRow, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_VERSION)
        iRow += 1
        For Each an As AssemblyName In aanPlugins
            Me(iRow, 0) = New EwERowHeaderCell(an.Name)
            Me(iRow, 1) = New EwECell(an.Version.ToString, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
            iRow += 1
        Next

        If Not Me.m_bShowEwEComponentsOnly Then

            ' -- Referenced section --
            Me(iRow, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_COMPONENTS_REFERENCED)
            Me(iRow, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_VERSION)
            iRow += 1
            For Each an As AssemblyName In aanRef
                Me(iRow, 0) = New EwERowHeaderCell(an.Name)
                Me(iRow, 1) = New EwECell(an.Version.ToString, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                ' Next
                iRow += 1
            Next

            ' -- Framework section --
            Me(iRow, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_COMPONENTS_FRAMEWORK)
            Me(iRow, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_VERSION)
            iRow += 1
            For Each an As AssemblyName In aanFramework
                Me(iRow, 0) = New EwERowHeaderCell(an.Name)
                Me(iRow, 1) = New EwECell(an.Version.ToString, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                ' Next
                iRow += 1
            Next

        End If

        ' Column 1 w version numbers must be fully visible. Column 0 will occupy the rest of the space
        Me.Columns(0).AutoSizeMode = SourceGrid2.AutoSizeMode.None
        Me.Columns(1).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize Or SourceGrid2.AutoSizeMode.EnableStretch
        Me.FitColumns()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Grid resize: resize the columns
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
        MyBase.OnResize(e)
        Me.FitColumns()
    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.FixedColumnWidths = False
    End Sub

    Private Sub FitColumns()
        If Me.ColumnsCount > 0 Then
            Me.AutoSizeAll()
            Dim iWidth As Integer = Me.ClientRectangle.Width - Me.Columns(1).Width - 2
            If (Me.VScrollBar IsNot Nothing) Then
                iWidth -= Me.VScrollBar.Width
            End If
            Me.Columns(0).Width = iWidth
        End If
    End Sub

    Public Property ShowEwEComponentsOnly As Boolean
        Get
            Return Me.m_bShowEwEComponentsOnly
        End Get
        Set(value As Boolean)
            If (value <> Me.m_bShowEwEComponentsOnly) Then
                Me.m_bShowEwEComponentsOnly = value
                Me.RefreshContent()
            End If
        End Set
    End Property

End Class
