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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmResults
    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmResults))
        Me.chkBiomass = New System.Windows.Forms.CheckBox()
        Me.chkConsumption = New System.Windows.Forms.CheckBox()
        Me.chkBiomassInteg = New System.Windows.Forms.CheckBox()
        Me.chkPredationMortality = New System.Windows.Forms.CheckBox()
        Me.chkFishingMortality = New System.Windows.Forms.CheckBox()
        Me.btnSetPredPrey = New System.Windows.Forms.Button()
        Me.btnSetPreyPred = New System.Windows.Forms.Button()
        Me.btnSaveResults = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.chkPredationPerPredator = New System.Windows.Forms.CheckBox()
        Me.btnSetParentOnly = New System.Windows.Forms.Button()
        Me.chkFishMortFleetToPrey = New System.Windows.Forms.CheckBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.chkCatch = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkDietProportions = New System.Windows.Forms.CheckBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnSetFleetPrey = New System.Windows.Forms.Button()
        Me.chkCatchFleet = New System.Windows.Forms.CheckBox()
        Me.btnSetFleetOnly = New System.Windows.Forms.Button()
        Me.chkFleetValue = New System.Windows.Forms.CheckBox()
        Me.Panel6 = New System.Windows.Forms.Panel()
        Me.chkEffort = New System.Windows.Forms.CheckBox()
        Me.chkBasicEstimates = New System.Windows.Forms.CheckBox()
        Me.Panel7 = New System.Windows.Forms.Panel()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.chkInitFishMort = New System.Windows.Forms.CheckBox()
        Me.chkInitFishingValues = New System.Windows.Forms.CheckBox()
        Me.chkInitFishingQuantities = New System.Windows.Forms.CheckBox()
        Me.chkSearchRates = New System.Windows.Forms.CheckBox()
        Me.chkElectivity = New System.Windows.Forms.CheckBox()
        Me.chkPredOverlap = New System.Windows.Forms.CheckBox()
        Me.chkPreyOverlap = New System.Windows.Forms.CheckBox()
        Me.chkRespiration = New System.Windows.Forms.CheckBox()
        Me.chkInitConsumption = New System.Windows.Forms.CheckBox()
        Me.chkInitPredMort = New System.Windows.Forms.CheckBox()
        Me.chkMortalityCoefficients = New System.Windows.Forms.CheckBox()
        Me.chkKeyIndices = New System.Windows.Forms.CheckBox()
        Me.btnAllOptions = New System.Windows.Forms.Button()
        Me.prgSave = New System.Windows.Forms.ProgressBar()
        Me.lblPrgInfo = New System.Windows.Forms.Label()
        Me.chkYearly = New System.Windows.Forms.CheckBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.optCSV = New System.Windows.Forms.RadioButton()
        Me.optExcel = New System.Windows.Forms.RadioButton()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.chkresiduals = New System.Windows.Forms.CheckBox()
        Me.Panel8 = New System.Windows.Forms.Panel()
        Me.chkSS = New System.Windows.Forms.CheckBox()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.Panel6.SuspendLayout()
        Me.Panel7.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel5.SuspendLayout()
        Me.Panel8.SuspendLayout()
        Me.SuspendLayout()
        '
        'chkBiomass
        '
        Me.chkBiomass.AutoSize = True
        Me.chkBiomass.Location = New System.Drawing.Point(16, 32)
        Me.chkBiomass.Name = "chkBiomass"
        Me.chkBiomass.Size = New System.Drawing.Size(65, 17)
        Me.chkBiomass.TabIndex = 1
        Me.chkBiomass.Text = Global.EwEResultsExtractor.My.Resources.Resources.BIOMASS
        Me.chkBiomass.UseVisualStyleBackColor = True
        '
        'chkConsumption
        '
        Me.chkConsumption.AutoSize = True
        Me.chkConsumption.Location = New System.Drawing.Point(13, 28)
        Me.chkConsumption.Name = "chkConsumption"
        Me.chkConsumption.Size = New System.Drawing.Size(87, 17)
        Me.chkConsumption.TabIndex = 4
        Me.chkConsumption.Text = Global.EwEResultsExtractor.My.Resources.Resources.CONSUMPTION
        Me.chkConsumption.UseVisualStyleBackColor = True
        '
        'chkBiomassInteg
        '
        Me.chkBiomassInteg.AutoSize = True
        Me.chkBiomassInteg.Location = New System.Drawing.Point(16, 63)
        Me.chkBiomassInteg.Name = "chkBiomassInteg"
        Me.chkBiomassInteg.Size = New System.Drawing.Size(115, 17)
        Me.chkBiomassInteg.TabIndex = 8
        Me.chkBiomassInteg.Text = Global.EwEResultsExtractor.My.Resources.Resources.BIOMASSINTEGRATED
        Me.chkBiomassInteg.UseVisualStyleBackColor = True
        '
        'chkPredationMortality
        '
        Me.chkPredationMortality.AutoSize = True
        Me.chkPredationMortality.Location = New System.Drawing.Point(141, 63)
        Me.chkPredationMortality.Name = "chkPredationMortality"
        Me.chkPredationMortality.Size = New System.Drawing.Size(112, 17)
        Me.chkPredationMortality.TabIndex = 9
        Me.chkPredationMortality.Text = Global.EwEResultsExtractor.My.Resources.Resources.PREDATIONMORT
        Me.chkPredationMortality.UseVisualStyleBackColor = True
        '
        'chkFishingMortality
        '
        Me.chkFishingMortality.AutoSize = True
        Me.chkFishingMortality.Location = New System.Drawing.Point(16, 94)
        Me.chkFishingMortality.Name = "chkFishingMortality"
        Me.chkFishingMortality.Size = New System.Drawing.Size(100, 17)
        Me.chkFishingMortality.TabIndex = 10
        Me.chkFishingMortality.Text = Global.EwEResultsExtractor.My.Resources.Resources.FISHMORT
        Me.chkFishingMortality.UseVisualStyleBackColor = True
        '
        'btnSetPredPrey
        '
        Me.btnSetPredPrey.Enabled = False
        Me.btnSetPredPrey.Location = New System.Drawing.Point(169, 80)
        Me.btnSetPredPrey.Name = "btnSetPredPrey"
        Me.btnSetPredPrey.Size = New System.Drawing.Size(99, 27)
        Me.btnSetPredPrey.TabIndex = 35
        Me.btnSetPredPrey.Text = "Change Selection"
        Me.btnSetPredPrey.UseVisualStyleBackColor = True
        '
        'btnSetPreyPred
        '
        Me.btnSetPreyPred.Enabled = False
        Me.btnSetPreyPred.Location = New System.Drawing.Point(172, 35)
        Me.btnSetPreyPred.Name = "btnSetPreyPred"
        Me.btnSetPreyPred.Size = New System.Drawing.Size(99, 27)
        Me.btnSetPreyPred.TabIndex = 36
        Me.btnSetPreyPred.Text = Global.EwEResultsExtractor.My.Resources.Resources.CHANGE_SELECTION
        Me.btnSetPreyPred.UseVisualStyleBackColor = True
        '
        'btnSaveResults
        '
        Me.btnSaveResults.Enabled = False
        Me.btnSaveResults.Location = New System.Drawing.Point(506, 562)
        Me.btnSaveResults.Name = "btnSaveResults"
        Me.btnSaveResults.Size = New System.Drawing.Size(86, 28)
        Me.btnSaveResults.TabIndex = 4
        Me.btnSaveResults.Text = Global.EwEResultsExtractor.My.Resources.Resources.SAVE_RESULTS
        Me.btnSaveResults.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(414, 562)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(86, 28)
        Me.btnCancel.TabIndex = 3
        Me.btnCancel.Text = Global.EwEResultsExtractor.My.Resources.Resources.CANCEL
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'chkPredationPerPredator
        '
        Me.chkPredationPerPredator.AutoSize = True
        Me.chkPredationPerPredator.Location = New System.Drawing.Point(16, 20)
        Me.chkPredationPerPredator.Name = "chkPredationPerPredator"
        Me.chkPredationPerPredator.Size = New System.Drawing.Size(112, 30)
        Me.chkPredationPerPredator.TabIndex = 38
        Me.chkPredationPerPredator.Text = Global.EwEResultsExtractor.My.Resources.Resources.PREDATION_PER_PRED
        Me.chkPredationPerPredator.UseVisualStyleBackColor = True
        '
        'btnSetParentOnly
        '
        Me.btnSetParentOnly.Enabled = False
        Me.btnSetParentOnly.Location = New System.Drawing.Point(172, 88)
        Me.btnSetParentOnly.Name = "btnSetParentOnly"
        Me.btnSetParentOnly.Size = New System.Drawing.Size(99, 27)
        Me.btnSetParentOnly.TabIndex = 39
        Me.btnSetParentOnly.Text = Global.EwEResultsExtractor.My.Resources.Resources.CHANGE_SELECTION
        Me.btnSetParentOnly.UseVisualStyleBackColor = True
        '
        'chkFishMortFleetToPrey
        '
        Me.chkFishMortFleetToPrey.AutoSize = True
        Me.chkFishMortFleetToPrey.Location = New System.Drawing.Point(16, 29)
        Me.chkFishMortFleetToPrey.Name = "chkFishMortFleetToPrey"
        Me.chkFishMortFleetToPrey.Size = New System.Drawing.Size(168, 17)
        Me.chkFishMortFleetToPrey.TabIndex = 40
        Me.chkFishMortFleetToPrey.Text = Global.EwEResultsExtractor.My.Resources.Resources.FISHMORT_FLEET2PREY
        Me.chkFishMortFleetToPrey.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.Azure
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.chkCatch)
        Me.Panel1.Controls.Add(Me.chkPredationMortality)
        Me.Panel1.Controls.Add(Me.chkBiomass)
        Me.Panel1.Controls.Add(Me.btnSetParentOnly)
        Me.Panel1.Controls.Add(Me.chkBiomassInteg)
        Me.Panel1.Controls.Add(Me.chkFishingMortality)
        Me.Panel1.Location = New System.Drawing.Point(28, 85)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(283, 132)
        Me.Panel1.TabIndex = 41
        '
        'chkCatch
        '
        Me.chkCatch.AutoSize = True
        Me.chkCatch.Location = New System.Drawing.Point(141, 32)
        Me.chkCatch.Name = "chkCatch"
        Me.chkCatch.Size = New System.Drawing.Size(54, 17)
        Me.chkCatch.TabIndex = 44
        Me.chkCatch.Text = Global.EwEResultsExtractor.My.Resources.Resources.TEXT_CATCH
        Me.chkCatch.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label1.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label1.Location = New System.Drawing.Point(28, 85)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(283, 18)
        Me.Label1.TabIndex = 45
        Me.Label1.Text = "Functional groups only"
        '
        'chkDietProportions
        '
        Me.chkDietProportions.AutoSize = True
        Me.chkDietProportions.Location = New System.Drawing.Point(13, 59)
        Me.chkDietProportions.Name = "chkDietProportions"
        Me.chkDietProportions.Size = New System.Drawing.Size(101, 17)
        Me.chkDietProportions.TabIndex = 44
        Me.chkDietProportions.Text = Global.EwEResultsExtractor.My.Resources.Resources.DIET_PROPS
        Me.chkDietProportions.UseVisualStyleBackColor = True
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.Azure
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(Me.chkDietProportions)
        Me.Panel2.Controls.Add(Me.chkConsumption)
        Me.Panel2.Controls.Add(Me.btnSetPredPrey)
        Me.Panel2.Location = New System.Drawing.Point(309, 89)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(283, 124)
        Me.Panel2.TabIndex = 42
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label3.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label3.Location = New System.Drawing.Point(309, 85)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(283, 18)
        Me.Label3.TabIndex = 47
        Me.Label3.Text = "Predators with attached prey"
        '
        'Panel3
        '
        Me.Panel3.BackColor = System.Drawing.Color.Azure
        Me.Panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel3.Controls.Add(Me.btnSetPreyPred)
        Me.Panel3.Controls.Add(Me.chkPredationPerPredator)
        Me.Panel3.Location = New System.Drawing.Point(28, 223)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(283, 70)
        Me.Panel3.TabIndex = 42
        '
        'Label4
        '
        Me.Label4.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label4.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label4.Location = New System.Drawing.Point(28, 210)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(283, 18)
        Me.Label4.TabIndex = 55
        Me.Label4.Text = "Prey with attached predators"
        '
        'Panel4
        '
        Me.Panel4.BackColor = System.Drawing.Color.Azure
        Me.Panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel4.Controls.Add(Me.Label2)
        Me.Panel4.Controls.Add(Me.btnSetFleetPrey)
        Me.Panel4.Controls.Add(Me.chkCatchFleet)
        Me.Panel4.Controls.Add(Me.chkFishMortFleetToPrey)
        Me.Panel4.Location = New System.Drawing.Point(28, 292)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(283, 85)
        Me.Panel4.TabIndex = 43
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label2.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label2.Location = New System.Drawing.Point(-1, -1)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(283, 18)
        Me.Label2.TabIndex = 46
        Me.Label2.Text = "Fleets with attached prey"
        '
        'btnSetFleetPrey
        '
        Me.btnSetFleetPrey.Enabled = False
        Me.btnSetFleetPrey.Location = New System.Drawing.Point(172, 46)
        Me.btnSetFleetPrey.Name = "btnSetFleetPrey"
        Me.btnSetFleetPrey.Size = New System.Drawing.Size(99, 27)
        Me.btnSetFleetPrey.TabIndex = 45
        Me.btnSetFleetPrey.Text = Global.EwEResultsExtractor.My.Resources.Resources.CHANGE_SELECTION
        Me.btnSetFleetPrey.UseVisualStyleBackColor = True
        '
        'chkCatchFleet
        '
        Me.chkCatchFleet.AutoSize = True
        Me.chkCatchFleet.Location = New System.Drawing.Point(16, 52)
        Me.chkCatchFleet.Name = "chkCatchFleet"
        Me.chkCatchFleet.Size = New System.Drawing.Size(95, 17)
        Me.chkCatchFleet.TabIndex = 45
        Me.chkCatchFleet.Text = Global.EwEResultsExtractor.My.Resources.Resources.CATCH_PER_FLEET
        Me.chkCatchFleet.UseVisualStyleBackColor = True
        '
        'btnSetFleetOnly
        '
        Me.btnSetFleetOnly.Enabled = False
        Me.btnSetFleetOnly.Location = New System.Drawing.Point(169, 32)
        Me.btnSetFleetOnly.Name = "btnSetFleetOnly"
        Me.btnSetFleetOnly.Size = New System.Drawing.Size(99, 27)
        Me.btnSetFleetOnly.TabIndex = 45
        Me.btnSetFleetOnly.Text = Global.EwEResultsExtractor.My.Resources.Resources.CHANGE_SELECTION
        Me.btnSetFleetOnly.UseVisualStyleBackColor = True
        '
        'chkFleetValue
        '
        Me.chkFleetValue.AutoSize = True
        Me.chkFleetValue.Location = New System.Drawing.Point(13, 38)
        Me.chkFleetValue.Name = "chkFleetValue"
        Me.chkFleetValue.Size = New System.Drawing.Size(94, 17)
        Me.chkFleetValue.TabIndex = 45
        Me.chkFleetValue.Text = Global.EwEResultsExtractor.My.Resources.Resources.VALUE_PER_FLEET
        Me.chkFleetValue.UseVisualStyleBackColor = True
        '
        'Panel6
        '
        Me.Panel6.BackColor = System.Drawing.Color.Azure
        Me.Panel6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel6.Controls.Add(Me.chkEffort)
        Me.Panel6.Controls.Add(Me.btnSetFleetOnly)
        Me.Panel6.Controls.Add(Me.chkFleetValue)
        Me.Panel6.Location = New System.Drawing.Point(309, 227)
        Me.Panel6.Name = "Panel6"
        Me.Panel6.Size = New System.Drawing.Size(283, 67)
        Me.Panel6.TabIndex = 45
        '
        'chkEffort
        '
        Me.chkEffort.AutoSize = True
        Me.chkEffort.Location = New System.Drawing.Point(13, 15)
        Me.chkEffort.Name = "chkEffort"
        Me.chkEffort.Size = New System.Drawing.Size(51, 17)
        Me.chkEffort.TabIndex = 45
        Me.chkEffort.Text = Global.EwEResultsExtractor.My.Resources.Resources.EFFORT
        Me.chkEffort.UseVisualStyleBackColor = True
        '
        'chkBasicEstimates
        '
        Me.chkBasicEstimates.AutoSize = True
        Me.chkBasicEstimates.Location = New System.Drawing.Point(17, 30)
        Me.chkBasicEstimates.Name = "chkBasicEstimates"
        Me.chkBasicEstimates.Size = New System.Drawing.Size(99, 17)
        Me.chkBasicEstimates.TabIndex = 45
        Me.chkBasicEstimates.Text = Global.EwEResultsExtractor.My.Resources.Resources.BASIC_ESTIMATES
        Me.chkBasicEstimates.UseVisualStyleBackColor = True
        '
        'Panel7
        '
        Me.Panel7.BackColor = System.Drawing.Color.Azure
        Me.Panel7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel7.Controls.Add(Me.Label6)
        Me.Panel7.Controls.Add(Me.chkInitFishMort)
        Me.Panel7.Controls.Add(Me.chkInitFishingValues)
        Me.Panel7.Controls.Add(Me.chkInitFishingQuantities)
        Me.Panel7.Controls.Add(Me.chkSearchRates)
        Me.Panel7.Controls.Add(Me.chkElectivity)
        Me.Panel7.Controls.Add(Me.chkPredOverlap)
        Me.Panel7.Controls.Add(Me.chkPreyOverlap)
        Me.Panel7.Controls.Add(Me.chkRespiration)
        Me.Panel7.Controls.Add(Me.chkInitConsumption)
        Me.Panel7.Controls.Add(Me.chkInitPredMort)
        Me.Panel7.Controls.Add(Me.chkMortalityCoefficients)
        Me.Panel7.Controls.Add(Me.chkKeyIndices)
        Me.Panel7.Controls.Add(Me.chkBasicEstimates)
        Me.Panel7.Location = New System.Drawing.Point(28, 372)
        Me.Panel7.Name = "Panel7"
        Me.Panel7.Size = New System.Drawing.Size(564, 129)
        Me.Panel7.TabIndex = 45
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label6.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label6.Location = New System.Drawing.Point(-1, -1)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(568, 18)
        Me.Label6.TabIndex = 55
        Me.Label6.Text = "Indicators"
        '
        'chkInitFishMort
        '
        Me.chkInitFishMort.AutoSize = True
        Me.chkInitFishMort.Location = New System.Drawing.Point(158, 30)
        Me.chkInitFishMort.Name = "chkInitFishMort"
        Me.chkInitFishMort.Size = New System.Drawing.Size(124, 17)
        Me.chkInitFishMort.TabIndex = 57
        Me.chkInitFishMort.Text = Global.EwEResultsExtractor.My.Resources.Resources.INIT_FISH_MORT
        Me.chkInitFishMort.UseVisualStyleBackColor = True
        '
        'chkInitFishingValues
        '
        Me.chkInitFishingValues.AutoSize = True
        Me.chkInitFishingValues.Location = New System.Drawing.Point(418, 53)
        Me.chkInitFishingValues.Name = "chkInitFishingValues"
        Me.chkInitFishingValues.Size = New System.Drawing.Size(93, 17)
        Me.chkInitFishingValues.TabIndex = 56
        Me.chkInitFishingValues.Text = Global.EwEResultsExtractor.My.Resources.Resources.FISHING_VALUES
        Me.chkInitFishingValues.UseVisualStyleBackColor = True
        '
        'chkInitFishingQuantities
        '
        Me.chkInitFishingQuantities.AutoSize = True
        Me.chkInitFishingQuantities.Location = New System.Drawing.Point(418, 30)
        Me.chkInitFishingQuantities.Name = "chkInitFishingQuantities"
        Me.chkInitFishingQuantities.Size = New System.Drawing.Size(107, 17)
        Me.chkInitFishingQuantities.TabIndex = 55
        Me.chkInitFishingQuantities.Text = Global.EwEResultsExtractor.My.Resources.Resources.FISHING_QUANT
        Me.chkInitFishingQuantities.UseVisualStyleBackColor = True
        '
        'chkSearchRates
        '
        Me.chkSearchRates.AutoSize = True
        Me.chkSearchRates.Location = New System.Drawing.Point(418, 76)
        Me.chkSearchRates.Name = "chkSearchRates"
        Me.chkSearchRates.Size = New System.Drawing.Size(86, 17)
        Me.chkSearchRates.TabIndex = 54
        Me.chkSearchRates.Text = Global.EwEResultsExtractor.My.Resources.Resources.SEARCH_RATES
        Me.chkSearchRates.UseVisualStyleBackColor = True
        '
        'chkElectivity
        '
        Me.chkElectivity.AutoSize = True
        Me.chkElectivity.Location = New System.Drawing.Point(299, 76)
        Me.chkElectivity.Name = "chkElectivity"
        Me.chkElectivity.Size = New System.Drawing.Size(68, 17)
        Me.chkElectivity.TabIndex = 53
        Me.chkElectivity.Text = Global.EwEResultsExtractor.My.Resources.Resources.ELECTIVITY
        Me.chkElectivity.UseVisualStyleBackColor = True
        '
        'chkPredOverlap
        '
        Me.chkPredOverlap.AutoSize = True
        Me.chkPredOverlap.Location = New System.Drawing.Point(299, 53)
        Me.chkPredOverlap.Name = "chkPredOverlap"
        Me.chkPredOverlap.Size = New System.Drawing.Size(101, 17)
        Me.chkPredOverlap.TabIndex = 52
        Me.chkPredOverlap.Text = Global.EwEResultsExtractor.My.Resources.Resources.PRED_OVERLAP
        Me.chkPredOverlap.UseVisualStyleBackColor = True
        '
        'chkPreyOverlap
        '
        Me.chkPreyOverlap.AutoSize = True
        Me.chkPreyOverlap.Location = New System.Drawing.Point(299, 30)
        Me.chkPreyOverlap.Name = "chkPreyOverlap"
        Me.chkPreyOverlap.Size = New System.Drawing.Size(85, 17)
        Me.chkPreyOverlap.TabIndex = 51
        Me.chkPreyOverlap.Text = Global.EwEResultsExtractor.My.Resources.Resources.PREY_OVERLAP
        Me.chkPreyOverlap.UseVisualStyleBackColor = True
        '
        'chkRespiration
        '
        Me.chkRespiration.AutoSize = True
        Me.chkRespiration.Location = New System.Drawing.Point(158, 76)
        Me.chkRespiration.Name = "chkRespiration"
        Me.chkRespiration.Size = New System.Drawing.Size(79, 17)
        Me.chkRespiration.TabIndex = 50
        Me.chkRespiration.Text = Global.EwEResultsExtractor.My.Resources.Resources.RESPIRATION
        Me.chkRespiration.UseVisualStyleBackColor = True
        '
        'chkInitConsumption
        '
        Me.chkInitConsumption.AutoSize = True
        Me.chkInitConsumption.Location = New System.Drawing.Point(158, 53)
        Me.chkInitConsumption.Name = "chkInitConsumption"
        Me.chkInitConsumption.Size = New System.Drawing.Size(113, 17)
        Me.chkInitConsumption.TabIndex = 49
        Me.chkInitConsumption.Text = Global.EwEResultsExtractor.My.Resources.Resources.INIT_CONSUMPTION
        Me.chkInitConsumption.UseVisualStyleBackColor = True
        '
        'chkInitPredMort
        '
        Me.chkInitPredMort.AutoSize = True
        Me.chkInitPredMort.Location = New System.Drawing.Point(16, 102)
        Me.chkInitPredMort.Name = "chkInitPredMort"
        Me.chkInitPredMort.Size = New System.Drawing.Size(138, 17)
        Me.chkInitPredMort.TabIndex = 48
        Me.chkInitPredMort.Text = Global.EwEResultsExtractor.My.Resources.Resources.INIT_PRED_MORT
        Me.chkInitPredMort.UseVisualStyleBackColor = True
        '
        'chkMortalityCoefficients
        '
        Me.chkMortalityCoefficients.AutoSize = True
        Me.chkMortalityCoefficients.Location = New System.Drawing.Point(17, 76)
        Me.chkMortalityCoefficients.Name = "chkMortalityCoefficients"
        Me.chkMortalityCoefficients.Size = New System.Drawing.Size(122, 17)
        Me.chkMortalityCoefficients.TabIndex = 47
        Me.chkMortalityCoefficients.Text = Global.EwEResultsExtractor.My.Resources.Resources.MORT_COEFFS
        Me.chkMortalityCoefficients.UseVisualStyleBackColor = True
        '
        'chkKeyIndices
        '
        Me.chkKeyIndices.AutoSize = True
        Me.chkKeyIndices.Location = New System.Drawing.Point(17, 53)
        Me.chkKeyIndices.Name = "chkKeyIndices"
        Me.chkKeyIndices.Size = New System.Drawing.Size(80, 17)
        Me.chkKeyIndices.TabIndex = 46
        Me.chkKeyIndices.Text = Global.EwEResultsExtractor.My.Resources.Resources.KEY_INDICES
        Me.chkKeyIndices.UseVisualStyleBackColor = True
        '
        'btnAllOptions
        '
        Me.btnAllOptions.Location = New System.Drawing.Point(313, 562)
        Me.btnAllOptions.Name = "btnAllOptions"
        Me.btnAllOptions.Size = New System.Drawing.Size(86, 28)
        Me.btnAllOptions.TabIndex = 48
        Me.btnAllOptions.Text = Global.EwEResultsExtractor.My.Resources.Resources.ALL_OPTIONS
        Me.btnAllOptions.UseVisualStyleBackColor = True
        '
        'prgSave
        '
        Me.prgSave.Location = New System.Drawing.Point(313, 596)
        Me.prgSave.Name = "prgSave"
        Me.prgSave.Size = New System.Drawing.Size(279, 18)
        Me.prgSave.TabIndex = 49
        Me.prgSave.Visible = False
        '
        'lblPrgInfo
        '
        Me.lblPrgInfo.AutoSize = True
        Me.lblPrgInfo.Location = New System.Drawing.Point(459, 624)
        Me.lblPrgInfo.Name = "lblPrgInfo"
        Me.lblPrgInfo.Size = New System.Drawing.Size(133, 13)
        Me.lblPrgInfo.TabIndex = 50
        Me.lblPrgInfo.Text = "Data retrieval in progress..."
        Me.lblPrgInfo.Visible = False
        '
        'chkYearly
        '
        Me.chkYearly.AutoSize = True
        Me.chkYearly.Location = New System.Drawing.Point(537, 534)
        Me.chkYearly.Name = "chkYearly"
        Me.chkYearly.Size = New System.Drawing.Size(55, 17)
        Me.chkYearly.TabIndex = 51
        Me.chkYearly.Text = Global.EwEResultsExtractor.My.Resources.Resources.YEARLY
        Me.chkYearly.UseVisualStyleBackColor = True
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(12, 507)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(270, 107)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 52
        Me.PictureBox1.TabStop = False
        '
        'PictureBox2
        '
        Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
        Me.PictureBox2.Location = New System.Drawing.Point(2, -9)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(350, 77)
        Me.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox2.TabIndex = 54
        Me.PictureBox2.TabStop = False
        '
        'Label5
        '
        Me.Label5.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label5.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label5.Location = New System.Drawing.Point(309, 210)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(283, 18)
        Me.Label5.TabIndex = 55
        Me.Label5.Text = "Fleets only"
        '
        'Panel5
        '
        Me.Panel5.Controls.Add(Me.optCSV)
        Me.Panel5.Controls.Add(Me.optExcel)
        Me.Panel5.Location = New System.Drawing.Point(406, 530)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(125, 26)
        Me.Panel5.TabIndex = 57
        '
        'optCSV
        '
        Me.optCSV.AutoSize = True
        Me.optCSV.Location = New System.Drawing.Point(67, 3)
        Me.optCSV.Name = "optCSV"
        Me.optCSV.Size = New System.Drawing.Size(49, 17)
        Me.optCSV.TabIndex = 1
        Me.optCSV.TabStop = True
        Me.optCSV.Text = Global.EwEResultsExtractor.My.Resources.Resources.DOTCSV
        Me.optCSV.UseVisualStyleBackColor = True
        '
        'optExcel
        '
        Me.optExcel.AutoSize = True
        Me.optExcel.Location = New System.Drawing.Point(10, 3)
        Me.optExcel.Name = "optExcel"
        Me.optExcel.Size = New System.Drawing.Size(51, 17)
        Me.optExcel.TabIndex = 0
        Me.optExcel.TabStop = True
        Me.optExcel.Text = Global.EwEResultsExtractor.My.Resources.Resources.EXCEL
        Me.optExcel.UseVisualStyleBackColor = True
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.RoyalBlue
        Me.Label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label7.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label7.Location = New System.Drawing.Point(309, 292)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(283, 18)
        Me.Label7.TabIndex = 55
        Me.Label7.Text = "Fitting statistics"
        '
        'chkresiduals
        '
        Me.chkresiduals.AutoSize = True
        Me.chkresiduals.Location = New System.Drawing.Point(13, 18)
        Me.chkresiduals.Name = "chkresiduals"
        Me.chkresiduals.Size = New System.Drawing.Size(72, 17)
        Me.chkresiduals.TabIndex = 38
        Me.chkresiduals.Text = Global.EwEResultsExtractor.My.Resources.Resources.RESIDUALS
        Me.chkresiduals.UseVisualStyleBackColor = True
        '
        'Panel8
        '
        Me.Panel8.BackColor = System.Drawing.Color.Azure
        Me.Panel8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel8.Controls.Add(Me.chkSS)
        Me.Panel8.Controls.Add(Me.chkresiduals)
        Me.Panel8.Location = New System.Drawing.Point(309, 307)
        Me.Panel8.Name = "Panel8"
        Me.Panel8.Size = New System.Drawing.Size(283, 66)
        Me.Panel8.TabIndex = 59
        '
        'chkSS
        '
        Me.chkSS.AutoSize = True
        Me.chkSS.Location = New System.Drawing.Point(13, 41)
        Me.chkSS.Name = "chkSS"
        Me.chkSS.Size = New System.Drawing.Size(99, 17)
        Me.chkSS.TabIndex = 39
        Me.chkSS.Text = Global.EwEResultsExtractor.My.Resources.Resources.SUM_OF_SQUARES
        Me.chkSS.UseVisualStyleBackColor = True
        '
        'frmResults
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(881, 699)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Panel8)
        Me.Controls.Add(Me.Panel5)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.chkYearly)
        Me.Controls.Add(Me.lblPrgInfo)
        Me.Controls.Add(Me.prgSave)
        Me.Controls.Add(Me.btnAllOptions)
        Me.Controls.Add(Me.Panel7)
        Me.Controls.Add(Me.Panel6)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSaveResults)
        Me.Controls.Add(Me.Panel4)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.PictureBox2)
        Me.Controls.Add(Me.Panel3)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmResults"
        Me.Text = "Results Extractor"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.Panel4.ResumeLayout(False)
        Me.Panel4.PerformLayout()
        Me.Panel6.ResumeLayout(False)
        Me.Panel6.PerformLayout()
        Me.Panel7.ResumeLayout(False)
        Me.Panel7.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        Me.Panel8.ResumeLayout(False)
        Me.Panel8.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkBiomass As System.Windows.Forms.CheckBox
    Friend WithEvents chkConsumption As System.Windows.Forms.CheckBox
    Friend WithEvents chkBiomassInteg As System.Windows.Forms.CheckBox
    Friend WithEvents chkPredationMortality As System.Windows.Forms.CheckBox
    Friend WithEvents chkFishingMortality As System.Windows.Forms.CheckBox
    Friend WithEvents btnSetPredPrey As System.Windows.Forms.Button
    Friend WithEvents btnSetPreyPred As System.Windows.Forms.Button
    Friend WithEvents btnSaveResults As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents chkPredationPerPredator As System.Windows.Forms.CheckBox
    Friend WithEvents btnSetParentOnly As System.Windows.Forms.Button
    Friend WithEvents chkFishMortFleetToPrey As System.Windows.Forms.CheckBox
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Panel4 As System.Windows.Forms.Panel
    Friend WithEvents chkDietProportions As System.Windows.Forms.CheckBox
    Friend WithEvents chkCatch As System.Windows.Forms.CheckBox
    Friend WithEvents btnSetFleetPrey As System.Windows.Forms.Button
    Friend WithEvents chkCatchFleet As System.Windows.Forms.CheckBox
    Friend WithEvents btnSetFleetOnly As System.Windows.Forms.Button
    Friend WithEvents chkFleetValue As System.Windows.Forms.CheckBox
    Friend WithEvents Panel6 As System.Windows.Forms.Panel
    Friend WithEvents chkBasicEstimates As System.Windows.Forms.CheckBox
    Friend WithEvents Panel7 As System.Windows.Forms.Panel
    Friend WithEvents chkKeyIndices As System.Windows.Forms.CheckBox
    Friend WithEvents chkMortalityCoefficients As System.Windows.Forms.CheckBox
    Friend WithEvents chkInitPredMort As System.Windows.Forms.CheckBox
    Friend WithEvents chkInitConsumption As System.Windows.Forms.CheckBox
    Friend WithEvents chkRespiration As System.Windows.Forms.CheckBox
    Friend WithEvents chkPreyOverlap As System.Windows.Forms.CheckBox
    Friend WithEvents chkPredOverlap As System.Windows.Forms.CheckBox
    Friend WithEvents chkSearchRates As System.Windows.Forms.CheckBox
    Friend WithEvents chkElectivity As System.Windows.Forms.CheckBox
    Friend WithEvents chkInitFishingQuantities As System.Windows.Forms.CheckBox
    Friend WithEvents chkInitFishingValues As System.Windows.Forms.CheckBox
    Friend WithEvents btnAllOptions As System.Windows.Forms.Button
    Friend WithEvents prgSave As System.Windows.Forms.ProgressBar
    Friend WithEvents lblPrgInfo As System.Windows.Forms.Label
    Friend WithEvents chkYearly As System.Windows.Forms.CheckBox
    Friend WithEvents chkEffort As System.Windows.Forms.CheckBox
    Friend WithEvents chkInitFishMort As System.Windows.Forms.CheckBox
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Panel5 As System.Windows.Forms.Panel
    Friend WithEvents optCSV As System.Windows.Forms.RadioButton
    Friend WithEvents optExcel As System.Windows.Forms.RadioButton
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents chkresiduals As System.Windows.Forms.CheckBox
    Friend WithEvents Panel8 As System.Windows.Forms.Panel
    Friend WithEvents chkSS As System.Windows.Forms.CheckBox
End Class
