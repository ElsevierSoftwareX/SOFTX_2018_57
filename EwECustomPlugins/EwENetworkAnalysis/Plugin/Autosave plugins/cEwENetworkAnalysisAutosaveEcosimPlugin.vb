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

Imports System.IO
Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in point to connect ENA to the EwE Autosave system. This plug-in point
''' manages auto-saving of Ecosim indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEwENetworkAnalysisAutosaveEcosimPlugin
    Implements IAutoSavePlugin

    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "ndENAAutosaveSim"
        End Get
    End Property

    Public Sub Initialize(core As Object) Implements IPlugin.Initialize
    End Sub

    Public ReadOnly Property Description As String Implements IPlugin.Description
        Get
            Return "Ecosim-based autosave functionality for the EwE Ecological Network Analysis plug-in"
        End Get
    End Property

    Public ReadOnly Property Author As String Implements IPlugin.Author
        Get
            Return "EwE development team"
        End Get
    End Property

    Public ReadOnly Property Contact As String Implements IPlugin.Contact
        Get
            Return "ewedevteam@gmail.com"
        End Get
    End Property

    Public Property AutoSave As Boolean Implements IAutoSavePlugin.AutoSave
        Get
            Dim pi As cEwENetworkAnalysisPlugin = cEwENetworkAnalysisPlugin.thePlugin
            Return pi.Autosave(cEwENetworkAnalysisPlugin.eAutosaveType.Ecosim)
        End Get
        Set(value As Boolean)
            Dim pi As cEwENetworkAnalysisPlugin = cEwENetworkAnalysisPlugin.thePlugin
            pi.Autosave(cEwENetworkAnalysisPlugin.eAutosaveType.Ecosim) = value
        End Set
    End Property

    Public Function AutoSaveName() As String Implements IAutoSavePlugin.AutoSaveName
        Return My.Resources.CAPTION
    End Function

    Public Function AutoSaveType() As eAutosaveTypes Implements IAutoSavePlugin.AutoSaveType
        Return eAutosaveTypes.Ecosim
    End Function

    Public Function AutoSaveOutputPath() As String Implements IAutoSavePlugin.AutoSaveOutputPath
        Return ""
    End Function

End Class
