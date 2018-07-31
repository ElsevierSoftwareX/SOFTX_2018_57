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

Namespace Other

    Partial Class ucAutosaveOption
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucAutosaveOption))
            Me.m_cbOption = New System.Windows.Forms.CheckBox()
            Me.m_lblPath = New System.Windows.Forms.Label()
            Me.m_btnVisitFolder = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'm_cbOption
            '
            resources.ApplyResources(Me.m_cbOption, "m_cbOption")
            Me.m_cbOption.Name = "m_cbOption"
            Me.m_cbOption.UseVisualStyleBackColor = True
            '
            'm_lblPath
            '
            resources.ApplyResources(Me.m_lblPath, "m_lblPath")
            Me.m_lblPath.Name = "m_lblPath"
            '
            'm_btnVisitFolder
            '
            resources.ApplyResources(Me.m_btnVisitFolder, "m_btnVisitFolder")
            Me.m_btnVisitFolder.Name = "m_btnVisitFolder"
            Me.m_btnVisitFolder.UseVisualStyleBackColor = True
            '
            'ucAutosaveOption
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.m_btnVisitFolder)
            Me.Controls.Add(Me.m_lblPath)
            Me.Controls.Add(Me.m_cbOption)
            Me.Name = "ucAutosaveOption"
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_cbOption As System.Windows.Forms.CheckBox
        Private WithEvents m_lblPath As System.Windows.Forms.Label
        Private WithEvents m_btnVisitFolder As System.Windows.Forms.Button

    End Class

End Namespace
