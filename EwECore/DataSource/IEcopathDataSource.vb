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
Imports EwEPlugin.Data
Imports EwEUtils.Core

#End Region ' Imports

Namespace DataSources

    ''' =======================================================================
    ''' <summary>
    ''' Base interface for implementing a datasource that reads and writes 
    ''' Ecospace data.
    ''' </summary>
    ''' =======================================================================
    Public Interface IEcopathDataSource
        Inherits IEwEDataSource

#Region " Generic "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Copies all current Ecopath data to a target datasource.
        ''' </summary>
        ''' <param name="ds">The datasource to copy data to.</param>
        ''' <returns>True if sucessful.</returns>
        ''' -------------------------------------------------------------------
        Overloads Function CopyTo(ByVal ds As IEcopathDataSource) As Boolean

#End Region ' Generic

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the datasource has unsaved changes for Ecopath.
        ''' </summary>
        ''' <returns>True if the datasource has pending changes for Ecopath.</returns>
        ''' -------------------------------------------------------------------
        Function IsEcopathModified() As Boolean

#End Region ' Diagnostics

#Region " EwEModel "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initiates a full load of an EwE model.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function LoadModel() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initiates a save of an EwE model
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function SaveModel() As Boolean

#End Region ' EwEModel

#Region " Groups "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a record for a new Ecopath group in the datasource.
        ''' </summary>
        ''' <param name="strGroupName">The name of the group to create.</param>
        ''' <param name="sPP">The type of the new group; 0=consumer, 1=producer, 2=detritus, or a cons/prod ratio.</param>
        ''' <param name="sVBK">vbK value to initialize the group with.</param>
        ''' <param name="iPosition">The position of the new group in the group sequence.</param>
        ''' <param name="iDBID">Database ID assigned to the new Group.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will not adjust the data arrays. Due to the complex organization of the
        ''' core a full data reload is required after a group is created.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Function AddGroup(ByVal strGroupName As String, ByVal sPP As Single, ByVal sVBK As Single, _
                          ByVal iPosition As Integer, ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a group from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the group to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will not adjust the data arrays. Due to the complex organization of the
        ''' core a full data reload is required after a group is removed.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Function RemoveGroup(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an Ecopath group to a different position in the group sequence.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the group to move.</param>
        ''' <param name="iPosition">The new position of the group in the group sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function MoveGroup(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean

#End Region ' Groups

#Region " Fleets "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a fleet to the datasource.
        ''' </summary>
        ''' <param name="strFleetName">Name of the new fleet.</param>
        ''' <param name="iPosition">Position of the new fleet in the fleet sequence.</param>
        ''' <param name="iDBID">Database ID assigned to the new fleet.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddFleet(ByVal strFleetName As String, ByVal iPosition As Integer, ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a fleet from the data source.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the fleet to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveFleet(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an Ecopath fleet to a different position in the fleet sequence.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the fleet to move.</param>
        ''' <param name="iPosition">The new position of the fleet in the fleet sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function MoveFleet(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean

#End Region ' Fleets

#Region " Stanza "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a stanza group to the datasource.
        ''' </summary>
        ''' <param name="strStanzaName">Name to assign to new stanza group.</param>
        ''' <param name="aiGroupID">Array of DBIDs of <see cref="cEcoPathGroupInput">Ecopath group</see>
        ''' to assign to this mutli-stanza configuration.</param>
        ''' <param name="iGroupAges">Array of start ages to assign to these groups.</param>
        ''' <param name="iDBID">Database ID assigned to the new stanza group.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>The EwE core cannot handle a situation where a stanza configuration
        ''' is defined without having any groups. To avoid this situation, this method
        ''' requires valid <paramref name="aiGroupID">group IDs</paramref>.</remarks>
        ''' -------------------------------------------------------------------
        Function AppendStanza(ByVal strStanzaName As String, ByVal aiGroupID() As Integer, ByVal iGroupAges() As Integer, _
                ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a stanza group from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the stanza group to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveStanza(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a life stage to an existing stanza configuration.
        ''' </summary>
        ''' <param name="iStanzaDBID">Database ID of the stanza group to add the life stage to.</param>
        ''' <param name="iGroupDBID">Group to add as a life stage.</param>
        ''' <param name="iStartAge">Start age of this life stage.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddStanzaLifestage(ByVal iStanzaDBID As Integer, ByVal iGroupDBID As Integer,
                                    ByVal iStartAge As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a life stage from an existing stanza configuration.
        ''' </summary>
        ''' <param name="iStanzaDBID">Database ID of the stanza group to remove the life stage from.</param>
        ''' <param name="iGroupDBID">Group to remove as the life stage.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveStanzaLifestage(ByVal iStanzaDBID As Integer, ByVal iGroupDBID As Integer) As Boolean

#End Region ' Stanza

#Region " Pedigree "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a pedigree level to the datasource.
        ''' </summary>
        ''' <param name="iPosition">The position of the new pedigree level in
        ''' the level sequence.</param>
        ''' <param name="strName">Name to assign to new pedigree level.</param>
        ''' <param name="strDescription">Description to assign to new pedigree level.</param>
        ''' <param name="varName"><see cref="eVarNameFlags">Variable name</see> 
        ''' this pedigree level pertains to</param>
        ''' <param name="sIndexValue">Value [0, 1] indicating...</param>
        ''' <param name="sConfidence">Confidence interval for this pedigree level.</param>
        ''' <param name="iColor">Color (as integer) to use for the new pedigree level.</param>
        ''' <param name="iDBID">Database ID assigned to the new pedigree level.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddPedigreeLevel(ByVal iPosition As Integer, _
                                  ByVal strName As String, _
                                  ByVal iColor As Integer, _
                                  ByVal strDescription As String, _
                                  ByVal varName As eVarNameFlags, _
                                  ByVal sIndexValue As Single, _
                                  ByVal sConfidence As Single, _
                                  ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a pedigree level from the datasource.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the pedigree level to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemovePedigreeLevel(ByVal iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move a pedigree level to a different position in the level sequence.
        ''' </summary>
        ''' <param name="iDBID">Database ID of the pedigree level to move.</param>
        ''' <param name="iPosition">The new position of the pedigree level in the 
        ''' level sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function MovePedigreeLevel(ByVal iDBID As Integer, ByVal iPosition As Integer) As Boolean

#End Region ' Pedigree

#Region " Taxa "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a taxonomy definition to the datasource.
        ''' </summary>
        ''' <param name="iTargetDBID">DBIDs of the target to assign this taxon to.</param>
        ''' <param name="bIsStanza">Flag stating whether the <paramref name="iTargetDBID"/>
        ''' is a stanza (true) or a group (false).</param>
        ''' <param name="data">Data to populate taxonomy definition with. This data can be NULL.</param>
        ''' <param name="sProportion">Proportion this taxon contributes to the group.</param>
        ''' <param name="iDBID">Database ID assigned to the new taxon.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddTaxon(ByVal iTargetDBID As Integer, _
                          ByVal bIsStanza As Boolean, _
                          ByVal data As ITaxonSearchData, _
                          ByVal sProportion As Single, _
                          ByRef iDBID As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a taxonomy definition from the datasource.
        ''' </summary>
        ''' <param name="iTaxonID">Database ID of the taxon to remove.</param>
        ''' -------------------------------------------------------------------
        Function RemoveTaxon(ByVal iTaxonID As Integer) As Boolean

#End Region ' Taxa

    End Interface

End Namespace
