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

#End Region ' Imports

Namespace Ecosim

    ''' <summary>
    ''' Grid for applying egg production shapes to multi-stanza configurations.
    ''' </summary>
    <CLSCompliant(False)>
    Public Class gridApplyEP
        Inherits EwEGrid

#Region " Private vars "

        Private m_EPManager As cEggProductionShapeManager = Nothing
        Private m_astrShapes() As String = Nothing
        Private m_ceCellClick As New BehaviorModels.CustomEvents

        Friend Enum eColumnTypes As Integer
            Index = 0
            Name
            Shape
        End Enum

#End Region ' Private vars

        Public Sub New()

            MyBase.New()

        End Sub

        Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
                If (value IsNot Nothing) Then
                    Me.m_EPManager = value.Core.EggProdShapeManager
                End If
                MyBase.UIContext = value
            End Set
        End Property

#Region " Public interfaces "

        Public Sub ResetData()

            Dim cnt As Integer = Me.RowsCount
            If cnt > 1 Then
                Me.Rows.RemoveRange(1, cnt - 1)
            End If
            Me.FillData()

        End Sub

        Public Function GetEPShapeNames() As String()

            Dim astrShapeNames As New List(Of String)

            ' Only when properly initialized
            If (Me.m_EPManager IsNot Nothing) Then

                ' Add empty string as first item
                ' JS26Jan08: SourceGrid will refuse to cancel a combo edit operation when the text box part is empty.
                '            By providing an 'empty' value of " " (instead of "") the text box is never empty, and 
                '            sourcegrid will thus allow cancellation of edit operations on an empty value. Sheesh...
                '            There must be a better way to do this!
                astrShapeNames.Add(" ")
                If m_EPManager.Count > 0 Then

                    For Each shapeFunc As cForcingFunction In m_EPManager
                        Dim tmpStr As String = String.Format(SharedResources.GENERIC_LABEL_INDEXED, (shapeFunc.ID + 1), shapeFunc.Name)
                        astrShapeNames.Add(tmpStr)
                    Next

                End If
            End If

            Return astrShapeNames.ToArray()

        End Function

        Public Sub SelectShapeName(ByVal strName As String)

            Dim r As Range = Me.Selection.GetRange()
            Dim iRow As Integer = 1
            Dim iShape As Integer = -1
            Dim pair As cGroupShapePair = Nothing
            Dim iID As Integer = 0

            If r.IsEmpty Then Return
            If (r.ContainsColumn(eColumnTypes.Shape) = False) Then Return

            ' Resolve shape index
            For iShapeTest As Integer = 0 To Me.m_astrShapes.Length - 1
                If m_astrShapes(iShapeTest) = strName Then iShape = iShapeTest - 1 : Exit For
            Next

            Try

                For iRow = r.Start.Row To r.End.Row
                    If r.Contains(New Position(iRow, eColumnTypes.Shape)) Then
                        pair = DirectCast(Me(iRow, eColumnTypes.Shape).Tag, cGroupShapePair)
                        If pair IsNot Nothing Then
                            If (iShape = -1) Then
                                pair.ShapeID = cCore.NULL_VALUE
                            Else
                                iID = Me.m_EPManager(iShape).ID
                                If pair.ShapeID <> iID Then
                                    pair.ShapeID = iID
                                End If
                            End If
                            Me(iRow, eColumnTypes.Shape).Value = strName
                        End If
                    End If
                Next iRow

            Catch ex As Exception

            End Try

        End Sub

#End Region ' Public interfaces

#Region " Grid overrides "

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_STANZAGROUP_NAME)
            Me(0, eColumnTypes.Shape) = New EwEColumnHeaderCell(SharedResources.HEADER_SHAPE)

        End Sub

        Protected Overrides Sub FillData()

            If (Me.m_EPManager Is Nothing) Then Return

            Dim cmb As Cells.Real.ComboBox = Nothing
            Dim pair As cGroupShapePair = Nothing
            Dim sg As cStanzaGroup = Nothing
            Dim iRow As Integer = 1

            m_astrShapes = GetEPShapeNames()

            For Each pair In m_EPManager.GroupShapeList
                Me.Rows.Insert(iRow)
                sg = Me.Core.StanzaGroups(pair.iStanzaGroup)

                Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iRow))
                Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, sg, eVarNameFlags.Name)

                ' Combo box with strings, no text box
                If pair.ShapeID < 0 Then
                    cmb = New Cells.Real.ComboBox(m_astrShapes(0), GetType(String), m_astrShapes, True)
                Else
                    ' JS bug 293: shape names are 1-based
                    cmb = New Cells.Real.ComboBox(m_astrShapes(pair.ShapeID + 1), GetType(String), m_astrShapes, True)
                End If
                cmb.DataModel.AllowStringConversion = False
                cmb.EditableMode = EditableMode.SingleClick

                Me(iRow, eColumnTypes.Shape) = cmb
                Me(iRow, eColumnTypes.Shape).Tag = pair
                Me(iRow, eColumnTypes.Shape).Behaviors.Add(Me.EwEEditHandler)

                iRow += 1
            Next

        End Sub

        Protected Overrides Sub FinishStyle()

            MyBase.FinishStyle()
            Me.FixedColumns = 1
            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Function OnCellEdited(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

            Dim iRow As Integer = p.Row
            Dim iCol As Integer = p.Column
            Dim pair As cGroupShapePair = Nothing
            Dim strValue As String = ""
            Dim iID As Integer = cCore.NULL_VALUE

            ' Ignore header row
            If (iRow = 0) Then Return False
            ' Ignore non-combo changes
            If (iCol <> eColumnTypes.Shape) Then Return False

            ' Get pair
            If Me(iRow, eColumnTypes.Shape).Tag IsNot Nothing Then
                If TypeOf Me(iRow, eColumnTypes.Shape).Tag Is cGroupShapePair Then
                    pair = CType(Me(iRow, eColumnTypes.Shape).Tag, cGroupShapePair)
                End If
            End If

            ' Hahaha
            If pair Is Nothing Then Return False

            ' Get value
            strValue = CStr(cell.GetValue(p))
            ' Assume the worst...
            iID = cCore.NULL_VALUE

            ' Cell value not empty?
            If Not String.IsNullOrEmpty(strValue) Then
                ' #Yes: find shape
                For Each shapeFunc As cForcingFunction In m_EPManager
                    Dim tmpStr As String = String.Format(SharedResources.GENERIC_LABEL_INDEXED, (shapeFunc.ID + 1), shapeFunc.Name)
                    If tmpStr = strValue Then
                        ' Shape manager needs position in list, not shape index!
                        iID = shapeFunc.ID
                    End If
                Next
            End If

            ' Need to change?
            If (pair.ShapeID <> iID) Then
                ' Update
                pair.ShapeID = iID
                Me(iRow, eColumnTypes.Shape).Value = strValue
            End If

            Return True

        End Function

#End Region ' Grid overrides

    End Class

End Namespace


