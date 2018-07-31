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
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecospace

    Public Class frmCapacityCalcType

#Region " Private vars "

        Private m_lProps As New List(Of cProperty)
        Private m_bDirty As Boolean = True

#End Region ' Private vars

#Region " Construction "

        Public Sub New()
            MyBase.New()

            Me.InitializeComponent()
            Me.Grid = Me.m_grid

        End Sub

#End Region ' Construction

#Region " Overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_tsbnClear.Image = SharedResources.Editable
            Me.m_tsbnHabitats.Image = SharedResources.Habitat
            Me.m_tsbnEnvResponses.Image = SharedResources.FunctionHS

            For i As Integer = 1 To Me.Core.nLivingGroups
                Dim grp As cEcospaceGroupInput = Me.Core.EcospaceGroupInputs(i)
                Dim prop As cProperty = Me.PropertyManager.GetProperty(grp, eVarNameFlags.EcospaceCapCalType)
                Me.m_lProps.Add(prop)
                AddHandler prop.PropertyChanged, AddressOf OnPropertyChanged
            Next

            Me.m_bDirty = True
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            For Each prop As cProperty In Me.m_lProps
                RemoveHandler prop.PropertyChanged, AddressOf OnPropertyChanged
            Next
            Me.m_lProps.Clear()

            MyBase.OnFormClosed(e)
        End Sub

        Protected Overrides Sub UpdateControls()

            If (Not Me.m_bDirty) Then Return

            Dim iHab As Integer = 0
            Dim iCap As Integer = 0

            For Each prop As cProperty In Me.m_lProps
                Dim val As eEcospaceCapacityCalType = DirectCast(prop.GetValue, eEcospaceCapacityCalType)
                If ((val And eEcospaceCapacityCalType.EnvResponses) = eEcospaceCapacityCalType.EnvResponses) Then iCap += 1
                If ((val And eEcospaceCapacityCalType.Habitat) = eEcospaceCapacityCalType.Habitat) Then iHab += 1
            Next

            Me.m_tsbnHabitats.Checked = (iHab = Me.Core.nLivingGroups)
            Me.m_tsbnEnvResponses.Checked = (iCap = Me.Core.nLivingGroups)

            MyBase.UpdateControls()

        End Sub

#End Region ' Overrides

#Region " Events "

        Private Sub OnUseOnlyInput(sender As Object, e As EventArgs) _
            Handles m_tsbnClear.Click

            Try
                Me.m_grid.SetAllCalcTypes(eEcospaceCapacityCalType.Input, True)
            Catch ex As Exception
                cLog.Write(ex, "frmCapacityCalcType.OnUseOnlyInput")
            End Try

        End Sub

        Private Sub OnToggleHabitatModeForAll(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnHabitats.Click

            Try
                Me.m_grid.SetAllCalcTypes(eEcospaceCapacityCalType.Habitat, Not Me.m_tsbnHabitats.Checked)
            Catch ex As Exception
                cLog.Write(ex, "frmCapacityCalcType.OnToggleHabitatModeForAll")
            End Try

        End Sub

        Private Sub OnToggleEnvResponsesModeForAll(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnEnvResponses.Click

            Try
                Me.m_grid.SetAllCalcTypes(eEcospaceCapacityCalType.EnvResponses, Not Me.m_tsbnEnvResponses.Checked)
            Catch ex As Exception
                cLog.Write(ex, "frmCapacityCalcType.OnToggleEnvResponsesModeForAll")
            End Try

        End Sub

        Private Sub OnPropertyChanged(prop As cProperty, ct As cProperty.eChangeFlags)
            Try
                Me.m_bDirty = True
                Me.BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Events

    End Class

End Namespace
