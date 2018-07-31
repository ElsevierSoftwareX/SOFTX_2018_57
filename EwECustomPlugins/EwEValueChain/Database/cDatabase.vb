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
Imports EwEUtils.Database
Imports EwEUtils.Core
Imports System.IO
Imports System.Reflection
Imports EwECore

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' 
''' </summary>
''' ===========================================================================
Public Class cDatabase
    Inherits EwECore.Database.cEwEAccessDatabase

#Region " Load "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to update the database when opened
    ''' </summary>
    ''' <param name="strDatabase"></param>
    ''' <param name="databaseType">Type to use to open the database. Set this
    ''' to 'NotSet' to auto-detect the database type.</param>
    ''' <returns>True if connected succesfully.</returns>
    ''' -------------------------------------------------------------------
    Public Overrides Function Open(ByVal strDatabase As String, _
                                   Optional ByVal databaseType As eDataSourceTypes = eDataSourceTypes.NotSet, _
                                   Optional ByVal bReadOnly As Boolean = False) As eDatasourceAccessType

        Dim result As eDatasourceAccessType = MyBase.Open(strDatabase, databaseType, bReadOnly)
        If result = eDatasourceAccessType.Opened Then
            Me.OOPEnabled = True
            Me.UpdateDatabase()
        End If
        Return result

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function LoadModel(ByVal data As cData) As Boolean

        Dim aObjects As cOOPStorable() = Nothing
        Dim bSucces As Boolean = True

        Me.OOPFlushObjectCache()
        data.Clear()

        Try
            aObjects = Me.ReadObjects(GetType(cParameters))
        Catch ex As Exception
            bSucces = False
            cLog.Write(ex, "ValueChain::LoadModel - reading objects")
        End Try

        If (aObjects.Length = 0) Then
            data.AddParameters(New cParameters())
            ' If no parameters found there is little need to continue...
            Return True
        Else
            data.AddParameters(DirectCast(aObjects(0), cParameters))
        End If

        Try

            ' Load default units
            aObjects = Me.ReadObjects(GetType(cProducerUnitDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddUnitDefault(DirectCast(obj, cUnit)) : Next
            aObjects = Me.ReadObjects(GetType(cProcessingUnitDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddUnitDefault(DirectCast(obj, cUnit)) : Next
            aObjects = Me.ReadObjects(GetType(cDistributionUnitDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddUnitDefault(DirectCast(obj, cUnit)) : Next
            aObjects = Me.ReadObjects(GetType(cWholesalerUnitDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddUnitDefault(DirectCast(obj, cUnit)) : Next
            aObjects = Me.ReadObjects(GetType(cRetailerUnitDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddUnitDefault(DirectCast(obj, cUnit)) : Next
            aObjects = Me.ReadObjects(GetType(cConsumerUnitDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddUnitDefault(DirectCast(obj, cUnit)) : Next

            ' Load default links
            aObjects = Me.ReadObjects(GetType(cLinkDefault), False)
            For Each obj As cOOPStorable In aObjects : data.AddLinkDefault(DirectCast(obj, cLinkDefault)) : Next

            ' Load units
            aObjects = Me.ReadObjects(GetType(cProducerUnit), False)
            For Each obj As cOOPStorable In aObjects
                data.AddUnit(DirectCast(obj, cUnit))
            Next
            aObjects = Me.ReadObjects(GetType(cProcessingUnit), False)
            For Each obj As cOOPStorable In aObjects
                data.AddUnit(DirectCast(obj, cUnit))
            Next
            aObjects = Me.ReadObjects(GetType(cDistributionUnit), False)
            For Each obj As cOOPStorable In aObjects
                data.AddUnit(DirectCast(obj, cUnit))
            Next
            aObjects = Me.ReadObjects(GetType(cWholesalerUnit), False)
            For Each obj As cOOPStorable In aObjects
                data.AddUnit(DirectCast(obj, cUnit))
            Next
            aObjects = Me.ReadObjects(GetType(cRetailerUnit), False)
            For Each obj As cOOPStorable In aObjects
                data.AddUnit(DirectCast(obj, cUnit))
            Next
            aObjects = Me.ReadObjects(GetType(cConsumerUnit), False)
            For Each obj As cOOPStorable In aObjects
                data.AddUnit(DirectCast(obj, cUnit))
            Next

            ' Load links
            aObjects = Me.ReadObjects(GetType(cLink), False)
            For Each obj As cOOPStorable In aObjects
                ' Is old-fashioned producer link?
                Dim l As cLink = DirectCast(obj, cLink)
                Dim bError As Boolean = False
                If (l.Source.GetType Is GetType(cProducerUnit)) Then
                    ' #Yes: dump it
                    l.Source.RemoveLink(l)
                    Try
                        For iGroup As Integer = 1 To data.Core.nGroups
                            Dim ll As cLinkLandings = data.CreateLandingsLink(DirectCast(l.Source, cProducerUnit), l.Target, data.Core.EcoPathGroupInputs(iGroup), bError)
                            If (ll IsNot Nothing) Then
                                ll.BiomassRatio = l.BiomassRatio
                                ll.ValueRatio = l.ValueRatio
                                ll.ValuePerTon = l.ValuePerTon
                            End If
                        Next
                    Catch ex As Exception

                    End Try
                Else
                    ' #No: use it
                    data.AddLink(DirectCast(obj, cLink))
                End If
            Next
            aObjects = Me.ReadObjects(GetType(cLinkLandings), False)
            For Each obj As cOOPStorable In aObjects
                data.AddLink(DirectCast(obj, cLink))
            Next

            ' Load flow diagrams
            aObjects = Me.ReadObjects(GetType(cFlowDiagram), False)
            For Each obj As cOOPStorable In aObjects : data.CreateFlowDiagram(DirectCast(obj, cFlowDiagram)) : Next

            ' Load flow positions
            aObjects = Me.ReadObjects(GetType(cFlowPosition), False)
            For Each obj As cOOPStorable In aObjects
                data.AddFlowPosition(DirectCast(obj, cFlowPosition))
            Next

        Catch ex As Exception
            bSucces = False
            cLog.Write(ex, "ValueChain::LoadModel - loading individual units")
        End Try

        Return bSucces

    End Function

#End Region ' Load

#Region " Save "

    Public Function SaveModel(ByVal data As cData) As Boolean

        Dim bSucces As Boolean = True
        Dim ass As Assembly = Assembly.GetAssembly(GetType(cUnit))

        Me.OOPFlushObjectCache()
        Me.OOPFlushSchemaCache()

        ' JS 17Nov11: Use OOP transaction to minimize time on getting and releasing adapters
        If Me.OOPBeginTransaction(ass, True) Then

            Try
                ' Store model parameters
                bSucces = bSucces And Me.WriteObject(data.Parameters)

                ' Store default units
                bSucces = bSucces And Me.WriteObject(data.GetUnitDefault(cUnitFactory.eUnitType.Producer))
                bSucces = bSucces And Me.WriteObject(data.GetUnitDefault(cUnitFactory.eUnitType.Processing))
                bSucces = bSucces And Me.WriteObject(data.GetUnitDefault(cUnitFactory.eUnitType.Distribution))
                bSucces = bSucces And Me.WriteObject(data.GetUnitDefault(cUnitFactory.eUnitType.Wholesaler))
                bSucces = bSucces And Me.WriteObject(data.GetUnitDefault(cUnitFactory.eUnitType.Retailer))
                bSucces = bSucces And Me.WriteObject(data.GetUnitDefault(cUnitFactory.eUnitType.Consumer))

                ' Store units
                For i As Integer = 0 To data.UnitCount - 1
                    bSucces = bSucces And Me.WriteObject(data.Unit(i))
                Next

                ' Store flow diagrams
                For i As Integer = 0 To data.FlowDiagramCount - 1
                    bSucces = bSucces And Me.WriteObject(data.FlowDiagram(i))
                Next i

            Catch ex As Exception
                bSucces = False
            End Try

            Try

                ' Store default links
                bSucces = bSucces And Me.WriteObject(data.GetLinkDefault(cLinkFactory.eLinkType.ProducerToProcessing))
                bSucces = bSucces And Me.WriteObject(data.GetLinkDefault(cLinkFactory.eLinkType.ProcessingToDistribution))
                bSucces = bSucces And Me.WriteObject(data.GetLinkDefault(cLinkFactory.eLinkType.DistributionToWholeseller))
                bSucces = bSucces And Me.WriteObject(data.GetLinkDefault(cLinkFactory.eLinkType.WholesellerToRetailer))
                bSucces = bSucces And Me.WriteObject(data.GetLinkDefault(cLinkFactory.eLinkType.RetailerToConsumer))

                ' Store links
                For i As Integer = 0 To data.LinkCount - 1
                    bSucces = bSucces And Me.WriteObject(data.Link(i))
                Next

            Catch ex As Exception
                bSucces = False
            End Try

            ' Store flow positions
            For i As Integer = 0 To data.FlowPositionCount - 1
                bSucces = bSucces And Me.WriteObject(data.FlowPosition(i))
            Next

            If bSucces Then
                bSucces = Me.OOPCommitTransaction(True)
            Else
                Me.OOPRollbackTransaction()
            End If

        End If

        Return bSucces
    End Function

#End Region ' Save

#Region " Updates "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run consecutive updates to bring the database schema up to date.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function UpdateDatabase() As Boolean

        Dim sVersion As Single = Me.GetVersion()
        Dim bSucces As Boolean = True

        Me.BeginTransaction()

        If bSucces Then
            Me.CommitTransaction()
        Else
            Me.RollbackTransaction()
        End If

        Return bSucces
    End Function

#End Region ' Updates

End Class
