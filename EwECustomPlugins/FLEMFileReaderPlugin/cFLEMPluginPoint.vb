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

Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core

#End Region ' Imports

Public Class cFLEMPluginPoint
    Implements EwEPlugin.ICorePlugin
    Implements EwEPlugin.IGUIPlugin
    Implements EwEPlugin.INavigationTreeItemPlugin
    Implements EwEPlugin.IUIContextPlugin
    Implements EwEPlugin.IEcospaceInitRunCompletedPlugin
    Implements EwEPlugin.IEcospaceBeginTimestepPlugin
    Implements EwEPlugin.IEcospaceRunCompletedPlugin

#Region "Public data"

    Public Property ForcePPSalinity As Boolean
    Public Property VaryHabCapWithCultch As Boolean
    Public Property ForceFile As String
    Public Property HabCapModGroup As Integer
    Public Property UsePPModifier As Boolean
    Private m_core As EwECore.cCore

#End Region 'Public data

#Region "Private data"

    Private Nactive As Integer 'number of active (depth>0) cells in forcing data file for each month
    Private ForceData(,,) As Single 'forcing input data (rel pp, salinity in Flem file)
    Private CellRatio As Single 'ratio of ecospace to physical model cell length (km/km)
    Private NrowForce As Integer, NcolForce As Integer  'map dimensions for Flem physical model forcing data
    Private FileNumber As Integer
    Private orgHabCap(,,) As Single
    Private orgRelPP(,) As Single

    Private PathData As cEcopathDataStructures
    Private SimData As cEcosimDatastructures
    Private SpaceData As cEcospaceDataStructures

#Region "Plugin stuff"

    Private Property Context As ScientificInterfaceShared.Controls.cUIContext
    Private m_frmInterface As frmFLEMReader

#End Region 'Plugin stuff


#Region "Construction"

    Public Sub New()

        Try
            Me.ForcePPSalinity = My.Settings.ForcePPSalinity
            Me.ForceFile = My.Settings.ForceFile
            Me.UsePPModifier = My.Settings.UsePPModifier

            Me.HabCapModGroup = 6 'Clutch
        Catch ex As Exception
            cLog.Write(ex, "cFlemPluginPoint.New()")
        End Try

    End Sub

#End Region

#End Region 'Private data

#Region "FLEM file reading and data forcing"


    ''' <summary>
    ''' Copy the salinity modifiers from the Ecosim into Ecospace spatial salinity modifiers
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitSalinityFromEcosim()

        ' ***************************************************************
        ' THIS NEEDS REWORKING WITH DATABASE UPDATES 6.5.0024 to 6.5.0027

        'If ForcePPSalinity = False Then
        '    'Turn OFF the Spatial fields
        '    SpaceData.SpatialFieldsInUse = False
        '    SpaceData.nSpatialFields = 0
        '    Return
        'End If

        ''Turn on the Spatial fields
        'SpaceData.SpatialFieldsInUse = True

        ''In Ecosim the Salinity Modifiers are stored in the first index
        ''Ecosim also contains fields for Temperature (in the second index) and a bunch of place holders that are empty
        'SpaceData.nSpatialFields = 1

        ''Redim SpatialFields the default size will be wrong
        'ReDim SpaceData.SpatialField(SpaceData.InRow, SpaceData.InCol, SpaceData.NGroups)
        'ReDim SpaceData.SpatialFieldOptimum(SpaceData.NGroups, SpaceData.nSpatialFields)
        'ReDim SpaceData.SpatialFieldStdLeft(SpaceData.NGroups, SpaceData.nSpatialFields)
        'ReDim SpaceData.SpatialFieldStdRight(SpaceData.NGroups, SpaceData.nSpatialFields)

        ''Copy salinity modifiers from the Ecosim variables into the equivalent Ecospace variables
        'For igrp As Integer = 1 To Me.SpaceData.NGroups
        '    'In Ecosim salintiy modifiers are stored in the first index
        '    SpaceData.SpatialFieldOptimum(igrp, 1) = Me.SimData.EnvResponseOpt(1, igrp)
        '    SpaceData.SpatialFieldStdLeft(igrp, 1) = Me.SimData.EnvResponseSdLeft(1, igrp)
        '    SpaceData.SpatialFieldStdRight(igrp, 1) = Me.SimData.EnvResponseSdRight(1, igrp)
        'Next

        ' ***************************************************************

    End Sub


    Private Sub InitFLEMFiles()
        'THE FOLLOWING CODE GOES BEFORE THE TIME LOOP IN FINDSPATIALEQUILIBRIUM
        Dim iForce As Integer, jForce As Integer 'indices for forcing map cells
        Dim CellLengthForce As Single 'cell length for forcing data (km)

        If ForcePPSalinity = False Then Return

        'Make a copy of the original HapCap and RelPP array so we can restore then after a run
        ReDim orgHabCap(SpaceData.InRow + 1, SpaceData.InCol + 1, SpaceData.NGroups)
        ReDim orgRelPP(SpaceData.InRow + 1, SpaceData.InCol + 1)
        'Array.Copy(SpaceData.HabCap, Me.orgHabCap, Me.orgHabCap.Length)
        'Array.Copy(SpaceData.RelPP, Me.orgRelPP, Me.orgRelPP.Length)

        FileNumber = FreeFile()

        If ForceFile <> "" Then
            FileOpen(FileNumber, ForceFile, OpenMode.Input)
            Input(FileNumber, Nactive)
            Input(FileNumber, NrowForce)
            Input(FileNumber, NcolForce)
            Input(FileNumber, CellLengthForce)
            ReDim ForceData(NrowForce + 1, NcolForce + 1, 2)
            For iForce = 1 To NrowForce + 1  'set default values for all the forcing array cells
                For jForce = 1 To NcolForce + 1
                    ForceData(iForce, jForce, 1) = 1  'default relative primary productivity
                    ForceData(iForce, jForce, 2) = 35  'default salinity
                Next
            Next
            CellRatio = SpaceData.CellLength / CellLengthForce
        End If

    End Sub


    Private Sub EcospaceTimeStep(ByVal iTime As Integer)
        'THE FOLLOWING CODE GOES INSIDE THE TIME LOOP IN FINDSPATIALEQUILIBRIUM NEAR THE TOP JUST AFTER ITT IS CALCULATED
        Dim iForce As Integer, jForce As Integer 'indices for forcing map cells
        Dim Bscale As Single

        If ForcePPSalinity Then
            If ForceFile <> "" Then   'read forcing data for this time step

                If EOF(FileNumber) Then  'close and reopn the forcefile to read data over again
                    FileClose(FileNumber)
                    FileOpen(FileNumber, ForceFile, OpenMode.Input)
                    LineInput(FileNumber) 'skip reading the map size information if this is second round
                End If

                For irec As Integer = 1 To Nactive 'read each of the forcing cell observations for this step
                    Input(FileNumber, iForce)
                    Input(FileNumber, jForce)
                    Input(FileNumber, ForceData(iForce, jForce, 1))
                    Input(FileNumber, ForceData(iForce, jForce, 2))
                Next

                'now have the forcing data for this month, put into forcing arrays for the ecospace map
                For i = 1 To SpaceData.InRow
                    For j = 1 To SpaceData.InCol
                        iForce = 1 + Int(CellRatio * i - 0.01)  'calculate forcing data cell row for this ecospace cell
                        jForce = 1 + Int(CellRatio * j - 0.01)  'calculate forcing data cell col for this ecospace cell
                        If iForce < 1 Then iForce = 1
                        If iForce > NrowForce Then iForce = NrowForce
                        If jForce < 1 Then jForce = 1
                        If jForce > NcolForce Then jForce = NcolForce

                        ' ***************************************************************
                        ' THIS NEEDS REWORKING WITH DATABASE UPDATES 6.5.0024 to 6.5.0027

                        ''Load salinity forcing into all the groups
                        ''Apply a modifier to a group by changing its Salinity Tolerance Modifier in the Ecosim>Group info dialogue
                        'For igrp As Integer = 1 To m_core.nGroups
                        '    SpaceData.SpatialField(i, j, igrp) = ForceData(iForce, jForce, 2)
                        'Next

                        ' ***************************************************************

                        If Me.UsePPModifier Then
                            'reduce the strong Flem nutrient effect here by using lower mean, power
                            SpaceData.RelPP(i, j) = (ForceData(iForce, jForce, 1) - 0.5) ^ 0.5
                        Else
                            'No modifier, use pp as it appears in the .nuo file
                            SpaceData.RelPP(i, j) = ForceData(iForce, jForce, 1)
                        End If
                    Next
                Next

            End If ' ForceFile <> "" 
        End If

        '************modify habcap for oysters using culth biomass (group 6)
        If VaryHabCapWithCultch Then
            Bscale = SimData.StartBiomass(Me.HabCapModGroup) * Me.SpaceData.nWaterCells / Me.SpaceData.TotHabCap(Me.HabCapModGroup)
            For i = 1 To SpaceData.InRow
                For j = 1 To SpaceData.InCol
                    For ig As Integer = 1 To 4
                        SpaceData.HabCap(ig)(i, j) = Math.Min(1, 0.8 * SpaceData.HabCap(ig)(i, j) + 0.2 * SpaceData.Bcell(i, j, Me.HabCapModGroup) / Bscale)
                    Next
                Next
            Next
        End If

        ''NOTE THERE NEEDS TO BE A CLOSEFILE(6) LINE AFTER THE END OF THE FINDSPATIALEQUILIBRIUM TIME LOOP
        'this is done in EcospaceRunCompleted
    End Sub




    ''' <summary>
    ''' Called when Ecospace has completed all its initialization and it about to start the time loop
    ''' </summary>
    ''' <param name="EcospaceDatastructures"></param>
    ''' <remarks></remarks>
    Public Sub EcospaceInitRunCompleted(EcospaceDatastructures As Object) Implements EwEPlugin.IEcospaceInitRunCompletedPlugin.EcospaceInitRunCompleted
        Try

            InitSalinityFromEcosim()
            InitFLEMFiles()

        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Called at the start of an Ecospace time step
    ''' </summary>
    ''' <param name="EcospaceDatastructures"></param>
    ''' <param name="iTime"></param>
    ''' <remarks></remarks>
    Public Sub EcospaceBeginTimeStep(ByVal EcospaceDatastructures As Object, ByVal iTime As Integer) Implements EwEPlugin.IEcospaceBeginTimestepPlugin.EcospaceBeginTimeStep
        Try

            EcospaceTimeStep(iTime)

        Catch ex As Exception

        End Try
    End Sub


    ''' <summary>
    ''' Called when a model has been loaded
    ''' </summary>
    ''' <param name="objEcoPath"></param>
    ''' <param name="objEcoSim"></param>
    ''' <param name="objEcoSpace"></param>
    Public Sub CoreInitialized(ByRef objEcoPath As Object, ByRef objEcoSim As Object, ByRef objEcoSpace As Object) Implements EwEPlugin.ICorePlugin.CoreInitialized
        Try

            Dim EcoPathModel As Ecopath.cEcoPathModel = DirectCast(objEcoPath, Ecopath.cEcoPathModel)
            Dim EcoSimModel As Ecosim.cEcoSimModel = DirectCast(objEcoSim, Ecosim.cEcoSimModel)
            Dim EcoSpaceModel As cEcoSpace = DirectCast(objEcoSpace, cEcoSpace)

            Me.SimData = EcoSimModel.EcosimData
            Me.PathData = EcoPathModel.EcopathData
            Me.SpaceData = EcoSpaceModel.EcoSpaceData

        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Called once an Ecospace run has completed
    ''' </summary>
    ''' <param name="EcoSpaceDatastructures"></param>
    ''' <remarks></remarks>
    Public Sub EcospaceRunCompleted(EcoSpaceDatastructures As Object) Implements EwEPlugin.IEcospaceRunCompletedPlugin.EcospaceRunCompleted

        Try

            If Not ForcePPSalinity Then Return
            FileClose(FileNumber)

            'Restore the arrays we modified so Ecospace and initialize properly
            'Array.Copy(Me.orgHabCap, SpaceData.HabCap, Me.orgHabCap.Length)
            'Array.Copy(Me.orgRelPP, SpaceData.RelPP, Me.orgRelPP.Length)

        Catch ex As Exception

        End Try

    End Sub


#End Region

#Region "Plugin Requirements"

    Public ReadOnly Property ControlImage As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property ControlText As String _
        Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return "FLEM reader"
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String _
        Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return "Configure the FLEM reader"
        End Get
    End Property

    Public ReadOnly Property EnabledState As EwEUtils.Core.eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return EwEUtils.Core.eCoreExecutionState.EcospaceLoaded
        End Get
    End Property

    Public Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, _
                              ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements EwEPlugin.IGUIPlugin.OnControlClick

#If 1 Then
        ' Show UI as docked panel
        frmPlugin = Me.MainInterfaceDocked
#Else
        ' Show UI as dialog
        MainInterfaceDialog()
#End If

    End Sub

    Public ReadOnly Property NavigationTreeItemLocation As String _
        Implements EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation
        Get
            Return "ndSpatialDynamic\ndEcospaceTools"
        End Get
    End Property

    Private Function MainInterfaceDocked() As System.Windows.Forms.Form

        Dim bUIMissing As Boolean = True
        If (Me.m_frmInterface IsNot Nothing) Then
            bUIMissing = Me.m_frmInterface.IsDisposed
        End If

        If bUIMissing Then
            Me.m_frmInterface = New frmFLEMReader(Me.Context, Me)
            Me.m_frmInterface.Text = Me.ControlText
        End If

        Return Me.m_frmInterface

    End Function

    Private Sub MainInterfaceDialog()
        Dim frm As New frmFLEMReader(Me.Context, Me)
        frm.ShowDialog()
    End Sub

    Public Sub UIContext(uic As Object) Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.Context = DirectCast(uic, ScientificInterfaceShared.Controls.cUIContext)
        If Context IsNot Nothing Then
            m_core = Context.Core
        End If
    End Sub


    ''' <summary>
    ''' Initialize the Plugin. This is called when the core loads the Plugin. It will only be called once.
    ''' </summary>
    Public Sub Initialize(ByVal core As Object) Implements EwEPlugin.IPlugin.Initialize

        Debug.Assert(TypeOf core Is EwECore.cCore, Me.ToString & ".Initialize() argument core is not a cCore object.")
        Try
            If TypeOf core Is EwECore.cCore Then
                m_core = DirectCast(core, EwECore.cCore)
                System.Console.WriteLine(Me.ToString & ".Initialize() Successfull.")
            Else
                'some kind of a message
                System.Console.WriteLine(Me.ToString & ".Initialize() Failed.")
                Return
            End If
        Catch ex As Exception
            cLog.Write(ex)
            System.Console.WriteLine(Me.ToString & ".Initialize() Error: " & ex.Message)
            Debug.Assert(False, ex.Message)
            Return
        End Try

    End Sub

    ''' <summary>Generic <see cref="EwEPlugin.IPlugin.Name">IPlugin.Name</see> implementation.</summary>
    Public ReadOnly Property Name() As String Implements EwEPlugin.IPlugin.Name
        Get
            Return "FLEM File Reader Plugin"
        End Get
    End Property

    ''' <summary>Generic <see cref="EwEPlugin.IPlugin.Description">IPlugin.Description</see> implementation.</summary>
    Public ReadOnly Property Description() As String Implements EwEPlugin.IPlugin.Description
        Get
            Return "Forces RelPP and Salinity modifiers from a FLEM nutrient file."
        End Get
    End Property

    ''' <summary>Generic <see cref="EwEPlugin.IPlugin.Author">IPlugin.Author</see> implementation.</summary>
    Public ReadOnly Property Author() As String Implements EwEPlugin.IPlugin.Author
        Get
            Return "UBC Institute for the Oceans and Fisheries"
        End Get
    End Property

    ''' <summary>Generic <see cref="EwEPlugin.IPlugin.Contact">IPlugin.Contact</see> implementation.</summary>
    Public ReadOnly Property Contact() As String Implements EwEPlugin.IPlugin.Contact
        Get
            Return "mailto:support@ecopath.org"
        End Get
    End Property

#End Region

 
End Class
