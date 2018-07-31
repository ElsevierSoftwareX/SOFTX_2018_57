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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Option Strict On
Option Explicit On

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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Imports System.IO
Imports EwECore
Imports EwEUtils.Utilities
Imports LumenWorks.Framework.IO.Csv

#End Region ' Imports

' TO MARK:
' JS 05May14: Sorry, hijacked this file and simplified things a bit:
' - Regulations are stored in a 'sister file' to a strategy rather than in the strategy 
'   file itself, which greatly simplifies matters.
' - The cReg object is gone since all we needed was a method flag for a given fleet

Public Class cRegulations
    Implements IMSEData

    Public Enum eRegMethod As Byte
        None = 1
        WeakestStock
        HighestValue
        SelectiveFishing
        NoFishing
    End Enum

    Private m_methods As eRegMethod()
    Private m_bIsChanged As Boolean = False

    Private m_MSE As cMSE = Nothing
    Private m_Core As cCore = Nothing

    Sub New(MSE As cMSE, Core As cCore)
        Me.m_MSE = MSE
        Me.m_Core = Core
        ReDim m_methods(Core.nFleets)
        ' Set proper defaults
        For i As Integer = 0 To Core.nFleets - 1
            Me.m_methods(i) = eRegMethod.None
        Next
        Me.Defaults()
    End Sub

    Public Property Method(iFleet As Integer) As eRegMethod
        Get
            If (iFleet < 1 Or iFleet > Me.m_Core.nFleets) Then Return eRegMethod.None
            Return Me.m_methods(iFleet)
        End Get
        Set(value As eRegMethod)
            If (iFleet < 1 Or iFleet > Me.m_Core.nFleets) Then Return
            If (value <> Me.m_methods(iFleet)) Then
                Me.m_methods(iFleet) = value
                Me.m_bIsChanged = True
            End If
        End Set
    End Property

    Public Function Load(Optional msg As cMessage = Nothing, _
                         Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Load

        Dim buff As String
        Dim recs() As String
        Dim breturn As Boolean = True

        strFilename = strFilename.Replace("_hcr", "_reg")

        Dim reader As StreamReader = cMSEUtils.GetReader(strFilename)
        Try
            If (reader IsNot Nothing) Then

                'read the header line
                reader.ReadLine()
                Do Until reader.EndOfStream

                    buff = reader.ReadLine()
                    recs = buff.Split(","c)
                    Dim iflt As Integer

                    iflt = cStringUtils.ConvertToInteger(recs(1))
                    'get the reg object out of the list based on the fleet index
                    'Debug.Assert(reg.mFleetName = cMSEUtils.FromCSVField(recs(0)), "Oppss Fleetname in file does not match Core Fleetname for fleet." + iflt.ToString)
                    Me.m_methods(iflt) = CType(cStringUtils.ConvertToInteger(recs(2)), eRegMethod)
                    If (Me.m_methods(iflt) = 0) Then Me.m_methods(iflt) = eRegMethod.None
                Loop

            End If '(reader IsNot Nothing)

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString + ".Read() Exception: " + ex.Message)
            cMSEUtils.LogError(msg, "Regulations could not load from " & strFilename & ". " & ex.Message)
            breturn = False
        End Try
        cMSEUtils.ReleaseReader(reader)

        Debug.Assert(breturn, Me.ToString + ".Read() Failed to read regulations from file.")

        Me.m_bIsChanged = False

        Return breturn

    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.Save

        If Not strFilename.Contains("_reg") Then
            Dim index_of_csv As Integer = strFilename.IndexOf(".csv")
            strFilename = strFilename.Insert(index_of_csv, "_reg")
        End If

        Dim strm As StreamWriter = cMSEUtils.GetWriter(strFilename, False)
        If (strm IsNot Nothing) Then

            strm.WriteLine("FleetName,FleetIndex,Regulation")
            For i As Integer = 1 To Me.m_Core.nFleets
                Dim flt As cEcopathFleetInput = Me.m_Core.EcopathFleetInputs(i)
                strm.WriteLine(cStringUtils.ToCSVField(flt.Name) & "," &
                                          cStringUtils.ToCSVField(flt.Index) & "," &
                                          cStringUtils.ToCSVField(Me.m_methods(i)))
            Next
        End If
        cMSEUtils.ReleaseWriter(strm)
        Return True

    End Function

    Public Function IsChanged() As Boolean _
        Implements IMSEData.IsChanged
        Return Me.m_bIsChanged
    End Function

    Public Sub Defaults() Implements IMSEData.Defaults
        For iFleet = 1 To m_Core.nFleets
            Me.Method(iFleet) = eRegMethod.None
        Next
        Me.m_bIsChanged = False
    End Sub

    Public Function FileExists(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.FileExists
        Return File.Exists(strFilename)
    End Function

End Class
