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
Imports ScientificInterfaceShared.Properties
Imports SourceGrid2.Cells.Real

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Cell class to implement a colunm header in an <see cref="EwEGrid">EWE grid</see>, 
    ''' that dynamically derives its <see cref="Cell.DisplayText">display text</see>
    ''' from the core.
    ''' </summary>
    ''' <remarks>
    ''' <para>This class inherits from <see cref="PropertyHeaderCell">PropertyHeaderCell</see> 
    ''' to implement basic, standardized formatting for column header cells, and
    ''' provides standardized grid column selection and sorting interaction. The
    ''' display text of the cell is tracked 'live' using <see cref="cProperty">properties</see>.</para>
    ''' <para>Additionally, the cell offers capabilities to incorporate units
    ''' that are updated whenever the system display units change.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class PropertyColumnHeaderCell
        : Inherits PropertyHeaderCell

        ''' <summary>One visualizer for all cells</summary>
        Private Shared g_visualizer As New cEwEGridColumnHeaderVisualizer()
        ''' <summary>Secundary property for monitoring tooltip.</summary>
        Private m_propTooltip As cProperty = Nothing

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor to create a column header cell that derives its 
        ''' <see cref="DisplayText">display text</see> from a 
        ''' <see cref="cProperty">cProperty</see> and a 
        ''' Both the property value and the unit mask text are inserted in the 
        ''' cell display text via a format mask.
        ''' </summary>
        ''' <param name="prop">cProperty to deliver the cell value.</param>
        ''' <param name="strUnit">Dynamic units to place in the cell
        ''' display text.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal prop As cProperty, Optional ByVal strUnit As String = "")
            MyBase.New(prop)
            Me.VisualModel = g_visualizer

            If (prop.VarName <> eVarNameFlags.Name) Then
                Dim pm As cPropertyManager = prop.PropertyManager
                Me.m_propTooltip = pm.GetProperty(prop.Source, eVarNameFlags.Name, prop.SourceSec)
                AddHandler Me.m_propTooltip.PropertyChanged, AddressOf OnPropertyChanged
                Me.UpdateTooltip()
            End If
            Me.SetUnits(strUnit)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor to create a column header cell that synchronizes 
        ''' its <see cref="DisplayText">display text</see> live with core data.
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see>
        ''' to extract data from.</param>
        ''' <param name="Source">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see> 
        ''' object to deliver the core data.</param>
        ''' <param name="VarName">The <see cref="eVarNameFlags">variable</see> 
        ''' of the <paramref name="Source">Source</paramref> to display in the cell.</param>
        ''' <param name="SourceSec">An optional secundary index in the 
        ''' <paramref name="VarName">variable</paramref>, or 
        ''' <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> when this variable
        ''' does not require an index.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal pm As cPropertyManager,
                       ByVal Source As cCoreInputOutputBase,
                       ByVal VarName As eVarNameFlags,
                       Optional ByVal SourceSec As cCoreInputOutputBase = Nothing,
                       Optional ByVal strUnit As String = "")
            Me.New(pm.GetProperty(Source, VarName, SourceSec), strUnit)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clean up!
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Dispose()

            ' Unregister property
            If (Me.m_propTooltip IsNot Nothing) Then
                RemoveHandler Me.m_propTooltip.PropertyChanged, AddressOf Me.OnPropertyChanged
                Me.m_propTooltip = Nothing
            End If

            MyBase.Dispose()

        End Sub

#End Region ' Construction 

#Region " Overrides "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Override to track property changes, may be needed to update
        ''' the text of the tooltip.
        ''' </summary>
        ''' <param name="prop">The property that changed.</param>
        ''' <param name="changeFlags">Bitwise flag that states what 
        ''' <see cref="cProperty.eChangeFlags">aspect</see>
        ''' of the property has changed.</param>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnPropertyChanged(ByVal prop As Properties.cProperty, ByVal changeFlags As Properties.cProperty.eChangeFlags)
            MyBase.OnPropertyChanged(prop, changeFlags)
            If (ReferenceEquals(prop, Me.m_propTooltip) And _
                (changeFlags And cProperty.eChangeFlags.Value) = cProperty.eChangeFlags.Value) Then
                Me.UpdateTooltip()
            End If
        End Sub

#End Region ' Overrides

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Synchronize cell tooltip text with the internal tooltip property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateTooltip()

            Dim strText As String = ""
            If (Me.m_propTooltip IsNot Nothing) Then
                If (Me.m_propTooltip.Source IsNot Nothing) Then
                    strText = String.Format(My.Resources.GENERIC_LABEL_INDEXED, _
                                            Me.m_propTooltip.Source.Index, _
                                            Me.m_propTooltip.Source().Name)
                End If
            End If
            Me.ToolTipText = strText

        End Sub

#End Region ' Internals

    End Class

End Namespace
