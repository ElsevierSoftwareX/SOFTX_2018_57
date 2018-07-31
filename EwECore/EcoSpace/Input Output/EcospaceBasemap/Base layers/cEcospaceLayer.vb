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
Imports EwECore.Core
Imports EwECore.SpatialData
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Base class for providing cell-based interaction with Ecospace data.
''' </summary>
''' <remarks>
''' <para>This class can be used in two ways:</para>
''' <para><list type="bullet">
''' <item><description>In conjunction with a manager, who will link this layer
''' to the actual data</description></item>
''' <item><description>Directly linked to a data array holding the data. In that
''' case the manager is obsolete.</description></item>
''' </list></para>
''' </remarks>
''' ---------------------------------------------------------------------------
Public MustInherit Class cEcospaceLayer
    Inherits cCoreInputOutputBase

#Region " Private variables "

    ''' <summary>Manager delivering the data, if any.</summary>
    Protected m_manager As IEcospaceLayerManager = Nothing
    ''' <summary>If set, this flag will direct the manager how to get to the actual map data.</summary>
    Protected m_vnData As eVarNameFlags = eVarNameFlags.NotSet
    ''' <summary>Metadata to restrict values that can enter a layer.</summary>
    Protected m_metadata As cVariableMetaData = Nothing
    ''' <summary>If set, a hard-linked reference to an array.</summary>
    Protected m_data As Object = Nothing
    ''' <summary>Type of the data.</summary>
    Protected m_typeValue As Type = Nothing
    ''' <summary>States whether cached statistics should be recalculated.</summary>
    ''' <remarks>True at startup to make sure that stats are properly calculated
    ''' when first queried.</remarks>
    Protected m_bInvalidateStats As Boolean = True

    Protected m_iSecundaryIndex As Integer = 1
    Protected m_ccSecundaryIndex As eCoreCounterTypes = eCoreCounterTypes.NotSet

#End Region ' Private variables

#Region " Constructors "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for definining a layer that dynamically obtains its data.
    ''' </summary>
    ''' <param name="core">The core to notify of changes.</param>
    ''' <param name="iDBID">Database ID to assign to the layer.</param>
    ''' <param name="manager">The manager providing data for this layer.</param>
    ''' <param name="vnData">The variable name identifying what data to obtain
    ''' from the manager.</param>
    ''' <param name="iIndex">Secundary index for obtaining the data.</param>
    ''' <param name="typeValue"><see cref="Type">Type</see> of layer values.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub New(ByVal core As cCore, _
                      ByVal iDBID As Integer, _
                      ByVal manager As IEcospaceLayerManager, _
                      ByVal strName As String, _
                      ByVal vnData As eVarNameFlags, _
                      ByVal iIndex As Integer, _
                      ByVal typeValue As Type)

        Me.New(core, iDBID, strName, typeValue, Nothing)

        Debug.Assert(vnData <> eVarNameFlags.NotSet)

        Me.AllowValidation = False

        ' Store details
        Me.m_manager = manager
        Me.m_vnData = vnData
        Me.Index = iIndex
        Me.m_ValidationStatus = New cVariableStatus()
        Me.m_ValidationStatus.CoreDataObject = Me

        If (TypeOf manager Is cCoreInputOutputBase) Then
            Me.m_metadata = DirectCast(manager, cCoreInputOutputBase).GetVariableMetadata(Me.m_vnData)
        End If

        Me.AllowValidation = True

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for defining a layer that is connected directly to its data.
    ''' </summary>
    ''' <param name="core">The core to notify of changes.</param>
    ''' <param name="data">The data to link to this layer.</param>
    ''' <param name="typeValue"><see cref="Type">Type</see> of layer values.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub New(ByVal core As cCore, _
                      ByVal data As Object, _
                      ByVal strName As String, _
                      ByVal typeValue As Type, _
                      Optional ByVal meta As cVariableMetaData = Nothing, _
                      Optional ByVal vn As eVarNameFlags = Nothing)

        Me.New(core, cCore.NULL_VALUE, strName, typeValue, meta)

        Me.m_data = data
        Me.m_vnData = vn
        Me.m_metadata = meta

    End Sub

    Private Sub New(ByVal core As cCore, _
                    ByVal iDBID As Integer, _
                    ByVal strName As String, _
                    ByVal typeValue As Type, _
                    ByVal meta As cVariableMetaData)

        MyBase.New(core)

        Dim val As cValue = Nothing

        Try
            Me.DBID = iDBID
            Me.m_dataType = eDataTypes.NotSet
            Me.m_coreComponent = eCoreComponentType.EcoSpace
            Me.m_metadata = meta
            Me.m_typeValue = typeValue

            Me.AllowValidation = False
            Me.Name = strName
            Me.AllowValidation = True

            Me.ResetStatusFlags()

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceLayer.")
            cLog.Write(Me.ToString & ".New(..) Error creating new cEcospaceLayer. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructors

#Region " Cell manipulation "

    Protected Function ValidateCellPosition(ByVal iRow As Integer, ByVal iCol As Integer) As Boolean
        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
        Return iRow > 0 And iRow <= bm.InRow And iCol > 0 And iCol <= bm.InCol
    End Function

    Protected MustOverride Function ValidateCellValue(ByVal value As Object) As Boolean

    Protected ReadOnly Property Data() As Object
        Get
            If (Me.m_data Is Nothing) Then
                Me.m_data = Me.m_manager.LayerData(Me.m_vnData, Me.Index)
            End If
            Return Me.m_data
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="IEcospaceLayerManager">manager</see> responsible for this layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Manager As IEcospaceLayerManager
        Get
            Return Me.m_manager
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="eVarNameFlags"/> for the variable of this layers' data
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property VarName() As eVarNameFlags
        Get
            Return Me.m_vnData
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="Type">type</see> of layer values.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ValueType() As Type
        Get
            Return Me.m_typeValue
        End Get
    End Property

    Public Overridable Property Description() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the value of a cell.
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <param name="iCol"></param>
    ''' <param name="iIndexSec"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public MustOverride Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the metadata associated with the values for a cell.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property MetadataCell() As cVariableMetaData
        Get
            Return Me.m_metadata
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the maximum value in a layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property MaxValue() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the minimum value in a layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property MinValue() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the number of value cells in the layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property NumValueCells As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Invalidates the content of a layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustOverride Sub Invalidate()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Secundary index type for layers that contain, for instance, monthly data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property SecundaryIndexCounter As eCoreCounterTypes
        Get
            Return Me.m_ccSecundaryIndex
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Secundary index for layers that contain, for instance, monthly data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property SecundaryIndex As Integer
        Get
            Return Me.m_iSecundaryIndex
        End Get
        Set(ByVal value As Integer)
            value = Math.Max(1, Math.Min(Me.m_core.GetCoreCounter(Me.SecundaryIndexCounter), value))
            If (value <> Me.m_iSecundaryIndex) Then
                Me.m_iSecundaryIndex = value
                Me.Invalidate()
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get if layer is receiving data from an external source.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsExternalData() As Boolean
        Get
            Dim man As SpatialData.cSpatialDataConnectionManager = Me.m_core.SpatialDataConnectionManager
            If (man Is Nothing) Then Return False
            Dim adapter As cSpatialDataAdapter = man.Adapter(Me.VarName)
            If (adapter Is Nothing) Then Return False
            Return adapter.IsConnected(Me.Index)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether this layer can be <see cref="IsActive">deactivated</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property CanDeactivate As Boolean
        Get
            Return False
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set if the layer is active. This can have different meanings for
    ''' different layers
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Property IsActive() As Boolean = True

#End Region ' Cell manipulation

#Region " Overrides "

    Public Overrides Function ToString() As String
        Return "cEcospaceLayer " & Me.m_vnData.ToString() ' Cannot show any variables here - may cause deadlocks
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to make sure there is always a name returned for a layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function GetVariable(VarName As eVarNameFlags,
                                          Optional iIndex As Integer = -9999,
                                          Optional iIndex2 As Integer = -9999,
                                          Optional iIndex3 As Integer = -9999) As Object

        If (VarName = eVarNameFlags.Name) Then
            Dim strName As String = CStr(MyBase.GetVariable(VarName, iIndex, iIndex2, iIndex3))
            If Not String.IsNullOrWhiteSpace(strName) Then
                Return strName
            Else
                Return Me.DefaultName
            End If
        End If
        Return MyBase.GetVariable(VarName, iIndex, iIndex2, iIndex3)

    End Function

    Public Overrides Function SetVariable(VarName As eVarNameFlags,
                                          newValue As Object, Optional iSecondaryIndex As Integer = -9999, Optional ByVal iThirdIndex As Integer = -9999) As Boolean
        If (VarName = eVarNameFlags.Name) Then
            If (Me.Index > 0) Then
                Try
                    Dim strName As String = CStr(newValue)
                    If String.Compare(strName, Me.DefaultName, True) = 0 Then
                        newValue = ""
                    End If
                Catch ex As Exception
                    ' FAll through to default handling
                End Try
            End If
        End If
        Return MyBase.SetVariable(VarName, newValue, iSecondaryIndex)
    End Function

    Protected Overridable Function DefaultName() As String
        ' ToDO: globalize this
        Return "Layer " & Me.Index
    End Function

#End Region ' Overrides

End Class

