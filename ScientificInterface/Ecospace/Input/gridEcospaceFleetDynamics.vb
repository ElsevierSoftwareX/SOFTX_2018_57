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
Imports SourceGrid2
Imports ScientificInterfaceShared.Style.cStyleGuide

#End Region

Namespace Ecospace

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Grid to configure Ecospace habitat fishing limitations.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)>
    Public Class gridEcospaceHabitatDyncamis
        Inherits EwEGrid

        Private m_bInUpdate As Boolean = False
        Private WithEvents m_bpEffort As cBooleanProperty = Nothing

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            EffPower
            SEmult
        End Enum

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = False
        End Sub

#Region " Overrides "

        Public Overrides Property UIContext As cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As cUIContext)
                If (Me.UIContext IsNot Nothing) Then
                    Me.m_bpEffort = Nothing
                End If

                If (value IsNot Nothing) Then
                    Dim ecospaceModelParams As cEcospaceModelParameters = value.Core.EcospaceModelParameters()
                    Dim propMan As cPropertyManager = value.PropertyManager
                    Me.m_bpEffort = DirectCast(propMan.GetProperty(ecospaceModelParams, eVarNameFlags.PredictEffort), cBooleanProperty)
                End If

                MyBase.UIContext = value

            End Set
        End Property

        Protected Overrides Sub InitStyle()

            'Call base class InitStyle method. 
            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            'Set header cells #(0,0)
            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
            Me(0, eColumnTypes.EffPower) = New EwEColumnHeaderCell(SharedResources.HEADER_EFFPOWER)
            Me(0, eColumnTypes.SEmult) = New EwEColumnHeaderCell(SharedResources.HEADER_TOTEFFMULTI)

        End Sub

        Protected Overrides Sub FillData()

            Me.RowsCount = 1

            For i As Integer = 1 To Me.Core.nFleets

                Dim fleet As cEcospaceFleetInput = Me.Core.EcospaceFleetInputs(i)
                Dim iRow As Integer = Me.AddRow()

                Me(iRow, eColumnTypes.Index) = New PropertyRowHeaderCell(Me.PropertyManager, fleet, eVarNameFlags.Index)
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, fleet, eVarNameFlags.Name)
                Me(iRow, eColumnTypes.EffPower) = New PropertyCell(Me.PropertyManager, fleet, eVarNameFlags.EffectivePower)
                Me(iRow, eColumnTypes.SEmult) = New PropertyCell(Me.PropertyManager, fleet, eVarNameFlags.SEmult)

            Next

            Me.UpdateEnabledState()

        End Sub

        Public Overrides ReadOnly Property CoreComponents() As eCoreComponentType()
            Get
                Return New eCoreComponentType() {eCoreComponentType.EcoPath, eCoreComponentType.EcoSpace}
            End Get
        End Property

        Private Sub UpdateEnabledState()

            Dim cell As EwECellBase = Nothing
            Dim bEnable As Boolean = (CBool(Me.m_bpEffort.GetValue()) = True)

            For iRow As Integer = 1 To Me.RowsCount - 1
                cell = DirectCast(Me(iRow, eColumnTypes.EffPower), EwECellBase)
                If bEnable Then
                    cell.Style = cell.Style And Not eStyleFlags.NotEditable
                Else
                    cell.Style = cell.Style Or eStyleFlags.NotEditable
                End If
                cell = DirectCast(Me(iRow, eColumnTypes.SEmult), EwECellBase)
                If bEnable Then
                    cell.Style = cell.Style And Not eStyleFlags.NotEditable
                Else
                    cell.Style = cell.Style Or eStyleFlags.NotEditable
                End If
            Next
            Me.InvalidateCells()

        End Sub

#End Region ' Overrides

#Region " Event handlers "

        Private Sub m_bpEffort_PropertyChanged(prop As cProperty, changeFlags As cProperty.eChangeFlags) _
            Handles m_bpEffort.PropertyChanged

            Try
                BeginInvoke(New MethodInvoker(AddressOf UpdateEnabledState))
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Event handlers

    End Class

End Namespace

