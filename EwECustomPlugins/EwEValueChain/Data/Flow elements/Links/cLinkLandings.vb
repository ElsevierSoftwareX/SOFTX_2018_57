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
Imports System.ComponentModel
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Species-dependent link.
''' </summary>
''' ===========================================================================
<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Landings"), _
    Serializable()> _
Public Class cLinkLandings
    : Inherits cLink

#Region " Helper classes "

    ''' =======================================================================
    ''' <summary>
    ''' Helper class; allows the property grid to pick a group DBID from a list 
    ''' of group names
    ''' </summary>
    ''' =======================================================================
    Public Class cGroupConverter
        Inherits TypeConverter

        Private Function GroupName(ByVal group As cCoreInputOutputBase) As String
            Dim fmt As New cCoreInterfaceFormatter()
            Return fmt.GetDescriptor(group, eDescriptorTypes.Name)
        End Function

        Private Function GroupList() As List(Of cEcoPathGroupInput)
            Dim lGroups As New List(Of cEcoPathGroupInput)
            Dim core As cCore = cData.GetInstance().Core
            For iGroup As Integer = 1 To core.nGroups
                lGroups.Add(core.EcoPathGroupInputs(iGroup))
            Next
            Return lGroups
        End Function

        Public Overrides Function GetStandardValuesSupported(ByVal context As ITypeDescriptorContext) As Boolean
            ' Show combo
            Return True
        End Function

        Public Overrides Function GetStandardValuesExclusive(ByVal context As ITypeDescriptorContext) As Boolean
            ' Do not edit combo
            Return True
        End Function

        ''' <summary>
        ''' Override the GetStandardValues method and return a 
        ''' StandardValuesCollection filled with your standard values
        ''' </summary>
        Public Overrides Function GetStandardValues(ByVal context As ITypeDescriptorContext) As TypeConverter.StandardValuesCollection
            Dim lGroups As List(Of cEcoPathGroupInput) = Me.GroupList
            Dim lGroupNames As New List(Of String)
            Dim group As cEcoPathGroupInput = Nothing

            For iGroup As Integer = 0 To lGroups.Count - 1
                group = lGroups(iGroup)
                lGroupNames.Add(Me.GroupName(group))
            Next
            Return New StandardValuesCollection(lGroupNames)
        End Function

        Public Overrides Function CanConvertFrom(ByVal context As ITypeDescriptorContext, ByVal sourceType As System.Type) As Boolean
            Return sourceType Is GetType(String)
        End Function

        Public Overrides Function CanConvertTo(ByVal context As ITypeDescriptorContext, ByVal destinationType As System.Type) As Boolean
            Return destinationType Is GetType(Integer)
        End Function

        ''' <summary>
        ''' Convert group name to DBID
        ''' </summary>
        Public Overrides Function ConvertFrom(ByVal context As ITypeDescriptorContext, _
                ByVal culture As System.Globalization.CultureInfo, _
                ByVal value As Object) As Object

            If TypeOf value Is String Then
                If Not String.IsNullOrEmpty(CStr(value)) Then
                    Dim lGroups As List(Of cEcoPathGroupInput) = Me.GroupList
                    Dim iDBID As Integer = 0
                    Dim strName As String = ""
                    For Each group As cEcoPathGroupInput In lGroups
                        If (Me.GroupName(group) = CStr(value)) Then
                            iDBID = CInt(group.GetVariable(eVarNameFlags.DBID))
                            Exit For
                        End If
                    Next
                    Return iDBID
                End If
            End If

            Return MyBase.ConvertFrom(context, culture, value)
        End Function

        ''' <summary>
        ''' Convert DBID to group name
        ''' </summary>
        Public Overrides Function ConvertTo(ByVal context As ITypeDescriptorContext, _
                ByVal culture As System.Globalization.CultureInfo, _
                ByVal value As Object, _
                ByVal destinationType As System.Type) As Object

            If TypeOf value Is Integer Then
                Dim lGroups As List(Of cEcoPathGroupInput) = Me.GroupList
                Dim strName As String = ""
                For Each group As cEcoPathGroupInput In lGroups
                    If (CInt(group.GetVariable(eVarNameFlags.DBID)) = CInt(value)) Then
                        strName = Me.GroupName(group)
                        Exit For
                    End If
                Next
                Return strName
            End If

            Return MyBase.ConvertTo(context, culture, value, destinationType)

        End Function

    End Class

#End Region ' Helper classes

#Region " Private bits "

    ''' <summary>Link species.</summary>
    Private m_iEcopathGroupID As Integer = 0
    Private m_group As cEcoPathGroupInput = Nothing

#End Region ' Private bits

    Public Sub New()
        MyBase.New()
    End Sub

#Region " Ecopath integration "

    Public Overrides Property Name() As String
        Get
            ' ToDo: globalize this
            If (Me.m_group Is Nothing) Then Return "! No group"
            Dim fmt As New cCoreInterfaceFormatter()
            Return cStringUtils.Localize("Landings of {0}", fmt.GetDescriptor(Me.m_group, eDescriptorTypes.Name))
        End Get
        Set(ByVal value As String)
            ' NOP
        End Set
    End Property

    <Browsable(False), _
        TypeConverter(GetType(cGroupConverter))> _
    Public Property EcopathGroupID() As Integer
        Get
            Return Me.m_iEcopathGroupID
        End Get
        Set(ByVal id As Integer)
            Me.m_iEcopathGroupID = id
        End Set
    End Property

    <Browsable(True), _
     Category(cCATEGORY_TRANSFER), _
     DisplayName("Group"), _
     Description("Landed Ecopath group"), _
     cPropertySorter.PropertyOrder(5)> _
    Public ReadOnly Property EcopathGroupName() As String
        Get
            If (Me.m_group Is Nothing) Then Return "! No group"
            Dim fmt As New cCoreInterfaceFormatter()
            Return fmt.GetDescriptor(Me.m_group, eDescriptorTypes.Name)
        End Get
    End Property

#End Region ' Ecopath integration

#Region " Overrides "

    <Browsable(False)> _
    Public Overrides Property ValuePerTon() As Single
        Get
            Return 0
        End Get
        Set(ByVal value As Single)
            ' nop
        End Set
    End Property

    <Browsable(False)> _
    Public Overridable Property Group() As cEcoPathGroupInput
        Get
            Return Me.m_group
        End Get
        Friend Set(ByVal value As cEcoPathGroupInput)
            Me.m_group = value
            If (Group IsNot Nothing) Then
                Me.m_iEcopathGroupID = CInt(Group.GetVariable(eVarNameFlags.DBID))
            Else
                Me.m_iEcopathGroupID = 0
            End If
        End Set
    End Property

    Public Overrides Function IsVisible() As Boolean
        Dim fleet As cEcopathFleetInput = DirectCast(Me.Source, cProducerUnit).Fleet
        Dim group As cEcoPathGroupInput = Me.Group
        If (fleet IsNot Nothing) And (group IsNot Nothing) Then
            Return (fleet.Landings(group.Index) > 0)
        End If
        Return False
    End Function

    Public Overrides ReadOnly Property Style() As ScientificInterfaceShared.Style.cStyleGuide.eStyleFlags
        Get
            Dim fleet As cEcopathFleetInput = DirectCast(Me.Source, cProducerUnit).Fleet
            If (fleet Is Nothing) Then Return cStyleGuide.eStyleFlags.FailedValidation
            Return MyBase.Style
        End Get
    End Property

#End Region ' Overrides

End Class
