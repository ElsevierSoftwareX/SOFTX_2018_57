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

Imports EwECore
Imports EwEUtils.Core

Public Class cAnomalySearchShapeGUIHandler
    Inherits cForcingShapeGUIHandler

    Private m_lProducers As List(Of Integer)

    Public Sub New(uic As cUIContext)
        MyBase.New(uic)

        ' Make snapshot of PP group indexes
        Me.m_lProducers = New List(Of Integer)
        For iGroup As Integer = 1 To Core.nGroups
            Dim grp As cEcoPathGroupInput = Core.EcoPathGroupInputs(iGroup)
            If (grp.IsProducer) Then
                Me.m_lProducers.Add(iGroup)
            End If
        Next
    End Sub

    ''' ---------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="stb"></param>
    ''' <param name="sp"></param>
    ''' ---------------------------------------------------------------
    Public Shadows Sub Attach(ByVal stb As ucShapeToolbox, _
                              ByVal sp As ucSketchPad)
        MyBase.Attach(stb, Nothing, sp, Nothing)
    End Sub

    ''' ---------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="shape"></param>
    ''' <returns></returns>
    ''' ---------------------------------------------------------------
    Protected Overrides Function IncludeShape(ByVal shape As EwECore.cShapeData) As Boolean

        If (Me.UIContext Is Nothing) Then Return False
        If Not (TypeOf shape Is cForcingFunction) Then Return False

        ' Fixed 
        Dim interactions As cMediatedInteractionManager = Core.MediatedInteractionManager
        Dim shapes As New List(Of cShapeData)
        Dim shpTest As cForcingFunction = Nothing
        Dim interact As cPredPreyInteraction = Nothing
        Dim ft As eForcingFunctionApplication = eForcingFunctionApplication.NotSet

        For Each iGroup As Integer In Me.m_lProducers
            interact = interactions.PredPreyInteraction(iGroup, iGroup)
            If (interact IsNot Nothing) Then
                For i As Integer = 1 To interact.nAppliedShapes
                    If (interact.getShape(i, shpTest, ft)) Then
                        If ReferenceEquals(shape, shpTest) Then
                            Return True
                        End If
                    End If
                Next
            End If
        Next
        Return False

    End Function

    Public Overrides Function NumDataYears() As Integer
        Return Me.UIContext.Core.nTimeSeriesYears
    End Function

End Class
