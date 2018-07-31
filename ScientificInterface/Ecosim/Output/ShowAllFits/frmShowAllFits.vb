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

Option Explicit On
Option Strict On

Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecosim

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Form implementing the Ecosim Show All Fits interface.
    ''' </summary>
    ''' <remarks>
    ''' Why are we not using ZedGraph here?!
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class frmShowAllFits

#Region " Private vars "

        Private m_sDotSize As Single
        Private m_sLineWidth As Single
        Private m_sLRMargin As Single
        Private m_sTBMargin As Single
        Private m_iCol As Integer
        Private m_iRow As Integer

        Private m_sPlotWidth As Single
        Private m_sPlotHeight As Single

        Private m_lPlots As New List(Of cShowAllFitsPlotData)

        Private m_nNumPlots As Integer
        Private m_NTimes As Integer

        Private m_lShownPlotsType As New List(Of eTimeSeriesType)

#End Region ' Private vars

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_NTimes = Me.Core.nEcosimTimeSteps

            Me.InitializeDefaultParams()
            Me.InitializePlotData()
            Me.CalcPlotParams()
            Me.SetPlotTypes()

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSim}

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            MyBase.OnFormClosed(e)
        End Sub

        Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
            MyBase.OnResize(e)
            Me.m_pbPlots.Invalidate()
        End Sub

#Region " Rendering "

        Private Sub DrawPlots(ByRef g As Graphics, ByVal iWidth As Integer, ByVal iHeight As Integer)

            Dim pen As Pen = New Pen(Color.Black, m_sLineWidth)
            Dim ptfTL As PointF = toDevicePoint(New PointF(m_sLRMargin, 1.02F * m_iRow + m_sTBMargin), iWidth, iHeight)
            Dim szPosName As SizeF = toDeviceSize(New SizeF(0.02F, 0.02F), iWidth, iHeight)
            Dim pzPosGraph As SizeF = toDeviceSize(New SizeF(m_sPlotWidth, m_sPlotHeight), iWidth, iHeight)
            Dim iPlot As Integer = 0
            Dim plot As cShowAllFitsPlotData = Nothing
            Dim strTitle As String = ""
            Dim iRow, iCol As Integer
            Dim sPosX, sPosY As Single
            Dim data() As Single
            Dim ftCaption As Font = Me.StyleGuide.Font(cStyleGuide.eApplicationFontType.Legend)
            Dim ftScale As Font = Me.StyleGuide.Font(cStyleGuide.eApplicationFontType.Scale)
            Dim handler As New cTimeSeriesShapeGUIHandler(Me.UIContext)

            For i As Integer = 0 To Me.m_lPlots.Count - 1

                plot = Me.m_lPlots(i)

                If plot.Visible Then

                    iRow = CInt(Math.Floor(iPlot \ m_iCol))
                    iCol = iPlot Mod m_iCol
                    sPosX = ptfTL.X + iCol * pzPosGraph.Width
                    sPosY = ptfTL.Y + iRow * pzPosGraph.Height

                    ' ===============
                    ' Draw background
                    ' ===============
                    g.DrawRectangle(Pens.Black, sPosX, sPosY, pzPosGraph.Width, pzPosGraph.Height)

                    ' ===============
                    ' Draw title
                    ' ===============
                    strTitle = plot.TimeSeries.Name
                    If (Me.m_cbShowGroupNo.Checked) Then
                        strTitle = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, plot.TimeSeries.DatPool, strTitle)
                    End If
                    If Me.m_chkShowWeight.Checked Then
                        strTitle = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, strTitle, Me.StyleGuide.FormatNumber(plot.TimeSeries.WtType))
                    End If
                    If Me.m_chkShowSS.Checked Then
                        strTitle = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, strTitle, Me.StyleGuide.FormatNumber(plot.TimeSeries.DataSS))
                    End If

                    g.DrawString(strTitle, ftCaption, Brushes.Black, sPosX + szPosName.Width, sPosY + szPosName.Height)

                    ' Test axis for extreme values
                    If (Not Single.IsNaN(plot.YMax)) And (plot.YMax > 0) Then

                        g.Clip = New Region(New Rectangle(CInt(sPosX), CInt(sPosY), CInt(pzPosGraph.Width), CInt(pzPosGraph.Height)))

                        ' ===============
                        ' Draw time series
                        ' ===============
                        Dim dx As Double = cCore.N_MONTHS / m_NTimes
                        If plot.TimeSeries.Interval = eTSDataSetInterval.TimeStep Then dx = 1 / m_NTimes
                        data = plot.TimeSeries.ShapeData
                        For k As Integer = 1 To data.Length - 1
                            If (data(k) > 0) Then
                                Dim dotXRelPos As Single = CSng(m_sPlotWidth * (k - 0.5!) * dx)
                                Dim dotYRelPos As Single = CSng(m_sPlotHeight * (1 - data(k) * plot.TSDataScale / plot.YMax))
                                Dim dotPos As SizeF = toDeviceSize(New SizeF(dotXRelPos, dotYRelPos), iWidth, iHeight)
                                Select Case handler.SketchDrawMode(plot.TimeSeries)
                                    Case eSketchDrawModeTypes.TimeSeriesDriver
                                        g.DrawEllipse(pen, New RectangleF(sPosX + dotPos.Width - (0.5! * m_sDotSize),
                                                                          sPosY + dotPos.Height - (0.5! * m_sDotSize), m_sDotSize, m_sDotSize))
                                    Case eSketchDrawModeTypes.TimeSeriesRefAbs
                                        g.DrawRectangle(pen, New Rectangle(CInt(sPosX + dotPos.Width - (0.5! * m_sDotSize)),
                                                                          CInt(sPosY + dotPos.Height - (0.5! * m_sDotSize)), CInt(m_sDotSize), CInt(m_sDotSize)))
                                    Case eSketchDrawModeTypes.TimeSeriesRefRel
                                        ' You've got to love pointless duplication!! See cShapeImage, ugh...
                                        Dim pts(4) As PointF
                                        pts(0) = New PointF(sPosX + dotPos.Width, sPosY + dotPos.Height - 0.5! * m_sDotSize)
                                        pts(1) = New PointF(sPosX + dotPos.Width + 0.5! * m_sDotSize, sPosY + dotPos.Height)
                                        pts(2) = New PointF(sPosX + dotPos.Width, sPosY + dotPos.Height + 0.5! * m_sDotSize)
                                        pts(3) = New PointF(sPosX + dotPos.Width - 0.5! * m_sDotSize, sPosY + dotPos.Height)
                                        pts(4) = pts(0)
                                        g.DrawPolygon(pen, pts)
                                    Case Else
                                        Debug.Assert(False)
                                End Select
                            End If
                        Next

                        ' ===============
                        ' Draw results
                        ' ===============
                        data = plot.SimData
                        If (Not data Is Nothing) Then
                            For k As Integer = 1 To data.Length - 2

                                Dim x1RelPos As Single = m_sPlotWidth * k / m_NTimes
                                Dim x2RelPos As Single = m_sPlotWidth * (k + 1) / m_NTimes

                                Dim y1RelPos As Single = m_sPlotHeight * (1 - data(k) / plot.YMax)
                                Dim y2RelPos As Single = m_sPlotHeight * (1 - data(k + 1) / plot.YMax)

                                Dim p1Pos As SizeF = toDeviceSize(New SizeF(x1RelPos, y1RelPos), iWidth, iHeight)
                                Dim p2Pos As SizeF = toDeviceSize(New SizeF(x2RelPos, y2RelPos), iWidth, iHeight)

                                g.DrawLine(pen, sPosX + p1Pos.Width, sPosY + p1Pos.Height, sPosX + p2Pos.Width, sPosY + p2Pos.Height)

                            Next
                        Else
                            Console.WriteLine("ShowAllFits: Missing Ecosim data for results {0}, time series {1}", i, plot.TimeSeries.Name)
                        End If

                        ' Restore clip
                        g.Clip = New Region(New Rectangle(0, 0, iWidth, iHeight))

                    Else
                        Using p As New Pen(Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.INVALIDMODELRESULT_TEXT))
                            g.DrawLine(p, sPosX, sPosY, sPosX + pzPosGraph.Width, sPosY + pzPosGraph.Height)
                            g.DrawLine(p, sPosX, sPosY + pzPosGraph.Height, sPosX + pzPosGraph.Width, sPosY)
                        End Using
                    End If

                    ' Plot handled
                    iPlot += 1
                End If
            Next i ' Next result

            If Me.m_chkShowYear.Checked Then

                Dim stepYear As Integer = m_NTimes \ (cCore.N_MONTHS * 4)
                'Dim fs As Single

                Dim iEnd As Integer = m_iCol - 1
                If iPlot < m_iCol Then
                    iEnd = iPlot - 1
                End If
                For i As Integer = 0 To iEnd
                    Dim yPos As Single = ptfTL.Y + CInt(Math.Ceiling(iPlot / m_iCol)) * pzPosGraph.Height + toDeviceSize(New SizeF(0, 0.005F + m_sTBMargin), iWidth, iHeight).Height
                    For t As Integer = 0 To 3
                        g.DrawString((Me.Core.EcosimFirstYear + t * stepYear).ToString, ftScale, Brushes.Black, ptfTL.X + i * pzPosGraph.Width + (t * pzPosGraph.Width / 4), yPos)
                    Next

                Next
            End If

            ftCaption.Dispose()
            ftScale.Dispose()

        End Sub

        Private Function GenerateImage() As System.Drawing.Image

            Dim img As Bitmap = New Bitmap(m_pbPlots.Width, m_pbPlots.Height)
            img.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)

            Dim bg As Graphics = Graphics.FromImage(img)
            bg.Clear(m_pbPlots.BackColor)
            DrawPlots(bg, img.Width, img.Height)

            Return img

        End Function

#End Region ' Rendering

#Region " Internal mucky bits "

        ''' <summary>
        ''' Init the form with default values. Call this only once.
        ''' </summary>
        Private Sub InitializeDefaultParams()

            ' Defaults
            m_sDotSize = 3
            m_sLineWidth = 1
            m_sLRMargin = 0.1
            m_sTBMargin = 0.1
            m_iCol = 3

            ' Update controls
            Me.m_nudRowNum.Value = Me.m_iCol
            Me.m_nudDotSize.Value = CDec(Me.m_sDotSize)
            Me.m_nudLineWidth.Value = CDec(Me.m_sLineWidth)
            Me.m_nudMarginLR.Value = CDec(Me.m_sLRMargin)
            Me.m_nudMarginTB.Value = CDec(Me.m_sTBMargin)

            ' Hmm
            SetPlotTypes()
        End Sub

        ''' <summary>
        ''' Init plot data administration structure. Call this only once.
        ''' </summary>
        Private Sub InitializePlotData()

            Dim ts As cTimeSeries = Nothing
            Dim asSimData(Me.Core.nEcosimTimeSteps) As Single
            Dim plot As cShowAllFitsPlotData = Nothing

            For iTS As Integer = 1 To Me.Core.nTimeSeries

                ts = Me.Core.EcosimTimeSeries(iTS)

                If m_lShownPlotsType.Contains(ts.TimeSeriesType) Then

                    Select Case cTimeSeriesFactory.TimeSeriesCategory(ts.TimeSeriesType)
                        Case eTimeSeriesCategoryType.Group
                            Dim gts As cGroupTimeSeries = DirectCast(ts, cGroupTimeSeries)
                            Dim iGroup As Integer = gts.GroupIndex

                            If (iGroup > 0) Then

                                Dim grpOutput As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

                                For iTime As Integer = 1 To Me.Core.nEcosimTimeSteps

                                    asSimData(iTime) = 0.0!

                                    Select Case gts.TimeSeriesType

                                        Case eTimeSeriesType.Catches,
                                             eTimeSeriesType.CatchesRel,
                                             eTimeSeriesType.CatchesForcing
                                            asSimData(iTime) = grpOutput.Catch(iTime)

                                        Case eTimeSeriesType.TotalMortality
                                            asSimData(iTime) = grpOutput.TotalMort(iTime)

                                        Case eTimeSeriesType.AverageWeight
                                            If grpOutput.IsMultiStanza Then
                                                asSimData(iTime) = grpOutput.AvgWeight(iTime)
                                            End If

                                        Case Else
                                            asSimData(iTime) = grpOutput.Biomass(iTime)

                                    End Select
                                Next
                            End If

                        Case eTimeSeriesCategoryType.Fleet
                            ' NOP

                        Case eTimeSeriesCategoryType.FleetGroup
                            Dim fts As cFleetTimeSeries = DirectCast(ts, cFleetTimeSeries)
                            Dim iGroup As Integer = fts.GroupIndex
                            Dim iFleet As Integer = fts.FleetIndex

                            If (iFleet > 0 And iGroup > 0) Then

                                Dim grpOutput As cEcosimGroupOutput = Me.Core.EcoSimGroupOutputs(iGroup)

                                For iTime As Integer = 1 To Me.Core.nEcosimTimeSteps

                                    asSimData(iTime) = 0.0!

                                    Select Case fts.TimeSeriesType
                                        Case eTimeSeriesType.Landings
                                            asSimData(iTime) = grpOutput.LandingsByFleet(iFleet, iTime)

                                        Case eTimeSeriesType.Discards
                                            asSimData(iTime) = grpOutput.DiscardByFleet(iFleet, iTime)

                                        Case Else
                                            Debug.Assert(False)

                                    End Select
                                Next
                            End If

                    End Select
                End If

                plot = New cShowAllFitsPlotData(ts, asSimData)
                Me.m_lPlots.Add(plot)
            Next iTS

        End Sub

        Private Sub CalcPlotParams()

            Me.m_nNumPlots = Me.SetPlotVisibility()
            'm_NumPlots = 15

            Me.m_iCol = CInt(Me.m_nudRowNum.Value)
            If Me.m_iCol <= 0 Then Me.m_iCol = 3
            Me.m_sPlotWidth = 8.0F / Me.m_iCol

            Me.m_iRow = CInt(Math.Ceiling(Me.m_nNumPlots / Me.m_iCol))
            If Me.m_chkScaleForPrinter.Checked Then
                If Me.m_iRow <= 10 Then Me.m_iRow = 10
            End If

            If Me.m_chkShowYear.Checked Then
                Me.m_sPlotHeight = 0.99F
            Else
                Me.m_sPlotHeight = 1.0F
            End If

            Me.m_sLRMargin = CSng(Me.m_nudMarginLR.Value)
            Me.m_sTBMargin = CSng(Me.m_nudMarginTB.Value)
            Me.m_sDotSize = CSng(Me.m_nudDotSize.Value)
            Me.m_sLineWidth = CSng(Me.m_nudLineWidth.Value)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Sets the type of plots to show, based on user preferences of what to view.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub SetPlotTypes()

            If (Me.UIContext Is Nothing) Then Return

            Me.m_lShownPlotsType.Clear()

            If Me.m_chkShowB.Checked Then
                Me.m_lShownPlotsType.Add(eTimeSeriesType.BiomassForcing)
                Me.m_lShownPlotsType.Add(eTimeSeriesType.BiomassRel)
                Me.m_lShownPlotsType.Add(eTimeSeriesType.BiomassAbs)
            End If

            If Me.m_chkShowZ.Checked Then
                m_lShownPlotsType.Add(eTimeSeriesType.TotalMortality)
            End If

            If Me.m_chkShowCatch.Checked Then
                m_lShownPlotsType.Add(eTimeSeriesType.Catches)
                m_lShownPlotsType.Add(eTimeSeriesType.CatchesForcing)
                m_lShownPlotsType.Add(eTimeSeriesType.CatchesRel)
            End If

            If Me.m_chkShowDiscards.Checked Then
                m_lShownPlotsType.Add(eTimeSeriesType.Discards)
            End If

            If Me.m_chkShowLandings.Checked Then
                m_lShownPlotsType.Add(eTimeSeriesType.Landings)
            End If

            Me.m_lShownPlotsType.Add(eTimeSeriesType.AverageWeight)

            Me.UpdatePlots()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Toggle the visibility state of plots based on user preferences.
        ''' </summary>
        ''' <returns>The number of visible plots</returns>
        ''' -------------------------------------------------------------------
        Private Function SetPlotVisibility() As Integer

            Dim iNumVisiblePlots As Integer = 0
            Dim ts As cTimeSeries = Nothing

            For Each plot As cShowAllFitsPlotData In Me.m_lPlots
                ' Assume the worst
                plot.Visible = False
                ' Get TS
                ts = plot.TimeSeries
                ' Can show type?
                If m_lShownPlotsType.Contains(ts.TimeSeriesType) Then
                    ' Is applied?
                    If ts.Enabled() Then
                        ' Dunno what this is (yet)
                        If plot.Selected Then
                            ' Show the plot
                            plot.Visible = True
                            ' Count it
                            iNumVisiblePlots += 1
                        End If
                    End If
                End If
            Next

            Return iNumVisiblePlots

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, transforms a model value (point) to a point on one of
        ''' the plots
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function toDevicePoint(ByVal p As PointF, ByVal width As Integer, ByVal height As Integer) As PointF
            ' Transforms the output value to the screen point value
            ' This comes from EWE5. Real men don't write code comments, *sigh*
            Dim screenPt As New PointF(p.X * width / (8 + 2 * m_sLRMargin), _
                            height - (height * p.Y) / (1.02F * m_iRow + 2.02F * m_sTBMargin))

            Return screenPt

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, transforms a model value (size) to a value on one of
        ''' the plots
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' -------------------------------------------------------------------
        Private Function toDeviceSize(ByVal s As SizeF, ByVal width As Integer, ByVal height As Integer) As SizeF
            ' This comes from EWE5. Real men don't write code comments, *sigh*
            Dim size As New SizeF(width * s.Width / (8 + 2 * m_sLRMargin),
                        height * s.Height / (1.02F * m_iRow + 2.02F * m_sTBMargin))

            Return size

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the plots by recalculating content and redrawing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdatePlots()
            Me.CalcPlotParams()
            Me.m_pbPlots.Invalidate()
        End Sub

#Region " Printing "

        Private Sub pdAllFits_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) _
            Handles m_printdocAllFits.PrintPage
            Dim g As Graphics = e.Graphics
            DrawPlots(g, e.MarginBounds.Width, e.MarginBounds.Height)
        End Sub

#End Region ' Printing

#Region " Saving "

        Private Enum eAllFitFile As Integer
            Biomass
            Mortality
            [Catch]
            Landings
            Discards
        End Enum

        Private Sub SaveToCSV(ByVal strPath As String)

            Dim strFileName As String = ""
            Dim strTargetPath As String = ""
            Dim ts As cTimeSeries = Nothing
            Dim plot As cShowAllFitsPlotData = Nothing
            Dim sw As StreamWriter = Nothing
            Dim msg As cMessage = Nothing
            Dim bSucces As Boolean = True

            If String.IsNullOrEmpty(strPath) Then Return
            If (Me.Core.ActiveTimeSeriesDatasetIndex <= 0) Then Return

            Dim tsd As cTimeSeriesDataset = Me.Core.TimeSeriesDataset(Me.Core.ActiveTimeSeriesDatasetIndex)
            Dim bAnnual As Boolean = (tsd.TimeSeriesInterval = eTSDataSetInterval.Annual)
            Dim nSteps As Integer = CInt(Me.Core.nEcosimTimeSteps / Me.Core.nEcosimYears)

            msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_SUCCESS, strPath), _
                               eMessageType.NotSet, eCoreComponentType.EcoSim, eMessageImportance.Information)

            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_PLEASE_WAIT)

            For Each f As eAllFitFile In [Enum].GetValues(GetType(eAllFitFile))

                strFileName = Me.Core.EwEModel.Name
                Select Case f
                    Case eAllFitFile.Biomass
                        strFileName &= "_allfit_biomass.csv"
                    Case eAllFitFile.Mortality
                        strFileName &= "_allfit_mortality.csv"
                    Case eAllFitFile.Catch
                        strFileName &= "_allfit_catches.csv"
                    Case eAllFitFile.Landings
                        strFileName &= "_allfit_landings.csv"
                    Case eAllFitFile.Discards
                        strFileName &= "_allfit_discards.csv"
                End Select
                strTargetPath = Path.Combine(strPath, cFileUtils.ToValidFileName(strFileName, False))

                If cFileUtils.IsDirectoryAvailable(strPath, True) Then
                    Try
                        sw = New StreamWriter(strTargetPath, False)
                    Catch ex As Exception
                        ' Notify user
                        msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_FAILURE, strPath, ex.Message),
                                eMessageType.NotSet, eCoreComponentType.EcoSim, eMessageImportance.Critical)
                        bSucces = False
                    End Try
                End If

                If (sw IsNot Nothing) Then

                    Try
                        If Me.Core.SaveWithFileHeader Then
                            sw.Write(Me.Core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                            sw.WriteLine()
                        End If
                        sw.Write("Timestep")
                        sw.Write(",")
                        For j As Integer = 1 To Me.Core.nTimeSeries

                            plot = Me.m_lPlots(j - 1)
                            ts = plot.TimeSeries

                            Select Case f
                                Case eAllFitFile.Biomass
                                    If ts.TimeSeriesType = eTimeSeriesType.BiomassRel Or ts.TimeSeriesType = eTimeSeriesType.BiomassAbs Then
                                        sw.Write(cStringUtils.ToCSVField("biomass (predicted) " & ts.Name))
                                        sw.Write(",")
                                        sw.Write(cStringUtils.ToCSVField("biomass (observed) " & ts.Name))
                                        sw.Write(",")
                                    End If
                                Case eAllFitFile.Mortality
                                    If ts.TimeSeriesType = eTimeSeriesType.TotalMortality Then
                                        sw.Write(cStringUtils.ToCSVField("z (predicted)" & ts.Name))
                                        sw.Write(",")
                                        sw.Write(cStringUtils.ToCSVField("z (observed)" & ts.Name))
                                        sw.Write(",")
                                    End If
                                Case eAllFitFile.Catch
                                    If ts.TimeSeriesType = eTimeSeriesType.Catches Or ts.TimeSeriesType = eTimeSeriesType.CatchesForcing _
                                        Or ts.TimeSeriesType = eTimeSeriesType.CatchesRel Then
                                        sw.Write(cStringUtils.ToCSVField("catch (predicted)" & ts.Name))
                                        sw.Write(",")
                                        sw.Write(cStringUtils.ToCSVField("catch (observed) " & ts.Name))
                                        sw.Write(",")
                                    End If
                                Case eAllFitFile.Landings
                                    If ts.TimeSeriesType = eTimeSeriesType.Landings Then
                                        sw.Write(cStringUtils.ToCSVField("landings (predicted)" & ts.Name))
                                        sw.Write(",")
                                        sw.Write(cStringUtils.ToCSVField("landings (observed)" & ts.Name))
                                        sw.Write(",")
                                    End If
                                Case eAllFitFile.Discards
                                    If ts.TimeSeriesType = eTimeSeriesType.Catches Or ts.TimeSeriesType = eTimeSeriesType.CatchesForcing _
                                        Or ts.TimeSeriesType = eTimeSeriesType.CatchesRel Then
                                        sw.Write(cStringUtils.ToCSVField("discards (predicted) " & ts.Name))
                                        sw.Write(",")
                                        sw.Write(cStringUtils.ToCSVField("discards (observed)" & ts.Name))
                                        sw.Write(",")
                                    End If
                            End Select
                        Next
                        sw.WriteLine()

                        For k As Integer = 1 To Me.Core.nEcosimTimeSteps

                            ' When in annual mode, only write TS  data values for the midway point of the year
                            Dim bWriteTS As Boolean = ((bAnnual = False) Or (1 + ((k - 1) Mod nSteps) = Math.Floor(nSteps / 2)))

                            sw.Write(cStringUtils.FormatInteger(k))
                            sw.Write(",")
                            For j As Integer = 1 To Me.m_lPlots.Count

                                plot = Me.m_lPlots(j - 1)
                                ts = plot.TimeSeries

                                Dim t As Integer = If(bAnnual, ((k - 1) \ nSteps) + 1, k)

                                Select Case f
                                    Case eAllFitFile.Biomass
                                        If ts.TimeSeriesType = eTimeSeriesType.BiomassRel Or ts.TimeSeriesType = eTimeSeriesType.BiomassAbs Then
                                            sw.Write(plot.SimData(k))
                                            sw.Write(",")
                                            If ((ts.ShapeData(t) > 0) And (bWriteTS = True)) Then
                                                sw.Write(cStringUtils.FormatNumber(ts.ShapeData(t) * plot.TSDataScale))
                                            Else
                                                sw.Write("")
                                            End If
                                            sw.Write(",")
                                        End If
                                    Case eAllFitFile.Mortality
                                        If ts.TimeSeriesType = eTimeSeriesType.TotalMortality Then
                                            sw.Write(plot.SimData(k))
                                            sw.Write(",")
                                            If ((ts.ShapeData(t) > 0) And (bWriteTS = True)) Then
                                                sw.Write(cStringUtils.FormatNumber(ts.ShapeData(t) * plot.TSDataScale))
                                            Else
                                                sw.Write("")
                                            End If
                                            sw.Write(",")
                                        End If
                                    Case eAllFitFile.Catch
                                        If ts.TimeSeriesType = eTimeSeriesType.Catches Or ts.TimeSeriesType = eTimeSeriesType.CatchesForcing _
                                            Or ts.TimeSeriesType = eTimeSeriesType.CatchesRel Then
                                            sw.Write(plot.SimData(k))
                                            sw.Write(",")
                                            If ((ts.ShapeData(t) > 0) And (bWriteTS = True)) Then
                                                sw.Write(cStringUtils.FormatNumber(ts.ShapeData(t) * plot.TSDataScale))
                                            Else
                                                sw.Write("")
                                            End If
                                            sw.Write(",")
                                        End If
                                    Case eAllFitFile.Landings
                                        If ts.TimeSeriesType = eTimeSeriesType.Landings Then
                                            sw.Write(plot.SimData(k))
                                            sw.Write(",")
                                            If ((ts.ShapeData(t) > 0) And (bWriteTS = True)) Then
                                                sw.Write(cStringUtils.FormatNumber(ts.ShapeData(t) * plot.TSDataScale))
                                            Else
                                                sw.Write("")
                                            End If
                                            sw.Write(",")
                                        End If
                                    Case eAllFitFile.Discards
                                        If ts.TimeSeriesType = eTimeSeriesType.Discards Then
                                            sw.Write(plot.SimData(k))
                                            sw.Write(",")
                                            If ((ts.ShapeData(t) > 0) And (bWriteTS = True)) Then
                                                sw.Write(cStringUtils.FormatNumber(ts.ShapeData(t) * plot.TSDataScale))
                                            Else
                                                sw.Write("")
                                            End If
                                            sw.Write(",")
                                        End If
                                End Select

                            Next
                            sw.WriteLine()
                        Next
                        msg.Hyperlink = strPath

                    Catch ex As Exception
                        msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_FAILURE, strPath, ex.Message),
                                           eMessageType.NotSet, eCoreComponentType.EcoSim, eMessageImportance.Critical)
                        bSucces = False
                    Finally
                        sw.Close()
                    End Try

                End If

                If bSucces = False Then
                    Exit For
                End If

            Next

            ' Clear status text
            cApplicationStatusNotifier.EndProgress(Me.Core)

            ' Notify user
            Me.Core.Messages.SendMessage(msg)

        End Sub

#End Region ' Saving

#End Region ' Internal mucky bits

#Region " Event handlers "

        Private Sub OnSaveAsCSVClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiSaveAsCSV.Click

            Dim cmd As cDirectoryOpenCommand = DirectCast(Me.CommandHandler.GetCommand(cDirectoryOpenCommand.COMMAND_NAME), cDirectoryOpenCommand)

            cmd.Directory = Me.UIContext.Core.DefaultOutputPath(eAutosaveTypes.Ecosim)
            cmd.Invoke()

            If cmd.Result = System.Windows.Forms.DialogResult.OK Then
                Me.SaveToCSV(cmd.Directory)
            End If

        End Sub

        Private Sub OnSaveAsImage(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiSaveAsImage.Click

            Dim img As Image = Nothing
            Dim imgFormat As System.Drawing.Imaging.ImageFormat = System.Drawing.Imaging.ImageFormat.Bmp
            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)

            cmdFS.Invoke(SharedResources.FILEFILTER_IMAGE)
            If cmdFS.Result = System.Windows.Forms.DialogResult.OK Then

                Select Case cmdFS.FilterIndex
                    Case 1
                        imgFormat = System.Drawing.Imaging.ImageFormat.Bmp
                    Case 2
                        imgFormat = System.Drawing.Imaging.ImageFormat.Jpeg
                    Case 3
                        imgFormat = System.Drawing.Imaging.ImageFormat.Gif
                    Case 4
                        imgFormat = System.Drawing.Imaging.ImageFormat.Png
                    Case 5
                        imgFormat = System.Drawing.Imaging.ImageFormat.Tiff
                    Case Else
                        Debug.Assert(False)
                End Select
                img = Me.GenerateImage()
                img.Save(Path.ChangeExtension(cmdFS.FileName, imgFormat.ToString()), imgFormat)
            End If
        End Sub

        Private Sub OnPrintClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiPrint.Click

            Dim dlgPrint As New PrintDialog()
            dlgPrint.UseEXDialog = True

            dlgPrint.Document = Me.m_printdocAllFits
            If dlgPrint.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                m_printdocAllFits.DocumentName = Me.Text
                m_printdocAllFits.Print()
            End If

        End Sub

        Private Sub OnPrintPreviewClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiPrintPreview.Click

            Dim dlg As New PrintPreviewDialog()
            Dim msg As cMessage = Nothing

            dlg.Document = Me.m_printdocAllFits

            Try
                dlg.ShowDialog()
            Catch ex As Exception
                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_PRINT_PREVIEW_FAILED, ex.Message), _
                                   eMessageType.Any, eCoreComponentType.External, eMessageImportance.Warning)
                Me.Core.Messages.SendMessage(msg)
            End Try

        End Sub

        Private Sub OnShowDataChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_chkShowB.CheckedChanged,
                    m_chkShowZ.CheckedChanged,
                    m_chkShowCatch.CheckedChanged,
                    m_chkShowLandings.CheckedChanged,
                    m_chkShowDiscards.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return
            Me.SetPlotTypes()

        End Sub

        Private Sub OnShowDetailsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_chkShowWeight.CheckedChanged, _
                    m_chkShowYear.CheckedChanged, _
                    m_chkScaleForPrinter.CheckedChanged, _
                    m_chkShowSS.CheckedChanged, _
                    m_cbShowGroupNo.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return
            Me.UpdatePlots()

        End Sub

        Private Sub OnDotSizeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudDotSize.ValueChanged

            If (Me.UIContext Is Nothing) Then Return
            Me.UpdatePlots()

        End Sub

        Private Sub OnLineWidthChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudLineWidth.ValueChanged

            If (Me.UIContext Is Nothing) Then Return
            Me.UpdatePlots()

        End Sub

        Private Sub OnRowNumChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudRowNum.ValueChanged

            If (Me.UIContext Is Nothing) Then Return
            Me.UpdatePlots()

        End Sub

        Private Sub OnMarginLRChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudMarginLR.ValueChanged

            If (Me.UIContext Is Nothing) Then Return
            Me.UpdatePlots()

        End Sub

        Private Sub OnMarginTBChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_nudMarginTB.ValueChanged
            Me.UpdatePlots()
        End Sub

        ''' <summary>
        ''' HS = Hide / Show
        ''' </summary>
        Private Sub tsBtnHSPlots_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiChoosePlots.Click

            Dim dlg As New dlgSelectAllFitsPlots(Me.m_lPlots.ToArray())
            If (dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
                Me.CalcPlotParams()
                Me.m_pbPlots.Invalidate()
            End If

        End Sub

        Private Sub OnPaintPlots(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) _
            Handles m_pbPlots.Paint

            Me.DrawPlots(e.Graphics, m_pbPlots.Width, m_pbPlots.Height)

        End Sub

        Private Sub tsBtnChangeYScale_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnScale.Click

            Dim dlgChYScale As New dlgChangeYScale(Me.m_lPlots)
            If (dlgChYScale.ShowDialog = System.Windows.Forms.DialogResult.OK) Then
                m_pbPlots.Invalidate()
            End If

        End Sub

        Protected Overrides Sub OnStyleguideChanged(ByVal changeType As cStyleGuide.eChangeType)
            ' Redraw
            Me.Invalidate()
        End Sub

        Private Sub OnToggleOptions(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiOptions.Click
            Me.m_scMain.Panel1Collapsed = (Me.m_tsmiOptions.Checked = False)
        End Sub

#End Region ' Event handlers

    End Class

End Namespace



