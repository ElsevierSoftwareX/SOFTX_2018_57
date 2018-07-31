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
Imports EwECore.Ecosim
Imports EwEUtils.Core
Imports EwECore.Ecospace.Advection.cAdvectionManager

#End Region ' Imports

Namespace Ecospace.Advection

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Advection patterns calculator class.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Class cAdvection


        'ToDo 14-Jun-2015
        'Database storage of Montly advection and upwelling arrays MonthlyXvel()(,)...
        '   Done
        'UI needs to use Monthly arrays MonthlyXvel()(,)...
        '   Done
        'Remove unused elements from UI
        '   Done
        'Reorganize modeling code to remove/archive old code
        '   Done
        'Hook the new model up the to events of the old model
        '   jb 14-Jun-2016 Ok Hook up the new model to the events and wired it into the threaded Run
        '   Seems to run but I haven't checked the events in the UI
        '   Done

#Region "Public Vars"

        Public Property UpwellingThreshold As Single = 30

#End Region

#Region " Private vars "

        ''' <summary>Core to operate on.</summary>
        Private m_core As cCore = Nothing
        ''' <summary>Ecospace model to work with.</summary>
        Private m_ecospace As cEcoSpace = Nothing
        ''' <summary>Ecospace data structures to operate on.</summary>
        Private m_data As cEcospaceDataStructures = Nothing

        ''' <summary>Delegate to notify that calculations have started.</summary>
        Private m_RunStartedDelegate As ComputationStartedDelegate
        ''' <summary>Delegate to notify that calculations have progressed through another iteration.</summary>
        Private m_RunProgressDelegate As ComputationProgressDelegate
        ''' <summary>Delegate to notify that calculations have ended.</summary>
        Private m_RunCompletedDelegate As ComputationCompletedDelegate

        ''' <summary>Iteration counter.</summary>
        Private m_iter As Integer = 0
        ''' <summary>Iteration interrupt flag.</summary>
        Private m_bInterrupted As Boolean = False
        ''' <summary>Iteration results quality flag.</summary>
        Private m_bBadFlow As Boolean = False

        Private m_OrgMonthlyXvel()(,) As Single
        Private m_OrgMonthlyYvel()(,) As Single
        Private m_OrgMonthlyUpWell()(,) As Single

#End Region ' Private vars

#Region " Public access "

        Public Sub Init(ByVal core As cCore, ByVal ecospace As cEcoSpace)

            Me.m_core = core
            Me.m_data = core.m_EcoSpaceData
            Me.m_ecospace = ecospace

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the current iteration that has been computed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Iteration() As Integer
            Get
                Return Me.m_iter
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the calculations should be interrupted.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Interrupted() As Boolean
            Get
                Return Me.m_bInterrupted
            End Get
            Set(ByVal value As Boolean)
                Me.m_bInterrupted = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the calculations produced bad flows.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property BadFlow() As Boolean
            Get
                Return Me.m_bBadFlow
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the delegate to call when a computations have started.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property RunStartedCallBack() As ComputationStartedDelegate
            Get
                Return Me.m_RunStartedDelegate
            End Get
            Set(ByVal value As ComputationStartedDelegate)
                Me.m_RunStartedDelegate = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the delegate to call when a new iteration has been calculated.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ProgressCallback() As ComputationProgressDelegate
            Get
                Return Me.m_RunProgressDelegate
            End Get
            Set(ByVal value As ComputationProgressDelegate)
                Me.m_RunProgressDelegate = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the delegate to call when a computations have completed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property RunCompletedCallback() As ComputationCompletedDelegate
            Get
                Return Me.m_RunCompletedDelegate
            End Get
            Set(ByVal value As ComputationCompletedDelegate)
                Me.m_RunCompletedDelegate = value
            End Set
        End Property


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Revert XVel and YVel to their initial state.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Revert() As Boolean

            Try
                For imon As Integer = 1 To 12

                    For ir As Integer = 0 To Me.m_data.InRow + 1
                        For ic As Integer = 0 To Me.m_data.InCol + 1
                            Me.m_data.MonthlyXvel(imon)(ir, ic) = Me.m_OrgMonthlyXvel(imon)(ir, ic)
                            Me.m_data.MonthlyYvel(imon)(ir, ic) = Me.m_OrgMonthlyYvel(imon)(ir, ic)
                            Me.m_data.MonthlyUpWell(imon)(ir, ic) = Me.m_OrgMonthlyUpWell(imon)(ir, ic)
                        Next ic
                    Next ir
                    'I'm not sure but using copy might in break the UI 
                    'the layer will no longer point to the correct memory???
                    'Array.Copy(m_OrgMonthlyXvel(imon), Me.m_data.MonthlyXvel(imon), Me.m_data.MonthlyXvel(imon).Length)
                    'Array.Copy(m_OrgMonthlyYvel(imon), Me.m_data.MonthlyXvel(imon), Me.m_data.MonthlyXvel(imon).Length)
                    'Array.Copy(m_OrgMonthlyUpWell(imon), Me.m_data.MonthlyXvel(imon), Me.m_data.MonthlyXvel(imon).Length)
                Next imon

                Return True
            Catch ex As Exception
                Return False
            End Try

        End Function

#End Region ' Public access

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="XvTot"></param>
        ''' <param name="YvTot"></param>
        ''' <param name="Corio"></param>
        ''' <param name="Hstress"></param>
        ''' -------------------------------------------------------------------
        Private Sub SetVtot(ByVal XvTot(,) As Single, ByVal YvTot(,) As Single, ByVal Corio As Single, ByVal Hstress As Single)
            'sets total pressure in x and y directions for all cells
            Dim i As Integer, j As Integer
            For i = 0 To Me.m_data.InRow + 1
                For j = 0 To Me.m_data.InCol + 1
                    If Me.m_data.Depth(i, j) > 0 Then
                        XvTot(i, j) = Me.m_data.Xvloc(i, j)
                        YvTot(i, j) = Me.m_data.Yvloc(i, j)
                    End If
                Next
            Next
            'add force components due to horizontal shear along box sides
            For i = 1 To Me.m_data.InRow
                For j = 1 To Me.m_data.InCol
                    If Me.m_data.Depth(i, j) > 0 Then
                        XvTot(i, j) = CSng(XvTot(i, j) - Corio * Me.m_data.Yvel(i, j) + Hstress * (Me.m_data.Xvel(i - 1, j) + Me.m_data.Xvel(i + 1, j) - 2.0# * Me.m_data.Xvel(i, j)))
                        YvTot(i, j) = CSng(YvTot(i, j) + Corio * Me.m_data.Xvel(i, j) + Hstress * (Me.m_data.Yvel(i, j - 1) + Me.m_data.Yvel(i, j + 1) - 2.0# * Me.m_data.Yvel(i, j)))
                    End If
                Next
            Next
        End Sub



        Public Function RunPhysicsModel() As Boolean
            Dim bReturn As Boolean = True

            Try

                'just in case
                'Can't do this at the end of the run because it is/can be used by the UI
                'To tell if a run was stopped
                Me.m_bInterrupted = False

                'make a backup copy of the advection vectors
                'incase this fails
                Me.storeOrgValues()

                Dim WindXbase(,) As Single, WindYbase(,) As Single
                WindXbase = New Single(m_data.InRow + 1, m_data.InCol + 1) {}
                WindYbase = New Single(m_data.InRow + 1, m_data.InCol + 1) {}

                Me.fireRunStarted()

                For imon As Integer = 1 To 12
                    Me.m_iter = imon

                    'copy the wind velocity vectors from the 3d array into 2d
                    For ir As Integer = 0 To m_data.InRow + 1
                        For ic As Integer = 0 To m_data.InCol + 1
                            WindXbase(ir, ic) = Me.m_data.Xv(ir, ic, imon)
                            WindYbase(ir, ic) = Me.m_data.Yv(ir, ic, imon)
                        Next ic
                    Next ir

                    If Me.m_bInterrupted Then Exit For

                    Physicsmodel(WindXbase, WindYbase)

                    Me.fireProgress()

                    'copy the X and Y velocities and upwelling into monthly arrays for storage
                    'The proper monthly value will get copied back into Xvel() and Yvel() 
                    'in the Ecospace time loop if isAdvectionActive = True 
                    For ir As Integer = 0 To m_data.InRow + 1
                        For ic As Integer = 0 To m_data.InCol + 1

                            Me.m_data.MonthlyXvel(imon)(ir, ic) = Me.m_data.Xvel(ir, ic)
                            Me.m_data.MonthlyYvel(imon)(ir, ic) = Me.m_data.Yvel(ir, ic)

                            Me.m_data.MonthlyUpWell(imon)(ir, ic) = Me.m_data.UpVel(ir, ic)

                        Next ic
                    Next ir

                Next imon

                Me.m_bBadFlow = False 'Really it must be good...

            Catch ex As Exception
                'The bad flow flag was part of the old model
                'The flow must be bad if we threw an error
                Me.m_bBadFlow = True
                bReturn = False
            End Try

            If Me.m_bBadFlow Or Me.m_bInterrupted Then
                Me.Revert()
            End If

            'Clear the X,Y and Upwelling data from memory
            'so it's not used in the model by "mistake"
            Me.ClearVelocityArrays()

            Me.fireRunEnded()
            Return bReturn

        End Function

        Private Sub ClearVelocityArrays()
            Array.Clear(Me.m_data.UpVel, 0, Me.m_data.UpVel.Length)
            Array.Clear(Me.m_data.Xvel, 0, Me.m_data.UpVel.Length)
            Array.Clear(Me.m_data.Yvel, 0, Me.m_data.Yvel.Length)

            Try
                Me.m_OrgMonthlyXvel = Nothing
                Me.m_OrgMonthlyYvel = Nothing
                Me.m_OrgMonthlyUpWell = Nothing
            Catch ex As Exception

            End Try

        End Sub


        Private Sub Physicsmodel(WindXbase(,) As Single, WindYbase(,) As Single)
            'simple model to predict wind-driven velocity patterns using Hunter equations, from Flem/Flabay model
            'basic input is open sea velocity field (cm/sec) at each model grid point in arrays WindXbase(i,j), WindYbase(i,j)
            'output is surface advection velocity field in Xvel(i,j),Yvel(i,j) and upwelling velocities UpVel(i,j)
            Dim i As Integer, j As Integer
            Dim grav As Single
            Dim honw As Single, hone As Single, hosw As Single, hose As Single
            Dim saveconst As Single, ho As Single, h1 As Single

            Dim ic As Integer, jc As Integer, im As Integer, jm As Integer
            Dim alphae As Single, depth As Single, dtotal As Single, hsurf As Single
            Dim wv As Single, gd As Single, Tol As Single, jstart As Integer
            Dim vxs As Single, vxd As Single, vys As Single, vyd As Single
            Dim uconst As Single
            Dim inrowp As Integer, incolp As Integer, h(,) As Single
            Dim W As Single

            ''xxxxxxxxxxxxxxxxxxxxxxxxx
            ''Variables not use in this implementation
            'Dim windx As Single, windy As Single
            'Dim velx As Single, vely As Single
            'Dim outconst As Single, groundconst As Single, evapconst As Single
            'Dim iout As Integer, jout As Integer
            'Dim ii As Single, d1 As Single, jj As Integer
            ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            'following initialize pressure field h, arrays for soln of equil h
            'WindConst cm/sec to m/day
            Const WindConst As Single = 864.0!
            Const alpha As Single = 0.7
            Const gravcon As Single = 9.8

            inrowp = m_data.InRow + 1 : incolp = m_data.InCol + 1

            Erase h : ReDim h(inrowp, incolp)
            'ERASE dh: REDIM dh(inrowp, incolp)

            Dim A(,) As Single, b(,) As Single, c(,) As Single, d(,) As Single, e(,) As Single, f(,) As Single, jord() As Integer
            Dim Lengthcell As Single
            Dim Windx(,) As Single, Windy(,) As Single, WindXw(,) As Single, WindYw(,) As Single
            ReDim A(inrowp, incolp)
            ReDim b(inrowp, incolp)
            ReDim c(inrowp, incolp)
            ReDim d(inrowp, incolp)
            ReDim e(inrowp, incolp)
            ReDim f(inrowp, incolp)
            ReDim Windx(inrowp + 1, incolp + 1)
            ReDim Windy(inrowp + 1, incolp + 1)
            ReDim WindXw(inrowp + 1, incolp + 1)
            ReDim WindYw(inrowp + 1, incolp + 1)
            ReDim jord(incolp)
            'Erase upwell: ReDim upwell(nftype, inrowp, incolp) As Single


            'set ocean boundary heights (nw,ne,sw,se corners of grid) to zero, i.e. assume pressure field effects are in input vel field
            honw = 0 'honwa
            hone = 0 'honea
            hosw = 0 'hoswa
            hose = 0 'hosea
            Lengthcell = m_data.CellLength
            'set wind driven surface velocity for flat ocean conditions, dependent on scenario number (e.g. ipscen can be month of year)

            'velx = velxs(ipscen) : vely = velys(ipscen)

            'set W wind stress constants to use
            'windx = WindConst * velx
            'windy = WindConst * vely

            'this must convert velocity from m/day (model units) to cm/sec (100/(60*60*24))
            'cmunit inflates by 1000 so that it can be stored as an integer
            'JB Aug-2016 removed the cmunit multiplier just save results as cm/sec
            saveconst = 100.0! / (60.0! * 60.0! * 24)

            'set pressure at boundaries (0, inrowp+1, incolp+1 array rows and cols)
            'and linear system forcing input f at each cell due to water inputs
            For i = 0 To inrowp
                ho = 0 ' honw + i / inrowp * (hosw - honw)
                h1 = 0 ' hone + i / inrowp * (hose - hone)
                For j = 0 To incolp
                    h(i, j) = 0 'ho + j / incolp * (h1 - ho)  Could avoid this after time 1 if running multiple fields over time
                    If i > 0 And i <= m_data.InRow And j > 0 And j <= m_data.InCol Then
                        If m_data.Depth(i, j) = 0 Then A(i, j) = -1.0E+30 : h(i, j) = 0 : f(i, j) = 0

                        '****make sure surface and groundwater inputs are saved as actual units in map array
                        'and therefore that the const are just conversions and are corrected above
                        'If MonthlyQ = False Then
                        'f(i, j) = outconst * map(1, i, j) * rainmonth(t) + groundconst * map(2, i, j)
                        'Else
                        'f(i, j) = outconst * map(1, i, j) * rainmonth(t) + groundconst * MonthFlow(i, j)
                        'End If
                        '  If map(0, i, j) > 0 Then f(i, j) = f(i, j) + evapconst
                    End If
                    'Convert input cm/sec to m/day
                    Windx(i, j) = WindConst * WindXbase(i, j)
                    Windy(i, j) = WindConst * WindYbase(i, j)
                    WindYw(i, j) = Windy(i, j) / (Lengthcell * 1000)
                    WindXw(i, j) = Windx(i, j) / (Lengthcell * 1000)

                Next : Next

            'rescale to cell size for wind and slope constants
            ' windyw = windy / (Lengthcell * 1000)
            ' windxw = windx / (Lengthcell * 1000)
            grav = CSng(gravcon / ((Lengthcell * 1000) ^ 2) * 86400 / 0.001)
            'then set up arrays for linear equation solver to find h field
            'see definitions of a(), b(),...f() in solvegrid routine (get to that from
            'here by typing F2, then selecting solvegrid from list under  MAPSCED)
            For i = 1 To inrowp
                For j = 1 To incolp
                    jc = j - 1 : If jc < 1 Then jc = 1
                    ic = i - 1 : If ic < 1 Then ic = 1
                    im = i : If im > m_data.InRow Then im = m_data.InRow
                    jm = j : If jm > m_data.InCol Then jm = m_data.InCol
                    If m_data.Depth(ic, jm) > 0 And m_data.Depth(im, jm) > 0 Then
                        'do rates for top boundary of cell
                        Call GetDepthUp(i, j, Me.UpwellingThreshold, dtotal, hsurf, depth)
                        alphae = alpha : If depth = 0 Then alphae = 0
                        wv = WindYw(i, j) * (dtotal + alphae * hsurf)
                        gd = CSng(grav * (dtotal ^ 2 + alphae * hsurf ^ 2))
                        f(i, j) = f(i, j) + wv
                        f(i - 1, j) = f(i - 1, j) - wv
                        A(i, j) = A(i, j) - gd
                        A(i - 1, j) = A(i - 1, j) - gd
                        b(i, j) = gd
                        c(i - 1, j) = gd
                    End If
                    If m_data.Depth(im, jc) > 0 And m_data.Depth(im, jm) > 0 Then
                        'do rates for left boundary of cell
                        Call getdepthleft(i, j, Me.UpwellingThreshold, dtotal, hsurf, depth)
                        alphae = alpha : If depth = 0 Then alphae = 0
                        wv = WindXw(i, j) * (dtotal + alphae * hsurf)
                        gd = CSng(grav * (dtotal ^ 2 + alphae * hsurf ^ 2))
                        f(i, j) = f(i, j) + wv
                        f(i, j - 1) = f(i, j - 1) - wv
                        A(i, j) = A(i, j) - gd
                        A(i, j - 1) = A(i, j - 1) - gd
                        e(i, j) = gd
                        d(i, j - 1) = gd

                    End If

                Next : Next

            For i = 1 To m_data.InRow
                For j = 1 To m_data.InCol
                    If A(i, j) = 0 Then A(i, j) = -1.0E+30
                    Me.m_data.UpVel(i, j) = 0.0!
                Next : Next

            Tol = 0.00001 '0.0000001
            W = 1.25
            For i = 1 To m_data.InCol : jord(i) = i : Next
            jstart = CInt(m_data.InCol / 3)
            jord(1) = jstart : If jord(1) = 0 Then jord(1) = 1
            i = 1
            For j = jstart + 1 To m_data.InCol
                i = i + 1
                jord(i) = j
            Next
            For j = jstart - 1 To 1 Step -1
                i = i + 1
                jord(i) = j
            Next

            'solve for the sea level height field h using solvegrid linear system solver

            FastSolveGrid(h, A, b, c, d, e, f, m_data.InRow, m_data.InCol, Tol, jord, W)

            ' next solve for the velocities given the h field results from above
            grav = CSng(gravcon / (Lengthcell * 1000.0#) * 86400.0! / 0.001)
            For i = 1 To m_data.InRow + 1
                For j = 1 To m_data.InCol + 1
                    jc = j - 1 : If jc < 1 Then jc = 1
                    ic = i - 1 : If ic < 1 Then ic = 1
                    im = i : If im > m_data.InRow Then im = m_data.InRow
                    jm = j : If jm > m_data.InCol Then jm = m_data.InCol
                    vxs = 0 : vxd = 0
                    vys = 0 : vyd = 0
                    If j <= m_data.InCol Then
                        If m_data.Depth(ic, j) > 0 And m_data.Depth(im, j) > 0 Then
                            Call GetDepthUp(i, j, Me.UpwellingThreshold, dtotal, hsurf, depth)
                            alphae = alpha : If depth = 0 Then alphae = 0
                            wv = Windy(i, j)
                            vyd = wv + grav * dtotal * (h(i - 1, j) - h(i, j))
                            vys = vyd + alphae * (wv + grav * hsurf * (h(i - 1, j) - h(i, j)))

                            Me.m_data.UpVel(i, j) = Me.m_data.UpVel(i, j) + vys * hsurf
                            Me.m_data.UpVel(i - 1, j) = Me.m_data.UpVel(i - 1, j) - vys * hsurf
                            '    dh(i, j) = dh(i, j) + vyd * depth
                            '    dh(i - 1, j) = dh(i - 1, j) - vyd * depth

                        End If
                    End If
                    If i <= m_data.InRow Then
                        If m_data.Depth(i, jc) > 0 And m_data.Depth(i, jm) > 0 Then
                            Call getdepthleft(i, j, Me.UpwellingThreshold, dtotal, hsurf, depth)
                            alphae = alpha : If depth = 0 Then alphae = 0
                            wv = Windx(i, j)
                            vxd = wv + grav * dtotal * (h(i, j - 1) - h(i, j))
                            vxs = vxd + alphae * (wv + grav * hsurf * (h(i, j - 1) - h(i, j)))
                            Me.m_data.UpVel(i, j) = Me.m_data.UpVel(i, j) + vxs * hsurf
                            Me.m_data.UpVel(i, j - 1) = Me.m_data.UpVel(i, j - 1) - vxs * hsurf
                            '   dh(i, j) = dh(i, j) + vxd * depth
                            '   dh(i, j - 1) = dh(i, j - 1) - vxd * depth
                        End If
                    End If

                    'save the velocities xvel and yvel
                    'convert from m/day to cm/sec
                    m_data.Yvel(i, j) = vys * saveconst
                    m_data.Xvel(i, j) = vxs * saveconst

                    'joe, velocities below are for deep water, deeper than hsurface; ecospace probably will never use those
                    'but can be saved in some other array names like YvelDeep and XvelDeep if we want
                    ' m_data.Yvel(ipscen + nftype, i, j) = vyd * saveconst : m_data.Xvel(ipscen + nftype, i, j) = vxd * saveconst

                Next
            Next

            'finally calculate upwelling from accumulated flows (accumulated during above
            'velocity calculation, in array upwell for each cell
            uconst = -1.0! / (Lengthcell * 1000)
            For i = 1 To m_data.InRow
                For j = 1 To m_data.InCol
                    'If dscale * map(0, i, j) > hsurface Then
                    If m_data.Depth(i, j) > Me.UpwellingThreshold Then

                        Me.m_data.UpVel(i, j) = Me.m_data.UpVel(i, j) * uconst

                    Else
                        Me.m_data.UpVel(i, j) = 0
                    End If
                Next
            Next

            'JK note **: no longer need outfall value since Map() is now single
            'If isave > 0 Then outvolused = outfall: iupsave = 1
            'If isave > 0 Then iupsave = 1
            'Erase h
            Erase A, b, c, d, e, f, jord


        End Sub


        Sub FastSolveGrid(x(,) As Single, A(,) As Single, b(,) As Single, C(,) As Single, d(,) As Single, e(,) As Single, F(,) As Single, M As Integer, n As Integer, Tol As Single, jord() As Integer, W As Single)

            'this routine solves for equilibrium field of concentrations x over a grid
            ' x(i,j) is equilibrium concentration of x in grid cell i,j
            'a(i,j) is total loss rate of x from cell i,j...NB:a(i,j)<0 !!!!!!
            'b(i,j) is loss rate from element i-1 to i in column j of grid
            'c(i,j) is loss rate from element i+1 to i in column j of grid
            'd(i,j) is loss rate from element j to element j+1 in row i of grid
            'e(i,j) is loss rate from element j to element j-1 in row i of grid
            'f(i,j) is forcing input to element i,j from sources outside the grid
            'm is number of rows (i) in grid
            'n is number of columns (j) in grid
            'tol is tolerance limit for change in iterative solution
            'jord(k) is which column j to do as k=1, k=2,...,k=n (iteration order)
            'w is SOR overrelaxation parameter-found 1.25 to be good for typical problems

            Dim i As Integer, j As Integer, k As Integer, iter As Integer, iflag As Integer
            Dim jj As Integer, ic As Long, xx As Integer
            Dim alfa(,) As Single, gam(,) As Single, rhs(,) As Single, g() As Single, Xold(,) As Single

            ReDim alfa(M, n)
            ReDim gam(M, n)
            ReDim rhs(M, n)
            ReDim g(M)
            ReDim Xold(M, n + 1)
            'iexitgrid = 0
            'first compute LU decomposition elements for each column j
            For j = 1 To n
                Xold(1, j) = x(1, j)
                alfa(1, j) = A(1, j) : gam(1, j) = C(1, j) / alfa(1, j)
                For i = 2 To M
                    Xold(i, j) = x(i, j)
                    alfa(i, j) = A(i, j) - b(i, j) * gam(i - 1, j)
                    If alfa(i, j) = 0 Then alfa(i, j) = 0.000001
                    gam(i, j) = C(i, j) / alfa(i, j)
                Next
            Next
            'now begin block Gauss-Seidel/SOR iteration over columns of grid
            'at each iteration, solve explicitly for values in each column given
            'current estimates of "forcing" input from other columns based on their
            'current estimates
            iter = 0
            iflag = 0
iterate:
            'IF iflag = 1 THEN STOP
            For jj = 1 To n
                j = jord(jj)
                For i = 1 To M
                    rhs(i, j) = -F(i, j) - d(i, j - 1) * x(i, j - 1) - e(i, j + 1) * x(i, j + 1)
                Next
                rhs(1, j) = rhs(1, j) - b(1, j) * x(0, j)
                rhs(M, j) = rhs(M, j) - C(M, j) * x(M + 1, j)
                'now solve for x(i,j) over i using these forcing inputs to one dimensional
                'tridiagonal solver

                g(1) = rhs(1, j) / alfa(1, j)
                'IF iflag > 0 THEN FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT: STOP
                For i = 2 To M
                    g(i) = (rhs(i, j) - b(i, j) * g(i - 1)) / alfa(i, j)
                Next
                x(M, j) = g(M)
                For i = M - 1 To 1 Step -1
                    x(i, j) = g(i) - gam(i, j) * x(i + 1, j)
                Next
                'IF iflag > 0 THEN
                '        FOR i = 1 TO m: PRINT x(i, j), xold(i, j): NEXT
                '        PRINT FRE(-1), FRE(-2)
                '        : STOP
                'END IF
                For i = 1 To M : x(i, j) = (1 - W) * Xold(i, j) + W * x(i, j) : Next
            Next

            ic = 0
            'If iter > 90 Then Tp.Show: Tp.DrawTPmap: DoEvents
            For i = 1 To M : For j = 1 To n
                    If Math.Abs(x(i, j) - Xold(i, j)) > Tol Then
                        ic = ic + 1
                        '                If iter > 90 Then Tp.P.Line (j, i)-Step(1, 1), QBColor(15), BF: DoEvents
                    End If
                    Xold(i, j) = x(i, j)
                    If Math.Abs(Xold(i, j)) < 1.0E-20 Then Xold(i, j) = 0
                Next : Next


            '*********************************************************************
            iter = iter + 1
            If ic > 0 And iter < 100 Then GoTo iterate

exitline:
            Erase alfa, gam, rhs, g, Xold

        End Sub


        Private Sub getdepthleft(i As Integer, j As Integer, hsurface As Single, ByRef dtotal As Single, ByRef hsurf As Single, ByRef depth As Single)
            Dim ii As Integer, d1 As Single
            'returns depths across left boundary of cell i,j
            'dscale no longer required since Map() array is now single rather than Integer
            'therefore depths don't have to be rescaled
            ii = i : If ii > m_data.InRow Then ii = m_data.InRow
            Select Case j
                Case 1
                    dtotal = m_data.Depth(ii, j) '* dscale
                Case m_data.InCol + 1
                    dtotal = m_data.Depth(ii, m_data.InCol) ' * dscale
                Case Else
                    dtotal = m_data.Depth(ii, j) '* dscale
                    d1 = m_data.Depth(ii, j - 1) '* dscale
                    If d1 < dtotal Then dtotal = d1
            End Select
            hsurf = hsurface
            depth = dtotal - hsurface
            If depth < 0 Then depth = 0 : hsurf = dtotal
        End Sub


        Private Sub GetDepthUp(i As Integer, j As Integer, hsurface As Single, ByRef dtotal As Single, ByRef hsurf As Single, ByRef depth As Single)
            Dim jj As Integer, d1 As Single
            'returns depths across top boundary of cell i,j
            jj = j : If jj > m_data.InCol Then jj = m_data.InCol
            Select Case i
                Case 1
                    dtotal = m_data.Depth(i, jj) '* dscale
                Case m_data.InRow + 1
                    dtotal = m_data.Depth(m_data.InRow, jj) ' * dscale
                Case Else
                    dtotal = m_data.Depth(i, jj) '* dscale
                    d1 = m_data.Depth(i - 1, jj) '* dscale
                    If d1 < dtotal Then dtotal = d1
            End Select
            hsurf = hsurface
            depth = dtotal - hsurf
            If depth < 0 Then depth = 0 : hsurf = dtotal

        End Sub


        Private Sub storeOrgValues()
            Try

                m_OrgMonthlyXvel = New Single(12)(,) {}
                m_OrgMonthlyYvel = New Single(12)(,) {}
                m_OrgMonthlyUpWell = New Single(12)(,) {}

                For imon As Integer = 1 To 12
                    m_OrgMonthlyXvel(imon) = New Single(Me.m_data.InRow + 1, Me.m_data.InCol + 1) {}
                    m_OrgMonthlyYvel(imon) = New Single(Me.m_data.InRow + 1, Me.m_data.InCol + 1) {}
                    m_OrgMonthlyUpWell(imon) = New Single(Me.m_data.InRow + 1, Me.m_data.InCol + 1) {}

                    Array.Copy(Me.m_data.MonthlyXvel(imon), m_OrgMonthlyXvel(imon), Me.m_data.MonthlyXvel(imon).Length)
                    Array.Copy(Me.m_data.MonthlyXvel(imon), m_OrgMonthlyYvel(imon), Me.m_data.MonthlyXvel(imon).Length)
                    Array.Copy(Me.m_data.MonthlyXvel(imon), m_OrgMonthlyUpWell(imon), Me.m_data.MonthlyXvel(imon).Length)
                Next

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try


        End Sub


        Private Sub fireRunStarted()
            Try
                If (Me.m_RunStartedDelegate IsNot Nothing) Then
                    Me.m_RunStartedDelegate.Invoke()
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub fireRunEnded()
            Try
                If (Me.m_RunCompletedDelegate IsNot Nothing) Then
                    Me.m_RunCompletedDelegate.Invoke(Me.m_iter, Me.m_bInterrupted, Me.m_bBadFlow)
                End If
            Catch ex As Exception

            End Try
        End Sub


        Private Sub fireProgress()
            Try
                If (Me.m_RunProgressDelegate IsNot Nothing) Then
                    Me.m_RunProgressDelegate.Invoke(m_iter)
                End If
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Internals

#Region "Old Code"

#If 0 Then

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Run advection computations.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Run() As Boolean

            Dim Vel(Me.m_data.InRow + 1, Me.m_data.InCol + 1) As Single
            Dim VelNew(Me.m_data.InRow + 1, Me.m_data.InCol + 1) As Single
            Dim XvTot(Me.m_data.InRow + 1, Me.m_data.InCol + 1) As Single
            Dim YvTot(Me.m_data.InRow + 1, Me.m_data.InCol + 1) As Single

            'iterates to find Xvel,Yvel velocity field at cell right boundaries
            'xvel(i,j) is velocity of flow from cell i,j to cell i,j+1
            'yvel(i,j) is velocity of flow from cell i,j to cell i+1,j
            Dim i As Integer, j As Integer, Differ As Single
            Dim Th As Single, Tn As Single, jj As Integer
            '   ReDim XvLoc(Inrow + 1, Incol + 1) As Single, YvLoc(Inrow + 1, Incol + 1) As Single
            Dim XvelBase As Single, YvelBase As Single
            Dim xMax As Single = 0

            Dim Grav As Single = CSng(9.8 * 60 * 60 * 24 * 365 / (1000 * Me.m_data.CellLength))
            Dim Upwell As Single = CSng(36.5 * Me.m_data.CellLength / 3) 'value of 6000*celllength
            Dim Hstress As Single = Math.Min(0.2!, 1 / Me.m_data.CellLength)
            Dim bDone As Boolean = False

            Try
                Me.m_RunStartedDelegate.Invoke()
            Catch ex As Exception

            End Try

            'set boundary flow depths and intial velocity field
            Me.m_ecospace.initSpatialEquilibrium()

            XvelBase = Me.m_data.XVelocity
            YvelBase = Me.m_data.YVelocity

            If Math.Abs(Me.m_data.XVelocity) > Math.Abs(Me.m_data.YVelocity) Then xMax = 2 * Math.Abs(Me.m_data.XVelocity) Else xMax = 2 * Math.Abs(Me.m_data.YVelocity)
            If xMax = 0 Then xMax = 1
            For i = 0 To Me.m_data.InRow + 1
                For j = 0 To Me.m_data.InCol + 1
                    Vel(i, j) = 0 '1 - i * yvel - j * Xvel
                    '      XvLoc(i, j) = Me.m_data.XVelocity
                    '      YvLoc(i, j) = Me.m_data.YVelocity
                Next
            Next

            ' Get ready for new run
            m_iter = 0
            m_bBadFlow = False
            m_bInterrupted = False

            While (m_iter < 10000) And (m_bInterrupted = False) And (Not bDone)

                m_iter = m_iter + 1
                Differ = 0

                Try

                    SetVtot(XvTot, YvTot, Me.m_data.Coriolis, Hstress)

                    For i = 1 To Me.m_data.InRow
                        For jj = 1 To Me.m_data.InCol
                            j = Me.m_data.jord(jj)
                            If Me.m_data.Depth(i, j) > 0 Then
                                Th = 0 : Tn = 0
                                ' For ii = i - 1 To i + 1 Step 2
                                '     If Depth(ii, j) > 0 Then
                                '         th = th + Vel(ii, j) ' - (ii - i) * YvLoc(ii, j)
                                '         Tn = Tn + 1
                                '     End If
                                ' Next
                                ' For jj = j - 1 To j + 1 Step 2
                                '     If Depth(i, jj) > 0 Then
                                '         th = th + Vel(i, jj) ' - (jj - j) * XvLoc(i, jj)
                                '         Tn = Tn + 1
                                '     End If
                                ' Next
                                ' th = Grav * th

                                If Me.m_data.Depth(i, j - 1) > 0 Then
                                    Th = Th + Me.m_data.DepthX(i, j - 1) * (XvTot(i, j - 1) + Grav * Vel(i, j - 1))
                                    Tn = Tn + Me.m_data.DepthX(i, j - 1)
                                End If
                                If Me.m_data.Depth(i, j + 1) > 0 Then
                                    Th = Th + Me.m_data.DepthX(i, j) * (Grav * Vel(i, j + 1) - XvTot(i, j))
                                    Tn = Tn + Me.m_data.DepthX(i, j)
                                End If
                                If Me.m_data.Depth(i - 1, j) > 0 Then
                                    Th = Th + Me.m_data.DepthY(i - 1, j) * (YvTot(i - 1, j) + Grav * Vel(i - 1, j))
                                    Tn = Tn + Me.m_data.DepthY(i - 1, j)
                                End If
                                If Me.m_data.Depth(i + 1, j) > 0 Then
                                    Th = Th + Me.m_data.DepthY(i, j) * (Grav * Vel(i + 1, j) - YvTot(i, j))
                                    Tn = Tn + Me.m_data.DepthY(i, j)
                                End If
                                Tn = Tn * Grav + Upwell * Me.m_data.DepthA(i, j)
                                If Tn > 0 Then
                                    VelNew(i, j) = Th / Tn
                                Else
                                    VelNew(i, j) = 0
                                End If
                                Differ = Differ + Math.Abs(Vel(i, j) - VelNew(i, j)) ' * Depth(i, j)
                                Vel(i, j) = VelNew(i, j)
                            End If
                        Next
                    Next
                    '                GoTo skipiter
                    '                For i = 1 To Me.m_data.InRow
                    '                    For j = 1 To Me.m_data.InCol
                    '                        If Me.m_data.Depth(i, j) > 0 Then
                    '                            Vel(i, j) = (1 - Me.m_data.SorWv) * Vel(i, j) + Me.m_data.SorWv * VelNew(i, j)
                    '                        End If
                    '                    Next
                    '                Next
                    'skipiter:

                    ' Verify_JS 14Sep2010: Should SorWv not be Ecospace W (SOR)?
                    Me.SetVelocities(Vel, Me.m_data.SorWv, Grav, Upwell, XvTot, YvTot)

                Catch ex As Exception
                    ' Computation error
                    Return False
                End Try

                Try
                    Me.m_RunProgressDelegate.Invoke(m_iter)
                Catch ex As Exception
                    Return False
                End Try

                bDone = (Differ < 0.0000001 * xMax / Grav / Me.m_data.CellLength)

            End While

            'check velocity field
            For i = 1 To Me.m_data.InRow
                For j = 1 To Me.m_data.InCol
                    If Me.m_data.Depth(i, j) > 0 Then
                        Th = Me.m_data.Xvel(i, j - 1) - Me.m_data.Xvel(i, j) + Me.m_data.Yvel(i - 1, j) - Me.m_data.Yvel(i, j) - Upwell * Me.m_data.DepthA(i, j) * Vel(i, j)
                        If Math.Abs(Th) > 0.00001 * xMax Then
                            m_bBadFlow = True ':        Stop
                            'map.Line (j, i)-Step(1, 1), QBColor(4), BF
                        End If
                    End If
                Next
            Next

            Try
                Me.m_RunCompletedDelegate.Invoke(Me.m_iter, Me.m_bInterrupted, Me.m_bBadFlow)
            Catch ex As Exception
                Return False
            End Try

            Return True

            'If m_bBadAdvection = True Then MsgBox("Inflows and outflows do not balance at cells shown in red; recommend not using this velocity field for simulations if ecospace shows strange behavior for these cells")
        End Function


        
        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vel"></param>
        ''' <param name="SorWv"></param>
        ''' <param name="Grav"></param>
        ''' <param name="UpWell"></param>
        ''' <param name="XvToT"></param>
        ''' <param name="YvTot"></param>
        ''' -------------------------------------------------------------------
        Private Sub SetVelocities(ByRef vel(,) As Single, _
                                  ByVal SorWv As Single, ByVal Grav As Single, ByVal UpWell As Single, _
                                  ByVal XvToT(,) As Single, ByVal YvTot(,) As Single)
            Dim i As Integer
            Dim j As Integer
            For i = 0 To Me.m_data.InRow
                For j = 0 To Me.m_data.InCol
                    If Me.m_data.Depth(i, j) > 0 Then
                        If Me.m_data.Depth(i, j + 1) > 0 Then Me.m_data.Xvel(i, j) = (1 - SorWv) * Me.m_data.Xvel(i, j) + SorWv * Me.m_data.DepthX(i, j) * (XvToT(i, j) + Grav * (vel(i, j) - vel(i, j + 1))) Else Me.m_data.Xvel(i, j) = 0
                        If Me.m_data.Depth(i + 1, j) > 0 Then Me.m_data.Yvel(i, j) = (1 - SorWv) * Me.m_data.Yvel(i, j) + SorWv * Me.m_data.DepthY(i, j) * (YvTot(i, j) + Grav * (vel(i, j) - vel(i + 1, j))) Else Me.m_data.Yvel(i, j) = 0
                        Me.m_data.UpVel(i, j) = -UpWell * Me.m_data.DepthA(i, j) * vel(i, j)
                    Else
                        Me.m_data.Xvel(i, j) = 0
                        Me.m_data.Yvel(i, j) = 0
                    End If
                Next
            Next
        End Sub

                
#End If


#End Region

    End Class

End Namespace
