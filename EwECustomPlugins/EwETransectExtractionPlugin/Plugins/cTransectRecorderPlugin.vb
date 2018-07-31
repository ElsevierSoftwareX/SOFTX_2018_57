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
Imports EwEPlugin

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in point that records transect summaries from Ecospace time step data.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTransectRecorderPlugin
    Implements IEcospaceInitRunCompletedPlugin
    Implements IEcospaceRunInvalidatedPlugin
    Implements IEcospaceRunCompletedPlugin

#Region " Private vars "

    Private m_core As cCore = Nothing
    Private m_data As cTransectDatastructures = Nothing
    Private m_dgt As cCore.EcoSpaceInterfaceDelegate = Nothing

#End Region ' Private vars

    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "Transect recorder"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements IPlugin.Description
        Get
            Return "This plug-in triggers recording of transect summary data when Ecospace is ran"
        End Get
    End Property

    Public ReadOnly Property Author As String Implements IPlugin.Author
        Get
            Return "Jeroen Steenbeek"
        End Get
    End Property

    Public ReadOnly Property Contact As String Implements IPlugin.Contact
        Get
            Return "ewedevteam@gmail.com"
        End Get
    End Property

    Public Sub Initialize(core As Object) Implements IPlugin.Initialize
        Me.m_core = CType(core, cCore)
        Me.m_data = cTransectDatastructures.Instance(Me.m_core)
    End Sub

    Public Sub EcospaceInitRunCompleted(EcospaceDatastructures As Object) _
        Implements IEcospaceInitRunCompletedPlugin.EcospaceInitRunCompleted

        For Each t As cTransect In Me.m_data.Transects
            t.InitRun(Me.m_core)
        Next

        ' Add Ecospace delegate to receive access to Ecospace time step results
        Me.m_dgt = New cCore.EcoSpaceInterfaceDelegate(AddressOf OnEcospaceTimeStep)
        Me.m_core.AddEcospaceTimeStepHandler(Me.m_dgt)

    End Sub

    Public Sub EcospaceRunInvalidated() _
        Implements IEcospaceRunInvalidatedPlugin.EcospaceRunInvalidated

        ' Clear out cached transect summaries
        For Each t As cTransect In Me.m_data.Transects
            t.Invalidate()
        Next

    End Sub

    Private Sub OnEcospaceTimeStep(ByRef data As cEcospaceTimestep)
        ' Ecospaec callback: record summaries
        For Each t As cTransect In Me.m_data.Transects
            t.Record(data)
        Next
    End Sub

    Public Sub EcospaceRunCompleted(EcoSpaceDatastructures As Object) _
        Implements IEcospaceRunCompletedPlugin.EcospaceRunCompleted

        ' Remove delegate at the end of an Ecospace run
        Me.m_core.RemoveEcospaceTimeStepHandler(Me.m_dgt)
        Me.m_dgt = Nothing

    End Sub

End Class
