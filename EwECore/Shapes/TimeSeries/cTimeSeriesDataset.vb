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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' ---------------------------------------------------------------------------
''' <summary>
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTimeSeriesDataset
    Inherits cCoreInputOutputBase
    Implements IList(Of cTimeSeries)

    Private m_lTimeSeries As New List(Of cTimeSeries)
    Private m_nTimeSeries As Integer = 0
    Private m_iNumTimeSeries As Integer = 0

#Region " Constructor "

    Public Sub New(ByVal core As cCore, ByVal nTimeSeries As Integer)
        MyBase.New(core)

        Dim val As cValue
        Dim desc() As Char

        Try

            Me.m_dataType = eDataTypes.TimeSeriesDataset
            Me.m_iNumTimeSeries = nTimeSeries

            ' Definition changes do not affect the running state of the model
            m_coreComponent = eCoreComponentType.DataSource

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' Description
            val = New cValue(New String(desc), eVarNameFlags.Description, eStatusFlags.NotEditable Or eStatusFlags.Null, eValueTypes.Str)
            m_values.Add(val.varName, val)

            ' Author
            val = New cValue(New String(desc), eVarNameFlags.Author, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            m_values.Add(val.varName, val)

            ' Contact
            val = New cValue(New String(desc), eVarNameFlags.Contact, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            m_values.Add(val.varName, val)

            'set status flags to default values
            ResetStatusFlags()
            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cTimeSeriesDataset.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cTimeSeriesDataset. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Overrides "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' All variables non-editable (for now, 11feb08)
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        Me.AllowValidation = False
        For Each vs As cValue In Me.m_values.Values
            vs.ValidationStatus = eStatusFlags.OK Or eStatusFlags.NotEditable
        Next
        Me.AllowValidation = True
        Return True
    End Function

#End Region ' Overrides

#Region " Variable via dot(.) operator"

    Public Property Description() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Description))
        End Get

        Friend Set(ByVal str As String)
            SetVariable(eVarNameFlags.Description, str)
        End Set
    End Property

    Public Property Author() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Author))
        End Get

        Friend Set(ByVal str As String)
            SetVariable(eVarNameFlags.Author, str)
        End Set
    End Property

    Public Property Contact() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Contact))
        End Get

        Friend Set(ByVal str As String)
            SetVariable(eVarNameFlags.Contact, str)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of time series in a dataaset, as obtained from the datasource.
    ''' </summary>
    ''' <remarks>
    ''' <para>This value is read from the database and provides an estimate of the number of
    ''' time series for this dataset PRIOR TO when the time series are loaded.</para>
    ''' <para>As soon as the dataset is loaded, the method <see cref="Count">Count</see>
    ''' will give the actual number of time series attached to this dataset.</para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property nTimeSeries() As Integer
        Get
            ' Return the cached number of time series if no series have been loaded yet
            If (Me.Count = 0) Then Return Me.m_iNumTimeSeries
            Return Me.Count
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the first year in the time series dataset.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property FirstYear() As Integer = 0

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of points in the time series dataset. Data points may
    ''' be spaced <see cref="eTSDataSetInterval.Annual">annually</see> or is given for each
    ''' <see cref="eTSDataSetInterval.TimeStep">time step</see>. The size of the 
    ''' interval can be checked via <see cref="TimeSeriesInterval"/>.
    ''' <seealso cref="TimeSeriesInterval"/>
    ''' <seealso cref="eTSDataSetInterval"/>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property NumPoints() As Integer = 0

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a time series from the dataset.
    ''' </summary>
    ''' <param name="iSeries">One-based index of the time series to retrieve.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property TimeSeries(iSeries As Integer) As cTimeSeries
        Get
            Return Me.m_core.EcosimTimeSeries(iSeries)
        End Get
    End Property

    Public Property TimeSeriesInterval As eTSDataSetInterval = eTSDataSetInterval.Annual

#End Region ' Variable via dot(.) operator

#Region " Status Flags via dot(.) operator"

    Public Property DescriptionStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Description)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Description, value)
        End Set

    End Property

    Public Property AuthorStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Author)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Author, value)
        End Set

    End Property

    Public Property ContactStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Description)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Description, value)
        End Set

    End Property

#End Region ' Status Flags via dot(.) operator

#Region " Public interfaces "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update all time series in the dataset
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Update()
        'For Each ts As cTimeSeries In Me.m_lTimeSeries
        '    ts.Enabled = True
        'Next
        Me.m_core.UpdateTimeSeries()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns>
    ''' <para>Return values are:</para>
    ''' <list>
    ''' <item><term>True</term><description>All of the time series in the Dataset are applied</description></item>
    ''' <item><term>False</term><description>None of the time series in the Dataset are applied</description></item>
    ''' <item><term>UseDefault</term><description>Some of the time series in the Dataset are applied</description></item>
    ''' </list>
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function IsEnabled() As TriState

        Dim iEnableCount As Integer = 0
        For Each ts As cTimeSeries In Me.m_lTimeSeries
            If ts.Enabled Then iEnableCount += 1
        Next
        If iEnableCount = 0 Then Return TriState.False
        If iEnableCount = Me.m_lTimeSeries.Count Then Return TriState.True
        Return TriState.UseDefault

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the dataset is currently loaded.
    ''' </summary>
    ''' <returns>True if the dataset is currently loaded.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsLoaded() As Boolean
        Return (Me.m_core.ActiveTimeSeriesDatasetIndex = Me.Index)
    End Function

#End Region ' Public interfaces

#Region " List interfaces "

    Friend Sub Add(ByVal item As cTimeSeries) Implements System.Collections.Generic.ICollection(Of cTimeSeries).Add
        Me.m_lTimeSeries.Add(item)
    End Sub

    Friend Overloads Sub Clear() Implements System.Collections.Generic.ICollection(Of cTimeSeries).Clear
         MyBase.Clear()
        Me.m_lTimeSeries.Clear()
    End Sub

    Public Function Contains(ByVal item As cTimeSeries) As Boolean Implements System.Collections.Generic.ICollection(Of cTimeSeries).Contains
        Return Me.m_lTimeSeries.Contains(item)
    End Function

    Public Sub CopyTo(ByVal array() As cTimeSeries, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of cTimeSeries).CopyTo
        Me.m_lTimeSeries.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of cTimeSeries).Count
        Get
            Return Me.m_lTimeSeries.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of cTimeSeries).IsReadOnly
        Get
            Return True
        End Get
    End Property

    Private Function Remove(ByVal item As cTimeSeries) As Boolean Implements System.Collections.Generic.ICollection(Of cTimeSeries).Remove
        ' Nope
        Debug.Assert(False, "Deliberately not implemented; datasets can only be populated from the core")
    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of cTimeSeries) Implements System.Collections.Generic.IEnumerable(Of cTimeSeries).GetEnumerator
        Return Me.m_lTimeSeries.GetEnumerator()
    End Function

    Friend Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return Me.m_lTimeSeries.GetEnumerator()
    End Function

    Public Function IndexOf(ByVal item As cTimeSeries) As Integer Implements System.Collections.Generic.IList(Of cTimeSeries).IndexOf
        Return Me.m_lTimeSeries.IndexOf(item)
    End Function

    Friend Sub Insert(ByVal index As Integer, ByVal item As cTimeSeries) Implements System.Collections.Generic.IList(Of cTimeSeries).Insert
        ' Nope
    End Sub

    ''' <summary>
    ''' Get a time series from the dataset.
    ''' </summary>
    ''' <param name="index">Zero-based index of the time series to access.</param>
    Default Public Property Item(ByVal index As Integer) As cTimeSeries Implements System.Collections.Generic.IList(Of cTimeSeries).Item
        Get
            Return Me.m_lTimeSeries.Item(index)
        End Get
        Friend Set(ByVal value As cTimeSeries)
            ' Nope
            Debug.Assert(False, "Deliberately not implemented; datasets can only be populated from the core")
        End Set
    End Property

    Private Sub RemoveAt(ByVal index As Integer) Implements System.Collections.Generic.IList(Of cTimeSeries).RemoveAt
        ' Nope
        Debug.Assert(False, "Deliberately not implemented; datasets can only be populated from the core")
    End Sub

#End Region ' List interfaces

#Region " Obsolete "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="nTimeSeries"/>
    ''' -----------------------------------------------------------------------
    <Obsolete("Use nTimeSeries instead")> _
    Public ReadOnly Property NumTimeSeries() As Integer
        Get
            If Me.Count = 0 Then Return Me.m_iNumTimeSeries
            Return Me.Count
        End Get
    End Property

#End Region ' Obsolete

End Class
