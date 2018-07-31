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

#Region " Imports "

Option Strict On
Imports EwECore.Database
Imports ScientificInterfaceShared.Controls.Wizard
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwECore.DataSources
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Database

#End Region ' Imports

Namespace Import

    ''' =======================================================================
    ''' <summary>
    ''' Import wizard model selection page.
    ''' </summary>
    ''' =======================================================================
    Public Class ucImportPageModels
        Implements IWizardPage

#Region " Private helper class "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class, reflects an EwE database type in the list candidate
        ''' import formats.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class cDatabaseTypeItem

            Private m_dst As eDataSourceTypes = eDataSourceTypes.NotSet

            Public Sub New(ByVal dst As eDataSourceTypes)
                Me.m_dst = dst
            End Sub

            Public ReadOnly Property DataSourceType() As eDataSourceTypes
                Get
                    Return Me.m_dst
                End Get
            End Property

            Public Overrides Function ToString() As String
                Dim strFileFilter As String = ""
                Select Case Me.m_dst
                    Case eDataSourceTypes.Access2007
                        strFileFilter = SharedResources.FILEFILTER_SAVE_ACCDB
                    Case eDataSourceTypes.Access2003
                        strFileFilter = SharedResources.FILEFILTER_SAVE_MDB
                    Case Else
                        Debug.Assert(False)
                End Select
                Return strFileFilter.Split("|"c)(0)
            End Function

        End Class

#End Region ' Private helper class

#Region " Private vars "

        ''' <summary>The attached wizard.</summary>
        Private m_wizard As cImportWizard = Nothing
        ''' <summary>UI context.</summary>
        Private m_uic As cUIContext = Nothing

#End Region ' Private vars

#Region " Interface "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the model selection page with the wizard content.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Init(ByVal wizard As cWizard, ByVal uic As cUIContext) _
             Implements IWizardPage.Init

            ' Sanity checks
            Debug.Assert(TypeOf (wizard) Is cImportWizard)

            ' Remember wizard
            Me.m_wizard = DirectCast(wizard, cImportWizard)
            Me.m_uic = uic

            ' Initialize output folder path
            Me.m_tbxOutputFolder.Text = Me.m_wizard.OutputFolder

            ' Initialize the grid
            Me.m_grid.UIContext = uic
            Me.m_grid.Init(Me.m_wizard)

            AddHandler Me.m_grid.OnSelectionChanged, AddressOf OnModelSelectionChanged

            ' Initialize the database targets combo box
            Me.InitDatabaseFormatsCombo()

            Me.m_lblComments.Text = ""
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close the model selection page.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Close() _
              Implements IWizardPage.Close

            ' Clean-up
            RemoveHandler Me.m_grid.OnSelectionChanged, AddressOf OnModelSelectionChanged
            Me.m_wizard = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the model selection page is busy.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsBusy() As Boolean _
              Implements IWizardPage.IsBusy
            ' Page does not have a 'busy' mode
            Return False
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the model selection page allows the wizard to 
        ''' navigate backward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function AllowNavBack() As Boolean _
            Implements IWizardPage.AllowNavBack
            ' No restrictions
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the model selection page allows the wizard to 
        ''' navigate forward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function AllowNavForward() As Boolean _
            Implements IWizardPage.AllowNavForward

            ' Can only navigate forward if the current page settings allow 
            ' the wizard to import.
            Return Me.m_wizard.HasModelSelectedForImport() And _
                   Me.m_wizard.HasValidOutputPath()

        End Function

#End Region ' Interface

#Region " Events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Grid reports a user change.
        ''' </summary>
        ''' <param name="grid"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnGridEdited(ByVal grid As cImportGrid) _
            Handles m_grid.OnEdited
            Me.m_wizard.PageChanged(Me)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, called when the user wants to browse for an output 
        ''' folder.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnBrowsePath(ByVal sender As System.Object, _
                                  ByVal e As EventArgs) _
             Handles m_btnBrowse.Click

            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmd As cDirectoryOpenCommand = DirectCast(cmdh.GetCommand(cDirectoryOpenCommand.COMMAND_NAME), cDirectoryOpenCommand)

            cmd.Invoke()

            If cmd.Result = DialogResult.OK Then
                Me.m_tbxOutputFolder.Text = cmd.Directory
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, called when the output folder has been modified.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnOutputPathChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tbxOutputFolder.TextChanged
            Me.m_wizard.OutputFolder = Me.m_tbxOutputFolder.Text
            Me.m_wizard.PageChanged(Me)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, called when the output format has been modified.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnDBFormatChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_cmbDatabaseFormat.SelectedIndexChanged
            Me.m_wizard.OutputFormat = DirectCast(Me.m_cmbDatabaseFormat.SelectedItem, cDatabaseTypeItem).DataSourceType
            Me.m_wizard.PageChanged(Me)
        End Sub

        Private Sub OnModelSelectionChanged()
            Dim inf As cExternalModelInfo = Me.m_grid.SelectedModelInfo
            If inf Is Nothing Then
                Me.m_lblComments.Text = ""
            Else
                Me.m_lblComments.Text = inf.Description
            End If
        End Sub

#End Region ' Events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub InitDatabaseFormatsCombo()

            Me.m_cmbDatabaseFormat.Items.Clear()
            If cDataSourceFactory.IsOSSupported(eDataSourceTypes.Access2007) Then
                Me.m_cmbDatabaseFormat.Items.Add(New cDatabaseTypeItem(eDataSourceTypes.Access2007))
            End If
            If cDataSourceFactory.IsOSSupported(eDataSourceTypes.Access2003) Then
                Me.m_cmbDatabaseFormat.Items.Add(New cDatabaseTypeItem(eDataSourceTypes.Access2003))
            End If

            Me.m_cmbDatabaseFormat.SelectedIndex = 0

        End Sub

#End Region ' Internals

    End Class

End Namespace
