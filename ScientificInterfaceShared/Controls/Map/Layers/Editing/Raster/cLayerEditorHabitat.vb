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
    ''' Layer editor that supports manual modifications of habitat layers. Setting
    ''' a cell value in one habitat will clear the cell values in another.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerEditorHabitat
        Inherits cLayerEditorRange

        Public Sub New()
            MyBase.New(GetType(ucLayerEditorHabitat))
            Me.CellValue = 1.0!
        End Sub

        Public Property UseHabitatAreaCorrection As Boolean = False

        Protected Overrides Sub SetCellValue(ptSet As System.Drawing.Point,
                                             value As Object,
                                             e As System.Windows.Forms.MouseEventArgs,
                                             ptClick As System.Drawing.Point)

            If (Me.UIContext Is Nothing) Then Return

            Dim core As cCore = Me.UIContext.Core
            Dim bm As cEcospaceBasemap = core.EcospaceBasemap
            Dim iHab As Integer = CInt(Me.Layer.Data.Index)
            Dim sValue As Single = Math.Min(Math.Max(0.0!, CSng(value)), 1.0!)
            Dim sTotal As Single = 0

            If (Me.UseHabitatAreaCorrection) Then

                For i As Integer = 1 To core.nHabitats - 1
                    If (i = iHab) Then
                        sTotal += sValue
                    Else
                        sTotal += CSng(bm.LayerHabitat(i).Cell(ptSet.Y, ptSet.X))
                    End If
                Next

                If (sTotal > 1) Then
                    Dim sRemainer As Single = (1 - sValue)
                    For i As Integer = 1 To core.nHabitats - 1
                        If (i <> iHab) Then
                            ' Scale down other habitat capacities
                            bm.LayerHabitat(i).Cell(ptSet.Y, ptSet.X) = CSng(bm.LayerHabitat(i).Cell(ptSet.Y, ptSet.X)) * sRemainer
                        End If
                    Next
                End If

            End If

            MyBase.SetCellValue(ptSet, sValue, e, ptClick)

        End Sub

    End Class

End Namespace