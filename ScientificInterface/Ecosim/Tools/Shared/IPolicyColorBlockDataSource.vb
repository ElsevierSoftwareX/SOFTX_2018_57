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
Option Explicit On

Imports EwECore
Imports EwECore.FishingPolicy
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core
Imports System.ComponentModel
Imports EwEUtils.Utilities

#End Region

Namespace Ecosim

#Region "Interface for Datasource (IPolicyColorBlockDataSource)"

    ''' <summary>
    ''' Interface for the core data that is used to populate a ucPolicyColorBlocks control
    ''' </summary>
    ''' <remarks>Different types of core data use the ucPolicyColorBlocks differently this allows the core data to all look the same to the control. </remarks>
    Public Interface IPolicyColorBlockDataSource

        ''' <summary>
        ''' Values used to color the grid
        ''' </summary>
        ''' <value>2d matrix of integers dimensiond be TotalBlocks and nRows</value>
        ''' <remarks>The Values in the grid are maintained by the datasource</remarks>
        ReadOnly Property BlockCells() As Integer(,)

        ''' <summary>
        ''' Total number of columns/year in the grid/data.
        ''' </summary>
        ''' <value>Integer</value>
        ''' <remarks>This is the X axis of the grid. Number of years from the data source.</remarks>
        ReadOnly Property TotalBlocks() As Integer
        ''' <summary>
        ''' Total number of rows in the grid
        ''' </summary>
        ''' <remarks>In the data source this is the number of group/fleets</remarks>
        ReadOnly Property nRows() As Integer

        ''' <summary>
        ''' Labels for the rows
        ''' </summary>
        ''' <param name="iRow">One based index of the row</param>
        ''' <remarks>Group names or Fleet names depending on the data source</remarks>
        ReadOnly Property RowLabel(ByVal iRow As Integer) As String

        ''' <summary>
        ''' Turns Off/On core updates while adding values to core data
        ''' </summary>
        ''' <remarks>True in batch edit mode and the core updated. False updates the core with the edits.</remarks>
        Property BatchEdit() As Boolean

        ''' <summary>
        ''' Does this data source use the Control panel/block-sequence selector
        ''' </summary>
        ReadOnly Property isControlPanelVisible() As Boolean

        ''' <summary>
        ''' Attach an <see cref="IBlockSelector">IBlockSelector</see> object to this data source 
        ''' </summary>
        ''' <param name="Blocks">implementation of IBlockSelector</param>
        ''' <remarks>The data source need to listen to the Block selector and set the number of blocks and cv values</remarks>
        Sub Attach(ByVal Blocks As IBlockSelector)

        ''' <summary>
        ''' Init the data source
        ''' </summary>
        Sub Init()

        ''' <summary>
        ''' Fills the BlockCells with the currently selected block and updates the core values
        ''' </summary>
        ''' <param name="iRow">Row</param>
        ''' <param name="iCol">Column</param>
        Sub FillBlock(ByVal iRow As Integer, ByVal iCol As Integer)

        ''' <summary>
        ''' Sets a sequence of BlockCells
        ''' </summary>
        ''' <param name="startYear">Year for the first block to fill</param>
        ''' <param name="endYear">End of the sequence</param>
        ''' <param name="yearPerBlock">Number of years per unique block</param>
        Sub SetSeqColorCodes(ByVal startYear As Integer, ByVal endYear As Integer, ByVal yearPerBlock As Integer)

        ''' <summary>
        ''' Update the core data 
        ''' </summary>
        ''' <remarks>Use when the BlockSelector has changed the values of blocks.</remarks>
        Sub Update()

        ''' <summary>
        ''' Return the value of a Block
        ''' </summary>
        ''' <param name="iBlock">Block index/value</param>
        Function BlockToValue(ByVal iBlock As Integer) As Single

    End Interface

#End Region

End Namespace