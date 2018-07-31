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
Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' IGUIPlugin, interface for implementing <see cref="IPlugin">plugins</see> that
''' must be accessible from a Windows GUI.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IGUIPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get an image to show in the control for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property ControlImage() As Image

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the item text to display in the control for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property ControlText() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the tool tip text to display for the control for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property ControlTooltipText() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler that will be called when the control for this plug-in
    ''' is clicked or activated.
    ''' </summary>
    ''' <param name="sender">The control that was clicked or activated.</param>
    ''' <param name="e">Event parameters pertaining the control.</param>
    ''' <param name="frmPlugin">A reference to the form that the plug-in creates
    ''' or activates in response to this event.</param>
    ''' -----------------------------------------------------------------------
    Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As Form)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get must meet to allow this plug-in to run. All GUI controls attached
    ''' to this plug-in will be enabled and disabled in tune with this state.
    ''' </summary>
    ''' <returns>A eCoreExecutionState value, or 0 if this plug-in should be accessible anytime.</returns>
    ''' <remarks>See EwECore/Core/cCoreStateMonitor.eCoreExecutionState for possible values.</remarks>
    ''' -----------------------------------------------------------------------
    ReadOnly Property EnabledState() As eCoreExecutionState

End Interface
