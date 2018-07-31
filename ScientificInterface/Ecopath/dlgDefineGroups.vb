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

Imports System.Windows.Forms
Imports EwECore
Imports ScientificInterfaceShared.Commands

#End Region

Namespace Ecopath

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Dialog class, implements the user interface to add/remove/reorder groups, 
    ''' and change multi-stanza compositions, in the EwE Scientific Interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class dlgDefineGroups

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new instance of this class.
        ''' </summary>
        ''' <param name="uic">The <see cref="cUIContext">UI context</see> to connect to.</param>
        ''' <param name="group">A group to select, if any.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                              Optional ByVal group As cEcoPathGroupInput = Nothing)

            Me.InitializeComponent()

            Me.m_grid.UIContext = uic
            Me.m_grid.SelectGroup(group)

        End Sub

#End Region ' Constructor

#Region " Event handlers "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.UpdateControls()
        End Sub

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles OK_Button.Click

            ' Try to apply grid changes
            If Me.m_grid.Apply() = False Then
                ' Abort! Abort!
                Return
            End If

            ' Close dialog
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles Cancel_Button.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub OnInsert(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnInsert.Click
            Me.m_grid.InsertRow()
            Me.UpdateControls()
        End Sub

        Private Sub OnMoveUp(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnMoveUp.Click
            Me.m_grid.MoveRowUp()
            Me.UpdateControls()
        End Sub

        Private Sub OnMoveDown(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnMoveDown.Click
            Me.m_grid.MoveRowDown()
            Me.UpdateControls()
        End Sub

        Private Sub OnDelete(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnDelete.Click
            Me.m_grid.ToggleDeleteRows()
            Me.UpdateControls()
        End Sub

        Private Sub OnPreserve(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnKeep.Click
            Me.m_grid.ToggleDeleteRows()
            Me.UpdateControls()
        End Sub

        Private Sub OnGroupSelected() _
            Handles m_grid.OnSelectionChanged
            Me.UpdateControls()
        End Sub

        Private Sub OnColourDefaultAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_bntColorDefault.Click
            Me.m_grid.SetDefaultGroupColors()
        End Sub

        Private Sub OnColourAlternateAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnColorAlternate.Click
            Me.m_grid.SetAlternatingGroupColors()
        End Sub

        Private Sub OnColourRandomAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnRandom.Click
            Me.m_grid.SetRandomGroupColors()
        End Sub

        Private Sub OnColourCustomCurrent(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnColourCustom.Click
            Me.m_grid.SelectCustomColors()
        End Sub

#End Region ' Event handlers 

#Region " Updating "

        Private Sub UpdateControls()

            Dim bHasSelection As Boolean = (Me.m_grid.SelectedRows.Length > 0)

            Me.m_btnMoveUp.Enabled = Me.m_grid.CanMoveRowUp()
            Me.m_btnMoveDown.Enabled = Me.m_grid.CanMoveRowDown()
            Me.m_btnInsert.Enabled = Me.m_grid.CanInsertRow()
            Me.m_btnDelete.Enabled = Me.m_grid.IsGroupRow() And (Not Me.m_grid.IsFlaggedForDeletionRow())
            Me.m_btnKeep.Enabled = Me.m_grid.IsGroupRow() And Me.m_grid.IsFlaggedForDeletionRow()

            Me.m_btnColorAlternate.Enabled = bHasSelection

        End Sub

#End Region ' Updating

        Private Sub CheckBox1_CheckedChanged(sender As System.Object, e As System.EventArgs)

        End Sub
    End Class

End Namespace

