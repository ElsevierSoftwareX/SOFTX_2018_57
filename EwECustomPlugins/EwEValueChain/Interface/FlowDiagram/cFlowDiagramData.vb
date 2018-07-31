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

Option Strict on
Imports System.IO
Imports EwECore
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Public Class cFlowDiagramData
    Implements IFlowDiagramData
    Implements IUIElement

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_data As cData = Nothing
    Private m_results As cResults = Nothing
    Private m_model As cModel = Nothing

    ' Units, to be accessed by iGroup
    Private m_units() As cUnit
    Private m_nLivingGroups As Integer
    Private m_nGroups As Integer

    Private m_sTTLX() As Single
    ''' <summary>DC pred x pred</summary>
    Private m_diets(,) As Single
    Private m_sValueMin As Single
    Private m_sValueMax As Single

    Private m_sLinkValueMin As Single
    Private m_sLinkValueMax As Single

    Private m_bValid As Boolean = False
    Private m_displayvalue As cResults.eGraphDataType = cResults.eGraphDataType.Cost

#End Region ' Private vars

    Public Sub New(ByVal uic As cUIContext, ByVal model As cModel,
                   ByVal data As cData, ByVal results As cResults)

        Me.m_uic = uic
        Me.m_model = model
        Me.m_data = data
        Me.m_results = results

        Dim lUnits As New List(Of cUnit)
        lUnits.Add(Nothing)
        lUnits.AddRange(Me.m_data.GetUnits(cUnitFactory.eUnitType.All))
        Me.m_nGroups = lUnits.Count - 1
        Me.m_nLivingGroups = lUnits.Count - 1

        ReDim Me.m_sTTLX(Me.m_nGroups)
        ReDim Me.m_diets(Me.m_nGroups, Me.m_nGroups)

        Me.m_units = lUnits.ToArray

        Me.Calculate()

    End Sub

#Region " Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cResults.eGraphDataType">graph data type</see> 
    ''' to display.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DisplayValue As cResults.eGraphDataType
        Get
            Return Me.m_displayvalue
        End Get
        Set(value As cResults.eGraphDataType)
            If (value <> Me.m_displayvalue) Then
                Me.m_displayvalue = value
                Dim fmt As New cGraphDataTypeFormatter()
                Me.Title = fmt.GetDescriptor(Me.m_displayvalue, eDescriptorTypes.Name)
                Me.DataTitle = Me.Title
                Me.m_bValid = False
            End If
        End Set
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.ItemColor"/>
    Public ReadOnly Property ItemColor(iGroup As Integer) As System.Drawing.Color _
        Implements IFlowDiagramData.ItemColor
        Get
            Select Case Me.GetUnit(iGroup).UnitType
                Case cUnitFactory.eUnitType.Consumer
                Case cUnitFactory.eUnitType.Distribution
                Case cUnitFactory.eUnitType.Processing
                Case cUnitFactory.eUnitType.Producer
                Case cUnitFactory.eUnitType.Retailer
                Case cUnitFactory.eUnitType.Wholesaler
            End Select
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.ItemName"/>
    Public ReadOnly Property ItemName(iGroup As Integer) As String _
        Implements IFlowDiagramData.ItemName
        Get
            Dim u As cUnit = Me.GetUnit(iGroup)
            Dim strName As String = ""
            If My.Settings.ShowAltNames Then strName = u.NameLocal
            If String.IsNullOrWhiteSpace(strName) Then strName = u.Name
            Return strName
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.IsItemVisible"/>
    Public ReadOnly Property IsItemVisible(iGroup As Integer) As Boolean _
        Implements IFlowDiagramData.IsItemVisible
        Get
            ' ToDo: use filters here
            Return True
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.LinkValue"/>
    Public ReadOnly Property LinkValue(iPred As Integer, iPrey As Integer) As Single _
        Implements IFlowDiagramData.LinkValue
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.m_diets(iPred, iPrey)
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.LinkValueMax"/>
    Public ReadOnly Property LinkValueMax As Single _
        Implements IFlowDiagramData.LinkValueMax
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.m_sLinkValueMax
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.LinkValueMin"/>
    Public ReadOnly Property LinkValueMin As Single _
        Implements IFlowDiagramData.LinkValueMin
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.m_sLinkValueMin
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.NumItems"/>
    Public ReadOnly Property NumItems As Integer _
        Implements IFlowDiagramData.NumItems
        Get
            Return Me.m_nGroups
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.NumLivingItems"/>
    Public ReadOnly Property NumLivingitems As Integer _
        Implements IFlowDiagramData.NumLivingItems
        Get
            Return Me.m_nLivingGroups
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.TrophicLevel"/>
    Public ReadOnly Property TrophicLevel(iGroup As Integer) As Single _
        Implements IFlowDiagramData.TrophicLevel
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.m_sTTLX(iGroup)
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.Refresh"/>
    Public Sub Refresh() _
        Implements IFlowDiagramData.Refresh
        Me.m_bValid = False
    End Sub

    ''' <inheritdocs cref="IFlowDiagramData.Value"/>
    Public ReadOnly Property Value(iGroup As Integer) As Single _
        Implements IFlowDiagramData.Value
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.GetUnitValue(Me.GetUnit(iGroup))
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.ValueLabel"/>
    Public ReadOnly Property ValueLabel(sValue As Single) As String _
        Implements IFlowDiagramData.ValueLabel
        Get
            Return Me.m_uic.StyleGuide.FormatNumber(sValue)
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.ValueMax"/>
    Public ReadOnly Property ValueMax As Single _
        Implements IFlowDiagramData.ValueMax
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.m_sValueMax
        End Get
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.ValueMin"/>
    Public ReadOnly Property ValueMin As Single _
        Implements IFlowDiagramData.ValueMin
        Get
            If Not Me.m_bValid Then Me.Calculate()
            Return Me.m_sValueMin
        End Get
    End Property

    ''' <inheritdocs cref="IUIElement.UIContext"/>
    Public Property UIContext As cUIContext _
        Implements IUIElement.UIContext
        Get
            Return Me.m_uic
        End Get
        Private Set(value As cUIContext)
            ' NOP
        End Set
    End Property

    ''' <inheritdocs cref="IFlowDiagramData.Title"/>
    Public Property Title As String _
        Implements IFlowDiagramData.Title

    ''' <inheritdocs cref="IFlowDiagramData.DataTitle"/>
    Public Property DataTitle As String _
        Implements IFlowDiagramData.DataTitle

#End Region ' Properties

#Region " Internals "

    Private Function GetUnit(iGroup As Integer) As cUnit
        Debug.Assert(iGroup > 0 And iGroup <= Me.m_nGroups)
        Return Me.m_units(iGroup)
    End Function

    Private Sub Calculate()

        Dim fn As cEcoFunctions = Me.m_data.Core.EcoFunction

        ' Trophic level calculations require a temporary PP array
        Dim PP(Me.m_nGroups) As Single

        Me.m_sLinkValueMax = Single.MinValue
        Me.m_sLinkValueMin = Single.MaxValue

        Me.m_sValueMax = Single.MinValue
        Me.m_sValueMin = Single.MaxValue

#If 1 Then

        ' Dump diet matrix
        Dim strModelFile As String = Me.UIContext.Core.DataSource.ToString
        Dim strDCFile As String = Path.Combine(Path.GetDirectoryName(strModelFile), Path.GetFileNameWithoutExtension(strModelFile)) & "_VC_flowOrg.csv"
        Dim sw As New StreamWriter(strDCFile)

        For iPred As Integer = 1 To m_nGroups
            Dim unitPred As cUnit = Me.m_units(iPred)
            sw.Write("," & cStringUtils.ToCSVField(unitPred.Name))
        Next
        sw.WriteLine()
        For iPrey As Integer = 1 To m_nGroups
            Dim unitPrey As cUnit = Me.m_units(iPrey)
            sw.Write(cStringUtils.ToCSVField(unitPrey.Name))
            For iPred As Integer = 1 To m_nGroups
                Dim unitPred As cUnit = Me.m_units(iPred)
                sw.Write("," & cStringUtils.FormatNumber(Me.m_results.FlowsBiomass(unitPrey.Sequence, unitPred.Sequence)))
            Next
            sw.WriteLine()
        Next
        sw.Close()

#End If
        ' -------------------------------------
        ' Compute diets, PP, and value extremes
        ' -------------------------------------

        ' A few notes:
        ' - Unit sequence numbers are zero-based. Diet logic expects one-based indexes
        ' - Diets are dimensioned (pred x prey). In the value chain, this is translated to (target x source)

        For iPred As Integer = 1 To Me.m_nGroups

            Dim total As Double = 0.0#
            Dim val As Single = 0.0!
            Dim unitPred As cUnit = Me.m_units(iPred)

            For iPrey As Integer = 1 To Me.m_nGroups
                Dim unitPrey As cUnit = Me.m_units(iPrey)
                ' Results are ordered as (prey x pred)
                total += Me.m_results.FlowsBiomass(unitPrey.Sequence, unitPred.Sequence)
            Next

            If total > 0 Then
                For iPrey As Integer = 1 To Me.m_nGroups
                    Dim unitPrey As cUnit = Me.m_units(iPrey)
                    ' Convert to single for EwE compatibility. Is ok when normalized, huge precision is not needed then
                    val = CSng(Me.m_results.FlowsBiomass(unitPrey.Sequence, unitPred.Sequence) / total)
                    Me.m_diets(iPred, iPrey) = val
                    ' Track max value
                    Me.m_sLinkValueMin = Math.Min(Me.m_sLinkValueMin, val)
                    Me.m_sLinkValueMax = Math.Max(Me.m_sLinkValueMax, val)
                Next
                'Else
                '    For iPrey As Integer = 1 To Me.m_nGroups
                '        Me.m_diets(iPred, iPrey) = 0.0!
                '    Next
            End If

            If (unitPred.UnitType = cUnitFactory.eUnitType.Producer) Then
                PP(iPred) = 1.0!
            Else
                PP(iPred) = 0.0!
            End If

            ' Compute value extremes
            val = Me.GetUnitValue(unitPred)
            Me.m_sValueMax = Math.Max(Me.m_sValueMax, val)
            Me.m_sValueMin = Math.Min(Me.m_sValueMin, val)

        Next

#If 1 Then

        ' Dump diet matrix
        strDCFile = Path.Combine(Path.GetDirectoryName(strModelFile), Path.GetFileNameWithoutExtension(strModelFile)) & "_VC_flowDC.csv"
        sw = New StreamWriter(strDCFile)

        For iPred As Integer = 1 To m_nGroups
            Dim unitPred As cUnit = Me.m_units(iPred)
            sw.Write("," & cStringUtils.ToCSVField(unitPred.Name))
        Next
        sw.WriteLine()
        For iPrey As Integer = 1 To m_nGroups
            Dim unitPrey As cUnit = Me.m_units(iPrey)
            sw.Write(cStringUtils.ToCSVField(unitPrey.Name))
            For iPred As Integer = 1 To m_nGroups
                Dim unitPred As cUnit = Me.m_units(iPred)
                sw.Write("," & cStringUtils.FormatNumber(Me.m_diets(iPred, iPrey)))
            Next
            sw.WriteLine()
        Next
        sw.Close()

#End If

        ' Calculate trophic levels
        fn.EstimateTrophicLevels(Me.m_nGroups, Me.m_nLivingGroups, PP, Me.m_diets, Me.m_sTTLX)

        ' Done
        Me.m_bValid = True

    End Sub

    ''' <summary>
    ''' Get the value for a unit for the current <see cref="m_displayvalue"/>.
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <returns></returns>
    Private Function GetUnitValue(unit As cUnit) As Single

        Dim sTotal As Single = 0.0
        Dim lUnits As New List(Of cUnit)
        lUnits.Add(unit)

        Dim vars() As cResults.eVariableType = cResults.GetVariables(Me.m_displayvalue)
        If (vars IsNot Nothing) Then
            For Each v As cResults.eVariableType In vars
                sTotal += Me.m_results.GetTotal(v, lUnits.ToArray)
            Next
        End If
        Return sTotal

    End Function

#End Region ' Internals

End Class
