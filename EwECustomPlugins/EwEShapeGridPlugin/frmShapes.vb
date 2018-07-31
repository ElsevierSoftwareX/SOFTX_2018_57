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

Option Strict On
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls

Friend Class frmShapes

    Private m_grid As gridShapeBase = Nothing
    Private m_cmdTimeSeries As cCommand = Nothing

    Public Sub New(ByVal typeGrid As Type)
        MyBase.New()

        Me.InitializeComponent()

        Me.m_grid = DirectCast(Activator.CreateInstance(typeGrid), gridShapeBase)
        Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plGrid.Controls.Add(Me.m_grid)
        Me.Grid = Me.m_grid

        Me.CoreExecutionState = EwEUtils.Core.eCoreExecutionState.EcosimLoaded

    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If Me.UIContext Is Nothing Then Return

        If (TypeOf Me.Grid Is gridTimeSeries) Then

            Me.m_cmdTimeSeries = Me.CommandHandler.GetCommand("LoadTimeSeries")
            If (Me.m_cmdTimeSeries IsNot Nothing) Then

                Me.m_cmdTimeSeries.AddControl(Me.m_tsbnTimeSeries)
                AddHandler Me.m_cmdTimeSeries.OnPostInvoke, AddressOf OnTimeSeriesLoaded

                ' Once hooked up, try to get TS if not here yet
                If Not Me.UIContext.Core.HasTimeSeries Then
                    Me.m_cmdTimeSeries.Invoke()
                End If
            End If
        End If

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        If (Me.m_cmdTimeSeries IsNot Nothing) Then
            Me.m_cmdTimeSeries.RemoveControl(Me.m_tsbnTimeSeries)
            RemoveHandler Me.m_cmdTimeSeries.OnPostInvoke, AddressOf OnTimeSeriesLoaded
            Me.m_cmdTimeSeries = Nothing
        End If

        Me.m_plGrid.Controls.Remove(Me.m_grid)
        Me.UIContext = Nothing
        Me.m_grid.Dispose()
        Me.m_grid = Nothing
        MyBase.OnFormClosed(e)

    End Sub

    Private Sub OnTimeSeriesLoaded(ByVal cmd As cCommand)
        Me.m_grid.RefreshContent()
    End Sub

    Private Sub OnViewSeasonal(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbnSeasonal.Click
        Me.m_grid.IsSeasonal = True
        Me.UpdateControls()
    End Sub

    Private Sub OnViewLongTerm(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsbnLongTerm.Click
        Me.m_grid.IsSeasonal = False
        Me.UpdateControls()
    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Dim handler As cShapeGUIHandler = DirectCast(Me.Grid, gridShapeBase).Handler
        Dim bSupportSeasonal As Boolean = handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.Seasonal)
        Dim bIsTimeSeries As Boolean = handler.IsTimeSeries
        Dim bIsForcing As Boolean = handler.IsForcing

        Me.m_tsbnSeasonal.Checked = Me.m_grid.IsSeasonal
        Me.m_tsbnLongTerm.Checked = Not Me.m_grid.IsSeasonal

        Me.m_tsbnTimeSeries.Visible = bIsTimeSeries
        Me.m_tsbnSeasonal.Visible = bSupportSeasonal
        Me.m_tsbnLongTerm.Visible = bSupportSeasonal

        Me.m_tsbnShowAllData.Visible = bIsForcing
        Me.m_tsbnShowAllData.Checked = Me.m_grid.ShowAllData

    End Sub

    Private Sub OnShowAllData(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnShowAllData.Click
        Me.m_grid.ShowAllData = Not m_tsbnShowAllData.Checked
        Me.UpdateControls()
    End Sub

End Class