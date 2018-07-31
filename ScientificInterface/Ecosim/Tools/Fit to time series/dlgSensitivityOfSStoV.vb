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
Imports EwECore.FitToTimeSeries
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Dialog, implementing the Ecosim - Fit to Time Series - Sensitivity of SS
''' to Vulnerabilities search interface.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class dlgSensitivityOfSStoV

#Region " Private variables "

    Private m_uic As cUIContext = Nothing
    Private m_SSPreyPred(,) As Single ' Sen by pred/prey
    Private m_iNumBlocks As Integer
    Private m_F2TSManager As cF2TSManager = Nothing
    Private m_SSbase As Single = 0.0
    Private m_bRunning As Boolean = False
    Private m_bInUpdate As Boolean = False

#End Region ' Private variables

#Region " Constructors "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext, _
                   ByVal manager As cF2TSManager)

        Me.InitializeComponent()

        ' Sanity checks
        Debug.Assert(uic IsNot Nothing)
        Debug.Assert(manager IsNot Nothing)

        Me.m_uic = uic
        Me.m_F2TSManager = manager

        ReDim Me.m_SSPreyPred(Me.m_uic.Core.nGroups, Me.m_uic.Core.nGroups)

        Me.m_ucVulBlocks.UIContext = Me.m_uic

    End Sub

#End Region ' Constructors

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property NumBlocks() As Integer
        Get
            Return Me.m_iNumBlocks
        End Get
        Set(ByVal value As Integer)

            ' Truncate
            value = Math.Max(0, Math.Min(CInt(Me.m_nudNumBlocks.Maximum), value))
            ' Set
            Me.m_iNumBlocks = value
            ' Respond
            Me.UpdateControls()
            Me.UpdateDisplay()

        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property VulnerabilityBlocks() As Integer(,)
        Get
            Return Me.m_ucVulBlocks.Vulblocks
        End Get
    End Property

#End Region ' Public access

#Region " Private events "

#Region " Form "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

        MyBase.OnLoad(e)
        Me.m_nudNumBlocks.Maximum = Me.m_uic.Core.nGroups * Me.m_uic.Core.nGroups
        Me.UpdateControls()
        Me.UpdateDisplay()

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)
        Me.m_F2TSManager = Nothing
        MyBase.OnFormClosed(e)
    End Sub

#End Region ' Form

#Region " Controls "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnSearchCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbSearchPred.CheckedChanged, _
                m_rbSearchPredPrey.CheckedChanged

        If (Me.m_rbSearchPredPrey.Checked) Then
            Me.RunType = eRunType.SensitivitySS2VByPredPrey
        Else
            Me.RunType = eRunType.SensitivitySS2VByPredator
        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub OnSearch(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSearch.Click

        Me.StartRun()

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnOk.Click

        If (Me.StopRun() = False) Then Return
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()

    End Sub

    Private Sub OnNumBlocksChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_nudNumBlocks.ValueChanged

        If (Me.m_uic Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.NumBlocks = CInt(Me.m_nudNumBlocks.Value)

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -------------------------------------------------------------------
    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnCancel.Click

        If (Me.StopRun() = False) Then Return
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()

    End Sub

    Private Sub OnSaveToCSV(sender As System.Object, e As System.EventArgs) _
        Handles m_btnSaveToCSV.Click

        Dim cmdFS As cFileSaveCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
        Dim strPath As String = Me.m_uic.Core.DefaultOutputPath(EwEUtils.Core.eAutosaveTypes.Ecosim)

        cmdFS.Invoke(Path.Combine(strPath, "SensitivityToV.csv"), SharedResources.FILEFILTER_CSV, 0)

        If (cmdFS.Result = DialogResult.OK) Then
            Try
                Me.m_F2TSManager.SaveToCSV(cmdFS.FileName)
            Catch ex As Exception
                ' NOP
            End Try
        End If

    End Sub

#End Region ' Controls

#Region " F2TS manager interface "

    Private Sub OnRunStarted(ByVal runType As eRunType, ByVal nSteps As Integer)
        ' Sanity check
        Debug.Assert(runType = Me.RunType)

        'Console.WriteLine("Dlg: run started " & runType)

        Me.m_progress.Maximum = nSteps

        Me.m_bRunning = True
        Me.UpdateControls()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnRunStep()

        Dim results As cSensitivityToVulResults = DirectCast(Me.m_F2TSManager.Results, cSensitivityToVulResults)
        Dim runType As eRunType = results.RunType

        ' Sanity check
        Debug.Assert(runType = Me.RunType)

        Dim iPred As Integer = results.iPred
        Dim iPrey As Integer = results.iPrey
        Dim sSen As Single = results.SSen

        Select Case runType

            Case eRunType.SensitivitySS2VByPredPrey
                ' Keep the ss for this prey pred for later use
                Me.m_SSPreyPred(iPrey, iPred) = sSen

            Case eRunType.SensitivitySS2VByPredator
                ' Keep the ss for this pred for later use
                For iPrey = 1 To Me.m_uic.Core.nGroups
                    Me.m_SSPreyPred(iPred, iPrey) = sSen
                Next

        End Select

        Me.m_progress.Value += 1
        Me.UpdateControls()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The model run has completed
    ''' </summary>
    ''' <param name="runType"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnRunStopped(ByVal runType As eRunType)

        ' Sanity check
        Debug.Assert(runType = Me.RunType)

        Me.m_bRunning = False

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="runType"></param>
    ''' <param name="iCurrentIterationStep"></param>
    ''' <param name="nTotalIterationSteps"></param>
    ''' -------------------------------------------------------------------
    Protected Sub OnModelRun(ByVal runType As eRunType, ByVal iCurrentIterationStep As Integer, ByVal nTotalIterationSteps As Integer)
        ' NOP
    End Sub

#End Region ' F2TS manager

#End Region ' Private events

#Region " Internal implementation "

    Private Property RunType() As eRunType

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateControls()

        ' Sanity checks
        If (Me.m_uic Is Nothing) Then Return
        If (Me.m_F2TSManager Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.m_bInUpdate = True

        Dim lColors As List(Of Color) = Me.m_uic.StyleGuide.GetEwE5ColorRamp(Me.m_iNumBlocks)
        lColors.Insert(0, Color.Black)
        Me.m_legend.Colors = lColors

        lColors = Me.m_uic.StyleGuide.GetEwE5ColorRamp(Me.m_iNumBlocks)
        lColors.Reverse()
        lColors.Insert(0, Color.Black)
        Me.m_ucVulBlocks.BlockColors = lColors.ToArray
        Me.m_ucVulBlocks.RefreshContent()

        Me.m_nudNumBlocks.Value = Me.m_iNumBlocks

        Me.m_rbSearchPred.Enabled = Not Me.m_bRunning
        Me.m_rbSearchPredPrey.Enabled = Not Me.m_bRunning

        Me.m_nudNumBlocks.Enabled = Me.HasRun()
        Me.m_progress.Visible = Me.m_bRunning
        Me.m_nudNumBlocks.Enabled = Not Me.m_bRunning

        Me.m_btnSearch.Enabled = Not Me.m_bRunning
        Me.m_btnOk.Enabled = Me.HasRun() And Not Me.m_bRunning
        Me.m_btnSaveToCSV.Enabled = Me.HasRun() And Not Me.m_bRunning

        Me.m_bInUpdate = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns>True if a new run was started succesfully.</returns>
    ''' -----------------------------------------------------------------------
    Private Function StartRun() As Boolean
        If (Me.m_F2TSManager.IsRunning()) Then Return False

        ' Reset controls
        Me.m_progress.Value = 0

        Me.m_F2TSManager.Connect(Me, AddressOf OnRunStarted, AddressOf OnRunStep, AddressOf OnRunStopped, Nothing)
        If (Me.m_rbSearchPredPrey.Checked) Then
            If (Me.m_F2TSManager.RunSensitivitySS2VByPredPrey(False, TriState.False) = False) Then
                Return False
            End If
            Me.RunType = eRunType.SensitivitySS2VByPredPrey
        Else
            If (Me.m_F2TSManager.RunSensitivitySS2VByPredator(False, TriState.False) = False) Then
                Return False
            End If
            Me.RunType = eRunType.SensitivitySS2VByPredator
        End If
        Me.m_F2TSManager.Disconnect()

        Me.UpdateControls()
        Me.UpdateDisplay()

        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stop an active F2TS manager run.
    ''' </summary>
    ''' <returns>
    ''' True if the manager is no longer running (or was not running at all).
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function StopRun() As Boolean
        Return Me.m_F2TSManager.StopRun()
    End Function

    Private Function HasRun() As Boolean
        ' States whether a search has been ran
        Return (Me.m_F2TSManager.HasRunSens)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -------------------------------------------r----------------------------
    Private Sub UpdateDisplay()

        If (Me.m_F2TSManager Is Nothing) Then Return

        If Me.HasRun Then

            'have the manager sort the blocks acording to the last run sensitivity type
            Me.m_F2TSManager.setNBlocksFromSensitivity(Me.NumBlocks)

            Dim vblocks(,) As Integer = m_F2TSManager.VulnerabilityBlocks
            For iPred As Integer = 1 To Me.m_uic.Core.nGroups
                For iPrey As Integer = 1 To Me.m_uic.Core.nGroups
                    If Me.m_F2TSManager.isPredPrey(iPred, iPrey) Then
                        Me.m_ucVulBlocks.Vulblocks(iPred, iPrey) = vblocks(iPred, iPrey)
                    End If
                Next iPrey
            Next iPred

            Me.m_ucVulBlocks.Invalidate()

        End If

    End Sub

#End Region ' Internal implementation

End Class