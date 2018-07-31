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

Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecosim

    Partial Class dlgApplyPredPreyShape
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgApplyPredPreyShape))
            Me.OK_Button = New System.Windows.Forms.Button()
            Me.Cancel_Button = New System.Windows.Forms.Button()
            Me.m_lblAvailableFF = New System.Windows.Forms.Label()
            Me.m_rbOpt1 = New System.Windows.Forms.RadioButton()
            Me.m_rbOpt2 = New System.Windows.Forms.RadioButton()
            Me.m_rbOpt4 = New System.Windows.Forms.RadioButton()
            Me.m_rbOpt3 = New System.Windows.Forms.RadioButton()
            Me.m_btnAdd = New System.Windows.Forms.Button()
            Me.m_lvAppliedShapes = New System.Windows.Forms.ListView()
            Me.m_colhdrShape = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_colhdrModifier = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_lvAllShapes = New System.Windows.Forms.ListView()
            Me.m_btnRemove = New System.Windows.Forms.Button()
            Me.m_lblAppliedFF = New System.Windows.Forms.Label()
            Me.m_tlpApplications = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lblApplyTo = New System.Windows.Forms.Label()
            Me.m_tlpInfo = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lblTarget = New System.Windows.Forms.Label()
            Me.m_lblSource = New System.Windows.Forms.Label()
            Me.m_tlpApplications.SuspendLayout()
            Me.m_tlpInfo.SuspendLayout()
            Me.SuspendLayout()
            '
            'OK_Button
            '
            resources.ApplyResources(Me.OK_Button, "OK_Button")
            Me.OK_Button.Name = "OK_Button"
            '
            'Cancel_Button
            '
            resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
            Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Cancel_Button.Name = "Cancel_Button"
            '
            'm_lblAvailableFF
            '
            resources.ApplyResources(Me.m_lblAvailableFF, "m_lblAvailableFF")
            Me.m_lblAvailableFF.Name = "m_lblAvailableFF"
            '
            'm_rbOpt1
            '
            resources.ApplyResources(Me.m_rbOpt1, "m_rbOpt1")
            Me.m_rbOpt1.Name = "m_rbOpt1"
            Me.m_rbOpt1.UseVisualStyleBackColor = True
            '
            'm_rbOpt2
            '
            resources.ApplyResources(Me.m_rbOpt2, "m_rbOpt2")
            Me.m_rbOpt2.Name = "m_rbOpt2"
            Me.m_rbOpt2.UseVisualStyleBackColor = True
            '
            'm_rbOpt4
            '
            resources.ApplyResources(Me.m_rbOpt4, "m_rbOpt4")
            Me.m_rbOpt4.Name = "m_rbOpt4"
            Me.m_rbOpt4.UseVisualStyleBackColor = True
            '
            'm_rbOpt3
            '
            resources.ApplyResources(Me.m_rbOpt3, "m_rbOpt3")
            Me.m_rbOpt3.Name = "m_rbOpt3"
            Me.m_rbOpt3.UseVisualStyleBackColor = True
            '
            'm_btnAdd
            '
            resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
            Me.m_btnAdd.Name = "m_btnAdd"
            Me.m_btnAdd.UseVisualStyleBackColor = True
            '
            'm_lvAppliedShapes
            '
            resources.ApplyResources(Me.m_lvAppliedShapes, "m_lvAppliedShapes")
            Me.m_lvAppliedShapes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.m_colhdrShape, Me.m_colhdrModifier})
            Me.m_lvAppliedShapes.FullRowSelect = True
            Me.m_lvAppliedShapes.HideSelection = False
            Me.m_lvAppliedShapes.Name = "m_lvAppliedShapes"
            Me.m_lvAppliedShapes.UseCompatibleStateImageBehavior = False
            Me.m_lvAppliedShapes.View = System.Windows.Forms.View.Details
            '
            'm_colhdrShape
            '
            resources.ApplyResources(Me.m_colhdrShape, "m_colhdrShape")
            '
            'm_colhdrModifier
            '
            resources.ApplyResources(Me.m_colhdrModifier, "m_colhdrModifier")
            '
            'm_lvAllShapes
            '
            resources.ApplyResources(Me.m_lvAllShapes, "m_lvAllShapes")
            Me.m_lvAllShapes.FullRowSelect = True
            Me.m_lvAllShapes.HideSelection = False
            Me.m_lvAllShapes.Name = "m_lvAllShapes"
            Me.m_lvAllShapes.UseCompatibleStateImageBehavior = False
            Me.m_lvAllShapes.View = System.Windows.Forms.View.List
            '
            'm_btnRemove
            '
            resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
            Me.m_btnRemove.Name = "m_btnRemove"
            Me.m_btnRemove.UseVisualStyleBackColor = True
            '
            'm_lblAppliedFF
            '
            resources.ApplyResources(Me.m_lblAppliedFF, "m_lblAppliedFF")
            Me.m_lblAppliedFF.Name = "m_lblAppliedFF"
            '
            'm_tlpApplications
            '
            resources.ApplyResources(Me.m_tlpApplications, "m_tlpApplications")
            Me.m_tlpApplications.Controls.Add(Me.m_rbOpt1, 0, 0)
            Me.m_tlpApplications.Controls.Add(Me.m_rbOpt4, 3, 0)
            Me.m_tlpApplications.Controls.Add(Me.m_rbOpt2, 1, 0)
            Me.m_tlpApplications.Controls.Add(Me.m_rbOpt3, 2, 0)
            Me.m_tlpApplications.Name = "m_tlpApplications"
            '
            'm_lblApplyTo
            '
            resources.ApplyResources(Me.m_lblApplyTo, "m_lblApplyTo")
            Me.m_lblApplyTo.Name = "m_lblApplyTo"
            '
            'm_tlpInfo
            '
            resources.ApplyResources(Me.m_tlpInfo, "m_tlpInfo")
            Me.m_tlpInfo.Controls.Add(Me.m_lblTarget, 1, 0)
            Me.m_tlpInfo.Controls.Add(Me.m_lblSource, 0, 0)
            Me.m_tlpInfo.Name = "m_tlpInfo"
            '
            'm_lblTarget
            '
            resources.ApplyResources(Me.m_lblTarget, "m_lblTarget")
            Me.m_lblTarget.Name = "m_lblTarget"
            '
            'm_lblSource
            '
            resources.ApplyResources(Me.m_lblSource, "m_lblSource")
            Me.m_lblSource.Name = "m_lblSource"
            '
            'dlgApplyPredPreyShape
            '
            Me.AcceptButton = Me.OK_Button
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.Cancel_Button
            Me.Controls.Add(Me.m_tlpInfo)
            Me.Controls.Add(Me.m_lblApplyTo)
            Me.Controls.Add(Me.m_tlpApplications)
            Me.Controls.Add(Me.Cancel_Button)
            Me.Controls.Add(Me.OK_Button)
            Me.Controls.Add(Me.m_lvAppliedShapes)
            Me.Controls.Add(Me.m_lvAllShapes)
            Me.Controls.Add(Me.m_btnRemove)
            Me.Controls.Add(Me.m_btnAdd)
            Me.Controls.Add(Me.m_lblAppliedFF)
            Me.Controls.Add(Me.m_lblAvailableFF)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgApplyPredPreyShape"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
            Me.m_tlpApplications.ResumeLayout(False)
            Me.m_tlpApplications.PerformLayout()
            Me.m_tlpInfo.ResumeLayout(False)
            Me.m_tlpInfo.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents OK_Button As System.Windows.Forms.Button
        Private WithEvents Cancel_Button As System.Windows.Forms.Button
        Private WithEvents m_btnAdd As System.Windows.Forms.Button
        Private WithEvents m_lvAppliedShapes As System.Windows.Forms.ListView
        Private WithEvents m_lvAllShapes As System.Windows.Forms.ListView
        Private WithEvents m_colhdrShape As System.Windows.Forms.ColumnHeader
        Private WithEvents m_colhdrModifier As System.Windows.Forms.ColumnHeader
        Private WithEvents m_btnRemove As System.Windows.Forms.Button
        Private WithEvents m_rbOpt1 As System.Windows.Forms.RadioButton
        Private WithEvents m_rbOpt4 As System.Windows.Forms.RadioButton
        Private WithEvents m_rbOpt3 As System.Windows.Forms.RadioButton
        Private WithEvents m_rbOpt2 As System.Windows.Forms.RadioButton
        Private WithEvents m_lblAvailableFF As System.Windows.Forms.Label
        Private WithEvents m_lblAppliedFF As System.Windows.Forms.Label
        Private WithEvents m_tlpApplications As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_lblApplyTo As System.Windows.Forms.Label
        Private WithEvents m_tlpInfo As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_lblTarget As System.Windows.Forms.Label
        Private WithEvents m_lblSource As System.Windows.Forms.Label

    End Class

End Namespace

