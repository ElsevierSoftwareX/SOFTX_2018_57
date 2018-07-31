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
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' PropertyHeaderCell implements a PropertyCell based class for creating 
    ''' header cells in <see cref="EwEGrid">EwE grids</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public MustInherit Class PropertyHeaderCell
        : Inherits PropertyCell

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="prop"><see cref="cProperty">Property</see> to attach to the cell.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal prop As cProperty)
            ' Call baseclass constructor
            MyBase.New(prop)
            ' Always
            Me.DataModel.EnableEdit = False
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, instructing the cell to use a unit mask.
        ''' </summary>
        ''' <param name="prop"><see cref="cProperty">Property</see> to attach to the cell.</param>
        ''' <param name="strUnit">The unit to substitute into the header cell text.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal prop As cProperty,
                       ByVal strUnit As String)
            Me.New(prop)
            Me.SetUnits(strUnit)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see> to obtain values from.</param>
        ''' <param name="Source">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see> data source</param>
        ''' <param name="VarName">The <see cref="eVarNameFlags">VarName flag</see> that defines which aspect of the Source to acces</param>
        ''' <param name="SourceSec">An optional secundary index in the VarName, or <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> when irrelevant</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal pm As cPropertyManager,
                       ByVal Source As cCoreInputOutputBase,
                       ByVal VarName As eVarNameFlags,
                       Optional ByVal SourceSec As cCoreInputOutputBase = Nothing)
            Me.New(pm.GetProperty(Source, VarName, SourceSec))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see> to obtain values from.</param>
        ''' <param name="Source">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see> data source</param>
        ''' <param name="VarName">The <see cref="eVarNameFlags">VarName flag</see> that defines which aspect of the Source to acces</param>
        ''' <param name="SourceSec">Secundary index in the VarName, or <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> when irrelevant</param>
        ''' <param name="strUnit">The unit to substitute into the header cell text.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal pm As cPropertyManager,
                       ByVal Source As cCoreInputOutputBase,
                       ByVal VarName As eVarNameFlags,
                       ByVal SourceSec As cCoreInputOutputBase,
                       ByVal strUnit As String)
            Me.New(pm.GetProperty(Source, VarName, SourceSec), strUnit)
        End Sub

#End Region ' Construction 

#Region " Data (style) "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to enusre that header cells use names colour feedback
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides Property Style() As cStyleGuide.eStyleFlags
            Get
                Return (MyBase.Style Or cStyleGuide.eStyleFlags.NotEditable)
            End Get
            Set(ByVal styleNew As cStyleGuide.eStyleFlags)
                MyBase.Style = (styleNew Or cStyleGuide.eStyleFlags.NotEditable)
            End Set
        End Property

#End Region ' Data (style) 

#Region " Unit header text "

        Protected m_strUnit As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure the cell to automatically incorporate unit strings into
        ''' its content. These unit strings will be synchronized with 
        ''' <see cref="cStyleGuide.UnitsChanged">cStyleGuide unit changes</see>.
        ''' </summary>
        ''' <param name="strUnit">The <see cref="cUnits">unit string</see> to set.
        ''' To clear units, simply pass in an empty string.</param>
        ''' -------------------------------------------------------------------
        Protected Sub SetUnits(ByVal strUnit As String)
            Me.m_strUnit = strUnit
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value for a header cell.
        ''' </summary>
        ''' <bugfix number="892">
        ''' Moved this functionality from DisplayText to make sure header values
        ''' are correctly picked up by Copy and Cut operations.
        ''' </bugfix>
        ''' <remarks>If a header cell value contains a '|' character, the value 
        ''' is split by this character. The first part (left side of '|') is used
        ''' as value part, and the last part (right side of '|') is used as tooltip
        ''' text.</remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Property Value() As Object
            Get
                Dim strVal As String = CStr(MyBase.Value)
                Dim strUnit As String = ""

                If (Me.UIContext IsNot Nothing) And (Not String.IsNullOrWhiteSpace(Me.m_strUnit)) Then
                    strUnit = New cUnits(Me.UIContext.Core).ToString(Me.m_strUnit)
                End If

                If (String.IsNullOrWhiteSpace(strUnit)) Then Return strVal
                If (String.IsNullOrWhiteSpace(strVal)) Then Return strUnit
                If (strVal.Contains("{0}")) Then Return cStringUtils.Localize(strVal, strUnit)
                Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, strVal, strUnit)
            End Get
            Set(ByVal value As Object)
                If (TypeOf value Is String) Then
                    Dim strValue As String = CStr(value)
                    If strValue.IndexOf("|"c) > -1 Then
                        Dim bits As String() = strValue.Split("|"c)
                        If (String.Compare(bits(0), bits(1), True) <> 0) Then
                            Me.ToolTipText = bits(1)
                        Else
                            Me.ToolTipText = ""
                        End If
                        value = bits(0)
                    End If
                End If
                MyBase.Value = value
            End Set
        End Property

#End Region ' Unit header text

    End Class

End Namespace
