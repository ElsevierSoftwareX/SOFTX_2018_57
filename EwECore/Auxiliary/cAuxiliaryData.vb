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
Imports System.Text
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace Auxiliary

    ''' =======================================================================
    ''' <summary>
    ''' <para>
    ''' This class represents all Auxiliary data that can be associated with
    ''' any value in the EwECore or an EwE user interface. This data is loose-typed;
    ''' each core and user interface value that requires Auxiliary data must define
    ''' a unique ID via which associated Auxillary data is stored and retreived.
    ''' </para>
    ''' <para>
    ''' When associated with <see cref="ICoreInterface">ICoreInterface</see>
    ''' -derived objects, cAuxillaryData offers the ability to maintain a
    ''' <see cref="ICoreInterface.DataType">data type</see> and 
    ''' <see cref="ICoreInterface.DBID">database ID</see> pair to uniquely
    ''' identify the object instance this data is associated with.
    ''' </para>
    ''' </summary>
    ''' =======================================================================
    Public Class cAuxiliaryData
        Implements ICoreInterface

#Region " Private vars "

        Private m_core As cCore = Nothing

        ''' <summary>Remark text for this data.</summary>
        Private m_strRemark As String = ""
        ''' <summary>Visual style for this data.</summary>
        Private m_visualstyle As cVisualStyle = Nothing

#End Region ' Private vars

#Region " Constructors "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of cAuxiliaryData.
        ''' </summary>
        ''' <param name="core"></param>
        ''' <param name="strValueID">Unique ID to assign to this cAuxillaryData instance.</param>
        ''' <remarks>
        ''' <para>This constructor should be used when defining cAuxilaryData for derived 
        ''' values and values from objects that do not originate from the EwE core.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Sub New(ByVal core As cCore, ByVal strValueID As String)
            Me.New(core, cValueID.FromString(strValueID))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of cAuxiliaryData that is soft-linked
        ''' to an <see cref="ICoreInterface">ICoreInterface</see>-derived object. 
        ''' </summary>
        ''' <param name="core"></param>
        ''' <param name="key"></param>
        ''' -------------------------------------------------------------------
        Sub New(ByVal core As cCore, ByVal key As cValueID)
            MyBase.New()

            Me.Key = key
            Me.m_core = core
            Me.Settings = New cXMLSettings()

            AddHandler Me.Settings.OnSettingsChanged, AddressOf OnSettingsChanged

            Me.AllowValidation = False
            ' NOP
            Me.AllowValidation = True

        End Sub

#End Region ' Constructors

#Region " Public properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this object is allowed to report data changes to the core.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overloads Property AllowValidation() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the key for this data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Key() As cValueID = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the unique ID assigned to this data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ID() As String
            Get
                Return Me.Key.ToString
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the remark text for this data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overloads Property Remark() As String
            Get
                Return Me.m_strRemark
            End Get
            Set(ByVal value As String)
                If (value <> m_strRemark) Then
                    Me.m_strRemark = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the visual style for this data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property VisualStyle() As cVisualStyle
            Get
                Return Me.m_visualstyle
            End Get
            Set(ByVal value As cVisualStyle)

                If ReferenceEquals(value, Me.VisualStyle) Then Return

                If (Me.m_visualstyle IsNot Nothing) Then
                    Me.m_visualstyle.Container = Nothing
                End If

                Me.m_visualstyle = value

                If (Me.m_visualstyle IsNot Nothing) Then
                    Me.m_visualstyle.Container = Me
                End If

                Me.Update()

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set settings for this data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Settings() As cXMLSettings = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether an instance holds any data.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return String.IsNullOrWhiteSpace(Me.Remark) And
                       (Me.m_visualstyle Is Nothing) And
                       String.IsNullOrWhiteSpace(Me.Settings.ToString())
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update auxillary data changes to the core.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub Update()

            If Me.AllowValidation Then
                ' Notify core, if provided
                If (Me.m_core IsNot Nothing) Then
                    Me.m_core.onChanged(Me, eMessageType.DataModified)
                End If
            End If

        End Sub

#End Region ' Public properties

#Region " ICoreComponent implementation "

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="ICoreInterface.CoreComponent" />
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property CoreComponent() As eCoreComponentType _
            Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.Core
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="ICoreInterface.CoreComponent" />
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property DataType() As eDataTypes _
            Implements ICoreInterface.DataType
            Get
                Return eDataTypes.Auxillary
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="ICoreInterface.CoreComponent" />
        ''' -----------------------------------------------------------------------
        Public Property DBID() As Integer Implements ICoreInterface.DBID

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="ICoreInterface.CoreComponent" />
        ''' -----------------------------------------------------------------------
        Public Function GetID() As String _
               Implements ICoreInterface.GetID
            Return Me.Key.ToString
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the ordinal number in the core storage structures for a core 
        ''' data entity.
        ''' </summary>
        ''' <remarks>
        ''' Since AuxillaryData are indexed via HashTables this property is not used.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Property Index() As Integer _
               Implements ICoreInterface.Index
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(ByVal value As Integer)
                ' NOP
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <inheritdoc cref="ICoreInterface.Name" />
        ''' -----------------------------------------------------------------------
        Public Property Name() As String _
                Implements ICoreInterface.Name
            Get
                Return Me.Remark
            End Get
            Set(ByVal value As String)
                Me.Remark = value
            End Set
        End Property

        Private Sub OnSettingsChanged(sender As Object, args As EventArgs)
            Try
                Me.Update()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' ICoreComponent implementation

#Region " Utility "

        Public Sub MergeWith(data As cAuxiliaryData)

            ' Sanity check
            If (data Is Nothing) Then Return

            ' -- Remark --
            If (String.IsNullOrWhiteSpace(Me.Remark)) Then
                Me.Remark = data.Remark
            ElseIf Not String.IsNullOrWhiteSpace(data.Remark) Then
                Dim sb As New StringBuilder()
                sb.AppendLine(Me.Remark)
                sb.Append(data.Remark)
                Me.Remark = sb.ToString
            End If

            ' Cannot automatically merge visualstyle
            ' Cannot automatically merge settings

        End Sub

#End Region ' Utility

    End Class

End Namespace
