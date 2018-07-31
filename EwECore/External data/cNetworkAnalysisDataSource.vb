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

Imports System.Data
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports EwEUtils.Database

#End Region ' Imports

Namespace ExternalData

    ''' <summary>
    ''' Implemention of IDataConsumerPlugin that fires an event when Network Analysis data is available.
    ''' </summary>
    Public Class cNetworkAnalysisDataSource
        Implements Data.IDataConsumerPlugin
        Implements IExternalDataSource

#Region " Private vars "

        Private Shared s_core As cCore = Nothing

#End Region ' Private vars

#Region " Public events "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that get fired when INetworkAnalysisData is available
        ''' </summary>
        ''' <param name="NetworkAnalysisData">The data that is available.</param>
        ''' -----------------------------------------------------------------------
        Public Event onNetworkAnalysisData(ByVal NetworkAnalysisData As INetworkAnalysisData)

#End Region ' Public events

#Region " Singleton 'Shared' interface "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Return the instance of this class created by the PluginManager.
        ''' </summary>
        ''' <returns>The only instance of cEconomicDataSource. Otherwise nothing</returns>
        ''' <remarks>An instance of this class is loaded from the Core via the Plugin 
        ''' manager. This allows classes in the core to retrieve an instance of 
        ''' cEconomicDataSource for Economic data.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function getInstance() As cNetworkAnalysisDataSource

            Dim dataSource As cNetworkAnalysisDataSource = Nothing

            ' JS 12Dec10: Plug-in may not be initialized (yet) on purpose
            If (cNetworkAnalysisDataSource.s_core Is Nothing) Then Return Nothing

            Try

                Dim plugins As ICollection(Of EwEPlugin.IPlugin)

                plugins = s_core.PluginManager.GetPlugins(cNetworkAnalysisDataSource.InternalName)
                For Each plugin As IPlugin In plugins
                    If TypeOf plugin Is cNetworkAnalysisDataSource Then
                        dataSource = DirectCast(plugin, cNetworkAnalysisDataSource)
                        Exit For
                    End If
                Next

            Catch ex As Exception
                System.Console.WriteLine("cNetworkAnalysisDataSource.getInstance() Error: " & ex.Message)
            End Try

            Debug.Assert(dataSource IsNot Nothing, "cNetworkAnalysisDataSource.getInstance() Failed to create instance.")

            Return dataSource

        End Function

#End Region ' Singleton 'Shared' interface

#Region " Public interfaces "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether any network analysis data producer, if available, 
        ''' should deliver data for a given <see cref="IRunType">run type</see>.
        ''' </summary>
        ''' <param name="runtype"></param>
        ''' -----------------------------------------------------------------------
        Public Property EnableData(ByVal runtype As IRunType) As Boolean _
            Implements IExternalDataSource.EnableData
            Get
                Return s_core.PluginManager.EnableData(GetType(INetworkAnalysisData), runtype)
            End Get
            Set(ByVal value As Boolean)
                s_core.PluginManager.EnableData(GetType(INetworkAnalysisData), runtype) = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' States whether a plug-in capable of delivering Network Analysis data is available.
        ''' </summary>
        ''' <param name="runtype">The core run type to check availability for.</param>
        ''' <returns>True if available.</returns>
        ''' -----------------------------------------------------------------------
        Public Function IsDataAvailable(ByVal runtype As EwEUtils.Core.IRunType) As Boolean _
              Implements IExternalDataSource.IsDataAvailable
            Return s_core.PluginManager.IsDataAvailable(GetType(IEconomicData), runtype)
        End Function

#End Region ' Public interfaces

#Region " Private methods "

        Private Sub FireonNetworkAnalysisData(ByVal data As INetworkAnalysisData)
            Try
                RaiseEvent onNetworkAnalysisData(data)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & "FireonEconomicData() Error: " & ex.Message)
            End Try
        End Sub

        Private Shared ReadOnly Property InternalName() As String
            Get
                Return GetType(cNetworkAnalysisDataSource).ToString
            End Get
        End Property

#End Region ' Private methods

#Region " IDataConsumerPlugin implementation "

        Public Function ReceiveData(ByVal strDataName As String, ByVal data As EwEPlugin.Data.IPluginData) As Boolean _
            Implements EwEPlugin.Data.IDataConsumerPlugin.ReceiveData

            Try
                If TypeOf data Is INetworkAnalysisData Then
                    Dim ecoData As INetworkAnalysisData = DirectCast(data, INetworkAnalysisData)
                    Me.FireonNetworkAnalysisData(ecoData)
                End If
            Catch ex As Exception
                'make sure all exceptions are handled here and not thrown back to the PluginManager
                cLog.Write(ex)
            End Try

        End Function

        Public ReadOnly Property Author() As String _
            Implements EwEPlugin.IPlugin.Author
            Get
                Return "UBC Institute for the Oceans and Fisheries"
            End Get
        End Property

        Public ReadOnly Property Contact() As String _
            Implements EwEPlugin.IPlugin.Contact
            Get
                Return "mailto:ewedevteam@gmail.com"
            End Get
        End Property

        Public ReadOnly Property Description() As String _
            Implements EwEPlugin.IPlugin.Description
            Get
                Return "Core plugin to provide network analysis data from an external source."
            End Get
        End Property

        Public Sub Initialize(ByVal core As Object) _
            Implements EwEPlugin.IPlugin.Initialize
            cNetworkAnalysisDataSource.s_core = DirectCast(core, cCore)
        End Sub

        Public ReadOnly Property Name() As String _
            Implements EwEPlugin.IPlugin.Name
            Get
                Return cNetworkAnalysisDataSource.InternalName
            End Get
        End Property

#End Region ' IDataConsumerPlugin implementation

    End Class

End Namespace ' ExternalData
