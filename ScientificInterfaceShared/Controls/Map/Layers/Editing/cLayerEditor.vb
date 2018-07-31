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

#End Region ' Imports 

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer editor base class that supports manual modification of Ecospace 
    ''' layers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustInherit Class cLayerEditor

#Region " Private vars "

        ' === LAYER SUPPORT ===
        ''' <summary>The raster layer to operate on.</summary>
        Protected m_layer As cDisplayLayer = Nothing
        ''' <summary>Flag stating whether the layer is editable.</summary>
        Protected m_bEditable As Boolean = True
        ''' <summary>Flag stating whether the layer is being edited.</summary>
        Private m_bEditing As Boolean = False

        ' === GUI SUPPORT ===
        ''' <summary>Runtime type of the <see cref="ucLayerEditor">layer editor GUI</see>
        ''' that implements the user interface controls to configure the editor.</summary>
        Private m_typeGUI As Type = Nothing
        ''' <summary>A GUI, if any.</summary>
        Private m_gui As ILayerEditorGUI = Nothing

#End Region ' Private vars

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="typeGUI">The class type for the GUI to attach to this editor, if any.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal typeGUI As Type)
            If typeGUI Is Nothing Then typeGUI = GetType(ucLayerEditorDefault)
            Me.m_typeGUI = typeGUI
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the layer editor.
        ''' </summary>
        ''' <param name="uic">UI context to attach.</param>
        ''' <param name="layer">Layer to attach.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Initialize(ByVal uic As cUIContext, ByVal layer As cDisplayLayer)
            Me.UIContext = uic
            Me.Layer = layer
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Destroy the editor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub Finalize()

            If (Me.m_gui IsNot Nothing) Then
                If (TypeOf Me.m_gui Is ucLayerEditor) Then
                    DirectCast(Me.m_gui, ucLayerEditor).Detach()
                End If
            End If

            Me.Layer = Nothing
            Me.UIContext = Nothing

            MyBase.Finalize()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Duplicate the editor
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function Clone() As cLayerEditor
            Dim minime As cLayerEditor = Nothing

            ' Clone without GUI type
            minime = DirectCast(Activator.CreateInstance(Me.GetType(), New Object() {}), cLayerEditor)
            minime.IsEditable = Me.IsEditable
            minime.IsReadOnly = Me.IsReadOnly

            Return minime
        End Function

#End Region ' Construction

#Region " Events "

        Protected Sub OnLayerChanged(ByVal layer As cDisplayLayer, ByVal cf As cDisplayLayer.eChangeFlags)
            If (Me.GUI IsNot Nothing) Then
                Me.GUI.UpdateContent(CType(Me, cLayerEditorRaster))
            End If
        End Sub

#End Region ' Events

#Region " GUI feedback "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a <see cref="ucLayerEditor">layer editor control</see> to 
        ''' allow a user to parameterize the edit process.
        ''' </summary>
        ''' <remarks>
        ''' Do not forget to destroy any control created with this method via 
        ''' <see cref="DestroyEditorControl">DestroyEditorControl</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Function CreateEditorControl() As ucLayerEditor

            Dim gui As ucLayerEditor = Nothing

            Debug.Assert(Me.m_gui Is Nothing)

            Try
                Dim obj As Object = Activator.CreateInstance(Me.m_typeGUI, New Object() {})
                ' Sanity check
                Debug.Assert(TypeOf obj Is ucLayerEditor)

                gui = DirectCast(obj, ucLayerEditor)
                gui.Attach(Me.UIContext, Me, CType(Me.m_layer, cDisplayLayerRaster))
                gui.Initialize(CType(Me, cLayerEditorRaster))

                ' Remember GUI
                Me.m_gui = gui

            Catch ex As Exception
                Debug.Assert(False, "Failed to create layer editor interface")
            End Try

            Return gui
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Destroys a <see cref="ucLayerEditor">layer editor control</see>.
        ''' </summary>
        ''' <remarks>
        ''' Only use this method on controls created with 
        ''' <see cref="CreateEditorControl">CreateEditorControl</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Sub DestroyEditorControl()

            Debug.Assert(Me.m_gui IsNot Nothing)

            If (TypeOf Me.m_gui Is ucLayerEditor) Then
                DirectCast(Me.m_gui, ucLayerEditor).Detach()
                DirectCast(Me.m_gui, ucLayerEditor).Dispose()
            End If
            Me.m_gui = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cursor feedback for the current location of the cursor.
        ''' </summary>
        ''' <param name="ptMouse">Mouse position over the map.</param>
        ''' <param name="map">The map that the mouse is moving over.</param>
        ''' -------------------------------------------------------------------
        Public MustOverride Function Cursor(ByVal ptMouse As Point, ByVal map As ucMap) As Cursor

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="ILayerEditorGUI">GUI</see> attached to the
        ''' editor. 
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property GUI() As ILayerEditorGUI
            Get
                Return Me.m_gui
            End Get
        End Property

#End Region ' GUI feedback

#Region " Mouse input processing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Process a mouse button click. This can put the editor in edit mode.
        ''' </summary>
        ''' <param name="e"></param>
        ''' <param name="map"></param>
        ''' -------------------------------------------------------------------
        Public MustOverride Sub ProcessMouseClick(e As MouseEventArgs, map As ucMap)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Process mouse drawing with the left button down. This is only called when
        ''' the editor is <see cref="IsEditing"/>
        ''' </summary>
        ''' <param name="e"></param>
        ''' <param name="map"></param>
        ''' -------------------------------------------------------------------
        Public MustOverride Sub ProcessMouseDraw(e As MouseEventArgs, map As ucMap)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Process the end of a mouse draw operation
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public MustOverride Sub ProcessMouseUp()

#End Region ' Mouse input processing

#Region " Editing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' User has started editing the layer.
        ''' </summary>
        ''' <param name="e">Click <see cref="MouseEventArgs">mouse state</see>
        ''' information.</param>
        ''' -------------------------------------------------------------------
        Protected MustOverride Sub StartEdit(ByVal e As MouseEventArgs, map As ucMap)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' User is done editing the layer.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected MustOverride Sub EndEdit()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the layer is being edited.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IsEditing As Boolean
            Get
                Return Me.m_bEditing
            End Get
            Protected Set(value As Boolean)
                Me.m_bEditing = value
            End Set
        End Property

#End Region ' Editing

#Region " Properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cUIContext"/> to operate onto.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property UIContext() As cUIContext

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the layer is editable.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property IsEditable() As Boolean
            Get
                Dim bEditable As Boolean = (Me.m_bEditable = True) And (Me.IsReadOnly = False)

                If (Me.m_layer IsNot Nothing) Then
                    ' JS 22Nov14: this makes it hard to play with data. Removed constraint
                    '' External data cannot be edited
                    'bEditable = bEditable And (Not Me.m_layer.IsExternal)
                    ' Invisible data cannot be edited
                    If (Me.m_layer.Renderer IsNot Nothing) Then bEditable = bEditable And Me.m_layer.Renderer.IsVisible
                Else
                    ' No need to edit a layer that does not exist, no?
                    bEditable = False
                End If
                Return bEditable
            End Get
            Set(ByVal value As Boolean)
                Dim bEditable As Boolean = value
                If (bEditable <> Me.m_bEditable) Then
                    Me.m_bEditable = bEditable
                    ' Send out change notification
                    If (Me.m_layer IsNot Nothing) Then
                        Me.m_layer.Update(cDisplayLayer.eChangeFlags.Editable)
                    End If
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the layer can be made editable at all.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property IsReadOnly() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the layer to attach to this Editor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Layer() As cDisplayLayer
            Get
                Return Me.m_layer
            End Get
            Protected Set(ByVal value As cDisplayLayer)
                If ReferenceEquals(value, Me.m_layer) Then Return

                ' Already has a layer?
                If (Me.m_layer IsNot Nothing) Then
                    ' #Yes: stop listening to layer changes
                    RemoveHandler Me.m_layer.LayerChanged, AddressOf OnLayerChanged
                End If

                ' Store new layer
                Me.m_layer = value

                ' Has a new layer?
                If (Me.m_layer IsNot Nothing) Then
                    ' #Yes: start listening to layer changes
                    AddHandler Me.m_layer.LayerChanged, AddressOf OnLayerChanged
                End If
            End Set
        End Property

#End Region ' Properties

    End Class

End Namespace
