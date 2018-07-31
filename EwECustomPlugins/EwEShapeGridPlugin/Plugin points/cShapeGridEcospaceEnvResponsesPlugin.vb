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
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class cShapeGridEcospaceEnvResponsesPlugin
    Inherits cShapeGridPlugin

    Public Overrides ReadOnly Property ControlText() As String
        Get
            Return My.Resources.GRID_FNRESPONSES
        End Get
    End Property

    Public Overrides ReadOnly Property ControlTooltipText() As String
        Get
            Return Me.ControlText
        End Get
    End Property

    Public Overrides ReadOnly Property Description() As String
        Get
            Return My.Resources.DESC_HABCAP
        End Get
    End Property

    Public Overrides ReadOnly Property Name() As String
        Get
            ' Sort at the end of it all
            Return "ndXForagingResponseGridSpace"
        End Get
    End Property

    Public Overrides ReadOnly Property NavigationTreeItemLocation() As String
        Get
            Return "ndSpatialDynamic\ndEcospaceInput\ndHabCap"
        End Get
    End Property

    Friend Overrides Function GridType() As Type
        Return GetType(gridFunctionalResponses)
    End Function

    Public Overrides ReadOnly Property EnabledState As EwEUtils.Core.eCoreExecutionState
        Get
            Return EwEUtils.Core.eCoreExecutionState.EcospaceLoaded
        End Get
    End Property

End Class
