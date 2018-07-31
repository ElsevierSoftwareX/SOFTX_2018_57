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

Imports EwECore
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="cMapDrawerBase"/> for rendering data by group.
    ''' </summary>
    Public Class cMapDrawerGroup
        Inherits cMapDrawerBase

        Public Sub New(core As cCore, sg As cStyleGuide)
            MyBase.New(core, sg)
        End Sub

        Public Overrides Sub DrawMap(ByVal iItem As Integer, ByVal rcPos As Rectangle, ByVal Args As cMapDrawerArgs)

            If Me.Map Is Nothing Then Return

            Dim FScaler As Single
            Dim maptype As cMapDrawerBase.eMapType = Args.MapType
            Dim RelScaler() As Single = Args.RelMapScaler
            Dim excl As cEcospaceLayerExclusion = Me.m_core.EcospaceBasemap.LayerExclusion
            Dim brExcluded As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.Red, Color.FromArgb(&H88FF4500))
            Dim font As Font = Me.m_sg.Font(cStyleGuide.eApplicationFontType.Legend)

            If maptype = eMapType.FishingMortRate Then
                FScaler = Me.Colors.Count / Args.FishingMortLegendMax
            End If

            Using br As New SolidBrush(Me.m_sg.ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND))
                Me.Graphics.FillRectangle(br, rcPos)
            End Using

            Try
                For i As Integer = 1 To Me.InRow
                    For j As Integer = 1 To Me.InCol

                        Dim rcfCell As RectangleF = New RectangleF(CSng(rcPos.Left + (j - 1) * rcPos.Width() / Me.InCol), _
                                                                           CSng(rcPos.Top + (i - 1) * rcPos.Height() / Me.InRow), _
                                                                           CSng(rcPos.Width() / Me.InCol), _
                                                                           CSng(rcPos.Height() / Me.InRow))
                        Dim brCell As Brush = Nothing

                        'If it is water
                        If m_core.EcospaceBasemap.LayerDepth.IsWaterCell(i, j) Then
                            ' Is not excluded
                            If (Not excl.IsExcludedCell(i, j)) Then
                                ' Water Cell
                                Dim sMapValue As Single = Me.Map(i, j, iItem) / RelScaler(iItem)
                                Dim icc As Single

                                ' Old EwE5:    icc = m_ColorNum * 1 / (MapValue + 1)
                                ' Latest EwE5: icc = MaxColorsInGrad * MapValue / (MaxColorsInGrad / ColorScaling - 1 + MapValue)
                                '              ColorScaling is MaxColorsInGrad / 2

                                Select Case maptype
                                    Case eMapType.FishingMortRate
                                        'Only Fishing mort map has its own color binning 
                                        icc = sMapValue * FScaler
                                    Case Else
                                        If (sMapValue > 10.0!) Or Single.IsPositiveInfinity(sMapValue) Then
                                            icc = Me.Colors.Count
                                        ElseIf (sMapValue < 0.1!) Or Single.IsNegativeInfinity(sMapValue) Or Single.IsNaN(sMapValue) Then
                                            icc = 1
                                        Else
                                            icc = Me.Colors.Count * sMapValue / (sMapValue + 1)
                                        End If
                                End Select

                                'Boundary check
                                icc = Math.Max(Math.Min(Me.Colors.Count - 1, icc), 1)
                                brCell = New SolidBrush(Me.Colors(CInt(icc)))
                            End If
                        ElseIf Me.ShowLand Then
                            ' #Land
                            brCell = New SolidBrush(Color.Gray)
                        End If

                        If (brCell IsNot Nothing) Then
                            Me.Graphics.FillRectangle(brCell, rcfCell)
                            brCell.Dispose()
                        End If

                        ' Always show excluded?
                        If Me.ShowExcluded And excl.IsExcludedCell(i, j) = True Then
                            ' Draw excluded Cell 
                            Me.Graphics.FillRectangle(brExcluded, rcfCell)
                        End If
                    Next
                Next

            Catch ex As Exception
                'Debug.Assert(False, ex.Message)
                Exit Sub
            End Try

            brExcluded.Dispose()
            brExcluded = Nothing

            If (Me.StanzaDS IsNot Nothing) Then

                Dim isp As Integer = -1

                For ispTmp As Integer = 1 To StanzaDS.Nsplit
                    For ist As Integer = 1 To StanzaDS.Nstanza(ispTmp)
                        If iItem = StanzaDS.EcopathCode(ispTmp, ist) Then
                            If (isp = -1) Then isp = ispTmp
                        End If
                    Next ist
                Next ispTmp

                Try
                    ' JS 06Mar18: Made robust to abuse. Toggling to IBM mode after a run messed up the map drawers, who want to draw IBM data that is not there
                    If isp > -1 And StanzaDS.MaxAgeSpecies IsNot Nothing Then
                        For iaa As Integer = 0 To StanzaDS.MaxAgeSpecies(isp)
                            Dim ia As Integer = StanzaDS.AgeIndex1(isp) + iaa : If ia > StanzaDS.MaxAgeSpecies(isp) Then ia = ia - StanzaDS.MaxAgeSpecies(isp) - 1
                            Dim ist As Integer = StanzaDS.StanzaNo(isp, ia)
                            Dim ieco As Integer = StanzaDS.EcopathCode(isp, ist)

                            If ieco = iItem Then
                                For ipkt As Integer = 1 To StanzaDS.Npackets

                                    Dim sy As Single = StanzaDS.iPacket(isp, iaa, ipkt)
                                    Dim sx As Single = StanzaDS.jPacket(isp, iaa, ipkt)

                                    If CBool(excl.Cell(CInt(Math.Floor(sy)), CInt(Math.Floor(sx)))) = False Then
                                        Dim ptfCell As New PointF(CSng(rcPos.Left + (sx - 1) * rcPos.Width() / Me.InCol),
                                                                      CSng(rcPos.Top + (sy - 1) * rcPos.Height() / Me.InRow))
                                        Dim rcF As New RectangleF(ptfCell.X, ptfCell.Y, 1, 1)

                                        Me.Graphics.DrawEllipse(Pens.Black, rcF)
                                    End If

                                Next ipkt

                            End If
                        Next iaa

                    End If

                Catch ex As Exception

                End Try

            End If

            MyBase.DrawMap(iItem, rcPos, Args)

            If Me.m_sg.ShowMapLabels Then

                Dim strLabel As String = ""
                If Me.m_sg.ShowMapsDateInLabels Then
                    strLabel = String.Format(My.Resources.GENERIC_LABEL_DOUBLE, Me.m_core.EcospaceGroupInputs(iItem).Name, Me.Date)
                Else
                    strLabel = Me.m_core.EcospaceGroupInputs(iItem).Name
                End If
                Dim br As Brush = Brushes.Black
                Dim fmt As New StringFormat()

                fmt.Alignment = Me.m_sg.MapLabelPosHorizontal
                fmt.LineAlignment = Me.m_sg.MapLabelPosVertical

                If Me.m_sg.InvertMapLabelColor Then br = Brushes.White

                Me.Graphics.DrawString(strLabel, font, br, rcPos, fmt)
            End If

            font.Dispose()

        End Sub

    End Class

End Namespace