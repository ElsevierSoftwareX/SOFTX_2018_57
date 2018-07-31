'==============================================================================
'
' $Log: gridReadLayers.vb,v $
' Revision 1.2  2008/11/07 23:53:13  jeroens
' Functional v1 - still quite blunt
'
' Revision 1.1  2008/11/07 08:15:18  jeroens
' Initial version
'
'==============================================================================

#Region " Imports "

Option Strict On

Imports EwECore
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports SAUPUtil.SAUPData
Imports SourceGrid2
Imports System.Windows.Forms

#End Region ' Imports

Namespace Ecospace

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Grid showing layer to spatial attribute mappings.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class gridReadLayers
        Inherits EwEGrid

        ' ToDo: Sort and display layers by group
        ' ToDo: Accept Attributes as delivered by SAUPUtil so datatype can be verified
        ' ToDo: Do not allow incompatible data types to be linked

#Region " Private vars "

        ''' <summary>Custom <see cref="BehaviorModels.IBehaviorModel">behaviour model</see>
        ''' to trap cell edit events locally in this grid.</summary>
        Private m_bm As BehaviorModels.IBehaviorModel = New EndEditHandler(Me)

        ''' <summary>The layers to map upon.</summary>
        Private m_aLayers As cLayer()
        ''' <summary>The attribute names to map upon.</summary>
        Private m_astrAttributes As String()
        ''' <summary>Mappings. MAPPINGS!</summary>
        Private m_dtLayerMapping As New Dictionary(Of cLayer, String)

        Private Enum eColumnTypes As Integer
            ColumnLayer = 0
            ColumnAttribute
            ' Show datatype columns?
        End Enum

#End Region ' Private vars

#Region " Construction "

        Public Sub New()

        End Sub

#End Region ' Construction

#Region " Public interfaces "

        Public Property Layers() As cLayer()
            Get
                Return Nothing
            End Get
            Set(ByVal value As cLayer())
                Me.m_aLayers = value
                Me.RefreshContent()
            End Set
        End Property

        Public Property Attributes() As String()
            Get
                Return Me.m_astrAttributes
            End Get
            Set(ByVal value As String())
                Me.m_astrAttributes = value
                Me.RefreshContent()
            End Set
        End Property

        Public Function Mappings() As Dictionary(Of cLayer, String)
            Return Me.m_dtLayerMapping
        End Function

#End Region ' Public interfaces

#Region " Overrides "

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            If Not Me.HasData() Then Return

            Me.Redim(1, System.Enum.GetValues(GetType(eColumnTypes)).Length)

            ' ToDo_JS: Globalize this
            Me(0, eColumnTypes.ColumnLayer) = New EwEColumnHeaderCell("Layer")
            Me(0, eColumnTypes.ColumnAttribute) = New EwEColumnHeaderCell("Attribute")

            Me.Columns(eColumnTypes.ColumnLayer).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.Columns(eColumnTypes.ColumnAttribute).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.AutoStretchColumnsToFitWidth = True
            Me.FixedColumns = 1

        End Sub

        Protected Overrides Sub FillData()

            If Not Me.HasData Then Return

            Me.RowsCount = 1

            Dim layer As cLayer = Nothing
            Dim ewec As EwECell = Nothing
            Dim cmb As Cells.Real.ComboBox = Nothing

            For iLayer As Integer = 0 To Me.m_aLayers.Length - 1

                Me.AddRow()
                layer = Me.m_aLayers(iLayer)

                ewec = New EwECell(layer.Name, GetType(String))
                ewec.Style = (StyleGuide.eStyleFlags.Names Or StyleGuide.eStyleFlags.NotEditable)
                Me(iLayer + 1, eColumnTypes.ColumnLayer) = ewec

                cmb = New Cells.Real.ComboBox("", GetType(String), Me.m_astrAttributes, True)
                cmb.EditableMode = EditableMode.SingleClick
                Me(iLayer + 1, eColumnTypes.ColumnAttribute) = cmb
                Me(iLayer + 1, eColumnTypes.ColumnAttribute).Behaviors.Add(m_bm)

                Me.Rows(iLayer + 1).Tag = layer

            Next iLayer

            Me.UpdateMappingsColumn()

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Function DefaultDockStyle() As System.Windows.Forms.DockStyle
            Return Windows.Forms.DockStyle.None
        End Function

        Protected Overrides Function OnCellEdited(ByVal p As SourceGrid2.Position, ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean

            Dim strAttribute As String = Me.AttributeAtRow(p.Row)
            Dim layer As cLayer = Me.LayerAtRow(p.Row)

            Try
                ' ToDo: Clear existing mappings to this attribute?
                Me.m_dtLayerMapping(layer) = strAttribute
                Me.UpdateMappingsColumn()
            Catch ex As Exception
            End Try

            Return True

        End Function

        Private Sub UpdateMappingsColumn()

            Dim layer As cLayer = Nothing
            Dim strAttribute As String = ""
            Dim cmb As Cells.Real.ComboBox = Nothing
            Dim dm As DataModels.EditorComboBox = Nothing

            For iRow As Integer = 1 To Me.RowsCount - 1

                layer = Me.LayerAtRow(iRow)

                cmb = DirectCast(Me(iRow, eColumnTypes.ColumnAttribute), Cells.Real.ComboBox)
                dm = DirectCast(cmb.DataModel, DataModels.EditorComboBox)
                dm.DefaultValue = " "

                Try
                    cmb.Value = Me.m_dtLayerMapping(layer)
                Catch ex As Exception
                    cmb.Value = " "
                End Try

            Next iRow

        End Sub

        Private Function LayerAtRow(ByVal iRow As Integer) As cLayer
            If iRow > 0 And iRow < Me.RowsCount - 1 Then
                Return DirectCast(Me.Rows(iRow).Tag, cLayer)
            End If
            Return Nothing
        End Function

        Private Function AttributeAtRow(ByVal iRow As Integer) As String
            If iRow > 0 And iRow < Me.RowsCount - 1 Then
                Return CStr(Me(iRow, eColumnTypes.ColumnAttribute).Value)
            End If
            Return ""
        End Function

        Private Function HasData() As Boolean
            If Me.m_aLayers Is Nothing Then Return False
            If Me.m_astrAttributes Is Nothing Then Return False
            If Me.m_astrAttributes.Length <= 1 Then Return False
            Return True
        End Function

#End Region ' Overrides

    End Class

End Namespace ' Ecospace
