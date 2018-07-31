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
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Base class for holding link information in the flow.
''' </summary>
''' <remarks>
''' Note that this class does not hold the actual references to flow units.
''' This class is a mere holder of shared behaviour between cUnitLinks and
''' cLinkDefaults
''' </remarks>
''' ===========================================================================
<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Name"), _
    Serializable()> _
Public Class cLink
    : Inherits cLinkDefault

#Region " Helper classes "

    ''' =======================================================================
    ''' <summary>
    ''' Helper class; allows the property grid to show a read-only unit name.
    ''' </summary>
    ''' =======================================================================
    Public Class cStaticUnitConverter
        Inherits TypeConverter

        Public Overrides Function GetStandardValuesSupported(ByVal context As ITypeDescriptorContext) As Boolean
            ' Do not show combo
            Return False
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
            Return New StandardValuesCollection(Nothing)
        End Function

        Public Overrides Function CanConvertFrom(ByVal context As ITypeDescriptorContext, ByVal sourceType As System.Type) As Boolean
            ' Can only convert FROM unit
            Return sourceType Is GetType(cUnit)
        End Function

        Public Overrides Function CanConvertTo(ByVal context As ITypeDescriptorContext, ByVal destinationType As System.Type) As Boolean
            ' Can only convert TO unit name
            Return destinationType Is GetType(String)
        End Function

        ''' <summary>
        ''' Convert unit to unit
        ''' </summary>
        Public Overrides Function ConvertFrom(ByVal context As ITypeDescriptorContext, _
                ByVal culture As System.Globalization.CultureInfo, _
                ByVal value As Object) As Object
            Return MyBase.ConvertFrom(context, culture, value)
        End Function

        ''' <summary>
        ''' Convert unit to unit name
        ''' </summary>
        Public Overrides Function ConvertTo(ByVal context As ITypeDescriptorContext, _
                ByVal culture As System.Globalization.CultureInfo, _
                ByVal value As Object, _
                ByVal destinationType As System.Type) As Object

            If TypeOf value Is cUnit Then
                Return DirectCast(value, cUnit).Name
            End If

            Return MyBase.ConvertTo(context, culture, value, destinationType)

        End Function

    End Class

#End Region ' Helper classes

#Region " Private bits "

    ''' <summary>Link name.</summary>
    Private m_strName As String = ""
    Private m_source As cUnit = Nothing
    Private m_target As cUnit = Nothing

#End Region ' Private bits

    Public Sub New()
        MyBase.New()
    End Sub

    <Browsable(True), _
        Category(cCATEGORY_GENERIC), _
        DisplayName("Name"), _
        Description("Name of this link"), _
        cPropertySorter.PropertyOrder(1)> _
    Public Overrides Property Name() As String
        Get
            If String.IsNullOrWhiteSpace(Me.m_strName) Then
                Try
                    Return String.Format("{0} to {1}", Me.Source.ToString, Me.Target.ToString)
                Catch ex As Exception
                    Return "<unnamed link>"
                End Try
            End If
            Return Me.m_strName
        End Get
        Set(ByVal value As String)
            Me.m_strName = value
            Me.SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(cCATEGORY_GENERIC), _
        DisplayName("Source"), _
        Description("Source unit for this link"), _
        cPropertySorter.PropertyOrder(2), _
        TypeConverter(GetType(cStaticUnitConverter))> _
    Public Property Source() As cUnit
        Get
            Return Me.m_source
        End Get
        Set(ByVal value As cUnit)
            Debug.Assert(value IsNot Nothing)
            Me.m_source = value
        End Set
    End Property

    <Browsable(True), _
        Category(cCATEGORY_GENERIC), _
        DisplayName("Target"), _
        Description("Target unit for this link"), _
        cPropertySorter.PropertyOrder(3), _
        TypeConverter(GetType(cStaticUnitConverter))> _
    Public Property Target() As cUnit
        Get
            Return Me.m_target
        End Get
        Set(ByVal value As cUnit)
            Debug.Assert(value IsNot Nothing)
            Me.m_target = value
        End Set
    End Property

    <Browsable(True), _
        Category(cCATEGORY_GENERIC), _
        DisplayName("External"), _
        Description("True when source and target differ in nationality."), _
        cPropertySorter.PropertyOrder(4)> _
    Public ReadOnly Property External() As Boolean
        Get
            If Me.Source Is Nothing Then Return False
            If Me.Target Is Nothing Then Return False
            Return Me.Source.Nationality <> Me.Target.Nationality
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the Style for this link
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <Browsable(False)> _
    Public Overridable ReadOnly Property Style() As cStyleGuide.eStyleFlags
        Get
            Dim st As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
            If (Me.ValuePerTon = 1.0) Then st = st Or cStyleGuide.eStyleFlags.ValueComputed
            Return st
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.Name & " " & Me.BiomassRatio.ToString()
    End Function

End Class
