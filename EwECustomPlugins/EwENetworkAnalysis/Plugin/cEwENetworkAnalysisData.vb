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
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports EwECore

#End Region

Friend Class cEwENetworkAnalysisData
    Implements EwEPlugin.Data.IPluginData
    Implements INetworkAnalysisData

    Private m_man As cNetworkManager = Nothing
    Private m_strPluginName As String = ""
    Private m_Ascendancy(6, 5) As Single
    Private m_OI As Single()

    Public Sub New(ByVal strPluginName As String, _
                   ByVal man As cNetworkManager)
        Me.m_strPluginName = strPluginName
        Me.m_man = man
    End Sub

    Public ReadOnly Property PluginName() As String _
        Implements IPluginData.PluginName
        Get
            Return Me.m_strPluginName
        End Get
    End Property

    Public ReadOnly Property Ascendancy() As Single(,) _
        Implements INetworkAnalysisData.Ascendancy
        Get
            Return Me.m_Ascendancy
        End Get
    End Property

    Public ReadOnly Property OmnivoryIndex As Single()
        Get
            Return Me.m_OI
        End Get
    End Property

    Public ReadOnly Property RunType() As IRunType _
        Implements IPluginData.RunType
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property LIndex As Single() _
        Implements EwEUtils.Core.INetworkAnalysisData.LIndex
        Get
            Me.m_man.RunRequiredPrimaryProd()
            Dim data(Me.m_man.nGroups) As Single
            For i As Integer = 1 To Me.m_man.nGroups
                data(i) = Me.m_man.Lindex(i)
            Next
            Return data
        End Get
    End Property

    Friend Sub Resize(core As cCore)
        ReDim Me.m_OI(core.nGroups)
    End Sub
End Class
