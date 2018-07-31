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
Imports EwEUtils.Utilities
Imports System.Drawing
Imports ScientificInterfaceShared.Style
Imports System.Drawing.Drawing2D

#End Region ' Imports

Public Class cLindemanSpineDiagram

#Region " Private vars "

    Private m_manager As cNetworkManager = Nothing
    Private m_sg As cStyleGuide = Nothing
    Private m_bCollapseDetritus As Boolean = True

    Private m_iNumTL As Integer = 42
    Private m_iColWidth As Integer = 150
    Private m_iColHeight As Integer = 140
    Private m_iBoxWidth As Integer = 80
    Private m_iBoxHeight As Integer = 50

    Private m_bShowImport As Boolean = False
    Private m_bShowTST As Boolean = True
    Private m_bShowB As Boolean = True
    Private m_bShowAccum As Boolean = False

#End Region ' Private vars

    Public Sub New(ByVal manager As cNetworkManager, ByVal sg As cStyleGuide)
        Me.m_manager = manager
        Me.m_sg = sg
    End Sub

#Region " Public properties "

    Public Property NumTrophicLevels() As Integer
        Get
            Return Math.Min(Me.m_iNumTL, Me.NetworkManager.nTrophicLevels)
        End Get
        Set(ByVal value As Integer)
            Me.m_iNumTL = value
        End Set
    End Property

    Public Property CollapseDetritus() As Boolean
        Get
            Return Me.m_bCollapseDetritus
        End Get
        Set(ByVal value As Boolean)
            Me.m_bCollapseDetritus = value
        End Set
    End Property

    Public Property ShowImport() As Boolean
        Get
            Return Me.m_bShowImport
        End Get
        Set(ByVal value As Boolean)
            Me.m_bShowImport = value
        End Set
    End Property

    Public Property ShowTST() As Boolean
        Get
            Return Me.m_bShowTST
        End Get
        Set(ByVal value As Boolean)
            Me.m_bShowTST = value
        End Set
    End Property

    Public Property ShowBiomass() As Boolean
        Get
            Return Me.m_bShowB
        End Get
        Set(ByVal value As Boolean)
            Me.m_bShowB = value
        End Set
    End Property

    Public Property ShowBiomassAccum() As Boolean
        Get
            Return Me.m_bShowAccum
        End Get
        Set(ByVal value As Boolean)
            Me.m_bShowAccum = value
        End Set
    End Property

    Public Property ColWidth() As Integer
        Get
            Return Me.m_iColWidth
        End Get
        Set(ByVal value As Integer)
            Me.m_iColWidth = value
        End Set
    End Property

    Public Property ColHeight() As Integer
        Get
            Return Me.m_iColHeight
        End Get
        Set(ByVal value As Integer)
            Me.m_iColHeight = value
        End Set
    End Property

    Public Property BoxWidth() As Integer
        Get
            Return Me.m_iBoxWidth
        End Get
        Set(ByVal value As Integer)
            Me.m_iBoxWidth = value
        End Set
    End Property

    Public Property BoxHeight() As Integer
        Get
            Return Me.m_iBoxHeight
        End Get
        Set(ByVal value As Integer)
            Me.m_iBoxHeight = value
        End Set
    End Property

#End Region ' Public properties

#Region " Doodle "

    Public ReadOnly Property Size() As Size
        Get
            Dim rcBoxFar As Rectangle = Nothing

            If Me.NetworkManager Is Nothing Then Return New Size(0, 0)
            rcBoxFar = Me.GetBoxRectangle(3, Me.NumTrophicLevels)
            Return New Size(GetMargin() * 2 + rcBoxFar.X + rcBoxFar.Width, _
                            GetMargin() * 2 + rcBoxFar.Y + rcBoxFar.Height)
        End Get
    End Property

    Public Sub Draw(ByVal g As Graphics)

        Dim strTL As String = ""
        Dim sImp As Single = 0
        Dim sTST As Single = 0
        Dim sB As Single = 0
        Dim sBAccum As Single = 0
        Dim sExp As Single = 0
        Dim sResp As Single = 0
        Dim sCons As Single = 0
        Dim sTE As Single = 0
        Dim sPPF2D As Single = 0
        Dim sDetF2D As Single = 0
        Dim sSumFlowToDet As Single = 0

        Dim clrText As Color = Nothing
        Dim clrBack As Color = Nothing
        Dim clrHighlight As Color = Nothing
        Dim clrDummy As Color = Nothing
        Dim brText As Brush = Nothing
        Dim brBack As Brush = Nothing
        Dim penNormal As Pen = Nothing
        Dim penHighlight As Pen = Nothing
        Dim ft As Font = Me.m_sg.Font(cStyleGuide.eApplicationFontType.Scale)

        Me.m_sg.GetStyleColors(cStyleGuide.eStyleFlags.OK, clrText, clrBack)
        Me.m_sg.GetStyleColors(cStyleGuide.eStyleFlags.Highlight, clrHighlight, clrDummy)

        brText = New SolidBrush(clrText)
        brBack = New SolidBrush(clrBack)
        penNormal = New Pen(clrText, 1)
        penHighlight = New Pen(clrText, 3)

        For iTL As Integer = 1 To Me.NumTrophicLevels

            ' Prepare TL label
            If iTL = 1 Then strTL = "P" Else strTL = cStringUtils.ToRoman(iTL)

            ' Prep values B and BAccum
            sB = Me.NetworkManager.BiomassByTrophicLevel(iTL)
            sBAccum = 0 ' ToDo: Find source

            If Me.m_bCollapseDetritus Then
                Select Case iTL
                    Case 1
                        sImp = Me.NetworkManager.PPImport(iTL)
                        sTST = Me.NetworkManager.PPThroughtput(iTL) * 100 / (Me.NetworkManager.DetThroughtputSum + Me.NetworkManager.PPThroughtputSum)
                        sCons = Me.NetworkManager.PPConsByPred(iTL)
                    Case Else
                        sImp = Me.NetworkManager.PPImport(iTL) + Me.NetworkManager.DetImport(iTL)
                        sTST = (Me.NetworkManager.PPThroughtput(iTL) + Me.NetworkManager.DetThroughtput(iTL)) * 100 / (Me.NetworkManager.DetThroughtputSum + Me.NetworkManager.PPThroughtputSum)
                        sCons = Me.NetworkManager.PPConsByPred(iTL) + Me.NetworkManager.DetConsByPred(iTL)
                End Select
                sTE = Me.NetworkManager.TotTransferEfficiency(iTL)
                sExp = Me.NetworkManager.PPExport(iTL) + Me.NetworkManager.DetExport(iTL)
                sResp = Me.NetworkManager.PPRespiration(iTL) + Me.NetworkManager.DetRespiration(iTL)
            Else
                sImp = Me.NetworkManager.PPImport(iTL)
                sTST = Me.NetworkManager.PPThroughtput(iTL) * 100 / (Me.NetworkManager.DetThroughtputSum + Me.NetworkManager.PPThroughtputSum)
                sExp = Me.NetworkManager.PPExport(iTL)
                sResp = Me.NetworkManager.PPRespiration(iTL)

                sCons = Me.NetworkManager.PPConsByPred(iTL)
                sTE = Me.NetworkManager.PPTransferEfficiency(iTL)
            End If

            Me.RenderBox(g, penNormal, ft, _
                         brText, brBack, _
                         1, iTL, strTL, _
                         Me.FormatNumber(sImp, Me.m_bShowImport), _
                         Me.FormatNumber(sTST, Me.m_bShowTST), _
                         Me.FormatNumber(sB, Me.m_bShowB), _
                         Me.FormatNumber(sBAccum, Me.m_bShowAccum), _
                         Me.FormatNumber(sExp, iTL > 1), _
                         Me.FormatNumber(sResp, iTL > 1))

            Me.DrawPredAndTE(g, penNormal, ft, brText, _
                             1, iTL, _
                             Me.FormatNumber(sCons), _
                             Me.FormatNumber(sTE, iTL > 1))

        Next iTL

        For iTL As Integer = 1 To Me.NumTrophicLevels
            If (iTL = 1) Then strTL = "D" Else strTL = cStringUtils.ToRoman(iTL)

            sImp = Me.NetworkManager.DetImport(iTL)
            sTST = Me.NetworkManager.DetThroughtput(iTL) * 100 / (Me.NetworkManager.DetThroughtputSum + Me.NetworkManager.PPThroughtputSum)
            sB = Me.NetworkManager.DetritusByTrophicLevel(iTL)
            sBAccum = 0 ' ToDo: Find source
            sExp = Me.NetworkManager.DetExport(iTL)
            sResp = Me.NetworkManager.DetRespiration(iTL)

            sCons = Me.NetworkManager.DetConsByPred(iTL)
            sTE = Me.NetworkManager.DetTransferEfficiency(iTL)

            Me.RenderBox(g, penNormal, ft, _
                         brText, brBack, _
                         2, iTL, strTL, _
                         Me.FormatNumber(sImp, Me.m_bShowImport), _
                         Me.FormatNumber(sTST, Me.m_bShowTST), _
                         Me.FormatNumber(sB, Me.m_bShowB), _
                         Me.FormatNumber(sBAccum, Me.m_bShowAccum), _
                         Me.FormatNumber(sExp, iTL > 1), _
                         Me.FormatNumber(sResp, sResp <> 0))

            Me.DrawPredAndTE(g, penNormal, ft, brText, _
                             2, iTL, _
                             Me.FormatNumber(sCons), _
                             Me.FormatNumber(sTE, iTL > 1))

        Next iTL

        For iTL As Integer = 1 To Me.NumTrophicLevels
            If Me.m_bCollapseDetritus Then
                sPPF2D = Me.NetworkManager.PPToDetritus(iTL) + Me.NetworkManager.DetToDetritus(iTL)
                sDetF2D = 0
            Else
                sPPF2D = Me.NetworkManager.PPToDetritus(iTL)
                sDetF2D = Me.NetworkManager.DetToDetritus(iTL)
            End If
            sSumFlowToDet += (sPPF2D + sDetF2D)

            Me.DrawFlowToDetritus(g, penNormal, ft, brText, _
                                  iTL, _
                                  Me.FormatNumber(sPPF2D), _
                                  Me.FormatNumber(sDetF2D), _
                                  Me.FormatNumber(sSumFlowToDet, iTL = Me.NumTrophicLevels))
        Next

        Me.DrawConsumptionOfDetritus(g, penNormal, ft, brText, _
                                     Me.FormatNumber(Me.NetworkManager.DetConsByPred(1)))

        Me.RenderLegend(g, penNormal, ft, brText, brBack)

        ' Clean up
        ft.Dispose()
        penNormal.Dispose()
        penHighlight.Dispose()
        brText.Dispose()
        brBack.Dispose()

    End Sub

#End Region ' Doodle

#Region " Internals "

    Private ReadOnly Property NetworkManager() As cNetworkManager
        Get
            Return Me.m_manager
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; returns the position rect for a diagram box by row, col.
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <param name="iTL"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GetBoxRectangle(ByVal iRow As Integer, ByVal iTL As Integer) As Rectangle
        Return New Rectangle( _
                Me.GetMargin() + Math.Max(0, iTL - 1) * Me.m_iColWidth, _
                Me.GetMargin() + Math.Max(0, iRow - 1) * Me.m_iColHeight, _
                Me.m_iBoxWidth, _
                Me.m_iBoxHeight)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; returns the dynamically calculated margin around the graph.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GetMargin() As Integer
        Return CInt(Math.Max(Me.m_iColWidth - Me.m_iBoxWidth, Me.m_iColHeight - Me.m_iBoxHeight) * 0.5)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Renders a box in the flow.
    ''' </summary>
    ''' <param name="g">Graphics surface to doodle onto.</param>
    ''' <param name="iRow">Row where this box should be positioned.</param>
    ''' <param name="iTL">Trophic level for this box.</param>
    ''' <param name="strTL">Value to display in the trophic level section of the box.</param>
    ''' <param name="strImport">Value to display in the top-right corner of the box.</param>
    ''' <param name="strTST">Value to display in the top-left corner of the box.</param>
    ''' <param name="strBiomass">Value to display in the bottom-right corner of the box.</param>
    ''' <param name="strBiomAccum">Value to display in the bottom-left corner of the box.</param>
    ''' <param name="strExport">Value to display in the export section of the box.</param>
    ''' <param name="strResp">Value to display in the respiration section of the box.</param>
    ''' -----------------------------------------------------------------------
    Private Sub RenderBox(ByVal g As Graphics, _
                          ByVal pen As Pen, _
                          ByVal font As Font, _
                          ByVal brFore As Brush, ByVal brBack As Brush, _
                          ByVal iRow As Integer, ByVal iTL As Integer, _
                          ByVal strTL As String, _
                          ByVal strImport As String, _
                          ByVal strTST As String, _
                          ByVal strBiomass As String, _
                          ByVal strBiomAccum As String, _
                          ByVal strExport As String, _
                          ByVal strResp As String)

        If Me.m_bCollapseDetritus Then
            ' Only allow one detritus box when detritus is collapsed
            If (iRow = 2 And iTL > 1) Then Return
        End If

        Dim rcBox As Rectangle = Me.GetBoxRectangle(iRow, iTL)
        Dim rcSub As Rectangle = Nothing
        Dim fmt As New StringFormat()

        g.FillRectangle(brBack, rcBox)
        g.DrawRectangle(pen, rcBox)

        fmt.Alignment = StringAlignment.Center
        fmt.LineAlignment = StringAlignment.Center
        Using f As Font = Me.m_sg.Font(cStyleGuide.eApplicationFontType.Title)
            g.DrawString(strTL, f, brFore, rcBox, fmt)
        End Using

        If (Not String.IsNullOrEmpty(strImport)) Then
            fmt.Alignment = StringAlignment.Near
            fmt.LineAlignment = StringAlignment.Near
            g.DrawString(strImport, font, brFore, rcBox, fmt)
        End If

        If (Not String.IsNullOrEmpty(strTST)) Then
            fmt.Alignment = StringAlignment.Far
            fmt.LineAlignment = StringAlignment.Near
            g.DrawString(strTST, font, brFore, rcBox, fmt)
        End If

        If (Not String.IsNullOrEmpty(strBiomass)) Then
            fmt.Alignment = StringAlignment.Near
            fmt.LineAlignment = StringAlignment.Far
            g.DrawString(strBiomass, font, brFore, rcBox, fmt)
        End If

        If (Not String.IsNullOrEmpty(strBiomAccum)) Then
            fmt.Alignment = StringAlignment.Far
            fmt.LineAlignment = StringAlignment.Far
            g.DrawString(strBiomAccum, font, brFore, rcBox, fmt)
        End If

        If (Not String.IsNullOrEmpty(strExport)) Then
            rcSub = New Rectangle(rcBox.X + CInt(rcBox.Width * 0.5), rcBox.Y - CInt(rcBox.Height * 0.3), 1000, CInt(rcBox.Height * 0.3))
            g.DrawLine(pen, rcSub.X, rcSub.Top, rcSub.X, rcSub.Top + rcSub.Height)

            fmt.Alignment = StringAlignment.Near
            fmt.LineAlignment = StringAlignment.Near
            g.DrawString(strExport, font, brFore, rcSub, fmt)
        End If

        If (Not String.IsNullOrEmpty(strResp)) Then
            rcSub = New Rectangle(rcBox.X + CInt(rcBox.Width * 0.5), rcBox.Y + rcBox.Height, rcBox.Width, CInt(rcBox.Height * 0.3))
            g.DrawLine(pen, rcSub.X, rcSub.Top, rcSub.X, rcSub.Top + rcSub.Height)
            g.DrawLine(pen, rcSub.X, rcSub.Top + CInt(rcSub.Height * 0.6), rcSub.X - CInt(rcSub.Height * 0.6), rcSub.Top + CInt(rcSub.Height * 0.6))
            g.DrawLine(pen, rcSub.X, rcSub.Top + CInt(rcSub.Height * 0.8), rcSub.X - CInt(rcSub.Height * 0.3), rcSub.Top + CInt(rcSub.Height * 0.8))

            fmt.Alignment = StringAlignment.Near
            fmt.LineAlignment = StringAlignment.Far
            g.DrawString(strResp, font, brFore, rcSub, fmt)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Draw predation/trophic efficiency connector between two boxes.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="iRowFrom"></param>
    ''' <param name="iColFrom"></param>
    ''' <param name="strPredation"></param>
    ''' <param name="strTE"></param>
    ''' -----------------------------------------------------------------------
    Private Sub DrawPredAndTE(ByVal g As Graphics, _
                              ByVal pen As Pen, _
                              ByVal font As Font, _
                              ByVal brText As Brush, _
                              ByVal iRowFrom As Integer, ByVal iColFrom As Integer, _
                              ByVal strPredation As String, ByVal strTE As String)


        If (iColFrom < 1) Or (iColFrom >= Me.NumTrophicLevels) Then Return
        If (Me.m_bCollapseDetritus And iRowFrom <> 1) Then Return

        Dim rcBoxFrom As Rectangle = Me.GetBoxRectangle(iRowFrom, iColFrom)
        Dim rcBoxTo As Rectangle = Me.GetBoxRectangle(iRowFrom, iColFrom + 1)
        Dim rcPred As New Rectangle(rcBoxFrom.X + rcBoxFrom.Width, _
                                    rcBoxFrom.Y, _
                                    rcBoxTo.X - (rcBoxFrom.X + rcBoxFrom.Width), _
                                    CInt(rcBoxFrom.Height / 2))
        Dim rcXferF As New Rectangle(rcPred.X, rcPred.Y + rcPred.Height, rcPred.Width, rcPred.Height)
        Dim fmt As New StringFormat()
        Dim penCap As Pen = DirectCast(pen.Clone, Pen)
        penCap.EndCap = LineCap.ArrowAnchor

        g.DrawLine(penCap, rcXferF.X, rcXferF.Y, rcXferF.X + rcXferF.Width, rcXferF.Y)

        If (Not String.IsNullOrEmpty(strPredation)) Then
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Far
            g.DrawString(strPredation, font, brText, rcPred, fmt)
        End If

        If (Not String.IsNullOrEmpty(strTE)) Then
            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Near
            Using f As New Font(font, FontStyle.Italic)
                g.DrawString(strTE, f, brText, rcXferF, fmt)
            End Using
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Draw flow to detritus connector.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="iTL"></param>
    ''' <param name="strFlowP">Flow from PP to Detritus.</param>
    ''' <param name="strFlowD">Flow from Detritus to Detritus.</param>
    ''' <param name="strFlowTotal"></param>
    ''' -----------------------------------------------------------------------
    Private Sub DrawFlowToDetritus(ByVal g As Graphics, _
                                   ByVal pen As Pen, _
                                   ByVal font As Font, _
                                   ByVal brText As Brush, _
                                   ByVal iTL As Integer, _
                                   ByVal strFlowP As String, _
                                   ByVal strFlowD As String, _
                                   ByVal strFlowTotal As String)

        Dim rcBoxFrom As Rectangle = Me.GetBoxRectangle(1, iTL)
        Dim rcBoxRight As Rectangle = Me.GetBoxRectangle(1, iTL + 1)
        Dim rcBoxD As Rectangle = Me.GetBoxRectangle(2, 1)
        Dim rcLabel As Rectangle = Nothing
        Dim iBoxSpacer As Integer = rcBoxRight.X - (rcBoxFrom.X + rcBoxFrom.Width)
        Dim fmt As New StringFormat()
        Dim penCap As Pen = DirectCast(pen.Clone, Pen)
        penCap.EndCap = LineCap.ArrowAnchor

        If Me.m_bCollapseDetritus Then
            rcBoxD = Me.GetBoxRectangle(2, 1)

            If iTL = 1 Then
                ' Flow to Det rendered on left to make room for consumption of detritus
                Dim pt1 As New Point(rcBoxFrom.X, rcBoxFrom.Y + CInt(rcBoxFrom.Height * 0.8))
                Dim pt2 As New Point(rcBoxFrom.X - CInt(GetMargin() * 0.5), pt1.Y)
                Dim pt3 As New Point(pt2.X, rcBoxD.Y + CInt(rcBoxD.Height * 0.2))
                Dim pt4 As New Point(rcBoxD.X, pt3.Y)

                g.DrawLine(pen, pt1, pt2)
                g.DrawLine(pen, pt2, pt3)
                g.DrawLine(penCap, pt3, pt4)

                rcLabel = New Rectangle(pt2.X, pt1.Y, 1000, pt3.Y - pt2.Y)
                fmt.Alignment = StringAlignment.Near
                fmt.LineAlignment = StringAlignment.Center
                g.DrawString(strFlowP, font, brText, rcLabel, fmt)
            Else
                Dim pt1 As New Point(rcBoxFrom.X + rcBoxFrom.Width, rcBoxFrom.Y + CInt(rcBoxFrom.Height * 0.8))
                Dim pt2 As New Point(pt1.X + CInt(iBoxSpacer * 0.5), pt1.Y)
                Dim pt3 As New Point(pt1.X + CInt(iBoxSpacer * 0.5), rcBoxD.Y + rcBoxD.Height + CInt(GetMargin() * 0.5))
                Dim pt4 As New Point(rcBoxD.X - CInt(GetMargin() * 0.5), pt3.Y)
                Dim pt5 As New Point(pt4.X, rcBoxD.Y + CInt(rcBoxD.Height * 0.8))
                Dim pt6 As New Point(rcBoxD.X, pt5.Y)

                g.DrawLine(pen, pt1, pt2)
                g.DrawLine(penCap, pt2, pt3)
                g.DrawLine(pen, pt3, pt4)
                g.DrawLine(pen, pt4, pt5)
                g.DrawLine(penCap, pt5, pt6)

                rcLabel = New Rectangle(pt2.X, pt1.Y, pt2.X + 1000, pt4.Y - pt1.Y)
                fmt.Alignment = StringAlignment.Near
                fmt.LineAlignment = StringAlignment.Far
                g.DrawString(strFlowP, font, brText, rcLabel, fmt)

                If (Not String.IsNullOrEmpty(strFlowTotal)) Then
                    rcLabel = New Rectangle(pt5.X, pt5.Y, pt1.X - pt5.X, pt4.Y - pt5.Y)
                    fmt.Alignment = StringAlignment.Near
                    fmt.LineAlignment = StringAlignment.Far
                    g.DrawString(strFlowTotal, font, brText, rcLabel, fmt)
                End If
            End If
        Else
            Dim rcBoxDi As Rectangle = Me.GetBoxRectangle(2, iTL)
            Dim iYMid As Integer = rcBoxFrom.Y + rcBoxFrom.Height + CInt((rcBoxD.Y - (rcBoxFrom.Y + rcBoxFrom.Height)) / 2)

            Dim pt1 As New Point(rcBoxFrom.X + rcBoxFrom.Width, rcBoxFrom.Y + CInt(rcBoxFrom.Height * 0.8))
            Dim pt2 As New Point(pt1.X + CInt(iBoxSpacer * 0.5), pt1.Y)
            Dim pt3 As New Point(pt2.X, iYMid)
            Dim pt4 As New Point(rcBoxD.X + CInt(rcBoxD.Width * 0.2), pt3.Y)
            Dim pt5 As New Point(pt4.X, rcBoxD.Y)
            Dim pt6 As New Point(rcBoxDi.X + rcBoxDi.Width, rcBoxDi.Y + CInt(rcBoxDi.Height * 0.2))
            Dim pt7 As New Point(pt2.X, pt6.Y)

            If Not String.IsNullOrEmpty(strFlowP) Then
                g.DrawLine(pen, pt1, pt2)
                g.DrawLine(penCap, pt2, pt3)
                g.DrawLine(pen, pt3, pt4)
                g.DrawLine(penCap, pt4, pt5)

                fmt.Alignment = StringAlignment.Near
                fmt.LineAlignment = StringAlignment.Far
                rcLabel = New Rectangle(pt2.X, pt2.Y, 1000, pt3.Y - pt2.Y)
                g.DrawString(strFlowP, font, brText, rcLabel, fmt)
            End If

            If Not String.IsNullOrEmpty(strFlowD) And (iTL > 1) Then
                g.DrawLine(pen, pt6, pt7)
                g.DrawLine(penCap, pt7, pt3)
                g.DrawLine(pen, pt3, pt4)
                g.DrawLine(penCap, pt4, pt5)

                fmt.Alignment = StringAlignment.Near
                fmt.LineAlignment = StringAlignment.Near
                rcLabel = New Rectangle(pt3.X, pt3.Y, 1000, pt3.Y + 1000)
                g.DrawString(strFlowD, font, brText, rcLabel, fmt)
            End If

            If Not String.IsNullOrEmpty(strFlowTotal) Then
                fmt.Alignment = StringAlignment.Near
                fmt.LineAlignment = StringAlignment.Near
                rcLabel = New Rectangle(pt4.X, pt4.Y, 1000, 1000)
                g.DrawString(strFlowTotal, font, brText, rcLabel, fmt)
            End If
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Draw consumption of detritus connector.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="strConsumption"></param>
    ''' <remarks>
    ''' This connector only applies to collapsed detritus diagrams for TL II.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub DrawConsumptionOfDetritus(ByVal g As Graphics, _
                                          ByVal pen As Pen, _
                                          ByVal font As Font, _
                                          ByVal brText As Brush, _
                                          ByVal strConsumption As String)

        Dim rcBoxTo As Rectangle = Me.GetBoxRectangle(1, 2)
        Dim rcBoxDetritus As Rectangle = Me.GetBoxRectangle(2, 1)
        Dim rcLabel As Rectangle = Nothing
        Dim iBoxSpacer As Integer = rcBoxTo.X - (rcBoxDetritus.X + rcBoxDetritus.Width)
        Dim fmt As New StringFormat()
        Dim penCap As Pen = DirectCast(pen.Clone, Pen)
        penCap.EndCap = LineCap.ArrowAnchor

        If (Not Me.m_bCollapseDetritus) Then Return
        If (Me.NumTrophicLevels < 2) Then Return

        Dim pt1 As New Point(rcBoxDetritus.X + rcBoxDetritus.Width, rcBoxDetritus.Y + CInt(rcBoxDetritus.Height * 0.5))
        Dim pt2 As New Point(pt1.X + CInt(iBoxSpacer * 0.5), pt1.Y)
        Dim pt3 As New Point(pt2.X, rcBoxTo.Y + CInt(rcBoxTo.Height * 0.8))
        Dim pt4 As New Point(rcBoxTo.X, pt3.Y)

        g.DrawLine(pen, pt1, pt2)
        g.DrawLine(pen, pt2, pt3)
        g.DrawLine(penCap, pt3, pt4)

        rcLabel = New Rectangle(pt2.X - 1000, rcBoxTo.Y + rcBoxTo.Height, 1000, CInt(rcBoxTo.Height * 0.3))
        fmt.Alignment = StringAlignment.Far
        fmt.LineAlignment = StringAlignment.Far
        g.DrawString(strConsumption, font, brText, rcLabel, fmt)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Render the legend box.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <remarks>
    ''' The legend box is now fixed in one place. It would be useful addition 
    ''' if this box could be repositioned.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub RenderLegend(ByVal g As Graphics, _
                             ByVal pen As Pen, ByVal font As Font, _
                             ByVal brText As Brush, ByVal brBack As Brush)

        Dim rcBox As Rectangle = Me.GetBoxRectangle(3, 2)
        Dim rcBoxRight As Rectangle = Me.GetBoxRectangle(3, 3)
        Dim iBoxHalfSpacer As Integer = CInt((rcBoxRight.X - (rcBox.X + rcBox.Width)) / 2)
        Dim rcSub As Rectangle = Nothing
        Dim fmt As New StringFormat()

        Me.RenderBox(g, pen, font,
                     brText, brBack,
                     3, 2,
                     My.Resources.LBL_TL,
                     If(Me.m_bShowImport, My.Resources.LBL_IMPORT, ""),
                     If(Me.m_bShowTST, My.Resources.LBL_TST, ""),
                     If(Me.m_bShowB, My.Resources.LBL_BIOMASS, ""),
                     If(Me.m_bShowAccum, My.Resources.LBL_BACCUM, ""),
                     My.Resources.LBL_EXPORT, My.Resources.LBL_RESP)

        rcSub = New Rectangle(rcBox.X - 2 * iBoxHalfSpacer, rcBox.Y, 2 * iBoxHalfSpacer, CInt(rcBox.Height * 0.5))
        fmt.Alignment = StringAlignment.Center
        fmt.LineAlignment = StringAlignment.Far
        g.DrawString(My.Resources.LBL_CONS, font, brText, rcSub, fmt)
        g.DrawLine(pen, rcSub.X, rcSub.Y + rcSub.Height, rcSub.X + rcSub.Width, rcSub.Y + rcSub.Height)

        rcSub = New Rectangle(rcBox.X + rcBox.Width, rcBox.Y, 2 * iBoxHalfSpacer, CInt(rcBox.Height * 0.5))
        fmt.Alignment = StringAlignment.Center
        fmt.LineAlignment = StringAlignment.Far
        g.DrawString(My.Resources.LBL_PREDATION, font, brText, rcSub, fmt)
        g.DrawLine(pen, rcSub.X, rcSub.Y + rcSub.Height, rcSub.X + rcSub.Width, rcSub.Y + rcSub.Height)

        rcSub.Offset(0, rcSub.Height)
        fmt.LineAlignment = StringAlignment.Near
        Using f As New Font(font, FontStyle.Italic)
            g.DrawString(My.Resources.LBL_TE, f, brText, rcSub, fmt)
        End Using

        rcSub = New Rectangle(rcBox.X - 3 * iBoxHalfSpacer, rcBox.Y + CInt(rcBox.Height * 0.8), 2 * iBoxHalfSpacer, iBoxHalfSpacer)
        fmt.LineAlignment = StringAlignment.Center
        fmt.Alignment = StringAlignment.Far
        g.DrawString(My.Resources.LBL_FLOWDET, font, brText, rcSub, fmt)
        g.DrawLine(pen, rcBox.X, rcSub.Top, rcBox.X - iBoxHalfSpacer, rcSub.Top)
        g.DrawLine(pen, rcBox.X - iBoxHalfSpacer, rcSub.Top, rcBox.X - iBoxHalfSpacer, rcSub.Top + iBoxHalfSpacer)

        rcSub = New Rectangle(rcBox.X + rcBox.Width + iBoxHalfSpacer, rcBox.Y + CInt(rcBox.Height * 0.8), 2 * iBoxHalfSpacer, iBoxHalfSpacer)
        fmt.Alignment = StringAlignment.Near
        g.DrawString(My.Resources.LBL_FLOWDET, font, brText, rcSub, fmt)
        g.DrawLine(pen, rcBox.X + rcBox.Width, rcSub.Top, rcBox.X + rcBox.Width + iBoxHalfSpacer, rcSub.Top)
        g.DrawLine(pen, rcBox.X + rcBox.Width + iBoxHalfSpacer, rcSub.Top, rcBox.X + rcBox.Width + iBoxHalfSpacer, rcSub.Top + iBoxHalfSpacer)

    End Sub

    Private Function FormatNumber(ByVal sVal As Single, _
                                  Optional ByVal bIsValid As Boolean = True) As String

        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
        If Not bIsValid Then style = cStyleGuide.eStyleFlags.Null
        Return Me.m_sg.FormatNumber(sVal, style)
        'Return Me.m_sg.FormatNumber(sVal, style Or cStyleGuide.eStyleFlags.ValueComputed)

    End Function

#End Region ' Internals

End Class
