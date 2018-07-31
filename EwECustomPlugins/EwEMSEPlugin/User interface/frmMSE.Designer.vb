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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Imports ScientificInterfaceShared.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMSE
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMSE))
        Me.m_tbxNModels2Run = New System.Windows.Forms.TextBox()
        Me.m_lblNTrials = New System.Windows.Forms.Label()
        Me.m_btnRun = New System.Windows.Forms.Button()
        Me.m_tbxNYearsProject = New System.Windows.Forms.TextBox()
        Me.m_lblNYears = New System.Windows.Forms.Label()
        Me.m_lblMassBalanceTol = New System.Windows.Forms.Label()
        Me.m_tbxTolerance = New System.Windows.Forms.TextBox()
        Me.m_btnRunCreateModels = New System.Windows.Forms.Button()
        Me.m_btnReviewTFM = New System.Windows.Forms.Button()
        Me.m_plStep2 = New System.Windows.Forms.Panel()
        Me.m_btnCreateSurvDist = New System.Windows.Forms.Button()
        Me.m_btnSampleSurvivabilities = New System.Windows.Forms.Button()
        Me.m_pbModelsCompatible = New System.Windows.Forms.PictureBox()
        Me.m_hdrStep2 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_btnStopCreateModels = New System.Windows.Forms.Button()
        Me.m_lblAvailableModels = New System.Windows.Forms.Label()
        Me.m_tbxNumAvailableModels = New System.Windows.Forms.TextBox()
        Me.m_tbxNTrials = New System.Windows.Forms.TextBox()
        Me.m_lblMaxAttempts = New System.Windows.Forms.Label()
        Me.m_lblMaxTime = New System.Windows.Forms.Label()
        Me.m_tbxMaxAttempts = New System.Windows.Forms.TextBox()
        Me.m_tbxMaxTime = New System.Windows.Forms.TextBox()
        Me.m_btnDeleteResults = New System.Windows.Forms.Button()
        Me.m_plStep4 = New System.Windows.Forms.Panel()
        Me.m_chkYearly = New System.Windows.Forms.CheckBox()
        Me.m_btnSelectStrategies = New System.Windows.Forms.Button()
        Me.m_hdrStep4 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblNModels = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.m_plStep3 = New System.Windows.Forms.Panel()
        Me.m_tlpFishingControls = New System.Windows.Forms.TableLayoutPanel()
        Me.m_rbWriteAlways = New System.Windows.Forms.CheckBox()
        Me.m_btnBiomassLimits = New System.Windows.Forms.Button()
        Me.m_btnDecreaseEffort = New System.Windows.Forms.Button()
        Me.m_btnStockAssessment = New System.Windows.Forms.Button()
        Me.m_btnSAError = New System.Windows.Forms.Button()
        Me.m_btnQuotaShares = New System.Windows.Forms.Button()
        Me.m_hdrStep3 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblAvailableStrategies = New System.Windows.Forms.Label()
        Me.m_tbxNumAvailableFishingStrategies = New System.Windows.Forms.TextBox()
        Me.m_btnEditSurvivabilities = New System.Windows.Forms.Button()
        Me.m_tbxArea = New System.Windows.Forms.TextBox()
        Me.m_lblArea = New System.Windows.Forms.Label()
        Me.m_pbCefas = New System.Windows.Forms.PictureBox()
        Me.m_lblAreaUnit = New System.Windows.Forms.Label()
        Me.m_btnEditBasicInputs = New System.Windows.Forms.Button()
        Me.m_tlpLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plStep1 = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnEditDiets = New System.Windows.Forms.Button()
        Me.m_lblPathValue = New System.Windows.Forms.Label()
        Me.m_lblDataPath = New System.Windows.Forms.Label()
        Me.m_pbPathCompatible = New System.Windows.Forms.PictureBox()
        Me.m_rbCustomPath = New System.Windows.Forms.RadioButton()
        Me.m_rbEwEDefaultPath = New System.Windows.Forms.RadioButton()
        Me.m_hdrStep1 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_btnChangePath = New System.Windows.Forms.Button()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.m_pbEII = New System.Windows.Forms.PictureBox()
        Me.m_plStep2.SuspendLayout()
        CType(Me.m_pbModelsCompatible, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_plStep4.SuspendLayout()
        Me.m_plStep3.SuspendLayout()
        Me.m_tlpFishingControls.SuspendLayout()
        CType(Me.m_pbCefas, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tlpLayout.SuspendLayout()
        Me.m_plStep1.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.m_pbPathCompatible, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel2.SuspendLayout()
        CType(Me.m_pbEII, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_tbxNModels2Run
        '
        resources.ApplyResources(Me.m_tbxNModels2Run, "m_tbxNModels2Run")
        Me.m_tbxNModels2Run.Name = "m_tbxNModels2Run"
        '
        'm_lblNTrials
        '
        resources.ApplyResources(Me.m_lblNTrials, "m_lblNTrials")
        Me.m_lblNTrials.Name = "m_lblNTrials"
        '
        'm_btnRun
        '
        resources.ApplyResources(Me.m_btnRun, "m_btnRun")
        Me.m_btnRun.Name = "m_btnRun"
        Me.m_btnRun.UseVisualStyleBackColor = True
        '
        'm_tbxNYearsProject
        '
        resources.ApplyResources(Me.m_tbxNYearsProject, "m_tbxNYearsProject")
        Me.m_tbxNYearsProject.Name = "m_tbxNYearsProject"
        '
        'm_lblNYears
        '
        resources.ApplyResources(Me.m_lblNYears, "m_lblNYears")
        Me.m_lblNYears.Name = "m_lblNYears"
        '
        'm_lblMassBalanceTol
        '
        resources.ApplyResources(Me.m_lblMassBalanceTol, "m_lblMassBalanceTol")
        Me.m_lblMassBalanceTol.Name = "m_lblMassBalanceTol"
        '
        'm_tbxTolerance
        '
        resources.ApplyResources(Me.m_tbxTolerance, "m_tbxTolerance")
        Me.m_tbxTolerance.Name = "m_tbxTolerance"
        '
        'm_btnRunCreateModels
        '
        resources.ApplyResources(Me.m_btnRunCreateModels, "m_btnRunCreateModels")
        Me.m_btnRunCreateModels.Name = "m_btnRunCreateModels"
        Me.m_btnRunCreateModels.UseVisualStyleBackColor = True
        '
        'm_btnReviewTFM
        '
        resources.ApplyResources(Me.m_btnReviewTFM, "m_btnReviewTFM")
        Me.m_btnReviewTFM.Name = "m_btnReviewTFM"
        Me.m_btnReviewTFM.UseVisualStyleBackColor = True
        '
        'm_plStep2
        '
        Me.m_plStep2.Controls.Add(Me.m_btnCreateSurvDist)
        Me.m_plStep2.Controls.Add(Me.m_btnSampleSurvivabilities)
        Me.m_plStep2.Controls.Add(Me.m_pbModelsCompatible)
        Me.m_plStep2.Controls.Add(Me.m_lblMassBalanceTol)
        Me.m_plStep2.Controls.Add(Me.m_tbxTolerance)
        Me.m_plStep2.Controls.Add(Me.m_hdrStep2)
        Me.m_plStep2.Controls.Add(Me.m_btnStopCreateModels)
        Me.m_plStep2.Controls.Add(Me.m_btnRunCreateModels)
        Me.m_plStep2.Controls.Add(Me.m_lblAvailableModels)
        Me.m_plStep2.Controls.Add(Me.m_lblNTrials)
        Me.m_plStep2.Controls.Add(Me.m_tbxNumAvailableModels)
        Me.m_plStep2.Controls.Add(Me.m_tbxNTrials)
        Me.m_plStep2.Controls.Add(Me.m_lblMaxAttempts)
        Me.m_plStep2.Controls.Add(Me.m_lblMaxTime)
        Me.m_plStep2.Controls.Add(Me.m_tbxMaxAttempts)
        Me.m_plStep2.Controls.Add(Me.m_tbxMaxTime)
        resources.ApplyResources(Me.m_plStep2, "m_plStep2")
        Me.m_plStep2.Name = "m_plStep2"
        '
        'm_btnCreateSurvDist
        '
        resources.ApplyResources(Me.m_btnCreateSurvDist, "m_btnCreateSurvDist")
        Me.m_btnCreateSurvDist.Name = "m_btnCreateSurvDist"
        Me.m_btnCreateSurvDist.UseVisualStyleBackColor = True
        '
        'm_btnSampleSurvivabilities
        '
        resources.ApplyResources(Me.m_btnSampleSurvivabilities, "m_btnSampleSurvivabilities")
        Me.m_btnSampleSurvivabilities.Name = "m_btnSampleSurvivabilities"
        Me.m_btnSampleSurvivabilities.UseVisualStyleBackColor = True
        '
        'm_pbModelsCompatible
        '
        resources.ApplyResources(Me.m_pbModelsCompatible, "m_pbModelsCompatible")
        Me.m_pbModelsCompatible.Name = "m_pbModelsCompatible"
        Me.m_pbModelsCompatible.TabStop = False
        '
        'm_hdrStep2
        '
        Me.m_hdrStep2.CanCollapseParent = True
        Me.m_hdrStep2.CollapsedParentHeight = 71
        resources.ApplyResources(Me.m_hdrStep2, "m_hdrStep2")
        Me.m_hdrStep2.IsCollapsed = False
        Me.m_hdrStep2.Name = "m_hdrStep2"
        '
        'm_btnStopCreateModels
        '
        resources.ApplyResources(Me.m_btnStopCreateModels, "m_btnStopCreateModels")
        Me.m_btnStopCreateModels.Name = "m_btnStopCreateModels"
        Me.m_btnStopCreateModels.UseVisualStyleBackColor = True
        '
        'm_lblAvailableModels
        '
        resources.ApplyResources(Me.m_lblAvailableModels, "m_lblAvailableModels")
        Me.m_lblAvailableModels.Name = "m_lblAvailableModels"
        '
        'm_tbxNumAvailableModels
        '
        resources.ApplyResources(Me.m_tbxNumAvailableModels, "m_tbxNumAvailableModels")
        Me.m_tbxNumAvailableModels.Name = "m_tbxNumAvailableModels"
        Me.m_tbxNumAvailableModels.ReadOnly = True
        '
        'm_tbxNTrials
        '
        resources.ApplyResources(Me.m_tbxNTrials, "m_tbxNTrials")
        Me.m_tbxNTrials.Name = "m_tbxNTrials"
        '
        'm_lblMaxAttempts
        '
        resources.ApplyResources(Me.m_lblMaxAttempts, "m_lblMaxAttempts")
        Me.m_lblMaxAttempts.Name = "m_lblMaxAttempts"
        '
        'm_lblMaxTime
        '
        resources.ApplyResources(Me.m_lblMaxTime, "m_lblMaxTime")
        Me.m_lblMaxTime.Name = "m_lblMaxTime"
        '
        'm_tbxMaxAttempts
        '
        resources.ApplyResources(Me.m_tbxMaxAttempts, "m_tbxMaxAttempts")
        Me.m_tbxMaxAttempts.Name = "m_tbxMaxAttempts"
        '
        'm_tbxMaxTime
        '
        resources.ApplyResources(Me.m_tbxMaxTime, "m_tbxMaxTime")
        Me.m_tbxMaxTime.Name = "m_tbxMaxTime"
        '
        'm_btnDeleteResults
        '
        resources.ApplyResources(Me.m_btnDeleteResults, "m_btnDeleteResults")
        Me.m_btnDeleteResults.Name = "m_btnDeleteResults"
        Me.m_btnDeleteResults.UseVisualStyleBackColor = True
        '
        'm_plStep4
        '
        Me.m_plStep4.Controls.Add(Me.m_chkYearly)
        Me.m_plStep4.Controls.Add(Me.m_btnSelectStrategies)
        Me.m_plStep4.Controls.Add(Me.m_btnDeleteResults)
        Me.m_plStep4.Controls.Add(Me.m_hdrStep4)
        Me.m_plStep4.Controls.Add(Me.m_tbxNModels2Run)
        Me.m_plStep4.Controls.Add(Me.m_lblNYears)
        Me.m_plStep4.Controls.Add(Me.m_lblNModels)
        Me.m_plStep4.Controls.Add(Me.Button1)
        Me.m_plStep4.Controls.Add(Me.m_btnRun)
        Me.m_plStep4.Controls.Add(Me.m_tbxNYearsProject)
        resources.ApplyResources(Me.m_plStep4, "m_plStep4")
        Me.m_plStep4.Name = "m_plStep4"
        '
        'm_chkYearly
        '
        resources.ApplyResources(Me.m_chkYearly, "m_chkYearly")
        Me.m_chkYearly.Name = "m_chkYearly"
        Me.m_chkYearly.UseVisualStyleBackColor = True
        '
        'm_btnSelectStrategies
        '
        resources.ApplyResources(Me.m_btnSelectStrategies, "m_btnSelectStrategies")
        Me.m_btnSelectStrategies.Name = "m_btnSelectStrategies"
        Me.m_btnSelectStrategies.UseVisualStyleBackColor = True
        '
        'm_hdrStep4
        '
        Me.m_hdrStep4.CanCollapseParent = False
        Me.m_hdrStep4.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrStep4, "m_hdrStep4")
        Me.m_hdrStep4.IsCollapsed = False
        Me.m_hdrStep4.Name = "m_hdrStep4"
        '
        'm_lblNModels
        '
        resources.ApplyResources(Me.m_lblNModels, "m_lblNModels")
        Me.m_lblNModels.Name = "m_lblNModels"
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'm_plStep3
        '
        Me.m_plStep3.Controls.Add(Me.m_tlpFishingControls)
        Me.m_plStep3.Controls.Add(Me.m_hdrStep3)
        Me.m_plStep3.Controls.Add(Me.m_btnReviewTFM)
        Me.m_plStep3.Controls.Add(Me.m_lblAvailableStrategies)
        Me.m_plStep3.Controls.Add(Me.m_tbxNumAvailableFishingStrategies)
        resources.ApplyResources(Me.m_plStep3, "m_plStep3")
        Me.m_plStep3.Name = "m_plStep3"
        '
        'm_tlpFishingControls
        '
        resources.ApplyResources(Me.m_tlpFishingControls, "m_tlpFishingControls")
        Me.m_tlpFishingControls.Controls.Add(Me.m_rbWriteAlways, 0, 2)
        Me.m_tlpFishingControls.Controls.Add(Me.m_btnBiomassLimits, 8, 0)
        Me.m_tlpFishingControls.Controls.Add(Me.m_btnDecreaseEffort, 0, 0)
        Me.m_tlpFishingControls.Controls.Add(Me.m_btnStockAssessment, 2, 0)
        Me.m_tlpFishingControls.Controls.Add(Me.m_btnSAError, 4, 0)
        Me.m_tlpFishingControls.Controls.Add(Me.m_btnQuotaShares, 6, 0)
        Me.m_tlpFishingControls.Name = "m_tlpFishingControls"
        '
        'm_rbWriteAlways
        '
        resources.ApplyResources(Me.m_rbWriteAlways, "m_rbWriteAlways")
        Me.m_rbWriteAlways.Name = "m_rbWriteAlways"
        Me.m_rbWriteAlways.UseVisualStyleBackColor = True
        '
        'm_btnBiomassLimits
        '
        resources.ApplyResources(Me.m_btnBiomassLimits, "m_btnBiomassLimits")
        Me.m_btnBiomassLimits.Name = "m_btnBiomassLimits"
        Me.m_btnBiomassLimits.UseVisualStyleBackColor = True
        '
        'm_btnDecreaseEffort
        '
        resources.ApplyResources(Me.m_btnDecreaseEffort, "m_btnDecreaseEffort")
        Me.m_btnDecreaseEffort.Name = "m_btnDecreaseEffort"
        Me.m_btnDecreaseEffort.UseVisualStyleBackColor = True
        '
        'm_btnStockAssessment
        '
        resources.ApplyResources(Me.m_btnStockAssessment, "m_btnStockAssessment")
        Me.m_btnStockAssessment.Name = "m_btnStockAssessment"
        Me.m_btnStockAssessment.UseVisualStyleBackColor = True
        '
        'm_btnSAError
        '
        resources.ApplyResources(Me.m_btnSAError, "m_btnSAError")
        Me.m_btnSAError.Name = "m_btnSAError"
        Me.m_btnSAError.UseVisualStyleBackColor = True
        '
        'm_btnQuotaShares
        '
        resources.ApplyResources(Me.m_btnQuotaShares, "m_btnQuotaShares")
        Me.m_btnQuotaShares.Name = "m_btnQuotaShares"
        Me.m_btnQuotaShares.UseVisualStyleBackColor = True
        '
        'm_hdrStep3
        '
        Me.m_hdrStep3.CanCollapseParent = False
        Me.m_hdrStep3.CollapsedParentHeight = 76
        resources.ApplyResources(Me.m_hdrStep3, "m_hdrStep3")
        Me.m_hdrStep3.IsCollapsed = False
        Me.m_hdrStep3.Name = "m_hdrStep3"
        '
        'm_lblAvailableStrategies
        '
        resources.ApplyResources(Me.m_lblAvailableStrategies, "m_lblAvailableStrategies")
        Me.m_lblAvailableStrategies.Name = "m_lblAvailableStrategies"
        '
        'm_tbxNumAvailableFishingStrategies
        '
        resources.ApplyResources(Me.m_tbxNumAvailableFishingStrategies, "m_tbxNumAvailableFishingStrategies")
        Me.m_tbxNumAvailableFishingStrategies.Name = "m_tbxNumAvailableFishingStrategies"
        Me.m_tbxNumAvailableFishingStrategies.ReadOnly = True
        '
        'm_btnEditSurvivabilities
        '
        resources.ApplyResources(Me.m_btnEditSurvivabilities, "m_btnEditSurvivabilities")
        Me.m_btnEditSurvivabilities.Name = "m_btnEditSurvivabilities"
        Me.m_btnEditSurvivabilities.UseVisualStyleBackColor = True
        '
        'm_tbxArea
        '
        resources.ApplyResources(Me.m_tbxArea, "m_tbxArea")
        Me.m_tbxArea.Name = "m_tbxArea"
        '
        'm_lblArea
        '
        resources.ApplyResources(Me.m_lblArea, "m_lblArea")
        Me.m_lblArea.Name = "m_lblArea"
        '
        'm_pbCefas
        '
        Me.m_pbCefas.BackColor = System.Drawing.Color.White
        Me.m_pbCefas.BackgroundImage = Global.EwEMSEPlugin.My.Resources.Resources.Cefas_logo
        resources.ApplyResources(Me.m_pbCefas, "m_pbCefas")
        Me.m_pbCefas.Name = "m_pbCefas"
        Me.m_pbCefas.TabStop = False
        '
        'm_lblAreaUnit
        '
        resources.ApplyResources(Me.m_lblAreaUnit, "m_lblAreaUnit")
        Me.m_lblAreaUnit.Name = "m_lblAreaUnit"
        '
        'm_btnEditBasicInputs
        '
        resources.ApplyResources(Me.m_btnEditBasicInputs, "m_btnEditBasicInputs")
        Me.m_btnEditBasicInputs.Name = "m_btnEditBasicInputs"
        Me.m_btnEditBasicInputs.UseVisualStyleBackColor = True
        '
        'm_tlpLayout
        '
        resources.ApplyResources(Me.m_tlpLayout, "m_tlpLayout")
        Me.m_tlpLayout.Controls.Add(Me.m_plStep2, 0, 1)
        Me.m_tlpLayout.Controls.Add(Me.m_plStep3, 0, 2)
        Me.m_tlpLayout.Controls.Add(Me.m_plStep4, 0, 3)
        Me.m_tlpLayout.Controls.Add(Me.m_plStep1, 0, 0)
        Me.m_tlpLayout.Controls.Add(Me.TableLayoutPanel2, 0, 4)
        Me.m_tlpLayout.Name = "m_tlpLayout"
        '
        'm_plStep1
        '
        Me.m_plStep1.Controls.Add(Me.TableLayoutPanel1)
        Me.m_plStep1.Controls.Add(Me.m_lblPathValue)
        Me.m_plStep1.Controls.Add(Me.m_lblDataPath)
        Me.m_plStep1.Controls.Add(Me.m_pbPathCompatible)
        Me.m_plStep1.Controls.Add(Me.m_lblAreaUnit)
        Me.m_plStep1.Controls.Add(Me.m_rbCustomPath)
        Me.m_plStep1.Controls.Add(Me.m_lblArea)
        Me.m_plStep1.Controls.Add(Me.m_tbxArea)
        Me.m_plStep1.Controls.Add(Me.m_rbEwEDefaultPath)
        Me.m_plStep1.Controls.Add(Me.m_hdrStep1)
        Me.m_plStep1.Controls.Add(Me.m_btnChangePath)
        resources.ApplyResources(Me.m_plStep1, "m_plStep1")
        Me.m_plStep1.Name = "m_plStep1"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.m_btnEditDiets, 4, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.m_btnEditSurvivabilities, 2, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.m_btnEditBasicInputs, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'm_btnEditDiets
        '
        resources.ApplyResources(Me.m_btnEditDiets, "m_btnEditDiets")
        Me.m_btnEditDiets.Name = "m_btnEditDiets"
        Me.m_btnEditDiets.UseVisualStyleBackColor = True
        '
        'm_lblPathValue
        '
        resources.ApplyResources(Me.m_lblPathValue, "m_lblPathValue")
        Me.m_lblPathValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.m_lblPathValue.Cursor = System.Windows.Forms.Cursors.Hand
        Me.m_lblPathValue.Name = "m_lblPathValue"
        '
        'm_lblDataPath
        '
        resources.ApplyResources(Me.m_lblDataPath, "m_lblDataPath")
        Me.m_lblDataPath.Name = "m_lblDataPath"
        '
        'm_pbPathCompatible
        '
        resources.ApplyResources(Me.m_pbPathCompatible, "m_pbPathCompatible")
        Me.m_pbPathCompatible.Name = "m_pbPathCompatible"
        Me.m_pbPathCompatible.TabStop = False
        '
        'm_rbCustomPath
        '
        resources.ApplyResources(Me.m_rbCustomPath, "m_rbCustomPath")
        Me.m_rbCustomPath.Name = "m_rbCustomPath"
        Me.m_rbCustomPath.TabStop = True
        Me.m_rbCustomPath.UseVisualStyleBackColor = True
        '
        'm_rbEwEDefaultPath
        '
        resources.ApplyResources(Me.m_rbEwEDefaultPath, "m_rbEwEDefaultPath")
        Me.m_rbEwEDefaultPath.Name = "m_rbEwEDefaultPath"
        Me.m_rbEwEDefaultPath.TabStop = True
        Me.m_rbEwEDefaultPath.UseVisualStyleBackColor = True
        '
        'm_hdrStep1
        '
        Me.m_hdrStep1.CanCollapseParent = False
        Me.m_hdrStep1.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrStep1, "m_hdrStep1")
        Me.m_hdrStep1.IsCollapsed = False
        Me.m_hdrStep1.Name = "m_hdrStep1"
        '
        'm_btnChangePath
        '
        resources.ApplyResources(Me.m_btnChangePath, "m_btnChangePath")
        Me.m_btnChangePath.Name = "m_btnChangePath"
        Me.m_btnChangePath.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.m_pbCefas, 1, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.m_pbEII, 3, 0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'm_pbEII
        '
        Me.m_pbEII.BackgroundImage = Global.EwEMSEPlugin.My.Resources.Resources.EII
        resources.ApplyResources(Me.m_pbEII, "m_pbEII")
        Me.m_pbEII.Name = "m_pbEII"
        Me.m_pbEII.TabStop = False
        '
        'frmMSE
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ControlBox = False
        Me.Controls.Add(Me.m_tlpLayout)
        Me.Name = "frmMSE"
        Me.TabText = ""
        Me.m_plStep2.ResumeLayout(False)
        Me.m_plStep2.PerformLayout()
        CType(Me.m_pbModelsCompatible, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_plStep4.ResumeLayout(False)
        Me.m_plStep4.PerformLayout()
        Me.m_plStep3.ResumeLayout(False)
        Me.m_plStep3.PerformLayout()
        Me.m_tlpFishingControls.ResumeLayout(False)
        Me.m_tlpFishingControls.PerformLayout()
        CType(Me.m_pbCefas, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tlpLayout.ResumeLayout(False)
        Me.m_plStep1.ResumeLayout(False)
        Me.m_plStep1.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.m_pbPathCompatible, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel2.ResumeLayout(False)
        CType(Me.m_pbEII, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_tbxNModels2Run As System.Windows.Forms.TextBox
    Private WithEvents m_lblNTrials As System.Windows.Forms.Label
    Private WithEvents m_btnRun As System.Windows.Forms.Button
    Private WithEvents m_tbxNYearsProject As System.Windows.Forms.TextBox
    Private WithEvents m_lblNYears As System.Windows.Forms.Label
    Private WithEvents m_tbxTolerance As System.Windows.Forms.TextBox
    Private WithEvents m_btnRunCreateModels As System.Windows.Forms.Button
    Private WithEvents m_lblMassBalanceTol As System.Windows.Forms.Label
    Private WithEvents m_btnReviewTFM As System.Windows.Forms.Button
    Private WithEvents m_plStep2 As System.Windows.Forms.Panel
    Private WithEvents m_plStep4 As System.Windows.Forms.Panel
    Private WithEvents m_lblNModels As System.Windows.Forms.Label
    Private WithEvents m_plStep3 As System.Windows.Forms.Panel
    Private WithEvents m_pbCefas As System.Windows.Forms.PictureBox
    Private WithEvents m_lblArea As System.Windows.Forms.Label
    Private WithEvents m_btnEditBasicInputs As System.Windows.Forms.Button
    Private WithEvents m_tbxArea As System.Windows.Forms.TextBox
    Private WithEvents m_tbxNTrials As System.Windows.Forms.TextBox
    Private WithEvents m_hdrStep2 As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrStep3 As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tlpLayout As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_hdrStep4 As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_plStep1 As System.Windows.Forms.Panel
    Private WithEvents m_hdrStep1 As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_rbCustomPath As System.Windows.Forms.RadioButton
    Private WithEvents m_rbEwEDefaultPath As System.Windows.Forms.RadioButton
    Private WithEvents m_btnChangePath As System.Windows.Forms.Button
    Friend WithEvents m_lblDataPath As System.Windows.Forms.Label
    Private WithEvents m_lblAvailableModels As System.Windows.Forms.Label
    Private WithEvents m_tbxNumAvailableModels As System.Windows.Forms.TextBox
    Private WithEvents m_lblAvailableStrategies As System.Windows.Forms.Label
    Private WithEvents m_tbxNumAvailableFishingStrategies As System.Windows.Forms.TextBox
    Private WithEvents m_pbModelsCompatible As System.Windows.Forms.PictureBox
    Private WithEvents m_lblMaxAttempts As System.Windows.Forms.Label
    Private WithEvents m_tbxMaxAttempts As System.Windows.Forms.TextBox
    Private WithEvents m_lblAreaUnit As System.Windows.Forms.Label
    Private WithEvents m_lblMaxTime As System.Windows.Forms.Label
    Private WithEvents m_tbxMaxTime As System.Windows.Forms.TextBox
    Private WithEvents m_lblPathValue As System.Windows.Forms.Label
    Private WithEvents m_btnDecreaseEffort As System.Windows.Forms.Button
    Private WithEvents m_btnEditSurvivabilities As System.Windows.Forms.Button
    Private WithEvents m_tlpFishingControls As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_btnDeleteResults As System.Windows.Forms.Button
    Private WithEvents m_pbPathCompatible As System.Windows.Forms.PictureBox
    Private WithEvents m_btnCreateSurvDist As System.Windows.Forms.Button
    Private WithEvents m_btnSampleSurvivabilities As System.Windows.Forms.Button
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_btnEditDiets As System.Windows.Forms.Button
    Private WithEvents m_btnStopCreateModels As System.Windows.Forms.Button
    Private WithEvents Button1 As System.Windows.Forms.Button
    Private WithEvents m_btnStockAssessment As System.Windows.Forms.Button
    Private WithEvents m_btnSAError As System.Windows.Forms.Button
    Private WithEvents m_btnQuotaShares As System.Windows.Forms.Button
    Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_pbEII As System.Windows.Forms.PictureBox
    Private WithEvents m_rbWriteAlways As System.Windows.Forms.CheckBox
    Private WithEvents m_btnBiomassLimits As Button
    Friend WithEvents m_btnSelectStrategies As Button
    Friend WithEvents m_chkYearly As CheckBox
End Class
