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

#End Region ' Imports

Namespace Samples

    ''' <summary>
    ''' Data structures for sampled Ecopath models.
    ''' <seealso cref="cEcopathSampleManager"/>.
    ''' <seealso cref="cEcopathSample"/>.
    ''' </summary>
    Public Class cEcopathSampleDatastructures

        Private m_ecopathds As cEcopathDataStructures = Nothing
        Friend m_samples As New List(Of cEcopathSample)
        Friend m_loaded As cEcopathSample = Nothing
        Friend m_backup As cEcopathSample = Nothing

        Friend Sub New(ecopathDS As cEcopathDataStructures)
            Me.m_ecopathds = ecopathDS
        End Sub

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an available sample.
        ''' <seealso cref="nSamples"/>
        ''' </summary>
        ''' <param name="iSample">The one-based index of the sample to retrieve.
        ''' This index cannot exceed <see cref="nSamples">the total number of
        ''' available samples</see></param>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Sample(iSample As Integer) As cEcopathSample
            Get
                If (iSample < 1 Or iSample > Me.nSamples) Then Return Nothing
                Return Me.m_samples(iSample - 1)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number of available samples
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property nSamples As Integer
            Get
                Return Me.m_samples.Count
            End Get
        End Property

#End Region ' Public access

    End Class

End Namespace
