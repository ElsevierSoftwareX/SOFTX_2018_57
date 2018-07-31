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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwECore

#End Region ' Imports

Namespace Ecosim

    ''' <summary>
    ''' Grid for displaying Fit to time series run results
    ''' </summary>
    <CLSCompliant(False)> _
    Public Class gridFitToTimeSeriesOutput
        Inherits EwEGrid

        Private Enum eColumnTypes As Integer
            Index
            NoParams
            NoAICPoints
            SS
            AIC
        End Enum

        ''' <summary>
        ''' 
        ''' </summary>
        Private Class cOutput

            Private m_iNumParams As Integer
            Private m_sSS As Single
            Private m_sAIC As Single

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="iNumParams"></param>
            ''' <param name="sSS"></param>
            ''' <param name="sAIC"></param>
            Public Sub New(ByVal iNumParams As Integer, _
                           ByVal sSS As Single, _
                           ByVal sAIC As Single)
                Me.m_iNumParams = iNumParams
                Me.m_sSS = sSS
                Me.m_sAIC = sAIC
            End Sub

            Public ReadOnly Property NumParams As Integer
                Get
                    Return Me.m_iNumParams
                End Get
            End Property

            Public ReadOnly Property SS As Single
                Get
                    Return Me.m_sSS
                End Get
            End Property

            Public Property AIC As Single
                Get
                    Return Me.m_sAIC
                End Get
                Set(ByVal value As Single)
                    Me.m_sAIC = value
                End Set
            End Property

        End Class

        Private m_lData As New List(Of cOutput)
        Private m_man As cF2TSManager = Nothing
        Private m_propAIC As cProperty = Nothing
        Private m_iNumAICPoints As Integer = 0

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
                If (Me.UIContext IsNot Nothing) Then
                    RemoveHandler Me.m_propAIC.PropertyChanged, AddressOf OnAICNumPointsChanged
                    Me.m_propAIC = Nothing
                    Me.m_man = Nothing
                End If

                MyBase.UIContext = value

                If (Me.UIContext IsNot Nothing) Then
                    Me.m_man = Me.UIContext.Core.EcosimFitToTimeSeries
                    Me.m_propAIC = Me.UIContext.PropertyManager.GetProperty(Me.m_man, EwEUtils.Core.eVarNameFlags.F2TSNAICData)
                    AddHandler Me.m_propAIC.PropertyChanged, AddressOf OnAICNumPointsChanged
                    ' Kick
                    Me.OnAICNumPointsChanged(Me.m_propAIC, cProperty.eChangeFlags.Value)
                End If
            End Set
        End Property

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            ' ToDo_JS: Globalize this

            Me.Redim(1 + Me.m_lData.Count, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.NoParams) = New EwEColumnHeaderCell(SharedResources.HEADER_NUMPARAMS)
            Me(0, eColumnTypes.NoAICPoints) = New EwEColumnHeaderCell("Number of AIC data points")
            Me(0, eColumnTypes.SS) = New EwEColumnHeaderCell(SharedResources.HEADER_SS)
            Me(0, eColumnTypes.AIC) = New EwEColumnHeaderCell(SharedResources.HEADER_AIC)

            Me.FixedColumnWidths = True
            Me.FixedColumns = 1

        End Sub

        Protected Overrides Sub FillData()
            For i As Integer = 0 To Me.m_lData.Count - 1
                Dim out As cOutput = Me.m_lData(i)
                Me(i + 1, eColumnTypes.Index) = New EwERowHeaderCell(CStr(i + 1))
                Me(i + 1, eColumnTypes.NoParams) = New EwECell(out.NumParams, GetType(Integer), cStyleGuide.eStyleFlags.NotEditable)
                Me(i + 1, eColumnTypes.NoAICPoints) = New EwECell(Me.m_iNumAICPoints, GetType(Integer), cStyleGuide.eStyleFlags.NotEditable)
                Me(i + 1, eColumnTypes.SS) = New EwECell(out.SS, GetType(Single), cStyleGuide.eStyleFlags.NotEditable)
                Me(i + 1, eColumnTypes.AIC) = New EwECell(out.AIC, GetType(Single), cStyleGuide.eStyleFlags.NotEditable)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="iNumParams"></param>
        ''' <param name="sSS"></param>
        ''' <remarks></remarks>
        Public Sub AddFitToTimeSeriesOutput(ByVal iNumParams As Integer, ByVal sSS As Single)
            Me.m_lData.Add(New cOutput(iNumParams, sSS, Me.m_man.getAIC(iNumParams, Me.m_iNumAICPoints, sSS)))
            Me.RefreshContent()
        End Sub

        Public Sub Clear()
            Me.m_lData.Clear()
            Me.RefreshContent()
        End Sub

        Public Property NumAICPoints As Integer
            Get
                Return Me.m_iNumAICPoints
            End Get
            Set(ByVal iNumAICPoints As Integer)

                If iNumAICPoints = Me.m_iNumAICPoints Then Return
                Me.m_iNumAICPoints = iNumAICPoints

                For Each out As cOutput In Me.m_lData
                    out.AIC = Me.m_man.getAIC(out.NumParams, Me.m_iNumAICPoints, out.SS)
                Next
                Me.RefreshContent()
            End Set
        End Property

        Private Sub OnAICNumPointsChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)
            If ((changeFlags And cProperty.eChangeFlags.Value) > 0) Then
                Me.NumAICPoints = CInt(Me.m_propAIC.GetValue)
            End If
        End Sub

    End Class

End Namespace
