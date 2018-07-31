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
Imports ScientificInterfaceShared.Style
Imports SourceGrid2.Cells.Real

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Cell class to implement a row header in an <see cref="EwEGrid">EWE grid</see>, 
    ''' that dynamically derives its <see cref="Cell.DisplayText">display text</see>
    ''' from the core.
    ''' </summary>
    ''' <remarks>
    ''' <para>This class inherits from <see cref="PropertyHeaderCell">PropertyHeaderCell</see> 
    ''' to implement basic, standardized formatting for row header cells. The
    ''' display text of the cell is tracked 'live' using <see cref="cProperty">properties</see>.</para>
    ''' <para>Additionally, the cell offers capabilities to incorporate units
    ''' that are updated whenever the system display units change.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class PropertyRowHeaderCell
        : Inherits PropertyHeaderCell

        ''' <summary>One visualizer for all cells</summary>
        Private Shared g_visualizer As cEwEGridVisualizerBase

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor to create a row header cell that derives its 
        ''' <see cref="DisplayText">display text</see> from a 
        ''' <see cref="cProperty">cProperty</see>.
        ''' </summary>
        ''' <param name="prop">cProperty to deliver the cell value.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal prop As cProperty)
            MyBase.New(prop)
            Me.VisualModel = New cEwEGridRowHeaderVisualizer
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor to create a row header cell that derives its 
        ''' <see cref="DisplayText">display text</see> from a 
        ''' <see cref="cProperty">cProperty</see>. The property value is 
        ''' inserted in the cell display text via a format mask.
        ''' </summary>
        ''' <param name="prop">cProperty to deliver the cell value.</param>
        ''' <param name="strUnit">The format mask to apply. This mask must
        ''' contain a '{0}' field where the property value is to be inserted.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal prop As cProperty,
                       ByVal strUnit As String)
            Me.New(prop)
            Me.SetUnits(strUnit)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor to create a row header cell that synchronizes 
        ''' its <see cref="DisplayText">display text</see> live with core data.
        ''' </summary>
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
        ''' Row headers always use <see cref="cStyleGuide.eStyleFlags.Names">Names</see> style.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Property Style() As Style.cStyleGuide.eStyleFlags
            Get
                Return MyBase.Style Or cStyleGuide.eStyleFlags.Names
            End Get
            Set(ByVal value As Style.cStyleGuide.eStyleFlags)
                MyBase.Style = value
            End Set
        End Property

#End Region ' Construction 

    End Class

End Namespace
