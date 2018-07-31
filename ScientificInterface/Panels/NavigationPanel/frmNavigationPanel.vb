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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterface.Ecopath.Controls
Imports ScientificInterface.Ecopath.Input
Imports ScientificInterface.Ecopath.Output
Imports ScientificInterface.Ecopath.Tools
Imports ScientificInterface.Ecosim
Imports ScientificInterface.Ecospace
Imports ScientificInterfaceShared.Integration
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

''' ---------------------------------------------------------------------------
''' <summary>
''' Navigation tree panel; contains the navigation structure that provides uniform
''' access to the screens of the EwE user interface. All interaction with the
''' tree is standardized and handled in this class.
''' </summary>
''' <remarks>
''' <para>The Navigation Panel will not actually create or highlight the GUI items 
''' that it provides access to. Instead, the panel will outsource this functionality 
''' via the central <see cref="ScientificInterfaceShared.Commands.cCommandHandler">CommandHandler</see> 
''' and the <see cref="ScientificInterfaceShared.Commands.cNavigationCommand">Navigation command</see>.</para>
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class frmNavigationPanel

    Private m_uic As cUIContext = Nothing
    Private m_nodecontroller As cTreeViewNodeController = Nothing
    Private m_pluginManager As cPluginManager = Nothing
    Private m_ntPluginHandler As cPluginNavTreeHandler = Nothing
    Private m_tnSelected As TreeNode = Nothing

    Private Enum eNodeImages As Integer
        Input
        Output
        Tool
        Ecopath
        Ecosim
        Ecospace
        Ecotracer
    End Enum

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor; initializes a new instance of the navigation panel.
    ''' </summary>
    ''' <param name="uic">The UI contezt to connect to.</param>
    ''' <param name="pluginManager">The plug-in manager to obtain tree 
    ''' extensions for.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext,
                   ByVal pluginManager As EwEPlugin.cPluginManager)

        ' Sanity check
        Debug.Assert(uic IsNot Nothing)

        ' Store refs
        Me.m_uic = uic
        Me.m_pluginManager = pluginManager

        Try
            ' Hit 'em, Jimmy
            Me.InitializeComponent()
        Catch ex As Exception

        End Try

    End Sub

    Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)

        If bDisposing Then
            If components IsNot Nothing Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(bDisposing)

    End Sub

#End Region ' Construction / destruction

#Region " Form overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Dim nodeModel As TreeNode = Nothing
        Dim nodeTools As TreeNode = Nothing
        Dim nodeInput As TreeNode = Nothing
        Dim nodeOutput As TreeNode = Nothing
        Dim nodeFolder As TreeNode = Nothing

        Me.Icon = Icon.FromHandle(ScientificInterfaceShared.My.Resources.NavHS.GetHicon)

        ' Put all the list here
        Me.m_nodecontroller = New cTreeViewNodeController()
        Me.m_nodecontroller.Attach(Me.m_uic, Me.m_tvNavigation)

        ' Build images list
        ' EwE 6.6 and newer will no longer distinguish between folder and item nodes
        Me.m_ilTreeIcons.Images.Clear()
        For key As Integer = 0 To [Enum].GetValues(GetType(eNodeImages)).Length - 1
            Dim img As Image = Nothing
            Select Case key
                Case eNodeImages.Ecopath : img = SharedResources.nav_ecopath
                Case eNodeImages.Ecosim : img = SharedResources.nav_ecosim
                Case eNodeImages.Ecospace : img = SharedResources.nav_ecospace
                Case eNodeImages.Ecotracer : img = SharedResources.nav_ecotracer
                Case eNodeImages.Input : img = SharedResources.nav_input
                Case eNodeImages.Output : img = SharedResources.nav_output
                Case eNodeImages.Tool : img = SharedResources.nav_tool
                Case Else : Debug.Assert(False)
            End Select
            Me.m_ilTreeIcons.Images.Add(img)
        Next

        ' JS 8Nov16: From now on build the node list manually. Too often we lose nodes when merging SVN branches; 
        '            SVN cannot merge the binary blob that the TreeView uses to store its node configuration.
        '            It is less easy to locate node names for hosting plug-ins in a coded tree, but at least we limit damage when merging
        Me.m_tvNavigation.Nodes.Clear()

        ' -- Ecopath --
        ' input
        nodeModel = Me.m_nodecontroller.Add(SharedResources.HEADER_ECOPATH, "ndParameterization", eCoreExecutionState.EcopathLoaded, Nothing, eNodeImages.Ecopath)
        nodeInput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_INPUT, "ndEcopathInput", eCoreExecutionState.EcopathLoaded, Nothing, eNodeImages.Input, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_PARAMETERS, "ndModelParameters", eCoreExecutionState.EcopathLoaded, GetType(frmModelParameters), eNodeImages.Input, nodeInput, "Model description.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_BASICINPUT, "ndBasicInput", eCoreExecutionState.EcopathLoaded, GetType(frmBasicInput), eNodeImages.Input, nodeInput, "Basic input.htm.htm", True)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_DIETCOMP, "ndDietComposition", eCoreExecutionState.EcopathLoaded, GetType(frmDietComp), eNodeImages.Input, nodeInput, "Diet composition.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_DETRITUSFATE, "ndDetritusFate", eCoreExecutionState.EcopathLoaded, GetType(gridDetritusFate), eNodeImages.Input, nodeInput, "Detritus fate.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_OTHERPRODUCTION, "ndOtherProduction", eCoreExecutionState.EcopathLoaded, GetType(gridOtherProduction), eNodeImages.Input, nodeInput, "Other production.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_FISHERY, "ndFishery", eCoreExecutionState.EcopathLoaded, Nothing, eNodeImages.Input, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_FLEETS, "ndDefFleets", eCoreExecutionState.EcopathLoaded, GetType(frmFisheryBasicInput), eNodeImages.Input, nodeFolder, "Definition of fleets.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_FISHERY_LANDINGS, "ndLandings", eCoreExecutionState.EcopathLoaded, GetType(gridFisheryInputLandings), eNodeImages.Input, nodeFolder, "Landings.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_FISHERY_DISCARDS, "ndDiscards", eCoreExecutionState.EcopathLoaded, GetType(gridFisheryInputDiscards), eNodeImages.Input, nodeFolder, "Discards.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_FISHERY_DISCARDMORTRATE, "ndDiscardMortRate", eCoreExecutionState.EcopathLoaded, GetType(gridFisheryInputDiscardMort), eNodeImages.Input, nodeFolder)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_FISHERY_DISCARDFATE, "ndDiscardFate", eCoreExecutionState.EcopathLoaded, GetType(gridFisheryInputDiscardFate), eNodeImages.Input, nodeFolder, "Discard fate.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_OFFVESSELPRICE, "ndOffVesselPrice", eCoreExecutionState.EcopathLoaded, GetType(gridFisheryOffVesselValue), eNodeImages.Input, nodeFolder, "Market price.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_NONMARKTEPRICE, "ndNonMarketPrice", eCoreExecutionState.EcopathLoaded, GetType(gridFisheryInputNonMarketPrice), eNodeImages.Input, nodeFolder, "Non market price.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_TOOLS, "ndEcopathInputTools", eCoreExecutionState.EcopathLoaded, Nothing, eNodeImages.Tool, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_INPUT_GROWTH, "ndGrowthParameters", eCoreExecutionState.EcopathLoaded, GetType(gridGrowthParameters), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_PEDIGREE, "ndPedigree", eCoreExecutionState.EcopathLoaded, GetType(frmPedigree), eNodeImages.Input, nodeFolder, "pedigree.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_INPUT_TRAITS, "ndEcopathInputTraits", eCoreExecutionState.EcopathLoaded, GetType(frmTaxonInput), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        ' output
        nodeOutput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_OUTPUT, "ndEcopathOutput", eCoreExecutionState.EcopathCompleted, Nothing, eNodeImages.Output, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_BASICESTIMATES, "ndBasicEstimates", eCoreExecutionState.EcopathCompleted, GetType(gridBasicEstimates), eNodeImages.Output, nodeOutput, "Basic estimates.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_KEYINDICES, "ndKeyIndices", eCoreExecutionState.EcopathCompleted, GetType(gridKeyIndices), eNodeImages.Output, nodeOutput, "Key indices.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_MORTALITRATES, "ndMortalityRates", eCoreExecutionState.EcopathCompleted, Nothing, eNodeImages.Output, nodeOutput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_MORTALITIES, "ndMortCoef", eCoreExecutionState.EcopathCompleted, GetType(gridMortalityCoefficients), eNodeImages.Output, nodeFolder, "Mortalities.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_PREDMORTRATE, "ndPredMort", eCoreExecutionState.EcopathCompleted, GetType(gridMortalityPredation), eNodeImages.Output, nodeFolder, "Predation mortality.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_FISHMORTRATE, "ndFleetFishingMortality", eCoreExecutionState.EcopathCompleted, GetType(gridFleetFishingMortality), eNodeImages.Output, nodeFolder) ' ToDo: connect to help

        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_CONSUMPTION, "ndConsumption", eCoreExecutionState.EcopathCompleted, GetType(gridConsumption), eNodeImages.Output, nodeOutput, "Consumption.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_RESPIRATION, "ndRespiration", eCoreExecutionState.EcopathCompleted, GetType(gridRespiration), eNodeImages.Output, nodeOutput, "Respiration.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_NICHEOVERLAP, "ndNicheOverlap", eCoreExecutionState.EcopathCompleted, Nothing, eNodeImages.Output, nodeOutput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_PREYOVERLAP, "ndPreyOverlap", eCoreExecutionState.EcopathCompleted, GetType(gridNicheOverlapPrey), eNodeImages.Output, nodeFolder, "Niche overlap.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_PREDATOROVERLAP, "ndPredatorOverlap", eCoreExecutionState.EcopathCompleted, GetType(gridNicheOverlapPredator), eNodeImages.Output, nodeFolder, "Niche overlap.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_NICHEOVERLAPPLOT, "ndNichePredPreyPlot", eCoreExecutionState.EcopathCompleted, GetType(frmNichePredPreyPlot), eNodeImages.Output, nodeFolder)

        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_ELECTIVITY, "ndElectivity", eCoreExecutionState.EcopathCompleted, GetType(gridElectivity), eNodeImages.Output, nodeOutput, "Electivity.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_SEARCHRATES, "ndSearchRates", eCoreExecutionState.EcopathCompleted, GetType(gridSearchRates), eNodeImages.Output, nodeOutput, "Search rates.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_FISHERY, "ndFisheryOutput", eCoreExecutionState.EcopathCompleted, Nothing, eNodeImages.Output, nodeOutput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_CATCH, "ndEcopathCatch", eCoreExecutionState.EcopathCompleted, GetType(gridFisheryOutputCatch), eNodeImages.Output, nodeFolder, "Fishery (Ecopath parameterization).htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_VALUE, "ndEcopathValue", eCoreExecutionState.EcopathCompleted, GetType(gridFisheryOutputValue), eNodeImages.Output, nodeFolder, "Fishery (Ecopath parameterization).htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_DISCARDMORT, "ndEcopathDiscardMortality", eCoreExecutionState.EcopathCompleted, GetType(gridFisheryOutputDiscardMort), eNodeImages.Output, nodeFolder)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_DISCARSURV, "ndEcopathDiscardSurvival", eCoreExecutionState.EcopathCompleted, GetType(gridFisheryOutputDiscardSurvival), eNodeImages.Output, nodeFolder)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_LANDINGS, "ndEcopathLandings", eCoreExecutionState.EcopathCompleted, GetType(gridFisheryOutputLandings), eNodeImages.Output, nodeFolder)

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD, "ndParticleSizeDistribution", eCoreExecutionState.EcopathCompleted, Nothing, eNodeImages.Output, nodeOutput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_OUTPUT_RUN, "ndRunPSD", eCoreExecutionState.EcopathLoaded, GetType(RunPSD), eNodeImages.Output, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_OUTPUT_GROWTHESTIMATES, "ndGrowthEstimates", eCoreExecutionState.PSDCompleted, GetType(gridPSDGrowthEstimates), eNodeImages.Output, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_OUTPUT_GROWTHCONTR, "ndPSDContributionPlot", eCoreExecutionState.PSDCompleted, GetType(PSDContributionPlot), eNodeImages.Output, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_OUTPUT_GROWTHCONTRRESULT, "ndPSDContributionResult", eCoreExecutionState.PSDCompleted, GetType(gridPSDContributionResult), eNodeImages.Output, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_OUTPUT_PLOTGROUP, "ndPSDPlotByGroup", eCoreExecutionState.PSDCompleted, GetType(PSDPlotByGroup), eNodeImages.Output, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_PSD_OUTPUT_PLOTWEIGHT, "ndSizeWeightPlot", eCoreExecutionState.PSDCompleted, GetType(SizeWeightPlot), eNodeImages.Output, nodeFolder) ' ToDo: connect to help

        ' tools
        nodeTools = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_TOOLS, "ndEcopathOutputTools", eCoreExecutionState.EcopathCompleted, Nothing, eNodeImages.Tool, nodeOutput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_FLOWDIAGRAM, "ndFlowDiagram", eCoreExecutionState.EcopathCompleted, GetType(FlowDiagram.frmEcopathFlowDiagram), eNodeImages.Output, nodeTools, "Flow diagram.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOPATH_OUTPUT_STATISTICS, "ndEcopathStats", eCoreExecutionState.EcopathCompleted, GetType(gridEcopathStatistics), eNodeImages.Output, nodeTools) ' ToDo: connect to help

        ' -- Ecosim --
        ' input
        nodeModel = Me.m_nodecontroller.Add(SharedResources.HEADER_ECOSIM, "ndTimeDynamic", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Ecosim)
        nodeInput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_INPUT, "ndEcosimInput", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Input, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_PARAMETERS, "ndEcosimParameters", eCoreExecutionState.EcosimLoaded, GetType(frmEcosimParameters), eNodeImages.Input, nodeInput, "Ecosim parameters.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_GROUPS, "ndGroupInfo", eCoreExecutionState.EcosimLoaded, GetType(gridEcosimGroupInput), eNodeImages.Input, nodeInput, "Group info.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_VULNERABILITES, "ndVulnerabilities", eCoreExecutionState.EcosimLoaded, GetType(frmVulnerabilities), eNodeImages.Input, nodeInput, "Vulnerabilities flow control.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_TIMESERIES, "ndTimeSeries", eCoreExecutionState.EcosimLoaded, GetType(frmTimeSeries), eNodeImages.Input, nodeInput, "Time series.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_MEDIATION, "ndMediation", eCoreExecutionState.EcosimLoaded, GetType(frmMediationFunction), eNodeImages.Input, nodeInput, "Forcing function.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYMEDCONS, "ndApplyMediation", eCoreExecutionState.EcosimLoaded, GetType(frmApplyMedConsumer), eNodeImages.Input, nodeFolder, "Mediation.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYMEDPROD, "ndApplyMediationPP", eCoreExecutionState.EcosimLoaded, GetType(frmApplyMedPP), eNodeImages.Input, nodeFolder, "Apply mediation.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYMEDDET, "ndApplyMediationDetritus", eCoreExecutionState.EcosimLoaded, GetType(frmApplyMedDetritus), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

#If DEBUG Then
        'MSE Batch
        '.Add("ndMSEBatchTFM", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchTFM), "") ' ToDo: connect to help
        '.Add("ndMSEBatchFixedF", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchFixedF), "") ' ToDo: connect to help
        ''jb Form not done yet
        ' '' .Add("ndMSEBatchTAC", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchTAC), "") ' ToDo: connect to help
        '.Add("ndMSEBatchParameters", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchParameters), "") ' ToDo: connect to help
        '.Add("ndRunBatch", eCoreExecutionState.EcosimLoaded, GetType(frmMSERunBatch), "") ' ToDo: connect to help
#End If

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_FORCINGFUNCTION, "ndForcingFunction", eCoreExecutionState.EcosimLoaded, GetType(frmForcingFunction), eNodeImages.Input, nodeInput, "Forcing function.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYFFCONS, "ndApplyFFConsumer", eCoreExecutionState.EcosimLoaded, GetType(frmApplyFFConsumer), eNodeImages.Input, nodeFolder, "Apply forcing function consumer.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYFFPROD, "ndApplyFFPP", eCoreExecutionState.EcosimLoaded, GetType(frmApplyFFPrimaryProducer), eNodeImages.Input, nodeFolder, "Apply forcing function primary.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYFFDET, "ndApplyFFDetritus", eCoreExecutionState.EcosimLoaded, GetType(frmApplyFFDetritus), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYOTHERMORT, "ndApplyFFOtherMort", eCoreExecutionState.EcosimLoaded, GetType(frmApplyFFOtherMort), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_ENVIRONMENTALRESPONSE, "ndEcosimEnvironmentalResponse", eCoreExecutionState.EcosimLoaded, GetType(frmEcosimFunctionalResponse), eNodeImages.Input, nodeInput) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYENVRESPONSE, "ndFunctionalResponseApply", eCoreExecutionState.EcosimLoaded, GetType(gridApplyEcosimEnvironmentalResponses), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_EGGPROD, "ndEP", eCoreExecutionState.EcosimLoaded, GetType(frmEggProduction), eNodeImages.Input, nodeInput, "Egg production.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_APPLYEGGPROD, "ndApplyEP", eCoreExecutionState.EcosimLoaded, GetType(ApplyEP), eNodeImages.Input, nodeFolder, "Apply egg production.htm")

        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_FISHINGEFFORT, "ndFishingEffort", eCoreExecutionState.EcosimLoaded, GetType(frmFishingEffort), eNodeImages.Input, nodeInput) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_FISHINGMORT, "ndFishingMortality", eCoreExecutionState.EcosimLoaded, GetType(frmFishingMortality), eNodeImages.Input, nodeInput) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_FLEETSIZEDYNAMICS, "ndFleetSizeDynamics", eCoreExecutionState.EcosimLoaded, GetType(gridEcosimFleetSizeDynamics), eNodeImages.Input, nodeInput, "Fleet size dynamics.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_INPUT_PRICEELAST, "ndPriceElasticity", eCoreExecutionState.EcosimLoaded, GetType(frmPriceElasticity), eNodeImages.Input, nodeInput) ' ToDo: connect to help
        'xxxxxxxxxxxxxxxxxxxxxxxx
        'jb test catchability form
        Me.m_nodecontroller.Add("Catchability", "ndCatchability", eCoreExecutionState.EcosimLoaded, GetType(frmCatchability), eNodeImages.Input, nodeInput) ' ToDo: connect to help
        'xxxxxxxxxxxxxxxxxxxxxxxxx
        ' output
        nodeOutput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_OUTPUT, "ndEcosimOutput", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Output, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_RUN, "ndRunEcosim", eCoreExecutionState.EcosimLoaded, GetType(frmRunEcosim), eNodeImages.Output, nodeOutput, "Run Ecosim.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_GROUPPLOTS, "ndEcosimPlots", eCoreExecutionState.EcosimCompleted, GetType(frmEcosimOutputGroupPlots), eNodeImages.Output, nodeOutput, "Ecosim plot.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_FLEETPLOTS, "ndEcosimFleetPlots", eCoreExecutionState.EcosimCompleted, GetType(frmEcosimOutputFleetPlots), eNodeImages.Output, nodeOutput, "Ecosim fleet plot.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_RESULTS, "ndEcosimResults", eCoreExecutionState.EcosimCompleted, GetType(frmEcosimResults), eNodeImages.Output, nodeOutput, "Ecosim results.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_ALLFITS, "ndEcosimAllFits", eCoreExecutionState.EcosimCompleted, GetType(frmShowAllFits), eNodeImages.Output, nodeOutput, "Ecosim results.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_SRPLOT, "ndSRPlot", eCoreExecutionState.EcosimLoaded, GetType(frmStockRecruitmentPlot), eNodeImages.Output, nodeOutput, "Stock recruitment S R plot.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_OUTPUT_SUSTPLOT, "ndSuitabilityPlot", eCoreExecutionState.EcosimCompleted, GetType(SuitabilityPlot), eNodeImages.Output, nodeOutput) ' ToDo: connect to help

        nodeTools = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_TOOLS, "ndEcosimOutputTools", eCoreExecutionState.EcosimCompleted, Nothing, eNodeImages.Tool, nodeOutput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_FLOWDIAGRAM, "ndEcosimFlowDiagram", eCoreExecutionState.EcosimCompleted, GetType(frmEcosimFlowDiagram), eNodeImages.Output, nodeTools)

        ' tools
        nodeTools = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_TOOLS, "ndEcosimTools", eCoreExecutionState.EcosimCompleted, Nothing, eNodeImages.Tool, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_MONTECARLO, "ndMCRun", eCoreExecutionState.EcosimLoaded, GetType(frmMCRun), eNodeImages.Output, nodeTools, "Monte Carlo runs.htm") ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_FISHINGPOLICY, "ndFishingPolicySearch", eCoreExecutionState.EcosimLoaded, GetType(frmFishingPolicySearch), eNodeImages.Output, nodeTools, "Fishing policy search.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_FITTOTIMESERIES, "ndFitToTimeSeries", eCoreExecutionState.EcosimLoaded, GetType(frmFitToTimeSeries), eNodeImages.Output, nodeTools, "Fit to time series.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSIM_FMSY, "ndMSY", eCoreExecutionState.EcosimLoaded, GetType(Ecosim.frmMSY), eNodeImages.Output, nodeTools) ' ToDo: connect to help

        ' -- Ecospace --
        nodeModel = Me.m_nodecontroller.Add(SharedResources.HEADER_ECOSPACE, "ndSpatialDynamic", eCoreExecutionState.EcospaceLoaded, Nothing, eNodeImages.Ecospace)
        ' input
        nodeInput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_INPUT, "ndEcospaceInput", eCoreExecutionState.EcospaceLoaded, Nothing, eNodeImages.Input, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_PARAMS, "ndEcospaceParameters", eCoreExecutionState.EcospaceLoaded, GetType(frmEcospaceParameters), eNodeImages.Input, nodeInput, "Ecospace parameters.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_MAPS, "ndBasemap", eCoreExecutionState.EcospaceLoaded, GetType(Basemap.frmEcospaceMap), eNodeImages.Input, nodeInput, "Basemap.htm")

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_HABCAP, "ndHabCap", eCoreExecutionState.EcospaceLoaded, GetType(frmForagingResponse), eNodeImages.Input, nodeInput) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_HABCAPMODEL, "ndHabCapModel", eCoreExecutionState.EcospaceLoaded, GetType(frmCapacityCalcType), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_HABCAPAPPLY, "ndHabCapApply", eCoreExecutionState.EcospaceLoaded, GetType(frmApplyForagingResponses), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_HABITATPREFS, "ndHabitatPrefs", eCoreExecutionState.EcospaceLoaded, GetType(Ecospace.gridHabitatPreference), eNodeImages.Input, nodeFolder, "Assign habitats.htm")

        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_DISPERSAL, "ndDispersal", eCoreExecutionState.EcospaceLoaded, GetType(gridEcospaceDispersal), eNodeImages.Input, nodeInput, "Dispersal.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_ADVECTION, "ndAdvection", eCoreExecutionState.EcospaceLoaded, GetType(Advection.frmAdvection), eNodeImages.Input, nodeInput)

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_FISHERY, "ndEcospaceFishery", eCoreExecutionState.EcospaceLoaded, Nothing, eNodeImages.Input, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_MPA, "ndEcospaceMPA", eCoreExecutionState.EcospaceLoaded, GetType(frmMPAs), eNodeImages.Input, nodeFolder, "Ecospace Fishery.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_MPAENFORCEMENT, "ndEcospaceMPAEnforcement", eCoreExecutionState.EcospaceLoaded, GetType(frmEcospaceMPAEnforcement), eNodeImages.Input, nodeFolder, "Ecospace Fishery.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_HABFISHERY, "ndEcospaceHabitatFishery", eCoreExecutionState.EcospaceLoaded, GetType(frmEcospaceHabitatFishery), eNodeImages.Input, nodeFolder, "Ecospace Fishery.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_FLEETDYNAMICS, "ndEcospaceFisheriesDynamics", eCoreExecutionState.EcospaceLoaded, GetType(gridEcospaceHabitatDyncamis), eNodeImages.Input, nodeFolder, "Ecospace Fishery.htm")

        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_INPUT_EXTERNALDATA, "ndEcospaceExtData", eCoreExecutionState.EcospaceLoaded, GetType(frmSpatialTimeSeries), eNodeImages.Input, nodeInput, "")

        '.Add("ndEcospaceScenario", eCoreExecutionState.EcospaceLoaded, GetType(dlgEcospaceScenario)) ' ToDo: connect to help

        ' output
        nodeOutput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_OUTPUT, "ndEcospaceOutput", eCoreExecutionState.EcospaceLoaded, Nothing, eNodeImages.Output, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_OUTPUT_RUN, "ndRunEcospace", eCoreExecutionState.EcospaceLoaded, GetType(frmRunEcospace), eNodeImages.Output, nodeOutput, "Run Ecospace.htm")
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_OUTPUT_RESULTS, "ndEcospaceResults", eCoreExecutionState.EcospaceCompleted, GetType(frmEcospaceResults), eNodeImages.Output, nodeOutput) ' ToDo: connect to help

        ' tools
        nodeTools = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_TOOLS, "ndEcospaceTools", eCoreExecutionState.EcospaceCompleted, Nothing, eNodeImages.Tool, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOSPACE_MPAOPT, "ndMPAOptimizations", eCoreExecutionState.EcospaceLoaded, GetType(frmMPAOptimizations), eNodeImages.Output, nodeTools)

        ' global tools
        nodeTools = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_TOOLS, "ndTools", eCoreExecutionState.EcopathLoaded, Nothing, eNodeImages.Tool, Nothing)

        ' -- Ecotracer -- (placed on the tree root now, yippee)
        nodeModel = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOTRACER, "ndEcotracer", eCoreExecutionState.EcotracerLoaded, Nothing, eNodeImages.Ecotracer, nodeTools)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOTRACER_PARAMS, "ndEcoTracer_Pram", eCoreExecutionState.EcotracerLoaded, GetType(Ecotracer.frmEcotracerParameters), eNodeImages.Input, nodeModel) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOTRACER_INPUT, "ndEcoTracer_Input", eCoreExecutionState.EcotracerLoaded, GetType(Ecotracer.frmEcotracerInput), eNodeImages.Input, nodeModel) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_ECOTRACER_OUTPUT, "ndEcoTracer_Output", eCoreExecutionState.EcotracerLoaded, GetType(frmEcotracerOutput), eNodeImages.Output, nodeModel) ' ToDo: connect to help

        ' -- MSE --
        nodeModel = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE, "ndMSE", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Tool, nodeTools)
        ' input
        nodeInput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_INPUT, "ndMSEInput", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Input, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_OPTIONS, "ndOptions", eCoreExecutionState.EcosimLoaded, GetType(frmMSEOptions), eNodeImages.Input, nodeInput) ' ToDo: connect to help

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_REGULATORY, "ndRegInput", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Input, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_TFM, "ndRegFishingMort", eCoreExecutionState.EcosimLoaded, GetType(frmTargetFishingMortalityPolicy), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_FIXEDESCAPE, "ndRefFixedEscape", eCoreExecutionState.EcosimLoaded, GetType(gridFixedEscapement), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_QUOTASHARE, "ndQuotaShare", eCoreExecutionState.EcosimLoaded, GetType(frmQuotaShare), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_ASSESSMENTS, "ndAssessments", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Input, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_ASSESSMENTGROUP, "ndAssessGroup", eCoreExecutionState.EcosimLoaded, GetType(frmMSEAssessGroups), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_ASSESSMENTFLEET, "ndAssessFleet", eCoreExecutionState.EcosimLoaded, GetType(frmMSEAssessFleets), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_RECRUITMENT, "ndMSERecruitment", eCoreExecutionState.EcosimLoaded, GetType(frmMSERecruitment), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_REFLEVELS, "ndRefLevels", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Input, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_REFMSY, "ndRefMSY", eCoreExecutionState.EcosimLoaded, GetType(frmMSY), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_GROUP, "ndRefBiomass", eCoreExecutionState.EcosimLoaded, GetType(frmGroupRefLevels), eNodeImages.Input, nodeFolder) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_FLEET, "ndRefCatch", eCoreExecutionState.EcosimLoaded, GetType(gridFleetRefLevels), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        nodeFolder = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_EFFORTTRACKING, "ndEffortTrackInput", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Input, nodeInput)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_INPUT_FLEETWEIGHTS, "ndEfTrackFleetWeights", eCoreExecutionState.EcosimLoaded, GetType(gridFishingWeights), eNodeImages.Input, nodeFolder) ' ToDo: connect to help

        ' output
        nodeOutput = Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_OUTPUT, "ndMSEOutput", eCoreExecutionState.EcosimLoaded, Nothing, eNodeImages.Output, nodeModel)
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_OUTPUT_RUN, "ndMSERun", eCoreExecutionState.EcosimLoaded, GetType(frmMSE), eNodeImages.Output, nodeOutput) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_OUTPUT_PLOTS, "ndMSEPlots", eCoreExecutionState.EcosimLoaded, GetType(frmMSEPlots), eNodeImages.Output, nodeOutput) ' ToDo: connect to help
        Me.m_nodecontroller.Add(My.Resources.LABEL_NAV_MSE_OUTPUT_RESULTS, "ndMSEResults", eCoreExecutionState.EcosimLoaded, GetType(frmMSEResults), eNodeImages.Output, nodeOutput) ' ToDo: connect to help

        ' -- Dead bits, to be assessed --

        ' .Add("ndControlType", eCoreExecutionState.EcosimLoaded, GetType(gridRegulatoryOptions), "") ' ToDo: connect to help
        '    .Add("ndRegFishingMort", eCoreExecutionState.EcosimLoaded, GetType(frmTargetFishingMortalityPolicy), "") ' ToDo: connect to help
        '    'jb march-8-2010 removed Group objectives and Objective weights as they are not being used by the MSE
        '    '.Add("ndEfTrackObjectives", eCoreExecutionState.EcosimLoaded, GetType(gridMSEOjectiveWeights), "") ' ToDo: connect to help
        '    '.Add("ndEfTrackEcoObjectives", eCoreExecutionState.EcosimLoaded, GetType(gridMSEGroupObjectives), "") ' ToDo: connect to help

        '    'MSE Batch
        '    'Not ready for release yet
        '    '.Add("ndRunBatch", eCoreExecutionState.EcosimLoaded, GetType(frmMSERunBatch), "") ' ToDo: connect to help
        '    '.Add("ndMSEBatchTFM", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchTFM), "") ' ToDo: connect to help
        '    '.Add("ndMSEBatchFixedF", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchFixedF), "") ' ToDo: connect to help
        '    ''jb Form not done yet
        '    ' '' .Add("ndMSEBatchTAC", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchTAC), "") ' ToDo: connect to help
        '    '.Add("ndMSEBatchParameters", eCoreExecutionState.EcosimLoaded, GetType(frmMSEBatchParameters), "") ' ToDo: connect to help

        '    .Add("ndFleetQuotas", eCoreExecutionState.EcosimLoaded, GetType(gridQuotaShare)) ' ToDo: connect to help
        '    '.Add("ndSpeciesQuotas", eCoreExecutionState.EcosimLoaded, GetType(frmTargetFishingMortalityPolicy)) ' ToDo: connect to help
        '    .Add("ndEcospaceScenario", eCoreExecutionState.EcospaceLoaded, GetType(dlgEcospaceScenario)) ' ToDo: connect to help

        ' JS 19Mar2010: now why was this necessary?
        If (Me.m_tvNavigation.SelectedNode IsNot Nothing) Then
            If cSystemUtils.IsRightToLeft Then
                Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            Else
                Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
            End If
        End If

        ' Integrate plug-ins
        If Me.m_pluginManager IsNot Nothing Then
            Me.m_ntPluginHandler = New cPluginNavTreeHandler(Me.m_tvNavigation, Me.m_pluginManager, Me.m_uic.CommandHandler)
        End If

        AddHandler Me.m_uic.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreEcecutionStateChanged

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        RemoveHandler Me.m_uic.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreEcecutionStateChanged

        Me.Icon.Dispose()

        Me.m_nodecontroller.Detach()
        Me.m_nodecontroller = Nothing

        Me.m_ntPluginHandler.Dispose()
        Me.m_ntPluginHandler = Nothing

        Me.m_pluginManager = Nothing
        Me.m_uic = Nothing

        MyBase.OnFormClosed(e)
    End Sub

    Public Overrides Function PanelType() As frmEwEDockContent.ePanelType
        Return ePanelType.SystemPanel
    End Function

#End Region ' Form overrides

#Region " Properties "

    ''' <summary>
    ''' Get or set the current selected node name in the nav structure
    ''' </summary>
    ''' <param name="bUseDefault">States whether default node may be considered.</param>
    ''' <remarks>In order to highlight the selection</remarks>
    Public Property SelectedNodeName(Optional ByVal bUseDefault As Boolean = False) As String

        Get
            If Me.m_tvNavigation.SelectedNode Is Nothing Then Return ""
            Return Me.m_tvNavigation.SelectedNode.Name
        End Get

        Set(ByVal value As String)

            Dim bSelected As Boolean = False
            Dim nd As TreeNode = Nothing

            If Me.m_tvNavigation.Nodes.Count = 0 Then Return

            If (String.IsNullOrEmpty(value) And bUseDefault) Then
                value = Me.m_nodecontroller.DefaultNodeName
            End If

            ' Try to find node to select
            If Not String.IsNullOrEmpty(value) Then
                nd = Me.FindNode(Me.m_tvNavigation.Nodes, value)
            End If

            If Not ReferenceEquals(nd, Me.m_tvNavigation.SelectedNode) Then
                Me.m_tvNavigation.SelectedNode = nd
            End If

        End Set

    End Property

    Public Sub Reset()
        For Each node As TreeNode In Me.m_tvNavigation.Nodes
            node.Collapse()
        Next
    End Sub

#End Region ' Properties

#Region " Event handlers "

    Private Sub OnCoreEcecutionStateChanged(ByVal csm As cCoreStateMonitor)
        With Me.m_tvNavigation
            If csm.HasEcopathLoaded Then
                .Visible = True
                .Dock = DockStyle.Fill
            Else
                .Visible = False
                .Dock = DockStyle.None
                .Width = 0
                .Height = 0
            End If
        End With
    End Sub

#End Region ' Event handlers

#Region " Internals "

    Protected Sub RemoveNode(strNode As String)
        Dim tn As TreeNode = Me.FindNode(m_tvNavigation.Nodes, strNode)
        If (tn IsNot Nothing) Then
            Me.m_tvNavigation.Nodes.Remove(tn)
            cLog.Write(String.Format("NavPanel: Removed BETA node '{0}' from navigation tree", strNode))
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Find a node, registered in the node controller tree, using the 
    ''' <see cref="TreeNode.Text">node text</see> as the key.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function FindNode(ByVal nodes As TreeNodeCollection, ByVal strText As String) As TreeNode

        Dim nodeFound As TreeNode = Nothing

        For Each nodeSearch As TreeNode In nodes

            ' Does either node text or name compare to requested text?
            If (String.Compare(nodeSearch.Text, strText) = 0) Or (String.Compare(nodeSearch.Name, strText) = 0) Then
                ' #Yes: got it
                nodeFound = nodeSearch
                Exit For
            End If

            ' Search all child nodes
            If nodeSearch.GetNodeCount(False) <> 0 Then
                nodeFound = FindNode(nodeSearch.Nodes, strText)
                If Not nodeFound Is Nothing Then Exit For
            End If
        Next
        Return nodeFound
    End Function

#End Region ' Internals

End Class