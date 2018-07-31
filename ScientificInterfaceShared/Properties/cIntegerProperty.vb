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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' cProperty derived class providing access to a strong-typed 
    ''' <see cref="Integer">Integer</see> value.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cIntegerProperty
        : Inherits cProperty

        Private m_iValue As Integer = 0
        Private m_Style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes the property
        ''' </summary>
        ''' <param name="Source">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see>
        ''' instance that is the data source for this property.</param>
        ''' <param name="VarName">The <see cref="eVarNameFlags">Variable name</see> in
        ''' <paramref name="Source">Source</paramref> that is the data source for this
        ''' property.</param>
        ''' <param name="SourceSec">The object acting as index on <paramref name="VarName"/>
        ''' in case this is an indexed variable.</param>
        ''' <param name="iSecIndexOffset">
        ''' <para>An optional offset that defines the diffence between the index provided by
        ''' <paramref name="SourceSec"/> and the actual storage position in the underlying arrays.
        ''' </para>
        ''' <para>For a detailed description of this variable refer to the constructor description of
        ''' <see cref="cProperty">cProperty</see>
        ''' </para>
        ''' </param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal Source As EwECore.cCoreInputOutputBase, ByVal VarName As eVarNameFlags, _
                Optional ByVal SourceSec As EwECore.cCoreInputOutputBase = Nothing, _
                Optional ByVal iSecIndexOffset As Integer = 0)
            MyBase.New(Source, VarName, SourceSec, iSecIndexOffset)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a keyless the property.
        ''' </summary>
        ''' <remarks>This Constructor is provided to allow for manual creation.</remarks>
        ''' -------------------------------------------------------------------
        Public Sub New()
            MyBase.New()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns <see cref="Type">type Integer</see>, the fixed type of this property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Function GetValueType() As System.Type
            Return GetType(Integer)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the property value.
        ''' </summary>
        ''' <param name="bHonourNull">Flag stating whether NULL status flags 
        ''' should return a NULL value.</param>
        ''' -------------------------------------------------------------------
        Protected Overrides Property Value(Optional ByVal bHonourNull As Boolean = True) As Object
            Get
                ' Is this a NULL value?
                If bHonourNull And ((Me.m_Style And cStyleGuide.eStyleFlags.Null) = cStyleGuide.eStyleFlags.Null) Then
                    ' #Yes: return nothing
                    Return Nothing
                End If
                ' Yes: return true value
                Return Me.m_iValue
            End Get
            Set(ByVal objValue As Object)
                Try
                    Me.m_iValue = CInt(objValue)
                Catch ex As Exception

                End Try
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set an Integer value in the property and commit it to the EwE core.
        ''' </summary>
        ''' <param name="newValue">The value to set.</param>
        ''' <param name="notify">Flag that states whether a change notification
        ''' must be sent out. For an detailed description of values see 
        ''' <see cref="cProperty.SetValue">cProperty.SetValue</see>.</param>
        ''' <returns>An Integer value</returns>
        ''' <remarks>
        ''' Overridden to make sure a value is passed to the core as a true Integer.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Function SetValue(ByVal newValue As Object, _
                                           Optional ByVal notify As TriState = TriState.UseDefault) As Boolean

            Dim val As cValue = Me.ValueDescriptor
            Dim meta As cVariableMetaData = Nothing
            Dim i As Integer = 0

            If val IsNot Nothing Then
                meta = val.Metadata
                If meta IsNot Nothing Then
                    i = CInt(meta.NullValue)
                End If
            End If

            If newValue Is Nothing Then newValue = i

            Try
                ' Try to convert to integer
                i = Convert.ToInt32(newValue)
            Catch ex As Exception
                'Debug.Assert(False, "Unable to convert value to Integer")
            End Try

            Return MyBase.SetValue(i, notify)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Compares a given value to the the property value.
        ''' </summary>
        ''' <param name="value">The value to compare against the value in the property.</param>
        ''' <returns>True if the values can be considered equal.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function IsValue(ByVal value As Object) As Boolean
            Try
                Return m_iValue = CInt(value)
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the Style for the property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Property Style() As cStyleGuide.eStyleFlags
            Get
                Return m_Style
            End Get
            Set(ByVal Style As cStyleGuide.eStyleFlags)
                Me.m_Style = Style
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Compares a given Style to the property Style.
        ''' </summary>
        ''' <param name="Style">The Style to compare.</param>
        ''' <returns>True if the Styles equal.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function IsStyle(ByVal Style As cStyleGuide.eStyleFlags) As Boolean
            Return Me.m_Style = Style
        End Function

    End Class

End Namespace
