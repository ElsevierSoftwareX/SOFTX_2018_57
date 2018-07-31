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

Option Strict On
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

Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' EwERowHeaderCell implements a EwERowHeaderCell to implement row headers. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class EwERowHeaderCell
        : Inherits EwEHeaderCell

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Construct a header cell with an optional static value.
        ''' </summary>
        ''' <param name="strValue">The value to set.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(Optional ByVal strValue As String = "")
            MyBase.New(strValue)
            ' Set visualizer
            Me.VisualModel = New cEwEGridRowHeaderVisualizer()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Construct a header cell displaying a single unit.
        ''' </summary>
        ''' <param name="strUnitMask">The mask should contain ONE {0} placeholder where
        ''' the <paramref name="strUnit">unit</paramref> will be displayed.</param>
        ''' <param name="strUnit">The unit to dynamically substitute in the cell display text.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal strUnitMask As String, ByVal strUnit As String)
            Me.New(strUnitMask)
            Me.SetUnits(strUnit)
        End Sub

        Public Sub New(ByVal varname As eVarNameFlags)
            Me.New(New cVarnameTypeFormatter().GetDescriptor(varname, eDescriptorTypes.Name))
        End Sub

        Public Sub New(ByVal varname As eVarNameFlags, ByVal strUnitMask As String, ByVal strUnit As String)
            Me.New(String.Format(My.Resources.GENERIC_LABEL_DOUBLE,
                                 New cVarnameTypeFormatter().GetDescriptor(varname, eDescriptorTypes.Name),
                                 strUnitMask), strUnit)
        End Sub

#End Region ' Construction 

    End Class

End Namespace
