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
Partial Class ucDefaults
    Inherits System.Windows.Forms.UserControl

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
        Me.m_pgDefaults = New System.Windows.Forms.PropertyGrid()
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.m_lbProducer = New EwEValueChainPlugin.ucUnitDefault()
        Me.m_lbProcessing = New EwEValueChainPlugin.ucUnitDefault()
        Me.m_lbWholesaler = New EwEValueChainPlugin.ucUnitDefault()
        Me.m_lbRetailer = New EwEValueChainPlugin.ucUnitDefault()
        Me.m_lbConsumer = New EwEValueChainPlugin.ucUnitDefault()
        Me.m_lnkProd2Proc = New EwEValueChainPlugin.ucLinkDefault()
        Me.m_lnkProc2Dist = New EwEValueChainPlugin.ucLinkDefault()
        Me.m_lnkWhole2Ret = New EwEValueChainPlugin.ucLinkDefault()
        Me.m_lnkRet2Cons = New EwEValueChainPlugin.ucLinkDefault()
        Me.m_lbDistribution = New EwEValueChainPlugin.ucUnitDefault()
        Me.m_lnkDist2Whole = New EwEValueChainPlugin.ucLinkDefault()
        Me.m_cbDefault = New System.Windows.Forms.ComboBox()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_pgDefaults
        '
        Me.m_pgDefaults.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_pgDefaults.Location = New System.Drawing.Point(3, 30)
        Me.m_pgDefaults.Name = "m_pgDefaults"
        Me.m_pgDefaults.Size = New System.Drawing.Size(257, 450)
        Me.m_pgDefaults.TabIndex = 1
        '
        'm_scMain
        '
        Me.m_scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.m_scMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_scMain.Location = New System.Drawing.Point(0, 0)
        Me.m_scMain.Margin = New System.Windows.Forms.Padding(0)
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.TableLayoutPanel1)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_cbDefault)
        Me.m_scMain.Panel2.Controls.Add(Me.m_pgDefaults)
        Me.m_scMain.Size = New System.Drawing.Size(452, 487)
        Me.m_scMain.SplitterDistance = 181
        Me.m_scMain.TabIndex = 0
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 3
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.m_lbProducer, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lbProcessing, 1, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lbWholesaler, 1, 7)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lbRetailer, 1, 9)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lbConsumer, 1, 11)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lnkProd2Proc, 1, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lnkProc2Dist, 1, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lnkWhole2Ret, 1, 8)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lnkRet2Cons, 1, 10)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lbDistribution, 1, 5)
        Me.TableLayoutPanel1.Controls.Add(Me.m_lnkDist2Whole, 1, 6)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 13
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.7149!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.140407!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.140407!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.143264!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.7149!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(177, 483)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'm_lbProducer
        '
        Me.m_lbProducer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lbProducer.BackColor = System.Drawing.SystemColors.Window
        Me.m_lbProducer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbProducer.Location = New System.Drawing.Point(38, 51)
        Me.m_lbProducer.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbProducer.Name = "m_lbProducer"
        Me.m_lbProducer.ObjDefault = Nothing
        Me.m_lbProducer.Selected = False
        Me.m_lbProducer.Size = New System.Drawing.Size(100, 34)
        Me.m_lbProducer.TabIndex = 0
        Me.m_lbProducer.UIContext = Nothing
        Me.m_lbProducer.UnitType = EwEValueChainPlugin.cUnitFactory.eUnitType.Producer
        '
        'm_lbProcessing
        '
        Me.m_lbProcessing.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lbProcessing.BackColor = System.Drawing.SystemColors.Window
        Me.m_lbProcessing.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbProcessing.Location = New System.Drawing.Point(38, 119)
        Me.m_lbProcessing.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbProcessing.Name = "m_lbProcessing"
        Me.m_lbProcessing.ObjDefault = Nothing
        Me.m_lbProcessing.Selected = False
        Me.m_lbProcessing.Size = New System.Drawing.Size(100, 34)
        Me.m_lbProcessing.TabIndex = 0
        Me.m_lbProcessing.UIContext = Nothing
        Me.m_lbProcessing.UnitType = EwEValueChainPlugin.cUnitFactory.eUnitType.Processing
        '
        'm_lbWholesaler
        '
        Me.m_lbWholesaler.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lbWholesaler.BackColor = System.Drawing.SystemColors.Window
        Me.m_lbWholesaler.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbWholesaler.Location = New System.Drawing.Point(38, 255)
        Me.m_lbWholesaler.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbWholesaler.Name = "m_lbWholesaler"
        Me.m_lbWholesaler.ObjDefault = Nothing
        Me.m_lbWholesaler.Selected = False
        Me.m_lbWholesaler.Size = New System.Drawing.Size(100, 34)
        Me.m_lbWholesaler.TabIndex = 0
        Me.m_lbWholesaler.UIContext = Nothing
        Me.m_lbWholesaler.UnitType = EwEValueChainPlugin.cUnitFactory.eUnitType.Wholesaler
        '
        'm_lbRetailer
        '
        Me.m_lbRetailer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lbRetailer.BackColor = System.Drawing.SystemColors.Window
        Me.m_lbRetailer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbRetailer.Location = New System.Drawing.Point(38, 323)
        Me.m_lbRetailer.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbRetailer.Name = "m_lbRetailer"
        Me.m_lbRetailer.ObjDefault = Nothing
        Me.m_lbRetailer.Selected = False
        Me.m_lbRetailer.Size = New System.Drawing.Size(100, 34)
        Me.m_lbRetailer.TabIndex = 0
        Me.m_lbRetailer.UIContext = Nothing
        Me.m_lbRetailer.UnitType = EwEValueChainPlugin.cUnitFactory.eUnitType.Retailer
        '
        'm_lbConsumer
        '
        Me.m_lbConsumer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lbConsumer.BackColor = System.Drawing.SystemColors.Window
        Me.m_lbConsumer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbConsumer.Location = New System.Drawing.Point(38, 391)
        Me.m_lbConsumer.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbConsumer.Name = "m_lbConsumer"
        Me.m_lbConsumer.ObjDefault = Nothing
        Me.m_lbConsumer.Selected = False
        Me.m_lbConsumer.Size = New System.Drawing.Size(100, 34)
        Me.m_lbConsumer.TabIndex = 0
        Me.m_lbConsumer.UIContext = Nothing
        Me.m_lbConsumer.UnitType = EwEValueChainPlugin.cUnitFactory.eUnitType.Consumer
        '
        'm_lnkProd2Proc
        '
        Me.m_lnkProd2Proc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lnkProd2Proc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lnkProd2Proc.Location = New System.Drawing.Point(41, 88)
        Me.m_lnkProd2Proc.Name = "m_lnkProd2Proc"
        Me.m_lnkProd2Proc.ObjDefault = Nothing
        Me.m_lnkProd2Proc.Selected = False
        Me.m_lnkProd2Proc.Size = New System.Drawing.Size(94, 28)
        Me.m_lnkProd2Proc.TabIndex = 1
        Me.m_lnkProd2Proc.UIContext = Nothing
        '
        'm_lnkProc2Dist
        '
        Me.m_lnkProc2Dist.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lnkProc2Dist.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lnkProc2Dist.Location = New System.Drawing.Point(41, 156)
        Me.m_lnkProc2Dist.Name = "m_lnkProc2Dist"
        Me.m_lnkProc2Dist.ObjDefault = Nothing
        Me.m_lnkProc2Dist.Selected = False
        Me.m_lnkProc2Dist.Size = New System.Drawing.Size(94, 28)
        Me.m_lnkProc2Dist.TabIndex = 1
        Me.m_lnkProc2Dist.UIContext = Nothing
        '
        'm_lnkWhole2Ret
        '
        Me.m_lnkWhole2Ret.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lnkWhole2Ret.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lnkWhole2Ret.Location = New System.Drawing.Point(41, 292)
        Me.m_lnkWhole2Ret.Name = "m_lnkWhole2Ret"
        Me.m_lnkWhole2Ret.ObjDefault = Nothing
        Me.m_lnkWhole2Ret.Selected = False
        Me.m_lnkWhole2Ret.Size = New System.Drawing.Size(94, 28)
        Me.m_lnkWhole2Ret.TabIndex = 1
        Me.m_lnkWhole2Ret.UIContext = Nothing
        '
        'm_lnkRet2Cons
        '
        Me.m_lnkRet2Cons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lnkRet2Cons.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lnkRet2Cons.Location = New System.Drawing.Point(41, 360)
        Me.m_lnkRet2Cons.Name = "m_lnkRet2Cons"
        Me.m_lnkRet2Cons.ObjDefault = Nothing
        Me.m_lnkRet2Cons.Selected = False
        Me.m_lnkRet2Cons.Size = New System.Drawing.Size(94, 28)
        Me.m_lnkRet2Cons.TabIndex = 1
        Me.m_lnkRet2Cons.UIContext = Nothing
        '
        'm_lbDistribution
        '
        Me.m_lbDistribution.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lbDistribution.BackColor = System.Drawing.SystemColors.Window
        Me.m_lbDistribution.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lbDistribution.Location = New System.Drawing.Point(38, 187)
        Me.m_lbDistribution.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbDistribution.Name = "m_lbDistribution"
        Me.m_lbDistribution.ObjDefault = Nothing
        Me.m_lbDistribution.Selected = False
        Me.m_lbDistribution.Size = New System.Drawing.Size(100, 34)
        Me.m_lbDistribution.TabIndex = 0
        Me.m_lbDistribution.UIContext = Nothing
        Me.m_lbDistribution.UnitType = EwEValueChainPlugin.cUnitFactory.eUnitType.Distribution
        '
        'm_lnkDist2Whole
        '
        Me.m_lnkDist2Whole.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_lnkDist2Whole.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lnkDist2Whole.Location = New System.Drawing.Point(41, 224)
        Me.m_lnkDist2Whole.Name = "m_lnkDist2Whole"
        Me.m_lnkDist2Whole.ObjDefault = Nothing
        Me.m_lnkDist2Whole.Selected = False
        Me.m_lnkDist2Whole.Size = New System.Drawing.Size(94, 28)
        Me.m_lnkDist2Whole.TabIndex = 1
        Me.m_lnkDist2Whole.UIContext = Nothing
        '
        'm_cbDefault
        '
        Me.m_cbDefault.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_cbDefault.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cbDefault.FormattingEnabled = True
        Me.m_cbDefault.Location = New System.Drawing.Point(3, 3)
        Me.m_cbDefault.Name = "m_cbDefault"
        Me.m_cbDefault.Size = New System.Drawing.Size(257, 21)
        Me.m_cbDefault.TabIndex = 0
        '
        'ucDefaults
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.m_scMain)
        Me.Name = "ucDefaults"
        Me.Size = New System.Drawing.Size(452, 487)
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel2.ResumeLayout(False)
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_pgDefaults As System.Windows.Forms.PropertyGrid
    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_lbProducer As ucUnitDefault
    Private WithEvents m_cbDefault As System.Windows.Forms.ComboBox
    Private WithEvents m_lbProcessing As ucUnitDefault
    Private WithEvents m_lbWholesaler As ucUnitDefault
    Private WithEvents m_lbRetailer As ucUnitDefault
    Private WithEvents m_lbConsumer As ucUnitDefault
    Private WithEvents m_lnkProd2Proc As ucLinkDefault
    Private WithEvents m_lnkProc2Dist As ucLinkDefault
    Private WithEvents m_lnkWhole2Ret As ucLinkDefault
    Private WithEvents m_lnkRet2Cons As ucLinkDefault
    Private WithEvents m_lnkDist2Whole As EwEValueChainPlugin.ucLinkDefault
    Private WithEvents m_lbDistribution As EwEValueChainPlugin.ucUnitDefault
End Class
