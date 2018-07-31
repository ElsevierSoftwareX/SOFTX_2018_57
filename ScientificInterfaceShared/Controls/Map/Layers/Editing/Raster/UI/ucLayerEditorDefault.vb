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

Imports EwEUtils.Utilities
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Definitions
Imports EwECore.Style

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' =======================================================================
    ''' <summary>
    ''' Default layer editor control for working with Ecospace map layers.
    ''' </summary>
    ''' =======================================================================
    Public Class ucLayerEditorDefault

        Protected m_fpName As cEwEFormatProvider = Nothing
        Protected m_bInUpdate As Boolean = False

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Public Overrides Sub Attach(uic As cUIContext, editor As cLayerEditor, layer As cDisplayLayerRaster)
            MyBase.Attach(uic, editor, layer)
            Me.m_fpName = New cEwEFormatProvider(uic, Me.m_tbxName, GetType(String))
            AddHandler Me.m_fpName.OnValueChanged, AddressOf OnNameChanged
        End Sub

        Public Overrides Sub Detach()
            RemoveHandler Me.m_fpName.OnValueChanged, AddressOf OnNameChanged
            Me.m_fpName.Release()
            MyBase.Detach()
        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            ' Sanity checks
            If (editor Is Nothing) Then Return
            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_ucSlider Is Nothing) Then Return

            Dim bEnabled As Boolean = editor.IsEditable

            Me.m_ucSlider.Value = editor.CursorSize
            Me.m_ucSlider.Enabled = bEnabled

            If (Me.Layer IsNot Nothing) Then
                Me.m_fpName.Enabled = Me.HasUniqueSource()
                Me.m_fpName.Value = Me.Layer.Name

                Dim sMin As Single = cCore.NULL_VALUE
                Dim sMax As Single = cCore.NULL_VALUE
                Dim r As cLayerRenderer = editor.Layer.Renderer

                If (r IsNot Nothing) Then
                    sMin = r.ScaleMin
                    sMax = r.ScaleMax
                End If

                If sMin = cCore.NULL_VALUE Then sMin = Me.Layer.Data.MinValue
                If sMax = cCore.NULL_VALUE Then sMax = Me.Layer.Data.MaxValue

                Me.m_tbxMin.Text = cStringUtils.FormatNumber(sMin)
                Me.m_tbxMax.Text = cStringUtils.FormatNumber(sMax)
                Me.m_tbxunits.Text = Me.Layer.Units

            Else
                Me.m_fpName.Enabled = False
            End If

        End Sub

        Private Sub OnSliderValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_ucSlider.ValueChanged

            If (Me.Editor Is Nothing) Then Return

            Me.Editor.CursorSize = CInt(Me.m_ucSlider.Value)
            Me.RaiseChangedEvent()

        End Sub

        Private Sub OnLegendDoubleclick(sender As Object, e As System.EventArgs) _
            Handles m_plLegend.DoubleClick
            Me.EditLayer(eLayerEditTypes.EditVisuals)
        End Sub

        Private Sub OnPaintLegend(sender As Object, e As System.Windows.Forms.PaintEventArgs) _
            Handles m_plLegend.Paint

            If (Me.Layer IsNot Nothing) Then
                Dim r As cLayerRenderer = Editor.Layer.Renderer
                If (r IsNot Nothing) Then
                    r.RenderPreview(e.Graphics, e.ClipRectangle)
                End If
            End If

        End Sub

        Private Sub OnNameChanged(sender As Object, args As EventArgs)
            If (Me.m_bInUpdate) Then Return
            Me.m_bInUpdate = True
            Try
                If Me.m_fpName.Enabled Then Me.Layer.Name = CStr(Me.m_fpName.Value)
            Catch ex As Exception

            End Try
            Me.m_bInUpdate = False
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Diagnostic method, states if a layer has a unique core variable 
        ''' link. Layers with unique sources support extras that can be stored
        ''' in the database such as remarks and visual styles, and can have their
        ''' name changed by the user.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function HasUniqueSource() As Boolean
            If (Me.Layer.Source Is Nothing) Then Return False
            If (TypeOf Me.Layer.Source Is cEcospaceBasemap) Then Return False
            Return True
        End Function

        Private Sub OnDoubleclickValue(sender As System.Object, e As System.EventArgs) _
            Handles m_tbxMax.DoubleClick, m_tbxunits.DoubleClick, m_tbxMin.DoubleClick
            Me.EditLayer(eLayerEditTypes.EditData)
        End Sub

        Private Sub EditLayer(ByVal edittype As eLayerEditTypes)
            Try
                Dim rl As cDisplayLayerRaster = DirectCast(Me.Layer, cDisplayLayerRaster)
                Dim cmd As cEditLayerCommand = DirectCast(Me.UIContext.CommandHandler.GetCommand(cEditLayerCommand.cCOMMAND_NAME), cEditLayerCommand)
                cmd.Invoke(rl, Nothing, edittype)
            Catch ex As Exception
                cLog.Write(ex, eVerboseLevel.Detailed, "ucLayerEditor::EditLayer " & Me.Layer.Name & "(" & edittype.ToString & ")")
            End Try

        End Sub

    End Class

End Namespace
