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

Imports ScientificInterfaceShared.Style
Imports EwEUtils.Core

Namespace Controls.Map.Layers

    ''' <summary>
    ''' Visual editor for <see cref="cLayerRendererExclusion"/>
    ''' </summary>
    Public Class ucLayerEditorExclusion

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

            Me.m_nudDepth.DecimalPlaces = 0
            Me.m_nudDepth.Maximum = 10000
            Me.m_nudDepth.Minimum = 1
            Me.m_nudDepth.Increment = 10

            Dim sg As cStyleGuide = editor.UIContext.StyleGuide
            Me.m_cbAlwaysShowExcluded.Checked = sg.ShowMapsExcludedCells

        End Sub

#End Region ' Overrides

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
        End Sub

        Private Sub OnClear(sender As System.Object, e As System.EventArgs) _
            Handles m_btnClear.Click
            Try
                Me.UIContext.Core.ClearExcludedCells()
            Catch ex As Exception
                cLog.Write(ex, "ucLayerEditorExclusion:OnClear()")
            End Try
        End Sub

        Private Sub OnInvert(sender As System.Object, e As System.EventArgs) _
            Handles m_btnInvert.Click
            Try
                Me.UIContext.Core.InvertExcludedCells()
            Catch ex As Exception
                cLog.Write(ex, "ucLayerEditorExclusion:OnInvert()")
            End Try
        End Sub

        Private Sub OnExcludeDepths(sender As System.Object, e As System.EventArgs) _
            Handles m_btnSet.Click
            Try
                Me.UIContext.Core.SetExcludedDepth(CInt(Me.m_nudDepth.Value))
            Catch ex As Exception
                cLog.Write(ex, "ucLayerEditorExclusion:OnExcludeDepths(" & Me.m_nudDepth.Value & ")")
            End Try
        End Sub

        Private Sub OnShowExcludedCellsToggled(sender As System.Object, e As System.EventArgs) _
            Handles m_cbAlwaysShowExcluded.CheckedChanged

            Try
                Dim sg As cStyleGuide = Editor.UIContext.StyleGuide
                sg.ShowMapsExcludedCells = Me.m_cbAlwaysShowExcluded.Checked
            Catch ex As Exception
                cLog.Write(ex, "ucLayerEditorExclusion:OnShowExcludedCellsToggled()")
            End Try

        End Sub

#End Region ' Events

#Region " Internals "

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            Me.m_btnClear.Enabled = (Me.IsAttached)
            Me.m_btnSet.Enabled = (Me.IsAttached)
            Me.m_rbExclude.Checked = (CBool(Me.Editor.CellValue) = True)
            Me.m_rbInclude.Checked = (CBool(Me.Editor.CellValue) = False)

        End Sub

#End Region ' Internals

        Private Sub OnValueChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_rbInclude.CheckedChanged, m_rbExclude.CheckedChanged
            Me.Editor.CellValue = (Me.m_rbExclude.Checked)
        End Sub

    End Class

End Namespace