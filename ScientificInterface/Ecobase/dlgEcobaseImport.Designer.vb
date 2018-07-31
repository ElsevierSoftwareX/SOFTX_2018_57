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

Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms

Partial Class dlgEcobaseImport
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
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

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEcobaseImport))
        Me.m_scEcobaseContent = New System.Windows.Forms.SplitContainer()
        Me.m_hdrModels = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lbxModels = New ScientificInterface.cEcoBaseModelListBox()
        Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
        Me.m_hdrDetails = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tlpFields = New System.Windows.Forms.TableLayoutPanel()
        Me.m_lblModelName = New System.Windows.Forms.Label()
        Me.m_lblModelNameValue = New System.Windows.Forms.Label()
        Me.m_lblAuthor = New System.Windows.Forms.Label()
        Me.m_lblAuthorValue = New System.Windows.Forms.Label()
        Me.m_lblCountry = New System.Windows.Forms.Label()
        Me.m_lblCountryValue = New System.Windows.Forms.Label()
        Me.m_lblEcosimUsed = New System.Windows.Forms.Label()
        Me.m_lblFitted = New System.Windows.Forms.Label()
        Me.m_lblEcospaceUsed = New System.Windows.Forms.Label()
        Me.m_lblEcosimUsedValue = New System.Windows.Forms.Label()
        Me.m_lblFittedValue = New System.Windows.Forms.Label()
        Me.m_lblEcospaceUsedValue = New System.Windows.Forms.Label()
        Me.m_lblEcosystemType = New System.Windows.Forms.Label()
        Me.m_lblEcosystemTypeValue = New System.Windows.Forms.Label()
        Me.m_lblPeriod = New System.Windows.Forms.Label()
        Me.m_lblPeriodValue = New System.Windows.Forms.Label()
        Me.m_lblAreaValue = New System.Windows.Forms.Label()
        Me.m_lblArea = New System.Windows.Forms.Label()
        Me.m_lblDessimAllow = New System.Windows.Forms.Label()
        Me.m_lblDessimAllowValue = New System.Windows.Forms.Label()
        Me.m_hdrRefs = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tlpSpatial = New System.Windows.Forms.TableLayoutPanel()
        Me.m_pbImage = New System.Windows.Forms.PictureBox()
        Me.m_tlpSpatFields = New System.Windows.Forms.TableLayoutPanel()
        Me.m_lblNS = New System.Windows.Forms.Label()
        Me.m_lblLat = New System.Windows.Forms.Label()
        Me.m_lblLonVal = New System.Windows.Forms.Label()
        Me.m_lblLatVal = New System.Windows.Forms.Label()
        Me.m_lblDepthMean = New System.Windows.Forms.Label()
        Me.m_lblTempMean = New System.Windows.Forms.Label()
        Me.m_lblTempRange = New System.Windows.Forms.Label()
        Me.m_lblDepthMeanVal = New System.Windows.Forms.Label()
        Me.m_lblDepthRangeVal = New System.Windows.Forms.Label()
        Me.m_lblDepth = New System.Windows.Forms.Label()
        Me.m_lblTempRangeVal = New System.Windows.Forms.Label()
        Me.m_lblTempMeanVal = New System.Windows.Forms.Label()
        Me.m_llToEcoBase = New System.Windows.Forms.LinkLabel()
        Me.m_hdrSpatial = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_llDOI = New System.Windows.Forms.LinkLabel()
        Me.m_lblRefValue = New System.Windows.Forms.Label()
        Me.m_llURL = New System.Windows.Forms.LinkLabel()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_wrkGetModels = New System.ComponentModel.BackgroundWorker()
        Me.m_tsFilter = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tstbSearch = New System.Windows.Forms.ToolStripTextBox()
        Me.m_tsddValue = New System.Windows.Forms.ToolStripDropDownButton()
        Me.m_tsmiNone = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiModelName = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiAuthor = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiCountry = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcoType = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiDepth = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiTemperature = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiReference = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsllShow = New System.Windows.Forms.ToolStripLabel()
        Me.m_tsbnShowYear = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnShowAuthor = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnShowDownloadable = New System.Windows.Forms.ToolStripButton()
        Me.m_tcContent = New System.Windows.Forms.TabControl()
        Me.m_tpAgreement = New System.Windows.Forms.TabPage()
        Me.m_pbAgreement = New System.Windows.Forms.PictureBox()
        Me.m_rtfAgreement = New System.Windows.Forms.RichTextBox()
        Me.m_pbLogo = New System.Windows.Forms.PictureBox()
        Me.m_cbEcoBaseAgreement = New System.Windows.Forms.CheckBox()
        Me.m_tpImport = New System.Windows.Forms.TabPage()
        Me.m_wrkGetAgreement = New System.ComponentModel.BackgroundWorker()
        Me.m_wrkGetImage = New System.ComponentModel.BackgroundWorker()
        CType(Me.m_scEcobaseContent, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scEcobaseContent.Panel1.SuspendLayout()
        Me.m_scEcobaseContent.Panel2.SuspendLayout()
        Me.m_scEcobaseContent.SuspendLayout()
        Me.m_tlpMain.SuspendLayout()
        Me.m_tlpFields.SuspendLayout()
        Me.m_tlpSpatial.SuspendLayout()
        CType(Me.m_pbImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tlpSpatFields.SuspendLayout()
        Me.m_tsFilter.SuspendLayout()
        Me.m_tcContent.SuspendLayout()
        Me.m_tpAgreement.SuspendLayout()
        CType(Me.m_pbAgreement, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpImport.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_scEcobaseContent
        '
        resources.ApplyResources(Me.m_scEcobaseContent, "m_scEcobaseContent")
        Me.m_scEcobaseContent.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.m_scEcobaseContent.Name = "m_scEcobaseContent"
        '
        'm_scEcobaseContent.Panel1
        '
        Me.m_scEcobaseContent.Panel1.Controls.Add(Me.m_hdrModels)
        Me.m_scEcobaseContent.Panel1.Controls.Add(Me.m_lbxModels)
        '
        'm_scEcobaseContent.Panel2
        '
        resources.ApplyResources(Me.m_scEcobaseContent.Panel2, "m_scEcobaseContent.Panel2")
        Me.m_scEcobaseContent.Panel2.Controls.Add(Me.m_tlpMain)
        '
        'm_hdrModels
        '
        Me.m_hdrModels.CanCollapseParent = False
        Me.m_hdrModels.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrModels, "m_hdrModels")
        Me.m_hdrModels.IsCollapsed = False
        Me.m_hdrModels.Name = "m_hdrModels"
        '
        'm_lbxModels
        '
        resources.ApplyResources(Me.m_lbxModels, "m_lbxModels")
        Me.m_lbxModels.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.m_lbxModels.FormattingEnabled = True
        Me.m_lbxModels.Name = "m_lbxModels"
        Me.m_lbxModels.ShowAuthor = True
        Me.m_lbxModels.ShowYear = True
        Me.m_lbxModels.Sorted = True
        Me.m_lbxModels.UIContext = Nothing
        '
        'm_tlpMain
        '
        resources.ApplyResources(Me.m_tlpMain, "m_tlpMain")
        Me.m_tlpMain.Controls.Add(Me.m_hdrDetails, 0, 0)
        Me.m_tlpMain.Controls.Add(Me.m_tlpFields, 0, 1)
        Me.m_tlpMain.Controls.Add(Me.m_hdrRefs, 0, 2)
        Me.m_tlpMain.Controls.Add(Me.m_tlpSpatial, 0, 8)
        Me.m_tlpMain.Controls.Add(Me.m_hdrSpatial, 0, 6)
        Me.m_tlpMain.Controls.Add(Me.m_llDOI, 0, 5)
        Me.m_tlpMain.Controls.Add(Me.m_lblRefValue, 0, 3)
        Me.m_tlpMain.Controls.Add(Me.m_llURL, 0, 4)
        Me.m_tlpMain.Name = "m_tlpMain"
        '
        'm_hdrDetails
        '
        Me.m_hdrDetails.CanCollapseParent = False
        Me.m_hdrDetails.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrDetails, "m_hdrDetails")
        Me.m_hdrDetails.IsCollapsed = False
        Me.m_hdrDetails.Name = "m_hdrDetails"
        '
        'm_tlpFields
        '
        resources.ApplyResources(Me.m_tlpFields, "m_tlpFields")
        Me.m_tlpFields.Controls.Add(Me.m_lblModelName, 0, 0)
        Me.m_tlpFields.Controls.Add(Me.m_lblModelNameValue, 1, 0)
        Me.m_tlpFields.Controls.Add(Me.m_lblAuthor, 0, 1)
        Me.m_tlpFields.Controls.Add(Me.m_lblAuthorValue, 1, 1)
        Me.m_tlpFields.Controls.Add(Me.m_lblCountry, 0, 2)
        Me.m_tlpFields.Controls.Add(Me.m_lblCountryValue, 1, 2)
        Me.m_tlpFields.Controls.Add(Me.m_lblEcosimUsed, 2, 0)
        Me.m_tlpFields.Controls.Add(Me.m_lblFitted, 2, 1)
        Me.m_tlpFields.Controls.Add(Me.m_lblEcospaceUsed, 2, 2)
        Me.m_tlpFields.Controls.Add(Me.m_lblEcosimUsedValue, 3, 0)
        Me.m_tlpFields.Controls.Add(Me.m_lblFittedValue, 3, 1)
        Me.m_tlpFields.Controls.Add(Me.m_lblEcospaceUsedValue, 3, 2)
        Me.m_tlpFields.Controls.Add(Me.m_lblEcosystemType, 2, 3)
        Me.m_tlpFields.Controls.Add(Me.m_lblEcosystemTypeValue, 3, 3)
        Me.m_tlpFields.Controls.Add(Me.m_lblPeriod, 0, 3)
        Me.m_tlpFields.Controls.Add(Me.m_lblPeriodValue, 1, 3)
        Me.m_tlpFields.Controls.Add(Me.m_lblAreaValue, 1, 4)
        Me.m_tlpFields.Controls.Add(Me.m_lblArea, 0, 4)
        Me.m_tlpFields.Controls.Add(Me.m_lblDessimAllow, 2, 4)
        Me.m_tlpFields.Controls.Add(Me.m_lblDessimAllowValue, 3, 4)
        Me.m_tlpFields.Name = "m_tlpFields"
        '
        'm_lblModelName
        '
        resources.ApplyResources(Me.m_lblModelName, "m_lblModelName")
        Me.m_lblModelName.Name = "m_lblModelName"
        '
        'm_lblModelNameValue
        '
        resources.ApplyResources(Me.m_lblModelNameValue, "m_lblModelNameValue")
        Me.m_lblModelNameValue.Name = "m_lblModelNameValue"
        '
        'm_lblAuthor
        '
        resources.ApplyResources(Me.m_lblAuthor, "m_lblAuthor")
        Me.m_lblAuthor.Name = "m_lblAuthor"
        '
        'm_lblAuthorValue
        '
        resources.ApplyResources(Me.m_lblAuthorValue, "m_lblAuthorValue")
        Me.m_lblAuthorValue.Name = "m_lblAuthorValue"
        '
        'm_lblCountry
        '
        resources.ApplyResources(Me.m_lblCountry, "m_lblCountry")
        Me.m_lblCountry.Name = "m_lblCountry"
        '
        'm_lblCountryValue
        '
        resources.ApplyResources(Me.m_lblCountryValue, "m_lblCountryValue")
        Me.m_lblCountryValue.Name = "m_lblCountryValue"
        '
        'm_lblEcosimUsed
        '
        resources.ApplyResources(Me.m_lblEcosimUsed, "m_lblEcosimUsed")
        Me.m_lblEcosimUsed.Name = "m_lblEcosimUsed"
        '
        'm_lblFitted
        '
        resources.ApplyResources(Me.m_lblFitted, "m_lblFitted")
        Me.m_lblFitted.Name = "m_lblFitted"
        '
        'm_lblEcospaceUsed
        '
        resources.ApplyResources(Me.m_lblEcospaceUsed, "m_lblEcospaceUsed")
        Me.m_lblEcospaceUsed.Name = "m_lblEcospaceUsed"
        '
        'm_lblEcosimUsedValue
        '
        resources.ApplyResources(Me.m_lblEcosimUsedValue, "m_lblEcosimUsedValue")
        Me.m_lblEcosimUsedValue.Name = "m_lblEcosimUsedValue"
        '
        'm_lblFittedValue
        '
        resources.ApplyResources(Me.m_lblFittedValue, "m_lblFittedValue")
        Me.m_lblFittedValue.Name = "m_lblFittedValue"
        '
        'm_lblEcospaceUsedValue
        '
        resources.ApplyResources(Me.m_lblEcospaceUsedValue, "m_lblEcospaceUsedValue")
        Me.m_lblEcospaceUsedValue.Name = "m_lblEcospaceUsedValue"
        '
        'm_lblEcosystemType
        '
        resources.ApplyResources(Me.m_lblEcosystemType, "m_lblEcosystemType")
        Me.m_lblEcosystemType.Name = "m_lblEcosystemType"
        '
        'm_lblEcosystemTypeValue
        '
        resources.ApplyResources(Me.m_lblEcosystemTypeValue, "m_lblEcosystemTypeValue")
        Me.m_lblEcosystemTypeValue.Name = "m_lblEcosystemTypeValue"
        '
        'm_lblPeriod
        '
        resources.ApplyResources(Me.m_lblPeriod, "m_lblPeriod")
        Me.m_lblPeriod.Name = "m_lblPeriod"
        '
        'm_lblPeriodValue
        '
        resources.ApplyResources(Me.m_lblPeriodValue, "m_lblPeriodValue")
        Me.m_lblPeriodValue.Name = "m_lblPeriodValue"
        '
        'm_lblAreaValue
        '
        resources.ApplyResources(Me.m_lblAreaValue, "m_lblAreaValue")
        Me.m_lblAreaValue.Name = "m_lblAreaValue"
        '
        'm_lblArea
        '
        resources.ApplyResources(Me.m_lblArea, "m_lblArea")
        Me.m_lblArea.Name = "m_lblArea"
        '
        'm_lblDessimAllow
        '
        resources.ApplyResources(Me.m_lblDessimAllow, "m_lblDessimAllow")
        Me.m_lblDessimAllow.Name = "m_lblDessimAllow"
        '
        'm_lblDessimAllowValue
        '
        resources.ApplyResources(Me.m_lblDessimAllowValue, "m_lblDessimAllowValue")
        Me.m_lblDessimAllowValue.Name = "m_lblDessimAllowValue"
        '
        'm_hdrRefs
        '
        Me.m_hdrRefs.CanCollapseParent = False
        Me.m_hdrRefs.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrRefs, "m_hdrRefs")
        Me.m_hdrRefs.IsCollapsed = False
        Me.m_hdrRefs.Name = "m_hdrRefs"
        '
        'm_tlpSpatial
        '
        resources.ApplyResources(Me.m_tlpSpatial, "m_tlpSpatial")
        Me.m_tlpSpatial.Controls.Add(Me.m_pbImage, 0, 0)
        Me.m_tlpSpatial.Controls.Add(Me.m_tlpSpatFields, 1, 0)
        Me.m_tlpSpatial.Name = "m_tlpSpatial"
        '
        'm_pbImage
        '
        Me.m_pbImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.m_pbImage, "m_pbImage")
        Me.m_pbImage.Name = "m_pbImage"
        Me.m_pbImage.TabStop = False
        '
        'm_tlpSpatFields
        '
        resources.ApplyResources(Me.m_tlpSpatFields, "m_tlpSpatFields")
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblNS, 0, 0)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblLat, 0, 1)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblLonVal, 1, 0)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblLatVal, 1, 1)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblDepthMean, 0, 3)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblTempMean, 0, 5)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblTempRange, 0, 4)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblDepthMeanVal, 1, 3)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblDepthRangeVal, 1, 2)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblDepth, 0, 2)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblTempRangeVal, 1, 4)
        Me.m_tlpSpatFields.Controls.Add(Me.m_lblTempMeanVal, 1, 5)
        Me.m_tlpSpatFields.Controls.Add(Me.m_llToEcoBase, 1, 7)
        Me.m_tlpSpatFields.Name = "m_tlpSpatFields"
        '
        'm_lblNS
        '
        resources.ApplyResources(Me.m_lblNS, "m_lblNS")
        Me.m_lblNS.Name = "m_lblNS"
        '
        'm_lblLat
        '
        resources.ApplyResources(Me.m_lblLat, "m_lblLat")
        Me.m_lblLat.Name = "m_lblLat"
        '
        'm_lblLonVal
        '
        resources.ApplyResources(Me.m_lblLonVal, "m_lblLonVal")
        Me.m_lblLonVal.Name = "m_lblLonVal"
        '
        'm_lblLatVal
        '
        resources.ApplyResources(Me.m_lblLatVal, "m_lblLatVal")
        Me.m_lblLatVal.Name = "m_lblLatVal"
        '
        'm_lblDepthMean
        '
        resources.ApplyResources(Me.m_lblDepthMean, "m_lblDepthMean")
        Me.m_lblDepthMean.Name = "m_lblDepthMean"
        '
        'm_lblTempMean
        '
        resources.ApplyResources(Me.m_lblTempMean, "m_lblTempMean")
        Me.m_lblTempMean.Name = "m_lblTempMean"
        '
        'm_lblTempRange
        '
        resources.ApplyResources(Me.m_lblTempRange, "m_lblTempRange")
        Me.m_lblTempRange.Name = "m_lblTempRange"
        '
        'm_lblDepthMeanVal
        '
        resources.ApplyResources(Me.m_lblDepthMeanVal, "m_lblDepthMeanVal")
        Me.m_lblDepthMeanVal.Name = "m_lblDepthMeanVal"
        '
        'm_lblDepthRangeVal
        '
        resources.ApplyResources(Me.m_lblDepthRangeVal, "m_lblDepthRangeVal")
        Me.m_lblDepthRangeVal.Name = "m_lblDepthRangeVal"
        '
        'm_lblDepth
        '
        resources.ApplyResources(Me.m_lblDepth, "m_lblDepth")
        Me.m_lblDepth.Name = "m_lblDepth"
        '
        'm_lblTempRangeVal
        '
        resources.ApplyResources(Me.m_lblTempRangeVal, "m_lblTempRangeVal")
        Me.m_lblTempRangeVal.Name = "m_lblTempRangeVal"
        '
        'm_lblTempMeanVal
        '
        resources.ApplyResources(Me.m_lblTempMeanVal, "m_lblTempMeanVal")
        Me.m_lblTempMeanVal.Name = "m_lblTempMeanVal"
        '
        'm_llToEcoBase
        '
        resources.ApplyResources(Me.m_llToEcoBase, "m_llToEcoBase")
        Me.m_llToEcoBase.Name = "m_llToEcoBase"
        Me.m_llToEcoBase.TabStop = True
        '
        'm_hdrSpatial
        '
        Me.m_hdrSpatial.CanCollapseParent = False
        Me.m_hdrSpatial.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrSpatial, "m_hdrSpatial")
        Me.m_hdrSpatial.IsCollapsed = False
        Me.m_hdrSpatial.Name = "m_hdrSpatial"
        '
        'm_llDOI
        '
        resources.ApplyResources(Me.m_llDOI, "m_llDOI")
        Me.m_llDOI.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline
        Me.m_llDOI.Name = "m_llDOI"
        Me.m_llDOI.TabStop = True
        '
        'm_lblRefValue
        '
        resources.ApplyResources(Me.m_lblRefValue, "m_lblRefValue")
        Me.m_lblRefValue.Name = "m_lblRefValue"
        '
        'm_llURL
        '
        resources.ApplyResources(Me.m_llURL, "m_llURL")
        Me.m_llURL.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline
        Me.m_llURL.Name = "m_llURL"
        Me.m_llURL.TabStop = True
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_btnOK
        '
        resources.ApplyResources(Me.m_btnOK, "m_btnOK")
        Me.m_btnOK.Name = "m_btnOK"
        Me.m_btnOK.UseVisualStyleBackColor = True
        '
        'm_wrkGetModels
        '
        '
        'm_tsFilter
        '
        Me.m_tsFilter.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsFilter.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tstbSearch, Me.m_tsddValue, Me.m_tsllShow, Me.m_tsbnShowYear, Me.m_tsbnShowAuthor, Me.m_tsbnShowDownloadable})
        resources.ApplyResources(Me.m_tsFilter, "m_tsFilter")
        Me.m_tsFilter.Name = "m_tsFilter"
        Me.m_tsFilter.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tstbSearch
        '
        Me.m_tstbSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tstbSearch.Name = "m_tstbSearch"
        resources.ApplyResources(Me.m_tstbSearch, "m_tstbSearch")
        '
        'm_tsddValue
        '
        Me.m_tsddValue.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tsddValue.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiNone, Me.m_tsmiModelName, Me.m_tsmiAuthor, Me.m_tsmiCountry, Me.m_tsmiEcoType, Me.m_tsmiDepth, Me.m_tsmiTemperature, Me.m_tsmiReference})
        resources.ApplyResources(Me.m_tsddValue, "m_tsddValue")
        Me.m_tsddValue.Name = "m_tsddValue"
        '
        'm_tsmiNone
        '
        Me.m_tsmiNone.Name = "m_tsmiNone"
        resources.ApplyResources(Me.m_tsmiNone, "m_tsmiNone")
        Me.m_tsmiNone.Tag = "0"
        '
        'm_tsmiModelName
        '
        Me.m_tsmiModelName.Name = "m_tsmiModelName"
        resources.ApplyResources(Me.m_tsmiModelName, "m_tsmiModelName")
        Me.m_tsmiModelName.Tag = "7"
        '
        'm_tsmiAuthor
        '
        Me.m_tsmiAuthor.Name = "m_tsmiAuthor"
        resources.ApplyResources(Me.m_tsmiAuthor, "m_tsmiAuthor")
        Me.m_tsmiAuthor.Tag = "1"
        '
        'm_tsmiCountry
        '
        Me.m_tsmiCountry.Name = "m_tsmiCountry"
        resources.ApplyResources(Me.m_tsmiCountry, "m_tsmiCountry")
        Me.m_tsmiCountry.Tag = "2"
        '
        'm_tsmiEcoType
        '
        Me.m_tsmiEcoType.Name = "m_tsmiEcoType"
        resources.ApplyResources(Me.m_tsmiEcoType, "m_tsmiEcoType")
        Me.m_tsmiEcoType.Tag = "3"
        '
        'm_tsmiDepth
        '
        Me.m_tsmiDepth.Name = "m_tsmiDepth"
        resources.ApplyResources(Me.m_tsmiDepth, "m_tsmiDepth")
        Me.m_tsmiDepth.Tag = "4"
        '
        'm_tsmiTemperature
        '
        Me.m_tsmiTemperature.Name = "m_tsmiTemperature"
        resources.ApplyResources(Me.m_tsmiTemperature, "m_tsmiTemperature")
        Me.m_tsmiTemperature.Tag = "5"
        '
        'm_tsmiReference
        '
        Me.m_tsmiReference.Name = "m_tsmiReference"
        resources.ApplyResources(Me.m_tsmiReference, "m_tsmiReference")
        Me.m_tsmiReference.Tag = "6"
        '
        'm_tsllShow
        '
        Me.m_tsllShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsllShow.Image = Global.ScientificInterface.My.Resources.Resources.EcoBase1
        Me.m_tsllShow.Name = "m_tsllShow"
        resources.ApplyResources(Me.m_tsllShow, "m_tsllShow")
        '
        'm_tsbnShowYear
        '
        Me.m_tsbnShowYear.CheckOnClick = True
        Me.m_tsbnShowYear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnShowYear, "m_tsbnShowYear")
        Me.m_tsbnShowYear.Name = "m_tsbnShowYear"
        '
        'm_tsbnShowAuthor
        '
        Me.m_tsbnShowAuthor.CheckOnClick = True
        Me.m_tsbnShowAuthor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnShowAuthor, "m_tsbnShowAuthor")
        Me.m_tsbnShowAuthor.Name = "m_tsbnShowAuthor"
        '
        'm_tsbnShowDownloadable
        '
        Me.m_tsbnShowDownloadable.CheckOnClick = True
        Me.m_tsbnShowDownloadable.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnShowDownloadable, "m_tsbnShowDownloadable")
        Me.m_tsbnShowDownloadable.Name = "m_tsbnShowDownloadable"
        '
        'm_tcContent
        '
        resources.ApplyResources(Me.m_tcContent, "m_tcContent")
        Me.m_tcContent.Controls.Add(Me.m_tpAgreement)
        Me.m_tcContent.Controls.Add(Me.m_tpImport)
        Me.m_tcContent.Name = "m_tcContent"
        Me.m_tcContent.SelectedIndex = 0
        '
        'm_tpAgreement
        '
        Me.m_tpAgreement.Controls.Add(Me.m_pbAgreement)
        Me.m_tpAgreement.Controls.Add(Me.m_rtfAgreement)
        Me.m_tpAgreement.Controls.Add(Me.m_pbLogo)
        Me.m_tpAgreement.Controls.Add(Me.m_cbEcoBaseAgreement)
        resources.ApplyResources(Me.m_tpAgreement, "m_tpAgreement")
        Me.m_tpAgreement.Name = "m_tpAgreement"
        Me.m_tpAgreement.UseVisualStyleBackColor = True
        '
        'm_pbAgreement
        '
        resources.ApplyResources(Me.m_pbAgreement, "m_pbAgreement")
        Me.m_pbAgreement.Name = "m_pbAgreement"
        Me.m_pbAgreement.TabStop = False
        '
        'm_rtfAgreement
        '
        resources.ApplyResources(Me.m_rtfAgreement, "m_rtfAgreement")
        Me.m_rtfAgreement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_rtfAgreement.Name = "m_rtfAgreement"
        Me.m_rtfAgreement.ReadOnly = True
        '
        'm_pbLogo
        '
        Me.m_pbLogo.BackgroundImage = Global.ScientificInterface.My.Resources.Resources.EcoBase1
        resources.ApplyResources(Me.m_pbLogo, "m_pbLogo")
        Me.m_pbLogo.Name = "m_pbLogo"
        Me.m_pbLogo.TabStop = False
        '
        'm_cbEcoBaseAgreement
        '
        resources.ApplyResources(Me.m_cbEcoBaseAgreement, "m_cbEcoBaseAgreement")
        Me.m_cbEcoBaseAgreement.Name = "m_cbEcoBaseAgreement"
        Me.m_cbEcoBaseAgreement.UseVisualStyleBackColor = True
        '
        'm_tpImport
        '
        Me.m_tpImport.Controls.Add(Me.m_scEcobaseContent)
        Me.m_tpImport.Controls.Add(Me.m_tsFilter)
        resources.ApplyResources(Me.m_tpImport, "m_tpImport")
        Me.m_tpImport.Name = "m_tpImport"
        Me.m_tpImport.UseVisualStyleBackColor = True
        '
        'm_wrkGetAgreement
        '
        '
        'm_wrkGetImage
        '
        '
        'dlgEcobaseImport
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_tcContent)
        Me.Controls.Add(Me.m_btnOK)
        Me.Controls.Add(Me.m_btnCancel)
        Me.DoubleBuffered = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgEcobaseImport"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.TabText = ""
        Me.m_scEcobaseContent.Panel1.ResumeLayout(False)
        Me.m_scEcobaseContent.Panel2.ResumeLayout(False)
        CType(Me.m_scEcobaseContent, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scEcobaseContent.ResumeLayout(False)
        Me.m_tlpMain.ResumeLayout(False)
        Me.m_tlpMain.PerformLayout()
        Me.m_tlpFields.ResumeLayout(False)
        Me.m_tlpFields.PerformLayout()
        Me.m_tlpSpatial.ResumeLayout(False)
        CType(Me.m_pbImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tlpSpatFields.ResumeLayout(False)
        Me.m_tlpSpatFields.PerformLayout()
        Me.m_tsFilter.ResumeLayout(False)
        Me.m_tsFilter.PerformLayout()
        Me.m_tcContent.ResumeLayout(False)
        Me.m_tpAgreement.ResumeLayout(False)
        Me.m_tpAgreement.PerformLayout()
        CType(Me.m_pbAgreement, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbLogo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpImport.ResumeLayout(False)
        Me.m_tpImport.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_scEcobaseContent As System.Windows.Forms.SplitContainer
    Private WithEvents m_lbxModels As cEcoBaseModelListBox
    Private WithEvents m_btnOK As System.Windows.Forms.Button
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_wrkGetModels As System.ComponentModel.BackgroundWorker
    Private WithEvents m_hdrModels As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrDetails As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tsFilter As cEwEToolstrip
    Private WithEvents m_tcContent As System.Windows.Forms.TabControl
    Private WithEvents m_tpAgreement As System.Windows.Forms.TabPage
    Private WithEvents m_tpImport As System.Windows.Forms.TabPage
    Private WithEvents m_pbAgreement As System.Windows.Forms.PictureBox
    Private WithEvents m_rtfAgreement As System.Windows.Forms.RichTextBox
    Private WithEvents m_cbEcoBaseAgreement As System.Windows.Forms.CheckBox
    Private WithEvents m_wrkGetAgreement As System.ComponentModel.BackgroundWorker
    Private WithEvents m_tstbSearch As System.Windows.Forms.ToolStripTextBox
    Private WithEvents m_tsddValue As System.Windows.Forms.ToolStripDropDownButton
    Private WithEvents m_tsmiCountry As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcoType As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiDepth As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiAuthor As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiTemperature As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiNone As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tlpMain As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_hdrRefs As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_lblRefValue As System.Windows.Forms.Label
    Private WithEvents m_wrkGetImage As System.ComponentModel.BackgroundWorker
    Private WithEvents m_pbImage As System.Windows.Forms.PictureBox
    Private WithEvents m_pbLogo As System.Windows.Forms.PictureBox
    Private WithEvents m_tlpSpatial As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_tlpSpatFields As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_lblNS As System.Windows.Forms.Label
    Private WithEvents m_lblLat As System.Windows.Forms.Label
    Private WithEvents m_lblLonVal As System.Windows.Forms.Label
    Private WithEvents m_lblLatVal As System.Windows.Forms.Label
    Private WithEvents m_lblDepthMean As System.Windows.Forms.Label
    Private WithEvents m_lblTempMean As System.Windows.Forms.Label
    Private WithEvents m_lblTempRange As System.Windows.Forms.Label
    Private WithEvents m_lblDepthMeanVal As System.Windows.Forms.Label
    Private WithEvents m_lblDepthRangeVal As System.Windows.Forms.Label
    Private WithEvents m_lblDepth As System.Windows.Forms.Label
    Private WithEvents m_lblTempRangeVal As System.Windows.Forms.Label
    Private WithEvents m_lblTempMeanVal As System.Windows.Forms.Label
    Private WithEvents m_hdrSpatial As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tlpFields As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_lblModelName As System.Windows.Forms.Label
    Private WithEvents m_lblModelNameValue As System.Windows.Forms.Label
    Private WithEvents m_lblAuthor As System.Windows.Forms.Label
    Private WithEvents m_lblAuthorValue As System.Windows.Forms.Label
    Private WithEvents m_lblCountry As System.Windows.Forms.Label
    Private WithEvents m_lblCountryValue As System.Windows.Forms.Label
    Private WithEvents m_lblEcosimUsed As System.Windows.Forms.Label
    Private WithEvents m_lblFitted As System.Windows.Forms.Label
    Private WithEvents m_lblEcospaceUsed As System.Windows.Forms.Label
    Private WithEvents m_lblEcosimUsedValue As System.Windows.Forms.Label
    Private WithEvents m_lblFittedValue As System.Windows.Forms.Label
    Private WithEvents m_lblEcospaceUsedValue As System.Windows.Forms.Label
    Private WithEvents m_lblEcosystemType As System.Windows.Forms.Label
    Private WithEvents m_lblEcosystemTypeValue As System.Windows.Forms.Label
    Private WithEvents m_lblPeriod As System.Windows.Forms.Label
    Private WithEvents m_lblPeriodValue As System.Windows.Forms.Label
    Private WithEvents m_lblAreaValue As System.Windows.Forms.Label
    Private WithEvents m_lblArea As System.Windows.Forms.Label
    Private WithEvents m_llToEcoBase As System.Windows.Forms.LinkLabel
    Private WithEvents m_tsmiReference As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsbnShowYear As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnShowAuthor As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsllShow As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tsbnShowDownloadable As System.Windows.Forms.ToolStripButton
    Private WithEvents m_lblDessimAllow As System.Windows.Forms.Label
    Private WithEvents m_lblDessimAllowValue As System.Windows.Forms.Label
    Private WithEvents m_llDOI As System.Windows.Forms.LinkLabel
    Private WithEvents m_tsmiModelName As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_llURL As System.Windows.Forms.LinkLabel
End Class
