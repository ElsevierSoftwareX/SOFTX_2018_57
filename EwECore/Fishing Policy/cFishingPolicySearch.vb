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

'Option Explicit On
Option Strict On

Imports EwECore.Ecosim
Imports EwECore.SearchObjectives
Imports EwEUtils.Core


Imports EwEPlugin


Namespace FishingPolicy

    ''' <summary>
    ''' A Fishing Policy search has completed all it's runs.
    ''' </summary>
    ''' <remarks>If there are multiple runs they have all been completed or an Error has occured and the runs could not be completed.</remarks>
    Public Delegate Sub SearchCompletedDelegate()

    ''' <summary>
    ''' A run of the Fishing Policy search has completed.
    ''' </summary>
    ''' <remarks></remarks>
    Public Delegate Sub RunCompletedDelegate()

    ''' <summary>
    ''' A Fishing Policy Search run has started.
    ''' </summary>
    ''' <remarks>When this is called the results object will be initialized and dimensioned but it will not contain any values.</remarks>
    Public Delegate Sub RunStartedDelegate()

    ''' <summary>
    ''' Progress of the current Fishing Policy run
    ''' </summary>
    ''' <remarks>The Results object will contain the results of the current interation</remarks>
    Public Delegate Sub ProgressDelegate()

    Public Delegate Sub AddMessageDelegate(ByRef message As cMessage)

#Region "Fishing Policy Search model"

    Public Class cFishingPolicySearch

        'ToDo_jb cFishingPolicySearch What is the message from EwE5 in UseCostPenalty() should this change the InitOption if it fails the test


#Region "Public variables"

        Public SearchCompletedCallBack As SearchCompletedDelegate
        Public RunCompletedCallBack As RunCompletedDelegate
        Public SearchStartedCallBack As RunStartedDelegate
        Public AddMessageCallBack As AddMessageDelegate
        Public ProgressCallBack As ProgressDelegate

        Public Results As cFPSSearchResults

        Public MaxRuns As Integer
        Public PrintOn As Boolean
        Public TotalTime As Integer
        Public TotValBase As Double
        Public EmployBase As Double
        Public ManValueBase As Double
        Public EcoValueBase As Double
        Public ExistValue As Single
        Public BioDivBase As Single

        ''' <summary>
        ''' Force a running search to exit
        ''' </summary>
        ''' <remarks></remarks>
        Public SearchFailed As Boolean
        Public StopEstimation As Boolean

        'Count of the current run at the start of a run
        'the first run will be one
        Public iRun As Integer
#End Region

#Region "Private modeling variables"


        Private Resline As Integer
        Private CritValue(cSearchDatastructures.N_CRIT_RESULTS) As Single
        'Dim X() As Double
        Private G() As Double, Xm() As Double ', Nam$(Nmax)
        Private H() As Double, W() As Double    'was 1000 when nmax was 100
        'Dim X(Nmax) As Double, G(Nmax) As Double, Xm(Nmax) As Double, Nam$(Nmax)
        'Dim H(10000) As Double, W(10000) As Double    'was 1000 when nmax was 100
        Private ColrNo() As Long
        Private VlocalPenalty As Double
        Private MaxNoOfIterations As Integer
        Private ifn As Integer

        Dim ncom As Integer, pcom(50) As Double, xicom(50) As Double

        Dim PaidToJbyI(,) As Single
        Dim Profitability() As Single

        'used by SearchForBaseProfitability
        Dim PropToPlaintiff As Single 'this never get set to anything other than zero


#End Region

#Region "Private Core variables"

        Private m_core As cCore
        Private m_ecosim As cEcoSimModel
        Private m_searchData As cSearchDatastructures
        Private m_pluginManager As cPluginManager

#End Region

#Region "Construction and Initialization"

        Public Sub New()

        End Sub

        Friend Sub init(ByRef theCore As cCore)

            Debug.Assert(theCore IsNot Nothing, Me.ToString & ".init(cCore) cCore must not be NULL.")

            Try

                m_core = theCore
                m_ecosim = m_core.m_EcoSim
                m_searchData = m_core.m_SearchData

                m_pluginManager = Me.m_core.PluginManager

                MaxNoOfIterations = 2000 'from EwE5 frmSim1.load() why it is intialized in ecosim I have no idea
                m_searchData.InitOption = eInitOption.EcopathBaseF
                m_searchData.SearchMethod = eSearchOptionTypes.Fletch

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException(Me.ToString & ".init(cCore) Failed to initialize.", ex)
            End Try

            ' Fire plug-in point
            If (Me.m_pluginManager IsNot Nothing) Then
                Me.m_core.m_SearchData.SearchMode = eSearchModes.FishingPolicy
                Me.m_pluginManager.SearchInitialized(Me.m_searchData)
                Me.m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch
            End If

        End Sub

#End Region

#Region "Running the search"

        Public Sub Run()

            'run the model
            Try
                runSearch()
            Catch ex As Exception
                'add a message to the manager
                addMessage(ex.Message)
            End Try

            If SearchCompletedCallBack IsNot Nothing Then
                SearchCompletedCallBack()
            End If

        End Sub


        Private Sub runSearch()
            '            Hi Villy if econ is 'net economic value' it is totval calculated in cSearchDatastructures.EcosimSummarizeIndicators()
            '[14:46:10] Joe Buszowski says: if econ is Ecosystem structure then it is ecovalue calculated in cSearchDatastructures.calcYearlySummaryValues()
            '[14:48:52] Joe Buszowski says: The actual values that appear in the interface are calculated in cFishingPolicySearch.FUNC()

            Try

                Dim nBlocksUsed As Integer

                SearchFailed = False
                StopEstimation = False

                m_searchData = m_core.m_SearchData
                m_searchData.SearchMode = eSearchModes.FishingPolicy 'make sure the search is turned on this will also set some default values based on the flag
                m_searchData.initForRun(m_core.m_EcoPathData, m_core.m_EcoSimData)

                TotalTime = m_core.nEcosimYears
                m_searchData.redimForRun()

                m_searchData.setLimitFishingMortality()

                'In EwE5
                'BaseYear can be zero in the interface however once the zero baseyear is used to set the searchblocks it is set back to one
                'EwE5 Code frmOptf.CmdSearch_Click()
                'FblockCode(ifleet, BaseYear) = 0 'set baseyear in search blocks to zero
                'If BaseYear <= 0 Then BaseYear = 1'make sure baseyear is not zero 
                'If BaseYear is zero this allows the optimization to vary the baseyear but still get the baseyear values from one
                'In EwE6 we constrain baseyear 1 to nEcosimYears right from the start
                If m_searchData.BaseYear < 1 Then
                    m_searchData.BaseYear = 1
                End If

                If m_searchData.BaseYear > m_core.nEcosimYears Then
                    m_searchData.BaseYear = m_core.nEcosimYears
                End If

                Me.m_searchData.bBaseYearSet = False

                'get the number of blocks, sets ParNumber() and BlockNumber()
                nBlocksUsed = m_searchData.SetFletchPars()

                'setting the number of blocks will set Frates to default values, for the new number of blocks
                m_searchData.nBlocks = nBlocksUsed

                'set a new results object with the number of blocks
                Results = New cFPSSearchResults(m_searchData.nBlocks, m_searchData.NumFleets)

                'the length of cSearchDataStructures.BlockNumber can/will be greater then cFPSSearchResults.BlockNumber
                'see SetFletchPars for why this is
                Debug.Assert(m_searchData.BlockNumber.Length >= Results.BlockNumber.Length, Me.ToString & " Number of search blocks is to big. This is a bug!")
                'copy the BlockNumber set in SetFletchPars into the results object
                Array.Copy(m_searchData.BlockNumber, Results.BlockNumber, Results.BlockNumber.Length)

                m_searchData.saveInitialFishingRate(m_core.m_EcoSimData)

                m_ecosim.Init(False)

                checkUseCostPenalty(nBlocksUsed)

                'set Frates() for base values for the different Search Initialization Options
                'Ecopath, Current and Random
                Dim baseFrate As Single
                If m_searchData.InitOption = eInitOption.EcopathBaseF Then
                    'EwE5 base Frates is always zero for Base values this may be a bug but it's copied here
                    baseFrate = 0
                Else
                    'Current F's or Random F's
                    baseFrate = 0.01
                End If

                For i As Integer = 1 To m_searchData.nBlocks
                    m_searchData.Frates(i) = baseFrate
                Next

                m_searchData.setMaxEffort(nBlocksUsed)
                'get the base values for the objective function by running ecosim 
                getBaseValues(nBlocksUsed)

                For Iter As Integer = 1 To m_searchData.nRuns
                    If SearchFailed Or StopEstimation Then
                        Exit For
                    End If

                    'set the fishing rate to initial values (Frates(nBlocks)) base on the initialization option (m_searchData.InitOption )
                    m_searchData.restoreSavedFishingRates()

                    'set maxEffort base on the initial fishing rates, maxEffort is used to constrain the fishing rates
                    m_searchData.setMaxEffort(nBlocksUsed)

                    'tell the world that a search 'Run' has started info about the run is available via properties of the manager and results objects
                    SearchStarted(Iter)

                    Minimize(nBlocksUsed, m_searchData.Frates, m_searchData.SearchMethod)

                    If RunCompletedCallBack IsNot Nothing Then
                        RunCompletedCallBack()
                    End If

                Next Iter

            Catch ex As Exception
                cLog.Write(ex)
                Throw New ApplicationException("Error running Fishing Policy Search.", ex)
            End Try

            ' Done
            If Me.m_pluginManager IsNot Nothing Then
                Me.m_core.m_SearchData.SearchMode = eSearchModes.FishingPolicy
                Me.m_pluginManager.SearchCompleted(Me.m_searchData)
                Me.m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch
            End If

        End Sub

        Private Sub getBaseValues(ByVal nBlocksUsed As Integer)

            If Me.m_pluginManager IsNot Nothing Then
                Me.m_pluginManager.SearchIterationsStarting()
            End If

            Me.m_ecosim.bStopRunning = False

            'get the base values used by FUNC to tell the change between the current run and the base run
            m_ecosim.RunModelValue(TotalTime, m_searchData.Frates, nBlocksUsed)

            If Me.m_pluginManager IsNot Nothing Then
                Me.m_pluginManager.PostRunSearchResults(Me.m_searchData)
            End If

            TotValBase = m_searchData.totval
            EmployBase = m_searchData.Employ
            ManValueBase = m_searchData.manvalue
            EcoValueBase = m_searchData.ecovalue
            BioDivBase = m_searchData.DiversityIndex

            If TotValBase = 0 Then TotValBase = 1
            If TotValBase < 0 Then TotValBase = -TotValBase
            If EmployBase = 0 Then EmployBase = 1
            If EmployBase < 0 Then EmployBase = -EmployBase
            If ManValueBase = 0 Then ManValueBase = 1
            If EcoValueBase = 0 Then EcoValueBase = 1
            If BioDivBase = 0 Then BioDivBase = 1

        End Sub

        Private Sub SearchStarted(ByVal iIteration As Integer)

            Try

                iRun = iIteration
                'clear out the results from the last run
                Results.Clear()

                If SearchStartedCallBack IsNot Nothing Then
                    SearchStartedCallBack()
                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub

#End Region

#Region "Message handling"


        Private Sub addMessage(ByVal strMessage As String, Optional ByVal msgType As eMessageType = eMessageType.ErrorEncountered, Optional ByVal msgImportance As eMessageImportance = eMessageImportance.Critical)

            Try

                addMessage(New cMessage(strMessage, msgType, eCoreComponentType.EcoSim, msgImportance))

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, Me.ToString & ".addMessage() Error: " & ex.Message)
            End Try

        End Sub


        Private Sub addMessage(ByRef msg As cMessage)

            Debug.Assert(AddMessageCallBack IsNot Nothing, Me.ToString & " Missing AddMessageCallBack().")

            Try
                If AddMessageCallBack IsNot Nothing Then
                    AddMessageCallBack(msg)
                End If
            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, Me.ToString & ".addMessage() Error: " & ex.Message)
            End Try

        End Sub

        Sub printstats(ByVal Xtime As Double, ByVal itn As Integer, ByVal ifn As Integer, ByVal F As Double, ByVal n As Integer, ByVal X() As Double, ByVal G() As Double)

            Try

                'process any data for output
                Results.nCalls = ifn

                For iblk As Integer = 1 To n
                    If X(iblk) < Math.Log(Me.m_searchData.MaxEffort) Then
                        Results.BlockResults(iblk) = CSng(Math.Exp(X(iblk)))
                    Else
                        Results.BlockResults(iblk) = 60
                    End If
                Next

                Dim WeightCorrection As Single
                If m_searchData.ValWeight(1) + m_searchData.ValWeight(2) + m_searchData.ValWeight(3) + m_searchData.ValWeight(4) > 0 Then
                    WeightCorrection = m_searchData.ValWeight(1) + m_searchData.ValWeight(2) + m_searchData.ValWeight(3) + m_searchData.ValWeight(4)
                End If
                If WeightCorrection <= 0 Then WeightCorrection = 1

                Results.Totals = CSng((-F + VlocalPenalty) / WeightCorrection)

                For icrit As Integer = 1 To cSearchDatastructures.N_CRIT_RESULTS
                    Results.CriteriaValues(icrit) = CritValue(icrit)
                Next

                If ProgressCallBack IsNot Nothing Then
                    ProgressCallBack()
                End If

#If DEBUG Then
                ''debuging output
                'System.Console.WriteLine("FPS iterations: " & Results.nCalls.ToString)
                'For icr As Integer = 1 To cSearchDatastructures.N_CRIT_RESULTS
                '    System.Console.Write(Results.CriteriaValues(icr).ToString & ", ")
                'Next

                'For iblk As Integer = 1 To n
                '    System.Console.Write(Math.Exp(X(iblk)).ToString & ", ")
                'Next
                'System.Console.WriteLine()
#End If


            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, Me.ToString & ".printstats() Error: " & ex.Message)
            End Try

            ''defint I-N
            ''defdbl A-H, O-Z
            'Dim lines As Integer
            'Dim FormStr As String
            'Dim WeightCorrection As Single
            'If ValWeight(1) + ValWeight(2) + ValWeight(3) + ValWeight(4) > 0 Then
            '    WeightCorrection = ValWeight(1) + ValWeight(2) + ValWeight(3) + ValWeight(4)
            'End If
            'If WeightCorrection <= 0 Then WeightCorrection = 1

            ''If PrintOn = True Then frmOptF.Res.Print itn; " evals "; ifn; " ";
            'If itn = 0 Then
            '    For lines = 1 To n : optVal(lines, 0) = 1 : Next
            'End If
            'If PrintOn = True Then 'frmOptF.Res.Print "func: "; Format(-f, "####.##"); " ";
            '    frmOptF.vaRes.maxRows = frmOptF.vaRes.maxRows + 1
            '    ReDim Preserve optVal(n, frmOptF.vaRes.maxRows - 1)
            '    SetBlock(frmOptF.vaRes, 0, 0, frmOptF.vaRes.maxCols, frmOptF.vaRes.maxRows)
            '    frmOptF.vaRes.TypeHAlign = 2
            '    frmOptF.vaRes.BlockMode = False
            '    SetCellValue(frmOptF.vaRes, 1, frmOptF.vaRes.maxRows, Format(ifn, "###0"))
            '    SetCellValue(frmOptF.vaRes, 2, frmOptF.vaRes.maxRows, Format((-F + VlocalPenalty) / WeightCorrection, GenNum))
            '    'frmOptF.Res.Print "Fs: ";
            '    For i = 1 To 4
            '        'If CritValue(i) < 0 Then Stop
            '        If CritValue(i) > 1000 Then
            '            SetCellText(frmOptF.vaRes, 2 + i, frmOptF.vaRes.maxRows, "")
            '        Else
            '            SetCellValue(frmOptF.vaRes, 2 + i, frmOptF.vaRes.maxRows, Format(CritValue(i), GenNum))
            '        End If
            '    Next
            '    For i = 1 To n
            '        'frmOptF.Res.Print Format(Exp(X(i)), "#.#"); "  ";
            '        '061129VC: the maxeffort is to make it possible to go beyond the max 60x effort
            '        'we've had to do that for models starting in 1950 where effort was 200x by 2003
            '        'MaxEffort = if(frmOptF.Option1(0).value, 5 * Frates(i), Log(60))
            '        'If MaxEffort < 60 Then MaxEffort = 60
            '        FormStr = if(X(i) > 4.6, "0", GenNum)  'no decimals if bigger than 100
            '        If X(i) < Log(MaxEffort) Then   '3.4011 Then   'exp(4.1)=60
            '            SetCellValue(frmOptF.vaRes, i + 6, frmOptF.vaRes.maxRows, Format(Exp(X(i)), FormStr))
            '            optVal(i, frmOptF.vaRes.maxRows - 1) = Exp(X(i))
            '        Else
            '            If frmOptF.chkBatch Then
            '                SetCellValue(frmOptF.vaRes, i + 6, frmOptF.vaRes.maxRows, 60)
            '            Else
            '                SetCellText(frmOptF.vaRes, i + 6, frmOptF.vaRes.maxRows, ">60")
            '            End If
            '            optVal(i, frmOptF.vaRes.maxRows - 1) = 60
            '        End If
            '        frmOptF.vaRes.BackColor = ColrNo(i)
            '    Next i
            '    If frmOptF.chkBatch Then
            '        SetCellValue(frmOptF.vaRes, i + 6, frmOptF.vaRes.maxRows, Format(ValWeight(1), GenNum))
            '        SetCellValue(frmOptF.vaRes, i + 7, frmOptF.vaRes.maxRows, Format(ValWeight(2), GenNum))
            '        SetCellValue(frmOptF.vaRes, i + 8, frmOptF.vaRes.maxRows, Format(ValWeight(3), GenNum))
            '        SetCellValue(frmOptF.vaRes, i + 9, frmOptF.vaRes.maxRows, Format(ValWeight(4), GenNum))
            '    Else
            '        frmOptF.UpdatePlot(CritValue())
            '    End If
            '    lines = frmOptF.vaRes.Height / 275 + 1
            '    If frmOptF.vaRes.maxRows >= lines Then frmOptF.vaRes.TopRow = frmOptF.vaRes.maxRows - lines + 2
            'End If
            'If StopEstimation = True Then SearchFailed = True : frmOptF.MousePointer = vbDefault : DoEvents()
        End Sub


#End Region

#Region "Private modeling code"


        Private Sub checkUseCostPenalty(ByVal nSearchBlocks As Integer)
            '  Dim TempTotVal As Double, TempEmploy As Double, TempManVal As Double, TempEcoVal As Double

            'jb Logic copied from EwE5 I'm not sure what the point of this 
            'it tells the user it is resetting the InitOption to Ecopath F's but it never resets InitOption flag
            If m_searchData.InitOption <> eInitOption.EcopathBaseF Then

                m_ecosim.RunModelValue(TotalTime, m_searchData.Frates, nSearchBlocks)

                For iflt As Integer = 1 To m_searchData.NumFleets
                    If m_searchData.CostRatio(iflt) > 1.15 And m_searchData.UseCostPenalty = True Then
                        'EwE5 message
                        'MsgBox("Cost exceeds income for fleet " + m_core.m_EcoPathData.FleetName(iflt) + " so initial fishing efforts violate earnings > cost constraint; restarting with Ecopath base efforts", vbOKOnly, "Ecosim policy search")

                        addMessage(New cFeedbackMessage("Cost exceeds income for fleet " + m_core.m_EcoPathData.FleetName(iflt) + _
                                        " so initial fishing efforts violate earnings > cost constraint; restarting with Ecopath base efforts", _
                                        eCoreComponentType.EcoSim, eMessageType.Any, eMessageImportance.Critical))
                        Exit For
                    End If  'Villy: Carl had introduced the clause above, omitting the calculation of basevalues
                Next

            End If

        End Sub


        Sub Minimize(ByVal n As Integer, ByVal X() As Double, ByVal SearchMethod As eSearchOptionTypes)
            'Sub Minimize(ByVal n As Integer, ByVal X() As Double, ByVal SearchMethod As Integer, ByVal ColorN() As Long, ByVal CritVa() As Single)
            '****************   NOTE TO FLETCH USERS   *****************************

            '       To use this program for fitting data to models, you must do the
            '       following:
            '         (1) Modify lines below as noted to name variables used by your
            '             model and data used in fitting, to set initial values
            '             for your parameter estimates x(1)...x(n), and number of
            '             parameters n to be estimated.
            '         (2) Fill in the subroutine called ReadData to name and read in
            '             any data that you want to use in the fitting; note your
            '             data variable name(s) must be declared in the SHARED statement
            '             below in this mainline program.  Note also that you cannot put
            '             DATA and READ statements in that subroutine; if you want to use
            '             that input approach, such statements must be in this mainline
            '             program, just below this starred instruction section.
            '         (3) Fill in the subroutine called func to generate your model
            '             predicted values and the value of the fitting criterion (eg,
            '             value of the sum of squares of deviations) given any values
            '             passed to the subroutine for the parameters x(1),...,x(n)
            '             by the Fletch subroutine; Fletch will call func with various
            '             values of the x's during its search for the x values that will
            '             minimize the fitting criterion.  Note that the last line in
            '             your func subroutine must be of the form func=xxx, where xxx is
            '             the calculated value of your fitting criterion.
            '         (4) the last call to func (after fitting is finished) will be with
            '             a variable called iprintresid set equal to 1 (it will be 0 for
            '             all other calls to func).  You might want to set up func so
            '             that it prints the observed and predicted values (or residuals)
            '             if iprintresid=1, perhaps to a file so that you can plot them.
            '             Alternatively, you might want to create a subroutine to print or
            '             plot them; in this case, call that routine right at the end of
            '             this main program.
            '
            '***********************************************************************



            'dimension variables to be passed between ReadData and Func here
            'replace example statements with your own variables
            'Dim x, y, z
            'Dim a, B, c(100)

            'ReDim X(n) As Double
            Try

                ReDim G(n), Xm(n) ', Nam$(Nmax)
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Dimensioning strangeness
                'In flet H() dimensions are accessed in two ways 
                'Via NN which =             
                'Np = n + 1
                'NN = n * Np / 2
                'and via ib + i
                'iv = n + n
                'ib = iv + n
                'for example
                'For i = 1 To n
                '    W(ib + i) = temp * Sig / Z
                'Next i

                Dim ndims As Integer
                ndims = n * (n + 1) \ 2
                If ndims < n * 4 Then
                    ndims = n * 4
                End If
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'EwE5 code
                'ReDim H(UBound(X) ^ 2 / 2 + 2), W(UBound(X) ^ 2 / 2 + 2)  'was 1000 when nmax was 100
                ReDim H(ndims), W(ndims)

                '061205VC: I had a model where the W would go out of bounds and bomb the optimization. it was with x = 3 letting
                'the expression be = 5.5, which caused it to bomb when the index was 6 !, so now adding 2 instead of 1 as before

                Dim F As Double, StepSize As Double, eps As Double, mode As Integer, maxfn As Integer, iprint As Integer, iexit As Integer
                Dim dfn As Double, iter As Integer, Gtol As Double
                Dim i As Integer, Estfn As Double

                'do not mess with the following parameters-used by Fletch
                StepSize = 0.0001
                eps = 0.000001
                Gtol = 0.0000000001
                mode = 1
                maxfn = MaxNoOfIterations
                iprint = 1
                For i = 1 To X.Length - 1 : Xm(i) = X(i) : Next

                Estfn = FUNC(X, n)
                printstats(0.0, 0, 0, Estfn, n, X, G)

                If SearchMethod = eSearchOptionTypes.Fletch Then
                    flet(F, X, n, G, H, dfn, Xm, StepSize, eps, mode, m_searchData.nInterations, iprint, W, iexit)
                ElseIf SearchMethod = eSearchOptionTypes.DFPmin Then
                    DFPmin(X, n, Gtol, iter, ifn, F)
                ElseIf SearchMethod = eSearchOptionTypes.BaseProfitability Then 'search for base profitability
                    SearchForBaseProfitability(X, n)
                End If

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.Message)
                Throw New ApplicationException("Minimize() Error: " & ex.Message, ex)
            End Try

        End Sub

        Sub flet(ByVal F As Double, ByVal X() As Double, ByVal n As Integer, ByVal G() As Double, ByVal H() As Double, ByVal dfn As Double, ByVal Xm() As Double, _
                     ByVal hh As Double, ByVal eps As Double, ByVal mode As Integer, ByVal maxfn As Integer, ByVal iprint As Integer, ByVal W() As Double, ByVal iexit As Integer)
            '      subroutine flet(f,x,n,g,h,dfn,xm,hh,eps,
            '     *                 mode,maxfn,iprint,w,iexit,func,*)
            '      implicit real*8 (a-h,o-z)
            '      dimension x(20),g(20),h(100),w(100),xm(20)

            Dim NN As Integer
            Dim llog As Integer
            Dim Np As Integer
            Dim N1 As Integer
            Dim iss As Integer
            Dim iu As Integer
            Dim iv As Integer

            Dim ib As Integer
            Dim idiff As Integer
            Dim ij As Integer
            Dim i As Integer
            Dim j As Integer
            Dim Z As Double
            Dim i1 As Integer
            Dim zz As Double
            Dim jk As Integer
            Dim ik As Integer
            Dim K As Integer
            Dim dmin As Double
            Dim itn As Integer
            Dim link As Integer
            Dim itime As Integer
            Dim Xtime As Double
            Dim gs0 As Double
            Dim aeps As Double
            Dim Alpha As Double
            Dim FF As Double
            Dim tot As Double
            Dim intt As Integer
            Dim DF As Double
            Dim f1 As Double
            Dim f2 As Double
            Dim dgs As Double
            Dim gys As Double
            Dim Sig As Double
            Dim temp As Double

            SearchFailed = False
            Resline = 0
            llog = 1
            Np = n + 1
            N1 = n - 1
            NN = n * Np \ 2
            iss = n
            iu = n
            iv = n + n
            ib = iv + n
            idiff = 1
            iexit = 0
            If mode = 3 Then GoTo pta
            If mode = 2 Then GoTo ptb
            ij = CInt(NN + 1)

            For i = 1 To n
                For j = 1 To i
                    ij = ij - 1
                    H(ij) = 0
                Next j
                H(ij) = 1
            Next i

            GoTo pta

ptb:        '    continue
            ij = 1
            For i = 2 To n
                Z = H(ij)
                If Z <= 0.0! Then GoTo Ptc
                ij = ij + 1
                i1 = ij
                For j = 1 To n
                    zz = H(ij)
                    H(ij) = H(ij) / Z
                    jk = ij
                    ik = i1
                    For K = i To j
                        jk = jk + Np - K
                        H(jk) = H(jk) - H(ik) * zz
                        ik = ik + 1
                    Next K
                    ij = ij + 1
                Next j
            Next i

            If H(ij) <= 0.0! Then GoTo Ptc
pta:        '    continue

            ij = Np
            dmin = H(1)

            For i = 2 To n
                If H(ij) <= dmin Then
                    dmin = H(ij)
                End If
                ij = ij + Np - i

            Next i

            If dmin <= 0.0! Then GoTo Ptc
            Z = F
            itn = 0
            F = FUNC(X, n) : If SearchFailed = True Then Exit Sub
            ifn = 1
            DF = dfn
            If (dfn = 0.0!) Then DF = F - Z
            If (dfn < 0.0!) Then DF = Math.Abs(DF * F)
            If (DF <= 0.0!) Then DF = 1
pte:        ' continue
            For i = 1 To n
                W(i) = X(i)
            Next i
            link = 1

            If (idiff - 1) > 0 Then
                GoTo 110
            Else
                GoTo 100
            End If

18:         '    continue
            If (ifn >= maxfn) Then GoTo 90
20:         '    continue
            If (iprint = 0) Then GoTo 21
            If (itn = 0) Then GoTo 7000
            ' skip for now      IF (amod(itn, iprint) <> 0) THEN GOTO 21
            If (llog = 1) Then GoTo 7010
7003:       itime = 1
            Xtime = itime / 1000.0!
            'If MaxRuns = 1 Then
            printstats(Xtime, itn, ifn, F, n, X, G)
            '  If frmOptF.chkBatch.value = False Then Call printstats(Xtime, itn, ifn, F, n, X(), G())
            '    printstats(Xtime, itn, ifn, F, n, X, G)
21:         itn = itn + 1
            W(1) = -G(1)

            For i = 2 To n
                ij = i
                i1 = i - 1
                Z = -G(i)
                For j = 1 To i1
                    Z = Z - H(ij) * W(j)
                    ij = ij + n - j
                Next j
                W(i) = Z
            Next i

            W(iss + n) = W(n) / H(NN)
            ij = NN

            For i = 1 To N1
                ij = ij - 1
                Z = 0
                For j = 1 To i
                    Z = Z + H(ij) * W(iss + Np - j)
                    ij = ij - 1
                Next j
                W(iss + n - i) = W(n - i) / H(ij) - Z
            Next i

            Z = 0
            gs0 = 0

            For i = 1 To n
                If (Z * Xm(i) >= Math.Abs(W(iss + i))) Then GoTo 29
                Z = Math.Abs(W(iss + i)) / Xm(i)
29:             gs0 = gs0 + G(i) * W(iss + i)
            Next i

            iexit = 2
            If (gs0 >= 0.0!) Then GoTo 92
            aeps = eps / Z
            Alpha = -2 * DF / gs0
            If (Alpha > 1) Then Alpha = 1
            FF = F
            tot = 0
            intt = 0
            iexit = 1
30:         '    continue
            If (ifn >= maxfn Or Alpha < 1.0E-20) Then GoTo 90

            For i = 1 To n
                W(i) = X(i) + Alpha * W(iss + i)
            Next i

            f1 = FUNC(W, n) : If SearchFailed = True Then Exit Sub
            ifn = ifn + 1
            If (f1 >= F) Then GoTo 40
            f2 = F
            tot = tot + Alpha
32:         '    continue

            For i = 1 To n
                X(i) = W(i)
            Next i

            F = f1
            If intt - 1 > 0 Then GoTo 50
            If intt - 1 = 0 Then GoTo 49

35:         '   continue
            If (ifn >= maxfn Or Alpha < 1.0E-20) Then GoTo 90

            For i = 1 To n
                W(i) = X(i) + Alpha * W(iss + i)
            Next i

            f1 = FUNC(W, n) : If SearchFailed = True Then Exit Sub
            ifn = ifn + 1
            If (f1 >= F) Then GoTo 50
            If (f1 + f2 >= F + F And 7 * f1 + 5 * f2 > 12 * F) Then intt = 2
            tot = tot + Alpha
            Alpha = 2 * Alpha
            GoTo 32
40:         '    continue
            If (Alpha < aeps) Then
                GoTo 92
            End If

            If (ifn >= maxfn) Then GoTo 90
            Alpha = 0.5 * Alpha

            For i = 1 To n
                W(i) = X(i) + Alpha * W(iss + i)
            Next i

            f2 = FUNC(W, n) : If SearchFailed = True Then Exit Sub
            ifn = ifn + 1
            If (f2 >= F) Then GoTo 45
            tot = tot + Alpha
            F = f2

            For i = 1 To n
                X(i) = W(i)
            Next i

            GoTo 49
45:         '   continue
            Z = 0.1
            If (f1 + F > f2 + f2) Then Z = 1 + 0.5 * (F - f1) / (F + f1 - f2 - f2)
            If (Z < 0.1) Then Z = 0.1
            Alpha = Z * Alpha
            intt = 1
            GoTo 30
49:         '  continue
            If (tot < aeps) Then
                GoTo 92
            End If

50:         '   continue
            Alpha = tot

            For i = 1 To n
                W(i) = X(i)
                W(ib + i) = G(i)
            Next i

            link = 2
            If idiff > 1 Then GoTo 110
            GoTo 100
54:         '   continue
            If (ifn >= maxfn) Then GoTo 90
            gys = 0

            For i = 1 To n
                W(i) = W(ib + i)
                gys = gys + G(i) * W(iss + i)
            Next i

            DF = FF - F
            dgs = gys - gs0
            If (dgs <= 0) Then GoTo 20
            link = 1
            If (dgs + Alpha * gs0 > 0.0!) Then GoTo 52

            For i = 1 To n
                W(iu + i) = G(i) - W(i)
            Next i

            Sig = 1 / (Alpha * dgs)
            GoTo 70
52:         '   continue
            zz = Alpha / (dgs - Alpha * gs0)
            Z = dgs * zz - 1

            For i = 1 To n
                W(iu + i) = Z * W(i) + G(i)
            Next i

            Sig = 1 / (zz * dgs * dgs)
            GoTo 70
60:         '    continue
            link = 2

            For i = 1 To n
                W(iu + i) = W(i)
            Next i

            If (dgs + Alpha * gs0 > 0) Then GoTo 62
            Sig = 1 / gs0
            GoTo 70
62:         '    continue
            Sig = -zz
70:         '    continue
            W(iv + 1) = W(iu + 1)

            For i = 2 To n
                ij = i
                i1 = i - 1
                Z = W(iu + i)

                For j = 1 To i1
                    Z = Z - H(ij) * W(iv + j)
                    ij = ij + n - j
                Next j
                W(iv + i) = Z
            Next i

            ij = 1

            For i = 1 To n
                temp = W(iv + i)
                Z = H(ij) + Sig * temp * temp
                If (Z <= 0) Then Z = dmin
                If (Z < dmin) Then dmin = Z
                H(ij) = Z
                W(ib + i) = temp * Sig / Z
                Sig = Sig - Z * W(ib + i) * W(ib + i)
                ij = ij + Np - i
            Next i

            ij = 1

            For i = 1 To N1
                ij = ij + 1
                i1 = i + 1

                For j = i1 To n
                    W(iu + j) = W(iu + j) - H(ij) * W(iv + i)
                    H(ij) = H(ij) + W(ib + i) * W(iu + j)
                    ij = ij + 1
                Next j
            Next i
            If link = 1 Then GoTo 60
            If link = 2 Then GoTo 20
            'go to (60,20),link
90:         '   continue
            iexit = 3
            If PrintOn And MaxRuns = 1 Then
                'If Alpha > 1E-20 Then frmOptF.Res.Print "maximum number of evaluations exceeded " Else frmOptF.Res.Print "can't find improving step"
                If Alpha > 1.0E-20 Then
                    Me.addMessage("maximum number of evaluations exceeded ")
                    ' MsgBox("maximum number of evaluations exceeded ")
                Else
                    Me.addMessage("can't find improving step")
                    ' MsgBox("can't find improving step")
                End If
            End If
            GoTo 94
92:         '    continue
            If (idiff = 2) Then GoTo 94
            idiff = 2
            GoTo pte
94:         '   continue
            If (iexit = 2) Then
                If PrintOn And MaxRuns = 1 Then
                    Me.addMessage("fletch grad transpose times delta x greater than or equal zero --- eps set too small?")
                    '    MsgBox("fletch grad transpose times delta x greater than or equal zero --- eps set too small?")
                End If
                'If PrintOn = True Then frmOptF.Res.Print "fletch  grad transpose times delta x greater than or"
                'If PrintOn = True Then frmOptF.Res.Print "equal zero ---   eps set too small?"
            End If
            If (iprint = 0) Then
                Debug.Assert(False, "Exiting flet().")
                Me.addMessage("Exiting optimization.")
                Return
            End If

            itime = 1
            Xtime = itime / 1000.0!
            'frmOptF.Res.Print "final statistics"
            'frmOptF.vaRes.maxRows = frmOptF.vaRes.maxRows + 1
            'SetCellText frmOptF.vaRes, 2, frmOptF.vaRes.maxRows, "Final statistics"
            '    If MaxRuns > 1 Then frmOptF.vaRes.maxRows = TotalRuns
            '      Call printstats(Xtime, itn, ifn, F, n, X(), G())
            'If (MaxRuns = 1 Or DoWhat = "LastRun") And ifn <= MaxNoOfIterations And frmOptF.chkBatch = False Then
            '    If PrintOn Then MsgBox("Optimization done", vbOKOnly, "EwE: optimum fishing strategy")
            '    DoWhat = ""
            'End If

            printstats(Xtime, itn, ifn, F, n, X, G)

            ' ToDo: globalize this
            Me.addMessage("Optimization done", eMessageType.Any, eMessageImportance.Information)
            ' MsgBox("Optimization done", vbOKOnly, "EwE: optimum fishing strategy")
            GoTo endline

100:        ' continue

            For i = 1 To n
                Z = hh * Xm(i)
                W(i) = W(i) + Z
                f1 = FUNC(W, n) : If SearchFailed = True Then Exit Sub
                G(i) = (f1 - F) / Z
                W(i) = W(i) - Z
            Next i

            ifn = ifn + n
            If link = 1 Then GoTo 18
            If link = 2 Then GoTo 54
            ' go to (18,54),link
110:        '  continue


            For i = 1 To n
                Z = hh * Xm(i)
                W(i) = W(i) + Z
                f1 = FUNC(W, n) : If SearchFailed = True Then Exit Sub
                W(i) = W(i) - Z - Z
                f2 = FUNC(W, n) : If SearchFailed = True Then Exit Sub
                G(i) = (f1 - f2) / (2 * Z)
                W(i) = W(i) + Z
            Next i

            ifn = ifn + n + n
            If link = 1 Then GoTo 18
            If link = 2 Then GoTo 54
            'go to (18,54),link
            'c *** print headings **
7000:       'If PrintOn = True Then frmOptF.Res.Print "initial statistics"
            GoTo 7003
7010:       'If PrintOn = True Then frmOptF.Res.Print "intermediate statistics"
            llog = 0
            GoTo 7003
Ptc:        If PrintOn = True Then
                Me.addMessage("fletch hessian not positive definate")
                'MsgBox("fletch hessian not positive definate")
            End If

endline:    ' '


        End Sub

        Function FUNC(ByVal X() As Double, ByVal n As Integer) As Double
            Dim i As Integer
            'Dim totval As Double, Employ As Double,ecovalue As Double, manvalue As Double,
            Dim LogUtil As Double
            Dim returnvalue As Double

            'then generate your predictions here and calculate the fitting criterion,
            'for example set sumdev=sum over observations of squared deviations between
            'predicted and observed values

            'For i = 1 To n
            '    System.Console.WriteLine(X(i))
            'Next
            'following is -log likelihood for variable linfinity growth fitting from tag data
            ' where x(1)=est linfinity, x(2)=est K

            Try

                m_ecosim.RunModelValue(TotalTime, X, n)

                If Me.m_searchData.FPSUseEconomicPlugin And (Me.m_pluginManager IsNot Nothing) Then
                    Me.m_pluginManager.PostRunSearchResults(Me.m_searchData)
                End If

                VlocalPenalty = 0
                For i = 1 To n
                    VlocalPenalty = VlocalPenalty + 0.001 * X(i) ^ 2
                Next

                If TotValBase <> 0 Then CritValue(eSearchCriteriaResultTypes.TotalValue) = CSng(m_searchData.totval / TotValBase)
                If EmployBase <> 0 Then CritValue(eSearchCriteriaResultTypes.Employment) = CSng(m_searchData.Employ / EmployBase)
                If ManValueBase <> 0 Then CritValue(eSearchCriteriaResultTypes.MandateReb) = CSng(m_searchData.manvalue / ManValueBase)
                If EcoValueBase <> 0 Then CritValue(eSearchCriteriaResultTypes.Ecological) = CSng(m_searchData.ecovalue / EcoValueBase)
                If BioDivBase <> 0 Then CritValue(eSearchCriteriaResultTypes.BioDiversity) = CSng(m_searchData.DiversityIndex / BioDivBase)

                returnvalue = VlocalPenalty - m_searchData.ValWeight(eSearchCriteriaResultTypes.TotalValue) * m_searchData.totval / TotValBase - _
                        m_searchData.ValWeight(eSearchCriteriaResultTypes.Employment) * m_searchData.Employ / EmployBase - _
                        m_searchData.ValWeight(eSearchCriteriaResultTypes.MandateReb) * m_searchData.manvalue / ManValueBase - _
                        m_searchData.ValWeight(eSearchCriteriaResultTypes.Ecological) * m_searchData.ecovalue / EcoValueBase - _
                        m_searchData.ValWeight(eSearchCriteriaResultTypes.BioDiversity) * m_searchData.DiversityIndex / BioDivBase

                If m_searchData.MinimizeEffortChange Then
                    If returnvalue < 0 Then
                        returnvalue = returnvalue * EffortChangePenalty()
                    Else
                        returnvalue = returnvalue * (1 / EffortChangePenalty())
                    End If
                End If

                If m_searchData.LimitFishingMortality Then returnvalue = returnvalue * LimitFPenalty()

                If m_searchData.PortFolio = True Then
                    'calculate general log utility for net economic value
                    'sets to quadratic function with continuous derivative if critvalue is <-.5
                    If CritValue(1) + 1 > 0.5 Then
                        LogUtil = Math.Log(CritValue(1) + 1)
                    Else
                        LogUtil = Math.Log(0.5) + 1 / 0.5 * (CritValue(1) + 1 - 0.5) - 1 / 0.25 * (CritValue(1) + 1 - 0.5) ^ 2
                    End If
                    returnvalue = -m_searchData.ValWeight(eSearchCriteriaResultTypes.TotalValue) * LogUtil + _
                                   m_searchData.ValWeight(eSearchCriteriaResultTypes.Employment) * m_searchData.Ecodistance - _
                                   m_searchData.ValWeight(eSearchCriteriaResultTypes.MandateReb) * ExistValue
                End If

                'Is the objective function value a valid number
                If Double.IsNaN(returnvalue) Or Double.IsInfinity(returnvalue) Then
                    'Nope...
                    'figure out which criteria value is an invalid number
                    'and dump it to the log
                    Dim enumNames As String
                    For icrt As Integer = 0 To CritValue.Length - 1
                        If Double.IsNaN(CritValue(icrt)) Or Double.IsInfinity(CritValue(icrt)) Then
                            Dim enumname As String = [Enum].GetName(GetType(eSearchCriteriaResultTypes), icrt)
                            enumNames += enumname + " "
                            cLog.Write("Fishing Policy Search criteria value " + enumname + " is invalid.")
                        End If
                    Next

                    'If there was an error the returnvalue will not matter
                    'SearchFailed will force the search to stop
                    returnvalue = 1.0E+20
                    SearchFailed = True
                    addMessage("Fishing Policy Search Error: Invalid optimization value for " + enumNames, eMessageType.ErrorEncountered)
                End If

                Return returnvalue

            Catch ex As Exception
                'If there was an error the returnvalue will not matter
                'SearchFailed will force the search to stop
                FUNC = 1.0E+20
                SearchFailed = True

                cLog.Write("Fishing Policy Search Aborted due to Error.")
                cLog.Write(ex)
                addMessage("Fishing Policy Search Error: " & ex.Message, eMessageType.ErrorEncountered)

            End Try

        End Function


        Sub DFPmin(ByVal P() As Double, ByVal n As Integer, ByVal FTOL As Double, ByVal iter As Integer, ByVal ift As Integer, ByVal FRET As Double)
            Dim Itmax As Integer, eps As Double, Fp As Double, i As Integer, j As Integer
            Dim its As Integer, Fac As Double, Fae As Double, Fad As Double, Dum As Double
            SearchFailed = False
            ifn = 0
            Itmax = MaxNoOfIterations '200
            eps = 0.0000000001
            Dim Hessin(,) As Double, xi() As Double, G() As Double, dg() As Double, hdg() As Double
            ReDim Hessin(n, n), xi(n), G(n), dg(n), hdg(n)

            Fp = FUNC2(P, n)
            DFUNC(P, G, n)

            For i = 1 To n
                For j = 1 To n
                    Hessin(i, j) = 0.0!
                Next j
                Hessin(i, i) = 1.0!
                xi(i) = -G(i)
            Next i

            For its = 1 To Itmax
                iter = its
                LINMIN(P, xi, n, FRET)
                If 2.0! * Math.Abs(FRET - Fp) <= FTOL * (Math.Abs(FRET) + Math.Abs(Fp) + eps) Then
                    Erase hdg, dg, G, xi, Hessin
                    ift = ifn
                    Exit Sub
                End If
                Fp = FRET
                For i = 1 To n
                    dg(i) = G(i)
                Next i
                FRET = FUNC2(P, n)
                DFUNC(P, G, n)
                For i = 1 To n
                    dg(i) = G(i) - dg(i)
                Next i
                For i = 1 To n
                    hdg(i) = 0.0!
                    For j = 1 To n
                        hdg(i) = hdg(i) + Hessin(i, j) * dg(j)
                    Next j
                Next i
                Fac = 0.0!
                Fae = 0.0!
                For i = 1 To n
                    Fac = Fac + dg(i) * xi(i)
                    Fae = Fae + dg(i) * hdg(i)
                Next i
                Fac = 1.0! / Fac
                Fad = 1.0! / Fae
                For i = 1 To n
                    dg(i) = Fac * xi(i) - Fad * hdg(i)
                Next i
                For i = 1 To n
                    For j = 1 To n
                        Dum = Fac * xi(i) * xi(j) - Fad * hdg(i) * hdg(j) + Fae * dg(i) * dg(j)
                        Hessin(i, j) = Hessin(i, j) + Dum
                    Next j
                Next i
                For i = 1 To n
                    xi(i) = 0.0!
                    For j = 1 To n
                        xi(i) = xi(i) - Hessin(i, j) * G(j)
                    Next j
                Next i
                printstats(0, its, ifn, FRET, n, P, G)
                ift = ifn
                If SearchFailed = True Then Exit Sub

            Next its
            ift = ifn
            'frmOptF.Res.Print "too many iterations in DFPMIN"
            Me.addMessage("too many iterations in DFPMIN")
            '  MsgBox("too many iterations in DFPMIN")
        End Sub




        Sub DFUNC(ByVal X() As Double, ByRef DF() As Double, ByVal n As Integer)
            Dim Dstep As Double, Fbase As Double, i As Integer
            'DF(1) = 2 * X(1) - 0.9 * X(2)
            'DF(2) = 2 * X(2) + -0.9 * X(1)
            Dstep = 0.000001
            Fbase = FUNC2(X, n)
            For i = 1 To n
                X(i) = X(i) + Dstep
                DF(i) = (FUNC2(X, n) - Fbase) / Dstep
                X(i) = X(i) - Dstep
            Next
        End Sub


        Function FUNC2(ByVal X() As Double, ByVal n As Integer) As Double
            'FUNC2 = X(1) ^ 2 + X(2) ^ 2 - 0.9 * X(1) * X(2)
            FUNC2 = FUNC(X, n)
            ifn = ifn + 1
        End Function


        Sub LINMIN(ByRef P() As Double, ByRef xi() As Double, ByVal n As Integer, ByRef FRET As Double)
            Dim Tol As Double, j As Integer, Ax As Double, XX As Double, Fa As Double
            Dim Fb As Double, Fx As Double, Dum As Double, Bx As Double
            Dim Xmin As Double
            Tol = 0.0001
            ncom = n
            For j = 1 To n
                pcom(j) = P(j)
                xicom(j) = xi(j)
            Next j
            Ax = 0.0!
            XX = 1.0!
            MNBRAK(Ax, XX, Bx, Fa, Fx, Fb, Dum)
            FRET = BRENT(Ax, XX, Bx, Dum, Tol, Xmin)
            For j = 1 To n
                xi(j) = Xmin * xi(j)
                P(j) = P(j) + xi(j)
            Next j
        End Sub


        Sub MNBRAK(ByRef Ax As Double, ByRef Bx As Double, ByRef cx As Double, ByRef Fa As Double, ByRef Fb As Double, ByRef FC As Double, ByRef Dum As Double)
            Dim Q As Double, R As Double, Gold As Double, Glimit As Double, Tiny As Double
            Dim U As Double, Ulim As Double, Fu As Double, done As Boolean
            Gold = 1.618034
            Glimit = 100.0!
            Tiny = 1.0E-20
            Fa = FUNC(Ax)
            Fb = FUNC(Bx)
            If Fb > Fa Then
                Dum = Ax
                Ax = Bx
                Bx = Dum
                Dum = Fb
                Fb = Fa
                Fa = Dum
            End If
            cx = Bx + Gold * (Bx - Ax)
            FC = FUNC(cx)
            Do
                If Fb < FC Then Exit Do
                done = True '-1
                R = (Bx - Ax) * (Fb - FC)
                Q = (Bx - cx) * (Fb - Fa)
                Dum = Q - R
                If Math.Abs(Dum) < Tiny Then Dum = Tiny
                U = Bx - ((Bx - cx) * Q - (Bx - Ax) * R) / (2.0! * Dum)
                Ulim = Bx + Glimit * (cx - Bx)
                If (Bx - U) * (U - cx) > 0.0! Then
                    Fu = FUNC(U)
                    If Fu < FC Then
                        Ax = Bx
                        Fa = Fb
                        Bx = U
                        Fb = Fu
                        Exit Sub
                    ElseIf Fu > Fb Then
                        cx = U
                        FC = Fu
                        Exit Sub
                    End If
                    U = cx + Gold * (cx - Bx)
                    Fu = FUNC(U)
                ElseIf (cx - U) * (U - Ulim) > 0.0! Then
                    Fu = FUNC(U)
                    If Fu < FC Then
                        Bx = cx
                        cx = U
                        U = cx + Gold * (cx - Bx)
                        Fb = FC
                        FC = Fu
                        Fu = FUNC(U)
                    End If
                ElseIf (U - Ulim) * (Ulim - cx) >= 0.0! Then
                    U = Ulim
                    Fu = FUNC(U)
                Else
                    U = cx + Gold * (cx - Bx)
                    Fu = FUNC(U)
                End If
                If done Then
                    Ax = Bx
                    Bx = cx
                    cx = U
                    Fa = Fb
                    Fb = FC
                    FC = Fu
                Else
                    done = False '0
                End If
            Loop While Not done
        End Sub

        Function BRENT(ByRef Ax As Double, ByRef Bx As Double, ByRef cx As Double, ByRef Dum As Double, ByRef Tol As Double, ByRef Xmin As Double) As Double
            Dim Itmax As Integer, Cgold As Double, Zeps As Double, A As Double, B As Double
            Dim v As Double, W As Double, X As Double, E As Double, Fx As Double
            Dim Fval As Double, Fw As Double, iter As Integer, done As Boolean
            Dim Xm As Double, Tol1 As Double, Tol2 As Double, R As Double, P As Double, Q As Double
            Dim d As Double, Etemp As Double, U As Double, Fu As Double
            Itmax = 100
            Cgold = 0.381966
            Zeps = 0.0000000001
            A = Ax
            If cx < Ax Then A = cx
            B = Ax
            If cx > Ax Then B = cx
            v = Bx
            W = v
            X = v
            E = 0.0!
            Fx = FUNC(X)
            Fval = Fx
            Fw = Fx
            For iter = 1 To Itmax
                Xm = 0.5 * (A + B)
                Tol1 = Tol * Math.Abs(X) + Zeps
                Tol2 = 2.0! * Tol1
                If Math.Abs(X - Xm) <= Tol2 - 0.5 * (B - A) Then Exit For
                done = True '-1
                If Math.Abs(E) > Tol1 Then
                    R = (X - W) * (Fx - Fval)
                    Q = (X - v) * (Fx - Fw)
                    P = (X - v) * Q - (X - W) * R
                    Q = 2.0! * (Q - R)
                    If Q > 0.0! Then P = -P
                    Q = Math.Abs(Q)
                    Etemp = E
                    E = d
                    Dum = Math.Abs(0.5 * Q * Etemp)
                    If Math.Abs(P) < Dum And P > Q * (A - X) And P < Q * (B - X) Then
                        d = P / Q
                        U = X + d
                        If U - A < Tol2 Or B - U < Tol2 Then d = Math.Abs(Tol1) * Math.Sign(Xm - X)
                        done = False '0
                    End If
                End If
                If done Then
                    If X >= Xm Then
                        E = A - X
                    Else
                        E = B - X
                    End If
                    d = Cgold * E
                End If
                If Math.Abs(d) >= Tol1 Then
                    U = X + d
                Else
                    U = X + Math.Abs(Tol1) * Math.Sign(d)
                End If
                Fu = FUNC(U)
                If Fu <= Fx Then
                    If U >= X Then
                        A = X
                    Else
                        B = X
                    End If
                    v = W
                    Fval = Fw
                    W = X
                    Fw = Fx
                    X = U
                    Fx = Fu
                Else
                    If U < X Then
                        A = U
                    Else
                        B = U
                    End If
                    If Fu <= Fw Or W = X Then
                        v = W
                        Fval = Fw
                        W = U
                        Fw = Fu
                    ElseIf Fu <= Fval Or v = X Or v = W Then
                        v = U
                        Fval = Fu
                    End If
                End If
            Next iter
            'If iter > Itmax Then frmOptF.Res.Print "Brent exceed maximum iterations.": End
            If iter > Itmax Then Me.addMessage("Brent exceed maximum iterations.") : Exit Function 'End
            Xmin = X
            BRENT = Fx
        End Function


        Private Function FUNC(ByVal X As Double) As Double
            FUNC = F1DIM(X)
        End Function

        Function F1DIM(ByVal X As Double) As Double
            Dim XT(50) As Double, j As Integer
            For j = 1 To ncom
                XT(j) = pcom(j) + X * xicom(j)
            Next j
            F1DIM = FUNC2(XT, ncom)
            Erase XT
        End Function


        Sub SearchForBaseProfitability(ByVal X() As Double, ByVal n As Integer)
            ' Dim totval As Double, Employ As Double, manvalue As Double, ecovalue As Double
            Dim BaseIncome() As Single, Temp As Double
            Dim CostToI() As Single, GainToJ(,) As Single, iter As Integer
            Dim PaidToJ() As Single
            Dim tcost As Single
            Dim Xtime As Double
            Dim RelaxWt As Single
            Dim GroMax As Single
            Dim Delp() As Single, LastX As Double, DelX() As Double, LastP() As Single, DpDx() As Double
            Dim i As Integer, j As Integer, K As Integer, SpGaintoJ As Single
            Dim gro As Double, SumGro As Double

            Dim epdata As cEcopathDataStructures = m_core.m_EcoPathData

            'exit if search is not over gear types
            If n <> m_searchData.NumFleets Then
                Me.m_core.Messages.SendMessage(New cMessage("This search method only allows you to search over all fleets. You must set the search blocks to one block per fleet.", _
                                                    eMessageType.ErrorEncountered, eCoreComponentType.FishingPolicySearch, eMessageImportance.Warning))
                Exit Sub
            End If

            RelaxWt = 0.5
            GroMax = 0.3
            PropToPlaintiff = 0.0

            Dim BaseIncomeSpecies(,) As Single
            ReDim BaseIncome(m_searchData.NumFleets), BaseIncomeSpecies(m_searchData.NumFleets, m_searchData.NumGroups) ', BaseEffort(m_searchData.NumFleets)
            ReDim CostToI(m_searchData.NumFleets), GainToJ(m_searchData.NumFleets, m_searchData.NumFleets), PaidToJ(m_searchData.NumFleets), PaidToJbyI(m_searchData.NumFleets, m_searchData.NumFleets)
            ReDim Profitability(m_searchData.NumFleets), G(m_searchData.NumFleets)
            Dim tincome As Single, Dummy As Double, nch As Integer
            ReDim Delp(m_searchData.NumFleets), DelX(m_searchData.NumFleets), LastP(m_searchData.NumFleets), DpDx(m_searchData.NumFleets)

            'varies fishing efforts so as to try and achieve baseprofitability for each fleet, while accounting
            'for transfer costs from fleets that cause reduced income to the fleets impacted by such reductions

            Do
                iter = iter + 1
                'get base incomes and costs for this iteration
                Dummy = FUNC(X, n)

                'LastYearIncome() and LastYearIncomeSpecies() set by Ecosim.RunModelValue called by FUNC()
                For i = 1 To m_searchData.NumFleets
                    BaseIncome(i) = m_searchData.LastYearIncome(i)
                    For K = 1 To m_searchData.NumGroups
                        BaseIncomeSpecies(i, K) = m_searchData.LastYearIncomeSpecies(i, K)
                    Next
                Next

                'then get gains to each gear j of eliminating gear i, while accumulating negative gains
                'as costs to gear i
                ReDim PaidToJ(m_searchData.NumFleets), PaidToJbyI(m_searchData.NumFleets, m_searchData.NumFleets)
                For i = 1 To m_searchData.NumFleets
                    Temp = X(i)
                    'turn off gear i temporarily and make a run
                    X(i) = -5
                    m_ecosim.RunModelValue(TotalTime, X, n)
                    CostToI(i) = 0
                    For j = 1 To m_searchData.NumFleets
                        GainToJ(i, j) = m_searchData.LastYearIncome(j) - BaseIncome(j)

                        If m_searchData.IncludeCompetitiveImpact Then

                            If GainToJ(i, j) > 0 Then
                                CostToI(i) = CostToI(i) + GainToJ(i, j)
                                PaidToJ(j) = PaidToJ(j) + GainToJ(i, j)
                                PaidToJbyI(i, j) = GainToJ(i, j)
                            End If

                        Else
                            For K = 1 To m_searchData.NumGroups
                                If BaseIncomeSpecies(i, K) = 0 Then
                                    SpGaintoJ = m_searchData.LastYearIncomeSpecies(j, K) - BaseIncomeSpecies(j, K)
                                    If SpGaintoJ > 0 Then
                                        CostToI(i) = CostToI(i) + SpGaintoJ
                                        PaidToJ(j) = PaidToJ(j) + SpGaintoJ
                                        PaidToJbyI(i, j) = PaidToJbyI(i, j) + SpGaintoJ
                                    End If
                                End If
                            Next
                        End If
                    Next
                    'restore log effort for gear i
                    X(i) = Temp
                Next
                For i = 1 To m_searchData.NumFleets
                    m_searchData.LastYearIncome(i) = BaseIncome(i)
                Next

                updateBaseProfitabilityResults()
                'If StopEstimation = False Then ShowFleetCosts()

                'now calculate profitabilities if all gears were charged for their costs to other gears
                'and increment/decrement log effort in proportion to excess of profitability over target
                SumGro = 0
                nch = 0
                For i = 1 To m_searchData.NumFleets
                    tcost = CSng((epdata.cost(i, eCostIndex.CUPE) + epdata.cost(i, eCostIndex.Sail)) * Math.Exp(X(i)) + CostToI(i) + 0.0000000001)
                    tincome = CSng(BaseIncome(i) + PropToPlaintiff * PaidToJ(i) + 0.0000000001)

                    Profitability(i) = (tincome - tcost) / tincome - m_searchData.TargetProfitability(i)
                    LastX = X(i)
                    If Math.Abs(DelX(i)) < 0.1 Then
                        'use simple step based on profitability
                        gro = Profitability(i)
                        If gro > GroMax Then gro = GroMax
                        If gro < -GroMax Then gro = -GroMax

                        X(i) = X(i) + RelaxWt * gro
                    Else
                        'use linear projection step based on dprofitability/dX
                        Delp(i) = Profitability(i) - LastP(i)
                        DpDx(i) = Delp(i) / DelX(i)
                        If Math.Abs(DpDx(i)) > 0.01 Then
                            gro = -LastP(i) / (DpDx(i))
                            If gro > GroMax Then gro = GroMax
                            If gro < -GroMax Then gro = -GroMax
                            X(i) = X(i) + RelaxWt * gro
                        Else
                            gro = 0.000001
                            X(i) = X(i) + gro
                        End If
                    End If
                    If X(i) < -5 Then X(i) = -5
                    DelX(i) = X(i) - LastX
                    SumGro = SumGro + Math.Abs(DelX(i))
                    If Math.Abs(gro) > 0.01 Then nch = nch + 1
                    LastP(i) = Profitability(i)
                Next

                printstats(Xtime, iter, m_searchData.NumFleets * iter, Dummy, n, X, G)

                If SumGro < 0.01 Or nch = 0 Or iter > 100 Or StopEstimation Then Exit Do
            Loop

            'do one last model run to set fishing rates to final optimum found
            Dummy = FUNC(X, n)

        End Sub

        Private Sub updateBaseProfitabilityResults()

            Try

                For iflt As Integer = 1 To Results.nFleets

                    Results.Income(iflt) = m_searchData.LastYearIncome(iflt)
                    Results.Profitability(iflt) = Profitability(iflt)

                    For iflt2 As Integer = 1 To Results.nFleets
                        Results.CompensationMatrix(iflt, iflt2) = CSng(PaidToJbyI(iflt, iflt2) / (m_searchData.LastYearIncome(iflt) + 1.0E-20))
                    Next iflt2

                Next iflt

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False)
            End Try

        End Sub

        ''' <summary>
        ''' Compute a penalty multiplier if change in effort from the last year to this year is greater than MaxEffortChange
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>The penalty is the sum of change ratio from year to year  penalty = penalty * (CurEffort(i) / LastEffort(i))</remarks>
        Private Function EffortChangePenalty() As Single
            Dim i As Integer
            Dim iyr As Integer
            Dim CurEffort() As Single
            Dim LastEffort() As Single
            Dim penalty As Single
            ReDim CurEffort(m_searchData.NumFleets)
            ReDim LastEffort(m_searchData.NumFleets)

            penalty = 1 'default return value 

            For i = 1 To m_searchData.NumFleets
                For iyr = 2 To TotalTime
                    If m_searchData.FblockCode(i, iyr) > 0 Then
                        LastEffort(i) = m_core.m_EcoSimData.FishRateGear(i, 12 * iyr - 23)
                        CurEffort(i) = m_core.m_EcoSimData.FishRateGear(i, 12 * iyr - 11)

                        If CurEffort(i) > 0 And LastEffort(i) > 0 Then

                            If CurEffort(i) > LastEffort(i) * m_searchData.MaxEffortChange Then
                                penalty = penalty * (LastEffort(i) / CurEffort(i))
                            ElseIf LastEffort(i) > CurEffort(i) * m_searchData.MaxEffortChange Then
                                penalty = penalty * (CurEffort(i) / LastEffort(i))
                            End If

                        End If

                    End If
                Next
            Next

            Return penalty

#If EWE5_CODE Then
'EwE5 code could return zero in some cases
            EffortChangePenalty = 1
'Exit Function
For i = 1 To NumGear
    LastEffort(i) = FishRateGear(i, 12 * 1 - 11)
Next
For i = 1 To NumGear
    For iyr = 2 To TotalTime
        If FblockCode(i, iyr) > 0 Then
            LastEffort(i) = FishRateGear(i, 12 * iyr - 23)
            CurEffort(i) = FishRateGear(i, 12 * iyr - 11)
            If CurEffort(i) > LastEffort(i) * MaxEffortChange And CurEffort(i) > 0 Then
                EffortChangePenalty = EffortChangePenalty * (LastEffort(i) / CurEffort(i)) '^ (1 / 2)
                'FishRateGear(i, 12 * iyr - 11) = LastEffort(i) / MaxEffortChange
            ElseIf LastEffort(i) > CurEffort(i) * MaxEffortChange And LastEffort(i) > 0 Then
                EffortChangePenalty = EffortChangePenalty * (CurEffort(i) / LastEffort(i)) '^ (1 / 2)
                'FishRateGear(i, 12 * iyr - 11) = LastEffort(i) * MaxEffortChange
            End If
            LastEffort(i) = CurEffort(i)
        End If
    Next
Next

#End If

        End Function

        Private Function LimitFPenalty() As Single
            Dim i As Integer
            Dim maxF As Single
            Dim Grp As Integer
            LimitFPenalty = 1
            Dim tSteps As Integer = m_core.nEcosimTimeSteps
            For Grp = 1 To m_core.nLivingGroups
                If m_searchData.FLimit(Grp) < 1000 And m_searchData.FLimit(Grp) > 0 Then
                    maxF = 0
                    '080610VC: changed the time loop below to start at baseyear+1, 
                    'we're not interested in what happened earlier when doing a search
                    'Also, changed it to annual steps, since effort is annual; faster!
                    For i = 12 * (m_searchData.BaseYear + 1) To tSteps Step 12
                        If m_core.m_EcoSimData.FishRateNo(Grp, i) > m_searchData.FLimit(Grp) Then
                            If m_core.m_EcoSimData.FishRateNo(Grp, i) > maxF Then
                                maxF = m_core.m_EcoSimData.FishRateNo(Grp, i)
                            End If
                        End If
                    Next
                    If maxF > 0 Then LimitFPenalty = CSng(LimitFPenalty * (m_searchData.FLimit(Grp) / maxF) ^ 2) ': Stop
                End If
            Next

        End Function


#End Region

    End Class

#End Region

#Region "Results Object"

    ''' <summary>
    ''' This is a wrapper for the Fishing Policy Search time step results
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cFPSSearchResults

        Public BlockResults() As Single
        Public BlockNumber() As Integer

        Public CriteriaValues(cSearchDatastructures.N_CRIT_RESULTS) As Single
        Public Totals As Single
        Public nCalls As Integer

        'output variables for base profitability
        Public Income() As Single
        Public Profitability() As Single
        Public CompensationMatrix(,) As Single

        Private m_nblocks As Integer
        Private m_nFleets As Integer

        Friend Sub New(ByVal NumberOfBlocks As Integer, ByVal NumberOfFleets As Integer)

            m_nblocks = NumberOfBlocks
            m_nFleets = NumberOfFleets

            RedimBlocks()
            RedimBaseProfitability()

        End Sub

        Private Sub RedimBlocks()
            ReDim BlockResults(m_nblocks)
            ReDim BlockNumber(m_nblocks)
        End Sub

        Private Sub RedimBaseProfitability()

            ReDim Income(m_nFleets)
            ReDim Profitability(m_nFleets)
            ReDim CompensationMatrix(m_nFleets, m_nFleets)

        End Sub


        Public Property nBlocks() As Integer
            Get
                Return m_nblocks
            End Get
            Friend Set(ByVal value As Integer)
                m_nblocks = value
                RedimBlocks()
            End Set
        End Property

        Public ReadOnly Property nFleets() As Integer
            Get
                Return m_nFleets
            End Get
        End Property


        Friend Sub Clear()
            Totals = 0
            nCalls = 0

            Array.Clear(Income, 0, Income.Length)
            Array.Clear(Profitability, 0, Profitability.Length)
            Array.Clear(BlockResults, 0, BlockResults.Length)

            Array.Clear(CompensationMatrix, 0, CompensationMatrix.Length)

        End Sub



    End Class

#End Region

End Namespace

