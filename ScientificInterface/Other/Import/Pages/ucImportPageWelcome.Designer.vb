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

    Partial Class ucImportPageWelcome
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
            Me.m_pbWelcome = New System.Windows.Forms.PictureBox()
            Me.m_lblWelcomeInstructions = New System.Windows.Forms.Label()
            Me.m_lblNext = New System.Windows.Forms.Label()
            CType(Me.m_pbWelcome, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_pbWelcome
            '
            Me.m_pbWelcome.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.m_pbWelcome.BackColor = System.Drawing.Color.White
            Me.m_pbWelcome.Image = Global.ScientificInterface.My.Resources.Resources.logo_EWE5_caption
            Me.m_pbWelcome.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_pbWelcome.Location = New System.Drawing.Point(9, 9)
            Me.m_pbWelcome.Margin = New System.Windows.Forms.Padding(0)
            Me.m_pbWelcome.Name = "m_pbWelcome"
            Me.m_pbWelcome.Size = New System.Drawing.Size(146, 280)
            Me.m_pbWelcome.TabIndex = 7
            Me.m_pbWelcome.TabStop = False
            '
            'm_lblWelcomeInstructions
            '
            Me.m_lblWelcomeInstructions.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_lblWelcomeInstructions.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_lblWelcomeInstructions.Location = New System.Drawing.Point(173, 214)
            Me.m_lblWelcomeInstructions.Name = "m_lblWelcomeInstructions"
            Me.m_lblWelcomeInstructions.Size = New System.Drawing.Size(290, 46)
            Me.m_lblWelcomeInstructions.TabIndex = 8
            Me.m_lblWelcomeInstructions.Text = "The Ecopath data that you selected contains one or more models that must be conve" &
        "rted to the format used by this version of EwE."
            '
            'm_lblNext
            '
            Me.m_lblNext.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_lblNext.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_lblNext.Location = New System.Drawing.Point(173, 264)
            Me.m_lblNext.Name = "m_lblNext"
            Me.m_lblNext.Size = New System.Drawing.Size(256, 25)
            Me.m_lblNext.TabIndex = 8
            Me.m_lblNext.Text = "Click Next to proceed."
            Me.m_lblNext.TextAlign = System.Drawing.ContentAlignment.BottomLeft
            '
            'ucImportPageWelcome
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
            Me.Controls.Add(Me.m_pbWelcome)
            Me.Controls.Add(Me.m_lblNext)
            Me.Controls.Add(Me.m_lblWelcomeInstructions)
            Me.Name = "ucImportPageWelcome"
            Me.Size = New System.Drawing.Size(510, 300)
            CType(Me.m_pbWelcome, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_pbWelcome As System.Windows.Forms.PictureBox
        Private WithEvents m_lblWelcomeInstructions As System.Windows.Forms.Label
        Private WithEvents m_lblNext As System.Windows.Forms.Label

    End Class

End Namespace