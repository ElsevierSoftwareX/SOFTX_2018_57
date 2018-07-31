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

Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SourceGrid2

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class ucMediationAssignmentsGrid
        Inherits EwEGrid.EwEGrid

        Private m_bLandings As Boolean = True

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create the grid
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
            MyBase.New()
        End Sub

#Region " Public interfaces "

        Public Property IsLandings() As Boolean
            Get
                Return Me.m_bLandings
            End Get
            Set(ByVal value As Boolean)
                Me.m_bLandings = value
                Me.RefreshContent()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="sWeight"></param>
        ''' -------------------------------------------------------------------
        Public Event OnWeightChanged(ByVal obj As cCoreInputOutputBase, ByVal sWeight As Single)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj">Group or fleet</param>
        ''' <param name="sWeight">Fleet if <paramref name="obj"/> refers to a group.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Add(ByVal obj As cCoreInputOutputBase, _
                            ByVal objSec As cCoreInputOutputBase, _
                            ByVal sWeight As Single) As Boolean
            If (Me.FindRow(obj, objSec) <> -1) Then Return False

            ' =====================
            ' Validate data
            ' =====================

            ' Is landings?
            If (Me.m_bLandings) Then
                ' #Yes: need both fleet and group
                If (obj Is Nothing) Or (objSec Is Nothing) Then Return False
            Else
                ' #No: cannot add mixed entries
                Dim t As Type = obj.GetType
                For i As Integer = 1 To Me.RowsCount - 1
                    If Not t.Equals(Me.RowItemPrim(i).GetType) Then
                        Dim msg As New cMessage(My.Resources.PROMPT_MEDIATION_CANNOTMIX, eMessageType.Any, EwEUtils.Core.eCoreComponentType.EcoSim, eMessageImportance.Warning)
                        Me.Core.Messages.SendMessage(msg)
                        Return False
                    End If
                Next
            End If

            Dim fmt As New cCoreInterfaceFormatter()
            Dim ewec As EwECellBase = Nothing
            Dim iCol As Integer = 0
            Dim iRow As Integer = Me.AddRow()

            ewec = New EwERowHeaderCell(fmt.GetDescriptor(obj))
            Me(iRow, iCol) = ewec
            iCol += 1

            ' Is landings?
            If Me.m_bLandings Then
                ewec = New EwERowHeaderCell(fmt.GetDescriptor(objSec))
                Me(iRow, iCol) = ewec
                iCol += 1
            End If

            Me(iRow, iCol) = New Cells.Real.Cell(sWeight, GetType(Single))
            Me(iRow, iCol).Behaviors.Add(Me.EwEEditHandler)

            Debug.Assert(iCol = Me.WeightCol)

            Me.RowItemPrim(iRow) = obj
            Me.RowItemSec(iRow) = objSec

            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Remove(ByVal obj As cCoreInputOutputBase, _
                               ByVal objSec As cCoreInputOutputBase) As Boolean
            Dim iRow As Integer = Me.FindRow(obj, objSec)
            If (iRow = -1) Then Return False
            Me.Rows.Remove(iRow)
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Find(ByVal obj As cCoreInputOutputBase, _
                             ByVal objSec As cCoreInputOutputBase) As Boolean
            Return (Me.FindRow(obj, objSec) > -1)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function SelectedItems() As cCoreInputOutputBase()()
            Dim lItems As New List(Of cCoreInputOutputBase())
            For Each ri As RowInfo In Me.Selection.SelectedRows
                lItems.Add(New cCoreInputOutputBase() {Me.RowItemPrim(ri.Index), Me.RowItemSec(ri.Index)})
            Next
            Return lItems.ToArray
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Hack and slash results to feed to graph control, ugh
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function Data() As ucMediationAssignments.cBioPercentData

            Dim d As New ucMediationAssignments.cBioPercentData()
            Dim lGroups As New List(Of cMediatingGroup)
            Dim lFleets As New List(Of cMediatingFleet)

            For iRow As Integer = 1 To Me.RowsCount - 1

                Try
                    Dim objPrim As cCoreInputOutputBase = Me.RowItemPrim(iRow)
                    Dim objSec As cCoreInputOutputBase = Me.RowItemSec(iRow)
                    Dim sWeight As Single = CSng(Me(iRow, Me.WeightCol).Value)

                    If Me.m_bLandings Then
                        If objSec IsNot Nothing Then
                            lGroups.Add(New cLandingsMediatingGroup(objPrim.Index, objSec.Index, sWeight))
                        Else
                            lGroups.Add(New cLandingsMediatingGroup(objPrim.Index, 0, sWeight))
                        End If
                    Else
                        If TypeOf objPrim Is cEcoPathGroupInput Then
                            lGroups.Add(New cMediatingGroup(objPrim.Index, sWeight))
                        ElseIf TypeOf objPrim Is cEcopathFleetInput Then
                            lFleets.Add(New cMediatingFleet(objPrim.Index, sWeight))
                        End If
                    End If
                Catch ex As Exception

                End Try
            Next

            d.Groups = lGroups.ToArray()
            d.Fleets = lFleets.ToArray()
            Return d

        End Function

#End Region ' Public interfaces

#Region " Internals "

        Private Function WeightCol() As Integer
            Return If(Me.m_bLandings, 2, 1)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function FindRow(ByVal obj As cCoreInputOutputBase, _
                                 ByVal objSec As cCoreInputOutputBase) As Integer
            Dim objTest As cCoreInputOutputBase = Nothing
            Dim objTestSec As cCoreInputOutputBase = Nothing
            For iRow As Integer = 1 To Me.RowsCount - 1
                objTest = Me.RowItemPrim(iRow)
                objTestSec = Me.RowItemSec(iRow)
                If (ReferenceEquals(obj, objTest) And ReferenceEquals(objSec, objTestSec)) Then
                    Return iRow
                End If
            Next
            Return -1
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' -------------------------------------------------------------------
        Private Property RowItemPrim(ByVal iRow As Integer) As cCoreInputOutputBase
            Get
                If (iRow <= 0) Then Return Nothing
                Return DirectCast(Me(iRow, 0).Tag, cCoreInputOutputBase)
            End Get
            Set(ByVal value As cCoreInputOutputBase)
                Me(iRow, 0).Tag = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' -------------------------------------------------------------------
        Private Property RowItemSec(ByVal iRow As Integer) As cCoreInputOutputBase
            Get
                If (iRow <= 0) Then Return Nothing
                Return DirectCast(Me(iRow, 1).Tag, cCoreInputOutputBase)
            End Get
            Set(ByVal value As cCoreInputOutputBase)
                Me(iRow, 1).Tag = value
            End Set
        End Property

        'Private Sub ucDefBioPercentGrid_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        '    If Me.ColumnsCount = [Enum].GetValues(GetType(eColumnTypes)).Length Then
        '        Me.Columns(eColumnTypes.NamePrim).Width = Math.Max(150, Me.ClientRectangle.Width - 150)
        '    End If
        'End Sub

#End Region ' Internals "

#Region " Grid overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Redim columns
            If Me.m_bLandings Then
                Me.Redim(1, 3)
                Me(0, 0) = New EwEColumnHeaderCell(My.Resources.HEADER_GROUP)
                Me(0, 1) = New EwEColumnHeaderCell(My.Resources.HEADER_FLEET)
                Me(0, 2) = New EwEColumnHeaderCell(My.Resources.HEADER_RELATIVEWEIGHT)
            Else
                Me.Redim(1, 2)
                Me(0, 0) = New EwEColumnHeaderCell(My.Resources.HEADER_NAME)
                Me(0, 1) = New EwEColumnHeaderCell(My.Resources.HEADER_RELATIVEWEIGHT)
            End If


        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub FillData()
            ' HAH!
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            'Me.Columns(eColumnTypes.Weight).Width = 100
            'Me.Columns(eColumnTypes.NamePrim).Width = 150
            'If Me.IsLandings Then
            '    Me.Columns(eColumnTypes.NamePrim).Width = 150
            'Else
            '    Me.Columns(eColumnTypes.NamePrim).Visible = False
            '    Me.Columns(eColumnTypes.NamePrim).Width = 0
            'End If
            Me.FixedColumns = 1
            Me.FixedColumnWidths = False
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <returns>
        ''' True if the value change is allowed, False to block the value change.
        ''' </returns>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function OnCellValueChanged(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

            If (p.Column = Me.WeightCol()) Then
                ' Parse value using UI number settings
                RaiseEvent OnWeightChanged(Me.RowItemPrim(p.Row), Single.Parse(CStr(cell.GetValue(p))))
            End If
            Return True

        End Function

#End Region ' Grid overrides

    End Class

End Namespace
