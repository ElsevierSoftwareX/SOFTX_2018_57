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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports System.Collections.Generic

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Inputs for a single pedigree level within a pedigree category.
''' </summary>
''' <remarks>
''' <para>Pedigree categories are identified by <see cref="cCore.PedigreeVariable">PedigreeVariable</see>, 
''' which will return <see cref="cCore.nPedigreeVariables">cCore.nPedigreeVariables</see> variables.</para>
''' <para>Pedigree levels are stored and accessible via a <see cref="cPedigreeManager">pedigree manager</see>.</para>
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cPedigreeLevel
    Inherits cCoreInputOutputBase

    Private m_manager As cPedigreeManager = Nothing
    Private m_iSequence As Integer = 0

    Friend Sub New(ByVal core As cCore, ByVal manager As cPedigreeManager, ByVal iDBID As Integer)
        MyBase.New(core)

        Dim val As cValue
        Dim desc() As Char

        Me.m_manager = manager
        Me.m_dataType = eDataTypes.PedigreeLevel
        Me.m_coreComponent = eCoreComponentType.EcoPath
        Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        Me.AllowValidation = False

        Me.DBID = iDBID

        'VarName
        val = New cValue(New Integer, eVarNameFlags.VariableName, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        'IndexValue
        val = New cValue(New Single, eVarNameFlags.IndexValue, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'ConfidenceInterval
        val = New cValue(New Single, eVarNameFlags.ConfidenceInterval, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        ' Description
        val = New cValue(New String(desc), eVarNameFlags.Description, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
        m_values.Add(val.varName, val)

        'PoolColor
        val = New cValue(New Integer, eVarNameFlags.PoolColor, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        'Estimated flag
        val = New cValue(New Boolean, eVarNameFlags.Estimated, eStatusFlags.Null, eValueTypes.Bool)
        Me.m_values.Add(val.varName, val)

        Me.AllowValidation = True

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eVarNameFlags">variable</see> that a pedigree
    ''' level pertains to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property VariableName() As eVarNameFlags
        Get
            Return DirectCast(Me.GetVariable(eVarNameFlags.VariableName), eVarNameFlags)
        End Get
        Set(ByVal value As eVarNameFlags)
            Me.SetVariable(eVarNameFlags.VariableName, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the index value of a pedigree level expressed as a ratio [0, 1].
    ''' </summary>
    ''' <remarks>
    ''' This value is not to be confused with <see cref="Index">Index</see>,
    ''' which reflects the position of an <see cref="ICoreInputOutput">core object</see>
    ''' within the underlying core arrays.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property IndexValue() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.IndexValue))
        End Get
        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.IndexValue, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the confidence interval of a pedigree level expressed as a
    ''' rounded percentage [0, 100].
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ConfidenceInterval() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.ConfidenceInterval))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.ConfidenceInterval, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a textual description of a pedigree level.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Description() As String
        Get
            Return CStr(Me.GetVariable(eVarNameFlags.Description))
        End Get
        Set(ByVal value As String)
            Me.SetVariable(eVarNameFlags.Description, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the color to represent a pedigree level.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property PoolColor() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.PoolColor))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.PoolColor, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the index of this level in its manager.
    ''' </summary>
    ''' <remarks>
    ''' This variable will never make it into the core; it's a mere administrative
    ''' value. Note that this value is one-based.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property Sequence() As Integer
        Get
            Return Me.m_iSequence
        End Get
        Friend Set(ByVal value As Integer)
            Me.m_iSequence = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the values for this level are estimated by EwE.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property IsEstimated() As Boolean
        Get
            Return CBool(Me.GetVariable(eVarNameFlags.Estimated))
        End Get
        Friend Set(ByVal value As Boolean)
            Me.SetVariable(eVarNameFlags.Estimated, value)
        End Set
    End Property

End Class
