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

    Partial Class dlgImportDatabase
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgImportDatabase))
            Me.m_sep = New ScientificInterfaceShared.Controls.ucFormSeparator()
            Me.m_navigator = New ScientificInterfaceShared.Controls.Wizard.ucWizardNavigation()
            Me.m_plWizardContent = New System.Windows.Forms.Panel()
            Me.SuspendLayout()
            '
            'm_sep
            '
            resources.ApplyResources(Me.m_sep, "m_sep")
            Me.m_sep.Horizontal = True
            Me.m_sep.Name = "m_sep"
            '
            'm_navigator
            '
            resources.ApplyResources(Me.m_navigator, "m_navigator")
            Me.m_navigator.Name = "m_navigator"
            '
            'm_plWizardContent
            '
            resources.ApplyResources(Me.m_plWizardContent, "m_plWizardContent")
            Me.m_plWizardContent.Name = "m_plWizardContent"
            '
            'dlgImportDatabase
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_plWizardContent)
            Me.Controls.Add(Me.m_navigator)
            Me.Controls.Add(Me.m_sep)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
            Me.Name = "dlgImportDatabase"
            Me.ShowInTaskbar = False
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_sep As ScientificInterfaceShared.Controls.ucFormSeparator
        Private WithEvents m_navigator As ScientificInterfaceShared.Controls.Wizard.ucWizardNavigation
        Private WithEvents m_plWizardContent As System.Windows.Forms.Panel
    End Class

End Namespace