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
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Dialog class, allows users to configure which plots to show in the Ecosim
    ''' Show All Fits interface.
    ''' </summary>
    ''' =======================================================================
    Public Class dlgSelectAllFitsPlots

        Private m_lplots As cShowAllFitsPlotData()

        Public Sub New(ByVal lplots() As cShowAllFitsPlotData)

            Me.InitializeComponent()
            Me.m_lplots = lplots

        End Sub

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            Dim plot As cShowAllFitsPlotData = Nothing
            Dim ts As cTimeSeries = Nothing

            clbAllPlots.Items.Clear()

            For i As Integer = 0 To Me.m_lplots.Count - 1
                plot = Me.m_lplots(i)
                clbAllPlots.Items.Add(plot, plot.Selected)
            Next

        End Sub

        Private Sub btnCheckAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCheckAll.Click

            For i As Integer = 0 To clbAllPlots.Items.Count - 1
                clbAllPlots.SetItemChecked(i, True)
            Next

        End Sub

        Private Sub btnUnCheckAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUnCheckAll.Click

            For i As Integer = 0 To clbAllPlots.Items.Count - 1
                clbAllPlots.SetItemChecked(i, False)
            Next

        End Sub

        Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

            Dim plot As cShowAllFitsPlotData = Nothing

            For i As Integer = 0 To Me.m_lplots.Count - 1
                plot = Me.m_lplots(i)
                plot.Selected = Me.clbAllPlots.GetItemChecked(i)
            Next

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click

            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()

        End Sub

        Private Sub clbAllPlots_Format(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles clbAllPlots.Format

            If (e.ListItem IsNot Nothing) Then
                Dim plot As cShowAllFitsPlotData = DirectCast(e.ListItem, cShowAllFitsPlotData)
                Dim ts As cTimeSeries = plot.TimeSeries
                Debug.Assert(ts IsNot Nothing)
                e.Value = String.Format(SharedResources.GENERIC_LABEL_INDEXED, ts.Index, ts.Name)
            End If
        End Sub
    End Class

End Namespace
