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

Imports ScientificInterfaceShared.Forms

Namespace Ecospace

    Partial Class frmEcospaceParameters
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
            Dim m_gbModel As System.Windows.Forms.GroupBox
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcospaceParameters))
            Me.m_rbNewStanzaModel = New System.Windows.Forms.RadioButton()
            Me.m_rbIBM = New System.Windows.Forms.RadioButton()
            Me.m_rbOldSchool = New System.Windows.Forms.RadioButton()
            Me.m_rbBaseBiomass = New System.Windows.Forms.RadioButton()
            Me.m_rbAdjustedBiomass = New System.Windows.Forms.RadioButton()
            Me.m_hdrInitialization = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrModel = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpModelTop = New System.Windows.Forms.TableLayoutPanel()
            Me.m_gbEffort = New System.Windows.Forms.GroupBox()
            Me.m_rbEcopathEffort = New System.Windows.Forms.RadioButton()
            Me.m_rbPredictEffort = New System.Windows.Forms.RadioButton()
            Me.m_gbIMB = New System.Windows.Forms.GroupBox()
            Me.m_cbMovePackets = New System.Windows.Forms.CheckBox()
            Me.m_tbNumPackets = New System.Windows.Forms.TextBox()
            Me.lbPacketsMultiplier = New System.Windows.Forms.Label()
            Me.m_lbNumThreads = New System.Windows.Forms.Label()
            Me.m_nudNumThreads = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudMaxIterations = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lbTotalTime = New System.Windows.Forms.Label()
            Me.m_lblNumTimstepsPerYear = New System.Windows.Forms.Label()
            Me.m_lbNumIterations = New System.Windows.Forms.Label()
            Me.m_lbTolerance = New System.Windows.Forms.Label()
            Me.m_lbSOR = New System.Windows.Forms.Label()
            Me.m_tbTotalTime = New System.Windows.Forms.TextBox()
            Me.m_tbNumTimeStepsPerYear = New System.Windows.Forms.TextBox()
            Me.m_tbTolerance = New System.Windows.Forms.TextBox()
            Me.m_tbSOR = New System.Windows.Forms.TextBox()
            Me.m_gbRunTime = New System.Windows.Forms.GroupBox()
            Me.m_cbAnnualOutput = New System.Windows.Forms.CheckBox()
            Me.m_clbAutosave = New System.Windows.Forms.CheckedListBox()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.m_nudFirstTimeStep = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_cbContaminantTracing = New System.Windows.Forms.CheckBox()
            Me.m_cbUseExact = New System.Windows.Forms.CheckBox()
            Me.m_tbContact = New System.Windows.Forms.TextBox()
            Me.m_tbAuthor = New System.Windows.Forms.TextBox()
            Me.m_lbContact = New System.Windows.Forms.Label()
            Me.m_lbAuthor = New System.Windows.Forms.Label()
            Me.m_tbName = New System.Windows.Forms.TextBox()
            Me.m_tbDescription = New System.Windows.Forms.TextBox()
            Me.m_lblDescription = New System.Windows.Forms.Label()
            Me.m_lbScenarioName = New System.Windows.Forms.Label()
            Me.m_hdrScenario = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plBiomass = New System.Windows.Forms.Panel()
            Me.m_tlpStuff = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plScenario = New System.Windows.Forms.Panel()
            Me.m_plSpatial = New System.Windows.Forms.Panel()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.m_pbLink = New System.Windows.Forms.PictureBox()
            Me.Label6 = New System.Windows.Forms.Label()
            Me.m_lblCellWidth = New System.Windows.Forms.Label()
            Me.m_lblNumCols = New System.Windows.Forms.Label()
            Me.m_lblCellSize = New System.Windows.Forms.Label()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_nudEast = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudSouth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudRowCount = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudCellLength = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudColCount = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudCellSize = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudWest = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudNorth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblNorth = New System.Windows.Forms.Label()
            Me.m_lblWest = New System.Windows.Forms.Label()
            Me.m_lblEast = New System.Windows.Forms.Label()
            Me.m_lblSouth = New System.Windows.Forms.Label()
            Me.m_hdrSpatial = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plModel = New System.Windows.Forms.Panel()
            Me.m_plTimeSeries = New System.Windows.Forms.Panel()
            Me.m_lblOutputResidualsFile = New System.Windows.Forms.Label()
            Me.m_tbxlOutputResidualsFile = New System.Windows.Forms.TextBox()
            Me.m_tbxXYTimeSeriesFile = New System.Windows.Forms.TextBox()
            Me.m_lblXY = New System.Windows.Forms.Label()
            Me.m_btnTimeSeriesOutputFile = New System.Windows.Forms.Button()
            Me.m_btnLoadXYTimeSeries = New System.Windows.Forms.Button()
            Me.m_cbUseEcosimDiscardForcing = New System.Windows.Forms.CheckBox()
            Me.m_cbUseEcosimBiomassForcing = New System.Windows.Forms.CheckBox()
            Me.m_hdrTimeSeries = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            m_gbModel = New System.Windows.Forms.GroupBox()
            m_gbModel.SuspendLayout()
            Me.m_tlpModelTop.SuspendLayout()
            Me.m_gbEffort.SuspendLayout()
            Me.m_gbIMB.SuspendLayout()
            CType(Me.m_nudNumThreads, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudMaxIterations, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_gbRunTime.SuspendLayout()
            CType(Me.m_nudFirstTimeStep, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_plBiomass.SuspendLayout()
            Me.m_tlpStuff.SuspendLayout()
            Me.m_plScenario.SuspendLayout()
            Me.m_plSpatial.SuspendLayout()
            CType(Me.m_pbLink, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudRowCount, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudCellLength, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudColCount, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudCellSize, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_plModel.SuspendLayout()
            Me.m_plTimeSeries.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_gbModel
            '
            m_gbModel.Controls.Add(Me.m_rbNewStanzaModel)
            m_gbModel.Controls.Add(Me.m_rbIBM)
            m_gbModel.Controls.Add(Me.m_rbOldSchool)
            resources.ApplyResources(m_gbModel, "m_gbModel")
            m_gbModel.Name = "m_gbModel"
            m_gbModel.TabStop = False
            '
            'm_rbNewStanzaModel
            '
            resources.ApplyResources(Me.m_rbNewStanzaModel, "m_rbNewStanzaModel")
            Me.m_rbNewStanzaModel.Checked = True
            Me.m_rbNewStanzaModel.Name = "m_rbNewStanzaModel"
            Me.m_rbNewStanzaModel.TabStop = True
            Me.m_rbNewStanzaModel.UseVisualStyleBackColor = True
            '
            'm_rbIBM
            '
            resources.ApplyResources(Me.m_rbIBM, "m_rbIBM")
            Me.m_rbIBM.Name = "m_rbIBM"
            Me.m_rbIBM.UseVisualStyleBackColor = True
            '
            'm_rbOldSchool
            '
            resources.ApplyResources(Me.m_rbOldSchool, "m_rbOldSchool")
            Me.m_rbOldSchool.Name = "m_rbOldSchool"
            Me.m_rbOldSchool.UseVisualStyleBackColor = True
            '
            'm_rbBaseBiomass
            '
            resources.ApplyResources(Me.m_rbBaseBiomass, "m_rbBaseBiomass")
            Me.m_rbBaseBiomass.Checked = True
            Me.m_rbBaseBiomass.Name = "m_rbBaseBiomass"
            Me.m_rbBaseBiomass.TabStop = True
            Me.m_rbBaseBiomass.UseVisualStyleBackColor = True
            '
            'm_rbAdjustedBiomass
            '
            resources.ApplyResources(Me.m_rbAdjustedBiomass, "m_rbAdjustedBiomass")
            Me.m_rbAdjustedBiomass.Name = "m_rbAdjustedBiomass"
            Me.m_rbAdjustedBiomass.UseVisualStyleBackColor = True
            '
            'm_hdrInitialization
            '
            resources.ApplyResources(Me.m_hdrInitialization, "m_hdrInitialization")
            Me.m_hdrInitialization.CanCollapseParent = True
            Me.m_hdrInitialization.CollapsedParentHeight = 0
            Me.m_hdrInitialization.IsCollapsed = False
            Me.m_hdrInitialization.Name = "m_hdrInitialization"
            '
            'm_hdrModel
            '
            resources.ApplyResources(Me.m_hdrModel, "m_hdrModel")
            Me.m_hdrModel.CanCollapseParent = True
            Me.m_hdrModel.CollapsedParentHeight = 0
            Me.m_hdrModel.IsCollapsed = False
            Me.m_hdrModel.Name = "m_hdrModel"
            '
            'm_tlpModelTop
            '
            resources.ApplyResources(Me.m_tlpModelTop, "m_tlpModelTop")
            Me.m_tlpModelTop.Controls.Add(Me.m_gbEffort, 2, 0)
            Me.m_tlpModelTop.Controls.Add(m_gbModel, 0, 0)
            Me.m_tlpModelTop.Controls.Add(Me.m_gbIMB, 1, 0)
            Me.m_tlpModelTop.Name = "m_tlpModelTop"
            '
            'm_gbEffort
            '
            Me.m_gbEffort.Controls.Add(Me.m_rbEcopathEffort)
            Me.m_gbEffort.Controls.Add(Me.m_rbPredictEffort)
            resources.ApplyResources(Me.m_gbEffort, "m_gbEffort")
            Me.m_gbEffort.Name = "m_gbEffort"
            Me.m_gbEffort.TabStop = False
            '
            'm_rbEcopathEffort
            '
            resources.ApplyResources(Me.m_rbEcopathEffort, "m_rbEcopathEffort")
            Me.m_rbEcopathEffort.Name = "m_rbEcopathEffort"
            Me.m_rbEcopathEffort.TabStop = True
            Me.m_rbEcopathEffort.UseVisualStyleBackColor = True
            '
            'm_rbPredictEffort
            '
            resources.ApplyResources(Me.m_rbPredictEffort, "m_rbPredictEffort")
            Me.m_rbPredictEffort.Name = "m_rbPredictEffort"
            Me.m_rbPredictEffort.TabStop = True
            Me.m_rbPredictEffort.UseVisualStyleBackColor = True
            '
            'm_gbIMB
            '
            Me.m_gbIMB.Controls.Add(Me.m_cbMovePackets)
            Me.m_gbIMB.Controls.Add(Me.m_tbNumPackets)
            Me.m_gbIMB.Controls.Add(Me.lbPacketsMultiplier)
            resources.ApplyResources(Me.m_gbIMB, "m_gbIMB")
            Me.m_gbIMB.Name = "m_gbIMB"
            Me.m_gbIMB.TabStop = False
            '
            'm_cbMovePackets
            '
            resources.ApplyResources(Me.m_cbMovePackets, "m_cbMovePackets")
            Me.m_cbMovePackets.Name = "m_cbMovePackets"
            Me.m_cbMovePackets.UseVisualStyleBackColor = True
            '
            'm_tbNumPackets
            '
            resources.ApplyResources(Me.m_tbNumPackets, "m_tbNumPackets")
            Me.m_tbNumPackets.Name = "m_tbNumPackets"
            '
            'lbPacketsMultiplier
            '
            resources.ApplyResources(Me.lbPacketsMultiplier, "lbPacketsMultiplier")
            Me.lbPacketsMultiplier.Name = "lbPacketsMultiplier"
            '
            'm_lbNumThreads
            '
            resources.ApplyResources(Me.m_lbNumThreads, "m_lbNumThreads")
            Me.m_lbNumThreads.Name = "m_lbNumThreads"
            '
            'm_nudNumThreads
            '
            Me.m_nudNumThreads.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudNumThreads, "m_nudNumThreads")
            Me.m_nudNumThreads.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
            Me.m_nudNumThreads.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudNumThreads.Name = "m_nudNumThreads"
            Me.m_nudNumThreads.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_nudMaxIterations
            '
            Me.m_nudMaxIterations.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudMaxIterations, "m_nudMaxIterations")
            Me.m_nudMaxIterations.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
            Me.m_nudMaxIterations.Name = "m_nudMaxIterations"
            Me.m_nudMaxIterations.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_lbTotalTime
            '
            resources.ApplyResources(Me.m_lbTotalTime, "m_lbTotalTime")
            Me.m_lbTotalTime.Name = "m_lbTotalTime"
            '
            'm_lblNumTimstepsPerYear
            '
            resources.ApplyResources(Me.m_lblNumTimstepsPerYear, "m_lblNumTimstepsPerYear")
            Me.m_lblNumTimstepsPerYear.Name = "m_lblNumTimstepsPerYear"
            '
            'm_lbNumIterations
            '
            resources.ApplyResources(Me.m_lbNumIterations, "m_lbNumIterations")
            Me.m_lbNumIterations.Name = "m_lbNumIterations"
            '
            'm_lbTolerance
            '
            resources.ApplyResources(Me.m_lbTolerance, "m_lbTolerance")
            Me.m_lbTolerance.Name = "m_lbTolerance"
            '
            'm_lbSOR
            '
            resources.ApplyResources(Me.m_lbSOR, "m_lbSOR")
            Me.m_lbSOR.Name = "m_lbSOR"
            '
            'm_tbTotalTime
            '
            resources.ApplyResources(Me.m_tbTotalTime, "m_tbTotalTime")
            Me.m_tbTotalTime.Name = "m_tbTotalTime"
            '
            'm_tbNumTimeStepsPerYear
            '
            resources.ApplyResources(Me.m_tbNumTimeStepsPerYear, "m_tbNumTimeStepsPerYear")
            Me.m_tbNumTimeStepsPerYear.Name = "m_tbNumTimeStepsPerYear"
            '
            'm_tbTolerance
            '
            resources.ApplyResources(Me.m_tbTolerance, "m_tbTolerance")
            Me.m_tbTolerance.Name = "m_tbTolerance"
            '
            'm_tbSOR
            '
            resources.ApplyResources(Me.m_tbSOR, "m_tbSOR")
            Me.m_tbSOR.Name = "m_tbSOR"
            '
            'm_gbRunTime
            '
            resources.ApplyResources(Me.m_gbRunTime, "m_gbRunTime")
            Me.m_gbRunTime.Controls.Add(Me.m_cbAnnualOutput)
            Me.m_gbRunTime.Controls.Add(Me.m_clbAutosave)
            Me.m_gbRunTime.Controls.Add(Me.Label2)
            Me.m_gbRunTime.Controls.Add(Me.m_nudFirstTimeStep)
            Me.m_gbRunTime.Controls.Add(Me.m_lbNumThreads)
            Me.m_gbRunTime.Controls.Add(Me.m_nudNumThreads)
            Me.m_gbRunTime.Controls.Add(Me.m_tbSOR)
            Me.m_gbRunTime.Controls.Add(Me.m_tbTolerance)
            Me.m_gbRunTime.Controls.Add(Me.m_tbNumTimeStepsPerYear)
            Me.m_gbRunTime.Controls.Add(Me.m_tbTotalTime)
            Me.m_gbRunTime.Controls.Add(Me.m_cbContaminantTracing)
            Me.m_gbRunTime.Controls.Add(Me.m_cbUseExact)
            Me.m_gbRunTime.Controls.Add(Me.m_lbSOR)
            Me.m_gbRunTime.Controls.Add(Me.m_lbTolerance)
            Me.m_gbRunTime.Controls.Add(Me.m_lbNumIterations)
            Me.m_gbRunTime.Controls.Add(Me.m_lblNumTimstepsPerYear)
            Me.m_gbRunTime.Controls.Add(Me.m_lbTotalTime)
            Me.m_gbRunTime.Controls.Add(Me.m_nudMaxIterations)
            Me.m_gbRunTime.Name = "m_gbRunTime"
            Me.m_gbRunTime.TabStop = False
            '
            'm_cbAnnualOutput
            '
            resources.ApplyResources(Me.m_cbAnnualOutput, "m_cbAnnualOutput")
            Me.m_cbAnnualOutput.Checked = True
            Me.m_cbAnnualOutput.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbAnnualOutput.Name = "m_cbAnnualOutput"
            Me.m_cbAnnualOutput.UseVisualStyleBackColor = True
            '
            'm_clbAutosave
            '
            resources.ApplyResources(Me.m_clbAutosave, "m_clbAutosave")
            Me.m_clbAutosave.CheckOnClick = True
            Me.m_clbAutosave.FormattingEnabled = True
            Me.m_clbAutosave.Name = "m_clbAutosave"
            Me.m_clbAutosave.Sorted = True
            '
            'Label2
            '
            resources.ApplyResources(Me.Label2, "Label2")
            Me.Label2.Name = "Label2"
            '
            'm_nudFirstTimeStep
            '
            Me.m_nudFirstTimeStep.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudFirstTimeStep, "m_nudFirstTimeStep")
            Me.m_nudFirstTimeStep.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
            Me.m_nudFirstTimeStep.Name = "m_nudFirstTimeStep"
            Me.m_nudFirstTimeStep.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_cbContaminantTracing
            '
            resources.ApplyResources(Me.m_cbContaminantTracing, "m_cbContaminantTracing")
            Me.m_cbContaminantTracing.Name = "m_cbContaminantTracing"
            Me.m_cbContaminantTracing.UseVisualStyleBackColor = True
            '
            'm_cbUseExact
            '
            resources.ApplyResources(Me.m_cbUseExact, "m_cbUseExact")
            Me.m_cbUseExact.Name = "m_cbUseExact"
            Me.m_cbUseExact.UseVisualStyleBackColor = True
            '
            'm_tbContact
            '
            resources.ApplyResources(Me.m_tbContact, "m_tbContact")
            Me.m_tbContact.Name = "m_tbContact"
            '
            'm_tbAuthor
            '
            resources.ApplyResources(Me.m_tbAuthor, "m_tbAuthor")
            Me.m_tbAuthor.Name = "m_tbAuthor"
            '
            'm_lbContact
            '
            resources.ApplyResources(Me.m_lbContact, "m_lbContact")
            Me.m_lbContact.Name = "m_lbContact"
            '
            'm_lbAuthor
            '
            resources.ApplyResources(Me.m_lbAuthor, "m_lbAuthor")
            Me.m_lbAuthor.Name = "m_lbAuthor"
            '
            'm_tbName
            '
            resources.ApplyResources(Me.m_tbName, "m_tbName")
            Me.m_tbName.Name = "m_tbName"
            '
            'm_tbDescription
            '
            resources.ApplyResources(Me.m_tbDescription, "m_tbDescription")
            Me.m_tbDescription.Name = "m_tbDescription"
            '
            'm_lblDescription
            '
            resources.ApplyResources(Me.m_lblDescription, "m_lblDescription")
            Me.m_lblDescription.Name = "m_lblDescription"
            '
            'm_lbScenarioName
            '
            resources.ApplyResources(Me.m_lbScenarioName, "m_lbScenarioName")
            Me.m_lbScenarioName.Name = "m_lbScenarioName"
            '
            'm_hdrScenario
            '
            resources.ApplyResources(Me.m_hdrScenario, "m_hdrScenario")
            Me.m_hdrScenario.CanCollapseParent = True
            Me.m_hdrScenario.CollapsedParentHeight = 106
            Me.m_hdrScenario.IsCollapsed = False
            Me.m_hdrScenario.Name = "m_hdrScenario"
            '
            'm_plBiomass
            '
            resources.ApplyResources(Me.m_plBiomass, "m_plBiomass")
            Me.m_plBiomass.Controls.Add(Me.m_rbBaseBiomass)
            Me.m_plBiomass.Controls.Add(Me.m_rbAdjustedBiomass)
            Me.m_plBiomass.Controls.Add(Me.m_hdrInitialization)
            Me.m_plBiomass.Name = "m_plBiomass"
            '
            'm_tlpStuff
            '
            resources.ApplyResources(Me.m_tlpStuff, "m_tlpStuff")
            Me.m_tlpStuff.Controls.Add(Me.m_plScenario, 0, 0)
            Me.m_tlpStuff.Controls.Add(Me.m_plBiomass, 0, 2)
            Me.m_tlpStuff.Controls.Add(Me.m_plSpatial, 0, 1)
            Me.m_tlpStuff.Controls.Add(Me.m_plModel, 0, 3)
            Me.m_tlpStuff.Controls.Add(Me.m_plTimeSeries, 0, 4)
            Me.m_tlpStuff.Name = "m_tlpStuff"
            '
            'm_plScenario
            '
            Me.m_plScenario.Controls.Add(Me.m_hdrScenario)
            Me.m_plScenario.Controls.Add(Me.m_tbContact)
            Me.m_plScenario.Controls.Add(Me.m_lbScenarioName)
            Me.m_plScenario.Controls.Add(Me.m_tbAuthor)
            Me.m_plScenario.Controls.Add(Me.m_lblDescription)
            Me.m_plScenario.Controls.Add(Me.m_lbContact)
            Me.m_plScenario.Controls.Add(Me.m_tbDescription)
            Me.m_plScenario.Controls.Add(Me.m_tbName)
            Me.m_plScenario.Controls.Add(Me.m_lbAuthor)
            resources.ApplyResources(Me.m_plScenario, "m_plScenario")
            Me.m_plScenario.Name = "m_plScenario"
            '
            'm_plSpatial
            '
            Me.m_plSpatial.Controls.Add(Me.Label4)
            Me.m_plSpatial.Controls.Add(Me.m_pbLink)
            Me.m_plSpatial.Controls.Add(Me.Label6)
            Me.m_plSpatial.Controls.Add(Me.m_lblCellWidth)
            Me.m_plSpatial.Controls.Add(Me.m_lblNumCols)
            Me.m_plSpatial.Controls.Add(Me.m_lblCellSize)
            Me.m_plSpatial.Controls.Add(Me.Label1)
            Me.m_plSpatial.Controls.Add(Me.m_nudEast)
            Me.m_plSpatial.Controls.Add(Me.m_nudSouth)
            Me.m_plSpatial.Controls.Add(Me.m_nudRowCount)
            Me.m_plSpatial.Controls.Add(Me.m_nudCellLength)
            Me.m_plSpatial.Controls.Add(Me.m_nudColCount)
            Me.m_plSpatial.Controls.Add(Me.m_nudCellSize)
            Me.m_plSpatial.Controls.Add(Me.m_nudWest)
            Me.m_plSpatial.Controls.Add(Me.m_nudNorth)
            Me.m_plSpatial.Controls.Add(Me.m_lblNorth)
            Me.m_plSpatial.Controls.Add(Me.m_lblWest)
            Me.m_plSpatial.Controls.Add(Me.m_lblEast)
            Me.m_plSpatial.Controls.Add(Me.m_lblSouth)
            Me.m_plSpatial.Controls.Add(Me.m_hdrSpatial)
            resources.ApplyResources(Me.m_plSpatial, "m_plSpatial")
            Me.m_plSpatial.Name = "m_plSpatial"
            '
            'Label4
            '
            resources.ApplyResources(Me.Label4, "Label4")
            Me.Label4.ForeColor = System.Drawing.Color.Red
            Me.Label4.Name = "Label4"
            '
            'm_pbLink
            '
            resources.ApplyResources(Me.m_pbLink, "m_pbLink")
            Me.m_pbLink.Name = "m_pbLink"
            Me.m_pbLink.TabStop = False
            '
            'Label6
            '
            resources.ApplyResources(Me.Label6, "Label6")
            Me.Label6.Name = "Label6"
            '
            'm_lblCellWidth
            '
            resources.ApplyResources(Me.m_lblCellWidth, "m_lblCellWidth")
            Me.m_lblCellWidth.Name = "m_lblCellWidth"
            '
            'm_lblNumCols
            '
            resources.ApplyResources(Me.m_lblNumCols, "m_lblNumCols")
            Me.m_lblNumCols.Name = "m_lblNumCols"
            '
            'm_lblCellSize
            '
            resources.ApplyResources(Me.m_lblCellSize, "m_lblCellSize")
            Me.m_lblCellSize.Name = "m_lblCellSize"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_nudEast
            '
            Me.m_nudEast.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudEast, "m_nudEast")
            Me.m_nudEast.Name = "m_nudEast"
            '
            'm_nudSouth
            '
            Me.m_nudSouth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudSouth, "m_nudSouth")
            Me.m_nudSouth.Name = "m_nudSouth"
            '
            'm_nudRowCount
            '
            Me.m_nudRowCount.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudRowCount, "m_nudRowCount")
            Me.m_nudRowCount.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
            Me.m_nudRowCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudRowCount.Name = "m_nudRowCount"
            Me.m_nudRowCount.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_nudCellLength
            '
            Me.m_nudCellLength.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudCellLength, "m_nudCellLength")
            Me.m_nudCellLength.Minimum = New Decimal(New Integer() {1, 0, 0, 589824})
            Me.m_nudCellLength.Name = "m_nudCellLength"
            Me.m_nudCellLength.Value = New Decimal(New Integer() {1, 0, 0, 589824})
            '
            'm_nudColCount
            '
            Me.m_nudColCount.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudColCount, "m_nudColCount")
            Me.m_nudColCount.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
            Me.m_nudColCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudColCount.Name = "m_nudColCount"
            Me.m_nudColCount.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_nudCellSize
            '
            Me.m_nudCellSize.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudCellSize, "m_nudCellSize")
            Me.m_nudCellSize.Minimum = New Decimal(New Integer() {1, 0, 0, 589824})
            Me.m_nudCellSize.Name = "m_nudCellSize"
            Me.m_nudCellSize.Value = New Decimal(New Integer() {1, 0, 0, 589824})
            '
            'm_nudWest
            '
            Me.m_nudWest.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudWest, "m_nudWest")
            Me.m_nudWest.Name = "m_nudWest"
            '
            'm_nudNorth
            '
            Me.m_nudNorth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudNorth, "m_nudNorth")
            Me.m_nudNorth.Name = "m_nudNorth"
            '
            'm_lblNorth
            '
            resources.ApplyResources(Me.m_lblNorth, "m_lblNorth")
            Me.m_lblNorth.Name = "m_lblNorth"
            '
            'm_lblWest
            '
            resources.ApplyResources(Me.m_lblWest, "m_lblWest")
            Me.m_lblWest.Name = "m_lblWest"
            '
            'm_lblEast
            '
            resources.ApplyResources(Me.m_lblEast, "m_lblEast")
            Me.m_lblEast.Name = "m_lblEast"
            '
            'm_lblSouth
            '
            resources.ApplyResources(Me.m_lblSouth, "m_lblSouth")
            Me.m_lblSouth.Name = "m_lblSouth"
            '
            'm_hdrSpatial
            '
            resources.ApplyResources(Me.m_hdrSpatial, "m_hdrSpatial")
            Me.m_hdrSpatial.CanCollapseParent = True
            Me.m_hdrSpatial.CollapsedParentHeight = 0
            Me.m_hdrSpatial.IsCollapsed = False
            Me.m_hdrSpatial.Name = "m_hdrSpatial"
            '
            'm_plModel
            '
            Me.m_plModel.Controls.Add(Me.m_hdrModel)
            Me.m_plModel.Controls.Add(Me.m_gbRunTime)
            Me.m_plModel.Controls.Add(Me.m_tlpModelTop)
            resources.ApplyResources(Me.m_plModel, "m_plModel")
            Me.m_plModel.Name = "m_plModel"
            '
            'm_plTimeSeries
            '
            Me.m_plTimeSeries.Controls.Add(Me.m_lblOutputResidualsFile)
            Me.m_plTimeSeries.Controls.Add(Me.m_tbxlOutputResidualsFile)
            Me.m_plTimeSeries.Controls.Add(Me.m_tbxXYTimeSeriesFile)
            Me.m_plTimeSeries.Controls.Add(Me.m_lblXY)
            Me.m_plTimeSeries.Controls.Add(Me.m_btnTimeSeriesOutputFile)
            Me.m_plTimeSeries.Controls.Add(Me.m_btnLoadXYTimeSeries)
            Me.m_plTimeSeries.Controls.Add(Me.m_cbUseEcosimDiscardForcing)
            Me.m_plTimeSeries.Controls.Add(Me.m_cbUseEcosimBiomassForcing)
            Me.m_plTimeSeries.Controls.Add(Me.m_hdrTimeSeries)
            resources.ApplyResources(Me.m_plTimeSeries, "m_plTimeSeries")
            Me.m_plTimeSeries.Name = "m_plTimeSeries"
            '
            'm_lblOutputResidualsFile
            '
            resources.ApplyResources(Me.m_lblOutputResidualsFile, "m_lblOutputResidualsFile")
            Me.m_lblOutputResidualsFile.Name = "m_lblOutputResidualsFile"
            '
            'm_tbxlOutputResidualsFile
            '
            resources.ApplyResources(Me.m_tbxlOutputResidualsFile, "m_tbxlOutputResidualsFile")
            Me.m_tbxlOutputResidualsFile.Name = "m_tbxlOutputResidualsFile"
            Me.m_tbxlOutputResidualsFile.ReadOnly = True
            '
            'm_tbxXYTimeSeriesFile
            '
            resources.ApplyResources(Me.m_tbxXYTimeSeriesFile, "m_tbxXYTimeSeriesFile")
            Me.m_tbxXYTimeSeriesFile.Name = "m_tbxXYTimeSeriesFile"
            Me.m_tbxXYTimeSeriesFile.ReadOnly = True
            '
            'm_lblXY
            '
            resources.ApplyResources(Me.m_lblXY, "m_lblXY")
            Me.m_lblXY.Name = "m_lblXY"
            '
            'm_btnTimeSeriesOutputFile
            '
            resources.ApplyResources(Me.m_btnTimeSeriesOutputFile, "m_btnTimeSeriesOutputFile")
            Me.m_btnTimeSeriesOutputFile.Name = "m_btnTimeSeriesOutputFile"
            Me.m_btnTimeSeriesOutputFile.UseVisualStyleBackColor = True
            '
            'm_btnLoadXYTimeSeries
            '
            resources.ApplyResources(Me.m_btnLoadXYTimeSeries, "m_btnLoadXYTimeSeries")
            Me.m_btnLoadXYTimeSeries.Name = "m_btnLoadXYTimeSeries"
            Me.m_btnLoadXYTimeSeries.UseVisualStyleBackColor = True
            '
            'm_cbUseEcosimDiscardForcing
            '
            resources.ApplyResources(Me.m_cbUseEcosimDiscardForcing, "m_cbUseEcosimDiscardForcing")
            Me.m_cbUseEcosimDiscardForcing.Name = "m_cbUseEcosimDiscardForcing"
            Me.m_cbUseEcosimDiscardForcing.UseVisualStyleBackColor = True
            '
            'm_cbUseEcosimBiomassForcing
            '
            resources.ApplyResources(Me.m_cbUseEcosimBiomassForcing, "m_cbUseEcosimBiomassForcing")
            Me.m_cbUseEcosimBiomassForcing.Name = "m_cbUseEcosimBiomassForcing"
            Me.m_cbUseEcosimBiomassForcing.UseVisualStyleBackColor = True
            '
            'm_hdrTimeSeries
            '
            resources.ApplyResources(Me.m_hdrTimeSeries, "m_hdrTimeSeries")
            Me.m_hdrTimeSeries.CanCollapseParent = False
            Me.m_hdrTimeSeries.CollapsedParentHeight = 0
            Me.m_hdrTimeSeries.IsCollapsed = False
            Me.m_hdrTimeSeries.Name = "m_hdrTimeSeries"
            '
            'frmEcospaceParameters
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_tlpStuff)
            Me.Name = "frmEcospaceParameters"
            Me.TabText = ""
            m_gbModel.ResumeLayout(False)
            m_gbModel.PerformLayout()
            Me.m_tlpModelTop.ResumeLayout(False)
            Me.m_gbEffort.ResumeLayout(False)
            Me.m_gbEffort.PerformLayout()
            Me.m_gbIMB.ResumeLayout(False)
            Me.m_gbIMB.PerformLayout()
            CType(Me.m_nudNumThreads, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudMaxIterations, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_gbRunTime.ResumeLayout(False)
            Me.m_gbRunTime.PerformLayout()
            CType(Me.m_nudFirstTimeStep, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_plBiomass.ResumeLayout(False)
            Me.m_plBiomass.PerformLayout()
            Me.m_tlpStuff.ResumeLayout(False)
            Me.m_plScenario.ResumeLayout(False)
            Me.m_plScenario.PerformLayout()
            Me.m_plSpatial.ResumeLayout(False)
            Me.m_plSpatial.PerformLayout()
            CType(Me.m_pbLink, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudRowCount, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudCellLength, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudColCount, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudCellSize, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_plModel.ResumeLayout(False)
            Me.m_plTimeSeries.ResumeLayout(False)
            Me.m_plTimeSeries.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_plBiomass As System.Windows.Forms.Panel
        Private WithEvents m_lbScenarioName As System.Windows.Forms.Label
        Private WithEvents m_hdrScenario As cEwEHeaderLabel
        Private WithEvents m_tbName As System.Windows.Forms.TextBox
        Private WithEvents m_tbDescription As System.Windows.Forms.TextBox
        Private WithEvents m_tbContact As System.Windows.Forms.TextBox
        Private WithEvents m_tbAuthor As System.Windows.Forms.TextBox
        Private WithEvents m_lbContact As System.Windows.Forms.Label
        Private WithEvents m_lbAuthor As System.Windows.Forms.Label
        Private WithEvents m_lblDescription As System.Windows.Forms.Label
        Private WithEvents m_hdrInitialization As cEwEHeaderLabel
        Private WithEvents m_rbBaseBiomass As System.Windows.Forms.RadioButton
        Private WithEvents m_rbAdjustedBiomass As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrModel As cEwEHeaderLabel
        Private WithEvents m_tlpModelTop As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_rbNewStanzaModel As System.Windows.Forms.RadioButton
        Private WithEvents m_rbIBM As System.Windows.Forms.RadioButton
        Private WithEvents m_rbOldSchool As System.Windows.Forms.RadioButton
        Private WithEvents m_gbRunTime As System.Windows.Forms.GroupBox
        Private WithEvents m_lbTotalTime As System.Windows.Forms.Label
        Private WithEvents m_tbTotalTime As System.Windows.Forms.TextBox
        Private WithEvents m_lblNumTimstepsPerYear As System.Windows.Forms.Label
        Private WithEvents m_tbNumTimeStepsPerYear As System.Windows.Forms.TextBox
        Private WithEvents m_lbNumIterations As System.Windows.Forms.Label
        Private WithEvents m_lbTolerance As System.Windows.Forms.Label
        Private WithEvents m_tbTolerance As System.Windows.Forms.TextBox
        Private WithEvents m_tbSOR As System.Windows.Forms.TextBox
        Private WithEvents m_lbSOR As System.Windows.Forms.Label
        Private WithEvents m_cbUseExact As System.Windows.Forms.CheckBox
        Private WithEvents m_cbContaminantTracing As System.Windows.Forms.CheckBox
        Private WithEvents m_nudMaxIterations As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_gbEffort As System.Windows.Forms.GroupBox
        Private WithEvents m_lbNumThreads As System.Windows.Forms.Label
        Private WithEvents m_nudNumThreads As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Friend WithEvents m_gbIMB As System.Windows.Forms.GroupBox
        Friend WithEvents m_cbMovePackets As System.Windows.Forms.CheckBox
        Private WithEvents m_tbNumPackets As System.Windows.Forms.TextBox
        Private WithEvents lbPacketsMultiplier As System.Windows.Forms.Label
        Private WithEvents m_rbEcopathEffort As System.Windows.Forms.RadioButton
        Private WithEvents m_rbPredictEffort As System.Windows.Forms.RadioButton
        Private WithEvents m_plScenario As System.Windows.Forms.Panel
        Private WithEvents m_plModel As System.Windows.Forms.Panel
        Private WithEvents m_plSpatial As System.Windows.Forms.Panel
        Friend WithEvents m_pbLink As System.Windows.Forms.PictureBox
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_nudEast As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudSouth As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudCellLength As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudCellSize As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudWest As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudNorth As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_lblNorth As System.Windows.Forms.Label
        Private WithEvents m_lblWest As System.Windows.Forms.Label
        Private WithEvents m_lblEast As System.Windows.Forms.Label
        Private WithEvents m_lblSouth As System.Windows.Forms.Label
        Private WithEvents m_hdrSpatial As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Friend WithEvents Label4 As System.Windows.Forms.Label
        Friend WithEvents Label6 As System.Windows.Forms.Label
        Private WithEvents m_lblNumCols As System.Windows.Forms.Label
        Private WithEvents m_nudRowCount As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudColCount As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_lblCellSize As System.Windows.Forms.Label
        Private WithEvents m_lblCellWidth As System.Windows.Forms.Label
        Private WithEvents m_tlpStuff As System.Windows.Forms.TableLayoutPanel
        Private WithEvents Label2 As System.Windows.Forms.Label
        Private WithEvents m_nudFirstTimeStep As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Friend WithEvents m_clbAutosave As System.Windows.Forms.CheckedListBox
        Friend WithEvents m_cbAnnualOutput As System.Windows.Forms.CheckBox
        Friend WithEvents m_plTimeSeries As Panel
        Friend WithEvents m_hdrTimeSeries As cEwEHeaderLabel
        Private WithEvents m_cbUseEcosimBiomassForcing As CheckBox
        Private WithEvents m_btnLoadXYTimeSeries As Button
        Private WithEvents m_btnTimeSeriesOutputFile As Button
        Private WithEvents m_lblOutputResidualsFile As Label
        Private WithEvents m_tbxlOutputResidualsFile As TextBox
        Private WithEvents m_tbxXYTimeSeriesFile As TextBox
        Private WithEvents m_lblXY As Label
        Private WithEvents m_cbUseEcosimDiscardForcing As CheckBox
    End Class

End Namespace
