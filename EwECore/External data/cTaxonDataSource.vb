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

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implemention of IDataConsumerPlugin that fires an event whenever 
    ''' taxonomy data or taxonomy data search results are available.
    ''' </summary>
    ''' <remarks>This could be extented to be a source for any data that is 
    ''' broadcasted via IDataBroadcaster plugin interfaces.</remarks>
    ''' -----------------------------------------------------------------------
    Public Class cTaxonDataSource
        Implements IDataConsumerPlugin
        Implements IExternalDataSource

#Region " Private vars "

        Private Shared s_core As cCore = Nothing
        Private m_strProviderName As String = ""

#End Region ' Private vars

#Region " Public events "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that gets fired when ITaxonData is available.
        ''' </summary>
        ''' <param name="TaxonData"></param>
        ''' -----------------------------------------------------------------------
        Public Event OnTaxonData(ByVal TaxonData As ITaxonSearchData)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that gets fired when IDataSearchResults are available.
        ''' </summary>
        ''' <param name="data"></param>
        ''' -----------------------------------------------------------------------
        Public Event OnTaxonSearchResults(ByVal data As IDataSearchResults)

#End Region ' Public events

#Region " Singleton 'Shared' interface "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Return the instance of this class created by the PluginManager
        ''' </summary>
        ''' <returns>The only instance of cTaxonDataSource. Otherwise nothing</returns>
        ''' <remarks>An instance of this class is loaded from the Core via the Plugin 
        ''' manager. This allows classes other objects to retrieve an instance of 
        ''' cTaxonDataSource for receiving taxonomy data notifications.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetInstance() As cTaxonDataSource

            Dim dataSource As cTaxonDataSource = Nothing

            ' JS 12Dec10: Plug-in may not be initialized (yet) on purpose
            If (cTaxonDataSource.s_core Is Nothing) Then Return Nothing

            Try

                Dim plugins As ICollection(Of EwEPlugin.IPlugin)

                plugins = s_core.PluginManager.GetPlugins(cTaxonDataSource.InternalName)
                For Each plugin As IPlugin In plugins
                    If TypeOf plugin Is cTaxonDataSource Then
                        dataSource = DirectCast(plugin, cTaxonDataSource)
                        Exit For
                    End If
                Next

            Catch ex As Exception
                System.Console.WriteLine("cTaxonDataSource.GetInstance() Error: " & ex.Message)
            End Try

            'JS 26May10: disabled assert; plug-in may not be available on purpose
            'Debug.Assert(dataSource IsNot Nothing, "cTaxonDataSource.getInstance() Failed to create instance.")

            Return dataSource

        End Function

#End Region ' Singleton 'Shared' interface

#Region " Public interfaces "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether any taxon data producer, if available, should deliver 
        ''' data for a given <see cref="IRunType">run type</see>.
        ''' </summary>
        ''' <param name="runtype"></param>
        ''' -----------------------------------------------------------------------
        Public Property EnableData(ByVal runtype As IRunType) As Boolean _
            Implements IExternalDataSource.EnableData
            Get
                Return s_core.PluginManager.EnableData(GetType(ITaxonSearchData), runtype)
            End Get
            Set(ByVal value As Boolean)
                s_core.PluginManager.EnableData(GetType(ITaxonSearchData), runtype) = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' States whether a plug-in capable of delivering taxon data is available.
        ''' </summary>
        ''' <param name="runtype">The core run type to check availability for.</param>
        ''' <returns>True if available.</returns>
        ''' -----------------------------------------------------------------------
        Public Function IsDataAvailable(ByVal runtype As EwEUtils.Core.IRunType) As Boolean _
              Implements IExternalDataSource.IsDataAvailable
            Return s_core.PluginManager.IsDataAvailable(GetType(ITaxonSearchData), runtype)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns all available producer names.
        ''' </summary>
        ''' <returns>An array of strings representing the producers.</returns>
        ''' -----------------------------------------------------------------------
        Public Function ProducerNames() As String()
            Dim lstrNames As New List(Of String)
            For Each dp As IDataProducerPlugin In s_core.PluginManager.DataProducers(GetType(ITaxonSearchData))
                lstrNames.Add(dp.Name)
            Next
            Return lstrNames.ToArray
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the active ITaxonData producer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property ActiveProducerName() As String
            Get
                Return Me.m_strProviderName
            End Get
            Set(ByVal value As String)
                If String.Compare(value, Me.m_strProviderName, True) = 0 Then Return
                Me.m_strProviderName = value
                For Each dp As IDataProducerPlugin In s_core.PluginManager.DataProducers(GetType(ITaxonSearchData))
                    dp.SetEnabled(String.Compare(dp.Name, Me.m_strProviderName, True) = 0)
                Next
            End Set

        End Property
#End Region ' Public interfaces

#Region " Private methods "

        Private Sub FireOnTaxonData(ByVal data As ITaxonSearchData)
            Try
                RaiseEvent OnTaxonData(data)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & "FireonTaxonData() Error: " & ex.Message)
            End Try
        End Sub

        Private Sub FireOnTaxonSearchResults(ByVal data As IDataSearchResults)
            Try
                RaiseEvent OnTaxonSearchResults(data)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & "FireOnTaxonSearchResults() Error: " & ex.Message)
            End Try
        End Sub

        Private Shared ReadOnly Property InternalName() As String
            Get
                Return GetType(cTaxonDataSource).ToString
            End Get
        End Property

#End Region ' Private methods

#Region " IDataConsumerPlugin implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Data comsumption handling; dispatches both incoming taxonomy data or
        ''' taxonomy search results.
        ''' </summary>
        ''' <param name="strDataName">Name of incoming data (not used).</param>
        ''' <param name="data">Actual incoming data.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function ReceiveData(ByVal strDataName As String, ByVal data As EwEPlugin.Data.IPluginData) As Boolean _
            Implements EwEPlugin.Data.IDataConsumerPlugin.ReceiveData

            Try

                If TypeOf data Is ITaxonSearchData Then
                    Dim taxon As ITaxonSearchData = DirectCast(data, ITaxonSearchData)
                    Me.FireOnTaxonData(taxon)
                ElseIf TypeOf data Is IDataSearchResults Then
                    Dim results As IDataSearchResults = DirectCast(data, IDataSearchResults)
                    If TypeOf results.SearchTerm Is ITaxonSearchData Then
                        Me.FireOnTaxonSearchResults(results)
                    End If
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
                Return "Core plugin to receive taxon data from an external source."
            End Get
        End Property

        Public Sub Initialize(ByVal core As Object) _
            Implements EwEPlugin.IPlugin.Initialize
            s_core = DirectCast(core, cCore)
        End Sub

        Public ReadOnly Property Name() As String _
            Implements EwEPlugin.IPlugin.Name
            Get
                Return cTaxonDataSource.InternalName
            End Get
        End Property

#End Region ' IDataConsumerPlugin implementation

    End Class

End Namespace ' ExternalData

