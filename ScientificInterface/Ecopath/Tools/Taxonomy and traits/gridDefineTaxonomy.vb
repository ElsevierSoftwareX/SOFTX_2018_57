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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Grid class implementing the Edit Group Taxon interface grid bit.
''' </summary>
''' -----------------------------------------------------------------------
<CLSCompliant(False)> _
Public Class gridDefineTaxonomy
    Inherits EwEGrid

#Region " Privates "

    Private m_bShowCodes As Boolean = False

    ''' <summary>List of active taxa.</summary>
    Private m_lTaxonInfo As New List(Of cTaxonInfo)
    ''' <summary>List of removed taxa.</summary>
    Private m_lTaxonInfoRemoved As New List(Of cTaxonInfo)

    ''' <summary>Search term for public use.</summary>
    Private m_tiSearch As ITaxonSearchData = Nothing
    ''' <summary>Internal item linked to the search term.</summary>
    Private m_tiSearchLinked As ITaxonSearchData = Nothing

    ''' <summary>Enumerated type defining the columns in this grid.</summary>
    ''' <remarks>The logic that shows and hides code columns depends on the position 
    ''' of the status column, which is presumed to reside before the code columns.
    ''' Please do not alter the position of the status and code columns.</remarks>
    Private Enum eColumnTypes
        Hierarchy = 0
        Name
        Proportion
        Genus
        Species
        Family
        Order
        [Class]
        Phylum
        Status
        ' - Codes -
        CodeSAUP
        CodeFB
        CodeSLB
        CodeFAO
        CodeLSID
    End Enum

#End Region ' Privates

#Region " Private helper classes "

    Private Class cTaxonInfo
        Inherits cTaxonSearchData
        Implements ITaxonSearchData
        Implements ITaxonDetailsData

#Region " Private vars "

        ' JS cannot hang on to a cTaxon because the core may reload taxa amidst applying.
        ' Private m_taxon As cTaxon = Nothing
        Private m_iDBIDTaxon As Integer = cCore.NULL_VALUE
        Private m_iTaxon As Integer = -1

        ''' <summary>The status of a Layer in the interface.</summary>
        Private m_status As eItemStatusTypes = eItemStatusTypes.Original

#End Region ' Private vars

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an new taxon administrative unit for an existing group.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal group As cEcoPathGroupInput)
            MyBase.New("")
            Me.iGroup = group.Index
            Me.iStanza = 0
            Me.Common = group.Name
            Me.m_status = eItemStatusTypes.Added
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an new taxon administrative unit for an existing stanza.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal stanza As cStanzaGroup)
            MyBase.New("")
            Me.iGroup = 0
            Me.iStanza = stanza.Index
            Me.Common = stanza.Name
            Me.m_status = eItemStatusTypes.Added
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create an administrative unit for an existing taxon.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal taxon As cTaxon)
            MyBase.New(taxon.Source)
            Me.m_iDBIDTaxon = CInt(taxon.GetVariable(eVarNameFlags.DBID))
            Me.m_iTaxon = taxon.Index
            Me.iGroup = taxon.iGroup
            Me.iStanza = taxon.iStanza
            Me.Proportion = taxon.Proportion
            Me.CodeSAUP = taxon.CodeSAUP
            Me.CodeFB = taxon.CodeFishBase
            Me.CodeSLB = taxon.CodeSeaLifeBase
            Me.CodeFAO = taxon.CodeFAO
            Me.CodeLSID = taxon.CodeLSID
            Me.Common = taxon.Name
            Me.Class = taxon.Class
            Me.Order = taxon.Order
            Me.Family = taxon.Family
            Me.Genus = taxon.Genus
            Me.Species = taxon.Species
            Me.North = taxon.North
            Me.South = taxon.South
            Me.East = taxon.East
            Me.West = taxon.West
            Me.EcologyType = taxon.EcologyType
            Me.OccurrenceStatus = taxon.OccurrenceStatus
            Me.OrganismType = taxon.OrganismType
            Me.IUCNConservationStatus = taxon.IUCNConservationStatus
            Me.MeanLength = taxon.MeanLength
            Me.MaxLength = taxon.MaxLength
            Me.MeanWeight = taxon.MeanWeight
            Me.MeanLifespan = taxon.MeanLifespan
            Me.LastUpdated = taxon.LastUpdated
            Me.SourceKey = taxon.SourceKey
            Me.SearchFields = taxon.SearchFields
            Me.m_status = eItemStatusTypes.Original
        End Sub

        Public Sub New(ByVal taxon As ITaxonSearchData)
            MyBase.New(taxon.Source)
            Me.Update(taxon)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update this unit with new Taxonomy data.
        ''' </summary>
        ''' <param name="taxon"></param>
        ''' -------------------------------------------------------------------
        Public Sub Update(ByVal taxon As ITaxonSearchData)
            Me.CodeSAUP = taxon.CodeSAUP
            Me.CodeFB = taxon.CodeFB
            Me.CodeSLB = taxon.CodeSLB
            Me.CodeFAO = taxon.CodeFAO
            Me.CodeLSID = taxon.CodeLSID
            Me.Common = taxon.Common
            Me.Phylum = taxon.Phylum
            Me.Class = taxon.Class
            Me.Order = taxon.Order
            Me.Family = taxon.Family
            Me.Genus = taxon.Genus
            Me.Species = taxon.Species
            Me.North = taxon.North
            Me.South = taxon.South
            Me.East = taxon.East
            Me.West = taxon.West
            If TypeOf (taxon) Is ITaxonDetailsData Then
                Dim details As ITaxonDetailsData = DirectCast(taxon, ITaxonDetailsData)
                Me.EcologyType = details.EcologyType
                Me.OccurrenceStatus = details.OccurrenceStatus
                Me.OrganismType = details.OrganismType
                Me.IUCNConservationStatus = details.IUCNConservationStatus
                Me.MeanLength = details.MeanLength
                Me.MaxLength = details.MaxLength
                Me.MeanWeight = details.MeanWeight
                Me.MeanLifespan = details.MeanLifespan
                Me.SearchFields = details.SearchFields
                Me.LastUpdated = details.LastUpdated
            End If
            Me.SourceKey = taxon.SourceKey
            Me.Source = taxon.Source
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the DBID of the <see cref="cTaxon">EwE Taxonomy code</see> associated
        ''' with this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property TaxonID() As Integer
            Get
                Return Me.m_iDBIDTaxon
            End Get
        End Property

        ReadOnly Property TaxonIndex() As Integer
            Get
                Return Me.m_iTaxon
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the Taxonomy code is to be created.
        ''' </summary>
        ''' <returns>
        ''' True when Layer <see cref="Name">Name</see> value has changed.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function IsNew() As Boolean
            Return (Me.m_iDBIDTaxon = cCore.NULL_VALUE)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the administrative unit has changed.
        ''' </summary>
        ''' <returns>
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property IsChanged(ByVal taxon As cTaxon) As Boolean
            Get
                If (Me.IsNew()) Then Return False

                Debug.Assert(CInt(taxon.GetVariable(eVarNameFlags.DBID)) = Me.m_iDBIDTaxon)

                If (Math.Round(taxon.Proportion, 5) <> Math.Round(Me.Proportion, 5)) Then Return True
                If (taxon.iGroup <> Me.iGroup) Then Return True
                If (taxon.iStanza <> Me.iStanza) Then Return True
                If (taxon.CodeSAUP <> Me.CodeSAUP) Then Return True
                If (taxon.CodeFishBase <> Me.CodeFB) Then Return True
                If (taxon.CodeSeaLifeBase <> Me.CodeSLB) Then Return True
                If (String.Compare(taxon.CodeLSID, Me.CodeLSID) <> 0) Then Return True
                If (String.Compare(taxon.CodeFAO, Me.CodeFAO) <> 0) Then Return True

                If (String.Compare(taxon.Name, Me.Common) <> 0) Then Return True
                If (String.Compare(taxon.Phylum, Me.Phylum) <> 0) Then Return True
                If (String.Compare(taxon.Class, Me.Class) <> 0) Then Return True
                If (String.Compare(taxon.Order, Me.Order) <> 0) Then Return True
                If (String.Compare(taxon.Family, Me.Family) <> 0) Then Return True
                If (String.Compare(taxon.Genus, Me.Genus) <> 0) Then Return True
                If (String.Compare(taxon.Species, Me.Species) <> 0) Then Return True
                If (String.Compare(taxon.Source, Me.Source) <> 0) Then Return True

                Return False
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eItemStatusTypes">item status</see>
        ''' for the layer object.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Status() As eItemStatusTypes
            Get
                Return Me.m_status
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this layer is flagged for deletion. Toggling this flag
        ''' will update the <see cref="Status">Status</see> of the item.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property FlaggedForDeletion() As Boolean
            Get
                Return Me.m_status = eItemStatusTypes.Removed
            End Get
            Set(ByVal bDelete As Boolean)
                If Not Me.IsNew Then
                    If bDelete Then
                        Me.m_status = eItemStatusTypes.Removed
                    Else
                        Me.m_status = eItemStatusTypes.Original
                    End If
                Else
                    If bDelete Then
                        Me.m_status = eItemStatusTypes.Invalid
                    Else
                        Me.m_status = eItemStatusTypes.Added
                    End If
                End If
            End Set
        End Property

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If (obj Is Nothing) Then Return False
            If (TypeOf obj Is ITaxonSearchData) Then
                Dim t As ITaxonSearchData = DirectCast(obj, ITaxonSearchData)
                Return (t.CodeSAUP > 0 And t.CodeSAUP = Me.CodeSAUP) Or _
                       (t.CodeFB > 0 And t.CodeFB = Me.CodeFB) Or _
                       (t.CodeSLB > 0 And t.CodeSLB = Me.CodeSLB) Or _
                       (Not String.IsNullOrWhiteSpace(t.CodeLSID) And String.Compare(t.CodeLSID, Me.CodeLSID, True) = 0) Or _
                       (Not String.IsNullOrWhiteSpace(t.CodeFAO) And String.Compare(t.CodeFAO, Me.CodeFAO, True) = 0)
            End If
            Return MyBase.Equals(obj)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group index of this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property iGroup() As Integer = 0

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the stanza index of this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property iStanza() As Integer = 0

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group/stanza biomass proportion for this taxon.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Proportion() As Single = 1.0!

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group/stanza catch proportion for this taxon.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property PropCatch() As Single = 1.0


        Public Sub ApplyChanges(ByVal taxon As cTaxon)
            If Me.IsChanged(taxon) Then
                With taxon
                    .Name = Me.Common
                    .iGroup = Me.iGroup
                    .iStanza = Me.iStanza
                    .Proportion = Me.Proportion
                    .CodeSAUP = Me.CodeSAUP
                    .CodeFishBase = Me.CodeFB
                    .CodeSeaLifeBase = Me.CodeSLB
                    .CodeLSID = Me.CodeLSID
                    .CodeFAO = Me.CodeFAO
                    .Species = Me.Species
                    .Family = Me.Family
                    .Genus = Me.Genus
                    .Order = Me.Order
                    .Class = Me.Class
                    .Source = Me.Source
                    .SourceKey = Me.SourceKey
                    .North = Me.North
                    .West = Me.West
                    .East = Me.East
                    .South = Me.South
                    .EcologyType = Me.EcologyType
                    .IUCNConservationStatus = Me.IUCNConservationStatus
                    .OrganismType = Me.OrganismType
                    .OccurrenceStatus = Me.OccurrenceStatus
                    .MaxLength = Me.MaxLength
                    .MeanLength = Me.MeanLength
                    .MeanWeight = Me.MeanWeight
                    .MeanLifespan = Me.MeanLifespan
                    .LastUpdated = cDateUtils.DateToJulian()
                End With
            End If
        End Sub

    End Class

#End Region ' Private helper classes

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create the grid
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()

        MyBase.New()

    End Sub

    Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(value As ScientificInterfaceShared.Controls.cUIContext)
            Me.m_lTaxonInfo.Clear()
            If (value IsNot Nothing) Then
                ' Make snapshot of configuration 
                For iTaxon As Integer = 1 To value.Core.nTaxon
                    Dim ti As New cTaxonInfo(value.Core.Taxon(iTaxon))
                    Me.m_lTaxonInfo.Add(ti)
                Next
            End If
            MyBase.UIContext = value
        End Set
    End Property

#End Region ' Constructor

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the grid.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub InitStyle()

        MyBase.InitStyle()

        Me.Selection.SelectionMode = GridSelectionMode.Row
        Me.Selection.EnableMultiSelection = False

        Dim iNumCols As Integer = CInt(if(Me.m_bShowCodes, System.Enum.GetValues(GetType(eColumnTypes)).Length, eColumnTypes.Status + 1))

        ' Redim columns
        Me.Redim(1, iNumCols)

        ' Group index cell
        Me(0, eColumnTypes.Hierarchy) = New EwEColumnHeaderCell()
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(eVarNameFlags.Name)
        Me(0, eColumnTypes.Proportion) = New EwEColumnHeaderCell(eVarNameFlags.TaxonPropBiomass)
        Me(0, eColumnTypes.Phylum) = New EwEColumnHeaderCell(eVarNameFlags.Phylum)
        Me(0, eColumnTypes.Class) = New EwEColumnHeaderCell(eVarNameFlags.Class)
        Me(0, eColumnTypes.Order) = New EwEColumnHeaderCell(eVarNameFlags.Order)
        Me(0, eColumnTypes.Family) = New EwEColumnHeaderCell(eVarNameFlags.Family)
        Me(0, eColumnTypes.Genus) = New EwEColumnHeaderCell(eVarNameFlags.Genus)
        Me(0, eColumnTypes.Species) = New EwEColumnHeaderCell(eVarNameFlags.Species)
        Me(0, eColumnTypes.Status) = New EwEColumnHeaderCell(SharedResources.HEADER_STATUS)

        If (Me.m_bShowCodes) Then
            Me(0, eColumnTypes.CodeFB) = New EwEColumnHeaderCell(eVarNameFlags.CodeFB)
            Me(0, eColumnTypes.CodeSLB) = New EwEColumnHeaderCell(eVarNameFlags.CodeSLB)
            Me(0, eColumnTypes.CodeSAUP) = New EwEColumnHeaderCell(eVarNameFlags.CodeSAUP)
            Me(0, eColumnTypes.CodeFAO) = New EwEColumnHeaderCell(eVarNameFlags.CodeFAO)
            Me(0, eColumnTypes.CodeLSID) = New EwEColumnHeaderCell(eVarNameFlags.CodeLSID)
        End If

        Me.FixedColumns = 1
        Me.FixedColumnWidths = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to first create a snapshot of the group taxon configuration
    ''' in the current EwE model. The grid will be populated from this local
    ''' administration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub FillData()

        Dim stz As cStanzaGroup = Nothing
        Dim grp As cEcoPathGroupInput = Nothing
        Dim abStanzaHandled(Me.Core.nStanzas) As Boolean
        Dim iRow As Integer = 0
        Dim hgcParent As EwEHierarchyGridCell = Nothing
        Dim taxon As cTaxon = Nothing
        Dim ti As cTaxonInfo = Nothing

        ' Populate local administration from a snapshot of the live data

        Me.NormalizeProportions()

        ' Create rows
        Me.RowsCount = 1

        For iGroup As Integer = 1 To Me.Core.nGroups

            grp = Me.Core.EcoPathGroupInputs(iGroup)
            If grp.IsMultiStanza Then

                If Not abStanzaHandled(grp.iStanza) Then
                    ' Create row for multi-stanza group
                    iRow = Me.AddRow()
                    stz = Me.Core.StanzaGroups(grp.iStanza)

                    hgcParent = New EwEHierarchyGridCell()
                    hgcParent.Tag = stz

                    Me(iRow, eColumnTypes.Hierarchy) = hgcParent
                    Me(iRow, eColumnTypes.Name) = New PropertyRowHeaderParentCell(Me.PropertyManager, stz, eVarNameFlags.Name, Nothing, hgcParent)
                    For iCol As Integer = eColumnTypes.Name + 1 To Me.ColumnsCount - 1
                        Me(iRow, iCol) = New EwERowHeaderCell("")
                    Next

                    For iTaxon As Integer = 0 To Me.m_lTaxonInfo.Count - 1
                        ti = Me.m_lTaxonInfo(iTaxon)
                        If ti.iStanza = stz.Index Then
                            Me.AddTaxonRow(ti, iRow)
                        End If
                    Next
                    abStanzaHandled(grp.iStanza) = True

                End If
            Else
                ' Add regular group row
                iRow = Me.AddRow()

                hgcParent = New EwEHierarchyGridCell()
                hgcParent.Tag = grp

                Me(iRow, eColumnTypes.Hierarchy) = hgcParent
                Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(String.Format(SharedResources.GENERIC_LABEL_INDEXED, grp.Index, grp.Name))
                For iCol As Integer = eColumnTypes.Name + 1 To Me.ColumnsCount - 1
                    Me(iRow, iCol) = New EwERowHeaderCell("")
                Next

                For iTaxon As Integer = 0 To Me.m_lTaxonInfo.Count - 1
                    ti = Me.m_lTaxonInfo(iTaxon)
                    If ti.iGroup = grp.Index Then
                        Me.AddTaxonRow(ti, iRow)
                    End If
                Next
            End If
        Next

        ' Populate rows
        For iRow = 1 To Me.RowsCount - 1
            Me.UpdateRow(iRow)
        Next iRow
    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()

        Dim bShowCol As Boolean = True

        For iCol As Integer = 1 To Me.ColumnsCount - 1
            Select Case DirectCast(iCol, eColumnTypes)
                Case eColumnTypes.Hierarchy
                    Me.Columns(iCol).Width = 20
                Case Else
                    Me.Columns(iCol).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
                    Me.AutoSizeColumn(iCol, 40)
            End Select
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; called when a cell has received focus. Overriden to notify
    ''' our parent that the selection has changed.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnCellGotFocus(ByVal e As SourceGrid2.PositionCancelEventArgs)
        MyBase.OnCellGotFocus(e)
        Me.RaiseSelectionChangeEvent()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; called when a cell has lost focus. Overriden to notify
    ''' our parent that the selection has changed.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnCellLostFocus(ByVal e As SourceGrid2.PositionCancelEventArgs)
        MyBase.OnCellLostFocus(e)
        Me.Selection.Clear()
        Me.RaiseSelectionChangeEvent()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Refresh the content of the Row with the given index.
    ''' </summary>
    ''' <param name="iRow">The index of the row to refresh.</param>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateRow(ByVal iRow As Integer)

        Dim ti As cTaxonInfo = Me.TaxonInfo(iRow)
        Dim dt As Date = Nothing
        Dim strText As String = ""
        Dim iNumOpen As Integer = 0

        If ti Is Nothing Then Return

        Me(iRow, eColumnTypes.Name).Value = ti.Common
        Me(iRow, eColumnTypes.Class).Value = ti.Class
        Me(iRow, eColumnTypes.Order).Value = ti.Order
        Me(iRow, eColumnTypes.Family).Value = ti.Family
        Me(iRow, eColumnTypes.Genus).Value = ti.Genus
        Me(iRow, eColumnTypes.Species).Value = ti.Species
        Me(iRow, eColumnTypes.Proportion).Value = ti.Proportion

        If (Me.m_bShowCodes) Then
            Me(iRow, eColumnTypes.CodeSAUP).Value = ti.CodeSAUP
            Me(iRow, eColumnTypes.CodeFB).Value = ti.CodeFB
            Me(iRow, eColumnTypes.CodeSLB).Value = ti.CodeSLB
            Me(iRow, eColumnTypes.CodeFAO).Value = ti.CodeFAO
            Me(iRow, eColumnTypes.CodeLSID).Value = ti.CodeLSID
        End If

        Me(iRow, eColumnTypes.Status).Value = ti.Status

    End Sub

    Private Function FindParentRow(ByVal iRow As Integer) As Integer
        If iRow < 1 Then Return -1
        While (iRow > 0) And Not (TypeOf Me(iRow, eColumnTypes.Hierarchy) Is EwEHierarchyGridCell)
            iRow -= 1
        End While
        Return iRow
    End Function

    Private Function AddTaxonRow(ByVal ti As cTaxonInfo, Optional ByVal iRow As Integer = -1) As Integer

        Dim cell As EwECell = Nothing

        If iRow = -1 Then
            iRow = Me.FindParentRow(Me.SelectedRow)
        End If

        Dim hgcParent As EwEHierarchyGridCell = DirectCast(Me(iRow, eColumnTypes.Hierarchy), EwEHierarchyGridCell)
        iRow += hgcParent.NumChildRows + 1
        Me.Rows.Insert(iRow)
        Me(iRow, eColumnTypes.Hierarchy) = New EwERowHeaderCell()
        Me(iRow, eColumnTypes.Hierarchy).Tag = ti
        Me(iRow, eColumnTypes.Name) = New EwECell(ti.Common, GetType(String))
        Me(iRow, eColumnTypes.Name).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Species) = New EwECell(ti.Species, GetType(String), cStyleGuide.eStyleFlags.Taxon)
        Me(iRow, eColumnTypes.Species).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Genus) = New EwECell(ti.Genus, GetType(String), cStyleGuide.eStyleFlags.Taxon)
        Me(iRow, eColumnTypes.Genus).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Family) = New EwECell(ti.Family, GetType(String))
        Me(iRow, eColumnTypes.Family).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Order) = New EwECell(ti.Order, GetType(String))
        Me(iRow, eColumnTypes.Order).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Class) = New EwECell(ti.Class, GetType(String))
        Me(iRow, eColumnTypes.Class).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Phylum) = New EwECell(ti.Phylum, GetType(String))
        Me(iRow, eColumnTypes.Phylum).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Proportion) = New EwECell(ti.Proportion, GetType(Single))
        Me(iRow, eColumnTypes.Proportion).Behaviors.Add(Me.EwEEditHandler)
        Me(iRow, eColumnTypes.Status) = New EwEStatusCell(eItemStatusTypes.Original)

        If (Me.m_bShowCodes) Then

            ' == CODE cells
            'Me(iRow, eColumnTypes.Code) = New EwECell(ti.SourceKey, GetType(String), cStyleGuide.eStyleFlags.NotEditable)

            cell = New EwECell(ti.CodeFB, GetType(Long))
            cell.SuppressZero(CLng(cCore.NULL_VALUE)) = True
            Me(iRow, eColumnTypes.CodeFB) = cell

            cell = New EwECell(ti.CodeSLB, GetType(Long))
            cell.SuppressZero(CLng(cCore.NULL_VALUE)) = True
            Me(iRow, eColumnTypes.CodeSLB) = cell

            cell = New EwECell(ti.CodeSAUP, GetType(Long))
            cell.SuppressZero(CLng(cCore.NULL_VALUE)) = True
            Me(iRow, eColumnTypes.CodeSAUP) = cell

            Me(iRow, eColumnTypes.CodeFAO) = New EwECell(ti.CodeFAO, GetType(String))
            Me(iRow, eColumnTypes.CodeLSID) = New EwECell(ti.CodeLSID, GetType(String))

        End If

        hgcParent.AddChildRow(iRow)
        Me.UpdateRow(iRow)

    End Function

    Private Sub RemoveTaxonRow(ByVal iRow As Integer)
        If iRow <= 0 Then iRow = Me.SelectedRow
        Dim iRowParent As Integer = Me.FindParentRow(iRow)
        If iRowParent >= 1 Then
            Dim hgcParent As EwEHierarchyGridCell = DirectCast(Me(iRowParent, eColumnTypes.Hierarchy), EwEHierarchyGridCell)
            hgcParent.RemoveChildRow(iRow)
            Me.Rows.Remove(iRow)
        End If
    End Sub

    Public Sub UpdateProportions()
        For iRow As Integer = 1 To Me.RowsCount - 1
            Dim ti As cTaxonInfo = Me.TaxonInfo(iRow)
            If ti IsNot Nothing Then
                Me(iRow, eColumnTypes.Proportion).Value = ti.Proportion
            End If
        Next
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Called when the user has finished editing a cell. Handled to update 
    ''' local admin based on cell value changes.
    ''' </summary>
    ''' <returns>
    ''' True if the edit operation is allowed, False to cancel the edit operation.
    ''' </returns>
    ''' <remarks>
    ''' This method differs from OnCellValueChanged; at the end of an edit
    ''' operation it is once again safe to alter the value of the cell that was
    ''' just edited for text and combo box controls. *sigh*
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function OnCellEdited(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

        Dim ti As cTaxonInfo = Me.TaxonInfo(p.Row)
        If ti Is Nothing Then Return False

        Dim val As Object = Me(p.Row, p.Column).Value

        Select Case DirectCast(p.Column, eColumnTypes)
            Case eColumnTypes.Name : ti.Common = CStr(val)
            Case eColumnTypes.Class : ti.Class = CStr(val)
            Case eColumnTypes.Family : ti.Family = CStr(val)
            Case eColumnTypes.Order : ti.Order = CStr(val)
            Case eColumnTypes.Genus : ti.Genus = CStr(val)
            Case eColumnTypes.Species : ti.Species = CStr(val)
            Case eColumnTypes.Phylum : ti.Phylum = CStr(val)
            Case eColumnTypes.Proportion : ti.Proportion = CSng(val)
                'Case eColumnTypes.Code : ti.CodeTaxon = CStr(val)
            Case eColumnTypes.CodeFB : ti.CodeFB = CLng(val)
            Case eColumnTypes.CodeSLB : ti.CodeSLB = CLng(val)
            Case eColumnTypes.CodeSAUP : ti.CodeSAUP = CLng(val)
            Case eColumnTypes.CodeFAO : ti.CodeFAO = CStr(val)
            Case eColumnTypes.CodeLSID : ti.CodeLSID = CStr(val)
        End Select

        ' Perhaps redundant but hey
        Me.UpdateRow(p.Row)

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, obtains the taxon info for a given row.
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <returns>A cTaxonInfo instance, or nothing if the row did not contain
    ''' a taxoninfo link.</returns>
    ''' -----------------------------------------------------------------------
    Private Function TaxonInfo(ByVal iRow As Integer) As cTaxonInfo
        Dim tag As Object = Nothing
        If (iRow <= 1) Then Return Nothing
        tag = Me(iRow, eColumnTypes.Hierarchy).Tag
        If Not (TypeOf tag Is cTaxonInfo) Then Return Nothing
        Return DirectCast(tag, cTaxonInfo)
    End Function

#End Region ' Internals

#Region " Public bits "

#Region " Data "

    Public Property SelectedTaxon() As ITaxonSearchData
        Get
            Return Me.TaxonInfo(Me.SelectedRow)
        End Get
        Set(ByVal taxon As ITaxonSearchData)
            If Not (TypeOf taxon Is cTaxonInfo) Then Return
            For iRow As Integer = 1 To Me.RowsCount - 1
                If ReferenceEquals(TaxonInfo(iRow), taxon) Then
                    Me.SelectRow(iRow)
                    Return
                End If
            Next
        End Set
    End Property

    Public ReadOnly Property SelectedGroup() As cEcoPathGroupInput
        Get
            Dim iRowParent As Integer = Me.FindParentRow(Me.SelectedRow)
            Dim tag As Object = Nothing

            If (iRowParent < 1) Then Return Nothing

            tag = Me(iRowParent, eColumnTypes.Hierarchy).Tag
            If (TypeOf tag Is cEcoPathGroupInput) Then
                Return DirectCast(tag, cEcoPathGroupInput)
            End If
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property SelectedStanza() As cStanzaGroup
        Get
            Dim iRowParent As Integer = Me.FindParentRow(Me.SelectedRow)
            Dim tag As Object = Nothing

            If (iRowParent < 1) Then Return Nothing

            tag = Me(iRowParent, eColumnTypes.Hierarchy).Tag
            If (TypeOf tag Is cStanzaGroup) Then
                Return DirectCast(tag, cStanzaGroup)
            End If
            Return Nothing
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the various keys need to be shown in the grid.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ShowCodes As Boolean
        Get
            Return Me.m_bShowCodes
        End Get
        Set(value As Boolean)
            If (Me.m_bShowCodes <> value) Then
                Me.m_bShowCodes = value
                Me.RefreshContent()
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns an array of all available taxa.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Taxa() As ITaxonSearchData()
        Get
            Return Me.m_lTaxonInfo.ToArray
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a taxon for the selected group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub AddTaxon(Optional ByVal taxon As ITaxonSearchData = Nothing)

        If Not Me.CanAddTaxon(taxon) Then Return

        Dim ti As cTaxonInfo = Nothing
        Dim iRow As Integer = Nothing
        Dim grp As cEcoPathGroupInput = Me.SelectedGroup
        Dim stz As cStanzaGroup = Me.SelectedStanza

        If (taxon Is Nothing) Then
            If (grp Is Nothing) Then
                ti = New cTaxonInfo(stz)
            Else
                ti = New cTaxonInfo(grp)
            End If
        Else
            ti = New cTaxonInfo(taxon)
            If (grp Is Nothing) Then
                ti.iStanza = stz.Index
            Else
                ti.iGroup = grp.Index
            End If
        End If

        Me.m_lTaxonInfo.Add(ti)
        Me.AddTaxonRow(ti)
        Me.SelectedTaxon = ti

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether a taxon can be added to the current selected row.
    ''' </summary>
    ''' <param name="taxon"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' <para>The following rules are checked:</para>
    ''' <list type="bullet">
    ''' <item><description>A row must be selected</description></item>
    ''' <item><description>A stanza group can have only ONE taxon assigned</description></item>
    ''' <item><description>A taxon code can be used multiple times</description></item>
    ''' </list>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Function CanAddTaxon(Optional ByVal taxon As ITaxonSearchData = Nothing) As Boolean

        Dim grp As cEcoPathGroupInput = Me.SelectedGroup
        Dim stz As cStanzaGroup = Me.SelectedStanza

        Dim bIsTaxonUsed As Boolean = False
        Dim bStanzaHasTaxon As Boolean = False

        If (grp Is Nothing) And (stz Is Nothing) Then Return False

        For Each ti As cTaxonInfo In Me.m_lTaxonInfo
            'bIsTaxonUsed = bIsTaxonUsed Or (ti.Equals(taxon))
            If (stz IsNot Nothing) Then
                bStanzaHasTaxon = bStanzaHasTaxon Or (stz.Index = ti.iStanza)
            End If
        Next

        Return (Not bIsTaxonUsed) And (Not bStanzaHasTaxon)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the delete state of all selected rows.
    ''' </summary>
    ''' <param name="bDelete">The state to set rows to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub DeleteRows(bDelete As Boolean)

        Dim sel As Selection = Me.Selection
        Dim rows As RowInfo() = sel.SelectedRows
        Dim ti As cTaxonInfo = Nothing

        For Each row As RowInfo In rows
            Dim iRow As Integer = row.Index

            ti = Me.TaxonInfo(iRow)

            If (ti IsNot Nothing) Then
                ti.FlaggedForDeletion = bDelete

                ' Check to see what is to happen to the MPA now
                If (bDelete) Then
                    Select Case ti.Status

                        Case eItemStatusTypes.Original
                            ' Clear removed status 
                            Me.m_lTaxonInfoRemoved.Remove(ti)
                            Me.UpdateRow(iRow)

                        Case eItemStatusTypes.Added
                            ' Remove new item
                            Me.m_lTaxonInfo.Remove(ti)
                            Me.RemoveTaxonRow(iRow)

                        Case eItemStatusTypes.Removed
                            ' Set removed status
                            Me.m_lTaxonInfoRemoved.Add(ti)
                            Me.UpdateRow(iRow)

                        Case eItemStatusTypes.Invalid
                            ' Set removed status
                            Me.m_lTaxonInfo.Remove(ti)
                            Me.RemoveTaxonRow(iRow)

                    End Select
                Else
                    Me.UpdateRow(iRow)
                End If

            End If
        Next

    End Sub

    Public Function CanDeleteTaxon() As Boolean
        Return (Me.SelectedTaxon IsNot Nothing)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether the taxon info row is flagged for deletion.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IsFlaggedForDeletionRow() As Boolean
        Dim ti As cTaxonInfo = Me.TaxonInfo(Me.SelectedRow)
        If (ti Is Nothing) Then Return False
        Return ti.FlaggedForDeletion
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Refresh the grid row for the current selected taxon.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub UpdateSelectedTaxonRow()
        Me.UpdateRow(Me.SelectedRow())
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Populate the selected taxon with new data.
    ''' </summary>
    ''' <param name="taxon"></param>
    ''' -----------------------------------------------------------------------
    Public Sub UpdateSelectedTaxon(ByVal taxon As ITaxonSearchData)
        Dim ti As cTaxonInfo = Me.TaxonInfo(Me.SelectedRow)
        If (ti Is Nothing) Then Return
        ti.Update(taxon)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether a taxon has already been used.
    ''' </summary>
    ''' <param name="taxon"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function IsTaxonUsed(taxon As ITaxonSearchData) As Boolean
        For Each ti As cTaxonInfo In Me.m_lTaxonInfo
            If ti.Equals(taxon) Then
                Return True
            End If
        Next
        Return False
    End Function

#End Region ' Data

#Region " Search "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a search term for the current selected taxon.
    ''' </summary>
    ''' <param name="taxonSearch">Taxon to create a search term for.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function GetSearchTerm(Optional ByVal taxonSearch As ITaxonSearchData = Nothing) As ITaxonSearchData

        Me.m_tiSearchLinked = Me.SelectedTaxon

        If taxonSearch Is Nothing Then taxonSearch = Me.m_tiSearchLinked
        Me.m_tiSearch = New cTaxonInfo(taxonSearch)

        Return Me.m_tiSearch

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether a specific taxon is the last created search term.
    ''' </summary>
    ''' <param name="taxon"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function IsSearchTerm(ByVal taxon As ITaxonSearchData) As Boolean
        Return (ReferenceEquals(taxon, Me.m_tiSearch)) And _
               (ReferenceEquals(Me.SelectedTaxon, Me.m_tiSearchLinked))
    End Function

#End Region ' Search

#Region " Apply changes "

    Public Sub NormalizeProportions()

        Dim asTotalGroup(Me.Core.nGroups) As Single
        Dim aiTotalGroup(Me.Core.nGroups) As Integer
        Dim asTotalTaxon(Me.Core.nStanzas) As Single
        Dim aiTotalTaxon(Me.Core.nStanzas) As Integer

        Dim ti As cTaxonInfo = Nothing
        Dim iTaxon As Integer = 0

        For iTaxon = 0 To Me.m_lTaxonInfo.Count - 1
            ti = Me.m_lTaxonInfo(iTaxon)
            If (ti.Status <> eItemStatusTypes.Removed) Then
                If ti.iStanza > 0 Then
                    asTotalTaxon(ti.iStanza) += ti.Proportion
                    aiTotalTaxon(ti.iStanza) += 1
                Else
                    asTotalGroup(ti.iGroup) += ti.Proportion
                    aiTotalGroup(ti.iGroup) += 1
                End If
            End If
        Next

        For iTaxon = 0 To Me.m_lTaxonInfo.Count - 1
            ti = Me.m_lTaxonInfo(iTaxon)
            If (ti.Status <> eItemStatusTypes.Removed) Then
                If ti.iStanza > 0 Then
                    ' Has a total of 0?
                    If (asTotalTaxon(ti.iStanza) = 0.0!) Then
                        ' #Yes: redistribute values
                        ti.Proportion = 1.0! / aiTotalTaxon(ti.iStanza)
                    Else
                        ti.Proportion = ti.Proportion / asTotalTaxon(ti.iStanza)
                    End If
                Else
                    ' Has a total of 0?
                    If (asTotalGroup(ti.iGroup) = 0.0!) Then
                        ' #Yes: redistribute values
                        ti.Proportion = 1.0! / aiTotalGroup(ti.iGroup)
                    Else
                        ti.Proportion = ti.Proportion / asTotalGroup(ti.iGroup)
                    End If
                End If
            End If
        Next
        Me.UpdateProportions()

    End Sub

    Public Function Apply() As Boolean

        Dim bConfigurationChanged As Boolean = False
        Dim ti As cTaxonInfo = Nothing
        Dim taxon As cTaxon = Nothing
        Dim iTaxon As Integer = 0
        Dim bSuccess As Boolean = True

        ' Assess Taxon changes
        For iTaxon = 0 To Me.m_lTaxonInfo.Count - 1
            ti = DirectCast(Me.m_lTaxonInfo(iTaxon), cTaxonInfo)
            ' Check this Taxon is newly added
            If ti.IsNew Then
                bConfigurationChanged = True
                Exit For
            Else
                ' Check if this Taxon is an existing Taxon that has been moved
                If ((iTaxon + 1) <> ti.TaxonIndex) Then
                    bConfigurationChanged = True
                    Exit For
                End If
            End If
        Next iTaxon

        ' Assess Taxons to remove
        If (Me.m_lTaxonInfoRemoved.Count > 0) Then

            Dim fmsg As New cFeedbackMessage(My.Resources.TAXON_DELETE_CONFIRMATION, eCoreComponentType.EcoPath, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            Me.UIContext.Core.Messages.SendMessage(fmsg)

            Select Case fmsg.Reply
                Case eMessageReply.NO
                    ' Abort
                    Return False
                Case eMessageReply.YES
                    ' Delete this Taxon
                    bConfigurationChanged = True
                Case Else
                    ' Unexpected anwer: assert
                    Debug.Assert(False)
            End Select
        End If

        ' Handle added and removed items
        If (bConfigurationChanged) Then

            If Not Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure) Then Return False

            cApplicationStatusNotifier.StartProgress(Me.Core, SharedResources.GENERIC_STATUS_APPLYCHANGES)

            Dim htTaxonID As New Dictionary(Of cTaxonInfo, Integer)
            Dim iDBID As Integer = Nothing

            Try

                ' Add new Taxons
                For iTaxon = 0 To Me.m_lTaxonInfo.Count - 1
                    ti = Me.m_lTaxonInfo(iTaxon)
                    If (ti.IsNew) Then
                        bSuccess = bSuccess And Me.Core.AddTaxon(Math.Max(ti.iGroup, ti.iStanza), (ti.iStanza > 0), ti, ti.Proportion, iDBID)
                        ' Map this new ID during update
                        htTaxonID.Add(ti, iDBID)
                    End If
                Next

                ' Remove deleted Taxons
                For Each ti In Me.m_lTaxonInfoRemoved
                    bSuccess = bSuccess And Me.Core.RemoveTaxon(ti.TaxonIndex)
                Next
                Me.m_lTaxonInfoRemoved.Clear()

            Catch ex As Exception

            End Try

            ' The core will reload now
            Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecopath)
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End If

        ' Update any changed taxa
        Dim dtTaxa As New Dictionary(Of Integer, cTaxon)
        For i As Integer = 1 To Me.Core.nTaxon
            taxon = Me.Core.Taxon(i)
            dtTaxa(CInt(taxon.GetVariable(eVarNameFlags.DBID))) = taxon
        Next

        For Each ti In Me.m_lTaxonInfo
            If (Not ti.IsNew And Not ti.FlaggedForDeletion) Then
                ti.ApplyChanges(dtTaxa(ti.TaxonID))
            End If
        Next

        Return bSuccess

    End Function

#End Region ' Apply changes

#End Region ' Public bits

End Class
