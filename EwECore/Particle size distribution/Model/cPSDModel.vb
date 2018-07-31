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

#Region "Imports"

Option Strict On
Option Explicit On
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region 'Imports

Public Class cPSDModel

#Region " Variables "

    Private m_bSuppressMsgs As Boolean
    Private m_msgPub As New cMessagePublisher

    Friend m_Data As cEcopathDataStructures
    Friend m_stanza As cStanzaDatastructures
    Friend m_psd As cPSDDatastructures

#End Region ' Variables

#Region " Public methods "

    Public Function Run() As Boolean

        ' Sanity check
        If Not Me.m_psd.Enabled Then Return False

        'Is there any missing input
        If CheckMissingInputParameters() Then
            'Yes: return failure
            Return False
        Else
            'No: estimate growth parameters 
            EstimateGrowthParameters()
            'Is PSD estimation successful?
            If EstimatePSD() Then
                'Yes: more estimation and return success
                EstimateSizeWeight()
                If m_psd.NPtsMovAvg > 0 Then EstimatePSDMovAvg()
                Return True
            Else
                'No: return failure
                Return False
            End If
        End If
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Exposes the MessagePublisher instance so that the core can add message handlers
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Messages() As cMessagePublisher
        Get
            Return m_msgPub
        End Get
    End Property

#End Region ' Public methods

#Region " Helper methods "

    Private Function CheckMissingInputParameters() As Boolean

        Dim str As String = ""
        Dim msg As cMessage = Nothing
        Dim vs As cVariableStatus = Nothing
        Dim bMissing As Boolean = False 'No missing data

        ' Check each group
        For i As Integer = 1 To m_Data.NumLiving
            ' Found an error?
            If (m_psd.WinfInput(i) < 0 And m_psd.LooInput(i) < 0) Or m_Data.vbK(i) <= 0 Then

                ' #Yes: generate - or append to - message
                If (msg Is Nothing) Then
                    msg = New cMessage(My.Resources.CoreMessages.PSD_MISSING_INPUT, eMessageType.TooManyMissingParameters, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                    msg.Suppressable = False
                End If
                bMissing = True

                ' JS: add variable status to report missing L or W 
                If (m_psd.WinfInput(i) < 0 And m_psd.LooInput(i) < 0) Then
                    str = cStringUtils.Localize(My.Resources.CoreMessages.PSD_REQ_LW, Me.m_Data.GroupName(i))
                    vs = New cVariableStatus(eStatusFlags.ErrorEncountered, str, eVarNameFlags.Name, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                    msg.AddVariable(vs)
                End If

                ' JS: add variable status to report missing K in VGBF
                If (m_Data.vbK(i) <= 0) Then
                    str = cStringUtils.Localize(My.Resources.CoreMessages.PSD_REQ_K_VBGF, Me.m_Data.GroupName(i))
                    vs = New cVariableStatus(eStatusFlags.ErrorEncountered, str, eVarNameFlags.Name, eDataTypes.EcoPathGroupInput, eCoreComponentType.EcoPath, i)
                    msg.AddVariable(vs)
                End If

                ' JS: do not exit For; keep going to add variable statuses
                'Exit For
            End If
        Next

        Me.NotifyCore(msg)

        Return bMissing

    End Function

    Private Sub NotifyCore(ByVal msg As cMessage)

        If msg Is Nothing Then Return

        Try
            'messages can be turned off be a user
            'to speed up the running of the model as in the case of the Monte Carlo which run the model multiple times
            'this puts the model into a 'silent' mode
            If Not m_bSuppressMsgs Then
                m_msgPub.SendMessage(msg)
            End If
        Catch ex As Exception
            cLog.Write(cStringUtils.Localize("cPSDModel.NotifyCore(...) Failed to post message {0}.", msg.ToString()))
        End Try

    End Sub

    Private Sub EstimateGrowthParameters()
        Dim sngTemp As Single
        Dim strTemp As String = Nothing
        Dim bIsFished As Boolean = False

        'A in LW
        For i As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.AinLW(i) < 0 Then Me.m_psd.AinLW(i) = 0.01
        Next

        'B in LW
        For i As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.BinLW(i) < 0 Then Me.m_psd.BinLW(i) = 3
        Next

        'Loo
        For i As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Loo(i) < 0 And Me.m_psd.Winf(i) > 0 And Me.m_psd.AinLW(i) > 0 And Me.m_psd.BinLW(i) > 0 Then
                Me.m_psd.Loo(i) = CSng(Math.Pow(10.0, (Math.Log10(Me.m_psd.Winf(i)) - Math.Log10(Me.m_psd.AinLW(i))) / Me.m_psd.BinLW(i)))
            End If
        Next

        'Winf
        For i As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Winf(i) < 0 And Me.m_psd.Loo(i) > 0 And Me.m_psd.AinLW(i) > 0 And Me.m_psd.BinLW(i) > 0 Then
                Me.m_psd.Winf(i) = CSng(Me.m_psd.AinLW(i) * Math.Pow(Me.m_psd.Loo(i), Me.m_psd.BinLW(i)))
            End If
        Next

        't0
        For i As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.t0(i) < -9998 And Me.m_psd.Loo(i) > 0 And m_Data.vbK(i) > 0 Then
                Me.m_psd.t0(i) = CSng(-Math.Exp(-0.3922 - 0.2752 * Math.Log10(Me.m_psd.Loo(i)) - 1.038 * Math.Log10(m_Data.vbK(i))))
            End If
        Next

        'Tcatch
        For i As Integer = 1 To m_Data.NumLiving
            'Determine if group is fished
            bIsFished = False
            For iFleet As Integer = 1 To m_Data.NumFleet
                If (m_Data.Landing(iFleet, i) + _
                    m_Data.Discard(iFleet, i)) > 0 Then
                    bIsFished = True
                    Exit For
                End If
            Next

            'Is user input?
            If Me.m_psd.TcatchInput(i) >= 0 Then
                'Yes user input
                'Use user input as it is
            Else
                'Not user input
                'Is group with catches?
                If bIsFished Then ' m_Data.fCatch(i) > 0 Then
                    'Yes group with catches
                    'Is stanza group?
                    If m_Data.StanzaGroup(i) Then
                        'Yes stanza group
                        For isp As Integer = 1 To m_stanza.Nsplit 'No. of split group
                            For ist As Integer = 1 To m_stanza.Nstanza(isp) ' No. of stanza in a split group
                                If m_stanza.EcopathCode(isp, ist) = i Then
                                    'Is first stanza with catches?
                                    ' WTF, string comparison!? This really needs to change
                                    If m_stanza.StanzaName(isp) <> strTemp Then
                                        'Yes first stanza with catches
                                        sngTemp = CSng(m_stanza.Age1(isp, ist) / cCore.N_MONTHS)
                                        strTemp = m_stanza.StanzaName(isp)
                                    Else
                                        'Not first stanza with catches
                                    End If
                                End If
                            Next
                        Next
                        Me.m_psd.Tcatch(i) = sngTemp
                    Else
                        'Not stanza group
                        Me.m_psd.Tcatch(i) = 0
                    End If
                Else
                    'Not group with catches
                    Me.m_psd.Tcatch(i) = 0
                End If
            End If
        Next

        'Tmax
        For i As Integer = 1 To m_Data.NumLiving
            'Is stanza group?
            If m_Data.StanzaGroup(i) Then
                'Yes
                For isp As Integer = 1 To m_stanza.Nsplit 'No. of split group
                    For ist As Integer = 1 To m_stanza.Nstanza(isp) ' No. of stanza in a split group
                        If m_stanza.EcopathCode(isp, ist) = i Then
                            Me.m_psd.Tmax(i) = CSng(m_stanza.Age2(isp, ist) / cCore.N_MONTHS)
                        End If
                    Next
                Next
            Else
                'No
                If m_Data.PBinput(i) > 0 Then
                    sngTemp = m_Data.PBinput(i)
                Else
                    sngTemp = m_Data.PB(i)
                End If
                If sngTemp = 0 And m_Data.QBinput(i) > 0 And m_Data.GEinput(i) > 0 Then
                    sngTemp = m_Data.QBinput(i) * m_Data.GEinput(i)
                End If
                If Me.m_psd.Tmax(i) < 0 And sngTemp > 0 Then Me.m_psd.Tmax(i) = CSng(Math.Exp((Math.Log(sngTemp) - 1.44) / -0.984))
            End If
        Next
    End Sub

    Private Function EstimatePSD() As Boolean
        Dim sTime As Single

        Dim WeightClass() As Single
        Dim Time() As Single
        Dim DeltaTime() As Single
        Dim Number() As Single
        Dim Biomass() As Single
        Dim StartWeightClassNum As Integer
        Dim ScaleFactor As Single

        Dim sSystemPSD(m_psd.NWeightClasses) As Single
        Dim iNWtClsSysPSDGTZero As Integer = 0
        Dim bSuccess As Boolean = False
        Dim msg As cMessage = Nothing

        ReDim Me.m_psd.PSD(m_Data.NumGroups, Me.m_psd.NWeightClasses)

        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then
                '(I) Estimate Weight, Number ad Biomass over time
                For iTimeStep As Integer = 1 To Me.m_psd.NAgeSteps
                    sTime = (iTimeStep - 1) * Me.m_psd.Tmax(iGroup) / (Me.m_psd.NAgeSteps - 1)

                    'Weight
                    If sTime > CalcStartTime(iGroup) Then
                        Me.m_psd.EcopathWeight(iGroup, iTimeStep) = CalcWeight(iGroup, sTime)
                    Else
                        Me.m_psd.EcopathWeight(iGroup, iTimeStep) = 0
                    End If

                    'Number and Lorenzen Mortality
                    If sTime > CalcStartTime(iGroup) Then
                        Me.m_psd.LorenzenMortality(iGroup, iTimeStep) = CalcMortality(iGroup, sTime)
                        Me.m_psd.EcopathNumber(iGroup, iTimeStep) = CSng(10000 * Math.Exp(-CalcMortality(iGroup, sTime) * sTime))
                    Else
                        Me.m_psd.LorenzenMortality(iGroup, iTimeStep) = 0
                        Me.m_psd.EcopathNumber(iGroup, iTimeStep) = 0
                    End If

                    'Biomass
                    If sTime > CalcStartTime(iGroup) Then
                        Me.m_psd.EcopathBiomass(iGroup, iTimeStep) = Me.m_psd.EcopathWeight(iGroup, iTimeStep) * Me.m_psd.EcopathNumber(iGroup, iTimeStep)
                    Else
                        Me.m_psd.EcopathBiomass(iGroup, iTimeStep) = 0
                    End If
                Next

                '(II) Estimate PSD as a function of weight class
                ReDim WeightClass(Me.m_psd.NWeightClasses)
                ReDim Time(Me.m_psd.NWeightClasses)
                ReDim DeltaTime(Me.m_psd.NWeightClasses)
                ReDim Number(Me.m_psd.NWeightClasses)
                ReDim Biomass(Me.m_psd.NWeightClasses)

                '(1) Find weight classes
                WeightClass(1) = Me.m_psd.FirstWeightClass
                For iWtClass As Integer = 2 To Me.m_psd.NWeightClasses
                    WeightClass(iWtClass) = WeightClass(iWtClass - 1) * 2
                Next

                '(2) Find times in weight classes
                't= ln(1-(Wt/Woo)^(1/b)) / (-K) + t0
                For iWtClass As Integer = 1 To Me.m_psd.NWeightClasses
                    If WeightClass(iWtClass) < Me.m_psd.Winf(iGroup) Then
                        Time(iWtClass) = CSng(Math.Log(1 - (WeightClass(iWtClass) / Me.m_psd.Winf(iGroup)) ^ _
                                         (1 / Me.m_psd.BinLW(iGroup))) / (-m_Data.vbK(iGroup)) + Me.m_psd.t0(iGroup))
                        If Time(iWtClass) < 0 Then Time(iWtClass) = 0
                        DeltaTime(iWtClass) = Time(iWtClass) - Time(iWtClass - 1)
                    Else
                        Time(iWtClass) = 0
                    End If
                Next

                '(3) Find survival(weight class)
                'Nt+1 = Nt * Exp(-Z * dT)
                '(a) Get start weight and start weight class
                For iWtClass As Integer = 0 To Me.m_psd.NWeightClasses - 1
                    If WeightClass(iWtClass + 1) > CalcStartWeight(iGroup) Then
                        If iWtClass = 0 Then
                            StartWeightClassNum = 1
                        Else
                            StartWeightClassNum = iWtClass
                            Number(StartWeightClassNum - 1) = 10000
                            Exit For
                        End If
                    End If
                Next
                '(b) Get survival
                For iWtClass As Integer = StartWeightClassNum To Me.m_psd.NWeightClasses
                    If Time(iWtClass) <= Me.m_psd.Tmax(iGroup) Then
                        If iWtClass = 0 Then
                            Number(iWtClass) = 10000
                        Else
                            Number(iWtClass) = CSng(Number(iWtClass - 1) * Math.Exp(-CalcMortality(iGroup, Time(iWtClass)) * DeltaTime(iWtClass)))
                        End If
                    ElseIf iWtClass = StartWeightClassNum Then
                        Number(iWtClass) = 10000
                    Else ' Done
                        Exit For
                    End If
                Next

                '(4) Find biomass(weight class)
                'Is group Winf smaller than StartWeight
                If WeightClass(1) > Me.m_psd.Winf(iGroup) Then
                    'Yes
                    Biomass(1) = m_Data.B(iGroup)
                Else
                    'No
                    ScaleFactor = 0
                    'The duration in each size group differs, spends more time in larger size group
                    For iWtClass As Integer = StartWeightClassNum To Me.m_psd.NWeightClasses
                        Biomass(iWtClass) = Number(iWtClass) * WeightClass(iWtClass) * DeltaTime(iWtClass)
                        If Biomass(iWtClass) < 0 Then
                            Biomass(iWtClass) = 0
                        Else
                            ScaleFactor = ScaleFactor + Biomass(iWtClass)
                        End If
                    Next
                End If
                'Now scale the biomass to the Ecopath value
                If ScaleFactor > 0 Then
                    For iWtClass As Integer = StartWeightClassNum To Me.m_psd.NWeightClasses
                        Biomass(iWtClass) = Biomass(iWtClass) * m_Data.B(iGroup) / ScaleFactor
                    Next
                End If

                '(5)Assign to group PSD
                For iWtClass As Integer = StartWeightClassNum To Me.m_psd.NWeightClasses
                    Me.m_psd.PSD(iGroup, iWtClass) = Biomass(iWtClass)
                    'm_Data.PSD(0, iWtClass) = m_Data.PSD(0, iWtClass) + Biomass(iWtClass)
                Next
            End If
        Next

        'Check if groups fall into at least two weight classes
        CalcSystemPSD(sSystemPSD)
        For i As Integer = 1 To m_psd.NWeightClasses
            If sSystemPSD(i) > 0.0 Then iNWtClsSysPSDGTZero = iNWtClsSysPSDGTZero + 1
        Next
        If iNWtClsSysPSDGTZero >= 2 Then
            bSuccess = True
        Else
            If (msg Is Nothing) Then
                msg = New cMessage(My.Resources.CoreMessages.PSD_ERROR_WEIGHTCLASSES, eMessageType.Any, eCoreComponentType.EcoPath, eMessageImportance.Warning)
                msg.Suppressable = False
            End If
        End If

        Me.NotifyCore(msg)

        Return bSuccess
    End Function

    Private Sub EstimatePSDMovAvg()
        Dim sSystemPSD(m_psd.NWeightClasses) As Single
        Dim sTempPSD(m_psd.NWeightClasses) As Single
        Dim iUpperLimit As Integer
        Dim iPoints As Integer
        Dim iFirstSelectedGrpNum As Integer

        CalcSystemPSD(sSystemPSD)

        For i As Integer = 1 To m_psd.NWeightClasses
            sTempPSD(i) = sSystemPSD(i)
            If sTempPSD(i) > 0 Then iUpperLimit = i
            sSystemPSD(i) = 0
        Next

        'if 3 make it 1; if 5 make it 2:
        iPoints = CInt((m_psd.NPtsMovAvg - 1) / 2)
        For i As Integer = iPoints + 1 To iUpperLimit - iPoints - 1 'halfway
            For j As Integer = -iPoints To iPoints     'fullway
                sSystemPSD(i) = sSystemPSD(i) + sTempPSD(i + j)
            Next
        Next

        For i As Integer = iPoints To m_psd.NWeightClasses - iPoints   'halfway
            sSystemPSD(i) = sSystemPSD(i) / m_psd.NPtsMovAvg
        Next

        'Determine the first selected group
        For iGroup As Integer = 1 To m_psd.NumLiving
            If m_psd.Include(iGroup) Then
                iFirstSelectedGrpNum = iGroup
                Exit For
            End If
        Next

        'Assign the system PSD to the first selected group
        For i As Integer = 1 To m_psd.NWeightClasses
            For j As Integer = 1 To m_Data.NumLiving
                m_psd.PSD(j, i) = 0
            Next
        Next
        For i As Integer = 1 To m_psd.NWeightClasses
            m_psd.PSD(iFirstSelectedGrpNum, i) = sSystemPSD(i)
        Next
    End Sub

    Private Sub EstimateSizeWeight()
        Dim AvgWeight() As Single
        Dim AvgNumber() As Single
        Dim Used() As Boolean
        Dim Max As Single
        Dim Biggest As Integer
        Dim Sum1 As Single = 0
        Dim Sum2 As Single = 0

        ReDim AvgWeight(m_Data.NumLiving)
        ReDim AvgNumber(m_Data.NumLiving)
        ReDim Used(m_Data.NumLiving)

        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then
                AvgWeight(iGroup) = CalcAvgWeight(iGroup)
                AvgNumber(iGroup) = CalcAvgNumber(iGroup)
            End If
        Next

        'For Biomass/(AvgWeight/AvgNumber)
        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then
                Max = -1
                For iGrp As Integer = 1 To m_Data.NumLiving
                    If Me.m_psd.Include(iGrp) Then
                        If AvgNumber(iGrp) > 0 Then
                            If m_Data.B(iGrp) / (AvgWeight(iGrp) / AvgNumber(iGrp)) > Max And Used(iGrp) = False Then
                                Max = m_Data.B(iGrp) / (AvgWeight(iGrp) / AvgNumber(iGrp))
                                Biggest = iGrp
                            End If
                        End If
                    End If
                Next
                If Max < 0 Then Max = 0
                Sum1 = Sum1 + Max
                m_psd.BiomassAvgSzWt(iGroup) = Max
                Used(Biggest) = True
            End If
        Next

        'Now for biomass
        ReDim Used(m_Data.NumLiving)
        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then
                Max = -1
                For iGrp As Integer = 1 To m_Data.NumLiving
                    If Me.m_psd.Include(iGrp) Then
                        If m_Data.B(iGrp) > Max And Used(iGrp) = False Then
                            Max = m_Data.B(iGrp)
                            Biggest = iGrp
                        End If
                    End If
                Next
                If Max <= 0 Then Max = 0
                Sum2 = Sum2 + Max
                m_psd.BiomassSzWt(iGroup) = Max
                Used(Biggest) = True
            End If
        Next

        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then 'might not work
                m_psd.BiomassAvgSzWt(iGroup) = m_psd.BiomassAvgSzWt(iGroup - 1) + m_psd.BiomassAvgSzWt(iGroup)
                m_psd.BiomassSzWt(iGroup) = m_psd.BiomassSzWt(iGroup - 1) + m_psd.BiomassSzWt(iGroup)
            End If
        Next

        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then 'might not work
                m_psd.BiomassAvgSzWt(iGroup) = 100 * m_psd.BiomassAvgSzWt(iGroup) / Sum1
                m_psd.BiomassSzWt(iGroup) = 100 * m_psd.BiomassSzWt(iGroup) / Sum2
            End If
        Next
    End Sub

    Private Function CalcStartTime(ByVal iGroup As Integer) As Single
        'Is stanza group?
        If m_Data.StanzaGroup(iGroup) Then
            'Yes
            For isp As Integer = 1 To m_stanza.Nsplit 'No. of split group
                For ist As Integer = 1 To m_stanza.Nstanza(isp) ' No. of stanza in a split group
                    If m_stanza.EcopathCode(isp, ist) = iGroup Then
                        If m_stanza.Age1(isp, ist) > 0 Then
                            Return CSng(m_stanza.Age1(isp, ist) / cCore.N_MONTHS)
                        Else
                            Return 0
                        End If
                    End If
                Next
            Next
        Else
            'No
            Return 0
        End If
    End Function

    Private Function CalcStartWeight(ByVal iGroup As Integer) As Single
        'Is stanza group?
        If m_Data.StanzaGroup(iGroup) Then
            'Yes
            For isp As Integer = 1 To m_stanza.Nsplit 'No. of split group
                For ist As Integer = 1 To m_stanza.Nstanza(isp) ' No. of stanza in a split group
                    If m_stanza.EcopathCode(isp, ist) = iGroup Then
                        If m_stanza.Age1(isp, ist) > 0 Then
                            Return CalcWeight(iGroup, CSng(m_stanza.Age1(isp, ist) / cCore.N_MONTHS))
                        Else
                            Return 0
                        End If
                    End If
                Next
            Next
        Else
            'No
            Return 0
        End If
    End Function

    Private Function CalcWeight(ByVal iGroup As Integer, ByVal sTime As Single) As Single
        'Wt = Woo (1-exp(-K(t-to))^b    where b is the exp of LW
        Return CSng(Me.m_psd.Winf(iGroup) * (1 - Math.Exp(-m_Data.vbK(iGroup) * _
                    (sTime - Me.m_psd.t0(iGroup))) ^ Me.m_psd.BinLW(iGroup)))
    End Function

    Private Function CalcMortality(ByVal iGroup As Integer, ByVal sTime As Single) As Single
        Dim Wt As Single
        Dim Mu As Single
        Dim Wb As Single
        Dim NatMortality As Single
        Dim FishMortality As Single

        Select Case Me.m_psd.MortalityType
            Case ePSDMortalityTypes.GroupZ
                If sTime < Me.m_psd.Tcatch(iGroup) Then
                    Return m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)
                Else
                    Return m_Data.PB(iGroup)
                End If
            Case ePSDMortalityTypes.Lorenzen
                'Calculate weight
                Wt = CalcWeight(iGroup, sTime)

                'Set Lorenzen parameters
                SetLorenzenParameters(Wb, Mu, Me.m_psd.ClimateType)

                'Calculate natural mortality
                NatMortality = CSng(Mu * Wt ^ Wb)

                'Calculate fishing mortality
                If sTime < Me.m_psd.Tcatch(iGroup) Then
                    FishMortality = 0
                Else
                    FishMortality = m_Data.fCatch(iGroup) / m_Data.B(iGroup)
                End If

                'Return total mortality
                Return NatMortality + FishMortality
        End Select
    End Function

    Private Sub SetLorenzenParameters(ByRef Wb As Single, ByRef Mu As Single, ByVal ClimateType As eClimateTypes)
        '0-30 o     tropical	-0.21	3.08
        '30-60 o    temperate	-0.309	3.13
        '60-90 o    polar	    -0.292	1.69
        Select Case ClimateType
            Case eClimateTypes.Tropical
                Wb = -0.21
                Mu = 3.08
            Case eClimateTypes.Temperate
                Wb = -0.309
                Mu = 3.13
            Case eClimateTypes.Polar
                Wb = -0.292
                Mu = 1.69
        End Select
    End Sub

    Private Function CalcAvgWeight(ByVal iGroup As Integer) As Single
        Dim Omega(3) As Integer
        Dim Recr(m_Data.NumLiving) As Single
        Dim Trec(m_Data.NumLiving) As Single
        Dim P1 As Single
        Dim P2 As Single
        Dim P3 As Single
        Dim P4 As Single
        Dim P5 As Single
        Dim P6 As Single
        Dim sum As Single = 0

        Omega(0) = 1
        Omega(1) = -3
        Omega(2) = 3
        Omega(3) = -1

        Recr(iGroup) = 1
        Trec(iGroup) = 0

        If Recr(iGroup) <= 0 Then Recr(iGroup) = 1
        For n As Integer = 0 To 3
            P1 = CSng(Omega(n) * Math.Exp(-n * m_Data.vbK(iGroup) * (Trec(iGroup) - m_psd.t0(iGroup))))
            P2 = CSng(1 - Math.Exp(-((m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)) + n * m_Data.vbK(iGroup)) * (m_psd.Tcatch(iGroup) - Trec(iGroup))))
            P3 = (m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)) + n * m_Data.vbK(iGroup)
            P4 = CSng(Math.Exp(-((m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)) + n * m_Data.vbK(iGroup)) * (m_psd.Tcatch(iGroup) - Trec(iGroup))))
            P5 = CSng(1 - Math.Exp(-(m_Data.PB(iGroup) + n * m_Data.vbK(iGroup)) * (m_psd.Tmax(iGroup) - m_psd.Tcatch(iGroup))))
            P6 = m_Data.PB(iGroup) + n * m_Data.vbK(iGroup)
            sum = sum + P1 * (P2 / P3 + P4 * P5 / P6)
        Next
        Return m_psd.Winf(iGroup) * Recr(iGroup) * sum
    End Function

    Private Function CalcAvgNumber(ByVal iGroup As Integer) As Single
        Dim Recr(m_Data.NumLiving) As Single
        Dim Trec(m_Data.NumLiving) As Single
        Dim P1 As Single
        Dim P2 As Single
        Dim P3 As Single

        Recr(iGroup) = 1
        Trec(iGroup) = 0

        If Recr(iGroup) <= 0 Then Recr(iGroup) = 1
        P1 = CSng(1 - Math.Exp(-(m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)) * (m_psd.Tcatch(iGroup) - Trec(iGroup))))
        P2 = CSng(Math.Exp(-(m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)) * (m_psd.Tcatch(iGroup) - Trec(iGroup)) * _
                           (1 - Math.Exp(-m_Data.PB(iGroup) * (m_psd.Tmax(iGroup) - m_psd.Tcatch(iGroup))))))
        P3 = m_Data.PB(iGroup)
        Return Recr(iGroup) * (P1 / (m_Data.PB(iGroup) - m_Data.fCatch(iGroup) / m_Data.B(iGroup)) + P2 / P3)
    End Function

    Private Sub CalcSystemPSD(ByVal sSystemPSD() As Single)
        For iGroup As Integer = 1 To m_Data.NumLiving
            If Me.m_psd.Include(iGroup) Then
                For iWtClass As Integer = 1 To m_psd.NWeightClasses
                    sSystemPSD(iWtClass) = sSystemPSD(iWtClass) + m_psd.PSD(iGroup, iWtClass)
                Next
            End If
        Next
    End Sub

#End Region ' Helper methods

End Class
