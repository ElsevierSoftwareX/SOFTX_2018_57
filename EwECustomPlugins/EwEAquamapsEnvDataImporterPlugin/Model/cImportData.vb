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
' Copyright 1991- Ecopath International Initiative, Barcelona, Spain and
'                 Joint Reseach Centre, Ispra, Italy.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore

#End Region ' Imports

''' <summary>
''' Data for the Aquamaps distribution envelope import process.
''' </summary>
Public Class cImportData

#Region " Helper classes "

    ''' <summary>
    ''' Data that holds information from a single Aquamaps envelope file.
    ''' </summary>
    Public Class cFileData

        ''' <summary>The species that the file was read for.</summary>
        Private m_strSpecies As String = ""
        ''' <summary>Envelopes contained in the file</summary>
        Private m_lEnvelopes As New List(Of cEnvelopeData)

        Public Sub New(strSpecies As String)
            Me.m_strSpecies = strSpecies
        End Sub

        Public ReadOnly Property Species As String
            Get
                Return Me.m_strSpecies
            End Get
        End Property

        Public ReadOnly Property Envelopes As cEnvelopeData()
            Get
                Return Me.m_lEnvelopes.ToArray
            End Get
        End Property

        Public Sub AddFunction(strName As String, sMin As Single, sMinPref As Single, sMaxPref As Single, sMax As Single)
            Me.m_lEnvelopes.Add(New cEnvelopeData(strName, sMin, sMinPref, sMaxPref, sMax))
        End Sub

    End Class

    ''' <summary>
    ''' Data that holds information from a single envelope.
    ''' </summary>
    Public Class cEnvelopeData
        Inherits cTrapezoidShapeFunction

        Public Sub New(strName As String, sMin As Single, sMinPref As Single, sMaxPref As Single, sMax As Single)
            Me.Name = strName
            Me.LeftBottom = sMin
            Me.LeftTop = sMinPref
            Me.RightTop = sMaxPref
            Me.RightBottom = sMax
        End Sub

        Public Property Name As String

    End Class

#End Region ' Helper classes

#Region " Private vars "

    Private m_lFileData As New List(Of cFileData)

#End Region ' Private vars

#Region " Public bits "

    ''' <summary>
    ''' Clear the data.
    ''' </summary>
    Public Sub Clear()
        Me.m_lFileData.Clear()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a file to the data.
    ''' </summary>
    ''' <param name="strSpecies">The species for the file that was read.</param>
    ''' <returns>A <see cref="cFileData">file data block</see> to populate further.</returns>
    ''' -----------------------------------------------------------------------
    Public Function AddFile(strSpecies As String) As cFileData

        Dim fd As New cFileData(strSpecies)
        Me.m_lFileData.Add(fd)
        Return fd

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get an array of all unique species names that have been read.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Species As String()
        Get
            Dim lSpecies As New List(Of String)
            For Each f As cFileData In Me.m_lFileData
                If Not lSpecies.Contains(f.Species) Then
                    lSpecies.Add(f.Species)
                End If
            Next
            Return lSpecies.ToArray
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get an array of all unique envelope names that have been read.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Envelopes As String()
        Get
            Dim lEnv As New List(Of String)
            For Each f As cFileData In Me.m_lFileData
                For Each e As cEnvelopeData In f.Envelopes
                    If Not lEnv.Contains(e.Name) Then
                        lEnv.Add(e.Name)
                    End If
                Next
            Next
            Return lEnv.ToArray
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get an array of the <see cref="cFileData">files</see> that have been read.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Files As cFileData()
        Get
            Return Me.m_lFileData.ToArray
        End Get
    End Property

#End Region ' Public bits

End Class
