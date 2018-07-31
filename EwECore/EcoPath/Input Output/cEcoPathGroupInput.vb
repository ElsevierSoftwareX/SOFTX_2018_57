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
Imports EwEUtils.Core
Imports EwECore.ValueWrapper
Imports EwECore.Style

''' <summary>
''' Inputs for EcoPath for a single group.
''' </summary>
''' <remarks>
''' This class wraps the inputs to EcoPath for one group into a single object.
''' </remarks>
Public Class cEcoPathGroupInput
    Inherits cCoreGroupBase

#Region " Private stuff "

    ''' <summary>Core Counter interface for group taxon</summary>
    Private m_CoreCounter As CoreIndexedCounterDelegate

    ''' <summary>
    ''' Clear the Status/message (CurrentStatus) object for this group 
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ClearCurrentStatus()

        m_ValidationStatus.Status = eStatusFlags.OK
        m_ValidationStatus.Source = eCoreComponentType.EcoPath
        m_ValidationStatus.Message = ""
        m_ValidationStatus.VarName = eVarNameFlags.NotSet
        m_ValidationStatus.Index = Index
        m_ValidationStatus.DataType = eDataTypes.EcoPathGroupInput
        m_ValidationStatus.CoreDataObject = Me

    End Sub

#End Region ' stuff

#Region " Constructor and Initialization "

    Sub New(ByVal core As cCore, ByVal DBID As Integer, ByVal iIndex As Integer)
        MyBase.New(core)

        Dim val As cValue = Nothing

        'get the core counter interface for the NTaxon (number of taxa) counter
        Me.m_CoreCounter = AddressOf m_core.GetCoreCounter

        Me.m_dataType = eDataTypes.EcoPathGroupInput
        Me.m_coreComponent = eCoreComponentType.EcoPath
        Me.Index = iIndex

        Me.AllowValidation = False

        Me.DBID = DBID

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'NULL VALUES values cleared by a user
        'values that are cleared by a user and then computed by Ecopath need to be set to <0 as there null value (this tells ecopath to compute the value)
        'see EwE5 frmInputData.vaInput_Change() for which values use this mechanism
        'this is handled here by the meta data object and the validator
        'the Meta data tells the validator what the min and max allowable values are
        'the validator decides what to do if a value is < min, set the value to the meta data nullValue or reject the value

        ClearCurrentStatus()

        ' HabitatArea
        val = New cValue(New Single, eVarNameFlags.HabitatArea, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' BioAccumInput
        val = New cValue(New Single, eVarNameFlags.BioAccumInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' Biomass
        val = New cValue(New Single, eVarNameFlags.Biomass, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' BiomassAreaInput
        val = New cValue(New Single, eVarNameFlags.BiomassAreaInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' DetImp
        val = New cValue(New Single, eVarNameFlags.DetImp, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' EEInput
        val = New cValue(New Single, eVarNameFlags.EEInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' OtherMortInput
        val = New cValue(New Single, eVarNameFlags.OtherMortInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' Emig
        val = New cValue(New Single, eVarNameFlags.Emig, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' EmigRate
        val = New cValue(New Single, eVarNameFlags.EmigRate, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' GEInput
        val = New cValue(New Single, eVarNameFlags.GEInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' GS
        val = New cValue(New Single, eVarNameFlags.GS, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' PBInput
        val = New cValue(New Single, eVarNameFlags.PBInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' Immig
        val = New cValue(New Single, eVarNameFlags.Immig, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' QBInput
        val = New cValue(New Single, eVarNameFlags.QBInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' BioAccumRate
        val = New cValue(New Single, eVarNameFlags.BioAccumRate, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' ImpDiet
        val = New cValue(New Single, eVarNameFlags.ImpDiet, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' PoolColor
        val = New cValue(New Integer, eVarNameFlags.PoolColor, eStatusFlags.Null, eValueTypes.Int)
        val.AffectsRunState = False
        m_values.Add(val.varName, val)
        ' NonMarketValue
        val = New cValue(New Single, eVarNameFlags.NonMarketValue, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' IsFished
        val = New cValue(New Boolean, eVarNameFlags.IsFished, eStatusFlags.OK, eValueTypes.Bool)
        val.AffectsRunState = False
        val.Stored = False
        m_values.Add(val.varName, val)

        ' -- arrays --

        ' DietComp
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.DietComp, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        ' DetritusFate
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.DetritusFate, eStatusFlags.Null, eCoreCounterTypes.nDetritus, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)
        ' VBK
        val = New cValue(New Single, eVarNameFlags.VBK, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' TCatchInput
        val = New cValue(New Single, eVarNameFlags.TCatchInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' AinLWInput
        val = New cValue(New Single, eVarNameFlags.AinLWInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' BinLWInput
        val = New cValue(New Single, eVarNameFlags.BinLWInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' LooInput
        val = New cValue(New Single, eVarNameFlags.LooInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' WinfInput
        val = New cValue(New Single, eVarNameFlags.WinfInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' t0Input
        val = New cValue(New Single, eVarNameFlags.t0Input, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' TmaxInput
        val = New cValue(New Single, eVarNameFlags.TmaxInput, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)
        ' GroupTaxa - dimensioned by nTaxa(iIndex)

        val = New cValueArray(eValueTypes.BoolArray, eVarNameFlags.IsPred, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        val.AffectsRunState = False
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.BoolArray, eVarNameFlags.IsPrey, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        val.AffectsRunState = False
        m_values.Add(val.varName, val)

        ' iTaxon dimensioned by nTaxa(iIndex)
        val = New cValueArrayIndexed(eValueTypes.IntArray, eVarNameFlags.GroupTaxa, eStatusFlags.Null, eCoreCounterTypes.nTaxonForGroup, AddressOf m_core.GetCoreCounter, Me.Index, Me.DataType)
        m_values.Add(val.varName, val)

        Me.AllowValidation = True

    End Sub

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        MyBase.ResetStatusFlags(bForceReset)

        Me.m_core.Set_PB_QB_GE_BA_Flags(Me, False)
        Me.m_core.Set_Migration_Flags(Me, False)
        Me.m_core.Set_GS_Flags(Me, False)
        Me.m_core.Set_EE_OtherMort_Flags(Me, False)
        Me.m_core.Set_DetImp_Flags(Me, False)

        Me.m_core.Set_VBK_Flags(Me, False)
        Me.m_core.Set_Tcatch_Flags(Me, False)
        Me.m_core.Set_Tmax_Flags(Me, False)

    End Function

#End Region ' Constructor and Initialization

#Region "Variables by dot (.) operator"

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.BA">biomass accumulation</see>
    ''' value for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BioAccumInput() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.BioAccumInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.BioAccumInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.PBinput">production per biomass</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property PBInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.PBInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.PBInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.QBinput">consuption per biomass</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property QBInput() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.QBInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.QBInput, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.GEinput">production per consuption</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property GEInput() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.GEInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.GEInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.GS">unassimilation per consumption</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property GS() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.GS))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.GS, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.DtImp">detritus import</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DetImport() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.DetImp))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.DetImp, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.Area">Area</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Area() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.HabitatArea))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.HabitatArea, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.BH">Biomass per Area</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BiomassAreaInput() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.BiomassAreaInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.BiomassAreaInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.EEinput">Ecotrophic efficiency</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EEInput() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EEInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EEInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the other mortality ratio for this group, defined as 1 - <see cref="EEInput"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property OtherMortInput() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.OtherMortInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.OtherMortInput, value)
        End Set

    End Property

    Public Property ImpDiet() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.ImpDiet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.ImpDiet, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.DC">Diet composition</see> ratio
    ''' for a particular prey for this group.
    ''' </summary>
    ''' <param name="iPreyGroup">The <see cref="Index">index</see> of the prey (or group)
    ''' that makes up a percentage of this predators diet.</param>
    ''' <remarks>
    ''' <para>How to use:</para>
    ''' <para>Set the diet composition of group 1 to 50% of its diet from group 4</para>
    ''' <code>
    ''' Dim core As cCore = cCore.GetInstance()
    ''' Dim group As cEcoPathGroupInput = Nothing
    ''' 
    ''' ' Get the group
    ''' group = core.EcoPathGroupInputs(1)
    ''' ' Set the diet comp for group 4 to 50%
    ''' EcoPathGroup.DietComp(4) = .5
    ''' ' or
    ''' core.EcoPathGroupInputs(1).DietComp(4) = .5
    ''' </code>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property DietComp(ByVal iPreyGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.DietComp, iPreyGroup))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.DietComp, value, iPreyGroup)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.DC">Diet composition</see>
    ''' ratio array for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DietComp() As Single()

        Get
            Return DirectCast(GetVariable(eVarNameFlags.DietComp), Single())
        End Get

        Set(ByVal value As Single())
            SetVariable(eVarNameFlags.DietComp, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.DF">Detritus fate</see> ratio
    ''' for a particular prey for this group.
    ''' </summary>
    ''' <param name="iDetritusGroup"></param>
    ''' -----------------------------------------------------------------------
    Public Property DetritusFate(ByVal iDetritusGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.DetritusFate, iDetritusGroup))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.DetritusFate, value, iDetritusGroup)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.DF">Detritus fate</see> ratio
    ''' array for a particular prey for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DetritusFate() As Single()

        Get
            Return DirectCast(GetVariable(eVarNameFlags.DetritusFate), Single())
        End Get

        Set(ByVal value As Single())
            SetVariable(eVarNameFlags.DetritusFate, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.Emig">emigration rate relative to biomass</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EmigRate() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EmigRate))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EmigRate, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.Babi">Biomass accumulation per biomass</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BioAccumRate() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.BioAccumRate))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.BioAccumRate, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.Immig">immigration</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Immigration() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Immig))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Immig, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcopathDataStructures.Emigration">emigration</see>
    ''' ratio for this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Emigration() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Emig))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Emig, value)
        End Set

    End Property

    Public Property VBK() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.VBK))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.VBK, value)
        End Set
    End Property

    Public Property PoolColor() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.PoolColor))
        End Get
        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.PoolColor, value)
        End Set
    End Property

    Public Property NonMarketValue() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.NonMarketValue))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.NonMarketValue, value)
        End Set

    End Property

    Public Property TcatchInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TCatchInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.TCatchInput, value)
        End Set
    End Property

    Public Property AinLWInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.AinLWInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.AinLWInput, value)
        End Set
    End Property

    Public Property BinLWInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.BinLWInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.BinLWInput, value)
        End Set
    End Property

    Public Property LooInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.LooInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.LooInput, value)
        End Set
    End Property

    Public Property WinfInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.WinfInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.WinfInput, value)
        End Set
    End Property

    Public Property t0Input() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.t0Input))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.t0Input, value)
        End Set
    End Property

    Public Property TmaxInput() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TmaxInput))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.TmaxInput, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether a group is being fished. This value is kept up to date by 
    ''' the core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property IsFished() As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.IsFished))
        End Get

        Friend Set(ByVal value As Boolean)
            Me.SetVariable(eVarNameFlags.IsFished, value)
        End Set
    End Property

    ''' <summary>
    ''' Get whether this group is predated on by <paramref name="iGroup">group index</paramref>.
    ''' </summary>
    ''' <param name="iGroup">Group index of the predator</param>
    Public Property IsPred(ByVal iGroup As Integer) As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.IsPred, iGroup))
        End Get

        Friend Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.IsPred, value, iGroup)
        End Set

    End Property

    ''' <summary>
    ''' Get whether this group predates on <paramref name="iGroup">group index</paramref>.
    ''' </summary>
    ''' <param name="iGroup">Group index of the prey</param>
    Public Property IsPrey(ByVal iGroup As Integer) As Boolean

        Get
            Return CBool(GetVariable(eVarNameFlags.IsPrey, iGroup))
        End Get

        Friend Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.IsPrey, value, iGroup)
        End Set

    End Property

#End Region

#Region "Status by dot (.) operator"

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="DietComp">DietComp value</see> of this group.
    ''' </summary>
    ''' <param name="iGroup">Prey <see cref="Index">index</see>.</param>
    ''' -----------------------------------------------------------------------
    Public Property DietCompStatus(ByVal iGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.DietComp, iGroup)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.DietComp, value, iGroup)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="DetritusFate">DestritusFate value</see> of this
    ''' group.
    ''' </summary>
    ''' <param name="iDetritusGroup">Detritus group <see cref="Index">index</see>.</param>
    ''' -----------------------------------------------------------------------
    Public Property DetritusFateStatus(ByVal iDetritusGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.DetritusFate, Index)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.DetritusFate, value, Index)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="Area">Area value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property AreaStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.HabitatArea)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.HabitatArea, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="BiomassAreaInput">BiomassArea input</see> value of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BiomassAreaStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.BiomassAreaInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BiomassAreaInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="PBInput">PBInput value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property PBStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.PBInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.PBInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="QBInput">QBInput value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property QBInputStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.QBInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.QBInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="EEInput">EEInput value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EEInputStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.EEInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EEInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="OtherMortInput">OtherMortInput value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property OtherMortInputStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.OtherMortInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.OtherMortInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="GEInput">GEInput value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property GEStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.GEInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.GEInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="GS">GS value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property GSStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.GS)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.GS, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="DetImport">DetImport value</see> this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DetImportStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.DetImp)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.DetImp, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="BioAccumInput">BioAccum input value</see> this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BioAccumStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.BioAccumInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BioAccumInput, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="EmigRate">EmigRate value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EmigRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.EmigRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EmigRate, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="Emigration">Emigration value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property EmigrationStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Emig)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Emig, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="Immigration">Immigration value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ImmigrationStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Immig)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Immig, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="BioAccumRate">BioAccumRate value</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BioAccumRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.BioAccumRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BioAccumRate, value)
        End Set

    End Property

    Public Property VBKStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.VBK)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.VBK, value)
        End Set
    End Property

    'Joeh
    Public Property AinLWInputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.AinLWInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.AinLWInput, value)
        End Set
    End Property

    Public Property BinLWInputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.BinLWInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.BinLWInput, value)
        End Set
    End Property

    Public Property LooInputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.LooInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.LooInput, value)
        End Set
    End Property

    Public Property WinfInputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.WinfInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.WinfInput, value)
        End Set
    End Property

    Public Property t0InputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.t0Input)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.t0Input, value)
        End Set
    End Property

    Public Property TcatchInputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.TCatchInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.TCatchInput, value)
        End Set
    End Property

    Public Property TmaxInputStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.TmaxInput)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.TmaxInput, value)
        End Set
    End Property
    'End Joeh
    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="ImpDiet">imported diet</see> of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ImpDietStatus() As eStatusFlags

        Get
            Return DietCompStatus(0)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            DietCompStatus(0) = value
        End Set

    End Property

    Public Property NonMarketValueStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.NonMarketValue)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NonMarketValue, value)
        End Set

    End Property

#If 0 Then

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eStatusFlags">status</see> of the 
    ''' <see cref="cEcopathDataStructures.B">biomass value</see> 
    ''' of this group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BiomassStatus() As eStatusFlags

        Get
            Return getStatus(eVarNameFlags.Biomass)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            setStatus(eVarNameFlags.Biomass, value)
        End Set

    End Property

#End If ' #0

#End Region

#Region " Taxa "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the number of taxa assigned to this group = either directly
    ''' via <see cref="cTaxon.Group"/>, or indirectly via <see cref="cTaxon.Stanza"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NTaxon() As Integer
        Get
            Return m_CoreCounter(eCoreCounterTypes.nTaxonForGroup, Me.Index)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="cCoreInputOutputBase.Index">Index</see> of a taxon
    ''' for this group. Taxa are stored in a one-based array.
    ''' </summary>
    ''' <param name="iIndex">The one-based index to obtain the taxon index for.</param>
    ''' <returns>Index of a taxon that is assigned to this group - either directly
    ''' via <see cref="cTaxon.Group"/>, or indirectly via <see cref="cTaxon.Stanza"/>.</returns>
    ''' <remarks>
    ''' <para>The returned index identifies the index of a particular taxon assigned
    ''' to this group.</para>
    ''' <code>
    ''' Dim grp As cEcoPathGroupInputs = Nothing
    ''' Dim taxon As cTaxon = Nothing
    ''' 
    ''' ' Get the first group
    ''' grp = core.EcopathGroupInputs(1)
    ''' 
    ''' ' Iterate over the taxa that are assigned to this group
    ''' For iIndex As Integer = 1 To grp.NTaxon
    '''    taxon = core.Taxon(grp.iTaxon(iIndex))
    '''    ' Do something with the taxon
    '''    ' ..
    '''    ' ..
    ''' Next iIndex
    ''' </code>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property iTaxon(ByVal iIndex As Integer) As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.GroupTaxa, iIndex))
        End Get
        Friend Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.GroupTaxa, value, iIndex)
        End Set
    End Property

#End Region

End Class
