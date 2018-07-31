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
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Definitions

#End Region ' Imports 

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for rendering a <see cref="cDisplayLayer">display layer</see>
    ''' onto the base map.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustInherit Class cLayerRenderer
        : Implements IDisposable

#Region " Helper classes "

        ''' <summary>
        ''' When rendering content, layers may require display of extra symbols that
        ''' can be registered by each layer. These symbols will then be included in
        ''' legends etc.
        ''' </summary>
        Protected Class cSymbol

            Private m_strName As String = ""
            Private m_iKey As Integer = 0

            ''' <summary>
            ''' Create a new symbol.
            ''' </summary>
            ''' <param name="strName">The name of the symbol.</param>
            ''' <param name="iKey">The layer-specific symbol key.</param>
            Public Sub New(strName As String, iKey As Integer)
                Me.m_strName = strName
                Me.m_iKey = iKey
            End Sub

            ''' <summary>
            ''' Get the layer-specific symbol name.
            ''' </summary>
            Public ReadOnly Property Name As String
                Get
                    Return Me.m_strName
                End Get
            End Property

            ''' <summary>
            ''' Get the layer-specific symbol key.
            ''' </summary>
            Public ReadOnly Property Key As Integer
                Get
                    Return Me.m_iKey
                End Get
            End Property
        End Class

#End Region ' Helper classes

#Region " Private vars "

        ''' <summary>Default brush to render the cell with.</summary>
        Protected Shared brDEFAULT As Brush = Brushes.Transparent
        ''' <summary><see cref="cVisualStyle">Style</see> describing what colours
        ''' and font to use for rendering.</summary>
        Private m_vs As cVisualStyle = Nothing

        Private m_lSymbols As New List(Of cSymbol)

#End Region ' Private vars

#Region " Construction / destruction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vs"></param>
        ''' <param name="layerStyleFlags"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal vs As cVisualStyle,
                       Optional ByVal layerStyleFlags As cVisualStyle.eVisualStyleTypes = cVisualStyle.eVisualStyleTypes.NotSet)
            Me.m_vs = vs
            Me.VisualStyleFlags = layerStyleFlags
            Me.Update()
        End Sub

        Protected Overridable Sub Dispose(ByVal bDisposing As Boolean)
            Me.m_vs = Nothing
        End Sub

#Region " IDisposable support "

        ' To detect redundant calls
        Private m_bDisposed As Boolean = False

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Haha I modified it
            If m_bDisposed = False Then
                Dispose(True)
                GC.SuppressFinalize(Me)
                Me.m_bDisposed = True
            End If
        End Sub

#End Region ' IDisposable support

#End Region ' Construction / destruction

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the visual style for this layer representation.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property VisualStyle() As cVisualStyle
            Get
                Return Me.m_vs
            End Get
            Set(ByVal value As cVisualStyle)
                Me.m_vs = value
                Me.Update()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the flags describing which visual style flags apply to a given 
        ''' layer representation.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property VisualStyleFlags() As cVisualStyle.eVisualStyleTypes

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update any cached data for this layer representation.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Sub Update()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Render a cell
        ''' </summary>
        ''' <param name="g">The graphics to render onto.</param>
        ''' <param name="rc">The area to render into.</param>
        ''' <param name="iSymbol">The <see cref="nSymbols">symbol</see> to render.
        ''' If left at 0 the default cell value should be drawn.</param>
        ''' -----------------------------------------------------------------------
        Public MustOverride Sub RenderPreview(ByVal g As Graphics,
                                              ByVal rc As Rectangle,
                                              Optional iSymbol As Integer = 0)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Render a layer onto a graphics context
        ''' </summary>
        ''' <param name="g">The graphics to render onto.</param>
        ''' <param name="rc">Device area to render cell onto.</param>
        ''' <param name="layer">The layer to render.</param>
        ''' <param name="ptfTL">Top-left coordinate represented by the device area.</param>
        ''' <param name="ptfBR">Bottom-right coordinate represented by the device area.</param>
        ''' <param name="style">Layer style to use when rendering/</param>
        ''' -----------------------------------------------------------------------
        Public MustOverride Sub Render(ByVal g As Graphics,
                                       ByVal layer As cDisplayLayer,
                                       ByVal rc As Rectangle,
                                       ByVal ptfTL As PointF,
                                       ByVal ptfBR As PointF,
                                       ByVal style As cStyleGuide.eStyleFlags)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' States whether the current visual style is valid
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overridable Function IsStyleValid() As Boolean
            Return (Me.VisualStyle IsNot Nothing)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Render a cell in error.
        ''' </summary>
        ''' <param name="g">Graphics device to render onto.</param>
        ''' <param name="rc">Area to render to.</param>
        ''' -----------------------------------------------------------------------
        Protected Sub RenderError(ByVal g As Graphics, ByVal rc As Rectangle)
            'g.FillRectangle(Brushes.White, rc)
            g.DrawLine(Pens.Red, rc.Left, rc.Top, rc.Right, rc.Bottom)
            g.DrawLine(Pens.Red, rc.Left, rc.Bottom, rc.Right, rc.Top)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create a shallow copy.
        ''' </summary>
        ''' <returns>A shallow copy.</returns>
        ''' -----------------------------------------------------------------------
        Public Overridable Function Clone() As cRasterLayerRenderer
            Dim minime As cRasterLayerRenderer = Nothing
            Dim vs As cVisualStyle = Me.VisualStyle.Clone()

            minime = DirectCast(Activator.CreateInstance(Me.GetType(), New Object() {vs}), cRasterLayerRenderer)
            minime.VisualStyleFlags = Me.VisualStyleFlags

            Return minime
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the layer is visible.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property IsVisible() As Boolean = True

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the scale max value to render to.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property ScaleMax() As Single = cCore.NULL_VALUE

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the scale min value to render to.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property ScaleMin() As Single = cCore.NULL_VALUE

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the display text for a given cell in the underlying data.
        ''' </summary>
        ''' <returns>The display text for a given cell in the underlying data.</returns>
        ''' -----------------------------------------------------------------------
        Public MustOverride Function GetDisplayText(value As Object) As String

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set how the <see cref="eLayerRenderType">layer should be drawn</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property RenderMode As eLayerRenderType = eLayerRenderType.Selected

#Region " Symbols "

        Private bSymbolsValid As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a layer-specific symbol.
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="i"></param>
        ''' -------------------------------------------------------------------
        Protected Sub AddSymbol(strName As String, i As Integer)
            Me.m_lSymbols.Add(New cSymbol(strName, i))
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of symbols that this renderer uses and will need displaying 
        ''' in legends.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property nSymbols As Integer
            Get
                If Not bSymbolsValid Then Me.UpdateSymbols()
                Return Me.m_lSymbols.Count
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name for a layer-specific symbol.
        ''' </summary>
        ''' <param name="iSymbol">One-based symbol index</param>
        ''' <returns>An empty string if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SymbolName(iSymbol As Integer) As String
            Get
                If Not bSymbolsValid Then Me.UpdateSymbols()
                If (iSymbol < 1) Or iSymbol > Me.m_lSymbols.Count Then Return ""
                Return Me.m_lSymbols(iSymbol - 1).Name
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the key for a layer-specific symbol.
        ''' </summary>
        ''' <param name="iSymbol">One-based symbol index</param>
        ''' <returns>0 if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SymbolKey(iSymbol As Integer) As Integer
            Get
                If Not bSymbolsValid Then Me.UpdateSymbols()
                If (iSymbol < 1) Or iSymbol > Me.m_lSymbols.Count Then Return 0
                Return Me.m_lSymbols(iSymbol - 1).Key
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Invalidate any symbols already registered. To update symbols override
        ''' <see cref="UpdateSymbols()"/> 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Sub InvalidateSymbols()
            Me.m_lSymbols.Clear()
            Me.bSymbolsValid = False
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Override to <see cref="AddSymbol(String, Integer)">add layer-specific
        ''' symbols</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub UpdateSymbols()
            Me.bSymbolsValid = True
        End Sub

#End Region ' Symbols

    End Class

End Namespace
