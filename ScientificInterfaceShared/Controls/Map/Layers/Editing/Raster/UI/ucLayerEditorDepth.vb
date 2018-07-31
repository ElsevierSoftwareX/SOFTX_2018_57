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
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map.Layers

    Public Class ucLayerEditorDepth

        Private m_fpDepth As cEwEFormatProvider = Nothing

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            Try
                If bDisposing Then
                    If (Me.UIContext Is Nothing) Then Return
                    If components IsNot Nothing Then
                        components.Dispose()
                    End If
                End If
            Finally
                MyBase.Dispose(bDisposing)
            End Try
        End Sub

#End Region ' Construction / destruction

#Region " Overrides "

        Public Overrides Sub Initialize(ByVal editor As cLayerEditorRaster)
            MyBase.Initialize(editor)

            Debug.Assert(TypeOf editor Is cLayerEditorDepth, "Depth editor expected")

            Dim meta As New cVariableMetaData(0.1, 10000, cOperatorManager.getOperator(eOperators.GreaterThan), cOperatorManager.getOperator(eOperators.LessThan))
            Me.m_fpDepth = New cEwEFormatProvider(Me.UIContext, Me.m_nudDepth, GetType(Single), meta)
            Me.m_fpDepth.Value = 1
            AddHandler Me.m_fpDepth.OnValueChanged, AddressOf OnValueChanged

            Me.m_cbProtectCoastline.Checked = DirectCast(editor, cLayerEditorDepth).ProtectCoastLine

            If CSng(Me.Editor.CellValue) > 0 Then
                Me.m_rbWater.Checked = True
            Else
                Me.m_rbLand.Checked = True
            End If
            Me.UpdatePreview(Me.m_pbPreviewLand, 0)

        End Sub

        Public Overrides Sub Detach()

            RemoveHandler Me.m_fpDepth.OnValueChanged, AddressOf OnValueChanged
            Me.m_fpDepth.Release()
            MyBase.Detach()

        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            Dim val As Single

            ' Sanity check
            If (Me.m_nudDepth Is Nothing) Then Return
            If (Me.UIContext Is Nothing) Then Return

            ' Set control value
            val = CSng(Me.Editor.CellValue)
            If (val <= 0) Then
                Me.m_rbLand.Checked = True
            Else
                Me.m_rbWater.Checked = True
                Me.m_fpDepth.Value = val
            End If
            ' Respond
            Me.UpdatePreview(Me.m_pbPreviewWater, CSng(Me.m_nudDepth.Value))

            Dim bEditable As Boolean = editor.IsEditable

            Me.m_rbLand.Enabled = bEditable
            Me.m_rbWater.Enabled = bEditable
            Me.m_nudDepth.Enabled = bEditable
            Me.m_cbProtectCoastline.Enabled = bEditable

        End Sub

#End Region ' Overrides

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            If (Me.UIContext Is Nothing) Then Return
            Me.UpdateContent(Me.Editor)
        End Sub

        Private Sub OnLandWaterSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_rbWater.CheckedChanged, m_rbLand.CheckedChanged
            Me.UpdateValue()
        End Sub

        Private Sub OnDepthFieldGotFocus(sender As Object, e As System.EventArgs) _
            Handles m_nudDepth.GotFocus
            Me.m_rbWater.Checked = True
        End Sub

        Private Sub OnValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
            If (Me.UIContext Is Nothing) Then Return
            Me.m_rbWater.Checked = True
            Me.UpdateValue()
        End Sub

        Private Sub OnProtectCoastlineCheckChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cbProtectCoastline.CheckedChanged
            If (Me.UIContext Is Nothing) Then Return
            If (TypeOf Me.Editor Is cLayerEditorDepth) Then
                Dim ed As cLayerEditorDepth = DirectCast(Me.Editor, cLayerEditorDepth)
                ed.ProtectCoastLine = Me.m_cbProtectCoastline.Checked
            End If
            Me.UpdateContent(Me.Editor)
        End Sub

        Private Sub OnSmooth(sender As System.Object, e As System.EventArgs) _
            Handles m_btnSmooth.Click
            Me.Editor.Smooth()
        End Sub

        Private Sub OnFillLayer(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnFill.Click
            Me.Editor.Reset()
        End Sub

#End Region ' Events

#Region " Internals "

        Private Sub UpdatePreview(ByVal pb As PictureBox, ByVal sValue As Single)

            Dim bmp As New Bitmap(pb.Width, pb.Height, Imaging.PixelFormat.Format32bppArgb)
            Dim g As Graphics = Graphics.FromImage(bmp)
            Dim renderer As cRasterLayerRenderer = Nothing

            If (Me.Layer IsNot Nothing) Then
                renderer = DirectCast(Me.Layer.Renderer, cRasterLayerRenderer)
            End If

            If (renderer IsNot Nothing) Then
                renderer.RenderCell(g, New Rectangle(0, 0, bmp.Width, bmp.Height),
                                    Me.Layer.Data, sValue,
                                    cStyleGuide.eStyleFlags.Highlight)
            End If
            pb.Image = bmp

            g.Dispose()

        End Sub

        Private Sub UpdateValue()

            If (Me.UIContext Is Nothing) Then Return

            If Me.m_rbWater.Checked Then
                Me.Editor.CellValue = CSng(Me.m_fpDepth.Value)
                Me.m_cbProtectCoastline.Enabled = True
            Else
                Me.Editor.CellValue = 0
                Me.m_cbProtectCoastline.Enabled = False
            End If
        End Sub

#End Region ' Internals

    End Class

End Namespace