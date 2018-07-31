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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Properties
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)> _
    Public Class gridMortalityPredation
        : Inherits EwEGrid

#Region " Helper classes "

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' A <see cref="cProperty">cProperty</see>-driven cell that reflects the 
        ''' property value by varying the colour intensity of the cell background.
        ''' </summary>
        ''' <remarks>This is a Hack'n'slash solution; no value range testing is 
        ''' performed when calculating the background colour.</remarks>
        ''' ---------------------------------------------------------------------------
        <CLSCompliant(False)> _
        Public Class MortalityGridCell
            : Inherits PropertyCell

            ''' <summary>PB value to monitor.</summary>
            Private m_propPB As cSingleProperty = Nothing

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Constructor.
            ''' </summary>
            ''' <param name="pm">Property manager to obtain data from.</param>
            ''' <param name="source">The source providing the property value.</param>
            ''' <param name="varname">Source variable name.</param>
            ''' <param name="sourceSec">Secundary index.</param>
            ''' -----------------------------------------------------------------------
            Public Sub New(ByVal pm As cPropertyManager, _
                            ByVal source As cCoreInputOutputBase, _
                            ByVal varname As eVarNameFlags, _
                            Optional ByVal sourceSec As cCoreInputOutputBase = Nothing)
                MyBase.New(pm, source, varname, sourceSec)

                Me.PB = DirectCast(pm.GetProperty(source, eVarNameFlags.PBOutput, sourceSec), cSingleProperty)
            End Sub

            Public Overrides Sub Dispose()
                Me.PB = Nothing
                MyBase.Dispose()
            End Sub

            Private Property PB() As cSingleProperty
                Get
                    Return Me.m_propPB
                End Get
                Set(ByVal value As cSingleProperty)

                    If (Me.m_propPB IsNot Nothing) Then
                        RemoveHandler Me.m_propPB.PropertyChanged, AddressOf OnPBChanged
                    End If

                    Me.m_propPB = value

                    If (Me.m_propPB IsNot Nothing) Then
                        AddHandler Me.m_propPB.PropertyChanged, AddressOf OnPBChanged
                        Me.UpdateStyle()
                    End If

                End Set
            End Property

            Private Sub UpdateStyle()

                Dim prop As cProperty = Me.GetProperty()
                Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.Null
                Dim sPB As Single = 0
                Dim sPmort As Single = 0

                If (prop IsNot Nothing) And (Me.m_propPB IsNot Nothing) Then
                    style = prop.GetStyle()
                    sPB = CSng(Me.m_propPB.GetValue())
                    sPmort = CSng(prop.GetValue())
                End If

                If (sPmort > sPB) Then
                    style = style Or cStyleGuide.eStyleFlags.Checked
                End If
                Me.Style = style

            End Sub

            Protected Overridable Sub OnPBChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)
                Me.UpdateStyle()
                Me.Invalidate()
            End Sub

            Protected Overrides Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)
                Me.UpdateStyle()
                MyBase.OnPropertyChanged(prop, changeFlags)
            End Sub

        End Class

#End Region ' Helper classes

        Public Sub New()
            MyBase.new()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreGroupBase = Nothing
            Dim iGroup As Integer

            Me.Redim(core.nLivingGroups + 1, 2)

            Dim rowCnt As Integer = Me.RowsCount

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_PREYPREDATOR)

            Dim columnIndex As Integer = 2

            For iGroup = 1 To core.nLivingGroups
                ' Column displays mixed consumer/producer groups ( PP < 1)
                source = core.EcoPathGroupOutputs(iGroup)
                Me(iGroup, 0) = New EwERowHeaderCell(CStr(iGroup))
                Me(iGroup, 1) = New EwERowHeaderCell(source.Name)

                If source.PP < 1 Then
                    Me.Columns.Insert(columnIndex)
                    Me(0, columnIndex) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
                    columnIndex = columnIndex + 1
                End If
            Next

            Me.FixedColumns = 2
            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreGroupBase = Nothing
            Dim sourceSec As cCoreGroupBase = Nothing
            Dim cell As PropertyCell = Nothing

            For rowIndex As Integer = 1 To core.nLivingGroups
                source = core.EcoPathGroupOutputs(rowIndex)
                Dim columnIndex As Integer = 2
                For groupIndex As Integer = 1 To core.nLivingGroups
                    sourceSec = core.EcoPathGroupOutputs(groupIndex)
                    If sourceSec.PP < 1 Then
                        ' Create cell
                        cell = New MortalityGridCell(Me.PropertyManager, source, eVarNameFlags.PredMort, sourceSec)
                        ' Value cells suppress zeroes to increase legibility of the grid
                        Cell.SuppressZero(-1) = True
                        ' Activate the cell
                        Me(rowIndex, columnIndex) = cell
                        ' Next
                        columnIndex = columnIndex + 1
                    End If
                Next
            Next
        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
