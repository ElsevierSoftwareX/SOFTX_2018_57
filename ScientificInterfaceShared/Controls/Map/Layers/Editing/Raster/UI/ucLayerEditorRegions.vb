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

Option Strict On
Imports EwECore

Namespace Controls.Map.Layers

    Public Class ucLayerEditorRegion

        Private m_mhSpace As cMessageHandler = Nothing

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

#Region " Overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.UpdateContent(Me.Editor)

            Me.m_mhSpace = New cMessageHandler(AddressOf OnCoreMessage, EwEUtils.Core.eCoreComponentType.EcoSpace, EwEUtils.Core.eMessageType.DataValidation, Me.UIContext.SyncObject)
            Me.UIContext.Core.Messages.AddMessageHandler(Me.m_mhSpace)
#If DEBUG Then
            Me.m_mhSpace.Name = "ucLayerEditorRegions"
#End If

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                    Me.UIContext.Core.Messages.RemoveMessageHandler(Me.m_mhSpace)
                    Me.m_mhSpace.Dispose()
                    Me.m_mhSpace = Nothing
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        Public Overrides Sub UpdateContent(editor As cLayerEditorRaster)

            MyBase.UpdateContent(editor)
            If (Me.UIContext Is Nothing) Then Return

            ' Sanity checks
            If (editor Is Nothing) Then Return
            If (editor.Layer Is Nothing) Then Return
            If (Me.m_nudRegion Is Nothing) Then Return

            editor.CellValueMax = editor.Layer.Data.MaxValue
            Dim decMax As Decimal = CDec(editor.CellValueMax)
            Dim decVal As Decimal = CDec(editor.CellValue)

            ' Set control value
            Dim val As Decimal = Math.Min(decMax, decVal)
            If (val > Me.m_nudRegion.Maximum) Then
                Me.m_nudRegion.Maximum = decMax
                Me.m_nudRegion.Value = decVal
            Else
                Me.m_nudRegion.Value = decVal
                Me.m_nudRegion.Maximum = decMax
            End If

        End Sub

#End Region ' Overrides

#Region " Event handlers "

        Private Sub OnDrawRegionChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_nudRegion.ValueChanged

            If (Me.UIContext Is Nothing) Then Return

            Me.Editor.CellValue = CInt(Me.m_nudRegion.Value)

        End Sub

        Private Sub OnCoreMessage(ByRef msg As cMessage)
            Try
                If (msg.DataType = EwEUtils.Core.eDataTypes.EcospaceModelParameter) And (msg.Type = EwEUtils.Core.eMessageType.DataValidation) Then
                    Me.UpdateContent(Me.Editor)
                End If
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Event handlers

    End Class

End Namespace
