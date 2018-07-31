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

Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Commands

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Command to launch a the interface to select which groups and fleets to
    ''' display on graphs.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cDisplayGroupsCommand
        Inherits cCommand

        Private m_groupOptions As eGroupDisplayOptions = eGroupDisplayOptions.All

        Public Shared cCOMMAND_NAME As String = "~displaygroups"

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Group display options.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eGroupDisplayOptions As Integer
            ''' <summary>Show fished groups.</summary>
            Fished = &H1
            ''' <summary>Show non-fished groups.</summary>
            NonFished = &H2
            ''' <summary>Show living groups.</summary>
            Living = &H4
            ''' <summary>Show non-living groups.</summary>
            NonLiving = &H8
            ''' <summary>Show producer groups.</summary>
            Producers = &H10
            ''' <summary>Show consumer groups.</summary>
            Consumers = &H20
            ''' <summary>Show detritus groups.</summary>
            Detritus = &H40
            ''' <summary>Show stanza groups.</summary>
            Stanza = &H80
            ''' <summary>Show non-stanza groups.</summary>
            NonStanza = &H100
            ''' <summary>Show all groups.</summary>
            All = &HFF
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="cmdh">The <see cref="cCommandHandler"/> to associate this command with.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, cDisplayGroupsCommand.cCOMMAND_NAME, My.Resources.COMMAND_DISPLAYGROUPS)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Launch the commands with a given set of group display options.
        ''' </summary>
        ''' <param name="groupOptions"></param>
        ''' -------------------------------------------------------------------
        Public Overloads Sub Invoke(groupOptions As eGroupDisplayOptions)
            Me.m_groupOptions = groupOptions
            MyBase.Invoke()
            Me.m_groupOptions = eGroupDisplayOptions.All
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eGroupDisplayOptions">group display options</see>
        ''' that the command was invoked with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property GroupDisplayOptions As eGroupDisplayOptions
            Get
                Return Me.m_groupOptions
            End Get
        End Property
    End Class

End Namespace
