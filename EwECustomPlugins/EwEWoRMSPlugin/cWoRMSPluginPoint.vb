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

Imports System.Reflection
Imports EwECore
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEWoRMSPlugin.WoRMSWebService
Imports System.Web.Services.Protocols
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class cWoRMSPluginPoint
    Implements IDataSearchProducerPlugin
    Implements IDisposedPlugin
    Implements IConfigurablePlugin
    Implements ITaxonSearchCapabilitiesPlugin

#Region " Private vars "

    Private m_bInitOk As Boolean = False
    Private m_core As cCore = Nothing
    Private m_thread As Threading.Thread = Nothing
    Private m_client As AphiaNameServicePortTypeClient = Nothing

    ''' <summary>Broadcaster for distributing data.</summary>
    Private m_broadcaster As IDataBroadcaster = Nothing
    ''' <summary>Search term.</summary>
    Private m_term As ITaxonSearchData = Nothing
    ''' <summary>Search results.</summary>
    Private m_results As cWoRMSTaxonSearchResults = Nothing
    ''' <summary>Data provider enabled state.</summary>
    Private m_bEnabled As Boolean = True
    ''' <summary>Flag stating whether a search is in progress.</summary>
    Private m_bSearching As Boolean = False

#End Region ' Private vars

#Region " Plugin points implementation "

#Region " Init "

    ''' <inheritdocs cref="IPlugin.Initialize"/>
    Friend Sub Initialize(ByVal core As Object) _
        Implements IPlugin.Initialize

        Debug.Assert(TypeOf core Is EwECore.cCore, Me.ToString & ".Initialize() argument core is not a cCore object.")
        m_bInitOk = False
        Try
            If TypeOf core Is EwECore.cCore Then
                Me.m_core = DirectCast(core, EwECore.cCore)
                Me.m_bInitOk = True
                'System.Console.WriteLine(Me.ToString & ".Initialize() Successfull.")
            Else
                'some kind of a message
                System.Console.WriteLine(Me.ToString & ".Initialize() Failed.")
                Return
            End If
        Catch ex As Exception
            cLog.Write(ex)
            System.Console.WriteLine(Me.ToString & ".Initialize() Error: " & ex.Message)
            Debug.Assert(False, ex.Message)
            Return
        End Try

    End Sub

    Friend Function InitClient() As AphiaNameServicePortTypeClient

        Dim binding = New ServiceModel.BasicHttpBinding()
        binding.Name = "AphiaNameServiceBinding"
        binding.CloseTimeout = TimeSpan.FromSeconds(Me.ConnectionTimeOut)
        binding.OpenTimeout = TimeSpan.FromSeconds(Me.ConnectionTimeOut)
        binding.ReceiveTimeout = TimeSpan.FromMinutes(Me.ResponseTimeOut)
        binding.SendTimeout = TimeSpan.FromMinutes(Me.ConnectionTimeOut)
        binding.AllowCookies = False
        binding.BypassProxyOnLocal = False
        binding.HostNameComparisonMode = ServiceModel.HostNameComparisonMode.StrongWildcard
        binding.MaxBufferPoolSize = 524288
        binding.MaxBufferSize = 524288
        binding.MaxReceivedMessageSize = 524288
        binding.MessageEncoding = ServiceModel.WSMessageEncoding.Text
        binding.TextEncoding = System.Text.Encoding.UTF8
        binding.TransferMode = ServiceModel.TransferMode.Buffered
        binding.UseDefaultWebProxy = True

        binding.ReaderQuotas.MaxDepth = 32
        binding.ReaderQuotas.MaxStringContentLength = 8192
        binding.ReaderQuotas.MaxArrayLength = 16384
        binding.ReaderQuotas.MaxBytesPerRead = 4096
        binding.ReaderQuotas.MaxNameTableCharCount = 16384

        binding.Security.Mode = ServiceModel.BasicHttpSecurityMode.None
        binding.Security.Transport.ClientCredentialType = ServiceModel.HttpClientCredentialType.None
        binding.Security.Transport.ProxyCredentialType = ServiceModel.HttpProxyCredentialType.None
        binding.Security.Transport.Realm = ""
        binding.Security.Message.ClientCredentialType = ServiceModel.BasicHttpMessageCredentialType.UserName
        binding.Security.Message.AlgorithmSuite = ServiceModel.Security.SecurityAlgorithmSuite.Default
        Dim endpointStr = "http://www.marinespecies.org/aphia.php?p=soap"
        Dim endpoint = New ServiceModel.EndpointAddress(endpointStr)

        Return New AphiaNameServicePortTypeClient(binding, endpoint)

    End Function

#End Region ' Init

#Region " Generic "

    ''' <inheritdocs cref="IPlugin.Author"/>
    Friend ReadOnly Property Author() As String _
        Implements IPlugin.Author
        Get
            Return "Ecopath International Initiative"
        End Get
    End Property

    ''' <inheritdocs cref="IPlugin.Contact"/>
    Friend ReadOnly Property Contact() As String _
        Implements IPlugin.Contact
        Get
            Return "mailto:ewedevteam@gmail.com"
        End Get
    End Property

    ''' <inheritdocs cref="IPlugin.Description"/>
    Friend ReadOnly Property Description() As String _
        Implements IPlugin.Description
        Get
            Return Me.Name
        End Get
    End Property

    ''' <inheritdocs cref="IPlugin.Name"/>
    Friend ReadOnly Property Name() As String _
        Implements IPlugin.Name
        Get
            Return My.Resources.ENGINE_NAME
        End Get
    End Property

#End Region ' Generic

#Region " Data "

    ''' <inheritdocs cref="IDataProducerPlugin.Broadcaster"/>
    Friend Sub Broadcaster(ByVal broadcaster As IDataBroadcaster) _
        Implements IDataProducerPlugin.Broadcaster
        Me.m_broadcaster = broadcaster
    End Sub

    ''' <inheritdocs cref="IDataProducerPlugin.GetDataByType"/>
    Friend Function GetDataByType(ByVal typeData As System.Type, ByRef data As IPluginData) As Boolean _
        Implements IDataProducerPlugin.GetDataByType
        If (TypeOf data Is ITaxonSearchData) Then data = DirectCast(Me.m_term, IPluginData)
        Return Me.IsEnabled
    End Function

    ''' <inheritdocs cref="IDataProducerPlugin.IsDataAvailable"/>
    Friend Function IsDataAvailable(ByVal typeData As System.Type, _
                                    Optional ByVal runType As EwEUtils.Core.IRunType = Nothing) As Boolean _
        Implements IDataProducerPlugin.IsDataAvailable
        Return (GetType(ITaxonSearchData).IsAssignableFrom(typeData))
    End Function

    ''' <inheritdocs cref="IDataProducerPlugin.IsEnabled"/>
    Friend Function IsEnabled() As Boolean _
        Implements IDataProducerPlugin.IsEnabled
        Return Me.m_bEnabled
    End Function

    ''' <inheritdocs cref="IDataProducerPlugin.IsEnabled"/>
    Friend Function IsEnabled(ByVal typeData As System.Type, _
                              ByVal runType As EwEUtils.Core.IRunType) As Boolean _
        Implements IDataProducerPlugin.IsEnabled
        Return Me.m_bEnabled
    End Function

    ''' <inheritdocs cref="IDataProducerPlugin.SetEnabled"/>
    Friend Function SetEnabled(ByVal bEnable As Boolean) As Boolean _
        Implements IDataProducerPlugin.SetEnabled
        Me.m_bEnabled = bEnable
        Return Me.m_bEnabled
    End Function

    ''' <inheritdocs cref="IDataProducerPlugin.SetEnabled"/>
    Friend Sub SetEnabled(ByVal typeData As System.Type, _
                          ByVal runType As EwEUtils.Core.IRunType, _
                          ByVal bEnable As Boolean) _
        Implements IDataProducerPlugin.SetEnabled
        ' NOP
    End Sub

#End Region ' Data

#Region " Search "

    ''' <inheritdocs cref="IDataSearchProducerPlugin.StartSearch"/>
    Friend Function StartSearch(ByVal data As Object, iMaxResults As Integer) As Boolean _
        Implements IDataSearchProducerPlugin.StartSearch

        If Not Me.IsEnabled() Then Return False
        If Not (TypeOf data Is ITaxonSearchData) Then Return False

        ' Get ready
        Me.m_term = DirectCast(data, ITaxonSearchData)
        Me.m_results = Nothing

        Return Me.Search(DirectCast(data, ITaxonSearchData))

    End Function

    ''' <inheritdocs cref="IDataSearchProducerPlugin.StartSearch"/>
    Friend Function StopSearch() As Boolean _
        Implements IDataSearchProducerPlugin.StopSearch
        Me.Search(Nothing)
        Return True
    End Function

    ''' <inheritdocs cref="IDataSearchProducerPlugin.IsSeaching"/>
    Friend Function IsSeaching() As Boolean _
        Implements IDataSearchProducerPlugin.IsSeaching
        Return Me.m_bSearching
    End Function

    ''' <inheritdocs cref="IDataSearchProducerPlugin.SearchResults"/>
    Friend Function SearchResults(ByVal dataTerm As Object, ByRef results As IDataSearchResults) As Boolean _
        Implements IDataSearchProducerPlugin.SearchResults

        If (ReferenceEquals(dataTerm, Me.m_term)) Then
            results = Me.m_results
            Return True
        End If
        Return False

    End Function

    ''' <inheritdocs cref="IDataSearchProducerPlugin.CreateSearchTerm"/>
    Public Function CreateSearchTerm() As Object _
        Implements IDataSearchProducerPlugin.CreateSearchTerm
        Return New cWoRMSTaxonData(cTypeUtils.TypeToString(Me.GetType()))
    End Function

#End Region ' Search

#Region " Disposal "

    ''' <inheritdocs cref="IDisposedPlugin.Dispose"/>
    Friend Sub Dispose() _
        Implements IDisposedPlugin.Dispose
    End Sub

#End Region ' Disposal

#End Region ' Plugin points implementation

#Region " Configuration "

    ''' <inheritdocs cref="IConfigurablePlugin.GetConfigUI"/>
    Public Function GetConfigUI() As System.Windows.Forms.Control _
        Implements EwEPlugin.IConfigurablePlugin.GetConfigUI
        Return New ucConfig(Me)
    End Function

    ''' <inheritdocs cref="IConfigurablePlugin.IsConfigured"/>
    Public Function IsConfigured() As Boolean _
        Implements EwEPlugin.IConfigurablePlugin.IsConfigured
        Return True
    End Function

    ''' <summary>
    ''' Get/set the number of seconds to wait for the webservice to connect.
    ''' </summary>
    Friend Property ConnectionTimeOut As Integer
        Get
            Return My.Settings.ConnectionTimeOut
        End Get
        Set(value As Integer)
            My.Settings.ConnectionTimeOut = value
            My.Settings.Save()
            Me.m_client = Nothing
        End Set
    End Property

    ''' <summary>
    ''' Get/set the number of seconds to wait for the webservice to reply.
    ''' </summary>
    Friend Property ResponseTimeOut As Integer
        Get
            Return My.Settings.ResponseTimeOut
        End Get
        Set(value As Integer)
            My.Settings.ResponseTimeOut = value
            My.Settings.Save()
            Me.m_client = Nothing
        End Set
    End Property

#End Region ' Configuration

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Execute a database search.
    ''' </summary>
    ''' <param name="taxon">The term to search for.</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' If successful, the local results will be populated.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Function Search(ByVal taxon As ITaxonSearchData) As Boolean

        If (Me.m_thread IsNot Nothing) Then
            If (Me.m_client IsNot Nothing) Then
                Me.m_client.Abort()
                Me.m_client = Nothing
            End If
            Me.m_thread = Nothing
            Me.m_bSearching = False
        End If

        If (taxon Is Nothing) Then Return True

        ' Started searching
        Me.m_bSearching = True

        Me.m_thread = New Threading.Thread(AddressOf Me.SearchThreaded)
        Me.m_thread.Start()

        Return True

    End Function

    Private Sub SearchThreaded()

        Dim c As AphiaNameServicePortTypeClient = Nothing
        Dim lRecords As AphiaRecord() = Nothing
        Dim lResults As New List(Of ITaxonSearchData)

        Try
            c = Me.InitClient()
        Catch ex As Exception
            cLog.Write(ex, "cWoRMSPluginPoint.SearchThreaded(InitClient)")
            Return
        End Try

        Try
            Me.m_client = c
            lRecords = Me.m_client.getAphiaRecords(Me.m_term.Common, True, True, True, 1)
            c.Close()
        Catch exThread As Threading.ThreadAbortException
            ' Search aborted, is ok. 
            ' Do not tinker with searching flag because it may already have been set in the calling thread
            ' Me.m_bSearching = False
            c.Abort()
        Catch exComm As ServiceModel.CommunicationObjectAbortedException
            ' Search was deliberately aborted: ignore this exception
        Catch exCfg As Configuration.ConfigurationException
            ' NOP
            c.Abort()
        Catch exSoap As SoapException
            cLog.Write(exSoap, "cWoRMSPluginPoint.SearchThreaded")
            ' Send message cross-threaded
            Dim msg As New cMessage(String.Format("An error occurred communicating with the WoRMS web service: '{0}'", exSoap.Message), _
                                    eMessageType.Any, eCoreComponentType.External, eMessageImportance.Warning)
            Me.m_core.Messages.SendMessage(msg)
            c.Abort()
        Catch exComm As ServiceModel.CommunicationException
            ' Too many results, or no end point (HRESULT -2146233087) ...
            c.Abort()
            cLog.Write(exComm, "cWoRMSPluginPoint.SearchThreaded")
        Catch exTime As TimeoutException
            ' Timeout
            c.Abort()
            cLog.Write(exTime, "cWoRMSPluginPoint.SearchThreaded")
        Catch exGeneral As Exception
            Debug.Assert(False, exGeneral.Message)
            cLog.Write(exGeneral, "cWoRMSPluginPoint.SearchThreaded")
            c.Abort()
        Finally
            Me.m_client = Nothing
        End Try

        If (lRecords IsNot Nothing) Then
            For Each record As AphiaRecord In lRecords
                Try
                    Dim result As ITaxonSearchData = Me.ReadTaxon(record)
                    If (result IsNot Nothing) Then
                        lResults.Add(result)
                    End If
                Catch ex As Exception
                    ' Ignore malformed result
                End Try
            Next
        End If


        ' Create new results
        Me.m_results = New cWoRMSTaxonSearchResults(Me.m_term, lResults.ToArray(), cTypeUtils.TypeToString(Me.GetType))
        ' Broadcast results
        Me.m_broadcaster.BroadcastData(Me.Name, Me.m_results)

        ' Done searching
        Me.m_bSearching = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Read a web service record.
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function ReadTaxon(ByVal record As AphiaRecord) As ITaxonSearchData

        If (record Is Nothing) Then Return Nothing

        Dim taxon As New cWoRMSTaxonData(cTypeUtils.TypeToString(Me.GetType()))

        Try

            taxon.Common = Me.Validate(record.scientificname)
            taxon.Species = Me.Validate(record.scientificname)
            taxon.Class = Me.Validate(record.class)
            taxon.Order = Me.Validate(record.order)
            taxon.Genus = Me.Validate(record.genus)
            taxon.Family = Me.Validate(record.family)
            taxon.Phylum = Me.Validate(record.phylum)
            taxon.SourceKey = Me.Validate(record.AphiaID.ToString)
            taxon.CodeLSID = Me.Validate(record.lsid)

            If taxon.Species.StartsWith(taxon.Genus) Then
                taxon.Species = taxon.Species.Substring(taxon.Genus.Length).Trim
            End If

        Catch ex As Exception
            cLog.Write(ex, "cWoRMSPluginPoint.ReadTaxon")
        End Try

        Return taxon

    End Function

    Private Function Validate(strField As String) As String
        If String.IsNullOrWhiteSpace(strField) Then Return ""
        Return strField
    End Function

#End Region ' Internals

#Region " Search capabilities "

    Public Function HasSpatialSearchCapabilities() As Boolean _
        Implements ITaxonSearchCapabilitiesPlugin.HasSpatialSearchCapabilities
        Return False
    End Function

    Public Function CanSearchByTaxonomicLevel() As eTaxonClassificationType _
        Implements ITaxonSearchCapabilitiesPlugin.TaxonSearchCapabilities
        Return eTaxonClassificationType.Latin
    End Function

    Public Function HasDepthRangeSearchCapabilities() As Boolean _
        Implements ITaxonSearchCapabilitiesPlugin.HasDepthRangeSearchCapabilities
        Return False
    End Function

#End Region ' Search capabilities

End Class
