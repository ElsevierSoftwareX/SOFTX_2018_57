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

Imports EwECore.Style
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for implementing a user interface for maintaining values in
    ''' an underlying <see cref="cLayerEditor">layer editor</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucLayerEditor
        Implements IUIElement
        Implements ILayerEditorGUI

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

#End Region ' Construction / destruction

#Region " Public interfaces "

        Public Overridable Sub Attach(ByVal uic As cUIContext,
                                      ByVal editor As cLayerEditor,
                                      ByVal layer As cDisplayLayerRaster)
            Me.UIContext = uic
            Me.Editor = CType(editor, cLayerEditorRaster)
            Me.Layer = layer
        End Sub

        Public Overridable Sub Detach()
            Me.UIContext = Nothing
            Me.Editor = Nothing
        End Sub

        Public Overridable Property UIContext() As cUIContext _
            Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cLayerEditor">layer editor</see> that this
        ''' GUI operates on.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Editor() As cLayerEditorRaster

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cLayerRenderer">layer renderer</see> for this
        ''' GUI to show editor previews.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Layer() As cDisplayLayerRaster

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="ILayerEditorGUI.Initialize"/>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Initialize(ByVal editor As cLayerEditorRaster) _
            Implements ILayerEditorGUI.Initialize
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="ILayerEditorGUI.StartEdit"/>
        ''' -------------------------------------------------------------------
        Public Overridable Sub StartEdit(ByVal editor As cLayerEditorRaster) _
            Implements ILayerEditorGUI.StartEdit
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="ILayerEditorGUI.EndEdit"/>
        ''' -------------------------------------------------------------------
        Public Overridable Sub EndEdit(ByVal editor As cLayerEditorRaster) _
            Implements ILayerEditorGUI.EndEdit
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="ILayerEditorGUI.UpdateContent"/>
        ''' -------------------------------------------------------------------
        Public Overridable Sub UpdateContent(ByVal editor As cLayerEditorRaster) _
            Implements ILayerEditorGUI.UpdateContent

            Dim strLabel As String = ""
            Dim img As Image = SharedResources.ProtectFormHS

            If (Me.IsAttached = True) Then
                Dim fmtV As New cVarnameTypeFormatter()
                'Dim fmtU As New cUnitFormatter(Me.UIContext.Core)
                Dim md As cVariableMetaData = Nothing
                Dim vn As eVarNameFlags = eVarNameFlags.NotSet

                ' Get label text 
                If (Layer.Data IsNot Nothing) Then
                    md = Layer.Data.MetadataCell()
                    vn = Layer.Data.VarName
                End If

                If (md.Units IsNot Nothing) Then
                    ' ToDo: format units
                End If

                If (vn = eVarNameFlags.NotSet) Then
                    strLabel = Layer.DisplayText()
                Else
                    strLabel = fmtV.GetDescriptor(vn)
                End If

                ' Get layer image
                If editor.IsReadOnly Then
                    img = SharedResources.ProtectFormHS
                Else
                    If editor.IsEditable Then
                        img = SharedResources.Editable
                    Else
                        img = SharedResources.NotEditable
                    End If
                End If
            End If

            Me.m_lblCaption.Image = img
            Me.m_lblCaption.Text = strLabel

        End Sub

#End Region ' Public interfaces

#Region " Public events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public event to notify the outside world that the layer editor
        ''' has undergone a significant change.
        ''' </summary>
        ''' <param name="ucEditor">The editor that changed, e.g. moi.</param>
        ''' -------------------------------------------------------------------
        Public Event OnChanged(ByVal ucEditor As ucLayerEditor)

#End Region ' Public events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Raise the event to notify the outside world that settings in this 
        ''' GUI have changed.
        ''' </summary>
        ''' <remarks>
        ''' The GUI may send change events when changes in its content may need
        ''' to be reflected in the basemap while the underlying GUI layer is 
        ''' not affected. Hence, the layer change event cannot be used for this
        ''' purpose.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Sub RaiseChangedEvent()
            RaiseEvent OnChanged(Me)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the editor is correctly attached to a layer and
        ''' a UI context.
        ''' </summary>
        ''' <returns>True if connected.</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function IsAttached() As Boolean
            If (Me.Editor Is Nothing) Then Return False
            If (Me.Editor.Layer Is Nothing) Then Return False
            Return (Me.UIContext IsNot Nothing)
        End Function

#End Region ' Internals

    End Class

End Namespace

