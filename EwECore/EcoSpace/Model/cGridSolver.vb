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
Imports System.Threading
Imports EwEUtils.Core

Public Class cGridSolver

#Region "Public data"

    ''' <summary>
    ''' Signal mechanism used by the calling thread for thread Synchronization
    ''' </summary>
    ''' <remarks>
    ''' When the Solve() thread is running (SignalState in a non-signaled state WaitHandle.Reset()) 
    ''' calls to WaitHandle.WaitOne() will block until Solve() has completed (SignalState in a signaled state WaitHandle.Set())
    ''' </remarks>
    Public WaitHandle As New ManualResetEvent(True)

    Public Shared ThreadIncrementer As Integer

    Public iterThread As Integer 'total iterations 

    Public CPUTime As Single


#End Region

#Region "Private data"

    Public ThreadID As Integer

    'arguments from original code
    Private X(,,) As Single
    Private Aloc(,,) As Single
    Private Floc(,,) As Single
    Private jord() As Integer
    Private W As Single
    Private Bcw(,,) As Single
    Private C(,,) As Single
    Private d(,,) As Single
    Private e(,,) As Single
    Private M As Integer
    Private NomCols As Integer
    Private Tol As Single
    Private Depth(,) As Single

    Private iFrstGrp As Integer
    Private iLastGrp As Integer

    Private ByPassIntegrate() As Boolean

    Private iStartRow() As Integer
    Private iEndRow() As Integer
    Private jStartCol() As Integer
    Private jEndCol() As Integer

    Private timeStep As Single

    Private maxIter As Integer

    Private alternateRowCol As Boolean = False

    Private isMigratory() As Boolean

    Public threadTime As Double

    Private threadGroups(,) As Integer

    Private useExact As Boolean

    Private m_rand As Random

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Local copies of the core arrays used by SolveGridLocalMemoryOnly()
    'this is for Hungabee and are only used if bUseLocalMemory = True
    Public bUseLocalMemory As Boolean

    Private Xloc(,) As Single
    Private AlocLoc(,) As Single
    Private FlocLoc(,) As Single
    Private bloc(,) As Single
    Private cloc(,) As Single
    Private dloc(,) As Single
    Private eloc(,) As Single

    Private DepthLoc(,) As Single

    Private iStartRowLoc() As Integer
    Private iEndRowLoc() As Integer
    Private jStartColLoc() As Integer
    Private jEndColLoc() As Integer
    Private jordLoc() As Integer
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    Private m_stpwCopy As Stopwatch


#End Region

#Region "Constructor and Initialization"

    Public Sub New(ByVal ThreadNumber As Integer)
        'isOkToRunning = True
        ThreadID = ThreadNumber
        Me.m_rand = New Random
    End Sub

    ''' <summary>
    ''' Set all references to data used for calculation
    ''' </summary>
    ''' <remarks>This needs a bunch more data</remarks>
    ''' SolveGrid(ip, AMm, F, m_Data.Bcell, m_Data.Inrow, m_Data.InCol, Tol, jord, m_Data.W)
    ''' SolveGridRow(ip, AMm, F, m_Data.Bcell, m_Data.Inrow, m_Data.InCol, Tol, jord, m_Data.W)
    Public Function Init(ByRef AMm(,,) As Single, ByRef F(,,) As Single, ByRef BCell(,,) As Single, ByRef Inrow As Integer, ByRef InCol As Integer, ByRef Tol1 As Single, ByRef jord1() As Integer, ByRef W1 As Single, ByRef Bcw1(,,) As Single, ByRef C1(,,) As Single, ByRef d1(,,) As Single, ByRef e1(,,) As Single, ByRef Depth1(,) As Single, ByVal BPIntegrate() As Boolean, ByRef iStartRow1() As Integer, ByRef iEndRow1() As Integer, ByVal timeStep1 As Single, ByVal maxIter1 As Integer, ByRef jStartCol1() As Integer, ByRef jEndCol1() As Integer, ByRef isMigratory1() As Boolean, ByRef threadGroups1(,) As Integer, ByVal bUseExact As Boolean) As Boolean
        Me.Aloc = AMm
        Me.Floc = F
        Me.X = BCell
        Me.jord = jord1
        Me.W = W1
        Me.Bcw = Bcw1
        Me.C = C1
        Me.d = d1
        Me.e = e1
        Me.M = Inrow
        Me.NomCols = InCol
        Me.Tol = Tol1
        Me.Depth = Depth1
        Me.ByPassIntegrate = BPIntegrate
        Me.iStartRow = iStartRow1
        Me.iEndRow = iEndRow1
        Me.timeStep = timeStep1
        Me.maxIter = maxIter1
        Me.jStartCol = jStartCol1
        Me.jEndCol = jEndCol1
        Me.isMigratory = isMigratory1
        Me.threadGroups = threadGroups1
        Me.useExact = bUseExact
    End Function

#End Region

#Region "Group Counters"

    Public ReadOnly Property iFirstIndex() As Integer
        Get
            Return iFrstGrp
        End Get
    End Property

    Public ReadOnly Property nGroupsComputed As Integer
        Get
            Return iLastGrp - iFirstIndex + 1
        End Get
    End Property


    ''' <summary>
    ''' Set the groups to iterate over.
    ''' </summary>
    ''' <param name="iFirstGroup"></param>
    ''' <param name="iLastGroup"></param>
    ''' <remarks>Call for each thread, before the thread is started, to set the groups to solve.</remarks>
    Public Sub FirstLastGroups(ByVal iFirstGroup As Integer, ByVal iLastGroup As Integer)
        iFrstGrp = iFirstGroup
        iLastGrp = iLastGroup
    End Sub

#End Region

#Region "Public 'Solve'"

    ''' <summary>
    ''' This is the method that the ThreadPool calls. 
    ''' It must have the object argument to match the Delegate signature required by ThreadPool.QueueUserWorkItem()
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Solve(ByVal obParam As Object)
        'For our purposes here we are ignoring the obParam argument 
        'this sub signature is required by the ThreadPool.QueueUserWorkItem(...)
        ' Dim timeTemp As Double = Timer
        'if this is running on a thread this may not work
        'all flags need to be set outside the thread
        'isOkToRunning = False
        iterThread = 0
        Dim iGrp As Integer
        Dim i As Integer
        Me.CPUTime = 0
        Dim stpw As Stopwatch = Stopwatch.StartNew
        m_stpwCopy = New Stopwatch
        Try

            'set signal state to 'non-signaled' SignalState.WaitOne() will block
            WaitHandle.Reset()
            alternateRowCol = True

            Me.CopyStartEndRowCol()

            'do the processing here
            'System.Console.WriteLine("Solve() " & iFrstGrp.ToString & "," & iLastGrp.ToString)
            For i = iFrstGrp To iLastGrp
                iGrp = threadGroups(ThreadID, i)

                If ByPassIntegrate(iGrp) = False Then
                    If useExact And isMigratory(iGrp) Then
                        solveExact(iGrp)
                    Else
                        If bUseLocalMemory Then
                            'Copies core memory to local arrays for running on Hungabee
                            'otherwise the same as SolveGrid_SharedMemory
                            SolveGrid_LocalMemory(iGrp)
                        Else
                            'Uses core map arrays directly
                            SolveGrid_SharedMemory(iGrp)
                        End If

                        If Not alternateRowCol Then
                            SolveGridRow(iGrp)
                        End If
                    End If
                End If

            Next i

        Catch ex As Exception
            cLog.Write(ex) 'this is dangerous clog.Write is not thread safe
            Debug.Assert(False, ex.Message)
        End Try

        'set signal state to 'signaled' 
        'the processing has finished SignalState.WaitOne() will return immediately
        If Interlocked.Decrement(cGridSolver.ThreadIncrementer) = 0 Then
            WaitHandle.Set()
        End If

        If Me.bUseLocalMemory Then
            '  System.Console.WriteLine("Grid copy CPU time (sec), " + Me.m_stpwCopy.Elapsed.TotalSeconds.ToString)
        End If

        Me.CPUTime = CSng(stpw.Elapsed.TotalSeconds)

    End Sub

    Private Sub solveExact(ByVal ip As Integer)
        'this solves the linearized implicit biomass flux equations directly rather that iteratively
        'this is takes longer than a few iterations in solvegrid, but shorter than a lot
        'if the species requires a lot of iterations (e.g. migratory), this should be better than s.g.
        'bandec and bandks are taken from numerical recipes (press et al.), translated to vb
        Dim a(,) As Double 'reduced band form of Bcell
        Dim al(,) As Double 'reduced band form of L (from LU decomp)
        Dim de As Single
        Dim b() As Single 'input vector b (=-F)
        Dim indx() As Integer 'index table for row subsitutions

        ReDim al(NomCols * M + 1, 2 * NomCols + 1)
        ReDim b(M * NomCols)
        ReDim indx(M * NomCols)

        'get b vector from F
        Dim k As Integer = 0
        For i As Integer = 1 To M
            For j As Integer = 1 To NomCols
                k += 1
                b(k) = -Floc(i, j, ip)
            Next
        Next

        Try
            'for the eq. Ax=b
            'get the reduced band form of a from the fluxes (see press et al. 2-4)
            a = arrangeMatrix(Aloc, Bcw, C, d, e, ip, M, NomCols)

            'get the LU decomposition of a (now a=L and al=L)
            bandec(a, M * NomCols, NomCols, al, indx, de)

            'back substitution to get x (now stored in b)
            bandks(a, M * NomCols, NomCols, al, indx, b)

            'refill the Bcell array with the values in the vector x
            refillX(X, b, ip, M, NomCols)

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub


#End Region

#Region "Grid solving computational code"

    ''' <summary>
    ''' Solve the grid diffusion matrix on directly on core memory.
    ''' </summary>
    ''' <param name="ip">Index of group</param>
    ''' <remarks>
    ''' Run if <see cref="bUseLocalMemory"></see> = False. 
    ''' This will run faster on small to medium sized models.
    ''' </remarks>
    Private Sub SolveGrid_SharedMemory(ByVal ip As Integer)
        'this routine solves for equilibrium field of concentrations x over a grid
        ' x(i,j) is equilibrium concentration of x in grid cell i,j
        'am(i,j) is total loss rate of x from cell i,j...NB:am(i,j)<0 !!!!!!
        'b(i,j) is loss rate from element i-1 to i in column j of grid
        'c(i,j) is loss rate from element i+1 to i in column j of grid
        'd(i,j) is loss rate from element j to element j+1 in row i of grid
        'e(i,j) is loss rate from element j to element j-1 in row i of grid
        'f(i,j) is forcing input to element i,j from sources outside the grid
        'm is number of rows (i) in grid
        'NomCols is number of columns (j) in grid
        'tol is tolerance limit for change in iterative solution
        'jord(k) is which column j to do as k=1, k=2,...,k=n (iteration order)
        'w is SOR overrelaxation parameter-found 1.25 to be good for typical problems
        Dim iter As Integer, j As Integer, i As Integer, jj As Integer, ic As Integer

        Dim alfa(,) As Single
        Dim gam(,) As Single
        Dim rhs(,) As Single
        Dim G() As Single
        Dim Xold(,) As Single
        ReDim alfa(M + 1, NomCols + 1)
        ReDim gam(M + 1, NomCols + 1)
        ReDim rhs(M + 1, NomCols + 1)
        ReDim G(M + 1)
        ReDim Xold(M + 1, NomCols + 1)
        Dim alfa2(,) As Single
        Dim gam2(,) As Single
        Dim G2() As Single
        ReDim G2(NomCols + 1)
        ReDim alfa2(M + 1, NomCols + 1)
        ReDim gam2(M + 1, NomCols + 1)

        Dim totDiff As Single
        Dim totOld As Single
        Dim totdiff2 As Single

        Dim Wold As Single = W

        'System.Console.WriteLine("SolveGrid() " & ip.ToString)
        Try
            'first compute LU decomposition elements for each column j
            'If StopRun = 1 Then Exit Sub
            For i = 0 To M + 1
                For j = 0 To NomCols + 1
                    Xold(i, j) = X(i, j, ip)
                Next
            Next
            For j = 1 To NomCols
                'Xold(1, j) = X(1, j, ip)
                If Aloc(iStartRow(j), j, ip) = 0 Then Aloc(iStartRow(j), j, ip) = -1.0 'E+30
                alfa(iStartRow(j), j) = Aloc(iStartRow(j), j, ip)
                gam(iStartRow(j), j) = C(iStartRow(j), j, ip) / alfa(iStartRow(j), j)
                'For i = 1 To M
                'Xold(i, j) = X(i, j, ip)
                'Next
                For i = iStartRow(j) + 1 To iEndRow(j)
                    If Aloc(i, j, ip) = 0 Then Aloc(i, j, ip) = -1.0 'E+30
                    alfa(i, j) = Aloc(i, j, ip) - Bcw(i, j, ip) * gam(i - 1, j)
                    gam(i, j) = C(i, j, ip) / alfa(i, j)
                Next
            Next
            If alternateRowCol Then
                For i = 1 To M
                    'Xold(i, 1) = X(i, 1, ip)
                    If jStartCol(i) <= jEndCol(i) Then 'if the row is not all land
                        If Aloc(i, jStartCol(i), ip) = 0 Then Aloc(i, jStartCol(i), ip) = -1.0 'E+30
                        alfa2(i, jStartCol(i)) = Aloc(i, jStartCol(i), ip)
                        gam2(i, jStartCol(i)) = e(i, jStartCol(i) + 1, ip) / alfa2(i, jStartCol(i))
                    End If
                    For j = jStartCol(i) + 1 To jEndCol(i)
                        'Xold(i, j) = X(i, j, ip)
                        If Aloc(i, j, ip) = 0 Then Aloc(i, j, ip) = -1.0 'E+30
                        alfa2(i, j) = Aloc(i, j, ip) - d(i, j - 1, ip) * gam2(i, j - 1)
                        gam2(i, j) = e(i, j + 1, ip) / alfa2(i, j)
                    Next
                Next
            End If
            'now begin block Gauss-Seidel/SOR iteration over columns of grid
            'at each iteration, solve explicitly for values in each column given
            'current estimates of "forcing" input from other columns based on their
            'current estimates
            iter = 0
iterate:
            For jj = 1 To NomCols

                j = jord(jj)
                For i = iStartRow(j) To iEndRow(j)
                    rhs(i, j) = -Floc(i, j, ip) - d(i, j - 1, ip) * X(i, j - 1, ip) - e(i, j + 1, ip) * X(i, j + 1, ip)
                Next
                rhs(iStartRow(j), j) = rhs(iStartRow(j), j) - Bcw(iStartRow(j), j, ip) * X(iStartRow(j) - 1, j, ip)
                rhs(iEndRow(j), j) = rhs(iEndRow(j), j) - C(iEndRow(j), j, ip) * X(iEndRow(j) + 1, j, ip)
                'now solve for x(i,j) over i using these forcing inputs to one dimensional
                'tridiagonal solver
                G(iStartRow(j)) = rhs(iStartRow(j), j) / alfa(iStartRow(j), j)
                'IF iflag > 0 THEN FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT: STOP
                For i = iStartRow(j) + 1 To iEndRow(j)
                    G(i) = (rhs(i, j) - Bcw(i, j, ip) * G(i - 1)) / alfa(i, j)
                Next

                X(iEndRow(j), j, ip) = G(iEndRow(j))
                For i = iEndRow(j) - 1 To iStartRow(j) Step -1
                    X(i, j, ip) = G(i) - gam(i, j) * X(i + 1, j, ip)
                Next

                For i = iStartRow(j) To iEndRow(j)
                    X(i, j, ip) = (1 - W) * Xold(i, j) + W * X(i, j, ip)
                Next
            Next

            ic = 0
            totDiff = 0
            totdiff2 = 0
            totOld = 0
            For j = 1 To NomCols
                For i = iStartRow(j) To iEndRow(j)
                    If Depth(i, j) > 0 Then

                        If X(i, j, ip) > 0.0000000001 And Math.Abs((X(i, j, ip) - Xold(i, j)) / (Xold(i, j) + 1.0E-20)) > Tol * timeStep Then
                            ic = ic + 1
                        End If
                        totDiff = totDiff + Math.Abs(X(i, j, ip) - Xold(i, j))
                        totdiff2 = totdiff2 + X(i, j, ip) - Xold(i, j)

                        Xold(i, j) = X(i, j, ip)
                        If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                            Xold(i, j) = 0
                        End If
                        totOld = totOld + Xold(i, j)
                    End If
                Next
            Next

            If alternateRowCol Then
                For i = 1 To M
                    ' If StopRun = 1 Then Exit Sub
                    'j = jord(jj)
                    For j = jStartCol(i) To jEndCol(i)
                        rhs(i, j) = -Floc(i, j, ip) - Bcw(i, j, ip) * X(i - 1, j, ip) - C(i, j, ip) * X(i + 1, j, ip)
                    Next
                    rhs(i, jStartCol(i)) = rhs(i, jStartCol(i)) - d(i, jStartCol(i) - 1, ip) * X(i, jStartCol(i) - 1, ip)
                    rhs(i, jEndCol(i)) = rhs(i, jEndCol(i)) - e(i, jEndCol(i) + 1, ip) * X(i, jEndCol(i) + 1, ip)
                    'now solve for x(i,j) over i using these forcing inputs to one dimensional
                    'tridiagonal solver
                    G2(jStartCol(i)) = rhs(i, jStartCol(i)) / alfa2(i, jStartCol(i))
                    For j = jStartCol(i) To jEndCol(i)
                        G2(j) = (rhs(i, j) - d(i, j - 1, ip) * G2(j - 1)) / alfa2(i, j)
                    Next

                    X(i, jEndCol(i), ip) = G2(jEndCol(i))
                    For j = jEndCol(i) - 1 To jStartCol(i) Step -1
                        X(i, j, ip) = G2(j) - gam2(i, j) * X(i, j + 1, ip)
                    Next

                    For j = jStartCol(i) To jEndCol(i)
                        X(i, j, ip) = (1 - W) * Xold(i, j) + W * X(i, j, ip)
                    Next
                Next

                totDiff = 0
                totdiff2 = 0
                totOld = 0
                ic = 0
                For i = 1 To M
                    For j = jStartCol(i) To jEndCol(i)
                        If Depth(i, j) > 0 Then

                            If X(i, j, ip) > 0.0000000001 And Math.Abs((X(i, j, ip) - Xold(i, j)) / (Xold(i, j) + 1.0E-20)) > Tol * timeStep Then
                                ic = ic + 1
                            End If

                            totDiff = totDiff + Math.Abs(X(i, j, ip) - Xold(i, j))
                            totdiff2 = totdiff2 + X(i, j, ip) - Xold(i, j)

                            Xold(i, j) = X(i, j, ip)
                            If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                                Xold(i, j) = 0 ': Stop
                            End If
                            totOld = totOld + Xold(i, j)
                        End If
                    Next j
                Next i

                'If Math.Abs(totdiff2 / totDiff) > 0.95 Then
                '    W = 1.9
                'Else
                '    W = Wold
                'End If

            End If

            iter = iter + 1
            If ic > 0 And iter < maxIter Then GoTo iterate
exitline:

            Erase alfa, gam, rhs, G, Xold
            iterThread = iterThread + iter
            If alternateRowCol Then
                iter = iter * 2
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub




    Private Sub CopyToLocal(ByVal ip As Integer)

        Debug.Assert(Me.bUseLocalMemory, Me.ToString + ".CopyToLocal() Called when local memory is not being used.")

        m_stpwCopy.Start()

        Xloc = New Single(M + 1, NomCols + 1) {}
        AlocLoc = New Single(M + 1, NomCols + 1) {}
        FlocLoc = New Single(M + 1, NomCols + 1) {}
        bloc = New Single(M + 1, NomCols + 1) {}
        cloc = New Single(M + 1, NomCols + 1) {}
        dloc = New Single(M + 1, NomCols + 1) {}
        eloc = New Single(M + 1, NomCols + 1) {}

        'ReDim Xloc(M + 1, NomCols + 1)
        'ReDim AlocLoc(M + 1, NomCols + 1)
        'ReDim FlocLoc(M + 1, NomCols + 1)
        'ReDim bloc(M + 1, NomCols + 1)
        'ReDim cloc(M + 1, NomCols + 1)
        'ReDim dloc(M + 1, NomCols + 1)
        'ReDim eloc(M + 1, NomCols + 1)


        For i As Integer = 0 To M + 1
            For j As Integer = 0 To NomCols + 1
                Xloc(i, j) = X(i, j, ip)
                AlocLoc(i, j) = Aloc(i, j, ip)
                FlocLoc(i, j) = Floc(i, j, ip)
                cloc(i, j) = C(i, j, ip)
                dloc(i, j) = d(i, j, ip)
                eloc(i, j) = e(i, j, ip)
                bloc(i, j) = Bcw(i, j, ip)
            Next
        Next

        m_stpwCopy.Stop()

    End Sub


    Private Sub CopyStartEndRowCol()

        If Me.bUseLocalMemory Then
            m_stpwCopy.Start()

            iStartRowLoc = New Integer(NomCols) {}
            iEndRowLoc = New Integer(NomCols) {}
            jordLoc = New Integer(NomCols) {}
            jStartColLoc = New Integer(M) {}
            jEndColLoc = New Integer(M) {}

            DepthLoc = New Single(M + 1, NomCols + 1) {}

            'ReDim iStartRowLoc(NomCols)
            'ReDim iEndRowLoc(NomCols)
            'ReDim jordLoc(NomCols)
            'ReDim jStartColLoc(M)
            'ReDim jEndColLoc(M)

            'ReDim DepthLoc(M + 1, NomCols + 1)

            'Dim j As Integer
            'Dim i As Integer
            'For i = 1 To M
            '    jStartColLoc(i) = jStartCol(i)
            '    jEndColLoc(i) = jEndCol(i)
            'Next

            'For j = 1 To NomCols
            '    Me.iStartRowLoc(j) = iStartRow(j)
            '    iEndRowLoc(j) = iEndRow(j)
            '    jordLoc(j) = jord(j)
            'Next

            'For i = 0 To M + 1
            '    For j = 0 To NomCols + 1
            '        DepthLoc(i, i) = Depth(i, j)
            '    Next
            'Next
            'ReDim DepthLoc(M + 1, NomCols + 1)
            Array.Copy(Me.Depth, Me.DepthLoc, Me.Depth.Length)

            Array.Copy(Me.iStartRow, Me.iStartRowLoc, NomCols + 1)
            Array.Copy(Me.iEndRow, Me.iEndRowLoc, NomCols + 1)
            Array.Copy(Me.jord, Me.jordLoc, NomCols + 1)

            Array.Copy(Me.jStartCol, Me.jStartColLoc, M + 1)
            Array.Copy(Me.jEndCol, Me.jEndColLoc, M + 1)

            m_stpwCopy.Stop()
        End If


    End Sub


    Private Sub UpdateCoreData(ip As Integer)
        m_stpwCopy.Start()
        For i As Integer = 0 To M + 1
            For j As Integer = 0 To NomCols + 1
                X(i, j, ip) = Xloc(i, j)
                '  Aloc(i, j, ip) = Alocloc(i, j)
                '  Floc(i, j, ip) = Flocloc(i, j)
            Next
        Next
        m_stpwCopy.Stop()
    End Sub

    ''' <summary>
    ''' Solve the grid diffusion matrix on local arrays copied from the core data
    ''' </summary>
    ''' <param name="ip">Index of group</param>
    ''' <remarks>
    ''' Run if <see cref="bUseLocalMemory"></see> = true. Use for big models run on a NUMA (non uniform memory access) computer. 
    ''' This can run faster on a normal multicore computer if the model is big enough i.e. One Degree World model.
    ''' </remarks>
    Private Sub SolveGrid_LocalMemory(ByVal ip As Integer)
        'this routine solves for equilibrium field of concentrations x over a grid
        ' Xloc(i,j) is equilibrium concentration of x in grid cell i,j
        'am(i,j) is total loss rate of x from cell i,j...NB:am(i,j)<0 !!!!!!
        'b(i,j) is loss rate from element i-1 to i in column j of grid
        'c(i,j) is loss rate from element i+1 to i in column j of grid
        'd(i,j) is loss rate from element j to element j+1 in row i of grid
        'e(i,j) is loss rate from element j to element j-1 in row i of grid
        'f(i,j) is forcing input to element i,j from sources outside the grid
        'm is number of rows (i) in grid
        'NomCols is number of columns (j) in grid
        'tol is tolerance limit for change in iterative solution
        'jord(k) is which column j to do as k=1, k=2,...,k=n (iteration order)
        'w is SOR overrelaxation parameter-found 1.25 to be good for typical problems
        Dim iter As Integer, j As Integer, i As Integer, jj As Integer, ic As Integer

        Dim alfa(,) As Single
        Dim gam(,) As Single
        Dim rhs(,) As Single
        Dim G() As Single
        Dim Xold(,) As Single
        ReDim alfa(M + 1, NomCols + 1)
        ReDim gam(M + 1, NomCols + 1)
        ReDim rhs(M + 1, NomCols + 1)
        ReDim G(M + 1)
        ReDim Xold(M + 1, NomCols + 1)
        Dim alfa2(,) As Single
        Dim gam2(,) As Single
        Dim G2() As Single
        ReDim G2(NomCols + 1)
        ReDim alfa2(M + 1, NomCols + 1)
        ReDim gam2(M + 1, NomCols + 1)

        Dim totDiff As Single
        Dim totOld As Single
        Dim totdiff2 As Single

        Dim Wold As Single = W

        'System.Console.WriteLine("SolveGrid() " & ip.ToString)
        Try

            Me.CopyToLocal(ip)

            'first compute LU decomposition elements for each column j
            'If StopRun = 1 Then Exit Sub
            For i = 0 To M + 1
                For j = 0 To NomCols + 1
                    Xold(i, j) = Xloc(i, j)
                Next
            Next
            For j = 1 To NomCols
                'Xold(1, j) = Xloc(1, j, ip)
                If AlocLoc(iStartRowLoc(j), j) = 0 Then AlocLoc(iStartRowLoc(j), j) = -1.0 'E+30
                alfa(iStartRowLoc(j), j) = AlocLoc(iStartRowLoc(j), j)
                gam(iStartRowLoc(j), j) = cloc(iStartRowLoc(j), j) / alfa(iStartRowLoc(j), j)
                'For i = 1 To M
                'Xold(i, j) = Xloc(i, j, ip)
                'Next
                For i = iStartRowLoc(j) + 1 To iEndRowLoc(j)
                    If AlocLoc(i, j) = 0 Then AlocLoc(i, j) = -1.0 'E+30
                    alfa(i, j) = AlocLoc(i, j) - bloc(i, j) * gam(i - 1, j)
                    gam(i, j) = cloc(i, j) / alfa(i, j)
                Next
            Next
            If alternateRowCol Then
                For i = 1 To M
                    'Xold(i, 1) = Xloc(i, 1, ip)
                    If jStartColLoc(i) <= jEndColLoc(i) Then 'if the row is not all land
                        If AlocLoc(i, jStartColLoc(i)) = 0 Then AlocLoc(i, jStartColLoc(i)) = -1.0 'E+30
                        alfa2(i, jStartColLoc(i)) = AlocLoc(i, jStartColLoc(i))
                        gam2(i, jStartColLoc(i)) = e(i, jStartColLoc(i) + 1, ip) / alfa2(i, jStartColLoc(i))
                    End If
                    For j = jStartColLoc(i) + 1 To jEndColLoc(i)
                        'Xold(i, j) = Xloc(i, j, ip)
                        If AlocLoc(i, j) = 0 Then AlocLoc(i, j) = -1.0 'E+30
                        alfa2(i, j) = AlocLoc(i, j) - dloc(i, j - 1) * gam2(i, j - 1)
                        gam2(i, j) = eloc(i, j + 1) / alfa2(i, j)
                    Next
                Next
            End If
            'now begin block Gauss-Seidel/SOR iteration over columns of grid
            'at each iteration, solve explicitly for values in each column given
            'current estimates of "forcing" input from other columns based on their
            'current estimates
            iter = 0
iterate:
            For jj = 1 To NomCols

                j = jordLoc(jj)
                For i = iStartRowLoc(j) To iEndRowLoc(j)
                    rhs(i, j) = -FlocLoc(i, j) - dloc(i, j - 1) * Xloc(i, j - 1) - eloc(i, j + 1) * Xloc(i, j + 1)
                Next
                rhs(iStartRowLoc(j), j) = rhs(iStartRowLoc(j), j) - bloc(iStartRowLoc(j), j) * Xloc(iStartRowLoc(j) - 1, j)
                rhs(iEndRowLoc(j), j) = rhs(iEndRowLoc(j), j) - cloc(iEndRowLoc(j), j) * Xloc(iEndRowLoc(j) + 1, j)
                'now solve for Xloc(i,j) over i using these forcing inputs to one dimensional
                'tridiagonal solver
                G(iStartRowLoc(j)) = rhs(iStartRowLoc(j), j) / alfa(iStartRowLoc(j), j)
                'IF iflag > 0 THEN FOR i = 1 TO m: PRINT Xloc(i, j), xold(i, j): NEXT: STOP
                For i = iStartRowLoc(j) + 1 To iEndRowLoc(j)
                    G(i) = (rhs(i, j) - bloc(i, j) * G(i - 1)) / alfa(i, j)
                Next

                Xloc(iEndRowLoc(j), j) = G(iEndRowLoc(j))
                For i = iEndRowLoc(j) - 1 To iStartRowLoc(j) Step -1
                    Xloc(i, j) = G(i) - gam(i, j) * Xloc(i + 1, j)
                Next

                For i = iStartRowLoc(j) To iEndRowLoc(j)
                    Xloc(i, j) = (1 - W) * Xold(i, j) + W * Xloc(i, j)
                Next
            Next

            ic = 0
            totDiff = 0
            totdiff2 = 0
            totOld = 0
            For j = 1 To NomCols
                For i = iStartRowLoc(j) To iEndRowLoc(j)
                    If DepthLoc(i, j) > 0 Then

                        If Xloc(i, j) > 0.0000000001 And Math.Abs((Xloc(i, j) - Xold(i, j)) / (Xold(i, j) + 1.0E-20)) > Tol * timeStep Then
                            ic = ic + 1
                        End If
                        totDiff = totDiff + Math.Abs(Xloc(i, j) - Xold(i, j))
                        totdiff2 = totdiff2 + Xloc(i, j) - Xold(i, j)

                        Xold(i, j) = Xloc(i, j)
                        If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                            Xold(i, j) = 0
                        End If
                        totOld = totOld + Xold(i, j)
                    End If
                Next
            Next

            If alternateRowCol Then
                For i = 1 To M
                    ' If StopRun = 1 Then Exit Sub
                    'j = jord(jj)
                    For j = jStartColLoc(i) To jEndColLoc(i)
                        rhs(i, j) = -FlocLoc(i, j) - bloc(i, j) * Xloc(i - 1, j) - cloc(i, j) * Xloc(i + 1, j)
                    Next
                    rhs(i, jStartColLoc(i)) = rhs(i, jStartColLoc(i)) - dloc(i, jStartColLoc(i) - 1) * Xloc(i, jStartColLoc(i) - 1)
                    rhs(i, jEndColLoc(i)) = rhs(i, jEndColLoc(i)) - eloc(i, jEndColLoc(i) + 1) * Xloc(i, jEndColLoc(i) + 1)
                    'now solve for Xloc(i,j) over i using these forcing inputs to one dimensional
                    'tridiagonal solver
                    G2(jStartColLoc(i)) = rhs(i, jStartColLoc(i)) / alfa2(i, jStartColLoc(i))
                    For j = jStartColLoc(i) To jEndColLoc(i)
                        G2(j) = (rhs(i, j) - dloc(i, j - 1) * G2(j - 1)) / alfa2(i, j)
                    Next

                    Xloc(i, jEndColLoc(i)) = G2(jEndColLoc(i))
                    For j = jEndColLoc(i) - 1 To jStartColLoc(i) Step -1
                        Xloc(i, j) = G2(j) - gam2(i, j) * Xloc(i, j + 1)
                    Next

                    For j = jStartColLoc(i) To jEndColLoc(i)
                        Xloc(i, j) = (1 - W) * Xold(i, j) + W * Xloc(i, j)
                    Next
                Next

                totDiff = 0
                totdiff2 = 0
                totOld = 0
                ic = 0
                For i = 1 To M
                    For j = jStartColLoc(i) To jEndColLoc(i)
                        If DepthLoc(i, j) > 0 Then

                            If Xloc(i, j) > 0.0000000001 And Math.Abs((Xloc(i, j) - Xold(i, j)) / (Xold(i, j) + 1.0E-20)) > Tol * timeStep Then
                                ic = ic + 1
                            End If

                            totDiff = totDiff + Math.Abs(Xloc(i, j) - Xold(i, j))
                            totdiff2 = totdiff2 + Xloc(i, j) - Xold(i, j)

                            Xold(i, j) = Xloc(i, j)
                            If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                                Xold(i, j) = 0 ': Stop
                            End If
                            totOld = totOld + Xold(i, j)
                        End If
                    Next j
                Next i

                'If Math.Abs(totdiff2 / totDiff) > 0.95 Then
                '    W = 1.9
                'Else
                '    W = Wold
                'End If

            End If

            iter = iter + 1
            If ic > 0 And iter < maxIter Then GoTo iterate
exitline:

            Erase alfa, gam, rhs, G, Xold
            iterThread = iterThread + iter
            If alternateRowCol Then
                iter = iter * 2
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Me.UpdateCoreData(ip)

    End Sub

    Private Sub SolveGridRow(ByVal ip As Integer)
        'this routine solves for equilibrium field of concentrations x over a grid
        ' x(i,j) is equilibrium concentration of x in grid cell i,j
        'am(i,j) is total loss rate of x from cell i,j...NB:am(i,j)<0 !!!!!!
        'b(i,j) is loss rate from element i-1 to i in column j of grid
        'c(i,j) is loss rate from element i+1 to i in column j of grid
        'd(i,j) is loss rate from element j to element j+1 in row i of grid
        'e(i,j) is loss rate from element j to element j-1 in row i of grid
        'f(i,j) is forcing input to element i,j from sources outside the grid
        'm is number of rows (i) in grid
        'NomCols is number of columns (j) in grid
        'tol is tolerance limit for change in iterative solution
        'jord(k) is which column j to do as k=1, k=2,...,k=n (iteration order)
        'w is SOR overrelaxation parameter-found 1.25 to be good for typical problems
        Dim iter As Integer, j As Integer, i As Integer, ic As Integer ', ii As Integer

        Dim alfa(,) As Single
        Dim gam(,) As Single
        Dim rhs(,) As Single
        Dim G() As Single
        Dim Xold(,) As Single
        ReDim alfa(M + 1, NomCols + 1)
        ReDim gam(M + 1, NomCols + 1)
        ReDim rhs(M + 1, NomCols + 1)
        ReDim G(NomCols + 1)
        ReDim Xold(M + 1, NomCols + 1)

        Dim totDiff As Single
        Dim totOld As Single

        Dim iindex As Integer
        Dim jindex As Integer

        'first compute LU decomposition elements for each column j
        'If StopRun = 1 Then Exit Sub
        For i = 0 To M + 1
            For j = 0 To NomCols + 1
                Xold(i, j) = X(i, j, ip)
            Next
        Next
        For i = 1 To M
            'Xold(i, 1) = X(i, 1, ip)
            If jStartCol(i) < jEndCol(i) Then 'if the row has any water
                If Aloc(i, jStartCol(i), ip) = 0 Then Aloc(i, jStartCol(i), ip) = -1.0E+30
                alfa(i, jStartCol(i)) = Aloc(i, jStartCol(i), ip)
                gam(i, jStartCol(i)) = e(i, jStartCol(i) + 1, ip) / alfa(i, jStartCol(i))
            End If
            For j = jStartCol(i) + 1 To jEndCol(i)
                'Xold(i, j) = X(i, j, ip)
                If Aloc(i, j, ip) = 0 Then Aloc(i, j, ip) = -1.0E+30
                alfa(i, j) = Aloc(i, j, ip) - d(i, j - 1, ip) * gam(i, j - 1)
                gam(i, j) = e(i, j + 1, ip) / alfa(i, j)
            Next
        Next
        'now begin block Gauss-Seidel/SOR iteration over columns of grid
        'at each iteration, solve explicitly for values in each column given
        'current estimates of "forcing" input from other columns based on their
        'current estimates
        iter = 0
iterate:
        For i = 1 To M
            ' If StopRun = 1 Then Exit Sub
            'j = jord(jj)
            For j = jStartCol(i) To jEndCol(i)
                rhs(i, j) = -Floc(i, j, ip) - Bcw(i, j, ip) * X(i - 1, j, ip) - C(i, j, ip) * X(i + 1, j, ip)
            Next
            rhs(i, jStartCol(i)) = rhs(i, jStartCol(i)) - d(i, jStartCol(i) - 1, ip) * X(i, jStartCol(i) - 1, ip)
            rhs(i, jEndCol(i)) = rhs(i, jEndCol(i)) - e(i, jEndCol(i) + 1, ip) * X(i, jEndCol(i) + 1, ip)

            'now solve for x(i,j) over i using these forcing inputs to one dimensional
            'tridiagonal solver
            G(jStartCol(i)) = rhs(i, jStartCol(i)) / alfa(i, jStartCol(i))
            For j = jStartCol(i) To jEndCol(i)
                G(j) = (rhs(i, j) - d(i, j - 1, ip) * G(j - 1)) / alfa(i, j)
            Next

            X(i, jEndCol(i), ip) = G(jEndCol(i))
            For j = jEndCol(i) - 1 To jStartCol(i) Step -1
                X(i, j, ip) = G(j) - gam(i, j) * X(i, j + 1, ip)
            Next

            For j = jStartCol(i) To jEndCol(i)
                X(i, j, ip) = (1 - W) * Xold(i, j) + W * X(i, j, ip)
            Next
        Next

        ic = 0
        totDiff = 0
        totOld = 0
        For i = 1 To M
            For j = jStartCol(i) To jEndCol(i)
                If Depth(i, j) > 0 Then

                    If X(i, j, ip) > 0.0000000001 And Math.Abs((X(i, j, ip) - Xold(i, j)) / (Xold(i, j) + 1.0E-20)) > Tol * timeStep Then
                        ic = ic + 1
                        iindex = i
                        jindex = j
                        totDiff = totDiff + Math.Abs(X(i, j, ip) - Xold(i, j))
                        totOld = totOld + Xold(i, j)
                    End If
                    If iter = maxIter - 1 Then

                    End If
                    Xold(i, j) = X(i, j, ip)
                    If Math.Abs(Xold(i, j)) < 1.0E-20 Then
                        Xold(i, j) = 0 ': Stop
                    End If

                End If
            Next j
        Next i

        iter = iter + 1
        If ic > 0 And iter < maxIter Then GoTo iterate

exitline:
        Erase alfa, gam, rhs, G, Xold
        iterThread = iterThread + iter
    End Sub

    Private Function arrangeMatrix(ByRef Amm(,,) As Single, ByRef Bcw(,,) As Single, ByRef C(,,) As Single, ByRef d(,,) As Single, ByRef e(,,) As Single, ByVal ip As Integer, ByVal M As Integer, ByVal N As Integer) As Double(,)
        'takes the Amm, Bcw, C, d and e arrays and puts them into a single compressed band diagonal array (a)

        Dim a(,) As Double
        ReDim a(N * M + 1, 2 * N + 1)
        Dim i As Integer, j As Integer
        Dim row As Integer

        For i = 1 To M
            For j = 1 To N
                row = j + N * (i - 1)
                a(row, 2 * N + 1) = C(i, j, ip)
                a(row - 1, N + 2) = e(i, j, ip)
                If Amm(i, j, ip) < 0 Then
                    a(row, N + 1) = Amm(i, j, ip)
                Else
                    a(row, N + 1) = -1
                End If
                a(row, 1) = Bcw(i, j, ip)
                a(row + 1, N) = d(i, j, ip)
            Next
        Next

        'Dim str As String
        'Dim tempstr As String
        ''Dim temp As Single
        'For i = 1 To N * M + 1
        '    str = ""
        '    For j = 1 To 2 * N + 1
        '        tempstr = Math.Round(a(i, j)).ToString
        '        str = str + tempstr + " "
        '    Next
        '    'Debug.Print(str) : Stop
        'Next
        'Debug.Print(vbCr)
        'Dim temp(,) As Single
        'ReDim temp(N * M, N * M)
        'For i = 1 To M
        '    For j = 1 To N
        '        row = j + M * (i - 1)
        '        temp(row, row) = Amm(i, j, ip)
        '        If row < N * M Then
        '            temp(row, row + 1) = e(i, j + 1, ip)
        '        End If
        '        If row < N * M - N + 1 Then
        '            temp(row, row + N) = C(i, j, ip)
        '        End If
        '        If row > 1 Then
        '            temp(row, row - 1) = d(i, j - 1, ip)
        '        End If
        '        If row > N Then
        '            temp(row, row - N) = Bcw(i, j, ip)
        '        End If
        '    Next
        'Next
        'For i = 1 To N * M
        '    str = ""
        '    For j = 1 To N * M
        '        str = str + temp(i, j).ToString + " "
        '    Next
        '    'Debug.Print(str)
        'Next
        arrangeMatrix = a

    End Function

    Private Sub bandec(ByRef a(,) As Double, ByVal totCells As Integer, ByVal N As Integer, _
        ByRef al(,) As Double, ByRef indx() As Integer, ByRef d As Single)

        Dim i As Integer, j As Integer, k As Integer, l As Integer
        Dim mm As Integer
        Dim dum As Double
        Dim TINY As Single = 1.0E-20
        Try
            mm = 2 * N + 1
            l = N

            For i = 1 To N
                For j = N + 2 - i To mm ' rearrange storage a bit
                    a(i, j - l) = a(i, j)
                Next
                l = l - 1
                For j = mm - l To mm
                    a(i, j) = 0.0
                Next
            Next
            d = 1.0
            l = N
            For k = 1 To totCells 'for each row
                dum = a(k, 1)
                i = k
                If l < totCells Then l = l + 1
                For j = k + 1 To l 'find the pivot element
                    If Math.Abs(a(j, 1)) > Math.Abs(dum) Then
                        dum = a(j, 1)
                        i = j
                    End If
                Next
                indx(k) = i
                If dum = 0.0 Then
                    a(k, 1) = TINY 'matrix is algorithmically singular
                    Debug.Assert(False, "Matrix  algorithm failed: 0 pivot found - matrix appears to be singular")
                    Throw New Exception("Matrix  algorithm failed: 0 pivot found - matrix appears to be singular")
                End If
                'displaymatrix(a, N, N)
                If i <> k Then 'interchange rows
                    d = -d
                    For j = 1 To mm 'swap elements
                        dum = a(k, j)
                        a(k, j) = a(i, j)
                        a(i, j) = dum
                    Next
                End If
                'displaymatrix(a, N, N)
                For i = k + 1 To l 'do the elimination
                    dum = a(i, 1) / (a(k, 1))
                    al(k, i - k) = dum
                    For j = 2 To mm
                        a(i, j - 1) = a(i, j) - dum * a(k, j)
                    Next
                    a(i, mm) = 0.0
                    'displaymatrix(a, N, N)
                Next
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub displaymatrix(ByRef a(,) As Double, ByVal M As Integer, ByVal N As Integer)
        Dim str As String
        Dim tempstr As String
        'Dim temp As Single
        Try
            For i As Integer = 1 To N * M
                str = ""
                For j As Integer = 1 To 2 * N + 1
                    If a(i, j) > 0 Then
                        tempstr = Math.Ceiling(a(i, j)).ToString
                    Else
                        tempstr = Math.Floor(a(i, j)).ToString
                    End If
                    str = str + tempstr + " "
                Next
                Debug.Print(str)
            Next
            Debug.Print(Environment.NewLine)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub bandks(ByRef a(,) As Double, ByVal totCells As Integer, ByVal N As Integer, ByRef al(,) As Double, ByRef indx() As Integer, ByRef b() As Single)
        Dim i As Integer, k As Integer, l As Integer
        Dim mm As Integer
        Dim dum As Single

        mm = 2 * N + 1
        l = N

        For k = 1 To totCells 'forward substitution, unscramblings rows as we go
            i = indx(k)
            If i <> k Then 'swap
                dum = b(k)
                b(k) = b(i)
                b(i) = dum
            End If
            If l < totCells Then l += 1
            For i = k + 1 To l
                b(i) -= CSng(al(k, i - k) * b(k))
            Next
        Next
        l = 1
        For i = totCells To 1 Step -1
            dum = b(i)
            For k = 2 To l
                dum -= CSng(a(i, k) * b(k + i - 1))
            Next
            b(i) = CSng(dum / (a(i, 1) + 1.0E-20))
            If l < mm Then l += 1
        Next
    End Sub

    Private Sub refillX(ByRef X(,,) As Single, ByRef b() As Single, ByVal ip As Integer, ByVal M As Integer, ByVal N As Integer)
        Dim i As Integer, j As Integer

        For i = 1 To M
            For j = 1 To N
                If b(j + N * (i - 1)) > 1.0E-20 Then
                    X(i, j, ip) = b(j + N * (i - 1))
                Else
                    X(i, j, ip) = 1.0E-21
                End If

            Next
        Next
    End Sub

    'Private Function randMatrix(ByVal M As Integer, ByVal N As Integer) As Double(,)
    '    Dim a(,) As Double
    '    Dim i As Integer
    '    ReDim a(M * N, 2 * N + 1)
    '    For i = 1 To M * N
    '        a(i, 1) = Me.m_rand.NextDouble()
    '        a(i, N) = Me.m_rand.NextDouble()
    '        a(i, N + 2) = Me.m_rand.NextDouble()
    '        a(i, 2 * N + 1) = Me.m_rand.NextDouble()
    '        a(i, N + 1) = (-1.0 - Me.m_rand.NextDouble() / 10) * (a(i, 2 * N + 1) + a(i, N + 2) + a(i, N) + a(i, 1)) 'Rnd()
    '    Next
    '    displaymatrix(a, M, N)
    '    randMatrix = a
    'End Function
#End Region

End Class
