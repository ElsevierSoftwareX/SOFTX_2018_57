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
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Definitions

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' <summary>
    ''' Class that wraps spatial data for display in the UI.
    ''' </summary>
    Public Class cDisplayLayer
        Implements IDisposable

#Region " Private vars "

        Protected m_uic As cUIContext = Nothing
        Protected m_bDisposed As Boolean = False

        Protected m_strName As String = ""
        Protected m_renderer As cLayerRenderer = Nothing
        Protected m_bSelected As Boolean = False
        Protected m_bInUpdate As Boolean = False
        Protected m_editor As cLayerEditor = Nothing

#End Region ' Private vars

#Region " Construction / destruction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="renderer"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal renderer As cLayerRenderer)

            Debug.Assert(uic IsNot Nothing)

            Me.m_uic = uic
            Me.m_strName = ""
            Me.m_renderer = renderer

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Copy constructor.
        ''' </summary>
        ''' <param name="layer">The layer to copy.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, ByVal layer As cDisplayLayer)

            Me.New(uic, layer.Renderer.Clone())
            Me.Name = layer.Name
            Me.IsSelected = layer.IsSelected

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bDisposing"></param>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub Dispose(ByVal bDisposing As Boolean)
            Me.m_bDisposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region ' Construction / destruction

#Region " Public definitions "

        ''' <summary>
        ''' Enumerated type to indicate layer changes.
        ''' </summary>
        Public Enum eChangeFlags As Integer
            ''' <summary>Value to indicate that a layers' map data has changed.</summary>
            Map = 1
            ''' <summary>Value to indicate that a layers' visual representation style has changed.</summary>
            VisualStyle = 2
            ''' <summary>Value to indicate that a layers' visible state has changed.</summary>
            Visibility = 4
            ''' <summary>Value to indicate that a layers' selected state has changed.</summary>
            Selected = 8
            ''' <summary>Value to indicate that one ore more of a layers' name, description (?) 
            ''' and other descriptive values have changed.</summary>
            Descriptive = 16
            ''' <summary>Value to indicate that a layers' editable state has changed.</summary>
            Editable = 32
            ''' <summary>All possible flags.</summary>
            All = &HFFFF

        End Enum

        ''' <summary>
        ''' Layer change event
        ''' </summary>
        ''' <param name="layer"></param>
        ''' <param name="updateType"></param>
        Public Event LayerChanged(ByVal layer As cDisplayLayer, ByVal updateType As eChangeFlags)

#End Region ' Public definitions

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Call this whenever properties and visual aspects of the layer have changed.
        ''' </summary>
        ''' <param name="updateType">Bitwise <see cref="eChangeFlags">flag</see>
        ''' indicating which aspects of the layer have changed.</param>
        ''' <param name="bNotifyCore">Flag stating whether this change should be
        ''' passed to the Core. This flag should be true if the method was called
        ''' to commit a layer data change to the core, and should be false if the 
        ''' layer is responding to a core layer change message.</param>
        ''' -----------------------------------------------------------------------
        Public Overridable Sub Update(ByVal updateType As eChangeFlags, _
                                      Optional ByVal bNotifyCore As Boolean = True)

            ' Prevent looped updates
            If Me.m_bInUpdate = True Then Return
            Me.m_bInUpdate = True

            ' Assess changes
            Try
                If ((updateType And eChangeFlags.VisualStyle) = eChangeFlags.VisualStyle) Then
                    Me.m_renderer.Update()
                    If (Me.AllowValidation) Then
                        Me.m_renderer.VisualStyle.Update()
                    End If
                End If

                ' Inform the world last
                RaiseEvent LayerChanged(Me, updateType)

            Catch ex As Exception

            End Try

            Me.m_bInUpdate = False

        End Sub

#End Region ' Public access

#Region " Public properties "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of this layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property Name() As String
            Get
                Return Me.m_strName
            End Get
            Set(ByVal value As String)
                If (String.Compare(value, Me.m_strName) <> 0) Then
                    Me.m_strName = value
                    Me.Update(eChangeFlags.Descriptive)
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the units of this layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property Units() As String
            Get
                Return ""
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the layer <see cref="cLayerRenderer">renderer</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Renderer() As cLayerRenderer
            Get
                Return Me.m_renderer
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the layer <see cref="cLayerEditor">editor</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property Editor() As cLayerEditor
            Get
                If (Me.m_editor IsNot Nothing) Then
                    If (Me.m_editor.UIContext Is Nothing) Then
                        Me.m_editor.Initialize(Me.m_uic, Me)
                    End If
                End If
                Return Me.m_editor
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the layer is selected.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property IsSelected() As Boolean
            Get
                Return Me.m_bSelected
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bSelected) Then
                    Me.m_bSelected = value
                    Me.Update(eChangeFlags.Selected)
                End If
            End Set
        End Property

        Public Property AllowValidation() As Boolean = True

        Public Property RenderMode As eLayerRenderType
            Get
                If (Me.m_renderer Is Nothing) Then Return eLayerRenderType.Always
                Return Me.m_renderer.RenderMode
            End Get
            Set(value As eLayerRenderType)
                If (Me.m_renderer Is Nothing) Then Return
                Me.m_renderer.RenderMode = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cProperty"/> that provides access to the layer name. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable ReadOnly Property GetNameProperty() As cProperty
            Get
                Return Nothing
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cProperty"/> that provides access to the layer data and metadata. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable ReadOnly Property GetDataProperty As Properties.cProperty
            Get
                Return Nothing
            End Get
        End Property

#End Region ' Public properties

#Region " Internals "

        Public Overridable ReadOnly Property DisplayText() As String
            Get
                Return Me.Name
            End Get
        End Property

#End Region ' Internals

    End Class ' Layer

End Namespace
