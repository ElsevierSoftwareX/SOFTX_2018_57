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
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.VisualModels
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports EwECore.Style

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' EwERowHeaderCell implements a Common cell rendered as an EwE name field.
    ''' EwERowHeaderCells are used in EwE to replace Row headers which values are statically
    ''' set.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public MustInherit Class EwEHeaderCell
        : Inherits EwECell

#Region " Construction "

        Public Sub New(Optional ByVal objValue As Object = Nothing)
            MyBase.New(objValue, GetType(String))
            ' Disable edit
            Me.DataModel.EnableEdit = False
            Me.SetUnits("")
        End Sub

        Public Sub New(ByVal strValue As String, ByVal strUnit As String)
            Me.New(strValue)
            Me.SetUnits(strUnit)
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
                Return (cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable Or MyBase.Style)
            End Get
            Set(ByVal styleNew As cStyleGuide.eStyleFlags)
                MyBase.Style = (styleNew Or cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable)
            End Set
        End Property

#End Region ' Data (style) 

#Region " Unit header text "

        Protected m_strUnit As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure the cell to automatically incorporate unit strings into
        ''' its content. These unit strings will be synchronized with 
        ''' <see cref="cStyleGuide.UnitsChanged">cStyleGuide unit changes</see>.
        ''' </summary>
        ''' <param name="strUnit">The unit to format into the header cell.</param>
        ''' -------------------------------------------------------------------
        Public Sub SetUnits(ByVal strUnit As String)
            Me.m_strUnit = strUnit
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure the cell to no longer incorporate unit strings into its 
        ''' text.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ClearUnitHeader()
            Me.m_strUnit = ""
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

                If (Me.UIContext IsNot Nothing) Then
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
