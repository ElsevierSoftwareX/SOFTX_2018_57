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
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports EwECore
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class cValueChainPlugin
    Inherits cNavTreeControlPlugin
    Implements EwEPlugin.IUIContextPlugin
    Implements EwEPlugin.IEcopathPlugin
    Implements EwEPlugin.IEcopathRunCompletedPlugin
    Implements EwEPlugin.IEcosimRunInitializedPlugin
    Implements EwEPlugin.IEcosimEndTimestepPlugin
    Implements EwEPlugin.IEcosimRunCompletedPlugin
    Implements EwEPlugin.Data.IDatabasePlugin
    Implements EwEPlugin.Data.IDataProducerPlugin
    Implements EwEPlugin.ISearchPlugin
    Implements EwEPlugin.IDisposedPlugin
    Implements EwEPlugin.IAutoSavePlugin

#Region " Privates "

    Private m_uic As cUIContext = Nothing
    Private m_core As EwECore.cCore = Nothing
    Private m_bInitOK As Boolean = False
    Private m_form As frmMain = Nothing
    Private m_data As cData = Nothing
    Private m_bIsEnabled As Boolean = True
    Private m_model As cModel = Nothing
    Private m_result As cResults = Nothing
    Private m_mhEcopath As cMessageHandler = Nothing
    Private m_linkman As cLandingsLinkManager = Nothing
    Private m_syncobj As SynchronizationContext = Nothing

    ' Data exchange
    Private m_dataBroadcaster As EwEPlugin.Data.IDataBroadcaster = Nothing
    ''' <summary>Ooooh, that was long ago...</summary>
    Private m_ddx As cPluginData = Nothing

#End Region ' Privates

#Region " Singleton "

    Private Shared _inst_ As cValueChainPlugin = Nothing

    Public Sub New()
        If _inst_ Is Nothing Then
            _inst_ = Me
        End If
    End Sub

    Public Shared Function SwitchForm(ByVal page As frmMain.eValueChainPageTypes) As frmMain

        ' Flag stating whether form is ready to be used. If so, we don't need to create it, do we?
        Dim bIsFormReady As Boolean = False
        Dim frm As frmMain = Nothing

        'Interface item has been clicked
        'Show the Ecotroph interface
        If cValueChainPlugin._inst_.m_bInitOK Then

            ' Does form still exist?
            If Not cValueChainPlugin._inst_.HasInterface() Then
                ' #No: create it
                frm = New frmMain(cValueChainPlugin._inst_)
                cValueChainPlugin._inst_.m_form = frm
            Else
                frm = cValueChainPlugin._inst_.m_form
            End If
            frm.ShowForm(page)
        Else
            Debug.Assert(False, "Plugin was not initialized properly.")
        End If
        Return frm

    End Function

#End Region ' Singleton

#Region " IPlugin point implementation "

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "vcNode00"
        End Get
    End Property

    Public Overrides ReadOnly Property ControlText() As String
        Get
            Return My.Resources.GENERIC_CAPTION
        End Get
    End Property

    Public Overrides Function FormPage() As frmMain.eValueChainPageTypes
        Return frmMain.eValueChainPageTypes.Parameters
    End Function

    Public Overrides ReadOnly Property NavigationTreeItemLocation() As String
        Get
            Return Me.NavTreeNodeRoot()
        End Get
    End Property

    Public Overrides ReadOnly Property ControlImage() As System.Drawing.Image
        Get
            Return SharedResources.nav_output
        End Get
    End Property

    Public Overrides ReadOnly Property Description() As String
        Get
            Dim sb As New StringBuilder()
            sb.AppendLine("ValueChain - an economic fisheries model for EwE6")
            sb.AppendLine("")
            sb.AppendLine("This plug-in calculates a range of economic and social-economic indicators based on Ecopath and Ecosim data, where users can define economic systems as value chains of desired complexity.")
            sb.AppendLine("")
            sb.AppendLine("This plug-in was developed in conjunction with the ECOST project (http://www.ird.fr/ecostproject), and was partially funded by the North Sea Centre in Hirtshals, Denmark.")
            Return sb.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Initialize the Plugin. This is called when the core loads the Plugin. It will only be called once.
    ''' </summary>
    Public Overrides Sub Initialize(ByVal core As Object)

        ' Sanity checks
        Debug.Assert(core IsNot Nothing)
        Debug.Assert(TypeOf core Is EwECore.cCore, Me.ToString & ".Initialize() argument core is not a cCore object.")
        Debug.Assert(Me.m_bInitOK = False)

        ' To prevent multiple calls
        Me.m_bInitOK = False

        Try
            If (TypeOf core Is EwECore.cCore) Then

                Me.m_core = DirectCast(core, EwECore.cCore)
                Me.m_ddx = New cPluginData(cTypeUtils.TypeToString(Me.GetType()))
                Me.m_data = New cData(Me.m_core)
                Me.m_model = New cModel()
                Me.m_result = New cResults(Me.m_data)
                Me.m_linkman = New cLandingsLinkManager(Me.m_data, Me.m_core)
                Me.m_syncobj = SynchronizationContext.Current

                If (Me.m_syncobj Is Nothing) Then
                    Me.m_syncobj = New SynchronizationContext()
                End If
                Me.m_mhEcopath = New cMessageHandler(AddressOf OnEcopathMessage, _
                                                     eCoreComponentType.EcoPath, _
                                                     eMessageType.DataValidation, _
                                                     Me.m_syncobj)
#If DEBUG Then
                Me.m_mhEcopath.Name = "ValueChain::Ecopath"
#End If
                Me.m_core.Messages.AddMessageHandler(Me.m_mhEcopath)

                ' Done initializing
                Me.m_bInitOK = True

            Else
                'some kind of a message
                Return
            End If

        Catch ex As Exception
            cLog.Write(ex, "VC::cPluginPoint.Initialize")
            Debug.Assert(False, ex.Message)
            Return

        End Try

    End Sub

    Public Sub Dispose() _
        Implements EwEPlugin.IDisposedPlugin.Dispose
        ' Clean up message handler
        If (Me.m_mhEcopath IsNot Nothing) Then
            Me.m_core.Messages.RemoveMessageHandler(Me.m_mhEcopath)
            Me.m_mhEcopath.Dispose()
            Me.m_mhEcopath = Nothing
        End If
    End Sub

#Region " GUI "

    Public Sub UIContext(ByVal uic As Object) _
        Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

#End Region ' GUI

#Region " Database integration "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, no longer called but kept for compliancy.
    ''' </summary>
    ''' <param name="strName">The name of the datasource.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Open(ByVal strName As String) As Boolean _
        Implements EwEPlugin.Data.IDatabasePlugin.Open
        ' NOP
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when an EwE6 model is closed. 
    ''' Handled to terminate the Value Chain model corresponding to an EwE model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Close() _
        Implements EwEPlugin.Data.IDatabasePlugin.Close
        If Me.HasInterface Then
            Me.m_form.Close()
            Me.m_form.Dispose()
        End If
        Me.m_form = Nothing
        Me.m_data.Close()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when an EwE6 model is polled for
    ''' unsaved changes. Handled to inform the EwE6 engine that the Value Chain 
    ''' model has unsaved changes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsModified() As Boolean _
        Implements EwEPlugin.Data.IDatabasePlugin.IsModified
        Return Me.m_data.IsChanged()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when an EwE6 model is loaded. 
    ''' Handled toload the Value Chain model corresponding to an EwE model.
    ''' </summary>
    ''' <param name="dataSource">The loaded datasource.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function LoadModel(ByVal dataSource As Object) As Boolean _
        Implements EwEPlugin.IEcopathPlugin.LoadModel

        ' Sanity checks
        Debug.Assert(Me.m_data.IsChanged() = False)

        If Me.m_data.Load(Me.m_core.DataSource.ToString) Then
            ' Manage incoming DB to weed out dead stuff
            Me.m_linkman.ManageLinks()
            Return True
        End If

        Return False

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when an EwE6 model is saved. 
    ''' Handled to save the Value Chain model corresponding to an EwE model.
    ''' </summary>
    ''' <param name="dataSource">The loaded datasource.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function SaveModel(ByVal dataSource As Object) As Boolean _
        Implements EwEPlugin.IEcopathPlugin.SaveModel
        Return Me.m_data.Save()
    End Function

    Private Function CloseModel() As Boolean _
        Implements IEcopathPlugin.CloseModel
        ' NOP
    End Function

#End Region ' Database integration

#Region " Ecopath integration "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when Ecopath has ran.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub EcopathRunCompleted(ByRef EcopathDataStructures As Object) _
        Implements EwEPlugin.IEcopathRunCompletedPlugin.EcopathRunCompleted

        Dim parms As cParameters = Me.m_data.Parameters

        ' Abort if no params
        If (parms Is Nothing) Then Return
        ' Abort if not allowed to run with Ecopath
        If (parms.RunWithEcopath = False) Then Return

        ' Running in auto mode?
        If (Me.m_model.IsManualRunMode = False) Then
            ' #Yes: prepare results for receiving Ecopath results
            Me.m_result.Reset(cModel.eRunTypes.Ecopath)
        End If

        ' Prepare data
        Me.m_data.InitRun()
        ' Run a single time step
        Me.m_model.RunTimeStep(Me.m_data, Me.m_result, 1)

#If DEBUG Then
        Debug.Assert(Me.m_data.HasCompletedRun(), "Chain computations are broken; one or more units did not compute")
#End If

        ' Send out data
        Me.BroadcastResults(1)

        If Me.AutoSave Then
            Me.m_model.SaveResults(Me.m_data, Me.m_result)
        End If

    End Sub

#End Region ' Ecopath integration

#Region " Ecosim integration "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called just before Ecosim will run.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub EcosimRunInitialized(ByVal EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimRunInitializedPlugin.EcosimRunInitialized

        Dim parms As cParameters = Me.m_data.Parameters

        ' Abort if no params
        If (parms Is Nothing) Then Return
        ' Abort if not allowed to run with Ecosim
        If (parms.RunWithEcosim = False) Then Return

        ' Running in auto mode?
        If (Me.m_model.IsManualRunMode = False) Then
            ' #Yes: prepare results for receiving Ecosim results
            Me.m_result.Reset(cModel.eRunTypes.Ecosim)
        End If

        ' Prepare data
        Me.m_data.InitRun()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called at end of every Ecosim timestep.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Sub EcosimEndTimeStep(ByRef BiomassAtTimestep() As Single, _
                          ByVal EcosimDatastructures As Object, _
                          ByVal iTimeStep As Integer, _
                          ByVal ecosimresults As Object) _
        Implements IEcosimEndTimestepPlugin.EcosimEndTimeStep

        Dim parms As cParameters = Me.m_data.Parameters

        ' Abort if no params
        If (parms Is Nothing) Then Return
        ' Abort if not allowed to run with Ecosim
        If (parms.RunWithEcosim = False) Then Return
        ' Do not run with searches
        If (Me.m_data.Core.StateMonitor.IsSearching) Then Return

        ' Run VC model
        Me.m_model.RunTimeStep(Me.m_data, Me.m_result, iTimeStep, DirectCast(ecosimresults, cEcoSimResults), DirectCast(EcosimDatastructures, cEcosimDatastructures))
        ' Send out data
        Me.BroadcastResults(iTimeStep)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when Ecosim has finished running.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub EcosimRunCompleted(ByVal EcosimDatastructures As Object) _
        Implements IEcosimRunCompletedPlugin.EcosimRunCompleted

        Dim parms As cParameters = Me.m_data.Parameters

        ' Abort if no params
        If (parms Is Nothing) Then Return
        ' Abort if not allowed to run with Ecosim
        If (parms.RunWithEcosim = False) Then Return


        If (Me.m_dataBroadcaster IsNot Nothing) Then
            Me.m_dataBroadcaster.BroadcastData(Me.Name, Me.m_ddx)
        End If

        If Me.AutoSave Then
            Me.m_model.SaveResults(Me.m_data, Me.m_result)
        End If

    End Sub

#End Region ' Ecosim integration

#Region " Data Exchange "

    Private Sub BroadcastResults(ByVal iTimeStep As Integer)

        If (Me.m_dataBroadcaster IsNot Nothing) And (Me.m_bIsEnabled = True) Then

            ' Fill exchange data based on the type of computed results
            Select Case Me.m_result.RunType
                Case cModel.eRunTypes.Ecopath
                    Me.m_ddx.m_runType = New cEcopathRunType()
                Case cModel.eRunTypes.Ecosim
                    Me.m_ddx.m_runType = New cEcosimRunType()
            End Select

            Me.m_ddx.Resize(Me.m_data.Core.nFleets)
            Me.m_ddx.m_iTimeStep = iTimeStep

            Me.Populate(DirectCast(Me.m_ddx.Total, cPluginData.cVCEconomicData), iTimeStep, 0)
            For iFleet As Integer = 1 To Me.m_data.Core.nFleets - 1
                Me.Populate(DirectCast(Me.m_ddx.Subtotal(iFleet), cPluginData.cVCEconomicData), iTimeStep, iFleet)
            Next iFleet

            Me.m_dataBroadcaster.BroadcastData("ValueChain", Me.m_ddx)
        End If

    End Sub

    Private Sub Populate(ByVal data As cPluginData.cVCEconomicData, ByVal iTimeStep As Integer, ByVal iFleet As Integer)

        data.m_sCost = Me.GetValue(cResults.eVariableType.Cost, iTimeStep, iFleet)
        data.m_sCostInput = Me.GetValue(cResults.eVariableType.CostRawmaterial, iTimeStep, iFleet)
        data.m_sCostLicenseObservers = Me.GetValue(cResults.eVariableType.CostManagementRoyaltyCertificationObservers, iTimeStep, iFleet)
        data.m_sCostSalariesShares = Me.GetValue(cResults.eVariableType.CostSalariesShares, iTimeStep, iFleet)
        data.m_sCostTaxes = Me.GetValue(cResults.eVariableType.CostTaxes, iTimeStep, iFleet)
        data.m_sCostTotalInputOther = Me.GetValue(cResults.eVariableType.CostTotalInputOther, iTimeStep, iFleet)
        data.m_sNumberOfDependentsTotal = Me.GetValue(cResults.eVariableType.NumberOfDependentsTotal, iTimeStep, iFleet)
        data.m_sNumberOfJobsFemaleTotal = Me.GetValue(cResults.eVariableType.NumberOfJobsFemaleTotal, iTimeStep, iFleet)
        data.m_sNumberOfJobsMaleTotal = Me.GetValue(cResults.eVariableType.NumberOfJobsMaleTotal, iTimeStep, iFleet)
        data.m_sNumberOfJobsTotal = Me.GetValue(cResults.eVariableType.NumberOfJobsTotal, iTimeStep, iFleet)
        data.m_sNumberOfOwnerDependents = Me.GetValue(cResults.eVariableType.NumberOfOwnerDependents, iTimeStep, iFleet)
        data.m_sNumberOfWorkerDependents = Me.GetValue(cResults.eVariableType.NumberOfWorkerDependents, iTimeStep, iFleet)
        data.m_sProduction = Me.GetValue(cResults.eVariableType.Production, iTimeStep, iFleet)
        data.m_sProductionLive = Me.GetValue(cResults.eVariableType.ProductionLive, iTimeStep, iFleet)
        data.m_sProfit = Me.GetValue(cResults.eVariableType.Profit, iTimeStep, iFleet)
        data.m_sRevenueProductsMain = Me.GetValue(cResults.eVariableType.RevenueProductsMain, iTimeStep, iFleet)
        data.m_sRevenueProductsOther = Me.GetValue(cResults.eVariableType.RevenueProductsOther, iTimeStep, iFleet)
        data.m_sRevenueSubsidies = Me.GetValue(cResults.eVariableType.RevenueSubsidies, iTimeStep, iFleet)
        data.m_sRevenueTotal = Me.GetValue(cResults.eVariableType.RevenueTotal, iTimeStep, iFleet)
        data.m_sThroughput = Me.GetValue(cResults.eVariableType.TotalUtility, iTimeStep, iFleet)

    End Sub

    Private Function GetValue(vn As cResults.eVariableType, iTimeStep As Integer, iFleet As Integer) As Single
        Return Me.m_result.GetTimeStepTotal(vn, iTimeStep, Nothing, iFleet, cResults.GetVariableContributionType(vn))
    End Function


    Public Sub Broadcaster(ByVal broadcaster As EwEPlugin.Data.IDataBroadcaster) _
        Implements EwEPlugin.Data.IDataProducerPlugin.Broadcaster

        Me.m_dataBroadcaster = broadcaster

    End Sub

    Public Function IsDataAvailable(ByVal typeData As System.Type, Optional ByVal runType As IRunType = Nothing) As Boolean _
        Implements EwEPlugin.Data.IDataProducerPlugin.IsDataAvailable

        Dim bIsAvailable As Boolean = False

        If (Me.m_data IsNot Nothing) Then
            If (Me.m_data.Parameters IsNot Nothing) Then
                Try
                    If (typeData Is GetType(IEconomicData)) Then
                        If TypeOf (runType) Is cEcopathRunType Then
                            bIsAvailable = Me.m_data.Parameters.RunWithEcopath
                        ElseIf TypeOf (runType) Is cEcosimRunType Then
                            bIsAvailable = Me.m_data.Parameters.RunWithEcosim
                        ElseIf TypeOf (runType) Is cSearchRunType Then
                            bIsAvailable = Me.m_data.Parameters.RunWithSearches
                        End If
                    End If
                Catch ex As Exception
                    bIsAvailable = False
                End Try
            End If
        End If

        Return bIsAvailable

    End Function

    Public Function GetDataByType(ByVal typeData As System.Type, _
                                  ByRef data As EwEPlugin.Data.IPluginData) As Boolean _
        Implements EwEPlugin.Data.IDataProducerPlugin.GetDataByType

        data = Nothing
        If (typeData Is GetType(IEconomicData)) Then
            data = Me.m_ddx
        End If

        Return (data IsNot Nothing)

    End Function

    Public Function IsEnabled(ByVal typeData As System.Type, ByVal runtype As IRunType) As Boolean _
         Implements EwEPlugin.Data.IDataProducerPlugin.IsEnabled

        If Not (typeData Is GetType(IEconomicData)) Then Return False

        Dim parms As cParameters = Me.Data.Parameters
        If (parms Is Nothing) Then Return False

        If TypeOf runtype Is cEcopathRunType Then
            Return parms.RunWithEcosim
        End If

        If TypeOf runtype Is cEcosimRunType Then
            Return parms.RunWithEcosim
        End If

        If TypeOf runtype Is cSearchRunType Then
            Return parms.RunWithSearches
        End If

        Return False

    End Function

    Public Sub SetEnabled(ByVal typeData As System.Type, ByVal runType As IRunType, ByVal bEnabled As Boolean) _
        Implements EwEPlugin.Data.IDataProducerPlugin.SetEnabled

        Dim parms As cParameters = Me.Data.Parameters
        If (parms Is Nothing) Then Return
        If Not (typeData Is GetType(IEconomicData)) Then Return

        If TypeOf runType Is cEcopathRunType Then
            parms.RunWithEcopath = bEnabled
        End If

        If TypeOf runType Is cEcosimRunType Then
            parms.RunWithEcosim = bEnabled
        End If

        If TypeOf runType Is cFishingPolicySearchRunType Then
            parms.RunWithSearches = bEnabled
        End If

    End Sub

    Public Function IsEnabled1() As Boolean _
        Implements EwEPlugin.Data.IDataProducerPlugin.IsEnabled
        Return Me.m_bIsEnabled
    End Function

    Public Function SetEnabled1(ByVal bEnable As Boolean) As Boolean _
        Implements EwEPlugin.Data.IDataProducerPlugin.SetEnabled
        Me.m_bIsEnabled = bEnable
    End Function
#End Region ' Data exchange

#Region " Search "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when a search is initialized.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub SearchInitialized(SearchDatastructures As Object) _
        Implements EwEPlugin.ISearchPlugin.SearchInitialized

        Dim ds As cSearchDatastructures = DirectCast(SearchDatastructures, cSearchDatastructures)
        Dim parms As cParameters = Me.m_data.Parameters

        ' Abort if no params
        If (parms Is Nothing) Then Return

        ' Only respond to fishing policy search when allowed to respond
        If (parms.RunWithSearches = False) Then Return
        ' Only respond to fishing policy search
        If (ds.SearchMode <> eSearchModes.FishingPolicy) Then Return

        ' Reset values that this plug-in will (hopefully) deliver.
        'VC090402: updated the blowe to use the value chain searchDS parameters (which is what I need)
        ds.profit = 0
        ds.totval = 0
        ds.Employ = 0

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when a search is starting.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub SearchIterationsStarting() Implements _
        EwEPlugin.ISearchPlugin.SearchIterationsStarting
        ' NOP
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point implementation, called when a search step has been 
    ''' performed. Implemented to provide economic search results.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub PostRunSearchResults(ByVal SearchDatastructures As Object) _
        Implements EwEPlugin.ISearchPlugin.PostRunSearchResults

        Dim ds As cSearchDatastructures = DirectCast(SearchDatastructures, cSearchDatastructures)
        Dim parms As cParameters = Me.m_data.Parameters

        ' Abort if no params
        If (parms Is Nothing) Then Return

        ' Only respond to fishing policy search when allowed to respond
        If (parms.RunWithSearches = False) Then Return
        ' Only respond to fishing policy search
        If (ds.SearchMode <> eSearchModes.FishingPolicy) Then Return

        'ds.SectorProfit = Me.Results.GetTotal(cEcostResults.eVariableType.Profit)
        'ds.SectorTotalValue = Me.Results.GetTotal(cEcostResults.eVariableType.RevenueTotal)
        'ds.SectorJobs = Me.Results.GetTotal(cEcostResults.eVariableType.NumberOfJobsTotal)

    End Sub

    Public Sub SearchCompleted(SearchDatastructures As Object) _
        Implements EwEPlugin.ISearchPlugin.SearchCompleted

    End Sub

#End Region ' Search

#End Region ' Plugin point implementation

#Region " Exhibitionism "

    Public ReadOnly Property Data() As cData
        Get
            Return Me.m_data
        End Get
    End Property

    Public ReadOnly Property Model() As cModel
        Get
            Return Me.m_model
        End Get
    End Property

    Public ReadOnly Property Results() As cResults
        Get
            Return Me.m_result
        End Get
    End Property

    Public ReadOnly Property Core() As cCore
        Get
            Return Me.m_core
        End Get
    End Property

    Public ReadOnly Property Context() As cUIContext
        Get
            Return Me.m_uic
        End Get
    End Property

#End Region ' Exhibitionism

#Region " Helpers "

    Private Sub OnEcopathMessage(ByRef msg As cMessage)

        ' Something in Ecopath has changed
        Try
            Me.m_linkman.OnEcopathMessage(msg)
        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Function HasInterface() As Boolean
        If Me.m_form Is Nothing Then Return False
        If Me.m_form.IsDisposed Then Return False
        Return True
    End Function

#End Region ' Helpers

#Region " Autosave "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IAutoSavePlugin.AutoSave"/>
    ''' -----------------------------------------------------------------------
    Public Property AutoSave As Boolean _
        Implements EwEPlugin.IAutoSavePlugin.AutoSave
        Get
            Return My.Settings.AutosaveResults
        End Get
        Set(value As Boolean)
            My.Settings.AutosaveResults = value
            My.Settings.Save()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IAutoSavePlugin.AutoSaveName"/>
    ''' -----------------------------------------------------------------------
    Public Function AutoSaveName() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveName
        Return My.Resources.GENERIC_CAPTION
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IAutoSavePlugin.AutoSaveOutputPath"/>
    ''' -----------------------------------------------------------------------
    Public Function AutoSaveSubPath() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveOutputPath
        Return ""
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IAutoSavePlugin.AutoSaveType"/>
    ''' -----------------------------------------------------------------------
    Public Function AutoSaveType() As EwEUtils.Core.eAutosaveTypes _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveType
        Return eAutosaveTypes.NotSet
    End Function

#End Region ' Autosave

End Class
