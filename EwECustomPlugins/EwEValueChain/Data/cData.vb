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
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Database

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Value chain central data storage.
''' </summary>
''' <remarks>
''' Inherited from cCoreInputOutputBase to be able to use cCore.OnChanged.
''' </remarks>
''' ===========================================================================
Public Class cData
    Inherits cCoreInputOutputBase

#Region " Private vars "

    Private m_strDatabase As String = ""
    Private m_parameters As cParameters = New cParameters()

    Private m_lUnits As New List(Of cUnit)
    Private m_lLinks As New List(Of cLink)

    Private m_lUnitDefaults As New List(Of cUnit)
    Private m_lLinkDefaults As New List(Of cLinkDefault)

    Private m_lFlowDiagrams As New List(Of cFlowDiagram)
    Private m_lFlowPositions As New List(Of cFlowPosition)

    Private m_db As New cDatabase()
    Private m_strDBName As String = ""

    Private m_bChanged As Boolean = False
    Private m_bInitializing As Boolean = False

    Private Shared s_inst As cData = Nothing

    Private m_lItems As New List(Of cCoreInputOutputBase)

#End Region ' Private vars 

    Public Sub New(ByVal core As cCore)
        MyBase.New(core)

        cData.s_inst = Me

        Me.m_coreComponent = eCoreComponentType.External
        Me.m_dataType = eDataTypes.External
    End Sub

    Public Shared Function GetInstance() As cData
        Return cData.s_inst
    End Function

    Public Overrides Sub Clear()

        MyBase.Clear()

        ' Properly detach events
        Me.RemoveParameters(Me.m_parameters)
        While Me.m_lUnits.Count > 0
            Me.RemoveUnit(Me.m_lUnits(0))
        End While
        While Me.m_lUnitDefaults.Count > 0
            Me.RemoveUnitDefault(m_lUnitDefaults(0))
        End While
        While Me.m_lLinks.Count > 0
            Me.RemoveLink(Me.m_lLinks(0))
        End While
        While Me.m_lLinkDefaults.Count > 0
            Me.RemoveLinkDefault(Me.m_lLinkDefaults(0))
        End While
        While Me.m_lFlowPositions.Count > 0
            Me.RemoveFlowPosition(Me.m_lFlowPositions(0))
        End While
        While Me.m_lFlowDiagrams.Count > 0
            Me.RemoveFlowDiagram(Me.m_lFlowDiagrams(0))
        End While

        Me.m_bChanged = False
        Me.m_strDBName = ""
    End Sub

#Region " Database access "

    Public Function Load(ByVal strModelName As String) As Boolean

        Dim strOldDBName As String = cData.GetDatabaseFileName(strModelName)
        Dim strDBName As String = ""
        Dim dst As eDataSourceTypes = eDataSourceTypes.NotSet
        Dim bMigrate As Boolean = False
        Dim bSucces As Boolean = False

        Me.Close()

        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_LOADING)

        ' Open for migration
        If File.Exists(strOldDBName) Then
            strDBName = strOldDBName
            Select Case Path.GetExtension(strDBName).ToLower
                Case ".ewevcmdb" : dst = eDataSourceTypes.Access2003
                Case ".ewevcaccdb" : dst = eDataSourceTypes.Access2007
            End Select
            bMigrate = True
        Else
            strDBName = strModelName
        End If

        If Me.m_db.Open(strDBName, dst) = eDatasourceAccessType.Success Then

            Me.m_bInitializing = True
            bSucces = Me.m_db.LoadModel(Me)

            If bSucces = False Then
                Me.SendMessage("Failed to load value chain from database, see error log for details.", eMessageType.Any, eCoreComponentType.External, eMessageImportance.Critical)
                Me.m_db.Close()
                Return False
            End If

            If bMigrate Then

                Me.m_db.Close()
                Me.m_db.Open(strModelName)
                Me.IsChanged = True
                Me.Save()

                Try
                    File.Delete(strOldDBName)
                Catch ex As Exception

                End Try
                Me.SendMessage(cStringUtils.Localize(My.Resources.STATUS_MERGED, Path.GetFileName(strOldDBName), Path.GetFileName(strModelName)), _
                               eMessageType.DataImport, eCoreComponentType.DataSource, eMessageImportance.Information)

            End If
            Me.m_bInitializing = False
        End If

        Me.m_strDBName = strModelName

        cApplicationStatusNotifier.EndProgress(Me.Core)

        ' Start clean
        Me.IsChanged = False

        Return bSucces And Me.m_db.IsConnected

    End Function

    Public Function Close() As Boolean
        If Me.m_db.IsConnected Then
            Me.m_db.Close()
        End If
        Me.Clear()
    End Function

    Public Function Save() As Boolean
        Dim bSucces As Boolean = True

        If (Not Me.m_db.IsConnected) Then Return bSucces
        If (Not Me.IsChanged) Then Return bSucces

        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_SAVING)
        bSucces = Me.m_db.SaveModel(Me)
        cApplicationStatusNotifier.EndProgress(Me.Core)

        If bSucces Then Me.IsChanged = False
        Return bSucces
    End Function

    Public ReadOnly Property Database() As cDatabase
        Get
            Return Me.m_db
        End Get
    End Property

    Public Property IsChanged() As Boolean
        Get
            Return Me.m_bChanged
        End Get
        Set(ByVal value As Boolean)
            ' Update changed value
            Me.m_bChanged = value
            ' Notify core (under strict set of circumstances)
            If (Me.m_bInitializing = False) And (Me.m_core IsNot Nothing) And (Me.m_bChanged = True) Then
                Me.m_core.onChanged(Me)
            End If
        End Set
    End Property

    Private Sub OnElementChanged(ByVal element As cEwEDatabase.cOOPStorable)
        Me.IsChanged = True
    End Sub

#Region " Database helpers "

    Private Shared Function GetDatabaseFileName(ByVal strModelName As String) As String
        Dim strPath As String = Path.GetDirectoryName(strModelName)
        Dim strFileName As String = Path.GetFileNameWithoutExtension(strModelName) + ".ewevcmdb"
        Return Path.Combine(strPath, strFileName)
    End Function

#End Region ' Database helpers

#End Region ' Database access

#Region " From GAMS / Formulas "

#If 0 Then
#Region "Metier"
    '                                                                   Hoaran's variable names
    Public m_MetierCostPUE() As Single 'by metier  Per Unit Effort           C har   m,pri
    Public m_MetierValuePUE() As Single 'by metier Per Unit Effort           y har   m,pri
    Public m_MetierIntermediateProductsUnitPrice() As Single 'by input type  P i
    Public m_MetierLabourUnitPrice() As Single       'by metier              Omega m
    Public m_MetierCapitalUnitPrice() As Single      'by metier              gamma m
    Public m_MetierIntermediateProducts() As Single  'by product             x i
    Public m_MetierLabour() As Single                'by metier              l m
    Public m_MetierCapital() As Single               'by metier              k m
    Public m_MetierCPUE(,) As Single                 'by metier, species     x       m,s
    Public m_MetierPriceRaw(,) As Single             'by metier, species     P raw   m,s
    Public m_MetierCostPublic() As Single            'by metier              C har   m,pub 
    Public m_MetierTaxProductionPUCatch() As Single  'by metier              Tau     m?
    Public m_MetierTaxEnvironmentalPUCatch() As Single 'by metier            Ro      m ?
    Public m_MetierCostManagement() As Single        'by metier              MC      m
    Public m_MetierEffort() As Single                'by metier              E       m
    Public m_MetierBenefitPublic() As Single         'by metier              b har   m,pub   
    Public m_MetierUnitSubsidy() As Single           'by metier              sub     m
    Public m_MetierCost() As Single                'by metier              c har   m
    Public m_MetierBenefit() As Single             'by metier              b har   m
    Public m_MetierProcessUnitCost(,) As Single      'by process, species    c pro   sp,pri
    Public m_MetierProcessRawUnitCost() As Single    'by species             P ld    sp,sp
    Public m_MetierProcessRawUnitAmount() As Single  'by species             x raw   sp,sp
    Public m_MetierOtherIntermediateProductsUnitCost() As Single 'by product P i
    Public m_MetierOtherIntermediateLabourUnitCost() As Single 'by species  omega    sp
    Public m_MetierOtherIntermediateCapitalUnitCost() As Single 'by species gamma    sp
    Public m_MetierOtherIntermediateProducts(,) As Single 'by product, species  x    sp,i
    Public m_MetierOtherIntermediateLabourAmount() As Single 'by species  l sp
    Public m_MetierOtherIntermediateCapitalAmount() As Single 'by species    k sp

    Public m_MetierCatch(,,) As Single                 'by metier,species,time x m s t
    'Public m_MetierEffortTime(,) As Single                 'by metier, time    E m t
    'above already dimensioned without a time dimension
    Public m_MetierProbability(,,) As Single           'by fleet, metier, time PR f m t

#End Region

#Region "Producer"
    Public m_ProducerValue(,) As Single         'by product, sp         y pro   sp,pri 
    Public m_ProducerPrice() As Single          'by sp                  p pro   sp
    Public m_ProducerAmount(,) As Single        'by product, sp         x pro   sp, sp ?
    Public m_ProducerBenefit() As Single       'b producer            b pro   sp,pri
    Public m_ProducerPublicUnitCost() As Single 'by producer           c pro   sp, pub
    Public m_ProducerTaxRate() As Single     'by producer?           tau
    Public m_ProducerManagementCost() As Single 'by producer (total)   MC pro  sp
    Public m_ProducerActivity() As Single      'by producer (total)    E pro   sp
    Public m_ProducerSubsidy() As Single       'by producer            b pro   sp,pub
    Public m_ProducerUnitSubsidy() As Single   'by producer            sub sp
    Public m_ProducerCostFinal() As Single     'by producer            c pro   sp
    Public m_ProducerBenefitFinal() As Single  'by producer            b pro   sp
#End Region

#Region "Distributor"
    Public m_DistributorUnitCost() As Single     'by Distributor           c mak   d pri
    Public m_DistributorRawUnitCost(,) As Single 'by Distributor,sp        P raw   s
    Public m_DistributorRawAmountIn(,) As Single   'by Distributor,sp        x raw   d s
    'x raw d s   in = out?
    Public m_DistributorProcessedUnitcost(,) As Single 'by Distributor,    sp  p pro   sp
    Public m_DistributorProcessedAmount(,) As Single 'by Distributor, sp    x rpo   d, sp
    Public m_DistributorOtherUnitcost(,) As Single 'by Distributor, other   P i
    Public m_DistributorOtherAmount(,) As Single 'by Distributor, other     x d i
    Public m_DistributorLabourUnitCost() As Single 'by Distributor         omega d
    Public m_DistributorLabourAmount() As Single 'by Distributor           l d
    Public m_DistributorCapitalUnitCost() As Single 'by Distributor         gamma d
    Public m_DistributorCapitalAmount() As Single 'by Distributor           k d
    Public m_DistributorProductionRevenue() As Single 'by Distributor       y mak   d pri
    Public m_DistributorRawUnitPrice(,) As Single   'by Distributor, species   P mak,raw   s
    Public m_DistributorRawAmountOut(,) As Single      'by Distributor, species   x raw   d s
    'x raw d s   in = out?
    Public m_DistributorProcessUnitPrice(,) As Single 'by Distributor, species P mak,pro  sp
    Public m_DistributorProcessAmount(,) As Single  'by Distributor, species   x pro   d sp
    Public m_DistributorProductionBenefit() As Single 'by Distributor       b mak   d pri
    Public m_DistributorPublicUnitCost() As Single     'by distributor     c mak   d pub
    Public m_DistributorTaxRate() As Single            'by distributor?    tau
    Public m_DistributorManagementCost() As Single     'by distributor (total) MC mak  d
    Public m_DistributorActivity() As Single           'by distributor     E mak   d
    Public m_DistributorBenefit() As Single            'by distributor     b mak   d pub
    Public m_DistributorUnitSubsidy() As Single        'by distributor     sub d
    Public m_DistributorCostFinal() As Single          'by distributor     c mak   d
    Public m_DistributorBenefitFinal() As Single       'by distributor     b mak   d
#End Region

#Region "Extended"

    Public m_ExtendedCost() As Single                  'by extended ?      c ext   m
    Public m_ExtendedProcessedProp(,,) As Single       'by extended,metier,sp  Teta pro    m sp
    Public m_ExtendedDistributedProp(,,) As Single     'by extended,distributor,metier Teta mak    m d
    Public m_ExtendedDistributedProcessedProp(,,,) As Single 'by exten,dist,processor, metier Teta mak m sp d
    Public m_ExtendedBenefit() As Single               'by extended        b ext m
#End Region

    #Region " General "

        ' JS: Does this need to get saved?

        Public TH As Integer
        Public Port As Integer
        Public SCal As Integer
        Public Spec As Integer
        Public Tech As Integer
        Public Uses As Integer

    #End Region ' General

#End If
#End Region ' From GAMS / Formulas

#Region " Running "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Init the data for a new run by resetting all units.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function InitRun() As Boolean
        Dim unit As cUnit = Nothing
        ' Re-index 
        For iSequence As Integer = 0 To Me.UnitCount - 1
            unit = Me.Unit(iSequence)
            ' Sequence is zero-based 
            unit.InitRun(iSequence)
        Next
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Init the data for a new run by resetting all units.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function InitTimeStep() As Boolean
        Dim unit As cUnit = Nothing
        For iUnit As Integer = 0 To Me.UnitCount - 1
            unit = Me.Unit(iUnit)
            unit.Clear()
        Next
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Diagnostics to determine whether the entire chain computed correctly.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function HasCompletedRun() As Boolean
        Dim unit As cUnit = Nothing
        For iSequence As Integer = 0 To Me.UnitCount - 1
            unit = Me.Unit(iSequence)
            If unit.IsRunError Then
#If DEBUG Then
                Debug.Assert(False, "Chain did not compute correctly for unit " & unit.Name)
#End If
                Return False
            End If
        Next
        Return True
    End Function

#End Region ' Running

#Region " Parameters "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the parameters that dictate how this monster will run.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Parameters() As cParameters
        Get
            Return Me.m_parameters
        End Get
    End Property

    Public Sub AddParameters(ByVal parms As cParameters)

        ' Detach
        If Me.m_parameters IsNot Nothing Then Me.RemoveParameters(Me.m_parameters)
        ' Store
        Me.m_parameters = parms
        ' Start listening for generic change events
        AddHandler parms.OnChanged, AddressOf OnElementChanged

    End Sub

    Public Sub RemoveParameters(ByVal parms As cParameters)

        If Me.m_parameters Is Nothing Then Return
        Debug.Assert(ReferenceEquals(Me.m_parameters, parms))

        ' Stop listening for generic change events
        RemoveHandler parms.OnChanged, AddressOf OnElementChanged
        ' Release
        Me.m_parameters = Nothing

    End Sub

#End Region ' Parameters

#Region " Defaults "

    Public Function GetUnitDefault(ByVal unitType As cUnitFactory.eUnitType) As cUnit
        Dim unit As cUnit = Nothing
        ' Try to find
        For Each unit In Me.m_lUnitDefaults
            If unit.UnitType = unitType Then Return unit
        Next
        ' Not found: create it
        unit = cUnitFactory.CreateUnitDefault(unitType)
        AddUnitDefault(unit)
        Return unit
    End Function

    Public Sub AddUnitDefault(ByVal unit As cUnit)
        If unit IsNot Nothing Then
            Me.m_lUnitDefaults.Add(unit)
            ' Start listening for generic change events
            AddHandler unit.OnChanged, AddressOf OnElementChanged
        End If
    End Sub

    Public Sub RemoveUnitDefault(ByVal unit As cUnit)
        If unit IsNot Nothing Then
            Me.m_lUnitDefaults.Remove(unit)
            ' Start listening for generic change events
            RemoveHandler unit.OnChanged, AddressOf OnElementChanged
        End If
    End Sub

    Public Function GetLinkDefault(ByVal linkType As cLinkFactory.eLinkType) As cLinkDefault
        Dim link As cLinkDefault = Nothing
        ' Try to find
        For Each link In Me.m_lLinkDefaults
            If link.LinkType = linkType Then Return link
        Next
        ' Not found: create it
        link = cLinkFactory.CreateLinkDefault(linkType)
        Me.AddLinkDefault(link)
        Return link
    End Function

    Public Sub AddLinkDefault(ByVal link As cLinkDefault)
        If link IsNot Nothing Then
            Me.m_lLinkDefaults.Add(link)
            ' Start listening for generic change events
            AddHandler link.OnChanged, AddressOf OnElementChanged
        End If
    End Sub

    Public Sub RemoveLinkDefault(ByVal link As cLinkDefault)
        If link IsNot Nothing Then
            Me.m_lLinkDefaults.Remove(link)
            ' Stop listening for generic change events
            RemoveHandler link.OnChanged, AddressOf OnElementChanged
        End If
    End Sub

#End Region ' Defaults

#Region " Units "

    Public Function UnitCount() As Integer
        Return Me.m_lUnits.Count
    End Function

    Public Function UnitByID(ByVal iDBID As Integer) As cUnit
        For Each unit As cUnit In Me.m_lUnits
            If unit.DBID = iDBID Then Return unit
        Next
        Return Nothing
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a unit from the lit of units
    ''' </summary>
    ''' <param name="iIndex">Zero-based unit index.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function Unit(ByVal iIndex As Integer) As cUnit
        Return Me.m_lUnits(iIndex)
    End Function

    Public Function GetUnits(ByVal unitType As cUnitFactory.eUnitType) As cUnit()
        Dim lUnits As New List(Of cUnit)
        Dim unit As cUnit = Nothing
        Dim tUnit As Type = cUnitFactory.MapType(unitType)
        Dim bAdd As Boolean = False
        For i As Integer = 0 To Me.m_lUnits.Count - 1
            unit = Me.m_lUnits(i)
            ' Need to filter by unit type?
            If (tUnit IsNot Nothing) Then
                ' #Yes: check unit type
                bAdd = (tUnit.IsInstanceOfType(unit))
            Else
                ' #No: assume all is well
                bAdd = True
            End If
            ' Hide default units
            If (TypeOf unit Is cProducerUnitDefault) Or _
                (TypeOf unit Is cProcessingUnitDefault) Or _
                (TypeOf unit Is cDistributionUnitDefault) Or _
                (TypeOf unit Is cWholesalerUnitDefault) Or _
                (TypeOf unit Is cRetailerUnitDefault) Or _
                (TypeOf unit Is cConsumerUnitDefault) Then
                bAdd = False
            End If

            If bAdd Then
                lUnits.Add(unit)
            End If
        Next
        Return lUnits.ToArray
    End Function

    ''' <summary>
    ''' Create a unit in the database
    ''' </summary>
    ''' <param name="unitType"></param>
    ''' <param name="strName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateUnit(ByVal unitType As cUnitFactory.eUnitType, ByVal strName As String) As cUnit
        Dim unit As cUnit = cUnitFactory.CreateUnit(unitType)
        If unit IsNot Nothing Then
            ' Populate unit with defaults
            unit.CopyFrom(Me.GetUnitDefault(unitType))
            ' Set default name
            unit.Name = strName
            ' Add it to the local admin
            Me.AddUnit(unit)
        End If
        Return unit
    End Function

    ''' <summary>
    ''' Delete a unit from the database
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DeleteUnit(ByVal unit As cUnit) As Boolean

        Dim bSucces As Boolean = True

        Me.m_db.BeginTransaction()

        bSucces = Me.m_db.DeleteObject(unit)
        If bSucces Then

            ' Remove all incoming links from the database
            For iLink As Integer = 0 To unit.LinkInCount - 1
                Dim link As cLink = unit.LinkIn(iLink)
                bSucces = bSucces And Me.m_db.DeleteObject(link)
            Next

            ' Remove all outgoing links from the database
            For iLink As Integer = 0 To unit.LinkOutCount - 1
                Dim link As cLink = unit.LinkOut(iLink)
                bSucces = bSucces And Me.m_db.DeleteObject(link)
            Next

            ' Remove all related flow positions from the database
            For Each fp As cFlowPosition In Me.FlowPositions(unit)
                bSucces = bSucces And Me.m_db.DeleteObject(fp)
            Next

        End If

        ' All attached data succesfully deleted?
        If bSucces Then

            ' #Yes: commit database
            Me.m_db.CommitTransaction(True)

            ' Remove the unit from local admin
            Me.RemoveUnit(unit)

            Me.IsChanged = True
        Else
            ' #No: preserve data
            Me.m_db.RollbackTransaction()
        End If

        Return bSucces

    End Function

    ''' <summary>
    ''' Add a unit to the local administration
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <remarks></remarks>
    Public Sub AddUnit(ByVal unit As cUnit)

        If (unit Is Nothing) Then Return

        ' Assign core
        unit.Core = Me.Core

        ' Add
        Me.m_lUnits.Add(unit)
        Me.IsChanged = True

        ' Perform the "production unit special"
        If TypeOf unit Is cProducerUnit Then
            ' Prepare production unit
            Me.OnChanged(unit)
            ' Start listening for further metier events
            AddHandler unit.OnChanged, AddressOf OnChanged
        End If

        ' Start listening for generic change events
        AddHandler unit.OnChanged, AddressOf OnElementChanged

    End Sub

    ''' <summary>
    ''' Remove a unit from the local administration
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <remarks></remarks>
    Public Sub RemoveUnit(ByVal unit As cUnit)

        If unit Is Nothing Then Return

        ' Stop listening for generic change events
        RemoveHandler unit.OnChanged, AddressOf OnElementChanged

        Try
            Me.m_lUnits.Remove(unit)

            ' Perform the "Metier special"
            If TypeOf unit Is cProducerUnit Then
                ' Stop listening for further metier events
                RemoveHandler unit.OnChanged, AddressOf OnChanged
            End If

        Catch ex As Exception
            Debug.Assert(False)
        End Try

        ' Remove all links from this unit
        While unit.LinkInCount > 0
            Me.RemoveLink(unit.LinkIn(0))
        End While

        ' Remove all links to this unit
        While unit.LinkOutCount > 0
            Me.RemoveLink(unit.LinkOut(0))
        End While

        ' Remove all flow positions pertaining to this unit
        For Each fp As cFlowPosition In Me.FlowPositions(unit)
            Me.RemoveFlowPosition(fp)
        Next

        Me.m_lUnits.Remove(unit)

    End Sub

#Region " Metier management "

    ''' <summary>
    ''' Helper: connect producer unit fleet from DBID
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub OnChanged(ByVal obj As cEwEDatabase.cOOPStorable)
        If TypeOf obj Is cProducerUnit Then
            Dim prod As cProducerUnit = DirectCast(obj, cProducerUnit)
            prod.Fleet = Me.FindEcopathFleetByID(prod.EcopathFleetID)
            Me.IsChanged = True
        End If
        If TypeOf obj Is cLinkLandings Then
            Dim link As cLinkLandings = DirectCast(obj, cLinkLandings)
            link.Group = Me.FindEcopathGroupByID(link.EcopathGroupID)
            Me.IsChanged = True
        End If
    End Sub

    Private Function FindEcopathGroupByID(ByVal iDBID As Integer) As cEcoPathGroupInput
        Dim group As cEcoPathGroupInput = Nothing
        For i As Integer = 1 To Me.m_core.nGroups
            group = Me.m_core.EcoPathGroupInputs(i)
            If CInt(group.GetVariable(eVarNameFlags.DBID)) = iDBID Then Return group
        Next
        Return Nothing
    End Function

    Private Function FindEcopathFleetByID(ByVal iDBID As Integer) As cEcopathFleetInput
        Dim fleet As cEcopathFleetInput = Nothing
        For i As Integer = 1 To Me.m_core.nFleets
            fleet = Me.m_core.EcopathFleetInputs(i)
            If CInt(fleet.GetVariable(eVarNameFlags.DBID)) = iDBID Then Return fleet
        Next
        Return Nothing
    End Function

#End Region ' Helpers

#End Region ' Units

#Region " Links "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all visible links of a given type.
    ''' </summary>
    ''' <param name="t"></param>
    ''' <param name="bIncludeInvisible">Flag stating that also links that are not
    ''' <see cref="cLink.IsVisible">visible</see> may be included.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function GetLinks(ByVal t As Type, Optional bIncludeInvisible As Boolean = False) As cLink()

        Dim lLinks As New List(Of cLink)
        Dim link As cLink = Nothing
        Dim bAdd As Boolean = False
        For i As Integer = 0 To Me.m_lLinks.Count - 1
            link = Me.m_lLinks(i)
            ' Need to filter by unit type?
            If (t IsNot Nothing) Then
                ' #Yes: check link type
                bAdd = (t.Equals(link.GetType)) And (link.IsVisible Or bIncludeInvisible)
            Else
                ' #No: assume all is well
                bAdd = True
            End If

            If bAdd Then
                lLinks.Add(link)
            End If
        Next
        Return lLinks.ToArray

    End Function

    Public Function LinkCount() As Integer
        Return Me.m_lLinks.Count
    End Function

    Public Function LinkByID(ByVal iDBID As Integer) As cLink
        For Each link As cLink In Me.m_lLinks
            If link.DBID = iDBID Then Return link
        Next
        Return Nothing
    End Function

    Public Function Link(ByVal iIndex As Integer) As cLink
        Return Me.m_lLinks(iIndex)
    End Function

    Public Function CreateLandingsLink(ByVal unitSource As cProducerUnit, ByVal unitTarget As cUnit, ByVal group As cEcoPathGroupInput, ByRef bError As Boolean) As cLinkLandings

        ' Sanity check
        If (unitSource Is Nothing) Or (unitTarget Is Nothing) Then
            Me.SendMessage(My.Resources.ERROR_LINK_NEEDUNITS)
            bError = True
            Return Nothing
        End If

        ' Check if link is allowed
        If Not cLinkFactory.CanCreateLink(unitSource, unitTarget) Then
            Me.SendMessage(My.Resources.ERROR_LINK_NOTALLOWED)
            bError = True
            Return Nothing
        End If

        ' Check for loop
        If unitTarget.IsLoop(unitSource) Then
            Me.SendMessage(My.Resources.ERROR_LINK_LOOP)
            bError = True
            Return Nothing
        End If

        ' Check for already present link
        If unitSource.HasTarget(unitTarget, group) Then
            Dim fmt As New cCoreInterfaceFormatter()
            Me.SendMessage(cStringUtils.Localize(My.Resources.ERROR_LINK_DUPLICATE, fmt.GetDescriptor(group)))
            bError = True
            Return Nothing
        End If

        If (unitSource.Fleet.Landings(group.Index) = 0) Then Return Nothing

        Dim link As New cLinkLandings()

        ' Provide link with defaults
        link.CopyFrom(Me.GetLinkDefault(cLinkFactory.GetLinkType(unitSource, unitTarget)))

        link.Source = unitSource
        link.Target = unitTarget
        link.Group = group
        Me.IsChanged = True

        Me.AddLink(link)

        Return link
    End Function

    ''' <summary>
    ''' Create a link in the database 
    ''' </summary>
    ''' <param name="unitSource"></param>
    ''' <param name="unitTarget"></param>
    ''' <returns></returns>
    Public Function CreateLink(ByVal unitSource As cUnit, ByVal unitTarget As cUnit) As cLink

        ' Sanity check
        If (unitSource Is Nothing) Or (unitTarget Is Nothing) Then
            Me.SendMessage(My.Resources.ERROR_LINK_NEEDUNITS)
            Return Nothing
        End If

        ' Check if link is allowed
        If Not cLinkFactory.CanCreateLink(unitSource, unitTarget) Then
            Me.SendMessage(My.Resources.ERROR_LINK_NOTALLOWED)
            Return Nothing
        End If

        ' Check for loop
        If unitTarget.IsLoop(unitSource) Then
            Me.SendMessage(My.Resources.ERROR_LINK_LOOP)
            Return Nothing
        End If

        Dim link As New cLink()

        ' Provide link with defaults
        link.CopyFrom(Me.GetLinkDefault(cLinkFactory.GetLinkType(unitSource, unitTarget)))

        link.Source = unitSource
        link.Target = unitTarget
        Me.IsChanged = True

        Me.AddLink(link)

        Return link

    End Function

    ''' <summary>
    ''' Remove a link from the database 
    ''' </summary>
    ''' <param name="link"></param>
    ''' <returns></returns>
    Public Function DeleteLink(ByVal link As cLink) As Boolean
        If Me.m_db.DeleteObject(link) Then
            Me.RemoveLink(link)
            Me.IsChanged = True
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Add an output link to the local administration
    ''' </summary>
    ''' <param name="link"></param>
    Public Sub AddLink(ByVal link As cLink)

        ' Sanity check
        Debug.Assert(link IsNot Nothing)
        Debug.Assert(link.Target IsNot Nothing)
        Debug.Assert(link.Source IsNot Nothing)

        Me.m_lLinks.Add(link)
        link.Source.AddLink(link)

        ' Perform the "Species link special"
        If TypeOf link Is cLinkLandings Then
            ' Prepare link
            Me.OnChanged(link)
            ' Start listening for further link events
            AddHandler link.OnChanged, AddressOf OnChanged
        End If

        ' Start listening for link change events
        AddHandler link.OnChanged, AddressOf Me.OnElementChanged

    End Sub

    ''' <summary>
    ''' Remove an output link from the local administration
    ''' </summary>
    ''' <param name="link"></param>
    Public Sub RemoveLink(ByVal link As cLink)

        ' Sanity check
        Debug.Assert(link IsNot Nothing)

        ' Stop listening for link change events
        RemoveHandler link.OnChanged, AddressOf Me.OnElementChanged

        ' Perform the "Species link special"
        If TypeOf link Is cLinkLandings Then
            ' Start listening for further link events
            RemoveHandler link.OnChanged, AddressOf OnChanged
        End If

        Me.m_lLinks.Remove(link)
        link.Source.RemoveLink(link)
    End Sub

    Public Sub OnGroupLinkChanged(ByVal unit As cEwEDatabase.cOOPStorable)
        If TypeOf unit Is cLinkLandings Then
            Dim mu As cLinkLandings = DirectCast(unit, cLinkLandings)
            mu.Group = Me.FindEcopathGroupByID(mu.EcopathGroupID)
            Me.IsChanged = True
        End If
    End Sub

#End Region ' Links

#Region " Flow diagrams "

    Public Function FlowDiagramCount() As Integer
        Return Me.m_lFlowDiagrams.Count
    End Function

    Public Function FlowDiagram(ByVal iIndex As Integer) As cFlowDiagram
        If Me.m_lFlowDiagrams.Count = 0 Then
            Me.CreateFlowDiagram(New cFlowDiagram())
        End If
        Return Me.m_lFlowDiagrams(iIndex)
    End Function

    Public Sub CreateFlowDiagram(ByVal diagram As cFlowDiagram)
        Me.AddFlowDiagram(diagram)
        Me.IsChanged = True
    End Sub

    Public Sub DeleteFlowDiagram(ByVal diagram As cFlowDiagram)
        Me.AddFlowDiagram(diagram)
        Me.IsChanged = True
    End Sub

    Public Sub AddFlowDiagram(ByVal diagram As cFlowDiagram)
        Me.m_lFlowDiagrams.Add(diagram)
    End Sub

    Public Sub RemoveFlowDiagram(ByVal diagram As cFlowDiagram)
        Me.m_lFlowDiagrams.Remove(diagram)
    End Sub

#End Region ' Flow diagrams

#Region " Flow positions "

    Public Function FlowPositionCount() As Integer
        Return Me.m_lFlowPositions.Count
    End Function

    Public Function FlowPosition(ByVal iIndex As Integer) As cFlowPosition
        If Me.m_lFlowDiagrams.Count = 0 Then
            Me.CreateFlowDiagram(New cFlowDiagram())
        End If
        Return Me.m_lFlowPositions(iIndex)
    End Function

    Public Function CreateFlowPosition(ByVal unit As cUnit, ByVal diagram As cFlowDiagram) As cFlowPosition

        ' Sanity checks
        Debug.Assert(unit IsNot Nothing)
        Debug.Assert(diagram IsNot Nothing)

        Dim fp As New cFlowPosition()
        fp.Unit = unit
        fp.Diagram = diagram

        Me.AddFlowPosition(fp)
        Me.IsChanged = True

        Return fp

    End Function

    Public Sub AddFlowPosition(ByVal pos As cFlowPosition)

        ' Sanity check
        Debug.Assert(pos IsNot Nothing)
        Debug.Assert(pos.Unit IsNot Nothing)

        ' Start listening for generic change events
        AddHandler pos.OnChanged, AddressOf OnElementChanged

        Me.m_lFlowPositions.Add(pos)
    End Sub

    Public Sub DeleteFlowPosition(ByVal pos As cFlowPosition)

        ' Sanity check
        Debug.Assert(pos IsNot Nothing)

        If Me.m_db.DeleteObject(pos) Then
            Me.RemoveFlowPosition(pos)
            Me.IsChanged = True
        End If
    End Sub

    Public Sub RemoveFlowPosition(ByVal pos As cFlowPosition)

        ' Sanity check
        Debug.Assert(pos IsNot Nothing)

        ' Stop listening for generic change events
        RemoveHandler pos.OnChanged, AddressOf OnElementChanged

        Me.m_lFlowPositions.Remove(pos)
    End Sub

    Public Function FlowPositions(ByVal unit As cUnit) As cFlowPosition()
        Dim lfp As New List(Of cFlowPosition)
        Dim fp As cFlowPosition = Nothing
        For i As Integer = 0 To Me.m_lFlowPositions.Count - 1
            fp = Me.m_lFlowPositions(i)
            ' Compare object references since DBIDs can be 0 (for unsaved objects)
            If (ReferenceEquals(fp.Unit, unit)) Then lfp.Add(fp)
        Next
        Return lfp.ToArray()
    End Function

    Public Function FlowPositions(ByVal diagram As cFlowDiagram) As cFlowPosition()
        Dim lfp As New List(Of cFlowPosition)
        Dim fp As cFlowPosition = Nothing
        For i As Integer = 0 To Me.m_lFlowPositions.Count - 1
            fp = Me.m_lFlowPositions(i)
            ' Compare object references since DBIDs can be 0 (for unsaved objects)
            If (ReferenceEquals(fp.Diagram, diagram)) Then lfp.Add(fp)
        Next
        Return lfp.ToArray()
    End Function

    Public Function FlowPosition(ByVal unit As cUnit, ByVal diagram As cFlowDiagram) As cFlowPosition
        Dim fp As cFlowPosition = Nothing
        For i As Integer = 0 To Me.m_lFlowPositions.Count - 1
            fp = Me.m_lFlowPositions(i)
            ' Compare object references since DBIDs can be 0 (for unsaved objects)
            If (ReferenceEquals(fp.Diagram, diagram) And ReferenceEquals(fp.Unit, unit)) Then
                Return fp
            End If
        Next
        Return Nothing
    End Function

#End Region ' Flow positions

#Region " Core access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the EwE core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Core() As cCore
        Get
            Return Me.m_core
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all units in the flow that are linked to this unit,
    ''' either serving as source units or as target units.
    ''' </summary>
    ''' <param name="unit"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function GetConnectedUnits(ByVal unit As cUnit) As List(Of cUnit)

        Dim lUnits As New List(Of cUnit)

        ' Sanity check
        Debug.Assert(unit IsNot Nothing)

        Me.GetSourceUnits(unit, lUnits)
        Me.GetTargetUnits(unit, lUnits)

        ' Sanity check
        Debug.Assert(lUnits.IndexOf(unit) = -1)

        ' Add m'self
        lUnits.Add(unit)
        ' Done
        Return lUnits

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return all units that operate (directly or indirectly) onto a given 
    ''' fleet and/or group.
    ''' </summary>
    ''' <param name="item"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function GetUnits(ByVal item As cCoreInputOutputBase) As cUnit()

        Dim lUnits As New List(Of cUnit)
        Dim pu As cProducerUnit = Nothing
        Dim bUseUnit As Boolean = False

        If (item Is Nothing) Then Return Me.GetUnits(cUnitFactory.eUnitType.All)

        ' * Determine for all producers whether the fleet and group filter matches
        ' * For all matching producers add all related units in the flow

        ' Iterate over producers
        For Each unit As cUnit In Me.GetUnits(cUnitFactory.eUnitType.Producer)
            ' Get producer unit
            pu = DirectCast(unit, cProducerUnit)

            ' Filtering by fleet?
            If (TypeOf item Is cEcopathFleetInput) Then
                ' #Yes: include producer if it uses this fleet
                bUseUnit = ReferenceEquals(pu.Fleet, item)
            Else
                ' #Yes: include producer if its fleet lands or discards a group
                bUseUnit = (pu.Fleet.Landings(item.Index) > 0) Or (pu.Fleet.Discards(item.Index) > 0)
            End If

            If bUseUnit Then
                ' Add producer
                lUnits.Add(pu)
                ' Get all flow units linked to this producer
                Me.GetTargetUnits(pu, lUnits)
            End If

        Next unit

        Return lUnits.ToArray()

    End Function

#End Region ' Core access

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all units that serve as source units to a given unit.
    ''' </summary>
    ''' <param name="unit">The unit to test incoming links for.</param>
    ''' <param name="lUnits">The list that will receive the linked units.</param>
    ''' -----------------------------------------------------------------------
    Private Sub GetSourceUnits(ByVal unit As cUnit, ByVal lUnits As List(Of cUnit))
        Dim unitSource As cUnit = Nothing
        For iLink As Integer = 0 To unit.LinkInCount - 1
            unitSource = unit.LinkIn(iLink).Source
            If lUnits.IndexOf(unitSource) = -1 Then
                lUnits.Add(unitSource)
                Me.GetSourceUnits(unitSource, lUnits)
            End If
        Next
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all units that link out of a given unit
    ''' </summary>
    ''' <param name="unit">The unit to test outgoing links for.</param>
    ''' <param name="lUnits">The list that will receive the linked units.</param>
    ''' -----------------------------------------------------------------------
    Private Sub GetTargetUnits(ByVal unit As cUnit, ByVal lUnits As List(Of cUnit))
        Dim unitTarget As cUnit = Nothing
        For iLink As Integer = 0 To unit.LinkOutCount - 1
            unitTarget = unit.LinkOut(iLink).Target
            If lUnits.IndexOf(unitTarget) = -1 Then
                lUnits.Add(unitTarget)
                Me.GetTargetUnits(unitTarget, lUnits)
            End If
        Next
    End Sub

    Public Sub SendMessage(strMessage As String, _
                            Optional msgtype As eMessageType = eMessageType.Any, _
                            Optional corecomp As eCoreComponentType = eCoreComponentType.External, _
                            Optional importance As eMessageImportance = eMessageImportance.Warning)
        Dim msg As New cMessage(strMessage, msgtype, corecomp, importance)
        If (Me.m_core IsNot Nothing) Then
            Try
                Me.m_core.Messages.SendMessage(msg)
            Catch ex As Exception
                ' Whoah
            End Try
        End If
    End Sub

#End Region ' Internals

End Class
