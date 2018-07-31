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

Public Class cDiets
    Implements IMSEData

#Region " Internal Variables "

    Private m_core As cCore
    Private m_MSE As cMSE
    Private m_meanProportions(,) As Single
    Private m_interacts(,) As Integer '(Pred, Prey)
    Private m_meanProportions_imports() As Single
    Private m_interacts_imports() As Integer
    Private m_dietPropMultipliers() As Double

#End Region

#Region " Construction initialisation"

    Public Sub New(MSE As cMSE, core As EwECore.cCore)
        Me.m_core = core
        Me.m_MSE = MSE
        ReDim m_meanProportions(m_core.nLivingGroups - 1, m_core.nGroups - 1)
        ReDim m_dietPropMultipliers(m_core.nLivingGroups - 1)
        ReDim m_interacts(m_core.nLivingGroups - 1, m_core.nGroups - 1)
        ReDim m_meanProportions_imports(m_core.nLivingGroups - 1)
        ReDim m_interacts_imports(m_core.nLivingGroups - 1)
        Me.Defaults()
    End Sub

#End Region

#Region " Properties "

    Public ReadOnly Property Core As cCore
        Get
            Return Me.m_core
        End Get
    End Property

    ''' <summary>
    ''' Mean diet proportions (by predator x prey). Note that predator and prey indices are ZERO-based!
    ''' </summary>
    Public ReadOnly Property MeanProportions As Single(,)
        Get
            Return Me.m_meanProportions
        End Get
    End Property

    ''' <summary>
    ''' Number of diet interactions (by predator x prey). Note that predator and prey indices are ZERO-based!
    ''' </summary>
    Public ReadOnly Property Interacts As Integer(,)
        Get
            Return Me.m_interacts
        End Get
    End Property

    Public ReadOnly Property MeanProportionsImports As Single()
        Get
            Return Me.m_meanProportions_imports
        End Get
    End Property

    Public ReadOnly Property InteractsImports As Integer()
        Get
            Return Me.m_interacts_imports
        End Get
    End Property

    ''' <summary>
    ''' Diet proportion multipliers (by predator). Note that predator indices are ZERO-based!
    ''' </summary>
    Public ReadOnly Property DietPropMultipliers As Double()
        Get
            Return Me.m_dietPropMultipliers
        End Get
    End Property

#End Region

    Public Sub Defaults() _
        Implements IMSEData.Defaults

        Dim mean As Single = 0

        ' Set proper defaults in-memory
        For iPred As Integer = 1 To m_core.nLivingGroups
            mean = m_core.EcoPathGroupInputs(iPred).ImpDiet
            Me.m_meanProportions_imports(iPred - 1) = mean
            'Me.m_meanProportions(iPred - 1, 0) = mean
            'Me.m_interacts(iPred - 1, 0) = IF(mean > 0, 1, 0)
            Me.m_interacts_imports(iPred - 1) = If(mean > 0, 1, 0)
            For iPrey As Integer = 1 To m_core.nGroups
                mean = m_core.EcoPathGroupInputs(iPred).DietComp(iPrey)
                Me.m_meanProportions(iPred - 1, iPrey - 1) = mean
                Me.m_interacts(iPred - 1, iPrey - 1) = If(mean > 0, 1, 0)
            Next
            Me.m_dietPropMultipliers(iPred - 1) = 1.0
        Next

    End Sub

    Public Function IsChanged() As Boolean Implements IMSEData.IsChanged
        Return True
    End Function

    Public Function Load(Optional msg As cMessage = Nothing, _
                         Optional strFilename As String = "") As Boolean Implements IMSEData.Load

        Dim reader As StreamReader = Nothing
        Dim csv As CsvReader = Nothing
        Dim bSuccess As Boolean = True

        strFilename = Me.DefaultFileName()
        reader = cMSEUtils.GetReader(strFilename)
        If (reader IsNot Nothing) Then
            'Read in the values from the DietCompositionMultipliers.csv
            csv = New CsvReader(reader, True)
            Try
                Do While Not csv.EndOfStream
                    If csv.ReadNextRecord() Then
                        m_dietPropMultipliers(cStringUtils.ConvertToInteger(csv(0)) - 1) = cStringUtils.ConvertToInteger(csv(2))
                    End If
                Loop
            Catch ex As Exception
                cMSEUtils.LogError(msg, "DietComposition multipliers cannot load from " & strFilename & ". " & ex.Message)
            End Try
            csv.Dispose()
        End If
        cMSEUtils.ReleaseReader(reader)

        Return bSuccess

    End Function

    Public Function Save(Optional strFilename As String = "") As Boolean Implements IMSEData.Save

        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = False

        strFilename = Me.DefaultFileName()
        writer = cMSEUtils.GetWriter(strFilename, False)
        If (writer IsNot Nothing) Then
            writer.WriteLine("PredatorIndexNumber,PredatorIndexName,Multiplier")
            For iPred As Integer = 1 To m_core.nLivingGroups
                writer.WriteLine("{0},{1},{2}", _
                                 cStringUtils.ToCSVField(iPred), _
                                 cStringUtils.ToCSVField(m_core.EcoPathGroupInputs(iPred).Name), _
                                 cStringUtils.ToCSVField(Me.DietPropMultipliers(iPred - 1)))
            Next
        Else
            bSuccess = False
        End If
        cMSEUtils.ReleaseWriter(writer)
        Return bSuccess

    End Function

    Private Function DefaultFileName() As String
        Return cMSEUtils.MSEFile(Me.m_MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams, "DietCompositionMultipliers.csv")
    End Function

    Public Function FileExists(Optional strFilename As String = "") As Boolean _
        Implements IMSEData.FileExists
        If (String.IsNullOrWhiteSpace(strFilename)) Then
            strFilename = Me.DefaultFileName
        End If
        Return File.Exists(strFilename)
    End Function

End Class
