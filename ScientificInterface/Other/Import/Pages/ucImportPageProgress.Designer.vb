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

Namespace Import

    Partial Class ucImportPageProgress
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.m_pb = New System.Windows.Forms.ProgressBar()
            Me.m_lbSummary = New System.Windows.Forms.ListBox()
            Me.m_hdrModels = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.SuspendLayout()
            '
            'm_pb
            '
            Me.m_pb.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_pb.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_pb.Location = New System.Drawing.Point(0, 277)
            Me.m_pb.Margin = New System.Windows.Forms.Padding(0, 3, 0, 0)
            Me.m_pb.Name = "m_pb"
            Me.m_pb.Size = New System.Drawing.Size(510, 23)
            Me.m_pb.Style = System.Windows.Forms.ProgressBarStyle.Continuous
            Me.m_pb.TabIndex = 9
            '
            'm_lbSummary
            '
            Me.m_lbSummary.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_lbSummary.Enabled = False
            Me.m_lbSummary.FormattingEnabled = True
            Me.m_lbSummary.Location = New System.Drawing.Point(0, 25)
            Me.m_lbSummary.Name = "m_lbSummary"
            Me.m_lbSummary.Size = New System.Drawing.Size(510, 238)
            Me.m_lbSummary.TabIndex = 11
            '
            'm_hdrModels
            '
            Me.m_hdrModels.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrModels.CanCollapseParent = False
            Me.m_hdrModels.CollapsedParentHeight = 0
            Me.m_hdrModels.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrModels.IsCollapsed = False
            Me.m_hdrModels.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrModels.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrModels.Name = "m_hdrModels"
            Me.m_hdrModels.Size = New System.Drawing.Size(510, 18)
            Me.m_hdrModels.TabIndex = 12
            Me.m_hdrModels.Text = "Progress"
            Me.m_hdrModels.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'ucImportPageProgress
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_hdrModels)
            Me.Controls.Add(Me.m_lbSummary)
            Me.Controls.Add(Me.m_pb)
            Me.Name = "ucImportPageProgress"
            Me.Size = New System.Drawing.Size(510, 300)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_pb As System.Windows.Forms.ProgressBar
        Private WithEvents m_lbSummary As System.Windows.Forms.ListBox
        Private WithEvents m_hdrModels As ScientificInterfaceShared.Controls.cEwEHeaderLabel

    End Class

End Namespace