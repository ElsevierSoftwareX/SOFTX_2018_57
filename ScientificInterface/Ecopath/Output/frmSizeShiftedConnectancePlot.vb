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
Option Explicit On

Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ZedGraph

#End Region

Namespace Ecopath.Output

    Public Class frmSizeShiftedConnectancePlot

#Region " Private vars "

        Private Enum ePlotType As Integer
            SizeShifted
            Connectance
        End Enum

        Private m_zgh As cZedGraphHelper = Nothing
        Private m_plottype As ePlotType = ePlotType.SizeShifted

#End Region ' Private vars

#Region " Form overloads "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_zgh.Attach(Me.UIContext, Me.m_graph)
            Me.m_zgh.ShowPointValue = True

            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If (cmd IsNot Nothing) Then cmd.AddControl(Me.m_btnShowHideGroups)

            Me.CoreExecutionState = eCoreExecutionState.EcopathCompleted

            Me.UpdateControls()
            Me.UpdatePlot()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_zgh.Detach()

            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If (cmd IsNot Nothing) Then cmd.RemoveControl(Me.m_btnShowHideGroups)

            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As ScientificInterfaceShared.Style.cStyleGuide.eChangeType)
            MyBase.OnStyleGuideChanged(ct)
            Me.UpdatePlot()
        End Sub

        Protected Overrides Sub UpdateControls()
            MyBase.UpdateControls()
        End Sub

#End Region ' Form overloads

#Region " Internals "

        Private Sub UpdatePlot()

            Dim strTitle As String = ""
            Dim strXAxis As String = ""
            Dim strYAxis As String = ""
            Dim pane As GraphPane = Me.m_zgh.ConfigurePane(strTitle, strXAxis, strYAxis, False)

        End Sub

#End Region ' Internals

    End Class

End Namespace

#Region " EwE5 code "
#If 0 Then

Option Explicit
Dim CutOff As Single
Dim NoPoints As Integer
Dim Xv() As Single
Dim Yv() As Single
Dim Xa() As Single
Dim Ya() As Single
Dim Moved() As Boolean
Dim Slope As Single
Dim Intercept As Single
Dim MoveGroup As Integer
Dim MinLong As Variant
Dim MinDC As Variant
Dim MinTL As Variant
Dim max As Variant
Dim min As Variant
Dim reg As Single
Dim Host() As Single
Dim localTTLX() As Single
Dim localPB() As Single
Dim LabelPos() As Single
Dim LabelAxis() As Single
Dim SelGrp As Integer
Dim ShowGrp() As Integer
Dim TLfish() As Single
Dim BoxL() As Single

Public Sub ArrangedAlongAxis()
On Local Error GoTo exitSub
Dim i As Integer
Dim j As Integer
On Local Error Resume Next
Dim Cnt As Integer
Dim maxTL As Integer
Dim X As Single
Dim Temp() As Variant
Dim Predat As Boolean
Dim Wi As Single
    MinDC = if(txtMin <> "", txtMin, 0.001)
    'Find the maximum trophic level:
    max = -1000
    For i = 1 To NumLiving
        'If ShowGrp(i) Then
        If TTLX(i) > max Then max = TTLX(i)
        'End If
    Next
    maxTL = CInt(max - 0.5) + 1
    'find out how many there are by half TL step
    Cnt = 2 * maxTL
    ReDim Temp(Cnt)
    For i = 1 To NumGroups
        If ShowGrp(i) Then
            Temp(CInt(2 * TTLX(i))) = Temp(CInt(2 * TTLX(i))) + 1
        End If
    Next
    For i = 1 To NumGear
        Temp(CInt(2 * TLfish(i))) = Temp(CInt(2 * TLfish(i))) + 1
    Next
    max = -1
    For i = 1 To Cnt
        If Temp(i) > max Then max = Temp(i)
    Next
    max = max + 1
    If max < 5 Then max = 5
    'Now scale the x-axis after max, and the y-axis after maxTL:
    picbox.AutoRedraw = True
    picbox.Cls
    If optGrp(0) Then
        picbox.Scale (-0.1 * max, 1.15 * maxTL)-(1.1 * max, 0.5)
    End If

    If chkFlow Then
        If optGrp(0) Then 'show all
            If SelGrp <= NumLiving Then
                For i = 1 To NumLiving
                    For j = 1 To NumGroups
                        If ShowGrp(i) And ShowGrp(j) And DC(i, j) > MinDC And DC(i, j) > 0 Then
                            If optGrp(1) Then
                                If i = SelGrp Then  'predator
                                    Predat = True
                                    picbox.ForeColor = RGB(255, 0, 0)
                                Else    'prey
                                    Predat = False
                                    picbox.ForeColor = RGB(0, 0, 255)
                                End If
                            ElseIf chkColor Then
                                picbox.ForeColor = PoolColor(i)
                            End If
                            GoSub DrawLine
                        End If
                    Next
                Next
            End If
            If chkFish Then
                For i = 1 To NumGear
                    For j = 1 To NumGroups
                        If ShowGrp(j) And Landing(i, j) + Discard(i, j) > 0 Then
                            If optGrp(1) Then
                                picbox.ForeColor = RGB(0, 255, 0)
                            'ElseIf chkColor Then
                            '    picbox.ForeColor = poolcolor(i)
                            End If
                            Wi = (Landing(i, j) + Discard(i, j)) / Catch(j) * 30
                            If chkHost Then picbox.DrawWidth = if(Wi > 1, Wi, 1)
                            picbox.Line (Xa(i + NumGroups), Ya(i + NumGroups))-(Xa(j), Ya(j))
                        End If
                    Next
                Next
            End If
        Else    'show selected group only:
            j = SelGrp
            If j <= NumGroups Then
                For i = 1 To NumGroups
                    If DC(i, j) > 0 Then   'pred
                        Predat = True
                        picbox.ForeColor = RGB(255, 0, 0)
                        GoSub DrawLine
                    End If
                    If DC(j, i) > 0 Then   'a prey -- used mixed color if 1'order cycle
                        Predat = False
                        picbox.ForeColor = if(DC(i, j) > 0 And i <= NumLiving, RGB(255, 0, 255), RGB(0, 0, 255))
                        GoSub DrawLine
                    End If
                Next
            End If
            If chkFish Then
                For i = 1 To NumGear
                    For j = 1 To NumGroups
                        If ShowGrp(j) And Landing(i, j) + Discard(i, j) > 0 And (i = SelGrp - NumGroups Or j = SelGrp) Then
                            If optGrp(1) Then
                                picbox.ForeColor = RGB(0, 255, 0)
                            ElseIf chkColor Then
                                picbox.ForeColor = PoolColor(i)
                            End If
                            Wi = (Landing(i, j) + Discard(i, j)) / Catch(j) * 30
                            If chkHost Then picbox.DrawWidth = if(Wi > 1, Wi, 1)
                            picbox.Line (Xa(i + NumGroups), Ya(i + NumGroups))-(Xa(j), Ya(j))
                       End If
                    Next
                Next
            End If
        End If
    End If
    picbox.DrawWidth = 1
    Cnt = 0
    picbox.ForeColor = QBColor(0)
    For i = 1 To NumGroups
        If ShowGrp(i) Then
            Cnt = Cnt + 1
            picbox.CurrentX = LabelAxis(i, 0) - 0.1 'Xv(i) - 0.1
            picbox.CurrentY = LabelAxis(i, 1) + 0.15 'Yv(i) + 0.15
            If chkNames Then
                If chkColor Then picbox.ForeColor = PoolColor(i)
                picbox.Print Specie(i)  'Mid(Specie(i), 1, 15)
                picbox.CurrentX = Xa(i) - 0.1
                picbox.CurrentY = Ya(i) + 0.15
                'picbox.Line (Xa(i) - 2 * BoxL(i), Ya(i) + 1 * BoxL(i))-(Xa(i) + 2 * BoxL(i), Ya(i) - 1 * BoxL(i)), QBColor(0), B
                picbox.Circle (Xa(i), Ya(i)), 0.01, if(chkColor, PoolColor(i), 0)
                picbox.Circle (Xa(i), Ya(i)), 0.02, if(chkColor, PoolColor(i), 0)
                picbox.Circle (Xa(i), Ya(i)), 0.03, if(chkColor, PoolColor(i), 0)
            Else
                picbox.Print CStr(i)
            End If
            picbox.ForeColor = 0
        End If
    Next
    If optGrp(1) Then
        For i = 1 To NumGear
            If SelGrp <= NumLiving Then
                If chkFish And Landing(i, SelGrp) + Discard(i, SelGrp) > 0 Then
                    Cnt = Cnt + 1
                    picbox.CurrentX = LabelAxis(NumGroups + i, 0) - 0.1 'Xv(i) - 0.1
                    picbox.CurrentY = LabelAxis(NumGroups + i, 1) + 0.15 'Yv(i) + 0.15
                    If chkNames Then
                        'If chkColor Then picbox.ForeColor = QBColor(0)  'poolcolor(i)
                        picbox.Print GearName(i)  'Mid(Specie(i), 1, 15)
                        picbox.CurrentX = Xa(NumGroups + i) - 0.1
                        picbox.CurrentY = Ya(NumGroups + i) + 0.15
                        'picbox.ForeColor = QBColor(0)
                        'picbox.Line (Xa(NumGroups + i) - 2 * BoxL(0), Ya(NumGroups + i + BoxL(0)))-(Xa(NumGroups + i) + 2 * BoxL(0), Ya(NumGroups + i) - BoxL(0)), QBColor(0), B
                        picbox.Circle (Xa(NumGroups + i), Ya(NumGroups + i)), 0.01, if(chkColor, PoolColor(i), 0)
                        picbox.Circle (Xa(NumGroups + i), Ya(NumGroups + i)), 0.02, if(chkColor, PoolColor(i), 0)
                        picbox.Circle (Xa(NumGroups + i), Ya(NumGroups + i)), 0.03, if(chkColor, PoolColor(i), 0)
                    Else
                        picbox.Print "F " + CStr(i)
                    End If
                    picbox.ForeColor = 0
                End If
            ElseIf SelGrp > NumGroups And chkFish And chkNames Then
                picbox.CurrentX = LabelAxis(NumGroups + i, 0) - 0.1 'Xv(i) - 0.1
                picbox.CurrentY = LabelAxis(NumGroups + i, 1) + 0.15 'Yv(i) + 0.15
                picbox.Print GearName(i)  'Mid(Specie(i), 1, 15)
                picbox.CurrentX = Xa(NumGroups + i) - 0.1
                picbox.CurrentY = Ya(NumGroups + i) + 0.15
                'picbox.ForeColor = QBColor(0)
                'picbox.Line (Xa(NumGroups + i) - 2 * BoxL(0), Ya(NumGroups + i + BoxL(0)))-(Xa(NumGroups + i) + 2 * BoxL(0), Ya(NumGroups + i) - BoxL(0)), QBColor(0), B
                picbox.Circle (Xa(NumGroups + i), Ya(NumGroups + i)), 0.01, if(chkColor, PoolColor(i), 0)
                picbox.Circle (Xa(NumGroups + i), Ya(NumGroups + i)), 0.02, if(chkColor, PoolColor(i), 0)
                picbox.Circle (Xa(NumGroups + i), Ya(NumGroups + i)), 0.03, if(chkColor, PoolColor(i), 0)
            End If
        Next
    End If
    If optPlot(0) Then
        For i = 1 To maxTL
            picbox.Line (1, i)-(1 + 0.01 * maxTL, i)
            picbox.CurrentX = 1 - 0.2
            picbox.CurrentY = i + 0.1
            picbox.Print CStr(i)
        Next
        picbox.Line (1, 1)-(1, maxTL)
        picbox.CurrentX = -0.2
        picbox.CurrentY = 1.12 * maxTL
        picbox.Print "Trophic level"
    End If
    picbox.FontName = cmbFonts.Text
    picbox.FontBold = chkBold.value ' True
    picbox.FontItalic = chkItalic.value
    If txtFontSize <> "" Then
        picbox.FontSize = CInt(txtFontSize)
    Else
        picbox.FontSize = 10
    End If
exitSub:
Exit Sub

DrawLine:
    If chkHost.value = Checked Then
        If Predat Then
            picbox.DrawWidth = if(Host(j, i) * 50 > 1, Host(j, i) * 50, 1)
        ElseIf optGrp(1) And Predat = False Then
            picbox.DrawWidth = if(DC(j, i) * 50 > 1, DC(j, i) * 50, 1)
        End If
    Else
        picbox.DrawWidth = 1
    End If
    picbox.Line (Xa(i), Ya(i))-(Xa(j), Ya(j))

Return
End Sub

Private Sub CalculateArrangedOnAxis()
Dim Cnt As Integer
Dim i As Integer
Dim maxTL As Integer
Dim Temp() As Variant
    'Find the maximum trophic level:
    max = -1000
    For i = 1 To NumGroups
        'If ShowGrp(i) Then
            If TTLX(i) > max Then max = TTLX(i)
        'End If
    Next
    maxTL = CInt(max - 0.5) + 1
    'find out how many there are by half TL step
    Cnt = 2 * maxTL
    ReDim Temp(Cnt)
    For i = 1 To NumGroups
        If ShowGrp(i) Then
            Temp(CInt(2 * TTLX(i))) = Temp(CInt(2 * TTLX(i))) + 1
        End If
    Next
    For i = 1 To NumGear
        Temp(CInt(2 * TLfish(i))) = Temp(CInt(2 * TLfish(i))) + 1
    Next
    max = -1
    For i = 1 To Cnt
        If Temp(i) > max Then max = Temp(i)
    Next
    max = max + 1

    ReDim Temp(Cnt)
    For i = 1 To NumGroups
        'If ShowGrp(i) Then
            Temp(CInt(2 * TTLX(i))) = Temp(CInt(2 * TTLX(i))) + 1
            Xa(i) = 1 + Temp(CInt(2 * TTLX(i)))
            Ya(i) = TTLX(i)
            LabelAxis(i, 0) = Xa(i)
            LabelAxis(i, 1) = Ya(i)
        'End If
    Next
    For i = 1 To NumGear
        'If ShowGrp(i) Then
            Temp(CInt(2 * TLfish(i))) = Temp(CInt(2 * TLfish(i))) + 1
            Xa(NumGroups + i) = 1 + Temp(CInt(2 * TLfish(i)))
            Ya(NumGroups + i) = TLfish(i)
            LabelAxis(NumGroups + i, 0) = Xa(NumGroups + i)
            LabelAxis(NumGroups + i, 1) = Ya(NumGroups + i)
        'End If
    Next
End Sub

Public Sub DimGraph()
    Select Case optPlot(0).value
    Case False
        SizeShifted
    Case True
        ArrangedAlongAxis
    End Select
End Sub

Private Sub chkFish_Click()
    FillCmbGrp chkFish.value
    DimGraph
End Sub

Private Sub chkHost_Click()
    DimGraph
End Sub


Private Sub chkBold_Click()
    DimGraph
End Sub

Private Sub chkColor_Click()
    DimGraph
End Sub

Private Sub chkFlow_Click()
    DimGraph
End Sub

Private Sub chkItalic_Click()
    DimGraph
End Sub

Private Sub chkNames_Click()
    DimGraph
End Sub

Private Sub chkRegres_Click()
    DimGraph
End Sub

Private Sub cmbFonts_Change()
    DimGraph
End Sub

Private Sub cmbGrp_Click()
    SelGrp = cmbGrp.ListIndex + 1
    optGrp(1).value = True
    optGrp_Click 1
End Sub

Private Sub cmdpic_Click(Index As Integer)
Dim Answer As Variant
    Select Case Index
    Case 0  'OK
        Unload Me
    Case 1  ' Save as BMP
        SaveGraph picbox.Image
    End Select
End Sub

Private Sub Form_Activate()
Dim i As Integer
    For i = 1 To NumLiving
        If ShowGrp(i) Then
            Xv(i) = Log(1 / localPB(i))
            LabelPos(i, 0) = Xv(i)
            Yv(i) = localTTLX(i)
            LabelPos(i, 1) = Yv(i)
        End If
    Next
    For i = NumGroups + 1 To NumGroups + NumGear
        LabelPos(i, 0) = Xv(i)
        LabelPos(i, 1) = Yv(i)
        Yv(i) = localTTLX(i)
    Next
End Sub

Private Sub Form_Load()
Dim i As Integer
Dim j As Integer
On Local Error Resume Next
Dim SumHost() As Single
    ReDim SumHost(NumGroups)
    ReDim localTTLX(NumGroups + NumGear)
    ReDim localPB(NumLiving)
    ReDim LabelPos(NumGroups + NumGear, 1)
    ReDim LabelAxis(NumGroups + NumGear, 1)
    ReDim ShowGrp(NumGroups + NumGear)
    ReDim TLfish(NumGear)
    ReDim BoxL(NumGroups)
    GetBoxLength
    CalcTLfish
    For i = 1 To NumLiving
        localTTLX(i) = TTLX(i)
        localPB(i) = PB(i)
    Next
    For i = 1 To NumGear
        localTTLX(i + NumGroups) = TLfish(i)
    Next
    For i = 1 To NumGroups
        If GrpsToShow(i) Then ShowGrp(i) = True
    Next
    MinLong = -8
    MinDC = 0.001
    Me.Tag = "frmsizes"
    Me.Caption = ModelTitle + ": Size-shifted connectance plot"
    Me.HelpContextID = Niche_overlap
    ReDim Xv(NumGroups + NumGear)
    ReDim Yv(NumGroups + NumGear)
    ReDim Xa(NumGroups + NumGear)
    ReDim Ya(NumGroups + NumGear)
    Form_Activate
    SelGrp = 1
    FillCmbGrp chkFish.value
    optGrp(0) = True
    optGrp_Click 0
    With cmbFonts
        For i = 0 To Printer.FontCount - 1   ' Determine number of fonts.
            .AddItem Printer.Fonts(i)         ' Put each font into list box.
        Next
        .Text = "Arial"
    End With
    Printer.FontName = "Arial"
    MoveGroup = 0

    ReDim Host(NumGroups, NumGroups)
    For i = 1 To NumGroups
        SumHost(i) = 0
        For j = 1 To NumLiving
            If B(j) > 0 And QB(j) > 0 And DC(j, i) > 0 Then
                Host(i, j) = B(j) * QB(j) * DC(j, i)
                SumHost(i) = SumHost(i) + Host(i, j)
            Else
                Host(i, j) = 0
            End If
        Next j
    Next i       'Host(ij) is amount eaten of group i by predator j
    'Now rescale host
    For i = 1 To NumGroups
        If SumHost(i) > 0 Then
            For j = 1 To NumLiving
                Host(i, j) = Host(i, j) / SumHost(i)
            Next j
        End If
    Next i

    CalculateArrangedOnAxis
    DimGraph
End Sub

Private Sub Form_Resize()
Dim Top As Single
Dim i As Integer
On Local Error GoTo exitSub
    If ScaleWidth > 1000 Then
        picbox.Width = ScaleWidth - 2000
        picbox.Height = ScaleHeight - 240
        'top = ScaleHeight - 500
        'cmdpic(1).top = top
        'chkFlow.top = top
        'chkItalic.top = top
        'cmbFonts.top = top
        'chkNames.top = top
        'chkBold.top = top
        'txtFontSize.top = top
        'chkRegres.top = top
        'txtMin.top = top
        'Label1.top = top
        'chkColor.top = top
        DimGraph
    End If
    picbox.ToolTipText = "Click and drag group labels"
exitSub:
End Sub

Private Sub MakeRegression()
Dim X As Integer
Dim j As Integer
Dim sumx2 As Single
Dim sumy2 As Single
Dim sumxy As Single
Dim sumx As Single
Dim sumy As Single
Dim sx2 As Single
Dim sy2 As Single
Dim sxy As Single
Dim Cnt As Integer
On Local Error Resume Next
    Cnt = 0
    reg = 0
    For j = 1 To NumLiving
        If ShowGrp(j) And Xv(j) > min Then
            Cnt = Cnt + 1
            sumx = sumx + Xv(j) 'WeightClass(x)
            sumx2 = sumx2 + Xv(j) ^ 2 'WeightClass(x) ^ 2
            sumy = sumy + Yv(j)
            sumy2 = sumy2 + Yv(j) ^ 2
            sumxy = sumxy + Xv(j) * Yv(j) ' WeightClass(x)
        End If
    Next
    If Cnt = 0 Then Exit Sub
    'Slope
    If sumx2 <> sumx ^ 2 / Cnt Then
        Slope = (sumxy - sumy * sumx / Cnt) / (sumx2 - sumx ^ 2 / Cnt)
    End If
    'intercept
    Intercept = (sumy - Slope * sumx) / Cnt
    If Cnt > 1 Then
        reg = (Cnt * sumxy - sumx * sumy) ^ 2 / (Cnt * sumx2 - sumx ^ 2) / (Cnt * sumy2 - sumy ^ 2)
    End If
End Sub

Private Sub optGrp_Click(Index As Integer)
Dim i As Integer
Dim j As Integer
    ReDim ShowGrp(NumGroups + NumGear)
    Select Case Index
    Case 0   'show all
        For i = 1 To NumGroups
            If GrpsToShow(i) Then ShowGrp(i) = True
        Next
        If chkFish Then
            For i = 1 To NumGear: ShowGrp(i) = True: Next
        End If
    Case 1  'show active group
        ShowGrp(SelGrp) = True
        For i = 1 To NumGroups
            If SelGrp <= NumGroups Then
                If (DC(i, SelGrp) > 0 And i <= NumLiving) Then ShowGrp(i) = True
            End If
            If SelGrp <= NumLiving Then
                 If DC(SelGrp, i) > 0 Then ShowGrp(i) = True
            End If
        Next
        If chkFish Then
            For i = 1 To NumGear
                If SelGrp <= NumLiving Then
                    If Landing(i, SelGrp) + Discard(i, SelGrp) > 0 Then
                        ShowGrp(NumGroups + i) = True
                    End If
                End If
                For j = 1 To NumGroups
                    If Landing(i, j) + Discard(i, j) > 0 And SelGrp > NumGroups Then
                        ShowGrp(j) = True
                    End If
                Next
            Next
        End If
    End Select
    DimGraph
End Sub

Private Sub optPlot_Click(Index As Integer)
    optGrp(0).value = True
    Select Case optPlot(0).value
    Case True
        chkRegres.Enabled = False
        chkHost.Enabled = True
        chkFish.Enabled = True
        txtMin.Text = MinDC
        Label1.Caption = "Min DC"
    Case False
        chkRegres.Enabled = True
        chkHost.Enabled = False
        chkFish.Enabled = False
        txtMin.Text = MinLong
        Label1.Caption = "Min value"
    End Select
    DimGraph

End Sub

Private Sub picbox_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
Dim i As Integer
    'Find the group that is being moved
    MoveGroup = 0
    Select Case optPlot(0).value
    Case False
        For i = 1 To NumLiving
            If ShowGrp(i) Then
                If Abs(LabelPos(i, 1) - Y) <= 0.1 And Abs(LabelPos(i, 0) - X) < 0.1 Then MoveGroup = i: Exit Sub
            End If
        Next
    Case True
        For i = 1 To NumGroups
            If ShowGrp(i) Then
                If Abs(LabelAxis(i, 1) - Y) <= 0.1 And Abs(LabelAxis(i, 0) - X) < 0.1 Then MoveGroup = i: Exit Sub
            End If
        Next
        For i = NumGroups + 1 To NumGroups + NumGear
            If Abs(LabelAxis(i, 1) - Y) <= 0.1 And Abs(LabelAxis(i, 0) - X) < 0.1 Then MoveGroup = i: Exit Sub
        Next
    End Select

End Sub

Private Sub picbox_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
    If MoveGroup > 0 Then
        If optPlot(1).value Then
            lblMoveGroup.Top = Y + 0.1
        Else
            lblMoveGroup.Top = lblMoveGroup.Top
        End If
        lblMoveGroup.Left = X - 0.1
        If MoveGroup <= NumGroups Then
            lblMoveGroup.Caption = if(chkNames, Mid(Specie(MoveGroup), 1, 15), CStr(MoveGroup))
            frmMdiEcopath4.ShowGroupName Specie(MoveGroup)
        Else
            lblMoveGroup.Caption = if(chkNames, Mid(GearName(MoveGroup - NumGroups), 1, 15), "F " + CStr(MoveGroup - NumGroups))
        End If
        lblMoveGroup.Visible = True
    End If
End Sub

Private Sub picbox_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
    If MoveGroup > 0 Then
        Select Case optPlot(1).value
        Case True
            lblMoveGroup.Visible = False
            LabelPos(MoveGroup, 0) = X
            LabelPos(MoveGroup, 1) = Y
        Case False
            lblMoveGroup.Visible = False
            LabelAxis(MoveGroup, 0) = X
            LabelAxis(MoveGroup, 1) = Y
            Xa(MoveGroup) = X
            If optPlot(1).value Then Ya(MoveGroup) = Y
        End Select
        DimGraph
        MoveGroup = 0
        frmMdiEcopath4.ShowGroupName ""
    End If
End Sub

Public Sub SizeShifted()
On Local Error GoTo exitSub
Dim i As Integer
Dim j As Integer
On Local Error Resume Next
Dim Cnt As Integer
Dim maxTL As Integer
Dim X As Single
    'picbox.FontBold = False
    'picbox.FontItalic = False
    min = 1000
    max = -1000
    For i = 1 To NumLiving
        If ShowGrp(i) Then
            If Log(1 / localPB(i)) > max Then max = Log(1 / localPB(i))
            If Log(1 / localPB(i)) < min Then min = Log(1 / localPB(i))
        End If
    Next
    min = 2 * min
    max = 2 * max
    min = CInt(min - 0.5) / 2 - 0.5

    MinLong = if(txtMin <> "", txtMin, -6)
    min = if(min < CSng(MinLong), CSng(MinLong), min)
    max = CInt(max + 0.5) / 2 + 0.5

    MinTL = 10
    max = -1000
    For i = 1 To NumLiving
        If ShowGrp(i) And Log(1 / localPB(i)) > min Then
            If localTTLX(i) > max Then max = localTTLX(i)
            If localTTLX(i) < MinTL Then MinTL = localTTLX(i)
        End If
    Next
    maxTL = CInt(max - 0.5) + 1
    MinTL = CInt(MinTL - 0.49) '- 1
    picbox.AutoRedraw = True
    picbox.Cls
    picbox.Scale (if(min < -1, 1.13 * min, min - 0.5), 1.15 * maxTL)-(1.1 * max, MinTL - 1)
    If chkFlow Then
        For i = 1 To NumLiving
            For j = 1 To NumLiving
                If ShowGrp(i) And ShowGrp(j) And DC(i, j) > 0 Then
                    If Xv(i) > min And Xv(j) > min Then
                        If chkColor Then picbox.ForeColor = PoolColor(i)
                        If chkHost.value = Checked Then
                            picbox.DrawWidth = Host(j, i) * 5
                        Else
                            picbox.DrawWidth = 1
                        End If
                        picbox.Line (Xv(i), Yv(i))-(Xv(j), Yv(j))
                    End If
                End If
            Next
        Next
    End If
    picbox.DrawWidth = 1
    picbox.ForeColor = QBColor(0)
    picbox.Line (min, MinTL)-(max, MinTL)
    picbox.Line (min, MinTL)-(min, maxTL)
    Cnt = 0
    For i = 1 To NumLiving
        If ShowGrp(i) And Xv(i) > min And Yv(i) >= MinTL Then
            Cnt = Cnt + 1
            picbox.CurrentX = LabelPos(i, 0) - 0.1 'Xv(i) - 0.1
            picbox.CurrentY = LabelPos(i, 1) + 0.15 'Yv(i) + 0.15
            If chkNames Then
                If chkColor Then picbox.ForeColor = PoolColor(i)
                picbox.Print Specie(i)  'Mid(Specie(i), 1, 15)
                picbox.CurrentX = Xv(i) - 0.1
                picbox.CurrentY = Yv(i) + 0.15
                picbox.Circle (Xv(i), Yv(i)), 0.01, if(chkColor, PoolColor(i), 0)
                picbox.Circle (Xv(i), Yv(i)), 0.02, if(chkColor, PoolColor(i), 0)
                picbox.Circle (Xv(i), Yv(i)), 0.03, if(chkColor, PoolColor(i), 0)
            Else
                picbox.Print CStr(i)
            End If
            picbox.ForeColor = 0
        End If
    Next

    For X = min To max Step if(max - min > 8, 1, 0.5)
        picbox.Line (X, MinTL)-(X, 0.008 * (max - min) + MinTL)
        picbox.CurrentY = MinTL '0.002 * (max - min)
        picbox.CurrentX = X - 0.1 '3
        picbox.Print Format(X, "0.0")
    Next
    For i = MinTL To maxTL
        picbox.Line (min, i)-(min + 0.01 * maxTL, i)
        picbox.CurrentX = min - 0.2
        picbox.CurrentY = i + 0.1
        picbox.Print CStr(i)
    Next
    picbox.CurrentX = 1.07 * min
    picbox.CurrentY = 1.12 * maxTL
    picbox.Print "Trophic level"
    picbox.CurrentX = 0.33 * (max - min) + min
    picbox.CurrentY = MinTL - 0.4
    picbox.Print "Longevity [biomass/production; log year]"
    If cmbFonts.Text <> "" Then picbox.FontName = cmbFonts.Text
    picbox.FontBold = chkBold.value ' True
    picbox.FontItalic = chkItalic.value
    If txtFontSize <> "" Then
        picbox.FontSize = CInt(txtFontSize)
    Else
        picbox.FontSize = 10
    End If
    If chkRegres Then
        MakeRegression
        picbox.CurrentX = max - 2
        picbox.CurrentY = MinTL + 1 'maxTL - 0.5
        picbox.ForeColor = QBColor(1)
        picbox.Print "Slope = " + Format(Slope, GenNum)
        picbox.CurrentX = max - 2
        picbox.Print "Intercept = " + Format(Intercept, GenNum)
        picbox.CurrentX = max - 2
        picbox.Print "r^2 = " + Format(reg, GenNum)
        picbox.CurrentX = max - 2
        picbox.Print "N = " + Format(Cnt, "0")

        picbox.Line (min + 0.2, Slope * (min + 0.2) + Intercept)-(max - 0.2, Slope * (max - 0.2) + Intercept), QBColor(1)

        picbox.ForeColor = QBColor(0)
    End If
exitSub:
End Sub

Private Sub txtFontSize_Change()
    DimGraph
End Sub

Private Sub txtMin_Change()
    Select Case optPlot(1).value
    Case True
        DimGraph
    End Select
End Sub

Public Sub FillCmbGrp(Fish As Boolean)
Dim i As Integer
    cmbGrp.Clear
    For i = 1 To NumGroups
        cmbGrp.AddItem Specie(i)
    Next
    If Fish Then
        For i = 1 To NumGear
            cmbGrp.AddItem GearName(i)
        Next
    Else
        If SelGrp > NumGroups Then SelGrp = 1
    End If
    cmbGrp.ListIndex = SelGrp - 1
End Sub

Private Sub CalcTLfish()
Dim i As Integer
Dim j As Integer
Dim value As Single
Dim Total As Single
    For i = 1 To NumGear
        value = 0
        Total = 0
        For j = 1 To NumGroups
            If Landing(i, j) + Discard(i, j) > 0 Then
                value = value + (Landing(i, j) + Discard(i, j)) * TTLX(j)
                Total = Total + Landing(i, j) + Discard(i, j)
            End If
        Next
        If Total > 0 Then Total = value / Total
        'Trophic level for each fleet
        TLfish(i) = Total
    Next

End Sub

Private Sub GetBoxLength()
Dim i As Integer
    For i = 1 To NumGroups
        If B(i) > 0 Then
            BoxL(i) = (B(i) / 2) ^ (1 / 3)
        Else
            BoxL(i) = 1
        End If
    Next
    'For i = 1 To NumGear
        BoxL(0) = 0.1
    'Next
End Sub

#End If
#End Region ' EwE5 code