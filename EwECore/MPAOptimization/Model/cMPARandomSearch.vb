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

Option Strict On
Imports System.Math
Imports EwEUtils.Core
Imports System.IO
Imports EwEUtils.Utilities

Public Class cMPARandomSearch
    Inherits cMPAOptBaseClass

#Region "Private data"

    Private LayerSumInMPA() As Single
    Private MaxLayerSumByLayerAndPctMPA(,) As Single

#End Region

#Region "Public Properties and Methods"

    Public Overrides ReadOnly Property OKtoRun() As Boolean
        Get
            'Nothing to check here...
            'Really??? That's hard to believe
            Return True
        End Get
    End Property


#End Region

#Region "Construction and Initialization"


    Public Sub New()
        MyBase.new()

        m_OutputFilename = "MPAOpt_random_Output.csv"

    End Sub

    Private Sub initForRun()

        Try

            'Ecoseed does not listen to the Ecospace time steps
            Me.m_EcoSpace.TimeStepDelegate = Nothing

            'create a new list to store the results
            m_lstObjectiveResults = New List(Of cObjectiveResult)
            TargetSumMax = 0

            'Clear out any values from a previous ecoseed run
            m_data.Clear()

            RedimSeedVariables()

        Catch ex As Exception
            Me.WriteError(ex)
            Throw New ApplicationException(Me.ToString & ".initForRun() Error: " & ex.Message, ex)
        End Try

    End Sub


#End Region

#Region "Running"

    Overrides Sub Run() 'Implements IMPASearchModel.Run

        Try

            Me.m_bRunning = True
            Me.setRunState(cMPAOptManager.eRunStates.Initializing)

            Me.m_data.StopRun = False

            Me.runSearch()

        Catch ex As Exception
            Me.WriteError("MPA Optimizatoin Random Search Error")
            Debug.Assert(False, ex.StackTrace)
        End Try

        Me.m_bRunning = False
        Me.setRunState(cMPAOptManager.eRunStates.Completed)

    End Sub


    Private Sub runSearch()

        Debug.Assert(m_data.iMPAtoUse > 0, "Current MPA not set!!!.")

        'VC changes
        'Main loop for running the Random MPA optimization

        Dim StoreOptimalPct As Single = 1 'from GUI
        Dim MinimalEvaluationValue As Single = 0
        Dim writer As StreamWriter = Nothing

        If Me.m_bAutosaveResults Then
            If cFileUtils.IsDirectoryAvailable(Me.m_strOutputPath, True) Then
                Try
                    writer = New StreamWriter(Path.Combine(Me.m_strOutputPath, m_OutputFilename))
                Catch ex As Exception

                End Try
            End If
        End If

        Try
            Debug.Assert(m_data IsNot Nothing, "Ecoseed: data not initialized")
            Debug.Assert(m_EcoSpace IsNot Nothing, "Ecoseed: Ecospace not initialized")
            System.Console.WriteLine("-----------MPA Random Search --------------")

            Me.initForRun()

            ' make snapshot of MPA cell occupation for quick lookup during computations
            Me.InitIsMPA()

            m_search.SearchMode = eSearchModes.SpatialOpt
            m_search.setMinSearchBlocks()
            Me.getBaseValues()

            Me.WriteOutputFileHeader(writer)

            CalculateCellWeightings()

            Dim iR As Integer = m_SpaceData.InRow
            Dim iC As Integer = m_SpaceData.InCol
            'we don't want to clear all data cells, only the one with the currently selected MPA
            'Array.Clear(Me.m_SpaceData.MPA, 0, Me.m_SpaceData.MPA.Length)
            For i As Integer = 1 To iR
                For j As Integer = 1 To iC
                    Me.m_SpaceData.MPA(m_data.iMPAtoUse)(i, j) = False
                Next
            Next

            'We need number of potential MPA cells, this is watercells 
            '  - (cells which are either not an MPA 
            '    or which already are the same kind of MPA.)
            Dim CellCount As Integer
            For i As Integer = 1 To iR
                For j As Integer = 1 To iC
                    If Me.m_SpaceData.Depth(i, j) > 0 And (m_SpaceData.MPA(m_data.iMPAtoUse)(i, j) Or Me.IsMPA(i, j) = False) Then
                        CellCount += 1
                    End If
                Next j
            Next i

            'Get the layer weights by percentage MPA coverage
            sortLayersByCellWeight(CellCount)

            'Step from Min area(%) (= integer) to Max area(%) (= integer) stepsize = Step (%) (=integer)
            Dim iStep As Integer = CInt((-m_data.MinArea + m_data.MaxArea) / m_data.stepSize)
            Dim nStep As Integer = 0

            Me.setRunState(cMPAOptManager.eRunStates.Searching)

            m_nIters = 0

            For iPropMPA As Integer = m_data.MinArea To m_data.MaxArea Step m_data.stepSize
                'keep track of how may times we've stepped: 
                'calculate how many cells that should be closed:
                'this is calculated based on number of water cells - number of other mpsa cells, not total number of cells:
                Dim NumberMPA As Integer = CInt(iPropMPA * CellCount / 100)

                'Step through and do iterations:
                For m_iIter As Integer = 1 To m_data.nIterations
                    'select the MPA cells that are to be evaluated in this run
                    Me.selectRandomCells(NumberMPA, m_data.iMPAtoUse)

                    Me.fireOnIteration()

                    'Run EcoSpace
                    Me.m_EcoSpace.Run()
                    If m_data.StopRun Then Exit For

                    'Evaluate the current MPA cell selection
                    Me.EvaluateRun()

                    'Store LayerSumInMPA
                    Me.calcImportanceLayersCoverageInRun()

                    ' Process results
                    Me.StoreObjectiveFunctionResults(writer)
                    ' Next
                    Me.m_nIters += 1

                Next
                If Me.m_data.StopRun Then Exit For
                nStep += 1
            Next

            Me.cleanUp()

        Catch ex As Exception
            Me.WriteError(ex)
            Me.m_bRunning = False
            Debug.Assert(False, ex.StackTrace)
        End Try

        If (writer IsNot Nothing) Then
            writer.Flush()
            writer.Close()
            writer.Dispose()
        End If

    End Sub

    Private Sub selectRandomCells(ByVal NumberMPA As Integer, ByVal curMPA As Integer)
        'VC changes
        Dim generator As New Random()   '

        Try

            'clear out the last set of cells
            m_data.ClearCells()

            ' Clear data cells with the currently selected MPA
            For i As Integer = 1 To Me.m_SpaceData.InRow
                For j As Integer = 1 To Me.m_SpaceData.InCol
                    m_SpaceData.MPA(m_data.iMPAtoUse)(i, j) = False
                Next
            Next

            'Now start selecting the ones to make MPAs
            Dim iThisCell As Integer
            Dim iC As Integer = 0
            Dim GetOut As Integer = 0

            Dim Rand As New Random() '  Double = generator.NextDouble

            Do While iC < NumberMPA And GetOut < 100 * NumberMPA
                Dim RanVal As Double = Rand.NextDouble
                For i As Integer = 1 To CellCount
                    If CumulativeCellWeight(i) >= RanVal Then iThisCell = i : Exit For
                Next

                Dim GetRow As Integer = (iThisCell - 1) \ Me.m_SpaceData.InCol + 1
                Dim GetCol As Integer = (iThisCell - 1) Mod Me.m_SpaceData.InCol + 1

                'now we know which cell to close
                'but check that the cell hasn't been made into an mpa already
                If Me.m_SpaceData.Depth(GetRow, GetCol) > 0 And m_SpaceData.MPA(m_data.iMPAtoUse)(GetRow, GetCol) = False Then
                    m_SpaceData.MPA(m_data.iMPAtoUse)(GetRow, GetCol) = True
                    'System.Console.WriteLine(GetRow.ToString & "  " & GetCol.ToString)
                    m_data.AddCell(GetRow, GetCol, curMPA)
                    iC += 1
                    GetOut = 0
                Else
                    GetOut += 1
                End If
            Loop

        Catch ex As Exception
            Me.WriteError(ex)
            Debug.Assert(False, Me.ToString & ".selectRandomCells() Error: " & ex.Message)
            Throw New ApplicationException(Me.ToString & ".selectRandomCells() Error:", ex)
        End Try

    End Sub


    Protected Sub CalculateCellWeightings()
        'VC added this sub
        Dim iC As Integer       'used to count the cells

        Try

            Dim inRow As Integer = m_SpaceData.InRow
            Dim inCol As Integer = m_SpaceData.InCol
            CellCount = inRow * inCol

            ReDim CumulativeCellWeight(CellCount)
            Dim CellWeight(inRow, inCol) As Double

            'If on the GUI the "Group weighting" is checked then calculate cellweight, otherwise, set to 1
            'use guidance function
            'cell contribution to objectivity function at the ecopath base case 
            '1. equal prob
            '2. biomass or habitat proportional
            '3. inverse objectivity function 
            'evt 4 mcmc search, start with a given number of closed cells, replace a cell (based on probability), evaluate, 

            'develop a measure including
            '1. spatial cost of fishing (distance from port): this becomes and "importance" layer, we can just cut and paste it in
            '2. depth factor (deeper  = more costly): this also becomes an importance layer
            '3. Any "importance" layer, i.e. Jeroen, we need to be able to store "importance" layers, which for now can be cut and pasted into ecospace. 
            '   The "importance" layers will need to have a title and description, plus a value for each cell. 
            '4. How much does the cell contribute to fishing pressure for the cells to be protected


            'Scan through the spreadsheet with the importance layers, and set up the likelihood function.

            'If Me.m_data.bUseCellWeight Then
            '    ''Get the ecosystem structure weightings from the GUI (needs to be added)
            '    ''for now hard code to 1
            '    'Dim GroupWeight(m_SpaceData.NGroups) As Single
            '    'For ip As Integer = 1 To m_SpaceData.NGroups
            '    '    GroupWeight(ip) = 1
            '    'Next

            '    For i As Integer = 1 To inRow
            '        For j As Integer = 1 To inCol
            '            For ip As Integer = 1 To m_SpaceData.NGroups
            '                '    CellWeight(i, j) += GroupWeight(ip) * BOrig(i, j, ip)
            '                CellWeight(i, j) += Me.m_search.BGoalValue(ip) * BOrig(i, j, ip)
            '            Next
            '        Next
            '    Next
            'Else
            'iC = 0

            Dim data()(,) As Single = Me.m_SpaceData.ImportanceLayerMap
            Dim weight As Double
            Dim LayerSum(Me.m_SpaceData.nImportanceLayers) As Double

            'VC2008Nov11, scaling each of the importance layers to have average 1
            For iL As Integer = 1 To Me.m_SpaceData.nImportanceLayers
                'weight = Me.m_SpaceData.ImportanceLayers(iL).sWeight
                Dim Count As Integer = 0
                For i As Integer = 1 To inRow
                    For j As Integer = 1 To inCol
                        If data(iL)(i, j) > 0 Then
                            Count += 1
                            LayerSum(iL) += data(iL)(i, j)
                        End If
                    Next j
                Next i
                'This will make the average for each layer 1, but then a layer that only has values 
                'in a few cells will count much less, than one with values in many cells
                'If Count > 0 Then AverageLayer(iL) /= Count
                'So insteat making the layers SUM to 1
                If LayerSum(iL) = 0 Then LayerSum(iL) = 1 'just to avoid division with 0, if a layer is empty
            Next iL

            Dim minCellWeight As Double = 1000000000000000
            For iL As Integer = 1 To Me.m_SpaceData.nImportanceLayers
                weight = Me.m_SpaceData.ImportanceLayerWeight(iL)
                For i As Integer = 1 To inRow
                    For j As Integer = 1 To inCol
                        CellWeight(i, j) += weight * data(iL)(i, j) / LayerSum(iL)
                        If CellWeight(i, j) < minCellWeight And CellWeight(i, j) > 0 Then minCellWeight = CellWeight(i, j)
                    Next j
                Next i
            Next iL

            'now make sure all cells can be selected:
            For i As Integer = 1 To inRow
                For j As Integer = 1 To inCol
                    If CellWeight(i, j) = 0 Then 'give it a value
                        CellWeight(i, j) = 0.001 * minCellWeight
                    End If
                Next j
            Next i


            'Now calculate cumulative weighted importance over all cells:
            iC = 0
            Dim Sum As Double = 0
            For i As Integer = 1 To inRow
                For j As Integer = 1 To inCol
                    iC += 1
                    If CellWeight(i, j) < 0 Then CellWeight(i, j) = 0
                    Sum += CellWeight(i, j)
                    CumulativeCellWeight(iC) = Sum
                Next
            Next

            'Finally scalse the cellweights so that they sum to 1
            If Sum > 0 Then
                For i As Integer = 1 To CellCount
                    CumulativeCellWeight(i) /= Sum
                Next
            Else
                'if there are no values in any of the importance layer
                'set CumulativeCellWeight() to an even gradient so that the cell selection will not be weighted
                Dim g As Single = CSng(1 / CellCount)
                For i As Integer = 1 To CellCount
                    CumulativeCellWeight(i) += g * i
                Next
            End If

        Catch ex As Exception
            Me.WriteError(ex)
            Debug.Assert(False, ex.StackTrace)
            Throw New ApplicationException(Me.ToString & ".CalculateCellWeightings() " & ex.Message, ex)
        End Try

    End Sub

    Protected Sub sortLayersByCellWeight(ByVal CellCount As Integer)
        Dim NoCells As Integer = m_SpaceData.InRow * m_SpaceData.InCol
        ReDim MaxLayerSumByLayerAndPctMPA(m_SpaceData.nImportanceLayers, 100)

        For iL As Integer = 1 To Me.m_SpaceData.nImportanceLayers
            Dim Cnt As Integer = 0
            Dim ArrayVal(NoCells) As Single

            For i As Integer = 1 To m_SpaceData.InRow
                For j As Integer = 1 To m_SpaceData.InCol
                    Cnt = Cnt + 1
                    'Make a copy of the data
                    ArrayVal(Cnt) = m_SpaceData.ImportanceLayerMap(iL)(i, j)
                Next j
            Next i
            'now we have all the layer values in ArrayVal, so sort them:
            System.Array.Sort(ArrayVal)
            System.Array.Reverse(ArrayVal)
            'We can now store the layerweight for each percentage coverage:
            For iMPA As Integer = 1 To 100
                'we want to store this for 100 levels (%) of protection
                For iC As Integer = 0 To CInt(CellCount * iMPA / 100) - 1
                    MaxLayerSumByLayerAndPctMPA(iL, iMPA) += ArrayVal(iC)
                Next
            Next
        Next iL
    End Sub

    Protected Sub calcImportanceLayersCoverageInRun()
        Dim Data()(,) As Single = Me.m_SpaceData.ImportanceLayerMap
        ReDim LayerSumInMPA(Me.m_SpaceData.nImportanceLayers)

        For iL As Integer = 1 To Me.m_SpaceData.nImportanceLayers
            For iR As Integer = 1 To m_SpaceData.InRow
                For iC As Integer = 1 To m_SpaceData.InCol
                    If m_SpaceData.MPA(m_data.iMPAtoUse)(iR, iC) Then 'this is a protected cell, so check what 
                        LayerSumInMPA(iL) += Data(iL)(iR, iC)
                    End If
                Next iC
            Next iR
        Next iL
    End Sub

#End Region
 
End Class
