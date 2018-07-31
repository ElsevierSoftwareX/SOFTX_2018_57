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
Imports EwECore
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
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' EwEColumnHeader implements a column header with EwE style
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class EwEColumnHeaderCell
        : Inherits EwEHeaderCell

        Private m_vizDefault As IVisualModel = Nothing

#Region " Construction / destruction "

        Public Sub New(Optional ByVal strValue As String = "")
            MyBase.New(strValue)
            Me.VisualModel = New cEwEGridColumnHeaderVisualizer()
            Me.m_vizDefault = Me.VisualModel
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        Public Sub New(ByVal strValue As String, ByVal strUnits As String)
            Me.New(strValue)
            Me.SetUnits(strUnits)
        End Sub

        ''' <summary>
        ''' Create a header cell with automated name and units.
        ''' </summary>
        ''' <param name="varname"></param>
        Public Sub New(ByVal varname As eVarNameFlags)
            Me.New(varname, eDescriptorTypes.Name)
        End Sub

        Public Sub New(ByVal varname As eVarNameFlags, detail As eDescriptorTypes)
            Me.New(New cVarnameTypeFormatter().GetDescriptor(varname, detail) & "|" & New cVarnameTypeFormatter().GetDescriptor(varname, eDescriptorTypes.Description))
            Dim md As cVariableMetaData = cVariableMetadata.Get(varname)
            If (md IsNot Nothing) Then
                Me.SetUnits(md.Units)
            End If
        End Sub

        Public Overrides Sub Dispose()
            Me.VisualModel = Me.m_vizDefault
            MyBase.Dispose()
        End Sub

#End Region ' Construction / destruction

    End Class

End Namespace
