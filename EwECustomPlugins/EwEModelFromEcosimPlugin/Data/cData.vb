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
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Friend Class cData

#Region " Private helper classes "

    Private Class cEcopathModelEntry

        Private m_strModelName As String = ""

        Public Sub New()
            ' NOP
        End Sub

        Public Property ModelName() As String
            Get
                Return Me.m_strModelName
            End Get
            Set(ByVal value As String)
                Me.m_strModelName = value
            End Set
        End Property

        Public ReadOnly Property IsDefaultName() As Boolean
            Get
                Return String.IsNullOrWhiteSpace(Me.m_strModelName)
            End Get
        End Property

    End Class

#End Region ' Private helper classes

#Region " Private vars "

    ''' <summary>Included to be able to make corrections to invalid parameters</summary>
    Private m_core As cCore = Nothing
    Private m_dtEntries As New Dictionary(Of Integer, cEcopathModelEntry)
    Private m_strOutputPath As String = ""

#End Region ' Private vars

#Region " Constructor "

    Public Sub New(core As cCore)
        Me.m_core = core
    End Sub

#End Region ' Constructor

#Region " Public properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether model generation should be enabled.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Enabled() As Boolean
        Get
            Return My.Settings.GenerationEnabled
        End Get
        Set(value As Boolean)
            My.Settings.GenerationEnabled = value
            My.Settings.Save()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathModelFromEcosim.eBACalcTypes">methodology to
    ''' calculate Biomass Accumulation</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BACalcMode As cEcopathModelFromEcosim.eBACalcTypes = cEcopathModelFromEcosim.eBACalcTypes.FromEcosimStart

    Public Property BAAverageYears As Integer
    Public Property WPower As Single = 2.0F

    Public Property OutputFormat As eDataSourceTypes = eDataSourceTypes.Access2003
    Public Property OutputTimeStep As Integer = 6

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether a model should be created for the given Ecosim year.
    ''' </summary>
    ''' <param name="iYear">The one-based year index.</param>
    ''' -----------------------------------------------------------------------
    Public Property CreateModel(ByVal iYear As Integer) As Boolean
        Get
            If (iYear < 1 Or iYear > Me.NumYears) Then Return False
            Return Me.m_dtEntries.ContainsKey(iYear)
        End Get
        Set(ByVal value As Boolean)
            If (iYear < 1 Or iYear > Me.NumYears) Then Return

            If value Then
                If Not Me.m_dtEntries.ContainsKey(iYear) Then
                    Me.m_dtEntries(iYear) = New cEcopathModelEntry
                End If
            Else
                If Me.m_dtEntries.ContainsKey(iYear) Then
                    Me.m_dtEntries.Remove(iYear)
                End If
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the name of the model to generate for a given year. To use the
    ''' default name, this value should be set to an empty string.
    ''' </summary>
    ''' <param name="iYear">The one-based year index to use.</param>
    ''' -----------------------------------------------------------------------
    Public Property ModelName(ByVal iYear As Integer) As String
        Get
            If Not Me.CreateModel(iYear) Then Return ""
            Dim record As cEcopathModelEntry = Me.m_dtEntries(iYear)
            If record.IsDefaultName Then Return DefaultModelName(iYear)
            Return record.ModelName
        End Get
        Set(ByVal value As String)
            If Not Me.CreateModel(iYear) Then Return
            Me.m_dtEntries(iYear).ModelName = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the number of Ecosim years.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property NumYears() As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the name of the EwE model that is currently loaded.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EwEModelName As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the model output directory.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property CustomOutputPath() As String
        Get
            If Not Directory.Exists(Me.m_strOutputPath) Then
                Return Me.m_core.OutputPath
            End If
            Return Me.m_strOutputPath
        End Get
        Set(value As String)
            Me.m_strOutputPath = value
        End Set
    End Property

    Public ReadOnly Property OutputPath As String
        Get
            'If Not String.IsNullOrWhiteSpace(Me.CustomOutputPath) Then
            '    Return Me.CustomOutputPath
            'End If
            Return Path.Combine(Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecosim), _
                                cFileUtils.ToValidFileName(My.Resources.CONTROL_TEXT, False))
        End Get
    End Property

    Public ReadOnly Property AutosaveType As eAutosaveTypes
        Get
            Return eAutosaveTypes.Ecosim
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the base year to display.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property FirstLabelYear() As Integer
        Get
            ' Do not show a 0, it looks a bit sad. The value matters little since
            ' it is intended for display purposes only
            Return Math.Max(Me.m_core.EcosimFirstYear, 1)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a year index in a label for this year, corrected by the <see cref="FirstLabelYear"/>.
    ''' </summary>
    ''' <param name="iYear">The one-based year index to get the label for.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property YearLabel(ByVal iYear As Integer) As String
        Get
            Return CStr(Me.FirstLabelYear + iYear - 1)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the default name for a model at a given year.
    ''' </summary>
    ''' <param name="iYear">The one-based year index to get the default model name for.</param>
    ''' -----------------------------------------------------------------------
    Private ReadOnly Property DefaultModelName(ByVal iYear As Integer) As String
        Get
            If (iYear < 1 Or iYear > Me.NumYears) Then Return ""
            Return cFileUtils.ToValidFileName(String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_DOUBLE, _
                                                            Me.EwEModelName, Me.YearLabel(iYear)), False)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Vipe zhe sheet. Leef no trace, Heinrich.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Clear()
        ' * kaploof *
        Me.m_dtEntries.Clear()
    End Sub

#End Region ' Public properties

End Class
