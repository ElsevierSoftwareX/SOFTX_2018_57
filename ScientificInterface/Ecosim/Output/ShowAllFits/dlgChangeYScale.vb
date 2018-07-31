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
Imports EwEUtils.Utilities

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Dialog class, allows users to configure the Y-scale in the Ecosim
    ''' Show All Fits interface.
    ''' </summary>
    ''' =======================================================================
    Public Class dlgChangeYScale

        Private m_lplots As List(Of cShowAllFitsPlotData)

        Public Sub New(ByVal lplots As List(Of cShowAllFitsPlotData))

            Me.InitializeComponent()
            Me.m_lplots = lplots

        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Dim plot As cShowAllFitsPlotData = Nothing
            Dim ts As cTimeSeries = Nothing
            Dim desc As New cShapeDataFormatter()

            m_lbAllPlots.Items.Clear()

            For i As Integer = 0 To Me.m_lplots.Count - 1
                plot = Me.m_lplots(i)
                ts = plot.TimeSeries
                m_lbAllPlots.Items.Add(desc.GetDescriptor(ts, eDescriptorTypes.Name))
            Next

            If m_lbAllPlots.Items.Count > 0 Then
                m_lbAllPlots.SelectedIndex = 0
            End If
        End Sub

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnOK.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnCancel.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub OnReset(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnReset.Click
            For Each plot As cShowAllFitsPlotData In Me.m_lplots
                plot.YMax = plot.YMaxDefault
            Next
        End Sub

        Private Sub lbAllPlots_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_lbAllPlots.SelectedIndexChanged

            Me.m_nudYScale.Enabled = False
            Me.m_txbSelPlotName.Text = ""
            Me.m_nudYScale.Value = 0

            If m_lbAllPlots.SelectedIndex >= 0 Then
                Dim plot As cShowAllFitsPlotData = Me.m_lplots(m_lbAllPlots.SelectedIndex)

                If Single.IsNaN(plot.YMax) Then
                    Me.m_txbSelPlotName.Text = "<plot invalid>"
                Else
                    Me.m_txbSelPlotName.Text = plot.TimeSeries.Name
                    Me.m_nudYScale.Enabled = True

                    Me.m_nudYScale.Minimum = 0
                    Me.m_nudYScale.Value = 0
                    Me.m_nudYScale.Maximum = CDec(plot.YMaxDefault * 10)
                    Me.m_nudYScale.Value = CDec(plot.YMax)
                End If
            End If
        End Sub

        Private Sub txbYScale_Validated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_nudYScale.Validated
            Try
                Dim plot As cShowAllFitsPlotData = Me.m_lplots(m_lbAllPlots.SelectedIndex)
                plot.YMax = CSng(Me.m_nudYScale.Value)
            Catch ex As Exception

            End Try
        End Sub

    End Class

End Namespace
