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
Imports EwEUtils.Extensions

#End Region ' Imports

Namespace Samples

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Parameter sample snapshot of an Ecopath model, recorded from 
    ''' Monte Carlo model perturbations.
    ''' </summary>
    ''' <seealso cref="cEcopathSampleManager"/>.
    ''' <seealso cref="cEcopathSampleDatastructures"/>.
    ''' -------------------------------------------------------------------
    Public Class cEcopathSample
        Inherits cCoreInputOutputBase

        Private m_b As Single()
        Private m_pb As Single()
        Private m_qb As Single()
        Private m_ee As Single()
        Private m_ba As Single()
        Private m_babi As Single()
        Private m_dc As Single(,)
        Private m_landing As Single(,)
        Private m_discard As Single(,)

        Private m_bStatsCalculated As Boolean = False
        Private m_dtPerturbed As New Dictionary(Of eVarNameFlags, Boolean)

        Public Sub New(core As cCore, ByVal DBID As Integer, ByVal iIndex As Integer)

            MyBase.New(core)

            Dim val As cValue = Nothing

            Me.m_coreComponent = eCoreComponentType.EcopathSample
            Me.m_dataType = eDataTypes.EcopathSample

            Me.AllowValidation = False
            Me.Index = iIndex
            Me.DBID = DBID

            ' ToDo: globalize this
            Me.Name = "Sample " & iIndex

            'Rating
            val = New cValue(New Integer, eVarNameFlags.SampleRating, eStatusFlags.Null, eValueTypes.Int)
            Me.m_values.Add(val.varName, val)

            Me.AllowValidation = True

            Me.ResetStatusFlags()

            ReDim Me.m_b(Me.m_core.nGroups)
            ReDim Me.m_pb(Me.m_core.nGroups)
            ReDim Me.m_qb(Me.m_core.nGroups)
            ReDim Me.m_ee(Me.m_core.nGroups)
            ReDim Me.m_ba(Me.m_core.nGroups)
            ReDim Me.m_babi(Me.m_core.nGroups)
            ReDim Me.m_dc(Me.m_core.nLivingGroups, Me.m_core.nGroups)
            ReDim Me.m_landing(Me.m_core.nFleets, Me.m_core.nGroups)
            ReDim Me.m_discard(Me.m_core.nFleets, Me.m_core.nGroups)

            Me.m_b.Fill(cCore.NULL_VALUE)
            Me.m_pb.Fill(cCore.NULL_VALUE)
            Me.m_qb.Fill(cCore.NULL_VALUE)
            Me.m_ee.Fill(cCore.NULL_VALUE)
            Me.m_ba.Fill(cCore.NULL_VALUE)
            Me.m_dc.Fill(cCore.NULL_VALUE)
            Me.m_landing.Fill(cCore.NULL_VALUE)
            Me.m_discard.Fill(cCore.NULL_VALUE)

        End Sub

        ''' <summary>
        ''' Get/set the MD5 hash of the Ecopath input set a sample was generated for.
        ''' </summary>
        Public Property Hash As String = ""

        ''' <summary>
        ''' Get/set the source computer that a sample was generated on.
        ''' </summary>
        Public Property Source As String = ""

        ''' <summary>
        ''' Returns the number of EE values that exceed 1
        ''' </summary>
        ''' <returns></returns>
        Public Function NumInvalidEE() As Integer
            Dim iTot As Integer = 0
            For Each ee As Single In Me.m_ee
                If ee > 1 Then iTot += 1
            Next
            Return iTot
        End Function

        Public Function PerturbedValues() As eVarNameFlags()
            ' Only do this when needed
            If (Not Me.m_bStatsCalculated) Then
                Me.CalculateStats()
            End If
            Return Me.m_dtPerturbed.Keys().ToArray()
        End Function

        ''' <summary>
        ''' Get/set the date that a sample was generated.
        ''' </summary>
        Public Property Generated As Date

        ''' <summary>
        ''' Sampled biomass rate (x group)
        ''' </summary>
        Public Function B() As Single()
            Return Me.m_b
        End Function

        ''' <summary>
        ''' Sampled production rate (x group)
        ''' </summary>
        Public Function PB() As Single()
            Return Me.m_pb
        End Function

        ''' <summary>
        ''' Sampled consumption rate (x group)
        ''' </summary>
        Public Function QB() As Single()
            Return Me.m_qb
        End Function

        ''' <summary>
        ''' Sampled biomass accummulation (x group)
        ''' </summary>
        Public Function BA() As Single()
            Return Me.m_ba
        End Function

        ''' <summary>
        ''' Sampled biomass accummulation rate (x group)
        ''' </summary>
        Public Function BaBi() As Single()
            Return Me.m_babi
        End Function

        ''' <summary>
        ''' Sampled EE (x group)
        ''' </summary>
        Public Function EE() As Single()
            Return Me.m_ee
        End Function

        ''' <summary>
        ''' Sampled diets (pred x prey)
        ''' </summary>
        Public Function DC() As Single(,)
            Return Me.m_dc
        End Function

        ''' <summary>
        ''' Sampled landings (fleet x group)
        ''' </summary>
        Public Function Landing() As Single(,)
            Return Me.m_landing
        End Function

        ''' <summary>
        ''' Sampled discards (fleet x group)
        ''' </summary>
        Public Function Discard() As Single(,)
            Return Me.m_discard
        End Function

#Region " Variable access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cEcopathDataStructures.Emig">emigration rate relative to biomass</see>
        ''' ratio for this group.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Rating() As Integer
            Get
                Return CInt(GetVariable(eVarNameFlags.SampleRating))
            End Get
            Set(ByVal value As Integer)
                SetVariable(eVarNameFlags.SampleRating, value)
            End Set
        End Property

        Public Property RatingStatus As eStatusFlags
            Get
                Return Me.GetStatus(eVarNameFlags.SampleRating)
            End Get
            Set(value As eStatusFlags)
                Me.SetStatus(eVarNameFlags.SampleRating, value)
            End Set
        End Property

#End Region ' Variable access 

#Region " Summary "

        Friend Sub CalculateStats()

            Me.m_dtPerturbed.Clear()

            For iGroup As Integer = 1 To Me.m_core.nGroups
                For iFleet As Integer = 1 To Me.m_core.nFleets
                    If Me.m_landing(iFleet, iGroup) <> cCore.NULL_VALUE Then Me.m_dtPerturbed(eVarNameFlags.Discards) = True
                    If Me.m_discard(iFleet, iGroup) <> cCore.NULL_VALUE Then Me.m_dtPerturbed(eVarNameFlags.Discards) = True
                Next
                For iPred As Integer = 1 To Me.m_core.nLivingGroups
                    If Me.m_dc(iPred, iGroup) <> cCore.NULL_VALUE Then Me.m_dtPerturbed(eVarNameFlags.DietComp) = True
                Next
                If (Me.B(iGroup) <> cCore.NULL_VALUE) Then Me.m_dtPerturbed(eVarNameFlags.Biomass) = True
                If (Me.PB(iGroup) <> cCore.NULL_VALUE) Then Me.m_dtPerturbed(eVarNameFlags.PBInput) = True
                If (Me.QB(iGroup) <> cCore.NULL_VALUE) Then Me.m_dtPerturbed(eVarNameFlags.QBInput) = True
                If (Me.BA(iGroup) <> cCore.NULL_VALUE) Then Me.m_dtPerturbed(eVarNameFlags.BioAccumInput) = True
                If (Me.BaBi(iGroup) <> cCore.NULL_VALUE) Then Me.m_dtPerturbed(eVarNameFlags.BioAccumRate) = True
                If (Me.EE(iGroup) <> cCore.NULL_VALUE) Then Me.m_dtPerturbed(eVarNameFlags.EEInput) = True
            Next
            Me.m_bStatsCalculated = True

        End Sub

#End Region ' Summary

    End Class

End Namespace
