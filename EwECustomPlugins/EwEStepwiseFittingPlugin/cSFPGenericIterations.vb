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
'    UBC Centre for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
'
' Stepwise Fitting Procedure by Sheila Heymans, Erin Scott, Jeroen Steenbeek
' Copyright 2015- Scottish Association for Marine Science, Oban, Scotland
'
' Erin Scott was funded by the Scottish Informatics and Computer Science
' Alliance (SICSA) Postgraduate Industry Internship Programme.
' ===============================================================================
'
#Region " Imports "

Option Strict On

Imports System.Windows.Forms
Imports EwECore
Imports EwECore.FitToTimeSeries
Imports EwEUtils.Core

#End Region ' Imports

Public MustInherit Class cSFPGenericIterations
    Implements ISFPIterations

    Protected m_core As cCore
    Protected m_iTimeSeries As Integer
    Protected m_F2TSManager As cF2TSManager
    Protected m_bPredOrPredPreySSToV As Boolean
    Public Property Parameters As cSFPParameters Implements ISFPIterations.Parameters

    Public Property k As Integer = 0 Implements ISFPIterations.K
    Public Property EstimatedV As Integer = 0 Implements ISFPIterations.EstimatedV
    Public Property SplinePoints As Integer = 0 Implements ISFPIterations.SplinePoints
    Public Property BaseorFish As Boolean Implements ISFPIterations.BaseorFishValue
    Public Property Enabled As Boolean = True Implements ISFPIterations.Enabled
    Public Property RunState As ISFPIterations.eRunState = ISFPIterations.eRunState.Idle Implements ISFPIterations.RunState

    ''' <summary>Calculated Sum of Squares</summary>
    Protected m_ss As Single = 0
    ''' <summary>Calculated AIC</summary>
    Protected m_aic As Single = 0
    ''' <summary>Calculated AICc</summary>
    Protected m_aicc As Single = 0
    ''' <summary>Anomaly shape data</summary>
    Protected m_anomalyshape() As Single = Nothing
    ''' <summary>Vulnerabilities data</summary>
    Protected m_vulnerabilities(,) As Single = Nothing
    ''' <summary>Calculated time series SS results</summary>
    Protected m_timeseriesSS As Single()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="c"></param>
    ''' <param name="tsi"></param>
    ''' <param name="SSToVChoice"></param>
    ''' <param name="Params"></param>
    ''' -----------------------------------------------------------------------
    Protected Sub Initiate(ByVal c As EwECore.cCore, ByVal tsi As Integer, ByVal SSToVChoice As Boolean, ByVal Params As cSFPParameters, ByVal frmUI As Form) _
        Implements ISFPIterations.Init

        'Get variables needed for SFP iteration
        m_core = c
        m_iTimeSeries = tsi
        m_bPredOrPredPreySSToV = SSToVChoice
        Parameters = Params
        m_F2TSManager = m_core.EcosimFitToTimeSeries
        m_F2TSManager.Connect(frmUI, AddressOf OnRunStarted, AddressOf OnRunStep, AddressOf OnRunStopped, AddressOf OnModelRun)

        ' Allocate memory for anomaly shape
        ReDim Me.m_anomalyshape(Me.m_core.nEcosimTimeSteps)

        ' Allocate memory for vulnerabilities matrix
        ReDim Me.m_vulnerabilities(Me.m_core.nGroups, Me.m_core.nGroups)

        'Allocate memory for time series SS results
        ReDim Me.m_timeseriesSS(Me.m_core.TimeSeriesDataset(m_iTimeSeries).nTimeSeries)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.Load"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride Function Load() As Boolean _
        Implements ISFPIterations.Load

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.Run"/>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Run() As Boolean _
        Implements ISFPIterations.Run

        'Run EcoSim
        RunEcosim()

        ' Store calculated values
        Me.m_ss = Me.GetSS()
        Me.m_aic = Me.GetAIC
        Me.m_aicc = Me.GetAICc()

        ' Store vulnerabilities
        For i As Integer = 1 To m_core.nGroups
            Dim grp As cEcoSimGroupInput = Me.m_core.EcoSimGroupInputs(i)
            For j As Integer = 1 To m_core.nGroups
                Me.m_vulnerabilities(i, j) = grp.VulMult(j)
            Next
        Next

        ' Store first anomaly shape
        Dim shape As cShapeData = Parameters.AppliedShape
        If (shape IsNot Nothing) Then
            Me.m_core.ForcingShapeManager.Load()
            Me.m_anomalyshape = shape.ShapeData
        End If

        'Store time series SS
        GetTimeSeriesSS()

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.Apply"/>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Apply() As Boolean _
        Implements ISFPIterations.Apply

        If (Not Me.RunState = ISFPIterations.eRunState.Completed) Then Return False

        Me.m_core.SetBatchLock(cCore.eBatchLockType.Update)

        ' ToDo: add error checking!
        Try
            ' Enable time series if baseline or fishing
            EnableTimeSeries()

            ' Restore vulnerabilities
            For i As Integer = 1 To m_core.nGroups
                Dim grp As cEcoSimGroupInput = Me.m_core.EcoSimGroupInputs(i)
                For j As Integer = 1 To m_core.nGroups
                    grp.VulMult(j) = Me.m_vulnerabilities(i, j)
                Next
            Next

            'Restore anomaly shape
            Dim shape As cShapeData = Me.Parameters.AppliedShape
            If (shape IsNot Nothing) Then
                shape.ShapeData = Me.m_anomalyshape
                shape.Update()
            End If

        Catch ex As Exception
            ' Whoah!
            ' ToDo: add error feedback!
        End Try

        Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim)

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enable only time series specific to Baseline or Fishing and apply to the Ecosim model
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Function EnableTimeSeries() As Boolean

        If (m_iTimeSeries < 1) Then Return False

        Dim dataset As cTimeSeriesDataset = m_core.TimeSeriesDataset(m_iTimeSeries)

        'Reset fishing effort shapes
        m_core.FishingEffortShapeManager.ResetToDefaults()

        'If Baseline
        If BaseorFish Then
            'Go through each time series of the time series dataset
            For i As Integer = 1 To dataset.nTimeSeries

                Dim ts As cTimeSeries = dataset.TimeSeries(i)
                'If the time series type is 0, 1, 5, 6, 7 enable it
                Select Case ts.TimeSeriesType
                    Case eTimeSeriesType.BiomassRel, _
                         eTimeSeriesType.TotalMortality, _
                         eTimeSeriesType.Catches, _
                         eTimeSeriesType.CatchesRel, _
                         eTimeSeriesType.AverageWeight
                        ts.Enabled = True
                    Case eTimeSeriesType.BiomassAbs
                        ts.Enabled = Parameters.EnableAbsoluteBiomass
                    Case Else
                        ts.Enabled = False
                End Select
            Next
        Else ' Else Fishing
            'Go through each time series of the time series dataset
            For i As Integer = 1 To dataset.nTimeSeries
                'Enable Time Series
                Dim ts As cTimeSeries = dataset.TimeSeries(i)
                ts.Enabled = True
            Next
        End If

        'Apply the enabled time series
        m_core.UpdateTimeSeries(False)

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Check if Ecosim has non-default vulnerabilities, and if so, reset the
    ''' vulnerabilties. 
    ''' </summary>
    ''' <returns>True if Ecosim has all default vulnerabilties.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function ResetVs() As Boolean
        ' Suppress prompt, just reset the vulnerabilities without asking
        Return m_core.CheckResetDefaultVulnerabilities(True)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run Sensitivity search according to user input
    ''' </summary>
    ''' <returns>True if run successful</returns>
    ''' -----------------------------------------------------------------------
    Protected Function RunSensitivityOfSSToV() As Boolean

        Dim BSuccess As Boolean = False

        'Set the number of blocks selected to Max K
        m_F2TSManager.nBlockCodes = Parameters.K

        'If PredOrPredPreySSToV = true then run SS2VBy Predator
        If m_bPredOrPredPreySSToV Then
            If m_F2TSManager.RunSensitivitySS2VByPredator(True, TriState.False) Then
                Debug.Assert(Not m_F2TSManager.IsRunning)
                'Set vulnerabiltiy blocks
                m_F2TSManager.setNBlocksFromSensitivity(Parameters.K)
                BSuccess = True
            End If
            'Else run SS2VBy Pred/Prey
        Else
            If m_F2TSManager.RunSensitivitySS2VByPredPrey(True, TriState.False) Then
                Debug.Assert(Not m_F2TSManager.IsRunning)
                'Set vulnerabiltiy blocks
                m_F2TSManager.setNBlocksFromSensitivity(Parameters.K)
                BSuccess = True
            End If
        End If

        Return BSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Launch an Ecosim run.
    ''' </summary>
    ''' <returns>True if a run started successfully.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function RunEcosim() As Boolean
        Return m_core.RunEcoSim()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run Vulnerability Search iterations according to k estimated parameters
    ''' </summary>
    ''' <returns>True if run successful</returns>
    ''' -----------------------------------------------------------------------
    Protected Function RunVulnerabilitySearch() As Boolean

        Dim bSuccess As Boolean = False

        'Has a sensitivity search been ran
        If m_F2TSManager.HasRunSens Then

            'Setup manager to do a vunerability search
            m_F2TSManager.VulnerabilitySearch = True
            m_F2TSManager.AnomalySearch = False
            m_F2TSManager.VulnerabilityVariance = 10.0
            'Set the number of blocks selected (Number of parameters to estimate)
            m_F2TSManager.nBlockCodes = EstimatedV

            ' Run the search silently
            If m_F2TSManager.RunSearch(True, TriState.False) Then
                Debug.Assert(Not m_F2TSManager.IsRunning)
                bSuccess = True
            End If
        End If

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reset the FF shape. 
    ''' </summary>
    ''' <returns>Always returns true, even if there may not be a shape to reset.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function ResetFF() As Boolean

        Dim sDefaultValue As Single = 1.0
        Dim shape As cShapeData = Parameters.AppliedShape

        'Reset all applied shapes 
        If (shape IsNot Nothing) Then
            For i As Integer = 0 To shape.nPoints
                shape.ShapeData(i) = sDefaultValue
            Next i
            shape.Update()
        End If

        ' #1421: do not affect other shapes

        ''More than one shape can be applied so reset the other shapes 
        'Dim interactions As cMediatedInteractionManager = core.MediatedInteractionManager
        'For Each shape In core.ForcingShapeManager
        '    If interactions.IsApplied(shape) Then
        '        For i As Integer = 0 To shape.nPoints
        '            shape.ShapeData(i) = sDefaultValue
        '        Next i
        '        shape.Update()
        '    End If
        'Next

        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run Anomaly Search according to spline point estimated parameters. This search will only run if a FF is applied to a PP.
    ''' </summary>
    ''' <returns>True if run successful</returns>
    ''' -----------------------------------------------------------------------
    Protected Function RunAnomalySearch() As Boolean

        Dim shape As cShapeData = Parameters.AppliedShape
        Dim bSuccess As Boolean = False

        'If there is no applied shape do not run search (This is already checked by the cSFPManager but just to make sure)
        If (shape IsNot Nothing) Then

            'Setup manager to do a Anomaly search
            m_F2TSManager.AnomalySearch = True
            m_F2TSManager.VulnerabilitySearch = False
            m_F2TSManager.FirstYear = 1
            m_F2TSManager.LastYear = m_core.TimeSeriesDataset(m_iTimeSeries).NumPoints
            m_F2TSManager.PPVariance = 0.1
            'Set the number of spline points selected (Number of parameters to estimate)
            m_F2TSManager.NumSplinePoints = SplinePoints
            m_F2TSManager.AnomalySearchShapeNumber = shape.Index

            ' Run the search silently
            If m_F2TSManager.RunSearch(True, TriState.False) Then
                Debug.Assert(Not Me.m_F2TSManager.IsRunning)
                bSuccess = True
            End If
        End If

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run Vunerability and Anomaly Search according estimated parameters and spline points. This search will only run if a FF is applied to a PP.
    ''' </summary>
    ''' <returns>True if iterations successful</returns>
    ''' -----------------------------------------------------------------------
    Protected Function RunVandASearch() As Boolean

        Dim shape As cShapeData = Parameters.AppliedShape
        Dim bSuccess As Boolean = False

        'If there is an applied shape and a sensitivity search has been ran : run the search
        If (shape IsNot Nothing) And m_F2TSManager.HasRunSens Then

            'Setup manager to do a Vulnerability and Anomaly search
            m_F2TSManager.AnomalySearch = True
            m_F2TSManager.FirstYear = 1
            m_F2TSManager.LastYear = m_core.TimeSeriesDataset(m_iTimeSeries).NumPoints
            m_F2TSManager.PPVariance = 0.1
            'Set the number of spline points selected (Number of parameters to estimate)
            m_F2TSManager.NumSplinePoints = SplinePoints
            m_F2TSManager.AnomalySearchShapeNumber = shape.Index

            m_F2TSManager.VulnerabilitySearch = True
            'Set the number of blocks selected (Number of parameters to estimate)
            m_F2TSManager.nBlockCodes = EstimatedV
            m_F2TSManager.VulnerabilityVariance = 10.0


            ' Run the search silently
            If m_F2TSManager.RunSearch(True, TriState.False) Then
                Debug.Assert(Not m_F2TSManager.IsRunning)
                bSuccess = True
            End If
        End If

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.Clear"/>
    ''' -----------------------------------------------------------------------
    Public Sub Clear() _
       Implements ISFPIterations.Clear
        m_F2TSManager.Disconnect()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return name of run 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Function BaselineOrFishing() As String
        If (BaseorFish) Then
            Return "Baseline"
        Else
            Return "Fishing"
        End If
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return name of hypothesis 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Name() As String _
        Implements ISFPIterations.Name
        Get
            ' ToDo: globalize this

            Dim HName As String
            'If simple run
            If (EstimatedV = 0 And SplinePoints = 0) Then
                HName = BaselineOrFishing()
                Return HName
            End If
            'If Vunerability Search
            If (EstimatedV > 0 And SplinePoints = 0) Then
                HName = BaselineOrFishing() & " and " & EstimatedV & "v"
                Return HName
            End If
            'If Anomaly Search
            If (EstimatedV = 0 And SplinePoints > 0) Then
                HName = BaselineOrFishing() & " and " & SplinePoints & "pp"
                Return HName
            Else 'If V and A Search
                HName = BaselineOrFishing() & " and " & EstimatedV & "v" & " + " & SplinePoints & "pp"
                Return HName
            End If

        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.SS"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property SS() As Single _
        Implements ISFPIterations.SS
        Get
            If (Me.RunState <> ISFPIterations.eRunState.Completed) Then Return 0
            Return Me.m_ss
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.AIC"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property AIC() As Single _
        Implements ISFPIterations.AIC
        Get
            If (Me.RunState <> ISFPIterations.eRunState.Completed) Then Return 0
            Return Me.m_aic
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.AICc"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property AICc() As Single _
        Implements ISFPIterations.AICc
        Get
            If (Me.RunState <> ISFPIterations.eRunState.Completed) Then Return 0
            Return Me.m_aicc
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.IsBestFit"/>
    ''' -----------------------------------------------------------------------
    Public Property IsBestFit As Boolean = False _
        Implements ISFPIterations.IsBestFit

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.AnomalyShape"/>
    ''' -----------------------------------------------------------------------
    Public Function AnomalyShape() As Single() _
        Implements ISFPIterations.AnomalyShape
        Return Me.m_anomalyshape
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.Vulnerabilities"/>
    ''' -----------------------------------------------------------------------
    Public Function Vulnerabilities() As Single(,) _
        Implements ISFPIterations.Vulnerabilities
        Return Me.m_vulnerabilities
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ISFPIterations.TimeSeriesSS"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property TimeSeriesSS As Single() _
          Implements ISFPIterations.TimeSeriesSS
        Get
            If (Me.RunState <> ISFPIterations.eRunState.Completed) Then Return Nothing
            Return Me.m_timeseriesSS
        End Get
    End Property


#Region " Subs needed for connect search runs of F2TSManager "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Called when a search run in F2TSManager has just started
    ''' </summary>
    ''' <param name="runType"></param>
    ''' <param name="nSteps"></param>
    ''' -------------------------------------------------------------------
    Protected Sub OnRunStarted(ByVal runType As eRunType, ByVal nSteps As Integer)

        'get the results of this iteration from the manager
        'Dim data As cF2TSResults = F2TSManager.Results
        ' Debug.WriteLine("Started SS= " & data.BaseSS)

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Called on each run step of a search run in F2TSManager
    ''' </summary>
    ''' -------------------------------------------------------------------
    Protected Sub OnRunStep()

        ''get the results of this iteration from the manager
        'Dim data As cF2TSResults = F2TSManager.Results
        'Dim runtype As eRunType = data.RunType

        'Select Case runtype

        '    Case eRunType.Search
        '        'retrieve search analysis result
        '        Dim rsltSearch As cSearchResults = CType(data, cSearchResults)
        '        'Debug.WriteLine("iStep= " & rsltSearch.iStep & " SS= " & rsltSearch.IterSS)

        '        ' Reload shape
        '        '   If F2TSManager.AnomalySearch Then
        '        'Me.Core.ForcingShapeManager.Load()
        '        '' Ugh, there must be a better way to do this
        '        'For Each shape As cShapeData In Me.m_shapeHandler.SelectedShapes
        '        '    shape.Update()
        '        'Next
        '        '  End If

        '        ' Case eRunType.SensitivitySS2VByPredPrey, eRunType.SensitivitySS2VByPredator
        '        '     Dim results As cSensitivityToVulResults = DirectCast(F2TSManager.Results, cSensitivityToVulResults)
        '        '   Debug.WriteLine("iPred= " & results.iPred & " iPrey= " & results.iPrey & " SS= " & results.SSen)

        'End Select

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' The model run has completed
    ''' </summary>
    ''' <param name="runType"></param>
    ''' -------------------------------------------------------------------
    Protected Sub OnRunStopped(ByVal runType As eRunType)

        'If (TypeOf F2TSManager.Results Is cSearchResults) Then
        '    Dim res As cSearchResults = DirectCast(F2TSManager.Results, cSearchResults)
        '    'Debug.WriteLine("nAICPars= " & res.nAICPars & " Stopped SS= " & res.IterSS)
        'End If

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
        'System.Console.WriteLine("Ecosim run " & iCurrentIterationStep.ToString & " of " & nTotalIterationSteps.ToString)
    End Sub

#End Region

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the current value of SS.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function GetSS() As Single

        ' Sanity checks
        Debug.Assert(Me.m_core IsNot Nothing)

        If (EstimatedV = 0 And SplinePoints = 0) Then
            Return m_core.EcosimStats.SS
        Else
            Dim res As cSearchResults = DirectCast(m_F2TSManager.Results, cSearchResults)
            Return res.IterSS
        End If
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the current value of AIC.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function GetAIC() As Single

        Debug.Assert(Me.m_core IsNot Nothing)
        Debug.Assert(Me.Parameters IsNot Nothing)

        Dim nData As Integer = Parameters.NumberOfObservations

        'If simple run
        If (EstimatedV = 0 And SplinePoints = 0) Then
            Return Me.m_F2TSManager.getAIC(0, nData, GetSS)
        End If
        'If Vunerability Search
        If (EstimatedV > 0 And SplinePoints = 0) Then
            Return m_F2TSManager.getAIC(EstimatedV, nData, GetSS)
        End If
        'If Anomaly Search
        If (EstimatedV = 0 And SplinePoints > 0) Then
            Return m_F2TSManager.getAIC(SplinePoints, nData, GetSS)
        Else 'V and A Search
            Return m_F2TSManager.getAIC(k, nData, GetSS)
        End If
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the current value of AICc.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function GetAICc() As Single

        Debug.Assert(Me.m_core IsNot Nothing)
        Debug.Assert(Me.Parameters IsNot Nothing)

        Dim nData As Integer = Parameters.NumberOfObservations
        Dim answer As Single = 0

        'If simple run
        If (EstimatedV = 0 And SplinePoints = 0) Then
            answer = GetAIC() + 2.0F * 0.0F * (0.0F - 1.0F) / (nData - 0.0F - 1.0F)
            Return answer
        End If
        'If Vunerability Search
        If (EstimatedV > 0 And SplinePoints = 0) Then
            answer = GetAIC() + 2.0F * EstimatedV * (EstimatedV - 1.0F) / (nData - EstimatedV - 1.0F)
            Return answer
        End If
        'If Anomaly Search
        If (EstimatedV = 0 And SplinePoints > 0) Then
            answer = GetAIC() + 2.0F * SplinePoints * (SplinePoints - 1.0F) / (nData - SplinePoints - 1.0F)
            Return answer
        Else 'V and A Search
            answer = GetAIC() + 2.0F * k * (k - 1.0F) / (nData - k - 1.0F)
            Return answer
        End If

    End Function

    Private Sub GetTimeSeriesSS()

        For i As Integer = 1 To Me.m_core.nTimeSeries
            m_timeseriesSS(i) = m_core.TimeSeriesDataset(m_iTimeSeries).TimeSeries(i).DataSS
        Next

    End Sub

#End Region ' Internals

End Class
