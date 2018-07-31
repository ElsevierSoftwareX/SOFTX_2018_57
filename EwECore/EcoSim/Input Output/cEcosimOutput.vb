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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

Public Class cEcosimOutput
    Inherits cCoreInputOutputBase

    Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)

        Me.AllowValidation = False

        Me.DBID = cCore.NULL_VALUE
        Me.m_dataType = eDataTypes.EcosimOutput
        Me.m_coreComponent = eCoreComponentType.EcoSim

    End Sub

    ''' <summary>
    ''' Get/set the fishing in-balance (FIB) index.
    ''' </summary>
    Public ReadOnly Property FIB(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.FIB, iTimeStep))
        End Get
    End Property

    Public ReadOnly Property FIBStatus(ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.FIB, iTimeStep)
        End Get
    End Property

    Public ReadOnly Property TLCatch(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TLCatch, iTimeStep))
        End Get
    End Property

    Public ReadOnly Property TLCatchStatus(ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.TLCatch, iTimeStep)
        End Get
    End Property

    Public ReadOnly Property TotalCatch(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.TotalCatch, iTimeStep))
        End Get
    End Property

    Public ReadOnly Property TotalCatchStatus(ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.TotalCatch, iTimeStep)
        End Get
    End Property

    Public ReadOnly Property KemptonsQ(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.KemptonsQ, iTimeStep))
        End Get
    End Property

    Public ReadOnly Property KemptonsQStatus(ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.KemptonsQ, iTimeStep)
        End Get
    End Property

    Public ReadOnly Property ShannonDiversity(ByVal iTimeStep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.ShannonDiversity, iTimeStep))
        End Get
    End Property

    Public ReadOnly Property ShannonDiversityStatus(ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.ShannonDiversity, iTimeStep)
        End Get
    End Property

    Public ReadOnly Property DiversityIndex(ByVal iTimeStep As Integer) As Single
        Get
            Select Case Me.m_core.m_EcoPathData.DiversityIndexType
                Case eDiversityIndexType.Shannon
                    Return Me.ShannonDiversity(iTimeStep)
                Case eDiversityIndexType.KemptonsQ
                    Return Me.KemptonsQ(iTimeStep)
                Case Else
                    Debug.Assert(False, "Diversity index type not supported")
            End Select
            Return cCore.NULL_VALUE
        End Get
    End Property

    Public ReadOnly Property DiversityIndexStatus(ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Select Case Me.m_core.m_EcoPathData.DiversityIndexType
                Case eDiversityIndexType.Shannon
                    Return Me.ShannonDiversityStatus(iTimeStep)
                Case eDiversityIndexType.KemptonsQ
                    Return Me.KemptonsQStatus(iTimeStep)
                Case Else
                    Debug.Assert(False, "Diversity index type not supported")
            End Select
            Return eStatusFlags.Null
        End Get
    End Property


    Public Overrides Function GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags,
                                          Optional ByVal iIndex As Integer = -9999,
                                          Optional ByVal iIndex2 As Integer = -9999,
                                          Optional ByVal iIndex3 As Integer = -9999) As Object

        Try

            Select Case VarName
                Case eVarNameFlags.FIB
                    Return Me.m_core.m_EcoSimData.FIB(iIndex)
                Case eVarNameFlags.TLCatch
                    Return Me.m_core.m_EcoSimData.TLC(iIndex)
                Case eVarNameFlags.KemptonsQ
                    Return Me.m_core.m_EcoSimData.Kemptons(iIndex)
                Case eVarNameFlags.ShannonDiversity
                    Return Me.m_core.m_EcoSimData.ShannonDiversity(iIndex)
                Case eVarNameFlags.TotalCatch
                    Return Me.m_core.m_EcoSimData.CatchSim(iIndex)

            End Select

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Return cCore.NULL_VALUE

    End Function

    Public Overrides Function GetStatus(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex As Integer = -9999, Optional ByVal iThirdIndex As Integer = -9999) As eStatusFlags
        Return eStatusFlags.NotEditable And eStatusFlags.ValueComputed
    End Function

End Class
