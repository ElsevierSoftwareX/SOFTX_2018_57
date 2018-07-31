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

Imports EwEUtils.Core
Imports System.Math


''' <summary>
''' Class to provide access to functions needed both internally to the core and 
''' externally by plugins or other things.
''' </summary>
Public Class cEcoFunctions

    Private m_core As cCore
    Private m_matrix As cMatrixCalc

    Public Sub New()
        Me.m_matrix = New cMatrixCalc()
    End Sub

    ''' <summary>
    ''' Initialize the instance to the current core.
    ''' </summary>
    ''' <param name="theCore">The core to initialize to.</param>
    Friend Sub Init(ByVal theCore As cCore)
        m_core = theCore
    End Sub

    ''' <summary>
    ''' Matrix calculation object used by various routines in the core and plugins
    ''' </summary>
    Public ReadOnly Property MatrixCalc() As cMatrixCalc
        Get
            Return m_matrix
        End Get
    End Property

    Public Function ShannonDiversityIndex(iNumLiving As Integer, ByVal Bio() As Single) As Single

        Try
            Dim sumB As Single = 0
            For i As Integer = 1 To iNumLiving
                sumB += Bio(i)
            Next

            Dim propB(iNumLiving) As Single
            For i As Integer = 1 To iNumLiving
                propB(i) = Bio(i) / sumB
            Next

            Dim Shannon As Double = 0
            For i As Integer = 1 To iNumLiving
                If propB(i) > 0 Then
                    Shannon += -propB(i) * Log(propB(i))
                End If
            Next

            Return System.Convert.ToSingle(Shannon)

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".FunctionShannonDiversityIndex() Error: " & ex.Message)

            If (Me.m_core IsNot Nothing) Then
                Dim msg As New cMessage("Error in FunctionShannonDiversityIndex() " & ex.Message, eMessageType.ErrorEncountered, eCoreComponentType.Core, eMessageImportance.Critical, EwEUtils.Core.eDataTypes.NotSet)
                Me.m_core.Messages.SendMessage(msg)
            End If
            Return 0.0

        End Try
    End Function


    Public Function KemptonsQ(iNumLiving As Integer, ttlx As Single(), _
                              ByVal Bio() As Single, ByVal Quan As Single) As Single

        'VC programmed this function 23 October 2002 from Tony Pitcher's description
        Dim BLower As Single
        Dim BUpper As Single
        Dim i As Integer
        Dim j As Integer
        Dim minB As Single
        Dim Smallest As Integer
        Dim Rank() As Integer
        Dim Used() As Boolean
        Dim Lower As Single
        Dim upper As Single
        Dim NumGr As Integer

        Dim returnValue As Single

        Try

            'Debug.Assert(m_core IsNot Nothing, Me.ToString & " not initialized properly!")
            'Dim epdata As cEcopathDataStructures = m_core.m_EcoPathData

            'We now know the current biomasses for each group = bb(i) the biomass for each group at the end of the simulation
            'Find the min and max biomass, only look at theliving groups
            KemptonsQ = 0
            ReDim Rank(iNumLiving)
            ReDim Used(iNumLiving)
            NumGr = 0
            For i = 1 To iNumLiving
                If ttlx(i) < 3 Then
                    Used(i) = True 'don't include low trophic level species in diversity index
                Else
                    NumGr = NumGr + 1
                End If
            Next

            If NumGr = 0 Then Return 0

            'if there are very few groups we better include all
            'VC Nov 2008
            If NumGr < 10 Then
                NumGr = 0
                ReDim Used(iNumLiving)
                For i = 1 To iNumLiving
                    NumGr += 1
                Next
            End If
            For i = 1 To NumGr
                minB = 1000000
                Smallest = 0
                For j = 1 To iNumLiving
                    If Used(j) = False And Bio(j) < minB Then
                        minB = Bio(j)
                        Smallest = j
                    End If
                Next
                'After each round we have the smallest remaining biomass
                If Smallest > 0 Then    'there will be some where it won't
                    Used(Smallest) = True
                    Rank(i) = Smallest
                End If
            Next
            'after i rounds we have sorted all groups after biomasses in Rank()
            'Now we can find the percentiles:
            Lower = Quan * NumGr
            upper = (1 - Quan) * NumGr
            BLower = (Lower - CInt(Lower - 0.5)) * Bio(Rank(CInt(Lower - 0.5))) + (1 - (Lower - CInt(Lower - 0.5))) * Bio(Rank(CInt(Lower - 0.5) + 1))
            BUpper = (1 - (upper - CInt(upper - 0.5))) * Bio(Rank(CInt(upper - 0.5))) + (upper - CInt(upper - 0.5)) * Bio(Rank(CInt(upper - 0.5) + 1))

            'jb 6-Jan-2014 make sure Log(BUpper / BLower) is not 1.0 to avoid a /zero error
            Dim BioRatio As Double = BUpper / BLower
            'Debug.Assert(Not BioRatio = 1.0)
            If BioRatio = 1.0 Then BioRatio = 1.0 + 0.0000000001
            'We can now calculate Kemptons Q-index:
            returnValue = CSng(NumGr / Math.Log(BioRatio) / 2)
            'Using the equation from Kemptons Species diversity index:
            'Q= St / [ 2 log(Piq*0.25ST/Piq*0.75St)] where Piq is the proportional abundance of the qth most abundant species
            'exitFunction:

            Debug.Assert(Not Single.IsInfinity(returnValue))

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, Me.ToString & ".FunctionKemptonsQ() Error: " & ex.Message)

            If (Me.m_core IsNot Nothing) Then
                Dim msg As New cMessage("Error in FunctionKemptonsQ() " & ex.Message, eMessageType.ErrorEncountered, eCoreComponentType.Core, eMessageImportance.Critical, EwEUtils.Core.eDataTypes.NotSet)
                Me.m_core.Messages.SendMessage(msg)
            End If
            Return 0.0
        End Try

        Return returnValue

    End Function

    ''' <summary>
    ''' Utility method, estimate trophic levels from diets.
    ''' </summary>
    ''' <param name="Diet"></param>
    ''' <param name="TLreturn"></param>
    ''' <remarks>
    ''' This method is totally independent of cCore; all required information
    ''' is passed in.
    ''' </remarks>
    Public Function EstimateTrophicLevels(ByVal iNumGroups As Integer, _
                                          ByVal iNumLiving As Integer, _
                                          ByVal PP() As Single, _
                                          ByVal Diet(,) As Single, _
                                          ByVal TLreturn() As Single) As Boolean

        Dim SumDC(iNumGroups) As Single
        Dim LHS(iNumGroups, iNumGroups) As Single
        Dim TL(iNumGroups) As Single
        Dim i As Integer, j As Integer
        Dim ErrCode As Integer

        Try
            For i = 1 To iNumGroups
                'TTLX(i) = 1
                TL(i) = 1
                For j = 1 To iNumGroups
                    LHS(i, j) = 0
                Next j
            Next i

            For i = 1 To iNumGroups
                SumDC(i) = 0
                For j = 1 To iNumGroups
                    SumDC(i) += Diet(i, j)
                Next j
            Next i

            'Estimation of trophic levels: TTLX
            'The DC is made to sum to one, this means that it is assumed
            'that import to strict consumers has the same trophic level as
            'other prey for the group
            For i = 1 To iNumGroups
                For j = 1 To iNumGroups
                    If PP(i) = 1 Then            'Strict Primary producer, so no diet composition (even if it may have in carbon model)
                        LHS(i, j) = 0
                    ElseIf PP(i) > 0 Then            'partly a primary producer
                        LHS(i, j) = -Diet(i, j)
                        'ElseIf SumDC(i) > 0 And SumDC(i) < 1 Then 'Consumer with import
                    ElseIf SumDC(i) > 0 And Math.Abs(SumDC(i) - 1) > 0.0001 Then 'Consumer with import
                        LHS(i, j) = -Diet(i, j) / SumDC(i)
                    Else                          'Consumer
                        LHS(i, j) = -Diet(i, j)
                    End If
                    If PP(i) > 0 And PP(i) < 1 Then
                        'Mixed producer / consumer: TTLX should reflect both roles
                        LHS(i, j) = -Diet(i, j) * (1 - PP(i))
                    End If
                Next j
                LHS(i, i) = 1 - Diet(i, i)
            Next i

            For i = iNumLiving + 1 To iNumGroups          'multidet version for
                For j = 1 To iNumGroups
                    LHS(i, j) = 0
                Next j
                LHS(i, i) = 1
            Next i

            ErrCode = Me.m_matrix.MatSEqnS(LHS, TL)   'Inverses matrix to find
            If ErrCode = 0 Then 'no error
                For i = 1 To iNumGroups : TLreturn(i) = TL(i) : Next
            End If

        Catch ex As Exception
            cLog.Write(ex, "cEcoFunctions::EstimateTrophicLevels")
            Debug.Assert(False, Me.ToString & ".EstimateTrophicLevels() Error: " & ex.Message)
            Return False
        End Try

        Return True

    End Function

End Class



Public Class cMatrixCalc

    'MatSEqnS and matluS variables
    'array dimensions used by MatSEqnS and matluS
    Public Lo As Integer
    Public Up As Integer

    Public rpvt() As Integer, cpvt() As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' MatSEqnS solves a system of n linear equations, Ax=b, and puts the 
    ''' answer in b. A is first put in LU form by matluS, then matbsS is called
    ''' to solve the system.  matrices A,b are single precision.
    ''' </summary>
    ''' <param name="A">contains coefficient matrix</param>
    ''' <param name="B">contains the right side</param>
    ''' <returns>A in LU form, solution in b</returns>
    ''' -----------------------------------------------------------------------
    Public Function MatSEqnS(ByRef A(,) As Single, ByRef B() As Single) As Integer

        '========================MatSEqnS==================================
        'MatSEqnS solves a system of n linear equations, Ax=b, and puts the
        'answer in b. A is first put in LU form by matluS, then matbsS is called
        'to solve the system.  matrices A,b are single precision.
        '
        'Parameters: A(n x n) contains coefficient matrix, b(N) contains the right side
        '
        'Returns: A in LU form, solution in b
        '===================================================================

        Dim ErrCode As Integer, bserrcode As Integer, row As Integer
        Dim X() As Single
        Dim OkToContinue As Boolean

        ErrCode = 0

        Try
            'On Local Error GoTo sseqnerr: 
            'Lo = LBound(A, 1)
            'jb in EwE5 lo boundary of the arrays was set to 1 we can not do that here so hard wire this value
            Lo = 1
            'Up = UBound(A, 1)
            Up = A.GetUpperBound(0)
            ReDim X(Up)
            ReDim rpvt(Up)
            ReDim cpvt(Up)

            ErrCode = matluS(A, OkToContinue)                      'Get LU matrix
            'If Not OkToContinue Then Error ErrCode
            If Not OkToContinue Then
                cLog.Write("Ecopath error matluS() returned False. Trophic Levels will not be computed for this run.")
                Return ErrCode
            End If
            'check dimensions of b
            'If (Lo <> LBound(B)) Or (Up <> UBound(B)) Then Error 197
            If (Up <> B.Length - 1) Then
                Debug.Assert(False)
                Return 197
            End If

            bserrcode = matbsS(A, B, X)          'Backsolve system
            'If bserrcode Then Error bserrcode
            If bserrcode <> 0 Then
                Debug.Assert(False)
                Return bserrcode
            End If

            For row = Lo To Up
                B(row) = X(row)                         'Put solution in b for return
            Next row

        Catch ex As Exception

            Debug.Assert(False)
            Return 0
        End Try



        '        If ErrCode Then Error ErrCode
        'sseqnexit:
        '        Erase X, rpvt, cpvt
        '        MatSEqnS = ErrCode
        '        Exit Function
        'sseqnerr:
        '        ErrCode = (Err() + 5) Mod 200 - 5
        '        Resume sseqnexit
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' matluS does Gaussian elimination with total pivoting to put a square, single
    ''' precision matrix in LU form. The multipliers used in the row operations to
    ''' create zeroes below the main diagonal are saved in the zero spaces.
    ''' </summary>
    ''' <param name="A">Matrix</param>
    ''' <param name="OkToContinue"></param>
    ''' <returns>A in LU form with corresponding pivot vectors; the total number 
    ''' of pivots in count, which is used to find the sign of the determinant.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function matluS(ByRef A(,) As Single, ByRef OkToContinue As Boolean) As Integer

        '========================matluS====================================
        'matluS does Gaussian elimination with total pivoting to put a square, single
        'precision matrix in LU form. The multipliers used in the row operations to
        'create zeroes below the main diagonal are saved in the zero spaces.
        '
        'Parameters: A(n x n) matrix, rpvt(n) and cpvt(n) permutation vectors
        '            used to index the row and column pivots
        '
        'Returns: A in LU form with corresponding pivot vectors; the total number of
        '         pivots in count, which is used to find the sign of the determinant.
        '===================================================================

        Dim row As Integer, col As Integer, pvt As Integer, max As Single, R As Integer
        Dim CCC As Integer, bestrow As Integer, bestcol As Integer
        Dim rownorm() As Single
        Dim seps As Single, oldmax As Single, rp As Integer, cp As Integer
        Dim count As Integer
        Dim Temp As Single

        'Dim ErrCode As Integer
        'On Local Error GoTo sluerr: ErrCode = 0

        Try
            'Checks if A is square, returns error code if not
            'If Up <> UBound(A, 2) Then
            If Up <> A.GetUpperBound(1) Then

                'If Not (Lo = LBound(A, 2) And Up = UBound(A, 2)) Then

                Debug.Assert(False)
                Return 198
            End If

            ReDim rownorm(Up)
            count = 0                            'initialize count, OkToContinue
            OkToContinue = True

            For row = Lo To Up                  'initialize rpvt and cpvt
                rpvt(row) = row
                cpvt(row) = row
                rownorm(row) = 0.0                'find the row norms of A()
                For col = Lo To Up
                    rownorm(row) = rownorm(row) + Math.Abs(A(row, col))
                Next col
                'If rownorm(Row) = 0! Then        'if any rownorm is zero, the matrix
                '    OkToContinue = 0                   'is singular, set error, exit and do
                '    Error 199                      'not OkToContinue
                'End If
            Next row

            For pvt = Lo To (Up - 1)
                'Find best available pivot
                max = 0.0                         'checks all values in rows and columns not
                For row = pvt To Up             'already used for pivoting and finds the
                    R = rpvt(row)                'number largest in absolute value relative
                    For col = pvt To Up          'to its row norm
                        CCC = cpvt(col)
                        If (rownorm(R) <> 0) Then
                            Temp = Math.Abs(A(R, CCC)) / rownorm(R)
                        End If

                        If Temp > max Then
                            max = Temp
                            bestrow = row          'save the position of new max!
                            bestcol = col
                        End If
                    Next col
                Next row

                If max = 0.0 Then                 'if no nonzero number is found, A is
                    System.Console.WriteLine("WARNING Error in matluS() max = 0")
                    OkToContinue = False                   'singular, send back error, do not OkToContinue
                    Return 199
                ElseIf pvt > 1 Then              'check if drop in pivots is too much
                    If max < (seps * oldmax) Then
                        OkToContinue = False
                        Return 199
                    End If
                End If

                oldmax = max
                If rpvt(pvt) <> rpvt(bestrow) Then
                    count = count + 1                    'if a row or column pivot is
                    'SWAP rpvt(pvt), rpvt(bestrow)      'necessary, count it and permute
                    Temp = rpvt(pvt)
                    rpvt(pvt) = rpvt(bestrow)
                    rpvt(bestrow) = CInt(Temp)
                End If                                  'rpvt or cpvt. Note: the rows and
                If cpvt(pvt) <> cpvt(bestcol) Then    'columns are not actually switched,
                    count = count + 1                    'only the order in which they are
                    'SWAP cpvt(pvt), cpvt(bestcol)      'used.
                    Temp! = cpvt(pvt)
                    cpvt(pvt) = cpvt(bestrow)
                    cpvt(bestrow) = CInt(Temp)
                End If
                'Eliminate all values below the pivot
                rp = rpvt(pvt)
                cp = cpvt(pvt)
                For row = (pvt + 1) To Up
                    R = rpvt(row)

                    If (A(rp, cp) <> 0) Then
                        A(R, cp) = -A(R, cp) / A(rp, cp)  'save multipliers
                    End If
                    For col = (pvt + 1) To Up
                        CCC = cpvt(col)                      'complete row operations
                        A(R, CCC) = A(R, CCC) + A(R, cp) * A(rp, CCC)
                    Next col
                Next row
            Next pvt

            If A(rpvt(Up), cpvt(Up)) = 0.0 Then
                'if last pivot is zero or pivot drop is
                'too large, A is singular, send back error
                OkToContinue = False
                'DispError 0, "Last pivot is zero or pivot drop is too large."
                'Debug.Assert(False, "matlus: Last pivot is zero or pivot drop is too large.")
                Return 199
            ElseIf (Math.Abs(A(rpvt(Up), cpvt(Up))) / rownorm(rpvt(Up))) < (seps * oldmax) Then
                'if pivot is not identically zero then
                'OkToContinue remains TRUE
                'Debug.Assert(False, "matlus: Last pivot is zero or pivot drop is too large.")
                Return 199
            End If

            'If ErrCode Then
            '    'DispError 0, "pivot is not identically zero."
            '    'Error ErrCode
            '    'jb I don't think this can happen
            '    Debug.Assert(False)
            '    Return ErrCode
            'End If

        Catch ex As Exception
            OkToContinue = False
            Debug.Assert(False, ex.Message)
            Return 199
        End Try

        'sluexit:
        '        matluS = ErrCode
        '        Exit Function

        'sluerr:
        '        ErrCode = Err()
        '        If ErrCode < 199 Then OkToContinue = False
        '        Resume sluexit
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' matbsS takes a matrix in LU form, found by matluS, and a vector b
    ''' and solves the system Ux=Lb for x. matrices A,b,x are single precision.
    ''' </summary>
    ''' <param name="A">LU matrix</param>
    ''' <param name="B">right side</param>
    ''' <param name="X">solution</param>
    ''' <returns>solution in x, b is modified, rest unchanged</returns>
    ''' -----------------------------------------------------------------------
    Public Function matbsS(ByRef A(,) As Single, ByRef B() As Single, ByRef X() As Single) As Integer

        '========================matbsS=====================================
        'matbsS takes a matrix in LU form, found by matluS, and a vector b
        'and solves the system Ux=Lb for x. matrices A,b,x are single precision.
        '
        'Parameters: LU matrix in A, corresponding pivot vectors in rpvt and cpvt,
        '            right side in b
        '
        'Returns: solution in x, b is modified, rest unchanged
        '===================================================================

        Dim pvt As Integer, CCC As Integer, col As Integer, row As Integer, R As Integer

        Try

            'On Local Error GoTo sbserr: matbsS = 0
            'do row operations on b using the multipliers in L to find Lb
            For pvt = Lo To (Up - 1)
                CCC = cpvt(pvt)
                For row = (pvt + 1) To Up
                    R = rpvt(row)
                    B(R) = B(R) + A(R, CCC) * B(rpvt(pvt))
                Next row
            Next pvt

            'backsolve Ux=Lb to find x
            For row = Up To Lo Step -1
                CCC = cpvt(row)
                R = rpvt(row)
                X(CCC) = B(R)
                For col = (row + 1) To Up
                    X(CCC) = X(CCC) - A(R, cpvt(col)) * X(cpvt(col))
                Next col

                If A(R, CCC) <> 0 Then
                    X(CCC) = X(CCC) / A(R, CCC)
                End If
            Next row

        Catch ex As Exception
            'any return value other the zero is considered an error
            Return 1
            Exit Function

        End Try

        'no error
        Return 0

        'sbsexit:
        '        Exit Function
        'sbserr:
        '        matbsS = Err()
        '        Resume sbsexit
    End Function

End Class
