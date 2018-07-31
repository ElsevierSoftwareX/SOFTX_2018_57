Imports ScientificInterfaceShared.Forms

Partial Class frmRun
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmRun))
        Me.m_lblModel = New System.Windows.Forms.Label()
        Me.m_tbxModel = New System.Windows.Forms.TextBox()
        Me.m_btnSelectModel = New System.Windows.Forms.Button()
        Me.m_lblScenario = New System.Windows.Forms.Label()
        Me.m_cmbScenario = New System.Windows.Forms.ComboBox()
        Me.m_cmbTimeSeries = New System.Windows.Forms.ComboBox()
        Me.m_lblTimeSeries = New System.Windows.Forms.Label()
        Me.m_hdrIterations = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_btnSelectAll = New System.Windows.Forms.Button()
        Me.m_btnSelectNone = New System.Windows.Forms.Button()
        Me.m_btnRun = New System.Windows.Forms.Button()
        Me.m_btnApply = New System.Windows.Forms.Button()
        Me.m_lblStepSize = New System.Windows.Forms.Label()
        Me.m_nudStepSize = New System.Windows.Forms.NumericUpDown()
        Me.m_lblSearchBy = New System.Windows.Forms.Label()
        Me.m_rbPredator = New System.Windows.Forms.RadioButton()
        Me.m_rbPredPrey = New System.Windows.Forms.RadioButton()
        Me.m_hdrParameters = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrOutput = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblOutputFolder = New System.Windows.Forms.Label()
        Me.m_tbxOutputFolder = New System.Windows.Forms.TextBox()
        Me.m_btnChooseFolder = New System.Windows.Forms.Button()
        Me.m_tlpContent = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plModel = New System.Windows.Forms.Panel()
        Me.m_hdrEwE = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_plConfig = New System.Windows.Forms.Panel()
        Me.m_btnSelectFishing = New System.Windows.Forms.Button()
        Me.m_btnSelectBaseline = New System.Windows.Forms.Button()
        Me.m_btnSelectVandA = New System.Windows.Forms.Button()
        Me.m_btnSelectA = New System.Windows.Forms.Button()
        Me.m_btnSelectV = New System.Windows.Forms.Button()
        Me.m_grid = New EwEStepwiseFittingPlugin.gridRun()
        Me.m_plRun = New System.Windows.Forms.Panel()
        Me.m_cmbAutoSave = New System.Windows.Forms.ComboBox()
        Me.m_lblAutoSave = New System.Windows.Forms.Label()
        Me.m_plSettings = New System.Windows.Forms.Panel()
        Me.m_cmbAnomalyShape = New System.Windows.Forms.ComboBox()
        Me.m_lblAnomalyShape = New System.Windows.Forms.Label()
        Me.m_cbEnableAbsBioforBaseline = New System.Windows.Forms.CheckBox()
        Me.m_btnTS = New System.Windows.Forms.Button()
        Me.m_nudK = New System.Windows.Forms.NumericUpDown()
        Me.m_lblNumVars = New System.Windows.Forms.Label()
        Me.m_btnClearAll = New System.Windows.Forms.Button()
        CType(Me.m_nudStepSize, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tlpContent.SuspendLayout()
        Me.m_plModel.SuspendLayout()
        Me.m_plConfig.SuspendLayout()
        Me.m_plRun.SuspendLayout()
        Me.m_plSettings.SuspendLayout()
        CType(Me.m_nudK, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_lblModel
        '
        resources.ApplyResources(Me.m_lblModel, "m_lblModel")
        Me.m_lblModel.Name = "m_lblModel"
        '
        'm_tbxModel
        '
        resources.ApplyResources(Me.m_tbxModel, "m_tbxModel")
        Me.m_tbxModel.Name = "m_tbxModel"
        Me.m_tbxModel.ReadOnly = True
        '
        'm_btnSelectModel
        '
        resources.ApplyResources(Me.m_btnSelectModel, "m_btnSelectModel")
        Me.m_btnSelectModel.Name = "m_btnSelectModel"
        Me.m_btnSelectModel.UseVisualStyleBackColor = True
        '
        'm_lblScenario
        '
        resources.ApplyResources(Me.m_lblScenario, "m_lblScenario")
        Me.m_lblScenario.Name = "m_lblScenario"
        '
        'm_cmbScenario
        '
        resources.ApplyResources(Me.m_cmbScenario, "m_cmbScenario")
        Me.m_cmbScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbScenario.FormattingEnabled = True
        Me.m_cmbScenario.Name = "m_cmbScenario"
        '
        'm_cmbTimeSeries
        '
        resources.ApplyResources(Me.m_cmbTimeSeries, "m_cmbTimeSeries")
        Me.m_cmbTimeSeries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbTimeSeries.FormattingEnabled = True
        Me.m_cmbTimeSeries.Name = "m_cmbTimeSeries"
        '
        'm_lblTimeSeries
        '
        resources.ApplyResources(Me.m_lblTimeSeries, "m_lblTimeSeries")
        Me.m_lblTimeSeries.Name = "m_lblTimeSeries"
        '
        'm_hdrIterations
        '
        Me.m_hdrIterations.CanCollapseParent = False
        Me.m_hdrIterations.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrIterations, "m_hdrIterations")
        Me.m_hdrIterations.IsCollapsed = False
        Me.m_hdrIterations.Name = "m_hdrIterations"
        '
        'm_btnSelectAll
        '
        resources.ApplyResources(Me.m_btnSelectAll, "m_btnSelectAll")
        Me.m_btnSelectAll.Name = "m_btnSelectAll"
        Me.m_btnSelectAll.UseVisualStyleBackColor = True
        '
        'm_btnSelectNone
        '
        resources.ApplyResources(Me.m_btnSelectNone, "m_btnSelectNone")
        Me.m_btnSelectNone.Name = "m_btnSelectNone"
        Me.m_btnSelectNone.UseVisualStyleBackColor = True
        '
        'm_btnRun
        '
        resources.ApplyResources(Me.m_btnRun, "m_btnRun")
        Me.m_btnRun.Name = "m_btnRun"
        Me.m_btnRun.UseVisualStyleBackColor = True
        '
        'm_btnApply
        '
        resources.ApplyResources(Me.m_btnApply, "m_btnApply")
        Me.m_btnApply.Name = "m_btnApply"
        Me.m_btnApply.UseVisualStyleBackColor = True
        '
        'm_lblStepSize
        '
        resources.ApplyResources(Me.m_lblStepSize, "m_lblStepSize")
        Me.m_lblStepSize.Name = "m_lblStepSize"
        '
        'm_nudStepSize
        '
        resources.ApplyResources(Me.m_nudStepSize, "m_nudStepSize")
        Me.m_nudStepSize.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.m_nudStepSize.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudStepSize.Name = "m_nudStepSize"
        Me.m_nudStepSize.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_lblSearchBy
        '
        resources.ApplyResources(Me.m_lblSearchBy, "m_lblSearchBy")
        Me.m_lblSearchBy.Name = "m_lblSearchBy"
        '
        'm_rbPredator
        '
        resources.ApplyResources(Me.m_rbPredator, "m_rbPredator")
        Me.m_rbPredator.Name = "m_rbPredator"
        Me.m_rbPredator.TabStop = True
        Me.m_rbPredator.UseVisualStyleBackColor = True
        '
        'm_rbPredPrey
        '
        resources.ApplyResources(Me.m_rbPredPrey, "m_rbPredPrey")
        Me.m_rbPredPrey.Name = "m_rbPredPrey"
        Me.m_rbPredPrey.TabStop = True
        Me.m_rbPredPrey.UseVisualStyleBackColor = True
        '
        'm_hdrParameters
        '
        Me.m_hdrParameters.CanCollapseParent = False
        Me.m_hdrParameters.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrParameters, "m_hdrParameters")
        Me.m_hdrParameters.IsCollapsed = False
        Me.m_hdrParameters.Name = "m_hdrParameters"
        '
        'm_hdrOutput
        '
        resources.ApplyResources(Me.m_hdrOutput, "m_hdrOutput")
        Me.m_hdrOutput.CanCollapseParent = False
        Me.m_hdrOutput.CollapsedParentHeight = 0
        Me.m_hdrOutput.IsCollapsed = False
        Me.m_hdrOutput.Name = "m_hdrOutput"
        '
        'm_lblOutputFolder
        '
        resources.ApplyResources(Me.m_lblOutputFolder, "m_lblOutputFolder")
        Me.m_lblOutputFolder.Name = "m_lblOutputFolder"
        '
        'm_tbxOutputFolder
        '
        resources.ApplyResources(Me.m_tbxOutputFolder, "m_tbxOutputFolder")
        Me.m_tbxOutputFolder.Name = "m_tbxOutputFolder"
        Me.m_tbxOutputFolder.ReadOnly = True
        '
        'm_btnChooseFolder
        '
        resources.ApplyResources(Me.m_btnChooseFolder, "m_btnChooseFolder")
        Me.m_btnChooseFolder.Name = "m_btnChooseFolder"
        Me.m_btnChooseFolder.UseVisualStyleBackColor = True
        '
        'm_tlpContent
        '
        resources.ApplyResources(Me.m_tlpContent, "m_tlpContent")
        Me.m_tlpContent.Controls.Add(Me.m_plModel, 0, 0)
        Me.m_tlpContent.Controls.Add(Me.m_plConfig, 0, 2)
        Me.m_tlpContent.Controls.Add(Me.m_plRun, 0, 3)
        Me.m_tlpContent.Controls.Add(Me.m_plSettings, 0, 1)
        Me.m_tlpContent.Name = "m_tlpContent"
        '
        'm_plModel
        '
        resources.ApplyResources(Me.m_plModel, "m_plModel")
        Me.m_plModel.Controls.Add(Me.m_hdrEwE)
        Me.m_plModel.Controls.Add(Me.m_lblTimeSeries)
        Me.m_plModel.Controls.Add(Me.m_lblModel)
        Me.m_plModel.Controls.Add(Me.m_lblScenario)
        Me.m_plModel.Controls.Add(Me.m_cmbScenario)
        Me.m_plModel.Controls.Add(Me.m_tbxModel)
        Me.m_plModel.Controls.Add(Me.m_cmbTimeSeries)
        Me.m_plModel.Controls.Add(Me.m_btnSelectModel)
        Me.m_plModel.Name = "m_plModel"
        '
        'm_hdrEwE
        '
        resources.ApplyResources(Me.m_hdrEwE, "m_hdrEwE")
        Me.m_hdrEwE.CanCollapseParent = False
        Me.m_hdrEwE.CollapsedParentHeight = 0
        Me.m_hdrEwE.IsCollapsed = False
        Me.m_hdrEwE.Name = "m_hdrEwE"
        '
        'm_plConfig
        '
        resources.ApplyResources(Me.m_plConfig, "m_plConfig")
        Me.m_plConfig.Controls.Add(Me.m_btnSelectFishing)
        Me.m_plConfig.Controls.Add(Me.m_btnSelectBaseline)
        Me.m_plConfig.Controls.Add(Me.m_btnSelectVandA)
        Me.m_plConfig.Controls.Add(Me.m_btnSelectA)
        Me.m_plConfig.Controls.Add(Me.m_btnSelectV)
        Me.m_plConfig.Controls.Add(Me.m_btnSelectNone)
        Me.m_plConfig.Controls.Add(Me.m_btnSelectAll)
        Me.m_plConfig.Controls.Add(Me.m_grid)
        Me.m_plConfig.Controls.Add(Me.m_btnApply)
        Me.m_plConfig.Controls.Add(Me.m_hdrIterations)
        Me.m_plConfig.Name = "m_plConfig"
        '
        'm_btnSelectFishing
        '
        resources.ApplyResources(Me.m_btnSelectFishing, "m_btnSelectFishing")
        Me.m_btnSelectFishing.Name = "m_btnSelectFishing"
        Me.m_btnSelectFishing.UseVisualStyleBackColor = True
        '
        'm_btnSelectBaseline
        '
        resources.ApplyResources(Me.m_btnSelectBaseline, "m_btnSelectBaseline")
        Me.m_btnSelectBaseline.Name = "m_btnSelectBaseline"
        Me.m_btnSelectBaseline.UseVisualStyleBackColor = True
        '
        'm_btnSelectVandA
        '
        resources.ApplyResources(Me.m_btnSelectVandA, "m_btnSelectVandA")
        Me.m_btnSelectVandA.Name = "m_btnSelectVandA"
        Me.m_btnSelectVandA.UseVisualStyleBackColor = True
        '
        'm_btnSelectA
        '
        resources.ApplyResources(Me.m_btnSelectA, "m_btnSelectA")
        Me.m_btnSelectA.Name = "m_btnSelectA"
        Me.m_btnSelectA.UseVisualStyleBackColor = True
        '
        'm_btnSelectV
        '
        resources.ApplyResources(Me.m_btnSelectV, "m_btnSelectV")
        Me.m_btnSelectV.Name = "m_btnSelectV"
        Me.m_btnSelectV.UseVisualStyleBackColor = True
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        resources.ApplyResources(Me.m_grid, "m_grid")
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        Me.m_grid.AutoStretchColumnsToFitWidth = False
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BackColor = System.Drawing.Color.White
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grid.CustomSort = False
        Me.m_grid.DataName = "grid content"
        Me.m_grid.FixedColumnWidths = False
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = True
        Me.m_grid.Name = "m_grid"
        Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_grid.UIContext = Nothing
        '
        'm_plRun
        '
        resources.ApplyResources(Me.m_plRun, "m_plRun")
        Me.m_plRun.Controls.Add(Me.m_cmbAutoSave)
        Me.m_plRun.Controls.Add(Me.m_lblAutoSave)
        Me.m_plRun.Controls.Add(Me.m_hdrOutput)
        Me.m_plRun.Controls.Add(Me.m_lblOutputFolder)
        Me.m_plRun.Controls.Add(Me.m_btnRun)
        Me.m_plRun.Controls.Add(Me.m_tbxOutputFolder)
        Me.m_plRun.Controls.Add(Me.m_btnChooseFolder)
        Me.m_plRun.Name = "m_plRun"
        '
        'm_cmbAutoSave
        '
        Me.m_cmbAutoSave.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbAutoSave.FormattingEnabled = True
        Me.m_cmbAutoSave.Items.AddRange(New Object() {resources.GetString("m_cmbAutoSave.Items"), resources.GetString("m_cmbAutoSave.Items1"), resources.GetString("m_cmbAutoSave.Items2"), resources.GetString("m_cmbAutoSave.Items3")})
        resources.ApplyResources(Me.m_cmbAutoSave, "m_cmbAutoSave")
        Me.m_cmbAutoSave.Name = "m_cmbAutoSave"
        '
        'm_lblAutoSave
        '
        resources.ApplyResources(Me.m_lblAutoSave, "m_lblAutoSave")
        Me.m_lblAutoSave.Name = "m_lblAutoSave"
        '
        'm_plSettings
        '
        Me.m_plSettings.Controls.Add(Me.m_cmbAnomalyShape)
        Me.m_plSettings.Controls.Add(Me.m_lblAnomalyShape)
        Me.m_plSettings.Controls.Add(Me.m_cbEnableAbsBioforBaseline)
        Me.m_plSettings.Controls.Add(Me.m_hdrParameters)
        Me.m_plSettings.Controls.Add(Me.m_btnTS)
        Me.m_plSettings.Controls.Add(Me.m_rbPredPrey)
        Me.m_plSettings.Controls.Add(Me.m_rbPredator)
        Me.m_plSettings.Controls.Add(Me.m_lblSearchBy)
        Me.m_plSettings.Controls.Add(Me.m_nudK)
        Me.m_plSettings.Controls.Add(Me.m_lblNumVars)
        Me.m_plSettings.Controls.Add(Me.m_nudStepSize)
        Me.m_plSettings.Controls.Add(Me.m_lblStepSize)
        resources.ApplyResources(Me.m_plSettings, "m_plSettings")
        Me.m_plSettings.Name = "m_plSettings"
        '
        'm_cmbAnomalyShape
        '
        Me.m_cmbAnomalyShape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbAnomalyShape.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbAnomalyShape, "m_cmbAnomalyShape")
        Me.m_cmbAnomalyShape.Name = "m_cmbAnomalyShape"
        '
        'm_lblAnomalyShape
        '
        resources.ApplyResources(Me.m_lblAnomalyShape, "m_lblAnomalyShape")
        Me.m_lblAnomalyShape.Name = "m_lblAnomalyShape"
        '
        'm_cbEnableAbsBioforBaseline
        '
        resources.ApplyResources(Me.m_cbEnableAbsBioforBaseline, "m_cbEnableAbsBioforBaseline")
        Me.m_cbEnableAbsBioforBaseline.Name = "m_cbEnableAbsBioforBaseline"
        Me.m_cbEnableAbsBioforBaseline.UseVisualStyleBackColor = True
        '
        'm_btnTS
        '
        resources.ApplyResources(Me.m_btnTS, "m_btnTS")
        Me.m_btnTS.Name = "m_btnTS"
        Me.m_btnTS.UseVisualStyleBackColor = True
        '
        'm_nudK
        '
        resources.ApplyResources(Me.m_nudK, "m_nudK")
        Me.m_nudK.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.m_nudK.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudK.Name = "m_nudK"
        Me.m_nudK.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_lblNumVars
        '
        resources.ApplyResources(Me.m_lblNumVars, "m_lblNumVars")
        Me.m_lblNumVars.Name = "m_lblNumVars"
        '
        'm_btnClearAll
        '
        resources.ApplyResources(Me.m_btnClearAll, "m_btnClearAll")
        Me.m_btnClearAll.Name = "m_btnClearAll"
        Me.m_btnClearAll.UseVisualStyleBackColor = True
        '
        'frmRun
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_tlpContent)
        Me.Name = "frmRun"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        CType(Me.m_nudStepSize, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tlpContent.ResumeLayout(False)
        Me.m_plModel.ResumeLayout(False)
        Me.m_plModel.PerformLayout()
        Me.m_plConfig.ResumeLayout(False)
        Me.m_plRun.ResumeLayout(False)
        Me.m_plRun.PerformLayout()
        Me.m_plSettings.ResumeLayout(False)
        Me.m_plSettings.PerformLayout()
        CType(Me.m_nudK, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_lblModel As System.Windows.Forms.Label
    Private WithEvents m_tbxModel As System.Windows.Forms.TextBox
    Private WithEvents m_btnSelectModel As System.Windows.Forms.Button
    Private WithEvents m_lblScenario As System.Windows.Forms.Label
    Private WithEvents m_cmbScenario As System.Windows.Forms.ComboBox
    Private WithEvents m_cmbTimeSeries As System.Windows.Forms.ComboBox
    Private WithEvents m_lblTimeSeries As System.Windows.Forms.Label
    Private WithEvents m_grid As gridRun
    Private WithEvents m_hdrIterations As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_btnSelectAll As System.Windows.Forms.Button
    Private WithEvents m_btnSelectNone As System.Windows.Forms.Button
    Private WithEvents m_btnRun As System.Windows.Forms.Button
    Private WithEvents m_btnApply As System.Windows.Forms.Button
    Private WithEvents m_lblStepSize As System.Windows.Forms.Label
    Private WithEvents m_nudStepSize As System.Windows.Forms.NumericUpDown
    Private WithEvents m_lblSearchBy As System.Windows.Forms.Label
    Private WithEvents m_hdrParameters As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_rbPredator As System.Windows.Forms.RadioButton
    Private WithEvents m_rbPredPrey As System.Windows.Forms.RadioButton
    Private WithEvents m_hdrOutput As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_lblOutputFolder As System.Windows.Forms.Label
    Private WithEvents m_tbxOutputFolder As System.Windows.Forms.TextBox
    Private WithEvents m_btnChooseFolder As System.Windows.Forms.Button
    Private WithEvents m_tlpContent As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_plModel As System.Windows.Forms.Panel
    Private WithEvents m_hdrEwE As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_plConfig As System.Windows.Forms.Panel
    Private WithEvents m_plRun As System.Windows.Forms.Panel
    Private WithEvents m_btnClearAll As System.Windows.Forms.Button
    Private WithEvents m_btnTS As System.Windows.Forms.Button
    Private WithEvents m_btnSelectV As System.Windows.Forms.Button
    Private WithEvents m_btnSelectA As System.Windows.Forms.Button
    Private WithEvents m_btnSelectVandA As System.Windows.Forms.Button
    Private WithEvents m_cbEnableAbsBioforBaseline As System.Windows.Forms.CheckBox
    Private WithEvents m_cmbAutoSave As System.Windows.Forms.ComboBox
    Private WithEvents m_lblAutoSave As System.Windows.Forms.Label
    Private WithEvents m_btnSelectBaseline As System.Windows.Forms.Button
    Private WithEvents m_btnSelectFishing As System.Windows.Forms.Button
    Private WithEvents m_cmbAnomalyShape As System.Windows.Forms.ComboBox
    Friend WithEvents m_lblAnomalyShape As System.Windows.Forms.Label
    Private WithEvents m_plSettings As System.Windows.Forms.Panel
    Private WithEvents m_nudK As System.Windows.Forms.NumericUpDown
    Private WithEvents m_lblNumVars As System.Windows.Forms.Label
End Class
