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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.SystemUtilities

#End Region

''' ---------------------------------------------------------------------------
''' <summary>
''' Data structures for the Pre-bal interfaces.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cPrebalModel

#Region " Internal helper classes "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class for sorting <see cref="cEcoPathGroupOutput">groups</see> by 
    ''' <see cref="cEcoPathGroupOutput.TTLX">Trophic Level</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cGroupTLComparer
        Implements IComparer(Of cEcoPathGroupOutput)

        Public Property Descending As Boolean = True

        Public Function Compare(x As cEcoPathGroupOutput, y As cEcoPathGroupOutput) As Integer _
            Implements IComparer(Of cEcoPathGroupOutput).Compare

            If (x Is Nothing) Or (y Is Nothing) Then Return 1

            Dim tlx As Single = x.TTLX
            Dim tly As Single = y.TTLX
            Dim iFact As Integer

            If (Me.Descending) Then iFact = -1 Else iFact = 1

            If (tlx = tly) Then Return 0
            If (tlx > tly) Then Return iFact
            Return -iFact

        End Function

    End Class

#End Region ' Internal helper classes

#Region " Private variables "

    Private m_core As cCore = Nothing
    Private m_data() As cPrebalPlotData = Nothing

#End Region ' Private variables

#Region " Construction "

    Public Sub New(core As cCore)
        Me.m_core = core

        ReDim Me.m_data(4)
        Me.m_data(eResultTypes.B) = New cPrebalPlotData(eResultTypes.B)
        Me.m_data(eResultTypes.PB) = New cPrebalPlotData(eResultTypes.PB)
        Me.m_data(eResultTypes.QB) = New cPrebalPlotData(eResultTypes.QB)
        Me.m_data(eResultTypes.PQ) = New cPrebalPlotData(eResultTypes.PQ)

    End Sub

#End Region ' Construction

#Region " Event "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event that notifies that the pre-bal data has been <see cref="cPrebalModel.Update">updated</see>.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    ''' -----------------------------------------------------------------------
    Public Event OnUpdated(sender As Object, args As EventArgs)

#End Region ' Event

#Region " Public interfaces "

    Public Enum eResultTypes As Integer
        NotSet = 0
        B
        PB
        QB
        PQ
    End Enum


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update the data for the pre-bal diagnostics. Call this after Ecopath has ran.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Update()
        Try
            Me.DoUpdate()
            RaiseEvent OnUpdated(Me, New EventArgs())
        Catch ex As Exception

        End Try
    End Sub

    Public Function Data(plot As eResultTypes) As cPrebalPlotData
        Try
            Return Me.m_data(plot)
        Catch ex As Exception
            Debug.Assert(False)
        End Try
        Return Nothing
    End Function

#End Region ' Public interfaces

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update the Pre-bal data cache from EwE model results.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub DoUpdate()

        Dim comp As New cGroupTLComparer()
        Dim grp As cEcoPathGroupOutput = Nothing
        Dim lgrps As New List(Of cEcoPathGroupOutput)
        Dim nConsumers As Integer = 0
        Dim iConsumer As Integer = 1

        ' Get all living groups
        For i As Integer = 1 To Me.m_core.nGroups
            grp = Me.m_core.EcoPathGroupOutputs(i)
            If (grp.IsLiving) Then
                lgrps.Add(grp)
                If (grp.IsConsumer) Then nConsumers += 1
            End If
        Next

        ' Sort the groups by trophic level, descending
        comp.Descending = True
        lgrps.Sort(comp)

        ' Allocate memory
        Me.m_data(eResultTypes.B).Resize(lgrps.Count)
        Me.m_data(eResultTypes.PB).Resize(lgrps.Count)
        Me.m_data(eResultTypes.QB).Resize(nConsumers)
        Me.m_data(eResultTypes.PQ).Resize(nConsumers)

        ' ToDo: take stanza-ness into account: find proper values, and set proper statuses?

        ' Update data
        For i As Integer = 1 To lgrps.Count

            ' Get real EwE group
            grp = lgrps(i - 1)

            Me.m_data(eResultTypes.B).EcopathGroupIndexes(i) = grp.Index
            Me.m_data(eResultTypes.B).Data(i) = grp.Biomass
            Me.m_data(eResultTypes.B).Status(i) = grp.BiomassStatus

            Me.m_data(eResultTypes.PB).EcopathGroupIndexes(i) = grp.Index
            Me.m_data(eResultTypes.PB).Data(i) = grp.PBOutput
            Me.m_data(eResultTypes.PB).Status(i) = grp.PBStatus

            If (grp.IsConsumer) Then

                Me.m_data(eResultTypes.QB).EcopathGroupIndexes(iConsumer) = grp.Index
                Me.m_data(eResultTypes.QB).Data(iConsumer) = grp.QBOutput
                Me.m_data(eResultTypes.QB).Status(iConsumer) = grp.QBStatus

                Me.m_data(eResultTypes.PQ).EcopathGroupIndexes(iConsumer) = grp.Index
                Me.m_data(eResultTypes.PQ).Data(iConsumer) = grp.PBOutput / grp.QBOutput
                Me.m_data(eResultTypes.PQ).Status(iConsumer) = grp.PBStatus Or grp.QBStatus

                iConsumer += 1

            End If

        Next

        ' Check orders of magnitude
        ' Check slope
        ' Check above slope, below slope

    End Sub

#End Region ' Internals

End Class
