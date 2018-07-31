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
Imports System.ComponentModel
Imports System.Reflection
Imports EwEUtils.Utilities
Imports EwEUtils.Database
Imports EwECore
Imports ScientificInterfaceShared.Style

#End Region ' Imports

<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Name"), _
    Serializable()> _
Public MustInherit Class cUnit
    Inherits EwEUtils.Database.cEwEDatabase.cOOPStorable

    Protected Const sPROPCAT_GENERAL As String = "01. General"
    Protected Const sPROPCAT_VALIDATION As String = "02. Validation"
    Protected Const sPROPCAT_PRODUCTS As String = "03. Products ($/t)"
    Protected Const sPROPCAT_REVENUE As String = "04. Revenue ($/effort)"
    Protected Const sPROPCAT_SUBSIDIES As String = "05. Subsidies ($/t)"
    Protected Const sPROPCAT_PAY As String = "06. Pay ($/t)"
    Protected Const sPROPCAT_SHARE As String = "07. Share (% revenue)"
    Protected Const sPROPCAT_INPUTCOST As String = "08. Input cost ($/t)"
    Protected Const sPROPCAT_TAXES As String = "09. Taxes ($/t)"
    Protected Const sPROPCAT_SOCIAL As String = "10. Social (#/t)"

    ''' <summary>Index of the unit, which this unit needs to store its values in the Results object</summary>
    Private m_iSequence As Integer = 0
    ''' <summary>List of input variables that this unit needs in order to perform its calculations.</summary>
    Protected m_lReceivedInputs As New List(Of cInput)
    ''' <summary>Name of the unit</summary>
    Private m_strName As String
    ''' <summary>Local name of the unit.</summary>
    Private m_strNameLocal As String
    ''' <summary>Nationality of a unit.</summary>
    Private m_iNationality As Integer
    ''' <summary>Zhe ceur</summary>
    Private m_core As cCore = Nothing

    Private m_bCanCompute As Boolean = False
    Private m_bRunStarted As Boolean = False

    ''' <summary>Units that receive outputs from this unit.</summary>
    Protected m_llinkOutput As New List(Of cLink)
    ''' <summary>Units that provide inputs for this unit.</summary>
    Protected m_llinkInput As New List(Of cLink)

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Links "

    Public Function LinkOutCount() As Integer
        Return Me.m_llinkOutput.Count
    End Function

    Public Function LinkOut(ByVal iIndex As Integer) As cLink
        Return Me.m_llinkOutput(iIndex)
    End Function

    ''' <summary>
    ''' Get all links directly linking to a target.
    ''' </summary>
    ''' <param name="unitTarget"></param>
    ''' <returns></returns>
    Public Function Links(ByVal unitTarget As cUnit) As cLink()
        Dim lLinks As New List(Of cLink)
        For Each link As cLink In Me.m_llinkOutput
            If ReferenceEquals(link.Target, unitTarget) Then
                lLinks.Add(link)
            End If
        Next
        Return lLinks.ToArray
    End Function

    Public Sub AddLink(ByVal link As cLink)

        ' Sanity check
        Debug.Assert(ReferenceEquals(link.Source, Me))

        Me.m_llinkOutput.Add(link)
        link.Target.AddInputLink(link)
    End Sub

    Public Sub RemoveLink(ByVal link As cLink)
        Me.m_llinkOutput.Remove(link)
        link.Target.RemoveInputLink(link)
    End Sub

    Public Function LinkInCount() As Integer
        Return Me.m_llinkInput.Count
    End Function

    Public Function LinkIn(ByVal iIndex As Integer) As cLink
        Return Me.m_llinkInput(iIndex)
    End Function

    Protected Sub AddInputLink(ByVal link As cLink)
        ' Sanity check
        Debug.Assert(ReferenceEquals(link.Target, Me))
        Me.m_llinkInput.Add(link)
        Me.UpdateComputeStatus()
    End Sub

    Protected Sub RemoveInputLink(ByVal link As cLink)
        Me.m_llinkInput.Remove(link)
        Me.UpdateComputeStatus()
    End Sub

    Public Function IsLoop(ByVal unit As cUnit) As Boolean

        ' Linked to self?
        Dim bIsLoop As Boolean = ReferenceEquals(unit, Me)

        ' If no loop yet
        If Not bIsLoop Then
            ' Follow each output link
            For Each link As cLink In Me.m_llinkOutput
                ' See the target link is the requesting unit
                If link.Target.IsLoop(unit) Then bIsLoop = True : Exit For
            Next link
        End If

        Return bIsLoop
    End Function

    'Public Overridable Function HasTarget(ByVal unit As cUnit) As Boolean

    '    ' Follow each output link
    '    For Each link As cLink In Me.m_llinkOutput
    '        ' See the target link is the requesting unit
    '        If Object.ReferenceEquals(link.Target, unit) Then Return True
    '    Next link
    '    Return False

    'End Function

#End Region ' Links 

#Region " Running "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the unit for a new Ecosim or Ecospace run.
    ''' </summary>
    ''' <param name="iSequence">The sequence number to assign to this unit for the run.</param>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub InitRun(ByVal iSequence As Integer)
        Me.Sequence = iSequence
        Me.m_bRunStarted = True
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the unit for running a chain.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub Clear()
        ' Clear all pending inputs
        Me.m_lReceivedInputs.Clear()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Calculate the economics for this unit.
    ''' </summary>
    ''' <param name="results"></param>
    ''' <param name="input"></param>
    ''' <param name="iTimeStep"></param>
    ''' <param name="iUnit">The unit to aggregate by.</param>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub Process(ByVal results As cResults, _
                                   ByVal input As cInput, _
                                   ByVal iTimeStep As Integer, _
                                   ByVal iUnit As Integer)

        Dim sTotalOutputBiomass As Single = 0
        Dim sTotalOutputValue As Single = 0
        Dim sValuePerTon As Single = 0.0!

        ' Store received values
        Me.m_lReceivedInputs.Add(input)

        ' At least expected inputs received?
        If (Me.m_lReceivedInputs.Count >= Me.LinkInCount()) Then

            ' #Yes: Process combined inputs
            input = Me.ProcessAndSumInputs(Me.m_lReceivedInputs, results)

            ' Store the amount that each fleet contributes to the total
            results.StoreContribution(iUnit, Me, iTimeStep, input.Value, input.Tons)

            ' Determine outgoing biomass
            For Each link As cLink In Me.m_llinkOutput
                ' Determine output biomass for a single link
                Dim sOutputBiomass As Single = link.BiomassRatio * input.Tons
                Dim sOutputValue As Single = 0

                If ((link.ValuePerTon <> 1.0!) And (link.ValuePerTon <> 0)) Or _
                    (input.Tons = 0) Then
                    sOutputValue = link.ValuePerTon * sOutputBiomass
                Else
                    sOutputValue = (input.Value / input.Tons) * link.ValueRatio * sOutputBiomass
                End If

                sTotalOutputBiomass += sOutputBiomass
                sTotalOutputValue += sOutputValue

                link.Target.Process(results, New cInput(Me, sOutputBiomass, sOutputValue), iTimeStep, iUnit)

            Next

            ' Running for all fleet?
            If iUnit = 0 Then
                ' #Yes: make all calculations. Calculations are not necessary when running for individual fleets
                '       where only transfer ratios are collected.
                Me.Calculate(results, input.Tons, input.Value, sTotalOutputBiomass, sTotalOutputValue, iTimeStep)
            End If

        End If

    End Sub

    Protected Function ProcessAndSumInputs(ByVal lInputs As List(Of cInput), results As cResults) As cInput

        Dim sTonsTotal As Single = 0.0
        Dim sValueTotal As Single = 0.0

        For Each input As cInput In lInputs
            If input.Tons > 0 Then

                sTonsTotal += input.Tons
                sValueTotal += input.Value

                results.FlowsBiomass(input.Source.Sequence, Me.Sequence) += input.Tons

            End If
        Next
        Return New cInput(Nothing, sTonsTotal, sValueTotal)

    End Function

    ''' <summary>
    ''' Make all calculations.
    ''' </summary>
    ''' <param name="results">The results object to store calculation results in.</param>
    Protected Overridable Function Calculate(ByVal results As cResults, _
            ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
            ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
            ByVal iTimeStep As Integer) As Boolean

        ' All good
        Return True

    End Function

    ''' <summary>
    ''' Assess whether a unit is ready to compute, e.g. when all its
    ''' inputs are (in)directly connected to EwE model data.
    ''' </summary>
    Private Sub UpdateComputeStatus()

        ' Check if all input links can compute
        Dim bCanCompute As Boolean = True
        Dim bHasInputs As Boolean = False
        For Each LinkIn As cLink In Me.m_llinkInput
            bCanCompute = bCanCompute And LinkIn.Source.CanCompute
            bHasInputs = True
        Next
        bCanCompute = bCanCompute And bHasInputs

        ' No changes? Abort
        If (bCanCompute = Me.CanCompute) Then Return

        Me.m_bCanCompute = bCanCompute

        For Each linkOut As cLink In Me.m_llinkOutput
            linkOut.Target.UpdateComputeStatus()
        Next

    End Sub

#End Region ' Running

#Region " Copy / paste "

    Public Overrides Sub CopyFrom(ByVal obj As cEwEDatabase.cOOPStorable)
        Me.AllowEvents = False
        MyBase.CopyFrom(obj)
        Me.AllowEvents = True
    End Sub

#End Region ' Copy / paste

#Region " Properties "

    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

    <Browsable(False)> _
    Public Property Sequence() As Integer
        Get
            Return Me.m_iSequence
        End Get
        Private Set(ByVal value As Integer)
            Me.m_iSequence = value
        End Set
    End Property

    <Browsable(False)> _
    Public Property Core() As cCore
        Get
            Return Me.m_core
        End Get
        Set(ByVal value As cCore)
            Me.m_core = value
        End Set
    End Property

    <Browsable(False)> _
    Public MustOverride ReadOnly Property UnitType() As cUnitFactory.eUnitType

    <Browsable(False)> _
    Public Overridable ReadOnly Property HasError() As Boolean
        Get
            Return False
        End Get
    End Property

    <Browsable(False)> _
    Public Overridable ReadOnly Property Style() As cStyleGuide.eStyleFlags
        Get
            Return cStyleGuide.eStyleFlags.OK
        End Get
    End Property

    <Browsable(False)> _
    Public Overridable ReadOnly Property CanCompute() As Boolean
        Get
            Return Me.m_bCanCompute
        End Get
    End Property

    <Browsable(False)> _
    Public Overridable ReadOnly Property IsRunError() As Boolean
        Get
            ' Return if all results received OR when not ready to run yet
            Return (Me.m_lReceivedInputs.Count < Me.m_llinkInput.Count) And Me.m_bRunStarted
        End Get
    End Property

#Region " General "

    <Browsable(True), _
        Category(sPROPCAT_GENERAL), _
        DisplayName("Name"), _
        Description("Name of this unit"), _
        cPropertySorter.PropertyOrder(1)> _
    Public Overridable Property Name() As String
        Get
            Return m_strName
        End Get
        Set(ByVal value As String)
            Me.m_strName = value
            Me.SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_GENERAL), _
        DisplayName("Category"), _
        Description("Category to which this unit belongs"), _
        cPropertySorter.PropertyOrder(2)> _
    Public MustOverride ReadOnly Property Category() As String

    <Browsable(True), _
        Category(sPROPCAT_VALIDATION), _
        DisplayName("Biomass ratio"), _
        Description("Total biomass ratio passed out of this unit"), _
        cPropertySorter.PropertyOrder(8)> _
    Public Overridable ReadOnly Property BiomassRatio() As String
        Get
            Dim sTot As Single = 0
            For i As Integer = 0 To Me.LinkOutCount - 1
                sTot += Me.LinkOut(i).BiomassRatio
            Next
            Return sTot.ToString()
        End Get
    End Property

    <Browsable(True), _
        Category(sPROPCAT_GENERAL), _
        DisplayName("Nationality"), _
        Description("Nationality of this unit"), _
        cPropertySorter.PropertyOrder(4)> _
    Public Overridable Property Nationality() As Integer
        Get
            Return Me.m_iNationality
        End Get
        Set(ByVal value As Integer)
            Me.m_iNationality = value
            Me.SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_GENERAL), _
        DisplayName("Name (local)"), _
        Description("Local name of this unit"), _
        cPropertySorter.PropertyOrder(5)> _
    Public Overridable Property NameLocal() As String
        Get
            Return m_strNameLocal
        End Get
        Set(ByVal value As String)
            Me.m_strNameLocal = value
            Me.SetChanged()
        End Set
    End Property

#End Region ' General

#End Region ' Properties

End Class
