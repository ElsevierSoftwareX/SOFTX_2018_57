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
Imports System.Drawing.Drawing2D
Imports System.Drawing
Imports System.ComponentModel
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This control class implements a toolbar for controlling a 
    ''' <see cref="ucMediationAssignments"/> control.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Class ucMediationAssignmentsToolbar

#Region " Variables "

        Private m_handler As cMediationShapeGUIHandler = Nothing

#End Region ' Variables

#Region " Constructors "

        Public Sub New()
            InitializeComponent()
        End Sub

#End Region ' Constructors

#Region " Properties "

        Public Property Handler() As cMediationShapeGUIHandler
            Get
                Return Me.m_handler
            End Get
            Set(ByVal value As cMediationShapeGUIHandler)
                Me.m_handler = value
                Me.UpdateControls()
            End Set
        End Property

        Public Property IsMenuVisible() As Boolean
            Get
                Return Me.m_tsMenus.Visible
            End Get
            Set(ByVal value As Boolean)
                Me.m_tsMenus.Visible = value
            End Set
        End Property

        Public Property DefineMediationLabel() As String
            Get
                Return Me.m_tsbnDefineMediatingItems.Text
            End Get
            Set(ByVal value As String)
                Me.m_tsbnDefineMediatingItems.Text = value
            End Set
        End Property

#End Region ' Properties

#Region " Public interfaces "

        Public Overrides Sub Refresh()
            MyBase.Refresh()
            Me.UpdateControls()
        End Sub

#End Region ' Public interfaces

#Region " Event handlers "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.m_tsbnViewAsPie.Checked = True
            Me.UpdateControls()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
                ' Release handler
                Me.Handler = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub OnDefineXAxis(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnDefineMediatingItems.Click
            If Me.Handler IsNot Nothing Then Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.DefineMediation)
        End Sub

        Private Sub OnViewAsBar(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnViewAsBar.Click
            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ViewMode, Nothing, ucMediationAssignments.eViewModeTypes.Bar)
                Me.m_tsbnViewAsBar.Checked = True
                Me.m_tsbnViewAsPie.Checked = False
            End If
        End Sub

        Private Sub OnViewAsPie(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnViewAsPie.Click
            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ViewMode, Nothing, ucMediationAssignments.eViewModeTypes.Pie)
                Me.m_tsbnViewAsBar.Checked = False
                Me.m_tsbnViewAsPie.Checked = True
            End If
        End Sub

#End Region ' Event handlers

#Region " Internal implementation "

        Private m_bInUpdate As Boolean = False

        Private Sub UpdateControls()

            If (Me.Handler Is Nothing) Then Return

            Dim bShowViewMode As Boolean = Me.Handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.ViewMode)
            Dim bEnableViewMode As Boolean = Me.Handler.EnableCommand(cShapeGUIHandler.eShapeCommandTypes.ViewMode)

            Me.m_tsMenus.SuspendLayout()

            Me.m_tsbnViewAsBar.Visible = bShowViewMode
            Me.m_tsbnViewAsBar.Enabled = bEnableViewMode

            Me.m_tsbnViewAsPie.Visible = bShowViewMode
            Me.m_tsbnViewAsPie.Enabled = bEnableViewMode

            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.DefineMediation, m_tsbnDefineMediatingItems)

            Me.m_tsMenus.ResumeLayout(True)

        End Sub

        Private Sub UpdateCommand(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes, ByVal tsi As ToolStripItem)
            If (Me.m_handler Is Nothing) Then Return
            If Me.m_handler.SupportCommand(cmd) Then
                tsi.Visible = True
                tsi.Enabled = (m_handler.EnableCommand(cmd))
            Else
                tsi.Visible = False
            End If
        End Sub

#End Region ' Internal implementation

    End Class

End Namespace