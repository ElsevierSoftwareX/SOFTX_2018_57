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
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Monitor class for currently selected data.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cSelectionMonitor

    Private m_cmdSelect As cPropertySelectionCommand = Nothing

    Public Sub New()
        ' NOP
    End Sub

    Public Sub Attach(uic As cUIContext)

        ' Sanity checks
        Debug.Assert(Me.m_cmdSelect Is Nothing)
        Debug.Assert(uic IsNot Nothing)

        ' Start monitoring
        Me.m_cmdSelect = DirectCast(uic.CommandHandler.GetCommand(cPropertySelectionCommand.COMMAND_NAME), cPropertySelectionCommand)
        AddHandler Me.m_cmdSelect.OnPostInvoke, AddressOf HandleSelectionChanged

    End Sub

    Public Sub Detach()

        ' Sanity checks
        Debug.Assert(Me.m_cmdSelect IsNot Nothing)

        ' Stop monitoring
        RemoveHandler Me.m_cmdSelect.OnPostInvoke, AddressOf HandleSelectionChanged
        Me.m_cmdSelect = Nothing

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns an array of currently selected properties.
    ''' </summary>
    ''' <returns>An array of currently selected <see cref="cProperty">properties</see>.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Selection() As cProperty()
        If (Me.m_cmdSelect IsNot Nothing) Then
            Return Me.m_cmdSelect.Selection
        End If
        Return New cProperty() {}
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Selection change notification
    ''' </summary>
    ''' <param name="sender"></param>
    ''' -----------------------------------------------------------------------
    Event OnSelectionChanged(sender As cSelectionMonitor)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="OnSelectionChanged">Selection change event</see> dispatch.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub HandleSelectionChanged(cmd As cCommand)
        Try
            RaiseEvent OnSelectionChanged(Me)
        Catch ex As Exception

        End Try
    End Sub

End Class
