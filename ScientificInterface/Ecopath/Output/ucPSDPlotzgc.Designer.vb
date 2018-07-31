<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucPSDPlotzgc
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.components = New System.ComponentModel.Container
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
        Me.zgcZedGraphCntl = New ZedGraph.ZedGraphControl
        Me.llbGroups = New ScientificInterfaceShared.Controls.LegendListBox
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.zgcZedGraphCntl)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.llbGroups)
        Me.SplitContainer1.Size = New System.Drawing.Size(786, 488)
        Me.SplitContainer1.SplitterDistance = 576
        Me.SplitContainer1.TabIndex = 0
        '
        'zgcZedGraphCntl
        '
        Me.zgcZedGraphCntl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.zgcZedGraphCntl.Location = New System.Drawing.Point(0, 0)
        Me.zgcZedGraphCntl.Name = "zgcZedGraphCntl"
        Me.zgcZedGraphCntl.ScrollGrace = 0
        Me.zgcZedGraphCntl.ScrollMaxX = 0
        Me.zgcZedGraphCntl.ScrollMaxY = 0
        Me.zgcZedGraphCntl.ScrollMaxY2 = 0
        Me.zgcZedGraphCntl.ScrollMinX = 0
        Me.zgcZedGraphCntl.ScrollMinY = 0
        Me.zgcZedGraphCntl.ScrollMinY2 = 0
        Me.zgcZedGraphCntl.Size = New System.Drawing.Size(576, 488)
        Me.zgcZedGraphCntl.TabIndex = 0
        '
        'llbGroups
        '
        Me.llbGroups.Dock = System.Windows.Forms.DockStyle.Fill
        Me.llbGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.llbGroups.FormattingEnabled = True
        Me.llbGroups.Location = New System.Drawing.Point(0, 0)
        Me.llbGroups.Name = "llbGroups"
        Me.llbGroups.Size = New System.Drawing.Size(206, 485)
        Me.llbGroups.TabIndex = 0
        '
        'ucGrowthPlotzgc
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "ucGrowthPlotzgc"
        Me.Size = New System.Drawing.Size(786, 488)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents zgcZedGraphCntl As ZedGraph.ZedGraphControl
    Friend WithEvents llbGroups As ScientificInterfaceShared.Controls.LegendListBox

End Class
