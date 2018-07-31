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

#End Region ' Imports 

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer editor that supports manual modifications of layers where cells
    ''' have two values: set or cleared.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerEditorTwoState
        Inherits cLayerEditorRaster

        Public Sub New()
            Me.New(Nothing, True)
        End Sub

        Public Sub New(ByVal typeGUI As Type, bAutoToggleCellValue As Boolean)
            MyBase.New(typeGUI)
            Me.AutoToggleCellValue = bAutoToggleCellValue
        End Sub

        Protected Property AutoToggleCellValue As Boolean = True

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cLayerEditor.Initialize"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Initialize(ByVal uic As cUIContext, ByVal layer As cDisplayLayer)
            MyBase.Initialize(uic, layer)
            Dim rl As cDisplayLayerRaster = DirectCast(layer, cDisplayLayerRaster)
            Me.CellValueMax = CSng(Math.Max(rl.ValueSet, rl.ValueClear))
            Me.CellValueMin = CSng(Math.Min(rl.ValueSet, rl.ValueClear))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cLayerEditor.StartEdit"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub StartEdit(ByVal e As MouseEventArgs, map As ucMap)

            If (Not Me.IsEditable) Then Return

            MyBase.StartEdit(e, map)

            Dim ptClick As Point = map.GetCellIndex(e.Location)

            If (Me.AutoToggleCellValue) Then

                ' Clicked on an empty cell?
                If Decimal.Equals(CSng(Layer.Value(ptClick.Y, ptClick.X)), CSng(Layer.ValueClear)) Then
                    ' #Yes: start setting values
                    Me.CellValue = CSng(Layer.ValueSet)
                Else
                    ' #No: start clearing values
                    Me.CellValue = CSng(Layer.ValueClear)
                End If

                If Me.GUI IsNot Nothing Then
                    ' Trigger GUI to update to the changes
                    Me.GUI.UpdateContent(Me)
                End If

            End If
        End Sub

    End Class

End Namespace