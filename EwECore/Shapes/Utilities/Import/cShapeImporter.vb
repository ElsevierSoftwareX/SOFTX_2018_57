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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Shapes.Utility

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Importer to create new shapes from text.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cShapeImporter

#Region " Private vars "

        Private m_core As cCore
        Private m_data As cShapeImportData

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(core As cCore, data As cShapeImportData)
            Me.m_core = core
            Me.m_data = data
        End Sub

#End Region ' Constructor 

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform the actual import.
        ''' </summary>
        ''' <param name="man"></param>
        ''' <param name="nPoints"></param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Import(man As cBaseShapeManager, nPoints As Integer) As Boolean

            Dim defs As cShapeImportData.cFunctionDefinition() = Me.m_data.FunctionDefinitions(man.DataType)
            Dim msgStatus As cMessage = Nothing
            Dim bSuccess As Boolean = True

            If (man IsNot Nothing) Then
                Try
                    Dim strMessage As String = cStringUtils.Localize(My.Resources.CoreMessages.SHAPE_IMPORT_SUCCESS, man.DataType)

                    msgStatus = New cMessage(strMessage, eMessageType.DataImport, eCoreComponentType.EcoSim, eMessageImportance.Information)
                    Me.m_core.SetBatchLock(cCore.eBatchLockType.Restructure)
                    For Each def As cShapeImportData.cFunctionDefinition In defs
                        Dim ff As cForcingFunction = man.CreateNewShape(def.Name, def.ShapeFunction.Shape(nPoints), def.ShapeFunction.ShapeFunctionType, def.ShapeParameters)
                        Dim vs As cVariableStatus = Nothing
                        If (ff IsNot Nothing) Then
                            vs = New cVariableStatus(eStatusFlags.OK, _
                                                     cStringUtils.Localize(My.Resources.CoreMessages.SHAPE_IMPORT_DETAIL, def.Name), _
                                                     eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
                        Else
                            bSuccess = False
                        End If
                        msgStatus.AddVariable(vs)
                    Next
                Catch ex As Exception

                End Try
                Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim, bSuccess)
            Else
                msgStatus = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.SHAPE_IMPORT_FAILED, man.DataType), _
                                         eMessageType.DataImport, eCoreComponentType.EcoSim, eMessageImportance.Warning)
                bSuccess = False
            End If

            Me.m_core.Messages.SendMessage(msgStatus)

            Return bSuccess

        End Function

#End Region ' Public access

    End Class

End Namespace
