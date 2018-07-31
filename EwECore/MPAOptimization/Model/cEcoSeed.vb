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
Imports EwECore
Imports EwECore.cEcoSpace
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports System.IO

Namespace EcoSeed

    'ToDo_jb EcoSeed YearTimeStep() sets iYear to TotalTime
    'ToDo_jb m_data.SeedBlockSize2 EwE5 m_data.SeedBlockSize2 is set by the user to 1,4,9,16 or 25 
    '   SideStep = Sqr(m_data.SeedBlockSize2) then in EvaluateSeedCell m_data.SeedBlockSize2 is set to 9 and SideStep is not reset
    ' it looks like the only value that maters here is SideStep as long as SideStep is set before m_data.SeedBlockSize2 is hardwired at 9
    'this is happening in a temp way in at the start of run

    Public Class cEcoSeed
        Inherits cMPAOptBaseClass

#Region "Private data"

        Const N_MAX_RESULTS As Integer = 100
        Const RESULTS_TO_KEEP As Integer = N_MAX_RESULTS \ 2

        Private TestedSeed(,) As Boolean

        Private SeedLeft As Boolean
        Private MPARow() As Integer
        Private MPACol() As Integer
        Private MPAVal() As Single   ', sum As Single
        Private MPABio() As Single, MPABioInit As Single
        Private MPAcount() As Integer, Tn As Integer ', MpaCnt As Integer ' added here for the pointer to be available in ecoseed abmpa
        Private MPAstep As Integer, ChangeFontClr As Boolean
        Private EffortMPA(,) As Single 'abmpa
        Private SailTot() As Single, SailMax() As Single
        'these next are to scale the value to a Von B type curve abmpa
        Private BOrigTot() As Single
        Private TotalSearchMax As Single
        Private SeedSumMax As Single

        Private SideStep As Integer


#End Region

#Region "Public Properties and Methods"

        Public Overrides ReadOnly Property OKtoRun() As Boolean 'Implements IMPASearchModel.OKtoRun
            Get
                '
                ' Check seeds
                For ir As Integer = 1 To Me.m_SpaceData.InRow
                    For ic As Integer = 1 To Me.m_SpaceData.InCol
                        If m_data.MPASeed(ir, ic) > 0 Then
                            Return True
                        End If
                    Next ic
                Next ir

                ' Check MPAs
                ' Check within MPAs
                For ir As Integer = 1 To Me.m_SpaceData.InRow
                    For ic As Integer = 1 To Me.m_SpaceData.InCol

                        'If m_SpaceData.MPA(ir, ic) > 0 Then
                        '    Return True
                        'End If

                        For impa As Integer = 1 To Me.m_SpaceData.MPAno
                            If m_SpaceData.MPA(impa)(ir, ic) Then
                                Return True
                            End If
                        Next
                    Next ic
                Next ir

                Return False

            End Get
        End Property

#End Region

#Region "Construction and Initialization"

        Public Sub New()
            MyBase.New()
            m_OutputFilename = "MPA_Ecoseed_output.csv"
        End Sub

        'Public Overrides Function Init(ByRef EcoSpaceModel As cEcoSpace, ByRef EcoSeedData As cMPAOptDataStructures) As Boolean 'Implements IMPASearchModel.Init
        '    MyBase.Init(EcoSpaceModel, EcoSeedData)
        '    Try

        '        'the seed array can be needed before the model is run
        '        ReDim m_data.MPASeed(m_SpaceData.InRow + 1, m_SpaceData.InCol + 1)

        '    Catch ex As Exception
        '        cLog.Write(ex)
        '        Return False
        '    End Try

        '    Return True

        'End Function

#End Region

#Region "Running the model"

        Private Sub initForRun()

            Try
                'Ecoseed does not listen to the Ecospace time steps
                Me.m_EcoSpace.TimeStepDelegate = Nothing

                'create a new list to store the results
                Me.m_lstObjectiveResults = New List(Of cObjectiveResult)

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".initForRun() Error: " & ex.Message, ex)
            End Try

        End Sub


        Public Overrides Sub Run() 'Implements IMPASearchModel.Run

            Me.m_bRunning = True
            Me.setRunState(cMPAOptManager.eRunStates.Initializing)

            Me.m_data.StopRun = False
            Try
                Me.runSeed()
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex)
            End Try

            Me.m_bRunning = False
            Me.setRunState(cMPAOptManager.eRunStates.Completed)

        End Sub


        Friend Sub runSeed()
            Dim NotAllCellsAreMPAs As Boolean
            Dim AreaBordary As Single
            'Dim bExitRun As Boolean

            'total objective sum of the current search 
            Dim CurSum As Single
            Dim writer As StreamWriter = Nothing

            Try
                Debug.Assert(m_data IsNot Nothing, "Ecoseed: data not initialized")
                Debug.Assert(m_EcoSpace IsNot Nothing, "Ecoseed: Ecospace not initialized")

                m_search = m_EcoSpace.SearchData

                Me.initForRun()
                Me.InitIsMPA()

                m_data.SeedBlockSize2 = 1

                'ToDo_jb SideStep EwE5 there is no explict cast of SideStep to int figure out if it is rounded or truncated
                SideStep = CInt(Math.Sqrt(m_data.SeedBlockSize2))

                SeedSumMax = Single.MinValue
                TotalSearchMax = Single.MinValue
                m_data.bestrow = -1
                m_data.bestcol = -1
                RedimSeedVariables()

                m_search.SearchMode = eSearchModes.SpatialOpt
                m_search.setMinSearchBlocks()

                getBaseValues()
                System.Console.WriteLine("------------Ecoseed----------------")

                If Me.m_bAutosaveResults And Not String.IsNullOrWhiteSpace(Me.m_strOutputPath) Then
                    If cFileUtils.IsDirectoryAvailable(Me.m_strOutputPath, True) Then
                        Try
                            writer = New StreamWriter(Path.Combine(Me.m_strOutputPath, m_OutputFilename))
                        Catch ex As Exception

                        End Try
                    End If
                End If
                Try
                    WriteOutputFileHeader(writer)
                Catch ex As Exception

                End Try

                Me.setRunState(cMPAOptManager.eRunStates.Searching)

                Me.m_nIters = 0
                NotAllCellsAreMPAs = True
                Do While NotAllCellsAreMPAs

                    'check if all cells are MPAs
                    NotAllCellsAreMPAs = CellsNotMPA()
                    If NotAllCellsAreMPAs = False Then
                        EcoSeedOn = False
                    Else
                        EcoSeedOn = True
                        ReDim TestedSeed(m_SpaceData.InRow, m_SpaceData.InCol)
                    End If

                    'Loop over all the Seed cells and find the one with the highest weighted value
                    Do While EcoSeedOn
                        If m_data.StopRun Then Exit Do

                        'Set the next seed cell to be a MPA cell:
                        'SelectNextSeedCell will set EcoSeedOn 
                        '   True if there is a valid cell to test
                        '   False if all the cells have been evaluated and it is time to set the next batch of SeedCells
                        SelectNextSeedCell()

                        If EcoSeedOn Then

                            fireOnIteration()
                            m_EcoSpace.Run()
                            If m_data.StopRun Then Exit Do

                            CurSum = 0 + m_search.ValWeight(eSearchCriteriaResultTypes.TotalValue) * m_search.totval / TotValBase + _
                                m_search.ValWeight(eSearchCriteriaResultTypes.Employment) * m_search.Employ / EmployBase + _
                                m_search.ValWeight(eSearchCriteriaResultTypes.MandateReb) * m_search.manvalue / ManValueBase + _
                                m_search.ValWeight(eSearchCriteriaResultTypes.Ecological) * m_search.ecovalue / EcoValueBase + _
                                m_search.ValWeight(eSearchCriteriaResultTypes.BioDiversity) * m_search.DiversityIndex / DiversityBase

                            'Calculate boundary length/area ratio
                            AreaBordary = CalculateAreaOverBondaryLength()
                            CurSum = CurSum + AreaBordary * m_data.BoundaryWeight

                            m_data.objFuncEcologicalValue = m_search.ecovalue / EcoValueBase
                            m_data.objFuncMandatedValue = m_search.manvalue / ManValueBase
                            m_data.objFuncSocialValue = m_search.Employ / EmployBase
                            m_data.objFuncEconomicValue = m_search.totval / TotValBase
                            m_data.objFuncBiodiversity = m_search.DiversityIndex / DiversityBase
                            m_data.objFuncAreaBorder = AreaBordary / AreaBoundBase
                            m_data.objFuncTotal = (m_search.WeightedTotal + AreaBordary * m_data.BoundaryWeight) / Me.TotWeightedValueBase

                            If CurSum > SeedSumMax Then

                                m_data.bestrow = m_data.CurRow
                                m_data.bestcol = m_data.CurCol

                                SeedSumMax = CurSum

                                If SeedSumMax > TotalSearchMax Then
                                    TotalSearchMax = SeedSumMax
                                    'new highest score across all the model runs
                                    Me.setRunState(cMPAOptManager.eRunStates.NewBestResultFound)
                                End If

                                'System.Console.WriteLine("m_data.bestrow = " & m_data.bestrow.ToString & ", m_data.bestcol= " & m_data.bestrow.ToString & ", TargetSum = " & TargetSum.ToString)
                            End If

                            'turn the current MPA cell off SelectNextSeedCell() will set the next cell
                            clearCurrentMPATestCells()
                            Me.m_nIters += 1

                        Else
                            'EcoSeedOn = False
                            SelectNewMPAcell()
                        End If

                    Loop ' Do While EcoSeedOn
                    If m_data.StopRun Then Exit Do

                    'All the current seed cells have been tested for the highest weighted value
                    'Add the best row col to the MPA configuration
                    'Select the next set of seed cells to test
                    If m_data.bestrow > 0 And m_data.bestcol > 0 Then

                        StoreObjectiveFunctionResults(writer)

                        'Tell the delegate that a new best cell has been selected. 
                        'this needs to be synchronous because the best row/col are set back to -1 (not selected) right after
                        'synchronization is handled by the manager
                        Me.setRunState(cMPAOptManager.eRunStates.NewCellSelected)

                        'set the MPA cell to the selected Seed cell
                        'm_SpaceData.MPA(m_data.bestrow, m_data.bestcol) = m_data.MPASeed(m_data.bestrow, m_data.bestcol)
                        Dim iMPA As Integer = m_data.MPASeed(m_data.bestrow, m_data.bestcol)
                        m_SpaceData.MPA(iMPA)(m_data.bestrow, m_data.bestcol) = (iMPA > 0)
                        m_data.MPASeed(m_data.bestrow, m_data.bestcol) = 0

                        SeedSumMax = Single.MinValue
                        m_data.bestrow = -1
                        m_data.bestcol = -1

                        Me.InitIsMPA()
                        Me.SetSeedCellsAdjacentToMPAs()

                    Else
                        EcoSeedOn = False
                    End If

                Loop ' Do While NotAllCellsAreMPAs

                fireOnIteration()
                m_EcoSpace.SearchData.SearchMode = eSearchModes.NotInSearch
                cleanUp()

            Catch ex As Exception
                cLog.Write(ex)
                Me.m_bRunning = False
                Debug.Assert(False, ex.StackTrace)
            End Try

            If (writer IsNot Nothing) Then
                writer.Flush()
                writer.Close()
                writer.Dispose()
            End If

        End Sub



        ''' <summary>
        ''' Find the next set of MPA cells to evaluate
        ''' </summary>
        ''' <remarks>In EwE5 this was called EvaluateSeedCell() the original code is at the bottom inactivated by a compiler directive </remarks>
        Private Sub SelectNextSeedCell()

            Dim i As Integer, ir As Integer, ic As Integer, j As Integer

            'If EcoSeedOn Then

            'EcoSeedOn controls the evaluation loop in RunSeed()
            'it tells the loop that we have found the next seed cell/block
            EcoSeedOn = False

            For ir = 1 To m_SpaceData.InRow
                For ic = 1 To m_SpaceData.InCol

                    If m_data.MPASeed(ir, ic) > 0 And TestedSeed(ir, ic) = False Then 'Found one

                        'EcoSeedOn controls the evaluation loop in RunSeed()
                        EcoSeedOn = True

                        m_data.CurRow = SideStep * ((ir - 1) \ SideStep) + 1
                        m_data.CurCol = SideStep * ((ic - 1) \ SideStep) + 1

                        For i = m_data.CurRow To m_data.CurRow + SideStep - 1
                            For j = m_data.CurCol To m_data.CurCol + SideStep - 1
                                If i >= 0 And i <= m_SpaceData.InRow And j >= 0 And j <= m_SpaceData.InCol Then
                                    'has to split the next in two as i or j may exceed dimensioning
                                    If Me.m_SpaceData.Depth(ir, ic) > 0 Then
                                        TestedSeed(i, j) = True

                                        ' m_data.MPASeed(i, j) make sure MPASeed() is set to an MPA index for this row and col
                                        Debug.Assert(m_data.MPASeed(i, j) <> 0, "Ecoseed MPASeed() not set correctly.")

                                        'set the MPA's to use the MPASeed for this row col
                                        'MPASeed(row,col) was set in SetSeedCellsAdjacentToMPAs()
                                        'MPA() will need to be cleared at the end of this iteration
                                        'm_SpaceData.MPA(i, j) = m_data.MPASeed(ir, ic)
                                        m_SpaceData.MPA(m_data.MPASeed(ir, ic))(i, j) = (m_data.MPASeed(ir, ic) > 0)

                                    End If ' If m_esData.Depth(i, j) > 0 Then
                                End If ' If i >= 0 And i <= m_esData.Inrow And j >= 0 And j <= m_esData.InCol Then
                            Next j
                        Next i

                        'done we have found the next seed cell/block
                        Exit Sub

                    End If ' If m_data.MPASeed(ir, ic) > 0 And TestedSeed(ir, ic) = False Then 'Found one
                Next ic
            Next ir
            'End If

            Exit Sub

#If 0 Then
            'EWE5 original code 
            'this was called EvaluateSeedCell()

            m_data.SeedBlockSize2 = 9
            ''next loop finds seed blocks, sets them according to user input seed block size abmpa
            ''If StartMPA And RunningMPA = False Then
            If EcoSeedOn Then
                EcoSeedOn = False
                For ir = 1 To m_esData.Inrow
                    For ic = 1 To m_esData.InCol

                        If EcoSeedOn Then
                            ir = m_esData.Inrow
                            ic = m_esData.InCol
                            Exit For
                        End If

                        If m_data.MPASeed(ir, ic) > 0 And TestedSeed(ir, ic) = False Then 'Found one
                            EcoSeedOn = True
                            Select Case m_data.SeedBlockSize2
                                Case 1
                                    TestedSeed(ir, ic) = True
                                    m_esData.MPA(ir, ic) = m_data.MPASeed(ir, ic)
                                    m_data.CurRow = ir
                                    m_data.CurCol = ic
                                Case 4, 9, 16, 25

                                    m_data.CurRow = SideStep * ((ir - 1) \ SideStep) + 1
                                    m_data.CurCol = SideStep * ((ic - 1) \ SideStep) + 1

                                    For i = m_data.CurRow To m_data.CurRow + SideStep - 1
                                        For j = m_data.CurCol To m_data.CurCol + SideStep - 1
                                            If i >= 0 And i <= m_esData.Inrow And j >= 0 And j <= m_esData.InCol Then
                                                'has to split the next in two as i or j may exceed dimensioning
                                                If m_esData.Depth(i, j) > 0 Then
                                                    TestedSeed(i, j) = True
                                                End If
                                            End If
                                        Next
                                    Next
                            End Select ' Select Case m_data.SeedBlockSize2

                            Exit For

                        End If ' If m_data.MPASeed(ir, ic) > 0 And TestedSeed(ir, ic) = False Then 'Found one
                    Next
                Next
            End If

            Exit Sub


           If 2 = 3 And StartMPA And RunningMPA Then
                Select Case m_data.SeedBlockSize2
                    Case 1
                        m_esData.MPA(m_data.CurRow, m_data.CurCol) = m_data.MPASeed(m_data.CurRow, m_data.CurCol)   ' 1
                        ''m_data.MPASeed(m_data.CurRow, m_data.CurCol) = 0
                        'frmSpace.MapDepth(frmSeed.MPAmap)
                        ''GetFactor1
                        'DoEvents()
                    Case 4, 9, 16, 25
                        For ir = m_data.CurRow To m_data.CurRow + SideStep - 1 : For ic = m_data.CurCol To m_data.CurCol + SideStep - 1
                                If ir <= m_esData.Inrow And ic <= m_esData.InCol Then
                                    If m_data.MPASeed(ir, ic) > 0 Then
                                        m_esData.MPA(ir, ic) = m_data.MPASeed(ir, ic)
                                        m_data.MPASeed(ir, ic) = 0
                                        'frmSpace.MapDepth(frmSeed.MPAmap)
                                        'GetFactor1
                                        m_data.MPASeed(ir, ic) = -1
                                        'DoEvents
                                    End If
                                End If
                            Next : Next
                        For ir = m_data.CurRow To m_data.CurRow + SideStep - 1 : For ic = m_data.CurCol To m_data.CurCol + SideStep - 1
                                If ir <= m_esData.Inrow And ic <= m_esData.InCol Then
                                    If m_data.MPASeed(ir, ic) = -1 Then m_data.MPASeed(ir, ic) = MPA(ir, ic) '1
                                End If
                            Next : Next
                End Select
            End If
#End If
            'If EndMPA And RunningMPA Then
            'SumValSeed = True
            'End If
            'ReDim bbTOT(NumGroups)
            'If MPAstep = 0 Then MPAstep = 1
        End Sub


        Private Sub clearCurrentMPATestCells()
            Dim ir As Integer, ic As Integer

            For ir = m_data.CurRow To m_data.CurRow + SideStep - 1
                For ic = m_data.CurCol To m_data.CurCol + SideStep - 1

                    If ir <= m_SpaceData.InRow And ic <= m_SpaceData.InCol Then

                        If m_data.MPASeed(ir, ic) > 0 Then
                            'm_SpaceData.MPA(ir, ic) = 0
                            m_SpaceData.MPA(m_data.MPASeed(ir, ic))(ir, ic) = False
                        End If

                    End If ' If ir <= m_esData.Inrow And ic <= m_esData.InCol Then

                Next ic
            Next ir

        End Sub

        Private Sub SelectNewMPAcell() 'this occurs just before before start of new timestep
            ' Dim ir As Integer, ic As Integer, i As Integer ', j As Integer

            'MPARow(MPAstep) = m_data.bestrow
            'MPACol(MPAstep) = m_data.bestcol
            'MPAstep = MPAstep + 1
            'Count how many MPA cells we have now
            'i = 0
            'For ir = 1 To m_SpaceData.InRow
            '    For ic = 1 To m_SpaceData.InCol
            '        If Me.IsMPA(ir, ic) Then i = i + 1
            '    Next
            'Next
            'MPAcount(MPAstep - 1) = i

            ' MPAstep = m_SpaceData.InRow * m_SpaceData.InCol + 1

            EcoSeedOn = False
            'villy: this next section only allows 'adjacent' cells to become seed cells _
            '- time saver ordered by daniel AB02242000
            SetSeedCellsAdjacentToMPAs() 's

            'Clear out the TestSeed() matrix
            Me.TestedSeed = New Boolean(m_SpaceData.InRow, m_SpaceData.InCol) {}

        End Sub

        Private Sub SetSeedCellsAdjacentToMPAs()
            Dim iro As Integer
            Dim ico As Integer
            Dim iTemp As Integer

            For iro = 1 To m_SpaceData.InRow
                For ico = 1 To m_SpaceData.InCol
                    If Me.IsMPA(iro, ico) And Me.m_SpaceData.Depth(iro, ico) > 0 Then
                        'get the MPA index of the current row col
                        'this index will be used to set the neighbouring cells
                        'iTemp = m_SpaceData.MPA3D_TEMP(iro, ico, Me.m_data.iMPAtoUse)

                        'find the first mpa in this cell
                        For impa As Integer = 1 To Me.m_SpaceData.MPAno
                            If m_SpaceData.MPA(impa)(iro, ico) Then
                                iTemp = impa
                                Exit For
                            End If
                        Next

                        If Not Me.IsMPA(iro - 1, ico) And Me.m_SpaceData.Depth(iro - 1, ico) > 0 Then m_data.MPASeed(iro - 1, ico) = iTemp 'cell above is m_esdata.m_data.MPASeed
                        If Not Me.IsMPA(iro + 1, ico) And Me.m_SpaceData.Depth(iro + 1, ico) > 0 Then m_data.MPASeed(iro + 1, ico) = iTemp 'cell below is m_esdata.m_data.MPASeed
                        If Not Me.IsMPA(iro, ico - 1) And Me.m_SpaceData.Depth(iro, ico - 1) > 0 Then m_data.MPASeed(iro, ico - 1) = iTemp 'cell left is m_esdata.m_data.MPASeed
                        If Not Me.IsMPA(iro, ico + 1) And Me.m_SpaceData.Depth(iro, ico + 1) > 0 Then m_data.MPASeed(iro, ico + 1) = iTemp 'cell right is m_esdata.m_data.MPASeed

                    End If ' If m_esData.MPA(iro, ico) > 0 Then
                Next ico
            Next iro
        End Sub

#If USE_OLD_MPA Then

        Private Sub SetSeedCellsAdjacentToMPAs()
            Dim ir As Integer
            Dim ic As Integer
            Dim iro As Integer
            Dim ico As Integer
            Dim iTemp As Integer

            For iro = 1 To m_SpaceData.InRow
                For ico = 1 To m_SpaceData.InCol
                    If m_SpaceData.MPA(iro, ico) > 0 Then
                        'get the MPA index of the current row col
                        'this index will be used to set the neighbouring cells
                        iTemp = m_SpaceData.MPA(iro, ico)
                        Select Case m_data.SeedBlockSize2
                            Case 1
                                If m_SpaceData.MPA(iro - 1, ico) = 0 And m_SpaceData.Depth(iro - 1, ico) > 0 Then m_data.MPASeed(iro - 1, ico) = iTemp '1  'cell above is m_esdata.m_data.MPASeed
                                If m_SpaceData.MPA(iro + 1, ico) = 0 And m_SpaceData.Depth(iro + 1, ico) > 0 Then m_data.MPASeed(iro + 1, ico) = iTemp '1 'cell below is m_esdata.m_data.MPASeed
                                If m_SpaceData.MPA(iro, ico - 1) = 0 And m_SpaceData.Depth(iro, ico - 1) > 0 Then m_data.MPASeed(iro, ico - 1) = iTemp '1 'cell left is m_esdata.m_data.MPASeed
                                If m_SpaceData.MPA(iro, ico + 1) = 0 And m_SpaceData.Depth(iro, ico + 1) > 0 Then m_data.MPASeed(iro, ico + 1) = iTemp '1 'cell right is m_esdata.m_data.MPASeed
                            Case 4, 9, 16, 25
                                'Cells above:
                                For ir = iro - SideStep To iro - 1
                                    For ic = ico To ico + SideStep - 1
                                        If ir >= 0 And ir <= m_SpaceData.InRow And ic >= 0 And ic <= m_SpaceData.InCol Then
                                            If m_SpaceData.MPA(ir, ic) = 0 And m_SpaceData.Depth(ir, ic) > 0 Then
                                                m_data.MPASeed(ir, ic) = iTemp '1 'cell above is m_esdata.m_data.MPASeed
                                            End If
                                        End If
                                    Next
                                Next
                                'cells below:
                                For ir = iro + SideStep To iro + 2 * SideStep - 1
                                    For ic = ico To ico + SideStep - 1
                                        If ir >= 0 And ir <= m_SpaceData.InRow And ic >= 0 And ic <= m_SpaceData.InCol Then
                                            If m_SpaceData.MPA(ir, ic) = 0 And m_SpaceData.Depth(ir, ic) > 0 Then
                                                m_data.MPASeed(ir, ic) = iTemp '1 'cell above is m_esdata.m_data.MPASeed
                                            End If
                                        End If
                                    Next
                                Next
                                'cells to the left:
                                For ir = iro To iro + SideStep - 1 : For ic = ico - SideStep To ico - 1
                                        If ir >= 0 And ir <= m_SpaceData.InRow And ic >= 0 And ic <= m_SpaceData.InCol Then
                                            If m_SpaceData.MPA(ir, ic) = 0 And m_SpaceData.Depth(ir, ic) > 0 Then
                                                m_data.MPASeed(ir, ic) = iTemp  '1 'cell above is m_esdata.m_data.MPASeed
                                            End If
                                        End If
                                    Next : Next
                                'cells to the right:
                                For ir = iro To iro + SideStep - 1
                                    For ic = ico + SideStep To ico + 2 * SideStep - 1
                                        If ir >= 0 And ir <= m_SpaceData.InRow And ic >= 0 And ic <= m_SpaceData.InCol Then
                                            If m_SpaceData.MPA(ir, ic) = 0 And m_SpaceData.Depth(ir, ic) > 0 Then
                                                m_data.MPASeed(ir, ic) = iTemp  '1 'cell above is m_esdata.m_data.MPASeed
                                            End If
                                        End If 'If ir >= 0 And ir <= m_esData.Inrow And ic >= 0 And ic <= m_esData.InCol Then
                                    Next ic
                                Next ir
                        End Select
                    End If ' If m_esData.MPA(iro, ico) > 0 Then
                Next ico
            Next iro
        End Sub



        Private Sub SelectNewMPAcell_orgMPALayers() 'this occurs just before before start of new timestep
            Dim ir As Integer, ic As Integer, i As Integer ', j As Integer
            Dim fnum As Integer

            'jb from EwE5 m_data.SeedBlockSize2 is hardwired at 9 at the start of each run
            'so only Case 4, 9, 16, 25 can run I'm not sure what the other case is for
            Select Case m_data.SeedBlockSize2
                Case 1
                    If SeedLeft = True Then
                        fnum = m_SpaceData.MPA3D_TEMP(m_data.CurRow, m_data.CurCol, Me.m_data.iMPAtoUse)
                        m_data.MPASeed(m_data.CurRow, m_data.CurCol) = fnum
                        m_SpaceData.MPA3D_TEMP(m_data.CurRow, m_data.CurCol, Me.m_data.iMPAtoUse) = 0 ' fnum

                        'fnum = m_SpaceData.MPA(m_data.CurRow, m_data.CurCol)
                        'm_data.MPASeed(m_data.CurRow, m_data.CurCol) = m_SpaceData.MPA(m_data.CurRow, m_data.CurCol)   '1
                        'm_SpaceData.MPA(m_data.CurRow, m_data.CurCol) = 0 ' fnum
                    End If
                Case 4, 9, 16, 25
                    For ir = m_data.CurRow To m_data.CurRow + SideStep - 1
                        For ic = m_data.CurCol To m_data.CurCol + SideStep - 1
                            If ir <= m_SpaceData.InRow And ic <= m_SpaceData.InCol Then
                                'If m_SpaceData.Depth(ir, ic) > 0 And m_data.MPASeed(ir, ic) > 0 Then
                                '    m_SpaceData.MPA(ir, ic) = 0
                                'End If

                                If Me.isCellModeled(ir, ic) And m_data.MPASeed(ir, ic) > 0 Then
                                    Me.m_SpaceData.MPA3D_TEMP(ir, ic, Me.m_data.iMPAtoUse) = 0
                                End If

                                'm_data.MPASeed(IR, ic) = 1
                            End If
                        Next ic
                    Next ir
            End Select

            MPARow(MPAstep) = m_data.bestrow
            MPACol(MPAstep) = m_data.bestcol
            MPAstep = MPAstep + 1
            'Count how many MPA cells we have now
            i = 0
            For ir = 1 To m_SpaceData.InRow
                For ic = 1 To m_SpaceData.InCol
                    'If m_SpaceData.MPA(ir, ic) > 0 Then i = i + 1
                    If Me.IsMPA(ir, ic) Then i = i + 1
                    'If m_SpaceData.MPA3D_TEMP(ir, ic, Me.m_data.iMPAtoUse) > 0 Then i = i + 1
                Next
            Next
            MPAcount(MPAstep - 1) = i
            ir = 0

            If ir = m_SpaceData.nFleets Then 'NO MORE FISHING GOING ON
                MPAstep = m_SpaceData.InRow * m_SpaceData.InCol + 1
                'Ecoseed.StartEvaluateSeedCell
            End If
            If SeedLeft = False Then
                'And TargetSumMax <= 0 Then 'VESSELS CANT MAKE ANY MONEY
                'MsgBox "The Ecoseed routine can no longer add MPA cells:" _
                '& Environment.NewLine  + "the rent(s) for all fishery(ies) are =< 0." _
                '& Environment.NewLine  + "The Ecospace routine will now continue without Ecoseed.", vbInformation + vbOKOnly
                EcoSeedOn = False
                MPAstep = m_SpaceData.InRow * m_SpaceData.InCol + 1
            End If

            'villy: this next section only allows 'adjacent' cells to become seed cells _
            '- time saver ordered by daniel AB02242000
            SetSeedCellsAdjacentToMPAs() 's

            Erase TestedSeed
            ReDim TestedSeed(m_SpaceData.InRow, m_SpaceData.InCol)

        End Sub



#End If

#End Region

#Region "Memory Managment"

        Private Overloads Sub cleanUp()
            MyBase.cleanUp()

            Erase MPARow
            Erase MPACol
            Erase EffortMPA
            Erase MPAcount

        End Sub

        Private Overloads Sub RedimSeedVariables()
            MyBase.RedimSeedVariables()

            Dim nvartot As Integer = m_SpaceData.NGroups + 2

            ReDim MPAcount(m_SpaceData.InRow * m_SpaceData.InCol + 1)
            ReDim MPARow(m_SpaceData.InRow * m_SpaceData.InCol + 1)
            ReDim MPACol(m_SpaceData.InRow * m_SpaceData.InCol + 1)

        End Sub
#End Region

    End Class

#Region "Dead code from EwE5"


#If 0 Then

        Public Sub KeepOrReloadCellValues(ByVal biomass() As Single)
            Dim i As Integer, j As Integer, ip As Integer
            'these are not being kept properly ab02182000
            'TimesCalled is reinitialized for each timestep

            'ToDo_jb KeepOrReloadCellValues WchangeVar() is only in the ecospace threads 
            'If this really needs to happen it needs to get copied out of the threads then copied back in?????
            Try
                If TimesCalled = 1 Then 'First time keep the original bcell values

                    For i = 1 To m_SpaceData.InRow
                        For j = 1 To m_SpaceData.InCol
                            For ip = 1 To m_SpaceData.NGroups
                                BOrig(i, j, ip) = m_SpaceData.Bcell(i, j, ip)
                                FOrig(i, j, ip) = m_EcoSpace.FtimeCell(i, j, ip)
                                Blastseed(i, j, ip) = m_SpaceData.Blast(i, j, ip)
                            Next
                        Next
                    Next
                    'Btime is needed when running Ecoseed
                    For i = 1 To m_SpaceData.NGroups
                        StoreBtimeForEcoSeed(i) = biomass(i)
                    Next
                End If

                If TimesCalled >= 2 Then 'second time recalls the original bcell values for each timestep
                    For i = 1 To m_SpaceData.InRow
                        For j = 1 To m_SpaceData.InCol
                            For ip = 1 To m_SpaceData.NGroups
                                m_SpaceData.Blast(i, j, ip) = Blastseed(i, j, ip)
                                m_SpaceData.Bcell(i, j, ip) = BOrig(i, j, ip)
                                m_EcoSpace.FtimeCell(i, j, ip) = FOrig(i, j, ip)
                            Next
                        Next
                    Next
                    For i = 1 To m_SpaceData.NGroups
                        biomass(i) = StoreBtimeForEcoSeed(i)
                    Next
                End If

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.StackTrace)
                Throw New ApplicationException("EcoSeed.KeepOrReloadCellValues() error: " & ex.Message, ex)
            End Try

        End Sub
#End If

#End Region

End Namespace
