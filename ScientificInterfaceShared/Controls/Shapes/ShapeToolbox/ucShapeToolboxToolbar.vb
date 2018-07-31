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

#End Region ' Imports

Namespace Controls

    Public Class ucShapeToolboxToolbar

#Region " Private vars "

        Private m_handler As cShapeGUIHandler = Nothing

#End Region ' Private vars

#Region " Constructors "

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint, True)
        End Sub

#End Region ' Constructors

#Region " Properties "

        Public Property Handler() As cShapeGUIHandler
            Get
                Return Me.m_handler
            End Get
            Set(ByVal handler As cShapeGUIHandler)
                Me.m_handler = handler
                Me.UpdateControls()
            End Set
        End Property

#End Region ' Properties

#Region " Public interfaces "

        Public Overrides Sub Refresh()
            MyBase.Refresh()
            Me.UpdateControls()
        End Sub

#End Region ' Public interfaces

#Region " Helper methods "

        Protected Overrides Sub OnVisibleChanged(ByVal e As System.EventArgs)
            Me.UpdateControls()
        End Sub

        Private Sub UpdateControls()

            If (Me.m_handler Is Nothing) Then Return

            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Add, Me.m_tsbAdd)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Weight, Me.m_tsbWeight)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Duplicate, Me.m_tsbDuplicate)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Load, Me.m_tsbLoad)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Import, Me.m_tsbImport)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Export, Me.m_tsbExport)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.Remove, Me.m_tsbRemove)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.ResetAll, Me.m_tsbResetAll)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.SetToZero, Me.m_tsbSetTo0)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.SetToEcopathBaseline, Me.m_tsbSetToBaseline)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.SetToValue, Me.m_tsbSetToValue)
            Me.UpdateCommand(New cShapeGUIHandler.eShapeCommandTypes() {cShapeGUIHandler.eShapeCommandTypes.FilterList, cShapeGUIHandler.eShapeCommandTypes.FilterName}, Me.m_tslFilter)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.FilterList, Me.m_tscmbFilter)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.FilterName, Me.m_tstbxFilterName)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.FilterName, Me.m_tsbnFilterCase)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.ShowExtraData, Me.m_tsbnShowExtraData)
            Me.UpdateCommand(cShapeGUIHandler.eShapeCommandTypes.DiscardExtraData, Me.m_tsbnDiscardExtraData)

            Dim astrFilters As String() = Me.m_handler.Filters()
            Me.m_tscmbFilter.Items.Clear()
            If (astrFilters IsNot Nothing) Then
                For i As Integer = 0 To astrFilters.Length - 1
                    Me.m_tscmbFilter.Items.Add(astrFilters(i))
                Next
                Try
                    Me.m_tscmbFilter.SelectedIndex = Me.m_handler.FilterIndex
                Catch ex As Exception
                    ' Hmm
                End Try
            End If

        End Sub

        Private Sub UpdateCommand(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes, ByVal tsi As ToolStripItem)
            If (Me.m_handler Is Nothing) Then Return
            If Me.m_handler.SupportCommand(cmd) Then
                tsi.Visible = True
                tsi.Enabled = (Me.m_handler.EnableCommand(cmd))
            Else
                tsi.Visible = False
            End If
        End Sub

        Private Sub UpdateCommand(ByVal cmds As cShapeGUIHandler.eShapeCommandTypes(), ByVal tsi As ToolStripItem)
            If (Me.m_handler Is Nothing) Then Return
            Dim bSupport As Boolean = False
            For Each cmd As cShapeGUIHandler.eShapeCommandTypes In cmds
                If Me.m_handler.SupportCommand(cmd) Then
                    tsi.Enabled = (Me.m_handler.EnableCommand(cmd))
                    bSupport = True
                End If
            Next
            tsi.Visible = bSupport

        End Sub

#End Region ' Helper methods

#Region " Event handlers "

        Private Sub tsbAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbAdd.Click
            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Add)
        End Sub

        Private Sub tsbDuplicate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbDuplicate.Click
            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Duplicate)
        End Sub

        Private Sub tsbRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsbRemove.Click
            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Remove)
        End Sub

        Private Sub tsbWeight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbWeight.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Weight)

        End Sub

        Private Sub tsbLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbLoad.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Load)

        End Sub

        Private Sub tsbImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbImport.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Import)

        End Sub

        Private Sub tsbExport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbExport.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Export)

        End Sub

        Private Sub m_tsbResetAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tsbResetAll.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ResetAll)

        End Sub

        Private Sub tsbSetTo0_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tsbSetTo0.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Reset, Me.m_handler.SelectedShapes, 0.0!)

        End Sub

        Private Sub tsbSetToValue_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tsbSetToValue.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.SetToValue)

        End Sub

        Private Sub OnSteToBaseline(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tsbSetToBaseline.Click

            If (Me.m_handler Is Nothing) Then Return
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.SetToEcopathBaseline)

        End Sub

        Private Sub OnFilterSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tscmbFilter.SelectedIndexChanged
            If (Me.m_handler Is Nothing) Then Return
            Try
                Me.m_handler.FilterIndex = Me.m_tscmbFilter.SelectedIndex
            Catch ex As Exception
                ' Hmm
            End Try
        End Sub

        Private Sub OnShowAllData(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbnShowExtraData.Click
            If (Me.m_handler Is Nothing) Then Return
            Try
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ShowExtraData, Nothing, Me.m_tsbnShowExtraData.Checked)
            Catch ex As Exception
                ' Hmm
            End Try
        End Sub

        Private Sub m_tsbnDiscardExtraData_Click(sender As System.Object, e As System.EventArgs) Handles m_tsbnDiscardExtraData.Click
            If (Me.m_handler Is Nothing) Then Return
            Try
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.DiscardExtraData)
            Catch ex As Exception
                ' Hmm
            End Try

        End Sub

        Private Sub OnToggleCase(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnFilterCase.Click
            Try
                Me.Handler.IsTextFilterCaseSensitive = Me.m_tsbnFilterCase.Checked
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnFilterTextChanged(sender As Object, e As System.EventArgs) _
            Handles m_tstbxFilterName.TextChanged
            Try
                Me.Handler.TextFilter = Me.m_tstbxFilterName.Text
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Event handlers

    End Class

End Namespace
