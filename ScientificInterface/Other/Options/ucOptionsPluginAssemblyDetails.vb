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

Imports EwEPlugin
Imports EwEUtils.SystemUtilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Public Class ucOptionsPluginAssemblyDetails

    Private m_pa As cPluginAssembly = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > Plug-in settings interface for
    ''' showing details on a plug-in assembly.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal pa As cPluginAssembly)

        Me.InitializeComponent()

        Me.m_tbCompany.Text = pa.Company
        Me.m_tbCopyright.Text = pa.Copyright
        Me.m_tbDescription.Text = pa.Description
        Me.m_tbFile.Text = pa.Filename
        Me.m_tbVersion.Text = pa.Version
        Me.m_tbxTrusted.Text = If(String.IsNullOrWhiteSpace(pa.Sandbox), SharedResources.GENERIC_VALUE_YES, SharedResources.GENERIC_VALUE_NO)

        Me.m_pa = pa


    End Sub

End Class
