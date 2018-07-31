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

Imports System.IO
Imports EwECore
Imports EwECore.DataSources
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.Wizard

#End Region ' Imports

Namespace Import

    ''' =======================================================================
    ''' <summary>
    ''' Wizard that guides users through the process of importing models of
    ''' previous EwE versions.
    ''' </summary>
    ''' =======================================================================
    Public Class cImportWizard
        Inherits cWizard

#Region " Private bits "

        ''' <summary>The actual importer.</summary>
        Private m_dbImp As IModelImporter = Nothing
        ''' <summary>A setting for each external model to import.</summary>
        Private m_lImportSettings As New List(Of cImportSettings)
        ''' <summary>Folder to place imported models into.</summary>
        Private m_strOutputFolder As String = ""
        ''' <summary>Format to output databases into.</summary>
        Private m_outputFormat As eDataSourceTypes
        ''' <summary>Database being opened</summary>
        Private m_strDatabase As String = ""
        ''' <summary>Last imported file name.</summary>
        Private m_strFileName As String = ""

#End Region ' Private bits

#Region " Helper classes "

        ''' ===================================================================
        ''' <summary>
        ''' Helper class, maintains import settings for a single model.
        ''' </summary>
        ''' ===================================================================
        Public Class cImportSettings

#Region " Privates vars "

            ''' <summary>External model info.</summary>
            Private m_mi As cExternalModelInfo = Nothing
            ''' <summary>Flag stating whether this external model should be imported.</summary>
            Private m_bImport As Boolean = False
            ''' <summary>EwE6 name of the model to import into.</summary>
            Private m_strEwE6Name As String = ""
            ''' <summary>Path to the import log file once an import is completed.</summary>
            Private m_strLogFile As String = ""

#End Region ' Privates vars

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Create a new import setting for an external model.
            ''' </summary>
            ''' <param name="mi">The <see cref="cExternalModelInfo">importable model</see>
            ''' to create import settings for.</param>
            ''' -----------------------------------------------------------------------
            Public Sub New(ByVal mi As cExternalModelInfo)
                Me.m_mi = mi
                Me.m_bImport = False
                Me.m_strEwE6Name = mi.Name
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cExternalModelInfo">importable model 
            ''' information</see> associated with this import setting.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public ReadOnly Property ModelInfo() As cExternalModelInfo
                Get
                    Return Me.m_mi
                End Get
            End Property

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether this external model should be imported.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Property SelectedForImport() As Boolean
                Get
                    Return Me.m_bImport
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bImport = value
                End Set
            End Property

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the name of the EwE6 model to import into.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Property EwE6ModelName() As String
                Get
                    Return Me.m_strEwE6Name
                End Get
                Set(ByVal value As String)
                    Me.m_strEwE6Name = Me.ToEwE6ModelName(value)
                End Set
            End Property

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the import log file.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Property LogFile() As String
                Get
                    Return Me.m_strLogFile
                End Get
                Set(ByVal value As String)
                    Me.m_strLogFile = value
                End Set
            End Property

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Convert a external model name to an EwE6 model name.
            ''' </summary>
            ''' <param name="strexternalModel"></param>
            ''' <returns></returns>
            ''' -----------------------------------------------------------------------
            Private Function ToEwE6ModelName(ByVal strexternalModel As String) As String
                Return cFileUtils.ToValidFileName(strexternalModel, False)
            End Function

        End Class

#End Region ' Helper classes

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Construct a new import wizard.
        ''' </summary>
        ''' <param name="uic">The UI context to operate on.</param>
        ''' <param name="strSource">External model to import.</param>
        ''' <param name="parent">The form hosting the wizard UI.</param>
        ''' <param name="content">The panel where this wizard can display its pages.</param>
        ''' <param name="nav">The navigation that controls this wizard.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal strSource As String, _
                       ByVal parent As Form, _
                       ByVal content As Panel, _
                       ByVal nav As IWizardNavigation)

            MyBase.New(uic, parent, content, nav)

            ' Sanity checks
            Debug.Assert(uic IsNot Nothing)
            Debug.Assert(uic.Core IsNot Nothing)

            ' Hook up with data
            Me.m_dbImp = cModelImporterFactory.GetModelImporter(Core, strSource, uic.Core.PluginManager)
            If Me.m_dbImp.Open(strSource) Then
                Me.m_strDatabase = strSource
                Me.m_strOutputFolder = Path.GetDirectoryName(strSource)

                ' Always make sure there is a path
                If (String.IsNullOrWhiteSpace(Me.m_strOutputFolder)) Then
                    ' Not sure how this works on MONO...
                    Me.m_strOutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                End If

                ' Prepare import settings
                For Each mi As cExternalModelInfo In Me.m_dbImp.Models
                    Dim imp As New cImportSettings(mi)
                    imp.SelectedForImport = (Me.m_dbImp.Models.Count = 1)
                    Me.m_lImportSettings.Add(imp)
                Next

                Me.m_dbImp.Close()
                ' Add pages
                Me.AddPage(GetType(ucImportPageWelcome))
                Me.AddPage(GetType(ucImportPageModels))
                Me.AddPage(GetType(ucImportPageProgress))
            Else
                Me.AddPage(GetType(ucImportPageError))
            End If
 
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="cImportSettings">import settings</see> for
        ''' the current selected external database.
        ''' </summary>
        ''' <returns>
        ''' An array of <see cref="cImportSettings">import settings</see>.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function ImportSettings() As cImportSettings()
            Return Me.m_lImportSettings.ToArray
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the database being opened.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Database() As String
            Get
                Return Me.m_strDatabase
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the database imported if only one database was imported.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Filename() As String
            Get
                Return Me.m_strFileName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the output folder for placing imported models.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property OutputFolder() As String
            Get
                Return Me.m_strOutputFolder
            End Get
            Set(ByVal value As String)
                Me.m_strOutputFolder = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the output database format.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property OutputFormat() As eDataSourceTypes
            Get
                Return Me.m_outputFormat
            End Get
            Set(ByVal value As eDataSourceTypes)
                Me.m_outputFormat = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the wizard has a valid output path.
        ''' </summary>
        ''' <returns>
        ''' True if the wizard has a valid output path.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function HasValidOutputPath() As Boolean

            Dim di As DirectoryInfo = Nothing
            Try
                di = New DirectoryInfo(Me.m_strOutputFolder)
            Catch ex As Exception
                Return False
            End Try
            ' ToDo: include checking of directory write access?
            Return di.Exists

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the wizard has at least ONE model selected for import.
        ''' </summary>
        ''' <returns>
        ''' True if the wizard has at least ONE model selected for import.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function HasModelSelectedForImport() As Boolean

            For Each setting As cImportWizard.cImportSettings In Me.ImportSettings
                If setting.SelectedForImport Then Return True
            Next

            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform a model import.
        ''' </summary>
        ''' <param name="setting">The <see cref="cImportSettings">model</see>
        ''' to import.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Import(ByVal setting As cImportSettings) As Boolean

            Dim appl As frmEwE6 = frmEwE6.GetInstance()
            Dim db As cEwEDatabase = Nothing
            Dim strModel As String = Me.EwE6ModelName(setting, Me.OutputFormat)
            Dim strLogFile As String = ""
            Dim bSucces As Boolean = False

            ' Only import models selected for import
            If (Not setting.SelectedForImport) Then Return bSucces

            ' Request a database to import into
            db = appl.CreateEcopathModel(strModel, setting.ModelInfo.ID, Me.OutputFormat)

            ' Able to create target model?
            If (db IsNot Nothing) Then
                ' #Yes: Open target model
                db.Open(strModel)
                ' Able to import?
                If Me.m_dbImp.Import(setting.ModelInfo, db, strLogFile) Then
                    ' #Yes: remember last imported model file
                    Me.m_strFileName = strModel
                    ' Succes
                    bSucces = True
                End If
                ' Update log file
                setting.LogFile = strLogFile

                ' Clean up
                db.Close()
            End If

            ' Report succes
            Return bSucces

        End Function

#End Region ' Public access

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a valid EwE6 model path for an import setting and a given 
        ''' output format.
        ''' </summary>
        ''' <param name="setting">The setting to create an EwE6 model path for.</param>
        ''' <returns>A valid EwE6 model path for an import setting.</returns>
        ''' -------------------------------------------------------------------
        Private Function EwE6ModelName(ByVal setting As cImportSettings, _
                                       ByVal format As eDataSourceTypes) As String
            Dim strModel As String = Path.Combine(Me.m_strOutputFolder, setting.EwE6ModelName)
            strModel += cDataSourceFactory.GetDefaultExtension(format)
            Return cFileUtils.ToValidFileName(strModel, True)

        End Function

#End Region ' Internals

    End Class

End Namespace
