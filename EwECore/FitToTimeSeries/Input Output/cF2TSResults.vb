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

Imports EwECore

Namespace FitToTimeSeries

#Region " cF2TSResultsFactory "

    Friend Class cF2TSResultsFactory

        Shared Function Create(ByVal runType As eRunType) As cF2TSResults
            Dim data As cF2TSResults = Nothing
            Select Case runType

                Case eRunType.Idle
                    '

                Case eRunType.SensitivitySS2VByPredPrey, eRunType.SensitivitySS2VByPredator
                    data = New cSensitivityToVulResults(runType)
                
                Case eRunType.Search
                    data = New cSearchResults(runType)

            End Select
            Return data
        End Function

        Shared Function CreateCopy(ByVal results As cF2TSResults) As cF2TSResults
            Dim resultsClone As cF2TSResults = cF2TSResultsFactory.Create(results.RunType)
            results.CopyTo(resultsClone)
            Return resultsClone
        End Function

    End Class

#End Region ' cF2TSResultsFactory

#Region " cF2TSResults "

    Public MustInherit Class cF2TSResults

        Protected m_runType As eRunType ' = eRunType.Idle
        Protected m_iStep As Integer = 0
        Protected m_nSteps As Integer = 0
        Protected m_BaseSS As Single

        Friend Sub New(ByVal runType As eRunType)
            Me.m_runType = runType
        End Sub

        Public Overridable Sub CopyTo(ByVal results As cF2TSResults)
            If results Is Nothing Then Return
            results.m_runType = Me.m_runType
        End Sub

        Public Property RunType() As eRunType
            Get
                Return Me.m_runType
            End Get
            Friend Set(ByVal value As eRunType)
                Me.m_runType = value
            End Set
        End Property


        ' Returns the 
        Public Property iStep() As Integer
            Get
                Return Me.m_iStep
            End Get
            Set(ByVal value As Integer)
                m_iStep = value
            End Set
        End Property

        Public ReadOnly Property nSteps() As Integer
            Get
                Return Me.m_nSteps
            End Get
        End Property

        Public Property BaseSS() As Single
            Get
                Return m_BaseSS
            End Get
            Friend Set(ByVal value As Single)
                m_BaseSS = value
            End Set
        End Property

    End Class

#End Region ' cF2TSResults

#Region " cSensitivitySS2VByPredPreyResults "

    Public Class cSensitivityToVulResults
        Inherits cF2TSResults
        Implements IComparable(Of cSensitivityToVulResults)



        'To be removed in the real thing
        Protected m_arySsen() As Single '= {0, 300.5268, 300.5222, 300.5268, 300.5268, 300.5268, 300.5269, 300.5268, 300.5245, 300.5378, 300.5289, 300.5257, 300.5239, 300.5268, 300.526, 300.5255, 300.5251, 300.5284, 300.5257, 300.5263, 300.5406, 300.5253, 300.4875, 300.5234, 300.5242, 300.5246, 300.5265, 300.5266, 300.5268, 300.5268, 300.5268, 300.5268, 300.5268, 300.5268, 300.5268, 300.5268, 300.5272, 300.5276, 300.5323, 300.5269, 300.5266, 300.5259, 300.5234, 300.5273, 300.5275, 300.5404, 300.5278, 300.5264, 300.5261, 300.5267, 300.5269, 300.5277, 300.5287, 300.5266, 300.5296, 300.5324, 300.5266, 300.5273, 300.5268, 300.526, 300.5222, 300.5211, 300.5222, 300.5271, 300.5272, 300.5286, 300.5296, 300.5306, 300.5457, 300.5286, 300.5266, 300.5382, 300.5305, 300.5558, 300.527, 300.5269, 300.5268, 300.5354, 300.5273, 300.5327, 300.548, 300.5385, 300.5294, 300.5275, 300.5245, 300.5241, 300.3995, 300.4747, 300.5267, 300.5266, 300.5269, 300.526, 300.5178, 300.5247, 300.5757, 300.8174, 300.5086, 300.5266, 300.5304, 300.5242, 300.5249, 300.4996, 300.4992, 300.5206, 300.5248, 300.5267, 300.5193, 300.5239, 300.5325, 300.5246, 300.5254, 300.5297, 300.5271, 300.5268, 300.5268, 300.5267, 300.5268, 300.5276, 300.5278, 300.5268, 300.5268, 300.5268, 300.5268, 300.5268, 300.5271, 300.5328, 300.533, 300.5551, 300.5478, 300.5274, 300.5305, 300.5326, 300.5277, 300.5298, 300.5394, 300.5549, 300.4502, 300.9679, 301.2622, 300.5279, 300.528, 300.5746, 300.7383, 301.0592, 300.6063, 300.5267, 300.5271, 300.5922, 300.5212, 300.5297, 300.551, 300.5285, 300.5259, 300.532, 300.5291, 300.5288, 300.5332, 300.5268, 300.5334, 300.5281, 300.529, 300.5282, 300.5268, 300.5226, 300.5248, 300.57, 300.5234, 300.5114, 300.5198, 300.5245}
        'Keep this in the real thing
        Protected m_Ssen As Single

        Protected m_SSMax As Single
        'To be removed in the real thing
        Protected m_Temp As Integer

        Private m_ipred As Integer
        Private m_iprey As Integer

        Friend Sub New(ByVal runType As eRunType)
            MyBase.New(runType)
        End Sub


        Friend Sub New(ByVal runType As eRunType, ByVal iPred As Integer, ByVal iprey As Integer, ByVal SS As Single, ByVal maxSS As Single)
            MyBase.New(runType)

            Me.m_ipred = iPred
            Me.m_iprey = iprey
            Me.m_Ssen = SS
            Me.m_SSMax = maxSS

        End Sub

        Public Overrides Sub CopyTo(ByVal results As cF2TSResults)
            MyBase.CopyTo(results)

            If results Is Nothing Then Return

            ' Copy
            If TypeOf results Is cSensitivityToVulResults Then

                Dim src As cSensitivityToVulResults = DirectCast(results, cSensitivityToVulResults)

                ReDim Me.m_arySsen(src.m_arySsen.Length - 1)

                Me.m_Ssen = src.m_Ssen
                Array.Copy(src.m_arySsen, Me.m_arySsen, src.m_arySsen.Length)
                Me.m_Temp = src.m_Temp
            End If

        End Sub

        Public Property SSen() As Single
            Get
                Return m_Ssen
            End Get

            Friend Set(ByVal value As Single)
                m_Ssen = value
            End Set

        End Property


        Public Property SSMax() As Single
            Get
                Return m_SSMax
            End Get

            Friend Set(ByVal value As Single)
                m_SSMax = value
            End Set

        End Property

        'To be removed in the real thing
        Public WriteOnly Property StepNumSensitivityPredPrey() As Integer
            Set(ByVal value As Integer)
                m_Temp = value
                m_Ssen = m_arySsen(m_Temp)
            End Set
        End Property


        Public Property iPred() As Integer
            Get
                Return m_ipred
            End Get
            Friend Set(ByVal value As Integer)
                m_ipred = value
            End Set
        End Property

        Public Property iPrey() As Integer
            Get
                Return m_iprey
            End Get
            Friend Set(ByVal value As Integer)
                m_iprey = value
            End Set
        End Property

        Public Function CompareTo(ByVal other As cSensitivityToVulResults) As Integer Implements System.IComparable(Of cSensitivityToVulResults).CompareTo
            ' Return m_Ssen.CompareTo(other.SSen)

            If other.SSen < Me.SSen Then
                Return -1
            ElseIf other.SSen = Me.SSen Then
                Return 0
            ElseIf other.SSen > Me.SSen Then
                Return 1
            End If
        End Function

    End Class

#End Region ' cSensitivitySS2VByPredPreyResults

#Region " cSearchResults "

    Public Class cSearchResults
        Inherits cF2TSResults

        'To be removed in the real thing
        Protected m_aryIterSS() As Single
        'Keep this in the real thing
        Protected m_IterSS As Single

        Protected m_AIC As Single
        Protected m_nAICPars As Integer

        Friend Sub New(ByVal runType As eRunType)
            MyBase.New(eRunType.Search)
        End Sub

        Public Overrides Sub CopyTo(ByVal results As cF2TSResults)
            MyBase.CopyTo(results)

            If results Is Nothing Then Return

            ' Copy
            If TypeOf results Is cSearchResults Then
                Dim src As cSearchResults = DirectCast(results, cSearchResults)

                ReDim Me.m_aryIterSS(src.m_aryIterSS.Length - 1)

                Me.m_BaseSS = src.m_BaseSS
                Me.m_IterSS = src.m_IterSS
                Array.Copy(src.m_aryIterSS, Me.m_aryIterSS, src.m_aryIterSS.Length)
            End If

        End Sub

        Public Property IterSS() As Single
            Get
                Return m_IterSS
            End Get
            Set(ByVal value As Single)
                m_IterSS = value
            End Set
        End Property

        Public Property AIC() As Single
            Get
                Return Me.m_AIC
            End Get
            Set(ByVal value As Single)
                m_AIC = value
            End Set
        End Property


        Public Property nAICPars() As Integer
            Get
                Return Me.m_nAICPars
            End Get
            Set(ByVal value As Integer)
                m_nAICPars = value
            End Set
        End Property

    End Class

#End Region ' cSearchResults

End Namespace
