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

Imports EwECore
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Definitions
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities
Imports System.Text

#End Region

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control, implements a control for viewing (not sketchng) Ecosim
    ''' time series shapes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucTimeSeriesSketchPad

        Public Sub New()

            Me.InitializeComponent()
            Me.m_sketchDrawMode = eSketchDrawModeTypes.TimeSeriesDriver

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the labels to display along the X axis.
        ''' </summary>
        ''' <param name="iWidth">Width of the X axis for the labels.</param>
        ''' <param name="sScale">Label placement scale factor along the X axis.</param>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub GetXAxisLabels(ByVal iWidth As Integer, ByRef astrLabels As String(), ByRef sScale As Single)

            Dim iDS As Integer = Me.UIContext.Core.ActiveTimeSeriesDatasetIndex
            Dim ds As cTimeSeriesDataset = Nothing
            Dim ts As cTimeSeries = DirectCast(Me.Shape, cTimeSeries)
            Dim iTSFinalYear As Integer = 0
            Dim iTSFirstYear As Integer = 0
            Dim iStepSize As Integer = 1
            Dim lstrAxis As New List(Of String)
            sScale = 1

            If (Me.Shape IsNot Nothing) And (TypeOf Me.Shape Is cTimeSeries) Then

                If iDS >= 0 Then ds = Me.UIContext.Core.TimeSeriesDataset(iDS)

                If ds IsNot Nothing Then
                    iTSFirstYear = ds.FirstYear
                Else
                    iTSFirstYear = 1
                End If

                Select Case ts.Interval
                    Case eTSDataSetInterval.Annual
                        iTSFinalYear = iTSFirstYear + Me.XAxisMaxValue
                        iStepSize = Math.Max(1, CInt((iTSFinalYear - iTSFirstYear) / 10))
                    Case eTSDataSetInterval.TimeStep
                        iTSFinalYear = iTSFirstYear + (Me.XAxisMaxValue \ cCore.N_MONTHS)
                        iStepSize = Math.Max(1, CInt((iTSFinalYear - iTSFirstYear) / cCore.N_MONTHS))
                    Case Else
                        Debug.Assert(False)
                End Select

                For i As Integer = iTSFirstYear To iTSFinalYear Step iStepSize
                    lstrAxis.Add(i.ToString)
                Next
            End If

            astrLabels = lstrAxis.ToArray

        End Sub

        Protected Overrides Function GetShapeTitle() As String
            Dim sb As New StringBuilder()
            Dim fmt As New cTimeSeriesTypeFormatter()

            sb.AppendLine(MyBase.GetShapeTitle())
            sb.Append(fmt.GetDescriptor(DirectCast(Me.Shape, cTimeSeries).TimeSeriesType))

            Return sb.ToString()
        End Function

    End Class

End Namespace
