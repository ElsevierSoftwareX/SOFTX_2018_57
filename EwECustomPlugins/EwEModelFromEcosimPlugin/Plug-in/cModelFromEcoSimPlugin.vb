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
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in point for the 'Ecopath model from Ecosim' plug-in.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cModelFromEcosimPluginPoint
    Implements IUIContextPlugin
    Implements INavigationTreeItemPlugin
    Implements IEcosimPlugin
    Implements IEcosimRunInitializedPlugin
    Implements IEcosimEndTimestepPostPlugin
    Implements IEcosimRunCompletedPlugin
    Implements IMenuItemPlugin
    Implements IAutoSavePlugin

    ' ToDo: consider what month to generate the models for. Right now, the default is the first month of the year

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_ui As frmUI = Nothing
    Private m_core As cCore = Nothing
    Private m_data As cData = Nothing
    Private m_generator As cEcopathModelFromEcosim = Nothing
    Private m_bAutosaving As Boolean = False
    Private m_simdata As cEcosimDatastructures = Nothing

#End Region ' Private vars

#Region " Basic plug-in bits "

    Public ReadOnly Property Author() As String Implements EwEPlugin.IPlugin.Author
        Get
            Return "Ecopath International Initiative"
        End Get
    End Property

    Public ReadOnly Property Contact() As String Implements EwEPlugin.IPlugin.Contact
        Get
            Return "mailto:ewedevteam@gmail.com"
        End Get
    End Property

    Public ReadOnly Property Description() As String Implements EwEPlugin.IPlugin.Description
        Get
            Return ""
        End Get
    End Property

    Public Sub Initialize(ByVal core As Object) Implements EwEPlugin.IPlugin.Initialize

        Debug.Assert(Me.m_core Is Nothing)
        Debug.Assert(Me.m_data Is Nothing)

        Me.m_core = DirectCast(core, cCore)
        Me.m_data = New cData(Me.m_core)
        Me.m_generator = New cEcopathModelFromEcosim(Me.m_core)

    End Sub

    Public ReadOnly Property Name() As String Implements EwEPlugin.IPlugin.Name
        Get
            Return "ndSaveEcopathModelFromEcosim"
        End Get
    End Property

#End Region ' Basic plug-in bits

#Region " UI integration "

    Public Sub UIContext(ByVal uic As Object) _
        Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

    Public ReadOnly Property ControlImage() As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property ControlText() As String Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return My.Resources.CONTROL_TEXT
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText() As String _
        Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return "Generate Ecopath models from Ecosim time steps"
        End Get
    End Property

    Public Sub OnControlClick(ByVal sender As Object, _
                              ByVal e As System.EventArgs, _
                              ByRef frmPlugin As System.Windows.Forms.Form) _
                              Implements EwEPlugin.IGUIPlugin.OnControlClick
        If (Me.m_uic IsNot Nothing) Then
            frmPlugin = DirectCast(Me.UI, System.Windows.Forms.Form)
        End If
    End Sub

    Public ReadOnly Property NavigationTreeItemLocation() As String _
        Implements EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation
        Get
            Return "ndTimeDynamic|ndEcosimTools"
        End Get
    End Property

    Public ReadOnly Property MenuItemLocation As String _
        Implements EwEPlugin.IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuFile\ExportModel"
        End Get
    End Property

    Public ReadOnly Property EnabledState() As EwEUtils.Core.eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return eCoreExecutionState.EcosimLoaded
        End Get
    End Property

#End Region ' UI integration

#Region " Autosaving "

    Public Property AutoSave As Boolean Implements EwEPlugin.IAutoSavePlugin.AutoSave
        Get
            Return Me.m_data.Enabled
        End Get
        Set(value As Boolean)
            Me.m_data.Enabled = value
        End Set
    End Property

    Public Function AutoSaveName() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveName
        Return Me.ControlText
    End Function

    Public Function AutoSaveSubPath() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveOutputPath
        Return Me.m_data.OutputPath
    End Function

    Public Function AutoSaveType() As eAutosaveTypes _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveType
        Return Me.m_data.AutosaveType
    End Function

#End Region ' Autosaving 

#Region " Ecosim integration "

    Public Sub LoadEcosimScenario(dataSource As Object) _
        Implements EwEPlugin.IEcosimPlugin.LoadEcosimScenario

        Me.m_data.Clear()

        ' JS 08Apr13: Initialized data from settings here, not in UI. This ensures that data 
        '             is always up and fine, and prepares the logic to interact with the database

        Dim bac As cEcopathModelFromEcosim.eBACalcTypes = cEcopathModelFromEcosim.eBACalcTypes.FromEcosimYearsAverage
        If (My.Settings.BACalcMode >= 0) Then bac = DirectCast(My.Settings.BACalcMode, EwECore.cEcopathModelFromEcosim.eBACalcTypes)
        Me.m_data.BACalcMode = bac
        Me.m_data.NumYears = Me.m_core.nEcosimYears
        Me.m_data.EwEModelName = cFileUtils.ToValidFileName(Me.m_core.EwEModel.Name, False)
        Me.m_data.WPower = My.Settings.WPower
        Me.m_data.BAAverageYears = My.Settings.BANumYears

    End Sub

    Public Sub SaveEcosimScenario(dataSource As Object) _
        Implements EwEPlugin.IEcosimPlugin.SaveEcosimScenario

        ' Save settings
        My.Settings.BACalcMode = Me.m_data.BACalcMode
        'If String.Compare(Me.m_data.CustomOutputPath, Me.m_core.OutputPath, True) <> 0 Then
        '    My.Settings.OutputPath = Me.m_data.CustomOutputPath
        'Else
        '    My.Settings.OutputPath = ""
        'End If
        My.Settings.BANumYears = Me.m_data.NumYears
        My.Settings.WPower = Me.m_data.WPower
        My.Settings.Save()

    End Sub

    Public Sub CloseEcosimScenario() _
        Implements EwEPlugin.IEcosimPlugin.CloseEcosimScenario

        Me.m_data.Clear()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim is about to run. Generate the 
    ''' </summary>
    ''' <param name="EcosimDatastructures"></param>
    ''' -----------------------------------------------------------------------
    Public Sub EcosimRunInitialized(EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimRunInitializedPlugin.EcosimRunInitialized

        ' Use a static version of the enabled flag for the duration of an Ecosim run 
        Me.m_bAutosaving = Me.AutoSave
        Me.m_simdata = DirectCast(EcosimDatastructures, cEcosimDatastructures)

        If Me.m_bAutosaving Then
            Me.m_generator.InitRun(Me.m_data.OutputPath)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim has finished running: send generation message, if any
    ''' </summary>
    ''' <param name="EcosimDatastructures"></param>
    ''' -----------------------------------------------------------------------
    Public Sub EcosimRunCompleted(EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimRunCompletedPlugin.EcosimRunCompleted

        If Me.m_bAutosaving Then
            Me.m_generator.EndRun()
        End If

    End Sub

    Public Sub EcosimEndTimeStepPost(ByRef BiomassAtTimestep() As Single, _
                                     ByVal EcosimDatastructures As Object, _
                                     ByVal iTime As Integer, _
                                     ByVal Ecosimresults As Object) _
                                     Implements EwEPlugin.IEcosimEndTimestepPostPlugin.EcosimEndTimeStepPost

        If (Not Me.m_bAutosaving) Then Return

        Dim strModelName As String = ""
        Dim strModelPath As String = ""
        Dim DBCreated As eDatasourceAccessType = eDatasourceAccessType.Failed_Unknown

        Try

            Dim nStepsPerYear As Integer = Me.m_simdata.NumStepsPerYear
            Dim iStep As Integer = 1 + ((iTime - 1) Mod nStepsPerYear) ' Engine uses one-based time steps
            Dim iYear As Integer = CInt((iTime - 1) / nStepsPerYear) + 1 ' Engine uses one-based years

            If (iStep <> Me.m_data.OutputTimeStep) Then Return

            ' Is generator explicitly enabled and should a model be created for the current time step?
            If (Me.m_data.CreateModel(iYear)) Then

                ' #Yes: generate
                strModelName = Me.m_data.ModelName(iYear)
                strModelPath = Path.Combine(Me.m_data.OutputPath,
                                        Path.ChangeExtension(strModelName, cDataSourceFactory.GetDefaultExtension(Me.m_data.OutputFormat)))

                ' Go Jimmy
                cApplicationStatusNotifier.StartProgress(Me.m_core, cStringUtils.Localize(My.Resources.STATUS_GENERATING_MODEL, strModelName))

                Try

                    Select Case Me.m_generator.SaveModel(cFileUtils.ToValidFileName(strModelPath, True), strModelName, iTime,
                                                     Me.m_data.BACalcMode, Me.m_data.BAAverageYears, Me.m_data.WPower)
                        Case eDatasourceAccessType.Created, eDatasourceAccessType.Opened, eDatasourceAccessType.Success
                            Me.m_generator.LogStatus(cStringUtils.Localize(My.Resources.STATUS_SAVE_SUCCESS, iTime, strModelName), eStatusFlags.OK)
                        Case eDatasourceAccessType.Failed_CannotSave
                            Me.m_generator.LogStatus(cStringUtils.Localize(My.Resources.STATUS_SAVE_ERROR_NOACCESS, strModelPath), eStatusFlags.ErrorEncountered)
                        Case eDatasourceAccessType.Failed_OSUnsupported
                            Me.m_generator.LogStatus(cStringUtils.Localize(My.Resources.STATUS_SAVE_ERROR_NODRIVERS, strModelPath), eStatusFlags.ErrorEncountered)
                        Case Else
                            Me.m_generator.LogStatus(cStringUtils.Localize(My.Resources.STATUS_SAVE_ERROR_SEELOG, strModelPath), eStatusFlags.ErrorEncountered)
                    End Select
                Catch ex As Exception

                End Try

                cApplicationStatusNotifier.EndProgress(Me.m_core)

            End If

        Catch ex As Exception

        End Try
    End Sub

#End Region ' Ecosim integration

#Region " Internals "

    Private Function UI() As frmUI
        If Not Me.HasUI Then
            Me.m_ui = New frmUI(Me.m_data)
            Me.m_ui.UIContext = Me.m_uic
        End If
        Return Me.m_ui
    End Function

    Private Function HasUI() As Boolean
        If Me.m_ui IsNot Nothing Then
            Return Not Me.m_ui.IsDisposed
        End If
        Return False
    End Function

#End Region ' Internals

End Class
