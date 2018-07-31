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

Imports EwECore.Ecopath
Imports EwECore.EcoSim
Imports System
Imports System.Threading

#Region "xxx DEAD CODE xxx"


#If 0 Then


Public Class cMonteCarloThread
    Public ES As New cEcoSimModel
    Public ESdata As New cEcosimDatastructures
    Public EP As New cEcoPathModel
    Public EPdata As New cEcopathDataStructures
    Public StanzaData As New cStanzaDatastructures

    Public signalState As New System.Threading.ManualResetEvent(True)

    Public pmean(,) As Single, CVpar(,) As Single, iter As Integer
    Public nLivingGroups As Integer
    Public ngroups As Integer
    Public parLimit(,,) As Single

    Private threadID As Integer

    'Public Sub New(ByVal trID As Integer)
    '    threadID = trID
    'End Sub

    Public Enum eMCParams
        Biomass = 1
        PB = 2
        QB = 3
        EE = 4
        BA = 5
    End Enum
    Public Sub New(ByVal trid As Integer)
        threadID = trid
    End Sub
    Public Sub init(ByVal ng As Integer, ByVal nl As Integer)
        ngroups = ng
        nLivingGroups = nl
    End Sub

    Public Sub run(ByVal obParam As Object)
        Try
            signalState.Reset()
            BalanceEcopathWithNewPars(pmean, CVpar, iter) '**********
            ES.Init(True)
            ES.Run()
            signalState.Set()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Function BalanceEcopathWithNewPars(ByVal ParCurVal(,) As Single, ByVal CVpar(,) As Single, ByRef iter As Integer) As Boolean
        'EwE5 StartEcosimWithNewPars(ByVal Pstartup(,) As Single, ByVal CVpar(,) As Single, ByVal iter As Long)
        Dim igrp As Integer
        Dim bEcopathNeedsBalancing As Boolean

        Try

            'AbortRun = True
            bEcopathNeedsBalancing = True
            Do While bEcopathNeedsBalancing
                iter = iter + 1
                EPdata.CopyInputToModelArrays() 'MakeUnknownUnknown())

                For igrp = 1 To nLivingGroups                               ' Using default if not
                    If EP.missing(igrp, 1) = False Then                   ' Then B is an input par
                        EPdata.B(igrp) = ChooseFeasiblePar(ParCurVal(eMCParams.Biomass, igrp), CVpar(eMCParams.Biomass, igrp), parLimit(0, eMCParams.Biomass, igrp), parLimit(1, eMCParams.Biomass, igrp))
                        EPdata.BA(igrp) = ChooseFeasibleBA(EPdata.B(igrp), ParCurVal(eMCParams.BA, igrp), CVpar(eMCParams.BA, igrp), parLimit(0, eMCParams.BA, igrp), parLimit(1, eMCParams.BA, igrp))
                    End If
                    If EP.missing(igrp, 2) = False Then                   ' Then B is an input par
                        EPdata.PB(igrp) = ChooseFeasiblePar(ParCurVal(eMCParams.PB, igrp), CVpar(eMCParams.PB, igrp), parLimit(0, eMCParams.PB, igrp), parLimit(1, eMCParams.PB, igrp))
                    End If
                    If EP.missing(igrp, 4) = False Then                   ' Then B is an input par
                        EPdata.EE(igrp) = ChooseFeasiblePar(ParCurVal(eMCParams.EE, igrp), CVpar(4, igrp), parLimit(0, eMCParams.EE, igrp), parLimit(1, eMCParams.EE, igrp))
                    End If
                Next igrp

                ES.InitStanza()
                'Estimate basic params
                '     Exit_Sub_Missing_Par = 1
                If Not EP.EstimateParameters() Then
                    Return False
                End If

                EP.DetritusCalculations()
                'Insufficient parameters
                'If Exit_Sub_Missing_Par = 0 Then
                '    AbortRun = True
                'End If

                bEcopathNeedsBalancing = False
                For igrp = 1 To ngroups
                    If EPdata.EE(igrp) > 1.0005 Or EPdata.EE(igrp) < 0 Then
                        'this loop did not balance Ecopath
                        bEcopathNeedsBalancing = True
                        Exit For
                    End If
                Next

                'tell the interface
                'EcopathIterationsProgress(iter)

                ' System.Console.WriteLine("Ecopath iteration " & iter.ToString)
                'tell the manager that an iteration has completed
                If iter > 2000 Then Exit Function 'frmBvary.lblNoGood.Caption = "Cannot find feasible Ecopath model; Quitting": Exit Sub

            Loop

            '  System.Console.WriteLine("Monte Carlo " & iter.ToString & " iteration(s) to balance Ecopath")

        Catch ex As Exception
            Debug.Assert(False, ex.StackTrace)
            cLog.Write(ex)
            Throw New ApplicationException(Me.ToString & ".BalanceEcopathWithNewPars()", ex)
        End Try

        Return True

    End Function

    Private Function ChooseFeasiblePar(ByVal xbar As Single, ByVal CV As Single, ByVal ParMin As Single, ByVal ParMax As Single) As Single
        Dim X As Single, ict As Integer
        '  Static Answer As Object

        Debug.Assert(ParMin <> ParMax, Me.ToString & ".ChooseFeasiblePar() ParMax = ParMin!!!!!")

        Do
            X = xbar * (1 + CV * RandomNormal())
            If X >= ParMin And X <= ParMax Then
                ChooseFeasiblePar = X
                Exit Function
            End If
            ict = ict + 1
            If ict > 10000 Then
                'If Answer <> vbCancel Then
                '    Answer = MsgBox("Can't find acceptable parameter, using mean", vbOKCancel)
                'End If
                System.Console.WriteLine("ChooseFeasiblePar() Can't find acceptable parameter, using mean")
                ChooseFeasiblePar = xbar
                Exit Function
            End If
        Loop
    End Function

    Private Function RandomNormal() As Single
        Dim i As Integer, X As Single
        X = -6
        For i = 1 To 12 : X = X + Rnd() : Next
        RandomNormal = X
    End Function

    Private Function ChooseFeasibleBA(ByVal Biomass As Single, ByVal xbar As Single, ByVal CV As Single, ByVal ParMin As Single, ByVal ParMax As Single) As Single
        Dim X As Single, ict As Integer
        Do
            X = xbar + Biomass * (CV * RandomNormal())
            If X >= ParMin And X <= ParMax Then
                ChooseFeasibleBA = X
                Exit Function
            End If
            ict = ict + 1
            If ict > 10000 Then
                'System.Console.WriteLine("Monte Carlo Can't find acceptable parameter for BA, using mean.")
                'If done = False Then RetVal = MsgBox("Can't find acceptable parameter, using mean. Press 'Cancel' to avoid this message", vbOKCancel)
                'If RetVal = vbCancel Then done = True
                ChooseFeasibleBA = 0    'xbar
                Exit Function
            End If
        Loop
    End Function

End Class

#End If

#End Region