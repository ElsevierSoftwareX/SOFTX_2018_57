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
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Layer that wraps a collection of <see cref="cEcospaceLayer"/>s for 
    ''' bundled display and processing in the UI. The indexing of the bundled
    ''' data is based on <see cref="eCoreCounterTypes"/>.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cDisplayLayerRasterBundle
        Inherits cDisplayLayerRaster

        Private m_layers As cEcospaceLayer()
        Private m_iLayer As Integer = 0
        Private m_cc As eCoreCounterTypes = eCoreCounterTypes.NotSet

        Public Sub New(ByVal uic As cUIContext,
                       ByVal data As cEcospaceLayer(),
                       ByVal renderer As cLayerRenderer,
                       ByVal editor As cLayerEditor,
                       ByVal cc As eCoreCounterTypes,
                       ByVal source As cCoreInputOutputBase,
                       Optional ByVal varName As eVarNameFlags = eVarNameFlags.Name,
                       Optional ByVal sValueSet As Single = cCore.NULL_VALUE,
                       Optional ByVal sValueClear As Single = cCore.NULL_VALUE)

            MyBase.New(uic, data(0), renderer, editor, source, varName, sValueSet, sValueClear)

            ' Sanity check
            Debug.Assert(cc <> eCoreCounterTypes.NotSet, "Cannot declare a layer bundle without providing a core counter that this bundle uses")

            Me.m_cc = cc

            ReDim Me.m_layers(uic.Core.GetCoreCounter(cc))
            For Each l As cEcospaceLayer In data
                Try
                    Me.m_layers(l.Index) = l
                Catch ex As Exception

                End Try
            Next

            For i As Integer = 0 To Me.m_layers.Length - 1
                If Me.m_layers(i) IsNot Nothing Then Me.m_iLayer = i : Exit For
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the index of the current active layer in the bundle.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property iLayer As Integer
            Get
                Return Me.m_iLayer
            End Get
            Set(value As Integer)
                Me.m_iLayer = Math.Max(0, Math.Min(value, Me.m_uic.Core.GetCoreCounter(Me.m_cc)))
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of layers in the bundle.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property nLayers As Integer
            Get
                Return Me.m_uic.Core.GetCoreCounter(Me.CoreCounter)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eCoreCounterTypes"/> that defines the indexing
        ''' of the layers bundled in this class.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property CoreCounter As eCoreCounterTypes
            Get
                Return Me.m_cc
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the Ecospace layer <see cref="iLayer">currently active</see> in
        ''' the bundle.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property Data As EwECore.cEcospaceLayer
            Get
                Return Me.Data(Me.m_iLayer)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get an Ecospace layer from the bundle.
        ''' </summary>
        ''' <param name="iLayer">The index of the layer to obtain. Note that this
        ''' value cannot exceed the range stipulted by the underlying <see cref="CoreCounter"/>.</param>
        ''' -------------------------------------------------------------------
        Public Overloads ReadOnly Property Data(ByVal iLayer As Integer) As EwECore.cEcospaceLayer
            Get
                Debug.Assert(iLayer <= Me.m_uic.Core.GetCoreCounter(Me.m_cc))
                Return Me.m_layers(iLayer)
            End Get
        End Property

        Public Overrides Property Name As String
            Get
                Dim fmtV As New cVarnameTypeFormatter()
                Dim l As cEcospaceLayer = Me.m_layers(Me.m_iLayer)
                Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, fmtV.GetDescriptor(l.VarName, eDescriptorTypes.Abbreviation), l.Name)
            End Get
            Set(value As String)
                Debug.Assert(False)
                ' Can't do this
            End Set
        End Property

    End Class ' Layer

End Namespace
