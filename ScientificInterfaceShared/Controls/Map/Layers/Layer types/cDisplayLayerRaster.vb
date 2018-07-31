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
Imports EwECore.SpatialData
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' <summary>
    ''' Class that wraps a single <see cref="cEcospaceLayer">Ecospace data layer</see> for 
    ''' manipulation in a User Interface.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Ok, the UI layer thing has gotten so complex that a bit of an explanation would not hurt.
    ''' </para>
    ''' <para>
    ''' The entire basemap chain consists of the following collaborating classes:
    ''' <list>
    ''' <item>
    ''' <description>One <see cref="cEcospaceBasemap">basemap</see> which defines the size and other aspects
    ''' of the map currently active in Ecospace. This class also provides access to individual data
    ''' <see cref="cEcospaceLayer">layers</see>.
    ''' </description></item>
    ''' <item><description>
    ''' <para>
    ''' Several <see cref="cEcospaceLayer">data layers</see> which each expose 
    ''' spatial array(s) of data. Basemap layers are two dimensional, allowing access to the array
    ''' via cell(row, col) interaction. Poking around in a basemap layer in fact modifies Ecospace
    ''' spatial cells of spatial data array that the the layer is connected to.
    ''' </para>
    ''' </description></item>
    ''' <item><description>
    ''' <para>
    ''' <see cref="cDisplayLayer">Display layers</see> combine one or more <see cref="cEcospaceLayer">Ecospace 
    ''' data layers</see> as a single unit for display and interaction in the user interface. The GUI
    ''' Layer uses a <see cref="cLayerRenderer">layer renderer</see> to decide how this assembly
    ''' of core data is reflected.
    ''' </para>
    ''' </description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Class cDisplayLayerRaster
        Inherits cDisplayLayer

#Region " Private helper classes "

        ''' ===================================================================
        ''' <summary>
        ''' Default editor class for layers without an editor.
        ''' </summary>
        ''' ===================================================================
        Private Class cLayerEditorRasterDefault
            Inherits cLayerEditorRaster

            Public Sub New()
                MyBase.New(Nothing)
            End Sub

            Public Overrides Property IsReadOnly() As Boolean
                Get
                    Return True
                End Get
                Set(ByVal value As Boolean)
                    ' NOP
                End Set
            End Property

        End Class

#End Region ' Private helper classes

#Region " Private vars "

        Private m_mh As cMessageHandler = Nothing

        Private m_source As cCoreInputOutputBase = Nothing
        Private m_varName As eVarNameFlags = eVarNameFlags.NotSet
        Private m_data As cEcospaceLayer = Nothing
        Private m_valueType As Type = GetType(Single)
        Private m_sValueSet As Single = cCore.NULL_VALUE
        Private m_sValueClear As Single = cCore.NULL_VALUE

        Private m_bModified As Boolean = False

        ''' <summary>
        ''' The <see cref="cProperty">property</see> that simulates live map changes to sync different
        ''' layer instances linked to the same data.
        ''' </summary>
        ''' <remarks>
        ''' This is a hack solution. <see cref="cEcospaceLayer">Basemap layers</see> are not exposed as true
        ''' <see cref="EwECore.ValueWrapper.cValue">core value objects</see>. To provide layers with common GUI issues
        ''' such as remark feedback and broadcasted updates, as well as the ability to attach 
        ''' <see cref="EwECore.Auxiliary.cVisualStyle">Visual Styles</see> to layers, this hidden property is used.
        ''' </remarks>
        Private m_propName As cProperty = Nothing

        Private m_strUnits As String = ""
        Private m_strUnitMask As String = ""

        ' --- shared defaults ---

        Private Shared s_editorLocked As New cLayerEditorRasterDefault()

#End Region ' Private vars

#Region " Construction / destruction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="renderer"></param>
        ''' <param name="source">
        ''' The core object that serves two purposes:
        ''' <list type="number">
        ''' <item><description>Provide the dynamic name for a layer</description></item>
        ''' <item><description>Provide the definition for distributing data changes</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="varName">The name of the variable to associate data changes with</param>
        ''' <param name="sValueSet"></param>
        ''' <param name="sValueClear"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext,
                       ByVal data As cEcospaceLayer,
                       ByVal renderer As cLayerRenderer,
                       ByVal editor As cLayerEditor,
                       Optional ByVal source As cCoreInputOutputBase = Nothing,
                       Optional ByVal varName As eVarNameFlags = eVarNameFlags.Name,
                       Optional ByVal sValueSet As Single = cCore.NULL_VALUE,
                       Optional ByVal sValueClear As Single = cCore.NULL_VALUE)

            MyBase.New(uic, renderer)

            Me.m_mh = New cMessageHandler(AddressOf EcospaceMessageHandler, eCoreComponentType.EcoSpace, eMessageType.Any, Me.m_uic.SyncObject)
#If DEBUG Then
            Me.m_mh.Name = "UI::cRasterLayer " & Me.m_varName.ToString
#End If
            uic.Core.Messages.AddMessageHandler(Me.m_mh)

            '' Sanity checks
            'Debug.Assert(Not Object.ReferenceEquals(data, Nothing))

            If (editor Is Nothing) Then editor = cDisplayLayerRaster.s_editorLocked

            Me.m_source = source
            Me.m_varName = varName
            Me.m_data = data
            Me.m_editor = editor
            Me.m_sValueSet = sValueSet
            Me.m_sValueClear = sValueClear
            Me.m_valueType = data.ValueType
            Me.m_propName = uic.PropertyManager.GetProperty(source, varName)

            If (m_propName IsNot Nothing) Then
                AddHandler Me.m_propName.PropertyChanged, AddressOf OnPropertyChanged
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Copy constructor.
        ''' </summary>
        ''' <param name="layer">The layer to copy.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, ByVal layer As cDisplayLayerRaster)

            Me.New(uic, layer.Data, layer.Renderer.Clone(), layer.Editor.Clone(), layer.Source, layer.VarName, layer.ValueSet, layer.ValueClear)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bDisposing"></param>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            If Not Me.m_bDisposed Then
                If bDisposing Then
                    If Me.m_uic IsNot Nothing Then
                        Me.m_uic.Core.Messages.RemoveMessageHandler(Me.m_mh)
                    End If
                    If Me.m_propName IsNot Nothing Then
                        RemoveHandler Me.m_propName.PropertyChanged, AddressOf OnPropertyChanged
                        Me.m_propName = Nothing
                    End If
                End If
            End If
            Me.m_bDisposed = True
        End Sub

#End Region ' Construction / destruction

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Instructs the layer to incorporate units the layer name display.
        ''' </summary>
        ''' <param name="strUnitMask">The format mask to apply. This mask must
        ''' contain a '{0}' field to place the layer name value, and a '{1}' field
        ''' to place the unit value.</param>
        ''' <param name="strUnits">Unit(s) to place in the layer display text.</param>
        ''' -----------------------------------------------------------------------
        Public Sub SetUnitMask(ByVal strUnitMask As String, ByVal strUnits As String)
            Me.SetUnitHeader(strUnitMask, strUnits)
        End Sub

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
        Public Overrides Sub Update(ByVal updateType As eChangeFlags,
                                    Optional ByVal bNotifyCore As Boolean = True)

            ' Prevent looped updates
            If (Me.m_bInUpdate = True) Then Return

            ' Assess changes
            Try
                ' Map has changed via user drawing
                If ((updateType And eChangeFlags.Map) = eChangeFlags.Map) Then
                    ' Update visuals
                    Me.Data.Invalidate()

                    ' Is a core layer?
                    If bNotifyCore Then
                        If Me.Data.DataType <> eDataTypes.NotSet Then
                            ' #Yes: inform the core
                            If (Me.m_uic IsNot Nothing) And (Me.AllowValidation) Then
                                Me.m_uic.Core.onChanged(Me.Data)
                            End If
                        Else
                            ' #No: Fire off property change to make other copies of non-core layers respond
                            If (Me.m_propName IsNot Nothing) Then
                                Me.m_propName.FireChangeNotification(cProperty.eChangeFlags.Custom)
                            End If
                        End If
                    End If

                End If

            Catch ex As Exception

            End Try

            MyBase.Update(updateType, bNotifyCore)

        End Sub

#End Region ' Public access

#Region " Public properties "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of this layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides Property Name() As String
            Get
                ' #Yes: is a backlink property provided?
                If (Me.m_propName IsNot Nothing) Then
                    ' #Yes: and is this property linked to a true name?
                    If (Me.m_propName.VarName = eVarNameFlags.Name) Then
                        ' #Yes: return name property value
                        Return CStr(Me.m_propName.GetValue())
                    End If
                End If
                ' Alternative: is data attached?
                If (Me.Data IsNot Nothing) Then
                    ' #Yes: return data name)
                    Return Me.Data.Name
                End If
                ' Return base name
                Return MyBase.Name
            End Get
            Set(ByVal value As String)
                ' #Yes: is a backlink property provided?
                If (Me.m_propName IsNot Nothing) Then
                    ' #Yes: and is this property linked to a true name?
                    If (Me.m_propName.VarName = eVarNameFlags.Name) Then
                        ' #Yes: return name property value
                        Me.m_propName.SetValue(value)
                        Return
                    End If
                End If
                MyBase.Name = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the formatted units for this layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides ReadOnly Property Units As String
            Get
                Dim u As New cUnits(Me.m_uic.Core)
                Dim md As cVariableMetaData = cVariableMetaData.Get(Me.VarName)
                If (Me.Data IsNot Nothing) Then
                    md = Me.Data.MetadataCell
                End If
                Return u.ToString(md)
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the source of this layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property Source() As cCoreInputOutputBase
            Get
                Return Me.m_source
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the source of this layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property SourceSec() As cCoreInputOutputBase
            Get
                Return Nothing
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the variable of the source this layer applies to.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property VarName() As eVarNameFlags
            Get
                Return Me.m_varName
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the underlying core-exposed layer data.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property Data() As cEcospaceLayer
            Get
                Return Me.m_data
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the value that this layer interprets as relevant values.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property ValueSet() As Single
            Get
                Return Me.m_sValueSet
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the value that this layer interprets as clear values.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property ValueClear() As Single
            Get
                Return Me.m_sValueClear
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether a given cell position has a value.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable ReadOnly Property IsValue(ByVal objValue As Object) As Boolean
            Get
                'If Object.ReferenceEquals(Me.m_sValueSet, Nothing) Then Return False

                ' Composed value types are too horrendous to test - just assume a value is there
                ' until a better idea is conconcted.
                If (TypeOf (objValue) Is Array) Then Return True
                If (TypeOf (objValue) Is Boolean) Then Return (CBool(objValue))

                Dim sCellValue As Single = CSng(objValue)

                If Me.m_sValueSet.Equals(cCore.NULL_VALUE) Then
                    If Me.m_sValueClear.Equals(cCore.NULL_VALUE) Then
                        Return (sCellValue <> cCore.NULL_VALUE)
                    Else
                        Return (sCellValue <> 0)
                    End If
                End If
                Return Me.m_sValueSet.Equals(sCellValue)
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value in the underlying data layer.
        ''' </summary>
        ''' <param name="iRow">One-based row index</param>
        ''' <param name="iCol">One-based column index</param>
        ''' -----------------------------------------------------------------------
        Public Overridable Property Value(ByVal iRow As Integer, ByVal iCol As Integer) As Object
            Get
                Return Me.Data.Cell(iRow, iCol)
            End Get
            Set(ByVal value As Object)
                Me.Data.Cell(iRow, iCol) = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the data type of values in the underlying data layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property ValueType() As Type
            Get
                Return Me.m_valueType
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the underlying data is 
        ''' <see cref="cEcospaceLayer.IsExternalData">driven externally</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property IsExternal As Boolean
            Get
                Dim l As cEcospaceLayer = Me.Data
                If (l Is Nothing) Then Return False
                For j As Integer = 1 To cSpatialDataStructures.cMAX_CONN
                    If l.IsExternalData() Then Return True
                Next
                Return False
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the underlying data is enabled for external driving.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property IsExternalEnabled As Boolean
            Get
                Dim l As cEcospaceLayer = Me.Data
                If (l Is Nothing) Then Return False
                If (Me.m_uic Is Nothing) Then Return False
                Dim adt As cSpatialDataAdapter = Me.m_uic.Core.SpatialDataConnectionManager.Adapter(l.VarName)
                If (adt Is Nothing) Then Return False
                Return adt.IsEnabled(l.Index)
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the layer has been modified.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property IsModified() As Boolean
            Get
                Return Me.m_bModified
            End Get
            Set(ByVal value As Boolean)
                Dim l As cEcospaceLayer = Me.Data
                If (l Is Nothing) Then Return
                Me.m_bModified = value
                If (value = True) Then
                    l.Invalidate()
                End If
            End Set
        End Property

        Public Overridable ReadOnly Property HasData As Boolean
            Get
                If (Me.Data Is Nothing) Then Return False
                Return (Me.Data.NumValueCells > 0)
            End Get
        End Property

        Public Function SetNameProperty() As Boolean

        End Function

        Public Overrides ReadOnly Property GetNameProperty As Properties.cProperty
            Get
                Return Me.m_propName
            End Get
        End Property

        Public Overrides ReadOnly Property GetDataProperty As Properties.cProperty
            Get
                If (Me.Data IsNot Nothing) Then
                    If (TypeOf Me.Data Is cEcospaceLayer) And (TypeOf Me.Data Is cCoreInputOutputBase) Then
                        Return Me.m_uic.PropertyManager.GetProperty(DirectCast(Me.Data.Manager, cCoreInputOutputBase), Me.Data.VarName, Me.SourceSec)
                    End If
                End If
                Return Nothing
            End Get
        End Property

#End Region ' Public properties

#Region " Events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' For core layers, the regular core DataModified messages relay layer 
        ''' updates.
        ''' </summary>
        ''' <param name="msg"></param>
        ''' -------------------------------------------------------------------
        Private Sub EcospaceMessageHandler(ByRef msg As cMessage)

            If (Me.Data Is Nothing) Then Return

            ' JS 22Nov14: also respond to Ecospace param settings
            If (msg.DataType = Me.Data.DataType) Or (msg.DataType = eDataTypes.EcospaceModelParameter) Then
                ' Trigger update
                Me.Update(eChangeFlags.Map, False)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' For non-core layers, a property change is used to trigger layer 
        ''' updates among independent copies of layers.
        ''' </summary>
        ''' <param name="prop"></param>
        ''' <param name="changeFlags"></param>
        ''' -------------------------------------------------------------------
        Private Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)

            ' Prevent looped updates
            If Me.m_bInUpdate Then Return

            ' Translate property change flags into layer change flags
            Dim flag As cDisplayLayer.eChangeFlags = 0

            ' Has the name or remark changed?
            If (changeFlags And (cProperty.eChangeFlags.Value Or cProperty.eChangeFlags.Remarks)) > 0 Then
                ' Send out layer name change event
                flag = flag Or eChangeFlags.Descriptive
            End If

            ' Using the property hacK?
            If (changeFlags And cProperty.eChangeFlags.Custom) > 0 Then
                ' Not so sure!
                flag = flag Or (eChangeFlags.All And (Not eChangeFlags.Map))
            End If

            If (flag <> 0) Then
                ' Trigger update
                Me.Update(flag)
            End If

        End Sub

#End Region ' Events

#Region " Internals "

        Protected Sub SetUnitHeader(ByVal strUnitMask As String, ByVal strUnits As String)
            Me.m_strUnitMask = strUnitMask
            Me.m_strUnits = strUnits
        End Sub

        Public Overrides ReadOnly Property DisplayText() As String
            Get
                Dim strDisplayText As String = ""

                If (m_strUnits Is Nothing) Or (String.IsNullOrEmpty(Me.m_strUnitMask)) Then
                    strDisplayText = Me.Name
                Else
                    Try
                        Dim sg As cStyleGuide = Me.m_uic.StyleGuide
                        strDisplayText = cStringUtils.Localize(Me.m_strUnitMask, Me.Name, sg.FormatUnitString(Me.m_strUnits))
                    Catch ex As Exception
                        Debug.Assert(False, "Failed to apply format mask, please check")
                        strDisplayText = Me.Name
                    End Try
                End If
                Return strDisplayText
            End Get
        End Property

#End Region ' Internals

    End Class ' Layer

End Namespace
